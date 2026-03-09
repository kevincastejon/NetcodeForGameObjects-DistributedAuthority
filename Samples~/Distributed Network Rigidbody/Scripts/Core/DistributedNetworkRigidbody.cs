using Caskev.NetcodeForGameObjects.DistributedAuthority;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.Events;
namespace Caskev.Samples.NetcodeForGameObjects.DistributedAuthority.DistributedNetworkRigidbody_3D
{
    [GenerateSerializationForType(typeof(CompleteRigidbodyPose))]
    public class DistributedNetworkRigidbody : DistributedNetworkObject<CompleteRigidbodyPose>
    {
        [SerializeField] private bool _automaticallyDeclineOnAsleep;
        [SerializeField] private UnityEvent _onAsleep;
        [SerializeField] private UnityEvent _onAwake;
        private NetworkTransform _netTransform;
        private Rigidbody _rigidbody;
        private bool _originalKinematicState;
        private bool _lastSleepState;

        public Rigidbody Rigidbody { get => _rigidbody; }
        public bool AutomaticallyDeclineOnAsleep { get => _automaticallyDeclineOnAsleep; set => _automaticallyDeclineOnAsleep = value; }
        public bool OriginalKinematicState { get => _originalKinematicState; }

        private void Awake()
        {
            _netTransform = GetComponent<NetworkTransform>();
            _rigidbody = GetComponent<Rigidbody>();
            _originalKinematicState = _rigidbody.isKinematic;
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
                        DeclineOwnership(CompleteRigidbodyPose.FromRigidbody(_rigidbody));
                    }
                    _onAsleep.Invoke();
                }
                else
                {
                    _onAwake.Invoke();
                }
            }
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
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            _netTransform.enabled = true;
        }
        public override void OnEnterAuthority()
        {
            _netTransform.enabled = true;
            _rigidbody.isKinematic = _originalKinematicState;
        }
        public override void OnEnterRemote()
        {
            _netTransform.enabled = true;
            _rigidbody.isKinematic = true;
        }
        public override void OnEnterRequesting()
        {
            _netTransform.enabled = IsOwner;
        }
        public override void OnEnterDeclining()
        {
            _netTransform.enabled = IsOwner;
        }

        public override void OnClientDecliningServerSide(DecliningReason decliningReason, bool isDecliningDataProvided, CompleteRigidbodyPose decliningData)
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