using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Racer
{
    // 0.96 Part E: Police, round 3: the AI cops chase you. Free Roam only, on the split-screen machinery (SplitRoam): one player
    // on the full screen, two split. Two games use it:
    //   Getaway          every human is a runner; the cops are AI. Heat starts at 1 (2 cops) and goes up one level every 45 s of
    //                    the runner staying free (max 5), a cop per level (max 6); from heat 3 roadblocks. A cop sees the runner
    //                    within 120 m with a clear line of sight; while no cop sees it an ESCAPE meter fills over 20 s (any cop
    //                    seeing it resets it): full = escaped. The round limit (3 / 5 / 8 min) counts as escaped. Caught = the bust
    //                    meter fills (0.94: a cop within 10 m and the runner under 7 m/s for 3 s; here also two cops boxing it in
    //                    under 14 m/s, slowly, and a cop's contact under 18 m/s, slower still).
    //   Cop vs Runner    a solo human runner against one AI cop (the 0.95 greyed Runner role): the same cop driving and bust
    //                    meter; no heat, escape meter or roadblocks; the clock at the limit = escaped.
    // The cops drive only the road network (RoadNet, CopDriver). On the road they chase (the nearest cop behind the runner, a
    // second alongside, the rest cutting it off ahead); off the road the cop following stops at the edge and radios it in while
    // the others drive to the exits (nearest first, one cop each) and wait with lights on; unseen, they search outward from the
    // last place the runner was seen. Two players: both run, the one who stays free longest wins; a caught player watches the
    // other's half full screen. Player 2 as the AI also runs (and draws cops) but never ends the round.
    public sealed class GetawayChase : MonoBehaviour
    {
        public const float SightRange = 120, SightRangeHard = 120, NearCops = 150, CatchDistance = 10, CatchSpeed = 7, BoxSpeed = 18, ContactSpeed = 22, PinDistance = 20, PinSpeed = 99, HeldSpeed = 15, HeldDistance = 20, RunnerRelease = 3, CopDelay = 2, EndPause = 4, ResetHold = 2, HeatSeconds = 45;
        public static GetawayChase Current { get; private set; }
        public enum Phase { Starting, Running, Over, Done }
        public enum Variant { Getaway, CopRunner }
        public sealed class Runner
        {
            public int player; public ArcadeVehicle car; public bool human; public Rigidbody body;
            public float bust, escape, freeSeconds, endedAt, seenAt = -99, lastSeenAt = -99, offSince = -1, onSince, holdUntil, contactAt = -99, unseenFor, offSeen, lastCallIn = -99, lastRam = -99;
            public Vector3 lastSeenPos, lastSeenVelocity, offFrom, prevVel; public int offFromNode = -1;
            public bool seen, offRoad, caught, escaped, wasSeen, heliSees, offBumped; public int topHeat = 1, dodged, blocks, copsChasing; public string outcome = "";
            public readonly HashSet<Cop> near = new(); public readonly HashSet<Roadblock> passedBlocks = new();
            public bool Done => caught || escaped; public bool limit; public System.Action resetHook;
        }
        public sealed class Cop
        {
            public int id; public ArcadeVehicle car; public CopDriver driver; public PoliceLights lights; public Runner assigned; public bool sees; public bool block, exitUnit, catching; public float closeAt = -99, bornAt, boost = 1, farSince = -1; public bool wasClose; public float nextPlan, searchUntil;
            public Vector3 Position => car ? car.Body.position : Vector3.zero;
        }
        public sealed class Roadblock { public Vector3 centre, tangent; public Cop a, b; public float born; public Runner target; public bool passed; public float closest = 999; public int node; }
        public readonly List<Runner> Runners = new(); public readonly List<Cop> Cops = new(); public readonly List<Roadblock> Blocks = new();
        public Variant Mode { get; private set; }
        public Phase State { get; private set; }
        public RoadNet Net { get; private set; }
        public float Clock { get; private set; }
        public int Heat { get; private set; } = 1;
        public float Limit => SplitScreen.PoliceMinutes * 60;
        public int Difficulty => Mathf.Clamp(SplitScreen.PoliceDifficulty, 0, 2);
        // 0.97: Easy / Normal / Hard change the most cops, the seconds per heat level, the seconds unseen to escape and how fast the
        // bust meter fills; never the cops' speed (every cop has the runner's vehicle's top speed and acceleration).
        public static readonly int[] CopCap = { 4, 6, 6 }; public static readonly float[] HeatEvery = { 45, 30, 20 }; public static readonly float[] EscapeBy = { 20, 30, 40 }; public static readonly float[] BustRate = { .8f, 1f, 1.25f };
        public float CopSpeed => 1f; public int MaxCops => Mode == Variant.CopRunner ? 1 : CopCap[Difficulty];
        public readonly List<string> TraceLog = new(); void Trace(string s) { TraceLog.Add(s); if (TraceLog.Count > 200) TraceLog.RemoveAt(0); }
        public List<string> RadioLog = new(); public string Radio => RadioLog.Count > 0 && Time.unscaledTime - radioAt < 9 ? RadioLog[RadioLog.Count - 1] : ""; float radioAt;
        public readonly List<Vector3> ExitPoints = new();
        public readonly List<(string name, float seconds, int heat, string outcome)> Results = new();
        public PoliceHelicopter Heli; public bool HeliSays; float nextPrune; RaceFlow flow; RaceDirector race; float phaseAt, nextTick, heatClock, nextBlock, nextBackup; int copSerial; readonly HashSet<ArcadeVehicle> held = new(); readonly RaycastHit[] rays = new RaycastHit[24];
        public bool Siren => State == Phase.Running; public bool Holds(ArcadeVehicle car) => held.Contains(car);
        public bool IsCop(Rigidbody body) => body && Cops.Any(c => c.car && c.car.Body == body);

        public void Initialize(RaceFlow owner) { flow = owner; race = owner.Race; Current = this; }
        void OnDestroy() { ClearCops(); if (Current == this) Current = null; }
        public ArcadeVehicle CarOf(int player) => player == 1 ? race.vehicle : SplitRoam.Current ? SplitRoam.Current.Car : null;
        public string VehicleFor(int player) => player == 1 ? SplitScreen.P1Vehicle : SplitScreen.P2Vehicle;
        public Runner RunnerOf(int player) => Runners.FirstOrDefault(r => r.player == player);
        public static string Mss(float seconds) { int s = Mathf.FloorToInt(seconds); return $"{s / 60}:{s % 60:00}"; }

        // RaceDirector.RestartRace (via SplitRace.PlayerTwo): the runners' vehicles exist; place them on the road, build the network, make the first cops
        public void ApplyPlayerOne()
        {
            var c = race.vehicle.GetComponent<VehicleConfiguration>(); c.Apply(VehicleFor(1)); c.SetBodyColor(SplitScreen.P1Color);
        }
        public void BeginRound()
        {
            SplitScreen.Watching = 0; ClearCops(); Runners.Clear(); Results.Clear(); RadioLog.Clear(); ExitPoints.Clear();
            Mode = SplitScreen.PoliceGame == SplitScreen.Game.Getaway ? Variant.Getaway : Variant.CopRunner;
            Net = RoadNet.Build(race);
            var road = race.ambientRoad ? race.ambientRoad : race.road;
            float s = road.Project(race.vehicle.Body.position, out _); var p = road.At(s, out var f); f = Vector3.ProjectOnPlane(f, Vector3.up).normalized; var right = Vector3.Cross(Vector3.up, f);
            var facing = Quaternion.LookRotation(f);
            var r1 = new Runner { player = 1, car = race.vehicle, human = true }; Runners.Add(r1);
            Put(r1.car, p + right * 2.4f, facing);
            var car2 = SplitRoam.Current ? SplitRoam.Current.Car : null;
            if (car2) { var r2 = new Runner { player = 2, car = car2, human = !SplitScreen.P2Ai }; Runners.Add(r2); Put(car2, p - right * 2.4f, facing); if (SplitScreen.P2Ai) SplitRoam.Current.Cruise(1.25f, 1); }
            foreach (var r in Runners)
            {
                r.body = r.car.Body; Hold(r.car, true);
                var respawn = r.car.GetComponent<VehicleRespawn>(); var capture = r; System.Action reset = () => RunnerReset(capture);
                respawn.Respawned -= r.resetHook; r.resetHook = reset; respawn.Respawned += reset;
                var lights = r.car.GetComponent<PoliceLights>(); if (lights) Destroy(lights);
            }
            Clock = 0; Heat = 1; heatClock = 0; nextBlock = 0; nextBackup = 0; nextPrune = 0; Heli = null; HeliSays = false; State = Phase.Starting; phaseAt = Time.time; nextTick = 0; copSerial = 0;
            // the first cops wait behind the runner (released CopDelay after the runners); the cop count by heat
            int node = Net.Nearest(r1.car.Body.position, out _, 200);
            int want = StartingCops();
            for (int i = 0; i < want; i++)
            {
                int at = node >= 0 ? Net.Ahead(node, -f, 85 + 45 * i) : -1;
                var cop = SpawnCop(at >= 0 ? Net.P[at] : p - f * (85 + 45 * i), at >= 0 ? TowardsHeading(at, f) : f, true); if (cop != null) cop.driver.Frozen = true;
            }
            Say(Mode == Variant.Getaway ? "Dispatch: all units, runner on the road" : "Dispatch: unit on the runner");
        }
        int StartingCops() => Mode == Variant.CopRunner ? 1 : Mathf.Min(MaxCops, 2);
        Vector3 TowardsHeading(int node, Vector3 f) { var t = Net.Tangent(node); return Vector3.Dot(t, f) < 0 ? -t : t; }
        void Put(ArcadeVehicle car, Vector3 at, Quaternion facing)
        {
            var respawn = car.GetComponent<VehicleRespawn>(); respawn.CancelRecovery();
            if (!respawn.TryFastTravel(at + Vector3.up, facing)) { car.Body.position = at + Vector3.up; car.Body.rotation = facing; car.transform.SetPositionAndRotation(at + Vector3.up, facing); }
            car.ClearSteering(); if (car == race.vehicle) FindAnyObjectByType<ChaseCamera>()?.Snap();
        }
        void Hold(ArcadeVehicle car, bool on)
        {
            if (!car) return;
            if (on) { held.Add(car); car.Body.linearVelocity = car.Body.angularVelocity = Vector3.zero; car.Body.isKinematic = true; car.enabled = false; var d = car.GetComponent<RoadDriver>(); if (d) d.enabled = false; }
            else { held.Remove(car); car.Body.isKinematic = false; car.enabled = true; var d = car.GetComponent<RoadDriver>(); if (d) d.enabled = true; }
            var input = car.GetComponent<VehicleInput>(); if (input && !(car != race.vehicle && SplitScreen.P2Ai)) input.enabled = !on;
        }
        void RunnerReset(Runner r) { if (State != Phase.Running || r.Done) return; r.holdUntil = Time.time + ResetHold; Hold(r.car, true); }

        // ---------- the cops ----------
        Cop SpawnCop(Vector3 at, Vector3 forward, bool quiet = false)
        {
            var clone = Instantiate(race.vehicle.gameObject); clone.name = "COP " + (++copSerial);
            foreach (var c in clone.GetComponents<ActivityLandingContact>()) Destroy(c);
            foreach (var d in clone.GetComponents<RoadDriver>()) { d.enabled = false; Destroy(d); }
            foreach (var d in clone.GetComponents<CopDriver>()) Destroy(d);
            foreach (var l in clone.GetComponents<PoliceLights>()) Destroy(l);
            var sound = clone.GetComponent<VehicleAudio>(); if (sound) { sound.enabled = false; DestroyImmediate(sound); }
            foreach (var a in clone.GetComponents<AudioSource>()) DestroyImmediate(a);
            var car = clone.GetComponent<ArcadeVehicle>(); var config = clone.GetComponent<VehicleConfiguration>(); config.classicVisual = false;
            config.Apply(VehicleProfile.Police.Id); config.SetPaint(new Color(.03f, .03f, .035f));
            clone.AddComponent<VehicleAudio>();
            var input = clone.GetComponent<VehicleInput>(); if (input) { input.Bind(null); input.enabled = false; }
            var respawn = clone.GetComponent<VehicleRespawn>(); if (respawn) respawn.enabled = false;
            car.Body.isKinematic = false; car.enabled = false;
            MatchRunner(car);
            var driver = clone.AddComponent<CopDriver>(); driver.BaseTop = car.topSpeed; driver.Car = car; driver.Race = race; driver.Net = Net; driver.Boss = this; driver.SpeedFactor = CopSpeed;
            var lights = clone.AddComponent<PoliceLights>(); lights.Siren = !quiet; lights.Target = null; driver.Lights = lights;
            var cop = new Cop { id = copSerial, car = car, driver = driver, lights = lights, bornAt = Time.time };
            driver.Place(at, forward);
            Cops.Add(cop); return cop;
        }
        // 0.97: every cop has the top speed and acceleration of the runner's vehicle (the faster of the two when two people run), and at
        // least its grip and braking, whatever the vehicle (the mower included) and on every difficulty
        void MatchRunner(ArcadeVehicle car)
        {
            float top = 0, acc = 0, grip = 0, brake = 0, lat = 0;
            foreach (var r in Runners) { var m = r.car; if (!m) continue; top = Mathf.Max(top, m.topSpeed); acc = Mathf.Max(acc, m.acceleration); grip = Mathf.Max(grip, m.maxGripAcceleration); brake = Mathf.Max(brake, m.braking); lat = Mathf.Max(lat, m.lateralGrip); }
            if (top <= 0) return;
            car.topSpeed = top; car.acceleration = acc; car.maxGripAcceleration = Mathf.Max(car.maxGripAcceleration, grip); car.braking = Mathf.Max(car.braking, brake); car.lateralGrip = Mathf.Max(car.lateralGrip, lat);
        }
        void ClearCops()
        {
            if (Heli) { Destroy(Heli.gameObject); Heli = null; }
            foreach (var c in Cops) if (c.car) { c.car.gameObject.SetActive(false); Destroy(c.car.gameObject); }
            Cops.Clear(); Blocks.Clear();
        }
        public void Contact(CopDriver driver, Collision c)
        {
            var other = c.collider.GetComponentInParent<ArcadeVehicle>(); if (!other) return;
            foreach (var r in Runners)
                if (r.car == other)
                {
                    r.contactAt = Time.time;
                    if (Mode == Variant.Getaway && Difficulty > 0 && State == Phase.Running && c.relativeVelocity.magnitude > 5 && Time.time - r.lastRam > 6 && r.body && driver.Car && RunnerRammed(r, driver)) { r.lastRam = Time.time; BumpHeat("the suspect rammed a unit"); }
                }
        }

        // heat goes up only when the runner drove into the cop (the runner closes on the cop faster than the cop closes on the runner), from the velocities before this physics step
        static bool RunnerRammed(Runner r, CopDriver cop)
        {
            var toCop = cop.Car.Body.position - r.body.position; toCop.y = 0; if (toCop.sqrMagnitude < .01f) return false; toCop.Normalize();
            return Vector3.Dot(r.prevVel, toCop) > Vector3.Dot(cop.PrevVel, -toCop);
        }
        void FixedUpdate() { foreach (var r in Runners) if (r.body) r.prevVel = r.body.linearVelocity; }

        // ---------- the round ----------
        void Update()
        {
            if (!flow || flow.State != RaceFlow.Stage.Racing || Runners.Count == 0) return;
            foreach (var car in held) if (car && (!car.Body.isKinematic || car.enabled)) { car.Body.isKinematic = true; car.enabled = false; var input = car.GetComponent<VehicleInput>(); if (input) input.enabled = false; }
            float t = Time.time - phaseAt;
            switch (State)
            {
                case Phase.Starting:
                    if (t >= RunnerRelease)
                    {
                        foreach (var r in Runners) if (held.Contains(r.car) && r.holdUntil == 0) Hold(r.car, false);
                        Clock = t - RunnerRelease;
                    }
                    if (t >= RunnerRelease + CopDelay) { State = Phase.Running; foreach (var c in Cops) { c.driver.Frozen = false; c.lights.Siren = true; } foreach (var r in Runners) r.holdUntil = 0; }
                    break;
                case Phase.Running: RunningUpdate(); break;
                case Phase.Over:
                    if (t >= EndPause) Finish();
                    break;
            }
            foreach (var r in Runners) if (r.holdUntil > 0 && Time.time >= r.holdUntil && !r.Done) { r.holdUntil = 0; Hold(r.car, false); }
        }
        void RunningUpdate()
        {
            float dt = Time.deltaTime; Clock += dt;
            foreach (var r in Runners)
            {
                if (r.Done) continue; r.freeSeconds = Clock;
                // the bust meter
                float speed = r.body.linearVelocity.magnitude; int near = 0; bool anyNear = false;
                foreach (var c in Cops) { if (!c.car) continue; float d = Vector3.Distance(c.car.Body.position, r.body.position); if (d < CatchDistance) { near++; anyNear = true; } }
                float rate = -1f / 3f;
                int pin = 0; foreach (var c in Cops) if (c.car && !c.block && Vector3.Distance(c.car.Body.position, r.body.position) < PinDistance) pin++;
                if (near >= 1 && speed < CatchSpeed) rate = 1f / 3f;
                else if (near >= 2 && speed < BoxSpeed) rate = 1f / 6f;
                else if (near >= 1 && Time.time - r.contactAt < .6f && speed < ContactSpeed) rate = 1f / 9f;
                else if (pin >= 2 && speed < PinSpeed) rate = 1f / 6f;
                else if (near >= 1) rate = 1f / 12f; // a cop glued to the runner's bumper (within 10 m) at any speed: a runner who cannot shake it is caught in about 12 s
                else if (speed < HeldSpeed && Cops.Any(c => c.car && !c.block && Vector3.Distance(c.car.Body.position, r.body.position) < HeldDistance)) rate = 1f / 6f; // 0.97: held up with a cop on its tail (a jam of cruisers) fills it too // 0.97: two cops hemming the runner in (alongside, ahead or behind) however fast it still goes
                if (rate > 0) rate *= BustRate[Difficulty];
                r.bust = Mathf.Clamp01(r.bust + rate * dt);
                if (r.bust >= 1) Caught(r);
                            }
            // escape meter (Easy 20 s, Normal 30, Hard 40): fills while no cop (or the helicopter) sees the runner and no cop is within
            // 150 m, seen or not; any cop seeing the runner resets it
            if (Mode == Variant.Getaway)
                foreach (var r in Runners)
                {
                    if (r.Done) continue;
                    if (r.seen) r.escape = 0;
                    else
                    {
                        float nearest = float.MaxValue; foreach (var c in Cops) if (c.car && !c.block) nearest = Mathf.Min(nearest, Vector3.Distance(c.car.Body.position, r.body.position));
                        if (nearest >= NearCops) r.escape = Mathf.Clamp01(r.escape + dt / EscapeBy[Difficulty]);
                    }
                    if (r.escape >= 1) Escaped(r, false);
                }
            if (Time.time >= nextTick) { nextTick = Time.time + .2f; Tick(); }
            // heat (0.97): +1 every HeatEvery seconds while being chased (Easy 45, Normal 30, Hard 20; max 5), and on Normal and Hard +1 for
            // ramming a cop, passing a roadblock and 10 s off-road while seen; it never drops during a chase
            if (Mode == Variant.Getaway && Runners.Any(r => !r.Done))
            {
                if (Runners.Any(r => !r.Done && r.copsChasing > 0)) heatClock += dt;
                if (heatClock >= HeatEvery[Difficulty]) BumpHeat("more units on the way");
                foreach (var r in Runners) if (!r.Done && r.offRoad && r.seen && !r.offBumped && Difficulty > 0) { r.offSeen += dt; if (r.offSeen >= 10) { r.offBumped = true; BumpHeat("the suspect is running cross-country"); } }
                if (Heat >= 4 && !Heli) SpawnHelicopter();
            }
            if (Mode == Variant.Getaway && Time.time >= nextBackup) { nextBackup = Time.time + 4; Backups(); }
            if (Mode == Variant.Getaway && Heat >= 3 && Time.time >= nextBlock && Blocks.Count < 2) { nextBlock = Time.time + 38; TryRoadblock(); }
            if (Mode == Variant.Getaway && Time.time >= nextPrune) { nextPrune = Time.time + 2; Prune(); }
            UpdateBlocks();
            // the end
            var humans = Runners.Where(r => r.human).ToList();
            bool anyEscapedFirst = humans.Any(r => r.escaped && !r.limit);
            if (State == Phase.Running)
            {
                if (humans.All(r => r.Done)) End();
                else if (humans.Count > 1 && anyEscapedFirst) End();
                else if (Clock >= Limit) { foreach (var r in Runners) if (!r.Done) Escaped(r, true); End(); }
            }
        }
        void End()
        {
            State = Phase.Over; phaseAt = Time.time;
            foreach (var r in Runners) if (!r.Done) Escaped(r, true);
            foreach (var c in Cops) { c.driver.Role = CopDriver.Task.Idle; }
            foreach (var r in Runners) if (r.human) Results.Add((SplitScreen.NameOf(r.player), r.freeSeconds, r.topHeat, r.outcome));
        }
        void Finish()
        {
            State = Phase.Done; foreach (var r in Runners) Hold(r.car, false);
            if (Mode == Variant.Getaway) RecordTop10();
            flow.CompleteResults();
        }
        public void Rematch() { flow.StartFreeRoam(); }
        void Caught(Runner r)
        {
            if (r.Done) return; r.caught = true; r.endedAt = Clock; r.freeSeconds = Clock; r.outcome = "CAUGHT"; Hold(r.car, true);
            Say(r.human ? $"{SplitScreen.NameOf(r.player)} is in custody" : "Suspect in custody");
            foreach (var c in Cops) if (c.assigned == r) c.assigned = null;
            CheckWatching();
        }
        // two players: the one still running has the whole screen
        void CheckWatching()
        {
            var humans = Runners.Where(x => x.human).ToList(); if (humans.Count < 2) return;
            var active = humans.Where(x => !x.Done).ToList();
            if (active.Count == 1) { SplitScreen.Watching = active[0].player; if (SplitScreen.Race) SplitScreen.Race.ApplyWatching(); }
        }
        void Escaped(Runner r, bool limit)
        {
            if (r.Done) return;
            if (!limit && !r.human) { r.escape = 0; Say("Unit lost the AI runner"); return; } // the AI player 2 never ends the round
            r.escaped = true; r.limit = limit; r.endedAt = Clock; r.freeSeconds = Clock; r.outcome = "ESCAPED";
            Say(limit ? "Time: the suspect is gone" : "We lost the suspect");
            if (!limit) Hold(r.car, true);
            CheckWatching();
        }

        // ---------- sight, roles, radio (5 times a second) ----------
        bool LineOfSight(Vector3 eye, Vector3 target, Transform ignore)
        {
            var dir = target - eye; float dist = dir.magnitude; if (dist < 1) return true;
            int n = Physics.RaycastNonAlloc(eye, dir / dist, rays, dist, ~0, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < n; i++)
            {
                var col = rays[i].collider; if (!col) continue;
                if (ignore && col.transform.IsChildOf(ignore)) continue;
                var rb = rays[i].rigidbody; if (rb && rb.GetComponent<ArcadeVehicle>()) continue; // cars never hide anyone
                if (col.GetComponentInParent<ArcadeVehicle>()) continue;
                return false;
            }
            return true;
        }
        void Tick()
        {
            // who sees whom
            foreach (var r in Runners)
            {
                if (r.Done) { r.seen = false; continue; }
                bool seen = false; int chasing = 0;
                foreach (var c in Cops)
                {
                    if (!c.car) continue; c.sees = false; float d = Vector3.Distance(c.car.Body.position, r.body.position);
                    if (d <= SightRange && LineOfSight(c.car.Body.position + Vector3.up * 1.5f, r.body.position + Vector3.up * 1.1f, r.car.transform)) { c.sees = true; seen = true; }
                    if (c.sees || (c.assigned == r && c.driver.Role != CopDriver.Task.Idle && d < 220)) chasing++;
                }
                r.heliSees = Heli && Heli.Sees(r, nextHeliCheck <= Time.time); if (r.heliSees) { seen = true; chasing++; }
                bool was = r.seen; r.seen = seen; r.copsChasing = chasing; r.wasSeen |= seen;
                if (seen) { r.lastSeenAt = Time.time; r.lastSeenPos = r.body.position; r.lastSeenVelocity = r.body.linearVelocity; r.unseenFor = 0; if (!was && Time.time - r.seenAt > 4 && r.seenAt > 0 && r.human) Say("Visual on the suspect"); r.seenAt = Time.time; }
                else
                {
                    r.unseenFor += .2f; if (was && r.human) Say("Lost visual. Last seen near " + Net.PlaceName(race, r.lastSeenPos));
                    // 0.97: breaking sight is a call-in too: within about 2 s units are posted ahead of the runner's heading
                    if (Mode == Variant.Getaway && r.human && r.wasSeen && r.unseenFor >= 1.6f && Time.time - r.lastCallIn > 14) { ComputeExits(r); CallIn(r); }
                }
                // on / off the road
                float off = Net.OffNet(r.body.position);
                if (!r.offRoad && off > 14) { if (r.offSince < 0) r.offSince = Time.time; if (Time.time - r.offSince > 1.2f) GoOffRoad(r); }
                else if (!r.offRoad) r.offSince = -1;
                if (r.offRoad && off < 9) { if (r.onSince <= 0) r.onSince = Time.time; if (Time.time - r.onSince > 1f) BackOnRoad(r); }
                else if (r.offRoad) r.onSince = 0;
                if (!r.offRoad) { int node = Net.Nearest(r.body.position, out float d2, 60); if (node >= 0) { r.offFrom = Net.P[node]; r.offFromNode = node; } }
                // dodges and roadblock passes
                foreach (var c in Cops) { if (!c.car) continue; float d = Vector3.Distance(c.car.Body.position, r.body.position); if (d < 10) c.wasClose = true; else if (d > 26 && c.wasClose) { c.wasClose = false; r.dodged++; } }
            }
            AssignRoles();
            // sirens: the two nearest cops to a runner are loud; the rest flash silently
            foreach (var c in Cops)
            {
                if (!c.car) continue; var r = c.assigned ?? Runners.FirstOrDefault(x => !x.Done); c.lights.Target = r != null ? r.car.transform : null;
                c.lights.Silent = r != null && Cops.Count(o => o != c && o.car && r != null && Vector3.Distance(o.car.Body.position, r.body.position) < Vector3.Distance(c.car.Body.position, r.body.position)) >= 2;
            }
            foreach (var r in Runners) r.topHeat = Mathf.Max(r.topHeat, Heat);
            if (nextHeliCheck <= Time.time) nextHeliCheck = Time.time + .6f;
            Boosts();
        }
        float nextHeliCheck;
        // catch-up: a cop more than 150 m behind its runner gains speed (up to 30 %) until it is within 80 m
        void Boosts()
        {
            foreach (var c in Cops)
            {
                if (!c.car || c.block) continue; c.boost = 1; var r = c.assigned; if (r == null || r.Done) { c.driver.Boost = 1; continue; }
                float d = Vector3.Distance(c.car.Body.position, r.body.position);
                if (d > 150) c.catching = true; else if (d < 80) c.catching = false;
                if (c.catching) c.boost = 1 + .3f * Mathf.Clamp01((d - 80) / 220f);
                c.driver.Boost = c.boost;
            }
        }
        void GoOffRoad(Runner r)
        {
            r.offRoad = true; r.onSince = 0; r.offSeen = 0; r.offBumped = false;
            if (r.seen || Time.time - r.lastSeenAt < 6)
            {
                var ahead = r.body.position + (r.body.linearVelocity.sqrMagnitude > 4 ? r.body.linearVelocity.normalized : r.car.transform.forward) * 260;
                Say("Suspect off-road heading toward " + Net.PlaceName(race, ahead), r.player);
            }
            ComputeExits(r); if (Mode == Variant.Getaway && r.human) CallIn(r);
        }
        void BackOnRoad(Runner r) { r.offRoad = false; r.offSince = -1; ExitPoints.Clear(); foreach (var c in Cops) c.exitUnit = false; if (r.seen || Time.time - r.lastSeenAt < 8) Say("Suspect back on the road near " + Net.PlaceName(race, r.body.position), r.player); }
        void Say(string line, int player = 0) { if (RadioLog.Count > 0 && RadioLog[RadioLog.Count - 1] == line && Time.unscaledTime - radioAt < 8) return; RadioLog.Add(line); if (RadioLog.Count > 30) RadioLog.RemoveAt(0); radioAt = Time.unscaledTime; }

        readonly List<int> exitNodes = new(); readonly Dictionary<Cop, int> exitOf = new();
        // the exits: road points ahead of the runner's heading (road ends and junctions first), nearest first, up to four, 90 m apart
        void ComputeExits(Runner r)
        {
            exitNodes.Clear(); ExitPoints.Clear(); var pos = r.body.position; var heading = r.body.linearVelocity; heading.y = 0; bool moving = heading.sqrMagnitude > 9; if (moving) heading.Normalize();
            var cands = new List<(float score, int node)>(); int here = Net.Nearest(pos, out _, 900); if (here < 0) return; int comp = Net.Component[here];
            foreach (int i in Net.Near(pos, 900))
            {
                if (Net.Component[i] != comp) continue;
                var d = Net.P[i] - pos; d.y = 0; float dist = d.magnitude; if (dist < 120) continue;
                float cone = moving ? Vector3.Angle(heading, d) : 0; if (cone > 80) continue;
                float junction = Net.Adj[i].Count >= 3 ? -60f : 0;
                cands.Add((dist + cone * 2.5f + junction, i));
            }
            foreach (var cand in cands.OrderBy(c => c.score))
            {
                if (exitNodes.Count >= 4) break;
                if (exitNodes.Any(n => Vector3.Distance(Net.P[n], Net.P[cand.node]) < 90)) continue;
                exitNodes.Add(cand.node);
            }
            foreach (int n in exitNodes) ExitPoints.Add(Net.P[n]);
            exitOf.Clear();
        }
        // 0.97 call-in: units are PLACED (out of sight, lights on) at the nearest two to four exits, so something always happens near
        // the runner, wherever the existing cops are. Two at heat 1-2, three from heat 3, four at heat 5.
        void CallIn(Runner r)
        {
            r.lastCallIn = Time.time; if (exitNodes.Count == 0) { Trace($"{Clock:F0}s call-in: no exits ahead"); return; }
            int want = Mathf.Min(exitNodes.Count, Mathf.Clamp(2 + (Heat >= 3 ? 1 : 0) + (Heat >= 5 ? 1 : 0), 2, 4)); int placed = 0; var names = new List<string>();
            foreach (int node in exitNodes.OrderBy(n => Vector3.Distance(Net.P[n], r.body.position)))
            {
                if (placed >= want) break;
                if (Cops.Count(c => c.car && !c.block) >= MaxCops + 4) break;
                if (Cops.Any(c => c.car && !c.block && Vector3.Distance(c.car.Body.position, Net.P[node]) < 45)) { placed++; continue; } // a unit is already there
                int at = node; if (Seen(Net.P[at])) { at = HiddenNear(node); if (at < 0) continue; }
                var face = r.body.position - Net.P[at]; face.y = 0; var t = Net.Tangent(at); if (Vector3.Dot(t, face) < 0) t = -t;
                // parked on the verge, not in the lane: the runner can pass it (and is then chased), it does not make a wall of its own
                var edge = Vector3.Cross(Vector3.up, t).normalized * Mathf.Max(0, Net.Half[at] - 1.6f) * (Random.value < .5f ? -1 : 1);
                var cop = SpawnCop(Net.P[at] + edge, t); if (cop == null) continue;
                cop.exitUnit = true; cop.driver.Frozen = false; cop.driver.Role = CopDriver.Task.Exit; cop.driver.Hold = true; cop.lights.Siren = true; cop.lights.Silent = false; placed++; names.Add(Net.PlaceName(race, Net.P[at]));
            }
            Trace($"{Clock:F0}s call-in: exits {exitNodes.Count}, wanted {want}, placed {names.Count}, already covered {placed - names.Count}, cops now {Cops.Count(c => c.car && !c.block)}");
            if (names.Count > 0 && r.human) Say($"{names.Count} unit{(names.Count == 1 ? "" : "s")} posted at the exits near {string.Join(" and ", names.Distinct())}", r.player);
            else if (placed > 0 && r.human) Say($"{placed} unit{(placed == 1 ? "" : "s")} covering the exits", r.player);
        }
        float nextExitRefresh;
        void AssignRoles()
        {
            // each cop works for the runner nearest to it
            foreach (var c in Cops) { if (!c.car || c.block) continue; Runner best = null; float bd = float.MaxValue; foreach (var r in Runners) { if (r.Done) continue; float d = Vector3.Distance(c.car.Body.position, r.body.position); if (d < bd) { bd = d; best = r; } } c.assigned = best; if (best == null) { c.driver.Role = CopDriver.Task.Idle; c.driver.Stop(); } }
            foreach (var r in Runners)
            {
                if (r.Done) continue;
                var mine = Cops.Where(c => c.car && !c.block && c.assigned == r).OrderBy(c => Vector3.Distance(c.car.Body.position, r.body.position)).ToList(); if (mine.Count == 0) continue;
                bool known = r.seen || Time.time - r.lastSeenAt < 3;
                if (r.offRoad && (r.seen || Time.time - r.lastSeenAt < 40 || !r.human)) OffRoadRoles(r, mine);
                else if (known) ChaseRoles(r, mine);
                else SearchRoles(r, mine);
            }
        }
        void Plan(Cop c, int node, CopDriver.Task role, bool hold, float every = 1.2f)
        {
            c.driver.Role = role; c.driver.Hold = hold;
            if (node < 0) return;
            bool force = c.driver.GoalNode < 0 || c.driver.Arrived && !hold;
            bool far = c.driver.GoalNode < 0 || Vector3.Distance(Net.P[node], Net.P[c.driver.GoalNode]) > 30;
            if (Time.time < c.nextPlan && !force && !far) return;
            c.nextPlan = Time.time + every + Random.value * .4f; if (far || force || Time.time >= c.nextPlan - every) c.driver.GoTo(node);
        }
        void ChaseRoles(Runner r, List<Cop> mine)
        {
            var heading = r.body.linearVelocity; heading.y = 0; if (heading.sqrMagnitude < 4) heading = r.car.transform.forward;
            int runnerNode = Net.Nearest(r.body.position + heading.normalized * Mathf.Min(12, heading.magnitude * .5f), out _, 120);
            for (int i = 0; i < mine.Count; i++)
            {
                var c = mine[i]; c.driver.Chasing = null; c.driver.ChasingBody = null; c.driver.Pushy = i < 2; c.driver.Catchup = 1;
                float d = Vector3.Distance(c.car.Body.position, r.body.position);
                if (i == 0 || (i == 1 && d < 160))
                {
                    c.driver.Chasing = r.car.transform; c.driver.ChasingBody = r.body; c.driver.Offset = i == 0 ? 0 : (Vector3.Dot(Vector3.Cross(Vector3.up, heading.normalized), c.car.Body.position - r.body.position) > 0 ? 3.4f : -3.4f);
                    c.driver.Catchup = 1;
                    Plan(c, runnerNode, i == 0 ? CopDriver.Task.Chase : CopDriver.Task.Flank, false, .8f);
                }
                else
                {
                    // cut it off: a point ahead along its road; further for each cop
                    float ahead = Mathf.Clamp(heading.magnitude * 9 + 60 * (i - 1), 160, 520);
                    int cut = runnerNode >= 0 ? Net.Ahead(runnerNode, heading, ahead) : -1; c.driver.Pushy = false;
                    Plan(c, cut, CopDriver.Task.Cutoff, true, 2.5f);
                }
            }
        }
        void SearchRoles(Runner r, List<Cop> mine)
        {
            int last = Net.Nearest(r.lastSeenPos == Vector3.zero ? r.body.position : r.lastSeenPos, out _, 600);
            float radius = Mathf.Clamp(110 + 22 * r.unseenFor, 110, 700);
            for (int i = 0; i < mine.Count; i++)
            {
                var c = mine[i]; c.driver.Chasing = null; c.driver.Pushy = false; c.driver.Catchup = 1;
                if (i <= 1 && r.unseenFor < 45 && last >= 0)
                {
                    // just lost it: it keeps going the way it was heading, so the first two drive on along that road, hard
                    var v = r.lastSeenVelocity; float ahead = Mathf.Clamp(v.magnitude * (r.unseenFor + 4) * (i == 0 ? .8f : 1.2f), 40, 900);
                    int guess = Net.Ahead(last, v, ahead); if (i == 0 && !r.offRoad && r.unseenFor < 45) { int on = Net.Nearest(r.body.position, out _, 300, 40); if (on >= 0) guess = on; }
                    // dispatch keeps the lead unit on the road the runner is on
                    c.driver.Catchup = 1; Plan(c, guess >= 0 ? guess : last, CopDriver.Task.Search, false, 1.5f); continue;
                }
                if (i == 0) { Plan(c, last, CopDriver.Task.Search, false, 2f); continue; }
                if (Time.time >= c.searchUntil || c.driver.Arrived || c.driver.GoalNode < 0)
                {
                    var around = last >= 0 ? Net.Around(last, radius, 70) : null;
                    if (around != null && around.Count > 0) { int pick = around[Random.Range(0, around.Count)]; c.driver.Role = CopDriver.Task.Search; c.driver.Hold = false; c.driver.GoTo(pick); }
                    c.searchUntil = Time.time + 14 + Random.value * 6;
                }
                c.driver.Role = CopDriver.Task.Search;
            }
        }
        void OffRoadRoles(Runner r, List<Cop> mine)
        {
            if (Time.time >= nextExitRefresh) { nextExitRefresh = Time.time + 4; ComputeExits(r); if (Mode == Variant.Getaway && r.human && Time.time - r.lastCallIn > 10) CallIn(r); }
            // the nearest cop stops at the edge, lights on, radioing; the others take the exits nearest first, one each
            var free = new List<Cop>(mine); var edgeCandidates = free.Where(c => !c.exitUnit).ToList(); var edge = edgeCandidates.Count > 0 ? edgeCandidates[0] : free[0]; free.Remove(edge); edge.driver.Chasing = null; edge.driver.Pushy = false;
            Plan(edge, r.offFromNode, CopDriver.Task.Edge, true, 2f);
            var taken = new HashSet<Cop>();
            foreach (int node in exitNodes.OrderBy(n => Vector3.Distance(Net.P[n], r.body.position)))
            {
                Cop best = null; float bd = float.MaxValue;
                foreach (var c in free) { if (taken.Contains(c)) continue; float d = Vector3.Distance(c.car.Body.position, Net.P[node]); if (d < bd) { bd = d; best = c; } }
                if (best == null) break; taken.Add(best); best.driver.Chasing = null; best.driver.Pushy = false; best.driver.Catchup = 1; Plan(best, node, CopDriver.Task.Exit, true, 3f);
            }
            foreach (var c in free) if (!taken.Contains(c)) { c.driver.Chasing = null; SearchAround(c, r); }
        }
        void SearchAround(Cop c, Runner r)
        {
            if (Time.time < c.searchUntil && !c.driver.Arrived && c.driver.GoalNode >= 0) return;
            int centre = Net.Nearest(r.lastSeenPos == Vector3.zero ? r.body.position : r.lastSeenPos, out _, 600); var around = centre >= 0 ? Net.Around(centre, 400, 70) : null;
            if (around != null && around.Count > 0) { c.driver.Role = CopDriver.Task.Search; c.driver.Hold = false; c.driver.GoTo(around[Random.Range(0, around.Count)]); }
            c.searchUntil = Time.time + 12;
        }

        // ---------- backup and roadblocks ----------
        // 0.97 backups: heat sets how many cops are NEAR the runner (within 600 m): heat 1 = 2, 2 = 3 ... 5 = 6. A new unit is placed out of
        // sight on the road network 250-450 m from the runner, ahead of its heading or on the roads that join its route, never behind,
        // and joins with lights and siren and a radio line. A cop more than 600 m behind that cannot see the runner is placed ahead again.
        int NearCount(Runner r) => Cops.Count(c => c.car && !c.block && Vector3.Distance(c.car.Body.position, r.body.position) <= 600);
        void Backups()
        {
            Runner r = null; int fewest = int.MaxValue;
            foreach (var x in Runners) { if (x.Done || !x.human) continue; int n = NearCount(x); if (n < fewest) { fewest = n; r = x; } } // the runner who needs them most
            if (r == null) return;
            int want = Mathf.Min(MaxCops, Heat + 1);
            if (fewest < want && Cops.Count(c => c.car && !c.block) < MaxCops + 4)
            {
                int node = PlaceAhead(r); if (node < 0) return;
                var cop = SpawnCop(Net.P[node], FaceRunner(node, r)); if (cop != null) { cop.driver.Frozen = false; cop.catching = false; Say($"Unit {cop.id} joining from {Net.RoadName(node)}", r.player); }
                return;
            }
            // withdraw and re-place a stray
            foreach (var c in Cops)
            {
                if (!c.car || c.block || c.exitUnit || c.driver.Role == CopDriver.Task.Exit || c.sees) { if (c.sees) c.farSince = -1; continue; }
                var cr = c.assigned ?? r; if (cr == null) continue; var heading = cr.body.linearVelocity; heading.y = 0; if (heading.sqrMagnitude < 4) heading = cr.car.transform.forward;
                var to = c.car.Body.position - cr.body.position; to.y = 0;
                bool behind = Vector3.Dot(to, heading) < 0 && to.magnitude > 600;
                if (!behind) { c.farSince = -1; continue; }
                if (c.farSince < 0) c.farSince = Time.time; if (Time.time - c.farSince < 3) continue;
                if (Seen(c.car.Body.position)) continue;
                int node = PlaceAhead(cr); if (node < 0) continue;
                c.driver.Place(Net.P[node], FaceRunner(node, cr)); c.driver.Stop(); c.farSince = -1; c.catching = false; Say($"Unit {c.id} rejoining from {Net.RoadName(node)}", cr.player);
                break;
            }
        }
        Vector3 FaceRunner(int node, Runner r) { var face = r.body.position - Net.P[node]; face.y = 0; var t = Net.Tangent(node); return Vector3.Dot(t, face) < 0 ? -t : t; }
        // a node 250-450 m from the runner, out of every view, ahead of its heading (never behind), preferring its own route and the roads joining it
        public int PlaceAhead(Runner r)
        {
            var heading = r.body.linearVelocity; heading.y = 0; if (heading.sqrMagnitude < 9) heading = r.car.transform.forward; heading.y = 0; heading.Normalize();
            int centre = r.offRoad && r.offFromNode >= 0 ? r.offFromNode : Net.Nearest(r.body.position, out _, 400); if (centre < 0) return -1;
            int route = Net.Ahead(centre, heading, 380);
            var best = new List<(float score, int node)>();
            foreach (int i in Net.Near(r.body.position, 450))
            {
                if (Net.Component[i] != Net.Component[centre]) continue; var d = Net.P[i] - r.body.position; d.y = 0; float dist = d.magnitude; if (dist < 250) continue;
                float ahead = Vector3.Dot(d / dist, heading); if (ahead < -.05f) continue; // never behind
                if (Seen(Net.P[i]) && dist < 380) continue; // out of sight, or too far to read as more than a speck
                if (Cops.Any(c => c.car && Vector3.Distance(c.car.Body.position, Net.P[i]) < 70)) continue;
                float score = ahead * 60 + Mathf.Max(0, 120 - Vector3.Distance(Net.P[i], Net.P[route])) * .6f - Mathf.Abs(dist - 350) * .15f;
                best.Add((score, i));
            }
            if (best.Count == 0) return -1;
            var top = best.OrderByDescending(x => x.score).Take(5).ToList(); return top[Random.Range(0, top.Count)].node;
        }
        // too many units (exit units that are no longer needed): the farthest one out of sight goes
        void Prune()
        {
            var live = Cops.Where(c => c.car && !c.block).ToList(); if (live.Count <= MaxCops + 2) return;
            var r = Runners.FirstOrDefault(x => !x.Done && x.human); if (r == null) return;
            foreach (var c in live.OrderByDescending(c => Vector3.Distance(c.car.Body.position, r.body.position)))
            {
                if (c.sees || Seen(c.car.Body.position) || Vector3.Distance(c.car.Body.position, r.body.position) < 200) continue;
                Cops.Remove(c); c.car.gameObject.SetActive(false); Destroy(c.car.gameObject); break;
            }
        }
        public void BumpHeat(string why)
        {
            heatClock = 0; if (Mode != Variant.Getaway || Heat >= 5) return;
            Heat++; foreach (var r in Runners) r.topHeat = Mathf.Max(r.topHeat, Heat);
            Say($"Heat {Heat}: {why}" + (Heat >= 3 ? ", roadblocks" : "") + (Heat == 4 ? ", air support" : ""));
        }
        void SpawnHelicopter()
        {
            var r = Runners.FirstOrDefault(x => !x.Done && x.human); if (r == null) return;
            var go = new GameObject("Police helicopter"); Heli = go.AddComponent<PoliceHelicopter>(); Heli.Begin(this, r);
            Say("Air support is on the way");
        }
        // 0.97: is this point in sight of anyone's screen (the main camera, or either split-screen view): inside the frustum within 450 m with a clear line
        // of sight (hills, buildings and trees hide it; cars do not)
        bool Seen(Vector3 p)
        {
            var cams = new List<Camera>(); if (Camera.main) cams.Add(Camera.main); foreach (var c in SplitScreen.Views) if (c && !cams.Contains(c)) cams.Add(c);
            foreach (var cam in cams)
            {
                var v = cam.WorldToViewportPoint(p); if (v.z <= 0 || v.z > 450 || v.x < -.1f || v.x > 1.1f || v.y < -.1f || v.y > 1.1f) continue;
                if (LineOfSight(cam.transform.position, p + Vector3.up * 1.2f, null)) return true;
            }
            return false;
        }
        // the nearest road point within 90 m of this one that nobody can see (-1 when there is none)
        int HiddenNear(int node)
        {
            int best = -1; float bd = float.MaxValue;
            foreach (int i in Net.Near(Net.P[node], 90)) { if (Net.Component[i] != Net.Component[node]) continue; float d = Vector3.Distance(Net.P[i], Net.P[node]); if (d < bd && !Seen(Net.P[i])) { bd = d; best = i; } }
            return best;
        }
        void TryRoadblock()
        {
            var r = Runners.FirstOrDefault(x => !x.Done && x.human && !x.offRoad); if (r == null) return;
            var heading = r.body.linearVelocity; heading.y = 0; if (heading.magnitude < 6) return; heading.Normalize();
            int here = Net.Nearest(r.body.position, out float off, 80); if (here < 0 || off > 20) return;
            float distance = Mathf.Clamp(r.body.linearVelocity.magnitude * 11f, 240, 420);
            int node = Net.Ahead(here, heading, distance); if (node < 0 || node == here) return;
            if (Vector3.Distance(Net.P[node], r.body.position) < 180) return;
            if (Seen(Net.P[node])) { nextBlock = Time.time + 4; return; }
            var tangent = Net.Tangent(node); var right = Vector3.Cross(Vector3.up, tangent).normalized; float half = Net.Half[node];
            float gap = 2.7f, centreLane = Random.Range(-Mathf.Max(0, half - gap - .8f), Mathf.Max(0, half - gap - .8f));
            var block = new Roadblock { centre = Net.P[node], tangent = tangent, born = Time.time, target = r, node = node };
            for (int side = -1; side <= 1; side += 2)
            {
                float lateral = centreLane + side * (gap + 2.3f); var at = Net.P[node] + right * lateral;
                var facing = Quaternion.LookRotation(Quaternion.Euler(0, 90 + Random.Range(-14f, 14f), 0) * tangent);
                var cop = SpawnCop(at, facing * Vector3.forward); if (cop == null) continue;
                cop.block = true; cop.driver.Role = CopDriver.Task.Block; cop.driver.Frozen = true; cop.car.Body.isKinematic = true; cop.lights.Siren = true; cop.lights.Silent = true;
                if (side < 0) block.a = cop; else block.b = cop;
            }
            Blocks.Add(block); Say("Roadblock set up on " + Net.RoadName(node) + ", ahead of the suspect");
        }
        void UpdateBlocks()
        {
            for (int i = Blocks.Count - 1; i >= 0; i--)
            {
                var b = Blocks[i]; var r = b.target; bool remove = Time.time - b.born > 80 || r == null || r.Done;
                // a roadblock is lifted once a regular unit gets to it with the runner well past it (the unit would only queue behind its own block)
                if (!remove && r != null && Vector3.Distance(r.body.position, b.centre) > 80) foreach (var c in Cops) if (c.car && !c.block && Vector3.Distance(c.car.Body.position, b.centre) < 45) { remove = true; break; }
                if (r != null && !r.Done)
                {
                    float d = Vector3.Distance(r.body.position, b.centre); b.closest = Mathf.Min(b.closest, d);
                    var along = Vector3.Dot(r.body.position - b.centre, r.body.linearVelocity.normalized);
                    if (b.closest < 22 && d > 45 && !b.passed && r.bust < .9f) { b.passed = true; r.blocks++; Say("The suspect got through the roadblock"); if (Difficulty > 0) BumpHeat("the suspect ran the roadblock"); }
                    if (b.passed && d > 120) remove = true;
                    if (!remove && d > 420 && Time.time - b.born > 25) remove = true;
                }
                if (!remove) continue;
                foreach (var c in new[] { b.a, b.b }) if (c != null) { Cops.Remove(c); if (c.car) { c.car.gameObject.SetActive(false); Destroy(c.car.gameObject); } }
                Blocks.RemoveAt(i);
            }
        }

        // ---------- results, Top 10 ----------
        [System.Serializable] public sealed class Entry { public string name; public float seconds; public int heat, minutes; public string outcome, date; }
        [System.Serializable] sealed class Board { public List<Entry> entries = new(); }
        public readonly List<int> Places = new();
        public static string BoardPath => Hints.Flow && Hints.Flow.Save != null ? Path.Combine(Hints.Flow.Save.DirectoryPath, "getaway-v1.json") : null;
        static Board LoadBoard() { try { var p = BoardPath; if (p != null && File.Exists(p)) return JsonUtility.FromJson<Board>(File.ReadAllText(p)) ?? new Board(); } catch { } return new Board(); }
        static void SaveBoard(Board b) { try { var p = BoardPath; if (p != null) AtomicSave.Write(p, JsonUtility.ToJson(b, true)); } catch (System.Exception e) { Debug.LogWarning("Getaway Top 10 not saved: " + e.Message); } }
        static IEnumerable<Entry> Sorted(Board b, int minutes) => b.entries.Where(x => x.minutes == minutes).OrderByDescending(x => x.seconds).ThenByDescending(x => x.heat);
        public static List<Entry> Top(int minutes) => Sorted(LoadBoard(), minutes).Take(10).ToList();
        void RecordTop10()
        {
            Places.Clear(); var board = LoadBoard();
            foreach (var r in Runners.Where(x => x.human))
            {
                var e = new Entry { name = SplitScreen.NameOf(r.player), seconds = Mathf.Round(r.freeSeconds * 10) / 10f, heat = r.topHeat, minutes = SplitScreen.PoliceMinutes, outcome = r.outcome, date = System.DateTime.Now.ToString("yyyy-MM-dd") };
                board.entries.Add(e); var top = Sorted(board, e.minutes).Take(10).ToList(); Places.Add(top.IndexOf(e) + 1);
                board.entries = board.entries.Where(x => x.minutes != e.minutes || top.Contains(x)).ToList();
            }
            SaveBoard(board);
        }
        // two players: the winner (0 = a draw / one player)
        public int Winner
        {
            get
            {
                var humans = Runners.Where(r => r.human).ToList(); if (humans.Count < 2) return 0;
                var esc = humans.Where(r => r.escaped && !r.limit).OrderBy(r => r.endedAt).FirstOrDefault(); if (esc != null) return esc.player;
                var a = humans[0]; var b = humans[1];
                if (a.caught != b.caught) return a.caught ? b.player : a.player;
                if (Mathf.Approximately(a.freeSeconds, b.freeSeconds)) return 0;
                return a.freeSeconds > b.freeSeconds ? a.player : b.player;
            }
        }
        public string Verdict(Runner r) => r.caught ? "CAUGHT after " + Mss(r.freeSeconds) : r.limit ? "ESCAPED (time up)" : "ESCAPED after " + Mss(r.freeSeconds);

        // ---------- what the HUD and the minimap show ----------
        public IEnumerable<(Vector3 position, bool sees)> CopMarks => Cops.Where(c => c.car).Select(c => (c.car.Body.position, c.sees));
        public Vector3? HeliMark => Heli ? Heli.transform.position : null;
        // F3 debug mode, Getaway only: heat, the cops and what each is doing
        public string DebugText
        {
            get
            {
                var sb = new System.Text.StringBuilder(); var r = Runners.FirstOrDefault(x => x.human) ?? Runners.FirstOrDefault(); if (r == null) return "";
                sb.AppendLine($"GETAWAY t {Clock:F0}s heat {Heat} cops {Cops.Count(c => !c.block)} (near {NearCount(r)}) seen {r.seen} heli {(Heli ? (r.heliSees ? "SEES" : "up") : "-")} escape {r.escape:F2} bust {r.bust:F2} off {r.offRoad}");
                foreach (var c in Cops) { if (!c.car) continue; float d = Vector3.Distance(c.car.Body.position, r.body.position); sb.AppendLine($" #{c.id} {c.driver.Role}{(c.exitUnit ? "(exit)" : "")}{(c.block ? "(block)" : "")} {d:F0}m {c.car.Body.linearVelocity.magnitude:F0}m/s x{c.boost:F2}{(c.sees ? " sees" : "")}"); }
                return sb.ToString();
            }
        }
        public string HeatBars => new string('★', Heat) + new string('☆', 5 - Heat);
    }
}
