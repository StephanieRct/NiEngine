using System.Linq;
using NiEngine;
using NUnit.Framework.Api;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using NiEngine.Expressions.GameObjects;

namespace NiEditor
{

    [CustomPropertyDrawer(typeof(ReactionReference))]
    public class ReactionReferencePropertyDrawer : PropertyDrawer
    {
        public static void Update(SerializedProperty property, Label lbFound)
        {
            var propTarget = property.FindPropertyRelative("Target");
            var propStateName = property.FindPropertyRelative("ReactionName");
            
            if (propTarget.managedReferenceValue is IExpressionGameObject {IsConst:true} r)
            {
                GameObject self = null;
                if (property.serializedObject.targetObject is MonoBehaviour mb2)
                    self = mb2.gameObject;
                var constValue = r.GetValue(new (), EventParameters.WithoutTrigger(self, null));
                if(constValue == null)
                    lbFound.text = "No Object";
                else if (ReactionReference.HasReaction(constValue, propStateName.stringValue, onlyEnabled:false, onlyActive:false, ReactionReference.k_MaxLoop))
                    lbFound.text = "Found";
                else
                    lbFound.text = "Missing";
                return;
            }
            if (propTarget.managedReferenceValue is IExpressionGameObjects { IsConst: true } rs)
            {
                GameObject self = null;
                if (property.serializedObject.targetObject is MonoBehaviour mb2)
                    self = mb2.gameObject;
                int count = 0;
                int countFound = 0;
                foreach (var target in rs.GetValues(new(), EventParameters.WithoutTrigger(self, null)))
                {
                    ++count;
                    if (target is not null && ReactionReference.HasReaction(target, propStateName.stringValue, onlyEnabled: false, onlyActive: false, ReactionReference.k_MaxLoop))
                        ++countFound;
                }
                if(count == 0)
                    lbFound.text = "Missing";
                else if(countFound == count)
                    lbFound.text = "Found";
                else 
                    lbFound.text = $"{countFound}/{count}";
                return;
            }
            lbFound.text = "Virtual";

        }
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var propType = property.FindPropertyRelative("ReactionType");
            //var propTargetReference = property.FindPropertyRelative("TargetObjectReference");
            var propTarget = property.FindPropertyRelative("Target");
            var propStateName = property.FindPropertyRelative("ReactionName");

            var info = property.GetPropertyInfo();

            var veRoot = new VisualElement();

            // "Found" label
            var lbFound = new Label("");
            lbFound.FixSizeByCharLines(12, 1);

            // Target
            var pfTargetReference = new NiPropertyField("");
            pfTargetReference.BindProperty(propTarget);
            pfTargetReference.RegisterValueChangeCallback(x =>
            {
                //Debug.Log("Target Changed");
                Update(property, lbFound);
            });
            veRoot.Add(pfTargetReference);

            // "Name" label
            var tfName = new TextField();
            tfName.MinWidthByCharLines(12);
            //tfName.FixSizeByCharLines(24, 1);
            tfName.value = propStateName.stringValue;
            tfName.style.flexGrow = 1;
            tfName.RegisterCallback<ChangeEvent<string>>(x =>
            {
                propStateName.stringValue = x.newValue;
                propStateName.serializedObject.ApplyModifiedProperties();
                Update(property, lbFound);
            });


            if (info.EditorField?.Inline ?? false)
            {
                veRoot.Add(tfName);
                veRoot.Add(lbFound);
                veRoot.style.flexDirection = FlexDirection.Row;
            }
            else
            {
                veRoot.style.flexDirection = FlexDirection.Column;
                var veNameRow = new VisualElement();
                veNameRow.style.flexDirection = FlexDirection.Row;

                var lbName = new Label("Name:");
                lbName.FixSizeByCharLines(5, 1);
                veNameRow.Add(lbName);
                veNameRow.Add(tfName);
                veNameRow.Add(lbFound);
                veRoot.Add(veNameRow);

            }

            Update(property, lbFound);
            return veRoot;


        }


        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float h = 8;
            h += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("ReactionName"));
            return h;
        }

        //public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        //{
        //    EditorGUI.BeginProperty(position, label, property);
        //    var layout = RectLayout.Horizontal(position);
        //    layout = layout.SubVertical();
        //    var layoutLine0 = layout.SubHorizontal(EditorGUIUtility.singleLineHeight);

        //    layoutLine0.Label("on");
        //    var propTargetReference = property.FindPropertyRelative("TargetObjectReference");

        //    layoutLine0.PropertyField(propTargetReference);

        //    //var t = (GameObjectReference.TypeEnum)propTargetReference_Type.intValue;
        //    //bool isVirtualCall = (t != GameObjectReference.TypeEnum.Object && t != GameObjectReference.TypeEnum.Self);
        //    var gameObjectReference = GameObjectReference.FromProperty(propTargetReference);//(GameObjectReference)propTargetReference.value;
        //    bool isVirtualCall = (gameObjectReference.Type != GameObjectReference.TypeEnum.Object && gameObjectReference.Type != GameObjectReference.TypeEnum.Self);

        //    GameObject self = null;
        //    if (property.serializedObject.targetObject is MonoBehaviour mb2)
        //        self = mb2.gameObject;
        //    gameObjectReference.TryGetTargetGameObject(new EventParameters()
        //    {
        //        Self = self,
        //        Current = EventParameters.ParameterSet.WithoutTrigger(self),
        //        OnBegin = EventParameters.ParameterSet.Default
        //    }, out var targetObject);

        //    if (targetObject != null || isVirtualCall)
        //    {
        //        var layoutLine1 = layout.SubHorizontal(EditorGUIUtility.singleLineHeight);
        //        layoutLine1.Label("Name:");
        //        var propStateName = property.FindPropertyRelative("ReactionName");
        //        layoutLine1.PropertyField(propStateName, -RectLayout.WidthOf("Missing"));
        //        string reactionName = propStateName.stringValue;

        //        if (isVirtualCall)
        //        {
        //            layoutLine1.Label("");
        //        }
        //        else if (ReactionReference.HasReaction(targetObject, reactionName, onlyEnabled: false, onlyActive: false, ReactionReference.k_MaxLoop))
        //        {
        //            layoutLine1.Label("Found");
        //        }
        //        else
        //        {
        //            layoutLine1.Label("Missing");
        //        }

        //    }

        //    EditorGUI.EndProperty();
        //}
    }

    [CustomPropertyDrawer(typeof(StateReactionReference))]
    public class StateReactionReferencePropertyDrawer : PropertyDrawer
    {
        static string FormatHasReaction(GameObject obj, string name)
            => ReactionReference.HasReaction(obj, name, onlyEnabled: false, onlyActive: false, ReactionReference.k_MaxLoop) ? "Found" : "Missing";

        static string FormatHasReaction(int count, int countFound)
            => count == countFound? "Found": $"{countFound}/{count}";

        public static void Update(SerializedProperty property, Label lbFound)
        {
            var propTarget = property.FindPropertyRelative("Target");
            var propOnBegin = property.FindPropertyRelative("OnBegin");
            var propOnEnd = property.FindPropertyRelative("OnEnd");

            if (propTarget.managedReferenceValue is IExpressionGameObject { IsConst: true } r)
            {
                GameObject self = null;
                if (property.serializedObject.targetObject is MonoBehaviour mb2)
                    self = mb2.gameObject;
                var constValue = r.GetValue(new(), EventParameters.WithoutTrigger(self, null));
                if (constValue == null)
                    lbFound.text = "No Object";
                else
                    lbFound.text = $"{FormatHasReaction(constValue, propOnBegin.stringValue)} | {FormatHasReaction(constValue, propOnEnd.stringValue)}";
                return;
            }

            if (propTarget.managedReferenceValue is IExpressionGameObjects { IsConst: true } rs)
            {
                GameObject self = null;
                if (property.serializedObject.targetObject is MonoBehaviour mb2)
                    self = mb2.gameObject;
                
                int count = 0;
                int countFoundOnBegin = 0;
                int countFoundOnEnd = 0;
                foreach (var target in rs.GetValues(new(), EventParameters.WithoutTrigger(self, null)))
                {
                    ++count;
                    if (target is not null)
                    {
                        if (ReactionReference.HasReaction(target, propOnBegin.stringValue, onlyEnabled: false, onlyActive: false, ReactionReference.k_MaxLoop))
                            ++countFoundOnBegin;
                        if (ReactionReference.HasReaction(target, propOnEnd.stringValue, onlyEnabled: false, onlyActive: false, ReactionReference.k_MaxLoop))
                            ++countFoundOnEnd;
                    }
                }
                if (count == 0)
                    lbFound.text = "No Object";
                else
                    lbFound.text = $"{FormatHasReaction(count, countFoundOnBegin)} | {FormatHasReaction(count, countFoundOnEnd)}";
                return;
            }
            lbFound.text = "Virtual";

        }
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            //var propType = property.FindPropertyRelative("ReactionType");
            //var propTargetReference = property.FindPropertyRelative("TargetObjectReference");
            var propTarget = property.FindPropertyRelative("Target");
            //var propStateName = property.FindPropertyRelative("ReactionName");
            var propOnBegin = property.FindPropertyRelative("OnBegin");
            var propOnEnd = property.FindPropertyRelative("OnEnd");

            var info = property.GetPropertyInfo();

            var veRoot = new VisualElement();

            // "Found" label
            var lbFound = new Label("");
            lbFound.FixSizeByCharLines(12, 1);

            // Target
            var pfTargetReference = new NiPropertyField("");
            pfTargetReference.style.minWidth = 128;
            pfTargetReference.BindProperty(propTarget);
            pfTargetReference.RegisterValueChangeCallback(x =>
            {
                //Debug.Log("Target Changed");
                Update(property, lbFound);
            });
            veRoot.Add(pfTargetReference);

            // OnBegin "Name" label
            var tfNameOnBegin = new TextField();
            tfNameOnBegin.MinWidthByCharLines(8);
            tfNameOnBegin.style.flexGrow = 1;
            tfNameOnBegin.value = propOnBegin.stringValue;
            //tfNameOnBegin.style.minWidth = 64;
            tfNameOnBegin.RegisterCallback<ChangeEvent<string>>(x =>
            {
                propOnBegin.stringValue = x.newValue;
                propOnBegin.serializedObject.ApplyModifiedProperties();
                Update(property, lbFound);
            });

            // OnEnd "Name" label
            var tfNameOnEnd = new TextField();
            tfNameOnEnd.MinWidthByCharLines(8);
            tfNameOnEnd.style.flexGrow = 1;
            tfNameOnEnd.value = propOnEnd.stringValue;
            //tfNameOnEnd.style.minWidth = 64;
            tfNameOnEnd.RegisterCallback<ChangeEvent<string>>(x =>
            {
                propOnEnd.stringValue = x.newValue;
                propOnEnd.serializedObject.ApplyModifiedProperties();
                Update(property, lbFound);
            });

            if (info.EditorField?.Inline ?? false)
            {
                veRoot.Add(tfNameOnBegin);
                var lbSep = new Label("|");
                veRoot.Add(lbSep);
                veRoot.Add(tfNameOnEnd);
                veRoot.Add(lbFound);
                veRoot.style.flexDirection = FlexDirection.Row;
            }
            else
            {
                veRoot.style.flexDirection = FlexDirection.Column;
                var veNameRow = new VisualElement();
                veNameRow.style.flexDirection = FlexDirection.Row;

                var lbName = new Label("Name:");
                lbName.FixSizeByCharLines(5, 1);
                veNameRow.Add(lbName);
                veNameRow.Add(tfNameOnBegin);
                var lbSep = new Label("|");
                veNameRow.Add(lbSep);
                veNameRow.Add(tfNameOnEnd);
                veNameRow.Add(lbFound);
                veRoot.Add(veNameRow);

            }

            Update(property, lbFound);
            return veRoot;


        }


        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float h = 8;
            h += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("ReactionName"));
            return h;
        }
        //public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        //{
        //    EditorGUI.BeginProperty(position, label, property);
        //    var layout = RectLayout.Horizontal(position);
        //    layout = layout.SubVertical();
        //    var layoutLine0 = layout.SubHorizontal(EditorGUIUtility.singleLineHeight);

        //    layoutLine0.Label("on");
        //    var propTargetReference = property.FindPropertyRelative("TargetObjectReference");

        //    layoutLine0.PropertyField(propTargetReference);

        //    //var t = (GameObjectReference.TypeEnum)propTargetReference_Type.intValue;
        //    //bool isVirtualCall = (t != GameObjectReference.TypeEnum.Object && t != GameObjectReference.TypeEnum.Self);
        //    var gameObjectReference = GameObjectReference.FromProperty(propTargetReference);//(GameObjectReference)propTargetReference.value;
        //    bool isVirtualCall = (gameObjectReference.Type != GameObjectReference.TypeEnum.Object && gameObjectReference.Type != GameObjectReference.TypeEnum.Self);

        //    GameObject self = null;
        //    if (property.serializedObject.targetObject is MonoBehaviour mb2)
        //        self = mb2.gameObject;
        //    gameObjectReference.TryGetTargetGameObject(new EventParameters()
        //    {
        //        Self = self,
        //        Current = EventParameters.ParameterSet.WithoutTrigger(self),
        //        OnBegin = EventParameters.ParameterSet.Default
        //    }, out var targetObject);

        //    if (targetObject != null || isVirtualCall)
        //    {
        //        var layoutLine1 = layout.SubHorizontal(EditorGUIUtility.singleLineHeight);
        //        layoutLine1.Label("Name:");
        //        var propStateName = property.FindPropertyRelative("ReactionName");
        //        layoutLine1.PropertyField(propStateName, -RectLayout.WidthOf("Missing"));
        //        string reactionName = propStateName.stringValue;

        //        if (isVirtualCall)
        //        {
        //            layoutLine1.Label("");
        //        }
        //        else if (ReactionReference.HasReaction(targetObject, reactionName))
        //        {
        //            layoutLine1.Label("Found");
        //        }
        //        else
        //        {
        //            layoutLine1.Label("Missing");
        //        }

        //    }

        //    EditorGUI.EndProperty();
        //}
    }

    ////[CustomPropertyDrawer(typeof(StateReactionReference))]
    //public class StateReactionReferenceDrawerOld : PropertyDrawer
    //{
    //    public static void Update(SerializedProperty property, Label lbFoundBegin, Label lbFoundEnd)
    //    {
    //        //var propType = property.FindPropertyRelative("ReactionType");
    //        var propTargetReference = property.FindPropertyRelative("TargetObjectReference");
    //        var propOnBegin = property.FindPropertyRelative("OnBegin");
    //        var propOnEnd = property.FindPropertyRelative("OnEnd");

    //        var gameObjectReference = GameObjectReference.FromProperty(propTargetReference);
    //        bool isVirtualCall = (gameObjectReference.Type != GameObjectReference.TypeEnum.Object && gameObjectReference.Type != GameObjectReference.TypeEnum.Self);

    //        GameObject self = null;
    //        if (property.serializedObject.targetObject is MonoBehaviour mb2)
    //            self = mb2.gameObject;
    //        gameObjectReference.TryGetTargetGameObject(EventParameters.WithoutTrigger(self, self), out var targetObject);

    //        if (isVirtualCall)
    //        {
    //            lbFoundBegin.text = "Virtual";
    //            lbFoundEnd.text = "Virtual";
    //        }
    //        else if (targetObject != null)
    //        {
    //            if (ReactionReference.HasReaction(targetObject, propOnBegin.stringValue))
    //                lbFoundBegin.text = "Found";
    //            else
    //                lbFoundBegin.text = "Missing";
    //            if (ReactionReference.HasReaction(targetObject, propOnEnd.stringValue))
    //                lbFoundEnd.text = "Found";
    //            else
    //                lbFoundEnd.text = "Missing";
    //        }
    //        else
    //        {
    //            lbFoundBegin.text = "No Target";
    //            lbFoundEnd.text = "No Target";
    //        }

    //    }
    //    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    //    {
    //        var propTargetReference = property.FindPropertyRelative("TargetObjectReference");
    //        var propOnBegin = property.FindPropertyRelative("OnBegin");
    //        var propOnEnd = property.FindPropertyRelative("OnEnd");

    //        var veVertical = new VisualElement();
    //        veVertical.style.flexDirection = FlexDirection.Column;

    //        var pfTargetReference = new PropertyField();
    //        pfTargetReference.BindProperty(propTargetReference);
    //        pfTargetReference.label = "";
    //        veVertical.Add(pfTargetReference);


    //        var veBeginRow = new VisualElement();
    //        veBeginRow.style.flexDirection = FlexDirection.Row;
    //        var lbBeginName = new Label("OnBegin:");
    //        var tfBeginName = new TextField();
    //        var lbBeginFound = new Label("");
    //        tfBeginName.style.flexGrow = 1;
    //        tfBeginName.value = propOnBegin.stringValue;
    //        veBeginRow.Add(lbBeginName);
    //        veBeginRow.Add(tfBeginName);
    //        veBeginRow.Add(lbBeginFound);
    //        veVertical.Add(veBeginRow);

    //        var veEndRow = new VisualElement();
    //        veEndRow.style.flexDirection = FlexDirection.Row;
    //        var lbEndName = new Label("OnEnd:");
    //        var tfEndName = new TextField();
    //        var lbEndFound = new Label("");
    //        tfEndName.style.flexGrow = 1;
    //        tfEndName.value = propOnEnd.stringValue;
    //        veEndRow.Add(lbEndName);
    //        veEndRow.Add(tfEndName);
    //        veEndRow.Add(lbEndFound);
    //        veVertical.Add(veEndRow);

    //        pfTargetReference.RegisterValueChangeCallback(x =>
    //        {
    //            Update(property, lbBeginFound, lbEndFound);
    //        });
    //        tfBeginName.RegisterCallback<ChangeEvent<string>>(x =>
    //        {
    //            propOnBegin.stringValue = x.newValue;
    //            propOnBegin.serializedObject.ApplyModifiedProperties();
    //            Update(property, lbBeginFound, lbEndFound);
    //        });
    //        tfEndName.RegisterCallback<ChangeEvent<string>>(x =>
    //        {
    //            propOnEnd.stringValue = x.newValue;
    //            propOnEnd.serializedObject.ApplyModifiedProperties();
    //            Update(property, lbBeginFound, lbEndFound);
    //        });
    //        Update(property, lbBeginFound, lbEndFound);
    //        return veVertical;
    //    }
    //    //public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    //    //{
    //    //    float h = 8;
    //    //    var propType = property.FindPropertyRelative("ReactionType");
    //    //    h += EditorGUI.GetPropertyHeight(propType);
    //    //    var type = (ReactionReference.Type)propType.enumValueIndex;
    //    //    propType.enumValueIndex = (int)type;
    //    //    switch (type)
    //    //    {
    //    //        case ReactionReference.Type.Reaction:
    //    //            // part of the Type line; h += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("TargetObjectReference"));
    //    //            h += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("ReactionName"));
    //    //            break;
    //    //        case ReactionReference.Type.Event:
    //    //            h += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("Event"));
    //    //            break;
    //    //    }
    //    //    return h;
    //    //}
    //    //public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    //    //{
    //    //    EditorGUI.BeginProperty(position, label, property);
    //    //    var layout = RectLayout.Horizontal(position);
    //    //    //layout.PrefixLabel(label);
    //    //    bool fixIndent = false;
    //    //    if (EditorGUI.indentLevel > 0)
    //    //    {
    //    //        EditorGUI.indentLevel--;
    //    //        fixIndent = true;
    //    //    }

    //    //    layout = layout.SubVertical();
    //    //    var layoutLine0 = layout.SubHorizontal(EditorGUIUtility.singleLineHeight);
    //    //    var propType = property.FindPropertyRelative("ReactionType");
    //    //    var type = layoutLine0.EnumPopup(RectLayout.WidthOf("ReactionOO") + 18, (ReactionReference.Type)propType.enumValueIndex);
    //    //    propType.enumValueIndex = (int)type;

    //    //    switch (type)
    //    //    {
    //    //        case ReactionReference.Type.Reaction:
    //    //            layoutLine0.Label("on");
    //    //            var propTargetReference = property.FindPropertyRelative("TargetObjectReference");

    //    //            layoutLine0.PropertyField(propTargetReference);

    //    //            //var t = (GameObjectReference.TypeEnum)propTargetReference_Type.intValue;
    //    //            //bool isVirtualCall = (t != GameObjectReference.TypeEnum.Object && t != GameObjectReference.TypeEnum.Self);
    //    //            var gameObjectReference = GameObjectReference.FromProperty(propTargetReference);//(GameObjectReference)propTargetReference.value;
    //    //            bool isVirtualCall = (gameObjectReference.Type != GameObjectReference.TypeEnum.Object && gameObjectReference.Type != GameObjectReference.TypeEnum.Self);

    //    //            GameObject self = null;
    //    //            if (property.serializedObject.targetObject is MonoBehaviour mb2)
    //    //                self = mb2.gameObject;
    //    //            gameObjectReference.TryGetTargetGameObject(new EventParameters()
    //    //            {
    //    //                Self = self,
    //    //                Current = EventParameters.ParameterSet.WithoutTrigger(self),
    //    //                OnBegin = EventParameters.ParameterSet.Default
    //    //            }, out var targetObject);



    //    //            //// if old values
    //    //            //var propTarget = property.FindPropertyRelative("ObjectTargetType");
    //    //            //if (propTarget != null)
    //    //            //{
    //    //            //    var propTargetReference_Type = propTargetReference.FindPropertyRelative("Type");
    //    //            //    var propTargetReference_ThisGameObject = propTargetReference.FindPropertyRelative("ThisGameObject");
    //    //            //    var propTargetReference_ObjectType = propTargetReference.FindPropertyRelative("ObjectType");

    //    //            //    var target = (ReactionReference.TargetType)propTarget.enumValueIndex;
    //    //            //    //target = layoutLine1.EnumPopup(RectLayout.WidthOf(target.ToString()) + 18, target);
    //    //            //    //propTarget.enumValueIndex = (int)target;
    //    //            //    //GameObject targetObject = null;
    //    //            //    //bool isVirtualCall = false;
    //    //            //    switch (target)
    //    //            //    {
    //    //            //        case ReactionReference.TargetType.Self:

    //    //            //            if (property.serializedObject.targetObject is MonoBehaviour mb)
    //    //            //                targetObject = mb.gameObject;


    //    //            //            propTargetReference_Type.intValue = (int)GameObjectReference.TypeEnum.Self;
    //    //            //            break;
    //    //            //        case ReactionReference.TargetType.Other:

    //    //            //            var propObject = property.FindPropertyRelative("TargetObject");
    //    //            //            targetObject = propObject.objectReferenceValue as GameObject;
    //    //            //            //layoutLine1.PropertyField(propObject);


    //    //            //            propTargetReference_Type.intValue = (int)GameObjectReference.TypeEnum.Object;
    //    //            //            propTargetReference_ThisGameObject.objectReferenceValue = propObject.objectReferenceValue;
    //    //            //            break;
    //    //            //        case ReactionReference.TargetType.TriggerObject:
    //    //            //            isVirtualCall = true;
    //    //            //            propTargetReference_Type.intValue = (int)GameObjectReference.TypeEnum.TriggerObject;
    //    //            //            break;
    //    //            //        case ReactionReference.TargetType.PreviousTriggerObject:

    //    //            //            isVirtualCall = true;
    //    //            //            propTargetReference_Type.intValue = (int)GameObjectReference.TypeEnum.PreviousTriggerObject;
    //    //            //            break;
    //    //            //    }


    //    //            //}
    //    //            if (targetObject != null || isVirtualCall)
    //    //            {
    //    //                var layoutLine1 = layout.SubHorizontal(EditorGUIUtility.singleLineHeight);
    //    //                layoutLine1.Label("Name:");
    //    //                var propStateName = property.FindPropertyRelative("ReactionName");
    //    //                layoutLine1.PropertyField(propStateName, -RectLayout.WidthOf("Missing"));
    //    //                string reactionName = propStateName.stringValue;

    //    //                if (isVirtualCall)
    //    //                {
    //    //                    layoutLine1.Label("");
    //    //                }
    //    //                else if (ReactionReference.HasReaction(targetObject, reactionName))
    //    //                {
    //    //                    layoutLine1.Label("Found");
    //    //                }
    //    //                else
    //    //                {
    //    //                    layoutLine1.Label("Missing");
    //    //                }

    //    //                //// if has old values
    //    //                //if (propTarget != null)
    //    //                //{
    //    //                //    var propTargetReference_String = propTargetReference.FindPropertyRelative("String");
    //    //                //    propTargetReference_String.stringValue = propStateName.stringValue;
    //    //                //}

    //    //            }

    //    //            break;
    //    //        case ReactionReference.Type.Event:
    //    //            layout.PropertyField(property.FindPropertyRelative("Event"));
    //    //            break;
    //    //    }

    //    //    if (fixIndent)
    //    //    {
    //    //        EditorGUI.indentLevel++;
    //    //    }

    //    //    EditorGUI.EndProperty();
    //    //}
    //}
}