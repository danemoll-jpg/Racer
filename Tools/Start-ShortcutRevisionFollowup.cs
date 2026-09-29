using System;using UnityEngine;using UnityEditor;using Racer;
public static class StartShortcutRevisionFollowup {
 public static string Main(){if(EditorApplication.isCompiling||Application.isPlaying)throw new Exception("Compiled edit mode required");EditorSettings.enterPlayModeOptionsEnabled=false;AudioListener.volume=0;foreach(var t in UnityEngine.Object.FindObjectsByType<ContinuationTraffic>())t.enabled=false;new GameObject("Temporary shortcut follow-up checks").AddComponent<ShortcutRevisionFollowupChecks>();EditorApplication.delayCall+=()=>EditorApplication.isPlaying=true;return "Bounded follow-up only; cabin success and AI not repeated";}
}
