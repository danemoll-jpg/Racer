EditorApplication.delayCall += () => {try {
 const string version="0.44.0-review1",evidence="Docs/WorldMap";
 if(Application.isPlaying||EditorApplication.isCompiling||UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)throw new Exception("Saved compiled edit mode required");
 if(PlayerSettings.bundleVersion!=version)throw new Exception("Version must be committed before build");
 long free=new System.IO.DriveInfo("C").AvailableFreeSpace;if(free<15L*1024*1024*1024)throw new Exception("Insufficient free disk space");
 string commit=System.IO.File.ReadAllText("Temp/worldmap-source-commit.txt").Trim();if(commit.Length!=40)throw new Exception("Completion source commit required");
 string output="Builds/Racer-"+version+"-Windows";if(System.IO.Directory.Exists(output))throw new Exception("Fresh output directory required; inspect existing output rather than reusing it");System.IO.Directory.CreateDirectory(output);
 var start=DateTime.UtcNow;var scenes=Racer.Editor.ReverseReviewRelease.Scenes.Concat(new[]{"Assets/Scenes/MountainLoop.unity","Assets/Scenes/MountainLoopReverse.unity","Assets/Scenes/DansBackyardForward.unity"}).ToArray();
 var result=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=scenes,locationPathName=output+"/Racer.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.CleanBuildCache});
 var report=$"{result.summary.result}; errors={result.summary.totalErrors}; warnings={result.summary.totalWarnings}; time={result.summary.totalTime}; startedUtc={start:o}; source={commit}; freeBytesBefore={free}; GUID={result.summary.guid}\n"+string.Join("\n",result.steps.SelectMany(s=>s.messages).Where(m=>m.type==LogType.Error||m.type==LogType.Warning).Select(m=>m.content));
 System.IO.File.WriteAllText(evidence+"/build-release.txt",report);if(result.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new Exception(report);
 System.IO.File.WriteAllText(output+"/VERSION.txt",$"Racer {version}\nGame build: 44000\nSource commit: {commit}\nFresh Unity Windows build UTC: {start:o}\nUnity build GUID: {result.summary.guid}\nClear Tree-Top main trail, Complete Race and rendered full world map\nUnity {Application.unityVersion}\n");
 System.IO.Directory.CreateDirectory(output+"/Licenses");foreach(var p in System.IO.Directory.GetFiles("Assets/Plugins/LocalRadio","*.txt"))System.IO.File.Copy(p,output+"/Licenses/"+System.IO.Path.GetFileName(p),true);
 foreach(var p in new[]{"LICENSE.txt","REVERSE-WILDLIFE-NOTICE.txt"})System.IO.File.Copy("Assets/Audio/Wildlife/"+p,output+"/Licenses/Wildlife-"+p,true);
 System.IO.File.WriteAllText(evidence+"/build-done.txt",report);
}catch(Exception e){System.IO.File.WriteAllText("Docs/WorldMap/build-error.txt",e.ToString());}};return "Scheduled fresh clean-cache Windows build from completion commit";
