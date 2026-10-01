using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using Object=UnityEngine.Object;
public static class FinalizeDebugReview {
 public static string Main(){
 if(Application.isPlaying)throw new Exception("Edit mode required");
 foreach(var t in Object.FindObjectsByType<Racer.DebugReportingChecks>())Object.DestroyImmediate(t.gameObject);
 foreach(var t in Object.FindObjectsByType<Racer.MountainCleanupChecks>())Object.DestroyImmediate(t.gameObject);
 var mat=AssetDatabase.LoadAssetAtPath<Material>("Assets/Resources/VehicleGlazing.mat");mat.SetFloat("_SrcBlend",1);EditorUtility.SetDirty(mat);
 EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());AssetDatabase.SaveAssets();
 var scenes=AssetDatabase.FindAssets("t:Scene",new[]{"Assets"}).Select(AssetDatabase.GUIDToAssetPath).ToArray();
 var used=new HashSet<string>(AssetDatabase.GetDependencies(scenes,true));var removed=new List<string>();
 // Only newly generated, untracked mesh intermediates listed before finalization are candidates.
 foreach(var p in File.ReadAllLines("Temp/mountaincleanup-new-assets.txt")){if(!(p.StartsWith("Assets/Track/MountainCleanup/")||p.StartsWith("Assets/Track/BackyardReverse/MountainLoopReverse-trees-"))||!p.EndsWith(".asset"))throw new Exception("Unexpected candidate "+p);if(!used.Contains(p)&&AssetDatabase.LoadAssetAtPath<Mesh>(p)){if(!AssetDatabase.DeleteAsset(p))throw new Exception("Could not remove unused intermediate "+p);removed.Add(p);}}
 PlayerSettings.bundleVersion="0.63.0-review1";EditorUserBuildSettings.development=EditorUserBuildSettings.allowDebugging=EditorUserBuildSettings.connectProfiler=false;
 AssetDatabase.SaveAssets();File.WriteAllLines("Docs/DebugReporting/unused-intermediates.txt",removed);return "Saved source, version 0.63.0-review1; removed "+removed.Count+" unreferenced generated mesh intermediates";
 }
}
