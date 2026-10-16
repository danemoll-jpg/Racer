using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

namespace Racer
{
    // 0.121 evidence only (the 0.106 check component: -p106check <dir> -racerTestSave <dir>; never active without both):
    //   -p106kind final121   BUG-005: Leviathan Forward with the player driven by the road AI, every racer watched at CP 44 (Hwy 92): each crossing of
    //                        the gate's plane (where in its opening, credited or not), each racer's gate state while it is near the gate and has not
    //                        passed it, resets, and how each finished. Default: the Stunt Chapter's Stunt Final as Dan drove it (2 laps, 5 rivals on
    //                        stunt vehicles, its own difficulty). -p121normal 1: a normal quick race instead (-p115laps, -p115diff, 5 rivals).
    //                        A shot at Dan's spot (X 291, Z 560) the first time rivals pass it.
    public sealed partial class Report106PlayerCheck
    {
        IEnumerator Final121()
        {
            DriverVariation.Seed = System.Environment.TickCount; Conditions();
            bool normal = Arg("-p121normal", "0") == "1";
            if (normal)
            {
                Campaign.Testing = true; int laps = int.Parse(Arg("-p115laps", "1")), diff = int.Parse(Arg("-p115diff", "2"));
                yield return QuickTo115(8); Conditions(); if (race.stunts) { flow.StepRaceType(1); yield return WaitMenu(); Bind(); }
                var st = flow.Save.Settings; st.opponents = true; st.traffic = false; st.laps = laps; st.difficulty = diff; st.estimateAiFinishes = false;
                st.opponentRoster = new[] { "fastback", "moto", "atv", "drifter", "scrambler" }; flow.Save.SaveSettings();
                race.opponents = true; race.traffic = false; race.laps = laps; race.difficulty = diff; race.opponentRoster = (string[])st.opponentRoster.Clone();
                flow.StartRace();
            }
            else
            {
                Campaign.Testing = false; flow.Save.Settings.unlockEverything = false; flow.Save.SaveSettings();
                var e = CampaignData.Find("c5-stunt-final");
                foreach (var before in CampaignData.InChapter(5).TakeWhile(x => x != e)) if (!Campaign.Passed(before)) { Campaign.Selected = before.Id; Campaign.DebugMarkWon(); }
                flow.StartCampaignEvent(e, "stuntbuggy"); yield return new WaitForSecondsRealtime(2);
            }
            yield return WaitRacing(); Bind();
            var me = race.Racers[0]; var car = race.vehicle; Pilot(car, me);
            if (race.stunts) { var steer = car.gameObject.AddComponent<RivalTricks>(); steer.Initialize(race, 9); steer.Bind(me); steer.SteerOnly = true; }
            race.finishGraceSeconds = 100000;
            int cp = race.gates.Length - 1; var gate = race.gates[cp]; float gs = race.GateStation(cp);
            Note($"== {(normal ? "normal race" : "Stunt Final (campaign)")} on {race.courseName} ({race.courseId}): stunts {race.stunts}, split {race.splitScore}, laps {race.laps}, difficulty {race.DifficultyName}, bumps built {StuntBumps.Built}; player {Rp(me)}; rivals {string.Join(", ", race.Racers.Where(x => x.IsAi).Select(x => x.Name + " " + Rp(x)))}");
            Note($"  CP {cp} '{gate.name}' at {V(gate.transform.position)} yaw {gate.transform.eulerAngles.y:F1}, half width {gate.halfWidth}, half height {gate.halfHeight}; station {gs:F1} (line {race.road.Length:F1})");
            var all = race.Racers.ToList(); var ai = all.Where(x => x.IsAi).ToList();
            var prevZ = all.ToDictionary(x => x, x => float.NaN); var prevPos = all.ToDictionary(x => x, x => Vector3.zero);
            var near = all.ToDictionary(x => x, x => 0f); var worst = all.ToDictionary(x => x, x => 0f); var reset0 = all.ToDictionary(x => x, x => x.Recoveries); var counted = all.ToDictionary(x => x, x => 0);
            var rows = all.ToDictionary(x => x, x => 0); var crossings = new List<string>(); var resets = new List<string>(); var gateLog = new List<string>();
            var lastGate = all.ToDictionary(x => x, x => x.Progress.NextGate);
            var ahead = all.ToDictionary(x => x, x => false); var last = all.ToDictionary(x => x, x => ""); float stopAt = float.Parse(Arg("-p121stop", "0"), CultureInfo.InvariantCulture);
            Vector3 dan = new Vector3(290.96f, 0, 560.12f); bool shot = false;
            var bumps = race.stunts ? StuntBumps.For(race.courseId)?.bumps ?? new LeviathanCourse.Kicker[0] : new LeviathanCourse.Kicker[0];
            Note($"  bumps: {string.Join("; ", bumps.Select(b => $"{b.at} at {b.pos[0]:F1}, {b.pos[1]:F1}"))}");
            var air = all.ToDictionary(x => x, x => false); var from = all.ToDictionary(x => x, x => Vector3.zero); var overGates = all.ToDictionary(x => x, x => new List<string>()); var bumpFlights = new List<string>();
            Time.timeScale = 3; float t0 = Time.realtimeSinceStartup;
            string Who(RacerState x) => x == me ? "player" : x.Name;
            while (Time.realtimeSinceStartup - t0 < 1800 && flow.State == RaceFlow.Stage.Racing && !(me.Progress.Finished && ai.All(x => x.Progress.Finished || x.Dnf)))
            {
                yield return new WaitForFixedUpdate(); AudioListener.volume = 0; if (Time.timeScale == 0) Time.timeScale = 3;
                foreach (var x in all)
                {
                    var c = x.Car; if (!c) continue; var p = c.Body.position; var p0 = prevPos[x]; prevPos[x] = p;
                    while (x.Recoveries - reset0[x] > counted[x]) { counted[x]++; resets.Add($"{Who(x)} L{x.Progress.CompletedLaps + 1} next {x.Progress.NextGate} at {V(p)} (road {x.RoadPosition:F0})"); }
                    if (x.Progress.NextGate != lastGate[x]) { if (x.Progress.NextGate == 0 || x.Progress.NextGate == cp || lastGate[x] == cp) gateLog.Add($"{Who(x)} L{x.Progress.CompletedLaps + 1} next {lastGate[x]} -> {x.Progress.NextGate} at {V(p)} t {race.Progress.RaceTime(race.Clock):F1}"); lastGate[x] = x.Progress.NextGate; }
                    var l = gate.transform.InverseTransformPoint(p);
                    if (!float.IsNaN(prevZ[x]) && (prevZ[x] < 0) != (l.z < 0) && Mathf.Abs(l.x) < 40 && Mathf.Abs(l.y) < 30)
                        crossings.Add($"{Who(x)} L{x.Progress.CompletedLaps + 1} {(l.z >= 0 ? "forward" : "back")} at x {l.x:F1} y {l.y:F1} (grounded {c.GroundedWheels}), next gate now {x.Progress.NextGate}, road {x.RoadPosition:F1} verified {x.VerifiedRoad:F1} tracked {x.TrackedStation:F1}, misses {x.Progress.MissedGates}");
                    prevZ[x] = l.z;
                    // every flight off a bump: how far, and any gate's plane crossed in the air
                    if (c.GroundedWheels == 0)
                    {
                        if (!air[x]) { air[x] = true; from[x] = p; overGates[x].Clear(); }
                        else for (int g = 1; g < race.gates.Length; g++) { var a = race.gates[g].transform.InverseTransformPoint(p0); var b = race.gates[g].transform.InverseTransformPoint(p); if ((a.z < 0) != (b.z < 0) && Mathf.Abs(b.x) < 40 && Mathf.Abs(b.y) < 30) overGates[x].Add($"CP {g} at x {b.x:F1} y {b.y:F1}"); }
                    }
                    else if (air[x] && c.GroundedWheels >= 2)
                    {
                        air[x] = false; int bi = -1; float bd = 12;
                        for (int i = 0; i < bumps.Length; i++) { float d = Vector2.Distance(new Vector2(from[x].x, from[x].z), new Vector2(bumps[i].pos[0], bumps[i].pos[1])); if (d < bd) { bd = d; bi = i; } }
                        float flown = Vector2.Distance(new Vector2(from[x].x, from[x].z), new Vector2(p.x, p.z));
                        if (bi >= 0 && flown > 4) bumpFlights.Add($"{Who(x)} L{x.Progress.CompletedLaps + 1} off {bumps[bi].at}: {flown:F0} m{(overGates[x].Count > 0 ? ", over " + string.Join(", ", overGates[x]) : "")}");
                    }
                    // diagnostics: the frame a racer's road position first runs more than 3 m ahead of its verified progress before / at the gate
                    var rsp = c.GetComponent<VehicleRespawn>();
                    string now = $"pos {V(p)} grounded {c.GroundedWheels} step {(p - p0).magnitude:F2} road {x.RoadPosition:F1} verified {x.VerifiedRoad:F1} tracked {x.TrackedStation:F1} respawn {(rsp && rsp.Tracking ? rsp.TrackingStation.ToString("F1") : "-")} recovering {!float.IsNaN(x.RecoveryStart)}";
                    if (x.Progress.NextGate == cp && x.RoadPosition > gs - 120 && x.RoadPosition < gs + 70 && x.RoadPosition - x.VerifiedRoad > 3 && !ahead[x]) { ahead[x] = true; gateLog.Add($"  {Who(x)} L{x.Progress.CompletedLaps + 1} road ahead of verified: before [{last[x]}] now [{now}]"); }
                    if (x.Progress.NextGate != cp) ahead[x] = false;
                    last[x] = now;
                    bool stuck = x.Progress.LapActive && x.Progress.NextGate == cp && Vector2.Distance(new Vector2(p.x, p.z), new Vector2(gate.transform.position.x, gate.transform.position.z)) < 150;
                    near[x] = stuck ? near[x] + Time.fixedDeltaTime : 0; worst[x] = Mathf.Max(worst[x], near[x]);
                    if (stuck && near[x] > 15 && rows[x] < 12 && Mathf.Repeat(near[x], 2) < Time.fixedDeltaTime)
                    {
                        rows[x]++; var rd = c.GetComponent<RoadDriver>();
                        gateLog.Add($"  {Who(x)} {near[x]:F0} s near CP {cp} unpassed: at {V(p)} heading {c.transform.eulerAngles.y:F0} speed {c.Body.linearVelocity.magnitude:F1}; road {x.RoadPosition:F1} verified {x.VerifiedRoad:F1} tracked {x.TrackedStation:F1} gate {gs:F1}; local x {l.x:F1} z {l.z:F1}");
                    }
                    if (!shot && x.IsAi && Vector2.Distance(new Vector2(p.x, p.z), new Vector2(dan.x, dan.z)) < 8 && (me.Car.Body.position - p).magnitude > 1)
                    {
                        // Dan's view (behind his spot, heading 82°) from a camera of its own, drawn over the race camera for the shot
                        shot = true; float ts = Time.timeScale; Time.timeScale = 0; var cam = Camera.main;
                        var rigs = FindObjectsByType<ChaseCamera>(FindObjectsSortMode.None).Where(k => k.enabled).ToArray(); foreach (var k in rigs) k.enabled = false;
                        var was = cam ? (cam.transform.position, cam.transform.rotation) : default;
                        if (cam) cam.transform.SetPositionAndRotation(new Vector3(dan.x - 30, p.y + 6, dan.z - 4.3f), Quaternion.Euler(10, 82, 0));
                        yield return Snap("spot-BUG-005-" + (normal ? "normal" : "stunt"));
                        if (cam) cam.transform.SetPositionAndRotation(was.Item1, was.Item2); foreach (var k in rigs) k.enabled = true; Time.timeScale = ts;
                        Note($"  shot at Dan's spot: {Who(x)} at {V(p)}, next gate {x.Progress.NextGate}");
                    }
                }
                if (me.Progress.Finished && ai.Any(x => !x.Progress.Finished && !x.Dnf) && race.Progress.RaceTime(race.Clock) > 1500) break;
                if (stopAt > 0 && race.Progress.RaceTime(race.Clock) > stopAt) { Note($"  stopped at {stopAt} s (-p121stop)"); break; }
            }
            Time.timeScale = 1;
            foreach (var x in all) Note($"  {Who(x)} {Rp(x)}: {(x.Progress.Finished ? "finished " + x.ClassifiedTime(race.Clock).ToString("0.0", CultureInfo.InvariantCulture) + " s" : x.Dnf ? "DNF" : $"UNFINISHED (lap {x.Progress.CompletedLaps + 1}, next gate {x.Progress.NextGate})")}; resets {x.Recoveries - reset0[x]}; misses {x.Progress.MissedGates} [{string.Join("; ", x.Progress.Penalties)}]; longest near CP {cp} unpassed {worst[x]:F0} s");
            Note($"  CP {cp} plane crossings: {crossings.Count}"); foreach (var s in crossings) Note("    " + s);
            Note($"  gate changes at CP {cp} / finish: {gateLog.Count}"); foreach (var s in gateLog) Note("    " + s);
            Note($"  resets: {resets.Count}"); foreach (var s in resets) Note("    " + s);
            Note($"  flights off a bump: {bumpFlights.Count}"); foreach (var s in bumpFlights) Note("    " + s);
            race.finishGraceSeconds = 0; float wr = Time.realtimeSinceStartup; while (flow.State != RaceFlow.Stage.Results && Time.realtimeSinceStartup - wr < 60) yield return null;
        }
    }
}
