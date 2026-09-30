using System;using System.IO;using UnityEngine;using UnityEditor;using Racer;
public static class StartAndersonArrivalCheck {
 public static string Main(){if(Application.isPlaying||EditorApplication.isCompiling||UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)throw new Exception("Saved compiled edit mode required");File.WriteAllText("Temp/worldcleanup-editor-options.txt",EditorSettings.enterPlayModeOptionsEnabled.ToString());EditorSettings.enterPlayModeOptionsEnabled=false;AudioListener.volume=0;new GameObject("Temporary Anderson arrival check").AddComponent<WorldCleanupChecks>().arrivalOnly=true;EditorApplication.delayCall+=()=>EditorApplication.isPlaying=true;return "One isolated Anderson arrival check scheduled";}
}
