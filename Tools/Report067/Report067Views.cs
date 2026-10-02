using System;using System.IO;using System.Linq;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using Object=UnityEngine.Object;
// Before/after views at each reported coordinate, framed like the chase camera along the reported heading.
// PROBE_TAG names the set (before/after); PROBE_IDS limits reports; extra views via Report067Views.Extra.
public static class Report067Views {
 public static readonly (string id,string scene,Vector3 eye,Vector3 target)[] Extra={
  ("BUG-009-side","MountainLoopReverse",new(1010,150,-150),new(988,128,-100)),
  ("BUG-010-below","MountainLoopReverse",new(960,128,-70),new(995,140,-85)),
  ("BUG-012-side","MountainLoopReverse",new(1040,150,-90),new(1006,137,-63)),
  ("BUG-017-below","MountainLoop",new(940,140,120),new(965,154,96)),
  ("BUG-018-high","MountainLoop",new(745,100,-20),new(725,80,-28)),
  ("BUG-020-top","DansBackyardForward",new(55,70,40),new(70,45,62)),
  ("FILL-west-portal","MountainLoopReverse",new(912,128,-100.5f),new(960,129,-101)),
  ("FILL-east-portal","MountainLoopReverse",new(1048,139,-85),new(1000,134,-95)),
  ("FILL-overview","MountainLoopReverse",new(925,185,-185),new(990,135,-110)),
  ("FILL-east-side","MountainLoopReverse",new(1070,160,-140),new(995,130,-110)),
  ("BUG-015-left","MountainLoop",new(1355,108,-232),new(1372,100,-236)),
  ("BUG-016-left","MountainLoop",new(722,96,40),new(735,90,30)),
  ("BUG-007-edge","MountainLoopReverse",new(832,109,-96),new(840,100.5f,-108)),
  ("BUG-007-ahead","MountainLoopReverse",new(834,105,-97),new(841,100.5f,-108)),
  ("BUG-007-top","MountainLoopReverse",new(834,122,-112),new(834.01f,100,-112)),
  ("BUG-007-top2","MountainLoopReverse",new(846,124,-104),new(846.01f,100,-104)),
  ("SWEEP-fwd-1910","MountainLoop",new(1272,160,236),new(1257,151,224)),
  ("SWEEP-rev-740","MountainLoopReverse",new(1006,160,300),new(990,151,300)),
  ("SWEEP-rev-424","MountainLoopReverse",new(980,172,140),new(970,164,131)),
  ("BUG-008-left","MountainLoopReverse",new(844,110,-92),new(853,102,-103))};
 public static void Run(){var tag=Environment.GetEnvironmentVariable("PROBE_TAG")??"view";var outDir=Environment.GetEnvironmentVariable("PROBE_OUT")+"/views";Directory.CreateDirectory(outDir);
  var only=Environment.GetEnvironmentVariable("PROBE_IDS");bool Want(string id)=>string.IsNullOrEmpty(only)||only.Split(',').Any(o=>id.StartsWith(o));
  var shots=Report067Probe.Reports.Where(r=>Want(r.id)).Select(r=>{var f=Quaternion.Euler(0,r.heading,0)*Vector3.forward;return(r.id,r.scene,eye:r.p-f*6.5f+Vector3.up*2.6f,target:r.p+f*12+Vector3.up*.5f);})
   .Concat(Extra.Where(e=>Want(e.id))).GroupBy(v=>v.scene);
  foreach(var g in shots){EditorSceneManager.OpenScene("Assets/Scenes/"+g.Key+".unity");
   var cam=new GameObject("view",typeof(Camera)).GetComponent<Camera>();if(Camera.main)cam.CopyFrom(Camera.main);cam.enabled=false;cam.farClipPlane=2500;cam.nearClipPlane=.1f;cam.fieldOfView=60;
   // Warm-up render: the first render after opening a scene can use stale palette/shader state.
   {var first=g.First();cam.transform.SetPositionAndRotation(first.eye,Quaternion.LookRotation(first.target-first.eye));var wrt=new RenderTexture(64,36,24);cam.targetTexture=wrt;cam.Render();cam.Render();cam.targetTexture=null;wrt.Release();}
   foreach(var v in g){cam.transform.SetPositionAndRotation(v.eye,Quaternion.LookRotation(v.target-v.eye));var rt=new RenderTexture(1280,720,24);cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;var tex=new Texture2D(1280,720,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,1280,720),0,0);tex.Apply();File.WriteAllBytes($"{outDir}/{v.id}-{tag}.png",tex.EncodeToPNG());cam.targetTexture=null;RenderTexture.active=null;rt.Release();Object.DestroyImmediate(tex);}
   Object.DestroyImmediate(cam.gameObject);}
  EditorApplication.Exit(0);}
}
