using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using Racer;using Object=UnityEngine.Object;
public static class InspectRouteAtlas {
 [Serializable] public class Atlas {public Course[] courses;}
 [Serializable] public class Course {public string scene,id;public Route[] routes;public Landmark[] landmarks;public Vector3[] gates;public string[] jumps;}
 [Serializable] public class Route {public string name,kind;public Vector3[] points;public float entry,exit;public bool ai;public Sample[] samples;}
 [Serializable] public class Landmark {public string name;public Vector3 position;}
 [Serializable] public class Sample {public float s;public Vector3 point;public float ground;public string collider,asset;}
 public static string Main(){
 if(Application.isPlaying||UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)throw new Exception("Saved edit mode required");
 EditorApplication.delayCall+=()=>{try{Export();}catch(Exception e){Directory.CreateDirectory("Docs/RouteAtlas");File.WriteAllText("Docs/RouteAtlas/error.txt",e.ToString());}};return "Scheduled read-only route atlas export";}
 static Sample SampleAt(float s,Vector3 p){var hits=Physics.RaycastAll(new Vector3(p.x,450,p.z),Vector3.down,900,~0,QueryTriggerInteraction.Ignore).Where(h=>h.collider.name.StartsWith("Ground")||h.collider.name=="Decorative Road pavement").OrderBy(h=>Math.Abs(h.point.y-p.y)).ToArray();var h=hits.FirstOrDefault();return new Sample{s=s,point=p,ground=hits.Length>0?h.point.y:-999,collider=hits.Length>0?h.collider.name:"MISSING",asset=h.collider is MeshCollider mc?AssetDatabase.GetAssetPath(mc.sharedMesh):""};}
 static void Export(){Directory.CreateDirectory("Docs/RouteAtlas");var list=new List<Course>();var original=UnityEngine.SceneManagement.SceneManager.GetActiveScene().path;
 foreach(var path in Racer.Editor.ReverseReviewRelease.Scenes.Concat(new[]{"Assets/Scenes/MountainLoop.unity","Assets/Scenes/MountainLoopReverse.unity"})){
 var scene=EditorSceneManager.OpenScene(path);var race=Object.FindAnyObjectByType<RaceDirector>();race.road.Initialize();Physics.SyncTransforms();var routes=new List<Route>();
 routes.Add(new Route{name="Main course",kind="main",points=race.road.points,samples=Enumerable.Range(0,(int)(race.road.Length/5)+1).Select(i=>SampleAt(i*5,race.road.At(i*5,out _))).ToArray()});
 foreach(var b in Object.FindObjectsByType<WoodlandRoute>())routes.Add(new Route{name=b.title,kind="shortcut",points=b.points,entry=b.entryRoad,exit=b.exitRoad,ai=b.aiValidated,samples=Enumerable.Range(0,(int)(b.Length/2)+1).Select(i=>SampleAt(i*2,b.At(i*2,out _))).ToArray()});
 foreach(var r in Object.FindObjectsByType<RaceRoad>().Where(r=>r.name.Contains("House 3")))routes.Add(new Route{name=r.name,kind="driveway",points=r.points});
 var marks=Object.FindObjectsByType<Transform>().Where(t=>t.name=="Original house 3"||t.name.Contains("Pool house")||t.name.Contains("Kennel")||t.name=="Original house 1"||t.name.Contains("Trickum")&&t.GetComponent<PhysicalSign>()).Select(t=>new Landmark{name=t.name,position=t.position}).ToArray();
 list.Add(new Course{scene=scene.name,id=race.courseId,routes=routes.ToArray(),landmarks=marks,gates=race.gates.Select(g=>g.transform.position).ToArray(),jumps=Object.FindObjectsByType<MountainFlights>().SelectMany(m=>m.flights.Select(f=>f.name)).Concat(Object.FindObjectsByType<ForestLayout>().SelectMany(f=>f.jumpNames??Array.Empty<string>())).ToArray()});
 }
 File.WriteAllText("Docs/RouteAtlas/routes-before.json",JsonUtility.ToJson(new Atlas{courses=list.ToArray()}));if(!string.IsNullOrEmpty(original))EditorSceneManager.OpenScene(original);
 }
}
