using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using Racer;using Object=UnityEngine.Object;
// 0.66 regression restoration. Edits only two 0.64 MountainLoopReverse terrain meshes in place:
// BUG-001 removes the two continuous-solid sheets standing across the lower main route (0.63 corridor was open).
// BUG-002 lowers continuous-solid vertices under the South Face ramp/lip to restore 0.63's clear underside.
// Road, ramp, landing, navigation, checkpoints and every other object are untouched.
public static class RestoreReverseRegressions {
 const float RampClearance=4f;
 public static void Run(){
  EditorSceneManager.OpenScene("Assets/Scenes/MountainLoopReverse.unity");Physics.SyncTransforms();
  var road=Object.FindAnyObjectByType<RaceDirector>(FindObjectsInactive.Include).road;road.Initialize();
  var drive=GameObject.Find("Ground_CR133 mountain driving surface").GetComponent<MeshCollider>();var notes=new List<string>();
  // BUG-001: clear the lower main-route corridor through both sheets.
  var sheet=GameObject.Find("Ground_ReportCleanup MountainLoopReverse continuous solid 2").GetComponent<MeshFilter>();
  var m=sheet.sharedMesh;var v=m.vertices;var c=m.colors;var t=m.triangles;var keep=new List<int>();var addV=new List<Vector3>(v);var addC=new List<Color>(c);int removed=0,clipped=0;
  var windows=new[]{new Vector2(1709,1719),new Vector2(1760,1772)};
  foreach(var w in windows)notes.Add($"BUG-001 window s={w.x}-{w.y} from {road.At(w.x,out _)} to {road.At(w.y,out _)}");
  for(int i=0;i<t.Length;i+=3){var a=v[t[i]];var b=v[t[i+1]];var d=v[t[i+2]];var n=Vector3.Cross(b-a,d-a).normalized;var centre=(a+b+d)/3;bool inside=false;
   foreach(var w in windows){float s=road.Project(centre,out float lat);if(s<w.x||s>w.y)continue;var p=road.At(s,out var f);var fh=Vector3.ProjectOnPlane(f,Vector3.up).normalized;
    float rel=centre.y-Surface(drive,centre,p.y);if(lat<14&&rel>.15f&&rel<6f&&Mathf.Abs(Vector3.Dot(n,fh))>.5f)inside=true;}
   if(inside){removed++;continue;}keep.AddRange(new[]{t[i],t[i+1],t[i+2]});}
  Store(m,addV,addC,keep);notes.Add($"BUG-001 sheet triangles removed from lower-route clearance: {removed}; remaining={keep.Count/3}");
  // BUG-002: restore clear space beneath the South Face ramp and lip.
  var branches=Object.FindObjectsByType<WoodlandRoute>(FindObjectsInactive.Include);foreach(var b in branches)b.Initialize();
  var hill=GameObject.Find("Ground_ReportCleanup MountainLoopReverse continuous solid 0").GetComponent<MeshFilter>();var hm=hill.sharedMesh;var hv=hm.vertices;int lowered=0;float deepest=0;
  for(int i=0;i<hv.Length;i++){var w=hill.transform.TransformPoint(hv[i]);float s=road.Project(w,out float lat);if(s<966||s>986)continue;if(branches.Any(b=>{b.Project(w,out float bl);return bl<12;}))continue;var p=road.At(s,out var f);if(Mathf.Abs(p.x-990)>.5f)continue;
   float pave=Pavement(drive,w,road,s);float weight=Mathf.Clamp01((19-lat)/6f)*Mathf.Clamp01((s-966)/6f)*Mathf.Clamp01((986-s)/1.5f+.0001f);if(weight<=0)continue;
   float target=pave-RampClearance;if(w.y<=target)continue;float y=Mathf.Lerp(w.y,target,weight);if(y<w.y-.001f){deepest=Mathf.Max(deepest,w.y-y);w.y=y;hv[i]=hill.transform.InverseTransformPoint(w);lowered++;}}
  hm.vertices=hv;hm.RecalculateNormals();hm.RecalculateBounds();EditorUtility.SetDirty(hm);hill.GetComponent<MeshCollider>().sharedMesh=null;hill.GetComponent<MeshCollider>().sharedMesh=hm;
  notes.Add($"BUG-002 solid-0 vertices lowered beneath ramp/lip: {lowered}; max drop={deepest:F2}m; clearance target={RampClearance}m below pavement");
  sheet.GetComponent<MeshCollider>().sharedMesh=null;sheet.GetComponent<MeshCollider>().sharedMesh=m;
  AssetDatabase.SaveAssets();File.WriteAllLines(Environment.GetEnvironmentVariable("PROBE_OUT")+"/restore-notes.txt",notes);EditorApplication.Exit(0);}
 static float Surface(MeshCollider drive,Vector3 at,float fallback)=>drive.Raycast(new Ray(new Vector3(at.x,fallback+3,at.z),Vector3.down),out var h,8)?h.point.y:fallback;
 // Ramp pavement height above a terrain point; beyond the lip/edges use the nearest centre-line pavement height.
 static float Pavement(MeshCollider drive,Vector3 at,RaceRoad road,float s){var p=road.At(Mathf.Min(s,982.5f),out _);if(drive.Raycast(new Ray(new Vector3(at.x,p.y+8,at.z),Vector3.down),out var h,16)&&Mathf.Abs(h.point.y-p.y)<3)return h.point.y;return p.y;}
 static void Store(Mesh m,List<Vector3> v,List<Color> c,List<int> t){m.indexFormat=UnityEngine.Rendering.IndexFormat.UInt32;m.SetTriangles(t,0);m.RecalculateNormals();m.RecalculateBounds();EditorUtility.SetDirty(m);}
}
