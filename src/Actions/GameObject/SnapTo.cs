using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using UnityEngine.Events;
using NiEngine.Expressions.GameObjects;

namespace NiEngine.Actions
{
    [Serializable, ClassPickerName("GameObject./SnapTo")]
    public class SnapTo : StateAction
    {
        [Tooltip("Object to snap")]
        [SerializeReference, ObjectReferencePicker, EditorField(showPrefixLabel: true, inline: false), NotSaved]
        public IExpressionGameObject Object = new Self();
        
        [SerializeReference, ObjectReferencePicker, EditorField(showPrefixLabel: true, inline: false), NotSaved]
        public IExpressionGameObject ToObject;

        [Tooltip("If not null, the Object will be place so that 'ObjectPivot' is aligned with ToObject")]
        [SerializeReference, ObjectReferencePicker, EditorField(showPrefixLabel: true, inline: false), NotSaved]
        public IExpressionGameObject ObjectPivot;

        [NotSaved]
        public SnapMethod SnapMethod;
        [NotSaved]
        public Move.Method MoveMethod;
        [NotSaved]
        public UpdatePhase MovePhase;
        [NotSaved]
        public bool DebugLog;


        [Serializable]
        public struct InternalState
        {
            public GameObject Object;
            public Rigidbody Rigidbody;
            public NiTransform ObjectOffset;

            public GameObject ToObject;
            public Rigidbody ToRigidbody;

            // TODO save this thing
            public FixedJoint FixedJointComponent;
            public NiEngine.Components.MoveToBase MoveToComponent;

            public Transform PreviousParent;
        }

        [SerializeField, EditorField(runtimeOnly: true)]
        public InternalState Internals;





        void DoMoveImmediate(Owner owner, EventParameters parameters)
        {
            if (DebugLog)
                parameters.Log(owner, "SnapTo.Move.Immediate", $"Moving immediately object '{Internals.Object.GetNameOrNull()}' to '{Internals.ToObject.GetNameOrNull()}'");
            Internals.Rigidbody = Internals.Object.GetComponentInParent<Rigidbody>();
            if(Internals.ObjectOffset.HasPosition || Internals.ObjectOffset.HasRotation)
            {
                var result = NiTransform.AddTransform(NiTransform.RigidTransformOf(Internals.ToObject.transform), Internals.ObjectOffset);
                Move.Object(Internals.Object, Internals.Rigidbody, MoveMethod, result);
            }
            else
                Move.Object(Internals.Object, Internals.Rigidbody, MoveMethod, Internals.ToObject.transform);

        }
        void DoMove(Owner owner, EventParameters parameters)
        {
            switch (MovePhase)
            {
                case UpdatePhase.Immediate:
                    DoMoveImmediate(owner, parameters);
                    return;
                case UpdatePhase.Update:
                    if (DebugLog)
                        parameters.Log(owner, "SnapTo.Move.Update", $"Adding MoveToUpdate on '{Internals.Object.GetNameOrNull()}' to '{Internals.ToObject.GetNameOrNull()}' using MoveMethod '{MoveMethod}'");
                    Internals.MoveToComponent = Internals.Object.gameObject.AddComponent<NiEngine.Components.MoveToUpdate>();
                    break;
                case UpdatePhase.LateUpdate:
                    if (DebugLog)
                        parameters.Log(owner, "SnapTo.Move.LateUpdate", $"Adding MoveToLateUpdate on '{Internals.Object.GetNameOrNull()}' to '{Internals.ToObject.GetNameOrNull()}' using MoveMethod '{MoveMethod}'");
                    Internals.MoveToComponent = Internals.Object.gameObject.AddComponent<NiEngine.Components.MoveToLateUpdate>();
                    break;
                case UpdatePhase.FixedUpdate:
                    if (DebugLog)
                        parameters.Log(owner, "SnapTo.Move.FixedUpdate", $"Adding MoveToFixedUpdate on '{Internals.Object.GetNameOrNull()}' to '{Internals.ToObject.GetNameOrNull()}' using MoveMethod '{MoveMethod}'");
                    Internals.MoveToComponent = Internals.Object.gameObject.AddComponent<NiEngine.Components.MoveToFixedUpdate>();
                    break;
            }

            if (Internals.MoveToComponent != null)
            {
                Internals.MoveToComponent.Target = Internals.ToObject.transform;
                Internals.MoveToComponent.MoveMethod = MoveMethod;
                Internals.MoveToComponent.Offset = Internals.ObjectOffset;
            }
        }
        public override void OnBegin(Owner owner, EventParameters parameters)
        {
            var obj = Object.GetValue(owner, parameters);
            var toObject = ToObject.GetValue(owner, parameters);

            Internals.Object = obj;
            Internals.Rigidbody = null;
            Internals.ToObject = toObject;
            Internals.ToRigidbody = null;

            if (ObjectPivot != null && ObjectPivot.TryGetValue(owner, parameters, out var pivot) && pivot != null)
            {
                Internals.ObjectOffset = NiTransform.RigidDifference(pivot.transform, obj.transform);
            }
            else
            {
                Internals.ObjectOffset = default;
            }
            switch (SnapMethod)
            {
                case SnapMethod.Move:
                    DoMove(owner, parameters);
                    break;
                case SnapMethod.Parent:
                    if(MovePhase == UpdatePhase.Immediate)
                        DoMoveImmediate(owner, parameters);
                    if (DebugLog)
                        parameters.Log(owner, "SnapTo.Parent", $"Parenting object '{Internals.Object.GetNameOrNull()}' to '{Internals.ToObject.GetNameOrNull()}'");
                    Internals.PreviousParent = Internals.Object.transform.parent;
                    Internals.Object.transform.SetParent(Internals.ToObject.transform);
                    break;
                case SnapMethod.FixedJoint:
                    if (MovePhase == UpdatePhase.Immediate)
                        DoMoveImmediate(owner, parameters);
                    Internals.ToRigidbody = Internals.ToObject.GetComponentInParent<Rigidbody>();
                    if (DebugLog)
                        parameters.Log(owner, "SnapTo.FixedJoint", $"Adding FixedJoint on '{Internals.Object.GetNameOrNull()}' to '{Internals.ToObject.GetNameOrNull()}'");
                    if (Internals.ToRigidbody != null)
                    {
                        Internals.FixedJointComponent = Internals.Object.gameObject.AddComponent<FixedJoint>();
                        Internals.FixedJointComponent.connectedBody = Internals.ToRigidbody;
                    }
                    else
                    {
                        parameters.LogError(this, owner, "SnapTo.ToObject.Rigidbody.Null", $"Could not find a Rigidbody for object '{Internals.ToObject.GetNameOrNull()}'");
                    }
                    break;
            }
        }

        public override void OnEnd(Owner owner, EventParameters parameters)
        {
            if (Internals.FixedJointComponent != null)
            {
                Component.Destroy(Internals.FixedJointComponent);
                Internals.FixedJointComponent = null;
            }
            if (Internals.MoveToComponent != null)
            {
                Component.Destroy(Internals.MoveToComponent);
                Internals.MoveToComponent = null;
            }

            if (SnapMethod == SnapMethod.Parent)
            {
                if (DebugLog)
                    parameters.Log(owner, "SnapTo.Parent", $"Parenting object '{Internals.Object.GetNameOrNull()}' to '{Internals.PreviousParent.GetNameOrNull()}'");
                Internals.Object.transform.SetParent(Internals.PreviousParent);
            }
        }
        
    }
}