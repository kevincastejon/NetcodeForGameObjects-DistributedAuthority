using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
namespace Caskev.NetcodeForGameObjects.DistributedAuthority
{
    [GenerateSerializationForType(typeof(CompleteTransformPose))]
    public class DistributedNetworkTransform : DistributedNetworkObject<CompleteTransformPose>
    {
        private NetworkTransform _netTransform;

        private void Awake()
        {
            _netTransform = GetComponent<NetworkTransform>();
        }
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            _netTransform.enabled = true;
        }

        public override void OnClientDecliningServerSide(DecliningReason decliningReason, bool isDecliningDataProvided, CompleteTransformPose decliningData)
        {
            if (isDecliningDataProvided)
            {
                decliningData.FeedTransform(transform);
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
        public override void OnEnterAuthority()
        {
            _netTransform.enabled = true;
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
    }
}
