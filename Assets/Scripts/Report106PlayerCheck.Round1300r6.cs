using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Racer
{
    // 1.3.0-review6 evidence only (Report106PlayerCheck: -p106check <dir> -racerTestSave <dir>, a copy of Dan's save):
    //   -p106kind lev1300r6     Leviathan Reverse, a normal race and a Stunt Race (-p1300types), car and bike (-p1300vehicles): the course as
    //                           built (gates, shortcuts open / closed, every barricade and how far it stands from the line, the scene arrows
    //                           hidden); the lap by the road AI to CP 33, then the pad by what the HUD shows (the road AI on the flights only) to
    //                           the finish: each gate, missed gates, TURN BACK, a barricade touched, the arrows in view at the landing and on the
    //                           start stretch; shots from the driver's view where the barricade was, the landing looking ahead, each new gate
    //   -p106kind menus1300r6   Parts D, E, F: the main menu, a Quick Race's finish screen, REPLAYS and the replay viewer by the controller and by
    //                           the keys (1280x800 shots); a campaign race with an upgraded vehicle (its board, UPG, its #1 replay); a split-screen
    //                           race; REPLAYS lists all three (RECENT); the 31st recent drops the oldest unsaved one; SAVE from the page; one watched
    public sealed partial class Report106PlayerCheck
    {
        // ---------------------------------------------------------------- Parts A, B, C
        IEnumerator Lev1300r6()
        {
            foreach (var m in InputSystem.devices.OfType<Mouse>().ToList()) InputSystem.DisableDevice(m);
            Campaign.Testing = true; DriverVariation.Seed = 1306; Conditions(); bool described = false;
            foreach (var type in Arg("-p1300types", "normal,stunt").Split(','))
                foreach (var vehicle in Arg("-p1300vehicles", "original,stuntbike").Split(','))
                {
                    yield return QuickTo115(9); Conditions(); bool stunt = type == "stunt"; if (race.stunts != stunt) flow.StepRaceType(1);
                    var st = flow.Save.Settings; st.opponents = false; st.traffic = false; st.laps = 1; flow.Save.SaveSettings(); race.opponents = false; race.traffic = false; race.laps = 1;
                    flow.OpenGarage(); flow.SelectVehicle(vehicle); flow.CloseGarage(); flow.StartRace(); yield return WaitRacing(); Bind();
                    Note($"== {race.courseName} ({race.courseId}), {(race.stunts ? "Stunt Race" : "normal race")}, {vehicle}: line {race.road.Length:F0} m, {race.gates.Length} gates; record course {race.RecordCourse}");
                    Note($"   shortcuts: {string.Join(", ", race.AllBranches.Select(b => $"{b.title} (s {b.entryRoad:F0}-{b.exitRoad:F0}, bypasses {string.Join("/", b.bypassedGates)}){(race.Branches.Contains(b) ? " open" : " CLOSED")}"))}");
                    foreach (var line in StuntShortcuts.Report) Note("   closed for the Stunt Race: " + line);
                    if (!described || stunt && vehicle == Arg("-p1300vehicles", "original,stuntbike").Split(',')[0]) { described = true; Describe1300r6(); }
                    yield return EndLap1300r6(vehicle, type);
                    flow.QuitRace(); yield return WaitMenu();
                }
        }
        void Describe1300r6()
        {
            var d = LeviathanCourse.Current;
            Note($"   flights: {string.Join(", ", race.GetComponent<MountainFlights>().flights.Select(f => $"{f.name} s {f.approachStation:F0}-{f.endStation:F0}"))}; the mountain s {d.mountain[0]:F0}-{d.mountain[1]:F0}");
            for (int i = 30; i < race.gates.Length; i++) { var g = race.gates[i]; Note($"   CP {i} {g.name} at {V(g.transform.position)} station {race.GateStation(i):F0}, opening {2 * g.halfWidth:F0} m wide, {g.halfHeight:F0} m below / {Mathf.Max(g.halfHeight, g.upperHeight):F0} m above"); }
            foreach (int i in new[] { 17, 21 }) { var g = race.gates[i]; Note($"   CP {i} {g.name}: opening {2 * g.halfWidth:F0} m wide, {Mathf.Max(g.halfHeight, g.upperHeight):F0} m above"); }
            // every barricade: what it closes and how far its nearest panel stands from the race line's edge (and a shortcut's, when open)
            var holders = race.gameObject.scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Transform>(true)).Where(t => t.name.StartsWith("Leviathan road closed (")).ToList();
            Note($"   barricades in this race: {holders.Count}");
            foreach (var h in holders)
            {
                float clear = float.MaxValue, shortcut = float.MaxValue;
                foreach (var c in h.GetComponentsInChildren<BoxCollider>())
                {
                    var p = c.bounds.center; float rs = race.road.Project(p, out float lat); var q = race.road.At(rs, out _);
                    if (Mathf.Abs(q.y - p.y) < 6) clear = Mathf.Min(clear, Mathf.Abs(lat) - race.road.HalfWidth(rs) - c.bounds.extents.magnitude * .5f);
                    foreach (var b in race.Branches) { float bs = b.Project(p, out float bl); var bq = b.At(bs, out _); if (Mathf.Abs(bq.y - p.y) < 6) shortcut = Mathf.Min(shortcut, Mathf.Abs(bl) - b.halfWidth); }
                }
                Note($"     {h.name.Replace("Leviathan road closed ", "")} at {V(h.position)}: {(clear == float.MaxValue ? "not near the line" : $"{clear:0.0} m clear of the line's edge")}{(shortcut < 30 ? $", {shortcut:0.0} m clear of an open shortcut" : "")}{(clear < 0 ? "   <-- ON THE ROUTE" : "")}");
            }
            Note($"   scene arrows hidden ({LeviathanCourse.ArrowReport.Count}): {string.Join("; ", LeviathanCourse.ArrowReport.Where(a => a.Contains("another part")))}");
        }
        // the scene arrows in view from the car: within 160 m, within 50 degrees of its heading, nothing solid between
        string ArrowsInView1300r6(ArcadeVehicle car)
        {
            var eye = car.Body.position + Vector3.up * 1.6f; var fwd = Flat(car.transform.forward).normalized; var seen = new List<string>();
            foreach (var r in FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None))
            {
                if (!r.enabled || !r.gameObject.activeInHierarchy || !r.name.EndsWith("arrow", System.StringComparison.OrdinalIgnoreCase) || r.name.StartsWith("Leviathan")) continue;
                var at = r.bounds.center; var to = at - eye; if (to.magnitude > 160 || Vector3.Angle(fwd, Flat(to)) > 50) continue;
                if (Physics.Linecast(eye, at + Vector3.up * .3f, out var hit, ~0, QueryTriggerInteraction.Ignore) && hit.distance < to.magnitude - 3 && !hit.collider.attachedRigidbody) continue;
                var mf = r.GetComponent<MeshFilter>(); var dir = Vector3.zero;
                if (mf && mf.sharedMesh && mf.sharedMesh.isReadable) { var v = mf.sharedMesh.vertices; var c = Vector3.zero; foreach (var x in v) c += x; c /= v.Length; dir = Flat(r.transform.TransformPoint(v.OrderByDescending(x => (x - c).sqrMagnitude).First()) - r.transform.TransformPoint(c)).normalized; }
                float s = race.road.Project(at, out float lat); race.road.At(s, out var f);
                seen.Add($"{r.name} {to.magnitude:0} m ahead ({Vector3.SignedAngle(fwd, Flat(to), Vector3.up):+0;-0}°), pointing {Vector3.SignedAngle(Flat(f), dir, Vector3.up):+0;-0}° from the course there, {Mathf.Abs(lat):0} m off the line at s {s:F0}");
            }
            return seen.Count == 0 ? "no scene arrow in view" : string.Join("; ", seen);
        }
        IEnumerator EndLap1300r6(string vehicle, string type)
        {
            var me = race.Racers[0]; var car = race.vehicle; var g = race.GetComponent<LeviathanGuide>(); Pilot(car, me); var pilot = car.GetComponent<RoadDriver>(); BindPad(); var vi = car.GetComponent<VehicleInput>();
            var flights = race.GetComponent<MountainFlights>()?.flights ?? new MountainFlights.Flight[0]; var gully = flights.FirstOrDefault(f => f.name == "Westbound Gully Flight");
            race.finishGraceSeconds = 100000; int from = int.Parse(Arg("-p1300from", "35")); int aiUntilGate = -1; Vector3 lastStuck = Vector3.zero; bool padOn = false, aiFlight = true; int lastNext = me.Progress.NextGate, lastTb = -1, turnBacks = 0, missed0 = 0;
            var shots = new HashSet<string>(); bool shoot = vehicle == Arg("-p1300vehicles", "original,stuntbike").Split(',')[0]; string tag = type + "-" + vehicle;
            var panels = race.gameObject.scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<BoxCollider>(true)).Where(c => c.name == "Barricade collision" && c.gameObject.activeInHierarchy).ToList(); var touched = new HashSet<string>();
            float t0 = Time.time, stuck = 0; Time.timeScale = 3; Note($"-- {tag}: the road AI to CP {from}, then the pad by the HUD ({panels.Count} barricade panels in the race)");
            while (Time.time - t0 < 1500 && (flow.State == RaceFlow.Stage.Racing || flow.State == RaceFlow.Stage.Paused) && !me.Progress.Finished)
            {
                yield return null; AudioListener.volume = 0; if (Time.timeScale == 0) Time.timeScale = padOn ? 1.5f : 3;
                if (flow.State == RaceFlow.Stage.Paused) { Note($"   paused (page {Page}): resumed"); flow.Resume(); yield return null; continue; }
                int next = me.Progress.NextGate; var p = car.Body.position; float station = Mathf.Repeat(LeviathanGuide.Station(race, me), race.road.Length);
                if (next != lastNext) { if (padOn || next >= from) Note($"   {Time.time - t0:0} s: CP {lastNext} passed (next {next}){(me.Progress.MissedGates > missed0 ? $", MISSED {me.Progress.MissedGates - missed0}" : "")}"); missed0 = me.Progress.MissedGates; lastNext = next; }
                if (!padOn && next >= from) { padOn = true; Time.timeScale = 1.5f; Note($"   {Time.time - t0:0} s: CP {from} next: the pad drives by the HUD from {V(p)}"); }
                if (g.TurnBackGate != lastTb) { if (g.TurnBackGate >= 0) { turnBacks++; Note($"   TURN BACK to CP {g.TurnBackGate} ({g.TurnBackWhy}) at {V(p)}"); } lastTb = g.TurnBackGate; }
                foreach (var c in panels) if (c && c.bounds.SqrDistance(p) < 2.5f * 2.5f && touched.Add(c.transform.parent.parent.name)) Note($"   BARRICADE within 2.5 m: {c.transform.parent.parent.name} at {V(p)}");
                // the shots, from the driver's view
                if (shoot)
                {
                    IEnumerator Shot(string name, string note) { if (!shots.Add(name)) yield break; float ts = Time.timeScale; Time.timeScale = 0; yield return Snap($"{name}-{type}"); Note($"   shot {name}-{type}: {note}"); Time.timeScale = ts; }
                    if (Vector3.Distance(Flat(p), new Vector3(808, 0, -167)) < 28 && next <= 36) yield return Shot("A-where-the-barricade-was", $"at {V(p)}, next CP {next}; {ArrowsInView1300r6(car)}");
                    if (gully != null && station > gully.endStation + 2 && station < gully.endStation + 30 && car.GroundedWheels > 0) yield return Shot("B-gully-landing-looking-ahead", $"at {V(p)}, s {station:F0}, next CP {next} {Vector3.Distance(Flat(p), Flat(race.GatePosition(next))):0} m on; HUD arrow {Arrow(car, me):+0;-0}°; {ArrowsInView1300r6(car)}");
                    if (next == 39 && Vector3.Distance(Flat(p), new Vector3(726, 0, -60)) < 20) yield return Shot("B-start-stretch-looking-ahead", $"at {V(p)}, next CP {next}; HUD arrow {Arrow(car, me):+0;-0}°; {ArrowsInView1300r6(car)}");
                    if (next >= 35 && next <= 39 && LeviathanGuide.ToNextGate(race, me) < 40) yield return Shot($"C-gate-{next}", $"{race.gates[next].name} {LeviathanGuide.ToNextGate(race, me):0} m ahead");
                }
                // who drives: the road AI to CP 33 and on the flights; the pad by the HUD's way elsewhere
                bool flight = flights.Any(f => station > f.approachStation - 15 && station < f.endStation + 10);
                if (aiUntilGate >= 0 && next != aiUntilGate) { aiUntilGate = -1; Note($"   a gate passed (next CP {next}): the pad again"); }
                bool ai = !padOn || flight || aiUntilGate >= 0; if (ai != aiFlight) { aiFlight = ai; pilot.enabled = ai; vi.enabled = !ai; if (!ai) vi.Bind(pad); if (padOn) Note($"   {Time.time - t0:0} s: {(ai ? "the road AI (a flight)" : "the pad by the HUD")} at s {station:F0}"); }
                if (!ai)
                {
                    var ahead = LeviathanGuide.Ahead(race, me, 24, 6); var target = ahead[ahead.Count - 1]; float cap = 20;
                    if (g.TurnBackGate >= 0 || Vector3.Distance(Flat(ahead[0]), Flat(p)) > 30) { target = race.GatePosition(next); cap = 9; }
                    else if (next > 0 && LeviathanGuide.ToNextGate(race, me) < 28) { target = race.GatePosition(next); cap = 14; }
                    Drive(car, target, Mathf.Min(cap, race.Leviathan.Mountain(station) ? 15 : cap));
                }
                stuck = car.Body.linearVelocity.magnitude < 1.5f ? stuck + Time.deltaTime : 0;
                if (stuck > 6)
                {
                    // stuck again where it was last stuck: the check's pad can't drive this bit (a person can); the road AI takes it to the next gate
                    bool again = true; lastStuck = p; // the pad stuck once: the road AI to the next gate (a second reset in a row opens the pause question)
                    Note($"   stuck at {V(p)} (next CP {next}, {(ai ? "road AI" : "pad")}): reset{(again && !ai ? "; the road AI drives to CP " + next : "")}"); if (again && !ai) aiUntilGate = next;
                    car.GetComponent<VehicleRespawn>()?.ResetVehicle(); stuck = 0; yield return new WaitForSecondsRealtime(2.5f); if (flow.State == RaceFlow.Stage.Paused) flow.Resume();
                }
            }
            Release(); Time.timeScale = 1;
            Note($"   end: finished {me.Progress.Finished} in {me.Progress.RaceTime(race.Clock):F1} s, missed gates {me.Progress.MissedGates}, TURN BACK {turnBacks}, barricades touched {touched.Count}");
        }

        // ---------------------------------------------------------------- Parts D, E, F
        IEnumerator PadPoke() { yield return HoldPad(new GamepadState { leftTrigger = .7f }, .25f); }   // a controller used (no menu action)
        IEnumerator KeyPoke() { yield return Tap(Key.LeftShift); }                                       // the keyboard used (no menu action)
        string Prompts() => string.Join(" | ", FindObjectsByType<UnityEngine.UI.Text>(FindObjectsSortMode.None).Where(t => t.isActiveAndEnabled && (t.name == "Menu controls" || t.text.Contains("REPLAY") || t.text.Contains("Esc") || t.text.Contains("Start"))).Select(t => t.text.Replace("\n", " / ")).Take(8));
        IEnumerator Both1300r6(string name)
        {
            yield return PadPoke(); yield return new WaitForSecondsRealtime(.6f); yield return Snap($"E-{name}-controller"); Note($"   {name}, controller ({MenuInput.Controller}): {Prompts()}");
            yield return KeyPoke(); yield return new WaitForSecondsRealtime(.6f); yield return Snap($"E-{name}-keyboard"); Note($"   {name}, keyboard (controller {MenuInput.Controller}): {Prompts()}");
            yield return PadPoke(); yield return new WaitForSecondsRealtime(.3f);
        }
        List<string> Recent1300r6() => Directory.Exists(ReplayData.RecentFolder) ? Directory.GetFiles(ReplayData.RecentFolder, "*" + ReplayData.Extension).OrderBy(f => Path.GetFileName(f), System.StringComparer.Ordinal).ToList() : new List<string>();
        IEnumerator Menus1300r6()
        {
            foreach (var m in InputSystem.devices.OfType<Mouse>().ToList()) InputSystem.DisableDevice(m);
            Campaign.Testing = true; Conditions(); flow.Save.Settings.hints = false; flow.Save.SaveSettings(); yield return WaitMenu();
            if (flow.State == RaceFlow.Stage.Title) { flow.EnterMenuAfterTitle(); yield return new WaitForSecondsRealtime(1); }
            Note($"== replays folder {ReplayData.Folder}: recent {Recent1300r6().Count}, saved {ReplayData.List(false).Count}, #1 {ReplayData.List(true).Count}");
            bool onlyF = Arg("-p1300only", "") == "F";
            if (!onlyF) {
            Note("-- E: the main menu"); yield return Both1300r6("main-menu");
            // a Quick Race (Street Loop Forward, 1 lap, solo) to its finish screen
            yield return QuickTo115(0); Conditions();
            var st = flow.Save.Settings; st.opponents = false; st.traffic = false; st.laps = 1; flow.Save.SaveSettings(); race.opponents = false; race.traffic = false; race.laps = 1;
            flow.StartRace(); yield return RaceToResults(); Time.timeScale = 1;
            Note($"-- D: Quick Race finished: recorder {ReplayRecorder.Note}; recent now {Recent1300r6().Count}");
            Note("-- E: the finish screen"); yield return Both1300r6("finish-screen");
            Menu.WatchLastReplay(); yield return new WaitForSecondsRealtime(2); var v = ReplayViewer.Current;
            Note($"-- E: the replay viewer open {(v ? "yes" : "NO")}");
            if (v) { yield return PadPoke(); yield return new WaitForSecondsRealtime(.5f); yield return Snap("E-replay-viewer-controller"); yield return KeyPoke(); yield return new WaitForSecondsRealtime(.5f); yield return Snap("E-replay-viewer-keyboard"); v.Close(); yield return new WaitForSecondsRealtime(1); }
            yield return Choose("replays"); yield return new WaitForSecondsRealtime(.8f); Note($"-- E: REPLAYS from the results (page {Page})"); yield return Both1300r6("replays");
            yield return Back(); flow.QuitRace(); yield return WaitMenu();
            }
            // F: a campaign race with an upgraded vehicle (the test save's campaign), on its own board
            var e = CampaignData.Find("c1-first-lap"); var c = Campaign.Current;
            if (c == null) Note("-- F: no campaign in the save copy");
            else
            {
                var u = c.upgrades.FirstOrDefault(x => x.id == "original"); if (u == null) c.upgrades.Add(u = new Campaign.Upgrade { id = "original" }); u.levels[0] = Mathf.Max(1, u.levels[0]);
                Note($"-- F: campaign event {e.Name} ({e.Kind}, {e.Laps} laps), the original upgraded: {Campaign.UpgradeSummary("original")} (Upgraded {Campaign.Upgraded("original")})");
                flow.StartCampaignEvent(e, "original"); yield return new WaitForSecondsRealtime(2); yield return WaitRacing(); Bind();
                // the event's scene load reads the campaign again from the save (Testing writes nothing): the upgrade set again on the loaded run
                c = Campaign.Current; u = c.upgrades.FirstOrDefault(x => x.id == "original"); if (u == null) c.upgrades.Add(u = new Campaign.Upgrade { id = "original" }); u.levels[0] = Mathf.Max(1, u.levels[0]); Note($"   in the race: {Campaign.UpgradeSummary("original")}");
                string cat = race.Category; int before = flow.Boards.Board(cat, true).Count; var top = flow.Boards.Board(cat, true).FirstOrDefault(); Note($"   board {cat}: {before} race entries before{(top != null ? $", #1 {RaceHud.FormatTime(top.seconds)} by {top.name}" : "")}");
                yield return RaceToResults(1.15f); Time.timeScale = 1;
                var board = flow.Boards.Board(cat, true); var lap = flow.Boards.Board(cat, false); var first = board.FirstOrDefault();
                Note($"   finished: race rank #{flow.RaceRank}, lap rank #{flow.LapRank}; the board's #1 {(first != null ? $"{RaceHud.FormatTime(first.seconds)} {first.name} {VehicleProfile.Find(first.vehicle).Name}{(first.upgraded ? " UPG" : "")} ({first.id})" : "none")}; laps board {lap.Count}");
                string racePath = ReplayData.BestPath(cat, true), lapPath = ReplayData.BestPath(RecordBoards.LapCategory(cat), false);
                Note($"   #1 replays kept: race {(File.Exists(racePath) ? Path.GetFileName(racePath) + " " + Kb(racePath) : "NONE")}, lap {(File.Exists(lapPath) ? Kb(lapPath) : "NONE")}; recorder {ReplayRecorder.Note}");
                yield return new WaitForSecondsRealtime(1); yield return Snap("F-campaign-results");
                flow.QuitRace(); yield return WaitMenu(); Bind();
                yield return Choose("records"); yield return new WaitForSecondsRealtime(1); flow.SelectRecords(cat); Menu.Show(); yield return new WaitForSecondsRealtime(1); yield return Snap("F-records-board"); Note($"   records page: {Texts("1 ", "#")}");
                yield return Back(); yield return Back();
            }
            // a split-screen race (Street Loop Forward, 1 lap, player 2 the AI, 1 rival)
            SplitScreen.Mode = SplitScreen.Kind.Race; SplitScreen.Solo = false; SplitScreen.P1Device = pad; SplitScreen.P2Device = null; SplitScreen.P2Ai = true; SplitScreen.Course = 0; SplitScreen.Laps = 1; SplitScreen.Rivals = 1;
            SplitScreen.P1Vehicle = "original"; SplitScreen.P2Vehicle = "atv"; SplitScreen.P1Name = "Player One"; SplitScreen.P2Pick = "Player Two"; flow.StartSplit();
            yield return new WaitForSecondsRealtime(2); yield return RaceToResults(); Time.timeScale = 1;
            Note($"-- D: split-screen race finished: recorder {ReplayRecorder.Note}");
            var last = ReplayRecorder.Instance ? ReplayRecorder.Instance.Last : null; flow.QuitSplit(false); yield return WaitMenu(); Bind();
            var recent = Recent1300r6(); Note($"-- D: recent replays now {recent.Count}: {string.Join(", ", recent.Select(f => Path.GetFileName(f) + " " + Kb(f)))}");
            foreach (var f in recent) { var h = ReplayData.Read(f, true); Note($"   {h.kind} {h.Title} {h.mode} · {h.label} · {h.setup} · {RaceHud.FormatTime(h.bestTime)} · {h.When}"); }
            // REPLAYS from the main menu: the list, a recent one opened, SAVE, then WATCH a recent one
            yield return Choose("replays"); yield return new WaitForSecondsRealtime(1); yield return Snap("D-replays-list");
            var rows = FindObjectsByType<UnityEngine.UI.Button>(FindObjectsSortMode.None).Where(b => b.isActiveAndEnabled && (b.name.StartsWith("recent-") || b.name.StartsWith("saved-") || b.name.StartsWith("best-"))).OrderBy(b => b.transform.GetSiblingIndex()).ToList();
            Note($"-- D: REPLAYS page: {rows.Count} rows; the first: {string.Join(" || ", rows.Take(5).Select(b => b.GetComponentInChildren<UnityEngine.UI.Text>(true).text.Replace("\n", " / ")))}");
            var row = rows.FirstOrDefault(b => b.name.StartsWith("recent-"));
            if (row) { yield return Choose(row.name); yield return new WaitForSecondsRealtime(.8f); yield return Snap("D-recent-replay-page"); Note($"   opened {row.name}: page {Page}, rows {RowText("watch")} / {RowText("save")} / {RowText("delete")}");
                int saved0 = ReplayData.List(false).Count, recent0 = Recent1300r6().Count; yield return Choose("save"); yield return new WaitForSecondsRealtime(1);
                Note($"   SAVE: saved {saved0} -> {ReplayData.List(false).Count}, recent {recent0} -> {Recent1300r6().Count}; page now {Texts("SAVED REPLAY", "RECENT REPLAY")}"); yield return Back(); }
            yield return new WaitForSecondsRealtime(.5f); row = FindObjectsByType<UnityEngine.UI.Button>(FindObjectsSortMode.None).Where(b => b.isActiveAndEnabled && b.name.StartsWith("recent-")).OrderBy(b => b.transform.GetSiblingIndex()).FirstOrDefault();
            if (row) { yield return Choose(row.name); yield return Choose("watch"); yield return new WaitForSecondsRealtime(3); var vw = ReplayViewer.Current; float a = vw ? vw.T : 0; yield return new WaitForSecondsRealtime(2); Note($"   WATCH a recent one: viewer {(vw ? $"open, {vw.Data.Title}, clock {a:F1} -> {vw.T:F1} s" : "NOT OPEN")}"); if (vw) { yield return Snap("D-recent-replay-playing"); vw.Close(); yield return new WaitForSecondsRealtime(1); } }
            // the 31st: the recent ones filled to 30 from the last race, then one more
            if (last != null)
            {
                int best = ReplayData.List(true).Count, saved = ReplayData.List(false).Count;
                while (Recent1300r6().Count < ReplayData.RecentKept) { ReplayData.KeepRecent(last); yield return null; }
                var full = Recent1300r6(); string oldest = Path.GetFileName(full[0]); long total = full.Sum(f => new FileInfo(f).Length);
                Note($"-- D: recent filled to {full.Count} ({total / 1048576.0:F1} MB); the oldest {oldest}");
                ReplayData.KeepRecent(last); var after = Recent1300r6();
                Note($"   the 31st kept: recent {after.Count}, the oldest {(after.Any(f => Path.GetFileName(f) == oldest) ? "STILL THERE" : "dropped")}; #1 {best} -> {ReplayData.List(true).Count}, saved {saved} -> {ReplayData.List(false).Count}");
                float t1 = Time.realtimeSinceStartup; yield return Choose("back"); yield return new WaitForSecondsRealtime(.5f); t1 = Time.realtimeSinceStartup; yield return Choose("replays"); Note($"   REPLAYS page with {after.Count + ReplayData.List(false).Count + ReplayData.List(true).Count} replays drawn in {Time.realtimeSinceStartup - t1:F2} s"); yield return Snap("D-replays-list-full");
            }
        }
    }
}
