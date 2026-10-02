using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using Object=UnityEngine.Object;
// 0.69 read-only safety probe: does any surface added this round (Ground_Report069 ...) lie on the pavement or stand
// inside a route's driving corridor (pavement width, 0.15-3 m above the pavement)? Every route, every 0.5 m.
public static class Report069Intrude {
 public static void Run(){var outDir=Environment.GetEnvironmentVariable("PROBE_OUT");Directory.CreateDirectory(outDir);Physics.queriesHitBackfaces=true;
  foreach(var scene in (Environment.GetEnvironmentVariable("PROBE_SCENES")??"MountainLoop,MountainLoopReverse").Split(',')){
   EditorSceneManager.OpenScene("Assets/Scenes/"+scene+".unity");Physics.SyncTransforms();var rows=new List<string>{"route,s,lat,x,y,z,kind,collider,heightAbovePavement"};
   foreach(var r in Report069Edges.Routes())for(float s=0;s<=r.len;s+=.5f){var c=r.at(s);var right=Vector3.Cross(Vector3.up,Vector3.ProjectOnPlane(r.fw(s),Vector3.up).normalized);
    foreach(int side in new[]{-1,1}){float w=Report069Edges.EdgeW(r,s,side,out float py,out _);if(float.IsNaN(w))continue;
     for(float l=0;l<=w;l+=.25f){var q=c+right*side*l;if(!Report069Edges.Pave(q,py,out float y))continue;py=y;
      // On top of the pavement: first hit from above is one of ours, above the pavement.
      if(Physics.Raycast(new Vector3(q.x,y+3,q.z),Vector3.down,out var h,3.2f,~0,QueryTriggerInteraction.Ignore)&&h.collider.name.StartsWith("Ground_Report069")&&h.point.y>y+.02f)
       rows.Add($"{r.n},{s},{side*l:F2},{q.x:F2},{y:F2},{q.z:F2},on-pavement,{h.collider.name},{h.point.y-y:F2}");
      }
     // Standing in the driving space: sideways rays across the paved width at 0.3 m and 1.0 m above the pavement.
     foreach(float hh in new[]{.3f,1f}){var o=c;o.y=py+hh;if(Physics.Raycast(o,right*side,out var sh,w,~0,QueryTriggerInteraction.Ignore)&&sh.collider.name.StartsWith("Ground_Report069"))rows.Add($"{r.n},{s},{side*sh.distance:F2},{sh.point.x:F2},{sh.point.y:F2},{sh.point.z:F2},in-corridor,{sh.collider.name},{hh}");}}}
   File.WriteAllLines($"{outDir}/intrude-{scene}.csv",rows);}
  EditorApplication.Exit(0);}
}
