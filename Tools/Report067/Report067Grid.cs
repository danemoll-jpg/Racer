using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using Racer;using Object=UnityEngine.Object;
// Read-only vertical-section grid: every non-trigger static surface crossed per XZ cell (repeated casts, so
// several surfaces of one mesh collider are all reported), plus route stations and flights.
// PROBE_GRID="scene:x0,x1,z0,z1,step[:name];..." output grid-<name or scene>.txt
public static class Report067Grid {
 public static void Run(){var outDir=Environment.GetEnvironmentVariable("PROBE_OUT");
  foreach(var job in Environment.GetEnvironmentVariable("PROBE_GRID").Split(';')){var parts=job.Split(':');var scene=parts[0];var g=parts[1].Split(',').Select(float.Parse).ToArray();var label=parts.Length>2?parts[2]:scene;
   EditorSceneManager.OpenScene("Assets/Scenes/"+scene+".unity");Physics.SyncTransforms();Physics.queriesHitBackfaces=true;var rows=new List<string>();
   var race=Object.FindAnyObjectByType<RaceDirector>(FindObjectsInactive.Include);race.road.Initialize();
   var bounds=new Rect(g[0],g[2],g[1]-g[0],g[3]-g[2]);
   for(float s=0;s<race.road.Length;s+=2){var p=race.road.At(s,out var f);if(bounds.Contains(new Vector2(p.x,p.z)))rows.Add($"MAIN s={s} p={p.ToString("F2")} hw={race.road.HalfWidth(s):F1}");}
   foreach(var b in Object.FindObjectsByType<WoodlandRoute>(FindObjectsInactive.Include)){b.Initialize();for(float s=0;s<b.Length;s+=2){var p=b.At(s,out _);if(bounds.Contains(new Vector2(p.x,p.z)))rows.Add($"BRANCH {b.title} s={s} p={p.ToString("F2")} hw={b.halfWidth:F1}");}}
   var fl=Object.FindAnyObjectByType<MountainFlights>(FindObjectsInactive.Include);if(fl)foreach(var f in fl.flights)rows.Add($"FLIGHT {f.name} approach={f.approachStation} end={f.endStation} start={f.start} lip={f.lip} land={f.landingEnd} fwd={f.forward}");
   var names=new Dictionary<Collider,string>();string N(Collider c){if(!names.TryGetValue(c,out var n)){n="C"+names.Count;names[c]=n;}return n;}
   for(float x=g[0];x<=g[1];x+=g[4])for(float z=g[2];z<=g[3];z+=g[4]){var hs=new List<RaycastHit>();var o=new Vector3(x,400,z);for(int k=0;k<40&&Physics.Raycast(o,Vector3.down,out var h,600,~0,QueryTriggerInteraction.Ignore);k++){if(!h.collider.attachedRigidbody)hs.Add(h);o=h.point+Vector3.down*.02f;}
    rows.Add($"G {x} {z} "+string.Join(" ",hs.Select(h=>$"{h.point.y:F1}{(h.normal.y<0?"v":"")}:{N(h.collider)}")));}
   foreach(var kv in names)rows.Add($"KEY {kv.Value} {kv.Key.name} | {(kv.Key is MeshCollider mc&&mc.sharedMesh?AssetDatabase.GetAssetPath(mc.sharedMesh):kv.Key.GetType().Name)}");
   File.WriteAllLines(outDir+"/grid-"+label+".txt",rows);}
  EditorApplication.Exit(0);}
}
