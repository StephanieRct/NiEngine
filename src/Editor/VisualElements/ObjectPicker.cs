//#define NIEDITOR_PRINT
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NiEngine;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Text.RegularExpressions;
using Unity.Properties;
using UnityEditor.Graphs;

namespace NiEditor
{
    public class ObjectPicker : InlineFoldout, IRegisterValueChangeCallback
    {
        static ICloneable s_CopiedObject;
        public FieldInfo FieldInfo;
        public Type ObjectBaseType;
        public ObjectReferencePicker Picker;
        public EditorField EditorField;

        public SerializedProperty Property;

        //public VisualElement VeInline;
        //public VisualElement VeContent;
        //public Toggle Toggle;
        public DropdownField DropdownField;
        public ChoiceLists Choices;
        EventCallback<SerializedPropertyChangeEvent> m_OnChangeCallback;

        string GetLabel(SerializedProperty property)
        {
            return EditorField?.ShowPrefixLabel ?? true ?
                (EditorField?.Prefix ?? property.displayName)
                : "";

        }
        public ObjectPicker(FieldInfo fieldInfo, ObjectReferencePicker picker, SerializedProperty property)
            : base(showToggle: false, updateLayout: false)
        {
            Log($"ObjectPicker property:{property.propertyPath} ctor");
            FieldInfo = fieldInfo;
            Picker = picker;
            ObjectBaseType = Picker?.BaseType ?? FieldInfo.FieldType;
            Property = property;
            tooltip = Property.tooltip;
            EditorField = fieldInfo.GetCustomAttributes().OfType<EditorField>().FirstOrDefault();
            if (EditorField is null)
                EditorField = Property.GetPropertyInfo().EditorField;

            
            DropdownField = new DropdownField();
            DropdownField.style.marginLeft = 0;
            if (EditorField?.ShowPrefixLabel ?? true)
            {
                VeLabel = new Label(GetLabel(property));
                VeLabel.style.paddingTop = 3;
                this.VeFace = DropdownField;
                //if (EditorField?.PrefixAligned ?? true)
                //{
                //    DropdownField.label = property.displayName;
                //    this.VeFace = DropdownField;
                //}
                //else
                //{
                //    GetPrefixElement().Add(new Label(property.displayName));
                //    //var LabelDropdownField = new VisualElement();
                //    //LabelDropdownField.style.flexDirection = FlexDirection.Row;
                //    //LabelDropdownField.Add(new Label(property.displayName));
                //    //LabelDropdownField.Add(DropdownField);
                //    //this.VeFace = LabelDropdownField;
                //}
            }
            else
            {
                this.VeFace = DropdownField;
            }

            //if (EditorField != null)
            //{
            //    if(EditorField.MinWidth >= 0)
            //        style.minWidth = EditorField.MinWidth;
            //    if (EditorField.MaxWidth >= 0)
            //        style.maxWidth = EditorField.MaxWidth;
            //    style.flexGrow = EditorField.GrowWidth ? 1 : 0;
            //}
            if(Property.managedReferenceValue is UidObject uidObject)
            {
                if (uidObject.Uid.IsDefault)
                {
                    Debug.Log($"Fixed a null Uid, asset will require to be saved again");
                    uidObject.Uid = Uid.NewUid();
                    Property.serializedObject.ApplyModifiedProperties();
                }
            }
            
            this.OnToggle += toggled =>
            {
                Property.isExpanded = toggled;
                Property.serializedObject.ApplyModifiedProperties();
            };

            Choices = GetChoices(ObjectBaseType);
            if (Property.managedReferenceValue == null)
            {
                Property.managedReferenceValue = null;
                Property.serializedObject.ApplyModifiedProperties();
            }

            var type = Property.managedReferenceValue?.GetType();
            DropdownField.choices = Choices.Names;
            DropdownField.index = Choices.Types.IndexOf(type);
            DropdownField.RegisterValueChangedCallback(x =>
            {
                Log($"ObjectPicker property:{property.propertyPath} DropdownField");
                int index = DropdownField.index;
                var t = Choices.Types[index];
                if (t is null)
                {
                    Property.managedReferenceValue = null;
                    Property.isExpanded = false;
                }
                else
                {
                    Property.managedReferenceValue = t.GetConstructor(Type.EmptyTypes).Invoke(null);
                    Property.isExpanded = true;
                }

                Property.serializedObject.ApplyModifiedProperties();
                //this.ReBuildContent();
                UpdateAllContent();
                SetContentVisible(Property.isExpanded, false);
                //TgFold?.SetValueWithoutNotify(Property.isExpanded);
            });

            DropdownField.AddManipulator(new ContextualMenuManipulator((ContextualMenuPopulateEvent evt) =>
            {
                evt.menu.AppendAction("Copy", (x) => 
                { 
                    if(Property.managedReferenceValue is ICloneable cloneable)
                        s_CopiedObject = (ICloneable)cloneable.Clone(); 
                });

                var pastStatus = s_CopiedObject != null 
                    && ObjectBaseType.IsAssignableFrom(s_CopiedObject.GetType())
                    ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled;

                //evt.menu.AppendAction("Paste/Before", (x) =>
                //{
                //    Property.managedReferenceValue = s_CopiedObject.Clone();
                //    Property.serializedObject.ApplyModifiedProperties();
                //    Property.serializedObject.Update();
                //    UpdateAllContent();
                //}, pastStatus);
                evt.menu.AppendAction("Paste", (x) =>
                {
                    var clone = s_CopiedObject.Clone();
                    if (clone != null)
                    {
                        Property.managedReferenceValue = clone;
                        //DropdownField.index = Choices.Types.IndexOf(Property.managedReferenceValue.GetType());
                        DropdownField.SetValueWithoutNotify(Choices.Names[Choices.Types.IndexOf(Property.managedReferenceValue.GetType())]);
                        Property.serializedObject.ApplyModifiedProperties();
                        Property.serializedObject.Update();
                        UpdateAllContent();
                    }
                }, pastStatus);
                evt.menu.AppendAction("Print Class", (x) =>
                {
                    Debug.Log($"Class: {Property.managedReferenceValue?.GetType().FullName}");
                });
                //evt.menu.AppendAction("Paste/After", (x) =>
                //{
                //    Property.managedReferenceValue = s_CopiedObject.Clone();
                //    Property.serializedObject.ApplyModifiedProperties();
                //    Property.serializedObject.Update();
                //    UpdateAllContent();
                //}, pastStatus);
            }));


            //if (Property.isExpanded)
            //    UpdateAllContent();
            //else
            //{
            //    //MakeContent = BuildContent;
            //    BuildInline();
            //}

            MakeContent = BuildContent;
            BuildInline();
            Value = Property.isExpanded;
        }

        protected override string GenerateDebugName()
            =>
                $"ObjectPicker(showPrefix:{EditorField?.ShowPrefixLabel.ToString() ?? ""}, inline:{EditorField?.Inline.ToString() ?? ""}) {base.GenerateDebugName()}";

        VisualElement BuildContent()
        {
            var root = new VisualElement();

            var itor = Property.Copy();
            bool first = true;
            var depth = Property.depth;
            List<VisualElement> listEnd = null;
            while (itor.NextVisible(first) && itor.depth > depth)
            {
                first = false;
                var info = itor.GetPropertyInfo();
                var hide = info.FieldInfo.GetCustomAttribute<HideInInspector>(inherit: true);
                if (hide == null)
                {
                    var isRuntimeOnly = info.EditorField?.RuntimeOnly ?? false;
                    if (EditorMenu.ShowRuntimeFields || !isRuntimeOnly || (isRuntimeOnly && EditorApplication.isPlaying))
                    {
                        var field = new NiPropertyField(info);
                        field.BindProperty(itor.Copy());
                        field.RegisterValueChangeCallback(e => m_OnChangeCallback?.Invoke(e));
                        if (!info.EditorField?.Inline ?? true)
                        {
                            if (info.EditorField?.AddToEnd ?? false)
                            {
                                if (listEnd == null)
                                    listEnd = new();
                                listEnd.Add(field);
                            }
                            else
                                root.Add(field);
                        }
                    }
                }
            }

            if (listEnd != null)
                foreach (var e in listEnd)
                    root.Add(e);

            return root;
        }
        void BuildInline()
        {
            Log($"ObjectPicker property:{Property.propertyPath} UpdateInline");
            if (DropdownField.index >= 0)
            {
                bool needExpand = false;
                var itor = Property.Copy();
                bool first = true;
                var depth = Property.depth;
                List<VisualElement> listPrefixEnd = null;
                List<VisualElement> listEnd = null;
                while (itor.NextVisible(first) && itor.depth > depth)
                {
                    first = false;
                    var info = itor.GetPropertyInfo();
                    var hide = info.FieldInfo.GetCustomAttribute<HideInInspector>(inherit: true);
                    if (hide == null)
                    {
                        var isRuntimeOnly = info.EditorField?.RuntimeOnly ?? false;
                        if (EditorMenu.ShowRuntimeFields || !isRuntimeOnly || (isRuntimeOnly && EditorApplication.isPlaying))
                        {
                            if (info.EditorField?.IsPrefix ?? false)
                            {
                                var field = new NiPropertyField(info);
                                //field.style.flexGrow = 1;
                                field.BindProperty(itor.Copy());
                                field.RegisterValueChangeCallback(e => m_OnChangeCallback?.Invoke(e));
                                GetPrefixElement(updateLayout: false).Add(field);
                                if (!string.IsNullOrEmpty(info.EditorField?.Suffix ?? null))
                                {
                                    var l = new Label(info.EditorField.Suffix);
                                    l.style.paddingTop = 3;
                                    l.style.paddingRight = 0;
                                    if (info.EditorField?.AddToEnd ?? false)
                                    {
                                        if (listPrefixEnd == null)
                                            listPrefixEnd = new();
                                        listPrefixEnd.Add(l);
                                    }
                                    else
                                        GetPrefixElement(updateLayout: false).Add(l);
                                }
                            }
                            else if (info.EditorField?.Inline ?? false)
                            {
                                var field = new NiPropertyField(info);
                                //field.style.flexGrow = 1;
                                field.BindProperty(itor.Copy());
                                field.RegisterValueChangeCallback(e => m_OnChangeCallback?.Invoke(e));
                                GetInlineElement(updateLayout: false).Add(field);
                                if (!string.IsNullOrEmpty(info.EditorField?.Suffix ?? null))
                                {
                                    var l = new Label(info.EditorField.Suffix);
                                    l.style.paddingTop = 3;
                                    l.style.paddingRight = 0;
                                    if (info.EditorField?.AddToEnd ?? false)
                                    {
                                        if (listEnd == null)
                                            listEnd = new();
                                        listEnd.Add(l);
                                    }
                                    else
                                        GetInlineElement(updateLayout: false).Add(l);
                                }
                            }
                            else
                                needExpand = true;
                            Log($"ObjectPicker UpdateInline base property:'{Property.propertyPath}', children:'{itor.name}', inline:{info.EditorField?.Inline ?? false}, needExpand:{needExpand}");
                        }
                    }
                }
                if (listPrefixEnd != null)
                {
                    var r = GetPrefixElement();
                    foreach (var e in listPrefixEnd)
                        r.Add(e);

                }
                if (listEnd != null)
                {
                    var r = GetInlineElement();
                    foreach (var e in listEnd)
                        r.Add(e);

                }
                ShowToggle(needExpand, updateLayout: false);
                UpdateLayout();
            }
            else
            {
                ShowToggle(false, updateLayout: false);
                RemovePrefixElement(updateLayout: false);
                RemoveInlineElement(updateLayout: false);
                RemoveContentElement(updateLayout: false);
                UpdateLayout();
            }
        }
        
        void UpdateAllContent()
        {
            Log($"ObjectPicker property:{Property.propertyPath} UpdateAllContent");
            ClearLayout();
            if (DropdownField.index >= 0)
            {
                if (Property.isExpanded)
                    this.ReBuildContent();
                BuildInline();

                //bool needExpand = false;
                //var itor = Property.Copy();
                //bool first = true;
                //var depth = Property.depth;
                //while (itor.NextVisible(first) && itor.depth > depth)
                //{
                //    first = false;
                //    var info = itor.GetPropertyInfo();
                //    var field = new NiPropertyField();
                //    field.BindProperty(itor.Copy());
                //    field.RegisterValueChangeCallback(e => m_OnChangeCallback?.Invoke(e));
                //    if (info.EditorField?.Inline ?? false)
                //        GetInlineElement(updateLayout: false).Add(field);
                //    else
                //    {
                //        //GetContentElement(updateLayout: false).Add(field);
                //        needExpand = true;
                //    }
                //}

                //ShowToggle(needExpand, updateLayout: false);
                //UpdateLayout();
            }
            else
            {
                ShowToggle(false, updateLayout: false);
                RemovePrefixElement(updateLayout: false);
                RemoveInlineElement(updateLayout: false);
                RemoveContentElement(updateLayout: false);
                UpdateLayout();
            }
        }

        public void RegisterValueChangeCallback(EventCallback<SerializedPropertyChangeEvent> ecb)
        {
            m_OnChangeCallback += ecb;
        }

        public void UnregisterValueChangeCallback(EventCallback<SerializedPropertyChangeEvent> ecb)
        {
            m_OnChangeCallback -= ecb;
        }

        public static VisualTreeAsset _ObjectPickerAsset;
        public static VisualTreeAsset ObjectPickerAsset => _ObjectPickerAsset ??= AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/NiEngine/src/Editor/Assets/ObjectPicker.uxml");

        [System.Diagnostics.Conditional("NIEDITOR_PRINT")]
        static void Log(string msg)
        {
            Debug.Log(msg);
        }
        static void OnBeforeAssemblyReload()
        {
            ChoicesByType = null;
        }


        public class ChoiceLists
        {
            public List<Type> Types = new();
            public List<string> Names = new();
            public List<ClassPickerName> Pickers = new();

            public bool IsInline(int index) => Pickers[index]?.Inline ?? false;
            public bool ShowPrefixLabel(int index) => Pickers[index]?.ShowPrefixLabel ?? false;
        }

        public static Dictionary<Type, ChoiceLists> ChoicesByType;

        public static ChoiceLists GetChoices(Type baseType)
        {
            if (ChoicesByType == null)
            {
                AssemblyReloadEvents.beforeAssemblyReload -= OnBeforeAssemblyReload;
                AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
                ChoicesByType = new();
            }

            if (!ChoicesByType.TryGetValue(baseType, out var choices))
            {
                choices = new();
                choices.Types.Add(null);
                choices.Names.Add("Null");
                choices.Pickers.Add(null);
                foreach (var type in System.AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes().Where(t => t.IsClass && !t.IsAbstract && !t.IsGenericType && baseType.IsAssignableFrom(t))))
                {
                    string name = type.FullName;
                    ClassPickerName picker = null;
                    foreach (var attribute in type.GetCustomAttributes(inherit: false))
                        if (attribute is ClassPickerName classPickerName)
                        {
                            name = classPickerName.Name;
                            picker = classPickerName;
                            break;
                        }

                    var existingIndex = choices.Names.IndexOf(name);
                    if (existingIndex >= 0)
                    {
                        if (typeof(IStateAction).IsAssignableFrom(type))
                        {

                            choices.Types[existingIndex] = type;
                            choices.Names[existingIndex] = name;
                            choices.Pickers[existingIndex] = picker;
                            continue;
                        }
                    }
                    choices.Types.Add(type);
                    choices.Names.Add(name);
                    choices.Pickers.Add(picker);
                }
                ChoicesByType.Add(baseType, choices);
            }

            return choices;
        }
    }
}