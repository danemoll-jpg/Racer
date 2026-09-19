using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
namespace Racer.Editor
{
    public static class CombinedReleaseBuild
    {
        public const string Version="0.6.2-review1";
        public static void Build(string output=null)
        {
            if(Application.isPlaying||UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)throw new InvalidOperationException("Saved scene/edit mode required");
            if(PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.Standalone).Contains("ENABLE_RUNTIME_PIPELINE"))throw new InvalidOperationException("Runtime pipeline must be excluded");
            PlayerSettings.bundleVersion=Version;
            EditorUserBuildSettings.development=EditorUserBuildSettings.allowDebugging=EditorUserBuildSettings.connectProfiler=false;
            output??="Builds/Racer-"+Version+"-Windows";Directory.CreateDirectory(output);Directory.CreateDirectory("Docs/CR046-049");
            var result=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{StreetLoopBuilder.ScenePath},locationPathName=output+"/Racer.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.None});
            File.WriteAllText("Docs/CR046-049/build.txt",$"{result.summary.result}; errors={result.summary.totalErrors}; warnings={result.summary.totalWarnings}; time={result.summary.totalTime}\n"+string.Join("\n",result.steps.SelectMany(s=>s.messages).Where(m=>m.type==LogType.Error||m.type==LogType.Warning).Select(m=>m.content)));
            if(result.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new InvalidOperationException("Build failed");
            File.WriteAllText(output+"/VERSION.txt",$"Racer {Version}\nWindows x64 combined review / BUG-004, BUG-008, CR-046–049\nRules: street-v7-entitlement; development=false; Runtime Pipeline excluded\nSource tree: SOURCE-SHA256.txt (final completion commit in repository)\nUnity {Application.unityVersion}\n");
            File.Copy("Docs/CR046-049/RADIO.md",output+"/RADIO.md",true);
            string licenses=output+"/Licenses";Directory.CreateDirectory(licenses);
            foreach(var p in Directory.GetFiles("Assets/Plugins/LocalRadio","*.txt"))File.Copy(p,licenses+"/"+Path.GetFileName(p),true);
        }
    }
}
