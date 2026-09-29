using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace Racer {
// Explicit isolated UI fixture. Never started by normal gameplay.
public sealed class TrackUiChecks:MonoBehaviour {
 const string Output="Docs/TrackUi";
 public static bool MenuFollowUp;
 readonly List<string> checks=new();
 void Check(bool ok,string text){checks.Add((ok?"PASS ":"FAIL ")+text);File.WriteAllLines(Output+(MenuFollowUp?"/menu-followup.txt":"/checks.txt"),checks);}
 public static void Launch(){var go=new GameObject("Focused track UI checks");DontDestroyOnLoad(go);go.AddComponent<TrackUiChecks>();}
 void Update(){AudioListener.volume=0;}
 static void Complete(RaceProgress p,double start,double finish){p.BeginTiming(start);p.Cross(0,true,start);for(int lap=0;lap<p.TargetLaps;lap++){for(int g=1;g<=p.CheckpointCount;g++)p.Cross(g,true,start+1);p.Cross(0,true,start+(finish-start)*(lap+1)/p.TargetLaps);}}
 IEnumerator Start(){
 Directory.CreateDirectory(Output);Application.runInBackground=true;AudioListener.volume=0;yield return null;yield return null;
 var order=RacePlaylists.DisplayOrder.ToArray();Check(order.SequenceEqual(new[]{6,2,3,4,5,0,1}),"Seven alphabetical base names; Forward before Reverse; stable saved indices");
 Check(RacePlaylists.Difficulties.All(d=>d==RacePlaylists.TrackDifficulty.Unassigned),"All ratings unassigned / TBD; Easy Medium Hard metadata available");
 var ids=new[]{"street-v14-discovery","street-reverse-local-laurel-v6","lake-v7-discovery","forest-reverse-v7-water-detour","mountain-forward-v5-supported","mountain-reverse-v5-supported","backyard-forward-v2-terrain"};
 foreach(int course in (MenuFollowUp?new[]{4,5}:order)){
  var current=FindAnyObjectByType<RaceDirector>();
#if UNITY_EDITOR
  current.Flow.UseValidationSave(Path.GetFullPath("Temp/TrackUiSave"));
#endif
  current.Flow.OpenCourses();yield return null;
  var buttons=FindObjectsByType<UnityEngine.UI.Button>(FindObjectsSortMode.None).Where(b=>b.isActiveAndEnabled&&b.GetComponentInChildren<UnityEngine.UI.Text>().text.Contains("Difficulty:")).OrderBy(b=>b.transform.GetSiblingIndex()).ToArray();
  Check(buttons.Length==7&&buttons.Select(b=>b.GetComponentInChildren<UnityEngine.UI.Text>().text.Split('\n')[0]).SequenceEqual(order.Select(i=>RacePlaylists.Titles[i])),"Selector names/order in "+SceneManager.GetActiveScene().name);
  if(course==6||MenuFollowUp)ThreeFeatureValidation.CaptureUi(Output+"/track-selector.png");
  buttons[Array.IndexOf(order,course)].onClick.Invoke();yield return null;yield return null;
  var loaded=FindAnyObjectByType<RaceDirector>();
  Check(SceneManager.GetActiveScene().name==RacePlaylists.Scenes[course],"Button launches "+RacePlaylists.Scenes[course]);
  Check(loaded.courseId==ids[course]&&loaded.Category.StartsWith(ids[course]+"-"),"Existing record category retained: "+ids[course]);
  File.AppendAllText(Output+"/course-identities.txt",course+" "+SceneManager.GetActiveScene().name+" "+loaded.courseId+" "+loaded.Category+"\n");
 }
 if(MenuFollowUp){File.WriteAllText(Output+"/menu-followup-done.txt",$"{checks.Count(x=>x.StartsWith("PASS"))}/{checks.Count} pass");
#if UNITY_EDITOR
 UnityEditor.EditorApplication.isPlaying=false;
#endif
 yield break;}
 var race=FindAnyObjectByType<RaceDirector>();var flow=race.Flow;
#if UNITY_EDITOR
 flow.UseValidationSave(Path.GetFullPath("Temp/TrackUiSave"));
#endif
 flow.OpenGarage();flow.SelectVehicle("moto");flow.CloseGarage();race.laps=1;race.opponents=true;race.traffic=false;flow.Save.Settings.estimateAiFinishes=false;
 flow.StartRace();while(flow.State!=RaceFlow.Stage.Racing)yield return null;
 race.enabled=false;foreach(var driver in race.Drivers)driver.enabled=false;
 var menus=flow.GetComponent<RaceMenus>();yield return null;
 var button=Resources.FindObjectsOfTypeAll<UnityEngine.UI.Button>().Single(b=>b.name=="Simulate remaining racers");
 Check(!menus.CanSimulateRemaining&&!button.gameObject.activeSelf,"Button absent before human finishes");button.onClick.Invoke();Check(!race.Racers.Any(r=>r.Estimated),"Early invocation cannot estimate");
 Complete(race.Racers[1].Progress,race.Clock-110,race.Clock-2);double measured=race.Racers[1].ClassifiedTime(race.Clock);
 Complete(race.Progress,race.Clock-100,race.Clock);flow.LapCompleted();yield return null;yield return null;
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
 File.WriteAllText(Output+"/done.txt",$"{checks.Count(x=>x.StartsWith("PASS"))}/{checks.Count} pass");
#if UNITY_EDITOR
 UnityEditor.EditorApplication.isPlaying=false;
#endif
 }
}}
