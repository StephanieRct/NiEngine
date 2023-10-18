using System.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;
using System.IO;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

namespace NiEngine
{

#if UNITY_EDITOR
    [InitializeOnLoad]
    public partial class UidGameObjectRegistry
    {
        public static HashSet<SaveId> NewSaveId = new();
        public static SortedDictionary<Uid, ObjectEntry> UidToObjects = new();
        public static SortedDictionary<string, Uid> PathToUid = new();

        public struct ObjectEntry
        {
            public string UniquePath;
            public List<SaveId> Instances;

        }
        [UnityEditor.MenuItem("Tools/NiEngine/Clear Uid Registry")]
        public static void ClearAllUid()
        {
            UidToObjects.Clear();
            PathToUid.Clear();
        }
        [UnityEditor.MenuItem("Tools/NiEngine/Print Uid Registry")]
        public static void PrintAllUid()
        {
            var sb = new StringBuilder();
            foreach(var (uid, entry) in UidToObjects)
            {
                sb.AppendLine($"{uid}, \"{entry.UniquePath}\", {entry.Instances.Count} ");
            }
            sb.AppendLine($"-----");
            foreach (var (path, uid) in PathToUid)
            {
                sb.AppendLine($"\"{path}\" => {uid}");
            }
            Debug.Log(sb.ToString());
        }
        public static void RegisterWithNewUid(SaveId saveId)
        {

            AssignNewIds(saveId);
            if (!EditorApplication.isPlaying)
                AddToRegistry(saveId);
        }
        public static void RemoveFromRegistry(SaveId saveId)
        {
            if (UidToObjects.TryGetValue(saveId.Uid, out var entry))
            {
                PathToUid.Remove(entry.UniquePath);
                entry.Instances.Remove(saveId);
                if (entry.Instances.Count == 0 || entry.Instances.All(x => x == null))
                    UidToObjects.Remove(saveId.Uid);

            }
        }
        static void EditorUpdate()
        {
            if (!EditorApplication.isPlayingOrWillChangePlaymode && NewSaveId.Count > 0)
            {
                var saveIds = NewSaveId;
                NewSaveId = new();
                foreach (var saveId in saveIds)
                {
                    if (saveId != null)
                    {
                        CheckPrefabConnection(saveId);
                        if (saveId.Uid.IsDefault)
                            saveId.Uid = Uid.NewUid();
                        AddToRegistry(saveId);
                    }
                }
            }
            else
                NewSaveId.Clear();
        }
        public static string FixKnownUid(SaveId saveId)
        {
            var path = saveId.gameObject.GetPath(out var isSceneOrStaged);
            if (isSceneOrStaged)
            {
                //redefine known path to uid
                if (UidToObjects.TryGetValue(saveId.Uid, out var entry))
                {
                    if (entry.UniquePath != path)
                    {
                        if (entry.Instances.Contains(saveId))
                        {
                            Debug.Log($"redefine '{entry.UniquePath}' to '{path}'");
                            //object has been move in the hierarchy
                            entry.UniquePath = path;
                            UidToObjects[saveId.Uid] = entry;
                            PathToUid[path] = saveId.Uid;
                        }
                    }
                }

            }
            else
            {
                // reset saveid with known path to uid
                if (PathToUid.TryGetValue(path, out var pathUid))
                {
                    if (saveId.Uid != pathUid)
                    {
                        Debug.Log($"reset '{path}' from {saveId.Uid} to {pathUid}");
                        saveId.Uid = pathUid;
                        EditorUtility.SetDirty(saveId);
                    }
                }
            }
            return path;
        }
        static void AddToRegistry(SaveId saveId)
        {
            var path = FixKnownUid(saveId);
            int iMax = 10;
            while (iMax > 0 && UidToObjects.TryGetValue(saveId.Uid, out var entry))
            {
                if (entry.UniquePath == path)
                {
                    if(!entry.Instances.Contains(saveId))
                        entry.Instances.Add(saveId);

                    return;
                }
                else
                {
                    AssignNewIds(saveId);
                }
                --iMax;
            }
            var newEntry = new ObjectEntry();
            newEntry.UniquePath = path;
            newEntry.Instances = new List<SaveId> { saveId };
            UidToObjects.Add(saveId.Uid, newEntry);
            PathToUid[newEntry.UniquePath] = saveId.Uid;
        }

        static void CheckPrefabConnection(SaveId saveId)
        {
            switch (PrefabUtility.GetPrefabInstanceStatus(saveId))
            {
                case PrefabInstanceStatus.NotAPrefab:
                case PrefabInstanceStatus.MissingAsset:
                    if (!saveId.PrefabUid.IsDefault)
                    {
                        saveId.PrefabUid = default;
                        EditorUtility.SetDirty(saveId);
                    }
                    break;
                case PrefabInstanceStatus.Connected:
                    var other = PrefabUtility.GetCorrespondingObjectFromSource(saveId);
                    if (other != null && saveId.PrefabUid != other.Uid)
                    {
                        saveId.PrefabUid = other.Uid;
                        EditorUtility.SetDirty(saveId);
                    }
                    break;
            }
            if (saveId.Uid == saveId.PrefabUid)
            {
                AssignNewIds(saveId);
            }
        }
        static UidGameObjectRegistry()
        {
            EditorApplication.update += EditorUpdate;
        }

        static string Join<T>(IEnumerable<SaveId> ids, Func<SaveId, T> f)
        {
            StringBuilder sb = new StringBuilder();
            bool first = true;
            foreach (var id in ids)
            {
                if (!first)
                    sb.Append(", ");
                sb.Append(f(id));
                first = false;
            }
            return sb.ToString();
        }
        [UnityEditor.MenuItem("Tools/NiEngine/Print GameObject Uid")]
        public static void PrintGameObjectUid()
        {
            StringBuilder uidObjects = new StringBuilder();
            uidObjects.AppendLine("Uid GameObjects:");
            foreach (var (u, os) in UidToObjects)
            {
                var ids = os.Instances.Where(x => x != null);
                uidObjects.AppendLine($"{u} : count({os.Instances.Count}, {ids.Count()}), Path: {os.UniquePath}");
                if (ids.Any())
                {
                    uidObjects.AppendLine($"\tIID: {Join(ids, x => x.GetInstanceID())}");
                    uidObjects.AppendLine($"\tIsPartOfPrefabAsset: {Join(ids, x => PrefabUtility.IsPartOfPrefabAsset(x))}");
                    uidObjects.AppendLine($"\tGetPrefabAssetType: {Join(ids, x => PrefabUtility.GetPrefabAssetType(x))}");
                    uidObjects.AppendLine($"\tIsPartOfPrefabInstance: {Join(ids, x => PrefabUtility.IsPartOfPrefabInstance(x))}");
                    uidObjects.AppendLine($"\tGetPrefabInstanceStatus: {Join(ids, x => PrefabUtility.GetPrefabInstanceStatus(x))}");
                    uidObjects.AppendLine($"\tIsPartOfPrefabThatCanBeAppliedTo: {Join(ids, x => PrefabUtility.IsPartOfPrefabThatCanBeAppliedTo(x))}");
                    uidObjects.AppendLine($"\tIsPartOfNonAssetPrefabInstance: {Join(ids, x => PrefabUtility.IsPartOfNonAssetPrefabInstance(x))}");
                    uidObjects.AppendLine($"\tGetPrefabAssetPathOfNearestInstanceRoot: {Join(ids, x => PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(x))}");
                    uidObjects.AppendLine($"\tGetPrefabStage: {Join(ids, x => PrefabStageUtility.GetPrefabStage(x.gameObject) != null)}");
                    uidObjects.AppendLine($"\tGetPath: {Join(ids, x => "\"" + x.gameObject.GetPath(out var isSceneOrStaged) + $"\", isSceneOrStaged: {isSceneOrStaged}")}");
                }
            }
            Debug.Log(uidObjects.ToString());
        }
    }
#else
    public partial class UidGameObjectRegistry
    {
        public static void RegisterWithNewUid(SaveId saveId)
        {
            AssignNewIds(saveId);
        }
    }
#endif

    public partial class UidGameObjectRegistry
    {
        static void AssignNewIds(SaveId saveId)
        {
            var newId = Uid.NewUid();
            Debug.Log($"AssignNewIds '{saveId.gameObject.GetPath(out var isSceneOrStaged)}'(isSceneOrStaged:{isSceneOrStaged}) from {saveId.Uid} to {newId}");

            saveId.Uid = newId;

            foreach (var c in saveId.gameObject.GetComponentsInChildren<NiBehaviour>())
            {
                c.Uid = Uid.NewUid();
#if UNITY_EDITOR
                EditorUtility.SetDirty(c);
#endif
            }
#if UNITY_EDITOR
            EditorUtility.SetDirty(saveId);
            EditorUtility.SetDirty(saveId.gameObject);
#endif
        }

    }
}