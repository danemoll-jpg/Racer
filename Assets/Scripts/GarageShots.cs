using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Racer
{
    // 0.76 Part D evidence (command-line opt-in only): "-garageShots <folder>" with "-racerTestSave <folder>". In the real
    // window (any resolution) it opens the garage and saves full-screen shots of every vehicle (New models) at three turn
    // angles, then the Rider page on the motorcycle and on a car with the first, a middle and the last row selected. It logs
    // whether the preview keeps true proportions (render texture aspect = panel aspect), sits wholly on screen, never moves
    // while rows change, and does not overlap the option list. Quits when done.
    public sealed class GarageShots : MonoBehaviour
    {
        string outDir; readonly List<string> rows = new();
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            var a = Environment.GetCommandLineArgs(); int i = Array.IndexOf(a, "-garageShots");
            if (i < 0 || i + 1 >= a.Length || Array.IndexOf(a, "-racerTestSave") < 0 || FindAnyObjectByType<GarageShots>()) return;
            var g = new GameObject("Garage shots"); DontDestroyOnLoad(g); g.AddComponent<GarageShots>().outDir = a[i + 1];
        }
        IEnumerator Shot(string name) { yield return null; yield return null; yield return new WaitForEndOfFrame(); ScreenCapture.CaptureScreenshot(Path.Combine(outDir, $"{Screen.width}x{Screen.height}-{name}.png")); yield return null; }
        static Rect ScreenRect(RectTransform r) { var c = new Vector3[4]; r.GetWorldCorners(c); return Rect.MinMaxRect(c[0].x, c[0].y, c[2].x, c[2].y); }
        void Log(string s) { rows.Add(s); File.WriteAllLines(Path.Combine(outDir, $"garage-{Screen.width}x{Screen.height}.txt"), rows); }
        IEnumerator Start()
        {
            Application.runInBackground = true; Directory.CreateDirectory(outDir); AudioListener.volume = 0;
            yield return null; yield return null;
            var race = FindAnyObjectByType<RaceDirector>(); var flow = race.Flow; var menus = FindAnyObjectByType<RaceMenus>();
            typeof(StartupTitle).GetField("completed", BindingFlags.Static | BindingFlags.NonPublic)?.SetValue(null, true);
            var title = FindAnyObjectByType<StartupTitle>(); if (title) Destroy(title.gameObject);
            if (flow.State == RaceFlow.Stage.Title) flow.EnterMenuAfterTitle();
            flow.Save.Settings.master = 0; flow.Save.Settings.newMotorcycle = true; VehicleVisual.NewModels = true; flow.Save.ApplySettings();
            yield return new WaitForSecondsRealtime(1); flow.OpenGarage(); yield return new WaitForSecondsRealtime(.5f);
            var preview = (UnityEngine.UI.RawImage)typeof(RaceMenus).GetField("preview", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(menus);
            var scroll = (UnityEngine.UI.ScrollRect)typeof(RaceMenus).GetField("scroll", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(menus);
            var buttons = (List<UnityEngine.UI.Button>)typeof(RaceMenus).GetField("buttons", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(menus);
            var screen = new Rect(0, 0, Screen.width, Screen.height);
            void Geometry(string label)
            {
                var r = ScreenRect(preview.rectTransform); var list = ScreenRect((RectTransform)scroll.transform); var tex = preview.texture;
                float ra = r.width / r.height, ta = (float)tex.width / tex.height;
                bool inside = r.xMin >= -1 && r.yMin >= -1 && r.xMax <= Screen.width + 1 && r.yMax <= Screen.height + 1, apart = !r.Overlaps(list);
                Log($"{label}: preview {r.width:F0}x{r.height:F0} px at ({r.xMin:F0},{r.yMin:F0}), texture {tex.width}x{tex.height} (aspect {ta:F3} vs panel {ra:F3}{(Mathf.Abs(ta - ra) < .01f ? ", true proportions" : ", DISTORTED")}); wholly on screen {inside}; separate from the list {apart}");
            }
            foreach (var id in new[] { "moto", "atv", "original", "tourer" })
            {
                flow.SelectVehicle(id); yield return null; Geometry(id);
                foreach (var yaw in new[] { 215f, 305f, 35f }) { menus.PreviewYaw = yaw; yield return Shot($"garage-{id}-{yaw:F0}"); }
            }
            var navigate = typeof(RaceMenus).GetMethod("Navigate", BindingFlags.Instance | BindingFlags.NonPublic);
            foreach (var id in new[] { "moto", "tourer" })
            {
                flow.SelectVehicle(id); yield return null; navigate.Invoke(menus, new object[] { "rider" }); yield return null; yield return null;
                var active = buttons.Where(b => b.gameObject.activeInHierarchy && b.interactable).ToList(); Rect first = default;
                foreach (var (name, index) in new[] { ("first", 0), ("middle", active.Count / 2), ("last", active.Count - 1) })
                {
                    EventSystem.current.SetSelectedGameObject(active[index].gameObject); yield return new WaitForSecondsRealtime(.4f);
                    var r = ScreenRect(preview.rectTransform); if (name == "first") first = r;
                    var sel = ScreenRect((RectTransform)active[index].transform);
                    Geometry($"rider page on {id}, {name} row '{active[index].GetComponentInChildren<UnityEngine.UI.Text>()?.text}' (row on screen {screen.Contains(sel.center)}; preview moved {Vector2.Distance(r.center, first.center):F1} px)");
                    menus.PreviewYaw = name == "middle" ? 120 : name == "last" ? 300 : 215; yield return Shot($"rider-{id}-{name}");
                }
                menus.BackPage(); yield return null;
            }
            Log("done"); Application.Quit();
        }
    }
}
