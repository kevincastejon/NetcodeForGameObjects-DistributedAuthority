using Caskev.NetcodeForGameObjects.DistributedAuthority;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
namespace Caskev.Samples.NetcodeForGameObjects.DistributedAuthority.DistributedNetworkTransform_2D
{
    public class CursorPlayer : NetworkBehaviour
    {
        [Tooltip("Mouse input action")]
        [SerializeField] private InputAction _mousePositionInput;
        [SerializeField] private InputAction _mouseClickInput;
        private Camera _camera;
        private Renderer _renderer;
        private DistributedNetworkTransform _draggedObject;
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
        private void Update()
        {
            FollowMouse();
            if (_draggedObject)
            {
                if (!_mouseClickInput.IsPressed())
                {
                    _draggedObject.DeclineOwnership(CompleteTransformPose.FromTransform(_draggedObject.transform));
                    _draggedObject = null;
                }
                else
                {
                    _draggedObject.transform.position = transform.position;
                }
            }
            else if (_mouseClickInput.WasPressedThisFrame())
            {
                RaycastHit2D hit = Physics2D.GetRayIntersection(_camera.ScreenPointToRay(_mousePositionInput.ReadValue<Vector2>()));
                if (hit.collider)
                {
                    DistributedNetworkTransform ball = hit.collider.GetComponent<DistributedNetworkTransform>();
                    if (ball)
                    {
                        if (ball.IsOwner || !ball.IsOwnershipLocked)
                        {
                            _draggedObject = ball;
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
