using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace NiEditor
{
    public static class VisualElementExt
    {
        public static void FixSize(this VisualElement ve, float w, float h)
        {
            ve.style.width = w;
            ve.style.minWidth = w;
            ve.style.maxWidth = w;
            ve.style.height = h;
            ve.style.minHeight = h;
            ve.style.maxHeight = h;
            ve.style.flexGrow = 0;
            ve.style.flexShrink = 0;
        }
        public static void FixSizeByLines(this VisualElement ve, float w, int lines)
        {
            ve.style.width = w;
            ve.style.minWidth = w;
            ve.style.maxWidth = w;
            ve.style.height = lines * 18;
            ve.style.minHeight = lines * 18;
            ve.style.maxHeight = lines * 18;
            ve.style.flexGrow = 0;
            ve.style.flexShrink = 0;
        }
        public static void FixSizeByCharLines(this VisualElement ve, int charCount, int lines)
        {
            ve.style.width = charCount * 5;
            ve.style.minWidth = charCount * 5;
            ve.style.maxWidth = charCount * 5;
            ve.style.height = lines * 18;
            ve.style.minHeight = lines * 18;
            ve.style.maxHeight = lines * 18;
            ve.style.flexGrow = 0;
            ve.style.flexShrink = 0;
        }
        public static void MinWidthByCharLines(this VisualElement ve, int charCount)
        {
            ve.style.minWidth = charCount * 5;
        }
        public static void FixHeightFlexWidth(this VisualElement ve, int lines)
        {
            ve.style.height = lines * 18;
            ve.style.minHeight = lines * 18;
            ve.style.maxHeight = lines * 18;
            ve.style.flexGrow = 1;
            ve.style.flexShrink = 1;
        }
        public static void FixHeightLines(this VisualElement ve, int lines, float plus =0)
        {
            ve.style.height = lines * 18 + plus;
            ve.style.minHeight = lines * 18 + plus;
            ve.style.maxHeight = lines * 18 + plus;
        }
        public static void RemoveAll(this VisualElement ve)
        {
            while (ve.childCount > 0)
                ve.RemoveAt(ve.childCount - 1);
        }
        public static void TransferAllFrom(this VisualElement destination, VisualElement source)
        {
            for(int i = source.childCount-1; i >=0; --i)
            {
                destination.Add(source[i]);
                source.RemoveAt(i);
            }
        }


    }
}
