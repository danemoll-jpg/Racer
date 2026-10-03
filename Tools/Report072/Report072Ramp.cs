using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;
// 0.72 read-only: top surface across the Reverse South Face ramp (x 984-996, z 130 -> 55).
public static class Report072Ramp { public static void Run(){var o=Environment.GetEnvironmentVariable("PROBE_OUT");Directory.CreateDirectory(o);EditorSceneManager.OpenScene("Assets/Scenes/MountainLoopReverse.unity");Physics.SyncTransforms();var rows=new List<string>();
 for(float z=130;z>=55;z-=5){var line=$"z {z,5:F0}:";foreach(float x in new[]{984.5f,986f,988.5f,991f,994.5f,996f}){line+=Physics.Raycast(new Vector3(x,400,z),Vector3.down,out var h,600,~0,QueryTriggerInteraction.Ignore)?$"  x{x}: {h.point.y:F2} n{h.normal.y:F2} {h.collider.name.Substring(0,Math.Min(14,h.collider.name.Length))}":"  -";}rows.Add(line);}
 File.WriteAllLines(o+"/ramp.txt",rows);EditorApplication.Exit(0);}}
