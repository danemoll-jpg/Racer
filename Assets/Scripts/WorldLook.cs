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
    // Everything a look sets is one named LookPreset; two presets interpolate field by field (LookPreset.Lerp), so
    // time-of-day and weather can be added later as more presets and a Free Roam day-night cycle can blend between them.
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
                colorFilter = C(a.colorFilter, b.colorFilter), waterSmoothness = F(a.waterSmoothness, b.waterSmoothness)
            };
        }
    }

    public static class LookPresets
    {
        // The only preset so far. Later presets (dusk, night, rain, snow) are added here with their own values.
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
        public static readonly string[] Names = { "Clear Day" };
        public static LookPreset Get(string name) => ClearDay;
    }

    public sealed class WorldLook : MonoBehaviour
    {
        public static bool Disabled { get; private set; }
        // Evidence switches (frame-rate cost of each part): -lookNoBloom, -lookNoHDR, -lookNoPost.
        static bool Arg(string a) => Array.IndexOf(Environment.GetCommandLineArgs(), a) >= 0;
        public static WorldLook Current { get; private set; }
        public LookPreset Preset { get; private set; }
        Light sun; Material sky; Volume volume; ColorAdjustments color; Bloom bloom; Tonemapping tone;
        float nextCameraCheck;

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
        void Awake() { Current = this; Apply(LookPresets.ClearDay); }
        void OnDestroy() { if (Current == this) Current = null; if (sky) Destroy(sky); if (volume) Destroy(volume.sharedProfile); foreach (var m in water.Values) if (m) Destroy(m); }

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
            DynamicGI.UpdateEnvironment();
            // Distance haze, matched to the horizon.
            RenderSettings.fog = true; RenderSettings.fogMode = FogMode.Linear; RenderSettings.fogColor = p.fogColor;
            RenderSettings.fogStartDistance = p.fogStart; RenderSettings.fogEndDistance = p.fogEnd;
            // Ground shader response (Racer/GreyboxGround and Racer/MarkedGround read these; 0 = authored look).
            Shader.SetGlobalFloat("_RacerLook", 1);
            Shader.SetGlobalFloat("_RacerSunBoost", p.sunBoost); Shader.SetGlobalFloat("_RacerAmbientScale", p.ambientScale);
            Shader.SetGlobalFloat("_RacerShadowLift", p.shadowLift); Shader.SetGlobalFloat("_RacerRoadSheen", p.roadSheen); Shader.SetGlobalFloat("_RacerGroundVariation", p.groundVariation);
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
            Cameras();
        }
        // Water (URP Lit "water" materials): a runtime copy with a smoother surface, and a sky-only reflection probe over each
        // lake rendered once, so lakes and ponds read as water. The authored materials are not changed.
        readonly Dictionary<Material, Material> water = new();
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
                    probe.RenderProbe();
                }
            }
            foreach (var copy in water.Values) copy.SetFloat("_Smoothness", p.waterSmoothness);
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
        static void Restore() => Shader.SetGlobalFloat("_RacerLook", 0);
    }
}
