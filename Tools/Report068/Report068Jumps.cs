using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using Racer;using Object=UnityEngine.Object;
// 0.68 read-only jump inventory (Parts C/E). For every Mountain route: pavement gaps of >= 2 m are jumps (lip = last
// paved station, landing = first paved station after the gap). After each landing, measure the route's heading change
// over 0..100 m, which side is the outside of that turn, and where each pavement edge drops off (ground > 2.5 m below
// the edge 2 m outside it). Also writes the main-route profile around both Forward/Reverse summit flights.
public static class Report068Jumps {
 static bool IsDrive(Collider c)=>c&&c.name.Contains("driving surface");
 static bool Pave(Vector3 q,float near,out float y){y=0;var o=new Vector3(q.x,near+2.5f,q.z);float depth=5;
  for(int k=0;k<12&&depth>0;k++){if(!Physics.Raycast(o,Vector3.down,out var h,depth,~0,QueryTriggerInteraction.Ignore))return false;if(IsDrive(h.collider)&&h.normal.y>0){y=h.point.y;return true;}depth-=h.distance+.02f;o=h.point+Vector3.down*.02f;}return false;}
 static float Ground(Vector3 q,float from){var o=new Vector3(q.x,from,q.z);return Physics.Raycast(o,Vector3.down,out var h,300,~0,QueryTriggerInteraction.Ignore)?h.point.y:float.NegativeInfinity;}
 static float Heading(Vector3 f)=>Mathf.Atan2(f.x,f.z)*Mathf.Rad2Deg;
 public static void Run(){var outDir=Environment.GetEnvironmentVariable("PROBE_OUT");Directory.CreateDirectory(outDir);
  foreach(var scene in new[]{"MountainLoop","MountainLoopReverse"}){EditorSceneManager.OpenScene("Assets/Scenes/"+scene+".unity");Physics.SyncTransforms();
   var race=Object.FindAnyObjectByType<RaceDirector>(FindObjectsInactive.Include);var road=race.road;road.Initialize();
   var routes=new List<(string n,Func<float,Vector3> at,Func<float,Vector3> fw,float len,Func<float,float> hw)>{("Main",x=>road.At(x,out _),x=>{road.At(x,out var f);return f;},road.Length,x=>road.HalfWidth(x))};
   foreach(var b in Object.FindObjectsByType<WoodlandRoute>().Where(b=>b.gameObject.activeInHierarchy)){b.Initialize();var bb=b;routes.Add((b.title,x=>bb.At(x,out _),x=>{bb.At(x,out var f);return f;},b.Length,x=>bb.halfWidth));}
   var rows=new List<string>{$"{scene} jumps (pavement gaps >= 2 m)"};
   foreach(var r in routes){float gap=-1,lastPaved=-1;
    for(float s=0;s<=r.len;s+=1){var c=r.at(s);bool on=Pave(c,c.y,out _);if(!on&&gap<0&&lastPaved>=0)gap=s;
     if(on&&gap>=0){float lip=lastPaved,land=s;if(land-lip>=3){
       var lp=r.at(lip);var ld=r.at(land);var f0=r.fw(land);
       // paved landing deck length
       float deck=0;for(float d=0;d<200&&land+d<=r.len;d+=1){var q=r.at(land+d);if(!Pave(q,q.y,out _))break;deck=d;}
       rows.Add($"JUMP {r.n} lip s={lip:F0} {lp:F1} -> landing s={land:F0} {ld:F1}; gap {land-lip:F0} m, drop {lp.y-ld.y:F1} m; paved deck after landing {deck:F0} m; heading at landing {Heading(f0):F0}");
       for(float d=0;d<=100&&land+d<=r.len;d+=10){var q=r.at(land+d);var f=r.fw(land+d);float turn=Mathf.DeltaAngle(Heading(f0),Heading(f));
        var right=Vector3.Cross(Vector3.up,Vector3.ProjectOnPlane(f,Vector3.up).normalized);float hw=r.hw(land+d);
        string Edge(int side){var e=q+right*side*(hw+2);float py=q.y;Pave(q,q.y,out py);float g=Ground(e,py+1.5f);return float.IsNegativeInfinity(g)?"VOID":$"{py-g:F1}";}
        rows.Add($"   +{d,3:F0} m s={land+d:F0} {q:F1} turn {turn,5:F0} deg; drop 2 m outside edge L={Edge(-1)} R={Edge(1)}");}}gap=-1;}
     if(on)lastPaved=s;}}
   // Authored jump activities (kickers on continuous pavement, e.g. Fern Creek Leap) near any route.
   foreach(var site in Object.FindObjectsByType<ActivitySite>(FindObjectsInactive.Include).Where(a=>a.kind==ActivitySite.Kind.Jump)){var p=site.transform.position;
    foreach(var r in routes){float best=float.MaxValue,bs=0;for(float s=0;s<=r.len;s+=1){var q=r.at(s);float d=new Vector2(q.x-p.x,q.z-p.z).magnitude+Mathf.Abs(q.y-p.y);if(d<best){best=d;bs=s;}}if(best>25)continue;
     rows.Add($"ACTIVITY JUMP '{site.title}' ({(site.gameObject.activeInHierarchy?"active":"inactive")}) at {p:F1} fwd {site.forward:F2} on {r.n} s={bs:F0} (distance {best:F1}); route heading {Heading(r.fw(bs)):F0}");
     var f0=r.fw(bs);for(float d=0;d<=120&&bs+d<=r.len;d+=10){var q=r.at(bs+d);var f=r.fw(bs+d);float turn=Mathf.DeltaAngle(Heading(f0),Heading(f));var right=Vector3.Cross(Vector3.up,Vector3.ProjectOnPlane(f,Vector3.up).normalized);float hw=r.hw(bs+d);
      string Edge(int side){var e=q+right*side*(hw+2);float py=q.y;Pave(q,q.y,out py);float g=Ground(e,py+1.5f);return float.IsNegativeInfinity(g)?"VOID":$"{py-g:F1}";}
      rows.Add($"   +{d,3:F0} m s={bs+d:F0} {q:F1} turn {turn,5:F0} deg; paved={Pave(q,q.y,out _)}; drop 2 m outside edge L={Edge(-1)} R={Edge(1)}");}}}
   // main profile around the summit flights
   var flights=Object.FindAnyObjectByType<MountainFlights>();if(flights)foreach(var f in flights.flights){rows.Add($"PROFILE {f.name} approach {f.approachStation:F0} end {f.endStation:F0}");
    for(float s=f.approachStation;s<=f.endStation+120;s+=5){var q=road.At(s,out var fw);bool on=Pave(q,q.y,out float py);rows.Add($"   s={s:F0} {q:F1} hdg {Heading(fw):F0} hw {road.HalfWidth(s):F1} pave={(on?py.ToString("F1"):"-")}");}}
   File.WriteAllLines($"{outDir}/{scene}-jumps.txt",rows);}
  EditorApplication.Exit(0);}
}
