using System;
using System.Collections.Generic;
using UnityEngine;

namespace NiEngine.Actions
{

    [Serializable, ClassPickerName("Delayed Actions")]
    public class DelayedActions : Action, IUpdate, IUidObjectHost
    {
        [EditorField(showPrefixLabel: true, inline: true), NotSaved]
        public float Seconds;
        [Serializable]
        public struct InternalState
        {
            [Tooltip("Will be set to Seconds when the state begin")]
            public float CurrentlyRemaining;
            public bool Acted;
        }
        [SerializeField, EditorField(runtimeOnly: true)]
        public InternalState Internals;

        [EditorField(showPrefixLabel: false, inline: false, header: false), Save(saveInPlace: true)]
        public ActionSetNoHead Actions;

        public IEnumerable<IUidObject> Uids => Actions.Uids;

        bool IUpdate.Update(Owner owner, EventParameters parameters)
        {
#if UNITY_EDITOR
            Debug.Assert(!Internals.Acted);
#endif
            Internals.CurrentlyRemaining -= Time.deltaTime;

            if (Internals.CurrentlyRemaining <= 0)
            {
                Internals.Acted = true;
                Actions.Act(owner, parameters);
                return false;
            }
            return true;
        }
        public override void Act(Owner owner, EventParameters parameters)
        {
            Internals.Acted = false;
            Internals.CurrentlyRemaining = Seconds;
        }
    }


    /// <summary>
    /// Cancel when state end
    /// </summary>
    [Serializable, ClassPickerName("Delayed Actions")]
    public class StateDelayedActions : StateAction, IUpdate, IUidObjectHost
    {
        [EditorField(showPrefixLabel: true, inline:true), NotSaved]
        [Tooltip("Execute actions after this amount of seconds")]
        public float Seconds;

        [Serializable]
        public struct InternalState
        {
            [Tooltip("Will be set to Seconds when the state begin")]
            public float CurrentlyRemaining;
            public bool Canceled;
            public bool Acted;
        }

        [SerializeField, EditorField(runtimeOnly: true)]
        public InternalState Internals;

        [EditorField(showPrefixLabel: false, inline: false, header: false), Save(saveInPlace: true)]
        public StateActionSetNoHead Actions;
        public IEnumerable<IUidObject> Uids => Actions.Uids;
        bool IUpdate.Update(Owner owner, EventParameters parameters)
        {
#if UNITY_EDITOR
            Debug.Assert(!Internals.Acted);
#endif
            if (Internals.Canceled) return false;
            Internals.CurrentlyRemaining -= Time.deltaTime;

            if (Internals.CurrentlyRemaining <= 0)
            {
                Internals.Acted = true;
                Actions.OnBegin(owner, parameters);
                return false;
            }
            return true;
        }
        public override void OnBegin(Owner owner, EventParameters parameters)
        {
            Internals.Acted = false;
            Internals.Canceled = false;
            Internals.CurrentlyRemaining = Seconds;
        }
        public override void OnEnd(Owner owner, EventParameters parameters)
        {
            if (Internals.Acted)
            {
                Actions.OnEnd(owner, parameters);
            }
            Internals.Canceled = true;
        }
    }



    [Serializable, ClassPickerName("Delayed Actions (Rnd)")]
    public class RndDelayedActions : Action, IUpdate, IUidObjectHost
    {
        [EditorField(showPrefixLabel: true, inline: true), NotSaved]
        public float Min;
        [EditorField(showPrefixLabel: true, inline: true), NotSaved]
        public float Max;

        [Serializable]
        public struct InternalState
        {
            [Tooltip("Will be set to Seconds when the state begin")]
            public float CurrentlyRemaining;
            public bool Acted;
        }
        [SerializeField, EditorField(runtimeOnly: true)]
        public InternalState Internals;

        [EditorField(showPrefixLabel: false, inline: false, header: false), Save(saveInPlace: true)]
        public ActionSetNoHead Actions;
        public IEnumerable<IUidObject> Uids => Actions.Uids;

        bool IUpdate.Update(Owner owner, EventParameters parameters)
        {
#if UNITY_EDITOR
            Debug.Assert(!Internals.Acted);
#endif
            Internals.CurrentlyRemaining -= Time.deltaTime;

            if (Internals.CurrentlyRemaining <= 0)
            {
                Internals.Acted = true;
                Actions.Act(owner, parameters);
                return false;
            }
            return true;
        }
        public override void Act(Owner owner, EventParameters parameters)
        {
            Internals.Acted = false;
            Internals.CurrentlyRemaining = UnityEngine.Random.Range(Min, Max);
        }
    }


    /// <summary>
    /// Cancel when state end
    /// </summary>
    [Serializable, ClassPickerName("Delayed Actions (Rnd)")]
    public class RndStateDelayedActions : StateAction, IUpdate, IUidObjectHost
    {
        [EditorField(showPrefixLabel: true, inline: true), NotSaved]
        public float Min;
        [EditorField(showPrefixLabel: true, inline: true), NotSaved]
        public float Max;

        [Serializable]
        public struct InternalState
        {
            [Tooltip("Will be set to Seconds when the state begin")]
            public float CurrentlyRemaining;
            public bool Canceled;
            public bool Acted;
        }

        [SerializeField, EditorField(runtimeOnly:true)]
        InternalState Internals;

        [EditorField(showPrefixLabel: false, inline: false, header: false), Save(saveInPlace: true)]
        public StateActionSetNoHead Actions;
        public IEnumerable<IUidObject> Uids => Actions.Uids;
        bool IUpdate.Update(Owner owner, EventParameters parameters)
        {
#if UNITY_EDITOR
            Debug.Assert(!Internals.Acted);
#endif
            if (Internals.Canceled) return false;
            Internals.CurrentlyRemaining -= Time.deltaTime;

            if (Internals.CurrentlyRemaining <= 0)
            {
                Internals.Acted = true;
                Actions.OnBegin(owner, parameters);
                return false;
            }
            return true;
        }
        public override void OnBegin(Owner owner, EventParameters parameters)
        {
            Internals.Acted = false;
            Internals.Canceled = false;
            Internals.CurrentlyRemaining = UnityEngine.Random.Range(Min, Max);
        }
        public override void OnEnd(Owner owner, EventParameters parameters)
        {
            if (Internals.Acted)
            {
                Actions.OnEnd(owner, parameters);
            }
            Internals.Canceled = true;
        }
    }
}