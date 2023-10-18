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
    
    [CustomPropertyDrawer(typeof(ObjectReferencePicker))]
    public class ObjectReferenceDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {

            if (attribute is not ObjectReferencePicker picker)
                return null;

            return new ObjectPicker(fieldInfo, picker, property);
        }

    }


}