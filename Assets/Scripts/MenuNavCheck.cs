using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Controls;

namespace Racer
{
    // 0.92 Part A evidence only: `-menuNavCheck <dir> -racerTestSave <dir>` drives the built player from a cold launch the
    // way Dan plays, with an emulated controller only (an Input System gamepad): A through the title, Start in Free Roam,
    // then on the Free Roam menu, the main menu, Race Setup and the race pause menu presses down repeatedly from the top with
    // the D-pad and again with the left stick, logging the focused row after every press, a frame per press, and whether the
    // sequence equals the rows as drawn. Never active without both arguments.
    public sealed class MenuNavCheck : MonoBehaviour
    {
        string outDir; Gamepad pad; readonly List<string> log = new(); int failures, shot;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            var a = Environment.GetCommandLineArgs(); int i = Array.IndexOf(a, "-menuNavCheck");
            if (i < 0 || i + 1 >= a.Length || Array.IndexOf(a, "-racerTestSave") < 0 || FindAnyObjectByType<MenuNavCheck>()) return;
            var g = new GameObject("Menu navigation check"); DontDestroyOnLoad(g); g.AddComponent<MenuNavCheck>().outDir = a[i + 1];
        }
        void Note(string line) { log.Add(line); Debug.Log("MENUNAV " + line); File.WriteAllLines(Path.Combine(outDir, "menunav.txt"), log); }
        static RaceFlow Flow => FindAnyObjectByType<RaceFlow>();
        static GameObject Selected => EventSystem.current ? EventSystem.current.currentSelectedGameObject : null;
        static string Name(GameObject g) => g ? g.name : "(nothing)";
        IEnumerator Frames(int n) { for (int i = 0; i < n; i++) yield return null; }
        IEnumerator Wait(float s) { float t = Time.realtimeSinceStartup + s; while (Time.realtimeSinceStartup < t) yield return null; }
        // -menuNavReal: the presses go into the connected controller's own device (its XInput layout and state format)
        // instead of an added test gamepad; held for a human-length press (-menuNavHold seconds, default 0.12).
        bool real; float hold = .12f;
        void Write(ButtonControl[] down, Vector2 stick)
        {
            using (StateEvent.From(pad, out var ptr))
            {
                foreach (var c in new[] { pad.dpad.up, pad.dpad.down, pad.dpad.left, pad.dpad.right, pad.startButton, pad.buttonEast, pad.buttonSouth, pad.rightShoulder }) c.WriteValueIntoEvent(0f, ptr);
                pad.leftStick.WriteValueIntoEvent(stick, ptr);
                foreach (var b in down) b.WriteValueIntoEvent(1f, ptr);
                InputSystem.QueueEvent(ptr);
            }
        }
        ButtonControl Control(GamepadButton b) => b switch { GamepadButton.DpadUp => pad.dpad.up, GamepadButton.DpadDown => pad.dpad.down, GamepadButton.DpadLeft => pad.dpad.left, GamepadButton.DpadRight => pad.dpad.right, GamepadButton.Start => pad.startButton, GamepadButton.B => pad.buttonEast, GamepadButton.RightShoulder => pad.rightShoulder, _ => pad.buttonSouth };
        IEnumerator Tap(GamepadButton b) { Write(new[] { Control(b) }, Vector2.zero); yield return Wait(hold); Write(new ButtonControl[0], Vector2.zero); yield return Frames(3); yield return Wait(.15f); }
        IEnumerator Stick(Vector2 v) { Write(new ButtonControl[0], v); yield return Wait(hold); Write(new ButtonControl[0], Vector2.zero); yield return Frames(3); yield return Wait(.15f); }
        // The menu's visible, usable rows as drawn: top to bottom, then left to right.
        static List<UnityEngine.UI.Selectable> Rows()
        {
            Canvas.ForceUpdateCanvases(); var card = GameObject.Find("Race menu"); if (!card) return new();
            return card.GetComponentsInChildren<UnityEngine.UI.Selectable>(false).Where(b => b.interactable && b.gameObject.activeInHierarchy && !(b is UnityEngine.UI.Scrollbar) && !(b is UnityEngine.UI.InputField) && b.navigation.mode != UnityEngine.UI.Navigation.Mode.None)
                .OrderByDescending(b => Mathf.Round(((RectTransform)b.transform).TransformPoint(((RectTransform)b.transform).rect.center).y)).ThenBy(b => b.transform.position.x).ToList();
        }
        void Shot(string label) { ScreenCapture.CaptureScreenshot(Path.Combine(outDir, $"{shot++:000}-{label}.png")); }
        // From the top row, down (rows) times: the focus must visit every row in drawn order (wrapping at the end).
        // the one-time "campaign updated" notice (a single OK row), closed with A as a player would
        IEnumerator Notice() { yield return Wait(.3f); var ok = GameObject.Find("ok"); if (ok && ok.activeInHierarchy && Rows().Count == 1) { Note("closed the one-time notice with A"); yield return Tap(GamepadButton.A); yield return Wait(.4f); } }
        IEnumerator Walk(string label, bool stick)
        {
            yield return Notice(); var rows = Rows(); if (rows.Count == 0) { Note($"{label}: no rows"); failures++; yield break; }
            Note($"{label}: rows as drawn: {string.Join(" | ", rows.Select(r => r.name + "  [" + (r.GetComponentInChildren<UnityEngine.UI.Text>(true)?.text ?? "").Replace("\n", " / ") + "]"))}");
            Note($"{label}: links: {string.Join("; ", rows.Select(r => $"{r.name}: up {Name(r.FindSelectableOnUp()?.gameObject)}, down {Name(r.FindSelectableOnDown()?.gameObject)}, nav {r.navigation.mode}"))}");
            Note($"{label}: focused when shown: {Name(Selected)}");
            var menus = FindAnyObjectByType<RaceMenus>(); string fault = menus ? menus.NavigationOrderFault() : "no menus";
            if (fault != null) failures++; Note($"{label}: structural order check ({menus?.PageName}): {(fault == null ? "PASS (down from the top follows the drawn order)" : "FAIL " + fault)}");
            // to the top row the way a player would: up until the focus is on the first drawn row (at most rows presses)
            for (int i = 0; i < rows.Count + 2 && Selected != rows[0].gameObject; i++) { if (stick) yield return Stick(Vector2.up); else yield return Tap(GamepadButton.DpadUp); }
            var seen = new List<string> { Name(Selected) }; Shot($"{label.Replace(' ', '-')}-{(stick ? "stick" : "dpad")}-0-{Name(Selected)}");
            for (int i = 0; i < rows.Count; i++) { if (stick) yield return Stick(Vector2.down); else yield return Tap(GamepadButton.DpadDown); seen.Add(Name(Selected)); Shot($"{label.Replace(' ', '-')}-{(stick ? "stick" : "dpad")}-{i + 1}-{Name(Selected)}"); yield return Frames(2); }
            var expected = rows.Select(r => r.name).Append(rows[0].name).ToList();
            bool ok = seen.SequenceEqual(expected); if (!ok) failures++;
            Note($"{label} {(stick ? "LEFT STICK" : "D-PAD")}: {(ok ? "PASS" : "FAIL")} pressed down {rows.Count} times from the top: {string.Join(" > ", seen)}{(ok ? "" : "   EXPECTED " + string.Join(" > ", expected))}");
        }
        IEnumerator Choose(string row)
        {
            yield return Notice();
            for (int i = 0; i < 30 && Name(Selected) != row; i++) yield return Tap(GamepadButton.DpadDown);
            if (Name(Selected) != row) { Note($"could not reach '{row}' (focus on {Name(Selected)})"); failures++; yield break; }
            yield return Tap(GamepadButton.A); yield return Wait(.5f);
        }
        // the focus to a named row (down, at most 40 presses) without pressing A
        IEnumerator GoTo(string row) { for (int i = 0; i < 40 && Name(Selected) != row; i++) yield return Tap(GamepadButton.DpadDown); if (Name(Selected) != row) { Note($"could not reach '{row}' (focus on {Name(Selected)})"); failures++; } }
        IEnumerator Until(Func<bool> condition, float seconds) { float t = Time.realtimeSinceStartup + seconds; while (!condition() && Time.realtimeSinceStartup < t) yield return null; }
        IEnumerator Start()
        {
            Directory.CreateDirectory(outDir); AudioListener.volume = 0;
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            Note($"version {Application.version}, screen {Screen.width}x{Screen.height}");
            Note("devices at launch: " + string.Join("; ", InputSystem.devices.Select(d => $"{d.name} ({d.layout}, {d.description.interfaceName}, {d.description.product})")));
            var args = Environment.GetCommandLineArgs(); real = Array.IndexOf(args, "-menuNavReal") >= 0;
            int h = Array.IndexOf(args, "-menuNavHold"); if (h >= 0 && h + 1 < args.Length) hold = float.Parse(args[h + 1], System.Globalization.CultureInfo.InvariantCulture);
            pad = real ? Gamepad.all.FirstOrDefault(g => g.description.interfaceName == "XInput") : null;
            if (pad == null) { if (real) Note("no XInput controller: using a test gamepad"); pad = InputSystem.AddDevice<Gamepad>("MenuNavCheck pad"); }
            Note($"pressing on: {pad.name} ({pad.layout}), hold {hold:F2} s");
            // the cursor rests over the middle of the menu, as when the game is launched full screen
            Mouse.current?.WarpCursorPosition(new Vector2(Screen.width / 2f, Screen.height / 2f));
            yield return Until(() => Flow && Flow.Started && !LoadingScreen.Holding, 120);
            // the title: A, as Dan does (once the title accepts input)
            for (int i = 0; i < 40 && Flow && Flow.State == RaceFlow.Stage.Title; i++) { yield return Tap(GamepadButton.A); yield return Wait(.5f); }
            yield return Until(() => Flow && !LoadingScreen.Holding && (Flow.State == RaceFlow.Stage.Racing || Flow.MenuVisible), 120); yield return Wait(2);
            Note($"after the title: {Flow.State}, Free Roam {Flow.Race.FreeRoam}");
            if (Flow.State == RaceFlow.Stage.Racing) { yield return Tap(GamepadButton.Start); yield return Wait(.8f); }
            Note($"menu: {Flow.State}, Free Roam menu {Flow.RoamMenu}");
            yield return Walk("free-roam menu", false); yield return Walk("free-roam menu", true);
            // RACE from the Free Roam menu opens Race Setup on the course (a scene load); B then shows the main menu
            yield return Choose("race"); yield return Until(() => Flow && Flow.Started && !LoadingScreen.Holding && Flow.State == RaceFlow.Stage.Ready, 120); yield return Wait(2);
            yield return Walk("race setup", false);
            yield return Tap(GamepadButton.B); yield return Wait(.6f);
            Note($"after B: {Flow.State}, Free Roam menu {Flow.RoamMenu}");
            yield return Walk("main menu", false); yield return Walk("main menu", true);
            // 0.92: a tour of the other pages from the main menu (each walked with the D-pad, then B back)
            yield return Choose("settings"); for (int tab = 0; tab < 4; tab++) { yield return Walk("settings tab " + (tab + 1), false); yield return Tap(GamepadButton.RightShoulder); }
            yield return Tap(GamepadButton.B); yield return Wait(.5f);
            yield return Choose("garage"); yield return Walk("garage", false); yield return Tap(GamepadButton.B); yield return Wait(.5f);
            yield return Choose("split"); yield return Walk("split-screen setup", false); yield return Tap(GamepadButton.B); yield return Wait(.5f);
            yield return Choose("records"); yield return Walk("records", false); yield return Tap(GamepadButton.B); yield return Wait(.5f);
            yield return Choose("campaign"); yield return Wait(.5f); yield return GoTo("continue"); yield return Tap(GamepadButton.A); yield return Wait(.6f);
            yield return Walk("campaign event page", false);
            yield return GoTo("event-vehicle"); yield return Tap(GamepadButton.A); yield return Wait(.6f);
            yield return Walk("vehicle choice (garage view)", false);
            yield return Tap(GamepadButton.B); yield return Wait(.4f); yield return Tap(GamepadButton.B); yield return Wait(.4f); yield return Tap(GamepadButton.B); yield return Wait(.6f);
            Note($"back on: {Flow.State} / {FindAnyObjectByType<RaceMenus>()?.PageName}");
            yield return Choose("race"); yield return Wait(.5f); yield return Choose("start");
            yield return Until(() => Flow && !LoadingScreen.Holding && Flow.State == RaceFlow.Stage.Racing, 120); yield return Wait(1);
            yield return Tap(GamepadButton.Start); yield return Wait(.8f);
            Note($"pause: {Flow.State}");
            yield return Walk("race pause menu", false); yield return Walk("race pause menu", true);
            Note(failures == 0 ? "ALL PASS" : $"FAILURES {failures}");
            File.WriteAllText(Path.Combine(outDir, "done.txt"), failures.ToString());
            yield return Wait(1); Application.Quit();
        }
    }
}
