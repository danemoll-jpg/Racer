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
    // 0.119 evidence only (the 0.106 check component: -p106check <dir> -racerTestSave <dir>; never active without both):
    //   -p106kind shots119   Leviathan (-p115course 8 / 9), solo, time of day -p119time (0 Day .. 2 Night): each of -p118spots "name|x|z|yaw|station;..."
    //                        a driver's-view shot (as shots118), the closures within 60 m and the next gate's place from the spot
    //   -p106kind roam119    Free Roam at -p119hour (23: night, clear): each of -p119spots "name|x|z|yaw" a driver's-view shot
    //   -p106kind stunt119   a quick Stunt Race (-p119course 4 / 5 / 8 / 9, -p119split 0|1, -p115laps, -p115diff): the player's Clay Hopper driven by the
    //                        road AI with its tricks turned by RivalTricks (scored by the player's own trick scoring), every rival on a stunt vehicle; the
    //                        trick bonus or stunt points, each rival's flights, tricks, bails and resets; a shot of the HUD line when a bonus lands and
    //                        of a rival's trick over it; the results; the Top 10 categories before and after (the normal boards untouched)
    //   -p106kind campaign119 the Stunt chapter from a test save that has just passed the Summit Final: chapter 5 open and the Clay Hopper given; its
    //                        first event (a Stunt Race, split score) and second (Summit Tricks) and the final, driven by the road AI; the results; the
    //                        prize message
    //   -p106kind menus119   the controller only: Race Setup > Race type to Stunt Race (and back), Scoring, Tracks (the four), the garage (stunt
    //                        vehicles), Records (a Stunt Race track), the split-screen setup's Race type; B back each time
    public sealed partial class Report106PlayerCheck
    {
        IEnumerator LevSolo119(int course, int time)
        {
            Campaign.Testing = true; Conditions(time); yield return QuickTo115(course); Conditions(time);
            flow.OpenGarage(); flow.SelectVehicle(Arg("-p106vehicle", "original")); flow.CloseGarage();
            SoloStart(1); yield return WaitRacing(); Bind(); BindPad(); AudioListener.volume = 0;
        }
        IEnumerator Shots119()
        {
            int course = int.Parse(Arg("-p115course", "8")), time = int.Parse(Arg("-p119time", "0")); yield return LevSolo119(course, time);
            var car = race.vehicle; var chase = FindAnyObjectByType<ChaseCamera>();
            Note($"== spots on {RacePlaylists.Titles[course]} ({(TimeOfDay)time}): {Lev()}");
            foreach (var spec in Arg("-p118spots", "").Split(';').Where(x => x.Length > 0))
            {
                var f = spec.Split('|'); var pos = new Vector3(F(f[1]), 0, F(f[2])); float yaw = F(f[3]), s = F(f[4]);
                pos.y = race.road.At(s, out _).y + 4; Put(car, pos, yaw, 0); car.Body.isKinematic = true; ProgressTo(s, car.Body.position); chase?.Snap();
                for (int k = 0; k < 20; k++) yield return null; yield return new WaitForSecondsRealtime(.6f);
                yield return Snap("spot-" + f[0]);
                var near = FindObjectsByType<Transform>(FindObjectsSortMode.None).Where(t => t.name.StartsWith("Leviathan road closed") && Vector3.Distance(Flat(t.position), Flat(pos)) < 60).Select(t => $"{t.name} at {V(t.position)} ({Vector3.Distance(Flat(t.position), Flat(pos)):F0} m)");
                int next = race.Racers[0].Progress.NextGate; var g = next > 0 ? race.gates[next] : null; string gate = g ? $"CP {next} '{g.name}' at {V(g.transform.position)} faces {g.transform.eulerAngles.y:F0}, {Vector3.Distance(Flat(g.transform.position), Flat(car.Body.position)):F0} m at bearing {Yaw(Flat(g.transform.position - car.Body.position)):F0} (heading {yaw:F0}: {Mathf.DeltaAngle(yaw, Yaw(Flat(g.transform.position - car.Body.position))):F0} off)" : "finish";
                Note($"  {f[0]}: at {V(car.Body.position)} heading {yaw:F0}, line s {s:F0}, next {gate}, gates drawn [{GatesDrawn()}], HUD '{HudLine()}'; closures within 60 m: {(near.Any() ? string.Join("; ", near) : "none")}");
                car.Body.isKinematic = false;
            }
        }
        IEnumerator Roam119()
        {
            yield return ToFreeRoam(Arg("-p106vehicle", "original")); var car = race.vehicle; float hour = F(Arg("-p119hour", "23"));
            WorldLook.Current?.SetClock(WorldLook.Current.Day, hour); yield return new WaitForSecondsRealtime(2);
            foreach (var spec in Arg("-p119spots", "").Split(';').Where(x => x.Length > 0))
            {
                var f = spec.Split('|'); Put(car, new Vector3(F(f[1]), 0, F(f[2])), F(f[3]), 0); car.Body.isKinematic = true; FindAnyObjectByType<ChaseCamera>()?.Snap();
                for (int k = 0; k < 20; k++) yield return null; yield return new WaitForSecondsRealtime(1.5f);
                yield return Snap("roam-" + f[0]); Note($"  {f[0]}: Free Roam at {V(car.Body.position)} heading {F(f[3]):F0}, {WorldLook.Current?.Clock} ({WorldLook.Current?.Mode})");
                car.Body.isKinematic = false;
            }
        }
        // ---------------------------------------------------------------- Stunt Races
        IEnumerator StuntQuick(int course, bool split, int laps, int diff)
        {
            yield return QuickTo115(course); Conditions();
            var st = flow.Save.Settings; st.stuntSplitScore = split; flow.Save.SaveSettings();
            if (!race.stunts) flow.StepRaceType(1); if (race.splitScore != split) flow.StepStuntScoring(1);
            flow.OpenGarage(); flow.SelectVehicle("stuntbuggy"); flow.CloseGarage();
            st.opponents = true; st.traffic = false; st.laps = laps; st.difficulty = diff; st.estimateAiFinishes = false; flow.Save.SaveSettings();
            race.opponents = true; race.traffic = false; race.laps = laps; race.difficulty = diff;
            flow.StartRace();
        }
        IEnumerator Stunt119()
        {
            Campaign.Testing = true; DriverVariation.Seed = System.Environment.TickCount; Conditions();
            int course = int.Parse(Arg("-p119course", "4")), laps = int.Parse(Arg("-p115laps", "1")), diff = int.Parse(Arg("-p115diff", "1")); bool split = Arg("-p119split", "0") == "1";
            var before = new RecordBoards(flow.Save.DirectoryPath); var catsBefore = Cats(before); string normalBefore = Normal(before);
            yield return StuntQuick(course, split, laps, diff);
            yield return WaitRacing(); Bind();
            Note($"== Stunt Race on {RacePlaylists.Titles[course]}: {race.courseName} ({race.RecordCourse}), stunts {race.stunts}, split score {race.splitScore}, laps {laps}, difficulty {race.DifficultyName}; player {race.vehicle.GetComponent<VehicleConfiguration>().profileId}; rivals {string.Join(", ", race.Racers.Where(r => r.IsAi).Select(Rp))}; category {race.Category}; flights {string.Join(", ", race.GetComponent<MountainFlights>()?.flights.Select(f => f.name) ?? new string[0])}");
            var me = race.Racers[0]; var car = race.vehicle; Pilot(car, me); var steer = car.gameObject.AddComponent<RivalTricks>(); steer.Initialize(race, 9); steer.Bind(me); steer.SteerOnly = true;
            race.finishGraceSeconds = 100000;
            var ai = race.Racers.Where(r => r.IsAi).ToList(); var reset0 = ai.ToDictionary(r => r, r => r.Recoveries); var resetAt = ai.ToDictionary(r => r, r => new List<string>());
            bool flashShot = false, labelShot = false; int combos = 0; string lastFlash = null; var flashes = new List<string>();
            Time.timeScale = 4; float t0 = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup - t0 < 1500 && flow.State == RaceFlow.Stage.Racing && !(me.Progress.Finished && ai.All(r => r.Progress.Finished || r.Dnf)))
            {
                AudioListener.volume = 0; yield return null; if (Time.timeScale == 0) Time.timeScale = 4;
                float clock = (float)race.Progress.RaceTime(race.Clock); var score = StuntRaceScore.Current;
                foreach (var r in ai) if (r.Recoveries - reset0[r] > resetAt[r].Count) resetAt[r].Add($"[{r.Car.GetComponent<RoadDriver>()?.LastRecoveryWhy}] {V(r.Car.Body.position)} road {r.RoadPosition:F0} t {clock:F0}");
                string flash = score ? score.Flash(me) : null;
                if (flash != null && flash != lastFlash) { flashes.Add($"t {clock:F0} '{flash}' (bonus {me.Progress.BonusSeconds:0.0} s, {me.Progress.StuntPoints} pts)"); if (!flashShot) { flashShot = true; Time.timeScale = 1; yield return Snap("stunt-hud-bonus-" + course); Time.timeScale = 4; } }
                lastFlash = flash;
                if (!labelShot && ai.Any(r => { var t = r.Car.GetComponent<RivalTricks>(); return t && Time.time < t.LineUntil && t.LastLine != "" && Vector3.Distance(Camera.main.transform.position, r.Car.Body.position) < 60; }))
                { labelShot = true; Time.timeScale = 1; yield return Snap("stunt-rival-trick-" + course); Time.timeScale = 4; }
                if (me.Progress.Finished && ai.Any(r => !r.Progress.Finished && !r.Dnf) && clock > 1200) break;
            }
            Time.timeScale = 1; combos = me.Progress.StuntCombos;
            Note($"  player: {(me.Progress.Finished ? me.ClassifiedTime(race.Clock).ToString("0.0", CultureInfo.InvariantCulture) : "unfinished")} s (race {me.Progress.RaceTime(race.Clock):0.0} − bonus {me.Progress.BonusSeconds:0.0}); {me.Progress.StuntPoints} stunt pts in {combos} combos, best {me.Progress.BestCombo}; own tricks turned: {steer.Tricks}; recoveries {me.Recoveries}");
            Note("  player flights: " + steer.Log.Replace("\n", " | "));
            Note("  bonus lines shown: " + string.Join("; ", flashes.Take(20)));
            foreach (var r in ai.OrderBy(r => r.Progress.Finished ? r.ClassifiedTime(race.Clock) : 1e9))
            {
                var t = r.Car.GetComponent<RivalTricks>();
                Note(string.Format(CultureInfo.InvariantCulture, "  rival {0}: {1} (bonus {2:0.0} s, {3} pts) laps [{4}] resets {5}{6}; flights {7}, tricks {8}, bails {9}", Rp(r), r.Progress.Finished ? r.ClassifiedTime(race.Clock).ToString("0.0", CultureInfo.InvariantCulture) : r.Dnf ? "DNF" : "unfinished", r.Progress.BonusSeconds, r.Progress.StuntPoints,
                    string.Join(", ", r.Progress.LapTimes.Select(l => l.ToString("0.0", CultureInfo.InvariantCulture))), r.Recoveries - reset0[r], resetAt[r].Count > 0 ? " at " + string.Join("; ", resetAt[r]) : "", t ? t.Flights : -1, t ? t.Tricks : -1, t ? t.Bails : -1));
                if (t) Note("    " + t.Log.Replace("\n", " | "));
            }
            if (race.splitScore && StuntRaceScore.Current) Note("  split score: " + string.Join("; ", StuntRaceScore.Current.SplitTable(true).Select(l => $"{(l.racer.IsAi ? Rp(l.racer) : "PLAYER")} finish {l.finish} stunt {l.stunt} total {l.total}")));
            race.finishGraceSeconds = 0; float w = Time.realtimeSinceStartup; while (flow.State != RaceFlow.Stage.Results && Time.realtimeSinceStartup - w < 40) yield return null;
            yield return new WaitForSecondsRealtime(1.5f); yield return Snap("stunt-results-" + course);
            Note("  results: " + Texts("1", "2", "3", "4", "Place").Replace("\n", " "));
            var after = new RecordBoards(flow.Save.DirectoryPath); var catsAfter = Cats(after);
            Note($"  Top 10 categories: before {catsBefore.Count}, after {catsAfter.Count}; new: {string.Join(", ", catsAfter.Except(catsBefore))}; the normal boards' entries {(Normal(after) == normalBefore ? "unchanged" : "CHANGED")}");
            Note($"  best combo record: {StuntRaceRecords.Best(StuntRace.RecordPrefix + race.courseId)?.points ?? 0}");
        }
        static List<string> Cats(RecordBoards b) => b.Categories(false).Concat(b.Categories(true)).Distinct().ToList();
        static string Normal(RecordBoards b) => string.Join("|", b.Categories(false).Where(c => !c.StartsWith(StuntRace.RecordPrefix)).SelectMany(c => b.Board(c, false).Select(e => e.id)).Concat(b.Categories(true).Where(c => !c.StartsWith(StuntRace.RecordPrefix)).SelectMany(c => b.Board(c, true).Select(e => e.id))));
        // ---------------------------------------------------------------- the Stunt chapter
        IEnumerator Campaign119()
        {
            Campaign.Testing = false; flow.Save.Settings.unlockEverything = false; flow.Save.SaveSettings(); DriverVariation.Seed = System.Environment.TickCount; Conditions();
            // the test save as a player's who has just passed the Summit Final (chapter 4's final), before this build: read again as at a start
            var c = Campaign.Current; var c4 = CampaignData.Find("c4-summit-final");
            if (Campaign.ResultOf(c4.Id)?.passed != true) c.results.Add(new Campaign.Result { id = c4.Id, runs = 1, passed = true, bestPlace = 1, field = 6 });
            c.chapter = 4; c.owned.Remove("stuntbuggy"); c.results.RemoveAll(r => r.id.StartsWith("c5-")); c.owned.Remove(VehicleProfile.StuntCupId); Campaign.Save(); Campaign.Load(flow.Save.DirectoryPath); c = Campaign.Current;
            Note($"== campaign (test save): chapter {c.chapter} of {CampaignData.Chapters.Length}, owned [{string.Join(", ", c.owned)}], load notice: {Campaign.LoadNotice ?? "none"}");
            Note($"  chapter 5 open: {Campaign.ChapterOpen(5)}; Clay Hopper owned: {Campaign.Owns("stuntbuggy")}; events: {string.Join(", ", CampaignData.InChapter(5).Select(e => e.Name + (Campaign.Available(e) ? "" : " (locked)")))}");
            Menu.OpenCampaign(5); yield return new WaitForSecondsRealtime(1.5f); yield return Snap("campaign-chapter5");
            foreach (var id in Arg("-p119events", "c5-stunt-opener,c5-summit-tricks,c5-park-session,c5-stunt-final").Split(','))
            {
                var e = CampaignData.Find(id); if (e == null) { Note("no event " + id); continue; }
                foreach (var before in CampaignData.InChapter(5).TakeWhile(x => x != e)) if (!Campaign.Passed(before)) { Campaign.Selected = before.Id; Note("  (marked passed first: " + Campaign.DebugMarkWon() + ")"); }
                yield return CampaignRun119(e);
                // the next event needs this one passed: an AI-driven run that did not pass it is marked passed (F6's "mark event won") to go on
                if (!Campaign.Passed(e) && !e.Final) { Campaign.Selected = e.Id; Note("  (marked passed to go on: " + Campaign.DebugMarkWon() + ")"); }
            }
            // the prize message (the final passed: committed as a pass if the AI-driven run did not pass it)
            var final = CampaignData.Find("c5-stunt-final");
            if (!Campaign.Owns(VehicleProfile.StuntCupId)) { var o = Campaign.Commit(new Campaign.Outcome { Event = final, Vehicle = "stuntbuggy", Place = 1, Field = 6, Time = 600 }); Note($"  final committed as passed for the prize message: passed {o.Passed}, prize {o.PrizeWon}"); }
            Menu.ResetPages(); flow.RefreshMenu(); yield return new WaitForSecondsRealtime(2.5f); yield return Snap("campaign-prize-message");
            Note($"  prize: owned {Campaign.Owns(VehicleProfile.StuntCupId)}, unlock panel: {Texts("CONGRATULATIONS", "You unlocked").Replace("\n", " ")}; Locked(hoppercup) {VehicleUnlocks.Locked(VehicleProfile.Find(VehicleProfile.StuntCupId))}");
        }
        IEnumerator CampaignRun119(CampaignEvent e)
        {
            Note($"-- {e.Name} ({e.Kind}{(e.SplitScore ? ", split score" : "")}) on {e.CourseTitle}: available {Campaign.Available(e)}");
            if (e.Kind == CampaignEventKind.Park) StuntEvent.TestLimit = 40; // checks only: a 40-second round
            flow.StartCampaignEvent(e, "stuntbuggy"); yield return new WaitForSecondsRealtime(2); yield return WaitRacing(); Bind();
            var me = race.Racers[0]; var car = race.vehicle; var steer = car.gameObject.AddComponent<RivalTricks>(); steer.Initialize(race, 9); steer.Bind(me); steer.SteerOnly = true;
            if (e.Kind != CampaignEventKind.Park) Pilot(car, me); else Note($"  the park event: scene {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}, split {SplitScreen.Active} {SplitScreen.Mode} solo {SplitScreen.Solo}, stunt event {(StuntEvent.Current ? StuntEvent.Current.State.ToString() : "none")}, vehicle {car.GetComponent<VehicleConfiguration>().profileId}, tricks on {flow.Activities.StuntsOn}");
            race.finishGraceSeconds = 100000; Note($"  stunts {race.stunts}, split {race.splitScore}, player {car.GetComponent<VehicleConfiguration>().profileId}, rivals {string.Join(", ", race.Racers.Where(r => r.IsAi).Select(Rp))}, laps {race.laps}");
            var watch = flow.GetComponent<CampaignTrapWatch>(); int tries = 0;
            Time.timeScale = 4; float t0 = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup - t0 < 1500 && flow.State == RaceFlow.Stage.Racing && !CampaignRun.Done)
            {
                AudioListener.volume = 0; yield return null; if (Time.timeScale == 0) Time.timeScale = 4;
                watch = flow.GetComponent<CampaignTrapWatch>();
                // a timed stunt event: after each try, back to the run-up (as the reset button does)
                if (watch && CampaignRun.Attempts.Count > tries)
                {
                    tries = CampaignRun.Attempts.Count; yield return new WaitForSeconds(1.5f);
                    if (CampaignRun.RunUpStart(race, out var p, out var q) && CampaignRun.StartStation(race, out float s)) { car.Body.position = p; car.Body.rotation = q; car.transform.SetPositionAndRotation(p, q); car.Body.linearVelocity = car.Body.angularVelocity = Vector3.zero; car.GetComponent<RoadDriver>()?.Place(s, 0); Note($"  try {tries}: {CampaignRun.Attempts.LastOrDefault()?.Value:0} ({CampaignRun.Attempts.LastOrDefault()?.Reason ?? "scored"}); back to the run-up"); }
                }
                if (race.Progress.Finished && race.Racers.Where(r => r.IsAi).All(r => r.Progress.Finished || r.Dnf)) { race.FinalizeUnfinishedAi(); }
            }
            Time.timeScale = 1; race.finishGraceSeconds = 0; float w = Time.realtimeSinceStartup; while (flow.State != RaceFlow.Stage.Results && Time.realtimeSinceStartup - w < 60) yield return null;
            yield return new WaitForSecondsRealtime(2); yield return Snap("campaign-results-" + e.Id);
            StuntEvent.TestLimit = -1;
            var o = CampaignRun.Last; Note($"  result: {(o == null ? "none" : $"{o.Headline}; passed {o.Passed}; pay {o.Pay}; medal {o.Medal}; score {o.Score}; unlocked [{string.Join(", ", o.Unlocked)}]")}; player stunt pts {me.Progress.StuntPoints}; own tricks {steer.Tricks}; {steer.Log.Replace("\n", " | ")}");
            if (race.splitScore && StuntRaceScore.Current) Note("  split score: " + string.Join("; ", StuntRaceScore.Current.SplitTable(true).Select(l => $"{(l.racer.IsAi ? l.racer.Name + " " + Rp(l.racer) : "PLAYER")} finish {l.finish} stunt {l.stunt} ({l.racer.Progress.StuntPoints} pts) total {l.total}")));
            flow.QuitRace(); yield return WaitMenu(); Bind();
            Note($"  after Continue: scene {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}, page '{Page}', campaign event {(CampaignRun.Active?.Id ?? "none")}, split {SplitScreen.Active}");
        }
        // ---------------------------------------------------------------- the menus with the controller only
        IEnumerator Menus119()
        {
            Campaign.Testing = true; Conditions(); var st = flow.Save.Settings; st.stuntRace = false; flow.Save.SaveSettings();
            yield return QuickTo115(0); Menu.ResetPages(); flow.RefreshMenu(); yield return new WaitForSecondsRealtime(1);
            Note($"start: scene {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}, page '{Page}', focus '{Focus}'");
            yield return FocusRow("race"); yield return Press(GamepadButton.South); yield return new WaitForSecondsRealtime(1);
            yield return FocusRow("race-type"); Note($"Race Setup: focus '{Focus}': {Texts("Race type")}"); yield return Snap("menu-race-normal");
            InputSystem.QueueStateEvent(pad, new GamepadState { leftStick = new Vector2(1, 0) }); yield return new WaitForSecondsRealtime(.15f); Release(); // right: Stunt Race (loads a Stunt Race course)
            float t1 = Time.realtimeSinceStartup; yield return new WaitForSecondsRealtime(2); while (Time.realtimeSinceStartup - t1 < 90 && !StuntRace.Scene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name)) yield return null;
            yield return WaitMenu(); Bind(); yield return new WaitForSecondsRealtime(1);
            if (Page.Contains("modal")) { Note($"a notice is up ({Texts("CAMPAIGN UPDATED", "Chapter")}): A"); yield return Press(GamepadButton.South); yield return new WaitForSecondsRealtime(1); }
            if (Page.Contains("unlock")) { Note($"an unlock panel is up: A"); yield return Press(GamepadButton.South); yield return new WaitForSecondsRealtime(1); }
            Note($"after right on Race type: scene {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}, page '{Page}', stunts {race.stunts}, focus '{Focus}', vehicle {race.vehicle.GetComponent<VehicleConfiguration>().profileId}, rivals {string.Join(", ", race.opponentRoster)}");
            yield return Snap("menu-race-stunt");
            yield return FocusRow("stunt-scoring"); Note($"focus '{Focus}': {Texts("Scoring")}"); InputSystem.QueueStateEvent(pad, new GamepadState { leftStick = new Vector2(1, 0) }); yield return new WaitForSecondsRealtime(.15f); Release(); yield return new WaitForSecondsRealtime(.6f); Note($"after right: {Texts("Scoring")}, split {race.splitScore}");
            InputSystem.QueueStateEvent(pad, new GamepadState { leftStick = new Vector2(-1, 0) }); yield return new WaitForSecondsRealtime(.15f); Release(); yield return new WaitForSecondsRealtime(.6f);
            yield return FocusRow("tracks"); yield return Press(GamepadButton.South); yield return new WaitForSecondsRealtime(1.2f);
            Note($"Tracks: {string.Join(", ", FindObjectsByType<UnityEngine.UI.Button>(FindObjectsSortMode.None).Where(b => b.isActiveAndEnabled && b.name.StartsWith("course-")).OrderBy(b => b.transform.GetSiblingIndex()).Select(b => b.GetComponentInChildren<UnityEngine.UI.Text>().text.Split('·')[0].Trim()))}");
            yield return Snap("menu-tracks-stunt"); yield return Press(GamepadButton.East); yield return new WaitForSecondsRealtime(.8f); Note($"B: state {flow.State}, page '{Page}'");
            yield return FocusRow("garage"); yield return Press(GamepadButton.South); yield return new WaitForSecondsRealtime(1.2f);
            Note($"Garage: {Texts("Clay", "Skywhip")}"); yield return Snap("menu-garage-stunt");
            for (int i = 0; i < 3; i++) { InputSystem.QueueStateEvent(pad, new GamepadState { leftStick = new Vector2(1, 0) }); yield return new WaitForSecondsRealtime(.15f); Release(); yield return new WaitForSecondsRealtime(.5f); Note($"  right: {Texts("Clay", "Skywhip", "Needle", "Street")}"); }
            yield return Press(GamepadButton.East); yield return new WaitForSecondsRealtime(.8f); Note($"B: state {flow.State}, page '{Page}', vehicle {race.vehicle.GetComponent<VehicleConfiguration>().profileId}, saved normal vehicle {flow.Save.Settings.vehicleId}, stunt vehicle {flow.Save.Settings.stuntVehicleId}");
            yield return FocusRow("opponents"); Note($"rivals row: {Texts("Rivals")}");
            yield return Press(GamepadButton.East); yield return new WaitForSecondsRealtime(.8f); Note($"B: page '{Page}'");
            yield return FocusRow("records"); yield return Press(GamepadButton.South); yield return new WaitForSecondsRealtime(1.2f);
            var seen = new List<string>(); bool shot = false;
            for (int i = 0; i < 16; i++)
            {
                var row = FindObjectsByType<UnityEngine.UI.Button>(FindObjectsSortMode.None).FirstOrDefault(b => b.isActiveAndEnabled && b.name == "record-track"); string t = row ? row.GetComponentInChildren<UnityEngine.UI.Text>().text : "(no track row)"; seen.Add(t);
                if (!shot && t.StartsWith("Stunt Race")) { shot = true; yield return Snap("menu-records-stunt"); Note("  " + Texts("Best combo", "BEST", "No ").Replace("\n", " / ")); }
                yield return FocusRow("record-track"); InputSystem.QueueStateEvent(pad, new GamepadState { leftStick = new Vector2(1, 0) }); yield return new WaitForSecondsRealtime(.15f); Release(); yield return new WaitForSecondsRealtime(.5f);
            }
            Note($"Records tracks (right, from the first): {string.Join(" | ", seen)}");
            yield return Press(GamepadButton.East); yield return new WaitForSecondsRealtime(.8f);
            yield return FocusRow("split"); yield return Press(GamepadButton.South); yield return new WaitForSecondsRealtime(1.2f);
            yield return FocusRow("split-race-type"); Note($"Split setup: focus '{Focus}': {Texts("Race type")}");
            InputSystem.QueueStateEvent(pad, new GamepadState { leftStick = new Vector2(1, 0) }); yield return new WaitForSecondsRealtime(.15f); Release(); yield return new WaitForSecondsRealtime(.6f);
            Note($"after right: {Texts("Race type", "Course", "Scoring")}; P1 {SplitScreen.P1Vehicle}, P2 {SplitScreen.P2Vehicle}"); yield return Snap("menu-split-stunt");
            InputSystem.QueueStateEvent(pad, new GamepadState { leftStick = new Vector2(1, 0) }); yield return new WaitForSecondsRealtime(.15f); Release(); yield return new WaitForSecondsRealtime(.6f);
            yield return Press(GamepadButton.East); yield return new WaitForSecondsRealtime(.8f); Note($"B: page '{Page}'");
            yield return FocusRow("race"); yield return Press(GamepadButton.South); yield return new WaitForSecondsRealtime(1);
            yield return FocusRow("race-type"); InputSystem.QueueStateEvent(pad, new GamepadState { leftStick = new Vector2(1, 0) }); yield return new WaitForSecondsRealtime(.15f); Release(); yield return new WaitForSecondsRealtime(1);
            Note($"Race type back to normal: stunts {race.stunts}, vehicle {race.vehicle.GetComponent<VehicleConfiguration>().profileId}, rivals {string.Join(", ", race.opponentRoster)}, setting {flow.Save.Settings.stuntRace}");
            yield return Press(GamepadButton.East); yield return new WaitForSecondsRealtime(.8f); Note($"B: page '{Page}'");
        }
    }
}
