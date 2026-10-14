using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;
// 0.114 Part B (read-only): is there a drivable surface under given points in a scene? PROBE_IN lines "tag s x y z"; per point the highest
// non-trigger hit within 30 m above / 60 m below the point's y: collider path and its height minus the point's y. Scenes never saved.
public static class Report114Probe {
 static string P(Transform t){var s=t.name;while(t.parent){t=t.parent;s=t.name+"/"+s;}return s;}
 public static void Run(){
  var outDir=Environment.GetEnvironmentVariable("PROBE_OUT");
  try{
  var lines=File.ReadAllLines(Environment.GetEnvironmentVariable("PROBE_IN"));
  foreach(var sn in Environment.GetEnvironmentVariable("PROBE_SCENES").Split(',')){
   EditorSceneManager.OpenScene("Assets/Scenes/"+sn+".unity");Physics.SyncTransforms();var o=new List<string>();
   foreach(var l in lines){var f=l.Split(' ');float x=float.Parse(f[2],System.Globalization.CultureInfo.InvariantCulture),y=float.Parse(f[3],System.Globalization.CultureInfo.InvariantCulture),z=float.Parse(f[4],System.Globalization.CultureInfo.InvariantCulture);
    RaycastHit best=default;bool any=false;
    foreach(var h in Physics.RaycastAll(new Vector3(x,y+30,z),Vector3.down,90,~0,QueryTriggerInteraction.Ignore)){if(h.collider.attachedRigidbody)continue;if(!any||h.point.y>best.point.y){best=h;any=true;}}
    o.Add($"{f[0]} {f[1]} {(any?$"{best.point.y-y:F2} {P(best.collider.transform)}":"none")}");}
   File.WriteAllLines(Path.Combine(outDir,"probe-"+sn+".txt"),o);}
  EditorApplication.Exit(0);
  }catch(Exception e){File.WriteAllText(Path.Combine(outDir,"probe-error.txt"),e.ToString());EditorApplication.Exit(1);}
 }
}
