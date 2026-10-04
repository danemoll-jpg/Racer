using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;
// 0.74 read-only: heights of the first collider (matching PROBE_FILTER regex, default any solid) under each grid point.
// PROBE_GRID="scene|x0,z0,x1,z1|step|tag;..." -> grid-<tag>.csv (x,z,y,name)
public static class Report074Grid {
 public static void Run(){var outDir=Environment.GetEnvironmentVariable("PROBE_OUT");Directory.CreateDirectory(outDir);string open=null;
  var rx=new System.Text.RegularExpressions.Regex(Environment.GetEnvironmentVariable("PROBE_FILTER")??".");Physics.queriesHitBackfaces=false;
  foreach(var spec in Environment.GetEnvironmentVariable("PROBE_GRID").Split(';')){var a=spec.Split('|');if(a[0]!=open){EditorSceneManager.OpenScene("Assets/Scenes/"+a[0]+".unity");open=a[0];Physics.SyncTransforms();}
   var b=a[1].Split(',').Select(float.Parse).ToArray();float st=float.Parse(a[2]);var rows=new List<string>{"x,z,y,name"};
   for(float z=b[1];z<=b[3]+1e-3f;z+=st)for(float x=b[0];x<=b[2]+1e-3f;x+=st){string cell="NaN,";
    foreach(var h in Physics.RaycastAll(new Vector3(x,600,z),Vector3.down,1200,~0,QueryTriggerInteraction.Ignore).OrderBy(h=>h.distance)){if(h.collider.attachedRigidbody||h.collider.isTrigger)continue;var n=h.collider.name;
     if(n.IndexOf("trunk",StringComparison.OrdinalIgnoreCase)>=0||n.IndexOf("tree",StringComparison.OrdinalIgnoreCase)>=0)continue;if(!rx.IsMatch(n))continue;cell=$"{h.point.y:F3},{n.Replace(',',' ')}";break;}
    rows.Add($"{x:F2},{z:F2},{cell}");}
   File.WriteAllLines($"{outDir}/grid-{a[3]}.csv",rows);}
  EditorApplication.Exit(0);}
}
