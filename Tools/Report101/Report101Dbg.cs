using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;
public static class Report101Dbg { public static void Run(){var log=new List<string>();
 foreach(var sn in new[]{"DansBackyardForward","FreeRoamWorld"}){EditorSceneManager.OpenScene("Assets/Scenes/"+sn+".unity");Physics.SyncTransforms();
  var g=GameObject.Find("Memory loop - north is +Z/Ground_480_400");var m=g.GetComponent<MeshFilter>().sharedMesh;var mc=g.GetComponent<MeshCollider>();
  int nan=m.vertices.Count(v=>float.IsNaN(v.x+v.y+v.z));log.Add($"{sn}: mesh {AssetDatabase.GetAssetPath(m)} nan {nan} collider mesh {(mc.sharedMesh?AssetDatabase.GetAssetPath(mc.sharedMesh):"null")} enabled {mc.enabled} bounds {m.bounds}");
  foreach(var p in new[]{new Vector3(190,0,76.6f),new Vector3(250,0,85)}){var hits=Physics.RaycastAll(new Vector3(p.x,200,p.z),Vector3.down,400,~0,QueryTriggerInteraction.Ignore);log.Add($"  at {p}: {string.Join(", ",hits.Select(h=>h.collider.name+" "+h.point.y.ToString("F2")))}");}
  var root=GameObject.Find("Backyard optional forest shortcuts").transform;foreach(Transform t in root)if(t.name.Contains("lead-in post")||t.name=="Gold optional route chevron"||t.name=="Cabin wooded approach")log.Add($"  {t.name} {t.position}");}
 File.WriteAllLines(Environment.GetEnvironmentVariable("PROBE_OUT")+"/dbg.txt",log);EditorApplication.Exit(0);}}
