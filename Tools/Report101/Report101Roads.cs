using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;
public static class Report101Roads {
 public static void Run(){var log=new List<string>();
  foreach(var scene in Environment.GetEnvironmentVariable("PROBE_SCENES").Split(',')){EditorSceneManager.OpenScene("Assets/Scenes/"+scene+".unity");log.Add("== "+scene);
   var lip=new Vector3(224.61f,74.15f,80.95f);var d=new Vector3(Mathf.Sin(82.82f*Mathf.Deg2Rad),0,Mathf.Cos(82.82f*Mathf.Deg2Rad));
   foreach(var r in UnityEngine.Object.FindObjectsByType<Racer.RaceRoad>(FindObjectsSortMode.None)){if(r.points==null||r.points.Length<4)continue;r.Initialize();
    for(float a=-44;a<=-9;a+=5){var p=lip+d*a;float s=r.Project(p,out float lat);p.y=r.At(s,out _).y;s=r.Project(p,out lat);var c=r.At(s,out var rf);float hz=new Vector2(p.x-c.x,p.z-c.z).magnitude;lat=hz;if(Mathf.Abs(lat)<r.HalfWidth(s)+5)log.Add($"  {r.name} path {P(r.transform)} at along {a}: s {s:F1} lat {lat:F1} hw {r.HalfWidth(s):F1} y {r.At(s,out _).y:F2} heading {Mathf.Atan2(rf.x,rf.z)*Mathf.Rad2Deg:F0}");}}}
  File.WriteAllLines(Environment.GetEnvironmentVariable("PROBE_OUT")+"/roads.txt",log);EditorApplication.Exit(0);}
 static string P(Transform t){var s=t.name;while(t.parent){t=t.parent;s=t.name+"/"+s;}return s;}
}
