using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using Racer;using Object=UnityEngine.Object;
// Read-only pavement hole map along a main-route window: '#' pavement, '.' no pavement, 'T' terrain within 0.6 m above
// or below where pavement is missing, numbers = terrain height above pavement level (dm) where it pokes up.
// PROBE_HOLES="scene:s0:s1"
public static class Report067Holes {
 public static void Run(){var a=Environment.GetEnvironmentVariable("PROBE_HOLES").Split(':');EditorSceneManager.OpenScene("Assets/Scenes/"+a[0]+".unity");Physics.SyncTransforms();Physics.queriesHitBackfaces=true;
  var road=Object.FindAnyObjectByType<RaceDirector>(FindObjectsInactive.Include).road;road.Initialize();var rows=new List<string>{"s \\ lateral -9..9 step .25 (left negative)"};
  for(float s=float.Parse(a[1]);s<=float.Parse(a[2]);s+=.25f){var c=road.At(s,out var f);var right=Vector3.Cross(Vector3.up,Vector3.ProjectOnPlane(f,Vector3.up).normalized);var line=new System.Text.StringBuilder($"{s,8:F2} ");float cy=float.NaN;
   for(float o=-9;o<=9.01f;o+=.25f){var q=c+right*o;var hs=new List<RaycastHit>();var org=new Vector3(q.x,c.y+3,q.z);for(int k=0;k<6&&Physics.Raycast(org,Vector3.down,out var h,8,~0,QueryTriggerInteraction.Ignore);k++){hs.Add(h);org=h.point+Vector3.down*.01f;}
    var pave=hs.FirstOrDefault(h=>h.collider.name.Contains("driving surface")&&h.normal.y>0);if(pave.collider){if(float.IsNaN(cy))cy=pave.point.y;var top=hs.First();line.Append(top.collider==pave.collider?'#':'^');continue;}
    var t=hs.FirstOrDefault(h=>h.collider.name.StartsWith("Ground")&&h.normal.y>0);line.Append(t.collider?(Mathf.Abs(t.point.y-c.y)<1.2f?'T':'.'):' ');}
   rows.Add(line.ToString());}
  File.WriteAllLines(Environment.GetEnvironmentVariable("PROBE_OUT")+"/holes.txt",rows);EditorApplication.Exit(0);}
}
