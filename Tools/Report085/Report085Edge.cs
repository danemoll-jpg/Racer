using System;using System.IO;using System.Linq;using System.Text;using System.Collections.Generic;using UnityEditor;using UnityEditor.SceneManagement;using UnityEngine;using Racer;
// 0.85 Part C (read only): cross-sections of a course's main road around a point. For every EDGE_STEP m of the main
// within EDGE_SPAN m of the point (EDGE="x,z,span,step"), the topmost drivable surface every 0.25 m from -16 to +16 m
// across the road (+ = right of travel), the collider hit at each, and every step of more than 0.15 m between
// neighbouring samples (a lip) or a gap (no surface). PROBE_SCENES=scene
public static class Report085Edge {
 static bool Skip(Collider c)=>c.isTrigger||c.name.IndexOf("trunk",StringComparison.OrdinalIgnoreCase)>=0||c.name.IndexOf("tree",StringComparison.OrdinalIgnoreCase)>=0||(c.attachedRigidbody&&!c.attachedRigidbody.isKinematic);
 static string P(Transform t){var n=t.name;while(t.parent){t=t.parent;n=t.name+"/"+n;}return n;}
 public static float Top(Vector3 p,float near,out Collider what){what=null;float best=float.NaN,bd=float.MaxValue;
  // the surface nearest the road height (not a bridge or overhang far above or a cave below)
  foreach(var h in Physics.RaycastAll(new Vector3(p.x,near+12,p.z),Vector3.down,40,~0,QueryTriggerInteraction.Ignore)){if(Skip(h.collider))continue;float d=Mathf.Abs(h.point.y-near);if(h.point.y>near+3)d+=100;if(d<bd){bd=d;best=h.point.y;what=h.collider;}}return best;}
 public static void Run(){var o=Environment.GetEnvironmentVariable("PROBE_OUT");Directory.CreateDirectory(o);var f=Environment.GetEnvironmentVariable("EDGE").Split(',').Select(x=>float.Parse(x,System.Globalization.CultureInfo.InvariantCulture)).ToArray();
  foreach(var scene in Environment.GetEnvironmentVariable("PROBE_SCENES").Split(',')){var s=EditorSceneManager.OpenScene("Assets/Scenes/"+scene+".unity");Physics.SyncTransforms();
   var race=s.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<RaceDirector>(true)).First();var rd=race.road;rd.Initialize();
   float s0=rd.Project(new Vector3(f[0],0,f[1]),out float lat0);var sb=new StringBuilder($"{scene}: point {f[0]},{f[1]} at main station {s0:F1} ({lat0:F1} m off the centre line); road length {rd.Length:F0}\n");
   var names=new Dictionary<Collider,int>();
   for(float st=s0-f[2];st<=s0+f[2];st+=f[3]){var c=rd.At(st,out var fw);fw.y=0;fw.Normalize();var right=Vector3.Cross(Vector3.up,fw);float hw=rd.HalfWidth(st);
    var row=new StringBuilder();var steps=new List<string>();float prev=float.NaN;Collider prevC=null;
    for(float x=-16;x<=16.001f;x+=.25f){var p=c+right*x;float y=Top(p,c.y,out var col);
     if(col!=null&&!names.ContainsKey(col))names[col]=names.Count;
     if(Mathf.Abs(x%2f)<.01f)row.Append(float.IsNaN(y)?"   -  ":$"{y-c.y,6:F2}");
     if(!float.IsNaN(prev)&&!float.IsNaN(y)&&Mathf.Abs(y-prev)>.15f)steps.Add($"{x-.125f:F2}m: {(y>prev?"+":"")}{y-prev:F2} ({(prevC?names[prevC]:-1)}->{(col?names[col]:-1)})");
     if(float.IsNaN(y)&&!float.IsNaN(prev))steps.Add($"{x:F2}m: GAP");prev=y;prevC=col;}
    sb.AppendLine($"s {st,7:F1} c {c.x:F1},{c.y:F2},{c.z:F1} hdg {(Mathf.Atan2(fw.x,fw.z)*Mathf.Rad2Deg+360)%360:F0} hw {hw:F1} | {row}");
    if(steps.Count>0)sb.AppendLine("           steps: "+string.Join("; ",steps));}
   sb.AppendLine("columns: -16..+16 m every 2 m, height relative to the centre line point");
   foreach(var kv in names.OrderBy(k=>k.Value))sb.AppendLine($"collider {kv.Value}: {P(kv.Key.transform)} [{kv.Key.GetType().Name}]{(kv.Key is MeshCollider mc&&mc.sharedMesh?" "+AssetDatabase.GetAssetPath(mc.sharedMesh):"")}");
   File.WriteAllText(o+"/edge-"+scene+".txt",sb.ToString());}
  EditorApplication.Exit(0);}
}
