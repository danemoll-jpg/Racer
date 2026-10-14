using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;
// 0.114 Part B (read-only): per scene (PROBE_SCENES): every RaceRoad and WoodlandRoute (all points, flags), every RaceDirector (course, road, gates),
// every RaceGate; and (PROBE_TOP=1) a top-down orthographic render of the whole world from the scene's main camera settings. Scenes never saved.
public static class Report114Survey {
 static string P(Transform t){var s=t.name;while(t.parent){t=t.parent;s=t.name+"/"+s;}return s;}
 static string V(Vector3 v)=>$"{v.x:F2} {v.y:F2} {v.z:F2}";
 public static void Run(){
  var outDir=Environment.GetEnvironmentVariable("PROBE_OUT");Directory.CreateDirectory(outDir);
  try{
  foreach(var sn in Environment.GetEnvironmentVariable("PROBE_SCENES").Split(',')){
   var scene=EditorSceneManager.OpenScene("Assets/Scenes/"+sn+".unity");Physics.SyncTransforms();
   var o=new List<string>();
   foreach(var d in UnityEngine.Object.FindObjectsByType<Racer.RaceDirector>(FindObjectsInactive.Include,FindObjectsSortMode.None)){
    o.Add($"# director {P(d.transform)} course {d.courseId} laps {d.laps} road {(d.road?P(d.road.transform):"-")} ambient {(d.ambientRoad?P(d.ambientRoad.transform):"-")} through {(d.throughRoad?P(d.throughRoad.transform):"-")} gates {d.gates?.Length}");
    if(d.gates!=null)for(int i=0;i<d.gates.Length;i++){var g=d.gates[i];if(!g)continue;float s=d.road?d.road.Project(g.transform.position,out _):-1;o.Add($"gate {i} {g.name} {V(g.transform.position)} fwd {V(g.transform.forward)} hw {g.halfWidth:F1} road-s {s:F1}");}}
   foreach(var r in UnityEngine.Object.FindObjectsByType<Racer.RaceRoad>(FindObjectsInactive.Include,FindObjectsSortMode.None).OrderBy(r=>P(r.transform))){
    r.Initialize();o.Add($"# road {P(r.transform)} forestTrail {r.forestTrail} openHighway {r.openHighway} active {r.gameObject.activeInHierarchy} length {r.Length:F1} n {r.points.Length}");
    for(int i=0;i<r.points.Length;i++)o.Add($"{i} {V(r.points[i])}");}
   foreach(var b in UnityEngine.Object.FindObjectsByType<Racer.WoodlandRoute>(FindObjectsInactive.Include,FindObjectsSortMode.None).OrderBy(b=>P(b.transform))){
    o.Add($"# branch {b.title} | {P(b.transform)} active {b.gameObject.activeInHierarchy} aiValidated {b.aiValidated} entry {b.entryRoad:F1} exit {b.exitRoad:F1} bypassed [{string.Join(",",b.bypassedGates??new int[0])}] length {b.Length:F1} n {b.points.Length} ai {(b.HasAiLine?b.aiPoints.Length:0)}");
    for(int i=0;i<b.points.Length;i++)o.Add($"{i} {V(b.points[i])}");}
   File.WriteAllLines(Path.Combine(outDir,"routes-"+sn+".txt"),o);
   var tops=(Environment.GetEnvironmentVariable("PROBE_TOPS")??"").Split(';',StringSplitOptions.RemoveEmptyEntries);int ti=0;
   foreach(var spec in tops){var f=spec.Split(',').Select(x=>float.Parse(x,System.Globalization.CultureInfo.InvariantCulture)).ToArray();float cx=f[0],cz=f[1],half=f[2];ti++;
    var go=new GameObject("top");var cam=go.AddComponent<Camera>();cam.orthographic=true;cam.orthographicSize=half;cam.nearClipPlane=1;cam.farClipPlane=3000;cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=Color.black;
    go.transform.SetPositionAndRotation(new Vector3(cx,1500,cz),Quaternion.Euler(90,0,0));
    var rt=new RenderTexture(4096,4096,24);cam.targetTexture=rt;cam.aspect=1;cam.Render();
    var prev=RenderTexture.active;RenderTexture.active=rt;var tex=new Texture2D(4096,4096,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,4096,4096),0,0);tex.Apply();RenderTexture.active=prev;
    File.WriteAllBytes(Path.Combine(outDir,$"top-{sn}-{ti}.png"),tex.EncodeToPNG());cam.targetTexture=null;rt.Release();UnityEngine.Object.DestroyImmediate(go);
    File.AppendAllText(Path.Combine(outDir,"top-info.txt"),$"top-{sn}-{ti}: ortho centre {cx},{cz} half-size {half} m, 4096 px (x right, z up)\n");}
  }
  EditorApplication.Exit(0);
  }catch(Exception e){File.WriteAllText(Path.Combine(outDir,"survey-error.txt"),e.ToString());EditorApplication.Exit(1);}
 }
}
