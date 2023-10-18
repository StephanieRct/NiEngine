using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling.Memory.Experimental;
using UnityEngine.UIElements;

namespace NiEngine.IO.SaveOverrides
{
    [Serializable]
    public struct GameObjectMetaData
    {
        public Uid ParentUid;
        public Uid PrefabUid;
        public List<Uid> ChildrenUid;
        public List<Uid> NiBehaviourUid;
    }
    public class GameObjectSO : SaveWithReference<GameObject, GameObjectMetaData>
    {
        public ISaveOverrideProxy Sub;
        
        public Dictionary<Uid, GameObject> DisplacedObjects = new();
        Func<Type, bool> ComponentHasSaveOverride;
        public GameObjectSO(ISaveOverrideProxy sub, Func<Type, bool> componentHasSaveOverride)
        {
            Sub = sub;
            ComponentHasSaveOverride = componentHasSaveOverride;
            IsNull = (x) => x == null;
        }

        public bool IsComponentSaved(Type type)
        {
            return ReflectionSO.IsSaveType(type) || (ComponentHasSaveOverride?.Invoke(type) ?? false);
        }

        public new void LoadMetaData(StreamContext context, IInput io)
        {
            base.LoadMetaData(context, io);

            // must set all NiBehaviour Uid 
            foreach (var (uid, metaData) in SavedObjectMeta)
            {
                if(SavedObjectByUid.TryGetValue(uid, out var obj))
                {
                    var niBehaviours = obj.GetComponents<NiBehaviour>();
                    for (int i = 0; i < niBehaviours.Length; ++i)
                    {
                        niBehaviours[i].Uid = metaData.NiBehaviourUid[i];
                    }
                }
            }
        }
        public override bool TryGetUid(GameObject obj, out Uid uid)
        {
            if(obj.TryGetComponent<SaveId>(out var saveId))
            {
                uid = saveId.Uid;
                return true;
            }
            uid = default;
            return false;
        }

        protected override void DeleteAllInstantiated()
        {
            foreach(var saveId in GameObject.FindObjectsOfType<SaveId>(includeInactive: true))
            {
                if(saveId != null && saveId.IsRuntime)
                    GameObject.DestroyImmediate(saveId.gameObject);
            }


        }
        protected override IDictionary<Uid, GameObject> GetAllExistingObjects()
        {
            // will get only the non-prefab GameObjects
            var dic = Resources.FindObjectsOfTypeAll<SaveId>()
                .ToDictionary(x=>x.Uid, x=>x.gameObject);
            
            return dic;
        }
        protected override void DeleteObject(Uid uid, GameObject obj)
        {
            
            if (obj != null && obj.TryGetComponent<SaveId>(out var saveId))
            {
                if (saveId.IsRuntime)
                    GameObject.DestroyImmediate(obj);
            }
        }
        protected override void LoadObjectData(StreamContext context, Type type, Uid uid, ref GameObject obj, GameObjectMetaData metaData, IInput io)
        {
            obj.SetActive(io.Load<bool>(context, "activeSelf"));


            if (obj.TryGetComponent<SaveId>(out var saveId))
            {

                if (io.TryLoad<List<bool>>(context, "activeChildren", out var activeChildren) && saveId.ChildrenOnLoad != null)
                {
                    for (int i = 0; i != saveId.ChildrenOnLoad.Count; i++)
                    {
                        var child = saveId.ChildrenOnLoad[i];
                        child.gameObject.SetActive(activeChildren[i]);
                    }
                }

                if (saveId.SaveType == SaveId.SaveTypeEnum.ReferenceOnly)
                    return;
                obj.transform.localPosition = io.Load<Vector3>(context, "Pos");
                obj.transform.localRotation = io.Load<Quaternion>(context, "Rot");
                obj.transform.localScale = io.Load<Vector3>(context, "Scale");
                if (saveId.SaveType == SaveId.SaveTypeEnum.TransformOnly)
                    return;
                var typesArray = io.Load<string[]>(context, "componentTypes");
                var componenTypes = typesArray.Select(x => StreamContext.StringToType(x)).ToList();
                var components = obj.GetComponents<Component>().ToList();
                using (var componentScope = context.ScopeKey("components", io))
                {
                    int length = io.Load<int>(context, "length");
                    for (int i = 0; i != length; ++i)
                    {
                        var componentType = componenTypes[i];
                        var compIndex = components.FindIndex(x => x.GetType() == componenTypes[i]);
                        object component = default;
                        if (compIndex < 0)
                        {
                            // this component does not exist and must be added 
                            component = obj.AddComponent(componenTypes[i]);
                        }
                        else
                        {
                            component = components[compIndex];
                            components.RemoveAt(compIndex);
                        }
                        io.LoadInPlace(context, i, ref component);
                    }
                }

            }
        }

        protected override void SaveObjectData(StreamContext context, Type type, GameObject obj, ref GameObjectMetaData metaData, IOutput io)
        {
            if (context.WithDebugData)
            {
                io.Save(context, "Name", obj.GetNameOrNull());
            }
            io.Save(context, "activeSelf", obj.activeSelf);


            if (obj.TryGetComponent<SaveId>(out var saveId))
            {
                metaData.ParentUid = GetUid(obj.transform.parent?.gameObject);
                metaData.PrefabUid = saveId.PrefabUid;

                if (saveId.IsInstantiatedRoot)
                {
                    Instantiate.Add(saveId.Uid);
                }
                Debug.Log($"'{saveId.GetNameOrNull()}': {saveId.Uid} children:{saveId.ChildrenOnLoad?.Count}");
                if (saveId.ChildrenOnLoad != null)
                {
                    metaData.ChildrenUid = saveId.ChildrenOnLoad.Select(x => x.GetComponent<SaveId>()).Where(x=>x!=null).Select(x=>x.Uid).ToList();
                    var childEnabled = saveId.ChildrenOnLoad.Select(x => x.gameObject.activeSelf).ToList();
                    io.Save(context, "activeChildren", childEnabled);
                }
                //List<bool> childEnabled = null;
                //for (int i = 0; i != obj.transform.childCount; i++)
                //{
                //    var child = obj.transform.GetChild(i).gameObject;
                //    if (child.TryGetComponent<SaveId>(out var childSaveId))
                //    {
                //        if (metaData.ChildrenUid == null)
                //            metaData.ChildrenUid = new();
                //        metaData.ChildrenUid.Add(childSaveId.Uid);
                //    }
                //    else
                //    {
                //        if (childEnabled == null)
                //            childEnabled = new();
                //        childEnabled.Add(child.gameObject.activeSelf);
                //    }
                //}
                //if (childEnabled != null)
                //    io.Save(context, "activeChildren", childEnabled);


                if (context.WithDebugData)
                {
                    io.Save(context, "ParentUid", metaData.ParentUid);
                    io.Save(context, "PrefabUid", metaData.PrefabUid);
                    io.Save(context, "IsRuntime", saveId.IsRuntime);
                    io.Save(context, "IsInstantiatedRoot", saveId.IsInstantiatedRoot);

                }

                var niBehaviourUids = obj.GetComponents<NiBehaviour>().Select(x => x.Uid).ToList();
                if (niBehaviourUids != null && niBehaviourUids.Count > 0)
                {
                    metaData.NiBehaviourUid = niBehaviourUids;
                }

                if (saveId.SaveType == SaveId.SaveTypeEnum.ReferenceOnly)
                    return;


                io.Save(context, "Pos", obj.transform.localPosition);
                io.Save(context, "Rot", obj.transform.localRotation);
                io.Save(context, "Scale", obj.transform.localScale);
                if (saveId.SaveType == SaveId.SaveTypeEnum.TransformOnly)
                    return;


                var components = obj.GetComponents<Component>().Where(x => (x is not Transform) && IsComponentSaved(x.GetType())).ToArray();
                var typesArray = components.Select(x => StreamContext.TypeToString(x.GetType())).ToArray();
                io.Save(context, "componentTypes", typesArray);
                using (var componentScope = context.ScopeKey("components", io))
                {
                    int length = components.Length;
                    io.Save(context, "length", length);
                    for (int i = 0; i != length; ++i)
                    {
                        io.SaveInPlace(context, i, components[i]);
                    }
                }
            }


        }

        protected override bool TryInstantiate(StreamContext context, IInput io, Uid uid, GameObjectMetaData metaData, IDictionary<Uid, GameObject> addAllNewObjects, out GameObject obj)
        {
            var prefab = GameObjectExt.FindGameObject(metaData.PrefabUid);
            if (prefab == null)
            {
                if (context.LogFailure($"Could not instantiate GameObject {uid} due to prefab GameObject {metaData.PrefabUid} not being found"))
                {
                    obj = default;
                    return false;
                }
            }
            else
            {
                obj = GameObject.Instantiate(prefab);
                SavedObjectByUid[uid] = obj;
                //InstantiatedObjects[uid] = obj;
                addAllNewObjects.Add(uid, obj);
                if (obj.TryGetComponent<SaveId>(out var saveId))
                {
                    saveId.Uid = uid;
                    saveId.PrefabUid = saveId.UidOnLoad;
                    saveId.IsInstantiatedRoot = true;
                    saveId.IsRuntime = true;
                }
                LinkChildrenToSavedObjects(context, uid, obj, addAllNewObjects);

                return true;
            }
            obj = default;
            return false;
        }
        void LinkChildrenToSavedObjects(StreamContext context, Uid uid, GameObject go, IDictionary<Uid, GameObject> addAllNewObjects)
        {
            if(!SavedObjectMeta.TryGetValue(uid, out var meta))
            {
                context.LogFailure($"Could not link children for GameObject {uid}, no meta data found");
                return;
            }

            var childrenSaveId = go.GetChildrenSaveId();

            if(meta.ChildrenUid != null)
            {
                foreach(var childUid in meta.ChildrenUid)
                {
                    if (SavedObjectMeta.TryGetValue(childUid, out var childMeta))
                    {
                        var childSaveId = childrenSaveId.FirstOrDefault(x => x.Uid == childMeta.PrefabUid);
                        if (childSaveId != null)
                        {
                            childSaveId.Uid = childUid;
                            childSaveId.PrefabUid = childMeta.PrefabUid;
                            childSaveId.IsRuntime = true;
                            addAllNewObjects.Add(childUid, childSaveId.gameObject);
                            LinkChildrenToSavedObjects(context, childUid, childSaveId.gameObject, addAllNewObjects);

                        }
                    }
                }
            }

            //if (childrenSaveId != null && childrenSaveId.Count > 0)
            //{
            //    for (int iSavedChildIndex = 0; iSavedChildIndex < meta.ChildrenUid.Count; ++iSavedChildIndex)
            //    {
            //        var savedChildUid = meta.ChildrenUid[iSavedChildIndex];
            //        // find the instantiated child corresponding to the saved uid.
            //        // get the prefabUid of the saved object
            //        if (SavedObjectMeta.TryGetValue(savedChildUid, out var childMeta))
            //        {
            //            // find the instantiated child with that prefabUid
            //            var index = childrenSaveId.FindIndex(x => x.UidOnLoad == childMeta.PrefabUid);
            //            if (index >= 0)
            //            {
            //                var childSaveId = childrenSaveId[index];
            //                childSaveId.Uid = savedChildUid;
            //                childSaveId.PrefabUid = childSaveId.UidOnLoad;
            //                childSaveId.IsRuntime = true;
            //                addAllNewObjects.Add(savedChildUid, childSaveId.gameObject);
            //                LinkChildrenToSavedObjects(context, savedChildUid, childSaveId.gameObject, addAllNewObjects);

            //            }
            //        }
            //    }
            //}
        }

        public static Uid GetUid(GameObject go)
        {
            if (go != null && go.TryGetComponent<SaveId>(out var saveId))
            {
                return saveId.Uid;
            }
            return Uid.Default;
        }

        public void SaveGameObjectHierarchy(StreamContext context, GameObject go, IOutput io)
        {
            if (go.TryGetComponent<SaveId>(out var saveId))
            {
                io.SaveInPlace(context, saveId.Uid, go);
                for (int i = 0; i != go.transform.childCount; i++)
                {
                    var child = go.transform.GetChild(i).gameObject;
                    SaveGameObjectHierarchy(context, child, io);
                }
            }
        }
        public void LoadGameObjects(StreamContext context, IInput io)
        {
            // reparent before loading data.
            // gameobject's local pos/rot/scale are saved, so we must build the parent-children hierarchy before loading the transform.
            foreach (var (uid, meta) in this.SavedObjectMeta)
            {

                if (SavedObjectByUid.TryGetValue(uid, out var go) && go != null)
                {
                    if (meta.ParentUid.IsDefault)
                    {
                        go.transform.parent = null;
                    }
                    else
                    {
                        if (m_AllObjects.TryGetValue(meta.ParentUid, out var goParent))
                        {
                            if (go.transform.parent != goParent.transform)
                                go.transform.parent = goParent.transform;
                        }
                        else
                            context.LogError($"Could not find parent from GameObject '{go.GetPathNameOrNull()}' uid {uid}, parentUid {meta.ParentUid}");
                    }
                }

            }

            // Load data
            foreach (var k in io.Keys)
            {
                if (k is Uid uid)
                {
                    if (SavedObjectByUid.TryGetValue(uid, out var go) && go != null)
                    {
                        io.LoadInPlace(context, k, ref go);
                    }
                    else
                    {
                        if (context.LogFailure($"Could not find saved GameObject {uid} and it does not have an associated prefab."))
                            return;
                    }
                }
            }
        }
    }
}