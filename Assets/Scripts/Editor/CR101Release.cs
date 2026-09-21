using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
namespace Racer.Editor
{
    [InitializeOnLoad] public static class CR101Release
    {
        public const string Version="0.18.0-review1", Evidence="Docs/CR101-104";
        static string job;static double due;
        static CR101Release(){var p=SessionState.GetString("CR101.Job","");if(p!="")Queue(p);EditorApplication.update+=ReadRequest;}
        static void ReadRequest(){
            const string request="Temp/cr101-request.txt";
            if(BuildPipeline.isBuildingPlayer||EditorApplication.isCompiling||EditorApplication.isUpdating||SessionState.GetString("CR101.Job","")!=""||!File.Exists(request))return;
            var value=File.ReadAllText(request).Trim();if(value==SessionState.GetString("CR101.LastRequest",""))return;
            SessionState.SetString("CR101.LastRequest",value);Queue(value.Split('|')[0]);
        }
        public static void Queue(string task){job=task;SessionState.SetString("CR101.Job",task);due=EditorApplication.timeSinceStartup+2;EditorApplication.update-=Work;EditorApplication.update+=Work;}
        static void Work()
        {
            if(BuildPipeline.isBuildingPlayer||EditorApplication.isCompiling||EditorApplication.isUpdating||EditorApplication.timeSinceStartup<due)return;
            EditorApplication.update-=Work;SessionState.EraseString("CR101.Job");Directory.CreateDirectory(Evidence);
            try{
                if(Application.isPlaying||UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)throw new Exception("Saved edit mode required");
                if(job=="audit")CR101Audit.Run();
                else if(job=="geometry")DiscoveryAuthoring.CR103Geometry();
                else if(job=="landing")DiscoveryAuthoring.CR103Landing();
                else if(job=="local-fixes"){DiscoveryAuthoring.CR103Connection();DiscoveryAuthoring.CR103Return();DiscoveryAuthoring.CR101CloseRemainingFence();DiscoveryAuthoring.CR102Signs();}
                else if(job=="finish-details"){DiscoveryAuthoring.CR101CloseFence();CR101Audit.Run();}
                else if(job=="details"){DiscoveryAuthoring.CR103Connection();DiscoveryAuthoring.CR101Property();DiscoveryAuthoring.CR102Signs();}
                else {
                    if(job!="release"&&!job.StartsWith("build-")&&job!="first-geometry"&&job!="corrected-first")throw new Exception("Unknown CR101 job; refusing an unintended build: "+job);
                    PlayerSettings.bundleVersion=Version;
                    string output=job=="release"?"Builds/Racer-"+Version+"-Windows":"Builds/CR101-"+job;
                    Directory.CreateDirectory(output);
                    var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=ReverseReviewRelease.Scenes,locationPathName=output+"/Racer.exe",target=BuildTarget.StandaloneWindows64,options=job=="release"?BuildOptions.CleanBuildCache:BuildOptions.None});
                    File.WriteAllText(Evidence+"/"+job+"-build.txt",report.summary.result+" errors="+report.summary.totalErrors+" warnings="+report.summary.totalWarnings+" elapsed="+report.summary.totalTime);
                    if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new Exception("Build failed");
                    File.WriteAllText(output+"/VERSION.txt","Racer "+Version+"\nWoodstock Rush / CR-101–104 review\nSafety checkpoint a4dfebb23ba65d2e5d9227b2b5c18d0cbaea578b\nCompletion and validation metadata stamped after checks\nUnity "+Application.unityVersion+"\n");
                    Directory.CreateDirectory(output+"/Licenses");foreach(var p in Directory.GetFiles("Assets/Plugins/LocalRadio","*.txt"))File.Copy(p,output+"/Licenses/"+Path.GetFileName(p),true);
                    foreach(var p in new[]{"LICENSE.txt","REVERSE-WILDLIFE-NOTICE.txt"})File.Copy("Assets/Audio/Wildlife/"+p,output+"/Licenses/Wildlife-"+p,true);
                }
                File.WriteAllText(Evidence+"/"+job+"-done.txt","Succeeded\n"+DateTime.UtcNow.ToString("o"));
            }catch(Exception e){File.WriteAllText(Evidence+"/"+job+"-done.txt",e.ToString());Debug.LogException(e);}
        }
    }
}


