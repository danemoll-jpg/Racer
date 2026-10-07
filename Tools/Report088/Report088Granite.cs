using System;using System.IO;using System.Linq;using System.Text;using System.Collections.Generic;using UnityEditor;using UnityEditor.SceneManagement;using UnityEngine;using Racer;
// 0.88 Part C (read only): the Granite Saddle line in ForestLoopReverse. Every metre along the branch: the centre point,
// the highest solid ground (non-trigger colliders, water excluded) on the centre and +-3 / +-6 / +-10 m across, the water
// surface if a water footprint covers it, the nearest main station and its distance, the nearest Fern Gully distance.
// Then water bodies, ground tiles (and which scenes share them), trees and other colliders within 25 m of the line from
// s 250 on (the lip, the flight and the landing side), and the House 3 driveway.
public static class Report088Granite {
 static string V(Vector3 v)=>$"{v.x:F2},{v.y:F2},{v.z:F2}";
 static string P(Transform t){var n=t.name;while(t.parent){t=t.parent;n=t.name+"/"+n;}return n;}
 public static void Run(){var o=Environment.GetEnvironmentVariable("PROBE_OUT");Directory.CreateDirectory(o);var scene=EditorSceneManager.OpenScene("Assets/Scenes/ForestLoopReverse.unity");Physics.SyncTransforms();var roots=scene.GetRootGameObjects();
  var all=roots.SelectMany(g=>g.GetComponentsInChildren<Transform>(true)).ToList();
  var gs=roots.SelectMany(g=>g.GetComponentsInChildren<WoodlandRoute>(true)).First(b=>b.title=="Granite Saddle");gs.Initialize();var fern=roots.SelectMany(g=>g.GetComponentsInChildren<WoodlandRoute>(true)).First(b=>b.title=="Fern Gully");fern.Initialize();
  var dir=roots.SelectMany(g=>g.GetComponentsInChildren<RaceDirector>(true)).First();var main=dir.road;main.Initialize();
  var waters=roots.SelectMany(g=>g.GetComponentsInChildren<ShallowWater>(true)).ToList();
  float Water(Vector3 p){float best=float.NaN;foreach(var w in waters){var l=w.transform.InverseTransformPoint(p);bool inside=w.round?new Vector2(l.x,l.z).magnitude<.5f:Mathf.Abs(l.x)<.5f&&Mathf.Abs(l.z)<.5f;if(!inside)continue;float y=w.transform.TransformPoint(new Vector3(l.x,.5f,l.z)).y;if(float.IsNaN(best)||y>best)best=y;}return best;}
  bool IsWater(Collider c){return c.GetComponentInParent<ShallowWater>()||c.name.Contains("water")||c.name.Contains("Water");}
  float Ground(Vector3 p,out string what){float best=float.NaN;what="";foreach(var h in Physics.RaycastAll(new Vector3(p.x,p.y+60,p.z),Vector3.down,160,~0,QueryTriggerInteraction.Ignore)){if(IsWater(h.collider))continue;if(float.IsNaN(best)||h.point.y>best){best=h.point.y;what=h.collider.name;}}return best;}
  var sb=new StringBuilder("s,x,y,z,hdg,ground,what,water,gL10,gL6,gL3,gR3,gR6,gR10,mainS,mainD,fernD\n");
  for(float s=0;s<=gs.Length;s+=1){var p=gs.At(s,out var f);var r=Vector3.Cross(Vector3.up,new Vector3(f.x,0,f.z).normalized);float g=Ground(p,out var what);var row=new List<string>();foreach(float l in new[]{-10f,-6f,-3f,3f,6f,10f})row.Add(Ground(p+r*l,out _).ToString("F2"));
   float ms=main.Project(p,out float ml);float fd=1e9f;foreach(var q in fern.points)fd=Mathf.Min(fd,Vector2.Distance(new(q.x,q.z),new(p.x,p.z)));
   sb.AppendLine($"{s:F0},{p.x:F2},{p.y:F2},{p.z:F2},{(Mathf.Atan2(f.x,f.z)*Mathf.Rad2Deg+360)%360:F0},{g:F2},{what},{Water(p):F2},{string.Join(",",row)},{ms:F0},{Mathf.Abs(ml):F1},{fd:F1}");}
  File.WriteAllText(o+"/C-granite-line.csv",sb.ToString());
  var rep=new StringBuilder();rep.AppendLine($"Granite Saddle length {gs.Length:F1} hw {gs.halfWidth} entryRoad {gs.entryRoad:F1} exitRoad {gs.exitRoad:F1} ai {gs.aiValidated} speed {gs.recommendedSpeed} bypassed [{string.Join(",",gs.bypassedGates)}] parent {P(gs.transform)}");
  foreach(var c in gs.GetComponents<Component>())rep.AppendLine("  component "+c.GetType().Name);
  var zone=new Bounds(new Vector3(380,50,-195),new Vector3(220,120,90));
  foreach(var w in waters){var b=new Bounds(w.transform.position,Vector3.Scale(w.transform.lossyScale,Vector3.one));if(!zone.Intersects(new Bounds(w.transform.position,w.transform.lossyScale)))continue;rep.AppendLine($"water {P(w.transform)} pos {V(w.transform.position)} scale {V(w.transform.lossyScale)} yaw {w.transform.eulerAngles.y:F0} round {w.round} surface {w.transform.TransformPoint(new Vector3(0,.5f,0)).y:F2}");}
  foreach(var mc in roots.SelectMany(g=>g.GetComponentsInChildren<MeshCollider>(true)).Where(c=>c.name.StartsWith("Ground")&&c.sharedMesh&&c.bounds.Intersects(zone))){var path=AssetDatabase.GetAssetPath(mc.sharedMesh);
   rep.AppendLine($"ground {P(mc.transform)} {path} verts {mc.sharedMesh.vertexCount} bounds {V(mc.bounds.min)}-{V(mc.bounds.max)} scenes {string.Join(",",AssetDatabase.FindAssets("t:Scene",new[]{"Assets/Scenes"}).Select(AssetDatabase.GUIDToAssetPath).Where(sp=>AssetDatabase.GetDependencies(sp,false).Contains(path)).Select(Path.GetFileNameWithoutExtension))}");}
  // colliders and renderers near the line beyond s 250
  var near=new List<Vector3>();for(float s=250;s<=Mathf.Min(gs.Length,400);s+=2)near.Add(gs.At(s,out _));
  float LineD(Vector3 p){float best=1e9f;foreach(var q in near)best=Mathf.Min(best,Vector2.Distance(new(q.x,q.z),new(p.x,p.z)));return best;}
  foreach(var c in roots.SelectMany(g=>g.GetComponentsInChildren<Collider>(true))){var b=c.bounds;if(!zone.Intersects(b)||b.size.x>40||b.size.z>40)continue;float d=LineD(b.center);if(d>25)continue;rep.AppendLine($"  collider {P(c.transform)} {c.GetType().Name} trig {c.isTrigger} at {V(b.center)} size {V(b.size)} lineD {d:F1}");}
  foreach(var rr in roots.SelectMany(g=>g.GetComponentsInChildren<RaceRoad>(true)).Where(r=>r!=main)){float bd=1e9f;Vector3 bp=default;foreach(var q in rr.points){float d=LineD(q);if(d<bd){bd=d;bp=q;}}if(bd<40)rep.AppendLine($"road {P(rr.transform)} nearest {bd:F1} at {V(bp)} points {rr.points.Length}");}
  for(int i=0;i<dir.gates.Length;i++)rep.AppendLine($"gate {i} at {V(dir.gates[i].transform.position)} lineD {LineD(dir.gates[i].transform.position):F1}");
  foreach(var l in roots.SelectMany(g=>g.GetComponentsInChildren<ForestLayout>(true)))for(int i=0;i<l.jumpStarts.Length;i++)rep.AppendLine($"jump {l.jumpNames[i]} {l.jumpStarts[i]:F1}..{l.jumpEnds[i]:F1}");
  File.WriteAllText(o+"/C-granite.txt",rep.ToString());EditorApplication.Exit(0);}
}
