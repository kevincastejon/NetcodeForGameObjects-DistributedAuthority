using System.Collections;
using System.Collections.Generic;
using UnityEditor;
namespace Caskev.Samples.NetcodeForGameObjects.DistributedAuthority.DistributedNetworkRigidbody_3D.Editor
{
    [CustomEditor(typeof(DistributedNetworkRigidbody))]
    public class DistributedNetworkRigidbodyEditor : UnityEditor.Editor
    {
        private SerializedProperty _currentState;
        private SerializedProperty _ownershipLockingOnGainPolicy;
        private SerializedProperty _statesEvents;
        private SerializedProperty _ownershipEvents;
        private SerializedProperty _debugParameters;
        private SerializedProperty _automaticallyDeclineOnAsleep;
        private SerializedProperty _onAsleep;
        private SerializedProperty _onAwake;

        private DistributedNetworkRigidbody _script;

        private void OnEnable()
        {
            _currentState = serializedObject.FindProperty("_currentState");
            _ownershipLockingOnGainPolicy = serializedObject.FindProperty("_ownershipLockingOnGainPolicy");
            _statesEvents = serializedObject.FindProperty("_statesEvents");
            _ownershipEvents = serializedObject.FindProperty("_ownershipEvents");
            _debugParameters = serializedObject.FindProperty("_debugParameters");
            _automaticallyDeclineOnAsleep = serializedObject.FindProperty("_automaticallyDeclineOnAsleep");
            _onAsleep = serializedObject.FindProperty("_onAsleep");
            _onAwake = serializedObject.FindProperty("_onAwake");

            _script = (DistributedNetworkRigidbody)target;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(_currentState);
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.PropertyField(_ownershipLockingOnGainPolicy);
            EditorGUILayout.PropertyField(_statesEvents);
            EditorGUILayout.PropertyField(_ownershipEvents);
            EditorGUILayout.PropertyField(_debugParameters);
            EditorGUILayout.PropertyField(_automaticallyDeclineOnAsleep);
            EditorGUILayout.PropertyField(_onAsleep);
            EditorGUILayout.PropertyField(_onAwake);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
