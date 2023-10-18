using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using UnityEngine.Events;
using static NiEngine.EventParameters;
using static UnityEngine.UI.GridLayoutGroup;
using System.Security.Cryptography;

namespace NiEngine.Actions
{

    [Serializable, ClassPickerName("State/Reaction")]
    public class ReactionState : StateAction
    {
        [EditorField(showPrefixLabel: false, inline: true), NotSaved]
        public StateReactionReference Reaction;
        [NotSaved]
        public bool IgnoreMissingReaction = false;
        public override void OnBegin(Owner owner, EventParameters parameters)
        {
            Process(owner, parameters, Reaction.ReactOnBegin(owner, parameters), Reaction.OnBegin);

        }
        public override void OnEnd(Owner owner, EventParameters parameters)
        {
            Process(owner, parameters, Reaction.ReactOnEnd(owner, parameters), Reaction.OnEnd);
        }

        void Process(Owner owner, EventParameters parameters, int reactionCount, string reactionName)
        {

            if (!IgnoreMissingReaction && reactionCount == 0)
            {
                parameters.LogError(this, owner, "ReactionNotFound",
                    $"Reaction '{reactionName}' not found on '{string.Join(", ", Reaction.Target.GetValues(owner, parameters).Select(x => x.GetNameOrNull()))}'",
                    owner.GameObject);
            }
        }
    }
}