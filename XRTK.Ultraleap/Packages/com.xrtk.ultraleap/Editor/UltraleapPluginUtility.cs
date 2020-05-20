﻿// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using XRTK.Extensions;
using XRTK.Utilities.Editor;

namespace XRTK.Ultraleap.Editor
{
    [InitializeOnLoad]
    public static class UltraleapPluginUtility
    {
        private const string GIT_ROOT = "../../../../";
        private const string NATIVE_ROOT_PATH = "/Submodules/UnityModules/Assets/Plugins/LeapMotion/Core/Plugins";
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

        static UltraleapPluginUtility()
        {
            if (!Directory.Exists(PluginPath) || EditorPreferences.Get($"Reimport_{nameof(UltraleapPluginUtility)}", false))
            {
                EditorPreferences.Set($"Reimport_{nameof(UltraleapPluginUtility)}", false);

                Debug.Assert(Directory.Exists(NativeRootPath), "Submodule not found! Did you make sure to recursively checkout this branch?");

                if (Directory.Exists(PluginPath))
                {
                    var oldFiles = Directory.GetFiles(PluginPath, "*", SearchOption.AllDirectories).ToList();

                    foreach (var oldFile in oldFiles)
                    {
                        File.Delete(oldFile);
                    }

                    var oldDirectories = Directory.GetDirectories(PluginPath, "*", SearchOption.AllDirectories);

                    foreach (var oldDirectory in oldDirectories)
                    {
                        Directory.Delete(oldDirectory);
                    }

                    Directory.Delete(PluginPath);
                }

                Directory.CreateDirectory(PluginPath);

                var directories = Directory.GetDirectories(NativeRootPath, "*", SearchOption.AllDirectories);

                foreach (var directory in directories)
                {
                    Directory.CreateDirectory(directory.Replace(NativeRootPath.ToForwardSlashes(), PluginPath.ToForwardSlashes()));
                }

                var files = Directory.GetFiles(NativeRootPath, "*.cs", SearchOption.AllDirectories).ToList();
                files.AddRange(Directory.GetFiles(NativeRootPath, "*.dll", SearchOption.AllDirectories));

                foreach (var file in files)
                {
                    File.Copy(file, file.ToForwardSlashes().Replace(NativeRootPath.ToForwardSlashes(), PluginPath.ToForwardSlashes()));
                }

                EditorApplication.delayCall += () => AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            }
        }

        [MenuItem("Mixed Reality Toolkit/Tools/Ultraleap/Reimport Plugins", true)]
        private static bool UpdatePluginValidation() => Directory.Exists(NativeRootPath);

        [MenuItem("Mixed Reality Toolkit/Tools/Ultraleap/Reimport Plugins", false)]
        private static void UpdatePlugins()
        {
            if (EditorUtility.DisplayDialog("Attention!",
                "In order to reimport the SteamVR plugins, we'll need to restart the editor, is this ok?", "Restart", "Cancel"))
            {
                EditorPreferences.Set($"Reimport_{nameof(UltraleapPluginUtility)}", true);
                EditorApplication.OpenProject(Directory.GetParent(Application.dataPath).FullName);
            }
        }
    }
}
