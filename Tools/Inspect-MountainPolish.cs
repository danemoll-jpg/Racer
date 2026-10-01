using System;using System.IO;using System.Linq;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using Racer;using Object=UnityEngine.Object;
public static class InspectMountainPolish {
 public static string Main(){
 if(Application.isPlaying||UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)throw new Exception("Saved edit mode required");
 Directory.CreateDirectory("Docs/MountainPolish");
 foreach(string scene in new[]{"MountainLoopReverse","MountainLoop","LakeWoods"}){
 EditorSceneManager.OpenScene("Assets/Scenes/"+scene+".unity");Physics.SyncTransforms();var race=Object.FindAnyObjectByType<RaceDirector>();race.road.Initialize();
 var targets=scene=="LakeWoods"?new[]{new Vector3(174.8f,48.9f,279.2f)}:new[]{new Vector3(1008.9f,161.8f,89.4f),new Vector3(983.1f,164.6f,121.7f),new Vector3(1009.5f,162.5f,95.9f),new Vector3(997.2f,135.1f,-66.4f),new Vector3(872.3f,111.1f,-89.3f)};
 var report=new {scene,road=race.road.points.Select(p=>new[]{p.x,p.y,p.z}).ToArray(),branches=Object.FindObjectsByType<WoodlandRoute>().Select(b=>new{b.name,b.title,b.entryRoad,b.exitRoad,b.halfWidth,b.aiValidated,b.recommendedSpeed,b.bypassedGates,points=b.points.Select(p=>new[]{p.x,p.y,p.z}).ToArray()}),targets=targets.Select(p=>new{p=p.ToString(),hits=Physics.RaycastAll(p+Vector3.up*60,Vector3.down,150).Where(h=>!h.collider.isTrigger).Select(h=>new{h.collider.name,y=h.point.y}),near=Object.FindObjectsByType<Renderer>().Where(r=>Vector3.Distance(r.bounds.ClosestPoint(p),p)<18).Select(r=>new{r.name,path=PathOf(r.transform),position=r.transform.position.ToString(),min=r.bounds.min.ToString(),max=r.bounds.max.ToString(),mesh=r.GetComponent<MeshFilter>()?AssetDatabase.GetAssetPath(r.GetComponent<MeshFilter>().sharedMesh):""})})};
 File.WriteAllText("Docs/MountainPolish/"+scene+"-before.json",Newtonsoft.Json.JsonConvert.SerializeObject(report,Newtonsoft.Json.Formatting.Indented));
 }return "Saved road, branch, support and dependency inventories for the three affected scenes";}
 static string PathOf(Transform t)=>t.parent?PathOf(t.parent)+"/"+t.name:t.name;
}
