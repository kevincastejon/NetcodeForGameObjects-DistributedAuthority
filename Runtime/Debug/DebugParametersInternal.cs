using System;
using UnityEngine;
namespace Caskev.NetcodeForGameObjects.DistributedAuthority
{
    /// <summary>
    /// A set of debug parameters
    /// </summary>
    [Serializable]
        public class DebugParametersInternal
        {
            [Tooltip("Will log every state entering and exiting")]
            [SerializeField] private bool _statesVerbose = new();
            [Tooltip("Will log every ownership gain, loss, locking, and unlocking")]
            [SerializeField] private bool _ownershipVerbose = new();
            /// <summary>
            /// Will log every state entering and exiting
            /// </summary>
            public bool StatesVerbose { get => _statesVerbose; set => _statesVerbose = value; }
            /// <summary>
            /// Will log every ownership gain, loss, locking, and unlocking
            /// </summary>
            public bool OwnershipVerbose { get => _ownershipVerbose; set => _ownershipVerbose = value; }
        }
}
