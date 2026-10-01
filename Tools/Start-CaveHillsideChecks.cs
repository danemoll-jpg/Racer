using System;using System.IO;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using Racer;
public static class StartCaveHillsideChecks {
 public static string Main(){return Start("DansBackyardReverse","backyard-driving");}
 public static string Cave(){return Start("LakeWoods","cave-driving");}
 static string Start(string scene,string output){if(Application.isPlaying||EditorApplication.isCompiling)throw new Exception("Compiled edit mode required");EditorSceneManager.OpenScene("Assets/Scenes/"+scene+".unity");AudioListener.volume=0;foreach(var traffic in UnityEngine.Object.FindObjectsByType<ContinuationTraffic>())traffic.enabled=false;FinalTwoAiChecks.Output="Docs/CaveHillside/"+output;Directory.CreateDirectory(FinalTwoAiChecks.Output);new GameObject("Temporary underground driving checks").AddComponent<FinalTwoAiChecks>();EditorApplication.delayCall+=()=>EditorApplication.isPlaying=true;return "Started bounded "+scene+" motorcycle/ATV production-driver checks";}
}


