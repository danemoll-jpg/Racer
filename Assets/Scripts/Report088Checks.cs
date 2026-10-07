#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Racer {
// 0.88 targeted checks, added to the 0.80 runner (same muted isolated save, same helpers): PROBE_CASES="case:args;...".
public sealed partial class Report080Checks {
 IEnumerator Cases088(string[] a)=>a[0] switch{"kyleviews88"=>KyleViews088(a.Length>1?a[1]:"day",a.Length>2?a[2]:""),"kyledrive88"=>KyleDrive088(a[1]),"carshots88"=>CarShots088(a[1],a.Length>2?a[2]:""),"mower88"=>Mower088(),_=>null};

 // Part D on the isolated save: the acorn save written with 23 of the 24 found; in Free Roam the mower is locked (not
 // eligible, not selectable, shown locked in the garage with the count); the 24th acorn is driven through: the celebration
 // and the unlock; the garage then selects it (stat bars rescaled); Restart Acorn Hunt keeps it; a fresh scene load
 // still has it (read from the acorn save). Leaves the isolated save with the mower earned for the later cases.
 IEnumerator Mower088(){
  var col0=FindAnyObjectByType<ExplorationCollection>();if(!col0){Check(false,"no acorn collection");yield break;}
  var ids=col0.sites.Select(x=>x.id).ToList();var last=ids[^1];
  File.WriteAllText(Path.Combine(saveDir,VehicleUnlocks.AcornFile),JsonUtility.ToJson(new ExplorationCollection.Save{found=ids.Take(ids.Count-1).ToList()},true));
  yield return EnterRoam("StreetLoopGreybox","original");yield return new WaitForSeconds(1);
  var col=FindAnyObjectByType<ExplorationCollection>();
  Check(!VehicleUnlocks.RewardEarned&&VehicleUnlocks.AcornsFound==ids.Count-1&&!race.EligibleVehicles.Any(p=>p.Id=="mower")&&race.EligibleVehicle("mower")!="mower",$"at {VehicleUnlocks.AcornsFound}/{ids.Count}: mower locked, not eligible for the player or the AI ({race.EligibleVehicles.Length} eligible); '{VehicleUnlocks.LockedText}'");
  var menus=FindAnyObjectByType<RaceMenus>();var lockField=typeof(RaceMenus).GetField("garageLocked",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance);
  flow.Pause();yield return null;flow.OpenGarage();yield return null;flow.SelectVehicle("mower");yield return null;
  string afterTry=race.vehicle.GetComponent<VehicleConfiguration>().profileId;
  lockField.SetValue(menus,"mower");menus.Show();yield return new WaitForSecondsRealtime(.5f);yield return Late(()=>Shot("D-garage-locked"));
  var titleText=FindObjectsByType<UnityEngine.UI.Text>(FindObjectsSortMode.None).Select(t=>t.text).FirstOrDefault(t=>t.Contains("Turf Rocket / Mower"))??"";
  Check(afterTry=="original"&&titleText.Contains("locked"),$"garage at 23: choosing the mower leaves '{afterTry}'; the list shows '{titleText.Replace("\n"," ")}' as a locked silhouette");
  lockField.SetValue(menus,null);flow.CloseGarage();yield return null;flow.Resume();yield return new WaitForSeconds(.5f);
  // the 24th acorn: carry the vehicle through it in 1 m steps
  var site=col.sites.First(x=>x.id==last);var car=race.vehicle;var dir=Vector3.forward;
  for(int k=0;k<=12;k++){var p=site.position+dir*(6-k);car.Body.position=p+Vector3.up*.2f;car.Body.linearVelocity=-dir*20;yield return new WaitForFixedUpdate();}
  yield return new WaitForSeconds(.3f);var hud=col.Hud;yield return Late(()=>Shot("D-celebration"));
  Check(VehicleUnlocks.RewardEarned&&hud.Contains("Turf Rocket"),$"24th acorn ({site.title}): unlocked {VehicleUnlocks.RewardEarned}, message '{hud.Replace("\n"," / ")}'");
  flow.Pause();yield return null;flow.OpenGarage();yield return null;flow.SelectVehicle("mower");yield return new WaitForSecondsRealtime(.8f);
  bool chosen=race.vehicle.GetComponent<VehicleConfiguration>().profileId=="mower";var mower=VehicleProfile.Find("mower");
  string bars=string.Join(", ",Enumerable.Range(0,5).Select(i=>$"{RaceMenus.StatFraction(i,mower):F2}"));yield return Late(()=>Shot("D-garage-unlocked"));
  Check(chosen&&RaceMenus.StatFraction(0,mower)>.99f&&RaceMenus.StatFraction(1,mower)>.99f&&RaceMenus.StatFraction(2,mower)>.99f&&RaceMenus.StatFraction(3,mower)>.99f,$"garage after the unlock: mower selected {chosen}; bars (speed, accel, grip, handling, weight) {bars}");
  flow.CloseGarage();yield return null;
  bool restarted=col.RestartCollection(true);var saved=JsonUtility.FromJson<ExplorationCollection.Save>(File.ReadAllText(Path.Combine(saveDir,VehicleUnlocks.AcornFile)));
  Check(restarted&&saved.found.Count==0&&saved.rewardEarned&&VehicleUnlocks.RewardEarned,$"Restart Acorn Hunt: found {saved.found.Count}, reward kept {saved.rewardEarned}");
  yield return EnterRoam("StreetLoopGreybox","mower");yield return new WaitForSeconds(1);
  Check(VehicleUnlocks.RewardEarned&&race.vehicle.GetComponent<VehicleConfiguration>().profileId=="mower",$"fresh load after the restart: still unlocked {VehicleUnlocks.RewardEarned}, driving '{race.vehicle.GetComponent<VehicleConfiguration>().profileId}'");}

 // Part B: each car parked at Dan's BUG-004 spot (Free Roam), the camera orbiting it (8 views at 7 m, 2.6 m up) and one
 // from above and behind (as Dan's chase view), by day and at night with the headlights on. carshots88:profiles:tag
 IEnumerator CarShots088(string profiles,string tag){foreach(var profile in profiles.Split(',')){yield return EnterRoam("StreetLoopGreybox",profile);yield return new WaitForSeconds(1);
   var car=race.vehicle;var spot=new Vector3(320.72f,0,471.31f);spot.y=Ground085(spot,80);var rot=Quaternion.Euler(0,205.67f,0);
   car.GetComponent<VehicleInput>().enabled=false;car.Body.position=spot+Vector3.up*(car.suspensionLength*.7f);car.Body.rotation=rot;car.transform.SetPositionAndRotation(car.Body.position,rot);car.Body.linearVelocity=Vector3.zero;car.Body.angularVelocity=Vector3.zero;
   for(int i=0;i<90;i++){car.Simulate(0,1,0,Time.fixedDeltaTime);yield return new WaitForFixedUpdate();}car.Body.isKinematic=true;
   var cam=Camera.main;var chase=FindAnyObjectByType<ChaseCamera>();if(chase)chase.enabled=false;var cv=CameraViews.Current;if(cv)cv.enabled=false;var hud=FindObjectsByType<Canvas>(FindObjectsSortMode.None);foreach(var c in hud)c.enabled=false;
   foreach(var when in new[]{"day","night"}){if(when=="night"){if(WorldLook.Current)WorldLook.Current.Pin(LookPresets.Compose(TimeOfDay.Night,Weather.Clear));}else Day();yield return new WaitForSeconds(.6f);
    var c0=car.Body.position+Vector3.up*.3f;var f=Vector3.ProjectOnPlane(car.Body.rotation*Vector3.forward,Vector3.up).normalized;
    var views=new List<(string n,Vector3 eye)>();for(int k=0;k<8;k++){var d=Quaternion.Euler(0,k*45,0)*(-f);views.Add(($"orbit{k}",c0+d*7+Vector3.up*2.1f));}
    views.Add(("above-behind",c0-f*5.2f+Vector3.up*4.4f));
    foreach(var v in views)yield return Late(()=>{cam.transform.position=v.eye;cam.transform.LookAt(c0);Shot($"B-{profile}-{when}-{v.n}{(tag==""?"":"-"+tag)}",960,540);});}
   foreach(var c in hud)if(c)c.enabled=true;if(chase)chase.enabled=true;if(cv)cv.enabled=true;car.Body.isKinematic=false;car.GetComponent<VehicleInput>().enabled=true;Note($"{profile}: 18 views ({tag})");}}

 // Part A: Kyle's driveway in Free Roam, both ways, following its route points: from the street 6 m before the mouth down to
 // the garage apron (stops there), then from the apron back up and 8 m out onto the street. Up to 9 m/s (4 on the apron),
 // what it touched, the steepest pitch and roll, resets. kyledrive88:profiles
 IEnumerator KyleDrive088(string profiles){foreach(var profile in profiles.Split(',')){
  foreach(var leg in new[]{"down","up"}){yield return EnterRoam("StreetLoopGreybox",profile);yield return new WaitForSeconds(1);
   var road=GameObject.Find("Kyle descending driveway")?.GetComponent<RaceRoad>();if(!road){Note("no Kyle driveway");yield break;}
   var pts=road.points.ToList();var out0=pts[0]-(pts[1]-pts[0]).normalized*6;out0.y=pts[0].y;pts.Insert(0,out0);if(leg=="up"){pts.Reverse();var o=pts[^1]+(pts[^1]-pts[^2]).normalized*8;pts.Add(o);}
   var path=new Path087(pts);var car=race.vehicle;
   car.GetComponent<VehicleInput>().enabled=false;car.enabled=false;var resp=car.GetComponent<VehicleRespawn>();int resets=0;Action onR=()=>resets++;resp.Respawned+=onR;var touch=car.gameObject.AddComponent<Touch087>();
   var start=path.At(.5f);var f0=path.At(3)-start;f0.y=0;start.y=Ground085(start,start.y+5);var rot=Quaternion.LookRotation(f0.normalized);car.Body.position=start+Vector3.up*(car.suspensionLength*.7f);car.Body.rotation=rot;car.transform.SetPositionAndRotation(car.Body.position,rot);car.Body.linearVelocity=Vector3.zero;car.Body.angularVelocity=Vector3.zero;car.ClearSteering();resp.SeedCoursePosition(start);
   for(int i=0;i<25;i++){car.Simulate(0,1,0,Time.fixedDeltaTime);yield return new WaitForFixedUpdate();}
   float t0=Time.time,d=0,maxPitch=0,maxRoll=0,maxLat=0;bool reached=false;
   while(Time.time-t0<45&&resets==0){AudioListener.volume=0;var p=car.Body.position;d=path.Project(p,d,20,out float lat);maxLat=Mathf.Max(maxLat,Mathf.Abs(lat));if(d>=path.Length-1.5f){reached=true;break;}
    var tgt=path.At(d+5);var to=tgt-p;to.y=0;float ang=Vector3.SignedAngle(Vector3.ProjectOnPlane(car.transform.forward,Vector3.up),to,Vector3.up);float v=car.ForwardSpeed;float lim=leg=="down"&&d>path.Length-12?4:9;
    car.Simulate(v<lim?.7f:0,v>lim+1?.6f:0,Mathf.Clamp(ang/25f,-1,1),Time.fixedDeltaTime);yield return new WaitForFixedUpdate();maxPitch=Mathf.Max(maxPitch,Vector3.Angle(car.transform.forward,Vector3.ProjectOnPlane(car.transform.forward,Vector3.up)));maxRoll=Mathf.Max(maxRoll,Vector3.Angle(car.transform.right,Vector3.ProjectOnPlane(car.transform.right,Vector3.up)));}
   for(int i=0;i<60;i++){car.Simulate(0,1,0,Time.fixedDeltaTime);yield return new WaitForFixedUpdate();}
   var site=GameObject.Find("Friend across street - blue circle").transform;var l=site.InverseTransformPoint(car.Body.position);
   resp.Respawned-=onR;Destroy(touch);car.enabled=true;car.GetComponent<VehicleInput>().enabled=true;
   Check(reached&&resets==0&&touch.hits.Count==0,$"{profile} {(leg=="down"?"street -> down Kyle's driveway -> garage apron":"garage apron -> up the driveway -> out onto the street")} ({path.Length:F0} m): {(reached?$"through in {Time.time-t0:F1} s, ended at house-frame {V(l)}":"NOT through")}, resets {resets}, steepest pitch {maxPitch:F0} deg, most roll {maxRoll:F0} deg, widest {maxLat:F1} m off the line, touched {(touch.hits.Count==0?"nothing":string.Join("; ",touch.hits.Take(3)))}");}}}

 // Part A views in Free Roam: Dan's three report positions (eye 1.3 m over the vehicle position, his heading, 6 deg down),
 // one from above the property, one of the mouth of the drive with the mailbox. kyleviews88:day|night:tag
 IEnumerator KyleViews088(string when,string tag){yield return EnterRoam("StreetLoopGreybox","moto");yield return new WaitForSeconds(1.5f);
  if(when=="night"){if(WorldLook.Current)WorldLook.Current.Pin(LookPresets.Compose(TimeOfDay.Night,Weather.Clear));}else Day();
  var site=GameObject.Find("Friend across street - blue circle")?.transform;if(!site){Note("no Kyle's house");yield break;}
  var cam=Camera.main;var chase=FindAnyObjectByType<ChaseCamera>();if(chase)chase.enabled=false;var cv=CameraViews.Current;if(cv)cv.enabled=false;var hud=FindObjectsByType<Canvas>(FindObjectsSortMode.None);foreach(var c in hud)c.enabled=false;
  if(race&&race.vehicle)race.vehicle.gameObject.SetActive(false);yield return new WaitForSeconds(.5f);
  var views=new List<(string n,Vector3 eye,Quaternion rot)>{("A-dan-BUG-001",new(471.98f,83.3f,-.56f),Quaternion.Euler(6,104.35f,0)),("A-dan-BUG-002",new(477.65f,87.2f,-21.06f),Quaternion.Euler(6,80.19f,0)),("A-dan-BUG-003",new(492.65f,82.4f,-17.57f),Quaternion.Euler(6,154.4f,0))};
  {var c=site.TransformPoint(new Vector3(12,0,14));views.Add(("A-above",c+new Vector3(-30,48,10),Quaternion.LookRotation(c-(c+new Vector3(-30,48,10)))));}
  {var c=site.TransformPoint(new Vector3(12,0,14));views.Add(("A-straight-down",c+Vector3.up*70,Quaternion.Euler(90,site.eulerAngles.y+180,0)));}
  var mb=GameObject.Find("Mailbox - Kyle")?.transform;if(mb){var e=mb.position+mb.forward*-0+new Vector3(0,1.6f,0)+Quaternion.Euler(0,mb.eulerAngles.y+35,0)*Vector3.forward*5.5f;views.Add(("A-mailbox",e,Quaternion.LookRotation(mb.position+Vector3.up*1.0f-e)));}
  foreach(var v in views)yield return Late(()=>{cam.transform.SetPositionAndRotation(v.eye,v.rot);Shot(v.n+(tag==""?"":"-"+tag)+(when=="night"?"-night":""));});
  if(race&&race.vehicle)race.vehicle.gameObject.SetActive(true);foreach(var c in hud)if(c)c.enabled=true;if(chase)chase.enabled=true;if(cv)cv.enabled=true;Note($"Kyle views ({when}): {string.Join(", ",views.Select(v=>v.n))}");}
}
}
#endif
