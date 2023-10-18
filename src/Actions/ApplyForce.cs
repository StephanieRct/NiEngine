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

    [Serializable, ClassPickerName("Physics/ApplyForce")]
    public class ApplyForce : Action
    {
        [SerializeReference, ObjectReferencePicker, EditorField(showPrefixLabel: false, inline: true), NotSaved]
        public IExpressionGameObjects On;

        [EditorField(showPrefixLabel: true, inline: true), NotSaved]
        public float Force;

        [SerializeReference, ObjectReferencePicker, EditorField(showPrefixLabel: true, inline: false), NotSaved]
        public IExpressionDirection Direction;

        [SerializeReference, ObjectReferencePicker, NotSaved]
        public IExpressionVector3 At;


        [SerializeReference, ObjectReferencePicker, NotSaved]
        public IExpressionGameObjects Opposite;
        public override void Act(Owner owner, EventParameters parameters)
        {
            var dir = Direction.GetValue(owner, parameters);
            var at = At?.GetValue(owner, parameters) ?? Vector3.zero;

            foreach (var obj in On.GetValues(owner, parameters))
                if(obj.TryGetComponent<Rigidbody>(out var rb))
                {
                    if (At != null)
                        rb.AddForceAtPosition(dir * Force, at);
                    else
                        rb.AddForce(dir * Force);
                }

            if(Opposite != null)
                foreach (var obj in Opposite.GetValues(owner, parameters))
                    if (obj.TryGetComponent<Rigidbody>(out var rb))
                    {
                        if (At != null)
                            rb.AddForceAtPosition(dir * -Force, at);
                        else
                            rb.AddForce(dir * -Force);
                    }


        }
    }
}