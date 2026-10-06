#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Racer {
// 0.81 targeted checks, added to the 0.80 runner (same muted isolated save, same helpers): PROBE_CASES="case:args;...".
public sealed partial class Report080Checks {
 partial void More(string[] a,ref IEnumerator run){
  run=a[0] switch{"edge"=>Edge(a[1]),"overlap"=>Overlap(a[1],a[2]),"wall"=>Wall(a[1],a[2],a[3]),_=>null};}

 // BUG-002: ride straight at the world's edge: wall:Scene:profile:x,z,yaw,speed. Full throttle for 8 s from the ground at
 // (x, z) heading yaw; pass = never below the ground's lowest point - 15 m (no fall reset), ends within 60 m of the start.
 IEnumerator Wall(string scene,string profile,string spec){
  if(scene==RaceFlow.RoamScene)yield return EnterRoam("StreetLoopGreybox",profile);else yield return EnterScene(scene);yield return new WaitForSeconds(1);
  var f=spec.Split(',').Select(F).ToArray();var car=race.vehicle;car.GetComponent<VehicleInput>().enabled=false;car.enabled=false;
  var p=new Vector3(f[0],600,f[1]);Physics.Raycast(p,Vector3.down,out var gh,900,~0,QueryTriggerInteraction.Ignore);var start=gh.point+Vector3.up*.8f;var rot=Quaternion.Euler(0,f[2],0);
  Put(car,start,rot,rot*Vector3.forward*f[3]);int resets=0;System.Action onR=()=>resets++;var resp=car.GetComponent<VehicleRespawn>();resp.Respawned+=onR;
  float minY=start.y,far=0;for(float t=0;t<8;t+=Time.fixedDeltaTime){AudioListener.volume=0;car.Simulate(1,0,0,Time.fixedDeltaTime);yield return new WaitForFixedUpdate();var q=car.Body.position;minY=Mathf.Min(minY,q.y);far=Mathf.Max(far,Vector3.Dot(q-start,rot*Vector3.forward));if(t>2.5f&&t<2.53f)yield return Late(()=>Shot($"wall-{Scene}-{profile}-{f[0]:F0}-{f[1]:F0}"));}
  resp.Respawned-=onR;var end=car.Body.position;
  Check(resets==0&&minY>-15&&far<60,$"{Scene} {profile} at the edge from {V(start)} heading {f[2]} at {f[3]} m/s: furthest {far:F1} m out, lowest y {minY:F1}, resets {resets}, end {V(end)}, up {car.transform.up.y:F2}");
  car.enabled=true;car.GetComponent<VehicleInput>().enabled=true;
  Note("world edge: "+string.Join(" | ",WorldEdge.Report));if(scene!=RaceFlow.RoamScene)yield return Menu();}
 // Every collider touching small spheres at points: overlap:Scene:x,y,z,r|... with the closest point on each.
 IEnumerator Overlap(string scene,string specs){yield return EnterScene(scene);yield return new WaitForSeconds(1);var sb=new StringBuilder();
  foreach(var spec in specs.Split('|')){var f=spec.Split(',');var p=ParseV(string.Join(",",f.Take(3)));float r=F(f[3]);sb.AppendLine($"== {V(p)} r {r}");
   foreach(var c in Physics.OverlapSphere(p,r,~0,QueryTriggerInteraction.Collide)){var q=c is MeshCollider mc&&!mc.convex?c.ClosestPointOnBounds(p):c.ClosestPoint(p);sb.AppendLine($"  {P(c.transform)} [{c.GetType().Name}] trig {c.isTrigger} rb {(c.attachedRigidbody?c.attachedRigidbody.name:"-")} closest {V(q)} bounds {V(c.bounds.min)}..{V(c.bounds.max)}");}
   foreach(var dir in new[]{Vector3.forward,Vector3.back,Vector3.left,Vector3.right,new Vector3(1,0,1).normalized,new Vector3(-1,0,-1).normalized,new Vector3(1,0,-1).normalized,new Vector3(-1,0,1).normalized})
    foreach(float dy in new[]{-.3f,-.15f,0f,.15f,.3f})foreach(var h in Physics.RaycastAll(p+Vector3.up*dy-dir*3,dir,6,~0,QueryTriggerInteraction.Ignore))if(!h.collider.attachedRigidbody)sb.AppendLine($"  ray {V(dir)} dy {dy}: {P(h.collider.transform)} at {V(h.point)} n {V(h.normal)}");}
  File.WriteAllText($"{output}/overlap-{Scene}.txt",sb.ToString());Note($"{Scene}: overlap written");if(scene!=RaceFlow.RoamScene)yield return Menu();}

 // The world's edge: every static ground collider's bounds, and, along rays from the centre every 2 degrees, the last
 // point that still has ground under it (x, z, y, collider). edge:Scene
 IEnumerator Edge(string scene){yield return EnterScene(scene);yield return new WaitForSeconds(1);
  var sb=new StringBuilder();Bounds all=default;bool any=false;
  foreach(var c in FindObjectsByType<Collider>(FindObjectsSortMode.None)){if(c.isTrigger||c.attachedRigidbody||!c.enabled)continue;var b=c.bounds;if(b.size.x<150&&b.size.z<150)continue;
   sb.AppendLine($"big {P(c.transform)} [{c.GetType().Name}] b {V(b.center)} s {V(b.size)} min {V(b.min)} max {V(b.max)}");if(!any){all=b;any=true;}else all.Encapsulate(b);}
  sb.AppendLine($"union min {V(all.min)} max {V(all.max)}");
  var c0=new Vector3(all.center.x,0,all.center.z);
  sb.AppendLine("angle,x,z,y,collider");
  for(int deg=0;deg<360;deg+=2){var d=Quaternion.Euler(0,deg,0)*Vector3.forward;Vector3 last=default;string name="-";
   for(float r=0;r<3000;r+=4){var p=c0+d*r;if(Physics.Raycast(new Vector3(p.x,900,p.z),Vector3.down,out var h,1400,~0,QueryTriggerInteraction.Ignore)&&!h.collider.attachedRigidbody){last=h.point;name=h.collider.name;}else if(r>200&&last!=default)break;}
   sb.AppendLine($"{deg},{last.x:F0},{last.z:F0},{last.y:F1},{name}");}
  File.WriteAllText($"{output}/edge-{Scene}.txt",sb.ToString());Note($"{Scene}: edge written, union {V(all.min)} .. {V(all.max)}");
  if(scene!=RaceFlow.RoamScene)yield return Menu();}
}
}
#endif
