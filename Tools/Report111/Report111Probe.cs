using System;using System.IO;using System.Linq;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;
// 0.111 evidence only (temporary editor tool, removed after the round): the smash sites of each scene, their props list and every breakable
// prop near them (in the list or not). PROBE_OUT = output folder, PROBE_SCENES = comma-separated scene names. Read only: no scene is saved.
public static class Report111Probe { public static void Smash(){
 var o=new System.Text.StringBuilder();
 try{foreach(var name in (Environment.GetEnvironmentVariable("PROBE_SCENES")??"").Split(',').Where(s=>s.Length>0)){
  EditorSceneManager.OpenScene("Assets/Scenes/"+name+".unity",OpenSceneMode.Single);
  var props=UnityEngine.Object.FindObjectsByType<Racer.BreakableProp>(FindObjectsInactive.Include,FindObjectsSortMode.None);
  o.AppendLine($"== {name}: {props.Length} breakable props in the scene");
  foreach(var s in UnityEngine.Object.FindObjectsByType<Racer.ActivitySite>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(s=>s.kind==Racer.ActivitySite.Kind.Smash).OrderBy(s=>s.id)){
   var list=s.props??new Racer.BreakableProp[0];var c=s.transform.position;
   float reach=list.Where(p=>p).Select(p=>Vector3.Distance(p.transform.position,c)).DefaultIfEmpty(0).Max();
   o.AppendLine($"site {s.id} '{s.title}' at {c} radius {s.radius} bronze {s.bronze} silver {s.silver} gold {s.gold} seconds {s.Seconds} listed {list.Length} (null {list.Count(p=>!p)}) farthest listed {reach:F1} m; per-vehicle gold {(s.vehicleGold==null?"-":string.Join("/",s.vehicleGold))}");
   foreach(var p in props.Select(p=>(p,d:Vector3.Distance(p.transform.position,c))).Where(x=>x.d<s.radius+40).OrderBy(x=>x.d))
    o.AppendLine($"   {(list.Contains(p.p)?"LISTED  ":"not list")} {p.d,6:F1} m  {p.p.name}  at {p.p.transform.position}");
  }}
 }catch(Exception e){o.AppendLine("ERROR "+e);}
 File.WriteAllText(Environment.GetEnvironmentVariable("PROBE_OUT")+"/smash-sites.txt",o.ToString());EditorApplication.Exit(0);}
 static string Path(Transform t){var s=t.name;while(t.parent){t=t.parent;s=t.name+"/"+s;}return s;}
}
