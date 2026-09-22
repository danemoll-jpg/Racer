using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Racer;
using Object=UnityEngine.Object;
public static class InspectFocusedRecovery {
 public static string Main(){
 if(Application.isPlaying)throw new Exception("Edit mode required");
 EditorSceneManager.OpenScene("Assets/Scenes/StreetLoopReverse.unity");Physics.SyncTransforms();Directory.CreateDirectory("Docs/FocusedRecovery");
 var rows=new List<string>();var race=Object.FindAnyObjectByType<RaceDirector>();var reset=race.vehicle.GetComponent<VehicleRespawn>();var flags=BindingFlags.Instance|BindingFlags.NonPublic;
 typeof(VehicleRespawn).GetMethod("Awake",flags).Invoke(reset,null);typeof(VehicleRespawn).GetField("race",flags).SetValue(reset,race);
 foreach(var z in Object.FindObjectsByType<JumpRecoveryExclusion>())rows.Add($"ZONE {z.name}: {z.start} to {z.end} width={z.halfWidth}");
 foreach(var b in Object.FindObjectsByType<WoodlandRoute>()){
 rows.Add($"BRANCH {b.title} length={b.Length}");
 for(float s=10;s<b.Length;s+=10){var p=b.At(s,out var f);var hits=Physics.RaycastAll(p+Vector3.up*3,Vector3.down,8,1,QueryTriggerInteraction.Ignore).OrderBy(h=>h.distance).ToArray();rows.Add($"{s:F0} {p} slope={f.y:F2} unsafe={typeof(VehicleRespawn).GetMethod("UnsafeJump",flags).Invoke(reset,new object[]{p+Vector3.up*.5f})} hits="+string.Join(";",hits.Select(h=>$"{h.collider.name}@{h.point.y:F2} n={h.normal.y:F2}")));}}
 File.WriteAllLines("Docs/FocusedRecovery/before-recovery.txt",rows);
 var go=new GameObject("Temporary inspection",typeof(Camera));var cam=go.GetComponent<Camera>();cam.transform.SetPositionAndRotation(new Vector3(380,400,-205),Quaternion.Euler(90,0,0));cam.orthographic=true;cam.orthographicSize=125;cam.farClipPlane=1000;
 var rt=new RenderTexture(1200,1200,24);var tex=new Texture2D(1200,1200,TextureFormat.RGB24,false);var prior=RenderTexture.active;cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,1200,1200),0,0);tex.Apply();File.WriteAllBytes("Docs/FocusedRecovery/before-map.png",tex.EncodeToPNG());RenderTexture.active=prior;Object.DestroyImmediate(tex);Object.DestroyImmediate(rt);Object.DestroyImmediate(go);return string.Join("\n",rows.Take(10));
 }
}
