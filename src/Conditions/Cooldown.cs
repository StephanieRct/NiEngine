using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using UnityEngine.Events;
using static UnityEngine.GraphicsBuffer;

namespace NiEngine.Conditions
{

    [Serializable, ClassPickerName("Cooldown")]
    public class ConditionCooldown : Condition, IStateObserver, IUpdate, IInitialize
    {
        [Tooltip("In Seconds")]
        [EditorField(showPrefixLabel: true, inline: true), NotSaved]
        public float TimeInSeconds;
        
        float TimeLeft;
        public override bool Pass(Owner owner, EventParameters parameters, bool logFalseResults = false)
        {
            if (TimeLeft <= 0)
                return true;

            if (logFalseResults)
                parameters.Log(owner, "ConditionCooldown.logFalseResults", $"Condition False: Still on cooldown. TimeLeft: {TimeLeft}");
            return false;
        }
        bool IUpdate.Update(Owner owner, EventParameters parameters)
        {
            TimeLeft -= Time.deltaTime;
            return false;
        }
        void IInitialize.Initialize(Owner owner)
        {
            TimeLeft = TimeInSeconds;
        }
        void IStateObserver.OnStateBegin(Owner owner, EventParameters parameters)
        {

        }
        void IStateObserver.OnStateEnd(Owner owner, EventParameters parameters)
        {
            TimeLeft = TimeInSeconds;
        }
    }
}