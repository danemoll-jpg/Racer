using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace Racer.Editor
{
    public static class Review6466Release
    {
        public const string Evidence="Docs/CR064-066", Version="0.10.0-review1";
        static double buildAt;
        public static void QueueBuild(){Directory.CreateDirectory(Evidence);buildAt=EditorApplication.timeSinceStartup+2;EditorApplication.update-=Tick;EditorApplication.update+=Tick;}
        static void Tick(){if(EditorApplication.timeSinceStartup<buildAt)return;EditorApplication.update-=Tick;try{Build();File.WriteAllText(Evidence+"/build-done.txt","Complete");}catch(Exception e){File.WriteAllText(Evidence+"/build-done.txt",e.ToString());}}
        public static void Build()
        {
            if(Application.isPlaying||SceneManager.GetActiveScene().isDirty)throw new Exception("Saved edit mode required");
            PlayerSettings.bundleVersion=Version;EditorUserBuildSettings.development=EditorUserBuildSettings.allowDebugging=EditorUserBuildSettings.connectProfiler=false;
            string output="Builds/Racer-"+Version+"-Windows";Directory.CreateDirectory(output);
            var result=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{StreetLoopBuilder.ScenePath,LakeCourseBuild.ScenePath},locationPathName=output+"/Racer.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.None});
            File.WriteAllText(Evidence+"/build.txt",$"{result.summary.result}; errors={result.summary.totalErrors}; warnings={result.summary.totalWarnings}; time={result.summary.totalTime}\n"+string.Join("\n",result.steps.SelectMany(s=>s.messages).Where(m=>m.type==LogType.Error||m.type==LogType.Warning).Select(m=>m.content)));
            if(result.summary.result!=BuildResult.Succeeded)throw new Exception("Build failed");
            File.WriteAllText(output+"/VERSION.txt",$"Racer {Version}\nWindows x64 review / CR-064 through CR-066\nRules: street-v9-life / lake-v3-shallows; development=false\nSource tree: SOURCE-SHA256.txt; completion commit recorded after validation\nUnity {Application.unityVersion}\n");
            Directory.CreateDirectory(output+"/Licenses");foreach(var notice in Directory.GetFiles("Assets/Plugins/LocalRadio","*.txt"))File.Copy(notice,output+"/Licenses/"+Path.GetFileName(notice),true);
            File.Copy("Assets/Audio/Wildlife/LICENSE.txt",output+"/Licenses/Wildlife-CC0.txt",true);
        }
    }
}
