using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Racer
{
    // 0.90 Part D: the HUD for each half in split-screen: lap, position, lap and race time (top left), speed (bottom right),
    // the countdown, GO, wrong way with the reset button, recovering and finished (centre), and a small minimap showing
    // both players (top right); sized for the half's shape. No campaign, hint or Free Roam text. The single-player HUD
    // hides itself while split-screen is on.
    public sealed class SplitHud : MonoBehaviour
    {
        sealed class Half
        {
            public RectTransform root, captionRoot; public Text info, speed, centre, tag, gaps, grid, caption, heat, alert, radio; public RacingMiniMap map; public GameObject infoPanel, speedPanel, gapsPanel, gridPanel;
        }
        RaceFlow flow; SplitRace split; Font font; readonly Half[] halves = new Half[2]; GameObject divider; RectTransform canvas;
        float goUntil; RaceFlow.Stage lastStage;
        public void Initialize(RaceFlow owner, SplitRace race)
        {
            flow = owner; split = race; var hud = FindAnyObjectByType<RaceHud>(); canvas = (RectTransform)hud.transform; font = hud.display.font;
            for (int i = 0; i < 2; i++) halves[i] = Build(i);
            divider = new GameObject("Split-screen divider", typeof(RectTransform), typeof(Image)); divider.transform.SetParent(canvas, false);
            divider.GetComponent<Image>().color = new Color(.02f, .03f, .04f, 1); divider.GetComponent<Image>().raycastTarget = false;
        }
        Text Label(Transform parent, int size, TextAnchor anchor, Color colour)
        {
            var t = new GameObject("Text", typeof(RectTransform), typeof(Text)).GetComponent<Text>(); t.transform.SetParent(parent, false);
            t.font = font; t.fontSize = size; t.alignment = anchor; t.color = colour; t.raycastTarget = false; t.supportRichText = true;
            var o = t.gameObject.AddComponent<Outline>(); o.effectColor = new Color(0, 0, 0, .85f); o.effectDistance = new Vector2(1.4f, -1.4f); return t;
        }
        // shrink a one-line label until it fits the width (never clipped, never wrapped)
        static void Fit(Text t, float avail) { float w = t.preferredWidth; if (w > avail) t.fontSize = Mathf.Max(11, Mathf.FloorToInt(t.fontSize * avail / w)); }
        static void Wide(Text t, float top, float height) { var r = t.rectTransform; r.anchorMin = new Vector2(0, 1); r.anchorMax = new Vector2(1, 1); r.pivot = new Vector2(.5f, 1); r.anchoredPosition = new Vector2(0, top); r.sizeDelta = new Vector2(-40, height); }
        Half Build(int player)
        {
            var h = new Half();
            h.root = new GameObject("Player " + (player + 1) + " HUD", typeof(RectTransform)).GetComponent<RectTransform>(); h.root.SetParent(canvas, false);
            h.infoPanel = new GameObject("Race info", typeof(RectTransform), typeof(Image)); var r = (RectTransform)h.infoPanel.transform; r.SetParent(h.root, false);
            r.anchorMin = r.anchorMax = r.pivot = new Vector2(0, 1); r.anchoredPosition = new Vector2(14, -14); r.sizeDelta = new Vector2(290, 104);
            h.infoPanel.GetComponent<Image>().color = new Color(.025f, .055f, .07f, .84f); h.infoPanel.GetComponent<Image>().raycastTarget = false;
            h.info = Label(r, 20, TextAnchor.UpperLeft, Color.white); h.info.rectTransform.anchorMin = Vector2.zero; h.info.rectTransform.anchorMax = Vector2.one; h.info.rectTransform.offsetMin = new Vector2(10, 6); h.info.rectTransform.offsetMax = new Vector2(-8, -6);
            h.speedPanel = new GameObject("Speed", typeof(RectTransform), typeof(Image)); var s = (RectTransform)h.speedPanel.transform; s.SetParent(h.root, false);
            s.anchorMin = s.anchorMax = s.pivot = new Vector2(1, 0); s.anchoredPosition = new Vector2(-14, 14); s.sizeDelta = new Vector2(150, 54);
            h.speedPanel.GetComponent<Image>().color = new Color(.025f, .055f, .07f, .84f); h.speedPanel.GetComponent<Image>().raycastTarget = false;
            h.speed = Label(s, 28, TextAnchor.MiddleCenter, Color.white); h.speed.rectTransform.anchorMin = Vector2.zero; h.speed.rectTransform.anchorMax = Vector2.one; h.speed.rectTransform.offsetMin = h.speed.rectTransform.offsetMax = Vector2.zero;
            h.centre = Label(h.root, 30, TextAnchor.MiddleCenter, new Color(.4f, 1, .85f)); h.centre.rectTransform.anchorMin = new Vector2(.1f, .3f); h.centre.rectTransform.anchorMax = new Vector2(.9f, .7f); h.centre.rectTransform.offsetMin = h.centre.rectTransform.offsetMax = Vector2.zero;
            h.tag = Label(h.root, 18, TextAnchor.LowerLeft, player == 0 ? new Color(.3f, .95f, .81f) : new Color(1, .74f, .25f)); h.tag.rectTransform.anchorMin = h.tag.rectTransform.anchorMax = h.tag.rectTransform.pivot = Vector2.zero; h.tag.rectTransform.anchoredPosition = new Vector2(16, 12); h.tag.rectTransform.sizeDelta = new Vector2(300, 26);
            // 0.94 Part A: the driver ahead / behind with the gap (under the race panel), the starting grid (countdown) and the
            // winner's name over a player's own winner shot
            h.gapsPanel = new GameObject("Ahead and behind", typeof(RectTransform), typeof(Image)); var g = (RectTransform)h.gapsPanel.transform; g.SetParent(h.root, false);
            g.anchorMin = g.anchorMax = g.pivot = new Vector2(0, 1); g.anchoredPosition = new Vector2(14, -122); g.sizeDelta = new Vector2(290, 54);
            h.gapsPanel.GetComponent<Image>().color = new Color(.025f, .055f, .07f, .72f); h.gapsPanel.GetComponent<Image>().raycastTarget = false;
            h.gaps = Label(g, 18, TextAnchor.MiddleLeft, Color.white); h.gaps.rectTransform.anchorMin = Vector2.zero; h.gaps.rectTransform.anchorMax = Vector2.one; h.gaps.rectTransform.offsetMin = new Vector2(10, 2); h.gaps.rectTransform.offsetMax = new Vector2(-8, -2);
            h.gridPanel = new GameObject("Starting grid", typeof(RectTransform), typeof(Image)); var gr = (RectTransform)h.gridPanel.transform; gr.SetParent(h.root, false);
            gr.anchorMin = gr.anchorMax = gr.pivot = new Vector2(0, 1); gr.anchoredPosition = new Vector2(14, -126); gr.sizeDelta = new Vector2(330, 160);
            h.gridPanel.GetComponent<Image>().color = new Color(.025f, .055f, .07f, .84f); h.gridPanel.GetComponent<Image>().raycastTarget = false;
            h.grid = Label(gr, 17, TextAnchor.UpperLeft, Color.white); h.grid.rectTransform.anchorMin = Vector2.zero; h.grid.rectTransform.anchorMax = Vector2.one; h.grid.rectTransform.offsetMin = new Vector2(10, 6); h.grid.rectTransform.offsetMax = new Vector2(-8, -6);
            // 0.98 Getaway: the heat big at the top centre, the alert for each rise under it, and the police radio as one outlined line (no box) under that
            h.heat = Label(h.root, 44, TextAnchor.UpperCenter, new Color(1f, .78f, .28f)); Wide(h.heat, -10, 64);
            h.alert = Label(h.root, 28, TextAnchor.UpperCenter, new Color(1f, .45f, .3f)); Wide(h.alert, -74, 40);
            h.radio = Label(h.root, 24, TextAnchor.UpperCenter, new Color(.62f, .8f, 1f)); Wide(h.radio, -116, 36); h.radio.horizontalOverflow = HorizontalWrapMode.Overflow; h.radio.verticalOverflow = VerticalWrapMode.Overflow;
            h.heat.horizontalOverflow = h.alert.horizontalOverflow = HorizontalWrapMode.Overflow; h.heat.verticalOverflow = h.alert.verticalOverflow = VerticalWrapMode.Overflow;
            h.heat.gameObject.SetActive(false); h.alert.gameObject.SetActive(false); h.radio.gameObject.SetActive(false);
            h.captionRoot = new GameObject("Player " + (player + 1) + " winner caption", typeof(RectTransform)).GetComponent<RectTransform>(); h.captionRoot.SetParent(canvas, false);
            h.caption = Label(h.captionRoot, 30, TextAnchor.UpperCenter, new Color(.4f, 1, .85f)); h.caption.rectTransform.anchorMin = new Vector2(.1f, .7f); h.caption.rectTransform.anchorMax = new Vector2(.9f, .95f); h.caption.rectTransform.offsetMin = h.caption.rectTransform.offsetMax = Vector2.zero;
            h.map = RacingMiniMap.Create(h.root, flow.Race, font); h.map.Split = true;
            var panel = (RectTransform)h.map.transform.parent.parent; panel.localScale = Vector3.one * .78f;
            return h;
        }
        public void Layout(bool leftRight)
        {
            bool one = SplitScreen.OneView; // 0.95 Part B: a solo Police Chase: player 1's HUD on the whole screen, no divider
            for (int i = 0; i < 2; i++)
            {
                bool watched = SplitScreen.Watching == i + 1;
                foreach (var r in new[] { halves[i].root, halves[i].captionRoot })
                {
                    r.anchorMin = one || watched ? Vector2.zero : leftRight ? new Vector2(i * .5f, 0) : new Vector2(0, i == 0 ? .5f : 0);
                    r.anchorMax = one || watched ? Vector2.one : leftRight ? new Vector2(i * .5f + .5f, 1) : new Vector2(1, i == 0 ? 1 : .5f);
                    r.offsetMin = r.offsetMax = Vector2.zero;
                }
                var map = (RectTransform)halves[i].map.transform.parent.parent; map.localScale = Vector3.one * (one || watched ? 1 : .78f);
            }
            var d = (RectTransform)divider.transform;
            d.anchorMin = leftRight ? new Vector2(.5f, 0) : new Vector2(0, .5f); d.anchorMax = leftRight ? new Vector2(.5f, 1) : new Vector2(1, .5f);
            d.sizeDelta = leftRight ? new Vector2(4, 0) : new Vector2(0, 4); d.anchoredPosition = Vector2.zero;
        }
        void LateUpdate()
        {
            if (!flow || !split) return;
            var race = flow.Race; bool driving = !flow.MenuVisible;
            divider.SetActive(!SplitScreen.OneView && SplitScreen.Watching == 0); divider.transform.SetAsLastSibling();
            if (flow.State == RaceFlow.Stage.Racing && lastStage == RaceFlow.Stage.Countdown) goUntil = Time.unscaledTime + 1.5f;
            lastStage = flow.State;
            if (SplitRoam.Current) { Roam(driving); return; }
            for (int i = 0; i < 2; i++)
            {
                var h = halves[i]; var state = i == 0 ? race.Racers[0] : split.P2; var car = i == 0 ? race.vehicle : split.P2Car;
                h.root.gameObject.SetActive(driving && state != null && car && !split.ShotActive(i + 1)); // 0.92 Part F: not over the winner shot
                h.captionRoot.gameObject.SetActive(driving && split.ShotActive(i + 1)); if (h.captionRoot.gameObject.activeSelf) h.caption.text = "WINNER\n" + state.Name;
                if (!h.root.gameObject.activeSelf) continue;
                // 0.95 Part C: in split-screen only the other person is named: no starting grid card, and the ahead / behind
                // line only when it is the other player (never an AI rival or the AI player 2)
                h.gridPanel.SetActive(false);
                var person = SplitScreen.P2Ai ? null : i == 0 ? split.P2 : race.Racers[0];
                string gaps = flow.State == RaceFlow.Stage.Racing && person != null ? string.Join("\n", RaceNames.AheadBehind(race, state).Split('\n').Where(l => l.Contains(" " + person.Name + "  "))) : "";
                h.gapsPanel.SetActive(gaps != ""); if (gaps != "") { h.gaps.text = gaps; ((RectTransform)h.gapsPanel.transform).sizeDelta = new Vector2(290, gaps.Contains("\n") ? 54 : 30); }
                h.map.Focus = car;
                var p = state.Progress; int position = race.Ordered(false).IndexOf(state) + 1;
                h.tag.text = (i == 0 ? "PLAYER 1 · " : "PLAYER 2 · ") + SplitScreen.NameOf(i + 1);
                h.info.text = $"LAP {Mathf.Min(p.CompletedLaps + 1, p.TargetLaps)}/{p.TargetLaps}     POS {position}/{race.Racers.Count}\nLap   {RaceHud.FormatTime(p.Finished ? p.LastLap : p.LapTime(race.Clock))}\nRace  {RaceHud.FormatTime(p.RaceTime(race.Clock))}" + (p.PenaltySeconds > 0 ? $"   +{p.PenaltySeconds:0}s" : "");
                h.speed.text = $"{DisplayUnits.Mph(Mathf.Abs(car.ForwardSpeed)):0} <size=16>mph</size>";
                string centre = "";
                var guidance = i == 0 ? race.GetComponent<WrongWayGuidance>() : split.P2Guidance; var respawn = car.GetComponent<VehicleRespawn>(); var input = car.GetComponent<VehicleInput>();
                if (flow.State == RaceFlow.Stage.Countdown) centre = Mathf.CeilToInt(flow.CountdownRemaining).ToString();
                else if (p.Finished) centre = $"FINISHED   {Campaign.Ordinal(race.Ordered(true).IndexOf(state) + 1)}\n<size=20>{RaceHud.FormatTime(state.ClassifiedTime(race.Clock))}</size>";
                else if (Time.unscaledTime < goUntil) centre = "GO!";
                else if (respawn && respawn.enabled && respawn.Pending) centre = "<size=22>Recovering…</size>";
                else if (guidance && guidance.Visible) centre = $"<color=#FFD659>WRONG WAY</color>\n<size=20>{(input ? input.ResetControlLabel : "R")}: reset to the track</size>";
                else if (i == 0 && flow.PenaltyNotice != null) centre = "<size=20>" + flow.PenaltyNotice + "</size>";
                h.centre.text = centre; h.centre.fontSize = flow.State == RaceFlow.Stage.Countdown ? 64 : 30;
            }
        }
        // 0.94 Parts B and C: Free Roam for two / Police Chase: each half shows its player's name (and role, the round clock and
        // the bust meter in a chase), the direction and distance to the other player, the player's own jump and speed-trap
        // results, the speed and the minimap; the centre shows the chase's start, GO and the round's end.
        void Roam(bool driving)
        {
            var race = flow.Race; var roam = SplitRoam.Current; var police = PoliceChase.Current; var patrol = SpeedPatrol.Current; var getaway = GetawayChase.Current;
            for (int i = 0; i < 2; i++)
            {
                var h = halves[i]; int player = i + 1; var car = i == 0 ? race.vehicle : split.P2Car; var other = i == 0 ? split.P2Car : race.vehicle;
                bool one = SplitScreen.OneView; if (one && i == 1) car = null; if (SplitScreen.Watching != 0 && SplitScreen.Watching != player) car = null; var runner = getaway != null ? getaway.RunnerOf(player) : null; if (getaway != null && (runner == null || !runner.human)) car = null; // 0.95 Part B: one view (the AI has no half)
                h.root.gameObject.SetActive(driving && car); h.captionRoot.gameObject.SetActive(false); h.gridPanel.SetActive(false); h.gapsPanel.SetActive(false);
                if (!car || !driving) continue;
                h.map.Focus = car;
                var cam = i == 0 ? Camera.main : split.Camera2;
                h.tag.text = one ? "" : (i == 0 ? "PLAYER 1 · " : "PLAYER 2 · ") + SplitScreen.NameOf(player);
                var lines = new System.Collections.Generic.List<string>();
                if (getaway != null && runner != null)
                {
                    bool ga = getaway.Mode == GetawayChase.Variant.Getaway;
                    lines.Add($"<b>{(ga ? "GETAWAY" : "<color=#FFC747>RUNNER</color>")}</b>   {RaceHud.FormatTime(getaway.Clock).Substring(0, 5)} / {getaway.Limit / 60:0}:00");
                    lines.Add($"COPS  <b>{runner.copsChasing}</b> on you   ·   {getaway.Cops.Count(c => !c.block)} out");
                    if (ga && !runner.seen) { int e = Mathf.RoundToInt(runner.escape * 10); lines.Add("ESCAPE  <color=#7FFFB0>" + new string('■', e) + "</color><color=#5A6066>" + new string('■', 10 - e) + "</color>   <color=#9CFFB0>hidden</color>"); } // 0.98: only while unseen
                    else if (ga) lines.Add("<color=#FF5A4A>SEEN</color>   get out of sight");
                    if (ga && getaway.Heli) lines.Add("AIR  " + (runner.heliSees ? "<color=#FF5A4A>helicopter has you</color>" : "<color=#FFC747>helicopter overhead</color>"));
                    int b = Mathf.RoundToInt(runner.bust * 10); lines.Add("BUST  <color=#FF5A4A>" + new string('■', b) + "</color><color=#5A6066>" + new string('■', 10 - b) + "</color>");
                }
                else if (police)
                {
                    bool cop = police.Cop == player;
                    lines.Add($"<b>{(cop ? "<color=#7FB2FF>COP</color>" : "<color=#FFC747>RUNNER</color>")}</b>   round {police.Round + 1}/{police.Rounds}   {RaceHud.FormatTime(police.Clock).Substring(0, 5)} / {police.Limit / 60:0}:00");
                    int filled = Mathf.RoundToInt(police.Meter * 10);
                    lines.Add("BUST  <color=#FF5A4A>" + new string('■', filled) + "</color><color=#5A6066>" + new string('■', 10 - filled) + "</color>");
                }
                else if (patrol)
                {
                    // 0.95 Part G: the clock, the score, the road's limit, the radar (red over the limit), the pull-over meter
                    var cop = patrol.CopOf(player); float left = Mathf.Max(0, patrol.Limit - patrol.Clock); int sec = Mathf.CeilToInt(left);
                    lines.Add($"<b>SPEED PATROL</b>   {sec / 60}:{sec % 60:00} left   <b>{(cop != null ? cop.points : 0)}</b> pts");
                    if (cop != null)
                    {
                        lines.Add($"LIMIT {(cop.limit > 0 ? cop.limit + " mph" : "—")}     RADAR {(cop.radar < 0 ? "—" : (cop.radarOver ? "<color=#FF4A3A>" : "<color=#9CFFB0>") + Mathf.RoundToInt(cop.radar) + "</color> mph")}");
                        float meter = patrol.MeterFor(player); if (meter > 0) { int filled = Mathf.RoundToInt(meter * 10); lines.Add("PULL OVER  <color=#FFC747>" + new string('■', filled) + "</color><color=#5A6066>" + new string('■', 10 - filled) + "</color>"); }
                    }
                }
                else lines.Add("<b>FREE ROAM</b>");
                if (other && getaway == null) lines.Add(SplitRoam.Toward(car.transform, cam ? cam.transform : car.transform, other ? other.transform : null, SplitScreen.NameOf(3 - player)));
                var acts = i == 0 ? flow.Activities : roam.Activities2; string hud = acts ? acts.Hud : "";
                if (hud.Contains("> Activities")) hud = hud.Split('\n')[0]; // timed attempts are single-player only (no Activities in the split-screen pause menu)
                h.info.text = string.Join("\n", lines); h.info.fontSize = 19;
                {
                    bool ga2 = getaway != null && runner != null && getaway.Mode == GetawayChase.Variant.Getaway && getaway.State != GetawayChase.Phase.Done; float hh = Mathf.Max(300, h.root.rect.height); float k = Mathf.Clamp(hh / 700f, .55f, 1.2f);
                    h.heat.gameObject.SetActive(ga2); h.alert.gameObject.SetActive(ga2 && getaway.AlertShown); h.radio.gameObject.SetActive(ga2 && getaway.Radio != "");
                    if (ga2)
                    {
                        // a narrow half: the heat block sits under the info panel and may use the whole width; a wide view: between the info panel and the minimap
                        bool narrow = h.root.rect.width < 900; float y0 = narrow ? -(22 + 25 * lines.Count + 10) : -8, avail = Mathf.Max(220, narrow ? h.root.rect.width - 24 : h.root.rect.width - 2 * 380);
                        h.heat.fontSize = Mathf.RoundToInt(46 * k); h.heat.text = $"HEAT {getaway.Heat}   <color=#FFC747>{getaway.HeatBars}</color>"; Wide(h.heat, y0, 64 * k); Fit(h.heat, avail);
                        h.alert.fontSize = Mathf.RoundToInt(30 * k); Wide(h.alert, y0 - 62 * k, 42 * k);
                        if (getaway.AlertShown) { float age = Time.unscaledTime - getaway.AlertAt; var c = h.alert.color; c.a = Mathf.Clamp01((GetawayChase.AlertSeconds - age) * 2) * (age < .6f ? .6f + .4f * Mathf.Abs(Mathf.Sin(age * 14)) : 1); h.alert.color = c; h.alert.text = getaway.Alert; Fit(h.alert, avail); }
                        if (getaway.Radio != "")
                        {
                            h.radio.fontSize = Mathf.RoundToInt(26 * k); Wide(h.radio, y0 - (getaway.AlertShown ? 104 : 62) * k, 38 * k); h.radio.text = "<color=#7FB2FF>RADIO</color>  " + getaway.Radio; Fit(h.radio, avail);
                        }
                    }
                }
                ((RectTransform)h.infoPanel.transform).sizeDelta = new Vector2(330, 22 + 25 * lines.Count);
                if (getaway != null && !SplitScreen.OneView && h.map) { var mp = (RectTransform)h.map.transform.parent.parent; mp.anchorMin = mp.anchorMax = mp.pivot = new Vector2(1, 0); mp.anchoredPosition = new Vector2(-14, 78); } // a half view: the map sits above the speed, clear of the chase panel
                h.speed.text = $"{DisplayUnits.Mph(Mathf.Abs(car.ForwardSpeed)):0} <size=16>mph</size>";
                string centre = "";
                var respawn = car.GetComponent<VehicleRespawn>();
                if (getaway != null && runner != null)
                {
                    if (getaway.State == GetawayChase.Phase.Starting) centre = getaway.Clock <= 0 ? "<size=26>YOU ARE THE RUNNER</size>\nget away!" : getaway.Clock < 1.5f ? "GO!" : "";
                    else if (runner.caught) centre = "<color=#FF5A4A>CAUGHT!</color>\n<size=22>" + RaceHud.FormatTime(runner.freeSeconds) + "</size>";
                    else if (runner.escaped) centre = "<color=#7FFFB0>" + (runner.limit ? "TIME UP: ESCAPED" : "ESCAPED!") + "</color>\n<size=22>" + RaceHud.FormatTime(runner.freeSeconds) + "</size>";
                    else if (getaway.Holds(car)) centre = "<size=22>Reset: held for 2 s</size>";
                }
                else if (police)
                {
                    bool cop = police.Cop == player; float t = police.Clock;
                    if (police.State == PoliceChase.Phase.Starting) centre = police.Clock <= 0 ? (cop ? "<size=26>YOU ARE THE COP</size>\nwait for the runner" : "<size=26>YOU ARE THE RUNNER</size>\nget away!") : cop ? $"GO IN {Mathf.CeilToInt(PoliceChase.CopDelay - t)}" : (t < 1.5f ? "GO!" : "");
                    else if (police.State == PoliceChase.Phase.Running && cop && t < PoliceChase.CopDelay + 1.5f) centre = "GO!";
                    else if (police.State == PoliceChase.Phase.Caught) centre = "<color=#FF5A4A>CAUGHT!</color>\n<size=22>" + RaceHud.FormatTime(police.Clock) + "</size>";
                    else if (police.State == PoliceChase.Phase.Away) centre = "<color=#7FFFB0>GOT AWAY!</color>\n<size=22>" + RaceHud.FormatTime(police.Clock) + "</size>";
                    else if (!cop && police.Holds(car)) centre = "<size=22>Reset: held for 2 s</size>";
                    else if (cop && police.State == PoliceChase.Phase.Running && Time.unscaledTime - roam.StartedAt < 12) centre = "<size=20>" + (SplitScreen.DeviceOf(player) is UnityEngine.InputSystem.Keyboard ? "H" : "RB") + ": siren on / off</size>";
                }
                else if (patrol)
                {
                    var cop = patrol.CopOf(player);
                    if (patrol.State == SpeedPatrol.Phase.Starting) centre = "<size=26>SPEED PATROL</size>\n<size=20>point the radar at a car to clock it · " + (SplitScreen.DeviceOf(player) is UnityEngine.InputSystem.Keyboard ? "H" : "RB") + ": lights</size>";
                    else if (cop != null && Time.unscaledTime < cop.lineUntil) centre = "<size=24>" + cop.line + "</size>";
                    else if (cop != null && patrol.Holds(car)) centre = "<size=22>Reset: held for 2 s</size>";
                }
                else if (roam.Car && Time.unscaledTime - roam.StartedAt < 7) centre = "<size=20>Split-screen Free Roam: nothing is recorded\n(no acorns, activity records or map discovery)</size>";
                if (centre == "" && respawn && respawn.Pending) centre = "<size=22>Recovering…</size>";
                if (centre == "" && !string.IsNullOrEmpty(hud)) centre = "<size=19>" + hud + "</size>";
                h.centre.text = centre; h.centre.fontSize = 30;
            }
        }
        public void Teardown()
        {
            foreach (var h in halves) { if (h != null && h.root) Destroy(h.root.gameObject); if (h != null && h.captionRoot) Destroy(h.captionRoot.gameObject); }
            if (divider) Destroy(divider);
        }
    }
}
