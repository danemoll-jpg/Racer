using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Racer
{
    // 0.99 Part D: hidden police in single-player Free Roam. 10-14 hiding places lie beside the roads that have a speed limit (found once per
    // session from the road network: a flat, clear verge pull-off, the patrol car parked nose to the road, lights off). Each session uses two to four
    // of them at random and moves one now and then while the player cannot see it. A hidden cop sees the player within 70 m in front of it with a
    // clear line of sight; if the player is on the road and over the road's limit by more than 10 mph it flashes "CLOCKED 58 in a 35", pulls out with
    // lights and siren on, and a Getaway chase starts where the player is (GetawayChase.BeginEmbedded: heat 1, the difficulty from Settings, the normal
    // escalation). Escaped: "You lost them", the cops leave; busted: "Busted", the vehicle is set at the roadside, stopped; Free Roam carries on, no money
    // or save penalty (the escapes and busts count toward the police unlocks, PoliceProgress). Never: during a timed Free Roam activity, in the first 60 s
    // after loading, within 3 minutes of the last chase, in split-screen, in Trailer Mode, or with Settings > Gameplay > Hidden police Off.
    public sealed class HiddenPolice : MonoBehaviour
    {
        public const float SeeRange = 70, OverBy = 10, QuietStart = 60, QuietAfter = 180, SpawnDistance = 230, DespawnDistance = 300, MoveEvery = 200;
        public static HiddenPolice Current { get; private set; }
        public sealed class Spot { public Vector3 pos, forward; public string road; public ArcadeVehicle prop; public float movedAt, heardAt = -999; }
        public readonly List<Spot> All = new(), Active = new();
        RaceFlow flow; RaceDirector race; RoadNet net; float attachedAt, lastEnd = -999, nextTick, nextMove; HiddenPoliceHud hud;
        public static string Flash = ""; public static float FlashAt = -99; public static string Line = ""; public static float LineAt = -99;
        public RoadNet Net => net; public float AttachedAt { get => attachedAt; set => attachedAt = value; } public float LastEnd { get => lastEnd; set => lastEnd = value; }
        public bool ChaseRunning => GetawayChase.Current && GetawayChase.Current.Embedded;
        public static void RadioLine(params string[] ids) { Line = PoliceRadio.TextOf(ids); LineAt = Time.unscaledTime; PoliceRadio.Speak(ids); }

        // RaceFlow.BeginRoaming: once per Free Roam session (single-player Free Roam only)
        public static void Attach(RaceFlow owner)
        {
            if (SplitScreen.Active || !owner.Race.FreeRoam || TrailerMode.Active) return;
            var h = owner.GetComponent<HiddenPolice>(); if (h) return;
            owner.gameObject.AddComponent<HiddenPolice>().Initialize(owner);
        }
        void Initialize(RaceFlow owner)
        {
            flow = owner; race = owner.Race; Current = this; attachedAt = Time.time; nextMove = Time.time + MoveEvery;
            net = RoadNet.Build(race); BuildSpots(); Pick(Mathf.Clamp(Random.Range(2, 5), 1, Mathf.Max(1, All.Count)));
            hud = gameObject.AddComponent<HiddenPoliceHud>(); hud.Initialize(flow);
        }
        void OnDestroy() { foreach (var s in All) DropProp(s); if (Current == this) Current = null; }
        public static void Ended(bool caught) { if (Current) { Current.lastEnd = Time.time; Current.ReplaceUsed(); } }
        public bool Enabled => flow && flow.Save != null && flow.Save.Settings.hiddenPolice;
        public int Difficulty => flow.Save.Settings.hiddenPoliceDifficulty;

        // ---------- the hiding places ----------
        static string Group(string roadName, Vector3 p)
        {
            string n = roadName.ToLowerInvariant();
            if (n.Contains("highway")) return "Hwy 92"; if (n.Contains("phase 3")) return p.x < -560 && p.x > -665 && p.z > -560 && p.z < 545 ? "Trickum Rd" : "Street Loop"; if (n.Contains("lake")) return "Lake road";
            return null;
        }
        void BuildSpots()
        {
            var rng = new System.Random(990909); var found = new List<(Spot s, string group)>();
            for (int i = 0; i < net.Count; i += 3)
            {
                string group = Group(net.RoadNames[net.Road[i]], net.P[i]); if (group == null) continue;
                bool junction = false; foreach (int j in net.Near(net.P[i], 45)) if (net.Adj[j].Count >= 3) { junction = true; break; } if (junction) continue;
                var t = net.Tangent(i); var right = Vector3.Cross(Vector3.up, t).normalized;
                foreach (int side in new[] { -1, 1 })
                {
                    var pos = net.P[i] + right * side * (net.Half[i] + 4.2f); pos.y = net.P[i].y + 6;
                    if (!Physics.Raycast(pos + Vector3.up * 25, Vector3.down, out var hit, 60, ~0, QueryTriggerInteraction.Ignore)) continue;
                    if (hit.normal.y < .97f || hit.point.y < net.P[i].y - 1.6f || hit.point.y > net.P[i].y + 1.0f || !hit.collider.name.StartsWith("Ground_")) continue;
                    pos = hit.point; int n = net.Nearest(pos, out _, 60); if (n < 0 || net.Road[n] != net.Road[i]) continue;
                    var forward = -right * side; var rot = Quaternion.LookRotation(forward);
                    bool blocked = false; foreach (var c in Physics.OverlapBox(pos + Vector3.up * 1.1f, new Vector3(1.2f, .8f, 2.6f), rot, ~0, QueryTriggerInteraction.Ignore)) if (!c.name.StartsWith("Ground_")) { blocked = true; break; }
                    if (blocked) continue;
                    found.Add((new Spot { pos = pos, forward = forward, road = group }, group));
                }
            }
            // a spread: Hwy 92 four, the Street Loop (S Cherokee Ln included) four, Trickum Rd two, the lake road two, then whatever is left, at least 170 m apart
            var want = new Dictionary<string, int> { ["Hwy 92"] = 4, ["Street Loop"] = 4, ["Trickum Rd"] = 2, ["Lake road"] = 2 };
            foreach (var group in want.Keys.ToList())
            {
                var pool = found.Where(f => f.group == group).OrderBy(_ => rng.Next()).ToList();
                foreach (var f in pool) { if (want[group] <= 0) break; if (All.Any(s => (s.pos - f.s.pos).sqrMagnitude < 170 * 170)) continue; All.Add(f.s); want[group]--; }
            }
            foreach (var f in found.OrderBy(_ => rng.Next())) { if (All.Count >= 14) break; if (All.Count >= 12) break; if (All.Any(s => (s.pos - f.s.pos).sqrMagnitude < 170 * 170)) continue; All.Add(f.s); }
        }
        void Pick(int count) { Active.Clear(); foreach (var s in All.OrderBy(_ => Random.value).Take(count)) { Active.Add(s); s.movedAt = Time.time; } }
        // after a chase the spot that was used is dropped and another one (out of sight) takes its place
        void ReplaceUsed()
        {
            int want = Random.Range(2, 5);
            while (Active.Count < want)
            {
                var free = All.Where(s => !Active.Contains(s) && Vector3.Distance(s.pos, race.vehicle.Body.position) > 250).OrderBy(_ => Random.value).FirstOrDefault(); if (free == null) break;
                Active.Add(free); free.movedAt = Time.time;
            }
        }

        // ---------- each frame ----------
        void Update()
        {
            if (!flow || !race || !race.vehicle) return;
            if (!Enabled) { foreach (var s in All) DropProp(s); return; }
            var car = race.vehicle; var me = car.Body.position;
            // props exist only near the player
            foreach (var s in Active) { float d = Vector3.Distance(me, s.pos); if (!s.prop && d < SpawnDistance) s.prop = GetawayChase.MakePatrolProp(race, s.pos, s.forward); else if (s.prop && d > DespawnDistance) DropProp(s); }
            foreach (var s in All) if (!Active.Contains(s) && s.prop) DropProp(s);
            if (Time.time >= nextMove) { nextMove = Time.time + MoveEvery * (.75f + .5f * Random.value); MoveOne(me); }
            if (Time.unscaledTime < nextTick) return; nextTick = Time.unscaledTime + .2f;
            if (!CanTrip()) return;
            float mps = car.Body.linearVelocity.magnitude; float mph = SpeedPatrol.Mph(mps); int limit = SpeedPatrol.LimitAt(race, me, out bool onRoad);
            foreach (var s in Active)
            {
                if (!s.prop) continue; var toPlayer = me - s.prop.Body.position; toPlayer.y = 0; float d = toPlayer.magnitude;
                if (d < 35 && Time.time - s.heardAt > 150 && Random.value < .5f) { s.heardAt = Time.time; if (mph <= limit + OverBy) RadioLine("H01"); }
                if (d > SeeRange || Vector3.Dot(s.prop.transform.forward, toPlayer / Mathf.Max(.01f, d)) < .45f) continue; // within 70 m, in front of the cop
                if (!onRoad || mph <= limit + OverBy) continue;
                if (!Clear(s.prop.Body.position + Vector3.up * 1.3f, me + Vector3.up * 1f, car.transform)) continue;
                Trip(s, Mathf.RoundToInt(mph), limit); return;
            }
        }
        bool CanTrip()
        {
            if (flow.State != RaceFlow.Stage.Racing || flow.MenuVisible || !race.FreeRoam || SplitScreen.Active || TrailerMode.Active || CampaignRun.Active != null || ChaseRunning) return false;
            if (flow.Activities && flow.Activities.AttemptActive) return false;
            if (UnlockNotice.Showing != null) return false;
            return Time.time - attachedAt >= QuietStart && Time.time - lastEnd >= QuietAfter;
        }
        bool Clear(Vector3 eye, Vector3 target, Transform ignore)
        {
            var dir = target - eye; float dist = dir.magnitude; if (dist < 1) return true; var hits = Physics.RaycastAll(eye, dir / dist, dist, ~0, QueryTriggerInteraction.Ignore);
            foreach (var h in hits) { if (ignore && h.collider.transform.IsChildOf(ignore)) continue; if (h.collider.GetComponentInParent<ArcadeVehicle>()) continue; return false; }
            return true;
        }
        void Trip(Spot s, int mph, int limit)
        {
            Flash = $"CLOCKED {mph} in a {limit}"; FlashAt = Time.unscaledTime;
            var at = s.prop.Body.position; var forward = s.prop.transform.forward; DropProp(s); Active.Remove(s);
            GetawayChase.BeginEmbedded(flow, net, at, forward, Difficulty);
        }
        void MoveOne(Vector3 me)
        {
            var out1 = Active.Where(s => Vector3.Distance(s.pos, me) > 260 && !s.prop && Time.time - s.movedAt > 90).OrderBy(_ => Random.value).FirstOrDefault(); if (out1 == null) return;
            var target = All.Where(s => !Active.Contains(s) && Vector3.Distance(s.pos, me) > 260).OrderBy(_ => Random.value).FirstOrDefault(); if (target == null) return;
            Active.Remove(out1); Active.Add(target); target.movedAt = Time.time;
        }
        void DropProp(Spot s) { if (s.prop) { Destroy(s.prop.gameObject); s.prop = null; } }
    }

    // the chase's HUD in ordinary Free Roam (the split-screen HUD is not up here): the heat big at the top centre with the alert and the radio line (outlined, no box),
    // a small panel with the cops / escape / bust, and the centre messages (CLOCKED, BUSTED, YOU LOST THEM)
    public sealed class HiddenPoliceHud : MonoBehaviour
    {
        RectTransform root; Text heat, alert, radio, centre, info; GameObject panel; Font font; RaceFlow flow;
        public void Initialize(RaceFlow owner)
        {
            flow = owner; var rh = FindAnyObjectByType<RaceHud>(); var canvas = (RectTransform)rh.transform; font = rh.display.font;
            root = new GameObject("Hidden police HUD", typeof(RectTransform)).GetComponent<RectTransform>(); root.SetParent(canvas, false); root.anchorMin = Vector2.zero; root.anchorMax = Vector2.one; root.offsetMin = root.offsetMax = Vector2.zero;
            heat = Label(44, TextAnchor.UpperCenter, new Color(1f, .78f, .28f)); Wide(heat, -10, 64); alert = Label(28, TextAnchor.UpperCenter, new Color(1f, .45f, .3f)); Wide(alert, -74, 40);
            radio = Label(24, TextAnchor.UpperCenter, new Color(.62f, .8f, 1f)); Wide(radio, -116, 36); centre = Label(54, TextAnchor.MiddleCenter, Color.white);
            centre.rectTransform.anchorMin = new Vector2(.1f, .55f); centre.rectTransform.anchorMax = new Vector2(.9f, .8f); centre.rectTransform.offsetMin = centre.rectTransform.offsetMax = Vector2.zero;
            foreach (var t in new[] { heat, alert, radio }) { t.horizontalOverflow = HorizontalWrapMode.Overflow; t.verticalOverflow = VerticalWrapMode.Overflow; }
            panel = new GameObject("Chase panel", typeof(RectTransform), typeof(Image)); var r = (RectTransform)panel.transform; r.SetParent(root, false); r.anchorMin = r.anchorMax = r.pivot = new Vector2(0, 1); r.anchoredPosition = new Vector2(14, -190); r.sizeDelta = new Vector2(330, 128);
            panel.GetComponent<Image>().color = new Color(.025f, .055f, .07f, .84f); panel.GetComponent<Image>().raycastTarget = false;
            info = Label(19, TextAnchor.UpperLeft, Color.white, r); info.rectTransform.anchorMin = Vector2.zero; info.rectTransform.anchorMax = Vector2.one; info.rectTransform.offsetMin = new Vector2(10, 6); info.rectTransform.offsetMax = new Vector2(-8, -6);
        }
        Text Label(int size, TextAnchor anchor, Color colour, Transform parent = null)
        {
            var t = new GameObject("Text", typeof(RectTransform), typeof(Text)).GetComponent<Text>(); t.transform.SetParent(parent ? parent : root, false);
            t.font = font; t.fontSize = size; t.alignment = anchor; t.color = colour; t.raycastTarget = false; t.supportRichText = true;
            var o = t.gameObject.AddComponent<Outline>(); o.effectColor = new Color(0, 0, 0, .85f); o.effectDistance = new Vector2(1.4f, -1.4f); return t;
        }
        static void Wide(Text t, float top, float height) { var r = t.rectTransform; r.anchorMin = new Vector2(0, 1); r.anchorMax = new Vector2(1, 1); r.pivot = new Vector2(.5f, 1); r.anchoredPosition = new Vector2(0, top); r.sizeDelta = new Vector2(-40, height); }
        static void Fit(Text t, float avail) { float w = t.preferredWidth; if (w > avail) t.fontSize = Mathf.Max(11, Mathf.FloorToInt(t.fontSize * avail / w)); }
        void OnDestroy() { if (root) Destroy(root.gameObject); }
        void LateUpdate()
        {
            if (!flow || !root) return; bool show = flow.State == RaceFlow.Stage.Racing && !flow.MenuVisible; root.gameObject.SetActive(show); if (!show) return;
            var g = GetawayChase.Current; bool chase = g && g.Embedded && g.Runners.Count > 0; var r = chase ? g.Runners[0] : null;
            float avail = Mathf.Max(320, root.rect.width - 760); string msg = "";
            if (Time.unscaledTime - HiddenPolice.FlashAt < 2.6f) msg = "<color=#FF8A3A>" + HiddenPolice.Flash + "</color>";
            else if (chase && Time.unscaledTime - g.BannerAt < 3f) msg = g.Banner == "BUSTED" ? "<color=#FF5A4A>BUSTED</color>" : "<color=#7FFFB0>" + g.Banner + "</color>";
            centre.text = msg; centre.gameObject.SetActive(msg != "");
            heat.gameObject.SetActive(chase); panel.SetActive(chase); alert.gameObject.SetActive(chase && g.AlertShown);
            string line = chase && g.Radio != "" ? g.Radio : Time.unscaledTime - HiddenPolice.LineAt < 4f ? HiddenPolice.Line : "";
            radio.gameObject.SetActive(line != ""); if (line != "") { radio.fontSize = 26; Wide(radio, chase && g.AlertShown ? -116 : -74, 38); radio.text = "<color=#7FB2FF>RADIO</color>  " + line; Fit(radio, avail); }
            if (!chase) return;
            heat.text = $"HEAT {g.Heat}   <color=#FFC747>{g.HeatBars}</color>"; Fit(heat, avail);
            if (g.AlertShown) { float age = Time.unscaledTime - g.AlertAt; var c = alert.color; c.a = Mathf.Clamp01((GetawayChase.AlertSeconds - age) * 2) * (age < .6f ? .6f + .4f * Mathf.Abs(Mathf.Sin(age * 14)) : 1); alert.color = c; alert.text = g.Alert; Fit(alert, avail); }
            var lines = new List<string> { "<b>GETAWAY</b>   " + RaceHud.FormatTime(g.Clock).Substring(0, 5), $"COPS  <b>{r.copsChasing}</b> on you   ·   {g.Cops.Count(c => !c.block)} out" };
            if (!r.seen) { int e = Mathf.RoundToInt(r.escape * 10); lines.Add("ESCAPE  <color=#7FFFB0>" + new string('■', e) + "</color><color=#5A6066>" + new string('■', 10 - e) + "</color>   <color=#9CFFB0>hidden</color>"); } else lines.Add("<color=#FF5A4A>SEEN</color>   get out of sight");
            if (g.Heli) lines.Add("AIR  " + (r.heliSees ? "<color=#FF5A4A>helicopter has you</color>" : "<color=#FFC747>helicopter overhead</color>"));
            int b = Mathf.RoundToInt(r.bust * 10); lines.Add("BUST  <color=#FF5A4A>" + new string('■', b) + "</color><color=#5A6066>" + new string('■', 10 - b) + "</color>");
            info.text = string.Join("\n", lines); ((RectTransform)panel.transform).sizeDelta = new Vector2(330, 22 + 25 * lines.Count);
        }
    }
}
