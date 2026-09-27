#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace Racer.Editor {
public sealed class RouteAtlasChecks : MonoBehaviour {
 readonly List<string> report=new();
 void Check(bool pass,string detail){report.Add((pass?"PASS: ":"FAIL: ")+detail);File.WriteAllLines("Docs/RouteAtlas/targeted-checks.txt",report);}
 IEnumerator Start(){
  yield return null;yield return null;
  var race=FindAnyObjectByType<RaceDirector>();var flow=race.Flow;var road=race.road;road.Initialize();
  flow.UseValidationSave(Path.GetFullPath("Temp/route-atlas-check-save"));AudioListener.volume=0;race.opponents=false;race.traffic=false;race.laps=1;flow.StartRace();
  while(flow.State!=RaceFlow.Stage.Racing)yield return null;
  Check(race.courseId=="forest-reverse-v6-granite-main","New Forest Reverse timing identity active");
  Check(race.Branches.All(b=>b.title!="Granite Saddle"),"Granite Saddle is absent from optional shortcut selection/entitlement");
  var car=race.vehicle;car.enabled=false;car.GetComponent<VehicleInput>().enabled=false;car.Body.isKinematic=true;
  float start=road.Project(race.gates[0].transform.position,out _)-12;
  var p=road.At(start,out var f)+Vector3.up*.6f;car.Body.position=p;car.transform.SetPositionAndRotation(p,Quaternion.LookRotation(Vector3.ProjectOnPlane(f,Vector3.up)));race.ResetSampling(p,0);
  double now=0;int samples=0;bool optional=false;float maxLateral=0;
  // One deterministic player-like traversal through production Sample and RaceGate crossing.
  // This checks progression/data, not human handling or a physics lap.
  for(float s=start+1.5f;s<610;s+=1.5f){p=road.At(s,out f)+Vector3.up*.6f;car.Body.position=p;car.Body.rotation=Quaternion.LookRotation(Vector3.ProjectOnPlane(f,Vector3.up));car.transform.SetPositionAndRotation(p,car.Body.rotation);race.Sample(p,f,now+=.075);road.Project(p,out float d);maxLateral=Math.Max(maxLateral,d);optional|=race.Racers[0].Branch.Route!=null;samples++;}
  Check(race.Progress.MissedGates==0&&race.Progress.PenaltySeconds==0,$"Player-like main traversal: {samples} samples, next gate {race.Progress.NextGate}, missed {race.Progress.MissedGates}, penalty {race.Progress.PenaltySeconds}; no former-detour gate required");
  Check(!optional,"Promoted main traversal requires no shortcut entitlement");
  Check(maxLateral<1,"Main-route projection stays on the promoted data line");
  // One ordinary AI driver, one bounded traversal. No AI parameters or physics are changed.
  race.opponents=true;flow.StartRace();while(flow.State!=RaceFlow.Stage.Racing)yield return null;
  var driver=race.Drivers.First(d=>d.Racer!=null);foreach(var other in race.Drivers.Where(d=>d!=driver)){other.enabled=false;other.Car.Body.isKinematic=true;}
  car.enabled=false;car.GetComponent<VehicleInput>().enabled=false;car.Body.isKinematic=true;
  driver.Place(120,.55f);int recoveries=driver.RecoveryCount;float begin=Time.time;float best=120;
  Check(driver.DriveRoad==race.road,"AI selects the promoted main RaceRoad directly");
  var telemetry=new List<string>{"seconds,station,speed,recoveries,branch"};
  while(Time.time-begin<40&&best<590){yield return new WaitForSeconds(.5f);float s=road.Project(driver.Car.Body.position,out _);best=Math.Max(best,s);telemetry.Add($"{Time.time-begin:F2},{s:F2},{driver.Car.ForwardSpeed:F2},{driver.RecoveryCount},{driver.Racer.Branch.Route?.title??"none"}");}
  File.WriteAllLines("Docs/RouteAtlas/ai-targeted.csv",telemetry);
  Check(best>=590,$"One 40-second AI check reached main station {best:F1}/590; recoveries {driver.RecoveryCount-recoveries}; route {driver.DriveRoad.name}");
  Check(driver.DriveRoad==race.road,"AI main-route identity retained at end of check");
  File.WriteAllText("Docs/RouteAtlas/targeted-done.txt","Completed one player-like progression traversal and one bounded AI check. See retained results; no broad matrix.");
  EditorApplication.isPlaying=false;
 }
}
}

#endif
