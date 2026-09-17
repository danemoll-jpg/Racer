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
public static class NeighborhoodBatch {
 public const string Dir="Docs/CR016-017", Root="CR-017 lightweight fences";
 static Material Wood=>AssetDatabase.LoadAssetAtPath<Material>(Phase6Buildings.Folder+"/Door timber.mat");
 static Transform Cube(Transform root,string name,Vector3 p,Vector3 scale){var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=name;g.transform.SetParent(root,false);g.transform.localPosition=p;g.transform.localScale=scale;g.GetComponent<Renderer>().sharedMaterial=Wood;Object.DestroyImmediate(g.GetComponent<Collider>());return g.transform;}
 static void Sensor(GameObject g){
  var points=new List<Vector3>();foreach(var r in g.GetComponentsInChildren<Renderer>()){var b=r.bounds;foreach(float x in new[]{-1f,1f})foreach(float y in new[]{-1f,1f})foreach(float z in new[]{-1f,1f})points.Add(g.transform.InverseTransformPoint(b.center+Vector3.Scale(b.extents,new Vector3(x,y,z))));}
  var bounds=new Bounds(Vector3.zero,Vector3.zero);foreach(var p in points)bounds.Encapsulate(p);
  var c=g.GetComponent<BoxCollider>();if(!c)c=g.AddComponent<BoxCollider>();c.center=bounds.center;c.size=Vector3.Max(bounds.size,new Vector3(.3f,.4f,.3f));c.isTrigger=true;
  if(!g.GetComponent<BreakableProp>())g.AddComponent<BreakableProp>();
 }
 public static void Apply(){
  var scene=SceneManager.GetActiveScene();if(Application.isPlaying||scene.isDirty||scene.path!=StreetLoopBuilder.ScenePath||GameObject.Find(Root))throw new Exception("Saved unapplied StreetLoopGreybox in edit mode required");
  var log=new List<string>();var dan=GameObject.Find("Dan - blue X").transform;var p=CompactYard.House;p.y=Phase6Buildings.Ground(p);CR015Neighborhood.Move(dan,p,log);
  var placements=CR015Neighborhood.Folder+"/House placements.json";var data=JsonUtility.FromJson<CR015Neighborhood.Snapshot>(File.ReadAllText(placements));var site=data.sites.Single(s=>s.name==dan.name);site.position=dan.position;site.rotation=dan.rotation;site.centerSetback=StreetLoopBuilder.Nearest(p,StreetLoopBuilder.Route(),out site.road);site.edgeSetback=site.centerSetback-4.5f;site.roadY=site.road.y;File.WriteAllText(placements,JsonUtility.ToJson(data,true));
  // Terrain heights, normals, topology and road support remain untouched; recolor released grass.
  int colors=0;foreach(var f in GameObject.Find("Memory loop - north is +Z").GetComponentsInChildren<MeshFilter>()){
   var mesh=f.sharedMesh;var vs=mesh.vertices;var cs=mesh.colors;bool changed=false;for(int i=0;i<vs.Length;i++){float old=CompactYard.OldDistance(vs[i]);if(old>38)continue;float yard=CompactYard.Distance(vs[i]);float access=CompactYard.AccessDistance(vs[i]);float amount=Mathf.SmoothStep(0,1,Mathf.InverseLerp(CompactYard.Radius-3,CompactYard.Radius+4,yard))*Mathf.SmoothStep(0,1,Mathf.InverseLerp(3,6,access));if(amount<=0)continue;cs[i]=Color.Lerp(cs[i],new Color(.32f,.40f,.26f),amount*.7f);colors++;changed=true;}if(changed){mesh.colors=cs;EditorUtility.SetDirty(mesh);}}
  var woods=GameObject.Find("Woods replacing later subdivisions").transform;var occupied=woods.GetComponentsInChildren<BoxCollider>().Select(c=>new Vector2(c.bounds.center.x,c.bounds.center.z)).ToList();var rng=new System.Random(16017);var road=StreetLoopBuilder.Route();var cut=Phase5Setup.Path();int added=0;
  for(float z=-32;z<143;z+=8)for(float x=316;x<453;x+=8){var v=new Vector3(x+(float)rng.NextDouble()*3-1.5f,0,z+(float)rng.NextDouble()*3-1.5f);if(CompactYard.OldDistance(v)>34||Phase6Vegetation.Reserved(v,road,cut)||occupied.Any(q=>Vector2.Distance(q,new Vector2(v.x,v.z))<6.8f))continue;v.y=Phase6Buildings.Ground(v);float h=15+(float)rng.NextDouble()*5;var g=new GameObject("CR016 trunk "+woods.GetComponentsInChildren<BoxCollider>().Count(c=>c.name.StartsWith("CR016 trunk")).ToString("D3"));g.transform.SetParent(woods);g.transform.position=v+Vector3.up*h*.25f;g.AddComponent<BoxCollider>().size=new(.75f,h*.5f,.75f);occupied.Add(new(v.x,v.z));added++;}
  var mailbox=GameObject.Find("Mailbox - Dan - blue X").transform;StreetLoopBuilder.Nearest(dan.position,road,out var r);var side=Vector3.ProjectOnPlane(dan.position-r,Vector3.up).normalized;var mp=r+side*12+Vector3.Cross(Vector3.up,side)*3;mp.y=Phase6Buildings.Ground(mp);mailbox.SetPositionAndRotation(mp,Quaternion.LookRotation(-side));PrefabUtility.RecordPrefabInstancePropertyModifications(mailbox);
  foreach(var path in new[]{CR015Neighborhood.Folder+"/Mailbox.prefab",CR015Neighborhood.Folder+"/Bend marker.prefab"}){var g=PrefabUtility.LoadPrefabContents(path);try{Sensor(g);PrefabUtility.SaveAsPrefabAsset(g,path);}finally{PrefabUtility.UnloadPrefabContents(g);}}
  // Gameplay boards and their sibling lettering become a single falling assembly.
  foreach(var board in Object.FindObjectsByType<Transform>().Where(t=>t.name=="Jump approach sign"||t.name=="Shortcut direction sign").ToArray()){
   string textName=board.name.StartsWith("Jump")?"Recommended speed":"Shortcut advice";var text=board.parent.Cast<Transform>().Where(t=>t.name==textName).OrderBy(t=>Vector3.Distance(t.position,board.position)).First();var holder=new GameObject("Breakable - "+board.name);holder.transform.SetParent(board.parent);var bp=board.position;bp.y=Phase6Buildings.Ground(bp);holder.transform.position=bp;board.SetParent(holder.transform,true);text.SetParent(holder.transform,true);Sensor(holder);
  }
  var fence=new GameObject("Light timber rail section");Cube(fence.transform,"Left post",new(-1.9f,.65f,0),new(.15f,1.3f,.15f));Cube(fence.transform,"Right post",new(1.9f,.65f,0),new(.15f,1.3f,.15f));foreach(float y in new[]{.5f,1.05f})Cube(fence.transform,"Light rail",new(0,y,0),new(4,.12f,.1f));Sensor(fence);var prefab=PrefabUtility.SaveAsPrefabAsset(fence,CR015Neighborhood.Folder+"/Light timber fence.prefab");Object.DestroyImmediate(fence);var root=new GameObject(Root).transform;
  void Fence(string name,Vector3 at,Quaternion rotation){at.y=Phase6Buildings.Ground(at);var g=(GameObject)PrefabUtility.InstantiatePrefab(prefab,root);g.name=name;g.transform.SetPositionAndRotation(at,rotation);PrefabUtility.RecordPrefabInstancePropertyModifications(g.transform);log.Add(name+" at "+at.ToString("F3"));}
  for(int i=0;i<3;i++)Fence("Dan west yard fence "+i,new Vector3(390,0,-5+i*4.2f),Quaternion.Euler(0,90,0));
  foreach(string name in new[]{"Original house 2","Friend across street - blue circle"}){var t=GameObject.Find(name).transform;for(int i=0;i<3;i++)Fence(name+" side fence "+i,t.TransformPoint(new Vector3(-19,0,-5+i*4.2f)),t.rotation*Quaternion.Euler(0,90,0));}
  log.Add($"Yard 9003.472 -> {Mathf.PI*CompactYard.Radius*CompactYard.Radius:F3} square metres; southern boundary old=-28.1, new={CompactYard.Center.z-CompactYard.Radius:F3}; house south inset=15.1m. Separate narrow road access retained. Added {added} solid trunks; {colors} terrain color entries; zero terrain height edits.");
  Physics.SyncTransforms();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();Phase6Buildings.RefreshVisualBatches();Phase6Vegetation.Refresh(Dir+"/vegetation.txt");File.WriteAllLines(Dir+"/changes.txt",log);
 }
 public static void Capture(string name,bool overview=true){
  var cam=Camera.main;var pos=cam.transform.position;var rot=cam.transform.rotation;bool ortho=cam.orthographic;float size=cam.orthographicSize;var chase=cam.GetComponent<ChaseCamera>();bool enabled=chase.enabled;
  try{chase.enabled=false;cam.orthographic=overview;cam.orthographicSize=130;if(overview)cam.transform.SetPositionAndRotation(new(390,700,53),Quaternion.LookRotation(Vector3.down,Vector3.forward));else{var t=GameObject.Find("Dan - blue X").transform;var target=t.position+Vector3.up*3;cam.transform.SetPositionAndRotation(target+t.forward*38+t.right*22+Vector3.up*17,Quaternion.LookRotation(-t.forward*38-t.right*22-Vector3.up*17));}
  var rt=new RenderTexture(1200,1000,24);var old=cam.targetTexture;var active=RenderTexture.active;try{cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;var tex=new Texture2D(1200,1000,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,1200,1000),0,0);tex.Apply();File.WriteAllBytes(Dir+"/"+name+".png",tex.EncodeToPNG());Object.DestroyImmediate(tex);}finally{cam.targetTexture=old;RenderTexture.active=active;Object.DestroyImmediate(rt);}}
  finally{cam.transform.SetPositionAndRotation(pos,rot);cam.orthographic=ortho;cam.orthographicSize=size;chase.enabled=enabled;}
 }
 public static void Build(){
  var renderer=AssetDatabase.LoadAssetAtPath<UnityEngine.Rendering.Universal.UniversalRendererData>("Assets/Settings/PC_Renderer.asset");var pipeline=AssetDatabase.LoadAssetAtPath<UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset>("Assets/Settings/PC_RPAsset.asset");var pipelineJson=EditorJsonUtility.ToJson(pipeline);var features=renderer.rendererFeatures.ToArray();renderer.rendererFeatures.RemoveAll(f=>f&&!f.isActive);
  try{EditorUtility.SetDirty(renderer);AssetDatabase.SaveAssetIfDirty(renderer);var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{StreetLoopBuilder.ScenePath},locationPathName="Builds/CR016-017/Racer.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development});File.WriteAllText(Dir+"/build.txt",$"{report.summary.result}; errors={report.summary.totalErrors}; warnings={report.summary.totalWarnings}; bytes={report.summary.totalSize}; duration={report.summary.totalTime}");}
  finally{renderer.rendererFeatures.Clear();renderer.rendererFeatures.AddRange(features);EditorUtility.SetDirty(renderer);AssetDatabase.SaveAssetIfDirty(renderer);EditorJsonUtility.FromJsonOverwrite(pipelineJson,pipeline);EditorUtility.SetDirty(pipeline);AssetDatabase.SaveAssetIfDirty(pipeline);}
 }
}
}
