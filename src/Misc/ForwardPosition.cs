using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
namespace NiEngine
{
    /// <summary>
    /// Move this object to a distance from it's parent's Z axis.
    /// The distance change with the mouse wheel
    /// </summary>
    [AddComponentMenu("Nie/Object/Constraints/ForwardPosition")]
    public class ForwardPosition : MonoBehaviour
    {

        [Tooltip("Distance to move to from the parent's transform position on the forward axis")]
        public float Distance;

        [NotSaved, Tooltip("Minimum distance to limit to")]
        public float MinDistance = 0.1f;

        [NotSaved, Tooltip("Maximum distance to limit to")]
        public float MaxDistance = 0.75f;

        [NotSaved, Tooltip("The speed coefficient applied to the scrolling input to move on the parent's transform forward axis")]
        public float ScrollSpeed = 0.3f;

        /// <summary>
        /// Offset from the parent's transform position to start with.
        /// </summary>
        //Vector3 Offset;
        Vector3 ParentPosition => transform.parent == null ? Vector3.zero : transform.parent.position;
        Vector3 ParentForward => transform.parent == null ? new Vector3(0,0,1) : transform.parent.forward;
        public void SetDistanceTo(Vector3 v)
        {
            Debug.DrawLine(ParentPosition, v, Color.blue, 10);
            Distance = math.dot(v - ParentPosition, ParentForward);
            Distance = Mathf.Clamp(Distance, MinDistance, MaxDistance);
            Move();
        }
        void Start()
        {
            //Offset = transform.localPosition;
            Move();
        }

        void Update()
        {
            // TODO, tie this to the input system
            if (Input.mouseScrollDelta.y != 0)
            {
                Distance += Input.mouseScrollDelta.y * ScrollSpeed;
                Distance = Mathf.Clamp(Distance, MinDistance, MaxDistance);
                Move();
            }
        }

        void Move()
        {
            transform.localPosition = new Vector3(0, 0, Distance);// + Offset;
        }
    }

}