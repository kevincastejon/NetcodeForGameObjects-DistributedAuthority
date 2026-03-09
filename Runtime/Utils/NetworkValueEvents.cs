using System;
using UnityEngine;
using UnityEngine.Events;

namespace Caskev.NetcodeForGameObjects.DistributedAuthority
{
    /// <summary>
    /// Three sets of events related to a network value change, organized into a server/client and local/remote oriented way.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]   
    public class NetworkValueEvents<T>
    {
        /// <summary>
        /// A set of events related to a network value change, organized into a local/remote oriented way.
        /// </summary>
        /// <typeparam name="T2">Any Netcode RPC's supported data</typeparam>
        [Serializable]
        public class ValueEvents<T2>
        {
            [Tooltip("Fires when the value is changed from either local or remote side.")]
            [SerializeField] private UnityEvent<T2> _onValueChanged = new();
            [Tooltip("Fires when the value is changed from local side.")]
            [SerializeField] private UnityEvent<T2> _onLocalValueChanged = new();
            [Tooltip("Fires when the value is changed from remote side.")]
            [SerializeField] private UnityEvent<T2> _onRemoteValueChanged = new();
            /// <summary>
            /// Fires when the value is changed from either local or remote side.
            /// </summary>
            public UnityEvent<T2> OnValueChanged { get => _onValueChanged; }
            /// <summary>
            /// Fires when the value is changed local side.
            /// </summary>
            public UnityEvent<T2> OnLocalValueChanged { get => _onLocalValueChanged; }
            /// <summary>
            /// Fires when the value is changed remote side.
            /// </summary>
            public UnityEvent<T2> OnRemoteValueChanged { get => _onRemoteValueChanged; }
        }
        [Tooltip("A set of events related to a network value change, organized into a local/remote oriented way. These events will fire regardless of the server/client state.")]
        [SerializeField] private ValueEvents<T> _crossSideEvents = new();
        [Tooltip("A set of events related to a network value change, organized into a local/remote oriented way. These events will fire only server-side.")]
        [SerializeField] private ValueEvents<T> _serverSideEvents = new();
        [Tooltip("A set of events related to a network value change, organized into a local/remote oriented way. These events will fire only client-side.")]
        [SerializeField] private ValueEvents<T> _clientSideEvents = new();
        /// <summary>
        /// A set of events related to a network value change, organized into a local/remote oriented way. These events will fire regardless of the server/client state.
        /// </summary>
        public ValueEvents<T> CrossSideEvents { get => _crossSideEvents; }
        /// <summary>
        /// A set of events related to a network value change, organized into a local/remote oriented way. These events will fire only server-side.
        /// </summary>
        public ValueEvents<T> ServerSideEvents { get => _serverSideEvents; }
        /// <summary>
        /// A set of events related to a network value change, organized into a local/remote oriented way. These events will fire only client-side.
        /// </summary>
        public ValueEvents<T> ClientSideEvents { get => _clientSideEvents; }
    }
}