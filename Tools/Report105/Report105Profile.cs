using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;
// 0.105 Part A: profiles along Hwy 92 at the flagged places (PROBE_PLAY "x0,x1;x0,x1"), and the road meshes' vertices there
public static class Report105Profile {
 static float F(string s)=>float.Parse(s,System.Globalization.CultureInfo.InvariantCulture);
 public static void Run(){
  var outDir=Environment.GetEnvironmentVariable("PROBE_OUT");
  foreach(var sn in Environment.GetEnvironmentVariable("PROBE_SCENES").Split(',')){
   var log=new List<string>();EditorSceneManager.OpenScene("Assets/Scenes/"+sn+".unity");Physics.SyncTransforms();
   var road=UnityEngine.Object.FindObjectsByType<Racer.RaceRoad>(FindObjectsSortMode.None).First(r=>r.openHighway&&r.name.Contains("CR113"));road.Initialize();
   foreach(var rg in Environment.GetEnvironmentVariable("PROBE_PLAY").Split(';')){var a=rg.Split(',');float x0=F(a[0]),x1=F(a[1]);
    log.Add($"==== x {x0}..{x1}");
    var cols=new HashSet<Collider>();
    foreach(float off in new[]{-6.15f,-2.05f,2.05f,6.15f}){log.Add($"-- lane offset {off}");
     for(float s=0;s<road.Length;s+=.05f){var p=road.At(s,out var f);if(p.x<x0||p.x>x1)continue;f.y=0;f.Normalize();var q=p+Vector3.Cross(Vector3.up,f)*off;
      var hs=Physics.RaycastAll(q+Vector3.up*1.5f,Vector3.down,5,~0,QueryTriggerInteraction.Ignore).OrderByDescending(h=>h.point.y).ToArray();if(hs.Length==0)continue;cols.Add(hs[0].collider);
      if(Mathf.Repeat(s,.25f)<.05f)log.Add($"  s {s:F2} ({hs[0].point.x:F2},{hs[0].point.z:F2}) y {hs[0].point.y:F3} {hs[0].collider.name}"+(hs.Length>1?$" | {hs[1].collider.name} {hs[1].point.y:F3}":""));}}
    foreach(var c in cols){var mf=c.GetComponent<MeshFilter>();var mc=c as MeshCollider;var mesh=mc?mc.sharedMesh:null;log.Add($"collider {c.name} mesh {(mesh?mesh.name+" "+AssetDatabase.GetAssetPath(mesh)+" verts "+mesh.vertexCount:"-")}");
     if(mesh&&mesh.isReadable){var v=mesh.vertices;for(int i=0;i<v.Length;i++){var w=c.transform.TransformPoint(v[i]);if(w.x>=x0-3&&w.x<=x1+3&&Mathf.Abs(w.z-road.At(road.Project(w,out _),out _).z)<12)log.Add($"   v{i} ({w.x:F2},{w.y:F3},{w.z:F2})");}}}
   }
   File.WriteAllLines(Path.Combine(outDir,"profile-"+sn+".txt"),log);
  }EditorApplication.Exit(0);}
}
