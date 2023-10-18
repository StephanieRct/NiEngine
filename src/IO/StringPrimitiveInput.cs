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

    public class StringData
    {
        public string Data = "";
        public Scope Root;

        public bool Parse(StreamContext context, string data)
        {
            Data = data;
            Root = new Scope(parent: null);

            RangeInt nextRange = new RangeInt(0, Data.Length);
            if (!Root.Parse(context, this, 0))
                return false;
            return true;
        }
        public class Scope
        {
            public object Key;
            public object Value;

            public Scope Parent;
            public StringData Data;
            public RangeInt Range;
            public List<Scope> Children = new();

            public Scope(Scope parent)
            {
                Parent = parent;
            }
            public Scope()
            {
                Parent = null;
            }
            public override string ToString()
            {
                return $"{(Parent == null ? "Root-" : "")}{Key}({Children.Count}){(Value != null ? "+" + Value.GetType().FullName : "")}";
            }
            public Scope GetChild(object key)
            {
                foreach (var c in Children)
                {
                    if (c.Key.Equals(key))
                        return c;
                }

                return null;
            }
            public bool TryGetChild(object key, out Scope child)
            {
                foreach (var c in Children)
                {
                    if (c.Key.Equals(key))
                    {
                        child = c;
                        return true;
                    }
                }

                child = default;
                return false;
            }
            // Parse until hit end of stream or hit a '}'
            // Final Range will exclude the trailing '}' is exist
            public bool Parse(StreamContext context, StringData data, int cursor)
            {
                Data = data;
                Range = new RangeInt();
                Range.start = cursor;


                Parser parser = new(context, Data, cursor);
                int maxLoop = 10000000;
                if (!parser.SkipSpaces()) return false;
                while (!parser.IsScopeCloseOrEnd && maxLoop > 0)
                {
                    if (!parser.ParseNext(out var object0)) return false;
                    if (!parser.SkipSpaces(expectMore: false)) return false;
                    // can be any of:
                    //  key = value
                    //  key = { ... }
                    //  value
                    //  { ... }
                    if (parser.TryParseChar('='))
                    {
                        // One of:
                        //  key = value
                        //  key = { ... }
                        if (!parser.SkipSpaces()) return false;
                        if (!parser.ParseNext(out var object1)) return false;
                        var kvScope = new Scope(parent: this);
                        kvScope.Key = object0;
                        kvScope.Value = object1;
                        if (object0 is Scope scope0)
                            scope0.Parent = this;
                        if (object1 is Scope scope1)
                        {
                            scope1.Parent = this;
                            kvScope.Children.AddRange(scope1.Children);
                        }
                        Children.Add(kvScope);
                    }
                    else //else if (parser.IsEnd || parser.IsScopeClose())
                    {
                        // One of:
                        //  value
                        //  { ... }
                        var vScope = new Scope(parent: this);
                        vScope.Key = null;
                        vScope.Value = object0;
                        if (object0 is Scope scope0)
                            scope0.Parent = this;
                        else if (Value == null)
                            Value = object0;
                        Children.Add(vScope);




                        //Range.length = parser.Cursor - Range.start;
                        //return true;
                    }
                    //else if (parser.IsEnd || parser.IsScopeClose())
                    //{
                    //    // One of:
                    //    //  value
                    //    //  { ... }

                    //    if (object0 is Scope object0Scope)
                    //    {
                    //        object0Scope.Parent = this;
                    //        Children.Add(object0Scope);
                    //    }
                    //    else
                    //    {
                    //        Value = object0;
                    //    }
                    //    Range.length = parser.Cursor - Range.start;
                    //    return true;
                    //}
                    if (!parser.SkipSpaces(expectMore: false)) return false;
                    --maxLoop;
                }

                Range.length = parser.Cursor - Range.start;
                if (maxLoop <= 0)
                {
                    context.LogError($"{nameof(StringData)}.{nameof(Parser)}.{nameof(Parse)}: Infinite loop detected");
                    return false;
                }
                return true;
            }


        }

        public struct Parser
        {
            public StreamContext Context;
            public StringData StringData;
            public string Data => StringData.Data;
            //public RangeInt Range;
            public int Cursor;
            private int CursorMax;
            public RangeInt UnparsedRange => new RangeInt(Cursor, CursorMax - Cursor);

            public Parser(StreamContext context, StringData stringData, int cursor)
            {
                Context = context;
                StringData = stringData;
                Cursor = cursor;
                CursorMax = stringData.Data.Length;

            }

            public bool IsEnd => Cursor >= CursorMax;
            public bool IsScopeCloseOrEnd => IsEnd || Data[Cursor] == '}';
            public bool IsScopeOpen()
            {
                if (IsEnd)
                {
                    Context.LogError($"{nameof(StringData)}.{nameof(Parser)}.{nameof(IsScopeOpen)}: Unexpected end of string");
                    return false;
                }

                return Data[Cursor] == '{';
            }
            public bool IsScopeClose()
            {
                if (IsEnd)
                {
                    Context.LogError($"{nameof(StringData)}.{nameof(Parser)}.{nameof(IsScopeClose)}: Unexpected end of string");
                    return false;
                }

                return Data[Cursor] == '}';
            }

            public bool SkipSpaces(bool expectMore = true)
            {
                while (Cursor < CursorMax &&
                       (Data[Cursor] == ' '
                       || Data[Cursor] == '\t'
                       || Data[Cursor] == '\n'
                       || Data[Cursor] == '\r'))
                {
                    ++Cursor;
                }

                if (IsEnd && expectMore)
                {
                    Context.LogError($"{nameof(StringData)}.{nameof(Parser)}.{nameof(SkipSpaces)}: Unexpected end of string");
                    return false;
                }
                return true;
            }
            public static int SkipSpaces(string data, int start, int end)
            {
                while (start < end &&
                       (data[start] == ' '
                        || data[start] == '\t'
                        || data[start] == '\n'
                        || data[start] == '\r'))
                {
                    ++start;
                }
                return start;
            }
            public bool ParseChar(char c)
            {
                if (IsEnd)
                {
                    Context.LogError($"{nameof(StringData)}.{nameof(Parser)}.{nameof(ParseChar)}: Unexpected end of string");
                    return false;
                }

                if (Data[Cursor] != c)
                {
                    Context.LogError($"{nameof(StringData)}.{nameof(Parser)}.{nameof(ParseChar)}: expecting '{c}' at {Cursor} but is '{Data[Cursor]}'");
                    return false;
                }

                ++Cursor;
                return true;
            }
            public bool TryParseChar(char c)
            {
                if (IsEnd)
                {
                    return false;
                }

                if (Data[Cursor] != c)
                {
                    return false;
                }

                ++Cursor;
                return true;
            }

            public bool ParseNext(out object result)
            {
                result = null;
                if (!SkipSpaces(expectMore: false)) return false;
                if (IsScopeCloseOrEnd) return true;

                // check if scope
                if (TryParseChar('{'))
                {
                    Scope scope = new Scope();
                    if (!scope.Parse(Context, StringData, Cursor)) return false;
                    Cursor = scope.Range.end;
                    if (!ParseChar('}')) return false;
                    result = scope;
                    return true;
                }
                else
                {
                    if (!ParseData(out result, out var dataRange)) return false;
                    return true;
                }
            }

            public bool FindScopeRange(out RangeInt result)
            {
                if (IsEnd)
                {
                    Context.LogError($"{nameof(StringData)}.{nameof(Parser)}.{nameof(FindScopeRange)}: Unexpected end of string");
                    result = new RangeInt(0, 0);
                    return false;
                }

                if (Data[Cursor] != '{')
                {
                    Context.LogError($"{nameof(StringData)}.{nameof(Parser)}.{nameof(FindScopeRange)}: expecting '{{' at {Cursor} while parsing scope but found '{Data[Cursor]}'");
                    result = new RangeInt(0, 0);
                    return false;
                }

                var start = Cursor + 1;
                var c = start;

                // start scopes at 1 to include the first expected '{'
                int scope = 1;
                while (c < CursorMax)
                {
                    if (Data[c] == '{')
                        ++scope;
                    else if (Data[c] == '}')
                    {
                        --scope;
                        if (scope <= 0)
                            break;
                    }
                    ++c;
                }
                if (c >= CursorMax)
                {
                    Context.LogError($"{nameof(StringData)}.{nameof(Parser)}.{nameof(FindScopeRange)}: Unexpected end of string");
                    result = new RangeInt(0, 0);
                    return false;
                }

                // length is - 1 to remove the last '}' from the scope
                result = new RangeInt(start, c - start);
                //// Set cursor c + 1 to exclude the last '}' from the scope
                //Cursor = c + 1;
                return true;
            }

            public bool ParseData(out object data, out RangeInt fullRange)
            {
                data = default;

                fullRange = new RangeInt();
                fullRange.start = Cursor;
                RangeInt valueRange;
                if (!ParseType(out var t, out var len)) return false;
                var scopedValueStart = Cursor;
                if (len < 0)
                {
                    //length is dictated by a pair of matching '{' and '}'
                    if (!SkipSpaces()) return false;
                    if (!FindScopeRange(out valueRange)) return false;
                    var scopedValueEnd = valueRange.end + 1; // +1 to include the trailing '}'
                    len = scopedValueEnd - scopedValueStart;
                    Cursor = scopedValueEnd;
                }
                else
                {
                    valueRange = new RangeInt(Cursor, len);
                    Cursor += len;
                }

                fullRange.length = Cursor - fullRange.start;
                var spanValue = Data.AsSpan(valueRange.start, valueRange.length);
                switch (t)
                {
                    case "s":
                        data = spanValue.ToString();// Data.Substring(rangeValue.start, rangeValue.length);
                        return true;
                    case "b":
                        if (bool.TryParse(spanValue, out var b))
                        {
                            data = b;
                            return true;
                        }
                        Context.LogError($"{nameof(StringData)}.{nameof(Parser)}.{nameof(ParseData)}: Could not parse bool value '{spanValue.ToString()}' at {valueRange.start}");
                        data = default;
                        return false;
                    case "by":
                        if (byte.TryParse(spanValue, out var by))
                        {
                            data = by;
                            return true;
                        }
                        Context.LogError($"{nameof(StringData)}.{nameof(Parser)}.{nameof(ParseData)}: Could not parse byte value '{spanValue.ToString()}' at {valueRange.start}");
                        data = default;
                        return false;
                    case "c":
                        if (spanValue.Length != 1)
                        {
                            Context.LogError($"{nameof(StringData)}.{nameof(Parser)}.{nameof(ParseData)}: Could not parse bool value '{spanValue.ToString()}' at {valueRange.start}");
                            data = default;
                            return false;
                        }
                        data = Data[valueRange.start];
                        return true;
                    case "sh":
                        if (short.TryParse(spanValue, out var sh))
                        {
                            data = sh;
                            return true;
                        }
                        Context.LogError($"{nameof(StringData)}.{nameof(Parser)}.{nameof(ParseData)}: Could not parse short value '{spanValue.ToString()}' at {valueRange.start}");
                        data = default;
                        return false;
                    case "ush":
                        if (ushort.TryParse(spanValue, out var ush))
                        {
                            data = ush;
                            return true;
                        }
                        Context.LogError($"{nameof(StringData)}.{nameof(Parser)}.{nameof(ParseData)}: Could not parse ushort value '{spanValue.ToString()}' at {valueRange.start}");
                        data = default;
                        return false;
                    case "i":
                        if (int.TryParse(spanValue, out var i))
                        {
                            data = i;
                            return true;
                        }
                        Context.LogError($"{nameof(StringData)}.{nameof(Parser)}.{nameof(ParseData)}: Could not parse int value '{spanValue.ToString()}' at {valueRange.start}");
                        data = default;
                        return false;
                    case "ui":
                        if (uint.TryParse(spanValue, out var ui))
                        {
                            data = ui;
                            return true;
                        }
                        Context.LogError($"{nameof(StringData)}.{nameof(Parser)}.{nameof(ParseData)}: Could not parse uint value '{spanValue.ToString()}' at {valueRange.start}");
                        data = default;
                        return false;
                    case "l":
                        if (long.TryParse(spanValue, out var l))
                        {
                            data = l;
                            return true;
                        }
                        Context.LogError($"{nameof(StringData)}.{nameof(Parser)}.{nameof(ParseData)}: Could not parse long value '{spanValue.ToString()}' at {valueRange.start}");
                        data = default;
                        return false;
                    case "ul":
                        if (ulong.TryParse(spanValue, out var ul))
                        {
                            data = ul;
                            return true;
                        }
                        Context.LogError($"{nameof(StringData)}.{nameof(Parser)}.{nameof(ParseData)}: Could not parse ulong value '{spanValue.ToString()}' at {valueRange.start}");
                        data = default;
                        return false;
                    case "f":
                        if (float.TryParse(spanValue, out var f))
                        {
                            data = f;
                            return true;
                        }
                        Context.LogError($"{nameof(StringData)}.{nameof(Parser)}.{nameof(ParseData)}: Could not parse float value '{spanValue.ToString()}' at {valueRange.start}");
                        data = default;
                        return false;
                    case "d":
                        if (double.TryParse(spanValue, out var d))
                        {
                            data = d;
                            return true;
                        }
                        Context.LogError($"{nameof(StringData)}.{nameof(Parser)}.{nameof(ParseData)}: Could not parse double value '{spanValue.ToString()}' at {valueRange.start}");
                        data = default;
                        return false;
                    case "e":
                        if (int.TryParse(spanValue, out var e))
                        {
                            data = e;
                            return true;
                        }
                        Context.LogError($"{nameof(StringData)}.{nameof(Parser)}.{nameof(ParseData)}: Could not parse enum value '{spanValue.ToString()}' at {valueRange.start}");
                        data = default;
                        return false;
                    case "guid":
                        if (Guid.TryParse(spanValue, out var guid))
                        {
                            data = guid;
                            return true;
                        }
                        Context.LogError($"{nameof(StringData)}.{nameof(Parser)}.{nameof(ParseData)}: Could not parse Guid value '{spanValue.ToString()}' at {valueRange.start}");
                        data = default;
                        return false;
                    case "uid":
                        if (Uid.TryParse(spanValue, out var uid))
                        {
                            data = uid;
                            return true;
                        }
                        Context.LogError($"{nameof(StringData)}.{nameof(Parser)}.{nameof(ParseData)}: Could not parse Uid value '{spanValue.ToString()}' at {valueRange.start}");
                        data = default;
                        return false;
                    default:
                        Context.LogError($"{nameof(StringData)}.{nameof(Parser)}: Could not parse value of unknown type '{t}'. Value: '{spanValue.ToString()}' at {valueRange.start}");
                        data = default;
                        return false;
                }
            }

            // Advance Cursor after the trailing ':'
            public bool ParseType(out string type, out int length)
            {
                var c = Cursor;
                var cLength = -1;
                while (c < CursorMax
                       && Data[c] != ' '
                       && Data[c] != '\t'
                       && Data[c] != '\n'
                       && Data[c] != '\r'
                       && Data[c] != ':')
                {
                    if (Data[c] == ',')
                        cLength = c;
                    ++c;
                }

                if (c >= CursorMax)
                {
                    Context.LogError($"{nameof(StringData)}.{nameof(Parser)}.{nameof(ParseType)}: Unexpected end of string");
                    type = default;
                    length = -1;
                    return false;
                }
                if (cLength >= 0)
                {
                    type = Data.Substring(Cursor, cLength - Cursor);
                    ++cLength; // skip the heading ','
                    var spanLength = Data.AsSpan(cLength, c - cLength);
                    if (!int.TryParse(spanLength, out length))
                    {
                        Context.LogError($"{nameof(StringData)}.{nameof(Parser)}.{nameof(ParseType)}: could not parse length '{spanLength.ToString()}' at {cLength}");
                        type = default;
                        length = -1;
                        return false;
                    }
                }
                else
                {
                    type = Data.Substring(Cursor, c - Cursor);
                    length = -1;
                }
                // +1 to skip the trailing ':'
                Cursor = c + 1;
                return true;
            }
        }
    }

    public class StringPrimitiveInput : IInput
    {
        public StringData StringData;
        public StringData.Scope CurrentScope;

        public StringPrimitiveInput(StringData data)
        {
            StringData = data;
            CurrentScope = data.Root;
        }

        public StringPrimitiveInput(StreamContext context, string data)
        {
            StringData = new StringData();
            StringData.Parse(context, data);
            CurrentScope = StringData.Root;
        }
        public bool IsSupportedType(Type type)
        {
            return type.IsPrimitive
                || type == typeof(string)
                || type == typeof(Uid)
                || type == typeof(Enum);
            //|| type == typeof(Guid)
        }

        public IEnumerable<object> Keys
        {
            get
            {
                foreach (var s in CurrentScope.Children)
                {
                    yield return s.Key;
                }
            }
        }
        public T Load<T>(StreamContext context, object key)
        {
            if (CurrentScope.TryGetChild(key, out var child))
            {
                return (T)child.Value;
            }
            context.LogError($"{nameof(StringPrimitiveInput)}.{nameof(Load)}: Could not find child with key '{key}'");
            return default;
        }
        public object Load(StreamContext context, object key, Type type)
        {
            if (CurrentScope.TryGetChild(key, out var child))
            {
                return child.Value;
            }
            context.LogError($"{nameof(StringPrimitiveInput)}.{nameof(Load)}: Could not find child with key '{key}'");
            return default;
        }

        public bool TryLoad<T>(StreamContext context, object key, out T obj)
        {
            if (CurrentScope.TryGetChild(key, out var child))
            {
                obj = (T)child.Value;
                return true;
            }
            obj = default;
            return false;
        }
        public bool TryLoad(StreamContext context, object key, Type type, out object obj)
        {
            if (CurrentScope.TryGetChild(key, out var child))
            {
                obj = child.Value;
                return true;
            }
            obj = default;
            return false;
        }




        public void LoadInPlace<T>(StreamContext context, object key, ref T target)
        {
            if (!TryLoadInPlace(context, key, ref target))
                context.LogError($"{nameof(StringPrimitiveInput)}.{nameof(LoadInPlace)}<{typeof(T).Name}>: Failed");
        }
        public void LoadInPlace(StreamContext context, object key, Type type, ref object target)
        {
            if (!TryLoadInPlace(context, key, ref target))
                context.LogError($"{nameof(StringPrimitiveInput)}.{nameof(LoadInPlace)}: Failed");
        }

        public bool TryLoadInPlace<T>(StreamContext context, object key, ref T obj)
        {
            if (TryLoad<T>(context, key, out var obj2))
            {
                obj = obj2;
                return true;
            }
            //context.LogError($"{nameof(StringPrimitiveInput)}.{nameof(TryLoadInPlace)}<{typeof(T).Name}>: Not supported");
            return false;
        }
        public bool TryLoadInPlace(StreamContext context, object key, Type type, ref object target)
        {
            if (TryLoad(context, key, out object obj2))
            {
                target = obj2;
                return true;
            }
            //context.LogError($"{nameof(StringPrimitiveInput)}.{nameof(TryLoadInPlace)}: Not supported");
            return false;
        }

        //public T LoadReference<T>(StreamContext context, object key)
        //    where T : class
        //{
        //    context.LogError($"{nameof(StringPrimitiveInput)}.{nameof(LoadReference)}: Not supported");
        //    //throw new NotImplementedException();
        //    return default;
        //}

        public bool ScopeBegin(StreamContext context, object key)
        {
            if (CurrentScope.TryGetChild(key, out var child))
            {
                CurrentScope = child;
                return true;
            }

            return false;
        }

        public void ScopeEnd(StreamContext context, object key)
        {
            CurrentScope = CurrentScope.Parent;
        }
    }
}