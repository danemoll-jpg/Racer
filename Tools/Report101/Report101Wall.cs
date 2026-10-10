using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;
// 0.101 Part A: the cabin's end wall under the roof's low edge (where the boards meet the roof) caught a vehicle's nose on the boards.
// Its collider's top is lowered 0.8 m (the wall as drawn is unchanged), like 0.43's recessed far gable. Forward and Free Roam.
public static class Report101Wall { public static void Run(){var log=new List<string>();var lip=new Vector3(224.61f,74.15f,80.95f);
 foreach(var sn in new[]{"DansBackyardForward","FreeRoamWorld"}){var sc=EditorSceneManager.OpenScene("Assets/Scenes/"+sn+".unity");var root=GameObject.Find("Backyard optional forest shortcuts").transform;
  var walls=root.Cast<Transform>().Where(t=>t.name=="Cabin end wall below roof").OrderByDescending(t=>Vector3.Distance(t.position,lip)).ToList();var near=walls[0];var bc=near.GetComponent<BoxCollider>();float H=near.lossyScale.y;
  var before=bc.bounds;if(bc.size.y>.999f){bc.size=new Vector3(bc.size.x,(H-.8f)/H,bc.size.z);bc.center=new Vector3(bc.center.x,-.8f/(2*H),bc.center.z);}EditorUtility.SetDirty(bc);Physics.SyncTransforms();
  log.Add($"{sn}: {near.name} at {near.position}: collider top {before.max.y:F2} -> {bc.bounds.max.y:F2} (the other end wall, at the lip, unchanged)");EditorSceneManager.MarkSceneDirty(sc);EditorSceneManager.SaveScene(sc);}
 File.WriteAllLines(Environment.GetEnvironmentVariable("PROBE_OUT")+"/wall.txt",log);EditorApplication.Exit(0);}}
