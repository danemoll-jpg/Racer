#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace Racer.Editor {
public sealed class ForestWaterJumpCheck : MonoBehaviour {
 IEnumerator Start(){
  yield return null;yield return null;
  var race=FindAnyObjectByType<RaceDirector>();var flow=race.Flow;
  flow.UseValidationSave(Path.GetFullPath("Temp/forest-water-check-save"));AudioListener.volume=0;race.opponents=false;race.traffic=false;race.laps=1;flow.StartRace();
  while(flow.State!=RaceFlow.Stage.Racing)yield return null;
  var car=race.vehicle;car.GetComponent<VehicleConfiguration>().Apply("moto");car.GetComponent<VehicleInput>().enabled=false;car.enabled=false;
  var axis=new Vector3(-1,0,.08f).normalized;var p=new Vector3(490,0,-202.58f);
  var hit=Physics.RaycastAll(p+Vector3.up*160,Vector3.down,220,1,QueryTriggerInteraction.Ignore).Where(h=>h.collider.name.StartsWith("Ground_")).OrderByDescending(h=>h.point.y).First();p.y=hit.point.y+.65f;
  var rotation=Quaternion.LookRotation(Vector3.ProjectOnPlane(axis,hit.normal),hit.normal);car.Body.position=p;car.Body.rotation=rotation;car.transform.SetPositionAndRotation(p,rotation);car.Body.linearVelocity=axis*24;car.Body.angularVelocity=Vector3.zero;
  race.ResetSampling(p,0);car.GetComponent<VehicleRespawn>().SeedCoursePosition(p);
  var log=new List<string>{"seconds,x,y,z,speed,grounded,water"};float begin=Time.time;bool flight=false,landed=false;float launchSpeed=0,launchX=0,landX=0,maxWet=0;
  while(Time.time-begin<14){
   car.Simulate(1,0,0,Time.fixedDeltaTime);yield return new WaitForFixedUpdate();p=car.Body.position;
   log.Add($"{Time.time-begin:F3},{p.x:F3},{p.y:F3},{p.z:F3},{car.Body.linearVelocity.magnitude:F3},{car.GroundedWheels},{car.WaterImmersion:F3}");maxWet=Math.Max(maxWet,car.WaterImmersion);
   if(!flight&&p.x<435&&car.GroundedWheels==0){flight=true;launchSpeed=car.Body.linearVelocity.magnitude;launchX=p.x;}
   if(flight&&p.x<365&&car.GroundedWheels>=2){landed=true;landX=p.x;break;}
  }
  File.WriteAllLines("Docs/ForestWaterJump/vehicle-check.csv",log);
  File.WriteAllText("Docs/ForestWaterJump/vehicle-check.txt",$"One bounded motorcycle check: start 16m before ramp at 24m/s, ordinary full throttle and zero steering, unchanged vehicle physics. Flight={flight}; launch speed={launchSpeed:F2}m/s, X={launchX:F2}; supported landing beyond both water bodies={landed}, X={landX:F2}; max immersion={maxWet:F3}; final={car.Body.position}. This is one local automated pass, not human handling acceptance.");
  EditorApplication.isPlaying=false;
 }
}
}
#endif
