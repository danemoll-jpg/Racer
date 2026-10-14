using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;
// 0.115 (read-only, temporary): ground grids over the Leviathan junctions. PROBE_GRID = "scene:name:x0,z0,x1,z1,step;..." -> grid-<name>.txt:
// x z then the top three upward-facing hits (y, normal y, collider path). Scenes never saved.
public static class Report115Grid {
 static string P(Transform t){var s=t.name;while(t.parent){t=t.parent;s=t.name+"/"+s;}return s;}
 public static void Run(){
  var outDir=Environment.GetEnvironmentVariable("PROBE_OUT");Directory.CreateDirectory(outDir);
  try{
   string open=null;
   foreach(var spec in Environment.GetEnvironmentVariable("PROBE_GRID").Split(';',StringSplitOptions.RemoveEmptyEntries)){
    var parts=spec.Split(':');var sn=parts[0];var name=parts[1];var f=parts[2].Split(',').Select(x=>float.Parse(x,System.Globalization.CultureInfo.InvariantCulture)).ToArray();
    if(open!=sn){EditorSceneManager.OpenScene("Assets/Scenes/"+sn+".unity");Physics.SyncTransforms();open=sn;}
    var o=new List<string>();
    for(float z=f[1];z<=f[3]+.001f;z+=f[4])for(float x=f[0];x<=f[2]+.001f;x+=f[4]){
     var hits=Physics.RaycastAll(new Vector3(x,400,z),Vector3.down,600,~0,QueryTriggerInteraction.Ignore).OrderBy(h=>h.distance).Take(3).ToArray();
     o.Add(FormattableString.Invariant($"{x:F1} {z:F1}")+string.Concat(hits.Select(h=>FormattableString.Invariant($"|{h.point.y:F2} {h.normal.y:F3} {P(h.collider.transform)}"))));}
    File.WriteAllLines(Path.Combine(outDir,"grid-"+name+".txt"),o);
    File.WriteAllLines(Path.Combine(outDir,"water-"+name+".txt"),UnityEngine.Object.FindObjectsByType<Racer.ShallowWater>(FindObjectsInactive.Include,FindObjectsSortMode.None).Select(w=>{var r=w.GetComponent<Renderer>();var b=r?r.bounds:new Bounds(w.transform.position,Vector3.zero);return FormattableString.Invariant($"{P(w.transform)} | round {w.round} | pos {w.transform.position} | scale {w.transform.lossyScale} | rot {w.transform.eulerAngles} | bounds {b.min} {b.max} | renderer {(r?r.enabled:false)}");}));
   }
   EditorApplication.Exit(0);
  }catch(Exception e){File.WriteAllText(Path.Combine(outDir,"grid-error.txt"),e.ToString());EditorApplication.Exit(1);}
 }
}
