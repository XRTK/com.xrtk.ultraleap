// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Security.Cryptography;

namespace XRTK.Definitions.Platforms
{
    /// <summary>
    /// Used by the XRTK to signal that the feature is available on the Ultraleap platform.
    /// </summary>
    [System.Runtime.InteropServices.Guid("2bea53e2-55f7-4d97-926c-d96aa392a254")]
    public class UltraleapPlatform : BasePlatform
    {
        /// <inheritdoc />
        public override bool IsAvailable => IsUltraleapServiceAccessible();

        /// <inheritdoc />
        public override bool IsBuildTargetAvailable
        {
            get
            {
#if UNITY_EDITOR
                return UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.StandaloneWindows ||
                       UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.StandaloneWindows64 ||
                       UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.StandaloneOSX ||
                       UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.StandaloneLinux ||
                       UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.StandaloneLinux64 ||
                       UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.StandaloneLinuxUniversal;
#else
                return false;
#endif
            }
        }

        private bool IsUltraleapServiceAccessible()
        {
            var serviceIsAccessible = false;

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.UseShellExecute = false;
            process.Start();

            process.StandardInput.WriteLine("sc query LeapService");
            process.StandardInput.Flush();
            process.StandardInput.Close();
            process.WaitForExit();

            var result = process.StandardOutput.ReadToEnd();
            if (!string.IsNullOrWhiteSpace(result))
            {
                serviceIsAccessible = result.Contains("4  RUNNING");
            }
#endif

            return serviceIsAccessible;
        }
    }
}
