using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;
// 0.115 (read-only, temporary): per scene, every course component as its serialized JSON (RaceDirector, RaceRoad, RaceGate,
// WoodlandRoute, MountainFlights, ReverseShortcutGuidance, JumpRecoveryExclusion, VehicleRespawn spawn) and ground probes:
// the street shortcuts' and street main road's points, raycast in their own scene and in the Mountain scenes. Scenes never saved.
public static class Report115Dump {
 static string P(Transform t){var s=t.name;while(t.parent){t=t.parent;s=t.name+"/"+s;}return s;}
 static string Q(string s)=>"\""+s.Replace("\\","\\\\").Replace("\"","\\\"")+"\"";
 static string V(Vector3 v)=>FormattableString.Invariant($"[{v.x:F3},{v.y:F3},{v.z:F3}]");
 static readonly Type[] Kinds={typeof(Racer.RaceDirector),typeof(Racer.RaceRoad),typeof(Racer.RaceGate),typeof(Racer.WoodlandRoute),typeof(Racer.MountainFlights),typeof(Racer.ReverseShortcutGuidance),typeof(Racer.JumpRecoveryExclusion),typeof(Racer.BackyardForwardCourse)};
 static string Hit(Vector3 p){p.y+=40;var hits=Physics.RaycastAll(p,Vector3.down,120,~0,QueryTriggerInteraction.Ignore).Where(h=>h.normal.y>.3f).OrderBy(h=>h.distance).ToArray();
  return "["+string.Join(",",hits.Take(3).Select(h=>FormattableString.Invariant($"[{h.point.y:F2},{h.normal.y:F3},{Q(P(h.collider.transform))}]")))+"]";}
 public static void Run(){
  var outDir=Environment.GetEnvironmentVariable("PROBE_OUT");Directory.CreateDirectory(outDir);
  try{
   var probes=new Dictionary<string,List<(string name,Vector3[] pts)>>();
   foreach(var sn in new[]{"StreetLoopGreybox","StreetLoopReverse","MountainLoop","MountainLoopReverse"}){
    var scene=EditorSceneManager.OpenScene("Assets/Scenes/"+sn+".unity");Physics.SyncTransforms();
    var o=new List<string>();
    foreach(var k in Kinds)foreach(var c in UnityEngine.Object.FindObjectsByType(k,FindObjectsInactive.Include,FindObjectsSortMode.None).Cast<Component>().OrderBy(c=>P(c.transform)))
     o.Add($"{{\"type\":{Q(k.Name)},\"path\":{Q(P(c.transform))},\"active\":{(c.gameObject.activeInHierarchy?"true":"false")},\"pos\":{V(c.transform.position)},\"rot\":{V(c.transform.eulerAngles)},\"scale\":{V(c.transform.lossyScale)},\"comps\":{Q(string.Join(";",c.GetComponents<Component>().Select(x=>x?x.GetType().Name:"null")))},\"children\":{c.transform.childCount},\"data\":{EditorJsonUtility.ToJson(c)}}}");
    foreach(var d in UnityEngine.Object.FindObjectsByType<Racer.RaceDirector>(FindObjectsInactive.Include,FindObjectsSortMode.None)){
     var r=d.vehicle?d.vehicle.GetComponent<Racer.VehicleRespawn>():null;var sp=r&&r.spawnPoint?r.spawnPoint:d.vehicle?d.vehicle.transform:null;
     o.Add($"{{\"type\":\"Director\",\"path\":{Q(P(d.transform))},\"road\":{Q(d.road?P(d.road.transform):"")},\"ambient\":{Q(d.ambientRoad?P(d.ambientRoad.transform):"")},\"through\":{Q(d.throughRoad?P(d.throughRoad.transform):"")},\"gates\":[{string.Join(",",d.gates.Select(g=>Q(P(g.transform))))}],\"spawn\":{(sp?V(sp.position):"null")},\"spawnYaw\":{(sp?sp.eulerAngles.y:0).ToString(System.Globalization.CultureInfo.InvariantCulture)},\"vehicle\":{Q(d.vehicle?P(d.vehicle.transform):"")},\"comps\":{Q(string.Join(";",d.GetComponents<Component>().Select(x=>x?x.GetType().Name:"null")))}}}");
     foreach(var g in d.gates){o.Add($"{{\"type\":\"GateTree\",\"path\":{Q(P(g.transform))},\"tree\":[{string.Join(",",g.GetComponentsInChildren<Transform>(true).Select(t=>Q(P(t)+" | "+string.Join(";",t.GetComponents<Component>().Select(x=>x?x.GetType().Name:"null"))+" | "+V(t.localPosition)+" "+V(t.localEulerAngles)+" "+V(t.localScale))))}]}}");}
    }
    File.WriteAllText(Path.Combine(outDir,"scene-"+sn+".json"),"[\n"+string.Join(",\n",o)+"\n]\n");
    // ground probes: this scene's own street roads / branches into the shared probe list (street scenes), then every list here
    if(sn.StartsWith("Street")){var list=new List<(string,Vector3[])>();
     foreach(var b in UnityEngine.Object.FindObjectsByType<Racer.WoodlandRoute>(FindObjectsInactive.Exclude,FindObjectsSortMode.None))list.Add((sn+"/branch/"+b.title,b.points));
     var dd=UnityEngine.Object.FindObjectsByType<Racer.RaceDirector>(FindObjectsInactive.Include,FindObjectsSortMode.None).First();list.Add((sn+"/main",dd.road.points.Where((p,i)=>i%4==0).ToArray()));probes[sn]=list;}
    var g2=new List<string>();
    foreach(var kv in probes)foreach(var (name,pts) in kv.Value)for(int i=0;i<pts.Length;i++)g2.Add($"{Q(name)},{i},{V(pts[i])},{Hit(pts[i])}");
    File.WriteAllLines(Path.Combine(outDir,"ground-"+sn+".txt"),g2);
   }
   EditorApplication.Exit(0);
  }catch(Exception e){File.WriteAllText(Path.Combine(outDir,"dump-error.txt"),e.ToString());EditorApplication.Exit(1);}
 }
}
