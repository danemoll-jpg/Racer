using System;using System.Collections;using System.Collections.Generic;using System.IO;using System.Linq;using UnityEngine;using UnityEngine.SceneManagement;
namespace Racer {
 // Explicit delivery checks only; no behavior during normal play.
 public sealed class BackyardValidation:MonoBehaviour {
  string output;bool drive,reverseOnly;readonly List<string> rows=new();RaceDirector race;ArcadeVehicle car;
  public static void BeginEditor(string folder,bool physical=false,bool onlyReverse=false){var v=new GameObject("Backyard targeted validation").AddComponent<BackyardValidation>();v.output=Path.GetFullPath(folder);v.drive=physical;v.reverseOnly=onlyReverse;DontDestroyOnLoad(v.gameObject);}
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Boot(){var a=Environment.GetCommandLineArgs();int i=Array.IndexOf(a,"-backyardValidation");if(i>=0&&i+1<a.Length&&!FindAnyObjectByType<BackyardValidation>())BeginEditor(a[i+1],Array.IndexOf(a,"-backyardDrive")>=0);}
  void Update(){AudioListener.volume=0;}
  void Check(bool pass,string label){rows.Add((pass?"PASS ":"FAIL ")+label);File.WriteAllLines(Path.Combine(output,"checks.txt"),rows);}
  IEnumerator Start(){
   Directory.CreateDirectory(output);
   foreach(bool reverse in reverseOnly?new[]{true}:new[]{false,true}){
    SceneManager.LoadScene(reverse?"DansBackyardReverse":"DansBackyard");yield return null;yield return new WaitForSecondsRealtime(1);
    race=FindAnyObjectByType<RaceDirector>();car=race.vehicle;var title=FindAnyObjectByType<StartupTitle>();if(title)Destroy(title.gameObject);race.Flow.EnterMenuAfterTitle();
    if(Application.isEditor){
     string isolated=Path.Combine(output,"isolated-saves");typeof(RaceFlow).GetProperty("Save").SetValue(race.Flow,new RacerSave(isolated,"backyard-check"));typeof(RaceFlow).GetProperty("Boards").SetValue(race.Flow,new RecordBoards(isolated));
    }
    if(race.Flow.Ghost){race.Flow.Ghost.enabled=false;typeof(RaceFlow).GetProperty("Ghost").SetValue(race.Flow,null);}
    if(race.Flow.Activities)race.Flow.Activities.enabled=false;
    foreach(var item in FindObjectsByType<ContinuationTraffic>())item.enabled=false;
    foreach(var item in FindObjectsByType<ExplorationCollection>())item.enabled=false;
    foreach(var item in FindObjectsByType<ExplorationMap>())item.enabled=false;
    race.Flow.Save.Settings.master=0;race.Flow.Save.Settings.vehicleId="moto";race.opponents=false;race.traffic=false;race.laps=1;race.Flow.StartRace();yield return new WaitForSecondsRealtime(4);
    string name=SceneManager.GetActiveScene().name;Check(race.courseName==(reverse?"Dan's Backyard - Reverse":"Dan's Backyard - Forward"),name+" distinct course identity");Check(race.requireOrderedGates,name+" ordered gates required");Check(FindAnyObjectByType<RacingMiniMap>()!=null,name+" existing minimap available");Check(race.Branches.Length==1,name+" only direction-appropriate new shortcut registered");
    car.enabled=false;car.GetComponent<VehicleInput>().enabled=false;car.GetComponent<VehicleRespawn>().enabled=false;car.Body.isKinematic=true;
    void Reset(){race.Progress.Restart();race.Racers[0].Travel=0;race.Racers[0].VerifiedRoad=0;race.Racers[0].FinishArmed=false;race.Racers[0].Branch.Clear();var p=race.road.At(-8,out _)+Vector3.up*.6f;car.Body.position=p;car.transform.position=p;race.ResetSampling(p,0);}
    double now=0;void Sample(float s){var p=race.road.At(s,out var f)+Vector3.up*.6f;car.Body.position=p;car.Body.rotation=Quaternion.LookRotation(Vector3.ProjectOnPlane(f,Vector3.up));car.transform.SetPositionAndRotation(p,car.Body.rotation);race.Sample(p,f,now+=.03);}
    Reset();for(float s=-7;s<=race.road.Length+3;s+=1)Sample(s);Check(race.Progress.CompletedLaps==1&&race.Progress.MissedGates==0,name+" full main-route production progression");
    Reset();now=0;for(float s=-7;s<=5;s+=1)Sample(s);for(float s=-4;s<=5;s+=1)Sample(s);Check(race.Progress.CompletedLaps==0,name+" immediate finish retrigger rejected");
    for(float s=6;s<=race.road.Length+3;s+=1){float gate=race.road.Project(race.gates[2].transform.position,out _);if(Math.Abs(s-gate)<10){var p=race.road.At(s,out var f)+Vector3.Cross(Vector3.up,f).normalized*25+Vector3.up*.6f;car.Body.position=p;car.transform.position=p;race.Sample(p,f,now+=.03);}else Sample(s);}Check(race.Progress.CompletedLaps==0,name+" skipped checkpoint cannot count a lap");
    Reset();now=0;var branch=race.Branches[0];for(float s=-7;s<branch.entryRoad;s+=1)Sample(s);for(float s=0;s<branch.Length;s+=1){var p=branch.At(s,out var f)+Vector3.up*.6f;car.Body.position=p;car.Body.rotation=Quaternion.LookRotation(Vector3.ProjectOnPlane(f,Vector3.up));car.transform.SetPositionAndRotation(p,car.Body.rotation);race.Sample(p,f,now+=.03);}for(float s=branch.exitRoad;s<=race.road.Length+3;s+=1)Sample(s);Check(race.Progress.CompletedLaps==1&&race.Progress.MissedGates==0,name+" shortcut entitlement and rejoin complete legal lap");
    if(drive){
     race.Flow.Pause();race.Flow.QuitRace();foreach(var b in race.Branches)b.aiValidated=false;race.laps=1;race.Flow.StartRace();yield return new WaitForSecondsRealtime(4);
     car.GetComponent<VehicleRespawn>().enabled=true;var driver=car.GetComponent<RoadDriver>()??car.gameObject.AddComponent<RoadDriver>();driver.Initialize(race,car,true,1,1);driver.Racer=race.Racers[0];float start=Time.realtimeSinceStartup;var samples=new List<string>();
     while(!race.Progress.Finished&&Time.realtimeSinceStartup-start<150){samples.Add($"{Time.realtimeSinceStartup-start:F1} pos={car.transform.position:F2} speed={car.ForwardSpeed:F2} gate={race.Progress.NextGate} recoveries={driver.RecoveryCount}");File.WriteAllLines(Path.Combine(output,name+"-drive.txt"),samples);yield return new WaitForSecondsRealtime(1);}
     Check(race.Progress.CompletedLaps==1&&race.Progress.MissedGates==0,name+" existing AI driver completes physical main-route lap; recoveries="+driver.RecoveryCount+" misses="+race.Progress.MissedGates);Destroy(driver);yield return null;
     if(!reverse){
      race.Flow.Pause();race.Flow.QuitRace();race.Flow.StartRace();yield return new WaitForSecondsRealtime(4);car.enabled=false;car.GetComponent<VehicleInput>().enabled=false;car.GetComponent<VehicleRespawn>().enabled=false;
      var at=new Vector3(478.3f,200,10);var support=Physics.RaycastAll(at,Vector3.down,300,1,QueryTriggerInteraction.Ignore).Where(h=>h.collider.name.StartsWith("Ground_")).OrderBy(h=>h.distance).First();at.y=support.point.y+.55f;car.Body.isKinematic=false;car.Body.position=at;car.Body.rotation=Quaternion.LookRotation(Vector3.back);car.transform.SetPositionAndRotation(at,car.Body.rotation);car.Body.linearVelocity=Vector3.back*16;car.Body.angularVelocity=Vector3.zero;Physics.SyncTransforms();float air=0,maxAir=0,minSpeed=100;bool crossed=false;
      for(int frame=0;frame<220;frame++){if(car.GroundedWheels==0)air+=Time.fixedDeltaTime;else air=0;maxAir=Math.Max(maxAir,air);if(frame>15)minSpeed=Math.Min(minSpeed,car.ForwardSpeed);if(car.Body.position.z< -12){crossed=true;break;}car.Simulate(car.ForwardSpeed<16?.4f:0,car.ForwardSpeed>18?.2f:0,0,Time.fixedDeltaTime);yield return new WaitForFixedUpdate();}
      Check(crossed&&maxAir<.4f&&minSpeed>5,"Kyle driveway live motorcycle crossing at 16m/s; min="+minSpeed.ToString("F2")+" maxAir="+maxAir.ToString("F2"));car.Body.isKinematic=true;
     }
    }
    race.Flow.Pause();race.Flow.QuitRace();race.Flow.OpenCourses();yield return null;yield return null;ScreenCapture.CaptureScreenshot(Path.Combine(output,name+"-menu.png"));yield return null;
    var texts=FindObjectsByType<UnityEngine.UI.Text>().Select(t=>t.text).ToArray();Check(texts.Contains("Dan's Backyard - Forward")&&texts.Contains("Dan's Backyard - Reverse")&&texts.Contains("Mountain Loop Reverse"),name+" both new and existing menu entries visible");
    race.Flow.SelectBackyard(!reverse);yield return null;Check(SceneManager.GetActiveScene().name==(reverse?"DansBackyard":"DansBackyardReverse"),name+" menu action launches correct direction");
   }
   File.WriteAllText(Path.Combine(output,"done.txt"),string.Join("\n",rows));
   #if UNITY_EDITOR
   if(Application.isEditor){UnityEditor.EditorApplication.isPlaying=false;yield break;}
   #endif
   Application.Quit(rows.Any(r=>r.StartsWith("FAIL"))?1:0);
  }
 }
}
