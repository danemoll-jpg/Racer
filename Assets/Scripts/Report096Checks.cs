#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.EventSystems;

namespace Racer {
// 0.96 targeted checks, added to the 0.80 runner (muted; the editor starts on a copy of Dan's save; menus by emulated controller):
//  events96   a Jump, a Speed Trap and a Time Trial campaign event: the event panel before, the attempt banner, the complete line, the results
//  screens96  the medal displays: campaign screen and event page, results, Free Roam prompt, Activities menu, Records
//  chapters96 each chapter's new order; a fresh campaign's first events; Dan's copy and an old save in progress keep what they had open
public sealed partial class Report080Checks {
 IEnumerator Run096(string[] a)=>a[0] switch{"events96"=>Events096(),"screens96"=>Screens096(),"chapters96"=>Chapters096(),"practice96"=>Practice096(a[1],a[2]),"look96"=>Look096(a[1],a[2],a.Length>3?a[3]:"day"),"culprit96"=>Culprit096(a[1],a[2]),"aijump96"=>AiJump096(a[1],int.Parse(a[2]),int.Parse(a[3])),_=>Run095(a)};
 string Sel096=>EventSystem.current&&EventSystem.current.currentSelectedGameObject?EventSystem.current.currentSelectedGameObject.name:"";
 IEnumerator Banner096(float seconds){float t0=Time.realtimeSinceStartup;while(Time.realtimeSinceStartup-t0<seconds&&(CampaignEventUi.Current==null||CampaignEventUi.Current.BannerShown==""))yield return null;}
 IEnumerator Chapters096(){
  yield return DanCopy095();
  foreach(var c in CampaignData.Chapters){Note($"Chapter {c.Number} {c.Name}:");int i=0;foreach(var e in CampaignData.InChapter(c.Number))Note($"  {++i}. {e.Name} [{e.Kind}] {e.CourseTitle}{(e.Kind==CampaignEventKind.TimeTrial?" targets "+string.Join("/",e.Targets.Reverse().Select(t=>MedalUi.Time(t))):"")} pay {e.Pay}");
   foreach(int course in c.Courses){var list=CampaignData.InChapter(c.Number);int firstRace=Array.FindIndex(list,e=>e.Kind==CampaignEventKind.Race&&e.Course==course),firstTt=Array.FindIndex(list,e=>e.Kind==CampaignEventKind.TimeTrial&&e.Course==course);
    Check(firstTt>=0&&firstTt<firstRace,$"chapter {c.Number}: a time trial (#{firstTt+1}) before the first race (#{firstRace+1}) on course {course} {RacePlaylists.Titles[course]}");}}
  int open=CampaignData.Events.Count(Campaign.Available);Check(open==CampaignData.Events.Length,$"Dan's copy: {open} of {CampaignData.Events.Length} events open (chapter {Campaign.Current.chapter}, legacy order {Campaign.Current.legacyOrder})");
  var fresh=Path.Combine(saveDir,"fresh96");Directory.CreateDirectory(fresh);Campaign.Load(fresh);
  var openFresh=CampaignData.Events.Where(Campaign.Available).Select(e=>e.Name).ToList();Check(openFresh.Count==1&&openFresh[0]=="Against the Clock",$"fresh campaign: open at the start: [{string.Join(", ",openFresh)}]");
  var next=Campaign.Next;Check(next!=null&&next.Kind==CampaignEventKind.TimeTrial,$"fresh campaign: Continue goes to \"{next?.Name}\" (a lap first); \"First Lap\" is locked: \"{Campaign.EventLock(CampaignData.Find("c1-first-lap"))}\"");
  var old=Path.Combine(saveDir,"old96");Directory.CreateDirectory(old);
  var st=new Campaign.State{chapter=1,owned={"original","atv"},courses={RacePlaylists.Scenes[0]},orderVersion=0};
  foreach(var id in new[]{"c1-first-lap","c1-hwy92-trap"})st.results.Add(new Campaign.Result{id=id,runs=1,passed=true,bestPlace=1});
  File.WriteAllText(Path.Combine(old,Campaign.File),JsonUtility.ToJson(st));Campaign.Load(old);
  bool clock=Campaign.Available(CampaignData.Find("c1-against-clock")),night=Campaign.Available(CampaignData.Find("c1-night-shift")),lap=Campaign.Available(CampaignData.Find("c1-first-lap")),trap=Campaign.Available(CampaignData.Find("c1-hwy92-trap"));
  Check(Campaign.Current.legacyOrder&&clock&&lap&&trap&&night,$"old save (First Lap and the trap passed, 0.95): First Lap open {lap}, trap open {trap}, Against the Clock open {clock}, Night Shift open {night} (it needed Against the Clock passed under the old order too)");
  foreach(var id in new[]{"c1-against-clock","c1-night-shift"})st.results.Add(new Campaign.Result{id=id,runs=1,passed=true,bestMedal=1,bestPlace=2});File.WriteAllText(Path.Combine(old,Campaign.File),JsonUtility.ToJson(st));Campaign.Load(old);
  Check(Campaign.Available(CampaignData.Find("c1-wrong-way"))&&Campaign.Available(CampaignData.Find("c1-practice-reverse")),$"old save with the clock passed: Night Shift open {Campaign.Available(CampaignData.Find("c1-night-shift"))}, the new practice lap open {Campaign.Available(CampaignData.Find("c1-practice-reverse"))}, Wrong Way Round open {Campaign.Available(CampaignData.Find("c1-wrong-way"))} (not required)");
  Campaign.Load(saveDir);}
 // camera views through the campaign path (the plain scene loader stops on the Mountain scenes): look96:eventId:name,x,y,z,yaw,pitch|...:day
 IEnumerator Look096(string eventId,string specs,string light){
  Campaign.Testing=true;flow.Save.Settings.unlockEverything=true;var e=CampaignData.Find(eventId);flow.StartCampaignEvent(e,"atv");yield return EventRunning089();yield return new WaitForSeconds(1);
  WorldLook.Current?.Pin(LookPresets.Compose(light=="night"?TimeOfDay.Night:TimeOfDay.Day,Weather.Clear));
  var cam=Camera.main;var chase=FindAnyObjectByType<ChaseCamera>();if(chase)chase.enabled=false;var cv=CameraViews.Current;if(cv)cv.enabled=false;
  var hud=FindObjectsByType<Canvas>(FindObjectsSortMode.None);foreach(var c in hud)c.enabled=false;
  foreach(var spec in specs.Split('|')){var f=spec.Split(',');var inv=System.Globalization.CultureInfo.InvariantCulture;string name=f[0];var p=new Vector3(float.Parse(f[1],inv),float.Parse(f[2],inv),float.Parse(f[3],inv));float yaw=float.Parse(f[4],inv),pitch=float.Parse(f[5],inv);
   yield return Late(()=>{cam.transform.SetPositionAndRotation(p,Quaternion.Euler(pitch,yaw,0));Shot($"look-{name}-{light}");});}
  Note($"{Scene}: views written");Campaign.Testing=false;}
 // which runtime renderer draws the white slivers: look from a camera at a point, switch off each renderer near it in turn and count the pale pixels
 // culprit96:camx,camy,camz,yaw,pitch:px,py,pz,radius
 int White096(string file){var bytes=File.ReadAllBytes(file);var t=new Texture2D(2,2);ImageConversion.LoadImage(t,bytes);var px=t.GetPixels32();int n=0;bool dark=Environment.GetEnvironmentVariable("CULPRIT_MODE")=="dark";foreach(var c in px){if(dark){if(c.r<55&&c.g<65&&c.b<65)n++;}else if(Mathf.Abs(c.r-168)<10&&Mathf.Abs(c.g-182)<10&&Mathf.Abs(c.b-183)<10)n++;}Destroy(t);return n;}
 IEnumerator Culprit096(string cam,string target){
  yield return EnterScene(RaceFlow.RoamScene);yield return new WaitForSeconds(1);WorldLook.Current?.Pin(LookPresets.Compose(TimeOfDay.Day,Weather.Clear));
  var f=cam.Split(',').Select(F).ToArray();var t=target.Split(',').Select(F).ToArray();var point=new Vector3(t[0],t[1],t[2]);float r=t[3];
  var c=Camera.main;var chase=FindAnyObjectByType<ChaseCamera>();if(chase)chase.enabled=false;var cv=CameraViews.Current;if(cv)cv.enabled=false;foreach(var cv2 in FindObjectsByType<Canvas>(FindObjectsSortMode.None))cv2.enabled=false;
  yield return Late(()=>{c.transform.SetPositionAndRotation(new Vector3(f[0],f[1],f[2]),Quaternion.Euler(f[4],f[3],0));});
  string baseFile=null;yield return Late(()=>baseFile=Shot("culprit-base",960,540));int baseline=White096(baseFile);Note("pale pixels with everything on: "+baseline);
  var list=FindObjectsByType<Renderer>(FindObjectsSortMode.None).Where(x=>x.enabled&&x.gameObject.activeInHierarchy&&x.bounds.SqrDistance(point)<r*r).ToList();Note(list.Count+" renderers within "+r+" m");
  var results=new List<(string,int)>();
  foreach(var rd in list){rd.enabled=false;string file=null;yield return Late(()=>file=Shot("culprit-try",960,540));int w=White096(file);rd.enabled=true;results.Add((P(rd.transform)+" ["+(rd.GetComponent<MeshFilter>()&&rd.GetComponent<MeshFilter>().sharedMesh?rd.GetComponent<MeshFilter>().sharedMesh.name:rd.GetType().Name)+"] "+Mat(rd),w));}
  foreach(var (n,w) in results.OrderBy(x=>x.Item2).Take(12))Note($"  off: {w} pale pixels ({w-baseline:+#;-#;0}) {n}");}
 IEnumerator Practice096(string ids,string vehicles){
  var rows=new List<string>();flow.Save.Settings.unlockEverything=true;Campaign.Testing=true;
  foreach(var id in ids.Split(',')){var e=CampaignData.Find(id);foreach(var v in vehicles.Split(',')){foreach(int skill in new[]{1,2}){flow.StartCampaignEvent(e,v);yield return EventRunning089();var pilot=Pilot089(skill);yield return Results089(400,3);Unpilot089(pilot);
     var t=CampaignRun.Last;string line=$"{id} {v} AI skill {skill}: {(t==null||t.Dnf?"DNF":RaceHud.FormatTime(t.Time))}";rows.Add(line);Note(line);flow.QuitRace();yield return null;}}}
  Campaign.Testing=false;File.AppendAllLines(output+"/practice-targets.txt",rows);}
 IEnumerator Events096(){
  yield return DanCopy095();Pads090();
  var jump=CampaignData.Find("c4-summit-flight");flow.StartCampaignEvent(jump,"atv");yield return EventRunning089();yield return new WaitForSeconds(.5f);
  Check(CampaignEventUi.Current&&CampaignEventUi.Current.PanelShown,$"jump event: the event panel shows: {CampaignEventUi.Current?.PanelText}");
  var hudText=string.Join(" | ",FindObjectsByType<UnityEngine.UI.Text>(FindObjectsSortMode.None).Where(t=>t.gameObject.activeInHierarchy&&t.text.StartsWith("LAP")).Select(t=>t.text));
  Check(hudText=="","no lap / position panel while the jump event runs (found: \""+hudText+"\")");
  yield return Late(()=>Shot4k("A-jump-panel-before"));
  bool done=false;var c1=StartCoroutine(Wrap089(Floor089(60),()=>done=true));yield return Banner096(70);
  string banner=CampaignEventUi.Current.BannerShown;Check(banner.Contains("ATTEMPT 1"),$"jump event: the attempt banner: \"{banner.Replace("\n"," / ")}\"");yield return Late(()=>Shot4k("A-jump-attempt-banner"));
  yield return new WaitForSeconds(3.6f);if(!done){StopCoroutine(c1);race.vehicle.enabled=true;race.vehicle.GetComponent<VehicleInput>().enabled=true;}
  Check(CampaignEventUi.Current.PanelText.Contains("COMPLETE")||CampaignRun.RunBestMedal==0,$"jump event: the panel after: {CampaignEventUi.Current.PanelText}");yield return Late(()=>Shot4k("A-jump-complete-line"));
  if(CampaignRun.RunBestMedal>=1){yield return Press091(GamepadButton.Start);Check(flow.State==RaceFlow.Stage.Paused&&Sel096=="end-event",$"START opens the pause menu on FINISH EVENT (focus {Sel096})");yield return Late(()=>Shot4k("A-jump-pause-finish"));
   yield return Press091(GamepadButton.A);yield return Settle091();Check(flow.State==RaceFlow.Stage.Results&&CampaignRun.Last!=null&&CampaignRun.Last.Passed,$"FINISH EVENT goes to the results: medal {CampaignRun.Last?.Medal}, earned {CampaignRun.Last?.Pay}");
   Check(FindObjectsByType<UnityEngine.UI.Image>(FindObjectsSortMode.None).Any(i=>i.name=="Medal"&&i.gameObject.activeInHierarchy),"results: the medal is drawn (no word)");yield return Late(()=>Shot4k("B-results-jump"));}
  flow.QuitRace();yield return null;
  var trap=CampaignData.Find("c1-hwy92-trap");flow.StartCampaignEvent(trap,"original");yield return EventRunning089();yield return new WaitForSeconds(.5f);
  Check(CampaignEventUi.Current.PanelShown,$"speed trap: the event panel: {CampaignEventUi.Current.PanelText}");yield return Late(()=>Shot4k("A-trap-panel-before"));
  done=false;c1=StartCoroutine(Wrap089(Floor089(60),()=>done=true));yield return Banner096(60);banner=CampaignEventUi.Current.BannerShown;
  Check(banner.Contains("ATTEMPT 1"),$"speed trap: the attempt banner: \"{banner.Replace("\n"," / ")}\"");yield return Late(()=>Shot4k("A-trap-attempt-banner"));
  float t0=Time.realtimeSinceStartup;while(flow.State!=RaceFlow.Stage.Results&&Time.realtimeSinceStartup-t0<20)yield return null;if(!done){StopCoroutine(c1);race.vehicle.enabled=true;race.vehicle.GetComponent<VehicleInput>().enabled=true;}
  yield return Settle091();Check(flow.State==RaceFlow.Stage.Results,"speed trap: then the results");flow.QuitRace();yield return null;
  var tt=CampaignData.Find("c1-against-clock");flow.StartCampaignEvent(tt,"original");yield return EventRunning089();yield return new WaitForSeconds(.5f);
  hudText=string.Join(" | ",FindObjectsByType<UnityEngine.UI.Text>(FindObjectsSortMode.None).Where(t=>t.gameObject.activeInHierarchy&&t.text.StartsWith("LAP")).Select(t=>t.text.Replace("\n"," / ")));
  Check(CampaignEventUi.Current.PanelShown&&hudText!="",$"time trial: its lap panel stays (\"{hudText}\") and the targets show: {CampaignEventUi.Current.PanelText}");yield return Late(()=>Shot4k("A-tt-panel-before"));
  var pilot=Pilot089(1);Time.timeScale=3;t0=Time.realtimeSinceStartup;while((CampaignEventUi.Current.BannerShown=="")&&Time.realtimeSinceStartup-t0<200){AudioListener.volume=0;yield return null;}Time.timeScale=1;
  banner=CampaignEventUi.Current.BannerShown;Check(banner.Contains("ATTEMPT 1"),$"time trial: the result at the line: \"{banner.Replace("\n"," / ")}\"");yield return Late(()=>Shot4k("A-tt-attempt-banner"));
  Unpilot089(pilot);yield return Results089(20,1);Check(flow.State==RaceFlow.Stage.Results,"time trial: then the results");yield return Late(()=>Shot4k("B-results-tt"));flow.QuitRace();yield return null;}
 IEnumerator Screens096(){
  yield return DanCopy095();Pads090();Menus089.ResetPages();Menus089.Show();yield return Settle091();
  yield return Choose091("campaign");Check(Page90=="campaign",$"campaign screen: {Page90}");
  Menus089.OpenCampaign(4);yield return Settle091();yield return Late(()=>Shot4k("B-campaign-chapter4"));
  yield return Goto091("cev-c4-summit-flight");yield return new WaitForSecondsRealtime(.4f);yield return Late(()=>Shot4k("B-campaign-medal-overlay"));
  Check(FindObjectsByType<UnityEngine.UI.Image>(FindObjectsSortMode.None).Count(i=>i.name=="Row medal"&&i.gameObject.activeInHierarchy)>=2,"the chapter list draws the medal won on each timed event");
  yield return Press091(GamepadButton.A);Check(Page90=="campaign-event",$"event page: {Page90}");yield return Late(()=>Shot4k("B-event-page"));
  Check(FindObjectsByType<UnityEngine.UI.Text>(FindObjectsSortMode.None).Where(t=>t.gameObject.activeInHierarchy).All(t=>!t.text.ToLower().Contains("bronze")&&!t.text.ToLower().Contains("silver")),"no medal word written on the event page: "+string.Join(" | ",FindObjectsByType<UnityEngine.UI.Text>(FindObjectsSortMode.None).Where(t=>t.gameObject.activeInHierarchy&&(t.text.ToLower().Contains("bronze")||t.text.ToLower().Contains("silver"))).Select(t=>t.text)));
  yield return Press091(GamepadButton.East);yield return Press091(GamepadButton.East);
  flow.StartFreeRoam();yield return WaitScene095(()=>Scene==RaceFlow.RoamScene&&flow.State==RaceFlow.Stage.Racing);
  var site=FindObjectsByType<ActivitySite>(FindObjectsSortMode.None).Where(s=>s.kind==ActivitySite.Kind.Jump).OrderBy(s=>s.id).First();
  var car=race.vehicle;var at=site.transform.position-site.forward.normalized*8+Vector3.up*1.5f;car.Body.position=at;car.Body.rotation=Quaternion.LookRotation(site.forward);car.transform.SetPositionAndRotation(at,car.Body.rotation);car.Body.linearVelocity=Vector3.zero;
  yield return new WaitForSeconds(1.5f);yield return Late(()=>Shot4k("B-roam-prompt"));
  flow.Pause();yield return Settle091();yield return Choose091("activities");yield return Late(()=>Shot4k("B-activities-menu"));
  yield return Press091(GamepadButton.East);yield return Press091(GamepadButton.East);
  flow.OpenBoards();yield return Settle091();SetField90("recordTab",3);Menus089.Show();yield return Settle091();yield return Late(()=>Shot4k("B-records-activity"));
  yield return Press091(GamepadButton.East);}
}
}
#endif
