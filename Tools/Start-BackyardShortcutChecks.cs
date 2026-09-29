using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using Racer;
public static class StartBackyardShortcutChecks {
 public static string Main(){if(EditorApplication.isCompiling||Application.isPlaying)throw new Exception("Compiled edit mode required");if(UnityEngine.Object.FindObjectsByType<BackyardShortcutChecks>().Length>0)throw new Exception("Fixture already present; inspect instead of duplicating");File.WriteAllText("Docs/BackyardShortcuts/fixture-started.txt",DateTime.UtcNow.ToString("o"));AudioListener.volume=0;foreach(var t in UnityEngine.Object.FindObjectsByType<ContinuationTraffic>())t.enabled=false;new GameObject("Temporary bounded shortcut fixture").AddComponent<BackyardShortcutChecks>().Only="Tree-Top Trail";EditorApplication.delayCall+=()=>EditorApplication.isPlaying=true;return "Scheduled muted checks; temporary fixture and disabled traffic will not be saved";}
}


