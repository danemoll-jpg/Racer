#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Racer {
// 0.74 targeted checks (muted, isolated save). PROBE_CASES="case:args;..." per scene:
//  ride:route:profile:dir:pace[:weather]   Free Roam; the vehicle follows the named road/branch (or "pts=x,z/x,z/...")
//                                          forward (dir=fwd) or backward (dir=back) at `pace` m/s (throttle/brake),
//                                          logging time, lowest speed after the start, airborne time, largest pitch/roll,
//                                          wipe-outs and resets; steepest 2 m grade of the surface actually driven.
public sealed partial class Report074Checks:MonoBehaviour {
 RaceDirector race;RaceFlow flow;readonly List<string> rows=new();string output,saveDir;int fails;
 void Note(string s){rows.Add(s);File.WriteAllLines(output+"/results.txt",rows);Debug.Log("REPORT074 "+s);}
 void Check(bool ok,string s){if(!ok)fails++;Note((ok?"PASS ":"FAIL ")+s);}
 IEnumerator Start(){AudioListener.volume=0;var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;output=Environment.GetEnvironmentVariable("PROBE_OUT")+"/checks-"+scene;Directory.CreateDirectory(output);Application.runInBackground=true;
  race=FindAnyObjectByType<RaceDirector>();yield return null;yield return null;flow=race.Flow;
  saveDir=Path.GetFullPath("Temp/Report074Save");if(Directory.Exists(saveDir))Directory.Delete(saveDir,true);flow.UseValidationSave(saveDir);
  var title=FindAnyObjectByType<StartupTitle>();if(title)Destroy(title.gameObject);typeof(StartupTitle).GetField("completed",BindingFlags.Static|BindingFlags.NonPublic).SetValue(null,true);flow.EnterMenuAfterTitle();
  yield return null;
  foreach(var spec in (Environment.GetEnvironmentVariable("PROBE_CASES")??"").Split(';').Where(x=>x.Length>0)){var a=spec.Split(':');
   IEnumerator run=Case(a,spec);
   if(run!=null)yield return run;else Note("unknown case "+spec);}
  Note($"done, failures {fails}");File.WriteAllLines(output+"/done.txt",rows);UnityEditor.EditorApplication.Exit(0);}
 IEnumerator Case(string[] a,string spec){switch(a[0]){
   case "ride":return Ride(a[1],a[2],a[3]=="back",float.Parse(a[4]),a.Length>5?(Weather)Enum.Parse(typeof(Weather),a[5]):Weather.Clear);
   default:return MoreCases(a,spec);}}
 partial void More(string[] a,string spec,ref IEnumerator run);
 IEnumerator MoreCases(string[] a,string spec){IEnumerator run=null;More(a,spec,ref run);return run;}

 IEnumerator ToMenu(){if(flow.State!=RaceFlow.Stage.Ready){flow.Pause();flow.QuitRace();}yield return null;yield return null;}
 IEnumerator Roam(string profile,Weather w){yield return ToMenu();flow.Save.Settings.roamWeather=(int)w;flow.Save.SaveSettings();flow.OpenGarage();flow.SelectVehicle(profile);flow.CloseGarage();race.opponents=false;race.traffic=false;flow.StartFreeRoam();
  float t0=Time.realtimeSinceStartup;while(flow.State!=RaceFlow.Stage.Racing&&Time.realtimeSinceStartup-t0<60){AudioListener.volume=0;yield return null;}Time.timeScale=1;yield return new WaitForSeconds(.3f);}
 ArcadeVehicle Car=>race.vehicle;
 void Put(Vector3 p,Quaternion r,Vector3 v){var car=Car;car.Body.isKinematic=false;car.transform.SetPositionAndRotation(p,r);car.Body.position=p;car.Body.rotation=r;car.Body.linearVelocity=v;car.Body.angularVelocity=Vector3.zero;car.ClearSteering();Physics.SyncTransforms();race.ResetSampling(p,race.Clock);car.GetComponent<VehicleRespawn>().SeedCoursePosition(p);}
 float Steer(Vector3 target){var car=Car;var d=Quaternion.Inverse(Quaternion.LookRotation(Vector3.ProjectOnPlane(car.transform.forward,Vector3.up)))*(target-car.Body.position);return Mathf.Clamp(Mathf.Atan2(d.x,d.z)*1.6f,-1,1);}
 static string Full(Transform t){var n=t.name;while(t.parent){t=t.parent;n=t.name+"/"+n;}return n;}
 static List<Vector3> Route(string name){
  if(name.StartsWith("file=")){return File.ReadAllLines(name.Substring(5)).Where(l=>l.Trim().Length>0).Select(l=>{var q=l.Split(' ').Select(float.Parse).ToArray();return new Vector3(q[0],q[1],q[2]);}).ToList();}
  if(name.StartsWith("pts=")){return name.Substring(4).Split('/').Select(q=>{var a=q.Split(',').Select(float.Parse).ToArray();var v=new Vector3(a[0],300,a[1]);return Physics.Raycast(v,Vector3.down,out var h,600,1,QueryTriggerInteraction.Ignore)?h.point:new Vector3(a[0],0,a[1]);}).ToList();}
  foreach(var r in FindObjectsByType<RaceRoad>(FindObjectsInactive.Include,FindObjectsSortMode.None))if(r.name==name||Full(r.transform).EndsWith(name))return r.points.ToList();
  foreach(var r in FindObjectsByType<WoodlandRoute>(FindObjectsInactive.Include,FindObjectsSortMode.None))if(r.title==name||r.name==name||Full(r.transform).EndsWith(name))return r.points.ToList();return null;}
 static float Len(List<Vector3> p){float s=0;for(int i=0;i+1<p.Count;i++)s+=Vector3.Distance(p[i],p[i+1]);return s;}
 static Vector3 AtMetre(List<Vector3> pts,float m,out Vector3 f){float s=0;for(int i=0;i+1<pts.Count;i++){float l=Vector3.Distance(pts[i],pts[i+1]);if(s+l>=m){f=(pts[i+1]-pts[i]).normalized;return Vector3.Lerp(pts[i],pts[i+1],(m-s)/Mathf.Max(l,1e-4f));}s+=l;}f=(pts[^1]-pts[^2]).normalized;return pts[^1];}

 // ---------- ride a road / branch at a fixed pace ----------
 IEnumerator Ride(string routeName,string profile,bool back,float pace,Weather w){yield return Roam(profile,w);AudioListener.volume=0;
  var pts=Route(routeName);if(pts==null){Check(false,"route not found "+routeName);yield break;}if(back)pts.Reverse();float L=Len(pts);
  var start=AtMetre(pts,1,out var f0);if(Physics.Raycast(start+Vector3.up*30,Vector3.down,out var g,80,1,QueryTriggerInteraction.Ignore))start=g.point;f0.y=0;
  var car=Car;car.enabled=false;Put(start+Vector3.up*.7f,Quaternion.LookRotation(f0.normalized),f0.normalized*pace*.5f);
  int resets=0;var rs=car.GetComponent<VehicleRespawn>();Action onReset=()=>resets++;rs.Respawned+=onReset;var cfg=car.GetComponent<VehicleConfiguration>();
  float t0=Time.time,minSpeed=float.MaxValue,air=0,airMax=0,pitchMax=0,rollMax=0,dist=0,maxSpeed=0,steep=0;int wipes=0;bool wasWiped=false;Vector3 last=car.Body.position;float m=1;string end="time";var hist=new List<Vector3>();
  string tag=$"{routeName.Replace('/','_').Replace(':','_').Substring(0,Math.Min(40,routeName.Length))}-{profile}-{(back?"back":"fwd")}";
  using(var log=new StreamWriter($"{output}/ride-{tag}.csv")){log.WriteLine("t,m,x,y,z,speed,grounded,pitch,roll");
   while(Time.time-t0<Mathf.Max(40,L/pace*2.5f)){yield return new WaitForFixedUpdate();var p=car.Body.position;dist+=Vector3.Distance(new Vector3(p.x,0,p.z),new Vector3(last.x,0,last.z));last=p;
    // progress = nearest point along the line ahead of the last progress (search window 25 m)
    float best=1e9f,bm=m;for(float q=m;q<Mathf.Min(L,m+25);q+=.5f){var c=AtMetre(pts,q,out _);float d=new Vector2(c.x-p.x,c.z-p.z).sqrMagnitude;if(d<best){best=d;bm=q;}}m=bm;
    var tgt=AtMetre(pts,Mathf.Min(m+8,L),out _);float sp=car.ForwardSpeed;
    car.Simulate(sp<pace?1:0,sp>pace+1.5f?.6f:0,Steer(tgt),Time.fixedDeltaTime);
    float elapsed=Time.time-t0;if(elapsed>2.5f&&m<L-8)minSpeed=Mathf.Min(minSpeed,sp);maxSpeed=Mathf.Max(maxSpeed,sp);
    if(car.GroundedWheels<2){air+=Time.fixedDeltaTime;airMax=Mathf.Max(airMax,air);}else air=0;
    var e=car.transform.eulerAngles;float pitch=Mathf.DeltaAngle(0,e.x),roll=Mathf.DeltaAngle(0,e.z);pitchMax=Mathf.Max(pitchMax,Mathf.Abs(pitch));rollMax=Mathf.Max(rollMax,Mathf.Abs(roll));
    bool wiped=cfg&&cfg.WipedOut;if(wiped&&!wasWiped)wipes++;wasWiped=wiped;
    hist.Add(p);log.WriteLine($"{elapsed:F2},{m:F1},{p.x:F2},{p.y:F2},{p.z:F2},{sp:F2},{car.GroundedWheels},{pitch:F1},{roll:F1}");
    if(m>=L-3){end="complete";break;}if(resets>0){end="reset";break;}}}
  rs.Respawned-=onReset;car.enabled=true;
  // steepest 2 m grade of the driven track (body positions are 0.6-0.9 m over the surface; grade from consecutive positions 2 m apart)
  for(int i=0,j=0;i<hist.Count;i++){while(j<hist.Count&&new Vector2(hist[j].x-hist[i].x,hist[j].z-hist[i].z).magnitude<2)j++;if(j>=hist.Count)break;float hz=new Vector2(hist[j].x-hist[i].x,hist[j].z-hist[i].z).magnitude;steep=Mathf.Max(steep,Mathf.Abs(hist[j].y-hist[i].y)/hz);}
  bool ok=end=="complete"&&wipes==0&&airMax<.35f&&(minSpeed>2.5f||minSpeed==float.MaxValue);
  Check(ok,$"RIDE {routeName} {(back?"back":"fwd")} {profile} pace {pace}: {end} in {Time.time-t0:F1} s over {L:F0} m; slowest {(minSpeed==float.MaxValue?0:minSpeed):F1} m/s, fastest {maxSpeed:F1} m/s; longest airborne {airMax:F2} s; largest pitch {pitchMax:F0} deg, roll {rollMax:F0} deg; wipe-outs {wipes}; resets {resets}; steepest driven 2 m grade {steep*100:F0}%");}
}
}
#endif
