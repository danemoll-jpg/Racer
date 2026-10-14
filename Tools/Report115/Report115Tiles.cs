using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;
// 0.115 (read-only, temporary): the ground tiles under the lake path's gully crossing in each scene: mesh asset, vertex count, the material, the collider mesh.
public static class Report115Tiles {
 public static void Run(){
  var outDir=Environment.GetEnvironmentVariable("PROBE_OUT");Directory.CreateDirectory(outDir);var o=new List<string>();
  try{
   foreach(var sn in new[]{"MountainLoop","MountainLoopReverse","FreeRoamWorld","LakeWoods","ForestLoopReverse","StreetLoopGreybox"}){
    EditorSceneManager.OpenScene("Assets/Scenes/"+sn+".unity");
    foreach(var name in new[]{"Ground_640_240","Ground_640_320","Ground_560_240","Ground_560_320"}){
     foreach(var t in UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(t=>t.name==name)){
      var mf=t.GetComponent<MeshFilter>();var mc=t.GetComponent<MeshCollider>();var mr=t.GetComponent<MeshRenderer>();var m=mf?mf.sharedMesh:null;
      o.Add($"{sn} {t.parent?.name}/{t.name} pos {t.position} active {t.gameObject.activeInHierarchy} mesh '{(m?m.name:"-")}' asset '{(m?AssetDatabase.GetAssetPath(m):"")}' verts {(m?m.vertexCount:0)} colors {(m?m.colors.Length:0)} bounds {(m?m.bounds.ToString():"")} readable {(m?m.isReadable:false)} | collider '{(mc&&mc.sharedMesh?AssetDatabase.GetAssetPath(mc.sharedMesh)+"#"+mc.sharedMesh.name:"-")}' same {(mc&&mf&&mc.sharedMesh==mf.sharedMesh)} | material '{(mr&&mr.sharedMaterial?mr.sharedMaterial.name+" / "+mr.sharedMaterial.shader.name+" / "+AssetDatabase.GetAssetPath(mr.sharedMaterial):"-")}'");
     }}}
   File.WriteAllLines(Path.Combine(outDir,"tiles.txt"),o);EditorApplication.Exit(0);
  }catch(Exception e){File.WriteAllText(Path.Combine(outDir,"tiles-error.txt"),e.ToString());EditorApplication.Exit(1);}
 }
}
