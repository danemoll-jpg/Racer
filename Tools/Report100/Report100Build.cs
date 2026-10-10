using System;using System.IO;using System.Linq;using UnityEngine;using UnityEditor;
// Temporary 0.100 check build (not the release): BUILD_OUT = output folder. The release build uses Build-Report099.cs.txt from the pushed commit.
public static class Report100Build { public static void Main(){
 try{string output=Environment.GetEnvironmentVariable("BUILD_OUT");if(Directory.Exists(output))Directory.Delete(output,true);Directory.CreateDirectory(output);
  Racer.Editor.PoliceRadioSync.Sync();var scenes=Racer.Editor.ReverseReviewRelease.Scenes.Concat(new[]{"Assets/Scenes/MountainLoop.unity","Assets/Scenes/MountainLoopReverse.unity","Assets/Scenes/DansBackyardForward.unity","Assets/Scenes/DansBackyardReverse.unity","Assets/Scenes/FreeRoamWorld.unity"}).ToArray();
  var r=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=scenes,locationPathName=output+"/Racer.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.None});
  File.WriteAllText(output+"/../"+Path.GetFileName(output)+"-build.txt",$"{r.summary.result}; errors={r.summary.totalErrors}; time={r.summary.totalTime}");EditorApplication.Exit(r.summary.result==UnityEditor.Build.Reporting.BuildResult.Succeeded?0:1);
 }catch(Exception e){File.WriteAllText(Path.GetTempPath()+"report100-build-error.txt",e.ToString());EditorApplication.Exit(1);}}
}
