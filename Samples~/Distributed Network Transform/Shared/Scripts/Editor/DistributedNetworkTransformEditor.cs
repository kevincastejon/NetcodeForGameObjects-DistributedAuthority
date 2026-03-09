using System.Collections;
using System.Collections.Generic;
using UnityEditor;
namespace Caskev.Samples.NetcodeForGameObjects.DistributedAuthority.DistributedNetworkTransform_Shared.Editor
{
    [CustomEditor(typeof(DistributedNetworkTransform))]
    public class DistributedNetworkTransformEditor : UnityEditor.Editor
    {
        private SerializedProperty _currentState;
        private SerializedProperty _ownershipLockingOnGainPolicy;
        private SerializedProperty _statesEvents;
        private SerializedProperty _ownershipEvents;
        private SerializedProperty _debugParameters;

        private DistributedNetworkTransform _script;

        private void OnEnable()
        {
            _currentState = serializedObject.FindProperty("_currentState");
            _ownershipLockingOnGainPolicy = serializedObject.FindProperty("_ownershipLockingOnGainPolicy");
            _statesEvents = serializedObject.FindProperty("_statesEvents");
            _ownershipEvents = serializedObject.FindProperty("_ownershipEvents");
            _debugParameters = serializedObject.FindProperty("_debugParameters");

            _script = (DistributedNetworkTransform)target;
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

            serializedObject.ApplyModifiedProperties();
        }
    }
}
