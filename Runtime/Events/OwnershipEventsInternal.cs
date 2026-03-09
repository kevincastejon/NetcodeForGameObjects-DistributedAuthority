using System;
using UnityEngine;
using UnityEngine.Events;
namespace Caskev.NetcodeForGameObjects.DistributedAuthority
{
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
                    [SerializeField] private UnityEvent<ulong> _onOwnershipGained = new();
                    [Tooltip("Fires when the ownership has been lost by player")]
                    [SerializeField] private UnityEvent<ulong> _onOwnershipLost = new();
                    [Tooltip("Fires when ownership is locked on player")]
                    [SerializeField] private UnityEvent<ulong> _onOwnershipLocked = new();
                    [Tooltip("Fires when ownership is unlocked on player")]
                    [SerializeField] private UnityEvent<ulong> _onOwnershipUnlocked = new();
                    [Tooltip("Fires when ownership is robbed from player")]
                    [SerializeField] private UnityEvent<ulong> _onOwnershipRobbedFrom = new();
                    [Tooltip("Fires when ownership request is rejected for player")]
                    [SerializeField] private UnityEvent<ulong> _onOwnershipRequestRejected = new();
                    [Tooltip("Fires when ownership request is accepted for player")]
                    [SerializeField] private UnityEvent<ulong> _onOwnershipRequestAccepted = new();
                    [Tooltip("Fires when ownership is declined from player")]
                    [SerializeField] private UnityEvent<ulong> _onOwnershipDeclined = new();

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
                [SerializeField] private OwnershipEventsSet _crossSideEvents = new();
                [Tooltip("A set of events related to ownership changes. These events will fire only client-side")]
                [SerializeField] private OwnershipEventsSet _clientSideEvents = new();
                [Tooltip("A set of events related to ownership changes. These events will fire only server-side")]
                [SerializeField] private OwnershipEventsSet _serverSideEvents = new();
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
            [SerializeField] private OwnershipEventsGroup _events = new();
            [Tooltip("Three sets of events related to ownership changes. These events will fire only when the local client or server is concerned")]
            [SerializeField] private OwnershipEventsGroup _localEvents = new();
            [Tooltip("Three sets of events related to ownership changes. These events will fire only when a remote client or server is concerned")]
            [SerializeField] private OwnershipEventsGroup _remoteEvents = new();
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
}
