using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using Object=UnityEngine.Object;
namespace Racer.Editor {
// Focused authoring, deliberately separate from the guarded legacy environment rebuild.
public static class CR015Neighborhood {
 public const string Evidence="Docs/CR015", Props="CR-015 modest roadside details", Folder="Assets/Buildings/Roadside";
 public static readonly Vector3 ValleySite=new(418,32.75478f,-166), HouseThreeSite=new(418,32.75478f,-160);
 static readonly string[] Names={"Dan - blue X","Original house 2","Friend across street - blue circle","Original house 3"};
 [Serializable] public class Site {public string name;public Vector3 position;public Quaternion rotation;public float centerSetback,edgeSetback,roadY;public Vector3 road;}
 [Serializable] public class Snapshot {public Site[] sites;public int trees;}
 // Saved placement data is the source for any future deliberate authoring. Never applied on load.
 public static Vector3 AuthoredPosition(string name,Vector3 fallback){var path=Folder+"/House placements.json";if(!File.Exists(path))return fallback;var data=JsonUtility.FromJson<Snapshot>(File.ReadAllText(path));var site=data.sites.FirstOrDefault(s=>s.name==name);return site==null?fallback:site.position;}
 static List<Vector3> Road=>StreetLoopBuilder.Route();
 static Transform House(string name)=>GameObject.Find(name).transform;
 public static void SnapshotSites(string label){
  Directory.CreateDirectory(Evidence);var road=Road;
  var sites=Names.Select(n=>{var t=House(n);float d=StreetLoopBuilder.Nearest(t.position,road,out var r);return new Site{name=n,position=t.position,rotation=t.rotation,centerSetback=d,edgeSetback=d-4.5f,roadY=r.y,road=r};}).ToArray();
  File.WriteAllText(Evidence+"/"+label+"-sites.json",JsonUtility.ToJson(new Snapshot{sites=sites,trees=GameObject.Find("Woods replacing later subdivisions").GetComponentsInChildren<BoxCollider>().Length},true));
 }
 public static void Capture(string label){
  var cam=Camera.main;var p=cam.transform.position;var q=cam.transform.rotation;bool o=cam.orthographic;float s=cam.orthographicSize,f=cam.fieldOfView;var chase=cam.GetComponent<ChaseCamera>();bool ce=chase.enabled;
  void Shot(string name,Vector3 pos,Vector3 target,bool top){
   cam.orthographic=top;cam.orthographicSize=225;cam.fieldOfView=65;cam.transform.SetPositionAndRotation(pos,Quaternion.LookRotation(target-pos,top?Vector3.forward:Vector3.up));
   var rt=new RenderTexture(1440,1000,24);var prev=cam.targetTexture;var active=RenderTexture.active;
   try{cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;var t=new Texture2D(1440,1000,TextureFormat.RGB24,false);t.ReadPixels(new Rect(0,0,1440,1000),0,0);t.Apply();File.WriteAllBytes(Evidence+"/"+label+"-"+name+".png",t.EncodeToPNG());Object.DestroyImmediate(t);
    if(top){var marks=Names.Select(n=>{var v=cam.WorldToViewportPoint(House(n).position);return new {name=n,x=v.x*1440,y=(1-v.y)*1000};}).ToList();var drop=cam.WorldToViewportPoint(new Vector3(514.8f,82,-132));File.WriteAllText(Evidence+"/"+label+"-labels.json",Newtonsoft.Json.JsonConvert.SerializeObject(new{marks,drop=new{x=drop.x*1440,y=(1-drop.y)*1000}},Newtonsoft.Json.Formatting.Indented));}
   }finally{cam.targetTexture=prev;RenderTexture.active=active;Object.DestroyImmediate(rt);}
  }
  try{chase.enabled=false;Shot("overview",new(442,700,-49),new(442,0,-49),true);
   foreach(string n in Names){var t=House(n);Shot(n.StartsWith("Dan")?"dan":n.Contains("2")?"house2":n.Contains("3")?"house3":"friend",t.position+t.forward*30+t.right*18+Vector3.up*14,t.position+Vector3.up*3,false);}
   var road=Road;foreach(var pair in new[]{("drive-north",new Vector3(477,0,-50)),("drive-drop",new Vector3(506,0,-114)),("drive-dan",new Vector3(412,0,164))}){int i=Phase5Setup.Closest(road,pair.Item2);var fw=(road[i+1]-road[i]).normalized;Shot(pair.Item1,road[i]-fw*7.5f+Vector3.up*4.25f,road[i]+fw*12+Vector3.up*1.5f,false);}
  }finally{cam.transform.SetPositionAndRotation(p,q);cam.orthographic=o;cam.orthographicSize=s;cam.fieldOfView=f;chase.enabled=ce;}
 }
 public static void Baseline(){if(Application.isPlaying)throw new Exception("Edit mode required");SnapshotSites("before");Capture("before");
  var road=Road;int i=Phase5Setup.Closest(road,new Vector3(422,0,146));var route=new WoodlandBenchmark.Route{name="affected-neighborhood",points=Enumerable.Range(0,260).Select(j=>road[i+j]).ToArray(),speed=15};
  File.WriteAllText(Evidence+"/profile-routes.json",JsonUtility.ToJson(new WoodlandBenchmark.Plan{seconds=30,routes=new[]{route}},true));
 }
 // Only the three requested sites; unrelated buildings and accepted architecture are never rebuilt.
 public static void Apply(){
  var scene=SceneManager.GetActiveScene();if(Application.isPlaying||scene.isDirty||scene.path!=StreetLoopBuilder.ScenePath||GameObject.Find(Props))throw new Exception("Saved StreetLoopGreybox, edit mode, unapplied CR015 required");
  var before=JsonUtility.FromJson<Snapshot>(File.ReadAllText(Evidence+"/before-sites.json"));var road=Road;var log=new List<string>();
  var dan=House(Names[0]);StreetLoopBuilder.Nearest(dan.position,road,out var dr);var away=Vector3.ProjectOnPlane(dan.position-dr,Vector3.up).normalized;var dp=dan.position+away*3;dp.y=Phase6Buildings.Ground(dp);Move(dan,dp,log);
  var friend=House(Names[2]);StreetLoopBuilder.Nearest(friend.position,road,out var fr);var normal=Vector3.ProjectOnPlane(friend.position-fr,Vector3.up).normalized;var hp=fr-normal*before.sites[1].centerSetback;hp.y=Phase6Buildings.Ground(hp);Move(House(Names[1]),hp,log);
  // Extend the existing low bowl toward the crest. Blend into existing hills outside the local site;
  // do not touch the road/ordinary shoulder envelope. Same mesh provides visible and physical ground.
  var old=House(Names[3]).position;var terrain=GameObject.Find("Memory loop - north is +Z").GetComponentsInChildren<MeshFilter>();
  var original=new Dictionary<Vector2,float>();foreach(var mf in terrain)foreach(var v in mf.sharedMesh.vertices)original[new(v.x,v.z)]=v.y;
  float Sample(float x,float z){float ax=Mathf.Floor((x+800)/2)*2-800,az=Mathf.Floor((z+750)/2)*2-750;float u=(x-ax)/2,v=(z-az)/2;float a=original[new(ax,az)],b=original[new(ax+2,az)],c=original[new(ax,az+2)],d=original[new(ax+2,az+2)];return u+v<=1?a+(b-a)*u+(c-a)*v:d+(c-d)*(1-u)+(b-d)*(1-v);}
  var changed=new HashSet<Vector2>();int tiles=0;float deepest=0;
  foreach(var mf in terrain){var mesh=mf.sharedMesh;var vertices=mesh.vertices;bool edit=false;for(int k=0;k<vertices.Length;k++){var v=vertices[k];float d=Vector2.Distance(new(v.x,v.z),new(ValleySite.x,ValleySite.z));if(d>=52)continue;float rd=StreetLoopBuilder.Nearest(v,road,out _);if(rd<=18)continue;float weight=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(17,52,d));float translated=Sample(v.x-(ValleySite.x-old.x),v.z-(ValleySite.z-old.z));float y=Mathf.Lerp(v.y,Mathf.Min(v.y,translated),weight);if(Mathf.Abs(y-v.y)<.0001f)continue;deepest=Mathf.Max(deepest,v.y-y);v.y=y;vertices[k]=v;changed.Add(new(v.x,v.z));edit=true;}if(edit){mesh.vertices=vertices;mesh.RecalculateBounds();EditorUtility.SetDirty(mesh);mf.GetComponent<MeshCollider>().sharedMesh=null;mf.GetComponent<MeshCollider>().sharedMesh=mesh;tiles++;}}
  // Shared central-difference normals also agree at duplicate tile boundaries.
  var final=new Dictionary<Vector2,float>();foreach(var mf in terrain)foreach(var v in mf.sharedMesh.vertices)final[new(v.x,v.z)]=v.y;
  foreach(var mf in terrain){var mesh=mf.sharedMesh;var v=mesh.vertices;var normals=mesh.normals;bool edit=false;for(int k=0;k<v.Length;k++){var p=v[k];if(!changed.Contains(new(p.x,p.z))&&!changed.Contains(new(p.x-2,p.z))&&!changed.Contains(new(p.x+2,p.z))&&!changed.Contains(new(p.x,p.z-2))&&!changed.Contains(new(p.x,p.z+2)))continue;float H(float x,float z)=>final.TryGetValue(new(x,z),out var h)?h:p.y;normals[k]=new Vector3(H(p.x-2,p.z)-H(p.x+2,p.z),4,H(p.x,p.z-2)-H(p.x,p.z+2)).normalized;edit=true;}if(edit){mesh.normals=normals;EditorUtility.SetDirty(mesh);}}
  Physics.SyncTransforms();var vp=HouseThreeSite;vp.y=Phase6Buildings.Ground(vp);Move(House(Names[3]),vp,log);log.Add($"Valley extension: {changed.Count} unique vertices, {tiles} tiles, maximum lowering {deepest:F3}m; no road/shoulder edits within 18m, no raised terrain or flattened hillside.");
  // Clear only footprint/access conflicts; retain woodland elsewhere. Reground trees on edited ground.
  int removed=0,grounded=0;var woods=GameObject.Find("Woods replacing later subdivisions");
  foreach(var c in woods.GetComponentsInChildren<BoxCollider>()){var p=c.bounds.center;bool conflict=false;foreach(string n in new[]{Names[0],Names[1],Names[3]}){var t=House(n);var local=t.InverseTransformPoint(p);if(Vector2.Distance(new(p.x,p.z),new(t.position.x,t.position.z))<16)conflict=true;var end=t.position+t.forward*18;var d=p-end;d.y=0;if(d.magnitude<4)conflict=true;}if(conflict){log.Add("Removed conflicting tree "+c.name+" "+p);Object.DestroyImmediate(c.gameObject);removed++;continue;}if(Vector2.Distance(new(p.x,p.z),new(ValleySite.x,ValleySite.z))<54){float y=Phase6Buildings.Ground(p),dy=y-c.bounds.min.y;if(Mathf.Abs(dy)>.01f){c.transform.position+=Vector3.up*dy;grounded++;}}}
  log.Add($"Trees removed {removed}; terrain-local trunks regrounded {grounded}. Remaining {woods.GetComponentsInChildren<BoxCollider>().Length}.");
  AddProps(log);Physics.SyncTransforms();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
  var oldReport=File.ReadAllBytes("Docs/PHASE6_RENDER_BATCHES.txt");try{Phase6Buildings.RefreshVisualBatches();}finally{File.WriteAllBytes("Docs/PHASE6_RENDER_BATCHES.txt",oldReport);}
  Phase6Vegetation.Refresh(Evidence+"/vegetation.txt");SnapshotSites("after");File.Copy(Evidence+"/after-sites.json",Folder+"/House placements.json",true);File.WriteAllLines(Evidence+"/changes.txt",log);
 }
 public static void Move(Transform t,Vector3 p,List<string> log){var old=t.position;t.position=p;var f=t.Find("Foundation");var a=t.Find("Phase 6 architecture");float ground=Phase6Buildings.Ground(p),low=ground,high=ground;foreach(float x in new[]{-.5f,.5f})foreach(float z in new[]{-.5f,.5f}){var v=f.TransformPoint(new(x,0,z));float h=Phase6Buildings.Ground(v);low=Mathf.Min(low,h);high=Mathf.Max(high,h);}float floor=Mathf.Max(p.y+a.localPosition.y,high+.25f);var ap=a.localPosition;ap.y=floor-p.y;a.localPosition=ap;PrefabUtility.RecordPrefabInstancePropertyModifications(a);float bottom=low-.15f;f.localPosition=new(0,(bottom+floor)*.5f-p.y,0);f.localScale=new(f.localScale.x,floor-bottom,f.localScale.z);
  foreach(Transform child in t.Cast<Transform>().Where(c=>c.name.StartsWith("Entrance step")).ToArray())Object.DestroyImmediate(child.gameObject);
  float dx=t.name==Names[1]?-2:0;for(int i=0;i<3;i++){float z=5.35f+i*.6f;float g=Phase6Buildings.Ground(t.TransformPoint(new(dx,0,z)));float top=floor-i*.24f;if(top<g+.05f)continue;Cube(t,"Entrance step "+(i+1),new(dx,(top+g)*.5f-p.y,z),new(2.1f,top-g+.15f,.65f),f.GetComponent<Renderer>().sharedMaterial,true);}
  log.Add($"{t.name}: {old:F4} -> {p:F4}, delta {p-old:F4}; floor {floor:F3}, foundation bottom {bottom:F3}. Accepted prefab and rotation retained.");
 }
 static Transform Cube(Transform parent,string name,Vector3 p,Vector3 s,Material mat,bool solid=false){var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=name;g.transform.SetParent(parent,false);g.transform.localPosition=p;g.transform.localScale=s;g.GetComponent<Renderer>().sharedMaterial=mat;if(!solid)Object.DestroyImmediate(g.GetComponent<Collider>());return g.transform;}
 static GameObject Prefab(bool sign){string path=Folder+(sign?"/Bend marker.prefab":"/Mailbox.prefab");var existing=AssetDatabase.LoadAssetAtPath<GameObject>(path);if(existing)return existing;var g=new GameObject(sign?"Bend marker":"Mailbox");Material M(string n)=>AssetDatabase.LoadAssetAtPath<Material>(Phase6Buildings.Folder+"/"+n+".mat");
  Cube(g.transform,"Timber post",new(0,.65f,0),new(.13f,1.3f,.13f),M("Door timber"));
  if(sign){Cube(g.transform,"Ochre warning panel",new(0,1.8f,0),new(.85f,.85f,.075f),M("Workshop ochre")).localRotation=Quaternion.Euler(0,0,45);foreach(int side in new[]{-1,1}){var stripe=Cube(g.transform,"Simple bend chevron",new(side*.13f,1.8f,.05f),new(.10f,.4f,.04f),M("Charcoal roof"));stripe.localRotation=Quaternion.Euler(0,0,side*45);}}
  else{Cube(g.transform,"Slate mailbox",new(0,1.25f,0),new(.48f,.42f,.72f),M("Slate blue siding"));Cube(g.transform,"Cap",new(0,1.48f,0),new(.51f,.08f,.75f),M("Charcoal roof"));Cube(g.transform,"Ivory door",new(0,1.25f,.37f),new(.39f,.32f,.04f),M("Ivory trim"));Cube(g.transform,"Small flag",new(.26f,1.43f,0),new(.04f,.17f,.18f),M("Diner red"));}
  // Small visual props intentionally have no physical snag/launch surface. No destruction system.
  var result=PrefabUtility.SaveAsPrefabAsset(g,path);Object.DestroyImmediate(g);return result;
 }
 static void AddProps(List<string> log){Directory.CreateDirectory(Folder);AssetDatabase.Refresh();var root=new GameObject(Props).transform;var mail=Prefab(false);var sign=Prefab(true);var road=Road;
  foreach(string n in Names){var t=House(n);StreetLoopBuilder.Nearest(t.position,road,out var r);var side=Vector3.ProjectOnPlane(t.position-r,Vector3.up).normalized;var tangent=Vector3.Cross(Vector3.up,side);var p=r+side*12+tangent*3;p.y=Phase6Buildings.Ground(p);var g=(GameObject)PrefabUtility.InstantiatePrefab(mail,root);g.name="Mailbox - "+n;g.transform.SetPositionAndRotation(p,Quaternion.LookRotation(-side));PrefabUtility.RecordPrefabInstancePropertyModifications(g.transform);log.Add(g.name+" at "+p.ToString("F3"));}
  foreach(var where in new[]{new Vector3(510,0,-125),new Vector3(337,0,302)}){int i=Phase5Setup.Closest(road,where);var f=Vector3.ProjectOnPlane(road[i+1]-road[i],Vector3.up).normalized;var p=road[i]+Vector3.Cross(Vector3.up,f)*12;p.y=Phase6Buildings.Ground(p);var g=(GameObject)PrefabUtility.InstantiatePrefab(sign,root);g.name="Simple bend warning";g.transform.SetPositionAndRotation(p,Quaternion.LookRotation(-f));PrefabUtility.RecordPrefabInstancePropertyModifications(g.transform);log.Add(g.name+" at "+p.ToString("F3"));}
  log.Add("4 reusable mailboxes + 2 simple bend signs. No names, addresses or historical claims. No colliders on small props; >=11m road-center clearance, outside ordinary shoulder.");
 }
}
}


