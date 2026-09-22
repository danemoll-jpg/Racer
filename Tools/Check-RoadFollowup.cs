using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Racer;
using Object=UnityEngine.Object;
public static class Check129 {
 public static string Main(){var rows=new List<string>();
 EditorSceneManager.OpenScene("Assets/Scenes/StreetLoopGreybox.unity");var r=Object.FindAnyObjectByType<RaceDirector>();r.road.Initialize();Physics.SyncTransforms();var origin=r.road.Project(r.gates[0].transform.position,out _);var grid=r.road.At(origin-32,out var dir)-Vector3.Cross(Vector3.up,dir).normalized*2.2f;float rest=Mathf.Max(.4f,r.vehicle.suspensionLength-Physics.gravity.magnitude/r.vehicle.springStrength);var rotation=Quaternion.LookRotation(Vector3.ProjectOnPlane(dir,Vector3.up));int supports=0;
 foreach(var local in r.vehicle.suspensionPoints){var p=grid+Vector3.up*rest+rotation*local;if(Physics.Raycast(p,Vector3.down,out var hit,r.vehicle.suspensionLength,r.vehicle.groundMask,QueryTriggerInteraction.Ignore))supports++;else rows.Add("MISSING GRID PROBE "+p);}
 rows.Add($"Street Forward intended grid {grid}; supported suspension probes={supports}/{r.vehicle.suspensionPoints.Length}");if(supports!=r.vehicle.suspensionPoints.Length)throw new Exception(string.Join("\n",rows));
 int sample=0;float maxStep=0;float previous=0;for(int i=0;i<=160;i++){float s=origin-70+i*.5f;var p=r.road.At(s,out var f)-Vector3.Cross(Vector3.up,f).normalized*2.2f;var hits=Physics.RaycastAll(p+Vector3.up*10,Vector3.down,20,1,QueryTriggerInteraction.Ignore).Where(h=>h.collider.name.StartsWith("Ground_")).OrderByDescending(h=>h.point.y).ToArray();if(hits.Length==0)throw new Exception("Junction support missing");if(i>0)maxStep=Math.Max(maxStep,Math.Abs(hits[0].point.y-previous));previous=hits[0].point.y;sample++;}rows.Add($"Junction support samples={sample}, max height delta per .5m={maxStep:F3}m (retained hill grade included)");
 EditorSceneManager.OpenScene("Assets/Scenes/MountainLoop.unity");r=Object.FindAnyObjectByType<RaceDirector>();r.road.Initialize();Physics.SyncTransforms();sample=0;float worst=0;
 foreach(var f in r.GetComponent<MountainFlights>().flights){float length=Vector3.Dot(f.lip-f.start,f.forward);for(float s=0;s<=length;s+=5){var p=f.start+f.forward*s;float u=Mathf.Clamp01((s-length+60)/60);p.y+=(f.lip.y-f.start.y)*u*u+.04f;foreach(float lane in new[]{-6f,0,6f}){var q=p+Vector3.Cross(Vector3.up,f.forward)*lane;var hits=Physics.RaycastAll(q+Vector3.up*1,Vector3.down,2,1,QueryTriggerInteraction.Ignore).Where(h=>h.collider.name.StartsWith("Ground_")).ToArray();if(hits.Length!=1)throw new Exception($"Expected one runway surface: {f.name} {s} lane {lane} hits={string.Join(",",hits.Select(h=>h.collider.name))}");worst=Math.Max(worst,Math.Abs(hits[0].point.y-p.y));sample++;}}}
 foreach(var mf in Object.FindObjectsByType<MeshFilter>().Where(m=>AssetDatabase.GetAssetPath(m.sharedMesh).Contains("CR129")&&m.TryGetComponent<MeshCollider>(out _))){if(mf.sharedMesh!=mf.GetComponent<MeshCollider>().sharedMesh)throw new Exception("Visual/collision mismatch "+mf.name);if(mf.sharedMesh.vertices.Any(p=>!float.IsFinite(p.x)||!float.IsFinite(p.y)||!float.IsFinite(p.z)))throw new Exception("Nonfinite geometry");}
 if(worst>.08f)throw new Exception("Ramp deviation "+worst);rows.Add($"Forward straight runway samples={sample}; exactly one collider per sample; max profile error={worst:F4}m; visible/collision assets match.");
 File.WriteAllLines("Docs/CR129-132/structural-check.txt",rows);return string.Join("\n",rows);
 }
}
