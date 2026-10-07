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
// 0.93 Parts C and D checks (controller only: an emulated gamepad; shots at 3840x2160):
//  map93        Free Roam: every acorn inside its area's region; the map opened with Back/Select, at the default zoom;
//               D-pad down through the areas (each centred and highlighted); B closes; the Exploration page's area row
//               (D-pad to it, A) opens the map on that area; the minimap's area line inside an area with acorns left and outside
//  compass93    the minimap heading north and heading east, in a race and in Free Roam, and the two split-screen minimaps
public sealed partial class Report080Checks {
 IEnumerator Walk093(string[] a)=>a[0] switch{"map93"=>Map093(),"compass93"=>Compass093(),_=>null};
 static GameObject Focus093=>EventSystem.current?EventSystem.current.currentSelectedGameObject:null;
 string Shot4k(string name)=>Shot(name,3840,2160);
 static string Text093(string name){var g=GameObject.Find(name);var t=g?g.GetComponent<UnityEngine.UI.Text>():null;return t&&t.enabled?t.text:null;}
 IEnumerator Map093(){
  // a copy of Dan's acorn and map progress (read only) in the check's own save, so the map looks as in his game
  var dan=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),"AppData/LocalLow/DefaultCompany/Racer/Phase7/street-loop-gates-v1-laps3");
  foreach(var f in new[]{"woodland-acorns-v1.json","exploration-map-woodstock-world-v2.json"})if(File.Exists(Path.Combine(dan,f)))File.Copy(Path.Combine(dan,f),Path.Combine(saveDir,f),true);
  yield return EnterScene(RaceFlow.RoamScene);flow.Save.Settings.roamMinimapHidden=false;Pads090();yield return new WaitForSeconds(1);
  var collection=race.GetComponent<ExplorationCollection>();var areas=AcornAreas.Of(collection);var rows=new List<string>();
  foreach(var area in areas){
   bool all=area.sites.All(s=>area.Contains(s.position));Check(all,$"{area.name}: {area.Count(collection)}, {area.regions.Length} region(s), every acorn inside: {all}");
   rows.Add($"{area.name} ({area.Count(collection)}): {area.direction}");
   foreach(var r in area.regions)rows.Add($"  region {r.bounds.min.x:F0}..{r.bounds.max.x:F0} x {r.bounds.min.z:F0}..{r.bounds.max.z:F0} (label at {r.label.x:F0},{r.label.z:F0}): {string.Join(", ",r.sites.Select(s=>$"{s.title} ({s.position.x:F0},{s.position.z:F0}){(collection.Discovered(s.id)?" found":"")}"))}");
   var others=areas.Where(o=>o!=area).SelectMany(o=>o.sites.Where(s=>area.Contains(s.position)).Select(s=>o.name+" / "+s.title)).ToList();if(others.Count>0)rows.Add("  also inside it: "+string.Join(", ",others));}
  File.WriteAllLines($"{output}/C-areas.txt",rows);
  // the map, opened with the controller (Back / Select), as the player sees it first (default zoom, centred on the player)
  yield return Press090(pad1,GamepadButton.Select);yield return new WaitForSecondsRealtime(.6f);var map=flow.GetComponent<ExplorationMap>();
  Check(map.Opened,"the map opens with the controller's Back / Select");yield return Late(()=>Shot4k("C-map-default-zoom"));
  for(int i=0;i<areas.Length;i++){yield return Press090(pad1,GamepadButton.DpadDown);yield return new WaitForSecondsRealtime(.4f);
   var list=Text093("Acorn areas list")??"";bool on=list.Contains("▶ "+areas[i].name);Check(on,$"D-pad down {i+1}: {areas[i].name} chosen in the list (highlighted and centred)");
   string tag=areas[i].name.Replace(' ','-');yield return Late(()=>Shot4k($"C-map-area-{i+1}-{tag}"));}
  yield return Press090(pad1,GamepadButton.East);yield return new WaitForSecondsRealtime(.5f);Check(!map.Opened,"B closes the map");
  // the Exploration page: the area rows, with the controller; A on one opens the map on that area
  flow.Pause();yield return new WaitForSecondsRealtime(.4f);flow.OpenExploration();yield return new WaitForSecondsRealtime(.5f);
  string want="area-1";for(int i=0;i<12&&Focus093?.name!=want;i++){yield return Press090(pad1,GamepadButton.DpadDown);yield return new WaitForSecondsRealtime(.25f);}
  Check(Focus093?.name==want,$"Exploration page: D-pad down reaches the {areas[1].name} row (focus {Focus093?.name})");yield return Late(()=>Shot4k("C-exploration-page"));
  yield return Press090(pad1,GamepadButton.South);yield return new WaitForSecondsRealtime(.7f);
  Check(map.Opened&&(Text093("Acorn areas list")??"").Contains("▶ "+areas[1].name),$"A on it opens the map on {areas[1].name}");yield return Late(()=>Shot4k("C-map-from-exploration-page"));
  yield return Press090(pad1,GamepadButton.East);yield return new WaitForSecondsRealtime(.5f);
  for(int i=0;i<4&&flow.State!=RaceFlow.Stage.Racing;i++){yield return Press090(pad1,GamepadButton.East);yield return new WaitForSecondsRealtime(.4f);}
  if(flow.State!=RaceFlow.Stage.Racing)flow.Resume();yield return new WaitForSecondsRealtime(.5f);
  // the minimap line: inside an area that still has acorns (at its label point, away from every acorn), then outside every area
  var open=areas.FirstOrDefault(x=>!x.Done(collection));
  if(open!=null){var spot=open.regions.Select(r=>r.label).First(p=>open.Contains(p)&&collection.sites.All(s=>Vector2.Distance(new(s.position.x,s.position.z),new(p.x,p.z))>15));
   yield return Put093(spot,0);var line=Text093("Acorn area");Check(line==open.name+"  "+open.Count(collection),$"minimap inside {open.name}: \"{line}\"");yield return Late(()=>Shot4k("C-minimap-inside-area"));}
  var outside=new Vector3(150,0,-560);Check(AcornAreas.At(collection,outside)==null,"the outside point is in no area");
  yield return Put093(outside,0);Check(Text093("Acorn area")==null,$"minimap outside every area: no line (\"{Text093("Acorn area")}\")");yield return Late(()=>Shot4k("C-minimap-outside-area"));}
 // the player's vehicle stood on the ground at a point, facing a heading, stopped; the minimap given time to update
 IEnumerator Put093(Vector3 at,float heading){
  var car=race.vehicle;float y=at.y;foreach(var h in Physics.RaycastAll(new Vector3(at.x,800,at.z),Vector3.down,1600,~0,QueryTriggerInteraction.Ignore).OrderBy(h=>h.distance)){var n=h.collider.name.ToLowerInvariant();if(n.Contains("trunk")||n.Contains("tree")||h.collider.attachedRigidbody)continue;y=h.point.y;break;}
  var p=new Vector3(at.x,y+1,at.z);var rot=Quaternion.Euler(0,heading,0);car.Body.position=p;car.Body.rotation=rot;car.transform.SetPositionAndRotation(p,rot);car.Body.linearVelocity=car.Body.angularVelocity=Vector3.zero;
  car.GetComponent<VehicleRespawn>()?.CancelRecovery();for(int i=0;i<30;i++)yield return new WaitForFixedUpdate();yield return new WaitForSecondsRealtime(.5f);}
 // the vehicle turned to a heading where it stands
 IEnumerator Face093(float heading){var car=race.vehicle;var rot=Quaternion.Euler(0,heading,0);car.Body.rotation=rot;car.transform.rotation=rot;car.Body.angularVelocity=car.Body.linearVelocity=Vector3.zero;for(int i=0;i<10;i++)yield return new WaitForFixedUpdate();yield return new WaitForSecondsRealtime(.4f);}
 IEnumerator Compass093(){
  foreach(var roam in new[]{false,true}){
   if(roam){yield return EnterScene(RaceFlow.RoamScene);flow.Save.Settings.roamMinimapHidden=false;}else yield return StartOn093("StreetLoopGreybox","original");
   var n=GameObject.Find("Compass N").GetComponent<UnityEngine.UI.Text>();
   foreach(var (heading,name) in new[]{(0f,"north"),(90f,"east")}){
    yield return Face093(heading);var at=n.rectTransform.anchoredPosition;
    bool ok=n.enabled&&(heading==0?at.y>60&&Mathf.Abs(at.x)<5:at.x<-80&&Mathf.Abs(at.y)<5);
    Check(ok,$"{(roam?"Free Roam":"race")} heading {name}: the N at {at} on the rim ({(heading==0?"top":"left")})");
    yield return Late(()=>Shot4k($"D-{(roam?"roam":"race")}-heading-{name}"));}
   if(!roam)yield return Menu();}
  // split-screen: both minimaps have it (player 2 the AI driver)
  yield return Load("StreetLoopGreybox");yield return Menu();Pads090();
  SplitScreen.P1Device=pad1;SplitScreen.P2Ai=true;SplitScreen.P1Vehicle="original";SplitScreen.P2Vehicle="atv";SplitScreen.Course=0;SplitScreen.Laps=1;SplitScreen.Rivals=0;
  flow.StartSplit();yield return SplitRunning090();yield return new WaitForSeconds(4);
  var ns=FindObjectsByType<UnityEngine.UI.Text>(FindObjectsSortMode.None).Where(t=>t.name=="Compass N"&&t.enabled&&t.canvasRenderer.GetAlpha()>0).ToList();
  Check(ns.Count>=2,$"split-screen: {ns.Count} minimaps show the N");yield return Late(()=>Shot4k("D-split-screen"));
  flow.QuitSplit(false);yield return new WaitForSecondsRealtime(1);}
}
}
#endif
