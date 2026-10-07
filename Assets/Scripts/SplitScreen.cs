using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Racer
{
    // 0.90 Part D: split-screen, stage 1: two players, one race on one PC. The setup screen (RaceMenus.Split.cs) chooses the
    // devices, vehicles, colours, course, laps and layout; the race is the normal race with player 2's vehicle made from the
    // rival slot (driven by player 2's device, or by the normal race AI when "Player 2: AI driver" is on). Everything is
    // unlocked and stock; Day / Clear, no traffic, no AI rivals; nothing is written to the records, ghosts, activities,
    // acorns or the campaign. Single-player is unchanged: every hook checks Active.
    // Built around one camera and left single or off in stage 1: first person and the other views (both halves use the
    // chase camera), Trailer Mode, the winner shot, gestures, hints; the loading screen is unchanged.
    public static class SplitScreen
    {
        public static bool Active { get; private set; }
        public static bool PendingStart;
        public static InputDevice P1Device, P2Device;
        public static bool P2Ai;
        public static string P1Vehicle = "original", P2Vehicle = "atv";
        public static int P1Color = 1, P2Color = 4, Course, Laps = 2;
        public static SplitRace Race { get; private set; }
        public static bool LeftRight => Hints.Flow && Hints.Flow.Save != null && Hints.Flow.Save.Settings.splitLeftRight;
        // Every vehicle except the acorn reward, which joins once earned; every course.
        public static VehicleProfile[] Vehicles => VehicleProfile.All.Where(p => !VehicleUnlocks.Locked(p)).ToArray();
        public static string DeviceName(InputDevice d) => d == null ? "—" : d is Keyboard ? "Keyboard" : d is Gamepad g ? "Controller " + (Gamepad.all.ToList().IndexOf(g) + 1) : d.displayName;
        public static bool Ready => P1Device != null && (P2Ai || P2Device != null);
        // One of the players' own devices (the mouse goes with the keyboard).
        public static bool PlayerDevice(InputDevice d) => d != null && (d == P1Device || (!P2Ai && d == P2Device) || (d is Mouse && (P1Device is Keyboard || (!P2Ai && P2Device is Keyboard))));

        public static void Begin() { Active = true; PendingStart = true; }
        // Called in the course scene, before the race is built.
        public static void Configure(RaceFlow flow)
        {
            var race = flow.Race;
            race.opponents = true; race.opponentRoster = new[] { P2Vehicle }; race.traffic = false; race.difficulty = 1; race.laps = Mathf.Clamp(Laps, 1, 5);
            var c = race.vehicle.GetComponent<VehicleConfiguration>(); c.Apply(P1Vehicle); c.SetBodyColor(P1Color);
            Race = flow.gameObject.AddComponent<SplitRace>(); Race.Initialize(flow);
        }
        public static void End()
        {
            if (Race) { Race.Teardown(); Object.Destroy(Race); }
            Race = null; Active = false; PendingStart = false;
        }
        // The positions of every split-screen view (the scenery's near detail follows each of them); empty when not split.
        public static readonly List<Vector3> Eyes = new();
    }

    public sealed class SplitRace : MonoBehaviour
    {
        RaceFlow flow; RaceDirector race; Camera cam1, cam2; ChaseCamera chase2; Rect rect1; float far1, lodBias;
        public ArcadeVehicle P2Car { get; private set; }
        public RacerState P2 { get; private set; }
        public WrongWayGuidance P2Guidance { get; private set; }
        public int MissingPlayer { get; private set; } // 0 = both devices present
        public InputDevice PausedBy;
        SplitHud hud; bool p2Done; GameObject divider;

        public void Initialize(RaceFlow owner)
        {
            flow = owner; race = flow.Race;
            race.vehicle.GetComponent<VehicleInput>().Bind(SplitScreen.P1Device);
            InputSystem.onDeviceChange += DeviceChanged;
            // per-view detail, lowered for the two views only (restored in Teardown); single-player rendering is unchanged
            lodBias = QualitySettings.lodBias; QualitySettings.lodBias = lodBias * LodScale;
            cam1 = Camera.main; rect1 = cam1.rect; far1 = cam1.farClipPlane;
            hud = gameObject.AddComponent<SplitHud>(); hud.Initialize(flow, this);
        }
        public const float LodScale = .7f, FarClip = 1400;

        // RaceDirector.RestartRace: player 2's vehicle is the one rival slot; make it player 2's (or leave the AI driving).
        public void PlayerTwo()
        {
            if (race.Racers.Count < 2) return;
            var state = race.Racers[1]; var car = state.Car; P2Car = car; car.name = "PLAYER 2";
            var config = car.GetComponent<VehicleConfiguration>(); config.SetBodyColor(SplitScreen.P2Color); // 0.92: the champion's scheme too
            if (!SplitScreen.P2Ai)
            {
                var driver = car.GetComponent<RoadDriver>(); race.Drivers.Remove(driver); if (driver) { driver.enabled = false; Destroy(driver); }
                P2 = new RacerState("PLAYER 2", car, race.gates.Length - 1, race.laps); race.Racers[1] = P2; P2.SampleOrigin(Time.timeAsDouble);
                var input = car.GetComponent<VehicleInput>(); input.Bind(SplitScreen.P2Device); input.enabled = false;
                car.enabled = false; car.Body.isKinematic = true;
                var respawn = car.GetComponent<VehicleRespawn>(); respawn.enabled = true; respawn.SeedCoursePosition(car.Body.position);
            }
            else { P2 = state; car.name = "PLAYER 2 (AI)"; }
            if (!car.GetComponent<VehicleAudio>()) car.gameObject.AddComponent<VehicleAudio>();
            P2Guidance = gameObject.AddComponent<WrongWayGuidance>(); P2Guidance.Target = P2;
            p2Done = false; Cameras();
        }
        // RaceDirector.RestartRace / AbandonEvent: player 2's vehicle goes (a human one is no longer in the AI list).
        public void ClearPlayerTwo()
        {
            if (P2Guidance) { Destroy(P2Guidance); P2Guidance = null; }
            if (P2Car && !SplitScreen.P2Ai) { P2Car.gameObject.SetActive(false); Destroy(P2Car.gameObject); }
            P2Car = null; P2 = null;
        }
        void Cameras()
        {
            bool lr = SplitScreen.LeftRight;
            cam1.rect = lr ? new Rect(0, 0, .5f, 1) : new Rect(0, .5f, 1, .5f);
            if (!cam2)
            {
                var go = new GameObject("Player 2 camera"); cam2 = go.AddComponent<Camera>(); cam2.CopyFrom(cam1);
                var d1 = cam1.GetUniversalAdditionalCameraData(); var d2 = cam2.GetUniversalAdditionalCameraData();
                d2.renderPostProcessing = d1.renderPostProcessing; d2.antialiasing = d1.antialiasing; d2.renderShadows = d1.renderShadows;
                chase2 = go.AddComponent<ChaseCamera>(); var chase1 = cam1.GetComponent<ChaseCamera>(); chase2.obstructionMask = chase1.obstructionMask;
            }
            cam2.rect = lr ? new Rect(.5f, 0, .5f, 1) : new Rect(0, 0, 1, .5f);
            cam1.farClipPlane = cam2.farClipPlane = Mathf.Min(far1, FarClip);
            chase2.enabled = false; chase2.target = P2Car.transform; chase2.offset = P2Car.GetComponent<VehicleConfiguration>().Profile.Camera; chase2.enabled = true; chase2.Snap();
            // the wide top / bottom halves: the camera tips down a little in long flights so the landing stays in view
            float tilt = lr ? 0 : 9; cam1.GetComponent<ChaseCamera>().flightTilt = tilt; chase2.flightTilt = tilt;
            hud.Layout(lr);
        }
        void Update()
        {
            if (!race || P2Car == null) return;
            var stage = flow.State;
            if (!SplitScreen.P2Ai)
            {
                bool driving = stage == RaceFlow.Stage.Racing && !P2.Progress.Finished;
                var input = P2Car.GetComponent<VehicleInput>(); input.enabled = driving;
                P2Car.GetComponent<VehicleRespawn>().enabled = driving;
                bool locked = stage == RaceFlow.Stage.Countdown || stage == RaceFlow.Stage.Ready;
                if (locked && P2Car.enabled) { P2Car.enabled = false; P2Car.Body.linearVelocity = P2Car.Body.angularVelocity = Vector3.zero; P2Car.Body.isKinematic = true; }
                if (stage == RaceFlow.Stage.Racing && !P2Car.enabled && !p2Done) { P2Car.enabled = true; P2Car.Body.isKinematic = false; }
                // finished: hand over to a runoff driver to clear the line, as for player 1
                if (P2.Progress.Finished && !p2Done)
                {
                    p2Done = true; input.enabled = false; P2Car.GetComponent<VehicleRespawn>().enabled = false;
                    var runoff = P2Car.gameObject.AddComponent<RoadDriver>(); runoff.Initialize(race, P2Car, true, 1, 1); runoff.Racer = P2;
                }
            }
            SplitScreen.Eyes.Clear(); if (cam1) SplitScreen.Eyes.Add(cam1.transform.position); if (cam2) SplitScreen.Eyes.Add(cam2.transform.position);
        }
        // ---------- devices ----------
        void DeviceChanged(InputDevice device, InputDeviceChange change)
        {
            if (change == InputDeviceChange.Removed || change == InputDeviceChange.Disconnected)
            {
                int player = device == SplitScreen.P1Device ? 1 : device == SplitScreen.P2Device && !SplitScreen.P2Ai ? 2 : 0; if (player == 0) return;
                MissingPlayer = player;
                if (flow.State == RaceFlow.Stage.Racing || flow.State == RaceFlow.Stage.Countdown) flow.Pause();
                flow.RefreshMenu();
            }
            else if ((change == InputDeviceChange.Added || change == InputDeviceChange.Reconnected) && MissingPlayer != 0 && device is Gamepad pad)
            {
                // the missing player's controller is back (or another controller takes its place)
                if (MissingPlayer == 1 && !(SplitScreen.P1Device is Keyboard)) { SplitScreen.P1Device = pad; race.vehicle.GetComponent<VehicleInput>().Bind(pad); }
                else if (MissingPlayer == 2 && !(SplitScreen.P2Device is Keyboard)) { SplitScreen.P2Device = pad; if (P2Car && !SplitScreen.P2Ai) P2Car.GetComponent<VehicleInput>().Bind(pad); }
                if (PausedBy != null && !PausedBy.added) PausedBy = pad;
                MissingPlayer = 0; flow.Notify("Controller reconnected: resume when ready", 5); flow.RefreshMenu();
            }
        }
        public string MissingText => MissingPlayer == 0 ? null : $"PLAYER {MissingPlayer}'S CONTROLLER IS DISCONNECTED\nReconnect it to resume, or end the race.";

        public void Teardown()
        {
            InputSystem.onDeviceChange -= DeviceChanged;
            ClearPlayerTwo();
            if (cam1) { cam1.rect = rect1; cam1.farClipPlane = far1; var c = cam1.GetComponent<ChaseCamera>(); if (c) c.flightTilt = 0; }
            if (cam2) Destroy(cam2.gameObject);
            if (race && race.vehicle) race.vehicle.GetComponent<VehicleInput>().Bind(null);
            QualitySettings.lodBias = lodBias;
            if (hud) { hud.Teardown(); Destroy(hud); }
            SplitScreen.Eyes.Clear(); PausedBy = null;
        }
        void OnDestroy() { InputSystem.onDeviceChange -= DeviceChanged; }
    }
}
