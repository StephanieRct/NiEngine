using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace NiEngine
{
    [AddComponentMenu("Nie/Object/ReactOnFocus")]
    public class ReactOnFocus : NiBehaviour
    {
        [NotSaved]
        public bool DebugLog;
        [NotSaved]
        public bool ShowHand = true;
        //public bool ReactOnColliderObject = true;
        [NotSaved]
        public bool SendFocusToRigidBodyObject = true;

        [UnityEngine.Serialization.FormerlySerializedAs("NewConditions")]
        public ConditionSet Conditions;

        [UnityEngine.Serialization.FormerlySerializedAs("NewOnFocus")]
        public StateActionSet OnFocus;

        [UnityEngine.Serialization.FormerlySerializedAs("NewOnUnfocus")]
        public ActionSet OnUnfocus;
        
        public EventStateProcessor Processor;


        public bool CanReact(FocusController by, Vector3 position)
        {
            if (!enabled) return false;
            return Processor.Pass(new(this), Conditions, EventParameters.Trigger(gameObject, by.gameObject, position));
        }
        public void Focus(FocusController by, Vector3 position)
        {
            Processor.Begin(new(this), OnFocus, EventParameters.Trigger(gameObject, by.gameObject, position));
        }

        public void Unfocus(FocusController by, Vector3 position)
        {
            Processor.End(new(this), OnFocus, OnUnfocus, EventParameters.Trigger(gameObject, by.gameObject, position));
        }
    }
}