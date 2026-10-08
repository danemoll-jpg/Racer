using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Racer
{
    // 0.97 Part B: weather changes the grip. Rain and snow multiply the tyres' grip by the surface under each wheel (ArcadeVehicle
    // reads it from its suspension contacts): paved / dirt / grass, and ice (the frozen lake and pool in Snow). Clear is unchanged;
    // so is everything while Settings > Gameplay > "Weather affects grip" is Off (the 0.95 behaviour). Braking distance and cornering
    // speed follow from the grip; top speed is not reduced.
    public static class WeatherGrip
    {
        public enum Surface { Paved, Dirt, Grass, Ice }
        // multipliers on the tyres' grip: [Paved, Dirt, Grass, Ice]
        public static readonly float[] Rain = { .85f, .78f, .75f, .35f }, Snow = { .70f, .65f, .62f, .35f };
        static int frame = -1; static Weather now; static bool enabled = true;
        public static bool Enabled { get { Refresh(); return enabled; } }
        public static Weather Now { get { Refresh(); return now; } }
        // the weather that grips the road now (Clear in the menus and with the setting Off)
        public static Weather InEffect { get { Refresh(); return enabled ? now : Weather.Clear; } }
        static void Refresh()
        {
            if (frame == Time.frameCount) return; frame = Time.frameCount;
            var flow = Hints.Flow; enabled = flow == null || flow.Save == null || flow.Save.Settings == null || flow.Save.Settings.weatherGrip;
            var look = WorldLook.Current; now = Weather.Clear;
            if (look) now = look.Mode == "Free Roam" ? (look.Trailer != null ? look.Trailer.weather : look.RoamWeather) : look.Mode == "Race" ? look.RaceWeather : Weather.Clear;
        }
        public static float Scale(Weather weather, Surface surface) => weather == Weather.Clear ? 1 : (weather == Weather.Rain ? Rain : Snow)[(int)surface];
        // the record-key suffix that keeps wet and snow times off the dry boards: "" in Clear and with the setting Off (those boards keep every
        // entry set before 0.97, when weather did not affect grip), "-rain" or "-snow" otherwise. `chosen` is the race's own weather
        // (campaign event, split-screen setup or the Race Setup setting), so it holds after the finish too.
        public static string RecordTag(Weather chosen) { Refresh(); return !enabled ? "" : chosen == Weather.Rain ? "-rain" : chosen == Weather.Snow ? "-snow" : ""; }
        public static Weather ChosenFor(RaceDirector race)
        {
            if (SplitScreen.Active) return SplitScreen.Weather;
            var campaign = CampaignRun.Active; if (campaign != null) return campaign.Weather;
            var flow = race ? race.Flow : null; return flow != null && flow.Save != null ? (Weather)Mathf.Clamp(flow.Save.Settings.weather, 0, 2) : Weather.Clear;
        }
        // the grip under one wheel's contact
        public static float ForHit(Weather weather, Collider collider, Vector3 point)
        {
            if (collider && collider.name.StartsWith("Frozen water", System.StringComparison.Ordinal)) return Scale(weather, Surface.Ice);
            return Scale(weather, SurfaceAt(point));
        }

        // ---------- paved / dirt / grass by the roads' centre lines ----------
        static readonly List<Vector3> pts = new(); static readonly List<float> half = new(); static readonly List<bool> dirt = new();
        static readonly Dictionary<long, List<int>> grid = new(); const float Cell = 16, Step = 6;
        static string builtScene = ""; static float builtAt = -99;
        static long Key(int x, int z) => ((long)x << 32) ^ (uint)z;
        static void Build()
        {
            pts.Clear(); half.Clear(); dirt.Clear(); grid.Clear(); builtScene = SceneManager.GetActiveScene().name; builtAt = Time.unscaledTime;
            foreach (var road in Object.FindObjectsByType<RaceRoad>(FindObjectsSortMode.None))
            {
                if (!road || !road.isActiveAndEnabled || road.points == null || road.points.Length < 4) continue; road.Initialize(); if (road.Length < 20) continue;
                int n = Mathf.Max(2, Mathf.RoundToInt(road.Length / Step)); float step = road.Length / n;
                for (int i = 0; i <= n; i++)
                {
                    float s = Mathf.Min(i * step, road.Length - .01f); var p = road.At(s, out _); int index = pts.Count;
                    pts.Add(p); half.Add(road.HalfWidth(s) + 1f); dirt.Add(road.forestTrail);
                    long k = Key(Mathf.FloorToInt(p.x / Cell), Mathf.FloorToInt(p.z / Cell)); if (!grid.TryGetValue(k, out var l)) grid[k] = l = new List<int>(); l.Add(index);
                }
            }
        }
        public static Surface SurfaceAt(Vector3 p)
        {
            if (builtScene != SceneManager.GetActiveScene().name || Time.unscaledTime - builtAt > 40) Build();
            int cx = Mathf.FloorToInt(p.x / Cell), cz = Mathf.FloorToInt(p.z / Cell); float best = float.MaxValue; int found = -1;
            for (int x = cx - 1; x <= cx + 1; x++)
                for (int z = cz - 1; z <= cz + 1; z++)
                    if (grid.TryGetValue(Key(x, z), out var l))
                        foreach (int i in l)
                        {
                            var d = pts[i] - p; if (Mathf.Abs(d.y) > 6) continue; float h = Mathf.Sqrt(d.x * d.x + d.z * d.z);
                            // nodes are 6 m apart: up to 3 m of the distance can be along the road
                            if (h <= half[i] + 3f && h < best) { best = h; found = i; }
                        }
            return found < 0 ? Surface.Grass : dirt[found] ? Surface.Dirt : Surface.Paved;
        }
    }
}
