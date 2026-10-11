using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Racer
{
    // 0.106 evidence only: the feature drives, the Stunt event, the menus and the frame times (Report106PlayerCheck)
    public sealed partial class Report106PlayerCheck
    {
        // a run along a line: from `from` toward the waypoints at `cap` m/s; trick: what to do in the air ("" none, "back", "front", "roll", "spin",
        // "halfback" (lands on the roof: a bail)); every flight, the stunt lines shown and the combos banked are logged
        sealed class Run { public string name, trick = ""; public Vector3 from; public Vector3[] path; public float cap, start = -1, limit = 14; public bool inPark = true; }
        static Vector3 PP(float u, float v, float y = 0) => StuntPark.P(u, v, y);
        static readonly Run[] Lines =
        {
            new() { name = "K1 small kicker", from = PP(150, 20), path = new[] { PP(105, 20) }, cap = 14 },
            new() { name = "K2 medium table", from = PP(121, 20), path = new[] { PP(66, 20) }, cap = 15 },
            new() { name = "K3 big table", from = PP(90, 20), path = new[] { PP(14, 20) }, cap = 16 },
            new() { name = "kicker line K1-K2-K3 (combo)", from = PP(152, 20), path = new[] { PP(14, 20) }, cap = 15, limit = 20 },
            new() { name = "west quarter-pipe", from = PP(42, 32), path = new[] { PP(-5, 32) }, cap = 13, limit = 8 },
            new() { name = "junk-car gap", from = PP(10, 18), path = new[] { PP(10, 30), PP(10, 100) }, cap = 16 },
            new() { name = "deck launch", from = PP(4, 138), path = new[] { PP(80, 138) }, cap = 15, limit = 10 },
            new() { name = "deck drop and the loop", from = PP(64, 138, 5), path = new[] { PP(150, 138) }, cap = 18, start = 14, limit = 16 },
            new() { name = "half-pipe (down its middle)", from = PP(146.5f, 145), path = new[] { PP(146.5f, 128), PP(146.5f, 30) }, cap = 14, start = 6 },
            new() { name = "half-pipe (up the east wall)", from = PP(144, 120), path = new[] { PP(160, 108) }, cap = 13, start = 8, limit = 7 },
            new() { name = "K4 huge table", from = PP(17, 46), path = new[] { PP(105, 46) }, cap = 18, start = 16 },
            new() { name = "barrel-roll ramp", from = PP(20, 74), path = new[] { PP(100, 72) }, cap = 17, start = 15 },
            new() { name = "school-bus gap", from = PP(18, 100), path = new[] { PP(100, 100) }, cap = 16, start = 14 },
        };
        int combos; readonly List<string> banked = new();
        IEnumerator Drive(Run run, string trick)
        {
            var car = race.vehicle; var acts = flow.Activities; var dir = Flat(run.path[0] - run.from).normalized;
            Put(car, run.from, Yaw(dir), run.start >= 0 ? run.start : run.cap * .7f); FindAnyObjectByType<ChaseCamera>()?.Snap();
            var hits = car.GetComponent<Report106Contacts>() ?? car.gameObject.AddComponent<Report106Contacts>(); hits.who = car.GetComponent<VehicleConfiguration>().profileId; hits.list.Clear();
            banked.Clear(); combos = 0;
            void OnBank(int p, bool park) { banked.Add($"{p}{(park ? "" : " (outside the park)")}"); combos++; }
            acts.ComboBanked += OnBank; acts.NewSession();
            int wp = 0; float t0 = Time.time; string lastFeedback = ""; var lines = new List<string>(); int resets = 0; void OnReset() => resets++;
            var respawn = car.GetComponent<VehicleRespawn>(); respawn.Respawned += OnReset;
            bool air = false; float airStart = 0, top = 0, airEstimate = 1, bank0 = 0; Vector3 takeoff = default, turn = default; var flights = new List<string>(); float maxY = float.MinValue; bool wiped = false;
            int loopsBefore = acts.Loops;
            while (Time.time - t0 < run.limit)
            {
                yield return new WaitForFixedUpdate(); AudioListener.volume = 0;
                var p = car.Body.position; maxY = Mathf.Max(maxY, p.y - StuntPark.FloorY);
                if (wp < run.path.Length - 1 && Vector3.Distance(Flat(p), Flat(run.path[wp])) < 6) wp++;
                var target = run.path[wp]; if (Vector3.Dot(Flat(target - p), dir) < 0 && wp == run.path.Length - 1) break;
                // the trick: in the air, push until the turn is nearly done, then let go (the vehicle levels itself)
                float tip = 0, roll = 0, spin = 0;
                if (air)
                {
                    var w = car.transform.InverseTransformDirection(car.Body.angularVelocity) * (Time.fixedDeltaTime * Mathf.Rad2Deg); turn += w;
                    // paced like a player: the turn left spread over the flight time left (estimated at takeoff), finished a little before touchdown
                    float left = Mathf.Max(.12f, airEstimate * .82f - (Time.time - airStart));
                    float Pace(float done, float goal, float max) => Mathf.Clamp((goal - done) / left / max, -1, 1);
                    float rate = ArcadeVehicle.AirTipRate * Mathf.Rad2Deg, spinRate = ArcadeVehicle.AirSpinRate * Mathf.Rad2Deg; if (airEstimate < 1.1f) rate = spinRate = float.MaxValue; // no trick on a hop
                    if (trick == "back") tip = -Pace(-turn.x, 360, rate); if (trick == "front") tip = Pace(turn.x, 360, rate); if (trick == "halfback") tip = -Pace(-turn.x, 180, rate);
                    if (trick == "roll") roll = Pace(-turn.z, 360 - bank0, rate); if (trick == "spin") spin = Pace(turn.y, 360, spinRate);
                }
                Drive(car, target, run.cap, tip, roll, spin);
                bool off = car.GroundedWheels == 0;
                if (off && !air)
                {
                    air = true; airStart = Time.time; takeoff = p; turn = Vector3.zero; top = p.y; bank0 = Mathf.Max(0, -Vector3.SignedAngle(Vector3.ProjectOnPlane(Vector3.up, car.transform.forward), car.transform.up, car.transform.forward));
                    float vy = car.Body.linearVelocity.y, drop = Mathf.Max(0, p.y - (StuntPark.FloorY + 1.6f)); airEstimate = (vy + Mathf.Sqrt(Mathf.Max(0, vy * vy + 2 * 9.81f * drop))) / 9.81f;
                }
                if (air) top = Mathf.Max(top, p.y);
                if (!off && air && car.GroundedWheels >= 2) { air = false; float at = Time.time - airStart; if (at > .3f) flights.Add($"flight {at:F2} s, {Vector3.Distance(Flat(takeoff), Flat(p)):F1} m, up {top - takeoff.y:F1} m, turned pitch {turn.x:F0} yaw {turn.y:F0} roll {turn.z:F0}, landed at {V(p)} speed {car.Body.linearVelocity.magnitude:F1}"); }
                if (car.GetComponent<VehicleConfiguration>().WipedOut) wiped = true;
                if (acts.Feedback != lastFeedback && !string.IsNullOrEmpty(acts.Feedback)) { lastFeedback = acts.Feedback; lines.Add(acts.Feedback.Replace("\n", " | ")); }
            }
            Release();
            float settle = Time.time; while (Time.time - settle < 3) { yield return new WaitForFixedUpdate(); if (acts.Feedback != lastFeedback && !string.IsNullOrEmpty(acts.Feedback)) { lastFeedback = acts.Feedback; lines.Add(acts.Feedback.Replace("\n", " | ")); } }
            acts.ComboBanked -= OnBank; respawn.Respawned -= OnReset;
            Note($"{run.name} [{hits.who}{(trick != "" ? ", trick " + trick : "")}] at {run.cap} m/s ({run.cap * 2.237f:F0} mph): {flights.Count} flight(s), highest {maxY:F1} m above the floor, loops {acts.Loops - loopsBefore}, resets {resets}, wiped out {wiped}, contacts {hits.list.Count}");
            foreach (var f in flights) Note("    " + f);
            foreach (var l in lines) Note("    shown: " + l);
            if (banked.Count > 0) Note("    combos banked: " + string.Join(", ", banked));
            foreach (var c in hits.list.Take(6)) Note("    " + c);
            yield return new WaitForSeconds(.3f);
        }
        IEnumerator Features()
        {
            var vehicles = Arg("-p106vehicle", "stuntbuggy").Split(',');
            Campaign.Testing = true; flow.Save.Settings.unlockEverything = true; flow.Save.SaveSettings(); // the stunt motorcycle (test save only)
            yield return ToFreeRoam(vehicles[0]);
            var only = Arg("-p106lines", ""); var tricks = Arg("-p106tricks", "");
            foreach (var v in vehicles)
            {
                yield return Vehicle(v); BindPad(); Note($"--- {VehicleProfile.Find(v).Name} ({v})");
                foreach (var run in Lines) if (only == "" || only.Split(';').Any(o => run.name.StartsWith(o))) yield return Drive(run, "");
                // set tricks: "line name=trick;..."
                foreach (var spec in tricks.Split(';').Where(x => x.Contains('=')))
                {
                    var kv = spec.Split('='); var run = Lines.FirstOrDefault(r => r.name.StartsWith(kv[0])); if (run == null) continue;
                    yield return Drive(run, kv[1]);
                }
            }
        }
    }
}
