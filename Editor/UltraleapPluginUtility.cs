// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using XRTK.Editor.Utilities;
using XRTK.Extensions;

namespace XRTK.Ultraleap.Editor
{
    [InitializeOnLoad]
    public static class UltraleapPluginUtility
    {
        private const string GIT_ROOT = "../../../../";
        private const string LEAP_API = "LeapC.dll";
        private const string NATIVE_ROOT_PATH = "/Submodules/UnityModules/Assets/Plugins/LeapMotion/Core";
        private static readonly string RootPath = PathFinderUtility.ResolvePath<IPathFinder>(typeof(UltraleapPathFinder));
        private static readonly string PluginPath = Path.GetFullPath($"{RootPath}/Runtime/Plugins");

        private static string NativeRootPath
        {
            get
            {
                var path = Path.GetFullPath($"{RootPath}{GIT_ROOT}{NATIVE_ROOT_PATH}");

                if (!Directory.Exists(path))
                {
                    path = Path.GetFullPath($"{RootPath}{GIT_ROOT}/Submodules/Ultraleap{NATIVE_ROOT_PATH}");
                }

                return path;
            }
        }

        private static string NativePluginPath => Path.GetFullPath($"{NativeRootPath}/Plugins");

        static UltraleapPluginUtility()
        {
            if (!Directory.Exists(NativeRootPath))
            {
                return;
            }

            if (EditorPreferences.Get($"Reimport_{nameof(UltraleapPluginUtility)}", true))
            {
                EditorPreferences.Set($"Reimport_{nameof(UltraleapPluginUtility)}", false);
                DeleteSupportLibraries();
            }

            if (Directory.Exists(PluginPath))
            {
                if (EditorPreferences.Get($"SetMeta_{nameof(UltraleapPluginUtility)}", true))
                {
                    SetPluginMeta();
                    EditorPreferences.Set($"SetMeta_{nameof(UltraleapPluginUtility)}", false);
                }
            }
            else
            {
                CopySupportLibraries();
                EditorPreferences.Set($"SetMeta_{nameof(UltraleapPluginUtility)}", true);
                EditorApplication.delayCall += () => AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            }
        }

        private static void DeleteSupportLibraries()
        {
            if (Directory.Exists(PluginPath))
            {
                var files = Directory.GetFiles(PluginPath, "*", SearchOption.AllDirectories);

                foreach (var file in files)
                {
                    File.Delete(file);
                }

                var directories = Directory.GetDirectories(PluginPath, "*", SearchOption.AllDirectories);

                foreach (var directory in directories)
                {
                    Directory.Delete(directory);
                }

                Directory.Delete(PluginPath);
            }
        }

        private static void CopySupportLibraries()
        {
            Directory.CreateDirectory(PluginPath);

            var directories = Directory.GetDirectories(NativePluginPath, "*", SearchOption.AllDirectories);

            foreach (var directory in directories)
            {
                Directory.CreateDirectory(directory.Replace(NativePluginPath.BackSlashes(), PluginPath.BackSlashes()));
            }

            var files = Directory.GetFiles(NativePluginPath, "*.cs", SearchOption.AllDirectories).ToList();
            files.AddRange(Directory.GetFiles(NativePluginPath, "*.dll", SearchOption.AllDirectories));

            foreach (var file in files)
            {
                File.Copy(file, file.BackSlashes().Replace(NativePluginPath.BackSlashes(), PluginPath.BackSlashes()));
            }

            File.Copy($"{NativeRootPath}/readme.txt", $"{PluginPath}/license.txt");
            File.Copy($"{NativeRootPath}/Version.txt", $"{PluginPath}/Version.txt");
            File.Copy($"{NativePluginPath}/LeapCSharp/LeapMotion.LeapCSharp.asmdef", $"{PluginPath}/LeapCSharp/LeapMotion.LeapCSharp.asmdef");
            File.Copy($"{NativePluginPath}/LeapCSharp/LeapMotion.LeapCSharp.asmdef.meta", $"{PluginPath}/LeapCSharp/LeapMotion.LeapCSharp.asmdef.meta");
        }

        private static void SetPluginMeta()
        {
            var rootPluginPath = $"{RootPath}/Runtime/Plugins";

            var x86Path = $"{rootPluginPath}/x86/{LEAP_API}";
            Debug.Assert(File.Exists(x86Path), $"Library not found at {x86Path}");
            var x86Importer = AssetImporter.GetAtPath(x86Path) as PluginImporter;
            Debug.Assert(x86Importer != null, $"Failed to load {x86Path}");
            x86Importer.ClearSettings();
            x86Importer.SetCompatibleWithAnyPlatform(false);
            x86Importer.SetCompatibleWithEditor(true);
            x86Importer.SetEditorData("CPU", "x86");
            x86Importer.SetPlatformData(BuildTarget.NoTarget, "CPU", "x86");
            x86Importer.SetCompatibleWithPlatform(BuildTarget.StandaloneWindows, true);
            x86Importer.SetPlatformData(BuildTarget.StandaloneWindows, "CPU", "x86");
            x86Importer.SetCompatibleWithPlatform(BuildTarget.WSAPlayer, true);
            x86Importer.SetPlatformData(BuildTarget.WSAPlayer, "CPU", "X86");
            x86Importer.SaveAndReimport();

            var x64Path = $"{rootPluginPath}/x86_64/{LEAP_API}";
            Debug.Assert(File.Exists(x64Path), $"Library not found at {x64Path}");
            var x64Importer = AssetImporter.GetAtPath(x64Path) as PluginImporter;
            Debug.Assert(x64Importer != null, $"Failed to load {x64Path}");
            x64Importer.ClearSettings();
            x64Importer.SetCompatibleWithAnyPlatform(false);
            x64Importer.SetCompatibleWithEditor(true);
            x64Importer.SetEditorData("CPU", "x86_64");
            x64Importer.SetPlatformData(BuildTarget.NoTarget, "CPU", "x86_64");
            x64Importer.SetCompatibleWithPlatform(BuildTarget.StandaloneWindows64, true);
            x64Importer.SetPlatformData(BuildTarget.StandaloneWindows64, "CPU", "x86_64");
            x64Importer.SetCompatibleWithPlatform(BuildTarget.WSAPlayer, true);
            x64Importer.SetPlatformData(BuildTarget.WSAPlayer, "CPU", "X64");
            x64Importer.SaveAndReimport();
        }

        [MenuItem("Mixed Reality Toolkit/Tools/Ultraleap/Reimport Plugins", true)]
        private static bool UpdatePluginValidation() => Directory.Exists(NativeRootPath);

        [MenuItem("Mixed Reality Toolkit/Tools/Ultraleap/Reimport Plugins", false)]
        private static void UpdatePlugins()
        {
            if (EditorUtility.DisplayDialog("Attention!",
                "In order to reimport the Ultraleap plugins, we'll need to restart the editor, is this ok?", "Restart", "Cancel"))
            {
                EditorPreferences.Set($"Reimport_{nameof(UltraleapPluginUtility)}", true);
                EditorApplication.OpenProject(Directory.GetParent(Application.dataPath).FullName);
            }
        }
    }
}
