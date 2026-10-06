using System;using System.IO;using System.Linq;using System.Text;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using Object=UnityEngine.Object;
// 0.83 survey (read only): the forward driving-surface mesh around BUG-002 and the deck edges around BUG-001.
public static class Report083Survey {
 public static void Run(){var o=Environment.GetEnvironmentVariable("PROBE_OUT");var sb=new StringBuilder();
  try{EditorSceneManager.OpenScene("Assets/Scenes/MountainLoop.unity");Physics.SyncTransforms();
   var mf=Object.FindObjectsByType<MeshFilter>().First(m=>m.name=="Ground_CR133 mountain driving surface");var mesh=mf.sharedMesh;var tr=mf.transform;
   sb.AppendLine($"mesh {AssetDatabase.GetAssetPath(mesh)} verts {mesh.vertexCount} subs {mesh.subMeshCount}");
   var v=mesh.vertices.Select(x=>tr.TransformPoint(x)).ToArray();var t=mesh.triangles;
   bool In(Vector3 p)=>p.x>975&&p.x<996&&p.z>-128&&p.z<-110;
   for(int i=0;i<v.Length;i++)if(In(v[i]))sb.AppendLine($"v {i} {v[i].x:F3} {v[i].y:F3} {v[i].z:F3}");
   for(int i=0;i<t.Length;i+=3)if(In(v[t[i]])||In(v[t[i+1]])||In(v[t[i+2]]))sb.AppendLine($"t {t[i]} {t[i+1]} {t[i+2]}");
   var cA=new Vector3(1255,152,231);var col=mf.GetComponent<MeshCollider>();sb.AppendLine("collider mesh "+(col?AssetDatabase.GetAssetPath(col.sharedMesh)+" same "+(col.sharedMesh==mesh):"none"));
   for(int i=0;i<t.Length;i+=3){var a=v[t[i]];var b=v[t[i+1]];var c=v[t[i+2]];if(Mathf.Min((a-cA).magnitude,Mathf.Min((b-cA).magnitude,(c-cA).magnitude))>12&&((a+b+c)/3-cA).magnitude>12)continue;var n=Vector3.Cross(b-a,c-a).normalized;sb.AppendLine($"A tri {i/3} n {n.x:F3},{n.y:F3},{n.z:F3} | {a.x:F2},{a.y:F3},{a.z:F2} | {b.x:F2},{b.y:F3},{b.z:F2} | {c.x:F2},{c.y:F3},{c.z:F2}");}
   var road=Object.FindObjectsByType<Racer.RaceDirector>().First().road;road.Initialize();sb.AppendLine("road "+road.name+" len "+road.Length);
   for(float s=1830;s<=1975;s+=1){var c=road.At(s,out var f);f.y=0;f.Normalize();
    foreach(int side in new[]{-1,1}){var r=Vector3.Cross(Vector3.up,f)*side;float y0=c.y;float w=0;float prev=y0;string what="";
     for(;w<16;w+=.1f){var p=c+r*w;if(!Physics.Raycast(new Vector3(p.x,y0+3,p.z),Vector3.down,out var h,8,~0,QueryTriggerInteraction.Ignore)){what="nothing";break;}if(prev-h.point.y>.25f){what=$"drop {prev-h.point.y:F2} to {h.collider.name}";break;}prev=h.point.y;}
     sb.AppendLine($"edge s {s} {(side<0?"L":"R")} hw {road.HalfWidth(s):F1} at {w:F1} m {what} c {c.x:F1},{c.y:F2},{c.z:F1}");}}
  }catch(Exception e){sb.AppendLine("ERROR "+e);}
  File.WriteAllText(o+"/survey.txt",sb.ToString());EditorApplication.Exit(0);}
}
