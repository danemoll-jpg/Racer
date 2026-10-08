using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Racer
{
    // 0.96 Part A (BUG-005): a campaign event that is not a race says what it is measuring, and says clearly when you
    // succeed. Jump, Speed Trap and Smash replace the lap / position / lap-time panel with an event panel: the name, the
    // medal targets (Part B), the time left and THIS RUN: the best so far in this event run, with its medal. Each attempt gets
    // a result banner the moment it is scored (centre, about 3 s, a sound per medal); once a medal that completes the event
    // is won a line stays on the panel. A Time Trial keeps its lap clock (the lap is what it measures) and gets the targets
    // and the record under it, and its result banner at the line. "Best" is this event run only; the all-time record, when
    // shown, is labelled "Record", separate and smaller. Own canvas scaled from 1080p, so the same size at 4K.
    public sealed class CampaignEventUi : MonoBehaviour
    {
        public const float BannerSeconds = 3f;
        RaceFlow flow; RectTransform root, panel, bannerBox, targetsHolder, runHolder, recordHolder; Text title, kind, time, hint, complete, bannerTop, bannerBottom;
        Image bannerBadge, panelBack; Font font; AudioSource source; readonly AudioClip[] clips = new AudioClip[5];
        float bannerUntil; string bannerKey, targetsKey, runKey, recordKey; int seenAttempts; string seenEvent;
        public static CampaignEventUi Current { get; private set; }
        // true while the event panel stands in for the race panel (Jump, Speed Trap, Smash)
        public static bool ReplacesRacePanel => Current && Current.panel && Current.panel.gameObject.activeSelf && CampaignRun.Active != null && CampaignRun.Active.Kind != CampaignEventKind.TimeTrial;
        public string BannerShown => bannerBox && bannerBox.gameObject.activeSelf ? bannerTop.text + " / " + bannerBottom.text : "";
        public bool PanelShown => panel && panel.gameObject.activeSelf;
        public string PanelText => panel ? string.Join(" | ", System.Array.ConvertAll(panel.GetComponentsInChildren<Text>(false), t => t.text)) : "";
        public static CampaignEventUi Attach(RaceFlow owner)
        {
            var ui = owner.GetComponent<CampaignEventUi>(); if (!ui) ui = owner.gameObject.AddComponent<CampaignEventUi>(); ui.flow = owner; Current = ui; return ui;
        }

        void Build()
        {
            var go = new GameObject("Campaign event UI", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler)); go.transform.SetParent(transform, false);
            var canvas = go.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 19;
            var scaler = go.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1920, 1080); scaler.matchWidthOrHeight = 1;
            root = (RectTransform)go.transform; font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            // the event panel
            panel = NewBox("Event panel", root, new Color(.02f, .04f, .05f, .84f)); panelBack = panel.GetComponent<Image>();
            panel.anchorMin = panel.anchorMax = panel.pivot = new Vector2(0, 1); panel.anchoredPosition = new Vector2(24, -24);
            var layout = panel.gameObject.AddComponent<VerticalLayoutGroup>(); layout.padding = new RectOffset(16, 16, 12, 12); layout.spacing = 5; layout.childControlWidth = layout.childControlHeight = true; layout.childForceExpandWidth = true; layout.childForceExpandHeight = false;
            var fit = panel.gameObject.AddComponent<ContentSizeFitter>(); fit.verticalFit = ContentSizeFitter.FitMode.PreferredSize; fit.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            title = Line("Name", panel, 30, FontStyle.Bold, new Color(1, .86f, .4f)); kind = Line("Kind", panel, 21, FontStyle.Normal, new Color(.7f, .8f, .86f));
            targetsHolder = Holder("Targets", panel); runHolder = Holder("This run", panel);
            time = Line("Time left", panel, 26, FontStyle.Normal, Color.white); hint = Line("Hint", panel, 21, FontStyle.Normal, new Color(.75f, .85f, .9f));
            complete = Line("Complete", panel, 27, FontStyle.Bold, new Color(.5f, 1f, .62f)); complete.horizontalOverflow = HorizontalWrapMode.Wrap;
            recordHolder = Holder("Record", panel);
            // the attempt banner
            bannerBox = NewBox("Attempt banner", root, new Color(.02f, .04f, .05f, .88f));
            bannerBox.anchorMin = bannerBox.anchorMax = bannerBox.pivot = new Vector2(.5f, .5f); bannerBox.anchoredPosition = new Vector2(0, 150); bannerBox.sizeDelta = new Vector2(980, 176);
            bannerTop = Line("Attempt", bannerBox, 56, FontStyle.Bold, Color.white); Place(bannerTop.rectTransform, 12, 70); bannerTop.alignment = TextAnchor.MiddleCenter;
            bannerBottom = Line("Result", bannerBox, 40, FontStyle.Bold, Color.white); Place(bannerBottom.rectTransform, 92, 64); bannerBottom.alignment = TextAnchor.MiddleCenter;
            bannerBadge = MedalUi.Badge(bannerBox, 3, 60, "Banner medal"); var br = bannerBadge.rectTransform; br.anchorMin = br.anchorMax = br.pivot = new Vector2(.5f, 1); br.anchoredPosition = new Vector2(-300, -96);
            source = gameObject.AddComponent<AudioSource>(); source.playOnAwake = false; source.spatialBlend = 0; source.ignoreListenerPause = true;
            bannerBox.gameObject.SetActive(false); panel.gameObject.SetActive(false);
        }
        static RectTransform NewBox(string name, Transform parent, Color colour)
        {
            var r = new GameObject(name, typeof(RectTransform), typeof(Image)).GetComponent<RectTransform>(); r.SetParent(parent, false); var i = r.GetComponent<Image>(); i.color = colour; i.raycastTarget = false; return r;
        }
        static void Place(RectTransform r, float top, float height) { r.anchorMin = new Vector2(0, 1); r.anchorMax = new Vector2(1, 1); r.pivot = new Vector2(.5f, 1); r.anchoredPosition = new Vector2(0, -top); r.sizeDelta = new Vector2(-24, height); }
        RectTransform Holder(string name, Transform parent)
        {
            var r = new GameObject(name, typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(LayoutElement)).GetComponent<RectTransform>(); r.SetParent(parent, false);
            var l = r.GetComponent<VerticalLayoutGroup>(); l.childControlWidth = l.childControlHeight = true; l.childForceExpandWidth = false; l.childForceExpandHeight = false; l.childAlignment = TextAnchor.UpperLeft;
            return r;
        }
        Text Line(string name, Transform parent, int size, FontStyle style, Color colour)
        {
            var t = new GameObject(name, typeof(RectTransform), typeof(Text)).GetComponent<Text>(); t.transform.SetParent(parent, false);
            t.font = font; t.fontSize = size; t.fontStyle = style; t.color = colour; t.alignment = TextAnchor.MiddleLeft; t.raycastTarget = false; t.horizontalOverflow = HorizontalWrapMode.Overflow; t.verticalOverflow = VerticalWrapMode.Overflow;
            var o = t.gameObject.AddComponent<Outline>(); o.effectColor = new Color(0, 0, 0, .85f); o.effectDistance = new Vector2(1.4f, -1.4f);
            var le = t.gameObject.AddComponent<LayoutElement>(); le.preferredHeight = size + 8; return t;
        }
        static void Clear(RectTransform holder) { for (int i = holder.childCount - 1; i >= 0; i--) { var c = holder.GetChild(i).gameObject; c.SetActive(false); Destroy(c); } }

        AudioClip Clip(int medal, bool scored)
        {
            int k = !scored ? 4 : medal; if (clips[k]) return clips[k];
            float[][] notes = { new[] { 392f, 330f }, new[] { 784f, 1046.5f }, new[] { 880f, 1174.7f, 1396.9f }, new[] { 1046.5f, 1318.5f, 1568f, 2093f }, new[] { 247f, 196f } };
            return clips[k] = Synth.Notes(k == 4 ? "Not scored" : "Medal " + MedalUi.Name(medal), notes[k], k == 3 ? .12f : .15f, k == 3 ? .6f : .45f);
        }

        // What the banner says about one attempt: the first line and the medal line.
        public static void Describe(CampaignEvent e, CampaignRun.AttemptResult a, out string top, out string bottom)
        {
            top = $"ATTEMPT {a.Number}" + (a.Scored ? " — " + MedalUi.Value(e.Kind, a.Value) : "");
            if (!a.Scored) { bottom = "NOT SCORED: " + a.Reason; return; }
            if (a.Medal <= 0) { bottom = "NO MEDAL"; return; }
            bottom = MedalUi.Name(a.Medal) + (a.FirstCompleting ? " — EVENT COMPLETE" : a.Better ? " — BETTER MEDAL" : "");
        }

        void ShowAttempt(CampaignRun.AttemptResult a)
        {
            var e = CampaignRun.Active; Describe(e, a, out string top, out string bottom);
            bannerTop.text = top; bannerBottom.text = bottom; bannerKey = top + bottom;
            bool medal = a.Scored && a.Medal > 0; bannerBadge.gameObject.SetActive(medal || !a.Scored);
            bannerBadge.sprite = MedalUi.Sprite(a.Scored ? a.Medal : 0); bannerBadge.color = a.Scored ? Color.white : new Color(1, .35f, .3f, .9f);
            bannerBottom.color = !a.Scored ? new Color(1, .5f, .45f) : a.Medal > 0 ? Color.Lerp(MedalUi.Tint(a.Medal), Color.white, .25f) : new Color(.8f, .86f, .9f);
            bannerBottom.rectTransform.offsetMin = new Vector2(medal || !a.Scored ? 60 : 12, bannerBottom.rectTransform.offsetMin.y);
            bannerUntil = Time.unscaledTime + BannerSeconds;
            if (source && flow != null && flow.Save != null) source.PlayOneShot(Clip(a.Medal, a.Scored), flow.Save.Settings.feedback * (a.Scored ? .55f : .4f));
        }

        void RebuildTargets(CampaignEvent e, int runMedal)
        {
            string key = e.Id + runMedal; if (key == targetsKey) return; targetsKey = key; Clear(targetsHolder);
            var t = MedalUi.Targets(e); if (t == null) return;
            MedalUi.TargetsRow(targetsHolder, font, t, 28, 36, runMedal);
        }
        void RebuildRun(CampaignEvent e)
        {
            string value = CampaignRun.RunBest > 0 ? MedalUi.Value(e.Kind, CampaignRun.RunBest) : e.Kind == CampaignEventKind.Smash && CampaignRun.LiveCount > 0 ? MedalUi.Value(e.Kind, CampaignRun.LiveCount) : "";
            string key = value + "|" + CampaignRun.RunBestMedal + "|" + CampaignRun.Attempts.Count + "|" + CampaignRun.LiveCount; if (key == runKey) return; runKey = key; Clear(runHolder);
            var label = Line("THIS RUN", runHolder, 20, FontStyle.Bold, new Color(.62f, .84f, .92f)); label.text = CampaignRun.Attempts.Count == 0 ? "THIS RUN" : $"THIS RUN  ·  {CampaignRun.Attempts.Count} attempt{(CampaignRun.Attempts.Count == 1 ? "" : "s")}";
            if (value == "") { var none = Line("None", runHolder, 28, FontStyle.Normal, new Color(.8f, .86f, .9f)); none.text = e.Kind == CampaignEventKind.Smash ? "nothing smashed yet" : CampaignRun.Attempts.Count > 0 ? "no scored attempt yet" : "no attempt yet"; return; }
            MedalUi.Row(runHolder, font, new[] { CampaignRun.RunBestMedal }, new[] { value + (CampaignRun.RunBestMedal == 0 ? "   (no medal)" : "") }, 36, 44, 0, Color.white, "This run best");
        }
        void RebuildRecord(CampaignEvent e)
        {
            var r = Campaign.ResultOf(e.Id); float best = r == null ? 0 : e.Kind == CampaignEventKind.TimeTrial ? (float)r.bestTime : r.bestScore;
            string key = best > 0 ? e.Id + best + r.bestMedal : "none"; if (key == recordKey) return; recordKey = key; Clear(recordHolder);
            if (best <= 0) return;
            var label = Line("Record label", recordHolder, 17, FontStyle.Normal, new Color(.62f, .7f, .75f)); label.text = "RECORD (all runs of this event)";
            MedalUi.Row(recordHolder, font, new[] { r.bestMedal }, new[] { MedalUi.Value(e.Kind, best) }, 22, 26, 0, new Color(.78f, .84f, .88f), "Record");
        }

        void Update()
        {
            if (!flow) return;
            if (!root) Build();
            var e = CampaignRun.Active;
            // a new event run clears what the banner remembers
            string id = e == null ? null : e.Id + CampaignRun.RunSerial; if (id != seenEvent) { seenEvent = id; seenAttempts = 0; bannerUntil = 0; targetsKey = runKey = recordKey = null; }
            bool inRace = e != null && e.Kind != CampaignEventKind.Race && !SplitScreen.Active && !TrailerMode.Active && flow.Race && !flow.Race.FreeRoam && e.Cup == null;
            // new attempts since last frame -> the banner
            if (inRace && CampaignRun.Attempts.Count > seenAttempts) { seenAttempts = CampaignRun.Attempts.Count; ShowAttempt(CampaignRun.Attempts[seenAttempts - 1]); }
            bool driving = inRace && (flow.State == RaceFlow.Stage.Racing || flow.State == RaceFlow.Stage.Countdown) && !flow.MenuVisible;
            bool showPanel = driving && !CampaignRun.Done; bool tt = e != null && e.Kind == CampaignEventKind.TimeTrial;
            if (panel.gameObject.activeSelf != showPanel) panel.gameObject.SetActive(showPanel);
            bool showBanner = inRace && Time.unscaledTime < bannerUntil && !flow.MenuVisible;
            if (bannerBox.gameObject.activeSelf != showBanner) bannerBox.gameObject.SetActive(showBanner);
            if (showBanner) { float a = Mathf.Clamp01((bannerUntil - Time.unscaledTime) / .4f); var c = bannerBox.GetComponent<Image>(); c.color = new Color(.02f, .04f, .05f, .88f * a); bannerTop.color = new Color(1, 1, 1, a); var bc = bannerBottom.color; bc.a = a; bannerBottom.color = bc; bannerBadge.color = new Color(bannerBadge.color.r, bannerBadge.color.g, bannerBadge.color.b, a); }
            if (!showPanel) return;
            // the panel: TT sits at the top centre (its lap panel stays on the left)
            panel.anchorMin = panel.anchorMax = panel.pivot = tt ? new Vector2(.5f, 1) : new Vector2(0, 1); panel.anchoredPosition = tt ? new Vector2(0, -24) : new Vector2(24, -24);
            panel.sizeDelta = new Vector2(tt ? 560 : 560, 0);
            title.text = e.Name.ToUpperInvariant();
            kind.text = e.Kind switch { CampaignEventKind.Jump => "JUMP  ·  go again as often as you like", CampaignEventKind.SpeedTrap => "SPEED TRAP  ·  hit the trap as fast as you can", CampaignEventKind.Smash => "SMASH  ·  as many different props as you can", _ => "TIME TRIAL  ·  one flying lap" };
            int shownMedal = CampaignRun.RunBestMedal; RebuildTargets(e, shownMedal);
            bool lapClock = tt; runHolder.gameObject.SetActive(!lapClock); if (!lapClock) RebuildRun(e);
            RebuildRecord(e);
            bool live = flow.State == RaceFlow.Stage.Racing;
            float left = live ? Mathf.Max(0, e.TimeLimit - (float)flow.Race.Progress.RaceTime(flow.Race.Clock)) : e.TimeLimit;
            time.gameObject.SetActive(!lapClock); if (!lapClock) time.text = e.Kind == CampaignEventKind.SpeedTrap ? $"{left:0} s to reach the trap" : $"{left:0} s left";
            // the hint line: how to go again (jump)
            bool jump = e.Kind == CampaignEventKind.Jump; hint.gameObject.SetActive(jump);
            if (jump) { var input = flow.Race.vehicle.GetComponent<VehicleInput>(); hint.text = input ? MenuGlyph.Label(MenuInput.Binding(input.CurrentBindings[3])) + ": back to the run-up" : ""; }
            bool done = CampaignRun.RunBestMedal >= 1 && !lapClock; complete.gameObject.SetActive(done);
            if (done) complete.text = $"COMPLETE — {MedalUi.Name(CampaignRun.RunBestMedal)}\n<size=21>Keep trying, or press {(MenuInput.Controller ? "START" : "Esc")} to finish</size>";
            complete.supportRichText = true;
            ((RectTransform)complete.transform).GetComponent<LayoutElement>().preferredHeight = done ? 62 : 0;
        }
        void OnDestroy() { foreach (var c in clips) if (c) Destroy(c); if (Current == this) Current = null; }
    }
}
