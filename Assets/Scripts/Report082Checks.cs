#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Racer {
// 0.82 targeted checks, added to the 0.80 runner (same muted isolated save, same helpers): PROBE_CASES="case:args;...".
public sealed partial class Report080Checks {
 IEnumerator Cases082(string[] a)=>a[0] switch{"project"=>ProjectCheck(a[1]),"loading"=>LoadingCheck(a[1]),"garage2"=>GarageStatsCheck(a[1],a[2]),"winner"=>WinnerCheck(a[1],a[2]),"faces"=>FacesCheck(a[1]),_=>null};

 // Part C: RaceRoad.Project (grid) gives exactly the old full-scan result: every road in the scene, 20000 points each
 // around it (on the road, beside it, high above and far outside). project:Scene
 IEnumerator ProjectCheck(string scene){yield return EnterScene(scene);yield return new WaitForSeconds(.5f);
  var rnd=new System.Random(82);int roads=0,points=0,diffs=0;double tNew=0,tOld=0;var sw=new System.Diagnostics.Stopwatch();
  foreach(var road in FindObjectsByType<RaceRoad>(FindObjectsSortMode.None).Where(r=>r.points!=null&&r.points.Length>1)){road.Initialize();roads++;
   var b=new Bounds(road.points[0],Vector3.zero);foreach(var q in road.points)b.Encapsulate(q);b.Expand(new Vector3(400,60,400));
   for(int k=0;k<20000;k++){Vector3 p;
    if(k%2==0){var q=road.points[rnd.Next(road.points.Length)];p=q+new Vector3((float)(rnd.NextDouble()-.5)*40,(float)(rnd.NextDouble()-.5)*20,(float)(rnd.NextDouble()-.5)*40);}
    else p=new Vector3(Mathf.Lerp(b.min.x,b.max.x,(float)rnd.NextDouble()),Mathf.Lerp(b.min.y,b.max.y,(float)rnd.NextDouble()),Mathf.Lerp(b.min.z,b.max.z,(float)rnd.NextDouble()));
    sw.Restart();float s1=road.Project(p,out float l1);tNew+=sw.Elapsed.TotalMilliseconds;sw.Restart();float s0=FullScan(road,p,out float l0);tOld+=sw.Elapsed.TotalMilliseconds;points++;
    if(s0!=s1||l0!=l1){if(diffs<5)Note($"  differs at {V(p)} on {road.name}: old {s0} / {l0}, new {s1} / {l1}");diffs++;}
    float prev=(float)rnd.NextDouble()*road.Length,win=k%3==0?30:k%3==1?75:400;float n1=road.ProjectNear(p,prev,win,out float nl1),n0=FullNear(road,p,prev,win,out float nl0);
    if(n0!=n1||nl0!=nl1){if(diffs<5)Note($"  ProjectNear differs at {V(p)} on {road.name}: old {n0} / {nl0}, new {n1} / {nl1}");diffs++;}}}
  Check(roads>0&&diffs==0,$"{Scene}: RaceRoad.Project and ProjectNear identical to the full scans on {roads} roads, {points} points ({diffs} different); time {tNew/points*1000:F2} µs vs {tOld/points*1000:F2} µs per call");
  if(scene!=RaceFlow.RoamScene)yield return Menu();}
 static GameObject Named(string n)=>FindObjectsByType<RectTransform>(FindObjectsInactive.Include,FindObjectsSortMode.None).Select(r=>r.gameObject).FirstOrDefault(g=>g.name==n);
 static float FullNear(RaceRoad r,Vector3 p,float previous,float window,out float lateral){var pts=r.points;var dist=new float[pts.Length+1];int n=pts.Length-(r.openHighway?1:0);
  for(int i=0;i<n;i++)dist[i+1]=dist[i]+Vector3.Distance(pts[i],pts[(i+1)%pts.Length]);float len=dist[n];float best=float.MaxValue,result=previous;
  for(int i=0;i<n;i++){var a=pts[i];var v=pts[(i+1)%pts.Length]-a;float t=Mathf.Clamp01(Vector3.Dot(p-a,v)/Mathf.Max(.001f,v.sqrMagnitude));float s=dist[i]+t*(dist[i+1]-dist[i]);float delta=Mathf.Repeat(s-previous+len*.5f,len)-len*.5f;if(Mathf.Abs(delta)>window)continue;float d=(p-a-v*t).sqrMagnitude;if(d<best){best=d;result=s;}}
  lateral=Mathf.Sqrt(best);return result;}
 static float FullScan(RaceRoad r,Vector3 p,out float lateral){var pts=r.points;var dist=new float[pts.Length+1];int n=pts.Length-(r.openHighway?1:0);
  for(int i=0;i<n;i++)dist[i+1]=dist[i]+Vector3.Distance(pts[i],pts[(i+1)%pts.Length]);
  float best=float.MaxValue,s=0;for(int i=0;i<n;i++){var a=pts[i];var v=pts[(i+1)%pts.Length]-a;float t=Mathf.Clamp01(Vector3.Dot(p-a,v)/v.sqrMagnitude);float d=(p-a-v*t).sqrMagnitude;if(d<best){best=d;s=dist[i]+t*(dist[i+1]-dist[i]);}}
  lateral=Mathf.Sqrt(best);return s;}

 // Part C (evidence for not using it): the batched Transform.TransformPoints against TransformPoint on every readable
 // mesh collider in the scene. faces:Scene
 IEnumerator FacesCheck(string scene){yield return EnterScene(scene);yield return new WaitForSeconds(.5f);int meshes=0,verts=0,diffs=0;var sw=new System.Diagnostics.Stopwatch();double one=0,batch=0;
  foreach(var m in FindObjectsByType<MeshCollider>(FindObjectsSortMode.None)){if(!m.sharedMesh||!m.sharedMesh.isReadable)continue;meshes++;var v=m.sharedMesh.vertices;var w=(Vector3[])v.Clone();
   sw.Restart();m.transform.TransformPoints(w);batch+=sw.Elapsed.TotalMilliseconds;sw.Restart();
   for(int i=0;i<v.Length;i++){var q=m.transform.TransformPoint(v[i]);verts++;if(q!=w[i]&&!(q.x==w[i].x&&q.y==w[i].y&&q.z==w[i].z))diffs++;}one+=sw.Elapsed.TotalMilliseconds;}
  Check(meshes>0&&diffs==0,$"{Scene}: TransformPoints identical to TransformPoint on {meshes} meshes, {verts} vertices ({diffs} different); {batch:F0} ms vs {one:F0} ms");
  if(scene!=RaceFlow.RoamScene)yield return Menu();}
 // Part B: the loading screen for a race start on another course (Mountain Loop Reverse via Tracks + START) or Free Roam:
 // it shows, the bar only moves forward and reaches the end, then it fades; a shot while it builds. loading:race|roam
 IEnumerator LoadingCheck(string what){yield return Load("StreetLoopGreybox");yield return Menu();int shown=LoadingScreen.Shown;
  var bars=new List<float>();var steps=new List<string>();bool shot=false;float t0=Time.realtimeSinceStartup;
  if(what=="race"){flow.OpenCourses();yield return null;flow.SelectCourseEntry(5);}else flow.StartFreeRoam();
  var screen=GameObject.Find("Loading screen (0.82)");
  while(Time.realtimeSinceStartup-t0<90){AudioListener.volume=0;yield return null;Bind();
   if(!screen)screen=GameObject.Find("Loading screen (0.82)");if(!screen)continue;
   var fill=screen.GetComponentsInChildren<RectTransform>(true).FirstOrDefault(r=>r.name=="Fill");var step=screen.GetComponentsInChildren<UnityEngine.UI.Text>(true).FirstOrDefault(t=>t.name=="Step");
   if(LoadingScreen.Holding&&fill){bars.Add(fill.anchorMax.x);if(step&&(steps.Count==0||steps[^1]!=step.text))steps.Add(step.text);}
   if(!shot&&LoadingScreen.Holding&&fill&&fill.anchorMax.x>.45f&&Camera.main){shot=true;string n=what;yield return Late(()=>Shot("loading-"+n));}
   if(!LoadingScreen.Holding&&LoadingScreen.Shown>shown&&flow!=null&&flow.Started)break;}
  float elapsed=Time.realtimeSinceStartup-t0;bool monotonic=bars.Zip(bars.Skip(1),(x,y)=>y>=x-1e-4f).All(x=>x);
  if(what=="race"){flow.StartRace();float tr=Time.realtimeSinceStartup;while(flow.State!=RaceFlow.Stage.Countdown&&flow.State!=RaceFlow.Stage.Racing&&Time.realtimeSinceStartup-tr<20){yield return null;}}
  yield return new WaitForSeconds(.6f);var canvasOn=screen&&screen.GetComponentInChildren<Canvas>(true).gameObject.activeInHierarchy;
  Check(LoadingScreen.Shown>shown&&bars.Count>5&&monotonic&&bars.Max()>.99f&&!canvasOn&&flow.Started,$"{what}: loading screen \"{LoadingScreen.LastTitle}\" shown, bar {bars.FirstOrDefault():F2} -> {bars.LastOrDefault():F2} over {bars.Count} frames (forward only {monotonic}), steps [{string.Join(" | ",steps)}], up {LoadingScreen.LastSeconds:F1} s, gone after the fade {!canvasOn}, scene {Scene}, state {flow.State}");
  yield return Menu();}

 // Part D: the garage bars for two vehicles (shots), equal to the profile numbers scaled over all ten. garage2:idA:idB
 IEnumerator GarageStatsCheck(string a,string b){yield return Load("StreetLoopGreybox");yield return Menu();flow.OpenGarage();yield return new WaitForSecondsRealtime(.5f);
  var menus=flow.GetComponent<RaceMenus>();
  foreach(var id in new[]{a,b}){flow.SelectVehicle(id);yield return new WaitForSecondsRealtime(.7f);var p=VehicleProfile.Find(id);var shown=menus.ShownStats;
   var want=Enumerable.Range(0,5).Select(i=>RaceMenus.StatFraction(i,p)).ToArray();bool ok=shown.Length==5&&shown.Zip(want,(x,y)=>Mathf.Abs(x-y)<1e-4f).All(x=>x);
   var block=Named("Vehicle stats");
   Check(ok&&block&&block.activeInHierarchy,$"garage {p.Name}: bars {string.Join(" ",shown.Select(x=>x.ToString("F2")))} (top speed {p.Speed}, accel {p.Acceleration}, grip {p.Grip}, handling {p.Response}, mass {p.Mass})");
   yield return Late(()=>Shot("garage-stats-"+id,1920,1080));}
  flow.SelectVehicle("original");flow.CloseGarage();}

 // Part E: Street Loop, one lap against three AI; the player is driven by the race pilot (fast = the player wins; slow =
 // an AI wins first). At the player's finish: the winner shot runs, the finish panel waits, the camera is in front of the
 // winner; after it the finish panel shows; order and times as the race produced. winner:player|ai:scene
 IEnumerator WinnerCheck(string who,string scene){yield return Load(scene);yield return Menu();var s=flow.Save.Settings;s.weather=0;s.timeOfDay=0;s.estimateAiFinishes=false;flow.Save.SaveSettings();
  flow.OpenGarage();flow.SelectVehicle("moto");flow.CloseGarage();race.opponents=true;race.traffic=false;race.laps=1;race.difficulty=who=="player"?0:2;race.opponentRoster=new[]{"moto","atv","original"};
  flow.StartRace();float t0=Time.realtimeSinceStartup;while(flow.State!=RaceFlow.Stage.Racing&&Time.realtimeSinceStartup-t0<30){AudioListener.volume=0;yield return null;}
  var car=race.vehicle;var pilot=car.gameObject.AddComponent<RoadDriver>();pilot.Initialize(race,car,true,1,who=="player"?1.15f:.8f);pilot.Racer=race.Racers[0];car.GetComponent<VehicleInput>().enabled=false;
  int shots0=WinnerShot.Shots;float t1=Time.time;while(!race.Progress.Finished&&Time.time-t1<600){AudioListener.volume=0;yield return null;}
  if(!race.Progress.Finished){Check(false,$"{who}: the player did not finish");yield break;}
  yield return null;var winner=race.Racers.Where(r=>r.Progress.Finished).OrderBy(r=>r.Progress.RaceTime(race.Clock)).First();
  bool started=WinnerShot.Shots==shots0+1&&WinnerShot.Active;var menus=flow.GetComponent<RaceMenus>();
  GameObject Panel()=>Named("Finish achievements");
  yield return new WaitForSecondsRealtime(1.1f);var cam=Camera.main.transform;var w=winner.Car.transform;var to=cam.position-w.position;
  bool front=Vector3.Dot(to.normalized,w.forward)>.4f&&to.magnitude<12;var panel=Panel();bool heldBack=!(panel&&panel.activeInHierarchy);var g=winner.Car.GetComponent<RiderGestures>();float weight=g?g.Weight:0;
  yield return Late(()=>Shot($"winner-{who}-celebration",1920,1080));
  yield return new WaitForSecondsRealtime(1.8f);panel=Panel();bool after=!WinnerShot.Active&&((panel&&panel.activeInHierarchy)||flow.State==RaceFlow.Stage.Results);
  yield return Late(()=>Shot($"winner-{who}-after",1920,1080));
  Check(started&&front&&heldBack&&after&&(who=="player")==!winner.IsAi&&weight>.5f,$"{who} win: winner {winner.Name} ({(winner.IsAi?"AI":"player")}), shot started {started}, camera {to.magnitude:F1} m in front ({Vector3.Dot(to.normalized,w.forward):F2}), celebration weight {weight:F2}, finish panel held back {heldBack}, back after 2.9 s {after} (state {flow.State}); order {string.Join(", ",race.Racers.Where(r=>r.Progress.Finished).OrderBy(r=>r.Progress.RaceTime(race.Clock)).Select(r=>r.Name+" "+r.Progress.RaceTime(race.Clock).ToString("F2")))}");
  Destroy(pilot);car.GetComponent<VehicleInput>().enabled=true;yield return Menu();}
}
}
#endif
