using System;using System.IO;using System.Linq;using UnityEngine;using UnityEditor;
// 0.67.0-review1 fresh Windows build from the pushed completion source (rules 15/16). Editor-only.
namespace Racer.Editor { public static class Report067Release { public static void Main(){
 const string version="0.67.0-review1",root="C:/Users/danmo/Racer",evidence=root+"/Docs/Report067";
 try{
 if(Application.isPlaying||EditorApplication.isCompiling||UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)throw new Exception("Saved compiled edit mode required");
 if(PlayerSettings.bundleVersion!=version)throw new Exception("Version must be committed before build");
 long free=new DriveInfo("C").AvailableFreeSpace;if(free<15L*1024*1024*1024)throw new Exception("Insufficient free disk space");
 string commit=File.ReadAllText(root+"/Temp/report067-source-commit.txt").Trim();if(commit.Length!=40)throw new Exception("Completion source commit required");
 string output=root+"/Builds/Racer-"+version+"-Windows";if(Directory.Exists(output))throw new Exception("Fresh output directory required");Directory.CreateDirectory(output);
 var start=DateTime.UtcNow;var scenes=Racer.Editor.ReverseReviewRelease.Scenes.Concat(new[]{"Assets/Scenes/MountainLoop.unity","Assets/Scenes/MountainLoopReverse.unity","Assets/Scenes/DansBackyardForward.unity","Assets/Scenes/DansBackyardReverse.unity"}).ToArray();
 var result=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=scenes,locationPathName=output+"/Racer.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.None});
 var report=$"{result.summary.result}; errors={result.summary.totalErrors}; warnings={result.summary.totalWarnings}; time={result.summary.totalTime}; startedUtc={start:o}; source={commit}; freeBytesBefore={free}; GUID={result.summary.guid}\n"+string.Join("\n",result.steps.SelectMany(s=>s.messages).Where(m=>m.type==LogType.Error||m.type==LogType.Warning).Select(m=>m.content));
 File.WriteAllText(evidence+"/build-release.txt",report);if(result.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new Exception(report);
 File.WriteAllText(output+"/VERSION.txt",$"Racer {version}\nGame build: 67000\nSource commit: {commit}\nFresh Unity Windows build UTC: {start:o}\nUnity build GUID: {result.summary.guid}\n20-report cleanup (Mountain Loop, Street Loop cairn, Backyard arrows/run-up) + Debug sessions survive quitting\nUnity {Application.unityVersion}\n");
 Directory.CreateDirectory(output+"/Licenses");foreach(var p in Directory.GetFiles("Assets/Plugins/LocalRadio","*.txt"))File.Copy(p,output+"/Licenses/"+Path.GetFileName(p),true);
 foreach(var p in new[]{"LICENSE.txt","REVERSE-WILDLIFE-NOTICE.txt"})File.Copy("Assets/Audio/Wildlife/"+p,output+"/Licenses/Wildlife-"+p,true);
 File.WriteAllText(evidence+"/build-done.txt",report);EditorApplication.Exit(0);
 }catch(Exception e){File.WriteAllText(evidence+"/build-error.txt",e.ToString());EditorApplication.Exit(1);}
}}}
