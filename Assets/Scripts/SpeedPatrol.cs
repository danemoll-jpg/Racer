using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Racer
{
    // 0.95 Part G: Police, round 2: Speed Patrol, in Free Roam (on the split-screen machinery: one player on the full screen,
    // two split). Everyone is a cop in a patrol car (player 2's in a dark blue livery). Each road has a speed limit (Hwy 92
    // 55 mph, Trickum Rd 45, S Cherokee Ln and the small roads 30): ordinary traffic keeps to it; now and then a traffic car
    // near a cop starts speeding (10 to 40 mph over; one every 20-30 s per cop, at most three at once). The radar shows the
    // speed of the vehicle the patrol car points at within 80 m (red over the limit) and clocks a speeder (then it shows on the
    // minimaps). Lights on (RB / H) within 30 m of a clocked speeder starts the pursuit: it runs (faster ones harder), on the
    // roads, turning back when a cop gets in front. The pull-over meter fills while a cop with lights on stays within 20 m (4 s
    // for a slow speeder up to 8 s for the fastest) and drains when it falls back; full = caught (it pulls over and stops).
    // 60 s after it was clocked, an uncaught speeder gets away. Points: a catch 100 + 10 per mph over when clocked (half for
    // ramming it to a stop); hitting any other vehicle -50; lights on a car that was not speeding (within 20 m for 4 s) -25 "NO
    // VIOLATION". At the end of the round a chase in progress may finish (up to 15 s). A cop's reset holds them 2 s. Results:
    // each cop's points, catches, fastest caught, penalties; the Speed Patrol Top 10 per round length (speed-patrol-v1.json).
    // Nothing else is recorded. Solo play never swaps roles (Part A): everyone is a cop.
    public sealed class SpeedPatrol : MonoBehaviour
    {
        public const float RadarRange = 80, RadarCone = 10, PursuitRange = 30, MeterRange = 20, GetAwaySeconds = 60, NoViolationSeconds = 4, Overtime = 15, ResetHold = 2, StartPause = 3;
        public const int MaxSpeeders = 3, CatchPoints = 100, PerMph = 10, HitPenalty = 50, NoViolationPenalty = 25;
        public static SpeedPatrol Current { get; private set; }
        public enum Phase { Starting, Running, Overtime, Done }
        public sealed class Cop
        {
            public int player; public ArcadeVehicle car; public bool siren; public PoliceLights lights;
            public int points, catches, penalties, hits, noViolations; public float fastest;
            public string line = ""; public float lineUntil; public float radar = -1; public bool radarOver; public int limit = -1, radarLimit = -1;
            public float nextSpawn, holdUntil; public readonly Dictionary<RoadDriver, float> lightsOn = new(); public readonly HashSet<RoadDriver> penalised = new(); public readonly Dictionary<ArcadeVehicle, float> lastHit = new();
            public readonly List<string> log = new(); public System.Action OnReset;
        }
        public sealed class Speeder
        {
            public RoadDriver driver; public int over; public float since; public bool clocked; public float clockedAt; public int clockedOver; public bool pursuit;
            public readonly float[] meter = new float[3]; public float stoppedUntil = -1; public bool caught, gone; public float rammedAt = -1; public int rammedBy; public float nextTurn;
            public ArcadeVehicle Car => driver ? driver.Car : null;
        }
        public readonly List<Cop> Cops = new(); public readonly List<Speeder> Speeders = new();
        public Phase State { get; private set; }
        public float Clock { get; private set; } // seconds of the round driven
        public float Limit => SplitScreen.PoliceMinutes * 60;
        public int Clocked => Speeders.Count(s => s.clocked); public int GotAway { get; private set; }
        RaceFlow flow; RaceDirector race; float phaseAt, overtimeAt, nextTraffic;

        public void Initialize(RaceFlow owner) { flow = owner; race = owner.Race; Current = this; }
        void OnDestroy() { if (Current == this) Current = null; foreach (var s in Speeders) Release(s); }
        public Cop CopOf(int player) => Cops.FirstOrDefault(c => c.player == player);

        // ---------- speed limits ----------
        // Hwy 92 (the through route and the four-lane corridor of the street loop) 55 mph; Trickum Rd (the west side of the loop,
        // from Hwy 92 at -605,540 south to S Cherokee Ln at -614,-544) 45; S Cherokee Ln and every small road and trail 30.
        public static int LimitAt(RaceDirector race, Vector3 p, out bool onRoad)
        {
            onRoad = false; if (!race) return 30;
            float best = float.MaxValue; RaceRoad road = null; float at = 0;
            foreach (var r in new[] { race.throughRoad, race.ambientRoad, race.road }) { if (!r || r.points == null || r.points.Length < 2) continue; float s = r.Project(p, out float lat); if (lat < best) { best = lat; road = r; at = s; } }
            if (road == null) return 30;
            onRoad = best < road.HalfWidth(at) + 2.5f;
            if (road.openHighway || road.HighwayBlend(at) > .5f) return 55;
            if (p.x < -560 && p.x > -665 && p.z > -560 && p.z < 545) return 45;
            return 30;
        }
        public static float Mps(float mph) => mph / 2.23694f;
        public static float Mph(float mps) => mps * 2.23694f;

        // RaceDirector.RestartRace (via SplitRace.PlayerTwo): the cops' cars (player 1's is the scene vehicle; player 2's made by
        // SplitRoam) as patrol cars, held for the start
        public void ApplyPlayerOne()
        {
            var c = race.vehicle.GetComponent<VehicleConfiguration>(); c.Apply(VehicleProfile.Police.Id); c.SetPaint(new Color(.03f, .03f, .035f));
        }
        public static readonly Color SecondLivery = new(.04f, .09f, .32f);
        public void BeginRound()
        {
            foreach (var s in Speeders) Release(s); Speeders.Clear(); Cops.Clear(); GotAway = 0;
            AddCop(1, race.vehicle); if (!SplitScreen.Solo && SplitRoam.Current && SplitRoam.Current.Car) AddCop(2, SplitRoam.Current.Car);
            Clock = 0; State = Phase.Starting; phaseAt = Time.time; nextTraffic = 0;
            foreach (var c in Cops) { Hold(c.car, true); c.nextSpawn = Time.time + StartPause + Random.Range(5f, 9f) + (c.player - 1) * 4; }
        }
        void AddCop(int player, ArcadeVehicle car)
        {
            var cop = new Cop { player = player, car = car };
            cop.lights = car.GetComponent<PoliceLights>(); if (!cop.lights) cop.lights = car.gameObject.AddComponent<PoliceLights>(); cop.lights.Siren = false; cop.lights.Target = null; cop.lights.Rebuild();
            var contacts = car.GetComponent<PatrolContacts>(); if (!contacts) contacts = car.gameObject.AddComponent<PatrolContacts>(); contacts.Cop = cop; contacts.Patrol = this;
            var respawn = car.GetComponent<VehicleRespawn>(); respawn.Respawned -= cop.OnReset; cop.OnReset = () => { if (State == Phase.Running || State == Phase.Overtime) { cop.holdUntil = Time.time + ResetHold; Hold(car, true); } }; respawn.Respawned += cop.OnReset;
            Cops.Add(cop);
        }
        readonly HashSet<ArcadeVehicle> held = new();
        public bool Holds(ArcadeVehicle car) => held.Contains(car);
        void Hold(ArcadeVehicle car, bool on)
        {
            if (!car) return;
            if (on) { held.Add(car); car.Body.linearVelocity = car.Body.angularVelocity = Vector3.zero; car.Body.isKinematic = true; car.enabled = false; }
            else { held.Remove(car); car.Body.isKinematic = false; car.enabled = true; }
            var input = car.GetComponent<VehicleInput>(); if (input) input.enabled = !on;
        }
        IEnumerable<RoadDriver> Traffic => race.Drivers.Where(d => d && d.Car && d.isActiveAndEnabled && d.GetComponent<AmbientVehicle>());
        Speeder SpeederOf(RoadDriver d) => Speeders.FirstOrDefault(s => s.driver == d && !s.gone && !s.caught);
        Speeder SpeederOf(ArcadeVehicle car) => Speeders.FirstOrDefault(s => s.Car == car && !s.gone);

        void Update()
        {
            if (!flow || flow.State != RaceFlow.Stage.Racing || State == Phase.Done) return;
            foreach (var car in held) if (car && (!car.Body.isKinematic || car.enabled)) { car.Body.isKinematic = true; car.enabled = false; var input = car.GetComponent<VehicleInput>(); if (input) input.enabled = false; }
            if (State == Phase.Starting) { if (Time.time - phaseAt >= StartPause) { foreach (var c in Cops) Hold(c.car, false); State = Phase.Running; } return; }
            Clock += Time.deltaTime;
            foreach (var c in Cops)
            {
                if (c.holdUntil > 0 && Time.time >= c.holdUntil) { c.holdUntil = 0; Hold(c.car, false); }
                if (Toggle(c.player)) c.siren = !c.siren;
                c.lights.Siren = c.siren; c.limit = LimitAt(race, c.car.Body.position, out bool on); if (!on) c.limit = -1;
                Radar(c); Spawn(c); LightsOnTraffic(c);
            }
            foreach (var s in Speeders.ToList()) Run(s);
            if (Time.time >= nextTraffic) { nextTraffic = Time.time + .5f; Pace(); }
            if (State == Phase.Running && Clock >= Limit) { State = Phase.Overtime; overtimeAt = Time.time; foreach (var c in Cops) Say(c, "TIME UP: finish your chase"); }
            if (State == Phase.Overtime && (!Speeders.Any(s => s.pursuit && !s.caught && !s.gone) || Time.time - overtimeAt >= Overtime)) Finish();
        }
        bool Toggle(int player)
        {
            var device = SplitScreen.DeviceOf(player);
            if (device is Gamepad g) return g.rightShoulder.wasPressedThisFrame;
            if (device is Keyboard k) return k.hKey.wasPressedThisFrame;
            return false;
        }
        // traffic keeps to the limit; a speeder drives its limit + its excess (more while it runs); a caught one stops
        void Pace()
        {
            foreach (var d in Traffic)
            {
                var s = SpeederOf(d); int limit = LimitAt(race, d.Car.Body.position, out _);
                if (s != null && s.stoppedUntil > 0) { d.SpeedOverride = 0; continue; }
                d.SpeedOverride = s == null ? Mps(limit) * .97f : Mps(limit + s.over + (s.pursuit ? 8 + s.over * .4f : 0));
            }
            foreach (var s in Speeders.Where(s => s.caught && s.stoppedUntil > 0 && Time.time >= s.stoppedUntil).ToList()) Release(s);
        }
        void Release(Speeder s) { if (s.driver) s.driver.SpeedOverride = -1; s.stoppedUntil = -1; Speeders.Remove(s); }
        // a new speeder near this cop: every 20-30 s, at most three at once
        void Spawn(Cop c)
        {
            if (State != Phase.Running || Time.time < c.nextSpawn) return;
            c.nextSpawn = Time.time + Random.Range(20f, 30f);
            if (Speeders.Count(s => !s.gone && !s.caught) >= MaxSpeeders) return;
            var pick = Traffic.Where(d => SpeederOf(d) == null && Speeders.All(s => s.driver != d)).Select(d => (d, dist: Vector3.Distance(d.Car.Body.position, c.car.Body.position))).Where(x => x.dist > 50 && x.dist < 260).OrderBy(x => x.dist).FirstOrDefault();
            if (!pick.d) { c.nextSpawn = Time.time + 4; return; }
            Speeders.Add(new Speeder { driver = pick.d, over = Random.Range(10, 41), since = Time.time });
        }
        // the radar: the nearest vehicle within 80 m inside 10 degrees of the patrol car's heading; clocking a speeder
        void Radar(Cop c)
        {
            var f = Vector3.ProjectOnPlane(c.car.transform.forward, Vector3.up).normalized; RoadDriver target = null; float best = RadarRange;
            foreach (var d in Traffic)
            {
                var to = d.Car.Body.position - c.car.Body.position; float dist = to.magnitude; if (dist > best || dist < 2) continue;
                if (Vector3.Angle(f, Vector3.ProjectOnPlane(to, Vector3.up)) > RadarCone) continue; best = dist; target = d;
            }
            if (!target) { c.radar = -1; c.radarOver = false; return; }
            c.radar = Mph(target.Car.Body.linearVelocity.magnitude); c.radarLimit = LimitAt(race, target.Car.Body.position, out _); c.radarOver = c.radar > c.radarLimit + 1;
            var s = SpeederOf(target);
            if (s != null && !s.clocked && c.radarOver) { s.clocked = true; s.clockedAt = Time.time; s.clockedOver = Mathf.Max(1, Mathf.RoundToInt(c.radar - c.radarLimit)); Say(c, $"CLOCKED  {Mathf.RoundToInt(c.radar)} in a {c.radarLimit}  (+{s.clockedOver})"); c.log.Add($"clocked {s.clockedOver} over"); }
        }
        // lights on a car that was not speeding: within 20 m for 4 s (not while chasing a speeder close by) costs 25, once a car
        void LightsOnTraffic(Cop c)
        {
            var near = new HashSet<RoadDriver>();
            bool chasing = Speeders.Any(s => s.pursuit && !s.caught && !s.gone && s.Car && Vector3.Distance(s.Car.Body.position, c.car.Body.position) < PursuitRange + 10);
            if (c.siren && !chasing)
                foreach (var d in Traffic)
                {
                    if (Vector3.Distance(d.Car.Body.position, c.car.Body.position) > MeterRange) continue; var s = SpeederOf(d); if (s != null && s.clocked) continue;
                    near.Add(d); c.lightsOn[d] = (c.lightsOn.TryGetValue(d, out var t) ? t : 0) + Time.deltaTime;
                    if (c.lightsOn[d] >= NoViolationSeconds && c.penalised.Add(d)) { c.points -= NoViolationPenalty; c.penalties++; c.noViolations++; Say(c, $"−{NoViolationPenalty}  NO VIOLATION"); c.log.Add("no violation"); }
                }
            foreach (var d in c.lightsOn.Keys.ToList()) if (!near.Contains(d)) c.lightsOn.Remove(d);
        }
        // a speeder: the pursuit (lights on within 30 m of it once clocked), running away, the pull-over meters, getting away
        void Run(Speeder s)
        {
            var car = s.Car; if (!car) { Speeders.Remove(s); return; }
            if (s.caught || s.gone) return;
            if (!s.clocked && Time.time - s.since > GetAwaySeconds) { Release(s); return; } // never clocked: back to ordinary traffic
            if (s.clocked && !s.pursuit && Cops.Any(c => c.siren && Vector3.Distance(c.car.Body.position, car.Body.position) < PursuitRange)) s.pursuit = true;
            if (s.pursuit)
            {
                float fill = Mathf.Lerp(4, 8, Mathf.InverseLerp(10, 40, s.clockedOver));
                foreach (var c in Cops)
                {
                    bool close = c.siren && Vector3.Distance(c.car.Body.position, car.Body.position) < MeterRange;
                    s.meter[c.player] = Mathf.Clamp01(s.meter[c.player] + (close ? 1 : -1) * Time.deltaTime / fill);
                    if (s.meter[c.player] >= 1) { Catch(s, c, false); return; }
                }
                // turning back when a cop gets round in front of it (within 120 m), at most every 12 s
                if (Time.time > s.nextTurn && s.driver)
                    foreach (var c in Cops)
                    {
                        var to = c.car.Body.position - car.Body.position; float ahead = Vector3.Dot(to, car.transform.forward);
                        if (ahead > 0 && ahead < 120 && to.magnitude > 25) { s.nextTurn = Time.time + 12; s.driver.Initialize(race, car, false, -s.driver.Direction, 1); break; }
                    }
            }
            // rammed to a stop: a catch at half points
            if (s.rammedAt > 0 && Time.time - s.rammedAt < 3 && car.Body.linearVelocity.magnitude < 2) { Catch(s, CopOf(s.rammedBy), true); return; }
            if (s.clocked && Time.time - s.clockedAt >= GetAwaySeconds) { s.gone = true; GotAway++; foreach (var c in Cops) Say(c, "A SPEEDER GOT AWAY"); foreach (var c in Cops) c.log.Add("got away"); Release(s); }
        }
        void Catch(Speeder s, Cop c, bool rammed)
        {
            if (c == null) return; s.caught = true; s.pursuit = false; s.stoppedUntil = Time.time + 6;
            int points = (CatchPoints + PerMph * s.clockedOver) / (rammed ? 2 : 1); c.points += points; c.catches++; c.fastest = Mathf.Max(c.fastest, s.clockedOver);
            Say(c, $"+{points}  CAUGHT{(rammed ? " (rammed: half points)" : "")}  ({s.clockedOver} mph over)"); c.log.Add($"caught {s.clockedOver} over{(rammed ? " rammed" : "")}");
            if (s.driver) s.driver.SpeedOverride = 0;
        }
        // PatrolContacts: a cop hit another vehicle
        public void Hit(Cop c, ArcadeVehicle other, float speed)
        {
            if (State != Phase.Running && State != Phase.Overtime) return;
            if (Cops.Any(x => x.car == other) || speed < 2.5f) return;
            if (c.lastHit.TryGetValue(other, out var t) && Time.time - t < 3) return; c.lastHit[other] = Time.time;
            var s = SpeederOf(other);
            if (s != null && s.clocked && !s.caught) { s.rammedAt = Time.time; s.rammedBy = c.player; return; }
            c.points -= HitPenalty; c.penalties++; c.hits++; Say(c, $"−{HitPenalty}  HIT TRAFFIC"); c.log.Add("hit traffic");
        }
        void Say(Cop c, string line) { c.line = line; c.lineUntil = Time.unscaledTime + 3.5f; }
        public IEnumerable<Vector3> ClockedMarks => Speeders.Where(s => s.clocked && !s.caught && !s.gone && s.Car).Select(s => s.Car.transform.position);
        public float MeterFor(int player) => Speeders.Where(s => s.pursuit && !s.caught && !s.gone).Select(s => s.meter[player]).DefaultIfEmpty(0).Max();

        // ---------- the end ----------
        [System.Serializable] public sealed class Entry { public string name; public int points, catches, minutes; public string date; }
        [System.Serializable] sealed class Board { public List<Entry> entries = new(); }
        public readonly List<int> Places = new(); // the board places this round's cops took (0 = none)
        void Finish()
        {
            State = Phase.Done; foreach (var c in Cops) { c.siren = false; c.lights.Siren = false; Hold(c.car, false); }
            foreach (var s in Speeders.ToList()) Release(s);
            Places.Clear(); var board = LoadBoard();
            foreach (var c in Cops)
            {
                var e = new Entry { name = SplitScreen.NameOf(c.player), points = c.points, catches = c.catches, minutes = SplitScreen.PoliceMinutes, date = System.DateTime.Now.ToString("yyyy-MM-dd") };
                board.entries.Add(e); var top = board.entries.Where(x => x.minutes == e.minutes).OrderByDescending(x => x.points).ThenBy(x => board.entries.IndexOf(x)).Take(10).ToList();
                Places.Add(top.IndexOf(e) + 1);
                board.entries = board.entries.Where(x => x.minutes != e.minutes || top.Contains(x)).ToList();
            }
            SaveBoard(board); flow.CompleteResults();
        }
        public static string BoardPath => Hints.Flow && Hints.Flow.Save != null ? Path.Combine(Hints.Flow.Save.DirectoryPath, "speed-patrol-v1.json") : null;
        static Board LoadBoard() { try { var p = BoardPath; if (p != null && File.Exists(p)) return JsonUtility.FromJson<Board>(File.ReadAllText(p)) ?? new Board(); } catch { } return new Board(); }
        static void SaveBoard(Board b) { try { var p = BoardPath; if (p != null) AtomicSave.Write(p, JsonUtility.ToJson(b, true)); } catch (System.Exception e) { Debug.LogWarning("Speed Patrol Top 10 not saved: " + e.Message); } }
        public static List<Entry> Top(int minutes) => LoadBoard().entries.Where(e => e.minutes == minutes).OrderByDescending(e => e.points).Take(10).ToList();
        public int Winner => Cops.Count < 2 ? 0 : Cops[0].points > Cops[1].points ? 1 : Cops[1].points > Cops[0].points ? 2 : 0;
        public void Rematch() { flow.StartFreeRoam(); }
    }

    // the patrol car's contacts with other vehicles (Speed Patrol)
    public sealed class PatrolContacts : MonoBehaviour
    {
        public SpeedPatrol.Cop Cop; public SpeedPatrol Patrol;
        void OnCollisionEnter(Collision c)
        {
            if (!Patrol || Cop == null) return; var other = c.collider.GetComponentInParent<ArcadeVehicle>(); if (!other || other.gameObject == gameObject) return;
            Patrol.Hit(Cop, other, c.relativeVelocity.magnitude);
        }
    }
}
