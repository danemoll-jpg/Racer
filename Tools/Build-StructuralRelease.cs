using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;
using Racer.Editor;
public static class BuildStructuralRelease {
 public const string Version="0.24.0-review1";
 public static string Configure(){PlayerSettings.bundleVersion=Version;EditorUserBuildSettings.development=EditorUserBuildSettings.allowDebugging=EditorUserBuildSettings.connectProfiler=false;AssetDatabase.SaveAssets();return Version;}
 public static string Main(){
  if(Application.isPlaying||EditorApplication.isCompiling)throw new Exception("Saved compiled edit mode required");Configure();
  string output="Builds/Racer-"+Version+"-Windows";Directory.CreateDirectory(output);
  var result=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=ReverseReviewRelease.Scenes,locationPathName=output+"/Racer.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.None});
  var report=$"{result.summary.result}; errors={result.summary.totalErrors}; warnings={result.summary.totalWarnings}; time={result.summary.totalTime}\n"+string.Join("\n",result.steps.SelectMany(s=>s.messages).Where(m=>m.type==LogType.Error||m.type==LogType.Warning).Select(m=>m.content));
  File.WriteAllText("Docs/CR133-137/build-release.txt",report);if(result.summary.result!=BuildResult.Succeeded)throw new Exception(report);
  File.WriteAllText(output+"/VERSION.txt",$"Racer {Version}\nCR133-137 structural roads and earned recovery\nRollback checkpoint: 46e689e9119395ce2eb26f163b685279d1b7f90a\nUnity {Application.unityVersion}\n");
  Directory.CreateDirectory(output+"/Licenses");foreach(var p in Directory.GetFiles("Assets/Plugins/LocalRadio","*.txt"))File.Copy(p,output+"/Licenses/"+Path.GetFileName(p),true);
  foreach(var p in new[]{"LICENSE.txt","REVERSE-WILDLIFE-NOTICE.txt"})File.Copy("Assets/Audio/Wildlife/"+p,output+"/Licenses/Wildlife-"+p,true);
  return report;
 }
}
