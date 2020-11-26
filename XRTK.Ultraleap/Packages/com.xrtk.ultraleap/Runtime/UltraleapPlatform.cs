// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace XRTK.Definitions.Platforms
{
    /// <summary>
    /// Used by the XRTK to signal that the feature is available on the Ultraleap platform.
    /// </summary>
    [System.Runtime.InteropServices.Guid("2bea53e2-55f7-4d97-926c-d96aa392a254")]
    public class UltraleapPlatform : BasePlatform
    {
        /// <inheritdoc />
        public override bool IsAvailable => IsUltraleapServiceAvailable();

        /// <inheritdoc />
        public override bool IsBuildTargetAvailable
        {
            get
            {
#if UNITY_EDITOR
                return UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.StandaloneWindows ||
                       UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.StandaloneWindows64 ||
                       UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.StandaloneOSX ||
                       UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.StandaloneLinux64;
#else
                return false;
#endif
            }
        }

        private bool IsUltraleapServiceAvailable()
        {
            var serviceIsAccessible = false;

            return serviceIsAccessible;
        }
    }
}
