using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Racer
{
    // 0.94 Part A: the racers' names where they can be seen: the starting grid card during the countdown, the driver directly
    // ahead and behind with the gap, and name tags over the other racers (NameTags below).
    public static class RaceNames
    {
        // The grid, front first (by the road distance before the start line), during the countdown.
        public static List<RacerState> Grid(RaceDirector race)
        {
            if (!race || !race.road || race.Racers.Count < 2) return new List<RacerState>();
            float L = race.road.Length;
            float Ahead(RacerState r) { if (!r.Car) return float.MinValue; float rel = race.road.Relative(race.road.Project(r.Car.transform.position, out _), race.Origin); return rel > L * .5f ? rel - L : rel; }
            return race.Racers.Where(r => r.Car).OrderByDescending(Ahead).ToList();
        }
        public static string Vehicle(RacerState r) { var c = r.Car ? r.Car.GetComponent<VehicleConfiguration>() : null; return c ? c.Profile.Name : ""; }
        // "STARTING GRID" then one line per car, front first; the given players' lines in their colour.
        public static string GridText(RaceDirector race, params RacerState[] players)
        {
            var grid = Grid(race); if (grid.Count < 2) return "";
            var lines = new List<string> { "<b>STARTING GRID</b>" };
            for (int i = 0; i < grid.Count; i++)
            {
                var r = grid[i]; string line = $"{i + 1}   {r.Name}   <size=16>{Vehicle(r)}</size>";
                lines.Add(players.Contains(r) ? $"<color=#FFC747>{line}</color>" : line);
            }
            return string.Join("\n", lines);
        }
        // The driver directly ahead and directly behind with the time gap ("▲ EMBER +1.2 s" / "▼ ROOK −0.8 s").
        public static string AheadBehind(RaceDirector race, RacerState me)
        {
            if (!race || me == null || race.Racers.Count < 2 || race.FreeRoam || me.Progress.Finished) return "";
            var gaps = Gaps.For(race); var order = race.Ordered(false); int i = order.IndexOf(me); if (i < 0) return "";
            var lines = new List<string>();
            if (i > 0) { var a = order[i - 1]; var g = gaps.Gap(a, race.Score(me)); lines.Add($"▲ {a.Name}  {(g.HasValue ? "+" + g.Value.ToString("0.0") + " s" : "")}"); }
            if (i < order.Count - 1) { var b = order[i + 1]; var g = gaps.Gap(me, race.Score(b)); lines.Add($"▼ {b.Name}  {(g.HasValue ? "−" + g.Value.ToString("0.0") + " s" : "")}"); }
            return string.Join("\n", lines);
        }
    }

    // Interval timing: each racer's progress (RaceDirector.Score) sampled every 0.1 s of race clock. The gap from a racer to a
    // point of progress = now - the time that racer reached it.
    public sealed class Gaps : MonoBehaviour
    {
        RaceDirector race; readonly Dictionary<RacerState, List<(float score, double t)>> samples = new(); double last = -1;
        public static Gaps For(RaceDirector race) { var g = race.GetComponent<Gaps>(); if (!g) { g = race.gameObject.AddComponent<Gaps>(); g.race = race; } return g; }
        void FixedUpdate()
        {
            if (!race || race.FreeRoam || race.Flow == null || race.Flow.State != RaceFlow.Stage.Racing) { if (race && race.Flow != null && race.Flow.State == RaceFlow.Stage.Countdown) { samples.Clear(); last = -1; } return; }
            double now = race.Clock; if (now < last) { samples.Clear(); last = -1; }
            if (last >= 0 && now - last < .1) return; last = now;
            foreach (var r in race.Racers) { if (!samples.TryGetValue(r, out var list)) samples[r] = list = new(); float s = race.Score(r); if (list.Count == 0 || s >= list[^1].score) list.Add((s, now)); }
            foreach (var k in samples.Keys.Where(k => !race.Racers.Contains(k)).ToList()) samples.Remove(k);
        }
        public double? Gap(RacerState racer, float score)
        {
            if (!samples.TryGetValue(racer, out var list) || list.Count == 0 || list[^1].score < score) return null;
            int lo = 0, hi = list.Count - 1; while (lo < hi) { int mid = (lo + hi) / 2; if (list[mid].score >= score) hi = mid; else lo = mid + 1; }
            double t = list[lo].t; if (lo > 0) { var a = list[lo - 1]; var b = list[lo]; float span = b.score - a.score; if (span > .01f) t = a.t + (b.t - a.t) * (score - a.score) / span; }
            return System.Math.Max(0, race.Clock - t);
        }
    }

    // Name tags over the other racers (never your own): Settings > Gameplay "Name tags" Off / Players only / Everyone. Facing
    // the camera (drawn on the screen over the vehicle), fading out from 45 to 60 m, hidden behind solid scenery, not in
    // Trailer Mode or a winner shot. In split-screen each half draws its own (the other player shows in your half). People
    // gold, AI drivers white-blue.
    public sealed class NameTags : MonoBehaviour
    {
        public struct Target { public ArcadeVehicle car; public string name; public bool human, player; }
        RaceFlow flow; RectTransform root; Font font; readonly List<(RectTransform rect, Image back, Text text)> pool = new();
        public static System.Func<List<Target>> Extra; // more targets (split-screen Free Roam / Police Chase players)
        public static int Shown { get; private set; }
        public static bool Trace; public static string LastTrace = ""; // checks: why each target was or was not tagged
        public static NameTags Attach(RaceFlow owner)
        {
            var tags = owner.GetComponent<NameTags>(); if (!tags) tags = owner.gameObject.AddComponent<NameTags>(); tags.flow = owner; return tags;
        }
        void Build()
        {
            var go = new GameObject("Name tags", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler)); go.transform.SetParent(transform, false);
            var canvas = go.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = -10;
            var scaler = go.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1920, 1080); scaler.matchWidthOrHeight = 1;
            root = (RectTransform)go.transform; font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }
        (RectTransform rect, Image back, Text text) Tag(int i)
        {
            while (pool.Count <= i)
            {
                var r = new GameObject("Tag", typeof(RectTransform), typeof(Image)).GetComponent<RectTransform>(); r.SetParent(root, false); r.pivot = new Vector2(.5f, 0);
                var back = r.GetComponent<Image>(); back.raycastTarget = false;
                var t = new GameObject("Name", typeof(RectTransform), typeof(Text)).GetComponent<Text>(); t.transform.SetParent(r, false);
                t.font = font; t.fontSize = 22; t.alignment = TextAnchor.MiddleCenter; t.raycastTarget = false; t.horizontalOverflow = HorizontalWrapMode.Overflow;
                t.rectTransform.anchorMin = Vector2.zero; t.rectTransform.anchorMax = Vector2.one; t.rectTransform.offsetMin = t.rectTransform.offsetMax = Vector2.zero;
                var o = t.gameObject.AddComponent<Outline>(); o.effectColor = new Color(0, 0, 0, .9f); o.effectDistance = new Vector2(1.2f, -1.2f);
                pool.Add((r, back, t));
            }
            return pool[i];
        }
        List<Target> Targets()
        {
            var list = new List<Target>(); var race = flow.Race; var split = SplitScreen.Race;
            if (!race.FreeRoam) foreach (var r in race.Racers) if (r.Car) list.Add(new Target { car = r.Car, name = r.Name, human = !r.IsAi || (split && r == split.P2 && !SplitScreen.P2Ai), player = r == race.Racers[0] || (split && r == split.P2) });
            if (Extra != null) foreach (var t in Extra()) if (t.car && !list.Any(x => x.car == t.car)) list.Add(t);
            return list;
        }
        void LateUpdate()
        {
            if (!flow || flow.Save == null) return;
            if (!root) Build();
            int mode = Mathf.Clamp(flow.Save.Settings.nameTags, 0, 2), used = 0; string trace = Trace ? $"mode {mode} state {flow.State} menu {flow.MenuVisible} views {SplitScreen.Views.Count} " : null;
            bool driving = flow.State == RaceFlow.Stage.Racing || flow.State == RaceFlow.Stage.Countdown;
            if (mode > 0 && driving && !flow.MenuVisible && !TrailerMode.Active && !(WinnerShot.Active && !SplitScreen.Active))
            {
                var split = SplitScreen.Race; var targets = Targets();
                var views = SplitScreen.Views.Count > 0 ? SplitScreen.Views.ToList() : new List<Camera> { Camera.main };
                for (int v = 0; v < views.Count; v++)
                {
                    var cam = views[v]; if (!cam) continue;
                    if (split && split.ShotActive(v + 1)) continue;
                    var own = v == 0 ? flow.Race.vehicle : split ? split.P2Car : null;
                    var px = cam.pixelRect; var placed = new List<Rect>(); var near = new List<(Target t, Vector3 sp, float d)>();
                    foreach (var t in targets)
                    {
                        if (t.car == own || (mode == 1 && !t.player)) continue;
                        var size = t.car.GetComponent<VehicleConfiguration>()?.Profile.Size ?? new Vector3(1, 1, 3);
                        var head = t.car.transform.position + t.car.transform.up * (size.y + 1.45f);
                        float d = Vector3.Distance(cam.transform.position, head); var sp = cam.WorldToScreenPoint(head);
                        if (Trace) trace += $"[view {v + 1} {t.name}: {d:F0} m, screen {sp.x:F0},{sp.y:F0},{sp.z:F1} in {px} {px.Contains(sp)}, hidden {Hidden(cam.transform.position, head)}] ";
                        if (d > 60) continue;
                        if (sp.z <= 0 || !px.Contains(sp)) continue;
                        if (Hidden(cam.transform.position, head)) continue;
                        near.Add((t, sp, d));
                    }
                    // nearest first; a tag that would overlap one already placed moves up above it
                    foreach (var (t, sp, d) in near.OrderBy(n => n.d))
                    {
                        float alpha = 1 - Mathf.InverseLerp(45, 60, d);
                        var tag = Tag(used++); tag.rect.gameObject.SetActive(true);
                        RectTransformUtility.ScreenPointToLocalPointInRectangle(root, sp, null, out var local);
                        tag.text.text = t.name; var colour = t.human ? PlayerNames.Human : PlayerNames.Machine; colour.a = alpha; tag.text.color = colour;
                        tag.back.color = new Color(.02f, .04f, .05f, .55f * alpha);
                        var box = new Vector2(tag.text.preferredWidth + 18, 30);
                        for (int k = 0; k < 6 && placed.Any(r => r.Overlaps(new Rect(local.x - box.x * .5f, local.y, box.x, box.y))); k++) local.y += box.y + 2;
                        placed.Add(new Rect(local.x - box.x * .5f, local.y, box.x, box.y));
                        tag.rect.anchorMin = tag.rect.anchorMax = new Vector2(.5f, .5f); tag.rect.anchoredPosition = local; tag.rect.sizeDelta = box;
                    }
                }
            }
            Shown = used; if (Trace) LastTrace = trace;
            for (int i = used; i < pool.Count; i++) if (pool[i].rect.gameObject.activeSelf) pool[i].rect.gameObject.SetActive(false);
        }
        // hidden behind solid scenery (vehicles and triggers do not hide a tag)
        static bool Hidden(Vector3 from, Vector3 to)
        {
            var dir = to - from; float len = dir.magnitude; if (len < .1f) return false;
            foreach (var h in Physics.RaycastAll(from, dir / len, len - .3f, ~0, QueryTriggerInteraction.Ignore))
                if (!h.collider.GetComponentInParent<ArcadeVehicle>() && !h.collider.attachedRigidbody) return true;
            return false;
        }
    }
}
