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
    public sealed class Review6466Validation:MonoBehaviour
    {
        static string Arg(string key,string fallback){var a=Environment.GetCommandLineArgs();int i=Array.IndexOf(a,key);return i>=0&&i+1<a.Length?a[i+1]:fallback;}
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot(){var a=Environment.GetCommandLineArgs();if(!a.Contains("-review6466")||!a.Contains("-racerTestSave")||FindAnyObjectByType<Review6466Validation>())return;var g=new GameObject("CR064-066 diagnostics");DontDestroyOnLoad(g);g.AddComponent<Review6466Validation>();}
        RaceDirector race; RaceFlow flow;string root;readonly List<string> checks=new();
        void Check(bool pass,string label){checks.Add((pass?"PASS ":"FAIL ")+label);File.WriteAllLines(root+"/checks.txt",checks);}
        IEnumerator Start()
        {
            Application.runInBackground=true;root=Arg("-evidence","Docs/CR064-066/candidate");Directory.CreateDirectory(root);
            string scene=Arg("-course","StreetLoopGreybox");if(SceneManager.GetActiveScene().name!=scene)SceneManager.LoadScene(scene);yield return null;yield return null;
            race=FindAnyObjectByType<RaceDirector>();flow=race.Flow;QualitySettings.vSyncCount=0;Application.targetFrameRate=120;
            string mode=Arg("-review6466","rules");
            if(mode=="art")yield return Art();else if(mode=="radio")yield return Radio();else yield return Rules();
            File.WriteAllText(root+"/done.txt",$"{checks.Count(c=>c.StartsWith("PASS"))}/{checks.Count} passed");Application.Quit(checks.Any(c=>c.StartsWith("FAIL"))?2:0);
        }
        static void Complete(RaceProgress p,double start,double finish)
        {
            p.BeginTiming(start);p.Cross(0,true,start);
            for(int lap=0;lap<p.TargetLaps;lap++){for(int g=1;g<=p.CheckpointCount;g++)p.Cross(g,true,start+1);p.Cross(0,true,start+(finish-start)*(lap+1)/p.TargetLaps);}
        }
        IEnumerator Rules()
        {
            var human=new RacerState("future human",race.vehicle,3,3);human.Progress.BeginTiming(0);Check(!human.FinalizeEstimate(100,4)&&!human.Estimated,"Unfinished future human cannot be finalized");
            var ai=new RacerState("AI",race.vehicle,3,3,true);ai.Progress.BeginTiming(0);ai.Progress.Cross(0,true,1);ai.Progress.Miss(1,5);
            Check(ai.FinalizeEstimate(100,10)&&ai.EstimatedPhysicalTime==110&&ai.ClassifiedTime(100)==115,"Physical >= snapshot and already incurred penalty exactly once");
            Check(!ai.FinalizeEstimate(200,1)&&ai.ClassifiedTime(200)==115&&ai.Progress.CompletedLaps==0&&ai.Progress.LapTimes.Count==0,"Repeated finalization idempotent; no synthetic gates/laps");
            var dnf=new RacerState("DNF",race.vehicle,3,3,true){Dnf=true};Check(!dnf.FinalizeEstimate(100,1),"Existing DNF retained");
            var measured=new RacerState("measured",race.vehicle,3,1,true);Complete(measured.Progress,0,90);Check(!measured.FinalizeEstimate(100,1)&&measured.ClassifiedTime(100)==90,"Measured finish retained");
            var boards=new RecordBoards(root+"/boards");Check(boards.CompletedRace("estimated",race.Category,"original",ai.Progress,100)==0&&boards.CompletedLap("estimated",race.Category,"original",ai.Progress)==0,"Estimated state rejected by measured race/lap boards");
            foreach(var p in VehicleProfile.All)foreach(bool forest in new[]{false,true})foreach(int difficulty in new[]{0,1,2})
            {
                var estimate=new AiFinishEstimate();double fallback=estimate.Duration(100,3000,p,forest,difficulty);Check(fallback>30&&fallback<400,$"Finite course/vehicle/difficulty fallback {p.Id}/{forest}/{difficulty}");
                for(int i=0;i<15;i++)estimate.Sample(i,3000-i*20,p.Speed,false);
                var recent=estimate.Duration(14,1000,p,forest,difficulty);estimate.Sample(15,2720,p.Speed,false);
                Check(Math.Abs(recent-estimate.Duration(15,1000,p,forest,difficulty))<.001,"One crash sample does not dominate "+p.Id+forest+difficulty);
                Check(Math.Abs(estimate.Duration(25,3000,p,forest,difficulty)-fallback)<.001,"Stationary uses documented fallback "+p.Id+forest+difficulty);
            }
            flow.Save.Settings.estimateAiFinishes=true;flow.Save.SaveSettings();Check(new RacerSave(flow.Save.DirectoryPath,"legacy").Settings.estimateAiFinishes,"Option true persists");
            flow.Save.Settings.estimateAiFinishes=false;flow.Save.SaveSettings();Check(!new RacerSave(flow.Save.DirectoryPath,"legacy").Settings.estimateAiFinishes,"Option false persists");
            flow.OpenSettings();var option=FindObjectsByType<UnityEngine.UI.Button>().First(b=>b.GetComponentInChildren<UnityEngine.UI.Text>().text.StartsWith("Estimate AI at your finish"));option.onClick.Invoke();
            Check(new RacerSave(flow.Save.DirectoryPath,"legacy").Settings.estimateAiFinishes,"Settings button toggles and saves option");ThreeFeatureValidation.CaptureUi(root+"/settings.png");option.onClick.Invoke();flow.CloseSettings();
            race.opponents=true;race.traffic=false;flow.StartRace();while(flow.State!=RaceFlow.Stage.Racing)yield return null;
            yield return new WaitForSeconds(.25f);
            race.enabled=false;foreach(var driver in race.Drivers)driver.enabled=false;
            var one=race.Racers[1];var two=race.Racers[2];var three=race.Racers[3];
            foreach(var r in new[]{one,two,three}){r.Progress.BeginTiming(race.Clock);r.Progress.Cross(0,true,race.Clock);}
            float length=race.road.Length;float far=race.RemainingDistance(one);
            for(int lap=0;lap<race.laps-1;lap++){for(int g=1;g<=one.Progress.CheckpointCount;g++)one.Progress.Cross(g,true,race.Clock+1+lap*2);one.Progress.Cross(0,true,race.Clock+2+lap*2);}
            for(int g=1;g<=one.Progress.CheckpointCount;g++)one.Progress.Cross(g,true,race.Clock+10);one.RoadPosition=length-2;
            Check(Math.Abs(race.RemainingDistance(one)-2)<.1&&far>length*2,"Near finish and multiple laps remaining use route lengths");
            two.Progress.Miss(1,5);three.Dnf=true;float before=race.RemainingDistance(two);
            if(race.Branches.Length>0)
            {
                var branch=race.Branches[0];two.Branch.Begin(branch);two.Branch.Advance(branch.At(40,out _),branch.At(42,out var heading),heading);
                float expected=(race.laps-1)*length+branch.Length-two.Branch.Position+length-race.road.Relative(branch.exitRoad,race.Origin);
                Check(Math.Abs(race.RemainingDistance(two)-expected)<.1,"Shortcut uses remaining branch geometry plus road/laps");
                two.Branch.Recovered(branch.At(20,out _));Check(race.RemainingDistance(two)>=expected,"Local branch recovery never invents forward progress");
            }
            two.RecoveryStart=10;
            if(ShallowWater.Active.Count>0)
            {
                var previous=two.Car.Body.position;float roadPosition=two.RoadPosition;var branch=two.Branch.Route;two.Branch.Clear();
                two.Car.Body.position=ShallowWater.Active[0].transform.position;two.RoadPosition=race.road.Relative(race.road.Project(two.Car.Body.position,out _),race.Origin);
                double duration=two.Estimate.Duration(race.Clock,race.RemainingDistance(two),two.Car.GetComponent<VehicleConfiguration>().Profile,race.Forest,race.difficulty);
                Check(duration>0&&!double.IsInfinity(duration)&&two.Progress.PenaltySeconds==5,"Stationary water snapshot uses bounded route progress and fallback; penalties unchanged");
                two.Car.Body.position=previous;two.RoadPosition=roadPosition;if(branch)two.Branch.Begin(branch);
            }
            race.FinalizeUnfinishedAi();Check(!one.Estimated&&!two.Estimated,"Action before human finish is a no-op");
            Complete(race.Progress,race.Clock-100,race.Clock);double humanTime=race.Progress.AdjustedTime(race.Clock);var positions=race.Racers.Select(r=>r.Car.transform.position).ToArray();
            flow.LapCompleted();int humanRecords=flow.Boards.Board(race.Category,true).Count;
            var future=new RacerState("SECOND HUMAN",race.vehicle,race.gates.Length-1,race.laps);future.Progress.BeginTiming(race.Clock);race.Racers.Add(future);
            race.FinalizeUnfinishedAi();Check(!future.Estimated&&!future.Progress.Finished&&!race.ClassificationFinal&&flow.State==RaceFlow.Stage.Racing,"Unfinished second human remains racing; AI-only action cannot end their race");race.Racers.Remove(future);
            race.FinalizeUnfinishedAi();double adjusted=two.ClassifiedTime(race.Clock);race.FinalizeUnfinishedAi();
            Check(one.Estimated&&two.Estimated&&!three.Estimated&&three.Dnf&&race.ClassificationFinal,"Only unfinished AI projected; existing DNF retained");
            Check(two.ClassifiedTime(race.Clock)==adjusted&&Math.Abs(adjusted-two.EstimatedPhysicalTime-5)<.001,"Repeated action preserves one incurred penalty");
            Check(race.Progress.AdjustedTime(race.Clock)==humanTime&&!race.Racers[0].Estimated,"Human actual finish unchanged");
            Check(humanRecords==1&&flow.Boards.Board(race.Category,true).Count==humanRecords&&flow.Boards.Board(race.Category,true)[0].seconds==humanTime,"Actual human record retained; no AI projections submitted");
            Check(race.Racers.Select((r,i)=>r.Car.transform.position==positions[i]).All(x=>x)&&one.Car.Body.isKinematic&&!one.Car.Body.detectCollisions,"Finalization does not teleport and removes AI contact participation");
            Check(race.Standings().Contains("Estimated"),"Classification clearly labels estimates");yield return null;ThreeFeatureValidation.CaptureUi(root+"/results.png");
            var tieA=new RacerState("Tie A",race.vehicle,3,1,true);var tieB=new RacerState("Tie B",race.vehicle,3,1,true);tieA.Progress.BeginTiming(0);tieB.Progress.BeginTiming(0);tieA.FinalizeEstimate(100,10);tieB.FinalizeEstimate(100,10);race.Racers.Add(tieA);race.Racers.Add(tieB);
            Check(race.Ordered(true).IndexOf(tieA)<race.Ordered(true).IndexOf(tieB),"Equal adjusted estimates retain stable registration order");race.Racers.Remove(tieA);race.Racers.Remove(tieB);
            race.enabled=true;flow.StartRace();while(flow.State!=RaceFlow.Stage.Racing)yield return null;
            yield return new WaitForFixedUpdate();yield return null;Check(race.Racers.Skip(1).All(r=>!r.Estimated&&!r.Dnf&&!r.Car.Body.isKinematic&&r.Car.Body.detectCollisions)&&race.Drivers.All(d=>d.enabled),"Restart restores normal AI simulation and contacts");
            flow.Save.Settings.estimateAiFinishes=true;Complete(race.Progress,race.Clock-100,race.Clock);yield return new WaitForFixedUpdate();yield return null;
            Check(race.ClassificationFinal&&race.Racers.Skip(1).All(r=>r.Estimated),"Enabled option finalizes at human finish without waiting");
        }
        IEnumerator Art()
        {
            var config=race.vehicle.GetComponent<VehicleConfiguration>();var report=new List<string>();
            foreach(var profile in VehicleProfile.All)
            {
                flow.OpenGarage();flow.SelectVehicle(profile.Id);yield return null;
                var box=race.vehicle.GetComponent<BoxCollider>();var size=box.size;var center=box.center;var mass=race.vehicle.Body.mass;var com=race.vehicle.Body.centerOfMass;
                foreach(int paint in new[]{0,1,2,3,4,5,6})
                {
                    var unpaintedBefore=VehiclePaint.UnpaintedSnapshot(race.vehicle.transform);flow.SetColor(paint);yield return null;if(paint==0||paint==6)ThreeFeatureValidation.CaptureUi(root+"/garage-"+profile.Id+"-"+paint+".png");
                    var rs=race.vehicle.GetComponentsInChildren<Renderer>();
                    foreach(var r in rs){var block=new MaterialPropertyBlock();r.GetPropertyBlock(block);if(!block.isEmpty&&!VehiclePaint.IsBodyPaint(r.sharedMaterial))report.Add($"NONBODY {profile.Id} {r.name} {r.GetType().Name} material={r.sharedMaterial.name} base={block.GetColor("_BaseColor")} color={block.GetColor("_Color")} has={r.HasPropertyBlock()}");}
                    Check(rs.Where(r=>VehiclePaint.IsBodyPaint(r.sharedMaterial)).All(r=>{var b=new MaterialPropertyBlock();r.GetPropertyBlock(b);return b.GetColor("_BaseColor")==VehiclePaint.Colors[paint];}),"Body paint "+profile.Id+paint);
                    Check(VehiclePaint.UnpaintedUnchanged(race.vehicle.transform,unpaintedBefore),"Trim/rider unchanged "+profile.Id+paint);
                    var t=race.vehicle.transform;var at=t.position;
                    if(paint==0||paint==6)foreach(var view in new[]{("front",new Vector3(4,2.4f,6)),("side",new Vector3(6,1.9f,0)),("chase",new Vector3(2,2.5f,-6))})LivingWorldValidation.Capture(root+"/"+profile.Id+"-"+paint+"-"+view.Item1+".png",at+t.TransformDirection(view.Item2),at+Vector3.up*.35f);
                    if(paint==0||paint==6)LivingWorldValidation.Capture(root+"/face-"+profile.Id+"-"+paint+".png",at+t.TransformDirection(new Vector3(1.3f,1.25f,2.1f)),at+t.TransformDirection(new Vector3(-.2f,.8f,0)));
                    var active=rs.Where(r=>r.enabled).ToArray();report.Add($"{profile.Id} paint={paint}: activeRenderers={active.Length}; materials={active.Select(r=>r.sharedMaterial).Distinct().Count()}; triangles={active.OfType<MeshRenderer>().Sum(r=>r.GetComponent<MeshFilter>().sharedMesh.triangles.Length/3)}; colliders={race.vehicle.GetComponentsInChildren<Collider>().Length}; size={box.size}; center={box.center}; mass={mass}; com={com}");
                    Check(box.size==size&&box.center==center&&race.vehicle.Body.mass==mass&&race.vehicle.Body.centerOfMass==com,"Visual/color leaves physics invariant "+profile.Id+paint);
                    Check(race.vehicle.GetComponentsInChildren<Collider>().Length==1,"No visual colliders "+profile.Id+paint);
                }
                flow.CloseGarage();
                race.opponents=race.traffic=true;flow.StartRace();while(flow.State!=RaceFlow.Stage.Racing)yield return null;
                Check(race.Racers.All(r=>r.Car.GetComponentsInChildren<MeshFilter>().All(f=>f.sharedMesh&&f.sharedMesh.vertexCount>0)),"Player and opponent meshes survive clone retirement "+profile.Id);
                yield return new WaitForSeconds(.5f);var atRace=race.vehicle.transform.position;var transformRace=race.vehicle.transform;
                foreach(var view in new[]{("front",new Vector3(4,2.4f,6)),("side",new Vector3(6,1.9f,0)),("chase",new Vector3(2,2.5f,-6))})LivingWorldValidation.Capture(root+"/gameplay-"+profile.Id+"-"+view.Item1+".png",atRace+transformRace.TransformDirection(view.Item2),atRace+Vector3.up*.35f);
                flow.Pause();flow.QuitRace();
            }
            File.WriteAllLines(root+"/renderers.txt",report);
        }
        IEnumerator Radio()
        {
            Check(LocalRadio.FormatSong("The Blahs","BlahBlahBlah","x.wav")=="Artist: The Blahs\nSong: BlahBlahBlah","Tagged two-line exact labels");
            Check(LocalRadio.FormatSong("",null,"A file with spaces.wav")=="Artist: Unknown Artist\nSong: A file with spaces","Untagged filename and artist fallback");
            Check(LocalRadio.FormatSong("Björk <b>","夜の曲","a.ogg").Contains("Björk <b>\nSong: 夜の曲"),"Unicode and literal plain text preserved");
            string music=Path.GetFullPath(root+"/music");Directory.CreateDirectory(music);Wav(music+"/Root tune.wav");Directory.CreateDirectory(music+"/Classic Punk");Wav(music+"/Classic Punk/Tagged tune.wav");TagWave(music+"/Classic Punk/Tagged tune.wav");Directory.CreateDirectory(music+"/Été Radio");Wav(music+"/Été Radio/Unicode tune.wav");
            var radio=flow.Radio;radio.SetFolder(music);float end=Time.realtimeSinceStartup+15;while(radio.Scanning&&Time.realtimeSinceStartup<end)yield return null;if(radio.ChannelName=="Off")radio.Toggle();
            Check(radio.Toast==radio.ChannelName+" Radio","Folder-based station announcement");string announcement=radio.Toast;yield return new WaitForSecondsRealtime(.7f);Check(radio.Toast==announcement,"Async load and metadata do not erase station announcement");
            yield return new WaitForSecondsRealtime(2);Check(radio.Song.StartsWith("Artist: Unknown Artist\nSong: ")&&!radio.Song.Contains(" / "),"Loaded song has no station prefix");
            flow.StartRace();while(flow.State!=RaceFlow.Stage.Racing)yield return null;radio.ShowSong();yield return null;ThreeFeatureValidation.CaptureUi(root+"/song.png");
            for(int i=0;i<6;i++){radio.Toggle();Check(radio.Toast==(radio.ChannelName=="Off"?"Radio Off":radio.ChannelName+" Radio"),"Rapid channel switch "+i);yield return new WaitForSecondsRealtime(.08f);}
            while(radio.ChannelName!="Off")radio.Toggle();Check(radio.Toast=="Radio Off"&&!radio.Playing,"Off cancels previous channel");yield return new WaitForSecondsRealtime(.5f);Check(radio.Song=="Radio Off","Late metadata cannot replace Off");
            radio.Toggle();Check(radio.Toast=="General Radio","Virtual root channel announcement");ThreeFeatureValidation.CaptureUi(root+"/station.png");yield return new WaitForSecondsRealtime(3);radio.Next();yield return new WaitForSecondsRealtime(.3f);Check(radio.Toast!=null&&radio.Toast.StartsWith("Artist:"),"Track change only shows song info");
            var source=(AudioSource)typeof(LocalRadio).GetField("source",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(radio);source.time=source.clip.length-.08f;yield return new WaitForSecondsRealtime(2);Check(radio.Playing&&radio.Toast!=null&&radio.Toast.StartsWith("Artist:"),"Natural next track preserves song trigger");
            yield return new WaitForSecondsRealtime(5.5f);Check(radio.Toast==null,"No periodic popup frequency increase");radio.ShowSong();Check(radio.Toast.StartsWith("Artist:"),"On-demand info retains two lines");
            radio.Toggle();yield return new WaitForSecondsRealtime(.7f);Check(radio.Toast=="Classic Punk Radio","Tagged metadata completion retains station announcement");yield return new WaitForSecondsRealtime(2);
            Check(radio.Song=="Artist: The Blahs\nSong: BlahBlahBlah","Actual tagged WAV metadata decoded by ATL");ThreeFeatureValidation.CaptureUi(root+"/tagged-song.png");
            var longSong=LocalRadio.FormatSong("Björk "+new string('A',145),"Long title "+new string('Z',150),"x.wav");typeof(LocalRadio).GetField("song",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(radio,longSong);radio.ShowSong();yield return null;
            Check(longSong.Split('\n').Length==2&&longSong.Contains('…'),"Long metadata keeps two logical labels with bounded truncation");ThreeFeatureValidation.CaptureUi(root+"/long-song.png");
            string empty=Path.GetFullPath(root+"/empty");Directory.CreateDirectory(empty);radio.SetFolder(empty);while(radio.Scanning)yield return null;Check(radio.Count==0&&radio.Song=="Radio Off","Empty library remains safe");
            File.WriteAllText(empty+"/Broken.wav","not a wave file despite its extension");radio.Rescan();while(radio.Scanning)yield return null;radio.Toggle();yield return new WaitForSecondsRealtime(2);Check(!radio.Playing&&radio.ChannelName=="Off","Failed track safely skips to Off");
        }
        static void Wav(string path)
        {
            using var w=new BinaryWriter(File.Create(path));int n=44100*12;w.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));w.Write(36+n*2);w.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));w.Write(16);w.Write((short)1);w.Write((short)1);w.Write(44100);w.Write(88200);w.Write((short)2);w.Write((short)16);w.Write(System.Text.Encoding.ASCII.GetBytes("data"));w.Write(n*2);for(int i=0;i<n;i++)w.Write((short)(Math.Sin(i*2*Math.PI*220/44100)*1000));
        }
        static void TagWave(string path)
        {
            using var data=new MemoryStream();using(var tags=new BinaryWriter(data,System.Text.Encoding.ASCII,true))
            {
                tags.Write(System.Text.Encoding.ASCII.GetBytes("INFO"));
                foreach(var tag in new[]{("IART","The Blahs"),("INAM","BlahBlahBlah")}){var bytes=System.Text.Encoding.ASCII.GetBytes(tag.Item2+"\0");tags.Write(System.Text.Encoding.ASCII.GetBytes(tag.Item1));tags.Write(bytes.Length);tags.Write(bytes);if(bytes.Length%2!=0)tags.Write((byte)0);}
            }
            using var stream=File.Open(path,FileMode.Open,FileAccess.ReadWrite);using var writer=new BinaryWriter(stream);stream.Position=stream.Length;writer.Write(System.Text.Encoding.ASCII.GetBytes("LIST"));writer.Write((int)data.Length);writer.Write(data.ToArray());stream.Position=4;writer.Write((int)stream.Length-8);
        }
    }
}
