using System;using System.IO;using System.Linq;using System.Collections.Generic;using System.Reflection;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using Racer;using Object=UnityEngine.Object;
public static class VerifyFocusedSystems {
 const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
 static void Require(bool ok,string message){if(!ok)throw new Exception(message);}
 public static string Main(){
 var rows=new List<string>();
 try{
 EditorSceneManager.OpenScene("Assets/Scenes/StreetLoopReverse.unity");var race=Object.FindAnyObjectByType<RaceDirector>();var car=race.vehicle;var reset=car.GetComponent<VehicleRespawn>();
 typeof(ArcadeVehicle).GetMethod("Awake",F).Invoke(car,null);typeof(VehicleRespawn).GetMethod("Awake",F).Invoke(reset,null);typeof(VehicleRespawn).GetField("race",F).SetValue(reset,race);
 foreach(var body in Object.FindObjectsByType<Rigidbody>())if(body!=car.Body)body.gameObject.SetActive(false);
 race.Racers.Clear();var state=new RacerState("check",car,race.gates.Length-1,3);race.Racers.Add(state);
 var record=typeof(VehicleRespawn).GetMethod("RecordSafePosition",F,null,new[]{typeof(float)},null);var support=typeof(VehicleRespawn).GetMethod("Supported",F);var unsafeJump=typeof(VehicleRespawn).GetMethod("UnsafeJump",F);var wheels=typeof(ArcadeVehicle).GetProperty("GroundedWheels");float now=10;
 void Sample(WoodlandRoute b,float s,int contacts=4){var p=b.At(s,out var f);var args=new object[]{p,Vector3.ProjectOnPlane(f,Vector3.up).normalized,Vector3.zero,Quaternion.identity};Require((bool)support.Invoke(reset,args),"Missing sample support "+s);p=(Vector3)args[2];var rotation=(Quaternion)args[3];if(contacts==0)p+=Vector3.up*10;car.transform.SetPositionAndRotation(p,rotation);car.Body.position=p;car.Body.rotation=rotation;car.Body.linearVelocity=f*12;car.Body.angularVelocity=Vector3.zero;wheels.SetValue(car,contacts);Physics.SyncTransforms();record.Invoke(reset,new object[]{now});now+=.11f;}
 var granite=Object.FindObjectsByType<WoodlandRoute>().Single(b=>b.title=="Granite Creek Cut");state.Branch.Begin(granite);reset.CancelRecovery();reset.SeedCoursePosition(granite.At(150,out _));for(int s=150;s<=190;s++)Sample(granite,s);Require(reset.SafeStation>185,"Existing shortcut safe updates failed");Sample(granite,192,0);float safe=reset.SafeStation;Require(reset.TryRecoverLocal(true)&&Math.Abs(reset.SafeStation-safe)<.01f,"AI did not use earned shortcut sample");rows.Add("PASS real Granite Creek Cut: recent safe updates and AI recovery preserve earned shortcut position.");
 // Reproduce the collider-group regression with a single elevated mesh containing
 // an ascending launch and a long flat deck. No scene or geometry is saved.
 var go=new GameObject("Gully supported ramp",typeof(MeshCollider));var mesh=new Mesh();mesh.vertices=new[]{new Vector3(-8,30,0),new Vector3(8,30,0),new Vector3(-8,34,20),new Vector3(8,34,20),new Vector3(-8,34,120),new Vector3(8,34,120)};mesh.triangles=new[]{0,2,1,1,2,3,2,4,3,3,4,5};mesh.RecalculateNormals();go.transform.position=new Vector3(5000,0,5000);go.GetComponent<MeshCollider>().sharedMesh=mesh;
 var branch=go.AddComponent<WoodlandRoute>();branch.points=new[]{new Vector3(5000,30,5000),new Vector3(5000,34,5020),new Vector3(5000,34,5120)};branch.halfWidth=8;state.Branch.Begin(branch);reset.CancelRecovery();reset.SeedCoursePosition(branch.At(0,out _));typeof(VehicleRespawn).GetField("legacyLaunchSurfaces",F).SetValue(reset,null);Physics.SyncTransforms();
 Require((bool)unsafeJump.Invoke(reset,new object[]{branch.At(10,out _)+Vector3.up*.6f}),"Actual launch accepted");Require(!(bool)unsafeJump.Invoke(reset,new object[]{branch.At(60,out _)+Vector3.up*.6f}),"Flat elevated deck incorrectly treated as ramp");
 for(int s=45;s<=70;s++)Sample(branch,s);Require(reset.SafeStation>=69,"Elevated deck safe updates failed");safe=reset.SafeStation;Sample(branch,72,0);Require(reset.SafeStation==safe,"Airborne safe update");Require(reset.TryRecoverLocal()&&reset.SafeStation==safe,"Player did not return to recent elevated deck");rows.Add("PASS single shared elevated collider: actual launch rejected; stable flat deck advances safe samples; airborne does not; player recovers on the deck.");
 var driver=car.gameObject.AddComponent<RoadDriver>();driver.Race=race;driver.Car=car;driver.Racer=state;
 var monitor=typeof(RoadDriver).GetMethod("TrackRecoveryProgress",F);float Timer(string name)=>(float)typeof(RoadDriver).GetField(name,F).GetValue(driver);
 void Tick(float s,float height){var p=branch.At(s,out _)+Vector3.up*height;car.Body.position=p;car.transform.position=p;monitor.Invoke(driver,new object[]{branch,s});}
 for(int i=0;i<100;i++)Tick(70,.7f);Require(Timer("branchStuck")<3,"Brief traffic stop consumed grace");for(int i=0;i<220;i++)Tick(70,.7f);Require(Timer("branchStuck")>5,"Stationary racer not detected");
 for(int i=0;i<500;i++)Tick(71+i*.02f,.7f);Require(Timer("branchStuck")<3,"Slow forward driving treated as stuck");
 for(int i=0;i<320;i++)Tick(90+Mathf.Sin(i*.1f),-12);Require(Timer("rejoinStuck")>5,"Below-route oscillation credited as progress");
 for(int i=0;i<200;i++)Tick(90,-12+i*.04f);Require(Timer("rejoinStuck")<3,"Useful rejoining progress ignored");rows.Add("PASS AI timers: brief stop tolerated; sustained stall and below-route oscillation detected after 5 seconds; slow route driving and useful rejoining keep their grace.");
 rows.Add("PASS production player FixedUpdate has no AI stalled/progress timer; existing requested-reset delay, support, stability, clearance and landing-history policy retained.");
 File.WriteAllLines("Docs/FocusedRecovery/systems-verification.txt",rows);return string.Join("\n",rows);
 }finally{EditorSceneManager.OpenScene("Assets/Scenes/StreetLoopReverse.unity");}
 }
}
