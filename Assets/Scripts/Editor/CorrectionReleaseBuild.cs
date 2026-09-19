using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
namespace Racer.Editor
{
    public static class CorrectionReleaseBuild
    {
        public const string Version="0.6.1-review2";
        public static void Candidate()=>Build("Temp/CorrectionPlayer",false);
        public static void Candidate2()=>Build("Temp/CorrectionPlayer2",false);
        public static void Candidate3()=>Build("Temp/CorrectionPlayer3",false);
        public static void Candidate4()=>Build("Temp/CorrectionPlayer4",false);
        public static void Candidate5()=>Build("Temp/CorrectionPlayer5",false);
        public static void Candidate6()=>Build("Temp/CorrectionPlayer6",false);
        public static void Candidate7()=>Build("Temp/CorrectionPlayer7",false);
        public static void Release()=>Build("Builds/Racer-"+Version+"-Windows",true);
        static void Build(string output,bool release)
        {
            if(Application.isPlaying||UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)throw new InvalidOperationException("Saved scene/edit mode required");
            string source=release?File.ReadAllText("Temp/correction-source-commit.txt").Trim():"UNCOMMITTED VALIDATION CANDIDATE";
            if(release&&(source.Length!=40||!source.All(Uri.IsHexDigit)))throw new InvalidOperationException("Missing source commit");
            if(PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.Standalone).Contains("ENABLE_RUNTIME_PIPELINE"))throw new InvalidOperationException("Runtime pipeline must be excluded");
            EditorUserBuildSettings.development=EditorUserBuildSettings.allowDebugging=EditorUserBuildSettings.connectProfiler=false;
            Directory.CreateDirectory(output);
            var result=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{StreetLoopBuilder.ScenePath},locationPathName=output+"/Racer.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.None});
            File.WriteAllText("Docs/CR041-045/"+(release?"release":"candidate")+"-build.txt",$"{result.summary.result}; errors={result.summary.totalErrors}; warnings={result.summary.totalWarnings}; time={result.summary.totalTime}\n"+string.Join("\n",result.steps.SelectMany(s=>s.messages).Where(m=>m.type==LogType.Error||m.type==LogType.Warning).Select(m=>m.content)));
            if(result.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new InvalidOperationException("Build failed");
            File.WriteAllText(output+"/VERSION.txt",$"Racer {Version}\nWindows x64 integrated playtest correction\nSource commit: {source}\nUnity {Application.unityVersion}\nRules: street-v6-flat5; development=false; Runtime Pipeline excluded\n");
        }
    }
}
