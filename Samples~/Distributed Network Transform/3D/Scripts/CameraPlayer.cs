using Caskev.Samples.NetcodeForGameObjects.DistributedAuthority.DistributedNetworkTransform_Shared;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace Caskev.Samples.NetcodeForGameObjects.DistributedAuthority.DistributedNetworkTransform_3D
{
    public class CameraPlayer : NetworkBehaviour
    {
        [SerializeField] private InputAction _lookInput;
        [SerializeField] private InputAction _grabInput;
        [SerializeField] private InputAction _moveInput;
        [SerializeField] private InputAction _moveUpInput;
        [SerializeField] private InputAction _moveDownInput;
        [SerializeField] private InputAction _sprintInput;
        [SerializeField] private InputAction _escapeInput;
        [SerializeField] private Transform _grabPoint;
        private Camera _camera;
        public float acceleration = 50; // how fast you accelerate
        public float accSprintMultiplier = 4; // how much faster you go when "sprinting"
        public float lookSensitivity = 0.1f; // mouse look sensitivity
        public float dampingCoefficient = 5; // how quickly you break to a halt after you stop your input
        public bool focusOnEnable = true; // whether or not to focus and lock cursor immediately on enable

        private DistributedNetworkTransform _draggedObject;

        private Vector3 _lastPosition;
        private Vector3 _kinematicVelocity;

        Vector3 velocity; // current velocity

        private static bool Focused
        {
            get => Cursor.lockState == CursorLockMode.Locked;
            set
            {
                Cursor.lockState = value ? CursorLockMode.Locked : CursorLockMode.None;
                Cursor.visible = value == false;
            }
        }


        private void Awake()
        {
            _camera = GetComponent<Camera>();
        }
        private void OnEnable()
        {
            _lookInput.Enable();
            _grabInput.Enable();
            _moveInput.Enable();
            _moveUpInput.Enable();
            _moveDownInput.Enable();
            _sprintInput.Enable();
            _escapeInput.Enable();
            if (focusOnEnable) Focused = true;
        }
        private void OnDisable()
        {
            _lookInput.Disable();
            _grabInput.Disable();
            _moveInput.Disable();
            _moveUpInput.Disable();
            _moveDownInput.Disable();
            _sprintInput.Disable();
            _escapeInput.Disable();
            Focused = false;
        }
        private void Update()
        {
            // Input
            if (Focused)
                UpdateInput();
            else if (_grabInput.WasPressedThisFrame())
                Focused = true;

            // Physics
            velocity = Vector3.Lerp(velocity, Vector3.zero, dampingCoefficient * Time.deltaTime);
            transform.position += velocity * Time.deltaTime;

            // Grabbing & UnGrabbing
            if (_draggedObject)
            {
                if (!_grabInput.IsPressed())
                {
                    _draggedObject.DeclineOwnership(CompleteTransformPose.FromTransform(_draggedObject.transform));
                    _draggedObject = null;
                }
                else
                {
                    _draggedObject.transform.position = _grabPoint.position;
                    _draggedObject.transform.rotation = _grabPoint.rotation;
                }
            }
            else if (_grabInput.WasPressedThisFrame())
            {
                if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, 3f))
                {
                    DistributedNetworkTransform obj = hit.collider.GetComponent<DistributedNetworkTransform>();
                    if (obj)
                    {
                        if (obj.IsOwner || !obj.IsOwnershipLocked)
                        {
                            _draggedObject = obj;
                            _grabPoint.position = _draggedObject.transform.position;
                            _grabPoint.rotation = _draggedObject.transform.rotation;
                            obj.RequestOwnership();
                        }
                    }
                }
            }
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (!IsOwner)
            {
                Destroy(_camera);
                Destroy(GetComponent<AudioListener>());
                enabled = false;
            }
        }
        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();

        }

        private void UpdateInput()
        {
            // Position
            velocity += GetAccelerationVector() * Time.deltaTime;

            // Rotation
            Vector2 mouseDelta = _lookInput.ReadValue<Vector2>();
            mouseDelta.y *= -1;
            mouseDelta = lookSensitivity * mouseDelta;
            Quaternion rotation = transform.rotation;
            Quaternion horiz = Quaternion.AngleAxis(mouseDelta.x, Vector3.up);
            Quaternion vert = Quaternion.AngleAxis(mouseDelta.y, Vector3.right);
            transform.rotation = horiz * rotation * vert;

            // Leave cursor lock
            if (_escapeInput.WasPressedThisFrame())
                Focused = false;
        }
        private Vector3 GetAccelerationVector()
        {
            Vector3 moveInput = default;

            void AddMovement(bool inputPressed, Vector3 dir)
            {
                if (inputPressed)
                    moveInput += dir;
            }
            Vector2 move = _moveInput.ReadValue<Vector2>();
            AddMovement(move.y > 0f, Vector3.forward);
            AddMovement(move.y < 0f, Vector3.back);
            AddMovement(move.x > 0f, Vector3.right);
            AddMovement(move.x < 0f, Vector3.left);
            AddMovement(_moveUpInput.IsPressed(), Vector3.up);
            AddMovement(_moveDownInput.IsPressed(), Vector3.down);
            Vector3 direction = transform.TransformVector(moveInput.normalized);

            if (_sprintInput.IsPressed())
                return direction * (acceleration * accSprintMultiplier); // "sprinting"
            return direction * acceleration; // "walking"
        }
    }
}