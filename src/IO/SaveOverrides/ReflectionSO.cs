using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using UnityEngine;

namespace NiEngine.IO.SaveOverrides
{

    public class ReflectionSO : ISaveOverrideProxy
    {
        static readonly BindingFlags k_BindingFlags = BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
        private static ReflectionSO _Instance = null;
        public static ReflectionSO Instance => _Instance ??= new();

        public Func<Type, bool> TypeCondition;
        public Func<Type, bool> SubTypeCondition;
        //public IStreamBranch SubStreamDecorator;

        //public IOutput RootStreamOutput;
        //public IInput RootStreamInput;
        void CheckBreakpoint(FieldInfo fi)
        {
#if UNITY_EDITOR
            if (fi.GetCustomAttribute<SaveBreakpointAttribute>(inherit: true) != null
                || fi.FieldType.GetCustomAttribute<SaveBreakpointAttribute>(inherit: true) != null)
            {
                Debug.Log("Save Breakpoint");
            }
#endif
        }
        void CheckBreakpoint(PropertyInfo pi)
        {
#if UNITY_EDITOR
            if (pi.GetCustomAttribute<SaveBreakpointAttribute>(inherit: true) != null
                || pi.PropertyType.GetCustomAttribute<SaveBreakpointAttribute>(inherit: true) != null)
            {
                Debug.Log("Save Breakpoint");
            }
#endif
        }
        public SupportType IsSupportedType(Type type)
            => (TypeCondition?.Invoke(type) ?? IsSaveType(type)) ? SupportType.Supported : SupportType.Unsupported;
        public bool IsSupportedField(StreamContext context, FieldInfo fi, out bool isSaveInPlace)
        {
            isSaveInPlace = false;
            var ns = fi.GetCustomAttribute<NotSavedAttribute>(inherit: true);
            if (ns != null)
                return ns.IsDebug && context.WithDebugData;
            var s = fi.GetCustomAttribute<SaveAttribute>(inherit: true);
            if (s != null)
            {
                isSaveInPlace = s.SaveInPlace;
                return true;
            }
            if (fi.GetCustomAttribute<NonSerializedAttribute>(inherit: true) != null)
                return false;
            var saveFieldType = fi.FieldType.GetCustomAttribute<SaveAttribute>(inherit: true);
            if (saveFieldType != null)
            {
                isSaveInPlace = saveFieldType.SaveInPlace;
                return true;
            }
            return SubTypeCondition?.Invoke(fi.FieldType) ?? IsSaveType(fi.FieldType);
        }
        public bool IsSupportedProperty(PropertyInfo pi, out bool isSaveInPlace)
        {
            var s = pi.GetCustomAttribute<SaveAttribute>(inherit: true);
            if (s != null)
            {
                isSaveInPlace = s.SaveInPlace;
                return true;
            }
            isSaveInPlace = false;
            return false;
        }
        public ReflectionSO(){}

        //public ReflectionSO(IStreamBranch subStreamDecorator)
        //{
        //    SubStreamDecorator = subStreamDecorator;
        //}
        //public ReflectionSO(IStreamBranch subStreamDecorator, Func<Type, bool> typeCondition, Func<Type, bool> subTypeCondition)
        //{
        //    SubStreamDecorator = subStreamDecorator;
        //    TypeCondition = typeCondition;
        //    SubTypeCondition = subTypeCondition;
        //}
        public ReflectionSO(Func<Type, bool> typeCondition, Func<Type, bool> subTypeCondition)//IStream rootStream,
        {
            TypeCondition = typeCondition;
            SubTypeCondition = subTypeCondition;
            //RootStreamOutput = rootStream as IOutput;
            //RootStreamInput = rootStream as IInput;
        }

        public void Save(StreamContext context, Type type, object obj, IOutput io)
        {
            if (obj == null)
            {
                //Debug.Log("null");
                io.Save(context, "null", true);
                return;
            }
            var objType = obj.GetType();
            foreach (var fi in objType.GetFields(k_BindingFlags))
            {
                CheckBreakpoint(fi);
                if (!IsSupportedField(context, fi, out var isSaveInPlace))
                    continue;
                var errCount = context.ErrorCount;
                var value = fi.GetValue(obj);
                //if (value is ISaveCallback scb)
                //    scb.BeforeSave(context, io);

                if (isSaveInPlace)
                {
                    io.SaveInPlace(context, fi.Name, fi.FieldType, value);
                }
                else
                {
                    io.Save(context, fi.Name, fi.FieldType, value);
                }

                if (context.ErrorCount > errCount)
                    context.LogAddition($"Stopped at field '{type.Name}.{fi.Name}'");
                if (context.MustStop)
                    return;
            }
            foreach(var pi in objType.GetProperties(k_BindingFlags))
            {
                CheckBreakpoint(pi);
                if (!IsSupportedProperty(pi, out var isSaveInPlace))
                    continue;
                var value = pi.GetValue(obj);
                //if (value is ISaveCallback scb)
                //    scb.BeforeSave(context, io);
                var errCount = context.ErrorCount;
                io.Save(context, pi.Name, pi.PropertyType, value);
                if (context.ErrorCount > errCount)
                    context.LogAddition($"Stopped at property '{type.Name}.{pi.Name}'");
                if (context.MustStop)
                    return;
            }
        }
        public object Load(StreamContext context, Type type, IInput io)
        {
            if (!type.IsValueType) return default;
            if(io.TryLoad<bool>(context, "null", out var isNull))
            {
                return null;
            }
            object obj = context.CreateObject(type);
            foreach (var fi in type.GetFields(k_BindingFlags))
            {
                CheckBreakpoint(fi);
                if (!IsSupportedField(context, fi, out var isSaveInPlace))
                    continue;
                var errCount = context.ErrorCount;
                if (isSaveInPlace)
                {
                    var value = fi.GetValue(obj);
                    io.LoadInPlace(context, fi.Name, fi.FieldType, ref value);
                    fi.SetValue(obj, value);
                    //if (value is ISaveCallback scb)
                    //    scb.AfterLoad(context, io);
                }
                else
                {
                    var value = io.Load(context, fi.Name, fi.FieldType);
                    fi.SetValue(obj, value);
                    //if (value is ISaveCallback scb)
                    //    scb.AfterLoad(context, io);
                }
                if (context.ErrorCount > errCount)
                    context.LogAddition($"Stopped at field '{type.Name}.{fi.Name}'");
                if (context.MustStop)
                    return null;
            }
            foreach (var pi in type.GetProperties(k_BindingFlags))
            {
                CheckBreakpoint(pi);
                if (!IsSupportedProperty(pi, out var isSaveInPlace))
                    continue;
                var errCount = context.ErrorCount;

                if (isSaveInPlace)
                {
                    var value = pi.GetValue(obj);
                    io.LoadInPlace(context, pi.Name, pi.PropertyType, ref value);
                    pi.SetValue(obj, value);
                    //if (value is ISaveCallback scb)
                    //    scb.AfterLoad(context, io);
                }
                else
                {
                    var value = io.Load(context, pi.Name, pi.PropertyType);
                    pi.SetValue(obj, value);
                    //if (value is ISaveCallback scb)
                    //    scb.AfterLoad(context, io);
                }

                if (context.ErrorCount > errCount)
                    context.LogAddition($"Stopped at property '{type.Name}.{pi.Name}'");
                if (context.MustStop)
                    return null;
            }
            return obj;
        }
        public void SaveInPlace(StreamContext context, Type type, object obj, IOutput io)
        {
            Save(context, type, obj, io);
        }
        public void LoadInPlace(StreamContext context, Type type, ref object obj, IInput io)
        {
            if (io.TryLoad<bool>(context, "null", out var isNull))
            {
                obj = null;
                return;
            }
            foreach (var fi in type.GetFields(k_BindingFlags))
            {
                CheckBreakpoint(fi);
                if (!IsSupportedField(context, fi, out var isSaveInPlace))
                    continue;
                var errCount = context.ErrorCount;
                if (isSaveInPlace)
                {
                    var value = fi.GetValue(obj);
                    io.LoadInPlace(context, fi.Name, ref value);
                    fi.SetValue(obj, value);
                    //if (value is ISaveCallback scb)
                    //    scb.AfterLoad(context, io);
                }
                else
                {
                    var value = io.Load(context, fi.Name, fi.FieldType);
                    fi.SetValue(obj, value);
                    //if (value is ISaveCallback scb)
                    //    scb.AfterLoad(context, io);
                }
                if (context.ErrorCount > errCount)
                    context.LogAddition($"Stopped at field '{type.Name}.{fi.Name}'");
                if (context.MustStop)
                    return;
            }
            foreach (var pi in type.GetProperties(k_BindingFlags))
            {
                CheckBreakpoint(pi);
                if (!IsSupportedProperty(pi, out var isSaveInPlace))
                    continue;
                var errCount = context.ErrorCount;

                if (isSaveInPlace)
                {
                    var value = pi.GetValue(obj);
                    io.LoadInPlace(context, pi.Name, ref value);
                    pi.SetValue(obj, value);
                    //if (value is ISaveCallback scb)
                    //    scb.AfterLoad(context, io);
                }
                else
                {
                    var value = io.Load(context, pi.Name, pi.PropertyType);
                    pi.SetValue(obj, value);
                    //if (value is ISaveCallback scb)
                    //    scb.AfterLoad(context, io);
                }

                if (context.ErrorCount > errCount)
                    context.LogAddition($"Stopped at property '{type.Name}.{pi.Name}'");
                if (context.MustStop)
                    return;
            }
        }

        public static bool IsSaveType(Type type)
        {
            if (type == null)
                return false;
            if (type.GetCustomAttribute<NotSavedAttribute>(inherit: true) != null)
                return false;
            else if (type.GetCustomAttribute<SaveAttribute>(inherit: true) != null)
                return true;
            if (type.GetCustomAttribute<SerializableAttribute>(inherit: true) != null)
                return true;
            return false;
        }
    }


    //public class DelegateSO : ISaveOverrideProxy
    //{
    //    static readonly BindingFlags k_BindingFlags = BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
    //    private static DelegateSO _Instance = null;
    //    public static DelegateSO Instance => _Instance ??= new();
    //    public bool IsSupportedType(Type type) => typeof(Delegate).IsAssignableFrom(type);
    //    public void Save(StreamContext context, Type type, object obj, IOutput io)
    //    {
    //        var d = obj as Delegate;
    //        io.Save(context, "target", d.Target);
    //        io.Save(context, "method", d.Method.Name);


    //        //foreach (var fi in typeof(Delegate).GetFields(k_BindingFlags))
    //        //{
    //        //    if (fi.FieldType == typeof(IntPtr))
    //        //        continue;
    //        //    var value = fi.GetValue(obj);
    //        //    using (var _ = context.ScopeKey(fi.Name, io))
    //        //    {
    //        //        if (fi.FieldType.IsValueType)
    //        //        {
    //        //            context.SaveData(fi.FieldType, value, io);
    //        //        }
    //        //        else
    //        //        {
    //        //            context.SaveReference(value, io);
    //        //        }
    //        //    }
    //        //}
    //    }

    //    public object Load(StreamContext context, Type type, IInput io)
    //    {
    //        var target = io.Load<object>(context, "target");
    //        var method = io.Load<string>(context, "method");
    //        object obj = Delegate.CreateDelegate(type, target, method);
    //        //object obj = context.CreateObject(typeof(Delegate), new object[]{target, method});
    //        return obj;
    //    }
    //    public void LoadInPlace(StreamContext context, Type type, ref object data, IInput io)
    //    {
    //        //var d = obj as Delegate;
    //        //d.
    //        throw new NotImplementedException();
    //    }

    //}
}
