using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Racer
{
    // 1.1 (ported from Chastain Rush's Round 57 F ReplayViewer): plays a ReplayData back. The live racers and traffic are frozen and hidden while it
    // plays (and put back after); each racer and traffic car is a puppet, a copy of the player's vehicle made as RaceDirector.CreateCars makes the
    // rivals and traffic, driven by the recording: kinematic, no contacts, its wheels, steering, lean and rider from the recorded speed and steering
    // (ArcadeVehicle.Puppet), its engine through OtherEngines; a traffic car keeps the body and paint it had (AmbientVehicle.Appear). The world's
    // events play at their moments (BreakableProp.ReplayBreak, the riders' waves and celebrations); going back in time puts the world as it was then
    // (every prop restored, the events up to then applied quietly). The race's own time of day and weather (WorldLook); the radio plays live.
    // Cameras: TV (roadside cameras along the focused racer's way, cut as it passes; never inside rock or terrain: each is placed only where a clear
    // line joins it to the racer's own path, and where none sees the racer a chase view stands in), Chase (ChaseCamera on a racer), Free (flies
    // anywhere, eased like Trailer Mode's free camera, and stops at walls and ground).
    // Controls (pad / keyboard): A / Space play-pause; RT / Right hold fast forward; LT / Left hold rewind; X / S slow motion; Y / C camera;
    // LB RB / Q E racer (in the free camera: down, up); D-pad left right / , . previous / next lap; D-pad up / Home restart; D-pad down / H HUD;
    // View / K save; B / Esc / Start leave. The free camera: left stick / WASD move, right stick / mouse (right button) look, Shift fast.
    [DefaultExecutionOrder(100)]   // after CameraViews (50): the camera is placed here last
    public sealed class ReplayViewer : MonoBehaviour
    {
        public static ReplayViewer Current { get; private set; }
        public static bool Active => Current != null;
        public static readonly List<ArcadeVehicle> EngineCars = new List<ArcadeVehicle>(), TrafficCars = new List<ArcadeVehicle>();
        public static float ThrottleOf(ArcadeVehicle c) { var v = Current; if (!v) return 0f; int i = v.puppets.IndexOf(c); return i >= 0 ? v.pedals[i] : 0f; }
        // a replay picked on the REPLAYS page for another course: watched once that course has loaded (RaceFlow.StartWhenBuilt)
        public static string PendingFile;

        public enum Cam { TV, Chase, Free }
        public ReplayData Data { get; private set; }
        public float T { get; private set; }            // the replay's clock (race clock seconds)
        public float Rate { get; private set; } = 1f;   // 1, 0.5, 0.25 (slow motion)
        public bool Paused { get; private set; }
        public Cam View { get; private set; } = Cam.TV;
        public int Focus { get; private set; }
        public int TvIndex { get; private set; } = -1;
        public int TvCount => tv.Count;
        public Vector3 TvSpot(int i) => tv[i].at;
        public bool TvOnPath(int i) => tv[i].onPath;
        public ArcadeVehicle PuppetOf(int racer) => racer >= 0 && racer < puppets.Count ? puppets[racer] : null;
        public ArcadeVehicle FocusCar => PuppetOf(Focus);
        public string Message = ""; float messageUntil;
        public bool HudOn => hud;
        Action onClose;
        bool hud, holdFf, holdRw;
        float shownUntil;
        RaceFlow flow; RaceDirector race;
        readonly List<ArcadeVehicle> puppets = new List<ArcadeVehicle>(), trafficPuppets = new List<ArcadeVehicle>();
        readonly List<float> pedals = new List<float>();
        int nextEvent; bool bumpsBuilt;
        // what is put back on leaving
        float timeScale; bool chaseWas, viewsWas; Transform chaseTarget; Vector3 chaseOffset, chaseScale; float fov; Vector3 camPos; Quaternion camRot;
        sealed class Held { public ArcadeVehicle car; public bool kinematic, detect, enabled; public Vector3 pos, vel, ang; public Quaternion rot; public List<Renderer> shown = new List<Renderer>(); public List<Behaviour> off = new List<Behaviour>(); }
        readonly List<Held> held = new List<Held>();
        readonly List<AudioSource> muted = new List<AudioSource>();
        InputAction aPlay, aSlow, aCam, aPrev, aNext, aLapPrev, aLapNext, aRestart, aHud, aSave, aLeave, aFf, aRw, aMove, aLook, aUp, aDown;
        // TV cameras
        struct Tv { public Vector3 at; public float s; public bool onPath; }
        readonly List<Tv> tv = new List<Tv>();
        float tvSince; Vector3 freePos, freeVelocity; float freeYaw, freePitch;

        // ---------------------------------------------------------------- open / close
        public static ReplayViewer Open(ReplayData d, Action closed = null)
        {
            if (d == null || d.racers.Count == 0 || Current) return null;
            var flow = FindAnyObjectByType<RaceFlow>(); if (!flow || !flow.Race || !flow.Race.vehicle) return null;
            var go = new GameObject("Replay viewer"); UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(go, flow.gameObject.scene);
            var v = go.AddComponent<ReplayViewer>(); v.flow = flow; v.race = flow.Race;
            v.Data = d; v.onClose = closed; v.Begin();
            return v;
        }
        public void Close() { if (Current == this) Destroy(gameObject); }

        void Begin()
        {
            Current = this;
            timeScale = Time.timeScale;
            var cam = Camera.main;
            if (cam)
            {
                var chase = cam.GetComponent<ChaseCamera>(); var views = cam.GetComponent<CameraViews>();
                chaseWas = chase && chase.enabled; viewsWas = views && views.enabled; chaseTarget = chase ? chase.target : null; chaseOffset = chase ? chase.offset : Vector3.zero; chaseScale = chase ? chase.offsetScale : Vector3.one;
                if (views) { views.ShowHead(); views.enabled = false; }
                fov = cam.fieldOfView; camPos = cam.transform.position; camRot = cam.transform.rotation;
                freePos = camPos; var e = camRot.eulerAngles; freeYaw = e.y; freePitch = Mathf.DeltaAngle(0f, e.x);
            }
            SplitScreen.Race?.ReplayView(true);   // a split-screen race plays full screen
            // the puppets: copies of the player's vehicle, made before the live vehicles are hidden
            var source = race.vehicle;
            for (int i = 0; i < Data.racers.Count; i++)
            {
                var r = Data.racers[i]; RiderLook look = null;
                if (!string.IsNullOrEmpty(r.rider)) { try { look = JsonUtility.FromJson<RiderLook>(r.rider); } catch (Exception) { } }
                var p = MakePuppet(source, "Replay " + r.name, VehicleProfile.IndexOf(r.vehicle) >= 0 ? r.vehicle : "original", look, false);
                var cfg = p ? p.GetComponent<VehicleConfiguration>() : null;
                if (cfg) { if (r.scheme >= 0) cfg.SetBodyColor(r.scheme); else if (r.hasPaint) cfg.SetPaint(r.paint); }
                puppets.Add(p); pedals.Add(0f);
            }
            foreach (var tr in Data.traffic) { var p = tr.frames.Count > 0 ? MakePuppet(source, "Replay traffic", "original", null, true) : null; trafficPuppets.Add(p); }
            // freeze and hide every live vehicle: the racers (player included), player 2 and the traffic
            var live = new List<ArcadeVehicle> { race.vehicle };
            foreach (var r in race.Racers) if (r != null && r.Car && !live.Contains(r.Car)) live.Add(r.Car);
            foreach (var d in race.Drivers) if (d && d.Car && !live.Contains(d.Car)) live.Add(d.Car);
            var p2 = SplitScreen.Race ? SplitScreen.Race.P2Car : null; if (p2 && !live.Contains(p2)) live.Add(p2);
            foreach (var c in live) Freeze(c);
            // a Stunt Race's bumps, when it is watched from the REPLAYS page (a race scene builds them only for its own Stunt Races)
            if (Data.stunt && !race.stunts) { race.stunts = true; StuntBumps.Apply(race); race.stunts = false; bumpsBuilt = true; }
            EngineCars.Clear(); EngineCars.AddRange(puppets.Where(p => p)); TrafficCars.Clear(); TrafficCars.AddRange(trafficPuppets.Where(p => p));
            Focus = Mathf.Clamp(Data.focus, 0, Data.racers.Count - 1);
            BuildTv();
            MakeInput();
            T = Data.t0; Seek(Data.t0);
            SetView(Cam.TV);
            shownUntil = Time.unscaledTime + 6f;
            if (!string.IsNullOrEmpty(Data.courseId) && race.courseId != Data.courseId) Say("THE COURSE HAS CHANGED SINCE: THE WORLD MAY DIFFER");
            Debug.Log($"[Replay] watching {Data.Title} ({Data.kind}), {Data.racers.Count} racers, {Data.traffic.Count} traffic, {Data.events.Count} events, {Data.t0:0.0} to {Data.t1:0.0} s, {tv.Count} TV cameras");
        }

        void Freeze(ArcadeVehicle c)
        {
            var h = new Held { car = c, kinematic = c.Body.isKinematic, detect = c.Body.detectCollisions, enabled = c.enabled, pos = c.Body.position, rot = c.Body.rotation };
            if (!c.Body.isKinematic) { h.vel = c.Body.linearVelocity; h.ang = c.Body.angularVelocity; }
            foreach (var b in c.GetComponents<Behaviour>()) if (b.enabled && (b is VehicleInput || b is RoadDriver || b is VehicleRespawn || b is RivalTricks)) { b.enabled = false; h.off.Add(b); }
            c.enabled = false; if (!c.Body.isKinematic) { c.Body.linearVelocity = c.Body.angularVelocity = Vector3.zero; } c.Body.isKinematic = true; c.Body.detectCollisions = false;
            foreach (var r in c.GetComponentsInChildren<Renderer>()) if (r.enabled) { r.enabled = false; h.shown.Add(r); }
            foreach (var l in c.GetComponentsInChildren<Light>()) if (l.enabled) { l.enabled = false; h.off.Add(l); }
            foreach (var s in c.GetComponentsInChildren<AudioSource>()) if (!s.mute) { s.mute = true; muted.Add(s); }
            held.Add(h);
        }

        // a copy of the player's vehicle without its driving, recovery, sound or contacts (as RaceDirector.CreateCars strips the rivals' copies),
        // made inside an inactive holder so nothing of the copy runs before it is stripped
        ArcadeVehicle MakePuppet(ArcadeVehicle source, string name, string vehicle, RiderLook look, bool traffic)
        {
            var holder = new GameObject("Replay puppet (building)"); holder.SetActive(false);
            var clone = Instantiate(source.gameObject, holder.transform); clone.name = name;
            foreach (var mb in clone.GetComponents<MonoBehaviour>())
            {
                if (mb is ArcadeVehicle || mb is VehicleConfiguration || mb is RiderGestures || mb is VehicleLights) continue;
                if (mb is VehicleInput || mb is WaterFeedback) { mb.enabled = false; continue; }   // VehicleInput is required by ArcadeVehicle
                DestroyImmediate(mb);
            }
            foreach (var src in clone.GetComponentsInChildren<AudioSource>(true)) DestroyImmediate(src);
            foreach (Transform child in clone.transform.Cast<Transform>().ToArray()) if (child.name == "Headlight") DestroyImmediate(child.gameObject);   // VehicleLights makes its own
            var car = clone.GetComponent<ArcadeVehicle>(); car.enabled = false; car.Puppet = true;
            var body = clone.GetComponent<Rigidbody>(); body.isKinematic = true; body.detectCollisions = false; body.interpolation = RigidbodyInterpolation.None;
            clone.transform.SetParent(null, false); UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(clone, flow.gameObject.scene); Destroy(holder);
            var cfg = clone.GetComponent<VehicleConfiguration>();
            cfg.riderLook = look; cfg.classicVisual = traffic; cfg.Apply(vehicle);
            if (traffic) clone.AddComponent<AmbientVehicle>().Initialize();
            foreach (var col in clone.GetComponentsInChildren<Collider>(true)) col.enabled = false;
            return car;
        }

        void OnDestroy()
        {
            if (Current != this) return;
            Current = null; EngineCars.Clear(); TrafficCars.Clear();
            foreach (var p in puppets.Concat(trafficPuppets)) if (p) { p.gameObject.SetActive(false); Destroy(p.gameObject); }
            if (bumpsBuilt && race) StuntBumps.Apply(race);   // removed again (this race is not a Stunt Race)
            BreakableProp.RestoreRace();
            foreach (var h in held)
            {
                var c = h.car; if (!c) continue;
                c.Body.isKinematic = h.kinematic; c.Body.detectCollisions = h.detect;
                c.Body.position = h.pos; c.Body.rotation = h.rot; c.transform.SetPositionAndRotation(h.pos, h.rot);
                if (!h.kinematic) { c.Body.linearVelocity = h.vel; c.Body.angularVelocity = h.ang; }
                c.enabled = h.enabled; foreach (var b in h.off) if (b) b.enabled = true; foreach (var r in h.shown) if (r) r.enabled = true;
            }
            Physics.SyncTransforms();
            foreach (var s in muted) if (s) s.mute = false;
            var cam = Camera.main;
            if (cam)
            {
                cam.fieldOfView = fov; cam.transform.SetPositionAndRotation(camPos, camRot);
                var chase = cam.GetComponent<ChaseCamera>(); var views = cam.GetComponent<CameraViews>();
                if (chase) { chase.enabled = false; chase.target = chaseTarget; chase.offset = chaseOffset; chase.offsetScale = chaseScale; chase.enabled = chaseWas; if (chaseWas && chaseTarget) chase.Snap(); }
                if (views) views.enabled = viewsWas;
            }
            SplitScreen.Race?.ReplayView(false);
            Time.timeScale = timeScale;
            foreach (var a in Actions()) a?.Dispose();
            MenuInput.ConsumeThroughRelease();
            onClose?.Invoke();
        }

        // ---------------------------------------------------------------- input
        IEnumerable<InputAction> Actions() => new[] { aPlay, aSlow, aCam, aPrev, aNext, aLapPrev, aLapNext, aRestart, aHud, aSave, aLeave, aFf, aRw, aMove, aLook, aUp, aDown };
        static InputAction B(string name, params string[] paths) { var a = new InputAction(name, InputActionType.Button); foreach (var p in paths) a.AddBinding(p); a.Enable(); return a; }
        void MakeInput()
        {
            aPlay = B("Replay play", "<Gamepad>/buttonSouth", "<Keyboard>/space");
            aSlow = B("Replay slow", "<Gamepad>/buttonWest", "<Keyboard>/s");
            aCam = B("Replay camera", "<Gamepad>/buttonNorth", "<Keyboard>/c");
            aPrev = B("Replay racer back", "<Gamepad>/leftShoulder", "<Keyboard>/q");
            aNext = B("Replay racer on", "<Gamepad>/rightShoulder", "<Keyboard>/e");
            aLapPrev = B("Replay lap back", "<Gamepad>/dpad/left", "<Keyboard>/comma");
            aLapNext = B("Replay lap on", "<Gamepad>/dpad/right", "<Keyboard>/period");
            aRestart = B("Replay restart", "<Gamepad>/dpad/up", "<Keyboard>/home");
            aHud = B("Replay HUD", "<Gamepad>/dpad/down", "<Keyboard>/h");
            aSave = B("Replay save", "<Gamepad>/select", "<Keyboard>/k");
            aLeave = B("Replay leave", "<Gamepad>/buttonEast", "<Gamepad>/start", "<Keyboard>/escape", "<Keyboard>/backspace");
            aFf = new InputAction("Replay fast", InputActionType.Value); aFf.AddBinding("<Gamepad>/rightTrigger"); aFf.AddBinding("<Keyboard>/rightArrow"); aFf.Enable();
            aRw = new InputAction("Replay rewind", InputActionType.Value); aRw.AddBinding("<Gamepad>/leftTrigger"); aRw.AddBinding("<Keyboard>/leftArrow"); aRw.Enable();
            aUp = B("Replay up", "<Gamepad>/rightShoulder", "<Keyboard>/e"); aDown = B("Replay down", "<Gamepad>/leftShoulder", "<Keyboard>/q");
            aMove = new InputAction("Replay move", InputActionType.Value, expectedControlType: "Vector2"); aMove.AddBinding("<Gamepad>/leftStick");
            aMove.AddCompositeBinding("2DVector").With("Up", "<Keyboard>/w").With("Down", "<Keyboard>/s").With("Left", "<Keyboard>/a").With("Right", "<Keyboard>/d"); aMove.Enable();
            aLook = new InputAction("Replay look", InputActionType.Value, expectedControlType: "Vector2"); aLook.AddBinding("<Gamepad>/rightStick"); aLook.Enable();
            MenuInput.ConsumeThroughRelease();
        }
        // the buttons' actions (checks call them too)
        public void TogglePause() { if (Paused && T >= Data.t1 - 0.05f) { Restart(); return; } Paused = !Paused; Say(Paused ? "PAUSED" : "PLAY"); }
        public void CycleSlow() { Rate = Rate >= 1f ? 0.5f : Rate >= 0.5f ? 0.25f : 1f; Say(Rate >= 1f ? "NORMAL SPEED" : $"SLOW MOTION x{Rate:0.##}"); }
        public void Restart() { Seek(Data.t0); Paused = false; Say("RESTART"); }
        public void SetView(Cam c)
        {
            View = c; var cam = Camera.main; if (!cam) return; var chase = cam.GetComponent<ChaseCamera>();
            if (c == Cam.Chase && chase && FocusCar)
            {
                chase.enabled = false; chase.target = FocusCar.transform; chase.offset = FocusCar.GetComponent<VehicleConfiguration>().Profile.Camera; chase.offsetScale = Vector3.one;
                chase.enabled = true; chase.Snap(); cam.fieldOfView = fov;
            }
            else if (chase) chase.enabled = false;
            if (c == Cam.Free) { freePos = cam.transform.position; var e = cam.transform.rotation.eulerAngles; freeYaw = e.y; freePitch = Mathf.DeltaAngle(0f, e.x); freeVelocity = Vector3.zero; cam.fieldOfView = fov; }
            if (c == Cam.TV) { TvIndex = -1; tvSince = -99f; }
            Say(c == Cam.TV ? "TV CAMERAS" : c == Cam.Chase ? "CHASE CAMERA: " + Data.racers[Focus].name : "FREE CAMERA");
        }
        public void NextView() => SetView((Cam)(((int)View + 1) % 3));
        public void FocusOn(int i)
        {
            Focus = (i % Data.racers.Count + Data.racers.Count) % Data.racers.Count;
            if (View == Cam.Chase) SetView(Cam.Chase); else { TvIndex = -1; Say(Data.racers[Focus].name); }
        }
        public void JumpLap(int d)
        {
            var laps = Data.racers[Focus].laps;
            var starts = new List<float> { Data.t0 }; starts.AddRange(laps.Select(l => l.end).Where(x => x > Data.t0 && x < Data.t1));
            int cur = starts.FindLastIndex(s => s <= T + 0.5f); int to = Mathf.Clamp(cur + d, 0, starts.Count - 1);
            if (d < 0 && T - starts[Mathf.Max(0, cur)] > 2f) to = Mathf.Max(0, cur);   // first back to this lap's start
            Seek(starts[to]); Say(to == 0 ? "THE START" : $"LAP {to + 1}");
        }
        public void SaveIt() => Say(ReplayData.Save(Data));
        public void ToggleHud() { hud = !hud; }
        public void Seek(float t)
        {
            // back in time (or a jump): the world as it was at t
            T = Mathf.Clamp(t, Data.t0, Data.t1);
            BreakableProp.ReplayRestoreAll();
            nextEvent = 0; PlayEvents(false);
            ApplyPoses();
        }
        void Say(string s) { Message = s; messageUntil = Time.unscaledTime + 2.2f; shownUntil = Time.unscaledTime + 4f; }

        void Update()
        {
            float dt = Time.unscaledDeltaTime;
            if (!MenuInput.Blocked)
            {
                bool any = false;
                if (aLeave.WasPressedThisFrame()) { Close(); return; }
                if (aPlay.WasPressedThisFrame()) { TogglePause(); any = true; }
                if (aSlow.WasPressedThisFrame() && !(View == Cam.Free && aSlow.activeControl?.device is Keyboard)) { CycleSlow(); any = true; }
                if (aCam.WasPressedThisFrame()) { NextView(); any = true; }
                if (View != Cam.Free) { if (aPrev.WasPressedThisFrame()) { FocusOn(Focus - 1); any = true; } if (aNext.WasPressedThisFrame()) { FocusOn(Focus + 1); any = true; } }
                if (aLapPrev.WasPressedThisFrame()) { JumpLap(-1); any = true; }
                if (aLapNext.WasPressedThisFrame()) { JumpLap(1); any = true; }
                if (aRestart.WasPressedThisFrame()) { Restart(); any = true; }
                if (aHud.WasPressedThisFrame()) { ToggleHud(); any = true; }
                if (aSave.WasPressedThisFrame()) { SaveIt(); any = true; }
                holdFf = aFf.ReadValue<float>() > 0.3f; holdRw = aRw.ReadValue<float>() > 0.3f;
                if (holdFf || holdRw || any) shownUntil = Time.unscaledTime + 4f;
            }
            else holdFf = holdRw = false;
            // the clock: fast forward x4, rewind x4, else the chosen rate; the world's own motion (debris, people) at that rate too
            float rate = holdRw ? -4f : holdFf ? 4f : Paused ? 0f : Rate;
            Time.timeScale = holdRw ? 1f : Mathf.Max(0f, rate);
            float t = Mathf.Clamp(T + rate * dt, Data.t0, Data.t1);
            if (t < T - 1e-4f) Seek(t); else { T = t; PlayEvents(true); }
            if (T >= Data.t1 && !Paused && rate > 0f) { Paused = true; Say("THE END: A plays again, B leaves"); }
            ApplyPoses();
        }
        void LateUpdate() { UpdateCamera(Time.unscaledDeltaTime); }

        // ---------------------------------------------------------------- the world
        void PlayEvents(bool live)
        {
            var ev = Data.events;
            while (nextEvent < ev.Count && ev[nextEvent].t <= T) { Apply(ev[nextEvent], live && T - ev[nextEvent].t < 1f); nextEvent++; }
        }
        void Apply(ReplayData.Event e, bool show)
        {
            switch (e.kind)
            {
                case ReplayData.Kind.Prop: BreakableProp.ReplayBreak(e.p, e.v, show); break;
                case ReplayData.Kind.Gesture:
                    if (show) { var p = PuppetOf(e.a); var g = p ? p.GetComponent<RiderGestures>() : null; if (g) { if (e.b == (int)RiderGestures.Kind.Celebrate) g.Celebrate(); else g.Wave(); } }
                    break;
            }
        }
        void ApplyPoses()
        {
            for (int i = 0; i < puppets.Count; i++)
            {
                var c = puppets[i]; if (!c) continue; var tr = Data.racers[i].track; if (tr.frames.Count == 0) continue;
                var f = ReplayData.At(tr, T);
                c.Body.position = f.p; c.Body.rotation = f.q; c.transform.SetPositionAndRotation(f.p, f.q);
                c.PuppetSpeed = f.speed; c.PuppetSteer = f.steer; pedals[i] = f.throttle;
            }
            for (int i = 0; i < trafficPuppets.Count; i++)
            {
                var c = trafficPuppets[i]; if (!c) continue; var tr = Data.traffic[i];
                var f = ReplayData.At(tr, T);
                c.Body.position = f.p; c.Body.rotation = f.q; c.transform.SetPositionAndRotation(f.p, f.q); c.PuppetSpeed = f.speed;
                c.GetComponent<AmbientVehicle>()?.Appear(f.look >> 4, f.look & 15);
            }
        }
        // name tags (RaceNames.NameTags, the name-over-head option): every racer, humans in their colour; the chased racer's own is not drawn
        public List<NameTags.Target> Tags()
        {
            var list = new List<NameTags.Target>();
            for (int i = 0; i < puppets.Count; i++) if (puppets[i] && !(View == Cam.Chase && i == Focus)) list.Add(new NameTags.Target { car = puppets[i], name = Data.racers[i].name, human = Data.racers[i].human, player = Data.racers[i].human });
            return list;
        }

        // ---------------------------------------------------------------- cameras
        // TV cameras: along the focused racer's own way (player 1's), every 70 m, 9 m (else 6 m) off to one side (the other if blocked) and 3.5 m above
        // the ground there, where they see the way. A camera is only placed where a clear straight line joins it to the racer's path (which is open
        // air), so it never stands inside rock, terrain or a building; nor on a roof or a hilltop far above, nor down a bank. Where nothing outside
        // can see (a tunnel, a cut, under a bridge) one on the way itself, 14 m back along it.
        void BuildTv()
        {
            tv.Clear();
            var path = Data.racers[Mathf.Clamp(Data.focus, 0, Data.racers.Count - 1)].track.frames.Select(f => f.p).ToList();
            if (path.Count < 2) return;
            var cum = new float[path.Count]; for (int i = 1; i < path.Count; i++) cum[i] = cum[i - 1] + Mathf.Min(30f, (path[i] - path[i - 1]).magnitude);
            int side = 1; float next = 20f;
            for (int i = 1; i < path.Count; i++)
            {
                if (cum[i] < next) continue; next = cum[i] + 70f;
                var p = path[i]; var fwd = path[Mathf.Min(path.Count - 1, i + 3)] - path[Mathf.Max(0, i - 3)]; fwd.y = 0f; if (fwd.sqrMagnitude < 1e-3f) continue; fwd.Normalize();
                var right = Vector3.Cross(Vector3.up, fwd);
                var aim = p + Vector3.up * 0.8f;
                bool placed = false;
                foreach (int sd in new[] { side, -side })
                {
                    foreach (float off in new[] { 9f, 6f })
                    {
                        var q = p + right * (sd * off) + fwd * 10f;
                        if (!Physics.Raycast(q + Vector3.up * 60f, Vector3.down, out var hit, 120f, ~0, QueryTriggerInteraction.Ignore)) continue;
                        if (hit.collider.attachedRigidbody) continue;
                        var at = hit.point + Vector3.up * 3.5f;
                        if (Mathf.Abs(at.y - p.y) > 12f) continue;
                        if (!Clear(aim, at) || Physics.CheckSphere(at, 0.6f, ~0, QueryTriggerInteraction.Ignore)) continue;
                        tv.Add(new Tv { at = at, s = cum[i] }); placed = true; side = -sd; break;
                    }
                    if (placed) break;
                }
                if (placed) continue;
                int j = i; while (j > 0 && cum[i] - cum[j] < 14f) j--;
                var back = path[j] + Vector3.up * 1.6f;
                if (Clear(path[j] + Vector3.up * 0.8f, back) && Clear(back, aim) && !Physics.CheckSphere(back, 0.4f, ~0, QueryTriggerInteraction.Ignore)) tv.Add(new Tv { at = back, s = cum[j], onPath = true });
            }
        }
        // a straight line with nothing solid across it (vehicles and triggers do not count)
        static bool Clear(Vector3 a, Vector3 b)
        {
            var d = b - a; float len = d.magnitude; if (len < 0.05f) return true;
            foreach (var h in Physics.RaycastAll(a, d / len, len, ~0, QueryTriggerInteraction.Ignore)) if (!h.collider.attachedRigidbody) return false;
            return true;
        }
        static bool Sees(Vector3 from, Vector3 to) { var d = to - from; return Clear(from, from + d.normalized * Mathf.Max(0f, d.magnitude - 1f)); }

        void UpdateCamera(float dt)
        {
            var cam = Camera.main; if (!cam) return;
            var target = FocusCar; if (!target) return;
            var tp = target.transform.position + Vector3.up * 0.8f;
            if (View == Cam.Chase) return;   // ChaseCamera follows the puppet
            if (View == Cam.Free)
            {
                // eased like Trailer Mode's free camera; it stops at walls and ground
                var look = aLook.ReadValue<Vector2>(); var mouse = Mouse.current;
                if (mouse != null && mouse.rightButton.isPressed) look += mouse.delta.ReadValue() * 0.06f;
                freeYaw += look.x * 110f * dt; freePitch = Mathf.Clamp(freePitch - look.y * 80f * dt, -85f, 85f);
                var rot = Quaternion.Euler(freePitch, freeYaw, 0f); var mv = aMove.ReadValue<Vector2>();
                float up = (aUp.IsPressed() ? 1f : 0f) - (aDown.IsPressed() ? 1f : 0f);
                var kb = Keyboard.current; float speed = kb != null && kb.leftShiftKey.isPressed ? 40f : 14f;
                var wanted = rot * new Vector3(mv.x, 0f, mv.y) + Vector3.up * up;
                freeVelocity = Vector3.Lerp(freeVelocity, Vector3.ClampMagnitude(wanted, 1f) * speed, 1f - Mathf.Exp(-6f * dt));
                var step = freeVelocity * dt; float m = step.magnitude;
                if (m > 1e-5f && Physics.SphereCast(freePos, .3f, step / m, out var hit, m, ~0, QueryTriggerInteraction.Ignore) && !hit.collider.attachedRigidbody) { step = step / m * Mathf.Max(0f, hit.distance - .05f); freeVelocity = Vector3.zero; }
                freePos += step;
                cam.transform.SetPositionAndRotation(freePos, rot);
                return;
            }
            // TV: keep the camera while it sees the racer within 75 m and the racer has not gone 25 m past it; then the next one ahead that sees it
            float sNow = NearestS(target.transform.position);
            bool keep = TvIndex >= 0 && TvIndex < tv.Count && (tv[TvIndex].at - tp).magnitude < 75f && Sees(tv[TvIndex].at, tp) && sNow - tv[TvIndex].s < 25f;
            if ((!keep && Time.unscaledTime - tvSince > 1.2f) || TvIndex < 0 || TvIndex >= tv.Count || !Sees(tv[TvIndex].at, tp))
            {
                int best = -1; float bestKey = float.MaxValue;
                for (int i = 0; i < tv.Count; i++)
                {
                    float dist = (tv[i].at - tp).magnitude; if (dist > 75f) continue;
                    float ahead = tv[i].s - sNow; float k = ahead >= -5f ? ahead : 200f - ahead;   // the next one ahead first
                    if (k < bestKey && Sees(tv[i].at, tp)) { bestKey = k; best = i; }
                }
                if (best != TvIndex) { TvIndex = best; tvSince = Time.unscaledTime; }
            }
            if (TvIndex < 0)
            {   // no TV camera sees it: a chase view from behind and above, pulled in front of anything between (as the chase camera is)
                var pivot = target.transform.position + Vector3.up;
                var want = target.transform.position - Vector3.ProjectOnPlane(target.transform.forward, Vector3.up).normalized * 8f + Vector3.up * 3.5f;
                var dir = want - pivot; if (Physics.SphereCast(pivot, .3f, dir.normalized, out var h2, dir.magnitude, ~0, QueryTriggerInteraction.Ignore) && !h2.collider.attachedRigidbody) want = pivot + dir.normalized * Mathf.Max(0f, h2.distance - .1f);
                cam.transform.position = Vector3.Lerp(cam.transform.position, want, 1f - Mathf.Exp(-6f * dt)); cam.transform.LookAt(tp); cam.fieldOfView = fov; return;
            }
            var at = tv[TvIndex].at; float distance = (tp - at).magnitude;
            cam.transform.position = at;
            var wantRot = Quaternion.LookRotation(tp - at, Vector3.up);
            cam.transform.rotation = Time.unscaledTime - tvSince < 0.05f ? wantRot : Quaternion.Slerp(cam.transform.rotation, wantRot, 1f - Mathf.Exp(-12f * dt));
            cam.fieldOfView = Mathf.Clamp(2f * Mathf.Atan(7f / Mathf.Max(1f, distance)) * Mathf.Rad2Deg, 14f, 62f);   // the vehicle about a third of the picture
        }
        float NearestS(Vector3 p)
        {
            if (tv.Count == 0) return 0f; var path = Data.racers[Mathf.Clamp(Data.focus, 0, Data.racers.Count - 1)].track.frames;
            int best = 0; float bd = float.MaxValue; int step = Mathf.Max(1, path.Count / 600);
            for (int i = 0; i < path.Count; i += step) { float d = (path[i].p - p).sqrMagnitude; if (d < bd) { bd = d; best = i; } }
            float s = 0f; for (int i = 1; i <= best; i++) s += Mathf.Min(30f, (path[i].p - path[i - 1].p).magnitude);
            return s;
        }

        // ---------------------------------------------------------------- the overlay
        GUIStyle big, small, keys, centre;
        void OnGUI()
        {
            if (Event.current.type != EventType.Repaint) return;
            float sc = Screen.height / 800f; var m = GUI.matrix; GUI.matrix = Matrix4x4.Scale(new Vector3(sc, sc, 1f)); float w = Screen.width / sc, h = 800f;
            big ??= new GUIStyle(GUI.skin.label) { fontSize = 22, fontStyle = FontStyle.Bold, normal = { textColor = Color.white } };
            small ??= new GUIStyle(GUI.skin.label) { fontSize = 15, normal = { textColor = new Color(1f, 1f, 1f, .92f) } };
            keys ??= new GUIStyle(small) { alignment = TextAnchor.MiddleCenter, wordWrap = true };
            centre ??= new GUIStyle(big) { alignment = TextAnchor.MiddleCenter, fontSize = 26 };
            void Box(Rect r) { GUI.color = new Color(0f, 0f, 0f, .55f); GUI.DrawTexture(r, Texture2D.whiteTexture); GUI.color = Color.white; }
            // always: a small REPLAY tag (its state)
            string state = Paused ? "II" : holdRw ? "<<" : holdFf ? ">>" : Rate < 1f ? $"x{Rate:0.##}" : ">";
            Box(new Rect(w - 190, 16, 174, 34)); GUI.Label(new Rect(w - 180, 18, 170, 30), $"REPLAY  {state}", big);
            var r = Data.racers[Focus];
            if (hud)
            {
                int lap = 1 + r.laps.Count(l => l.end <= T); lap = Mathf.Min(lap, Mathf.Max(1, Data.laps));
                float since = T - r.laps.Where(l => l.end <= T).Select(l => l.end).DefaultIfEmpty(r.laps.Count > 0 ? r.laps[0].start : 0f).Max();
                var p = FocusCar; string speed = p ? DisplayUnits.Speed(Mathf.Abs(p.PuppetSpeed)) : "";
                Box(new Rect(16, 16, 420, 118));
                GUI.Label(new Rect(28, 20, 400, 30), Data.Title, big);
                GUI.Label(new Rect(28, 52, 400, 24), $"{r.name}   {VehicleProfile.Find(r.vehicle).Name}   {speed}", small);
                GUI.Label(new Rect(28, 76, 400, 24), T < 0f ? $"Countdown {-T:0.0}" : r.finishedAt >= 0f && T >= r.finishedAt ? "Finished" + (r.total > 0f ? "  " + RaceHud.FormatTime(r.total) : "") : $"Lap {lap} / {Mathf.Max(1, Data.laps)}   {RaceHud.FormatTime(Mathf.Max(0f, since))}", small);
                GUI.Label(new Rect(28, 100, 400, 24), $"Race clock {RaceHud.FormatTime(Mathf.Max(0f, T))}   {(View == Cam.TV ? "TV cameras" : View == Cam.Chase ? "Chase camera" : "Free camera")}", small);
            }
            if (Time.unscaledTime < messageUntil && Message != "") { Box(new Rect(w / 2 - 300, 90, 600, 44)); GUI.Label(new Rect(w / 2 - 300, 92, 600, 40), Message, centre); }
            if (hud || Time.unscaledTime < shownUntil)
            {
                bool pad = MenuInput.Controller;
                string help = pad ? "A play / pause    RT fast forward    LT rewind    X slow motion    Y camera    LB / RB racer    D-pad: ‹ › lap, up restart, down HUD    View save    B leave"
                                  : "Space play / pause    Right fast forward    Left rewind    S slow motion    C camera    Q / E racer    , . lap    Home restart    H HUD    K save    Esc leave";
                if (View == Cam.Free) help += pad ? "    (free camera: sticks move and look, LB / RB down / up)" : "    (free camera: WASD move, right mouse look, Q / E down / up, Shift fast)";
                Box(new Rect(40, h - 70, w - 80, 50)); GUI.Label(new Rect(50, h - 68, w - 100, 46), help, keys);
                // the time line
                float u = Mathf.InverseLerp(Data.t0, Data.t1, T); GUI.color = new Color(1f, 1f, 1f, .35f); GUI.DrawTexture(new Rect(40, h - 80, w - 80, 4), Texture2D.whiteTexture);
                GUI.color = new Color(.3f, .95f, .81f); GUI.DrawTexture(new Rect(40, h - 80, (w - 80) * u, 4), Texture2D.whiteTexture); GUI.color = Color.white;
            }
            GUI.matrix = m;
        }
    }
}
