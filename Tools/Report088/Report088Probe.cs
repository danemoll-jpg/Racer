using System;using System.IO;using System.Linq;using System.Text;using System.Collections.Generic;using UnityEditor;using UnityEditor.SceneManagement;using UnityEngine;using Racer;
// 0.88 Part A (read only): terrain-only heights round Kyle's house (house frame, drive/gravel meshes ignored), the street's
// centre and half width near the new drive mouth, every Ground hit along the BUG-002 line, trees within 45 m.
public static class Report088Probe {
 static string V(Vector3 v)=>$"{v.x:F2},{v.y:F2},{v.z:F2}";
 static string P(Transform t){var n=t.name;while(t.parent){t=t.parent;n=t.name+"/"+n;}return n;}
 public static void Run(){var o=Environment.GetEnvironmentVariable("PROBE_OUT");Directory.CreateDirectory(o);
  foreach(var scene in Environment.GetEnvironmentVariable("PROBE_SCENES").Split(',')){var sb=new StringBuilder();
   var s=EditorSceneManager.OpenScene("Assets/Scenes/"+scene+".unity");Physics.SyncTransforms();var roots=s.GetRootGameObjects();
   var site=roots.SelectMany(g=>g.GetComponentsInChildren<Transform>(true)).First(t=>t.name=="Friend across street - blue circle");
   bool Drive(string n)=>n.Contains("drive")||n.Contains("Kyle")||n.Contains("edge join");
   float T(float x,float z,out string what){var w=site.TransformPoint(new Vector3(x,0,z));float best=float.NaN;what="";foreach(var h in Physics.RaycastAll(new Vector3(w.x,w.y+60,w.z),Vector3.down,140,~0,QueryTriggerInteraction.Ignore)){if(!h.collider.name.StartsWith("Ground")||Drive(h.collider.name))continue;if(float.IsNaN(best)||h.point.y>best){best=h.point.y;what=h.collider.name;}}return best-site.position.y;}
   var g=new StringBuilder("# terrain only; x -20..40, z -12..44, 1 m; value or value@name-initial\n");var names=new Dictionary<string,char>();
   for(int z=44;z>=-12;z--){var row=new List<string>();for(int x=-20;x<=40;x++){float y=T(x,z,out var w);if(!names.ContainsKey(w))names[w]=(char)('A'+names.Count);row.Add($"{y:F2}{names[w]}");}g.AppendLine($"{z}:"+string.Join(",",row));}
   g.AppendLine(string.Join("; ",names.Select(k=>$"{k.Value}={k.Key}")));File.WriteAllText($"{o}/terrain-{scene}.txt",g.ToString());
   // street
   var street=roots.SelectMany(x=>x.GetComponentsInChildren<RaceRoad>(true)).FirstOrDefault(r=>r.name=="Phase 3 - Race Systems");
   if(street){street.Initialize();for(float st=0;st<street.Length;st+=1){var q=street.At(st,out var f);var l=site.InverseTransformPoint(q);if(l.x<0||l.x>45||l.z<15||l.z>60)continue;if(Mathf.Repeat(st,2)!=0)continue;sb.AppendLine($"street s {st:F0} {V(q)} local {V(l)} hw {street.HalfWidth(st):F2} fwd {V(f)}");}}
   // all hits along the BUG-002 line (local), every 1 m from 6 m road-side of the mouth to the apron
   var a=site.InverseTransformPoint(new Vector3(477.65f,0,-21.06f));var b=new Vector3(11.4f,0,-1.8f);a.y=0;
   for(float d=-8;d<=Vector3.Distance(a,b);d+=1){var l=a+(b-a).normalized*d;var w=site.TransformPoint(l);sb.Append($"line d {d:F0} local {l.x:F1},{l.z:F1} world {w.x:F1},{w.z:F1}:");foreach(var h in Physics.RaycastAll(new Vector3(w.x,w.y+60,w.z),Vector3.down,140,~0,QueryTriggerInteraction.Ignore).OrderByDescending(h=>h.point.y))sb.Append($" {h.collider.name}@{h.point.y-site.position.y:F2}");sb.AppendLine();}
   // trees
   foreach(var c in roots.SelectMany(x=>x.GetComponentsInChildren<Collider>(true))){var n=c.name.ToLowerInvariant();if(!(n.Contains("trunk")||n.Contains("tree"))||c.isTrigger)continue;var bb=c.bounds;var l=site.InverseTransformPoint(new Vector3(bb.center.x,bb.min.y,bb.center.z));if(l.x<-25||l.x>45||l.z<-15||l.z>48)continue;sb.AppendLine($"tree {P(c.transform)} {c.GetType().Name} local {V(l)} r {Mathf.Max(bb.extents.x,bb.extents.z):F2} h {bb.size.y:F1}");}
   File.WriteAllText($"{o}/probe-{scene}.txt",sb.ToString());}
  EditorApplication.Exit(0);}
}
