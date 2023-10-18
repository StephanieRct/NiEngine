//#define NIEDITOR_PRINT
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using NiEngine;
using System.Runtime.InteropServices;
using static Codice.CM.WorkspaceServer.WorkspaceTreeDataStore;

namespace NiEditor
{
    public struct PropertyInfo
    {
        public FieldInfo FieldInfo;
        public object Value;

        public EditorField EditorField;

        IEnumerable<object> Attributes => FieldInfo.GetCustomAttributes(inherit: false);

        public PropertyInfo(FieldInfo fieldInfo, object value, EditorField editorField)
        {
            FieldInfo = fieldInfo;
            Value = value;
            EditorField = editorField;
        }

    }
    public static class SerializedPropertyExt
    {
        public static string GetDebugName(this SerializedProperty property) => $"[{property.serializedObject?.GetType().FullName}]:{property.propertyPath}";
        public static PropertyInfo GetPropertyInfo(this SerializedProperty property)
        {
            var path = property.propertyPath.Replace(".Array.data[", "[");
            object obj = property.serializedObject.targetObject;
            FieldInfo fi = default;
            var elements = path.Split('.');
            foreach (var element in elements)
            {
                if (element.Contains("["))
                {
                    var elementName = element.Substring(0, element.IndexOf("["));
                    var index = Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
                    (fi, obj) = GetField(obj, elementName, index);
                }
                else
                {
                    (fi, obj) = GetField(obj, element);
                }
            }

            EditorField editorField = fi?.GetCustomAttributes(inherit: false).OfType<EditorField>().FirstOrDefault();
            if (editorField == null && property.isArray)
                editorField = new EditorField(showPrefixLabel: false, inline: false);
            var propertyInfo = new PropertyInfo(fi, obj, editorField);
            //if (propertyInfo.EditorField == null && property.propertyType == SerializedPropertyType.ManagedReference)
            //{
            //    var objValue = property.managedReferenceValue;
            //    if (objValue is not null)
            //    {
            //        var actualType = property.managedReferenceValue.GetType();
            //        foreach (var m in actualType.GetMember(property.name))
            //        {
            //            foreach (var attribute in m.GetCustomAttributes(inherit: false))
            //                if (attribute is EditorField editorField)
            //                {
            //                    propertyInfo.EditorField = editorField;
            //                    propertyInfo.Value = objValue;
            //                    return propertyInfo;
            //                }
            //        }
            //    }
            //}
            return propertyInfo;
        }
        
        public static (FieldInfo, object) FindFieldInfo(this SerializedProperty prop)
        {
            var path = prop.propertyPath.Replace(".Array.data[", "[");
            object obj = prop.serializedObject.targetObject;
            FieldInfo fi = default;
            var elements = path.Split('.');
            foreach (var element in elements)
            {
                if (element.Contains("["))
                {
                    var elementName = element.Substring(0, element.IndexOf("["));
                    var index = Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
                    (fi, obj) = GetField(obj, elementName, index);
                }
                else
                {
                    (fi, obj) = GetField(obj, element);
                    
                }
            }
            return (fi, obj);
        }
        public static (FieldInfo, object) GetField(object source, string name, int index)
        {
            var fi = GetFieldInfo(source, name);
            var enumerable = fi.GetValue(source) as IEnumerable;
            var enm = enumerable.GetEnumerator();
            while (index-- >= 0)
                enm.MoveNext();
            return (fi, enm.Current);
        }
        public static (FieldInfo, object) GetField(object source, string name)
        {
            if (source == null)
                return (null, null);
            var type = source.GetType();
            var f = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (f == null)
            {
                Debug.LogError($"Could not find field '{name}' on object of type '{type.FullName}'");
                return (f, null);
            }

            return (f, f.GetValue(source));
        }
        public static FieldInfo GetFieldInfo(object source, string name)
        {
            if (source == null)
                return null;
            var type = source.GetType();
            var f = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            return f;
        }

        public static object GetFieldValue(object source, string name) => GetFieldInfo(source, name).GetValue(source);

        public static object GetFieldValue(object source, string name, int index)
        {
            var enumerable = GetFieldValue(source, name) as IEnumerable;
            var enm = enumerable.GetEnumerator();
            while (index-- >= 0)
                enm.MoveNext();
            return enm.Current;
        }
        
        public static Type GetManagedReferenceFieldType(this SerializedProperty property)
        {
            if (string.IsNullOrEmpty(property.managedReferenceFieldTypename))
                return property.GetFieldType();
            var atNames = property.managedReferenceFieldTypename.Split(' ');
            foreach (var a in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                var name = a.GetName();
                if (name.Name == atNames[0])
                {
                    return a.GetType(atNames[1]);
                }
            }
            Debug.LogError($"Could not find type of managed reference on property {property.GetDebugName()}");
            return default;
        }

        public static Type GetFieldType(this SerializedProperty property)
        {
            var (fi, obj) = FindFieldInfo(property);

            return fi.FieldType;
        }
    }
    public class EditorFieldVe : VisualElement
    {

        public SerializedProperty Property;
        EditorField m_EditorField;
        PropertyInfo m_PropertyInfo;
        public PropertyInfo PropertyInfo
        {
            get
            {
                if (m_PropertyInfoState == PropertyInfoStateEnum.Dirty)
                {
                    m_PropertyInfo = Property.GetPropertyInfo();
                    m_PropertyInfoState = PropertyInfoStateEnum.Set;
                }
                return m_PropertyInfo;
            }
            set
            {
                m_PropertyInfo = value;
                m_PropertyInfoState = PropertyInfoStateEnum.Forced;
            }
        }
        enum PropertyInfoStateEnum
        {
            Forced,
            Dirty,
            Set
        }
        PropertyInfoStateEnum m_PropertyInfoState = PropertyInfoStateEnum.Dirty;
        public EditorField EditorField => m_EditorField ?? PropertyInfo.EditorField;

        public EditorFieldVe()
        {
        }
        public EditorFieldVe(PropertyInfo propertyInfo)
        {
            m_PropertyInfo = propertyInfo;
            m_PropertyInfoState =  PropertyInfoStateEnum.Forced;

            //name = $"{GetType().FullName}({DebugDetails})";
        }
        public EditorFieldVe(EditorField editorField)
        {
            m_EditorField = editorField;
            m_PropertyInfoState = m_EditorField is null ? PropertyInfoStateEnum.Dirty : PropertyInfoStateEnum.Forced;
            
            //name = $"{GetType().FullName}({DebugDetails})";
        }

        public string DebugDetails =>
            $"ShowPrefixLabel:{(EditorField?.ShowPrefixLabel.ToString() ?? "n/a")}, Inline:{(EditorField?.Inline.ToString() ?? "n/a")} prop:{Property?.propertyPath}";
        public bool BindProperty(SerializedProperty property)
        {
            if (Property == property) return false;
            Property = property;

            name = $"{GetType().FullName}({DebugDetails})";

            if (m_PropertyInfoState == PropertyInfoStateEnum.Set)
                m_PropertyInfoState = PropertyInfoStateEnum.Dirty;

            return true;
        }
    }

    public interface IRegisterValueChangeCallback
    {
        void RegisterValueChangeCallback(EventCallback<SerializedPropertyChangeEvent> ecb);
        void UnregisterValueChangeCallback(EventCallback<SerializedPropertyChangeEvent> ecb);
    }
    public class NiPropertyField : EditorFieldVe, IRegisterValueChangeCallback
    {
        [System.Diagnostics.Conditional("NIEDITOR_PRINT")]
        void Log(string msg)
        {
            Debug.Log(msg);
        }
        public static Dictionary<SerializedProperty, VisualElement> Cache = null;// new();
        //private PropertyField Field = new();
        //PropertyDrawer
        string m_Label;
        //string label
        VisualElement m_Content;
        //PropertyField m_PropertyField;
        EventCallback<SerializedPropertyChangeEvent> m_OnChangeCallback;

        public NiPropertyField()
        {
        }
        public NiPropertyField(EditorField editorField)
            :base(editorField)
        {
        }
        public NiPropertyField(string label)
        {
            m_Label = label;
            //Add(Field);
        }
        public NiPropertyField(PropertyInfo propertyInfo)
            : base(propertyInfo)
        {
        }


        void UpdateChangeCallback()
        {
            switch (m_Content)
            {
                case PropertyField pf:
                    if (m_OnChangeCallback != null)
                        pf.RegisterValueChangeCallback(OnPropertyChange);
                    else
                        pf.UnregisterCallback(m_OnChangeCallback);
                    break;
                case IRegisterValueChangeCallback r:
                    if (m_OnChangeCallback != null)
                        r.RegisterValueChangeCallback(m_OnChangeCallback);
                    else
                        r.UnregisterValueChangeCallback(m_OnChangeCallback);
                    break;
            }
        }
        public void RegisterValueChangeCallback(EventCallback<SerializedPropertyChangeEvent> ecb)
        {
            m_OnChangeCallback += ecb;
            UpdateChangeCallback();
        }

        public void UnregisterValueChangeCallback(EventCallback<SerializedPropertyChangeEvent> ecb)
        {
            m_OnChangeCallback -= ecb;
            UpdateChangeCallback();
        }

        void OnPropertyChange(SerializedPropertyChangeEvent e) => m_OnChangeCallback?.Invoke(SerializedPropertyChangeEvent.GetPooled(e.changedProperty));
        //void OnPropertyChange(SerializedProperty property) => m_OnChangeCallback?.Invoke(SerializedPropertyChangeEvent.GetPooled(property));
        //public NiPropertyField(SerializedProperty property)
        //{
        //    //Field = new(property);
        //    name = "NiPropertyField";
        //    m_Label = "";
        //    //Add(Field);
        //}
        //public NiPropertyField(SerializedProperty property, string label)
        //{
        //    //Field = new(property);
        //    name = "NiPropertyField";
        //    m_Label = label;
        //    //Add(Field);
        //}
        void BindPropertyField(SerializedProperty property)
        {
            var f = new PropertyField(property);
            f.BindProperty(property);
            f.style.paddingRight = 0;

            if (EditorField?.Inline ?? false)
            {
                f.label = "";
                var lf = new VisualElement();
                f.style.flexShrink = 0;
                //f.style.flexGrow = 1;
                var l = new Label(GetLabel(property));
                l.style.paddingRight = 0;
                l.style.paddingTop = 3;
                lf.Add(l);
                lf.Add(f);
                lf.style.flexDirection = FlexDirection.Row;
                m_Content = lf;
                Add(lf);
            }
            else
            {
                f.label = GetLabel(property);
                m_Content = f;
                Add(m_Content);
            }

            UpdateChangeCallback();
        }
        void BindDrawer(SerializedProperty property, Type baseType)
        {

            var (fieldInfo, value) = property.FindFieldInfo();

            var drawersBaseType = GetDrawers(baseType);
            foreach (var d in drawersBaseType.Drawers.Values)
            {
                var propertyDrawerType = d.PropertyDrawer.GetType();
                propertyDrawerType.GetField("m_Attribute", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(d.PropertyDrawer, null);
                propertyDrawerType.GetField("m_FieldInfo", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(d.PropertyDrawer, fieldInfo);
                Log($"Drawer '{d.PropertyDrawer.GetType().FullName}' found for type '{baseType.FullName}' on property '{property.GetDebugName()}'");
                m_Content = d.PropertyDrawer.CreatePropertyGUI(property);
                UpdateChangeCallback();
                Add(m_Content);
                return;
            }

            // check for a drawer associated with any of the field attributes
            foreach (var a in fieldInfo.GetCustomAttributes(inherit: false))
                if (a is PropertyAttribute pa)
                {
                    var ds = GetDrawers(a.GetType());
                    foreach (var d in ds.Drawers.Values)
                    {
                        var propertyDrawerType = d.PropertyDrawer.GetType();
                        propertyDrawerType.GetField("m_Attribute", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(d.PropertyDrawer, pa);
                        propertyDrawerType.GetField("m_FieldInfo", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(d.PropertyDrawer, fieldInfo);
                        Log($"Drawer '{d.PropertyDrawer.GetType().FullName}' found for type '{baseType.FullName}' on property '{property.GetDebugName()}' via attribute '{pa.GetType().FullName}'");
                        m_Content = d.PropertyDrawer.CreatePropertyGUI(property);
                        UpdateChangeCallback();
                        Add(m_Content);
                        return;
                    }
                }

            Log($"No drawer found for type '{baseType.FullName}' on property '{property.GetDebugName()}'");
            m_Content = null;
        }
        void BindChildren(SerializedProperty property, Action<PropertyInfo, VisualElement> add)
        {
            var itor = property.Copy();
            bool first = true;
            var depth = property.depth;
            while (itor.NextVisible(first) && itor.depth > depth)
            {
                first = false;
                var propInfo = itor.GetPropertyInfo();
                var hide = propInfo.FieldInfo.GetCustomAttribute<HideInInspector>(inherit: true);
                if (hide == null)
                {
                    var isRuntimeOnly = propInfo.EditorField?.RuntimeOnly ?? false;
                    if (EditorMenu.ShowRuntimeFields || !isRuntimeOnly || (isRuntimeOnly && EditorApplication.isPlaying))
                    {
                        var field = new NiPropertyField(propInfo);
                        field.BindProperty(itor.Copy());
                        add(propInfo, field);
                    }
                }
            }
        }
        void BindFoldout(SerializedProperty property)
        {
            var isFoldoutRuntimeOnly = EditorField?.RuntimeOnly ?? false;
            if (!EditorMenu.ShowRuntimeFields && isFoldoutRuntimeOnly && !EditorApplication.isPlaying)
                return;
            if(EditorField?.Unfold ?? false)
            {
                m_Content = new VisualElement();

                BindChildren(property, (propInfo, field) =>
                {
                    m_Content.Add(field);
                });
                return;

            }

            var rootInline = EditorField?.Inline ?? false;
            var foldout = new InlineFoldout(showToggle: !rootInline, updateLayout: false);
            foldout.Value = property.isExpanded;
            foldout.OnToggle += value => property.isExpanded = value;

            if (EditorField?.ShowPrefixLabel ?? true)
            {
                foldout.VeFace = new Label(GetLabel(property));
            }
            BindChildren(property, (propInfo, field) =>
            {
                if (propInfo.EditorField?.Inline ?? false)
                    foldout.GetInlineElement(updateLayout: false).Add(field);
                else
                    foldout.GetContentElement(updateLayout: false).Add(field);
            });
            foldout.UpdateLayout();
            m_Content = foldout;
        }
        string GetLabel(SerializedProperty property)
        {
            return EditorField?.ShowPrefixLabel ?? true ? 
                (m_Label ?? EditorField?.Prefix ?? property.displayName)
                : "";

        }
        public new bool BindProperty(SerializedProperty property)
        {
            if (!base.BindProperty(property)) return false;
            
            this.RemoveAll();
            //if (Cache == null || !Cache.TryGetValue(property, out var veRoot))
            {
                
                switch (property.propertyType)
                {
                    case SerializedPropertyType.ObjectReference:
                        Log($"[NiFP:ObjectReference] ObjectRefType:{property.objectReferenceValue?.GetType().FullName}\nProp: '{property.propertyPath}'");
                        BindPropertyField(property);
                        break;
                    case SerializedPropertyType.ManagedReference:
                        Log($"[NiFP:ManagedReference] type:{property.type}\nProp: '{property.propertyPath}'");
                        BindDrawer(property, property.GetManagedReferenceFieldType());
                        if(m_Content == null)
                            BindPropertyField(property);
                        break;
                    case SerializedPropertyType.Generic:
                        if (property.isArray)
                        {
                            Log($"[NiFP:Generic:Array:{property.propertyType}] type:{property.type}\nProp: '{property.propertyPath}'");
                            var lv = new ListView2(EditorField);
                            lv.ShowIcon(false);
                            lv.SetText(GetLabel(property));
                            
                            lv.BindProperty(property);
                            m_Content = lv;
                        }
                        else
                        {
                            Log($"[NiFP:Generic] type:{property.type}\nProp: '{property.propertyPath}'");
                            var fieldType = property.GetFieldType();
                            if (fieldType == typeof(UnityEngine.Events.UnityEvent)
                                || (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(UnityEngine.Events.UnityEvent<>))
                                )
                                BindPropertyField(property);
                            else
                            {
                                BindDrawer(property, fieldType);
                                if (m_Content == null)
                                    BindFoldout(property);
                            }
                        }
                        break;
                    default:
                        Log($"[NiFP:default:{property.propertyType}] type:{property.type}\nProp: '{property.propertyPath}'");
                        BindPropertyField(property);
                        break;

                }
                //Cache?.Add(property, veRoot);
            }
            if (m_Content != null)
            {

                //m_Content.style.flexDirection = FlexDirection.Row;
                if (EditorField != null)
                {
                    if (EditorField.MinWidth >= 0)
                        m_Content.style.minWidth = EditorField.MinWidth;
                    if (EditorField.MaxWidth >= 0)
                        m_Content.style.maxWidth = EditorField.MaxWidth;

                    m_Content.style.flexGrow = EditorField.FlexGrow;
                }

                Add(m_Content);
            }
            return true;
            //property.type
            //Field.BindProperty(property);
        }

        public class DrawerReference
        {
            public Type BaseType;
            public PropertyDrawer PropertyDrawer;
            public CustomPropertyDrawer Attribute;
        }

        public class DrawerSet
        {
            public SortedDictionary<int, DrawerReference> Drawers = new();
            //public List<DrawerReference> Drawers = new();

        }
        public static Dictionary<Type, DrawerSet> DrawerSetByType;
        public static Dictionary<string, DrawerSet> DrawerSetByTypeName;
        public static List<(Type, DrawerReference)> Drawers;

        static void OnBeforeAssemblyReload()
        {
            DrawerSetByType = null;
            DrawerSetByTypeName = null;
            Drawers = null;
        }
        public static void FindAllDrawers()
        {
            if (DrawerSetByType != null)
                return;

            AssemblyReloadEvents.beforeAssemblyReload -= OnBeforeAssemblyReload;
            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
            DrawerSetByType = new();
            DrawerSetByTypeName = new();
            Drawers = new();
            var baseType = typeof(PropertyDrawer);
            foreach (var type in System.AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes().Where(t => t.IsClass && !t.IsAbstract && baseType.IsAssignableFrom(t))))
            {
                foreach (var a in type.GetCustomAttributes(inherit: false))
                    if (a is CustomPropertyDrawer ad)
                    {
                        var t = (Type)ad.GetType().GetField("m_Type", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(ad);
                        Drawers.Add((t, new DrawerReference
                        {
                            BaseType = t,
                            PropertyDrawer = (PropertyDrawer)type.GetConstructor(Type.EmptyTypes).Invoke(null),
                            Attribute = ad
                        }));
                    }
            }
        }

        public static int GetAssignmentDepth(Type rootType, Type leafType)
        {
            int count = 0;
            var curType = leafType;
            int maxIterator = 1000;
            while (curType is not null && curType != rootType && maxIterator > 0)
            {
                ++count;
                if (curType == curType.BaseType) return -1;
                curType = curType.BaseType;
                --maxIterator;
            }

            if (maxIterator <= 0)
            {
                Debug.LogError($"GetAssignmentDepth infinite loop rootType:{rootType.FullName}, leafType:{leafType.FullName}");
            }
            if (curType is null) return -1;
            return count;
        }
        public static DrawerSet GetDrawers(Type baseType)
        {
            FindAllDrawers();
            if (!DrawerSetByType.TryGetValue(baseType, out var choices))
            {
                choices = new DrawerSet();
                
                foreach (var (type, drawer) in Drawers)
                {
                    var d = GetAssignmentDepth(baseType, type);
                    if (d >= 0)
                        choices.Drawers.Add(d, drawer);
                    else if (baseType.IsAssignableFrom(type))
                        choices.Drawers.Add(int.MaxValue, drawer);
                }
                DrawerSetByType.Add(baseType, choices);
            }

            return choices;
        }

    }
}