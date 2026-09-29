using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using Racer;
public static class CaptureBackyardShortcuts {
 public static string Main(){
 var go=new GameObject("Temporary shortcut review camera");go.hideFlags=HideFlags.HideAndDontSave;var cam=go.AddComponent<Camera>();cam.CopyFrom(Camera.main);cam.enabled=false;
 void Shot(string name,Vector3 eye,Vector3 target,float size){cam.orthographic=true;cam.orthographicSize=size;cam.transform.SetPositionAndRotation(eye,Quaternion.LookRotation(target-eye));var rt=new RenderTexture(1600,1000,24);var old=RenderTexture.active;cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;var tex=new Texture2D(1600,1000,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,1600,1000),0,0);tex.Apply();File.WriteAllBytes("Docs/BackyardShortcuts/"+name+".png",tex.EncodeToPNG());RenderTexture.active=old;cam.targetTexture=null;rt.Release();UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(tex);}
 Shot("tree-top",new(270,145,-110),new(202,63,-47),55);Shot("cabin",new(235,119,40),new(219,77,81),26);
 UnityEngine.Object.DestroyImmediate(go);return "Two actual scene captures";
 }
}
