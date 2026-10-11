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
    // 0.107: a combo whose flights took off in the park counts wherever they land (Part B). Part E: at 0:00 a player in the air or in the loop
    // finishes the trick ("FINISH THE TRICK!", the clock held at 0:00): the landing scores, the open combo is banked at once, then the results
    // (both players' flights in split-screen; at most OvertimeLimit seconds). A player on the ground at 0:00 has the open combo banked at once.
    // 0.108 Part A (Dan: "You should be able to continue going until either you crash or you didn't make it to another trick in time"): this
    // replaces 0.107's banking at once. At 0:00 a player with a combo going (in the air, in the loop, or on the ground inside the 5 s chain time)
    // or in the air keeps going, each player on their own ("COMBO OVERTIME", the chain time big on screen): every trick chained in time counts.
    // That player is done when the combo ends (the 5 s pass on the ground, a bail, a reset: banked as always), or a flight in the air at 0:00
    // lands with no trick; a player with nothing going at 0:00 is done then. The only safety: stopped for StillLimit seconds ends it. The results
    // wait for everyone; a player who is done sees "WAITING FOR P2" and their final score. Part B: each player's next medal (SplitHud).
    public sealed class StuntEvent : MonoBehaviour
    {
        public const float StartPause = 3;
        public static StuntEvent Current { get; private set; }
        public enum Phase { Starting, Running, Done }
        public sealed class Rider
        {
            public int player; public ArcadeVehicle car; public ArcadeActivities acts; public string vehicle;
            public int points, best, combos, outside, place, medal; public string line = ""; public float lineUntil; public bool closed;
            public int goalMedal; public float goalFlashUntil, still; // 0.108: the medal reached so far (its moment on the HUD); time stopped in overtime
            public readonly List<string> log = new(); public System.Action<int, bool> banked;
        }
        public readonly List<Rider> Riders = new();
        public Phase State { get; private set; }
        public float Clock { get; private set; }
        public float Limit => StuntProgress.Minutes * 60;
        public float Left => Mathf.Max(0, RoundLimit - Clock);
        public const float StillLimit = 5, MedalMoment = 2.5f;
        // 0.107 Part E / 0.108 Part A: past 0:00, players still going (a flight or a combo)
        public bool Overtime => State == Phase.Running && Clock >= RoundLimit;
        public bool Finishing(int player) { var r = Of(player); return Overtime && r != null && !r.closed; }
        // 0.108 Part D: a combo banked by this vehicle counts for the Top 10 (always outside the event; in it, while that player's round still counts)
        public static bool Counts(ArcadeVehicle car) { var e = Current; if (!e) return true; var r = e.Riders.FirstOrDefault(x => x.car == car); return r != null && e.State == Phase.Running && !r.closed; }
        // 0.107 Part C: a vehicle taking part in the running event (VehicleRespawn puts it back in the park)
        public static bool InEvent(ArcadeVehicle car) => Current && Current.State != Phase.Done && Current.Riders.Any(r => r.car == car);
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
                if (State != Phase.Running || r.closed) return;
                if (!inPark) { r.outside++; Say(r, "Outside the park: not counted"); r.log.Add($"{Clock:0.0}s outside the park {points}"); return; }
                r.points += points; r.combos++; r.best = Mathf.Max(r.best, points); Say(r, $"+{points:N0}"); r.log.Add($"{Clock:0.0}s +{points} ({r.acts.LastStunt}; x{r.acts.LastComboLinks}: {r.acts.LastComboTricks}){(r.acts.LastComboPlace > 0 ? $" Top 10 combo #{r.acts.LastComboPlace}" : "")}");
                int m = StuntProgress.MedalFor(r.points); if (m > r.goalMedal) { r.goalMedal = m; r.goalFlashUntil = Time.unscaledTime + MedalMoment; } // 0.108 Part B
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
            if (Clock < RoundLimit)
            {
                Clock = Mathf.Min(RoundLimit, Clock + Time.deltaTime); if (Clock < RoundLimit) return; overtime = 0;
                foreach (var r in Riders) { r.still = 0; r.log.Add(r.acts.ComboLinks > 0 ? $"0:00 with a combo going (x{r.acts.ComboLinks}, {r.acts.ComboTotal:N0} pts{(r.acts.StuntBusy ? ", in the air" : $", {r.acts.ComboLeft:0.0} s to chain")}): overtime" : r.acts.StuntBusy ? "0:00 in the air: overtime" : "0:00: done"); }
            }
            else overtime += Time.deltaTime;
            // 0.108 Part A: a player is done when nothing is going any more (no combo, not in the air); stopped for StillLimit s ends it too
            foreach (var r in Riders)
            {
                if (r.closed) continue;
                r.still = r.car && r.car.Body.linearVelocity.magnitude < 1 ? r.still + Time.deltaTime : 0;
                if (r.acts.ComboLinks == 0 && !r.acts.StuntBusy) { r.closed = true; if (overtime > 0) r.log.Add($"done {overtime:0.0}s after 0:00"); }
                else if (r.still >= StillLimit) { r.acts.EndStuntCombo(); r.closed = true; r.log.Add($"stopped for {StillLimit:0} s, {overtime:0.0}s after 0:00: ended"); }
            }
            if (Riders.All(r => r.closed)) Finish();
        }
        float overtime;
        public int Winner => Riders.Count < 2 ? 0 : Riders[0].points > Riders[1].points ? 1 : Riders[1].points > Riders[0].points ? 2 : 0;
        void Finish()
        {
            foreach (var r in Riders) if (!r.closed) { r.acts.EndStuntCombo(); r.closed = true; } // the open combos count
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
