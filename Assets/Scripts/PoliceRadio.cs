using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Racer
{
    // 0.99 Part F: the police radio speaks the script (Docs/Audio/police-radio-script.md). Every radio line is a list of the script's IDs
    // (a whole line, or a phrase + a unit + a place or road); the on-screen text is built from the same IDs, so the words on screen are the
    // words in the script. Files are Dan's recordings (SourceArt/Audio/PoliceRadio/<id>.mp3|wav, copied into Resources/PoliceRadio by
    // PoliceRadioSync before a build); a take is R05a, R05b ... (one picked at random); a missing file falls back to the offline test voice
    // (Resources/PoliceRadioPlaceholder), and with neither nothing plays and the text still shows. The pieces of a line are trimmed,
    // joined, run through a radio effect (band-pass 300-3,400 Hz, light distortion, a click and a burst of static at the start and the end)
    // and played as one clip: one line at a time, queued, the music radio ducks a little while it speaks.
    public static class PoliceRadio
    {
        // what the screen shows for each ID (the script's wording; a place or road is what follows the phrase)
        static readonly Dictionary<string, string> Display = new()
        {
            ["R01"] = "Dispatch, unit in pursuit. Suspect on the road.", ["R02"] = "Visual on the suspect.", ["R03"] = "Suspect in custody.", ["R04"] = "We lost the suspect.",
            ["R05"] = "Time's up. The suspect is gone.", ["R06"] = "Air support inbound. Eyes on the suspect.", ["R07"] = "Air one, I have the suspect in the light.", ["R08"] = "Air one, lost them under the trees.",
            ["R09"] = "The suspect got through the roadblock.", ["R10"] = "Units covering the exits.", ["R11"] = "Suspect rammed a unit!", ["R12"] = "All units, all units, suspect still at large.",
            ["R13"] = "Backup requested.", ["R14"] = "Requesting roadblocks and more units.", ["R15"] = "Every unit responding.", ["R16"] = "Suspect is running cross-country.",
            ["R17"] = "Pull over! Pull over now!", ["R18"] = "Stop the vehicle!",
            ["P01"] = "Lost visual. Last seen near", ["P02"] = "Suspect off-road, heading toward", ["P03"] = "Suspect back on the road near", ["P04"] = "Units posted at the exits near", ["P05"] = "and",
            ["P06"] = "Roadblock set up on", ["P07"] = "ahead of the suspect", ["P08"] = "joining from", ["P09"] = "rejoining from", ["P10"] = "Speeder heading toward",
            ["U1"] = "Unit one,", ["U2"] = "Unit two,", ["U3"] = "Unit three,", ["U4"] = "Unit four,", ["U5"] = "Unit five,", ["U6"] = "Unit six,", ["U7"] = "Air one,",
            ["L01"] = "Moll's", ["L02"] = "the lake gateway", ["L03"] = "the ridge overlook", ["L04"] = "the Highway market", ["L05"] = "Trickum woods", ["L06"] = "the summit run-up", ["L07"] = "Roger's", ["L08"] = "McFadden's",
            ["L09"] = "Anderson's", ["L10"] = "the campsite", ["L11"] = "the storm drain", ["L12"] = "South Cherokee Lane", ["L13"] = "the Hwy 92 speed trap", ["L14"] = "the pool house", ["L15"] = "Kyle's house", ["L16"] = "the lake",
            ["D01"] = "Hwy 92", ["D02"] = "the Street Loop", ["D03"] = "the Summit road", ["D04"] = "the mountain trails", ["D05"] = "the lake road", ["D06"] = "a driveway", ["D07"] = "the forest trails", ["D08"] = "the back roads",
            ["D09"] = "the hills", ["D10"] = "Trickum Road",
            ["S01"] = "Dispatch to all units, speeders reported in the area.", ["S02"] = "Clocked him.", ["S03"] = "Speeder's running!", ["S04"] = "Suspect pulled over. Nice work.", ["S05"] = "Speeder got away.",
            ["S06"] = "Watch it, that was a civilian.", ["S07"] = "No violation, let them go.", ["S08"] = "Time's up, finish your chase.",
            ["H01"] = "Unit in position, watching the road.", ["H02"] = "Got a speeder! In pursuit.", ["H03"] = "Suspect's gone. Back to patrol.", ["H04"] = "Suspect in custody.",
        };
        // map destination titles (ExplorationMap) to the script's place IDs
        static readonly (string title, string id)[] Places =
        {
            ("Moll's", "L01"), ("Lake gateway", "L02"), ("Ridge overlook", "L03"), ("Highway market", "L04"), ("Trickum woods", "L05"), ("Summit Homeward Flight / run-up", "L06"), ("Roger's", "L07"),
            ("McFadden's", "L08"), ("Anderson's", "L09"), ("Campsite", "L10"), ("Storm drain tunnel", "L11"), ("S Cherokee speed trap", "L12"), ("Hwy 92 speed trap", "L13"), ("Pool-house jump", "L14"),
        };
        // Kyle's house is not a map destination: it is named when the point is within 90 m of it
        static readonly Vector3 KylesHouse = new(492.7f, 81.1f, -17.6f);

        public static string TextOf(params string[] ids)
        {
            var parts = new List<string>(); foreach (var id in ids) { if (Display.TryGetValue(id, out var t)) parts.Add(t); }
            if (parts.Count == 0) return ""; string s = string.Join(" ", parts).Replace(",  ", ", ").Trim();
            if (!(s.EndsWith(".") || s.EndsWith("!") || s.EndsWith("?"))) s += "."; return s;
        }
        public static string Unit(int n) => n >= 1 && n <= 6 ? "U" + n : "U" + Mathf.Clamp(n, 1, 6);
        // the nearest named place within 450 m of a point (the script's ID), else the road's ID
        public static string PlaceId(RoadNet net, RaceDirector race, Vector3 p)
        {
            if ((p - KylesHouse).sqrMagnitude < 90 * 90) return "L15";
            var map = race && race.Flow ? race.Flow.GetComponent<ExplorationMap>() : null; string best = null; float bd = 450;
            if (map != null && map.destinations != null) foreach (var d in map.destinations) { float dist = Vector3.Distance(d.position, p); if (dist < bd) { var hit = Array.Find(Places, x => x.title == d.title); if (hit.id != null) { bd = dist; best = hit.id; } } }
            if (best != null) return best; int n = net != null ? net.Nearest(p, out _, 300) : -1; return n >= 0 ? RoadId(net, n) : "D09";
        }
        public static string RoadId(RoadNet net, int node)
        {
            if (net == null || node < 0) return "D08"; string n = net.RoadNames[net.Road[node]].ToLowerInvariant(); var p = net.P[node];
            if (n.Contains("highway")) return "D01"; if (n.Contains("phase 3")) return p.x < -560 && p.x > -665 && p.z > -560 && p.z < 545 ? "D10" : "D02"; if (n.Contains("summit")) return "D03"; if (n.Contains("mountain")) return "D04";
            if (n.Contains("lake")) return "D05"; if (n.Contains("driveway")) return "D06"; if (n.Contains("navigation")) return "D07"; return "D08";
        }

        // ---- speaking ----
        public static void Speak(string[] ids, bool priority = false) { PoliceRadioVoice.Instance?.Enqueue(ids, priority); }
        public static void Clear() { PoliceRadioVoice.Instance?.Flush(); }
    }

    public sealed class PoliceRadioVoice : MonoBehaviour
    {
        const int Rate = 44100; const float Gap = .07f, Pad = .025f, Threshold = .012f;
        public static PoliceRadioVoice Instance { get; private set; }
        public static float Duck { get; private set; }
        public static bool Speaking => Instance && Instance.source && Instance.source.isPlaying;
        public static readonly List<string> Played = new(); // evidence: one entry per line spoken: time, the IDs, the length, whose recording each piece was
        public static AudioClip Current => Instance && Instance.source ? Instance.source.clip : null; string lastSources = "";
        struct Item { public string[] ids; public bool priority; public float at; }
        readonly Queue<Item> queue = new(); AudioSource source; bool currentPriority; float busyUntil; readonly Dictionary<string, List<AudioClip>> cache = new(); readonly Dictionary<string, float> lastSpoken = new();
        readonly System.Random rng = new(7);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot() { if (Instance) return; var g = new GameObject("Police radio voice"); DontDestroyOnLoad(g); g.AddComponent<PoliceRadioVoice>(); }
        void Awake() { Instance = this; source = gameObject.AddComponent<AudioSource>(); source.playOnAwake = false; source.spatialBlend = 0; source.ignoreListenerPause = false; }
        void OnDestroy() { if (Instance == this) Instance = null; Duck = 0; }

        public void Flush() { queue.Clear(); if (source) source.Stop(); busyUntil = 0; }
        public void Enqueue(string[] ids, bool priority)
        {
            if (ids == null || ids.Length == 0) return; string key = string.Join("+", ids);
            if (lastSpoken.TryGetValue(key, out var t) && Time.unscaledTime - t < 6) return; lastSpoken[key] = Time.unscaledTime;
            if (priority) { var keep = queue.Where(q => q.priority).ToList(); queue.Clear(); foreach (var k in keep) queue.Enqueue(k); if (source.isPlaying && !currentPriority) { source.Stop(); busyUntil = 0; } }
            else { while (queue.Count >= 3) queue.Dequeue(); }
            queue.Enqueue(new Item { ids = ids, priority = priority, at = Time.unscaledTime });
        }
        void Update()
        {
            float volume = Hints.Flow && Hints.Flow.Save != null && Hints.Flow.Save.Settings != null ? Mathf.Clamp01(Hints.Flow.Save.Settings.feedback) : 1f;
            float target = source.isPlaying ? 1 : 0; Duck = Mathf.MoveTowards(Duck, target, Time.unscaledDeltaTime * (target > Duck ? 6f : 1.5f));
            if (source.isPlaying || Time.unscaledTime < busyUntil || queue.Count == 0) return;
            var item = queue.Dequeue(); if (Time.unscaledTime - item.at > 8 && !item.priority) return; // too old to be worth saying
            var clip = Build(item.ids); if (!clip) return;
            Played.Add($"{Time.unscaledTime:F1}s {string.Join("+", item.ids)} {clip.length:F2}s [{lastSources}]"); currentPriority = item.priority; source.volume = volume * .9f; source.clip = clip; source.Play(); busyUntil = Time.unscaledTime + clip.length + .25f;
        }

        // ---- the clips ----
        List<AudioClip> Takes(string id)
        {
            if (cache.TryGetValue(id, out var list)) return list; list = new List<AudioClip>();
            foreach (var folder in new[] { "PoliceRadio/", "PoliceRadioPlaceholder/" })
            {
                foreach (var name in new[] { id, id + "a", id + "b", id + "c", id + "d" }) { var c = Resources.Load<AudioClip>(folder + name); if (c) list.Add(c); }
                if (list.Count > 0) break; // Dan's files always win: a placeholder is used only when his file for that ID is missing
            }
            cache[id] = list; return list;
        }
        float[] Samples(AudioClip clip)
        {
            if (clip.loadState != AudioDataLoadState.Loaded) clip.LoadAudioData();
            var raw = new float[clip.samples * clip.channels]; if (!clip.GetData(raw, 0)) return null;
            int ch = clip.channels; float[] mono = new float[clip.samples]; for (int i = 0; i < mono.Length; i++) { float s = 0; for (int c = 0; c < ch; c++) s += raw[i * ch + c]; mono[i] = s / ch; }
            if (clip.frequency != Rate) { int n = Mathf.RoundToInt(mono.Length * (float)Rate / clip.frequency); var r = new float[n]; for (int i = 0; i < n; i++) { float pos = i * (float)clip.frequency / Rate; int a = Mathf.Min(mono.Length - 1, (int)pos); int b = Mathf.Min(mono.Length - 1, a + 1); r[i] = Mathf.Lerp(mono[a], mono[b], pos - a); } mono = r; }
            // trim the silence at both ends, keep a little
            int first = 0, last = mono.Length - 1; while (first < last && Mathf.Abs(mono[first]) < Threshold) first++; while (last > first && Mathf.Abs(mono[last]) < Threshold) last--;
            first = Mathf.Max(0, first - (int)(Pad * Rate)); last = Mathf.Min(mono.Length - 1, last + (int)(Pad * Rate)); var trimmed = new float[Mathf.Max(0, last - first + 1)]; Array.Copy(mono, first, trimmed, 0, trimmed.Length); return trimmed;
        }
        AudioClip Build(string[] ids)
        {
            var parts = new List<float[]>(); int total = 0; var sources = new List<string>();
            foreach (var id in ids)
            {
                var takes = Takes(id); if (takes.Count == 0) { sources.Add(id + ":none"); continue; } sources.Add(id + (Resources.Load<AudioClip>("PoliceRadio/" + id) || Resources.Load<AudioClip>("PoliceRadio/" + id + "a") ? ":dan" : ":placeholder")); var s = Samples(takes[rng.Next(takes.Count)]); if (s == null || s.Length == 0) continue;
                parts.Add(s); total += s.Length + (int)(Gap * Rate);
            }
            if (parts.Count == 0) return null;
            var line = new float[total]; int at = 0; foreach (var p in parts) { Array.Copy(p, 0, line, at, p.Length); at += p.Length + (int)(Gap * Rate); }
            lastSources = string.Join(",", sources); var shaped = Radio(line); var clip = AudioClip.Create("Police radio " + string.Join("+", ids), shaped.Length, 1, Rate, false); clip.SetData(shaped, 0); return clip;
        }
        // the radio effect: a second-order high-pass at 300 Hz and low-pass at 3,400 Hz, a light tanh distortion, then a click and a short burst of static at each end
        float[] Radio(float[] x)
        {
            Biquad(x, 300, true); Biquad(x, 3400, false);
            float peak = 0; foreach (var v in x) peak = Mathf.Max(peak, Mathf.Abs(v)); float gain = peak > 1e-4f ? .8f / peak : 1; float drive = 1.7f, norm = (float)Math.Tanh(drive);
            for (int i = 0; i < x.Length; i++) x[i] = (float)Math.Tanh(drive * x[i] * gain) / norm * .85f;
            int lead = (int)(.16f * Rate), tail = (int)(.12f * Rate); var y = new float[x.Length + lead + tail]; Array.Copy(x, 0, y, lead, x.Length);
            Burst(y, 0, lead, .5f); Burst(y, lead + x.Length, tail, .4f); return y;
        }
        void Burst(float[] y, int start, int length, float click)
        {
            var noise = new float[length]; for (int i = 0; i < length; i++) noise[i] = (float)(rng.NextDouble() * 2 - 1); Biquad(noise, 300, true); Biquad(noise, 3600, false);
            for (int i = 0; i < length; i++)
            {
                float t = i / (float)length, env = Mathf.Clamp01(1 - t) * .22f; float ck = i < (int)(.004f * Rate) ? click * (1 - i / (.004f * Rate)) * (i % 2 == 0 ? 1 : -1) : 0;
                y[start + i] = Mathf.Clamp(y[start + i] + noise[i] * env * 2.4f + ck, -1, 1);
            }
        }
        static void Biquad(float[] x, float hz, bool high)
        {
            double w = 2 * Math.PI * hz / Rate, cos = Math.Cos(w), alpha = Math.Sin(w) / (2 * .7071), a0 = 1 + alpha, a1 = -2 * cos, a2 = 1 - alpha;
            double b0 = high ? (1 + cos) / 2 : (1 - cos) / 2, b1 = high ? -(1 + cos) : 1 - cos, b2 = b0; b0 /= a0; b1 /= a0; b2 /= a0; a1 /= a0; a2 /= a0; double x1 = 0, x2 = 0, y1 = 0, y2 = 0;
            for (int i = 0; i < x.Length; i++) { double v = x[i], o = b0 * v + b1 * x1 + b2 * x2 - a1 * y1 - a2 * y2; x2 = x1; x1 = v; y2 = y1; y1 = o; x[i] = (float)o; }
        }
    }
}
