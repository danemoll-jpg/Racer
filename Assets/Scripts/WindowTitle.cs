#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
using System;
using System.Runtime.InteropServices;
using System.Text;
#endif
using UnityEngine;

namespace Racer
{
    /// <summary>0.84: the game window reads "Woodstock Rush". productName stays "Racer" (the save folder, records, settings
    /// and debug reports depend on it), so the title is set on the window itself after it opens.</summary>
    public static class WindowTitle
    {
        public const string Title = "Woodstock Rush";
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        delegate bool EnumProc(IntPtr window, IntPtr data);
        [DllImport("user32.dll")] static extern bool EnumWindows(EnumProc proc, IntPtr data);
        [DllImport("user32.dll")] static extern uint GetWindowThreadProcessId(IntPtr window, out uint process);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)] static extern int GetClassNameW(IntPtr window, StringBuilder name, int size);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)] static extern bool SetWindowTextW(IntPtr window, string text);
        [DllImport("kernel32.dll")] static extern uint GetCurrentProcessId();

        static uint self;
        [AOT.MonoPInvokeCallback(typeof(EnumProc))]
        static bool Visit(IntPtr window, IntPtr data)
        {
            GetWindowThreadProcessId(window, out uint process);
            if (process != self) return true;
            var name = new StringBuilder(64);
            GetClassNameW(window, name, name.Capacity);
            if (name.ToString() == "UnityWndClass") SetWindowTextW(window, Title);
            return true;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Apply()
        {
            try { self = GetCurrentProcessId(); EnumWindows(Visit, IntPtr.Zero); }
            catch (Exception e) { Debug.LogWarning("Window title not set: " + e.Message); }
        }
#endif
    }
}
