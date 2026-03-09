using System;
using UnityEngine;
using UnityEngine.Events;
namespace Caskev.NetcodeForGameObjects.DistributedAuthority
{
    /// <summary>
    /// A set of events related to the distributed authority state changes
    /// </summary>
    [Serializable]
        public class StatesEventsInternal
        {
            [Tooltip("Fires when the state has changed")]
            [SerializeField] private UnityEvent<DistributedAuthorityState> _onStateChanged = new();
            [Tooltip("Fires when entered NONE state")]
            [SerializeField] private UnityEvent _onEnterNone = new();
            [Tooltip("Fires when exited NONE state")]
            [SerializeField] private UnityEvent _onExitNone = new();
            [Tooltip("Fires when entered AUTHORITY state")]
            [SerializeField] private UnityEvent _onEnterAuthority = new();
            [Tooltip("Fires when exited AUTHORITY state")]
            [SerializeField] private UnityEvent _onExitAuthority = new();
            [Tooltip("Fires when entered REMOTE state")]
            [SerializeField] private UnityEvent _onEnterRemote = new();
            [Tooltip("Fires when exited REMOTE state")]
            [SerializeField] private UnityEvent _onExitRemote = new();
            [Tooltip("Fires when entered REQUESTING state")]
            [SerializeField] private UnityEvent _onEnterRequesting = new();
            [Tooltip("Fires when exited REQUESTING state")]
            [SerializeField] private UnityEvent _onExitRequesting = new();
            [Tooltip("Fires when entered DECLINING state")]
            [SerializeField] private UnityEvent _onEnterDeclining = new();
            [Tooltip("Fires when exited DECLINING state")]
            [SerializeField] private UnityEvent _onExitDeclining = new();
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
            /// <summary>
            /// Fires when entered NONE state
            /// </summary>
            public UnityEvent OnEnteredNone { get => _onEnterNone; }
            /// <summary>
            /// Fires when exited NONE state
            /// </summary>
            public UnityEvent OnExitedNone { get => _onExitNone; }
        }
}
