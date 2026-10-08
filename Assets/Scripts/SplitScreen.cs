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
        // 0.92 Part F (stage 2): AI rivals (0-4, their difficulty and vehicle mix), the time of day and weather, traffic.
        // Kept for rematches and "Change setup"; never saved.
        public static int Rivals, RivalDifficulty = 1; public static bool RivalsRandom, Traffic;
        public static TimeOfDay Time = TimeOfDay.Day; public static Weather Weather = Weather.Clear;
        // 0.94 Part A: the players' names (player 1 starts as the saved player name; player 2 picks a name used before on
        // this PC or enters one; the AI driver is "AI").
        public static string P1Name = "", P2Pick = "";
        // 0.94 Parts B and C (stage 3): what the two players do: a race, Free Roam (FreeRoamWorld, starting at Course's start)
        // or Police Chase (Free Roam, cop vs runner: who is the cop first, and the round limit in minutes).
        public enum Kind { Race, FreeRoam, Police }
        public static Kind Mode = Kind.Race;
        public static int CopFirst = 1, PoliceMinutes = 5;
        // 0.95 Parts A and B: Police Chase has its own setup (main menu / Free Roam > POLICE CHASE). Solo = one player against
        // the AI on a full screen (no split, one role kept, one round); two players = split-screen with the swap. SoloRole:
        // 1 = cop, 2 = runner (the runner needs the chasing AI: coming later). FromRoam: started from inside Free Roam, so the
        // menu after it goes back to Free Roam (at RoamReturn's start).
        // 0.95 Part G: the Police Chase game: cop vs runner, or Speed Patrol (everyone a cop catching speeders for points)
        public enum Game { CopRunner, SpeedPatrol }
        public static Game PoliceGame = Game.CopRunner;
        public static bool Solo = true, FromRoam; public static int SoloRole = 1, RoamReturn;
        public static bool OneView => Active && Solo && Mode == Kind.Police;
        public static bool Roaming => Active && Mode != Kind.Race;
        public static string P2Name => P2Ai ? PlayerNames.Ai : string.IsNullOrEmpty(P2Pick) ? "Player 2" : P2Pick;
        public static UnityEngine.InputSystem.InputDevice DeviceOf(int player) => player == 1 ? P1Device : P2Ai ? null : P2Device;
        public static string NameOf(int player) => player == 1 ? (string.IsNullOrEmpty(P1Name) ? PlayerNames.Player : P1Name) : P2Name;
        // The two views' cameras while a split-screen race runs (empty otherwise).
        public static readonly List<Camera> Views = new();
        // The nearer of the two players (player 1 at p1) to a point.
        public static float DistanceToPlayers(Vector3 point, Vector3 p1)
        {
            float d = Vector3.Distance(point, p1);
            if (Race && Race.P2Car) d = Mathf.Min(d, Vector3.Distance(point, Race.P2Car.transform.position));
            return d;
        }
        // Whether either view shows a point (margin as a fraction of the screen); false when not split.
        public static bool InAView(Vector3 point, float margin)
        {
            foreach (var cam in Views)
            {
                if (!cam) continue; var v = cam.WorldToViewportPoint(point);
                if (v.z > 0 && v.x > -margin && v.x < 1 + margin && v.y > -margin && v.y < 1 + margin) return true;
            }
            return false;
        }
        // The rivals' vehicles: Mixed takes each vehicle once before repeating, Random may repeat; only vehicles the course
        // allows (motorcycles and ATVs on the restricted courses).
        static string[] RivalRoster(string scene)
        {
            var pool = VehicleProfile.All.Where(p => !p.Reward && (CarAccess.CourseAllowsCars(scene) || p.Small)).Select(p => p.Id).ToList();
            var list = new List<string>(); var deck = new List<string>();
            for (int i = 0; i < Mathf.Clamp(Rivals, 0, 4); i++)
            {
                if (RivalsRandom) { list.Add(pool[Random.Range(0, pool.Count)]); continue; }
                if (deck.Count == 0) deck.AddRange(pool.OrderBy(_ => Random.value));
                list.Add(deck[0]); deck.RemoveAt(0);
            }
            return list.ToArray();
        }
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
            if (Mode != Kind.Race)
            {
                // 0.94 Part B: Free Roam for two (player 2's vehicle is made by SplitRoam); Part C: Police Chase on top of it
                race.opponents = false; race.traffic = Traffic;
                var c1 = race.vehicle.GetComponent<VehicleConfiguration>(); c1.Apply(P1Vehicle); c1.SetBodyColor(P1Color);
                flow.gameObject.AddComponent<SplitRoam>().Initialize(flow);
                if (Mode == Kind.Police) { if (Solo) P2Ai = true; if (PoliceGame == Game.SpeedPatrol) { race.traffic = true; flow.gameObject.AddComponent<SpeedPatrol>().Initialize(flow); } else { if (P2Ai) CopFirst = SoloRole == 2 ? 2 : 1; flow.gameObject.AddComponent<PoliceChase>().Initialize(flow); } }
                Race = flow.gameObject.AddComponent<SplitRace>(); Race.Initialize(flow); return;
            }
            // 0.92 Part F: player 2's vehicle is the first rival slot; the AI rivals follow it
            race.opponents = true; race.opponentRoster = new[] { P2Vehicle }.Concat(RivalRoster(flow.gameObject.scene.name)).ToArray(); race.traffic = Traffic; race.difficulty = Mathf.Clamp(RivalDifficulty, 0, 2); race.laps = Mathf.Clamp(Laps, 1, 5);
            var c = race.vehicle.GetComponent<VehicleConfiguration>(); c.Apply(P1Vehicle); c.SetBodyColor(P1Color);
            Race = flow.gameObject.AddComponent<SplitRace>(); Race.Initialize(flow);
        }
        public static void End()
        {
            if (Race) { Race.Teardown(); Object.Destroy(Race); }
            if (PoliceChase.Current) Object.Destroy(PoliceChase.Current); if (SpeedPatrol.Current) Object.Destroy(SpeedPatrol.Current); if (SplitRoam.Current) { SplitRoam.Current.Clear(); Object.Destroy(SplitRoam.Current); }
            Race = null; Active = false; PendingStart = false; Views.Clear();
        }
        // The positions of every split-screen view (the scenery's near detail follows each of them); empty when not split.
        public static readonly List<Vector3> Eyes = new();
    }

    public sealed class SplitRace : MonoBehaviour
    {
        RaceFlow flow; RaceDirector race; Camera cam1, cam2; ChaseCamera chase2; Rect rect1; float far1, lodBias; int mask1;
        public Camera Camera2 => cam2;
        // 0.92 Part F: the layers a view's own weather is drawn on (rain, snow, stars, moon, mist), each hidden from the other view
        public const int View1Layer = 29, View2Layer = 30;
        public ArcadeVehicle P2Car { get; private set; }
        public RacerState P2 { get; private set; }
        public WrongWayGuidance P2Guidance { get; private set; }
        public int MissingPlayer { get; private set; } // 0 = both devices present
        public InputDevice PausedBy;
        SplitHud hud; bool p2Done; GameObject divider; CameraViews views2;

        public void Initialize(RaceFlow owner)
        {
            flow = owner; race = flow.Race;
            race.vehicle.GetComponent<VehicleInput>().Bind(SplitScreen.P1Device);
            InputSystem.onDeviceChange += DeviceChanged;
            // per-view detail, lowered for the two views only (restored in Teardown); single-player rendering is unchanged
            lodBias = QualitySettings.lodBias; if (!SplitScreen.OneView) QualitySettings.lodBias = lodBias * LodScale; // 0.95 Part B: one view = single-player detail
            // 0.92 Part F: two views with weather, night lamps, traffic and rivals: shadows drawn to 60 % of the distance (built players only: in the editor it would change the asset itself)
            var urp = Application.isEditor || SplitScreen.OneView ? null : GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset; if (urp) { shadowDistance = urp.shadowDistance; urp.shadowDistance = shadowDistance * ShadowScale; }
            cam1 = Camera.main; rect1 = cam1.rect; far1 = cam1.farClipPlane; mask1 = cam1.cullingMask;
            hud = gameObject.AddComponent<SplitHud>(); hud.Initialize(flow, this);
        }
        public const float LodScale = .6f, FarClip = 1000, ShadowScale = .6f; float shadowDistance = -1;

        // RaceDirector.RestartRace: player 2's vehicle is the one rival slot; make it player 2's (or leave the AI driving).
        public void PlayerTwo()
        {
            if (SplitRoam.Current && race.FreeRoam && SpeedPatrol.Current)
            {
                // 0.95 Part G: Speed Patrol: player 1 (and player 2 with two players, in the second livery) in a patrol car
                var patrol = SpeedPatrol.Current; patrol.ApplyPlayerOne(); finished1 = finished2 = false;
                if (SplitScreen.Solo) { P2Car = null; P2 = null; hud.Layout(false); }
                else { P2Car = SplitRoam.Current.CreatePlayerTwo(VehicleProfile.Police.Id, SplitScreen.P2Color); P2Car.GetComponent<VehicleConfiguration>().SetPaint(SpeedPatrol.SecondLivery); P2 = null; if (!P2Car.GetComponent<VehicleAudio>()) P2Car.gameObject.AddComponent<VehicleAudio>(); Cameras(); }
                patrol.BeginRound(); return;
            }
            if (SplitRoam.Current && race.FreeRoam)
            {
                // 0.94 Part B: Free Roam for two: player 2's vehicle beside player 1 (Police Chase: the round's vehicles and places)
                var police = PoliceChase.Current; if (police) police.ApplyPlayerOne();
                P2Car = SplitRoam.Current.CreatePlayerTwo(police ? police.VehicleFor(2) : SplitScreen.P2Vehicle, SplitScreen.P2Color); P2 = null;
                if (!P2Car.GetComponent<VehicleAudio>()) P2Car.gameObject.AddComponent<VehicleAudio>();
                finished1 = finished2 = false; EndShot(1); EndShot(2); Cameras();
                if (police) police.BeginRound();
                return;
            }
            if (race.Racers.Count < 2) return;
            var state = race.Racers[1]; var car = state.Car; P2Car = car; car.name = "PLAYER 2";
            var config = car.GetComponent<VehicleConfiguration>(); config.SetBodyColor(SplitScreen.P2Color); // 0.92: the champion's scheme too
            if (!SplitScreen.P2Ai)
            {
                var driver = car.GetComponent<RoadDriver>(); race.Drivers.Remove(driver); if (driver) { driver.enabled = false; Destroy(driver); }
                P2 = new RacerState(SplitScreen.P2Name, car, race.gates.Length - 1, race.laps); race.Racers[1] = P2; P2.SampleOrigin(Time.timeAsDouble);
                var input = car.GetComponent<VehicleInput>(); input.Bind(SplitScreen.P2Device); input.enabled = false;
                car.enabled = false; car.Body.isKinematic = true;
                var respawn = car.GetComponent<VehicleRespawn>(); respawn.enabled = true; respawn.SeedCoursePosition(car.Body.position);
            }
            else { P2 = state; P2.Name = PlayerNames.Ai; car.name = "PLAYER 2 (AI)"; }
            if (!car.GetComponent<VehicleAudio>()) car.gameObject.AddComponent<VehicleAudio>();
            P2Guidance = gameObject.AddComponent<WrongWayGuidance>(); P2Guidance.Target = P2;
            p2Done = false; finished1 = finished2 = false; EndShot(1); EndShot(2); Cameras();
            attempt1 = System.Guid.NewGuid().ToString("N"); attempt2 = System.Guid.NewGuid().ToString("N"); laps1 = laps2 = 0; raced1 = raced2 = false; TopTen.Clear();
        }
        // RaceDirector.RestartRace / AbandonEvent: player 2's vehicle goes (a human one is no longer in the AI list).
        public void ClearPlayerTwo()
        {
            if (P2Guidance) { Destroy(P2Guidance); P2Guidance = null; }
            if (SplitRoam.Current) SplitRoam.Current.Clear();
            else if (P2Car && !SplitScreen.P2Ai) { P2Car.gameObject.SetActive(false); Destroy(P2Car.gameObject); }
            P2Car = null; P2 = null;
        }
        void Cameras()
        {
            bool lr = SplitScreen.LeftRight, one = SplitScreen.OneView;
            // 0.95 Part B: a solo Police Chase is one full-screen view (player 2 is the AI: its camera is never drawn)
            cam1.rect = one ? rect1 : lr ? new Rect(0, 0, .5f, 1) : new Rect(0, .5f, 1, .5f);
            if (!cam2)
            {
                var go = new GameObject("Player 2 camera"); cam2 = go.AddComponent<Camera>(); cam2.CopyFrom(cam1);
                var d1 = cam1.GetUniversalAdditionalCameraData(); var d2 = cam2.GetUniversalAdditionalCameraData();
                d2.renderPostProcessing = d1.renderPostProcessing; d2.antialiasing = d1.antialiasing; d2.renderShadows = d1.renderShadows;
                chase2 = go.AddComponent<ChaseCamera>(); var chase1 = cam1.GetComponent<ChaseCamera>(); chase2.obstructionMask = chase1.obstructionMask;
            }
            cam2.rect = lr ? new Rect(.5f, 0, .5f, 1) : new Rect(0, 0, 1, .5f); cam2.enabled = !one;
            cam1.farClipPlane = cam2.farClipPlane = one ? far1 : Mathf.Min(far1, FarClip);
            cam1.cullingMask = mask1 & ~(1 << View2Layer); cam2.cullingMask = (mask1 & ~(1 << View1Layer)) | (1 << View2Layer);
            chase2.enabled = false; chase2.target = P2Car.transform; chase2.offset = P2Car.GetComponent<VehicleConfiguration>().Profile.Camera; chase2.enabled = true; chase2.Snap();
            // 0.94 Part B: player 2's own camera views (player 1's are the scene camera's CameraViews)
            views2 = CameraViews.AttachSecond(flow, cam2, chase2, P2Car); if (CameraViews.Current) CameraViews.Current.Player = 1;
            // the wide top / bottom halves: the camera tips down a little in long flights so the landing stays in view
            float tilt = lr || one ? 0 : 9; cam1.GetComponent<ChaseCamera>().flightTilt = tilt; chase2.flightTilt = tilt;
            hud.Layout(lr);
        }
        void Update()
        {
            if (!race || P2Car == null) return;
            var stage = flow.State;
            if (SplitRoam.Current)
            {
                // 0.94 Part B: Free Roam for two: the views only (no race to run)
                bool two = !SplitScreen.OneView;
                SplitScreen.Eyes.Clear(); if (cam1) SplitScreen.Eyes.Add(cam1.transform.position); if (cam2 && two) SplitScreen.Eyes.Add(cam2.transform.position);
                SplitScreen.Views.Clear(); if (cam1) SplitScreen.Views.Add(cam1); if (cam2 && two) SplitScreen.Views.Add(cam2);
                return;
            }
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
            Record(1); Record(2);
            SplitScreen.Eyes.Clear(); if (cam1) SplitScreen.Eyes.Add(cam1.transform.position); if (cam2) SplitScreen.Eyes.Add(cam2.transform.position);
            SplitScreen.Views.Clear(); if (cam1) SplitScreen.Views.Add(cam1); if (cam2) SplitScreen.Views.Add(cam2);
            // 0.92 Part F: with AI rivals the race is decided once both players have finished: the rivals still running get
            // the game's estimate of their finish (as single-player's "Complete Race")
            // 0.92 Part F: a player who wins gets the winner shot in their own half when they finish
            bool f1 = race.Racers[0].Progress.Finished, f2 = P2 != null && P2.Progress.Finished;
            if (f1 && !finished1) { finished1 = true; if (Winner() == race.Racers[0]) StartShot(1); }
            if (f2 && !finished2) { finished2 = true; if (!SplitScreen.P2Ai && Winner() == P2) StartShot(2); }
            if (stage == RaceFlow.Stage.Racing && race.Racers.Count > 2 && race.Progress.Finished && P2 != null && P2.Progress.Finished && !race.ClassificationFinal) race.FinalizeUnfinishedAi();
        }
        // ---------- 0.94 Part A: the players' times on the Top 10 ----------
        // A human's laps and race total go on the course's Top 10 under their name, in the board a single-player race with
        // these settings uses (their vehicle, the AI rivals - not the other player - their difficulty, traffic, laps) and under
        // the same rules (no debug movement). Not recorded: the AI driver, ghosts, personal-best files, campaign, acorns.
        string attempt1, attempt2; int laps1, laps2; bool raced1, raced2;
        public readonly List<string> TopTen = new();
        public string Category(int player)
        {
            var car = player == 1 ? race.vehicle : P2Car; var rivals = race.opponentRoster.Skip(1).ToArray();
            return $"{race.courseId}-{car.GetComponent<VehicleConfiguration>().profileId}-{(rivals.Length > 0 ? "race4-d" + race.difficulty + "-" + string.Join("-", rivals) : "solo")}-{(race.traffic ? "traffic" : "clear")}-laps{race.laps}";
        }
        void Record(int player)
        {
            var state = player == 1 ? race.Racers[0] : P2; if (state == null || (player == 2 && SplitScreen.P2Ai) || flow.DebugMovementUsed || flow.Boards == null) return;
            var p = state.Progress; ref int seen = ref (player == 1 ? ref laps1 : ref laps2); ref bool raced = ref (player == 1 ? ref raced1 : ref raced2);
            string attempt = player == 1 ? attempt1 : attempt2; string name = SplitScreen.NameOf(player); var car = player == 1 ? race.vehicle : P2Car;
            if (string.IsNullOrEmpty(attempt) || !car) return;
            string vehicle = car.GetComponent<VehicleConfiguration>().profileId, category = Category(player);
            if (p.CompletedLaps > seen && p.LapTimes.Count == p.CompletedLaps)
            {
                seen = p.CompletedLaps; int rank = flow.Boards.CompletedLap("split-" + attempt, category, vehicle, p, name);
                if (rank > 0) TopTen.Add($"{name} lap #{rank}");
            }
            if (p.Finished && !raced)
            {
                raced = true; int rank = flow.Boards.CompletedRace("split-" + attempt, category, vehicle, p, race.Clock, name);
                if (rank > 0) TopTen.Add($"{name} race #{rank}");
                if (flow.State == RaceFlow.Stage.Results) flow.RefreshMenu();
            }
        }
        // ---------- 0.92 Part F: the winner shot in a player's own half ----------
        // When a human player wins, their half shows them from the front three-quarter side for 2.5 s (their two-fist
        // celebration from the line is playing) before their finished panel; the other half is untouched and keeps racing.
        // An AI winner (a rival or player 2 as the AI driver) only celebrates on the road: no camera is taken from anyone.
        bool finished1, finished2; float shotUntil1, shotUntil2;
        public bool ShotActive(int player) => (player == 1 ? shotUntil1 : shotUntil2) > Time.unscaledTime;
        public int Shots { get; private set; }
        RacerState Winner() { RacerState won = null; foreach (var r in race.Racers) if (r.Progress.Finished && (won == null || r.Progress.RaceTime(race.Clock) < won.Progress.RaceTime(race.Clock))) won = r; return won; }
        void StartShot(int player)
        {
            if (player == 1) { shotUntil1 = Time.unscaledTime + WinnerShot.Seconds; var c = cam1.GetComponent<ChaseCamera>(); if (c) c.enabled = false; if (CameraViews.Current) { CameraViews.Current.ShowHead(); CameraViews.Current.enabled = false; } }
            else { shotUntil2 = Time.unscaledTime + WinnerShot.Seconds; if (chase2) chase2.enabled = false; if (views2) { views2.ShowHead(); views2.enabled = false; } }
            Shots++;
        }
        void EndShot(int player)
        {
            if (player == 1) { if (shotUntil1 <= 0) return; shotUntil1 = 0; var c = cam1 ? cam1.GetComponent<ChaseCamera>() : null; if (c) c.enabled = true; if (CameraViews.Current) CameraViews.Current.enabled = true; }
            else { if (shotUntil2 <= 0) return; shotUntil2 = 0; if (chase2) chase2.enabled = true; if (views2) views2.enabled = true; }
        }
        void LateUpdate()
        {
            for (int player = 1; player <= 2; player++)
            {
                float until = player == 1 ? shotUntil1 : shotUntil2; if (until <= 0) continue;
                if (Time.unscaledTime >= until || flow.State != RaceFlow.Stage.Racing) { EndShot(player); continue; }
                var cam = player == 1 ? cam1 : cam2; var car = player == 1 ? race.vehicle : P2Car; if (!cam || !car) continue;
                var w = car.transform; var fwd = Vector3.ProjectOnPlane(w.forward, Vector3.up).normalized; var right = Vector3.Cross(Vector3.up, fwd);
                var size = car.GetComponent<VehicleConfiguration>()?.Profile.Size ?? new Vector3(1, 1, 3); float reach = Mathf.Max(4.5f, size.z * 1.3f);
                cam.transform.position = w.position + fwd * reach + right * reach * .6f + Vector3.up * 1.5f; cam.transform.LookAt(w.position + Vector3.up * .9f);
            }
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
            EndShot(1); EndShot(2);
            ClearPlayerTwo();
            if (cam1) { cam1.rect = rect1; cam1.farClipPlane = far1; cam1.cullingMask = mask1; var c = cam1.GetComponent<ChaseCamera>(); if (c) c.flightTilt = 0; }
            if (cam2) Destroy(cam2.gameObject);
            CameraViews.Current?.ResetSplitView();
            if (race && race.vehicle) race.vehicle.GetComponent<VehicleInput>().Bind(null);
            QualitySettings.lodBias = lodBias;
            { var urp = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset; if (urp && shadowDistance >= 0) urp.shadowDistance = shadowDistance; shadowDistance = -1; }
            if (hud) { hud.Teardown(); Destroy(hud); }
            SplitScreen.Eyes.Clear(); SplitScreen.Views.Clear(); PausedBy = null;
        }
        void OnDestroy() { InputSystem.onDeviceChange -= DeviceChanged; }
    }
}
