using System.Collections.Generic;
using UnityEngine;

namespace Vchod.Player
{
    /// <summary>
    /// First-person walker. No jump (GDD: none; the house is about walking, hiding and air).
    /// Symptom systems slow it down through SpeedMultiplier and LookMultiplier.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerController : MonoBehaviour
    {
        [SerializeField] private PlayerConfig config;
        [SerializeField] private Transform cameraPivot;

        private CharacterController _body;
        private PlayerInputActions _input;
        private Vector3 _planarVelocity;
        private float _verticalSpeed;
        private float _pitch;
        private readonly HashSet<object> _inputLocks = new();

        public PlayerConfig Config => config;
        public Transform CameraPivot => cameraPivot;
        public PlayerInputActions Input => _input;

        /// <summary>1 = normal. Lowered by CO symptoms.</summary>
        public float SpeedMultiplier { get; set; } = 1f;

        /// <summary>1 = normal. Lowered by CO symptoms ("slower turning").</summary>
        public float LookMultiplier { get; set; } = 1f;

        public bool IsSprinting { get; private set; }
        public float PlanarSpeed => _planarVelocity.magnitude;

        /// <summary>False while anything holds an input lock (collapse, debug UI, later menus).</summary>
        public bool InputEnabled => _inputLocks.Count == 0;

        public void SetInputLock(object owner, bool locked)
        {
            if (locked) _inputLocks.Add(owner);
            else _inputLocks.Remove(owner);
            ApplyCursor();
        }

        private void Awake()
        {
            _body = GetComponent<CharacterController>();
            _input = new PlayerInputActions();
        }

        private void OnEnable()
        {
            _input.Enable();
            ApplyCursor();
        }

        private void OnDisable() => _input.Disable();

        private void OnDestroy() => _input.Dispose();

        private void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus) ApplyCursor();
        }

        private void ApplyCursor()
        {
            Cursor.lockState = InputEnabled ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !InputEnabled;
        }

        private void Update()
        {
            var dt = Time.deltaTime;
            if (InputEnabled) UpdateLook(dt);
            UpdateMove(dt);
        }

        private void UpdateLook(float dt)
        {
            var look = _input.LookMouse.ReadValue<Vector2>() * config.mouseSensitivity
                       + _input.LookStick.ReadValue<Vector2>() * (config.gamepadLookSpeed * dt);
            look *= LookMultiplier;

            transform.Rotate(0f, look.x, 0f, Space.Self);
            _pitch = Mathf.Clamp(_pitch - look.y, -config.maxPitch, config.maxPitch);
            cameraPivot.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
        }

        private void UpdateMove(float dt)
        {
            var move = InputEnabled ? Vector2.ClampMagnitude(_input.Move.ReadValue<Vector2>(), 1f) : Vector2.zero;
            IsSprinting = InputEnabled && _input.Sprint.IsPressed() && move.y > 0.1f;
            var speed = (IsSprinting ? config.sprintSpeed : config.walkSpeed) * SpeedMultiplier;
            var target = (transform.forward * move.y + transform.right * move.x) * speed;

            _planarVelocity = Vector3.MoveTowards(_planarVelocity, target, config.acceleration * dt);

            _verticalSpeed = _body.isGrounded ? -1f : _verticalSpeed + config.gravity * dt;
            _body.Move((_planarVelocity + Vector3.up * _verticalSpeed) * dt);
        }

        /// <summary>Moves the player instantly (reshuffle, debug teleport). Keeps pitch.</summary>
        public void Teleport(Vector3 position, float yaw)
        {
            _body.enabled = false;
            transform.SetPositionAndRotation(position, Quaternion.Euler(0f, yaw, 0f));
            _body.enabled = true;
            _planarVelocity = Vector3.zero;
            _verticalSpeed = 0f;
        }
    }
}
