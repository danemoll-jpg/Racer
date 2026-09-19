using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
namespace Racer
{
    // Explicit opt-in diagnostics: generated tones and an isolated save, never player data.
    public sealed class ThreeFeatureValidation:MonoBehaviour
    {
        string dir,mode; readonly List<string> checks=new();
        Camera renderCamera;
        RenderTexture performanceTarget;
        void LateUpdate(){if(renderCamera&&performanceTarget)renderCamera.Render();}
        static string Arg(string name,string fallback){var a=Environment.GetCommandLineArgs();int i=Array.IndexOf(a,name);return i>=0&&i+1<a.Length?a[i+1]:fallback;}
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot(){var a=Environment.GetCommandLineArgs();if(a.Contains("-threeFeatureTest")&&a.Contains("-racerTestSave")){var g=new GameObject("Three feature validation");DontDestroyOnLoad(g);g.AddComponent<ThreeFeatureValidation>();}}
        void Check(bool ok,string name){checks.Add((ok?"PASS ":"FAIL ")+name);File.WriteAllLines(dir+"/checks.txt",checks);}
        IEnumerator Start()
        {
            Application.runInBackground=true;dir=Path.GetFullPath(Arg("-evidence","Docs/CR040-054-055/tests"));Directory.CreateDirectory(dir);mode=Arg("-threeFeatureTest","rules");
            string scene=Arg("-course","StreetLoopGreybox");if(SceneManager.GetActiveScene().name!=scene)SceneManager.LoadScene(scene);
            yield return null;yield return null;
            if(mode=="rules")Rules();else if(mode=="radio")yield return Radio();else if(mode=="drive"||mode=="performance")yield return Drive();else if(mode=="jumps")yield return Jumps();
            else if(mode=="routes")
            {
                WoodlandValidation.Label="../CR040-054-055/"+Arg("-routeLabel","lake-routes");WoodlandValidation.ProfileFilter=Arg("-vehicle","");WoodlandValidation.Launch();
                while(true){if(FindAnyObjectByType<RaceFlow>()?.State==RaceFlow.Stage.Racing)Time.timeScale=float.Parse(Arg("-testSpeed","1"));yield return null;}
            }
            else if(mode=="ui")
            {
                var flow=FindAnyObjectByType<RaceFlow>();yield return null;CaptureUi(dir+"/ready.png");yield return new WaitForSecondsRealtime(.2f);
                for(int i=0;i<10;i++)flow.Boards.Add("display"+i,flow.Race.Category,false,80+i*.101,"original",DateTime.UtcNow.ToString("o"));
                flow.OpenBoards();yield return null;CaptureUi(dir+"/boards.png");yield return new WaitForSecondsRealtime(.3f);Check(true,"Ready and populated board rendered for visual inspection");
            }
            File.WriteAllText(dir+"/done.txt",$"{checks.Count(c=>c.StartsWith("PASS"))}/{checks.Count}");Application.Quit();
        }
        void Rules()
        {
            string root=Path.Combine(dir,"save-"+Guid.NewGuid().ToString("N"));var board=new RecordBoards(root);
            const string category="street-v8-landings-original-race4-d1-tourer-moto-atv-traffic-laps3";
            for(int i=0;i<25;i++)board.Add("attempt"+i,category,false,100-i*.1234567,"original","2026-09-19T00:00:00Z");
            var rows=board.Board(category,false);Check(rows.Count==10&&rows[0].id=="attempt24"&&rows[9].id=="attempt15","Fastest ten of 25 sorted by full precision");
            board.Add("tie1",category,false,80,"original");board.Add("tie2",category,false,80,"original");board.Add("nearTie",category,false,80.0000001,"original");
            Check(board.Board(category,false).Take(3).Select(e=>e.id).SequenceEqual(new[]{"tie1","tie2","nearTie"}),"Distinct tied attempts retained in stable order; unrounded near-tie follows");
            Check(board.Add("tie1",category,false,1,"original")==0,"Duplicate identity rejected even with changed time");
            board=new RecordBoards(root);Check(board.Add("tie1",category,false,1,"original")==0&&board.Board(category,false)[0].seconds==80,"Reload preserves order and duplicate protection");
            string raceCategory=category.Replace("d1","d0");for(int i=0;i<25;i++)board.Add("race-attempt"+i,raceCategory,true,300-i*.1234567,"original");
            Check(board.Board(raceCategory,true).Count==10&&board.Board(raceCategory,true)[0].id=="race-attempt24","Total-race board also retains fastest ten of 25 attempts");
            board.Add("race-tie1",raceCategory,true,200,"original");board.Add("race-tie2",raceCategory,true,200,"original");board.Add("race-near",raceCategory,true,200.0000001,"original");
            Check(new RecordBoards(root).Board(raceCategory,true).Take(3).Select(e=>e.id).SequenceEqual(new[]{"race-tie1","race-tie2","race-near"}),"Total-race ties and sub-millisecond order persist");
            var p=new RaceProgress(2,2);p.BeginTiming(10);p.Cross(0,true,12);p.Miss(1,55);p.Cross(2,true,15);p.Cross(0,true,30);
            Check(p.LastLap==23&&p.LapTimes[0]==23&&p.PenaltySeconds==5&&p.CurrentLapPenalty==0,"Lap penalty finalized on correct lap exactly once");
            board.CompletedLap("lapRace",category,"original",p);Check(board.CompletedRace("lapRace",category,"original",p,30)==0,"Incomplete race cannot create total-race entry");
            Check(new RecordBoards(root).Board(category,false).Any(e=>e.id=="lapRace/lap/1"),"Completed eligible lap persists before race completion/abandonment");
            p.Cross(1,true,32);p.Miss(2,999);p.Cross(0,true,50);board.CompletedLap("lapRace",category,"original",p);board.CompletedRace("lapRace",category,"original",p,999);
            Check(p.LastLap==25&&p.AdjustedTime(999)==50,"Final total uses frozen finish plus both penalties once");
            Check(board.Board(category,true).Single().seconds==50,"Stored race agrees with authoritative adjusted timing");
            Check(board.CompletedRace("lapRace",category,"original",p,999)==0&&board.CompletedLap("lapRace",category,"original",p)==0,"Repeated completion/results cannot duplicate lap or total");
            int beforeLaps=board.Board(category,false).Count,beforeRaces=board.Board(category,true).Count;
            var compatibility=new RacerSave(root,category);compatibility.SelectRecords(category);compatibility.RecordLap(p.LastLap);compatibility.RecordRace(p.AdjustedTime(999));
            board=new RecordBoards(root);Check(board.Board(category,false).Count==beforeLaps&&board.Board(category,true).Count==beforeRaces&&!board.Board(category,true).Any(e=>e.legacy),"Reload does not re-import newly written compatibility bests as duplicate legacy attempts");
            foreach(var key in new[]{category.Replace("street-v8-landings","lake-v1"),category.Replace("original","moto"),category.Replace("d1","d2"),category.Replace("tourer-moto-atv","atv-tourer-moto"),category.Replace("traffic","clear"),category.Replace("laps3","laps2")})
            {board.Add(key,key,true,40,"original");Check(board.Board(key,true).Count==1,"Race category isolated: "+key);}
            Check(board.Board(category,true).Count==1,"Original race board isolated from all alternatives");
            Check(board.Board(category.Replace("laps3","laps9"),false).Count==board.Board(category,false).Count,"Lap boards comparable across lap counts");
            foreach(double bad in new[]{-1,0,double.NaN,double.PositiveInfinity})Check(board.Add(Guid.NewGuid().ToString(),category,false,bad,"original")==0,"Invalid time rejected "+bad);
            string legacy=Path.Combine(dir,"legacy-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(legacy);
            var known=new RacerSave.Records{course=category,lap=50,race=150};string file=Path.Combine(legacy,"records-"+category+".json");File.WriteAllText(file,JsonUtility.ToJson(known));string bytes=File.ReadAllText(file);
            File.WriteAllText(Path.Combine(legacy,"records-obsolete.json"),JsonUtility.ToJson(new RacerSave.Records{course="unknown",lap=2,race=3}));
            board=new RecordBoards(legacy);Check(board.Board(category,false).Single().date==null&&board.Board(category,false).Single().legacy,"Known legacy best migrates with date explicitly unknown");
            board=new RecordBoards(legacy);Check(board.Board(category,false).Count==1&&board.Board(category,true).Count==1,"Legacy migration occurs once across reloads");
            Check(File.ReadAllText(file)==bytes&&board.Categories(false).Length==1,"Legacy source retained; incompatible history not relabeled");
            var race=FindAnyObjectByType<RaceDirector>();Check(race.gates.All(g=>g.gameObject.activeInHierarchy),"Director references only active course gates");
        }
        public static void CaptureUi(string file)
        {
            var camera=Camera.main;var canvas=FindAnyObjectByType<RaceHud>().GetComponent<Canvas>();var mode=canvas.renderMode;var oldCamera=canvas.worldCamera;float plane=canvas.planeDistance;
            // Hidden standalone windows don't automatically render secondary cameras.
            // Populate the real garage RenderTexture before capturing its RawImage.
            foreach(var auxiliary in Camera.allCameras)if(auxiliary!=camera&&auxiliary.enabled&&auxiliary.targetTexture)auxiliary.Render();
            var rt=new RenderTexture(1280,720,24);var previous=camera.targetTexture;var active=RenderTexture.active;
            canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=2;camera.targetTexture=rt;Canvas.ForceUpdateCanvases();camera.Render();RenderTexture.active=rt;
            var image=new Texture2D(1280,720,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1280,720),0,0);image.Apply();File.WriteAllBytes(file,image.EncodeToPNG());
            camera.targetTexture=previous;RenderTexture.active=active;canvas.renderMode=mode;canvas.worldCamera=oldCamera;canvas.planeDistance=plane;Destroy(image);Destroy(rt);
        }
        IEnumerator Jumps()
        {
            var race=FindAnyObjectByType<RaceDirector>();race.opponents=race.traffic=false;var car=race.vehicle;
            foreach(var profile in VehicleProfile.All)
            foreach(float station in new[]{620f,1020f,1390f})
            {
                if(race.Flow.State!=RaceFlow.Stage.Ready){race.Flow.Pause();race.Flow.QuitRace();}
                race.Flow.OpenGarage();race.Flow.SelectVehicle(profile.Id);race.Flow.CloseGarage();race.Flow.StartRace();while(race.Flow.State!=RaceFlow.Stage.Racing)yield return null;
                Time.timeScale=3;car.enabled=false;car.GetComponent<VehicleInput>().enabled=false;
                var p=roadPoint(station-30,out var forward);car.Body.position=p+Vector3.up*.7f;car.Body.rotation=Quaternion.LookRotation(Vector3.ProjectOnPlane(forward,Vector3.up));car.transform.SetPositionAndRotation(car.Body.position,car.Body.rotation);car.Body.linearVelocity=forward*32;car.Body.angularVelocity=Vector3.zero;car.ClearSteering();race.ResetSampling(car.Body.position,Time.timeAsDouble);
                float begin=Time.time,air=0,height=0;bool landed=false;
                while(Time.time-begin<20)
                {
                    float s=race.road.Project(car.Body.position,out _);var target=roadPoint(s+18,out _);var local=car.transform.InverseTransformPoint(target);float curvature=2*local.x/Mathf.Max(1,local.x*local.x+local.z*local.z);float angle=Mathf.Lerp(car.slowSteerAngle,car.fastSteerAngle,Mathf.Clamp01(car.ForwardSpeed/car.topSpeed));float steer=Mathf.Clamp(Mathf.Atan(curvature*car.wheelbase)*Mathf.Rad2Deg/angle,-1,1);
                    car.Simulate(Mathf.Clamp01((32-car.ForwardSpeed)*.4f),car.ForwardSpeed>34?.3f:0,steer,Time.fixedDeltaTime);
                    if(car.GroundedWheels<2)air+=Time.fixedDeltaTime;
                    height=Mathf.Max(height,car.Body.position.y-roadPoint(s,out _).y);
                    if(s>station+150&&car.GroundedWheels>=3&&car.transform.up.y>.8f){landed=true;break;}
                    yield return new WaitForFixedUpdate();
                }
                Check(landed&&air>.1f,$"{profile.Id} jump at {station}: landed={landed}, airtime={air:F2}s, peak above route={height:F2}m");
            }
            Vector3 roadPoint(float s,out Vector3 f)=>race.road.At(s,out f);
        }
        static void Tone(string file)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(file));using var w=new BinaryWriter(File.Create(file));const int n=44100*8;
            w.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));w.Write(36+n*2);w.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));w.Write(16);w.Write((short)1);w.Write((short)1);w.Write(44100);w.Write(88200);w.Write((short)2);w.Write((short)16);w.Write(System.Text.Encoding.ASCII.GetBytes("data"));w.Write(n*2);
            for(int i=0;i<n;i++)w.Write((short)(Math.Sin(i*2*Math.PI*330/44100)*2000));
        }
        IEnumerator Ready(LocalRadio r){float until=Time.realtimeSinceStartup+40;while((r.Scanning||r.Loading||(!r.Playing&&r.ChannelName!="Off"))&&Time.realtimeSinceStartup<until)yield return null;}
        IEnumerator Radio()
        {
            var flow=FindAnyObjectByType<RaceFlow>();var r=flow.Radio;yield return Ready(r);
            Check(r.Bundled&&r.Folder==Path.GetFullPath(Path.Combine(Application.dataPath,"..","Music")),"Fresh save defaults to portable bundled Music beside executable");
            if(Environment.GetCommandLineArgs().Contains("-portableOnly"))
            {
                Check(r.ChannelNames.SequenceEqual(new[]{"Country","Rock"}),"Extracted folder names are Country and Rock; nested album is not a channel");
                if(r.ChannelName=="Off"){r.Toggle();yield return Ready(r);}
                var samples=new float[1024];foreach(var source in r.GetComponents<AudioSource>())source.GetOutputData(samples,0);
                yield return new WaitForSecondsRealtime(.3f);
                double energy=0;foreach(var source in r.GetComponents<AudioSource>()){source.GetOutputData(samples,0);energy+=samples.Sum(s=>Math.Abs(s));}
                Check(r.Playing&&energy>0,"Extracted portable generated tone produces nonzero decoded DSP output (not human listening)");
                r.Toggle();yield return Ready(r);Check(r.ChannelName=="Rock"&&r.CurrentPath.Contains("Album"),"Nested album plays in Rock after one cycle");yield break;
            }
            string root=Path.GetFullPath("Temp/radio-fixtures-"+Guid.NewGuid().ToString("N"));
            Tone(root+"/root.wav");Tone(root+"/Rock/Artist/Album/one.wav");Tone(root+"/Rock/Other/two.wav");Tone(root+"/Country/song.wav");Directory.CreateDirectory(root+"/Empty");Directory.CreateDirectory(root+"/Broken");File.WriteAllText(root+"/Broken/corrupt.wav","RIFFcorrupt");
            r.SetFolder(root);yield return Ready(r);if(r.ChannelName=="Off"){r.Toggle();yield return Ready(r);}
            Check(r.ChannelNames.Contains("Rock")&&!r.ChannelNames.Contains("Album")&&!r.ChannelNames.Contains("Empty"),"Immediate folder channels include nested albums and exclude empty folders");
            var visited=new List<string>();for(int i=0;i<5;i++){yield return Ready(r);visited.Add(r.ChannelName);r.Toggle();yield return Ready(r);}
            Check(visited.Contains("General")&&visited.Contains("Country")&&visited.Contains("Rock")&&visited.Contains("Off")&&!visited.Contains("Broken"),"One action cycles playable General/Country/Rock/Off and skips corrupt channel");
            for(int i=0;i<6&&r.ChannelName!="Rock";i++){r.Toggle();yield return Ready(r);}
            string first=r.CurrentPath;r.Next();yield return Ready(r);string second=r.CurrentPath;
            Check(first!=second&&second.Contains("Rock"),"Shuffle avoids immediate repeat and stays in channel");r.Previous();yield return Ready(r);Check(r.CurrentPath==first,"Previous returns channel-specific history");
            for(int i=0;i<24;i++)r.Toggle();yield return Ready(r);Check(r.GetComponents<AudioSource>().Count(s=>s.isPlaying)<=1,"Rapid switching produces at most one playing source on radio host");
            r.Rescan();r.Rescan();yield return Ready(r);Check(!r.Scanning&&!r.Loading,"Repeated rescans complete without hanging");
            for(int i=0;i<6&&r.ChannelName!="Rock";i++){r.Toggle();yield return Ready(r);}
            string removed=r.CurrentPath;File.Delete(removed);r.Next();yield return Ready(r);Check(r.CurrentPath!=removed&&r.Playing,"Removed file skipped while channel retains another playable track");
            Directory.Move(root+"/Rock",root+"/Jazz");r.Rescan();yield return Ready(r);Check(!r.ChannelNames.Contains("Rock")&&r.ChannelNames.Contains("Jazz"),"Rescan handles renamed channel without stale paths");
            flow.Save.Settings.music=.2f;flow.Save.SaveSettings();yield return null;var saved=new RacerSave(flow.Save.DirectoryPath,"unused");Check(saved.Settings.music==.2f&&saved.Settings.musicSource=="custom"&&saved.Settings.radioChannel==flow.Save.Settings.radioChannel,"Source, channel/Off and independent volume persist");
            var pad=InputSystem.AddDevice<Gamepad>();InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
            string before=r.ChannelName;InputSystem.QueueStateEvent(pad,new GamepadState().WithButton(GamepadButton.DpadDown));yield return null;yield return null;Check(r.ChannelName==before,"Menu D-pad Down does not change radio channel");InputSystem.QueueStateEvent(pad,new GamepadState());yield return null;
            flow.Race.opponents=flow.Race.traffic=false;flow.StartRace();while(flow.State!=RaceFlow.Stage.Racing)yield return null;flow.Race.vehicle.Body.isKinematic=true;
            before=r.ChannelName;InputSystem.QueueStateEvent(pad,new GamepadState().WithButton(GamepadButton.DpadDown));yield return null;yield return null;Check(r.ChannelName!=before,"Gameplay D-pad Down cycles channel");InputSystem.QueueStateEvent(pad,new GamepadState());InputSystem.RemoveDevice(pad);
            string one=root+"/Country";r.SetFolder(one);yield return Ready(r);if(r.ChannelName=="Off"){r.Toggle();yield return Ready(r);}Check(r.ChannelName=="General"&&r.Playing,"Single root-track channel plays");r.Toggle();yield return null;Check(r.ChannelName=="Off"&&!r.Playing&&!r.Loading,"Single channel cycles to Off and stops playback");r.Toggle();yield return Ready(r);Check(r.Playing,"Off cycles back to single channel");
            r.SetFolder(root+"/Empty");yield return Ready(r);r.Toggle();Check(r.ChannelName=="Off"&&!r.Playing&&!r.Loading,"Zero channels stays clearly Off");
        }
        IEnumerator Drive()
        {
            var race=FindAnyObjectByType<RaceDirector>();var flow=race.Flow;string vehicle=Arg("-vehicle","original");int laps=int.Parse(Arg("-laps","2"));
            flow.OpenGarage();flow.SelectVehicle(vehicle);flow.CloseGarage();race.opponents=true;race.traffic=true;race.laps=laps;race.opponentRoster=VehicleProfile.All.Where(p=>p.Id!=vehicle).Select(p=>p.Id).ToArray();race.Racers[0]=new RacerState("YOU automated",race.vehicle,race.gates.Length-1,laps);
            flow.StartRace();var pilot=race.vehicle.gameObject.AddComponent<RoadDriver>();pilot.Initialize(race,race.vehicle,true,1,1);pilot.Racer=race.Racers[0];
            if(mode=="performance")
            {
                // Hidden windows suspend presentation. Render explicitly into a fixed-size target
                // so these timings include scene rendering, unlike hidden-window update-only runs.
                performanceTarget=new RenderTexture(1280,720,24);renderCamera=Camera.main;renderCamera.targetTexture=performanceTarget;
                QualitySettings.vSyncCount=0;Application.targetFrameRate=120;
            }
            var frames=new List<float>();float start=Time.realtimeSinceStartup,next=0;bool recovered=false; int view=0;
            using var ambient=new StreamWriter(dir+"/ambient.csv");ambient.WriteLine("time,name,streetDistance,routeIsStreet,recoveries,x,y,z");
            using var log=new StreamWriter(dir+"/driving.csv");log.WriteLine("time,vehicle,lap,gate,speed,recoveries,misses,x,y,z");
            while(!race.ClassificationFinal&&Time.realtimeSinceStartup-start<900)
            {
                yield return null;if(flow.State!=RaceFlow.Stage.Racing)continue;Time.timeScale=float.Parse(Arg("-testSpeed","1"));frames.Add(Time.unscaledDeltaTime*1000);
                if(!recovered&&race.Progress.LapActive&&race.Progress.NextGate>=3){int gate=race.Progress.NextGate;double penalty=race.Progress.PenaltySeconds;race.vehicle.GetComponent<VehicleRespawn>().TryRecoverLocal(true);Check(race.Progress.NextGate==gate&&race.Progress.PenaltySeconds==penalty,"Local recovery preserves gate progress and penalties");recovered=true;}
                if(Time.time<next)continue;next=Time.time+1;
                if(view<8&&Time.time-start>view*7){CaptureUi(dir+"/gameplay-"+view+++".png");}
                foreach(var d in race.Drivers.Where(d=>d.GetComponent<AmbientVehicle>())){d.DriveRoad.Project(d.transform.position,out float lateral);var at=d.transform.position;ambient.WriteLine($"{race.Clock:F2},{d.name},{lateral:F3},{d.DriveRoad!=race.road||!race.Forest},{d.RecoveryCount},{at.x:F2},{at.y:F2},{at.z:F2}");}ambient.Flush();
                foreach(var state in race.Racers){var p=state.Car.Body.position;log.WriteLine($"{race.Clock:F3},{state.Car.GetComponent<VehicleConfiguration>().profileId},{state.Progress.CompletedLaps},{state.Progress.NextGate},{state.Car.ForwardSpeed:F2},{state.Recoveries},{state.Progress.MissedGates},{p.x:F2},{p.y:F2},{p.z:F2}");}log.Flush();
            }
            foreach(var state in race.Racers)Check(state.Progress.Finished&&!state.Dnf,$"{race.courseId} {state.Car.GetComponent<VehicleConfiguration>().profileId} completed {state.Progress.CompletedLaps}/{laps} laps; misses={state.Progress.MissedGates}; recoveries={state.Recoveries}; adjusted={state.Progress.AdjustedTime(race.Clock):F6}");
            frames.Sort();File.WriteAllText(dir+"/performance.txt",$"Automated physical pilot; explicit offscreen rendering={mode=="performance"}; timeScale={Arg("-testSpeed","1")}; frames={frames.Count}; median={frames[frames.Count/2]:F3}ms; p95={frames[(int)(frames.Count*.95)]:F3}ms; peak memory measured separately by host process monitor. Hidden-window update-only timings are NOT render/FPS measurements.");
            Check(flow.Boards.Board(race.Category,true).Count==1,"Completed player race creates one total entry");
            ScreenCapture.CaptureScreenshot(dir+"/results.png");yield return null;
        }
    }
}

