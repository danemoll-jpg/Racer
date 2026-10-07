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
        void ShowCourseOnMap(int index)
        {
            if (!courseView || index == shownCourse || !courseVisual) return;
            shownCourse = index;
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
            courseCaption.text = $"{RacePlaylists.Titles[index]}\nLap {length / 1000f:0.0} km ({length / 1609.344f:0.0} mi)   ·   Race Setup: {laps} lap{(laps == 1 ? "" : "s")}\nCyan: main  ·  Gold: shortcuts  ·  Arrows: direction";
        }
    }
    // Mouse hover moves the highlight in the Tracks list (so the map follows the pointer as well as the keys).
    public sealed class CourseRowHover : MonoBehaviour, IPointerEnterHandler
    {
        public void OnPointerEnter(PointerEventData e) { if ((name.StartsWith("course-") || name.StartsWith("cev-")) && EventSystem.current && GetComponent<UnityEngine.UI.Selectable>()?.interactable == true) EventSystem.current.SetSelectedGameObject(gameObject); }
    }
}
