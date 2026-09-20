using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace Racer
{
    public sealed class ArcadeReviewValidation:MonoBehaviour
    {
        static string Arg(string key,string fallback){var args=Environment.GetCommandLineArgs();int n=Array.IndexOf(args,key);return n>=0&&n+1<args.Length?args[n+1]:fallback;}
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot(){var args=Environment.GetCommandLineArgs();if(!args.Contains("-arcadeTest")||!args.Contains("-racerTestSave")||FindAnyObjectByType<ArcadeReviewValidation>())return;var go=new GameObject("Arcade combined diagnostics");DontDestroyOnLoad(go);go.AddComponent<ArcadeReviewValidation>();}
        readonly List<string> checks=new();RaceDirector race;RaceFlow flow;ArcadeVehicle car;string root;
        void Check(bool pass,string name){checks.Add((pass?"PASS ":"FAIL ")+name);File.WriteAllLines(root+"/checks.txt",checks);}
        IEnumerator Start()
        {
            root=Arg("-evidence","Docs/CR075-080/tests");Directory.CreateDirectory(root);Application.runInBackground=true;QualitySettings.vSyncCount=0;Application.targetFrameRate=120;
            string scene=Arg("-course","StreetLoopGreybox");if(SceneManager.GetActiveScene().name!=scene)SceneManager.LoadScene(scene);yield return null;yield return null;
            race=FindAnyObjectByType<RaceDirector>();flow=race.Flow;car=race.vehicle;
            if(Arg("-arcadeTest","rules")=="persistence")yield return Persistence();else if(Arg("-arcadeTest","rules")=="roam-recovery")yield return RoamRecovery();else if(Arg("-arcadeTest","rules")=="recovery")yield return Recovery();else if(Arg("-arcadeTest","rules")=="speed")yield return Speed();else if(Arg("-arcadeTest","rules")=="smash")yield return SmashDrive();else if(Arg("-arcadeTest","rules")=="ai")yield return AiRecovery();else if(Arg("-arcadeTest","rules")=="forest-jump")yield return ForestJump();else if(Arg("-arcadeTest","rules")=="offroute")yield return OffRoute();else yield return Rules();
            File.WriteAllText(root+"/done.txt",$"{checks.Count(c=>c.StartsWith("PASS"))}/{checks.Count}");Application.Quit(checks.Any(c=>c.StartsWith("FAIL"))?2:0);
        }
        IEnumerator Begin(string id,bool roam)
        {
            if(flow.State!=RaceFlow.Stage.Ready){flow.Pause();flow.QuitRace();}flow.OpenGarage();flow.SelectVehicle(id);flow.CloseGarage();race.traffic=true;race.opponents=true;if(roam)flow.StartFreeRoam();else flow.StartRace();while(flow.State!=RaceFlow.Stage.Racing)yield return null;
        }
        void Place(Vector3 p,Vector3 f,float speed=0)
        {
            car.Body.position=p;car.Body.rotation=Quaternion.LookRotation(Vector3.ProjectOnPlane(f,Vector3.up));car.transform.SetPositionAndRotation(p,car.Body.rotation);car.Body.linearVelocity=f.normalized*speed;car.Body.angularVelocity=Vector3.zero;car.ClearSteering();Physics.SyncTransforms();race.ResetSampling(p,race.Clock);FindAnyObjectByType<ChaseCamera>()?.Snap();
        }
        IEnumerator Rules()
        {
            ThreeFeatureValidation.CaptureUi(root+"/main-menu.png");
            yield return Begin(race.Forest?"moto":"original",true);var activity=flow.Activities;
            Check(race.FreeRoam&&flow.State==RaceFlow.Stage.Racing&&flow.CountdownRemaining==0,"Free roam enters directly without countdown");
            Check(race.Racers.Count==1&&race.Drivers.All(d=>d.Racer==null),"No AI race opponents; ambient traffic retained");
            Check(race.Drivers.Count>0,"Normal ambient traffic active");
            Check(!race.Progress.Started&&!race.Progress.LapActive,"No race/lap clock in roam");
            Check(race.gates.All(g=>g.GetComponentsInChildren<Renderer>().All(r=>!r.enabled)),"Race gate renderers hidden");
            Check(activity.Sites.Count(s=>s.kind==ActivitySite.Kind.Speed)==2&&activity.Sites.Any(s=>s.kind==ActivitySite.Kind.Jump)&&activity.Sites.Any(s=>s.kind==ActivitySite.Kind.Smash),"Authored jump, smash and two speed traps present");
            var before=Directory.GetFiles(flow.Save.DirectoryPath,"*",SearchOption.AllDirectories).Where(p=>!p.Contains("settings")).ToArray();
            foreach(var gate in race.gates){race.Sample(gate.transform.position-gate.transform.forward,gate.transform.forward,race.Clock);race.Sample(gate.transform.position+gate.transform.forward,gate.transform.forward,race.Clock);}
            flow.LapCompleted();Check(race.Progress.CompletedLaps==0&&race.Progress.PenaltySeconds==0&&flow.RaceRank==0,"Roam crossings cannot advance gates/laps/penalties or write race records");
            var guidance=race.GetComponent<WrongWayGuidance>();guidance.Observe(-10,true,true,6);yield return new WaitForFixedUpdate();Check(!guidance.Visible,"Wrong-way guidance suppressed in roam");
            activity.BeginAttempt();Check(activity.AttemptActive,"Deliberate challenge start");flow.Pause();yield return null;float clock=Time.time;yield return new WaitForSecondsRealtime(.3f);Check(Mathf.Abs(Time.time-clock)<.001f&&activity.AttemptActive,"Pause freezes attempt time");ThreeFeatureValidation.CaptureUi(root+"/activities-menu.png");flow.Resume();
            car.GetComponent<VehicleRespawn>().TryRecoverLocal();Check(!activity.AttemptActive,"Recovery cancels challenge without an award");
            var smash=activity.Sites.First(s=>s.kind==ActivitySite.Kind.Smash);while(activity.Selected!=smash)activity.Cycle();activity.BeginAttempt();
            // Explicit event-contract fixture, separate from physical driving evidence.
            typeof(ArcadeActivities).GetField("warm",BindingFlags.NonPublic|BindingFlags.Instance).SetValue(activity,1f);typeof(ArcadeActivities).GetField("blockedUntil",BindingFlags.NonPublic|BindingFlags.Instance).SetValue(activity,0f);
            var method=typeof(ArcadeActivities).GetMethod("Smash",BindingFlags.NonPublic|BindingFlags.Instance);var prop=smash.props[0];method.Invoke(activity,new object[]{prop,car});method.Invoke(activity,new object[]{prop,car});
            Check(activity.SmashCount==1,"RULE: repeated same eligible prop counts once per attempt");
            var unrelated=FindObjectsByType<BreakableProp>().First(p=>!smash.props.Contains(p));method.Invoke(activity,new object[]{unrelated,car});Check(activity.SmashCount==1,"RULE: unrelated props cannot score");
            activity.BeginAttempt();Check(activity.SmashCount==0,"Deliberate retry clears distinct-prop set");
            typeof(ArcadeActivities).GetField("warm",BindingFlags.NonPublic|BindingFlags.Instance).SetValue(activity,1f);
            foreach(var target in smash.props.Take((int)smash.gold))method.Invoke(activity,new object[]{target,car});
            Check(!activity.AttemptActive&&activity.PersonalBest(smash)?.medal==3,"RULE: gold awarded only for distinct eligible prop target");
            string save=Path.Combine(flow.Save.DirectoryPath,"activities-v1.json");Check(File.Exists(save)&&JsonUtility.FromJson<ArcadeActivities.Archive>(File.ReadAllText(save)).results.Any(b=>b.key==activity.Key(smash)&&b.medal==3),"Activity PB/medal persisted under course/rules/vehicle category");
            var flying=typeof(ArcadeActivities).GetField("flying",BindingFlags.NonPublic|BindingFlags.Instance);var invalid=typeof(ArcadeActivities).GetField("invalid",BindingFlags.NonPublic|BindingFlags.Instance);
            flying.SetValue(activity,true);invalid.SetValue(activity,false);activity.SolidContact(Vector3.right,5);Check((bool)invalid.GetValue(activity),"RULE: airborne side impact invalidates a landing");
            activity.BeginAttempt();Check(!(bool)flying.GetValue(activity)&&(float)typeof(ArcadeActivities).GetField("warm",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(activity)==0,"RULE: retry midair requires a new grounded launch");
            int awarded=activity.Awards;Place(car.Body.position+Vector3.up*12,car.transform.forward);yield return new WaitForSeconds(2);Check(activity.Awards==awarded,"Spawn/teleport drop does not earn an activity award");
            string key=activity.Key(smash);flow.Pause();flow.QuitRace();flow.OpenGarage();flow.SelectVehicle(race.Forest?"atv":"tourer");flow.CloseGarage();Check(activity.Key(smash)!=key,"Vehicle categories separated");
            flow.StartRace();Check(!race.FreeRoam&&flow.State==RaceFlow.Stage.Countdown&&!activity.AttemptActive,"Roam to race restores countdown and clears activity state");
            while(flow.State!=RaceFlow.Stage.Racing)yield return null;
            Check(race.Racers.Count==4&&race.Progress.Started&&!guidance.Visible,"Race AI/timer restored without stale warning");
            Check(race.gates.All(g=>g.GetComponentsInChildren<Renderer>().Any(r=>r.enabled)),"Race markers restored");
            Check(race.Forest?race.EligibleVehicles.All(p=>p.Small):race.EligibleVehicles.Length==4,"Eligibility preserved");
            ThreeFeatureValidation.CaptureUi(root+"/race-return.png");
        }
        IEnumerator Persistence()
        {
            var activity=flow.Activities;string path=Path.Combine(flow.Save.DirectoryPath,"activities-v1.json");
            Check(File.Exists(path),"Existing isolated activity save present on cold process start");
            if(File.Exists(path)){var archive=JsonUtility.FromJson<ArcadeActivities.Archive>(File.ReadAllText(path));Check(activity.Results.results.Count>0&&archive.results.All(saved=>activity.Results.results.Any(loaded=>loaded.key==saved.key&&loaded.value==saved.value&&loaded.medal==saved.medal)),"All persisted category/PB/medal values loaded in a fresh process");}
            Check(!race.Progress.Started&&race.Progress.CompletedLaps==0,"Cold activity load does not create race progress");yield return null;
        }
        IEnumerator RoamRecovery()
        {
            foreach(var profile in race.EligibleVehicles)
            foreach(int direction in new[]{1,-1})
            {
                yield return Begin(profile.Id,true);var reset=car.GetComponent<VehicleRespawn>();
                foreach(var road in new[]{race.road,race.ambientRoad}.Where(r=>r).Distinct())
                {
                    reset.CancelRecovery();var p=road.At(road.Length*.4f,out var f);Place(p+Vector3.up*(car.suspensionLength-.1f),f*direction,4);yield return new WaitForSeconds(.9f);reset.RecordSafePosition();var earned=car.Body.position;
                    Place(earned+Vector3.Cross(Vector3.up,f)*30+Vector3.down*12,f);bool recovered=reset.TryRecoverLocal();road.Project(car.Body.position,out float lateral);
                    Check(recovered&&lateral<road.HalfWidth(road.Project(earned,out _))+1&&Vector3.Distance(car.Body.position,earned)<165&&Vector3.Dot(car.transform.forward,f*direction)>.8f,profile.Id+" roam "+road.name+" direction="+direction+" recent supported local recovery");
                    yield return new WaitForSeconds(.5f);Check(car.GroundedWheels>=2&&!race.Progress.Started&&race.Progress.PenaltySeconds==0,profile.Id+" roam support and race isolation after recovery");
                }
            }
        }
        IEnumerator Recovery()
        {
            foreach(var profile in race.EligibleVehicles)
            {
                yield return Begin(profile.Id,false);foreach(var d in race.Drivers)d.gameObject.SetActive(false);
                foreach(string scenario in new[]{"hill-bottom","tree","ramp-side","ramp-underside","water","occupied","overhead"})
                {
                    var reset=car.GetComponent<VehicleRespawn>();reset.CancelRecovery();float s=race.road.Length*(scenario=="hill-bottom"?.22f:scenario=="tree"?.34f:scenario=="ramp-side"?.53f:.71f);var p=race.road.At(s,out var f);
                    Vector3? hazard=null;
                    if(scenario=="tree")
                    {
                        var tree=FindObjectsByType<Collider>().Where(c=>!c.isTrigger&&c.name.IndexOf("trunk",StringComparison.OrdinalIgnoreCase)>=0).OrderBy(c=>Vector3.Distance(c.bounds.center,p)).FirstOrDefault();
                        if(tree){hazard=tree.bounds.center;s=race.road.Project(hazard.Value,out _);p=race.road.At(s,out f);}
                    }
                    if(scenario=="water"&&ShallowWater.Active.Count>0){var water=ShallowWater.Active.OrderBy(w=>Vector3.Distance(w.transform.position,p)).First();hazard=water.transform.position+Vector3.up*.25f;s=race.road.Project(hazard.Value,out _);p=race.road.At(s,out f);}
                    if(scenario.StartsWith("ramp"))
                    {
                        var ramp=FindObjectsByType<MeshCollider>().FirstOrDefault(c=>c.enabled&&(c.name=="Reverse supported roadworks transition"||c.name.StartsWith("Takeoff -")));
                        if(ramp){s=race.road.Project(ramp.bounds.center,out _);p=race.road.At(s,out f);hazard=scenario=="ramp-underside"?p+Vector3.up*.2f:ramp.bounds.center+Vector3.Cross(Vector3.up,f)*4;}
                    }
                    Place(p+Vector3.up*(car.suspensionLength-.1f),f);yield return new WaitForSeconds(.7f);reset.RecordSafePosition();float earned=reset.SafeStation;
                    int gate=race.Progress.NextGate,laps=race.Progress.CompletedLaps;double penalty=race.Progress.PenaltySeconds;double elapsed=race.Progress.RaceTime(race.Clock);
                    GameObject obstacle=null;if(scenario=="occupied"||scenario=="overhead"){obstacle=GameObject.CreatePrimitive(PrimitiveType.Cube);obstacle.name="Diagnostic occupied reset pad";obstacle.transform.position=race.road.At(earned-2,out _)+Vector3.up*(scenario=="overhead"?1.2f:1);obstacle.transform.localScale=new(5,2,9);}
                    var displaced=hazard??(p+Vector3.Cross(Vector3.up,f)*18+Vector3.down*(scenario=="hill-bottom"?18:0));Place(displaced,f);
                    bool success=reset.TryRecoverLocal();float recovered=race.road.Project(car.Body.position,out float lateral);float delta=Mathf.Repeat(recovered-earned+race.road.Length*.5f,race.road.Length)-race.road.Length*.5f;
                    Check(success&&lateral<race.road.HalfWidth(recovered)+1&&delta<=.5f&&delta>=-165,profile.Id+" "+scenario+" supported earned-route recovery; delta="+delta.ToString("F2"));
                    Check(gate==race.Progress.NextGate&&laps==race.Progress.CompletedLaps&&penalty==race.Progress.PenaltySeconds&&race.Progress.RaceTime(race.Clock)>=elapsed,profile.Id+" "+scenario+" progress/time/penalty preserved");
                    if(success){yield return new WaitForSeconds(.5f);Check(car.GroundedWheels>=2&&car.transform.up.y>.65f,profile.Id+" "+scenario+" remains supported after ordinary physics frames");}
                    if(obstacle)Destroy(obstacle);yield return null;
                }
                foreach(var branch in race.Branches)
                {
                    var reset=car.GetComponent<VehicleRespawn>();race.Racers[0].Branch.Begin(branch);float s=branch.Length*.5f;var p=branch.At(s,out var f);race.Racers[0].Branch.Advance(p-f*.5f,p,f);Place(p+Vector3.down*8,f);int gate=race.Progress.NextGate;bool success=reset.TryRecoverLocal();float at=branch.Project(car.Body.position,out float lateral);
                    Check(success&&lateral<branch.halfWidth&&at<=s+.1f&&race.Progress.NextGate==gate,profile.Id+" "+branch.title+" branch/cave recovery retains earned station and gate entitlement");race.Racers[0].Branch.Clear();yield return null;
                }
            }
        }
        IEnumerator Speed()
        {
            bool stress=Arg("-recordsStress","no")=="yes";
            foreach(var profile in stress?race.EligibleVehicles.Take(1):race.EligibleVehicles)
            foreach(int direction in stress?new[]{1}:new[]{1,-1})
            foreach(int trapIndex in stress?new[]{0}:new[]{0,1})
            for(int repeat=0;repeat<(stress?14:1);repeat++)
            {
                yield return Begin(profile.Id,Arg("-raceActivities","no")!="yes");var activity=flow.Activities;var trap=activity.Sites.Where(s=>s.kind==ActivitySite.Kind.Speed).ElementAt(trapIndex);var f=trap.forward*direction;car.enabled=false;float target=race.Forest?28:40;
                float trapS=race.road.Project(trap.transform.position,out _);var p=race.road.At(trapS-direction*85,out var startForward)+Vector3.up*(car.suspensionLength-.1f);Place(p,startForward*direction,target);activity.NewSession();int awards=activity.Awards;float start=Time.time;
                while(Time.time-start<9&&Vector3.Dot(car.Body.position-trap.transform.position,f)<35)
                {
                    float station=race.road.Project(car.Body.position,out _);float look=Mathf.Clamp(7+Mathf.Abs(car.ForwardSpeed)*.48f,8,25);var line=race.road.At(station+direction*look,out _);var local=car.transform.InverseTransformPoint(line);float angle=Mathf.Atan2(local.x,local.z);float maxAngle=Mathf.Lerp(car.slowSteerAngle,car.fastSteerAngle,Mathf.Clamp01(Mathf.Abs(car.ForwardSpeed)/car.topSpeed))*Mathf.Deg2Rad;float steering=Mathf.Clamp(Mathf.Atan(2*car.wheelbase*Mathf.Sin(angle)/look)/maxAngle,-1,1);
                    car.Simulate(Mathf.Clamp01((target-car.ForwardSpeed)*.5f),car.ForwardSpeed>target+1?.2f:0,steering,Time.fixedDeltaTime);yield return new WaitForFixedUpdate();
                }
                Check(activity.Awards==awards+1&&activity.PersonalBest(trap)!=null,profile.Id+" trap="+trapIndex+" direction="+direction+" physical speed crossing awarded once; measured="+activity.LastSpeed.ToString("F2")+" m/s");
                Check(activity.LastSpeed>5&&activity.LastSpeed<=profile.Speed+2,profile.Id+" plausible measured speed");
                ThreeFeatureValidation.CaptureUi(root+"/"+profile.Id+"-"+direction+"-"+trapIndex+"-trap.png");
                int counted=activity.Awards;Place(trap.transform.position+f, f);yield return new WaitForFixedUpdate();Place(trap.transform.position-f,f);yield return new WaitForFixedUpdate();Check(activity.Awards==counted,"Teleport/reset cannot award a crossing");car.enabled=true;
                if(stress&&repeat==13){Check(activity.Records.Board(activity.Key(trap)).Count==10,"Fourteen actual crossings retain top ten attempts");flow.Pause();flow.OpenActivities();yield return null;yield return new WaitForEndOfFrame();ThreeFeatureValidation.CaptureUi(root+"/top-ten-after-driving.png");flow.CloseExtras();flow.Resume();}
            }
        }
        IEnumerator SmashDrive()
        {
            foreach(var profile in race.EligibleVehicles)
            {
                yield return Begin(profile.Id,true);var activity=flow.Activities;var site=activity.Sites.First(s=>s.kind==ActivitySite.Kind.Smash);while(activity.Selected!=site)activity.Cycle();
                // Approach the fence from its open eastern end; the western end contains a tree.
                var ordered=site.props.OrderByDescending(p=>p.transform.position.x).ToArray();var first=ordered.First().transform.position;var last=ordered.Last().transform.position;var f=Vector3.ProjectOnPlane(last-first,Vector3.up).normalized;
                car.enabled=false;var start=first-f*12;if(Physics.Raycast(start+Vector3.up*15,Vector3.down,out var ground,50,1,QueryTriggerInteraction.Ignore))start.y=ground.point.y+car.suspensionLength-.1f;
                Place(start,f);activity.NewSession();yield return new WaitForSeconds(2.5f);activity.BeginAttempt();float begin=Time.time;int before=activity.Awards;
                while(activity.AttemptActive&&Time.time-begin<27)
                {
                    var target=last+f*8;var local=car.transform.InverseTransformPoint(target);float yaw=Mathf.Atan2(local.x,local.z)*Mathf.Rad2Deg;
                    car.Simulate(Mathf.Clamp01((10-car.ForwardSpeed)*.5f),car.ForwardSpeed>11?.2f:0,Mathf.Clamp(yaw*.035f,-.6f,.6f),Time.fixedDeltaTime);yield return new WaitForFixedUpdate();
                }
                yield return null;
                Check(activity.Awards==before+1&&activity.PersonalBest(site)?.medal==3,profile.Id+" PHYSICAL distinct fence contacts; count="+activity.SmashCount+" elapsed="+(Time.time-begin).ToString("F2"));
                ThreeFeatureValidation.CaptureUi(root+"/"+profile.Id+"-smash.png");float best=activity.PersonalBest(site)?.value??0;activity.BeginAttempt();Check(activity.SmashCount==0&&activity.PersonalBest(site)?.value==best,profile.Id+" retry preserves PB and clears count");activity.Cancel();car.enabled=true;
            }
        }
        IEnumerator AiRecovery()
        {
            foreach(var profile in race.EligibleVehicles)
            {
                yield return Begin(profile.Id,false);foreach(var other in race.Drivers)other.gameObject.SetActive(false);
                var driver=car.gameObject.AddComponent<RoadDriver>();driver.Initialize(race,car,true,1,1);driver.Racer=race.Racers[0];
                float s=race.road.Length*.55f;var ramp=GameObject.Find("Reverse supported roadworks transition");if(ramp)s=race.road.Project(ramp.GetComponent<Renderer>().bounds.center,out _)-35;
                driver.Place(s,0);yield return new WaitForSeconds(.8f);var at=car.Body.position;var wall=GameObject.CreatePrimitive(PrimitiveType.Cube);wall.name="Diagnostic stationary ramp-side obstruction";wall.transform.SetPositionAndRotation(at+car.transform.forward*3,car.transform.rotation);wall.transform.localScale=new(12,4,1);Physics.SyncTransforms();
                int count=driver.RecoveryCount;float begin=Time.time;
                while(Time.time-begin<22&&driver.RecoveryCount==count)yield return null;
                Check(driver.RecoveryCount>count,profile.Id+" sustained main-route stall recovers within 22s; elapsed="+(Time.time-begin).ToString("F2"));
                Destroy(wall);float recovered=race.road.Project(car.Body.position,out _);yield return new WaitForSeconds(8);
                float now=race.road.Project(car.Body.position,out _);float distance=Mathf.Repeat(now-recovered+race.road.Length*.5f,race.road.Length)-race.road.Length*.5f;
                Check(distance>15&&driver.RecoveryCount<=count+2,profile.Id+" resumes ordinary route progress without a recovery loop; metres="+distance.ToString("F1"));
                driver.enabled=false;Destroy(driver);car.enabled=true;yield return null;
                driver=car.gameObject.AddComponent<RoadDriver>();driver.Initialize(race,car,true,1,1);driver.Racer=race.Racers[0];driver.Racer.Branch.Clear();foreach(var branch in race.Branches)branch.aiValidated=false;
                float blockedAt=race.Forest?100:race.road.Length*.8f;driver.Place(blockedAt,0);
                var pad=race.road.At(blockedAt+3,out var forward);wall=GameObject.CreatePrimitive(PrimitiveType.Cube);wall.name="Diagnostic persistent partial obstruction";wall.transform.SetPositionAndRotation(pad+Vector3.up,Quaternion.LookRotation(Vector3.ProjectOnPlane(forward,Vector3.up)));wall.transform.localScale=new(race.Forest?.6f:2.5f,2,1);Physics.SyncTransforms();begin=Time.time;
                float travel=0;using(var trace=new StreamWriter(root+"/persistent-"+profile.Id+".csv")){trace.WriteLine("seconds,travel,recoveries,x,y,z,safeStation,speed");while(Time.time-begin<35&&travel<60){yield return null;travel=Mathf.Repeat(race.road.Project(car.Body.position,out _)-blockedAt+race.road.Length*.5f,race.road.Length)-race.road.Length*.5f;var p=car.Body.position;trace.WriteLine($"{Time.time-begin:F2},{travel:F2},{driver.RecoveryCount},{p.x:F2},{p.y:F2},{p.z:F2},{car.GetComponent<VehicleRespawn>().SafeStation:F2},{car.ForwardSpeed:F2}");}}
                Check(travel>=60&&driver.RecoveryCount<=2,profile.Id+" persistent partial obstruction cleared without recovery loop; recoveries="+driver.RecoveryCount+" seconds="+(Time.time-begin).ToString("F2"));
                Destroy(wall);driver.enabled=false;Destroy(driver);car.enabled=true;yield return null;
            }
        }
        IEnumerator OffRoute()
        {
            foreach(var profile in race.EligibleVehicles)
            {
                yield return Begin(profile.Id,false);foreach(var d in race.Drivers)d.gameObject.SetActive(false);car.enabled=false;
                var road=race.ambientRoad?race.ambientRoad:race.road;float s=3900;var p=road.At(s,out var f);Place(p+Vector3.up*(car.suspensionLength-.1f),f);
                var guidance=race.GetComponent<WrongWayGuidance>();guidance.Clear();float begin=Time.time;
                while(Time.time-begin<.7f){car.Simulate(0,0,0,Time.fixedDeltaTime);yield return new WaitForFixedUpdate();}
                int direction=Vector3.Dot(guidance.Direction,f)>=0?-1:1;Place(car.Body.position,f*direction,10);guidance.Clear();begin=Time.time;float onset=-1;
                while(Time.time-begin<8)
                {
                    float station=road.Project(car.Body.position,out _);var target=road.At(station+direction*12,out _);var local=car.transform.InverseTransformPoint(target);float angle=Mathf.Atan2(local.x,local.z);float max=Mathf.Lerp(car.slowSteerAngle,car.fastSteerAngle,10/car.topSpeed)*Mathf.Deg2Rad;
                    car.Simulate(Mathf.Clamp01((10-car.ForwardSpeed)*.5f),0,Mathf.Clamp(Mathf.Atan(2*car.wheelbase*Mathf.Sin(angle)/12)/max,-1,1),Time.fixedDeltaTime);yield return new WaitForFixedUpdate();
                    if(guidance.Visible&&onset<0){onset=Time.time-begin;ThreeFeatureValidation.CaptureUi(root+"/"+profile.Id+"-offroute-warning.png");}
                }
                race.road.Project(car.Body.position,out float offRoute);
                Check(onset>=4.8f&&onset<7,profile.Id+" ordinary street driving outside forest corridor produces warning after "+onset.ToString("F2")+"s; route distance="+offRoute.ToString("F1"));car.enabled=true;
            }
        }
        IEnumerator ForestJump()
        {
            var layout=FindAnyObjectByType<ForestLayout>();if(!layout){Check(false,"Forest layout required");yield break;}
            foreach(var profile in race.EligibleVehicles)
            foreach(float pace in new[]{.3f,.45f,.6f})
            {
                yield return Begin(profile.Id,true);foreach(var d in race.Drivers)d.gameObject.SetActive(false);var activity=flow.Activities;
                var driver=car.gameObject.AddComponent<RoadDriver>();driver.Initialize(race,car,true,1,pace);driver.Racer=race.Racers[0];foreach(var branch in race.Branches)branch.aiValidated=false;
                driver.Place(layout.jumpStarts[0]-80,0);activity.NewSession();activity.BeginAttempt();int before=activity.Awards;float begin=Time.time;
                while(Time.time-begin<35&&race.road.Project(car.Body.position,out _)<layout.jumpEnds[0]+70)yield return null;
                Check(activity.Awards>before,profile.Id+" pace="+pace+" actual authored Forest jump scoring / awarded distance="+activity.LastJumpAward.ToString("F2")+" latest airborne time="+activity.LastAirtime.ToString("F2"));
                ThreeFeatureValidation.CaptureUi(root+"/"+profile.Id+"-"+pace+"-jump.png");driver.enabled=false;Destroy(driver);car.enabled=true;yield return null;
            }
        }
    }
}
