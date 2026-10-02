using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using Racer;using Object=UnityEngine.Object;
public static class RegressionNeighbors {
 public static void Run(){EditorSceneManager.OpenScene("Assets/Scenes/MountainLoopReverse.unity");Physics.SyncTransforms();Physics.queriesHitBackfaces=true;
  var race=Object.FindAnyObjectByType<RaceDirector>(FindObjectsInactive.Include);race.road.Initialize();var rows=new List<string>();
  var regions=new[]{new Bounds(new(990,130,-95),new(110,80,60)),new Bounds(new(990,165,80),new(70,80,110))};
  var routes=new List<(string n,Func<float,(Vector3,Vector3)> at,float len)>{("Main",s=>{var p=race.road.At(s,out var f);return(p,f);},race.road.Length)};
  foreach(var b in Object.FindObjectsByType<WoodlandRoute>(FindObjectsInactive.Include)){b.Initialize();var bb=b;routes.Add((b.title,s=>{var p=bb.At(s,out var f);return(p,f);},b.Length));}
  bool Ok(RaycastHit h)=>!h.collider.attachedRigidbody;
  foreach(var r in routes)for(float s=0;s<r.len;s+=1){var (p,f)=r.at(s);if(!regions.Any(g=>g.Contains(p)))continue;var right=Vector3.Cross(Vector3.up,f).normalized;var line=$"{r.n} s={s} p={p.ToString("F1")}";
   foreach(var o in new[]{-3f,0f,3f}){var q=p+right*o;var top=Physics.RaycastAll(q+Vector3.up*1.5f,Vector3.down,4,~0,QueryTriggerInteraction.Ignore).Where(Ok).OrderBy(h=>h.distance).FirstOrDefault();float y=top.collider?top.point.y:float.NaN;
    var b=new Vector3(q.x,float.IsNaN(y)?q.y:y,q.z);var up=Physics.RaycastAll(b+Vector3.up*.3f,Vector3.up,4.5f,~0,QueryTriggerInteraction.Ignore).Where(Ok).OrderBy(h=>h.distance).FirstOrDefault();var fw=Physics.RaycastAll(b+Vector3.up*1.2f,f,1.2f,~0,QueryTriggerInteraction.Ignore).Where(Ok).OrderBy(h=>h.distance).FirstOrDefault();
    var below=Physics.RaycastAll(b+Vector3.down*.05f,Vector3.down,30,~0,QueryTriggerInteraction.Ignore).Where(Ok).OrderBy(h=>h.distance).FirstOrDefault();
    line+=$" | o{o} top={(top.collider?top.collider.name+"@"+y.ToString("F2"):"NONE")} up={(up.collider?up.collider.name+"@"+up.distance.ToString("F1"):"-")} fw={(fw.collider?fw.collider.name:"-")} below={(below.collider?below.collider.name+"@"+below.distance.ToString("F1"):"-")}";}
   rows.Add(line);}
  File.WriteAllLines(Environment.GetEnvironmentVariable("PROBE_OUT")+"/neighbors.txt",rows);EditorApplication.Exit(0);}
}
