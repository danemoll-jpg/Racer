using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;
// Temporary 0.98 world tool (copied into Assets/Editor/Report098Temp only while it runs).
public static class Report098Barriers {
 static string Out=>Environment.GetEnvironmentVariable("PROBE_OUT");
 const string Group="Closed roads (race only)";
 // the road "Mountain return trails" (index given by name prefix) near its start: where it leaves the trail / race road
 public static void Mouth(){
  var log=new List<string>();string roadName=Environment.GetEnvironmentVariable("PROBE_ROAD");
  foreach(var scene in Environment.GetEnvironmentVariable("PROBE_SCENES").Split(',')){
   EditorSceneManager.OpenScene("Assets/Scenes/"+scene+".unity");Physics.SyncTransforms();var race=UnityEngine.Object.FindAnyObjectByType<Racer.RaceDirector>();
   var roads=UnityEngine.Object.FindObjectsByType<Racer.RaceRoad>(FindObjectsInactive.Exclude,FindObjectsSortMode.None).Where(r=>r.points!=null&&r.points.Length>3).ToList();foreach(var r in roads)r.Initialize();
   var trails=UnityEngine.Object.FindObjectsByType<Racer.WoodlandRoute>(FindObjectsInactive.Exclude,FindObjectsSortMode.None).Where(w=>w.points!=null&&w.points.Length>3).ToList();foreach(var w in trails)w.Initialize();
   log.Add("==== "+scene+" race road "+race.road.name+" len "+race.road.Length);
   foreach(var road in roads.Where(r=>r.name.StartsWith(roadName))){
    log.Add($" road '{road.name}' length {road.Length:F0} half width {road.HalfWidth(0):F1}");
    for(float s=0;s<Mathf.Min(road.Length,260);s+=6){var p=road.At(s,out var f);string rr="";
     float bl=1e9f;string bn="";foreach(var o in roads){if(o==road)continue;float so=o.Project(p,out float lat);var c=o.At(so,out _);if(Mathf.Abs(c.y-p.y)<9&&lat<bl){bl=lat;bn=o.name+" s"+so.ToString("F0");}}
     foreach(var w in trails){float so=w.Project(p,out float lat);var c=w.At(so,out _);if(Mathf.Abs(c.y-p.y)<9&&lat<bl){bl=lat;bn="TRAIL "+w.name+" s"+so.ToString("F0")+" halfW "+w.halfWidth;}}
     float gy=float.NaN;if(Physics.Raycast(new Vector3(p.x,p.y+20,p.z),Vector3.down,out var h,60,~0,QueryTriggerInteraction.Ignore))gy=h.point.y;
     log.Add($"   s{s:F0} ({p.x:F1},{p.y:F1},{p.z:F1}) heading {Vector3.SignedAngle(Vector3.forward,f,Vector3.up):F0} halfW {road.HalfWidth(s):F1} ground {gy:F1}; nearest other {bn} lateral {bl:F1}");}}}
  Directory.CreateDirectory(Out);File.WriteAllLines(Out+"/mouth.txt",log);EditorApplication.Exit(0);}
 static Material Mat(string name){return AssetDatabase.LoadAssetAtPath<Material>("Assets/Scenery/Report096/"+name+".mat");}
 static GameObject Cube(string name,Transform parent,Vector3 local,Vector3 size,Material m,bool collider){var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=name;g.transform.SetParent(parent,false);g.transform.localPosition=local;g.transform.localScale=size;g.GetComponent<MeshRenderer>().sharedMaterial=m;if(!collider)UnityEngine.Object.DestroyImmediate(g.GetComponent<BoxCollider>());return g;}
 // A timber barrier across a road. `travel` is the direction a driver is going as he comes up to it (away from the route); the sign faces him
 // (its face is on the -z side of the root, which looks back along -travel). The board is 1.9 x 1.15 m with two lines of lettering 1.4 m wide.
 public static Transform Build(Transform parent,string name,Vector3 at,Vector3 travel,float half){
  travel.y=0;travel.Normalize();var root=new GameObject(name).transform;root.SetParent(parent,false);root.position=at;root.rotation=Quaternion.LookRotation(travel);
  var wood=Mat("RoadClosedTimber");var panel=Mat("RoadClosedSign");float w=2*(half+1.2f);
  Cube("Post left",root,new Vector3(-w/2,.65f,0),new Vector3(.26f,1.3f,.26f),wood,false);Cube("Post right",root,new Vector3(w/2,.65f,0),new Vector3(.26f,1.3f,.26f),wood,false);
  Cube("Rail low",root,new Vector3(0,.5f,0),new Vector3(w,.2f,.14f),wood,false);Cube("Rail high",root,new Vector3(0,.95f,0),new Vector3(w,.2f,.14f),wood,false);
  var block=Cube("Barrier collision",root,new Vector3(0,.7f,0),new Vector3(w,1.4f,.3f),wood,true);block.GetComponent<MeshRenderer>().enabled=false;
  var sign=new GameObject("Road closed sign").transform;sign.SetParent(root,false);sign.gameObject.AddComponent<Racer.PhysicalSign>();
  Cube("Sign post left",sign,new Vector3(-.75f,1.1f,.05f),new Vector3(.12f,2.2f,.1f),wood,false);Cube("Sign post right",sign,new Vector3(.75f,1.1f,.05f),new Vector3(.12f,2.2f,.1f),wood,false);
  Cube("Sign backing",sign,new Vector3(0,1.95f,0),new Vector3(1.9f,1.15f,.06f),panel,false);
  var t=new GameObject("Road closed lettering").AddComponent<TextMesh>();t.transform.SetParent(sign,false);t.transform.localPosition=new Vector3(0,1.95f,-.045f);t.transform.localRotation=Quaternion.identity;
  t.text="ROAD\nCLOSED";t.anchor=TextAnchor.MiddleCenter;t.alignment=TextAlignment.Center;t.fontSize=64;t.characterSize=Float("SIGN_K",.0576f);t.lineSpacing=.9f;t.color=Color.white;t.fontStyle=FontStyle.Bold;
  return root;}
 static float Float(string env,float d){var s=Environment.GetEnvironmentVariable(env);return string.IsNullOrEmpty(s)?d:float.Parse(s,System.Globalization.CultureInfo.InvariantCulture);}
 // MountainLoop: keep the one barrier on the dirt road that climbs away from Climbing Ridge Cut, rebuilt facing the driver; everything else of 0.96 goes.
 // MountainLoopReverse: the same road meets Downhill Ridge Cut at its west end; one barrier across it, facing a driver coming off the cut.
 // Every other scene: the group goes.
 public static void Apply(){
  var log=new List<string>();
  foreach(var scene in Environment.GetEnvironmentVariable("PROBE_SCENES").Split(',')){
   var sc=EditorSceneManager.OpenScene("Assets/Scenes/"+scene+".unity");Physics.SyncTransforms();var g=GameObject.Find(Group);
   if(!g&&scene!="MountainLoop"&&scene!="MountainLoopReverse"){log.Add(scene+": no barrier group, nothing to do");continue;}
   var roads=UnityEngine.Object.FindObjectsByType<Racer.RaceRoad>(FindObjectsInactive.Exclude,FindObjectsSortMode.None).Where(r=>r.points!=null&&r.points.Length>3).ToList();foreach(var r in roads)r.Initialize();
   if(g)foreach(Transform t in g.transform)log.Add($"{scene}: removed {t.name} at ({t.position.x:F2},{t.position.y:F2},{t.position.z:F2})");
   bool keep=false;Transform keepRef=null;Vector3 keepAt=default,keepTravel=default;float keepHalf=0;string keepName="";
   if(scene=="MountainLoop"){var at4=new Vector3(789.09f,92.40f,-127.08f);if(g)foreach(Transform t in g.transform){if(Vector3.Distance(new Vector3(t.position.x,0,t.position.z),new Vector3(765.48f,0,-119.56f))<35){at4=t.position;}}
    {keep=true;keepAt=at4;keepName="Road closed barrier 4";var road=roads.First(r=>r.name.StartsWith("Mountain return trails"));float s=road.Project(keepAt,out _);road.At(s,out var f);keepTravel=f;keepHalf=road.HalfWidth(s);log.Add($"{scene}: kept {keepName} (road s{s:F0}), rebuilt facing a driver coming up from Climbing Ridge Cut");}}
   if(scene=="MountainLoopReverse"){var road=roads.First(r=>r.name.StartsWith("Mountain return trails"));float s=Float("REVERSE_S",64);var p=road.At(s,out var f);keepAt=p;keepTravel=f;keepHalf=road.HalfWidth(s);keepName="Road closed barrier 1";keep=Environment.GetEnvironmentVariable("REVERSE_BARRIER")=="1"; // 0.98: the checked shot shows this part of the road is the race route itself (pavement, arrows): no barrier there
   log.Add($"{scene}: new barrier on the same road at s{s:F0} ({p.x:F1},{p.y:F1},{p.z:F1})");}
   if(g)UnityEngine.Object.DestroyImmediate(g);
   {var old=GameObject.Find(Group);if(old)UnityEngine.Object.DestroyImmediate(old);}
   if(keep){
    if(Physics.Raycast(new Vector3(keepAt.x,keepAt.y+30,keepAt.z),Vector3.down,out var hit,80,~0,QueryTriggerInteraction.Ignore))keepAt.y=hit.point.y;
    var parent=new GameObject(Group).transform;UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(parent.gameObject,sc);
    var b=Build(parent,keepName,keepAt,keepTravel,keepHalf);log.Add($"{scene}: {b.name} now at ({b.position.x:F2},{b.position.y:F2},{b.position.z:F2}) facing {Vector3.SignedAngle(Vector3.forward,b.forward,Vector3.up):F0} (travel direction), width {2*(keepHalf+1.2f):F1} m");}
   EditorSceneManager.MarkSceneDirty(sc);EditorSceneManager.SaveScene(sc);}
  AssetDatabase.SaveAssets();Directory.CreateDirectory(Out);File.WriteAllLines(Out+"/apply.txt",log);EditorApplication.Exit(0);}
 public static void Trail(){
  var log=new List<string>();string name=Environment.GetEnvironmentVariable("PROBE_ROAD");
  foreach(var scene in Environment.GetEnvironmentVariable("PROBE_SCENES").Split(',')){
   EditorSceneManager.OpenScene("Assets/Scenes/"+scene+".unity");Physics.SyncTransforms();
   var roads=UnityEngine.Object.FindObjectsByType<Racer.RaceRoad>(FindObjectsInactive.Exclude,FindObjectsSortMode.None).Where(r=>r.points!=null&&r.points.Length>3).ToList();foreach(var r in roads)r.Initialize();
   var ex=roads.First(r=>r.name.StartsWith("Mountain return trails"));
   foreach(var w in UnityEngine.Object.FindObjectsByType<Racer.WoodlandRoute>(FindObjectsInactive.Exclude,FindObjectsSortMode.None).Where(w=>w.points!=null&&w.points.Length>3&&w.title.StartsWith(name))){w.Initialize();
    log.Add($"==== {scene} trail '{w.title}' length {w.Length:F0} halfWidth {w.halfWidth}");
    for(float s=0;s<Mathf.Min(w.Length,160);s+=6){var p=w.At(s,out var f);float se=ex.Project(p,out float lat);log.Add($"   s{s:F0} ({p.x:F1},{p.y:F1},{p.z:F1}) heading {Vector3.SignedAngle(Vector3.forward,f,Vector3.up):F0}; exploration road s{se:F0} lateral {lat:F1}");}}}
  Directory.CreateDirectory(Out);File.WriteAllLines(Out+"/trail.txt",log);EditorApplication.Exit(0);}
 public static void List(){
  var log=new List<string>();
  foreach(var scene in Environment.GetEnvironmentVariable("PROBE_SCENES").Split(',')){
   var sc=EditorSceneManager.OpenScene("Assets/Scenes/"+scene+".unity");var g=GameObject.Find(Group);log.Add("==== "+scene+(g?"":" (no group)"));
   var race=UnityEngine.Object.FindAnyObjectByType<Racer.RaceDirector>();
   foreach(var r in UnityEngine.Object.FindObjectsByType<Racer.RaceRoad>(FindObjectsInactive.Exclude,FindObjectsSortMode.None))if(r.points!=null&&r.points.Length>3)r.Initialize();
   foreach(var w in UnityEngine.Object.FindObjectsByType<Racer.WoodlandRoute>(FindObjectsInactive.Exclude,FindObjectsSortMode.None))if(w.points!=null&&w.points.Length>3)w.Initialize();
   if(!g)continue;
   foreach(Transform t in g.transform){
    var p=t.position;string near="";float best=1e9f;
    foreach(var r in UnityEngine.Object.FindObjectsByType<Racer.RaceRoad>(FindObjectsInactive.Exclude,FindObjectsSortMode.None)){if(r.points==null||r.points.Length<4)continue;float s=r.Project(p,out float lat);var c=r.At(s,out _);if(Mathf.Abs(c.y-p.y)<9&&lat<best){best=lat;near=r.name+" s"+s.ToString("F0")+(r==race.road?" (RACE ROAD)":"");}}
    float bestT=1e9f;string nt="";
    foreach(var w in UnityEngine.Object.FindObjectsByType<Racer.WoodlandRoute>(FindObjectsInactive.Exclude,FindObjectsSortMode.None)){if(w.points==null||w.points.Length<4)continue;float s=w.Project(p,out float lat);var c=w.At(s,out _);if(Mathf.Abs(c.y-p.y)<9&&lat<bestT){bestT=lat;nt=w.name;}}
    log.Add($"  {t.name} at ({p.x:F2},{p.y:F2},{p.z:F2}) heading {Vector3.SignedAngle(Vector3.forward,t.forward,Vector3.up):F0}; nearest road {near} {best:F1} m; nearest trail {nt} {bestT:F1} m");}
   // any other lettering that says closed (outside the group)
   foreach(var tm in UnityEngine.Object.FindObjectsByType<TextMesh>(FindObjectsInactive.Include,FindObjectsSortMode.None))if(tm.text.ToUpper().Contains("CLOSED")&&(!g||!tm.transform.IsChildOf(g.transform)))log.Add($"  OTHER closed text '{tm.text}' {tm.name} at {tm.transform.position:F1}");}
  Directory.CreateDirectory(Out);File.WriteAllLines(Out+"/barriers.txt",log);EditorApplication.Exit(0);}
}
