using Caskev.NetcodeForGameObjects.DistributedAuthority;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Caskev.Samples.NetcodeForGameObjects.DistributedAuthority.DistributedNetworkUIComponents
{
    [GenerateSerializationForType(typeof(UIData))]
    public class DistributedNetworkUIComponent : DistributedNetworkObject<UIData>
    {
        /// <summary>
        /// An enumeration of the Selectable UI component supported types
        /// </summary>
        public enum SelectableInternalType
        {
            TMP_Dropdown,
            TMP_InputField,
            Scrollbar,
            Slider,
            Toggle,
            Button
        }

        [Tooltip("The UI component which you want to distribute authority")]
        [SerializeField] private Selectable _selectable;
        [Tooltip("Is the UI component selected.")]
        [SerializeField] private bool _isSelected;
        [Tooltip("The UI component will be interactable only when ownership is unlocked, or locked on the local owner")]
        [SerializeField] private bool _disableInteractionOnLockedOwnership;
        [Tooltip("Will allow triggering for non owners client and server")]
        [SerializeField] private bool _allowTriggeringForNonOwners;
        [Tooltip("Three sets of events related to a network value change, organized into a server/client and local/remote oriented way.")]
        [SerializeField] private NetworkValueEvents<int> _intValueEvents;
        [Tooltip("Three sets of events related to a network value change, organized into a server/client and local/remote oriented way.")]
        [SerializeField] private NetworkValueEvents<float> _floatValueEvents;
        [Tooltip("Three sets of events related to a network value change, organized into a server/client and local/remote oriented way.")]
        [SerializeField] private NetworkValueEvents<bool> _boolValueEvents;
        [Tooltip("Three sets of events related to a network value change, organized into a server/client and local/remote oriented way.")]
        [SerializeField] private NetworkValueEvents<string> _stringValueEvents;
        [Tooltip("Three sets of events related to a network trigger, organized into a server/client and local/remote oriented way.")]
        [SerializeField] private NetworkTriggerEvents _triggerEvents;
        [SerializeField][HideInInspector] private SelectableInternalType _selectableType;

        private NetworkVariable<UIData> _networkValue = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        private TMP_Dropdown _tmpDropdown;
        private TMP_InputField _tmpInputField;
        private Scrollbar _scrollbar;
        private Slider _slider;
        private Toggle _toggle;
        private Button _button;
        private EventTrigger _evtTrigger;
        private EventTrigger.Entry _onSelect;
        private EventTrigger.Entry _onDeselect;

        /// <summary>
        /// Is the UI component selected.
        /// </summary>
        public bool IsSelected { get => _isSelected; }
        /// <summary>
        /// The Selectable UI component type.
        /// </summary>
        public SelectableInternalType SelectableType { get => _selectableType; }
        /// <summary>
        /// The UI component which you want to distribute authority.
        /// </summary>
        public Selectable Selectable { get => _selectable; }

        protected override void Awake()
        {
            if (_selectable == null)
            {
                Debug.LogWarning("No Selectable reference. Disabling the behaviour.");
            }
            DetermineUIType();
            _evtTrigger = _selectable.gameObject.AddComponent<EventTrigger>();
            _onSelect = new EventTrigger.Entry() { eventID = EventTriggerType.Select };
            _evtTrigger.triggers.Add(_onSelect);
            _onDeselect = new EventTrigger.Entry() { eventID = EventTriggerType.Deselect };
            _evtTrigger.triggers.Add(_onDeselect);
            _onSelect.callback.AddListener(OnStartControl);
            _onDeselect.callback.AddListener(OnStopControl);
            RegisterLocalEvent();
            _networkValue.OnValueChanged += OnRemoteValueChange;
        }
        private void Update()
        {
            if (_disableInteractionOnLockedOwnership)
            {
                _selectable.interactable = !(CurrentState == DistributedAuthorityState.REMOTE && IsOwnershipLocked);
            }
        }

        private void DetermineUIType()
        {
            if (_selectable is TMP_Dropdown)
            {
                _selectableType = SelectableInternalType.TMP_Dropdown;
                _tmpDropdown = _selectable as TMP_Dropdown;
            }
            else if (_selectable is TMP_InputField)
            {
                _selectableType = SelectableInternalType.TMP_InputField;
                _tmpInputField = _selectable as TMP_InputField;
            }
            else if (_selectable is Scrollbar)
            {
                _selectableType = SelectableInternalType.Scrollbar;
                _scrollbar = _selectable as Scrollbar;
            }
            else if (_selectable is Slider)
            {
                _selectableType = SelectableInternalType.Slider;
                _slider = _selectable as Slider;
            }
            else if (_selectable is Toggle)
            {
                _selectableType = SelectableInternalType.Toggle;
                _toggle = _selectable as Toggle;
            }
            else if (_selectable is Button)
            {
                _selectableType = SelectableInternalType.Button;
                _button = _selectable as Button;
            }
        }
        private void RegisterLocalEvent()
        {
            switch (_selectableType)
            {
                case SelectableInternalType.TMP_Dropdown:
                    _tmpDropdown.onValueChanged.AddListener(OnLocalValueChange);
                    break;
                case SelectableInternalType.TMP_InputField:
                    _tmpInputField.onValueChanged.AddListener(OnLocalValueChange);
                    break;
                case SelectableInternalType.Scrollbar:
                    _scrollbar.onValueChanged.AddListener(OnLocalValueChange);
                    break;
                case SelectableInternalType.Slider:
                    _slider.onValueChanged.AddListener(OnLocalValueChange);
                    break;
                case SelectableInternalType.Toggle:
                    _toggle.onValueChanged.AddListener(OnLocalValueChange);
                    break;
                case SelectableInternalType.Button:
                    _button.onClick.AddListener(OnLocalTrigger);
                    break;
                default:
                    Debug.LogWarning("No supported UI component found! Disabling the behaviour.");
                    enabled = false;
                    break;
            }
        }

        private void OnStartControl(BaseEventData evt = null)
        {
            RequestOwnership(true);
        }
        private void OnStopControl(BaseEventData evt = null)
        {
            if (_selectableType == SelectableInternalType.TMP_Dropdown && _tmpDropdown.IsExpanded)
            {
                return;
            }
            if (_selectableType == SelectableInternalType.Button)
            {
                DeclineOwnership();
            }
            else
            {
                DeclineOwnership(GetUIData());
            }
        }

        private int GetSelectableIntValue()
        {
            if (_selectableType == SelectableInternalType.TMP_Dropdown)
            {
                return _tmpDropdown.value;
            }
            else
            {
                throw new Exception("Wrong value requested");
            }
        }
        private float GetSelectableFloatValue()
        {
            switch (_selectableType)
            {
                case SelectableInternalType.Scrollbar:
                    return _scrollbar.value;
                case SelectableInternalType.Slider:
                    return _slider.value;
                default:
                    throw new Exception("Wrong value requested");
            }
        }
        private bool GetSelectableBoolValue()
        {
            if (_selectableType == SelectableInternalType.Toggle)
            {
                return _toggle.isOn;
            }
            else
            {
                throw new Exception("Wrong value requested");
            }
        }
        private string GetSelectableStringValue()
        {
            if (_selectableType == SelectableInternalType.TMP_InputField)
            {
                return _tmpInputField.text;
            }
            else
            {
                throw new Exception("Wrong value requested");
            }
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (IsOwner)
            {
                switch (_selectableType)
                {
                    case SelectableInternalType.TMP_Dropdown:
                        _networkValue.OnValueChanged -= OnRemoteValueChange;
                        _networkValue.Value = new(GetSelectableIntValue());
                        _networkValue.OnValueChanged += OnRemoteValueChange;
                        break;
                    case SelectableInternalType.TMP_InputField:
                        _networkValue.OnValueChanged -= OnRemoteValueChange;
                        _networkValue.Value = new(GetSelectableStringValue());
                        _networkValue.OnValueChanged += OnRemoteValueChange;
                        break;
                    case SelectableInternalType.Scrollbar:
                        _networkValue.OnValueChanged -= OnRemoteValueChange;
                        _networkValue.Value = new(GetSelectableFloatValue());
                        _networkValue.OnValueChanged += OnRemoteValueChange;
                        break;
                    case SelectableInternalType.Slider:
                        _networkValue.OnValueChanged -= OnRemoteValueChange;
                        _networkValue.Value = new(GetSelectableFloatValue());
                        _networkValue.OnValueChanged += OnRemoteValueChange;
                        break;
                    case SelectableInternalType.Toggle:
                        _networkValue.OnValueChanged -= OnRemoteValueChange;
                        _networkValue.Value = new(GetSelectableBoolValue());
                        _networkValue.OnValueChanged += OnRemoteValueChange;
                        break;
                    default:
                        break;
                }
            }
        }
        public override void OnOwnershipGained(ulong clientId)
        {
            if (clientId != NetworkManager.LocalClientId || CurrentState == DistributedAuthorityState.REMOTE)
            {
                return;
            }
            switch (_selectableType)
            {
                case SelectableInternalType.TMP_Dropdown:
                    if (_tmpDropdown.value != _networkValue.Value.intValue)
                    {
                        _networkValue.Value = new(_tmpDropdown.value);
                    }
                    break;
                case SelectableInternalType.TMP_InputField:
                    if (_tmpInputField.text != _networkValue.Value.stringValue)
                    {
                        _networkValue.Value = new(_tmpInputField.text);
                    }
                    break;
                case SelectableInternalType.Scrollbar:
                    if (_scrollbar.value != _networkValue.Value.floatValue)
                    {
                        _networkValue.Value = new(_scrollbar.value);
                    }
                    break;
                case SelectableInternalType.Slider:
                    if (_slider.value != _networkValue.Value.floatValue)
                    {
                        _networkValue.Value = new(_slider.value);
                    }
                    break;
                case SelectableInternalType.Toggle:
                    if (_toggle.isOn != _networkValue.Value.boolValue)
                    {
                        _networkValue.Value = new(_toggle.isOn);
                    }
                    break;
                default:
                    break;
            }
        }
        public override void OnClientDecliningServerSide(DecliningReason decliningReason, bool isDecliningDataProvided, UIData decliningData)
        {
            if (isDecliningDataProvided)
            {
                switch (_selectableType)
                {
                    case SelectableInternalType.TMP_Dropdown:
                        _tmpDropdown.onValueChanged.RemoveListener(OnLocalValueChange);
                        _networkValue.OnValueChanged -= OnRemoteValueChange;
                        _tmpDropdown.value = decliningData.intValue;
                        _networkValue.Value = new(decliningData.intValue);
                        _tmpDropdown.onValueChanged.AddListener(OnLocalValueChange);
                        _networkValue.OnValueChanged += OnRemoteValueChange;
                        FireRemoteEvents();
                        break;
                    case SelectableInternalType.TMP_InputField:
                        _tmpInputField.onValueChanged.RemoveListener(OnLocalValueChange);
                        _networkValue.OnValueChanged -= OnRemoteValueChange;
                        _tmpInputField.text = decliningData.stringValue.ToString();
                        _networkValue.Value = new(decliningData.stringValue);
                        _tmpInputField.onValueChanged.AddListener(OnLocalValueChange);
                        _networkValue.OnValueChanged += OnRemoteValueChange;
                        FireRemoteEvents();
                        break;
                    case SelectableInternalType.Scrollbar:
                        _scrollbar.onValueChanged.RemoveListener(OnLocalValueChange);
                        _networkValue.OnValueChanged -= OnRemoteValueChange;
                        _scrollbar.value = decliningData.floatValue;
                        _networkValue.Value = new(decliningData.floatValue);
                        _scrollbar.onValueChanged.AddListener(OnLocalValueChange);
                        _networkValue.OnValueChanged += OnRemoteValueChange;
                        FireRemoteEvents();
                        break;
                    case SelectableInternalType.Slider:
                        _slider.onValueChanged.RemoveListener(OnLocalValueChange);
                        _networkValue.OnValueChanged -= OnRemoteValueChange;
                        _slider.value = decliningData.floatValue;
                        _networkValue.Value = new(decliningData.floatValue);
                        _slider.onValueChanged.AddListener(OnLocalValueChange);
                        _networkValue.OnValueChanged += OnRemoteValueChange;
                        FireRemoteEvents();
                        break;
                    case SelectableInternalType.Toggle:
                        _toggle.onValueChanged.RemoveListener(OnLocalValueChange);
                        _networkValue.OnValueChanged -= OnRemoteValueChange;
                        _toggle.isOn = decliningData.boolValue;
                        _networkValue.Value = new(decliningData.boolValue);
                        _toggle.onValueChanged.AddListener(OnLocalValueChange);
                        _networkValue.OnValueChanged += OnRemoteValueChange;
                        FireRemoteEvents();
                        break;
                    default:
                        break;
                }
            }
        }

        private void OnRemoteValueChange(UIData previousValue, UIData newValue)
        {
            if (CurrentState == DistributedAuthorityState.REMOTE)
            {
                switch (_selectableType)
                {
                    case SelectableInternalType.TMP_Dropdown:
                        _tmpDropdown.onValueChanged.RemoveListener(OnLocalValueChange);
                        _tmpDropdown.value = newValue.intValue;
                        _tmpDropdown.onValueChanged.AddListener(OnLocalValueChange);
                        FireRemoteEvents();
                        break;
                    case SelectableInternalType.TMP_InputField:
                        _tmpInputField.onValueChanged.RemoveListener(OnLocalValueChange);
                        _tmpInputField.text = newValue.stringValue.ToString();
                        _tmpInputField.onValueChanged.AddListener(OnLocalValueChange);
                        FireRemoteEvents();
                        break;
                    case SelectableInternalType.Scrollbar:
                        _scrollbar.onValueChanged.RemoveListener(OnLocalValueChange);
                        _scrollbar.value = newValue.floatValue;
                        _scrollbar.onValueChanged.AddListener(OnLocalValueChange);
                        FireRemoteEvents();
                        break;
                    case SelectableInternalType.Slider:
                        _slider.onValueChanged.RemoveListener(OnLocalValueChange);
                        _slider.value = newValue.floatValue;
                        _slider.onValueChanged.AddListener(OnLocalValueChange);
                        FireRemoteEvents();
                        break;
                    case SelectableInternalType.Toggle:
                        _toggle.onValueChanged.RemoveListener(OnLocalValueChange);
                        _toggle.isOn = newValue.boolValue;
                        _toggle.onValueChanged.AddListener(OnLocalValueChange);
                        FireRemoteEvents();
                        break;
                    default:
                        break;
                }
            }
        }
        private void OnLocalValueChange(int value)
        {
            if (CurrentState != DistributedAuthorityState.REMOTE && IsOwner)
            {
                _networkValue.OnValueChanged -= OnRemoteValueChange;
                _networkValue.Value = new(value);
                _networkValue.OnValueChanged += OnRemoteValueChange;
                FireLocalEvents();
            }
            else
            {
                if (_selectableType == SelectableInternalType.TMP_Dropdown)
                {
                    _tmpDropdown.value = value;
                }
            }
        }
        private void OnLocalValueChange(float value)
        {
            if (CurrentState != DistributedAuthorityState.REMOTE && IsOwner)
            {
                _networkValue.OnValueChanged -= OnRemoteValueChange;
                _networkValue.Value = new(value);
                _networkValue.OnValueChanged += OnRemoteValueChange;
                FireLocalEvents();
            }
            else
            {
                switch (_selectableType)
                {
                    case SelectableInternalType.Scrollbar:
                        _scrollbar.value = value;
                        break;
                    case SelectableInternalType.Slider:
                        _slider.value = value;
                        break;
                    default:
                        break;
                }
            }
        }
        private void OnLocalValueChange(bool value)
        {
            if (CurrentState != DistributedAuthorityState.REMOTE && IsOwner)
            {
                _networkValue.OnValueChanged -= OnRemoteValueChange;
                _networkValue.Value = new(value);
                _networkValue.OnValueChanged += OnRemoteValueChange;
                FireLocalEvents();
            }
            else
            {
                _toggle.isOn = value;
            }
        }
        private void OnLocalValueChange(string value)
        {
            if (CurrentState != DistributedAuthorityState.REMOTE && IsOwner)
            {
                _networkValue.OnValueChanged -= OnRemoteValueChange;
                _networkValue.Value = new(value);
                _networkValue.OnValueChanged += OnRemoteValueChange;
                FireLocalEvents();
            }
            else
            {
                if (_selectableType == SelectableInternalType.TMP_InputField)
                {
                    _tmpInputField.text = value;
                }
            }
        }

        private void OnLocalTrigger()
        {
            if (_allowTriggeringForNonOwners || (CurrentState != DistributedAuthorityState.REMOTE && IsOwner))
            {
                FireLocalEvents();
                RelayTriggerRpc();
            }
        }
        [Rpc(SendTo.Server)]
        private void RelayTriggerRpc(RpcParams rpcParams = default)
        {
            if (_selectableType == SelectableInternalType.Button)
            {
                OnRemoteTriggerRpc(new() { Send = new() { Target = RpcTarget.Not(rpcParams.Receive.SenderClientId, RpcTargetUse.Temp) } });
            }
        }
        [Rpc(SendTo.Everyone, AllowTargetOverride = true)]
        private void OnRemoteTriggerRpc(RpcParams rpcParams = default)
        {
            _button.onClick.RemoveListener(OnLocalTrigger);
            _button.onClick.Invoke();
            _button.onClick.AddListener(OnLocalTrigger);
            FireRemoteEvents();
        }

        private UIData GetUIData()
        {
            switch (_selectableType)
            {
                case SelectableInternalType.TMP_Dropdown:
                    return new(_tmpDropdown.value);
                case SelectableInternalType.TMP_InputField:
                    return new(_tmpInputField.text);
                case SelectableInternalType.Scrollbar:
                    return new(_scrollbar.value);
                case SelectableInternalType.Slider:
                    return new(_slider.value);
                case SelectableInternalType.Toggle:
                    return new(_toggle.isOn);
                default:
                    throw new Exception("Wrong type requested");
            }
        }

        private void FireLocalEvents()
        {
            switch (_selectableType)
            {
                case SelectableInternalType.TMP_Dropdown:
                    _intValueEvents.CrossSideEvents.OnValueChanged.Invoke(_tmpDropdown.value);
                    _intValueEvents.CrossSideEvents.OnLocalValueChanged.Invoke(_tmpDropdown.value);
                    if (IsServer)
                    {
                        _intValueEvents.ServerSideEvents.OnValueChanged.Invoke(_tmpDropdown.value);
                        _intValueEvents.ServerSideEvents.OnLocalValueChanged.Invoke(_tmpDropdown.value);
                    }
                    else
                    {
                        _intValueEvents.ClientSideEvents.OnValueChanged.Invoke(_tmpDropdown.value);
                        _intValueEvents.ClientSideEvents.OnLocalValueChanged.Invoke(_tmpDropdown.value);
                    }
                    break;
                case SelectableInternalType.TMP_InputField:
                    _stringValueEvents.CrossSideEvents.OnValueChanged.Invoke(_tmpInputField.text);
                    _stringValueEvents.CrossSideEvents.OnLocalValueChanged.Invoke(_tmpInputField.text);
                    if (IsServer)
                    {
                        _stringValueEvents.ServerSideEvents.OnValueChanged.Invoke(_tmpInputField.text);
                        _stringValueEvents.ServerSideEvents.OnLocalValueChanged.Invoke(_tmpInputField.text);
                    }
                    else
                    {
                        _stringValueEvents.ClientSideEvents.OnValueChanged.Invoke(_tmpInputField.text);
                        _stringValueEvents.ClientSideEvents.OnLocalValueChanged.Invoke(_tmpInputField.text);
                    }
                    break;
                case SelectableInternalType.Scrollbar:
                    _floatValueEvents.CrossSideEvents.OnValueChanged.Invoke(_scrollbar.value);
                    _floatValueEvents.CrossSideEvents.OnLocalValueChanged.Invoke(_scrollbar.value);
                    if (IsServer)
                    {
                        _floatValueEvents.ServerSideEvents.OnValueChanged.Invoke(_scrollbar.value);
                        _floatValueEvents.ServerSideEvents.OnLocalValueChanged.Invoke(_scrollbar.value);
                    }
                    else
                    {
                        _floatValueEvents.ClientSideEvents.OnValueChanged.Invoke(_scrollbar.value);
                        _floatValueEvents.ClientSideEvents.OnLocalValueChanged.Invoke(_scrollbar.value);
                    }
                    break;
                case SelectableInternalType.Slider:
                    _floatValueEvents.CrossSideEvents.OnValueChanged.Invoke(_slider.value);
                    _floatValueEvents.CrossSideEvents.OnLocalValueChanged.Invoke(_slider.value);
                    if (IsServer)
                    {
                        _floatValueEvents.ServerSideEvents.OnValueChanged.Invoke(_slider.value);
                        _floatValueEvents.ServerSideEvents.OnLocalValueChanged.Invoke(_slider.value);
                    }
                    else
                    {
                        _floatValueEvents.ClientSideEvents.OnValueChanged.Invoke(_slider.value);
                        _floatValueEvents.ClientSideEvents.OnLocalValueChanged.Invoke(_slider.value);
                    }
                    break;
                case SelectableInternalType.Toggle:
                    _boolValueEvents.CrossSideEvents.OnValueChanged.Invoke(_toggle.isOn);
                    _boolValueEvents.CrossSideEvents.OnLocalValueChanged.Invoke(_toggle.isOn);
                    if (IsServer)
                    {
                        _boolValueEvents.ServerSideEvents.OnValueChanged.Invoke(_toggle.isOn);
                        _boolValueEvents.ServerSideEvents.OnLocalValueChanged.Invoke(_toggle.isOn);
                    }
                    else
                    {
                        _boolValueEvents.ClientSideEvents.OnValueChanged.Invoke(_toggle.isOn);
                        _boolValueEvents.ClientSideEvents.OnLocalValueChanged.Invoke(_toggle.isOn);
                    }
                    break;
                default:
                    break;
            }
        }
        private void FireRemoteEvents()
        {
            switch (_selectableType)
            {
                case SelectableInternalType.TMP_Dropdown:
                    _intValueEvents.CrossSideEvents.OnValueChanged.Invoke(_tmpDropdown.value);
                    _intValueEvents.CrossSideEvents.OnRemoteValueChanged.Invoke(_tmpDropdown.value);
                    if (IsServer)
                    {
                        _intValueEvents.ServerSideEvents.OnValueChanged.Invoke(_tmpDropdown.value);
                        _intValueEvents.ServerSideEvents.OnRemoteValueChanged.Invoke(_tmpDropdown.value);
                    }
                    else
                    {
                        _intValueEvents.ClientSideEvents.OnValueChanged.Invoke(_tmpDropdown.value);
                        _intValueEvents.ClientSideEvents.OnRemoteValueChanged.Invoke(_tmpDropdown.value);
                    }
                    break;
                case SelectableInternalType.TMP_InputField:
                    _stringValueEvents.CrossSideEvents.OnValueChanged.Invoke(_tmpInputField.text);
                    _stringValueEvents.CrossSideEvents.OnRemoteValueChanged.Invoke(_tmpInputField.text);
                    if (IsServer)
                    {
                        _stringValueEvents.ServerSideEvents.OnValueChanged.Invoke(_tmpInputField.text);
                        _stringValueEvents.ServerSideEvents.OnRemoteValueChanged.Invoke(_tmpInputField.text);
                    }
                    else
                    {
                        _stringValueEvents.ClientSideEvents.OnValueChanged.Invoke(_tmpInputField.text);
                        _stringValueEvents.ClientSideEvents.OnRemoteValueChanged.Invoke(_tmpInputField.text);
                    }
                    break;
                case SelectableInternalType.Scrollbar:
                    _floatValueEvents.CrossSideEvents.OnValueChanged.Invoke(_scrollbar.value);
                    _floatValueEvents.CrossSideEvents.OnLocalValueChanged.Invoke(_scrollbar.value);
                    if (IsServer)
                    {
                        _floatValueEvents.ServerSideEvents.OnValueChanged.Invoke(_scrollbar.value);
                        _floatValueEvents.ServerSideEvents.OnLocalValueChanged.Invoke(_scrollbar.value);
                    }
                    else
                    {
                        _floatValueEvents.ClientSideEvents.OnValueChanged.Invoke(_scrollbar.value);
                        _floatValueEvents.ClientSideEvents.OnLocalValueChanged.Invoke(_scrollbar.value);
                    }
                    break;
                case SelectableInternalType.Slider:
                    _floatValueEvents.CrossSideEvents.OnValueChanged.Invoke(_slider.value);
                    _floatValueEvents.CrossSideEvents.OnLocalValueChanged.Invoke(_slider.value);
                    if (IsServer)
                    {
                        _floatValueEvents.ServerSideEvents.OnValueChanged.Invoke(_slider.value);
                        _floatValueEvents.ServerSideEvents.OnLocalValueChanged.Invoke(_slider.value);
                    }
                    else
                    {
                        _floatValueEvents.ClientSideEvents.OnValueChanged.Invoke(_slider.value);
                        _floatValueEvents.ClientSideEvents.OnLocalValueChanged.Invoke(_slider.value);
                    }
                    break;
                case SelectableInternalType.Toggle:
                    _boolValueEvents.CrossSideEvents.OnValueChanged.Invoke(_toggle.isOn);
                    _boolValueEvents.CrossSideEvents.OnLocalValueChanged.Invoke(_toggle.isOn);
                    if (IsServer)
                    {
                        _boolValueEvents.ServerSideEvents.OnValueChanged.Invoke(_toggle.isOn);
                        _boolValueEvents.ServerSideEvents.OnLocalValueChanged.Invoke(_toggle.isOn);
                    }
                    else
                    {
                        _boolValueEvents.ClientSideEvents.OnValueChanged.Invoke(_toggle.isOn);
                        _boolValueEvents.ClientSideEvents.OnLocalValueChanged.Invoke(_toggle.isOn);
                    }
                    break;
                default:
                    break;
            }
        }
    }
}
