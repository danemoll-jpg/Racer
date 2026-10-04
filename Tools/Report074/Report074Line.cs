using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using Object=UnityEngine.Object;
// 0.74 read-only (BUG-003): the original winding House 3 driveway centre line (StreetLoopReverse "House 3 valley driveway"
// RaceRoad) and, in each scene of PROBE_SCENES, the ground along it every 2 m: top hit (no trees), its name, and the nearest
// other route point (RaceRoad / WoodlandRoute, not the driveway) with its height.
public static class Report074Line {
 static string Path(Transform t){var n=t.name;while(t.parent){t=t.parent;n=t.name+"/"+n;}return n.Replace('\n','/');}
 public static void Run(){var outDir=Environment.GetEnvironmentVariable("PROBE_OUT");Directory.CreateDirectory(outDir);var rows=new List<string>();
  EditorSceneManager.OpenScene("Assets/Scenes/StreetLoopReverse.unity");var src=Object.FindObjectsByType<Racer.RaceRoad>(FindObjectsInactive.Include,FindObjectsSortMode.None).First(r=>r.transform.parent&&r.name=="House 3 valley driveway");
  var pts=src.points.ToArray();File.WriteAllLines(outDir+"/winding-points.txt",pts.Select((p,i)=>$"{i} {p.x:F3} {p.y:F3} {p.z:F3}"));src.Initialize();float L=src.Length;rows.Add($"source points {pts.Length} length {L:F1}");
  var samples=new List<Vector3>();for(float s=0;s<=L;s+=2)samples.Add(src.At(s,out _));
  foreach(var scene in Environment.GetEnvironmentVariable("PROBE_SCENES").Split(',')){EditorSceneManager.OpenScene("Assets/Scenes/"+scene+".unity");Physics.SyncTransforms();rows.Add("SCENE "+scene);
   var other=new List<(Vector3 p,string n)>();
   foreach(var r in Object.FindObjectsByType<Racer.RaceRoad>(FindObjectsInactive.Include,FindObjectsSortMode.None))if(r.points!=null&&r.name!="House 3 valley driveway")other.AddRange(r.points.Select(p=>(p,r.name)));
   foreach(var w in Object.FindObjectsByType<Racer.WoodlandRoute>(FindObjectsInactive.Include,FindObjectsSortMode.None))if(w.points!=null)other.AddRange(w.points.Select(p=>(p,"W:"+w.title)));
   for(int i=0;i<samples.Count;i++){var p=samples[i];string g="--";
    foreach(var h in Physics.RaycastAll(new Vector3(p.x,300,p.z),Vector3.down,600,~0,QueryTriggerInteraction.Ignore).OrderBy(h=>h.distance)){if(h.collider.attachedRigidbody)continue;var n=h.collider.name;if(n.IndexOf("trunk",StringComparison.OrdinalIgnoreCase)>=0||n.IndexOf("tree",StringComparison.OrdinalIgnoreCase)>=0)continue;g=$"{h.point.y:F2} {n}";break;}
    var near=other.OrderBy(o=>new Vector2(o.p.x-p.x,o.p.z-p.z).sqrMagnitude).FirstOrDefault();float d=near.n==null?999:new Vector2(near.p.x-p.x,near.p.z-p.z).magnitude;
    rows.Add($"s {i*2,4} ({p.x:F1},{p.y:F1},{p.z:F1}) ground {g} | near {(d<15?$"{near.n} {d:F1}m y{near.p.y:F1}":"-")}");}}
  File.WriteAllLines(outDir+"/line.txt",rows);EditorApplication.Exit(0);}
}
