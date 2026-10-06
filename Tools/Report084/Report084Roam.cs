using System;using System.IO;using System.Linq;using System.Text;using System.Collections.Generic;using UnityEditor;using UnityEditor.SceneManagement;using UnityEngine;using Racer;using Object=UnityEngine.Object;
// 0.84 Part C: FreeRoamWorld survey for new homes of the Forest opening jump and Forest speed traps 1 and 2 (read only).
public static class Report084Roam {
 static string P(Transform t){var n=t.name;while(t.parent){t=t.parent;n=t.name+"/"+n;}return n.Replace('\n','/');}
 static string V(Vector3 v)=>$"{v.x:F1},{v.y:F2},{v.z:F1}";
 // Part E: every merged box mesh that looks like a fence (name or material: fence, rail, picket, post, gate, paddock, corral),
 // in every course scene and FreeRoamWorld: path, vertices, boxes, material, colliders, breakable, bounds.
 public static void Fences(){var o=Environment.GetEnvironmentVariable("PROBE_OUT");var sb=new StringBuilder();var words=new[]{"fence","rail","picket","post","gate","paddock","corral"};
  try{foreach(var scene in RacePlaylists.Scenes.Concat(new[]{"FreeRoamWorld"}).Distinct()){var s=EditorSceneManager.OpenScene("Assets/Scenes/"+scene+".unity");int n=0;
    foreach(var r in s.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<MeshRenderer>(true))){var mf=r.GetComponent<MeshFilter>();var m=mf?mf.sharedMesh:null;if(!m)continue;string name=P(r.transform).ToLowerInvariant(),mat=r.sharedMaterial?r.sharedMaterial.name.ToLowerInvariant():"";
     if(!words.Any(w=>name.Contains(w)||mat.Contains(w)))continue;if(name.Contains("signpost")||name.Contains("sign post")||name.Contains("lamp"))continue;
     sb.AppendLine($"{scene}\t{P(r.transform)}\tverts {m.vertexCount}\tboxes {(m.vertexCount%24==0?m.vertexCount/24:-1)}\tsub {m.subMeshCount}\treadable {m.isReadable}\tmat {(r.sharedMaterial?r.sharedMaterial.name:"-")}\tcol {string.Join(",",r.GetComponents<Collider>().Select(c=>c.GetType().Name))}\tbreak {r.GetComponent<BreakableProp>()!=null}\tactive {r.gameObject.activeInHierarchy&&r.enabled}\tcentre {V(r.bounds.center)}\tsize {V(r.bounds.size)}");n++;}
    sb.AppendLine($"== {scene}: {n}");}
  }catch(Exception e){sb.AppendLine("ERROR "+e);}
  Directory.CreateDirectory(o);File.WriteAllText(o+"/E-fences.txt",sb.ToString());EditorApplication.Exit(0);}
 // Part F: the rock SceneryProps leaves alone (names with cave, vault, portal, enclosed) in the Forest and Mountain race
 // scenes: path, vertices, material, collider on the same object, bounds.
 public static void Caves(){var o=Environment.GetEnvironmentVariable("PROBE_OUT");var sb=new StringBuilder();var words=new[]{"cave","vault","portal","enclosed"};
  try{foreach(var scene in new[]{"LakeWoods","ForestLoopReverse","MountainLoop","MountainLoopReverse"}){var s=EditorSceneManager.OpenScene("Assets/Scenes/"+scene+".unity");int n=0;
    foreach(var r in s.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<MeshRenderer>(true))){var mf=r.GetComponent<MeshFilter>();var m=mf?mf.sharedMesh:null;if(!m)continue;string name=P(r.transform).ToLowerInvariant();
     if(!words.Any(name.Contains))continue;
     sb.AppendLine($"{scene}\t{P(r.transform)}\tverts {m.vertexCount}\tmesh {m.name} ({AssetDatabase.GetAssetPath(m)})\treadable {m.isReadable}\tmat {(r.sharedMaterial?r.sharedMaterial.name+"/"+r.sharedMaterial.shader.name:"-")}\tcol {string.Join(",",r.GetComponents<Collider>().Select(c=>c.GetType().Name+(c is MeshCollider mc&&mc.sharedMesh==m?"(same mesh)":"")))}\tactive {r.gameObject.activeInHierarchy&&r.enabled}\tcentre {V(r.bounds.center)}\tsize {V(r.bounds.size)}");n++;}
    sb.AppendLine($"== {scene}: {n}");}
  }catch(Exception e){sb.AppendLine("ERROR "+e);}
  Directory.CreateDirectory(o);File.WriteAllText(o+"/F-caves.txt",sb.ToString());EditorApplication.Exit(0);}
 public static void Survey(){var o=Environment.GetEnvironmentVariable("PROBE_OUT");var sb=new StringBuilder();
  try{EditorSceneManager.OpenScene("Assets/Scenes/FreeRoamWorld.unity");Physics.SyncTransforms();
   var roads=Object.FindObjectsByType<RaceRoad>(FindObjectsSortMode.None).Where(r=>r.points!=null&&r.points.Length>1).ToList();foreach(var r in roads)r.Initialize();
   foreach(var r in roads.Where(r=>r.name.Contains("Phase 3")||r.name.Contains("highway"))){sb.AppendLine($"== {P(r.transform)} {r.Length:F0} m");
    for(float s=0;s<r.Length;s+=25){var p=r.At(s,out var f);r.At(s+40,out var f2);var q=r.At(s+40,out _);sb.AppendLine($"  s {s:F0} {V(p)} heading {Mathf.Atan2(f.x,f.z)*Mathf.Rad2Deg:F0} turn40 {Vector3.Angle(new Vector3(f.x,0,f.z),new Vector3(f2.x,0,f2.z)):F0} grade40 {(q.y-p.y)/40*100:F1}% hw {r.HalfWidth(s):F1}");}}
   foreach(var c in Object.FindObjectsByType<Collider>(FindObjectsSortMode.None).Where(c=>c.name.StartsWith("Takeoff")||c.name.Contains("supported ramp"))){var b=c.bounds;
    var near=roads.Select(r=>{float s=r.Project(b.center,out float lat);return (r,s,lat);}).OrderBy(x=>x.lat).First();near.r.At(near.s,out var rf);
    sb.AppendLine($"LAUNCH {P(c.transform)} centre {V(b.center)} size {V(b.size)} fwd {V(c.transform.forward)} up {V(c.transform.up)}; nearest road {near.r.name} s {near.s:F0} {near.lat:F1} m off, road heading {Mathf.Atan2(rf.x,rf.z)*Mathf.Rad2Deg:F0}");
    if(c is MeshCollider mc&&mc.sharedMesh&&mc.sharedMesh.isReadable){var v=mc.sharedMesh.vertices.Select(x=>c.transform.TransformPoint(x)).ToArray();var lo=v.OrderBy(x=>x.y).First();var hi=v.OrderByDescending(x=>x.y).First();sb.AppendLine($"   lowest {V(lo)} highest {V(hi)}");}}
  }catch(Exception e){sb.AppendLine("ERROR "+e);}
  Directory.CreateDirectory(o);File.WriteAllText(o+"/C-roam.txt",sb.ToString());EditorApplication.Exit(0);}
}
