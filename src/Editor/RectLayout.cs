using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;
using Unity.Mathematics;

namespace NiEditor
{
    /// <summary>
    /// GUI horizontal layout within a given Rect
    /// </summary>
    public struct RectLayout
    {
        public Rect OriginalRect;
        public Rect FreeRect;
        public bool IsHorizontal;
        public bool IsVertical => !IsHorizontal;
        public RectLayout(Rect rect, bool horizontal = true)
        {
            OriginalRect = rect;
            FreeRect = rect;
            IsHorizontal = horizontal;
        }
        static public RectLayout Horizontal(Rect rect) => new RectLayout(rect);
        static public RectLayout Vertical(Rect rect) => new RectLayout(rect, false);

        public RectLayout SubHorizontal(float size) => new RectLayout(AcquireSize(size), true);
        public RectLayout SubVertical(float size) => new RectLayout(AcquireSize(size), false);

        public RectLayout SubHorizontal() => new RectLayout(Acquire(FreeRect.width, FreeRect.height), true);
        public RectLayout SubHorizontalLine(int lineCount) => new RectLayout(Acquire(FreeRect.width, EditorGUIUtility.singleLineHeight * lineCount), true);
        public RectLayout SubVertical() => new RectLayout(Acquire(FreeRect.width, FreeRect.height), false);
        //public Rect Acquire(float size) => IsHorizontal ? Acquire(size, OriginalRect.height) : Acquire(OriginalRect.width, size);
        public static float WidthOf(string text) => GUI.skin.box.CalcSize(new GUIContent(text)).x;// text.Length * 8;//GUI.skin.box.CalcSize(new GUIContent(text)).x;
        public static float MinHeight => EditorGUIUtility.singleLineHeight + 2;// GUI.skin.box.CalcSize(new GUIContent("Fj")).y;
        public float FreeWidth => FreeRect.width;
        public float FreeHeight => FreeRect.width;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="width">if negative, will compute FreeWidth + with, which make the width right aligned</param>
        /// <param name="height">if negative, will compute FreeHeight + height, which make the height bottom aligned</param>
        /// <returns></returns>
        public Rect Acquire(float width, float height)
        {
            if (IsHorizontal)
            {
                if (width < 0)
                    width = FreeRect.width + width;
                Rect r = new Rect(FreeRect.xMin, OriginalRect.yMin, width, height);
                FreeRect.xMin += r.width;
                return r;
            }
            else
            {
                if (height < 0)
                    height = FreeRect.height + height;
                //height = math.max(height, MinHeight);
                Rect r = new Rect(OriginalRect.xMin, FreeRect.yMin, width, height);
                FreeRect.yMin += r.height;
                return r;
            }
        }
        public Rect AcquireSize(float size)
        {
            if (IsHorizontal)
            {
                return Acquire(size, FreeRect.height);
            }
            else
            {
                
                return Acquire(FreeRect.width, size);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="width">if negative, will compute FreeWidth + with, which make the width right aligned</param>
        /// <returns></returns>
        public Rect AcquireWidth(float width)
        {
            if (IsHorizontal)
            {
                return Acquire(width, FreeRect.height);
            }
            else
            {
                return Acquire(FreeRect.width, MinHeight);
            }
        }
        public Rect Acquire()
        {
            if (IsHorizontal)
            {
                return Acquire(FreeRect.width, FreeRect.height);
            }
            else
            {
                return Acquire(FreeRect.width, MinHeight);
            }
        }
        public Rect AcquireLines(int lineCount) => Acquire(FreeRect.width, EditorGUIUtility.singleLineHeight * lineCount);
        public Rect AcquireHeight(float height)
        {
            if (IsHorizontal)
            {
                return Acquire(FreeRect.width, FreeRect.height);
            }
            else
            {
                return Acquire(FreeRect.width, height);
            }
        }
        public bool Foldout(bool isExpanded, GUIContent content)
        {
            var size = CalcSize(new GUIContent(content.text));
            return EditorGUI.Foldout(AcquireHeight(size.y), isExpanded, content, true);
        }
        public bool Foldout(float width, bool isExpanded, GUIContent content)
        {
            var size = CalcSize(content);
            return EditorGUI.Foldout(Acquire(width, size.y), isExpanded, content, true);
        }
        public void Label(string text) => Label(new GUIContent(text));
        public void Label(GUIContent content)
        {
            var size = CalcSize(content);
            EditorGUI.LabelField(AcquireWidth(size.x), content);//, new GUIContent(text));
        }
        public void Box(string text)
        {
            GUI.Box(Acquire(), text);
        }
        public void Box(string text, float width)
        {
            GUI.Box(AcquireWidth(width), text);
        }

        public Rect PropertyField(SerializedProperty property, float width = 100)
        {
            var rect = AcquireWidth(width);
            EditorGUI.PropertyField(rect, property, GUIContent.none);
            return rect;

        }
        public Rect PropertyField(SerializedProperty property)=> PropertyField(property, GUIContent.none);
        public Rect PropertyField(SerializedProperty property, GUIContent content, bool includeChildren)
        {
            var rect = AcquireHeight(EditorGUI.GetPropertyHeight(property, includeChildren));
            EditorGUI.PropertyField(rect, property, content, includeChildren);
            return rect;
        }
        public Rect PropertyField(SerializedProperty property, GUIContent content)
        {
            var rect = AcquireHeight(EditorGUI.GetPropertyHeight(property));
            EditorGUI.PropertyField(rect, property, content);
            return rect;
        }

        public Rect PropertyFieldSquare(SerializedProperty property) => PropertyFieldSquare(property, GUIContent.none);
        public Rect PropertyFieldSquare(SerializedProperty property, GUIContent content)
        {
            var size = EditorGUI.GetPropertyHeight(property);
            var rect = Acquire(size, size);
            EditorGUI.PropertyField(rect, property, content, true);
            return rect;
        }
        public bool Button(string caption)
        {
            var content = new GUIContent(caption);
            var size = CalcSize(content);
            return GUI.Button(Acquire(size.x + 4, math.min(size.y + 4, OriginalRect.height)), content);
        }
        Vector2 CalcSize(GUIContent content)
        {
            Vector2 size = new();

            size.x = WidthOf(content.text);
            if (content.image != null)
                size.x += 24;
            size.y = MinHeight;
            //var size = GUI.skin.box.CalcSize(content);
            return size;
        }
        public void PrefixLabel(GUIContent label)
        {
            var size = CalcSize(label);
            var r = EditorGUI.PrefixLabel(Acquire(size.x, size.y), label);
            FreeRect.xMin = r.xMin;
            OriginalRect = FreeRect;
        }
        public TEnum EnumPopup<TEnum>(float minWidth, TEnum value)
            where TEnum : System.Enum
        {
            return (TEnum)EditorGUI.EnumPopup(AcquireWidth(minWidth), value);
        }


    }

}