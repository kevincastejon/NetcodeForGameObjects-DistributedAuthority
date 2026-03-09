using UnityEngine;
using Caskev.NetcodeForGameObjects.DistributedAuthority;
namespace Caskev.Samples.NetcodeForGameObjects.DistributedAuthority.DistributedNetworkUIComponents
{
    public class OwnershipLockingOnGainPolicySetter : MonoBehaviour
    {
        [SerializeField] private DistributedNetworkUIComponent[] _networkUIComponents;
        public void SetOwnershipLockingOnGainPolicy(bool value)
        {
            foreach (var nuic in _networkUIComponents)
            {
                nuic.OwnershipLockingOnGain = value ? OwnershipLockingOnGainPolicy.FORCE_LOCK : OwnershipLockingOnGainPolicy.FORCE_UNLOCK;
            }
        }
    }
}
