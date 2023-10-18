//using NiEngine.IO.SaveOverrides;
//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Reflection;
//using System.Text;
//using UnityEngine;

///*

//string format:
//    Syntax:
//    <x>: inserted data called x
//    []: empty statement
//    [x]: statement of type x
//    [...]: multiple instance of last statement may be present
//    x | y: statement x or statement y

//    format:
//    [Data]: <t>,<l>:<data of length l>
//          | <t>:{<data of variable length>}
//    [KeyValue]: k,<l>:<key string> = [Value]
//    [Scope]: { [block] }

//    [Value]: [Data] | [Scope]
//    [Expression]: [Value] | [KeyValue] | [Scope]
//    [Block]: [] | [Expression][...]
    


//Object:
//    k,5:~type = s,?:typename
//    k,3:~id = s,?:id
//    k,5:~ctor = {
//        i:{31}
//        id,?:{myid}
//    }
    
    

//*/
//public class Parser
//{

//}
//namespace NiEngine.IOold
//{

//    public class StringData
//    {
//        public string Data = "";
//        public Scope Root;

//        public bool Parse(SaveContext context, string data)
//        {
//            Data = data;
//            Root = new Scope(parent: null);

//            RangeInt nextRange = new RangeInt(0, Data.Length);
//            if (!Root.Parse(context, this, 0))
//                return false;
//            //Scope child;
//            //do
//            //{
//            //    child = new Scope(parent: Root);
//            //    Root.Children.Add(child);
//            //    if (!child.Parse(context, this, nextRange))
//            //        return false;
//            //    nextRange = new RangeInt(child.Range.end, Data.Length - child.Range.end);
//            //    var nextStart = Parser.SkipSpaces(Data, nextRange.start, nextRange.end);
//            //    if (nextStart >= nextRange.end)
//            //        break;
//            //    nextRange = new RangeInt(nextStart, Data.Length - nextStart);
//            //} while (child.Range.end < Data.Length);



//            //    if (Root.Range.end < Data.Length)
//            //{
//            //    // There's more than one root.
//            //    var child = Root;
//            //    Root = new Scope(parent: null);
//            //    Root.Children.Add(child);
//            //    child.Parent = Root;
//            //    while (child.Range.end < Data.Length)
//            //    {
//            //        var previousChild = child;
//            //        child = new Scope(parent: Root);
//            //        var nextRange = new RangeInt(previousChild.Range.end, Data.Length - previousChild.Range.end);
//            //        var nextStart = Parser.SkipSpaces(Data, nextRange.start, nextRange.end);
//            //        if (nextStart >= nextRange.end)
//            //            break;
//            //        if (!child.Parse(context, this, new RangeInt(nextStart, Data.Length - nextStart)))
//            //            return false;
//            //        Root.Children.Add(child);

//            //    }
//            //}

//            return true;
//        }
//        public class Scope //: IComparable<Scope>, IComparable
//        {
//            public object Key; // can be a value or a scope
//            public object Value;
//            public Scope Parent;
//            public StringData Data;
//            public RangeInt Range;
//            public RangeInt ValueRange;
//            public List<Scope> Children = new();

//            public Scope(Scope parent)
//            {
//                Parent = parent;
//            }
//            public Scope()
//            {
//                Parent = null;
//            }
//            public override string ToString()
//            {
//                return $"{Key}({Children.Count}){(Value != null ? "+" + Value.GetType().FullName : "")}";
//            }
//            // range can be pass the actual scope, it will be fixed up into the Range field
//            //public Scope(SaveContext context, StringData data, RangeInt range)
//            //{
//            //    Parse(context, data, range);
//            //}

//            //public static int Compare(Scope a, Scope b)
//            //{
//            //    if (b == null) return -1;
//            //    if (a == null) return 1;
//            //    if (a.Key is IComparable aKeyC)
//            //    {
//            //        return aKeyC.CompareTo(b.Key);
//            //    }
//            //    if (b.Key is IComparable bKeyC)
//            //    {
//            //        return 1 - bKeyC.CompareTo(a.Key);
//            //    }

//            //    throw new InvalidOperationException($"{typeof(Scope).FullName} : At least one key must be IComparable");
//            //}

//            //public int CompareTo(Scope other)
//            //    => Compare(this, other);
//            //public int CompareTo(object other)
//            //    => Compare(this, (Scope)other);

//            public Scope GetChild(object key)
//            {
//                foreach (var c in Children)
//                {
//                    if (c.Key.Equals(key))
//                        return c;
//                }

//                return null;
//            }
//            //bool ParseBase(SaveContext context, StringData data, RangeInt range)
//            //{
//            //    Parser parser = new(context, data, range);
//            //    if (!parser.SkipSpaces()) return false;
//            //    if (parser.IsScopeOpen())
//            //    {
//            //        if (!parser.ParseScope(out var keyRange)) return false;
//            //        if (!parser.SkipSpaces()) return false;
//            //        if (!parser.ParseChar('=')) return false;
//            //        if (!parser.SkipSpaces()) return false;
//            //        if (!parser.ParseScope(out var valueRange)) return false;
//            //        Parser keyParser = new(context, data, keyRange);
//            //        if (!keyParser.ParseData(out Key, out var dataRange)) return false;
//            //        //Key = Data.Data.Substring(keyRange.start, keyRange.length);
//            //        ValueRange = valueRange;

//            //        // fix up our range, + 1 to include the trailing '}'
//            //        Range.length = ValueRange.end - Range.start + 1;
//            //        return true;
//            //    }
//            //    else
//            //    {
//            //        if (!parser.ParseData(out var dataParsed, out var dataRange)) return false;
//            //        ValueRange = dataRange;
//            //        Value = dataParsed;
//            //        Range.length = dataRange.end - Range.start;
//            //        return true;
//            //    }
//            //}

//            //bool ParseValueAndChildren(SaveContext context)
//            //{
//            //    // value is:
//            //    //  a single data node that becomes the Value object.
//            //    //  multiple key-value scopes that are added to Children
//            //    Parser valueParser = new(context, Data, ValueRange);
//            //    while (!valueParser.IsEnd)
//            //    {
//            //        if (!valueParser.SkipSpaces(expectMore: false)) return false;
//            //        if (valueParser.IsEnd)
//            //            break;
//            //        if (valueParser.IsScopeOpen())
//            //        {
//            //            // children
//            //            var child = new Scope(parent: this);
//            //            if (!child.Parse(context, Data, valueParser.UnparsedRange)) return false;
//            //            Children.Add(child);
//            //            valueParser.Cursor = child.Range.end;
//            //        }
//            //        else
//            //        {
//            //            // Value
//            //            if (!valueParser.ParseData(out var valueData, out var _)) return false;
//            //            Value = valueData;
//            //        }
//            //    }

//            //    return true;
//            //}
//            // Parse until hit end of stream or hit a '}'
//            // Final Range will exclude the trailing '}' is exist
//            public bool Parse(SaveContext context, StringData data, int cursor)
//            {
//                Data = data;
//                Range = new RangeInt();
//                Range.start = cursor;


//                Parser parser = new(context, Data, cursor);
//                int maxLoop = 10000000;
//                while (!parser.IsScopeCloseOrEnd && maxLoop > 0)
//                {
//                    if (!parser.SkipSpaces()) return false;
//                    if (!parser.ParseNext(out var object0)) return false;
//                    if (!parser.SkipSpaces(expectMore: false)) return false;
//                    if (parser.IsEnd || parser.IsScopeClose())
//                    {
//                        if (object0 is Scope object0Scope)
//                        {
//                            object0Scope.Parent = this;
//                            Children.Add(object0Scope);
//                        }
//                        else
//                        {
//                            Value = object0;
//                        }
//                        Range.length = parser.Cursor - Range.start;
//                        return true;
//                    }
//                    if (parser.TryParseChar('='))
//                    {
//                        // scope0/value0 is a key
//                        if (!parser.SkipSpaces()) return false;
//                        if (!parser.ParseNext(out var object1)) return false;
//                        if (object1 is Scope object1Scope)
//                        {
//                            object1Scope.Key = object0;
//                            object1Scope.Parent = this;
//                            Children.Add(object1Scope);
//                        }
//                        else
//                        {
//                            var kvScope = new Scope(parent: this);
//                            kvScope.Key = object0;
//                            kvScope.Value = object1;
//                            Children.Add(kvScope);
//                        }
//                    }
//                    --maxLoop;
//                }
//                if(maxLoop <= 0)
//                {
//                    context.LogError($"{nameof(StringData)}.{nameof(Parser)}.{nameof(Parse)}: Infinite loop detected");
//                    return false;
//                }
//                return true;

//                //if (parser.TryParseChar('{'))
//                //{
//                //    Scope scope0 = new Scope(parent: this);
//                //    scope0.Parse(context, Data, parser.UnparsedRange);
//                //    parser.Cursor = scope0.Range.end;

//                //    if (!parser.ParseChar('}')) return false;

//                //    if (!parser.SkipSpaces()) return false;
//                //    if (parser.TryParseChar('='))
//                //    {
//                //        // scope0 is a key
//                //        if (!parser.SkipSpaces()) return false;

//                //    }

//                //    //if (!parser.ParseScope(out var keyRange)) return false;
//                //    //if (!parser.SkipSpaces()) return false;
//                //    //if (!parser.ParseChar('=')) return false;
//                //    //if (!parser.SkipSpaces()) return false;
//                //    //if (!parser.ParseScope(out var valueRange)) return false;
//                //    //Parser keyParser = new(context, data, keyRange);
//                //    //if (!keyParser.ParseData(out Key, out var dataRange)) return false;
//                //    ////Key = Data.Data.Substring(keyRange.start, keyRange.length);
//                //    //ValueRange = valueRange;

//                //    //// fix up our range, + 1 to include the trailing '}'
//                //    //Range.length = ValueRange.end - Range.start + 1;
//                //    ////if (!ParseValueAndChildren(context)) return false;
//                //    ////return true;
//                //}
//                //else
//                //{
//                //    ValueRange = range;
//                //    Range = ValueRange;
//                //    //if (!ParseValueAndChildren(context)) return false;

//                //    ////if (!parser.ParseData(out var dataParsed, out var dataRange)) return false;
//                //    ////ValueRange = dataRange;
//                //    ////Value = dataParsed;
//                //    ////Range.length = dataRange.end - Range.start;
//                //    //return true;
//                //}
//                //if (!ParseValueAndChildren(context)) return false;
//                //return true;

//                ////if (!ParseBase(context, data, range)) return false;
//                ////if (!ParseValueAndChildren(context, data)) return false;
//                ////return true;
//            }


//        }

//        public struct Parser
//        {
//            public SaveContext Context;
//            public StringData StringData;
//            public string Data => StringData.Data;
//            //public RangeInt Range;
//            public int Cursor;
//            private int CursorMax;
//            public RangeInt UnparsedRange => new RangeInt(Cursor, CursorMax - Cursor);

//            public Parser(SaveContext context, StringData stringData, int cursor)
//            {
//                Context = context;
//                StringData = stringData;
//                Cursor = cursor;
//                CursorMax = stringData.Data.Length;

//            }

//            public bool IsEnd => Cursor >= CursorMax;
//            public bool IsScopeCloseOrEnd => IsEnd || Data[Cursor] == '}';
//            public bool IsScopeOpen()
//            {
//                if (IsEnd)
//                {
//                    Context.LogError($"{nameof(StringData)}.{nameof(Parser)}.{nameof(IsScopeOpen)}: Unexpected end of string");
//                    return false;
//                }

//                return Data[Cursor] == '{';
//            }
//            public bool IsScopeClose()
//            {
//                if (IsEnd)
//                {
//                    Context.LogError($"{nameof(StringData)}.{nameof(Parser)}.{nameof(IsScopeClose)}: Unexpected end of string");
//                    return false;
//                }

//                return Data[Cursor] == '}';
//            }

//            public bool SkipSpaces(bool expectMore = true)
//            {
//                while (Cursor < CursorMax &&
//                       (Data[Cursor] == ' '
//                       || Data[Cursor] == '\t'
//                       || Data[Cursor] == '\n'
//                       || Data[Cursor] == '\r'))
//                {
//                    ++Cursor;
//                }

//                if (IsEnd && expectMore)
//                {
//                    Context.LogError($"{nameof(StringData)}.{nameof(Parser)}.{nameof(SkipSpaces)}: Unexpected end of string");
//                    return false;
//                }
//                return true;
//            }
//            public static int SkipSpaces(string data, int start, int end)
//            {
//                while (start < end &&
//                       (data[start] == ' '
//                        || data[start] == '\t'
//                        || data[start] == '\n'
//                        || data[start] == '\r'))
//                {
//                    ++start;
//                }
//                return start;
//            }
//            public bool ParseChar(char c)
//            {
//                if (IsEnd)
//                {
//                    Context.LogError($"{nameof(StringData)}.{nameof(Parser)}.{nameof(ParseChar)}: Unexpected end of string");
//                    return false;
//                }

//                if (Data[Cursor] != c)
//                {
//                    Context.LogError($"{nameof(StringData)}.{nameof(Parser)}.{nameof(ParseChar)}: expecting '{c}' at {Cursor} but is '{Data[Cursor]}'");
//                    return false;
//                }

//                ++Cursor;
//                return true;
//            }
//            public bool TryParseChar(char c)
//            {
//                if (IsEnd)
//                {
//                    return false;
//                }

//                if (Data[Cursor] != c)
//                {
//                    return false;
//                }

//                ++Cursor;
//                return true;
//            }

//            public bool ParseNext(out object result)
//            {
//                result = null;
//                if (!SkipSpaces(expectMore: false)) return false;
//                if (IsScopeCloseOrEnd) return true;

//                // check if scope
//                if (TryParseChar('{'))
//                {
//                    Scope scope = new Scope();
//                    if (!scope.Parse(Context, StringData, Cursor)) return false;
//                    Cursor = scope.Range.end;
//                    if (!ParseChar('}')) return false;
//                    result = scope;
//                    return true;
//                }
//                else
//                {
//                    if (!ParseData(out result, out var dataRange)) return false;
//                    return true;
//                }

//                //if (!parser.SkipSpaces()) return false;
//                //if (parser.TryParseChar('='))
//                //{
//                //    // scope0 is a key
//                //    if (!parser.SkipSpaces()) return false;


//                //}

//                //if (!parser.ParseScope(out var keyRange)) return false;
//                //if (!parser.SkipSpaces()) return false;
//                //if (!parser.ParseChar('=')) return false;
//                //if (!parser.SkipSpaces()) return false;
//                //if (!parser.ParseScope(out var valueRange)) return false;
//                //Parser keyParser = new(context, data, keyRange);
//                //if (!keyParser.ParseData(out Key, out var dataRange)) return false;
//                ////Key = Data.Data.Substring(keyRange.start, keyRange.length);
//                //ValueRange = valueRange;

//                //// fix up our range, + 1 to include the trailing '}'
//                //Range.length = ValueRange.end - Range.start + 1;
//                ////if (!ParseValueAndChildren(context)) return false;
//                ////return true;
//                //}
//            }

//            public bool FindScopeRange(out RangeInt result)
//            {
//                if (IsEnd)
//                {
//                    Context.LogError($"{nameof(StringData)}.{nameof(Parser)}.{nameof(FindScopeRange)}: Unexpected end of string");
//                    result = new RangeInt(0, 0);
//                    return false;
//                }

//                if (Data[Cursor] != '{')
//                {
//                    Context.LogError($"{nameof(StringData)}.{nameof(Parser)}.{nameof(FindScopeRange)}: expecting '{{' at {Cursor} while parsing scope but found '{Data[Cursor]}'");
//                    result = new RangeInt(0, 0);
//                    return false;
//                }

//                var start = Cursor + 1;
//                var c = start;

//                // start scopes at 1 to include the first expected '{'
//                int scope = 1;
//                while (c < CursorMax)
//                {
//                    if (Data[c] == '{')
//                        ++scope;
//                    else if (Data[c] == '}')
//                    {
//                        --scope;
//                        if (scope <= 0)
//                            break;
//                    }
//                    ++c;
//                }
//                if (c >= CursorMax)
//                {
//                    Context.LogError($"{nameof(StringData)}.{nameof(Parser)}.{nameof(FindScopeRange)}: Unexpected end of string");
//                    result = new RangeInt(0, 0);
//                    return false;
//                }

//                // length is - 1 to remove the last '}' from the scope
//                result = new RangeInt(start, c - start);
//                //// Set cursor c + 1 to exclude the last '}' from the scope
//                //Cursor = c + 1;
//                return true;
//            }

//            public bool ParseData(out object data, out RangeInt fullRange)
//            {
//                data = default;

//                fullRange = new RangeInt();
//                fullRange.start = Cursor;
//                RangeInt valueRange;
//                if (!ParseType(out var t, out var len)) return false;
//                var scopedValueStart = Cursor;
//                if (len < 0)
//                {
//                    //length is dictated by a pair of matching '{' and '}'
//                    if (!SkipSpaces()) return false;
//                    if (!FindScopeRange(out valueRange)) return false;
//                    var scopedValueEnd = valueRange.end + 1; // +1 to include the trailing '}'
//                    len = scopedValueEnd - scopedValueStart;
//                    Cursor = scopedValueEnd;
//                }
//                else
//                {
//                    valueRange = new RangeInt(Cursor, len);
//                    Cursor += len;
//                }

//                fullRange.length = Cursor - fullRange.start;
//                var spanValue = Data.AsSpan(valueRange.start, valueRange.length);
//                switch (t)
//                {
//                    case "s":
//                        data = spanValue.ToString();// Data.Substring(rangeValue.start, rangeValue.length);
//                        return true;
//                    case "b":
//                        if (bool.TryParse(spanValue, out var b))
//                        {
//                            data = b;
//                            return true;
//                        }
//                        Context.LogError($"{nameof(StringData)}.{nameof(Parser)}.{nameof(ParseData)}: Could not parse bool value '{spanValue.ToString()}' at {valueRange.start}");
//                        data = default;
//                        return false;
//                    case "by":
//                        if (byte.TryParse(spanValue, out var by))
//                        {
//                            data = by;
//                            return true;
//                        }
//                        Context.LogError($"{nameof(StringData)}.{nameof(Parser)}.{nameof(ParseData)}: Could not parse byte value '{spanValue.ToString()}' at {valueRange.start}");
//                        data = default;
//                        return false;
//                    case "c":
//                        if (spanValue.Length != 1)
//                        {
//                            Context.LogError($"{nameof(StringData)}.{nameof(Parser)}.{nameof(ParseData)}: Could not parse bool value '{spanValue.ToString()}' at {valueRange.start}");
//                            data = default;
//                            return false;
//                        }
//                        data = Data[valueRange.start];
//                        return true;
//                    case "sh":
//                        if (short.TryParse(spanValue, out var sh))
//                        {
//                            data = sh;
//                            return true;
//                        }
//                        Context.LogError($"{nameof(StringData)}.{nameof(Parser)}.{nameof(ParseData)}: Could not parse short value '{spanValue.ToString()}' at {valueRange.start}");
//                        data = default;
//                        return false;
//                    case "ush":
//                        if (ushort.TryParse(spanValue, out var ush))
//                        {
//                            data = ush;
//                            return true;
//                        }
//                        Context.LogError($"{nameof(StringData)}.{nameof(Parser)}.{nameof(ParseData)}: Could not parse ushort value '{spanValue.ToString()}' at {valueRange.start}");
//                        data = default;
//                        return false;
//                    case "i":
//                        if (int.TryParse(spanValue, out var i))
//                        {
//                            data = i;
//                            return true;
//                        }
//                        Context.LogError($"{nameof(StringData)}.{nameof(Parser)}.{nameof(ParseData)}: Could not parse int value '{spanValue.ToString()}' at {valueRange.start}");
//                        data = default;
//                        return false;
//                    case "ui":
//                        if (uint.TryParse(spanValue, out var ui))
//                        {
//                            data = ui;
//                            return true;
//                        }
//                        Context.LogError($"{nameof(StringData)}.{nameof(Parser)}.{nameof(ParseData)}: Could not parse uint value '{spanValue.ToString()}' at {valueRange.start}");
//                        data = default;
//                        return false;
//                    case "l":
//                        if (long.TryParse(spanValue, out var l))
//                        {
//                            data = l;
//                            return true;
//                        }
//                        Context.LogError($"{nameof(StringData)}.{nameof(Parser)}.{nameof(ParseData)}: Could not parse long value '{spanValue.ToString()}' at {valueRange.start}");
//                        data = default;
//                        return false;
//                    case "ul":
//                        if (ulong.TryParse(spanValue, out var ul))
//                        {
//                            data = ul;
//                            return true;
//                        }
//                        Context.LogError($"{nameof(StringData)}.{nameof(Parser)}.{nameof(ParseData)}: Could not parse ulong value '{spanValue.ToString()}' at {valueRange.start}");
//                        data = default;
//                        return false;
//                    case "f":
//                        if (float.TryParse(spanValue, out var f))
//                        {
//                            data = f;
//                            return true;
//                        }
//                        Context.LogError($"{nameof(StringData)}.{nameof(Parser)}.{nameof(ParseData)}: Could not parse float value '{spanValue.ToString()}' at {valueRange.start}");
//                        data = default;
//                        return false;
//                    case "d":
//                        if (double.TryParse(spanValue, out var d))
//                        {
//                            data = d;
//                            return true;
//                        }
//                        Context.LogError($"{nameof(StringData)}.{nameof(Parser)}.{nameof(ParseData)}: Could not parse double value '{spanValue.ToString()}' at {valueRange.start}");
//                        data = default;
//                        return false;
//                    case "e":
//                        if (int.TryParse(spanValue, out var e))
//                        {
//                            data = e;
//                            return true;
//                        }
//                        Context.LogError($"{nameof(StringData)}.{nameof(Parser)}.{nameof(ParseData)}: Could not parse enum value '{spanValue.ToString()}' at {valueRange.start}");
//                        data = default;
//                        return false;
//                    case "id":
//                        data = Id.FromString(spanValue.ToString());
//                        return true;
//                    //case "o":

//                    //    string typeString;
//                    //    using (var _ = Context.ScopeKey("type", this))
//                    //    {
//                    //        typeString = Context.LoadData(typeof(string), this);
//                    //    }
//                    //    using (var _ = Context.ScopeKey("data", this))
//                    //    {
//                    //        Context.SaveData(type, value, this);
//                    //    }
//                    //    Context.ScopeKey("type", this)
//                    //    if(!ParseScope(out var typeRange))
//                    //    {
//                    //        Context.LogError($"{nameof(StringData)}.{nameof(Parser)}: Could not parse object type {spanValue.ToString()}");
//                    //        data = default;
//                    //        return false;
//                    //    }
//                    //    Parser objectParser = new(Context, StringData, typeRange);
//                    //    if (!objectParser.ParseData(out Key)) return false;
//                    //    Context.
//                    //case "j":

//                    //    if (jsonType != null)
//                    //    {
//                    //        try
//                    //        {
//                    //            data = JsonUtility.FromJson(spanValue.ToString(), jsonType);
//                    //        }
//                    //        catch (Exception e)
//                    //        {
//                    //            Context.LogException(
//                    //                $"{nameof(StringData)}.{nameof(Parser)}: Could not parse json value {spanValue.ToString()}",
//                    //                e);
//                    //            data = default;
//                    //            return false;
//                    //        }
//                    //    }
//                    //    else
//                    //    {
//                    //        data = spanValue.ToString();
//                    //    }

//                    //    return true;
//                    default:
//                        Context.LogError($"{nameof(StringData)}.{nameof(Parser)}: Could not parse value of unknown type '{t}'. Value: '{spanValue.ToString()}' at {valueRange.start}");
//                        data = default;
//                        return false;
//                }
//            }

//            // Advance Cursor after the trailing ':'
//            public bool ParseType(out string type, out int length)
//            {
//                var c = Cursor;
//                var cLength = -1;
//                while (c < CursorMax
//                       && Data[c] != ' '
//                       && Data[c] != '\t'
//                       && Data[c] != '\n'
//                       && Data[c] != '\r'
//                       && Data[c] != ':')
//                {
//                    if (Data[c] == ',')
//                        cLength = c;
//                    ++c;
//                }

//                if (c >= CursorMax)
//                {
//                    Context.LogError($"{nameof(StringData)}.{nameof(Parser)}.{nameof(ParseType)}: Unexpected end of string");
//                    type = default;
//                    length = -1;
//                    return false;
//                }
//                if (cLength >= 0)
//                {
//                    type = Data.Substring(Cursor, cLength - Cursor);
//                    ++cLength; // skip the heading ','
//                    var spanLength = Data.AsSpan(cLength, c - cLength);
//                    if (!int.TryParse(spanLength, out length))
//                    {
//                        Context.LogError($"{nameof(StringData)}.{nameof(Parser)}.{nameof(ParseType)}: could not parse length '{spanLength.ToString()}' at {cLength}");
//                        type = default;
//                        length = -1;
//                        return false;
//                    }
//                }
//                else
//                {
//                    type = Data.Substring(Cursor, c - Cursor);
//                    length = -1;
//                }
//                // +1 to skip the trailing ':'
//                Cursor = c + 1;
//                return true;
//            }
//            //public static string FormatTypeOf(object value)
//            //{
//            //    switch (value)
//            //    {
//            //        case string s: return "s";
//            //        case bool b: return "b";
//            //        case byte by: return "by";
//            //        case char c: return "c";
//            //        case short sh: return "sh";
//            //        case ushort ush: return "ush";
//            //        case int i: return "i";
//            //        case uint ui: return "ui";
//            //        case long l: return "l";
//            //        case ulong ul: return "ul";
//            //        case float f: return "f";
//            //        case double d: return "d";
//            //        default: return "j";
//            //    }
//            //}
//        }
//    }

//    public class StringDataOutput : IDataOutput
//    {
//        public StringBuilder StringBuilder = new();
//        private string CurrentIndent = "";
//        public string Result => StringBuilder.ToString();
//        private bool IsEmptyScope = true;
//        private int InlineCount = 0;
//        void BeginLine()
//        {
//            StringBuilder.Append(CurrentIndent);
//        }

//        void EndLine()
//        {
//            StringBuilder.AppendLine();
//        }

//        void AppendData(string t, object v)
//        {
//            StringBuilder.Append($"{t}:{{{v}}}");
//        }
//        void AppendData(string t, object v, int length)
//        {
//            StringBuilder.Append($"{t},{length}:{v}");
//        }
//        void AppendData(SaveContext context, Type type, object value)
//        {
//            switch (value)
//            {
//                case string s: AppendData("s", s, s.Length); break;
//                case bool b: AppendData("b", b); break;
//                case byte by: AppendData("by", by); break;
//                case char c: AppendData("c", c); break;
//                case short sh: AppendData("sh", sh); break;
//                case ushort ush: AppendData("ush", ush); break;
//                case int i: AppendData("i", i); break;
//                case uint ui: AppendData("ui", ui); break;
//                case long l: AppendData("l", l); break;
//                case ulong ul: AppendData("ul", ul); break;
//                case float f: AppendData("f", f); break;
//                case double d: AppendData("d", d); break;
//                case Enum e: AppendData("e", (int)value); break;
//                case Id id:
//                    var idString = id.AsString;
//                    AppendData("id", id.AsString, idString.Length);
//                    break;
//                default:
//                    context.LogError($"Cannot save object of type {value.GetType()}");
//                    break;
//                    //if (type.IsEnum)
//                    //    throw new Exception();
//                    //else if(type.IsValueType || type.IsClass)
//                    //{
//                    //    StringBuilder.Append("o:{");
//                    //    using (var _ = context.ScopeKey("type", this))
//                    //    {
//                    //        context.SaveData(context.TypeToString(type), this);
//                    //    }
//                    //    using (var _ = context.ScopeKey("data", this))
//                    //    {
//                    //        context.SaveData(type, value, this);
//                    //    }
//                    //    StringBuilder.Append("}");
//                    //}
//                    //else
//                    //    throw new Exception();
//                    //break;
//                    //throw new Exception();
//                    //try
//                    //{
//                    //    var json = JsonUtility.ToJson(value);
//                    //    AppendData("j", json);
//                    //}
//                    //catch (Exception e)
//                    //{
//                    //    Debug.LogError($"Cannot serialize object of type {value.GetType()}");
//                    //}
//                    //break;
//            }
//            //StringBuilder.AppendLine($"{CurrentIndent}({value.GetType().FullName}):{value}");
//        }

//        public void Data(SaveContext context, Type type, object data)
//        {
//            if (InlineCount <= 0)
//            {
//                EndLine();
//                BeginLine();
//            }
//            AppendData(context, type, data);
//            IsEmptyScope = false;
//        }
//        public bool ScopeBegin(SaveContext context, object key)
//        {
//            //if (!IsEmptyScope)
//            EndLine();
//            BeginLine();
//            //StringBuilder.Append("{");
//            ++InlineCount;
//            AppendData(context, key.GetType(), key);
//            //context.SaveData(key, this);
//            //context.SaveObject(key, this);
//            --InlineCount;
//            //StringBuilder.Append("}");
//            StringBuilder.Append(" = {");
//            CurrentIndent += "  ";
//            IsEmptyScope = true;
//            return true;
//        }

//        public void ScopeEnd(SaveContext context, object key)
//        {
//            CurrentIndent = CurrentIndent.Substring(0, CurrentIndent.Length - 2);
//            if (IsEmptyScope)
//                StringBuilder.Append($"}}");
//            else
//            {
//                StringBuilder.AppendLine();
//                StringBuilder.Append($"{CurrentIndent}}}");
//            }
//            IsEmptyScope = false;
//        }

//        public bool SupportType(Type type)
//        {
//            return type.IsPrimitive || type == typeof(string);
//        }
//    }

//    public class StringDataInput : IDataInput
//    {
//        public int Cursor;
//        public StringData StringData;
//        public StringData.Scope CurrentScope;

//        public StringDataInput(StringData data)
//        {
//            StringData = data;
//            CurrentScope = data.Root;
//        }

//        public StringDataInput(SaveContext context, string data)
//        {
//            StringData = new StringData();
//            StringData.Parse(context, data);
//            CurrentScope = StringData.Root;
//        }

//        public object Data(SaveContext context, Type type)
//        {
//            return CurrentScope.Value;
//        }

//        public void DataInPlace(SaveContext context, Type type, object data)
//        {
//            context.LogError($"{nameof(StringDataInput)}.{nameof(DataInPlace)}: Could not load-in-place object of type '{type.FullName}'");
//        }
//        public T Data<T>(SaveContext context)
//        {
//            return (T)CurrentScope.Value;
//        }
//        public object ObjectOfType(SaveContext context, Type type)
//        {
//            if (type.IsAssignableFrom(CurrentScope.Value?.GetType()))
//                return CurrentScope.Value;

//            context.LogError($"Could not load object of type '{type.FullName}'. Serialized object is of type '{CurrentScope.Value?.GetType()}'");
//            return null;
//        }

//        public bool ScopeBegin(SaveContext context, object key)
//        {
//            var child = CurrentScope.GetChild(key);
//            if (child == null)
//                return false;
//            CurrentScope = child;
//            return true;
//        }

//        public void ScopeEnd(SaveContext context, object key)
//        {
//            CurrentScope = CurrentScope.Parent;
//        }

//        public IEnumerable<object> AllScopeKeys
//        {
//            get
//            {
//                foreach (var c in CurrentScope.Children)
//                    yield return c.Key;
//            }
//        }
//        public bool SupportType(Type type)
//        {
//            return type.IsPrimitive || type == typeof(string);
//        }

//    }



//    //public class ObjectDataOutput : IDataOutput
//    //{
//    //    public SaveTypeRegistry SaveTypeRegistry;
//    //    public IDataOutput Output;
//    //    public void Data(SaveContext context, Type type, object data)
//    //    {
//    //        if (type.IsArray)
//    //        {
//    //            SaveArrayData(type, data as Array, io);
//    //        }
//    //        else if (SaveTypeRegistry.TryGetSaveOverride(type, out var sop) && sop != null)
//    //        {
//    //            sop.Save(context, type, data, this);
//    //        }
//    //        else if (data is ISaveOverride so)
//    //        {
//    //            so.Save(context, this);
//    //        }
//    //        else if (SaveTypeRegistry.TryGetSaveFallback(type, out var sopFallback) && sopFallback != null)
//    //        {
//    //            sopFallback.Save(context, type, data, this);
//    //        }
//    //        else if (IsTypePrimitive(type))
//    //            Output.Data(context, type, data);
//    //        else
//    //            ReflectionSO.Instance.Save(context, type, data, this);
//    //    }

//    //    public bool ScopeBegin(SaveContext context, object key)
//    //    {
//    //        throw new NotImplementedException();
//    //    }

//    //    public void ScopeEnd(SaveContext context, object key)
//    //    {
//    //        throw new NotImplementedException();
//    //    }
//    //}
//    //public class ObjectDataInput : IDataInput
//    //{
//    //    public SaveTypeRegistry SaveTypeRegistry;
//    //    public IDataInput Input;
//    //    public object Data(SaveContext context, Type type)
//    //    {
//    //        throw new NotImplementedException();
//    //    }

//    //    public void DataInPlace(SaveContext context, Type type, object data)
//    //    {
//    //        throw new NotImplementedException();
//    //    }

//    //    public bool ScopeBegin(SaveContext context, object key)
//    //    {
//    //        throw new NotImplementedException();
//    //    }

//    //    public void ScopeEnd(SaveContext context, object key)
//    //    {
//    //        throw new NotImplementedException();
//    //    }
//    //    public IEnumerable<object> AllScopeKeys => throw new NotImplementedException();

//    //}

//}