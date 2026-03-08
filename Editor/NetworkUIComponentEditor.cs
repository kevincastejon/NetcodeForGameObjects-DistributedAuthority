using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
namespace Caskev.NetcodeForGameObjects.DistributedAuthority.Editor
{
    [CustomEditor(typeof(DistributedNetworkUIComponent))]
    [CanEditMultipleObjects]
    public class NetworkUIComponentEditor : UnityEditor.Editor
    {
        private SerializedProperty _currentState;
        private SerializedProperty _ownershipLockingOnGainPolicy;
        private SerializedProperty _statesEvents;
        private SerializedProperty _ownershipEvents;
        private SerializedProperty _debugParameters;
        private SerializedProperty _selectable;
        private SerializedProperty _isSelected;
        private SerializedProperty _disableInteractionOnLockedOwnership;
        private SerializedProperty _allowTriggeringForNonOwners;
        private SerializedProperty _intValueEvents;
        private SerializedProperty _floatValueEvents;
        private SerializedProperty _boolValueEvents;
        private SerializedProperty _stringValueEvents;
        private SerializedProperty _triggerEvents;
        private SerializedProperty _selectableType;

        private DistributedNetworkUIComponent _script; 

        private void OnEnable() 
        {
            _currentState = serializedObject.FindProperty("_currentState");
            _ownershipLockingOnGainPolicy = serializedObject.FindProperty("_ownershipLockingOnGainPolicy");
            _statesEvents = serializedObject.FindProperty("_statesEvents");
            _ownershipEvents = serializedObject.FindProperty("_ownershipEvents");
            _debugParameters = serializedObject.FindProperty("_debugParameters");
            _selectable = serializedObject.FindProperty("_selectable");
            _isSelected = serializedObject.FindProperty("_isSelected");
            _disableInteractionOnLockedOwnership = serializedObject.FindProperty("_disableInteractionOnLockedOwnership");
            _allowTriggeringForNonOwners = serializedObject.FindProperty("_allowTriggeringForNonOwners");
            _intValueEvents = serializedObject.FindProperty("_intValueEvents");
            _floatValueEvents = serializedObject.FindProperty("_floatValueEvents");
            _boolValueEvents = serializedObject.FindProperty("_boolValueEvents");
            _stringValueEvents = serializedObject.FindProperty("_stringValueEvents");
            _triggerEvents = serializedObject.FindProperty("_triggerEvents");
            _selectableType = serializedObject.FindProperty("_selectableType");

            _script = (DistributedNetworkUIComponent)target;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            UnityEngine.Object oldSelectable = _selectable.objectReferenceValue;
            EditorGUILayout.LabelField("UI Settings");
            EditorGUI.indentLevel++;
            EditorGUI.BeginChangeCheck();
            EditorGUI.BeginDisabledGroup(EditorApplication.isPlaying);
            EditorGUILayout.PropertyField(_selectable);
            EditorGUI.EndDisabledGroup();
            bool isSelectableNull = _selectable.objectReferenceValue == null;
            if (EditorGUI.EndChangeCheck() && !isSelectableNull)
            {
                bool goodType = TryGetSelectableType(out DistributedNetworkUIComponent.SelectableInternalType selectableType);
                if (!goodType)
                {
                    _selectable.objectReferenceValue = oldSelectable;
                    Debug.LogWarning("This Selectable UI component is not supported!");
                }
                else
                {
                    _selectableType.enumValueIndex = (int)selectableType;
                }
            }
            if (!isSelectableNull)
            {
                DistributedNetworkUIComponent.SelectableInternalType selType = (DistributedNetworkUIComponent.SelectableInternalType)_selectableType.enumValueIndex;
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.PropertyField(_isSelected);
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.PropertyField(_disableInteractionOnLockedOwnership);
                if (targets.Length == 1)
                {
                    switch (selType)
                    {
                        case DistributedNetworkUIComponent.SelectableInternalType.TMP_Dropdown:
                            EditorGUILayout.PropertyField(_intValueEvents);
                            break;
                        case DistributedNetworkUIComponent.SelectableInternalType.TMP_InputField:
                            EditorGUILayout.PropertyField(_stringValueEvents);
                            break;
                        case DistributedNetworkUIComponent.SelectableInternalType.Scrollbar:
                            EditorGUILayout.PropertyField(_floatValueEvents);
                            break;
                        case DistributedNetworkUIComponent.SelectableInternalType.Slider:
                            EditorGUILayout.PropertyField(_floatValueEvents);
                            break;
                        case DistributedNetworkUIComponent.SelectableInternalType.Toggle:
                            EditorGUILayout.PropertyField(_boolValueEvents);
                            break;
                        case DistributedNetworkUIComponent.SelectableInternalType.Button:
                            EditorGUILayout.PropertyField(_allowTriggeringForNonOwners);
                            EditorGUILayout.PropertyField(_triggerEvents);
                            break;
                        default:
                            break;
                    }
                }
            }
            EditorGUI.indentLevel--;
            EditorGUILayout.LabelField("Distributed Authority Settings");
            EditorGUI.indentLevel++;
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(_currentState);
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.PropertyField(_ownershipLockingOnGainPolicy);
            EditorGUILayout.PropertyField(_statesEvents);
            EditorGUILayout.PropertyField(_ownershipEvents);
            EditorGUILayout.PropertyField(_debugParameters);
            EditorGUI.indentLevel--;

            serializedObject.ApplyModifiedProperties();
        }

        private bool TryGetSelectableType(out DistributedNetworkUIComponent.SelectableInternalType selectableType)
        {
            Selectable selectable = (Selectable)_selectable.objectReferenceValue;
            if (selectable is TMP_Dropdown)
            {
                selectableType = DistributedNetworkUIComponent.SelectableInternalType.TMP_Dropdown;
                return true;
            }
            else if (selectable is TMP_InputField)
            {
                selectableType = DistributedNetworkUIComponent.SelectableInternalType.TMP_InputField;
                return true;
            }
            else if (selectable is Scrollbar)
            {
                selectableType = DistributedNetworkUIComponent.SelectableInternalType.Scrollbar;
                return true;
            }
            else if (selectable is Slider)
            {
                selectableType = DistributedNetworkUIComponent.SelectableInternalType.Slider;
                return true;
            }
            else if (selectable is Toggle)
            {
                selectableType = DistributedNetworkUIComponent.SelectableInternalType.Toggle;
                return true;
            }
            else if (selectable is Button)
            {
                selectableType = DistributedNetworkUIComponent.SelectableInternalType.Button;
                return true;
            }
            else
            {
                selectableType = default;
                return false;
            }
        }
    }
}
