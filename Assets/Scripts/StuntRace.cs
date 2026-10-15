using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Racer
{
    // 0.119 Part B: Stunt Races. The same courses as the normal races (Mountain Loop Forward / Reverse, Leviathan Forward / Reverse: routes, gates,
    // shortcuts and in-order rules unchanged), but the player and every rival on stunt vehicles (Clay Hopper, Skywhip 250, any later one), with air
    // steering and the stunt park's landing rules (0.107: a 1.5 s recovery window, a bail only on a real roof or side landing). Scored by finish time
    // with a trick bonus: each combo banked knocks SecondsPer500 off the race time for every 500 points (capped at CapPerLap a lap, so tricks alone
    // never win a race), shown live. Or Split score (0.119 Part C): ranked by finish and by stunt points, 10 / 7 / 5 / 3 / 2 / 1 for each. Rivals do
    // tricks too (RivalTricks). Its own records: the Top 10 boards under "stunt-<course id>" (RaceDirector.RecordCourse), ghosts likewise, and the
    // best combo per course (StuntRaceRecords). Stunt vehicles stay out of every normal race, the other campaign chapters and their records.
    public static class StuntRace
    {
        public const string RecordPrefix = "stunt-";
        public static readonly int[] Courses = { 4, 5, 8, 9 }; // RacePlaylists.Scenes: Mountain Loop Forward / Reverse, Leviathan Forward / Reverse
        public static bool Course(int course) => Array.IndexOf(Courses, course) >= 0;
        public static bool Scene(string scene) => scene == "MountainLoop" || scene == "MountainLoopReverse"; // Leviathan races in these two
        public static VehicleProfile[] All => VehicleProfile.All.Where(p => p.Stunt).ToArray();
        // the player's choice: the stunt vehicles earned
        public static VehicleProfile[] Vehicles => All.Where(p => !VehicleUnlocks.Locked(p)).ToArray();
        // Race > Race type is offered once a stunt vehicle is owned (the Clay Hopper is from the start, 0.107)
        public static bool Offered => Vehicles.Length > 0;
        public static string Eligible(string id) { var p = VehicleProfile.Find(id); return p.Id == id && p.Stunt && !VehicleUnlocks.Locked(p) ? id : VehicleProfile.StuntBuggy.Id; }
        // the rivals: the stunt vehicles in turn from a random one (never a Cup edition), whether or not the player has earned them
        public static string[] Roster(int count)
        {
            var pool = All.Where(p => !p.CupEdition).Select(p => p.Id).ToArray(); int at = UnityEngine.Random.Range(0, pool.Length);
            return Enumerable.Range(0, Mathf.Max(0, count)).Select(i => pool[(at + i) % pool.Length]).ToArray();
        }
        public const double SecondsPer500 = .5; public const float CapPerLap = 8;
        public static double Bonus(int points) => Math.Round(points / 500.0 * SecondsPer500, 1);
        public const string Rules = "Stunt vehicles only, for you and every rival. Steer in the air: flips, rolls, spins; land on your wheels.";
        public static string ScoringText(bool split) => split
            ? "Split score: ranked by finish and by stunt points; each pays 10 / 7 / 5 / 3 / 2 / 1, the total decides (ties: the better finish)."
            : $"Trick bonus: every combo landed takes {SecondsPer500:0.0} s off your race time for each 500 points (up to {CapPerLap:0} s a lap).";
    }

    // 0.119 Part B: a Stunt Race's scoring, on the race object while one runs. Every combo banked (the players' by ArcadeActivities, the rivals' by
    // RivalTricks) adds its points to that racer and, with the trick bonus, its seconds off (RaceProgress.AddStunt). The live lines for the HUDs, the
    // split-score table and the rivals' trick names over them.
    public sealed class StuntRaceScore : MonoBehaviour
    {
        public static StuntRaceScore Current { get; private set; }
        RaceDirector race;
        readonly List<(ArcadeActivities acts, RacerState racer, Action<int, bool> handler)> hooks = new();
        readonly Dictionary<RacerState, (string text, float until)> flash = new();
        readonly HashSet<RacerState> closed = new();
        public static void Prepare(RaceDirector race)
        {
            var s = race.GetComponent<StuntRaceScore>();
            if (!race.stunts || race.FreeRoam || !race.road) { if (s) Destroy(s); return; }
            if (!s) s = race.gameObject.AddComponent<StuntRaceScore>();
            s.Begin(race);
        }
        void Begin(RaceDirector r)
        {
            race = r; Current = this; Unhook(); flash.Clear(); closed.Clear();
            Hook(race.Flow ? race.Flow.Activities : null, race.Racers[0]);
            var split = SplitScreen.Race; if (split && split.P2 != null && split.P2Acts) Hook(split.P2Acts, split.P2);
        }
        void Hook(ArcadeActivities acts, RacerState racer)
        {
            if (!acts || racer == null) return;
            Action<int, bool> h = (points, _) => Bank(racer, points, acts.LastComboTricks, acts.LastComboLinks);
            acts.ComboBanked += h; hooks.Add((acts, racer, h));
        }
        void Unhook() { foreach (var h in hooks) if (h.acts) h.acts.ComboBanked -= h.handler; hooks.Clear(); }
        void OnDestroy() { Unhook(); if (Current == this) Current = null; }
        public double Cap => StuntRace.CapPerLap * Mathf.Max(1, race ? race.laps : 1);
        // a combo banked by a racer: its points, and (trick bonus) its seconds off up to the cap; nothing after that racer's race has ended
        public void Bank(RacerState r, int points, string tricks, int links)
        {
            if (!race || r == null || points <= 0 || closed.Contains(r) || !race.Racers.Contains(r)) return;
            double bonus = race.splitScore ? 0 : Math.Max(0, Math.Min(StuntRace.Bonus(points), Cap - r.Progress.BonusSeconds));
            r.Progress.AddStunt(points, bonus);
            flash[r] = (race.splitScore ? $"+{points:N0} STUNT PTS" : bonus > 0 ? $"−{bonus:0.0} s TRICK BONUS" : "TRICK BONUS FULL", Time.time + 3);
            // the best combo per course (quick Stunt Races only, the human players)
            bool human = r == race.Racers[0] || (SplitScreen.Race && r == SplitScreen.Race.P2 && !SplitScreen.P2Ai);
            if (human && CampaignRun.Active == null && race.Flow && !race.Flow.DebugMovementUsed)
            {
                var car = r.Car; string name = SplitScreen.Active ? SplitScreen.NameOf(r == race.Racers[0] ? 1 : 2) : PlayerNames.Player;
                StuntRaceRecords.Record(StuntRace.RecordPrefix + race.courseId, points, car.GetComponent<VehicleConfiguration>().profileId, name, (links > 1 ? "×" + links + " " : "") + tricks);
            }
        }
        // the end of a racer's race: its open combo counts now (before its time is recorded), then nothing more
        public void Close(RacerState r)
        {
            if (!race || r == null || closed.Contains(r)) return;
            foreach (var h in hooks) if (h.racer == r && h.acts) h.acts.EndStuntCombo();
            r.Car?.GetComponent<RivalTricks>()?.Close();
            closed.Add(r);
        }
        // ---------- split score (0.119 Part C) ----------
        public static int Points(int place) => place >= 1 && place <= CampaignData.CupPoints.Length ? CampaignData.CupPoints[place - 1] : 0;
        public sealed class Line { public RacerState racer; public int finish, stunt, total; }
        // every racer: finish place (the race's own final order; 0 = did not finish), stunt place (most stunt points; ties to the better finish), the
        // points for each and the total; ordered by the total (ties: the better finish)
        public List<Line> SplitTable(bool final)
        {
            var order = race.Ordered(final); var lines = order.Select((r, i) => new Line { racer = r, finish = r.Dnf ? 0 : i + 1 }).ToList();
            int Fin(Line l) => l.finish == 0 ? 99 : l.finish;
            var byStunt = lines.OrderByDescending(l => l.racer.Progress.StuntPoints).ThenBy(Fin).ToList();
            for (int i = 0; i < byStunt.Count; i++) byStunt[i].stunt = i + 1;
            foreach (var l in lines) l.total = Points(l.finish) + Points(l.stunt);
            return lines.OrderByDescending(l => l.total).ThenBy(Fin).ToList();
        }
        public int StuntPlace(RacerState r) { if (!race) return 0; var t = SplitTable(false); return t.FirstOrDefault(l => l.racer == r)?.stunt ?? 0; }
        // the HUD's line for a racer, and the moment's bonus / points ("−2.5 s TRICK BONUS")
        public string HudLine(RacerState r)
        {
            if (!race || r == null) return "";
            if (race.splitScore) { var l = SplitTable(false).FirstOrDefault(x => x.racer == r); return $"STUNT  {r.Progress.StuntPoints:N0} pts  ·  {Campaign.Ordinal(l?.stunt ?? 1)}"; }
            return $"TRICKS  −{r.Progress.BonusSeconds:0.0} s" + (r.Progress.BonusSeconds >= Cap - .05 ? "  (max)" : "");
        }
        public string Flash(RacerState r) => r != null && flash.TryGetValue(r, out var f) && Time.time < f.until ? f.text : null;
        // ---------- the rivals' tricks over them, near a player ----------
        GUIStyle style;
        void OnGUI()
        {
            if (!race || race.Flow == null || race.Flow.MenuVisible || TrailerMode.Active || Event.current.type != EventType.Repaint) return;
            var cams = SplitScreen.Views.Count > 0 ? SplitScreen.Views.ToList() : new List<Camera> { Camera.main };
            style ??= new GUIStyle(GUI.skin.label) { fontSize = Mathf.RoundToInt(Screen.height / 46f), fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            foreach (var r in race.Racers)
            {
                var t = r.Car ? r.Car.GetComponent<RivalTricks>() : null; if (!t || Time.time >= t.LineUntil || string.IsNullOrEmpty(t.LastLine)) continue;
                var at = r.Car.Body.position + Vector3.up * 2.4f;
                foreach (var cam in cams)
                {
                    if (!cam || Vector3.Distance(cam.transform.position, at) > 70) continue;
                    var v = cam.WorldToScreenPoint(at); if (v.z <= 0) continue;
                    var px = cam.pixelRect; if (v.x < px.xMin || v.x > px.xMax || v.y < px.yMin || v.y > px.yMax) continue;
                    var rect = new Rect(v.x - 160, Screen.height - v.y - 18, 320, 36);
                    style.normal.textColor = new Color(0, 0, 0, .85f); GUI.Label(new Rect(rect.x + 2, rect.y + 2, rect.width, rect.height), t.LastLine, style);
                    style.normal.textColor = t.LastBail ? new Color(1f, .5f, .4f) : new Color(1f, .84f, .3f); GUI.Label(rect, t.LastLine, style);
                }
            }
        }
    }

    // 0.119 Part B: a rival's tricks in a Stunt Race. On every flight long enough for one (the big jumps: the summit flights, Downhill Ridge Cut's
    // flight, the Street Loop jumps a course has), the rival picks a trick its airtime allows (a flip, a roll, a 360, or a combo on long flights) by
    // the race's difficulty and its own luck, turns through it with the player's air steering (ArcadeVehicle.AiAir: the same rates), stops it square
    // and lets the vehicle level itself for the landing. Scored like the player's (ArcadeActivities: the trick points, the jump's distance and
    // airtime, combos of flights within ComboGap on the ground). Now and then a bail (more on Easy, rare on Hard): it lands a little crossed up,
    // loses the trick and some speed (never a crash or a stuck reset). Its trick shows briefly over it near a player (StuntRaceScore).
    public sealed class RivalTricks : MonoBehaviour
    {
        RaceDirector race; ArcadeVehicle car; RacerState racer; System.Random rng; int slot;
        bool flying, planned, bail, abort; float air, ground, stable, touch = -1, since, landAt; Vector3 takeoff, landing, turned;
        readonly List<(int axis, float degrees)> plan = new(); int step; float stepBase; string planName = "";
        int links, comboPoints; readonly List<string> names = new();
        public int Tricks { get; private set; }
        public int Bails { get; private set; }
        public int Flights { get; private set; }
        public string LastLine { get; private set; } = ""; public float LineUntil { get; private set; } public bool LastBail { get; private set; }
        public string Log { get; private set; } = ""; // checks: each flight's plan and outcome
        // checks only (Report106PlayerCheck stunt119): the player's own vehicle driven by the road AI turns its tricks with this, and the player's
        // own trick scoring (ArcadeActivities) counts them; nothing is banked here. Never set in play.
        [System.NonSerialized] public bool SteerOnly;
        // by difficulty (Easy, Normal, Hard): the chance to try a trick on a flight that allows one, and to bail it
        static readonly float[] TryChance = { .6f, .82f, .95f }, BailChance = { .22f, .1f, .04f };
        sealed class Trick { public string name; public (int axis, float degrees)[] steps; public float seconds; }
        // seconds: the turn at the player's rates (about 1.5 s a 360, spin-up and stop included)
        static readonly Trick[] Menu =
        {
            new() { name = "BACKFLIP", steps = new[] { (0, -360f) }, seconds = 1.5f },
            new() { name = "ROLL", steps = new[] { (2, 360f) }, seconds = 1.5f },
            new() { name = "360 SPIN", steps = new[] { (1, 360f) }, seconds = 1.6f },
            new() { name = "FRONTFLIP", steps = new[] { (0, 360f) }, seconds = 1.5f },
            new() { name = "DOUBLE BACKFLIP", steps = new[] { (0, -720f) }, seconds = 2.7f },
            new() { name = "BACKFLIP + 360 SPIN", steps = new[] { (0, -360f), (1, 360f) }, seconds = 3.2f },
            new() { name = "ROLL + 360 SPIN", steps = new[] { (2, 360f), (1, 360f) }, seconds = 3.2f },
            new() { name = "DOUBLE BACKFLIP + 360 SPIN", steps = new[] { (0, -720f), (1, 360f) }, seconds = 4.4f },
        };
        public void Initialize(RaceDirector owner, int n) { race = owner; car = GetComponent<ArcadeVehicle>(); slot = n; rng = new System.Random(DriverVariation.Seed * 31 + n * 7919 + 17); }
        public void Bind(RacerState state) => racer = state;
        public void Close() { if (links > 0) Bank(); car.AiAir = false; }
        void Say(string line, bool bailed) { LastLine = line; LineUntil = Time.time + 2.2f; LastBail = bailed; }
        void FixedUpdate()
        {
            if (!race || !car || racer == null) return;
            if (!race.Flow || race.Flow.State != RaceFlow.Stage.Racing || racer.Progress.Finished || racer.Dnf) { car.AiAir = false; if (racer.Progress.Finished && links > 0) Bank(); return; }
            float dt = Time.fixedDeltaTime; bool any = car.GroundedWheels > 0, grounded = car.GroundedWheels >= 2;
            var cfg = car.GetComponent<VehicleConfiguration>();
            if (!flying)
            {
                if (any) { ground += dt; if (links > 0 && (since += dt) >= ArcadeActivities.ComboGap) Bank(); }
                else if (ground >= .2f && car.Body.linearVelocity.magnitude > 3 && !(cfg && cfg.WipedOut))
                { flying = true; air = stable = 0; touch = -1; turned = Vector3.zero; takeoff = car.Body.position; planned = bail = abort = false; plan.Clear(); step = 0; planName = ""; }
                else ground = 0;
                return;
            }
            turned += car.transform.InverseTransformDirection(car.Body.angularVelocity) * (dt * Mathf.Rad2Deg);
            if (cfg && cfg.WipedOut) { Bailed("wiped out"); return; }
            if (any && touch < 0) { touch = 0; car.AiAir = false; }
            if (touch >= 0) touch += dt;
            if (!grounded || car.transform.up.y < .5f)
            {
                if (!grounded && touch < 0) { air += dt; if (!planned && air >= .12f) Plan(); Steer(); }
                stable = 0;
                if (touch >= ArcadeActivities.RecoverWindow) { if (car.transform.up.y < .5f) { Bailed("landed on its " + (car.transform.up.y < ArcadeActivities.RoofUp ? "roof" : "side")); return; } touch = -1; }
                return;
            }
            car.AiAir = false;
            if (stable == 0) landing = car.Body.position;
            stable += dt; if (stable < .3f) return;
            flying = false; ground = stable; Landed();
        }
        // the flight's remaining time: its path (no air drag) stepped until the wheels would meet the ground
        float TimeToLand()
        {
            var p = car.Body.position - car.transform.up * .55f; var v = car.Body.linearVelocity; var g = Physics.gravity; float t = 0, dt = .05f;
            while (t < 9)
            {
                var next = p + v * dt + .5f * g * dt * dt; var seg = next - p;
                foreach (var hit in Physics.RaycastAll(p, seg.normalized, seg.magnitude, car.groundMask, QueryTriggerInteraction.Ignore))
                    if (hit.rigidbody != car.Body && !hit.collider.transform.IsChildOf(car.transform)) return t + dt * hit.distance / Mathf.Max(1e-3f, seg.magnitude);
                p = next; v += g * dt; t += dt;
            }
            return 9;
        }
        void Plan()
        {
            planned = true; Flights++;
            float toLand = TimeToLand(), total = air + toLand, left = toLand - .5f; int d = Mathf.Clamp(race.difficulty, 0, 2); landAt = Time.time + toLand;
            var fit = Menu.Where(m => m.seconds <= left).ToList();
            if (fit.Count == 0 || car.airControl <= 0) { Log += $"flight {Flights}: {total:0.0} s, no trick\n"; return; }
            if (rng.NextDouble() > TryChance[d]) { Log += $"flight {Flights}: {total:0.0} s, chose none\n"; return; }
            // Easy: one of the two plainest that fit; Normal: any that fits; Hard: one of the two biggest that fit (more points for longer turns)
            var byTime = fit.OrderBy(m => m.seconds).ThenBy(_ => rng.Next()).ToList();
            var pick = d == 0 ? byTime[rng.Next(Mathf.Min(2, byTime.Count))] : d == 2 ? byTime[byTime.Count - 1 - rng.Next(Mathf.Min(2, byTime.Count))] : byTime[rng.Next(byTime.Count)];
            plan.AddRange(pick.steps); planName = pick.name; step = 0; stepBase = turned[plan[0].axis];
            bail = rng.NextDouble() < BailChance[d];
            Log += $"flight {Flights}: {total:0.0} s, {pick.name}{(bail ? " (will bail)" : "")}\n";
        }
        // the stick on the trick's axis: full rate, then eased in as the turn nears its end (so it stops square); every other axis held still
        void Steer()
        {
            if (step >= plan.Count || abort) { car.AiAir = false; return; }
            // out of time for the rest of it: let the vehicle level itself for the landing (whatever was turned is scored)
            if (landAt - Time.time < .35f) { abort = true; car.AiAir = false; return; }
            var (axis, degrees) = plan[step]; float done = turned[axis] - stepBase, remaining = degrees - done;
            float rate = axis == 1 ? ArcadeVehicle.AirSpinRate : axis == 0 ? ArcadeVehicle.AirTipRate : ArcadeVehicle.AirRollRate;
            float want = Mathf.Clamp(remaining * Mathf.Deg2Rad * 4f, -rate, rate);
            float w = car.transform.InverseTransformDirection(car.Body.angularVelocity)[axis];
            if (Mathf.Abs(remaining) < 6 && Mathf.Abs(w) < .5f) { step++; if (step < plan.Count) stepBase = turned[plan[step].axis]; else car.AiAir = false; return; }
            var input = Vector3.zero; input[axis] = axis == 2 ? -want / rate : want / rate; // roll: stick right rolls right (w.z = -stick * rate)
            car.AiAir = true; car.AiAirInput = input;
        }
        void Landed()
        {
            float distance = Vector3.ProjectOnPlane(landing - takeoff, Vector3.up).magnitude;
            if (air < ArcadeActivities.MinStuntAir || distance < ArcadeActivities.MinStuntDistance && Mathf.Abs(turned.y) < 300) return; // a hop
            if (bail && planName != "" && !SteerOnly) { Bailed("bailed the " + planName.ToLowerInvariant()); car.Body.linearVelocity *= .6f; car.Body.angularVelocity += car.transform.forward * 1.2f; return; }
            int flips = Mathf.RoundToInt(turned.x / 360), rolls = Mathf.RoundToInt(turned.z / 360), spins = Mathf.FloorToInt((Mathf.Abs(turned.y) + 30) / 360);
            var line = new List<string>(); int points = 0;
            if (flips != 0) { int n = Mathf.Abs(flips); line.Add((n == 2 ? "DOUBLE " : n >= 3 ? "TRIPLE " : "") + (flips < 0 ? "BACKFLIP" : "FRONTFLIP")); points += ArcadeActivities.FlipPoints * n; }
            if (rolls != 0) { int n = Mathf.Abs(rolls); line.Add((n == 2 ? "DOUBLE " : n >= 3 ? "TRIPLE " : "") + "ROLL"); points += ArcadeActivities.RollPoints * n; }
            if (spins > 0) { line.Add(spins * 360 + " SPIN"); points += ArcadeActivities.SpinPoints * spins; }
            int jump = Mathf.RoundToInt(Mathf.Min(distance, 250) * 10 + Mathf.Min(air, 12) * 100);
            Tricks += line.Count; if (links < ArcadeActivities.MaxLinks) links++; comboPoints += points + jump; since = 0;
            names.Add(line.Count > 0 ? string.Join(" + ", line) : "JUMP");
            if (line.Count > 0) Say(string.Join(" + ", line), false);
            Log += $"  landed: {(line.Count > 0 ? string.Join(" + ", line) : "no trick")} {points + jump} pts ({distance:0} m, {air:0.0} s)\n";
        }
        void Bailed(string why)
        {
            flying = false; ground = 0; car.AiAir = false; Bails++; Say("BAILED", true); Log += $"  bail: {why}\n";
            if (links > 0) Bank();
        }
        void Bank()
        {
            int total = comboPoints * Mathf.Max(1, links);
            if (total > 0 && !SteerOnly) StuntRaceScore.Current?.Bank(racer, total, string.Join(" · ", names), links);
            links = comboPoints = 0; names.Clear(); since = 0;
        }
        void OnDisable() { if (car) car.AiAir = false; }
    }

    // 0.119 Part B: the best combo per Stunt Race course (stunt-races-v1.json beside the other saves): the human players' combos in quick Stunt
    // Races (never the campaign's). The race and lap Top 10 boards are the normal ones under the Stunt Race's own course key.
    public static class StuntRaceRecords
    {
        public const string File = "stunt-races-v1.json";
        [Serializable] public sealed class Entry { public string course, vehicle, name, date, tricks; public int points; }
        [Serializable] sealed class Data { public List<Entry> best = new(); }
        static Data data = new(); static string path;
        public static string LastNew { get; private set; }
        public static void Load(string root)
        {
            path = Path.Combine(root, File); data = new Data();
            try { if (System.IO.File.Exists(path)) data = JsonUtility.FromJson<Data>(System.IO.File.ReadAllText(path)) ?? new Data(); data.best ??= new(); }
            catch (Exception e) { Debug.LogWarning("Stunt race records could not be read: " + e.Message); data = new Data(); }
        }
        public static Entry Best(string course) => data.best.FirstOrDefault(e => e.course == course);
        public static void Record(string course, int points, string vehicle, string name, string tricks)
        {
            var best = Best(course); if (best != null && best.points >= points) return;
            if (best == null) data.best.Add(best = new Entry { course = course });
            best.points = points; best.vehicle = vehicle; best.name = name; best.tricks = tricks; best.date = DateTime.UtcNow.ToString("o"); LastNew = course;
            try { if (path != null) AtomicSave.Write(path, JsonUtility.ToJson(data, true)); } catch (Exception e) { Debug.LogWarning("Stunt race records could not be saved: " + e.Message); }
        }
    }
}
