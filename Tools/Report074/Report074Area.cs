using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using Object=UnityEngine.Object;
// 0.74 read-only: a height grid (top collider hit, first letter of its name) over a box, then every renderer whose name or
// path matches PROBE_MATCH (regex) with bounds in the box, then every RaceRoad passing through the box (station ranges).
// PROBE_AREA="scene|x0,z0,x1,z1|step"
public static class Report074Area {
 static string Path(Transform t){var n=t.name;while(t.parent){t=t.parent;n=t.name+"/"+n;}return n.Replace('\n','/');}
 public static void Run(){var outDir=Environment.GetEnvironmentVariable("PROBE_OUT");Directory.CreateDirectory(outDir);var rows=new List<string>();string open=null;
  var match=new System.Text.RegularExpressions.Regex(Environment.GetEnvironmentVariable("PROBE_MATCH")??"(?i)ramp|house|drive|jump|lip|kicker",System.Text.RegularExpressions.RegexOptions.IgnoreCase);
  foreach(var spec in Environment.GetEnvironmentVariable("PROBE_AREA").Split(';')){var a=spec.Split('|');if(a[0]!=open){EditorSceneManager.OpenScene("Assets/Scenes/"+a[0]+".unity");open=a[0];Physics.SyncTransforms();}
   var b=a[1].Split(',').Select(float.Parse).ToArray();float st=float.Parse(a[2]);rows.Add($"AREA {spec}");
   var head="z\\x  ";for(float x=b[0];x<=b[2]+.01f;x+=st)head+=$"{x,7:F0}";rows.Add(head);
   for(float z=b[3];z>=b[1]-.01f;z-=st){var line=$"{z,6:F0} ";for(float x=b[0];x<=b[2]+.01f;x+=st){string cell="     --";
     foreach(var h in Physics.RaycastAll(new Vector3(x,400,z),Vector3.down,800,~0,QueryTriggerInteraction.Ignore).OrderBy(h=>h.distance)){if(h.collider.attachedRigidbody)continue;var n=h.collider.name;if(n.IndexOf("trunk",StringComparison.OrdinalIgnoreCase)>=0||n.IndexOf("tree",StringComparison.OrdinalIgnoreCase)>=0)continue;
      char c=n.StartsWith("Ground_")?(n.Length>7?n[7]:'G'):n[0];cell=$"{h.point.y,6:F1}{c}";break;}line+=cell;}rows.Add(line);}
   var box=new Bounds(new Vector3((b[0]+b[2])/2,100,(b[1]+b[3])/2),new Vector3(b[2]-b[0],400,b[3]-b[1]));
   foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include,FindObjectsSortMode.None)){var p=Path(r.transform);if(!match.IsMatch(p))continue;if(!r.bounds.Intersects(box))continue;
    rows.Add($"OBJ {(r.gameObject.activeInHierarchy?"A":"-")}{(r.enabled?"R":"-")}{(r.GetComponent<Collider>()?"C":"-")} {p} c {r.bounds.center:F1} s {r.bounds.size:F1}");}
   foreach(var road in Object.FindObjectsByType<Racer.RaceRoad>(FindObjectsInactive.Include,FindObjectsSortMode.None)){if(road.points==null||road.points.Length<2)continue;road.Initialize();
    var inside=new List<float>();for(float s=0;s<road.Length;s+=2){var q=road.At(s,out _);if(q.x>=b[0]&&q.x<=b[2]&&q.z>=b[1]&&q.z<=b[3])inside.Add(s);}
    if(inside.Count>0)rows.Add($"ROAD {(road.gameObject.activeInHierarchy?"A":"-")} {Path(road.transform)} s {inside.First():F0}..{inside.Last():F0} ({inside.Count*2} m) at start {road.At(inside.First(),out _):F1} end {road.At(inside.Last(),out _):F1}");}
   foreach(var wr in Object.FindObjectsByType<Racer.WoodlandRoute>(FindObjectsInactive.Include,FindObjectsSortMode.None)){if(wr.points==null||wr.points.Length<2)continue;
    var pts=wr.points.Where(q=>q.x>=b[0]&&q.x<=b[2]&&q.z>=b[1]&&q.z<=b[3]).ToList();if(pts.Count==0)continue;
    rows.Add($"WROUTE {(wr.gameObject.activeInHierarchy?"A":"-")} {Path(wr.transform)} '{wr.title}' hw {wr.halfWidth} n {pts.Count}: "+string.Join(" ",pts.Where((q,i)=>i%Math.Max(1,pts.Count/14)==0).Select(q=>$"({q.x:F0},{q.y:F0},{q.z:F0})")));}
   if(Environment.GetEnvironmentVariable("PROBE_ROADPTS")=="1")foreach(var road in Object.FindObjectsByType<Racer.RaceRoad>(FindObjectsInactive.Include,FindObjectsSortMode.None)){if(road.points==null)continue;var pts=road.points.Where(q=>q.x>=b[0]&&q.x<=b[2]&&q.z>=b[1]&&q.z<=b[3]).ToList();if(pts.Count==0)continue;
    rows.Add($"RPTS {Path(road.transform)} n {pts.Count}: "+string.Join(" ",pts.Where((q,i)=>i%Math.Max(1,pts.Count/20)==0).Select(q=>$"({q.x:F0},{q.y:F0},{q.z:F0})")));}}
  File.WriteAllLines(outDir+"/area.txt",rows);EditorApplication.Exit(0);}
}
