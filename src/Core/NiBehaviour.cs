using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.VisualScripting.YamlDotNet.Core.Tokens;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace NiEngine
{
#if UNITY_EDITOR
    [ExecuteInEditMode]
    [InitializeOnLoad]
#endif
    public class NiBehaviour : MonoBehaviour, IUidObject
#if UNITY_EDITOR
        , ISerializationCallbackReceiver
#endif
    {
        Uid IUidObject.Uid => Uid;
        public Uid Uid = Uid.NewUid();

#if UNITY_EDITOR
        [NonSerialized]
        public Uid UidBackup;
        public NiBehaviour()
        {
            UidBackup = Uid;
        }
        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
            if (!UidBackup.IsDefault)
                Uid = UidBackup;
            else
                UidBackup = Uid;

        }
#endif
        public virtual bool TryFindUidObject(Uid uid, out IUidObject uidObject)
        {
            return TryFindUidObjectInObject(this, uid, out uidObject);
        }
        protected bool TryFindUidObjectInIUidObject(object obj, Uid uid, out IUidObject uidObject)
        {
            switch (obj)
            {
                case IUidObject o:
                    if (o.Uid == uid)
                    {
                        uidObject = o;
                        return true;
                    }
                    break;
                case IUidObjectHost host:
                    foreach (var o in host.Uids)
                        if (TryFindUidObjectInIUidObject(o, uid, out uidObject))
                            return true;
                    break;
            }
            uidObject = default;
            return false;
        
        }
        protected bool TryFindUidObjectInObject(object obj, Uid uid, out IUidObject uidObject)
        {
            if(obj is IUidObjectHost host)
            {
                foreach (var o in host.Uids)
                    if (TryFindUidObjectInIUidObject(o, uid, out uidObject))
                        return true;
                uidObject = default;
                return false;
            }

            var type = GetType();
            foreach (var fi in type.GetFields(BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
            {
                var value = fi.GetValue(this);
                if (TryFindUidObjectInIUidObject(value, uid, out uidObject))
                    return true;
            }
            uidObject = default;
            return false;
        }

    }

}