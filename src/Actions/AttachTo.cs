using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using UnityEngine.Events;
using NiEngine.Expressions.GameObjects;

namespace NiEngine.Actions
{

    [Serializable, ClassPickerName("Transform/Set Parent")]
    public class TransformSetParent : Action
    {

        [SerializeReference, ObjectReferencePicker, EditorField(showPrefixLabel: true, inline: true)]
        public IExpressionGameObjects Objects;

        [Tooltip("Can be null")]
        [SerializeReference, ObjectReferencePicker, EditorField(showPrefixLabel: true, inline: true)]
        public IExpressionGameObject To;

        public override void Act(Owner owner, EventParameters parameters)
        {
            var target = To?.GetValue(owner, parameters) ?? null;
            foreach (var obj in Objects.GetValues(owner, parameters))
            {
                if (target != null)
                {
                    obj.transform.parent = target.transform;
                }
                else
                {
                    obj.transform.parent = null;
                }
            }
        }
    }
}