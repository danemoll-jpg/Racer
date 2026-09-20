using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
namespace Racer
{
    // Opt-in compiled-player checks use isolated saves only.
    public sealed class CorrectionPassValidation:MonoBehaviour
    {
        static string Arg(string key,string fallback){var a=Environment.GetCommandLineArgs();int i=Array.IndexOf(a,key);return i>=0&&i+1<a.Length?a[i+1]:fallback;}
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot(){var a=Environment.GetCommandLineArgs();if(!a.Contains("-correctionPass")||!a.Contains("-racerTestSave")||FindAnyObjectByType<CorrectionPassValidation>())return;var go=new GameObject("Correction pass diagnostics");DontDestroyOnLoad(go);go.AddComponent<CorrectionPassValidation>();}
        string root;RaceFlow flow;RaceDirector race;readonly List<string> checks=new();
        void Check(bool value,string label){checks.Add((value?"PASS ":"FAIL ")+label);File.WriteAllLines(root+"/checks.txt",checks);}
        IEnumerator Start()
        {
            root=Arg("-evidence","Docs/CR082-089/systems");Directory.CreateDirectory(root);Application.runInBackground=true;QualitySettings.vSyncCount=0;Application.targetFrameRate=120;
            yield return null;yield return null;Bind();
            if(Arg("-correctionPass","systems")=="radio")yield return Radio();else if(Arg("-correctionPass","")=="departure")yield return Departure();else yield return Systems();
            File.WriteAllText(root+"/done.txt",checks.Count(c=>c.StartsWith("PASS"))+"/"+checks.Count+" passed");Application.Quit(checks.Any(c=>c.StartsWith("FAIL"))?2:0);
        }
        void Bind(){race=FindAnyObjectByType<RaceDirector>();flow=race.Flow;}
        IEnumerator Course(int i){flow.OpenCourses();flow.SelectCourse((i&1)!=0,i>=2);yield return null;yield return null;Bind();}
        IEnumerator Systems()
        {
            Check(Math.Abs(DisplayUnits.Mph(1609.344f/3600)-1)<.00001,"One mph conversion; canonical metres/second untouched");
            Check(Math.Abs(DisplayUnits.Feet(.3048f)-1)<.00001&&DisplayUnits.Distance(1609.344f).Contains("mi"),"Feet and miles presentation");
            string migration=Path.Combine(flow.Save.DirectoryPath,"migration");Directory.CreateDirectory(migration);
            var options=new RacerSave.Options{displayUnits="metric",music=.37f};File.WriteAllText(migration+"/settings.json",JsonUtility.ToJson(options));
            File.WriteAllText(migration+"/records.json",JsonUtility.ToJson(new RacerSave.Records{course="historical",lap=92.123456,race=310.456789}));
            string originalRecord=File.ReadAllText(migration+"/records.json");var migrated=new RacerSave(migration,"historical");migrated.SaveSettings();
            Check(migrated.Settings.displayUnits=="imperial"&&migrated.Settings.music==.37f&&migrated.Best.lap==92.123456&&File.ReadAllText(migration+"/records.json")==originalRecord,"Preference migration changes presentation only, preserving historical record bytes and precision");
            Check(ArcadeActivities.ActivityCourse("street-v12-corrections")=="street-v11-arcade"&&ArcadeActivities.ActivityCourse("lake-v5-corrections")=="lake-v4-arcade"&&ArcadeActivities.ActivityCourse("forest-reverse-v3-corrections")=="forest-reverse-v2-arcade","Unchanged activity categories keep existing PBs");
            var schedule=new HouseholdSchedule();var rng=new System.Random(82);int last=-1;var visits=new HashSet<int>();
            for(int i=0;i<40;i++){schedule.Next(rng,out int scene,out bool smoke);int visit=scene<2?scene:smoke?2:3;Check(!(scene<2&&smoke)&&visit!=last,"Shared visit exclusive and no immediate repeat "+i);last=visit;visits.Add(visit);if(i%4==3){Check(visits.Count==4,"Complete four-state shuffle bag "+i);visits.Clear();}}
            for(int course=0;course<4;course++)
            {
                yield return Course(course);race.opponents=false;race.traffic=false;
                Check(race.gates[0].name.Contains("START FINISH"),race.courseName+" true finish index zero");
                var spawn=race.vehicle.GetComponent<VehicleRespawn>().spawnPoint;
                Check(race.road.Relative(race.Origin,race.road.Project(spawn.position,out _))<45,race.courseName+" grid behind timing start");
                foreach(var profile in race.EligibleVehicles)
                {
                    flow.OpenGarage();flow.SelectVehicle(profile.Id);flow.CloseGarage();yield return null;
                    Check(race.vehicle.GetComponent<VehicleConfiguration>().profileId==profile.Id,"Actual gate-test profile selected "+profile.Id);
                    var box=race.vehicle.GetComponent<BoxCollider>();race.vehicle.transform.rotation=race.gates[0].transform.rotation;
                    foreach(var gate in new[]{race.gates[0],race.gates[1]})foreach(float side in new[]{-1f,1f})foreach(float flight in new[]{0f,3f})
                    {
                        race.vehicle.transform.rotation=gate.transform.rotation;
                        float extent=box.size.x*.5f;
                        foreach(float extra in new[]{-.1f,.15f,.30f})
                        {
                            float x=side*(gate.halfWidth+extent+extra);
                            var a=gate.transform.TransformPoint(new Vector3(x,flight,-6));var b=gate.transform.TransformPoint(new Vector3(x,flight,6));
                            bool cross=gate.TryCross(a,b,box,out bool forward,out float fraction);
                            Check(cross==(extra<.2f)&&(!cross||forward&&Math.Abs(fraction-.5)<.001),$"{race.courseName}/{profile.Id}/{gate.name} edge={side} airborne={flight} beyond-body={extra} swept 12m");
                        }
                    }
                }
                var progress=new RaceProgress(race.gates.Length-1,2);progress.BeginTiming(0);progress.Cross(0,true,1);progress.Cross(0,true,1.1);Check(progress.CompletedLaps==0,"Start arms once "+course);progress.Miss(1,5);progress.Miss(1,5);Check(progress.PenaltySeconds==5,"Genuine miss +5 once "+course);
                var life=race.GetComponent<AmbientLife>();var observed=new HashSet<int>();
                for(int session=0;session<8;session++)
                {
                    if(session%2==0)flow.StartFreeRoam();else flow.StartRace();
                    yield return null;int state=life.DanScene<2?life.DanScene:life.FriendScene?2:3;bool firstView=observed.Add(state);
                    Check(!(life.DanScene<2&&life.FriendScene),$"Global household state {course}/{session}/{state}");
                    var people=(IEnumerable)typeof(AmbientLife).GetField("people",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(life);
                    var actions=new HashSet<int>();foreach(var person in people){var type=person.GetType();int action=(int)type.GetField("action").GetValue(person);var t=(Transform)type.GetField("root").GetValue(person);if(action<3&&t.gameObject.activeSelf)actions.Add(action);}
                    Check(actions.Count<2&&(!actions.Any()||actions.Single()==state),"No stale pooled household group "+course+"/"+session);
                    if(firstView)foreach(var site in new[]{("football",life.football[0]),("coffee",life.coffee[0]),("smoking",life.smoking[0])})LivingWorldValidation.Capture(root+$"/households-{course}-{state}-{site.Item1}.png",site.Item2+new Vector3(8,5,10),site.Item2+Vector3.up);
                    int seed=life.Seed;flow.Pause();yield return null;flow.Resume();race.vehicle.GetComponent<VehicleRespawn>().ResetVehicle();
                    Check(life.Seed==seed,"Selection stable through pause/local recovery "+course+"/"+session);
                    flow.Pause();flow.QuitRace();
                }
                Check(observed.Count==4,"All household states reached on course "+course);
                Check(new RacerSave(flow.Save.DirectoryPath,"legacy").Settings.households.lastVisit==flow.Save.Settings.households.lastVisit,"Shared visit persisted across save reload "+course);
            }
        }
        IEnumerator Radio()
        {
            string folder=Path.GetFullPath(root+"/music");Directory.CreateDirectory(folder);Wave(folder+"/Long test A.wav");Wave(folder+"/Long test B.wav");
            var radio=flow.Radio;radio.SetFolder(folder);float until=Time.realtimeSinceStartup+20;while(radio.Scanning&&Time.realtimeSinceStartup<until)yield return null;if(radio.ChannelName=="Off")radio.Toggle();
            yield return new WaitForSecondsRealtime(3);Check(radio.Playing,"Long local WAV playback established");
            var source=radio.GetComponent<AudioSource>();string path=radio.CurrentPath,channel=radio.ChannelName;var clip=source.clip;
            var keyboard=InputSystem.AddDevice<Keyboard>();var pad=InputSystem.AddDevice<Gamepad>();
            for(int i=0;i<12;i++)
            {
                float before=radio.PlaybackSeconds;yield return Course(i%4);
                Check(flow.Radio==radio&&source.clip==clip&&radio.CurrentPath==path&&radio.ChannelName==channel,"Course navigation preserves voice/clip/channel/song "+i);
                Check(radio.PlaybackSeconds>=before-.1f,"Playback position never resets on course selection "+i+" "+before+" -> "+radio.PlaybackSeconds);
                flow.OpenCourses();InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.DownArrow));yield return null;InputSystem.QueueStateEvent(keyboard,new KeyboardState());
                InputSystem.QueueStateEvent(pad,new GamepadState().WithButton(GamepadButton.DpadRight));yield return null;InputSystem.QueueStateEvent(pad,new GamepadState());yield return null;
                Check(radio.CurrentPath==path&&radio.ChannelName==channel,"Synthetic keyboard/controller menu navigation does not skip "+i);flow.CloseGarage();
            }
            InputSystem.RemoveDevice(keyboard);InputSystem.RemoveDevice(pad);
            flow.StartFreeRoam();yield return null;flow.Pause();flow.QuitRace();Check(radio.CurrentPath==path&&source.clip==clip,"Returning to menu preserves song");
            radio.Next();yield return new WaitForSecondsRealtime(2);Check(radio.Playing&&radio.CurrentPath!=path,"Explicit next still advances");
            Check(new RacerSave(flow.Save.DirectoryPath,"legacy").Settings.musicFolder==folder,"Music preference survives course changes and save reload");
            source.time=source.clip.length-.1f;yield return new WaitForSecondsRealtime(2);Check(radio.Playing&&radio.PlaybackSeconds<5,"Natural completion still advances");
        }
        IEnumerator Departure()
        {
            foreach(string profile in new[]{"moto","atv"})foreach(bool shortcut in new[]{false,true})
            {
                yield return Course(3);flow.OpenGarage();flow.SelectVehicle(profile);flow.CloseGarage();race.opponents=race.traffic=false;
                var first=race.Branches.OrderBy(b=>b.entryRoad).First();foreach(var b in race.Branches)b.aiValidated=shortcut&&b==first;
                flow.StartRace();while(flow.State!=RaceFlow.Stage.Racing)yield return null;
                var pilot=race.vehicle.gameObject.AddComponent<RoadDriver>();pilot.Initialize(race,race.vehicle,true,1,1);pilot.Racer=race.Racers[0];
                var chase=FindAnyObjectByType<ChaseCamera>();chase.Snap();yield return null;
                ThreeFeatureValidation.CaptureUi(root+$"/{profile}-{shortcut}-grid.png");float begin=Time.time,next=3;bool used=false;int seed=race.GetComponent<AmbientLife>().Seed;
                using(var trace=new StreamWriter(root+$"/{profile}-{shortcut}.csv"))
                {
                    trace.WriteLine("time,station,branch,speed,throttle,brake,recoveries,gate,penalty");
                    while(Time.time-begin<40)
                    {
                        yield return new WaitForFixedUpdate();used|=race.Racers[0].Branch.Route==first;
                        trace.WriteLine($"{Time.time-begin:F3},{race.road.Project(race.vehicle.Body.position,out _):F3},{race.Racers[0].Branch.Route?.title},{race.vehicle.ForwardSpeed:F3},{pilot.LastThrottle:F3},{pilot.LastBrake:F3},{pilot.RecoveryCount},{race.Progress.NextGate},{race.Progress.PenaltySeconds}");
                        if(Time.time-begin>=next&&next<=15){ThreeFeatureValidation.CaptureUi(root+$"/{profile}-{shortcut}-{next}.png");next+=3;}
                    }
                }
                Check(used==shortcut,profile+" actual departure chooses "+(shortcut?"optional shortcut":"main route"));
                Check(race.Progress.PenaltySeconds==0,profile+" departure legitimate route choice penalty-free "+shortcut);
                Check(race.GetComponent<AmbientLife>().Seed==seed,"Household stable while driving departure "+profile+shortcut);
                flow.Pause();flow.QuitRace();
            }
        }
        static void Wave(string path)
        {
            using var w=new BinaryWriter(File.Create(path));int rate=8000,n=rate*600;w.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));w.Write(36+n*2);w.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));w.Write(16);w.Write((short)1);w.Write((short)1);w.Write(rate);w.Write(rate*2);w.Write((short)2);w.Write((short)16);w.Write(System.Text.Encoding.ASCII.GetBytes("data"));w.Write(n*2);for(int i=0;i<n;i++)w.Write((short)(Math.Sin(i*2*Math.PI*220/rate)*500));
        }
    }
}
