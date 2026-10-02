using System;using UnityEditor;using UnityEditor.SceneManagement;using UnityEngine;
// Temporary 0.69 runner (copied into Assets/Editor/Report069Temp only while checks run).
// PROBE_SCENES=scene, PROBE_PLAY=Racer component type to attach once play mode starts.
// Dan's request: test runs are silent. The editor master mute is set before play mode and the
// listener is muted on the first play-mode callback, before any title/menu audio starts.
// The previous editor mute preference is restored when the batch editor quits.
[InitializeOnLoad] public static class Report069Play {
 static Report069Play(){EditorApplication.quitting+=()=>{if(SessionState.GetString("Report069Play","")!="")EditorUtility.audioMasterMute=SessionState.GetBool("Report069OldMute",false);};EditorApplication.playModeStateChanged+=s=>{var t=SessionState.GetString("Report069Play","");if(t=="")return;
  if(s==PlayModeStateChange.ExitingEditMode||s==PlayModeStateChange.EnteredPlayMode){EditorUtility.audioMasterMute=true;AudioListener.volume=0;}
  if(s==PlayModeStateChange.EnteredPlayMode)new GameObject("Report069 check").AddComponent(Type.GetType("Racer."+t+", Assembly-CSharp"));};}
 public static void Run(){SessionState.SetBool("Report069OldMute",EditorUtility.audioMasterMute);EditorUtility.audioMasterMute=true;EditorSceneManager.OpenScene("Assets/Scenes/"+Environment.GetEnvironmentVariable("PROBE_SCENES")+".unity");SessionState.SetString("Report069Play",Environment.GetEnvironmentVariable("PROBE_PLAY"));EditorApplication.EnterPlaymode();}
}
