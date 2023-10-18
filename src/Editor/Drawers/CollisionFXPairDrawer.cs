using System.Linq;
using NiEngine;
using UnityEngine;
using UnityEditor;

namespace NiEditor
{
    [CustomPropertyDrawer(typeof(CollisionFXPair))]
    public class CollisionFXPairPropertyDrawer : PropertyDrawer
    {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            var layout = new RectLayout(position);
            layout.Label("A:");
            layout.PropertyField(property.FindPropertyRelative("MaterialA"), 100);
            layout.Label("B:");
            layout.PropertyField(property.FindPropertyRelative("MaterialB"), 100);
            layout.Label("Sound:");
            var propSFX = property.FindPropertyRelative("Sound");
            layout.PropertyField(propSFX, 200);
            if (layout.Button("Play"))
            {
                if (propSFX.objectReferenceValue is SoundFX sfx)
                {
                    sfx.PlayInEditor();
                }
            }
            EditorGUI.EndProperty();
        }
    }
}