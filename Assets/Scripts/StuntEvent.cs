using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Racer
{
    // 0.106 Part D: the Stunt event at Red Clay Stunt Park, on the split-screen machinery like Speed Patrol (one player on the full screen, two split):
    // 10 minutes to score as many points as possible. Everyone starts on the start line (held 3 s), then every combo banked inside the park counts
    // (ArcadeActivities: jumps, flips, rolls, spins, the loop and the barrel roll, chained for a multiplier; a bail ends a combo). The clock runs only
    // while driving (a pause keeps the clock and the points); at 0:00 the open combos count and the round ends. Results: each player's points and
    // medal (StuntProgress.Targets), best combo, tricks and bails; with two players the winner gets the usual winner shot and celebration; the Stunt
    // event Top 10 (every human player, under their names). Player 1's first Gold earns the stunt motorcycle.
    public sealed class StuntEvent : MonoBehaviour
    {
        public const float StartPause = 3;
        public static StuntEvent Current { get; private set; }
        public enum Phase { Starting, Running, Done }
        public sealed class Rider
        {
            public int player; public ArcadeVehicle car; public ArcadeActivities acts; public string vehicle;
            public int points, best, combos, outside, place, medal; public string line = ""; public float lineUntil;
            public readonly List<string> log = new(); public System.Action<int, bool> banked;
        }
        public readonly List<Rider> Riders = new();
        public Phase State { get; private set; }
        public float Clock { get; private set; }
        public float Limit => StuntProgress.Minutes * 60;
        public float Left => Mathf.Max(0, Limit - Clock);
        RaceFlow flow; RaceDirector race; float phaseAt;
        readonly HashSet<ArcadeVehicle> held = new();
        // checks only: a shorter round (seconds); never set in play
        public static float TestLimit = -1;

        public void Initialize(RaceFlow owner) { flow = owner; race = owner.Race; Current = this; }
        void OnDestroy() { Unhook(); foreach (var c in held.ToList()) Hold(c, false); if (Current == this) Current = null; }
        public Rider Of(int player) => Riders.FirstOrDefault(r => r.player == player);
        public bool Holds(ArcadeVehicle car) => held.Contains(car);
        // RaceDirector.RestartRace (via SplitRace.PlayerTwo): player 1 in their chosen vehicle and colour
        public void ApplyPlayerOne() { var c = race.vehicle.GetComponent<VehicleConfiguration>(); c.Apply(SplitScreen.P1Vehicle); c.SetBodyColor(SplitScreen.P1Color); }
        public void BeginRound()
        {
            Unhook(); Riders.Clear(); Clock = 0; State = Phase.Starting; phaseAt = Time.time;
            // to the start line, facing the kicker line (player 2 beside)
            var p1 = race.vehicle; var respawn = p1.GetComponent<VehicleRespawn>();
            if (!respawn.TryFastTravel(StuntPark.StartPos, StuntPark.StartRot)) { p1.Body.position = StuntPark.StartPos; p1.Body.rotation = StuntPark.StartRot; p1.transform.SetPositionAndRotation(StuntPark.StartPos, StuntPark.StartRot); }
            FindAnyObjectByType<ChaseCamera>()?.Snap();
            Add(1, p1, flow.Activities);
            var roam = SplitRoam.Current;
            if (!SplitScreen.Solo && roam && roam.Car) { roam.BringBeside(StuntPark.StartPos, StuntPark.StartRot); Add(2, roam.Car, roam.Activities2); }
            foreach (var r in Riders) { r.acts.NewSession(); Hold(r.car, true); }
        }
        void Add(int player, ArcadeVehicle car, ArcadeActivities acts)
        {
            if (!car || !acts) return;
            var r = new Rider { player = player, car = car, acts = acts, vehicle = car.GetComponent<VehicleConfiguration>().profileId };
            r.banked = (points, inPark) =>
            {
                if (State != Phase.Running) return;
                if (!inPark) { r.outside++; Say(r, "Outside the park: not counted"); r.log.Add($"{Clock:0.0}s outside the park {points}"); return; }
                r.points += points; r.combos++; r.best = Mathf.Max(r.best, points); Say(r, $"+{points:N0}"); r.log.Add($"{Clock:0.0}s +{points} ({r.acts.LastStunt})");
            };
            acts.ComboBanked += r.banked; Riders.Add(r);
        }
        void Unhook() { foreach (var r in Riders) if (r.acts) r.acts.ComboBanked -= r.banked; }
        void Say(Rider r, string text) { r.line = text; r.lineUntil = Time.unscaledTime + 3; }
        void Hold(ArcadeVehicle car, bool on)
        {
            if (!car) return;
            if (on) { held.Add(car); car.Body.linearVelocity = car.Body.angularVelocity = Vector3.zero; car.Body.isKinematic = true; car.enabled = false; }
            else { held.Remove(car); car.Body.isKinematic = false; car.enabled = true; }
            var input = car.GetComponent<VehicleInput>(); if (input) input.enabled = !on;
        }
        float RoundLimit => TestLimit > 0 ? TestLimit : Limit;
        void Update()
        {
            if (!flow || flow.State != RaceFlow.Stage.Racing || State == Phase.Done) return;
            foreach (var car in held) if (car && (!car.Body.isKinematic || car.enabled)) { car.Body.isKinematic = true; car.enabled = false; var input = car.GetComponent<VehicleInput>(); if (input) input.enabled = false; }
            if (State == Phase.Starting) { if (Time.time - phaseAt >= StartPause) { foreach (var r in Riders) Hold(r.car, false); State = Phase.Running; } return; }
            Clock += Time.deltaTime;
            if (Clock >= RoundLimit) Finish();
        }
        public int Winner => Riders.Count < 2 ? 0 : Riders[0].points > Riders[1].points ? 1 : Riders[1].points > Riders[0].points ? 2 : 0;
        void Finish()
        {
            foreach (var r in Riders) r.acts.EndStuntCombo(); // the open combos count
            State = Phase.Done;
            foreach (var r in Riders)
            {
                r.medal = StuntProgress.MedalFor(r.points);
                bool human = r.player == 1 || !SplitScreen.P2Ai;
                r.place = human ? StuntProgress.Finish(r.player, SplitScreen.NameOf(r.player), r.vehicle, r.points, r.best) : 0;
            }
            int w = Winner;
            if (w > 0 && SplitScreen.Race && !SplitScreen.OneView) { SplitScreen.Race.WinnerShotFor(w); Of(w)?.car.GetComponent<RiderGestures>()?.Celebrate(); StartCoroutine(Results()); }
            else flow.CompleteResults();
        }
        System.Collections.IEnumerator Results()
        {
            float until = Time.unscaledTime + WinnerShot.Seconds;
            while (Time.unscaledTime < until && flow.State == RaceFlow.Stage.Racing) yield return null;
            if (flow.State == RaceFlow.Stage.Racing || flow.State == RaceFlow.Stage.Paused) flow.CompleteResults();
        }
        public void Rematch() { flow.StartFreeRoam(); }
    }
}
