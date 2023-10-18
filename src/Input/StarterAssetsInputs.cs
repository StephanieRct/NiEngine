using UnityEngine;
using UnityEngine.InputSystem;

namespace NiEngine
{
	public class StarterAssetsInputs : NiBehaviour
    {
		[Header("Character Input Values")]
		public Vector2 move;
		public Vector2 look;
		public bool jump;
		public bool sprint;

		[Header("Movement Settings")]
		public bool analogMovement;

		[Header("Mouse Cursor Settings")]
		public bool cursorLocked = true;
		public bool cursorInputForLook = true;
		bool m_LockControls = true;

        public GrabberController Grabber;
        public FocusController Focus;
        //public bool InteractWithFocusGameObject = true;
        //public bool InteractWithFocusBody;
        public ActionSet ActionsOnInteract;
        public EventProcessor Processor;

        public void LockControls()
        {
			cursorLocked = true;
            cursorInputForLook = true;
			m_LockControls = true;
			move = Vector3.zero;
			look = Vector3.zero;
			jump = false;
			sprint = false;
			SetCursorState(cursorLocked);

        }
        public void UnlockControls()
        {
            cursorLocked = false;
            cursorInputForLook = false;
            m_LockControls = false;
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

        public void OnInteract()
        {
            //Debug.Log($"OnInteract", this);
            if (Focus is null)
            {
                Debug.LogError($"{nameof(StarterAssetsInputs)} focus controller not set", this);
                return;
            }

            if (Focus.IsFocusing)
            {
                var owner = new Owner(this);
                var actCount = Processor.Act(owner, ActionsOnInteract, Processor.MakeEvent(owner, Focus.FocusTarget.GameObject, Focus.FocusPosition));
                if(actCount == 0 && Focus.FocusTarget.Body != null)
                    Processor.Act(owner, ActionsOnInteract, Processor.MakeEvent(owner, Focus.FocusTarget.Body, Focus.FocusPosition));
            }
        }
        public void OnGrabOrRelease(InputValue value)
        {

            if (Grabber is null)
            {
                Debug.LogError($"{nameof(StarterAssetsInputs)} grabber not set", this);
                return;
            }
            if (Grabber.IsGrabbing)
                Grabber.ReleaseGrabbed();
            else
                Grabber.TryGrabFocused();
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