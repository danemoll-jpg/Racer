using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Racer
{
    // 0.95 Part F: an acorn pickup can no longer be missed. A banner at the top centre for about 4 s with its own chime:
    // "ACORN FOUND 23 / 24" and under it the area with its count ("Ridge woodland 5 / 6"); its own canvas scaled from 1080p,
    // so it is the same size on a 4K screen; a dark backing reads day and night. Not over a menu or in split-screen.
    public sealed class AcornBanner : MonoBehaviour
    {
        public const float Seconds = 4;
        RaceFlow flow; RectTransform panel; Text title, area; Image back, accent; float until; AudioSource source; AudioClip chime;
        public static AcornBanner Current { get; private set; }
        public string Shown => panel && panel.gameObject.activeSelf ? title.text + " / " + area.text : "";
        public static AcornBanner Attach(RaceFlow owner)
        {
            var b = owner.GetComponent<AcornBanner>(); if (!b) b = owner.gameObject.AddComponent<AcornBanner>(); b.flow = owner; Current = b; return b;
        }
        void Build()
        {
            var go = new GameObject("Acorn banner", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler)); go.transform.SetParent(transform, false);
            var canvas = go.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 20;
            var scaler = go.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1920, 1080); scaler.matchWidthOrHeight = 1;
            panel = new GameObject("Panel", typeof(RectTransform), typeof(Image)).GetComponent<RectTransform>(); panel.SetParent(go.transform, false);
            panel.anchorMin = panel.anchorMax = panel.pivot = new Vector2(.5f, 1); panel.anchoredPosition = new Vector2(0, -64); panel.sizeDelta = new Vector2(760, 124);
            back = panel.GetComponent<Image>(); back.raycastTarget = false;
            accent = new GameObject("Accent", typeof(RectTransform), typeof(Image)).GetComponent<Image>(); accent.transform.SetParent(panel, false); accent.raycastTarget = false;
            var a = accent.rectTransform; a.anchorMin = new Vector2(0, 0); a.anchorMax = new Vector2(1, 0); a.pivot = new Vector2(.5f, 0); a.sizeDelta = new Vector2(0, 6);
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            Text Line(string name, int size, float top, float height)
            {
                var t = new GameObject(name, typeof(RectTransform), typeof(Text)).GetComponent<Text>(); t.transform.SetParent(panel, false);
                t.font = font; t.fontSize = size; t.alignment = TextAnchor.MiddleCenter; t.raycastTarget = false; t.horizontalOverflow = HorizontalWrapMode.Overflow;
                var r = t.rectTransform; r.anchorMin = new Vector2(0, 1); r.anchorMax = new Vector2(1, 1); r.pivot = new Vector2(.5f, 1); r.anchoredPosition = new Vector2(0, -top); r.sizeDelta = new Vector2(-24, height);
                var o = t.gameObject.AddComponent<Outline>(); o.effectColor = new Color(0, 0, 0, .9f); o.effectDistance = new Vector2(1.6f, -1.6f); return t;
            }
            title = Line("Found", 46, 10, 58); title.fontStyle = FontStyle.Bold; area = Line("Area", 28, 70, 40);
            source = gameObject.AddComponent<AudioSource>(); source.playOnAwake = false; source.spatialBlend = 0; source.ignoreListenerPause = true;
            chime = Synth.Notes("Acorn found", new[] { 1318.5f, 1760f, 2637f }, .11f, .42f);
            panel.gameObject.SetActive(false);
        }
        public void Show(int found, int total, string areaName, string areaCount)
        {
            if (!panel) Build();
            title.text = $"ACORN FOUND   {found} / {total}"; area.text = string.IsNullOrEmpty(areaName) ? "" : $"{areaName}   {areaCount}";
            until = Time.unscaledTime + Seconds;
            if (source && chime && flow && flow.Save != null) source.PlayOneShot(chime, flow.Save.Settings.feedback * .5f);
        }
        void LateUpdate()
        {
            if (!panel || !flow) return;
            bool on = Time.unscaledTime < until && !flow.MenuVisible && !SplitScreen.Active && !TrailerMode.Active;
            if (panel.gameObject.activeSelf != on) panel.gameObject.SetActive(on); if (!on) return;
            float a = Mathf.Clamp01((until - Time.unscaledTime) / .4f);
            back.color = new Color(.02f, .04f, .05f, .86f * a); accent.color = new Color(1, .72f, .2f, a);
            title.color = new Color(1, .84f, .35f, a); area.color = new Color(.93f, .96f, .9f, a);
        }
        void OnDestroy() { if (chime) Destroy(chime); if (Current == this) Current = null; }
    }

    // 0.95 Part F: one-off unlocks that happen while driving (the acorn reward now; any later one the same way) get a panel
    // that stays until A: the heading, "UNLOCKED: <vehicle>", the vehicle turning with its stat bars, "Choose it in the
    // Garage", and a fanfare. Free Roam is paused (the vehicle stopped) while it is up. One that was earned but never shown
    // (Dan's mower) is shown once the next time the main menu or Free Roam opens; pressing A remembers it (settings.json
    // unlocksSeen) - loading never writes. Campaign prize reveals are unchanged.
    public static class UnlockNotice
    {
        public sealed class Notice { public string id, heading, vehicle; }
        public const string AcornMower = "acorn-mower";
        public static readonly List<Notice> Pending = new();
        public static Notice Showing;
        public static bool Seen(RacerSave save, string id) => save != null && save.Settings.unlocksSeen != null && save.Settings.unlocksSeen.Contains(id);
        public static void Raise(RacerSave save, Notice n)
        {
            if (n == null || Seen(save, n.id) || Pending.Any(p => p.id == n.id) || Showing?.id == n.id) return;
            Pending.Add(n);
        }
        public static Notice Acorns(int total) => new() { id = AcornMower, heading = $"ALL {total} WOODLAND ACORNS FOUND", vehicle = "mower" };
        // earned before this update (or on another run) but its panel never shown
        public static void Missed(RacerSave save) { if (VehicleUnlocks.RewardEarned) Raise(save, Acorns(VehicleUnlocks.AcornsNeeded)); }
        public static void MarkSeen(RacerSave save, string id)
        {
            if (save == null || Seen(save, id)) return;
            save.Settings.unlocksSeen = (save.Settings.unlocksSeen ?? new string[0]).Append(id).ToArray(); save.SaveSettings();
        }
    }

    // short original tones (sine with soft harmonics and a quick decay); CC0, no audio assets
    public static class Synth
    {
        public static AudioClip Notes(string name, float[] hz, float each, float last)
        {
            const int rate = 44100; int n = Mathf.CeilToInt(rate * (each * (hz.Length - 1) + last)); var data = new float[n];
            for (int k = 0; k < hz.Length; k++)
            {
                int start = Mathf.RoundToInt(rate * each * k); float length = k == hz.Length - 1 ? last : each * 1.6f; int count = Mathf.Min(n - start, Mathf.CeilToInt(rate * length));
                for (int i = 0; i < count; i++)
                {
                    float t = i / (float)rate, env = Mathf.Min(1, t / .006f) * Mathf.Exp(-t * (k == hz.Length - 1 ? 5 : 9));
                    data[start + i] += (Mathf.Sin(2 * Mathf.PI * hz[k] * t) + .3f * Mathf.Sin(4 * Mathf.PI * hz[k] * t) + .12f * Mathf.Sin(6 * Mathf.PI * hz[k] * t)) * env * .42f;
                }
            }
            var clip = AudioClip.Create(name, n, 1, rate, false); clip.SetData(data, 0); return clip;
        }
    }
}
