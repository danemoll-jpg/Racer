using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Racer;
using Object=UnityEngine.Object;

public static class FocusedExistingRecoveryChecks
{
    const string Folder="Docs/FocusedRecovery";
    const BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
    static void Require(bool condition,string message){if(!condition)throw new Exception(message);}
    public static string Recovery()
    {
        var rows=new List<string>();float now=10;
        foreach(var scene in new[]{"StreetLoopGreybox","StreetLoopReverse","ForestLoopReverse","MountainLoop","MountainLoopReverse"}){
            EditorSceneManager.OpenScene("Assets/Scenes/"+scene+".unity");var race=Object.FindAnyObjectByType<RaceDirector>();var car=race.vehicle;var reset=car.GetComponent<VehicleRespawn>();var road=race.road;
            typeof(ArcadeVehicle).GetMethod("Awake",Private).Invoke(car,null);typeof(VehicleRespawn).GetMethod("Awake",Private).Invoke(reset,null);
            foreach(var body in Object.FindObjectsByType<Rigidbody>())if(body!=car.Body)body.gameObject.SetActive(false);
            race.Racers.Clear();var state=new RacerState("check",car,race.gates.Length-1,3);race.Racers.Add(state);
            var record=typeof(VehicleRespawn).GetMethod("RecordSafePosition",Private,null,new[]{typeof(float)},null);
            var support=typeof(VehicleRespawn).GetMethod("Supported",Private);var wheels=typeof(ArcadeVehicle).GetProperty("GroundedWheels");
            void Sample(float station,int contacts,WoodlandRoute branch=null,bool airborne=false){
                var p=branch?branch.At(station,out var f):road.At(station,out f);var args=new object[]{p,Vector3.ProjectOnPlane(f,Vector3.up).normalized,Vector3.zero,Quaternion.identity};
                bool supported=(bool)support.Invoke(reset,args);if(supported){p=(Vector3)args[2];}else p+=Vector3.up*.55f;
                if(airborne)p.y+=12;
                var rotation=supported?(Quaternion)args[3]:Quaternion.LookRotation(f);
                car.transform.SetPositionAndRotation(p,rotation);car.Body.position=p;car.Body.rotation=rotation;car.Body.linearVelocity=f*18;car.Body.angularVelocity=Vector3.zero;wheels.SetValue(car,contacts);Physics.SyncTransforms();record.Invoke(reset,new object[]{now});now+=.11f;
            }
            float origin=road.Project(car.transform.position,out _)+20;reset.CancelRecovery();reset.SeedCoursePosition(road.At(origin,out _));
            for(int i=0;i<36;i++)Sample(origin+i,2);
            float anchor=reset.SafeStation;Require(Math.Abs(anchor-(origin+35))<5,$"{scene}: normal two-contact driving left anchor stale ({anchor} vs {origin+35})");
            Require(anchor<=origin+35+.05f,$"{scene}: forward recovery credit");
            Sample(origin+38,0,null,true);Require(Math.Abs(reset.SafeStation-anchor)<.001f,scene+": airborne updated safe point");
            int next=state.Progress.NextGate;Require(reset.TryRecoverLocal(),scene+": recent occupied support was not usable");Require(state.Progress.NextGate==next,scene+": recovery awarded gate progress");
            Require(Math.Abs(road.Project(car.Body.position,out _)-anchor)<2,scene+": did not recover to recent station");
            reset.ResetVehicle();Require(reset.Pending,scene+": short delay not queued");float retry=(float)typeof(VehicleRespawn).GetField("nextAttempt",Private).GetValue(reset);Require(retry-Time.time>.7f&&retry-Time.time<.9f,scene+": unexpected reset delay");reset.CancelRecovery();
            rows.Add($"PASS {scene}: two supported contacts advance every 0.1s after 0.35s/1m stability; recent reset succeeds; no airborne/forward/gate credit; 0.8s requested-reset delay.");
            if(scene=="ForestLoopReverse"){
                var layout=Object.FindAnyObjectByType<ForestLayout>();float start=layout.jumpStarts[0];
                var unsafeJump=typeof(VehicleRespawn).GetMethod("UnsafeJump",Private);
                Require((bool)unsafeJump.Invoke(reset,new object[]{road.At(start+15,out _)+Vector3.up*.5f}),"Forest ramp was accepted");
                float landing=layout.jumpEnds[0]-25;reset.SeedCoursePosition(road.At(landing,out _));
                for(int i=0;i<15;i++)Sample(landing+i,2);
                Require(Math.Abs(reset.SafeStation-(landing+14))<5,"Forest post-jump runout did not replace old recovery");
                rows.Add("PASS Forest: launch rejected while stable driving in the same jump window's landing runout advances recovery.");reset.CancelRecovery();
            }
            foreach(var flight in race.GetComponent<MountainFlights>()?.flights??Array.Empty<MountainFlights.Flight>()){
                float before=road.Project(flight.start,out _)-75;reset.SeedCoursePosition(road.At(before,out _));for(int i=0;i<9;i++)Sample(before+i,4);float old=reset.SafeStation;
                float lip=road.Project(flight.lip,out _),finish=road.Project(flight.landingEnd,out _)+20;
                for(float s=before+10;s<finish;s+=6){bool ramp=s>=road.Project(flight.start,out _)-55&&s<=lip;Sample(s,ramp?4:0,null,!ramp);Require(Math.Abs(reset.SafeStation-old)<.01f,$"{scene}/{flight.name}: ramp/airborne anchor");}
                for(int i=0;i<25;i++)Sample(finish+i,2);
                Require(Math.Abs(reset.SafeStation-(finish+24))<5,$"{scene}/{flight.name}: landing failed to advance: {reset.SafeStation}, target {finish+24}");
                var history=(IList)typeof(VehicleRespawn).GetField("history",Private).GetValue(reset);Require(history.Count>0,"No landing history");
                foreach(var h in history)Require((float)h.GetType().GetField("station").GetValue(h)>=finish-1,"Pre-jump fallback retained");
                float post=reset.SafeStation;Sample(finish+27,0,null,true);Require(reset.TryRecoverLocal(),"Post-jump recovery unavailable");Require(Math.Abs(reset.SafeStation-post)<.01f,"Later crash returned before completed jump");
                rows.Add($"PASS {scene}/{flight.name}: ramp and air rejected; post-landing anchor={post:F2}; pre-jump history retired; later crash resets after jump.");reset.CancelRecovery();
            }
            now+=10;
        }
        EditorSceneManager.OpenScene("Assets/Scenes/StreetLoopReverse.unity");File.WriteAllLines(Folder+"/recovery-checks.txt",rows);return string.Join("\n",rows);
    }
}
