using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using Racer;using Object=UnityEngine.Object;
public static class InspectUndergroundPolish {
 public static string Main(){
 if(Application.isPlaying||UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)throw new Exception("Saved edit mode required");
 Directory.CreateDirectory("Docs/UndergroundPolish");var rows=new List<string>();
 foreach(var scene in new[]{"DansBackyardReverse","ForestLoopReverse"}){
 EditorSceneManager.OpenScene("Assets/Scenes/"+scene+".unity");Physics.SyncTransforms();rows.Add(scene);
 foreach(var b in Object.FindObjectsByType<WoodlandRoute>())rows.Add($"ROUTE {b.name} {b.title} length={b.Length} width={b.halfWidth} underground={b.undergroundStart}-{b.undergroundEnd} ai={b.aiValidated}");
 foreach(var r in Object.FindObjectsByType<Renderer>().Where(r=>scene.StartsWith("Dans")?(Vector3.Distance(r.bounds.center,new(464.6f,82.2f,-2.1f))<30||r.name.Contains("LOGGING")||r.name.Contains("mouth")||r.name.Contains("lintel")):(r.name.Contains("Cave")||r.name.Contains("cave"))))rows.Add($"OBJECT {r.name} path={r.transform.parent?.name} pos={r.transform.position} bounds={r.bounds} material={r.sharedMaterial?.name}");
 foreach(var t in Object.FindObjectsByType<TextMesh>().Where(t=>t.name.Contains("LOGGING")))rows.Add($"TEXT {t.text} forward={t.transform.forward} scale={t.transform.lossyScale} shader={t.GetComponent<Renderer>().sharedMaterial.shader.name}");
 }
 File.WriteAllLines("Docs/UndergroundPolish/baseline.txt",rows);EditorSceneManager.OpenScene("Assets/Scenes/DansBackyardReverse.unity");return string.Join("\n",rows);
 }
}
