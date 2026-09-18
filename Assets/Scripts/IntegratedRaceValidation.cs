using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Racer
{
    public sealed class IntegratedRaceValidation : MonoBehaviour
    {
        RaceDirector race;
        RoadDriver pilot;
        readonly List<float> frames = new();
        float maximum;
        string output;
        int run;
        float begin;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            var args = System.Environment.GetCommandLineArgs();
            if (System.Array.IndexOf(args, "-racerIntegratedTest") >= 0 && System.Array.IndexOf(args, "-racerTestSave") >= 0)
                new GameObject("Explicit isolated race validation").AddComponent<IntegratedRaceValidation>();
        }

        public static void Launch()
        {
            new GameObject("Integrated validation").AddComponent<IntegratedRaceValidation>();
        }

        IEnumerator Start()
        {
            yield return null;
            Application.runInBackground = true;
            race = FindAnyObjectByType<RaceDirector>();
            output = "Docs/CR022-025";
            if (!Application.isEditor)
                output = Path.Combine(Application.dataPath, "..", "Validation");
            Directory.CreateDirectory(output);
            File.WriteAllText(Path.Combine(output, "races.txt"), "Ordinary-frame physics autopilot; no physical controller.\n");
#if UNITY_EDITOR
  race.Flow.UseValidationSave(Path.GetFullPath("Temp/CR022-025-save"));
#endif
            int testLaps = 1, testRuns = 3;
            var args = System.Environment.GetCommandLineArgs();
            for (int i = 0; i + 1 < args.Length; i++)
            {
                if (args[i] == "-racerTestLaps")
                    int.TryParse(args[i + 1], out testLaps);
                if (args[i] == "-racerTestRuns")
                    int.TryParse(args[i + 1], out testRuns);
            }

            testLaps = Mathf.Clamp(testLaps, 1, 3);
            testRuns = Mathf.Clamp(testRuns, 1, 3);
            for (run = 0; run < testRuns; run++)
            {
                race.laps = testLaps; // A bounded complete race; default review gameplay remains three laps.
                if (pilot)
                    Destroy(pilot);
                race.opponents = run != 2;
                race.traffic = run != 2;
                race.Flow.StartRace();
                // The player state was authored for three laps, so use all racers' target consistently.
                race.Racers[0] = new RacerState("YOU", race.vehicle, race.gates.Length - 1, race.laps);
                race.ResetSampling(race.vehicle.Body.position, Time.timeAsDouble);
                pilot = race.vehicle.gameObject.AddComponent<RoadDriver>();
                pilot.Initialize(race, race.vehicle, true, 1, 1);
                pilot.Racer = race.Racers[0];
                maximum = 0;
                frames.Clear();
                begin = Time.realtimeSinceStartup;
                while (!race.ClassificationFinal && Time.realtimeSinceStartup - begin < 420 * testLaps)
                {
                    yield return null;
                    if (race.Flow.State == RaceFlow.Stage.Racing)
                    {
                        frames.Add(Time.unscaledDeltaTime * 1000);
                        maximum = Mathf.Max(maximum, race.vehicle.Body.linearVelocity.magnitude);
                    }
                }

                var sorted = frames.OrderBy(x => x).ToArray();
                string result = $"Run {run + 1}: AI={race.opponents} traffic={race.traffic} final={race.ClassificationFinal} maxSpeed={maximum:0.00}m/s frames={sorted.Length} median={(sorted.Length > 0 ? sorted[sorted.Length / 2] : 0):0.00}ms p95={(sorted.Length > 0 ? sorted[(int)(sorted.Length * .95f)] : 0):0.00}ms\n" + race.Standings() + "\n" + string.Join("\n", race.Racers.Select(r => $"{r.Name} laps={r.Progress.CompletedLaps} next={r.Progress.NextGate} recoveries={r.Recoveries} position={r.Car.transform.position}")) + "\n";
                File.AppendAllText(Path.Combine(output, "races.txt"), result);
                ScreenCapture.CaptureScreenshot(Path.Combine(output, $"race-{run + 1}.png"));
                yield return new WaitForSecondsRealtime(2);
            }

            if (pilot)
                Destroy(pilot);
            File.AppendAllText(Path.Combine(output, "races.txt"), "DONE\n");
        }
    }
}
