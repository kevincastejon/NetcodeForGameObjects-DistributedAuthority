using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.Events;
namespace Caskev.NetcodeForGameObjects.DistributedAuthority
{
    [GenerateSerializationForType(typeof(CompleteRigidbody2DPose))]
    public class DistributedNetworkRigidbody2D : DistributedAuthorityBehaviour<CompleteRigidbody2DPose>
    {
        [SerializeField] private bool _automaticallyDeclineOnAsleep;
        [SerializeField] private UnityEvent _onAsleep;
        [SerializeField] private UnityEvent _onAwake;
        private NetworkTransform _netTransform;
        private Rigidbody2D _rigidbody; 
        private RigidbodyType2D _originalKinematicState;
        private bool _lastSleepState;

        public Rigidbody2D Rigidbody { get => _rigidbody; }
        public bool AutomaticallyDeclineOnAsleep { get => _automaticallyDeclineOnAsleep; }
        public RigidbodyType2D OriginalKinematicState { get => _originalKinematicState; }

        private void Awake()
        {
            _netTransform = GetComponent<NetworkTransform>();
            _rigidbody = GetComponent<Rigidbody2D>();
            _originalKinematicState = _rigidbody.bodyType;
        }
        private void FixedUpdate()
        {
            bool newSleepState = _rigidbody.IsSleeping();
            if (_lastSleepState != newSleepState)
            {
                _lastSleepState = newSleepState;
                if (newSleepState)
                {
                    if (_automaticallyDeclineOnAsleep && CurrentState != DistributedAuthorityState.REMOTE)
                    {
                        DeclineOwnership(CompleteRigidbody2DPose.FromRigidbody(_rigidbody));
                    }
                    _onAsleep.Invoke();
                }
                else
                {
                    _onAwake.Invoke();
                }
            }
        }
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            _netTransform.enabled = true;
        }
        public override void OnOwnershipGained(ulong clientId)
        {
            base.OnOwnershipGained(clientId);
            if (clientId == NetworkManager.LocalClientId)
            {
                _netTransform.enabled = true;
            }
        }
        public override void OnOwnershipLost(ulong clientId)
        {
            base.OnOwnershipLost(clientId);
            if (clientId == NetworkManager.LocalClientId)
            {
                _netTransform.enabled = IsServer || CurrentState == DistributedAuthorityState.REMOTE;
            }
        }
        public override void OnEnterAuthority()
        {
            _netTransform.enabled = true;
            _rigidbody.bodyType = _originalKinematicState;
        }
        public override void OnEnterRemote()
        {
            _netTransform.enabled = true;
        }
        public override void OnEnterRequesting()
        {
            _netTransform.enabled = IsOwner;
        }
        public override void OnEnterDeclining()
        {
            _netTransform.enabled = IsOwner;
        }

        public override void OnClientDecliningServerSide(DecliningReason decliningReason, bool isDecliningDataProvided, CompleteRigidbody2DPose decliningData)
        {
            if (isDecliningDataProvided)
            {
                decliningData.FeedRigidbody(_rigidbody);
            }
            else if (decliningReason == DecliningReason.SERVER_FORCE_ON_CLIENT_DISCONNECT)
            {
                _rigidbody.linearVelocity = Vector2.zero;
            }
        }
    }
}