using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;
// Temporary 0.102 area probe. AREA="x0,x1,z0,z1,step" PROBE_SCENES. Writes area.txt: top-collider letter map + legend, renderers intersecting the box, named objects (PROBE_NAMES, comma list, substring).
public static class Report102Area {
 static string Out=>Environment.GetEnvironmentVariable("PROBE_OUT");
 static float F(string s)=>float.Parse(s,System.Globalization.CultureInfo.InvariantCulture);
 static string P(Transform t){var s=t.name;while(t.parent){t=t.parent;s=t.name+"/"+s;}return s;}
 static string V(Vector3 v)=>$"({v.x:F2},{v.y:F2},{v.z:F2})";
 public static void Run(){
  var log=new List<string>();var a=Environment.GetEnvironmentVariable("AREA").Split(',');float x0=F(a[0]),x1=F(a[1]),z0=F(a[2]),z1=F(a[3]),st=F(a[4]);
  var names=(Environment.GetEnvironmentVariable("PROBE_NAMES")??"").Split(',').Where(n=>n.Length>0).ToArray();
  foreach(var scene in Environment.GetEnvironmentVariable("PROBE_SCENES").Split(',')){
   EditorSceneManager.OpenScene("Assets/Scenes/"+scene+".unity");Physics.SyncTransforms();log.Add("===== "+scene);
   var legend=new Dictionary<string,char>();string L="abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
   log.Add($"map rows z {z1}..{z0} cols x {x0}..{x1} step {st} (letter = top collider; heights after)");
   var hrows=new List<string>();
   for(float z=z1;z>=z0-.001f;z-=st){var sb=new System.Text.StringBuilder($"{z,6:F1} ");var hb=new System.Text.StringBuilder($"{z,6:F1} ");
    for(float x=x0;x<=x1+.001f;x+=st){RaycastHit best=default;bool any=false;foreach(var h in Physics.RaycastAll(new Vector3(x,200,z),Vector3.down,300,~0,QueryTriggerInteraction.Ignore)){if(h.collider.attachedRigidbody)continue;if(!any||h.point.y>best.point.y){best=h;any=true;}}
     if(!any){sb.Append(' ');hb.Append("  -  ");continue;}var n=P(best.collider.transform);if(!legend.ContainsKey(n))legend[n]=L[Math.Min(legend.Count,L.Length-1)];sb.Append(legend[n]);hb.Append($"{best.point.y,5:F1}");}
    log.Add(sb.ToString());hrows.Add(hb.ToString());}
   foreach(var kv in legend)log.Add($"  {kv.Value} = {kv.Key}");
   log.Add("heights:");log.AddRange(hrows);
   var box=new Bounds(new Vector3((x0+x1)/2,80,(z0+z1)/2),new Vector3(x1-x0,80,z1-z0));
   foreach(var r in UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(r=>r.bounds.Intersects(box)).OrderBy(r=>P(r.transform))){
    var mf=r.GetComponent<MeshFilter>();var c=r.GetComponent<Collider>();var m=mf?mf.sharedMesh:null;
    log.Add($"R {P(r.transform)} pos {V(r.transform.position)} rot {V(r.transform.eulerAngles)} sc {V(r.transform.lossyScale)} b {V(r.bounds.min)}-{V(r.bounds.max)} mesh {(m?m.name+" v"+m.vertexCount+" "+AssetDatabase.GetAssetPath(m):"-")} mats {string.Join("|",r.sharedMaterials.Select(x=>x?x.name:"null"))} col {(c?c.GetType().Name:"-")} static {r.gameObject.isStatic} active {r.gameObject.activeInHierarchy}");}
   foreach(var c in UnityEngine.Object.FindObjectsByType<Collider>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(c=>c.bounds.Intersects(box)&&!c.GetComponent<Renderer>()).OrderBy(c=>P(c.transform)))
    log.Add($"C {P(c.transform)} {c.GetType().Name} pos {V(c.transform.position)} rot {V(c.transform.eulerAngles)} sc {V(c.transform.lossyScale)} b {V(c.bounds.min)}-{V(c.bounds.max)} trigger {c.isTrigger} {(c is MeshCollider mc&&mc.sharedMesh?mc.sharedMesh.name+" "+AssetDatabase.GetAssetPath(mc.sharedMesh):"")}");
   foreach(var t in UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(t=>names.Any(n=>t.name.IndexOf(n,StringComparison.OrdinalIgnoreCase)>=0))){
    log.Add($"N {P(t)} pos {V(t.position)} rot {V(t.eulerAngles)} sc {V(t.lossyScale)} comps {string.Join(",",t.GetComponents<Component>().Select(c=>c.GetType().Name))}");
    foreach(var mb in t.GetComponents<MonoBehaviour>())if(mb)log.Add("   json "+EditorJsonUtility.ToJson(mb).Substring(0,Math.Min(1500,EditorJsonUtility.ToJson(mb).Length)));}
  }
  Directory.CreateDirectory(Out);File.WriteAllLines(Out+"/area.txt",log);EditorApplication.Exit(0);}
}
