using UnityEngine;

namespace Racer
{
    // 0.95 Part F: the one-off unlock panel (UnlockNotice): a page of the main menu. While driving in single-player Free Roam
    // it pauses (the vehicle stopped) and opens; at the main menu (or the Free Roam menu) it opens over it. OK (A) closes it,
    // remembers it and resumes driving if it paused it. B does not close it.
    public sealed partial class RaceMenus
    {
        bool unlockResume; AudioClip fanfare; AudioSource fanfareSource;
        void UpdateUnlock()
        {
            if (UnlockNotice.Pending.Count == 0 || UnlockNotice.Showing != null || SplitScreen.Active || modalConfirm != null || page == "keyboard" || LoadingScreen.Holding || !flow.Started) return;
            if (flow.State == RaceFlow.Stage.Racing && flow.Race.FreeRoam && !TrailerMode.Active)
            {
                var car = flow.Race.vehicle; car.Body.linearVelocity = car.Body.angularVelocity = Vector3.zero; // stopped where it is
                flow.Pause(); unlockResume = true; return; // Free Roam's pause is the main menu: the panel opens next frame
            }
            if (flow.State != RaceFlow.Stage.Ready || (page != "" && page != "roam")) return;
            UnlockNotice.Showing = UnlockNotice.Pending[0]; UnlockNotice.Pending.RemoveAt(0);
            if (!fanfare) { fanfare = Synth.Notes("Unlock fanfare", new[] { 523.25f, 659.25f, 783.99f, 1046.5f }, .14f, .9f); fanfareSource = gameObject.AddComponent<AudioSource>(); fanfareSource.playOnAwake = false; fanfareSource.spatialBlend = 0; fanfareSource.ignoreListenerPause = true; }
            fanfareSource.PlayOneShot(fanfare, flow.Save.Settings.feedback * .55f);
            Navigate("unlock");
        }
        void RenderUnlock()
        {
            var n = UnlockNotice.Showing; if (n == null) { page = ""; Show(); return; }
            var p = VehicleProfile.Find(n.vehicle);
            ClearCore(n.heading, "UNLOCKED:  " + p.Name.ToUpperInvariant() + "\nChoose it in the Garage.");
            details.fontSize = 26; details.alignment = TextAnchor.MiddleCenter; details.GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight = 76; details.color = new Color(1, .84f, .35f);
            Row(0, "unlock-ok", "OK", CloseUnlock);
            var colors = buttons[0].colors; colors.normalColor = new(.1f, .38f, .35f); buttons[0].colors = colors; buttons[0].GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight = 54;
            buttons[0].GetComponentInChildren<UnityEngine.UI.Text>(true).alignment = TextAnchor.MiddleCenter;
            var strip = PreviewStrip("Unlocked vehicle", buttons[0].transform.GetSiblingIndex(), 250);
            PreviewCard(strip, p, -1, false, p.Name + "  ·  " + p.Class, new Color(1, .84f, .35f), 470);
            var stats = Rect("Unlocked vehicle stats", strip); stats.gameObject.AddComponent<UnityEngine.UI.LayoutElement>().preferredWidth = 380;
            var v = stats.gameObject.AddComponent<UnityEngine.UI.VerticalLayoutGroup>(); v.padding = new RectOffset(8, 8, 40, 8); v.spacing = 10; v.childControlWidth = v.childControlHeight = true; v.childForceExpandHeight = false;
            StatBars(stats, p);
        }
        void CloseUnlock()
        {
            var n = UnlockNotice.Showing; UnlockNotice.Showing = null; if (n != null) UnlockNotice.MarkSeen(flow.Save, n.id);
            page = pages.Count > 0 ? pages.Pop() : ""; flow.Click();
            if (unlockResume && UnlockNotice.Pending.Count == 0) { unlockResume = false; flow.Resume(); } else Show();
        }
        public bool UnlockOpen => page == "unlock" && UnlockNotice.Showing != null;
    }
}
