using System;using UnityEngine;using UnityEditor;using Racer;
public static class StopShortcutFixture{public static string Main(){foreach(var f in UnityEngine.Object.FindObjectsByType<BackyardShortcutChecks>()){f.StopAllCoroutines();UnityEngine.Object.DestroyImmediate(f.gameObject);}EditorApplication.ExitPlaymode();return "Temporary fixtures removed; exit Play mode requested";}}
