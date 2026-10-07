using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Racer
{
    // 0.94 Part C: Police Chase, round 1: cop vs runner, in split-screen Free Roam (SplitRoam). Two rounds, the roles swapped
    // for the second: the runner drives their own chosen vehicle, the cop the patrol car (VehicleProfile.Police). The runner
    // starts on the road; the cop 80 m behind, released 3 s after the runner. The bust meter fills while the cop is within
    // 10 m of the runner and the runner is slower than 7 m/s (3 s to fill) and drains at the same rate otherwise; full =
    // caught. At the round limit the runner got away. A runner's reset holds them still for 2 s. The longer run as the runner
    // wins; getting away beats being caught; both away is a draw. With player 2 as the AI it is the runner (the chasing AI is
    // a later round), so the match is that one round. Nothing is recorded.
    public sealed class PoliceChase : MonoBehaviour
    {
        public const float CatchDistance = 10, CatchSpeed = 7, FillSeconds = 3, CopDelay = 3, CopBehind = 80, ResetHold = 2, EndPause = 4;
        public static PoliceChase Current { get; private set; }
        public enum Phase { Starting, Running, Caught, Away, Done }
        public sealed class Run { public int runner; public bool caught; public float seconds; public string runnerVehicle; }
        public readonly List<Run> Runs = new();
        public int Round { get; private set; }      // 0, 1
        public Phase State { get; private set; }
        public float Meter { get; private set; }    // 0..1
        public float Clock { get; private set; }    // seconds since the runner was released
        public float Limit => SplitScreen.PoliceMinutes * 60;
        public int Rounds => SplitScreen.P2Ai ? 1 : 2;
        public int Cop => Round == 0 ? SplitScreen.CopFirst : 3 - SplitScreen.CopFirst;
        public int Runner => 3 - Cop;
        public bool Siren { get; private set; }
        RaceFlow flow; RaceDirector race; float phaseAt, holdUntil; PoliceLights lights; int resets;
        readonly HashSet<ArcadeVehicle> held = new();

        public void Initialize(RaceFlow owner) { flow = owner; race = owner.Race; Current = this; Runs.Clear(); Round = 0; }
        void OnDestroy() { if (Current == this) Current = null; }
        public ArcadeVehicle CarOf(int player) => player == 1 ? race.vehicle : SplitRoam.Current ? SplitRoam.Current.Car : null;
        public string VehicleFor(int player) => player == Cop ? VehicleProfile.Police.Id : player == 1 ? SplitScreen.P1Vehicle : SplitScreen.P2Vehicle;
        public bool Holds(ArcadeVehicle car) => held.Contains(car);
        public string Role(int player) => player == Cop ? "COP" : "RUNNER";

        // RaceDirector.RestartRace (via SplitRace): player 1's vehicle for this round; player 2's is made by SplitRoam
        public void ApplyPlayerOne()
        {
            var c = race.vehicle.GetComponent<VehicleConfiguration>(); c.Apply(VehicleFor(1));
            if (Cop == 1) c.SetPaint(new Color(.03f, .03f, .035f)); else c.SetBodyColor(SplitScreen.P1Color);
        }
        // after both vehicles exist: the runner on the road at the start, the cop 80 m behind; both held until GO
        public void BeginRound()
        {
            var runner = CarOf(Runner); var cop = CarOf(Cop); if (!runner || !cop) return;
            var road = race.ambientRoad ? race.ambientRoad : race.road;
            float s = road.Project(race.vehicle.Body.position, out _); var p = road.At(s, out var f);
            float sCop = road.openHighway ? Mathf.Max(0, s - CopBehind) : Mathf.Repeat(s - CopBehind, road.Length); var pc = road.At(sCop, out var fc);
            var right = Vector3.Cross(Vector3.up, Vector3.ProjectOnPlane(f, Vector3.up)).normalized; var rightC = Vector3.Cross(Vector3.up, Vector3.ProjectOnPlane(fc, Vector3.up)).normalized;
            Put(runner, p + right * 2.2f, Quaternion.LookRotation(Vector3.ProjectOnPlane(f, Vector3.up)));
            Put(cop, pc + rightC * 2.2f, Quaternion.LookRotation(Vector3.ProjectOnPlane(fc, Vector3.up)));
            Meter = 0; Clock = 0; State = Phase.Starting; phaseAt = Time.time; holdUntil = 0; Siren = false;
            // the AI runner drives away along the road (the cop is behind it)
            if (SplitScreen.P2Ai && Runner == 2 && SplitRoam.Current) SplitRoam.Current.Cruise(1.25f, 1);
            Hold(runner, true); Hold(cop, true);
            lights = cop.GetComponent<PoliceLights>(); if (!lights) lights = cop.gameObject.AddComponent<PoliceLights>(); lights.Target = runner.transform; lights.Siren = false; lights.Rebuild();
            var other = CarOf(Runner).GetComponent<PoliceLights>(); if (other && other != lights) Destroy(other);
            runner.GetComponent<VehicleRespawn>().Respawned -= RunnerReset; runner.GetComponent<VehicleRespawn>().Respawned += RunnerReset; resets = 0;
        }
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
        // a runner's reset holds them still for 2 s (no cheap escapes)
        void RunnerReset() { if (State != Phase.Running) return; resets++; holdUntil = Time.time + ResetHold; Hold(CarOf(Runner), true); }
        public int RunnerResets => resets;
        void Update()
        {
            if (!flow || flow.State != RaceFlow.Stage.Racing) return;
            var runner = CarOf(Runner); var cop = CarOf(Cop); if (!runner || !cop) return;
            // held vehicles stay held (starting Free Roam unlocks player 1's vehicle after the round was set up)
            foreach (var car in held) if (car && (!car.Body.isKinematic || car.enabled)) { car.Body.isKinematic = true; car.enabled = false; var input = car.GetComponent<VehicleInput>(); if (input) input.enabled = false; }
            float t = Time.time - phaseAt;
            switch (State)
            {
                case Phase.Starting:
                    if (t >= 3 && held.Contains(runner) && holdUntil == 0) { Hold(runner, false); holdUntil = -1; } // GO for the runner
                    if (t >= 3 + CopDelay) { Hold(cop, false); State = Phase.Running; Siren = true; holdUntil = 0; }
                    if (t >= 3) Clock = t - 3;
                    break;
                case Phase.Running:
                    Clock += Time.deltaTime;
                    if (holdUntil > 0 && Time.time >= holdUntil) { holdUntil = 0; Hold(runner, false); }
                    bool close = Vector3.Distance(cop.Body.position, runner.Body.position) < CatchDistance && runner.Body.linearVelocity.magnitude < CatchSpeed;
                    Meter = Mathf.Clamp01(Meter + (close ? 1 : -1) * Time.deltaTime / FillSeconds);
                    if (Toggle()) Siren = !Siren;
                    if (Meter >= 1) End(true); else if (Clock >= Limit) { Clock = Limit; End(false); }
                    break;
                case Phase.Caught: case Phase.Away:
                    if (t >= EndPause) { if (Round + 1 < Rounds) NextRound(); else Finish(); }
                    break;
            }
            if (lights) lights.Siren = Siren && (State == Phase.Running || State == Phase.Caught);
            // the AI runner: when the cop gets round in front of it (within 120 m ahead), it turns back the other way
            if (SplitScreen.P2Ai && Runner == 2 && State == Phase.Running && SplitRoam.Current?.Cruiser is RoadDriver ai && Time.time > nextTurn)
            {
                var ahead = Vector3.Dot(cop.Body.position - runner.Body.position, runner.transform.forward);
                if (ahead > 0 && ahead < 120 && Vector3.Distance(cop.Body.position, runner.Body.position) > 25) { nextTurn = Time.time + 12; SplitRoam.Current.Cruise(1.25f, -ai.Direction, false); }
            }
        }
        float nextTurn;
        bool Toggle()
        {
            var device = Cop == 1 ? SplitScreen.P1Device : SplitScreen.P2Device;
            if (device is Gamepad g) return g.rightShoulder.wasPressedThisFrame;
            if (device is Keyboard k) return k.hKey.wasPressedThisFrame;
            return false;
        }
        void End(bool caught)
        {
            Runs.Add(new Run { runner = Runner, caught = caught, seconds = Clock, runnerVehicle = VehicleFor(Runner) });
            State = caught ? Phase.Caught : Phase.Away; phaseAt = Time.time; Hold(CarOf(Runner), caught); Hold(CarOf(Cop), caught);
        }
        void NextRound() { Round++; flow.StartFreeRoam(); }
        void Finish() { State = Phase.Done; Hold(CarOf(1), false); Hold(CarOf(2), false); flow.CompleteResults(); }
        public void Rematch() { Runs.Clear(); Round = 0; flow.StartFreeRoam(); }
        // the result: the longer run as the runner wins; getting away beats being caught; both away = a draw (0)
        public int Winner
        {
            get
            {
                if (Runs.Count < 2) return 0;
                var a = Runs[0]; var b = Runs[1];
                if (!a.caught && !b.caught) return 0;
                if (a.caught != b.caught) return a.caught ? b.runner : a.runner;
                return a.seconds > b.seconds ? a.runner : b.seconds > a.seconds ? b.runner : 0;
            }
        }
        public string Line(Run r) => $"{SplitScreen.NameOf(r.runner)} ran ({VehicleProfile.Find(r.runnerVehicle).Name}) from {SplitScreen.NameOf(3 - r.runner)} (Patrol Car): {(r.caught ? "CAUGHT after " + RaceHud.FormatTime(r.seconds) : "GOT AWAY (" + RaceHud.FormatTime(r.seconds) + ")")}";
    }
}
