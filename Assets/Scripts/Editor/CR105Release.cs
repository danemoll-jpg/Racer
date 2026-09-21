using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
namespace Racer.Editor
{
    [InitializeOnLoad] public static class CR105Release
    {
        public const string Version="0.19.0-review3";
        static CR105Release(){EditorApplication.update+=Tick;}
        static void Tick()
        {
            if(EditorApplication.isCompiling||EditorApplication.isUpdating||BuildPipeline.isBuildingPlayer)return;
            const string request="Temp/cr105-request.txt";if(!File.Exists(request))return;string value=File.ReadAllText(request).Trim();if(SessionState.GetString("CR105.request","")==value)return;SessionState.SetString("CR105.request",value);
            string job=value.Split('|')[0];Directory.CreateDirectory(CR105Authoring.Evidence);
            try{
                if(Application.isPlaying||UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)throw new Exception("Saved edit mode required");
                if(job=="world")CR105Authoring.World();
                else if(job=="mailbox-audit")CR105Authoring.MailboxAudit();
                else if(job=="mailbox-shoulder")CR105Authoring.Mailbox3Shoulder();
                else if(job=="mountain-geometry")DiscoveryAuthoring.MountainGeometry();
                else if(job=="mountain-details")DiscoveryAuthoring.MountainDetails();
                else if(job=="mountain-routing")DiscoveryAuthoring.MountainRouting();
                else if(job=="mountain-obstruction")DiscoveryAuthoring.MountainObstruction();
                else if(job=="mountain-local-corrections")DiscoveryAuthoring.MountainLocalCorrections();
                else if(job.StartsWith("build")){
                    string output=job.StartsWith("build-release")?"Builds/Racer-"+Version+"-Windows":"Builds/CR105-"+job;
                    Directory.CreateDirectory(output);PlayerSettings.bundleVersion=Version;
                    var scenes=ReverseReviewRelease.Scenes.Concat(new[]{"Assets/Scenes/MountainLoop.unity","Assets/Scenes/MountainLoopReverse.unity"}.Where(File.Exists)).ToArray();
                    var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=scenes,locationPathName=output+"/Racer.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.None});
                    File.WriteAllText(CR105Authoring.Evidence+"/"+job+"-report.txt",report.summary.result+" errors="+report.summary.totalErrors+" warnings="+report.summary.totalWarnings);
                    if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new Exception("Build failed");
                    File.WriteAllText(output+"/VERSION.txt","Racer "+Version+"\nCR-105–111\nSafety checkpoint 542a61629c4a8ea0ef0763064b7c01c9c8d2c33f\n");
                    Directory.CreateDirectory(output+"/Licenses");foreach(var p in Directory.GetFiles("Assets/Plugins/LocalRadio","*.txt"))File.Copy(p,output+"/Licenses/"+Path.GetFileName(p),true);
                    foreach(var p in new[]{"LICENSE.txt","REVERSE-WILDLIFE-NOTICE.txt"})File.Copy("Assets/Audio/Wildlife/"+p,output+"/Licenses/Wildlife-"+p,true);
                }else throw new Exception("Unknown job "+job);
                File.WriteAllText(CR105Authoring.Evidence+"/"+job+"-done.txt","Succeeded "+DateTime.UtcNow.ToString("o"));
            }catch(Exception e){File.WriteAllText(CR105Authoring.Evidence+"/"+job+"-done.txt",e.ToString());Debug.LogException(e);}
        }
    }
}
