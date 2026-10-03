using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;
// 0.72 read-only: stations of a race road whose chase-camera point has something solid overhead (the weather "under cover" test).
public static class Report072Cover { public static void Run(){var o=Environment.GetEnvironmentVariable("PROBE_OUT");Directory.CreateDirectory(o);var rows=new List<string>();
 foreach(var scene in new[]{"LakeWoods","MountainLoop"}){EditorSceneManager.OpenScene("Assets/Scenes/"+scene+".unity");Physics.SyncTransforms();var race=UnityEngine.Object.FindAnyObjectByType<Racer.RaceDirector>();race.road.Initialize();
  for(float s=0;s<race.road.Length;s+=5){var p=race.road.At(s,out var f);f=Vector3.ProjectOnPlane(f,Vector3.up).normalized;var eye=p-f*6+Vector3.up*2.4f;
   if(Physics.Raycast(eye+Vector3.up*.5f,Vector3.up,out var h,45,~0,QueryTriggerInteraction.Ignore))rows.Add($"{scene} s {s} eye {eye:F1}: {h.distance:F1} m {h.collider.name}");}}
 File.WriteAllLines(o+"/cover.txt",rows);EditorApplication.Exit(0);}}
