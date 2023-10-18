//#define NIENGINE_SAVEID_LOG_VALIDATION
using System;
using System.Collections.Generic;
using UnityEditor;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif
using UnityEngine;

namespace NiEngine
{
#if UNITY_EDITOR
    [InitializeOnLoad]
    [ExecuteAlways]
#endif
    [DisallowMultipleComponent]
    public class SaveId : MonoBehaviour, ISerializationCallbackReceiver
    {
        
        public Uid Uid = Uid.NewUid();

        //[NonSerialized]
        //public Uid UidRegistered;
        [NonSerialized]
        public Uid UidOnLoad;

        public Uid PrefabUid;
        public enum SaveTypeEnum
        {
            SaveData,
            TransformOnly,
            ReferenceOnly,
        }
        [SerializeField]
        public SaveTypeEnum SaveType;
        //public GameObject Prefab;
        [NonSerialized]
        public bool IsRuntime = false;
        [NonSerialized]
        public bool IsInstantiatedRoot = false;
        //[HideInInspector]
        //public bool IsPrefabRoot;
        //[HideInInspector]
        //public bool IsPrefab;
        //public bool IsPrefabInstance = false;
        //[NonSerialized]
        //public List<SaveId> ChildrenOnLoad;
        public List<GameObject> ChildrenOnLoad;
        public void OnAfterDeserialize()
        {
            UidOnLoad = Uid;
#if UNITY_EDITOR
            UidGameObjectRegistry.NewSaveId.Add(this);
            //UidObject.RegisterUUID2(this, "AfterDeserialize");
            //if (Uid.IsDefault || Uid != UidRegistered)
            //    UidObject.RegisterUUID(this);
#endif
        }
        public void OnBeforeSerialize()
        {
            ChildrenOnLoad = gameObject.GetChildren();
#if UNITY_EDITOR
            UidGameObjectRegistry.FixKnownUid(this);
            //if (Uid.IsDefault)
            //    UidGameObjectRegistry.NewSaveId.Add(this);
            //UidObject.RegisterUUID2(this, "BeforeSerialize");
            //if (Uid.IsDefault || Uid != UidRegistered)
            //    UidObject.RegisterUUID(this);
#endif
        }
        //void OnDestroy()
        //{
        //    //UidObject.UnregisterUUID(this);
        //    //Uid = default;
        //}

#if UNITY_EDITOR
        void OnValidate()
        {
            UidGameObjectRegistry.NewSaveId.Add(this);
            //if (EditorApplication.isPlayingOrWillChangePlaymode)
            //    return;
            //switch (PrefabUtility.GetPrefabInstanceStatus(this))
            //{
            //    case PrefabInstanceStatus.NotAPrefab:
            //    case PrefabInstanceStatus.MissingAsset:
            //        if (!PrefabUid.IsDefault)
            //        {
            //            PrefabUid = default;
            //            EditorUtility.SetDirty(this);
            //        }
            //        //ValidatePrefabInstanceMissingAsset();
            //        break;
            //    case PrefabInstanceStatus.Connected:
            //        var other = PrefabUtility.GetCorrespondingObjectFromSource(this);
            //        if (other != null && PrefabUid != other.Uid)
            //        {
            //            Debug.Log($"{Uid} is a prefab but its PrefabUid isn't set. should be {other.Uid}");
            //            PrefabUid = other.Uid;
            //            EditorUtility.SetDirty(this);
            //        }
            //        break;
            //}
        }



        static void Unpack(GameObject go, PrefabUnpackMode mode)
        {
            GameObjectExt.UnlinkPrefab(go, mode == PrefabUnpackMode.Completely);

        }

        public int GetInstanceID2()
        {
            var fi = typeof(UnityEngine.Object).GetField("m_InstanceID", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.FlattenHierarchy | System.Reflection.BindingFlags.GetField);
            return (int)fi.GetValue(this);
        }
#endif

        public override string ToString()
        {
            if(gameObject == null)
            {
                return "Object was deleted";
            }
#if UNITY_EDITOR
            return $"SaveId({Uid}, \"{name}\", {GetInstanceID2()})";
#else
            return $"SaveId({Uid}, \"{name}\")";
#endif
        }
        static SaveId()
        {

#if UNITY_EDITOR
            PrefabUtility.prefabInstanceUnpacking += Unpack;
            
#endif

        }

    }
}


