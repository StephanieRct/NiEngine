using NiEngine.Expressions.GameObjects;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace NiEngine.Actions
{

    [Serializable, ClassPickerName("Transform/Interpolate Transform")]
    public class InterpolateTransform : StateAction, IUpdatePhase, IUidObjectHost
    {

        [SerializeReference, ObjectReferencePicker, EditorField(showPrefixLabel: true, inline: true)]
        public IExpressionGameObject Object;
        
        [Tooltip("if left null, will use initial object transform")]
        [SerializeReference, ObjectReferencePicker, EditorField(showPrefixLabel: true, inline: false)]
        public IExpressionGameObject From;

        [SerializeReference, ObjectReferencePicker, EditorField(showPrefixLabel: true, inline: false)]
        public IExpressionGameObject To;

        [NotSaved]
        public float Duration;
        [NotSaved]
        public AnimationCurve Curve;
        [NotSaved]
        public bool InterpolatePosition = true;
        [NotSaved]
        public bool InterpolateRotation = true;
        [NotSaved]
        public bool ReevaluateFrom = true;
        [NotSaved]
        public bool ReevaluateTo = true;
        [Tooltip("Will move to object directly at 'To' when the state ends")]
        public enum StateEnum
        {
            Continue,
            Stop,
            Stop_Complete,
            Stop_Reset,
        }
        //public UpdatePhase AdvanceTimePhases = UpdatePhase.Update;
        [NotSaved]
        public UpdatePhase UpdatePhase = UpdatePhase.Update;
        [NotSaved]
        public StateEnum StateAtEnd = StateEnum.Stop;
        

        [EditorField(showPrefixLabel: true, inline: false), Save(saveInPlace: true)]
        public StateActionSet ActionsAtEnd;

        public IEnumerable<IUidObject> Uids => ActionsAtEnd.Uids;
        [Serializable]
        public struct InternalState
        {
            public GameObject Object;
            public GameObject From;
            public GameObject To;


            public Vector3 FromPosition;
            public Quaternion FromRotation;

            public Vector3 ToPosition;
            public Quaternion ToRotation;

            public float Time;
            public float Value;
            public bool Active;
        }
        [SerializeField, EditorField(runtimeOnly: true)]
        public InternalState Internals;

        public override void OnBegin(Owner owner, EventParameters parameters)
        {
            Internals.Object = Object.GetValue(owner, parameters);
            Internals.From = From?.GetValue(owner, parameters) ?? null;
            if (Internals.From != null)
            {
                if (InterpolatePosition) Internals.FromPosition = Internals.From.transform.position;
                if (InterpolateRotation) Internals.FromRotation = Internals.From.transform.rotation;
            }
            else
            {
                if (InterpolatePosition) Internals.FromPosition = Internals.Object.transform.position;
                if (InterpolateRotation) Internals.FromRotation = Internals.Object.transform.rotation;
            }
            Internals.To = To.GetValue(owner, parameters);

            if (InterpolatePosition) Internals.ToPosition = Internals.To.transform.position;
            if (InterpolateRotation) Internals.ToRotation = Internals.To.transform.rotation;
            if (Internals.Object != null && Internals.To != null)
            {
                Internals.Time = 0;
                Internals.Active = true;
            }
        }
        public override void OnEnd(Owner owner, EventParameters parameters)
        {
            switch (StateAtEnd)
            {
                case StateEnum.Continue:
                    break;
                case StateEnum.Stop:
                    Internals.Active = false;
                    break;
                case StateEnum.Stop_Complete:
                    if (InterpolatePosition) Internals.Object.transform.position = Internals.To.transform.position;
                    if (InterpolateRotation) Internals.Object.transform.rotation = Internals.To.transform.rotation;
                    Internals.Active = false;
                    break;
                case StateEnum.Stop_Reset:
                    if (InterpolatePosition) Internals.Object.transform.position = Internals.FromPosition;
                    if (InterpolateRotation) Internals.Object.transform.rotation = Internals.FromRotation;
                    Internals.Active = false;
                    break;
            }
        }

        UpdatePhase IUpdatePhase.ActiveUpdatePhaseFlags => UpdatePhase;
        bool IFixedUpdate.FixedUpdate(Owner owner, EventParameters parameters) => Interpolate(owner, parameters, Time.fixedDeltaTime);
        bool IUpdate.Update(Owner owner, EventParameters parameters) => Interpolate(owner, parameters, Time.deltaTime);
        bool ILateUpdate.LateUpdate(Owner owner, EventParameters parameters) => Interpolate(owner, parameters, Time.deltaTime);
        bool Interpolate(Owner owner, EventParameters parameters, float dt)
        {
            if (!Internals.Active || Internals.Object == null || Internals.To == null)
                return false;

            Internals.Time += dt;
            if (Internals.Time > Duration)
            {
                if (InterpolatePosition) Internals.Object.transform.position = Internals.To.transform.position;
                if (InterpolateRotation) Internals.Object.transform.rotation = Internals.To.transform.rotation;

                ActionsAtEnd.OnBegin(owner, parameters);
                
                return false;
            }

            Internals.Value = Internals.Time / Duration;
            if (Curve != null && Curve.length > 0)
                Internals.Value = Curve.Evaluate(Internals.Value);
            var fromPosition = ReevaluateFrom ? Internals.From.transform.position : Internals.FromPosition;
            var fromRotation = ReevaluateFrom ? Internals.From.transform.rotation : Internals.FromRotation;
            var toPosition = ReevaluateTo ? Internals.To.transform.position : Internals.ToPosition;
            var toRotation = ReevaluateTo ? Internals.To.transform.rotation : Internals.ToRotation;
            if (InterpolatePosition) Internals.Object.transform.position = Vector3.Lerp(fromPosition, toPosition, Internals.Value);
            if (InterpolateRotation) Internals.Object.transform.rotation = Quaternion.Lerp(fromRotation, toRotation, Internals.Value);
            return true;
        }
    }
}