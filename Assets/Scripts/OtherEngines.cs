using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Racer
{
    // 0.113 Part E: the other vehicles' engines (rivals, traffic, Getaway cops). VehicleAudio's engine model (the same two synthesized
    // loops, rev estimate from that car's own speed and pedal, the profile's pitch, so bikes / ATVs / the mower sound like themselves),
    // played from where the car is. Only the nearest few play: 4 rivals / cops and 2 traffic, through a pool of 6 voices that fade in
    // and out when the set changes. The player's own engine (VehicleAudio, 2D) is not touched; any car with a VehicleAudio is skipped.
    // Split-screen: the one listener follows player 1's camera, so 3D placement would put player 2's cars in the wrong place; there each
    // player gets their own nearest cars (2 rivals / cops and 1 traffic each) and the volume falls off with the distance to that player's
    // vehicle (no left / right placement, as with the players' own engines). Follows the Vehicle volume; silent outside the race like the
    // player's engine; the listener pause stops them in the pause menu.
    public sealed class OtherEngines : MonoBehaviour
    {
        public const int RivalVoices = 4, TrafficVoices = 2;
        public const float MinDistance = 7, Audible = 140, Quieter = .7f;
        sealed class Voice
        {
            public Transform at; public AudioSource idle, load;
            public ArcadeVehicle car, next; public float level, rpm = .85f, pedal;
        }
        readonly List<Voice> voices = new();
        readonly List<(ArcadeVehicle car, bool traffic)> candidates = new();
        readonly HashSet<ArcadeVehicle> wanted = new();
        RaceFlow flow; float nextPick;
        public static OtherEngines Current { get; private set; }
        public static bool Disabled; // checks only (Report106PlayerCheck audio113: the frame time without them); never set in play
        public int Playing { get; private set; }
        public string Info { get; private set; } = ""; // checks: what the last pick saw
        public IEnumerable<ArcadeVehicle> Heard { get { foreach (var v in voices) if (v.car && v.level > .01f) yield return v.car; } }
        // checks: each playing voice's car, its fade level and the two sources' volumes and pitch
        public IEnumerable<(ArcadeVehicle car, float level, float idle, float load, float pitch, float spatial)> Voices { get { foreach (var v in voices) if (v.car) yield return (v.car, v.level, v.idle.volume, v.load.volume, v.idle.pitch, v.idle.spatialBlend); } }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            if (Current) return;
            var g = new GameObject("Other vehicles' engines"); DontDestroyOnLoad(g); Current = g.AddComponent<OtherEngines>();
        }
        void Awake()
        {
            var idleClip = VehicleSoundSynthesis.Shared("Engine idle", 0); var loadClip = VehicleSoundSynthesis.Shared("Engine load", 1);
            for (int i = 0; i < RivalVoices + TrafficVoices; i++)
            {
                var t = new GameObject("Engine voice " + i).transform; t.SetParent(transform, false);
                AudioSource Make(AudioClip clip)
                {
                    var s = t.gameObject.AddComponent<AudioSource>(); s.clip = clip; s.loop = true; s.playOnAwake = false; s.volume = 0;
                    s.dopplerLevel = 0; s.spread = 0; s.priority = 140; s.rolloffMode = AudioRolloffMode.Logarithmic; s.minDistance = MinDistance; s.maxDistance = Audible;
                    s.timeSamples = Random.Range(0, clip.samples); return s;
                }
                voices.Add(new Voice { at = t, idle = Make(idleClip), load = Make(loadClip) });
            }
        }
        void Update()
        {
            if (!flow) flow = FindAnyObjectByType<RaceFlow>();
            var race = flow ? flow.Race : null;
            bool engine = race && (flow.State == RaceFlow.Stage.Racing || flow.State == RaceFlow.Stage.Countdown);
            bool paused = flow && (flow.State == RaceFlow.Stage.Paused || flow.State == RaceFlow.Stage.Settings);
            if (paused) return; // the listener pause holds them where they are
            if (!engine || Disabled) { foreach (var v in voices) Stop(v); Playing = 0; return; }
            float dt = Mathf.Min(Time.unscaledDeltaTime, .05f);
            if (Time.unscaledTime >= nextPick) { nextPick = Time.unscaledTime + .25f; Pick(race); }
            bool split = SplitScreen.Active && SplitScreen.Race && SplitScreen.Race.P2Car;
            float volume = (flow.Save != null ? flow.Save.Settings.vehicle : .75f) * (SplitScreen.Active ? .6f : 1) * (1 - .3f * WeatherEffects.Duck) * Quieter;
            Playing = 0;
            foreach (var v in voices)
            {
                // a voice whose car left the set fades out first, then takes its new car
                bool keep = v.car && v.car.gameObject.activeInHierarchy && wanted.Contains(v.car);
                v.level = Mathf.MoveTowards(v.level, keep ? 1 : 0, dt * (keep ? 1.5f : 2.5f));
                if (v.level <= 0 && !keep) { v.car = v.next; v.next = null; v.rpm = .85f; v.pedal = 0; }
                if (!v.car || !v.car.Body) { Stop(v); continue; }
                var car = v.car; v.at.position = car.Body.position;
                float speed = Mathf.Abs(car.ForwardSpeed);
                float pedal = Throttle(car);
                v.pedal = Mathf.MoveTowards(v.pedal, pedal, dt * 3);
                float revs = .82f + Mathf.Sqrt(Mathf.Clamp01(speed / Mathf.Max(1, car.topSpeed))) * 1.25f + v.pedal * .22f;
                v.rpm = Mathf.MoveTowards(v.rpm, revs, dt * (pedal > .05f ? 1.25f : .65f));
                var configuration = car.GetComponent<VehicleConfiguration>(); float pitch = configuration ? configuration.Profile.Pitch : 1;
                v.idle.pitch = v.rpm * pitch; v.load.pitch = v.rpm * pitch * 1.005f;
                float gain = v.level * volume;
                float spatial = split ? 0 : 1;
                if (split) { float d = SplitScreen.DistanceToPlayers(car.Body.position, race.vehicle.Body.position); gain *= Mathf.Min(1, MinDistance / Mathf.Max(MinDistance, d)) * Mathf.InverseLerp(Audible, Audible * .75f, d); }
                else if (Camera.main) gain *= Mathf.InverseLerp(Audible, Audible * .75f, Vector3.Distance(Camera.main.transform.position, car.Body.position));
                v.idle.spatialBlend = v.load.spatialBlend = spatial;
                v.idle.volume = Mathf.Lerp(.17f, .10f, v.pedal) * gain; v.load.volume = .16f * v.pedal * gain;
                if (!v.idle.isPlaying) { v.idle.Play(); v.load.Play(); }
                if (gain > .001f) Playing++;
            }
        }
        static float Throttle(ArcadeVehicle car)
        {
            var road = car.GetComponent<RoadDriver>(); if (road && road.enabled) return road.LastThrottle;
            var cop = car.GetComponent<CopDriver>(); if (cop && cop.enabled) return cop.LastThrottle;
            return 0;
        }
        static void Stop(Voice v)
        {
            v.level = 0; v.car = v.next = null; v.idle.volume = v.load.volume = 0;
            if (v.idle.isPlaying) { v.idle.Stop(); v.load.Stop(); }
        }
        void Pick(RaceDirector race)
        {
            candidates.Clear();
            var p2 = SplitScreen.Active && SplitScreen.Race ? SplitScreen.Race.P2Car : null;
            bool Eligible(ArcadeVehicle c) => c && c != race.vehicle && c != p2 && c.gameObject.activeInHierarchy && c.Body && !HasOwnAudio(c);
            foreach (var d in race.Drivers) if (d && Eligible(d.Car)) candidates.Add((d.Car, d.GetComponent<AmbientVehicle>()));
            var chase = GetawayChase.Current;
            if (chase) foreach (var cop in chase.Cops) if (cop.car && Eligible(cop.car)) candidates.Add((cop.car, false));
            float near = float.MaxValue; foreach (var c in candidates) near = Mathf.Min(near, Vector3.Distance(c.car.Body.position, race.vehicle.Body.position));
            Info = $"flow {flow.name} in {flow.gameObject.scene.name} state {flow.State}; drivers {race.Drivers.Count}, candidates {candidates.Count} (traffic {candidates.Count(c => c.traffic)}), cops {(chase ? chase.Cops.Count : -1)}, nearest {(candidates.Count > 0 ? near.ToString("F0") : "-")} m";
            wanted.Clear();
            var players = new List<Vector3> { race.vehicle.Body.position };
            if (p2) players.Add(p2.Body.position);
            int rivalsEach = players.Count > 1 ? RivalVoices / 2 : RivalVoices, trafficEach = players.Count > 1 ? TrafficVoices / 2 : TrafficVoices;
            foreach (var at in players)
                foreach (bool traffic in new[] { false, true })
                {
                    candidates.Sort((a, b) => (a.car.Body.position - at).sqrMagnitude.CompareTo((b.car.Body.position - at).sqrMagnitude));
                    int n = 0;
                    foreach (var c in candidates)
                    {
                        if (c.traffic != traffic || n >= (traffic ? trafficEach : rivalsEach)) continue;
                        if ((c.car.Body.position - at).sqrMagnitude > Audible * Audible) break;
                        if (wanted.Add(c.car)) n++;
                    }
                }
            // every wanted car not yet on a voice goes to a free voice, or to one fading out (as its next car)
            foreach (var car in wanted)
            {
                bool placed = false;
                foreach (var v in voices) if (v.car == car || v.next == car) { placed = true; break; }
                if (placed) continue;
                foreach (var v in voices) if (!v.car) { v.car = car; v.level = 0; v.rpm = .85f; v.pedal = 0; placed = true; break; }
                if (placed) continue;
                foreach (var v in voices) if (!wanted.Contains(v.car) && !v.next) { v.next = car; break; }
            }
        }
        static bool HasOwnAudio(ArcadeVehicle c) { var a = c.GetComponent<VehicleAudio>(); return a && a.enabled; }
    }
}
