using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;
// 0.118 (read-only, temporary; scenes never saved): every collider and renderer within PROBE_BOX "x0,y0,z0,x1,y1,z1" in each of PROBE_SCENES -> spot-<scene>.txt
public static class Report118Spot {
 static string P(Transform t){var s=t.name;while(t.parent){t=t.parent;s=t.name+"/"+s;}return s;}
 public static void Run(){
  var o=Environment.GetEnvironmentVariable("PROBE_OUT");Directory.CreateDirectory(o);var inv=System.Globalization.CultureInfo.InvariantCulture;
  var f=Environment.GetEnvironmentVariable("PROBE_BOX").Split(',').Select(x=>float.Parse(x,inv)).ToArray();var box=new Bounds();box.SetMinMax(new Vector3(f[0],f[1],f[2]),new Vector3(f[3],f[4],f[5]));
  foreach(var scene in Environment.GetEnvironmentVariable("PROBE_SCENES").Split(';')){var log=new List<string>();
   try{EditorSceneManager.OpenScene("Assets/Scenes/"+scene+".unity");Physics.SyncTransforms();
    foreach(var c in UnityEngine.Object.FindObjectsByType<Collider>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(c=>c.bounds.Intersects(box)))log.Add($"COL {c.GetType().Name} {(c.enabled&&c.gameObject.activeInHierarchy?"on":"off")} trigger {c.isTrigger} {P(c.transform)} {c.bounds.min:F2}-{c.bounds.max:F2}");
    foreach(var r in UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(r=>r.bounds.Intersects(box)&&r.bounds.size.magnitude<60))log.Add($"REN {(r.enabled&&r.gameObject.activeInHierarchy?"on":"off")} {P(r.transform)} {r.bounds.min:F2}-{r.bounds.max:F2}");
   }catch(Exception e){log.Add("ERROR "+e);}File.WriteAllLines(Path.Combine(o,"spot-"+scene+".txt"),log);}
  EditorApplication.Exit(0);}
}
