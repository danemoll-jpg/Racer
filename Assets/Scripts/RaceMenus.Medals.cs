using System.Linq;
using UnityEngine;

namespace Racer
{
    // 0.96 Part B: medals as medals on the menu screens (the HUD has its own in RaceHud / CampaignEventUi): the three
    // targets read highest first with the medal graphic and the value, the medal won shown as the medal, never the word.
    public sealed partial class RaceMenus
    {
        // a badge at the right end of a row's button (the row text keeps clear of it)
        void RowBadge(int index, int medal, float size = 38, float right = 14)
        {
            var b = buttons[index]; var image = MedalUi.Badge(b.transform, medal, size, "Row medal"); var r = image.rectTransform;
            r.anchorMin = r.anchorMax = r.pivot = new Vector2(1, .5f); r.anchoredPosition = new Vector2(-right, 0); tableCells.Add(image.gameObject);
            var t = b.GetComponentInChildren<UnityEngine.UI.Text>(true); if (t) t.rectTransform.offsetMax = new Vector2(-(right + size + 8), t.rectTransform.offsetMax.y);
        }
        // a badge centred in one column of a table row (the column's cell text is left empty by the caller)
        void CellBadge(int index, float[] widths, int column, int medal, float size = 24)
        {
            float x = widths.Take(column).Sum() + widths[column] / 2; var b = buttons[index]; var image = MedalUi.Badge(b.transform, medal, size, "Cell medal"); var r = image.rectTransform;
            r.anchorMin = r.anchorMax = new Vector2(x, .5f); r.pivot = new Vector2(.5f, .5f); r.anchoredPosition = Vector2.zero; tableCells.Add(image.gameObject);
        }
        // one campaign event's medal rows: targets (highest first), what each medal pays, the best so far
        void EventMedalRows(Transform parent, CampaignEvent e, int fontSize, float icon)
        {
            var font = details.font; var colour = new Color(.95f, .97f, .98f);
            MedalUi.TargetsRow(parent, font, MedalUi.Targets(e), fontSize, icon, 0, colour, "TARGETS");
            MedalUi.Row(parent, font, new[] { 3, 2, 1 }, new[] { Campaign.Money(Campaign.MedalPay(e, 3)), Campaign.Money(Campaign.MedalPay(e, 2)), Campaign.Money(Campaign.MedalPay(e, 1)) }, fontSize, icon, 0, colour, "Medal pay", "PAYS");
            var r = Campaign.ResultOf(e.Id);
            if (r == null || r.runs == 0) { var rowEmpty = MedalUi.Row(parent, font, new int[0], new string[0], fontSize, icon, 0, colour, "Best", "BEST   not run yet"); return; }
            float v = e.Kind == CampaignEventKind.TimeTrial ? (float)r.bestTime : r.bestScore;
            if (v <= 0) { MedalUi.Row(parent, font, new int[0], new string[0], fontSize, icon, 0, colour, "Best", "BEST   no result"); return; }
            MedalUi.Row(parent, font, new[] { r.bestMedal }, new[] { MedalUi.Value(e.Kind, v) }, fontSize, icon, 0, colour, "Best", "BEST");
        }
        // the medal overlay in the corner of the course panel (campaign screen): the highlighted timed event's rows
        RectTransform medalOverlay;
        void ShowMedalOverlay(CampaignEvent e)
        {
            bool show = e != null && e.Kind != CampaignEventKind.Race && courseView && flow.State == RaceFlow.Stage.Ready && page == "campaign";
            if (medalOverlay) { for (int i = medalOverlay.childCount - 1; i >= 0; i--) { var c = medalOverlay.GetChild(i).gameObject; c.SetActive(false); Destroy(c); } }
            if (!show) { if (medalOverlay) medalOverlay.gameObject.SetActive(false); return; }
            if (!medalOverlay)
            {
                medalOverlay = Rect("Medal overlay", coursePanel); medalOverlay.anchorMin = medalOverlay.anchorMax = medalOverlay.pivot = new Vector2(0, 1); medalOverlay.anchoredPosition = new Vector2(10, -10);
                medalOverlay.gameObject.AddComponent<UnityEngine.UI.Image>().color = new Color(.03f, .05f, .07f, .9f);
                var layout = medalOverlay.gameObject.AddComponent<UnityEngine.UI.VerticalLayoutGroup>(); layout.padding = new RectOffset(8, 8, 6, 6); layout.spacing = 2; layout.childControlWidth = layout.childControlHeight = true; layout.childForceExpandWidth = false; layout.childForceExpandHeight = false;
                var fit = medalOverlay.gameObject.AddComponent<UnityEngine.UI.ContentSizeFitter>(); fit.verticalFit = fit.horizontalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;
            }
            medalOverlay.gameObject.SetActive(true); EventMedalRows(medalOverlay, e, 16, 22); medalOverlay.SetAsLastSibling();
        }
    }
}
