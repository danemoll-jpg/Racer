using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.SceneManagement;
using Racer;
using Object=UnityEngine.Object;
public static class InspectStructuralRoads {
 public static string Main(){var rows=new List<string>();
 foreach(var scene in new[]{"MountainLoop","MountainLoopReverse","StreetLoopReverse"}){
 EditorSceneManager.OpenScene("Assets/Scenes/"+scene+".unity");Physics.SyncTransforms();var race=Object.FindAnyObjectByType<RaceDirector>();race.road.Initialize();rows.Add(scene+" road length="+race.road.Length);
 foreach(var f in race.GetComponent<MountainFlights>()?.flights??Array.Empty<MountainFlights.Flight>())rows.Add($"Flight {f.name}: start={f.start} lip={f.lip} end={f.landingEnd} dir={f.forward}");
 foreach(var b in Object.FindObjectsByType<WoodlandRoute>()){b.Initialize();rows.Add($"Branch {b.title}: len={b.Length} start={b.points[0]} end={b.points[^1]} width={b.halfWidth}");}
 foreach(var m in Object.FindObjectsByType<MeshCollider>().Where(m=>m.name.StartsWith("Ground_CR122")||m.name.Contains("Laurel")||m.name.Contains("Summit authored"))){var v=m.sharedMesh.vertices;rows.Add($"Mesh {m.name}: vertices={v.Length} bounds={m.bounds} first={m.transform.TransformPoint(v[0])} last={m.transform.TransformPoint(v[^1])}");}
 }
 Directory.CreateDirectory("Docs/CR133-137");File.WriteAllLines("Docs/CR133-137/before.txt",rows);return string.Join("\n",rows);
 }
}
