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
        private SerializedProperty leapControllerOffset;

        private bool showUltraleapHandTrackingSettings = true;
        private static readonly GUIContent ultraleapSettingsFoldoutHeader = new GUIContent("Ultraleap Hand Tracking Settings");

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

            showUltraleapHandTrackingSettings = EditorGUILayoutExtensions.FoldoutWithBoldLabel(showUltraleapHandTrackingSettings, ultraleapSettingsFoldoutHeader, true);
            if (showUltraleapHandTrackingSettings)
            {
                EditorGUI.indentLevel++;

                EditorGUILayout.PropertyField(operationMode);

                bool deskModeDisabled = (UltraleapOperationMode)operationMode.intValue != UltraleapOperationMode.Desktop;
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
