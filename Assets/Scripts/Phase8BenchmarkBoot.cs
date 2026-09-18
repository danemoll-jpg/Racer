#if UNITY_EDITOR || DEBUG
using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace Racer
{
    // Explicit development-player opt-in. Uses the production countdown and isolated storage.
    public sealed class Phase8BenchmarkBoot : MonoBehaviour
    {
        string plan, report;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            var args = Environment.GetCommandLineArgs();
            int i = Array.IndexOf(args, "-phase8Benchmark");
            if (i < 0 || i + 2 >= args.Length) return;
            var runner = new GameObject("Phase 8 benchmark bootstrap").AddComponent<Phase8BenchmarkBoot>();
            runner.plan = args[i + 1]; runner.report = args[i + 2];
        }
        IEnumerator Start()
        {
            yield return null;
            var flow = FindAnyObjectByType<RaceFlow>();
            flow.UseValidationSave(Path.Combine(Path.GetDirectoryName(Path.GetFullPath(report)), "benchmark-save"));
            QualitySettings.vSyncCount = 0; Application.targetFrameRate = -1;
            Application.runInBackground = true;
            flow.StartRace();
            while (flow.State == RaceFlow.Stage.Countdown) yield return null;
            WoodlandBenchmark.Begin(plan, report, true);
            Destroy(gameObject);
        }
    }
}
#endif
