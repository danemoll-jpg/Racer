using System;using System.IO;using System.Linq;using System.Text;using System.Collections.Generic;using UnityEditor;using UnityEditor.SceneManagement;using UnityEngine;
// 0.85 census (read only) for the "nothing else changed" comparison of a course scene: every collider (path, type, enabled,
// trigger, bounds, and for mesh colliders the mesh asset and a hash of its vertices and triangles), every route (main road,
// branches: point count and a hash of the points, entry/exit), gates, ForestLayout jumps, course id. PROBE_SCENES=a,b
public static class Report087Census {
 static string Path(Transform t){var n=t.name;while(t.parent){t=t.parent;n=t.name+"/"+n;}return n;}
 static string V(Vector3 v)=>$"{v.x:F3},{v.y:F3},{v.z:F3}";
 static string Hash(IEnumerable<Vector3> pts,IEnumerable<int> tris=null){var sb=new StringBuilder();foreach(var p in pts)sb.Append(V(p)).Append(';');if(tris!=null)foreach(var t in tris)sb.Append(t).Append(',');
  using var h=System.Security.Cryptography.SHA256.Create();return BitConverter.ToString(h.ComputeHash(Encoding.UTF8.GetBytes(sb.ToString()))).Replace("-","").Substring(0,16);}
 public static void Run(){var outDir=Environment.GetEnvironmentVariable("PROBE_OUT");Directory.CreateDirectory(outDir);
  foreach(var scene in Environment.GetEnvironmentVariable("PROBE_SCENES").Split(',')){
   var s=EditorSceneManager.OpenScene("Assets/Scenes/"+scene+".unity");var roots=s.GetRootGameObjects();
   var c=new StringBuilder("path\ttype\tenabled\ttrigger\tcenter\tsize\tmesh\tmeshHash\n");
   foreach(var col in roots.SelectMany(g=>g.GetComponentsInChildren<Collider>(true)).OrderBy(x=>Path(x.transform))){string mesh="",hash="";
    if(col is MeshCollider mc&&mc.sharedMesh){mesh=AssetDatabase.GetAssetPath(mc.sharedMesh)+"#"+mc.sharedMesh.name;hash=mc.sharedMesh.isReadable?Hash(mc.sharedMesh.vertices.Select(v=>mc.transform.TransformPoint(v)),mc.sharedMesh.triangles):"unreadable";}
    c.Append($"{Path(col.transform)}\t{col.GetType().Name}\t{col.enabled&&col.gameObject.activeInHierarchy}\t{col.isTrigger}\t{V(col.bounds.center)}\t{V(col.bounds.size)}\t{mesh}\t{hash}\n");}
   File.WriteAllText(outDir+"/"+scene+"-colliders.tsv",c.ToString());
   var r=new StringBuilder();
   foreach(var road in roots.SelectMany(g=>g.GetComponentsInChildren<Racer.RaceRoad>(true)))r.Append($"road\t{Path(road.transform)}\t{road.points.Length}\t{Hash(road.points)}\n");
   foreach(var b in roots.SelectMany(g=>g.GetComponentsInChildren<Racer.WoodlandRoute>(true)))r.Append($"branch\t{Path(b.transform)}\t{b.title}\t{b.points.Length}\t{Hash(b.points)}\tentry {b.entryRoad:F3}\texit {b.exitRoad:F3}\thw {b.halfWidth:F2}\n");
   foreach(var d in roots.SelectMany(g=>g.GetComponentsInChildren<Racer.RaceDirector>(true))){r.Append($"course\t{d.courseId}\n");for(int i=0;i<d.gates.Length;i++)r.Append($"gate\t{i}\t{Path(d.gates[i].transform)}\t{V(d.gates[i].transform.position)}\t{d.gates[i].transform.eulerAngles}\t{d.gates[i].transform.lossyScale}\n");}
   foreach(var l in roots.SelectMany(g=>g.GetComponentsInChildren<Racer.ForestLayout>(true)))for(int i=0;i<l.jumpStarts.Length;i++)r.Append($"jump\t{l.jumpNames[i]}\t{l.jumpStarts[i]:F3}\t{l.jumpEnds[i]:F3}\n");
   File.WriteAllText(outDir+"/"+scene+"-routes.tsv",r.ToString());
   Debug.Log("REPORT085 census "+scene);}
  EditorApplication.Exit(0);}
}
