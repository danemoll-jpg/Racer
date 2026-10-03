using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;
public static class Report072CaveFind { public static void Run(){var o=Environment.GetEnvironmentVariable("PROBE_OUT");Directory.CreateDirectory(o);var rows=new List<string>();
 foreach(var scene in new[]{"LakeWoods","MountainLoop"}){EditorSceneManager.OpenScene("Assets/Scenes/"+scene+".unity");var race=UnityEngine.Object.FindAnyObjectByType<Racer.RaceDirector>();race.road.Initialize();
  var pts=UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None).Where(r=>r.name.Contains("Cave gravel floor")||r.name.Contains("Echo Cave")||r.name.Contains("cave puddle")).Select(r=>r.bounds.center).ToList();
  if(pts.Count==0){rows.Add(scene+": none");continue;}var c=pts.Aggregate(Vector3.zero,(a,b)=>a+b)/pts.Count;float s=race.road.Project(c,out float lat);rows.Add($"{scene}: {pts.Count} cave pieces, centre {c:F1}, main road s {s:F0} lateral {lat:F1}");
  foreach(var b in race.Branches??new Racer.WoodlandRoute[0]){if(!b)continue;float bs=b.Project(c,out float bl);rows.Add($"   branch {b.title}: s {bs:F0} lateral {bl:F1} length {b.Length:F0}");}
  foreach(var p in pts.Take(6))rows.Add($"   piece {p:F1}");}
 File.WriteAllLines(o+"/cave.txt",rows);EditorApplication.Exit(0);}}
