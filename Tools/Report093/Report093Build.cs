using System;using System.IO;using System.Linq;using UnityEngine;using UnityEditor;
// 0.93 Windows test player for the menu and split-screen checks (REPORT093_OUT), with frame-timing stats switched on for
// that build only (restored afterwards). Not the release build.
public static class Report093Build {
 public static void Test(){var output=Environment.GetEnvironmentVariable("REPORT093_OUT");Directory.CreateDirectory(output);bool stats=PlayerSettings.enableFrameTimingStats;PlayerSettings.enableFrameTimingStats=true;
  var scenes=Racer.Editor.ReverseReviewRelease.Scenes.Concat(new[]{"Assets/Scenes/MountainLoop.unity","Assets/Scenes/MountainLoopReverse.unity","Assets/Scenes/DansBackyardForward.unity","Assets/Scenes/DansBackyardReverse.unity","Assets/Scenes/FreeRoamWorld.unity"}).ToArray();
  var r=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=scenes,locationPathName=output+"/Racer.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.None});
  PlayerSettings.enableFrameTimingStats=stats;AssetDatabase.SaveAssets();File.WriteAllText(output+"/build-result.txt",$"{r.summary.result} errors={r.summary.totalErrors} warnings={r.summary.totalWarnings} time={r.summary.totalTime}");EditorApplication.Exit(r.summary.result==UnityEditor.Build.Reporting.BuildResult.Succeeded?0:1);}
}
