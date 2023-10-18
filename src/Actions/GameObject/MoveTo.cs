using System;
using NiEngine.Expressions;
using NiEngine.Expressions.GameObjects;
using UnityEngine;

namespace NiEngine.Actions
{
    [Serializable, ClassPickerName("GameObject./MoveTo")]
    public class MoveTo : Action
    {
        [Tooltip("Object to move")]
        [SerializeReference, ObjectReferencePicker, EditorField(showPrefixLabel: true, inline: false), NotSaved]
        public IExpressionGameObject Object = new Self();
        
        [SerializeReference, ObjectReferencePicker, EditorField(showPrefixLabel: true, inline: false), NotSaved]
        public IExpressionNiTransform To;

        [NotSaved]
        public Move.Method MoveMethod;

        [NotSaved]
        public bool ResetVelocity = false;
        public override void Act(Owner owner, EventParameters parameters)
        {
            var obj = Object.GetValue(owner, parameters);
            var to = To.GetValue(owner, parameters);
            Move.Object(obj, MoveMethod, to);
            if(ResetVelocity && obj.TryGetComponent<Rigidbody>(out var rb))
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }

    }
}