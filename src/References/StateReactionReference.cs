using NiEngine.Expressions.GameObjects;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NiEngine
{

    [System.Serializable]
    public struct StateReactionReference
    {
        [Tooltip("The target of the reaction.")]
        [SerializeReference, ObjectReferencePicker, EditorField(showPrefixLabel: false, inline: true)]
        public IExpressionGameObjects Target;
        
        [Tooltip("Name of the reaction to trigger on begin")]
        public string OnBegin;
        [Tooltip("Name of the reaction to trigger on end")]
        public string OnEnd;

        List<GameObject> CurrentGameObjects;
        public int ReactOnBegin(Owner owner, EventParameters parameters)
        {
            Debug.Assert(parameters.Self != null);
            int count = 0;
            CurrentGameObjects = Target.GetValues(owner, parameters).ToList();
            foreach (var obj in CurrentGameObjects)
                if (obj != null)
                {
                    parameters = parameters.WithSelf(obj);
                    parameters.AttachOrNewSource();
                    parameters.RecordEventSource?.BeginRecordReaction(owner, OnBegin, parameters);
                    int reactionCount = ReactionReference.SendReaction(OnBegin, parameters);
                    parameters.RecordEventSource?.EndRecordReaction();
                    count += reactionCount;
                }

            return count;
        }

        public int ReactOnEnd(Owner owner, EventParameters parameters)
        {
            Debug.Assert(parameters.Self != null);
            int count = 0;
            if (CurrentGameObjects != null)
            {
                foreach (var obj in CurrentGameObjects)
                    if (obj != null)
                    {
                        parameters = parameters.WithSelf(obj);
                        parameters.AttachOrNewSource();
                        parameters.RecordEventSource?.BeginRecordReaction(owner, OnEnd, parameters);
                        int reactionCount = ReactionReference.SendReaction(OnEnd, parameters);
                        parameters.RecordEventSource?.EndRecordReaction();
                        count += reactionCount;
                    }
            }
            return count;
        }
    }
}