using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;
// 0.103 Part A (editor only): the Cabin takeoff collider (boards through the roof deck) gets a frictionless surface, both scenes, so a vehicle
// body that touches the boards on landing slides over them instead of being braked by them (the wheels' grip is the vehicle's own, not
// collider friction; vehicle bodies already use 0.1 friction, combine Minimum). MATERIAL_OFF=1 takes it off again.
public static class Report103Material {
 const string Path="Assets/Scenery/Report103/Cabin takeoff boards smooth (0.103).physicMaterial";
 public static void Run(){var log=new List<string>();
  try{
   bool off=Environment.GetEnvironmentVariable("MATERIAL_OFF")=="1";
   var mat=AssetDatabase.LoadAssetAtPath<PhysicsMaterial>(Path);
   if(!mat&&!off){mat=new PhysicsMaterial("Cabin takeoff boards smooth (0.103)"){dynamicFriction=0,staticFriction=0,bounciness=0,frictionCombine=PhysicsMaterialCombine.Minimum,bounceCombine=PhysicsMaterialCombine.Minimum};AssetDatabase.CreateAsset(mat,Path);}
   foreach(var sn in new[]{"DansBackyardForward","FreeRoamWorld"}){var scene=EditorSceneManager.OpenScene("Assets/Scenes/"+sn+".unity");
    var c=GameObject.Find("Backyard optional forest shortcuts").GetComponentsInChildren<MeshCollider>(true).First(x=>x.name=="Takeoff - Leaning boards through cabin roof");
    log.Add($"{sn}: {c.name} material {(c.sharedMaterial?c.sharedMaterial.name:"none")} -> {(off?"none":mat.name)}");c.sharedMaterial=off?null:mat;EditorUtility.SetDirty(c);EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);}
   AssetDatabase.SaveAssets();
  }catch(Exception e){log.Add("FAILED "+e);}
  File.WriteAllLines(Environment.GetEnvironmentVariable("PROBE_OUT")+"/material.txt",log);EditorApplication.Exit(0);}
}
