using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
namespace Caskev.NetcodeForGameObjects.DistributedAuthority
{
    public class DistributedAuthorityBehaviour<T> : NetworkBehaviour
    {
        /// <summary>
        /// A set of debug parameters
        /// </summary>
        [Serializable]
        public class DebugParametersInternal
        {
            [Tooltip("Will log every state entering and exiting")]
            [SerializeField] private bool _statesVerbose;
            [Tooltip("Will log every ownership gain, loss, locking, and unlocking")]
            [SerializeField] private bool _ownershipVerbose;
            /// <summary>
            /// Will log every state entering and exiting
            /// </summary>
            public bool StatesVerbose { get => _statesVerbose; set => _statesVerbose = value; }
            /// <summary>
            /// Will log every ownership gain, loss, locking, and unlocking
            /// </summary>
            public bool OwnershipVerbose { get => _ownershipVerbose; set => _ownershipVerbose = value; }
        }
        /// <summary>
        /// A set of events related to the distributed authority state changes
        /// </summary>
        [Serializable]
        public class StatesEventsInternal
        {
            [Tooltip("Fires when the state has changed")]
            [SerializeField] private UnityEvent<DistributedAuthorityState> _onStateChanged;
            [Tooltip("Fires when entered AUTHORITY state")]
            [SerializeField] private UnityEvent _onEnterAuthority;
            [Tooltip("Fires when exited AUTHORITY state")]
            [SerializeField] private UnityEvent _onExitAuthority;
            [Tooltip("Fires when entered REMOTE state")]
            [SerializeField] private UnityEvent _onEnterRemote;
            [Tooltip("Fires when exited REMOTE state")]
            [SerializeField] private UnityEvent _onExitRemote;
            [Tooltip("Fires when entered REQUESTING state")]
            [SerializeField] private UnityEvent _onEnterRequesting;
            [Tooltip("Fires when exited REQUESTING state")]
            [SerializeField] private UnityEvent _onExitRequesting;
            [Tooltip("Fires when entered DECLINING state")]
            [SerializeField] private UnityEvent _onEnterDeclining;
            [Tooltip("Fires when exited DECLINING state")]
            [SerializeField] private UnityEvent _onExitDeclining;
            /// <summary>
            /// Fires when the state has changed
            /// </summary>
            public UnityEvent<DistributedAuthorityState> OnStateChanged { get => _onStateChanged; }
            /// <summary>
            /// Fires when entered AUTHORITY state
            /// </summary>
            public UnityEvent OnEnteredAuthority { get => _onEnterAuthority; }
            /// <summary>
            /// Fires when exited AUTHORITY state
            /// </summary>
            public UnityEvent OnExitedAuthority { get => _onExitAuthority; }
            /// <summary>
            /// Fires when entered REMOTE state
            /// </summary>
            public UnityEvent OnEnteredRemote { get => _onEnterRemote; }
            /// <summary>
            /// Fires when exited REMOTE state
            /// </summary>
            public UnityEvent OnExitedRemote { get => _onExitRemote; }
            /// <summary>
            /// Fires when entered REQUESTING state
            /// </summary>
            public UnityEvent OnEnteredRequesting { get => _onEnterRequesting; }
            /// <summary>
            /// Fires when exited REQUESTING state
            /// </summary>
            public UnityEvent OnExitedRequesting { get => _onExitRequesting; }
            /// <summary>
            /// Fires when entered DECLINING state
            /// </summary>
            public UnityEvent OnEnteredDeclining { get => _onEnterDeclining; }
            /// <summary>
            /// Fires when exited DECLINING state
            /// </summary>
            public UnityEvent OnExitedDeclining { get => _onExitDeclining; }
        }
        [Serializable]
        /// <summary>
        /// Three groups of events set related to the ownership changes, organized into a local/remote and client/server oriented way.
        /// </summary>
        public class OwnershipEventsInternal
        {
            /// <summary>
            /// Three sets of events related to ownership changes, organized into a client/server oriented way.
            /// </summary>
            [Serializable]
            public class OwnershipEventsGroup
            {
                /// <summary>
                /// A set of events related to ownership changes.
                /// </summary>
                [Serializable]
                public class OwnershipEventsSet
                {
                    [Tooltip("Fires when the ownership has been gained by player")]
                    [SerializeField] private UnityEvent<ulong> _onOwnershipGained;
                    [Tooltip("Fires when the ownership has been lost by player")]
                    [SerializeField] private UnityEvent<ulong> _onOwnershipLost;
                    [Tooltip("Fires when ownership is locked on player")]
                    [SerializeField] private UnityEvent<ulong> _onOwnershipLocked;
                    [Tooltip("Fires when ownership is unlocked on player")]
                    [SerializeField] private UnityEvent<ulong> _onOwnershipUnlocked;
                    [Tooltip("Fires when ownership is robbed from player")]
                    [SerializeField] private UnityEvent<ulong> _onOwnershipRobbedFrom;
                    [Tooltip("Fires when ownership request is rejected for player")]
                    [SerializeField] private UnityEvent<ulong> _onOwnershipRequestRejected;
                    [Tooltip("Fires when ownership request is accepted for player")]
                    [SerializeField] private UnityEvent<ulong> _onOwnershipRequestAccepted;
                    [Tooltip("Fires when ownership is declined from player")]
                    [SerializeField] private UnityEvent<ulong> _onOwnershipDeclined;

                    /// <summary>
                    /// Fires when the ownership has been gained by player
                    /// </summary>
                    public UnityEvent<ulong> OnOwnershipGained { get => _onOwnershipGained; }
                    /// <summary>
                    /// Fires when the ownership has been lost by player
                    /// </summary>
                    public UnityEvent<ulong> OnOwnershipLost { get => _onOwnershipLost; }
                    /// <summary>
                    /// Fires when ownership is locked on player
                    /// </summary>
                    public UnityEvent<ulong> OnOwnershipLocked { get => _onOwnershipLocked; }
                    /// <summary>
                    /// Fires when ownership is unlocked on player
                    /// </summary>
                    public UnityEvent<ulong> OnOwnershipUnlocked { get => _onOwnershipUnlocked; }
                    /// <summary>
                    /// Fires when ownership is robbed from player
                    /// </summary>
                    public UnityEvent<ulong> OnOwnershipRobbedFrom { get => _onOwnershipRobbedFrom; }
                    /// <summary>
                    /// Fires when ownership request is rejected for player
                    /// </summary>
                    public UnityEvent<ulong> OnOwnershipRequestRejected { get => _onOwnershipRequestRejected; }
                    /// <summary>
                    /// Fires when ownership request is accepted for player
                    /// </summary>
                    public UnityEvent<ulong> OnOwnershipRequestAccepted { get => _onOwnershipRequestAccepted; }
                    /// <summary>
                    /// Fires when ownership is declined from player
                    /// </summary>
                    public UnityEvent<ulong> OnOwnershipDeclined { get => _onOwnershipDeclined; }
                }
                [Tooltip("A set of events related to ownership changes. These events will fire on both client and server sides")]
                [SerializeField] private OwnershipEventsSet _crossSideEvents;
                [Tooltip("A set of events related to ownership changes. These events will fire only client-side")]
                [SerializeField] private OwnershipEventsSet _clientSideEvents;
                [Tooltip("A set of events related to ownership changes. These events will fire only server-side")]
                [SerializeField] private OwnershipEventsSet _serverSideEvents;
                /// <summary>
                /// A set of events related to ownership changes. These events will fire on both client and server sides
                /// </summary>
                public OwnershipEventsSet CrossSideEvents { get => _crossSideEvents; }
                /// <summary>
                /// A set of events related to ownership changes. These events will fire only client-side
                /// </summary>
                public OwnershipEventsSet ClientSideEvents { get => _clientSideEvents; }
                /// <summary>
                /// A set of events related to ownership changes. These events will fire only server-side
                /// </summary>
                public OwnershipEventsSet ServerSideEvents { get => _serverSideEvents; }
            }
            [Tooltip("Three sets of events related to ownership changes. These events will fire when any client or server is concerned")]
            [SerializeField] private OwnershipEventsGroup _events;
            [Tooltip("Three sets of events related to ownership changes. These events will fire only when the local client or server is concerned")]
            [SerializeField] private OwnershipEventsGroup _localEvents;
            [Tooltip("Three sets of events related to ownership changes. These events will fire only when a remote client or server is concerned")]
            [SerializeField] private OwnershipEventsGroup _remoteEvents;
            /// <summary>
            /// A set of events related to ownership changes. These events will fire when any client or server is concerned
            /// </summary>
            public OwnershipEventsGroup Events { get => _events; }
            /// <summary>
            /// A set of events related to ownership changes. These events will fire only when the local client or server is concerned
            /// </summary>
            public OwnershipEventsGroup LocalEvents { get => _localEvents; }
            /// <summary>
            /// A set of events related to ownership changes. These events will fire only when a remote client or server is concerned
            /// </summary>
            public OwnershipEventsGroup RemoteEvents { get => _remoteEvents; }
        }
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
        /// <summary>
        /// An enumeration of the different distributedc authority states
        /// - AUTHORITY : The local player has authority over the object
        /// - REMOTE : A remote player has authority over the object
        /// - REQUESTING : The local player is requesting authority (non-host clients only)
        /// - DECLINING : The local player is declining authority (non-host clients only)
        /// </summary>
        public enum DistributedAuthorityState
        {
            AUTHORITY,
            REMOTE,
            REQUESTING,
            DECLINING
        }
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

        private enum SignalsType
        {
            OWNERSHIP_REQUEST,
            OWNERSHIP_REQUEST_CONFIRM,
            OWNERSHIP_REQUEST_REJECT,
            OWNERSHIP_DECLINE,
            OWNERSHIP_DECLINE_CONFIRM,
            CLIENT_DECLINE_CONFIRM,
            BEEN_ROBBED,
        }

        [Tooltip("The current distributed authority state.")][SerializeField] private DistributedAuthorityState _currentState;
        [Tooltip("Determine the ownership locking behaviour when an ownership request is accepted\r\n        - CUSTOM : Will use the RequestOwnership method parameter to determine if the ownership will be locked or not\r\n        - FORCE_LOCK : Will lock the ownership regardless of the RequestOwnership method parameter\r\n        - FORCE_UNLOCK : Will not lock the ownership regardless of the RequestOwnership method parameter")]
        [SerializeField] private OwnershipLockingOnGainPolicy _ownershipLockingOnGainPolicy;
        [Tooltip("A set of events related to the distributed authority state changes.")]
        [SerializeField] private StatesEventsInternal _statesEvents;
        [Tooltip("A set of events related to the ownership changes.")]
        [SerializeField] private OwnershipEventsInternal _ownershipEvents;
        [Tooltip("A set of debug parameters.")]
        [SerializeField] private DebugParametersInternal _debugParameters;


        // client-side only
        private uint _localOwnershipSessionId = 0;
        private bool _useDecliningData;
        private bool _useLockOwnership;
        private T _decliningData;
        // server-side only
        private uint _remoteOwnershipSessionId = 0;
        // both-side
        private bool _isOwnershipLocked;
        /// <summary>
        /// The current state of the distributed authority
        /// </summary>
        public DistributedAuthorityState CurrentState
        {
            get => _currentState;
            private set
            {
                if (_currentState == value)
                {
                    return;
                }
                DistributedAuthorityState oldValue = _currentState;
                _currentState = value;
                switch (oldValue)
                {
                    case DistributedAuthorityState.AUTHORITY:
                        OnExitAuthorityInternal();
                        break;
                    case DistributedAuthorityState.REMOTE:
                        OnExitRemoteInternal();
                        break;
                    case DistributedAuthorityState.REQUESTING:
                        OnExitRequestingInternal();
                        break;
                    case DistributedAuthorityState.DECLINING:
                        OnExitDecliningInternal();
                        break;
                    default:
                        break;
                }
                switch (_currentState)
                {
                    case DistributedAuthorityState.AUTHORITY:
                        OnEnterAuthorityInternal();
                        break;
                    case DistributedAuthorityState.REMOTE:
                        OnEnterRemoteInternal();
                        break;
                    case DistributedAuthorityState.REQUESTING:
                        OnEnterRequestingInternal();
                        break;
                    case DistributedAuthorityState.DECLINING:
                        OnEnterDecliningInternal();
                        break;
                    default:
                        break;
                }
                _statesEvents.OnStateChanged.Invoke(_currentState);
            }
        }
        /// <summary>
        /// Determine the ownership locking behaviour when an ownership request is accepted
        /// - CUSTOM : Will use the RequestOwnership method parameter to determine if the ownership will be locked or not
        /// - FORCE_LOCK : Will lock the ownership regardless of the RequestOwnership method parameter
        /// - FORCE_UNLOCK : Will not lock the ownership regardless of the RequestOwnership method parameter
        /// </summary>
        public OwnershipLockingOnGainPolicy OwnershipLockingOnGain { get => _ownershipLockingOnGainPolicy; set => _ownershipLockingOnGainPolicy = value; }
        /// <summary>
        /// A set of events related to the distributed authority state changes
        /// </summary>
        public StatesEventsInternal StatesEvents { get => _statesEvents; }
        /// <summary>
        /// A set of events related to the ownership changes
        /// </summary>
        public OwnershipEventsInternal OwnershipEvents { get => _ownershipEvents; }
        /// <summary>
        /// A set of debug parameters
        /// </summary>
        public DebugParametersInternal DebugParameters { get => _debugParameters; }
        /// <summary>
        /// Is the ownership of the object locked on the current owner
        /// </summary>
        public bool IsOwnershipLocked { get => _isOwnershipLocked; }

        public virtual void Start()
        {
            OnEnterAuthorityInternal();
        }
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            CurrentState = IsServer ? DistributedAuthorityState.AUTHORITY : DistributedAuthorityState.REMOTE;
            if (IsServer)
            {
                NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectedServerSide;
            }
        }

        private void OnClientDisconnectedServerSide(ulong obj)
        {
            DeclineOwnershipServerSide(obj, _remoteOwnershipSessionId, DecliningReason.SERVER_FORCE_ON_CLIENT_DISCONNECT, default, false);
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            _localOwnershipSessionId = 0;
            _remoteOwnershipSessionId = 0;
            _decliningData = default;
            _useDecliningData = false;
            _useLockOwnership = false;
            if (_isOwnershipLocked)
            {
                _isOwnershipLocked = false;
                if (_debugParameters.OwnershipVerbose) Debug.Log("OWNERSHIP UNLOCKED ON DESPAWN");
                OnOwnershipUnlocked(0);
                _ownershipEvents.Events.CrossSideEvents.OnOwnershipUnlocked.Invoke(0);
            }
            CurrentState = DistributedAuthorityState.AUTHORITY;
        }
        protected override void OnOwnershipChanged(ulong previous, ulong current)
        {
            base.OnOwnershipChanged(previous, current);
            if (previous == current)
            {
                return;
            }
            OnOwnershipLostInternal(previous);
            OnOwnershipGainedInternal(current);
        }

        #region OwnershipRequest
        /// <summary>
        /// Sends a request for getting the ownership of the object.
        /// </summary>
        /// <param name="lockOwnership">When OwnershipLockingOnGain is set to CUSTOM, then this parameter will determine wether to lock the object or not if the request is accepted.</param>
        public virtual void RequestOwnership(bool lockOwnership = false)
        {
            if (!IsSpawned)
            {
                return;
            }
            if (IsServer)
            {
                RequestOwnershipRpc(_localOwnershipSessionId, lockOwnership);
            }
            else
            {
                _useLockOwnership = lockOwnership;
                OnSignal(SignalsType.OWNERSHIP_REQUEST);
            }
        }
        [Rpc(SendTo.Server)]
        private void RequestOwnershipRpc(uint requestId, bool lockOwnership = false, RpcParams rpcParams = default)
        {
            ulong requester = rpcParams.Receive.SenderClientId;
            if (OwnerClientId != requester && _isOwnershipLocked)
            {
                OnOwnershipRequestRejectRpc(requester, requestId);
                return;
            }
            lockOwnership = _ownershipLockingOnGainPolicy == OwnershipLockingOnGainPolicy.CUSTOM ? lockOwnership : _ownershipLockingOnGainPolicy == OwnershipLockingOnGainPolicy.FORCE_LOCK;
            uint formerRequestId = _remoteOwnershipSessionId;
            ulong formerOwner = OwnerClientId;
            _remoteOwnershipSessionId = requestId;
            if (requester != formerOwner)
            {
                NetworkObject.ChangeOwnership(requester);
            }
            OnOwnershipRequestConfirmRpc(formerOwner, formerRequestId, requester, requestId, lockOwnership);
        }

        [Rpc(SendTo.Everyone)]
        private void OnOwnershipRequestConfirmRpc(ulong formerOwnerId, uint formerOwnerRequestId, ulong newOwnerId, uint newOwnerRequestId, bool lockOwnership, RpcParams rpcParams = default)
        {
            bool oldLockValue = _isOwnershipLocked;
            _isOwnershipLocked = lockOwnership;
            bool isRequestUpdate = formerOwnerId == newOwnerId;
            if (!isRequestUpdate && formerOwnerId == NetworkManager.LocalClientId && formerOwnerRequestId == _localOwnershipSessionId)
            {
                OnSignal(SignalsType.BEEN_ROBBED);
            }
            if (newOwnerId == NetworkManager.LocalClientId && newOwnerRequestId == _localOwnershipSessionId)
            {
                OnSignal(SignalsType.OWNERSHIP_REQUEST_CONFIRM);
            }
            OnOwnershipRequestAcceptedInternal(newOwnerId);
            if (!isRequestUpdate)
            {
                OnOwnershipRobbedFromInternal(formerOwnerId);
            }
            if (_isOwnershipLocked != oldLockValue)
            {
                if (_isOwnershipLocked)
                {
                    OnOwnershipLockedInternal(newOwnerId);
                }
                else
                {
                    OnOwnershipUnlockedInternal(newOwnerId);
                }
            }
        }
        [Rpc(SendTo.Everyone)]
        private void OnOwnershipRequestRejectRpc(ulong requester, uint rejectedRequestId, RpcParams rpcParams = default)
        {
            if (requester == NetworkManager.LocalClientId && rejectedRequestId == _localOwnershipSessionId)
            {
                OnSignal(SignalsType.OWNERSHIP_REQUEST_REJECT);
            }
            OnOwnershipRequestRejectedInternal(requester);
        }
        #endregion

        #region OwnershipDecline
        /// <summary>
        /// Sends a request for declining the ownership of the object.
        /// Calling this method will always unlock the ownership of the object if it was locked.
        /// Thus a server cannot really "decline" ownership, if called from the server and the ownership was locked, then it will still unlock it and relay to clients so it is still usefull as a host.
        /// </summary>
        public virtual void DeclineOwnership()
        {
            DeclineOwnership(default, false);
        }
        /// <summary>
        /// Sends a request for declining the ownership of the object along with the last object state to be applied server-side if the decline request is treated.
        /// Calling this method will always unlock the ownership of the object if it was locked.
        /// Thus a server cannot really "decline" ownership, if called from the server and the ownership was locked, then it will only unlock it and relay to clients so it is still usefull as a host.
        /// </summary>
        /// <param name="decliningData">State data to be applied server-side if the decline request is treated.</param>
        public virtual void DeclineOwnership(T decliningData)
        {
            DeclineOwnership(decliningData, true);
        }
        private void DeclineOwnership(T decliningData, bool useDecliningData = false)
        {
            if (!IsSpawned || (!IsServer && _localOwnershipSessionId == 0))
            {
                return;
            }
            if (IsServer)
            {
                DeclineOwnershipRpc(_localOwnershipSessionId);
            }
            else
            {
                _useDecliningData = useDecliningData;
                if (_useDecliningData)
                {
                    _decliningData = decliningData;
                }
                OnSignal(SignalsType.OWNERSHIP_DECLINE);
            }
        }
        public void ForceDeclineOwnershipServerSide()
        {
            if (!IsServer)
            {
                Debug.LogError("This method can only be called server-side");
                return;
            }
            DeclineOwnershipServerSide(OwnerClientId, _remoteOwnershipSessionId, DecliningReason.SERVER_FORCE, default, false);
        }
        public void ForceDeclineOwnershipServerSide(T decliningData)
        {
            if (!IsServer)
            {
                Debug.LogError("This method can only be called server-side");
                return;
            }
            DeclineOwnershipServerSide(OwnerClientId, _remoteOwnershipSessionId, DecliningReason.SERVER_FORCE, decliningData, true);
        }

        [Rpc(SendTo.Server)]
        private void DeclineOwnershipRpc(uint requestId, RpcParams rpcParams = default)
        {
            DeclineOwnershipServerSide(rpcParams.Receive.SenderClientId, requestId, DecliningReason.REMOTE_REQUEST, default, false);
        }
        [Rpc(SendTo.Server)]
        private void DeclineOwnershipRpc(uint requestId, T decliningData, RpcParams rpcParams = default)
        {
            DeclineOwnershipServerSide(rpcParams.Receive.SenderClientId, requestId, DecliningReason.REMOTE_REQUEST, decliningData, true);
        }
        private void DeclineOwnershipServerSide(ulong requester, uint requestId, DecliningReason decliningReason, T decliningData, bool useDecliningData)
        {
            if (OwnerClientId == requester && requestId == _remoteOwnershipSessionId)
            {
                if (requester == NetworkManager.ServerClientId)
                {
                    if (_isOwnershipLocked)
                    {
                        _isOwnershipLocked = false;
                        OnOwnershipUnlockedRpc();
                        return;
                    }
                }
                else
                {
                    NetworkObject.RemoveOwnership();
                    _remoteOwnershipSessionId = 0;
                    OnOwnershipDeclineConfirmRpc(requester, requestId);
                    OnClientDecliningServerSide(decliningReason, useDecliningData, decliningData);
                }
            }
        }
        [Rpc(SendTo.Everyone)]
        private void OnOwnershipDeclineConfirmRpc(ulong requester, uint requestId)
        {
            if (IsServer)
            {
                OnSignal(SignalsType.CLIENT_DECLINE_CONFIRM);
            }
            if (requester == NetworkManager.LocalClientId && requestId == _localOwnershipSessionId)
            {
                OnSignal(SignalsType.OWNERSHIP_DECLINE_CONFIRM);
            }
            OnOwnershipDeclinedInternal(requester);
            if (_isOwnershipLocked)
            {
                _isOwnershipLocked = false;
                OnOwnershipUnlockedInternal(OwnerClientId);
            }
        }
        #endregion

        #region OwnershipLock
        /// <summary>
        /// Locks ownership on the object so peers requests will be rejected. Can only be called by owner and server.
        /// </summary>
        public virtual void LockOwnership()
        {
            LockOwnershipRpc(_localOwnershipSessionId);
        }
        [Rpc(SendTo.Server)]
        private void LockOwnershipRpc(uint requestId, RpcParams rpcParams = default)
        {
            if (rpcParams.Receive.SenderClientId == NetworkManager.ServerClientId || (requestId == _remoteOwnershipSessionId && OwnerClientId == rpcParams.Receive.SenderClientId && !_isOwnershipLocked))
            {
                OnOwnershipLockedRpc();
            }
        }
        [Rpc(SendTo.Everyone)]
        private void OnOwnershipLockedRpc(RpcParams rpcParams = default)
        {
            _isOwnershipLocked = true;
            OnOwnershipLockedInternal(OwnerClientId);
        }
        /// <summary>
        /// Unlocks ownership on the object so peers requests will be accepted. Can only be called by owner and server.
        /// </summary>
        public virtual void UnlockOwnership()
        {
            UnlockOwnershipRpc(_localOwnershipSessionId);
        }
        [Rpc(SendTo.Server)]
        private void UnlockOwnershipRpc(uint requestId, RpcParams rpcParams = default)
        {
            if (rpcParams.Receive.SenderClientId == NetworkManager.ServerClientId || (requestId == _remoteOwnershipSessionId && OwnerClientId == rpcParams.Receive.SenderClientId && _isOwnershipLocked))
            {
                OnOwnershipUnlockedRpc();
            }
        }
        [Rpc(SendTo.Everyone)]
        private void OnOwnershipUnlockedRpc(RpcParams rpcParams = default)
        {
            _isOwnershipLocked = false;
            OnOwnershipUnlockedInternal(OwnerClientId);
        }
        #endregion

        #region StateMachine
        private void OnSignal(SignalsType signal)
        {
            switch (_currentState)
            {
                case DistributedAuthorityState.AUTHORITY:
                    OnAuthoritySignal(signal);
                    break;
                case DistributedAuthorityState.REMOTE:
                    OnRemoteSignal(signal);
                    break;
                case DistributedAuthorityState.REQUESTING:
                    OnRequestingSignal(signal);
                    break;
                case DistributedAuthorityState.DECLINING:
                    OnDecliningSignal(signal);
                    break;
                default:
                    break;
            }
        }
        private void OnAuthoritySignal(SignalsType signal)
        {
            if (IsServer)
            {
                if (signal == SignalsType.BEEN_ROBBED)
                {
                    CurrentState = DistributedAuthorityState.REMOTE;
                }
            }
            else
            {
                if (signal == SignalsType.OWNERSHIP_DECLINE)
                {
                    CurrentState = DistributedAuthorityState.DECLINING;
                }
                else if (signal == SignalsType.BEEN_ROBBED || signal == SignalsType.OWNERSHIP_DECLINE_CONFIRM)
                {
                    CurrentState = DistributedAuthorityState.REMOTE;
                }
                else if (signal == SignalsType.OWNERSHIP_REQUEST)
                {
                    CurrentState = DistributedAuthorityState.REQUESTING;
                }
            }
        }
        private void OnRemoteSignal(SignalsType signal)
        {
            if (IsServer)
            {
                if (signal == SignalsType.OWNERSHIP_REQUEST_CONFIRM || signal == SignalsType.CLIENT_DECLINE_CONFIRM)
                {
                    CurrentState = DistributedAuthorityState.AUTHORITY;
                }
            }
            else
            {
                if (signal == SignalsType.OWNERSHIP_REQUEST)
                {
                    CurrentState = DistributedAuthorityState.REQUESTING;
                }
            }
        }
        private void OnRequestingSignal(SignalsType signal)
        {
            if (signal == SignalsType.OWNERSHIP_REQUEST_REJECT)
            {
                CurrentState = DistributedAuthorityState.REMOTE;
            }
            else if (signal == SignalsType.OWNERSHIP_REQUEST_CONFIRM)
            {
                CurrentState = DistributedAuthorityState.AUTHORITY;
            }
            else if (signal == SignalsType.OWNERSHIP_DECLINE)
            {
                CurrentState = DistributedAuthorityState.DECLINING;
            }
            else if (signal == SignalsType.OWNERSHIP_REQUEST)
            {
                CurrentState = DistributedAuthorityState.REQUESTING;
            }
        }
        private void OnDecliningSignal(SignalsType signal)
        {
            if (signal == SignalsType.OWNERSHIP_REQUEST_REJECT || signal == SignalsType.OWNERSHIP_DECLINE_CONFIRM || signal == SignalsType.BEEN_ROBBED)
            {
                CurrentState = DistributedAuthorityState.REMOTE;
            }
            else if (signal == SignalsType.OWNERSHIP_REQUEST)
            {
                CurrentState = DistributedAuthorityState.REQUESTING;
            }
        }

        #endregion
        #region Statemachine events and callbacks
        private void OnEnterAuthorityInternal()
        {
            if (_debugParameters.StatesVerbose) Debug.Log("ENTER AUTHORITY");
            _remoteOwnershipSessionId = 0;
            OnEnterAuthority();
            _statesEvents.OnEnteredAuthority.Invoke();
        }
        private void OnExitAuthorityInternal()
        {
            if (_debugParameters.StatesVerbose) Debug.Log("EXIT AUTHORITY");
            OnExitAuthority();
            _statesEvents.OnExitedAuthority.Invoke();
        }
        private void OnEnterRemoteInternal()
        {
            if (_debugParameters.StatesVerbose) Debug.Log("ENTER REMOTE");
            OnEnterRemote();
            _statesEvents.OnEnteredRemote.Invoke();
        }
        private void OnExitRemoteInternal()
        {
            if (_debugParameters.StatesVerbose) Debug.Log("EXIT REMOTE");
            OnExitRemote();
            _statesEvents.OnExitedRemote.Invoke();
        }
        private void OnEnterRequestingInternal()
        {
            if (_debugParameters.StatesVerbose) Debug.Log("ENTER REQUESTING");
            _localOwnershipSessionId++;
            RequestOwnershipRpc(_localOwnershipSessionId, _useLockOwnership);
            OnEnterRequesting();
            _statesEvents.OnEnteredRequesting.Invoke();
        }
        private void OnExitRequestingInternal()
        {
            if (_debugParameters.StatesVerbose) Debug.Log("EXIT REQUESTING");
            OnExitRequesting();
            _statesEvents.OnExitedRequesting.Invoke();
        }
        private void OnEnterDecliningInternal()
        {
            if (_debugParameters.StatesVerbose) Debug.Log("ENTER DECLINING");
            if (_useDecliningData)
            {
                DeclineOwnershipRpc(_localOwnershipSessionId, _decliningData);
            }
            else
            {
                DeclineOwnershipRpc(_localOwnershipSessionId);
            }
            OnEnterDeclining();
            _statesEvents.OnEnteredDeclining.Invoke();
        }
        private void OnExitDecliningInternal()
        {
            if (_debugParameters.StatesVerbose) Debug.Log("EXIT DECLINING");
            OnExitDeclining();
            _statesEvents.OnExitedDeclining.Invoke();
        }

        /// <summary>
        /// Called when entered the AUTHORITY state.
        /// </summary>
        public virtual void OnEnterAuthority() { }
        /// <summary>
        /// Called when exited the AUTHORITY state.
        /// </summary>
        public virtual void OnExitAuthority() { }
        /// <summary>
        /// Called when entered the REMOTE state.
        /// </summary>
        public virtual void OnEnterRemote() { }
        /// <summary>
        /// Called when exited the REMOTE state.
        /// </summary>
        public virtual void OnExitRemote() { }
        /// <summary>
        /// Called when entered the REQUESTING state.
        /// </summary>
        public virtual void OnEnterRequesting() { }
        /// <summary>
        /// Called when exited the REQUESTING state.
        /// </summary>
        public virtual void OnExitRequesting() { }
        /// <summary>
        /// Called when entered the DECLINING state.
        /// </summary>
        public virtual void OnEnterDeclining() { }
        /// <summary>
        /// Called when exited the DECLINING state.
        /// </summary>
        public virtual void OnExitDeclining() { }
        /// <summary>
        /// Called server-side only, when getting passive authority on the object after a client (not host) has declined ownership.
        /// This method is usefull, when a client declined authority itself, to handle short ownership duration, where data might not have had the time to actually fully reach the server, so the last state can still be applied.
        /// Also useful for conveniently providing data, on server-side force ownership decline, to be apply on the regular flow.
        /// </summary>
        /// <param name="decliningReason">The reason of the ownership decline. REMOTE_REQUEST, SERVER_FORCE and SERVER_FORCE_ON_CLIENT_DISCONNECT</param>
        /// <param name="isDecliningDataProvided">A boolean to check wether or not some data has been provided</param>
        /// <param name="decliningData">Some data that may have been provided so it can be applied on this server-side callback</param>
        public virtual void OnClientDecliningServerSide(DecliningReason decliningReason, bool isDecliningDataProvided, T decliningData) { }
        #endregion
        #region Ownership events and callbacks
        private void OnOwnershipGainedInternal(ulong clientId)
        {
            if (_debugParameters.OwnershipVerbose) Debug.Log("OWNERSHIP GAINED ON PLAYER " + clientId);
            OnOwnershipGained(clientId);
            _ownershipEvents.Events.CrossSideEvents.OnOwnershipGained.Invoke(clientId);
            if (IsServer)
            {
                _ownershipEvents.Events.ServerSideEvents.OnOwnershipGained.Invoke(clientId);
            }
            else
            {
                _ownershipEvents.Events.ClientSideEvents.OnOwnershipGained.Invoke(clientId);
            }
            if (clientId == NetworkManager.LocalClientId)
            {
                _ownershipEvents.LocalEvents.CrossSideEvents.OnOwnershipGained.Invoke(clientId);
                if (IsServer)
                {
                    _ownershipEvents.LocalEvents.ServerSideEvents.OnOwnershipGained.Invoke(clientId);
                }
                else
                {
                    _ownershipEvents.LocalEvents.ClientSideEvents.OnOwnershipGained.Invoke(clientId);
                }
            }
            else
            {
                _ownershipEvents.RemoteEvents.CrossSideEvents.OnOwnershipGained.Invoke(clientId);
                if (IsServer)
                {
                    _ownershipEvents.RemoteEvents.ServerSideEvents.OnOwnershipGained.Invoke(clientId);
                }
                else
                {
                    _ownershipEvents.RemoteEvents.ClientSideEvents.OnOwnershipGained.Invoke(clientId);
                }
            }
        }
        private void OnOwnershipLostInternal(ulong clientId)
        {
            if (_debugParameters.OwnershipVerbose) Debug.Log("OWNERSHIP LOST ON PLAYER " + clientId);
            OnOwnershipLost(clientId);
            _ownershipEvents.Events.CrossSideEvents.OnOwnershipLost.Invoke(clientId);
            if (IsServer)
            {
                _ownershipEvents.Events.ServerSideEvents.OnOwnershipLost.Invoke(clientId);
            }
            else
            {
                _ownershipEvents.Events.ClientSideEvents.OnOwnershipLost.Invoke(clientId);
            }
            if (clientId == NetworkManager.LocalClientId)
            {
                _ownershipEvents.LocalEvents.CrossSideEvents.OnOwnershipLost.Invoke(clientId);
                if (IsServer)
                {
                    _ownershipEvents.LocalEvents.ServerSideEvents.OnOwnershipLost.Invoke(clientId);
                }
                else
                {
                    _ownershipEvents.LocalEvents.ClientSideEvents.OnOwnershipLost.Invoke(clientId);
                }
            }
            else
            {
                _ownershipEvents.RemoteEvents.CrossSideEvents.OnOwnershipLost.Invoke(clientId);
                if (IsServer)
                {
                    _ownershipEvents.RemoteEvents.ServerSideEvents.OnOwnershipLost.Invoke(clientId);
                }
                else
                {
                    _ownershipEvents.RemoteEvents.ClientSideEvents.OnOwnershipLost.Invoke(clientId);
                }
            }
        }
        private void OnOwnershipLockedInternal(ulong clientId)
        {
            if (_debugParameters.OwnershipVerbose) Debug.Log("OWNERSHIP LOCKED ON THE PLAYER " + clientId);
            OnOwnershipLocked(clientId);
            _ownershipEvents.Events.CrossSideEvents.OnOwnershipLocked.Invoke(clientId);
            if (IsServer)
            {
                _ownershipEvents.Events.ServerSideEvents.OnOwnershipLocked.Invoke(clientId);
            }
            else
            {
                _ownershipEvents.Events.ClientSideEvents.OnOwnershipLocked.Invoke(clientId);
            }
            if (clientId == NetworkManager.LocalClientId)
            {
                _ownershipEvents.LocalEvents.CrossSideEvents.OnOwnershipLocked.Invoke(clientId);
                if (IsServer)
                {
                    _ownershipEvents.LocalEvents.ServerSideEvents.OnOwnershipLocked.Invoke(clientId);
                }
                else
                {
                    _ownershipEvents.LocalEvents.ClientSideEvents.OnOwnershipLocked.Invoke(clientId);
                }
            }
            else
            {
                _ownershipEvents.RemoteEvents.CrossSideEvents.OnOwnershipLocked.Invoke(clientId);
                if (IsServer)
                {
                    _ownershipEvents.RemoteEvents.ServerSideEvents.OnOwnershipLocked.Invoke(clientId);
                }
                else
                {
                    _ownershipEvents.RemoteEvents.ClientSideEvents.OnOwnershipLocked.Invoke(clientId);
                }
            }
        }
        private void OnOwnershipUnlockedInternal(ulong clientId)
        {
            if (_debugParameters.OwnershipVerbose) Debug.Log("OWNERSHIP UNLOCKED ON THE PLAYER " + clientId);
            OnOwnershipUnlocked(clientId);
            _ownershipEvents.Events.CrossSideEvents.OnOwnershipUnlocked.Invoke(clientId);
            if (IsServer)
            {
                _ownershipEvents.Events.ServerSideEvents.OnOwnershipUnlocked.Invoke(clientId);
            }
            else
            {
                _ownershipEvents.Events.ClientSideEvents.OnOwnershipUnlocked.Invoke(clientId);
            }
            if (clientId == NetworkManager.LocalClientId)
            {
                _ownershipEvents.LocalEvents.CrossSideEvents.OnOwnershipUnlocked.Invoke(clientId);
                if (IsServer)
                {
                    _ownershipEvents.LocalEvents.ServerSideEvents.OnOwnershipUnlocked.Invoke(clientId);
                }
                else
                {
                    _ownershipEvents.LocalEvents.ClientSideEvents.OnOwnershipUnlocked.Invoke(clientId);
                }
            }
            else
            {
                _ownershipEvents.RemoteEvents.CrossSideEvents.OnOwnershipUnlocked.Invoke(clientId);
                if (IsServer)
                {
                    _ownershipEvents.RemoteEvents.ServerSideEvents.OnOwnershipUnlocked.Invoke(clientId);
                }
                else
                {
                    _ownershipEvents.RemoteEvents.ClientSideEvents.OnOwnershipUnlocked.Invoke(clientId);
                }
            }
        }
        private void OnOwnershipRobbedFromInternal(ulong clientId)
        {
            if (_debugParameters.OwnershipVerbose) Debug.Log("OWNERSHIP ROBBED FROM THE PLAYER " + clientId);
            OnOwnershipRobbedFrom(clientId);
            _ownershipEvents.Events.CrossSideEvents.OnOwnershipRobbedFrom.Invoke(clientId);
            if (IsServer)
            {
                _ownershipEvents.Events.ServerSideEvents.OnOwnershipRobbedFrom.Invoke(clientId);
            }
            else
            {
                _ownershipEvents.Events.ClientSideEvents.OnOwnershipRobbedFrom.Invoke(clientId);
            }
            if (clientId == NetworkManager.LocalClientId)
            {
                _ownershipEvents.LocalEvents.CrossSideEvents.OnOwnershipRobbedFrom.Invoke(clientId);
                if (IsServer)
                {
                    _ownershipEvents.LocalEvents.ServerSideEvents.OnOwnershipRobbedFrom.Invoke(clientId);
                }
                else
                {
                    _ownershipEvents.LocalEvents.ClientSideEvents.OnOwnershipRobbedFrom.Invoke(clientId);
                }
            }
            else
            {
                _ownershipEvents.RemoteEvents.CrossSideEvents.OnOwnershipRobbedFrom.Invoke(clientId);
                if (IsServer)
                {
                    _ownershipEvents.RemoteEvents.ServerSideEvents.OnOwnershipRobbedFrom.Invoke(clientId);
                }
                else
                {
                    _ownershipEvents.RemoteEvents.ClientSideEvents.OnOwnershipRobbedFrom.Invoke(clientId);
                }
            }
        }
        private void OnOwnershipRequestRejectedInternal(ulong clientId)
        {
            if (_debugParameters.OwnershipVerbose) Debug.Log("OWNERSHIP REQUEST REJECTED ON THE PLAYER " + clientId);
            OnOwnershipRequestRejected(clientId);
            _ownershipEvents.Events.CrossSideEvents.OnOwnershipRequestRejected.Invoke(clientId);
            if (IsServer)
            {
                _ownershipEvents.Events.ServerSideEvents.OnOwnershipRequestRejected.Invoke(clientId);
            }
            else
            {
                _ownershipEvents.Events.ClientSideEvents.OnOwnershipRequestRejected.Invoke(clientId);
            }
            if (clientId == NetworkManager.LocalClientId)
            {
                _ownershipEvents.LocalEvents.CrossSideEvents.OnOwnershipRequestRejected.Invoke(clientId);
                if (IsServer)
                {
                    _ownershipEvents.LocalEvents.ServerSideEvents.OnOwnershipRequestRejected.Invoke(clientId);
                }
                else
                {
                    _ownershipEvents.LocalEvents.ClientSideEvents.OnOwnershipRequestRejected.Invoke(clientId);
                }
            }
            else
            {
                _ownershipEvents.RemoteEvents.CrossSideEvents.OnOwnershipRequestRejected.Invoke(clientId);
                if (IsServer)
                {
                    _ownershipEvents.RemoteEvents.ServerSideEvents.OnOwnershipRequestRejected.Invoke(clientId);
                }
                else
                {
                    _ownershipEvents.RemoteEvents.ClientSideEvents.OnOwnershipRequestRejected.Invoke(clientId);
                }
            }
        }
        private void OnOwnershipRequestAcceptedInternal(ulong clientId)
        {
            if (_debugParameters.OwnershipVerbose) Debug.Log("OWNERSHIP REQUEST ACCEPTED ON THE PLAYER " + clientId);
            OnOwnershipRequestAccepted(clientId);
            _ownershipEvents.Events.CrossSideEvents.OnOwnershipRequestAccepted.Invoke(clientId);
            if (IsServer)
            {
                _ownershipEvents.Events.ServerSideEvents.OnOwnershipRequestAccepted.Invoke(clientId);
            }
            else
            {
                _ownershipEvents.Events.ClientSideEvents.OnOwnershipRequestAccepted.Invoke(clientId);
            }
            if (clientId == NetworkManager.LocalClientId)
            {
                _ownershipEvents.LocalEvents.CrossSideEvents.OnOwnershipRequestAccepted.Invoke(clientId);
                if (IsServer)
                {
                    _ownershipEvents.LocalEvents.ServerSideEvents.OnOwnershipRequestAccepted.Invoke(clientId);
                }
                else
                {
                    _ownershipEvents.LocalEvents.ClientSideEvents.OnOwnershipRequestAccepted.Invoke(clientId);
                }
            }
            else
            {
                _ownershipEvents.RemoteEvents.CrossSideEvents.OnOwnershipRequestAccepted.Invoke(clientId);
                if (IsServer)
                {
                    _ownershipEvents.RemoteEvents.ServerSideEvents.OnOwnershipRequestAccepted.Invoke(clientId);
                }
                else
                {
                    _ownershipEvents.RemoteEvents.ClientSideEvents.OnOwnershipRequestAccepted.Invoke(clientId);
                }
            }
        }
        private void OnOwnershipDeclinedInternal(ulong clientId)
        {
            if (_debugParameters.OwnershipVerbose) Debug.Log("OWNERSHIP REQUEST DECLINED FROM THE PLAYER " + clientId);
            OnOwnershipDeclined(clientId);
            _ownershipEvents.Events.CrossSideEvents.OnOwnershipDeclined.Invoke(clientId);
            if (IsServer)
            {
                _ownershipEvents.Events.ServerSideEvents.OnOwnershipDeclined.Invoke(clientId);
            }
            else
            {
                _ownershipEvents.Events.ClientSideEvents.OnOwnershipDeclined.Invoke(clientId);
            }
            if (clientId == NetworkManager.LocalClientId)
            {
                _ownershipEvents.LocalEvents.CrossSideEvents.OnOwnershipDeclined.Invoke(clientId);
                if (IsServer)
                {
                    _ownershipEvents.LocalEvents.ServerSideEvents.OnOwnershipDeclined.Invoke(clientId);
                }
                else
                {
                    _ownershipEvents.LocalEvents.ClientSideEvents.OnOwnershipDeclined.Invoke(clientId);
                }
            }
            else
            {
                _ownershipEvents.RemoteEvents.CrossSideEvents.OnOwnershipDeclined.Invoke(clientId);
                if (IsServer)
                {
                    _ownershipEvents.RemoteEvents.ServerSideEvents.OnOwnershipDeclined.Invoke(clientId);
                }
                else
                {
                    _ownershipEvents.RemoteEvents.ClientSideEvents.OnOwnershipDeclined.Invoke(clientId);
                }
            }
        }

        /// <summary>
        /// Called when the ownership has been gained by any player
        /// </summary>
        public virtual void OnOwnershipGained(ulong clientId) { }
        /// <summary>
        /// Called when the ownership has been lost by any player
        /// </summary>
        public virtual void OnOwnershipLost(ulong clientId) { }
        /// <summary>
        /// Called when the ownership has been locked on any client or server
        /// </summary>
        public virtual void OnOwnershipLocked(ulong clientId) { }
        /// <summary>
        /// Called when the ownership has been unlocked on any client or server
        /// </summary>
        public virtual void OnOwnershipUnlocked(ulong clientId) { }
        /// <summary>
        /// Called when the ownership has been robbed from any client or server
        /// </summary>
        public virtual void OnOwnershipRobbedFrom(ulong clientId) { }
        /// <summary>
        /// Called when the ownership request has been rejected for any client or server
        /// </summary>
        public virtual void OnOwnershipRequestRejected(ulong clientId) { }
        /// <summary>
        /// Called when the ownership request has been accepted for any client or server
        /// </summary>
        public virtual void OnOwnershipRequestAccepted(ulong clientId) { }
        /// <summary>
        /// Called when the ownership request has been declined for any client or server
        /// </summary>
        public virtual void OnOwnershipDeclined(ulong clientId) { }
        #endregion
    }
}
