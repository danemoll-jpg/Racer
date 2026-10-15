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
    // 0.118 evidence only (the 0.106 check component: -p106check <dir> -racerTestSave <dir>; never active without both):
    //   -p106kind shots118  Leviathan (-p115course 8 / 9), solo: each of -p118spots "name|x|z|yaw|station;..." the car stopped there, the race's
    //                       gate progress set as a driver at that station would have it, a driver's-view shot (and the closures and gates near it).
    //                       Then Kyle's bank: the ground under the line every 2 m with its grade, the trunks on the bank and the ground under each,
    //                       the driveway gate's posts against their ground.
    //   -p106kind seg118    Leviathan, solo: the pad bot along the course from station -p118from to -p118to (-p118lane m to the right of the line),
    //                       a frame every -p118every s (0 = none), camera view -p118view (0 chase .. 2 first person). Every 0.25 s: the next CP, whether
    //                       its gate is on screen and seen (nothing solid between the camera and its bar); when a gate is passed, whether the next one is
    //                       seen within 1.5 s; every wrong-way warning (with the racer's station and the stations of both passes there); missed gates;
    //                       resets. -p118resets "s,s": the reset button pressed as the bot passes each station, then on.
    //   -p106kind rider118  Part C: first-person shots in Free Roam (-p118vehicles) at rest and at full steer each way; a chase-view shot of the
    //                       rider; the garage Rider page with the controller only: Hair stepped through every style (a shot each), B back to the main
    //                       menu; the rider in settings.json before and after.
    public sealed partial class Report106PlayerCheck
    {
        IEnumerator Shots118()
        {
            int course = int.Parse(Arg("-p115course", "8")); yield return LevSolo(course);
            var car = race.vehicle; var chase = FindAnyObjectByType<ChaseCamera>();
            Note($"== spots on {RacePlaylists.Titles[course]}: {Lev()}");
            foreach (var spec in Arg("-p118spots", "").Split(';').Where(x => x.Length > 0))
            {
                var f = spec.Split('|'); var pos = new Vector3(F(f[1]), 0, F(f[2])); float yaw = F(f[3]), s = F(f[4]);
                pos.y = race.road.At(s, out _).y + 4; Put(car, pos, yaw, 0); car.Body.isKinematic = true; ProgressTo(s, car.Body.position); chase?.Snap();
                for (int k = 0; k < 20; k++) yield return null; yield return new WaitForSecondsRealtime(.4f);
                yield return Snap("spot-" + f[0]);
                var near = FindObjectsByType<Transform>(FindObjectsSortMode.None).Where(t => (t.name.StartsWith("Leviathan road closed") || t.name.StartsWith("NO ENTRY")) && Vector3.Distance(Flat(t.position), Flat(pos)) < 60).Select(t => $"{t.name} at {V(t.position)} ({Vector3.Distance(Flat(t.position), Flat(pos)):F0} m)");
                Note($"  {f[0]}: at {V(car.Body.position)} heading {yaw:F0}, line s {s:F0}, next CP {race.Racers[0].Progress.NextGate}, gates drawn [{GatesDrawn()}], HUD '{HudLine()}'; closures within 60 m: {(near.Any() ? string.Join("; ", near) : "none")}");
                car.Body.isKinematic = false;
            }
            // Kyle's bank (the link's stations on this line: from the driveway's end to the lake path, both passes)
            var road = race.road; Note("  Kyle's bank, the ground under the line (top surface; course ground included):");
            foreach (var span in Arg("-p118bank", "").Split(';').Where(x => x.Length > 0))
            {
                var ab = span.Split(','); float a = F(ab[0]), b = F(ab[1]); float prevY = float.NaN; Vector3 prev = default; float worst = 0; var rows2 = new List<string>();
                for (float s = a; s <= b; s += 2)
                {
                    var p = road.At(s, out _); float y = TopAt(p);
                    if (!float.IsNaN(prevY)) { float g = Mathf.Abs(y - prevY) / Mathf.Max(.1f, Vector3.Distance(Flat(p), Flat(prev))); worst = Mathf.Max(worst, g); rows2.Add($"s {s:F0} ({p.x:F1}, {p.z:F1}) {y:F2} m {100 * g:F0} %"); }
                    prevY = y; prev = p;
                }
                Note($"    s {a:F0}-{b:F0}: steepest 2 m step {100 * worst:F0} %: " + string.Join("; ", rows2));
            }
            foreach (var box in FindObjectsByType<BoxCollider>(FindObjectsInactive.Include, FindObjectsSortMode.None).Where(b => b.name.IndexOf("trunk", System.StringComparison.OrdinalIgnoreCase) >= 0))
            {
                var t = box.transform; var bottom = t.TransformPoint(box.center - Vector3.up * box.size.y * .5f); if (bottom.x < 500 || bottom.x > 530 || bottom.z < -26 || bottom.z > 2) continue;
                float g = float.NaN; foreach (var h in Physics.RaycastAll(bottom + Vector3.up * 3, Vector3.down, 10, ~0, QueryTriggerInteraction.Ignore)) if (h.collider.name.StartsWith("Ground_")) g = float.IsNaN(g) ? h.point.y : Mathf.Max(g, h.point.y);
                Note($"    trunk at {bottom.x:F1},{bottom.z:F1}: bottom {bottom.y:F2}, ground {g:F2} ({(float.IsNaN(g) ? "no ground" : Mathf.Abs(bottom.y - g) < .15f ? "on the ground" : bottom.y > g ? $"FLOATS {bottom.y - g:F2} m" : $"in the ground {g - bottom.y:F2} m (a trunk's foot)")}){(box.enabled ? "" : " (cleared for the race)")}");
            }
            // every gate's posts against the ground under them (the gates built for the course; the scene's own are as they were)
            int floating = 0;
            foreach (var gate in race.gates.Where(g => g))
            {
                var posts = gate.GetComponentsInChildren<Transform>().Where(t => t.name == "Non-colliding gate marking" && t.localScale.y >= 3.9f).ToList(); if (posts.Count < 2) continue;
                var gaps = posts.Select(t => { var foot = t.TransformPoint(Vector3.down * .5f); return foot.y - TopAt(foot); }).ToList();
                if (gaps.Max() > .1f || gate.name.Contains("Kyle")) { if (gaps.Max() > .1f) floating++; Note($"    {gate.name}: posts' feet {string.Join(" / ", gaps.Select(x => (x >= 0 ? "+" : "") + x.ToString("0.00")))} m from the ground under them"); }
            }
            Note($"    gates with a post more than 0.1 m above its ground: {floating} of {race.gates.Length}");
            flow.QuitRace(); yield return WaitMenu();
        }

        IEnumerator Seg118()
        {
            int course = int.Parse(Arg("-p115course", "9")); float from = F(Arg("-p118from", "4000")), to = F(Arg("-p118to", "6420")), lane = F(Arg("-p118lane", "0")), every = F(Arg("-p118every", ".2"));
            int view = int.Parse(Arg("-p118view", "0")); string tag = Arg("-p118tag", "seg"); var resetAt = Arg("-p118resets", "").Split(',').Where(x => x.Length > 0).Select(F).ToList();
            yield return LevSolo(course);
            var car = race.vehicle; var road = race.road; var me = race.Racers[0]; var chase = FindAnyObjectByType<ChaseCamera>();
            CameraViews.Current?.SetPlayerView((CameraViews.View)view);
            var start = road.At(from, out var f0); Put(car, start, Yaw(f0), 8); ProgressTo(from, car.Body.position); chase?.Snap(); yield return new WaitForSecondsRealtime(.5f);
            Note($"== {tag}: {RacePlaylists.Titles[course]}: {Lev()}; the pad bot from s {from:F0} to s {to:F0}, {lane:F1} m right of the line, view {(CameraViews.View)view}, vehicle {car.GetComponent<VehicleConfiguration>().profileId}");
            string frames = Path.Combine(outDir, tag); if (every > 0) Directory.CreateDirectory(frames);
            var guidance = race.GetComponent<WrongWayGuidance>(); bool wasWrong = false; var wrong = new List<string>(); var gates = new List<string>();
            float station = from, t0 = Time.time, nextFrame = 0, nextLog = 0, lastMove = Time.time, best = 0; int frame = 0, resets = 0, lastNext = me.Progress.NextGate; float passedAt = Time.time; bool seenSincePass = false; float seenTime = 0, logTime = 0; Vector3 moved = car.Body.position;
            // -p118pilot 1: driven by the rivals' own driver (the road AI) instead of the pad bot (which cannot drive Downhill Ridge Cut's drop)
            bool pilot = Arg("-p118pilot", "0") == "1"; if (pilot) Pilot(car, me);
            var cam = Camera.main; var air = new List<string>(); float airFrom = -1; Vector3 airAt = default; float airS = 0, airV = 0, top = 0;
            bool Seen(int g, out bool onScreen)
            {
                onScreen = false; if (g <= 0 || g >= race.gates.Length || !cam) { return false; }
                var top = race.GatePosition(g) + race.gates[g].transform.up * 2.5f; var vp = cam.WorldToViewportPoint(top); onScreen = vp.z > 0 && vp.x > 0 && vp.x < 1 && vp.y > 0 && vp.y < 1;
                if (!onScreen) return false; var c = cam.transform.position; var d = top - c;
                foreach (var h in Physics.RaycastAll(c, d.normalized, d.magnitude, ~0, QueryTriggerInteraction.Ignore)) if (!h.collider.attachedRigidbody) return false;
                return true;
            }
            while (flow.State == RaceFlow.Stage.Racing && Time.time - t0 < 600)
            {
                var pos = car.Body.position; float s = road.ProjectNear(pos, station, 40, out _); station = s;
                if (road.Relative(s, from) > road.Relative(to, from)) break;
                if ((pos - moved).magnitude > 3) { moved = pos; lastMove = Time.time; }
                top = Mathf.Max(top, car.ForwardSpeed);
                // every time off the ground (no wheel down) for more than 0.15 s: where, how long, how fast
                if (car.GroundedWheels == 0) { if (airFrom < 0) { airFrom = Time.time; airAt = pos; airS = s; airV = car.ForwardSpeed; } }
                else if (airFrom >= 0) { if (Time.time - airFrom > .15f) air.Add($"  off the ground {Time.time - airFrom:F2} s from {V(airAt)} (s {airS:F0}) to {V(pos)} (s {s:F0}) at {airV * 2.237f:F0} mph"); airFrom = -1; }
                if (guidance && guidance.Visible != wasWrong)
                {
                    wasWrong = guidance.Visible;
                    if (wasWrong) { float other = road.Project(pos, out _); wrong.Add($"WRONG WAY at {V(pos)} t {Time.time - t0:F1}: line s {s:F0}, racer station {race.Origin + me.RoadPosition:F0} (tracked {me.TrackedStation:F0}), nearest point of any pass s {other:F0}, next CP {me.Progress.NextGate}"); }
                }
                int next = me.Progress.NextGate; bool sn = Seen(next, out bool on);
                if (next != lastNext) { if (lastNext > 0) gates.Add($"  passed CP {lastNext} at {V(pos)} t {Time.time - t0:F1} -> next CP {next}: {(seenSincePass ? "seen" : "NOT YET SEEN")}"); lastNext = next; passedAt = Time.time; seenSincePass = false; }
                if (sn && !seenSincePass) { seenSincePass = true; if (Time.time - passedAt > 1.5f) gates.Add($"    CP {next} first seen {Time.time - passedAt:F1} s after the gate before"); else gates.Add($"    CP {next} seen {Time.time - passedAt:F1} s after the gate before"); }
                if (Time.time >= nextLog) { nextLog = Time.time + .25f; logTime += .25f; if (sn) seenTime += .25f; }
                foreach (var r in resetAt.ToArray()) if (road.Relative(s, from) >= road.Relative(r, from)) { resetAt.Remove(r); Note($"  the reset button at s {s:F0} ({V(pos)})"); Release(); yield return Press(GamepadButton.North); yield return new WaitForSecondsRealtime(2.5f); station = road.ProjectNear(car.Body.position, station, 120, out _); Note($"    put at {V(car.Body.position)}, racer station {race.Origin + me.RoadPosition:F0}, next CP {me.Progress.NextGate}"); resets++; lastMove = Time.time; }
                if (!pilot && Time.time - lastMove > 6) { Note($"  assist: stuck at {V(pos)} s {s:F0}: the reset button"); resets++; Release(); yield return Press(GamepadButton.North); yield return new WaitForSecondsRealtime(2.5f); station = road.ProjectNear(car.Body.position, station, 120, out _); lastMove = Time.time; continue; }
                if (!pilot) { var target = RoadTarget(s, car.ForwardSpeed, out float cap); road.At(s + 10, out var tf); target += Vector3.Cross(Vector3.up, tf).normalized * lane; Drive(car, target, Mathf.Max(cap, 7), 0, 0, 0, 22); }
                if (every > 0 && Time.time >= nextFrame) { nextFrame = Time.time + every; yield return new WaitForEndOfFrame(); var tex = ScreenCapture.CaptureScreenshotAsTexture(); File.WriteAllBytes(Path.Combine(frames, $"f{frame++:00000}.jpg"), tex.EncodeToJPG(80)); Destroy(tex); continue; }
                yield return new WaitForFixedUpdate();
            }
            Release();
            Note($"  driven {Time.time - t0:F0} s to s {station:F0}: missed gates {me.Progress.MissedGates}, resets {resets} (game resets {me.Recoveries}), wrong-way warnings {wrong.Count}, next gate seen {100 * seenTime / Mathf.Max(.25f, logTime):F0} % of the time, frames {frame}");
            Note($"  top speed {top * 2.237f:F0} mph; times off the ground over 0.15 s: {air.Count}"); foreach (var a in air) Note(a);
            foreach (var w in wrong) Note("  " + w); foreach (var g in gates) Note(g); foreach (var p in me.Progress.Penalties) Note("  penalty: " + p);
            CameraViews.Current?.SetPlayerView(CameraViews.View.Chase);
            flow.QuitRace(); yield return WaitMenu();
        }

        IEnumerator Rider118()
        {
            string settings = Path.Combine(Arg("-racerTestSave", ""), "settings.json");
            string Look() { var l = flow.Save.Settings.rider; return l == null ? "(none)" : JsonUtility.ToJson(l); }
            string before = Look(); Note($"rider in settings.json before: {before}");
            // the garage's Rider page by the controller only
            Conditions(); yield return new WaitForSecondsRealtime(1.5f); Note($"main menu: page '{Page}', focus '{Focus}'");
            yield return FocusRow("garage"); Note($"  focus '{Focus}' (D-pad down)"); yield return Press(GamepadButton.South); yield return new WaitForSecondsRealtime(1.2f);
            Note($"  A: page '{Page}', focus '{Focus}'");
            yield return FocusRow("rider"); Note($"  focus '{Focus}'"); yield return Press(GamepadButton.South); yield return new WaitForSecondsRealtime(1.2f);
            Note($"  A: page '{Page}', focus '{Focus}'"); yield return Snap("rider-page");
            // the hat off for the hair shots (a hat hides the hair above its band), by the controller: down to Hat, left to None, back up to Hair
            int hat0 = flow.Save.Settings.rider.hat;
            yield return FocusRow("rider-4"); for (int i = 0; i < 6 && flow.Save.Settings.rider.hat != 0; i++) yield return Press(GamepadButton.DpadLeft);
            Note($"  focus '{Focus}': '{RowText("rider-4")}' (the hat off for the hair shots)"); yield return Press(GamepadButton.DpadUp, 2);
            Note($"  focus '{Focus}': '{RowText("rider-2")}'");
            for (int i = 0; i < RiderLook.Hairs.Length; i++)
            {
                yield return Press(GamepadButton.DpadRight); yield return new WaitForSecondsRealtime(.8f);
                string row = RowText("rider-2"); Note($"  D-pad right: '{row}' (focus '{Focus}')");
                int hair = flow.Save.Settings.rider.hair; if (hair >= 5) yield return Snap($"rider-hair-{hair}-{RiderLook.Hairs[hair].Replace(' ', '-').ToLowerInvariant()}");
            }
            yield return Press(GamepadButton.DpadDown, 2); for (int i = 0; i < 6 && flow.Save.Settings.rider.hat != hat0; i++) yield return Press(GamepadButton.DpadRight);
            Note($"  focus '{Focus}': '{RowText("rider-4")}' (the hat back on)");
            yield return Back(); Note($"  B: page '{Page}', focus '{Focus}'"); yield return Back(); Note($"  B: page '{Page}', focus '{Focus}'"); yield return Back(); Note($"  B: page '{Page}', focus '{Focus}'");
            foreach (var id in Arg("-p118vehicles", "fastback,moto,stuntbike").Split(','))
            {
                yield return ToFreeRoam(id); Bind(); yield return Vehicle(id); Bind(); BindPad(); var car = race.vehicle; Release(); yield return new WaitForSecondsRealtime(1.5f);
                CameraViews.Current?.SetPlayerView(CameraViews.View.FirstPerson); yield return new WaitForSecondsRealtime(1);
                var cfg = car.GetComponent<VehicleConfiguration>(); var profile = VehicleProfile.All.FirstOrDefault(p => p.Id == cfg.profileId);
                Note($"{id}: {profile?.Name} (pose {profile?.Pose}), rider parts {string.Join(" ", car.GetComponentsInChildren<MeshRenderer>(true).Where(r => r.name.Contains("_H") || r.name.Contains("Hair")).Select(r => r.name).Distinct().Take(12))}");
                yield return Snap($"rider-fp-{id}-rest");
                foreach (float steer in new[] { -1f, 1f })
                {
                    for (int k = 0; k < 40; k++) { InputSystem.QueueStateEvent(pad, new GamepadState { leftStick = new Vector2(steer, 0), leftTrigger = 1 }); yield return new WaitForFixedUpdate(); }
                    yield return Snap($"rider-fp-{id}-steer-{(steer < 0 ? "left" : "right")}"); Release(); yield return new WaitForSecondsRealtime(.5f);
                }
                if (id == "moto") { CameraViews.Current?.SetPlayerView(CameraViews.View.Chase); yield return new WaitForSecondsRealtime(1.2f); yield return Snap("rider-chase-moto"); }
                CameraViews.Current?.SetPlayerView(CameraViews.View.Chase);
                flow.QuitRace(); yield return WaitMenu();
            }
            string after = Look(); Note($"rider in settings.json after (every style stepped through once, back where it was): {after}: {(after == before ? "UNCHANGED" : "CHANGED")}");
            Note($"settings.json on disk: {(File.Exists(settings) ? File.ReadAllText(settings).Contains("\"hair\": " + flow.Save.Settings.rider.hair) ? "hair " + flow.Save.Settings.rider.hair : "?" : "missing")}");
        }
    }
}
