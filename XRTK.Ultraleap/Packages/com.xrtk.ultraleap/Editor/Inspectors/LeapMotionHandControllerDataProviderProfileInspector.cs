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
    /// Default inspector for the <see cref="LeapMotionHandControllerDataProviderProfile"/> asset.
    /// </summary>
    [CustomEditor(typeof(LeapMotionHandControllerDataProviderProfile))]
    public class LeapMotionHandControllerDataProviderProfileInspector : BaseMixedRealityHandControllerDataProviderProfileInspector
    {
        private SerializedProperty operationMode;
        private SerializedProperty leapControllerOffset;

        private bool showLeapMotionHandTrackingSettings = true;
        private static readonly GUIContent leapMotionSettingsFoldoutHeader = new GUIContent("Leap Motion Hand Tracking Settings");

        protected override void OnEnable()
        {
            base.OnEnable();
            operationMode = serializedObject.FindProperty(nameof(operationMode));
            leapControllerOffset = serializedObject.FindProperty(nameof(leapControllerOffset));
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            showLeapMotionHandTrackingSettings = EditorGUILayoutExtensions.FoldoutWithBoldLabel(showLeapMotionHandTrackingSettings, leapMotionSettingsFoldoutHeader, true);
            if (showLeapMotionHandTrackingSettings)
            {
                EditorGUI.indentLevel++;

                EditorGUILayout.PropertyField(operationMode);

                bool deskModeDisabled = (LeapMotionOperationMode)operationMode.intValue != LeapMotionOperationMode.Desktop;
                using (new EditorGUI.DisabledScope(deskModeDisabled))
                {
                    EditorGUILayout.PropertyField(leapControllerOffset);
                }

                if (deskModeDisabled)
                {
                    EditorGUILayout.HelpBox("Controller offset is only applied when in desk mode.", MessageType.Info);
                }

                EditorGUI.indentLevel--;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
