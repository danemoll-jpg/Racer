using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Racer
{
    // 0.100 checks for Parts B-F in the built player (see Report100PlayerCheck.cs)
    public sealed partial class Report100PlayerCheck
    {
        // ---------- Part B: vehicle previews ----------
        string Page => (string)typeof(RaceMenus).GetField("page", NonPublic).GetValue(Menus);
        void Nav(string page) => typeof(RaceMenus).GetMethod("Navigate", NonPublic).Invoke(Menus, new object[] { page });
        void Home() { var m = Menus; m.ResetPages(); m.Show(); }
        // every picture on screen that shows a preview camera's texture: its size on screen against the texture and the camera
        void Measure(string where)
        {
            var cams = FindObjectsByType<Camera>(FindObjectsSortMode.None).Where(c => c.targetTexture && c.enabled).ToList(); int n = 0; float worst = 0;
            foreach (var raw in FindObjectsByType<UnityEngine.UI.RawImage>(FindObjectsSortMode.None))
            {
                if (!raw.isActiveAndEnabled || !(raw.texture is RenderTexture rt)) continue; var cam = cams.FirstOrDefault(c => c.targetTexture == rt); if (!cam) continue;
                var canvas = raw.canvas ? raw.canvas.rootCanvas : null; float scale = canvas ? canvas.scaleFactor : 1; var px = raw.rectTransform.rect.size * scale; if (px.x < 4 || px.y < 4) continue;
                float shown = px.x / px.y, drawn = cam.aspect, squish = shown / drawn - 1; worst = Mathf.Max(worst, Mathf.Abs(squish)); n++;
                Note($"  {where}: '{raw.transform.parent?.name}/{raw.name}' on screen {px.x:F0}x{px.y:F0} (aspect {shown:F3}), texture {rt.width}x{rt.height}, camera aspect {drawn:F3}: stretch {squish * 100:+0.0;-0.0}%");
            }
            Note($"{where}: {n} previews, worst stretch {worst * 100:F1}% {(worst < .02f ? "OK" : "SQUISHED")}");
        }
        IEnumerator Settle() { for (int i = 0; i < 4; i++) yield return null; yield return new WaitForSecondsRealtime(.9f); }
        IEnumerator Previews()
        {
            Campaign.Testing = true; Conditions(0, 0); flow.Save.Settings.hints = false;
            foreach (var (tag, cop, runner) in new[] { ("car", "police", "original"), ("bike", "policebike", "moto") })
            {
                flow.OpenGarage(); flow.SelectVehicle(runner); yield return Settle(); Measure($"garage ({tag})"); yield return Snap($"prev-garage-{tag}"); flow.CloseGarage(); yield return Settle();
                SplitScreen.Mode = SplitScreen.Kind.Police; SplitScreen.Solo = true; SplitScreen.CopVehicle = cop; SplitScreen.P1Vehicle = runner; SplitScreen.P2Vehicle = runner;
                SplitScreen.PoliceGame = SplitScreen.Game.CopRunner; Home(); Menus.OpenPoliceSetup(); yield return Settle(); Measure($"Cop vs Runner setup ({tag}), page {Page}"); yield return Snap($"prev-coprunner-{tag}");
                SplitScreen.PoliceGame = SplitScreen.Game.Getaway; Menus.Show(); yield return Settle(); Measure($"Getaway setup ({tag})"); yield return Snap($"prev-getaway-{tag}");
                SplitScreen.PoliceGame = SplitScreen.Game.SpeedPatrol; Menus.Show(); yield return Settle(); Measure($"Speed Patrol setup ({tag})"); yield return Snap($"prev-patrol-{tag}");
                SplitScreen.Mode = SplitScreen.Kind.Race; Home(); Menus.OpenSplitSetup(); yield return Settle(); Measure($"split-screen setup ({tag}), page {Page}"); yield return Snap($"prev-split-{tag}");
                typeof(RaceMenus).GetMethod("OpenSplitPick", NonPublic).Invoke(Menus, new object[] { false, 0 }); yield return Settle(); Measure($"split-screen garages, two halves ({tag}), page {Page}"); yield return Snap($"prev-splitgarage-{tag}");
                Home(); Menus.OpenCampaign(1); var ev = CampaignData.Events[0]; typeof(RaceMenus).GetField("campaignEvent", NonPublic).SetValue(Menus, ev.Id); typeof(RaceMenus).GetField("campaignVehicle", NonPublic)?.SetValue(Menus, runner); Nav("campaign-event"); yield return Settle();
                Measure($"campaign event page '{ev.Name}' ({tag}), page {Page}"); yield return Snap($"prev-campaign-{tag}");
                Home(); UnlockNotice.Raise(flow.Save, new UnlockNotice.Notice { id = "p100-preview-" + tag, heading = "PREVIEW CHECK", vehicle = cop }); float t0 = Time.realtimeSinceStartup;
                while (!Menus.UnlockOpen && Time.realtimeSinceStartup - t0 < 4) { Menus.Show(); yield return null; }
                yield return Settle(); Measure($"unlock panel ({tag}), open {Menus.UnlockOpen}"); yield return Snap($"prev-unlock-{tag}");
                if (Menus.UnlockOpen) typeof(RaceMenus).GetMethod("CloseUnlock", NonPublic).Invoke(Menus, null); Home(); yield return Settle();
            }
        }

        // ---------- Part C: the moon ----------
        void MoonReport(string where)
        {
            var cam = Camera.main; bool any = false;
            foreach (var m in FindObjectsByType<MoonDisc>(FindObjectsSortMode.None))
            {
                var disc = m.transform.Find("Moon disc"); if (!disc) continue; var r = disc.GetComponent<MeshRenderer>(); if (!r.enabled) continue; any = true;
                var to = disc.position - cam.transform.position; float dist = to.magnitude, elev = Mathf.Asin(to.y / dist) * Mathf.Rad2Deg, az = Mathf.Atan2(to.x, to.z) * Mathf.Rad2Deg;
                float ang = 2 * Mathf.Atan(disc.lossyScale.x * .5f / dist) * Mathf.Rad2Deg; var vp = cam.WorldToViewportPoint(disc.position); bool onScreen = vp.z > 0 && vp.x > 0 && vp.x < 1 && vp.y > 0 && vp.y < 1;
                var mat = r.sharedMaterial; Note($"{where}: moon drawn, elevation {elev:F1}°, azimuth {az:F0}°, {ang:F2}° wide = {ang / cam.fieldOfView * Screen.height:F0} px at {Screen.height} lines, phase drawn {mat.GetFloat("_Phase"):F3}, visibility {mat.GetFloat("_Visibility"):F2}, on screen {onScreen} at ({vp.x:F2},{vp.y:F2}); camera fov {cam.fieldOfView:F0}, camera pitch {cam.transform.eulerAngles.x:F1}");
            }
            if (!any) Note($"{where}: no moon drawn");
        }
        float MoonHeading() { foreach (var m in FindObjectsByType<MoonDisc>(FindObjectsSortMode.None)) { var d = m.transform.Find("Moon disc"); if (d && d.GetComponent<MeshRenderer>().enabled) { var to = d.position - Camera.main.transform.position; return Mathf.Atan2(to.x, to.z) * Mathf.Rad2Deg; } } return float.NaN; }
        IEnumerator Moon()
        {
            if (Arg("-p100mountain", "0") == "1")
            {
                // a night race on Mountain Loop (open ground): at the start, then driven by the road AI and shot when the moon is ahead
                yield return GoMountain(false); Conditions(2, 0); yield return StartRace("original", null, 1); yield return new WaitForSecondsRealtime(3f);
                MoonReport("Mountain Loop race, Night / Clear, at the start"); yield return Snap("moon-race-mountain-start");
                var car = race.vehicle; var pilot = car.gameObject.AddComponent<RoadDriver>(); pilot.Initialize(race, car, true, 1, 1); pilot.Racer = race.Racers[0]; car.GetComponent<VehicleInput>().enabled = false;
                float t0 = Time.realtimeSinceStartup; int shots = 0; float next = 0;
                while (Time.realtimeSinceStartup - t0 < 120 && shots < 2)
                {
                    yield return null; var cam = Camera.main; var d = FindObjectsByType<MoonDisc>(FindObjectsSortMode.None).Select(m => m.transform.Find("Moon disc")).FirstOrDefault(x => x && x.GetComponent<MeshRenderer>().enabled);
                    if (!d || Time.realtimeSinceStartup < next) continue; var vp = cam.WorldToViewportPoint(d.position);
                    if (vp.z > 0 && vp.x > .3f && vp.x < .7f && vp.y < .95f && !Physics.Linecast(cam.transform.position, cam.transform.position + (d.position - cam.transform.position).normalized * 600)) { shots++; next = Time.realtimeSinceStartup + 20; MoonReport($"Mountain Loop race, driving, {Time.realtimeSinceStartup - t0:F0} s"); yield return Snap($"moon-race-mountain-driving-{shots}"); }
                }
                Destroy(pilot); car.GetComponent<VehicleInput>().enabled = true; yield break;
            }
            Conditions(2, 0); yield return StartRace("original", null, 1); yield return new WaitForSecondsRealtime(3f);
            MoonReport("race, Night / Clear, at the start"); yield return Snap("moon-race-night");
            flow.QuitRace(); yield return new WaitForSecondsRealtime(2);
            yield return ToFreeRoam("original"); flow.Save.Settings.roamWeather = 0; var look = WorldLook.Current;
            foreach (var (name, day, hour) in new[] { ("full-moon-23h", 15, 23f), ("full-moon-20h", 15, 20.5f), ("crescent-day4-20h", 4, 20f), ("crescent-day27-04h", 27, 4.5f), ("new-moon-day1-00h", 1, 0.2f) })
            {
                look.SetClock(day, hour); yield return new WaitForSecondsRealtime(2f); float h = MoonHeading();
                var car = race.vehicle; car.Body.isKinematic = false; if (!float.IsNaN(h)) { Put(car.Body.position, h); } car.Body.isKinematic = true; yield return new WaitForSecondsRealtime(1.8f);
                Note($"Free Roam day {look.Day} {look.Clock}, phase {look.MoonPhase:F3}, lit {WorldLook.Illumination(look.MoonPhase):F2}"); MoonReport("Free Roam " + name); yield return Snap("moon-roam-" + name);
            }
            race.vehicle.Body.isKinematic = false;
        }
        IEnumerator Calendar()
        {
            yield return ToFreeRoam("original"); var look = WorldLook.Current;
            Note($"new save: Free Roam starts on day {look.Day} at {look.Clock}, moon phase {look.MoonPhase:F3} (0.5 = full), lit {WorldLook.Illumination(look.MoonPhase):F2}; saved roamDay {flow.Save.Settings.roamDay}");
        }

        // ---------- Part D: hidden police both ways ----------
        void Hands(bool on) { race.vehicle.GetComponent<VehicleInput>().enabled = on; }
        IEnumerator Carry(RoadNet net, List<int> path, float mps, Func<bool> stop)
        {
            var car = race.vehicle; Hands(false); var pts = path.Select(n => net.P[n]).ToList(); int i = 0; var cur = pts[0];
            while (i + 1 < pts.Count && !stop())
            {
                yield return new WaitForFixedUpdate(); float walked = mps * Time.fixedDeltaTime;
                while (walked > 0 && i + 1 < pts.Count) { float seg = Vector3.Distance(cur, pts[i + 1]); if (seg <= walked) { walked -= seg; cur = pts[i + 1]; i++; } else { cur = Vector3.MoveTowards(cur, pts[i + 1], walked); walked = 0; } }
                var dir = i + 1 < pts.Count ? (pts[i + 1] - cur) : car.transform.forward; dir.y = 0; if (dir.sqrMagnitude < .01f) dir = car.transform.forward; dir.Normalize();
                car.Body.position = cur + Vector3.up * .55f; car.Body.rotation = Quaternion.LookRotation(dir); car.Body.linearVelocity = dir * mps; car.Body.angularVelocity = Vector3.zero;
            }
        }
        IEnumerator Hidden()
        {
            yield return ToFreeRoam("original");
            float w0 = Time.realtimeSinceStartup; while (!HiddenPolice.Current && Time.realtimeSinceStartup - w0 < 20) yield return null;
            var hp = HiddenPolice.Current; if (!hp) { Note("FAIL no HiddenPolice in Free Roam"); yield break; }
            var net = hp.Net; int chases = 0, trips = 0;
            var spots = hp.Active.Concat(hp.All.Where(s => !hp.Active.Contains(s))).Take(2).ToList();
            foreach (var spot in spots)
            {
                int n = net.Nearest(spot.pos, out _, 80); var t = net.Tangent(n); int a = net.Ahead(n, -t, 150), b = net.Ahead(n, t, 110); var path = net.Path(a, b);
                if (path == null || path.Count < 4) { Note($"no road path past {spot.road} {spot.pos:F0}"); continue; }
                int limit = SpeedPatrol.LimitAt(race, spot.pos, out _);
                foreach (int dir in new[] { 1, -1 })
                {
                    // a cop that has chased you leaves its spot (Trip): put a fresh one there for each pass
                    if (!hp.Active.Contains(spot)) hp.Active.Add(spot);
                    var p = dir > 0 ? path : Enumerable.Reverse(path).ToList(); hp.LastEnd = -999; hp.AttachedAt = Time.time - 300; trips++;
                    // which way the parked car faces relative to this pass (dot of its forward with the direction of travel past it)
                    float closest = 1e9f; string atClosest = "the cop car never stood";
                    yield return Carry(net, p, SpeedPatrol.Mps(limit + 20), () =>
                    {
                        if (spot.prop) { var me = race.vehicle.Body.position; var to = me - spot.prop.Body.position; to.y = 0; float d = to.magnitude; if (d < closest) { closest = d; int lim = SpeedPatrol.LimitAt(race, me, out bool onR); var eye = spot.prop.Body.position + Vector3.up * 1.3f; var tgt = me + Vector3.up; var hit = Physics.RaycastAll(eye, (tgt - eye).normalized, (tgt - eye).magnitude, ~0, QueryTriggerInteraction.Ignore).Where(h => !h.collider.GetComponentInParent<ArcadeVehicle>()).OrderBy(h => h.distance).FirstOrDefault(); atClosest = $"closest {d:F0} m: limit there {lim}, on a road {onR}, line of sight {(hit.collider ? "blocked by " + hit.collider.name : "clear")}"; } }
                        return GetawayChase.Current != null;
                    });
                    Note("   " + atClosest);
                    float t1 = Time.realtimeSinceStartup; while (GetawayChase.Current == null && Time.realtimeSinceStartup - t1 < 5) yield return null;
                    var g = GetawayChase.Current; var travel = net.P[p[p.Count - 1]] - net.P[p[0]]; travel.y = 0;
                    Note($"spot {spot.road} at {spot.pos:F0} (car faces {Vector3.SignedAngle(travel, spot.forward, Vector3.up):F0}° from the direction of travel), pass {(dir > 0 ? "one way" : "the other way")} at {limit + 20} in a {limit}: chase started = {g != null}, flash '{HiddenPolice.Flash}'");
                    if (g != null) { chases++; yield return Snap($"hidden-{spots.IndexOf(spot)}-{(dir > 0 ? "a" : "b")}"); typeof(GetawayChase).GetMethod("Escaped", NonPublic).Invoke(g, new object[] { g.Runners[0], false }); float t3 = Time.realtimeSinceStartup; while (GetawayChase.Current != null && Time.realtimeSinceStartup - t3 < 12) yield return null; }
                    Hands(true); yield return new WaitForSecondsRealtime(.5f);
                }
            }
            Note($"hidden police both ways: {trips} trips, {chases} chases (expected {trips})");
        }

        // ---------- Part E: free look ----------
        static void PadState(Gamepad pad, Vector2 right, float trigger, bool r3) { var s = new GamepadState { rightStick = right, rightTrigger = trigger }; if (r3) s = s.WithButton(GamepadButton.RightStick); InputSystem.QueueStateEvent(pad, s); }
        static float Yaw(CameraViews v) => v ? (float)typeof(CameraViews).GetField("lookYaw", NonPublic).GetValue(v) : float.NaN;
        static float Pitch(CameraViews v) => v ? (float)typeof(CameraViews).GetField("lookPitch", NonPublic).GetValue(v) : float.NaN;
        IEnumerator Hold(Gamepad pad, Vector2 stick, bool r3, float seconds) { float t0 = Time.realtimeSinceStartup; while (Time.realtimeSinceStartup - t0 < seconds) { PadState(pad, stick, .3f, r3); yield return null; } }
        IEnumerator Release(Gamepad pad, CameraViews v, string what)
        {
            float t0 = Time.realtimeSinceStartup; while (Time.realtimeSinceStartup - t0 < 2.5f) { PadState(pad, Vector2.zero, .3f, false); yield return null; if (Mathf.Abs(Yaw(v)) < 2 && Mathf.Abs(Pitch(v)) < 2) break; }
            Note($"   {what}: released, back behind the vehicle (within 2°) after {Time.realtimeSinceStartup - t0:F2} s (yaw {Yaw(v):F1})");
        }
        IEnumerator Look(Gamepad pad, CameraViews v, string name, Vector2 stick, bool r3)
        {
            var input = race.vehicle.GetComponent<VehicleInput>();
            yield return Hold(pad, stick, r3, .9f);
            Note($"{name}: stick {stick} R3 {r3}: camera turned {Yaw(v):F0}°, view {v.ShownView}; driving input while looking: steering {input.Steering:F2}, throttle {input.Throttle:F2}, speed {race.vehicle.Body.linearVelocity.magnitude:F1} m/s");
            yield return Snap("look-" + name); yield return Release(pad, v, name);
        }
        IEnumerator FreeLook()
        {
            var pad = InputSystem.AddDevice<Gamepad>("Report100Pad1"); Conditions(0, 0); flow.Save.Settings.cameraView = 0; flow.Save.SaveSettings();
            yield return StartRace("original", null, 1); yield return new WaitForSecondsRealtime(1.5f); var v = CameraViews.Current;
            yield return Look(pad, v, "chase-left", new Vector2(-1, 0), false); yield return Look(pad, v, "chase-right", new Vector2(1, 0), false);
            yield return Look(pad, v, "chase-behind-stick", new Vector2(0, -1), false); yield return Look(pad, v, "chase-behind-r3", Vector2.zero, true);
            yield return Look(pad, v, "chase-half-left", new Vector2(-.55f, .45f), false);
            flow.Save.Settings.cameraView = 2; yield return new WaitForSecondsRealtime(1.2f);
            yield return Look(pad, v, "firstperson-left", new Vector2(-1, 0), false); yield return Look(pad, v, "firstperson-behind-r3", Vector2.zero, true);
            flow.Save.Settings.cameraView = 0; yield return new WaitForSecondsRealtime(1f);
            // keyboard / mouse: the right mouse button held, the mouse moved right
            var mouse = InputSystem.AddDevice<Mouse>("Report100Mouse"); float m0 = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup - m0 < .7f) { InputSystem.QueueStateEvent(mouse, new MouseState { delta = new Vector2(30, 4) }.WithButton(MouseButton.Right)); yield return null; }
            Note($"mouse: right button held, moved right: camera turned {Yaw(v):F0}°, tilted {Pitch(v):F0}°"); yield return Snap("look-mouse");
            float r0 = Time.realtimeSinceStartup; while (Time.realtimeSinceStartup - r0 < 2.5f) { InputSystem.QueueStateEvent(mouse, new MouseState()); yield return null; if (Mathf.Abs(Yaw(v)) < 2 && Mathf.Abs(Pitch(v)) < 2) break; }
            Note($"   mouse released: back after {Time.realtimeSinceStartup - r0:F2} s"); InputSystem.RemoveDevice(mouse);
            // split-screen: each player's own pad
            flow.QuitRace(); yield return new WaitForSecondsRealtime(2); Bind();
            var pad2 = InputSystem.AddDevice<Gamepad>("Report100Pad2");
            SplitScreen.Mode = SplitScreen.Kind.Race; SplitScreen.P1Device = pad; SplitScreen.P2Device = pad2; SplitScreen.P2Ai = false; SplitScreen.Course = 0; SplitScreen.Time = TimeOfDay.Day; SplitScreen.Weather = Weather.Clear; SplitScreen.P1Vehicle = "original"; SplitScreen.P2Vehicle = "moto"; SplitScreen.Traffic = false;
            flow.StartSplit(); float s0 = Time.realtimeSinceStartup; yield return null;
            while (Time.realtimeSinceStartup - s0 < 150) { Bind(); if (flow && race && flow.Started && !LoadingScreen.Holding && flow.State == RaceFlow.Stage.Racing) break; yield return null; }
            yield return new WaitForSecondsRealtime(4f);
            var cars = FindObjectsByType<ArcadeVehicle>(FindObjectsSortMode.None); var c1 = cars.FirstOrDefault(c => c.GetComponent<VehicleInput>()?.Device == pad); var c2 = cars.FirstOrDefault(c => c.GetComponent<VehicleInput>()?.Device == pad2);
            var v1 = c1 ? CameraViews.For(c1) : null; var v2 = c2 ? CameraViews.For(c2) : null; Note($"split-screen: player 1 car {(c1 ? c1.name : "none")} view {(v1 ? "yes" : "no")}, player 2 car {(c2 ? c2.name : "none")} view {(v2 ? "yes" : "no")}");
            float h0 = Time.realtimeSinceStartup; while (Time.realtimeSinceStartup - h0 < .9f) { PadState(pad, new Vector2(-1, 0), .3f, false); PadState(pad2, Vector2.zero, .3f, true); yield return null; }
            Note($"split-screen both at once: player 1 (stick left) turned {Yaw(v1):F0}°, player 2 (R3) turned {Yaw(v2):F0}°"); yield return Snap("look-split-both");
            float q0 = Time.realtimeSinceStartup; while (Time.realtimeSinceStartup - q0 < 2.5f) { PadState(pad, Vector2.zero, 0, false); PadState(pad2, Vector2.zero, 0, false); yield return null; if (Mathf.Abs(Yaw(v1)) < 2 && Mathf.Abs(Yaw(v2)) < 2) break; }
            Note($"   split-screen released: both back after {Time.realtimeSinceStartup - q0:F2} s ({Yaw(v1):F1}°, {Yaw(v2):F1}°)");
            flow.QuitSplit(false); yield return new WaitForSecondsRealtime(2); InputSystem.RemoveDevice(pad); InputSystem.RemoveDevice(pad2);
        }

        // ---------- Part E: the controls page and the controls card, controller only ----------
        static void Press(Gamepad pad, GamepadButton b) { InputSystem.QueueStateEvent(pad, new GamepadState().WithButton(b)); }
        static void Free(Gamepad pad) { InputSystem.QueueStateEvent(pad, new GamepadState()); }
        IEnumerator Tap(Gamepad pad, GamepadButton b) { Press(pad, b); yield return null; yield return null; Free(pad); yield return new WaitForSecondsRealtime(.22f); }
        string Focused() { var es = UnityEngine.EventSystems.EventSystem.current; var g = es ? es.currentSelectedGameObject : null; return g ? (g.GetComponentInChildren<UnityEngine.UI.Text>(true)?.text ?? g.name) : "(none)"; }
        IEnumerator Controls()
        {
            var pad = InputSystem.AddDevice<Gamepad>("Report100Pad"); yield return Tap(pad, GamepadButton.DpadDown); // the pad becomes the menu device
            flow.OpenSettings(); yield return Settle(); Nav("settings-controls"); yield return Settle(); Note($"controls page open: page {Page}, focus '{Focused()}', controller {MenuInput.Controller}");
            var seen = new List<string>(); string last = "";
            for (int i = 0; i < 60; i++) { yield return Tap(pad, GamepadButton.DpadDown); string f = Focused(); if (f == last) break; last = f; seen.Add(f.Replace('\n', ' ')); if (f.StartsWith("Back")) break; }
            Note($"walked with the d-pad: {seen.Count} rows; the free-look rows reached: {string.Join(" | ", seen.Where(x => x.Contains("Look")))}");
            yield return Snap("controls-page-pad"); string before = Page; yield return Tap(pad, GamepadButton.East); yield return Settle(); Note($"B: page '{before}' -> '{Page}', state {flow.State}");
            for (int i = 0; i < 3 && flow.State == RaceFlow.Stage.Settings; i++) { yield return Tap(pad, GamepadButton.East); yield return Settle(); }
            Note($"after B: state {flow.State}");
            // the controls card before a race (held until a press)
            Conditions(0, 0); flow.OpenGarage(); flow.SelectVehicle("original"); flow.CloseGarage(); race.opponents = false; race.laps = 1; flow.StartRace();
            float t0 = Time.realtimeSinceStartup; while (flow.State != RaceFlow.Stage.Countdown && Time.realtimeSinceStartup - t0 < 30) yield return null;
            typeof(RaceFlow).GetProperty("ControlsCard").SetValue(flow, true); typeof(RaceFlow).GetField("controlsShownAt", NonPublic)?.SetValue(flow, Time.unscaledTime);
            yield return Settle(); Note($"controls card shown {flow.ControlsCard} (controller {MenuInput.Controller})"); yield return Snap("controls-card-pad");
            yield return Tap(pad, GamepadButton.South); yield return Settle(); Note($"after A: controls card {flow.ControlsCard}, state {flow.State}");
            InputSystem.RemoveDevice(pad);
        }

        // ---------- Part F: the storm drain ----------
        // a path follower (as the editor's Pilot097): steers along the points at a set speed, stepped from the check's fixed-update loop
        sealed class PathDriver
        {
            public ArcadeVehicle Car; public List<Vector3> Path; public float Speed, Look; public bool Done; int seg;
            public void Step(float dt)
            {
                if (!Car || Path.Count < 2 || Car.Body.isKinematic) return; Car.enabled = false; var p = Car.Body.position; float speed = Car.ForwardSpeed;
                while (seg + 1 < Path.Count - 1) { var a = Path[seg]; var b = Path[seg + 1]; var ab = b - a; ab.y = 0; var ap = p - a; ap.y = 0; if (Vector3.Dot(ap, ab) > ab.sqrMagnitude) seg++; else break; }
                float left = Look; int i = seg; var cur = p; Vector3 target = Path[Path.Count - 1];
                while (i + 1 < Path.Count) { var end = Path[i + 1]; float len = Vector3.Distance(new Vector3(cur.x, 0, cur.z), new Vector3(end.x, 0, end.z)); if (len >= left) { var d = end - cur; d.y = 0; target = cur + d.normalized * left; break; } left -= len; cur = end; i++; }
                Done = Vector3.Distance(new Vector3(p.x, 0, p.z), new Vector3(Path[^1].x, 0, Path[^1].z)) < 4;
                float want = Done ? 0 : Speed; var local = Car.transform.InverseTransformPoint(target); float angle = Mathf.Atan2(local.x, local.z);
                float maxAngle = Mathf.Lerp(Car.slowSteerAngle, Car.fastSteerAngle, Mathf.Clamp01(Mathf.Abs(speed) / Car.topSpeed)) * Mathf.Deg2Rad;
                float steering = Mathf.Clamp(Mathf.Atan(2 * Car.wheelbase * Mathf.Sin(angle) / Look) / maxAngle, -1, 1);
                Car.Simulate(speed < want ? Mathf.Clamp01((want - speed) * .5f) : 0, speed > want + .5f ? Mathf.Clamp01((speed - want) * .3f) : 0, steering, dt);
            }
        }
        const string Roof = "Culvert roof top (0.100)";
        static float RoofAt(Vector3 p) { foreach (var h in Physics.RaycastAll(new Vector3(p.x, 120, p.z), Vector3.down, 200, ~0, QueryTriggerInteraction.Ignore)) if (h.collider.name == Roof) return h.point.y; return float.NaN; }
        IEnumerator Drive(string name, List<Vector3> path, float speed)
        {
            var car = race.vehicle; Hands(false); var d0 = path[1] - path[0]; d0.y = 0; Put(path[0], Quaternion.LookRotation(d0).eulerAngles.y);
            yield return new WaitForSecondsRealtime(.4f); var resp = car.GetComponent<VehicleRespawn>(); int resets = 0; Action onR = () => resets++; resp.Respawned += onR;
            var pilot = new PathDriver { Car = car, Path = path, Speed = speed, Look = 7 };
            float t0 = Time.time, below = 0, onRoof = 0, minUp = 1; bool inside = false;
            while (!pilot.Done && Time.time - t0 < 40)
            {
                AudioListener.volume = 0; yield return new WaitForFixedUpdate(); pilot.Step(Time.fixedDeltaTime); var p = car.Body.position; minUp = Mathf.Min(minUp, car.transform.up.y);
                float r = RoofAt(p); if (!float.IsNaN(r) && p.y > r - .5f) onRoof += Time.fixedDeltaTime;
                if (Physics.Raycast(p + Vector3.up * .5f, Vector3.up, out var up, 9, ~0, QueryTriggerInteraction.Ignore) && up.collider.name == "Long storm culvert walls and ceiling") { inside = true; below = Mathf.Max(below, r - p.y); }
            }
            resp.Respawned -= onR; yield return null; car.enabled = true; Hands(true);
            Note($"{name}: {(pilot.Done ? "reached the far side" : "DID NOT reach the far side")} in {Time.time - t0:F1} s, {onRoof:F1} s on the roof, {(inside ? "INSIDE THE TUNNEL (fell in)" : "never inside the tunnel")}, resets {resets}, lowest up {minUp:F2}, end {V(car.Body.position)}");
        }
        IEnumerator Drain()
        {
            string vehicle = Arg("-p100vehicle", "original"); yield return ToFreeRoam(vehicle); AudioListener.volume = 0;
            WorldLook.Current?.Pin(LookPresets.Compose(TimeOfDay.Day, Weather.Clear));
            List<Vector3> P(params float[] xz) { var l = new List<Vector3>(); for (int i = 0; i < xz.Length; i += 2) l.Add(Ground(new Vector3(xz[i], 0, xz[i + 1]))); return l; }
            // across the middle from the north-west, across from the south-east, and along it from the south-west end then off to the north-west
            yield return Drive($"{vehicle} onto the top from the north-west, across, off the south-east side", P(141.5f, 70f, 146.5f, 65.5f, 152f, 60f, 157.5f, 55f, 163f, 50f), 9);
            yield return Drive($"{vehicle} onto the top from the south-east, across, off the north-west side", P(158f, 41f, 152.5f, 46f, 147f, 51f, 141.5f, 56f, 136f, 61f), 9);
            yield return Drive($"{vehicle} onto the top at the south-west end, along it, off the north-west side", P(132f, 33f, 137f, 39f, 141f, 45f, 145f, 51f, 149f, 57f, 144f, 63f, 139f, 68f), 8);
            // shots: the top and the open entrance
            var cam = Camera.main; var chase = FindAnyObjectByType<ChaseCamera>(); var cv = CameraViews.Current; if (chase) chase.enabled = false; if (cv) cv.enabled = false; HideHud(true);
            cam.transform.SetPositionAndRotation(new Vector3(178, 79, 38), Quaternion.LookRotation(new Vector3(152, 62, 60) - new Vector3(178, 79, 38))); yield return new WaitForSecondsRealtime(.6f); yield return Snap($"drain-top-{vehicle}");
            cam.transform.SetPositionAndRotation(new Vector3(190, 70, 73), Quaternion.Euler(6, 270, 0)); yield return new WaitForSecondsRealtime(.6f); yield return Snap($"drain-entrance-{vehicle}");
            if (chase) chase.enabled = true; if (cv) cv.enabled = true; HideHud(false);
            // through the tunnel both ways (as 0.98: the Storm Drain trail)
            var wr = FindObjectsByType<WoodlandRoute>(FindObjectsSortMode.None).FirstOrDefault(w => w.title != null && w.title.Contains("Storm Drain")); if (!wr) { Note("no Storm Drain trail"); yield break; }
            wr.Initialize(); var pts = new List<Vector3>(); for (float s = 0; s < wr.Length; s += 5) pts.Add(wr.At(s, out _)); pts.Add(wr.At(wr.Length - .1f, out _));
            int first = pts.FindIndex(p => p.z < 82 && p.z > -80 && p.x > 100 && p.x < 180), last = pts.FindLastIndex(p => p.z < 82 && p.z > -80 && p.x > 100 && p.x < 180);
            first = Mathf.Max(0, first - 6); last = Mathf.Min(pts.Count - 1, last + 6); var path = pts.GetRange(first, last - first + 1);
            foreach (int dir in new[] { 1, -1 })
            {
                var p = new List<Vector3>(path); if (dir < 0) p.Reverse(); var car = race.vehicle; Hands(false); var d0 = p[1] - p[0]; d0.y = 0; Put(p[0], Quaternion.LookRotation(d0).eulerAngles.y);
                var resp = car.GetComponent<VehicleRespawn>(); int resets = 0; Action onR = () => resets++; resp.Respawned += onR;
                var pilot = new PathDriver { Car = car, Path = p, Speed = vehicle == "moto" ? 14 : 12, Look = 9 };
                float t0 = Time.time, minUp = 1, stuck = 0; var lastPos = car.Body.position; float lastMove = Time.time;
                while (!pilot.Done && Time.time - t0 < 90) { AudioListener.volume = 0; yield return new WaitForFixedUpdate(); pilot.Step(Time.fixedDeltaTime); minUp = Mathf.Min(minUp, car.transform.up.y); if (Vector3.Distance(car.Body.position, lastPos) > 1.5f) { lastPos = car.Body.position; lastMove = Time.time; } if (Time.time - lastMove > 6) { stuck = Time.time - lastMove; break; } }
                resp.Respawned -= onR; yield return null; car.enabled = true;
                Note($"{vehicle} {(dir > 0 ? "down" : "up")} the storm drain tunnel ({p.Count} points from {V(p[0])} to {V(p[^1])}): {(pilot.Done ? "reached the far end" : "DID NOT reach the end")} in {Time.time - t0:F1} s, resets {resets}, stuck {stuck:F0} s, lowest up {minUp:F2}");
            }
            Hands(true);
        }
    }
}
