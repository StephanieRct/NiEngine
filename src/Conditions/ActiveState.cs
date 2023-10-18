using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using UnityEngine.Events;
using NiEngine.Expressions.GameObjects;

namespace NiEngine.Conditions
{


    [Serializable, ClassPickerName("State")]
    public class ConditionActiveState : Condition
    {
        
        [SerializeReference, ObjectReferencePicker, EditorField(showPrefixLabel: false, inline: true)]
        public IExpressionGameObjects Target;

        public enum OperationEnum
        {
            All,
            Any,
            None,
        }
        [EditorField(inline: true), NotSaved]
        public OperationEnum Operation;

        [Tooltip("List of state name separated by commas. prefix a state name with ! to inverse the condition for that state only")]
        [EditorField(inline: true), NotSaved]
        public string State;

        public override bool Pass(Owner owner, EventParameters parameters, bool logFalseResults=false)
        {
            foreach(var obj in Target.GetValues(owner, parameters))
            {
                bool not = false;
                char[] delimiterChars = { ' ', ',', '\t' };
                string[] stateCommands = State.Split(delimiterChars, System.StringSplitOptions.RemoveEmptyEntries);
                foreach(var sc in stateCommands)
                {
                    string state;
                    if (sc.StartsWith("!"))
                    {
                        state = sc.Substring(1).Trim();
                        not = true;
                    }
                    else
                        state = sc;
                    var result = ReactionReference.HasReaction(obj, sc, onlyEnabled:true, onlyActive:true, ReactionReference.k_MaxLoop);
                    if (not) result = !result;
                    switch (Operation)
                    {
                        case OperationEnum.All:
                            if (!result)
                            {
                                if(logFalseResults)
                                    parameters.Log(owner, "ConditionActiveState.logFalseResults", $"Condition False: state '{sc}' on [{obj.GetNameOrNull()}] is false and operation is 'All'");
                                return false;
                            }

                            break;
                        case OperationEnum.Any:
                            if (result)
                                return true;
                            break;
                        case OperationEnum.None:
                            if (result)
                            {
                                if (logFalseResults)
                                    parameters.Log(owner, "ConditionActiveState.logFalseResults", $"Condition False: state '{sc}' on [{obj.GetNameOrNull()}] is true and operation is 'None'");
                                return false;
                            }

                            break;
                    }

                }
                return true;
            }
            return false;
        }
    }
}