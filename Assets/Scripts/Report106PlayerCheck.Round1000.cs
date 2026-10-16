using System.Collections;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Racer
{
    // 1.0 evidence only (the 0.106 check component: -p106check <dir> -racerTestSave <dir>; never active without both), run in the released build:
    //   -p106kind debug1000 -p1000phase 1   main menu: the version, no debug UI anywhere; F3 shows the panel, F4 makes a report into the session
    //                                        (kept in the test save's DebugReports), F6 opens the menu; then quits with the session open
    //   -p106kind debug1000 -p1000phase 2   the relaunch: hidden again, the session resumed still OPEN; then one quick race (1 lap, solo, the
    //                                        player's vehicle driven by the road AI) started and finished
    public sealed partial class Report106PlayerCheck
    {
        Keyboard keys1000;
        IEnumerator Key1000(Key key) { InputSystem.QueueStateEvent(keys1000, new KeyboardState(key)); yield return new WaitForSecondsRealtime(.1f); InputSystem.QueueStateEvent(keys1000, new KeyboardState()); yield return new WaitForSecondsRealtime(.25f); }
        // every visible text that mentions the debug tool or its keys
        string DebugTexts() => string.Join(" | ", FindObjectsByType<UnityEngine.UI.Text>(FindObjectsSortMode.None)
            .Where(t => t.isActiveAndEnabled && (t.text.Contains("F3") || t.text.Contains("F4") || t.text.Contains("F6") || t.text.ToUpperInvariant().Contains("DEBUG")))
            .Select(t => t.text.Replace("\n", " / ")));
        string HudState(DeveloperLocationHud hud) => hud ? $"visible {hud.Visible}, panel {hud.transform.Find("Debug HUD")?.gameObject.activeInHierarchy}, menu {DeveloperLocationHud.Interactive}" : "no debug HUD";
        IEnumerator Debug1000()
        {
            keys1000 = InputSystem.AddDevice<Keyboard>("Report1000Keys"); Conditions();
            string phase = Arg("-p1000phase", "1"); var hud = DeveloperLocationHud.Instance;
            yield return new WaitForSecondsRealtime(2);
            string main = string.Join(" | ", FindObjectsByType<UnityEngine.UI.Text>(FindObjectsSortMode.None).Where(t => t.isActiveAndEnabled && (t.text.StartsWith("WOODSTOCK RUSH") || t.text.StartsWith("Version "))).Select(t => t.text));
            Note($"phase {phase}: state {flow.State}, main menu texts [{main}]");
            Note($"  at launch: {HudState(hud)}; debug texts on screen [{DebugTexts()}]");
            yield return Snap("menu-" + phase);
            yield return Key1000(Key.F3);
            Note($"  after F3: {HudState(hud)}; {hud?.SessionText.Replace("\n", " / ")}");
            yield return Snap("debug-on-" + phase);
            if (phase == "1")
            {
                yield return Key1000(Key.F4); float until = Time.realtimeSinceStartup + 10; while (hud && !hud.CommentOpen && Time.realtimeSinceStartup < until) yield return null;
                Note($"  after F4: comment dialog {hud?.CommentOpen}");
                if (hud && hud.CommentOpen) hud.SaveComment("1.0.0 release check: report made with F4");
                yield return new WaitForSecondsRealtime(.5f);
                var s = DeveloperLocationHud.Session; Note($"  session {s?.Id}: {(s == null ? "none" : s.Closed ? "CLOSED" : "OPEN")}, reports {s?.Count}, folder {s?.DirectoryPath}");
                yield return Key1000(Key.F6); Note($"  after F6: {HudState(hud)}"); yield return Snap("debug-menu-" + phase);
                hud?.CloseMenu(); yield return null; hud?.SetEnabled(false); yield return null;
                Note($"  quitting with debug {(hud && hud.Visible ? "ON" : "off")}");
                yield break;
            }
            var resumed = DeveloperLocationHud.Session; Note($"  resumed session {resumed?.Id}: {(resumed == null ? "none" : resumed.Closed ? "CLOSED" : "OPEN")}, reports {resumed?.Count}");
            yield return Key1000(Key.F3); Note($"  F3 again: {HudState(hud)}");
            // one quick race: 1 lap, solo, on the course the menu is on
            var st = flow.Save.Settings; st.opponents = false; st.traffic = false; st.laps = 1; flow.Save.SaveSettings();
            race.opponents = false; race.traffic = false; race.laps = 1;
            flow.StartRace(); yield return WaitRacing(); Bind();
            Note($"  race started: {race.courseName}, {race.laps} lap, state {flow.State}, debug {HudState(DeveloperLocationHud.Instance)}");
            yield return Snap("race-" + phase);
            var me = race.Racers[0]; Pilot(race.vehicle, me);
            yield return Drive(600, () => me.Progress.Finished || flow.State != RaceFlow.Stage.Racing);
            float wr = Time.realtimeSinceStartup; while (flow.State != RaceFlow.Stage.Results && Time.realtimeSinceStartup - wr < 60) yield return null;
            Note($"  race {(me.Progress.Finished ? "finished in " + me.ClassifiedTime(race.Clock).ToString("0.0", CultureInfo.InvariantCulture) + " s" : "NOT finished")}, resets {me.Recoveries}, state {flow.State}");
            yield return new WaitForSecondsRealtime(1.5f); yield return Snap("results-" + phase);
        }
    }
}
