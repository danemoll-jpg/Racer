using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;
// 0.105: the triangles (world) of the named colliders in an x range. PROBE_PLAY "x0,x1,name|name"
public static class Report105Tris {
 static float F(string s)=>float.Parse(s,System.Globalization.CultureInfo.InvariantCulture);
 public static void Run(){
  var outDir=Environment.GetEnvironmentVariable("PROBE_OUT");var a=Environment.GetEnvironmentVariable("PROBE_PLAY").Split(',');float x0=F(a[0]),x1=F(a[1]);var names=a[2].Split('|');
  foreach(var sn in Environment.GetEnvironmentVariable("PROBE_SCENES").Split(',')){
   var log=new List<string>();EditorSceneManager.OpenScene("Assets/Scenes/"+sn+".unity");
   foreach(var mc in UnityEngine.Object.FindObjectsByType<MeshCollider>(FindObjectsSortMode.None).Where(c=>names.Contains(c.name))){
    var m=mc.sharedMesh;var v=m.vertices;var t=m.triangles;var w=mc.transform.localToWorldMatrix;
    for(int i=0;i<t.Length;i+=3){var p=new[]{w.MultiplyPoint3x4(v[t[i]]),w.MultiplyPoint3x4(v[t[i+1]]),w.MultiplyPoint3x4(v[t[i+2]])};if(p.All(q=>q.x<x0)||p.All(q=>q.x>x1))continue;
     log.Add(mc.name+";"+i/3+";"+string.Join(";",p.Select(q=>$"{q.x:F4},{q.y:F4},{q.z:F4}"))+$";{t[i]},{t[i+1]},{t[i+2]}");}
   }
   File.WriteAllLines(Path.Combine(outDir,"tris-"+sn+".txt"),log);
  }EditorApplication.Exit(0);}
}
