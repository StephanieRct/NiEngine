using System;
using UnityEngine;

namespace NiEngine.Actions
{
    [Serializable, ClassPickerName("Log")]
    public class Log : Action
    {
        [EditorField(showPrefixLabel: false, inline: true), NotSaved]
        public string Message;

        public override void Act(Owner owner, EventParameters parameters)
        {
            parameters.Log(owner, "Action.Log", Message);
        }
    }

    [Serializable, ClassPickerName("Breakpoint")]
    public class Breakpoint : Action
    {
        public override void Act(Owner owner, EventParameters parameters)
        {
            Debug.Log($"Breakpoint at: {owner}");
        }
    }
}