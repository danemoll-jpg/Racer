using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Racer
{
    // 0.90 Part B: the welcome panel (a page of the main menu), the controls card shown once before a new player's first
    // event (an overlay that holds the countdown until one press) and the small timed hint line (Hints.cs).
    public sealed partial class RaceMenus
    {
        public void OpenWelcome() { page = "welcome"; pages.Clear(); stagePages[RaceFlow.Stage.Ready] = page; modalConfirm = null; Show(); }
        void RenderWelcome()
        {
            ClearCore("WELCOME TO WOODSTOCK RUSH",
                "Race motorcycles, ATVs and cars around Woodstock: the streets, the woods, Dan's backyard and the mountain.\n" +
                "The campaign is the way in: its events pay money and open new tracks and vehicles.\n" +
                "Spend your winnings in the Shop on more vehicles and upgrades.\n" +
                "Free Roam lets you drive the whole world, find acorns and try the jumps and speed traps.");
            details.fontSize = 20; details.GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight = 150;
            Row(0, "welcome-campaign", "START THE CAMPAIGN", () => { Hints.MarkSeen("welcome"); var first = CampaignData.Events[0]; campaignChapter = first.Chapter; campaignEvent = first.Id; page = "campaign"; pages.Clear(); stagePages[RaceFlow.Stage.Ready] = page; Navigate("campaign-event"); });
            var colors = buttons[0].colors; colors.normalColor = new(.1f, .38f, .35f); buttons[0].colors = colors; buttons[0].GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight = 58;
            buttons[0].GetComponentInChildren<UnityEngine.UI.Text>(true).alignment = TextAnchor.MiddleCenter;
            Row(1, "welcome-roam", "Look around first  (Free Roam)", () => { Hints.MarkSeen("welcome"); page = ""; flow.EnterFreeRoamAfterTitle(); });
            buttons[1].GetComponentInChildren<UnityEngine.UI.Text>(true).alignment = TextAnchor.MiddleCenter; buttons[1].GetComponentInChildren<UnityEngine.UI.Text>(true).fontSize = 18;
            buttons[1].GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight = 38;
        }
        // No campaign event finished yet: CAMPAIGN is the main menu's default selection.
        static bool NewToCampaign => !Campaign.Current.results.Any(r => r.runs > 0) && !Campaign.Testing;

        // ---------- the controls card ----------
        GameObject controlsCard; RectTransform controlsRows; bool controlsBuiltForPad, controlsBuilt;
        void UpdateControlsCard()
        {
            bool show = flow.ControlsCard && flow.State == RaceFlow.Stage.Countdown;
            if (!show) { if (controlsCard) controlsCard.SetActive(false); return; }
            if (!controlsCard)
            {
                var r = Rect("Controls card", shade.transform.parent); r.anchorMin = r.anchorMax = new(.5f, .5f); r.sizeDelta = new(760, 520);
                r.gameObject.AddComponent<UnityEngine.UI.Image>().color = new(.025f, .065f, .085f, .97f); controlsCard = r.gameObject;
                var heading = Label("Heading", r, 32, 0); heading.rectTransform.anchorMin = new(0, 1); heading.rectTransform.anchorMax = new(1, 1); heading.rectTransform.pivot = new(.5f, 1); heading.rectTransform.sizeDelta = new(-48, 50); heading.rectTransform.anchoredPosition = new(0, -18); heading.text = "CONTROLS"; heading.color = new(.3f, .95f, .81f);
                controlsRows = Rect("Rows", r); controlsRows.anchorMin = new(0, 0); controlsRows.anchorMax = new(1, 1); controlsRows.offsetMin = new(40, 70); controlsRows.offsetMax = new(-40, -80);
                var foot = Label("Dismiss", r, 20, 0); foot.rectTransform.anchorMin = new(0, 0); foot.rectTransform.anchorMax = new(1, 0); foot.rectTransform.pivot = new(.5f, 0); foot.rectTransform.sizeDelta = new(-48, 40); foot.rectTransform.anchoredPosition = new(0, 18); foot.alignment = TextAnchor.MiddleCenter; foot.text = "Press any button to start"; foot.color = new(1, .9f, .62f);
            }
            controlsCard.SetActive(true); controlsCard.transform.SetAsLastSibling();
            if (controlsBuilt && controlsBuiltForPad == MenuInput.Controller) return;
            controlsBuilt = true; controlsBuiltForPad = MenuInput.Controller;
            foreach (Transform child in controlsRows) Destroy(child.gameObject);
            var input = flow.Race.vehicle.GetComponent<VehicleInput>().CurrentBindings; // throttle, brake, steering, reset, fist
            var rows = new (string label, string[] paths)[]
            {
                ("Steer", Paths(input[2], 2)), ("Accelerate", Paths(input[0], 1)), ("Brake / reverse", Paths(input[1], 1)),
                ("Reset to the nearest point of the track", Paths(input[3], 1)),
                ("Camera view", CameraViews.Current ? Paths(CameraViews.Current.CycleAction, 1) : new[] { "<Keyboard>/v" }),
                ("Pause", MenuInput.Controller ? new[] { "<Gamepad>/start" } : new[] { "<Keyboard>/escape" }),
            };
            for (int i = 0; i < rows.Length; i++)
            {
                var row = Rect(rows[i].label, controlsRows); row.anchorMin = new(0, 1); row.anchorMax = new(1, 1); row.pivot = new(.5f, 1); row.sizeDelta = new(0, 56); row.anchoredPosition = new(0, -i * 62);
                row.gameObject.AddComponent<UnityEngine.UI.Image>().color = i % 2 == 0 ? new(.06f, .14f, .19f) : new(.10f, .20f, .25f);
                var label = Label("Action", row, 23, 0); label.rectTransform.anchorMin = new(0, 0); label.rectTransform.anchorMax = new(1, 1); label.rectTransform.offsetMin = new(18, 0); label.rectTransform.offsetMax = new(-200, 0); label.alignment = TextAnchor.MiddleLeft; label.text = rows[i].label;
                for (int k = 0; k < rows[i].paths.Length; k++)
                {
                    string path = rows[i].paths[k];
                    var icon = Rect("Key", row); icon.anchorMin = icon.anchorMax = icon.pivot = new(1, .5f); icon.sizeDelta = new(path.Contains("Gamepad") && !path.Contains("Trigger") && !path.Contains("start") ? 40 : 62, 40); icon.anchoredPosition = new(-16 - (rows[i].paths.Length - 1 - k) * 78, 0);
                    var glyph = icon.gameObject.AddComponent<MenuGlyph>(); glyph.raycastTarget = false; glyph.SetPath(path);
                    var key = Label("Key text", icon, 18, 0); Stretch(key.rectTransform, 0, 0, 0, 0); key.alignment = TextAnchor.MiddleCenter; key.text = MenuGlyph.Label(path);
                }
            }
        }
        // Up to count bindings of the device in use (composite parts included: the keyboard's A and D for steering).
        static string[] Paths(InputAction action, int count)
        {
            if (action == null) return new string[0];
            string device = MenuInput.Controller ? "Gamepad" : "Keyboard";
            return action.bindings.Where(b => !b.isComposite && (b.effectivePath ?? "").Contains(device)).Select(b => b.effectivePath).Distinct().Take(count).ToArray();
        }

        // ---------- the hint line ----------
        GameObject hintPanel; UnityEngine.UI.Text hintText;
        void UpdateHint()
        {
            Hints.Tick();
            bool show = Hints.Visible && flow.State != RaceFlow.Stage.Title && !flow.ControlsCard;
            if (!show) { if (hintPanel) hintPanel.SetActive(false); return; }
            if (!hintPanel)
            {
                var r = Rect("New-player hint", shade.transform.parent); r.anchorMin = r.anchorMax = new(.5f, .16f); r.sizeDelta = new(820, 74);
                r.gameObject.AddComponent<UnityEngine.UI.Image>().color = new(.02f, .05f, .07f, .9f); r.GetComponent<UnityEngine.UI.Image>().raycastTarget = false; hintPanel = r.gameObject;
                var bar = Rect("Accent", r); bar.anchorMin = new(0, 0); bar.anchorMax = new(0, 1); bar.pivot = new(0, .5f); bar.sizeDelta = new(6, 0); bar.gameObject.AddComponent<UnityEngine.UI.Image>().color = new(1, .78f, .3f);
                hintText = Label("Hint", r, 20, 0); hintText.rectTransform.anchorMin = Vector2.zero; hintText.rectTransform.anchorMax = Vector2.one; hintText.rectTransform.offsetMin = new(24, 4); hintText.rectTransform.offsetMax = new(-16, -4); hintText.alignment = TextAnchor.MiddleLeft;
            }
            hintPanel.SetActive(true); hintPanel.transform.SetAsLastSibling(); hintText.text = Hints.Text;
            var c = hintText.color; c.a = Mathf.Clamp01((Hints.Until - Time.unscaledTime) / .6f); hintText.color = c;
        }
    }
}
