using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.InputSystem;

namespace Racer
{
    // 0.77 Part A: Trailer Mode, for filming footage and screenshots (also a photo mode), in Free Roam and in races. F8 or the
    // pause menu entry "Trailer Mode" turns it on and off; it stays on across scene changes until turned off. It changes
    // nothing about gameplay, physics or AI. A race run in Trailer Mode does not count for records (as with Debug movement).
    // While it is on: nothing is drawn on screen (H brings the HUD back, G the arrows / gates / waypoint beacon), the cameras
    // of CameraViews.Trailer.cs, slow motion, a screenshot key, and in Free Roam the time of day, clock, weather, moon and
    // lightning on demand - none of it is written to the save; turning the mode off restores the Free Roam clock, calendar
    // and weather exactly. Bindings: Docs/TrailerMode/TRAILER_MODE.md and (0.79) the on-screen controls panel
    // (TrailerMode.Panel.cs), shown when the mode starts and hidden with H.
    [DefaultExecutionOrder(900)]
    public sealed partial class TrailerMode : MonoBehaviour
    {
        public static bool Active { get; private set; }
        public static TrailerMode Instance { get; private set; }
        RaceFlow flow; Canvas hud;
        float hintUntil; // 0.77 hint timer; still set by the 0.77 evidence tools (TrailerShots), unused since the 0.79 panel
        public bool HudShown { get; private set; }
        public bool GuidesShown { get; private set; }
        public bool HalfSpeed { get; private set; }
        public float Speed { get; private set; } = 1;
        public string LastScreenshot { get; private set; }
        public static string ScreenshotFolder => Path.Combine(Application.persistentDataPath, "Screenshots");
        float baseFixed; bool slowed, cleaned, freeHeld; bool screenshotRequested; int screenshotsTaken;
        bool slowHeldByTest;
        readonly List<Renderer> guides = new(); bool guidesHidden;

        public static void Attach(RaceFlow owner)
        {
            var mode = owner.gameObject.AddComponent<TrailerMode>(); mode.flow = owner; Instance = mode;
            mode.baseFixed = Time.fixedDeltaTime;
            var raceHud = FindAnyObjectByType<RaceHud>(); mode.hud = raceHud ? raceHud.GetComponent<Canvas>() : null;
            mode.BuildPanel();
            if (Active) mode.Began();
        }
        void OnDestroy() { if (Instance == this) Instance = null; Restore(); }
        public void Toggle() { if (Active) End(); else Begin(); }
        public void Begin()
        {
            if (Active) return;
            Active = true; Began(); flow.Click(); flow.RefreshMenu();
        }
        void Began()
        {
            HudShown = false; GuidesShown = false; HalfSpeed = false; PanelShown = true; PanelFocused = false;
            CameraViews.Current?.TrailerStarted();
            if (ConditionsAvailable) Conditions();
        }
        public void End()
        {
            if (!Active) return;
            Active = false; Restore(); CameraViews.Current?.TrailerEnded(); flow.Click(); flow.RefreshMenu();
        }
        // Everything Trailer Mode changed goes back: speed, sound level, screen, guides, vehicle controls, conditions.
        void Restore()
        {
            if (slowed) { slowed = false; Time.fixedDeltaTime = baseFixed; if (flow && (flow.State == RaceFlow.Stage.Racing || flow.State == RaceFlow.Stage.Countdown)) Time.timeScale = 1; if (flow && flow.Save != null) AudioListener.volume = flow.Save.Settings.master; }
            Speed = 1; HalfSpeed = false;
            if (cleaned) { cleaned = false; if (hud) hud.enabled = true; var debug = DebugCanvas; if (debug) debug.enabled = true; }
            ShowGuides(true);
            if (freeHeld) { freeHeld = false; ReleaseVehicle(); }
            if (WorldLook.Current) WorldLook.Current.Trailer = null;
            HidePanel();
        }
        static Canvas DebugCanvas => DeveloperLocationHud.Instance ? DeveloperLocationHud.Instance.GetComponent<Canvas>() : null;
        bool Driving => flow.State == RaceFlow.Stage.Racing || flow.State == RaceFlow.Stage.Countdown;

        void Update()
        {
            if (!flow || flow.Save == null || flow.State == RaceFlow.Stage.Title) return;
            var k = Keyboard.current; var g = Gamepad.current;
            if (k?.f8Key.wasPressedThisFrame == true && !DeveloperLocationHud.OwnsInput) Toggle();
            if (!Active) return;
            // Free Roam: the saved clock stands still from the moment the mode is on (the trailer clock carries on from the
            // same hour), so turning the mode off restores it exactly.
            if (ConditionsAvailable && WorldLook.Current.Trailer == null) Conditions();
            // A race in Trailer Mode does not count (the same rule as Debug movement).
            if (!flow.Race.FreeRoam && (Driving || flow.State == RaceFlow.Stage.Paused) && !flow.DebugMovementUsed) flow.MarkDebugMovement("TRAILER MODE");
            var views = CameraViews.Current;
            bool controls = Driving && !flow.MenuVisible && !MenuInput.Blocked && flow.GetComponent<ExplorationMap>()?.OwnsInput != true;
            // controller B: the panel's focus. B is also Back, which RaceFlow consumes first this frame (a no-op while
            // driving), so this does not wait for the release; a B that closed a menu (menu visible last frame) is ignored.
            if (g != null && g.buttonEast.wasPressedThisFrame && Driving && !menuLastFrame && !flow.MenuVisible && !DeveloperLocationHud.OwnsInput && flow.GetComponent<ExplorationMap>()?.OwnsInput != true) FocusPanel(!PanelFocused);
            float slowTarget = HalfSpeed ? .5f : 1;
            if (controls && views)
            {
                if (k != null)
                {
                    for (int i = 0; i < 9; i++) if (k[Key.Digit1 + i].wasPressedThisFrame) views.SelectTrailerCamera((CameraViews.Shot)i);
                    if (k.tabKey.wasPressedThisFrame) views.CameraAction();
                    if (k.hKey.wasPressedThisFrame) ToggleScreen();
                    if (k.gKey.wasPressedThisFrame) GuidesShown = !GuidesShown;
                    if (k.xKey.wasPressedThisFrame) HalfSpeed = !HalfSpeed;
                    if (k.pKey.wasPressedThisFrame || k.f12Key.wasPressedThisFrame) Screenshot();
                    if (k.f1Key.wasPressedThisFrame) TogglePanel();
                    if (k.zKey.isPressed) slowTarget = .25f;
                    if (flow.Race.FreeRoam)
                    {
                        if (k.tKey.wasPressedThisFrame) CycleTimeOfDay();
                        if (k.commaKey.wasPressedThisFrame) NudgeClock(-1);
                        if (k.periodKey.wasPressedThisFrame) NudgeClock(1);
                        if (k.kKey.wasPressedThisFrame) ToggleClock();
                        if (k.bKey.wasPressedThisFrame) CycleWeather();
                        if (k.oKey.wasPressedThisFrame) CycleMoon();
                        if (k.lKey.wasPressedThisFrame) LightningNow();
                    }
                }
                if (g != null)
                {
                    int shot = (int)views.TrailerCamera;
                    // while the panel has the controller's focus, the D-pad and A work the panel instead
                    if (!PanelFocused && g.dpad.right.wasPressedThisFrame) views.SelectTrailerCamera((CameraViews.Shot)((shot + 1) % 9));
                    if (!PanelFocused && g.dpad.left.wasPressedThisFrame) views.SelectTrailerCamera((CameraViews.Shot)((shot + 8) % 9));
                    if (!PanelFocused && g.buttonSouth.wasPressedThisFrame) views.CameraAction();
                    if (g.rightShoulder.wasPressedThisFrame) HalfSpeed = !HalfSpeed;
                    if (g.leftShoulder.isPressed) slowTarget = .25f;
                    if (g.rightStickButton.wasPressedThisFrame) Screenshot();
                    if (g.leftStickButton.wasPressedThisFrame) ToggleScreen();
                }
                bool free = views.TrailerCamera == CameraViews.Shot.Free;
                float dt = Time.unscaledDeltaTime;
                float fov = ((k?.equalsKey.isPressed == true ? 1 : 0) - (k?.minusKey.isPressed == true ? 1 : 0) + (!PanelFocused && g?.dpad.down.isPressed == true ? 1 : 0) - (!PanelFocused && g?.dpad.up.isPressed == true ? 1 : 0)) * 30 * dt;
                float far = ((k?.pageDownKey.isPressed == true ? 1 : 0) - (k?.pageUpKey.isPressed == true ? 1 : 0)) * .8f * dt;
                float up = ((k?.homeKey.isPressed == true ? 1 : 0) - (k?.endKey.isPressed == true ? 1 : 0)) * 3 * dt;
                if (g != null && !free) { var rs = g.rightStick.ReadValue(); if (rs.magnitude > .2f) { far += rs.x * .8f * dt; up += rs.y * 3 * dt; } }
                if (fov != 0 || far != 0 || up != 0) views.Adjust(fov, far, up);
                views.AutoHold = k?.leftShiftKey.isPressed == true && !free || g?.buttonWest.isPressed == true || TestHold;
                FreeCamera(views, free, k, g);
            }
            else if (views) { views.FreeMove = Vector3.zero; views.FreeLook = Vector2.zero; views.AutoHold = false; }
            if (slowHeldByTest) slowTarget = .25f;
            Slow(slowTarget);
        }
        // Free camera: WASD / left stick move, Q-E / triggers down-up, right mouse button or right stick look, Shift fast,
        // Ctrl precise. The vehicle's own controls are off meanwhile (it rolls to a stop) and come back afterwards.
        void FreeCamera(CameraViews views, bool free, Keyboard k, Gamepad g)
        {
            if (!free) { views.FreeMove = Vector3.zero; views.FreeLook = Vector2.zero; if (freeHeld) { freeHeld = false; ReleaseVehicle(); } return; }
            var input = flow.Race.vehicle.GetComponent<VehicleInput>(); if (input && input.enabled) input.enabled = false; freeHeld = true;
            float x = (k?.dKey.isPressed == true ? 1 : 0) - (k?.aKey.isPressed == true ? 1 : 0), z = (k?.wKey.isPressed == true ? 1 : 0) - (k?.sKey.isPressed == true ? 1 : 0);
            float y = (k?.eKey.isPressed == true ? 1 : 0) - (k?.qKey.isPressed == true ? 1 : 0);
            var stick = g?.leftStick.ReadValue() ?? Vector2.zero; x += stick.x; z += stick.y; y += (g?.rightTrigger.ReadValue() ?? 0) - (g?.leftTrigger.ReadValue() ?? 0);
            Vector2 look = (g?.rightStick.ReadValue() ?? Vector2.zero) * (100 * Time.unscaledDeltaTime);
            var m = Mouse.current; if (m?.rightButton.isPressed == true) look += m.delta.ReadValue() * .13f;
            x += TestMove.x; y += TestMove.y; z += TestMove.z; look += TestLook; TestLook = Vector2.zero;
            views.FreeMove = new Vector3(x, y, z); views.FreeLook = look;
            views.FreeSpeed = TestSpeed > 0 ? TestSpeed : k?.leftShiftKey.isPressed == true ? 45 : k?.leftCtrlKey.isPressed == true ? 3 : 14;
        }
        void ReleaseVehicle()
        {
            var input = flow && flow.Race ? flow.Race.vehicle.GetComponent<VehicleInput>() : null;
            if (input) input.enabled = flow.State == RaceFlow.Stage.Racing && !flow.Race.Progress.Finished;
        }
        // Slow motion: 0.25x while held, 0.5x toggled; eased in and out (about 0.3 s). The physics step shrinks with it so the
        // motion stays smooth; the game's sound is turned down rather than slowed.
        void Slow(float target)
        {
            Speed = Mathf.MoveTowards(Speed, target, Time.unscaledDeltaTime * 2.5f);
            if (!Driving || DeveloperLocationHud.OwnsInput) return;
            if (Speed < .999f)
            {
                slowed = true; Time.timeScale = Speed; Time.fixedDeltaTime = baseFixed * Speed;
                AudioListener.volume = flow.Save.Settings.master * Mathf.Lerp(.3f, 1, (Speed - .25f) / .75f);
            }
            else if (slowed)
            {
                slowed = false; Speed = 1; Time.timeScale = 1; Time.fixedDeltaTime = baseFixed; AudioListener.volume = flow.Save.Settings.master;
            }
        }
        // Evidence checks only (Report077Checks): input as if from the controls.
        public void HoldSlowForTest(bool value) => slowHeldByTest = value;
        [NonSerialized] public bool TestHold; [NonSerialized] public Vector3 TestMove; [NonSerialized] public Vector2 TestLook; [NonSerialized] public float TestSpeed;
        public void ToggleHud() => HudShown = !HudShown;
        public void ToggleGuides() => GuidesShown = !GuidesShown;
        public void ToggleHalfSpeed() => HalfSpeed = !HalfSpeed;

        void LateUpdate()
        {
            if (!flow || flow.Save == null) return;
            // Clean screen: the HUD canvas (HUD, speedometer, minimap, lap box, Free Roam text, waypoint line, notifications,
            // radio song) and the debug panel are not drawn; menus, the map and the debug menu still show when opened.
            bool clean = Active && !HudShown && !flow.MenuVisible;
            if (clean || cleaned)
            {
                if (hud) hud.enabled = !clean;
                var debug = DebugCanvas; if (debug) debug.enabled = !clean || DeveloperLocationHud.OwnsInput;
                cleaned = clean;
            }
            ShowGuides(!Active || GuidesShown);
            if (screenshotRequested) { screenshotRequested = false; Capture(); }
            UpdatePanel(); menuLastFrame = flow.MenuVisible || flow.GetComponent<ExplorationMap>()?.OwnsInput == true;
        }
        // G: course arrows, checkpoint gates and the waypoint beacon. Hidden with forceRenderingOff, so the systems that show
        // and hide them (Free Roam, races, the waypoint) are not disturbed.
        WaypointGuide waypoint; float nextWaypointLook;
        void ShowGuides(bool show)
        {
            // The waypoint beacon is created when the first waypoint is set, possibly after the guides were hidden.
            if (!show && !waypoint && Time.unscaledTime >= nextWaypointLook)
            {
                nextWaypointLook = Time.unscaledTime + 1; waypoint = FindAnyObjectByType<WaypointGuide>();
                if (waypoint && guidesHidden) foreach (var l in waypoint.GetComponentsInChildren<LineRenderer>(true)) { guides.Add(l); l.forceRenderingOff = true; }
            }
            if (show == !guidesHidden) return;
            guidesHidden = !show;
            if (guidesHidden && guides.Count == 0)
            {
                foreach (var r in FindObjectsByType<MeshRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None)) if (Guide(r)) guides.Add(r);
                if (flow.Race.gates != null) foreach (var gate in flow.Race.gates) if (gate) guides.AddRange(gate.GetComponentsInChildren<Renderer>(true));
                waypoint = FindAnyObjectByType<WaypointGuide>(); if (waypoint) guides.AddRange(waypoint.GetComponentsInChildren<LineRenderer>(true));
            }
            foreach (var r in guides) if (r) r.forceRenderingOff = guidesHidden;
        }

        // Course arrows and chevrons on the ground (not the road signs).
        public static bool Guide(Renderer r) => (r.name.IndexOf("arrow", StringComparison.OrdinalIgnoreCase) >= 0 || r.name.IndexOf("chevron", StringComparison.OrdinalIgnoreCase) >= 0) && !r.GetComponentInParent<PhysicalSign>();

        // ---------- screenshots ----------
        public void Screenshot() => screenshotRequested = true;
        // The camera's own view at the window's full resolution, without any UI, as PNG in Screenshots beside DebugReports.
        void Capture()
        {
            var cam = Camera.main; if (!cam) return;
            int w = Mathf.Max(64, Screen.width), h = Mathf.Max(64, Screen.height);
            var rt = new RenderTexture(w, h, 24); var oldTarget = cam.targetTexture; var active = RenderTexture.active; Texture2D image = null;
            try
            {
                cam.targetTexture = rt; cam.Render(); RenderTexture.active = rt;
                image = new Texture2D(w, h, TextureFormat.RGB24, false); image.ReadPixels(new Rect(0, 0, w, h), 0, 0); image.Apply();
                var raw = image.GetRawTextureData<byte>().ToArray(); var format = image.graphicsFormat;
                Directory.CreateDirectory(ScreenshotFolder);
                string path = Path.Combine(ScreenshotFolder, "WoodstockRush_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-fff") + ".png");
                LastScreenshot = path; screenshotsTaken++;
                Task.Run(() => File.WriteAllBytes(path, ImageConversion.EncodeArrayToPNG(raw, format, (uint)w, (uint)h)));
            }
            catch (Exception e) { Debug.LogWarning("Screenshot failed: " + e.Message); flow.Notify("Screenshot failed: " + e.Message, 5); }
            finally { cam.targetTexture = oldTarget; RenderTexture.active = active; rt.Release(); Destroy(rt); if (image) Destroy(image); }
        }
        public void OpenScreenshotFolder()
        {
            try { Directory.CreateDirectory(ScreenshotFolder); System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(ScreenshotFolder) { UseShellExecute = true }); }
            catch (Exception e) { flow.Notify("Could not open the folder: " + e.Message, 5); }
        }

        // ---------- Free Roam conditions on demand ----------
        public bool ConditionsAvailable => Active && flow.Race.FreeRoam && WorldLook.Current && WorldLook.Current.Mode == "Free Roam";
        WorldLook.TrailerConditions Conditions()
        {
            var look = WorldLook.Current; if (!ConditionsAvailable) return null;
            return look.Trailer ??= new WorldLook.TrailerConditions { hour = look.Hour, weather = look.RoamWeather, phase = look.MoonPhase };
        }
        static readonly (string name, float hour)[] Times = { ("Dawn", 6.6f), ("Day", 12), ("Dusk", 19.2f), ("Night", 23.5f) };
        public static readonly string[] MoonNames = { "New", "Waxing crescent", "First quarter", "Waxing gibbous", "Full", "Waning gibbous", "Last quarter", "Waning crescent" };
        public void CycleTimeOfDay()
        {
            var c = Conditions(); if (c == null) return;
            int i = Array.FindIndex(Times, t => t.hour > c.hour + .01f); c.hour = Times[i < 0 ? 0 : i].hour;
        }
        public void NudgeClock(int hours) { var c = Conditions(); if (c != null) c.hour = Mathf.Repeat(Mathf.Round(c.hour) + hours, 24); }
        public void ToggleClock() { var c = Conditions(); if (c != null) c.paused = !c.paused; }
        public void CycleWeather() { var c = Conditions(); if (c != null) c.weather = (Weather)(((int)c.weather + 1) % 3); }
        public void CycleMoon() { var c = Conditions(); if (c != null) c.phase = Mathf.Repeat(Mathf.Round(c.phase * 8) + 1, 8) / 8f; }
        // A strike now; lightning needs a storm, so this turns the weather to Rain first if it is not raining.
        public void LightningNow()
        {
            var c = Conditions(); if (c == null) return;
            c.weather = Weather.Rain; WorldLook.Current.GetComponent<WeatherEffects>()?.StrikeNow();
        }
        public string ConditionsLabel
        {
            get
            {
                var look = WorldLook.Current; if (!look) return "";
                var c = look.Trailer; float hour = c?.hour ?? look.Hour; float phase = c?.phase ?? look.MoonPhase;
                return $"{Mathf.FloorToInt(hour) % 24:00}:{Mathf.FloorToInt(hour * 60) % 60:00} {(c != null && c.paused ? "(clock paused)" : "")} · {(c?.weather ?? look.RoamWeather)} · Moon {MoonNames[Mathf.RoundToInt(phase * 8) % 8].ToLowerInvariant()}";
            }
        }
        public string TimeLabel { get { var c = WorldLook.Current?.Trailer; float h = c?.hour ?? WorldLook.Current?.Hour ?? 12; return h < 4.6f || h >= 20.8f ? "Night" : h < 7.6f ? "Dawn" : h < 17.8f ? "Day" : "Dusk"; } }
        public bool ClockPaused => WorldLook.Current?.Trailer?.paused == true;
        public string WeatherLabel => (WorldLook.Current?.Trailer?.weather ?? WorldLook.Current?.RoamWeather ?? Weather.Clear).ToString();
        public string MoonLabel { get { var look = WorldLook.Current; float p = look ? (look.Trailer?.phase ?? look.MoonPhase) : .5f; return MoonNames[Mathf.RoundToInt(p * 8) % 8]; } }

        // ---------- help text (pause menu > Trailer / Photo Mode > Controls) ----------
        public static string HelpText(bool controller, bool freeRoam)
        {
            string conditions = freeRoam ? "\nFree Roam: T time of day · , / . clock −1 / +1 h · K pause clock · B weather · O moon · L lightning" : "";
            return controller
                ? "TRAILER / PHOTO MODE   (off: pause menu > Trailer / Photo Mode, or F8)\n"
                  + "D-pad ← →  camera (Chase, Orbit, Side, Front, Fixed, Flyover, Free, Auto, First person)\n"
                  + "A  camera action: Fixed re-plant · Side other side · Auto next shot\nX (hold)  Auto: keep this shot\n"
                  + "LB (hold)  slow motion 0.25×   RB  0.5× on / off\nD-pad ↑ ↓  field of view   Right stick  distance / height\n"
                  + "R3  screenshot   L3  HUD and controls panel on / off   B  use the panel\nFree camera: left stick move · right stick look · RT / LT up / down\n"
                  + "Time of day, weather, moon, arrows and screenshots folder: pause menu > Trailer Mode"
                : "TRAILER / PHOTO MODE   (F8 on / off)    F1 controls panel\n"
                  + "1 Chase  2 Orbit  3 Side  4 Front  5 Fixed  6 Flyover  7 Free  8 Auto  9 First person\n"
                  + "Tab / same number again: Fixed re-plant · Side other side · Auto next shot\nLeft Shift (hold)  Auto: keep this shot\n"
                  + "Z (hold)  slow motion 0.25×   X  0.5× on / off\n− / =  field of view   PgUp / PgDn  distance   Home / End  height\n"
                  + "H  HUD and controls panel on / off   G  arrows, gates, waypoint beacon   P / F12  screenshot   F  shake a fist\n"
                  + "Free camera: WASD move · Q / E down / up · right mouse look · Shift fast · Ctrl precise" + conditions;
        }
    }
}
