using System;using UnityEditor;using UnityEditor.SceneManagement;using UnityEngine;
[InitializeOnLoad] public static class RegressionPlay {
 static RegressionPlay(){EditorApplication.playModeStateChanged+=s=>{if(s==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool("RegressionPlay",false))new GameObject("Regression drive").AddComponent<Racer.RegressionDriveChecks>();};}
 public static void Run(){EditorSceneManager.OpenScene("Assets/Scenes/"+Environment.GetEnvironmentVariable("PROBE_SCENES")+".unity");SessionState.SetBool("RegressionPlay",true);EditorApplication.EnterPlaymode();}
}
