using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using Object=UnityEngine.Object;
// 0.74 read-only (Parts B/C): for each named object (PROBE_ROOTS="scene|name|name...;...") the world bounds of all its
// renderers, child count, components on it and its direct children (name, renderer bounds, collider, components), plus
// every route (RaceRoad / WoodlandRoute) passing through those bounds.
public static class Report074Roots {
 static string Path(Transform t){var n=t.name;while(t.parent){t=t.parent;n=t.name+"/"+n;}return n.Replace('\n','/');}
 public static void Run(){var outDir=Environment.GetEnvironmentVariable("PROBE_OUT");Directory.CreateDirectory(outDir);var rows=new List<string>();
  foreach(var spec in Environment.GetEnvironmentVariable("PROBE_ROOTS").Split(';')){var a=spec.Split('|');EditorSceneManager.OpenScene("Assets/Scenes/"+a[0]+".unity");rows.Add("SCENE "+a[0]);
   foreach(var name in a.Skip(1)){var all=Object.FindObjectsByType<Transform>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(t=>t.name==name).ToArray();if(all.Length==0){rows.Add($"  {name}: not found");continue;}
    foreach(var t in all){var rs=t.GetComponentsInChildren<Renderer>(true);var b=rs.Length>0?rs[0].bounds:new Bounds(t.position,Vector3.zero);foreach(var r in rs)b.Encapsulate(r.bounds);
     rows.Add($"  {Path(t)} active={t.gameObject.activeInHierarchy} children={t.childCount} renderers={rs.Length} colliders={t.GetComponentsInChildren<Collider>(true).Length} bounds min {b.min:F1} max {b.max:F1} comps [{string.Join(",",t.GetComponents<Component>().Select(c=>c.GetType().Name))}]");
     var groups=t.Cast<Transform>().GroupBy(c=>System.Text.RegularExpressions.Regex.Replace(c.name,@"\s*\d+$",""));
     foreach(var g in groups){var c=g.First();var cr=g.SelectMany(x=>x.GetComponentsInChildren<Renderer>(true)).ToArray();var cb=cr.Length>0?cr[0].bounds:new Bounds(c.position,Vector3.zero);foreach(var r in cr)cb.Encapsulate(r.bounds);
      rows.Add($"    x{g.Count(),3} {g.Key} active={c.gameObject.activeSelf} comps [{string.Join(",",c.GetComponents<Component>().Select(x=>x.GetType().Name).Where(x=>x!="Transform"))}] bounds min {cb.min:F1} max {cb.max:F1} mesh {c.GetComponent<MeshFilter>()?.sharedMesh?.name}");}
     foreach(var road in Object.FindObjectsByType<Racer.RaceRoad>(FindObjectsInactive.Include,FindObjectsSortMode.None))if(road.points!=null&&road.points.Any(p=>b.Contains(new Vector3(p.x,b.center.y,p.z))))rows.Add($"    ROUTE RaceRoad {Path(road.transform)} points inside {road.points.Count(p=>b.Contains(new Vector3(p.x,b.center.y,p.z)))}");
     foreach(var w in Object.FindObjectsByType<Racer.WoodlandRoute>(FindObjectsInactive.Include,FindObjectsSortMode.None))if(w.points!=null&&w.points.Any(p=>b.Contains(new Vector3(p.x,b.center.y,p.z))))rows.Add($"    ROUTE Woodland '{w.title}' {Path(w.transform)} points inside {w.points.Count(p=>b.Contains(new Vector3(p.x,b.center.y,p.z)))}");}}}
  File.WriteAllLines(outDir+"/roots.txt",rows);EditorApplication.Exit(0);}
}
