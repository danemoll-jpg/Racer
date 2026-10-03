using System;using UnityEditor;using UnityEditor.SceneManagement;using UnityEngine;
// Temporary 0.71 runner (copied into Assets/Editor/Report071Temp only while checks run).
// PROBE_SCENES=scene, PROBE_PLAY=Racer component type to attach once play mode starts.
// Dan's request: test runs are silent. The editor master mute is set before play mode and the
// listener is muted on the first play-mode callback, before any title/menu audio starts.
// The previous editor mute preference is restored when the batch editor quits.
[InitializeOnLoad] public static class Report071Play {
 static Report071Play(){EditorApplication.quitting+=()=>{if(SessionState.GetString("Report071Play","")!="")EditorUtility.audioMasterMute=SessionState.GetBool("Report071OldMute",false);};EditorApplication.playModeStateChanged+=s=>{var t=SessionState.GetString("Report071Play","");if(t=="")return;
  if(s==PlayModeStateChange.ExitingEditMode||s==PlayModeStateChange.EnteredPlayMode){EditorUtility.audioMasterMute=true;AudioListener.volume=0;}
  if(s==PlayModeStateChange.EnteredPlayMode)new GameObject("Report071 check").AddComponent(Type.GetType("Racer."+t+", Assembly-CSharp"));
  // A check that ends play mode on purpose (Part A: QUIT) is recorded here, then the batch editor exits.
  if(s==PlayModeStateChange.EnteredEditMode){var o=Environment.GetEnvironmentVariable("PROBE_OUT");if(o!=null&&System.IO.File.Exists(o+"/quit-pressed.txt"))System.IO.File.AppendAllText(o+"/quit-checks.txt","PASS QUIT ends the game (play mode stopped by RaceFlow.Quit; Application.Quit in a build)\n");EditorApplication.Exit(0);}};}
 public static void Run(){SessionState.SetBool("Report071OldMute",EditorUtility.audioMasterMute);EditorUtility.audioMasterMute=true;EditorSceneManager.OpenScene("Assets/Scenes/"+Environment.GetEnvironmentVariable("PROBE_SCENES")+".unity");SessionState.SetString("Report071Play",Environment.GetEnvironmentVariable("PROBE_PLAY"));EditorApplication.EnterPlaymode();}
}
