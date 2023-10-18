using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace NiEngine
{

    /// <summary>
    /// Make this object RigidBody fixed while any Fixed Points are alive.
    /// Will set this object RigidBody.isKinematic to true when fixed.
    /// Will set this object RigidBody.isKinematic to false when all Fixed Points are destroyed.
    /// </summary>
    [AddComponentMenu("Nie/Object/Constraints/FixedConstraint")]
    [RequireComponent(typeof(Rigidbody))]
    public class FixedConstraint : MonoBehaviour
    {

        [Tooltip("Objects keeping this FixedConstraint fixed.")]
        public List<Transform> FixedPoints = new();

        void Start()
        {
            GetComponent<Rigidbody>().isKinematic = true;
        }

        void Update()
        {
            if (FixedPoints.All(x => x == null))
                GetComponent<Rigidbody>().isKinematic = false;
        }
    }
}