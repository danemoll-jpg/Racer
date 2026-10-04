using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using Object=UnityEngine.Object;
// 0.73 read-only (Part A3): a map of the right side of the road downhill from Dan's sled position. Along the road from the
// projected station (every 3 m), laterally from the road centre to the right (every 1 m): R = within the driving half
// width, T = a tree / prop / fence renderer within 1.4 m, digits = ground slope along the road (tenths), and the height.
public static class Report073Place {
 static string P(Transform t){var n=t.name;while(t.parent){t=t.parent;n=t.name+"/"+n;}return n;}
 public static void Run(){var outDir=Environment.GetEnvironmentVariable("PROBE_OUT");Directory.CreateDirectory(outDir);var rows=new List<string>();
  foreach(var scene in Environment.GetEnvironmentVariable("PROBE_SCENES").Split(',')){EditorSceneManager.OpenScene("Assets/Scenes/"+scene+".unity");Physics.SyncTransforms();rows.Add("SCENE "+scene);
   var dan=new Vector3(519.21f,74.77f,-153.47f);Racer.RaceRoad road=null;float best=1e9f;
   foreach(var r in Object.FindObjectsByType<Racer.RaceRoad>(FindObjectsInactive.Include,FindObjectsSortMode.None)){if(r.points==null||r.points.Length<2)continue;r.Initialize();r.Project(dan,out float l);rows.Add($"road {P(r.transform)} lateral={l:F1}");if(Mathf.Abs(l)<best){best=Mathf.Abs(l);road=r;}}
   rows.Add("using "+P(road.transform));float s0=road.Project(dan,out float lat0);var at0=road.At(s0,out var f0);
   rows.Add($"dan station={s0:F1} lateral={lat0:F2} halfWidth={road.HalfWidth(s0):F1} fwd={f0} roadLen={road.Length:F0}");
   var obst=Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None).Where(r=>r.enabled&&(r.bounds.center-dan).sqrMagnitude<120*120&&r.bounds.size.y>.6f&&r.bounds.size.x<30&&r.bounds.size.z<30).ToList();
   // direction of travel for Dan (heading 172) decides which way "ahead" runs along the road
   var hd=new Vector3(Mathf.Sin(172*Mathf.Deg2Rad),0,Mathf.Cos(172*Mathf.Deg2Rad));float dir=Vector3.Dot(f0,hd)>0?1:-1;rows.Add("dir="+dir);
   for(int k=-2;k<=24;k++){float s=s0+dir*k*3;var c=road.At(s,out var f);f*=dir;f.y=0;f.Normalize();var right=new Vector3(f.z,0,-f.x);float hw=road.HalfWidth(s);var line=new List<string>();
    for(int d=0;d<=16;d++){var p=c+right*d;float? g=null;string gn="";foreach(var h in Physics.RaycastAll(p+Vector3.up*60,Vector3.down,140,1,QueryTriggerInteraction.Ignore).OrderBy(h=>h.distance)){if(h.collider.attachedRigidbody)continue;g=h.point.y;gn=h.collider.name;break;}
     bool tree=obst.Any(r=>{var b=r.bounds;var q=new Vector3(Mathf.Clamp(p.x,b.min.x,b.max.x),0,Mathf.Clamp(p.z,b.min.z,b.max.z));return (new Vector3(p.x,0,p.z)-q).magnitude<1.4f&&b.min.y<(g??p.y)+2.5f&&!(P(r.transform).StartsWith("Phase")&&r.name.Contains("Road"));});
     line.Add((d<=hw?"R":tree?"T":".")+(g.HasValue?g.Value.ToString("F1"):"--"));}
    rows.Add($"k={k,3} s={s,7:F1} c=({c.x:F1},{c.z:F1}) hw={hw:F1} | "+string.Join(" ",line));}
   // what the T marks are near the lane
   var lanes=new List<string>();foreach(var r in obst){var b=r.bounds;if(Vector3.Distance(new Vector3(b.center.x,0,b.center.z),new Vector3(dan.x,0,dan.z))>80)continue;lanes.Add($"OBST {P(r.transform)} c=({b.center.x:F1},{b.min.y:F1},{b.center.z:F1}) s=({b.size.x:F1},{b.size.y:F1},{b.size.z:F1})");}
   rows.AddRange(lanes.Take(400));}
  File.WriteAllLines($"{outDir}/place.txt",rows);EditorApplication.Exit(0);}
}
