using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using UnityEngine.Events;
using NiEngine.Expressions.GameObjects;

namespace NiEngine.Actions
{

    [Serializable, ClassPickerName("Physics/Rigidbody/SetKinematic")]
    public class SetKinematic : Action
    {
        [SerializeReference, ObjectReferencePicker, EditorField(showPrefixLabel: false, inline: true), NotSaved]
        public IExpressionGameObjects Target;

        [EditorField(showPrefixLabel: true, inline: true), NotSaved]
        public bool Enabled;
        
        public override void Act(Owner owner, EventParameters parameters)
        {
            foreach (var obj in Target.GetValues(owner, parameters))
                if(obj.TryGetComponent<Rigidbody>(out var rb))
                    rb.isKinematic = Enabled;
        }
    }
    
    [Serializable, ClassPickerName("Physics/Rigidbody/State/SetKinematic")]
    public class StateSetKinematic : StateActionComponentToggle<Rigidbody>
    {
        public override void SetValue(Rigidbody component, bool value)
        {
            component.isKinematic = value;
        }
    }
    
}