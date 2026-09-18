using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Racer.Editor
{
    public static class WoodlandReleaseBuild
    {
        public const string Version="0.6.0-review1";
        public const string Output="Builds/Racer-"+Version+"-Windows";
        public static void Candidate(string output="Temp/WoodlandPlayer")=>BuildTo(output,false);
        public static void Release()=>BuildTo(Output,true);
        static void BuildTo(string output,bool release)
        {
            if(Application.isPlaying || SceneManager.GetActiveScene().isDirty) throw new InvalidOperationException("Exit Play and save scene first.");
            string source=release?File.ReadAllText("Temp/release-source-commit.txt").Trim():"Uncommitted validation candidate; not a distribution package";
            if(release && (source.Length!=40 || !source.All(Uri.IsHexDigit))) throw new InvalidOperationException("Missing source commit");
            if(PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.Standalone).Split(';').Contains("ENABLE_RUNTIME_PIPELINE")) throw new InvalidOperationException("Runtime Pipeline must be excluded");
            EditorUserBuildSettings.development=EditorUserBuildSettings.allowDebugging=EditorUserBuildSettings.connectProfiler=EditorUserBuildSettings.waitForPlayerConnection=false;
            Directory.CreateDirectory(output);
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{StreetLoopBuilder.ScenePath},locationPathName=output+"/Racer.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.None});
            File.WriteAllText("Docs/CR034-039/"+(release?"release":"candidate")+"-build.txt",$"{report.summary.result}; errors={report.summary.totalErrors}; warnings={report.summary.totalWarnings}; bytes={report.summary.totalSize}; duration={report.summary.totalTime}\n"+string.Join("\n",report.steps.SelectMany(s=>s.messages).Where(m=>m.type==LogType.Error || m.type==LogType.Warning).Select(m=>m.content)));
            if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded) throw new InvalidOperationException("Build failed");
            File.WriteAllText(output+"/VERSION.txt",$"Racer {Version}\nWindows x64 woodland arcade-racing review\nSource commit: {source}\nUnity {Application.unityVersion}\nDevelopment: false; script debugging: false; profiler: false; Runtime Pipeline excluded\n");
        }
    }
}
