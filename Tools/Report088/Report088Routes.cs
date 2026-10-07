using System;using System.IO;using System.Linq;using System.Text;using UnityEditor;using UnityEditor.SceneManagement;using UnityEngine;using Racer;
// 0.85 (read only): a scene's race data as text: course id, gates (position, station), branches (entry/exit, length, AI,
// points), ForestLayout jumps, and the main sampled every ROUTE_STEP m (default 10). PROBE_SCENES=a,b
public static class Report088Routes {
 static string V(Vector3 v)=>$"{v.x:F1},{v.y:F2},{v.z:F1}";
 static string P(Transform t){var n=t.name;while(t.parent){t=t.parent;n=t.name+"/"+n;}return n;}
 public static void Run(){var o=Environment.GetEnvironmentVariable("PROBE_OUT");Directory.CreateDirectory(o);float step=float.Parse(Environment.GetEnvironmentVariable("ROUTE_STEP")??"10",System.Globalization.CultureInfo.InvariantCulture);
  foreach(var scene in Environment.GetEnvironmentVariable("PROBE_SCENES").Split(',')){var s=EditorSceneManager.OpenScene("Assets/Scenes/"+scene+".unity");var roots=s.GetRootGameObjects();var sb=new StringBuilder();
   foreach(var d in roots.SelectMany(g=>g.GetComponentsInChildren<RaceDirector>(true))){var rd=d.road;rd.Initialize();
    sb.AppendLine($"course {d.courseId} road {P(rd.transform)} length {rd.Length:F1} points {rd.points.Length} forestTrail {rd.forestTrail}");
    for(int i=0;i<d.gates.Length;i++){var g=d.gates[i].transform;float st=rd.Project(g.position,out float lat);sb.AppendLine($"gate {i} {P(g)} at {V(g.position)} yaw {g.eulerAngles.y:F0} scale {g.lossyScale} station {st:F1} lat {lat:F1}");}
    foreach(var b in roots.SelectMany(x=>x.GetComponentsInChildren<WoodlandRoute>(true))){b.Initialize();
     sb.AppendLine($"branch '{b.title}' {P(b.transform)} active {b.gameObject.activeInHierarchy} entry {b.entryRoad:F1} exit {b.exitRoad:F1} length {b.Length:F1} hw {b.halfWidth} ai {b.aiValidated} speed {b.recommendedSpeed} bypassed [{string.Join(",",b.bypassedGates??new int[0])}] entryInset {b.entryInset} first {V(b.points[0])} last {V(b.points[^1])}");
     sb.AppendLine("   pts "+string.Join(" ",b.points.Select(V)));}
    foreach(var l in roots.SelectMany(x=>x.GetComponentsInChildren<ForestLayout>(true)))for(int i=0;i<l.jumpStarts.Length;i++)sb.AppendLine($"jump {l.jumpNames[i]} {l.jumpStarts[i]:F1}..{l.jumpEnds[i]:F1} at {V(rd.At(l.jumpStarts[i],out _))}");
    for(float st=0;st<rd.Length;st+=step){var p=rd.At(st,out var f);sb.AppendLine($"main {st:F0} {V(p)} hdg {(Mathf.Atan2(f.x,f.z)*Mathf.Rad2Deg+360)%360:F0}");}}
   File.WriteAllText(o+"/routes-"+scene+".txt",sb.ToString());}
  EditorApplication.Exit(0);}
}
