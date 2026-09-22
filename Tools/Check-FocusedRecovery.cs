using System;using System.IO;using System.Linq;using System.Collections.Generic;using System.Reflection;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using Racer;using Object=UnityEngine.Object;
public static class CheckFocusedRecovery {
 const BindingFlags Flags=BindingFlags.Instance|BindingFlags.NonPublic;
 public static string Main(){
 EditorSceneManager.OpenScene("Assets/Scenes/StreetLoopReverse.unity");var race=Object.FindAnyObjectByType<RaceDirector>();var car=race.vehicle;var reset=car.GetComponent<VehicleRespawn>();
 typeof(ArcadeVehicle).GetMethod("Awake",Flags).Invoke(car,null);typeof(VehicleRespawn).GetMethod("Awake",Flags).Invoke(reset,null);
 foreach(var body in Object.FindObjectsByType<Rigidbody>())if(body!=car.Body)body.gameObject.SetActive(false);
 race.Racers.Clear();var state=new RacerState("check",car,race.gates.Length-1,3);race.Racers.Add(state);var rows=new List<string>();float now=10;
 var record=typeof(VehicleRespawn).GetMethod("RecordSafePosition",Flags,null,new[]{typeof(float)},null);var support=typeof(VehicleRespawn).GetMethod("Supported",Flags);var wheels=typeof(ArcadeVehicle).GetProperty("GroundedWheels");
 foreach(var branch in Object.FindObjectsByType<WoodlandRoute>()){
 reset.CancelRecovery();state.Branch.Begin(branch);reset.SeedCoursePosition(branch.At(0,out _));float previous=-1;
 for(float s=0;s<branch.Length-5;s+=1){var p=branch.At(s,out var f);var args=new object[]{p,Vector3.ProjectOnPlane(f,Vector3.up).normalized,Vector3.zero,Quaternion.identity};bool ok=(bool)support.Invoke(reset,args);var pos=ok?(Vector3)args[2]:p+Vector3.up*.55f;var rot=ok?(Quaternion)args[3]:Quaternion.LookRotation(f);
 car.transform.SetPositionAndRotation(pos,rot);car.Body.position=pos;car.Body.rotation=rot;car.Body.linearVelocity=f*10;car.Body.angularVelocity=Vector3.zero;wheels.SetValue(car,4);Physics.SyncTransforms();record.Invoke(reset,new object[]{now});now+=.11f;
 if(s%10==0){rows.Add($"{branch.title} s={s} y={p.y:F2} support={ok} poseY={pos.y:F2} anchor={reset.SafeStation:F2} advanced={reset.SafeStation>previous+.1f}");previous=reset.SafeStation;}}
 }
 File.WriteAllLines("Docs/FocusedRecovery/before-safe-samples.txt",rows);EditorSceneManager.OpenScene("Assets/Scenes/StreetLoopReverse.unity");return string.Join("\n",rows.Where(r=>r.Contains("advanced=False")));
 }
}
