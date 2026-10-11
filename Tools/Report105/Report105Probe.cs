using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;
// 0.105 editor probes. PROBE_SCENES, PROBE_PLAY = "x,z,radius,step"
public static class Report105Probe {
 static string P(Transform t){var s=t.name;while(t.parent){t=t.parent;s=t.name+"/"+s;}return s;}
 static float F(string s)=>float.Parse(s,System.Globalization.CultureInfo.InvariantCulture);
 static UnityEngine.SceneManagement.Scene Open(string sn,bool roadside){var scene=EditorSceneManager.OpenScene("Assets/Scenes/"+sn+".unity");Physics.SyncTransforms();if(roadside){Racer.Hwy92Roadside.Clear(scene);var host=new GameObject("check host");host.AddComponent<Racer.Hwy92Roadside>().Build(scene);Physics.SyncTransforms();}return scene;}
 public static void Spot(){
  var outDir=Environment.GetEnvironmentVariable("PROBE_OUT");var a=Environment.GetEnvironmentVariable("PROBE_PLAY").Split(',');float cx=F(a[0]),cz=F(a[1]),r=F(a[2]),st=F(a[3]);
  foreach(var sn in Environment.GetEnvironmentVariable("PROBE_SCENES").Split(',')){
   var log=new List<string>();Open(sn,true);
   var names=new Dictionary<string,int>();
   for(float x=cx-r;x<=cx+r;x+=st)for(float z=cz-r;z<=cz+r;z+=st){
    var hits=Physics.RaycastAll(new Vector3(x,200,z),Vector3.down,400,~0,QueryTriggerInteraction.Ignore).OrderByDescending(h=>h.point.y).ToArray();
    var s=string.Join(" | ",hits.Take(4).Select(h=>$"{h.collider.name}[{h.collider.GetType().Name}] {h.point.y:F3} n{h.normal.y:F3}"));
    log.Add($"{x:F1},{z:F1}: {s}");foreach(var h in hits)names[P(h.collider.transform)]=names.TryGetValue(P(h.collider.transform),out var n)?n+1:1;
   }
   log.Insert(0,"== colliders seen: "+string.Join("\n  ",names.Select(k=>k.Key+" x"+k.Value)));
   foreach(var c in Physics.OverlapSphere(new Vector3(cx,9,cz),r)) log.Insert(1,$"OVERLAP {P(c.transform)} {c.GetType().Name} trig {c.isTrigger} bounds {c.bounds.min:F2}-{c.bounds.max:F2}");
   File.WriteAllLines(Path.Combine(outDir,"spot-"+sn+".txt"),log);
  }
  EditorApplication.Exit(0);}
}
