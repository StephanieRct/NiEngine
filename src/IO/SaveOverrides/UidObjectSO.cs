using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NiEngine.IO.SaveOverrides
{
    public class UidObjectSO : ISaveOverrideProxy
    {
        public NiBehaviourSO NiBehaviourSO;
        public ReflectionSO ReflectionSO;
        public UidObjectSO(NiBehaviourSO niBehaviourSO, ReflectionSO reflectionSO)
        {
            NiBehaviourSO = niBehaviourSO;
            ReflectionSO = reflectionSO;
        }
        public SupportType IsSupportedType(Type type)
        {
            return typeof(IUidObject).IsAssignableFrom(type) ? SupportType.Supported : SupportType.Unsupported;
        }

        public object Load(StreamContext context, Type type, IInput io)
        {
            var uid = io.Load<Uid>(context, "ref");
            if (uid.IsDefault)
                return null;
            if (NiBehaviourSO.CurrentNiBehaviour.TryFindUidObject(uid, out var obj))
            {
                return obj;
            }
            context.LogError($"Could not find UidObject {uid} in '{NiBehaviourSO.CurrentNiBehaviour.GetNameOrNull()}'", NiBehaviourSO.CurrentNiBehaviour);
            return null;
        }

        public void LoadInPlace(StreamContext context, Type type, ref object obj, IInput io)
        {
            ReflectionSO.LoadInPlace(context, type, ref obj, io);
        }

        public void Save(StreamContext context, Type type, object obj, IOutput io)
        {
            if (obj == null)
            {
                io.Save(context, "ref", Uid.Default);
            }
            else if (obj is IUidObject uidObj)
            {
                io.Save(context, "ref", uidObj.Uid);
            }
            else
                context.LogError($"Could not save object, must be a IUidObject in '{NiBehaviourSO.CurrentNiBehaviour.GetNameOrNull()}'", NiBehaviourSO.CurrentNiBehaviour);
        }

        public void SaveInPlace(StreamContext context, Type type, object obj, IOutput io)
        {
            ReflectionSO.SaveInPlace(context, type, obj, io);

        }
    }
}