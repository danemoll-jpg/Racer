using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using Object=UnityEngine.Object;
namespace Racer.Editor {
public sealed class CR014ContactProbe : MonoBehaviour {
 public int contacts; public string last;
 void OnCollisionEnter(Collision c){if(c.collider.transform.root.name=="Woods replacing later subdivisions"){contacts++;last=c.collider.name;}}
}
public static class CR014Driving {
 public static void Maneuvers(){
  if(!Application.isPlaying)throw new Exception("Play first");
  var car=Object.FindAnyObjectByType<ArcadeVehicle>();var body=car.Body;var woods=GameObject.Find("Woods replacing later subdivisions");
  var path=JsonUtility.FromJson<WoodlandBenchmark.Plan>(File.ReadAllText("Docs/CR014/routes.json")).routes[1].points;
  var trunk=woods.GetComponentsInChildren<BoxCollider>().Where(c=>c.name.StartsWith("CR014")).OrderBy(c=>(c.bounds.center-path[0]).sqrMagnitude).First();
  Vector3 target=trunk.bounds.center;target.y=trunk.bounds.min.y;
  var terrain=GameObject.Find("Memory loop - north is +Z").transform;
  bool Ground(Vector3 p,out Vector3 q){foreach(var h in Physics.RaycastAll(p+Vector3.up*100,Vector3.down,200,1).OrderBy(h=>h.distance))if(h.collider.transform.IsChildOf(terrain)){q=h.point;return true;}q=p;return false;}
  Vector3 start=default,forward=default;bool found=false;
  for(int a=0;a<360;a+=15){var f=Quaternion.Euler(0,a,0)*Vector3.forward;var p=target-f*7;if(!Ground(p,out p))continue;
   bool clear=!Physics.OverlapBox(p+Vector3.up*1.2f,new Vector3(1.1f,1,2.3f),Quaternion.LookRotation(f),1).Any(c=>c.transform.root==woods.transform);
   if(clear){start=p;forward=f;found=true;break;}}
  if(!found)throw new Exception("No supported clear contact-test start");
  body.position=start+Vector3.up*.7f;body.rotation=Quaternion.LookRotation(forward);car.transform.SetPositionAndRotation(body.position,body.rotation);body.linearVelocity=body.angularVelocity=Vector3.zero;car.ClearSteering();Physics.SyncTransforms();Object.FindAnyObjectByType<ChaseCamera>().Snap();
  var probe=car.gameObject.AddComponent<CR014ContactProbe>();var pad=InputSystem.AddDevice<Gamepad>();
  var settings=InputSystem.settings;var oldFocus=settings.editorInputBehaviorInPlayMode;var oldBackground=settings.backgroundBehavior;bool oldRun=Application.runInBackground;
  settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;Application.runInBackground=true;
  float started=Time.time,peak=0,reverse=0,minUp=1;int phase=-1;Vector3 collisionPosition=start,reverseEnd=start;float collisionTime=-1;var log=new List<string>();
  EditorApplication.CallbackFunction tick=null;
  tick=()=>{try{
   float elapsed=Time.time-started;peak=Mathf.Max(peak,car.ForwardSpeed);reverse=Mathf.Min(reverse,car.ForwardSpeed);minUp=Mathf.Min(minUp,car.transform.up.y);
   if(probe.contacts>0&&collisionTime<0){collisionTime=elapsed;collisionPosition=body.position;}
   int next=elapsed<4?0:elapsed<7?1:elapsed<10?2:elapsed<13?3:elapsed<14?4:5;
   if(next!=phase){log.Add($"phase={next} time={elapsed:F2} position={body.position:F2} speed={car.ForwardSpeed:F2} contacts={probe.contacts}");if(next==3)reverseEnd=body.position;phase=next;}
   var state=new GamepadState();if(phase==0)state.rightTrigger=Mathf.Clamp01((4-car.ForwardSpeed)*.7f+.2f);if(phase==1||phase==2)state.leftTrigger=.65f;if(phase==3){state.rightTrigger=.5f;state.leftStick=new Vector2(.45f,0);}if(phase==4)state=state.WithButton(GamepadButton.North);
   InputSystem.QueueStateEvent(pad,state);
   if(phase<5)return;
   log.Add($"{(probe.contacts>0&&reverse< -1&&minUp>.8f?"PASS":"FAIL")} ordinary-frame virtual Gamepad contact/brake/reverse/turn/recovery: contacts={probe.contacts}, tree={probe.last}, first contact={collisionTime:F2}s, peak={peak:F2}m/s, reverse={reverse:F2}m/s, upright={minUp:F3}, reversed distance={Vector3.Distance(collisionPosition,reverseEnd):F2}m. Reset button sent at 13s; final position={body.position:F2}; HUD={Object.FindAnyObjectByType<RaceHud>().display.text}");
   Finish();
  }catch(Exception ex){log.Add("FAIL "+ex);Finish();}};
  void Finish(){EditorApplication.update-=tick;InputSystem.RemoveDevice(pad);Object.Destroy(probe);settings.editorInputBehaviorInPlayMode=oldFocus;settings.backgroundBehavior=oldBackground;Application.runInBackground=oldRun;File.WriteAllLines("Docs/CR014/maneuvers.txt",log);}
  EditorApplication.update+=tick;
 }
}
}
