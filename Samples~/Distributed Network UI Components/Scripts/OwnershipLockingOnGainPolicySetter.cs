using UnityEngine;
using Caskev.NetcodeForGameObjects.DistributedAuthority;
namespace Caskev.Samples.NetcodeForGameObjects.DistributedAuthority.DistributedNetworkUI_2D
{
    public class OwnershipLockingOnGainPolicySetter : MonoBehaviour
    {
        [SerializeField] private DistributedNetworkUIComponent[] _networkUIComponents;
        public void SetOwnershipLockingOnGainPolicy(bool value)
        {
            foreach (var nuic in _networkUIComponents)
            {
                nuic.OwnershipLockingOnGain = value ? DistributedNetworkUIComponent.OwnershipLockingOnGainPolicy.FORCE_LOCK : DistributedNetworkUIComponent.OwnershipLockingOnGainPolicy.FORCE_UNLOCK;
            }
        }
    }
}
