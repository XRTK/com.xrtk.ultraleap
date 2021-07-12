// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEditor;
using UnityEngine;
using XRTK.Editor.Extensions;
using XRTK.Editor.Profiles.InputSystem.Controllers;
using XRTK.Ultraleap.Definitions;
using XRTK.Ultraleap.Profiles;

namespace XRTK.Ultraleap.Editor.Inspectors
{
    /// <summary>
    /// Default inspector for the <see cref="UltraleapHandControllerDataProviderProfile"/> asset.
    /// </summary>
    [CustomEditor(typeof(UltraleapHandControllerDataProviderProfile))]
    public class UltraleapHandControllerDataProviderProfileInspector : BaseMixedRealityHandControllerDataProviderProfileInspector
    {
        private SerializedProperty operationMode;
        private SerializedProperty maxReconnectionAttempts;
        private SerializedProperty reconnectionInterval;
        private SerializedProperty frameOptimizationMode;
        private SerializedProperty leapControllerOffset;
        private SerializedProperty deviceOffsetMode;
        private SerializedProperty deviceOffsetYAxis;
        private SerializedProperty deviceOffsetZAxis;
        private SerializedProperty deviceTiltXAxis;

        private bool showUltraleapHandTrackingSettings = true;
        private static readonly GUIContent ultraleapSettingsFoldoutHeader = new GUIContent("Ultraleap Hand Tracking Settings");

        protected override void OnEnable()
        {
            base.OnEnable();
            operationMode = serializedObject.FindProperty(nameof(operationMode));
            maxReconnectionAttempts = serializedObject.FindProperty(nameof(maxReconnectionAttempts));
            reconnectionInterval = serializedObject.FindProperty(nameof(reconnectionInterval));
            frameOptimizationMode = serializedObject.FindProperty(nameof(frameOptimizationMode));
            leapControllerOffset = serializedObject.FindProperty(nameof(leapControllerOffset));
            deviceOffsetMode = serializedObject.FindProperty(nameof(deviceOffsetMode));
            deviceOffsetYAxis = serializedObject.FindProperty(nameof(deviceOffsetYAxis));
            deviceOffsetZAxis = serializedObject.FindProperty(nameof(deviceOffsetZAxis));
            deviceTiltXAxis = serializedObject.FindProperty(nameof(deviceTiltXAxis));
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            EditorGUILayout.Space();
            showUltraleapHandTrackingSettings = EditorGUILayoutExtensions.FoldoutWithBoldLabel(showUltraleapHandTrackingSettings, ultraleapSettingsFoldoutHeader, true);
            if (showUltraleapHandTrackingSettings)
            {
                EditorGUI.indentLevel++;

                EditorGUILayout.PropertyField(operationMode);
                EditorGUILayout.PropertyField(maxReconnectionAttempts);
                EditorGUILayout.PropertyField(reconnectionInterval);
                EditorGUILayout.PropertyField(frameOptimizationMode);

                var configuredOperationMode = (UltraleapOperationMode)operationMode.intValue;
                switch (configuredOperationMode)
                {
                    case UltraleapOperationMode.Desktop:
                        EditorGUILayout.PropertyField(leapControllerOffset);
                        break;
                    case UltraleapOperationMode.HeadsetMounted:
                        EditorGUILayout.PropertyField(deviceOffsetMode);
                        var configuredOffsetMode = (UltraleapDeviceOffsetMode)deviceOffsetMode.intValue;
                        switch (configuredOffsetMode)
                        {
                            case UltraleapDeviceOffsetMode.Manual:
                                EditorGUILayout.PropertyField(deviceOffsetYAxis);
                                EditorGUILayout.PropertyField(deviceOffsetZAxis);
                                EditorGUILayout.PropertyField(deviceTiltXAxis);
                                break;
                            case UltraleapDeviceOffsetMode.Default:
                            default:
                                break;
                        }
                        break;
                }

                EditorGUI.indentLevel--;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
