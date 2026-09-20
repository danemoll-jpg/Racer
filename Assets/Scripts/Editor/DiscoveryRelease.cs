using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
namespace Racer.Editor
{
    [InitializeOnLoad] public static class DiscoveryRelease
    {
        public const string Version="0.17.0-review1", Evidence="Docs/CR097-100";
        static string job;static double due;
        static DiscoveryRelease(){var p=SessionState.GetString("CR097.Job","");if(p!="")Queue(p);EditorApplication.update+=ReadRequest;}
        static void ReadRequest(){
            const string request="Temp/cr097-request.txt";
            if(BuildPipeline.isBuildingPlayer||EditorApplication.isCompiling||EditorApplication.isUpdating||SessionState.GetString("CR097.Job","")!=""||!File.Exists(request))return;
            var value=File.ReadAllText(request).Trim();if(value==SessionState.GetString("CR097.LastRequest",""))return;
            SessionState.SetString("CR097.LastRequest",value);Queue(value.Split('|')[0]);
        }
        public static void Queue(string task){job=task;SessionState.SetString("CR097.Job",task);due=EditorApplication.timeSinceStartup+2;EditorApplication.update-=Work;EditorApplication.update+=Work;}
        static void Work()
        {
            if(BuildPipeline.isBuildingPlayer||EditorApplication.isCompiling||EditorApplication.isUpdating||EditorApplication.timeSinceStartup<due)return;
            EditorApplication.update-=Work;SessionState.EraseString("CR097.Job");Directory.CreateDirectory(Evidence);
            try{
                if(Application.isPlaying||UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)throw new Exception("Saved edit mode required");
                if(job=="integrate"){DiscoveryAuthoring.CorrectPropertiesAndProps(true);DiscoveryAuthoring.CorrectGuidanceAndMap();foreach(var p in new[]{"Assets/Resources/Title/Artwork.png","Assets/Resources/Title/Voice.wav","Assets/Resources/Title/ThemeLoop.wav"})AssetDatabase.ImportAsset(p,ImportAssetOptions.ForceUpdate);}
                else if(job=="title-assets"){foreach(var p in new[]{"Assets/Resources/Title/Artwork.png","Assets/Resources/Title/Voice.wav","Assets/Resources/Title/ThemeLoop.wav"})AssetDatabase.ImportAsset(p,ImportAssetOptions.ForceUpdate);}
                else if(job=="properties-final")DiscoveryAuthoring.CorrectPropertiesAndProps(true);
                else if(job=="guidance")DiscoveryAuthoring.CorrectGuidanceAndMap();
                else if(job=="activity-links")DiscoveryAuthoring.RefreshFenceActivities();
                else if(job=="drive-aprons")DiscoveryAuthoring.RefineDriveAprons();
                else if(job=="properties")DiscoveryAuthoring.CorrectPropertiesAndProps();
                else if(job=="summit")DiscoveryAuthoring.Summit();
                else if(job=="refine-world"){DiscoveryAuthoring.RefineSummit();DiscoveryAuthoring.ProtectBaselineRoutes();DiscoveryAuthoring.RefineDriveway();DiscoveryAuthoring.AuditFinish();}
                else if(job=="refine-summit")DiscoveryAuthoring.RefineSummit();
                else if(job=="audit")DiscoveryAuthoring.AuditFinish();
                else if(job=="protect-baseline")DiscoveryAuthoring.ProtectBaselineRoutes();
                else if(job=="driveway")DiscoveryAuthoring.RefineDriveway();
                else if(job=="world")DiscoveryAuthoring.WorldPass();
                else {
                    if(job!="release"&&!job.StartsWith("build-")&&job!="first-geometry"&&job!="corrected-first")throw new Exception("Unknown CR097 job; refusing an unintended build: "+job);
                    PlayerSettings.bundleVersion=Version;
                    string output=job=="release"?"Builds/Racer-"+Version+"-Windows":"Builds/CR097-"+job;
                    Directory.CreateDirectory(output);
                    var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=ReverseReviewRelease.Scenes,locationPathName=output+"/Racer.exe",target=BuildTarget.StandaloneWindows64,options=job=="release"?BuildOptions.CleanBuildCache:BuildOptions.None});
                    File.WriteAllText(Evidence+"/"+job+"-build.txt",report.summary.result+" errors="+report.summary.totalErrors+" warnings="+report.summary.totalWarnings+" elapsed="+report.summary.totalTime);
                    if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new Exception("Build failed");
                    File.WriteAllText(output+"/VERSION.txt","Racer "+Version+"\nWoodstock Rush / CR-097–100 review\nSafety checkpoint acafa41ce76c410e4b970dbbd7c9cf14e487fc8e\nCompletion and validation metadata stamped after checks\nUnity "+Application.unityVersion+"\n");
                    Directory.CreateDirectory(output+"/Licenses");foreach(var p in Directory.GetFiles("Assets/Plugins/LocalRadio","*.txt"))File.Copy(p,output+"/Licenses/"+Path.GetFileName(p),true);
                    foreach(var p in new[]{"LICENSE.txt","REVERSE-WILDLIFE-NOTICE.txt"})File.Copy("Assets/Audio/Wildlife/"+p,output+"/Licenses/Wildlife-"+p,true);
                }
                File.WriteAllText(Evidence+"/"+job+"-done.txt","Succeeded\n"+DateTime.UtcNow.ToString("o"));
            }catch(Exception e){File.WriteAllText(Evidence+"/"+job+"-done.txt",e.ToString());Debug.LogException(e);}
        }
    }
}
