using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor.SceneManagement;
using Racer;
public static class InspectBackyardShortcuts {
 public static string Main(){
 if(Application.isPlaying||UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)throw new Exception("Saved edit mode required");
 EditorSceneManager.OpenScene("Assets/Scenes/DansBackyardForward.unity");
 var race=UnityEngine.Object.FindAnyObjectByType<RaceDirector>();race.road.Initialize();
 float Ground(Vector3 p)=>Physics.RaycastAll(new Vector3(p.x,250,p.z),Vector3.down,500,1,QueryTriggerInteraction.Ignore).Where(h=>h.collider.name.StartsWith("Ground_")).OrderBy(h=>h.distance).First().point.y;
 var data=new { route=race.road.points.Select(p=>new{x=p.x,y=p.y,z=p.z}), gates=race.gates.Select(g=>new{g.name,s=race.road.Project(g.transform.position,out _)}),samples=Enumerable.Range(0,29).SelectMany(i=>Enumerable.Range(0,49).Select(j=>{var p=new Vector3(165+i*5,0,-105+j*5);return new{x=p.x,z=p.z,y=Ground(p)};})), trees=UnityEngine.Object.FindObjectsByType<Collider>(FindObjectsSortMode.None).Where(c=>(c.name.Contains("tree")||c.name.Contains("trunk"))&&c.bounds.center.x>165&&c.bounds.center.x<320&&c.bounds.center.z>-115&&c.bounds.center.z<115).Select(c=>new{c.name,x=c.bounds.center.x,y=c.bounds.center.y,z=c.bounds.center.z,height=c.bounds.size.y})};
 Directory.CreateDirectory("Docs/BackyardShortcuts");File.WriteAllText("Docs/BackyardShortcuts/baseline.json",Newtonsoft.Json.JsonConvert.SerializeObject(data));
 return "Saved scene inspected; route, gates, terrain grid and individual trees exported";
 }
}
