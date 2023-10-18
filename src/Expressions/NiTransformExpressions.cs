using NiEngine.Expressions.GameObjects;
using NiEngine.Expressions.Variables;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NiEngine.Expressions.NiTransforms
{


    [Serializable, ClassPickerName("GameObject./NiTransform", inline: true, showPrefixLabel: false)]
    public class GameObjectNiTransform : ExpressionNiTransform
    {
        [SerializeReference, ObjectReferencePicker, EditorFieldThis]
        public IExpressionGameObject On = new Self();
        public bool SetPosition = true;
        public bool SetRotation = true;
        public bool SetScale = false;

        public override bool TryGetValue(Owner owner, EventParameters parameters, out NiTransform value)
        {
            if(On.TryGetValue(owner, parameters, out var go))
            {
                value = new NiTransform(go.transform, SetPosition, SetRotation, SetScale);
                return true;
            }
            value = default;
            return false;
        }
    }
}