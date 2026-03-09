namespace Caskev.NetcodeForGameObjects.DistributedAuthority
{
    /// <summary>
    /// An enumeration of the different policies on the ownership locking beehaviour when an ownership is accepted
    /// - CUSTOM : Will use the RequestOwnership method parameter to determine if the ownership will be locked or not
    /// - FORCE_LOCK : Will lock the ownership regardless of the RequestOwnership method parameter
    /// - FORCE_UNLOCK : Will not lock the ownership regardless of the RequestOwnership method parameter
    /// </summary>
    public enum OwnershipLockingOnGainPolicy
        {
            CUSTOM,
            FORCE_LOCK,
            FORCE_UNLOCK,
        }
}
