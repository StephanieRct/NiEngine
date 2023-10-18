
//using System;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.Serialization;
//using UnityEngine.Events;
//using System.Linq;

//namespace NiEngine
//{

//    /// <summary>
//    /// 
//    /// </summary>
//    [AddComponentMenu("NiEngine/Object/NiRigidbody")]
//    [RequireComponent(typeof(Rigidbody))]
//    public class NiRigidbody : MonoBehaviour
//    {
//        public enum DiscoveryTypeEnum
//        {
//            None,
//            OnStart,
//            Always,
//        }
//        public DiscoveryTypeEnum DiscoveryType = DiscoveryTypeEnum.OnStart;

//        public Rigidbody Rigidbody;
//        public List<ReactOnCollisionPair> KnownReactOnCollisionPairs;
//        void Discover()
//        {
//            KnownReactOnCollisionPairs = Rigidbody.gameObject.GetComponentsInChildren<ReactOnCollisionPair>().Where(x=>x.ReactOnRigidbodyCollision).ToList();
//        }
//        private void Start()
//        {
//            Rigidbody = GetComponent<Rigidbody>();
//            if (DiscoveryType == DiscoveryTypeEnum.OnStart)
//                Discover();
//        }
//        public List<ReactOnCollisionPair> AllReactOnCollisionPair
//        {
//            get 
//            {
//                if (DiscoveryType == DiscoveryTypeEnum.Always)
//                    Discover();
//                return KnownReactOnCollisionPairs;
//            }
//        }
//        public void OnCollisionEnter(Collision collision)
//        {
//            if (!enabled) return;

//            foreach (var thisReact in AllReactOnCollisionPair)
//                thisReact.OnCollisionEnter(collision);

//            //var position = collision.GetContact(0).point;
//            //var otherNiRigidbody = collision.rigidbody?.GetComponent<NiRigidbody>();
//            //if (otherNiRigidbody != null)
//            //{
//            //    // match all Pairs N to N
//            //    foreach (var thisReact in AllReactOnCollisionPair)
//            //        foreach (var otherReact in otherNiRigidbody.AllReactOnCollisionPair)
//            //            thisReact.MatchPairEnter(otherReact, position);
//            //}
//            //else
//            //{
//            //    // match all owned ROCP to the ROCP of the target rigidbody or collider gameobject
//            //    foreach(var otherReact in collision.gameObject.GetComponents<ReactOnCollisionPair>())
//            //        foreach (var thisReact in AllReactOnCollisionPair)
//            //            thisReact.MatchPairEnter(otherReact, position);
//            //}
//        }
//        public void OnCollisionExit(Collision collision)
//        {
//            if (!enabled) return;

//            foreach (var thisReact in AllReactOnCollisionPair)
//                thisReact.OnCollisionExit(collision);

//            //var position = collision.GetContact(0).point;
//            //var otherNiRigidbody = collision.rigidbody?.GetComponent<NiRigidbody>();
//            //if (otherNiRigidbody != null)
//            //{
//            //    // match all Pairs N to N
//            //    foreach (var thisReact in AllReactOnCollisionPair)
//            //        foreach (var otherReact in otherNiRigidbody.AllReactOnCollisionPair)
//            //            thisReact.MatchPairExit(otherReact, position);
//            //}
//            //else
//            //{
//            //    // match all owned ROCP to the ROCP of the target rigidbody or collider gameobject
//            //    foreach (var otherReact in collision.gameObject.GetComponents<ReactOnCollisionPair>())
//            //        foreach (var thisReact in AllReactOnCollisionPair)
//            //            thisReact.MatchPairEnter(otherReact, position);
//            //}
//        }

//        public void OnTriggerEnter(Collider otherCollider)
//        {
//            // OnTriggerEnter is called event when gameobject is disabled.
//            if (!enabled) return;

//            foreach (var thisReact in AllReactOnCollisionPair)
//                thisReact.OnTriggerEnter(otherCollider);
//        }

//        public void OnTriggerExit(Collider otherCollider)
//        {
//            // OnTriggerExit is called event when gameobject is disabled.
//            if (!enabled) return;

//            foreach (var thisReact in AllReactOnCollisionPair)
//                thisReact.OnTriggerExit(otherCollider);
//        }

//    }
//}