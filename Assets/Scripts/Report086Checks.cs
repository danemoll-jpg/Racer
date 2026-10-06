#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Racer {
// 0.86 targeted checks, added to the 0.80 runner (same muted isolated save, same helpers): PROBE_CASES="case:args;...".
public sealed partial class Report080Checks {
 IEnumerator Cases086(string[] a)=>a[0] switch{"inout86"=>InOut086(a[1],a[2]),_=>null};

 // Part A: drive into the House 3 pool and back out. inout86:Scene:profiles. The vehicle starts on the basin floor 16 m
 // north of the coping facing south and drives straight in at full throttle (down the shallow-end slope into the water),
 // brakes to a stop in the water, then reverses straight back up the slope at full reverse until it is on dry ground 3 m
 // clear of the water for 1 s. Then the same from the deepest point forwards (as 0.85): facing north, full throttle.
 IEnumerator InOut086(string scene,string profiles){
  foreach(var profile in profiles.Split(',')){
   yield return Enter085(scene,profile);var car=race.vehicle;
   var pool=ShallowWater.Active.FirstOrDefault(w=>w&&w.gameObject.scene==SceneManagerScene()&&w.name=="House 3 swimming pool");if(!pool){Note($"{Scene}: no House 3 swimming pool");yield break;}
   var c=pool.transform.position;var start=new Vector3(c.x,0,c.z+24);start.y=Ground085(start,pool.Surface+10);
   car.GetComponent<VehicleInput>().enabled=false;car.enabled=false;var resp=car.GetComponent<VehicleRespawn>();int resets=0;Action onR=()=>resets++;resp.Respawned+=onR;
   var rot=Quaternion.Euler(0,180,0);car.Body.position=start+Vector3.up*(car.suspensionLength*.7f);car.Body.rotation=rot;car.transform.SetPositionAndRotation(car.Body.position,rot);
   car.Body.linearVelocity=Vector3.zero;car.Body.angularVelocity=Vector3.zero;car.ClearSteering();resp.SeedCoursePosition(start);
   for(int i=0;i<25;i++){car.Simulate(0,0,0,Time.fixedDeltaTime);yield return new WaitForFixedUpdate();}
   float t0=Time.time,maxImm=0,inAt=-1,deepest=99;var dir=Vector3.back;var log=new StringBuilder("phase,t,x,y,z,speed,wheels,immersion\n");
   // In: full throttle until in the water, then 1.2 s more, then brake to a stop.
   while(Time.time-t0<14){AudioListener.volume=0;var p=car.Body.position;bool wet=car.WaterImmersion>.05f;if(wet&&inAt<0)inAt=Time.time-t0;maxImm=Mathf.Max(maxImm,car.WaterImmersion);deepest=Mathf.Min(deepest,p.y);
    float ang=Vector3.SignedAngle(Vector3.ProjectOnPlane(car.transform.forward,Vector3.up),dir,Vector3.up);bool braking=inAt>=0&&Time.time-t0>inAt+1.2f;
    car.Simulate(braking?0:1,braking?1:0,Mathf.Clamp(ang/25f,-1,1),Time.fixedDeltaTime);yield return new WaitForFixedUpdate();
    log.AppendLine($"in,{Time.time-t0:F2},{p.x:F2},{p.y:F2},{p.z:F2},{car.ForwardSpeed:F2},{car.GroundedWheels},{car.WaterImmersion:F2}");
    if(braking&&Mathf.Abs(car.ForwardSpeed)<.3f)break;}
   var stopAt=car.Body.position;
   // Out: reverse straight back north up the slope.
   float t1=Time.time,dryFor=0;bool outR=false;
   while(Time.time-t1<25&&resets==0){AudioListener.volume=0;var p=car.Body.position;var back=-car.transform.forward;back.y=0;
    float ang=Vector3.SignedAngle(Vector3.ProjectOnPlane(car.transform.forward,Vector3.up),dir,Vector3.up);
    car.Simulate(0,1,Mathf.Clamp(-ang/25f,-1,1),Time.fixedDeltaTime);yield return new WaitForFixedUpdate();
    log.AppendLine($"out,{Time.time-t1:F2},{p.x:F2},{p.y:F2},{p.z:F2},{car.ForwardSpeed:F2},{car.GroundedWheels},{car.WaterImmersion:F2}");
    bool clear=!Wet085(p)&&!Wet085(p+Vector3.forward*3)&&car.WaterImmersion<=.001f;dryFor=clear?dryFor+Time.fixedDeltaTime:0;if(dryFor>1){outR=true;break;}}
   resp.Respawned-=onR;car.enabled=true;car.GetComponent<VehicleInput>().enabled=true;File.WriteAllText($"{output}/A-inout-{Scene}-{profile}.csv",log.ToString());
   Check(inAt>=0&&outR&&resets==0,$"{Scene} {profile}: drove in from the basin floor (in the water after {inAt:F1}s, lowest body y {deepest:F2}, immersion up to {maxImm:F2}, stopped at {V(stopAt)}), reversed out up the shallow end: {(outR?$"out in {Time.time-t1:F1}s":"NOT out")}, resets {resets}");
   yield return Menu();}
  yield return Escape085(scene,profiles,"House 3 swimming pool","0,90,180,270");}
}
}
#endif
