using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Racer
{
    // 0.115 evidence only (the 0.106 check component: -p106check <dir> -racerTestSave <dir>; never active without both):
    //   -p106kind lev115    AI-only quick races on Leviathan (-p115course 8 Forward / 9 Reverse), the player's vehicle parked out of the race (as 0.113's
    //                       ai113): -p115rivals "a+b+c|d+e+f" (one line-up per run, in turn), -p115runs, -p115laps 2, -p115diff 1, -p113traffic 1, -p113watch.
    //                       One line per rival per run: vehicle, time, laps, missed gates, resets (where and why).
    //   -p106kind drive115  the controller only: main menu > RACE > Tracks > Leviathan (its map preview shot) > Laps 2 > START RACE, then the race
    //                       driven by the virtual pad (a bot on the course line, the game's own reset button when stuck) against 3 rivals; the results
    //                       and Records shots, the ghost file and the results log line for the race
    //   -p106kind split115  a split-screen race on Leviathan, player 2 the AI, both players' cars driven by the road AI, to the results
    public sealed partial class Report106PlayerCheck
    {
        IEnumerator QuickTo115(int course)
        {
            Campaign.Testing = true;
            if (RacePlaylists.CourseOf(UnityEngine.SceneManagement.SceneManager.GetActiveScene()) == course) yield break;
            if (flow.State == RaceFlow.Stage.Title) flow.EnterMenuAfterTitle();
            flow.OpenCourses(); yield return null; flow.SelectCourseEntry(course);
            float t1 = Time.realtimeSinceStartup; yield return new WaitForSecondsRealtime(1);
            while (RacePlaylists.CourseOf(UnityEngine.SceneManagement.SceneManager.GetActiveScene()) != course && Time.realtimeSinceStartup - t1 < 90) yield return null;
            yield return WaitMenu(); Bind();
        }
        string Lev() { var lev = race.Leviathan; return lev ? $"{race.courseName} ({race.courseId}): line {race.road.Length:F0} m, {race.gates.Length} gates, {race.Branches?.Length ?? 0} shortcuts, flights {string.Join(", ", race.GetComponent<MountainFlights>()?.flights.Select(f => $"{f.name} {f.approachStation:F0}-{f.endStation:F0}") ?? new string[0])}" : $"NOT LEVIATHAN: {race.courseName} ({race.courseId})"; }
        IEnumerator Lev115()
        {
            Campaign.Testing = true; DriverVariation.Seed = System.Environment.TickCount; Conditions();
            // 0.116: -p116forced "title,title": every rival takes these shortcuts (checks only; ShortcutStrategy.Forced)
            ShortcutStrategy.Forced = Arg("-p116forced", "").Replace('_', ' ').Split(new[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);
            // 0.116: -p116off "title,title": Leviathan Reverse shortcuts switched off (as Dan would in the F6 menu; saved in the test save's settings)
            if (Arg("-p116off", "") != "") { var off = Arg("-p116off", "").Replace('_', ' ').Split(new[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries); flow.Save.Settings.leviathanShortcutsOff = off; flow.Save.SaveSettings(); LeviathanCourse.ShortcutsOff = off; }
            int course = int.Parse(Arg("-p115course", "8")); int runs = int.Parse(Arg("-p115runs", "3")); int laps = int.Parse(Arg("-p115laps", "2")); int diff = int.Parse(Arg("-p115diff", "1"));
            var lineups = Arg("-p115rivals", "tourer+moto+atv|original+roadster+scrambler|fastback+drifter+pebble").Split('|');
            for (int run = 1; run <= runs; run++)
            {
                yield return QuickTo115(course); Conditions();
                flow.OpenGarage(); flow.SelectVehicle("atv"); flow.CloseGarage();
                var lineup = lineups[(run - 1) % lineups.Length];
                if (run == 1) Note($"== AI-only quick races on {RacePlaylists.Titles[course]}: {Lev()}; laps {laps}, difficulty {diff}, traffic {Arg("-p113traffic", "0")}");
                QuickStart(lineup.Split('+'), laps, diff);
                var rows = new List<string>(); yield return Field115(rows, $"run {run} [{lineup}]"); foreach (var r in rows) Note(r);
                flow.QuitRace(); yield return WaitMenu();
            }
        }
        // the rivals to the line (the player's vehicle parked out of the race), with each rival's laps, missed gates and resets
        IEnumerator Field115(List<string> outRows, string tag)
        {
            yield return new WaitForSecondsRealtime(.5f); yield return WaitRacing(); Bind();
            var me = race.vehicle; me.Body.isKinematic = true; foreach (var c in me.GetComponentsInChildren<Collider>()) c.enabled = false;
            var vi = me.GetComponent<VehicleInput>(); if (vi) vi.enabled = false; var respawn = me.GetComponent<VehicleRespawn>(); if (respawn) respawn.enabled = false;
            race.finishGraceSeconds = 100000;
            var ai = race.Racers.Where(r => r.IsAi).ToList(); var reset0 = ai.ToDictionary(r => r, r => r.Recoveries); var resetAt = ai.ToDictionary(r => r, r => new List<string>());
            var shortcuts = ai.ToDictionary(r => r, r => new List<string>()); var on = ai.ToDictionary(r => r, r => (WoodlandRoute)null);
            var watchArg = Arg("-p113watch", ""); Vector3 watch = default; float watchR = 0; if (watchArg.Length > 0) { var w3 = watchArg.Split(','); watch = new Vector3(F(w3[0]), 0, F(w3[1])); watchR = F(w3[2]); }
            var watched = ai.ToDictionary(r => r, r => new List<string>()); var nextWatch = ai.ToDictionary(r => r, r => 0f);
            float airMax = 0; string airWho = "";
            // 0.116: each rival's time from the entry to the exit of each of the course's shortcuts (planes across the road there), on it or on the main road
            var seg = ai.ToDictionary(r => r, r => new List<string>()); var segIn = new Dictionary<(RacerState, string), (float t, bool via)>(); var segPrev = ai.ToDictionary(r => r, r => r.Car.Body.position); var segData = race.Leviathan ? LeviathanCourse.Current?.branches : null; // the course's shortcuts, switched on or off var segPrev = ai.ToDictionary(r => r, r => r.Car.Body.position);
            Time.timeScale = 6; float t0 = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup - t0 < 1500 && flow.State == RaceFlow.Stage.Racing && !ai.All(r => r.Progress.Finished || r.Dnf))
            {
                AudioListener.volume = 0; yield return null; if (Time.timeScale == 0) Time.timeScale = 6;
                float clock = (float)race.Progress.RaceTime(race.Clock);
                foreach (var r in ai)
                {
                    if (r.Recoveries - reset0[r] > resetAt[r].Count) resetAt[r].Add($"[{r.Car.GetComponent<RoadDriver>()?.LastRecoveryWhy}] {V(r.Car.Body.position)} {(r.Branch.Route ? r.Branch.Route.title : "road " + r.RoadPosition.ToString("F0"))} lap {r.Progress.CompletedLaps + 1} next CP {r.Progress.NextGate} t {clock:F0}");
                    if (r.Branch.Route != on[r]) { on[r] = r.Branch.Route; if (on[r]) shortcuts[r].Add(on[r].title + " lap " + (r.Progress.CompletedLaps + 1)); }
                    if (segData != null) foreach (var b in segData)
                    {
                        var p1 = r.Car.Body.position; var p0 = segPrev[r]; var k = (r, b.title);
                        var ea = race.road.At(b.entryRoad, out var ef); var xa = race.road.At(b.exitRoad, out var xf);
                        if (Vector3.Dot(p1 - ea, ef) >= 0 && Vector3.Dot(p0 - ea, ef) < 0 && Vector3.Distance(p1, ea) < 25) segIn[k] = (Time.time, false);
                        if (segIn.TryGetValue(k, out var e) && r.Branch.Route && r.Branch.Route.title == b.title && !e.via) segIn[k] = (e.t, true);
                        if (segIn.TryGetValue(k, out e) && Vector3.Dot(p1 - xa, xf) >= 0 && Vector3.Dot(p0 - xa, xf) < 0 && Vector3.Distance(p1, xa) < 25) { seg[r].Add($"{b.title} lap {r.Progress.CompletedLaps + 1} {(e.via ? "VIA" : "main")} {Time.time - e.t:F1} s"); segIn.Remove(k); }
                    }
                    segPrev[r] = r.Car.Body.position;
                    if (r.Car.Body.position.y > 140 && r.Car.GroundedWheels == 0 && r.Car.Body.linearVelocity.magnitude > airMax) { airMax = r.Car.Body.linearVelocity.magnitude; airWho = Rp(r); }
                    if (watchR > 0 && Time.time >= nextWatch[r] && Vector3.Distance(Flat(r.Car.Body.position), watch) < watchR && watched[r].Count < 400)
                    {
                        nextWatch[r] = Time.time + .25f; var d = r.Car.GetComponent<RoadDriver>();
                        watched[r].Add(string.Format(CultureInfo.InvariantCulture, "t{0:F1} {1} v{2:F1} tgt{3:F1} thr{4:F2} brk{5:F2} obst[{6}] road {7:F0} hdg {8:F0}", clock, V(r.Car.Body.position), r.Car.ForwardSpeed, d ? d.TargetSpeed : -1, d ? d.LastThrottle : -1, d ? d.LastBrake : -1, d ? d.LastObstacle : "", r.RoadPosition, Yaw(r.Car.transform.forward)));
                    }
                }
            }
            Time.timeScale = 1;
            foreach (var r in ai.OrderBy(r => r.Progress.Finished ? r.ClassifiedTime(race.Clock) : 1e9))
            {
                outRows.Add(string.Format(CultureInfo.InvariantCulture, "  {0} {1,-10} {2} laps [{3}] missed gates {4} (+{5:0} s) resets {6} shortcuts [{7}]{8}", tag, Rp(r),
                    r.Progress.Finished ? r.ClassifiedTime(race.Clock).ToString("0.0", CultureInfo.InvariantCulture) : (r.Dnf ? "DNF" : "unfinished"), string.Join(", ", r.Progress.LapTimes.Select(l => l.ToString("0.0", CultureInfo.InvariantCulture))),
                    r.Progress.MissedGates, r.Progress.PenaltySeconds, r.Recoveries - reset0[r], string.Join("; ", shortcuts[r]),
                    resetAt[r].Count > 0 ? " | resets at " + string.Join("; ", resetAt[r].Take(12)) : ""));
                if (r.Progress.Penalties.Count > 0) outRows.Add("      penalties " + Rp(r) + ": " + string.Join(" | ", r.Progress.Penalties.Take(12)));
                if (seg[r].Count > 0) outRows.Add("      shortcut segments " + Rp(r) + ": " + string.Join(" | ", seg[r]));
                if (watched[r].Count > 0) outRows.Add("      watch " + Rp(r) + ": " + string.Join(" | ", watched[r].Take(200)));
            }
            race.finishGraceSeconds = 0; float w = Time.realtimeSinceStartup; while (flow.State != RaceFlow.Stage.Results && Time.realtimeSinceStartup - w < 30) yield return null;
        }

        // ---------------------------------------------------------------- the controller: menus, then the race driven by the pad
        IEnumerator Drive115()
        {
            int course = int.Parse(Arg("-p115course", "8")); Conditions();
            var st = flow.Save.Settings; st.opponents = true; st.traffic = true; st.difficulty = 1; st.opponentChoices = new[] { "mixed", "mixed", "mixed" }; st.vehicleId = Arg("-p106vehicle", "original"); flow.Save.SaveSettings();
            race.opponents = true; race.traffic = true; race.difficulty = 1;
            Note($"start: state {flow.State}, page '{Page}', focus '{Focus}', scene {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}");
            yield return FocusRow("race"); Note($"main menu: focus '{Focus}'"); yield return Press(GamepadButton.South); yield return new WaitForSecondsRealtime(1);
            Note($"after A: page '{Page}', focus '{Focus}'"); yield return FocusRow("tracks"); yield return Press(GamepadButton.South); yield return new WaitForSecondsRealtime(1);
            Note($"Tracks: state {flow.State}, rows {string.Join(", ", FindObjectsByType<UnityEngine.UI.Button>(FindObjectsSortMode.None).Where(b => b.isActiveAndEnabled && b.name.StartsWith("course-")).OrderBy(b => b.transform.GetSiblingIndex()).Select(b => b.name + "=" + b.GetComponentInChildren<UnityEngine.UI.Text>().text))}");
            yield return FocusRow("course-" + course, 30); yield return new WaitForSecondsRealtime(1); Note($"focus '{Focus}': {Texts("Leviathan")}"); yield return Snap($"tracks-preview-{course}");
            var map = FindObjectsByType<UnityEngine.UI.RawImage>(FindObjectsSortMode.None).FirstOrDefault(r => r.name == "Map" && r.isActiveAndEnabled);
            if (map) { var rt = map.rectTransform.rect; Note($"preview map {rt.width:F0} x {rt.height:F0} px (aspect {rt.width / rt.height:F3}), uv {map.uvRect.width:F3} x {map.uvRect.height:F3} (aspect {map.uvRect.width / map.uvRect.height:F3})"); }
            yield return Press(GamepadButton.South); float t1 = Time.realtimeSinceStartup; yield return new WaitForSecondsRealtime(2);
            while (RacePlaylists.CourseOf(UnityEngine.SceneManagement.SceneManager.GetActiveScene()) != course && Time.realtimeSinceStartup - t1 < 90) yield return null;
            yield return WaitMenu(); Bind(); Note($"loaded: {Lev()}; page '{Page}', focus '{Focus}', menu says: {Texts("RACE SETUP", "Leviathan")}");
            for (int k = 0; k < 6 && flow.LapLabel != "2"; k++) { yield return FocusRow("laps"); yield return Press(flow.Race.laps < 2 ? GamepadButton.DpadRight : GamepadButton.DpadLeft); }
            Note($"laps {flow.LapLabel}, mode {(race.opponents ? "rivals" : "solo")}, rivals {race.RosterLabel}"); yield return Snap($"race-setup-{course}");
            yield return FocusRow("start"); yield return Press(GamepadButton.South);
            yield return WaitRacing(); Bind(); Note($"racing: {race.Racers.Count} racers ({race.RosterLabel}), the player in the {race.vehicle.GetComponent<VehicleConfiguration>().Profile.Name}");
            BindPad(); var car = race.vehicle; var contacts = car.gameObject.GetComponent<Report106Contacts>() ?? car.gameObject.AddComponent<Report106Contacts>(); contacts.who = "player"; contacts.list.Clear();
            var pts = race.road.points; int n = pts.Length; var s = new float[n + 1]; for (int i = 1; i <= n; i++) s[i] = s[i - 1] + Vector3.Distance(Flat(pts[i % n]), Flat(pts[i - 1])); float L = s[n];
            int idx = 0; { float bd = float.MaxValue; for (int i = 0; i < n; i++) { float d = (Flat(pts[i]) - Flat(car.Body.position)).sqrMagnitude; if (d < bd) { bd = d; idx = i; } } }
            var flights = race.GetComponent<MountainFlights>(); var assists = new List<string>(); float lastMove = Time.time, bestS = 0, t0 = Time.time, airMax = 0, air = 0; int resets = 0; double along = 0;
            int Ahead(int i, float d) { float want = s[i] + d; int j = i; while (s[j] < want) { j++; if (j >= n) { j -= n; want -= L; } } return j; }
            float nextNote = Time.time + 30;
            while (flow.State == RaceFlow.Stage.Racing && Time.time - t0 < 1100 && !race.Racers[0].Progress.Finished)
            {
                var pos = car.Body.position; int best = idx; float bdist = float.MaxValue;
                for (int k = -10; k <= 40; k++) { int i = (idx + k + n) % n; float d = (Flat(pts[i]) - Flat(pos)).sqrMagnitude + Mathf.Max(0, Mathf.Abs(pts[i].y - pos.y) - 4) * 30; if (d < bdist) { bdist = d; best = i; } }
                int moved = (best - idx + n) % n; if (moved < n / 2) { along += s[best] >= s[idx] ? s[best] - s[idx] : s[best] + L - s[idx]; idx = best; }
                if (along > bestS + 3) { bestS = (float)along; lastMove = Time.time; }
                float speed = car.ForwardSpeed;
                if (Time.time >= nextNote) { nextNote = Time.time + 30; var pr = race.Racers[0].Progress; Note($"  t {Time.time - t0:F0} s: {V(pos)} line s {s[idx]:F0}, lap {pr.CompletedLaps + 1}, next CP {pr.NextGate}, {speed:F1} m/s, resets {resets}, fps {1 / Mathf.Max(.001f, Time.smoothDeltaTime):F0}"); }
                if (car.GroundedWheels == 0) { air += Time.fixedDeltaTime; airMax = Mathf.Max(airMax, air); } else air = 0;
                if (Time.time - lastMove > 6 || (Mathf.Sqrt(bdist) > 18 && car.GroundedWheels > 0))
                {
                    assists.Add($"{(Time.time - lastMove > 6 ? "stuck" : "off the line")} at {V(pos)} (line s {s[idx]:F0}, {Time.time - t0:F0} s): the reset button"); resets++;
                    Release(); yield return Press(GamepadButton.North); yield return new WaitForSecondsRealtime(2.5f);
                    { float bd = float.MaxValue; int from = idx; for (int k = -60; k <= 60; k++) { int i = (from + k + n) % n; float d = (Flat(pts[i]) - Flat(car.Body.position)).sqrMagnitude; if (d < bd) { bd = d; idx = i; } } }
                    lastMove = Time.time; continue;
                }
                // the speed from the curvature ahead (6 m/s2 sideways, 5 m/s2 braking); a flight's run-up at its take-off speed, along its axis
                float cap = 34;
                for (int k = 0; k <= 60; k += 15) { int a = Ahead(idx, k), b = Ahead(idx, k + 30), c = Ahead(idx, k + 3); float turn = Vector3.Angle(Flat(pts[b] - pts[a]), Flat(pts[c] - pts[a])); float r = turn < 1 ? 1e4f : 30 / (turn * Mathf.Deg2Rad); cap = Mathf.Min(cap, Mathf.Sqrt(Mathf.Clamp(6 * r, 25, 1150) + 2 * 5 * k)); }
                var target = pts[Ahead(idx, Mathf.Clamp(speed * .9f, 7, 24))];
                float station = race.road.ProjectNear(pos, race.road.Relative(0, 0) + s[idx] * race.road.Length / L, 40, out _);
                var flight = flights ? flights.At(race.road, station) : null;
                if (flight != null) { var axis = Flat(flight.forward).normalized; float al = Vector3.Dot(pos - flight.start, axis); target = flight.start + axis * (al + 20); target.y = pos.y; cap = flight.aiTakeoffSpeed > 0 ? flight.aiTakeoffSpeed : flight.aiEntrySpeed > 0 ? 45 : 32; }
                Drive(car, target, Mathf.Max(cap, 7), 0, 0, 0, 22);
                if (Time.timeScale > 0) yield return new WaitForFixedUpdate(); else yield return null; // the results stop time
            }
            Release(); float total = Time.time - t0;
            var me = race.Racers[0];
            Note($"the pad drove {total:F0} s: finished {me.Progress.Finished}, laps [{string.Join(", ", me.Progress.LapTimes.Select(l => l.ToString("0.0", CultureInfo.InvariantCulture)))}], race {me.Progress.RaceTime(race.Clock):0.0} s + {me.Progress.PenaltySeconds:0} s penalties ({me.Progress.MissedGates} missed), resets {resets}, longest in the air {airMax:F1} s");
            foreach (var a in assists) Note("  assist: " + a);
            foreach (var p in me.Progress.Penalties) Note("  penalty: " + p);
            foreach (var c in contacts.list.Take(15)) Note("  contact: " + c);
            float w = Time.realtimeSinceStartup; while (flow.State != RaceFlow.Stage.Results && Time.realtimeSinceStartup - w < 120) yield return null;
            yield return new WaitForSecondsRealtime(3); Note($"state {flow.State}; results: {race.Standings().Replace("\n", " / ")}"); yield return Snap($"results-{course}");
            foreach (var r in race.Racers.Where(r => r.IsAi)) Note($"  rival {Rp(r)}: {(r.Progress.Finished ? r.ClassifiedTime(race.Clock).ToString("0.0", CultureInfo.InvariantCulture) : r.Dnf ? "DNF" : "unfinished")} laps [{string.Join(", ", r.Progress.LapTimes.Select(l => l.ToString("0.0", CultureInfo.InvariantCulture)))}] resets {r.Recoveries} missed {r.Progress.MissedGates}");
            Note($"records: lap rank {flow.LapRank}, race rank {flow.RaceRank}, category {race.Category}, records track title '{RacePlaylists.Titles[course]}'");
            var ghosts = Path.Combine(flow.Save.DirectoryPath, "CleanLapGhosts"); Note($"ghost files for Leviathan: {(Directory.Exists(ghosts) ? string.Join(", ", Directory.GetFiles(ghosts).Select(Path.GetFileName).Where(f => f.StartsWith("leviathan"))) : "(no folder)")} (ghosts {(flow.Ghost && flow.Ghost.Enabled ? "on" : "off")})");
            if (File.Exists(ResultsLog.FilePath)) { var lines = File.ReadAllLines(ResultsLog.FilePath).Where(l => l.Contains("Leviathan")).ToArray(); Note($"results log lines with Leviathan: {lines.Length}; last: {(lines.Length > 0 ? lines[^1].Substring(0, Mathf.Min(400, lines[^1].Length)) : "-")}"); }
        }

        IEnumerator Split115()
        {
            int course = int.Parse(Arg("-p115course", "8")); Conditions(); Campaign.Testing = true;
            var pad2 = InputSystem.AddDevice<Gamepad>("Report115Pad2");
            SplitScreen.Mode = SplitScreen.Kind.Race; SplitScreen.Solo = false; SplitScreen.P1Device = pad; SplitScreen.P2Device = null; SplitScreen.P2Ai = true; SplitScreen.Course = course; SplitScreen.Laps = 1; SplitScreen.Rivals = 2; SplitScreen.RivalDifficulty = 1; SplitScreen.Traffic = false;
            SplitScreen.P1Vehicle = "original"; SplitScreen.P2Vehicle = "atv"; SplitScreen.P1Name = "Player One"; SplitScreen.P2Pick = "Player Two"; flow.StartSplit();
            yield return new WaitForSecondsRealtime(2); float t1 = Time.realtimeSinceStartup; while (RacePlaylists.CourseOf(UnityEngine.SceneManagement.SceneManager.GetActiveScene()) != course && Time.realtimeSinceStartup - t1 < 90) yield return null;
            yield return WaitRacing(); Bind(); Note($"split-screen race: {race.Racers.Count} racers, {Lev()}"); yield return Snap($"split-{course}");
            Pilot(race.vehicle, race.Racers[0]); Time.timeScale = 4;
            float t0 = Time.realtimeSinceStartup; while (flow.State != RaceFlow.Stage.Results && Time.realtimeSinceStartup - t0 < 600) { AudioListener.volume = 0; yield return null; if (Time.timeScale == 0 && flow.State == RaceFlow.Stage.Racing) Time.timeScale = 4; }
            Time.timeScale = 1; yield return new WaitForSecondsRealtime(2); Note($"split-screen results: state {flow.State}: {race.Standings().Replace("\n", " / ")}"); yield return Snap($"split-results-{course}");
            flow.QuitSplit(false); yield return WaitMenu(); InputSystem.RemoveDevice(pad2);
        }
    }
}
