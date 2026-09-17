using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Object=UnityEngine.Object;
namespace Racer.Editor {
public static class VegetationReview {
 public static void Capture(string label){
  var cam=Camera.main;var pos=cam.transform.position;var rot=cam.transform.rotation;bool ortho=cam.orthographic;float size=cam.orthographicSize,fov=cam.fieldOfView;
  var chase=cam.GetComponent<ChaseCamera>();bool enabled=chase.enabled;chase.enabled=false;
  var car=Object.FindAnyObjectByType<ArcadeVehicle>();var cp=car.transform.position;var cr=car.transform.rotation;
  void Shot(string name,Vector3 p,Vector3 target,bool overhead=false){
   cam.transform.SetPositionAndRotation(p,Quaternion.LookRotation(target-p));cam.orthographic=overhead;cam.orthographicSize=150;cam.fieldOfView=65;
   var rt=new RenderTexture(1440,900,24);var previous=cam.targetTexture;var active=RenderTexture.active;
   try{cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;var tex=new Texture2D(1440,900,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,1440,900),0,0);tex.Apply();File.WriteAllBytes($"Docs/VEGETATION_{label}_{name}.png",tex.EncodeToPNG());Object.DestroyImmediate(tex);}
   finally{cam.targetTexture=previous;RenderTexture.active=active;Object.DestroyImmediate(rt);}
  }
  void Drive(string name,List<Vector3> path,int i){var p=path[i];var forward=Vector3.ProjectOnPlane(path[i+1]-p,Vector3.up).normalized;car.transform.SetPositionAndRotation(p+Vector3.up*.65f,Quaternion.LookRotation(forward));chase.Snap();Shot(name,cam.transform.position,car.transform.position+Vector3.up*.7f+forward*chase.lookAhead);}
  try{var road=StreetLoopBuilder.Route();var cut=Phase5Setup.Path();
   Shot("YARD",new Vector3(389,380,53),new Vector3(389,78,53),true);
   Shot("EDGE",new Vector3(340,100,43),new Vector3(310,82,83));
   Drive("HOUSE_DRIVE",road,Phase5Setup.Closest(road,new Vector3(470,0,30)));
   Drive("WOODED_ROAD",road,Phase5Setup.Closest(road,new Vector3(350,0,-450)));
   Drive("SHORTCUT_ENTRY",cut,0);Drive("SHORTCUT",cut,40);Drive("REENTRY",cut,80);
   Drive("JUMP",road,Phase5Setup.Closest(road,new Vector3(-625,0,-170)));
  }finally{car.transform.SetPositionAndRotation(cp,cr);cam.transform.SetPositionAndRotation(pos,rot);cam.orthographic=ortho;cam.orthographicSize=size;cam.fieldOfView=fov;chase.enabled=enabled;}
 }
 public static void Snapshot(string label){
  var w=GameObject.Find("Woods replacing later subdivisions");
  File.WriteAllLines($"Docs/VEGETATION_{label}_COLLIDERS.txt",w.GetComponentsInChildren<BoxCollider>().Select(c=>$"{c.transform.position:F6}|{c.transform.rotation:F6}|{c.transform.lossyScale:F6}|{c.center:F6}|{c.size:F6}").OrderBy(s=>s));
  File.WriteAllText($"Docs/VEGETATION_{label}_COUNTS.txt",$"Trees {w.GetComponentsInChildren<BoxCollider>().Length}; forest renderers {w.GetComponentsInChildren<MeshRenderer>().Count(r=>r.enabled)}; forest triangles {w.GetComponentsInChildren<MeshFilter>().Where(f=>f.GetComponent<Renderer>().enabled).Sum(f=>f.sharedMesh.triangles.Length/3)}; total enabled renderers {Object.FindObjectsByType<Renderer>().Count(r=>r.enabled)}\n");
 }
 public static void Validate(){
  Snapshot("AFTER");Physics.SyncTransforms();var log=new List<string>();int fail=0;
  void Check(bool ok,string s){log.Add((ok?"PASS ":"FAIL ")+s);if(!ok)fail++;}
  Check(File.ReadAllText("Docs/VEGETATION_BEFORE_COLLIDERS.txt")==File.ReadAllText("Docs/VEGETATION_AFTER_COLLIDERS.txt"),"All tree collider positions, rotation, scale, center and size unchanged");
  var woods=GameObject.Find("Woods replacing later subdivisions");var boxes=woods.GetComponentsInChildren<BoxCollider>();var road=StreetLoopBuilder.Route();var cut=Phase5Setup.Path();int yard=0,roadBlocked=0,cutBlocked=0;float rd=999,cd=999;
  foreach(var c in boxes){var p=c.bounds.center;rd=Mathf.Min(rd,StreetLoopBuilder.Nearest(p,road,out _)-.71f);cd=Mathf.Min(cd,Phase5Setup.Distance(p,cut,out _)-.71f);if(Phase6Buildings.YardClear(p))yard++;}
  Check(yard==0,$"Expanded yard has {yard} tree trunks");Check(rd>=16,$"Minimum trunk surface setback from road center {rd:F2}m; includes jump/bypass/landing");Check(cd>=7.6f,$"Minimum trunk surface setback from shortcut {cd:F2}m");
  int probes=0;void Probe(Vector3 p,bool shortcut){probes++;foreach(var c in Physics.OverlapBox(p+Vector3.up*1.5f,new Vector3(.8f,1.2f,.8f),Quaternion.identity,1,QueryTriggerInteraction.Ignore))if(c.transform.IsChildOf(woods.transform)){if(shortcut)cutBlocked++;else roadBlocked++;}}
  for(int i=0;i<road.Count;i+=8){var side=Vector3.Cross(Vector3.up,(road[(i+1)%road.Count]-road[i]).normalized);foreach(float x in new[]{-14f,-9f,0,9f,14f})Probe(road[i]+side*x,false);}
  for(int i=0;i<cut.Count;i+=3){var side=Vector3.Cross(Vector3.up,(cut[Mathf.Min(i+1,cut.Count-1)]-cut[Mathf.Max(0,i-1)]).normalized);foreach(float x in new[]{-5f,0,5f})Probe(cut[i]+side*x,true);}
  Check(roadBlocked==0&&cutBlocked==0,$"{probes} vehicle-volume road/shoulder/shortcut/re-entry probes; forest obstacles {roadBlocked}/{cutBlocked}");
  Check(woods.GetComponentsInChildren<Renderer>().All(r=>r.enabled),"Only active combined geometry exists; no duplicate source renderers");
  log.Add($"RESULT {fail} failures. Sampling is not exhaustive. Trunk meshes use exact collider center/size transforms; crowns have no colliders.");File.WriteAllLines("Docs/VEGETATION_GEOMETRY.txt",log);
 }
}
}
