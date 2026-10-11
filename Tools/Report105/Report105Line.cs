using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;
// 0.105: the top surface along a line (PROBE_PLAY "x0,z0,x1,z1"), every 5 cm, with slope changes over 0.5 m
public static class Report105Line {
 static float F(string s)=>float.Parse(s,System.Globalization.CultureInfo.InvariantCulture);
 public static void Run(){
  var a=Environment.GetEnvironmentVariable("PROBE_PLAY").Split(',');var p0=new Vector3(F(a[0]),0,F(a[1]));var p1=new Vector3(F(a[2]),0,F(a[3]));var log=new List<string>();
  foreach(var sn in Environment.GetEnvironmentVariable("PROBE_SCENES").Split(',')){EditorSceneManager.OpenScene("Assets/Scenes/"+sn+".unity");Physics.SyncTransforms();
   float L=Vector3.Distance(p0,p1);var ys=new List<(float d,float y,string c)>();
   for(float d=0;d<=L;d+=.05f){var p=Vector3.Lerp(p0,p1,d/L);var hs=Physics.RaycastAll(p+Vector3.up*100,Vector3.down,200,~0,QueryTriggerInteraction.Ignore).Where(h=>h.collider.name.StartsWith("Ground_")).OrderByDescending(h=>h.point.y).ToArray();if(hs.Length>0)ys.Add((d,hs[0].point.y,hs[0].collider.name));}
   for(int i=10;i+10<ys.Count;i+=1){float s1=(ys[i].y-ys[i-10].y)/.5f,s2=(ys[i+10].y-ys[i].y)/.5f;if(Mathf.Abs(s2-s1)>.04f||i%20==0)log.Add($"{sn} d {ys[i].d:F2} y {ys[i].y:F3} {ys[i].c} slope before {s1*100:F1}% after {s2*100:F1}%{(Mathf.Abs(s2-s1)>.04f?"  <-- change "+(s2-s1)*100+"%":"")}");}
  }
  File.WriteAllLines(Path.Combine(Environment.GetEnvironmentVariable("PROBE_OUT"),"line.txt"),log);EditorApplication.Exit(0);}
}
