using UnityEngine;

namespace Racer
{
    // 0.77 pause menu > Trailer Mode: the mode on / off, its camera, slow motion, HUD and guides, the Free Roam conditions
    // (Trailer Mode only; nothing is saved), a screenshot, the Screenshots folder and the controls. Controller players reach
    // everything here that has no button of its own.
    public sealed partial class RaceMenus
    {
        void RenderTrailer()
        {
            var mode = TrailerMode.Instance; var views = CameraViews.Current;
            if (!mode) { ClearCore("TRAILER MODE", "Not available here."); Row(0, "back", "Back", () => BackPage()); return; }
            bool on = TrailerMode.Active, roam = flow.Race.FreeRoam;
            string summary = on
                ? "On: nothing is drawn on screen while driving.\n" + (roam ? "Conditions: " + mode.ConditionsLabel + "\nNothing here is saved; turning it off restores your Free Roam clock and weather." : "This race does not count for records. Time and weather: Race Setup.")
                : "For filming and screenshots: a clean screen, extra cameras, slow motion" + (roam ? " and time / weather on demand." : ".") + "\nTurn it on, then Resume. A race in Trailer Mode does not count for records.";
            if (mode.LastScreenshot != null) summary += "\nLast screenshot: " + System.IO.Path.GetFileName(mode.LastScreenshot);
            ClearCore("TRAILER MODE", summary);
            int i = 0;
            Row(i++, "trailer-toggle", "Trailer Mode: " + (on ? "On" : "Off") + "   (F8)", () => { mode.Toggle(); Show(); });
            if (on && views)
            {
                Step(i++, "trailer-camera", "Camera: " + CameraViews.ShotNames[(int)views.TrailerCamera], d => { views.SelectTrailerCamera((CameraViews.Shot)(((int)views.TrailerCamera + d + 9) % 9)); Show(); });
                Row(i++, "trailer-slow", "Slow motion 0.5×: " + (mode.HalfSpeed ? "On" : "Off"), () => { mode.ToggleHalfSpeed(); Show(); });
                Row(i++, "trailer-hud", "HUD: " + (mode.HudShown ? "Shown" : "Hidden"), () => { mode.ToggleHud(); Show(); });
                Row(i++, "trailer-guides", "Arrows, gates, waypoint beacon: " + (mode.GuidesShown ? "Shown" : "Hidden"), () => { mode.ToggleGuides(); Show(); });
                if (mode.ConditionsAvailable)
                {
                    var look = WorldLook.Current; float hour = look.Trailer?.hour ?? look.Hour;
                    Row(i++, "trailer-time", "Time of day: " + mode.TimeLabel, () => { mode.CycleTimeOfDay(); Show(); });
                    Step(i++, "trailer-clock", $"Clock {Mathf.FloorToInt(hour) % 24:00}:{Mathf.FloorToInt(hour * 60) % 60:00}   (−1 h / +1 h)", d => { mode.NudgeClock(d); Show(); });
                    Row(i++, "trailer-clock-run", "Clock: " + (mode.ClockPaused ? "Paused" : "Running"), () => { mode.ToggleClock(); Show(); });
                    Row(i++, "trailer-weather", "Weather: " + mode.WeatherLabel, () => { mode.CycleWeather(); Show(); });
                    Row(i++, "trailer-moon", "Moon: " + mode.MoonLabel, () => { mode.CycleMoon(); Show(); });
                    Row(i++, "trailer-lightning", "Lightning strike now", () => { mode.LightningNow(); Show(); });
                }
                Row(i++, "trailer-shot", "Take screenshot (no menus or HUD)", () => { mode.Screenshot(); Show(); });
            }
            Row(i++, "trailer-folder", "Open Screenshots Folder", mode.OpenScreenshotFolder);
            Row(i++, "trailer-controls", "Controls", () => Help(TrailerMode.HelpText(MenuInput.Controller, roam)));
            Row(i, "back", "Back", () => BackPage());
            details.GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight = Mathf.Max(110, details.preferredHeight + 20);
        }
    }
}
