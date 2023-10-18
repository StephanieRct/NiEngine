using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using UnityEngine.Events;
using NiEngine.Expressions.GameObjects;
using NiEngine.Expressions;

namespace NiEngine.Actions
{

    [Serializable, ClassPickerName("Event/Event(GameObject)")]
    public class EventGameObject : Action
    {
        [SerializeReference, ObjectReferencePicker, EditorField(showPrefixLabel: false, inline: true)]
        public IExpressionGameObject Object;

        [NotSaved]
        public bool IgnoreIfNull = false;
        [NotSaved]
        public UnityEvent<GameObject> UnityEvent;

        public override void Act(Owner owner, EventParameters parameters)
        {
            var obj = Object?.GetValue(owner, parameters);
            if(obj != null || !IgnoreIfNull)
                UnityEvent?.Invoke(obj);
            
        }
    }

    [Serializable, ClassPickerName("State/Event/Event(GameObject)")]
    public class EventGameObjectState : StateAction
    {
        [SerializeReference, ObjectReferencePicker, EditorField(showPrefixLabel: false, inline: true)]
        public IExpressionGameObject Object;
        [NotSaved]
        public bool IgnoreIfNull = false;
        [NotSaved]
        public bool ReevaluatedObjectOnEnd = false;
        [NotSaved]
        public UnityEvent<GameObject> UnityEventOnBegin;
        [NotSaved]
        public UnityEvent<GameObject> UnityEventOnEnd;

        [SerializeField] 
        GameObject CurrentGameObject;

        public override void OnBegin(Owner owner, EventParameters parameters)
        {
            var obj = Object?.GetValue(owner, parameters);
            CurrentGameObject = obj;
            if (obj != null || !IgnoreIfNull)
                UnityEventOnBegin?.Invoke(obj);
        }

        public override void OnEnd(Owner owner, EventParameters parameters)
        {
            var obj = CurrentGameObject;
            if (ReevaluatedObjectOnEnd)
                obj = Object?.GetValue(owner, parameters);
            if (obj != null || !IgnoreIfNull)
                UnityEventOnEnd?.Invoke(obj);
        }
    }


    [Serializable, ClassPickerName("Event/Event(Vector3)")]
    public class EventVector3 : Action
    {
        [SerializeReference, ObjectReferencePicker, EditorField(showPrefixLabel: false, inline: true)]
        public IExpressionVector3 Vector;
        [NotSaved]
        public UnityEvent<Vector3> UnityEvent;

        public override void Act(Owner owner, EventParameters parameters)
        {
            var pos = Vector.GetValue(owner, parameters);
            UnityEvent?.Invoke(pos);
        }
    }

    [Serializable, ClassPickerName("State/Event/Event(Vector3)")]
    public class EventVector3State : StateAction
    {
        [SerializeReference, ObjectReferencePicker, EditorField(showPrefixLabel: false, inline: true)]
        public IExpressionVector3 Vector;
        [NotSaved]
        public bool ReevaluatedValueOnEnd = false;
        [NotSaved]
        public UnityEvent<Vector3> UnityEventOnBegin;
        [NotSaved]
        public UnityEvent<Vector3> UnityEventOnEnd;

        [SerializeField]
        Vector3 CurrentVector3;

        public override void OnBegin(Owner owner, EventParameters parameters)
        {
            if (Vector == null) return;
            var value = Vector.GetValue(owner, parameters);
            CurrentVector3 = value;
            UnityEventOnBegin?.Invoke(value);
        }

        public override void OnEnd(Owner owner, EventParameters parameters)
        {
            if (Vector == null) return;
            var value = CurrentVector3;
            if (ReevaluatedValueOnEnd)
                value = Vector.GetValue(owner, parameters);
            UnityEventOnEnd?.Invoke(value);
        }
    }


    [Serializable, ClassPickerName("Event/Event(string)")]
    public class EventString : Action
    {
        [NotSaved]
        public string String;
        [NotSaved]
        public UnityEvent<string> UnityEvent;

        public override void Act(Owner owner, EventParameters parameters)
        {
            UnityEvent?.Invoke(String);
        }
    }

    [Serializable, ClassPickerName("State/Event/Event(string)")]
    public class EventStringState : StateAction
    {
        [NotSaved]
        public string StringBegin;
        [NotSaved]
        public UnityEvent<string> UnityEventOnBegin;

        [NotSaved]
        public string StringEnd;
        [NotSaved]
        public UnityEvent<string> UnityEventOnEnd;


        public override void OnBegin(Owner owner, EventParameters parameters)
        {
            UnityEventOnBegin?.Invoke(StringBegin);
        }

        public override void OnEnd(Owner owner, EventParameters parameters)
        {
            UnityEventOnEnd?.Invoke(StringEnd);
        }
    }
}
