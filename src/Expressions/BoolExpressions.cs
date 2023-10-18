using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NiEngine.Expressions.Bools
{
    [Serializable, ClassPickerName("Bool./Op", inline: true, showPrefixLabel: false)]
    public class GameObjectAxis : ExpressionBool
    {
        public enum OpEnum
        {
            All,
            Any,
            None
        }

        [EditorField(inline:true, showPrefixLabel:false)]
        public OpEnum Op;

        [SerializeReference, ObjectReferencePicker(typeof(IExpressionBool)), EditorField(showPrefixLabel:false, header: false)]
        public List<IExpressionBool> Values = new ();

        public override bool TryGetValue(Owner owner, EventParameters parameters, out bool value)
        {
            switch (Op)
            {
                case OpEnum.All:
                    foreach (var ve in Values)
                        if(!ve.GetValue(owner, parameters))
                        {
                            value = false;
                            return true;
                        }
                    value = true;
                    return true;
                case OpEnum.Any:
                    foreach (var ve in Values)
                        if (ve.GetValue(owner, parameters))
                        {
                            value = true;
                            return true;
                        }
                    value = false;
                    return true;
                case OpEnum.None:
                    foreach (var ve in Values)
                        if (ve.GetValue(owner, parameters))
                        {
                            value = false;
                            return true;
                        }
                    value = true;
                    return true;
                default:
                    value = default;
                    return false;
            }
        }
    }

}