using System;
using System.IO;
using UnityEngine;
using UnityEditor;
namespace Racer.Editor
{
    [InitializeOnLoad] public static class ExplorationRelease
    {
        public const string Version="0.15.0-review1";
        static string job;static double due;
        static ExplorationRelease(){var pending=SessionState.GetString("CR081.Job","");if(pending!="")Queue(pending);}
        public static void Queue(string task){job=task;SessionState.SetString("CR081.Job",task);due=EditorApplication.timeSinceStartup+2;EditorApplication.update-=Work;EditorApplication.update+=Work;}
        static void Work()
        {
            if(EditorApplication.isCompiling||EditorApplication.isUpdating||EditorApplication.timeSinceStartup<due)return;
            EditorApplication.update-=Work;SessionState.EraseString("CR081.Job");Directory.CreateDirectory("Docs/CR081-090");
            try
            {
                if(Application.isPlaying||UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)throw new Exception("Saved edit mode required");
                if(job=="mountain")ExplorationAuthoring.MountainPass();
                else if(job=="home")ExplorationAuthoring.HomePass();
                else if(job=="refine")ExplorationAuthoring.Refine();
                else
                {
                    PlayerSettings.bundleVersion=Version;string output=job=="release"?"Builds/Racer-"+Version+"-Windows":"Builds/CR081-"+job;
                    var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=ReverseReviewRelease.Scenes,locationPathName=output+"/Racer.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.None});
                    if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new Exception(report.summary.result+" errors="+report.summary.totalErrors);
                    File.WriteAllText(output+"/VERSION.txt","Racer "+Version+"\nWoodstock Rush CR-081 + CR-090\nSafety checkpoint: 8419e9194985955bb56a41854f1b5dce7968b270\nSource manifest and completion commit stamped after validation\nUnity "+Application.unityVersion+"\n");
                    Directory.CreateDirectory(output+"/Licenses");foreach(var p in Directory.GetFiles("Assets/Plugins/LocalRadio","*.txt"))File.Copy(p,output+"/Licenses/"+Path.GetFileName(p),true);
                    foreach(var p in new[]{"LICENSE.txt","REVERSE-WILDLIFE-NOTICE.txt"})File.Copy("Assets/Audio/Wildlife/"+p,output+"/Licenses/Wildlife-"+p,true);
                }
                File.WriteAllText("Docs/CR081-090/"+job+"-done.txt","Succeeded errors=0\n"+DateTime.UtcNow.ToString("o"));
            }catch(Exception e){File.WriteAllText("Docs/CR081-090/"+job+"-done.txt",e.ToString());Debug.LogException(e);}
        }
    }
}
