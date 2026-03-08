using Caskev.NetcodeForGameObjects.DistributedAuthority;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
namespace Caskev.Samples.NetcodeForGameObjects.DistributedAuthority.DistributedRigidbody2D
{
    public class CursorPlayer : NetworkBehaviour
    {
        [Tooltip("Mouse input action")]
        [SerializeField] private InputAction _mousePositionInput;
        [SerializeField] private InputAction _mouseClickInput;
        [Tooltip("Throw rigidbodies on release")]
        [SerializeField] private bool _throwOnRelease;
        private Camera _camera;
        private Renderer _renderer;
        private DistributedNetworkRigidbody2D _draggedPhysicsObject;
        private Vector2 _lastPosition;
        private Vector2 _kinematicVelocity;
        private void Awake()
        {
            _camera = Camera.main;
            _renderer = GetComponent<Renderer>();
        }
        private void OnEnable()
        {
            _mousePositionInput.Enable();
            _mouseClickInput.Enable();
        }
        private void OnDisable()
        {
            _mousePositionInput.Disable();
            _mouseClickInput.Disable();
        }
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (IsOwner)
            {
                _renderer.enabled = false;
            }
            else
            {
                enabled = false;
            }
        }
        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
        }
        private void FixedUpdate()
        {
            if (_draggedPhysicsObject)
            {
                if (_draggedPhysicsObject.Rigidbody.bodyType != RigidbodyType2D.Kinematic)
                {
                    _draggedPhysicsObject.Rigidbody.bodyType = RigidbodyType2D.Kinematic;
                }
                _draggedPhysicsObject.Rigidbody.MovePosition(_camera.ScreenToWorldPoint(_mousePositionInput.ReadValue<Vector2>()));
                _kinematicVelocity = (_draggedPhysicsObject.Rigidbody.position - _lastPosition) / Time.fixedDeltaTime;
                _lastPosition = _draggedPhysicsObject.Rigidbody.position;
            }
        }
        private void Update()
        {
            FollowMouse();
            if (_draggedPhysicsObject)
            {
                if (!_mouseClickInput.IsPressed())
                {
                    if (_draggedPhysicsObject.OriginalKinematicState != RigidbodyType2D.Kinematic || _throwOnRelease)
                    {
                        _draggedPhysicsObject.Rigidbody.bodyType = _draggedPhysicsObject.OriginalKinematicState;
                        _draggedPhysicsObject.Rigidbody.linearVelocity = _kinematicVelocity;
                    }
                    _draggedPhysicsObject.UnlockOwnership();
                    _draggedPhysicsObject = null;
                }
            }
            else if (_mouseClickInput.WasPressedThisFrame())
            {
                RaycastHit2D hit = Physics2D.GetRayIntersection(_camera.ScreenPointToRay(_mousePositionInput.ReadValue<Vector2>()));
                if (hit.collider)
                {
                    DistributedNetworkRigidbody2D ball = hit.collider.GetComponent<DistributedNetworkRigidbody2D>();
                    if (ball)
                    {
                        if (ball && (ball.IsOwner || !ball.IsOwnershipLocked))
                        {
                            _draggedPhysicsObject = ball;
                            _draggedPhysicsObject.Rigidbody.bodyType = RigidbodyType2D.Kinematic;
                            ball.RequestOwnership();
                        }
                    }
                }
            }
        }

        private void FollowMouse()
        {
            if (IsSpawned && IsOwner)
            {
                transform.position = (Vector2)_camera.ScreenToWorldPoint(_mousePositionInput.ReadValue<Vector2>());
            }
        }
    }
}
