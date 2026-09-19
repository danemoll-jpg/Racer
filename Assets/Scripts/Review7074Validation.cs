using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
namespace Racer
{
    // Release-player opt-in harness. Selection remains unforced; saves are isolated.
    public sealed class Review7074Validation:MonoBehaviour
    {
        static string Arg(string key,string fallback){var a=Environment.GetCommandLineArgs();int i=Array.IndexOf(a,key);return i>=0&&i+1<a.Length?a[i+1]:fallback;}
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot(){var a=Environment.GetCommandLineArgs();if(!a.Contains("-review7074")||!a.Contains("-racerTestSave")||FindAnyObjectByType<Review7074Validation>())return;var g=new GameObject("CR070-074 diagnostics");DontDestroyOnLoad(g);g.AddComponent<Review7074Validation>();}
        RaceDirector race;RaceFlow flow;ArcadeVehicle car;WrongWayGuidance guidance;string root;
        readonly List<string> checks=new();
        void Check(bool pass,string label){checks.Add((pass?"PASS ":"FAIL ")+label);File.WriteAllLines(root+"/checks.txt",checks);}
        IEnumerator Start()
        {
            Application.runInBackground=true;root=Arg("-evidence","Docs/CR070-074/candidate");Directory.CreateDirectory(root);
            string scene=Arg("-course","StreetLoopGreybox");if(SceneManager.GetActiveScene().name!=scene)SceneManager.LoadScene(scene);yield return null;yield return null;
            race=FindAnyObjectByType<RaceDirector>();flow=race.Flow;car=race.vehicle;guidance=race.GetComponent<WrongWayGuidance>();
            QualitySettings.vSyncCount=0;Application.targetFrameRate=120;InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
            switch(Arg("-review7074","rules")){case "households":yield return Households();break;case "hairpin":yield return Hairpin();break;case "physical":yield return Physical();break;default:yield return Rules();break;}
            File.WriteAllText(root+"/done.txt",$"{checks.Count(c=>c.StartsWith("PASS"))}/{checks.Count} passed");Application.Quit(checks.Any(c=>c.StartsWith("FAIL"))?2:0);
        }
        IEnumerator Begin()
        {flow.StartRace();while(flow.State!=RaceFlow.Stage.Racing)yield return null;yield return new WaitForSeconds(.1f);}
        void Place(Vector3 p,Vector3 forward)
        {car.Body.position=p;car.Body.rotation=Quaternion.LookRotation(Vector3.ProjectOnPlane(forward,Vector3.up));car.transform.SetPositionAndRotation(car.Body.position,car.Body.rotation);if(!car.Body.isKinematic)car.Body.linearVelocity=car.Body.angularVelocity=Vector3.zero;car.ClearSteering();Physics.SyncTransforms();race.ResetSampling(p,Time.timeAsDouble);}
        IEnumerator Households()
        {
            Check(AmbientLife.ForcedSeed==0,"Ordinary unforced release selection");race.opponents=race.traffic=false;
            var life=race.GetComponent<AmbientLife>();var visits=new List<string>();
            for(int visit=0;visit<4;visit++)
            {
                yield return Begin();car.Body.isKinematic=true;car.enabled=false;
                int dan=life.DanScene;bool friend=life.FriendScene;
                var residents=life.transform.Cast<Transform>().Where(t=>t.name=="Ambient resident").ToArray();
                Check(residents.Take(3).Count(t=>t.gameObject.activeSelf)==(dan==0?3:0)&&residents.Skip(3).Take(2).Count(t=>t.gameObject.activeSelf)==(dan==1?2:0)&&residents.Skip(5).Take(2).Count(t=>t.gameObject.activeSelf)==(friend?2:0),"No stale household figures visit "+visit);
                var street=race.ambientRoad?race.ambientRoad:race.road;
                foreach(string site in new[]{"dan","friend"})
                {
                    var people=site=="dan"?(dan==0?residents.Take(3):dan==1?residents.Skip(3).Take(2):Enumerable.Empty<Transform>()):friend?residents.Skip(5).Take(2):Enumerable.Empty<Transform>();
                    var position=site=="dan"?life.football[0]:life.smoking[0];float s=street.Project(position,out _);
                    int bestVisible=0;
                    foreach(float approach in new[]{-22f,-10f,0f,10f})
                    {
                        var at=street.At(s+approach,out var forward);Place(at+Vector3.up*.7f,forward);
                        Camera.main.GetComponent<ChaseCamera>().Snap();yield return new WaitForSeconds(.22f);
                        var cam=Camera.main;int visible=0;
                        foreach(var person in people)
                        {
                            var head=person.position+Vector3.up*1.25f;var screen=cam.WorldToViewportPoint(head);
                            bool inFrame=screen.z>0&&screen.x>0&&screen.x<1&&screen.y>0&&screen.y<1;
                            bool clear=!Physics.Linecast(cam.transform.position,head,1,QueryTriggerInteraction.Ignore);
                            if(inFrame&&clear)visible++;
                        }
                        bestVisible=Math.Max(bestVisible,visible);
                        ThreeFeatureValidation.CaptureUi(root+$"/visit-{visit}-{site}-{approach}.png");
                    }
                    int expected=people.Count();Check(bestVisible>0||expected==0,$"Normal street approach shows {site}, state={dan}/{friend}, visible={bestVisible}/{expected}");
                    visits.Add($"{visit},{life.Seed},{dan},{friend},{site},{bestVisible},{expected}");
                }
                Check(!life.transform.Find("Football").gameObject.activeSelf||dan==0,"Ball only during football");
                flow.Pause();yield return new WaitForSecondsRealtime(.2f);flow.Resume();car.Body.isKinematic=false;car.enabled=true;
                car.GetComponent<VehicleRespawn>().TryRecoverLocal();yield return null;
                Check(life.DanScene==dan&&life.FriendScene==friend,"Occupancy stable through pass/pause/local recovery");
                flow.Pause();flow.QuitRace();
            }
            File.WriteAllLines(root+"/ordinary-visits.csv",new[]{"visit,seed,dan,friend,site,visible,selected"}.Concat(visits));
            var saved=new RacerSave(flow.Save.DirectoryPath,"unused").Settings;var deck=race.Forest?saved.forestHouseholds:saved.streetHouseholds;
            Check(deck.lastDan==life.DanScene&&deck.lastFriend==(life.FriendScene?1:0),"Final visit persisted for next application launch");
        }
        IEnumerator Rules()
        {
            flow.OpenCourses();yield return null;yield return null;ThreeFeatureValidation.CaptureUi(root+"/course-selection.png");var labels=FindObjectsByType<UnityEngine.UI.Text>().Select(t=>t.text).ToArray();Check(new[]{"Street Loop","Forest Loop","Street Loop Reverse","Forest Loop Reverse"}.All(labels.Contains),"All four course choices visible in selection menu");flow.CloseGarage();yield return null;
            // State-machine boundary tests complement real signed-route samples below.
            guidance.enabled=false;
            foreach(float speed in new[]{0f,-2f}){guidance.Clear();for(int i=0;i<400;i++)guidance.Observe(speed,true,true,.02f);Check(!guidance.Visible,"Stationary/slow maneuver speed="+speed);}
            guidance.Clear();for(int i=0;i<240;i++)guidance.Observe(-9,true,true,.02f);Check(!guidance.Visible,"Brief reverse under five seconds suppressed");
            for(int i=0;i<12;i++)guidance.Observe(-9,true,true,.02f);Check(guidance.Visible,"Continuous reverse beyond five seconds warns");
            for(int i=0;i<10;i++)guidance.Observe(7,true,true,.02f);Check(!guidance.Visible,"Correct movement clears within 200ms");
            guidance.Clear();for(int i=0;i<300;i++)guidance.Observe(-9,true,i%25!=0,.02f);Check(guidance.Visible,"Brief projection noise cannot continually erase sustained travel");
            guidance.Clear();for(int i=0;i<300;i++)guidance.Observe(i%30==0?2:-9,true,true,.02f);Check(guidance.Visible,"Single noisy forward samples do not restart detection");
            guidance.Clear();for(int i=0;i<40;i++)guidance.Observe(-9,true,true,.02f);for(int i=0;i<40;i++)guidance.Observe(0,true,true,.02f);Check(!guidance.Visible&&guidance.WrongSeconds==0,"Brief escape reverse followed by stop clears evidence");
            for(int i=0;i<400;i++)guidance.Observe(-12,false,true,.02f);Check(!guidance.Visible&&guidance.WrongSeconds==0,"Airborne spin cannot accumulate");
            for(int i=0;i<400;i++)guidance.Observe(-12,true,false,.02f);Check(!guidance.Visible,"Invalid offroad segment cannot accumulate");
            race.opponents=race.traffic=false;yield return Begin();car.enabled=false;car.Body.isKinematic=true;race.enabled=false;
            var grounded=typeof(ArcadeVehicle).GetField("<GroundedWheels>k__BackingField",BindingFlags.Instance|BindingFlags.NonPublic);
            var tick=typeof(WrongWayGuidance).GetMethod("FixedUpdate",BindingFlags.Instance|BindingFlags.NonPublic);
            var samples=new List<string>();
            var sites=new List<(string,float,WoodlandRoute)>{("lap-seam",4,null),("ordinary",300,null),("bounded-reacquisition",300,null)};
            if(!race.Forest)sites.Add(("hairpin",race.reverseCourse?race.road.Length-1235:1235,null));
            foreach(var b in race.Branches)sites.Add((b.title,Mathf.Min(140,b.Length*.55f),b));
            if(race.Branches.Length>0)sites.Add(("branch-reacquisition",300,race.Branches[0]));
            foreach(var site in sites)
            {
                foreach(bool reverse in new[]{false,true})
                {
                    guidance.Clear();race.Racers[0].Branch.Clear();if(site.Item3)race.Racers[0].Branch.Begin(site.Item3);
                    for(int i=0;i<300;i++)
                    {
                        float s=site.Item2+(reverse?-1:1)*i*.16f;
                        var p=site.Item3&&site.Item1!="branch-reacquisition"?site.Item3.At(s,out var forward):race.road.At(s,out forward);
                        if(site.Item1=="bounded-reacquisition")p+=Vector3.Cross(Vector3.up,forward).normalized*16;
                        Place(p+Vector3.up*.65f,-forward);grounded.SetValue(car,4);tick.Invoke(guidance,null);
                        if(i==240)Check(!guidance.Visible,site.Item1+" no premature warning reverse="+reverse);
                    }
                    Check(guidance.Visible==reverse,site.Item1+" signed travel despite backwards-facing body reverse="+reverse);
                    samples.Add($"{site.Item1},{reverse},{guidance.WrongSeconds},{guidance.Visible},{guidance.Direction}");
                    if(reverse)
                    {
                        var input=car.GetComponent<VehicleInput>();var pad=InputSystem.AddDevice<Gamepad>();InputSystem.QueueStateEvent(pad,new GamepadState{rightTrigger=.6f});yield return null;yield return null;
                        Check(input.UsingGamepad&&input.ResetControlLabel=="Y","Emulated Gamepad prompt uses actual Y binding");Camera.main.GetComponent<ChaseCamera>().Snap();ThreeFeatureValidation.CaptureUi(root+"/wrong-gamepad-"+sites.IndexOf(site)+".png");InputSystem.RemoveDevice(pad);
                        var keyboard=InputSystem.AddDevice<Keyboard>();InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.W));yield return null;yield return null;
                        Check(!input.UsingGamepad&&input.ResetControlLabel.ToUpperInvariant()=="R","Emulated keyboard prompt uses actual R binding");ThreeFeatureValidation.CaptureUi(root+"/wrong-keyboard-"+sites.IndexOf(site)+".png");InputSystem.RemoveDevice(keyboard);
                        float time=guidance.WrongSeconds;flow.Pause();yield return new WaitForSecondsRealtime(.2f);tick.Invoke(guidance,null);Check(guidance.WrongSeconds==time,"Pause does not advance timer");flow.Resume();
                        race.enabled=true;car.Body.isKinematic=false;bool recovered=car.GetComponent<VehicleRespawn>().TryRecoverLocal();Check(recovered&&!guidance.Visible,"Actual local recovery clears warning recovered="+recovered);car.Body.isKinematic=true;race.enabled=false;
                    }
                }
            }
            File.WriteAllLines(root+"/signed-route-samples.csv",samples);
            race.enabled=true;guidance.enabled=true;flow.StartRace();Check(!guidance.Visible&&guidance.WrongSeconds==0,"Restart clears guidance");
            Check(!FindObjectsByType<Transform>(FindObjectsInactive.Include).Any(t=>t.name=="South Cherokee Lane south"),"Removed extension absent including inactive colliders");
            Check(FindObjectsByType<ContinuationTraffic>().Length==3,"Three unrelated continuations preserved");
            while(flow.State!=RaceFlow.Stage.Racing)yield return null;
            guidance.enabled=false;for(int i=0;i<260;i++)guidance.Observe(-8,true,true,.02f);
            race.Progress.BeginTiming(race.Clock);race.Progress.Cross(0,true,race.Clock);
            for(int lap=0;lap<race.laps;lap++){for(int g=1;g<=race.Progress.CheckpointCount;g++)race.Progress.Cross(g,true,race.Clock+lap*3+1);race.Progress.Cross(0,true,race.Clock+lap*3+2);}
            flow.LapCompleted();tick.Invoke(guidance,null);Check(race.Progress.Finished&&!guidance.Visible,"Finish clears warning");
            SceneManager.LoadScene(race.Forest?"StreetLoopGreybox":"LakeWoods");yield return null;yield return null;
            Check(FindObjectsByType<WrongWayGuidance>().Length==1&&!FindAnyObjectByType<WrongWayGuidance>().Visible,"Track change creates one clear guidance instance");
        }
        IEnumerator Hairpin()
        {
            Check(!race.Forest,"Street hairpin physical matrix");race.opponents=race.traffic=false;
            var rows=new List<string>{"vehicle,mode,elapsed,station,lateral,minUpright,recoveries,warnings"};
            foreach(var profile in VehicleProfile.All)
            {
                flow.OpenGarage();flow.SelectVehicle(profile.Id);flow.CloseGarage();yield return Begin();
                var pilot=car.gameObject.AddComponent<RoadDriver>();pilot.Initialize(race,car,true,1,1);pilot.Racer=race.Racers[0];
                foreach(bool offroad in new[]{false,true})
                {
                    float beginS=race.reverseCourse?race.road.Length-1350:1110,endS=race.reverseCourse?race.road.Length-1110:1350;pilot.Place(beginS,offroad?-7:1.7f);guidance.Clear();float start=Time.time,minUp=1;int warnings=0;
                    while(Time.time-start<35)
                    {
                        float s=race.road.Project(car.Body.position,out _);if(s>endS&&s<endS+100)break;
                        minUp=Mathf.Min(minUp,car.transform.up.y);if(guidance.Visible)warnings++;
                        if(Time.time-start>6&&Time.time-start<6.03f)ThreeFeatureValidation.CaptureUi(root+"/hairpin-"+profile.Id+"-"+offroad+".png");
                        yield return null;
                    }
                    float end=race.road.Project(car.Body.position,out float lateral);rows.Add($"{profile.Id},{(offroad?"reentry":"AI pilot")},{Time.time-start:F3},{end:F3},{lateral:F3},{minUp:F3},{race.Racers[0].Recoveries},{warnings}");
                    Check(end>endS&&end<endS+100&&minUp>.65f,profile.Id+" physical hairpin completed offroad="+offroad);Check(warnings==0,profile.Id+" no false wrong-way warning at hairpin");
                }
                Destroy(pilot);yield return null;car.enabled=true;flow.Pause();flow.QuitRace();
            }
            File.WriteAllLines(root+"/hairpin.csv",rows);
        }
        IEnumerator Physical()
        {
            race.opponents=race.traffic=false;foreach(var branch in race.Branches)branch.aiValidated=false;yield return Begin();
            var pilot=car.gameObject.AddComponent<RoadDriver>();pilot.Initialize(race,car,true,-1,.7f);pilot.Racer=race.Racers[0];
            pilot.Place(race.Forest?260:900,1.7f);guidance.Clear();float start=Time.time,first=-1;float meaningfulAt=-1;var trace=new List<string>{"time,speed,signedSpeed,valid,grounded,timer,visible,rendered"};
            while(Time.time-start<12)
            {
                if(meaningfulAt<0&&guidance.SignedSpeed<=-3)meaningfulAt=Time.time-start; trace.Add($"{Time.time-start:F3},{car.ForwardSpeed:F3},{guidance.SignedSpeed:F3},{guidance.SampleValid},{car.GroundedWheels},{guidance.WrongSeconds:F3},{guidance.Visible},{(GameObject.Find("Wrong way guidance")!=null)}");
                if(guidance.Visible&&GameObject.Find("Wrong way guidance")){first=Time.time-start;break;}yield return null;
            }
            Check(first-meaningfulAt>=4.9f&&first-meaningfulAt<6,"Visible HUD onset after meaningful wrong travel="+(first-meaningfulAt)+"s; launch-to-HUD="+first+"s");
            yield return null; ThreeFeatureValidation.CaptureUi(root+"/physical-wrong.png");
            float timer=guidance.WrongSeconds;flow.Pause();yield return new WaitForSecondsRealtime(.5f);Check(guidance.WrongSeconds==timer,"Physical pause freezes wrong-way timer");flow.Resume();
            pilot.enabled=false;car.enabled=true;var before=car.Body.position;int gate=race.Progress.NextGate;double penalty=race.Progress.PenaltySeconds;
            var recovery=car.GetComponent<VehicleRespawn>();bool resetEvent=false;int recoveredGate=-1;double recoveredPenalty=-1;float resetDistance=999;
            void Recovered(){resetEvent=true;recoveredGate=race.Progress.NextGate;recoveredPenalty=race.Progress.PenaltySeconds;resetDistance=Vector3.Distance(before,car.Body.position);}
            recovery.Respawned+=Recovered;
            var keyboard=InputSystem.AddDevice<Keyboard>();InputSystem.QueueStateEvent(keyboard,new KeyboardState());yield return null;yield return null;
            before=car.Body.position;gate=race.Progress.NextGate;penalty=race.Progress.PenaltySeconds;
            InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.R));float wait=Time.time;
            while(!resetEvent&&Time.time-wait<1)yield return null;
            Check(resetEvent&&!guidance.Visible&&resetDistance<60&&recoveredGate==gate&&recoveredPenalty==penalty,$"Emulated R local recovery event={resetEvent}, clear={!guidance.Visible}, distance={resetDistance:F2}, gate={gate}/{recoveredGate}, penalty={penalty}/{recoveredPenalty}");recovery.Respawned-=Recovered;InputSystem.RemoveDevice(keyboard);
            pilot.Initialize(race,car,true,-1,.7f);pilot.enabled=true;pilot.Place(race.Forest?260:900,1.7f);guidance.Clear();start=Time.time;
            while(!guidance.Visible&&Time.time-start<12)yield return null;
            pilot.enabled=false;car.enabled=true;var pad=InputSystem.AddDevice<Gamepad>();InputSystem.QueueStateEvent(pad,new GamepadState());yield return null;yield return null;
            resetEvent=false;before=car.Body.position;gate=race.Progress.NextGate;penalty=race.Progress.PenaltySeconds;recovery.Respawned+=Recovered;
            InputSystem.QueueStateEvent(pad,new GamepadState().WithButton(GamepadButton.North));wait=Time.time;while(!resetEvent&&Time.time-wait<1)yield return null;
            Check(resetEvent&&!guidance.Visible&&car.GetComponent<VehicleInput>().UsingGamepad&&resetDistance<60&&recoveredGate==gate&&recoveredPenalty==penalty,$"Emulated Y recovery event={resetEvent}, clear={!guidance.Visible}, device={car.GetComponent<VehicleInput>().UsingGamepad}, distance={resetDistance:F2}");recovery.Respawned-=Recovered;InputSystem.RemoveDevice(pad);
            pilot.Initialize(race,car,true,1,.7f);pilot.enabled=true;pilot.Place(race.Forest?260:900,1.7f);start=Time.time;bool falseWarning=false;
            while(Time.time-start<7){falseWarning|=guidance.Visible;yield return null;}Check(!falseWarning,"Physical correct travel remains clear");
            File.WriteAllLines(root+"/physical-trace.csv",trace);
            Destroy(pilot);flow.Pause();flow.QuitRace();yield return null;Check(!guidance.Visible,"Quit clears guidance");
        }
    }
}
