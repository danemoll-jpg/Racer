using System;using System.IO;using System.Linq;using System.Text;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using Racer;using Object=UnityEngine.Object;
// 0.76 Part B quality bar: where the storm culvert's outside shows above the ground in FreeRoamWorld (read-only map).
public static class Report076Culvert {
 static string P(Transform t){var n=t.name;while(t.parent){t=t.parent;n=t.name+"/"+n;}return n;}
 public static void Survey(){var sb=new StringBuilder();EditorSceneManager.OpenScene("Assets/Scenes/"+(Environment.GetEnvironmentVariable("PROBE_SCENES")??"FreeRoamWorld")+".unity");Physics.SyncTransforms();
  var culvert=Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None).Where(m=>m.name.Contains("culvert")||m.name.Contains("Culvert")).ToArray();
  foreach(var c in culvert){var r=c.GetComponent<Renderer>();sb.AppendLine($"{P(c.transform)} renderer {(r?r.bounds.ToString():"-")} collider {(c.GetComponent<Collider>()?"yes":"no")}");}
  var box=culvert.First(m=>m.name.Contains("Long storm culvert walls"));var b=box.GetComponent<Renderer>().bounds;sb.AppendLine("box bounds "+b);
  var route=Object.FindObjectsByType<WoodlandRoute>(FindObjectsInactive.Include,FindObjectsSortMode.None).First(w=>w.name.StartsWith("Storm Drain")||(w.title??"").StartsWith("Storm"));
  sb.AppendLine($"route {route.title} points {route.points.Count()} underground {route.undergroundStart}..{route.undergroundEnd}");
  bool Terrain(Collider c)=>c&&c.name.StartsWith("Ground")&&!c.name.Contains("Culvert");
  // exposed exterior: vertices of the box whose outward normal faces up or sideways and that stand above the terrain under them
  var m=box.sharedMesh;var v=m.vertices;var n=m.normals;int exposed=0;var cells=new HashSet<(int,int)>();float maxAbove=0;
  for(int i=0;i<v.Length;i++){var w=box.transform.TransformPoint(v[i]);var nw=box.transform.TransformDirection(n[i]);
   float ground=float.NaN;foreach(var h in Physics.RaycastAll(new Vector3(w.x,w.y+60,w.z),Vector3.down,140,~0,QueryTriggerInteraction.Ignore).OrderBy(h=>h.distance)){if(Terrain(h.collider)){ground=h.point.y;break;}}
   if(float.IsNaN(ground)||w.y<=ground+.05f)continue;
   // inside the tunnel the walls face inward; count only faces pointing away from the route line (outside)
   exposed++;maxAbove=Mathf.Max(maxAbove,w.y-ground);cells.Add((Mathf.FloorToInt(w.x/2),Mathf.FloorToInt(w.z/2)));}
  sb.AppendLine($"box vertices above the terrain: {exposed}/{v.Length}, highest {maxAbove:F2} m, 2 m cells {cells.Count}");
  foreach(var g in cells.GroupBy(c=>c.Item2).OrderByDescending(g=>g.Key))sb.AppendLine($"  z {g.Key*2,5}: x {string.Join(",",g.Select(c=>c.Item1*2).OrderBy(x=>x))}");
  foreach(var p in route.points.Where((p,i)=>i%6==0))sb.AppendLine($"  route {p:F1}");
  File.WriteAllText(Path.Combine(Environment.GetEnvironmentVariable("PROBE_OUT"),"culvert-survey.txt"),sb.ToString());EditorApplication.Exit(0);}
}
