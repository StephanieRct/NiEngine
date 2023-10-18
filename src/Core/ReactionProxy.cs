using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NiEngine.Expressions.GameObjects;
using UnityEngine;

namespace NiEngine
{
    [NotSaved]
    public class ReactionProxy : NiBehaviour, IReactionReceiver
    {
        bool IReactionReceiver.ReactEnabled => enabled;
        [SerializeReference, ObjectReferencePicker, EditorField(showPrefixLabel: true, prefixAligned: true, flexGrow: 1.0f, inline: false)]
        public IExpressionGameObjects Target = new NiEngine.Expressions.GameObjects.ConstExpressionGameObject();

        [EditorField(unfold: true)]
        public EventParameters.Overrides Overrides = new EventParameters.Overrides
        {
            From = new From(),
            Trigger = null,
        };
        


        public int React(string name, EventParameters parameters)
        {
            var owner = new Owner(this);
            if(Target == null)
            {
                parameters.LogError(null, this, $"ReactionProxy.Target.Null");
                return 0;
            }
            foreach (var target in Target.GetValues(owner, parameters))
            {
                if (target == gameObject) return 0;

                var param2 = parameters.WithOverride(owner, Overrides);
                param2.Self = target;
                return ReactionReference.React(owner, target, name, param2);
            }
            return 0;
        }

        public bool HasReaction(string reactionName, bool onlyEnabled, bool onlyActive, int maxLoop)
        {
            var owner = new Owner(this);

            if (Target?.IsConst ?? false)
                foreach (var t in Target.GetValues(owner, EventParameters.WithoutTrigger(gameObject, gameObject)).Where(x => x != gameObject))
                    if (ReactionReference.HasReaction(t, reactionName, onlyEnabled, onlyActive, maxLoop - 1))
                        return true;
            return false;
        }
    }
}