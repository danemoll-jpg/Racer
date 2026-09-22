using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEditor.SceneManagement;
using Racer;
using Object=UnityEngine.Object;
public static class CheckEarnedRecovery {
 const BindingFlags Flags=BindingFlags.Instance|BindingFlags.NonPublic;
 public static string Main(){
  EditorSceneManager.OpenScene("Assets/Scenes/MountainLoop.unity");var race=Object.FindAnyObjectByType<RaceDirector>();var car=race.vehicle;var reset=car.GetComponent<VehicleRespawn>();var road=race.road;road.Initialize();
  typeof(ArcadeVehicle).GetMethod("Awake",Flags).Invoke(car,null);typeof(VehicleRespawn).GetMethod("Awake",Flags).Invoke(reset,null);
  var record=typeof(VehicleRespawn).GetMethod("RecordSafePosition",Flags,null,new[]{typeof(float)},null);if(record==null)throw new Exception("Recovery changes have not compiled");
  var grounded=typeof(ArcadeVehicle).GetProperty("GroundedWheels");float now=10;var rows=new System.Collections.Generic.List<string>();
  void Sample(Vector3 p,Vector3 f,int wheels){var rotation=Quaternion.LookRotation(f,Vector3.up);car.transform.SetPositionAndRotation(p,rotation);car.Body.position=p;car.Body.rotation=rotation;car.Body.linearVelocity=f*8;car.Body.angularVelocity=Vector3.zero;grounded.SetValue(car,wheels);Physics.SyncTransforms();record.Invoke(reset,new object[]{now});now+=.25f;}
  foreach(var flight in race.GetComponent<MountainFlights>().flights){reset.CancelRecovery();float seed=0;var seedPoint=road.At(seed,out var dir);reset.SeedCoursePosition(seedPoint);for(int i=0;i<7;i++){var p=road.At(seed+i,out var f)+Vector3.up*.7f;Sample(p,f,4);}float saved=reset.SafeStation;if(((System.Collections.IList)typeof(VehicleRespawn).GetField("history",Flags).GetValue(reset)).Count==0)throw new Exception("Pre-jump fixture did not establish safe history: p="+car.Body.position+" up="+car.transform.up.y+" wheels="+car.GroundedWheels+" unsafe="+typeof(VehicleRespawn).GetMethod("UnsafeJump",Flags).Invoke(reset,new object[]{car.Body.position})+" stable="+typeof(VehicleRespawn).GetField("stableSince",Flags).GetValue(reset));
   float lip=road.Project(flight.lip,out _);float finish=road.Project(flight.landingEnd,out _)+35;
   for(float s=seed+8;s<finish;s+=8){var p=road.At(s,out var f)+Vector3.up*.7f;bool runway=s>=road.Project(flight.start,out _)-50&&s<=lip;int wheels=runway?4:0;if(!runway)p.y+=25;Sample(p,f,wheels);if(Math.Abs(reset.SafeStation-saved)>.01f)throw new Exception("Airborne/ramp anchor advanced "+flight.name);}
   for(float s=finish;s<finish+28;s+=2){var p=road.At(s,out var f)+Vector3.up*.7f;Sample(p,f,4);}
   float advanced=reset.SafeStation;float error=Mathf.Abs(Mathf.DeltaAngle(advanced/road.Length*360,(finish+26)/road.Length*360))*road.Length/360;
   if(error>10)throw new Exception($"Post-landing recovery did not advance: {flight.name}, safe={advanced}, expected near={finish+26}, error={error}");
   float anchor=reset.SafeStation;var crash=road.At(finish+35,out var crashDir)+Vector3.up*18;Sample(crash,crashDir,0);if(Math.Abs(reset.SafeStation-anchor)>.01f)throw new Exception("Crash replaced safe anchor");
   var history=(System.Collections.IList)typeof(VehicleRespawn).GetField("history",Flags).GetValue(reset);foreach(var h in history){float hs=(float)h.GetType().GetField("station").GetValue(h);if(hs<finish)throw new Exception("Pre-jump history survived successful landing");}
   rows.Add($"PASS {flight.name}: ramp/airborne anchor retained; stable grounded forward travel advanced recovery to {advanced:F2}; crash retained it; pre-jump fallback retired.");
  }
  // Discard fixture transforms; never save or change player progress.
  EditorSceneManager.OpenScene("Assets/Scenes/MountainLoop.unity");File.WriteAllLines("Docs/CR133-137/recovery-checks.txt",rows);return string.Join("\n",rows);
 }
}


