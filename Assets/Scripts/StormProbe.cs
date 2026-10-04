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
    // 0.76 Part C evidence (command-line opt-in only): "-stormProbe <folder>" with "-racerTestSave <folder>" (isolated
    // storage). "-stormCases roam-day,roam-night,race-day" (default all three). Each case plays two minutes of Rain as Dan
    // does: the motorcycle driven at cruising speed by the AI pilot (engine sound follows its throttle), radio on, default
    // volumes, listener at -60 dB (output RMS needs a live mix; 0 virtualises every voice). Logged: strikes and when the first came, bolts (how long lit, whether in front of the
    // camera), flash peaks, thunder plays and their level against the engine (per-source output RMS), voices culled, time
    // under cover. Results: <folder>/storm-<case>.txt. Quits when done.
    public sealed class StormProbe : MonoBehaviour
    {
        string outDir;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            var a = Environment.GetCommandLineArgs(); int i = Array.IndexOf(a, "-stormProbe");
            if (i < 0 || i + 1 >= a.Length || Array.IndexOf(a, "-racerTestSave") < 0 || FindAnyObjectByType<StormProbe>()) return;
            var g = new GameObject("Storm probe"); DontDestroyOnLoad(g); g.AddComponent<StormProbe>().outDir = a[i + 1];
        }
        public static IEnumerator Run(string dir, string spec, float seconds) { var p = new GameObject("Storm probe").AddComponent<StormProbe>(); DontDestroyOnLoad(p.gameObject); p.outDir = dir; yield return p.Case(spec, seconds); Destroy(p.gameObject); }
        IEnumerator Start()
        {
            if (string.IsNullOrEmpty(outDir)) yield break;
            Application.runInBackground = true; Directory.CreateDirectory(outDir); var a = Environment.GetCommandLineArgs(); int i = Array.IndexOf(a, "-stormCases");
            var cases = i >= 0 && i + 1 < a.Length ? a[i + 1].Split(',') : new[] { "roam-day", "roam-night", "race-day" };
            foreach (var c in cases) yield return Case(c, 120);
            Application.Quit();
        }
        // Signal level of a voice: RMS of its clip around the play position (1024 samples) times its volume. Read from the
        // clip data, so it needs no audible output (with the listener near silent the mixer virtualises quiet voices and
        // GetOutputData returns nothing). Filters and pitch are not included.
        static readonly Dictionary<AudioClip, float[]> clipData = new();
        // hp: only the part above about 150 Hz (what ordinary laptop, monitor and TV speakers actually play).
        static float Rms(AudioSource s, float[] buf, bool hp = false)
        {
            if (!s || !s.isPlaying || !s.clip || s.volume <= 0) return 0; var c = s.clip;
            if (!clipData.TryGetValue(c, out var d)) { d = new float[c.samples * c.channels]; try { if (!c.GetData(d, 0)) d = null; } catch { d = null; } clipData[c] = d; }
            if (d == null || d.Length == 0) return 0; int ch = c.channels, at = Mathf.Clamp(s.timeSamples, 0, c.samples - 1); double sum = 0; int n = 0;
            float a = Mathf.Exp(-2 * Mathf.PI * 150f / c.frequency), y = 0, prev = 0;
            for (int i = -256; i < 1024; i++) { int k = (((at + i) % c.samples + c.samples) % c.samples) * ch; float v = d[k]; if (hp) { y = a * (y + v - prev); prev = v; v = y; } if (i < 0) continue; sum += v * v; n++; }
            return (float)Math.Sqrt(sum / n) * s.volume;
        }
        static float Db(float rms) => rms > 1e-6f ? 20 * Mathf.Log10(rms) : -120;
        IEnumerator Case(string spec, float seconds)
        {
            bool roam = spec.StartsWith("roam"); bool night = spec.EndsWith("night");
            AudioListener.volume = .001f;
            if (SceneManager.GetActiveScene().name != "StreetLoopGreybox") { SceneManager.LoadScene("StreetLoopGreybox"); yield return null; yield return null; }
            var race = FindAnyObjectByType<RaceDirector>(); var flow = race.Flow; yield return null;
            typeof(StartupTitle).GetField("completed", BindingFlags.Static | BindingFlags.NonPublic)?.SetValue(null, true);
            var title = FindAnyObjectByType<StartupTitle>(); if (title) Destroy(title.gameObject);
            if (flow.State == RaceFlow.Stage.Title) flow.EnterMenuAfterTitle();
            var s = flow.Save.Settings; s.radioOn = true; s.music = 1; s.ambience = 1; s.master = .001f; s.lightningFlashes = true; s.roamWeather = (int)Weather.Rain; s.weather = (int)Weather.Rain; s.timeOfDay = (int)(night ? TimeOfDay.Night : TimeOfDay.Day); s.vehicleId = "moto"; s.opponents = false; s.traffic = true; flow.Save.SaveSettings();
            if (roam) { flow.StartFreeRoam(); for (int k = 0; k < 6; k++) { AudioListener.volume = .001f; yield return null; } race = FindAnyObjectByType<RaceDirector>(); flow = race.Flow; for (int k = 0; k < 30 && flow.State != RaceFlow.Stage.Racing; k++) yield return null; WorldLook.Current?.SetClock(1, night ? 23 : 13); }
            else { flow.StartRace(); while (flow.State != RaceFlow.Stage.Racing) yield return null; }
            AudioListener.volume = .001f;
            var car = race.vehicle; var input = car.GetComponent<VehicleInput>(); input.enabled = false;
            var driver = car.gameObject.AddComponent<RoadDriver>();
            if (roam) { driver.Initialize(race, car, false, 1, 1f); var road = driver.DriveRoad; road.Initialize(); driver.Place(road.Project(car.Body.position, out _), road.TrafficLane(road.Project(car.Body.position, out _), 1, false)); }
            else { driver.Initialize(race, car, true, 1, 1f); driver.Racer = race.Racers[0]; }
            var throttle = typeof(VehicleInput).GetField("<Throttle>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
            var fx = FindAnyObjectByType<WeatherEffects>(); var bolts = FindAnyObjectByType<LightningBolts>();
            AudioSource Named(string n) => fx ? fx.GetComponentsInChildren<AudioSource>(true).FirstOrDefault(x => x.gameObject.name == n) : null;
            var thunder = Named("Thunder"); var rumble = Named("Distant thunder"); var rain = fx ? fx.GetComponent<AudioSource>() : null;
            var engine = car.GetComponents<AudioSource>(); var buf = new float[1024];
            int strikes0 = fx ? fx.Strikes : 0, thunder0 = fx ? fx.ThunderPlayed : 0, bolts0 = bolts ? bolts.Shown : 0;
            var rows = new List<string>(); float t0 = Time.time, firstStrike = -1, coveredTime = 0, flashPeak = 0; int lastStrikes = strikes0, lastThunder = thunder0, maxPlaying = 0, virtualFrames = 0, thunderFrames = 0;
            var engineDb = new List<float>(); var engineHp = new List<float>(); var hpPeaks = new List<(float t, float th, float en)>(); float hpPeak = -120, hpEngine = -120; var radioDb = new List<float>(); var speeds = new List<float>();
            var thunderPeaks = new List<(float t, float peakDb, float engineDb, float radioDb, bool near)>(); float peak = -120, peakEngine = -120, peakRadio = -120, thunderStart = -1;
            var boltOn = new List<float>(); float boltStart = -1; int inView = 0, boltsSeen = 0;
            var cover = typeof(WeatherEffects).GetField("covered", BindingFlags.Instance | BindingFlags.NonPublic);
            while (Time.time - t0 < seconds)
            {
                yield return null; AudioListener.volume = .001f; float now = Time.time - t0;
                throttle?.SetValue(input, Mathf.Clamp01(driver.LastThrottle));
                speeds.Add(car.Body.linearVelocity.magnitude);
                var radio = LocalRadio.Source;
                float e = 0, e2 = 0; foreach (var src in engine) { float r = Rms(src, buf), r2 = Rms(src, buf, true); e += r * r; e2 += r2 * r2; }
                float eHp = Db(Mathf.Sqrt(e2)); engineHp.Add(eHp);
                float eDb = Db(Mathf.Sqrt(e)), rDb = Db(Rms(radio, buf)); engineDb.Add(eDb); radioDb.Add(rDb);
                maxPlaying = Mathf.Max(maxPlaying, FindObjectsByType<AudioSource>(FindObjectsSortMode.None).Count(x => x.isPlaying));
                if (fx && fx.Strikes != lastStrikes) { lastStrikes = fx.Strikes; if (firstStrike < 0) firstStrike = now; }
                if (fx) flashPeak = Mathf.Max(flashPeak, fx.FlashLevel);
                if (cover != null && fx && (bool)cover.GetValue(fx)) coveredTime += Time.deltaTime;
                bool lit = bolts && bolts.GetComponentsInChildren<LineRenderer>().Any(l => l.enabled);
                if (lit && boltStart < 0) { boltStart = now; boltsSeen++; var cam = Camera.main; var dir = Vector3.ProjectOnPlane(bolts.LastBase - cam.transform.position, Vector3.up); if (Vector3.Angle(Vector3.ProjectOnPlane(cam.transform.forward, Vector3.up), dir) < 45) inView++; }
                if (!lit && boltStart >= 0) { boltOn.Add(now - boltStart); boltStart = -1; }
                if (fx && fx.ThunderPlayed != lastThunder) { if (thunderStart >= 0) { thunderPeaks.Add((thunderStart, peak, peakEngine, peakRadio, false)); hpPeaks.Add((thunderStart, hpPeak, hpEngine)); } hpPeak = -120; lastThunder = fx.ThunderPlayed; thunderStart = now; peak = -120; }
                if (thunderStart >= 0 && thunder && thunder.isPlaying) { thunderFrames++; if (thunder.isVirtual) virtualFrames++; float tDb = Db(Rms(thunder, buf)); if (tDb > peak) { peak = tDb; peakEngine = eDb; peakRadio = rDb; } float hDb = Db(Rms(thunder, buf, true)); if (hDb > hpPeak) { hpPeak = hDb; hpEngine = eHp; } }
                if (thunderStart >= 0 && now - thunderStart > 8) { thunderPeaks.Add((thunderStart, peak, peakEngine, peakRadio, false)); hpPeaks.Add((thunderStart, hpPeak, hpEngine)); thunderStart = -1; }
            }
            if (thunderStart >= 0) { thunderPeaks.Add((thunderStart, peak, peakEngine, peakRadio, false)); hpPeaks.Add((thunderStart, hpPeak, hpEngine)); }
            float Med(List<float> l) { var o = l.Where(x => x > -119).OrderBy(x => x).ToList(); return o.Count == 0 ? -120 : o[o.Count / 2]; }
            rows.Add($"case {spec} scene {SceneManager.GetActiveScene().name} mode {(race.FreeRoam ? "Free Roam" : "Race")} look {WorldLook.Current?.Preset?.name} rain {WorldLook.Current?.Preset?.rain:F2} hour {WorldLook.Current?.Hour:F1}");
            rows.Add($"seconds {seconds:F0} strikes {(fx ? fx.Strikes - strikes0 : 0)} first strike at {firstStrike:F1} s; thunder played {(fx ? fx.ThunderPlayed - thunder0 : 0)}; bolts {boltsSeen} ({inView} within 45 deg of the camera's view), lit for {(boltOn.Count > 0 ? boltOn.Average() : 0):F2} s on average (max {(boltOn.Count > 0 ? boltOn.Max() : 0):F2}); flash peak {flashPeak:F2}");
            rows.Add($"speed median {speeds.OrderBy(x => x).ElementAt(speeds.Count / 2) * 3.6f:F0} km/h; time under cover {coveredTime:F1} s; most voices playing at once {maxPlaying} (real voices {AudioSettings.GetConfiguration().numRealVoices}); thunder virtual (culled) in {virtualFrames}/{thunderFrames} frames");
            rows.Add($"engine RMS median {Med(engineDb):F1} dBFS; radio RMS median {Med(radioDb):F1} dBFS");
            rows.Add($"engine above 150 Hz median {Med(engineHp):F1} dBFS (radio is streamed; its level is measured offline from the bundled songs)");
            foreach (var p in hpPeaks) rows.Add($"  thunder at {p.t:F1} s above 150 Hz: peak {p.th:F1} dBFS (0.3 s window peak of 1024-sample RMS), engine {p.en:F1} -> over engine {p.th - p.en:+0.0;-0.0} dB");
            foreach (var p in thunderPeaks) rows.Add($"  thunder at {p.t:F1} s: peak {p.peakDb:F1} dBFS, engine {p.engineDb:F1}, radio {p.radioDb:F1} -> over engine {p.peakDb - p.engineDb:+0.0;-0.0} dB, over radio {p.peakDb - p.radioDb:+0.0;-0.0} dB");
            File.WriteAllLines(Path.Combine(outDir, "storm-" + spec + ".txt"), rows);
            Destroy(driver); input.enabled = true;
        }
    }
}
