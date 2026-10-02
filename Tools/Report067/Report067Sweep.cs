using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using Racer;using Object=UnityEngine.Object;
// 0.67 Part C support sweep (read-only). Terrain is intentionally cut away beneath the pavement, so a road
// reads as "floating" when its EDGES are unsupported. Every 2 m along the main route and every branch, on
// both sides:
//  down : ray straight down 0.8 m outside the pavement edge; the first surface must face up (not a
//         back face) and lie no more than 1.2 m below the pavement;
//  side : otherwise, a ray from 3 m outside the edge, 0.6 m below pavement, toward the road must meet a
//         front-facing support wall/bank before passing under the pavement edge.
// An edge failing both is UNSUPPORTED. Labels (not exclusions): FLIGHT spans, and edges above another
// authored route (OVER ...). Stations are listed with coordinates for the Part C record.
public static class Report067Sweep {
 static bool Ok(Collider c)=>c&&!c.isTrigger&&!c.attachedRigidbody&&c is MeshCollider;
 public static void Run(){var outDir=Environment.GetEnvironmentVariable("PROBE_OUT");var tag=Environment.GetEnvironmentVariable("PROBE_TAG")??"sweep";
  foreach(var scene in Environment.GetEnvironmentVariable("PROBE_SCENES").Split(',')){
   EditorSceneManager.OpenScene("Assets/Scenes/"+scene+".unity");Physics.SyncTransforms();Physics.queriesHitBackfaces=true;var rows=new List<string>();
   var race=Object.FindAnyObjectByType<RaceDirector>(FindObjectsInactive.Include);var road=race.road;road.Initialize();
   var drive=Object.FindObjectsByType<MeshCollider>().Where(c=>c.name.Contains("driving surface")||c.name.Contains("pavement")).ToArray();
   var flights=Object.FindAnyObjectByType<MountainFlights>(FindObjectsInactive.Include);
   var branches=Object.FindObjectsByType<WoodlandRoute>(FindObjectsInactive.Include).Where(b=>b.gameObject.activeInHierarchy).ToArray();foreach(var b in branches)b.Initialize();
   var routes=new List<(string n,Func<float,Vector3> at,Func<float,Vector3> fw,float len,Func<float,float> hw)>{("Main",s=>road.At(s,out _),s=>{road.At(s,out var f);return f;},road.Length,s=>road.HalfWidth(s))};
   foreach(var b in branches){var bb=b;routes.Add((b.title,s=>bb.At(s,out _),s=>{bb.At(s,out var f);return f;},b.Length,s=>bb.halfWidth));}
   bool InFlight(Vector3 p){if(!flights)return false;foreach(var f in flights.flights){var ax=Vector3.ProjectOnPlane(f.forward,Vector3.up).normalized;float a=Vector3.Dot(p-f.lip,ax),len=Vector3.Dot(f.landingEnd-f.lip,ax);float lat=Vector3.Dot(p-f.lip,Vector3.Cross(Vector3.up,ax));if(a>-2&&a<len&&Mathf.Abs(lat)<30)return true;}return false;}
   // Another authored route (or another level of the main route) close below this edge.
   string Under(Vector3 p){for(int i=0;i<road.points.Length-1;i++){var a=road.points[i];var v=road.points[i+1]-a;float t=Mathf.Clamp01(Vector3.Dot(p-a,v)/v.sqrMagnitude);var q=a+v*t;if(Vector2.Distance(new(q.x,q.z),new(p.x,p.z))<road.HalfWidth(0)+6&&q.y<p.y-2.5f&&q.y>p.y-40)return "Main";}
    foreach(var b in branches){float bs=b.Project(p,out float bd);var q=b.At(bs,out _);if(Vector2.Distance(new(q.x,q.z),new(p.x,p.z))<b.halfWidth+6&&q.y<p.y-2.5f&&q.y>p.y-40)return b.title;}return null;}
   bool Pavement(Vector3 q,float near,out float y){y=0;foreach(var h in Physics.RaycastAll(q+Vector3.up*3,Vector3.down,8,~0,QueryTriggerInteraction.Ignore))if(drive.Contains(h.collider)&&Mathf.Abs(h.point.y-near)<2.5f){y=h.point.y;return true;}return false;}
   int total=0;
   foreach(var r in routes){int flagged=0;
    for(float s=0;s<r.len;s+=2){var c=r.at(s);var f=Vector3.ProjectOnPlane(r.fw(s),Vector3.up).normalized;var right=Vector3.Cross(Vector3.up,f);float hw=r.hw(s);var parts=new List<string>();
     foreach(var side in new[]{-1,1}){var edge=c+right*side*(hw-.3f);if(!Pavement(edge,c.y,out float py))continue;
      // Pavement may be wider than the nominal half width; walk out to the real edge.
      float w=hw-.3f;for(;w<hw+6;w+=.25f){if(!Pavement(c+right*side*(w+.25f),py,out float ny)||Mathf.Abs(ny-py)>.6f)break;py=ny;}
      var outside=c+right*side*(w+.8f);bool down=false;string detail="";
      if(Physics.Raycast(new Vector3(outside.x,py+2,outside.z),Vector3.down,out var dh,80,~0,QueryTriggerInteraction.Ignore)&&Ok(dh.collider)){float drop=py-dh.point.y;down=dh.normal.y>0&&drop<1.2f;detail=$"drop={drop:F1}{(dh.normal.y<=0?" BACKFACE":"")} {dh.collider.name}";}else detail="drop=NONE";
      bool wall=false;if(!down){var from=c+right*side*(w+3f);from.y=py-.6f;var dir=-right*side;if(Physics.Raycast(from,dir,out var wh,3.4f,~0,QueryTriggerInteraction.Ignore)&&Ok(wh.collider)){wall=Vector3.Dot(wh.normal,-dir)>0;detail+=$" wall={(wall?"front":"BACK")} {wh.collider.name}";}else detail+=" wall=NONE";}
      if(!down&&!wall){var at=c+right*side*(w+.8f);string label=InFlight(at)?" FLIGHT":"";var u=Under(new Vector3(at.x,py,at.z));if(u!=null)label+=" OVER "+u;parts.Add($"{(side<0?"L":"R")} w={w:F1} {detail}{label}");}}
     if(parts.Count>0){flagged++;rows.Add($"{r.n} s={s} p={c.ToString("F1")} | "+string.Join(" | ",parts));}}
    total+=flagged;rows.Add($"SUMMARY {r.n} len={r.len:F0} flaggedStations={flagged}");}
   rows.Add($"TOTAL {scene} flaggedStations={total}");File.WriteAllLines(outDir+"/"+tag+"-"+scene+".txt",rows);}
  EditorApplication.Exit(0);}
}
