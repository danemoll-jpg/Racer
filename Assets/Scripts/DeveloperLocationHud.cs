using System.Globalization;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Racer
{
    public sealed class DeveloperLocationHud : MonoBehaviour
    {
        RaceDirector race;
        GameObject panel;
        UnityEngine.UI.Text label, confirmation;
        bool visible;
        float copiedUntil, nextRefresh;
        public bool Visible => visible;
        public static void Create(Transform parent, RaceDirector owner, Font font)
        {
            var host = new GameObject("Developer location HUD", typeof(RectTransform));
            host.transform.SetParent(parent, false);
            var hud = host.AddComponent<DeveloperLocationHud>();
            hud.race = owner;
            hud.panel = new GameObject("World XYZ panel", typeof(RectTransform), typeof(UnityEngine.UI.Image));
            hud.panel.transform.SetParent(parent, false);
            var rect = (RectTransform)hud.panel.transform;
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(1, 0);
            rect.anchoredPosition = new(-18, 96); rect.sizeDelta = new(236, 148);
            var background = hud.panel.GetComponent<UnityEngine.UI.Image>();
            background.color = new(.025f, .055f, .07f, .9f); background.raycastTarget = false;
            hud.label = Text("Unity world coordinates", rect, font);
            hud.label.rectTransform.anchorMin = Vector2.zero; hud.label.rectTransform.anchorMax = Vector2.one;
            hud.label.rectTransform.offsetMin = new(10, 8); hud.label.rectTransform.offsetMax = new(-8, -8);
            hud.confirmation = Text("Location copied", parent, font);
            var cr = hud.confirmation.rectTransform;
            cr.anchorMin = cr.anchorMax = cr.pivot = new(1, 0);
            cr.anchoredPosition = new(-18, 250); cr.sizeDelta = new(236, 26);
            hud.confirmation.alignment = TextAnchor.MiddleRight;
            hud.confirmation.gameObject.AddComponent<UnityEngine.UI.Outline>();
            hud.confirmation.text = "LOCATION COPIED"; hud.confirmation.enabled = false;
            hud.panel.SetActive(false);
        }
        static UnityEngine.UI.Text Text(string name, Transform parent, Font font)
        {
            var text = new GameObject(name, typeof(RectTransform), typeof(UnityEngine.UI.Text)).GetComponent<UnityEngine.UI.Text>();
            text.transform.SetParent(parent, false); text.font = font; text.fontSize = 16;
            text.color = Color.white; text.raycastTarget = false; text.supportRichText = false;
            return text;
        }
        bool ActiveCourse => race && !race.FreeRoam && race.Flow && !race.Flow.MenuVisible;
        public string LocationText()
        {
            var p = race.vehicle.transform.position;
            return string.Format(CultureInfo.InvariantCulture, "Position: X={0:F1}, Y={1:F1}, Z={2:F1} | Course: {3}", p.x, p.y, p.z, ActiveCourse ? race.courseName : "None");
        }
        public void Toggle() { visible = !visible; nextRefresh = 0; }
        public void CopyLocation()
        {
            if (!race || !race.vehicle) return;
            string text = LocationText(); GUIUtility.systemCopyBuffer = text;
            if (GUIUtility.systemCopyBuffer == text) copiedUntil = Time.unscaledTime + 1.7f;
        }
        void Update()
        {
            if (!race || !race.vehicle) return;
            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.f3Key.wasPressedThisFrame) Toggle();
                if (keyboard.f4Key.wasPressedThisFrame) CopyLocation();
            }
            bool gameplay = race.Flow && !race.Flow.MenuVisible;
            panel.SetActive(visible && gameplay);
            confirmation.enabled = gameplay && Time.unscaledTime < copiedUntil;
            if (!visible || Time.unscaledTime < nextRefresh) return;
            nextRefresh = Time.unscaledTime + .1f;
            var p = race.vehicle.transform.position;
            label.text = string.Format(CultureInfo.InvariantCulture, "WORLD XYZ\nX: {0:F1}\nY: {1:F1}\nZ: {2:F1}\nCourse: {3}\nF3 hide  /  F4 copy", p.x, p.y, p.z, ActiveCourse ? race.courseName : "None");
        }
        void OnDestroy() { if (panel) Destroy(panel); if (confirmation) Destroy(confirmation.gameObject); }
    }
}
