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
    // Explicit standalone diagnostics only, using the ordinary vehicle motor and race sampler.
    public sealed class ReverseCourseValidation:MonoBehaviour
    {
        static string Arg(string key,string fallback){var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,key);return i>=0&&i+1<args.Length?args[i+1]:fallback;}
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot(){var args=Environment.GetCommandLineArgs();if(!args.Contains("-reverseReview")||!args.Contains("-racerTestSave")||FindAnyObjectByType<ReverseCourseValidation>())return;var go=new GameObject("Reverse course diagnostics");DontDestroyOnLoad(go);go.AddComponent<ReverseCourseValidation>();}
        RaceDirector race;RaceFlow flow;ArcadeVehicle car;string root;readonly List<string> checks=new();
        readonly List<string> rows=new(){"course,vehicle,route,mode,repeat,seconds,recoveries,misses,maxLateral,minUpright,finished"};
        void Check(bool pass,string label){checks.Add((pass?"PASS ":"FAIL ")+label);File.WriteAllLines(root+"/checks.txt",checks);}
        IEnumerator Start()
        {
            Application.runInBackground=true;root=Arg("-evidence","Docs/CR070-074/routes");Directory.CreateDirectory(root);string scene=Arg("-course","StreetLoopReverse");
            if(SceneManager.GetActiveScene().name!=scene)SceneManager.LoadScene(scene);yield return null;yield return null;
            race=FindAnyObjectByType<RaceDirector>();flow=race.Flow;car=race.vehicle;QualitySettings.vSyncCount=0;Application.targetFrameRate=120;
            if(Arg("-reverseReview","routes")=="rules")yield return Rules();else if(Arg("-reverseReview","routes")=="jumps")yield return Jumps();else yield return Routes();
            File.WriteAllText(root+"/done.txt",$"{checks.Count(c=>c.StartsWith("PASS"))}/{checks.Count}");Application.Quit(checks.Any(c=>c.StartsWith("FAIL"))?2:0);
        }
        IEnumerator Begin(string vehicle)
        {
            if(flow.State!=RaceFlow.Stage.Ready){flow.Pause();flow.QuitRace();}flow.OpenGarage();flow.SelectVehicle(vehicle);flow.CloseGarage();race.traffic=race.opponents=false;flow.StartRace();
            while(flow.State!=RaceFlow.Stage.Racing){Time.timeScale=float.Parse(Arg("-testSpeed","3"));yield return null;}
        }
        IEnumerator Rules()
        {
            Check(race.reverseCourse,"Distinct reverse course flag");Check(race.Branches.Length>=2,"At least two authored optional branches");
            Check(race.Forest?race.EligibleVehicles.All(p=>p.Small):race.EligibleVehicles.Length==4,"Vehicle eligibility");
            Check(race.ambientRoad&&race.ambientRoad!=race.road&&!race.ambientRoad.forestTrail,"Civilian route remains original street");
            Check(!FindObjectsByType<TextMesh>(FindObjectsInactive.Include).Any(t=>SceneryText.RetiredHairpin(t.text,t.transform.parent.position)),"Specific sign absent");
            var categories=new HashSet<string>();string initial=race.courseId;
            foreach(string id in new[]{"street-v10-hairpin","lake-v3-shallows","street-reverse-v1","forest-reverse-v1"}){race.courseId=id;categories.Add(race.Category);}race.courseId=initial;Check(categories.Count==4,"Course/direction record namespaces distinct");
            yield return Begin(race.Forest?"moto":"original");
            foreach(var b in race.Branches)
            {
                Check(b.entryRoad<b.exitRoad&&b.exitRoad<race.road.Length,b.title+" ordered entry/rejoin");
                bool accidental=false;for(float s=b.entryRoad-15;s<b.entryRoad+140;s+=.5f)foreach(float lane in new[]{-2.5f,0f,2.5f}){var a=race.road.At(s,out var f)+Vector3.Cross(Vector3.up,f).normalized*lane+Vector3.up*.5f;var next=race.road.At(s+.5f,out var nf)+Vector3.Cross(Vector3.up,nf).normalized*lane+Vector3.up*.5f;accidental|=b.Enter(a,next,nf);}Check(!accidental,b.title+" normal trail does not grant shortcut credit");
                var probe=b.At(b.entryInset+1,out var bf)+Vector3.up*.5f;Check(b.Enter(probe-bf*.5f,probe,bf),b.title+" separated entrance grants actual directional entry");
                Check(b.Length<b.exitRoad-b.entryRoad,b.title+" shorter physical route");
                Check(b.bypassedGates.All(i=>i>0&&i<race.gates.Length),b.title+" explicit bypass excludes finish");
                Check(race.gates.All(g=>{float s=race.road.Project(g.transform.position,out _);return Mathf.Abs(s-b.entryRoad)>=60&&Mathf.Abs(s-b.exitRoad)>=65;}),b.title+" required gates separated from mouths/rejoins");
                var p=new RaceProgress(race.gates.Length-1,2);p.BeginTiming(1);p.Cross(0,true,1);
                if(b.bypassedGates.Length>0){for(int g=1;g<b.bypassedGates[0];g++)p.Cross(g,true,1.5);foreach(int gate in b.bypassedGates)if(p.NextGate==gate)p.Cross(gate,true,2);}
                Check(p.PenaltySeconds==0,b.title+" explicit bypass is penalty-free");
            }
            var road=race.road;var samples=new List<string>{"station,groundError,collider"};float worst=0;
            for(float s=0;s<road.Length;s+=4)
            {
                var p=road.At(s,out _);if(Physics.Raycast(p+Vector3.up*10,Vector3.down,out var hit,30,1,QueryTriggerInteraction.Ignore))
                {float error=Mathf.Abs(hit.point.y-p.y);if(hit.collider.name!="Reverse supported roadworks transition")worst=Mathf.Max(worst,error);if(error>1.1f)samples.Add($"{s},{error},{hit.collider.name}");}
                else samples.Add($"{s},missing,none");
            }
            File.WriteAllLines(root+"/main-support.csv",samples);Check(worst<1.4f,"Main route matches supported ground; worst="+worst);
            yield return null;
        }
        IEnumerator Jumps()
        {
            DriverVariation.Disabled=true;
            var layout=FindAnyObjectByType<ForestLayout>();
            var starts=layout?layout.jumpStarts:new[]{race.road.Project(GameObject.Find("Reverse supported roadworks transition").GetComponent<Renderer>().bounds.center,out _)-35};
            var ends=layout?layout.jumpEnds:new[]{starts[0]+100};
            var metrics=new List<string>{"vehicle,jump,mode,seconds,airSeconds,peakHeight,minUpright,recoveries,landed,wrongWay"};
            foreach(var profile in race.EligibleVehicles)
            for(int j=0;j<starts.Length;j++)
            foreach(bool imperfect in new[]{false,true})
            {
                yield return Begin(profile.Id);foreach(var b in race.Branches)b.aiValidated=false;
                var driver=car.gameObject.AddComponent<RoadDriver>();driver.Initialize(race,car,true,1,1);driver.Racer=race.Racers[0];
                typeof(RoadDriver).GetField("lane",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(driver,0f);
                if(imperfect)driver.Initialize(race,car,true,1,.75f);
                driver.Place(starts[j]-65,imperfect?1.4f:0);
                float start=Time.time,air=0,peak=0,minUp=1;bool landed=false,wrong=false;int frame=0;
                while(Time.time-start<45)
                {
                    yield return new WaitForFixedUpdate();Time.timeScale=3;
                    float station=race.road.Project(car.Body.position,out _);
                    if(car.GroundedWheels<2)air+=Time.fixedDeltaTime;
                    peak=Mathf.Max(peak,car.Body.position.y-race.road.At(station,out _).y);minUp=Mathf.Min(minUp,car.transform.up.y);
                    wrong|=race.GetComponent<WrongWayGuidance>().Visible;
                    if(++frame%75==0&&!imperfect)ThreeFeatureValidation.CaptureUi(root+$"/{profile.Id}-jump{j}-{frame}.png");
                    if(station>ends[j]+25&&station<ends[j]+120&&car.GroundedWheels>=2){landed=true;break;}
                }
                metrics.Add($"{profile.Id},{j},{imperfect},{Time.time-start:F3},{air:F3},{peak:F3},{minUp:F3},{driver.RecoveryCount},{landed},{wrong}");File.WriteAllLines(root+"/jumps.csv",metrics);
                Check(landed&&driver.RecoveryCount==0&&minUp>.65f&&!wrong,$"{profile.Id} jump {j} imperfect={imperfect} supported landing; air={air:F2}; recovery={driver.RecoveryCount}");
                driver.enabled=false;Destroy(driver);yield return null;car.enabled=true;Time.timeScale=1;
            }
        }
        IEnumerator Routes()
        {
            DriverVariation.Disabled=true;var selected=Arg("-vehicle","all");
            foreach(var profile in race.EligibleVehicles.Where(p=>selected=="all"||p.Id==selected))
            foreach(var branch in race.Branches.OrderBy(b=>b.entryRoad))
            foreach(string mode in new[]{"normal","clean","imperfect","recovery"})
            for(int repeat=0;repeat<int.Parse(Arg("-repeats","3"));repeat++)
            {
                yield return Begin(profile.Id);foreach(var b in race.Branches)b.aiValidated=false;
                var driver=car.gameObject.AddComponent<RoadDriver>();driver.Initialize(race,car,true,1,1);driver.Racer=race.Racers[0];
                typeof(RoadDriver).GetField("lane",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(driver,0f);
                if(mode!="normal")typeof(RoadDriver).GetField("plannedBranch",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(driver,branch);
                float startS=branch.entryRoad-65,endS=branch.exitRoad+65;
                driver.Place(startS,mode=="imperfect"?3.0f:0);
                if(mode=="imperfect")car.Body.rotation*=Quaternion.Euler(0,repeat%2==0?12:-12,0);
                race.Progress.BeginTiming(race.Clock);race.Progress.Cross(0,true,race.Clock);
                float entryRelative=race.road.Relative(branch.entryRoad,race.Origin);
                for(int g=1;g<race.gates.Length;g++)if(race.road.Relative(race.road.Project(race.gates[g].transform.position,out _),race.Origin)<entryRelative-65)race.Progress.Cross(g,true,race.Clock+.01);
                race.ResetSampling(car.Body.position,race.Clock);float start=Time.time,minUp=1,maxLateral=0;bool finished=false,seenEntry=false;int frame=0;bool mistake=false;
                using(var trace=new StreamWriter(root+$"/{profile.Id}-{branch.title}-{mode}-{repeat}.csv"))
                {
                    trace.WriteLine("time,speed,x,y,z,grounded,branch,station,lateral,brake,steering");
                    while(Time.time-start<150)
                    {
                        yield return null;Time.timeScale=float.Parse(Arg("-testSpeed","2"));float station=race.road.Project(car.Body.position,out _);seenEntry|=race.Racers[0].Branch.Route==branch;
                        if(mode=="imperfect"&&!mistake&&seenEntry){mistake=true;driver.enabled=false;car.enabled=false;float until=Time.time+3f;while(Time.time<until){car.Simulate(0,1,repeat%2==0?.6f:-.6f,Time.fixedDeltaTime);yield return new WaitForFixedUpdate();}driver.enabled=true;}
                        if(mode=="recovery"&&!mistake&&seenEntry&&race.Racers[0].Branch.Position>branch.entryInset+35){mistake=true;int nextGate=race.Progress.NextGate;double penalty=race.Progress.PenaltySeconds;bool recovered=car.GetComponent<VehicleRespawn>().TryRecoverLocal();Check(recovered&&race.Progress.NextGate==nextGate&&race.Progress.PenaltySeconds==penalty&&race.Racers[0].Branch.Route==branch,profile.Id+" actual local recovery preserves bypass entitlement "+branch.title);}
                        float side;float along=branch.Project(car.Body.position,out side);if(mode!="normal"&&seenEntry&&along<branch.Length-10)maxLateral=Mathf.Max(maxLateral,side);
                        minUp=Mathf.Min(minUp,car.transform.up.y);
                        if(++frame%8==0){var p=car.Body.position;trace.WriteLine($"{Time.time-start:F3},{car.ForwardSpeed:F3},{p.x:F3},{p.y:F3},{p.z:F3},{car.GroundedWheels},{race.Racers[0].Branch.Route?.title},{station:F3},{side:F3},{driver.LastBrake:F3},{car.VisualSteering:F3}");trace.Flush();}
                        if(station>=endS&&station<endS+80){finished=true;break;}
                        if(repeat==0&&frame%240==0)ThreeFeatureValidation.CaptureUi(root+$"/{profile.Id}-{branch.title}-{mode}-{frame}.png");
                    }
                }
                rows.Add($"{race.courseName},{profile.Id},{branch.title},{mode},{repeat},{Time.time-start:F3},{driver.RecoveryCount},{race.Progress.MissedGates},{maxLateral:F3},{minUp:F3},{finished}");File.WriteAllLines(root+"/times.csv",rows);
                Check(finished,$"{profile.Id} {branch.title} {mode} #{repeat} completes in {Time.time-start:F2}s; recovery={driver.RecoveryCount}; misses={race.Progress.MissedGates}");
                if(mode=="recovery")Check(mistake&&finished&&race.Progress.MissedGates==0,profile.Id+" recovered branch finishes penalty-free "+branch.title);
                if(mode=="clean")Check(finished&&seenEntry&&race.Progress.MissedGates==0&&driver.RecoveryCount==0,profile.Id+" clean branch has actual entry, no penalty/recovery "+branch.title);
                driver.enabled=false;Destroy(driver);yield return null;car.enabled=true;Time.timeScale=1;
            }
        }
    }
}
