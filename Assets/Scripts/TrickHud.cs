using System.Collections.Generic;
using UnityEngine;

namespace Racer
{
    // 0.120 Part A (Dan: "There is no counter for your current stunt and multiplier"): the trick panel. Wherever tricks score as an event (a Stunt
    // Race, the Stunt chapter's events, the Stunt event at the park; solo and split screen, each player in their own view: ArcadeActivities.TrickBox)
    // a panel on the left of the view shows the trick as it happens: in the air the tricks turned so far ("BACKFLIP + ROLL"), the flight's
    // multiplier ("x2": one for each trick type) and the points on the line; then "LANDED +1,240" or "BAILED"; on the ground the combo going (its
    // chain multiplier, its points and the 5 s to chain the next trick). The total points stay on each mode's own panel (the race panel, the Stunt
    // event's panel). It sits under the position / lap panel, clear of the NEXT CP line (bottom centre), the minimap (top right) and the speedometer
    // (bottom right); smaller in races than in the park. Hidden while nothing is going.
    public sealed class TrickHud : MonoBehaviour
    {
        public const float ResultSeconds = 2.5f;
        RaceFlow flow; GUIStyle style; Texture2D back;
        public static TrickHud Attach(RaceFlow owner) { var h = owner.GetComponent<TrickHud>(); if (!h) h = owner.gameObject.AddComponent<TrickHud>(); h.flow = owner; return h; }
        public static TrickHud Current { get; private set; }
        void Awake() => Current = this;
        void OnDestroy() { if (Current == this) Current = null; if (back) Destroy(back); }

        // the panel's lines for one player (rich text; none: nothing going)
        public static List<string> Lines(ArcadeActivities a)
        {
            var lines = new List<string>(); if (!a || !a.TrickBox) return lines;
            var live = a.Live;
            if (live != null)
            {
                lines.Add(live.names.Count > 0 ? $"<b>{live.Names}</b>" + (live.types > 1 ? $"   <color=#FFC747><b>{live.Multiplier}</b></color>" : "") : "<color=#BFD8E0>IN THE AIR</color>");
                lines.Add($"<color=#FFE58A>+{live.points:N0}</color> <size={{small}}>on the line</size>");
            }
            else if (Time.time - a.LastResultAt < ResultSeconds && a.LastResult != "")
                lines.Add((a.LastResultKind == 1 ? "<color=#8CFFA8>" : a.LastResultKind == 2 ? "<color=#FF7A66>" : "<color=#FFB45A>") + "<b>" + a.LastResult + "</b></color>");
            if (a.ComboLinks > 0)
                lines.Add($"COMBO <color=#FFC747><b>x{a.ComboLinks}</b></color>  ·  {a.ComboTotal:N0}" + (live == null && a.ComboLinks < ArcadeActivities.MaxLinks ? $"  ·  <b>{a.ComboLeft:0.0}</b> s" : ""));
            return lines;
        }
        // what the panel shows now for player 1 / 2, as plain text (checks)
        public static string Text(ArcadeActivities a) => System.Text.RegularExpressions.Regex.Replace(string.Join(" / ", Lines(a)).Replace("{small}", "1"), "<[^>]+>", "");

        void OnGUI()
        {
            if (Event.current.type != EventType.Repaint || !flow || !flow.Race || flow.MenuVisible || flow.State != RaceFlow.Stage.Racing || TrailerMode.Active) return;
            bool split = SplitScreen.Active && SplitScreen.Race;
            if (!split || SplitScreen.Watching != 2) Draw(flow.Activities, Camera.main);
            if (split && !SplitScreen.OneView && !SplitScreen.P2Ai && SplitScreen.Watching != 1)
                Draw(SplitScreen.Race.P2Acts ? SplitScreen.Race.P2Acts : SplitRoam.Current ? SplitRoam.Current.Activities2 : null, SplitScreen.Race.Camera2);
        }
        void Draw(ArcadeActivities a, Camera cam)
        {
            if (!a || !cam || !cam.isActiveAndEnabled) return;
            var lines = Lines(a); if (lines.Count == 0) return;
            var view = cam.pixelRect; bool race = !flow.Race.FreeRoam;
            int size = Mathf.Max(15, Mathf.RoundToInt(view.height / (race ? 38f : 30f))), small = Mathf.Max(12, Mathf.RoundToInt(size * .7f));
            if (style == null) { style = new GUIStyle(GUI.skin.label) { richText = true, alignment = TextAnchor.UpperLeft, wordWrap = false, clipping = TextClipping.Overflow }; back = new Texture2D(1, 1); back.SetPixel(0, 0, new Color(.02f, .045f, .06f, .78f)); back.Apply(); }
            style.fontSize = size;
            string text = string.Join("\n", lines).Replace("{small}", small.ToString());
            var c = new GUIContent(text); var dims = style.CalcSize(c); float pad = size * .45f;
            // under the position / lap panel on the left (the Stunt chapter's event panel there is taller: lower)
            float top = Screen.height - view.yMax + view.height * (CampaignEventUi.Current && CampaignEventUi.Current.PanelShown ? .6f : .42f);
            var box = new Rect(view.x + Mathf.Max(14, view.width * .012f), top, dims.x + pad * 2, dims.y + pad * 1.4f);
            GUI.DrawTexture(box, back);
            var inner = new Rect(box.x + pad, box.y + pad * .7f, dims.x, dims.y);
            var colour = style.normal.textColor;
            style.normal.textColor = new Color(0, 0, 0, .85f); GUI.Label(new Rect(inner.x + 1.5f, inner.y + 1.5f, inner.width, inner.height), System.Text.RegularExpressions.Regex.Replace(text, "<color=[^>]+>|</color>", ""), style);
            style.normal.textColor = Color.white; GUI.Label(inner, c, style); style.normal.textColor = colour;
        }
    }
}
