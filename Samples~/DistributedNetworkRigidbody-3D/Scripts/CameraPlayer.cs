using Caskev.NetcodeForGameObjects.DistributedAuthority;
using Unity.Netcode;
using UnityEngine;

namespace Caskev.Samples.NetcodeForGameObjects.DistributedNetworkRigidbody
{
    public class CameraPlayer : NetworkBehaviour
    {
        [Tooltip("Throw rigidbodies on release")]
        [SerializeField] private bool _throwOnRelease;
        [SerializeField] private Transform _grabPoint;
        private Camera _camera;
        public float acceleration = 50; // how fast you accelerate
        public float accSprintMultiplier = 4; // how much faster you go when "sprinting"
        public float lookSensitivity = 1; // mouse look sensitivity
        public float dampingCoefficient = 5; // how quickly you break to a halt after you stop your input
        public bool focusOnEnable = true; // whether or not to focus and lock cursor immediately on enable

        private DistributedNetworkTransform _draggedObject;
        private Caskev.NetcodeForGameObjects.DistributedAuthority.DistributedNetworkRigidbody _draggedPhysicsObject;

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
            if (focusOnEnable) Focused = true;
        }
        private void OnDisable() => Focused = false;
        private void FixedUpdate()
        {
            if (_draggedPhysicsObject)
            {
                if (!_draggedPhysicsObject.Rigidbody.isKinematic)
                {
                    _draggedPhysicsObject.Rigidbody.isKinematic = true;
                }
                _draggedPhysicsObject.Rigidbody.MovePosition(_grabPoint.position);
                _draggedPhysicsObject.Rigidbody.MoveRotation(_grabPoint.rotation);
                _kinematicVelocity = (_draggedPhysicsObject.Rigidbody.position - _lastPosition) / Time.fixedDeltaTime;
                _lastPosition = _draggedPhysicsObject.Rigidbody.position;
            }
        }
        private void Update()
        {
            // Input
            if (Focused)
                UpdateInput();
            else if (Input.GetMouseButtonDown(0))
                Focused = true;

            // Physics
            velocity = Vector3.Lerp(velocity, Vector3.zero, dampingCoefficient * Time.deltaTime);
            transform.position += velocity * Time.deltaTime;

            // Grabbing & UnGrabbing
            if (_draggedObject)
            {
                if (!Input.GetMouseButton(0))
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
            else if (_draggedPhysicsObject)
            {
                if (!Input.GetMouseButton(0))
                {
                    if (!_draggedPhysicsObject.OriginalKinematicState || _throwOnRelease)
                    {
                        _draggedPhysicsObject.Rigidbody.isKinematic = _draggedPhysicsObject.OriginalKinematicState;
                        _draggedPhysicsObject.Rigidbody.linearVelocity = _kinematicVelocity;
                    }
                    _draggedPhysicsObject.AutomaticallyDeclineOnAsleep = true;
                    _draggedPhysicsObject.UnlockOwnership();
                    _draggedPhysicsObject = null;
                }
            }
            else if (Input.GetMouseButtonDown(0))
            {
                if(Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, 3f))
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
                    else
                    {
                        Caskev.NetcodeForGameObjects.DistributedAuthority.DistributedNetworkRigidbody physicsObj = hit.collider.GetComponent<Caskev.NetcodeForGameObjects.DistributedAuthority.DistributedNetworkRigidbody>();
                        if (physicsObj && (physicsObj.IsOwner || !physicsObj.IsOwnershipLocked))
                        {
                            _draggedPhysicsObject = physicsObj;
                            _draggedPhysicsObject.AutomaticallyDeclineOnAsleep = false;
                            _grabPoint.position = _draggedPhysicsObject.Rigidbody.position;
                            _grabPoint.rotation = _draggedPhysicsObject.Rigidbody.rotation;
                            _draggedPhysicsObject.Rigidbody.isKinematic = true;
                            physicsObj.RequestOwnership();
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
            Vector2 mouseDelta = lookSensitivity * new Vector2(Input.GetAxis("Mouse X"), -Input.GetAxis("Mouse Y"));
            Quaternion rotation = transform.rotation;
            Quaternion horiz = Quaternion.AngleAxis(mouseDelta.x, Vector3.up);
            Quaternion vert = Quaternion.AngleAxis(mouseDelta.y, Vector3.right);
            transform.rotation = horiz * rotation * vert;

            // Leave cursor lock
            if (Input.GetKeyDown(KeyCode.Escape))
                Focused = false;
        }
        private Vector3 GetAccelerationVector()
        {
            Vector3 moveInput = default;

            void AddMovement(KeyCode key, Vector3 dir)
            {
                if (Input.GetKey(key))
                    moveInput += dir;
            }

            AddMovement(KeyCode.W, Vector3.forward);
            AddMovement(KeyCode.S, Vector3.back);
            AddMovement(KeyCode.D, Vector3.right);
            AddMovement(KeyCode.A, Vector3.left);
            AddMovement(KeyCode.Space, Vector3.up);
            AddMovement(KeyCode.LeftControl, Vector3.down);
            Vector3 direction = transform.TransformVector(moveInput.normalized);

            if (Input.GetKey(KeyCode.LeftShift))
                return direction * (acceleration * accSprintMultiplier); // "sprinting"
            return direction * acceleration; // "walking"
        }
    }
}