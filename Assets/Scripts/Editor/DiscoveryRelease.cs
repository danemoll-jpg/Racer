using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
namespace Racer.Editor
{
    [InitializeOnLoad] public static class DiscoveryRelease
    {
        public const string Version="0.16.0-review3", Evidence="Docs/CR091-096";
        static string job;static double due;
        static DiscoveryRelease(){var p=SessionState.GetString("CR091.Job","");if(p!="")Queue(p);}
        public static void Queue(string task){job=task;SessionState.SetString("CR091.Job",task);due=EditorApplication.timeSinceStartup+2;EditorApplication.update-=Work;EditorApplication.update+=Work;}
        static void Work()
        {
            if(BuildPipeline.isBuildingPlayer||EditorApplication.isCompiling||EditorApplication.isUpdating||EditorApplication.timeSinceStartup<due)return;
            EditorApplication.update-=Work;SessionState.EraseString("CR091.Job");Directory.CreateDirectory(Evidence);
            try{
                if(Application.isPlaying||UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)throw new Exception("Saved edit mode required");
                if(job=="summit")DiscoveryAuthoring.Summit();
                else if(job=="refine-world"){DiscoveryAuthoring.RefineSummit();DiscoveryAuthoring.ProtectBaselineRoutes();DiscoveryAuthoring.RefineDriveway();DiscoveryAuthoring.AuditFinish();}
                else if(job=="refine-summit")DiscoveryAuthoring.RefineSummit();
                else if(job=="audit")DiscoveryAuthoring.AuditFinish();
                else if(job=="protect-baseline")DiscoveryAuthoring.ProtectBaselineRoutes();
                else if(job=="driveway")DiscoveryAuthoring.RefineDriveway();
                else if(job=="world")DiscoveryAuthoring.WorldPass();
                else {
                    PlayerSettings.bundleVersion=Version;
                    string output=job=="release"?"Builds/Racer-"+Version+"-Windows":"Builds/CR091-"+job;
                    Directory.CreateDirectory(output);
                    var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=ReverseReviewRelease.Scenes,locationPathName=output+"/Racer.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.CleanBuildCache});
                    File.WriteAllText(Evidence+"/"+job+"-build.txt",report.summary.result+" errors="+report.summary.totalErrors+" warnings="+report.summary.totalWarnings+" elapsed="+report.summary.totalTime);
                    if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new Exception("Build failed");
                    File.WriteAllText(output+"/VERSION.txt","Racer "+Version+"\nWoodstock Rush / CR-091–096 review\nSafety checkpoint a89577e9b0d3dfb4bf61652dd6ea71bae39bcdd1\nCompletion and validation metadata stamped after checks\nUnity "+Application.unityVersion+"\n");
                    Directory.CreateDirectory(output+"/Licenses");foreach(var p in Directory.GetFiles("Assets/Plugins/LocalRadio","*.txt"))File.Copy(p,output+"/Licenses/"+Path.GetFileName(p),true);
                    foreach(var p in new[]{"LICENSE.txt","REVERSE-WILDLIFE-NOTICE.txt"})File.Copy("Assets/Audio/Wildlife/"+p,output+"/Licenses/Wildlife-"+p,true);
                }
                File.WriteAllText(Evidence+"/"+job+"-done.txt","Succeeded\n"+DateTime.UtcNow.ToString("o"));
            }catch(Exception e){File.WriteAllText(Evidence+"/"+job+"-done.txt",e.ToString());Debug.LogException(e);}
        }
    }
}
