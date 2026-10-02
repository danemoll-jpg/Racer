using System;using System.IO;using System.Linq;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using Object=UnityEngine.Object;
public static class RegressionViews {
 public static void Run(){var outDir=Environment.GetEnvironmentVariable("PROBE_OUT")+"/views";Directory.CreateDirectory(outDir);
  foreach(var scene in Environment.GetEnvironmentVariable("PROBE_SCENES").Split(',')){EditorSceneManager.OpenScene("Assets/Scenes/"+scene+".unity");
   var cam=new GameObject("view",typeof(Camera)).GetComponent<Camera>();if(Camera.main)cam.CopyFrom(Camera.main);cam.enabled=false;cam.farClipPlane=2000;cam.fieldOfView=60;
   var views=new (string n,Vector3 p,Vector3 t)[]{("b1-driver",new(938,129.5f,-101.2f),new(975,128.5f,-101f)),("b1-high",new(930,150,-125),new(985,129,-100)),("b1-far",new(1040,140,-80),new(990,130,-100)),
    ("b2-side",new(945,185,72),new(990,172,72)),("b2-behind",new(991,168,120),new(990,176,64)),("b2-afterlip",new(990,172,35),new(990,174,72)),("b2-over",new(1035,215,90),new(990,170,68)),("b2-ridercam",new(991,178,84),new(990,182,40))};
   foreach(var v in views){cam.transform.SetPositionAndRotation(v.p,Quaternion.LookRotation(v.t-v.p));var rt=new RenderTexture(1280,720,24);cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;var tex=new Texture2D(1280,720,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,1280,720),0,0);tex.Apply();File.WriteAllBytes($"{outDir}/{scene}-{v.n}.png",tex.EncodeToPNG());cam.targetTexture=null;RenderTexture.active=null;rt.Release();}
   Object.DestroyImmediate(cam.gameObject);}
  EditorApplication.Exit(0);}
}
