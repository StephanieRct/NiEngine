using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NiEngine.IO.SaveOverrides
{
    [Serializable]
    public struct NiBehaviourMetaData
    {
        public Uid GameObjectUid;
    }
    public class NiBehaviourSO : SaveWithReference<NiBehaviour, NiBehaviourMetaData>
    {
        public ISaveOverrideProxy Sub;
        public NiBehaviour CurrentNiBehaviour;
        public NiBehaviourSO(ISaveOverrideProxy sub)
        {
            Sub = sub;
            //DeleteUnsavedObjectsOnLoad = false;
        }
        public override bool TryGetUid(NiBehaviour obj, out Uid uid)
        {
            uid = obj.Uid;
            return true;
        }


        public static T FindNiBehaviourByUid<T>(Uid uid)
            where T : NiBehaviour
        {
            var objs = GameObject.FindObjectsOfType<T>(includeInactive: true);
            foreach (var nb in objs)
            {
                if (nb.Uid == uid)
                    return nb;
            }
            return null;
        }
        protected override void DeleteAllInstantiated()
        {
        }
        protected override IDictionary<Uid, NiBehaviour> GetAllExistingObjects()
        {
            var dic = GameObject.FindObjectsOfType<NiBehaviour>(includeInactive: true).ToDictionary(x => x.Uid, x => x);
            return dic;
        }
        protected override void DeleteObject(Uid uid, NiBehaviour obj)
        {
            //GameObject.DestroyImmediate(obj);
        }
        //protected override void BeforeLoad(StreamContext context, IInput io)
        //{

        //}
        //public override NiBehaviour FindObjectByUid(Uid uid)
        //{
        //    var objs = GameObject.FindObjectsOfType<NiBehaviour>(includeInactive: true);
        //    foreach (var nb in objs)
        //    {
        //        if (nb.Uid == uid)
        //            return nb;
        //    }
        //    return null;
        //}

        protected override void LoadObjectData(StreamContext context, Type type, Uid uid, ref NiBehaviour obj, NiBehaviourMetaData metaData, IInput io)
        {
            object o = obj;
            CurrentNiBehaviour = obj;
            if (!TryLoadInPlaceWithOverride(context, obj, io))
                Sub.LoadInPlace(context, type, ref o, io);
            CurrentNiBehaviour = null;
        }

        protected override void SaveObjectData(StreamContext context, Type type, NiBehaviour obj, ref NiBehaviourMetaData metaData, IOutput io)
        {
            CurrentNiBehaviour = obj;
            if (obj.gameObject.TryGetComponent<SaveId>(out var saveId))
                metaData.GameObjectUid = saveId.Uid;

            if (!TrySaveWithOverride(context, type, obj, io))
                Sub.SaveInPlace(context, type, obj, io);
            CurrentNiBehaviour = null;
        }

        protected override bool TryInstantiate(StreamContext context, IInput io, Uid uid, NiBehaviourMetaData metaData, IDictionary<Uid, NiBehaviour> addAllNewObjects, out NiBehaviour obj)
        {
            obj = null;
            return false;
        }
    }


}