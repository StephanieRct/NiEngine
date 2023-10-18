//using NiEngine.IO.SaveOverrides;
//using System;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using System.Linq;
//using System.Runtime.InteropServices;

//namespace NiEngine.IOold
//{
//    //public struct ObjectGCHandle
//    //{
//    //    GCHandle m_handle;

//    //    public override bool Equals(object other)
//    //    {
//    //        if (other is ObjectGCHandle o)
//    //            return m_handle.Equals(o.m_handle);
//    //        return false;
//    //    }
//    //    public override int GetHashCode()
//    //    {
//    //        return m_handle.GetHashCode();
//    //    }

//    //    object m_Object;
//    //    public ObjectGCHandle(object obj)
//    //    {
//    //        m_Object = obj;
//    //        unsafe
//    //        {
//    //            Marshal.
//    //            System.TypedReference tr = __makeref(obj);
//    //            //System.TypedReference.TargetTypeToken()
//    //            IntPtr ptr = **(IntPtr**)(&tr);
//    //            m_handle = GCHandle.FromIntPtr(ptr);
//    //        }
//    //    }
//    //    public object Object => m_Object;
//    //}
//    public struct SavingState
//    {
//        public bool Success;
//        public string Message;
//        public object Reference;
//        public bool MustStop => !Success;
//        public void Merge(SavingState other)
//        {
//            Success = Success && other.Success;

//            Message = other.Message;
//            Reference = other.Reference;
//            //
//            //if(string.IsNullOrEmpty(Message))
//            //    Message = other.Message;
//            //else if (!string.IsNullOrEmpty(Message))
//            //    Message = $"{Message}\n{other.Message}";
//            //
//            //if(Reference == null)
//            //    Reference = other.Reference;
//            //if (other.Reference != null)
//            //{
//            //    Reference = other.Reference;
//            //}
//        }

//        public static SavingState Successful => new SavingState
//        {
//            Success = true
//        };
//        public static SavingState Failure(string reason, object reference) => new SavingState
//        {
//            Success = false,
//            Message = reason,
//            Reference = reference
//        };
//    }

//    public class SaveContext
//    {
//        public SaveTypeRegistry SaveTypeRegistry;
//        public SavingState State = SavingState.Successful;
//        public bool MustStop => State.MustStop;
//        public bool SafetyChecks = true;
//        public bool StopOnError = true;
//        public bool ThrowOnError = true;
//        public List<object> Scopes = new();
//        private int m_NextUId = 1;
//        public struct SavedObjectReference
//        {
//            //public Id FullId;
//            public int UId;
//            public object Object;
//            public bool InPlace;
//        }

//        //public SortedDictionary<Id, SavedObjectReference> SavedObjectReferences = new();
//        public SortedDictionary<int, SavedObjectReference> SavedObjectReferences = new();
//        public Dictionary<object, int> SavedObjectReferenceIds = new();


//        public SaveContext(SaveTypeRegistry saveTypeRegistry)
//        {
//            SaveTypeRegistry = saveTypeRegistry;
//        }
//        public SaveContext()
//        {
//            SaveTypeRegistry = new();
//        }

//        public bool IsTypePrimitive(Type type)
//        {
//            return type.IsPrimitive || type.IsEnum || type == typeof(string);
//        }


//        public bool IsReferenceType(Type type)
//        {
//            return type.IsInterface
//            || ( type.IsClass 
//                 && !typeof(System.Delegate).IsAssignableFrom(type)
//                 && type != typeof(string));
//        }

//        public object CreateObject(Type type, object[] parameters = null)
//        {
//            if(parameters is not null)
//                return Activator.CreateInstance(type, parameters);
//            return Activator.CreateInstance(type);
//        }
//        public object CreateArray(Type arrayType, int lenght)
//        {
//            return Activator.CreateInstance(arrayType, lenght);
//        }

//        public object CreateArrayOf(Type elementType, int lenght)
//            => CreateArray(elementType.MakeArrayType(), lenght);

//        public string TypeToString(Type type)
//        {
//            return type.AssemblyQualifiedName;
//        }
//        public Type StringToType(string s)
//        {
//            return Type.GetType(s);
//        }

//        int NextUId()
//        {
//            var uid = m_NextUId;
//            ++m_NextUId;
//            return uid;
//        }
//        public Id MakeFullId() => new Id
//        {
//            Elements = Scopes.Select(x=>(object)x.ToString()).ToArray()
//        };
//        public SavedObjectReference AddSavedObjectReference(object obj, bool inPlace)
//        {
//            if(SavedObjectReferenceIds.TryGetValue(obj, out var existingId))
//            {
//                var r = SavedObjectReferences[existingId];
//                r.InPlace |= inPlace;
//                SavedObjectReferences[existingId] = r;
//                return r;
//            }
//            else
//            {
//                return AddSavedObjectReference(NextUId(), obj, inPlace);
//            }

//        }

//        public SavedObjectReference AddSavedObjectReference(int uid, object obj, bool inPlace)
//        {
//            var r = new SavedObjectReference
//            {
//                //FullId = id,
//                UId = uid,
//                Object = obj,
//                InPlace = inPlace,
//            };
//            SavedObjectReferences.Add(uid, r);
//            SavedObjectReferenceIds.Add(obj, uid);
//            //++m_NextUId;
//            return r;
//        }
//        public object GetSavedObjectReference(int uid)//Id fullId)
//        {
//            if (SavedObjectReferences.TryGetValue(uid, out SavedObjectReference r))
//            {
//                return r.Object;
//            }
//            LogError($"Could not find object for reference id '{uid}'");
//            return null;
//        }

//        public void SaveReferencedObjects(IDataOutput io, object key)
//        {
//            using var _ = ScopeKey(key, io);
//            SaveReferencedObjects(io);
//        }
//        public void SaveReferencedObjects(IDataOutput io)
//        {
//            HashSet<int> IdProcessed = new();
//            //HashSet<Id> IdToProcess = new();
//            int index = 0;
//            int newIdCount = 0;
//            do
//            {
//                newIdCount = 0;
//                var IdToProcess = SavedObjectReferences.Keys.ToArray();
//                foreach (var id in IdToProcess)
//                {
//                    if (!IdProcessed.Contains(id))
//                    {
//                        var o = SavedObjectReferences[id];
//                        if (!o.InPlace)
//                        {
//                            SaveKeyObject(id, o.Object.GetType(), o.Object, io);
//                            ++index;
//                        }
//                        ++newIdCount;

//                        IdProcessed.Add(id);
//                        if (MustStop)
//                            return;
//                    }
//                }
//            } while (newIdCount > 0);




//            //var currentSavedObjectReferences = SavedObjectReferences;
//            //do
//            //{
//            //    SavedObjectReferences = new();
//            //    foreach (var o in currentSavedObjectReferences.Values)
//            //    {
//            //        SaveKeyObject(o.FullId, o.Object.GetType(), o.Object, io);
//            //        if (MustStop)
//            //            return;
//            //    }

//            //    foreach (var (k, v) in SavedObjectReferences)
//            //    {
//            //        currentSavedObjectReferences.Add(k, v);
//            //    }

//            //} while (SavedObjectReferences.Count > 0);

//            //SavedObjectReferences = currentSavedObjectReferences;
//        }
//        public void LoadReferencedObjects(IDataInput io, object key)
//        {
//            using var _ = ScopeKey(key, io);
//            LoadReferencedObjects(io);
//        }
//        public void LoadReferencedObjects(IDataInput io)
//        {
//            var keys = io.AllScopeKeys.ToArray();
//            Array.Reverse(keys);
//            foreach (var key in keys)
//            {
//                using var _ = ScopeKey(key, io);
//                var obj = CreateObject(io);
//                if (MustStop)
//                    return;
//                //var r = new SavedObjectReference
//                //{
//                //    //FullId = (Id)scopeKey,
//                //    UId = (int)key,
//                //    Object = obj,
//                //    InPlace = false,
//                //};
//                //SavedObjectReferences.Add(r.UId, r);
//            }

//            foreach (var key in keys)
//            {
//                using var _ = ScopeKey(key, io);
//                var id = (int)key;
//                if (!SavedObjectReferences.TryGetValue(id, out var r))
//                {
//                    LogError($"{nameof(LoadReferencedObjects)}: Missing object");
//                    return;
//                }
//                LoadObjectInPlace(r.Object, io);
//                if (MustStop)
//                    return;

//            }


//            //foreach (var scopeKey in io.AllScopeKeys)
//            //{
//            //    using var _ = ScopeKey(scopeKey, io);
//            //    var obj = CreateObject(io);
//            //    if (MustStop)
//            //        return;
//            //    var r = new SavedObjectReference
//            //    {
//            //        FullId = (Id)scopeKey,
//            //        Object = obj,
//            //        InPlace = false,
//            //    };
//            //    SavedObjectReferences.Add(r.FullId, r);
//            //}
//            //foreach (var scopeKey in io.AllScopeKeys)
//            //{
//            //    using var _ = ScopeKey(scopeKey, io);
//            //    var id = (Id)scopeKey;
//            //    if(!SavedObjectReferences.TryGetValue(id, out var r))
//            //    {
//            //        LogError($"{nameof(LoadReferencedObjects)}: Missing object");
//            //        return;
//            //    }
//            //    LoadObjectInPlace(r.Object, io);
//            //    if (MustStop)
//            //        return;
//            //}
//        }


//        public SaveScope<T> ScopeKey<T>(object key, T io, bool errorOnFail = true)
//            where T : IDataIO
//            => new SaveScope<T>(this, key, io, errorOnFail);

//        public void LogError(string message, object reference = null)
//        {
//            if (StopOnError)
//                State.Success = false;
//            State.Message = message;
//            State.Reference = reference;
//            Debug.LogError(message, reference as UnityEngine.Object);
//            if(ThrowOnError)
//                throw new Exception(message);
//        }
//        public void LogException(string message, Exception e)
//        {
//            if (StopOnError)
//                State.Success = false;
//            State.Message = message;
//            State.Reference = e;
//            Debug.LogError($"{message}\nException: {e}");
//            if (ThrowOnError)
//                throw e;
//        }

//        static Type GetEnumerableElementType(IEnumerable e)
//        {
//            var type = e.GetType();
//            var etype = typeof(IEnumerable<>);
//            foreach (var bt in type.GetInterfaces())
//                if (bt.IsGenericType && bt.GetGenericTypeDefinition() == etype)
//                    return bt.GetGenericArguments()[0];
//            return null;
//        }


//        public void SaveSequenceData(Type eType, IEnumerable enumerable, int count, IDataOutput io)
//        {
//            //using var _ = ScopeKey(TypeToString(eType), io);
//            SaveKeyData("length", typeof(int), count, io);
//            if (IsReferenceType(eType))
//            {
//                int i = 0;
//                var itor = enumerable.GetEnumerator();
//                while (itor.MoveNext() && !MustStop)
//                {
//                    SaveKeyReference(i, itor.Current, io);
//                    ++i;
//                }
//            }
//            else
//            {
//                int i = 0;
//                var itor = enumerable.GetEnumerator();
//                while (itor.MoveNext() && !MustStop)
//                {
//                    SaveKeyData(i, itor.Current, io);
//                    ++i;
//                }
//            }
//        }
//        public IEnumerable LoadSequenceData(Type eType, IDataInput io)
//        {
//            int length = (int)LoadKeyData("length", typeof(int), io);
//            if (IsReferenceType(eType))
//            {
//                for (int i = 0; i < length && !MustStop; ++i)
//                {
//                    var e = LoadKeyReference(i, io);
//                    yield return e;
//                }
//            }
//            else
//            {
//                for (int i = 0; i < length && !MustStop; ++i)
//                {
//                    var e = LoadKeyData(i, eType, io);
//                    yield return e;
//                }
//            }
//        }

//        public void SaveSequenceDataTyped(Type eType, IEnumerable enumerable, int count, IDataOutput io)
//        {
//            using var _ = ScopeKey(TypeToString(eType), io);
//            SaveSequenceData(eType, enumerable, count, io);
//        }

//        public IEnumerable LoadSequenceDataTyped(IDataInput io)
//        {
//            foreach (var scopeKey in io.AllScopeKeys)
//            {
//                using var _ = ScopeKey(scopeKey, io);
//                var type = StringToType(scopeKey as string);
//                if (type is null)
//                {
//                    LogError($"{nameof(LoadSequenceData)}: Could not find type named '{scopeKey}'");
//                    yield break;
//                }

//                foreach (var e in LoadSequenceData(type, io))
//                    yield return e;
//                break;
//            }
//        }
//        //public void SaveKeySequenceData(object keyData, IEnumerable enumerable, int count, IDataOutput io)
//        //    => SaveKeySequenceData(keyData, enumerable.GetType().GetElementType(), enumerable, count, io);
//        public void SaveKeySequenceData(object keyData, Type eType, IEnumerable enumerable, int count, IDataOutput io)
//        {
//            using var scope = ScopeKey(keyData, io);
//            SaveSequenceData(eType, enumerable, count, io);
//        }
//        public IEnumerable<T> LoadKeySequenceData<T>(object keyData, IDataInput io)
//            => (IEnumerable<T>)LoadKeySequenceData(keyData, typeof(T), io);
//        public IEnumerable LoadKeySequenceData(object keyData, Type eType, IDataInput io)
//        {
//            using var scope = ScopeKey(keyData, io);
//            return LoadSequenceData(eType, io);
//        }

//        public void SaveArrayData(Type type, Array array, IDataOutput io)
//        {
//            //var a = new Int16();
//            //var ii = new System.Int8()
//            var eType = type.GetElementType();
//            int length = array.GetLength(0);
//            //SaveKeySequenceData(".ctor", typeof(int), new int[]{length}, 1, io);

//            SaveSequenceData(eType, array, length, io);
//            //SaveKeyData("length", typeof(int), length, io);
//            //if (eType.IsClass)
//            //{
//            //    for (int i = 0; i < length; ++i)
//            //    {
//            //        SaveKeyReference(i, array.GetValue(i), io);
//            //    }
//            //}
//            //else
//            //{
//            //    for (int i = 0; i < length; ++i)
//            //    {
//            //        SaveKeyData(i, eType, array.GetValue(i), io);
//            //    }
//            //}
//        }
//        public Array LoadArrayData(Type type, IDataInput io)
//        {
//            var eType = type.GetElementType();
//            int length = (int)LoadKeyData("length", typeof(int), io);
//            var array = CreateArray(type, length) as Array;
//            int i = 0;
//            foreach (var e in LoadSequenceData(eType, io))
//            {
//                array.SetValue(e, i);
//                ++i;
//            }
//            return array;
//        }
//        public Array LoadArrayDataInPlace(Type type, Array array, IDataInput io)
//        {
//            var eType = type.GetElementType();
//            int i = 0;
//            foreach (var e in LoadSequenceData(eType, io))
//            {
//                array.SetValue(e, i);
//                ++i;
//            }
//            return array;
//        }
        
//        public void SaveData(object data, IDataOutput io)
//            => SaveData(data.GetType(), data, io);
//        public void SaveData(Type type, object data, IDataOutput io)
//        {
//            System.Diagnostics.Debug.Assert(type == data.GetType());
//            if (type.IsArray)
//            {
//                SaveArrayData(type, data as Array, io);
//            }
//            else if (SaveTypeRegistry.TryGetSaveOverride(type, out var sop) && sop != null)
//            {
//                sop.Save(this, type, data, io);
//            }
//            //else if (data is IEnumerable e)
//            //{
//            //    var eType = GetEnumerableElementType(e);
//            //    if (eType == null)
//            //    {
//            //        LogError("");
//            //        if (MustStop)
//            //            return;
//            //    }
//            //    var itor = e.GetEnumerator()
//            //    var g = Enumerable.Count(e);
//            //    SaveSequenceData(eType, e, e.co);
//            //    so.Save(this, io);
//            //}
//            else if (data is ISaveOverride so)
//            {
//                so.Save(this, io);
//            }
//            else if (SaveTypeRegistry.TryGetSaveFallback(type, out var sopFallback) && sopFallback != null)
//            {
//                sopFallback.Save(this, type, data, io);
//            }
//            else if (IsTypePrimitive(type))
//                io.Data(this, type, data);
//            else
//                ReflectionSO.Instance.Save(this, type, data, io);
//        }

//        public T LoadData<T>(IDataInput io)
//            => (T)LoadData(typeof(T), io);
//        public object LoadData(Type type, IDataInput io)
//        {
//            object obj = default;
//            if (type.IsArray)
//            {
//                obj = LoadArrayData(type, io);
//            }
//            else if (SaveTypeRegistry.TryGetSaveOverride(type, out var sop) && sop != null)
//            {
//                obj = sop.Load(this, type, io);
//            }
//            else if (typeof(ISaveOverride).IsAssignableFrom(type))
//            {
//                obj = CreateObject(type);
//                (obj as ISaveOverride).Load(this, io);
//            }
//            else if (SaveTypeRegistry.TryGetSaveFallback(type, out var sopFallback) && sopFallback != null)
//            {
//                obj = sopFallback.Load(this, type, io);
//            }
//            else if (IsTypePrimitive(type))
//                obj = io.Data(this, type);
//            else
//                obj = ReflectionSO.Instance.Load(this, type, io);


//            AddSavedIdObjectReference(obj, io);
//            return obj;
//        }

//        public void LoadDataInPlace(object data, IDataInput io)
//            => LoadDataInPlace(data.GetType(), data, io);
//        public void LoadDataInPlace(Type type, object data, IDataInput io)
//        {
//            System.Diagnostics.Debug.Assert(type == data.GetType());
//            if (type.IsArray)
//            {
//                LoadArrayDataInPlace(type, data as Array, io);
//            }
//            else if (SaveTypeRegistry.TryGetSaveOverride(type, out var sop) && sop != null)
//            {
//                sop.LoadInPlace(this, type, data, io);
//            }
//            else if (data is ISaveOverride so)
//            {
//                so.Load(this, io);
//            }
//            else if (SaveTypeRegistry.TryGetSaveFallback(type, out var sopFallback) && sopFallback != null)
//            {
//                sopFallback.LoadInPlace(this, type, data, io);
//            }
//            else if (IsTypePrimitive(type))
//                io.DataInPlace(this, type, data);
//            else
//                ReflectionSO.Instance.LoadInPlace(this, type, data, io);
//        }

//        public void SaveKeyData(object keyData, object valueData, IDataOutput io)
//            => SaveKeyData(keyData, valueData.GetType(), valueData, io);
//        public void SaveKeyData(object keyData, Type valueType, object valueData, IDataOutput io)
//        {
//            System.Diagnostics.Debug.Assert(valueType == valueData.GetType());
//            using var scope = ScopeKey(keyData, io);
//            SaveData(valueType, valueData, io);
//        }
//        public T LoadKeyData<T>(object keyData, IDataInput io)
//            => (T)LoadKeyData(keyData, typeof(T), io);
//        public object LoadKeyData(object keyData, Type valueType, IDataInput io)
//        {
//            using var scope = ScopeKey(keyData, io);
//            return LoadData(valueType, io);
//        }
//        public void LoadKeyDataInPlace(object keyData, Type valueType, object valueData, IDataInput io)
//        {
//            System.Diagnostics.Debug.Assert(valueType == valueData.GetType());
//            using var scope = ScopeKey(keyData, io);
//            LoadDataInPlace(valueType, valueData, io);
//        }

//        #region Reference /////////////////////////////////////////////////////////////////////////////////////////////

//        public void SaveReference(object obj, IDataOutput io)
//        {
//            if (obj == null)
//            {
//                //SaveData(typeof(Id), Id.Null, io);
//                SaveData(typeof(int), 0, io);
//            }
//            else
//            {
//                var r = AddSavedObjectReference(obj, inPlace: false);
//                SaveData(typeof(int), r.UId, io);
//            }
//        }
//        public object LoadReference(IDataInput io)
//        {
            
//            var id = LoadData<int>(io);
//            if (id == 0)
//                return null;
//            if (MustStop)
//                return null;
//            return GetSavedObjectReference(id);
//        }

//        public void SaveKeyReference(object keyData, object obj, IDataOutput io)
//        {
//            using var scope = ScopeKey(keyData, io);
//            SaveReference(obj, io);
//        }
//        public object LoadKeyReference(object keyData, IDataInput io)
//        {
//            using var scope = ScopeKey(keyData, io);
//            return LoadReference(io);
//        }
//        #endregion







//        //public void SaveObject(object obj, IDataOutput io)
//        //    => SaveObject(obj.GetType(), obj, io);
//        public void SaveObject(Type objectType, object obj, IDataOutput io)
//        {
//            System.Diagnostics.Debug.Assert(objectType == obj.GetType());
//            var r = AddSavedObjectReference(obj, true);
//            SaveKeyData("~type", TypeToString(objectType), io);
//            SaveKeyData("~id", r.UId, io);
//            //using var _ = ScopeKey(TypeToString(objectType), io);
//            SaveData(objectType, obj, io);
//        }
//        //public void SaveObject(Id id, Type objectType, object obj, IDataOutput io)
//        //{
//        //    System.Diagnostics.Debug.Assert(objectType == obj.GetType());
//        //    AddSavedObjectReference(obj, true);
//        //    SaveKeyData("~type", TypeToString(objectType), io);
//        //    SaveKeyData("~id", id, io);
//        //    //using var _ = ScopeKey(TypeToString(objectType), io);
//        //    SaveData(objectType, obj, io);
//        //}
//        //public SaveScope<IDataInput> LoadTypeScope(IDataInput io, out Type type)
//        //{
//        //    foreach (var scopeKey in io.AllScopeKeys)
//        //    {
//        //        var scope = ScopeKey(scopeKey, io);
//        //        type = StringToType(scopeKey as string);
//        //        if (type is null)
//        //        {
//        //            LogError($"{nameof(LoadType)}: Could not find type named '{scopeKey}'");
//        //            return default;
//        //        }
//        //        return scope;
//        //    }
//        //    LogError($"{nameof(LoadType)}: Could not load type. No scope found.");
//        //    type = null;
//        //    return default;
//        //}
//        public Type LoadType(IDataInput io)
//        {
//            var typeString = LoadKeyData<string>("~type", io);
//            if (typeString is null)
//            {
//                LogError($"{nameof(LoadType)}: Could not load type. No type key found.");
//                return null;
//            }

//            var type = StringToType(typeString);
//            if (type is null)
//            {
//                LogError($"{nameof(LoadType)}: Could not find type named '{typeString}'");
//                return null;
//            }
//            return type;

//            //foreach (var scopeKey in io.AllScopeKeys)
//            //{
//            //    using var _ = ScopeKey(scopeKey, io);
//            //    var type = StringToType(scopeKey as string);
//            //    if (type is null)
//            //    {
//            //        LogError($"{nameof(LoadType)}: Could not find type named '{scopeKey}'");
//            //        return null;
//            //    }
//            //    return type;
//            //}
//            //LogError($"{nameof(LoadType)}: Could not load type. No scope found.");
//            //return null;
//        }

//        void AddSavedIdObjectReference(object obj, IDataInput io)
//        {
//            using (var idScope = ScopeKey("~id", io, errorOnFail: false))
//            {
//                if (idScope.Success)
//                {
//                    var id = LoadData<int>(io);
//                    if (MustStop)
//                        return;
//                    AddSavedObjectReference(id, obj, inPlace: true);
//                }
//            }
//        }
//        public object CreateObject(IDataInput io)//, object[] parameters = null)
//        {
//            var type = LoadType(io);
//            if (type is null)
//                return null;


//            object obj;
//            if (type.IsArray)
//            {
//                var l = LoadKeyData<int>("length", io);
//                obj = CreateArray(type, l);
//            }
//            else
//            {
//                obj = CreateObject(type);
//            }
//            if (MustStop)
//                return obj;
//            AddSavedIdObjectReference(obj, io);
//            return obj;
//            //using (var _ = LoadTypeScope(io, out var type))
//            //{
//            //    if (type.IsArray)
//            //    {
//            //        var l = LoadKeyData<int>("length", io);
//            //        return CreateArray(type, l);
//            //    }
//            //    else
//            //    {
//            //        return CreateObject(type);
//            //    }
//            //}

//            //using (var __ = LoadTypeScope(io, out var type))
//            //{

//            //    if (type.IsArray)
//            //    {
//            //        CreateArray()
//            //    }
//            //}
//            //Type type = LoadType(io);
//            //using (var scopeCtor = ScopeKey(".ctor", io, errorOnFail: false))
//            //{
//            //    if (scopeCtor.Success)
//            //    {
//            //        //List<object> ps = new List<object>();
//            //        //foreach (var e in LoadSequenceData(io))
//            //        //{
//            //        //
//            //        //}
//            //        parameters = LoadSequenceData(io).Cast<object>().ToArray();
//            //        //parameters =
//            //        //    (LoadSequenceData(io) as IEnumerable<object>).ToArray(); // as object[];
//            //    }
//            //}

//            //var type = LoadType(io);
//            //if (MustStop)
//            //    return null;

//            //return CreateObject(type, parameters);
//        }
//        public object LoadObject(IDataInput io)
//        {
//            var type = LoadType(io);
//            if (type is null)
//                return null;

//            return LoadData(type, io);

//            //using (var _ = LoadTypeScope(io, out var type))
//            //{
//            //    return LoadData(type, io);
//            //}

//            //foreach (var scopeKey in io.AllScopeKeys)
//            //{
//            //    using var _ = ScopeKey(scopeKey, io);
//            //    var type = StringToType(scopeKey as string);
//            //    if (type is null)
//            //    {
//            //        LogError($"{nameof(LoadObject)}: Could not find type named '{scopeKey}'");
//            //        return null;
//            //    }
//            //    return LoadData(type, io);
//            //}
//            //LogError($"{nameof(LoadObject)}: Could not load object. No object scope found.");
//            //return null;
//        }
//        public object LoadObject(Type type, IDataInput io)
//        {
//            return LoadData(type, io);
//            //foreach (var scopeKey in io.AllScopeKeys)
//            //{
//            //    using var _ = ScopeKey(scopeKey, io);
//            //    //var type = StringToType(scopeKey as string);
//            //    if (type is null)
//            //    {
//            //        LogError($"{nameof(LoadObject)}: Could not find type named '{scopeKey}'");
//            //        return null;
//            //    }
//            //    return LoadData(type, io);
//            //}
//            //LogError($"{nameof(LoadObject)}: Could not load object. No object scope found.");
//            //return null;
//        }
//        public void LoadObjectInPlace(object obj, IDataInput io)
//        {
//            var type = LoadType(io);
//            if (type is null)
//                return;
//            if (type != obj.GetType())
//            {
//                LogError($"{nameof(LoadObjectInPlace)}: Could not load object in place, type mismatch. In-place type: '{obj.GetType().FullName}', saved type: '{type.FullName}'");
//                return;
//            }
//            LoadDataInPlace(type, obj, io);

//            //foreach (var scopeKey in io.AllScopeKeys)
//            //{
//            //    using var _ = ScopeKey(scopeKey, io);
//            //    var type = StringToType(scopeKey as string);
//            //    if (type is null)
//            //    {
//            //        LogError($"{nameof(LoadObjectInPlace)}: Could not find type named '{scopeKey}'");
//            //        return;
//            //    }
//            //    if (type != obj.GetType())
//            //    {
//            //        LogError($"{nameof(LoadObjectInPlace)}: Could not load object in place, type mismatch. In-place type: '{obj.GetType().FullName}', saved type: '{type.FullName}'");
//            //        return;
//            //    }
//            //    LoadDataInPlace(type, obj, io);
//            //    return;
//            //}
//            //LogError($"{nameof(LoadObjectInPlace)}: Could not load object. No object scope found.");
//        }







//        //public void SaveKeyObject(object keyData, object valueData, IDataOutput io)
//        //    => SaveKeyObject(keyData, valueData.GetType(), valueData, io);
//        public void SaveKeyObject(object keyData, Type valueType, object valueData, IDataOutput io)
//        {
//            System.Diagnostics.Debug.Assert(valueType == valueData.GetType());
//            using var scope = ScopeKey(keyData, io);
//            SaveObject(valueType, valueData, io);
//        }
//        //public void SaveKeyObject(object keyData, Id id, Type valueType, object valueData, IDataOutput io)
//        //{
//        //    System.Diagnostics.Debug.Assert(valueType == valueData.GetType());
//        //    using var scope = ScopeKey(keyData, io);
//        //    SaveObject(id, valueType, valueData, io);
//        //}
//        public object LoadKeyObject(object keyData, IDataInput io)
//        {
//            using var scope = ScopeKey(keyData, io);
//            return LoadObject(io);
//        }
//        public void LoadKeyObjectInPlace(object keyData, object valueData, IDataInput io)
//        {
//            using var scope = ScopeKey(keyData, io);
//            LoadObjectInPlace(valueData, io);
//        }


//    }

//    public struct SaveScope<T> : IDisposable
//        where T : IDataIO
//    {
//        public object Key;
//        public T IO;
//        public SaveContext Context;
//        public bool Success;
//        //public bool ErrorOnFail { get; private set; }
//        public SaveScope(SaveContext context, object key, T io, bool errorOnFail= true)
//        {
//            Context = context;
//            Key = key;
//            IO = io;
//            //ErrorOnFail = errorOnFail;
//            Context.Scopes.Add(Key);
//            if (!IO.ScopeBegin(Context, Key))
//            {
//                if(errorOnFail)
//                    Context.LogError($"Failed to begin scope '{key}'", key);
//                Context.Scopes.RemoveAt(Context.Scopes.Count - 1);
//                Success = false;
//            }
//            else
//                Success = true;

//        }
//        public void Dispose()
//        {
//            if (Success)
//            {
//                IO.ScopeEnd(Context, Key);
//                var k = Context.Scopes[^1];
//                Context.Scopes.RemoveAt(Context.Scopes.Count - 1);
//#if UNITY_EDITOR
//                if (!k.Equals(Key))
//                {
//                    Debug.LogError($"SaveScope bad match\nReceived: {Key}\nShould be: {k}");
//                }
//#endif
//            }
//        }
//    }
//}