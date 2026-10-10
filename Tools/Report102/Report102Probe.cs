using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;
// Temporary 0.102 probe: profiles along the Forward main (PROFILES "s0:s1:step;..."), lateral offsets -4..4, top collider and height,
// routes and their entries, and objects within RADIUS of the points in SPOTS "x,z;...". Writes probe.txt.
public static class Report102Probe {
 static string Out=>Environment.GetEnvironmentVariable("PROBE_OUT");
 static float F(string s)=>float.Parse(s,System.Globalization.CultureInfo.InvariantCulture);
 static string P(Transform t){var s=t.name;while(t.parent){t=t.parent;s=t.name+"/"+s;}return s;}
 static string V(Vector3 v)=>$"({v.x:F2},{v.y:F2},{v.z:F2})";
 static (float y,string n) Top(Vector3 p){float best=float.NaN;string name="none";foreach(var h in Physics.RaycastAll(new Vector3(p.x,p.y+30,p.z),Vector3.down,80,~0,QueryTriggerInteraction.Ignore)){if(h.collider.attachedRigidbody)continue;if(float.IsNaN(best)||h.point.y>best){best=h.point.y;name=h.collider.name;}}return (best,name);}
 public static void Run(){var log=new List<string>();
  try{
  foreach(var sn in Environment.GetEnvironmentVariable("PROBE_SCENES").Split(',')){
   EditorSceneManager.OpenScene("Assets/Scenes/"+sn+".unity");Physics.SyncTransforms();log.Add("===== "+sn);
   var roads=UnityEngine.Object.FindObjectsByType<Racer.RaceRoad>(FindObjectsInactive.Include,FindObjectsSortMode.None);foreach(var r in roads){r.Initialize();log.Add($"road {P(r.transform)} len {r.Length:F1} pts {r.points?.Length} active {r.gameObject.activeInHierarchy}");}
   foreach(var w in UnityEngine.Object.FindObjectsByType<Racer.WoodlandRoute>(FindObjectsInactive.Include,FindObjectsSortMode.None)){w.Initialize();log.Add($"route '{w.title}' {P(w.transform)} active {w.gameObject.activeInHierarchy} len {w.Length:F1} entry {w.entryRoad:F1} exit {w.exitRoad:F1} hw {w.halfWidth} start {V(w.points[0])}");}
   var main=roads.FirstOrDefault(r=>r.name=="Forward navigation only - no road mesh");
   foreach(var spec in (Environment.GetEnvironmentVariable("PROFILES")??"").Split(';').Where(x=>x.Length>0)){var a=spec.Split(':');string rn=a[0];float s0=F(a[1]),s1=F(a[2]),st=F(a[3]),lm=a.Length>4?F(a[4]):4,ls=a.Length>5?F(a[5]):1;
    var wr=rn=="main"?null:UnityEngine.Object.FindObjectsByType<Racer.WoodlandRoute>(FindObjectsInactive.Include,FindObjectsSortMode.None).FirstOrDefault(w=>w.title==rn);if(rn=="main"&&!main||rn!="main"&&!wr){log.Add("no route "+rn);continue;}
    Vector3 At(float s,out Vector3 f){if(wr)return wr.At(s,out f);return main.At(s,out f);}
    log.Add($"--- {rn} profile {s0}-{s1}; columns lateral -{lm}..{lm} step {ls} (+ = right)");var legend=new Dictionary<string,char>();const string L="abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
    for(float s=s0;s<=s1+.001f;s+=st){var c=At(s,out var f);var fl=new Vector3(f.x,0,f.z).normalized;var right=Vector3.Cross(Vector3.up,fl);var sb=new System.Text.StringBuilder($"s{s,6:F1} {V(c)} hd {Mathf.Atan2(f.x,f.z)*Mathf.Rad2Deg,5:F0} |");var nb=new System.Text.StringBuilder(" ");
     for(float l=-lm;l<=lm+.001f;l+=ls){var t=Top(c+right*l);sb.Append($" {t.y,6:F2}");if(!legend.ContainsKey(t.n))legend[t.n]=L[Math.Min(legend.Count,L.Length-1)];nb.Append(legend[t.n]);}log.Add(sb.ToString()+nb);}
    foreach(var kv in legend)log.Add($"   {kv.Value} = {kv.Key}");}
   float rad=F(Environment.GetEnvironmentVariable("RADIUS")??"12");
   foreach(var spec in (Environment.GetEnvironmentVariable("SPOTS")??"").Split(';').Where(x=>x.Length>0)){var a=spec.Split(',');var c=new Vector3(F(a[0]),0,F(a[1]));log.Add($"--- objects within {rad} m of ({a[0]},{a[1]})");
    if(main){c.y=Top(c+Vector3.up*100).y;float s=main.Project(c,out float lat);log.Add($"   main s {s:F1} lat {lat:F1}");}
    foreach(var r in UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(r=>{var b=r.bounds;var q=b.ClosestPoint(new Vector3(c.x,b.center.y,c.z));return new Vector2(q.x-c.x,q.z-c.z).magnitude<rad&&b.size.magnitude<400;}).OrderBy(r=>P(r.transform))){var mf=r.GetComponent<MeshFilter>();var col=r.GetComponent<Collider>();
     log.Add($"   R {P(r.transform)} pos {V(r.transform.position)} rot {V(r.transform.eulerAngles)} sc {V(r.transform.lossyScale)} b {V(r.bounds.min)}-{V(r.bounds.max)} mesh {(mf&&mf.sharedMesh?mf.sharedMesh.name+" v"+mf.sharedMesh.vertexCount+" "+AssetDatabase.GetAssetPath(mf.sharedMesh):"-")} mat {(r.sharedMaterial?r.sharedMaterial.name:"-")} col {(col?col.GetType().Name+(col.enabled?"":"(off)"):"-")} active {r.gameObject.activeInHierarchy}");}
    foreach(var col in UnityEngine.Object.FindObjectsByType<Collider>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(x=>!x.GetComponent<Renderer>()).Where(x=>{var b=x.bounds;var q=b.ClosestPoint(new Vector3(c.x,b.center.y,c.z));return new Vector2(q.x-c.x,q.z-c.z).magnitude<rad&&b.size.magnitude<400;}).OrderBy(x=>P(x.transform)))
     log.Add($"   C {P(col.transform)} {col.GetType().Name} pos {V(col.transform.position)} rot {V(col.transform.eulerAngles)} sc {V(col.transform.lossyScale)} b {V(col.bounds.min)}-{V(col.bounds.max)} trig {col.isTrigger} en {col.enabled} active {col.gameObject.activeInHierarchy} {(col is BoxCollider bc?"box c "+V(bc.center)+" s "+V(bc.size):"")}{(col is MeshCollider mc&&mc.sharedMesh?mc.sharedMesh.name+" "+AssetDatabase.GetAssetPath(mc.sharedMesh):"")}");
   }}
  }catch(Exception e){log.Add("FAILED "+e);}
  Directory.CreateDirectory(Out);File.WriteAllLines(Out+"/probe.txt",log);EditorApplication.Exit(0);}
}
