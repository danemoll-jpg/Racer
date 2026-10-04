using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using Object=UnityEngine.Object;
// 0.73 read-only inventory: every water body (ShallowWater volumes and water-material renderers), the household scene
// positions, and the ground around Dan's two snow-scene positions (sled hill, frozen pool). PROBE_SCENES list.
public static class Report073Scan {
 static string P(Transform t){var n=t.name;while(t.parent){t=t.parent;n=t.name+"/"+n;}return n;}
 static string V(Vector3 v)=>$"({v.x:F2},{v.y:F2},{v.z:F2})";
 public static void Run(){var outDir=Environment.GetEnvironmentVariable("PROBE_OUT");Directory.CreateDirectory(outDir);
  foreach(var scene in Environment.GetEnvironmentVariable("PROBE_SCENES").Split(',')){EditorSceneManager.OpenScene("Assets/Scenes/"+scene+".unity");var rows=new List<string>{"SCENE "+scene};
   Physics.SyncTransforms();
   foreach(var w in Object.FindObjectsByType<Racer.ShallowWater>(FindObjectsInactive.Include,FindObjectsSortMode.None)){
    var t=w.transform;var r=w.GetComponent<Renderer>();var c=w.GetComponent<Collider>();
    var hits=Physics.RaycastAll(t.position+Vector3.up*5,Vector3.down,30,~0,QueryTriggerInteraction.Ignore).OrderBy(h=>h.distance).Select(h=>$"{h.collider.name}@{h.point.y:F2}{(h.collider.isTrigger?"T":"")}").Take(4);
    rows.Add($"WATER {P(t)} active={w.gameObject.activeInHierarchy} pos={V(t.position)} scale={V(t.lossyScale)} rotY={t.eulerAngles.y:F1} rot={V(t.eulerAngles)} round={w.round} surface={w.Surface:F2} rend={(r?r.sharedMaterial?.name+"/"+r.enabled:"none")} col={(c?c.GetType().Name+(c.isTrigger?"(trig)":"")+"/"+c.enabled:"none")} layer={w.gameObject.layer} below=[{string.Join(", ",hits)}]");}
   foreach(var mr in Object.FindObjectsByType<MeshRenderer>(FindObjectsInactive.Include,FindObjectsSortMode.None)){
    foreach(var m in mr.sharedMaterials){if(!m)continue;bool wat=m.name.IndexOf("water",StringComparison.OrdinalIgnoreCase)>=0||m.shader.name.Contains("Drain");if(!wat)continue;
     var col=mr.GetComponent<Collider>();rows.Add($"WATERMAT {P(mr.transform)} active={mr.gameObject.activeInHierarchy} mat={m.name} shader={m.shader.name} bounds c={V(mr.bounds.center)} s={V(mr.bounds.size)} sw={(mr.GetComponent<Racer.ShallowWater>()!=null)} col={(col?col.GetType().Name+(col.isTrigger?"(trig)":""):"none")} mesh={mr.GetComponent<MeshFilter>()?.sharedMesh?.name}");}}
   foreach(var a in Object.FindObjectsByType<Racer.AmbientLife>(FindObjectsInactive.Include,FindObjectsSortMode.None)){
    rows.Add($"LIFE {P(a.transform)} football=[{string.Join(" ",a.football.Select(V))}] coffee=[{string.Join(" ",a.coffee.Select(V))}] smoking=[{string.Join(" ",a.smoking.Select(V))}] highway={a.highway.Length} residential=[{string.Join(" ",a.residential.Select(V))}]");}
   // Sled hill: Dan at (519.21,74.77,-153.47) heading 172; right side is -x. Grid ahead (s) x right (d).
   var o=new Vector3(519.21f,0,-153.47f);float hd=172*Mathf.Deg2Rad;var f=new Vector3(Mathf.Sin(hd),0,Mathf.Cos(hd));var right=new Vector3(f.z,0,-f.x);
   rows.Add($"SLED fwd={V(f)} right={V(right)}");
   for(int s=-10;s<=90;s+=5){var line=new List<string>();for(int d=-10;d<=40;d+=5){var p=o+f*s+right*d;
     if(Physics.Raycast(p+Vector3.up*300,Vector3.down,out var h,600,~0,QueryTriggerInteraction.Ignore))line.Add($"{d}:{h.point.y:F1}{Tag(h.collider.name)}");else line.Add($"{d}:--");}
    rows.Add($"SLED s={s} "+string.Join(" ",line));}
   // Pool area: Dan at (400.78,80.17,-5.15) heading 196.6
   var pool=new Vector3(400.78f,80.17f,-5.15f);
   foreach(var t in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include,FindObjectsSortMode.None)){if((t.position-pool).sqrMagnitude>28*28)continue;var mr=t.GetComponent<Renderer>();var cl=t.GetComponent<Collider>();if(!mr&&!cl)continue;
    rows.Add($"POOLAREA {P(t)} pos={V(t.position)} b={(mr?V(mr.bounds.center)+" s"+V(mr.bounds.size):"")} col={(cl?cl.GetType().Name+(cl.isTrigger?"T":"")+" "+V(cl.bounds.center)+" s"+V(cl.bounds.size):"")} mat={(mr?mr.sharedMaterial?.name:"")}");}
   File.WriteAllLines($"{outDir}/scan-{scene}.txt",rows);}
  EditorApplication.Exit(0);}
 static string Tag(string n){n=n.ToLowerInvariant();return n.Contains("road")||n.Contains("pave")||n.Contains("asphalt")?"R":n.Contains("water")?"W":n.Contains("drive")?"D":n.Contains("terrain")||n.StartsWith("ground")?"g":"?"+n.Substring(0,Math.Min(10,n.Length));}
}
