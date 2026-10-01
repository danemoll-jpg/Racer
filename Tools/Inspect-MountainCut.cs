using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using Racer;using Object=UnityEngine.Object;
public static class InspectMountainCut {
 public const string Dir="Docs/MountainCut";
 public static string Main(){
 if(Application.isPlaying||UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)throw new Exception("Saved edit mode required");
 Directory.CreateDirectory(Dir);EditorSceneManager.OpenScene("Assets/Scenes/MountainLoopReverse.unity");Physics.SyncTransforms();var race=Object.FindAnyObjectByType<RaceDirector>();var road=race.road;road.Initialize();var branch=Object.FindObjectsByType<WoodlandRoute>().Single(b=>b.title=="Summit Traverse");branch.Initialize();
 var lines=new List<string>();
 for(float s=branch.entryRoad-70;s<branch.exitRoad+50;s+=10){var p=road.At(s,out var f);if(p.x<820||p.x>1070||p.z < -160||p.z>180)continue;var hits=Physics.RaycastAll(p+Vector3.up*.8f,Vector3.up,35,1,QueryTriggerInteraction.Ignore);lines.Add($"s={s:F1} p={p:F2} width={road.HalfWidth(s):F2} overhead="+string.Join(";",hits.Select(h=>$"{h.collider.name}@{h.distance:F2}")));}
 lines.Add("branch entry="+branch.entryRoad+" exit="+branch.exitRoad);
 foreach(var sign in Object.FindObjectsByType<PhysicalSign>().Where(s=>s.transform.position.x>820&&s.transform.position.x<1070))lines.Add("SIGN "+sign.name+" p="+sign.transform.position+" text="+string.Join("|",sign.GetComponentsInChildren<TextMesh>().Select(t=>t.text)));
 foreach(var g in race.gates)if(g.transform.position.x>820)lines.Add("GATE "+g.name+" "+g.transform.position+" "+string.Join(";",g.GetComponentsInChildren<Renderer>().Select(r=>r.name+":"+r.bounds)));
 File.WriteAllLines(Dir+"/before.txt",lines);
 File.WriteAllText(Dir+"/routes-before.json",Newtonsoft.Json.JsonConvert.SerializeObject(new {road=road.points.Select(p=>new[]{p.x,p.y,p.z}),branch=branch.points.Select(p=>new[]{p.x,p.y,p.z}),branch.entryRoad,branch.exitRoad,branch.bypassedGates,gates=race.gates.Select(g=>new{name=g.name,p=g.transform.position.ToString("F5")})},Newtonsoft.Json.Formatting.Indented));
 return string.Join("\n",lines);
 }
}
