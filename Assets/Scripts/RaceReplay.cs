using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using UnityEngine;

namespace Racer
{
    // 1.1 (Dan, 2026-10-22: "I implemented a Replay system in Chastain Rush. Can we add this system to Woodstock Rush"), ported from Chastain Rush's
    // Round 57 F (RaceReplay.cs there). A replay is what ReplayRecorder sampled during one race: every racer's pose, speed, steering and pedal 25 times
    // a second, the traffic's poses and looks 10 times a second, and the world events the race caused (a BreakableProp broken, a rider's wave or
    // celebration). ReplayViewer plays it back. Files beside the records (RaceFlow.SaveRoot)/Replays/: best/<Top 10 board>.lap.wrreplay and
    // .race.wrreplay (the #1 lap and race of each Top 10 board; a new #1 replaces it) and saved/<course> <date time>.wrreplay (kept by the player).
    // A gzip'd binary: a header (enough for the REPLAYS page), then the samples. Each file marks the build and the course version (its courseId:
    // Leviathan's moves on when its gates change); an older one still plays, the page says the world may differ.
    public sealed class ReplayData
    {
        public const int Version = 1;
        public const string Extension = ".wrreplay";
        public string build = "", courseId = "", courseName = "", scene = "", category = "", date = "", label = "", mode = "", setup = "";
        public int course = -1, laps, timeOfDay, weather;
        public bool stunt, split;
        public string kind = "race";          // "race" (a #1 race), "lap" (a #1 lap: only its window plays), "saved", "last" (the race just run)
        public float t0, t1;                  // the window that plays (race clock seconds; the countdown is negative)
        public float bestTime;                // the #1's time (lap or race), 0 otherwise
        public int focus;                     // the racer the cameras start on (player 1)
        public readonly List<Racer> racers = new List<Racer>();
        public readonly List<Track> traffic = new List<Track>();
        public readonly List<Event> events = new List<Event>();

        // look: the traffic's body and paint (AmbientVehicle), body * 16 + paint; racers: wheels on the ground
        public struct Frame { public float t; public Vector3 p; public Quaternion q; public float speed, steer, throttle; public byte look; }
        public sealed class Track { public readonly List<Frame> frames = new List<Frame>(); }
        public sealed class Racer
        {
            public string name = "", vehicle = "original", rider = ""; public bool hasPaint, human; public Color paint = Color.white; public int scheme = -1, seat;
            public readonly Track track = new Track();
            public readonly List<(int lap, float start, float end)> laps = new List<(int, float, float)>();
            public float finishedAt = -1f, total = -1f;
        }
        public enum Kind : byte { Prop = 1, Gesture = 2 }
        public struct Event { public float t; public Kind kind; public int a, b; public Vector3 p, v; }

        public float Duration => t1 - t0;

        // a track's pose at time t (between samples: position lerped, rotation slerped; held at its ends; a jump of more than 30 m between two
        // samples, a traffic car recycled out of sight, is not drawn across the gap)
        public static Frame At(Track tr, float t)
        {
            var f = tr.frames; if (f.Count == 0) return default;
            if (t <= f[0].t) return f[0]; if (t >= f[f.Count - 1].t) return f[f.Count - 1];
            int lo = 0, hi = f.Count - 1; while (hi - lo > 1) { int mid = (lo + hi) >> 1; if (f[mid].t <= t) lo = mid; else hi = mid; }
            var a = f[lo]; var b = f[lo + 1]; float k = b.t > a.t ? Mathf.Clamp01((t - a.t) / (b.t - a.t)) : 1f;
            if ((b.p - a.p).sqrMagnitude > 900f) return k < 1f ? a : b;
            return new Frame { t = t, p = Vector3.Lerp(a.p, b.p, k), q = Quaternion.Slerp(a.q, b.q, k), speed = Mathf.Lerp(a.speed, b.speed, k), steer = Mathf.Lerp(a.steer, b.steer, k),
                               throttle = Mathf.Lerp(a.throttle, b.throttle, k), look = k < 0.5f ? a.look : b.look };
        }

        // ---------------------------------------------------------------- the file
        static readonly byte[] Magic = { (byte)'W', (byte)'R', (byte)'R', (byte)'P' };
        // written the safe way the other saves are (AtomicSave): a temporary file, then it replaces the old one
        public void Write(string path)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            string tmp = path + ".tmp";
            using (var fs = File.Create(tmp))
            using (var gz = new GZipStream(fs, System.IO.Compression.CompressionLevel.Optimal))
            using (var w = new BinaryWriter(gz))
            {
                w.Write(Magic); w.Write(Version);
                WriteHeader(w);
                w.Write(racers.Count);
                foreach (var r in racers)
                {
                    w.Write(r.name); w.Write(r.vehicle); w.Write(r.rider); w.Write(r.hasPaint); w.Write(r.paint.r); w.Write(r.paint.g); w.Write(r.paint.b); w.Write(r.scheme); w.Write(r.human); w.Write(r.seat);
                    w.Write(r.finishedAt); w.Write(r.total);
                    w.Write(r.laps.Count); foreach (var (lap, s, e) in r.laps) { w.Write(lap); w.Write(s); w.Write(e); }
                    WriteTrack(w, r.track, true);
                }
                w.Write(traffic.Count); foreach (var tr in traffic) WriteTrack(w, tr, false);
                w.Write(events.Count);
                foreach (var e in events) { w.Write(e.t); w.Write((byte)e.kind); w.Write(e.a); w.Write(e.b); W3(w, e.p); W3(w, e.v); }
            }
            for (int attempt = 0; ; attempt++)
            {
                try { if (File.Exists(path)) File.Replace(tmp, path, null); else File.Move(tmp, path); return; }
                catch (Exception ex) when ((ex is IOException || ex is UnauthorizedAccessException) && attempt < 4) { System.Threading.Thread.Sleep(10 << attempt); }
            }
        }
        void WriteHeader(BinaryWriter w)
        {
            w.Write(build); w.Write(courseId); w.Write(courseName); w.Write(scene); w.Write(category); w.Write(date); w.Write(label); w.Write(mode); w.Write(setup);
            w.Write(course); w.Write(laps); w.Write(timeOfDay); w.Write(weather); w.Write(stunt); w.Write(split); w.Write(kind); w.Write(t0); w.Write(t1); w.Write(bestTime); w.Write(focus);
        }
        void ReadHeader(BinaryReader r)
        {
            build = r.ReadString(); courseId = r.ReadString(); courseName = r.ReadString(); scene = r.ReadString(); category = r.ReadString(); date = r.ReadString(); label = r.ReadString(); mode = r.ReadString(); setup = r.ReadString();
            course = r.ReadInt32(); laps = r.ReadInt32(); timeOfDay = r.ReadInt32(); weather = r.ReadInt32(); stunt = r.ReadBoolean(); split = r.ReadBoolean(); kind = r.ReadString();
            t0 = r.ReadSingle(); t1 = r.ReadSingle(); bestTime = r.ReadSingle(); focus = r.ReadInt32();
        }
        static void W3(BinaryWriter w, Vector3 v) { w.Write(v.x); w.Write(v.y); w.Write(v.z); }
        static Vector3 R3(BinaryReader r) => new Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
        // a pose: position as floats, rotation as four shorts, speed in cm/s; racers also steering and pedal in a byte each; the look byte
        static void WriteTrack(BinaryWriter w, Track tr, bool full)
        {
            w.Write(tr.frames.Count);
            foreach (var f in tr.frames)
            {
                w.Write(f.t); W3(w, f.p); var q = f.q.normalized; w.Write((short)(q.x * 32767f)); w.Write((short)(q.y * 32767f)); w.Write((short)(q.z * 32767f)); w.Write((short)(q.w * 32767f));
                w.Write((short)Mathf.Clamp(f.speed * 100f, -32767f, 32767f));
                if (full) { w.Write((sbyte)Mathf.RoundToInt(Mathf.Clamp(f.steer, -1f, 1f) * 127f)); w.Write((byte)Mathf.RoundToInt(Mathf.Clamp01(f.throttle) * 255f)); }
                w.Write(f.look);
            }
        }
        static void ReadTrack(BinaryReader r, Track tr, bool full)
        {
            int n = r.ReadInt32();
            for (int i = 0; i < n; i++)
            {
                var f = new Frame { t = r.ReadSingle(), p = R3(r) };
                f.q = new Quaternion(r.ReadInt16() / 32767f, r.ReadInt16() / 32767f, r.ReadInt16() / 32767f, r.ReadInt16() / 32767f).normalized;
                f.speed = r.ReadInt16() / 100f;
                if (full) { f.steer = r.ReadSByte() / 127f; f.throttle = r.ReadByte() / 255f; }
                f.look = r.ReadByte();
                tr.frames.Add(f);
            }
        }
        public static ReplayData Read(string path, bool headerOnly = false)
        {
            using (var fs = File.OpenRead(path))
            using (var gz = new GZipStream(fs, CompressionMode.Decompress))
            using (var r = new BinaryReader(gz))
            {
                var m = r.ReadBytes(4); if (!m.SequenceEqual(Magic)) throw new InvalidDataException("not a replay");
                int ver = r.ReadInt32(); if (ver != Version) throw new InvalidDataException("replay version " + ver);
                var d = new ReplayData(); d.ReadHeader(r);
                if (headerOnly) return d;
                int nr = r.ReadInt32();
                for (int i = 0; i < nr; i++)
                {
                    var x = new Racer { name = r.ReadString(), vehicle = r.ReadString(), rider = r.ReadString(), hasPaint = r.ReadBoolean() };
                    x.paint = new Color(r.ReadSingle(), r.ReadSingle(), r.ReadSingle()); x.scheme = r.ReadInt32(); x.human = r.ReadBoolean(); x.seat = r.ReadInt32();
                    x.finishedAt = r.ReadSingle(); x.total = r.ReadSingle();
                    int nl = r.ReadInt32(); for (int k = 0; k < nl; k++) x.laps.Add((r.ReadInt32(), r.ReadSingle(), r.ReadSingle()));
                    ReadTrack(r, x.track, true); d.racers.Add(x);
                }
                int nt = r.ReadInt32(); for (int i = 0; i < nt; i++) { var tr = new Track(); ReadTrack(r, tr, false); d.traffic.Add(tr); }
                int ne = r.ReadInt32();
                for (int i = 0; i < ne; i++) d.events.Add(new Event { t = r.ReadSingle(), kind = (Kind)r.ReadByte(), a = r.ReadInt32(), b = r.ReadInt32(), p = R3(r), v = R3(r) });
                return d;
            }
        }

        // the same race trimmed to [a, b] (the #1 lap's replay: the lap with a little either side, every racer as they were then)
        public ReplayData Window(float a, float b, string asKind)
        {
            var c = Copy(asKind); c.t0 = Mathf.Max(t0, a); c.t1 = Mathf.Min(t1, b);
            const float pad = 1f;
            void Cut(Track src, Track o) => o.frames.AddRange(src.frames.Where(f => f.t >= c.t0 - pad && f.t <= c.t1 + pad));
            foreach (var r in racers)
            {
                var x = new Racer { name = r.name, vehicle = r.vehicle, rider = r.rider, hasPaint = r.hasPaint, paint = r.paint, scheme = r.scheme, human = r.human, seat = r.seat, finishedAt = r.finishedAt, total = r.total };
                x.laps.AddRange(r.laps); Cut(r.track, x.track); c.racers.Add(x);
            }
            foreach (var tr in traffic) { var o = new Track(); Cut(tr, o); c.traffic.Add(o); }
            c.events.AddRange(events);   // every event: those before the window set the world as it was when it opens
            return c;
        }
        ReplayData Copy(string asKind) => new ReplayData
        {
            build = build, courseId = courseId, courseName = courseName, scene = scene, category = category, date = date, label = label, mode = mode, setup = setup, course = course, laps = laps,
            timeOfDay = timeOfDay, weather = weather, stunt = stunt, split = split, kind = asKind, t0 = t0, t1 = t1, bestTime = bestTime, focus = focus
        };

        // ---------------------------------------------------------------- where they live
        public static string Root;   // RaceFlow.SaveRoot (the records' folder; a test save's own folder in checks)
        public static string Folder => Path.Combine(Root ?? Application.persistentDataPath, "Replays");
        public static string BestFolder => Path.Combine(Folder, "best");
        public static string SavedFolder => Path.Combine(Folder, "saved");
        public static string Safe(string s) { foreach (var ch in Path.GetInvalidFileNameChars()) s = s.Replace(ch, '_'); return s; }
        public static string BestPath(string board, bool race) => Path.Combine(BestFolder, Safe(board) + (race ? ".race" : ".lap") + Extension);

        // the files there, newest first (headers only; a file that cannot be read is skipped)
        public static List<(string path, ReplayData head)> List(bool best)
        {
            var list = new List<(string, ReplayData)>();
            var dir = best ? BestFolder : SavedFolder;
            if (!Directory.Exists(dir)) return list;
            foreach (var f in Directory.GetFiles(dir, "*" + Extension))
                try { list.Add((f, Read(f, true))); } catch (Exception e) { Debug.LogWarning($"[Replay] {Path.GetFileName(f)}: {e.Message}"); }
            return list.OrderByDescending(x => x.Item2.date).ToList();
        }
        // save any replay (the finish screen's SAVE REPLAY, the viewer's View / K): named by course, direction, date and time
        public static string LastSaved { get; private set; } = "";
        public static string Save(ReplayData d)
        {
            if (d == null) return "NOTHING TO SAVE";
            if (d.kind == "saved" || !string.IsNullOrEmpty(d.savedAs)) return "ALREADY SAVED";
            try
            {
                var s = d.Window(d.t0, d.t1, "saved"); s.label = $"{s.Title} {DateTime.Now:yyyy-MM-dd HH.mm.ss}";
                string path = Path.Combine(SavedFolder, Safe(s.label) + Extension);
                s.Write(path); LastSaved = path; d.savedAs = path; Debug.Log($"[Replay] saved {path} ({new FileInfo(path).Length / 1024} KB)");
                return "REPLAY SAVED";
            }
            catch (Exception ex) { Debug.LogWarning("[Replay] save failed: " + ex.Message); return "SAVE FAILED"; }
        }
        [NonSerialized] public string savedAs;
        // what the page says about a replay recorded on another build or on another version of its course
        public string Staleness
        {
            get
            {
                string now = CurrentCourseId(course);
                if (!string.IsNullOrEmpty(now) && now != courseId) return "the course has changed since: still playable, but the world may differ";
                if (build != Application.version) return $"recorded on {(string.IsNullOrEmpty(build) ? "an older version" : build)}: still playable, but the world may differ";
                return "";
            }
        }
        // the course's version where it can be known without loading it: Leviathan's ids are in its course files
        public static string CurrentCourseId(int course)
        {
            if (course < 0 || course >= RacePlaylists.Scenes.Length) return "";
            string key = RacePlaylists.Scenes[course];
            if (key != LeviathanCourse.Forward && key != LeviathanCourse.Reverse) return "";
            var a = Resources.Load<TextAsset>("Leviathan/" + (key == LeviathanCourse.Reverse ? "LeviathanReverse" : "LeviathanForward")); if (!a) return "";
            var m = System.Text.RegularExpressions.Regex.Match(a.text.Length > 400 ? a.text.Substring(0, 400) : a.text, "\"id\"\\s*:\\s*\"([^\"]+)\"");
            return m.Success ? m.Groups[1].Value : "";
        }
        public string Title => (course >= 0 && course < RacePlaylists.Titles.Length ? RacePlaylists.Titles[course] : courseName).Replace(" - ", " ") + (stunt ? " (Stunt Race)" : "");
        public string When => DateTime.TryParse(date, null, System.Globalization.DateTimeStyles.RoundtripKind, out var d) ? d.ToLocalTime().ToString("MMM d, yyyy h:mm tt") : date;
    }
}
