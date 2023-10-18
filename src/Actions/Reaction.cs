using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

namespace NiEngine.Actions
{

    [Serializable, ClassPickerName("Reaction")]
    public class Reaction : Action
    {
        [EditorField(showPrefixLabel:false, inline: true), NotSaved]
        public ReactionReference Reference = ReactionReference.Default;

        [NotSaved]
        public bool IgnoreMissingReaction = false;
        //public bool UseOverrides;
        [EditorField(unfold: true), NotSaved]
        public EventParameters.Overrides Overrides;
        public override void Act(Owner owner, EventParameters parameters)
        {
            int count = Reference.React(owner, parameters, Overrides);
            
            if (!IgnoreMissingReaction && count == 0 && !Reference.HasReaction(owner, parameters, Overrides, ReactionReference.k_MaxLoop))
            {
                parameters.LogError(this, owner, "ReactionNotFound",
                    $"Reaction '{Reference.ReactionName}' not found on '{string.Join(", ", Reference.Target.GetValues(owner, parameters).Select(x => x.GetNameOrNull()))}'",
                    owner.GameObject);
            }
        }


        //void IStateAction.OnBegin(Owner owner, EventParameters parameters) => Act(owner, parameters);
        //void IStateAction.OnEnd(Owner owner, EventParameters parameters) { }

        //public object Clone()
        //{
        //    return new Reaction
        //    {
        //        Reference = Reference,
        //        UseOverrides = UseOverrides,
        //        Overrides = Overrides,
        //    };
        //}
    }
}