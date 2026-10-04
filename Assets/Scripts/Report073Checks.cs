#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Racer {
// 0.73 targeted checks (muted, isolated save). PROBE_CASES="case:args;..." per scene:
//  cross:route:from:to:profile:weather   Free Roam in that weather; the vehicle is driven along the named course route from
//                                        path metre `from` to `to` at race pace. Logs time, time over water, lowest body
//                                        height above the water surface, largest immersion, resets/wipe-outs.
//  views:label|ex,ey,ez|tx,ty,tz|...     Screenshots of each view in Free Roam at Day and Night in Snow and Clear (and Rain
//                                        for the snow-scene check), plus which snow scenes are active.
//  scenes                                Snow scenes: present only in Snow (Free Roam Day / Night, a race), absent in Clear and
//                                        Rain; sled moves down and back up on the ground; hockey ball moves; figures on the ice.
//  storm:minutes                         Night / Rain race: strikes, flash peaks and thunder over `minutes` (strike timer
//                                        shortened after the first natural strike to keep the check short); the Off setting;
//                                        under cover (Forest cave) no flash and muffled thunder.
//  race:tod:weather:laps                 0.72 race check at those conditions (start, checkpoints, finish, results, records).
public sealed class Report073Checks:MonoBehaviour {
 RaceDirector race;RaceFlow flow;readonly List<string> rows=new();string output,saveDir;int fails;
 void Note(string s){rows.Add(s);File.WriteAllLines(output+"/results.txt",rows);Debug.Log("REPORT073 "+s);}
 void Check(bool ok,string s){if(!ok)fails++;Note((ok?"PASS ":"FAIL ")+s);}
 IEnumerator Start(){AudioListener.volume=0;var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;output=Environment.GetEnvironmentVariable("PROBE_OUT")+"/checks-"+scene;Directory.CreateDirectory(output);Application.runInBackground=true;
  race=FindAnyObjectByType<RaceDirector>();yield return null;yield return null;flow=race.Flow;
  saveDir=Path.GetFullPath("Temp/Report073Save");if(Directory.Exists(saveDir))Directory.Delete(saveDir,true);flow.UseValidationSave(saveDir);
  var title=FindAnyObjectByType<StartupTitle>();if(title)Destroy(title.gameObject);typeof(StartupTitle).GetField("completed",BindingFlags.Static|BindingFlags.NonPublic).SetValue(null,true);flow.EnterMenuAfterTitle();
  yield return null;
  foreach(var spec in (Environment.GetEnvironmentVariable("PROBE_CASES")??"").Split(';').Where(x=>x.Length>0)){var a=spec.Split(':');
   IEnumerator run=a[0] switch{"cross"=>Cross(a[1],float.Parse(a[2]),float.Parse(a[3]),a[4],(Weather)Enum.Parse(typeof(Weather),a[5])),"views"=>Views(spec.Substring(6)),"scenes"=>Scenes(),"storm"=>Storm(float.Parse(a[1])),
    "moto"=>Moto(),"race"=>Race((TimeOfDay)Enum.Parse(typeof(TimeOfDay),a[1]),(Weather)Enum.Parse(typeof(Weather),a[2]),int.Parse(a[3])),_=>null};
   if(run!=null)yield return run;else Note("unknown case "+spec);}
  Note($"done, failures {fails}");File.WriteAllLines(output+"/done.txt",rows);UnityEditor.EditorApplication.Exit(0);}
 IEnumerator ToMenu(){if(flow.State!=RaceFlow.Stage.Ready){flow.Pause();flow.QuitRace();}yield return null;yield return null;}
 IEnumerator Roam(string profile,Weather w){yield return ToMenu();flow.Save.Settings.roamWeather=(int)w;flow.Save.SaveSettings();flow.OpenGarage();flow.SelectVehicle(profile);flow.CloseGarage();race.opponents=false;race.traffic=false;flow.StartFreeRoam();
  float t0=Time.realtimeSinceStartup;while(flow.State!=RaceFlow.Stage.Racing&&Time.realtimeSinceStartup-t0<60){AudioListener.volume=0;yield return null;}Time.timeScale=1;yield return new WaitForSeconds(.3f);}
 ArcadeVehicle Car=>race.vehicle;
 void Put(Vector3 p,Quaternion r,Vector3 v){var car=Car;car.Body.isKinematic=false;car.transform.SetPositionAndRotation(p,r);car.Body.position=p;car.Body.rotation=r;car.Body.linearVelocity=v;car.Body.angularVelocity=Vector3.zero;car.ClearSteering();Physics.SyncTransforms();race.ResetSampling(p,race.Clock);car.GetComponent<VehicleRespawn>().SeedCoursePosition(p);}
 float Steer(Vector3 target){var car=Car;var d=Quaternion.Inverse(Quaternion.LookRotation(Vector3.ProjectOnPlane(car.transform.forward,Vector3.up)))*(target-car.Body.position);return Mathf.Clamp(Mathf.Atan2(d.x,d.z)*1.6f,-1,1);}
 static List<Vector3> Route(string name){
  foreach(var r in FindObjectsByType<RaceRoad>(FindObjectsInactive.Include,FindObjectsSortMode.None))if(r.name==name||Full(r.transform).EndsWith(name))return r.points.ToList();
  foreach(var r in FindObjectsByType<WoodlandRoute>(FindObjectsInactive.Include,FindObjectsSortMode.None))if(r.name==name||Full(r.transform).EndsWith(name))return r.points.ToList();return null;}
 static string Full(Transform t){var n=t.name;while(t.parent){t=t.parent;n=t.name+"/"+n;}return n;}
 static Vector3 AtMetre(List<Vector3> pts,float m,out Vector3 f){float s=0;for(int i=0;i+1<pts.Count;i++){float l=Vector3.Distance(pts[i],pts[i+1]);if(s+l>=m){f=(pts[i+1]-pts[i]).normalized;return Vector3.Lerp(pts[i],pts[i+1],(m-s)/l);}s+=l;}f=(pts[^1]-pts[^2]).normalized;return pts[^1];}

 // ---------- A2: a water crossing in Clear and Snow ----------
 IEnumerator Cross(string routeName,float from,float to,string profile,Weather w){yield return Roam(profile,w);AudioListener.volume=0;
  // "line=x,z/x,z/..." drives a straight authored line instead of a course route (from/to in metres along it).
  var pts=routeName.StartsWith("line=")?routeName.Substring(5).Split('/').Select(q=>{var a=q.Split(',').Select(float.Parse).ToArray();var v=new Vector3(a[0],200,a[1]);return Physics.Raycast(v,Vector3.down,out var h,300,1,QueryTriggerInteraction.Ignore)?h.point:v;}).ToList():Route(routeName);
  if(pts==null){Check(false,"route not found "+routeName);yield break;}
  var start=AtMetre(pts,from,out var f0);if(Physics.Raycast(start+Vector3.up*30,Vector3.down,out var g,80,1,QueryTriggerInteraction.Ignore))start=g.point;
  f0.y=0;var car=Car;car.enabled=false;const float pace=16;Put(start+Vector3.up*.7f,Quaternion.LookRotation(f0.normalized),f0.normalized*pace);
  int resets=0;var rs=car.GetComponent<VehicleRespawn>();Action onReset=()=>resets++;rs.Respawned+=onReset;
  float t0=Time.time,tWater=0,minAbove=float.MaxValue,maxWet=0,dist=0;Vector3 last=car.Body.position;string end="time";float m=from;int samples=0;
  using(var log=new StreamWriter($"{output}/cross-{routeName.Replace('/','_')}-{profile}-{w}.csv")){log.WriteLine("t,x,y,z,speed,wet,aboveSurface,iceUnder");
   while(Time.time-t0<40){yield return new WaitForFixedUpdate();var p=car.Body.position;dist+=Vector3.Distance(p,last);last=p;
    // target: the route point 10 m ahead of the vehicle's own progress
    m=Mathf.Max(m,from+dist*.98f);var tgt=AtMetre(pts,Mathf.Min(m+10,to+10),out _);float sp=car.ForwardSpeed;
    car.Simulate(sp<pace?1:0,sp>pace+2?.5f:0,Steer(tgt),Time.fixedDeltaTime);
    ShallowWater over=null;foreach(var wa in ShallowWater.Active)if(wa&&wa.transform.lossyScale.x>4&&wa.Contains(p))over=wa;
    if(over){tWater+=Time.fixedDeltaTime;minAbove=Mathf.Min(minAbove,p.y-over.Surface);samples++;}
    maxWet=Mathf.Max(maxWet,car.WaterImmersion);
    log.WriteLine($"{Time.time-t0:F2},{p.x:F2},{p.y:F2},{p.z:F2},{sp:F2},{car.WaterImmersion:F2},{(over?p.y-over.Surface:float.NaN):F2},{(over&&over.Ice?1:0)}");
    if(m>=to){end="complete";break;}if(resets>0){end="reset";break;}}}
  rs.Respawned-=onReset;car.enabled=true;
  Note($"CROSS {routeName} m{from:F0}-{to:F0} {profile} {w}: {end} in {Time.time-t0:F2} s; over water {tWater:F2} s ({samples} steps); lowest body above surface {(samples>0?minAbove:float.NaN):F2} m; largest immersion {maxWet:F2}; resets {resets}; frozen {ShallowWater.Frozen}");}

 // ---------- screenshots ----------
 IEnumerator Shot(string name,Vector3 eye,Vector3 target){
  var cam=Camera.main;foreach(var c in FindObjectsByType<ChaseCamera>(FindObjectsSortMode.None))c.enabled=false;cam.transform.SetPositionAndRotation(eye,Quaternion.LookRotation(target-eye));
  yield return null;yield return null;
  var rt=new RenderTexture(1600,900,24,RenderTextureFormat.ARGB32){antiAliasing=2};var old=cam.targetTexture;cam.targetTexture=rt;cam.Render();cam.targetTexture=old;
  RenderTexture.active=rt;var tex=new Texture2D(1600,900,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,1600,900),0,0);tex.Apply();RenderTexture.active=null;rt.Release();
  File.WriteAllBytes(Path.Combine(output,name),tex.EncodeToJPG(88));Destroy(tex);
  foreach(var c in FindObjectsByType<ChaseCamera>(FindObjectsSortMode.None))c.enabled=true;}
 void SetHour(float h){typeof(WorldLook).GetProperty("Hour").SetValue(WorldLook.Current,h);}
 IEnumerator Views(string spec){
  var views=spec.Split('|');var label=views[0];var list=new List<(string id,Vector3 e,Vector3 t)>();
  Vector3 V(string s){var a=s.Split(',').Select(float.Parse).ToArray();return new Vector3(a[0],a[1],a[2]);}
  // "@Water name,lx,lz" targets the top of that water volume at local (lx, lz); the eye is then an offset from it.
  for(int i=1;i+1<views.Length;i+=3){var e=V(views[i+1]);Vector3 t;if(views[i+2].StartsWith("@")){var a=views[i+2].Substring(1).Split(',');var w=ShallowWater.Active.First(x=>x&&x.name==a[0]);t=w.transform.TransformPoint(float.Parse(a[1]),.5f,float.Parse(a[2]));e=t+e;}else t=V(views[i+2]);list.Add((views[i],e,t));}
  foreach(var w in new[]{Weather.Snow,Weather.Clear,Weather.Rain}){if(w==Weather.Rain&&label!="scenes")continue;yield return Roam("atv",w);
   var car=Car;car.Body.isKinematic=true;car.transform.position+=Vector3.down*400;Physics.SyncTransforms();// out of every view
   foreach(float h in w==Weather.Clear&&label!="scenes"?new[]{12f}:new[]{12f,23f}){SetHour(h);yield return new WaitForSeconds(1.2f);
    var sc=FindAnyObjectByType<SnowScenes>();
    foreach(var v in list){yield return Shot($"{label}-{v.id}-{w}-{(h<20?"day":"night")}.jpg".ToLowerInvariant(),v.e,v.t);}
    Note($"VIEWS {label} {w} {(h<20?"Day":"Night")} ({WorldLook.Current.Preset.name}): frozen {ShallowWater.Frozen}; sled scene {(sc?sc.SledActive:false)}, hockey scene {(sc?sc.HockeyActive:false)}; clouds visible {FindAnyObjectByType<SkyClouds>()?.Visible}");}
   car.Body.isKinematic=false;}
  yield return ToMenu();}

 // ---------- A3/A4 snow scenes ----------
 IEnumerator Scenes(){
  foreach(var w in new[]{Weather.Clear,Weather.Rain,Weather.Snow}){yield return Roam("atv",w);yield return new WaitForSeconds(1);var sc=FindAnyObjectByType<SnowScenes>();
   bool want=w==Weather.Snow;Check(sc&&sc.SledActive==want&&sc.HockeyActive==want,$"Free Roam {w}: sled scene {(sc?sc.SledActive:false)}, hockey scene {(sc?sc.HockeyActive:false)} (expected {(want?"present":"absent")})");
   if(!want)continue;
   // Park the player next to the scenes so they animate; follow them for a full sled cycle.
   var car=Car;car.Body.isKinematic=true;car.transform.position=new Vector3(512,90,-170);Physics.SyncTransforms();
   float minSledY=999,maxSledY=-999,minGap=999,maxGap=-999;var ball0=sc.BallPosition;float ballMoved=0;Vector3 lastBall=ball0;float onIce=0;int n=0;float worstFig=0;
   var hockey=GameObject.Find("Snow day / broom hockey (Snow only)");var sledScene=GameObject.Find("Snow day / sledding (Snow only)");
   float t0=Time.time;bool shotTop=false,shotMid=false,shotWalk=false;
   while(Time.time-t0<48){yield return new WaitForSeconds(.25f);var s=sc.SledPosition;minSledY=Mathf.Min(minSledY,s.y);maxSledY=Mathf.Max(maxSledY,s.y);
    if(Physics.Raycast(s+Vector3.up*3,Vector3.down,out var g,8,1,QueryTriggerInteraction.Ignore)){float gap=s.y-g.point.y;minGap=Mathf.Min(minGap,gap);maxGap=Mathf.Max(maxGap,gap);}
    var b=sc.BallPosition;ballMoved+=Vector3.Distance(b,lastBall);lastBall=b;n++;
    foreach(Transform fig in hockey.transform)if(fig.name=="Snow day resident")worstFig=Mathf.Max(worstFig,Mathf.Abs(fig.position.y-79.90f));
    float age=Time.time-t0;
    if(!shotMid&&s.y<maxSledY-6&&s.y>minSledY+.1f&&age<12){shotMid=true;yield return Shot("sled-sliding-snow-day.jpg",s+new Vector3(-9,4,4),s);}
    if(!shotWalk&&age>22){shotWalk=true;yield return Shot("sled-walking-up-snow-day.jpg",s+new Vector3(-9,4,4),s);}}
   Check(maxSledY-minSledY>12,$"sled travels down the hill and back: height range {minSledY:F1} - {maxSledY:F1} m");
   Check(minGap>-.05f&&maxGap<.35f,$"sled stays on the ground: clearance {minGap:F2} .. {maxGap:F2} m");
   Check(ballMoved>8&&worstFig<.05f,$"hockey: ball travelled {ballMoved:F1} m in {n} samples; figures on the ice (largest height error {worstFig:F3} m); ice collider {(ShallowWater.Active.FirstOrDefault(x=>x&&x.name=="Rear swimming pool")?.Ice!=null)}");
   // Night look of both
   SetHour(23);yield return new WaitForSeconds(1.5f);
   yield return Shot("hockey-snow-night.jpg",new Vector3(400.8f,82.8f,-1.0f),new Vector3(397.3f,79.9f,-12.5f));
   yield return Shot("sled-snow-night.jpg",new Vector3(523.2f,77.5f,-148f),sc.SledPosition);
   SetHour(12);yield return new WaitForSeconds(1.5f);
   yield return Shot("hockey-snow-day.jpg",new Vector3(400.8f,82.8f,-1.0f),new Vector3(397.3f,79.9f,-12.5f));
   car.Body.isKinematic=false;}
  // A race in Snow shows them too
  yield return ToMenu();flow.Save.Settings.timeOfDay=0;flow.Save.Settings.weather=2;flow.Save.SaveSettings();race.opponents=false;flow.StartRace();float tr=Time.realtimeSinceStartup;while(flow.State!=RaceFlow.Stage.Racing&&Time.realtimeSinceStartup-tr<30)yield return null;yield return new WaitForSeconds(.5f);
  var sc2=FindAnyObjectByType<SnowScenes>();Check(sc2&&sc2.SledActive&&sc2.HockeyActive,$"race in Snow: sled {sc2?.SledActive}, hockey {sc2?.HockeyActive}");
  yield return ToMenu();flow.Save.Settings.weather=0;flow.Save.SaveSettings();yield return null;
  Check(sc2&&!sc2.SledActive&&!sc2.HockeyActive,$"menu (Clear Day): scenes hidden (sled {sc2?.SledActive}, hockey {sc2?.HockeyActive})");}

 // ---------- B: lightning ----------
 IEnumerator Storm(float minutes){
  yield return ToMenu();var s=flow.Save.Settings;s.timeOfDay=2;s.weather=1;s.lightningFlashes=true;flow.Save.SaveSettings();race.opponents=false;race.traffic=false;flow.StartRace();
  float tr=Time.realtimeSinceStartup;while(flow.State!=RaceFlow.Stage.Racing&&Time.realtimeSinceStartup-tr<30)yield return null;
  var fx=WorldLook.Current.GetComponent<WeatherEffects>();var car=Car;car.Body.isKinematic=true;
  var fNext=typeof(WeatherEffects).GetField("nextStrike",BindingFlags.NonPublic|BindingFlags.Instance);
  float t0=Time.time;var gaps=new List<float>();float lastStrike=-1;int seen=0;float maxFlash=0;int maxPulses=0;var pulseLog=new List<string>();
  // Natural timing first: wait for the first two strikes as scheduled (20-60 s apart).
  while(Time.time-t0<minutes*60&&fx.Strikes<3){yield return null;AudioListener.volume=0;if(fx.Strikes!=seen){seen=fx.Strikes;if(lastStrike>=0)gaps.Add(Time.time-lastStrike);lastStrike=Time.time;
    // count pulses of this strike and capture the brightest frame
    int pulses=0;bool up=false;float peak=0;float ts=Time.time;bool shot=false;
    while(Time.time-ts<.6f){yield return null;float f=fx.FlashLevel;peak=Mathf.Max(peak,f);if(f>.05f&&!up){up=true;pulses++;}if(f<.02f)up=false;if(!shot&&f>.25f&&seen==1){shot=true;yield return Shot("lightning-night-rain-flash.jpg",car.transform.position+new Vector3(0,4,0)-car.transform.forward*8,car.transform.position+car.transform.forward*30);}}
    maxFlash=Mathf.Max(maxFlash,peak);maxPulses=Mathf.Max(maxPulses,pulses);pulseLog.Add($"strike {seen} at {Time.time-t0:F1} s: {pulses} pulse(s), peak {peak:F2}");}}
  yield return new WaitForSeconds(5);
  Note(string.Join("; ",pulseLog));
  Check(fx.Strikes>=3&&gaps.All(g=>g>=19.9f&&g<=60.6f),$"Night/Rain race: {fx.Strikes} strikes in {Time.time-t0:F0} s, gaps {string.Join(", ",gaps.Select(g=>g.ToString("F1")))} s (20-60 s), thunder played {fx.ThunderPlayed}");
  Check(maxPulses<=2&&maxPulses>=1&&maxFlash>.3f,$"flashes: at most {maxPulses} pulses per strike, peak level {maxFlash:F2} (night)");
  yield return Shot("lightning-night-rain-between.jpg",car.transform.position+new Vector3(0,4,0)-car.transform.forward*8,car.transform.position+car.transform.forward*30);
  // Off: thunder stays, no flash
  s.lightningFlashes=false;flow.Save.SaveSettings();int th0=fx.ThunderPlayed,st0=fx.Strikes;float peakOff=0;fNext.SetValue(fx,Time.time+1);float t1=Time.time;
  while(Time.time-t1<8){yield return null;peakOff=Mathf.Max(peakOff,fx.FlashLevel);}
  Check(fx.Strikes>st0&&fx.ThunderPlayed>th0&&peakOff==0,$"Lightning flashes Off: strike {fx.Strikes-st0}, thunder {fx.ThunderPlayed-th0}, flash peak {peakOff:F2}");
  s.lightningFlashes=true;flow.Save.SaveSettings();
  // Under cover: inside the Forest cave if this scene has it, else under the nearest roof the weather sees.
  var cave=new Vector3(59.7f,37.4f,87.0f);var cam=Camera.main;foreach(var c in FindObjectsByType<ChaseCamera>(FindObjectsSortMode.None))c.enabled=false;cam.transform.SetPositionAndRotation(cave,Quaternion.LookRotation(new Vector3(87.4f,40.4f,157.5f)-cave));
  yield return new WaitForSeconds(1);int th1=fx.ThunderPlayed,st1=fx.Strikes;float peakCov=0;fNext.SetValue(fx,Time.time+1);float t2=Time.time;float cutoff=0,vol=0;var src=fx.GetComponentsInChildren<AudioSource>(true).First(a=>a.name=="Thunder");
  while(Time.time-t2<8){yield return null;peakCov=Mathf.Max(peakCov,fx.FlashLevel);if(src.isPlaying){cutoff=src.GetComponent<AudioLowPassFilter>().cutoffFrequency;vol=src.volume;}}
  var covered=typeof(WeatherEffects).GetField("covered",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(fx);
  Check(fx.Strikes>st1&&peakCov==0&&fx.ThunderPlayed>th1&&cutoff<=600,$"under cover (cave, covered={covered}): strike {fx.Strikes-st1}, flash peak {peakCov:F2}, thunder {fx.ThunderPlayed-th1} muffled (low-pass {cutoff:F0} Hz, volume {vol:F2})");
  foreach(var c in FindObjectsByType<ChaseCamera>(FindObjectsSortMode.None))c.enabled=true;car.Body.isKinematic=false;
  yield return ToMenu();Check(SkyClouds.Flash==0&&WorldLook.Current.Preset.name=="Clear Day",$"menu after the storm: Clear Day, no flash left ({SkyClouds.Flash})");
  s.timeOfDay=0;s.weather=0;flow.Save.SaveSettings();}

 // ---------- D: the Needle 600 model ----------
 IEnumerator Turntable(string tag){// old/new model, 4 views, garage-style isolated render (layer 31) in the scene light
  var root=new GameObject("Model sheet root");root.layer=31;root.transform.position=new Vector3(10000,10000,10000);
  var body=VehicleVisual.Build(root.transform,VehicleProfile.Find("moto"));VehiclePaint.Apply(root.transform,VehiclePaint.Colors[3]);
  var cam=new GameObject("Model sheet camera").AddComponent<Camera>();cam.cullingMask=1<<31;cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=new Color(.62f,.68f,.76f);cam.fieldOfView=30;cam.nearClipPlane=.05f;cam.enabled=false;
  var c=root.transform.position+Vector3.up*.25f;
  foreach(var (name,eye) in new[]{("front",new Vector3(0,.45f,4.6f)),("side",new Vector3(4.8f,.35f,0)),("three-quarter",new Vector3(3.3f,1.3f,3.3f)),("top",new Vector3(0,5.2f,-.01f))}){
   cam.transform.position=root.transform.position+eye;cam.transform.LookAt(c);var rt=new RenderTexture(1000,760,24){antiAliasing=4};cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;var tex=new Texture2D(1000,760,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,1000,760),0,0);tex.Apply();RenderTexture.active=null;cam.targetTexture=null;rt.Release();
   File.WriteAllBytes(Path.Combine(output,$"unity-{tag}-{name}.png"),tex.EncodeToPNG());Destroy(tex);yield return null;}
  int tris=0;foreach(var mf in root.GetComponentsInChildren<MeshFilter>())if(mf.sharedMesh)tris+=mf.sharedMesh.triangles.Length/3;
  var wheels=root.GetComponentsInChildren<Transform>().Where(t=>t.name.Contains("wheel")||t.name=="Wheel").Select(t=>$"{t.name}@{root.transform.InverseTransformPoint(t.position):F3}");
  var rs=root.GetComponentsInChildren<Renderer>();var b=rs[0].bounds;foreach(var r in rs)b.Encapsulate(r.bounds);
  Note($"MODEL {tag}: triangles {tris}, renderers {rs.Length}, materials {rs.Select(r=>r.sharedMaterial.name).Distinct().Count()} ({string.Join(", ",rs.Select(r=>r.sharedMaterial.name).Distinct())}); bounds min {root.transform.InverseTransformPoint(b.min):F2} max {root.transform.InverseTransformPoint(b.max):F2}; wheels {string.Join(" ",wheels)}");
  Destroy(cam.gameObject);Destroy(root);yield return null;}
 IEnumerator Moto(){
  yield return ToMenu();
  VehicleVisual.NewModels=false;yield return Turntable("classic");VehicleVisual.NewModels=true;yield return Turntable("new");
  // Garage: select the motorcycle, default New, switch both ways, colours including black
  var s=flow.Save.Settings;Check(s.newMotorcycle,"default Model: New");
  flow.OpenGarage();flow.SelectVehicle("moto");yield return null;
  var cfg=Car.GetComponent<VehicleConfiguration>();bool HasNew()=>Car.GetComponentsInChildren<Transform>(true).Any(t=>t.name=="Needle 600 model"&&t.gameObject.activeInHierarchy);
  var modelButton=FindObjectsByType<UnityEngine.UI.Button>(FindObjectsSortMode.None).FirstOrDefault(b=>b.gameObject.activeInHierarchy&&b.GetComponentInChildren<UnityEngine.UI.Text>(true).text.StartsWith("Model:"));
  Check(HasNew()&&modelButton,$"garage, motorcycle: new model on the player bike {HasNew()}; row '{modelButton?.GetComponentInChildren<UnityEngine.UI.Text>(true).text}'");
  flow.ToggleModel();yield return null;Check(!HasNew()&&!s.newMotorcycle&&new RacerSave(saveDir,"street-loop-gates-v1-laps3").Settings.newMotorcycle==false,$"switch to Classic: new model present {HasNew()}, saved {s.newMotorcycle}");
  yield return GarageShot("garage-classic.png");
  flow.ToggleModel();yield return null;Check(HasNew()&&s.newMotorcycle&&new RacerSave(saveDir,"street-loop-gates-v1-laps3").Settings.newMotorcycle,$"switch back to New: new model present {HasNew()}, saved (relaunch reads) {s.newMotorcycle}");
  foreach(int colour in new[]{6,1}){flow.SetColor(colour);yield return null;
   var painted=Car.GetComponentsInChildren<Renderer>().Where(r=>VehiclePaint.IsBodyPaint(r.sharedMaterial)).ToArray();var blk=new MaterialPropertyBlock();painted[0].GetPropertyBlock(blk);
   var riderMats=Car.GetComponentsInChildren<Renderer>().Where(r=>r.transform.parent&&r.transform.parent.name=="Steering pose").Select(r=>r.sharedMaterial.name).ToArray();
   Check(painted.Length>=2&&blk.GetColor("_BaseColor")==VehiclePaint.Colors[colour]&&riderMats.All(m=>!m.Contains("paint")),$"colour {VehiclePaint.Names[colour]}: {painted.Length} bodywork renderers painted {blk.GetColor("_BaseColor")}, rider keeps own materials ({string.Join(", ",riderMats.Distinct())})");
   yield return GarageShot($"garage-{VehiclePaint.Names[colour].ToLowerInvariant()}.png");}
  flow.CloseGarage();yield return null;
  // Race Day / Night with an AI field of motorcycles; chase view + close side views; wipe-out and reset (Day)
  s.opponentChoices=new[]{"moto","moto","moto"};s.opponentRoster=new[]{"moto","moto","moto"};flow.Save.SaveSettings();
  foreach(var t in new[]{TimeOfDay.Day,TimeOfDay.Night}){yield return ToMenu();s.timeOfDay=(int)t;s.weather=0;flow.Save.SaveSettings();race.opponents=true;race.opponentRoster=new[]{"moto","moto","moto"};race.traffic=false;flow.StartRace();
   float tr=Time.realtimeSinceStartup;while(flow.State!=RaceFlow.Stage.Racing&&Time.realtimeSinceStartup-tr<30)yield return null;
   var car=Car;var pilot=car.gameObject.AddComponent<RoadDriver>();pilot.Initialize(race,car,true,1,1);pilot.Racer=race.Racers[0];car.GetComponent<VehicleInput>().enabled=false;
   yield return new WaitForSeconds(6);
   var ais=race.Racers.Where(r=>r.IsAi).Select(r=>r.Car).ToArray();
   var aiNew=ais.Count(a=>a.GetComponentsInChildren<Transform>().Any(x=>x.name=="Needle 600 model"));
   var shirts=ais.Select(a=>a.GetComponentsInChildren<Renderer>().FirstOrDefault(r=>r.name=="Rider__shirt")?.sharedMaterial.name).ToArray();
   Check(aiNew==ais.Length&&ais.Length==3,$"{t} race: AI motorcycles with the new model {aiNew}/{ais.Length}; rider shirts {string.Join(", ",shirts)}; lamps level {VehicleLights.Level:F2}");
   var camT=Camera.main.transform;yield return Shot($"race-{t.ToString().ToLowerInvariant()}-chase.jpg",camT.position,camT.position+camT.forward*10);
   var side=car.transform.position+car.transform.right*3.2f+Vector3.up*.6f;yield return Shot($"race-{t.ToString().ToLowerInvariant()}-side.jpg",side,car.transform.position+Vector3.up*.4f);
   var ai0=ais[0].transform;var aside=ai0.position+ai0.right*3.5f+Vector3.up*.8f;yield return Shot($"race-{t.ToString().ToLowerInvariant()}-ai-side.jpg",aside,ai0.position+Vector3.up*.4f);
   if(t==TimeOfDay.Day){
    pilot.enabled=false;car.Body.linearVelocity=car.transform.forward*2;car.Body.MoveRotation(car.Body.rotation*Quaternion.Euler(0,0,170));car.Body.angularVelocity=Vector3.zero;
    float tw=Time.time;while(Time.time-tw<4&&!cfg.WipedOut)yield return null;bool wiped=cfg.WipedOut;
    yield return Shot("wipeout.jpg",car.transform.position+new Vector3(3,2,3),car.transform.position);
    var rs=car.GetComponent<VehicleRespawn>();rs.ResetVehicle();float tt=Time.time;while(Time.time-tt<3)yield return null;
    Check(wiped&&car.transform.up.y>.9f&&!cfg.WipedOut&&HasNew(),$"wipe-out then reset: wiped {wiped}, after reset up {car.transform.up.y:F2}, wiped {cfg.WipedOut}, model intact {HasNew()}");
    yield return Shot("after-reset.jpg",car.transform.position+car.transform.right*3.5f+Vector3.up*1.2f,car.transform.position+Vector3.up*.3f);}
   Destroy(pilot);car.GetComponent<VehicleInput>().enabled=true;}
  yield return ToMenu();s.timeOfDay=0;flow.Save.SaveSettings();}
 IEnumerator GarageShot(string name){yield return null;var m=FindAnyObjectByType<RaceMenus>();var rt=typeof(RaceMenus).GetField("previewTexture",BindingFlags.NonPublic|BindingFlags.Instance)?.GetValue(m) as RenderTexture;
  var cam=typeof(RaceMenus).GetField("previewCamera",BindingFlags.NonPublic|BindingFlags.Instance)?.GetValue(m) as Camera;if(!rt||!cam){Note("no garage preview");yield break;}
  cam.Render();RenderTexture.active=rt;var tex=new Texture2D(rt.width,rt.height,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,rt.width,rt.height),0,0);tex.Apply();RenderTexture.active=null;File.WriteAllBytes(Path.Combine(output,name),tex.EncodeToPNG());Destroy(tex);}

 // ---------- race at conditions (0.72 check) ----------
 IEnumerator Race(TimeOfDay t,Weather w,int laps){
  yield return ToMenu();
  flow.Save.Settings.timeOfDay=(int)t;flow.Save.Settings.weather=(int)w;flow.Save.SaveSettings();
  flow.OpenGarage();flow.SelectVehicle("moto");flow.CloseGarage();race.opponents=true;race.laps=laps;flow.Save.Settings.estimateAiFinishes=true;
  string category=race.Category;
  flow.StartRace();float t0=Time.realtimeSinceStartup;while(flow.State!=RaceFlow.Stage.Countdown&&flow.State!=RaceFlow.Stage.Racing&&Time.realtimeSinceStartup-t0<30)yield return null;
  yield return null;yield return null;var look=WorldLook.Current;
  Check(look.Mode=="Race"&&look.RaceTime==t&&look.RaceWeather==w,$"{t}/{w}: race look '{look.Preset.name}' frozen water {ShallowWater.Frozen}");
  while(flow.State==RaceFlow.Stage.Countdown)yield return null;
  var car=race.vehicle;var pilot=car.gameObject.AddComponent<RoadDriver>();pilot.Initialize(race,car,true,1,1);pilot.Racer=race.Racers[0];car.GetComponent<VehicleInput>().enabled=false;
  Time.timeScale=3;int gates=0,last=race.Progress.NextGate;float start=Time.time;float maxWet=0;float iceTime=0;
  while(!race.ClassificationFinal&&Time.time-start<600){yield return null;AudioListener.volume=0;if(race.Progress.NextGate!=last){gates++;last=race.Progress.NextGate;}maxWet=Mathf.Max(maxWet,car.WaterImmersion);
   foreach(var wa in ShallowWater.Active)if(wa&&wa.Ice&&wa.Contains(car.Body.position)&&car.Body.position.y>wa.Surface)iceTime+=Time.deltaTime;if(flow.State==RaceFlow.Stage.Results)break;}
  Time.timeScale=1;
  var p=race.Progress;Check(p.Finished&&p.CompletedLaps==laps,$"{t}/{w}: player (AI driver) finished {p.CompletedLaps}/{laps} laps, checkpoint changes {gates}, missed gates {p.MissedGates}, time {RaceHud.FormatTime(p.AdjustedTime(race.Clock))}, recoveries {pilot.RecoveryCount}; time on ice {iceTime:F1} s, largest immersion {maxWet:F2}");
  float waitResults=Time.realtimeSinceStartup;while(flow.State!=RaceFlow.Stage.Results&&Time.realtimeSinceStartup-waitResults<60)yield return null;
  Check(flow.State==RaceFlow.Stage.Results,$"{t}/{w}: results screen; standings:\n{race.Standings()}");
  var recent=Directory.GetFiles(saveDir,"*",SearchOption.AllDirectories).Where(f=>File.GetLastWriteTime(f)>DateTime.Now.AddMinutes(-3)&&Path.GetFileName(f).Contains("record")).Select(Path.GetFileName).ToArray();
  Check(recent.Length>0&&flow.Save.Best.race>0,$"{t}/{w}: race record saved in category '{category}' (best race {flow.Save.Best.race:F2} s; files {string.Join(", ",recent)})");
  if(pilot){pilot.enabled=false;Destroy(pilot);}car.GetComponent<VehicleInput>().enabled=true;
  yield return ToMenu();flow.Save.Settings.timeOfDay=0;flow.Save.Settings.weather=0;flow.Save.SaveSettings();}
}
}
#endif
