using System;using System.IO;using UnityEngine;using UnityEditor;using Racer;
public static class StartWorldMapChecks {
 public static string Main(){if(Application.isPlaying||EditorApplication.isCompiling||UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)throw new Exception("Saved compiled edit mode required");File.WriteAllText("Temp/worldmap-editor-options.txt",EditorSettings.enterPlayModeOptionsEnabled.ToString());EditorSettings.enterPlayModeOptionsEnabled=false;AudioListener.volume=0;new GameObject("Temporary world map UI checks").AddComponent<WorldMapChecks>();EditorApplication.delayCall+=()=>EditorApplication.isPlaying=true;return "Scheduled one muted map/finish fixture with isolated saves";}
}
