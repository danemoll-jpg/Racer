using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;
// 0.113 Parts A / D (read-only): the ground under each shortcut's AI line (PROBE_SCENES), every 2 m: line height, the top solid surface within
// +-25 m of it (name), the grade of that surface along the line, the line's lateral distance to the nearest other route / road, and the
// nearby RaceRoads (non-course), the jump exclusions and the guidance components. Nothing is saved.
public static class Report113Lines {
 static string V(Vector3 v)=>$"{v.x:F1},{v.y:F2},{v.z:F1}";
 public static void Run(){
  var outDir=Environment.GetEnvironmentVariable("PROBE_OUT");
  foreach(var sn in Environment.GetEnvironmentVariable("PROBE_SCENES").Split(',')){
   var log=new List<string>();EditorSceneManager.OpenScene("Assets/Scenes/"+sn+".unity");Physics.SyncTransforms();
   var race=UnityEngine.Object.FindAnyObjectByType<Racer.RaceDirector>();
   log.Add($"scene {sn}: course road '{(race&&race.road?race.road.name:"-")}' length {(race&&race.road?race.road.Length:0):F0}");
   foreach(var z in UnityEngine.Object.FindObjectsByType<Racer.JumpRecoveryExclusion>(FindObjectsSortMode.None))log.Add($"exclusion '{z.name}' {V(z.start)} -> {V(z.end)} half {z.halfWidth}");
   foreach(var r in UnityEngine.Object.FindObjectsByType<Racer.RaceRoad>(FindObjectsSortMode.None)){if(r==race?.road)continue;r.Initialize();var p=r.points;log.Add($"road '{r.name}' ({r.transform.parent?.name}) {p.Length} pts {V(p[0])} .. {V(p[p.Length/2])} .. {V(p[^1])}");}
   foreach(var b in UnityEngine.Object.FindObjectsByType<Racer.WoodlandRoute>(FindObjectsSortMode.None)){
    b.Initialize();var g=b.GetComponent<Racer.ReverseShortcutGuidance>();
    log.Add($"==== route '{b.title}' length {b.Length:F0} entry {b.entryRoad:F0} exit {b.exitRoad:F0} half {b.halfWidth} rec {b.recommendedSpeed} entrySpeed {b.entrySpeed}/{b.entrySpeedDistance} validated {b.aiValidated} guidance {(g?$"look {g.lookAhead} takeoff {g.takeoff} landing {g.landing}":"none")}");
    float prevTop=float.NaN;
    for(float s=0;s<=b.Length;s+=2){
     var p=b.At(s,out var f);
     var hs=Physics.RaycastAll(p+Vector3.up*25,Vector3.down,50,~0,QueryTriggerInteraction.Ignore).Where(h=>!h.collider.attachedRigidbody).OrderByDescending(h=>h.point.y).ToArray();
     // the surface nearest the line height (not a roof above it)
     var near=hs.OrderBy(h=>Mathf.Abs(h.point.y-p.y)).FirstOrDefault();
     float top=hs.Length>0?near.point.y:float.NaN;string what=hs.Length>0?near.collider.name+(hs.Length>1?$" (+{hs.Length-1} more, top {hs[0].collider.name} {hs[0].point.y:F1})":""):"NOTHING";
     float grade=float.IsNaN(prevTop)?0:(top-prevTop)/2;prevTop=top;
     float ny=hs.Length>0?near.normal.y:0;
     var fl=f;fl.y=0;fl.Normalize();var right=Vector3.Cross(Vector3.up,fl);
     // side ground (cross slope) 3 m each side
     float Side(float d){var q=p+right*d;var h=Physics.RaycastAll(q+Vector3.up*25,Vector3.down,50,~0,QueryTriggerInteraction.Ignore).Where(x=>!x.collider.attachedRigidbody).OrderBy(x=>Mathf.Abs(x.point.y-p.y)).FirstOrDefault();return h.collider?h.point.y:float.NaN;}
     log.Add($"  s {s,5:F0} line {V(p)} ground {top:F2} (line-ground {p.y-top:+0.00;-0.00}) grade {grade:+0.00;-0.00} normalY {ny:F2} sides {Side(-3)-top:+0.0;-0.0}/{Side(3)-top:+0.0;-0.0} {what}");
    }
   }
   File.WriteAllLines(Path.Combine(outDir,"lines-"+sn+".txt"),log);
  }EditorApplication.Exit(0);}
}
