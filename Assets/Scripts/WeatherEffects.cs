using UnityEngine;

namespace Racer
{
    // 0.72 weather and night sky, driven by the current WorldLook preset (visual and audio only):
    // falling rain / snow in a box around the camera (world-space particles, so they stream past at speed), cut off under
    // cover (caves, tunnels, underpasses: something solid overhead), a rain ambience loop on the Ambience volume, and a
    // star field that fades in at night. Vehicle lamps are set here too (VehicleLights on every vehicle in the scene).
    // 0.73: thunderstorms in Rain - an occasional strike (one every 20-60 s, irregular): one or two short flashes that light
    // the sky, the clouds and the world (strongest at night, subtle by day; never more than two pulses, ~0.1 s each), then
    // thunder after a short varying delay on the Ambience volume. Settings > Display "Lightning flashes: Off" keeps the
    // thunder and removes the flash. Under cover: no flash, thunder muffled. No gameplay effect. The clouds live here too.
    public sealed class WeatherEffects : MonoBehaviour
    {
        ParticleSystem rain, snow, stars; ParticleSystemRenderer starRenderer;
        AudioSource rainAudio, thunderAudio; AudioLowPassFilter thunderFilter; AudioClip[] thunderClips;
        float nextStrike = -1, strikeAt = -100, thunderAt = -1, strikeStrength, thunderDistance; int pulses; bool strikeFlashes;
        public int Strikes { get; private set; }
        // Evidence only (ConditionsBench): no new strikes while fixed-view screenshots and frame times are taken.
        public static bool HoldStrikes;
        public int ThunderPlayed { get; private set; }
        public float FlashLevel { get; private set; }
        public float PeakFlash { get; private set; }
        public bool LastStrikeCovered { get; private set; }
        RaceFlow flow;
        float rainLevel, snowLevel, starLevel, lightsLevel, nextVehicleScan, nextCoverCheck;
        bool covered;
        static Material rainMat, snowMat, starMat;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            WorldLook.Applied -= Attach; WorldLook.Applied += Attach;
        }
        static void Attach(LookPreset p)
        {
            var look = WorldLook.Current; if (look && !look.GetComponent<WeatherEffects>()) look.gameObject.AddComponent<WeatherEffects>();
        }
        void Awake()
        {
            // Materials live in Resources so their shaders are in the build (Shader.Find alone would not include them).
            if (!rainMat) { var a = Resources.Load<Material>("WeatherRain"); rainMat = a ? new Material(a) : Particle(new Color(.78f, .82f, .90f, .55f)); }
            if (!snowMat) { var a = Resources.Load<Material>("WeatherSnow"); snowMat = a ? new Material(a) : Particle(new Color(1, 1, 1, .9f)); }
            // A soft round dot (an untextured particle is a square, which reads as a white block close to the camera).
            var dot = Dot(); foreach (var m in new[] { rainMat, snowMat }) if (m && m.HasProperty("_BaseMap") && !m.GetTexture("_BaseMap")) m.SetTexture("_BaseMap", dot);
            rain = Make("Rain", rainMat, true);
            snow = Make("Snow", snowMat, false);
            stars = Stars();
            rainAudio = gameObject.AddComponent<AudioSource>(); rainAudio.clip = RainClip(); rainAudio.loop = true; rainAudio.spatialBlend = 0; rainAudio.volume = 0; rainAudio.playOnAwake = false; rainAudio.priority = 170;
            var thunder = new GameObject("Thunder"); thunder.transform.SetParent(transform, false);
            thunderAudio = thunder.AddComponent<AudioSource>(); thunderAudio.spatialBlend = 0; thunderAudio.playOnAwake = false; thunderAudio.priority = 160;
            thunderFilter = thunder.AddComponent<AudioLowPassFilter>(); thunderFilter.cutoffFrequency = 5000;
            thunderClips = new[] { ThunderClip(7311), ThunderClip(7312), ThunderClip(7313) };
            if (!GetComponentInChildren<SkyClouds>()) { var sky = new GameObject("Sky clouds"); sky.transform.SetParent(transform, false); sky.AddComponent<SkyClouds>(); }
        }
        static Material Particle(Color c)
        {
            var shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            var m = new Material(shader) { name = "Weather particle" };
            m.SetFloat("_Surface", 1); m.SetFloat("_Blend", 0); m.SetColor("_BaseColor", c);
            m.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha); m.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha); m.SetFloat("_ZWrite", 0);
            m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT"); m.renderQueue = 3050;
            return m;
        }
        ParticleSystem Make(string name, Material mat, bool isRain)
        {
            var go = new GameObject(name); go.transform.SetParent(transform, false);
            var ps = go.AddComponent<ParticleSystem>(); ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = ps.main; main.loop = true; main.simulationSpace = ParticleSystemSimulationSpace.World; main.playOnAwake = false;
            main.cullingMode = ParticleSystemCullingMode.AlwaysSimulate;// an empty system has no visible bounds; "automatic" culling would never let it start
            main.maxParticles = isRain ? 6000 : 5000; main.startSpeed = 0; main.gravityModifier = 0;
            main.startLifetime = isRain ? 1.25f : 7f; main.startSize = isRain ? .03f : .09f; main.startColor = Color.white;
            var shape = ps.shape; shape.shapeType = ParticleSystemShapeType.Box; shape.scale = isRain ? new Vector3(56, 1, 56) : new Vector3(64, 1, 64);
            var vel = ps.velocityOverLifetime; vel.enabled = true; vel.space = ParticleSystemSimulationSpace.World;
            vel.x = isRain ? new ParticleSystem.MinMaxCurve(-.6f, .6f) : new ParticleSystem.MinMaxCurve(-.7f, .7f);
            vel.y = isRain ? new ParticleSystem.MinMaxCurve(-24f, -20f) : new ParticleSystem.MinMaxCurve(-2.6f, -1.8f);
            vel.z = isRain ? new ParticleSystem.MinMaxCurve(-.6f, .6f) : new ParticleSystem.MinMaxCurve(-.7f, .7f);
            if (!isRain) { var noise = ps.noise; noise.enabled = true; noise.strength = .6f; noise.frequency = .35f; noise.scrollSpeed = .2f; noise.quality = ParticleSystemNoiseQuality.Low; }
            var em = ps.emission; em.rateOverTime = 0;
            var r = go.GetComponent<ParticleSystemRenderer>(); r.sharedMaterial = mat; r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; r.receiveShadows = false;
            if (isRain) { r.renderMode = ParticleSystemRenderMode.Stretch; r.velocityScale = .028f; r.lengthScale = 1; } else r.renderMode = ParticleSystemRenderMode.Billboard;
            // Never larger than a small fraction of the screen, however close to the camera (no blocks or bars in view).
            r.maxParticleSize = isRain ? .006f : .012f;
            return ps;
        }
        // Stars: fixed points on a large dome around the camera, drawn with a fog-free additive shader so they show at night.
        ParticleSystem Stars()
        {
            var go = new GameObject("Night sky stars"); go.transform.SetParent(transform, false);
            var ps = go.AddComponent<ParticleSystem>(); ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = ps.main; main.loop = false; main.playOnAwake = false; main.simulationSpace = ParticleSystemSimulationSpace.Local; main.maxParticles = 1400; main.startLifetime = 1e6f; main.startSpeed = 0;
            var em = ps.emission; em.enabled = false;
            var r = go.GetComponent<ParticleSystemRenderer>(); r.renderMode = ParticleSystemRenderMode.Billboard; r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; r.receiveShadows = false;
            if (!starMat) { var asset = Resources.Load<Material>("NightStars"); starMat = asset ? new Material(asset) { name = "Night stars" } : Particle(Color.white); }
            r.sharedMaterial = starMat;
            var rng = new System.Random(72); var parts = new ParticleSystem.Particle[1400];
            for (int i = 0; i < parts.Length; i++)
            {
                float u = (float)rng.NextDouble(), v = (float)rng.NextDouble();
                float y = Mathf.Lerp(.06f, 1, Mathf.Sqrt(u)); float a = v * Mathf.PI * 2, rr = Mathf.Sqrt(1 - y * y);
                parts[i].position = new Vector3(Mathf.Cos(a) * rr, y, Mathf.Sin(a) * rr) * 600;
                float b = (float)rng.NextDouble(); parts[i].startSize = Mathf.Lerp(1.1f, 3f, b * b);
                parts[i].startColor = Color.Lerp(new Color(.8f, .86f, 1f), new Color(1f, .95f, .85f), (float)rng.NextDouble()) * Mathf.Lerp(.5f, 1, b);
                parts[i].remainingLifetime = parts[i].startLifetime = 1e6f;
            }
            ps.SetParticles(parts, parts.Length); ps.Play(); r.enabled = false;
            return ps;
        }
        void LateUpdate()
        {
            var look = WorldLook.Current; if (!look || look.Preset == null) return;
            var p = look.Preset; var cam = Camera.main; if (!cam) return;
            if (!flow) flow = FindAnyObjectByType<RaceFlow>();
            // Under cover: something solid within 45 m straight above the camera.
            if (Time.unscaledTime >= nextCoverCheck)
            {
                nextCoverCheck = Time.unscaledTime + .2f;
                var at = cam.transform.position;
                bool now = Physics.Raycast(at + Vector3.up * .5f, Vector3.up, 45, ~0, QueryTriggerInteraction.Ignore) || UnderCaveMesh(at);
                if (now && !covered) { rain.Clear(); snow.Clear(); }
                covered = now;
            }
            float shelter = covered ? 0 : 1;
            rainLevel = Mathf.MoveTowards(rainLevel, p.rain * shelter, Time.unscaledDeltaTime * 2);
            snowLevel = Mathf.MoveTowards(snowLevel, p.snowfall * shelter, Time.unscaledDeltaTime * 2);
            var pos = cam.transform.position; var fwd = Vector3.ProjectOnPlane(cam.transform.forward, Vector3.up).normalized;
            // The emitter box sits above and a little ahead of the camera, so the road ahead has falling weather at speed.
            rain.transform.position = pos + fwd * 10 + Vector3.up * 17; snow.transform.position = pos + fwd * 12 + Vector3.up * 13;
            Rate(rain, rainLevel * 3600); Rate(snow, snowLevel * 1700);
            // Weather turned off: what is still falling goes too (no snow left in the air of a clear race).
            if (p.rain <= 0 && rainLevel <= 0 && rain.particleCount > 0) rain.Clear();
            if (p.snowfall <= 0 && snowLevel <= 0 && snow.particleCount > 0) snow.Clear();
            // Stars follow the camera (a sky dome); visible only when dark and not overcast.
            starLevel = p.stars * (1 - Mathf.Max(p.rain, p.snowfall));
            starRenderer ??= stars.GetComponent<ParticleSystemRenderer>(); starRenderer.enabled = starLevel > .02f; stars.transform.position = pos;
            if (starMat && starMat.HasProperty("_Visibility")) starMat.SetFloat("_Visibility", starLevel);
            // Rain ambience on the Ambience volume, muffled under cover.
            float volume = flow && flow.Save != null ? flow.Save.Settings.ambience : 1;
            float targetAudio = p.rain * (covered ? .35f : 1) * .55f * volume;
            if (targetAudio > .001f && !rainAudio.isPlaying) rainAudio.Play();
            rainAudio.volume = Mathf.MoveTowards(rainAudio.volume, targetAudio, Time.unscaledDeltaTime * .5f);
            if (rainAudio.volume <= .001f && targetAudio <= .001f && rainAudio.isPlaying) rainAudio.Stop();
            Storm(p, look);
            // Vehicle lamps: every vehicle (player, rivals, traffic) gets its lamps once; their level follows the preset.
            if (Time.unscaledTime >= nextVehicleScan) { nextVehicleScan = Time.unscaledTime + 1; foreach (var v in FindObjectsByType<ArcadeVehicle>(FindObjectsSortMode.None)) if (!v.GetComponent<VehicleLights>()) v.gameObject.AddComponent<VehicleLights>(); }
            lightsLevel = Mathf.MoveTowards(lightsLevel, p.lights, Time.unscaledDeltaTime);
            VehicleLights.Level = lightsLevel;
        }
        // ---------- 0.73 lightning and thunder ----------
        void Storm(LookPreset p, WorldLook look)
        {
            bool storm = p.rain > .5f && look.Mode != "Menu";
            float now = Time.time;// scaled: nothing new while paused
            if (!storm) { nextStrike = -1; thunderAt = -1; if (FlashLevel > 0) Flash(0, look); if (thunderAudio.isPlaying && p.rain <= .5f) thunderAudio.Stop(); return; }
            if (nextStrike < 0 || HoldStrikes && nextStrike < now + 5) nextStrike = now + Random.Range(8f, 25f);
            if (now >= nextStrike)
            {
                Strikes++; strikeAt = now; nextStrike = now + Random.Range(20f, 60f);
                pulses = Random.value < .55f ? 2 : 1; thunderDistance = Random.value;
                // Strongest at night, a little less at dusk, subtle in daylight; nearer strikes are brighter.
                strikeStrength = Mathf.Lerp(.32f, 1f, Mathf.Clamp01(p.lights)) * Mathf.Lerp(1.1f, .65f, thunderDistance);
                LastStrikeCovered = covered; strikeFlashes = !covered && (flow == null || flow.Save == null || flow.Save.Settings.lightningFlashes);
                thunderAt = now + Mathf.Lerp(.8f, 4.2f, thunderDistance) + Random.Range(0f, .5f);
            }
            // Flash envelope: pulse 1 at 0-0.11 s, pulse 2 (weaker) at 0.22-0.32 s; fast rise, quick fall.
            float age = now - strikeAt, f = 0;
            if (strikeFlashes && age >= 0 && age < .4f)
            {
                f = Pulse(age, 0, .11f);
                if (pulses > 1) f = Mathf.Max(f, .7f * Pulse(age, .22f, .1f));
                f *= strikeStrength;
            }
            if (f > 0 || FlashLevel > 0) Flash(f, look);
            PeakFlash = Mathf.Max(PeakFlash, f);
            if (thunderAt > 0 && now >= thunderAt)
            {
                thunderAt = -1; ThunderPlayed++;
                float volume = flow && flow.Save != null ? flow.Save.Settings.ambience : 1;
                thunderAudio.clip = thunderClips[Random.Range(0, thunderClips.Length)];
                thunderAudio.volume = Mathf.Lerp(.95f, .55f, thunderDistance) * (covered ? .45f : 1) * volume;
                thunderFilter.cutoffFrequency = covered ? 520 : Mathf.Lerp(5200, 1100, thunderDistance);
                thunderAudio.pitch = Random.Range(.92f, 1.06f); thunderAudio.Play();
            }
        }
        static float Pulse(float age, float start, float length) { float x = (age - start) / length; return x < 0 || x > 1 ? 0 : x < .15f ? x / .15f : Mathf.Pow(1 - (x - .15f) / .85f, 2); }
        void Flash(float f, WorldLook look) { FlashLevel = f; SkyClouds.Flash = f; look.Lightning(f); }
        // Thunder made at start-up (no audio asset): a dull rumble of filtered noise in a few rolling swells, with a short
        // crack at the front of some.
        static AudioClip ThunderClip(int seed)
        {
            const int rate = 22050; float seconds = 6.5f; var data = new float[(int)(rate * seconds)]; var rng = new System.Random(seed);
            float brown = 0, lp = 0, lp2 = 0, swell = 0, swellTarget = 1; float peak = 0; bool crack = seed % 2 == 1;
            for (int i = 0; i < data.Length; i++)
            {
                float t = i / (float)rate, n = (float)rng.NextDouble() * 2 - 1;
                brown = brown * .995f + n * .06f; lp += (brown - lp) * .08f; lp2 += (lp - lp2) * .12f;
                if (i % 2200 == 0) swellTarget = .35f + (float)rng.NextDouble() * .9f; swell += (swellTarget - swell) * .0009f;
                float env = Mathf.Clamp01(t / .09f) * Mathf.Exp(-t / 2.1f) * swell;
                float v = lp2 * 6f * env;
                if (crack && t < .35f) v += n * .5f * Mathf.Exp(-t / .06f);
                data[i] = v; peak = Mathf.Max(peak, Mathf.Abs(v));
            }
            if (peak > 0) for (int i = 0; i < data.Length; i++) data[i] *= .9f / peak;
            int fade = rate / 2; for (int i = 0; i < fade; i++) data[data.Length - 1 - i] *= i / (float)fade;
            var clip = AudioClip.Create("Thunder " + seed, data.Length, 1, rate, false); clip.SetData(data, 0); return clip;
        }
        void OnDestroy() { if (FlashLevel > 0) SkyClouds.Flash = 0; if (thunderClips != null) foreach (var c in thunderClips) if (c) Destroy(c); }
        static Texture2D dotTexture;
        static Texture2D Dot()
        {
            if (dotTexture) return dotTexture;
            const int n = 32; dotTexture = new Texture2D(n, n, TextureFormat.RGBA32, false) { name = "Weather dot", wrapMode = TextureWrapMode.Clamp };
            for (int y = 0; y < n; y++) for (int x = 0; x < n; x++) { float dx = (x + .5f) / n * 2 - 1, dy = (y + .5f) / n * 2 - 1; float a = Mathf.Clamp01(1 - Mathf.Sqrt(dx * dx + dy * dy)); dotTexture.SetPixel(x, y, new Color(1, 1, 1, a * a)); }
            dotTexture.Apply(); return dotTexture;
        }
        // Cave and tunnel roofs are often visual meshes without colliders: their triangles are tested directly (no physics,
        // so nothing in the game can collide with or sense them). Candidates: cave / tunnel / canopy / ceiling pieces and the
        // underground surface shader.
        System.Collections.Generic.List<(Transform t, Vector3[] v, int[] tri, Renderer r)> caveMeshes;
        bool UnderCaveMesh(Vector3 at)
        {
            if (caveMeshes == null)
            {
                caveMeshes = new System.Collections.Generic.List<(Transform, Vector3[], int[], Renderer)>();
                foreach (var r in FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None))
                {
                    var n = r.name.ToLowerInvariant(); var mf = r.GetComponent<MeshFilter>();
                    bool cover = n.Contains("cave") || n.Contains("tunnel") || n.Contains("canopy") && !n.Contains("woodland") || n.Contains("ceiling") || n.Contains("cavern") || n.Contains("enclosed");
                    foreach (var m in r.sharedMaterials) if (m && m.shader && m.shader.name.Contains("Underground")) cover = true;
                    if (cover && mf && mf.sharedMesh && mf.sharedMesh.isReadable) caveMeshes.Add((r.transform, mf.sharedMesh.vertices, mf.sharedMesh.triangles, r));
                }
            }
            var ray = new Ray(at + Vector3.up * .5f, Vector3.up);
            foreach (var (t, v, tri, r) in caveMeshes)
            {
                if (!r || !r.enabled) continue; var b = r.bounds;
                if (at.x < b.min.x || at.x > b.max.x || at.z < b.min.z || at.z > b.max.z || b.max.y < at.y || b.min.y > at.y + 45) continue;
                var o = t.InverseTransformPoint(ray.origin); var d = t.InverseTransformDirection(ray.direction);
                for (int i = 0; i < tri.Length; i += 3)
                {
                    // Moller-Trumbore, either facing
                    Vector3 a = v[tri[i]], e1 = v[tri[i + 1]] - a, e2 = v[tri[i + 2]] - a; var pv = Vector3.Cross(d, e2); float det = Vector3.Dot(e1, pv);
                    if (Mathf.Abs(det) < 1e-7f) continue; float inv = 1 / det; var tv = o - a; float u = Vector3.Dot(tv, pv) * inv; if (u < 0 || u > 1) continue;
                    var q = Vector3.Cross(tv, e1); float w = Vector3.Dot(d, q) * inv; if (w < 0 || u + w > 1) continue;
                    float dist = Vector3.Dot(e2, q) * inv; if (dist > 0 && t.TransformVector(d * dist).magnitude < 45) return true;
                }
            }
            return false;
        }
        static void Rate(ParticleSystem ps, float rate)
        {
            var em = ps.emission; em.rateOverTime = rate;
            if (rate > 1 && !ps.isPlaying) ps.Play(); else if (rate <= 1 && ps.isPlaying && ps.particleCount == 0) ps.Stop();
        }
        // A rain loop made at start-up: soft filtered noise with a slight patter, so no audio asset is needed.
        static AudioClip RainClip()
        {
            const int rate = 22050, seconds = 4; var data = new float[rate * seconds]; var rng = new System.Random(7202);
            float lp = 0, lp2 = 0;
            for (int i = 0; i < data.Length; i++)
            {
                float n = (float)rng.NextDouble() * 2 - 1; lp += (n - lp) * .22f; lp2 += (lp - lp2) * .5f;
                float patter = rng.NextDouble() < .0025 ? ((float)rng.NextDouble() * 2 - 1) * .6f : 0;
                data[i] = (lp2 * .55f + patter) * .8f;
            }
            // Cross-fade the ends so the loop has no click.
            int fade = rate / 4; for (int i = 0; i < fade; i++) { float t = i / (float)fade; data[i] = data[i] * t + data[data.Length - fade + i] * (1 - t); }
            var clip = AudioClip.Create("Rain ambience", data.Length - fade, 1, rate, false); var trimmed = new float[data.Length - fade]; System.Array.Copy(data, trimmed, trimmed.Length); clip.SetData(trimmed, 0);
            return clip;
        }
    }
}
