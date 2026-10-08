#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Racer {
// 0.97 Part B checks (muted, isolated save), added to Report097Getaway's Run097 chain:
//   lap97:scene:vehicle:weather:grip      one lap of the course with the racing AI driver at the player's vehicle; lap time, resets, missed gates
//   rivals97:scene:vehicle:weather        a 1-lap race against three AI rivals in that weather: every AI finishes, resets counted
//   feel97:vehicle:weather                a straight-line brake from 25 m/s and a steady turn at 20 m/s on the Street Loop: stopping distance, sideways slip
//   ice97                                 the frozen pool in Snow: the grip under a vehicle standing on the ice, and a stop from 15 m/s
//   cat97:weather:grip                    the record key a race in that weather would use
public sealed partial class Report080Checks {
 void FixColors97(){var st=flow.Save.Settings;if(st.bodyColors==null||st.bodyColors.Length!=VehicleProfile.All.Length){var g=new int[VehicleProfile.All.Length];for(int i=0;i<g.Length;i++)g[i]=st.bodyColors!=null&&i<st.bodyColors.Length?st.bodyColors[i]:-1;st.bodyColors=g;}}
 static Weather W97(string s)=>(Weather)Enum.Parse(typeof(Weather),s,true);
 IEnumerator Race97(string scene,string player,string roster,string weather,bool grip,bool lap){
  yield return Load(scene);yield return Menu();var st=flow.Save.Settings;st.weather=(int)W97(weather);st.timeOfDay=0;st.estimateAiFinishes=false;st.weatherGrip=grip;flow.Save.SaveSettings();
  FixColors97();flow.OpenGarage();flow.SelectVehicle(player);flow.CloseGarage();race.opponents=roster.Length>0;race.traffic=false;race.laps=1;race.difficulty=1;if(roster.Length>0)race.opponentRoster=roster.Split(',');
  flow.StartRace();float t0=Time.realtimeSinceStartup;while(flow.State!=RaceFlow.Stage.Racing&&Time.realtimeSinceStartup-t0<40){AudioListener.volume=0;yield return null;}
  var car=race.vehicle;string got=car.GetComponent<VehicleConfiguration>().profileId;
  var pilot=car.gameObject.AddComponent<RoadDriver>();pilot.Initialize(race,car,true,1,1);pilot.Racer=race.Racers[0];car.GetComponent<VehicleInput>().enabled=false;
  Time.timeScale=3;float t1=Time.time;float rt1=Time.realtimeSinceStartup;float minGrip=1;string look=WorldLook.Current?WorldLook.Current.Conditions:"";
  while(flow.State!=RaceFlow.Stage.Results&&Time.time-t1<600&&Time.realtimeSinceStartup-rt1<900){AudioListener.volume=0;yield return null;minGrip=Mathf.Min(minGrip,car.GripScale);}
  Time.timeScale=1;Destroy(pilot);car.GetComponent<VehicleInput>().enabled=true;
  string Prof(RacerState r)=>r.Car.GetComponent<VehicleConfiguration>().profileId;
  string Row(RacerState r)=>$"{(r.IsAi?r.Name:"PLAYER")} {Prof(r)}: {(r.Progress.Finished?RaceHud.FormatTime(r.Progress.AdjustedTime(race.Clock)):r.Dnf?"DNF":"not finished")} missed {r.Progress.MissedGates} resets {r.Recoveries}";
  bool ok=flow.State==RaceFlow.Stage.Results&&race.Racers.All(r=>r.Progress.Finished);
  var line=$"{scene} {got} {weather} grip {(grip?"on":"OFF")} [{look}] lowest grip scale {minGrip:F2}: {string.Join("; ",race.Racers.Select(Row))}; record key {race.Category}";
  Check(ok,line);File.AppendAllText(Path.Combine(Environment.GetEnvironmentVariable("PROBE_OUT")??".","weather97.txt"),line+"\n");
  yield return Menu();}
 // braking and cornering numbers on a straight/curve of the loop, stepped by hand (the car's own step off)
 IEnumerator Feel97(string vehicle,string weather,bool grip){
  yield return Load("StreetLoopGreybox");yield return Menu();var st=flow.Save.Settings;st.weather=(int)W97(weather);st.timeOfDay=0;st.weatherGrip=grip;flow.Save.SaveSettings();
  FixColors97();flow.OpenGarage();flow.SelectVehicle(vehicle);flow.CloseGarage();race.opponents=false;race.traffic=false;race.laps=1;
  flow.StartRace();float t0=Time.realtimeSinceStartup;while(flow.State!=RaceFlow.Stage.Racing&&Time.realtimeSinceStartup-t0<40){AudioListener.volume=0;yield return null;}
  var car=race.vehicle;car.GetComponent<VehicleInput>().enabled=false;car.enabled=false;var rd=race.road;
  // the straightest 200 m of the road
  float bestS=0,bestTurn=999;for(float s=0;s<rd.Length-260;s+=20){rd.At(s,out var f0);rd.At(s+200,out var f1);float turn=Vector3.Angle(f0,f1);if(turn<bestTurn){bestTurn=turn;bestS=s;}}
  var p=rd.At(bestS,out var f);f.y=0;f.Normalize();
  void Put(Vector3 at,Vector3 fwd,float speed){var rot=Quaternion.LookRotation(fwd);at+=Vector3.up*1.1f;car.Body.position=at;car.Body.rotation=rot;car.transform.SetPositionAndRotation(at,rot);car.Body.linearVelocity=fwd*speed;car.Body.angularVelocity=Vector3.zero;car.ClearSteering();}
  Put(p,f,25);for(int i=0;i<30;i++){car.Simulate(0,0,0,Time.fixedDeltaTime);yield return new WaitForFixedUpdate();}
  // brake from 25 m/s
  Put(p,f,25);var start=car.Body.position;float stopT=0;float scale0=0;
  for(int i=0;i<600&&car.Body.linearVelocity.magnitude>.5f;i++){car.Simulate(0,1,0,Time.fixedDeltaTime);yield return new WaitForFixedUpdate();stopT+=Time.fixedDeltaTime;scale0=car.GripScale;}
  float stop=Vector3.Distance(start,car.Body.position);
  // steady left turn at 20 m/s, full lock for 2 s: the sideways slip angle and the speed lost
  Put(p,f,20);float maxSlip=0;float v0=20;for(int i=0;i<30;i++){car.Simulate(0,0,0,Time.fixedDeltaTime);yield return new WaitForFixedUpdate();}
  for(int i=0;i<100;i++){car.Simulate(.5f,0,-.6f,Time.fixedDeltaTime);yield return new WaitForFixedUpdate();var v=car.Body.linearVelocity;v.y=0;if(v.magnitude>3)maxSlip=Mathf.Max(maxSlip,Vector3.Angle(car.transform.forward,v));}
  float vEnd=car.Body.linearVelocity.magnitude;
  var line=$"{vehicle,-8} {weather,-5} grip {(grip?"on ":"OFF")}: stop from 25 m/s in {stop:F1} m ({stopT:F1} s), grip scale {scale0:F2}; turn at 20 m/s, steer 0.6: slip angle {maxSlip:F0} deg, speed after 2 s {vEnd:F1} m/s";
  Check(stop>1,line);File.AppendAllText(Path.Combine(Environment.GetEnvironmentVariable("PROBE_OUT")??".","weather97.txt"),line+"\n");
  car.enabled=true;car.GetComponent<VehicleInput>().enabled=true;yield return Menu();}
 // the frozen pool at Dan's house in Free Roam, in Snow: a vehicle standing on the ice has the ice grip, and stops slowly
 IEnumerator Ice97(string vehicle){
  yield return Load("StreetLoopGreybox");yield return Menu();var st=flow.Save.Settings;st.roamWeather=2;st.weatherGrip=true;flow.Save.SaveSettings();
  FixColors97();yield return EnterRoam("StreetLoopGreybox",vehicle);Bind();
  yield return new WaitForSecondsRealtime(3);
  var waters=FindObjectsByType<ShallowWater>(FindObjectsSortMode.None).Where(w=>w.Ice).ToList();Note($"frozen water bodies: {waters.Count}: {string.Join(", ",waters.Select(w=>w.name+" at "+V(w.Ice.bounds.center)))}");
  var car=race.vehicle;car.GetComponent<VehicleInput>().enabled=false;car.enabled=false;
  foreach(var w in waters.Take(4)){
   var c=w.Ice.bounds;var at=new Vector3(c.center.x,c.max.y+1.1f,c.center.z);car.Body.position=at;car.Body.rotation=Quaternion.LookRotation(Vector3.right);car.transform.SetPositionAndRotation(at,Quaternion.LookRotation(Vector3.right));car.Body.linearVelocity=Vector3.right*15;car.Body.angularVelocity=Vector3.zero;car.ClearSteering();
   float scale=0;var start=car.Body.position;for(int i=0;i<300&&car.Body.linearVelocity.magnitude>.5f;i++){car.Simulate(0,1,0,Time.fixedDeltaTime);yield return new WaitForFixedUpdate();scale=car.GripScale;}
   float onIce=Vector3.Distance(start,car.Body.position);
   string below="";if(Physics.Raycast(new Vector3(c.center.x,c.max.y+3,c.center.z),Vector3.down,out var hb,10,~0,QueryTriggerInteraction.Ignore))below=hb.collider.name+" "+hb.collider.bounds.size.ToString("F1")+" ice bounds "+c.size.ToString("F1");
   var line=$"{w.name}: below [{below}] on the ice grip scale {scale:F2}, stop from 15 m/s in {onIce:F1} m (bounds {c.size.x:F0} x {c.size.z:F0} m)";
   Check(scale<=.4f,line);File.AppendAllText(Path.Combine(Environment.GetEnvironmentVariable("PROBE_OUT")??".","weather97.txt"),line+"\n");}
  car.enabled=true;car.GetComponent<VehicleInput>().enabled=true;flow.QuitSplit(false);}
 IEnumerator Cat97(string weather,bool grip){
  yield return Load("StreetLoopGreybox");yield return Menu();var st=flow.Save.Settings;st.weather=(int)W97(weather);st.weatherGrip=grip;flow.Save.SaveSettings();
  var line=$"record key {weather} grip {(grip?"on":"off")}: {race.Category} -> era {RecordView.Era(race.Category)}";Check(true,line);File.AppendAllText(Path.Combine(Environment.GetEnvironmentVariable("PROBE_OUT")??".","weather97.txt"),line+"\n");yield return null;}
 IEnumerator Run097b(string[] a)=>a[0] switch{
  "lap97"=>Race97(a[1],a[2],"",a[3],a[4]=="on",true),
  "rivals97"=>Race97(a[1],a[2],a.Length>4?a[4]:"original,tourer,atv",a[3],true,false),
  "feel97"=>Feel97(a[1],a[2],a.Length<4||a[3]=="on"),
  "ice97"=>Ice97(a[1]),
  "cat97"=>Cat97(a[1],a[2]=="on"),
  _=>null};
}
}
#endif
