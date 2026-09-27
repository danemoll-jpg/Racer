EditorApplication.delayCall += () => {try {
 const string version="0.30.0-review1";
 if(Application.isPlaying||EditorApplication.isCompiling||UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)throw new Exception("Saved compiled edit mode required");
 if(PlayerSettings.bundleVersion!=version)throw new Exception("Version must be configured and committed before build");
 if(new System.IO.DriveInfo("C").AvailableFreeSpace<15L*1024*1024*1024)throw new Exception("Insufficient free disk space");
 string output="Builds/Racer-"+version+"-Windows";System.IO.Directory.CreateDirectory(output);
 var scenes=Racer.Editor.ReverseReviewRelease.Scenes.Concat(new[]{"Assets/Scenes/MountainLoop.unity","Assets/Scenes/MountainLoopReverse.unity"}).ToArray();
 var result=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=scenes,locationPathName=output+"/Racer.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.None});
 var report=$"{result.summary.result}; errors={result.summary.totalErrors}; warnings={result.summary.totalWarnings}; time={result.summary.totalTime}\n"+string.Join("\n",result.steps.SelectMany(s=>s.messages).Where(m=>m.type==LogType.Error||m.type==LogType.Warning).Select(m=>m.content));
 System.IO.File.WriteAllText("Docs/RouteAtlas/build-release.txt",report);if(result.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new Exception(report);
 System.IO.File.WriteAllText(output+"/VERSION.txt",$"Racer {version}\nGranite Saddle is Forest Reverse MAIN; six-course navigation and route atlas; local House 3 cleanup, Laurel preserved\nSafety checkpoint: aa96293f564eb99d100864cb503351be44cd8495\nUnity {Application.unityVersion}\n");
 System.IO.Directory.CreateDirectory(output+"/Licenses");foreach(var p in System.IO.Directory.GetFiles("Assets/Plugins/LocalRadio","*.txt"))System.IO.File.Copy(p,output+"/Licenses/"+System.IO.Path.GetFileName(p),true);
 foreach(var p in new[]{"LICENSE.txt","REVERSE-WILDLIFE-NOTICE.txt"})System.IO.File.Copy("Assets/Audio/Wildlife/"+p,output+"/Licenses/Wildlife-"+p,true);
 System.IO.File.WriteAllText("Docs/RouteAtlas/build-done.txt",report);
}catch(Exception e){System.IO.File.WriteAllText("Docs/RouteAtlas/build-error.txt",e.ToString());}};return "Scheduled existing Windows release process";
