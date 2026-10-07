using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace Racer
{
    // 0.71 lighting and atmosphere. One shared look, applied to every scene at load by code (no per-scene edits):
    // sun, ambient, sky, distance haze, ground-surface response, water and post-processing. The PC pipeline asset carries
    // the settings that cannot change at runtime without rebuilding the pipeline (HDR on, 80 m shadows, 4 cascades).
    // Screen-space ambient occlusion was tried and dropped: it cost ~1.4 ms per frame at 3840x2160 (GTX 1660 Ti), far over
    // the 10% budget (Docs/Report071/LOOK.md).
    // Everything a look sets is one named LookPreset; two presets interpolate field by field (LookPreset.Lerp).
    // 0.72 time of day and weather (visual and audio only): Day / Dusk / Night presets, Clear / Rain / Snow applied on top of
    // any of them. Races use the conditions chosen at race setup, fixed for the race; Free Roam runs a live day-night cycle
    // (FreeRoamHoursPerRealMinute) through the same presets; menus and the garage always show Clear Day.
    // 0.74: a Dawn preset (race option and the Free Roam early morning), the Free Roam clock saved and resumed with a 30-day
    // calendar (Day N), a visible moon with the day's phase (new on day 1, full around day 15; brighter nights near full),
    // a fixed full moon in races.
    // "-lookOff" on the command line leaves the scenes' authored lighting - used only for evidence.
    [Serializable]
    public sealed class LookPreset
    {
        public string name = "Clear Day";
        [Header("Sun")] public float sunElevation, sunAzimuth, sunIntensity, shadowStrength;
        public Color sunColor;
        [Header("Ambient (trilight)")] public Color ambientSky, ambientEquator, ambientGround; public float ambientIntensity;
        [Header("Sky (procedural)")] public Color skyTint, skyGround; public float skyExposure, atmosphere, sunSize;
        [Header("Distance haze (linear fog)")] public Color fogColor; public float fogStart, fogEnd;
        [Header("Ground surfaces (Racer ground shader)")] public float sunBoost, ambientScale, shadowLift, roadSheen, groundVariation;
        [Header("Water")] public float waterSmoothness;
        [Header("Post-processing")] public float postExposure, contrast, saturation, bloomIntensity, bloomThreshold;
        public Color colorFilter;
        // 0.72: vehicle lamps / course markings glow (0 day .. 1 night), star field, wet surfaces, snow cover, falling rain and snow.
        [Header("Night and weather")] public float lights, stars, wetness, snow, rain, snowfall;
        // 0.74: light ground mist in low areas (Dawn).
        public float mist;

        public static LookPreset Lerp(LookPreset a, LookPreset b, float t)
        {
            float F(float x, float y) => Mathf.Lerp(x, y, t);
            Color C(Color x, Color y) => Color.Lerp(x, y, t);
            return new LookPreset
            {
                name = t < .5f ? a.name : b.name,
                sunElevation = F(a.sunElevation, b.sunElevation), sunAzimuth = Mathf.LerpAngle(a.sunAzimuth, b.sunAzimuth, t),
                sunIntensity = F(a.sunIntensity, b.sunIntensity), shadowStrength = F(a.shadowStrength, b.shadowStrength),
                sunColor = C(a.sunColor, b.sunColor),
                ambientSky = C(a.ambientSky, b.ambientSky), ambientEquator = C(a.ambientEquator, b.ambientEquator), ambientGround = C(a.ambientGround, b.ambientGround), ambientIntensity = F(a.ambientIntensity, b.ambientIntensity),
                skyTint = C(a.skyTint, b.skyTint), skyGround = C(a.skyGround, b.skyGround), skyExposure = F(a.skyExposure, b.skyExposure), atmosphere = F(a.atmosphere, b.atmosphere), sunSize = F(a.sunSize, b.sunSize),
                fogColor = C(a.fogColor, b.fogColor), fogStart = F(a.fogStart, b.fogStart), fogEnd = F(a.fogEnd, b.fogEnd),
                sunBoost = F(a.sunBoost, b.sunBoost), ambientScale = F(a.ambientScale, b.ambientScale), shadowLift = F(a.shadowLift, b.shadowLift), roadSheen = F(a.roadSheen, b.roadSheen), groundVariation = F(a.groundVariation, b.groundVariation),
                postExposure = F(a.postExposure, b.postExposure), contrast = F(a.contrast, b.contrast), saturation = F(a.saturation, b.saturation),
                bloomIntensity = F(a.bloomIntensity, b.bloomIntensity), bloomThreshold = F(a.bloomThreshold, b.bloomThreshold),
                colorFilter = C(a.colorFilter, b.colorFilter), waterSmoothness = F(a.waterSmoothness, b.waterSmoothness),
                lights = F(a.lights, b.lights), stars = F(a.stars, b.stars), wetness = F(a.wetness, b.wetness), snow = F(a.snow, b.snow), rain = F(a.rain, b.rain), snowfall = F(a.snowfall, b.snowfall),
                mist = F(a.mist, b.mist)
            };
        }
        public LookPreset Copy() => Lerp(this, this, 0);
    }

    // Saved as ints: Day 0, Dusk 1, Night 2, Dawn 3 (appended in 0.74; the menus show Dawn / Day / Dusk / Night).
    public enum TimeOfDay { Day, Dusk, Night, Dawn }
    public enum Weather { Clear, Rain, Snow }

    public static class LookPresets
    {
        // Day is the accepted 0.71 "Clear Day", unchanged.
        public static LookPreset ClearDay => new LookPreset
        {
            name = "Clear Day",
            sunElevation = 47, sunAzimuth = 218, sunIntensity = 1.15f, shadowStrength = .9f,
            sunColor = new Color(1f, .95f, .86f),
            ambientSky = new Color(.66f, .74f, .86f), ambientEquator = new Color(.58f, .62f, .58f), ambientGround = new Color(.36f, .36f, .30f), ambientIntensity = 1,
            skyTint = new Color(.42f, .55f, .78f), skyGround = new Color(.64f, .71f, .78f), skyExposure = 1.15f, atmosphere = .85f, sunSize = .035f,
            fogColor = new Color(.69f, .77f, .86f), fogStart = 180, fogEnd = 1600,
            sunBoost = .7f, ambientScale = .88f, shadowLift = .15f, roadSheen = .18f, groundVariation = 1,
            postExposure = .1f, contrast = 12, saturation = 12, bloomIntensity = .2f, bloomThreshold = 1.05f, waterSmoothness = .92f,
            colorFilter = Color.white
        };
        // Warm low sun from the west-north-west, long shadows, amber haze; vehicle lamps on at half.
        public static LookPreset Dusk => new LookPreset
        {
            name = "Dusk",
            sunElevation = 9, sunAzimuth = 286, sunIntensity = .95f, shadowStrength = .8f,
            sunColor = new Color(1f, .64f, .40f),
            ambientSky = new Color(.46f, .46f, .60f), ambientEquator = new Color(.60f, .47f, .40f), ambientGround = new Color(.25f, .21f, .19f), ambientIntensity = .92f,
            skyTint = new Color(.50f, .48f, .60f), skyGround = new Color(.52f, .42f, .38f), skyExposure = 1.05f, atmosphere = 1.35f, sunSize = .05f,
            fogColor = new Color(.66f, .55f, .52f), fogStart = 150, fogEnd = 1350,
            sunBoost = .8f, ambientScale = .82f, shadowLift = .12f, roadSheen = .32f, groundVariation = 1,
            postExposure = .18f, contrast = 13, saturation = 10, bloomIntensity = .35f, bloomThreshold = 1f, waterSmoothness = .92f,
            colorFilter = new Color(1f, .95f, .89f), lights = .55f
        };
        // Moonlit: a pale blue moon (the main light; 0.74: drawn by WeatherEffects with its phase), dark blue sky and haze, stars, lamps on.
        // Kept bright enough to race: lifted exposure and ambient, headlights and glowing markings carry the road ahead.
        public static LookPreset Night => new LookPreset
        {
            name = "Night",
            sunElevation = 38, sunAzimuth = 140, sunIntensity = .36f, shadowStrength = .55f,
            sunColor = new Color(.62f, .73f, 1f),
            ambientSky = new Color(.24f, .30f, .48f), ambientEquator = new Color(.18f, .22f, .32f), ambientGround = new Color(.08f, .09f, .12f), ambientIntensity = 1,
            skyTint = new Color(.12f, .17f, .34f), skyGround = new Color(.06f, .08f, .13f), skyExposure = .28f, atmosphere = .6f, sunSize = 0,
            fogColor = new Color(.07f, .10f, .18f), fogStart = 90, fogEnd = 950,
            sunBoost = .7f, ambientScale = .62f, shadowLift = .1f, roadSheen = .25f, groundVariation = 1,
            postExposure = .55f, contrast = 8, saturation = -8, bloomIntensity = .55f, bloomThreshold = .9f, waterSmoothness = .95f,
            colorFilter = new Color(.86f, .91f, 1f), lights = 1, stars = 1
        };
        // 0.74 Dawn: low sun in the east (the opposite side of the sky from Dusk), cool pink-to-pale-gold light, bluish
        // shadows, a pale lavender-pink haze and light ground mist in the low areas; vehicle lamps on at half, as at Dusk.
        public static LookPreset Dawn => new LookPreset
        {
            name = "Dawn",
            sunElevation = 8, sunAzimuth = 96, sunIntensity = .9f, shadowStrength = .72f,
            sunColor = new Color(1f, .79f, .70f),
            ambientSky = new Color(.48f, .55f, .74f), ambientEquator = new Color(.55f, .52f, .62f), ambientGround = new Color(.21f, .22f, .27f), ambientIntensity = .92f,
            skyTint = new Color(.62f, .50f, .72f), skyGround = new Color(.70f, .56f, .63f), skyExposure = 1.0f, atmosphere = 1.1f, sunSize = .045f,
            fogColor = new Color(.77f, .67f, .74f), fogStart = 110, fogEnd = 1150,
            sunBoost = .75f, ambientScale = .84f, shadowLift = .14f, roadSheen = .26f, groundVariation = 1,
            postExposure = .16f, contrast = 11, saturation = 6, bloomIntensity = .3f, bloomThreshold = 1f, waterSmoothness = .93f,
            colorFilter = new Color(.97f, .96f, 1f), lights = .55f, mist = 1
        };
        public static LookPreset ForTime(TimeOfDay t) => t == TimeOfDay.Dusk ? Dusk : t == TimeOfDay.Night ? Night : t == TimeOfDay.Dawn ? Dawn : ClearDay;
        // Weather on top of any time of day. Brightness follows the base (night rain is dark grey, not daylight grey).
        public static LookPreset WithWeather(LookPreset b, Weather w)
        {
            if (w == Weather.Clear) return b;
            var p = b.Copy();
            float bright = Mathf.Clamp(b.fogColor.grayscale / .76f, .1f, 1.1f);
            if (w == Weather.Rain)
            {
                p.name = b.name + " Rain";
                p.sunIntensity *= .38f; p.shadowStrength *= .35f; p.sunSize *= .25f;
                var grey = new Color(.47f, .50f, .55f) * bright; grey.a = 1;
                p.skyTint = Color.Lerp(b.skyTint, grey, .85f); p.skyGround = Color.Lerp(b.skyGround, grey * .9f, .8f); p.skyExposure *= .85f; p.atmosphere *= .7f;
                p.ambientSky = Color.Lerp(b.ambientSky, grey * 1.15f, .5f); p.ambientEquator = Color.Lerp(b.ambientEquator, grey, .5f);
                p.fogColor = Color.Lerp(b.fogColor, grey * 1.05f, .8f); p.fogStart = Mathf.Min(b.fogStart, 55); p.fogEnd = Mathf.Min(b.fogEnd, 560);
                p.roadSheen = Mathf.Max(b.roadSheen, .6f); p.shadowLift = Mathf.Max(b.shadowLift, .3f);
                p.saturation -= 14; p.contrast -= 2; p.wetness = 1; p.rain = 1;
            }
            else
            {
                p.name = b.name + " Snow";
                p.sunIntensity *= .55f; p.shadowStrength *= .45f; p.sunSize *= .4f;
                var pale = new Color(.80f, .83f, .88f) * bright; pale.a = 1;
                p.skyTint = Color.Lerp(b.skyTint, pale, .8f); p.skyGround = Color.Lerp(b.skyGround, pale, .8f); p.atmosphere *= .7f;
                p.ambientSky = Color.Lerp(b.ambientSky, pale, .45f); p.ambientGround = Color.Lerp(b.ambientGround, pale * .8f, .5f);
                p.fogColor = Color.Lerp(b.fogColor, pale, .85f); p.fogStart = Mathf.Min(b.fogStart, 50); p.fogEnd = Mathf.Min(b.fogEnd, 520);
                p.shadowLift = Mathf.Max(b.shadowLift, .3f); p.saturation -= 8; p.snow = 1; p.snowfall = 1;
            }
            return p;
        }
        public static LookPreset Compose(TimeOfDay t, Weather w) => WithWeather(ForTime(t), w);
        public static readonly string[] Names = { "Clear Day", "Dusk", "Night", "Dawn" };
        public static LookPreset Get(string name) => name == "Dusk" ? Dusk : name == "Night" ? Night : name == "Dawn" ? Dawn : ClearDay;
        // Menu order of the race Time of Day option (saved values: Day 0, Dusk 1, Night 2, Dawn 3).
        public static readonly TimeOfDay[] MenuOrder = { TimeOfDay.Dawn, TimeOfDay.Day, TimeOfDay.Dusk, TimeOfDay.Night };
    }

    public sealed class WorldLook : MonoBehaviour
    {
        // Free Roam: game hours per real minute (1 = a full day in 24 minutes). Dan will tune this one value.
        public const float FreeRoamHoursPerRealMinute = 1f;
        public const float FreeRoamStartHour = 8f;

        public static bool Disabled { get; private set; }
        // Evidence switches (frame-rate cost of each part): -lookNoBloom, -lookNoHDR, -lookNoPost.
        static bool Arg(string a) => Array.IndexOf(Environment.GetCommandLineArgs(), a) >= 0;
        public static WorldLook Current { get; private set; }
        public LookPreset Preset { get; private set; }
        // What is showing now: "Menu" (Clear Day), "Race" (chosen conditions) or "Free Roam" (the live cycle).
        public string Mode { get; private set; } = "Menu";
        public TimeOfDay RaceTime { get; private set; }
        public Weather RaceWeather { get; private set; }
        public Weather RoamWeather { get; private set; }
        public float Hour { get; private set; } = FreeRoamStartHour;
        // 0.74: the Free Roam calendar (1-30, advancing at midnight) and the moon (0 new .. 0.5 full .. 1 new again).
        public const int CalendarDays = 30;
        public int Day { get; private set; } = 1;
        public float MoonPhase => Trailer != null && Mode == "Free Roam" ? Trailer.phase : Mode == "Free Roam" ? PhaseOf(Day, Hour) : .5f;// races: a fixed full moon
        // 0.77 Trailer Mode conditions (Free Roam only): while set, the live cycle shows this hour, weather and moon phase. The
        // saved Free Roam clock stands still and nothing is written to the settings; clearing it (Trailer Mode ends) leaves the
        // saved clock, calendar and weather exactly as they were.
        public sealed class TrailerConditions { public float hour; public Weather weather; public float phase; public bool paused; }
        public TrailerConditions Trailer { get; set; }
        // The hour the sky shows now (the Trailer Mode hour while one is set).
        public float LookHour => Trailer != null && Mode == "Free Roam" ? Trailer.hour : Hour;
        public static float PhaseOf(int day, float hour) => Mathf.Repeat((day - 1 + hour / 24f) / CalendarDays, 1);
        public static float Illumination(float phase) => (1 - Mathf.Cos(phase * 2 * Mathf.PI)) * .5f;
        public string Clock => $"{Mathf.FloorToInt(Hour) % 24:00}:{Mathf.FloorToInt(Hour * 60) % 60:00}";
        public string RoamClock => $"Day {Day} {Clock}";
        public string Conditions => Mode == "Free Roam" ? $"Free Roam {RoamClock} ({Preset?.name}) / {RoamWeather}" : Mode == "Race" ? $"{RaceTime} / {RaceWeather}" : "Clear Day (menus)";
        Light sun; Material sky; Volume volume; ColorAdjustments color; Bloom bloom; Tonemapping tone;
        float nextCameraCheck;
        RaceFlow flow; string session = "Menu"; string applied = ""; bool roamMenuSaved;
        public static event Action<LookPreset> Applied;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            Disabled = Array.IndexOf(Environment.GetCommandLineArgs(), "-lookOff") >= 0;
            SceneManager.sceneLoaded += (_, _) => Ensure();
            Application.quitting += Restore;
            Ensure();
        }
        static void Ensure()
        {
            if (Disabled || FindAnyObjectByType<WorldLook>()) return;
            if (!FindAnyObjectByType<RaceDirector>()) return;
            new GameObject("World look").AddComponent<WorldLook>();
        }
        void Awake() { Current = this; Apply(LookPresets.ClearDay); applied = "Menu"; if (!GetComponent<WeatherEffects>()) gameObject.AddComponent<WeatherEffects>(); }
        void OnDestroy() { if (session == "Free Roam") SaveClock(); if (Current == this) Current = null; if (sky) Destroy(sky); if (volume) Destroy(volume.sharedProfile); foreach (var m in water.Values) if (m) Destroy(m); foreach (var m in glow.Values) if (m) Destroy(m); foreach (var e in iceRenderers) foreach (var m in e.ice) if (m && m.name == "Frozen water (world look)") Destroy(m); }

        // ---------- 0.72 conditions: which look shows now ----------
        void Update()
        {
            if (!flow) flow = FindAnyObjectByType<RaceFlow>();
            if (!flow || flow.Save == null) return;
            var s = flow.Save.Settings;
            RaceTime = (TimeOfDay)Mathf.Clamp(s.timeOfDay, 0, 3); RaceWeather = (Weather)Mathf.Clamp(s.weather, 0, 2);
            var campaign = CampaignRun.Active; if (campaign != null) { RaceTime = campaign.Time; RaceWeather = campaign.Weather; } // 0.89: the event's conditions
            if (SplitScreen.Active) { RaceTime = SplitScreen.Time; RaceWeather = SplitScreen.Weather; } // 0.92 Part F: the setup screen's conditions (0.90: Day / Clear)
            // 0.94: the Free Roam weather setting again (0.92 left this assignment inside the comment above, so Free Roam was
            // always Clear)
            RoamWeather = (Weather)Mathf.Clamp(s.roamWeather, 0, 2);
            var stage = flow.State;
            bool live = stage == RaceFlow.Stage.Countdown || stage == RaceFlow.Stage.Racing;
            string was = session;
            // 0.94 Part B: split-screen Free Roam has the setup's fixed time and weather (its clock is not run or saved)
            if (live) { string next = flow.Race.FreeRoam && !SplitScreen.Active ? "Free Roam" : "Race"; if (next == "Free Roam" && session != "Free Roam") Resume(s); session = next; }
            else if (stage == RaceFlow.Stage.Ready && !flow.RoamMenu) session = "Menu";
            // "Return to menu" from the Free Roam pause menu leaves Free Roam even while that menu is still up.
            if (session == "Free Roam" && !flow.Race.FreeRoam) session = "Menu";
            // 0.74: Free Roam ended (a race, the menu, another course) - its clock is saved; it resumes from there next time.
            if (was == "Free Roam" && session != "Free Roam") SaveClock();
            // The Free Roam pause menu is the main menu (the session goes on behind it): its clock is saved when it opens.
            bool roamMenu = session == "Free Roam" && stage == RaceFlow.Stage.Ready && flow.RoamMenu;
            if (roamMenu && !roamMenuSaved) SaveClock();
            roamMenuSaved = roamMenu;
            Mode = stage == RaceFlow.Stage.Garage ? "Menu" : session;
            if (pinned != null) return;
            if (Mode == "Free Roam" && Trailer != null)
            {
                if (stage == RaceFlow.Stage.Racing && !Trailer.paused) Trailer.hour = Mathf.Repeat(Trailer.hour + Time.deltaTime / 60f * FreeRoamHoursPerRealMinute, 24);
                Apply(Cycle(Trailer.hour, Trailer.weather, Trailer.phase)); applied = "Trailer";
            }
            else if (Mode == "Free Roam")
            {
                if (stage == RaceFlow.Stage.Racing)
                {
                    float h = Hour + Time.deltaTime / 60f * FreeRoamHoursPerRealMinute;
                    if (h >= 24) Day = Day % CalendarDays + 1;// midnight: the next day (30 wraps to 1)
                    Hour = Mathf.Repeat(h, 24);
                }
                s.roamHour = Hour; s.roamDay = Day;// kept in the settings in memory; written to disk when Free Roam ends or the game quits
                Apply(Cycle(Hour, RoamWeather, MoonPhase)); applied = "Free Roam";
            }
            else
            {
                string key = Mode == "Race" ? $"Race {RaceTime} {RaceWeather}" : "Menu";
                if (key != applied) { Apply(Mode == "Race" ? LookPresets.Compose(RaceTime, RaceWeather) : LookPresets.ClearDay); applied = key; }
            }
        }
        // The saved Free Roam clock (a missing or unreadable value falls back to day 1, 08:00).
        void Resume(RacerSave.Options s)
        {
            Hour = float.IsNaN(s.roamHour) || s.roamHour < 0 || s.roamHour >= 24 ? FreeRoamStartHour : s.roamHour;
            Day = s.roamDay < 1 || s.roamDay > CalendarDays ? 1 : s.roamDay;
            s.roamHour = Hour; s.roamDay = Day;
        }
        void SaveClock()
        {
            if (!flow || flow.Save == null) return;
            flow.Save.Settings.roamHour = Hour; flow.Save.Settings.roamDay = Day; flow.Save.SaveSettings();
        }
        void OnApplicationQuit() { if (session == "Free Roam") SaveClock(); }
        // 0.76: leaving FreeRoamWorld for a course scene ends Free Roam; its clock is saved first.
        public void SaveRoamClock() { if (session == "Free Roam") SaveClock(); }
        // Evidence / checks: set the Free Roam calendar directly.
        public void SetClock(int day, float hour) { Day = Mathf.Clamp(day, 1, CalendarDays); Hour = Mathf.Repeat(hour, 24); }
        // Evidence only (ConditionsBench): show one preset until unpinned.
        LookPreset pinned;
        public void Pin(LookPreset p) { pinned = p; if (p != null) Apply(p); applied = ""; }
        // The Free Roam day: presets blended by hour (night -> dawn -> day -> dusk -> night); the sun and moon move continuously.
        // The main light follows the sun while it is up and the moon otherwise; it fades to nothing at each horizon crossing,
        // so the switch never pops a shadow.
        // 0.74: the early morning passes through the Dawn preset (3), the evening through Dusk (1).
        static readonly (float hour, int preset)[] keys = { (0, 2), (4.6f, 2), (6f, 3), (7.6f, 0), (17.8f, 0), (19.4f, 1), (20.8f, 2), (24, 2) };
        // The sun is up 06:00-20:00 (rising ENE, setting WNW, highest 58 degrees); the moon has the night (highest ~36 degrees).
        public static (float elevation, float azimuth, bool sun) Sky(float hour)
        {
            if (hour >= 6 && hour < 20) return (58 * Mathf.Sin(Mathf.PI * (hour - 6) / 14), 75 + 210 * (hour - 6) / 14, true);
            float n = Mathf.Repeat(hour - 20, 24);
            return (36 * Mathf.Sin(Mathf.PI * n / 10), 105 + 150 * n / 10, false);
        }
        // 0.74 moon: it rises in the east about six hours before it is highest and sets six hours after; highest at noon on
        // day 1 (new, unseen), at midnight around day 15 (full). Returns elevation (negative = below the horizon) and azimuth.
        public static (float elevation, float azimuth) Moon(float hour, float phase)
        {
            float transit = Mathf.Repeat(12 + phase * 24, 24);
            float d = Mathf.Repeat(hour - transit + 12, 24) - 12;// hours from the moon's highest point, -12..12
            return (52 * Mathf.Cos(d / 24f * 2 * Mathf.PI) - 2, 180 + Mathf.Clamp(d / 6f, -1.4f, 1.4f) * 90);
        }
        public static LookPreset Cycle(float hour, Weather weather) => Cycle(hour, weather, .5f);
        public static LookPreset Cycle(float hour, Weather weather, float phase)
        {
            LookPreset P(int i) => i == 0 ? LookPresets.ClearDay : i == 1 ? LookPresets.Dusk : i == 3 ? LookPresets.Dawn : LookPresets.Night;
            int k = 0; while (k < keys.Length - 2 && hour >= keys[k + 1].hour) k++;
            float t = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(keys[k].hour, keys[k + 1].hour, hour));
            var p = LookPreset.Lerp(P(keys[k].preset), P(keys[k + 1].preset), t);
            var (elev, az, isSun) = Sky(hour);
            float fade;
            if (isSun)
            {
                p.sunElevation = Mathf.Max(elev, 1); p.sunAzimuth = az;
                // Off within 2 degrees of the horizon, so the switch between sun and moon never shows.
                fade = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(2, 9, elev));
            }
            else
            {
                // Night light: the moon while it is up (brighter near full), otherwise a dim sky glow from high in the south;
                // both fade out at 6 degrees, so the change of direction never shows. "elev" is the old night arc, kept only
                // for the hand-over from and to the sun (it starts and ends at the horizon at 20:00 and 06:00).
                var (me, ma) = Moon(hour, phase); float lit = Illumination(phase);
                float handover = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(2, 9, elev));
                if (me >= 6) { p.sunElevation = me; p.sunAzimuth = ma; fade = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(6, 14, me)) * Mathf.Lerp(.55f, 1.12f, lit); }
                else { p.sunElevation = 62; p.sunAzimuth = 180; fade = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(6, 0, me)) * .5f; }
                fade *= handover;
                // Moonlit nights a little brighter near full, darker near new, always playable.
                float bright = Mathf.Lerp(.86f, 1.06f, lit) * p.stars + (1 - p.stars);
                p.ambientIntensity *= bright; p.postExposure += Mathf.Lerp(-.06f, .04f, lit) * p.stars;
                p.sunSize = 0;// the moon is drawn by WeatherEffects with its phase, not as the sky's sun disc
            }
            p.sunIntensity *= fade; p.shadowStrength *= fade;
            p.name = hour < 4.6f || hour >= 20.8f ? "Night" : hour < 7.6f ? "Dawn" : hour < 17.8f ? "Day" : "Dusk";
            return LookPresets.WithWeather(p, weather);
        }

        public void Apply(LookPreset p)
        {
            Preset = p;
            // Sun: the scene's main directional light (created if a scene has none).
            if (!sun)
            {
                foreach (var l in FindObjectsByType<Light>(FindObjectsSortMode.None))
                    if (l.type == LightType.Directional && l.enabled && (!sun || l.intensity > sun.intensity)) sun = l;
                if (!sun) { sun = new GameObject("Sun").AddComponent<Light>(); sun.type = LightType.Directional; }
            }
            sun.transform.rotation = Quaternion.Euler(p.sunElevation, p.sunAzimuth, 0);
            sun.color = p.sunColor; sun.intensity = p.sunIntensity; sun.shadows = LightShadows.Soft; sun.shadowStrength = p.shadowStrength;
            RenderSettings.sun = sun;
            // Ambient: sky / horizon / ground, so shaded sides keep colour.
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = p.ambientSky * p.ambientIntensity; RenderSettings.ambientEquatorColor = p.ambientEquator * p.ambientIntensity; RenderSettings.ambientGroundColor = p.ambientGround * p.ambientIntensity;
            // Sky: the procedural sky; below the horizon it takes the haze colour, so the world's edge disappears into it.
            if (!sky && RenderSettings.skybox && RenderSettings.skybox.HasProperty("_SkyTint")) sky = new Material(RenderSettings.skybox) { name = "World look sky" };
            if (sky)
            {
                sky.SetColor("_SkyTint", p.skyTint); sky.SetColor("_GroundColor", p.skyGround); sky.SetFloat("_Exposure", p.skyExposure);
                sky.SetFloat("_AtmosphereThickness", p.atmosphere); sky.SetFloat("_SunSize", p.sunSize);
                RenderSettings.skybox = sky;
            }
            // The environment (ambient probe) is refreshed only when a preset changes noticeably, not every cycle frame.
            if (environmentKey < 0 || Mathf.Abs(p.skyExposure - environmentKey) > .01f || Mathf.Abs(p.skyTint.grayscale - environmentTint) > .01f)
            { DynamicGI.UpdateEnvironment(); environmentKey = p.skyExposure; environmentTint = p.skyTint.grayscale; }
            // Distance haze, matched to the horizon.
            RenderSettings.fog = true; RenderSettings.fogMode = FogMode.Linear; RenderSettings.fogColor = p.fogColor;
            RenderSettings.fogStartDistance = p.fogStart; RenderSettings.fogEndDistance = p.fogEnd;
            // Ground shader response (Racer/GreyboxGround and Racer/MarkedGround read these; 0 = authored look).
            Shader.SetGlobalFloat("_RacerLook", 1);
            Shader.SetGlobalFloat("_RacerSunBoost", p.sunBoost); Shader.SetGlobalFloat("_RacerAmbientScale", p.ambientScale);
            Shader.SetGlobalFloat("_RacerShadowLift", p.shadowLift); Shader.SetGlobalFloat("_RacerRoadSheen", p.roadSheen); Shader.SetGlobalFloat("_RacerGroundVariation", p.groundVariation);
            Shader.SetGlobalFloat("_RacerWet", p.wetness); Shader.SetGlobalFloat("_RacerSnow", p.snow);
            // Post-processing: neutral tonemapping, gentle grade, mild bloom. Nothing else (no blur, grain, vignette...).
            if (!volume)
            {
                volume = new GameObject("World look volume").AddComponent<Volume>(); volume.transform.SetParent(transform, false);
                volume.isGlobal = true; volume.priority = 100;
                var profile = ScriptableObject.CreateInstance<VolumeProfile>(); profile.name = "World look";
                tone = profile.Add<Tonemapping>(true); color = profile.Add<ColorAdjustments>(true); bloom = profile.Add<Bloom>(true);
                volume.sharedProfile = profile;
            }
            tone.mode.Override(TonemappingMode.Neutral);
            color.postExposure.Override(p.postExposure); color.contrast.Override(p.contrast); color.saturation.Override(p.saturation); color.colorFilter.Override(p.colorFilter);
            bloom.intensity.Override(p.bloomIntensity); bloom.threshold.Override(p.bloomThreshold); bloom.scatter.Override(.6f); bloom.highQualityFiltering.Override(false); bloom.active = !Arg("-lookNoBloom");
            Water(p);
            ShallowWater.SetFrozen(p.snow > .5f); Ice();
            Glow(p);
            Cameras();
            Applied?.Invoke(p);
        }
        float environmentKey = -1, environmentTint;
        // 0.73 lightning (WeatherEffects): a brief brightening of the sky and the whole image on top of the current preset;
        // 0 puts the preset back exactly.
        public void Lightning(float f)
        {
            if (Preset == null || !color) return;
            color.postExposure.Override(Preset.postExposure + f * 1.3f);
            if (sky) sky.SetFloat("_Exposure", Preset.skyExposure * (1 + f * 2.5f));
        }
        // Water (URP Lit "water" materials): a runtime copy with a smoother surface, and a sky-only reflection probe over each
        // lake rendered once, so lakes and ponds read as water. The authored materials are not changed.
        readonly Dictionary<Material, Material> water = new();
        readonly List<ReflectionProbe> probes = new();
        float probeExposure = -1;
        void Water(LookPreset p)
        {
            if (water.Count == 0)
            {
                foreach (var r in FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None))
                {
                    var mats = r.sharedMaterials; bool any = false;
                    for (int i = 0; i < mats.Length; i++)
                    {
                        var m = mats[i]; if (!m || m.name.IndexOf("water", StringComparison.OrdinalIgnoreCase) < 0 || !m.HasProperty("_Smoothness") || !m.shader.name.StartsWith("Universal Render Pipeline/Lit")) continue;
                        if (!water.TryGetValue(m, out var copy)) water[m] = copy = new Material(m) { name = m.name + " (world look)" };
                        mats[i] = copy; any = true;
                    }
                    if (!any) continue;
                    r.sharedMaterials = mats;
                    var b = r.bounds; if (b.size.x < 15 && b.size.z < 15) continue;
                    var probe = new GameObject("World look water reflection").AddComponent<ReflectionProbe>(); probe.transform.SetParent(transform, false);
                    probe.transform.position = b.center; probe.size = b.size + new Vector3(10, 40, 10); probe.mode = ReflectionProbeMode.Realtime;
                    probe.refreshMode = ReflectionProbeRefreshMode.ViaScripting; probe.cullingMask = 0; probe.resolution = 128; probe.clearFlags = ReflectionProbeClearFlags.Skybox; probe.importance = 2;
                    probes.Add(probe);
                }
            }
            foreach (var copy in water.Values) copy.SetFloat("_Smoothness", p.waterSmoothness);
            // The sky reflection is re-rendered when the sky changes (a race's conditions, or every few game minutes of the cycle).
            if (probeExposure < 0 || Mathf.Abs(probeExposure - p.skyExposure) > .04f) { foreach (var probe in probes) if (probe) probe.RenderProbe(); probeExposure = p.skyExposure; }
        }
        // 0.73 Snow: every water surface (lakes, creeks, pools, the storm-drain flow) shows as ice - pale blue-white, matte
        // with a soft sheen, a light dusting of snow, no animation and no sky reflection. Each water renderer gets its own
        // ice copy (the dusting tiles about every 6 m); the unfrozen materials are put back for Clear and Rain.
        readonly List<(Renderer r, Material[] water, Material[] ice)> iceRenderers = new();
        bool iceScanned, iceShown;
        static bool IsWater(Material m) => m && (m.name.IndexOf("water", StringComparison.OrdinalIgnoreCase) >= 0 || m.name.StartsWith("Lake blue green") || m.name.StartsWith("Pool turquoise") || m.shader && m.shader.name == "Racer/ShallowDrainWater");
        void Ice()
        {
            bool frozen = ShallowWater.Frozen;
            if (frozen && !iceScanned)
            {
                iceScanned = true;
                var template = Resources.Load<Material>("WaterIce");
                if (template)
                    foreach (var r in FindObjectsByType<MeshRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                    {
                        var mats = r.sharedMaterials; if (!r.GetComponent<ShallowWater>() && !Array.Exists(mats, IsWater)) continue;
                        var m = new Material(template) { name = "Frozen water (world look)" };
                        if (r.GetComponent<MeshFilter>()?.sharedMesh?.name is "Cube" or "Cylinder") m.SetTextureScale("_BaseMap", new Vector2(Mathf.Max(1, Mathf.Round(r.transform.lossyScale.x / 6)), Mathf.Max(1, Mathf.Round(r.transform.lossyScale.z / 6))));
                        bool all = r.GetComponent<ShallowWater>(); var frozenMats = new Material[mats.Length];
                        for (int k = 0; k < mats.Length; k++) frozenMats[k] = all || IsWater(mats[k]) ? m : mats[k];
                        iceRenderers.Add((r, mats, frozenMats));
                    }
            }
            if (frozen == iceShown) return;
            iceShown = frozen;
            foreach (var (r, w, i) in iceRenderers) if (r) r.sharedMaterials = frozen ? i : w;
        }

        // 0.72 night readability: the course arrows and checkpoint gate markings glow (their own colour, emitted) as it gets
        // dark, so they read at night under every weather. Day = no emission, exactly the 0.71 look. Runtime copies only.
        readonly Dictionary<Material, Material> glow = new();
        bool glowScanned;
        void Glow(LookPreset p)
        {
            if (!glowScanned)
            {
                glowScanned = true;
                var race = FindAnyObjectByType<RaceDirector>();
                var marked = new HashSet<Renderer>();
                foreach (var r in FindObjectsByType<MeshRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                    if (r.name.IndexOf("arrow", StringComparison.OrdinalIgnoreCase) >= 0) marked.Add(r);
                if (race && race.gates != null) foreach (var g in race.gates) if (g) foreach (var r in g.GetComponentsInChildren<MeshRenderer>(true)) marked.Add(r);
                foreach (var r in marked)
                {
                    var mats = r.sharedMaterials; bool any = false;
                    for (int i = 0; i < mats.Length; i++)
                    {
                        var m = mats[i]; if (!m || !m.HasProperty("_EmissionColor")) continue;
                        if (!glow.TryGetValue(m, out var copy)) glow[m] = copy = new Material(m) { name = m.name + " (night glow)" };
                        mats[i] = copy; any = true;
                    }
                    if (any) r.sharedMaterials = mats;
                }
            }
            foreach (var kv in glow)
            {
                var c = kv.Key.HasProperty("_BaseColor") ? kv.Key.GetColor("_BaseColor") : kv.Key.color;
                if (p.lights > .01f) { kv.Value.EnableKeyword("_EMISSION"); kv.Value.SetColor("_EmissionColor", c * (1.8f * p.lights)); kv.Value.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None; }
                else { kv.Value.DisableKeyword("_EMISSION"); kv.Value.SetColor("_EmissionColor", Color.black); }
            }
        }
        // Every camera (race, garage, menus) renders post-processing.
        void Cameras()
        {
            foreach (var c in Camera.allCameras)
            {
                var data = c.GetUniversalAdditionalCameraData();
                if (data && c.targetTexture == null) data.renderPostProcessing = !Arg("-lookNoPost");
                if (Arg("-lookNoHDR")) c.allowHDR = false;
            }
        }
        void LateUpdate() { if (Time.unscaledTime >= nextCameraCheck) { nextCameraCheck = Time.unscaledTime + 1; Cameras(); } }

        // Editor play mode: clear the shader globals so edit-mode views show the authored look.
        static void Restore() { Shader.SetGlobalFloat("_RacerLook", 0); Shader.SetGlobalFloat("_RacerWet", 0); Shader.SetGlobalFloat("_RacerSnow", 0); }
    }
}
