using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NiEngine.IO.SaveOverrides
{
    public class ArraySO : ISaveOverrideProxy
    {
        public Func<Type, bool> ElementTypeCondition;
        public SupportType IsSaveType(Type type)
            =>
            type.IsArray
                ? (ElementTypeCondition?.Invoke(type.GetElementType()) ?? true
                 ? SupportType.Supported
                 : SupportType.Ignored)
                : SupportType.Unsupported;
        public SupportType IsSupportedType(Type type)
            => IsSaveType(type);

        public ArraySO()
        {
        }
        public ArraySO(Func<Type, bool> elementTypeCondition)
        {
            ElementTypeCondition = elementTypeCondition;
        }
        public void Save(StreamContext context, Type type, object obj, IOutput io)
        {
            var array = obj as Array;
            if (array == null)
            {
                io.Save<int>(context, "length", -1);
            }
            else
            {
                io.Save(context, "length", array.Length);
                for (int i = 0; i != array.Length; i++)
                {
                    var e = array.GetValue(i);
                    io.Save(context, i, e);
                }
            }
        }

        public object Load(StreamContext context, Type type, IInput io)
        {
            int length = io.Load<int>(context, "length");
            if (length < 0)
                return null;
            var array = context.CreateArray(type, length);
            var eType = type.GetElementType();
            for (int i = 0; i != array.Length; i++)
            {
                if (io.TryLoad(context, i, eType, out object e))
                {
                    array.SetValue(e, i);
                }
                if (context.MustStop)
                    return array;
            }
            return array;
        }

        public void SaveInPlace(StreamContext context, Type type, object obj, IOutput io)
        {
            var array = obj as Array;
            if (array == null)
            {
                io.Save<int>(context, "length", -1);
            }
            else
            {
                io.Save(context, "length", array.Length);
                for (int i = 0; i != array.Length; i++)
                {
                    var e = array.GetValue(i);
                    io.SaveInPlace(context, i, e);
                }
            }
        }
        public void LoadInPlace(StreamContext context, Type type, ref object obj, IInput io)
        {
            var array = obj as Array;
            int length = io.Load<int>(context, "length");
            if (length < 0 && array == null)
                return;
            if (array == null || length != array.Length)
            {
                context.LogError(
                    $"{nameof(ArraySO)}.{nameof(LoadInPlace)}: Array in memory must be the same length ({array.Length}) as the array in stream ({length})",
                    obj);
                return;
            }

            var eType = type.GetElementType();
            if (eType.IsValueType)
            {
                for (int i = 0; i != array.Length; i++)
                {
                    if (io.TryLoad(context, i, eType, out object e))
                    {
                        array.SetValue(e, i);
                    }
                    if (context.MustStop)
                        return;
                }
            }
            else
            {
                for (int i = 0; i != array.Length; i++)
                {
                    var e = array.GetValue(i);
                    io.LoadInPlace(context, i, ref e);
                }
            }
        }

    }
}