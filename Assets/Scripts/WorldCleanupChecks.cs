using System;using System.IO;using System.Linq;using System.Collections;using System.Collections.Generic;using UnityEngine;using UnityEngine.SceneManagement;using UnityEngine.EventSystems;
namespace Racer {
public sealed class WorldCleanupChecks:MonoBehaviour {
 const string Output="Docs/WorldCleanup";
 readonly List<string> checks=new();
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Boot(){var args=Environment.GetCommandLineArgs();if(args.Contains("-worldCleanupCheck")&&args.Contains("-racerTestSave")&&!FindAnyObjectByType<WorldCleanupChecks>()){var go=new GameObject("Isolated world map checks");DontDestroyOnLoad(go);go.AddComponent<WorldCleanupChecks>();}}
 void Check(bool ok,string text){checks.Add((ok?"PASS ":"FAIL ")+text);File.WriteAllLines(Output+"/ui-checks.txt",checks);}
 void Update(){AudioListener.volume=0;}
 static void Complete(RaceProgress p,double start,double finish){p.BeginTiming(start);p.Cross(0,true,start);for(int lap=0;lap<p.TargetLaps;lap++){for(int g=1;g<=p.CheckpointCount;g++)p.Cross(g,true,start+1);p.Cross(0,true,start+(finish-start)*(lap+1)/p.TargetLaps);}}
 IEnumerator Start(){Directory.CreateDirectory(Output);Application.runInBackground=true;AudioListener.volume=0;if(!Application.isEditor&&SceneManager.GetActiveScene().name!="DansBackyardForward")SceneManager.LoadScene("DansBackyardForward");yield return null;yield return null;
 var race=FindAnyObjectByType<RaceDirector>();var flow=race.Flow;
#if UNITY_EDITOR
 flow.UseValidationSave(Path.GetFullPath("Temp/WorldCleanupSave"));
#endif
 // Isolated UI fixture: skip startup art/audio, just as standalone -racerSkipTitle does.
 var title=FindAnyObjectByType<StartupTitle>();if(title)DestroyImmediate(title.gameObject);if(EventSystem.current){var input=EventSystem.current.GetComponent<BaseInputModule>();if(input)input.enabled=true;}flow.EnterMenuAfterTitle();yield return null;
 flow.OpenGarage();flow.SelectVehicle("moto");flow.CloseGarage();race.laps=1;race.opponents=true;race.traffic=false;flow.Save.Settings.estimateAiFinishes=false;
 flow.StartRace();while(flow.State!=RaceFlow.Stage.Racing)yield return null;

 var map=race.GetComponent<ExplorationMap>();map.Open();yield return null;
 var visual=Resources.Load<WorldMapVisual>("WorldMaps/PermanentWorld");
 Check(visual&&visual.image.width>=5000,"Full menu uses shared permanent world render");
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
 Check(button.GetComponentInChildren<UnityEngine.UI.Text>().text==menus.CompleteRacePrompt&&menus.CompleteRacePrompt.Contains("Space"),"Explicit Space Complete Race prompt from submit binding");
 Check(menus.CanSimulateRemaining&&button.gameObject.activeInHierarchy&&Cursor.visible,"Finished human with unfinished AI gets visible clickable waiting button");
 ThreeFeatureValidation.CaptureUi(Output+"/waiting.png");var human=race.Progress.AdjustedTime(race.Clock);int records=flow.Boards.Board(race.Category,true).Count;
 var testKeyboard=UnityEngine.InputSystem.InputSystem.AddDevice<UnityEngine.InputSystem.Keyboard>();
 UnityEngine.InputSystem.InputSystem.QueueStateEvent(testKeyboard,new UnityEngine.InputSystem.LowLevel.KeyboardState(UnityEngine.InputSystem.Key.Space));
 yield return null;yield return null;
 UnityEngine.InputSystem.InputSystem.QueueStateEvent(testKeyboard,new UnityEngine.InputSystem.LowLevel.KeyboardState());
 yield return null;UnityEngine.InputSystem.InputSystem.RemoveDevice(testKeyboard);
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

 // Four existing Western gullies collection triggers, with short continuous grounded samples.
 flow.StartFreeRoam();yield return null;race.traffic=false;
 var vehicle=race.vehicle;vehicle.enabled=false;vehicle.GetComponent<VehicleInput>().enabled=false;
 vehicle.Body.isKinematic=true;var collection=race.GetComponent<ExplorationCollection>();
 foreach(var site in collection.sites.Where(s=>s.approach=="Western gullies")){ var blockers=new HashSet<string>();
     for(int step=0;step<=65;step++){
         var p=Vector3.Lerp(site.access,site.position,Mathf.Clamp01((step-2)/60f));
         var ground=Physics.RaycastAll(new Vector3(p.x,300,p.z),Vector3.down,600).Where(h=>h.collider.name.StartsWith("Ground_")).OrderBy(h=>h.distance).First();
                  foreach(var c in Physics.OverlapCapsule(ground.point+Vector3.up*.8f,ground.point+Vector3.up*1.4f,.65f,~0,QueryTriggerInteraction.Ignore))
             if(!c.name.StartsWith("Ground_")&&!c.GetComponentInParent<ArcadeVehicle>())blockers.Add(c.name);
         vehicle.Body.position=ground.point+Vector3.up;yield return new WaitForFixedUpdate();
     }
     Check(collection.Discovered(site.id),"Existing Western gullies swept trigger fires: "+site.id); File.AppendAllText(Output+"/western-access.txt",site.id+" solid capsule overlaps: "+string.Join(", ",blockers)+"\n");
 }
 Check(map.destinations.Count(d=>new[]{"Moll's","Roger's","McFadden's","Anderson's"}.Contains(d.title))==4,"Four unique corrected property destinations");
 foreach(var d in map.destinations.Where(d=>new[]{"Moll's","Roger's","McFadden's","Anderson's"}.Contains(d.title))){map.Reveal(d.position);Check(map.Travel(Array.IndexOf(map.destinations,d)),"Supported free-roam arrival: "+d.title);}
 var fixtureRoot=Path.GetFullPath("Temp/WorldCleanupMigration");Directory.CreateDirectory(fixtureRoot);
 var data=new ExplorationMap.Data();data.landmarks.Add("home");var visitedProperty=map.destinations.Single(d=>d.id=="property-anderson");
 var cell=ExplorationMap.Normalized(visitedProperty.position);data.visited.Add(Mathf.FloorToInt(cell.y*ExplorationMap.Rows)*ExplorationMap.Columns+Mathf.FloorToInt(cell.x*ExplorationMap.Columns));
 File.WriteAllText(Path.Combine(fixtureRoot,"exploration-map-"+ExplorationMap.Compatibility+".json"),JsonUtility.ToJson(data));
 var fixture=new GameObject("Isolated discovery migration").AddComponent<ExplorationMap>();fixture.destinations=map.destinations;fixture.Initialize(race,fixtureRoot);
 Check(fixture.Discovered("home")&&fixture.Discovered("property-anderson"),"Existing home ID and previously visited Anderson property retained");
 Check(!fixture.Discovered("property-mcfadden"),"Unvisited property remains undiscovered");Destroy(fixture.gameObject);
 var pad=UnityEngine.InputSystem.InputSystem.AddDevice<UnityEngine.InputSystem.XInput.XInputController>();yield return null;
 Check(menus.CompleteRacePrompt.Contains("A")&&menus.CompleteRacePrompt.Contains("Space"),"Controller A and keyboard Space labels resolve from existing submit action");
 UnityEngine.InputSystem.InputSystem.RemoveDevice(pad);
 File.WriteAllText(Output+"/ui-done.txt",$"{checks.Count(x=>x.StartsWith("PASS"))}/{checks.Count} pass");
#if UNITY_EDITOR
 UnityEditor.EditorApplication.isPlaying=false;
 #else
 Application.Quit(checks.Any(s=>s.StartsWith("FAIL"))?1:0);
#endif
 }
}}
