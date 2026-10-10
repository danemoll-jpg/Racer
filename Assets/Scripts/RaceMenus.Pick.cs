using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Racer
{
    // 0.92 Part B: vehicles are chosen in the garage view, not from a line of text.
    // - Campaign: an event's or championship's vehicle row opens the garage view (the 0.76 turning preview, name, class, the
    //   stat bars with campaign upgrades in their second tone, colour) over the owned vehicles; those the event does not allow
    //   are listed after the others, dimmed, with the reason, and cannot be used; the Shop is one row away. Use returns to
    //   the event page, which shows the chosen vehicle as a small preview with its name.
    // - Split-screen: each player chooses in a garage view of their own, both at once in their own half with their own
    //   device (with the race AI as player 2, player 1 chooses both, one after the other); the setup screen shows both
    //   vehicles as small previews.
    // Small previews (event page, prize, split-screen setup) are the garage's display model on a small turning camera each.
    public sealed partial class RaceMenus
    {
        // ---------- small previews ----------
        sealed class Mini { public Camera cam; public RenderTexture rt; public GameObject model; public string key; public float yaw = 200, radius = 1; public Vector3 centre; public UnityEngine.UI.RawImage raw; }
        readonly List<Mini> minis = new(); int minisUsed;
        static readonly Vector3 MiniBase = new(20000, 20000, 20000);
        Vector3 MiniFocus(int i) => MiniBase + Vector3.right * 200 * i;
        // A turning preview of a vehicle (its paint scheme, or a dark silhouette) in a RawImage under parent.
        UnityEngine.UI.RawImage MiniPreview(Transform parent, VehicleProfile p, int scheme, bool silhouette, int width, int height)
        {
            if (minisUsed >= minis.Count)
            {
                var m = new Mini(); var go = new GameObject("Small vehicle preview camera " + minis.Count); m.cam = go.AddComponent<Camera>();
                m.cam.cullingMask = 1 << 31; m.cam.clearFlags = CameraClearFlags.SolidColor; m.cam.backgroundColor = new Color(.06f, .1f, .13f); m.cam.fieldOfView = 30; m.cam.enabled = false; minis.Add(m);
            }
            int index = minisUsed++; var mini = minis[index];
            SetMini(mini, index, p, scheme, silhouette);
            if (!mini.rt || mini.rt.width != width || mini.rt.height != height)
            {
                if (mini.rt) { mini.cam.targetTexture = null; mini.rt.Release(); Destroy(mini.rt); }
                mini.rt = new RenderTexture(width, height, 16) { antiAliasing = 4, name = "Small vehicle preview" }; mini.cam.targetTexture = mini.rt;
            }
            mini.cam.enabled = true; mini.cam.aspect = width / (float)height;
            var raw = new GameObject("Vehicle preview", typeof(RectTransform)).AddComponent<UnityEngine.UI.RawImage>(); raw.transform.SetParent(parent, false);
            raw.texture = mini.rt; raw.raycastTarget = false; mini.raw = raw; return raw;
        }
        // 0.100 Part B: a small preview is never squished. Its texture (and so its camera's aspect) follows the size of the picture on
        // screen in pixels, whatever the screen size, the layout or a split-screen half does to it (as the garage view's own preview does).
        static void MatchMini(Mini m)
        {
            if (!m.raw || !m.raw.isActiveAndEnabled) return;
            var canvas = m.raw.canvas ? m.raw.canvas.rootCanvas : null; float scale = canvas ? canvas.scaleFactor : 1;
            var size = m.raw.rectTransform.rect.size * scale; if (size.x < 8 || size.y < 8) return;
            int w = Mathf.Clamp(Mathf.RoundToInt(size.x), 32, 2400), h = Mathf.Clamp(Mathf.RoundToInt(size.y), 32, 2400);
            if (m.rt && m.rt.width == w && m.rt.height == h) return;
            m.cam.targetTexture = null; if (m.rt) { m.rt.Release(); Destroy(m.rt); }
            m.rt = new RenderTexture(w, h, 16) { antiAliasing = 4, name = "Small vehicle preview" }; m.cam.targetTexture = m.rt; m.raw.texture = m.rt; m.cam.aspect = w / (float)h;
        }
        void SetMini(Mini mini, int index, VehicleProfile p, int scheme, bool silhouette)
        {
            string key = p.Id + "/" + scheme + "/" + silhouette + "/" + VehicleVisual.NewModels + "/" + Campaign.ChampionPaint;
            if (mini.key == key && mini.model) return;
            if (mini.model) { mini.model.SetActive(false); Destroy(mini.model); }
            mini.model = new GameObject("Small preview model") { layer = 31 }; mini.model.transform.position = MiniFocus(index);
            VehicleVisual.Build(mini.model.transform, p);
            if (silhouette) Silhouette(mini.model.transform); else if (scheme >= 0) VehiclePaint.Scheme(mini.model.transform, scheme);
            var shown = mini.model.GetComponentsInChildren<Renderer>().Where(r => r.enabled && !(r is ParticleSystemRenderer)).ToArray();
            if (shown.Length > 0) { var b = shown[0].bounds; foreach (var r in shown) b.Encapsulate(r.bounds); mini.centre = b.center - mini.model.transform.position; mini.radius = Mathf.Max(.3f, b.extents.magnitude); }
            mini.key = key;
        }
        void UpdateMinis()
        {
            if (prizeMini != null) { bool on = prizeOverlay && prizeOverlay.gameObject.activeInHierarchy && flow.MenuVisible; prizeMini.cam.enabled = on; if (on) { MatchMini(prizeMini); Turn(prizeMini, MiniFocus(50)); } }
            for (int i = 0; i < minis.Count; i++)
            {
                var m = minis[i]; bool on = i < minisUsed && flow.MenuVisible && m.model;
                if (m.cam.enabled != on) m.cam.enabled = on; if (!on) continue;
                MatchMini(m); Turn(m, MiniFocus(i));
            }
        }
        void Turn(Mini m, Vector3 focus)
        {
            {
                m.yaw = Mathf.Repeat(m.yaw + 16 * Time.unscaledDeltaTime, 360);
                var turn = Quaternion.Euler(0, m.yaw, 0);
                m.model.transform.SetPositionAndRotation(focus - turn * m.centre, turn);
                float half = m.cam.fieldOfView * .5f * Mathf.Deg2Rad, fit = Mathf.Min(half, Mathf.Atan(Mathf.Tan(half) * m.cam.aspect)), distance = m.radius * 1.04f / Mathf.Sin(fit);
                var look = Quaternion.Euler(11, 0, 0); m.cam.transform.SetPositionAndRotation(focus - look * Vector3.forward * distance, look);
                m.cam.nearClipPlane = .05f; m.cam.farClipPlane = distance + m.radius * 2 + 5;
            }
        }
        // A strip of small previews with captions (row height 150).
        RectTransform PreviewStrip(string name, int siblingIndex, float height = 150)
        {
            var strip = LaterGroup(name, content, true, height); strip.SetSiblingIndex(siblingIndex);
            var layout = strip.GetComponent<UnityEngine.UI.HorizontalLayoutGroup>(); layout.spacing = 14; layout.childForceExpandWidth = false; layout.childControlWidth = true; layout.childForceExpandHeight = true;
            return strip;
        }
        void PreviewCard(RectTransform strip, VehicleProfile p, int scheme, bool silhouette, string caption, Color captionColour, float width = 250)
        {
            var cell = Rect("Preview " + p.Id, strip); cell.gameObject.AddComponent<UnityEngine.UI.LayoutElement>().preferredWidth = width;
            cell.gameObject.AddComponent<UnityEngine.UI.Image>().color = new Color(.06f, .1f, .13f);
            var raw = MiniPreview(cell, p, scheme, silhouette, Mathf.RoundToInt(width * 1.5f), 165); var r = raw.rectTransform; r.anchorMin = new Vector2(0, 0); r.anchorMax = Vector2.one; r.offsetMin = new Vector2(0, 38); r.offsetMax = Vector2.zero;
            if (silhouette) tableCells.Add(PadlockMark.Add(cell, new Vector2(1, 1), new Vector2(-24, -24), 26, new Color(1, .82f, .35f, .92f)).gameObject);
            var text = Label("Caption", cell, 17, 0); text.rectTransform.anchorMin = Vector2.zero; text.rectTransform.anchorMax = new Vector2(1, 0); text.rectTransform.pivot = new Vector2(.5f, 0); text.rectTransform.sizeDelta = new Vector2(-8, 38);
            text.alignment = TextAnchor.MiddleCenter; text.text = caption; text.color = captionColour;
        }
        int SchemeOf(string vehicleId) { var c = flow.Save.Settings.bodyColors; int i = VehicleProfile.IndexOf(vehicleId); return c != null && i >= 0 && i < c.Length ? c[i] : -1; }

        // ---------- campaign: the vehicle in the garage view ----------
        string pickVehicle; bool pickCup, shopToPick;
        VehicleProfile[] PickAllowed => pickCup ? CupVehicles(CampaignData.FindCup(campaignCup)) : EventVehicles(CampaignData.Find(campaignEvent));
        void OpenVehiclePick(bool cup) { pickCup = cup; pickVehicle = campaignVehicle; Navigate("vehicle-pick"); }
        string NotAllowed(VehicleProfile p)
        {
            if (pickCup) return "Not for this championship: some of its courses are for motorcycles and ATVs only";
            var e = CampaignData.Find(campaignEvent); if (e == null) return "";
            if (!e.Allows(p)) return "Not for this event: " + e.Entry.ToLowerInvariant();
            return "Not for this event: " + e.CourseTitle + " is for motorcycles and ATVs only";
        }
        void RenderVehiclePick()
        {
            var allowed = PickAllowed; var owned = VehicleProfile.All.Where(v => !v.Reward && (Campaign.Testing || Campaign.Owns(v.Id))).OrderBy(v => allowed.Contains(v) ? 0 : 1).ToArray();
            if (owned.Length == 0) { BackPage(); return; }
            var p = owned.FirstOrDefault(v => v.Id == pickVehicle) ?? owned[0]; pickVehicle = p.Id; int at = System.Array.IndexOf(owned, p); bool ok = allowed.Contains(p);
            string what = pickCup ? CampaignData.FindCup(campaignCup)?.Name : CampaignData.Find(campaignEvent)?.Name;
            ClearCore("CHOOSE YOUR VEHICLE", $"{p.Name} / {p.Class}  ·  for {what}\n" + (ok ? "Upgrades: " + Campaign.UpgradeSummary(p.Id) + (Campaign.Upgraded(p.Id) ? "  (gold on the bars)" : "") : NotAllowed(p)));
            details.fontSize = 18; details.GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight = 52; if (!ok) details.color = new Color(1, .82f, .45f);
            EnterGarageView(); preview.gameObject.SetActive(true); previewCamera.enabled = true; swatchRow.gameObject.SetActive(false);
            if (previewRoot) { previewRoot.SetActive(false); Destroy(previewRoot); }
            previewRoot = new GameObject("Garage display model") { layer = 31 }; previewRoot.transform.position = new Vector3(10000, 10000, 10000);
            VehicleVisual.Build(previewRoot.transform, p); int scheme = SchemeOf(p.Id); if (scheme >= 0) VehiclePaint.Scheme(previewRoot.transform, scheme);
            if (!ok) Silhouette(previewRoot.transform);
            previewLock = ok ? null : NotAllowed(p); FramePreview(p); ShowPreviewLock();
            void StepPick(int d) { pickVehicle = owned[(at + d + owned.Length) % owned.Length].Id; flow.Click(); Show(); }
            Step(0, "pick-vehicle", $"{p.Name}   ·   {at + 1} of {owned.Length}" + (ok ? "" : "   ·   not allowed"), StepPick);
            if (!ok) buttons[0].GetComponentInChildren<UnityEngine.UI.Text>(true).color = new Color(.62f, .66f, .66f);
            ShowGarageStats(p, Campaign.Testing ? null : p.Id);
            Step(1, "pick-colour", "Colour:   " + (scheme < 0 ? "Factory" : VehiclePaint.Name(scheme)), d => { int i = VehicleProfile.IndexOf(p.Id); if (i >= 0) { flow.Save.Settings.bodyColors[i] = VehiclePaint.Next(scheme, d); flow.Save.SaveSettings(); } flow.Click(); Show(); });
            Row(2, "pick-use", ok ? "USE THIS VEHICLE" : "NOT ALLOWED HERE", () => { campaignVehicle = p.Id; BackPage(); });
            var colors = buttons[2].colors; colors.normalColor = new(.1f, .38f, .35f); buttons[2].colors = colors; buttons[2].GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight = 54; buttons[2].interactable = ok;
            Row(3, "pick-shop", "Shop…   (" + Campaign.Money(Campaign.Current.money) + ")", () => { shopToPick = true; shopFromCampaign = false; shopVehicle = p.Id; flow.OpenGarage(); page = "shop"; pages.Clear(); Show(); });
            Row(4, "pick-back", "Back", () => BackPage());
            details.transform.SetSiblingIndex(0); buttons[0].transform.SetSiblingIndex(1); statBlock.SetSiblingIndex(2); for (int k = 1; k <= 4; k++) buttons[k].transform.SetSiblingIndex(2 + k);
        }
        // The Shop opened from the vehicle choice returns to it (campaign screen > event or championship > vehicle).
        void ReturnToPick()
        {
            pages.Clear(); pages.Push("campaign"); pages.Push(pickCup ? "campaign-cup" : "campaign-event");
            page = "vehicle-pick"; stagePages[RaceFlow.Stage.Ready] = page; modalConfirm = null; Show();
        }

        // ---------- split-screen: each player in their own half ----------
        public bool SplitPickOpen => flow && flow.State == RaceFlow.Stage.Ready && page == "split-garage" && modalConfirm == null;
        bool pickReady1, pickReady2; int pickFocus = 1; // with the AI as player 2: the half player 1 is choosing
        readonly Dictionary<InputDevice, float> pickRepeat = new();
        // 0.95 Part B: runnerOnly = a solo Police Chase: only the AI runner's vehicle is chosen (player 1 drives the patrol car)
        bool pickRunnerOnly, pickPlayerOne;
        void OpenSplitPick(bool runnerOnly = false, int only = 0) { pickRunnerOnly = runnerOnly; pickPlayerOne = only == 1; pickReady1 = runnerOnly; pickReady2 = pickPlayerOne; pickFocus = runnerOnly ? 2 : 1; Navigate("split-garage"); }
        void RenderSplitPick()
        {
            bool ai = SplitScreen.P2Ai || SplitScreen.P2Device == null;
            if (pickPlayerOne) ClearCore("POLICE CHASE · YOUR VEHICLE", "You run in this vehicle: left / right the vehicle, up / down the colour, A when done. B goes back.");
            else if (pickRunnerOnly) ClearCore("POLICE CHASE · THE RUNNER'S VEHICLE", "The AI runs from you in this vehicle: left / right the vehicle, up / down the colour, A when done. B goes back.");
            else ClearCore(SplitScreen.Mode == SplitScreen.Kind.Police ? "POLICE CHASE · THE RUNNERS' VEHICLES" : "SPLIT SCREEN · VEHICLES", ai
                ? "Player 1 chooses both vehicles: left / right the vehicle, up / down the colour, A when done (then the AI's). B goes back."
                : "Each player on their own device: left / right the vehicle, up / down the colour, A when ready. Both ready returns to the setup.");
            details.fontSize = 18; details.GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight = 54;
            var halves = LaterGroup("Players' garages", content, true, 430); halves.SetSiblingIndex(1);
            var layout = halves.GetComponent<UnityEngine.UI.HorizontalLayoutGroup>(); layout.spacing = 16; layout.childForceExpandWidth = true;
            for (int player = pickRunnerOnly ? 2 : 1; player <= (pickPlayerOne ? 1 : 2); player++)
            {
                int who = player; string id = who == 1 ? SplitScreen.P1Vehicle : SplitScreen.P2Vehicle; int colour = who == 1 ? SplitScreen.P1Color : SplitScreen.P2Color; var p = VehicleProfile.Find(id);
                bool ready = who == 1 ? pickReady1 : pickReady2; bool focused = !ai || pickFocus == who;
                var half = Rect("Player " + who, halves); half.gameObject.AddComponent<UnityEngine.UI.Image>().color = focused && !ready ? new Color(.09f, .19f, .23f) : new Color(.06f, .1f, .13f);
                var v = half.gameObject.AddComponent<UnityEngine.UI.VerticalLayoutGroup>(); v.padding = new RectOffset(10, 10, 8, 8); v.spacing = 4; v.childControlWidth = v.childControlHeight = true; v.childForceExpandHeight = false;
                var head = Label("Player", half, 20, 26); head.alignment = TextAnchor.MiddleCenter; head.color = who == 1 ? new Color(.3f, .95f, .81f) : new Color(1, .74f, .25f);
                head.text = pickRunnerOnly ? "THE RUNNER   ·   the AI (you choose)" : $"PLAYER {who}   ·   " + (who == 2 && ai ? "the race AI (player 1 chooses)" : SplitScreen.DeviceName(who == 1 ? SplitScreen.P1Device : SplitScreen.P2Device));
                var cell = Rect("Preview", half); cell.gameObject.AddComponent<UnityEngine.UI.LayoutElement>().preferredHeight = 190;
                var raw = MiniPreview(cell, p, colour, false, 640, 280); var rr = raw.rectTransform; rr.anchorMin = Vector2.zero; rr.anchorMax = Vector2.one; rr.offsetMin = rr.offsetMax = Vector2.zero;
                ValueLine(half, $"{p.Name}  ({p.Class})", d => PickStep(who, d, 0));
                StatBars(half, p);
                ValueLine(half, "Colour:   " + VehiclePaint.Name(colour), d => PickStep(who, d, 1));
                var state = Label("State", half, 20, 30); state.alignment = TextAnchor.MiddleCenter;
                state.text = ready ? "READY  ✓" : focused ? "A: ready" : "waiting"; state.color = ready ? new Color(.4f, 1, .6f) : Color.white;
            }
            Row(0, "split-pick-done", "Done: back to the setup", () => BackPage()); // for the mouse; the controllers use A in their half
        }
        // A "‹ value ›" line inside a half: the arrows can be clicked with the mouse.
        void ValueLine(Transform parent, string label, System.Action<int> change)
        {
            var row = Rect("Value", parent); row.gameObject.AddComponent<UnityEngine.UI.LayoutElement>().preferredHeight = 32;
            var text = Label("Label", row, 20, 0); Stretch(text.rectTransform, 40, 0, -40, 0); text.alignment = TextAnchor.MiddleCenter; text.text = label;
            for (int side = -1; side <= 1; side += 2)
            {
                int d = side; var r = Rect(d < 0 ? "Previous" : "Next", row); r.anchorMin = new Vector2(d < 0 ? 0 : 1, 0); r.anchorMax = new Vector2(d < 0 ? 0 : 1, 1); r.pivot = new Vector2(d < 0 ? 0 : 1, .5f); r.sizeDelta = new Vector2(38, 0); r.anchoredPosition = Vector2.zero;
                var image = r.gameObject.AddComponent<UnityEngine.UI.Image>(); image.color = new Color(1, 1, 1, .001f); var b = r.gameObject.AddComponent<UnityEngine.UI.Button>(); b.targetGraphic = image; b.navigation = new UnityEngine.UI.Navigation { mode = UnityEngine.UI.Navigation.Mode.None };
                b.onClick.AddListener(() => change(d)); var glyph = Label("Arrow", r, 28, 0); Stretch(glyph.rectTransform, 0, 0, 0, 0); glyph.alignment = TextAnchor.MiddleCenter; glyph.text = d < 0 ? "‹" : "›"; glyph.color = new Color(.55f, 1, .9f);
            }
        }
        // The garage's stock stat bars (split-screen vehicles are stock).
        void StatBars(Transform parent, VehicleProfile p)
        {
            for (int i = 0; i < GarageStats.Length; i++)
            {
                var row = Rect(GarageStats[i].label, parent); row.gameObject.AddComponent<UnityEngine.UI.LayoutElement>().preferredHeight = 20;
                var text = Label("Label", row, 16, 0); text.rectTransform.anchorMin = Vector2.zero; text.rectTransform.anchorMax = new Vector2(.4f, 1); text.rectTransform.offsetMin = text.rectTransform.offsetMax = Vector2.zero; text.text = GarageStats[i].label; text.color = new Color(.86f, .86f, .8f);
                var track = Rect("Bar", row); track.anchorMin = new Vector2(.42f, .2f); track.anchorMax = new Vector2(1, .8f); track.offsetMin = track.offsetMax = Vector2.zero; track.gameObject.AddComponent<UnityEngine.UI.Image>().color = new Color(1, 1, 1, .12f);
                var fill = Rect("Fill", track); fill.anchorMin = Vector2.zero; fill.anchorMax = new Vector2(Mathf.Clamp01(StatFraction(i, p)), 1); fill.offsetMin = fill.offsetMax = Vector2.zero; fill.gameObject.AddComponent<UnityEngine.UI.Image>().color = new Color(.3f, 1, .88f);
            }
        }
        // what: 0 = vehicle, 1 = colour
        void PickStep(int player, int d, int what)
        {
            if (player == 1 ? pickReady1 : pickReady2) return;
            if (what == 0) { if (player == 1) SplitScreen.P1Vehicle = NextSplitVehicle(SplitScreen.P1Vehicle, d); else SplitScreen.P2Vehicle = NextSplitVehicle(SplitScreen.P2Vehicle, d); }
            else { if (player == 1) SplitScreen.P1Color = VehiclePaint.Next(SplitScreen.P1Color, d); else SplitScreen.P2Color = VehiclePaint.Next(SplitScreen.P2Color, d); }
            flow.Click(); Show();
        }
        void PickReady(int player)
        {
            bool ai = SplitScreen.P2Ai || SplitScreen.P2Device == null;
            if (player == 1) pickReady1 = !pickReady1; else pickReady2 = !pickReady2;
            if (ai && player == 1 && pickReady1) pickFocus = 2;
            flow.Click();
            if (pickReady1 && pickReady2) { pickFocus = 1; BackPage(); return; } // (focus back first: B on the AI's half is not this)
            Show();
        }
        // B from player 1 with the AI's half chosen goes back to player 1's half (not out of the screen).
        bool SplitPickBack()
        {
            if (!SplitPickOpen) return false;
            bool ai = SplitScreen.P2Ai || SplitScreen.P2Device == null;
            if (ai && pickFocus == 2 && !pickRunnerOnly) { pickFocus = 1; pickReady1 = false; flow.Click(); Show(); return true; }
            return false;
        }
        // Each device works its own half: left / right the vehicle, up / down the colour (held to repeat), A ready; player 2's B
        // takes back its ready.
        void UpdateSplitPick()
        {
            if (!SplitPickOpen || MenuInput.Blocked) return;
            bool ai = SplitScreen.P2Ai || SplitScreen.P2Device == null;
            Read(SplitScreen.P1Device, ai ? pickFocus : 1);
            if (!ai) Read(SplitScreen.P2Device, 2);
            void Read(InputDevice device, int player)
            {
                if (device == null || !device.added) return;
                int dx = 0, dy = 0; bool a = false, b = false;
                if (device is Gamepad g)
                {
                    var v = g.dpad.ReadValue(); var s = g.leftStick.ReadValue(); if (s.magnitude > .6f) v += s;
                    dx = v.x > .5f ? 1 : v.x < -.5f ? -1 : 0; dy = Mathf.Abs(v.y) > Mathf.Abs(v.x) && Mathf.Abs(v.y) > .5f ? (v.y > 0 ? 1 : -1) : 0; if (dy != 0) dx = 0;
                    a = g.buttonSouth.wasPressedThisFrame; b = g.buttonEast.wasPressedThisFrame;
                }
                else if (device is Keyboard k)
                {
                    dx = k.rightArrowKey.isPressed || k.dKey.isPressed ? 1 : k.leftArrowKey.isPressed || k.aKey.isPressed ? -1 : 0; dy = k.upArrowKey.isPressed || k.wKey.isPressed ? 1 : k.downArrowKey.isPressed || k.sKey.isPressed ? -1 : 0; if (dy != 0) dx = 0;
                    a = k.enterKey.wasPressedThisFrame || k.numpadEnterKey.wasPressedThisFrame || k.spaceKey.wasPressedThisFrame; b = k.escapeKey.wasPressedThisFrame || k.backspaceKey.wasPressedThisFrame;
                }
                if (a) { MenuInput.ConsumeThroughRelease(); PickReady(player); return; }
                if (b && player == 2 && device != SplitScreen.P1Device) { if (pickReady2) { pickReady2 = false; flow.Click(); Show(); } return; }
                int dir = dx != 0 ? dx : dy * 10; if (dir == 0) { heldDir.Remove(device); return; }
                bool fresh = !heldDir.TryGetValue(device, out int last) || last != dir;
                if (!fresh && pickRepeat.TryGetValue(device, out float until) && Time.unscaledTime < until) return;
                heldDir[device] = dir; pickRepeat[device] = Time.unscaledTime + (fresh ? .42f : .11f);
                if (dx != 0) PickStep(player, dx, 0); else PickStep(player, -dy, 1);
            }
        }
        readonly Dictionary<InputDevice, int> heldDir = new();
    }
}
