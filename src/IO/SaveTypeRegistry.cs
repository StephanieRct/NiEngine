//using NiEngine.IO.SaveOverrides;
//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Reflection;
//using System.Text;
//using UnityEngine;

//namespace NiEngine.IOold
//{

//    public class SaveTypeRegistry
//    {

//        public Dictionary<Type, ISaveOverrideProxy> Overrides = new();
//        public Dictionary<Type, ISaveOverrideProxy> Fallbacks = new();
//        /// <summary>
//        /// These Overrides are the first to be considered before ISaveClassOverrideProxy and Fallbacks
//        /// Types must match exactly
//        /// </summary>
//        public void RegisterSaveOverride(Type type, ISaveOverrideProxy saveOverride)
//        {
//            Overrides[type] = saveOverride;
//        }
//        public bool TryGetSaveOverride(Type type, out ISaveOverrideProxy saveClassOverride)
//            => Overrides.TryGetValue(type, out saveClassOverride);


//        /// <summary>
//        /// These Fallbacks SaveOverride are the last to be considered after Overrides and ISaveOverride.
//        /// The first type that can be assigned to will be used.
//        /// </summary>
//        public void RegisterSaveFallback(Type type, ISaveOverrideProxy saveOverride)
//        {
//            Fallbacks[type] = saveOverride;
//        }

//        public bool TryGetSaveFallback(Type type, out ISaveOverrideProxy saveClassOverride)
//        {
//            foreach (var (t, sop) in Fallbacks)
//            {
//                if (t.IsAssignableFrom(type))
//                {
//                    saveClassOverride = sop;
//                    return true;
//                }
//            }
//            saveClassOverride = default;
//            return false;
//        }

//        public SaveTypeRegistry()
//        {
//            //RegisterSaveOverride(typeof(GameObject), new GameObjectSO());
//            //RegisterSaveOverride(typeof(Transform), new TransformSO());
//            //RegisterSaveOverride(typeof(Vector3), new ReflectionSO());
//            //RegisterSaveOverride(typeof(Quaternion), new ReflectionSO());
//            //RegisterSaveFallback(typeof(MonoBehaviour), new ReflectionSO());
//            //RegisterSaveFallback(typeof(Delegate), new DelegateSO());

//            //RegisterSaveFallback(typeof(MulticastDelegate), new DelegateSO());
//            //DelegateSO
//        }
//    }
//}