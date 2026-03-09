namespace Caskev.NetcodeForGameObjects.DistributedAuthority
{
    /// <summary>
    /// An enumeration of the possible ownership decline reason
    /// - REMOTE_REQUEST : The client declined ownership itself. It may have provided some data that can be apply server-side.
    /// - SERVER_FORCE : The client has been forced to decline ownership by the server. The server may have conveniently provided some data that can be apply on the regular flow.
    /// - SERVER_FORCE_ON_CLIENT_DISCONNECT : The disconnected client has been forced to decline ownership by the server. No data can be provided, since the client disconnected and the server did not call for it.
    /// </summary>
    public enum DecliningReason
        {
            REMOTE_REQUEST,
            SERVER_FORCE,
            SERVER_FORCE_ON_CLIENT_DISCONNECT,
        }
}
