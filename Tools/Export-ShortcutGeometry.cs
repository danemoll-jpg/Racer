using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using Racer;
public static class ExportShortcutGeometry {
 public static string Main(){if(Application.isPlaying)throw new Exception("Saved edit mode required");EditorSceneManager.OpenScene("Assets/Scenes/DansBackyardForward.unity");var race=UnityEngine.Object.FindAnyObjectByType<RaceDirector>();var root=GameObject.Find("Backyard optional forest shortcuts");var branches=UnityEngine.Object.FindObjectsByType<WoodlandRoute>();var checks=new List<string>();
 float Ground(Vector3 p)=>Physics.RaycastAll(new Vector3(p.x,250,p.z),Vector3.down,500,1,QueryTriggerInteraction.Ignore).Where(h=>h.collider.name.StartsWith("Ground_")).OrderBy(h=>h.distance).First().point.y;
 foreach(var t in root.GetComponentsInChildren<Transform>().Where(t=>t.name=="Timber grounded support")){var bottom=t.position-t.forward*t.localScale.z*.5f;checks.Add((Mathf.Abs(bottom.y-Ground(bottom))<.18f?"PASS ":"FAIL ")+"Support base "+bottom);}
 foreach(var b in branches){var mesh=root.GetComponentsInChildren<MeshCollider>().Where(c=>c.name.Contains(b.title=="Tree-Top Trail"?"Tree-top":"Cabin angled")).First();checks.Add((mesh.sharedMesh.isReadable?"PASS ":"FAIL ")+b.title+" readable authored support mesh / existing surface-contact handling");}
 var tree=branches.First(b=>b.title=="Tree-Top Trail");float minimum=999;var ramp=root.GetComponentsInChildren<MeshCollider>().First(c=>c.name=="Takeoff - Canopy offroad launch");var vertices=ramp.sharedMesh.vertices;for(int i=0;i<vertices.Length;i+=2){var p=(vertices[i]+vertices[i+1])*.5f;race.road.Project(p,out var lateral);if(p.y>Ground(p)+.20f)minimum=Mathf.Min(minimum,lateral-Vector3.Distance(vertices[i],vertices[i+1])*.5f-2.65f);}checks.Add((minimum>=0?"PASS ":"FAIL ")+"Raised tree-top launch separation from main drivable corridor: "+minimum.ToString("F2")+"m");
 File.WriteAllLines("Docs/BackyardShortcuts/grounding.txt",checks);
 File.WriteAllText("Docs/BackyardShortcuts/geometry.json",Newtonsoft.Json.JsonConvert.SerializeObject(new{scene=race.gameObject.scene.path,courseId=race.courseId,branches=branches.Select(b=>new{b.title,b.entryRoad,b.exitRoad,b.entryInset,b.halfWidth,b.bypassedGates,b.aiValidated,length=b.Length,mainLength=b.exitRoad-b.entryRoad,points=b.points.Select(p=>new{x=p.x,y=p.y,z=p.z})})}));
 return string.Join("\n",checks.Where(s=>s.StartsWith("FAIL")))+"; "+checks.Count+" local geometry checks exported";
 }
}

