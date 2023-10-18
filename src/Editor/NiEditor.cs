#define NIENGINE_ASSIGNNEWUID
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NiEngine;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System.IO;
using NiEngine.IO.SaveOverrides;
using UnityEditor.SceneManagement;
using Codice.CM.Common;

namespace NiEngine
{
    [UnityEditor.InitializeOnLoad]
    public static class EditorMenu
    {
#if UNITY_EDITOR
        private const string AutoSaveOnPlayName = "Tools/NiEngine/Auto-save on play";
        private const string DrawStatesGizmosName = "Tools/NiEngine/Debug/Gizmos";
        private const string DrawStatesLabelName = "Tools/NiEngine/Debug/Label";
        private const string LogAllEventsName = "Tools/NiEngine/Debug/Log All Events";
        private const string RecordOnPlayName = "Tools/NiEngine/Debug/RecordOnPlay";
        public static bool AutoSaveOnPlay = true;
        public static bool RecordOnPlay;

        private const string ShowRuntimeFieldsName = "Tools/NiEngine/Show Runtime Fields";
        public static bool ShowRuntimeFields = false;
        private const string ShowDebugFieldsName = "Tools/NiEngine/Show Debug Fields";
        public static bool ShowDebugFields = false;
        static EditorMenu()
        {
            Debug.Log("NiEngine EditorMenu InitializeOnLoad");
            DebugStates.DrawStatesGizmos = UnityEditor.EditorPrefs.GetBool(DrawStatesGizmosName, true);
            DebugStates.DrawStatesLabel = UnityEditor.EditorPrefs.GetBool(DrawStatesLabelName, false);
            DebugStates.LogAllEvents = UnityEditor.EditorPrefs.GetBool(LogAllEventsName, false);
            ShowRuntimeFields = UnityEditor.EditorPrefs.GetBool(ShowRuntimeFieldsName, false);
            UnityEditor.Menu.SetChecked(ShowRuntimeFieldsName, ShowRuntimeFields);
            ShowDebugFields = UnityEditor.EditorPrefs.GetBool(ShowDebugFieldsName, false);
            UnityEditor.Menu.SetChecked(ShowDebugFieldsName, ShowDebugFields);

            AutoSaveOnPlay = UnityEditor.EditorPrefs.GetBool(AutoSaveOnPlayName, true);

            RecordOnPlay = UnityEditor.EditorPrefs.GetBool(RecordOnPlayName, true);

            UnityEditor.EditorApplication.delayCall += () => SetDrawStates(DebugStates.DrawStatesGizmos, DebugStates.DrawStatesLabel);
            EditorApplication.playmodeStateChanged += () =>
            {
                if (EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isPlaying)
                {
                    if (EditorMenu.AutoSaveOnPlay)
                    {
                        //Debug.Log("Auto-saving all open scenes...");
                        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
                        AssetDatabase.SaveAssets();
                    }
                }
            };
            UnityEditor.Menu.SetChecked(AutoSaveOnPlayName, AutoSaveOnPlay);

            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                if (RecordOnPlay && Recording.EventRecorder.Instance == null)
                {
                    Debug.Log("NiEngine Begin Recording Events.");
                    Recording.EventRecorder.Instance = new Recording.EventRecorder();

                }
            }
        }

        [UnityEditor.MenuItem(AutoSaveOnPlayName)]
        private static void SetAutoSaveOnPlay()
        {
            AutoSaveOnPlay = !AutoSaveOnPlay;
            UnityEditor.Menu.SetChecked(AutoSaveOnPlayName, AutoSaveOnPlay);
            UnityEditor.EditorPrefs.SetBool(AutoSaveOnPlayName, AutoSaveOnPlay);
        }

        [UnityEditor.MenuItem(ShowRuntimeFieldsName)]
        private static void SetShowRuntimeFields()
        {
            ShowRuntimeFields = !ShowRuntimeFields;
            UnityEditor.Menu.SetChecked(ShowRuntimeFieldsName, ShowRuntimeFields);
            UnityEditor.EditorPrefs.SetBool(ShowRuntimeFieldsName, ShowRuntimeFields);
        }
        [UnityEditor.MenuItem(ShowDebugFieldsName)]
        private static void SetShowDebugFields()
        {
            ShowDebugFields = !ShowDebugFields;
            UnityEditor.Menu.SetChecked(ShowDebugFieldsName, ShowDebugFields);
            UnityEditor.EditorPrefs.SetBool(ShowDebugFieldsName, ShowDebugFields);
        }
        

        //[UnityEditor.MenuItem("Tools/NiEngine/Draw States/Off")]
        private static void SetOff() => SetDrawStates(false, false);

        //[UnityEditor.MenuItem(DrawStatesGizmosName)]
        private static void SetGizmo() => SetDrawStates(!DebugStates.DrawStatesGizmos, DebugStates.DrawStatesLabel);

        //[UnityEditor.MenuItem(DrawStatesLabelName)]
        private static void SetLabel() => SetDrawStates(DebugStates.DrawStatesGizmos, !DebugStates.DrawStatesLabel);
        public static void SetDrawStates(bool gizmos, bool label)
        {
            DebugStates.DrawStatesGizmos = gizmos;
            DebugStates.DrawStatesLabel = label;
            UnityEditor.Menu.SetChecked(DrawStatesGizmosName, DebugStates.DrawStatesGizmos);
            UnityEditor.Menu.SetChecked(DrawStatesLabelName, DebugStates.DrawStatesLabel);
            UnityEditor.EditorPrefs.SetBool(DrawStatesGizmosName, DebugStates.DrawStatesGizmos);
            UnityEditor.EditorPrefs.SetBool(DrawStatesLabelName, DebugStates.DrawStatesLabel);


        }
        public static void SetRecordOnPlay(bool value)
        {
            RecordOnPlay = value;
            UnityEditor.EditorPrefs.SetBool(RecordOnPlayName, RecordOnPlay);


        }


        //[UnityEditor.MenuItem(LogAllEventsName)]
        private static void SetLogAllEvents()
        {
            DebugStates.LogAllEvents = !DebugStates.LogAllEvents;
            UnityEditor.Menu.SetChecked(LogAllEventsName, DebugStates.LogAllEvents);
            UnityEditor.EditorPrefs.SetBool(LogAllEventsName, DebugStates.LogAllEvents);
        }

        public static GameObject DebugLabelAsset = (GameObject)UnityEditor.AssetDatabase.LoadAssetAtPath("Packages/NiEngine/src/Editor/Assets/label.prefab", typeof(GameObject));

#endif
    }

}

namespace NiEditor
{
    public class Assets
    {
        public static Texture2D IconReactionState = (Texture2D)AssetDatabase.LoadAssetAtPath("Packages/NiEngine/src/Editor/Assets/IconReactionState@16.png", typeof(Texture2D));
        public static Texture2D IconReactionReference = (Texture2D)AssetDatabase.LoadAssetAtPath("Packages/NiEngine/src/Editor/Assets/IconReactionReference.png", typeof(Texture2D));
        public static Texture2D IconCondition = (Texture2D)AssetDatabase.LoadAssetAtPath("Packages/NiEngine/src/Editor/Assets/IconCondition.png", typeof(Texture2D));
        public static Texture2D IconAction = (Texture2D)AssetDatabase.LoadAssetAtPath("Packages/NiEngine/src/Editor/Assets/IconReactionReference.png", typeof(Texture2D));

        public static VisualTreeAsset _EventRecord;
        public static VisualTreeAsset EventRecord => _EventRecord ??= AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/NiEngine/src/Editor/Assets/EventRecord.uxml");

        static VisualTreeAsset _FoldoutHeadAsset;
        public static VisualTreeAsset FoldoutHeadAsset => _FoldoutHeadAsset ??= AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/NiEngine/src/Editor/Assets/FoldoutHead.uxml");



    }


    //[CustomPropertyDrawer(typeof(EditorField))]
    //public class EditorFieldDrawer : PropertyDrawer
    //{

    //    [System.Diagnostics.Conditional("NIEDITOR_PRINT")]
    //    void Log(string msg)
    //    {
    //        Debug.Log(msg);
    //    }
    //    //Dictionary<SerializedObject, VisualElement> Cache = new();
    //    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    //    {

    //        if (attribute is not EditorField ef)
    //            return null;
    //        Log($"EditorClassDrawer.CreatePropertyGUI({property.propertyPath})");
    //        var field = new NiPropertyField();
    //        field.BindProperty(property);

    //        return field;
    //    }
    //}

    // TODO: need to update when undo/redo/past
    public class EditorBase : UnityEditor.Editor
    {
        [System.Diagnostics.Conditional("NIEDITOR_PRINT")]
        void Log(string msg)
        {
            Debug.Log(msg);
        }

        VisualElement Field = null;
        public override VisualElement CreateInspectorGUI()
        {
            if (Field == null)
            {
                var prop = serializedObject.GetIterator();
                Log($"EditorBase.CreatePropertyGUI({prop.propertyPath})");
                bool first = true;
                Field = new VisualElement();
                while (prop.NextVisible(first))
                {
                    if (first)
                    {
                        first = false;
                        if (prop.name == "m_Script")
                            prop.NextVisible(false);
                    }
                    //var (fi, v) = prop.FindFieldInfo();
                    var propInfo = prop.GetPropertyInfo();
                    var hide = propInfo.FieldInfo.GetCustomAttribute<HideInInspector>( inherit: true );
                    if (hide == null)
                    {
                        var propField = prop.Copy();
                        var field = new NiPropertyField(propInfo);
                        field.BindProperty(propField);
                        Field.Add(field);
                    }
                }
            }
            return Field;

        }
    }

    [CustomEditor(typeof(Grabbable))] public class GrabbableEditor : EditorBase { }
    [CustomEditor(typeof(ReactOnCollisionPair))] public class ReactOnCollisionPairEditor : EditorBase { }
    [CustomEditor(typeof(ReactOnInputKey))] public class ReactOnInputKeyEditor : EditorBase { }
    [CustomEditor(typeof(ReactOnFocus))] public class ReactOnFocusEditor : EditorBase { }
    [CustomEditor(typeof(Reactions))] public class ReactionsEditor : EditorBase { }
    [CustomEditor(typeof(NiVariables))] public class NiVariablesEditor : EditorBase { }
    [CustomEditor(typeof(SaveId))] 
    public class SaveIdEditor : EditorBase
    {
        VisualElement CreateCopyableLabel(string text, string copy)
        {
            var label = new Label(text);
            label.AddManipulator(new ContextualMenuManipulator((ContextualMenuPopulateEvent evt) =>
            {
                evt.menu.AppendAction("Copy", (x) =>
                {
                    GUIUtility.systemCopyBuffer = copy;
                });

            }));
            return label;
        }
        public override VisualElement CreateInspectorGUI()
        {
            if (target is not SaveId saveId)
                return null;

            var root = new VisualElement();
            //var prop = serializedObject.GetIterator();
            var propSaveType = serializedObject.FindProperty("SaveType");
            //var propSaveType = prop.FindPropertyRelative("SaveType");
            if (propSaveType != null)
            {
                var fieldSaveType = new NiPropertyField("Save Type");
                fieldSaveType.BindProperty(propSaveType);
                root.Add(fieldSaveType);
            }
            root.Add(CreateCopyableLabel($"Uid: {saveId.Uid}", $"{saveId.Uid}"));
            root.Add(CreateCopyableLabel($"Prefab Uid: {saveId.PrefabUid}", $"{saveId.PrefabUid}"));

            AddGameObjectDetails(root, saveId.gameObject);


            //if (UidObject.UidToObject.TryGetValue(saveId.Uid, out object existingObject))
            //{
            //    if (existingObject is SaveId existingSaveId )
            //    {
            //        if (existingSaveId != null && existingSaveId.gameObject != null)
            //        {
            //            var comparison = SaveId.IsSameGameObject(saveId.gameObject, existingSaveId.gameObject, out var shouldOverwrite);
            //            root.Add(new Label($"{comparison} with: {existingSaveId.gameObject.GetPathNameOrNull()}, shouldOverwrite:{shouldOverwrite}"));
            //            AddGameObjectDetails(root, existingSaveId.gameObject);
            //        }
            //        else
            //        {
            //            root.Add(new Label($"Conflicting with destroyed GameObject"));
            //        }
            //    }
            //}
            //var btRecompare = new Button(() =>
            //{
            //    if (UidObject.UidToObject.TryGetValue(saveId.Uid, out object existingObject))
            //    {
            //        if (existingObject is SaveId existingSaveId)
            //        {
            //            if (existingSaveId != null && existingSaveId.gameObject != null)
            //            {
            //                var comparison = SaveId.IsSameGameObject(saveId.gameObject, existingSaveId.gameObject, out var shouldOverwrite);
            //            }
            //        }
            //    }
            //});
            //btRecompare.contentContainer.Add(new Label("Recompare"));
            //root.Add(btRecompare);
            return root;
        }
        void AddGameObjectDetails(VisualElement root, GameObject obj)
        {
            if (EditorMenu.ShowDebugFields)
            {
                var saveId = obj.GetComponent<SaveId>();
                root.Add(CreateCopyableLabel($"UidOnLoad: {saveId.UidOnLoad}", $"{saveId.UidOnLoad}"));

                var btNewUid = new Button(() =>
                {
                    UidGameObjectRegistry.RemoveFromRegistry(saveId);
                    UidGameObjectRegistry.RegisterWithNewUid(saveId);
                });
                btNewUid.contentContainer.Add(new Label("Assign New Uid"));
                root.Add(btNewUid);

                root.Add(new Label($"IsRuntime: {saveId.IsRuntime}"));
                root.Add(new Label($"IsInstantiatedRoot: {saveId.IsInstantiatedRoot}"));

                var sourceSaveId = saveId;

                int max = 100;
                while (max >= 0)
                {
                    var previous = sourceSaveId;
                    if (sourceSaveId == null)
                        break;
                    sourceSaveId = UnityEditor.PrefabUtility.GetCorrespondingObjectFromSource(sourceSaveId);
                    if (sourceSaveId != null && sourceSaveId != previous)
                    {
                        var prefabType = UnityEditor.PrefabUtility.GetPrefabAssetType(sourceSaveId);
                        //var path = UnityEditor.PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(sourceSaveId);
                        var path = AssetDatabase.GetAssetPath(sourceSaveId);

                        var label = CreateCopyableLabel($"Link: Uid:{sourceSaveId.Uid}, PrefabUid:{sourceSaveId.PrefabUid}, type:{prefabType}, path:{path}", $"{sourceSaveId.Uid}");
                        root.Add(label);
                        --max;
                    }
                    else
                        break;
                }

                root.Add(new Label($"IID: {saveId.GetInstanceID()}"));
                root.Add(new Label($"IsPartOfPrefabAsset: {PrefabUtility.IsPartOfPrefabAsset(obj)}"));
                root.Add(new Label($"GetPrefabAssetType: {PrefabUtility.GetPrefabAssetType(obj)}"));
                root.Add(new Label($"IsPartOfPrefabInstance: {PrefabUtility.IsPartOfPrefabInstance(obj)}"));
                root.Add(new Label($"GetPrefabInstanceStatus: {PrefabUtility.GetPrefabInstanceStatus(obj)}"));
                root.Add(new Label($"IsPartOfPrefabThatCanBeAppliedTo: {PrefabUtility.IsPartOfPrefabThatCanBeAppliedTo(obj)}"));
                root.Add(new Label($"IsPartOfNonAssetPrefabInstance: {PrefabUtility.IsPartOfNonAssetPrefabInstance(obj)}"));
                root.Add(new Label($"GetPrefabAssetPathOfNearestInstanceRoot: {PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(obj)}"));

                var stageCurrent = PrefabStageUtility.GetPrefabStage(obj);
                root.Add(new Label($"GetPrefabStage: {stageCurrent != null}"));

                var stage = PrefabStageUtility.GetCurrentPrefabStage();
                if (stage != null)
                {
                    var isPartOfPrefabContents = stage.IsPartOfPrefabContents(obj);
                    root.Add(new Label($"IsPartOfPrefabContents: {isPartOfPrefabContents}"));
                    if (isPartOfPrefabContents)
                        root.Add(new Label($"stage.assetPath: {stage.assetPath}"));

                }

                root.Add(new Label($"Path: \"{obj.GetPath(out var isSceneOrStaged)}\", isSceneOrStaged:{isSceneOrStaged}"));
            }
        }
    }


    [CustomPropertyDrawer(typeof(DerivedClassPicker))]
    public class DerivedClassPickerDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            Toggle Toggle;
            DropdownField DropdownField;
            VisualElement VeContent;
            //PropertyField PropertyField;


            var veRoot = new VisualElement();
            ReactionStateMachineEditor.ClassPickerAsset.CloneTree(veRoot);
            Toggle = veRoot.Query<Toggle>().First();
            Toggle.value = property.isExpanded;
            DropdownField = veRoot.Query<DropdownField>().First();
            VeContent = veRoot.Query<VisualElement>("veContent").First();

            VeContent.style.display = property.isExpanded ? DisplayStyle.Flex : DisplayStyle.None;
            Toggle.RegisterCallback<ChangeEvent<bool>>(x =>
            {
                VeContent.style.display = x.newValue ? DisplayStyle.Flex : DisplayStyle.None;
                property.isExpanded = x.newValue;
                property.serializedObject.ApplyModifiedProperties();
            });

            var itor = property.Copy();
            int d = itor.depth;
            bool child = true;
            while (itor.Next(child))
            {
                if (itor.depth <= d)
                    break;
                child = false;

                var field = new PropertyField();
                field.BindProperty(itor.Copy());
                VeContent.Add(field);
            }

            Type baseType = fieldInfo.FieldType;
            if (attribute is DerivedClassPicker derivedClassPicker)
            {
                baseType = derivedClassPicker.BaseType;
                //showPrefixLabel = derivedClassPicker.ShowPrefixLabel;
            }
            var choicesType = new List<System.Type>();
            var choices = new List<string>();
            var currentIndex = -1;
            var type = property.managedReferenceValue?.GetType();
            foreach (var (t, name) in DerivedClassOf(baseType))
            {
                if(type == t)
                {
                    currentIndex = choices.Count;
                }
                choices.Add(name);
                choicesType.Add(t);
            }
            DropdownField.choices = choices;
            DropdownField.index = currentIndex;

            DropdownField.RegisterValueChangedCallback(x =>
            {
                int index = choices.FindIndex(y => y == x.newValue);
                //Debug.Log(index);
                var t = choicesType[index];
                property.isExpanded = true;

                
                property.managedReferenceValue = t.GetConstructor(Type.EmptyTypes).Invoke(null);
                
                property.isExpanded = true;
                property.serializedObject.ApplyModifiedProperties();

                VeContent.Clear();
                VeContent.style.display = property.isExpanded ? DisplayStyle.Flex : DisplayStyle.None;

                var itor = property.Copy();
                int d = itor.depth;
                bool child = true;
                while (itor.Next(child))
                {
                    if (itor.depth <= d)
                        break;
                    child = false;

                    var field = new PropertyField();
                    field.BindProperty(itor.Copy());
                    
                    VeContent.Add(field);
                }

            });
            return veRoot;
        }

        //// TODO can this be removed?
        //public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        //{
        //    float h = 0;

        //    h += EditorGUIUtility.singleLineHeight;
        //    if (property.managedReferenceValue != null && property.isExpanded)
        //        h += EditorGUI.GetPropertyHeight(property);
        //    return h;
        //}

        //public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        //{
        //    bool showPrefixLabel = true;
        //    Type baseType = fieldInfo.FieldType;
        //    if (attribute is DerivedClassPicker derivedClassPicker)
        //    {
        //        baseType = derivedClassPicker.BaseType;
        //        showPrefixLabel = derivedClassPicker.ShowPrefixLabel;
        //    }

        //    Rect dropdownRect = position;
        //    if (showPrefixLabel)
        //    {
        //        EditorGUI.PrefixLabel(position, new GUIContent(property.displayName));
        //        dropdownRect.x += EditorGUIUtility.labelWidth + 2;
        //        dropdownRect.width -= EditorGUIUtility.labelWidth + 2;
        //    }

        //    dropdownRect.height = EditorGUIUtility.singleLineHeight;
        //    DerivedClassPicker(dropdownRect, baseType, property);

        //    if (property.managedReferenceValue != null)
        //        EditorGUI.PropertyField(position, property, GUIContent.none, true);
        //}


        static Dictionary<Type, Dictionary<Type, string>> _DerivedClass;

        static void OnBeforeAssemblyReload()
        {
            _DerivedClass = null;
        }

        static string NameOfDerivedClass(Type baseType, Type derivedType)
        {
            if (derivedType is null) return "<Null>";
            var types = DerivedClassOf(baseType);
            if (types.TryGetValue(derivedType, out var name))
                return name;
            return derivedType.FullName;
        }
        static Dictionary<Type, string> DerivedClassOf(Type baseType)
        {

            if (_DerivedClass == null)
            {
                AssemblyReloadEvents.beforeAssemblyReload -= OnBeforeAssemblyReload;
                AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
                _DerivedClass = new();
            }
            if (!_DerivedClass.TryGetValue(baseType, out var derivedTypes))
            {
                derivedTypes = new();
                //ClassPickerName

                var ll = System.AppDomain.CurrentDomain.GetAssemblies().SelectMany(
                    x => x.GetTypes().Where(t => t.IsClass && !t.IsAbstract && baseType.IsAssignableFrom(t)));
                foreach (var type in System.AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes().Where(t => t.IsClass && !t.IsAbstract && baseType.IsAssignableFrom(t))))
                //foreach (var type in System.AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes().Where(t => (t.IsValueType || (t.IsClass && !t.IsAbstract)) && baseType.IsAssignableFrom(t))))
                {
                    string name = type.FullName;
                    foreach (var attribute in type.GetCustomAttributes(inherit: false))
                        if (attribute is ClassPickerName classPickerName)
                        {
                            name = classPickerName.Name;
                            break;
                        }
                    derivedTypes.Add(type, name);
                }
                _DerivedClass.Add(baseType, derivedTypes);

            }
            return derivedTypes;
        }
        public static void DerivedClassPicker(Rect position, Type baseType, SerializedProperty property)
        {
            var type = property.managedReferenceValue?.GetType();
            string typeName = NameOfDerivedClass(baseType, type);
            //string typeName = property.managedReferenceValue?.GetType().Name ?? "Not set";
            if (EditorGUI.DropdownButton(position, new(typeName), FocusType.Keyboard))
            {
                GenericMenu menu = new GenericMenu();

                // null
                menu.AddItem(new GUIContent("Null"), property.managedReferenceValue == null, () =>
                {
                    property.isExpanded = false;
                    property.managedReferenceValue = null;
                    property.serializedObject.ApplyModifiedProperties();
                });

                //string typeName = NameOfDerivedClass(baseType, type);
                // inherited types
                foreach (var (t, name) in DerivedClassOf(baseType))
                {
                    menu.AddItem(new GUIContent(name), typeName == t.Name, () =>
                    {
                        property.isExpanded = true;
                        property.managedReferenceValue = t.GetConstructor(Type.EmptyTypes).Invoke(null); ;
                        property.serializedObject.ApplyModifiedProperties();
                    });
                }
                menu.ShowAsContext();
            }
        }
    }

    [CustomPropertyDrawer(typeof(SerializableType<>), true)]
    public class InspectableTypeDrawer : PropertyDrawer
    {

        static Dictionary<Type, Dictionary<Type, string>> _DerivedClass;
        static string NameOfDerivedClass(Type baseType, Type derivedType)
        {
            if (derivedType is null) return "<Null>";
            var types = DerivedClassOf(baseType);
            if (types.TryGetValue(derivedType, out var name))
                return name;
            return derivedType.FullName;
        }

        static Dictionary<Type, string> DerivedClassOf(Type baseType)
        {

            if (_DerivedClass == null)
            {
                _DerivedClass = new();
            }
            if (!_DerivedClass.TryGetValue(baseType, out var derivedTypes))
            {
                derivedTypes = new();
                var ll = System.AppDomain.CurrentDomain.GetAssemblies().SelectMany(
                    x => x.GetTypes().Where(t => t.IsClass && !t.IsAbstract && baseType.IsAssignableFrom(t)));
                foreach (var type in System.AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes().Where(t => t.IsClass && !t.IsAbstract && baseType.IsAssignableFrom(t))))
                {
                    string name = type.FullName;
                    foreach (var attribute in type.GetCustomAttributes(inherit: false))
                        if (attribute is ClassPickerName classPickerName)
                        {
                            name = classPickerName.Name;
                            break;
                        }
                    derivedTypes.Add(type, name);
                }
                _DerivedClass.Add(baseType, derivedTypes);

            }
            return derivedTypes;
        }

        //GUIContent[] _optionLabels;
        //int _selectedIndex;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var baseTypeProperty = property.FindPropertyRelative("baseTypeName");
            var baseType = Type.GetType(baseTypeProperty.stringValue);


            var propTypeName = property.FindPropertyRelative("TypeName");
            string typeName = propTypeName.stringValue;
            //string typeName = NameOfClass(property.managedReferenceValue?.GetType());
            var layout = RectLayout.Horizontal(position);
            var str = EditorGUI.TextField(layout.AcquireWidth(-16), propTypeName.stringValue);
            Debug.Log(str);
            if (EditorGUI.DropdownButton(layout.AcquireWidth(16), label, FocusType.Keyboard))
            {
                GenericMenu menu = new GenericMenu();

                // null
                menu.AddItem(new GUIContent("Null"), propTypeName.stringValue == null, () =>
                {
                    propTypeName.stringValue = null;
                });

                // inherited types
                foreach (var (t, name) in DerivedClassOf(baseType))
                {
                    if (string.IsNullOrEmpty(str) || name.IndexOf(str, StringComparison.OrdinalIgnoreCase) != -1)
                    {
                        menu.AddItem(new GUIContent(name), typeName == t.FullName, () =>
                        {
                            propTypeName.stringValue = t.FullName;
                        });
                    }
                }
                menu.ShowAsContext();
            } else
            {

                propTypeName.stringValue = str;
            }
            

            EditorGUI.EndProperty();

        }

    }

    //[CustomEditor(typeof(SaveId))]
    //public class SaveIdDrawer : Editor
    //{
    //    public override VisualElement CreateInspectorGUI()
    //    {
    //        if(target is SaveId saveId)
    //        {
    //            return new Label($"Uid: {saveId.Uid}");
    //        }
    //        return new Label("??");
    //    }

    //}

    [CustomPropertyDrawer(typeof(Uid))]
    public class UidPropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            Uid uid = Uid.FromSerializedProperty(property);
            return new Label($"{property.displayName}: {uid}");

        }

    }

    //[CustomPropertyDrawer(typeof(ReactionStateMachine.StateGroup))]
    //public class ReactionStateMachineStateGroupPropertyDrawer : PropertyDrawer
    //{
    //    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    //    {
    //        // Name
    //        float h = EditorGUIUtility.singleLineHeight + 5;

    //        if (property.isExpanded)
    //        {

    //            // Name & Notes foldout
    //            h += EditorGUIUtility.singleLineHeight;

    //            var propNotes = property.FindPropertyRelative("Notes");
    //            if (propNotes.isExpanded)
    //            {
    //                // Name
    //                h += EditorGUIUtility.singleLineHeight;
    //                // Notes field
    //                h += EditorGUIUtility.singleLineHeight * 4;
    //            }

    //            //// Conditions
    //            //h += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("TestCondition"));

    //            //// Action
    //            //h += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("TestAction"));

    //            var propIsActiveState = property.FindPropertyRelative("HasActiveState");

    //            // Name & Notes foldout
    //            h += EditorGUIUtility.singleLineHeight;

    //            if (propIsActiveState.isExpanded)
    //            {
    //                h += EditorGUI.GetPropertyHeight(propIsActiveState);

    //            }

    //            var propStates = property.FindPropertyRelative("States");
    //            for (int i = 0; i != propStates.arraySize; ++i)
    //            {
    //                var elem = propStates.GetArrayElementAtIndex(i);
    //                h += EditorGUI.GetPropertyHeight(elem);
    //            }
    //            h += EditorGUIUtility.singleLineHeight;

    //            // States 
    //            //h += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("States"));

    //            h += 7;
    //        }
    //        return h;
    //    }
    //    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    //    {
    //        EditorGUI.BeginProperty(position, label, property);
    //        var layout = RectLayout.Vertical(position);

    //        var propGroupName = property.FindPropertyRelative("GroupName");
    //        var propName = propGroupName.FindPropertyRelative("Name");
    //        property.isExpanded = layout.Foldout(property.isExpanded, new GUIContent(propName.stringValue, Assets.IconReactionState));



    //        var propHasActiveState = property.FindPropertyRelative("HasActiveState");
    //        Color stateColor = propHasActiveState.boolValue ? Color.green : Color.black;// new Color(0.6f, 0.6f, 0.6f);

    //        Rect left = position;
    //        //left.yMin += EditorGUIUtility.singleLineHeight;
    //        left.xMin -= 26;
    //        left.width = 3;
    //        left.height = EditorGUIUtility.singleLineHeight;
    //        EditorGUI.DrawRect(left, stateColor);

    //        if (property.isExpanded)
    //        {

    //            // Name

    //            // Notes dropdown
    //            var propNotes = property.FindPropertyRelative("Notes");

    //            propNotes.isExpanded = layout.Foldout(propNotes.isExpanded, new("Group Name & Notes"));

    //            // Notes
    //            if (propNotes.isExpanded)
    //            {
    //                layout.PropertyField(propName, new GUIContent("Name"));
    //                propNotes.stringValue = EditorGUI.TextField(layout.AcquireHeight(EditorGUIUtility.singleLineHeight * 4), propNotes.stringValue);
    //            }
    //            //layout.Label("Run-Time Values");
    //            propHasActiveState.isExpanded = layout.Foldout(propHasActiveState.isExpanded, new("Run-Time Values"));
    //            if (propHasActiveState.isExpanded)
    //            {
    //                layout.PropertyField(propHasActiveState, new GUIContent("Has Active State"));
    //                //layout.PropertyField(property.FindPropertyRelative("LastBeginEvent"), new GUIContent("Last Begin Event:"), includeChildren: true);
    //                //layout.PropertyField(property.FindPropertyRelative("LastEndEvent"), new GUIContent("Last End Event:"), includeChildren: true);

    //            }

    //            var propStates = property.FindPropertyRelative("States");
    //            for (int i = 0; i != propStates.arraySize; ++i)
    //            {
    //                var group = propStates.GetArrayElementAtIndex(i);
    //                layout.PropertyField(group);
    //            }
    //            if (layout.Button("Add State"))
    //            {
    //                propStates.InsertArrayElementAtIndex(0);
    //            }
    //            //layout.PropertyField(property.FindPropertyRelative("States"), new GUIContent("States:", Assets.IconReactionState));
    //        }
    //        EditorGUI.EndProperty();
    //    }

    //}

}