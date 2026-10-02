using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using Racer;using Object=UnityEngine.Object;
// Read-only pavement profile along a route: driving-surface height at lateral offsets.
// PROBE_PROFILE="scene:route:s0:s1:step;..." (route = Main or branch title), output profile-<scene>-<route>-<s0>.txt
public static class Report067Profile {
 public static void Run(){var outDir=Environment.GetEnvironmentVariable("PROBE_OUT");
  foreach(var job in Environment.GetEnvironmentVariable("PROBE_PROFILE").Split(';')){var a=job.Split(':');EditorSceneManager.OpenScene("Assets/Scenes/"+a[0]+".unity");Physics.SyncTransforms();
   var race=Object.FindAnyObjectByType<RaceDirector>(FindObjectsInactive.Include);race.road.Initialize();var branch=Object.FindObjectsByType<WoodlandRoute>(FindObjectsInactive.Include).FirstOrDefault(b=>b.title==a[1]);if(branch)branch.Initialize();
   var drive=Object.FindObjectsByType<MeshCollider>().Where(c=>c.name.Contains("driving surface")).ToArray();var rows=new List<string>();
   for(float s=float.Parse(a[2]);s<=float.Parse(a[3]);s+=float.Parse(a[4])){var c=branch?branch.At(s,out var f):race.road.At(s,out f);var right=Vector3.Cross(Vector3.up,Vector3.ProjectOnPlane(f,Vector3.up).normalized);var line=$"s={s:F1} nav={c.ToString("F2")} hdg={Mathf.Atan2(f.x,f.z)*Mathf.Rad2Deg:F1}";
    foreach(var o in new[]{-3f,-1.5f,0,1.5f,3f}){var q=c+right*o;float y=float.NaN,n=0;foreach(var d in drive)if(d.Raycast(new Ray(q+Vector3.up*3,Vector3.down),out var h,8)){y=h.point.y;n=h.normal.y;}line+=$" | {o}:{y:F2}/{n:F3}";}
    rows.Add(line);}
   File.WriteAllLines($"{outDir}/profile-{a[0]}-{a[1].Replace(' ','_')}-{a[2]}.txt",rows);}
  EditorApplication.Exit(0);}
}
