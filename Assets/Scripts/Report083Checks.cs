#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System.Reflection;

namespace Racer {
// 0.83 targeted checks, added to the 0.80 runner (same muted isolated save, same helpers): PROBE_CASES="case:args;...".
public sealed partial class Report080Checks {
 IEnumerator Cases083(string[] a)=>a[0] switch{"tris"=>Tris(a[1],a[2]),"contacts"=>Contacts(a),"racelog"=>RaceLog(a),"people83"=>People083(),"tracks"=>TracksCheck(),"winner83"=>Winner083(a[1],a[2],int.Parse(a[3])),"pov"=>PovCheck(a[1]),"records83"=>Records083(),_=>null};
 // The drive trace (see Drive) with every body contact logged (collider, point in car space, normal, impulse):
 // contacts:Scene:profile:Main:from:seconds:speed
 IEnumerator Contacts(string[] a){var log=new List<string>{"t,x,y,z,collider,lx,ly,lz,nx,ny,nz,impulse"};
  IEnumerator Watch(){float t0=Time.realtimeSinceStartup;while(Time.realtimeSinceStartup-t0<120){Bind();if(race&&race.vehicle&&flow.State==RaceFlow.Stage.Racing&&!race.vehicle.GetComponent<ContactLog083>()){race.vehicle.gameObject.AddComponent<ContactLog083>().log=log;yield break;}yield return null;}}
  StartCoroutine(Watch());yield return Case(new[]{"drive",a[1],a[2],a[3],a[4],a[5],a[6]});
  File.WriteAllLines($"{output}/contacts-{a[1]}-{a[2]}-{a[4]}-{a[6]}.csv",log);Note($"contacts written ({log.Count-1})");}
 sealed class ContactLog083:MonoBehaviour{public List<string> log;void OnCollisionEnter(Collision c)=>Add(c);void OnCollisionStay(Collision c)=>Add(c);
  void Add(Collision c){var p0=transform.position;foreach(var p in c.contacts){var lp=transform.InverseTransformPoint(p.point);log.Add($"{Time.time:F3},{p0.x:F2},{p0.y:F2},{p0.z:F2},{c.collider.name},{lp.x:F2},{lp.y:F2},{lp.z:F2},{p.normal.x:F2},{p.normal.y:F2},{p.normal.z:F2},{c.impulse.magnitude:F1}");}}}
 // carrace (see CarRace) with every reset logged: where the racer was 0.5 s before (main-road station) and where it
 // was put back. racelog:Scene:player:ai1,ai2,ai3
 IEnumerator RaceLog(string[] a){var log=new List<string>();bool hooked=false;
  IEnumerator Watch(){float t0=Time.realtimeSinceStartup;while(!hooked&&Time.realtimeSinceStartup-t0<120){Bind();if(race&&flow.State==RaceFlow.Stage.Racing&&race.Racers.Count>1){hooked=true;
     foreach(var r in race.Racers){var rr=r;var trail=new Queue<Vector3>();var resp=r.Car.GetComponent<VehicleRespawn>();
      resp.Respawned+=()=>{var was=trail.Count>0?trail.Peek():rr.Car.Body.position;log.Add($"{Time.time:F1} {rr.Name} reset: was at {V(was)} (station {race.road.Project(was,out _):F0}), put at {V(rr.Car.Body.position)}");};
      StartCoroutine(Trail(rr,trail));}}yield return null;}}
  IEnumerator Trail(RacerState r,Queue<Vector3> q){while(r!=null&&r.Car){q.Enqueue(r.Car.Body.position);while(q.Count>25)q.Dequeue();yield return new WaitForFixedUpdate();}}
  StartCoroutine(Watch());yield return Case(new[]{"carrace",a[1],a[2],a[3]});foreach(var l in log)Note("  "+l);}
 // Part C: the three characters close up - football (all three), coffee (two), the sled in Snow - and their looks as
 // built (shirt / emblem / hair parts present). people83
 IEnumerator People083(){yield return EnterRoam("StreetLoopGreybox","moto");yield return new WaitForSeconds(1);
  var life=FindAnyObjectByType<AmbientLife>();var cam=Camera.main;var chase=FindAnyObjectByType<ChaseCamera>();if(chase)chase.enabled=false;var cv=CameraViews.Current;if(cv)cv.enabled=false;
  WorldLook.Current?.Pin(LookPresets.Compose(TimeOfDay.Day,Weather.Clear));
  Vector3 Mid(Vector3[] a)=>a.Aggregate(Vector3.zero,(x,y)=>x+y)/a.Length;
  string Parts(ScenePerson p)=>string.Join(" ",new[]{"Shirt_","Emblem_","Hair_","Hat_"}.Select(k=>k+string.Join("+",p.GetComponentsInChildren<Renderer>().Where(r=>r.name.Contains("_"+k)).Select(r=>r.name.Split('_')[2]).Distinct())));
  foreach(var (want,name,spots) in new[]{(0,"football",life.football),(1,"coffee",life.coffee)}){
   for(int seed=1;seed<200;seed++){AmbientLife.ForcedSeed=seed;life.SelectScenes();if(life.DanScene==want)break;}
   var c=Mid(spots)+Vector3.up*1.2f;var people=FindObjectsByType<ScenePerson>(FindObjectsSortMode.None).Where(p=>p.isActiveAndEnabled&&(p.transform.position-c).magnitude<15).ToArray();
   bool dan=people.Any(p=>p.Who==ScenePeople.Dan&&Parts(p).Contains("Emblem_Tee")),kyle=people.Any(p=>p.Who==ScenePeople.Kyle&&Parts(p).Contains("Shirt_Leather"));
   Check(people.Length==spots.Length&&dan&&kyle,$"{name}: {string.Join("; ",people.Select(p=>p.Who.Name+" ["+Parts(p)+"]"))}");
   var dir=spots.Length>1?Vector3.Cross(Vector3.up,(spots[1]-spots[0]).normalized):Vector3.forward;
   foreach(var side in new[]{1,-1}){var from=c+dir*side*(name=="football"?6f:3.6f)+Vector3.up*.5f;Park(c+dir*side*14+Vector3.Cross(Vector3.up,dir)*8);
    yield return new WaitForSeconds(1.5f);int sd=side;yield return Late(()=>{cam.transform.position=from;cam.transform.LookAt(c-Vector3.up*.2f);Shot($"C-people-{name}-{(sd>0?"a":"b")}",1920,1080);});}}
  AmbientLife.ForcedSeed=0;
  WorldLook.Current?.Pin(LookPresets.Compose(TimeOfDay.Day,Weather.Snow));yield return new WaitForSeconds(2);
  var snow=FindAnyObjectByType<SnowScenes>();
  if(snow){Park(snow.SledPosition+new Vector3(14,0,0));yield return new WaitForSeconds(1);var who=FindObjectsByType<ScenePerson>(FindObjectsSortMode.None).Where(p=>p.isActiveAndEnabled&&(p.transform.position-snow.SledPosition).magnitude<8).ToArray();
   Check(snow.SledActive&&who.Length>=2,$"sled: {string.Join("; ",who.Select(p=>p.Who.Name+" ["+Parts(p)+"]"))}");
   var sp=snow.SledPosition;yield return Late(()=>{cam.transform.position=sp+new Vector3(3,1.6f,1.2f);cam.transform.LookAt(sp+Vector3.up*.8f);Shot("C-people-sled",1920,1080);});
   Park(snow.BallPosition+new Vector3(-14,0,0));yield return new WaitForSeconds(2.5f);var bp=snow.BallPosition;yield return Late(()=>{cam.transform.position=bp+new Vector3(-5,2.2f,0);cam.transform.LookAt(bp+Vector3.up*.9f);Shot("C-people-hockey",1920,1080);});}
  WorldLook.Current?.Pin(LookPresets.Compose(TimeOfDay.Day,Weather.Clear));if(chase)chase.enabled=true;if(cv)cv.enabled=true;}
 T Field<T>(object o,string name)=>(T)o.GetType().GetField(name,BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public).GetValue(o);
 void Invoke(object o,string name,params object[] args)=>o.GetType().GetMethod(name,BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public).Invoke(o,args);
 GameObject Button(string id)=>FindObjectsByType<UnityEngine.UI.Button>(FindObjectsInactive.Exclude,FindObjectsSortMode.None).Select(b=>b.gameObject).FirstOrDefault(g=>g.name==id);

 // Part D: Tracks shows the map beside the list and follows the highlight: a Street Loop row, a Mountain row, Back (the
 // plain map); no preview row; the worst frame while the highlight runs through all eight rows one per frame. tracks
 IEnumerator TracksCheck(){yield return Load("StreetLoopGreybox");yield return Menu();var menus=flow.GetComponent<RaceMenus>();flow.OpenCourses();yield return new WaitForSecondsRealtime(.6f);
  var panel=Named("Course map panel");bool previewRow=Button("preview-track");
  foreach(var (course,name) in new[]{(0,"street"),(5,"mountain-reverse")}){EventSystem.current.SetSelectedGameObject(Button("course-"+course));yield return new WaitForSecondsRealtime(.5f);
   var caption=panel?panel.GetComponentsInChildren<UnityEngine.UI.Text>().FirstOrDefault(t=>t.name=="Course"):null;
   Check(panel&&panel.activeInHierarchy&&menus.ShownCourse==course&&!previewRow,$"Tracks, {RacePlaylists.Titles[course]} highlighted: map panel shown {panel&&panel.activeInHierarchy}, map on course {menus.ShownCourse}, preview row present {previewRow}; caption \"{caption?.text.Replace("\n"," | ")}\"");
   yield return Late(()=>Shot("D-tracks-"+name,1920,1080));}
  float worst=0;for(int k=0;k<3;k++)foreach(int c in RacePlaylists.DisplayOrder){EventSystem.current.SetSelectedGameObject(Button("course-"+c));yield return null;worst=Mathf.Max(worst,Time.unscaledDeltaTime);}
  EventSystem.current.SetSelectedGameObject(Button("back"));yield return new WaitForSecondsRealtime(.3f);
  Check(menus.ShownCourse==-1,$"Back highlighted: plain map ({menus.ShownCourse}); worst frame while moving one row per frame through the list 3 times: {worst*1000:F0} ms");
  yield return Late(()=>Shot("D-tracks-back",1920,1080));
  flow.CloseGarage();yield return null;}

 // Part F: win (or lose to an AI) with the given camera view (0 chase, 2 first person): during the winner shot the
 // player's head parts are drawn (not shadows-only), after it the view's own hiding is back. winner83:player|ai:scene:view
 IEnumerator Winner083(string who,string scene,int view){yield return Load(scene);yield return Menu();var s=flow.Save.Settings;s.weather=0;s.timeOfDay=0;s.estimateAiFinishes=false;s.cameraView=view;flow.Save.SaveSettings();
  flow.OpenGarage();flow.SelectVehicle("moto");flow.CloseGarage();race.opponents=true;race.traffic=false;race.laps=1;race.difficulty=who=="player"?0:2;race.opponentRoster=new[]{"moto","atv","original"};
  flow.StartRace();float t0=Time.realtimeSinceStartup;while(flow.State!=RaceFlow.Stage.Racing&&Time.realtimeSinceStartup-t0<30){AudioListener.volume=0;yield return null;}
  var car=race.vehicle;var pilot=car.gameObject.AddComponent<RoadDriver>();pilot.Initialize(race,car,true,1,who=="player"?1.15f: .55f);pilot.Racer=race.Racers[0];car.GetComponent<VehicleInput>().enabled=false;
  yield return new WaitForSeconds(3);bool hiddenRacing=CameraViews.Current.HeadHidden;
  float t1=Time.time;while(!race.Progress.Finished&&Time.time-t1<600){AudioListener.volume=0;yield return null;}
  yield return null;var winner=race.Racers.Where(r=>r.Progress.Finished).OrderBy(r=>r.Progress.RaceTime(race.Clock)).First();
  yield return new WaitForSecondsRealtime(1.1f);bool active=WinnerShot.Active;
  int Drawn(ArcadeVehicle v)=>v.GetComponentsInChildren<Renderer>().Count(r=>(r.name.Contains("_Hair")||r.name.Contains("_Hat_")||(r.name.Contains("_Base_")&&r.name.EndsWith("__eyes")))&&r.enabled&&r.shadowCastingMode!=UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly);
  int Head(ArcadeVehicle v)=>v.GetComponentsInChildren<Renderer>().Count(r=>r.name.Contains("_Hair")||r.name.Contains("_Hat_")||(r.name.Contains("_Base_")&&r.name.EndsWith("__eyes")));
  int drawn=Drawn(winner.Car),parts=Head(winner.Car),playerDrawn=Drawn(car),playerParts=Head(car);bool hiddenShot=CameraViews.Current.HeadHidden;
  yield return Late(()=>Shot($"F-winner-{who}-view{view}",1920,1080));
  float t2=Time.realtimeSinceStartup;while(WinnerShot.Active&&Time.realtimeSinceStartup-t2<5)yield return null;yield return new WaitForSecondsRealtime(.6f);bool hiddenAfter=CameraViews.Current.HeadHidden;
  Check(active&&(who=="player")==!winner.IsAi&&drawn==parts&&parts>0&&playerDrawn==playerParts&&!hiddenShot&&hiddenRacing==(view==2)&&hiddenAfter==(view==2&&CameraViews.Current.ShownView=="First person"),
   $"{who} win, view {CameraViews.Names[view]}: winner {winner.Name}; head parts drawn in the shot {drawn} of {parts} (player {playerDrawn} of {playerParts}); hidden while racing {hiddenRacing}, during the shot {hiddenShot}, after {hiddenAfter} (view shown {CameraViews.Current.ShownView}, state {flow.State})");
  s.cameraView=0;flow.Save.SaveSettings();Destroy(pilot);car.GetComponent<VehicleInput>().enabled=true;yield return Menu();}

 // Part F: cycle the four views (V) several times while riding; every rendered frame the head is hidden exactly when the
 // camera is inside it (within 17 cm of the head centre). pov:profile
 IEnumerator PovCheck(string profile){yield return EnterRoam("StreetLoopGreybox",profile);yield return new WaitForSeconds(1);
  var car=race.vehicle;var views=CameraViews.Current;flow.Save.Settings.cameraView=0;flow.Save.SaveSettings();yield return new WaitForSeconds(.5f);
  var eyes=car.GetComponentsInChildren<Renderer>(true).Where(r=>r.name.EndsWith("__eyes")&&r.name.Contains("_Base_")).ToArray();
  int frames=0,bald=0,inside=0,hiddenFrames=0;float nearestShown=99,farthestHidden=0;var cam=Camera.main;
  for(int k=0;k<16;k++){views.NextPlayerView();for(int f=0;f<40;f++){yield return Late(()=>{
    var c=eyes.Aggregate(Vector3.zero,(a,r)=>a+r.bounds.center)/Mathf.Max(1,eyes.Length)-car.transform.forward*.09f;float d=(cam.transform.position-c).magnitude;frames++;
    bool hid=views.HeadHidden;if(hid){hiddenFrames++;farthestHidden=Mathf.Max(farthestHidden,d);if(d>.2f)bald++;}else{nearestShown=Mathf.Min(nearestShown,d);if(d<.14f)inside++;}});}}
  Check(frames>0&&bald==0&&inside==0&&hiddenFrames>0,$"{profile}: 16 view changes, {frames} frames: head hidden in {hiddenFrames} (camera at most {farthestHidden:F2} m from the head centre), shown with the camera inside the head {inside}, hidden with the camera outside it (a bald head) {bald}; nearest shown {nearestShown:F2} m");
  flow.Save.Settings.cameraView=0;flow.Save.SaveSettings();}

 // Part G: a records fixture in the isolated save (laps and races on Street Loop Forward with three vehicles incl. a 0.81
 // one, a legacy lap, a Mountain Loop Reverse lap), then Records from the main menu: a filled board at once on the track last
 // raced, the track control steps through all eight (empty ones say so in one line), RACE shows the laps column.
 IEnumerator Records083(){yield return Load("StreetLoopGreybox");yield return Menu();var menus=flow.GetComponent<RaceMenus>();var courses=CoursePreviewCatalog.Courses;
  string street=courses[0].id,mountain=courses[5].id;int n=0;
  void Lap(string course,string v,double t,bool legacy=false)=>flow.Boards.Add("fixture-"+(n++),$"{course}-{v}-solo-clear",false,t,v,DateTime.UtcNow.AddDays(-n).ToString("o"),legacy);
  void Race(string course,string v,double t,int laps)=>flow.Boards.Add("fixture-"+(n++),$"{course}-{v}-race4-d1-moto-atv-drifter-clear-laps{laps}",true,t,v,DateTime.UtcNow.AddDays(-n).ToString("o"));
  Lap(street,"moto",61.2);Lap(street,"drifter",62.9);Lap(street,"roadster",66.4);Lap(street,"moto",60.5);Lap("street-v4-old","original",70.1,true);Lap(mountain,"scrambler",118.3);
  Race(street,"moto",185.0,3);Race(street,"drifter",64.0,1);
  flow.OpenBoards();yield return new WaitForSecondsRealtime(.6f);
  string Details()=>Field<UnityEngine.UI.Text>(menus,"details").text.Replace("\n"," | ");
  int Rows()=>FindObjectsByType<UnityEngine.UI.Button>(FindObjectsInactive.Exclude,FindObjectsSortMode.None).Count(b=>b.name.StartsWith("record-fixture"));
  string Track()=>Button("record-track")?.GetComponentInChildren<UnityEngine.UI.Text>(true).text;
  int first=Rows();string firstTrack=Track(),firstDetails=Details();
  Check(first==5&&firstTrack!=null&&firstTrack.Contains("Street Loop"),$"Records opened: {first} rows on \"{firstTrack}\" ({firstDetails})");
  yield return Late(()=>Shot("G-records-filled",1920,1080));
  var seen=new List<string>();int empties=0;string emptyShot=null;
  for(int i=0;i<8;i++){Invoke(menus,"StepRecordTrack",1);yield return new WaitForSecondsRealtime(.3f);int rows=Rows();string t=Track(),d=Details();seen.Add($"{t}: {rows} rows; {d}");
   if(rows==0){empties++;if(!d.StartsWith("No lap times on")){Check(false,"empty board without a plain explanation: "+d);}if(emptyShot==null){emptyShot=t;yield return Late(()=>Shot("G-records-empty",1920,1080));}}}
  foreach(var l in seen)Note("  "+l);
  Check(seen.Count==8&&empties>=5,$"track control stepped through {seen.Count} tracks, {empties} explained empty boards");
  Field<int>(menus,"recordTab");typeof(RaceMenus).GetField("recordTab",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(menus,1);menus.Show();yield return new WaitForSecondsRealtime(.4f);
  int races=Rows();Check(races==2&&Details().StartsWith("BEST RACES"),$"RACE tab on \"{Track()}\": {races} rows ({Details()})");yield return Late(()=>Shot("G-records-races",1920,1080));
  flow.CloseExtras();yield return null;}
 // Every mesh-collider triangle within r of a point that is not a flat floor (normal y < 0.98), with its corners: tris:Scene:x,y,z,r
 IEnumerator Tris(string scene,string spec){yield return EnterScene(scene);yield return new WaitForSeconds(.5f);var f=spec.Split(',').Select(F).ToArray();var c=new Vector3(f[0],f[1],f[2]);float r=f[3];
  var sb=new StringBuilder();
  foreach(var mc in FindObjectsByType<MeshCollider>(FindObjectsSortMode.None)){if(!mc.enabled||!mc.sharedMesh||!mc.sharedMesh.isReadable||mc.bounds.SqrDistance(c)>r*r)continue;var m=mc.sharedMesh;var v=m.vertices;var t=m.triangles;var tr=mc.transform;int n=0,all=0;
   for(int i=0;i<t.Length;i+=3){var p0=tr.TransformPoint(v[t[i]]);var p1=tr.TransformPoint(v[t[i+1]]);var p2=tr.TransformPoint(v[t[i+2]]);var cen=(p0+p1+p2)/3;if((cen-c).sqrMagnitude>r*r)continue;all++;
    var nn=Vector3.Cross(p1-p0,p2-p0).normalized;if(Mathf.Abs(nn.y)>.98f)continue;n++;sb.AppendLine($"{P(mc.transform)} n {nn.x:F2},{nn.y:F2},{nn.z:F2} | {V(p0)} | {V(p1)} | {V(p2)}");}
   sb.AppendLine($"== {P(mc.transform)}: {all} triangles near, {n} not flat");}
  File.WriteAllText($"{output}/tris-{Scene}-{c.x:F0}-{c.z:F0}.txt",sb.ToString());Note($"{Scene}: triangles near {V(c)} written");if(scene!=RaceFlow.RoamScene)yield return Menu();}
}
}
#endif
