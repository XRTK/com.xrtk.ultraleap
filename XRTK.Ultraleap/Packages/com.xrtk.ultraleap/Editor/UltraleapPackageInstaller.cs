// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.IO;
using UnityEditor;
using XRTK.Editor;
using XRTK.Editor.Utilities;
using XRTK.Extensions;

namespace XRTK.Ultraleap.Editor
{
    [InitializeOnLoad]
    internal static class UltraleapPackageInstaller
    {
        private static readonly string DefaultPath = $"{MixedRealityPreferences.ProfileGenerationPath}Ultraleap";
        private static readonly string HiddenPath = Path.GetFullPath($"{PathFinderUtility.ResolvePath<IPathFinder>(typeof(UltraleapPathFinder)).BackSlashes()}\\{MixedRealityPreferences.HIDDEN_PROFILES_PATH}");

        static UltraleapPackageInstaller()
        {
            if (!EditorPreferences.Get($"{nameof(UltraleapPackageInstaller)}", false))
            {
                EditorPreferences.Set($"{nameof(UltraleapPackageInstaller)}", PackageInstaller.TryInstallAssets(HiddenPath, DefaultPath));
            }

            EditorApplication.delayCall += CheckPackage;
        }

        [MenuItem("Mixed Reality Toolkit/Packages/Install Ultraleap Package Assets...", true)]
        private static bool ImportPackageAssetsValidation()
        {
            return !Directory.Exists($"{DefaultPath}\\Profiles");
        }

        [MenuItem("Mixed Reality Toolkit/Packages/Install Ultraleap Package Assets...")]
        private static void ImportPackageAssets()
        {
            EditorPreferences.Set($"{nameof(UltraleapPackageInstaller)}.Profiles", false);
            EditorApplication.delayCall += CheckPackage;
        }

        private static void CheckPackage()
        {
            if (!EditorPreferences.Get($"{nameof(UltraleapPackageInstaller)}.Profiles", false))
            {
                EditorPreferences.Set($"{nameof(UltraleapPackageInstaller)}.Profiles", PackageInstaller.TryInstallAssets(HiddenPath, $"{DefaultPath}\\Profiles"));
            }
        }
    }
}
