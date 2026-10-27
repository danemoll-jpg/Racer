using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Racer
{
    // 0.83 Part D: the Tracks screen shows the course map at all times, in a fixed panel beside the list (the 0.76 garage
    // layout): as the highlight moves through the list (keyboard, controller or mouse hover) the map shows that course's
    // route (main route cyan, shortcuts gold, white direction arrows, the start marked), with its name, lap length and the
    // Race Setup lap count. Rows without a route (Back) show the plain map. The map and overlay are built once; a change of
    // highlight only re-aims the map picture and redraws the overlay, so moving quickly through the list does not stutter.
    // (Replaces the 0.76 "Preview highlighted track" row and its separate preview page.)
    public sealed partial class RaceMenus
    {
        int previewTrack = -1;
        RectTransform coursePanel, courseMapRect; UnityEngine.UI.RawImage courseMap; WorldMapCourseOverlay courseOverlay;
        UnityEngine.UI.Text courseCaption; WorldMapVisual courseVisual; bool courseView; int shownCourse = -2;
        public int ShownCourse => shownCourse;

        void EnterCourseView(float share = .5f)
        {
            if (!coursePanel)
            {
                if (menuCardSize == Vector2.zero) menuCardSize = card.sizeDelta;
                courseVisual = Resources.Load<WorldMapVisual>("WorldMaps/PermanentWorld");
                coursePanel = Rect("Course map panel", card);
                coursePanel.gameObject.AddComponent<UnityEngine.UI.Image>().color = new Color(.06f, .1f, .13f);
                courseMapRect = Rect("Map", coursePanel); courseMapRect.anchorMin = courseMapRect.anchorMax = courseMapRect.pivot = new Vector2(.5f, .5f);
                courseMap = courseMapRect.gameObject.AddComponent<UnityEngine.UI.RawImage>(); courseMap.raycastTarget = false;
                if (courseVisual) courseMap.texture = courseVisual.image;
                courseMapRect.gameObject.AddComponent<UnityEngine.UI.RectMask2D>();
                var overlay = Rect("Route", courseMapRect); Stretch(overlay, 0, 0, 0, 0);
                courseOverlay = overlay.gameObject.AddComponent<WorldMapCourseOverlay>(); courseOverlay.raycastTarget = false;
                courseCaption = Label("Course", coursePanel, 18, 0); courseCaption.alignment = TextAnchor.MiddleCenter; courseCaption.color = new Color(.92f, .92f, .86f);
                courseCaption.rectTransform.anchorMin = new Vector2(0, 0); courseCaption.rectTransform.anchorMax = new Vector2(1, 0); courseCaption.rectTransform.pivot = new Vector2(.5f, 0);
                courseCaption.rectTransform.offsetMin = new Vector2(12, 8); courseCaption.rectTransform.offsetMax = new Vector2(-12, 100);
            }
            if (menuCardSize == Vector2.zero) menuCardSize = card.sizeDelta;
            // As wide as the screen allows, as the garage.
            var parent = (RectTransform)card.parent;
            float width = Mathf.Min(1220, parent.rect.width - 40), height = Mathf.Min(menuCardSize.y, parent.rect.height - 24);
            card.sizeDelta = new Vector2(width, height);
            float panel = Mathf.Clamp(width * share, 340, 600);
            coursePanel.gameObject.SetActive(true); courseCaption.fontSize = 18;
            coursePanel.anchorMin = new Vector2(0, 0); coursePanel.anchorMax = new Vector2(0, 1); coursePanel.pivot = new Vector2(0, .5f);
            coursePanel.offsetMin = new Vector2(32, 78); coursePanel.offsetMax = new Vector2(32 + panel, -82);
            ((RectTransform)scroll.transform).offsetMin = new Vector2(32 + panel + 24, 78);
            // the map keeps the world map's proportions inside the panel, above the caption
            Canvas.ForceUpdateCanvases();
            float aspect = courseVisual ? courseVisual.bounds.width / courseVisual.bounds.height : 1;
            var room = coursePanel.rect.size - new Vector2(24, 24 + 108);
            var size = room.x / room.y > aspect ? new Vector2(room.y * aspect, room.y) : new Vector2(room.x, room.x / aspect);
            courseMapRect.sizeDelta = size; courseMapRect.anchoredPosition = new Vector2(0, 54);
            courseView = true; shownCourse = -2;
        }
        void LeaveCourseView()
        {
            if (!courseView) return;
            courseView = false; card.sizeDelta = menuCardSize; ((RectTransform)scroll.transform).offsetMin = new Vector2(32, 78);
            coursePanel.gameObject.SetActive(false);
        }
        // The highlighted course on the map (-1: the plain map).
        // 0.90 Part A: greyLocked (Tracks) draws a course the campaign has not opened in grey, with how to reach it.
        void ShowCourseOnMap(int index, bool greyLocked = false)
        {
            if (!courseView || index == shownCourse || !courseVisual) return;
            shownCourse = index; bool locked = greyLocked && index >= 0 && index < RacePlaylists.Scenes.Length && !Campaign.CourseOpen(index); courseOverlay.SetLocked(locked);
            var courses = CoursePreviewCatalog.Courses;
            if (index < 0 || index >= courses.Length)
            {
                courseMap.uvRect = new Rect(0, 0, 1, 1); courseOverlay.SetView(null, courseVisual, Vector2.one * .5f, 1, (CoursePreviewCatalog.Course)null);
                courseCaption.text = "Highlight a track to see its route\nCyan: main  ·  Gold: shortcuts  ·  Arrows: direction";
                return;
            }
            var course = courses[index];
            var bounds = new Bounds(course.main[0], Vector3.zero); foreach (var p in course.main) bounds.Encapsulate(p);
            foreach (var branch in course.branches) foreach (var p in branch.points) bounds.Encapsulate(p);
            var center = courseVisual.Normalized(bounds.center); var extent = courseVisual.Normalized(bounds.max) - courseVisual.Normalized(bounds.min);
            float zoom = Mathf.Clamp(.85f / Mathf.Max(extent.x, extent.y), 1, 6);
            courseMap.uvRect = new Rect(center - Vector2.one * .5f / zoom, Vector2.one / zoom);
            courseOverlay.SetView(flow.Race, courseVisual, center, zoom, course);
            float length = 0; for (int i = 0; i < course.main.Length; i++) { var a = course.main[i]; var b = course.main[(i + 1) % course.main.Length]; a.y = b.y = 0; length += Vector3.Distance(a, b); }
            int laps = flow.Race.laps;
            courseCaption.text = locked ? $"{RacePlaylists.Titles[index]}   ·   LOCKED\n{Campaign.CourseHowTo(index).Replace("Campaign: ", "")}\nLap {length / 1000f:0.0} km ({length / 1609.344f:0.0} mi)" : $"{RacePlaylists.Titles[index]}\nLap {length / 1000f:0.0} km ({length / 1609.344f:0.0} mi)   ·   Race Setup: {laps} lap{(laps == 1 ? "" : "s")}\nCyan: main  ·  Gold: shortcuts  ·  Arrows: direction";
        }
        // 1.3.0-review4: a campaign event held at one place shows that place, not a race route: the park events the stunt park (its outline,
        // filled), the speed trap / jump / smash / tricks / land-it-clean events their site, with a faint line of the course's road round it and
        // an arrow along the run-up (RunUp metres of the course's road before the site, where CampaignRun starts the event). false: a race or
        // time trial (or a site not listed below), drawn as before by ShowCourseOnMap. The events' Course (0 for the park events) is not changed.
        // The sites as saved in their course scenes (ActivitySite, the event's Course scene; read from the scenes by Tools/Report1300/review4/DumpEventSites.cs).
        static readonly System.Collections.Generic.Dictionary<string, Vector3> EventSites = new()
        {
            ["StreetLoopGreybox/speed-0"] = new(-396.33f, 8.44f, 534.11f),
            ["LakeWoods/jump-01"] = new(372.46f, 78.09f, 230.92f),
            ["DansBackyardForward/smash-01"] = new(481.34f, 88.11f, -89.71f),
            ["MountainLoop/summit-homeward"] = new(980.38f, 177.92f, 147.27f),
        };
        bool ShowEventSpot(CampaignEvent e)
        {
            if (!courseView || !courseVisual || e.Kind is CampaignEventKind.Race or CampaignEventKind.TimeTrial) return false;
            var kind = e.Kind switch { CampaignEventKind.Park => WorldMapCourseOverlay.SpotKind.Park, CampaignEventKind.SpeedTrap => WorldMapCourseOverlay.SpotKind.SpeedTrap, CampaignEventKind.Jump => WorldMapCourseOverlay.SpotKind.Jump,
                CampaignEventKind.Smash => WorldMapCourseOverlay.SpotKind.Smash, CampaignEventKind.Trick => WorldMapCourseOverlay.SpotKind.Trick, _ => WorldMapCourseOverlay.SpotKind.Clean };
            Vector3 site; Vector3[] area = null, road = null, runUp = null;
            var bounds = new Bounds();
            if (kind == WorldMapCourseOverlay.SpotKind.Park)
            {
                area = StuntPark.Outline; site = StuntPark.Centre; bounds = new Bounds(site, Vector3.zero); foreach (var p in area) bounds.Encapsulate(p);
                bounds.Expand(new Vector3(60, 0, 40));
            }
            else
            {
                var courses = CoursePreviewCatalog.Courses;
                if (e.Site == null || e.Course < 0 || e.Course >= courses.Length || e.Course >= RacePlaylists.Scenes.Length || !EventSites.TryGetValue(RacePlaylists.Scenes[e.Course] + "/" + e.Site, out site)) return false;
                var main = courses[e.Course].main; if (main == null || main.Length < 2) return false;
                // the course line's stations (flat), the site projected onto it, then RunUp metres back from there (the event's start)
                var at = new float[main.Length + 1]; float best = float.MaxValue, station = 0;
                for (int i = 0; i < main.Length; i++)
                {
                    Vector2 a = new(main[i].x, main[i].z), b = new(main[(i + 1) % main.Length].x, main[(i + 1) % main.Length].z), q = new(site.x, site.z);
                    float len = Vector2.Distance(a, b); at[i + 1] = at[i] + len;
                    float t = len > 0 ? Mathf.Clamp01(Vector2.Dot(q - a, b - a) / (len * len)) : 0; float d = (a + (b - a) * t - q).sqrMagnitude;
                    if (d < best) { best = d; station = at[i] + t * len; }
                }
                float total = at[main.Length];
                Vector3 On(float s) { s = Mathf.Repeat(s, total); int i = System.Array.FindLastIndex(at, x => x <= s); i = Mathf.Clamp(i, 0, main.Length - 1); float len = at[i + 1] - at[i]; return Vector3.Lerp(main[i], main[(i + 1) % main.Length], len > 0 ? (s - at[i]) / len : 0); }
                Vector3[] Stretch(float from, float to) { int n = Mathf.Max(2, Mathf.CeilToInt((to - from) / 4) + 1); var pts = new Vector3[n]; for (int k = 0; k < n; k++) pts[k] = On(Mathf.Lerp(from, to, k / (n - 1f))); return pts; }
                float run = Mathf.Max(40, e.RunUp);
                runUp = Stretch(station - run, station); road = Stretch(station - run - 120, station + 160);
                bounds = new Bounds(site, Vector3.zero); foreach (var p in runUp) bounds.Encapsulate(p); bounds.Encapsulate(On(station + 60));
                bounds.Expand(new Vector3(80, 0, 80));
            }
            var center = courseVisual.Normalized(bounds.center); var extent = courseVisual.Normalized(bounds.max) - courseVisual.Normalized(bounds.min);
            // the place fits in the lower part of the map, clear of the targets panel drawn across its top (ShowMedalOverlay)
            float zoom = Mathf.Clamp(Mathf.Min(.9f / extent.x, .6f / extent.y), 1, 10); center.y += .17f / zoom;
            courseMap.uvRect = new Rect(center - Vector2.one * .5f / zoom, Vector2.one / zoom);
            courseOverlay.SetLocked(false); courseOverlay.SetSpot(courseVisual, center, zoom, site, kind, area, road, runUp);
            shownCourse = -3; // not a course: the next course shown is drawn again
            return true;
        }
    }
    // Mouse hover moves the highlight in the Tracks list (so the map follows the pointer as well as the keys).
    public sealed class CourseRowHover : MonoBehaviour, IPointerEnterHandler
    {
        public void OnPointerEnter(PointerEventData e) { if ((name.StartsWith("course-") || name.StartsWith("cev-") || name.StartsWith("ccup-")) && EventSystem.current && GetComponent<UnityEngine.UI.Selectable>()?.interactable == true) EventSystem.current.SetSelectedGameObject(gameObject); }
    }
}
