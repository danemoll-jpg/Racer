#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
namespace Racer.Tests {
// Opt-in ordinary-frame impact validation. Never saved on the scene.
public sealed class NeighborhoodImpactValidation:MonoBehaviour {
 readonly List<string> rows=new();ArcadeVehicle car;RaceDirector race;Gamepad pad;
 InputSettings.BackgroundBehavior oldBackground;InputSettings.EditorInputBehaviorInPlayMode oldFocus;bool oldRun;
 void Check(bool ok,string s){rows.Add((ok?"PASS ":"FAIL ")+s);File.WriteAllLines("Docs/CR016-017/impacts.txt",rows);}
 float Ground(Vector3 p){foreach(var h in Physics.RaycastAll(new Vector3(p.x,300,p.z),Vector3.down,600,1,QueryTriggerInteraction.Ignore).OrderBy(h=>h.distance))if(h.collider.transform.root.name=="Memory loop - north is +Z")return h.point.y;throw new Exception("No terrain support");}
 void Place(Vector3 p,Vector3 f,float speed){p.y=Ground(p)+.75f;car.Body.position=p;car.Body.rotation=Quaternion.LookRotation(f);car.transform.SetPositionAndRotation(p,car.Body.rotation);car.Body.linearVelocity=f*speed;car.Body.angularVelocity=Vector3.zero;car.ClearSteering();Physics.SyncTransforms();Camera.main.GetComponent<ChaseCamera>().Snap();race.ResetSampling(p,Time.timeAsDouble);}
 IEnumerator Start(){
  car=FindAnyObjectByType<ArcadeVehicle>();race=FindAnyObjectByType<RaceDirector>();pad=InputSystem.AddDevice<Gamepad>();oldRun=Application.runInBackground;Application.runInBackground=true;oldBackground=InputSystem.settings.backgroundBehavior;oldFocus=InputSystem.settings.editorInputBehaviorInPlayMode;InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
  rows.Add("Ordinary Update/FixedUpdate, real PhysX trigger contacts; virtual Gamepad. Initial pose and 3/30 m/s velocity injected. No physical-controller testing. "+Screen.width+"x"+Screen.height);
  var all=FindObjectsByType<BreakableProp>();var samples=new[]{all.First(p=>p.name=="Mailbox - Dan - blue X"),all.First(p=>p.name=="Simple bend warning"),all.First(p=>p.name=="Dan west yard fence 1"),all.First(p=>p.name=="Breakable - Shortcut direction sign"),all.First(p=>p.name=="Breakable - Jump approach sign")};
  foreach(var prop in samples)foreach(float speed in new[]{3f,30f})foreach(bool glance in new[]{false,true}){
   race.RestartRace();yield return null;var c=prop.GetComponent<BoxCollider>();var f=prop.transform.forward;f.y=0;f.Normalize();var right=Vector3.Cross(Vector3.up,f);float half=Vector3.Dot(c.bounds.extents,new Vector3(Mathf.Abs(right.x),0,Mathf.Abs(right.z)));var target=c.bounds.center;target.y=0;var start=target-f*5+right*(glance?half+.35f:0);Place(start,f,speed);int count=prop.BreakCount;float elapsed=0,minUp=1,maxVertical=0,minSpeed=speed;int gate=race.Progress.NextGate;
   while(elapsed<1.5f){float desired=speed;InputSystem.QueueStateEvent(pad,new GamepadState{rightTrigger=Mathf.Clamp01((desired-car.ForwardSpeed)*.7f+.3f)});elapsed+=Time.deltaTime;minUp=Mathf.Min(minUp,car.transform.up.y);maxVertical=Mathf.Max(maxVertical,Mathf.Abs(car.Body.linearVelocity.y));minSpeed=Mathf.Min(minSpeed,car.ForwardSpeed);yield return null;}
   Check(prop.IsBroken&&prop.BreakCount==count+1&&!c.enabled,$"{prop.name}; {speed}m/s {(glance?"glance":"center")}; broken={prop.IsBroken}, count delta={prop.BreakCount-count}; min speed={minSpeed:F2}, upright={minUp:F3}, peak vertical={maxVertical:F2}; gate {gate}->{race.Progress.NextGate}; laps={race.Progress.CompletedLaps}");
   Check(minUp>.8f&&race.Progress.CompletedLaps==0,"Stable impact and no credited lap");
   if(speed==3&&!glance){car.GetComponent<VehicleRespawn>().ResetVehicle();Check(prop.IsBroken&&!c.enabled,"Ordinary vehicle reset leaves prop broken");}
  }
  race.RestartRace();yield return null;var run=all.Where(p=>p.name.StartsWith("Dan west yard fence")).OrderBy(p=>p.transform.position.z).ToArray();Place(run[0].transform.position-Vector3.forward*5,Vector3.forward,12);float t=0;while(t<1.7f){InputSystem.QueueStateEvent(pad,new GamepadState{rightTrigger=.4f});t+=Time.deltaTime;yield return null;}Check(run.All(p=>p.IsBroken),"Consecutive drive through all three Dan fence sections");
  InputSystem.QueueStateEvent(pad,new GamepadState());race.RestartRace();yield return null;
  // Stress dispatches the actual contact handler with the vehicle collider; physical approach matrix above is separate.
  var vc=car.GetComponent<Collider>();for(int repeat=0;repeat<8;repeat++){race.RestartRace();car.Body.linearVelocity=Vector3.forward*30;foreach(var p in all)p.SendMessage("OnTriggerEnter",vc);Check(all.All(p=>p.IsBroken)&&BreakableProp.MovingDebrisCount<=BreakableProp.MaximumMovingDebris,"Burst "+repeat+": all 18 break once; moving="+BreakableProp.MovingDebrisCount);foreach(var p in all)p.SendMessage("OnTriggerEnter",vc);}
  car.Body.linearVelocity=Vector3.zero;yield return new WaitForSeconds(4.5f);Check(BreakableProp.MovingDebrisCount==0&&all.All(p=>p.GetComponentsInChildren<Renderer>().All(r=>!r.enabled)),"Debris cleaned after lifetime; zero active fragments; no spawned rigidbodies");
  for(int i=0;i<5;i++){race.RestartRace();yield return null;Check(all.All(p=>!p.IsBroken&&!p.PendingRestore&&p.GetComponent<BoxCollider>().enabled&&p.GetComponentsInChildren<Renderer>().All(r=>r.enabled))&&race.Progress.CompletedLaps==0&&race.Progress.NextGate==0,"Restart "+i+": visual/sensor restoration and race at grid");}
  var overlap=samples[0];Place(overlap.transform.position,Vector3.forward,0);BreakableProp.RestoreRace();Check(overlap.PendingRestore&&!overlap.GetComponent<BoxCollider>().enabled,"Restoration deferred while car overlaps original prop bounds");car.GetComponent<VehicleRespawn>().ResetVehicle();yield return null;Check(!overlap.PendingRestore&&!overlap.IsBroken,"Deferred prop restored only after car vacates");
  Check(race.Progress.CompletedLaps==0&&race.Progress.NextGate==0,"Impacts/resets credited no checkpoints or laps");
  rows.Add("COMPLETE failures="+rows.Count(s=>s.StartsWith("FAIL")));File.WriteAllLines("Docs/CR016-017/impacts.txt",rows);race.RestartRace();Destroy(gameObject);
 }
 void OnDestroy(){if(pad!=null&&pad.added)InputSystem.RemoveDevice(pad);InputSystem.settings.backgroundBehavior=oldBackground;InputSystem.settings.editorInputBehaviorInPlayMode=oldFocus;Application.runInBackground=oldRun;}
}
}

#endif
