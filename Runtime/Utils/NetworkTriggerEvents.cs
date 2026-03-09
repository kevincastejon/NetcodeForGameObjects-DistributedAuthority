using System;
using UnityEngine;
using UnityEngine.Events;

namespace Caskev.NetcodeForGameObjects.DistributedAuthority
{   
    /// <summary>
    /// Three sets of events related to a network trigger, organized into a server/client and local/remote oriented way.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class NetworkTriggerEvents
    {
        /// <summary>
        /// A set of events related to a network trigger, organized into a local/remote oriented way.
        /// </summary>
        /// <typeparam name="T2">Any Netcode RPC's supported data</typeparam>
        [Serializable]
        public class TriggerEvents
        {
            [Tooltip("Fires when triggered is changed from either local or remote side.")]
            [SerializeField] private UnityEvent _onTrigger = new();
            [Tooltip("Fires when triggered is changed from local side.")]
            [SerializeField] private UnityEvent _onLocalTrigger = new();
            [Tooltip("Fires when triggered is changed from remote side.")]
            [SerializeField] private UnityEvent _onRemoteTrigger = new();
            /// <summary>
            /// Fires when triggered from either local or remote side.
            /// </summary>
            public UnityEvent OnTrigger { get => _onTrigger; }
            /// <summary>
            /// Fires when triggered is changed local side.
            /// </summary>
            public UnityEvent OnLocalTrigger { get => _onLocalTrigger; }
            /// <summary>
            /// Fires when triggered is changed remote side.
            /// </summary>
            public UnityEvent OnRemoteTrigger { get => _onRemoteTrigger; }
        }
        [Tooltip("A set of events related to a network trigger, organized into a local/remote oriented way. These events will fire regardless of the server/client state.")]
        [SerializeField] private TriggerEvents _crossSideEvents = new();
        [Tooltip("A set of events related to a network trigger, organized into a local/remote oriented way. These events will fire only server-side.")]
        [SerializeField] private TriggerEvents _serverSideEvents = new();
        [Tooltip("A set of events related to a network trigger, organized into a local/remote oriented way. These events will fire only client-side.")]
        [SerializeField] private TriggerEvents _clientSideEvents = new();
        /// <summary>
        /// A set of events related to a network trigger, organized into a local/remote oriented way. These events will fire regardless of the server/client state.
        /// </summary>
        public TriggerEvents CrossSideEvents { get => _crossSideEvents; }
        /// <summary>
        /// A set of events related to a network trigger, organized into a local/remote oriented way. These events will fire only server-side.
        /// </summary>
        public TriggerEvents ServerSideEvents { get => _serverSideEvents; }
        /// <summary>
        /// A set of events related to a network trigger, organized into a local/remote oriented way. These events will fire only client-side.
        /// </summary>
        public TriggerEvents ClientSideEvents { get => _clientSideEvents; }
    }
}