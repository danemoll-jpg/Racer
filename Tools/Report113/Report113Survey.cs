using System;using System.IO;using System.Linq;using System.Text;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;
// 0.113 Part C (read-only): the Trickum Rd / S Cherokee Ln band as data for planning its roadside (like 0.104/0.105 Report105Grid + Protect):
// per scene (PROBE_SCENES): every RaceRoad's points inside the band; a 2 m grid (ground height, top height, top category) over the band
// (first scene only); everything standing in the band that is not ground (path, centre, radius, solid/trigger/visual, active).
public static class Report113Survey {
 public const float X0=-780,X1=-150,Z0=-660,Z1=560;
 static string P(Transform t){var s=t.name;while(t.parent){t=t.parent;s=t.name+"/"+s;}return s;}
 static readonly string[] Skip={"Memory loop","PrototypeCar","Chase Camera","Sun","Phase 6 - architectural","World edge"};
 public static void Run(){
  var outDir=Environment.GetEnvironmentVariable("PROBE_OUT");bool first=true;
  foreach(var sn in Environment.GetEnvironmentVariable("PROBE_SCENES").Split(',')){
   EditorSceneManager.OpenScene("Assets/Scenes/"+sn+".unity");Physics.SyncTransforms();
   var roads=new List<string>();
   foreach(var r in UnityEngine.Object.FindObjectsByType<Racer.RaceRoad>(FindObjectsInactive.Include,FindObjectsSortMode.None)){
    var pts=r.points.Where(p=>p.x>=X0&&p.x<=X1&&p.z>=Z0&&p.z<=Z1).ToArray();if(pts.Length<2)continue;
    roads.Add($"# {P(r.transform)} forestTrail {r.forestTrail} openHighway {r.openHighway} active {r.gameObject.activeInHierarchy}");
    for(int i=0;i<r.points.Length;i++){var p=r.points[i];if(p.x>=X0-40&&p.x<=X1+40&&p.z>=Z0-40&&p.z<=Z1+40)roads.Add($"{i} {p.x:F3} {p.y:F3} {p.z:F3}");}}
   foreach(var b in UnityEngine.Object.FindObjectsByType<Racer.WoodlandRoute>(FindObjectsInactive.Include,FindObjectsSortMode.None)){
    if(!b.points.Any(p=>p.x>=X0&&p.x<=X1&&p.z>=Z0&&p.z<=Z1))continue;roads.Add($"# branch {b.title}");for(int i=0;i<b.points.Length;i++)roads.Add($"{i} {b.points[i].x:F3} {b.points[i].y:F3} {b.points[i].z:F3}");}
   File.WriteAllLines(Path.Combine(outDir,"roads-"+sn+".txt"),roads);
   var log=new List<string>();var seen=new HashSet<string>();
   foreach(var go in UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include,FindObjectsSortMode.None).Select(t=>t.gameObject)){
    var ren=go.GetComponent<Renderer>();var col=go.GetComponent<Collider>();bool special=go.GetComponent<Racer.ActivitySite>()||go.GetComponent<Racer.RaceGate>()||go.GetComponent<Racer.PhysicalSign>();
    if(!ren&&!col&&!special)continue;
    var path=P(go.transform);if(Skip.Any(s=>path.StartsWith(s)))continue;
    if(col&&col.name.StartsWith("Ground_"))continue;
    var b=ren?ren.bounds:col?col.bounds:new Bounds(go.transform.position,Vector3.one);if(b.max.x<X0||b.min.x>X1||b.max.z<Z0||b.min.z>Z1)continue;
    bool big=b.size.x>200||b.size.z>200;
    var key=path+$"@{Mathf.Round(b.center.x)},{Mathf.Round(b.center.z)}";if(!seen.Add(key))continue;
    log.Add($"{b.center.x:F1}\t{b.center.y:F1}\t{b.center.z:F1}\t{b.extents.x:F1}\t{b.extents.z:F1}\t{b.extents.y:F1}\t{(col&&!col.isTrigger?"solid":col?"trigger":special?"marker":"visual")}\t{(go.activeInHierarchy?"on":"off")}{(big?"\tBIG":"")}\t{path.Replace('\n',' ')}");
   }
   File.WriteAllLines(Path.Combine(outDir,"objects-"+sn+".txt"),log);
   if(first){first=false;var sb=new StringBuilder();var cats=new Dictionary<string,int>();
    for(float z=Z0;z<=Z1;z+=2){var line=new StringBuilder();
     for(float x=X0;x<=X1;x+=2){var hits=Physics.RaycastAll(new Vector3(x,400,z),Vector3.down,600,~0,QueryTriggerInteraction.Ignore);float ground=float.NaN,top=float.NaN;string cat="-";
      foreach(var h in hits.OrderBy(h=>h.distance)){if(h.collider.attachedRigidbody)continue;var path=P(h.collider.transform);if(float.IsNaN(top)){top=h.point.y;var parts=path.Split('/');cat=parts[0]+(parts.Length>1?"/"+parts[1]:"");}if(float.IsNaN(ground)&&h.collider.name.StartsWith("Ground_"))ground=h.point.y;}
      if(!cats.TryGetValue(cat,out var ci)){ci=cats.Count;cats[cat]=ci;}
      line.Append(float.IsNaN(ground)?"nan":ground.ToString("F2")).Append(',').Append(float.IsNaN(top)?"nan":top.ToString("F2")).Append(',').Append(ci).Append(' ');}
     sb.AppendLine(line.ToString());}
    File.WriteAllText(Path.Combine(outDir,"grid-"+sn+".txt"),sb.ToString());File.WriteAllLines(Path.Combine(outDir,"cats-"+sn+".txt"),cats.OrderBy(k=>k.Value).Select(k=>k.Value+"\t"+k.Key));}
  }
  EditorApplication.Exit(0);}
}
