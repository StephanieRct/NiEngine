using NiEngine.IO;
using NiEngine.IO.SaveOverrides;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;

namespace NiEngine.IO
{

    public class StringPrimitiveOutput : IOutput
    {
        public StringBuilder StringBuilder = new();
        private string CurrentIndent = "";
        public string Result => StringBuilder.ToString();
        private bool IsEmptyScope = true;
        private int InlineCount = 0;
        public bool IsSupportedType(Type type)
        {
            return type.IsPrimitive
                || type == typeof(string)
                || type == typeof(Enum)
                || type == typeof(Uid);
            //|| type == typeof(Guid);
        }
        void BeginLine()
        {
            StringBuilder.Append(CurrentIndent);
        }

        void EndLine()
        {
            StringBuilder.AppendLine();
        }

        void Append(string t)
        {
            StringBuilder.Append(t);
        }
        void AppendData(string t, object v)
        {
            StringBuilder.Append($"{t}:{{{v}}}");
        }
        void AppendData(string t, object v, int length)
        {
            StringBuilder.Append($"{t},{length}:{v}");
        }
        bool AppendData(StreamContext context, Type type, object value)
        {
            if (value == null && type == typeof(string))
            {
                AppendData("s", "", 0);
                return true;
            }
            switch (value)
            {
                case string s: AppendData("s", s, s.Length); break;
                case bool b: AppendData("b", b); break;
                case byte by: AppendData("by", by); break;
                case char c: AppendData("c", c); break;
                case short sh: AppendData("sh", sh); break;
                case ushort ush: AppendData("ush", ush); break;
                case int i: AppendData("i", i); break;
                case uint ui: AppendData("ui", ui); break;
                case long l: AppendData("l", l); break;
                case ulong ul: AppendData("ul", ul); break;
                case float f: AppendData("f", f); break;
                case double d: AppendData("d", d); break;
                case Enum e: AppendData("e", (int)value); break;
                case System.Guid guid: AppendData("guid", guid.ToString()); break;
                case Uid uid: AppendData("uid", uid.ToString()); break;
                default:
                    if (!context.IgnoreUnhandledTypes)
                        context.LogError($"{nameof(StringPrimitiveOutput)}: Cannot save object of type {value?.GetType()}");
                    return false;
            }
            return true;
        }

        bool AppendKey(StreamContext context, object key)
        {
            if (AppendData(context, key.GetType(), key))
            {
                Append(" = ");
                return true;
            }

            return false;
        }

        bool AppendKeyData(StreamContext context, object key, Type type, object value)
        {
            if (AppendKey(context, key))
            {
                AppendData(context, type, value);
                return true;
            }

            return false;
        }

        public void Save<T>(StreamContext context, object key, T value)
            => Save(context, key, typeof(T), value);
        public void Save(StreamContext context, object key, Type type, object value)
        {

            if (InlineCount <= 0)
            {
                EndLine();
                BeginLine();
            }
            AppendKeyData(context, key, type, value);
            IsEmptyScope = false;
        }

        public void SaveReference<T>(StreamContext context, object key, T o)
            where T : class
        {
            context.LogError($"{nameof(StringPrimitiveOutput)}: Does not support saving references");
        }
        public void SaveInPlace<T>(StreamContext context, object key, T value)
        {
            var type = value.GetType() ?? typeof(T);
            Save(context, key, type, value);
        }
        public void SaveInPlace(StreamContext context, object key, Type type, object value)
        {
            Save(context, key, type, value);
        }

        public bool ScopeBegin(StreamContext context, object key)
        {
            if (InlineCount <= 0)
            {
                EndLine();
                BeginLine();
            }
            ++InlineCount;
            if (AppendKey(context, key))
            {
                Append("{");
                --InlineCount;
                CurrentIndent += "  ";
                IsEmptyScope = true;
                return true;
            }

            --InlineCount;
            return false;
        }

        public void ScopeEnd(StreamContext context, object key)
        {
            CurrentIndent = CurrentIndent.Substring(0, CurrentIndent.Length - 2);
            if (IsEmptyScope)
                Append("}");
            else
            {
                EndLine();
                Append($"{CurrentIndent}}}");
            }
            IsEmptyScope = false;
        }

    }
}