#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Racer {
// 0.96 Part D7 (BUG-009): the AI rivals over the Mountain Loop summit flight (Homeward Summit Flight, main 2220 m): each rival's take-off speed
// and where it lands, by vehicle. aijump96:eventId:flightIndex   (an event whose rivals include cars: c4-summit-final)
public sealed partial class Report080Checks {
 IEnumerator AiJump096(string id,int flightIndex,int seconds){
  yield return DanCopy095();Campaign.Testing=true;flow.Save.Settings.unlockEverything=true;var e=CampaignData.Find(id);
  flow.StartCampaignEvent(e,"atv");yield return EventRunning089();
  var flights=race.GetComponent<MountainFlights>();var flight=flights.flights[flightIndex];var road=race.road;
  Note($"flight '{flight.name}': approach {flight.approachStation:F0}, end {flight.endStation:F0}, lip {V(flight.lip)}, landing end {V(flight.landingEnd)}, takeoff cap {flight.aiTakeoffSpeed}, entry {flight.aiEntrySpeed}");
  var pilot=Pilot089(2);Time.timeScale=3;float t0=Time.time;
  var state=new Dictionary<RacerState,(bool air,float airStart,Vector3 lastGround,float lastSpeed,float takeoffSpeed,bool took,bool landed,float maxSpeedWindow)>();
  var report=new List<string>();var done=new HashSet<RacerState>();
  while(Time.time-t0<seconds&&flow.State==RaceFlow.Stage.Racing&&done.Count<race.Racers.Count-1){AudioListener.volume=0;yield return new WaitForFixedUpdate();
   foreach(var r in race.Racers.Skip(1)){if(done.Contains(r)||!r.Car)continue;float s=road.Project(r.Car.Body.position,out _);bool inWindow=road.Relative(s,flight.approachStation)<road.Relative(flight.endStation,flight.approachStation);
    state.TryGetValue(r,out var st);bool grounded=r.Car.GroundedWheels>=2;float speed=r.Car.Body.linearVelocity.magnitude;
    if(inWindow){st.maxSpeedWindow=Mathf.Max(st.maxSpeedWindow,speed);
     if(!st.air&&!grounded&&speed>8&&Vector3.Distance(r.Car.Body.position,flight.lip)<60){st.air=true;st.airStart=Time.time;st.takeoffSpeed=st.lastSpeed;st.took=true;}
     else if(st.air&&grounded&&Time.time-st.airStart>.4f&&!st.landed){st.landed=true;st.air=false;var p=r.Car.Body.position;float ls=road.Project(p,out float lat);
      string vehicle=r.Car.GetComponent<VehicleConfiguration>().profileId;float along=Vector3.Dot(p-flight.landingEnd,flight.forward);
      report.Add($"{r.Name} ({vehicle}): took off at {st.takeoffSpeed:F1} m/s ({st.takeoffSpeed*2.237f:F0} mph), air {Time.time-st.airStart:F2} s, landed at {V(p)} station {ls:F0} (end {flight.endStation:F0}), {Vector3.Distance(p,flight.landingEnd):F0} m from the landing end ({(along>0?"beyond":"short of")} it by {Mathf.Abs(along):F0} m), upright {r.Car.transform.up.y:F2}, lateral {lat:F1}");Note("   "+report[report.Count-1]);}
     if(!st.air&&grounded)st.lastSpeed=speed;}
    else{if(st.took&&st.landed)done.Add(r);if(grounded)st.lastSpeed=speed;}
    state[r]=st;}}
  Time.timeScale=1;Unpilot089(pilot);
  foreach(var r in race.Racers.Skip(1)){state.TryGetValue(r,out var st);string vehicle=r.Car?r.Car.GetComponent<VehicleConfiguration>().profileId:"?";if(!st.took)Note($"{r.Name} ({vehicle}): never took off in the window (max speed there {st.maxSpeedWindow:F1} m/s) - ran out of time or went round");}
  File.WriteAllLines(output+"/aijump.txt",report);flow.QuitRace();Campaign.Testing=false;yield return null;}
}
}
#endif
