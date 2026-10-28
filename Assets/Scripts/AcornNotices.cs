using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Racer
{
    // 0.95 Part F: an acorn pickup can no longer be missed. A banner at the top centre for about 4 s with its own chime:
    // "ACORN FOUND 23 / 24" and under it the area with its count ("Ridge woodland 5 / 6"); its own canvas scaled from 1080p,
    // so it is the same size on a 4K screen; a dark backing reads day and night. Not over a menu.
    // 1.3.0-review5 Part A: in split screen it shows at the top of the half of the player who found it (smaller).
    public sealed class AcornBanner : MonoBehaviour
    {
        public const float Seconds = 4;
        RaceFlow flow; RectTransform panel; Text title, area; Image back, accent; float until; AudioSource source; AudioClip chime; int player = 1;
        public int Player => player;
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
        public void Show(int found, int total, string areaName, string areaCount, int who = 1)
        {
            if (!panel) Build(); player = who;
            title.text = $"ACORN FOUND   {found} / {total}"; area.text = string.IsNullOrEmpty(areaName) ? "" : $"{areaName}   {areaCount}";
            until = Time.unscaledTime + Seconds;
            if (source && chime && flow && flow.Save != null) source.PlayOneShot(chime, flow.Save.Settings.feedback * .5f);
        }
        void LateUpdate()
        {
            if (!panel || !flow) return;
            bool on = Time.unscaledTime < until && !flow.MenuVisible && !TrailerMode.Active;
            if (panel.gameObject.activeSelf != on) panel.gameObject.SetActive(on); if (!on) return;
            bool split = SplitScreen.Active && !SplitScreen.OneView, lr = SplitScreen.LeftRight; int i = player - 1;
            var min = !split ? Vector2.zero : lr ? new Vector2(i * .5f, 0) : new Vector2(0, i == 0 ? .5f : 0); var max = !split ? Vector2.one : lr ? new Vector2(i * .5f + .5f, 1) : new Vector2(1, i == 0 ? 1 : .5f);
            panel.anchorMin = panel.anchorMax = new Vector2((min.x + max.x) * .5f, max.y); panel.localScale = Vector3.one * (split ? .72f : 1); panel.anchoredPosition = new Vector2(0, split && lr ? -310 : -64); // side by side: under the info panel and the minimap
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
    // 0.108 Part C (Dan: "It should do this anytime you unlock a vehicle"): every vehicle unlock - the acorn mower, the patrol car, the patrol cycle,
    // the stunt motorcycle and each campaign chapter prize - is "CONGRATULATIONS!  You unlocked the <vehicle>" with its picture and how it was earned
    // (heading, one line), until A / Enter. Earned while a results screen is up, it opens as that screen is left (RaceMenus.HoldResults), then the
    // player's choice goes ahead. Raised notices are kept in settings.json (unlocksPending) until shown, and an earned vehicle whose panel was never
    // shown is found again at load, so none is lost when the game closes or a scene loads (0.106's stunt-bike panel was raised during the Stunt
    // event's results, where split-screen holds the panel back, and the scene load leaving the event cleared the queue before it could show).
    public static class UnlockNotice
    {
        public sealed class Notice { public string id, heading, vehicle; }
        public const string AcornMower = "acorn-mower", PoliceCar = "police-car", PoliceBike = "police-bike", StuntBike = "stunt-bike", Prize = "prize-";
        public static readonly List<Notice> Pending = new();
        public static Notice Showing;
        public static bool Seen(RacerSave save, string id) => save != null && save.Settings.unlocksSeen != null && save.Settings.unlocksSeen.Contains(id);
        // the moment it is earned: queued, and kept in the settings until it has been shown
        public static void Raise(RacerSave save, Notice n)
        {
            if (n == null || Seen(save, n.id)) return;
            if (!earnedForLog.Contains(n.id)) earnedForLog.Add(n.id); // 0.109 Part C: the next results-log line names it
            Queue(n);
            if (save != null && !(save.Settings.unlocksPending ?? new string[0]).Contains(n.id)) { save.Settings.unlocksPending = (save.Settings.unlocksPending ?? new string[0]).Append(n.id).ToArray(); save.SaveSettings(); }
        }
        static readonly List<string> earnedForLog = new();
        public static string[] TakeEarnedForLog() { var a = earnedForLog.ToArray(); earnedForLog.Clear(); return a; }
        static void Queue(Notice n) { if (n != null && !Pending.Any(p => p.id == n.id) && Showing?.id != n.id) Pending.Add(n); }
        public static Notice Acorns(int total) => new() { id = AcornMower, heading = $"Found all {total} Woodland Acorns", vehicle = "mower" };
        public static Notice For(string id)
        {
            if (id == AcornMower) return Acorns(VehicleUnlocks.AcornsNeeded);
            if (id == PoliceCar) return new() { id = id, heading = $"Escaped the police at heat {PoliceProgress.HeatNeeded}+", vehicle = VehicleProfile.Police.Id };
            if (id == PoliceBike) return new() { id = id, heading = $"Caught {PoliceProgress.SpeedersNeeded} speeders in Speed Patrol", vehicle = VehicleProfile.PoliceBike.Id };
            if (id == StuntBike) return new() { id = id, heading = "Won a Gold medal in the Stunt event", vehicle = VehicleProfile.StuntBike.Id };
            // 0.111 Part C: "fresh-prize-<vehicle>" = the fresh run's own prize (Campaign.PrizeNotice); Part D: a championship's Cup edition
            bool fresh = id != null && id.StartsWith(Campaign.FreshNotice + Prize);
            if (id != null && (fresh || id.StartsWith(Prize)))
            {
                string vehicle = id.Substring((fresh ? Campaign.FreshNotice : "").Length + Prize.Length); var e = Campaign.PrizeEvent(vehicle); var cup = Campaign.PrizeCup(vehicle); if (VehicleProfile.All.All(p => p.Id != vehicle)) return null;
                string how = e != null ? $"Passed the {e.Name} (campaign chapter {e.Chapter})" : cup != null ? $"Won the {cup.Name} (campaign championship)" : "A campaign prize";
                return new() { id = id, heading = how + (fresh ? "  ·  FRESH RUN" : ""), vehicle = vehicle };
            }
            return null;
        }
        // at load: earned but never shown (the vehicle goals), and any raised but not yet shown (settings); loading never writes
        public static void Missed(RacerSave save)
        {
            void Again(string id) { if (!Seen(save, id)) Queue(For(id)); }
            if (VehicleUnlocks.RewardEarned) Again(AcornMower);
            if (PoliceProgress.CarEarned) Again(PoliceCar);
            if (PoliceProgress.BikeEarned) Again(PoliceBike);
            if (StuntProgress.GoldEarned) Again(StuntBike);
            foreach (var id in save?.Settings.unlocksPending ?? new string[0]) Again(id);
            // 0.111 Part D: a Cup edition the active campaign slot holds whose congratulations was never shown (championships won before 0.111)
            foreach (var c in CampaignData.Cups) if (c.Prize != null && Campaign.Owns(c.Prize) && Campaign.CupResult(c.Id)?.won == true && !Campaign.Testing) Again(Campaign.PrizeNotice(c.Prize));
        }
        public static void MarkSeen(RacerSave save, string id)
        {
            if (save == null) return;
            if (!Seen(save, id)) save.Settings.unlocksSeen = (save.Settings.unlocksSeen ?? new string[0]).Append(id).ToArray();
            save.Settings.unlocksPending = (save.Settings.unlocksPending ?? new string[0]).Where(p => p != id).ToArray(); save.SaveSettings();
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
