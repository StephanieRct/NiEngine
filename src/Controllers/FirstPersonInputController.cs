using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace NiEngine
{
    [RequireComponent(typeof(PlayerInput))]
    public class FirstPersonInputController : NiBehaviour
    {
		[Header("Character Input Values")]
		public Vector2 move;
		public Vector2 look;
		public bool jump;
		public bool sprint;
        public bool Rotating;
        public float RotatingSpeed;

		[Header("Movement Settings")]
		public bool analogMovement;

		[Header("Mouse Cursor Settings")]
		public bool cursorLocked = true;
		public bool cursorInputForLook = true;
		bool m_LockControls = true;
		bool m_LockControlsExceptInteract = false;

        [NotSaved]
        public GrabberController Grabber;
        [NotSaved]
        public FocusController Focus;

        [Tooltip("Trigger is object currently in focus")]
        public ActionSet ActionsOnInteract;

        [Tooltip("When the player toggles their hand")]
        public ActionSet OnToggleFlashlight;

        [Tooltip("When the player throw the currently gabbed object")]
        public ActionSet ActionsOnThrow;

        [Tooltip("Trigger is object currently in focus")]
        public ActionSet ActionsOnInventory;

        [Tooltip("Trigger is object to put into inventory")]
        public ActionSet ActionsOnPutItemInInventory;

        [Tooltip("Trigger is object currently in focus")]
        public ActionSet ActionsOnPullItemFromInventory;
        public EventProcessor Processor;

        void Awake()
        {
            Focus ??= GetComponent<FocusController>() ?? FindObjectOfType<FocusController>();
            if (Focus is null)
            {
                Debug.LogError($"{nameof(FirstPersonInputController)} focus controller not set", this);
                return;
            }

            Grabber ??= GetComponent<GrabberController>() ?? FindObjectOfType<GrabberController>();
            if (Grabber is null)
            {
                Debug.LogError($"{nameof(FirstPersonInputController)} grabber not set", this);
                return;
            }
        }
        public void LockControls()
        {
			cursorLocked = true;
            cursorInputForLook = true;
			m_LockControls = true;
			m_LockControlsExceptInteract = true;
			move = Vector3.zero;
			look = Vector3.zero;
			jump = false;
			sprint = false;
			SetCursorState(cursorLocked);

        }
        public void UnlockControlsExceptInteract()
        {
	        UnlockControls();
	        
	        cursorLocked = true;
	        m_LockControlsExceptInteract = true;
	        SetCursorState(cursorLocked);
        }
        public void UnlockControls()
        {
            cursorLocked = false;
            cursorInputForLook = false;
            m_LockControls = false;
            m_LockControlsExceptInteract = false;
            move = Vector3.zero;
            look = Vector3.zero;
            jump = false;
            sprint = false;
            SetCursorState(cursorLocked);

        }
        public void OnMove(InputValue value)
        {
            if (!m_LockControls) return;
            MoveInput(value.Get<Vector2>());
		}

		public void OnLook(InputValue value)
        {
            if (!m_LockControls) return;
            if (cursorInputForLook)
			{
				LookInput(value.Get<Vector2>());
			}
		}

		public void OnJump(InputValue value)
		{
			if (!m_LockControls) return;
			JumpInput(value.isPressed);
		}

		public void OnSprint(InputValue value)
        {
            if (!m_LockControls) return;
            SprintInput(value.isPressed);
        }

        int ActWithFocusTarget(ActionSet actionSet)
        {
            var owner = new Owner(this);
            var actCount = 0;
            if (Focus.FocusTarget.GameObject != null)
            {
                actCount = Processor.Act(owner, actionSet, Processor.MakeEvent(owner, Focus.FocusTarget.GameObject, Focus.FocusPosition));
                if (actCount == 0 && Focus.FocusTarget.Body != null)
                {
                    Debug.Log($"Sending to body {Focus.FocusTarget.Body.GetNameOrNull()}");
                    actCount = Processor.Act(owner, actionSet, Processor.MakeEvent(owner, Focus.FocusTarget.Body, Focus.FocusPosition));
                }
                return actCount;
            }
            else
            {
                actCount = Processor.Act(owner, actionSet, Processor.MakeEvent(owner, null, Focus.FocusPosition));
            }

            return actCount;
        }
        public void OnInteract()
        {
            if (!m_LockControls && !m_LockControlsExceptInteract) return;
            //Debug.Log($"OnInteract", this);
            if (Focus.IsFocusing)
                ActWithFocusTarget(ActionsOnInteract);
        }
        public void OnFlashlight()
        {
            if (!m_LockControls) return;
            var owner = new Owner(this);
            Processor.Act(owner, OnToggleFlashlight, Processor.MakeEvent(owner));
        }
        public void OnInventory()
        {
            if (!m_LockControls) return;
            //Debug.Log($"OnInventory", this);
            ActWithFocusTarget(ActionsOnInventory);
        }
        public void OnThrow()
        {
            if (!m_LockControls) return;
            Grabber.ThrowGrabbed();
            ActWithFocusTarget(ActionsOnThrow);
        }
        public void OnPutPullItem()
        {
            var owner = new Owner(this);
            //Debug.Log($"OnPutPullItem", this);
            if (Grabber.IsGrabbing)
                Processor.Act(owner, ActionsOnPutItemInInventory, Processor.MakeEvent(owner, Grabber.GrabbedGrabbable.gameObject, Grabber.GrabPosition.position));
            else
                ActWithFocusTarget(ActionsOnPullItemFromInventory);
        }
        
        public void OnGrabOrRelease(InputValue value)
        {
            if (Grabber.IsGrabbing)
                Grabber.ReleaseGrabbed();
            else
                Grabber.TryGrabFocused();
        }

        public void OnRotate(InputValue value)
        {
            Rotating = value.isPressed;
            Debug.Log($"Rotating {Rotating}");
        }



        public void MoveInput(Vector2 newMoveDirection)
        {
            if (!m_LockControls) return;
            move = newMoveDirection;
        }

        public void LookInput(Vector2 newLookDirection)
        {
            if (!m_LockControls) return;
            look = newLookDirection;
        }

        public void JumpInput(bool newJumpState)
        {
            if (!m_LockControls) return;
            jump = newJumpState;
        }

        public void SprintInput(bool newSprintState)
        {
            if (!m_LockControls) return;
            sprint = newSprintState;
        }

        private void OnApplicationFocus(bool hasFocus)
		{
			SetCursorState(cursorLocked);
		}
        private void Start()
        {
            SetCursorState(cursorLocked);
        }
        private void SetCursorState(bool newState)
		{
			Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
		}


    }
	
}