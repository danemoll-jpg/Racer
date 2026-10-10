using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;
// Temporary 0.101 inspection (copied into Assets/Editor/Report101Temp only while it runs). Writes lists to PROBE_OUT.
public static class Report101Inspect {
 static string Out=>Environment.GetEnvironmentVariable("PROBE_OUT");
 static string Path(Transform t){var s=t.name;while(t.parent){t=t.parent;s=t.name+"/"+s;}return s;}
 static string V(Vector3 v)=>$"({v.x:F2},{v.y:F2},{v.z:F2})";
 static float Ground(Vector3 p,out string name){name="none";float best=float.NaN;foreach(var h in Physics.RaycastAll(new Vector3(p.x,p.y+40,p.z),Vector3.down,200,~0,QueryTriggerInteraction.Ignore)){if(h.collider.attachedRigidbody)continue;if(float.IsNaN(best)||h.point.y>best){best=h.point.y;name=h.collider.name;}}return best;}
 static float Terrain(Vector3 p){float best=float.NaN;foreach(var h in Physics.RaycastAll(new Vector3(p.x,p.y+40,p.z),Vector3.down,200,~0,QueryTriggerInteraction.Ignore))if(h.collider.name.StartsWith("Ground_")&&(float.IsNaN(best)||h.point.y>best))best=h.point.y;return best;}
 // PROBE_SCENES: list. Cabin branch dump + objects near the cabin, and objects in the box around Dan's kennel/garage.
 public static void Run(){
  var log=new List<string>();
  foreach(var scene in Environment.GetEnvironmentVariable("PROBE_SCENES").Split(',')){
   EditorSceneManager.OpenScene("Assets/Scenes/"+scene+".unity");Physics.SyncTransforms();log.Add("===== "+scene);
   var race=UnityEngine.Object.FindAnyObjectByType<Racer.RaceDirector>();var road=race?race.road:null;if(road)road.Initialize();
   foreach(var w in UnityEngine.Object.FindObjectsByType<Racer.WoodlandRoute>(FindObjectsInactive.Include,FindObjectsSortMode.None)){
    log.Add($"route '{w.title}' {Path(w.transform)} active {w.gameObject.activeInHierarchy} pts {w.points?.Length} len {(w.points!=null&&w.points.Length>1?w.Length:0):F1} hw {w.halfWidth} entryRoad {w.entryRoad:F1} exitRoad {w.exitRoad:F1} rec {w.recommendedSpeed} entrySpeed {w.entrySpeed}/{w.entrySpeedDistance} inset {w.entryInset} margin {w.entryMargin} ai {w.aiValidated}");
    if(w.title==null||!w.title.Contains("Cabin"))continue;
    float acc=0;for(int i=0;i<w.points.Length;i++){if(i>0)acc+=Vector2.Distance(new(w.points[i].x,w.points[i].z),new(w.points[i-1].x,w.points[i-1].z));log.Add($"   pt{i} s{acc:F1} {V(w.points[i])}");}
    for(float s=0;s<=w.Length;s+=2){var p=w.At(s,out var f);float hd=Mathf.Atan2(f.x,f.z)*Mathf.Rad2Deg;float g=Ground(p,out var n);float tg=Terrain(p);string mainInfo="";if(road){float t=road.Project(p,out float lat);mainInfo=$" main s {t:F1} lat {lat:F1} mainY {road.At(t,out _).y:F2}";}
     log.Add($"   s{s,5:F1} {V(p)} head {hd:F1} top {g:F2} [{n}] terrain {tg:F2}{mainInfo}");}
    if(road){for(float t=w.entryRoad-90;t<=w.entryRoad+30;t+=5){var p=road.At(t,out var f);log.Add($"   main s{t:F1} {V(p)} head {Mathf.Atan2(f.x,f.z)*Mathf.Rad2Deg:F1} hw {road.HalfWidth(t):F1}");}
     for(float t=w.exitRoad-10;t<=w.exitRoad+10;t+=5){var p=road.At(t,out var f);log.Add($"   main(exit) s{t:F1} {V(p)} head {Mathf.Atan2(f.x,f.z)*Mathf.Rad2Deg:F1}");}}
    var c0=w.At(30,out _);
    foreach(var r in UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(r=>Vector2.Distance(new(r.bounds.center.x,r.bounds.center.z),new(c0.x,c0.z))<60&&r.bounds.size.magnitude<200).OrderBy(r=>Path(r.transform)))
     {var mf=r.GetComponent<MeshFilter>();log.Add($"   R {Path(r.transform)} b {V(r.bounds.min)}-{V(r.bounds.max)} mesh {(mf&&mf.sharedMesh?mf.sharedMesh.name:"-")} col {(r.GetComponent<Collider>()?r.GetComponent<Collider>().GetType().Name:"-")}");}
    foreach(var u in UnityEngine.Object.FindObjectsByType<Racer.ShortcutUndergrowth>(FindObjectsInactive.Include,FindObjectsSortMode.None))log.Add($"   undergrowth {Path(u.transform)} radius {u.radius} centres {u.centres?.Length} first {(u.centres!=null&&u.centres.Length>0?V(u.centres[0]):"")} clearRoute {(u.clearRoute?u.clearRoute.title:"-")} clearFrom {u.clearFrom} hw {u.clearHalfWidth} brushFrom {u.brushFrom} mesh {(u.GetComponent<MeshFilter>()&&u.GetComponent<MeshFilter>().sharedMesh?u.GetComponent<MeshFilter>().sharedMesh.name:"-")}");
   }
   // Dan's house box
   var box=new Bounds(new Vector3(400,80,10),new Vector3(90,40,90));
   foreach(var t in UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(t=>box.Contains(t.position)&&(t.GetComponent<Renderer>()||t.GetComponent<Collider>()||t.childCount>0)).OrderBy(t=>Path(t)).Take(1500)){
    var r=t.GetComponent<Renderer>();var c=t.GetComponent<Collider>();var mf=t.GetComponent<MeshFilter>();
    log.Add($"   H {Path(t)} pos {V(t.position)} rot {V(t.eulerAngles)} scale {V(t.lossyScale)} {(r?"b "+V(r.bounds.min)+"-"+V(r.bounds.max):"")} mesh {(mf&&mf.sharedMesh?mf.sharedMesh.name:"-")} mat {(r&&r.sharedMaterial?r.sharedMaterial.name:"-")} col {(c?c.GetType().Name:"-")} kids {t.childCount}");}
  }
  Directory.CreateDirectory(Out);File.WriteAllLines(Out+"/inspect.txt",log);EditorApplication.Exit(0);}
}
