using System;using System.IO;using System.Linq;using System.Text;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;
// 0.104: the Hwy 92 band as data for planning the roadside: the through route's points, and a 2 m grid of
// (ground height, top surface height, what the top surface is) from vertical rays.
public static class Report104Grid {
 static string P(Transform t){var s=t.name;while(t.parent){t=t.parent;s=t.name+"/"+s;}return s;}
 public static void Run(){
  var outDir=Environment.GetEnvironmentVariable("PROBE_OUT");
  foreach(var sn in Environment.GetEnvironmentVariable("PROBE_SCENES").Split(',')){
   EditorSceneManager.OpenScene("Assets/Scenes/"+sn+".unity");Physics.SyncTransforms();
   var road=UnityEngine.Object.FindObjectsByType<Racer.RaceRoad>(FindObjectsInactive.Include,FindObjectsSortMode.None).First(r=>r.openHighway);
   File.WriteAllLines(Path.Combine(outDir,"hwy92-"+sn+".txt"),road.points.Select(p=>$"{p.x:F3} {p.y:F3} {p.z:F3}"));
   var cats=new Dictionary<string,int>();var sb=new StringBuilder();
   float step=2;int nx=0;
   for(float z=380;z<=760;z+=step){var line=new StringBuilder();
    for(float x=-1260;x<=970;x+=step){
     var hits=Physics.RaycastAll(new Vector3(x,400,z),Vector3.down,600,~0,QueryTriggerInteraction.Ignore);
     float ground=float.NaN,top=float.NaN;string cat="-";
     foreach(var h in hits.OrderBy(h=>h.distance)){var path=P(h.collider.transform);
      if(float.IsNaN(top)){top=h.point.y;var parts=path.Split('/');cat=parts[0]+(parts.Length>1?"/"+parts[1]:"");}
      if(float.IsNaN(ground)&&(path.StartsWith("Memory loop")||path.Contains("Ground_")))ground=h.point.y;}
     if(!cats.TryGetValue(cat,out var ci)){ci=cats.Count;cats[cat]=ci;}
     line.Append(float.IsNaN(ground)?"nan":ground.ToString("F2")).Append(',').Append(float.IsNaN(top)?"nan":top.ToString("F2")).Append(',').Append(ci).Append(' ');
    }
    sb.AppendLine(line.ToString());}
   File.WriteAllText(Path.Combine(outDir,"grid-"+sn+".txt"),sb.ToString());
   File.WriteAllLines(Path.Combine(outDir,"cats-"+sn+".txt"),cats.OrderBy(k=>k.Value).Select(k=>k.Value+"\t"+k.Key));
  }
  EditorApplication.Exit(0);}
}
