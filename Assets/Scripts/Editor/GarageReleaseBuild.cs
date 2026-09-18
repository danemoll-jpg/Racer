using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Racer.Editor
{
    public static class GarageReleaseBuild
    {
        public const string Version = "0.4.0-review1";
        public const string Output = "Builds/Racer-" + Version + "-Windows";
        public static void Build()
        {
            if (Application.isPlaying || SceneManager.GetActiveScene().isDirty)
                throw new InvalidOperationException("Exit Play mode and save scene first.");
            string commit = File.ReadAllText("Temp/release-source-commit.txt").Trim();
            if (commit.Length != 40 || !commit.All(Uri.IsHexDigit))
                throw new InvalidOperationException("Missing source commit.");
            var defines = PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.Standalone);
            if (defines.Split(';').Contains("ENABLE_RUNTIME_PIPELINE"))
                throw new InvalidOperationException("Runtime Pipeline must not be enabled in the solo release.");
            EditorUserBuildSettings.development = false;
            EditorUserBuildSettings.allowDebugging = false;
            EditorUserBuildSettings.connectProfiler = false;
            EditorUserBuildSettings.waitForPlayerConnection = false;
            Directory.CreateDirectory(Output);
            var r = BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes = new[]{StreetLoopBuilder.ScenePath}, locationPathName = Output + "/Racer.exe", target = BuildTarget.StandaloneWindows64, options = BuildOptions.None});
            File.WriteAllText("Docs/CR026-027/build.txt", $"{r.summary.result}; errors={r.summary.totalErrors}; warnings={r.summary.totalWarnings}; bytes={r.summary.totalSize}; duration={r.summary.totalTime}\n" + string.Join("\n", r.steps.SelectMany(s => s.messages).Where(m => m.type == LogType.Error || m.type == LogType.Warning).Select(m => m.content)));
            if (r.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
                throw new InvalidOperationException("Build failed.");
            File.WriteAllText(Output + "/VERSION.txt", $"Racer {Version}\nWindows x64 single-player garage and difficulty review\nSource commit: {commit}\nUnity {Application.unityVersion}\nDevelopment: false; script debugging: false; profiler connection: false; Runtime Pipeline: excluded\n");
        }
    }
}
