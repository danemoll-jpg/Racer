using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEditor;
using Object=UnityEngine.Object;
namespace Racer.Editor {
public sealed class CR019ContactProbe:MonoBehaviour {
 public readonly List<string> hits=new();
 void OnCollisionEnter(Collision c){if(c.collider.transform.root.name!="Memory loop - north is +Z")hits.Add(c.collider.name);}
}
// Ordinary-frame virtual Gamepad pursuit, no per-frame pose/velocity correction or manual simulation.
public static class CR019Driving {
 static Gamepad pad;static ArcadeVehicle car;static RaceFlow flow;static CR019ContactProbe probe;
 static InputSettings.EditorInputBehaviorInPlayMode focus;static InputSettings.BackgroundBehavior background;static bool run;
 static int direction,lastFrame,stage;static float start,peak,minUp,maxLateral,minBrakeSpeed;static Vector3 initial;static bool braking,shoulder;
 static readonly List<string> log=new();static readonly List<double> frames=new();static double last;
 public static void Start(){
  if(!Application.isPlaying||pad!=null)throw new Exception("Play mode and no active drive required");
  flow=Object.FindAnyObjectByType<RaceFlow>();flow.UseValidationSave(Path.GetFullPath(CR019Commercial.Dir+"/test-save"));flow.Save.Settings.vsync=true;flow.Save.Settings.frameLimit=60;flow.Save.ApplySettings();
  car=flow.Race.vehicle;probe=car.gameObject.AddComponent<CR019ContactProbe>();pad=InputSystem.AddDevice<Gamepad>();var s=InputSystem.settings;focus=s.editorInputBehaviorInPlayMode;background=s.backgroundBehavior;run=Application.runInBackground;s.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;s.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;Application.runInBackground=true;
  log.Clear();log.Add($"Ordinary-frame virtual Gamepad; unmodified vehicle; fixedDeltaTime={Time.fixedDeltaTime}; {Screen.width}x{Screen.height}; VSync={QualitySettings.vSyncCount}; cap={Application.targetFrameRate}; isolated save={flow.Save.DirectoryPath}. No human/physical-device claim.");
  direction=1;stage=0;flow.Race.RestartRace();lastFrame=-1;EditorApplication.update+=Tick;
 }
 static float Offset(float x){return 9.5f*Mathf.SmoothStep(0,1,Mathf.InverseLerp(-475,-375,x))*(1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(-335,-235,x)));}
 static Vector3 Target(float x){var p=CR019Commercial.Road(x);p.z-=Offset(x);return p;}
 static void Place(){float x=direction>0?-558:254;var p=CR019Commercial.Road(x);var f=(CR019Commercial.Road(x+direction)-p).normalized;car.Body.isKinematic=false;car.Body.position=p+Vector3.up*.65f;car.Body.rotation=Quaternion.LookRotation(f);car.transform.SetPositionAndRotation(car.Body.position,car.Body.rotation);car.Body.linearVelocity=f*30;car.Body.angularVelocity=Vector3.zero;car.ClearSteering();Physics.SyncTransforms();Object.FindAnyObjectByType<ChaseCamera>().Snap();flow.Race.ResetSampling(car.Body.position,Time.timeAsDouble);start=Time.time;initial=car.Body.position;peak=0;minUp=1;maxLateral=0;minBrakeSpeed=100;braking=shoulder=false;probe.hits.Clear();frames.Clear();last=EditorApplication.timeSinceStartup;stage=1;}
 static void Tick(){
  if(!Application.isPlaying){Finish("ABORT Play mode ended");return;}if(Time.frameCount==lastFrame)return;lastFrame=Time.frameCount;
  try{
   if(stage==0){if(flow.State==RaceFlow.Stage.Racing)Place();return;}
   var p=car.Body.position;float elapsed=Time.time-start;double now=EditorApplication.timeSinceStartup;if(elapsed>1)frames.Add((now-last)*1000);last=now;
   peak=Mathf.Max(peak,car.ForwardSpeed);minUp=Mathf.Min(minUp,car.transform.up.y);float lateral=Mathf.Abs(p.z-CR019Commercial.Road(Mathf.Clamp(p.x,-565,265)).z);maxLateral=Mathf.Max(maxLateral,lateral);if(lateral>7)shoulder=true;
   bool brakeZone=p.x> -170&&p.x< -65;float desired=brakeZone?13:32;if(brakeZone){braking=true;minBrakeSpeed=Mathf.Min(minBrakeSpeed,car.ForwardSpeed);}
   float look=Mathf.Max(10,Mathf.Abs(car.ForwardSpeed)*.65f);var target=Target(Mathf.Clamp(p.x+direction*look,-560,260));var local=car.transform.InverseTransformDirection(target-p);float angle=Mathf.Atan2(local.x,local.z)*Mathf.Rad2Deg;float steering=Mathf.Clamp(angle/24,-1,1);var state=new GamepadState{leftStick=new Vector2(steering,0),rightTrigger=car.ForwardSpeed<desired?Mathf.Clamp01((desired-car.ForwardSpeed)*.45f+.15f):0,leftTrigger=car.ForwardSpeed>desired+1?Mathf.Clamp01((car.ForwardSpeed-desired)*.2f):0};InputSystem.QueueStateEvent(pad,state);
   if((direction>0&&p.x>=250)||(direction<0&&p.x<=-552)||elapsed>80||car.transform.up.y<.3f){
    bool pass=elapsed<80&&minUp>.8f&&peak>=29&&braking&&minBrakeSpeed<17&&shoulder&&lateral<3&&probe.hits.Count==0;
    frames.Sort();log.Add($"{(pass?"PASS":"FAIL")} {(direction>0?"eastbound":"westbound")} entire commercial road: {elapsed:F2}s, start={initial:F2}, end={p:F2}, peak={peak:F2}m/s, brake minimum={minBrakeSpeed:F2}m/s, min upright={minUp:F3}, max shoulder offset={maxLateral:F2}m, end road error={lateral:F2}m, non-terrain contacts={probe.hits.Count} [{string.Join(",",probe.hits)}]. Median/p95 editor intervals={frames[frames.Count/2]:F2}/{frames[(int)(frames.Count*.95f)]:F2}ms; samples={frames.Count}.");
    InputSystem.QueueStateEvent(pad,new GamepadState());if(direction==1){direction=-1;Place();}else Finish("Completed two directions; gentle road bends, brake/reaccelerate, south shoulder departure and re-entry. No race completion or player-data write.");
   }
  }catch(Exception e){Finish("FAIL "+e);}
 }
 static void Finish(string message){EditorApplication.update-=Tick;log.Add(message);File.WriteAllLines(CR019Commercial.Dir+"/driving.txt",log);if(pad!=null)InputSystem.RemoveDevice(pad);pad=null;if(probe)Object.Destroy(probe);var s=InputSystem.settings;s.editorInputBehaviorInPlayMode=focus;s.backgroundBehavior=background;Application.runInBackground=run;if(flow&&Application.isPlaying)flow.Pause();}
}
}
