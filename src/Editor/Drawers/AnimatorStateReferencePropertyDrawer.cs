using System.Linq;
using NiEngine;
using UnityEngine;
using UnityEditor;

namespace NiEditor
{
    [CustomPropertyDrawer(typeof(AnimatorStateReference))]
    public class AnimatorStateReferencePropertyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var propAnimator = property.FindPropertyRelative("Animator");
            if (propAnimator.objectReferenceValue is not null)
                return RectLayout.MinHeight * 2 + 4;
            return RectLayout.MinHeight + 4;
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            var layout = RectLayout.Horizontal(position);
            layout.PrefixLabel(label);
            layout = layout.SubVertical();
            var propAnimator = property.FindPropertyRelative("Animator");
            var animator = propAnimator.objectReferenceValue as Animator;
            layout.PropertyField(propAnimator);
            if (animator != null)
            {
                layout = layout.SubHorizontal();
                layout.Label("State:");
                var propStateName = property.FindPropertyRelative("State");
                layout.PropertyField(propStateName, -RectLayout.WidthOf("Missing"));

                var stateHash = Animator.StringToHash(propStateName.stringValue);
                if (animator.HasState(0, stateHash))
                {
                    property.FindPropertyRelative("StateHash").intValue = stateHash;
                    layout.Label("Found");
                }
                else
                {
                    layout.Label("Missing");
                }
            }

            EditorGUI.EndProperty();
        }
    }
}