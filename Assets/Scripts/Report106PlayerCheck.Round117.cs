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
    // 0.117 evidence only (the 0.106 check component: -p106check <dir> -racerTestSave <dir>; never active without both):
    //   -p106kind audit117  Leviathan (-p115course 8 / 9), solo: for each item of -p117items (Tools/Report117/py/audit.py: every turn, every closed
    //                       way, Dan's 14 spots) the car stopped at its spot, the race's gate progress set as a driver there would have it (the next
    //                       gate drawn, the HUD's NEXT CP), and a driver's-view shot. -p117before 1 -p117data <dir>: the 0.116 course files and code
    //                       paths, for the before shots.
    //   -p106kind lap117    Leviathan, solo, 1 lap driven by the virtual pad (a bot on the course line; the game's reset button when stuck),
    //                       the whole lap recorded (a frame every 0.2 s, for the full-lap recording); every wrong-way warning, the HUD's NEXT CP
    //                       line every 2 s (a "0 ft" or a jump while far from the gate is flagged), missed gates. -p117via "title": the bot takes that
    //                       shortcut. Then the radio's song caption shown over the HUD (a shot).
    //   -p106kind reset117  Leviathan, solo: at each -p117resets station (the progress there set), the car pushed off the road's lower side, the
    //                       reset button, and where it was put (its station, the gates either side).
    //   -p106kind skip117   Leviathan, solo: the ways to skip the mountain (Part D2), each driven by the pad from its start: what the race made of it
    //                       (the gates counted, the MOUNTAIN MISSED banner, the HUD), and the time it took.
    public sealed partial class Report106PlayerCheck
    {
        [System.Serializable] sealed class AuditItem { public string kind, label; public float x, z, yaw, sNew, sOld, y; }
        [System.Serializable] sealed class AuditList { public AuditItem[] LeviathanForward, LeviathanReverse; }
        // the race's progress as a driver at station s (absolute, on this line) would have it: the start crossed, every gate before s passed
        void ProgressTo(float s, Vector3 pos)
        {
            var r = race.Racers[0]; var p = r.Progress; float rel = race.road.Relative(s, race.Origin);
            p.Restart(); p.Cross(0, true, race.Clock);
            for (int i = 1; i < race.gates.Length; i++) { if (race.GateStation(i) < rel) p.Cross(i, true, race.Clock); else break; }
            r.RoadPosition = rel; r.VerifiedRoad = rel; r.Travel = 0; r.TrackedStation = s; r.FinishArmed = rel > race.road.Length * .35f; r.FinishApproach = 0; r.Branch.Clear(); r.SampleOrigin(race.Clock);
            race.ResetSampling(pos, race.Clock); race.GetComponent<WrongWayGuidance>()?.Clear();
        }
        IEnumerator LevSolo(int course)
        {
            bool before = Arg("-p117before", "0") == "1"; LeviathanCourse.Before117 = before; LeviathanCourse.DataOverride = before ? Arg("-p117data", "") : null;
            Campaign.Testing = true; Conditions(); yield return QuickTo115(course); Conditions();
            flow.OpenGarage(); flow.SelectVehicle(Arg("-p106vehicle", "original")); flow.CloseGarage();
            SoloStart(1); yield return WaitRacing(); Bind(); BindPad(); AudioListener.volume = 0;
        }
        IEnumerator Audit117()
        {
            int course = int.Parse(Arg("-p115course", "8")); bool before = Arg("-p117before", "0") == "1"; string tag = before ? "before" : "after";
            var list = JsonUtility.FromJson<AuditList>(File.ReadAllText(Arg("-p117items", "")));
            var items = course == 8 ? list.LeviathanForward : list.LeviathanReverse; string d = course == 8 ? "F" : "R";
            yield return LevSolo(course);
            Note($"== audit {RacePlaylists.Titles[course]} ({tag}): {Lev()}; {items.Length} spots; blocks built {FindObjectsByType<Transform>(FindObjectsSortMode.None).Count(t => t.name.StartsWith("Leviathan road closed") || t.name.StartsWith("NO ENTRY"))}");
            var car = race.vehicle; var chase = FindAnyObjectByType<ChaseCamera>();
            for (int k = 0; k < items.Length; k++)
            {
                var it = items[k]; float s = before ? it.sOld : it.sNew; if (before && it.kind == "closeup") continue; if (s < 0) { Note($"  {k:00} {it.kind} {it.label}: no station on this line"); continue; }
                var pos = new Vector3(it.x, (it.kind == "closeup" ? it.y : race.road.At(s, out _).y) + 4, it.z); Put(car, pos, it.yaw, 0); car.Body.isKinematic = true; ProgressTo(s, car.Body.position); chase?.Snap();
                for (int f = 0; f < 20; f++) yield return null; yield return new WaitForSecondsRealtime(.4f);
                string file = $"audit-{d}-{k:00}-{it.kind}-{tag}"; yield return Snap(file);
                Note($"  {k:00} {it.kind,-7} {it.label}: at {V(car.Body.position)} heading {it.yaw:F0}, line s {s:F0}, next CP {race.Racers[0].Progress.NextGate}, gates drawn [{GatesDrawn()}], HUD '{HudLine()}' -> {file}.png");
                car.Body.isKinematic = false;
            }
            flow.QuitRace(); yield return WaitMenu();
        }
        // the course line ahead of a station for the bot, or a shortcut's line when it takes one
        IEnumerator Lap117()
        {
            int course = int.Parse(Arg("-p115course", "8")); string via = Arg("-p117via", "").Replace('_', ' '); bool record = Arg("-p117record", "1") == "1"; string d = course == 8 ? "F" : "R";
            yield return LevSolo(course);
            var car = race.vehicle; var road = race.road; var me = race.Racers[0]; float station = road.Project(car.Body.position, out _);
            Note($"== lap {RacePlaylists.Titles[course]}: {Lev()}; the pad bot on the course line{(via != "" ? ", taking " + via : "")}");
            string frames = Path.Combine(outDir, $"lap-{d}{(via != "" ? "-via" : "")}"); if (record) Directory.CreateDirectory(frames);
            var guidance = race.GetComponent<WrongWayGuidance>(); bool wasWrong = false; var wrong = new List<string>(); var hud = new List<string>(); var flags = new List<string>();
            float t0 = Time.time, lastMove = Time.time, best = 0, travelled = 0, prevS = station, nextHud = 0, nextFrame = 0; int resets = 0, frame = 0; WoodlandRoute onBranch = null; float bs = 0, bestB = 0;
            var shortcut = via != "" ? race.Branches.FirstOrDefault(b => b.title == via) : null;
            string lastHud = ""; float lastHudDist = -1;
            // -p117pilot 1: the car driven by the rivals' own driver (the road AI) instead of the pad bot (which misjudges the Forward summit flight)
            bool pilot = Arg("-p117pilot", "0") == "1"; if (pilot) { Pilot(car, me); Note("  driven by the road AI (pilot)"); }
            while (flow.State == RaceFlow.Stage.Racing && Time.time - t0 < 900 && !me.Progress.Finished)
            {
                var pos = car.Body.position; float s = road.ProjectNear(pos, station, 50, out float lat);
                float step = Mathf.Repeat(s - prevS + road.Length * .5f, road.Length) - road.Length * .5f; if (step > 0 && step < 60 && onBranch == null) travelled += step; prevS = s; station = s;
                if (travelled > best + 3) { best = travelled; lastMove = Time.time; }
                if (onBranch && bs > bestB + 3) { bestB = bs; lastMove = Time.time; }
                if (guidance && guidance.Visible != wasWrong) { wasWrong = guidance.Visible; if (wasWrong) wrong.Add($"WRONG WAY shown at {V(pos)} line s {s:F0}, racer station {race.Origin + me.RoadPosition:F0}, next CP {me.Progress.NextGate}"); }
                if (Time.time >= nextHud)
                {
                    nextHud = Time.time + 2; string h = HudLine(); hud.Add($"t {Time.time - t0:F0} {V(pos)} s {s:F0} racer {race.Origin + me.RoadPosition:F0} next {me.Progress.NextGate}: '{h}'");
                    int next = me.Progress.NextGate; float straight = next > 0 ? Vector3.Distance(Flat(race.GatePosition(next)), Flat(pos)) : 0; float along = LeviathanGuide.ToNextGate(race, me);
                    if (next > 0 && along < 2 && straight > 40) flags.Add($"FLAG: NEXT CP {next} at {along:F0} m along the course while it is {straight:F0} m away (t {Time.time - t0:F0}, {V(pos)})");
                    if (next > 0 && lastHudDist >= 0 && lastHud.Contains("CP " + next + " ") && along > lastHudDist + 120) flags.Add($"FLAG: NEXT CP {next} distance jumped {lastHudDist:F0} -> {along:F0} m (t {Time.time - t0:F0}, {V(pos)})");
                    lastHud = h; lastHudDist = along;
                }
                if (pilot) { if (record && Time.time >= nextFrame) { nextFrame = Time.time + .2f; yield return new WaitForEndOfFrame(); var tx = ScreenCapture.CaptureScreenshotAsTexture(); File.WriteAllBytes(Path.Combine(frames, $"f{frame++:00000}.jpg"), tx.EncodeToJPG(72)); Destroy(tx); } else yield return null; continue; }
                if (Time.time - lastMove > 6 || (lat > 18 && car.GroundedWheels > 0 && onBranch == null))
                {
                    Note($"  assist: {(Time.time - lastMove > 6 ? "stuck" : "off the line")} at {V(pos)} s {s:F0}: the reset button"); resets++;
                    Release(); yield return Press(GamepadButton.North); yield return new WaitForSecondsRealtime(2.5f);
                    station = road.ProjectNear(car.Body.position, station, 120, out _); prevS = station; lastMove = Time.time; onBranch = me.Branch.Route; bestB = onBranch ? onBranch.ProjectFor(true, car.Body.position, out _) : 0; continue;
                }
                Vector3 target; float cap;
                if (shortcut && onBranch == null && Near(s, shortcut.entryRoad - 30, shortcut.entryRoad + 4, road.Length)) { onBranch = shortcut; bs = 0; bestB = 0; Note($"  onto {via} at {V(pos)}"); }
                if (onBranch)
                {
                    bs = onBranch.ProjectFor(true, pos, out _); target = onBranch.AtFor(true, bs + Mathf.Clamp(car.ForwardSpeed * .8f, 6, 18), out _); cap = CurveCap(x => onBranch.AtFor(true, x, out _), bs, onBranch.recommendedSpeed + 4);
                    if (bs > onBranch.LengthFor(true) - 8) { Note($"  off {via} at {V(pos)}: racer station {race.Origin + me.RoadPosition:F0}, next CP {me.Progress.NextGate}, HUD '{HudLine()}'"); onBranch = null; station = race.Origin + me.RoadPosition; prevS = station; lastMove = Time.time; }
                }
                else target = RoadTarget(s, car.ForwardSpeed, out cap);
                Drive(car, target, Mathf.Max(cap, 7), 0, 0, 0, 22);
                if (record && Time.time >= nextFrame) { nextFrame = Time.time + .2f; yield return new WaitForEndOfFrame(); var tex = ScreenCapture.CaptureScreenshotAsTexture(); File.WriteAllBytes(Path.Combine(frames, $"f{frame++:00000}.jpg"), tex.EncodeToJPG(72)); Destroy(tex); continue; }
                yield return new WaitForFixedUpdate();
            }
            Release();
            Note($"  the pad drove {Time.time - t0:F0} s: lap {(me.Progress.Finished ? me.Progress.LapTimes.FirstOrDefault().ToString("0.0", CultureInfo.InvariantCulture) + " s" : "not finished")}, resets {resets} (game resets {me.Recoveries}), missed gates {me.Progress.MissedGates}, wrong-way warnings {wrong.Count}, frames {frame}");
            foreach (var w in wrong) Note("  " + w); foreach (var p in me.Progress.Penalties) Note("  penalty: " + p); foreach (var f in flags) Note("  " + f);
            Note($"  NEXT CP flags: {flags.Count}"); foreach (var h in hud) Note("    hud " + h);
            float wt = Time.realtimeSinceStartup; while (flow.State != RaceFlow.Stage.Results && flow.State != RaceFlow.Stage.Racing && Time.realtimeSinceStartup - wt < 20) yield return null;
            flow.QuitRace(); yield return WaitMenu();
        }
        // the radio's song caption over the race HUD (and over Free Roam's): its rectangle against the guide line's, and a shot
        IEnumerator Song117()
        {
            yield return LevSolo(int.Parse(Arg("-p115course", "9")));
            var car = race.vehicle; Drive(car, car.Body.position + car.transform.forward * 50, 12); yield return new WaitForSecondsRealtime(2); Release();
            flow.Radio?.Toggle(); yield return new WaitForSecondsRealtime(.6f);
            var song = FindObjectsByType<UnityEngine.UI.Text>(FindObjectsSortMode.None).FirstOrDefault(t => t.name == "Current song"); var guide = FindObjectsByType<RectTransform>(FindObjectsSortMode.None).FirstOrDefault(r => r.name == "Waypoint guidance");
            Rect R(RectTransform rt) { var c = new Vector3[4]; rt.GetWorldCorners(c); return new Rect(c[0].x, c[0].y, c[2].x - c[0].x, c[2].y - c[0].y); }
            if (song && guide) Note($"song caption '{song.text.Replace("\n", " / ")}' at {R(song.rectTransform)}, guide line at {R(guide)} (active {guide.gameObject.activeInHierarchy}): overlap {R(song.rectTransform).Overlaps(R(guide))}; HUD '{HudLine()}'");
            foreach (var t in FindObjectsByType<UnityEngine.UI.Text>(FindObjectsSortMode.None).Where(t => t.isActiveAndEnabled && t != song && !string.IsNullOrEmpty(t.text) && song && R(t.rectTransform).Overlaps(R(song.rectTransform)))) Note($"  overlaps text '{t.name}': '{t.text.Replace("\n", " / ")}'");
            yield return Snap("song-over-race-hud");
            flow.QuitRace(); yield return WaitMenu();
        }
        // pushed off the road at each station, then the reset button
        IEnumerator Reset117()
        {
            int course = int.Parse(Arg("-p115course", "8")); yield return LevSolo(course);
            var car = race.vehicle; var road = race.road; var me = race.Racers[0];
            Note($"== resets on {RacePlaylists.Titles[course]}: {Lev()}");
            foreach (var arg in Arg("-p117resets", "1550,1750,2540").Split(','))
            {
                float s = F(arg); var at = road.At(s, out var f); var right = Vector3.Cross(Vector3.up, f).normalized;
                // the lower side: where the ground 18 m out is lowest
                float yl = TopAt(at - right * 18), yr = TopAt(at + right * 18); var side = yr < yl ? right : -right;
                Put(car, at, Yaw(f), 6); ProgressTo(s, car.Body.position); yield return new WaitForSecondsRealtime(.5f);
                for (int k = 0; k < 30; k++) { car.Body.linearVelocity = side * 16 + Vector3.up * 2; yield return new WaitForFixedUpdate(); }
                yield return new WaitForSecondsRealtime(2.5f); var fell = car.Body.position; int next = me.Progress.NextGate;
                yield return Snap($"reset-{course}-{s:0}-fallen");
                Release(); yield return Press(GamepadButton.North); yield return new WaitForSecondsRealtime(3);
                var put = car.Body.position; float ps = road.Project(put, out float lat); var respawn = car.GetComponent<VehicleRespawn>();
                float rel = road.Relative(race.Origin + me.RoadPosition, race.Origin); int last = next == 0 ? race.gates.Length - 1 : next - 1;
                bool ok = me.RoadPosition >= race.GateStation(last) - .5f && (next == 0 || me.RoadPosition <= race.GateStation(next));
                Note($"  s {s:F0} (next CP {next}): pushed off {V(at)} -> fell to {V(fell)} ({at.y - fell.y:F1} m down); reset to {V(put)}, racer station {race.Origin + me.RoadPosition:F0} (rel {me.RoadPosition:F0}; last gate {race.GateStation(last):F0}, next {(next == 0 ? road.Length : race.GateStation(next)):F0}) {(ok ? "ON THIS PASS, after the last gate, before the next" : "WRONG")}; next CP still {me.Progress.NextGate}; {respawn?.LastRecovery} [{respawn?.RecoveryDiagnostic}]");
                yield return Snap($"reset-{course}-{s:0}-after");
            }
            flow.QuitRace(); yield return WaitMenu();
        }
        // Part D2: each way to skip the mountain, driven by the pad from where it starts (the progress there set)
        IEnumerator Skip117()
        {
            int course = int.Parse(Arg("-p115course", "8")); yield return LevSolo(course);
            var car = race.vehicle; var road = race.road; var me = race.Racers[0];
            Note($"== skipping the mountain on {RacePlaylists.Titles[course]}: {Lev()}");
            foreach (var spec in Arg("-p117skips", "").Split(';').Where(x => x.Length > 0))
            {
                // name|start station|mode: back:<station to turn round to>,<station to go on from>,<until station> / to:x,z(,x,z...) / reset
                var p = spec.Split('|'); string name = p[0]; float s0 = F(p[1]); string mode = p[2];
                var at = road.At(s0, out var f); Put(car, at, Yaw(f) + (mode.StartsWith("back") ? 180 : 0), 8); ProgressTo(s0, car.Body.position);
                int need = me.Progress.NextGate; float t0 = Time.time; var seen = new List<string>(); bool banner = false; string firstBanner = ""; var crossed = new List<int>();
                int lastNext = need; float travelled = 0; var prev = car.Body.position;
                if (mode == "reset")
                {
                    yield return new WaitForSecondsRealtime(1.5f); Release(); yield return Press(GamepadButton.North); yield return new WaitForSecondsRealtime(3);
                    Note($"  {name}: from s {s0:F0} (next CP {need}, at {race.GateStation(need):F0}), the reset button: put at {V(car.Body.position)}, racer station {race.Origin + me.RoadPosition:F0}; next CP {me.Progress.NextGate}: {(me.RoadPosition <= race.GateStation(need) ? "not past the gate" : "PAST THE GATE")}");
                    continue;
                }
                // place:x,z,yaw,until: the car put there (the way back driven), the progress kept from s0; then on along the course to station until
                if (mode.StartsWith("place"))
                {
                    var q = mode.Substring(6).Split(','); var pp = new Vector3(F(q[0]), 0, F(q[1])); float pu = F(q[3]); float sp = road.ProjectNear(new Vector3(pp.x, at.y, pp.z), pu - 150, 200, out _); // the pass before station until (the return)
                    pp.y = road.At(sp, out _).y + 3; Put(car, pp, F(q[2]), 10); ProgressTo(s0, car.Body.position); race.Racers[0].TrackedStation = float.NaN; yield return new WaitForFixedUpdate();
                    float tp = Time.time; var gone = new List<int>(); int ln = me.Progress.NextGate; bool shown = false; string first = "";
                    while (Time.time - tp < 60)
                    {
                        sp = road.ProjectNear(car.Body.position, sp, 40, out _); if (sp > pu) break;
                        if (me.Progress.NextGate != ln) { gone.Add(ln); ln = me.Progress.NextGate; }
                        if (race.MountainMissedGate >= 0 && !shown) { shown = true; first = $"at {V(car.Body.position)} {Time.time - tp:F1} s in: MOUNTAIN MISSED (CP {race.MountainMissedGate}), HUD '{HudLine()}'"; yield return new WaitForSecondsRealtime(.3f); yield return Snap($"skip-{course}-{name}-banner"); }
                        var tg = RoadTarget(sp, car.ForwardSpeed, out float cp); Drive(car, tg, Mathf.Max(cp, 7), 0, 0, 0, 22); yield return new WaitForFixedUpdate();
                    }
                    Release();
                    Note($"  {name}: progress as after s {s0:F0} (next CP {need}), put on the way back at ({q[0]}, {q[1]}) and driven on along the course to s {pu:F0}: gates counted [{string.Join(",", gone)}], next CP still {me.Progress.NextGate}, laps {me.Progress.CompletedLaps}, missed {me.Progress.MissedGates}; banner {(shown ? first : "NOT SHOWN")}; the lap cannot end until CP {need} is passed");
                    continue;
                }
                var targets = new List<Vector3>(); float back = 0, onFrom = 0, until = 0; int phase = 0; float sb = s0;
                if (mode.StartsWith("back")) { var q = mode.Substring(5).Split(','); back = F(q[0]); onFrom = F(q[1]); until = F(q[2]); }
                else { var q = mode.Substring(3).Split(','); for (int i = 0; i + 1 < q.Length; i += 2) targets.Add(new Vector3(F(q[i]), 0, F(q[i + 1]))); }
                int ti = 0; float lastMove = Time.time; var moved = car.Body.position;
                while (Time.time - t0 < 240)
                {
                    var pos = car.Body.position; travelled += Vector3.Distance(Flat(pos), Flat(prev)); prev = pos;
                    if (me.Progress.NextGate != lastNext) { crossed.Add(lastNext); lastNext = me.Progress.NextGate; }
                    if (race.MountainMissedGate >= 0 && !banner) { banner = true; firstBanner = $"t {Time.time - t0:F1} s at {V(pos)}: MOUNTAIN MISSED (CP {race.MountainMissedGate}), HUD '{HudLine()}'"; yield return Snap($"skip-{course}-{name}-banner"); }
                    if ((pos - moved).magnitude > 4) { moved = pos; lastMove = Time.time; }
                    if (Time.time - lastMove > 8) { seen.Add($"stopped at {V(pos)} after {Time.time - t0:F0} s"); break; }
                    Vector3 target; float cap = mode.StartsWith("to") ? 15 : 22;
                    if (mode.StartsWith("back"))
                    {
                        if (phase == 0) { sb = road.ProjectNear(pos, sb, 40, out _); target = road.At(sb - 16, out _); cap = 16; if (Mathf.Abs(sb - back) < 12) { phase = 1; sb = onFrom; } }
                        else { sb = road.ProjectNear(pos, sb, 40, out _); target = RoadTarget(sb, car.ForwardSpeed, out cap); if (sb > until) break; }
                    }
                    else { if (ti >= targets.Count) break; target = targets[ti]; if (Vector3.Distance(Flat(pos), Flat(target)) < 8) { ti++; continue; } }
                    Drive(car, target, cap, 0, 0, 0, 22); yield return new WaitForFixedUpdate();
                }
                Release(); var end = car.Body.position;
                Note($"  {name}: from s {s0:F0} (next CP {need}) {(mode.StartsWith("back") ? "turned round" : "across")} -> {V(end)} in {Time.time - t0:F0} s, {travelled:F0} m; gates counted on the way [{string.Join(",", crossed)}], next CP now {me.Progress.NextGate}, finished laps {me.Progress.CompletedLaps}; missed {me.Progress.MissedGates} (+{me.Progress.PenaltySeconds:0} s); banner: {(banner ? firstBanner : "none")}; HUD now '{HudLine()}'; {string.Join("; ", seen)}");
                yield return Snap($"skip-{course}-{name}-end");
            }
            flow.QuitRace(); yield return WaitMenu();
        }
    }
}
