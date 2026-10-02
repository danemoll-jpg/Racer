using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using Racer;using Object=UnityEngine.Object;
public static class RegressionProbe2 {
 static readonly string Out=Environment.GetEnvironmentVariable("PROBE_OUT")??"Temp/probe";
 public static void Run(){Directory.CreateDirectory(Out);foreach(var scene in Environment.GetEnvironmentVariable("PROBE_SCENES").Split(','))Probe(scene);EditorApplication.Exit(0);}
 static bool Ok(RaycastHit h)=>!h.collider.attachedRigidbody;
 static void Probe(string scene){
  EditorSceneManager.OpenScene("Assets/Scenes/"+scene+".unity");Physics.SyncTransforms();Physics.queriesHitBackfaces=true;
  var road=Object.FindAnyObjectByType<RaceDirector>(FindObjectsInactive.Include).road;road.Initialize();var drive=GameObject.Find("Ground_CR133 mountain driving surface").GetComponent<MeshCollider>();
  var rows=new List<string>();
  for(float s=1640;s<=1800;s+=1){var p=road.At(s,out var f);var r=Vector3.Cross(Vector3.up,f).normalized;var line=$"LOW s={s} p={p.ToString("F1")}";
   foreach(var o in new[]{-4f,-2f,0f,2f,4f}){var q=p+r*o;float surf=drive.Raycast(new Ray(q+Vector3.up*1.5f,Vector3.down),out var dh,5)?dh.point.y:float.NaN;var b=new Vector3(q.x,float.IsNaN(surf)?q.y:surf,q.z);
    var up=Physics.RaycastAll(b+Vector3.up*.3f,Vector3.up,6f,~0,QueryTriggerInteraction.Ignore).Where(Ok).OrderBy(h=>h.distance).FirstOrDefault();
    var fw=new[]{1.0f,2.5f}.Select(hh=>Physics.RaycastAll(b+Vector3.up*hh,f,1.0f,~0,QueryTriggerInteraction.Ignore).Where(Ok).OrderBy(h=>h.distance).FirstOrDefault()).ToArray();
    line+=$" | o{o} surf={surf:F2} up={(up.collider?up.distance.ToString("F2")+" "+up.collider.name:"-")} fw={string.Join(",",fw.Select(h=>h.collider?h.collider.name:"-"))}";}
   rows.Add(line);}
  for(float s=930;s<=990;s+=1){var p=road.At(s,out var f);var r=Vector3.Cross(Vector3.up,f).normalized;float c=drive.Raycast(new Ray(p+Vector3.up*3,Vector3.down),out var ch,6)?ch.point.y:float.NaN;var line=$"RAMP s={s} center={c:F2}";
   for(float o=-10;o<=10;o+=1){var q=p+r*o;var hs=Physics.RaycastAll(q+Vector3.up*30,Vector3.down,60,~0,QueryTriggerInteraction.Ignore).Where(Ok).OrderByDescending(h=>h.point.y).ToArray();var top=hs.FirstOrDefault();
    line+=$" | {o}:{(top.collider?(top.point.y-c).ToString("F2")+(top.collider==drive?"D":"T")+top.normal.y.ToString("F2"):"-")}";}
   rows.Add(line);}
  foreach(var w in new[]{new Bounds(new(990,170,80),new(60,60,70)),new Bounds(new(990,125,-100),new(100,60,40))}){rows.Add("WINDOW "+w);foreach(var c in Object.FindObjectsByType<MeshCollider>().Where(c=>c.enabled&&c.bounds.Intersects(w)&&c.sharedMesh))rows.Add("  "+c.name+" | "+AssetDatabase.GetAssetPath(c.sharedMesh)+" | "+c.sharedMesh.name+" | "+c.bounds+" | parent="+(c.transform.parent?c.transform.parent.name:"-")+" | pos="+c.transform.position+" rot="+c.transform.rotation.eulerAngles+" scale="+c.transform.lossyScale);}
  File.WriteAllLines(Out+"/"+scene+"-2.txt",rows);}
}
