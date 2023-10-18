using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;

namespace NiEngine.Components
{
    public abstract class MoveToBase : NiBehaviour
    {
        public Transform Target;
        public Move.Method MoveMethod = Move.Method.TransformSet;

        public NiTransform Offset;
        private Rigidbody m_Rigidbody;
        private Rigidbody Rigidbody => m_Rigidbody ??= gameObject.GetComponent<Rigidbody>();

        protected void UpdateSnap() => Move.Object(gameObject, Rigidbody, MoveMethod, NiTransform.AddTransform(NiTransform.RigidTransformOf(Target), Offset));
    }

    [AddComponentMenu("NiEngine/Constraints/MoveToUpdate")]
    public class MoveToUpdate : MoveToBase
    {
        void Update() => UpdateSnap();
    }

    [AddComponentMenu("NiEngine/Constraints/MoveToFixedUpdate")]
    public class MoveToFixedUpdate : MoveToBase
    {
        void FixedUpdate() => UpdateSnap();
    }

    [AddComponentMenu("NiEngine/Constraints/MoveToLateUpdate")]
    public class MoveToLateUpdate : MoveToBase
    {
        void LateUpdate() => UpdateSnap();
    }
}