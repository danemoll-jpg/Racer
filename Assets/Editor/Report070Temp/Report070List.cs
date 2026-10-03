using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using Object=UnityEngine.Object;
// 0.70 read-only: every transform whose path contains PROBE_MATCH (comma list) in PROBE_SCENES, with bounds/components/mesh.
// Also every ActivitySite and ExplorationMap destination.
public static class Report070List {
 static string Path(Transform t){var n=t.name;while(t.parent){t=t.parent;n=t.name+"/"+n;}return n.Replace('\n','/');}
 public static void Run(){var outDir=Environment.GetEnvironmentVariable("PROBE_OUT");Directory.CreateDirectory(outDir);var match=Environment.GetEnvironmentVariable("PROBE_MATCH").Split(',');
  foreach(var scene in Environment.GetEnvironmentVariable("PROBE_SCENES").Split(',')){EditorSceneManager.OpenScene("Assets/Scenes/"+scene+".unity");var rows=new List<string>();
   foreach(var t in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include,FindObjectsSortMode.None)){var p=Path(t);if(!match.Any(m=>p.Contains(m)))continue;
    var r=t.GetComponent<Renderer>();var c=t.GetComponent<Collider>();var mf=t.GetComponent<MeshFilter>();var bb=r?r.bounds:c?c.bounds:new Bounds(t.position,Vector3.zero);
    rows.Add($"{(t.gameObject.activeInHierarchy?"A":"-")} {p} | pos={t.position:F2} rot={t.eulerAngles:F0} scl={t.lossyScale:F2} | b={bb.center:F1} sz={bb.size:F1} | {string.Join(",",t.GetComponents<Component>().Where(x=>x&&!(x is Transform)).Select(x=>x.GetType().Name))} | {(mf&&mf.sharedMesh?AssetDatabase.GetAssetPath(mf.sharedMesh):"")}");}
   foreach(var a in Object.FindObjectsByType<Racer.ActivitySite>(FindObjectsInactive.Include,FindObjectsSortMode.None))rows.Add($"ACTIVITY {a.kind} '{a.title}' {Path(a.transform)} pos={a.transform.position:F1} fwd={a.transform.forward:F2}");
   var map=Object.FindAnyObjectByType<Racer.ExplorationMap>(FindObjectsInactive.Include);if(map)foreach(var d in map.destinations)rows.Add($"DEST {d.id} '{d.title}' {d.position:F1} yaw {d.yaw}");
   File.WriteAllLines($"{outDir}/list-{scene}.txt",rows);}
  EditorApplication.Exit(0);}
}
