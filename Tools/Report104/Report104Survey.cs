using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;
// 0.104 survey: what stands along Hwy 92 in each scene (renderers and colliders in the band around the road), terrains, roads.
public static class Report104Survey {
 static string P(Transform t){var s=t.name;while(t.parent){t=t.parent;s=t.name+"/"+s;}return s;}
 static string R(Vector3 v)=>$"({v.x:F1},{v.y:F1},{v.z:F1})";
 public static void Run(){
  var outDir=Environment.GetEnvironmentVariable("PROBE_OUT");
  float x0=-1100,x1=1100,z0=330,z1=800;
  foreach(var sn in Environment.GetEnvironmentVariable("PROBE_SCENES").Split(',')){
   var log=new List<string>();EditorSceneManager.OpenScene("Assets/Scenes/"+sn+".unity");
   foreach(var t in UnityEngine.Object.FindObjectsByType<Terrain>(FindObjectsInactive.Include,FindObjectsSortMode.None))log.Add($"TERRAIN {P(t.transform)} pos {R(t.transform.position)} size {R(t.terrainData.size)} res {t.terrainData.heightmapResolution} active {t.gameObject.activeInHierarchy}");
   foreach(var r in UnityEngine.Object.FindObjectsByType<Racer.RaceRoad>(FindObjectsInactive.Include,FindObjectsSortMode.None)){var pts=r.points??new Vector3[0];log.Add($"RACEROAD {P(r.transform)} open {r.openHighway} trail {r.forestTrail} pts {pts.Length} first {(pts.Length>0?R(pts[0]):"")} last {(pts.Length>0?R(pts[^1]):"")}");}
   foreach(var g in EditorSceneManager.GetActiveScene().GetRootGameObjects())log.Add($"ROOT {g.name} active {g.activeSelf} children {g.transform.childCount}");
   var agg=new Dictionary<string,int>();var big=new List<string>();
   foreach(var go in UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include,FindObjectsSortMode.None).Select(t=>t.gameObject)){
    var ren=go.GetComponent<Renderer>();var col=go.GetComponent<Collider>();if(!ren&&!col)continue;if(go.GetComponent<Terrain>())continue;
    var b=ren?ren.bounds:col.bounds;if(b.max.x<x0||b.min.x>x1||b.max.z<z0||b.min.z>z1)continue;
    if(b.size.x>600||b.size.z>600){big.Add($"HUGE {P(go.transform)} {R(b.min)}-{R(b.max)}");continue;}
    var p=P(go.transform).Split('/');var key=string.Join("/",p.Take(Math.Min(2,p.Length)))+(p.Length>2?"/…":"");
    agg[key]=agg.TryGetValue(key,out var n)?n+1:1;
    var foot=Mathf.Max(b.size.x,b.size.z);
    if(foot>=4&&!(go.name.StartsWith("Tree")||go.name.Contains("tree")))big.Add($"{P(go.transform)} | {R(b.center)} size {R(b.size)} ren {(ren?ren.enabled+"":"-")} col {(col?col.GetType().Name+(col.isTrigger?"T":"")+(col.enabled?"":"off"):"-")} act {go.activeInHierarchy}");
   }
   log.Add("== groups (count of renderers/colliders in band)");foreach(var kv in agg.OrderByDescending(k=>k.Value))log.Add($"  {kv.Value,6} {kv.Key}");
   log.Add("== objects with footprint >= 4 m");log.AddRange(big.OrderBy(s=>s));
   File.WriteAllLines(Path.Combine(outDir,"survey-"+sn+".txt"),log);
  }
  EditorApplication.Exit(0);}
}
