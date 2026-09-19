using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Racer
{
    public sealed class WoodlandRaceValidation : MonoBehaviour
    {
        public static int Difficulty=2;
        public static int Laps=3;
        public static bool Mistake, Traffic=true;
        RaceDirector race;
        RoadDriver pilot;
        readonly List<float> frames=new();
        public static void Launch()=>new GameObject("Revision full mixed race").AddComponent<WoodlandRaceValidation>();
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            var args=System.Environment.GetCommandLineArgs();
            if(System.Array.IndexOf(args,"-woodlandRace")<0 || System.Array.IndexOf(args,"-racerTestSave")<0) return;
            int li=System.Array.IndexOf(args,"-woodlandLaps"); if(li>=0&&li+1<args.Length)int.TryParse(args[li+1],out Laps);
            int index=System.Array.IndexOf(args,"-racerDifficulty"); if(index>=0 && index+1<args.Length) int.TryParse(args[index+1],out Difficulty);
            Mistake=System.Array.IndexOf(args,"-deliberateMistake")>=0; Traffic=System.Array.IndexOf(args,"-noTraffic")<0; Application.runInBackground=true; Launch();
        }
        IEnumerator Start()
        {
            yield return null; Application.runInBackground=true; race=FindAnyObjectByType<RaceDirector>();
#if UNITY_EDITOR
            race.Flow.UseValidationSave(Path.GetFullPath("Temp/woodland-race-"+Difficulty));
#endif
            string root="Docs/CR041-045/"+(Application.isEditor?"editor":"standalone")+"-race-"+Difficulty+(Mistake?"-mistake":"-clean")+(Traffic?"-traffic":"-clear");
            Directory.CreateDirectory(root);
            race.Flow.OpenGarage(); race.Flow.SelectVehicle("original"); race.Flow.CloseGarage();
            race.opponents=true; race.traffic=Traffic; race.difficulty=Mathf.Clamp(Difficulty,0,2); race.laps=Laps;
            race.opponentRoster=new[]{"tourer","moto","atv"};
            race.Racers[0]=new RacerState("YOU reference pilot",race.vehicle,race.gates.Length-1,Laps);
            race.Flow.StartRace();
            pilot=race.vehicle.gameObject.AddComponent<RoadDriver>(); pilot.Initialize(race,race.vehicle,true,1,1); pilot.Racer=race.Racers[0];
            yield return null; ScreenCapture.CaptureScreenshot(Path.GetFullPath(root+"/grid.png"));
            bool injected=false;
            var peak=new float[4]; var speedSum=new double[4]; var samples=new int[4];
            using(var log=new StreamWriter(root+"/pace.csv"))
            {
                log.WriteLine("time,name,profile,lap,nextGate,speed,target,throttle,brake,grounded,recoveries,misses,x,y,z,roadDistance");
                float start=Time.realtimeSinceStartup,nextLog=0;
                while(!race.ClassificationFinal && Time.realtimeSinceStartup-start<1250)
                {
                    yield return null;
                    if(race.Flow.State!=RaceFlow.Stage.Racing) continue;
                    if(Mistake&&!injected&&race.Progress.NextGate==8)
                    {
                        injected=true;pilot.enabled=false;float until=Time.time+3;
                        while(Time.time<until){race.vehicle.Simulate(0,1,0,Time.fixedDeltaTime);yield return new WaitForFixedUpdate();}
                        bool recovered=race.vehicle.GetComponent<VehicleRespawn>().TryRecoverLocal();
                        File.WriteAllText(root+"/mistake.txt",$"Three-second full-brake mistake at CP8; local recovery={recovered}; clock={race.Clock:F3}; player position={race.PlayerPosition}\n");
                        pilot.enabled=true;
                    }
                    frames.Add(Time.unscaledDeltaTime*1000);
                    for(int i=0;i<4;i++)
                    {
                        var r=race.Racers[i]; if(r.Progress.Finished || r.Dnf) continue;
                        float speed=r.Car.Body.linearVelocity.magnitude; peak[i]=Mathf.Max(peak[i],speed); speedSum[i]+=speed; samples[i]++;
                    }
                    if(Time.time<nextLog) continue; nextLog=Time.time+.25f;
                    foreach(var r in race.Racers)
                    {
                        var d=r.Car.GetComponent<RoadDriver>();
                        race.road.Project(r.Car.Body.position,out float distance); var pos=r.Car.Body.position;
                        log.WriteLine($"{race.Clock:F3},{r.Name},{r.Car.GetComponent<VehicleConfiguration>().profileId},{r.Progress.CompletedLaps},{r.Progress.NextGate},{r.Car.ForwardSpeed:F3},{d.TargetSpeed:F3},{d.LastThrottle:F3},{d.LastBrake:F3},{r.Car.GroundedWheels},{r.Recoveries},{r.Progress.MissedGates},{pos.x:F3},{pos.y:F3},{pos.z:F3},{distance:F3}");
                    }
                    log.Flush();
                }
            }
            frames.Sort(); File.WriteAllText(root+"/performance.txt",$"{Screen.width}x{Screen.height}; vsync={QualitySettings.vSyncCount}; cap={Application.targetFrameRate}; peakWorkingSetMiB={System.Diagnostics.Process.GetCurrentProcess().PeakWorkingSet64/1048576.0:F1}; traffic={race.trafficCount+race.highwayTrafficCount}; CPU={SystemInfo.processorType}; GPU={SystemInfo.graphicsDeviceName}"); var rows=new List<string>{$"Difficulty={race.DifficultyName}; laps={Laps}; traffic={Traffic}; local={race.trafficCount}; highway={race.highwayTrafficCount}; shared capabilities; ordinary-frame motor commands. Reference pilot is NOT a human-competition acceptance test.",$"Final={race.ClassificationFinal}; frames={frames.Count}; median={frames[frames.Count/2]:F2}ms; p95={frames[(int)(frames.Count*.95f)]:F2}ms"};
            for(int i=0;i<4;i++)
            {
                var r=race.Racers[i]; var d=r.Car.GetComponent<RoadDriver>();
                rows.Add($"{r.Name} {r.Car.GetComponent<VehicleConfiguration>().profileId}: finished={r.Progress.Finished}; DNF={r.Dnf}; laps={r.Progress.CompletedLaps}; lapTimes={string.Join("/",r.Progress.LapTimes.Select(t=>t.ToString("F3")))}; race={r.Progress.RaceTime(race.Clock):F3}; adjusted={r.Progress.AdjustedTime(race.Clock):F3}; peak={peak[i]:F2}m/s; mean={speedSum[i]/System.Math.Max(1,samples[i]):F2}m/s; brakeSeconds={d.BrakingSeconds:F2}; recoveries={r.Recoveries}; misses={r.Progress.MissedGates}");
            }
            File.WriteAllLines(root+"/results.txt",rows); ScreenCapture.CaptureScreenshot(Path.GetFullPath(root+"/results.png"));
            if(pilot) { pilot.enabled=false; Destroy(pilot); }
            race.Flow.Pause();
            if(!Application.isEditor) Application.Quit();
        }
    }
}
