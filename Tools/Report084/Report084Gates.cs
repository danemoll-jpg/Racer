using System;using System.IO;using System.Linq;using System.Text;using System.Collections.Generic;using UnityEditor;using UnityEditor.SceneManagement;using UnityEngine;using Racer;using Object=UnityEngine.Object;
// 0.84 Part H: Dan's Backyard gates against the main line (read only): each gate's station, its offset from the main centre
// line, the angle between the gate and the road, its half-width; each branch: span, bypassed gates and how close it passes each
// gate; the Backyard Forward flights. GATES_FIX=1 applies the corrections listed in Fix() (route metadata only).
public static class Report084Gates {
 static string V(Vector3 v)=>$"{v.x:F1},{v.y:F2},{v.z:F1}";
 static float Dist(Vector3[] line,Vector3 q){float best=1e9f;for(int i=0;i+1<line.Length;i++){var a=line[i];var b=line[i+1];var ab=new Vector2(b.x-a.x,b.z-a.z);var aq=new Vector2(q.x-a.x,q.z-a.z);float t=Mathf.Clamp01(Vector2.Dot(aq,ab)/Mathf.Max(1e-6f,ab.sqrMagnitude));best=Mathf.Min(best,(aq-ab*t).magnitude);}return best;}
 public static void Survey(){var o=Environment.GetEnvironmentVariable("PROBE_OUT");var sb=new StringBuilder();
  try{foreach(var name in new[]{"DansBackyardForward","DansBackyardReverse"}){EditorSceneManager.OpenScene("Assets/Scenes/"+name+".unity");var race=Object.FindAnyObjectByType<RaceDirector>();var road=race.road;road.Initialize();
    sb.AppendLine($"== {name} {race.courseId} main {road.Length:F0} m, origin {race.Origin:F1}");
    for(int i=0;i<race.gates.Length;i++){var g=race.gates[i];var gate=g.GetComponent<RaceGate>();float s=road.Project(g.transform.position,out float lat);var c=road.At(s,out var f);
     float ang=Vector3.Angle(Vector3.ProjectOnPlane(g.transform.forward,Vector3.up),Vector3.ProjectOnPlane(f,Vector3.up));var side=Vector3.Dot(g.transform.position-c,Vector3.Cross(Vector3.up,f).normalized);
     sb.AppendLine($"gate {i} {g.name} at {V(g.transform.position)} station {s:F1} offset {side:F1} m from the main centre (road half-width {road.HalfWidth(s):F1}), angle to road {ang:F0}, half-width {(gate?gate.halfWidth:-1):F1}");}
    foreach(var b in Object.FindObjectsByType<WoodlandRoute>(FindObjectsSortMode.None)){b.Initialize();sb.AppendLine($"branch {b.title}: entry {b.entryRoad:F1} exit {b.exitRoad:F1} bypasses [{string.Join(",",b.bypassedGates??new int[0])}] ai {b.aiValidated}; passes gates at "+string.Join(", ",Enumerable.Range(1,race.gates.Length-1).Select(k=>$"CP{k} {Dist(b.points,race.gates[k].transform.position):F0} m")));}
    var bf=Object.FindAnyObjectByType<BackyardForwardCourse>();if(bf)for(int i=0;i<bf.launchStations.Length;i++)sb.AppendLine($"flight {i}: launch {bf.launchStations[i]:F1} landing {bf.landingStations[i]:F1} start {V(bf.flightStarts[i])} dir {V(bf.flightDirections[i])}");
    var fl=Object.FindAnyObjectByType<ForestLayout>();if(fl)for(int i=0;i<fl.jumpStarts.Length;i++)sb.AppendLine($"jump {fl.jumpNames[i]} {fl.jumpStarts[i]:F1}->{fl.jumpEnds[i]:F1}");}
  }catch(Exception e){sb.AppendLine("ERROR "+e);}
  Directory.CreateDirectory(o);File.WriteAllText(o+"/H-gates.txt",sb.ToString());EditorApplication.Exit(0);}

 // 0.84 Part H: Dan's Backyard Forward CP 1 stands where the Dirt crest lands; riders leave the crest heading a little left of
 // the trail and cross 6-8 m left of the gate centre (outside its 3.3 m half-width). Its detection half-width becomes 9 m (the
 // drawn gate and everything else unchanged; RaceGate.halfWidth is used only to decide a pass).
 public static void Fix(){var o=Environment.GetEnvironmentVariable("PROBE_OUT");var sb=new StringBuilder();
  // Then (second pass) Forward CP 2 and Reverse CP 5, where the AI line passed 5.4-6.3 m from the centre in two of three
  // race sets: 7.5 m. GATES_STEP=2 applies only the second pass.
  try{foreach(var (sceneName,index,name,width) in new[]{("DansBackyardForward",1,"Backyard blue CP 1",9f),("DansBackyardForward",2,"Backyard blue CP 2",7.5f),("DansBackyardReverse",5,"Reverse blue CP 5",7.5f)}){
    if(Environment.GetEnvironmentVariable("GATES_STEP")=="2"&&index==1)continue;
    var scene=EditorSceneManager.OpenScene("Assets/Scenes/"+sceneName+".unity");var race=Object.FindAnyObjectByType<RaceDirector>();var g=race.gates[index].GetComponent<RaceGate>();
    if(g.name!=name)throw new Exception("unexpected gate "+g.name);sb.AppendLine($"{sceneName} {g.name}: half-width {g.halfWidth} -> {width}");g.halfWidth=width;EditorUtility.SetDirty(g);
    EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);}}catch(Exception e){sb.AppendLine("ERROR "+e);}
  Directory.CreateDirectory(o);File.WriteAllText(o+"/H-fix.txt",sb.ToString());EditorApplication.Exit(0);}
}
