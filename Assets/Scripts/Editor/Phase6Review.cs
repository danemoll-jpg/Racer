using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using Object=UnityEngine.Object;
namespace Racer.Editor {
public static class Phase6Review {
 public const string Root="Remembered houses and approximate buildings";
 public static void Snapshot(string label){
  var root=GameObject.Find(Root).transform;
  var rows=new List<string>();
  foreach(Transform t in root) rows.Add($"{t.GetSiblingIndex()}|{t.name}|{t.position.ToString("F6")}|{t.rotation.ToString("F6")}|{t.localScale.ToString("F6")}");
  File.WriteAllLines("Docs/PHASE6_"+label+"_PLACEMENTS.txt",rows);
  File.WriteAllText("Docs/PHASE6_"+label+"_COUNTS.txt",$"Buildings {root.childCount}; renderers {Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None).Length}; colliders {Object.FindObjectsByType<Collider>(FindObjectsSortMode.None).Length}; building colliders {root.GetComponentsInChildren<Collider>().Length}\n");
 }
 public static void Capture(string label){
  var cam=Camera.main;var pos=cam.transform.position;var rot=cam.transform.rotation;bool ortho=cam.orthographic;float size=cam.orthographicSize,fov=cam.fieldOfView;
  var chase=cam.GetComponent<ChaseCamera>();bool enabled=chase&&chase.enabled;if(chase)chase.enabled=false;
  void Shot(string name,Vector3 p,Vector3 target,bool overhead=false){
   cam.transform.position=p;cam.transform.LookAt(target);cam.orthographic=overhead;cam.orthographicSize=112;cam.fieldOfView=65;
   var rt=new RenderTexture(1440,900,24);var previous=cam.targetTexture;var active=RenderTexture.active;
   try {cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;var tex=new Texture2D(1440,900,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,1440,900),0,0);tex.Apply();File.WriteAllBytes($"Docs/PHASE6_{label}_{name}.png",tex.EncodeToPNG());Object.DestroyImmediate(tex);}
   finally{cam.targetTexture=previous;RenderTexture.active=active;Object.DestroyImmediate(rt);}
  }
  try{
   Shot("YARD",new Vector3(389,350,53),new Vector3(389,78,53),true);
   foreach(string name in new[]{"Dan - blue X","Original house 2","Original house 3","Friend across street - blue circle","Remembered house behind southern hairpin"}){
    var t=GameObject.Find(name).transform;Shot(name.StartsWith("Dan")?"DAN":name.StartsWith("Original")?"HOUSE"+name.Last():name.StartsWith("Friend")?"FRIEND":"HAIRPIN",t.position+t.forward*(name=="Original house 2"?17:27)+t.right*(name=="Original house 2"?5:15)+Vector3.up*(name=="Original house 2"?9:12),t.position+Vector3.up*4);
   }
   var b=GameObject.Find(Root).transform.Cast<Transform>().First(t=>t.name.Contains("business")&&t.position.x > -250 &&t.position.z<530);
   Shot("BUSINESS",b.position+b.forward*36+b.right*22+Vector3.up*13,b.position+Vector3.up*3);
   var road=StreetLoopBuilder.Route();
   foreach(var entry in new[]{("NEIGHBORHOOD_DRIVE",new Vector3(470,0,30)),("COMMERCIAL_DRIVE",new Vector3(-300,0,530))}){
    int i=Phase5Setup.Closest(road,entry.Item2);var f=(road[i+1]-road[i]).normalized;Shot(entry.Item1,road[i]-f*7.5f+Vector3.up*4.25f,road[i]+f*12+Vector3.up*1.5f);
   }
  }finally{cam.transform.SetPositionAndRotation(pos,rot);cam.orthographic=ortho;cam.orthographicSize=size;cam.fieldOfView=fov;if(chase)chase.enabled=enabled;}
 }
 static List<double> samples=new();static int skip,lastFrame;static double last;static string profile;static Vector3 cp;static Quaternion cr;static bool ce;
 public static void Profile(string label){
  if(!Application.isPlaying)throw new Exception("Play mode required");profile=label;samples.Clear();skip=45;lastFrame=Time.frameCount;last=EditorApplication.timeSinceStartup;
  var cam=Camera.main;cp=cam.transform.position;cr=cam.transform.rotation;ce=cam.GetComponent<ChaseCamera>().enabled;cam.GetComponent<ChaseCamera>().enabled=false;
  cam.transform.position=new Vector3(-260,23,531);cam.transform.LookAt(new Vector3(-120,11,532));EditorApplication.update+=Tick;
 }
 static void Tick(){if(Time.frameCount==lastFrame)return;lastFrame=Time.frameCount;double now=EditorApplication.timeSinceStartup;double dt=(now-last)*1000;last=now;if(skip-->0)return;samples.Add(dt);if(samples.Count<240)return;EditorApplication.update-=Tick;samples.Sort();File.WriteAllText($"Docs/PHASE6_{profile}_PERFORMANCE.txt",$"240 ordinary Play frames after 45 warmup; fixed commercial camera (-260,23,531) toward (-120,11,532); forest enabled.\nMedian editor interval {samples[120]:F2}ms; p95 {samples[228]:F2}ms; resolution {Screen.width}x{Screen.height}; vSync {QualitySettings.vSyncCount}; target {Application.targetFrameRate}; renderers {Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None).Length}.\nEditor timing includes editor/host overhead; not standalone GPU timings.\n");var cam=Camera.main;cam.transform.SetPositionAndRotation(cp,cr);cam.GetComponent<ChaseCamera>().enabled=ce;}
}
}
