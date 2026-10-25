using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Racer
{
    // 1.2 HIGH MODE evidence only (the 0.106 check component: -p106check <dir> -racerTestSave <dir>; never active without both):
    //   -p106kind high1200 -p1200part pages      the main menu by the controller only: the HIGH MODE switch, FREE ROAM (1 and 2 players, both modes),
    //                                            CHASES & STUNTS, VEHICLE, TIME & WEATHER, RACE; one shot of each HIGH MODE page (each audited for
    //                                            anything off screen or cut), then RUN FROM THE COPS started from CHASES & STUNTS
    //   -p1200part mountain -p1200course 4|5     one HIGH MODE lap of a mountain course by the pad, steering at the edges on purpose
    //   -p1200part landing                       a crooked landing (upside down) in HIGH MODE, then the same in normal mode
    //   -p1200part records                       a HIGH MODE race to its results: where its times went (HIGH boards only)
    //   -p1200part prompt                        normal mode: forced wrecks bring up the question; No; more wrecks: not asked again
    public sealed partial class Report106PlayerCheck
    {
        IEnumerator High1200()
        {
            string part = Arg("-p1200part", "pages"); Conditions(); flow.Save.Settings.hints = false; flow.Save.SaveSettings();
            Note($"HIGH MODE check part {part}: save {flow.SaveRoot}, HIGH MODE saved {(flow.Save.Settings.highMode ? "on" : "off")}");
            if (part == "pages") yield return Pages1200();
            if (part == "mountain") yield return Mountain1200(int.Parse(Arg("-p1200course", "4")));
            if (part == "landing") yield return Landing1200();
            if (part == "records") yield return Records1200();
            if (part == "prompt") yield return Prompt1200();
            if (part == "split") yield return Split1200();
        }
        // ---- 1.2.0-review2: a split-screen race, player 1 normal, player 2 in HIGH MODE
        IEnumerator Split1200()
        {
            foreach (var m in InputSystem.devices.OfType<Mouse>().ToList()) InputSystem.DisableDevice(m);
            Campaign.Testing = true; yield return QuickTo115(0); Conditions(); if (HighMode.On) flow.SetHighMode(false);
            var pad2 = InputSystem.AddDevice<Gamepad>("Report1200Pad2");
            // the setup by the controller: SPLIT SCREEN, Mode Race, player 2 joins, player 2's HIGH MODE on with A
            Menu.ResetPages(); Menu.Show(); yield return new WaitForSecondsRealtime(1); var path = new List<string>();
            yield return On1200("split", path); SplitScreen.Mode = SplitScreen.Kind.Race; Menu.Show(); yield return new WaitForSecondsRealtime(.6f);
            InputSystem.QueueStateEvent(pad2, new GamepadState().WithButton(GamepadButton.South)); yield return new WaitForSecondsRealtime(.2f); InputSystem.QueueStateEvent(pad2, new GamepadState()); yield return new WaitForSecondsRealtime(1);
            Reach1200("split-screen setup (race, player 2 joined)"); yield return Page1101("split-setup-per-player", 1.2f);
            path.Clear(); yield return On1200("high-p2", path); Note($"  A on player 2's row: P1 HIGH {SplitScreen.P1High}, P2 HIGH {SplitScreen.P2High}; rows '{Btn("high-p1")?.GetComponentInChildren<UnityEngine.UI.Text>(true).text}' / '{Btn("high-p2")?.GetComponentInChildren<UnityEngine.UI.Text>(true).text}'");
            yield return Page1101("split-setup-p2-high", 1f);
            SplitScreen.Laps = 1; SplitScreen.Rivals = 0; SplitScreen.Traffic = false; SplitScreen.Course = 0; SplitScreen.P1Device = pad; SplitScreen.P2Device = pad2; SplitScreen.P2Ai = false;
            flow.StartSplit(); yield return WaitRacing(); Bind(); AudioListener.volume = 0;
            var split = SplitScreen.Race; var root = HighBumpers.Root(race.gameObject.scene); var c1 = race.vehicle; var c2 = split ? split.P2Car : null;
            Note($"split race on {race.courseName}: session HIGH {race.high}, player 1 {race.high1}, player 2 {race.high2}; bumpers {(root && root.activeInHierarchy ? "built" : "MISSING")}, layer {(root && root.transform.childCount > 0 ? root.transform.GetChild(0).gameObject.layer : -1)} (player 1's view draws it: {(Camera.main.cullingMask & (1 << (root ? root.transform.GetChild(0).gameObject.layer : 0))) != 0}, player 2's: {split && split.Camera2 && (split.Camera2.cullingMask & (1 << root.transform.GetChild(0).gameObject.layer)) != 0})");
            Note($"  record boards: player 1 '{split.Category(1)}', player 2 '{split.Category(2)}'");
            yield return new WaitForSecondsRealtime(2); yield return Snap("split-race-halves");
            // each vehicle pushed sideways at 14 m/s toward the road's edge on a straight
            IEnumerator Push(ArcadeVehicle car, float s, string who)
            {
                var at = race.road.At(s, out var f); var right = Vector3.Cross(Vector3.up, Flat(f).normalized); float half = race.road.HalfWidth(s);
                Put(car, at, Yaw(f), 0); var hits = car.gameObject.GetComponent<Bumps1200>() ?? car.gameObject.AddComponent<Bumps1200>(); int h0 = hits.Hits;
                for (int k = 0; k < 20; k++) { car.Body.linearVelocity = right * 14; yield return new WaitForFixedUpdate(); }
                yield return new WaitForSeconds(1.5f);
                float lat = Vector3.Dot(Flat(car.Body.position - at), right);
                Note($"  {who} pushed at the edge (s {s:F0}): ended {lat:F1} m from the centre (the bumper stands at about {half + 1.2f:F1}-{half + 2.2f:F1} m), bumper hits {hits.Hits - h0}, assist {(HighAssist.For(car) ? "on" : "off")} -> {(lat < half + 1.3f ? "STAYED ON" : "WENT PAST")}");
            }
            float s1 = race.Origin + 120, s2 = race.Origin + 220;
            yield return Push(c1, s1, "player 1 (normal)"); yield return Push(c2, s2, "player 2 (HIGH)");
            yield return Snap("split-race-pushed");
            flow.Pause(); yield return new WaitForSecondsRealtime(1); Reach1200("split pause"); yield return Page1101("split-pause-per-player", .5f);
        }
        string Rows1200() => string.Join(" | ", FindObjectsByType<UnityEngine.UI.Button>(FindObjectsSortMode.None).Where(b => b.gameObject.activeInHierarchy && b.transform.IsChildOf(Menu.transform.root) && b.GetComponentInChildren<UnityEngine.UI.Text>(true) && b.name != "Previous value" && b.name != "Next value")
            .OrderBy(b => -b.transform.position.y).Select(b => b.name));
        IEnumerator On1200(string row, List<string> path) { yield return PadTo(row, path); yield return Press1100(GamepadButton.South); yield return new WaitForSecondsRealtime(.8f); }
        void Reach1200(string page) { var miss = Menu.UnreachableRows(); Note($"  {page}: page {Menu.PageName}, rows {Rows1200()}; not reachable by the controller: {(miss.Count == 0 ? "none" : string.Join(", ", miss))}"); }
        IEnumerator Pages1200()
        {
            foreach (var m in InputSystem.devices.OfType<Mouse>().ToList()) InputSystem.DisableDevice(m); // the controller only
            if (flow.State == RaceFlow.Stage.Title) flow.EnterMenuAfterTitle();
            if (HighMode.On) flow.SetHighMode(false);
            Menu.ResetPages(); Menu.Show(); yield return new WaitForSecondsRealtime(1.2f);
            var path = new List<string>();
            // normal mode: the switch on top, FREE ROAM right after RACE asking 1 / 2 players; 2 players is the split-screen setup on Free Roam
            Reach1200("normal main menu"); yield return Page1101("normal-main-menu");
            yield return On1200("roam", path); Reach1200("normal FREE ROAM"); yield return Page1101("normal-free-roam-players");
            yield return On1200("roam-2p", path); var mode = Btn("split-mode")?.GetComponentInChildren<UnityEngine.UI.Text>(true).text; Note($"  2 PLAYERS (normal): page {Menu.PageName}, mode row '{mode}'"); yield return Page1101("normal-free-roam-2p-split-setup");
            yield return Back1101(); yield return Back1101(); yield return Home1101();
            // the switch with A: HIGH MODE on (saved)
            path.Clear(); yield return PadTo("high-mode", path); Note("  controller path to the switch: " + string.Join(" > ", path.Distinct()));
            yield return Press1100(GamepadButton.South); yield return new WaitForSecondsRealtime(1);
            string Saved() { try { return File.ReadAllText(Path.Combine(flow.SaveRoot, "settings.json")).Contains("\"highMode\": true") ? "highMode true" : "highMode false"; } catch { return "unreadable"; } }
            Note($"  A on the switch: HIGH MODE {(HighMode.On ? "ON" : "OFF")}, settings.json {Saved()}, unlock (campaign testing) {Campaign.Testing}, race in HIGH {race.high}");
            Reach1200("HIGH MODE main menu"); yield return Page1101("high-main-menu");
            path.Clear(); yield return On1200("roam", path); Reach1200("HIGH FREE ROAM"); yield return Page1101("high-free-roam-players");
            yield return On1200("roam-2p", path); Reach1200("HIGH FREE ROAM 2 players"); yield return Page1101("high-free-roam-2-players");
            yield return Back1101(); yield return Back1101(); yield return Home1101();
            path.Clear(); yield return On1200("high-vehicle", path); Reach1200("HIGH VEHICLE"); yield return Page1101("high-vehicle", 2f);
            yield return Press1100(GamepadButton.DpadRight); yield return new WaitForSecondsRealtime(1.2f); string picked = Btn("high-vehicle-pick")?.GetComponentInChildren<UnityEngine.UI.Text>(true).text;
            yield return Press1100(GamepadButton.South); yield return new WaitForSecondsRealtime(1);
            Note($"  VEHICLE: right then A on '{picked}': page {Menu.PageName}, saved vehicle {flow.Save.Settings.vehicleId}, driving {race.vehicle.GetComponent<VehicleConfiguration>().profileId}");
            path.Clear(); yield return On1200("high-time", path); Reach1200("HIGH TIME & WEATHER"); yield return Page1101("high-time-weather");
            yield return Press1100(GamepadButton.DpadRight); yield return new WaitForSecondsRealtime(.6f); Note($"  TIME & WEATHER: right on the time row: '{Btn("high-time-of-day")?.GetComponentInChildren<UnityEngine.UI.Text>(true).text}', saved highTime {flow.Save.Settings.highTime}");
            yield return Press1100(GamepadButton.DpadLeft); yield return Back1101(); yield return Home1101();
            path.Clear(); yield return On1200("race", path); yield return new WaitForSecondsRealtime(1.5f); Reach1200("HIGH RACE (tracks)"); yield return Page1101("high-race-tracks", 1.5f); yield return Back1101(); yield return Home1101();
            path.Clear(); yield return On1200("high-chases", path); Reach1200("HIGH CHASES & STUNTS"); yield return Page1101("high-chases-stunts");
            yield return Press1100(GamepadButton.DpadRight); yield return new WaitForSecondsRealtime(.6f); Reach1200("HIGH CHASES & STUNTS, 2 players"); yield return Page1101("high-chases-stunts-2p");
            yield return Press1100(GamepadButton.DpadLeft); yield return new WaitForSecondsRealtime(.6f);
            // RUN FROM THE COPS from here: the Getaway starts in Free Roam's world, in HIGH MODE
            path.Clear(); yield return On1200("high-getaway", path); yield return WaitRacing(); Bind();
            Note($"  RUN FROM THE COPS: scene {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}, getaway {(GetawayChase.Current ? "running" : "NOT running")}, difficulty {SplitScreen.PoliceDifficulty}, {SplitScreen.PoliceMinutes} min, vehicle {race.vehicle.GetComponent<VehicleConfiguration>().profileId}, HIGH {race.high}");
            yield return new WaitForSecondsRealtime(3); yield return Snap("high-getaway-running");
        }
        // ---- a mountain lap, steering at the edges
        IEnumerator Mountain1200(int course)
        {
            Campaign.Testing = true; yield return QuickTo115(course); Conditions();
            flow.Save.Settings.vehicleId = Arg("-p106vehicle", "original"); flow.Save.SaveSettings(); flow.SetHighMode(true);
            flow.StartRace(); yield return WaitRacing(); Bind(); BindPad(); AudioListener.volume = 0;
            var car = race.vehicle; var road = race.road; var me = race.Racers[0]; var respawn = car.GetComponent<VehicleRespawn>();
            var root = HighBumpers.Root(race.gameObject.scene);
            Note($"== HIGH MODE lap of {RacePlaylists.Titles[course]}: race HIGH {race.high}, {race.laps} laps, rivals {race.RivalCount} ({race.DifficultyName}); bumpers {(root && root.activeInHierarchy ? "shown" : "MISSING")}: {HighBumpers.Samples} stations ({HighBumpers.Samples * 2} m of bumper), {HighBumpers.Openings} left open for crossings and shortcuts");
            yield return Snap($"mountain-{course}-start");
            var contacts = car.gameObject.AddComponent<Bumps1200>();
            float station = road.Project(car.Body.position, out _), t0 = Time.time, worst = 0; int resets0 = respawn.Resets, beyond = 0, lap0 = me.Progress.CompletedLaps; float nextShot = 30, lastLog = 0; string worstAt = "";
            var flights = race.GetComponent<MountainFlights>(); Time.timeScale = 2; int pinned = 0; float lastS = station, lastProgress = Time.time;
            var outs = new List<string>(); bool wasOut = false;
            bool toEdge = true; int side = 1, hitsAtTurn = 0, edges = 0; float phaseAt = Time.time; race.finishGraceSeconds = 100000; // the race stays open until this lap is done (Easy rivals finish their laps first)
            while (flow.State == RaceFlow.Stage.Racing && Time.time - t0 < 700 && me.Progress.CompletedLaps == lap0)
            {
                if (Time.timeScale == 0) Time.timeScale = 2;
                var pos = car.Body.position; float s = road.ProjectNear(pos, station, 60, out _); station = s;
                var c = road.At(s, out var f); var right = Vector3.Cross(Vector3.up, Flat(f).normalized); float half = road.HalfWidth(s);
                var branch = me.Branch.Route; float lat = Vector3.Dot(Flat(pos - c), right);
                if (branch) { float bs = branch.Project(pos, out float bl); lat = bl; half = branch.halfWidth; }
                float over = Mathf.Abs(lat) - (half + 2.4f); // past where the bumper stands (HalfWidth + 1.2 m, and its 1 m width)
                if (over > worst) { worst = over; worstAt = $"t {Time.time - t0:F0} s at {V(pos)} line s {s:F0}{(branch ? " on " + branch.title : "")}"; }
                bool outside = over > 0 && car.GroundedWheels >= 2; if (outside) beyond++;
                if (outside && !wasOut) { outs.Add($"s {s:F0} at {V(pos)}{(branch ? " on " + branch.title : "")}"); if (outs.Count <= 6) yield return Snap($"mountain-{course}-outside-{outs.Count}"); } wasOut = outside;
                // the target: 8 s toward one edge, 8 s toward the other (aimed 4 m past the bumper), straight through a jump
                bool flight = flights && flights.At(road, s) != null;
                // as a careless player: steer for one edge (aimed past the bumper) until the bumper is hit (or 6 s), back to the middle for 3 s, then the other edge
                if (toEdge && (contacts.Hits > hitsAtTurn || Time.time - phaseAt > 6)) { toEdge = false; phaseAt = Time.time; }
                else if (!toEdge && Time.time - phaseAt > 3) { toEdge = true; phaseAt = Time.time; side = -side; hitsAtTurn = contacts.Hits; edges++; }
                Vector3 target; float cap;
                if (branch) { float bs = branch.ProjectFor(true, pos, out _); target = branch.AtFor(true, bs + Mathf.Clamp(car.ForwardSpeed * .8f, 6, 18), out _); cap = CurveCap(x => branch.AtFor(true, x, out _), bs, branch.recommendedSpeed + 4); }
                else { target = RoadTarget(s, car.ForwardSpeed, out cap); if (!flight && toEdge) target += right * side * (half + 3); }
                // pinned (no progress for 8 s): the reset button, counted apart from the game's own resets
                float progress = Mathf.Repeat(s - lastS + road.Length * .5f, road.Length) - road.Length * .5f; if (progress > 2 || branch) { lastS = s; lastProgress = Time.time; }
                if (Time.time - lastProgress > 8) { pinned++; Note($"  pinned at {V(pos)} s {s:F0}: the reset button"); Release(); yield return Press(GamepadButton.North); yield return new WaitForSeconds(2); lastProgress = Time.time; station = road.ProjectNear(car.Body.position, station, 120, out _); lastS = station; continue; }
                Drive(car, target, Mathf.Min(Mathf.Max(cap, 7), 22), 0, 0, 0, 22);
                if (Time.time - t0 > nextShot) { nextShot += 60; yield return Snap($"mountain-{course}-t{Time.time - t0:000}"); }
                if (Time.time - lastLog > 60) { lastLog = Time.time; Note($"  t {Time.time - t0:F0} s: s {s:F0} of {road.Length:F0}, bumper hits {contacts.Hits}, resets {respawn.Resets - resets0}, righted {HighAssist.Righted}"); }
                yield return new WaitForFixedUpdate();
            }
            Release(); Time.timeScale = 1;
            Note($"  lap {(me.Progress.CompletedLaps > lap0 ? "COMPLETED in " + me.Progress.LapTimes.Last().ToString("0.0") + " s" : "NOT completed")} ({Time.time - t0:F0} s game time); {edges + 1} runs at an edge, bumper hits {contacts.Hits}; resets {respawn.Resets - resets0 - pinned} by the game (a fall off the course or below the world) and {pinned} by the bot's reset button when pinned; flipped upright {HighAssist.Righted}; missed gates {me.Progress.MissedGates}");
            Note($"  furthest out past the bumper line: {(worst <= 0 ? "never (always inside it)" : worst.ToString("0.0") + " m, " + worstAt)}; physics steps grounded past it: {beyond}; times it got past it: {outs.Count}{(outs.Count > 0 ? " (" + string.Join("; ", outs.Take(12)) + ")" : "")}");
            yield return Snap($"mountain-{course}-end");
        }
        // ---- a crooked landing: HIGH MODE, then normal
        IEnumerator Landing1200()
        {
            Campaign.Testing = true; yield return QuickTo115(0); Conditions();
            foreach (bool high in new[] { true, false })
            {
                flow.Save.Settings.vehicleId = "moto"; flow.Save.SaveSettings(); flow.SetHighMode(high);
                var st = flow.Save.Settings; st.opponents = false; st.traffic = false; st.laps = 1; flow.Save.SaveSettings(); race.opponents = false; race.traffic = false; race.laps = 1;
                flow.StartRace(); yield return WaitRacing(); Bind(); BindPad(); Release();
                var car = race.vehicle; var cfg = car.GetComponent<VehicleConfiguration>(); int wipe0 = cfg.Wipeouts, right0 = HighAssist.Righted;
                float s = race.Origin + 40; var at = race.road.At(s, out var f);
                Put(car, at, Yaw(f), 0); car.Body.position += Vector3.up * 3; car.Body.rotation = Quaternion.Euler(0, Yaw(f), 170); car.transform.SetPositionAndRotation(car.Body.position, car.Body.rotation); car.Body.linearVelocity = Flat(f).normalized * 6; Physics.SyncTransforms();
                yield return new WaitForSeconds(.6f); yield return Snap($"landing-{(high ? "high" : "normal")}-falling");
                float t0 = Time.time; string trace = "";
                while (Time.time - t0 < 4) { yield return new WaitForSeconds(.5f); trace += $" {Time.time - t0:F1}s up {car.transform.up.y:F2}{(cfg.WipedOut ? " WRECKED" : "")};"; }
                Note($"{(high ? "HIGH MODE" : "normal mode")}: {race.vehicle.GetComponent<VehicleConfiguration>().profileId} dropped upside down from 3 m: wrecks {cfg.Wipeouts - wipe0}, flipped upright {HighAssist.Righted - right0}, now up {car.transform.up.y:F2}, wrecked {cfg.WipedOut};{trace}");
                yield return Snap($"landing-{(high ? "high" : "normal")}-after");
                flow.Pause(); yield return new WaitForSecondsRealtime(.5f); flow.QuitRace(); yield return WaitMenu();
            }
        }
        // ---- a HIGH MODE race's times land on the HIGH boards only
        IEnumerator Records1200()
        {
            Campaign.Testing = true; yield return QuickTo115(0); Conditions();
            string Hash(string f) => Hash1100(Path.Combine(flow.SaveRoot, f));
            string camp0 = Hash("campaign-v1.json"), stunt0 = Hash("stunt-park-v1.json"), police0 = Hash("police-progress-v1.json");
            string boards = Path.Combine(flow.SaveRoot, "top-ten-v1.json");
            int Count(bool high) { try { var d = JsonUtility.FromJson<RecordBoards.Data>(File.ReadAllText(boards)); return d.entries.Count(e => e.category.StartsWith(HighMode.RecordPrefix) == high); } catch { return -1; } }
            int normal0 = Count(false), high0 = Count(true);
            flow.SetHighMode(true); flow.StartRace(); yield return RaceToResults(1.15f); Bind();
            Note($"HIGH MODE race on {race.courseName}: category {race.Category}; lap rank {flow.LapRank}, race rank {flow.RaceRank}; results title '{FindObjectsByType<UnityEngine.UI.Text>(FindObjectsSortMode.None).FirstOrDefault(t => t.name == "Title" && t.isActiveAndEnabled)?.text}'");
            Note($"  Top 10 file: normal entries {normal0} -> {Count(false)}, HIGH entries {high0} -> {Count(true)}");
            Note($"  campaign save unchanged {camp0 == Hash("campaign-v1.json")}, stunt park save unchanged {stunt0 == Hash("stunt-park-v1.json")}, police progress unchanged {police0 == Hash("police-progress-v1.json")}");
            var kept = ReplayData.List(true).Where(x => x.head.category.StartsWith(HighMode.RecordPrefix)).Select(x => x.head.Title).Distinct().ToList(); Note($"  #1 replays kept from it: {(kept.Count == 0 ? "none" : string.Join(", ", kept))}");
            yield return Snap("records-high-results");
            flow.OpenBoards(); yield return new WaitForSecondsRealtime(1.5f); Note($"  RECORDS from the results: opens on tab {(Menu.PageName)}, rows {Rows1200()}"); yield return Snap("records-high-tab");
            yield return Press1100(GamepadButton.LeftShoulder); yield return new WaitForSecondsRealtime(1); // back to COMBOS, then the normal LAP board of the same track
            for (int i = 0; i < 6; i++) { yield return Press1100(GamepadButton.LeftShoulder); yield return new WaitForSecondsRealtime(.4f); }
            Note($"  the normal LAP tab: '{Btn("record-track")?.GetComponentInChildren<UnityEngine.UI.Text>(true).text}', table rows {FindObjectsByType<UnityEngine.UI.Button>(FindObjectsSortMode.None).Count(b => b.gameObject.activeInHierarchy && b.name.StartsWith("record-") && b.name != "record-track" && b.name != "record-vehicle")}");
            yield return Snap("records-normal-lap-tab");
        }
        // ---- the question after forced wrecks (normal mode); No; more wrecks: not asked again
        IEnumerator Prompt1200()
        {
            Campaign.Testing = true; yield return QuickTo115(0); Conditions(); flow.SetHighMode(false);
            flow.Save.Settings.vehicleId = "moto"; var st = flow.Save.Settings; st.opponents = false; st.traffic = false; st.laps = 3; flow.Save.SaveSettings(); race.opponents = false; race.traffic = false; race.laps = 3;
            flow.StartRace(); yield return WaitRacing(); Bind(); BindPad(); Release();
            var car = race.vehicle; var cfg = car.GetComponent<VehicleConfiguration>(); var respawn = car.GetComponent<VehicleRespawn>();
            int asked0 = HighPrompt.Asked;
            IEnumerator Wreck(int n)
            {
                var at = race.road.At(race.Origin + 30 + n * 25, out var f); Put(car, at, Yaw(f), 0); car.Body.position += Vector3.up * 2; car.Body.rotation = Quaternion.Euler(0, Yaw(f), 175); car.transform.SetPositionAndRotation(car.Body.position, car.Body.rotation); Physics.SyncTransforms();
                float t0 = Time.time; while (Time.time - t0 < 3 && !cfg.WipedOut && flow.State == RaceFlow.Stage.Racing) yield return null;
                bool wrecked = cfg.WipedOut; if (flow.State == RaceFlow.Stage.Racing) { respawn.ResetVehicle(); float r0 = Time.realtimeSinceStartup; while (Time.realtimeSinceStartup - r0 < 1.5f) yield return null; }
                Note($"  forced wreck {n}: wrecked {wrecked}, reset, state {flow.State}, asked {HighPrompt.Asked - asked0}, page {Menu.PageName}");
                yield return new WaitForSecondsRealtime(1); // the question pauses the game, so real time
            }
            for (int n = 1; n <= 3 && HighPrompt.Asked == asked0; n++) yield return Wreck(n);
            var titleText = FindObjectsByType<UnityEngine.UI.Text>(FindObjectsSortMode.None).FirstOrDefault(t => t.name == "Title" && t.isActiveAndEnabled)?.text;
            Note($"the question: asked {HighPrompt.Asked - asked0} time(s), state {flow.State}, title '{titleText}', rows {Rows1200()}, focus {Selected}");
            yield return Snap("prompt-question");
            yield return Press1100(GamepadButton.East); yield return new WaitForSecondsRealtime(1);
            Note($"B (No): state {flow.State}, HIGH MODE {(HighMode.On ? "ON" : "off")}, notice '{flow.Notice}', not asked again this session {HighMode.Declined}");
            yield return Snap("prompt-after-no");
            int asked1 = HighPrompt.Asked; for (int n = 4; n <= 7; n++) yield return Wreck(n);
            Note($"4 more forced wrecks after No: asked {HighPrompt.Asked - asked1} more time(s), state {flow.State}");
        }
    }
    // checks only: hits against the HIGH MODE bumpers
    public sealed class Bumps1200 : MonoBehaviour
    {
        public int Hits;
        void OnCollisionEnter(Collision c) { if (HighBumpers.IsBumper(c.collider)) Hits++; }
    }
}
