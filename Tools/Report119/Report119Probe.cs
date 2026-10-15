using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;
// 0.119 (read-only, temporary; scenes never saved): compile check, and probes for the round's checks.
public static class Report119Probe {
 public static void Compile(){ Debug.Log("REPORT119 compiled"); File.WriteAllText(Path.Combine(Environment.GetEnvironmentVariable("PROBE_OUT"),"compiled.txt"),"ok"); EditorApplication.Exit(0); }
}
