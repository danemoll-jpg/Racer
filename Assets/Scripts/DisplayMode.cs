using System;
using UnityEngine;

namespace Racer
{
    // 1.1.0 (Steam Deck): Settings > Display "Screen mode": Full screen (the default, also for every save from before) or Windowed.
    // On start the game applies the saved choice itself, so a stale windowed size Unity remembered (its Screenmanager values) no longer
    // wins: full screen is the display's native resolution; a window never comes out bigger than the display.
    public static class DisplayMode
    {
        static bool started;
        // test players and checks are started with explicit -screen-* arguments: those win at start (the menu option still works)
        static bool CommandLineScreen => Array.Exists(Environment.GetCommandLineArgs(), a => a.StartsWith("-screen-", StringComparison.OrdinalIgnoreCase) || a.Equals("-popupwindow", StringComparison.OrdinalIgnoreCase));
        static Vector2Int Native
        {
            get
            {
                int w = Display.main.systemWidth, h = Display.main.systemHeight;
                if (w <= 0 || h <= 0) { var r = Screen.currentResolution; w = r.width; h = r.height; }
                return new Vector2Int(w, h);
            }
        }
        // once per launch, from RacerSave.ApplySettings
        public static void Startup(bool windowed)
        {
            if (started) return; started = true;
            if (Application.isEditor || Application.isBatchMode || CommandLineScreen) return;
            Set(windowed);
        }
        // the menu option (and the start)
        public static void Set(bool windowed)
        {
            var n = Native;
            if (!windowed) { Screen.SetResolution(n.x, n.y, FullScreenMode.FullScreenWindow); return; }
            // a window: keep the size it has when it fits inside the display (with room for its title bar), otherwise 80% of the display
            int w = Screen.width, h = Screen.height;
            bool fits = Screen.fullScreenMode == FullScreenMode.Windowed && w > 0 && h > 0 && w <= n.x * .9f && h <= n.y * .9f;
            if (!fits) { w = Mathf.RoundToInt(n.x * .8f); h = Mathf.RoundToInt(n.y * .8f); }
            Screen.SetResolution(w, h, FullScreenMode.Windowed);
        }
    }
}
