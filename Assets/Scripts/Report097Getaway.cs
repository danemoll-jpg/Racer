#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Racer {
// 0.97 Part A pacing checks (see Report096Getaway): pace97:runner:vehicle:difficulty:seed
//   road      the 0.96 road-only runner (RoadDriver, pace 1.25, never stops)
//   roadfast  a human on the main road: flat out, slowing only for the bends (Runner097, no trails, no leaving the road)
//   human     flat out, takes the trails and shortcuts, and leaves the road on purpose (cross-country for 8-25 s every 20-40 s)
// One summary line per run is appended to PROBE_OUT/pace97.txt (vehicle, runner, outcome, time, top heat, cops near at each heat, chased at 2:00,
// off-road episodes with the seconds until exit units stood at an exit).
public sealed class Runner097 : MonoBehaviour {
 public GetawayChase G; public ArcadeVehicle Car; public bool Human, Trails; public float Cap; // 0.98: Cap = a top speed the runner will not exceed (a human barely pressing the gas)
 enum Mode{Road,Country,Return}
 Mode mode; readonly List<Vector3> path=new(); int seg; Vector3 country; float modeUntil,nextCountry,stalled,lastTeleport=-99,flipped; public int Resets, Countrys; public float LastVmax; float startedAt=-1;
 RoadNet Net=>G.Net; readonly RaycastHit[] hits=new RaycastHit[8];
 void Start(){startedAt=Time.time;nextCountry=Time.time+UnityEngine.Random.Range(20f,35f);NewRoadPath();}
 void NewRoadPath(){
  int here=CopDriver.StartNode(Net,Car.Body.position,Car.transform.forward); if(here<0)return;
  var fwd=Car.transform.forward;fwd.y=0;var far=Net.Near(Car.Body.position,1600).Where(i=>Net.Component[i]==Net.Component[here]&&Vector3.Distance(Net.P[i],Car.Body.position)>500&&Vector3.Dot((Net.P[i]-Car.Body.position).normalized,fwd.normalized)>.1f).ToList(); if(far.Count==0)far=Net.Near(Car.Body.position,1600).Where(i=>Net.Component[i]==Net.Component[here]&&Vector3.Distance(Net.P[i],Car.Body.position)>500).ToList(); if(far.Count==0)return;
  var pick=far; if(Human&&UnityEngine.Random.value<.6f){var t=far.Where(i=>Net.Roads[Net.Road[i]].forestTrail).ToList();if(t.Count>0)pick=t;}
  else if(!Trails){var m=far.Where(i=>!Net.Roads[Net.Road[i]].forestTrail).ToList();if(m.Count>0)pick=m;}
  var nodes=Net.Path(here,pick[UnityEngine.Random.Range(0,pick.Count)]); path.Clear(); if(nodes!=null)foreach(int n in nodes)path.Add(Net.P[n]); seg=0;}
 static float Seg(Vector3 p,Vector3 a,Vector3 b){var ab=b-a;ab.y=0;var ap=p-a;ap.y=0;float t=ab.sqrMagnitude<.001f?0:Mathf.Clamp01(Vector3.Dot(ap,ab)/ab.sqrMagnitude);return (ap-ab*t).magnitude;}
 Vector3 Along(float metres){
  var p=Car.Body.position; int i=Mathf.Clamp(seg,0,path.Count-2); var a=path[i];var b=path[i+1];var ab=b-a;ab.y=0;float t=ab.sqrMagnitude<.001f?0:Mathf.Clamp01(Vector3.Dot(p-a,ab)/ab.sqrMagnitude);var cur=Vector3.Lerp(a,b,t);float left=metres;
  while(true){var end=path[i+1];float len=Vector3.Distance(cur,end);if(len>=left)return cur+(end-cur).normalized*left;left-=len;cur=end;i++;if(i+1>=path.Count)return cur;}}
 void Teleport(){
  lastTeleport=Time.time;Resets++;int node=Net.Nearest(Car.Body.position,out _,900);if(node<0)return;var at=Net.P[node];var f=Net.Tangent(node);
  var rot=Quaternion.LookRotation(f);var pos=at+Vector3.up*1.2f;Car.Body.position=pos;Car.Body.rotation=rot;Car.transform.SetPositionAndRotation(pos,rot);Car.Body.linearVelocity=Car.Body.angularVelocity=Vector3.zero;Car.ClearSteering();
  mode=Mode.Road;stalled=flipped=0;NewRoadPath();}
 void FixedUpdate(){
  if(!Car||G==null||(G.State!=GetawayChase.Phase.Running&&G.State!=GetawayChase.Phase.Starting)||Car.Body.isKinematic)return;
  Car.enabled=false;float dt=Time.fixedDeltaTime;var p=Car.Body.position;float speed=Mathf.Abs(Car.ForwardSpeed);
  flipped=Car.transform.up.y<.3f?flipped+dt:0; if(flipped>2f||(stalled>3f&&Time.time-lastTeleport>5)){Teleport();return;}
  if(mode==Mode.Road&&Human&&Time.time>nextCountry){mode=Mode.Country;Countrys++;modeUntil=Time.time+UnityEngine.Random.Range(8f,25f);
   var f=Car.transform.forward;f.y=0;f.Normalize();var dir=Quaternion.Euler(0,UnityEngine.Random.Range(35f,75f)*(UnityEngine.Random.value<.5f?-1:1),0)*f;country=p+dir*220;}
  if(mode==Mode.Country&&Time.time>modeUntil){mode=Mode.Return;int back=Net.Nearest(p,out _,900);path.Clear();path.Add(p);if(back>=0)path.Add(Net.P[back]);path.Add(Net.P[back>=0?back:0]+Vector3.forward*.1f);seg=0;}
  if(mode==Mode.Return&&(Net.OffNet(p)<6||path.Count<2)){mode=Mode.Road;nextCountry=Time.time+UnityEngine.Random.Range(20f,40f);NewRoadPath();}
  if(mode!=Mode.Country){
   if(path.Count<2){NewRoadPath();Brake(speed);return;}
   while(seg+1<path.Count-1){var a=path[seg];var b=path[seg+1];var ab=b-a;ab.y=0;var ap=p-a;ap.y=0;if(Vector3.Dot(ap,ab)>ab.sqrMagnitude)seg++;else break;}
   if(mode==Mode.Road&&seg+2>=path.Count)NewRoadPath();
   if(path.Count<2){Brake(speed);return;}
  }
  float look=Mathf.Clamp(7+speed*.5f,9,30);Vector3 target;float vmax=Car.topSpeed;if(Cap>0)vmax=Mathf.Min(vmax,Cap);
  if(mode==Mode.Country){
   var d0=country-p;d0.y=0;if(d0.magnitude<20){modeUntil=0;}d0.Normalize();Vector3 dir=d0;
   foreach(float off in new[]{0f,20f,-20f,40f,-40f,65f,-65f,95f,-95f}){var d=Quaternion.Euler(0,off,0)*d0;
    int n=Physics.SphereCastNonAlloc(p+Vector3.up*.6f,.8f,d,hits,9+speed*.9f,~0,QueryTriggerInteraction.Ignore);bool clear=true;
    for(int k=0;k<n;k++){var h=hits[k];if(h.rigidbody==Car.Body||h.distance<.01f||h.normal.y>.6f||h.collider.GetComponentInParent<ArcadeVehicle>())continue;clear=false;break;}
    if(clear){dir=d;break;}}
   target=p+dir*look*1.5f;vmax=Mathf.Min(vmax,34);
  } else {
   target=Along(look);float cornerGrip=Mathf.Min(Car.maxGripAcceleration*.62f*Car.GripScale,14f),decel=Mathf.Max(6f,Car.braking*.75f*Car.GripScale);int v0=Mathf.Min(seg+1,path.Count-1);float dist=Vector3.Distance(p,path[v0]);
   for(int k=v0;k<path.Count-1&&dist<130;k++){if(k>v0){var st=path[k]-path[k-1];st.y=0;dist+=st.magnitude;}int i0=Mathf.Max(0,k-2),i1=Mathf.Min(path.Count-1,k+2);var a=path[k]-path[i0];var b=path[i1]-path[k];a.y=0;b.y=0;if(a.sqrMagnitude<.01f||b.sqrMagnitude<.01f)continue;
    float curvature=Vector3.Angle(a,b)*Mathf.Deg2Rad/Mathf.Max(8f,(a.magnitude+b.magnitude)*.5f);float cs=Mathf.Sqrt(cornerGrip/Mathf.Max(.00025f,curvature));vmax=Mathf.Min(vmax,Mathf.Sqrt(cs*cs+2*decel*Mathf.Max(0,dist-10)));}
  }
  var local=Car.transform.InverseTransformPoint(target);float angle=Mathf.Atan2(local.x,local.z);
  float maxAngle=Mathf.Lerp(Car.slowSteerAngle,Car.fastSteerAngle,Mathf.Clamp01(speed/Car.topSpeed))*Mathf.Deg2Rad;
  float steering=Mathf.Clamp(Mathf.Atan(2*Car.wheelbase*Mathf.Sin(angle)/look)/maxAngle,-1,1);if(Mathf.Abs(angle)>Mathf.PI*.5f)steering=Mathf.Sign(angle);
  if(Mathf.Abs(angle)>.7f)vmax=Mathf.Min(vmax,12);
  // traffic ahead: slow to it (a human does not drive into a car)
  int c=Physics.SphereCastNonAlloc(p+Vector3.up*.45f,.8f,Car.transform.forward,hits,8+speed*1.2f,~0,QueryTriggerInteraction.Ignore);
  for(int k=0;k<c;k++){var h=hits[k];if(h.rigidbody==Car.Body||h.normal.y>.55f||h.distance<.01f)continue;float ahead=h.rigidbody?Mathf.Max(0,Vector3.Dot(h.rigidbody.linearVelocity,Car.transform.forward)):0;vmax=Mathf.Min(vmax,Mathf.Sqrt(ahead*ahead+2*20*Mathf.Max(0,h.distance-5)));}
  float throttle=speed<vmax?Mathf.Clamp01((vmax-speed)*.6f):0,brake=speed>vmax+.5f?Mathf.Clamp01((speed-vmax)*.25f):0;
  LastVmax=vmax; if(speed<2f&&(Time.time-startedAt>4f))stalled+=dt;else if(speed>3)stalled=0;
  Car.Simulate(throttle,brake,steering,dt);}
 void Brake(float speed){Car.Simulate(0,speed>.6f?1f:0f,0,Time.fixedDeltaTime);}
}
public sealed partial class Report080Checks {
 // 0.97 Part A evidence shots at night with a human-like runner: backup joining (the radio line and the minimap), exit units on the minimap, a roadblock, the helicopter's
 // spotlight, the F3 overlay. shots97:vehicle
 IEnumerator Shots97(string vehicle){
  mode96="road";yield return StartGetaway096Night(vehicle);var g=GetawayChase.Current;
  float t0=Time.time;while(g.State==GetawayChase.Phase.Starting&&Time.time-t0<20)yield return null;
  var car=race.vehicle;var human=car.gameObject.AddComponent<Runner097>();human.G=g;human.Car=car;human.Human=true;human.Trails=true;car.GetComponent<VehicleInput>().enabled=false;
  var runner=g.Runners[0];Time.timeScale=3;float real0=Time.realtimeSinceStartup;bool joined=false,exits=false,block=false,heli=false,f3=false,hud=false;int radioSeen=0;
  while(g.State==GetawayChase.Phase.Running||g.State==GetawayChase.Phase.Starting){if(Time.realtimeSinceStartup-real0>900||flow.State!=RaceFlow.Stage.Racing)break;AudioListener.volume=0;yield return null;
   for(;radioSeen<g.RadioLog.Count;radioSeen++){var line=g.RadioLog[radioSeen];Note($"   [{g.Clock:F0}s] radio: {line}");
    if(!joined&&line.StartsWith("Unit ")&&line.Contains("joining")){joined=true;Time.timeScale=1;yield return new WaitForSeconds(2.5f);yield return Late(()=>Shot4k("A-backup-joining"));Note($"   shot: backup joining; cops near {g.Cops.Count(c=>!c.block&&Vector3.Distance(c.car.Body.position,runner.body.position)<600)}");Time.timeScale=3;}}
   if(!hud&&g.Clock>14){hud=true;Time.timeScale=1;yield return Late(()=>Shot4k("A-hud-"+vehicle));Time.timeScale=3;}
   if(!exits&&g.ExitPoints.Count>0&&g.Cops.Count(c=>c.exitUnit)>=2){exits=true;Time.timeScale=1;yield return new WaitForSeconds(1.5f);yield return Late(()=>Shot4k("A-exit-units-minimap"));Note($"   shot: exit units {g.Cops.Count(c=>c.exitUnit)} placed, exits {g.ExitPoints.Count}");Time.timeScale=3;}
   if(!block&&g.Blocks.Count>0&&Vector3.Distance(g.Blocks[0].centre,car.Body.position)<110){block=true;Time.timeScale=1;yield return Late(()=>Shot4k("A-roadblock"));Note("   shot: roadblock at "+Vector3.Distance(g.Blocks[0].centre,car.Body.position).ToString("F0")+" m");Time.timeScale=3;}
   if(!heli&&g.Heli&&Vector3.Distance(g.Heli.Aim,runner.body.position)<25){heli=true;Time.timeScale=1;yield return new WaitForSeconds(1.2f);yield return Late(()=>Shot4k("A-helicopter-spotlight"));Note($"   shot: helicopter at {V(g.Heli.transform.position)}, aim {V(g.Heli.Aim)}, sees runner {runner.heliSees}");Time.timeScale=3;}
   if(!f3&&g.Heli&&heli){f3=true;Time.timeScale=1;var hudObj=DeveloperLocationHud.Instance;if(hudObj){typeof(DeveloperLocationHud).GetMethod("Toggle",BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.Public)?.Invoke(hudObj,null);}yield return new WaitForSeconds(.6f);yield return Late(()=>Shot4k("A-f3-overlay"));Note("   shot: F3 overlay\n"+(DeveloperLocationHud.Instance?DeveloperLocationHud.Instance.HudText:"(no overlay)"));if(hudObj)typeof(DeveloperLocationHud).GetMethod("Toggle",BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.Public)?.Invoke(hudObj,null);Time.timeScale=3;}
   if(joined&&exits&&block&&heli&&f3)break;}
  Time.timeScale=1;Note($"shots97 done: backup {joined}, exits {exits}, roadblock {block}, helicopter {heli}, F3 {f3}; outcome {runner.outcome} at {g.Clock:F0} s heat {g.Heat}");
  flow.QuitSplit(false);yield return WaitScene095(()=>!SplitScreen.Active&&flow.State==RaceFlow.Stage.Ready);}
 IEnumerator StartGetaway096Night(string vehicle){
  yield return DanCopy095();Pads090();SplitScreen.Solo=true;SplitScreen.P2Device=null;SplitScreen.P2AiRunner=false;SplitScreen.P1Device=pad1;SplitScreen.Mode=SplitScreen.Kind.Police;SplitScreen.PoliceGame=SplitScreen.Game.Getaway;
  SplitScreen.PoliceDifficulty=1;SplitScreen.PoliceMinutes=8;SplitScreen.Course=0;SplitScreen.Time=TimeOfDay.Night;SplitScreen.Weather=Weather.Clear;SplitScreen.Traffic=true;SplitScreen.P1Vehicle=vehicle;SplitScreen.P2Vehicle="atv";SplitScreen.P2Ai=true;
  flow.StartSplit();yield return WaitScene095(()=>SplitScreen.Active&&GetawayChase.Current&&GetawayChase.Current.Runners.Count>0&&flow.State==RaceFlow.Stage.Racing,150);}
 IEnumerator Run097(string[] a)=>a[0] switch{"pace97"=>Pace097(a[1],a[2],int.Parse(a[3]),int.Parse(a[4])),"shots97"=>Shots97(a[1]),_=>Run097c(a)??Run097b(a)??Run096(a)};
 IEnumerator Pace097(string kind,string vehicle,int difficulty,int seed){
  UnityEngine.Random.InitState(seed*7919+13);mode96="road";
  yield return StartGetaway096(vehicle,difficulty,5);var g=GetawayChase.Current;
  float t0=Time.time;while(g.State==GetawayChase.Phase.Starting&&Time.time-t0<20)yield return null;
  var car=race.vehicle;Runner097 human=null;
  if(kind=="road"){var pilot=car.gameObject.AddComponent<RoadDriver>();pilot.Initialize(race,car,false,1,1.25f);car.GetComponent<VehicleInput>().enabled=false;}
  else{human=car.gameObject.AddComponent<Runner097>();human.G=g;human.Car=car;human.Human=kind=="human";human.Trails=kind=="human";car.GetComponent<VehicleInput>().enabled=false;}
  var runner=g.Runners[0];Time.timeScale=3;float last=-10;float real0=Time.realtimeSinceStartup;
  int radioSeen=0,traceSeen=0;var heatAt=new Dictionary<int,string>();bool chased120=false,sampled120=false;int topSpeedKmh=0;
  bool inOff=false;float offStart=0;bool one=false,two=false;var episodes=new List<string>();int offEpisodes=0;float maxSpeed=0;
  while(g.State!=GetawayChase.Phase.Over&&g.State!=GetawayChase.Phase.Done&&flow.State==RaceFlow.Stage.Racing&&Time.realtimeSinceStartup-real0<900){AudioListener.volume=0;yield return null;
   float sp=runner.body.linearVelocity.magnitude;maxSpeed=Mathf.Max(maxSpeed,sp);
   if(!heatAt.ContainsKey(g.Heat)){heatAt[g.Heat]=$"{g.Clock:F0}s:{g.Cops.Count(c=>!c.block&&Vector3.Distance(c.car.Body.position,runner.body.position)<150)}/{g.Cops.Count(c=>!c.block&&Vector3.Distance(c.car.Body.position,runner.body.position)<600)}";}
   if(!sampled120&&g.Clock>=120){sampled120=true;chased120=g.Cops.Any(c=>!c.block&&Vector3.Distance(c.car.Body.position,runner.body.position)<150);}
   float off=g.Net.OffNet(car.Body.position);
   if(!inOff&&off>14){inOff=true;offStart=g.Clock;one=two=false;offEpisodes++;}
   if(inOff){int placed=g.Cops.Count(c=>!c.block&&c.driver.Role==CopDriver.Task.Exit&&g.ExitPoints.Any(e=>Vector3.Distance(e,c.car.Body.position)<70));
    if(!one&&placed>=1){one=true;}
    if(!two&&placed>=2){two=true;episodes.Add($"{g.Clock-offStart:F1}s");}
    if(off<9){if(!two)episodes.Add(one?"1 unit only":"none");inOff=false;}
    else if(!two&&g.Clock-offStart>12&&!episodes.Contains("late")){}}
   if(g.RadioLog.Count>radioSeen){for(int q=radioSeen;q<g.RadioLog.Count;q++)Note($"   [{g.Clock:F0}s] radio: {g.RadioLog[q]}");radioSeen=g.RadioLog.Count;}
   if(g.TraceLog.Count>traceSeen){for(int q=traceSeen;q<g.TraceLog.Count;q++)Note("   trace: "+g.TraceLog[q]);traceSeen=g.TraceLog.Count;}
   if(g.Clock-last>=10){last=g.Clock;Note("   limits: "+string.Join(" ",g.Cops.Select(c=>c.driver.Label[0]+":"+c.driver.Limit+"@"+Mathf.RoundToInt(c.driver.TargetSpeed)+(c.driver.HasPath?"":"(nopath)"))));Note("   "+State096(g)+(human!=null?$" resets {human.Resets} country {human.Countrys} vmax {human.LastVmax:F0}":""));}
  }
  Time.timeScale=1;var r=g.Runners[0];
  if(inOff&&!two)episodes.Add(one?"1 unit only (still off-road)":"none (still off-road)");
  string line=$"{vehicle,-9} {kind,-8} diff {difficulty} seed {seed}: {r.outcome,-8} {r.freeSeconds,5:F0} s  top heat {r.topHeat}  heat@time cops<150m/<600m [{string.Join(", ",heatAt.OrderBy(k=>k.Key).Select(k=>"H"+k.Key+" "+k.Value))}]  chased at 2:00 {(sampled120?chased120.ToString():"n/a")}  blocks passed {r.blocks}  max speed {maxSpeed:F0} m/s  off-road episodes {offEpisodes}: exit units (2+) after [{string.Join(", ",episodes)}]";
  Note(line);var dir=Environment.GetEnvironmentVariable("PROBE_OUT");if(!string.IsNullOrEmpty(dir))File.AppendAllText(Path.Combine(dir,"pace97.txt"),line+"\n");
  Check(r.Done||g.State==GetawayChase.Phase.Over,"run ended: "+line);
  flow.QuitSplit(false);yield return WaitScene095(()=>!SplitScreen.Active&&flow.State==RaceFlow.Stage.Ready);}
}
}
#endif
