#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Racer {
// 0.90 targeted checks, added to the 0.80 runner (same muted isolated save, same helpers): PROBE_CASES="case:args;...".
public sealed partial class Report080Checks {
 // The new-player hints (and the controls card that holds a new player's first countdown) are off for every 0.90 case
 // except hints90, which checks them on a fresh save.
 IEnumerator Cases090(string[] a){if(a[0].EndsWith("90")&&flow){flow.Save.Settings.hints=a[0]=="hints90";flow.Save.SaveSettings();}return Cases090Run(a);}
 IEnumerator Cases090Run(string[] a)=>a[0] switch{"sites90"=>Sites090(a[1]),"targets90"=>Targets090(a[1],a[2]),"smash90"=>Smash090(a[1],a.Length>2&&a[2]=="save"),"jumps90"=>Jumps090(a[1],a[2],a[3]),_=>Cases090b(a)};
 partial void More090(string[] a,ref IEnumerator run);
 IEnumerator Cases090b(string[] a){IEnumerator run=null;More090(a,ref run);return run;}

 // The smash event driven by a simple script: head for the nearest prop of the site not yet broken, at full throttle.
 // (The driving runs beside the case and is stopped when the results stop time, like RunFloor089.)
 IEnumerator Smash090(string vehicle,bool real){
  var e=CampaignData.Find("c3-fence-smash");if(!real){flow.Save.Settings.unlockEverything=true;Campaign.Testing=true;flow.Save.SaveSettings();}
  flow.StartCampaignEvent(e,vehicle);yield return EventRunning089();var start=race.vehicle.Body.position;var site=CampaignRun.Site(race);
  var broken=new HashSet<BreakableProp>();bool done=false;var c=StartCoroutine(Wrap089(SmashDrive090(site,e.TimeLimit+5,broken),()=>done=true));float r0=Time.realtimeSinceStartup;
  while(!done&&flow.State!=RaceFlow.Stage.Results&&Time.realtimeSinceStartup-r0<200)yield return null;
  if(!done){StopCoroutine(c);var car=race.vehicle;car.enabled=true;car.GetComponent<VehicleInput>().enabled=true;}
  yield return Results089(15);
  var o=CampaignRun.Last;Note($"smash {vehicle}: start {V(start)}, site props {site.props.Length}, result {(o==null?"none":o.Headline)}, smashed by script {broken.Count}");
  yield return Page089("C-results-smash");flow.QuitRace();yield return null;
  if(!real){flow.Save.Settings.unlockEverything=false;Campaign.Testing=false;flow.Save.SaveSettings();}}
 IEnumerator SmashDrive090(ActivitySite site,float seconds,HashSet<BreakableProp> broken){
  var car=race.vehicle;car.GetComponent<VehicleInput>().enabled=false;car.enabled=false;float t0=Time.time;BreakableProp target=null;float targetSince=0;
  while(flow.State==RaceFlow.Stage.Racing&&!CampaignRun.Done&&Time.time-t0<seconds){AudioListener.volume=0;var p=car.Body.position;
   if(!target||target.IsBroken||Time.time-targetSince>8){if(target&&target.IsBroken)broken.Add(target);target=site.props.Where(x=>x&&!x.IsBroken&&!broken.Contains(x)).OrderBy(x=>Vector3.Distance(x.transform.position,p)).FirstOrDefault();targetSince=Time.time;if(target==null)break;}
   var to=target.transform.position-p;to.y=0;float ang=Vector3.SignedAngle(Vector3.ProjectOnPlane(car.transform.forward,Vector3.up),to,Vector3.up);
   car.Simulate(Mathf.Abs(ang)>70?.4f:1,0,Mathf.Clamp(ang/25f,-1,1),Time.fixedDeltaTime);yield return new WaitForFixedUpdate();}
  car.enabled=true;car.GetComponent<VehicleInput>().enabled=true;}

 // Jump targets: the event's run-up driven along the course road holding a set speed (a clean landing needs the
 // right speed, not full throttle), one run per speed; the scored distance or why it was not scored.
 IEnumerator Jumps090(string id,string vehicle,string speeds){
  var e=CampaignData.Find(id);var rows=new List<string>();flow.Save.Settings.unlockEverything=true;Campaign.Testing=true;flow.Save.SaveSettings();
  foreach(var v in speeds.Split(',').Select(F)){
   flow.StartCampaignEvent(e,vehicle);yield return EventRunning089();
   bool done=false;var c=StartCoroutine(Wrap089(Hold090(v,e.TimeLimit+2),()=>done=true));float r0=Time.realtimeSinceStartup;
   while(!done&&flow.State!=RaceFlow.Stage.Results&&Time.realtimeSinceStartup-r0<e.TimeLimit*3+30)yield return null;
   if(!done){StopCoroutine(c);race.vehicle.enabled=true;race.vehicle.GetComponent<VehicleInput>().enabled=true;}
   yield return Results089(10);var o=CampaignRun.Last;
   string line=$"{id} {vehicle} at {v:F0} m/s: {(o==null?"none":o.Dnf?"no score":Campaign.Measure(e,o.Score))}; last jump: {flow.Activities.LastJumpDiagnostic}";rows.Add(line);Note(line);
   flow.QuitRace();yield return null;}
  flow.Save.Settings.unlockEverything=false;Campaign.Testing=false;flow.Save.SaveSettings();File.AppendAllLines(output+"/campaign-targets.txt",rows);}
 IEnumerator Hold090(float speed,float seconds){var car=race.vehicle;car.GetComponent<VehicleInput>().enabled=false;car.enabled=false;float t0=Time.time;
  while(Time.time-t0<seconds&&flow.State==RaceFlow.Stage.Racing&&!CampaignRun.Done){AudioListener.volume=0;var p=car.Body.position;float s=race.road.Project(p,out _);var to=race.road.At(s+15,out _)-p;to.y=0;
   float ang=Vector3.SignedAngle(Vector3.ProjectOnPlane(car.transform.forward,Vector3.up),to,Vector3.up);float v=car.ForwardSpeed;
   car.Simulate(v<speed?1:0,v>speed+2?.4f:0,car.GroundedWheels>=2?Mathf.Clamp(ang/25f,-1,1):0,Time.fixedDeltaTime);yield return new WaitForFixedUpdate();}
  car.enabled=true;car.GetComponent<VehicleInput>().enabled=true;}

 // Every activity site against each course's road: station, lateral distance, and whether its forward runs with the
 // course (the campaign's trap / jump events start a run-up before the site on the course road).
 IEnumerator Sites090(string scenes){
  var sb=new StringBuilder();
  foreach(var scene in scenes.Split(',')){yield return Load(scene);yield return Menu();
   sb.AppendLine($"== {scene} ({race.courseName}) road length {race.road.Length:F0}");
   foreach(var s in FindObjectsByType<ActivitySite>(FindObjectsSortMode.None).OrderBy(x=>x.id)){
    float st=race.road.Project(s.transform.position,out float lat);race.road.At(st,out var f);float dot=Vector3.Dot(Vector3.ProjectOnPlane(f,Vector3.up).normalized,Vector3.ProjectOnPlane(s.forward,Vector3.up).normalized);
    s.Targets("atv",out float b,out float sv,out float g);
    sb.AppendLine($"  {s.id} [{s.kind}] '{s.title}' at {V(s.transform.position)} r {s.radius} station {st:F0} lateral {lat:F1} forward·road {dot:F2} both {s.bothDirections} targets(atv) {b:F1}/{sv:F1}/{g:F1}");}
   File.WriteAllText(output+"/sites.txt",sb.ToString());}
  Note(sb.ToString());}

 // Measured targets for chapters 2-4 through the real event flow with Testing on (nothing saved): a time trial's flying
 // lap by the race AI at Normal and Hard; a trap or jump at full throttle along the course road from the event start.
 IEnumerator Targets090(string events,string vehicles){
  var rows=new List<string>();flow.Save.Settings.unlockEverything=true;Campaign.Testing=true;flow.Save.SaveSettings();
  foreach(var id in events.Split(',')){var e=CampaignData.Find(id);if(e==null){Note("no event "+id);continue;}
   foreach(var v in vehicles.Split(',')){
    if(e.Kind==CampaignEventKind.TimeTrial){
     foreach(int skill in new[]{1,2}){
      flow.StartCampaignEvent(e,v);yield return EventRunning089();var pilot=Pilot089(skill);yield return Results089(600,3);Unpilot089(pilot);
      var t=CampaignRun.Last;string line=$"{e.Id} {v} AI skill {skill}: {(t==null||t.Dnf?"DNF":RaceHud.FormatTime(t.Time)+$" ({t.Time:F2} s)")}, missed gates {race.Progress.MissedGates}";rows.Add(line);Note(line);
      flow.QuitRace();yield return null;}}
    else{
     flow.StartCampaignEvent(e,v);yield return EventRunning089();var start=race.vehicle.Body.position;
     yield return RunFloor089(e.TimeLimit+2);yield return Results089(10);
     var o=CampaignRun.Last;string line=$"{e.Id} {v}: start {V(start)}, result {(o==null?"none":o.Dnf?"no score":Campaign.Measure(e,o.Score)+$" ({o.Score:F2})")}, medal {o?.Medal}";rows.Add(line);Note(line);
     flow.QuitRace();yield return null;}}}
  flow.Save.Settings.unlockEverything=false;Campaign.Testing=false;flow.Save.SaveSettings();
  File.AppendAllLines(output+"/campaign-targets.txt",rows);}
}
}
#endif
