namespace Caskev.NetcodeForGameObjects.DistributedAuthority
{
    /// <summary>
    /// An enumeration of the different distributedc authority states
    /// - NONE : The object is not spawned.
    /// - AUTHORITY : The local player (or server) has authority over the object.
    /// - REMOTE : The local player (or server) has not authority over the object.
    /// - REQUESTING : The local player is requesting authority (non-host clients only).
    /// - DECLINING : The local player is declining authority (non-host clients only).
    /// </summary>
    public enum DistributedAuthorityState
        {
            NONE,
            AUTHORITY,
            REMOTE,
            REQUESTING,
            DECLINING
        }
}
