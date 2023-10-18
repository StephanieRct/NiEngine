//#define FIRSTPERSONCONTROLLER_DEBUG
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NiEngine
{
    [AddComponentMenu("Nie/Player/FirstPersonController")]
    [RequireComponent(typeof(CharacterController))]
	public class FirstPersonController : MonoBehaviour
	{
		[Header("Player")]
		[NotSaved, Tooltip("Move speed of the character in m/s")]
		public float MoveSpeed = 4.0f;
		[NotSaved, Tooltip("Sprint speed of the character in m/s")]
		public float SprintSpeed = 6.0f;
		[NotSaved, Tooltip("Rotation speed of the character")]
		public float RotationSpeed = 1.0f;
        [NotSaved, Tooltip("Minimum input value to perform rotation")]
        public float RotationInputThreshold = 0.01f;
        [NotSaved, Tooltip("Acceleration and deceleration")]
		public float SpeedChangeRate = 10.0f;

		[Space(10)]
		[NotSaved, Tooltip("The height the player can jump")]
		public float JumpHeight = 1.2f;
		[NotSaved, Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
		public float Gravity = -15.0f;

		[Space(10)]
		[NotSaved, Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
		public float JumpTimeout = 0.1f;
		[NotSaved, Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
		public float FallTimeout = 0.15f;

		[Header("Player Grounded")]
		[NotSaved, Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
		public bool Grounded = true;
		[NotSaved, Tooltip("Useful for rough ground")]
		public float GroundedOffset = -0.14f;
		[NotSaved, Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
		public float GroundedRadius = 0.5f;
		[NotSaved, Tooltip("What layers the character uses as ground")]
		public LayerMask GroundLayers;

		[Header("Cinemachine")]
		[NotSaved, Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
		public GameObject CinemachineCameraTarget;
		[NotSaved, Tooltip("How far in degrees can you move the camera up")]
		public float TopClamp = 90.0f;
		[NotSaved, Tooltip("How far in degrees can you move the camera down")]
		public float BottomClamp = -90.0f;

        [NotSaved]
        public PlayerInput PlayerInput;
        [NotSaved]
        public FirstPersonInputController InputController;

        // cinemachine
        private float _cinemachineTargetPitch;

		// player
		private float _speed;
		private float _rotationVelocity;
		private float _verticalVelocity;
		private float _terminalVelocity = 53.0f;

		// timeout deltatime
		private float _jumpTimeoutDelta;
		private float _fallTimeoutDelta;



        [NotSaved]
        private CharacterController _controller;

        [NotSaved]
        private GameObject _mainCamera;

		private bool IsCurrentDeviceMouse => PlayerInput.currentControlScheme == "KeyboardMouse";

		private void Awake()
		{
			// get a reference to our main camera
			if (_mainCamera == null)
			{
				_mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            }

            _controller = GetComponent<CharacterController>();

            InputController ??= GetComponent<FirstPersonInputController>() ?? FindObjectOfType<FirstPersonInputController>();
            if (InputController is null)
            {
                Debug.LogError($"{nameof(FirstPersonController)} InputController not set", this);
                return;
            }

            PlayerInput ??= GetComponent<PlayerInput>() ?? FindObjectOfType<PlayerInput>();
            if (PlayerInput is null)
            {
                Debug.LogError($"{nameof(FirstPersonController)} PlayerInput not set", this);
                return;
            }
        }

        private void Start()
		{

			// reset our timeouts on start
			_jumpTimeoutDelta = JumpTimeout;
			_fallTimeoutDelta = FallTimeout;
		}

		private void Update()
		{
			JumpAndGravity();
			GroundedCheck();
			Move();
		}

		private void LateUpdate()
		{
			CameraRotation();
		}

		private void GroundedCheck()
		{
			// set sphere position, with offset
			Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
			Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);
		}

		private void CameraRotation()
		{
			// if there is an input
			if (InputController.look.sqrMagnitude >= RotationInputThreshold)
			{
				//Don't multiply mouse input by Time.deltaTime
				float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;
				
				_cinemachineTargetPitch += InputController.look.y * RotationSpeed * deltaTimeMultiplier;
				_rotationVelocity = InputController.look.x * RotationSpeed * deltaTimeMultiplier;

				// clamp our pitch rotation
				_cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

				// Update Cinemachine camera target pitch
				CinemachineCameraTarget.transform.localRotation = Quaternion.Euler(_cinemachineTargetPitch, 0.0f, 0.0f);

				// rotate the player left and right
				transform.Rotate(Vector3.up * _rotationVelocity);
			}
		}

		private void Move()
		{
			// set target speed based on move speed, sprint speed and if sprint is pressed
			float targetSpeed = InputController.sprint ? SprintSpeed : MoveSpeed;

			// a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

			// note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
			// if there is no input, set the target speed to 0
			if (InputController.move == Vector2.zero) targetSpeed = 0.0f;

			// a reference to the players current horizontal velocity
			float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

			float speedOffset = 0.1f;
			float inputMagnitude = InputController.analogMovement ? InputController.move.magnitude : 1f;

			// accelerate or decelerate to target speed
			if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
			{
				// creates curved result rather than a linear one giving a more organic speed change
				// note T in Lerp is clamped, so we don't need to clamp our speed
				_speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * SpeedChangeRate);

				// round speed to 3 decimal places
				_speed = Mathf.Round(_speed * 1000f) / 1000f;
			}
			else
			{
				_speed = targetSpeed;
			}

			// normalise input direction
			Vector3 inputDirection = new Vector3(InputController.move.x, 0.0f, InputController.move.y).normalized;

			// note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
			// if there is a move input rotate player when the player is moving
			if (InputController.move != Vector2.zero)
			{
				// move
				inputDirection = transform.right * InputController.move.x + transform.forward * InputController.move.y;
			}

			// move the player 
            var horizontalVelocity = inputDirection.normalized * _speed;
            var finalVelocity = horizontalVelocity + new Vector3(0.0f, _verticalVelocity, 0.0f);
			_controller.Move(finalVelocity * Time.deltaTime);
#if FIRSTPERSONCONTROLLER_DEBUG
			Debug.DrawLine(transform.position, transform.position + horizontalVelocity, Color.red);
#endif
		}

		private void JumpAndGravity()
		{
			if (Grounded)
			{
				// reset the fall timeout timer
				_fallTimeoutDelta = FallTimeout;

				// stop our velocity dropping infinitely when grounded
				if (_verticalVelocity < 0.0f)
                    _verticalVelocity = -2f;

                // Jump
                if (InputController.jump && _jumpTimeoutDelta <= 0.0f)
				{
					// the square root of H * -2 * G = how much velocity needed to reach desired height
					_verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);
				}

				// jump timeout
				if (_jumpTimeoutDelta >= 0.0f)
                    _jumpTimeoutDelta -= Time.deltaTime;
            }
			else
			{
				// reset the jump timeout timer
				_jumpTimeoutDelta = JumpTimeout;

				// fall timeout
				if (_fallTimeoutDelta >= 0.0f)
				{
					_fallTimeoutDelta -= Time.deltaTime;
				}

				// if we are not grounded, do not jump
				InputController.jump = false;
			}

			// apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
			if (_verticalVelocity < _terminalVelocity)
			{
				_verticalVelocity += Gravity * Time.deltaTime;
			}
		}

		private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
		{
			if (lfAngle < -360f) lfAngle += 360f;
			if (lfAngle > 360f) lfAngle -= 360f;
			return Mathf.Clamp(lfAngle, lfMin, lfMax);
		}

		private void OnDrawGizmosSelected()
		{
			Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
			Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

			if (Grounded) Gizmos.color = transparentGreen;
			else Gizmos.color = transparentRed;

			// when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
			Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z), GroundedRadius);
		}
	}
}