using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Object=UnityEngine.Object;
namespace Racer.Editor
{
 public static partial class DiscoveryAuthoring
 {
  const string CR122Dir="Docs/CR122-128";
  public static void CR122Inspect(bool reverse)
  {
   if(UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)throw new Exception("Save before inspection");
   Directory.CreateDirectory(CR122Dir);EditorSceneManager.OpenScene("Assets/Scenes/MountainLoop"+(reverse?"Reverse":"")+".unity");owner=Object.FindAnyObjectByType<RaceDirector>();var road=owner.road;road.Initialize();var rows=new List<string>{owner.courseId};
   string tag=reverse?"reverse":"forward";
   foreach(var b in Object.FindObjectsByType<WoodlandRoute>()){
    rows.Add($"BRANCH {b.title} entry={b.entryRoad} exit={b.exitRoad} length={b.Length} first={b.points[0]} last={b.points[^1]}");
    for(float s=0;s<=b.Length;s+=5){var p=b.At(s,out var f);rows.Add($"BRANCHPOINT {b.title} {s} {p} {f}");if(s<65)CR122Hits(rows,b.title+" "+s,p);}
    foreach(float s in new[]{5f,22f,b.Length-15}){var p=b.At(s,out var f);CR122Shot(tag+"-"+b.title.Replace(" ","-")+"-"+s.ToString("F0"),p-f*7+Vector3.up*3,p+f*28+Vector3.up);}
   }
   foreach(var g in owner.gates)rows.Add($"GATE {g.name} {road.Project(g.transform.position,out _)} {g.transform.position}");
   foreach(var f in owner.GetComponent<MountainFlights>().flights)rows.Add($"FLIGHT {f.name} start={f.start} lip={f.lip} landingEnd={f.landingEnd} heading={f.forward} stations={f.approachStation},{f.endStation}");
   foreach(var s in Object.FindObjectsByType<PhysicalSign>())rows.Add($"SIGN {s.name} {s.transform.position} forward={s.transform.forward}");
   for(float s=0;s<road.Length;s+=5){var p=road.At(s,out var f);rows.Add($"ROAD {s} {p} {f}");if((reverse&&s>2850)||(!reverse&&s<400))CR122Hits(rows,"road "+s,p);}
   if(reverse)foreach(float s in new[]{2920f,2960f,3000f,3040f}){var p=road.At(s,out var f);CR122Shot(tag+"-main-"+s,p-f*7+Vector3.up*3,p+f*30+Vector3.up);}
   else foreach(var f in owner.GetComponent<MountainFlights>().flights){var p=road.At(f.endStation+25,out var d);CR122Shot(tag+"-after-"+f.name.Replace(" ","-"),p-d*7+Vector3.up*3,p+d*35+Vector3.up);}
   File.WriteAllLines(CR122Dir+"/"+tag+"-trace.txt",rows);
  }
  static void CR122Hits(List<string> rows,string label,Vector3 p){foreach(var h in Physics.RaycastAll(new(p.x,400,p.z),Vector3.down,800,~0,QueryTriggerInteraction.Ignore).OrderByDescending(h=>h.point.y).Take(6))rows.Add($"HIT {label} at={p} owner={h.collider.name} parent={h.collider.transform.parent?.name} y={h.point.y} normal={h.normal} mesh={(h.collider as MeshCollider)?.sharedMesh?.name}");}
  static void CR122Shot(string name,Vector3 p,Vector3 target)
  {
   var go=new GameObject("Temporary screenshot reference camera");var c=go.AddComponent<Camera>();c.transform.position=p;c.transform.LookAt(target);c.fieldOfView=65;c.farClipPlane=1800;var rt=new RenderTexture(1280,720,24);c.targetTexture=rt;c.Render();var previous=RenderTexture.active;RenderTexture.active=rt;var tex=new Texture2D(1280,720,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,1280,720),0,0);tex.Apply();File.WriteAllBytes(CR122Dir+"/"+name+".png",tex.EncodeToPNG());RenderTexture.active=previous;Object.DestroyImmediate(tex);c.targetTexture=null;Object.DestroyImmediate(rt);Object.DestroyImmediate(go);
  }
 }
}
