using System.Collections.Generic;
using UnityEngine;

namespace Racer
{
    // 1.2 HIGH MODE (Dan, 2026-10-25: "pure mindless fun"): one ON / OFF switch at the top of the main menu, remembered per save
    // (settings.json highMode), so the game starts in the mode last used. On: fewer menu choices (RaceMenus.High.cs), everything unlocked and
    // stock (Campaign.Testing, so the campaign save is never written), gutter bumpers along the race courses (HighBumpers), forgiving driving
    // (HighAssist), easier rivals (Easy), its own records ("high-" boards, marked HIGH). A race keeps the mode it started with
    // (RaceDirector.high); Free Roam follows the switch at once. HighPrompt offers it to a player who keeps wrecking in normal mode.
    public static class HighMode
    {
        public const string RecordPrefix = "high-";
        public static bool On { get; private set; }
        public static void Set(bool on) => On = on;
        // the session now (race or Free Roam) is in HIGH MODE: its records and unlocks are kept apart
        public static bool Now => Hints.Flow && Hints.Flow.Race && Hints.Flow.Race.high;
        // "No" to "Would you like to play HIGH MODE?": not asked again this session
        public static bool Declined;
        // TIME & WEATHER (applies to Free Roam and races in HIGH MODE)
        public static readonly string[] TimeNames = { "Morning", "Day", "Sunset", "Night", "Day cycle" };
        public static readonly string[] WeatherNames = { "Clear", "Rain", "Snow", "Random" };
        static RacerSave.Options S => Hints.Flow && Hints.Flow.Save != null ? Hints.Flow.Save.Settings : null;
        public static int TimeChoice => Mathf.Clamp(S?.highTime ?? 4, 0, 4);
        public static int WeatherChoice => Mathf.Clamp(S?.highWeather ?? 0, 0, 3);
        static Weather rolled;
        // Random: a new weather for each race and each Free Roam start
        public static void Roll() => rolled = (Weather)Random.Range(0, 3);
        public static TimeOfDay RaceTime => TimeChoice switch { 0 => TimeOfDay.Dawn, 2 => TimeOfDay.Dusk, 3 => TimeOfDay.Night, _ => TimeOfDay.Day };
        public static Weather Weather => WeatherChoice == 3 ? rolled : (Weather)WeatherChoice;
        // Free Roam's sky held at this hour (-1: the normal day cycle)
        public static float FixedHour => TimeChoice switch { 0 => 6.3f, 1 => 12.5f, 2 => 19.4f, 3 => 23f, _ => -1 };
        public static string Badge => "HIGH";
        public static readonly Color Colour = new(1f, .62f, .2f);
    }

    // Forgiving driving on the players' own vehicles in a HIGH MODE race or Free Roam: a vehicle on its roof or side flips itself upright after
    // about a second (no wreck), the flight levels itself more so landings come down straighter, and a hit (wall, bumper, traffic) gives back
    // half the speed it took (never into what was hit). VehicleRespawn resets quicker; VehicleConfiguration never wipes out.
    public sealed class HighAssist : MonoBehaviour
    {
        ArcadeVehicle car; VehicleRespawn respawn; RaceDirector race;
        Vector3 lastVelocity, hitNormal; bool hit; float overturned;
        public static int Righted { get; private set; } // checks
        public bool On => race && race.high && car && !car.Body.isKinematic && !car.Puppet;
        public static void Attach(RaceDirector race)
        {
            foreach (var c in new[] { race.vehicle, SplitScreen.Race ? SplitScreen.Race.P2Car : null })
            {
                if (!c) continue; var a = c.GetComponent<HighAssist>(); if (!a) a = c.gameObject.AddComponent<HighAssist>();
                a.race = race;
            }
        }
        void Awake() { car = GetComponent<ArcadeVehicle>(); respawn = GetComponent<VehicleRespawn>(); }
        void FixedUpdate()
        {
            bool on = On; car.Forgiving = on;
            if (!on) { overturned = 0; hit = false; lastVelocity = car.Body.linearVelocity; return; }
            if (hit)
            {
                hit = false;
                var lost = Vector3.ProjectOnPlane(lastVelocity - car.Body.linearVelocity, Vector3.up);
                var n = Vector3.ProjectOnPlane(hitNormal, Vector3.up).normalized; if (Vector3.Dot(n, lastVelocity) > 0) n = -n;
                float into = Vector3.Dot(lost, -n); if (into > 0) lost += n * into;
                if (lost.magnitude > 1.5f) car.Body.linearVelocity += lost * .5f;
            }
            lastVelocity = car.Body.linearVelocity;
            bool over = transform.up.y < .45f && car.Body.linearVelocity.magnitude < 8 && car.GroundedWheels < 2 && !(respawn && respawn.Pending);
            overturned = over ? overturned + Time.fixedDeltaTime : 0;
            if (overturned >= 1f) Right();
        }
        void Right()
        {
            overturned = 0; var f = Vector3.ProjectOnPlane(transform.forward, Vector3.up);
            if (f.sqrMagnitude < .02f) f = Vector3.ProjectOnPlane(transform.up, Vector3.up);
            if (f.sqrMagnitude < .02f) f = Vector3.forward;
            var rot = Quaternion.LookRotation(f.normalized, Vector3.up); var pos = car.Body.position + Vector3.up * 1.1f;
            car.Body.position = pos; car.Body.rotation = rot; transform.SetPositionAndRotation(pos, rot);
            car.Body.linearVelocity = Vector3.ProjectOnPlane(car.Body.linearVelocity, Vector3.up) * .5f; car.Body.angularVelocity = Vector3.zero;
            GetComponent<VehicleConfiguration>()?.Recover(); Physics.SyncTransforms(); Righted++;
        }
        void OnCollisionEnter(Collision c)
        {
            if (!On || c.contactCount == 0) return;
            var n = c.GetContact(0).normal; if (Mathf.Abs(n.y) > .6f) return; // the ground, a landing, a ramp: not a hit
            hit = true; hitNormal = n;
        }
        void OnDisable() { if (car) car.Forgiving = false; }
    }

    // "Would you like to play HIGH MODE?" (Part F): in normal mode, 3 wrecks or resets within 2 minutes, or 2 in one lap, pauses and asks.
    // A reset right after a wreck is the same mishap; map travel and a campaign event's run-up reset are not mishaps. Not in a championship
    // round, the campaign's jump / trap / smash / park events, a replay or Trailer Mode. No: not asked again this session.
    public sealed class HighPrompt : MonoBehaviour
    {
        public const int InTwoMinutes = 3, InOneLap = 2; public const float Window = 120, SameMishap = 5;
        RaceFlow flow; VehicleRespawn respawn; VehicleConfiguration config; int wipeouts, lap = -1, lapCount;
        readonly List<float> times = new(); float lastWreck = -100;
        public static int Asked { get; private set; } // checks
        public void Initialize(RaceFlow owner) { flow = owner; Watch(); }
        void Watch()
        {
            var car = flow.Race.vehicle; if (respawn) respawn.Respawned -= OnReset;
            respawn = car.GetComponent<VehicleRespawn>(); config = car.GetComponent<VehicleConfiguration>();
            if (respawn) respawn.Respawned += OnReset; wipeouts = config ? config.Wipeouts : 0;
        }
        void OnDestroy() { if (respawn) respawn.Respawned -= OnReset; }
        bool Watching
        {
            get
            {
                if (!flow || flow.Save == null || HighMode.On || HighMode.Declined || flow.Race.high || flow.State != RaceFlow.Stage.Racing) return false;
                if (ReplayViewer.Active || TrailerMode.Active || DeveloperLocationHud.Inspecting) return false;
                var e = CampaignRun.Active; if (CampaignRun.Cup != null) return false;
                if (e != null && e.Kind is CampaignEventKind.SpeedTrap or CampaignEventKind.Jump or CampaignEventKind.Smash or CampaignEventKind.Trick or CampaignEventKind.Clean or CampaignEventKind.Park) return false;
                return true;
            }
        }
        void OnReset()
        {
            if (!Watching || respawn.QuietReset) return;
            if (Time.time - lastWreck < SameMishap) return; // the reset after a wreck: one mishap
            Mishap(false);
        }
        void Mishap(bool wreck)
        {
            if (wreck) lastWreck = Time.time;
            times.Add(Time.time); lapCount++;
        }
        void Update()
        {
            if (!flow) return;
            if (flow.Race.high && SplitScreen.Race && SplitScreen.Race.P2Car && !SplitScreen.Race.P2Car.GetComponent<HighAssist>()) HighAssist.Attach(flow.Race); // player 2's vehicle made after the start
            if (config && config.gameObject != flow.Race.vehicle.gameObject) Watch();
            if (config && config.Wipeouts != wipeouts) { int n = config.Wipeouts - wipeouts; wipeouts = config.Wipeouts; if (Watching) for (int i = 0; i < n; i++) Mishap(true); }
            if (!Watching) { if (flow.State != RaceFlow.Stage.Racing && flow.State != RaceFlow.Stage.Paused && flow.State != RaceFlow.Stage.Settings) { times.Clear(); lapCount = 0; lap = -1; } return; }
            int laps = flow.Race.FreeRoam ? -1 : flow.Race.Progress.CompletedLaps; if (laps != lap) { lap = laps; lapCount = 0; }
            times.RemoveAll(t => Time.time - t > Window);
            if (times.Count >= InTwoMinutes || (!flow.Race.FreeRoam && lapCount >= InOneLap)) { times.Clear(); lapCount = 0; Asked++; flow.AskHighMode(); }
        }
    }
}
