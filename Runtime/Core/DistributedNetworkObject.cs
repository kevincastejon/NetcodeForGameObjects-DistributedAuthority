using Unity.Netcode;
using UnityEngine;
namespace Caskev.NetcodeForGameObjects.DistributedAuthority
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(NetworkObject))]
    public class DistributedNetworkObject<T> : NetworkBehaviour
    {
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

        [Tooltip("The current distributed authority state.")]
        [SerializeField] private DistributedAuthorityState _currentState = DistributedAuthorityState.NONE;
        [Tooltip("Determine the ownership locking behaviour when an ownership request is accepted\r\n        - CUSTOM : Will use the RequestOwnership method parameter to determine if the ownership will be locked or not\r\n        - FORCE_LOCK : Will lock the ownership regardless of the RequestOwnership method parameter\r\n        - FORCE_UNLOCK : Will not lock the ownership regardless of the RequestOwnership method parameter")]
        [SerializeField] private OwnershipLockingOnGainPolicy _ownershipLockingOnGainPolicy;
        [Tooltip("A set of events related to the distributed authority state changes.")]
        [SerializeField] private StatesEventsInternal _statesEvents = new();
        [Tooltip("A set of events related to the ownership changes.")]
        [SerializeField] private OwnershipEventsInternal _ownershipEvents = new();
        [Tooltip("A set of debug parameters.")]
        [SerializeField] private DebugParametersInternal _debugParameters = new();


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
                    case DistributedAuthorityState.NONE:
                        OnExitNoneInternal();
                        break;
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
                    case DistributedAuthorityState.NONE:
                        OnEnterNoneInternal();
                        break;
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
        protected virtual void Awake()
        {
            if (GetComponent<NetworkObject>() == null)
            {
                Debug.LogError($"[DistributedNetworkObject] [Awake] No NetworkObject found on the GameObject {gameObject.name}", gameObject);
                enabled = false;
            }
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
            if (IsServer && NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnectedServerSide;
            }
            _localOwnershipSessionId = 0;
            _remoteOwnershipSessionId = 0;
            _decliningData = default;
            _useDecliningData = false;
            _useLockOwnership = false;
            if (_isOwnershipLocked)
            {
                _isOwnershipLocked = false;
                if (_debugParameters.OwnershipVerbose) Debug.Log($"[DistributedNetworkObject] [OnNetworkDespawn] OWNERSHIP UNLOCKED ON DESPAWN", gameObject);
                OnOwnershipUnlocked(0);
                _ownershipEvents.Events.CrossSideEvents.OnOwnershipUnlocked.Invoke(0);
            }
            CurrentState = DistributedAuthorityState.NONE;
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
                Debug.LogError($"[DistributedNetworkObject] [Awake] This method can only be called server-side", gameObject);
                return;
            }
            DeclineOwnershipServerSide(OwnerClientId, _remoteOwnershipSessionId, DecliningReason.SERVER_FORCE, default, false);
        }
        public void ForceDeclineOwnershipServerSide(T decliningData)
        {
            if (!IsServer)
            {
                Debug.LogError($"[DistributedNetworkObject] [Awake] This method can only be called server-side", gameObject);
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
                OnOwnershipUnlockedInternal(requester);
            }
        }
        #endregion

        #region OwnershipLock
        /// <summary>
        /// Locks ownership on the object so peers requests will be rejected. Can only be called by owner and server.
        /// </summary>
        public virtual void LockOwnership()
        {
            if (!IsSpawned)
            {
                return;
            }
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
            if (!IsSpawned)
            {
                return;
            }
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
        private void OnEnterNoneInternal()
        {
            if (_debugParameters.StatesVerbose) Debug.Log($"[DistributedNetworkObject] [OnEnterNoneInternal] ENTER NONE", gameObject);
            OnEnterNone();
            _statesEvents.OnEnteredNone.Invoke();
        }
        private void OnExitNoneInternal()
        {
            if (_debugParameters.StatesVerbose) Debug.Log($"[DistributedNetworkObject] [OnExitNoneInternal] EXIT NONE", gameObject);
            OnExitNone();
            _statesEvents.OnExitedNone.Invoke();
        }
        private void OnEnterAuthorityInternal()
        {
            if (_debugParameters.StatesVerbose) Debug.Log($"[DistributedNetworkObject] [OnEnterAuthorityInternal] ENTER AUTHORITY", gameObject);
            _remoteOwnershipSessionId = 0;
            OnEnterAuthority();
            _statesEvents.OnEnteredAuthority.Invoke();
        }
        private void OnExitAuthorityInternal()
        {
            if (_debugParameters.StatesVerbose) Debug.Log($"[DistributedNetworkObject] [OnExitAuthorityInternal] EXIT AUTHORITY", gameObject);
            OnExitAuthority();
            _statesEvents.OnExitedAuthority.Invoke();
        }
        private void OnEnterRemoteInternal()
        {
            if (_debugParameters.StatesVerbose) Debug.Log($"[DistributedNetworkObject] [OnEnterRemoteInternal] ENTER REMOTE", gameObject);
            OnEnterRemote();
            _statesEvents.OnEnteredRemote.Invoke();
        }
        private void OnExitRemoteInternal()
        {
            if (_debugParameters.StatesVerbose) Debug.Log($"[DistributedNetworkObject] [OnExitRemoteInternal] EXIT REMOTE", gameObject);
            OnExitRemote();
            _statesEvents.OnExitedRemote.Invoke();
        }
        private void OnEnterRequestingInternal()
        {
            if (_debugParameters.StatesVerbose) Debug.Log($"[DistributedNetworkObject] [OnEnterRequestingInternal] ENTER REQUESTING", gameObject);
            _localOwnershipSessionId++;
            RequestOwnershipRpc(_localOwnershipSessionId, _useLockOwnership);
            OnEnterRequesting();
            _statesEvents.OnEnteredRequesting.Invoke();
        }
        private void OnExitRequestingInternal()
        {
            if (_debugParameters.StatesVerbose) Debug.Log($"[DistributedNetworkObject] [OnExitRequestingInternal] EXIT REQUESTING", gameObject);
            OnExitRequesting();
            _statesEvents.OnExitedRequesting.Invoke();
        }
        private void OnEnterDecliningInternal()
        {
            if (_debugParameters.StatesVerbose) Debug.Log($"[DistributedNetworkObject] [OnEnterDecliningInternal] ENTER DECLINING", gameObject);
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
            if (_debugParameters.StatesVerbose) Debug.Log($"[DistributedNetworkObject] [OnExitDecliningInternal] EXIT DECLINING", gameObject);
            OnExitDeclining();
            _statesEvents.OnExitedDeclining.Invoke();
        }
        /// <summary>
        /// Called when entered the NONE state.
        /// </summary>
        public virtual void OnEnterNone() { }
        /// <summary>
        /// Called when exited the NONE state.
        /// </summary>
        public virtual void OnExitNone() { }
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
            if (_debugParameters.OwnershipVerbose) Debug.Log($"[DistributedNetworkObject] [OnOwnershipGainedInternal] OWNERSHIP GAINED ON PLAYER " + clientId, gameObject);
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
            if (_debugParameters.OwnershipVerbose) Debug.Log($"[DistributedNetworkObject] [OnOwnershipLostInternal] OWNERSHIP LOST ON PLAYER " + clientId, gameObject);
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
            if (_debugParameters.OwnershipVerbose) Debug.Log($"[DistributedNetworkObject] [OnOwnershipLockedInternal] OWNERSHIP LOCKED ON THE PLAYER " + clientId, gameObject);
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
            if (_debugParameters.OwnershipVerbose) Debug.Log($"[DistributedNetworkObject] [OnOwnershipUnlockedInternal] OWNERSHIP UNLOCKED ON THE PLAYER " + clientId, gameObject);
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
            if (_debugParameters.OwnershipVerbose) Debug.Log($"[DistributedNetworkObject] [OnOwnershipRobbedFromInternal] OWNERSHIP ROBBED FROM THE PLAYER " + clientId, gameObject);
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
            if (_debugParameters.OwnershipVerbose) Debug.Log($"[DistributedNetworkObject] [OnOwnershipRequestRejectedInternal] OWNERSHIP REQUEST REJECTED ON THE PLAYER " + clientId, gameObject);
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
            if (_debugParameters.OwnershipVerbose) Debug.Log($"[DistributedNetworkObject] [OnOwnershipRequestAcceptedInternal] OWNERSHIP REQUEST ACCEPTED ON THE PLAYER " + clientId, gameObject);
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
            if (_debugParameters.OwnershipVerbose) Debug.Log($"[DistributedNetworkObject] [OnOwnershipDeclinedInternal] OWNERSHIP REQUEST DECLINED FROM THE PLAYER " + clientId, gameObject);
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
