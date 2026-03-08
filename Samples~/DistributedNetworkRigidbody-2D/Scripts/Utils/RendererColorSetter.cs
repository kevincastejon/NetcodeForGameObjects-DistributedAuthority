using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Caskev.Samples.NetcodeForGameObjects.DistributedAuthority.DistributedRigidbody2D
{
    public class RendererColorSetter : MonoBehaviour
    {
        [SerializeField] private Renderer _renderer;
        [SerializeField] private Color _authorityColor;
        [SerializeField] private Color _requestingColor;
        [SerializeField] private Color _remoteColor;
        [SerializeField] private Color _decliningColor;
        [SerializeField] private Color _lockedColor;
        [SerializeField] private Color _unlockedColor;
        private void Awake()
        {
            _renderer = _renderer ? _renderer : GetComponent<Renderer>();
        }
        public void Start()
        {
        }
        public void SetAuthorityColor()
        {
            _renderer.material.color = _authorityColor;
        }
        public void SetRequestingColor()
        {
            _renderer.material.color = _requestingColor;
        }
        public void SetRemoteColor()
        {
            _renderer.material.color = _remoteColor;
        }
        public void SetDecliningColor()
        {
            _renderer.material.color = _decliningColor;
        }
        public void SetLockedColor()
        {
            _renderer.material.color = _lockedColor;
        }
        public void SetUnlockedColor()
        {
            _renderer.material.color = _unlockedColor;
        }
    }
}
