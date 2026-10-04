#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Racer {
// 0.75 targeted checks (muted, isolated save). PROBE_CASES="case:args;..." (Street Loop scene):
//  models                     Turntable renders of every vehicle, classic and new (garage-style isolated render, layer 31),
//                             with triangles, renderers, materials, bounds and wheel pivots of each.
//  garage                     Each vehicle in the garage: new model and rider present, wheels, paint on the bodywork only
//                             (Black and Red), rider untouched by paint; Classic / New both ways for every vehicle; saved.
//  rider                      Rider page: every row changes the preview and is saved; every hat with every hair style (no
//                             hair above the band under a hat); Randomize; persistence (relaunch reads it); the page with
//                             Classic models; preview shots on each vehicle.
//  race:player:ai,ai,ai:tod   One lap with the player's vehicle driven by the AI pilot and that AI roster at Day / Night:
//                             new models on every racer, varied AI riders (never the player's look), lamps, shots of the grid,
//                             chase, the player close up (the car cabin) and an AI; wipe-out (small vehicles) and reset;
//                             finish, results and record.
public sealed class Report075Checks:MonoBehaviour {
 RaceDirector race;RaceFlow flow;RaceMenus menus;readonly List<string> rows=new();string output,saveDir;int fails;
 void Note(string s){rows.Add(s);File.WriteAllLines(output+"/results.txt",rows);Debug.Log("REPORT075 "+s);}
 void Check(bool ok,string s){if(!ok)fails++;Note((ok?"PASS ":"FAIL ")+s);}
 static readonly string[] Ids={"moto","atv","original","tourer"};
 IEnumerator Start(){AudioListener.volume=0;var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;output=Environment.GetEnvironmentVariable("PROBE_OUT")+"/checks-"+scene;Directory.CreateDirectory(output);Application.runInBackground=true;
  race=FindAnyObjectByType<RaceDirector>();yield return null;yield return null;flow=race.Flow;menus=FindAnyObjectByType<RaceMenus>();
  saveDir=Path.GetFullPath("Temp/Report075Save");if(Directory.Exists(saveDir))Directory.Delete(saveDir,true);flow.UseValidationSave(saveDir);
  var title=FindAnyObjectByType<StartupTitle>();if(title)Destroy(title.gameObject);typeof(StartupTitle).GetField("completed",BindingFlags.Static|BindingFlags.NonPublic).SetValue(null,true);flow.EnterMenuAfterTitle();
  yield return null;
  foreach(var spec in (Environment.GetEnvironmentVariable("PROBE_CASES")??"").Split(';').Where(x=>x.Length>0)){var a=spec.Split(':');
   IEnumerator run=a[0] switch{"models"=>Models(),"garage"=>Garage(),"rider"=>RiderPage(),"race"=>Race(a[1],a[2].Split(','),(TimeOfDay)Enum.Parse(typeof(TimeOfDay),a[3])),_=>null};
   if(run!=null)yield return run;else Note("unknown case "+spec);}
  Note($"done, failures {fails}");File.WriteAllLines(output+"/done.txt",rows);UnityEditor.EditorApplication.Exit(0);}
 IEnumerator ToMenu(){if(flow.State!=RaceFlow.Stage.Ready){flow.Pause();flow.QuitRace();}yield return null;yield return null;}
 ArcadeVehicle Car=>race.vehicle;
 static bool HasNew(Transform t,string id)=>t.GetComponentsInChildren<Transform>(true).Any(x=>x.name==VehicleProfile.Find(id).Name+" model"&&x.gameObject.activeInHierarchy);
 static Transform RiderOf(Transform t)=>t.GetComponentsInChildren<Transform>(true).FirstOrDefault(x=>x.name=="Rider"&&x.parent&&x.parent.name=="Steering pose");
 static string Parts(Transform rider)=>rider?string.Join(" ",rider.Cast<Transform>().Select(x=>x.name+"="+x.GetComponent<Renderer>().sharedMaterial.name+"#"+ColorUtility.ToHtmlStringRGB(x.GetComponent<Renderer>().sharedMaterial.color))):"";
 static string Full(Transform t){var n=t.name;while(t.parent){t=t.parent;n=t.name+"/"+n;}return n;}
 static int Tris(Transform t){int n=0;foreach(var mf in t.GetComponentsInChildren<MeshFilter>())if(mf.sharedMesh&&mf.gameObject.activeInHierarchy)for(int s=0;s<mf.sharedMesh.subMeshCount;s++)n+=(int)mf.sharedMesh.GetIndexCount(s)/3;return n;}
 // Scene shots through the main camera (URP post as in the game); the chase camera is paused and restored afterwards.
 IEnumerator Shot(string name,Vector3 eye,Vector3 target,int w=1600,int h=900){
  var cam=Camera.main;var chase=FindObjectsByType<ChaseCamera>(FindObjectsSortMode.None);foreach(var c in chase)c.enabled=false;
  var old=(cam.transform.position,cam.transform.rotation,cam.fieldOfView);cam.transform.SetPositionAndRotation(eye,Quaternion.LookRotation(target-eye));cam.fieldOfView=50;
  float ts=Time.timeScale;Time.timeScale=0;yield return null;yield return null;Time.timeScale=ts;
  var rt=new RenderTexture(w,h,24,RenderTextureFormat.ARGB32){antiAliasing=4};var prev=cam.targetTexture;cam.targetTexture=rt;cam.Render();cam.targetTexture=prev;
  Save(rt,name);cam.transform.SetPositionAndRotation(old.Item1,old.Item2);cam.fieldOfView=old.Item3;foreach(var c in chase)c.enabled=true;}
 void Save(RenderTexture rt,string name){RenderTexture.active=rt;var tex=new Texture2D(rt.width,rt.height,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,rt.width,rt.height),0,0);tex.Apply();RenderTexture.active=null;rt.Release();
  File.WriteAllBytes(Path.Combine(output,name),name.EndsWith(".jpg")?tex.EncodeToJPG(90):tex.EncodeToPNG());Destroy(tex);}
 IEnumerator GarageShot(string name){yield return null;yield return null;var rt=typeof(RaceMenus).GetField("previewTexture",BindingFlags.NonPublic|BindingFlags.Instance)?.GetValue(menus) as RenderTexture;
  var cam=typeof(RaceMenus).GetField("previewCamera",BindingFlags.NonPublic|BindingFlags.Instance)?.GetValue(menus) as Camera;if(!rt||!cam){Note("no garage preview");yield break;}
  cam.Render();RenderTexture.active=rt;var tex=new Texture2D(rt.width,rt.height,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,rt.width,rt.height),0,0);tex.Apply();RenderTexture.active=null;File.WriteAllBytes(Path.Combine(output,name),tex.EncodeToPNG());Destroy(tex);}
 // Menu shots (batch mode has no screen capture): the menu canvas is drawn through the main camera into a 1920x1080 image.
 IEnumerator ScreenShot(string name){
  var canvas=FindAnyObjectByType<RaceHud>().GetComponent<Canvas>();var cam=Camera.main;var rt=new RenderTexture(1920,1080,24,RenderTextureFormat.ARGB32){antiAliasing=4};
  var mode=canvas.renderMode;var prev=cam.targetTexture;cam.targetTexture=rt;canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=cam;canvas.planeDistance=cam.nearClipPlane+.1f;
  yield return null;yield return null;Canvas.ForceUpdateCanvases();cam.Render();cam.targetTexture=prev;canvas.renderMode=mode;Save(rt,name);yield return null;}
 Transform PreviewRoot()=>(typeof(RaceMenus).GetField("previewRoot",BindingFlags.NonPublic|BindingFlags.Instance)?.GetValue(menus) as GameObject)?.transform;
 UnityEngine.UI.Button Button(Func<UnityEngine.UI.Button,bool> f)=>FindObjectsByType<UnityEngine.UI.Button>(FindObjectsSortMode.None).FirstOrDefault(b=>b.gameObject.activeInHierarchy&&b.interactable&&f(b));
 static string Text(UnityEngine.UI.Button b)=>b?b.GetComponentInChildren<UnityEngine.UI.Text>(true).text:"(none)";
 IEnumerator Click(UnityEngine.UI.Button b){MenuInput.ConsumeThroughRelease();for(int i=0;i<4;i++)yield return null;b.onClick.Invoke();for(int i=0;i<3;i++)yield return null;}

 // ---------- turntables: classic and new ----------
 IEnumerator Turntable(string id,string tag){
  var root=new GameObject("Model sheet root");root.layer=31;root.transform.position=new Vector3(10000,10000,10000);
  var p=VehicleProfile.Find(id);VehicleVisual.Build(root.transform,p);VehiclePaint.Apply(root.transform,VehiclePaint.Colors[3]);
  var cam=new GameObject("Model sheet camera").AddComponent<Camera>();cam.cullingMask=1<<31;cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=new Color(.62f,.68f,.76f);cam.fieldOfView=30;cam.nearClipPlane=.05f;cam.enabled=false;
  float k=p.Small?1:1.55f;var c=root.transform.position+Vector3.up*.25f;
  foreach(var (name,eye) in new[]{("front",new Vector3(0,.45f,4.6f)),("side",new Vector3(4.8f,.35f,0)),("three-quarter",new Vector3(3.3f,1.3f,3.3f)),("top",new Vector3(0,5.2f,-.01f))}){
   cam.transform.position=root.transform.position+eye*k;cam.transform.LookAt(c);var rt=new RenderTexture(1000,760,24){antiAliasing=4};cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;var tex=new Texture2D(1000,760,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,1000,760),0,0);tex.Apply();RenderTexture.active=null;cam.targetTexture=null;rt.Release();
   File.WriteAllBytes(Path.Combine(output,$"unity-{id}-{tag}-{name}.png"),tex.EncodeToPNG());Destroy(tex);yield return null;}
  var rs=root.GetComponentsInChildren<Renderer>();var b=rs[0].bounds;foreach(var r in rs)b.Encapsulate(r.bounds);
  var wheels=root.GetComponentsInChildren<Transform>().Where(t=>t.name.StartsWith("Wheel")||t.name.EndsWith("wheel")).Select(t=>$"{t.name}@{root.transform.InverseTransformPoint(t.position):F3}").Distinct();
  Note($"MODEL {id} {tag}: triangles {Tris(root.transform)}, renderers {rs.Length}, materials {rs.Select(r=>r.sharedMaterial.name).Distinct().Count()}; bounds min {root.transform.InverseTransformPoint(b.min):F2} max {root.transform.InverseTransformPoint(b.max):F2}; wheels {string.Join(" ",wheels)}");
  Destroy(cam.gameObject);Destroy(root);yield return null;}
 IEnumerator Models(){yield return ToMenu();foreach(var id in Ids){VehicleVisual.NewModels=false;yield return Turntable(id,"classic");VehicleVisual.NewModels=true;yield return Turntable(id,"new");}}

 // ---------- garage: every vehicle ----------
 IEnumerator Garage(){
  yield return ToMenu();var s=flow.Save.Settings;Check(s.newMotorcycle&&RiderLook.Player.SameAs(new RiderLook()),"defaults: Model New, rider = the 0.73 identity (man, short dark-brown hair, medium skin, flat cap, blue T-shirt, jeans)");
  flow.OpenGarage();yield return null;
  foreach(var id in Ids){
   flow.SelectVehicle(id);yield return null;yield return null;var t=Car.transform;var p=VehicleProfile.Find(id);
   var rider=RiderOf(t);int wheels=t.GetComponentsInChildren<Transform>().Count(x=>x.name.StartsWith("Wheel")&&x.GetComponentInChildren<MeshRenderer>()&&x.parent&&(x.parent.name=="Vehicle visual"||x.parent.name=="Front end"));
   var box=Car.GetComponent<BoxCollider>();
   Check(HasNew(t,id)&&rider&&rider.childCount>=8&&wheels==(id=="moto"?2:4),$"{p.Name}: new model {HasNew(t,id)}, rider parts {rider?.childCount}, wheel pivots {wheels}; triangles {Tris(t.Find("Vehicle visual"))}; collider {box.size:F2} centre {box.center:F2}; camera offset {p.Camera}");
   var row=Button(b=>Text(b).StartsWith("Model:"));var riderRow=Button(b=>Text(b).StartsWith("Rider"));Check(row&&riderRow,$"{p.Name}: garage rows '{Text(row)}' and '{Text(riderRow)}'");
   foreach(int colour in new[]{6,1}){
    var before=VehiclePaint.UnpaintedSnapshot(t);flow.SetColor(colour);yield return null;
    var painted=t.GetComponentsInChildren<Renderer>().Where(r=>VehiclePaint.IsBodyPaint(r.sharedMaterial)).ToArray();var blk=new MaterialPropertyBlock();painted[0].GetPropertyBlock(blk);
    bool riderClean=rider.GetComponentsInChildren<Renderer>().All(r=>!VehiclePaint.IsBodyPaint(r.sharedMaterial));
    Check(painted.Length>=1&&blk.GetColor("_BaseColor")==VehiclePaint.Colors[colour]&&riderClean&&VehiclePaint.UnpaintedUnchanged(t,before),$"{p.Name} colour {VehiclePaint.Names[colour]}: bodywork renderers painted {painted.Length} ({blk.GetColor("_BaseColor")}); every other renderer unchanged; rider keeps its own materials {riderClean}");
    yield return GarageShot($"garage-{id}-{VehiclePaint.Names[colour].ToLowerInvariant()}.png");}
   flow.SetColor(3);yield return null;
   flow.ToggleModel();yield return null;bool classic=!HasNew(Car.transform,id)&&!s.newMotorcycle&&!new RacerSave(saveDir,"street-loop-gates-v1-laps3").Settings.newMotorcycle;
   yield return GarageShot($"garage-{id}-classic.png");
   flow.ToggleModel();yield return null;bool back=HasNew(Car.transform,id)&&s.newMotorcycle&&new RacerSave(saveDir,"street-loop-gates-v1-laps3").Settings.newMotorcycle;
   Check(classic&&back,$"{p.Name}: Model Classic (classic model, saved) {classic}, then New again (saved) {back}");
   yield return GarageShot($"garage-{id}-new.png");}
  flow.SelectVehicle("moto");flow.CloseGarage();yield return null;}

 // ---------- rider page ----------
 IEnumerator RiderPage(){
  yield return ToMenu();flow.OpenGarage();flow.SelectVehicle("moto");yield return null;
  var open=Button(b=>Text(b).StartsWith("Rider"));yield return Click(open);
  var rows=Enumerable.Range(0,RiderLook.Fields).Select(i=>Button(b=>b.name=="rider-"+i)).ToArray();var rnd=Button(b=>b.name=="randomize");
  Check(rows.All(r=>r)&&rnd,$"Rider page open: {rows.Count(r=>r)} option rows ({string.Join(" | ",rows.Select(Text))}), Randomize {Text(rnd)}");
  yield return ScreenShot("rider-page.png");yield return GarageShot("rider-page-preview-moto.png");
  // every row changes the preview and is saved
  for(int i=0;i<RiderLook.Fields;i++){
   var before=Parts(RiderOf(PreviewRoot()));var look0=flow.Rider.Copy();var row=Button(b=>b.name=="rider-"+i);string label0=Text(row);
   yield return Click(row);var after=Parts(RiderOf(PreviewRoot()));row=Button(b=>b.name=="rider-"+i);
   var saved=new RacerSave(saveDir,"street-loop-gates-v1-laps3").Settings.rider;
   Check(after!=before&&!flow.Rider.SameAs(look0)&&saved.SameAs(flow.Rider)&&RiderOf(Car.transform)&&Parts(RiderOf(Car.transform))==after,$"row {i}: '{label0}' -> '{Text(row)}': preview changed {after!=before}, saved {saved.SameAs(flow.Rider)}, player vehicle matches the preview {Parts(RiderOf(Car.transform))==after}");
   flow.StepRider(i,-1);yield return null;}
  Check(flow.Rider.SameAs(new RiderLook()),"left/right: stepping back restores the default look");
  // hats with every hair style: no hair above the band under a hat; full hair without a hat
  var look=flow.Rider;int combos=0,bad=0;
  foreach(int body in new[]{0,1})for(int hat=0;hat<RiderLook.Hats.Length;hat++)for(int hair=0;hair<RiderLook.Hairs.Length;hair++){
   look.body=body;look.hat=hat;look.hair=hair;flow.StepRider(0,0);yield return null;
   var names=RiderOf(PreviewRoot()).Cast<Transform>().Select(x=>x.name).ToArray();combos++;
   bool top=names.Any(n=>n.Contains("_HairTop_")),low=names.Any(n=>n.Contains("_Hair_")),hatOn=names.Any(n=>n.Contains("_Hat_"));
   bool ok=(hat>0?(!top&&hatOn):(!hatOn&&(hair==4?!top:top)))&&(hair==4?!low:low)&&names.Count(n=>n.Contains("_Base_"))>=5;if(!ok){bad++;Note($"combination body {body} hat {hat} hair {hair}: {string.Join(" ",names)}");}
   if(body==0&&hair<4||body==1&&hat==0)yield return GarageShot($"rider-hat{hat}-hair{hair}-{RiderLook.Bodies[body].ToLowerInvariant()}.png");}
  Check(bad==0,$"hat x hair x body: {combos} combinations, part sets correct (hair above the band hidden under every hat) {combos-bad}/{combos}");
  // Randomize
  var r0=flow.Rider.Copy();rnd=Button(b=>b.name=="randomize");yield return Click(rnd);
  Check(!flow.Rider.SameAs(r0)&&new RacerSave(saveDir,"street-loop-gates-v1-laps3").Settings.rider.SameAs(flow.Rider),$"Randomize: new look saved ({string.Join(" / ",Enumerable.Range(0,RiderLook.Fields).Select(flow.Rider.Label))})");
  // a set look, previewed on every vehicle, survives a fresh read of the settings (relaunch)
  var set=new RiderLook{body=1,skin=4,hair=3,hairColor=4,hat=2,hatColor=1,shirt=2,shirtColor=5,pants=1,pantsColor=2};
  flow.Save.Settings.rider=set;flow.StepRider(0,0);yield return null;
  var reread=new RacerSave(saveDir,"street-loop-gates-v1-laps3").Settings.rider;Check(reread.SameAs(set),"persistence: a fresh read of settings.json returns the chosen look");
  flow.Back();yield return null;flow.CloseGarage();yield return null;
  foreach(var id in Ids){
   flow.OpenGarage();flow.SelectVehicle(id);yield return null;
   open=Button(b=>Text(b).StartsWith("Rider"));yield return Click(open);yield return GarageShot($"rider-page-preview-{id}.png");yield return ScreenShot($"rider-page-{id}.png");
   Check(RiderOf(PreviewRoot())&&Parts(RiderOf(PreviewRoot()))==Parts(RiderOf(Car.transform)),$"{VehicleProfile.Find(id).Name}: Rider page preview shows the chosen rider in the {(id=="moto"?"Moto":id=="atv"?"Atv":"Car")} pose ({RiderOf(PreviewRoot())?.childCount} parts)");
   flow.Back();yield return null;flow.CloseGarage();yield return null;}
  // Classic: the old rider, and the page says customization needs the New models
  flow.OpenGarage();flow.SelectVehicle("moto");yield return null;
  flow.ToggleModel();yield return null;open=Button(b=>Text(b).StartsWith("Rider"));yield return Click(open);
  bool noRows=!Button(b=>b.name=="rider-0");var details=FindObjectsByType<UnityEngine.UI.Text>(FindObjectsSortMode.None).Any(x=>x.gameObject.activeInHierarchy&&x.text.Contains("needs the New models"));
  Check(noRows&&details&&!RiderOf(Car.transform),$"Classic: Rider page shows the note {details}, no option rows {noRows}, classic rider on the vehicle {!RiderOf(Car.transform)}");
  yield return ScreenShot("rider-page-classic.png");
  flow.Back();yield return null;flow.ToggleModel();yield return null;
  flow.Save.Settings.rider=new RiderLook();flow.StepRider(0,0);yield return null;flow.CloseGarage();yield return null;}

 // ---------- one race lap ----------
 IEnumerator Race(string player,string[] roster,TimeOfDay t){
  yield return ToMenu();var s=flow.Save.Settings;s.timeOfDay=(int)t;s.weather=0;s.opponentChoices=roster.ToArray();s.opponentRoster=roster.ToArray();flow.Save.SaveSettings();
  flow.OpenGarage();flow.SelectVehicle(player);flow.SetColor(player=="tourer"?6:3);flow.CloseGarage();race.opponents=true;race.opponentRoster=roster.ToArray();race.traffic=true;race.laps=1;s.estimateAiFinishes=true;
  string category=race.Category;string tag=$"{player}-{t.ToString().ToLowerInvariant()}";
  flow.StartRace();float t0=Time.realtimeSinceStartup;while(flow.State!=RaceFlow.Stage.Countdown&&flow.State!=RaceFlow.Stage.Racing&&Time.realtimeSinceStartup-t0<30)yield return null;
  yield return new WaitForSeconds(.5f);
  var ais=race.Racers.Where(r=>r.IsAi).Select(r=>r.Car).ToArray();var looks=ais.Select(a=>a.GetComponent<VehicleConfiguration>().riderLook).ToArray();
  bool allNew=ais.All(a=>HasNew(a.transform,a.GetComponent<VehicleConfiguration>().profileId)&&RiderOf(a.transform))&&HasNew(Car.transform,player);
  bool varied=looks.All(l=>l!=null&&!l.SameAs(RiderLook.Player))&&looks.Select(l=>l.shirtColor).Distinct().Count()==looks.Length&&looks.Select(l=>l.body).Distinct().Count()>1;
  var traffic=FindObjectsByType<VehicleConfiguration>(FindObjectsSortMode.None).Where(c=>c.name.StartsWith("Traffic")).ToArray();
  bool Classic(VehicleConfiguration c)=>!c.transform.GetComponentsInChildren<Transform>(true).Any(x=>x.name.EndsWith(" model")||x.name.Contains("__"));
  var odd=traffic.Where(c=>!Classic(c)).Select(c=>Full(c.transform.GetComponentsInChildren<Transform>(true).First(x=>x.name.EndsWith(" model")||x.name.Contains("__")))).Take(3);
  Check(allNew&&varied&&ais.Length==3&&traffic.All(Classic),$"{tag}: new models on the player and {ais.Length} AI ({string.Join(", ",ais.Select(a=>a.GetComponent<VehicleConfiguration>().Profile.Name))}); AI riders varied and not the player's: {varied}; traffic classic {traffic.Count(Classic)}/{traffic.Length} {string.Join(" ",odd)}");
  for(int i=0;i<ais.Length;i++)Note($"  {ais[i].name} ({ais[i].GetComponent<VehicleConfiguration>().Profile.Name}): {string.Join(" / ",Enumerable.Range(0,RiderLook.Fields).Select(looks[i].Label))}");
  // the grid from the front
  var fwd=Car.transform.forward;var centre=race.Racers.Select(r=>r.Car.transform.position).Aggregate(Vector3.zero,(a,b)=>a+b)/race.Racers.Count;
  yield return Shot($"race-{tag}-grid.jpg",centre+fwd*9+Vector3.up*2.2f,centre+Vector3.up*.6f);
  while(flow.State==RaceFlow.Stage.Countdown)yield return null;
  var car=Car;var pilot=car.gameObject.AddComponent<RoadDriver>();pilot.Initialize(race,car,true,1,1);pilot.Racer=race.Racers[0];car.GetComponent<VehicleInput>().enabled=false;
  yield return new WaitForSeconds(5);
  var camT=Camera.main.transform;yield return Shot($"race-{tag}-chase.jpg",camT.position,camT.position+camT.forward*10);
  bool small=car.GetComponent<VehicleConfiguration>().Profile.Small;
  var side=car.transform.position-car.transform.right*(small?3.2f:3.6f)+car.transform.forward*(small?.3f:.2f)+Vector3.up*(small?.7f:.9f);
  yield return Shot($"race-{tag}-player-side.jpg",side,car.transform.position+Vector3.up*(small?.5f:.55f));
  var ai0=ais.OrderBy(a=>Vector3.Distance(a.transform.position,car.transform.position)).First().transform;
  yield return Shot($"race-{tag}-ai-side.jpg",ai0.position+ai0.right*3.5f+Vector3.up*.9f,ai0.position+Vector3.up*.45f);
  Note($"{tag}: lamps level {VehicleLights.Level:F2}");
  // wipe-out (small vehicles) and reset, mid-race (as 0.73), then the pilot finishes the lap
  pilot.enabled=false;var cfg=car.GetComponent<VehicleConfiguration>();bool wiped=false;
  if(small){car.Body.linearVelocity=car.transform.forward*2;car.Body.MoveRotation(car.Body.rotation*Quaternion.Euler(0,0,170));car.Body.angularVelocity=Vector3.zero;
   float tw=Time.time;while(Time.time-tw<4&&!cfg.WipedOut)yield return null;wiped=cfg.WipedOut;
   yield return Shot($"race-{tag}-wipeout.jpg",car.transform.position+new Vector3(3,2,3),car.transform.position);}
  var rs=car.GetComponent<VehicleRespawn>();rs.ResetVehicle();float tt=Time.time;while(Time.time-tt<3)yield return null;
  Check((!small||wiped)&&car.transform.up.y>.9f&&!cfg.WipedOut&&HasNew(car.transform,player)&&RiderOf(car.transform),$"{tag}: {(small?$"wipe-out {wiped}, then ":"")}reset: up {car.transform.up.y:F2}, wiped {cfg.WipedOut}, model and rider intact {HasNew(car.transform,player)&&RiderOf(car.transform)}");
  pilot.enabled=true;
  Time.timeScale=3;float start=Time.time;
  while(!race.Progress.Finished&&Time.time-start<400){yield return null;AudioListener.volume=0;}
  Time.timeScale=1;var p=race.Progress;
  Check(p.Finished&&p.CompletedLaps==1&&HasNew(car.transform,player),$"{tag}: lap finished {p.Finished} ({RaceHud.FormatTime(p.AdjustedTime(race.Clock))}), missed gates {p.MissedGates}, recoveries {pilot.RecoveryCount}; model intact {HasNew(car.transform,player)}");
  Destroy(pilot);car.GetComponent<VehicleInput>().enabled=true;
  if(race.opponents&&!race.ClassificationFinal)race.FinalizeUnfinishedAi();
  float wr=Time.realtimeSinceStartup;while(flow.State!=RaceFlow.Stage.Results&&Time.realtimeSinceStartup-wr<40)yield return null;
  Check(flow.State==RaceFlow.Stage.Results&&flow.Save.Best.race>0,$"{tag}: results and race record (category '{category}', best {flow.Save.Best.race:F2} s)");
  yield return ToMenu();s.timeOfDay=0;flow.Save.SaveSettings();}
}
}
#endif
