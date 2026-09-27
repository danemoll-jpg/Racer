using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Object=UnityEngine.Object;
namespace Racer.Editor {
public static class RouteAtlasPass {
 const string Evidence="Docs/RouteAtlas",Folder="Assets/Track/RouteAtlas";
 static void Guard(){if(Application.isPlaying||EditorApplication.isCompiling||UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)throw new Exception("Saved compiled edit mode required");Directory.CreateDirectory(Evidence);Directory.CreateDirectory(Folder);}
 static void Save(){var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();}
 static float Ground(Vector3 p,float range=200){var hits=Physics.RaycastAll(p+Vector3.up*range,Vector3.down,range*2,~0,QueryTriggerInteraction.Ignore).Where(h=>h.collider.name.StartsWith("Ground")||h.collider.name.Contains("pavement")).OrderBy(h=>Math.Abs(h.point.y-p.y)).ToArray();if(hits.Length==0)throw new Exception("No support at "+p);return hits[0].point.y;}
 public static string PromoteReverse(){
  Guard();EditorSceneManager.OpenScene("Assets/Scenes/ForestLoopReverse.unity");var race=Object.FindAnyObjectByType<RaceDirector>();var road=race.road;road.Initialize();Physics.SyncTransforms();
  var branch=Object.FindObjectsByType<WoodlandRoute>().Single(b=>b.title=="Granite Saddle");float entry=branch.entryRoad,exit=branch.exitRoad;var oldPoints=road.points.ToArray();var oldStations=new float[oldPoints.Length];for(int i=1;i<oldPoints.Length;i++)oldStations[i]=oldStations[i-1]+Vector3.Distance(oldPoints[i-1],oldPoints[i]);
  var gateStations=race.gates.Select(g=>road.Project(g.transform.position,out _)).ToArray();var branches=Object.FindObjectsByType<WoodlandRoute>().Where(b=>b!=branch).ToArray();var branchEnds=branches.Select(b=>(entry:road.At(b.entryRoad,out _),exit:road.At(b.exitRoad,out _))).ToArray();
  var layout=Object.FindAnyObjectByType<ForestLayout>();var jumps=layout.jumpStarts.Select((s,i)=>(a:s,b:layout.jumpEnds[i],name:layout.jumpNames[i],start:road.At(s,out _),end:road.At(layout.jumpEnds[i],out _))).ToArray();
  var promoted=branch.points.Select(p=>new Vector3(p.x,Ground(p),p.z)).ToArray();
  var points=new List<Vector3>();var geometry=new List<float>();float OldGeometry(int i)=>road.geometryStations!=null&&road.geometryStations.Length==oldPoints.Length?road.geometryStations[i]:oldStations[i];
  for(int i=0;i<oldPoints.Length;i++)if(oldStations[i]<entry){points.Add(oldPoints[i]);geometry.Add(OldGeometry(i));}
  int first=Array.FindIndex(oldStations,s=>s>=entry),last=Array.FindIndex(oldStations,s=>s>=exit);float ga=OldGeometry(first),gb=OldGeometry(last),gl=road.geometryLength>0?road.geometryLength:road.Length;
  for(int i=0;i<promoted.Length;i++){points.Add(promoted[i]);geometry.Add(Mathf.Repeat(ga+Mathf.DeltaAngle(ga/gl*360,gb/gl*360)/360*gl*i/(promoted.Length-1),gl));}
  for(int i=0;i<oldPoints.Length;i++)if(oldStations[i]>exit){points.Add(oldPoints[i]);geometry.Add(OldGeometry(i));}
  road.points=points.ToArray();road.geometryStations=geometry.ToArray();road.geometryLength=gl;road.Initialize();
  float newEntry=road.Project(promoted[0],out _),newExit=road.Project(promoted[^1],out _);
  for(int i=1;i<race.gates.Length;i++)if(gateStations[i]>entry&&gateStations[i]<exit){float s=Mathf.Lerp(newEntry,newExit,Mathf.InverseLerp(entry,exit,gateStations[i]));var p=road.At(s,out var f);race.gates[i].transform.SetPositionAndRotation(p+Vector3.up*1.6f,Quaternion.LookRotation(Vector3.ProjectOnPlane(f,Vector3.up)));}
  float origin=road.Project(race.gates[0].transform.position,out _);race.gates=race.gates.OrderBy(g=>road.Relative(road.Project(g.transform.position,out _),origin)).ToArray();
  for(int i=0;i<branches.Length;i++){var b=branches[i];b.entryRoad=road.Project(branchEnds[i].entry,out _);b.exitRoad=road.Project(branchEnds[i].exit,out _);b.bypassedGates=Enumerable.Range(1,race.gates.Length-1).Where(k=>road.Relative(road.Project(race.gates[k].transform.position,out _),b.entryRoad)<road.Relative(b.exitRoad,b.entryRoad)).ToArray();EditorUtility.SetDirty(b);}
  var kept=jumps.Where(j=>!(j.a>=entry&&j.a<=exit)&&!(j.b>=entry&&j.b<=exit)).ToArray();layout.jumpStarts=kept.Select(j=>road.Project(j.start,out _)).ToArray();layout.jumpEnds=kept.Select(j=>road.Project(j.end,out _)).ToArray();layout.jumpNames=kept.Select(j=>j.name).ToArray();EditorUtility.SetDirty(layout);
  foreach(var t in Object.FindObjectsByType<TextMesh>())if(t.text.IndexOf("Granite Saddle",StringComparison.OrdinalIgnoreCase)>=0){t.text=t.text.Replace("OPTIONAL","MAIN ROUTE").Replace("SHORTCUT","MAIN ROUTE");EditorUtility.SetDirty(t);}
  Object.DestroyImmediate(branch);race.courseId="forest-reverse-v6-granite-main";EditorUtility.SetDirty(road);EditorUtility.SetDirty(race);
  Save();File.WriteAllText(Evidence+"/promotion.txt",$"ForestLoopReverse: Granite Saddle promoted from WoodlandRoute optional branch to RaceDirector.road main centreline.\nOld stations {entry:F3}..{exit:F3}; new {newEntry:F3}..{newExit:F3}.\nExisting X/Z alignment preserved exactly; navigation Y sampled from existing supporting colliders. No mesh, terrain or collider changed.\nOld House 3 detour no longer required. Affected gates relocated, remaining branch station/bypass references and ForestLayout stations remapped. AI, recovery and wrong-way use the same main road reference.\nCourse ID: {race.courseId}. Forward scene unchanged: no authored Granite counterpart.\n");return "Promoted Granite Saddle with route data only";
 }
 static bool LaurelProtected(string scene,Vector3 p)=>scene=="StreetLoopReverse"&&p.x>140&&p.x<565&&p.z> -390&&p.z<110;
 public static string Guidance(){
  Guard();var reports=new List<string>();
  foreach(var path in ReverseReviewRelease.Scenes.Concat(new[]{"Assets/Scenes/MountainLoop.unity","Assets/Scenes/MountainLoopReverse.unity"})){
   var scene=EditorSceneManager.OpenScene(path);var race=Object.FindAnyObjectByType<RaceDirector>();var road=race.road;road.Initialize();Physics.SyncTransforms();
   if(GameObject.Find("Route atlas direction guidance"))throw new Exception("Guidance already applied to "+scene.name);
   var root=new GameObject("Route atlas direction guidance").transform;var branches=Object.FindObjectsByType<WoodlandRoute>();
   var teal=AssetDatabase.LoadAssetAtPath<Material>("Assets/Track/Discovery/CR117 main teal.mat");var gold=AssetDatabase.LoadAssetAtPath<Material>("Assets/Track/Discovery/CR117 alternate gold.mat");if(!teal||!gold)throw new Exception("Existing guidance materials missing");
   var old=Object.FindObjectsByType<MeshFilter>().Where(m=>m.name.IndexOf("arrow",StringComparison.OrdinalIgnoreCase)>=0&&m.GetComponent<Renderer>()&&!m.GetComponent<Collider>()).ToArray();int suppressed=0;
   foreach(var m in old){var p=m.GetComponent<Renderer>().bounds.center;if(LaurelProtected(scene.name,p))continue;
    if(m.name=="Main teal trail arrow"||m.name=="Optional gold trail arrow"||m.name.Contains("continuous gold arrow")){m.gameObject.SetActive(false);suppressed++;}
   }
   var placed=new List<Vector3>();int mainCount=0,optionalCount=0,protectedCount=0,unsupported=0;
   bool Paint(Vector3 p,Vector3 heading,bool optional,string label){
    if(LaurelProtected(scene.name,p)){protectedCount++;return false;}if(placed.Any(q=>Vector3.Distance(q,p)<18))return false;
    var f=Vector3.ProjectOnPlane(heading,Vector3.up).normalized;var right=Vector3.Cross(Vector3.up,f);float w=optional?1:1.5f;
    var shape=new[]{new Vector2(-w*.4f,-3),new Vector2(w*.4f,-3),new Vector2(w*.4f,0),new Vector2(w,0),new Vector2(0,3),new Vector2(-w,0),new Vector2(-w*.4f,0)};var vertices=new List<Vector3>();
    foreach(var v in shape){var q=p+right*v.x+f*v.y;var hits=Physics.RaycastAll(q+Vector3.up*5,Vector3.down,10,~0,QueryTriggerInteraction.Ignore).Where(h=>h.collider.name.StartsWith("Ground")||h.collider.name.Contains("pavement")).OrderBy(h=>Math.Abs(h.point.y-q.y)).ToArray();if(hits.Length==0){unsupported++;return false;}q.y=hits[0].point.y+.075f;vertices.Add(q);}
    if(vertices.Max(v=>v.y)-vertices.Min(v=>v.y)>4){unsupported++;return false;}
    var mesh=new Mesh{vertices=vertices.ToArray(),triangles=new[]{0,6,1,1,6,2,6,5,4,6,4,2,2,4,3}};mesh.RecalculateNormals();mesh.RecalculateBounds();AssetDatabase.CreateAsset(mesh,$"{Folder}/{scene.name}-arrow-{mainCount+optionalCount:D3}.asset");
    var go=new GameObject((optional?"Optional gold / ":"Main teal / ")+label,typeof(MeshFilter),typeof(MeshRenderer));go.transform.SetParent(root);go.GetComponent<MeshFilter>().sharedMesh=mesh;go.GetComponent<Renderer>().sharedMaterial=optional?gold:teal;placed.Add(p);if(optional)optionalCount++;else mainCount++;return true;
   }
   var stations=new List<(float s,string label)>();
   foreach(var b in branches){if(b.title.StartsWith("Laurel"))continue;stations.Add((b.entryRoad-24,"before "+b.title+" fork"));stations.Add((b.entryRoad+22,"main at "+b.title+" fork"));stations.Add((b.exitRoad+24,b.title+" rejoin"));}
   if(scene.name=="ForestLoopReverse")foreach(float s in new[]{110f,150f,185f,570f,630f})stations.Add((s,"Granite Saddle main"));
   for(float s=20;s<road.Length;s+=25){road.At(s-15,out var a);road.At(s+15,out var b);if(Vector3.Angle(Vector3.ProjectOnPlane(a,Vector3.up),Vector3.ProjectOnPlane(b,Vector3.up))>22)stations.Add((s-8,"turn"));}
   for(float s=25;s<road.Length;s+=100)stations.Add((s,"reassurance"));
   foreach(var item in stations){var p=road.At(item.s,out var f);Paint(p,f,false,item.label);}
   foreach(var b in branches){if(b.title.StartsWith("Laurel"))continue;foreach(float s in new[]{Mathf.Max(18,b.entryInset+5),b.Length-22}.Concat(Enumerable.Range(1,(int)(b.Length/130)).Select(i=>i*130f))){var p=b.At(s,out var f);Paint(p,f,true,b.title);}}
   if(root.GetComponentsInChildren<Collider>(true).Length!=0)throw new Exception("Guidance collider created");
   Save();reports.Add($"{scene.name}: main={mainCount}, optional={optionalCount}, replaced legacy paint={suppressed}, protected Laurel placements skipped={protectedCount}, unsupported/jump placements skipped={unsupported}; new arrow colliders=0. Main intervals approximately 100m; turns/forks receive closer guidance (18m minimum separation). Existing Laurel objects untouched.");
  }
  File.WriteAllLines(Evidence+"/guidance.txt",reports);return string.Join("\n",reports);
 }
}
}
