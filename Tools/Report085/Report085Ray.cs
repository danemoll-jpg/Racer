using System;using System.IO;using System.Linq;using System.Text;using UnityEditor;using UnityEditor.SceneManagement;using UnityEngine;
// 0.85 (read only): every collider hit by rays RAYS="ox,oy,oz,dx,dy,dz,len|..." in PROBE_SCENES, with point, normal, triangle.
public static class Report085Ray {
 static string P(Transform t){var n=t.name;while(t.parent){t=t.parent;n=t.name+"/"+n;}return n;}
 public static void Run(){var o=Environment.GetEnvironmentVariable("PROBE_OUT");Directory.CreateDirectory(o);
  foreach(var scene in Environment.GetEnvironmentVariable("PROBE_SCENES").Split(',')){EditorSceneManager.OpenScene("Assets/Scenes/"+scene+".unity");Physics.SyncTransforms();var sb=new StringBuilder();
   foreach(var spec in Environment.GetEnvironmentVariable("RAYS").Split('|')){var f=spec.Split(',').Select(x=>float.Parse(x,System.Globalization.CultureInfo.InvariantCulture)).ToArray();
    var org=new Vector3(f[0],f[1],f[2]);var dir=new Vector3(f[3],f[4],f[5]).normalized;sb.AppendLine($"ray {spec}");
    foreach(var h in Physics.RaycastAll(org,dir,f[6],~0,QueryTriggerInteraction.Ignore).OrderBy(h=>h.distance))sb.AppendLine($"  {h.distance:F2} m {P(h.collider.transform)} at {h.point.x:F2},{h.point.y:F2},{h.point.z:F2} n {h.normal.x:F2},{h.normal.y:F2},{h.normal.z:F2} tri {h.triangleIndex}");}
   File.WriteAllText(o+"/rays-"+scene+".txt",sb.ToString());}
  EditorApplication.Exit(0);}
}
