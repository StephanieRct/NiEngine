using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace NiEngine
{
    /// <summary>
    /// 
    /// </summary>
    [Save]
    public interface IGrabberSolver : IUidObject
    {
        bool CanGrab(GrabberController by, GameObject obj, Vector3 position);
        void Grab(GrabberController by, GameObject obj, Vector3 grabPosition);
        void Release(GrabberController by);
        void Pull(GrabberController by, Vector3 localGrabbedPosition, Vector3 to);
        void Throw(GrabberController by, Vector3 localGrabbedPosition, Vector3 direction);
        //void RotateHV(GrabberController by, quaternion rotation);
    }

    /// <summary>
    /// 
    /// </summary>
    [Serializable, ClassPickerName("Rigidbody")]
    public class RigidbodyGrabberSolver : IGrabberSolver
    {

        public Uid Uid = Uid.NewUid();
        Uid IUidObject.Uid => Uid;


        [NotSaved, Tooltip("Force applied to the grabbed object")]
        public float HoldForce = 35000;
        [NotSaved, Tooltip("Physics velocity drag to apply on the held object")]
        public float HoldDrag = 30;
        [NotSaved, Tooltip("Physics angular velocity drag to apply on the held object")]
        public float HoldAngularDrag = 45;

        [NotSaved]
        public float ThrowForce = 200;
        [NotSaved]
        public float ThrowMinVelocity = 150;
        [NotSaved]
        public float ThrowMaxVelocity = 500;
        [Serializable]
        struct InternalStates
        {
            public Rigidbody Rigidbody;
            // Velocity drag previously when not being grabbed
            public float OldDrag;

            // Angular velocity drag when not being grabbed
            public float OldAngularDrag;
        }

        [SerializeField, EditorField(runtimeOnly: true)]
        InternalStates Internals;

        public bool CanGrab(GrabberController by, GameObject obj, Vector3 position)
            => obj.TryGetComponent<Rigidbody>(out var rb);// && !rb.isKinematic;
        
        public void Grab(GrabberController by, GameObject obj, Vector3 grabPosition)
        {
            Internals.Rigidbody = obj.GetComponent<Rigidbody>();
            if (HoldDrag >= 0)
            {
                Internals.OldDrag = Internals.Rigidbody.drag;
                Internals.Rigidbody.drag = HoldDrag;
            }
            if (HoldAngularDrag >= 0)
            {
                Internals.OldAngularDrag = Internals.Rigidbody.angularDrag;
                Internals.Rigidbody.angularDrag = HoldAngularDrag;
            }

            by.DoGrab(this, obj.GetComponent<Grabbable>(), grabPosition);
        }
        
        public void Release(GrabberController by)
        {
            if (HoldDrag >= 0)
                Internals.Rigidbody.drag = Internals.OldDrag;
            if (HoldAngularDrag >= 0)
                Internals.Rigidbody.angularDrag = Internals.OldAngularDrag;
            
            Internals.Rigidbody = null;
            by.DoReleaseGrabbed(this);
        }

        public void Pull(GrabberController by, Vector3 localGrabbedPosition, Vector3 to)
        {
            var grabPoint = Internals.Rigidbody.transform.TransformPoint(localGrabbedPosition);
            var diff = to - grabPoint;
            Internals.Rigidbody.AddForceAtPosition(diff * HoldForce * Time.deltaTime * Internals.Rigidbody.mass, grabPoint);
        }

        public void Throw(GrabberController by, Vector3 localGrabbedPosition, Vector3 direction)
        {

            if (HoldDrag >= 0)
                Internals.Rigidbody.drag = Internals.OldDrag;
            if (HoldAngularDrag >= 0)
                Internals.Rigidbody.angularDrag = Internals.OldAngularDrag;
            var rigidbodyToThrow = Internals.Rigidbody;
            Internals.Rigidbody = null;
            by.DoReleaseGrabbed(this);
            var finalVelocity = Math.Clamp(ThrowForce / rigidbodyToThrow.mass, ThrowMinVelocity, ThrowMaxVelocity); 
            //var grabPoint = rigidbodyToThrow.transform.TransformPoint(localGrabbedPosition);
            //rigidbodyToThrow.AddForceAtPosition(direction * ThrowVelocity * rigidbodyToThrow.mass, grabPoint);
            rigidbodyToThrow.AddForce(direction * finalVelocity * rigidbodyToThrow.mass);
        }

        //public void RotateHV(GrabberController by, quaternion rotation)
        //{
        //    Internals.Rigidbody.ad
        //}

    }

/// <summary>
    /// Makes the owner gameobject able to grab and release Grabbable Gameobjects
    /// </summary>
    [AddComponentMenu("Nie/Player/GrabberController")]
    public class GrabberController : NiBehaviour, IUidObjectHost
    {
        [NotSaved, Tooltip("Only grab objects that are currently focused by this FocusController")]
        public FocusController FocusController;
        
        [NotSaved, Tooltip("Where the grabbed object will be move toward.")]
        public Transform GrabPosition;
        
        public ConditionSet Conditions;
        [NotSaved, Tooltip("Grab only objects of these layers")]
        public LayerMask LayerMask = -1;

        public StateActionSet OnGrab;

        [NotSaved, Tooltip("If true, set the grabbed object and all its children to a different layer")]
        public bool ChangedGrabbedObjectLayer = false;
        [NotSaved, Tooltip("The layer to set on the grabbed object if ChangedGrabbedObjectLayer is checked")]
        public GameObjectLayer GrabbedObjectLayer;

        public ActionSet OnRelease;


        [SerializeReference, ObjectReferencePicker(typeof(IGrabberSolver)), EditorField(showPrefixLabel: false, inline: false), Save(saveInPlace:true)]
        public List<IGrabberSolver> Solvers = new(new IGrabberSolver[] { new RigidbodyGrabberSolver() });

        public IEnumerable<IUidObject> Uids => Solvers;

        [Serializable]
        public struct InternalState
        {
            [Tooltip("The currently grabbed object")]
            public Grabbable GrabbedGrabbable;

            //[Tooltip("The parent of the grabbed object when grabbed. If parent changes, will release the grabbed object")]
            //public Transform GrabbedParent;
            
            public IGrabberSolver CurrentSolver;

            public int PreviousGrabbedObjectLayer;

            // Relative position where the currently grabbed grabbable was grabbed
            public Vector3 LocalGrabbedPosition;

            public EventStateProcessor Processor;
        }
        [SerializeField, EditorField(runtimeOnly: true)]
        public InternalState Internals;
        
        public bool IsGrabbing => Internals.GrabbedGrabbable != null;

        public Grabbable GrabbedGrabbable => Internals.GrabbedGrabbable;
        
        public bool CanGrab(GameObject obj, Vector3 position, out Grabbable grabbable)
        {
            if (obj != null
                && obj.TryGetComponent<Grabbable>(out grabbable)
                && obj.TryGetComponent<Rigidbody>(out var rb) 
                && (!grabbable.OnlyNonKinematic || !rb.isKinematic)
                )
            {
                return Internals.Processor.Pass(new (this), Conditions, EventParameters.Trigger(gameObject, gameObject, position)) && grabbable.CanGrab(this, position);
            }

            grabbable = default;
            return false;
        }

        public bool IsShowingHand { get; private set; }


        void Start()
        {
            FocusController ??= GetComponent<FocusController>();
        }
        void Update()
        {
            if (IsGrabbing)
            {
                if (!Internals.GrabbedGrabbable.CanGrab(this, GrabPosition.position))
                    //|| Internals.GrabbedGrabbable.transform.parent != Internals.GrabbedParent)
                    ReleaseGrabbed();
                else
                {
                    Internals.CurrentSolver.Pull(this, Internals.LocalGrabbedPosition, GrabPosition.position);
                }
            }
        }
        


        /// <summary>
        /// Will try to grab the first grabbable in front of the controller using a ray-cast
        /// </summary>
        public bool TryGrabFocused()
        {
            if (FocusController.IsFocusing 
                && CanGrab(FocusController.FocusTarget.BodyOrGameObject, FocusController.FocusPosition, out var grabbable))
            {
                return TryGrab(grabbable, FocusController.FocusPosition);
            }
            return false;
        }


        /// <summary>
        /// Grab a given grabbable
        /// </summary>
        /// <param name="grabbable"></param>
        /// <param name="grabPosition"></param>
        bool TryGrab(Grabbable grabbable, Vector3 grabPosition)
        {
            //Debug.DrawLine(grabPosition, grabPosition + new Vector3(0,1,0), Color.green, 10);
            //ReleaseGrabbed();

            foreach (var solver in Solvers)
            {
                if (solver.CanGrab(this, grabbable.gameObject, grabPosition))
                {
                    solver.Grab(this, grabbable.gameObject, grabPosition);
                    Internals.CurrentSolver = solver;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Release currently grabbed grabbable
        /// </summary>
        public void ReleaseGrabbed()
        {
            if (Internals.GrabbedGrabbable == null) return;
            Internals.CurrentSolver.Release(this);
        }

        public void ThrowGrabbed()
        {
            if (Internals.GrabbedGrabbable == null) return;
            Internals.CurrentSolver.Throw(this, Internals.LocalGrabbedPosition, GrabPosition.forward);
        }



        /// <summary>
        /// Grab a given grabbable
        /// </summary>
        /// <param name="grabbable"></param>
        /// <param name="grabPosition"></param>
        public void DoGrab(IGrabberSolver solverFrom, Grabbable grabbable, Vector3 grabPosition)
        {
            Internals.GrabbedGrabbable = grabbable;
            //Internals.GrabbedParent = grabbable.transform.parent;
            if (ChangedGrabbedObjectLayer)
            {
                Internals.PreviousGrabbedObjectLayer = grabbable.gameObject.layer;
                SetGameObjectLayer(grabbable.gameObject, GrabbedObjectLayer.LayerIndex);
            }
            Internals.LocalGrabbedPosition = grabbable.transform.InverseTransformPoint(grabPosition);
            grabbable.GrabBy(this, grabPosition);
            Internals.Processor.Begin(new (this), OnGrab, EventParameters.Trigger(grabbable.gameObject, grabbable.gameObject, grabPosition));
        }

        /// <summary>
        /// Release currently grabbed grabbable
        /// </summary>
        public void DoReleaseGrabbed(IGrabberSolver solverFrom)
        {
            if (Internals.GrabbedGrabbable == null) return;
            if (ChangedGrabbedObjectLayer)
                SetGameObjectLayer(Internals.GrabbedGrabbable.gameObject, Internals.PreviousGrabbedObjectLayer);
            Internals.Processor.End(new Owner(this), OnGrab, OnRelease, EventParameters.WithoutTrigger(gameObject));
            Internals.GrabbedGrabbable.ReleaseBy(this);
            Internals.GrabbedGrabbable = null;
        }



        void SetGameObjectLayer(GameObject obj, int layer)
        {
            obj.layer = layer;
            for (int i = 0; i < obj.transform.childCount; i++)
                SetGameObjectLayer(obj.transform.GetChild(i).gameObject, layer);
        }


        public void RotateGrabbed(Vector2 rotateHV)
        {
            if (Internals.CurrentSolver != null)
            {
                //quaternion.EulerXYZ()
                //GrabPosition.right
                //Internals.CurrentSolver.RotateHV(rotateHV);
            }
        }
    }

}