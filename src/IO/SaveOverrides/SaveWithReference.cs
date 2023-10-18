using NiEngine.Expressions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NiEngine.IO.SaveOverrides
{
    public abstract class SaveWithReference<T, TMetaData> : ISaveOverrideProxy
        where T : class
        where TMetaData : new()
    {
        // return the Uid for the saved object
        //protected abstract void BeforeLoad(StreamContext context, IInput io);

        public abstract bool TryGetUid(T obj, out Uid uid);

        protected abstract void SaveObjectData(StreamContext context, Type type, T obj, ref TMetaData metaData, IOutput io);
        protected abstract void LoadObjectData(StreamContext context, Type type, Uid uid, ref T obj, TMetaData metaData, IInput io);
        protected abstract bool TryInstantiate(StreamContext context, IInput io, Uid uid, TMetaData metaData, IDictionary<Uid, T> addAllNewObjects, out T obj);
        protected abstract void DeleteAllInstantiated();
        protected abstract IDictionary<Uid, T> GetAllExistingObjects();
        protected abstract void DeleteObject(Uid uid, T obj);




        public SortedDictionary<Uid, T> ReferencedObjects = new();
        public SortedDictionary<Uid, T> SavedObjectByUid = new();
        public SortedDictionary<Uid, TMetaData> SavedObjectMeta = new();
        public List<Uid> Instantiate = new();
        protected IDictionary<Uid, T> m_AllObjects;
        public Func<T, bool> IsNull;
        public T FindObjectByUid(Uid uid)
        {
            if (m_AllObjects.TryGetValue(uid, out T obj))
                return obj;
            return default;
        }
        public virtual SupportType IsSupportedType(Type type)
        {
            return typeof(T).IsAssignableFrom(type) ? SupportType.Supported : SupportType.Unsupported;
        }

        //public SaveReference<T, TMetaData> AsReference() => new SaveReference<T, TMetaData>(this);

        protected bool TrySaveWithOverride(StreamContext context, Type type, T obj, IOutput io)
        {
            if (obj is ISaveOverride sa)
            {
                sa.Save(context, io);
                return true;
            }
            return false;
        }
        protected bool TryLoadInPlaceWithOverride(StreamContext context, T obj, IInput io)
        {
            if (obj is ISaveOverride sa)
            {
                sa.Load(context, io);
                return true;
            }
            return false;
        }

        bool LogError(StreamContext context, string method, string msg)
        {
            return context.LogError($"{GetType().FullName}.{method}: {msg}");
        }
        bool LogFailure(StreamContext context, string method, string msg)
        {
            return context.LogFailure($"{GetType().FullName}.{method}: {msg}");
        }

        public void Save(StreamContext context, Type type, object obj, IOutput io)
        {
            var o = obj as T;
            if (!(IsNull?.Invoke(o) ?? o == null))
            {
                if (!TryGetUid(o, out var uid))
                {
                    LogError(context, nameof(Save), $"Could not save a reference to object '{o}'. It has no associated Uid."
                        + ((o is GameObject go) ? $"Add a SaveId component to the GameObject '{go.GetPathNameOrNull()}'" : ""));
                    return;
                }
                io.Save(context, "ref", uid);
                ReferencedObjects.TryAdd(uid, o);
            }
            else
            {
                io.Save(context, "ref", Uid.Default);
            }
        }

        public object Load(StreamContext context, Type type, IInput io)
        {
            if (io.TryLoad(context, "ref", out Uid uidRef))
            {
                if (uidRef.IsDefault)
                    return null;
                if (ReferencedObjects.TryGetValue(uidRef, out var o))
                    return o;
                LogError(context, nameof(Load), $"Could not find referenced GameObejct with uid {uidRef}.");
            }
            LogError(context, nameof(Load), $"Could not load GameObejct ref.");
            return default;
        }

        //public void Save(StreamContext context, Type type, object obj, IOutput io)
        //{
        //    var o = obj as T;
        //    var metaData = new TMetaData();
        //    if (!TryGetUid(o, out var uid))
        //    {
        //        LogError(context, nameof(Save), $"Could not save object '{o}'. It has no associated Uid."
        //            + ((o is GameObject) ? "Add a SaveId component to the GameObject" : ""));
        //        return;
        //    }
        //    io.Save(context, "uid", uid);
        //    Debug.Log($"Save uid:{uid} object:{o}", o as UnityEngine.Object);
        //    SaveObjectData(context, type, o, ref metaData, io);
        //    SavedObjectByUid.Add(uid, o);
        //    SavedObjectMeta.Add(uid, metaData);
        //}

        //public object Load(StreamContext context, Type type, IInput io)
        //{
        //    if (io.TryLoad(context, "ref", out Uid uidRef))
        //    {
        //        if (uidRef.IsDefault)
        //            return null;
        //        if (ReferencedObjects.TryGetValue(uidRef, out var o))
        //            return o;
        //        LogError(context, nameof(Load), $"Could not find referenced GameObejct with uid {uidRef}.");
        //    }
        //    else if (io.TryLoad(context, "uid", out Uid uid))
        //    {
        //        if (SavedObjectByUid.TryGetValue(uid, out var o))
        //        {
        //            LoadInPlace(context, ref o, io);
        //            return o;
        //        }
        //        LogError(context, nameof(Load), $"Could not load GameObejct. GameObject with uid {uid} not found");
        //    }
        //    LogError(context, nameof(Load), $"Could not load GameObejct without Uid.");
        //    return default;
        //}

        public void SaveInPlace(StreamContext context, Type type, object obj, IOutput io)
        {
            if (obj is T objT)
            {
                if (TryGetUid(objT, out var uid))
                {
                    //var type = objT.GetType();
                    io.Save(context, "uid", uid);
                    var metaData = new TMetaData();
                    SaveObjectData(context, type, objT, ref metaData, io);
                    SavedObjectByUid.Add(uid, objT);
                    SavedObjectMeta.Add(uid, metaData);
                }
            }
            else
                context.LogError($"Failed to save in place", obj);
        }
        public void LoadInPlace(StreamContext context, Type type, ref object data, IInput io)
        {
            var o = data as T;
            LoadInPlace(context, ref o, io);
            data = o;
        }

        public void LoadInPlace(StreamContext context, ref T o, IInput io)
        {
            var uid = io.Load<Uid>(context, "uid");
            var metaData = SavedObjectMeta[uid];
            LoadObjectData(context, o.GetType(), uid, ref o, metaData, io);

        }


        public void SaveMetaData(StreamContext context, IOutput io)
        {
            var referencedObjects = ReferencedObjects.Keys.ToArray();
            io.Save(context, "ReferencedObjects", referencedObjects);
            io.Save(context, "ObjectMeta", SavedObjectMeta);
            io.Save(context, "Instantiate", Instantiate);


        }
        public void LoadMetaData(StreamContext context, IInput io)
        {
            DeleteAllInstantiated();
            m_AllObjects = GetAllExistingObjects();
            SavedObjectMeta = io.Load<SortedDictionary<Uid, TMetaData>>(context, "ObjectMeta");
            Instantiate = io.Load<List<Uid>>(context, "Instantiate");

            SavedObjectByUid = new();


            // Instantiate new objects
            foreach (var uid in Instantiate)
            {
                if (SavedObjectMeta.TryGetValue(uid, out var md))
                {
                    if (TryInstantiate(context, io, uid, md, m_AllObjects, out var obj))
                    {
                        SavedObjectByUid[uid] = obj;
                    }
                    else
                        LogFailure(context, nameof(LoadMetaData), $"Could not instantiate GameObject with Uid {uid}");
                }
            }

            // Find all saved GameObject currently existing in the scene
            foreach (var (uid, md) in SavedObjectMeta)
            {
                if (!SavedObjectByUid.ContainsKey(uid))
                {
                    if (m_AllObjects.TryGetValue(uid, out var obj))
                    {
                        SavedObjectByUid[uid] = obj;
                    }
                    else if (LogFailure(context, nameof(LoadMetaData), $"Could not find saved object with Uid {uid} and is not instantiated"))
                        return;
                }
            }

            // Find all referenced objects from the save
            var referencedObjects = io.Load<Uid[]>(context, "ReferencedObjects");
            ReferencedObjects = new();
            foreach (var uid in referencedObjects)
            {
                if (m_AllObjects.TryGetValue(uid, out var obj))
                {
                    ReferencedObjects.TryAdd(uid, obj);
                }
                else if (LogFailure(context, nameof(LoadMetaData), $"Could not find referenced object with Uid {uid}"))
                    return;
            }

        }

    }
    //public class SaveReference<T, TMetaData> : ISaveOverrideProxy
    //    where T : class
    //    where TMetaData : new()
    //{
    //    public SaveWithReference<T, TMetaData> Source;
    //    public SaveReference(SaveWithReference<T, TMetaData> source)
    //    {
    //        Source = source;
    //    }
    //    public bool IsSupportedType(Type type)
    //        => Source.IsSupportedType(type);

    //    bool LogError(StreamContext context, string method, string msg)
    //    {
    //        return context.LogError($"{GetType().Name}.{method}: {msg}");
    //    }
    //    bool LogFailure(StreamContext context, string method, string msg)
    //    {
    //        return context.LogFailure($"{GetType().Name}.{method}: {msg}");
    //    }
    //    public void Save(StreamContext context, Type type, object obj, IOutput io)
    //    {
    //        var o = obj as T;
    //        if (!(Source.IsNull?.Invoke(o) ?? o == null))
    //        {
    //            if (!Source.TryGetUid(o, out var uid))
    //            {
    //                LogError(context, nameof(Save), $"Could not save a reference to object '{o}'. It has no associated Uid."
    //                    + ((o is GameObject) ? "Add a SaveId component to the GameObject" : ""));
    //                return;
    //            }
    //            io.Save(context, "ref", uid);
    //            Source.ReferencedObjects.TryAdd(uid, o);
    //        }
    //        else
    //        {
    //            io.Save(context, "ref", Uid.Default);
    //        }
    //    }

    //    public object Load(StreamContext context, Type type, IInput io)
    //    {
    //        if (io.TryLoad(context, "ref", out Uid uidRef))
    //        {
    //            if (uidRef.IsDefault)
    //                return null;
    //            if (Source.ReferencedObjects.TryGetValue(uidRef, out var o))
    //                return o;
    //            LogError(context, nameof(Load), $"Could not find referenced GameObejct with uid {uidRef}.");
    //        }
    //        else if (io.TryLoad(context, "uid", out Uid uid))
    //        {
    //            if (Source.SavedObjectByUid.TryGetValue(uid, out var o))
    //            {
    //                LoadInPlace(context, ref o, io);
    //                return o;
    //            }
    //            LogError(context, nameof(Load), $"Could not load GameObejct. GameObject with uid {uid} not found");
    //        }
    //        LogError(context, nameof(Load), $"Could not load GameObejct without Uid.");
    //        return default;
    //    }

    //    public void LoadInPlace(StreamContext context, Type type, ref object data, IInput io)
    //    {
    //        var o = data as T;
    //        LoadInPlace(context, ref o, io);
    //        data = o;
    //    }

    //    public void LoadInPlace(StreamContext context, ref T o, IInput io)
    //    {
    //        var refUid = io.Load<Uid>(context, "ref");
    //        if (refUid.IsDefault)
    //            o = null;
    //        else
    //            o = Source.FindObjectByUid(refUid);
    //    }
    //    public void SaveInPlace(StreamContext context, object obj, IOutput io)
    //    {
    //        Save(context, obj.GetType(), obj, io);
    //    }

    //}

}