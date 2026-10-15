using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;
// 0.118 Part A2 (read-only, temporary; scene never saved): the ground under Leviathan Forward's line over Kyle's bank, with the course's ground
// (Kyle's bank) applied as in the race: every 0.25 m from PROBE_FROM to PROBE_TO, the top surface under the line and 2 m either side, the
// grade over 1 m and the change of grade per metre (a crest above what a car can follow at speed v: change > 9.81 / v^2) -> profile-bank.txt
public static class Report118Profile {
 public static void Run(){
  var o=Environment.GetEnvironmentVariable("PROBE_OUT");Directory.CreateDirectory(o);var log=new List<string>();
  try{
   var d=JsonUtility.FromJson<Racer.LeviathanCourse.Data>(File.ReadAllText("Assets/Resources/Leviathan/LeviathanForward.json"));
   var scene=EditorSceneManager.OpenScene("Assets/Scenes/"+d.scene+".unity");
   var reshape=typeof(Racer.LeviathanCourse).GetMethod("ReshapeGround",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Static);
   foreach(var rs in d.reshape)reshape.Invoke(null,new object[]{rs,scene});Physics.SyncTransforms();
   var P=d.Points;var S=new float[P.Length];for(int i=1;i<P.Length;i++)S[i]=S[i-1]+Vector3.Distance(P[i-1],P[i]);
   Vector3 At(float s){int k=Array.FindLastIndex(S,x=>x<=s);if(k<0)k=0;if(k>=P.Length-1)return P[^1];return Vector3.Lerp(P[k],P[k+1],(s-S[k])/Mathf.Max(1e-5f,S[k+1]-S[k]));}
   float Top(Vector3 p){float y=float.NaN;foreach(var h in Physics.RaycastAll(new Vector3(p.x,p.y+10,p.z),Vector3.down,20,~0,QueryTriggerInteraction.Ignore))if(!h.collider.attachedRigidbody&&h.collider.name.IndexOf("trunk",StringComparison.OrdinalIgnoreCase)<0)y=float.IsNaN(y)?h.point.y:Mathf.Max(y,h.point.y);return y;}
   float from=float.Parse(Environment.GetEnvironmentVariable("PROBE_FROM")??"668",System.Globalization.CultureInfo.InvariantCulture),to=float.Parse(Environment.GetEnvironmentVariable("PROBE_TO")??"702",System.Globalization.CultureInfo.InvariantCulture);
   var ys=new List<(float s,float y,float yl,float yr,Vector3 p)>();
   for(float s=from;s<=to;s+=.25f){var p=At(s);var f=(At(s+.5f)-At(s-.5f));f.y=0;f.Normalize();var r=Vector3.Cross(Vector3.up,f);ys.Add((s,Top(p),Top(p-r*2),Top(p+r*2),p));}
   float worst=0,worstAt=0;
   for(int i=4;i+4<ys.Count;i++){
    float g1=(ys[i].y-ys[i-4].y)/1f,g2=(ys[i+4].y-ys[i].y)/1f,dg=(g2-g1)/1f; // per metre
    if(-dg>worst){worst=-dg;worstAt=ys[i].s;}
    if(i%2==0)log.Add(FormattableString.Invariant($"s {ys[i].s,6:F2} ({ys[i].p.x:F1},{ys[i].p.z:F1}) y {ys[i].y:F3} (left {ys[i].yl-ys[i].y:+0.00;-0.00} right {ys[i].yr-ys[i].y:+0.00;-0.00}) grade {100*g2,5:F1} % change {100*dg,6:F1} %/m{(dg<0?$"  crest: a car leaves it above {2.237f*Mathf.Sqrt(9.81f/Mathf.Max(1e-4f,-dg)):F0} mph":"")}"));}
   log.Insert(0,FormattableString.Invariant($"sharpest crest (grade falling) {100*worst:F1} %/m at s {worstAt:F2}: a car leaves it above {2.237f*Mathf.Sqrt(9.81f/Mathf.Max(1e-4f,worst)):F0} mph"));
  }catch(Exception e){log.Add("ERROR "+e);}
  File.WriteAllLines(Path.Combine(o,"profile-bank.txt"),log);EditorApplication.Exit(0);}
}
