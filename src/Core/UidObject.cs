using System.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

namespace NiEngine
{




#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    public class UidObject : NiReference, IUidObject, ISerializationCallbackReceiver, ICloneable
    {
        [HideInInspector]
        public Uid Uid;
        // TODO, need to make UidObject uids unique within a NiBehaviour only
        public UidObject()
        {
            Uid = Uid.NewUid();
        }
        public struct SaveIdInstance
        {
            public SaveId SaveId;
            public string Info;
        }
        public struct ObjectEntry
        {
            public List<SaveIdInstance> Objects;
            //public List<string> Info;
        }


        
        public static SortedDictionary<Uid, ObjectEntry> UidToObjects = new();
        public static SortedDictionary<Uid, object> UidToObject = new();

        Uid IUidObject.Uid => Uid;

        [System.Diagnostics.Conditional("NIENGINE_UID_LOG_REGISTRATION")]
        static void LogRegistration(string msg, object obj = null)
        {
            Debug.Log(msg, obj as UnityEngine.Object);
        }
#if UNITY_EDITOR
        //[UnityEditor.MenuItem("Tools/NiEngine/Uid/Print All Uid")]
        //public static void PrintAllUid()
        //{
        //    StringBuilder uidObjects = new StringBuilder();
        //    uidObjects.AppendLine("UidObjects:");
        //    foreach (var (u, o) in NiEngine.UidObject.UidToObject)
        //    {
        //        if (o is SaveId saveId)
        //        { 
        //            if(saveId == null || saveId.gameObject == null)
        //                uidObjects.AppendLine($"{u} : GameObject was deleted");
        //            else
        //            {
        //                uidObjects.AppendLine($"{u} : GameObject {o}");
        //                var stageCurrent = PrefabStageUtility.GetPrefabStage(saveId.gameObject);
        //                uidObjects.AppendLine($"IsPartOfPrefabAsset: {PrefabUtility.IsPartOfPrefabAsset(saveId)}");
        //                uidObjects.AppendLine($"GetPrefabAssetType: {PrefabUtility.GetPrefabAssetType(saveId)}");
        //                uidObjects.AppendLine($"IsPartOfPrefabInstance: {PrefabUtility.IsPartOfPrefabInstance(saveId)}");
        //                uidObjects.AppendLine($"GetPrefabInstanceStatus: {PrefabUtility.GetPrefabInstanceStatus(saveId)}");
        //                uidObjects.AppendLine($"IsPartOfPrefabThatCanBeAppliedTo: {PrefabUtility.IsPartOfPrefabThatCanBeAppliedTo(saveId)}");
        //                uidObjects.AppendLine($"IsPartOfNonAssetPrefabInstance: {PrefabUtility.IsPartOfNonAssetPrefabInstance(saveId)}");
        //                uidObjects.AppendLine($"GetPrefabAssetPathOfNearestInstanceRoot: {PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(saveId)}");
        //                uidObjects.AppendLine($"GetPrefabStage: {stageCurrent != null}");
        //            }
        //        }
        //        else if (o != null)
        //            uidObjects.AppendLine($"{u} : [{o.GetHashCode():X8}] : \"{o}\" (type:{o?.GetType().FullName})");
        //        else
        //            uidObjects.AppendLine($"{u} : Object was deleted");
        //    }
        //    Debug.Log(uidObjects.ToString());
        //}

        //static string Join<T>(IEnumerable<SaveIdInstance> ids, Func<SaveId, T> f)
        //{
        //    StringBuilder sb = new StringBuilder();
        //    bool first = true;
        //    foreach (var id in ids)
        //    {
        //        if(!first)
        //            sb.Append(", ");
        //        sb.Append(f(id.SaveId));
        //        first = false;
        //    }
        //    return sb.ToString();
        //}
        //[UnityEditor.MenuItem("Tools/NiEngine/Uid/Print GameObject Uid")]
        //public static void PrintGameObjectUid()
        //{
        //    StringBuilder uidObjects = new StringBuilder();
        //    uidObjects.AppendLine("Single Uid GameObjects:");
        //    foreach (var (u, o) in NiEngine.UidObject.UidToObject)
        //    {
        //        if (o is SaveId saveId)
        //        {
        //            if (saveId == null || saveId.gameObject == null)
        //                uidObjects.AppendLine($"{u} : GameObject was deleted");
        //            else
        //            {
        //                uidObjects.AppendLine($"{u} : GameObject {o}");
        //                var stageCurrent = PrefabStageUtility.GetPrefabStage(saveId.gameObject);
        //                uidObjects.AppendLine($"\tIID: {saveId.GetInstanceID()}");
        //                uidObjects.AppendLine($"\tIsPartOfPrefabAsset: {PrefabUtility.IsPartOfPrefabAsset(saveId)}");
        //                uidObjects.AppendLine($"\tGetPrefabAssetType: {PrefabUtility.GetPrefabAssetType(saveId)}");
        //                uidObjects.AppendLine($"\tIsPartOfPrefabInstance: {PrefabUtility.IsPartOfPrefabInstance(saveId)}");
        //                uidObjects.AppendLine($"\tGetPrefabInstanceStatus: {PrefabUtility.GetPrefabInstanceStatus(saveId)}");
        //                uidObjects.AppendLine($"\tIsPartOfPrefabThatCanBeAppliedTo: {PrefabUtility.IsPartOfPrefabThatCanBeAppliedTo(saveId)}");
        //                uidObjects.AppendLine($"\tIsPartOfNonAssetPrefabInstance: {PrefabUtility.IsPartOfNonAssetPrefabInstance(saveId)}");
        //                uidObjects.AppendLine($"\tGetPrefabAssetPathOfNearestInstanceRoot: {PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(saveId)}");
        //                uidObjects.AppendLine($"\tGetPrefabStage: {stageCurrent != null}");
        //            }
                    
        //        }
        //    }
        //    Debug.Log(uidObjects.ToString());

        //    uidObjects.Clear();
        //    uidObjects.AppendLine("Uid GameObjects:");
        //    foreach (var (u, os) in NiEngine.UidObject.UidToObjects)
        //    {
        //        var ids = os.Objects.Where(x=>x.SaveId != null);
        //        uidObjects.AppendLine($"{u} : count({os.Objects.Count}, {ids.Count()})");
        //        //if (ids.Any())
        //        {
        //            uidObjects.AppendLine($"\tIID: {Join(ids, x => x.GetInstanceID())}");
        //            uidObjects.AppendLine($"\tIsPartOfPrefabAsset: {Join(ids, x => PrefabUtility.IsPartOfPrefabAsset(x))}");
        //            uidObjects.AppendLine($"\tGetPrefabAssetType: {Join(ids, x => PrefabUtility.GetPrefabAssetType(x))}");
        //            uidObjects.AppendLine($"\tIsPartOfPrefabInstance: {Join(ids, x => PrefabUtility.IsPartOfPrefabInstance(x))}");
        //            uidObjects.AppendLine($"\tGetPrefabInstanceStatus: {Join(ids, x => PrefabUtility.GetPrefabInstanceStatus(x))}");
        //            uidObjects.AppendLine($"\tIsPartOfPrefabThatCanBeAppliedTo: {Join(ids, x => PrefabUtility.IsPartOfPrefabThatCanBeAppliedTo(x))}");
        //            uidObjects.AppendLine($"\tIsPartOfNonAssetPrefabInstance: {Join(ids, x => PrefabUtility.IsPartOfNonAssetPrefabInstance(x))}");
        //            uidObjects.AppendLine($"\tGetPrefabAssetPathOfNearestInstanceRoot: {Join(ids, x => PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(x))}");
        //            uidObjects.AppendLine($"\tGetPrefabStage: {Join(ids, x => PrefabStageUtility.GetPrefabStage(x.gameObject) != null)}");
        //            uidObjects.AppendLine($"\tGetPath: {Join(ids, x => x.gameObject.GetPath())}");
        //        }
        //        foreach (var si in os.Objects)
        //        {
        //            uidObjects.AppendLine($"\tIID:{si.SaveId.GetInstanceID()} {(si.SaveId != null ? "Alive" : "Dead")}\n {si.Info.Replace("\n", "\n\t\t")}");
                    
        //        }
        //    }
        //    Debug.Log(uidObjects.ToString());
        //}
        //[UnityEditor.MenuItem("Tools/NiEngine/Uid/Clear Registry")]
        //public static void ClearAllUid()
        //{
        //    NiEngine.UidObject.UidToObject.Clear();
        //}
#endif
        //public static Uid RegisterNewUid(object obj)
        //{
        //    int i = 10;
        //    while (i > 0)
        //    {
        //        var newUid = Uid.NewUid();
        //        if (UidToObject.TryAdd(newUid, obj))
        //        {
        //            LogRegistration($"++++ Uid {newUid} with [{obj.GetHashCode():X8}] \"{obj}\"", obj);
        //            return newUid;
        //        }
        //        --i;
        //    }
        //    throw new Exception("Could not create a non-conflicting Uid.");
        //}

        public static void Register(Uid uid, object obj)
        {
            LogRegistration($"++++ Uid {uid} with [{obj.GetHashCode():X8}] \"{obj}\"", obj);
            UidObject.UidToObject[uid] = obj;
        }
        public static void Unregister(Uid uid, object obj)
        {
            if (UidObject.UidToObject.TryGetValue(uid, out var existing))
            {
                if(existing == obj)
                    if (UidObject.UidToObject.Remove(uid))
                        LogRegistration($"---- Uid {uid} with [{obj.GetHashCode():X8}] \"{obj}\"", obj);
            }
            else
                LogRegistration($"---- Uid {uid}(NotFound) with [{obj.GetHashCode():X8}] \"{obj}\"", obj);
        }
        public static bool TryGetUidObject(Uid uid, out object obj)
        {
            return UidToObject.TryGetValue(uid, out obj);
        }
        public void OnBeforeSerialize()
        {
#if UNITY_EDITOR
            if (Uid.IsDefault)
                Uid = Uid.NewUid();
#endif
        }

        public void OnAfterDeserialize()
        {
            if (Uid.IsDefault)
                Debug.LogError($"UidObject with default Uid {GetType().FullName}");
            else
            {
                Register(Uid, this);
            }
        }

        public override object Clone()
        {
            var clone = (UidObject)this.DeepClone();
            clone.Uid = Uid.NewUid();
            Register(clone.Uid, clone);
            return clone;
        }




        //public static SortedDictionary<Uid, SaveId> UidToSaveId = new();
        //public static Dictionary<SaveId, Uid> SaveIdToUid = new();
        //static Dictionary<SaveId, Uid> m_ObjToUUID = new Dictionary<SaveId, Uid>();
        //static Dictionary<Uid, SaveId> m_UUIDtoObj = new Dictionary<Uid, SaveId>();
        //static string PrintDetails(SaveId saveId)
        //{
        //    return "";
        //    var sd = new StringBuilder();
        //    //sd.AppendLine($"\tIID: {saveId.GetInstanceID2()}");
        //    //return sd.ToString();
        //    //return "";
        //    try
        //    {
        //        sd.AppendLine($"\tIID: {saveId.GetInstanceID2()}");
        //        sd.AppendLine($"\tIsPartOfPrefabAsset: {PrefabUtility.IsPartOfPrefabAsset(saveId)}");
        //        sd.AppendLine($"\tGetPrefabAssetType: {PrefabUtility.GetPrefabAssetType(saveId)}");
        //        sd.AppendLine($"\tIsPartOfPrefabInstance: {PrefabUtility.IsPartOfPrefabInstance(saveId)}");
        //        sd.AppendLine($"\tGetPrefabInstanceStatus: {PrefabUtility.GetPrefabInstanceStatus(saveId)}");
        //        sd.AppendLine($"\tIsPartOfPrefabThatCanBeAppliedTo: {PrefabUtility.IsPartOfPrefabThatCanBeAppliedTo(saveId)}");
        //        sd.AppendLine($"\tIsPartOfNonAssetPrefabInstance: {PrefabUtility.IsPartOfNonAssetPrefabInstance(saveId)}");
        //        sd.AppendLine($"\tGetPrefabAssetPathOfNearestInstanceRoot: {PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(saveId)}");

        //        var stageCurrent = PrefabStageUtility.GetPrefabStage(saveId.gameObject);
        //        sd.AppendLine($"\tGetPrefabStage: {stageCurrent != null}");
        //        sd.AppendLine($"\tGetPath: {saveId.gameObject.GetPath()}");
                
        //    }
        //    catch (Exception e)
        //    {

        //    }

        //    return sd.ToString();
        //}

//        public static void UnregisterUUID(SaveId saveId)
//        {
//            UidToObject.Remove(saveId.Uid);
//            SaveIdToUid.Remove(saveId);
//            //if (UidToObjects.TryGetValue(saveId.Uid, out var e))
//            {
//                //e.Objects.Remove(saveId);
//                //if (e.Objects.Count == 0)
//                //{
//                //    UidToObjects.Remove(saveId.Uid);
//                //}
//            }
//        }
//        public static void RegisterUUID(SaveId aID)
//        {
//            Uid UID;
//            if (SaveIdToUid.TryGetValue(aID, out UID))
//            {
//                // found object instance, update ID
//                aID.Uid = UID;
//                aID.UidRegistered = aID.Uid;
//                if (!UidToObject.ContainsKey(UID))
//                    UidToObject.Add(UID, aID);
//                return;
//            }

//            if (aID.Uid.IsDefault)
//            {
//                // No ID yet, generate a new one.
//                aID.Uid = Uid.NewUid();
//                aID.UidRegistered = aID.Uid;
//                UidToObject.Add(aID.Uid, aID);
//                SaveIdToUid.Add(aID, aID.Uid);
//                return;
//            }

//            if (!UidToObject.TryGetValue(aID.Uid, out var existingObject))
//            {
//                // ID not known to the DB, so just register it
//                UidToObject.Add(aID.Uid, aID);
//                SaveIdToUid.Add(aID, aID.Uid);
//                return;
//            }
//            if (existingObject is SaveId existing)
//            {
//                if (existing == aID)
//                {
//                    // DB inconsistency
//                    SaveIdToUid.Add(aID, aID.Uid);
//                    return;
//                }
//                if (existing == null)
//                {
//                    // object in DB got destroyed, replace with new
//                    UidToObject[aID.Uid] = aID;
//                    SaveIdToUid.Add(aID, aID.Uid);
//                    return;
//                }
//                // we got a duplicate
//                // check if instance of prefab
//                //if(PrefabUtility.)
//                //var prefab = PrefabUtility.GetCorrespondingObjectFromSource(aID);
//                //if(existing == prefab)
//                //{
//                //    // get a new instance of a prefab
//                //    aID.Uid = Uid.NewUid();
//                //    aID.UidBackup = aID.Uid;
//                //    aID.PrefabUid = prefab.Uid;
//                //    UidToSaveId.Add(aID.Uid, aID);
//                //    SaveIdToUid.Add(aID, aID.Uid);
//                //}

//#if UNITY_EDITOR
//                try
//                {

//                    var newId = Uid.NewUid();
//                    var stageCurrent = PrefabStageUtility.GetPrefabStage(aID.gameObject);
//                    var stageExisting = PrefabStageUtility.GetPrefabStage(existing.gameObject);

//                    if (stageCurrent != null || stageExisting != null)
//                        //PrefabUtility.IsPartOfPrefabThatCanBeAppliedTo(aID) || PrefabUtility.IsPartOfPrefabThatCanBeAppliedTo(existing))
//                        //|| (!PrefabUtility.IsPartOfPrefabThatCanBeAppliedTo(aID) && PrefabUtility.IsPartOfPrefabThatCanBeAppliedTo(existing)))
//                    {
//                        string msg = $"Duplicate ignored! {aID.Uid}";
//                        if (stageCurrent == null)
//                        {
//                            aID.UidRegistered = aID.Uid;
//                            UidToObject[aID.Uid] = aID;
//                            SaveIdToUid.Add(aID, aID.Uid);
//                            msg += " Overwrite existing";
//                        }
//                        msg += $"\nIsPartOfPrefabAsset: {PrefabUtility.IsPartOfPrefabAsset(aID)} <===> {PrefabUtility.IsPartOfPrefabAsset(existing)}";
//                        msg += $"\nGetPrefabAssetType: {PrefabUtility.GetPrefabAssetType(aID)} <===> {PrefabUtility.GetPrefabAssetType(existing)}";
//                        msg += $"\nIsPartOfPrefabInstance: {PrefabUtility.IsPartOfPrefabInstance(aID)} <===> {PrefabUtility.IsPartOfPrefabInstance(existing)}";
//                        msg += $"\nGetPrefabInstanceStatus: {PrefabUtility.GetPrefabInstanceStatus(aID)} <===> {PrefabUtility.GetPrefabInstanceStatus(existing)}";
//                        msg += $"\nIsPartOfPrefabThatCanBeAppliedTo: {PrefabUtility.IsPartOfPrefabThatCanBeAppliedTo(aID)} <===> {PrefabUtility.IsPartOfPrefabThatCanBeAppliedTo(existing)}";
//                        msg += $"\nIsPartOfNonAssetPrefabInstance: {PrefabUtility.IsPartOfNonAssetPrefabInstance(aID)} <===> {PrefabUtility.IsPartOfNonAssetPrefabInstance(existing)}";
//                        msg += $"\nGetPrefabAssetPathOfNearestInstanceRoot: {PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(aID)} <===> {PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(existing)}";
//                        msg += $"\nGetPrefabStage: {stageCurrent != null} <===> {stageExisting != null}";

//                        Debug.Log(msg);
//                        return;
//                    }
//                    else
//                    {
//                        string msg = $"Duplicate! {aID.Uid} => {newId}";
//                        msg += $"\nIsPartOfPrefabAsset: {PrefabUtility.IsPartOfPrefabAsset(aID)} <===> {PrefabUtility.IsPartOfPrefabAsset(existing)}";
//                        msg += $"\nGetPrefabAssetType: {PrefabUtility.GetPrefabAssetType(aID)} <===> {PrefabUtility.GetPrefabAssetType(existing)}";
//                        msg += $"\nIsPartOfPrefabInstance: {PrefabUtility.IsPartOfPrefabInstance(aID)} <===> {PrefabUtility.IsPartOfPrefabInstance(existing)}";
//                        msg += $"\nGetPrefabInstanceStatus: {PrefabUtility.GetPrefabInstanceStatus(aID)} <===> {PrefabUtility.GetPrefabInstanceStatus(existing)}";
//                        msg += $"\nIsPartOfPrefabThatCanBeAppliedTo: {PrefabUtility.IsPartOfPrefabThatCanBeAppliedTo(aID)} <===> {PrefabUtility.IsPartOfPrefabThatCanBeAppliedTo(existing)}";
//                        msg += $"\nIsPartOfNonAssetPrefabInstance: {PrefabUtility.IsPartOfNonAssetPrefabInstance(aID)} <===> {PrefabUtility.IsPartOfNonAssetPrefabInstance(existing)}";
//                        msg += $"\nGetPrefabAssetPathOfNearestInstanceRoot: {PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(aID)} <===> {PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(existing)}";
//                        msg += $"\nGetPrefabStage: {stageCurrent != null} <===> {stageExisting != null}";
//                        Debug.Log(msg);
//                        // we got a duplicate, generate new ID
//                        aID.Uid = newId;
//                        aID.UidRegistered = aID.Uid;
//                        UidToObject.Add(aID.Uid, aID);
//                        SaveIdToUid.Add(aID, aID.Uid);
//                    }
//                } catch(UnityException e)
//                {

//                }
//#else
//                aID.Uid = Uid.NewUid();
//                aID.UidBackup = aID.Uid;
//                UidToObject.Add(aID.Uid, aID);
//                SaveIdToUid.Add(aID, aID.Uid);
//#endif
//            }
//        }

//        static void HandleCopy(SaveId saveId)
//        {
//            var newUid = Uid.NewUid();
//            Debug.Log($"Make Copy of {saveId.Uid} => {newUid}");
//            saveId.Uid = newUid;
//            saveId.UidRegistered = saveId.Uid;
//            UidToObject.Add(saveId.Uid, saveId);
//            Add(saveId);
//            SaveIdToUid.Add(saveId, saveId.Uid);
//        }

//        static void Add(SaveId saveId)
//        {
//            if (UidToObjects.TryGetValue(saveId.Uid, out var e))
//            {
//                //try
//                //{
//                //    var stageCurrent = PrefabStageUtility.GetPrefabStage(saveId.gameObject);
//                //    if (stageCurrent == null)
//                //    {
//                //        // remove old objects
//                //        //e.Objects.RemoveAll(x => x.SaveId == null || x.SaveId.gameObject == null);

//                //        // check if there's another non-stage object. if so, it's a new object and need a new uid.
//                //        // if there's a stage object existing, make it become the new uid as well
//                //        var indexNonStage = e.Objects.FindIndex(x => x.SaveId != null && PrefabStageUtility.GetPrefabStage(x.SaveId.gameObject) == null);
                        
//                //        if(indexNonStage >= 0)
//                //        {
//                //            if (e.Objects[indexNonStage].SaveId != saveId)
//                //            {
//                //                //var oldUid = saveId.Uid;
//                //                //HandleCopy(saveId);
//                //                //e.Objects.RemoveAt(indexNonStage);
//                //                //var indexStage = e.Objects.FindIndex(x => PrefabStageUtility.GetPrefabStage(x.gameObject) != null);
//                //                //if (indexStage >= 0)
//                //                //{
//                //                //    e.Objects[indexStage].Uid = saveId.Uid;
//                //                //    e.Objects[indexStage].UidRegistered = saveId.Uid;
//                //                //    Add(e.Objects[indexStage]);
//                //                //    e.Objects.RemoveAt(indexStage);
//                //                //}

//                //                //if (e.Objects.Count == 0)
//                //                //{
//                //                //    UidToObjects.Remove(oldUid);
//                //                //}
//                //            }
//                //            //return;
//                //        }
//                //    }
//                //}
//                //catch (UnityException ex)
//                //{
//                //}


//                if (!e.Objects.Any(x=> x.SaveId == saveId))
//                {
//                    s_UidToUpdate.Add(saveId.Uid);
//                    var details = PrintDetails(saveId);
//                    e.Objects.Add(new SaveIdInstance
//                    {
//                        SaveId = saveId,
//                        Info = details
//                    });
//                    Debug.Log($"Add {saveId.Uid} \n{details}");
//                }
//            }
//            else
//            {
//                e = new ObjectEntry
//                {
//                    Objects = new()
//                };
//                var details = PrintDetails(saveId);
//                e.Objects.Add(new SaveIdInstance
//                {
//                    SaveId = saveId,
//                    Info = details
//                });
//                UidToObjects.Add(saveId.Uid, e);
//                Debug.Log($"Add {saveId.Uid} \n{details}");
//            }
//        }
//        public static void RegisterUUID2(SaveId aID, string from)
//        {
//            Add(aID);
////            if (SaveIdToUid.TryGetValue(aID, out var UID))
////            {
////                // found object instance, update ID
////                aID.Uid = UID;
////                aID.UidRegistered = aID.Uid;
////                if (!UidToObject.ContainsKey(UID))
////                {
////                    UidToObject.Add(UID, aID);
////                }
////                Add(aID);
////                Debug.Log($"Register known {aID.Uid} from: {from} \n{PrintDetails(aID)}");
////                return;
////            }

////            if (aID.Uid.IsDefault)
////            {
////                // No ID yet, generate a new one.
////                aID.Uid = Uid.NewUid();
////                aID.UidRegistered = aID.Uid;
////                UidToObject.Add(aID.Uid, aID);
////                Add(aID);
////                SaveIdToUid.Add(aID, aID.Uid);
                
////                Debug.Log($"Register new {aID.Uid} from: {from} \n{PrintDetails(aID)}");
////                return;
////            }

////            if (!UidToObject.TryGetValue(aID.Uid, out var existingObject))
////            {
////                // ID not known to the DB, so just register it
////                aID.UidRegistered = aID.Uid;
////                UidToObject.Add(aID.Uid, aID);
////                Add(aID);
////                SaveIdToUid.Add(aID, aID.Uid);
////                Debug.Log($"Register unknown {aID.Uid} from: {from} \n{PrintDetails(aID)}");
////                return;
////            }

////#if UNITY_EDITOR
////            if (existingObject is SaveId existing)
////            {
////                if (existing == aID)
////                {
////                    // DB inconsistency
////                    aID.UidRegistered = aID.Uid;
////                    SaveIdToUid.Add(aID, aID.Uid);
////                    Debug.Log($"DB inconsistency {aID.Uid} from: {from} \n{PrintDetails(aID)}");
////                    return;
////                }
////                if (existing == null)
////                {
////                    // object in DB got destroyed, replace with new
////                    aID.UidRegistered = aID.Uid;
////                    UidToObject[aID.Uid] = aID;
////                    Add(aID);
////                    SaveIdToUid.Remove(existing);
////                    SaveIdToUid.Add(aID, aID.Uid);
////                    Debug.Log($"Takeover destroyed {aID.Uid} from: {from} \n{PrintDetails(aID)}");
////                    return;
////                }
                
////                try
////                {
////                    var newId = Uid.NewUid();
////                    var stageCurrent = PrefabStageUtility.GetPrefabStage(aID.gameObject);
////                    var stageExisting = PrefabStageUtility.GetPrefabStage(existing.gameObject);

////                    string msg;
////                    if(stageCurrent != null)
////                    {
////                        msg = $"Duplicate on stage ignored. {aID.Uid} from: {from}";
////                    }
////                    else if(stageExisting != null)
////                    {

////                        msg = $"Overwrite stage object! {aID.Uid} from: {from}";
////                        aID.UidRegistered = aID.Uid;
////                        UidToObject[aID.Uid] = aID;
////                        Add(aID);
////                        SaveIdToUid.Remove(existing);
////                        SaveIdToUid.Add(aID, aID.Uid);
////                    }
////                    else
////                        msg = $"Duplicate! {aID.Uid} => {newId} from: {from}";


////                    msg += $"\nIID: {aID.GetInstanceID()} <===> {existing.GetInstanceID()}";
////                    msg += $"\nIsPartOfPrefabAsset: {PrefabUtility.IsPartOfPrefabAsset(aID)} <===> {PrefabUtility.IsPartOfPrefabAsset(existing)}";
////                    msg += $"\nGetPrefabAssetType: {PrefabUtility.GetPrefabAssetType(aID)} <===> {PrefabUtility.GetPrefabAssetType(existing)}";
////                    msg += $"\nIsPartOfPrefabInstance: {PrefabUtility.IsPartOfPrefabInstance(aID)} <===> {PrefabUtility.IsPartOfPrefabInstance(existing)}";
////                    msg += $"\nGetPrefabInstanceStatus: {PrefabUtility.GetPrefabInstanceStatus(aID)} <===> {PrefabUtility.GetPrefabInstanceStatus(existing)}";
////                    msg += $"\nIsPartOfPrefabThatCanBeAppliedTo: {PrefabUtility.IsPartOfPrefabThatCanBeAppliedTo(aID)} <===> {PrefabUtility.IsPartOfPrefabThatCanBeAppliedTo(existing)}";
////                    msg += $"\nIsPartOfNonAssetPrefabInstance: {PrefabUtility.IsPartOfNonAssetPrefabInstance(aID)} <===> {PrefabUtility.IsPartOfNonAssetPrefabInstance(existing)}";
////                    msg += $"\nGetPrefabAssetPathOfNearestInstanceRoot: {PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(aID)} <===> {PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(existing)}";
////                    msg += $"\nGetPrefabStage: {stageCurrent != null} <===> {stageExisting != null}";
////                    Debug.Log(msg);
////                    return;
////                }
////                catch (UnityException e)
////                {
////                    Debug.Log($"Duplicate! {aID.Uid} but no info. taking over, from: {from}");
////                }
////                aID.UidRegistered = aID.Uid;
////                UidToObject[aID.Uid] = aID;
////                Add(aID);
////                SaveIdToUid.Remove(existing);
////                SaveIdToUid.Add(aID, aID.Uid);
////            }
////#else
////                aID.Uid = Uid.NewUid();
////                aID.UidRegistered = aID.Uid;
////                UidToObject.Add(aID.Uid, aID);
////                    Add(aID);
////                SaveIdToUid.Add(aID, aID.Uid);
////#endif
//        }
        //public static void ReassignUid(SaveId saveId, Uid newUid, Uid newPrefabUid)
        //{
        //    UnregisterUUID(saveId);
        //    saveId.Uid = newUid;
        //    saveId.UidRegistered = newUid;
        //    saveId.PrefabUid = newPrefabUid;
        //    UidToObject.Add(newUid, saveId);
        //    Add(saveId);
        //    SaveIdToUid.Add(saveId, newUid);
        //}

//#if UNITY_EDITOR
//        static HashSet<Uid> s_UidToUpdate = new();
//        static UidObject()
//        {
//            EditorApplication.update += EditorUpdate;
//        }
//        static void EditorUpdate()
//        {
//            foreach(var uid in s_UidToUpdate)
//            {
//                if (UidToObjects.TryGetValue(uid, out var e))
//                {
//                    var sb = new StringBuilder();

//                    var ids = e.Objects.Where(x => x.SaveId != null);
//                    sb.AppendLine($"{uid} : count({e.Objects.Count}, {ids.Count()})");
//                    //if (ids.Any())
//                    {
//                        sb.AppendLine($"\tIID: {Join(ids, x => x.GetInstanceID())}");
//                        sb.AppendLine($"\tIsPartOfPrefabAsset: {Join(ids, x => PrefabUtility.IsPartOfPrefabAsset(x))}");
//                        sb.AppendLine($"\tGetPrefabAssetType: {Join(ids, x => PrefabUtility.GetPrefabAssetType(x))}");
//                        sb.AppendLine($"\tIsPartOfPrefabInstance: {Join(ids, x => PrefabUtility.IsPartOfPrefabInstance(x))}");
//                        sb.AppendLine($"\tGetPrefabInstanceStatus: {Join(ids, x => PrefabUtility.GetPrefabInstanceStatus(x))}");
//                        sb.AppendLine($"\tIsPartOfPrefabThatCanBeAppliedTo: {Join(ids, x => PrefabUtility.IsPartOfPrefabThatCanBeAppliedTo(x))}");
//                        sb.AppendLine($"\tIsPartOfNonAssetPrefabInstance: {Join(ids, x => PrefabUtility.IsPartOfNonAssetPrefabInstance(x))}");
//                        sb.AppendLine($"\tGetPrefabAssetPathOfNearestInstanceRoot: {Join(ids, x => PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(x))}");
//                        sb.AppendLine($"\tGetPrefabStage: {Join(ids, x => PrefabStageUtility.GetPrefabStage(x.gameObject) != null)}");
//                        sb.AppendLine($"\tGetPath: {Join(ids, x => x.gameObject.GetPath())}");
//                    }
//                    foreach (var si in e.Objects)
//                    {
//                        sb.AppendLine($"\tIID:{si.SaveId.GetInstanceID()} {(si.SaveId != null ? "Alive" : "Dead")}\n {si.Info.Replace("\n", "\n\t\t")}");

//                    }
//                    Debug.Log($"Update uid {uid}\n{sb.ToString()}");
//                }
//            }
//            s_UidToUpdate.Clear();
//        }
//#endif
    }
}