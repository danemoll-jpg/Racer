using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;
// Temporary 0.102: for objects whose name matches NAMES (comma list, exact), the mesh asset path of their MeshFilter and MeshCollider, per scene.
public static class Report102Meshes { public static void Run(){var log=new List<string>();var names=Environment.GetEnvironmentVariable("NAMES").Split(',');
 foreach(var sn in Environment.GetEnvironmentVariable("PROBE_SCENES").Split(',')){EditorSceneManager.OpenScene("Assets/Scenes/"+sn+".unity");log.Add("===== "+sn);
  foreach(var t in UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(t=>names.Contains(t.name))){var mf=t.GetComponent<MeshFilter>();var mc=t.GetComponent<MeshCollider>();
   log.Add($"{t.name} active {t.gameObject.activeInHierarchy} filter {(mf&&mf.sharedMesh?AssetDatabase.GetAssetPath(mf.sharedMesh)+" ("+mf.sharedMesh.name+", v"+mf.sharedMesh.vertexCount+")":"-")} collider {(mc&&mc.sharedMesh?AssetDatabase.GetAssetPath(mc.sharedMesh)+" ("+mc.sharedMesh.name+")":"-")}");}}
 File.WriteAllLines(Environment.GetEnvironmentVariable("PROBE_OUT")+"/meshes.txt",log);EditorApplication.Exit(0);}}
