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

namespace NiEditor
{
    [CustomPropertyDrawer(typeof(ConditionSet))]
    public class ConditionSetDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var (fi, obj) = property.FindFieldInfo();
            var ef = fi.GetCustomAttribute<EditorField>();
            ListView2 listView = new ListView2(ef);
            var label = ef?.ShowPrefixLabel ?? true ?
                (ef?.Prefix ?? property.displayName)
                : "";
            listView.SetText(label);
            listView.tooltip = property.tooltip;
            listView.SetIcon(Assets.IconCondition);
            listView.SetColor(new Color(0.75f, 0.75f, 0));
            listView.style.marginBottom = 2;
            listView.BindProperty(property.FindPropertyRelative("Conditions"), property.FindPropertyRelative("Expanded"));
            return listView;
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property.FindPropertyRelative("Conditions"));
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.PropertyField(position, property.FindPropertyRelative("Conditions"), new GUIContent(property.name, Assets.IconCondition), true);
        }
    }

    public class ActionSetsBaseDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var (fi, obj) = property.FindFieldInfo();
            var ef = fi.GetCustomAttribute<EditorField>();
            ListView2 listView = new ListView2(ef);

            var label =  ef?.ShowPrefixLabel ?? true ?
                (ef?.Prefix ?? property.displayName)
                : "";

            listView.SetText(label);
            listView.tooltip = property.tooltip;
            listView.style.marginBottom = 2;
            listView.SetIcon(Assets.IconAction);

            if (property.name.Contains("OnBegin"))
            {
                listView.SetColor(new Color(0.0f, 0.75f, 0.75f));
            }
            else if (property.name.Contains("OnUpdate"))
            {
                listView.SetColor(new Color(0.2f, 0.2f, 0.75f));
            }
            else if (property.name.Contains("OnEnd"))
            {
                listView.SetColor(new Color(0.75f, 0, 0.75f));
            }
            var propActions = property.FindPropertyRelative("Actions");
            if (propActions != null)
                listView.BindProperty(propActions, property.FindPropertyRelative("Expanded"));
            return listView;
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property.FindPropertyRelative("Actions"));
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.PropertyField(position, property.FindPropertyRelative("Actions"), new GUIContent(property.displayName, Assets.IconAction), true);
        }
    }
    [CustomPropertyDrawer(typeof(ActionSet))] public class ActionSetDrawer : ActionSetsBaseDrawer { }
    [CustomPropertyDrawer(typeof(ActionSetNoHead))] public class ActionSetNoHeadDrawer : ActionSetsBaseDrawer { }

    [CustomPropertyDrawer(typeof(StateActionSet))] public class StateActionSetDrawer : ActionSetsBaseDrawer { }
    [CustomPropertyDrawer(typeof(StateActionSetNoHead))] public class StateActionSetNoHeadDrawer : ActionSetsBaseDrawer { }



    [CustomEditor(typeof(ReactionStateMachine))]
    public class ReactionStateMachineEditor : UnityEditor.Editor
    {
        public static VisualTreeAsset _StateMachineAsset;
        public static VisualTreeAsset _GroupAsset;
        public static VisualTreeAsset _StateAsset;
        public static VisualTreeAsset _StateFoldoutAsset;
        public static VisualTreeAsset _ClassPickerAsset;

        public static VisualTreeAsset StateMachineAsset => _StateMachineAsset ??= AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/NiEngine/src/Editor/Assets/StateMachine.uxml");
        public static VisualTreeAsset GroupAsset => _GroupAsset ??= AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/NiEngine/src/Editor/Assets/StateGroup.uxml");
        public static VisualTreeAsset StateAsset => _StateAsset ??= AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/NiEngine/src/Editor/Assets/State.uxml");
        public static VisualTreeAsset StateFoldoutAsset => _StateFoldoutAsset ??= AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/NiEngine/src/Editor/Assets/StateFoldout.uxml");
        
        public static VisualTreeAsset ClassPickerAsset => _ClassPickerAsset ??= AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/NiEngine/src/Editor/Assets/ClassPicker.uxml");

        ReactionStateMachineVE Root;


        public void OnEnable()
        {
            Undo.undoRedoPerformed += OnUndo;
        }
        private void OnDisable()
        {
            Undo.undoRedoPerformed -= OnUndo;
            Root = null;
        }
        void OnUndo()
        {
            serializedObject.Update();
            Root?.OnUndo();
        }
        public override VisualElement CreateInspectorGUI()
        {
            if(Root == null)
                Root = new ReactionStateMachineVE(this, serializedObject);
            return Root;

        }


    }

}