using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;
public static class Report101Nan { public static void Run(){var log=new List<string>();
 foreach(var g in AssetDatabase.FindAssets("t:Mesh",new[]{"Assets/Scenery/Report101","Assets/Track/BackyardShortcuts","Assets/Track/Discovery"})){var p=AssetDatabase.GUIDToAssetPath(g);if(!p.Contains("0.101")&&!p.Contains("Report101")&&!p.Contains("cr101-Batched"))continue;
  foreach(var m in AssetDatabase.LoadAllAssetsAtPath(p).OfType<Mesh>()){int nanV=m.vertices.Count(v=>float.IsNaN(v.x+v.y+v.z)||float.IsInfinity(v.x+v.y+v.z));var nn=m.normals;int nanN=nn.Count(v=>float.IsNaN(v.x+v.y+v.z));int zeroN=nn.Count(v=>v.sqrMagnitude<.5f);log.Add($"{p} {m.name}: v{m.vertexCount} nanV {nanV} nanN {nanN} badN {zeroN} bounds {m.bounds.min}-{m.bounds.max}");}}
 File.WriteAllLines(Environment.GetEnvironmentVariable("PROBE_OUT")+"/nan.txt",log);EditorApplication.Exit(0);}}
