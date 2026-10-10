using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;
// 0.102 "nothing else changed": for each scene, every collider (path, type, bounds to 5 cm) and every route's points, the committed scene
// (copied by the runner into Assets/_Compare102) against the working one. Lists what differs, with where it is.
public static class Report102Compare {
 static string P(Transform t){var s=t.name;while(t.parent){t=t.parent;s=t.name+"/"+s;}return s;}
 static Dictionary<string,List<string>> Snap(string path){var d=new Dictionary<string,List<string>>();EditorSceneManager.OpenScene(path);Physics.SyncTransforms();
  void Add(string k,string v){if(!d.TryGetValue(k,out var l))d[k]=l=new();l.Add(v);}
  foreach(var c in UnityEngine.Object.FindObjectsByType<Collider>(FindObjectsInactive.Include,FindObjectsSortMode.None)){var b=c.bounds;Add("C "+P(c.transform)+" "+c.GetType().Name,$"{R(b.min)}-{R(b.max)} {c.enabled&&c.gameObject.activeInHierarchy} {(c is MeshCollider mc&&mc.sharedMesh?mc.sharedMesh.vertexCount:0)}");}
  foreach(var w in UnityEngine.Object.FindObjectsByType<Racer.WoodlandRoute>(FindObjectsInactive.Include,FindObjectsSortMode.None))Add("W "+P(w.transform),string.Join(";",(w.points??new Vector3[0]).Select(R))+$" {w.entryRoad:F1} {w.exitRoad:F1} {w.gameObject.activeInHierarchy}");
  foreach(var r in UnityEngine.Object.FindObjectsByType<Racer.RaceRoad>(FindObjectsInactive.Include,FindObjectsSortMode.None))Add("R "+P(r.transform),string.Join(";",(r.points??new Vector3[0]).Select(R)));
  return d;}
 static string R(Vector3 v)=>$"({Mathf.Round(v.x*20)/20:F2},{Mathf.Round(v.y*20)/20:F2},{Mathf.Round(v.z*20)/20:F2})";
 public static void Run(){var log=new List<string>();
  foreach(var sn in Environment.GetEnvironmentVariable("PROBE_SCENES").Split(',')){
   var a=Snap("Assets/_Compare102/"+sn+".unity");var b=Snap("Assets/Scenes/"+sn+".unity");int same=0;var diff=new List<string>();
   foreach(var k in a.Keys.Union(b.Keys).OrderBy(x=>x)){a.TryGetValue(k,out var va);b.TryGetValue(k,out var vb);va??=new();vb??=new();var sa=va.OrderBy(x=>x).ToList();var sb=vb.OrderBy(x=>x).ToList();
    if(sa.SequenceEqual(sb)){same+=sa.Count;continue;}var gone=sa.Except(sb).ToList();var added=sb.Except(sa).ToList();same+=sa.Intersect(sb).Count();
    foreach(var g in gone)diff.Add($"  - {k}: {(g.Length>160?g.Substring(0,160)+"...":g)}");foreach(var g in added)diff.Add($"  + {k}: {(g.Length>160?g.Substring(0,160)+"...":g)}");}
   log.Add($"===== {sn}: {same} colliders/routes identical, {diff.Count} lines differ");log.AddRange(diff);}
  File.WriteAllLines(Environment.GetEnvironmentVariable("PROBE_OUT")+"/compare.txt",log);EditorApplication.Exit(0);}
}
