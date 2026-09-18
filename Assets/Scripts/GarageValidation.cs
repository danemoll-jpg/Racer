using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Racer
{
    // Explicit opt-in only; ordinary frame simulation and isolated saves.
    public sealed class GarageValidation : MonoBehaviour
    {
        public static string Output => Application.isEditor ? "Docs/CR026-027" : Path.Combine(Application.dataPath,"..","Validation");
        RaceDirector race;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            var args=System.Environment.GetCommandLineArgs();
            if((System.Array.IndexOf(args,"-racerGarageTest")>=0 || System.Array.IndexOf(args,"-racerGarageSample")>=0) && System.Array.IndexOf(args,"-racerTestSave")>=0) Launch();
        }
        public static void Launch() => new GameObject("Garage validation").AddComponent<GarageValidation>();
        IEnumerator Start()
        {
            yield return null;
            Directory.CreateDirectory(Output); Application.runInBackground=true;
            race=FindAnyObjectByType<RaceDirector>();
#if UNITY_EDITOR
            race.Flow.UseValidationSave(Path.GetFullPath("Temp/CR026-027-save"));
#endif
            bool sample=System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"-racerGarageSample")>=0;
            string report=sample?"sample.txt":"races.txt";
            File.WriteAllText(Path.Combine(Output,report),"Ordinary-frame RoadDriver autopilot; not human pace or physical controller testing.\n");
            for(int difficulty=sample?1:0;difficulty<(sample?2:3);difficulty++)
            {
                race.difficulty=difficulty; race.laps=1; race.opponents=true; race.traffic=true;
                race.Flow.StartRace();
                race.Racers[0]=new RacerState("YOU",race.vehicle,race.gates.Length-1,1);
                race.ResetSampling(race.vehicle.Body.position,Time.timeAsDouble);
                yield return null;
                ScreenCapture.CaptureScreenshot(Path.Combine(Output,$"grid-{difficulty}.png"));
                var camera=Camera.main;
                File.AppendAllText(Path.Combine(Output,report),$"Difficulty={race.DifficultyName}; grid viewport: "+string.Join("; ",race.Racers.Skip(1).Select(r=>r.Name+" "+camera.WorldToViewportPoint(r.Car.transform.position)))+"\n");
                var pilot=race.vehicle.gameObject.AddComponent<RoadDriver>();
                pilot.Initialize(race,race.vehicle,true,1,1); pilot.Racer=race.Racers[0];
                var frames=new List<float>(); float begin=Time.realtimeSinceStartup; float captureAt=12;
                while(!race.ClassificationFinal && Time.realtimeSinceStartup-begin<(sample?60:420))
                {
                    yield return null;
                    if(Time.realtimeSinceStartup-begin>captureAt) { ScreenCapture.CaptureScreenshot(Path.Combine(Output,$"interaction-{difficulty}-{captureAt:0}.png")); captureAt+=24; }
                    if(race.Flow.State==RaceFlow.Stage.Racing) frames.Add(Time.unscaledDeltaTime*1000);
                }
                frames.Sort();
                File.AppendAllText(Path.Combine(Output,report),$"{race.DifficultyName}: final={race.ClassificationFinal}; median={(frames.Count>0?frames[frames.Count/2]:0):F2}ms p95={(frames.Count>0?frames[(int)(frames.Count*.95f)]:0):F2}ms\n"+race.Standings()+"\n"+string.Join("\n",race.Racers.Select(r=>$"{r.Name} laps={r.Progress.CompletedLaps} recovery={r.Recoveries} misses={r.Progress.MissedGates}"))+"\n");
                pilot.enabled=false; Destroy(pilot); yield return null;
            }
            if(sample) race.Flow.Pause();
            File.AppendAllText(Path.Combine(Output,report),"DONE\n");
        }
    }
}
