using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using Racer;using Object=UnityEngine.Object;
// Read-only: full vertical hit stack across one road edge. PROBE_EDGE="scene:s0:s1:side" (side -1 left, 1 right).
public static class Report067EdgeStack {
 public static void Run(){var a=Environment.GetEnvironmentVariable("PROBE_EDGE").Split(':');EditorSceneManager.OpenScene("Assets/Scenes/"+a[0]+".unity");Physics.SyncTransforms();Physics.queriesHitBackfaces=true;
  var road=Object.FindAnyObjectByType<RaceDirector>(FindObjectsInactive.Include).road;road.Initialize();int side=int.Parse(a[3]);var rows=new List<string>();
  var fl=Object.FindAnyObjectByType<MountainFlights>(FindObjectsInactive.Include);if(fl)foreach(var f in fl.flights)rows.Add($"FLIGHT approach={f.approachStation} end={f.endStation}");
  for(float s=float.Parse(a[1]);s<=float.Parse(a[2]);s+=1){var c=road.At(s,out var f);var right=Vector3.Cross(Vector3.up,Vector3.ProjectOnPlane(f,Vector3.up).normalized);rows.Add($"s={s} c={c.ToString("F2")} hw={road.HalfWidth(s):F2}");
   float o0=a.Length>4?float.Parse(a[4]):5.5f,o1=a.Length>5?float.Parse(a[5]):10f;for(float o=o0;o<=o1+.01f;o+=.25f){var q=c+right*side*o;var org=new Vector3(q.x,c.y+4,q.z);var parts=new List<string>();
    for(int k=0;k<8&&Physics.Raycast(org,Vector3.down,out var h,10,~0,QueryTriggerInteraction.Ignore);k++){parts.Add($"{h.collider.name.Replace("Ground_","").Replace("MountainLoopReverse","MLR")}@{h.point.y-c.y:F2}{(h.normal.y<0?"(back)":"")}#{h.triangleIndex}");org=h.point+Vector3.down*.01f;}
    rows.Add($"  o={o:F2}: "+string.Join(" | ",parts));}}
  File.WriteAllLines(Environment.GetEnvironmentVariable("PROBE_OUT")+"/edge-"+a[1]+".txt",rows);EditorApplication.Exit(0);}
}
