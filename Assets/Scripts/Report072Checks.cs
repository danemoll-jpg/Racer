#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Racer {
// 0.72 targeted checks (muted). PROBE_CASES="case:args;..." per scene:
//  path:profile:speed:x,z|x,z|...   Free Roam: start on the first point facing the second, drive the waypoints at about
//                                   `speed` m/s (slower in tight turns). Logs completion, stops, resets, wipe-outs, minimum
//                                   up-vector and every contact impulse over 1500 N s.
//  airace:laps:timescale            Race with the normal AI field (the player waits off the course, out of the way). Logs,
//                                   per AI and per authored flight: take-off speed, landing place and ground, what happens in
//                                   the 8 s after touchdown; every AI recovery (from / to position and station, distance,
//                                   diagnostic); the off-route detector (near / trackingRejoin / rejoinStuck) around flights;
//                                   and whether each rival would be drawn on the minimap (not DNF). Final classification.
public sealed class Report072Checks:MonoBehaviour {
 RaceDirector race;ArcadeVehicle car;VehicleRespawn respawn;readonly List<string> rows=new();string output;int resets;
 void Note(string s){rows.Add(s);File.WriteAllLines(output+"/results.txt",rows);Debug.Log("REPORT072 "+s);}
 IEnumerator Start(){AudioListener.volume=0;var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;output=Environment.GetEnvironmentVariable("PROBE_OUT")+"/checks-"+scene;Directory.CreateDirectory(output);Application.runInBackground=true;
  race=FindAnyObjectByType<RaceDirector>();yield return null;yield return null;
  race.Flow.UseValidationSave(Path.GetFullPath("Temp/Report072Save"));
  var title=FindAnyObjectByType<StartupTitle>();if(title)Destroy(title.gameObject);typeof(StartupTitle).GetField("completed",BindingFlags.Static|BindingFlags.NonPublic).SetValue(null,true);race.Flow.EnterMenuAfterTitle();
  car=race.vehicle;respawn=car.GetComponent<VehicleRespawn>();respawn.Respawned+=()=>resets++;
  foreach(var spec in (Environment.GetEnvironmentVariable("PROBE_CASES")??"").Split(';').Where(x=>x.Length>0)){var a=spec.Split(':');
   IEnumerator run=a[0] switch{"path"=>PathRide(a[1],float.Parse(a[2]),a[3]),"airace"=>AiRace(int.Parse(a[1]),float.Parse(a[2])),_=>null};
   if(run!=null)yield return run;else Note("unknown case "+spec);}
  File.WriteAllLines(output+"/done.txt",rows);UnityEditor.EditorApplication.Exit(0);}
 IEnumerator Begin(string profile,bool roam,bool opponents){var flow=race.Flow;if(flow.State!=RaceFlow.Stage.Ready){flow.Pause();flow.QuitRace();}yield return null;
  flow.OpenGarage();flow.SelectVehicle(profile);flow.CloseGarage();race.opponents=opponents;if(!opponents)race.traffic=false;
  if(roam)flow.StartFreeRoam();else flow.StartRace();
  float t0=Time.realtimeSinceStartup;while(flow.State!=RaceFlow.Stage.Racing&&Time.realtimeSinceStartup-t0<60){AudioListener.volume=0;yield return null;}Time.timeScale=1;yield return new WaitForSeconds(.5f);}
 string Profile=>car.GetComponent<VehicleConfiguration>().profileId;
 bool Wiped=>car.GetComponent<VehicleConfiguration>().WipedOut;
 void Put(Vector3 p,Quaternion r,Vector3 v){car.Body.isKinematic=false;car.transform.SetPositionAndRotation(p,r);car.Body.position=p;car.Body.rotation=r;car.Body.linearVelocity=v;car.Body.angularVelocity=Vector3.zero;car.ClearSteering();Physics.SyncTransforms();race.ResetSampling(p,race.Clock);respawn.SeedCoursePosition(p);}
 float Steer(Vector3 target){var d=Quaternion.Inverse(Quaternion.LookRotation(Vector3.ProjectOnPlane(car.transform.forward,Vector3.up)))*(target-car.Body.position);return Mathf.Clamp(Mathf.Atan2(d.x,d.z)*1.6f,-1,1);}
 sealed class AllContacts:MonoBehaviour{public readonly List<(float imp,string name,Vector3 p,Vector3 n,float t)> hits=new();
  void Record(Collision c){if(c.impulse.magnitude<1)return;var cp=c.contactCount>0?c.GetContact(0):default;hits.Add((c.impulse.magnitude,c.collider.name,cp.point,cp.normal,Time.time));}
  void OnCollisionEnter(Collision c)=>Record(c);void OnCollisionStay(Collision c)=>Record(c);}

 // ---------- Part A: ride a waypoint path in Free Roam ----------
 IEnumerator PathRide(string profile,float speed,string pts){yield return Begin(profile,true,false);AudioListener.volume=0;
  var w=pts.Split('|').Select(t=>{var q=t.Split(',').Select(float.Parse).ToArray();return new Vector3(q[0],0,q[1]);}).ToList();
  var p0=w[0];if(Physics.Raycast(new Vector3(p0.x,600,p0.z),Vector3.down,out var gh,1200,~0,QueryTriggerInteraction.Ignore))p0=gh.point;
  var f0=w[1]-w[0];f0.y=0;car.enabled=false;Put(p0+Vector3.up*.7f,Quaternion.LookRotation(f0.normalized),f0.normalized*Mathf.Min(speed,6));
  var con=car.gameObject.AddComponent<AllContacts>();int r0=resets,next=1;float t0=Time.time,stuck=0,maxStuck=0,minUp=1,maxImp=0;string end="time",maxImpName="";
  using(var log=new StreamWriter($"{output}/path-{profile}-{rows.Count}.csv")){log.WriteLine("t,x,y,z,speed,wheels,upY,next");
   while(Time.time-t0<120){yield return new WaitForFixedUpdate();var p=car.Body.position;var flat=new Vector3(p.x,0,p.z);
    while(next<w.Count-1&&(w[next]-flat).magnitude<5)next++;
    if(next==w.Count-1&&(w[next]-flat).magnitude<4){end="complete";break;}
    // slow for the turn at the next waypoint
    float turn=next<w.Count-1?Vector3.Angle(w[next]-flat,w[next+1]-w[next]):0;float want=Mathf.Lerp(speed,4.5f,Mathf.Clamp01((turn-25)/90f));
    var tgt=w[next];tgt.y=p.y;float sp=car.ForwardSpeed;car.Simulate(sp<want?1:0,sp>want+1.5f?.6f:0,Steer(tgt),Time.fixedDeltaTime);
    minUp=Mathf.Min(minUp,car.transform.up.y);if(sp<1.2f){stuck+=Time.fixedDeltaTime;maxStuck=Mathf.Max(maxStuck,stuck);}else stuck=0;
    log.WriteLine($"{Time.time-t0:F2},{p.x:F2},{p.y:F2},{p.z:F2},{sp:F1},{car.GroundedWheels},{car.transform.up.y:F2},{next}");
    if(maxStuck>5){end="blocked";break;}if(resets>r0){end="reset";break;}if(Wiped){end="wiped out";break;}}}
  car.enabled=true;foreach(var h in con.hits)if(h.imp>maxImp){maxImp=h.imp;maxImpName=$"{h.name} at {h.p:F1} n{h.n.y:F2}";}Destroy(con);
  Note($"path {profile} {w[0].x:F0},{w[0].z:F0} -> {w[w.Count-1].x:F0},{w[w.Count-1].z:F0} ({w.Count} points): {end}; reached point {next}/{w.Count-1}; longest stop {maxStuck:F1} s; min up {minUp:F2}; resets {resets-r0}; largest contact impulse {maxImp:F0} ({maxImpName}); time {Time.time-t0:F1} s");}

 // ---------- Part B: AI races ----------
 static readonly FieldInfo fTracking=typeof(RoadDriver).GetField("trackingRejoin",BindingFlags.NonPublic|BindingFlags.Instance),fRejoin=typeof(RoadDriver).GetField("rejoinStuck",BindingFlags.NonPublic|BindingFlags.Instance),
  fRoute=typeof(RoadDriver).GetField("routeStuck",BindingFlags.NonPublic|BindingFlags.Instance),fBranch=typeof(RoadDriver).GetField("branchStuck",BindingFlags.NonPublic|BindingFlags.Instance);
 sealed class Jump{public string ai,flight;public int lap;public float takeoffSpeed=-1,takeoffAlong,landAlong=-1,landY,landTime=-1,airTime;public string landOn="",outcome="";public bool airborne;public float airStart;public Vector3 landPos;}
 IEnumerator AiRace(int laps,float timescale){race.laps=laps;yield return Begin("moto",false,true);AudioListener.volume=0;
  var flights=race.GetComponent<MountainFlights>()?.flights??Array.Empty<MountainFlights.Flight>();
  // the player waits 60 m to the side of the start, kinematic, out of every racing line
  var park=car.Body.position+Vector3.Cross(Vector3.up,car.transform.forward)*60+Vector3.up*30;car.enabled=false;car.Body.isKinematic=true;car.Body.position=park;car.transform.position=park;Physics.SyncTransforms();
  Time.timeScale=timescale;
  var ais=race.Racers.Where(r=>r.IsAi).ToList();var drivers=ais.Select(r=>race.Drivers.First(d=>d.Car==r.Car)).ToList();
  Note($"airace {race.courseId} laps {laps} AI {ais.Count} ({string.Join(", ",ais.Select(r=>r.Name+"/"+r.Car.GetComponent<VehicleConfiguration>().profileId))}) difficulty {race.difficulty} flights {string.Join(", ",flights.Select(f=>f.name))}");
  var jumps=new List<Jump>();var open=new Dictionary<(int,int),Jump>();var lastPos=new Vector3[ais.Count];var lastS=new float[ais.Count];var lastRoad=new float[ais.Count];int recoveries=0;
  for(int i=0;i<ais.Count;i++){int k=i;var rs=ais[i].Car.GetComponent<VehicleRespawn>();rs.Respawned+=()=>{var to=ais[k].Car.Body.position;float sTo=drivers[k].DriveRoad.Project(to,out _);
    var nearFlight=flights.Select(f=>(f,along:Vector3.Dot(lastPos[k]-f.start,Vector3.ProjectOnPlane(f.forward,Vector3.up).normalized))).OrderBy(x=>Mathf.Abs(Vector3.Dot(lastPos[k]-x.f.start,Vector3.Cross(Vector3.up,Vector3.ProjectOnPlane(x.f.forward,Vector3.up).normalized)))).FirstOrDefault();
    recoveries++;Note($"RECOVERY {ais[k].Name} lap {ais[k].Progress.CompletedLaps} from {lastPos[k]:F1} s {lastS[k]:F0} (road pos {lastRoad[k]:F0}) to {to:F1} s {sTo:F0} (road pos {race.road.Relative(sTo,race.Origin):F0}); moved {Vector3.Distance(lastPos[k],to):F0} m; backward along route {race.road.Relative(lastS[k],sTo):F0}/{race.road.Length:F0}; diagnostic {rs.RecoveryDiagnostic}");
    foreach(var j in open.Where(o=>o.Key.Item1==k).Select(o=>o.Value))if(j.outcome=="")j.outcome=$"recovered {(j.landTime>=0?$"{Time.time-j.landTime:F1} s after touchdown":"in the air/before landing")}, moved {Vector3.Distance(lastPos[k],to):F0} m";};}
  using var log=new StreamWriter($"{output}/airace-{race.courseId}-{DateTime.Now:HHmmss}.csv");log.WriteLine("t,ai,lap,gate,s,roadPos,x,y,z,speed,wheels,upY,tracking,rejoinStuck,routeStuck,recov,flightAlong");
  float t0=Time.time,nextLog=0;
  while(Time.time-t0<laps*420){yield return new WaitForFixedUpdate();AudioListener.volume=0;
   if(race.Flow.State!=RaceFlow.Stage.Racing)break;
   for(int i=0;i<ais.Count;i++){var r=ais[i];var d=drivers[i];var p=r.Car.Body.position;float s=d.DriveRoad.Project(p,out _);lastPos[i]=p;lastS[i]=s;lastRoad[i]=r.RoadPosition;
    for(int fi=0;fi<flights.Length;fi++){var f=flights[fi];var axis=Vector3.ProjectOnPlane(f.forward,Vector3.up).normalized;float along=Vector3.Dot(p-f.start,axis);float side=Vector3.Dot(p-f.start,Vector3.Cross(Vector3.up,axis));
     float lip=Vector3.Dot(f.lip-f.start,axis),end=Vector3.Dot(f.landingEnd-f.start,axis);bool inFlight=along>-40&&along<end+60&&Mathf.Abs(side)<40&&p.y>Mathf.Min(f.start.y,f.landingEnd.y)-25;
     var key=(i,fi);open.TryGetValue(key,out var j);
     if(inFlight&&j==null&&along<lip-20){j=new Jump{ai=r.Name,flight=f.name,lap=r.Progress.CompletedLaps};open[key]=j;}
     if(j==null)continue;
     if(!j.airborne&&j.takeoffSpeed<0&&r.Car.GroundedWheels==0&&along>lip-15){j.airborne=true;j.airStart=Time.time;j.takeoffSpeed=r.Car.Body.linearVelocity.magnitude;j.takeoffAlong=along;}
     if(j.airborne&&r.Car.GroundedWheels>=1&&Time.time-j.airStart>.4f&&j.landTime<0){j.airborne=false;j.landTime=Time.time;j.airTime=Time.time-j.airStart;j.landAlong=along;j.landY=p.y;j.landPos=p;
      j.landOn=Physics.Raycast(p+Vector3.up,Vector3.down,out var lh,4,~0,QueryTriggerInteraction.Ignore)?lh.collider.name:"?";}
     if(j.airborne&&r.Car.GroundedWheels>=1&&Time.time-j.airStart<=.4f)j.airborne=false;// a hop, not the flight
     bool done=j.outcome!=""||(j.landTime>=0&&Time.time-j.landTime>8)||(!inFlight&&along>end+60)||Time.time-t0>laps*420-1;
     if(j.landTime>=0&&j.outcome==""&&Time.time-j.landTime>1.5f&&r.Car.ForwardSpeed<1.5f&&(j.landTime>=0)){}
     if(done){if(j.outcome=="")j.outcome=j.landTime<0?(j.takeoffSpeed<0?"no take-off (stopped before the lip)":"no touchdown logged"):$"continued: 8 s later speed {r.Car.ForwardSpeed:F1} s {s:F0} recoveries {d.RecoveryCount}";
      jumps.Add(j);open.Remove(key);}
     else if(j.landTime>=0&&Time.time-j.landTime<=8){bool tr=(bool)fTracking.GetValue(d);float rj=(float)fRejoin.GetValue(d);if(tr&&rj>j.airTime)j.outcome=j.outcome;}}
    if(Time.time>=nextLog){var fl=flights.Select(f=>Vector3.Dot(p-f.start,Vector3.ProjectOnPlane(f.forward,Vector3.up).normalized)).ToArray();
     log.WriteLine($"{Time.time-t0:F1},{r.Name},{r.Progress.CompletedLaps},{r.Progress.NextGate},{s:F0},{r.RoadPosition:F0},{p.x:F1},{p.y:F1},{p.z:F1},{r.Car.ForwardSpeed:F1},{r.Car.GroundedWheels},{r.Car.transform.up.y:F2},{fTracking.GetValue(d)},{(float)fRejoin.GetValue(d):F1},{(float)fRoute.GetValue(d):F1},{d.RecoveryCount},{string.Join("/",fl.Select(x=>x.ToString("F0")))}");}}
   if(Time.time>=nextLog)nextLog=Time.time+.25f;
   if(ais.All(r=>r.Progress.Finished||r.Dnf))break;}
  Time.timeScale=1;
  foreach(var g in jumps.GroupBy(j=>j.flight)){Note($"FLIGHT {g.Key}: {g.Count()} attempts");foreach(var j in g)Note($"  {j.ai} lap {j.lap}: take-off {(j.takeoffSpeed<0?"-":j.takeoffSpeed.ToString("F1"))} m/s at along {j.takeoffAlong:F0}; touchdown {(j.landTime<0?"-":$"along {j.landAlong:F0} y {j.landY:F1} on {j.landOn} after {j.airTime:F1} s")}; {j.outcome}");}
  foreach(var r in ais)Note($"AI {r.Name}: laps {r.Progress.CompletedLaps}/{laps} finished {r.Progress.Finished} dnf {r.Dnf} recoveries {r.Recoveries} minimap-drawn {(!r.Dnf)}");
  Note($"airace {race.courseId}: total AI recoveries {recoveries}; race time {Time.time-t0:F0} s");}
}
}
#endif
