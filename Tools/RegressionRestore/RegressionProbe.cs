using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using Racer;using Object=UnityEngine.Object;
public static class RegressionProbe {
 static readonly string Out=Environment.GetEnvironmentVariable("PROBE_OUT")??"Temp/probe";
 static readonly System.Globalization.CultureInfo C=System.Globalization.CultureInfo.InvariantCulture;
 public static void Run(){Directory.CreateDirectory(Out);var ms=AssetDatabase.LoadAssetAtPath<MonoScript>("Assets/Scripts/RaceDirector.cs");Debug.Log("PROBE ms="+ms+" class="+(ms?ms.GetClass():null));if(Environment.GetEnvironmentVariable("PROBE_REIMPORT")=="1"){AssetDatabase.ImportAsset("Assets/Scripts",ImportAssetOptions.ImportRecursive|ImportAssetOptions.ForceUpdate);ms=AssetDatabase.LoadAssetAtPath<MonoScript>("Assets/Scripts/RaceDirector.cs");Debug.Log("PROBE after ms="+ms+" class="+(ms?ms.GetClass():null));}
  foreach(var scene in (Environment.GetEnvironmentVariable("PROBE_SCENES")??"MountainLoopReverse").Split(','))Probe(scene);
  EditorApplication.Exit(0);}
 static string Path(Collider c){var m=c as MeshCollider;var t=c.transform;var n=t.name;while(t.parent){t=t.parent;n=t.name+"/"+n;}return n+(m&&m.sharedMesh?" ["+AssetDatabase.GetAssetPath(m.sharedMesh)+":"+m.sharedMesh.name+" v"+m.sharedMesh.vertexCount+"]":"");}
 static void Probe(string scene){
  EditorSceneManager.OpenScene("Assets/Scenes/"+scene+".unity");Physics.SyncTransforms();
  var race=Object.FindAnyObjectByType<RaceDirector>(FindObjectsInactive.Include);var road=race?race.road:Object.FindAnyObjectByType<RaceRoad>(FindObjectsInactive.Include);Debug.Log("PROBE race="+race+" road="+road+" mbs="+Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include).Length+" missing="+Object.FindObjectsByType<Transform>(FindObjectsInactive.Include).Sum(t=>GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject))+" missingGOs="+string.Join(";",Object.FindObjectsByType<Transform>(FindObjectsInactive.Include).Where(t=>GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject)>0).Select(t=>t.name))+" types="+string.Join(",",Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include).Select(m=>m.GetType().Name).Distinct().Take(40)));road.Initialize();
  var rows=new List<string>{"SCENE "+scene+" length="+road.Length};
  var fl=Object.FindAnyObjectByType<MountainFlights>(FindObjectsInactive.Include);if(fl)foreach(var f in fl.flights)rows.Add($"FLIGHT {f.name} approach={f.approachStation} end={f.endStation} start={f.start} lip={f.lip} land={f.landingEnd}");
  var reports=new[]{new Vector3(948.26f,127.27f,-101.06f),new Vector3(983.14f,176.29f,68.85f)};
  foreach(var p in reports){var s=road.Project(p,out var d);rows.Add($"REPORT {p} s={s:F1} lateral={d:F2} roadPt={road.At(s,out _)}");
   rows.Add(" colliders within 60m: ");foreach(var c in Object.FindObjectsByType<Collider>().Where(c=>!c.isTrigger&&c.enabled&&c.bounds.SqrDistance(p)<3600).OrderBy(c=>Path(c)))rows.Add("  "+Path(c)+" bounds="+c.bounds);}
  // clearance along main road
  for(float s=1560;s<=1840;s+=2){var p=road.At(s,out var f);var r=Vector3.Cross(Vector3.up,f).normalized;
   float surf=Surface(p);var center=new Vector3(p.x,surf,p.z)+Vector3.up*2.35f;
   var hits=Physics.OverlapBox(center,new Vector3(2.6f,1.85f,0.9f),Quaternion.LookRotation(f),~0,QueryTriggerInteraction.Ignore).Where(c=>!c.attachedRigidbody).Select(Path).Distinct();
   rows.Add($"CLEAR s={s} p={p.ToString("F2")} surf={surf:F2} block=[{string.Join(" | ",hits)}]");}
  // surface profile around jump
  for(float s=880;s<=1080;s+=1){var p=road.At(s,out var f);var r=Vector3.Cross(Vector3.up,f).normalized;var line=$"PROF s={s} p={p.ToString("F2")}";
   foreach(var o in new[]{-3f,0f,3f}){var q=p+r*o;var hs=Physics.RaycastAll(q+Vector3.up*40,Vector3.down,90,~0,QueryTriggerInteraction.Ignore).Where(h=>!h.collider.attachedRigidbody).OrderByDescending(h=>h.point.y).Take(3);
    line+=$" | o{o}: "+string.Join(" ; ",hs.Select(h=>$"{h.point.y:F2} n{h.normal.y:F3} {h.collider.name}"));}
   rows.Add(line);}
  File.WriteAllLines(Out+"/"+scene+".txt",rows);}
 static float Surface(Vector3 p){var hs=Physics.RaycastAll(p+Vector3.up*1.2f,Vector3.down,8,~0,QueryTriggerInteraction.Ignore).Where(h=>!h.collider.attachedRigidbody).ToArray();if(hs.Length==0)return p.y;return hs.OrderBy(h=>Mathf.Abs(h.point.y-p.y)).First().point.y;}
}
