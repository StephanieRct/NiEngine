using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NiEngine.IO.SaveOverrides
{
    public class ListSO : ISaveOverrideProxy
    {
        public Func<Type, bool> ElementTypeCondition;

        public SupportType IsSaveType(Type type)
            =>
            (typeof(IList).IsAssignableFrom(type) && type.IsGenericType)
                ? (ElementTypeCondition?.Invoke(type.GenericTypeArguments.FirstOrDefault()) ?? true
                 ? SupportType.Supported
                 : SupportType.Ignored)
                : SupportType.Unsupported;
        public SupportType IsSupportedType(Type type)
            => IsSaveType(type);

        public ListSO()
        {
        }
        public ListSO(Func<Type, bool> elementTypeCondition)
        {
            ElementTypeCondition = elementTypeCondition;
        }

        public void Save(StreamContext context, Type type, object obj, IOutput io)
        {
            var list = obj as IList;
            if (list == null)
            {
                io.Save<int>(context, "length", -1);
            }
            else
            {
                var eType = list.GetType().GetGenericArguments()[0];
                io.Save(context, "length", list.Count);
                for (int i = 0; i != list.Count; i++)
                {
                    var e = list[i];
                    io.Save(context, i, eType, e);
                }
            }

        }

        public object Load(StreamContext context, Type type, IInput io)
        {
            int length = io.Load<int>(context, "length");
            if (length < 0)
                return null;
            var list = (IList)context.CreateObject(type);
            var eType = list.GetType().GetGenericArguments()[0];
            for (int i = 0; i != length; i++)
            {
                var e = io.Load(context, i, eType);
                if (context.MustStop)
                    return list;
                list.Add(e);
            }
            return list;
        }
        public void SaveInPlace(StreamContext context, Type type, object obj, IOutput io)
        {
            var list = obj as IList;
            if (list == null)
            {
                io.Save<int>(context, "length", -1);
            }
            else
            {
                var eType = list.GetType().GetGenericArguments()[0];
                io.Save(context, "length", list.Count);
                for (int i = 0; i != list.Count; i++)
                {
                    var e = list[i];
                    io.SaveInPlace(context, i, eType, e);
                }
            }
        }
        public void LoadInPlace(StreamContext context, Type type, ref object obj, IInput io)
        {
            var list = obj as IList;
            int length = io.Load<int>(context, "length");
            if (length < 0 && list == null)
                return;
            if (list == null || length != list.Count)
            {
                context.LogError(
                    $"{nameof(ListSO)}.{nameof(LoadInPlace)}: List in memory must be the same length ({list.Count}) as the array in stream ({length})",
                    obj);
                return;
            }

            var eType = list.GetType().GetGenericArguments()[0];
            if (eType.IsValueType)
            {
                for (int i = 0; i != list.Count; i++)
                {
                    var e = list[i];
                    io.LoadInPlace(context, i, ref e);
                    if (context.MustStop)
                        return;
                    list[i] = e;
                }
            }
            else
            {
                for (int i = 0; i != list.Count; i++)
                {
                    var e = list[i];
                    if (e == null)
                        continue;
                    io.LoadInPlace(context, i, ref e);
                    if (context.MustStop)
                        return;
                    list[i] = e;
                }
            }
        }

    }


    public class HashSetSO<T> : ISaveOverrideProxy
    {
        public Func<Type, bool> ElementTypeCondition;


        public SupportType IsSaveType(Type type)
            =>
            (type.GetGenericTypeDefinition() == typeof(HashSet<>))
                ? (ElementTypeCondition?.Invoke(type.GenericTypeArguments.FirstOrDefault()) ?? true
                 ? SupportType.Supported
                 : SupportType.Ignored)
                : SupportType.Unsupported;

        public SupportType IsSupportedType(Type type)
            => IsSaveType(type);

        public HashSetSO()
        {
        }
        public HashSetSO(Func<Type, bool> elementTypeCondition)
        {
            ElementTypeCondition = elementTypeCondition;
        }

        public void Save(StreamContext context, Type type, object obj, IOutput io)
        {
            var hashset = obj as HashSet<T>;
            if (hashset == null)
            {
                io.Save<int>(context, "length", -1);
            }
            else
            {
                io.Save(context, "length", hashset.Count);
                int i = 0;
                foreach(var e in hashset)
                {
                    io.Save(context, i, typeof(T), e);
                    ++i;
                }
            }

        }

        public object Load(StreamContext context, Type type, IInput io)
        {
            int length = io.Load<int>(context, "length");
            if (length < 0)
                return null;
            var hashset = new HashSet<T>();
            for (int i = 0; i != length; i++)
            {
                var e = io.Load<T>(context, i);
                if (context.MustStop)
                    return hashset;
                hashset.Add(e);
            }
            return hashset;
        }
        public void SaveInPlace(StreamContext context, Type type, object obj, IOutput io)
        {
            var hashset = obj as HashSet<T>;
            if (hashset == null)
            {
                io.Save<int>(context, "length", -1);
            }
            else
            {
                var eType = hashset.GetType().GetGenericArguments()[0];
                io.Save(context, "length", hashset.Count);
                int i = 0;
                foreach (var e in hashset)
                {
                    io.SaveInPlace(context, i, typeof(T), e);
                    ++i;
                }
            }
        }
        public void LoadInPlace(StreamContext context, Type type, ref object obj, IInput io)
        {
            var hashset = obj as HashSet<T>;
            int length = io.Load<int>(context, "length");
            if (length < 0 && hashset == null)
                return;
            if (hashset == null || length != hashset.Count)
            {
                context.LogError(
                    $"{nameof(ListSO)}.{nameof(LoadInPlace)}: List in memory must be the same length ({hashset.Count}) as the array in stream ({length})",
                    obj);
                return;
            }

            var eType = hashset.GetType().GetGenericArguments()[0];
            if (eType.IsValueType)
            {
                List<T> loaded = new();
                int i = 0;
                foreach (var e in hashset)
                {
                    object o = e;
                    io.LoadInPlace(context, i, typeof(T), ref o);
                    loaded.Add((T)o);
                    ++i;
                }
                hashset.Clear();
                foreach (var e in loaded)
                {
                    hashset.Add(e);
                }
            }
            else
            {
                int i = 0;
                foreach (var e in hashset)
                {
                    object o = e;
                    io.LoadInPlace(context, i, typeof(T), ref o);
                    if (o != (object)e)
                        context.LogError("HashSet failed to load in place object");
                    ++i;
                }
            }
        }

    }

    public class Generic1SO : ISaveOverrideProxy
    {
        public Type GenericType;
        public Type GenericSO;
        public Func<Type, bool> ElementTypeCondition;


        public SupportType IsSaveType(Type type)
            =>
            (type.GetGenericTypeDefinition() == GenericType)
                ? (ElementTypeCondition?.Invoke(type.GenericTypeArguments.FirstOrDefault()) ?? true
                 ? SupportType.Supported
                 : SupportType.Ignored)
                : SupportType.Unsupported;

        public SupportType IsSupportedType(Type type)
            => IsSaveType(type);

        public Generic1SO(Type genericType, Type genericSO)
        {
            GenericType = genericType;
            GenericSO = genericSO;
        }
        public Generic1SO(Type genericType, Type genericSO, Func<Type, bool> elementTypeCondition)
        {
            GenericType = genericType;
            GenericSO = genericSO;
            ElementTypeCondition = elementTypeCondition;
        }
        ISaveOverrideProxy GetSo(StreamContext context, Type type)
        {
            // todo: cache these
            var eType = type.GetGenericArguments()[0];
            return (ISaveOverrideProxy)context.CreateObject(GenericSO.MakeGenericType(eType));
        }
        public void Save(StreamContext context, Type type, object obj, IOutput io)
        {
            var so = GetSo(context, type);
            so.Save(context, type, obj, io);
        }

        public object Load(StreamContext context, Type type, IInput io)
        {
            var so = GetSo(context, type);
            return so.Load(context, type, io);
        }
        public void SaveInPlace(StreamContext context, Type type, object obj, IOutput io)
        {
            var so = GetSo(context, type);
            so.SaveInPlace(context, type, obj, io);
        }
        public void LoadInPlace(StreamContext context, Type type, ref object obj, IInput io)
        {
            var so = GetSo(context, type);
            so.LoadInPlace(context, type, ref obj, io);
        }

    }
    //public class HashSetSO : ISaveOverrideProxy
    //{
    //    public Func<Type, bool> ElementTypeCondition;
    //    public bool IsSaveType(Type type)
    //        => type.GetGenericTypeDefinition() == typeof(HashSet<>)
    //        && (ElementTypeCondition?.Invoke(type.GenericTypeArguments.FirstOrDefault()) ?? true);
            

    //    public bool IsSupportedType(Type type)
    //        => IsSaveType(type);

    //    public HashSetSO()
    //    {
    //    }
    //    public HashSetSO(Func<Type, bool> elementTypeCondition)
    //    {
    //        ElementTypeCondition = elementTypeCondition;
    //    }
    //    ISaveOverrideProxy GetSo(StreamContext context, Type type)
    //    {
    //        // todo: cache these
    //        var eType = type.GetGenericArguments()[0];
    //        return (ISaveOverrideProxy)context.CreateObject(typeof(HashSetSO<>).MakeGenericType(eType));
    //    }
    //    public void Save(StreamContext context, Type type, object obj, IOutput io)
    //    {
    //        var so = GetSo(context, type);
    //        so.Save(context, type, obj, io);
    //    }

    //    public object Load(StreamContext context, Type type, IInput io)
    //    {
    //        var so = GetSo(context, type);
    //        return so.Load(context, type, io);
    //    }
    //    public void SaveInPlace(StreamContext context, Type type, object obj, IOutput io)
    //    {
    //        var so = GetSo(context, type);
    //        so.SaveInPlace(context, type, obj, io);
    //    }
    //    public void LoadInPlace(StreamContext context, Type type, ref object obj, IInput io)
    //    {
    //        var so = GetSo(context, type);
    //        so.LoadInPlace(context, type, ref obj, io);
    //    }

    //}


    public class DictionarySO : ISaveOverrideProxy
    {
        public Func<Type, bool> KeyTypeCondition;
        public Func<Type, bool> ValueTypeCondition;


        public SupportType IsSaveType(Type type)
            =>
            (typeof(IDictionary).IsAssignableFrom(type))
                ? ((KeyTypeCondition?.Invoke(type.GenericTypeArguments.FirstOrDefault()) ?? true) && (ValueTypeCondition?.Invoke(type.GenericTypeArguments[1]) ?? true)
                 ? SupportType.Supported
                 : SupportType.Ignored)
                : SupportType.Unsupported;

        public SupportType IsSupportedType(Type type)
            => IsSaveType(type);

        public DictionarySO(Func<Type, bool> keyTypeCondition, Func<Type, bool> valueTypeCondition)
        {
            KeyTypeCondition = keyTypeCondition;
            ValueTypeCondition = valueTypeCondition;
        }

        public void Save(StreamContext context, Type type, object obj, IOutput io)
        {
            var dic = obj as IDictionary;
            if (dic == null)
            {
                io.Save(context, "null", true);
            }
            else
            {
                var vType = type.GenericTypeArguments[1];
                var e = dic.GetEnumerator();
                while (e.MoveNext())
                {
                    io.Save(context, e.Key, vType, e.Value);
                    if (context.MustStop)
                        return;
                }
            }

        }

        public object Load(StreamContext context, Type type, IInput io)
        {
            
            if (io.TryLoad(context, "null", out bool isNull))
                return null;
            var dic = (IDictionary)context.CreateObject(type);
            var vType = dic.GetType().GetGenericArguments()[1];
            foreach (var k in io.Keys)
            {
                var v = io.Load(context, k, vType);
                if (context.MustStop)
                    return dic;
                dic.Add(k, v);
            }
            return dic;
        }
        public void SaveInPlace(StreamContext context, Type type, object obj, IOutput io)
        {
            var dic = obj as IDictionary;
            if (dic == null)
            {
                io.Save(context, "null", true);
            }
            else
            {
                var vType = type.GenericTypeArguments[1];
                var e = dic.GetEnumerator();
                while (e.MoveNext())
                {
                    io.SaveInPlace(context, e.Key, vType, e.Value);
                    if (context.MustStop)
                        return;
                }
            }
        }
        public void LoadInPlace(StreamContext context, Type type, ref object obj, IInput io)
        {
            var dic = obj as IDictionary;

            var vType = dic.GetType().GetGenericArguments()[1];
            if (vType.IsValueType)
            {
                foreach (var k in io.Keys)
                {
                    var v = dic[k];
                    io.LoadInPlace(context, k, ref v);
                    if (context.MustStop)
                        return;
                    dic[k] = v;
                }
            }
            else
            {
                foreach (var k in io.Keys)
                {
                    var v = dic[k];
                    if (v == null)
                        continue;
                    io.LoadInPlace(context, k, ref v);
                    if (context.MustStop)
                        return;
                    dic[k] = v;
                }
            }
        }

    }
}
