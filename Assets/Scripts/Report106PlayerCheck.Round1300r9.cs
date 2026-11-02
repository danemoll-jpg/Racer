using System.Collections;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Racer
{
    // 1.3.0-review9 evidence only (Report106PlayerCheck: -p106check <dir> -racerTestSave <dir>, a copy of Dan's save):
    //   -p106kind notes1300r9  split-screen Free Roam at the run's screen size: each half's notes (a jump distance, a speed trap result, a stunt
    //                          combo) raised through the game's own result functions (ArcadeActivities.Award with the world's real jump and speed
    //                          sites, StuntSay for the combo line), both halves at once, top / bottom and side by side; two notes stacked in one
    //                          half; a note with the acorn banner up; one-player Free Roam's jump note (unchanged) first
    public sealed partial class Report106PlayerCheck
    {
        IEnumerator Notes1300r9()
        {
            foreach (var m in InputSystem.devices.OfType<Mouse>().ToList()) InputSystem.DisableDevice(m);
            yield return ToFreeRoam("original");
            var award = typeof(ArcadeActivities).GetMethod("Award", BindingFlags.Instance | BindingFlags.NonPublic);
            var say = typeof(ArcadeActivities).GetMethod("StuntSay", BindingFlags.Instance | BindingFlags.NonPublic);
            var acts1 = flow.Activities; var jump = acts1.Sites.First(s => s.kind == ActivitySite.Kind.Jump); var trap = acts1.Sites.First(s => s.kind == ActivitySite.Kind.Speed);
            Note($"Free Roam ({race.gameObject.scene.name}), screen {Screen.width}x{Screen.height}; jump site '{jump.title}', speed trap '{trap.title}'");
            void Jump(ArcadeActivities a) => award.Invoke(a, new object[] { a.Sites.First(s => s.id == jump.id), 38.4f, "m" });
            void Trap(ArcadeActivities a) => award.Invoke(a, new object[] { a.Sites.First(s => s.id == trap.id), 31.6f, "m/s" });
            void Combo(ArcadeActivities a) => say.Invoke(a, new object[] { "BACKFLIP + ROLL  x2 / 2,340 pts\nCOMBO x2 · 3,120", 3.5f });
            // one player: today's layout
            Jump(acts1); yield return new WaitForSecondsRealtime(.6f); yield return Snap("0-one-player-jump-note");
            Note($"  one player: activity line '{SplitHud.Plain(acts1.Hud).Replace("\n", " | ")}' (RaceHud, unchanged)");
            yield return new WaitForSecondsRealtime(6.5f);
            // split screen
            var pad2 = InputSystem.AddDevice<Gamepad>("Report1300r9Pad2");
            flow.Save.Settings.splitLeftRight = false; flow.Save.SaveSettings();
            SplitScreen.Mode = SplitScreen.Kind.FreeRoam; SplitScreen.Solo = false; SplitScreen.P1Device = pad; SplitScreen.P2Device = pad2; SplitScreen.P2Ai = false; SplitScreen.Traffic = false; SplitScreen.P1High = SplitScreen.P2High = false;
            flow.StartSplit(); yield return WaitRacing(); Bind(); yield return new WaitForSecondsRealtime(1.5f);
            var split = SplitScreen.Race; var hud = FindAnyObjectByType<SplitHud>(); var roam = SplitRoam.Current;
            ArcadeActivities A(int p) => p == 1 ? flow.Activities : roam.Activities2;
            Note($"  split screen Free Roam: active {SplitScreen.Active}, player 2 '{(split.P2Car ? split.P2Car.name : "none")}', hud {(hud ? "yes" : "no")}");
            void Log(string tag) => Note($"  {tag}: P1 top '{hud.NoteText(1).Replace("\n", " | ")}' centre '{hud.CentreText(1)}';  P2 top '{hud.NoteText(2).Replace("\n", " | ")}' centre '{hud.CentreText(2)}'");
            // two notes in one half: the start note (first 7 s) and a jump: the newest on top
            Jump(A(2)); yield return new WaitForSecondsRealtime(.6f); Log("stacked (start note + jump, P2)"); yield return Snap("1-top-bottom-stacked-p2");
            yield return new WaitForSecondsRealtime(7);
            foreach (bool lr in new[] { false, true })
            {
                string L = lr ? "side-by-side" : "top-bottom";
                flow.Save.Settings.splitLeftRight = lr; split.ApplyWatching(); yield return new WaitForSecondsRealtime(1);
                Jump(A(1)); Trap(A(2)); yield return new WaitForSecondsRealtime(.6f); Log($"{L}: jump P1, speed trap P2"); yield return Snap($"2-{L}-p1-jump-p2-speedtrap");
                yield return new WaitForSecondsRealtime(6.2f);
                Trap(A(1)); Combo(A(2)); yield return new WaitForSecondsRealtime(.6f); Log($"{L}: speed trap P1, combo P2"); yield return Snap($"3-{L}-p1-speedtrap-p2-combo");
                yield return new WaitForSecondsRealtime(6.2f);
                Combo(A(1)); Jump(A(2)); yield return new WaitForSecondsRealtime(.6f); Log($"{L}: combo P1, jump P2"); yield return Snap($"4-{L}-p1-combo-p2-jump");
                yield return new WaitForSecondsRealtime(6.2f);
                AcornBanner.Current?.Show(12, 40, "Woodstock", "3 / 6", 1); Jump(A(1)); yield return new WaitForSecondsRealtime(.6f); Log($"{L}: acorn banner + jump P1"); yield return Snap($"5-{L}-p1-acorn-banner-and-jump");
                yield return new WaitForSecondsRealtime(6.2f);
            }
            flow.Save.Settings.splitLeftRight = false; flow.Save.SaveSettings();
        }
    }
}
