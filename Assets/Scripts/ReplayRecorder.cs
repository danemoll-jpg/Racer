using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Racer
{
    // 1.1 (ported from Chastain Rush's Round 57 F ReplayRecorder): records every race that ends on the finish screen (quick races, campaign events
    // and championships, Stunt Races, split screen; never Free Roam, police chases or the other Free Roam activities) from the countdown until the
    // results show, the race is left, or 30 minutes. Racers 25 times a second, traffic 10 times; the world events as they happen. Then it keeps the
    // replay of a new #1 Top 10 lap and race (ReplayData.BestPath; Stunt Races by their own boards, split screen too) and holds the race as Last for
    // the finish screen's WATCH / SAVE REPLAY. RaceFlow calls Begin / Keep / Revoke; SplitScreen calls Keep for both players.
    [DefaultExecutionOrder(200)]   // after RaceDirector's FixedUpdate (its gates and laps)
    public sealed class ReplayRecorder : MonoBehaviour
    {
        public static ReplayRecorder Instance { get; private set; }
        public const float RacerHz = 25f, TrafficHz = 10f, MaxSeconds = 1800f;
        RaceFlow flow; RaceDirector race;
        public ReplayData Current { get; private set; }   // being recorded
        public ReplayData Last { get; private set; }      // the last race's, once its recording stopped (the finish screen's WATCH / SAVE)
        public bool Recording => Current != null;
        readonly List<RacerState> who = new List<RacerState>();
        readonly List<RoadDriver> cars = new List<RoadDriver>();
        readonly List<RiderGestures.Kind> gestureWas = new List<RiderGestures.Kind>();
        readonly List<int> lapsSeen = new List<int>(); readonly List<float> lapStart = new List<float>(); readonly List<bool> lapWas = new List<bool>();
        readonly List<(string board, bool race, float a, float b, float time, int racer)> keep = new List<(string, bool, float, float, float, int)>();
        string setupRest = "";
        int step; float lastT = float.NegativeInfinity; double goAt = double.NaN;
        public static string Note = "";   // checks: what the last stop kept

        public void Initialize(RaceFlow owner) { flow = owner; race = owner.Race; Instance = this; }
        void OnEnable() => BreakableProp.BrokenByVehicle += OnProp;
        void OnDisable() => BreakableProp.BrokenByVehicle -= OnProp;
        void OnDestroy() { if (Recording) Stop("the scene closed"); if (Instance == this) Instance = null; }
        void OnApplicationQuit() { if (Recording) Stop("the game closed"); }

        float Now => flow.State == RaceFlow.Stage.Countdown || double.IsNaN(goAt) ? -flow.CountdownRemaining : (float)(Time.timeAsDouble - goAt);

        // RaceFlow.BeginCountdown (a race, not Free Roam): the racers are on the grid
        public void Begin()
        {
            if (Recording) Stop("a new race");
            int course = RacePlaylists.CourseOf(race.gameObject.scene); var look = WorldLook.Current;
            var campaign = CampaignRun.Active;
            var d = new ReplayData
            {
                build = Application.version, courseId = race.courseId, courseName = race.courseName, scene = LeviathanCourse.KeyOf(race.gameObject.scene), course = course,
                category = race.Category, date = DateTime.UtcNow.ToString("o"), laps = race.laps, stunt = race.stunts, split = SplitScreen.Active, kind = "last", t0 = -3.2f,
                timeOfDay = (int)(look ? look.RaceTime : (TimeOfDay)Mathf.Clamp(flow.Save.Settings.timeOfDay, 0, 3)), weather = (int)(look ? look.RaceWeather : (Weather)Mathf.Clamp(flow.Save.Settings.weather, 0, 2)),
                mode = campaign != null ? "Campaign: " + campaign.Name : SplitScreen.Active ? "Split screen" : race.stunts ? "Stunt Race" : RacePlaylists.Active != null ? "Playlist" : "Race"
            };
            who.Clear(); cars.Clear(); gestureWas.Clear(); lapsSeen.Clear(); lapStart.Clear(); lapWas.Clear(); keep.Clear();
            var p2 = SplitScreen.Race ? SplitScreen.Race.P2 : null;
            foreach (var r in race.Racers)
            {
                if (r == null || !r.Car) continue;
                var cfg = r.Car.GetComponent<VehicleConfiguration>();
                bool human = r == race.Racers[0] || (r == p2 && !SplitScreen.P2Ai);
                var x = new ReplayData.Racer { name = r.Name ?? "", vehicle = cfg ? cfg.profileId : "original", human = human, seat = r == race.Racers[0] ? 1 : r == p2 ? 2 : 0 };
                var rider = cfg && cfg.riderLook != null ? cfg.riderLook : RiderLook.Player; x.rider = rider != null ? JsonUtility.ToJson(rider) : "";
                if (cfg && cfg.Paint.HasValue) { x.hasPaint = true; x.paint = cfg.Paint.Value; }
                x.scheme = cfg ? cfg.Scheme : -1;
                d.racers.Add(x); who.Add(r); var g = r.Car.GetComponent<RiderGestures>(); gestureWas.Add(g ? g.Current : RiderGestures.Kind.None);
                lapsSeen.Add(0); lapStart.Add(0f); lapWas.Add(false);
            }
            foreach (var driver in race.Drivers) if (driver && driver.GetComponent<AmbientVehicle>()) { cars.Add(driver); d.traffic.Add(new ReplayData.Track()); }
            setupRest = $"{race.laps} lap{(race.laps == 1 ? "" : "s")} · {(race.Racers.Count > 1 ? $"{race.Racers.Count - 1} rival{(race.Racers.Count == 2 ? "" : "s")}, {race.DifficultyName}" : "solo")} · {(race.traffic ? "traffic" : "no traffic")}";
            d.setup = Setup(d, 0);
            Current = d; step = 0; lastT = float.NegativeInfinity; goAt = double.NaN; Last = null;
        }

        void Update()
        {
            // the results show (or the race was left): the recording ends there
            if (!Recording) return;
            var s = flow.State;
            if (s != RaceFlow.Stage.Countdown && s != RaceFlow.Stage.Racing && s != RaceFlow.Stage.Paused && s != RaceFlow.Stage.Settings) Stop(s == RaceFlow.Stage.Results ? "the results" : "the race was left");
        }

        void FixedUpdate()
        {
            var d = Current; if (d == null || ReplayViewer.Active) return;
            if (flow.State == RaceFlow.Stage.Racing && double.IsNaN(goAt)) goAt = Time.timeAsDouble;
            if (flow.State != RaceFlow.Stage.Countdown && flow.State != RaceFlow.Stage.Racing) return;
            float t = Now; if (t <= lastT) return; lastT = t;
            int racerEvery = Mathf.Max(1, Mathf.RoundToInt(1f / (RacerHz * Time.fixedDeltaTime))), trafficEvery = Mathf.Max(1, Mathf.RoundToInt(1f / (TrafficHz * Time.fixedDeltaTime)));
            for (int i = 0; i < who.Count; i++)
            {
                var r = who[i]; var x = d.racers[i]; var c = r.Car; if (!c) continue;
                // laps as the race counts them (a lap starts when its START is crossed; a reset to the grid starts it again)
                var p = r.Progress;
                if (p.LapActive && !lapWas[i]) lapStart[i] = t;
                lapWas[i] = p.LapActive;
                if (p.CompletedLaps > lapsSeen[i]) { lapsSeen[i] = p.CompletedLaps; x.laps.Add((p.CompletedLaps, lapStart[i], t)); lapStart[i] = t; }
                if (p.Finished && x.finishedAt < 0f) x.finishedAt = t;
                if (step % racerEvery == 0)
                {
                    x.track.frames.Add(new ReplayData.Frame { t = t, p = c.Body.position, q = c.Body.rotation, speed = c.ForwardSpeed, steer = c.VisualSteering, throttle = Pedal(c), look = (byte)c.GroundedWheels });
                    var g = c.GetComponent<RiderGestures>();
                    if (g && g.Current != gestureWas[i]) { if (g.Current != RiderGestures.Kind.None) d.events.Add(new ReplayData.Event { t = t, kind = ReplayData.Kind.Gesture, a = i, b = (int)g.Current }); gestureWas[i] = g.Current; }
                }
            }
            if (step % trafficEvery == 0)
                for (int i = 0; i < cars.Count; i++)
                {
                    var driver = cars[i]; if (!driver) continue; var car = driver.Car; if (!car) continue; var look = driver.GetComponent<AmbientVehicle>();
                    d.traffic[i].frames.Add(new ReplayData.Frame { t = t, p = car.Body.position, q = car.Body.rotation, speed = car.ForwardSpeed, look = (byte)(look ? look.BodyType * 16 + look.PaintIndex : 0) });
                }
            step++;
            d.t1 = t;
            if (t - d.t0 > MaxSeconds) Stop("30 minutes");
        }
        static float Pedal(ArcadeVehicle c)
        {
            var road = c.GetComponent<RoadDriver>(); if (road && road.enabled) return road.LastThrottle;
            var vi = c.GetComponent<VehicleInput>(); return vi && vi.enabled ? Mathf.Max(0f, vi.Throttle) : 0f;
        }
        void OnProp(BreakableProp prop, ArcadeVehicle car)
        {
            var d = Current; if (d == null || ReplayViewer.Active || (flow.State != RaceFlow.Stage.Countdown && flow.State != RaceFlow.Stage.Racing)) return;
            d.events.Add(new ReplayData.Event { t = Now, kind = ReplayData.Kind.Prop, p = prop.Home, v = car ? car.Body.linearVelocity : Vector3.zero });
        }

        // a lap or race went on a Top 10 board (RaceFlow.LapCompleted, SplitScreen.Record): a new #1 keeps its replay when the recording stops.
        // Called as the lap or race ends (the racer's lap is the one between its START crossing and now).
        public void Keep(RacerState r, bool raceRecord, int rank, string board, double time)
        {
            var d = Current; if (d == null || rank != 1 || string.IsNullOrEmpty(board)) return; int i = who.IndexOf(r); if (i < 0) return;
            float t = Now, start = lapStart[i], end = t; var laps = d.racers[i].laps;
            // already counted here (SplitScreen.Record runs after this FixedUpdate): that lap
            if (laps.Count > 0 && laps[laps.Count - 1].lap == r.Progress.CompletedLaps) { start = laps[laps.Count - 1].start; end = laps[laps.Count - 1].end; }
            if (raceRecord) keep.Add((board, true, d.t0, float.MaxValue, (float)time, i));   // the whole race, the others to their finish
            else keep.Add((RecordBoards.LapCategory(board), false, start - 2f, end + 2f, (float)time, i));
        }
        // debug movement took the attempt off the boards (RaceFlow.MarkDebugMovement): nothing of it is kept
        public void Revoke() => keep.Clear();

        string Setup(ReplayData d, int racer) => (racer >= 0 && racer < d.racers.Count ? (d.split ? d.racers[racer].name + " · " : "") + VehicleProfile.Find(d.racers[racer].vehicle).Name + " · " : "") + setupRest;
        public float Clock => Now;   // checks: the recording's clock now
        public string SaveLast() => Last == null ? "NOTHING TO SAVE" : ReplayData.Save(Last);

        public void Stop(string why)
        {
            var d = Current; if (d == null) return; Current = null;
            for (int i = 0; i < who.Count && i < d.racers.Count; i++) { var r = who[i]; if (r != null && r.Classified) d.racers[i].total = (float)r.ClassifiedTime(race.Clock); }
            Last = d; d.focus = 0;
            var notes = new List<string>();
            foreach (var (board, raceRecord, a, b, time, racer) in keep)
            {
                try
                {
                    var path = ReplayData.BestPath(board, raceRecord); var c = d.Window(a, Mathf.Min(b, d.t1), raceRecord ? "race" : "lap"); c.bestTime = time; c.category = board;
                    c.focus = racer; c.setup = Setup(c, racer); c.Write(path);   // the cameras start on the record's holder
                    notes.Add($"#1 {(raceRecord ? "race" : "lap")} kept {System.IO.Path.GetFileName(path)} ({new System.IO.FileInfo(path).Length / 1024} KB)");
                }
                catch (Exception ex) { notes.Add("keeping failed: " + ex.Message); }
            }
            keep.Clear();
            Note = $"stopped ({why}) at {d.t1:0.0} s; {d.racers.Count} racers, {d.racers.Sum(r => r.track.frames.Count)} racer frames, {d.traffic.Count} traffic, {d.traffic.Sum(r => r.frames.Count)} traffic frames, {d.events.Count} events" + (notes.Count > 0 ? "; " + string.Join("; ", notes) : "");
            Debug.Log("[Replay] " + Note);
        }
    }
}
