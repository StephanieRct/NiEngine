using NiEngine.IO.SaveOverrides;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

namespace NiEngine.IO
{
    public class TypeRegistry
    {
        public ISaveOverrideProxy ArrayOverride;
        public Dictionary<Type, ISaveOverrideProxy> Overrides = new();
        public Dictionary<Type, ISaveOverrideProxy> Fallbacks = new();
        public HashSet<Type> Ignored = new();
        public ISaveOverrideProxy FinalFallback;

        public bool TypeHasSaveOverride(Type t)
            => Overrides.ContainsKey(t) || Fallbacks.Any(x => x.Key.IsAssignableFrom(t));
        

        /// <summary>
        /// These Overrides are the first to be considered before ISaveClassOverrideProxy and Fallbacks
        /// Types must match exactly
        /// </summary>
        public void RegisterSaveOverride(Type type, ISaveOverrideProxy saveOverride)
        {
            Overrides[type] = saveOverride;
        }
        public bool TryGetSaveOverride(Type type, out ISaveOverrideProxy saveClassOverride, out bool ignore)
        { 
            if(type.IsGenericType)
            {
                var genericDefinition = type.GetGenericTypeDefinition();
                if (Overrides.TryGetValue(genericDefinition, out saveClassOverride))
                {
                    ignore = false;
                    return true;
                }
            }
            if(Overrides.TryGetValue(type, out saveClassOverride))
            {
                ignore = false;
                return true;
            }
            ignore = false;
            return false;
        }


        /// <summary>
        /// These Fallbacks SaveOverride are the last to be considered after Overrides and ISaveOverride.
        /// The first type that can be assigned to will be used.
        /// </summary>
        public void RegisterSaveFallback(Type type, ISaveOverrideProxy saveOverride)
        {
            Fallbacks[type] = saveOverride;
        }
        public void RegisterIgnored(Type type)
        {
            Ignored.Add(type);
        }

        public bool TryGetSaveFallback(Type type, out ISaveOverrideProxy saveClassOverride, out bool ignore)
        {
            if (type.IsGenericType)
            {
                var genericDefinition = type.GetGenericTypeDefinition();

                foreach (var (t, sop) in Fallbacks)
                {
                    if (t.IsAssignableFrom(genericDefinition))
                    {
                        saveClassOverride = sop;
                        switch (sop.IsSupportedType(type))
                        {
                            case SupportType.Unsupported:
                                ignore = false;
                                return false;
                            case SupportType.Supported:
                                ignore = false;
                                return true;
                            case SupportType.Ignored:
                                ignore = true;
                                return true;
                        }
                    }
                }
            }
            foreach (var (t, sop) in Fallbacks)
            {
                if (t.IsAssignableFrom(type))
                {
                    saveClassOverride = sop;
                    switch (sop.IsSupportedType(type))
                    {
                        case SupportType.Unsupported:
                            ignore = false;
                            return false;
                        case SupportType.Supported:
                            ignore = false;
                            return true;
                        case SupportType.Ignored:
                            ignore = true;
                            return true;
                    }
                }
            }
            saveClassOverride = default;
            ignore = false;
            return false;
        }
        public bool IsIgnored(Type type)
        {
            if (type.IsGenericType)
            {
                var genericDefinition = type.GetGenericTypeDefinition();
                foreach (var t in Ignored)
                {
                    if (t.IsAssignableFrom(genericDefinition))
                    {
                        return true;
                    }
                }
            }
            foreach (var t in Ignored)
            {
                if (t.IsAssignableFrom(type))
                {
                    return true;
                }
            }
            return false;
        }


        public SupportType IsSupportedType(Type type)
        {
            if (type.IsArray && ArrayOverride != null)
            {
                var result = ArrayOverride.IsSupportedType(type);
                if (result != SupportType.Unsupported)
                    return result;
            }

            if (TryGetSaveOverride(type, out var sop, out var ignore0) && sop != null)
            {
                if (ignore0) return SupportType.Ignored;
                var result = sop.IsSupportedType(type);
                if (result != SupportType.Unsupported)
                    return result;
            }

            if (TryGetSaveFallback(type, out var sopFallback, out var ignore1) && sopFallback != null)
            {
                if (ignore1) return SupportType.Ignored;
                var result = sopFallback.IsSupportedType(type);
                if (result != SupportType.Unsupported)
                    return result;
            }
            if (typeof(ISaveOverride).IsAssignableFrom(type))
                return SupportType.Supported;
            if (IsIgnored(type))
                return SupportType.Ignored;

            if (FinalFallback != null)
                return FinalFallback.IsSupportedType(type);
            return SupportType.Unsupported;
        }

        public void SaveData(StreamContext context, IOutput baseStream, IOutput nestedStream, object key, Type type, object value)
        {
            if (type.IsArray && ArrayOverride != null)
            {
                using var _ = context.ScopeKey(key, nestedStream);
                ArrayOverride.Save(context, type, value, nestedStream);
            }
            else if (TryGetSaveOverride(type, out var sop, out var ignore0) && sop != null)
            {
                if(ignore0) return;
                using var _ = context.ScopeKey(key, nestedStream);
                sop.Save(context, type, value, nestedStream);
            }
            else if (TryGetSaveFallback(type, out var sopFallback, out var ignore1) && sopFallback != null)
            {
                if (ignore1) return;
                using var _ = context.ScopeKey(key, nestedStream);
                sopFallback.Save(context, type, value, nestedStream);
            }
            else if (value is ISaveOverride so)
            {
                using var _ = context.ScopeKey(key, nestedStream);
                so.Save(context, nestedStream);
            }
            else if (!IsIgnored(type))
            {
                if (baseStream.IsSupportedType(type))
                {
                    baseStream.Save(context, key, type, value);
                }
                else if (FinalFallback != null && FinalFallback.IsSupportedType(type) == SupportType.Supported)
                {
                    using var _ = context.ScopeKey(key, nestedStream);
                    FinalFallback.Save(context, type, value, nestedStream);
                }
            }
        }

        public void SaveInPlace(StreamContext context, IOutput baseStream, IOutput nestedStream, object key, Type type, object value)
        {
            if (type.IsArray && ArrayOverride != null)
            {
                using var _ = context.ScopeKey(key, nestedStream);
                ArrayOverride.SaveInPlace(context, type, value, nestedStream);
            }
            else if (TryGetSaveOverride(type, out var sop, out var ignore0) && sop != null)
            {
                if (ignore0) return;
                using var _ = context.ScopeKey(key, nestedStream);
                sop.SaveInPlace(context, type, value, nestedStream);
            }
            else if (TryGetSaveFallback(type, out var sopFallback, out var ignore1) && sopFallback != null)
            {
                if (ignore1) return;
                using var _ = context.ScopeKey(key, nestedStream);
                sopFallback.SaveInPlace(context, type, value, nestedStream);
            }
            else if (value is ISaveOverride so)
            {
                using var _ = context.ScopeKey(key, nestedStream);
                so.Save(context, nestedStream);
            }
            else if (!IsIgnored(type))
            {
                if (baseStream.IsSupportedType(type))
                {
                    baseStream.SaveInPlace(context, key, type, value);
                }
                else if (FinalFallback != null && FinalFallback.IsSupportedType(type) == SupportType.Supported)
                    {
                    
                    using var _ = context.ScopeKey(key, nestedStream);
                    FinalFallback.SaveInPlace(context, type, value, nestedStream);
                }
            }
        }
        public T Load<T>(StreamContext context, IInput baseStream, IInput nestedStream, object key)
            => (T)Load(context, baseStream, nestedStream, key, typeof(T));
        public object Load(StreamContext context, IInput baseStream, IInput nestedStream, object key, Type type)
        {
            object obj = null;
            if (type.IsArray)
            {
                using var _ = context.ScopeKey(key, nestedStream);
                obj = ArrayOverride.Load(context, type, nestedStream);
            }
            else if (TryGetSaveOverride(type, out var sop, out var ignore0) && sop != null)
            {
                if (ignore0) return null;
                using var _ = context.ScopeKey(key, nestedStream);
                obj = sop.Load(context, type, nestedStream);
            }
            else if (TryGetSaveFallback(type, out var sopFallback, out var ignore1) && sopFallback != null)
            {
                if (ignore1) return null;
                using var _ = context.ScopeKey(key, nestedStream);
                obj = sopFallback.Load(context, type, nestedStream);
            }
            else if (typeof(ISaveOverride).IsAssignableFrom(type))
            {
                using var _ = context.ScopeKey(key, nestedStream);
                obj = context.CreateObject(type);
                (obj as ISaveOverride).Load(context, nestedStream);
            }
            else if (!IsIgnored(type))
            {
                if (baseStream.IsSupportedType(type))
                {
                    obj = baseStream.Load(context, key, type);
                }
                else if (FinalFallback != null && FinalFallback.IsSupportedType(type) == SupportType.Supported)
                {
                    using var _ = context.ScopeKey(key, nestedStream);
                    obj = FinalFallback.Load(context, type, nestedStream);
                }
            }

            return obj;

        }
        public bool TryLoad<T>(StreamContext context, IInput baseStream, IInput nestedStream, object key, out T obj)
        {
            if (TryLoad(context, baseStream, nestedStream, key, typeof(T), out object obj2))
            {
                obj = (T)obj2;
                return true;
            }
            obj = default;
            return false;
        }
        public bool TryLoad(StreamContext context, IInput baseStream, IInput nestedStream, object key, Type type, out object obj)
        {
            if (type.IsArray)
            {
                using var keyScope = context.ScopeKey(key, nestedStream, errorOnFail: false);
                if (keyScope.Success)
                {
                    obj = ArrayOverride.Load(context, type, nestedStream);
                    return true;
                }
            }
            else if (TryGetSaveOverride(type, out var sop, out var ignore0) && sop != null)
            {
                if (ignore0)
                {
                    obj = default;
                    return false;
                }
                using var keyScope = context.ScopeKey(key, nestedStream, errorOnFail: false);
                if (keyScope.Success)
                {
                    obj = sop.Load(context, type, nestedStream);
                    return true;
                }
            }
            else if (TryGetSaveFallback(type, out var sopFallback, out var ignore1) && sopFallback != null)
            {
                if (ignore1)
                {
                    obj = default;
                    return false;
                }
                using var keyScope = context.ScopeKey(key, nestedStream, errorOnFail: false);
                if (keyScope.Success)
                {
                    obj = sopFallback.Load(context, type, nestedStream);
                    return true;
                }
            }
            else if (typeof(ISaveOverride).IsAssignableFrom(type))
            {
                using var keyScope = context.ScopeKey(key, nestedStream, errorOnFail: false);
                if (keyScope.Success)
                {
                    obj = context.CreateObject(type);
                    (obj as ISaveOverride).Load(context, nestedStream);
                    return true;
                }
            }
            else if (!IsIgnored(type))
            {
                if (baseStream.IsSupportedType(type))
                {
                    return baseStream.TryLoad(context, key, type, out obj);
                }
                else if (FinalFallback != null && FinalFallback.IsSupportedType(type) == SupportType.Supported)
                {
                    using var keyScope = context.ScopeKey(key, nestedStream, errorOnFail: false);
                    if (keyScope.Success)
                    {
                        obj = FinalFallback.Load(context, type, nestedStream);
                        return true;
                    }
                }
            }
            obj = default;
            return false;

        }
        public void LoadInPlace<T>(StreamContext context, IInput baseStream, IInput nestedStream, object key, ref T target)
        {
            Type type = target?.GetType() ?? typeof(T);
            if (type.IsArray)
            {
                using var _ = context.ScopeKey(key, nestedStream);
                object obj = target;
                ArrayOverride.LoadInPlace(context, type, ref obj, nestedStream);
                target = (T)obj;
            }
            else if (TryGetSaveOverride(type, out var sop, out var ignore0) && sop != null)
            {
                if (ignore0) return;
                using var _ = context.ScopeKey(key, nestedStream);
                object obj = target;
                sop.LoadInPlace(context, type, ref obj, nestedStream);
                target = (T)obj;
            }
            else if (TryGetSaveFallback(type, out var sopFallback, out var ignore1) && sopFallback != null)
            {
                if (ignore1) return;
                using var _ = context.ScopeKey(key, nestedStream);
                object obj = target;
                sopFallback.LoadInPlace(context, type, ref obj, nestedStream);
                target = (T)obj;
            }
            else if (typeof(ISaveOverride).IsAssignableFrom(type))
            {
                using var _ = context.ScopeKey(key, nestedStream);
                (target as ISaveOverride).Load(context, nestedStream);
            }
            else if (!IsIgnored(type))
            {
                if (baseStream.IsSupportedType(type))
                {
                    baseStream.LoadInPlace<T>(context, key, ref target);
                }
                else if (FinalFallback != null && FinalFallback.IsSupportedType(type) == SupportType.Supported)
                {
                    using var _ = context.ScopeKey(key, nestedStream);
                    object obj = target;
                    FinalFallback.LoadInPlace(context, type, ref obj, nestedStream);
                    target = (T)obj;
                }
            }
        }
        public void LoadInPlace(StreamContext context, IInput baseStream, IInput nestedStream, object key, Type type, ref object target)
        {
            if (type.IsArray)
            {
                using var keyScope = context.ScopeKey(key, nestedStream);
                ArrayOverride.LoadInPlace(context, type, ref target, nestedStream);
            }
            else if (TryGetSaveOverride(type, out var sop, out var ignore0) && sop != null)
            {
                if (ignore0) return;
                using var keyScope = context.ScopeKey(key, nestedStream);
                sop.LoadInPlace(context, type, ref target, nestedStream);
            }
            else if (TryGetSaveFallback(type, out var sopFallback, out var ignore1) && sopFallback != null)
            {
                if (ignore1) return;
                using var keyScope = context.ScopeKey(key, nestedStream);
                sopFallback.LoadInPlace(context, type, ref target, nestedStream);
            }
            else if (typeof(ISaveOverride).IsAssignableFrom(type))
            {
                using var keyScope = context.ScopeKey(key, nestedStream);
                (target as ISaveOverride).Load(context, nestedStream);
            }
            else if (!IsIgnored(type))
            {
                if (baseStream.IsSupportedType(type))
                {
                    baseStream.LoadInPlace(context, key, type, ref target);
                }
                else if (FinalFallback != null && FinalFallback.IsSupportedType(type) == SupportType.Supported)
                {
                    using var keyScope = context.ScopeKey(key, nestedStream);
                    FinalFallback.LoadInPlace(context, type, ref target, nestedStream);
                }
            }
        }
        public bool TryLoadInPlace<T>(StreamContext context, IInput baseStream, IInput nestedStream, object key, ref T target)
        {
            Type type = target?.GetType() ?? typeof(T);
            if (type.IsArray)
            {
                using var keyScope = context.ScopeKey(key, nestedStream, errorOnFail: false);
                if (keyScope.Success)
                {
                    object obj = target;
                    ArrayOverride.LoadInPlace(context, type, ref obj, nestedStream);
                    target = (T)obj;
                    return true;
                }
            }
            else if (TryGetSaveOverride(type, out var sop, out var ignore0) && sop != null)
            {
                if (ignore0) return false;
                using var keyScope = context.ScopeKey(key, nestedStream, errorOnFail: false);
                if (keyScope.Success)
                {
                    object obj = target;
                    sop.LoadInPlace(context, type, ref obj, nestedStream);
                    target = (T)obj;
                    return true;
                }
            }
            else if (TryGetSaveFallback(type, out var sopFallback, out var ignore1) && sopFallback != null)
            {
                if (ignore1) return false;
                using var keyScope = context.ScopeKey(key, nestedStream, errorOnFail: false);
                if (keyScope.Success)
                {
                    object obj = target;
                    sopFallback.LoadInPlace(context, type, ref obj, nestedStream);
                    target = (T)obj;
                    return true;
                }
            }
            else if (typeof(ISaveOverride).IsAssignableFrom(type))
            {
                using var keyScope = context.ScopeKey(key, nestedStream, errorOnFail: false);
                if (keyScope.Success)
                {
                    (target as ISaveOverride).Load(context, nestedStream);
                    return true;
                }
            }
            else if (!IsIgnored(type))
            {
                if (baseStream.IsSupportedType(type))
                {
                    return baseStream.TryLoadInPlace(context, key, ref target);
                }
                else if (FinalFallback != null && FinalFallback.IsSupportedType(type) == SupportType.Supported)
                {
                    using var keyScope = context.ScopeKey(key, nestedStream, errorOnFail: false);
                    if (keyScope.Success)
                    {
                        object obj = target;
                        FinalFallback.LoadInPlace(context, type, ref obj, nestedStream);
                        target = (T)obj;
                        return true;
                    }
                }
            }
            return false;
        }
        public bool TryLoadInPlace(StreamContext context, IInput baseStream, IInput nestedStream, object key, Type type, ref object target)
        {
            if (type.IsArray)
            {
                using var keyScope = context.ScopeKey(key, nestedStream, errorOnFail: false);
                if (keyScope.Success)
                {
                    ArrayOverride.LoadInPlace(context, type, ref target, nestedStream);
                    return true;
                }
            }
            else if (TryGetSaveOverride(type, out var sop, out var ignore0) && sop != null)
            {
                if (ignore0) return false;
                using var keyScope = context.ScopeKey(key, nestedStream, errorOnFail: false);
                if (keyScope.Success)
                {
                    sop.LoadInPlace(context, type, ref target, nestedStream);
                    return true;
                }
            }
            else if (TryGetSaveFallback(type, out var sopFallback, out var ignore1) && sopFallback != null)
            {
                if (ignore1) return false;
                using var keyScope = context.ScopeKey(key, nestedStream, errorOnFail: false);
                if (keyScope.Success)
                {
                    sopFallback.LoadInPlace(context, type, ref target, nestedStream);
                    return true;
                }
            }
            else if (typeof(ISaveOverride).IsAssignableFrom(type))
            {
                using var keyScope = context.ScopeKey(key, nestedStream, errorOnFail: false);
                if (keyScope.Success)
                {
                    (target as ISaveOverride).Load(context, nestedStream);
                    return true;
                }
            }
            else if (!IsIgnored(type))
            {
                if (baseStream.IsSupportedType(type))
                {
                    return baseStream.TryLoadInPlace(context, key, type, ref target);
                }
                else if (FinalFallback != null && FinalFallback.IsSupportedType(type) == SupportType.Supported)
                {
                    using var keyScope = context.ScopeKey(key, nestedStream, errorOnFail: false);
                    if (keyScope.Success)
                    {
                        FinalFallback.LoadInPlace(context, type, ref target, nestedStream);
                        return true;
                    }
                }
            }
            return false;
        }

    }
    public class RootStream : IStream
    {
        public TypeRegistry TypeRegistry;
        public GameObjectSO GameObjectSO;
        public NiBehaviourSO NiBehaviourSO;
        public IStream BaseStream;
        public RootStream(IStream baseStream)
        {
            BaseStream = baseStream;
        }
        public void Init()
        {
            var reflectionSO = new ReflectionSO(typeCondition: null, subTypeCondition: t => IsSupportedType(t));

            // Array
            var arraySO = new ArraySO(x => IsSupportedType(x));
            TypeRegistry.ArrayOverride = arraySO;

            // TODO add custom save for these types
            TypeRegistry.RegisterSaveOverride(typeof(Vector3), reflectionSO);
            TypeRegistry.RegisterSaveOverride(typeof(Quaternion), reflectionSO);

            // List
            var listSO = new ListSO(x => IsSupportedType(x));
            TypeRegistry.RegisterSaveFallback(typeof(IList), listSO);
            //NestedTypeRegistry.RegisterSaveFallback(typeof(IList), listSO);

            // HashSet
            TypeRegistry.RegisterSaveFallback(typeof(HashSet<>), new Generic1SO(typeof(HashSet<>), typeof(HashSetSO<>), x => IsSupportedType(x)));

            // Dictionary
            var dictionarySO = new DictionarySO(k => BaseStream.IsSupportedType(k), v => IsSupportedType(v));
            TypeRegistry.RegisterSaveFallback(typeof(IDictionary), dictionarySO);

            // GameObject
            GameObjectSO = new GameObjectSO(reflectionSO, x=> TypeRegistry.TypeHasSaveOverride(x));
            TypeRegistry.RegisterSaveOverride(typeof(GameObject), GameObjectSO);
            
            // NiBehaviour
            NiBehaviourSO = new NiBehaviourSO(reflectionSO);
            TypeRegistry.RegisterSaveFallback(typeof(NiBehaviour), NiBehaviourSO);
            
            // IUidObject
            var uidObjectSO = new UidObjectSO(NiBehaviourSO, reflectionSO);
            TypeRegistry.RegisterSaveFallback(typeof(IUidObject), uidObjectSO);

            TypeRegistry.RegisterSaveOverride(typeof(Rigidbody), new RigidbodySO(GameObjectSO));
            TypeRegistry.RegisterSaveOverride(typeof(Transform), new TransformSO(GameObjectSO));
            TypeRegistry.RegisterSaveOverride(typeof(CharacterController), new CharacterControllerSO(GameObjectSO));
            TypeRegistry.RegisterSaveOverride(typeof(NavMeshAgent), new NavMeshAgentSO(GameObjectSO));
            

            TypeRegistry.RegisterIgnored(typeof(Delegate));
            TypeRegistry.RegisterIgnored(typeof(UnityEvent));
            TypeRegistry.RegisterIgnored(typeof(UnityEvent<>));
            TypeRegistry.RegisterIgnored(typeof(UnityEvent<GameObject>));
            TypeRegistry.RegisterIgnored(typeof(UnityEvent<Vector3>));
            TypeRegistry.RegisterIgnored(typeof(UnityEvent<String>));

            // Fallback
            var finalFallback = new ReflectionSO(typeCondition: t =>
            {
                if (!ReflectionSO.IsSaveType(t))
                    return false;
                if (typeof(IList).IsAssignableFrom(t))
                    return false;
                if (typeof(IDictionary).IsAssignableFrom(t))
                    return false;
                if (t.IsArray)
                    return false;
                return true;

            }, subTypeCondition: t => IsSupportedType(t));
            TypeRegistry.FinalFallback = finalFallback;
            //NestedTypeRegistry.FinalFallback = finalFallback;


            //Output = NestedStream;
            //var reflectionSO = new ReflectionSO(typeCondition: null, subTypeCondition: t => NestedStream.IsSupportedType(t));// x => ReflectionSO.IsSaveType(x) || Stream.IsSupportedType(x));

            //GameObjectSO = new(comp => NestedStream.IsSupportedType(comp));

            //var dictionarySO = new DictionarySO(k => TypeRegistry.StreamPrimitive.IsSupportedType(k), v => NestedStream.IsSupportedType(v));

            //TypeRegistry.RegisterSaveOverride(typeof(Type), new TypeSO());
            //TypeRegistry.RegisterSaveFallback(typeof(IList), listSO);
            //TypeRegistry.RegisterSaveFallback(typeof(IDictionary), dictionarySO);
            //TypeRegistry.RegisterSaveOverride(typeof(GameObject), GameObjectSO);
            //TypeRegistry.RegisterSaveOverride(typeof(Transform), new TransformSO());
            //TypeRegistry.RegisterSaveOverride(typeof(Vector3), reflectionSO);
            //TypeRegistry.RegisterSaveOverride(typeof(Quaternion), reflectionSO);

            //// Todo: MonoBehaviour must support references to them
            //NiBehaviourSO = new NiBehaviourSO(reflectionSO);
            //TypeRegistry.RegisterSaveFallback(typeof(NiBehaviour), NiBehaviourSO);
            //TypeRegistry.RegisterSaveFallback(typeof(MonoBehaviour), reflectionSO);
            //TypeRegistry.RegisterIgnored(typeof(UnityEngine.Object));
            //TypeRegistry.RegisterIgnored(typeof(UnityEngine.EventSystems.UIBehaviour));
            //TypeRegistry.RegisterIgnored(typeof(Delegate));
            //TypeRegistry.RegisterIgnored(typeof(UnityEngine.Events.UnityEvent));
            //TypeRegistry.RegisterIgnored(typeof(UnityEngine.Events.UnityEvent<>));



        }
        public bool IsSupportedType(Type type)
        {
            if (TypeRegistry.IsSupportedType(type) != SupportType.Supported)
                return BaseStream.IsSupportedType(type);
            return true;
        }
        public bool ScopeBegin(StreamContext context, object key)
        {
            return BaseStream.ScopeBegin(context, key);
        }

        public void ScopeEnd(StreamContext context, object key)
        {
            BaseStream.ScopeEnd(context, key);
        }
    }
}