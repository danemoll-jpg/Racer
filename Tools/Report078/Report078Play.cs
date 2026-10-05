using System;using UnityEditor;using UnityEditor.SceneManagement;using UnityEngine;
// Temporary 0.78 runner (copied into Assets/Editor/Report078Temp only while checks run).
// PROBE_SCENES=scene, PROBE_PLAY=Racer component type to attach once play mode starts.
// Test runs are silent: the editor master mute is set before play mode and restored when the batch editor quits.
[InitializeOnLoad] public static class Report078Play {
 static Report078Play(){EditorApplication.quitting+=()=>{if(SessionState.GetString("Report078Play","")!="")EditorUtility.audioMasterMute=SessionState.GetBool("Report078OldMute",false);};EditorApplication.playModeStateChanged+=s=>{var t=SessionState.GetString("Report078Play","");if(t=="")return;
  if(s==PlayModeStateChange.ExitingEditMode||s==PlayModeStateChange.EnteredPlayMode){EditorUtility.audioMasterMute=true;AudioListener.volume=0;}
  if(s==PlayModeStateChange.EnteredPlayMode)new GameObject("Report078 check").AddComponent(Type.GetType("Racer."+t+", Assembly-CSharp"));
  if(s==PlayModeStateChange.EnteredEditMode)EditorApplication.Exit(0);};}
 public static void Run(){SessionState.SetBool("Report078OldMute",EditorUtility.audioMasterMute);EditorUtility.audioMasterMute=true;var sc=Environment.GetEnvironmentVariable("PROBE_SCENES");EditorSceneManager.OpenScene("Assets/Scenes/"+sc+".unity");
  SessionState.SetString("Report078Play",Environment.GetEnvironmentVariable("PROBE_PLAY"));EditorApplication.EnterPlaymode();}
}
