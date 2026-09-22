using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Racer;
using Object=UnityEngine.Object;
public static class VerifyStructuralRoads {
 public static string Forward()=>Check("MountainLoop");
 public static string Reverse()=>Check("MountainLoopReverse");
 public static string Laurel()=>Check("StreetLoopReverse");
 static string Check(string scene){EditorSceneManager.OpenScene("Assets/Scenes/"+scene+".unity");Physics.SyncTransforms();var rows=new List<string>();var failures=new List<string>();var race=Object.FindAnyObjectByType<RaceDirector>();int samples=0;float worst=0;
 var courses=new List<(string name,Vector3[] p,float width)>();if(scene!="StreetLoopReverse")courses.Add(("main",race.road.points,3));foreach(var b in Object.FindObjectsByType<WoodlandRoute>().Where(b=>scene!="StreetLoopReverse"||b.title=="Laurel Switchbacks"))courses.Add((b.title,b.points,Mathf.Min(2.5f,b.halfWidth-1)));
 foreach(var course in courses){int before=failures.Count;for(int i=1;i<course.p.Length;i++){var a=course.p[i-1];var b=course.p[i];float length=Vector3.Distance(a,b);if(length>10){if(course.name=="Summit Traverse"||course.name=="Climbing Ridge Cut")failures.Add("Unexpected route discontinuity "+course.name+" "+length);rows.Add($"{course.name} authored flight interval: {a} -> {b}, {length:F2}m");continue;}if(i%3!=0&&i>4&&i<course.p.Length-5)continue;var f=(b-a).normalized;var side=Vector3.Cross(Vector3.up,f).normalized;foreach(float lane in new[]{-course.width,0,course.width}){var p=(a+b)*.5f+side*lane;var hits=Physics.RaycastAll(p+Vector3.up*3,Vector3.down,6,1,QueryTriggerInteraction.Ignore).Where(h=>h.collider.name.StartsWith("Ground_")&&h.normal.y>.35f).OrderBy(h=>Math.Abs(h.point.y-p.y)).ToArray();samples++;if(hits.Length==0||Math.Abs(hits[0].point.y-p.y)>(lane==0?.4f:.12f+Math.Abs(lane)*.45f)){if(failures.Count<100)failures.Add($"{course.name} sample {i} lane={lane} at {p}: "+string.Join(";",hits.Select(h=>$"{h.collider.name} y={h.point.y:F3}")));continue;}worst=Mathf.Max(worst,Math.Abs(hits[0].point.y-p.y));int near=hits.Count(h=>Math.Abs(h.point.y-hits[0].point.y)<.015f);if(near>1&&failures.Count<100)failures.Add($"{course.name} duplicate support {i} lane={lane}: "+string.Join(";",hits.Take(near).Select(h=>h.collider.name)));}}rows.Add($"{course.name}: support failures={failures.Count-before}");}
 int meshSamples=0;foreach(var mf in Object.FindObjectsByType<MeshFilter>().Where(m=>m.name.StartsWith("Ground_CR133"))){var mesh=mf.sharedMesh;var c=mf.GetComponent<MeshCollider>();if(!c||c.sharedMesh!=mesh)throw new Exception("Mesh/collider mismatch "+mf.name);if(mesh.vertices.Any(v=>!float.IsFinite(v.x)||!float.IsFinite(v.y)||!float.IsFinite(v.z)))throw new Exception("Nonfinite vertex");var v=mesh.vertices;var ix=mesh.triangles;for(int i=0;i<ix.Length;i+=90){var p=mf.transform.TransformPoint((v[ix[i]]+v[ix[i+1]]+v[ix[i+2]])/3);if(!c.Raycast(new Ray(p+Vector3.up*.2f,Vector3.down),out var hit,.4f)||Math.Abs(hit.point.y-p.y)>.015f){if(failures.Count<100)failures.Add($"Mesh support miss {mf.name} triangle {i/3}");}meshSamples++;}}
 rows.Add($"Route/lane samples={samples}; mesh/collider centroid samples={meshSamples}; maximum route height deviation={worst:F4}m; failures={failures.Count}");rows.AddRange(failures);File.WriteAllLines("Docs/CR133-137/"+scene+"-checks.txt",rows);return string.Join("\n",rows);
 }

}



