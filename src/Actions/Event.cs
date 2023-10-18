using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace NiEngine.Actions
{

    [Serializable, ClassPickerName("Event/Event()")]
    public class Event : Action
    {
        [NotSaved]
        public UnityEvent UnityEvent;
        
        public override void Act(Owner owner, EventParameters parameters)
        {
            UnityEvent?.Invoke();
        }
    }

    [Serializable, ClassPickerName("State/Event/Event()")]
    public class EventState : StateAction
    {
        [NotSaved]
        public UnityEvent UnityEventBegin;
        [NotSaved]
        public UnityEvent UnityEventEnd;

        public override void OnBegin(Owner owner, EventParameters parameters)
        {
            UnityEventBegin?.Invoke();
        }

        public override void OnEnd(Owner owner, EventParameters parameters)
        {
            UnityEventEnd?.Invoke();
        }
    }
}