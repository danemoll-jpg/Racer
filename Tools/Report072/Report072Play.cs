using System;using UnityEditor;using UnityEditor.SceneManagement;using UnityEngine;
// Temporary 0.72 runner (copied into Assets/Editor/Report072Temp only while checks run).
// PROBE_SCENES=scene, PROBE_PLAY=Racer component type to attach once play mode starts.
// Dan's request: test runs are silent. The editor master mute is set before play mode and the
// listener is muted on the first play-mode callback, before any title/menu audio starts.
// The previous editor mute preference is restored when the batch editor quits.
[InitializeOnLoad] public static class Report072Play {
 static Report072Play(){EditorApplication.quitting+=()=>{if(SessionState.GetString("Report072Play","")!="")EditorUtility.audioMasterMute=SessionState.GetBool("Report072OldMute",false);};EditorApplication.playModeStateChanged+=s=>{var t=SessionState.GetString("Report072Play","");if(t=="")return;
  if(s==PlayModeStateChange.ExitingEditMode||s==PlayModeStateChange.EnteredPlayMode){EditorUtility.audioMasterMute=true;AudioListener.volume=0;}
  if(s==PlayModeStateChange.EnteredPlayMode)new GameObject("Report072 check").AddComponent(Type.GetType("Racer."+t+", Assembly-CSharp"));
  // A check that ends play mode on purpose (Part A: QUIT) is recorded here, then the batch editor exits.
  if(s==PlayModeStateChange.EnteredEditMode){var o=Environment.GetEnvironmentVariable("PROBE_OUT");if(o!=null&&System.IO.File.Exists(o+"/quit-pressed.txt"))System.IO.File.AppendAllText(o+"/quit-checks.txt","PASS QUIT ends the game (play mode stopped by RaceFlow.Quit; Application.Quit in a build)\n");EditorApplication.Exit(0);}};}
 public static void Run(){SessionState.SetBool("Report072OldMute",EditorUtility.audioMasterMute);EditorUtility.audioMasterMute=true;var sc=Environment.GetEnvironmentVariable("PROBE_SCENES");EditorSceneManager.OpenScene("Assets/Scenes/"+sc+".unity");
  // Isolation tests only (scene not saved): PROBE_OLD_RIBBON=1 puts the 0.71 ribbon collider back, PROBE_OLD_TILES=1 the 0.71 tiles (Assets/Report072Head).
  foreach(var mc in UnityEngine.Object.FindObjectsByType<MeshCollider>(FindObjectsSortMode.None)){if(!mc.sharedMesh)continue;
   if(Environment.GetEnvironmentVariable("PROBE_OLD_RIBBON")=="1"&&mc.name=="Ground_CR103 supported return"){mc.sharedMesh=AssetDatabase.LoadAssetAtPath<Mesh>($"Assets/Track/Report071/{sc}-own-cr103-return.asset");Debug.Log("REPORT072 old ribbon collider");}
   var hp="Assets/Report072Head/"+System.IO.Path.GetFileName(AssetDatabase.GetAssetPath(mc.sharedMesh));if(Environment.GetEnvironmentVariable("PROBE_OLD_TILES")=="1"&&System.IO.File.Exists(hp)){mc.sharedMesh=AssetDatabase.LoadAssetAtPath<Mesh>(hp);Debug.Log("REPORT072 old tile "+mc.name);}}SessionState.SetString("Report072Play",Environment.GetEnvironmentVariable("PROBE_PLAY"));EditorApplication.EnterPlaymode();}
}
