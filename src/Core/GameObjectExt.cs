using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Transform = UnityEngine.Transform;
using UnityEditor;
using System;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

namespace NiEngine
{
    public static class GameObjectExt
    {
        public static void BootNi(this GameObject @this)
        {
            foreach(var sm in @this.GetComponents<ReactionStateMachine>())
            {
                sm.Boot();
            }
        }
        public static IEnumerable<ReactionStateMachine> AllReactionStateMachine(this GameObject @this)
            => @this.GetComponents<ReactionStateMachine>().Where(x => x.enabled);
        public static string GetNameOrNull(this GameObject @this)
            => @this == null ? "<null>" : @this.name;
        public static string GetNameOrNull(this MonoBehaviour @this)
            => @this == null ? "<null>" : @this.name;
        public static string GetNameOrNull(this Rigidbody @this)
            => @this == null ? "<null>" : @this.name;
        public static GameObject GetGameObjectOrNull(this Component @this)
            => @this == null ? null : @this.gameObject;
        public static string GetNameOrNull(this Transform @this)
            => @this == null ? "<null>" : @this.name;

        public static string GetPathNameOrNull(this GameObject @this)
            => @this == null ? "<null>" :
            (@this.transform.parent != null ? (@this.transform.parent.gameObject.GetNameOrNull() + "." + @this.name) : @this.name);
        public static Uid GetUid(this GameObject @this)
        {
            if (@this.TryGetComponent<SaveId>(out var saveId))
            {
                return saveId.Uid;
            }
            return Uid.Default;
        }


        public static bool TryGetNiVariableValue<T>(this GameObject @this, string variableName, out T value)
            => NiVariables.TryGetValue(@this, variableName, out value);
        public static bool TryGetNiVariableValue(this GameObject @this, string variableName, out object value)
            => NiVariables.TryGetValue(@this, variableName, out value);

        public static bool TrySetNiVariableValue(this GameObject @this, string variableName, object value)
            => NiVariables.TrySetValue(@this, variableName, value);
        public static bool TrySetNiVariableValue<T>(this GameObject @this, string variableName, T value)
            => NiVariables.TrySetValue<T>(@this, variableName, value);


        public static T GetNiVariableValue<T>(this GameObject @this, string variableName, Owner owner, EventParameters parameters, NiReference location)
        {
            if (NiVariables.TryGetValue<T>(@this, variableName, out var value))
                return value;
            parameters.LogError(location, owner, $"GetNiVariableValue<{typeof(T).FullName}>.Failed", $"On GameObject: '{@this.GetPathNameOrNull()}'");
            return default;
        }
        public static T GetNiVariableValue<T>(this GameObject @this, string variableName)
        {
            if (NiVariables.TryGetValue<T>(@this, variableName, out var value))
                return value;
            Debug.LogError($"GetNiVariableValue<{typeof(T).FullName}>.Failed On GameObject: '{@this.GetPathNameOrNull()}'");
            return default;
        }

        public static bool SetNiVariableValue<T>(this GameObject @this, string variableName, T value, Owner owner, EventParameters parameters, NiReference location)
        {
            if (NiVariables.TrySetValue<T>(@this, variableName, value))
                return true;
            parameters.LogError(location, owner, $"SetNiVariableValue<{typeof(T).FullName}>.Failed", $"On GameObject: '{@this.GetPathNameOrNull()}'");
            return false;
        }
        public static bool SetNiVariableValue<T>(this GameObject @this, string variableName, T value)
        {
            if (NiVariables.TrySetValue<T>(@this, variableName, value))
                return true;
            Debug.LogError($"SetNiVariableValue<{typeof(T).FullName}>.Failed On GameObject: '{@this.GetPathNameOrNull()}'");
            return false;
        }


        public static bool HasActiveReactionState(this GameObject @this, string name)
        {
            return ReactionReference.HasReaction(@this, name, onlyEnabled: true, onlyActive: true, 100);
        }
        public static int React(this GameObject @this, Owner owner, string name, EventParameters parameters)
            => ReactionReference.React(owner, @this, name, parameters);


        //public static void RegisterSaveUid(GameObject go)
        //{
        //    if (go.TryGetComponent<SaveId>(out var saveId))
        //    {
        //        UidObject.Register(saveId.Uid, saveId);
        //    }
        //}
        //public static void RegisterSaveUidAndChildren(GameObject go)
        //{
        //    if (go.TryGetComponent<SaveId>(out var saveId))
        //    {
        //        UidObject.Register(saveId.Uid, saveId);
        //    }
        //    for (int i = 0; i != go.transform.childCount; ++i)
        //    {
        //        RegisterSaveUidAndChildren(go.transform.GetChild(i).gameObject);
        //    }

        //}
        // instance must be an instance of the provided prefab
        public static void LinkPrefab(GameObject instance, GameObject prefab)
        {
            if(instance.TryGetComponent<SaveId>(out var saveId))
            {
                
                saveId.IsRuntime = true;
                saveId.IsInstantiatedRoot = true;
                saveId.PrefabUid = prefab.GetUid();
                //if (saveId.PrefabUid.IsDefault)
                //{
                //    Debug.LogError($"Failed to link prefab on {saveId.Uid} '{instance.name}', Prefab is missing a {nameof(SaveId)} component");
                //    return;
                //}
                UidGameObjectRegistry.RegisterWithNewUid(saveId);
            }
            LinkPrefabChildren(instance);
        }
        static void LinkPrefabChildren(GameObject go)
        {
            for (int i = 0; i != go.transform.childCount; ++i)
            {
                var child = go.transform.GetChild(i).gameObject;
                if (child.TryGetComponent<SaveId>(out var saveId))
                {
                    saveId.IsRuntime = true;
                    if(FindGameObject(saveId.UidOnLoad, out var prefabSaveId) != null)
                    {
                        saveId.PrefabUid = prefabSaveId.Uid;
                    }
                    UidGameObjectRegistry.RegisterWithNewUid(saveId);
                    LinkPrefabChildren(child);
                }
                else
                {
                    LinkPrefabChildren(child);
                }
                
            }

        }



#if UNITY_EDITOR

        public static string GetPath(this GameObject obj, out bool isSceneOrStaged)
        {
            if (obj.transform.parent != null)
            {
                for (int i = 0; i < obj.transform.parent.childCount; ++i)
                {
                    if (obj.transform.parent.GetChild(i) == obj.transform)
                    {
                        return $"{GetPath(obj.transform.parent.gameObject, out isSceneOrStaged)}.{i}";
                    }
                }
                isSceneOrStaged = false;
                return "Failed";
            }
            else
            {
                //PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(saveId)
                var stageCurrent = PrefabStageUtility.GetPrefabStage(obj);
                if (stageCurrent != null)
                {
                    isSceneOrStaged = true;
                    return $"Prefab({stageCurrent.assetPath})";
                }
                if (!string.IsNullOrEmpty(obj.scene.name))
                {
                    var i = Array.IndexOf(obj.scene.GetRootGameObjects(), obj);

                    isSceneOrStaged = true;
                    return $"Scene({obj.scene.name}).{i}";
                }
                var prefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(obj);
                if (!string.IsNullOrEmpty(prefabPath))
                {
                    isSceneOrStaged = false;
                    return $"Prefab({prefabPath})";
                }

                isSceneOrStaged = default;
                return "<unknown>";
            }

        }
        public static void UnlinkPrefab(GameObject instance, bool completely)
        {
            if (instance.TryGetComponent<SaveId>(out var saveId))
            {
                saveId.PrefabUid = Uid.Default;
                //saveId.IsPrefabInstance = false;
            }
            UnlinkPrefabChildren(instance, instance, completely);
        }
        static void UnlinkPrefabChildren(GameObject root, GameObject go, bool completely)
        {
            for (int i = 0; i != go.transform.childCount; ++i)
            {
                var child = go.transform.GetChild(i).gameObject;
                //var pis = PrefabUtility.GetPrefabInstanceStatus(child);
                
                if (!PrefabUtility.IsAddedGameObjectOverride(child)
                    && (completely || PrefabUtility.GetNearestPrefabInstanceRoot(child) == root ))//PrefabUtility.GetPrefabInstanceStatus(child) == PrefabInstanceStatus.NotAPrefab))
                {
                    if (child.TryGetComponent<SaveId>(out var saveId))
                    {
                        //if (!saveId.IsPrefabRoot)
                        {
                            saveId.PrefabUid = Uid.Default;
                            //saveId.IsPrefabInstance = false;
                            UnlinkPrefabChildren(root, child, completely);
                        }
                    }
                    else
                    {
                        UnlinkPrefabChildren(root, child, completely);
                    }
                }

            }
        }
#endif

        public static GameObject InstantiateSavable(GameObject prefab)
        {
            var obj = GameObject.Instantiate(prefab);
            LinkPrefab(obj, prefab);
            return obj;
        }
        public static GameObject InstantiateSavable(GameObject prefab, Transform parent)
        {
            var obj = GameObject.Instantiate(prefab, parent);
            LinkPrefab(obj, prefab);
            return obj;
        }

        public static GameObject InstantiateSavable(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            var obj = GameObject.Instantiate(prefab, position, rotation);
            LinkPrefab(obj, prefab);
            return obj;
        }
        public static GameObject InstantiateSavable(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent)
        {
            var obj = GameObject.Instantiate(prefab, position, rotation, parent);
            LinkPrefab(obj, prefab);
            return obj;
        }

        public static List<SaveId> GetChildrenSaveId(this GameObject go)
        {
            List<SaveId> l = null;
            for (int i = 0; i < go.transform.childCount; ++i)
            {
                var child = go.transform.GetChild(i);
                if(child != null && child.TryGetComponent<SaveId>(out var saveId))
                {
                    if(l == null)
                        l = new List<SaveId>();
                    l.Add(saveId);
                }
            }
            return l;
        }
        public static List<GameObject> GetChildren(this GameObject go)
        {
            List<GameObject> l = null;
            for (int i = 0; i < go.transform.childCount; ++i)
            {
                var child = go.transform.GetChild(i);
                if (child != null)
                {
                    if (l == null)
                        l = new List<GameObject>();
                    l.Add(child.gameObject);
                }
            }
            return l;
        }

        public static GameObject FindPrefab(Uid uid)
        {
            var resObjs = UnityEngine.Resources.FindObjectsOfTypeAll<SaveId>();
            foreach (var saveId in resObjs)
            {
                if (saveId.Uid == uid)
                    return saveId.gameObject;
            }
            return null;
        }
        public static GameObject FindGameObject(Uid uid)
        {
            var resObjs = UnityEngine.Resources.FindObjectsOfTypeAll<SaveId>();
            foreach (var saveId in resObjs)
            {
                if (saveId.Uid == uid)
                    return saveId.gameObject;
            }
            var objs = GameObject.FindObjectsOfType<SaveId>(includeInactive: true);
            foreach (var saveId in objs)
            {
                if (saveId.Uid == uid)
                    return saveId.gameObject;
            }
            return null;
        }
        public static GameObject FindGameObject(Uid uid, out SaveId saveIdOut)
        {
            var resObjs = UnityEngine.Resources.FindObjectsOfTypeAll<SaveId>();
            foreach (var saveId in resObjs)
            {
                if (saveId.Uid == uid)
                {
                    saveIdOut = saveId;
                    return saveId.gameObject;
                }
            }
            var objs = GameObject.FindObjectsOfType<SaveId>(includeInactive: true);
            foreach (var saveId in objs)
            {
                if (saveId.Uid == uid)
                {
                    saveIdOut = saveId;
                    return saveId.gameObject;
                }
            }
            saveIdOut = default;
            return null;
        }
        public static GameObject FindGameObjectInScene(Uid uid)
        {
            // TODO: cache somehow?
            var objs = GameObject.FindObjectsOfType<SaveId>(includeInactive: true);
            foreach (var saveId in objs)
            {
                if (saveId.Uid == uid)
                    return saveId.gameObject;
            }
            return null;
        }
        public static GameObject[] FindGameObjects(Uid[] uids)
        {
            // TODO: Get array of all object only once
            var gos = new GameObject[uids.Length];
            for (int i = 0; i != uids.Length; i++)
                gos[i] = FindGameObject(uids[i]);
            return gos;
        }
        public static NiBehaviour FindNiBehaviour(Uid uid)
        {
            var resObjs = UnityEngine.Resources.FindObjectsOfTypeAll<NiBehaviour>();
            foreach (var b in resObjs)
            {
                if (b.Uid == uid)
                {
                    return b;
                }
            }
            var objs = GameObject.FindObjectsOfType<NiBehaviour>(includeInactive: true);
            foreach (var b in objs)
            {
                if (b.Uid == uid)
                {
                    return b;
                }
            }
            return null;
        }

    }
}