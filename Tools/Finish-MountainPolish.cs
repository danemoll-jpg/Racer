using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using Racer;using Object=UnityEngine.Object;
public static class FinishMountainPolish {
 static string Key(Transform t){var id=GlobalObjectId.GetGlobalObjectIdSlow(t);return id.targetObjectId+":"+id.targetPrefabId;}
 static float Flat(Vector3 a,Vector3 b)=>Vector2.Distance(new(a.x,a.z),new(b.x,b.z));
 public static string Main(){var notes=new List<string>();foreach(string scene in new[]{"MountainLoopReverse","MountainLoop"}){
 EditorSceneManager.OpenScene("Assets/Scenes/"+scene+".unity");var live=UnityEngine.SceneManagement.SceneManager.GetActiveScene();var root=live.GetRootGameObjects();var transforms=root.SelectMany(g=>g.GetComponentsInChildren<Transform>(true)).ToDictionary(t=>Key(t));
 string temp="Assets/Scenes/MountainPolishBaseline.unity";File.Copy("Temp/"+scene+"-baseline.unity",temp,true);AssetDatabase.ImportAsset(temp);var baseline=EditorSceneManager.OpenScene(temp,OpenSceneMode.Additive);
 foreach(var sign in baseline.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<PhysicalSign>(true))){string id=Key(sign.transform);if(transforms.TryGetValue(id,out var target))target.localPosition=sign.transform.localPosition;}
 EditorSceneManager.CloseScene(baseline,true);AssetDatabase.DeleteAsset(temp);Physics.SyncTransforms();
 if(scene.EndsWith("Reverse")){
 var race=Object.FindAnyObjectByType<RaceDirector>();var b=Object.FindObjectsByType<WoodlandRoute>().Single(b=>b.title=="Summit Traverse");
 Vector3 Map(Vector3 p){float d=Flat(p,new(993,0,111));if(d>38||p.y<158||p.y>168)return p;float y=163.65f-(p.x-990)*.075f+(p.z-115)*.02f;float weight=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(22,38,d));p.y=Mathf.Lerp(p.y,y,weight);return p;}
 foreach(var mf in Object.FindObjectsByType<MeshFilter>().Where(m=>m.sharedMesh&&(m.name.Contains("driving surface")||m.name.Contains("arrow")||m.name.Contains("Main teal")||m.name.Contains("Optional gold")))){var v=mf.sharedMesh.vertices;bool changed=false;for(int i=0;i<v.Length;i++){var p=mf.transform.TransformPoint(v[i]);var q=Map(p);if(Vector3.Distance(p,q)>.0001f){v[i]=mf.transform.InverseTransformPoint(q);changed=true;}}if(!changed)continue;var m=Object.Instantiate(mf.sharedMesh);m.vertices=v;m.RecalculateNormals();m.RecalculateBounds();string path=AssetDatabase.GenerateUniqueAssetPath("Assets/Track/MountainPolish/entry-"+mf.name.Replace('/','-')+".asset");AssetDatabase.CreateAsset(m,path);mf.sharedMesh=m;if(mf.TryGetComponent<MeshCollider>(out var mc))mc.sharedMesh=m;}
 b.points=b.points.Select(Map).ToArray();race.road.points=race.road.points.Select(Map).ToArray();b.bypassedGates=race.gates.Select((g,i)=>new{i,s=race.road.Project(g.transform.position,out _)}).Where(g=>g.s>b.entryRoad+5&&g.s<b.exitRoad-5).Select(g=>g.i).ToArray();EditorUtility.SetDirty(b);EditorUtility.SetDirty(race.road);if(!b.GetComponent<ReverseShortcutGuidance>())b.gameObject.AddComponent<ReverseShortcutGuidance>().lookAhead=9;notes.Add("Summit bypass gates="+string.Join(",",b.bypassedGates));
 }
 Physics.SyncTransforms();int posts=0;foreach(var sign in Object.FindObjectsByType<PhysicalSign>())foreach(var post in sign.GetComponentsInChildren<Renderer>().Where(r=>r.name.IndexOf("post",StringComparison.OrdinalIgnoreCase)>=0)){
 var p=post.bounds.center;var main=Object.FindAnyObjectByType<RaceDirector>().road;main.Project(p,out float rd);float bd=Object.FindObjectsByType<WoodlandRoute>().Select(b=>{b.Project(p,out float d);return d;}).DefaultIfEmpty(999).Min();if(Math.Min(rd,bd)>30)continue;var hits=Physics.RaycastAll(new(p.x,post.bounds.min.y+12,p.z),Vector3.down,80,1,QueryTriggerInteraction.Ignore).Where(h=>h.collider.name.StartsWith("Ground_")).ToArray();if(hits.Length==0)continue;float ground=hits.Max(h=>h.point.y),gap=post.bounds.min.y-ground;if(gap<.08f||gap>20)continue;var scale=post.transform.localScale;scale.y*=(post.bounds.size.y+gap+.04f)/post.bounds.size.y;post.transform.localScale=scale;post.transform.position-=Vector3.up*(gap+.04f)*.5f;posts++;}
 // Match support palette to the existing green earth; no white vertex-color walls.
 var support=GameObject.Find("Ground_MountainPolish supported shoulders").GetComponent<MeshFilter>();var sm=support.sharedMesh;sm.colors=Enumerable.Repeat(new Color(.39f,.48f,.29f),sm.vertexCount).ToArray();EditorUtility.SetDirty(sm);
 EditorSceneManager.MarkSceneDirty(live);EditorSceneManager.SaveScene(live);AssetDatabase.SaveAssets();notes.Add(scene+" grounded extended posts="+posts);
 }
 File.WriteAllLines("Docs/MountainPolish/finish.txt",notes);return string.Join("\n",notes);}
}


