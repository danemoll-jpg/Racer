using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using Object=UnityEngine.Object;
namespace Racer.Editor
{
    public static class ForestRelease
    {
        public const string Version="0.8.0-review1";
        static double buildAt;
        public static void QueueBuild(){buildAt=EditorApplication.timeSinceStartup+2;EditorApplication.update-=BuildTick;EditorApplication.update+=BuildTick;}
        static void BuildTick()
        {
            if(EditorApplication.timeSinceStartup<buildAt)return;
            EditorApplication.update-=BuildTick;
            try{Build();File.WriteAllText(ForestCourseBuild.Evidence+"/build-done.txt","Complete");}
            catch(Exception e){File.WriteAllText(ForestCourseBuild.Evidence+"/build-done.txt",e.ToString());}
        }
        public static void Build()
        {
            if(Application.isPlaying||UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)throw new InvalidOperationException("Saved edit mode required");
            if(PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.Standalone).Contains("ENABLE_RUNTIME_PIPELINE"))throw new InvalidOperationException("Runtime pipeline must be excluded");
            PlayerSettings.bundleVersion=Version;EditorUserBuildSettings.development=EditorUserBuildSettings.allowDebugging=EditorUserBuildSettings.connectProfiler=false;
            string output="Builds/Racer-"+Version+"-Windows";Directory.CreateDirectory(output);
            var result=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{StreetLoopBuilder.ScenePath,LakeCourseBuild.ScenePath},locationPathName=output+"/Racer.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.None});
            File.WriteAllText(ForestCourseBuild.Evidence+"/build.txt",$"{result.summary.result}; errors={result.summary.totalErrors}; warnings={result.summary.totalWarnings}; time={result.summary.totalTime}\n"+string.Join("\n",result.steps.SelectMany(s=>s.messages).Where(m=>m.type==LogType.Error||m.type==LogType.Warning).Select(m=>m.content)));
            if(result.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new InvalidOperationException("Build failed");
            File.WriteAllText(output+"/VERSION.txt",$"Racer {Version}\nWindows x64 review / CR-056 Forest Loop\nRules: street-v8-landings / lake-v2-forest; difficulty unchanged; development=false; Runtime Pipeline excluded\nSource tree: SOURCE-SHA256.txt; completion commit recorded after validation\nUnity {Application.unityVersion}\n");
            Directory.CreateDirectory(output+"/Music");
            string licenses=output+"/Licenses";Directory.CreateDirectory(licenses);
            foreach(var p in Directory.GetFiles("Assets/Plugins/LocalRadio","*.txt"))File.Copy(p,licenses+"/"+Path.GetFileName(p),true);
        }
        public static void Capture()
        {
            var race=Object.FindAnyObjectByType<RaceDirector>();var camera=new GameObject("Course evidence").AddComponent<Camera>();
            camera.transform.SetPositionAndRotation(new(245,1050,20),Quaternion.Euler(90,0,0));camera.orthographic=true;camera.orthographicSize=570;camera.farClipPlane=2000;
            var rt=new RenderTexture(1400,1000,24);camera.targetTexture=rt;camera.Render();var old=RenderTexture.active;RenderTexture.active=rt;
            var t=new Texture2D(1400,1000,TextureFormat.RGB24,false);t.ReadPixels(new Rect(0,0,1400,1000),0,0);t.Apply();File.WriteAllBytes(ForestCourseBuild.Evidence+"/lake-overview.png",t.EncodeToPNG());RenderTexture.active=old;
            Object.DestroyImmediate(t);Object.DestroyImmediate(rt);Object.DestroyImmediate(camera.gameObject);
        }
    }
}

