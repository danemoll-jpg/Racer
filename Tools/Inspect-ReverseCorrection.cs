using System;using System.IO;using System.Linq;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using Racer;
public static class InspectReverseCorrection {
 public static string Main(){
 if(Application.isPlaying||UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)throw new Exception("Saved edit mode required");
 EditorSceneManager.OpenScene("Assets/Scenes/DansBackyardReverse.unity");Physics.SyncTransforms();
 object P(Vector3 p)=>new{x=p.x,y=p.y,z=p.z};
 var data=new{samples=Enumerable.Range(0,27).SelectMany(i=>Enumerable.Range(0,39).Select(j=>{var p=new Vector3(30+i*5,0,-130+j*5);return new{x=p.x,z=p.z,hits=Physics.RaycastAll(new(p.x,250,p.z),Vector3.down,500,1,QueryTriggerInteraction.Ignore).OrderBy(h=>h.distance).Select(h=>new{h.collider.name,y=h.point.y})};})),roads=UnityEngine.Object.FindObjectsByType<RaceRoad>().Select(r=>new{r.name,points=r.points.Select(P)}),objects=UnityEngine.Object.FindObjectsByType<Collider>().Where(c=>c.bounds.center.x>30&&c.bounds.center.x<180&&c.bounds.center.z>-130&&c.bounds.center.z<80).Select(c=>new{c.name,p=P(c.bounds.center),size=P(c.bounds.size)})};
 Directory.CreateDirectory("Docs/ReverseCorrection");File.WriteAllText("Docs/ReverseCorrection/baseline.json",Newtonsoft.Json.JsonConvert.SerializeObject(data));
 return "Gully and protected surface geometry captured";
 }
}
