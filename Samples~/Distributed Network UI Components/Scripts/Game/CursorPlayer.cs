using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
namespace Caskev.Samples.NetcodeForGameObjects.DistributedAuthority.DistributedNetworkUIComponents
{
    public class CursorPlayer : NetworkBehaviour
    {
        [Tooltip("Mouse input action")]
        [SerializeField] private InputAction _mousePositionInput;
        [SerializeField] private InputAction _mouseClickInput;
        private Camera _camera;
        private Renderer _renderer;
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
