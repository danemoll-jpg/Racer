using System;using System.IO;using System.Linq;using System.Collections.Generic;using System.Text.RegularExpressions;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using Racer;using Object=UnityEngine.Object;
public static class ReportReview {
 const string Out="Docs/ReportCleanup";
 public static string Reverse()=>Inspect("MountainLoopReverse");
 public static string Forward()=>Inspect("MountainLoop");
 static string Inspect(string scene){
 if(Application.isPlaying)throw new Exception("Edit mode required");if(UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)throw new Exception("Unsaved scene");
 EditorSceneManager.OpenScene("Assets/Scenes/"+scene+".unity");Directory.CreateDirectory(Out);Physics.SyncTransforms();
 var race=Object.FindAnyObjectByType<RaceDirector>();race.road.Initialize();var branches=Object.FindObjectsByType<WoodlandRoute>();foreach(var b in branches)b.Initialize();
 var rows=new List<string>();var grounds=Object.FindObjectsByType<MeshCollider>().Where(c=>c.name.StartsWith("Ground_")).ToArray();
 foreach(var c in grounds)rows.Add($"GROUND {c.name} asset={AssetDatabase.GetAssetPath(c.sharedMesh)} triangles={c.sharedMesh.triangles.Length/3} bounds={c.bounds}");
 var md=File.ReadAllText("Builds/DebugReportCleanup/Input/Report/BUG_REPORT.md").Replace("\r", "");
 var camera=new GameObject("Temporary report evidence",typeof(Camera)).GetComponent<Camera>();camera.CopyFrom(Camera.main);camera.enabled=false;
 foreach(var section in Regex.Split(md,@"(?=## BUG-)").Where(s=>s.StartsWith("## BUG-")&&s.Contains("Scene: "+scene+"\n"))){
 var id=Regex.Match(section,@"BUG-\d+").Value;var m=Regex.Match(section,@"Position: X=([-\d.]+), Y=([-\d.]+), Z=([-\d.]+)");var p=new Vector3(float.Parse(m.Groups[1].Value,System.Globalization.CultureInfo.InvariantCulture),float.Parse(m.Groups[2].Value,System.Globalization.CultureInfo.InvariantCulture),float.Parse(m.Groups[3].Value,System.Globalization.CultureInfo.InvariantCulture));float yaw=float.Parse(Regex.Match(section,@"Heading: ([-\d.]+)").Groups[1].Value,System.Globalization.CultureInfo.InvariantCulture);
 rows.Add(id+" "+p);var s=race.road.Project(p,out float d);rows.Add($" main s={s} d={d} p={race.road.At(s,out _)}");foreach(var b in branches){var bs=b.Project(p,out var bd);rows.Add($" branch {b.title} s={bs} d={bd} p={b.At(bs,out _)}");}
 for(float x=-8;x<=8;x+=4)for(float z=-8;z<=8;z+=4){var q=p+new Vector3(x,5,z);rows.Add($" hits {x}/{z}: "+string.Join("; ",Physics.RaycastAll(q,Vector3.down,200,1,QueryTriggerInteraction.Ignore).Where(h=>h.collider.name.StartsWith("Ground_")).OrderByDescending(h=>h.point.y).Select(h=>$"{h.collider.name}@{h.point.y:F3}")));}
 rows.Add(" nearby signs/arrows: "+string.Join("; ",Object.FindObjectsByType<Renderer>().Where(r=>(r.name.Contains("arrow")||r.GetComponentInParent<PhysicalSign>())&&Vector3.Distance(r.bounds.center,p)<45).Select(r=>$"{r.name} @{r.bounds.center}")));
 var f=Quaternion.Euler(0,yaw,0)*Vector3.forward;camera.transform.SetPositionAndRotation(p-f*6+Vector3.up*3.5f,Quaternion.LookRotation(f*25-Vector3.up*3));View(camera,Out+"/"+id+"-after.png");
 }
 Object.DestroyImmediate(camera.gameObject);File.WriteAllLines(Out+"/"+scene+"-after-inspection.txt",rows);
 File.WriteAllText(Out+"/"+scene+"-after-routes.json",Newtonsoft.Json.JsonConvert.SerializeObject(new{road=race.road.points.Select(p=>new[]{p.x,p.y,p.z}),branches=branches.Select(b=>new{b.title,b.entryRoad,b.exitRoad,points=b.points.Select(p=>new[]{p.x,p.y,p.z})})}));return scene+" inspected";
 }
 public static void View(Camera camera,string path){var rt=new RenderTexture(1280,720,24);var prev=RenderTexture.active;camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;var tex=new Texture2D(1280,720,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,1280,720),0,0);tex.Apply();File.WriteAllBytes(path,tex.EncodeToPNG());camera.targetTexture=null;RenderTexture.active=prev;rt.Release();Object.DestroyImmediate(rt);Object.DestroyImmediate(tex);}
 public static string Signs(){Directory.CreateDirectory(Out);var rows=new List<string>();foreach(var path in EditorBuildSettings.scenes.Select(s=>s.path)){if(!File.Exists(path))continue;EditorSceneManager.OpenScene(path);rows.Add("SCENE "+path);foreach(var t in Object.FindObjectsByType<TextMesh>()){var sign=t.GetComponentInParent<PhysicalSign>();rows.Add($"{(sign?sign.name:t.name)} | {t.text.Replace('\n','/')} | {t.transform.position}");}}File.WriteAllLines(Out+"/sign-inventory.txt",rows);return "Sign inventory saved";}
}



