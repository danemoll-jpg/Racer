using System;using System.IO;using System.Linq;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using Racer;using Object=UnityEngine.Object;
public static class CaptureUndergroundPolish {
 public static string Main(){
 foreach(var scene in new[]{"DansBackyardReverse","ForestLoopReverse"}){
 EditorSceneManager.OpenScene("Assets/Scenes/"+scene+".unity");var g=new GameObject("Temporary review camera",typeof(Camera));var cam=g.GetComponent<Camera>();cam.CopyFrom(Camera.main);cam.enabled=false;
 void Shot(string name,Vector3 eye,Vector3 target,float ortho=0){cam.orthographic=ortho>0;cam.orthographicSize=ortho;cam.fieldOfView=70;cam.transform.SetPositionAndRotation(eye,Quaternion.LookRotation(target-eye));var rt=new RenderTexture(1280,800,24);var prior=RenderTexture.active;cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;var t=new Texture2D(1280,800,TextureFormat.RGB24,false);t.ReadPixels(new Rect(0,0,1280,800),0,0);t.Apply();File.WriteAllBytes("Docs/UndergroundPolish/"+name+".png",t.EncodeToPNG());RenderTexture.active=prior;cam.targetTexture=null;rt.Release();Object.DestroyImmediate(t);Object.DestroyImmediate(rt);}
 if(scene.StartsWith("Dans")){var b=Object.FindObjectsByType<WoodlandRoute>().Single(b=>b.title.StartsWith("Storm"));foreach(float s in new[]{5f,25f,65f,145f})Shot("drain-"+s,b.At(s,out _)+Vector3.up*1.8f,b.At(s+16,out _)+Vector3.up*1.8f);var sign=GameObject.Find("LOGGING RIDGE").transform;Shot("logging-sign",sign.position-sign.forward*10+Vector3.up*.5f,sign.position);Shot("driveway-arrows",new(467,122,3),new(467,80,3),30);}
 else{var b=Object.FindObjectsByType<WoodlandRoute>().OrderBy(b=>{b.Project(new(50,40,85),out float d);return d;}).First();foreach(float s in new[]{277f,285f,291f})Shot("cave-"+s,b.At(s,out _)+Vector3.up*1.8f,b.At(s+18,out _)+Vector3.up*2);}
 Object.DestroyImmediate(g);
 }EditorSceneManager.OpenScene("Assets/Scenes/DansBackyardReverse.unity");return "Local review images captured";
 }
}

