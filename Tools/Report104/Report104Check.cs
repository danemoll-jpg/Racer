using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;
// 0.104 editor check: the Hwy 92 roadside built in a scene (edit mode) and drawn from a few cameras. PROBE_PLAY = "name,x,y,z,yaw,pitch;..."
public static class Report104Check {
 public static void Run(){
  var outDir=Environment.GetEnvironmentVariable("PROBE_OUT");var log=new List<string>();
  foreach(var sn in Environment.GetEnvironmentVariable("PROBE_SCENES").Split(',')){
   var scene=EditorSceneManager.OpenScene("Assets/Scenes/"+sn+".unity");Physics.SyncTransforms();
   var t0=DateTime.Now;Racer.Hwy92Roadside.Clear(scene);log.Add($"{sn}: clear {(DateTime.Now-t0).TotalMilliseconds:F0} ms, removed {Racer.Hwy92Roadside.Removed.Count}: {string.Join(", ",Racer.Hwy92Roadside.Removed)}; trunks cleared {Racer.SceneryTrees.Cleared.Count}");
   var host=new GameObject("check host");var r=host.AddComponent<Racer.Hwy92Roadside>();t0=DateTime.Now;r.Build(scene);log.Add($"{sn}: build {(DateTime.Now-t0).TotalMilliseconds:F0} ms; "+string.Join(" | ",r.Report));
   // grounding: each building's floor above its highest ground (lift) and foundation reach
   foreach(var b in r.Buildings.OrderByDescending(b=>b.bounds.size.y-b.lift).Take(5))log.Add($"  tallest foundation: {b.name} ({b.kind}) at {b.bounds.center:F0} foundation from {b.lowest:F2}, height span {b.bounds.size.y:F1}");
   Canvas.ForceUpdateCanvases();for(int i=0;i<3;i++){r.FitLabels();}
   var views=(Environment.GetEnvironmentVariable("PROBE_PLAY")??"").Split(';').Where(x=>x.Length>0);
   foreach(var v in views){var a=v.Split(',');var go=new GameObject("cam");var cam=go.AddComponent<Camera>();cam.fieldOfView=60;cam.farClipPlane=3000;
    go.transform.SetPositionAndRotation(new Vector3(F(a[1]),F(a[2]),F(a[3])),Quaternion.Euler(F(a[5]),F(a[4]),0));
    var rt=new RenderTexture(1600,900,24);cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;var tex=new Texture2D(1600,900,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,1600,900),0,0);tex.Apply();
    File.WriteAllBytes(Path.Combine(outDir,$"{sn}-{a[0]}.png"),tex.EncodeToPNG());RenderTexture.active=null;UnityEngine.Object.DestroyImmediate(go);}
  }
  File.WriteAllLines(Path.Combine(outDir,"check.txt"),log);EditorApplication.Exit(0);}
 static float F(string s)=>float.Parse(s,System.Globalization.CultureInfo.InvariantCulture);
}
