using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using Object=UnityEngine.Object;
public static class InspectFocusedRamp {
 public static string Main(){
 EditorSceneManager.OpenScene("Assets/Scenes/StreetLoopReverse.unity");Physics.SyncTransforms();var rows=new List<string>();
 foreach(float dx in new[]{.2f,.35f,.5f})for(float s=-10;s<=125;s+=5){var p=new Vector3(326,0,-235)+new Vector3(dx,0,1).normalized*s;var hits=Physics.RaycastAll(p+Vector3.up*150,Vector3.down,160,1,QueryTriggerInteraction.Ignore).OrderBy(h=>h.distance).ToArray();rows.Add($"PROFILE {dx} {s} {p}: "+string.Join(";",hits.Select(h=>$"{h.collider.name} {h.point.y:F2}")));}
 foreach(var c in Object.FindObjectsByType<Collider>().Where(c=>!c.attachedRigidbody&&(c.name.Contains("supported")||c.name.StartsWith("Takeoff")||c.name.StartsWith("Landing"))))rows.Add($"SURFACE {c.name}: {c.bounds}");
 File.WriteAllLines("Docs/FocusedRecovery/ramp-profile.txt",rows);
 foreach(float dx in new[]{.2f,.35f,.5f}){var go=new GameObject("Temporary camera",typeof(Camera));var cam=go.GetComponent<Camera>();cam.transform.SetPositionAndRotation(new Vector3(326,71,-235),Quaternion.LookRotation(new Vector3(dx,-.05f,1)));cam.fieldOfView=65;cam.farClipPlane=1500;var rt=new RenderTexture(1200,700,24);var tex=new Texture2D(1200,700,TextureFormat.RGB24,false);var prior=RenderTexture.active;cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,1200,700),0,0);tex.Apply();File.WriteAllBytes("Docs/FocusedRecovery/view-"+dx+".png",tex.EncodeToPNG());RenderTexture.active=prior;Object.DestroyImmediate(tex);Object.DestroyImmediate(rt);Object.DestroyImmediate(go);}return "Inspected profiles and approach headings";
 }
}
