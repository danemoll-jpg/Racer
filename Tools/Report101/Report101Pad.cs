using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;
// 0.101 Part C follow-up: the parking pad sits 4 cm over the old paving where it overlaps it (was 1.5 cm: the paving's own detail showed through).
public static class Report101Pad { public static void Run(){var log=new List<string>();
 foreach(var sn in new[]{"FreeRoamWorld","DansBackyardForward","DansBackyardReverse","ForestLoopReverse","LakeWoods","MountainLoop","MountainLoopReverse","StreetLoopGreybox","StreetLoopReverse"}){
  var sc=EditorSceneManager.OpenScene("Assets/Scenes/"+sn+".unity");Physics.SyncTransforms();
  var pad=GameObject.Find("Ground_Dan beige parking to the garage doors (0.101)");var beige=GameObject.Find("Ground_Dan beige concrete descent and parking").GetComponent<MeshCollider>();
  var m=pad.GetComponent<MeshFilter>().sharedMesh;var v=m.vertices;int raised=0;
  for(int i=0;i<v.Length;i++){float best=float.NaN;foreach(var h in Physics.RaycastAll(new Vector3(v[i].x,v[i].y+5,v[i].z),Vector3.down,10,~0,QueryTriggerInteraction.Ignore))if(h.collider==beige&&(float.IsNaN(best)||h.point.y>best))best=h.point.y;
   // the band's highest point within 0.3 m (its detail between the pad's grid points)
   if(float.IsNaN(best))continue;float top=best;for(int k=0;k<8;k++){var o=Quaternion.Euler(0,k*45,0)*Vector3.forward*.3f;foreach(var h in Physics.RaycastAll(new Vector3(v[i].x+o.x,v[i].y+5,v[i].z+o.z),Vector3.down,10,~0,QueryTriggerInteraction.Ignore))if(h.collider==beige)top=Mathf.Max(top,h.point.y);}
   float y=top+.04f;if(y>v[i].y){v[i].y=y;raised++;}}
  m.vertices=v;m.RecalculateNormals();m.RecalculateBounds();EditorUtility.SetDirty(m);var mc=pad.GetComponent<MeshCollider>();mc.sharedMesh=null;mc.sharedMesh=m;EditorUtility.SetDirty(pad);
  log.Add($"{sn}: pad {AssetDatabase.GetAssetPath(m)}: {raised} vertices raised to 4 cm over the old paving");EditorSceneManager.MarkSceneDirty(sc);EditorSceneManager.SaveScene(sc);}
 AssetDatabase.SaveAssets();File.WriteAllLines(Environment.GetEnvironmentVariable("PROBE_OUT")+"/pad.txt",log);EditorApplication.Exit(0);}}
