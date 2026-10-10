using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;
public static class Report101Shots {
 static string Out=>Environment.GetEnvironmentVariable("PROBE_OUT");
 // PROBE_VIEWS="name,x,y,z,yaw,pitch[,ortho size]|..." renders the saved scene (authored ground, roads, objects; no runtime trees) from each eye point; ortho size > 0 = a top-down orthographic view
 public static void Run(){
  Directory.CreateDirectory(Out);var inv=System.Globalization.CultureInfo.InvariantCulture;
  foreach(var scene in Environment.GetEnvironmentVariable("PROBE_SCENES").Split(',')){
   EditorSceneManager.OpenScene("Assets/Scenes/"+scene+".unity");
   var go=new GameObject("shotcam");var cam=go.AddComponent<Camera>();cam.fieldOfView=60;cam.nearClipPlane=.3f;cam.farClipPlane=2500;cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=new Color(.55f,.7f,.9f);
   go.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
   foreach(var v in Environment.GetEnvironmentVariable("PROBE_VIEWS").Split('|')){var a=v.Split(',');
    go.transform.SetPositionAndRotation(new Vector3(float.Parse(a[1],inv),float.Parse(a[2],inv),float.Parse(a[3],inv)),Quaternion.Euler(float.Parse(a[5],inv),float.Parse(a[4],inv),0));
    float ortho=a.Length>6?float.Parse(a[6],inv):0;cam.orthographic=ortho>0;if(ortho>0)cam.orthographicSize=ortho;
    var rt=new RenderTexture(1600,900,24);cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;var tex=new Texture2D(1600,900,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,1600,900),0,0);tex.Apply();RenderTexture.active=null;
    File.WriteAllBytes($"{Out}/{scene}-{a[0]}.png",tex.EncodeToPNG());}
   UnityEngine.Object.DestroyImmediate(go);}
  EditorApplication.Exit(0);}
}
