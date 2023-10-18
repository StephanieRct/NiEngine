using NiEngine.Expressions.GameObjects;
using System;
using System.Linq;
using UnityEngine;

namespace NiEngine
{

    /// <summary>
    /// Trigger a reaction on a target
    /// </summary>
    [System.Serializable, NotSaved]
    public struct ReactionReference
    {
        [Tooltip("The target of the reaction.")]
        [SerializeReference, ObjectReferencePicker, EditorField(showPrefixLabel: false, inline: true)]
        public IExpressionGameObjects Target;

        [Tooltip("Name of the reaction to trigger")]
        [EditorField(showPrefixLabel:false, inline:true)]
        public string ReactionName;

        public static ReactionReference Default => new ReactionReference
        {
            Target = new Self(),
            ReactionName = default
        };

        public static readonly int k_MaxLoop = 1000;
        //public static bool HasActiveReaction(GameObject obj, string stateName, bool onlyEnabled, int maxLoop)
        //{
        //    ReactionActiveSum sum = default;
        //    foreach (var rr in obj.GetComponents<IReactionReceiver>().Where(x => x.ReactEnabled))
        //    {
        //        if(rr.HasActiveReaction(stateName, onlyEnabled, maxLoop))
        //            return true;
        //    }
        //    return false;
        //}

        //public static ReactionActiveSum IsActiveReactionState(GameObject obj, string stateName, bool onlyEnabled, int maxLoop)
        //{
        //    ReactionActiveSum sum = default;
        //    foreach (var rr in obj.GetComponents<IReactionReceiver>().Where(x => x.ReactEnabled))
        //    {
        //        sum.Add(rr.ActiveReactionSum(stateName, onlyEnabled, maxLoop));
        //    }
        //    return sum;
        //    //bool hasPotential = false;
        //    //foreach (var sm in obj.AllReactionStateMachine())
        //    //    foreach (var (group, state) in sm.AllStatesNamed(stateName))
        //    //    {
        //    //        hasPotential = true;
        //    //        if (state.IsActiveState)
        //    //            return true;
        //    //    }
        //    //return !hasPotential;
        //}

        /// <summary>
        /// Tells of an object has a specific reaction
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="reactionOrStateName"></param>
        /// <returns></returns>
        public static bool HasReaction(GameObject obj, string name, bool onlyEnabled, bool onlyActive, int maxLoop)
        {
            if (obj.GetComponents<IReactionReceiver>().Any(x => x.HasReaction(name, onlyEnabled, onlyActive, maxLoop)))
                return true;
            return false;
        }

        public bool HasReaction(Owner owner, EventParameters parameters, EventParameters.Overrides overrides, int maxLoop)
        {
            Debug.Assert(parameters.Self != null);
            //Debug.Assert(parameters.Current.From != null);
            foreach (var obj in Target.GetValues(owner, parameters))
                if (obj != null)
                    if (HasReaction(obj, ReactionName, onlyEnabled: true, onlyActive: false, maxLoop))
                        return true;
            return false;
        }



        /// <summary>
        /// Trigger this reaction reference
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public int React(Owner owner, EventParameters parameters, EventParameters.Overrides overrides)
        {
            Debug.Assert(parameters.Self != null);
            //Debug.Assert(parameters.Current.From != null);
            int count = 0;
            var parametersOverriden = parameters.WithOverride(owner, overrides);
            foreach (var obj in Target.GetValues(owner, parameters))
                if (obj != null)
                    count += React(owner, ReactionName, parametersOverriden.WithSelf(obj));
            return count;
        }
        public int React(Owner owner, EventParameters parameters, GameObject overrideFrom, GameObject overrideTrigger)
        {
            Debug.Assert(parameters.Self != null);
            //Debug.Assert(parameters.Current.From != null);
            int count = 0;
            foreach (var obj in Target.GetValues(owner, parameters))
                if (obj != null)
                    count += React(owner, ReactionName, parameters.WithSelf(obj).WithOverride(overrideFrom, overrideTrigger));
            return count;
        }
        ///// <summary>
        ///// Tells if this reaction reference can be triggered.
        ///// </summary>
        ///// <param name="parameters"></param>
        ///// <returns></returns>
        //public bool CanReact(Owner owner, EventParameters parameters)
        //{
        //    Debug.Assert(parameters.Self != null);
        //    Debug.Assert(parameters.Current.From != null);
        //    foreach (var obj in Target.GetValues(parameters))
        //        if (obj != null)
        //            if (CanReact(owner, ReactionName, parameters.WithSelf(obj)))
        //                return true;
        //    return true;
        //}
        //public bool TryReact(Owner owner, EventParameters parameters)
        //{
        //    Debug.Assert(parameters.Self != null);
        //    Debug.Assert(parameters.Current.From != null);
        //    if (CanReact(owner, parameters))
        //    {
        //        React(owner, parameters);
        //        return true;
        //    }
        //    return false;
        //}



        ///// <summary>
        ///// Tells if the conditions for a reaction are met.
        ///// </summary>
        ///// <param name="parameters"></param>
        ///// <param name="reactionOrStateName"></param>
        ///// <returns></returns>
        //public static bool CanReact(Owner owner, EventParameters parameters, string reactionOrStateName)
        //{
        //    Debug.Assert(parameters.Self != null);
        //    Debug.Assert(parameters.Current.From != null);

        //    parameters.AttachOrNewSource();
        //    parameters.RecordEventSource?.BeginRecordConditionSet(owner, parameters);

        //    int potentialReactCount = 0;

        //    foreach (var reaction in parameters.Self.GetComponents<Reactions>())
        //        if (reaction.enabled && (string.IsNullOrEmpty(reaction.ReactionName) || reaction.ReactionName == reactionOrStateName))
        //        {
        //            ++potentialReactCount;
        //            var can = reaction.CanReact(parameters.Current.TriggerObject, parameters.Current.TriggerPosition);
        //            if (can)
        //            {
        //                parameters.RecordEventSource?.EndRecordConditionSet(result: true);
        //                return true;
        //            }
        //        }

        //    foreach (var sm in parameters.Self.GetComponents<ReactionStateMachine>())
        //        if(sm.enabled)
        //        {
        //            var can = sm.CanReact(reactionOrStateName, parameters);
        //            if (can)
        //            {
        //                parameters.RecordEventSource?.EndRecordConditionSet(result: true);
        //                return true;
        //            }
        //        }
        //    // can react when no potential reaction were found.
        //    if (potentialReactCount == 0)
        //    {
        //        parameters.RecordEventSource?.EndRecordConditionSet(result: true);
        //        return true;
        //    }
        //    parameters.RecordEventSource?.EndRecordConditionSet(result: false);
        //    return false;
        //}

        /// <summary>
        /// Trigger a reaction
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="reactionOrStateName"></param>
        /// <returns></returns>
        public static int React(Owner owner, string name, EventParameters parameters)
        {
            Debug.Assert(parameters.Self != null);
            //Debug.Assert(parameters.Current.From != null);

            parameters.AttachOrNewSource();
            parameters.RecordEventSource?.BeginRecordReaction(owner, name, parameters);
            int reactionCount = SendReaction(name, parameters);
            parameters.RecordEventSource?.EndRecordReaction();

            return reactionCount;
        }

        public static int React(Owner owner, GameObject target, string name, EventParameters parameters)
        {
            Debug.Assert(parameters.Self != null);
            //Debug.Assert(parameters.Current.From != null);

            parameters.AttachOrNewSource();
            parameters.RecordEventSource?.BeginRecordReaction(owner, target, name, parameters);
            int reactionCount = SendReaction(target, name, parameters);
            parameters.RecordEventSource?.EndRecordReaction();

            return reactionCount;
        }

        public static int SendReaction(GameObject target, string name, EventParameters parameters)
        {
            Debug.Assert(parameters.Self != null);
            //Debug.Assert(parameters.Current.From != null);
            if (!target.activeInHierarchy) return 0;

            int reactionCount = 0;

            foreach (var reactions in target.GetComponents<IReactionReceiver>().Where(x => x.ReactEnabled))
                reactionCount += reactions.React(name, parameters);

            //// Execute the reaction on Reaction components
            //foreach (var reactions in target.GetComponents<Reactions>().Where(x => x.enabled))
            //    reactionCount += reactions.React(name, parameters);

            //// Execute the reaction on ReactionStateMachine components
            //foreach (var sm in target.GetComponents<ReactionStateMachine>().Where(x => x.enabled))
            //    reactionCount += sm.React(name, parameters);

            //// Execute the reaction on proxy components
            //foreach (var smProxy in target.GetComponents<ReactionProxy>().Where(x => x.enabled))
            //    reactionCount += smProxy.React(name, parameters);

            return reactionCount;
        }

        public static int SendReaction(string name, EventParameters parameters)
            => SendReaction(parameters.Self, name, parameters);
    }
}