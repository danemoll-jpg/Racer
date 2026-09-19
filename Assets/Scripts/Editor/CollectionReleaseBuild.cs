using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
namespace Racer.Editor
{
    public static class CollectionReleaseBuild
    {
        public const string Version="0.6.3-review1";
        public static void Build()
        {
            if(Application.isPlaying||UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)throw new InvalidOperationException("Saved edit mode required");
            if(PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.Standalone).Contains("ENABLE_RUNTIME_PIPELINE"))throw new InvalidOperationException("Runtime pipeline must be excluded");
            PlayerSettings.bundleVersion=Version;EditorUserBuildSettings.development=EditorUserBuildSettings.allowDebugging=EditorUserBuildSettings.connectProfiler=false;
            string output="Builds/Racer-"+Version+"-Windows";Directory.CreateDirectory(output);
            var result=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{StreetLoopBuilder.ScenePath},locationPathName=output+"/Racer.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.None});
            File.WriteAllText("Docs/CR050-053/build.txt",$"{result.summary.result}; errors={result.summary.totalErrors}; warnings={result.summary.totalWarnings}; time={result.summary.totalTime}\n"+string.Join("\n",result.steps.SelectMany(s=>s.messages).Where(m=>m.type==LogType.Error||m.type==LogType.Warning).Select(m=>m.content)));
            if(result.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new InvalidOperationException("Build failed");
            File.WriteAllText(output+"/VERSION.txt",$"Racer {Version}\nWindows x64 review / CR-050–053\nRules: street-v8-landings; difficulty unchanged; development=false; Runtime Pipeline excluded\nSource tree: SOURCE-SHA256.txt; completion commit recorded after validation\nUnity {Application.unityVersion}\n");
            Directory.CreateDirectory(output+"/Music");
            File.Copy("Docs/CR050-053/RADIO.md",output+"/RADIO.md",true);
            string licenses=output+"/Licenses";Directory.CreateDirectory(licenses);
            foreach(var p in Directory.GetFiles("Assets/Plugins/LocalRadio","*.txt"))File.Copy(p,licenses+"/"+Path.GetFileName(p),true);
        }
    }
}
