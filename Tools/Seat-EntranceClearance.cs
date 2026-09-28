using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using Racer;using Object=UnityEngine.Object;
public static class SeatEntranceClearance {
 public static string Main(){
 if(Application.isPlaying||BuildPipeline.isBuildingPlayer||UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)throw new Exception("Saved idle Editor required");var rows=new List<string>();
 foreach(string name in new[]{"StreetLoopGreybox","LakeWoods","StreetLoopReverse","ForestLoopReverse","MountainLoop","MountainLoopReverse"}){
 var scene=EditorSceneManager.OpenScene("Assets/Scenes/"+name+".unity");var race=Object.FindAnyObjectByType<RaceDirector>();var street=race.ambientRoad?race.ambientRoad:race.road;street.Initialize();
 float Ground(Vector3 p)=>Physics.RaycastAll(new Vector3(p.x,400,p.z),Vector3.down,800,1,QueryTriggerInteraction.Ignore).Where(h=>h.collider.name.StartsWith("Ground_")).OrderBy(h=>Math.Abs(h.point.y-p.y)).First().point.y;
 var sign=GameObject.Find("House 3 / Rocky Way Acres entrance").transform;var mouth=street.At(street.Project(new Vector3(515.1f,81.8f,-132.7f),out _),out _);var toward=Vector3.ProjectOnPlane(GameObject.Find("Original house 3").transform.position-mouth,Vector3.up).normalized;var side=Vector3.Cross(Vector3.up,toward);var at=mouth+toward*23+side*15;at.y=Ground(at);sign.SetPositionAndRotation(at,Quaternion.LookRotation(toward));
 var posts=sign.Cast<Transform>().Where(t=>t.name=="Tall grounded entrance post").ToArray();at.y=posts.Max(p=>Ground(new Vector3(p.position.x,at.y,p.position.z)));sign.position=at;
 foreach(var post in posts){var p=post.position;float bottom=Ground(new Vector3(p.x,at.y,p.z))-.06f,top=at.y+6.8f;p.y=(bottom+top)/2;post.position=p;post.localScale=new(.4f,top-bottom,.4f);
 var basePoint=p-Vector3.up*post.localScale.y*.5f;if(Math.Abs(Ground(basePoint)-basePoint.y-.06f)>.025f)throw new Exception("Post gap");
 if(name=="StreetLoopReverse"){var laurel=Object.FindObjectsByType<WoodlandRoute>().Single(b=>b.title=="Laurel Switchbacks");laurel.Project(basePoint,out float d);if(d<laurel.halfWidth+2)throw new Exception("Laurel edge clearance "+d);rows.Add("PASS Laurel post distance="+d+"m; corridor half-width="+laurel.halfWidth+"m; additional margin >2m");}
 }
 Physics.SyncTransforms();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);rows.Add(name+" final entrance sign="+sign.position+"; posts independently grounded");
 }AssetDatabase.SaveAssets();File.WriteAllLines("Docs/PropertyCorrections/sign-clearance.txt",rows);return string.Join("\n",rows);
 }
}
