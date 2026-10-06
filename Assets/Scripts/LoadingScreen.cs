using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Racer
{
    // 0.82 Part B: a full-screen loading screen for every scene load (the first load after launch, a course or race, Free
    // Roam, back to the menu) instead of a frozen frame and Windows' busy circle. Process lifetime. It shows what is loading
    // (with the course's route on the world map, the chosen conditions and vehicle), a progress bar that follows the real
    // work (the scene load itself, then each world-building step SceneryWorld reports, then the vehicles and the first
    // rendered frames, which also compile the shaders of what is in view) and one rotating tip. It stays up until the
    // world is built and the first frames have rendered, then fades out; no input is needed. While it is up, RaceFlow
    // holds the countdown and the menus and the vehicle takes no input.
    // 0.83 Part E: the game poster (Resources/LoadingPoster, SourceArt/Poster/WoodstockRushPoster.png unaltered) is the
    // full-screen picture, scaled to cover the screen without stretching (cropped evenly on other aspect ratios), drawn
    // with bilinear filtering and no sharpening. The loading information sits in a dark gradient band along the bottom,
    // clear of the title lettering (upper centre) and the two vehicles (centre); the route map is a small inset in the
    // bottom-right corner.
    public sealed class LoadingScreen : MonoBehaviour
    {
        static LoadingScreen instance;
        // up and not yet fading out
        public static bool Holding => instance && instance.holding;
        public static int Shown { get; private set; }
        public static string LastTitle { get; private set; } = "";
        public static float LastSeconds { get; private set; }

        // the bar: the scene load 0-30 %, world-building steps 30-88 %, vehicles 88-92 %, first rendered frames 92-100 %
        const float SceneShare = .30f, WorldEnd = .88f, StartEnd = .92f;
        const float Fade = .35f;
        static readonly string[] Tips =
        {
            "Stuck or upside down? R (Y on a controller) puts you back on the road.",
            "V (X on a controller) changes the camera view.",
            "F (LB on a controller) shakes a fist out of the window.",
            "J (B on a controller) shows or hides the minimap.",
            "In Free Roam, M (View on a controller) opens the map; F (Y) sets a waypoint to drive to.",
            "F8 turns on Trailer / Photo Mode for filming and screenshots.",
            "Race setup has Time of Day and Weather options; Free Roam has its own weather choice.",
        };
        static int tipIndex = -1;

        GameObject canvas; CanvasGroup group; RectTransform fill; Text title, detail, step, tip;
        bool holding; float shown, target, bar, tipAt; bool worldReady, started;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetProcess() { instance = null; Shown = 0; }
        // the first load after launch: up before the first scene's first frame
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Boot() { if (Application.isBatchMode) return; Show("Woodstock Rush", "Starting up", null); instance.target = SceneShare; }

        // Loads a scene behind the loading screen (replaces SceneManager.LoadScene for the player's scene changes).
        public static void LoadScene(string scene, string what, string detail, CoursePreviewCatalog.Course course)
        {
            Show(what, detail, course); instance.StartCoroutine(instance.Load(scene));
        }
        // world-building steps (SceneryWorld): done of total
        public static void Report(string label, int done, int total)
        {
            if (!instance || !instance.holding) return;
            instance.step.text = label; instance.target = Mathf.Max(instance.target, Mathf.Lerp(SceneShare, WorldEnd, total > 0 ? done / (float)total : 1));
        }
        // the scene's world is built (RaceFlow then places the vehicles and starts the race / Free Roam / menu)
        public static void WorldReady() { if (instance && instance.holding) { instance.worldReady = true; instance.step.text = "Vehicles"; instance.target = Mathf.Max(instance.target, WorldEnd); } }
        // RaceFlow has started: the first frames are rendered, then the screen fades out
        public static void Started() { if (instance && instance.holding) { instance.started = true; instance.worldReady = true; } }

        static void Show(string what, string detail, CoursePreviewCatalog.Course course)
        {
            if (!instance) { instance = new GameObject("Loading screen (0.82)").AddComponent<LoadingScreen>(); DontDestroyOnLoad(instance.gameObject); instance.Build(); }
            var s = instance; s.StopAllCoroutines(); s.canvas.SetActive(true); s.group.alpha = 1; s.holding = true; s.worldReady = s.started = false;
            s.shown = Time.realtimeSinceStartup; s.target = 0; s.bar = 0; s.title.text = what; s.detail.text = (detail ?? "").Replace("\n", "   ·   "); s.step.text = "Loading";
            s.SetCourse(course); tipIndex = (tipIndex + 1 + Random.Range(0, Tips.Length - 1)) % Tips.Length; s.tip.text = Tips[tipIndex]; s.tipAt = Time.realtimeSinceStartup;
            Shown++; LastTitle = what; s.StartCoroutine(s.Watch());
        }

        IEnumerator Load(string scene)
        {
            yield return null; yield return null; // the screen is drawn before the old scene stops
            var op = SceneManager.LoadSceneAsync(scene);
            while (!op.isDone) { target = Mathf.Max(target, op.progress / .9f * SceneShare); yield return null; }
            target = Mathf.Max(target, SceneShare);
        }
        // after the scene: wait for the world and RaceFlow (60 s at most), then the first rendered frames, then fade
        IEnumerator Watch()
        {
            float t0 = Time.realtimeSinceStartup;
            while (!started && Time.realtimeSinceStartup - t0 < 60) { if (SceneManager.GetActiveScene().isLoaded && !FindAnyObjectByType<RaceFlow>() && Time.realtimeSinceStartup - t0 > 3) break; yield return null; }
            step.text = "Getting ready"; target = Mathf.Max(target, StartEnd);
            // the first rendered frames: at least 8, until 4 in a row take under 50 ms (shader compiling and first-use loads
            // show as long frames), 4 s at most
            int frames = 0, smooth = 0; float t1 = Time.realtimeSinceStartup;
            while (frames < 8 || (smooth < 4 && Time.realtimeSinceStartup - t1 < 4)) { yield return null; frames++; smooth = Time.unscaledDeltaTime < .05f ? smooth + 1 : 0; target = Mathf.Max(target, Mathf.Lerp(StartEnd, 1, Mathf.Min(1, frames / 12f))); }
            target = 1; step.text = "Ready"; while (bar < .995f) yield return null;
            holding = false; LastSeconds = Time.realtimeSinceStartup - shown;
            for (float t = 0; t < Fade; t += Time.unscaledDeltaTime) { group.alpha = 1 - t / Fade; yield return null; }
            canvas.SetActive(false);
        }

        void Update()
        {
            if (!holding) return;
            bar = Mathf.MoveTowards(bar, target, Time.unscaledDeltaTime * 1.6f);
            fill.anchorMax = new Vector2(Mathf.Clamp01(bar), 1);
            if (Time.realtimeSinceStartup - tipAt > 5) { tipIndex = (tipIndex + 1) % Tips.Length; tip.text = Tips[tipIndex]; tipAt = Time.realtimeSinceStartup; }
        }

        RectTransform map, mapFrame; RawImage mapImage; WorldMapCourseOverlay route;
        void SetCourse(CoursePreviewCatalog.Course course)
        {
            var visual = course != null ? Resources.Load<WorldMapVisual>("WorldMaps/PermanentWorld") : null;
            mapFrame.gameObject.SetActive(visual);
            if (!visual) return;
            mapFrame.sizeDelta = new Vector2(110 * visual.bounds.width / visual.bounds.height + 8, 118); mapImage.texture = visual.image;
            var bounds = new Bounds(course.main[0], Vector3.zero); foreach (var p in course.main) bounds.Encapsulate(p); foreach (var b in course.branches) foreach (var p in b.points) bounds.Encapsulate(p);
            var center = visual.Normalized(bounds.center); var extent = visual.Normalized(bounds.max) - visual.Normalized(bounds.min); float zoom = Mathf.Clamp(.85f / Mathf.Max(extent.x, extent.y), 1, 6);
            mapImage.uvRect = new Rect(center - Vector2.one * .5f / zoom, Vector2.one / zoom); route.SetView(null, visual, center, zoom, course);
        }

        // 0.83: the poster behind everything; a gradient band at the bottom with the information, the menus' cyan accent
        void Build()
        {
            canvas = new GameObject("Loading screen", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(CanvasGroup));
            canvas.transform.SetParent(transform, false);
            var c = canvas.GetComponent<Canvas>(); c.renderMode = RenderMode.ScreenSpaceOverlay; c.sortingOrder = 32500; // above the title artwork
            var scaler = canvas.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1280, 720); scaler.matchWidthOrHeight = .5f;
            group = canvas.GetComponent<CanvasGroup>(); group.blocksRaycasts = true;
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var matte = Rect("Matte", canvas.transform); Stretch(matte); matte.gameObject.AddComponent<Image>().color = new Color(.012f, .019f, .025f);
            var poster = Resources.Load<Texture2D>("LoadingPoster");
            if (poster)
            {
                var art = Rect("Poster", matte); art.anchorMin = art.anchorMax = art.pivot = new Vector2(.5f, .5f);
                art.gameObject.AddComponent<RawImage>().texture = poster; art.GetComponent<RawImage>().raycastTarget = false;
                var fit = art.gameObject.AddComponent<AspectRatioFitter>(); fit.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent; fit.aspectRatio = poster.width / (float)poster.height;
            }
            // the band: transparent at its top edge, dark at the bottom of the screen
            var shade = new Texture2D(1, 64, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear, name = "Loading band" };
            for (int y = 0; y < 64; y++) { float u = y / 63f; shade.SetPixel(0, y, new Color(.01f, .016f, .022f, Mathf.Lerp(.9f, 0, Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.35f, 1, u))))); }
            shade.Apply();
            var band = Rect("Band", matte); band.anchorMin = Vector2.zero; band.anchorMax = new Vector2(1, .27f); band.offsetMin = band.offsetMax = Vector2.zero;
            var bandImage = band.gameObject.AddComponent<RawImage>(); bandImage.texture = shade; bandImage.raycastTarget = false;
            var panel = band;
            Text Label(string name, Vector2 min, Vector2 max, int size, Color color, TextAnchor anchor, FontStyle style = FontStyle.Normal)
            {
                var r = Rect(name, panel); r.anchorMin = min; r.anchorMax = max; r.offsetMin = r.offsetMax = Vector2.zero;
                var t = r.gameObject.AddComponent<Text>(); t.font = font; t.fontSize = size; t.color = color; t.alignment = anchor; t.fontStyle = style; t.raycastTarget = false; t.horizontalOverflow = HorizontalWrapMode.Wrap;
                r.gameObject.AddComponent<Shadow>().effectColor = new Color(0, 0, 0, .8f); return t;
            }
            // band coordinates: 0 = bottom of the screen, 1 = 27 % up
            Label("Loading label", new Vector2(.03f, .62f), new Vector2(.5f, .74f), 14, new Color(.3f, 1, .88f), TextAnchor.LowerLeft, FontStyle.Bold).text = "LOADING";
            title = Label("Title", new Vector2(.03f, .40f), new Vector2(.62f, .64f), 32, Color.white, TextAnchor.MiddleLeft, FontStyle.Bold);
            step = Label("Step", new Vector2(.55f, .40f), new Vector2(.80f, .50f), 15, new Color(.86f, .86f, .8f), TextAnchor.LowerRight);
            detail = Label("Detail", new Vector2(.03f, .24f), new Vector2(.80f, .40f), 16, new Color(.9f, .9f, .84f), TextAnchor.MiddleLeft);
            var track = Rect("Bar", panel); track.anchorMin = new Vector2(.03f, .17f); track.anchorMax = new Vector2(.80f, .21f); track.offsetMin = track.offsetMax = Vector2.zero; track.gameObject.AddComponent<Image>().color = new Color(1, 1, 1, .16f);
            fill = Rect("Fill", track); fill.anchorMin = Vector2.zero; fill.anchorMax = new Vector2(0, 1); fill.offsetMin = fill.offsetMax = Vector2.zero; fill.gameObject.AddComponent<Image>().color = new Color(.3f, 1, .88f);
            tip = Label("Tip", new Vector2(.03f, .02f), new Vector2(.80f, .15f), 15, new Color(.8f, .82f, .78f), TextAnchor.MiddleLeft, FontStyle.Italic);
            // the route: a small inset in the bottom-right corner, in a dark frame
            mapFrame = Rect("Course frame", matte); mapFrame.anchorMin = mapFrame.anchorMax = mapFrame.pivot = new Vector2(1, 0); mapFrame.anchoredPosition = new Vector2(-18, 14);
            mapFrame.gameObject.AddComponent<Image>().color = new Color(.02f, .03f, .04f, .9f);
            map = Rect("Course", mapFrame); Stretch(map); map.offsetMin = new Vector2(4, 4); map.offsetMax = new Vector2(-4, -4);
            mapImage = map.gameObject.AddComponent<RawImage>(); mapImage.raycastTarget = false; map.gameObject.AddComponent<RectMask2D>();
            var overlay = Rect("Route", map); Stretch(overlay); route = overlay.gameObject.AddComponent<WorldMapCourseOverlay>(); route.raycastTarget = false;
            mapFrame.gameObject.SetActive(false);
        }
        static RectTransform Rect(string name, Transform parent) { var r = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>(); r.SetParent(parent, false); return r; }
        static void Stretch(RectTransform r) { r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one; r.offsetMin = r.offsetMax = Vector2.zero; }
    }
}
