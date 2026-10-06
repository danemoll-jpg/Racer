#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Racer {
// 0.84 targeted checks, added to the 0.80 runner (same muted isolated save, same helpers): PROBE_CASES="case:args;...".
public sealed partial class Report080Checks {
 IEnumerator Cases084(string[] a)=>a[0] switch{"creep"=>Creep(a[1],a[2]),"deadzone"=>DeadZone(),"startpress"=>StartPress(a[1],a[2]),"title84"=>Title084(F(a[1])),"lap84"=>Lap084(a[1],a[2]),"profile84"=>Profile084(a[1],a[2]),"jshots"=>JShots(a[1]),"floaters"=>Floaters(a[1],a[2]),"fpv"=>Fpv(a[1],a.Length>2?a[2]:"before"),"roamsurvey"=>RoamSurvey(),"gatelog"=>GateLog(a),"sites84"=>Sites084(a[1]),"cave84"=>Cave084(a[1],a[2]),"fence84"=>Fence084(),_=>null};

 // Part B: release everything, from a stop and from 20 mph, on the flattest and on the steepest ordinary stretch of the main
 // road (grade up to 15 %): the vehicle steps exactly as the player's own step does (Simulate(0,0,0)), once as before
 // (no hold) and once as now. creep:Scene:profile1,profile2,...
 IEnumerator Creep(string scene,string profiles){yield return Load(scene);yield return Menu();race.road.Initialize();var rd=race.road;
  float flat=-1,steep=-1,fg=99,sg=0;
  for(float s=10;s<rd.Length-10;s+=2){var a=rd.At(s-4,out _);var b=rd.At(s+4,out _);float g=Mathf.Abs(b.y-a.y)/Vector3.ProjectOnPlane(b-a,Vector3.up).magnitude;if(g<fg){fg=g;flat=s;}if(g>sg&&g<.15f){sg=g;steep=s;}}
  Note($"{scene}: flattest main-road station {flat:F0} (grade {fg*100:F1} %), steepest under 15 % {steep:F0} (grade {sg*100:F1} %) at {V(rd.At(steep,out _))}");
  foreach(var profile in profiles.Split(',')){
   flow.OpenGarage();flow.SelectVehicle(profile);flow.CloseGarage();race.opponents=false;race.traffic=false;race.laps=1;flow.StartRace();
   float tr=Time.realtimeSinceStartup;while(flow.State!=RaceFlow.Stage.Racing&&Time.realtimeSinceStartup-tr<40){AudioListener.volume=0;yield return null;}
   var car=race.vehicle;car.GetComponent<VehicleInput>().enabled=false;car.enabled=false;var resp=car.GetComponent<VehicleRespawn>();resp.enabled=false;
   foreach(var (where,s) in new[]{("flat",flat),("hill",steep)})foreach(float mph in new[]{0f,20f})foreach(bool hold in new[]{false,true}){
    // facing downhill on the hill (the harder case for holding)
    var p=rd.At(s,out var fw);fw.y=0;fw.Normalize();if(where=="hill"&&rd.At(s+4,out _).y>rd.At(s-4,out _).y)fw=-fw;
    Put(car,p+Vector3.up*Mathf.Max(.4f,car.suspensionLength-Physics.gravity.magnitude/car.springStrength),Quaternion.LookRotation(fw),Vector3.zero);
    car.ForceHoldAtRest=true;for(int i=0;i<60;i++){car.Simulate(0,1,0,Time.fixedDeltaTime);yield return new WaitForFixedUpdate();}
    car.Body.linearVelocity=car.transform.forward*mph*.44704f;car.ForceHoldAtRest=hold;
    float t0=Time.time,stopAt=-1;Vector3 at5=default;float top=0;
    while(Time.time-t0<25){car.Simulate(0,0,0,Time.fixedDeltaTime);yield return new WaitForFixedUpdate();float v=car.Body.linearVelocity.magnitude;
     if(stopAt<0&&v<.05f)stopAt=Time.time-t0;if(Time.time-t0>=20&&at5==default)at5=car.Body.position;if(Time.time-t0>=20)top=Mathf.Max(top,v);}
    float drift=Vector3.Distance(at5,car.Body.position);float endMph=car.Body.linearVelocity.magnitude/.44704f;
    bool ok=!hold||(endMph<.1f&&drift<.05f);
    Check(ok,$"{profile} {where} ({(where=="hill"?$"{sg*100:F1} % grade, facing downhill":"flat")}) from {mph:F0} mph, {(hold?"NOW":"before")}: {(stopAt<0?"never stopped":$"at rest after {stopAt:F1} s")}; after 25 s {endMph:F2} mph, moved {drift:F2} m in the last 5 s (fastest {top/.44704f:F2} mph)");}
   car.ForceHoldAtRest=false;resp.enabled=true;car.enabled=true;car.GetComponent<VehicleInput>().enabled=true;yield return Menu();}}

 // Part B: a pad whose triggers rest at 6-9 % reads no throttle and no brake; a full press still reads 1. deadzone
 IEnumerator DeadZone(){yield return EnterRoam("StreetLoopGreybox","moto");var input=race.vehicle.GetComponent<VehicleInput>();
  var d=InputSystem.AddDevice<Gamepad>();string Read(float rt,float lt){InputSystem.QueueStateEvent(d,new GamepadState{rightTrigger=rt,leftTrigger=lt});return null;}
  var rows=new List<string>();bool ok=true;
  foreach(var (rt,lt,wantT,wantB) in new[]{(.06f,.09f,0f,0f),(.09f,0f,0f,0f),(1f,0f,1f,0f),(0f,1f,0f,1f),(.55f,0f,.5f,0f)}){Read(rt,lt);yield return null;yield return null;
   ok&=Mathf.Abs(input.Throttle-wantT)<.01f&&Mathf.Abs(input.BrakeReverse-wantB)<.01f;rows.Add($"triggers {rt:F2}/{lt:F2} -> throttle {input.Throttle:F2}, brake {input.BrakeReverse:F2}");}
  InputSystem.RemoveDevice(d);Check(ok,"pad dead zone: "+string.Join("; ",rows));}

 // Part G: what pressing START RACE costs: the frame of the press and the next frames until the countdown runs, three
 // rivals and traffic as the menu has them. startpress:Scene:roster
 IEnumerator StartPress(string scene,string roster){yield return Load(scene);yield return Menu();var st=flow.Save.Settings;st.weather=0;st.timeOfDay=0;flow.Save.SaveSettings();
  for(int k=0;k<3;k++){
   flow.OpenGarage();flow.SelectVehicle("moto");flow.CloseGarage();race.opponents=true;race.traffic=true;race.laps=1;race.opponentRoster=roster.Split(',');
   yield return new WaitForSecondsRealtime(.5f);
   var sw=System.Diagnostics.Stopwatch.StartNew();flow.StartRace();sw.Stop();bool covered=LoadingScreen.Holding;var frames=new List<float>();float t0=Time.realtimeSinceStartup;
   yield return null;frames.Add(Time.unscaledDeltaTime*1000);if(k==0)yield return Late(()=>Shot("G-start-race-cover",1920,1080));
   while((LoadingScreen.Holding||flow.State!=RaceFlow.Stage.Countdown)&&Time.realtimeSinceStartup-t0<15){yield return null;frames.Add(Time.unscaledDeltaTime*1000);}
   float ready=Time.realtimeSinceStartup-t0;float cd=flow.CountdownRemaining;
   Check(sw.Elapsed.TotalMilliseconds<50&&covered,$"{scene} START RACE {k+1}: the press itself {sw.Elapsed.TotalMilliseconds:F0} ms, loading screen up at once {covered}; countdown running after {ready:F2} s with {cd:F1} s of it left; frames {string.Join(" ",frames.Select(f=>f.ToString("F0")))} ms; racers {race.Racers.Count}, drivers {race.Drivers.Count}");
   yield return Menu();}}

 // Part D: the title as at a cold start (artwork, then the voice), a button pressed `at` seconds after the voice starts
 // (0 = never): every frame until the speech has ended, whether an AudioListener was active, as before and as now.
 IEnumerator Title084(float at){yield return Load("StreetLoopGreybox");yield return Menu();
  foreach(bool keep in new[]{false,true}){
   if(Scene!="StreetLoopGreybox"){yield return Load("StreetLoopGreybox");yield return Menu();}
   StartupTitle.KeepListener=keep;typeof(StartupTitle).GetField("completed",BindingFlags.Static|BindingFlags.NonPublic).SetValue(null,false);
   StartupTitle.Begin(flow);var title=FindAnyObjectByType<StartupTitle>();float t0=Time.realtimeSinceStartup;
   while(title&&title.VoiceStarts==0&&Time.realtimeSinceStartup-t0<20)yield return null;
   double voiceAt=title.VoiceScheduledDsp;bool pressed=false;int frames=0,silent=0;double firstSilent=-1;
   while(title&&(StartupTitle.SpeechPending||title.Voice.isPlaying)&&Time.realtimeSinceStartup-t0<40){
    if(at>0&&!pressed&&AudioSettings.dspTime-voiceAt>=at){pressed=true;yield return PressPad(GamepadButton.South,3);continue;}
    yield return null;frames++;bool any=FindObjectsByType<AudioListener>(FindObjectsSortMode.None).Any(l=>l.isActiveAndEnabled);
    if(!any){silent++;if(firstSilent<0)firstSilent=AudioSettings.dspTime-voiceAt;}}
   double clip=title?title.Voice.clip.length:0;
   Check(!keep||silent==0,$"title voice ({clip:F2} s clip incl. 1 s lead silence), button {(at>0?$"pressed {at:F1} s after the voice started":"not pressed")}, {(keep?"NOW":"before")}: {frames} frames while speaking, {silent} with no active AudioListener{(silent>0?$" (from {firstSilent:F2} s into the clip)":"")}; title listener frames {(title?title.FramesOnFallback:-1)}; scene now {Scene}");
   StartupTitle.KeepListener=true;
   float t1=Time.realtimeSinceStartup;while(FindAnyObjectByType<StartupTitle>()&&Time.realtimeSinceStartup-t1<10)yield return null;
   Bind();if(Scene==RaceFlow.RoamScene){flow.Pause();yield return null;}}
  }

 // Part B: one Street Loop lap on the Needle 600 on the race autopilot, as before (no hold) and as now (the hold applied to
 // the autopilot's steps, as on the player's own): lap times compared. lap84:Scene:profile
 IEnumerator Lap084(string scene,string profile){foreach(bool hold in new[]{false,true}){
   yield return Load(scene);yield return Menu();var st=flow.Save.Settings;st.weather=0;st.timeOfDay=0;flow.Save.SaveSettings();
   flow.OpenGarage();flow.SelectVehicle(profile);flow.CloseGarage();race.opponents=false;race.traffic=false;race.laps=1;flow.StartRace();
   float tr=Time.realtimeSinceStartup;while(flow.State!=RaceFlow.Stage.Racing&&Time.realtimeSinceStartup-tr<40){AudioListener.volume=0;yield return null;}
   var car=race.vehicle;car.ForceHoldAtRest=hold;var pilot=car.gameObject.AddComponent<RoadDriver>();pilot.Initialize(race,car,true,1,1);pilot.Racer=race.Racers[0];car.GetComponent<VehicleInput>().enabled=false;
   float t1=Time.time;while(flow.State!=RaceFlow.Stage.Results&&Time.time-t1<400){AudioListener.volume=0;yield return null;}
   var r=race.Racers[0];Check(r.Progress.Finished,$"{scene} {profile} lap on the autopilot, {(hold?"NOW (hold applied)":"before")}: {(r.Progress.Finished?RaceHud.FormatTime(r.Progress.AdjustedTime(race.Clock)):"not finished")}, missed {r.Progress.MissedGates}, resets {r.Recoveries}");
   car.ForceHoldAtRest=false;Destroy(pilot);car.GetComponent<VehicleInput>().enabled=true;yield return Menu();}}

 // Part J step 1: the driving surface along the main and every branch of ForestLoopReverse within a station span: the
 // authored navigation height, the real surface at the centre and at both edges (what collider), grade, and the vertical
 // curvature over +-8 m (crest < 0, dip > 0); gates, jumps and branch mouths listed. profile84:tag:from-to
 static float Surface(Vector3 p,out string what){what="-";var hits=Physics.RaycastAll(new Vector3(p.x,p.y+40,p.z),Vector3.down,120,~0,QueryTriggerInteraction.Ignore).Where(h=>!h.collider.attachedRigidbody||h.collider.attachedRigidbody.isKinematic).Where(h=>!h.collider.GetComponentInParent<ArcadeVehicle>()).OrderBy(h=>Mathf.Abs(h.point.y-p.y)).ToArray();if(hits.Length==0)return float.NaN;what=hits[0].collider.name;return hits[0].point.y;}
 IEnumerator Profile084(string tag,string span){yield return EnterScene("ForestLoopReverse");var f=span.Split('-').Select(F).ToArray();var rd=race.road;rd.Initialize();
  var sb=new StringBuilder("route,s,x,z,navY,surfY,leftY,rightY,halfWidth,grade,curv,collider\n");
  void Line(string name,Func<float,(Vector3 p,Vector3 fw)> at,float hw,float a,float b){var ys=new List<(float s,Vector3 p,float y,float l,float r,string c)>();
   for(float s=a;s<=b;s+=1){var (p,fw)=at(s);fw.y=0;fw.Normalize();var right=Vector3.Cross(Vector3.up,fw);float y=Surface(p,out var c);float l=Surface(p-right*hw*.9f,out _),r=Surface(p+right*hw*.9f,out _);ys.Add((s,p,y,l,r,c));}
   for(int i=0;i<ys.Count;i++){var q=ys[i];float g=i>2&&i<ys.Count-3?(ys[i+3].y-ys[i-3].y)/6:float.NaN;float cv=i>=8&&i<ys.Count-8?(ys[i+8].y-2*q.y+ys[i-8].y)/64:float.NaN;
    sb.AppendLine($"{name},{q.s:F0},{q.p.x:F2},{q.p.z:F2},{q.p.y:F2},{q.y:F2},{q.l:F2},{q.r:F2},{hw:F2},{g:F3},{cv:F4},{q.c}");}}
  Line("Main",s=>(rd.At(s,out var fw),fw),rd.HalfWidth((f[0]+f[1])/2),f[0],f[1]);
  var notes=new StringBuilder($"main length {rd.Length:F1}; half-width at stations: {string.Join(", ",Enumerable.Range(0,8).Select(i=>f[0]+(f[1]-f[0])*i/7).Select(s=>$"{s:F0}:{rd.HalfWidth(s):F1}"))}\n");
  foreach(var wr in FindObjectsByType<WoodlandRoute>(FindObjectsSortMode.None)){wr.Initialize();bool near=wr.entryRoad>=f[0]-50&&wr.entryRoad<=f[1]+50||wr.exitRoad>=f[0]-50&&wr.exitRoad<=f[1]+50;
   notes.AppendLine($"branch \"{wr.title}\" ({wr.name}) entry {wr.entryRoad:F1} exit {wr.exitRoad:F1} length {wr.Length:F0} half-width {wr.halfWidth:F2} start {V(wr.points[0])} end {V(wr.points[^1])}{(near?" <- in span":"")}");
   if(near){var w=wr;Line(wr.title,s=>(w.At(s,out var fw),fw),wr.halfWidth,0,Mathf.Min(wr.Length,f[1]-f[0]+100));}}
  for(int i=0;i<race.gates.Length;i++){float s=rd.Project(race.gates[i].transform.position,out _);notes.AppendLine($"gate {i} {race.gates[i].name} at station {s:F1} {V(race.gates[i].transform.position)}");}
  var lay=FindAnyObjectByType<ForestLayout>();if(lay)for(int i=0;i<lay.jumpStarts.Length;i++)notes.AppendLine($"jump \"{lay.jumpNames[i]}\" {lay.jumpStarts[i]:F1} -> {lay.jumpEnds[i]:F1} start {V(rd.At(lay.jumpStarts[i],out _))} end {V(rd.At(lay.jumpEnds[i],out _))}");
  File.WriteAllText($"{output}/profile-{tag}.csv",sb.ToString());File.WriteAllText($"{output}/profile-{tag}-notes.txt",notes.ToString());Note($"profile {tag} written");yield return Menu();}
 // Part J: views of the reshaped section: Dan's BUG-001 position looking along the main, the main from 40 m before the L04
 // fork, and the hump from the side. jshots:tag
 IEnumerator JShots(string tag){yield return EnterScene("ForestLoopReverse");WorldLook.Current?.Pin(LookPresets.Compose(TimeOfDay.Day,Weather.Clear));var rd=race.road;
  var cam=Camera.main;var chase=FindAnyObjectByType<ChaseCamera>();if(chase)chase.enabled=false;var cv=CameraViews.Current;if(cv)cv.enabled=false;foreach(var c in FindObjectsByType<Canvas>(FindObjectsSortMode.None))c.enabled=false;
  Park(rd.At(900,out _));yield return new WaitForSeconds(1);
  void Eye(Vector3 from,Vector3 to){cam.transform.position=from;cam.transform.LookAt(to);}
  var dan=new Vector3(285.6f,45.5f,-196.7f);float sd=rd.Project(dan,out _);
  yield return Late(()=>{Eye(dan+Vector3.up*1.6f,rd.At(sd+45,out _)+Vector3.up*1.2f);Shot($"J-{tag}-dan-position",1920,1080);});
  yield return Late(()=>{var fwd=Quaternion.Euler(0,88.84f,0)*Vector3.forward;Eye(dan-fwd*6+Vector3.up*2.6f,dan+fwd*40+Vector3.up*3f);Shot($"J-{tag}-dan-heading",1920,1080);});
  yield return Late(()=>{var a=rd.At(139.4f-40,out _);Eye(a+Vector3.up*1.8f,rd.At(139.4f+25,out _)+Vector3.up*1f);Shot($"J-{tag}-fork",1920,1080);});
  foreach(float s in (Environment.GetEnvironmentVariable("J_STATIONS")??"").Split(',').Where(x=>x.Length>0).Select(F)){float ss=s;yield return Late(()=>{var a=rd.At(ss-30,out _);Eye(a+Vector3.up*1.8f,rd.At(ss+20,out _)+Vector3.up*1f);Shot($"J-{tag}-s{ss:F0}",1920,1080);});}
  if(chase)chase.enabled=true;if(cv)cv.enabled=true;yield return Menu();}

 // Part K: every tree the scenery placed (trunk or crown-only clump) and every trunk collider, against the ground under its
 // base (terrain, paving, rocks; not trees, not vehicles, not triggers): floating more than 0.3 m, or a trunk sunk deeper
 // than half its height. Listed with the nearest main-road station; those within r of x,z marked. floaters:Scene:x,z,r
 static bool TreeLike(Collider c)=>c.name.IndexOf("trunk",StringComparison.OrdinalIgnoreCase)>=0||c.name.IndexOf("tree",StringComparison.OrdinalIgnoreCase)>=0||c.GetComponentInParent<ArcadeVehicle>();
 static bool GroundUnder(Vector3 p,float from,out RaycastHit hit){hit=default;var hits=Physics.RaycastAll(new Vector3(p.x,from,p.z),Vector3.down,from-p.y+60,~0,QueryTriggerInteraction.Ignore).Where(h=>!TreeLike(h.collider)&&(!h.collider.attachedRigidbody||h.collider.attachedRigidbody.isKinematic)).OrderBy(h=>h.distance).ToArray();if(hits.Length==0)return false;hit=hits[0];return true;}
 IEnumerator Floaters(string scene,string spec){yield return EnterScene(scene);yield return new WaitForSeconds(1);var f=spec.Split(',').Select(F).ToArray();var c=new Vector3(f[0],0,f[1]);float r=f[2];
  var world=SceneryWorld.Current;var sb=new StringBuilder();int n=0,flo=0,near=0;race.road.Initialize();
  void Test(string what,Vector3 bottom,float height){n++;
   // the ground at the base: from just above the trunk's top (a tree standing in a cutting must not read the bank above)
   if(!GroundUnder(bottom,bottom.y+Mathf.Max(2,height*.5f),out var h))return;float gap=bottom.y-h.point.y;var d=bottom-c;d.y=0;bool close=d.magnitude<r;
   if(gap>.3f){flo++;if(close)near++;sb.AppendLine($"{(close?"NEAR ":"")}FLOATS {gap:F2} m: {what} base {V(bottom)} height {height:F1}, ground {h.point.y:F2} ({h.collider.name}), main station {race.road.Project(bottom,out float lat):F0} ({lat:F0} m off)");}}
  int added=0;if(world&&world.Trees)foreach(var t in world.Trees.Placements){if(t.bottom.y<-1000)continue;if(t.source.Contains("trunk added")){added++;continue;}Test("tree "+t.source,t.bottom,t.height);}
  foreach(var b in FindObjectsByType<BoxCollider>(FindObjectsSortMode.None))if(b.enabled&&b.name.IndexOf("trunk",StringComparison.OrdinalIgnoreCase)>=0){var tr=b.transform;Test("trunk collider "+P(tr),tr.TransformPoint(b.center-Vector3.up*b.size.y*.5f),b.bounds.size.y);}
  File.WriteAllText($"{output}/floaters-{Scene}.txt",sb.ToString());
  Check(near==0,$"{Scene}: crown-only clumps given a trunk {added}, visual-only trunks set down {world?.Trees?.BarkSeated}; {n} other trees and trunk colliders checked: {flo} float more than 0.3 m above the ground, {near} of them within {r} m of ({c.x}, {c.z}) (floaters-{Scene}.txt)");
  if(scene!=RaceFlow.RoamScene)yield return Menu();}

 // Part L: first person in each vehicle on a straight road (Street Loop): one shot each, and where the lowest edge of
 // anything above the view centre (the windscreen top / roof front, within +-12 degrees of straight ahead) sits relative to
 // the centre of the view, in degrees (the view is 2 x {fov/2} tall). fpv:profile1,profile2,...:tag
 IEnumerator Fpv(string profiles,string tag){yield return Load("StreetLoopGreybox");yield return Menu();var rd=race.road;rd.Initialize();
  float bestS=0,bestC=999;for(float s0=50;s0<rd.Length-50;s0+=5){rd.At(s0-40,out var f0);rd.At(s0+40,out var f1);float c=Vector3.Angle(f0,f1);if(c<bestC){bestC=c;bestS=s0;}}
  var st=flow.Save.Settings;st.weather=0;st.timeOfDay=0;st.cameraView=2;st.bodyColors=Enumerable.Repeat(-1,VehicleProfile.All.Length).ToArray();flow.Save.SaveSettings();
  foreach(var profile in profiles.Split(',')){
   flow.OpenGarage();flow.SelectVehicle(profile);flow.CloseGarage();race.opponents=false;race.traffic=false;race.laps=1;flow.StartRace();
   float tr=Time.realtimeSinceStartup;while(flow.State!=RaceFlow.Stage.Racing&&Time.realtimeSinceStartup-tr<40){AudioListener.volume=0;yield return null;}
   var car=race.vehicle;var p=rd.At(bestS,out var fw);fw.y=0;Put(car,p+Vector3.up*Mathf.Max(.4f,car.suspensionLength-Physics.gravity.magnitude/car.springStrength),Quaternion.LookRotation(fw),Vector3.zero);
   yield return new WaitForSeconds(2.5f);var cam=Camera.main;string line="";
   yield return Late(()=>{float lowest=99;string what="-";
    foreach(var mf in car.GetComponentsInChildren<MeshFilter>()){var r=mf.GetComponent<Renderer>();if(!r||!r.enabled||!mf.sharedMesh||!mf.sharedMesh.isReadable)continue;string n=mf.name;if(n.Contains("Rider")||n.Contains("_Base_")||n.Contains("_Hair")||n.Contains("_Hat_")||n.Contains("Shirt")||n.Contains("Emblem"))continue;
     foreach(var v in mf.sharedMesh.vertices){var q=cam.transform.InverseTransformPoint(mf.transform.TransformPoint(v));if(q.z<.05f)continue;float az=Mathf.Atan2(q.x,q.z)*Mathf.Rad2Deg,el=Mathf.Atan2(q.y,q.z)*Mathf.Rad2Deg;if(Mathf.Abs(az)>12||el<0)continue;if(el<lowest){lowest=el;what=n;}}}
    line=$"{profile}: view {cam.fieldOfView:F0} deg tall; lowest edge above the view centre within +-12 deg of straight ahead: {(lowest>90?"none":$"{lowest:F1} deg ({what})")}; top of the view at {cam.fieldOfView/2:F1} deg";
    Shot($"L-{tag}-{profile}",1920,1080);});
   Note(line);yield return Menu();}
  st.cameraView=0;flow.Save.SaveSettings();}

 // Part C: what FreeRoamWorld offers: every road / trail (name, length, forest trail or road, longest straight-ish run), the
 // launch surfaces (Takeoff - / Landing - / ramps), and the activity sites already there. roamsurvey
 IEnumerator RoamSurvey(){yield return EnterRoam("StreetLoopGreybox","moto");yield return new WaitForSeconds(1);var sb=new StringBuilder();
  foreach(var r in FindObjectsByType<RaceRoad>(FindObjectsSortMode.None)){if(r.points==null||r.points.Length<2)continue;r.Initialize();
   // longest run where the heading changes less than 8 degrees over 60 m and the grade stays under 8 %
   float best=0,bs=0,start=0;for(float s0=0;s0+60<r.Length;s0+=5){r.At(s0,out var f0);r.At(s0+60,out var f1);var a=r.At(s0,out _);var b=r.At(s0+60,out _);bool ok=Vector3.Angle(new Vector3(f0.x,0,f0.z),new Vector3(f1.x,0,f1.z))<8&&Mathf.Abs(b.y-a.y)/60<.08f;if(!ok){start=s0+5;continue;}if(s0+60-start>best){best=s0+60-start;bs=start;}}
   sb.AppendLine($"ROAD {P(r.transform)} len {r.Length:F0} trail {r.forestTrail} hw {r.HalfWidth(r.Length/2):F1} from {V(r.points[0])} to {V(r.points[^1])}; longest straight level run {best:F0} m from s {bs:F0} at {V(r.At(bs,out _))} to {V(r.At(bs+best,out _))}");}
  foreach(var w in FindObjectsByType<WoodlandRoute>(FindObjectsSortMode.None))sb.AppendLine($"BRANCH {w.title} len {w.Length:F0} from {V(w.points[0])} to {V(w.points[^1])}");
  foreach(var c in FindObjectsByType<Collider>(FindObjectsSortMode.None)){string n=c.name;if(n.StartsWith("Takeoff")||n.StartsWith("Landing")||n.IndexOf("ramp",StringComparison.OrdinalIgnoreCase)>=0||n.IndexOf("launch",StringComparison.OrdinalIgnoreCase)>=0||n.IndexOf("kicker",StringComparison.OrdinalIgnoreCase)>=0)sb.AppendLine($"LAUNCH {P(c.transform)} at {V(c.bounds.center)} size {V(c.bounds.size)}");}
  foreach(var a in FindObjectsByType<ActivitySite>(FindObjectsSortMode.None))sb.AppendLine($"SITE {a.id} \"{a.title}\" {a.kind} at {V(a.transform.position)} forward {V(a.forward)} radius {a.radius} targets {a.bronze}/{a.silver}/{a.gold}; children {string.Join(",",a.GetComponentsInChildren<Transform>().Skip(1).Select(t=>t.name).Distinct())}");
  foreach(var r in FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None)){if(r.name.IndexOf("fence",StringComparison.OrdinalIgnoreCase)<0&&!(r.transform.parent&&r.transform.parent.name.IndexOf("fence",StringComparison.OrdinalIgnoreCase)>=0))continue;var mf=r.GetComponent<MeshFilter>();var m=mf?mf.sharedMesh:null;
   sb.AppendLine($"FENCE {P(r.transform)} verts {(m?m.vertexCount:0)} mesh {(m?m.name:"-")} readable {(m&&m.isReadable)} mat {Mat(r)} colliders {string.Join(",",r.GetComponents<Collider>().Select(c=>c.GetType().Name))} comps {string.Join(",",r.GetComponents<Component>().Select(c=>c.GetType().Name))} center {V(r.bounds.center)} size {V(r.bounds.size)} on {r.enabled}");}
  File.WriteAllText($"{output}/roam-survey.txt",sb.ToString());Note("roam survey written");}

 // Part H: carrace with every racer's path recorded; for each missed gate, where that racer crossed the gate's plane (gate
 // space: x across, the opening is +-halfWidth + 0.2 m) or, if it never crossed it, how close it came and on which branch.
 // gatelog:Scene:player:ai1,ai2,ai3
 IEnumerator GateLog(string[] a){var paths=new Dictionary<RacerState,List<(Vector3 p,string branch)>>();bool hooked=false;
  IEnumerator Watch(){float t0=Time.realtimeSinceStartup;while(!hooked&&Time.realtimeSinceStartup-t0<120){Bind();if(race&&flow.State==RaceFlow.Stage.Racing&&race.Racers.Count>1){hooked=true;foreach(var r in race.Racers)paths[r]=new();StartCoroutine(Rec());}yield return null;}}
  IEnumerator Rec(){var racers=race.Racers.ToList();while(flow.State==RaceFlow.Stage.Racing||flow.State==RaceFlow.Stage.Countdown){foreach(var r in racers)if(r.Car&&!r.Progress.Finished)paths[r].Add((r.Car.Body.position,r.Branch.Route?r.Branch.Route.title:"main"));yield return new WaitForFixedUpdate();}}
  StartCoroutine(Watch());var gates=race?race.gates:null;yield return Case(new[]{"carrace",a[1],a[2],a[3]});
  {var csv=new StringBuilder("racer,i,x,y,z,branch\n");foreach(var kv in paths){int i=0;foreach(var q in kv.Value){if(i++%5==0)csv.AppendLine($"{kv.Key.Name},{i},{q.p.x:F1},{q.p.y:F1},{q.p.z:F1},{q.branch}");}}
   var rd=race.road;rd.Initialize();for(float s0=0;s0<rd.Length;s0+=2){var q=rd.At(s0,out _);csv.AppendLine($"MAIN,{s0:F0},{q.x:F1},{q.y:F1},{q.z:F1},{rd.HalfWidth(s0):F1}");}
   for(int gi=0;gi<race.gates.Length;gi++){var gt=race.gates[gi].transform;var gr=gt.right*race.gates[gi].GetComponent<RaceGate>().halfWidth;csv.AppendLine($"GATE,{gi},{gt.position.x+gr.x:F1},{gt.position.y:F1},{gt.position.z+gr.z:F1},{gt.position.x-gr.x:F1}/{gt.position.z-gr.z:F1}");}
   foreach(var b in FindObjectsByType<WoodlandRoute>(FindObjectsSortMode.None))foreach(var q in b.points)csv.AppendLine($"BRANCH,{b.title},{q.x:F1},{q.y:F1},{q.z:F1},");
   File.WriteAllText($"{output}/paths-{Scene}-{a[2]}-{RaceTable.Count}.csv",csv.ToString());}
  foreach(var kv in paths){var r=kv.Key;foreach(var e in r.Progress.Ledger){if(e.Checkpoint<=0)continue;var g=race.gates[e.Checkpoint].GetComponent<RaceGate>();var t=g.transform;var path=kv.Value;string where="never crossed its plane";float near=1e9f,bestX=60;
    for(int i=1;i<path.Count;i++){var q0=t.InverseTransformPoint(path[i-1].p);var q1=t.InverseTransformPoint(path[i].p);near=Mathf.Min(near,(path[i].p-t.position).magnitude);
     if(Mathf.Sign(q0.z)!=Mathf.Sign(q1.z)&&Mathf.Abs(q1.z-q0.z)<10){float f=q0.z/(q0.z-q1.z);var x=Vector3.Lerp(q0,q1,f);if(Mathf.Abs(x.x)<bestX){bestX=Mathf.Abs(x.x);where=$"crossed at x {x.x:F1} m, y {x.y:F1} m (opening +-{g.halfWidth+RaceGate.EdgeTolerance:F1}) on {path[i].branch}";}}}
    Note($"  MISS {r.Name} lap {e.Lap} CP{e.Checkpoint} ({e.Reason}, branch {e.Branch}) gate {V(t.position)} half-width {g.halfWidth}: {where}; nearest {near:F1} m");}}}

 // Part C: the three relocated sites in Free Roam: each speed trap driven through both ways at 28 m/s, the jump taken from
 // 90 m back at full throttle; the on-site prompt, the result shown, the distance (for the medal targets). sites84:profiles
 IEnumerator Sites084(string profiles){foreach(var profile in profiles.Split(',')){yield return EnterRoam("StreetLoopGreybox",profile);yield return new WaitForSeconds(1);
   var acts=flow.Activities;var car=race.vehicle;car.GetComponent<VehicleInput>().enabled=false;car.enabled=false;
   foreach(var site in acts.Sites.Where(x=>x.id.StartsWith("forest-")).OrderBy(x=>x.id)){var f=site.forward.normalized;f.y=0;f.Normalize();
    foreach(int dir in site.kind==ActivitySite.Kind.Speed?new[]{1,-1}:new[]{1}){var d=f*dir;var start=site.transform.position-d*(site.kind==ActivitySite.Kind.Jump?90:70);
     if(Physics.Raycast(start+Vector3.up*30,Vector3.down,out var gh,80,~0,QueryTriggerInteraction.Ignore))start=gh.point;
     RaceRoad trail=null;float ts=0;if(site.kind==ActivitySite.Kind.Jump){trail=FindObjectsByType<RaceRoad>(FindObjectsSortMode.None).First(r=>r.name=="Reverse navigation only - no pavement");trail.Initialize();ts=trail.Project(site.transform.position,out _)-110;start=trail.At(ts,out var tf);tf.y=0;d=tf.normalized;}
     Put(car,start+Vector3.up*.8f,Quaternion.LookRotation(d),Vector3.zero);acts.NewSession();for(int i=0;i<40;i++){car.Simulate(0,0,0,Time.fixedDeltaTime);yield return new WaitForFixedUpdate();}
     string prompt="";int awards=acts.Awards;float t0=Time.time;bool past=false;
     car.Body.linearVelocity=d*(site.kind==ActivitySite.Kind.Jump?20:28);
     while(Time.time-t0<14){var p=car.Body.position;var aim=site.transform.position+d*60;if(trail){float sp=trail.Project(p,out _);aim=trail.At(sp+12,out _);}float v=car.ForwardSpeed;float thr=v<(site.kind==ActivitySite.Kind.Jump?30:28)?1:0;
      car.Simulate(thr,0,car.GroundedWheels>=2?Steer(car,aim):0,Time.fixedDeltaTime);yield return new WaitForFixedUpdate();
      if(acts.AtSite==site&&prompt=="")prompt=FindAnyObjectByType<RaceHud>()?acts.Hud.Replace("\n"," / "):"";
      if(Vector3.Dot(car.Body.position-site.transform.position,d)>(site.kind==ActivitySite.Kind.Jump?90:30)){past=true;if(acts.Awards>awards||site.kind==ActivitySite.Kind.Speed)break;}}
     if(site.kind==ActivitySite.Kind.Jump){float t1=Time.time;while(acts.Awards==awards&&Time.time-t1<4){car.Simulate(0,.3f,0,Time.fixedDeltaTime);yield return new WaitForFixedUpdate();}}
     bool got=acts.Awards>awards;
     Check(got,$"{profile} {site.title} ({(dir>0?"forward":"opposite")}): {(got?"scored":"NO RESULT")}; prompt at the site \"{prompt}\"; result \"{acts.Feedback?.Replace("\n"," / ")}\"; {(site.kind==ActivitySite.Kind.Jump?$"distance {acts.LastJumpAward:F1} m, airtime {acts.LastAirtime:F2} s":$"speed {acts.LastSpeed:F1} m/s")}");
     if(site.kind==ActivitySite.Kind.Jump&&profile==profiles.Split(',')[0])yield return Late(()=>{var cam=Camera.main;Shot($"C-{site.id}",1600,900);});}}
   car.enabled=true;car.GetComponent<VehicleInput>().enabled=true;}}

 // Part F: the Echo Cave: the route passing nearest its enclosure (a branch or the main), ridden from 80 m before to 80 m past
 // it with each profile; a shot at the mouth (day) and one inside (night, headlights on); how many cave pieces were restyled.
 // cave84:Scene:profile1,profile2
 IEnumerator Cave084(string scene,string profiles){yield return Load(scene);yield return Menu();
  var rock=FindObjectsByType<Renderer>(FindObjectsSortMode.None).Where(r=>r.name.Contains("Echo Cave enclosed rock")).ToArray();if(rock.Length==0){Note($"{scene}: no Echo Cave here");yield break;}
  var b=rock[0].bounds;foreach(var r in rock)b.Encapsulate(r.bounds);var c=b.center;race.road.Initialize();
  // the passage itself (Dan's Forest Loop Forward Echo Cave route, s 189-349, runs through (53.3, 34.9, 78.3)); the enclosure's
  // bounds span 200 m and more, so their centre is not on the passage
  c=new Vector3(53.3f,34.9f,78.3f);
  WoodlandRoute best=null;float bestD=race.road.Project(c,out float mainLat)>=0?mainLat:999,bestS=race.road.Project(c,out _);
  foreach(var w in FindObjectsByType<WoodlandRoute>(FindObjectsSortMode.None)){w.Initialize();float s=w.Project(c,out float lat);if(lat<bestD){bestD=lat;best=w;bestS=s;}}
  Note($"{scene}: Echo Cave enclosure centre {V(c)} size {V(b.size)}; nearest route {(best?best.title:"main")} at s {bestS:F0}, {bestD:F1} m off; restyled cave pieces {SceneryWorld.Current?.Props?.CaveRocks}");
  foreach(var profile in profiles.Split(',')){
   if(best){yield return Case(new[]{"ride",scene,profile,best.title,Mathf.Max(0,bestS-80).ToString(System.Globalization.CultureInfo.InvariantCulture),Mathf.Min(best.Length-2,bestS+80).ToString(System.Globalization.CultureInfo.InvariantCulture),"16"});var r=LastRide;Check(r.reached&&r.resets==0&&!r.stuck,$"{scene} {profile} through the Echo Cave on {best.title}: {(r.reached?"through":"NOT through")}, resets {r.resets}, stuck {r.stuck}, max air {r.maxAir:F2}s, min up {r.minUp:F2}");}
   else yield return Case(new[]{"drive",scene,profile,"Main",Mathf.Max(0,bestS-80).ToString(System.Globalization.CultureInfo.InvariantCulture),"14","16"});}
  yield return EnterScene(scene);var cam=Camera.main;var chase=FindAnyObjectByType<ChaseCamera>();if(chase)chase.enabled=false;var cv=CameraViews.Current;if(cv)cv.enabled=false;foreach(var cv2 in FindObjectsByType<Canvas>(FindObjectsSortMode.None))cv2.enabled=false;
  Vector3 At(float s,out Vector3 f){if(best)return best.At(s,out f);return race.road.At(s,out f);}
  var pin=At(bestS,out var fin);var pmouth=At(Mathf.Max(0,bestS-40),out var fm);
  Park(At(Mathf.Max(0,bestS-120),out _));yield return new WaitForSeconds(1);
  WorldLook.Current?.Pin(LookPresets.Compose(TimeOfDay.Day,Weather.Clear));yield return new WaitForSeconds(.5f);
  yield return Late(()=>{cam.transform.position=pmouth+Vector3.up*1.6f-fm.normalized*6;cam.transform.LookAt(pin+Vector3.up*1.2f);Shot($"F-{scene}-cave-mouth",1600,900);});
  var car=race.vehicle;Put(car,pin-fin.normalized*4+Vector3.up*.8f,Quaternion.LookRotation(Vector3.ProjectOnPlane(fin,Vector3.up)),Vector3.zero);
  WorldLook.Current?.Pin(LookPresets.Compose(TimeOfDay.Night,Weather.Clear));yield return new WaitForSeconds(1.5f);
  yield return Late(()=>{cam.transform.position=car.transform.position-fin.normalized*5+Vector3.up*2.2f;cam.transform.LookAt(pin+fin.normalized*15+Vector3.up*1f);Shot($"F-{scene}-cave-inside-night",1600,900);});
  WorldLook.Current?.Pin(LookPresets.Compose(TimeOfDay.Day,Weather.Clear));if(chase)chase.enabled=true;if(cv)cv.enabled=true;yield return Menu();}

 // Part E: the rebuilt fences in Free Roam: the section nearest Dan's driveway and the one on the steepest ground, each shot
 // with the new look and with Classic; posts lengthened to the ground; colliders identical New / Classic. fence84
 IEnumerator Fence084(){yield return EnterRoam("StreetLoopGreybox","moto");yield return new WaitForSeconds(1);var props=SceneryWorld.Current.Props;
  bool IsFence(Renderer r)=>r.GetComponent<MeshFilter>()&&r.GetComponent<MeshFilter>().sharedMesh&&r.GetComponent<MeshFilter>().sharedMesh.name.EndsWith("(0.84 fence)");
  var fences=FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None).Where(IsFence).ToArray();
  float Slope(Renderer r){var bb=r.bounds;float lo=1e9f,hi=-1e9f;foreach(var (x,z) in new[]{(bb.min.x,bb.min.z),(bb.max.x,bb.max.z),(bb.min.x,bb.max.z),(bb.max.x,bb.min.z)})if(Physics.Raycast(new Vector3(x,bb.max.y+5,z),Vector3.down,out var h,30,~0,QueryTriggerInteraction.Ignore)){lo=Mathf.Min(lo,h.point.y);hi=Mathf.Max(hi,h.point.y);}return hi-lo;}
  var dan=fences.OrderBy(r=>(r.bounds.center-new Vector3(440,80,12)).sqrMagnitude).FirstOrDefault();var steep=fences.OrderByDescending(Slope).FirstOrDefault();
  Note($"fences rebuilt {props.Fences} (posts lengthened to the ground {props.FencePostsGrounded}); nearest Dan's driveway {(dan?P(dan.transform)+" at "+V(dan.bounds.center):"-")}; steepest {(steep?P(steep.transform)+" at "+V(steep.bounds.center)+$", {Slope(steep):F1} m of fall across it":"-")}");
  static string Cols(){var sb2=new StringBuilder();foreach(var c in FindObjectsByType<Collider>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(c=>!c.attachedRigidbody||c.attachedRigidbody.isKinematic).OrderBy(c=>P(c.transform)).ThenBy(c=>c.bounds.center.x).ThenBy(c=>c.bounds.center.z))sb2.Append($"{P(c.transform)}|{c.enabled}|{c.bounds.center:F3}|{c.bounds.size:F3}\n");return sb2.ToString();}
  var cam=Camera.main;var chase=FindAnyObjectByType<ChaseCamera>();if(chase)chase.enabled=false;var cv=CameraViews.Current;if(cv)cv.enabled=false;foreach(var cv2 in FindObjectsByType<Canvas>(FindObjectsSortMode.None))cv2.enabled=false;
  WorldLook.Current?.Pin(LookPresets.Compose(TimeOfDay.Day,Weather.Clear));
  string colsNew=Cols();
  foreach(var (r,tag) in new[]{(dan,"dan-driveway"),(steep,"slope")}){if(!r)continue;var bb=r.bounds;var along=bb.size.x>bb.size.z?Vector3.right:Vector3.forward;var side=Vector3.Cross(Vector3.up,along);
   Park(bb.center+side*25);yield return new WaitForSeconds(1);
   foreach(bool on in new[]{true,false}){Scenery.Set(on);yield return null;yield return null;var eye=bb.center+side*5+along*3+Vector3.up*1.3f;yield return Late(()=>{cam.transform.position=eye;cam.transform.LookAt(bb.center);Shot($"E-fence-{tag}-{(on?"new":"classic")}",1600,900);});}
   Scenery.Set(true);yield return null;}
  // back to back, nothing moving in between: New, Classic, New
  colsNew=Cols();Scenery.Set(false);yield return null;string colsClassic=Cols();Scenery.Set(true);yield return null;string colsNew2=Cols();
  var diff=colsNew.Split('\n').Except(colsClassic.Split('\n')).Concat(colsClassic.Split('\n').Except(colsNew.Split('\n'))).ToArray();File.WriteAllLines($"{output}/fence-collider-diff.txt",diff);
  Check(colsNew==colsClassic&&colsNew==colsNew2&&props.Fences>0,$"static colliders identical New / Classic / New taken back to back: {colsNew==colsClassic&&colsNew==colsNew2} ({colsNew.Split('\n').Length-1} colliders, {diff.Length} differing lines)");
  if(chase)chase.enabled=true;if(cv)cv.enabled=true;}
}
}
#endif
