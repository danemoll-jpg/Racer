using System;using System.IO;using System.Linq;using UnityEngine;using UnityEditor;public static class BuildFocusedRelease {public static string Main(){// Executes the existing Tools/Build-StructuralRelease.cs build body on the Editor update loop.
UnityEditor.EditorApplication.delayCall += () => {try { const string Version="0.28.0-review1";

  if(Application.isPlaying||EditorApplication.isCompiling)throw new Exception("Saved compiled edit mode required");PlayerSettings.bundleVersion=Version;EditorUserBuildSettings.development=EditorUserBuildSettings.allowDebugging=EditorUserBuildSettings.connectProfiler=false;AssetDatabase.SaveAssets();
  string output="Builds/Racer-"+Version+"-Windows";System.IO.Directory.CreateDirectory(output);
  // Keep the established scene order and include both courses offered by the menu.
  var scenes=Racer.Editor.ReverseReviewRelease.Scenes.Concat(new[]{"Assets/Scenes/MountainLoop.unity","Assets/Scenes/MountainLoopReverse.unity"}).ToArray();
  foreach(var scene in scenes)if(!System.IO.File.Exists(scene))throw new Exception("Required race scene missing: "+scene);
  var result=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=scenes,locationPathName=output+"/Racer.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.None});
  var report=$"{result.summary.result}; errors={result.summary.totalErrors}; warnings={result.summary.totalWarnings}; time={result.summary.totalTime}\n"+string.Join("\n",result.steps.SelectMany(s=>s.messages).Where(m=>m.type==LogType.Error||m.type==LogType.Warning).Select(m=>m.content));
  System.IO.File.WriteAllText("Docs/FocusedRecovery/build-release.txt",report);if(result.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new Exception(report);
  System.IO.File.WriteAllText(output+"/VERSION.txt",$"Racer {Version}\nStraight incoming Laurel launch; precise recovery surface classification; AI lack-of-progress recovery\nRollback checkpoint: b992c9cf\nUnity {Application.unityVersion}\n");
  System.IO.Directory.CreateDirectory(output+"/Licenses");foreach(var p in System.IO.Directory.GetFiles("Assets/Plugins/LocalRadio","*.txt"))System.IO.File.Copy(p,output+"/Licenses/"+System.IO.Path.GetFileName(p),true);
  foreach(var p in new[]{"LICENSE.txt","REVERSE-WILDLIFE-NOTICE.txt"})System.IO.File.Copy("Assets/Audio/Wildlife/"+p,output+"/Licenses/Wildlife-"+p,true);
  System.IO.File.WriteAllText("Docs/FocusedRecovery/build-done.txt",report);

}catch(Exception e){System.IO.File.WriteAllText("Docs/FocusedRecovery/build-error.txt",e.ToString());}};return "Scheduled existing Windows release build";
}}
