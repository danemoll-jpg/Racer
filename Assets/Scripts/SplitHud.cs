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
            public RectTransform root; public Text info, speed, centre, tag; public RacingMiniMap map; public GameObject infoPanel, speedPanel;
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
            h.map = RacingMiniMap.Create(h.root, flow.Race, font); h.map.Split = true;
            var panel = (RectTransform)h.map.transform.parent.parent; panel.localScale = Vector3.one * .78f;
            return h;
        }
        public void Layout(bool leftRight)
        {
            for (int i = 0; i < 2; i++)
            {
                var r = halves[i].root;
                r.anchorMin = leftRight ? new Vector2(i * .5f, 0) : new Vector2(0, i == 0 ? .5f : 0);
                r.anchorMax = leftRight ? new Vector2(i * .5f + .5f, 1) : new Vector2(1, i == 0 ? 1 : .5f);
                r.offsetMin = r.offsetMax = Vector2.zero;
            }
            var d = (RectTransform)divider.transform;
            d.anchorMin = leftRight ? new Vector2(.5f, 0) : new Vector2(0, .5f); d.anchorMax = leftRight ? new Vector2(.5f, 1) : new Vector2(1, .5f);
            d.sizeDelta = leftRight ? new Vector2(4, 0) : new Vector2(0, 4); d.anchoredPosition = Vector2.zero;
        }
        void LateUpdate()
        {
            if (!flow || !split) return;
            var race = flow.Race; bool driving = !flow.MenuVisible;
            divider.SetActive(true); divider.transform.SetAsLastSibling();
            if (flow.State == RaceFlow.Stage.Racing && lastStage == RaceFlow.Stage.Countdown) goUntil = Time.unscaledTime + 1.5f;
            lastStage = flow.State;
            for (int i = 0; i < 2; i++)
            {
                var h = halves[i]; var state = i == 0 ? race.Racers[0] : split.P2; var car = i == 0 ? race.vehicle : split.P2Car;
                h.root.gameObject.SetActive(driving && state != null && car);
                if (!h.root.gameObject.activeSelf) continue;
                h.map.Focus = car;
                var p = state.Progress; int position = race.Ordered(false).IndexOf(state) + 1;
                h.tag.text = i == 0 ? "PLAYER 1" : SplitScreen.P2Ai ? "PLAYER 2 (AI)" : "PLAYER 2";
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
        public void Teardown()
        {
            foreach (var h in halves) if (h != null && h.root) Destroy(h.root.gameObject);
            if (divider) Destroy(divider);
        }
    }
}
