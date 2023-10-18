using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace NiEngine.Conditions
{


    [Serializable, ClassPickerName("Animator State")]
    public class AnimatorState : Condition
    {
        [NotSaved]
        public AnimatorStateReference Target;
        public override bool Pass(Owner owner, EventParameters parameters, bool logFalseResults = false)
        {
            if (Target.Animator.GetCurrentAnimatorStateInfo(0).fullPathHash == Target.StateHash)
            {
                return true;
            }
            if (logFalseResults)
                parameters.Log(owner, "AnimatorState.logFalseResults", $"Condition False: Animator on [{Target.Animator.gameObject.GetNameOrNull()}] is not on state '{Target.State}'");
            return false;
        }
    }
}