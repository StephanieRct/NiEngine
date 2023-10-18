using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace NiEngine
{
    [AddComponentMenu("Nie/Object/ReactOnTouch")]
    public class ReactOnTouch : NiBehaviour
    {

        public ConditionSet Conditions;

        public StateActionSet OnTouch;

        public ActionSet OnRelease;

        public EventStateProcessor Processor;

        public GameObject TargetObject => gameObject;// ThisObject != null ? TargetObject : gameObject;
        public bool CanTouch(TouchController by, Vector3 position)
        {
            if (!enabled) return false;
            return Processor.Pass(new(this), Conditions, EventParameters.Trigger(gameObject, by.gameObject, position));
        }

        public void Touch(TouchController by, Vector3 position)
        {
            Processor.Begin(new(this), OnTouch, EventParameters.Trigger(gameObject, by.gameObject, position));
        }

        public void Release(TouchController by, Vector3 position)
        {

            Processor.End(new(this), OnTouch, OnRelease, EventParameters.Trigger(gameObject, by.gameObject, position));
        }

    }
}