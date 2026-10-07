#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Racer {
// 0.91 targeted checks, added to the 0.80 runner (same muted isolated save, same helpers). Everything is driven through an
// emulated controller (an InputSystem test gamepad): menus with the D-pad / A / B / LB / RB, driving with the stick and
// triggers. Dan's real campaign save is only ever copied (PROBE_DANSAVE names the copy).
public sealed partial class Report080Checks {
 // The structural check (RaceMenus logs "MENU NAVIGATION" when a page has a row the controller cannot reach) over every
 // page this session showed: navsummary91 fails if any did.
 static readonly List<string> navErrors=new();static bool navHooked;
 IEnumerator NavSummary091(){Check(navErrors.Count==0,$"every menu page shown in this session: every visible row reachable with the controller ({(navErrors.Count==0?"no failures":string.Join(" | ",navErrors.Distinct()))})");yield break;}
 IEnumerator Run091(string[] a){if(!navHooked){navHooked=true;Application.logMessageReceived+=(m,st,t)=>{if(m.StartsWith("MENU NAVIGATION"))navErrors.Add(m);};}
  if(flow){flow.Save.Settings.hints=false;flow.Save.SaveSettings();}
  return a[0] switch{"jump91"=>Jump091(a[1],a[2],a.Length>3?a[3]:""),"navsummary91"=>NavSummary091(),"measure91"=>Measure091(a[1],a[2],a[3]),"roamjump91"=>RoamJump091(a[1],a[2],a[3]),_=>More091(a)};}
 IEnumerator More091(string[] a){IEnumerator run=null;Cases091(a,ref run);return run;}
 partial void Cases091(string[] a,ref IEnumerator run);
 Gamepad pad91;
 // The batch editor has no focus: let the test devices' input through (as the 0.90 keyboard check did).
 void Pad091(){InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
  if(pad91==null||!pad91.added)pad91=InputSystem.AddDevice<Gamepad>("Report091 pad");}
 void Hold091(float throttle,float brake,float steer){InputSystem.QueueStateEvent(pad91,new GamepadState{rightTrigger=throttle,leftTrigger=brake,leftStick=new Vector2(Mathf.Clamp(steer,-1,1),0)});}
 IEnumerator Tap091(GamepadButton b){InputSystem.QueueStateEvent(pad91,new GamepadState().WithButton(b));yield return null;yield return null;InputSystem.QueueStateEvent(pad91,new GamepadState());yield return null;yield return null;yield return null;}
 IEnumerator DanSave091(){var copy=Environment.GetEnvironmentVariable("PROBE_DANSAVE");var target=Path.Combine(saveDir,Campaign.File);File.Copy(copy,target,true);Campaign.Load(saveDir);
  Note($"  Dan's save copy loaded: money {Campaign.Current.money}, chapter {Campaign.Current.chapter}, owned [{string.Join(",",Campaign.Current.owned)}], error {Campaign.Error??"none"}");yield return null;}
 // Flat out along the course road with the controller (trigger fully in, stick steering at the road 15 m ahead), as a
 // player takes a jump event; every flight's verdict is logged.
 IEnumerator FlatOut091(float seconds,List<string> flights){float t0=Time.time;string lastDiag=flow.Activities.LastJumpDiagnostic,lastFeedback=null;
  while(Time.time-t0<seconds&&flow.State==RaceFlow.Stage.Racing&&!CampaignRun.Done){AudioListener.volume=0;var car=race.vehicle;var p=car.Body.position;float s=race.road.Project(p,out _);var to=race.road.At(s+15,out _)-p;to.y=0;
   float ang=Vector3.SignedAngle(Vector3.ProjectOnPlane(car.transform.forward,Vector3.up),to,Vector3.up);Hold091(1,0,car.GroundedWheels>=2?ang/25f:0);yield return null;
   var diag=flow.Activities.LastJumpDiagnostic;if(diag!=lastDiag){lastDiag=diag;flights.Add("flight: "+diag);}
   var fb=flow.Activities.Feedback;if(fb!=lastFeedback&&!string.IsNullOrEmpty(fb)){lastFeedback=fb;flights.Add("message: "+fb.Replace("\n"," / "));}}
  Hold091(0,0,0);}
 // Jump targets: per speed ("flat" = trigger fully in; a number = held m/s) one run from the event's start through the
 // real event flow with Testing on (nothing saved), driven with the controller; the first flight from the event's site.
 IEnumerator Measure091(string id,string vehicle,string speeds){
  yield return Load("StreetLoopGreybox");yield return Menu();Pad091();var e=CampaignData.Find(id);var rows=new List<string>();
  flow.Save.Settings.unlockEverything=true;Campaign.Testing=true;flow.Save.SaveSettings();
  foreach(var sp in speeds.Split(',')){float hold=sp=="flat"?999:F(sp);
   flow.StartCampaignEvent(e,vehicle);yield return EventRunning089();string before=flow.Activities.LastJumpDiagnostic,diag=null;float t0=Time.time,top=0;
   while(Time.time-t0<60&&flow.State==RaceFlow.Stage.Racing){AudioListener.volume=0;var car=race.vehicle;var p=car.Body.position;float s=race.road.Project(p,out _);var to=race.road.At(s+15,out _)-p;to.y=0;
    float ang=Vector3.SignedAngle(Vector3.ProjectOnPlane(car.transform.forward,Vector3.up),to,Vector3.up);float v=car.ForwardSpeed;top=Mathf.Max(top,v);
    Hold091(v<hold?1:0,v>hold+2?.4f:0,car.GroundedWheels>=2?ang/25f:0);yield return null;
    var d=flow.Activities.LastJumpDiagnostic;if(d!=before&&d!=null&&d.Contains("site="+e.Site+" ")){diag=d;break;}if(d!=before)before=d;}
   Hold091(0,0,0);string msg=flow.Activities.Feedback?.Replace("\n"," / ");
   string line=$"{id} {vehicle} {(sp=="flat"?"flat out":"held "+sp+" m/s")} (top {top:F1} m/s): {diag??"no flight from the site"} | {msg}";rows.Add(line);Note(line);
   flow.Pause();yield return null;flow.QuitRace();yield return null;}
  flow.Save.Settings.unlockEverything=false;Campaign.Testing=false;flow.Save.SaveSettings();File.AppendAllLines(output+"/jump-targets.txt",rows);}
 // A Free Roam jump site: the vehicle placed 140 m before it on its line, then driven with the controller along the site's
 // forward line (flat out or a held speed); the first flight from the site. Nothing is saved (records go to the isolated save).
 IEnumerator RoamJump091(string id,string vehicle,string speeds){
  yield return Load("StreetLoopGreybox");yield return Menu();Pad091();var rows=new List<string>();
  flow.Save.Settings.unlockEverything=true;Campaign.Testing=true;flow.OpenGarage();flow.SelectVehicle(vehicle);flow.CloseGarage();flow.StartFreeRoam();
  float w0=Time.realtimeSinceStartup;while((flow==null||!flow.Started||LoadingScreen.Holding||flow.State!=RaceFlow.Stage.Racing||!race.FreeRoam)&&Time.realtimeSinceStartup-w0<120){yield return null;Bind();AudioListener.volume=0;}
  var site=flow.Activities.Sites.FirstOrDefault(s=>s.id==id);if(!site){Note("no site "+id);yield break;}var f=Vector3.ProjectOnPlane(site.forward,Vector3.up).normalized;
  foreach(var sp in speeds.Split(',')){float hold=sp=="flat"?999:F(sp);var car=race.vehicle;
   var from=site.transform.position-f*140;if(Physics.Raycast(from+Vector3.up*200,Vector3.down,out var hit,600,car.groundMask,QueryTriggerInteraction.Ignore))from=hit.point;
   car.Body.position=from+Vector3.up*1.2f;car.Body.rotation=Quaternion.LookRotation(f);car.transform.SetPositionAndRotation(car.Body.position,car.Body.rotation);car.Body.linearVelocity=car.Body.angularVelocity=Vector3.zero;
   car.GetComponent<VehicleRespawn>().SeedCoursePosition(car.Body.position);float s0=Time.time;while(Time.time-s0<3){Hold091(0,0,0);yield return null;}
   string before=flow.Activities.LastJumpDiagnostic,diag=null;float t0=Time.time,top=0;
   while(Time.time-t0<25&&flow.State==RaceFlow.Stage.Racing){AudioListener.volume=0;var p=car.Body.position;float along=Vector3.Dot(p-site.transform.position,f);var aim=site.transform.position+f*(along+15)-p;aim.y=0;
    float ang=Vector3.SignedAngle(Vector3.ProjectOnPlane(car.transform.forward,Vector3.up),aim,Vector3.up);float v=car.ForwardSpeed;top=Mathf.Max(top,v);
    Hold091(v<hold?1:0,v>hold+2?.4f:0,car.GroundedWheels>=2?ang/25f:0);yield return null;
    var d=flow.Activities.LastJumpDiagnostic;if(d!=before&&d!=null&&d.Contains("site="+id+" ")){diag=d;break;}if(d!=before)before=d;}
   Hold091(0,0,0);string line=$"Free Roam {id} '{site.title}' {vehicle} {(sp=="flat"?"flat out":"held "+sp+" m/s")} (top {top:F1} m/s): {diag??"no flight from the site"} | targets {site.bronze}/{site.silver}/{site.gold} | {flow.Activities.Feedback?.Replace("\n"," / ")}";rows.Add(line);Note(line);}
  flow.Save.Settings.unlockEverything=false;Campaign.Testing=false;flow.Save.SaveSettings();File.AppendAllLines(output+"/roam-jumps.txt",rows);}
 // The Opening Jump (or another jump event) on a copy of Dan's save, driven flat out with the controller.
 IEnumerator Jump091(string id,string vehicle,string mode){
  yield return Load("StreetLoopGreybox");yield return Menu();Pad091();yield return DanSave091();var e=CampaignData.Find(id);
  flow.StartCampaignEvent(e,vehicle);yield return EventRunning089();var flights=new List<string>();
  bool done=false;var c=StartCoroutine(Wrap089(FlatOut091(e.TimeLimit+2,flights),()=>done=true));float r0=Time.realtimeSinceStartup;
  while(!done&&flow.State!=RaceFlow.Stage.Results&&Time.realtimeSinceStartup-r0<e.TimeLimit*3+30)yield return null;
  if(!done){StopCoroutine(c);Hold091(0,0,0);}
  yield return Results089(e.TimeLimit+10);var o=CampaignRun.Last;
  foreach(var f in flights)Note("  "+f);
  Note($"{id} {vehicle} flat out ({mode}): result {(o==null?"none":o.Dnf?"no score":Campaign.Measure(e,o.Score))}, medal {o?.Medal}, passed {o?.Passed}");
  if(flow.State==RaceFlow.Stage.Results)flow.QuitRace();yield return null;}
}
}
#endif
