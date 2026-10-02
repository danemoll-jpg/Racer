using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using Racer;using Object=UnityEngine.Object;
public static class RegressionPoint {
 public static void Run(){var rows=new List<string>();foreach(var scene in Environment.GetEnvironmentVariable("PROBE_SCENES").Split(',')){EditorSceneManager.OpenScene("Assets/Scenes/"+scene+".unity");Physics.SyncTransforms();Physics.queriesHitBackfaces=true;rows.Add("SCENE "+scene);
  var road=Object.FindAnyObjectByType<RaceDirector>(FindObjectsInactive.Include).road;road.Initialize();
  foreach(var c in Physics.OverlapSphere(new Vector3(991.4f,149.9f,-82.9f),3f,~0,QueryTriggerInteraction.Ignore))rows.Add(" overlap3m "+c.name+" "+c.bounds);
  // vertical terrain profile along the flight centreline x=991.4 from z=-60 to -140: highest non-trigger surface
  for(float z=-60;z>=-140;z-=2){var hs=Physics.RaycastAll(new Vector3(991.4f,260,z),Vector3.down,250,~0,QueryTriggerInteraction.Ignore).Where(h=>!h.collider.attachedRigidbody).OrderByDescending(h=>h.point.y).Take(3);rows.Add($" z={z} "+string.Join(" ; ",hs.Select(h=>$"{h.point.y:F2} {h.collider.name}")));}
 }File.WriteAllLines(Environment.GetEnvironmentVariable("PROBE_OUT")+"/point.txt",rows);EditorApplication.Exit(0);}
}
