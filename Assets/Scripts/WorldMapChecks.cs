using System;using System.IO;using System.Linq;using System.Collections;using System.Collections.Generic;using UnityEngine;using UnityEngine.SceneManagement;using UnityEngine.EventSystems;
namespace Racer {
public sealed class WorldMapChecks:MonoBehaviour {
 const string Output="Docs/WorldMap";
 readonly List<string> checks=new();
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Boot(){var args=Environment.GetCommandLineArgs();if(args.Contains("-worldMapCheck")&&args.Contains("-racerTestSave")&&!FindAnyObjectByType<WorldMapChecks>()){var go=new GameObject("Isolated world map checks");DontDestroyOnLoad(go);go.AddComponent<WorldMapChecks>();}}
 void Check(bool ok,string text){checks.Add((ok?"PASS ":"FAIL ")+text);File.WriteAllLines(Output+"/ui-checks.txt",checks);}
 void Update(){AudioListener.volume=0;}
 static void Complete(RaceProgress p,double start,double finish){p.BeginTiming(start);p.Cross(0,true,start);for(int lap=0;lap<p.TargetLaps;lap++){for(int g=1;g<=p.CheckpointCount;g++)p.Cross(g,true,start+1);p.Cross(0,true,start+(finish-start)*(lap+1)/p.TargetLaps);}}
 IEnumerator Start(){Directory.CreateDirectory(Output);Application.runInBackground=true;AudioListener.volume=0;if(!Application.isEditor&&SceneManager.GetActiveScene().name!="DansBackyardForward")SceneManager.LoadScene("DansBackyardForward");yield return null;yield return null;
 var race=FindAnyObjectByType<RaceDirector>();var flow=race.Flow;
#if UNITY_EDITOR
 flow.UseValidationSave(Path.GetFullPath("Temp/WorldMapSave"));
#endif
 // Isolated UI fixture: skip startup art/audio, just as standalone -racerSkipTitle does.
 var title=FindAnyObjectByType<StartupTitle>();if(title)DestroyImmediate(title.gameObject);if(EventSystem.current){var input=EventSystem.current.GetComponent<BaseInputModule>();if(input)input.enabled=true;}flow.EnterMenuAfterTitle();yield return null;
 flow.OpenGarage();flow.SelectVehicle("moto");flow.CloseGarage();race.laps=1;race.opponents=true;race.traffic=false;flow.Save.Settings.estimateAiFinishes=false;
 flow.StartRace();while(flow.State!=RaceFlow.Stage.Racing)yield return null;

 var map=race.GetComponent<ExplorationMap>();map.Open();yield return null;
 var visual=Resources.Load<WorldMapVisual>("WorldMaps/"+race.gameObject.scene.name);
 Check(visual&&visual.image.width>=5000,"Full menu uses high-resolution saved-scene render");
 Check(ExplorationMap.Compatibility=="woodstock-world-v2"&&ExplorationMap.Columns==110&&ExplorationMap.Rows==80,"Existing discovery save grid preserved");
 foreach(var d in map.destinations)Check(Vector3.Distance(Vector3.Scale(d.position,new Vector3(1,0,1)),visual.WorldPoint(visual.Normalized(d.position)))<.001f,"Map coordinate round trip: "+d.title);
 var picture=Resources.FindObjectsOfTypeAll<UnityEngine.UI.RawImage>().Single(p=>p.name=="Terrain"&&p.gameObject.activeInHierarchy);
 var heading=picture.GetComponentsInChildren<UnityEngine.UI.Text>().Single(t=>t.name=="Player heading");
 var uv=visual.Normalized(race.vehicle.Body.position);var shown=(uv-picture.uvRect.center)/picture.uvRect.size.x;
 Check(Vector2.Distance(heading.rectTransform.anchoredPosition,Vector2.Scale(shown,picture.rectTransform.rect.size))<.1f,"Player marker aligned to actual texture bounds");
 ThreeFeatureValidation.CaptureUi(Output+"/menu-map.png");
 var wheel=new PointerEventData(EventSystem.current){scrollDelta=new Vector2(0,8)};map.OnScroll(wheel);yield return null;var beforePan=picture.uvRect;
 map.OnDrag(new PointerEventData(EventSystem.current){delta=new Vector2(65,30)});yield return null;
 Check(picture.uvRect.width<1&&picture.uvRect.center!=beforePan.center,"Wheel zoom and drag pan remain operational");
 map.SetWaypoint(race.vehicle.Body.position);yield return null;Check(map.Waypoint.HasValue,"Waypoint remains available");
 var toggle=Resources.FindObjectsOfTypeAll<UnityEngine.UI.Button>().Single(b=>b.name=="Show / hide course");toggle.onClick.Invoke();yield return null;
 Check(picture.GetComponentInChildren<WorldMapCourseOverlay>().gameObject.activeInHierarchy,"Optional active-course overlay uses actual route/branches");
 ThreeFeatureValidation.CaptureUi(Output+"/menu-map-overlay.png");toggle.onClick.Invoke();yield return null;
 Check(!picture.GetComponentInChildren<WorldMapCourseOverlay>(true).gameObject.activeSelf,"Course overlay removable; clean map is default");
 map.Close();yield return null;Check(!map.Opened&&flow.State==RaceFlow.Stage.Racing,"Close restores racing flow");
 race.enabled=false;foreach(var driver in race.Drivers)driver.enabled=false;
 var menus=flow.GetComponent<RaceMenus>();yield return null;
 var button=Resources.FindObjectsOfTypeAll<UnityEngine.UI.Button>().Single(b=>b.name=="Simulate remaining racers");
 Check(!menus.CanSimulateRemaining&&!button.gameObject.activeSelf,"Button absent before human finishes");button.onClick.Invoke();Check(!race.Racers.Any(r=>r.Estimated),"Early invocation cannot estimate");
 Complete(race.Racers[1].Progress,race.Clock-110,race.Clock-2);double measured=race.Racers[1].ClassifiedTime(race.Clock);
 Complete(race.Progress,race.Clock-100,race.Clock);flow.LapCompleted();yield return null;yield return null;
 Check(button.GetComponentInChildren<UnityEngine.UI.Text>().text=="COMPLETE RACE","Exact requested button label");
 Check(menus.CanSimulateRemaining&&button.gameObject.activeInHierarchy&&Cursor.visible,"Finished human with unfinished AI gets visible clickable waiting button");
 ThreeFeatureValidation.CaptureUi(Output+"/waiting.png");var human=race.Progress.AdjustedTime(race.Clock);int records=flow.Boards.Board(race.Category,true).Count;
 button.onClick.Invoke();yield return null;
 Check(race.ClassificationFinal&&flow.State==RaceFlow.Stage.Results,"Waiting button uses CR-064 and opens normal final results");
 Check(!race.Racers[0].Estimated&&!race.Racers[1].Estimated&&race.Racers[1].ClassifiedTime(race.Clock)==measured&&race.Progress.AdjustedTime(race.Clock)==human,"Human and already-finished AI measured results unchanged");
 Check(race.Racers.Skip(2).All(r=>r.Estimated)&&race.Standings().Contains("Estimated"),"Only unfinished AI estimated and labeled");
 var times=race.Racers.Select(r=>r.ClassifiedTime(race.Clock)).ToArray();button.onClick.Invoke();button.onClick.Invoke();
 Check(times.SequenceEqual(race.Racers.Select(r=>r.ClassifiedTime(race.Clock))),"Repeated activation is harmless");
 Check(records==flow.Boards.Board(race.Category,true).Count,"Estimated AI do not enter measured record board");
 Check(!menus.CanSimulateRemaining&&!button.gameObject.activeInHierarchy,"Button absent after final classification");ThreeFeatureValidation.CaptureUi(Output+"/results.png");
 flow.StartRace();while(flow.State!=RaceFlow.Stage.Racing)yield return null;race.enabled=false;foreach(var driver in race.Drivers)driver.enabled=false;
 foreach(var r in race.Racers)Complete(r.Progress,race.Clock-100,race.Clock);yield return null;
 Check(!menus.CanSimulateRemaining&&!button.gameObject.activeInHierarchy,"No button when all racers have measured finishes");race.enabled=true;yield return new WaitForFixedUpdate();yield return null;
 Check(race.ClassificationFinal&&flow.State==RaceFlow.Stage.Results&&race.Racers.All(r=>!r.Estimated),"Normal all-measured completion unaffected");
 File.WriteAllText(Output+"/ui-done.txt",$"{checks.Count(x=>x.StartsWith("PASS"))}/{checks.Count} pass");
#if UNITY_EDITOR
 UnityEditor.EditorApplication.isPlaying=false;
 #else
 Application.Quit(checks.Any(s=>s.StartsWith("FAIL"))?1:0);
#endif
 }
}}
