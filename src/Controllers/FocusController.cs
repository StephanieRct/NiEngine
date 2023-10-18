using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.HID;

namespace NiEngine
{
    [Serializable]
    public struct FocusTarget
    {
        public GameObject Body;
        public GameObject GameObject;

        public GameObject BodyOrGameObject => Body ?? GameObject;
        public FocusTarget(GameObject body, GameObject gameObject)
        {
            Body = body;
            GameObject = gameObject;
        }
        public FocusTarget(GameObject gameObject)
        {
            Body = gameObject;
            GameObject = gameObject;
        }
        public bool HasFocus => GameObject != null;
    }
    /// <summary>
    /// 
    /// </summary>
    [Save]
    public interface IFocusSolver : IUidObject
    {
        public struct QueryResult
        {
            public Vector3 Position;
            public float Distance;
        }

        bool Query(FocusController controller, Ray ray, float maxDistance, int layerMask, out QueryResult result);
        FocusTarget Focus(FocusController controller, out bool showHand);
        void Unfocus(FocusController controller);
    }

    /// <summary>
    /// 
    /// </summary>
    [Serializable, ClassPickerName("Rigidbody")]
    public class RigidbodyFocusSolver : IFocusSolver
    {
        public Uid Uid = Uid.NewUid();
        Uid IUidObject.Uid => Uid;
        [NotSaved]
        public bool OnlySaveable = false;

        [NotSaved]
        public bool IgnoreTriggers = true;
        enum FocusType
        {
            N_N,    // No Collider, no RigidBody
            A_N,    // With Collider, no RigidBody
            A_A,    // With Collider and RigidBody on same GameObject
            A_B,    // With Collider and RigidBody on different GameObject
            N_A,    // No collider, with RigidBody
        }

        [Serializable]
        struct InternalStates
        {
            public GameObject NewHitRigidbody;
            public GameObject NewHitCollider;
            public Vector3 NewHitPos;
            public GameObject FocusHitRigidbody;
            public GameObject FocusHitCollider;
            public Vector3 FocusHitPos;
            //public RaycastHit NewHit;
            //public RaycastHit FocusedHit;
            public FocusType NewFocusType;
            public FocusType CurrentFocusType;
        }

        [SerializeField, EditorField(runtimeOnly: true)]
        InternalStates Internals;

        GameObject CurrentCollider => Internals.FocusHitCollider;
        GameObject CurrentRigidBody => Internals.FocusHitRigidbody;

        public bool Query(FocusController controller, Ray ray, float maxDistance, int layerMask, out IFocusSolver.QueryResult result)
        {
            if (Physics.Raycast(ray, out var hit, maxDistance, layerMask, IgnoreTriggers ? QueryTriggerInteraction.Ignore : QueryTriggerInteraction.Collide))
            {
                GameObject nCollider = hit.collider.GetGameObjectOrNull();
                GameObject nRigidBody = hit.rigidbody.GetGameObjectOrNull();
                var target = nRigidBody ?? nCollider ?? null;
                if (!OnlySaveable || target.TryGetComponent<SaveId>(out _))
                {
                    Internals.NewFocusType = GetFocusType(controller, hit.point, ref nCollider, ref nRigidBody, out var sentToRB);
                    Internals.NewHitRigidbody = hit.rigidbody.GetGameObjectOrNull();
                    Internals.NewHitCollider = hit.collider.GetGameObjectOrNull();
                    Internals.NewHitPos = hit.point;

                    //result.GameObject = hit.
                    result.Distance = hit.distance;
                    result.Position = hit.point;
                    return true;
                }
            }
            result = default;
            return false;
        }

        public FocusTarget Focus(FocusController controller, out bool showHand)
        {
            
            showHand = false;
            if (Internals.NewFocusType == FocusType.N_N)
            {
                Unfocus(controller);
                return default;
            }
            GameObject nCollider = Internals.NewHitCollider;
            GameObject nRigidBody = Internals.NewHitRigidbody;
            // Transition notation: Current -> Next
            switch (Internals.CurrentFocusType)
            {
                case FocusType.N_N:
                    switch (Internals.NewFocusType)
                    {
                        case FocusType.A_N:
                            // N_N -> A_N
                            controller.DoFocusOn(nCollider, Internals.NewHitPos, ref showHand);
                            break;
                        case FocusType.A_A:
                            // N_N -> A_A
                            controller.DoFocusOn(nCollider, Internals.NewHitPos, ref showHand);
                            break;
                        case FocusType.A_B:
                            // N_N -> A_B
                            controller.DoFocusOn(nCollider, Internals.NewHitPos, ref showHand);
                            controller.DoFocusOn(nRigidBody, Internals.NewHitPos, ref showHand);
                            break;
                        case FocusType.N_A:
                            // N_N -> N_A
                            controller.DoFocusOn(nRigidBody, Internals.NewHitPos, ref showHand);
                            break;
                    }
                    break;
                case FocusType.A_N:
                    switch (Internals.NewFocusType)
                    {
                        case FocusType.A_N:
                            // A_N -> A_N
                            if (CurrentCollider != nCollider)
                            {
                                controller.DoUnfocusOn(CurrentCollider, Internals.FocusHitPos);
                                controller.DoFocusOn(nCollider, Internals.NewHitPos, ref showHand);
                            }
                            break;
                        case FocusType.A_A:
                            // A_N -> A_A
                            if (CurrentCollider != nCollider)
                            {
                                controller.DoUnfocusOn(CurrentCollider, Internals.FocusHitPos);
                                controller.DoFocusOn(nCollider, Internals.NewHitPos, ref showHand);
                            }
                            break;
                        case FocusType.A_B:
                            // A_N -> A_B
                            if (CurrentCollider == nCollider)
                            {
                                // Same A, new B
                                controller.DoFocusOn(nRigidBody, Internals.NewHitPos, ref showHand);

                            }
                            else
                            {
                                // new A, new B
                                controller.DoUnfocusOn(CurrentCollider, Internals.FocusHitPos);
                                controller.DoFocusOn(nCollider, Internals.NewHitPos, ref showHand);
                                controller.DoFocusOn(nRigidBody, Internals.NewHitPos, ref showHand);
                            }
                            break;
                        case FocusType.N_A:
                            // A_N -> N_A
                            if (CurrentCollider != nRigidBody)
                            {
                                // new A
                                controller.DoUnfocusOn(CurrentCollider, Internals.FocusHitPos);
                                controller.DoFocusOn(nRigidBody, Internals.NewHitPos, ref showHand);
                            }
                            break;
                    }
                    break;
                case FocusType.A_A:
                    switch (Internals.NewFocusType)
                    {
                        case FocusType.A_N:
                            // A_A -> A_N
                            if (CurrentCollider != nCollider)
                            {
                                // new A
                                controller.DoUnfocusOn(CurrentCollider, Internals.FocusHitPos);
                                controller.DoFocusOn(nCollider, Internals.NewHitPos, ref showHand);
                            }
                            break;
                        case FocusType.A_A:
                            // A_A -> A_A
                            if (CurrentCollider != nCollider)
                            {
                                // new A
                                controller.DoUnfocusOn(CurrentCollider, Internals.FocusHitPos);
                                controller.DoFocusOn(nCollider, Internals.NewHitPos, ref showHand);
                            }
                            break;
                        case FocusType.A_B:
                            // A_A -> A_B
                            if (CurrentCollider != nCollider)
                            {
                                controller.DoUnfocusOn(CurrentCollider, Internals.FocusHitPos);
                                controller.DoFocusOn(nCollider, Internals.NewHitPos, ref showHand);
                            }
                            controller.DoFocusOn(nRigidBody, Internals.NewHitPos, ref showHand);
                            break;
                        case FocusType.N_A:
                            // A_A -> N_A
                            if (CurrentRigidBody != nRigidBody)
                            {
                                // new A
                                controller.DoUnfocusOn(CurrentRigidBody.gameObject, Internals.FocusHitPos);
                                controller.DoFocusOn(nRigidBody, Internals.NewHitPos, ref showHand);
                            }
                            break;
                    }
                    break;
                case FocusType.A_B:
                    switch (Internals.NewFocusType)
                    {
                        case FocusType.A_N:
                            // A_B -> A_N
                            if (CurrentCollider != nCollider)
                            {
                                if (CurrentRigidBody != nCollider)
                                {
                                    // new A
                                    controller.DoUnfocusOn(CurrentCollider, Internals.FocusHitPos);
                                    controller.DoUnfocusOn(CurrentRigidBody.gameObject, Internals.FocusHitPos);
                                    controller.DoFocusOn(nCollider, Internals.NewHitPos, ref showHand);
                                }
                                else
                                {
                                    // cur B == next A
                                    controller.DoUnfocusOn(CurrentCollider, Internals.FocusHitPos);
                                }
                            }
                            break;
                        case FocusType.A_A:
                            // A_B -> A_A
                            if (CurrentCollider != nCollider)
                            {
                                if (CurrentRigidBody != nCollider)
                                {
                                    // new A
                                    controller.DoUnfocusOn(CurrentCollider, Internals.FocusHitPos);
                                    controller.DoUnfocusOn(CurrentRigidBody.gameObject, Internals.FocusHitPos);
                                    controller.DoFocusOn(nCollider, Internals.NewHitPos, ref showHand);
                                }
                                else
                                {
                                    // cur B == next A
                                    controller.DoUnfocusOn(CurrentCollider, Internals.FocusHitPos);
                                }
                            }
                            break;
                        case FocusType.A_B:
                            // A_B -> A_B
                            if (CurrentCollider != nCollider && CurrentRigidBody != nCollider)
                            {
                                // new A
                                controller.DoUnfocusOn(CurrentCollider, Internals.FocusHitPos);
                                controller.DoFocusOn(nCollider, Internals.NewHitPos, ref showHand);
                            }
                            if (CurrentCollider != nRigidBody && CurrentRigidBody != nRigidBody)
                            {
                                // new B
                                controller.DoUnfocusOn(CurrentRigidBody.gameObject, Internals.FocusHitPos);
                                controller.DoFocusOn(nRigidBody, Internals.NewHitPos, ref showHand);
                            }
                            break;
                        case FocusType.N_A:
                            // A_B -> N_A

                            if (CurrentCollider != nRigidBody)
                            {
                                controller.DoUnfocusOn(CurrentCollider, Internals.FocusHitPos);
                            }
                            if (CurrentRigidBody != nRigidBody)
                            {
                                controller.DoUnfocusOn(CurrentRigidBody.gameObject, Internals.FocusHitPos);
                                if (CurrentCollider != nRigidBody)
                                {
                                    // new A
                                    controller.DoFocusOn(nRigidBody, Internals.NewHitPos, ref showHand);
                                }
                            }
                            break;
                    }
                    break;
                case FocusType.N_A:
                    switch (Internals.NewFocusType)
                    {
                        case FocusType.A_N:
                            // N_A -> A_N
                            if (CurrentRigidBody != nCollider)
                            {
                                // new A
                                controller.DoUnfocusOn(CurrentRigidBody.gameObject, Internals.FocusHitPos);
                                controller.DoFocusOn(nCollider, Internals.NewHitPos, ref showHand);
                            }
                            break;
                        case FocusType.A_A:
                            // N_A -> A_A
                            if (CurrentRigidBody != nCollider)
                            {
                                // new A
                                controller.DoUnfocusOn(CurrentRigidBody.gameObject, Internals.FocusHitPos);
                                controller.DoFocusOn(nCollider, Internals.NewHitPos, ref showHand);
                            }
                            break;
                        case FocusType.A_B:
                            // N_A -> A_B

                            if (CurrentRigidBody != nCollider && CurrentRigidBody != nRigidBody)
                            {
                                // old A
                                controller.DoUnfocusOn(CurrentRigidBody.gameObject, Internals.FocusHitPos);
                            }
                            if (CurrentRigidBody != nCollider)
                            {
                                // new A
                                controller.DoFocusOn(nCollider, Internals.NewHitPos, ref showHand);

                            }
                            if (CurrentRigidBody != nRigidBody)
                            {
                                // new B
                                controller.DoFocusOn(nRigidBody, Internals.NewHitPos, ref showHand);
                            }
                            break;
                        case FocusType.N_A:
                            // N_A -> N_A
                            if (CurrentRigidBody != nRigidBody)
                            {
                                // new A
                                controller.DoUnfocusOn(CurrentRigidBody.gameObject, Internals.FocusHitPos);
                                controller.DoFocusOn(nRigidBody, Internals.NewHitPos, ref showHand);

                            }
                            break;
                    }
                    break;
            }
            Internals.CurrentFocusType = Internals.NewFocusType;
            Internals.FocusHitRigidbody = Internals.NewHitRigidbody;
            Internals.FocusHitCollider = Internals.NewHitCollider;
            Internals.FocusHitPos = Internals.NewHitPos;
            return new FocusTarget(nRigidBody, nCollider); //nCollider;//nRigidBody ?? nCollider;
        }

        public void Unfocus(FocusController controller)
        {
            if (CurrentCollider == CurrentRigidBody)
                controller.DoUnfocusOn(CurrentCollider, Internals.FocusHitPos);
            else
            {
                controller.DoUnfocusOn(CurrentCollider, Internals.FocusHitPos);
                if (CurrentRigidBody != null)
                    controller.DoUnfocusOn(CurrentRigidBody.gameObject, Internals.FocusHitPos);
            }

            Internals.FocusHitRigidbody = default;
            Internals.FocusHitCollider = default;
            Internals.FocusHitPos = default;
            Internals.CurrentFocusType = FocusType.N_N;
        }
        FocusType GetFocusType(FocusController controller, Vector3 position, ref GameObject collider, ref GameObject rigidBody, out bool sendToRigidBody)
        {
            sendToRigidBody = false;
            if (rigidBody == null)
            {
                // *_N
                if (collider == null || !controller.CanFocusOn(collider, position, out sendToRigidBody))
                {
                    collider = null;
                    rigidBody = null;
                    return FocusType.N_N;
                }
                else
                {
                    rigidBody = null;
                    return FocusType.A_N;
                }
            }
            else
            {
                // *_A, *_B
                if (collider == null)
                {
                    // N_A
                    if (controller.CanFocusOn(rigidBody, position, out var _))
                    {
                        collider = null;
                        return FocusType.N_A;
                    }
                    else
                    {
                        collider = null;
                        rigidBody = null;
                        return FocusType.N_N;
                    }
                }
                else if (collider == rigidBody)
                {
                    // A_A
                    if (controller.CanFocusOn(collider, position, out sendToRigidBody))
                        return FocusType.A_A;
                    else
                    {
                        collider = null;
                        rigidBody = null;
                        return FocusType.N_N;
                    }
                }
                else
                {
                    // A_B
                    if (controller.CanFocusOn(collider, position, out sendToRigidBody))
                    {
                        if (sendToRigidBody && controller.CanFocusOn(rigidBody, position, out var _))
                            return FocusType.A_B;
                        else
                        {
                            rigidBody = null;
                            return FocusType.A_N;
                        }
                    }
                    else if (controller.CanFocusOn(rigidBody, position, out var _))
                    {
                        collider = null;
                        return FocusType.N_A;
                    }
                    else
                    {
                        collider = null;
                        rigidBody = null;
                        return FocusType.N_N;
                    }
                }
            }
        }
    }





    /// <summary>
    /// 
    /// </summary>
    [AddComponentMenu("Nie/Player/FocusController")]
    public class FocusController : NiBehaviour, IUidObjectHost
    {
        [Tooltip("Where the ray cast will be originating from. If left null, will cast from this GameObject")]
        [NotSaved]
        public Transform RayCastOriginObject;

        [Tooltip("Where the ray cast will be directed toward. If left null, will ray cast forward (in the middle of the screen on a camera)")]
        [NotSaved]
        public Transform RayCastTargetObject;

        [Tooltip("Will only focus on object with a ReactOnFocus component")]
        [NotSaved]
        public bool FocusOnlyWithReactOnFocus = false;

        [Tooltip("Object to move to the currently focused 'ReactOnFocus' object.")]
        [NotSaved]
        public GameObject Hand;

        [Tooltip("Focus only on object of these layers")]
        [NotSaved]
        public LayerMask LayerMask = -1;

        [Tooltip("Focus only on object closer to this distance")]
        [NotSaved]
        public float MaxDistance = 10;
        public ConditionSet Conditions;
        public StateActionSet OnFocus;
        public ActionSet OnUnfocus;

        [Tooltip("Output debug log when objects are focused or unfocused")]
        [NotSaved]
        public bool DebugLog;


        [SerializeReference, ObjectReferencePicker(typeof(IFocusSolver)), EditorField(showPrefixLabel: false, inline: false), Save(saveInPlace:true)]
        //[Save(isSaveInPlace: true)]
        public List<IFocusSolver> Solvers = new(new IFocusSolver[]{new RigidbodyFocusSolver()});

        public IEnumerable<IUidObject> Uids => Solvers;
        [Serializable]
        struct InternalState
        {
            public FocusTarget FocusTarget;
            public Vector3 FocusPosition;
            //public int CurrentSolverIndexPlusOne;
            public IFocusSolver CurrentSolver;
            public EventStateProcessor Processor;
        }

        [SerializeField, EditorField(runtimeOnly: true)]
        InternalState Internals;

        public Vector3 RayCastOrigin => RayCastOriginObject != null ? RayCastOriginObject.position : transform.position;
        public Vector3 RayCastTarget => RayCastTargetObject != null ? RayCastTargetObject.position : transform.position + transform.forward;
        public Ray RayToCast => new Ray(RayCastOrigin, (RayCastTarget - RayCastOrigin).normalized);
        //public Rigidbody CurrentRigidBody => Internals.CurrentRigidBody;
        //public GameObject FocusTargetBody => Internals.FocusTarget;
        public FocusTarget FocusTarget => Internals.FocusTarget;
        public Vector3 FocusPosition => Internals.FocusPosition;

        public bool IsFocusing => Internals.FocusTarget.GameObject != null;


        void Start()
        {
        }

        void Update()
        {
            //if (Internals.CurrentSolverIndexPlusOne > 0 && !Internals.FocusTarget.HasFocus)
            //{
            //    Solvers[Internals.CurrentSolverIndexPlusOne - 1].Unfocus(this);
            //    Internals.CurrentSolverIndexPlusOne = 0;
            //    Internals.FocusPosition = Vector3.zero;
            //}
            if (Internals.CurrentSolver != null && !Internals.FocusTarget.HasFocus)
            {
                Internals.CurrentSolver.Unfocus(this);
                Internals.CurrentSolver = null;
                Internals.FocusPosition = Vector3.zero;
            }
            IFocusSolver bestSolver = null;
            IFocusSolver.QueryResult bestResult = default;
            bestResult.Distance = float.MaxValue;
            foreach (var solver in Solvers)
            {
                if (solver.Query(this, RayToCast, MaxDistance, LayerMask.value, out var result))
                {
                    if (result.Distance < bestResult.Distance)
                    {
                        bestSolver = solver;
                        bestResult = result;
                    }
                }
            }
            if (Internals.CurrentSolver != bestSolver && Internals.CurrentSolver != null)
                Internals.CurrentSolver.Unfocus(this);

            if (bestSolver != null)
            {
                Internals.FocusTarget = bestSolver.Focus(this, out var showHand);
                Internals.FocusPosition = bestResult.Position;
                if (showHand)
                    ShowHand(bestResult.Position);
                else
                    MoveHand(bestResult.Position);
            }
            else
            {
                Internals.FocusTarget = default;
                Internals.FocusPosition = Vector3.zero;
            }

            Internals.CurrentSolver = bestSolver;


        }

        void HideHand()
        {
            if (Hand != null && Hand.TryGetComponent<MeshRenderer>(out var rendererHand))
                rendererHand.enabled = false;
        }
        void ShowHand(Vector3 position)
        {
            if (Hand != null)
            {
                Hand.transform.position = position;
                if (Hand.TryGetComponent<MeshRenderer>(out var rendererHand))
                    rendererHand.enabled = true;
            }
        }
        void MoveHand(Vector3 position)
        {
            if (Hand != null)
            {
                Hand.transform.position = position;
            }
        }
        public void Unfocus()
        {
            Internals.CurrentSolver?.Unfocus(this);
            Internals.CurrentSolver = null;
            HideHand();
        }


        public bool CanFocusOn(GameObject gameObject, Vector3 position, out bool sendToRigidBody)
        {
            if (gameObject == null)
            {
                sendToRigidBody = false;
                return false;
            }
            bool canFocus = false;
            bool hasReactOnFocus = false;
            sendToRigidBody = false;
            foreach (var f in gameObject.GetComponents<ReactOnFocus>())
            {
                hasReactOnFocus = true;
                if (Internals.Processor.Pass(new(this), Conditions, EventParameters.Trigger(gameObject, gameObject, position))
                    && f.CanReact(this, position))
                {
                    canFocus = true;
                }
                if(f.SendFocusToRigidBodyObject)
                    sendToRigidBody = true;
            }

            
            return canFocus || !FocusOnlyWithReactOnFocus || hasReactOnFocus;
        }
        // return the GameObject to focus on
        public void DoFocusOn(GameObject gameObject, Vector3 position, ref bool showHand)
        {
            if (gameObject == null)
            {
                Debug.LogError($"[{Time.frameCount}] FocuserController '{name}' cannot focus on null gameObject", gameObject); 
                return;
            }

            if (DebugLog)
                Debug.Log($"Focus on {gameObject.GetNameOrNull()}");

            Internals.Processor.Begin(new(this), OnFocus, EventParameters.Trigger(gameObject, gameObject, position));

            foreach (var f in gameObject.GetComponents<ReactOnFocus>())
            {

                f.Focus(this, position);
                if (f.ShowHand)
                    showHand = true;
            }
        }

        public void DoUnfocusOn(GameObject gameObject, Vector3 position)
        {
            if (DebugLog)
                Debug.Log($"Unfocus on {gameObject.GetNameOrNull()}");

            Internals.Processor.End(new(this), OnFocus, OnUnfocus, EventParameters.Trigger(gameObject, gameObject, position));

            if (gameObject != null)
                foreach (var f in gameObject.GetComponents<ReactOnFocus>())
                    f.Unfocus(this, position);
        }
    }
}