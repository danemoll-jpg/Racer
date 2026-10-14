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
    // 0.116 evidence only (the 0.106 check component: -p106check <dir> -racerTestSave <dir>; never active without both):
    //   -p106kind turns116  Leviathan (-p115course 8 Forward / 9 Reverse), solo, 1 lap, driven by the virtual pad (a bot on the course line, the
    //                       game's reset button when stuck): at 70 m and 25 m before each decision point a driver's-view shot (HUD, minimap, gates
    //                       and turn signs as the player sees them) with the HUD's next-CP line and the gates drawn; frames at 10 a second from
    //                       110 m before to 30 m after the key ones; every wrong-way warning over the lap (where, which pass). Then the pause
    //                       menu, the race's map, and (after quitting) Race > Tracks with the course highlighted. -p116before: the 0.115 way
    //                       (every gate drawn, no turn signs, no next-CP line or course ahead on the minimap, no map cover), for the before shots.
    //   -p106kind short116  Leviathan Reverse, solo: for each of its shortcuts, the pad bot from 90 m before its entry, once along the main road
    //                       and once along the shortcut: the time between the entry and the exit (planes across the road there), its contacts
    //                       and resets; frames at 10 a second through the shortcut; a top-down shot of it.
    public sealed partial class Report106PlayerCheck
    {
        static readonly float[] ForwardTurns = { 640, 905, 963, 1268, 2503, 2822 }, ReverseTurns = { 3998, 4286, 4404, 5820, 6043, 6081, 6195, 6512 };
        static readonly float[] ForwardClips = { 905, 2503 }, ReverseClips = { 4404, 6043 };
        void SoloStart(int laps)
        {
            var st = flow.Save.Settings; st.opponents = false; st.traffic = false; st.laps = laps; st.estimateAiFinishes = false; flow.Save.SaveSettings();
            race.opponents = false; race.traffic = false; race.laps = laps; flow.StartRace();
        }
        // the bot's speed for the line ahead (6 m/s2 sideways, 5 m/s2 braking) and its target, on any polyline
        static float CurveCap(System.Func<float, Vector3> at, float s, float max)
        {
            float cap = max;
            for (int k = 0; k <= 60; k += 15) { var a = at(s + k); var b = at(s + k + 30); var c = at(s + k + 3); float turn = Vector3.Angle(Flat(b - a), Flat(c - a)); float r = turn < 1 ? 1e4f : 30 / (turn * Mathf.Deg2Rad); cap = Mathf.Min(cap, Mathf.Sqrt(Mathf.Clamp(6 * r, 25, 1150) + 2 * 5 * k)); }
            return cap;
        }
        float RoadCap(float s) { return CurveCap(x => race.road.At(x, out _), s, 34); }
        Vector3 RoadTarget(float s, float speed, out float cap)
        {
            var target = race.road.At(s + Mathf.Clamp(speed * .9f, 7, 24), out _); cap = RoadCap(s);
            var flights = race.GetComponent<MountainFlights>(); var flight = flights ? flights.At(race.road, s) : null;
            if (flight != null) { var pos = race.vehicle.Body.position; var axis = Flat(flight.forward).normalized; float al = Vector3.Dot(pos - flight.start, axis); target = flight.start + axis * (al + 20); target.y = pos.y; cap = flight.aiTakeoffSpeed > 0 ? flight.aiTakeoffSpeed : flight.aiEntrySpeed > 0 ? 45 : 32; }
            return target;
        }
        IEnumerator Frames(string dir, List<float> times)
        {
            Directory.CreateDirectory(Path.Combine(outDir, dir)); yield return new WaitForEndOfFrame();
            ScreenCapture.CaptureScreenshot(Path.Combine(outDir, dir, $"f{times.Count:0000}.png")); times.Add(Time.realtimeSinceStartup);
        }
        string GatesDrawn() => string.Join(",", Enumerable.Range(0, race.gates.Length).Where(i => race.gates[i].GetComponentsInChildren<Renderer>(true).Any(r => r.enabled)));
        string HudLine() { var t = FindObjectsByType<UnityEngine.UI.Text>(FindObjectsSortMode.None).FirstOrDefault(x => x.isActiveAndEnabled && x.text.StartsWith("NEXT  ")); return t ? t.text : "(no next-CP line)"; }

        IEnumerator Turns116()
        {
            int course = int.Parse(Arg("-p115course", "8")); bool before = Arg("-p116before", "0") == "1"; string tag = before ? "before" : "after";
            LeviathanGuide.Disabled = before; Campaign.Testing = true; Conditions();
            yield return QuickTo115(course); Conditions();
            flow.OpenGarage(); flow.SelectVehicle(Arg("-p106vehicle", "original")); flow.CloseGarage();
            SoloStart(1); yield return WaitRacing(); Bind(); BindPad(); AudioListener.volume = 0;
            var signs = FindObjectsByType<PhysicalSign>(FindObjectsSortMode.None).Where(p => p.name.StartsWith("Leviathan ")).ToArray();
            Note($"== {RacePlaylists.Titles[course]} ({tag}): {Lev()}; turn signs {signs.Length} (arrows {signs.Count(p => p.name.Contains("arrow"))}, chevrons {signs.Count(p => p.name.Contains("chevron"))})");
            foreach (var p in signs) { var gy = TopAt(p.transform.position + Vector3.up * 3); Note($"  sign {p.name} at {V(p.transform.position)}, ground {gy - p.transform.position.y:+0.00;-0.00} m, colliders {p.GetComponentsInChildren<Collider>(true).Length}"); }
            var car = race.vehicle; var road = race.road; float station = road.Project(car.Body.position, out _);
            var turns = course == 8 ? ForwardTurns : ReverseTurns; var clips = course == 8 ? ForwardClips : ReverseClips;
            var events = new List<(float s, string what)>(); foreach (var d in turns) { events.Add((d - 70, $"turn-{course}-{d:0}-70m-{tag}")); events.Add((d - 25, $"turn-{course}-{d:0}-25m-{tag}")); }
            // Reverse: where Street Loop Reverse's Granite Creek Cut and Laurel Switchbacks leave the road (they are not in this scene), and the
            // mouths of Leviathan Reverse's own shortcuts
            if (course == 9) foreach (var (st, what) in new[] { (404f - 30, "granite-mouth"), (3064f - 30, "laurel-mouth"), (3379f - 40, "hollow-creek-hop-entry"), (3900f - 40, "rocky-way-cut-entry") }) events.Add((st, $"short-{what}-{tag}"));
            events = events.OrderBy(e => e.s).ToList(); int nextEvent = 0;
            var wrong = new List<string>(); bool wasWrong = false; var guidance = race.GetComponent<WrongWayGuidance>();
            float t0 = Time.time, lastMove = Time.time, bestS = 0; int resets = 0; var frameTimes = new Dictionary<float, List<float>>(); float nextFrame = 0;
            float travelled = 0, prevS = station;
            while (flow.State == RaceFlow.Stage.Racing && Time.time - t0 < 1200 && travelled < road.Length - 30)
            {
                var pos = car.Body.position; float s = road.ProjectNear(pos, station, 50, out float lat);
                float step = Mathf.Repeat(s - prevS + road.Length * .5f, road.Length) - road.Length * .5f; if (step > 0 && step < 60) travelled += step; prevS = s; station = s;
                if (travelled > bestS + 3) { bestS = travelled; lastMove = Time.time; }
                bool inClip = clips.Any(c => Near(s, c - 110, c + 30, road.Length)); bool nearTurn = turns.Any(d => Near(s, d - 120, d + 40, road.Length));
                Time.timeScale = nearTurn ? 1 : 3;
                if (guidance && guidance.Visible != wasWrong) { wasWrong = guidance.Visible; if (wasWrong) wrong.Add($"WRONG WAY shown at {V(pos)} line s {s:F0} (lap travel {travelled:F0} m)"); }
                if (Time.time - lastMove > 6 || (lat > 18 && car.GroundedWheels > 0))
                {
                    Note($"  assist: {(Time.time - lastMove > 6 ? "stuck" : "off the line")} at {V(pos)} s {s:F0}: the reset button"); resets++;
                    Release(); yield return Press(GamepadButton.North); yield return new WaitForSecondsRealtime(2.5f);
                    station = road.ProjectNear(car.Body.position, station, 120, out _); prevS = station; lastMove = Time.time; continue;
                }
                var target = RoadTarget(s, car.ForwardSpeed, out float cap); Drive(car, target, Mathf.Max(cap, 7), 0, 0, 0, 22);
                if (nextEvent < events.Count && Near(s, events[nextEvent].s, events[nextEvent].s + 40, road.Length))
                {
                    var e = events[nextEvent++]; yield return Snap(e.what);
                    var pr = race.Racers[0].Progress; Note($"  {e.what}: at {V(pos)} {car.ForwardSpeed:F0} m/s, next CP {pr.NextGate}, gates drawn [{GatesDrawn()}], HUD '{HudLine()}'");
                    continue;
                }
                if (inClip && Time.realtimeSinceStartup >= nextFrame)
                {
                    float c = clips.First(cc => Near(s, cc - 110, cc + 30, road.Length)); if (!frameTimes.ContainsKey(c)) frameTimes[c] = new List<float>();
                    nextFrame = Time.realtimeSinceStartup + .1f; yield return Frames($"clip-{course}-{c:0}-{tag}", frameTimes[c]); continue;
                }
                yield return new WaitForFixedUpdate();
            }
            Time.timeScale = 1; Release();
            var me = race.Racers[0].Progress;
            Note($"  the pad drove {Time.time - t0:F0} s game time, {travelled:F0} m; resets {resets}; missed gates {me.MissedGates}; finished {me.Finished}; wrong-way warnings {wrong.Count}");
            foreach (var w in wrong) Note("  " + w); foreach (var p in me.Penalties) Note("  penalty: " + p);
            foreach (var kv in frameTimes) Note($"  clip {course}-{kv.Key:0}: {kv.Value.Count} frames");
            // the maps: pause menu, the race's map (the world map in the race)
            float w0 = Time.realtimeSinceStartup; while (flow.State != RaceFlow.Stage.Racing && flow.State != RaceFlow.Stage.Results && Time.realtimeSinceStartup - w0 < 30) yield return null;
            if (flow.State == RaceFlow.Stage.Racing) { flow.Pause(); yield return new WaitForSecondsRealtime(1.5f); yield return Snap($"pause-{course}-{tag}"); flow.Resume(); yield return new WaitForSecondsRealtime(.5f); }
            var map = flow.GetComponent<ExplorationMap>(); if (map) { map.Open(); yield return new WaitForSecondsRealtime(1.5f); yield return Snap($"racemap-{course}-{tag}"); map.Close(); yield return new WaitForSecondsRealtime(1); }
            if (flow.State == RaceFlow.Stage.Racing) { flow.Pause(); yield return new WaitForSecondsRealtime(.5f); }
            flow.QuitRace(); yield return WaitMenu(); Note($"  back at the menu: state {flow.State}, page '{Page}'");
            // Race > Tracks with the course highlighted (its preview map)
            yield return FocusRow("race"); yield return Press(GamepadButton.South); yield return new WaitForSecondsRealtime(1); yield return FocusRow("tracks"); yield return Press(GamepadButton.South); yield return new WaitForSecondsRealtime(1);
            yield return FocusRow("course-" + course, 30); yield return new WaitForSecondsRealtime(1.5f); yield return Snap($"tracks-preview-{course}-{tag}"); Note($"  Tracks: focus '{Focus}'");
        }
        // -p106kind debug116  the controller only: in a race, Debug Mode on, Start opens the debug menu, the D-pad to the Leviathan Reverse
        //                     shortcut row, A switches the first one off (and the setting is saved), A again on, B closes; shots of each step
        IEnumerator Debug116()
        {
            Campaign.Testing = true; Conditions(); yield return QuickTo115(9); Conditions(); SoloStart(1); yield return WaitRacing(); Bind(); BindPad(); AudioListener.volume = 0;
            var hud = DeveloperLocationHud.Instance; hud.SetEnabled(true); yield return new WaitForSecondsRealtime(.5f);
            yield return Press(GamepadButton.Start); yield return new WaitForSecondsRealtime(1); Note($"debug menu open: {DeveloperLocationHud.Interactive}, focus '{Focus}'"); yield return Snap("debug-menu-open");
            for (int i = 0; i < 20 && !Focus.StartsWith("Lev. Reverse"); i++) yield return Press(GamepadButton.DpadDown);
            string Label() => UnityEngine.EventSystems.EventSystem.current?.currentSelectedGameObject?.GetComponentInChildren<UnityEngine.UI.Text>()?.text ?? "(none)";
            Note($"focus '{Focus}' label '{Label()}'"); yield return Snap("debug-menu-row");
            yield return Press(GamepadButton.South); yield return new WaitForSecondsRealtime(.5f);
            Note($"after A: label '{Label()}', switched off {string.Join(", ", LeviathanCourse.ShortcutsOff)}; settings.json leviathanShortcutsOff [{string.Join(", ", flow.Save.Settings.leviathanShortcutsOff ?? new string[0])}]; on disk: {File.ReadAllText(Path.Combine(flow.Save.DirectoryPath, "settings.json")).Contains("leviathanShortcutsOff")}");
            yield return Snap("debug-menu-switched-off");
            yield return Press(GamepadButton.South); yield return new WaitForSecondsRealtime(.5f); Note($"after A again: label '{Label()}', switched off [{string.Join(", ", LeviathanCourse.ShortcutsOff)}]");
            yield return Press(GamepadButton.East); yield return new WaitForSecondsRealtime(.5f); Note($"after B: debug menu open {DeveloperLocationHud.Interactive}");
            hud.SetEnabled(false); flow.QuitRace(); yield return WaitMenu();
        }
        static bool Near(float s, float from, float to, float length) { float a = Mathf.Repeat(s - from, length); return a <= to - from; }

        IEnumerator Short116()
        {
            Campaign.Testing = true; Conditions(); yield return QuickTo115(9); Conditions();
            flow.OpenGarage(); flow.SelectVehicle(Arg("-p106vehicle", "original")); flow.CloseGarage();
            SoloStart(1); yield return WaitRacing(); Bind(); BindPad(); AudioListener.volume = 0;
            var car = race.vehicle; var road = race.road; var chase = FindAnyObjectByType<ChaseCamera>();
            Note($"== shortcuts on {Lev()}: {string.Join(", ", race.Branches.Select(b => $"{b.title} (entry s {b.entryRoad:F0}, exit s {b.exitRoad:F0}, {b.Length:F0} m, bypasses [{string.Join(",", b.bypassedGates ?? new int[0])}])"))}");
            var contacts = car.gameObject.GetComponent<Report106Contacts>() ?? car.gameObject.AddComponent<Report106Contacts>(); contacts.who = "player";
            foreach (var b in race.Branches)
            {
                var entryAt = road.At(b.entryRoad, out var entryF); var exitAt = road.At(b.exitRoad, out var exitF);
                var bb = new Bounds(b.points[0], Vector3.zero); foreach (var p in b.points) bb.Encapsulate(p);
                yield return Free($"top-{b.title.Replace(' ', '-')}", new Vector3(bb.center.x, 600, bb.center.z), bb.center, 60, 1400, 1400, true, Mathf.Max(bb.extents.x, bb.extents.z) + 30);
                foreach (bool viaShortcut in new[] { false, true })
                {
                    for (int attempt = 0; attempt < 2; attempt++)
                    {
                        var start = road.At(b.entryRoad - 90, out var f); Put(car, start, Yaw(f), 18); chase?.Snap(); contacts.list.Clear();
                        yield return new WaitForFixedUpdate();
                        float s = b.entryRoad - 90, bs = 0, tIn = -1, tOut = -1, t0 = Time.time, lastMove = Time.time; bool onBranch = false; var movedFrom = car.Body.position; int resets = 0; var frames = new List<float>(); float nextFrame = 0; float maxSpeed = 0, air = 0, airMax = 0;
                        string name = $"{(viaShortcut ? "via" : "main")}-{b.title.Replace(' ', '-')}";
                        var prev = car.Body.position;
                        while (Time.time - t0 < 150 && tOut < 0)
                        {
                            var pos = car.Body.position;
                            if (tIn < 0 && Vector3.Dot(pos - entryAt, entryF) >= 0 && Vector3.Dot(prev - entryAt, entryF) < 0 && Vector3.Distance(pos, entryAt) < 25) tIn = Time.time;
                            if (tIn >= 0 && Vector3.Dot(pos - exitAt, exitF) >= 0 && Vector3.Dot(prev - exitAt, exitF) < 0 && Vector3.Distance(pos, exitAt) < 25) tOut = Time.time;
                            prev = pos;
                            if ((pos - movedFrom).magnitude > 3) { movedFrom = pos; lastMove = Time.time; } // stuck: not 3 m in 6 s
                            float speed = car.ForwardSpeed; maxSpeed = Mathf.Max(maxSpeed, speed); if (car.GroundedWheels == 0) { air += Time.fixedDeltaTime; airMax = Mathf.Max(airMax, air); } else air = 0;
                            Vector3 target; float cap;
                            if (viaShortcut && !onBranch && tIn >= 0) { onBranch = true; }
                            if (viaShortcut && onBranch && bs < b.LengthFor(true) - 6)
                            {
                                bs = b.ProjectFor(true, pos, out _); target = b.AtFor(true, bs + Mathf.Clamp(speed * .8f, 6, 18), out _); cap = CurveCap(x => b.AtFor(true, x, out _), bs, b.recommendedSpeed + 4);
                                if (Time.realtimeSinceStartup >= nextFrame) { nextFrame = Time.realtimeSinceStartup + .1f; yield return Frames($"clip-{name}", frames); }
                            }
                            else
                            {
                                s = road.ProjectNear(pos, s, 50, out _); target = RoadTarget(s, speed, out cap);
                                if (viaShortcut && Time.realtimeSinceStartup >= nextFrame && (tIn < 0 ? s > b.entryRoad - 40 : true)) { nextFrame = Time.realtimeSinceStartup + .1f; yield return Frames($"clip-{name}", frames); }
                            }
                            if (Time.time - lastMove > 6) { Note($"    {name}: stuck at {V(pos)} (shortcut s {bs:F0}): the reset button"); resets++; Release(); yield return Press(GamepadButton.North); yield return new WaitForSecondsRealtime(2.5f); lastMove = Time.time; s = road.ProjectNear(car.Body.position, s, 120, out _); continue; }
                            Drive(car, target, Mathf.Max(cap, 7), 0, 0, 0, 22);
                            yield return new WaitForFixedUpdate();
                        }
                        Release();
                        string result = tIn >= 0 && tOut >= 0 ? $"{tOut - tIn:F1} s" : "NOT TIMED";
                        Note(string.Format(CultureInfo.InvariantCulture, "  {0} {1}: entry to exit {2}, resets {3}, top speed {4:F0} m/s, longest in the air {5:F1} s, frames {6}, contacts [{7}]", b.title, viaShortcut ? "via the shortcut" : "on the main road", result, resets, maxSpeed, airMax, frames.Count, string.Join("; ", contacts.list.Take(8))));
                        if (tOut >= 0 && resets == 0) break;
                    }
                    yield return new WaitForSecondsRealtime(.5f);
                }
            }
            flow.QuitRace(); yield return WaitMenu();
        }
    }
}
