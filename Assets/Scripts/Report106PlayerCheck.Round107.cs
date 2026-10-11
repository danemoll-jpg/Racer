using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Racer
{
    // 0.107 evidence only, on the 0.106 check (same switches: -p106check <dir> -racerTestSave <dir>; never active without both):
    //   -p106kind landings  Free Roam, each of -p106vehicle: flights off the open park floor, turned in the air to a set angle and held there until the
    //                       first touch (as a player who over- or under-rotated): whether it counts or bails, the message, the tilt at the first touch
    //   -p106kind event107  the Stunt event (-p106players 1|2, -p106limit seconds, shortened): solo, the 0.106 circuit bot first (the ground time from
    //                       each landing to the next takeoff: Part D); then a flight out over the south fence and a reset (Parts B, C; both players in
    //                       split-screen), a sketchy landing (A), the hay bales counted (F), and a takeoff about 1 s before 0:00 with a combo open (E)
    public sealed class Report107Touches : MonoBehaviour { public int n; void OnCollisionEnter(Collision c) { n++; } }
    public sealed partial class Report106PlayerCheck
    {
        ArcadeActivities ActsOf(ArcadeVehicle car) => car == race.vehicle ? flow.Activities : SplitRoam.Current ? SplitRoam.Current.Activities2 : null;
        static void Neutral(Gamepad g) => InputSystem.QueueStateEvent(g, new GamepadState());
        static void DriveP(Gamepad g, ArcadeVehicle car, Vector3 target, float cap, float tip = 0)
        {
            bool air = car.GroundedWheels == 0; var p = car.Body.position;
            float ang = Vector3.SignedAngle(Flat(car.transform.forward), Flat(target - p), Vector3.up), speed = Vector3.Dot(car.Body.linearVelocity, car.transform.forward);
            InputSystem.QueueStateEvent(g, new GamepadState { leftStick = new Vector2(air ? 0 : Mathf.Clamp(ang / 25, -1, 1), air ? tip : 0), rightTrigger = speed < cap ? 1 : 0, leftTrigger = speed > cap + 1.5f ? Mathf.Clamp01((speed - cap) / 5) : 0 });
        }
        static string UV(Vector3 p) => $"u {p.x - StuntPark.X0:F0}, v {p.z - StuntPark.Z0:F0}";
        sealed class Seq { public float start, takeoff = -1; public bool done; public Vector3 at; }

        // standing at `from` facing `yaw` (2.2 s: a teleport pauses the stunt watch for 2 s), then moving at `speed`, a short run-up, and the launch
        IEnumerator Stand(Gamepad g, ArcadeVehicle car, Vector3 from, float yaw, float speed, float runup = .5f)
        {
            Put(car, from, yaw, 0); if (car == race.vehicle) FindAnyObjectByType<ChaseCamera>()?.Snap();
            float t0 = Time.time; while (Time.time - t0 < 2.2f) { yield return new WaitForFixedUpdate(); Neutral(g); }
            var fwd = Quaternion.Euler(0, yaw, 0) * Vector3.forward; car.Body.linearVelocity = fwd * speed;
            t0 = Time.time; while (Time.time - t0 < runup) { yield return new WaitForFixedUpdate(); DriveP(g, car, from + fwd * 300, speed); }
            Neutral(g);
        }
        // the launch: `vy` up, turned about the vehicle's own axes by `turn` (x: pitch, + nose down; z: roll) at `rate` degrees a second, then held
        // at that angle until it first touches (a wheel or the body); then the game's physics alone. Returns the tilt at the first touch in tilt[0].
        IEnumerator Launch(ArcadeVehicle car, float vy, Vector3 turn, float[] tilt, float rate = 250)
        {
            var touches = car.GetComponent<Report107Touches>() ?? car.gameObject.AddComponent<Report107Touches>();
            car.Body.linearVelocity += Vector3.up * vy; var q0 = car.Body.rotation; Vector3 done = Vector3.zero; bool off = false; float tk = Time.time;
            while (Time.time - tk < 6)
            {
                yield return new WaitForFixedUpdate();
                if (!off) { if (car.GroundedWheels == 0) { off = true; touches.n = 0; } continue; }
                if (car.GroundedWheels > 0 || touches.n > 0) { tilt[0] = Vector3.Angle(car.transform.up, Vector3.up); tilt[1] = touches.n > 0 ? 1 : 0; yield break; }
                var left = turn - done; float s = rate * Time.fixedDeltaTime;
                var step = new Vector3(Mathf.Sign(left.x) * Mathf.Min(Mathf.Abs(left.x), s), 0, Mathf.Sign(left.z) * Mathf.Min(Mathf.Abs(left.z), s)); done += step;
                car.Body.rotation = q0 * Quaternion.Euler(done.x, 0, 0) * Quaternion.Euler(0, 0, done.z);
                car.Body.angularVelocity = car.transform.TransformDirection(step * (Mathf.Deg2Rad / Time.fixedDeltaTime));
            }
        }
        // a flight and what the game made of it (counted / bailed, the message shown, the tilt at the first touch)
        IEnumerator Flight(Gamepad g, ArcadeVehicle car, string name, Vector3 from, float yaw, float speed, float vy, Vector3 turn, List<string> outcome, float runup = .5f)
        {
            var acts = ActsOf(car); yield return Stand(g, car, from, yaw, speed, runup);
            int bails = acts.Bails; var shown = new List<string>(); string last = acts.Feedback; var tilt = new float[] { -1, 0 }; var takeoff = car.Body.position;
            yield return Launch(car, vy, turn, tilt);
            float t0 = Time.time; float up0 = 1;
            while (Time.time - t0 < 4)
            {
                yield return new WaitForFixedUpdate(); Neutral(g); up0 = car.transform.up.y;
                if (acts.Feedback != last && !string.IsNullOrEmpty(acts.Feedback)) { last = acts.Feedback; shown.Add(acts.Feedback.Replace("\n", " | ")); }
                if (acts.Bails > bails || acts.ComboLinks > 0 && !acts.StuntBusy) break;
            }
            float settle = Time.time; while (Time.time - settle < .6f) { yield return new WaitForFixedUpdate(); if (acts.Feedback != last && !string.IsNullOrEmpty(acts.Feedback)) { last = acts.Feedback; shown.Add(acts.Feedback.Replace("\n", " | ")); } }
            string result = acts.Bails > bails ? $"BAILED ({acts.LastBail})" : acts.ComboLinks > 0 ? $"COUNTED, {acts.LastStuntPoints} pts{(acts.LastSketchy ? ", sketchy" : "")}" : "no flight scored";
            outcome.Add($"{name}: took off at {UV(takeoff)}, {Mathf.Abs(turn.x) + Mathf.Abs(turn.z):F0} degrees turned; first touch by the {(tilt[1] > 0 ? "body" : "wheels")} at {tilt[0]:F0} degrees from upright (game: {acts.LastTouchTilt:F0}); landed at {UV(car.Body.position)}{(StuntPark.OnPark(car.Body.position) ? "" : " (outside the park)")}, upright {Mathf.Acos(Mathf.Clamp(up0, -1, 1)) * Mathf.Rad2Deg:F0} degrees from level -> {result}");
            foreach (var s in shown) outcome.Add("    shown: " + s);
        }

        // ---------------------------------------------------------------- Part A: landings
        IEnumerator Landings()
        {
            var vehicles = Arg("-p106vehicle", "stuntbuggy,stuntbike,original").Split(',');
            Campaign.Testing = true; flow.Save.Settings.unlockEverything = true; flow.Save.SaveSettings(); // the stunt motorcycle (test save only)
            yield return ToFreeRoam(vehicles[0]);
            var cases = new (string name, Vector3 turn)[]
            {
                ("backflip, 60 degrees short: nose first onto the front wheels", new Vector3(-300, 0, 0)),
                ("backflip, 60 degrees over: tail first onto the back wheels", new Vector3(-420, 0, 0)),
                ("backflip, 75 degrees over: tail first, steeper", new Vector3(-435, 0, 0)),
                ("frontflip, 70 degrees short: tail first onto the back wheels", new Vector3(290, 0, 0)),
                ("rolled 70 degrees: onto the wheels of one side", new Vector3(0, 0, 70)),
                ("half backflip: onto the roof", new Vector3(-180, 0, 0)),
                ("rolled 95 degrees: onto the side", new Vector3(0, 0, 95)),
                ("rolled 115 degrees: onto the side, past the balance point", new Vector3(0, 0, 115)),
            };
            foreach (var v in vehicles)
            {
                yield return Vehicle(v); BindPad(); Note($"--- {VehicleProfile.Find(v).Name} ({v}): launched from the floor at u 112, v 52 heading north, 10 m/s, 10 m/s up");
                int k = 0;
                foreach (var (name, turn) in cases)
                {
                    var outcome = new List<string>(); yield return Flight(pad, race.vehicle, name, P(112, 52), 0, 10, 10, turn, outcome); foreach (var o in outcome) Note(o);
                    if (k++ == 0) yield return Snap("A-" + v + "-sketchy");
                }
            }
        }

        // ---------------------------------------------------------------- the event, A-F together
        // the hop and the huge table (a frontflip): the hop opens a combo, the table's flight is the one that should finish after 0:00
        IEnumerator HugeTable(Gamepad g, ArcadeVehicle car, Seq s)
        {
            s.start = Time.time; s.takeoff = -1; s.done = false; var acts = ActsOf(car);
            yield return Stand(g, car, P(2, 46), 90, 14, .4f); var tilt = new float[2]; yield return Launch(car, 5, Vector3.zero, tilt);
            float t0 = Time.time, airStart = 0, airEstimate = 1; Vector3 turn = default; bool air = false; float landed = -1;
            while (Time.time - t0 < 12)
            {
                yield return new WaitForFixedUpdate(); var p = car.Body.position; float tip = 0;
                bool off = car.GroundedWheels == 0;
                if (off && !air && p.x - StuntPark.X0 > 45) { air = true; s.takeoff = Time.time; s.at = p; airStart = Time.time; float vy = car.Body.linearVelocity.y, drop = Mathf.Max(0, p.y - (StuntPark.FloorY + 1.6f)); airEstimate = (vy + Mathf.Sqrt(Mathf.Max(0, vy * vy + 2 * 9.81f * drop))) / 9.81f; }
                if (air && landed < 0)
                {
                    turn += car.transform.InverseTransformDirection(car.Body.angularVelocity) * (Time.fixedDeltaTime * Mathf.Rad2Deg);
                    float left = Mathf.Max(.12f, airEstimate * .82f - (Time.time - airStart)); tip = Mathf.Clamp((360 - turn.x) / left / (ArcadeVehicle.AirTipRate * Mathf.Rad2Deg), -1, 1);
                    if (!off && car.GroundedWheels >= 2 && Time.time - airStart > .5f) landed = Time.time;
                }
                if (landed > 0 && Time.time - landed > 1.5f) break;
                DriveP(g, car, P(110, 46), 18, tip);
            }
            Neutral(g); s.done = true;
        }
        // player 2's line: K2 then K3 (a backflip on K3); K3's flight should finish after 0:00
        IEnumerator KickerLine(Gamepad g, ArcadeVehicle car, Seq s)
        {
            s.start = Time.time; s.takeoff = -1; s.done = false;
            yield return Stand(g, car, P(121, 20), 270, 15, .3f);
            float t0 = Time.time, airStart = 0, airEstimate = 1; Vector3 turn = default; bool air = false; float landed = -1;
            while (Time.time - t0 < 14)
            {
                yield return new WaitForFixedUpdate(); var p = car.Body.position; float tip = 0;
                bool off = car.GroundedWheels == 0;
                if (off && !air && p.x - StuntPark.X0 < 60) { air = true; s.takeoff = Time.time; s.at = p; airStart = Time.time; float vy = car.Body.linearVelocity.y, drop = Mathf.Max(0, p.y - (StuntPark.FloorY + 1.6f)); airEstimate = (vy + Mathf.Sqrt(Mathf.Max(0, vy * vy + 2 * 9.81f * drop))) / 9.81f; }
                if (air && landed < 0)
                {
                    turn += car.transform.InverseTransformDirection(car.Body.angularVelocity) * (Time.fixedDeltaTime * Mathf.Rad2Deg);
                    float left = Mathf.Max(.12f, airEstimate * .82f - (Time.time - airStart)); tip = -Mathf.Clamp((360 + turn.x) / left / (ArcadeVehicle.AirTipRate * Mathf.Rad2Deg), -1, 1);
                    if (!off && car.GroundedWheels >= 2 && Time.time - airStart > .5f) landed = Time.time;
                }
                if (landed > 0 && Time.time - landed > 1.5f) break;
                DriveP(g, car, P(10, 20), 15, tip);
            }
            Neutral(g); s.done = true;
        }
        IEnumerator OutAndReset(Gamepad g, ArcadeVehicle car, StuntEvent ev, int player, Vector3 from, List<string> notes)
        {
            var r = ev.Of(player); int before = r.points, outside = r.outside; var outcome = new List<string>();
            yield return Flight(g, car, $"P{player}: a backflip off the floor by the south fence, out toward Hwy 92", from, 180, 12, 8, new Vector3(-360, 0, 0), outcome, .3f); notes.AddRange(outcome);
            var at = car.Body.position; yield return new WaitForSeconds(1);
            InputSystem.QueueStateEvent(g, new GamepadState().WithButton(GamepadButton.North)); yield return new WaitForSeconds(.15f); Neutral(g);
            yield return new WaitForSeconds(2.5f); var resp = car.GetComponent<VehicleRespawn>(); var p = car.Body.position;
            float facing = Vector3.Angle(Flat(car.transform.forward), Flat(StuntPark.Centre - p));
            notes.Add($"P{player}: before the reset at {V(at)} ({(StuntPark.OnPark(at) ? "in the park" : "outside the park")}); reset (Y) -> '{resp.LastRecovery}' at {V(p)} ({UV(p)}), {(StuntPark.OnPark(p) ? "IN the park" : "NOT in the park")}, {facing:F0} degrees off facing the middle; event points {before:N0} -> {r.points:N0}, not-counted combos {outside} -> {r.outside}; last log: {r.log.LastOrDefault()}");
        }
        IEnumerator Event107()
        {
            int players = int.Parse(Arg("-p106players", "1")); StuntEvent.TestLimit = F(Arg("-p106limit", players > 1 ? "50" : "100"));
            var pad2 = players > 1 ? InputSystem.AddDevice<Gamepad>("Report107Pad2") : null;
            Conditions(); flow.Save.Settings.hints = false; flow.Save.SaveSettings();
            SplitScreen.Mode = SplitScreen.Kind.Stunt; SplitScreen.Solo = players == 1; SplitScreen.P1Device = pad; SplitScreen.P2Device = pad2; SplitScreen.P2Ai = players == 1;
            SplitScreen.P1Vehicle = Arg("-p106vehicle", "stuntbuggy"); SplitScreen.P2Vehicle = "stuntbuggy"; SplitScreen.FromRoam = false; SplitScreen.Time = TimeOfDay.Day; SplitScreen.Weather = Weather.Clear;
            if (players > 1) { SplitScreen.P1Name = "Player One"; SplitScreen.P2Pick = "Player Two"; }
            Note($"Stunt event: {players} player(s), round shortened to {StuntEvent.TestLimit} s (StuntEvent.TestLimit, checks only); combo gap {ArcadeActivities.ComboGap} s");
            flow.StartSplit(); float r0 = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup - r0 < 150) { Bind(); if (flow && StuntEvent.Current && StuntEvent.Current.State == StuntEvent.Phase.Running) break; yield return null; }
            var ev = StuntEvent.Current; if (!ev) { Note("the event did not start"); yield break; }
            AudioListener.volume = 0; WorldLook.Current?.SetClock(WorldLook.Current.Day, 12);
            var c1 = race.vehicle; var c2 = players > 1 && SplitRoam.Current ? SplitRoam.Current.Car : null;
            Note($"Part F: hay bale colliders in the scene: {FindObjectsByType<Collider>(FindObjectsSortMode.None).Count(c => c.name.Contains("hay bale"))}; park report: {StuntPark.Current?.Report.LastOrDefault()}");
            var notes = new List<string>();
            if (players == 1)
            {
                // Part D: the 0.106 circuit bot for 50 s; every flight's landing and the ground time to the next takeoff
                var b = new Bot { pad = pad, car = c1 }; var flights = new List<(float t0, float t1, Vector3 a, Vector3 b)>(); bool air = false; float ta = 0; Vector3 pa = default;
                var banks = new List<string>(); void OnBank(int pts, bool park) => banks.Add($"{ev.Clock:F1}s banked {pts} at {UV(c1.Body.position)}{(c1.GroundedWheels >= 2 ? " (on the ground)" : "")}");
                flow.Activities.ComboBanked += OnBank;
                while (ev.Clock < 53) { yield return null; if (flow.State != RaceFlow.Stage.Racing) continue; BotStep(b); bool off = c1.GroundedWheels == 0; if (off && !air) { air = true; ta = Time.time; pa = c1.Body.position; } if (!off && c1.GroundedWheels >= 2 && air) { air = false; if (Time.time - ta > .6f) flights.Add((ta, Time.time, pa, c1.Body.position)); } }
                flow.Activities.ComboBanked -= OnBank; Neutral(pad);
                Note($"Part D: the circuit bot, {flights.Count} flights in 50 s; points {ev.Of(1).points:N0}");
                for (int i = 0; i + 1 < flights.Count; i++) Note($"    landing at {UV(flights[i].b)} -> next takeoff at {UV(flights[i + 1].a)} after {flights[i + 1].t0 - flights[i].t1:F1} s on the ground{(flights[i + 1].t0 - flights[i].t1 > ArcadeActivities.ComboGap ? "  (over 5 s: the combo ends)" : "")}");
                foreach (var s in banks) Note("    " + s);
                foreach (var l in resetLog) Note("    bot " + l);
            }
            // Parts B and C: out over the south fence, then the reset
            var tasks = new List<Coroutine> { StartCoroutine(OutAndReset(pad, c1, ev, 1, P(60, 13), notes)) };
            if (c2) tasks.Add(StartCoroutine(OutAndReset(pad2, c2, ev, 2, P(95, 13), notes)));
            foreach (var t in tasks) yield return t;
            yield return Snap($"C-after-reset-{players}p");
            foreach (var n in notes) Note(n); notes.Clear();
            if (players == 1)
            {
                var outcome = new List<string>(); yield return Flight(pad, c1, "Part A in the event: a backflip 60 degrees short", P(112, 52), 0, 10, 10, new Vector3(-300, 0, 0), outcome); foreach (var o in outcome) Note(o);
                yield return Snap("A-event-sketchy");
            }
            // Part E: dry runs (how long from the start of each line to its big takeoff), then the real ones timed to take off 1 s before 0:00
            var s1 = new Seq(); var s2 = new Seq();
            var dry = new List<Coroutine> { StartCoroutine(HugeTable(pad, c1, s1)) }; if (c2) dry.Add(StartCoroutine(KickerLine(pad2, c2, s2)));
            foreach (var t in dry) yield return t;
            float T1 = s1.takeoff - s1.start, T2 = c2 ? s2.takeoff - s2.start : 0;
            Note($"Part E dry runs: P1 hop + huge table takes off at {UV(s1.at)} {T1:F1} s after its start{(c2 ? $"; P2 K2 + K3 takes off {T2:F1} s after its start" : "")}; clock {ev.Clock:F1} s; points {string.Join(" / ", ev.Riders.Select(r => r.points.ToString("N0")))}");
            float limit = StuntEvent.TestLimit; bool go1 = false, go2 = !c2; var runs = new List<Coroutine>(); bool shot = false; float realAt1 = -1, realAt2 = -1;
            string finishText = "";
            while (ev.State != StuntEvent.Phase.Done)
            {
                yield return null;
                if (!go1 && ev.Clock >= limit - 1 - T1) { go1 = true; realAt1 = ev.Clock; runs.Add(StartCoroutine(HugeTable(pad, c1, s1))); }
                if (!go2 && ev.Clock >= limit - 1 - T2) { go2 = true; realAt2 = ev.Clock; runs.Add(StartCoroutine(KickerLine(pad2, c2, s2))); }
                if (ev.Overtime && !shot) { shot = true; yield return Snap($"E-finish-the-trick-{players}p"); finishText = string.Join(" | ", FindObjectsByType<UnityEngine.UI.Text>(FindObjectsSortMode.None).Where(t => t.isActiveAndEnabled && t.text.Contains("FINISH")).Select(t => t.text.Replace("\n", " / "))); }
            }
            float doneAt = ev.Clock;
            Note($"Part E: P1 started at {realAt1:F1} s, took off at {UV(s1.at)} at {(s1.takeoff > 0 ? (ev.Clock - (Time.time - s1.takeoff)).ToString("F1") : "?")} s (0:00 is {limit} s){(c2 ? $"; P2 started at {realAt2:F1} s, took off at {(s2.takeoff > 0 ? (ev.Clock - (Time.time - s2.takeoff)).ToString("F1") : "?")} s" : "")}; HUD at 0:00: '{finishText}'");
            float r1 = Time.realtimeSinceStartup; while (Time.realtimeSinceStartup - r1 < 15 && flow.State != RaceFlow.Stage.Results) yield return null;
            yield return new WaitForSecondsRealtime(1.5f);
            Note($"finished; state {flow.State}; winner {(ev.Winner == 0 ? (ev.Riders.Count > 1 ? "a draw" : "solo") : SplitScreen.NameOf(ev.Winner))}");
            foreach (var r in ev.Riders) { Note($"  {SplitScreen.NameOf(r.player)}: {r.points:N0} pts, medal {new[] { "none", "Bronze", "Silver", "Gold" }[r.medal]}, Top 10 #{r.place}, combos {r.combos}, best {r.best:N0}, not counted {r.outside}, tricks {r.acts.Tricks}, bails {r.acts.Bails}"); foreach (var l in r.log) Note("      " + l); }
            yield return Snap($"E-results-{players}p");
            var file = Path.Combine(flow.Save.DirectoryPath, StuntProgress.File); Note($"test save {flow.Save.DirectoryPath}; stunt file {(File.Exists(file) ? "written" : "missing")}");
            yield return new WaitForSecondsRealtime(1);
        }
    }
}
