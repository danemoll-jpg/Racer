using System;using System.IO;using System.Linq;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;
// 0.115 (read-only, temporary): top-down orthographic renders. PROBE_TOPS = "scene:name:cx,cz,half,px;..." -> top-<name>.png (x right, z up).
public static class Report115Top {
 public static void Run(){
  var outDir=Environment.GetEnvironmentVariable("PROBE_OUT");Directory.CreateDirectory(outDir);
  try{string open=null;
   foreach(var spec in Environment.GetEnvironmentVariable("PROBE_TOPS").Split(';',StringSplitOptions.RemoveEmptyEntries)){
    var parts=spec.Split(':');var sn=parts[0];var name=parts[1];var f=parts[2].Split(',').Select(x=>float.Parse(x,System.Globalization.CultureInfo.InvariantCulture)).ToArray();
    if(open!=sn){EditorSceneManager.OpenScene("Assets/Scenes/"+sn+".unity");open=sn;}
    int px=(int)f[3];var go=new GameObject("top");var cam=go.AddComponent<Camera>();cam.orthographic=true;cam.orthographicSize=f[2];cam.nearClipPlane=1;cam.farClipPlane=3000;cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=Color.black;
    go.transform.SetPositionAndRotation(new Vector3(f[0],1500,f[1]),Quaternion.Euler(90,0,0));
    var rt=new RenderTexture(px,px,24);cam.targetTexture=rt;cam.aspect=1;cam.Render();
    var prev=RenderTexture.active;RenderTexture.active=rt;var tex=new Texture2D(px,px,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,px,px),0,0);tex.Apply();RenderTexture.active=prev;
    File.WriteAllBytes(Path.Combine(outDir,$"top-{name}.png"),tex.EncodeToPNG());cam.targetTexture=null;rt.Release();UnityEngine.Object.DestroyImmediate(go);}
   EditorApplication.Exit(0);
  }catch(Exception e){File.WriteAllText(Path.Combine(outDir,"top-error.txt"),e.ToString());EditorApplication.Exit(1);}
 }
}
