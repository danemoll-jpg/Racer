using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;
// 0.105 Part C probe: what the ground is at each point (PROBE_PLAY "x,z;x,z"), which route lines pass near, and the trunks nearby.
public static class Report105Paths {
 static string P(Transform t){var s=t.name;while(t.parent){t=t.parent;s=t.name+"/"+s;}return s;}
 static float F(string s)=>float.Parse(s,System.Globalization.CultureInfo.InvariantCulture);
 public static void Run(){
  var outDir=Environment.GetEnvironmentVariable("PROBE_OUT");var log=new List<string>();
  foreach(var sn in Environment.GetEnvironmentVariable("PROBE_SCENES").Split(',')){
   EditorSceneManager.OpenScene((Environment.GetEnvironmentVariable("PROBE_DIR")??"Assets/Scenes/")+sn+".unity");Physics.SyncTransforms();log.Add("===== "+sn);
   var roads=UnityEngine.Object.FindObjectsByType<Racer.RaceRoad>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(r=>r.points!=null&&r.points.Length>1).ToList();
   var routes=UnityEngine.Object.FindObjectsByType<Racer.WoodlandRoute>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(r=>r.points!=null&&r.points.Length>1).ToList();
   foreach(var spec in Environment.GetEnvironmentVariable("PROBE_PLAY").Split(';')){var a=spec.Split(',');var p=new Vector3(F(a[0]),0,F(a[1]));
    log.Add($"--- point {p.x},{p.z}");
    foreach(var h in Physics.RaycastAll(new Vector3(p.x,300,p.z),Vector3.down,600,~0,QueryTriggerInteraction.Ignore).OrderBy(h=>h.distance).Take(4)){var r=h.collider.GetComponent<Renderer>();log.Add($"  hit {P(h.collider.transform)} y {h.point.y:F2} mat {(r&&r.sharedMaterial?r.sharedMaterial.name:"-")}");}
    foreach(var r in roads){r.Initialize();float s=r.Project(p,out float lat);var q=r.At(s,out _);var d=q-p;d.y=0;if(d.magnitude<40)log.Add($"  RaceRoad {P(r.transform)} trail {r.forestTrail} highway {r.openHighway} lateral {d.magnitude:F1} half {r.HalfWidth(s):F1} s {s:F0}/{r.Length:F0}");}
    foreach(var w in routes){w.Initialize();float s=w.Project(p,out float lat);var q=w.At(s,out _);var d=q-p;d.y=0;if(d.magnitude<40)log.Add($"  WoodlandRoute {P(w.transform)} '{w.title}' lateral {d.magnitude:F1} half {w.halfWidth:F1} active {w.gameObject.activeInHierarchy} pts {w.points.Length}");}
    foreach(var r in roads){float best=1e9f;Vector3 bp=default;for(int i=0;i+1<r.points.Length;i++){var A=r.points[i];var B=r.points[i+1];var ab=new Vector2(B.x-A.x,B.z-A.z);var ap=new Vector2(p.x-A.x,p.z-A.z);float t=Mathf.Clamp01(Vector2.Dot(ap,ab)/Mathf.Max(1e-6f,ab.sqrMagnitude));float d=(ap-ab*t).magnitude;if(d<best){best=d;bp=Vector3.Lerp(A,B,t);}}if(best<120)log.Add($"  2D RaceRoad {P(r.transform)} trail {r.forestTrail} dist {best:F1} at {bp:F1} active {r.isActiveAndEnabled}");}
    foreach(var w in routes){float best=1e9f;for(int i=0;i+1<w.points.Length;i++){var A=w.points[i];var B=w.points[i+1];var ab=new Vector2(B.x-A.x,B.z-A.z);var ap=new Vector2(p.x-A.x,p.z-A.z);float t=Mathf.Clamp01(Vector2.Dot(ap,ab)/Mathf.Max(1e-6f,ab.sqrMagnitude));best=Mathf.Min(best,(ap-ab*t).magnitude);}if(best<120)log.Add($"  2D WoodlandRoute {P(w.transform)} '{w.title}' dist {best:F1} active {w.gameObject.activeInHierarchy}");}
    foreach(var r in UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None)){var b=r.bounds;var c=b.center;c.y=0;if((c-p).magnitude>30||b.size.x>200||b.size.z>200)continue;var n=P(r.transform);if(n.Contains("trunk")||n.Contains("Tree"))continue;log.Add($"  renderer {n} {b.min:F1}-{b.max:F1} mat {(r.sharedMaterial?r.sharedMaterial.name:"-")}");}
    foreach(var c in Physics.OverlapSphere(p+Vector3.up*20,40).Where(c=>!c.name.StartsWith("Ground_")).GroupBy(c=>P(c.transform).Split('/')[0]))log.Add($"  near: {c.Key} x{c.Count()} e.g. {c.First().name}");
    foreach(var box in UnityEngine.Object.FindObjectsByType<BoxCollider>(FindObjectsSortMode.None).Where(b=>b.name.IndexOf("trunk",StringComparison.OrdinalIgnoreCase)>=0)){var t=box.transform;var bottom=t.TransformPoint(box.center-Vector3.up*box.size.y*.5f);var d=bottom-p;d.y=0;if(d.magnitude<15)log.Add($"  trunk {P(t)} at {bottom:F2} dist {d.magnitude:F1} enabled {box.enabled&&box.gameObject.activeInHierarchy}");}
   }
  }
  File.WriteAllLines(Path.Combine(outDir,"paths.txt"),log);EditorApplication.Exit(0);}
}
