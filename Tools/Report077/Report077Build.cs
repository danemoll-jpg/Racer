using System;using System.IO;using System.Linq;using UnityEngine;using UnityEditor;
// 0.77 Windows test player builds. Test(): a scratch build for the look benchmark (REPORT077_OUT), with frame-timing stats
// switched on for that build only (restored afterwards). The release build is in Build-Report072.cs.txt.
public static class Report077Build {
 public static string[] Scenes=>Racer.Editor.ReverseReviewRelease.Scenes.Concat(new[]{"Assets/Scenes/MountainLoop.unity","Assets/Scenes/MountainLoopReverse.unity","Assets/Scenes/DansBackyardForward.unity","Assets/Scenes/DansBackyardReverse.unity","Assets/Scenes/FreeRoamWorld.unity"}).ToArray();
 public static void Test(){var output=Environment.GetEnvironmentVariable("REPORT077_OUT");Directory.CreateDirectory(output);bool stats=PlayerSettings.enableFrameTimingStats;PlayerSettings.enableFrameTimingStats=true;
  var r=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=Scenes,locationPathName=output+"/Racer.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.None});
  PlayerSettings.enableFrameTimingStats=stats;AssetDatabase.SaveAssets();File.WriteAllText(output+"/build-result.txt",$"{r.summary.result} errors={r.summary.totalErrors} warnings={r.summary.totalWarnings} time={r.summary.totalTime}");EditorApplication.Exit(r.summary.result==UnityEditor.Build.Reporting.BuildResult.Succeeded?0:1);}
}
