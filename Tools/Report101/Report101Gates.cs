using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;
public static class Report101Gates { public static void Run(){var log=new List<string>();EditorSceneManager.OpenScene("Assets/Scenes/DansBackyardForward.unity");
 var race=UnityEngine.Object.FindAnyObjectByType<Racer.RaceDirector>();race.road.Initialize();foreach(var g in race.gates)log.Add($"gate {g.name} main s {race.road.Project(g.transform.position,out _):F1}");
 foreach(var b in UnityEngine.Object.FindObjectsByType<Racer.WoodlandRoute>(FindObjectsSortMode.None))log.Add($"branch {b.title} entry {b.entryRoad:F1} exit {b.exitRoad:F1} bypass [{string.Join(",",b.bypassedGates)}] entrySpeed {b.entrySpeed}/{b.entrySpeedDistance} inset {b.entryInset}");
 File.WriteAllLines(Environment.GetEnvironmentVariable("PROBE_OUT")+"/gates.txt",log);EditorApplication.Exit(0);}}
