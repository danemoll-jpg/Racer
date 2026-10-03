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
    // 0.71 look evidence (command-line opt-in only): "-lookBench <folder>" visits fixed viewpoints in every course scene
    // and the garage (isolated storage: launch with "-racerTestSave <folder>"), saves a screenshot of each, and measures frame rate at three of them. Run once with "-lookOff"
    // (the authored 0.70 rendering) and once without (the Clear Day look) for before/after pairs. Isolated save, muted,
    // vsync and frame cap off. Quits when done. Refuses to run without -racerTestSave so Dan's saves are never touched.
    public sealed class LookBench : MonoBehaviour
    {
        string outDir;
        // scene, route station of the view, camera height, measure frame rate, label
        static readonly (string scene, float s, float up, bool fps, string label)[] Views =
        {
            ("StreetLoopGreybox", 12, 3.2f, true, "street-loop-forward"),
            ("StreetLoopReverse", 12, 3.2f, false, "street-loop-reverse"),
            ("LakeWoods", 12, 3.2f, true, "forest-loop-forward"),
            ("LakeWoods", 209, 2.4f, false, "forest-loop-forward-cave"),
            ("ForestLoopReverse", 12, 3.2f, false, "forest-loop-reverse"),
            ("MountainLoop", 400, 3.6f, true, "mountain-loop-forward"),
            ("MountainLoop", 1740, 2.8f, false, "mountain-loop-forward-lower-route"),
            ("MountainLoopReverse", 400, 3.6f, false, "mountain-loop-reverse"),
            ("DansBackyardForward", 12, 3.2f, false, "backyard-forward"),
            ("DansBackyardReverse", 12, 3.2f, false, "backyard-reverse"),
        };

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            var a = Environment.GetCommandLineArgs(); int i = Array.IndexOf(a, "-lookBench");
            if (i < 0 || i + 1 >= a.Length || Array.IndexOf(a, "-racerTestSave") < 0 || FindAnyObjectByType<LookBench>()) return;
            var g = new GameObject("Look bench"); DontDestroyOnLoad(g); g.AddComponent<LookBench>().outDir = a[i + 1];
        }

        IEnumerator Start()
        {
            Directory.CreateDirectory(outDir);
            string tag = WorldLook.Disabled ? "before" : "after";
            var rows = new List<string> { $"look={tag} screen={Screen.width}x{Screen.height} gpu={SystemInfo.graphicsDeviceName} quality={QualitySettings.names[QualitySettings.GetQualityLevel()]}" };
            Screen.SetResolution(3840, 2160, FullScreenMode.FullScreenWindow); yield return null; yield return null;
            bool fpsOnly = Array.IndexOf(Environment.GetCommandLineArgs(), "-lookFpsOnly") >= 0;
            string variant = string.Join(" ", Environment.GetCommandLineArgs().Where(x => x.StartsWith("-lookNo") || x == "-lookOff"));
            rows[0] += $" variant=[{variant}]";
            foreach (var v in Views)
            {
                if (fpsOnly && !v.fps) continue;
                SceneManager.LoadScene(v.scene); yield return null; yield return null;
                var race = Prepare();
                race.road.Initialize();
                var p = race.road.At(v.s, out var f); f = Vector3.ProjectOnPlane(f, Vector3.up).normalized;
                var cam = Camera.main; foreach (var c in FindObjectsByType<ChaseCamera>(FindObjectsSortMode.None)) c.enabled = false;
                var eye = p - f * 9 + Vector3.up * v.up; cam.transform.SetPositionAndRotation(eye, Quaternion.LookRotation(p + f * 30 + Vector3.up * .5f - eye));
                yield return Hold(3);
                yield return Shot($"{v.label}-{tag}.png");
                if (v.fps)
                {
                    // Wall-clock frame times can be throttled by the desktop (a 30 Hz display, an occluded window), so the GPU
                    // and CPU time of each frame (FrameTimingManager; the test player is built with frame-timing stats) are
                    // recorded as well; the GPU time is the cost of the look.
                    var frames = new List<float>(); var gpu = new List<float>(); var cpu = new List<float>(); var ft = new FrameTiming[1]; float t0 = Time.unscaledTime;
                    while (Time.unscaledTime - t0 < 12)
                    {
                        yield return null; Keep(); frames.Add(Time.unscaledDeltaTime);
                        FrameTimingManager.CaptureFrameTimings(); if (FrameTimingManager.GetLatestTimings(1, ft) > 0) { if (ft[0].gpuFrameTime > 0) gpu.Add((float)ft[0].gpuFrameTime); if (ft[0].cpuMainThreadFrameTime > 0) cpu.Add((float)ft[0].cpuMainThreadFrameTime); }
                    }
                    frames.Sort(); gpu.Sort(); cpu.Sort(); float avg = frames.Count / frames.Sum(); float low = 1f / frames[(int)(frames.Count * .99f)];
                    float Med(List<float> l) => l.Count > 0 ? l[l.Count / 2] : float.NaN; float P95(List<float> l) => l.Count > 0 ? l[(int)(l.Count * .95f)] : float.NaN;
                    rows.Add($"FPS {v.label} {tag}: GPU median {Med(gpu):F2} ms (95th {P95(gpu):F2}) = {1000 / Med(gpu):F0} fps GPU-limited; CPU main median {Med(cpu):F2} ms; wall-clock average {avg:F1} fps, 1% low {low:F1} fps, median frame {frames[frames.Count / 2] * 1000:F2} ms ({gpu.Count} timed frames), screen {Screen.width}x{Screen.height}");
                }
                else rows.Add($"VIEW {v.label} {tag}: screen {Screen.width}x{Screen.height}");
                File.WriteAllLines(Path.Combine(outDir, $"bench-{tag}.txt"), rows);
            }
            if (fpsOnly) { File.WriteAllLines(Path.Combine(outDir, $"bench-{tag}{variant.Replace(" ", "")}.txt"), rows); Application.Quit(); yield break; }
            // Garage (Street Loop scene): the game's own garage camera and vehicle display.
            SceneManager.LoadScene("StreetLoopGreybox"); yield return null; yield return null;
            var r = Prepare(); r.Flow.OpenGarage(); yield return Hold(3); yield return Shot($"garage-{tag}.png", false); rows.Add($"VIEW garage {tag}: screen {Screen.width}x{Screen.height}");
            File.WriteAllLines(Path.Combine(outDir, $"bench-{tag}.txt"), rows);
            Application.Quit();
        }
        RaceDirector Prepare()
        {
            var race = FindAnyObjectByType<RaceDirector>();
            var title = FindAnyObjectByType<StartupTitle>(); if (title) Destroy(title.gameObject);
            typeof(StartupTitle).GetField("completed", BindingFlags.Static | BindingFlags.NonPublic)?.SetValue(null, true);
            race.Flow.EnterMenuAfterTitle(); Keep(); return race;
        }
        static void Keep() { AudioListener.volume = 0; QualitySettings.vSyncCount = 0; Application.targetFrameRate = -1; }
        static IEnumerator Hold(float seconds) { float t0 = Time.unscaledTime; while (Time.unscaledTime - t0 < seconds) { Keep(); yield return null; } }
        // World views: the game camera rendered into a 1920x1080 texture (post-processing included when it is on). Screen
        // capture is not used: it reads black from the back buffer when no post-processing pass runs (the -lookOff case).
        // Garage: the garage's own vehicle preview texture (what the garage shows), so vehicle colours can be compared.
        IEnumerator Shot(string name, bool world = true)
        {
            yield return new WaitForEndOfFrame();
            Texture2D tex;
            if (world)
            {
                var cam = Camera.main; var rt = new RenderTexture(1920, 1080, 24, RenderTextureFormat.ARGB32) { antiAliasing = 2 };
                var old = cam.targetTexture; cam.targetTexture = rt; cam.Render(); cam.targetTexture = old;
                RenderTexture.active = rt; tex = new Texture2D(1920, 1080, TextureFormat.RGB24, false); tex.ReadPixels(new Rect(0, 0, 1920, 1080), 0, 0); tex.Apply(); RenderTexture.active = null; rt.Release();
            }
            else
            {
                var preview = Camera.allCameras.Concat(FindObjectsByType<Camera>(FindObjectsSortMode.None)).FirstOrDefault(c => c.targetTexture != null && c.isActiveAndEnabled);
                if (!preview) yield break;
                var rt = preview.targetTexture; preview.Render(); RenderTexture.active = rt;
                tex = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false); tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0); tex.Apply(); RenderTexture.active = null;
            }
            File.WriteAllBytes(Path.Combine(outDir, name.Replace(".png", ".jpg")), tex.EncodeToJPG(90)); Destroy(tex);
        }
    }
}
