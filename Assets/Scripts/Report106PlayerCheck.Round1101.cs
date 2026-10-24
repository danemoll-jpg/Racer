using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Racer
{
    // 1.1.0 evidence only (the 0.106 check component: -p106check <dir> -racerTestSave <dir>; never active without both):
    //   -p106kind display1101 -p1101part pages    every menu page and the HUD at the window size given (-p106w 1280 -p106h 800): one shot each, and
    //                                             each page audited: any visible element outside the screen (or outside the mask that clips it) and
    //                                             any text the game cut short are listed
    //   -p1101part toggle   Settings > Display > Screen mode with the controller only: switched both ways, the real screen mode each time, B back
    //   -p1101part startup  with -p106keepscreen and no -screen-* arguments: the screen mode and size the game opened with (what the player sees)
    public sealed partial class Report106PlayerCheck
    {
        IEnumerator Display1101()
        {
            string part = Arg("-p1101part", "pages"); Campaign.Testing = true; Conditions();
            Note($"display check part {part}: screen {Screen.width}x{Screen.height} {Screen.fullScreenMode}, display {Display.main.systemWidth}x{Display.main.systemHeight}, saved Screen mode {(flow.Save.Settings.windowed ? "Windowed" : "Full screen")}");
            if (part == "pages") yield return Pages1101();
            if (part == "toggle") yield return Toggle1101();
            if (part == "startup") { yield return new WaitForSecondsRealtime(3); Note($"startup: after 3 s screen {Screen.width}x{Screen.height} {Screen.fullScreenMode}, display {Display.main.systemWidth}x{Display.main.systemHeight}"); yield return Snap("startup"); }
        }
        // ---- the 1280x800 pages
        int shot1101;
        IEnumerator Page1101(string name, float settle = 1.2f)
        {
            yield return new WaitForSecondsRealtime(settle); Canvas.ForceUpdateCanvases(); yield return null;
            var problems = Audit1101(); string file = $"{++shot1101:00}-{name}";
            yield return Snap(file); Note($"page {file}: {(problems.Count == 0 ? "everything on screen, no text cut" : string.Join("; ", problems.Take(12)) + (problems.Count > 12 ? $" (+{problems.Count - 12} more)" : ""))}");
        }
        static Rect ScreenRect(RectTransform r)
        {
            var c = new Vector3[4]; r.GetWorldCorners(c); var canvas = r.GetComponentInParent<Canvas>()?.rootCanvas; var cam = canvas && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;
            Vector2 a = RectTransformUtility.WorldToScreenPoint(cam, c[0]), b = RectTransformUtility.WorldToScreenPoint(cam, c[2]);
            return Rect.MinMaxRect(Mathf.Min(a.x, b.x), Mathf.Min(a.y, b.y), Mathf.Max(a.x, b.x), Mathf.Max(a.y, b.y));
        }
        static string Path1101(Transform t) { var s = t.name; for (int i = 0; i < 2 && t.parent; i++) { t = t.parent; s = t.name + "/" + s; } return s; }
        List<string> Audit1101()
        {
            var list = new List<string>(); var screen = new Rect(0, 0, Screen.width, Screen.height);
            foreach (var g in FindObjectsByType<UnityEngine.UI.Graphic>(FindObjectsSortMode.None))
            {
                if (!g.isActiveAndEnabled || g.color.a < .02f || !g.canvas || !g.canvas.enabled || g.canvas.rootCanvas.renderMode == RenderMode.WorldSpace) continue;
                if (g is UnityEngine.UI.Text tx && string.IsNullOrWhiteSpace(tx.text)) continue;
                var cg = g.GetComponentsInParent<CanvasGroup>(); if (cg.Any(x => x.alpha < .02f)) continue;
                var r = ScreenRect(g.rectTransform); if (r.width >= screen.width * .98f && r.height >= screen.height * .98f) continue;   // full-screen backdrops
                // what is clipped by a scroll view's mask only has to be on screen where the mask shows it
                var mask = g.GetComponentsInParent<UnityEngine.UI.RectMask2D>().FirstOrDefault(m => m.enabled && m.gameObject != g.gameObject)?.rectTransform ?? g.GetComponentsInParent<UnityEngine.UI.Mask>().FirstOrDefault(m => m.enabled && m.gameObject != g.gameObject)?.rectTransform;
                var shown = r; if (mask) { var m = ScreenRect(mask); shown = Rect.MinMaxRect(Mathf.Max(r.xMin, m.xMin), Mathf.Max(r.yMin, m.yMin), Mathf.Min(r.xMax, m.xMax), Mathf.Min(r.yMax, m.yMax)); if (shown.width <= 0 || shown.height <= 0) continue; }
                if (shown.xMin < -2 || shown.yMin < -2 || shown.xMax > screen.width + 2 || shown.yMax > screen.height + 2)
                    list.Add($"OFF SCREEN '{Path1101(g.transform)}'{(g is UnityEngine.UI.Text t0 ? " \"" + t0.text.Split('\n')[0].Substring(0, Mathf.Min(30, t0.text.Split('\n')[0].Length)) + "\"" : "")} {shown.xMin:F0},{shown.yMin:F0}-{shown.xMax:F0},{shown.yMax:F0}");
                // a text the game cut short: fewer characters drawn than it has (rich-text tags and spaces aside)
                if (g is UnityEngine.UI.Text text && text.verticalOverflow == VerticalWrapMode.Truncate && text.cachedTextGenerator.characterCount > 0)
                {
                    string plain = System.Text.RegularExpressions.Regex.Replace(text.supportRichText ? System.Text.RegularExpressions.Regex.Replace(text.text, "<[^>]+>", "") : text.text, @"\s", "");
                    int drawn = text.cachedTextGenerator.characters.Take(text.cachedTextGenerator.characterCountVisible).Count(ch => ch.charWidth > 0);
                    if (drawn < plain.Length) list.Add($"TEXT CUT '{Path1101(text.transform)}' \"{plain.Substring(0, Mathf.Min(30, plain.Length))}\" shows {drawn} of {plain.Length} characters, ends \"{text.text.Substring(Mathf.Max(0, text.text.Length - 40)).Replace('\n', '/')}\"");
                }
            }
            return list.Distinct().ToList();
        }
        UnityEngine.UI.Button Btn(string id) => FindObjectsByType<UnityEngine.UI.Button>(FindObjectsSortMode.None).FirstOrDefault(b => b.gameObject.activeInHierarchy && b.name == id);
        IEnumerator Click1101(string id) { var b = Btn(id); if (!b) { Note($"no '{id}' row on this page"); yield break; } b.onClick.Invoke(); yield return null; }
        IEnumerator Back1101() { yield return Press1100(GamepadButton.East); yield return new WaitForSecondsRealtime(.6f); }
        IEnumerator Home1101() { Menu.ResetPages(); Menu.Show(); yield return new WaitForSecondsRealtime(.6f); }
        IEnumerator Pages1101()
        {
            yield return QuickTo115(0); flow.Save.Settings.hints = false; flow.Save.SaveSettings();
            Menu.ResetPages();
            Menu.Show(); yield return Page1101("main-menu");
            yield return Click1101("settings"); yield return Page1101("settings-gameplay");
            foreach (var tab in new[] { "audio", "display", "controls" }) { yield return Click1101("tab-" + tab); yield return Page1101("settings-" + tab); }
            yield return Click1101("tab-gameplay"); yield return new WaitForSecondsRealtime(.6f); var path = new List<string>(); yield return PadTo("player-name", path); yield return Press1100(GamepadButton.South); yield return Page1101("player-name-entry");
            yield return Back1101(); flow.CloseSettings(); yield return Home1101();
            flow.OpenGarage(); yield return Page1101("garage", 2.5f); flow.CloseGarage(); yield return Home1101();
            Menu.OpenCampaign(); yield return Page1101("campaign", 2f); yield return Home1101();
            flow.OpenCourses(); yield return Page1101("track-select-map", 2.5f); flow.CloseExtras(); yield return Home1101();
            flow.OpenBoards(); yield return Page1101("records", 1.5f); flow.CloseExtras(); yield return Home1101();
            yield return Click1101("replays"); yield return Page1101("replays"); yield return Home1101();
            // a race: the HUD, the pause menu, the finish screen, the replay viewer
            var st = flow.Save.Settings; st.opponents = true; st.traffic = true; st.laps = 1; st.difficulty = 1; flow.Save.SaveSettings();
            race.opponents = true; race.traffic = true; race.laps = 1; race.difficulty = 1;
            flow.StartRace(); yield return WaitRacing(); Bind(); AudioListener.volume = 0; yield return new WaitForSecondsRealtime(6); yield return Page1101("race-hud", 0);
            flow.Pause(); yield return Page1101("pause-menu"); flow.Resume(); yield return null;
            yield return RaceToResults(); yield return Page1101("finish-screen");
            yield return Press1100(GamepadButton.West); yield return new WaitForSecondsRealtime(2); var v = ReplayViewer.Current;
            if (v) { yield return Page1101("replay-viewer", 1); yield return Press1100(GamepadButton.DpadDown); yield return Page1101("replay-viewer-hud", 1); yield return Back1101(); }
            else Note("replay viewer did not open");
        }
        // ---- the option by the controller only
        IEnumerator Toggle1101()
        {
            yield return QuickTo115(0); var path = new List<string>();
            // the controller only: the real mouse is switched off in this check (full screen covers the desktop, where a moving pointer would pick rows)
            foreach (var m in InputSystem.devices.OfType<Mouse>().ToList()) InputSystem.DisableDevice(m);
            Menu.ResetPages();
            Menu.Show(); yield return new WaitForSecondsRealtime(1);
            yield return PadTo("settings", path); yield return Press1100(GamepadButton.South);
            yield return PadTo("tab-display", path); yield return Press1100(GamepadButton.South);
            yield return PadTo("screen-mode", path); Note("controller path: " + string.Join(" > ", path.Distinct()));
            string Label() => Btn("screen-mode")?.GetComponentInChildren<UnityEngine.UI.Text>(true).text.Replace("\n", " ");
            string Saved() { try { return File.ReadAllText(Path.Combine(flow.SaveRoot, "settings.json")).Contains("\"windowed\": true") ? "windowed: true" : "windowed: false"; } catch { return "unreadable"; } }
            Note($"before: row '{Label()}', screen {Screen.width}x{Screen.height} {Screen.fullScreenMode}");
            // Full screen (the saved default; the check started windowed) > Windowed > Full screen > Windowed: ends windowed
            for (int i = 0; i < 3; i++)
            {
                yield return Press1100(i == 1 ? GamepadButton.DpadLeft : GamepadButton.DpadRight); yield return new WaitForSecondsRealtime(1.5f);
                Note($"D-pad {(i == 1 ? "left" : "right")}: row '{Label()}', focus {Selected}, screen {Screen.width}x{Screen.height} {Screen.fullScreenMode} (display {Display.main.systemWidth}x{Display.main.systemHeight}), settings.json {Saved()}");
                if (i == 1) yield return Snap("toggle-full-screen");
            }
            yield return Back1101(); Note($"B: focus {Selected}, settings open {Btn("screen-mode") != null}");
        }
    }
}
