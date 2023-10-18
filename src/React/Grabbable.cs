using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace NiEngine
{
    public interface IGrabbableObserver : IUidObject
    {
        public void OnGrab(Grabbable grabbable);
        public void OnRelease(Grabbable grabbable);
    }
    /// <summary>
    /// Makes the owner Gameobject grabbable by a GrabberController
    /// </summary>
    public class Grabbable : NiBehaviour
    {
        public bool OnlyNonKinematic = true;
        [UnityEngine.Serialization.FormerlySerializedAs("NewConditions")]
        public ConditionSet Conditions;

        [UnityEngine.Serialization.FormerlySerializedAs("NewOnGrab")]
        public StateActionSet OnGrab;

        [UnityEngine.Serialization.FormerlySerializedAs("NewOnRelease")]
        public ActionSet OnRelease;

        public EventStateProcessor Processor;

        public List<IGrabbableObserver> Observers;
        
        public void AddObserver(IGrabbableObserver observer)
        {
            if (Observers == null)
                Observers = new();
            Observers.Add(observer);
        }
        public void RemoveObserver(IGrabbableObserver observer)
        {
            Observers?.Remove(observer);
        }


        void OnDisable()
        {
            ReleaseIfGrabbed();
        }
        public bool IsGrabbed => GrabbedBy != null;
        public GrabberController GrabbedBy { get; private set; }
        public Vector3 GrabbedPosition { get; private set; }
        public void ReleaseIfGrabbed()
        {
            GrabbedBy?.ReleaseGrabbed();
        }

        public bool CanGrab(GrabberController by, Vector3 position)
        {
            if (!enabled) return false;
            return Processor.Pass(new(this), Conditions, EventParameters.Trigger(gameObject, by.gameObject, position));
        }
        /// <summary>
        /// Call when a GrabberController grabs this grabbable
        /// </summary>
        /// <param name="by"></param>
        public void GrabBy(GrabberController by, Vector3 grabPosition)
        {
            GrabbedBy = by;
            GrabbedPosition = grabPosition;
            
            Processor.Begin(new(this), OnGrab, EventParameters.Trigger(gameObject, by.gameObject, grabPosition));

            // ToArray so observers can remove themselves while iterating the array instead of the list
            if (Observers != null)
            {
                foreach (var o in Observers.ToArray())
                {
                    o.OnGrab(this);
                }
            }
        }

        /// <summary>
        /// Call when a GrabberController release this grabbable
        /// </summary>
        /// <param name="by"></param>
        public void ReleaseBy(GrabberController by)
        {
            Processor.End(new(this), OnGrab, OnRelease, EventParameters.Trigger(gameObject, by.gameObject, GrabbedPosition));
            GrabbedBy = null;
            GrabbedPosition = Vector3.zero;

            // ToArray so observers can remove themselves while iterating the array instead of the list
            if (Observers != null)
            {
                foreach (var o in Observers.ToArray())
                {
                    o.OnRelease(this);
                }
            }
        }
        
    }
}