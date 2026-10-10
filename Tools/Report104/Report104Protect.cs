using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;
// 0.104: everything standing in the Hwy 92 band that the new roadside must keep clear of (signs, activity sites, collectibles, markers),
// i.e. every object with a renderer or collider that is not ground, tree, old business, lane paint or render batch; plus the ground mesh spacing.
public static class Report104Protect {
 static string P(Transform t){var s=t.name;while(t.parent){t=t.parent;s=t.name+"/"+s;}return s;}
 static readonly string[] Skip={"Memory loop","Woods replacing","World cleanup additional woodland","Remembered houses","Phase 6 - architectural","CR113 continuous","CR113 supported","Decorative road","PrototypeCar","Chase Camera","Sun"};
 public static void Run(){
  var outDir=Environment.GetEnvironmentVariable("PROBE_OUT");
  foreach(var sn in Environment.GetEnvironmentVariable("PROBE_SCENES").Split(',')){
   EditorSceneManager.OpenScene("Assets/Scenes/"+sn+".unity");var log=new List<string>();var seen=new HashSet<string>();
   foreach(var go in UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Exclude,FindObjectsSortMode.None).Select(t=>t.gameObject)){
    var ren=go.GetComponent<Renderer>();var col=go.GetComponent<Collider>();if(!ren&&!col)continue;
    var path=P(go.transform);if(Skip.Any(s=>path.StartsWith(s)))continue;if(path.Contains("Highway edge")||path.Contains("lane line")||path.Contains("Broken lane"))continue;
    var b=ren?ren.bounds:col.bounds;if(b.max.x<-820||b.min.x>820||b.max.z<380||b.min.z>860)continue;if(b.size.x>300||b.size.z>300)continue;
    var top=path.Split('/');var key=string.Join("/",top.Take(2))+$"@{Mathf.Round(b.center.x/3)},{Mathf.Round(b.center.z/3)}";if(!seen.Add(key))continue;
    log.Add($"{b.center.x:F1}\t{b.center.z:F1}\t{Mathf.Max(b.extents.x,b.extents.z):F1}\t{(col&&!col.isTrigger?"solid":col?"trigger":"visual")}\t{path.Replace('\n',' ')}");
   }
   var g=GameObject.Find("Ground_320_560");if(g){var m=g.GetComponent<MeshFilter>().sharedMesh;var v=m.vertices;log.Add($"#ground mesh {m.name} verts {v.Length} first {v[0]} {v[1]} {v[2]} scale {g.transform.lossyScale} pos {g.transform.position}");}
   File.WriteAllLines(Path.Combine(outDir,"protect-"+sn+".txt"),log);
  }
  EditorApplication.Exit(0);}
}
