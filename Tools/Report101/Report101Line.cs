using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;
// Temporary 0.101 probe: terrain along the Abandoned Cabin ramp's axis (from the Forward route's s22 -> s41), and the main road's crossing of it.
public static class Report101Line {
 static string Out=>Environment.GetEnvironmentVariable("PROBE_OUT");
 public static readonly Vector3 Foot=new(205.76f,71.10f,78.58f),Lip=new(223.62f+1.98f*.5f,76.66f,80.83f+.125f);
 static float Terrain(Vector3 p,out string n){n="-";float best=float.NaN;foreach(var h in Physics.RaycastAll(new Vector3(p.x,120,p.z),Vector3.down,200,~0,QueryTriggerInteraction.Ignore))if(h.collider.name.StartsWith("Ground_")&&(float.IsNaN(best)||h.point.y>best)){best=h.point.y;n=h.collider.name;}return best;}
 public static void Run(){
  var log=new List<string>();
  foreach(var scene in Environment.GetEnvironmentVariable("PROBE_SCENES").Split(',')){
   EditorSceneManager.OpenScene("Assets/Scenes/"+scene+".unity");Physics.SyncTransforms();log.Add("===== "+scene);
   var fw=GameObject.Find("Backyard optional forest shortcuts/Takeoff - Leaning boards through cabin roof");
   var d=(Lip-Foot);d.y=0;d.Normalize();var n=new Vector3(-d.z,0,d.x);log.Add($"axis heading {Mathf.Atan2(d.x,d.z)*Mathf.Rad2Deg:F2}");
   var race=UnityEngine.Object.FindAnyObjectByType<Racer.RaceDirector>();var roads=UnityEngine.Object.FindObjectsByType<Racer.RaceRoad>(FindObjectsInactive.Exclude,FindObjectsSortMode.None);
   foreach(var r in roads){if(r.points==null||r.points.Length<4)continue;r.Initialize();
    for(float s=0;s<r.Length;s+=.25f){var a=r.At(s,out var f);var b=r.At(s+.25f,out _);float ya=Vector3.Dot(a-Lip,n),yb=Vector3.Dot(b-Lip,n);if(Mathf.Sign(ya)!=Mathf.Sign(yb)){float along=Vector3.Dot(a-Lip,d);if(along>-90&&along<0)log.Add($"road {r.name} (race main {r==race.road}) crosses axis at s {s:F1} along {along:F1} pos {a} heading {Mathf.Atan2(f.x,f.z)*Mathf.Rad2Deg:F1} hw {r.HalfWidth(s):F1}");}}}
   foreach(var w in UnityEngine.Object.FindObjectsByType<Racer.WoodlandRoute>(FindObjectsInactive.Exclude,FindObjectsSortMode.None))for(float s=0;s<w.Length;s+=.5f){var a=w.At(s,out _);var b=w.At(s+.5f,out _);if(Mathf.Sign(Vector3.Dot(a-Lip,n))!=Mathf.Sign(Vector3.Dot(b-Lip,n))){float along=Vector3.Dot(a-Lip,d);if(along>-90&&along<0)log.Add($"trail {w.title} crosses axis at s {s:F1} along {along:F1} pos {a}");}}
   for(float along=-75;along<=-15;along+=1){var sb=new System.Text.StringBuilder($"al {along,4:F0} ");var c=Lip+d*along;for(float l=-10;l<=10;l+=2){var p=c+n*l;float t=Terrain(p,out var nm);sb.Append($"{t,7:F2}");}float tc=Terrain(c,out var nc);sb.Append($"  centre {c.x:F1},{c.z:F1} {nc}");log.Add(sb.ToString());}
  }
  Directory.CreateDirectory(Out);File.WriteAllLines(Out+"/line.txt",log);EditorApplication.Exit(0);}
}
