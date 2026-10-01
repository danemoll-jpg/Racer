using System;using System.IO;using System.Linq;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using Racer;
public static class InspectReverseShortcuts {
 public static string Main(){
 if(Application.isPlaying||UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)throw new Exception("Saved edit mode required");
 EditorSceneManager.OpenScene("Assets/Scenes/DansBackyardReverse.unity");Physics.SyncTransforms();
 var race=UnityEngine.Object.FindAnyObjectByType<RaceDirector>();race.road.Initialize();
 object P(Vector3 p)=>new{x=p.x,y=p.y,z=p.z};
 var data=new {route=race.road.points.Select(P),gates=race.gates.Select(g=>new{g.name,p=P(g.transform.position),s=race.road.Project(g.transform.position,out _)}),roads=UnityEngine.Object.FindObjectsByType<RaceRoad>().Select(r=>new{r.name,points=r.points.Select(P)}),samples=Enumerable.Range(0,74).SelectMany(i=>Enumerable.Range(0,40).Select(j=>{var p=new Vector3(35+i*5,0,-65+j*5);var hits=Physics.RaycastAll(new Vector3(p.x,250,p.z),Vector3.down,500,1,QueryTriggerInteraction.Ignore).Where(h=>h.collider.name.StartsWith("Ground_")||h.collider.name.Contains("Road")).OrderBy(h=>h.distance);return new{x=p.x,z=p.z,hits=hits.Select(h=>new{h.collider.name,y=h.point.y})};})),objects=UnityEngine.Object.FindObjectsByType<Collider>().Where(c=>c.bounds.center.x>40&&c.bounds.center.x<405&&c.bounds.center.z>-65&&c.bounds.center.z<140).Select(c=>new{c.name,p=P(c.bounds.center),size=P(c.bounds.size)})};
 Directory.CreateDirectory("Docs/ReverseShortcuts");File.WriteAllText("Docs/ReverseShortcuts/baseline.json",Newtonsoft.Json.JsonConvert.SerializeObject(data));return "Saved terrain, roads, gates and dependent colliders inspected";
 }
}
