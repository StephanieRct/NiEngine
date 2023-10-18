using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NiEngine
{
    [AddComponentMenu("Nie/Player/TouchController")]
    public class TouchController : MonoBehaviour
    {
        [NotSaved, Tooltip("touch only objects of these layers")]
        public LayerMask LayerMask = -1;

        [NotSaved, Tooltip("touch only objects closer to this distance")]
        public float MaxDistance = 10;
        [NotSaved, Tooltip("Where the ray cast to detect any touchable will be directer toward. If left null, will ray cast in the middle of the screen")]
        public Transform TouchPositionObject;

        [NotSaved, Tooltip("Output debug log when objects are grabbed or released")]
        public bool DebugLog;

        
        public Vector3 LastTouchedPosition;
        public Vector3 RayCastTarget => TouchPositionObject != null ? TouchPositionObject.position : transform.position + transform.forward;

        ReactOnTouch m_Touching;
        void Update()
        {
            // TODO tie this into the input system
            if (Input.GetMouseButton(0))
                TryTouchInFront();
            else
                Release();
        }

        public void TryTouchInFront()
        {
            var ray = new Ray(transform.position, (RayCastTarget - transform.position).normalized);
            if (Physics.Raycast(ray, out var hit, MaxDistance, LayerMask.value))
            {
                if (DebugLog)
                    Debug.Log($"[{Time.frameCount}] ToucherController TryTouchInFront '{name}' ray hits '{hit.transform.name}'");
                if (hit.collider.gameObject.TryGetComponent<ReactOnTouch>(out var touchable) && touchable.CanTouch(this, hit.point))
                    Touch(touchable, hit.point);
                else
                    Release();
            }
            else
                Release();
        }

        public void Touch(ReactOnTouch touchable, Vector3 position)
        {
            LastTouchedPosition = position;
            if (m_Touching == touchable) return;
            if (DebugLog)
                Debug.Log($"[{Time.frameCount}] ToucherController '{name}' touches '{touchable.name}'");
            if (TryGetComponent<FocusController>(out var focusCtrl))
                focusCtrl.Unfocus();
            m_Touching = touchable;
            touchable.Touch(this, position);
        }

        public void Release()
        {
            if (m_Touching == null) return;
            if (DebugLog)
                Debug.Log($"[{Time.frameCount}] ToucherController '{name}' Release ReactOnTouch '{m_Touching.name}'");
            m_Touching.Release(this, LastTouchedPosition);
            m_Touching = null;
        }
    }
}