using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Racer
{
    // 1.1 (ported from Chastain Rush's Round 57 F GameMenus.Replays): race replays in the menus. On the finish screen (the results), WATCH REPLAY
    // (X / V) and SAVE REPLAY (Y / K), Chastain's buttons (nothing else uses them on the results screen), as rows under the first action and as
    // shortcuts. The REPLAYS page (main menu, and the results of a quick race): the kept #1 lap and race of each Top 10 board (watch; replaced only
    // by a new #1), the saved replays (watch or delete) and (1.3.0-review6) the newest 30 races finished (watch, save or delete), newest first, each marked when recorded on another build or before its course changed. A replay of
    // another course loads that course first (RaceFlow.LoadCourseForReplay). While a replay plays, the menu is hidden and its buttons wait.
    public sealed partial class RaceMenus
    {
        InputAction watchAction, saveAction; string replayNote = ""; float replayNoteUntil; string replayPick; bool replayHidden;
        ReplayRecorder Recorder => flow ? flow.Replays : null;
        bool ReplayOffered => flow.State == RaceFlow.Stage.Results && page == "" && modalConfirm == null && Recorder && Recorder.Last != null && !ReplayViewer.Active;
        // the REPLAYS row on the results: quick races only (a campaign event, a playlist or split screen goes on from its own results)
        bool ReplaysFromResults => flow.State == RaceFlow.Stage.Results && CampaignRun.Active == null && RacePlaylists.Active == null && !SplitScreen.Active;

        // RaceMenus.Update / LateUpdate: while a replay plays the menu (the HUD's canvas) is hidden and its confirm button off; true while it is
        bool ReplayHide()
        {
            bool on = ReplayViewer.Active;
            if (on != replayHidden && shade)
            {
                replayHidden = on; var canvas = shade.transform.parent.GetComponent<Canvas>(); if (canvas) canvas.enabled = !on;
                if (on) { submit.Disable(); if (EventSystem.current) EventSystem.current.SetSelectedGameObject(null); }
                else { submit.Enable(); MenuInput.ConsumeThroughRelease(); Show(); }
            }
            return on;
        }

        // after RenderCore: the results' WATCH / SAVE REPLAY rows (and REPLAYS), under the first action row
        void ReplayRows()
        {
            if (watchAction == null)
            {
                watchAction = new InputAction("Watch replay", InputActionType.Button, "<Gamepad>/buttonWest"); watchAction.AddBinding("<Keyboard>/v"); watchAction.Enable();
                saveAction = new InputAction("Save replay", InputActionType.Button, "<Gamepad>/buttonNorth"); saveAction.AddBinding("<Keyboard>/k"); saveAction.Enable();
            }
            if (!ReplayOffered) return;
            int n = buttons.FindLastIndex(b => b.gameObject.activeSelf) + 1;
            var anchor = buttons.FirstOrDefault(b => b.gameObject.activeSelf && (b.name == "primary" || b.name == "campaign-continue" || b.name == "rematch"))
                      ?? buttons.FirstOrDefault(b => b.gameObject.activeSelf && b.transform.parent == content);
            bool pad = MenuInput.Controller;
            Row(n, "watch-replay", "WATCH REPLAY   (" + (pad ? "X" : "V") + ")", WatchLastReplay);
            Row(n + 1, "save-replay", (Time.unscaledTime < replayNoteUntil ? replayNote : "SAVE REPLAY") + "   (" + (pad ? "Y" : "K") + ")", SaveLastReplay);
            int at = anchor ? anchor.transform.GetSiblingIndex() + 1 : buttons[n].transform.GetSiblingIndex();
            buttons[n].transform.SetSiblingIndex(at); buttons[n + 1].transform.SetSiblingIndex(at + 1);
            if (ReplaysFromResults) { Row(n + 2, "replays", "Replays", () => Navigate("replays")); var records = buttons.FirstOrDefault(b => b.gameObject.activeSelf && b.name == "records"); if (records) buttons[n + 2].transform.SetSiblingIndex(records.transform.GetSiblingIndex() + 1); }
        }
        // RaceMenus.Update: X watches, Y saves while the results show the rows
        void UpdateReplayKeys()
        {
            if (watchAction == null || !ReplayOffered || !flow.MenuVisible || MenuInput.Blocked || OwnsTextInput) return;
            if (watchAction.WasPressedThisFrame()) { MenuInput.ConsumeThroughRelease(); WatchLastReplay(); }
            else if (saveAction.WasPressedThisFrame()) { MenuInput.ConsumeThroughRelease(); SaveLastReplay(); }
        }
        void ReplayNote(string s) { replayNote = s; replayNoteUntil = Time.unscaledTime + 3f; }

        // the race just run
        public void WatchLastReplay()
        {
            var rec = Recorder; if (!rec || rec.Last == null || ReplayViewer.Active) { ReplayNote("NO REPLAY"); Show(); return; }
            if (EventSystem.current) EventSystem.current.SetSelectedGameObject(null);
            ReplayViewer.Open(rec.Last);
        }
        public void SaveLastReplay() { var rec = Recorder; ReplayNote(rec ? rec.SaveLast() : "SAVE FAILED"); Show(); }

        // ---------------------------------------------------------------- the REPLAYS page
        bool RenderReplayPages()
        {
            if (page == "replays") { RenderReplays(); return true; }
            if (page == "replay-one") { RenderReplayOne(); return true; }
            return false;
        }
        static string Stale(ReplayData d) { var s = d.Staleness; return s == "" ? "" : "   ·   " + s; }
        void ReplayRow(int index, string id, string first, string second, System.Action act)
        {
            Row(index, id, first + "\n" + second, act); var b = buttons[index];
            b.GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight = 58; var label = b.GetComponentInChildren<UnityEngine.UI.Text>(true); label.fontSize = 17; label.alignment = TextAnchor.MiddleLeft;
        }
        void RenderReplays()
        {
            // 1.3.0-review6 Part D: one list, newest first: the #1 lap and race of each Top 10 board, the saved ones and the recent 30
            var all = ReplayData.List(true).Select(x => (x.path, x.head, tag: x.head.kind == "lap" ? "#1 LAP" : "#1 RACE"))
                .Concat(ReplayData.List(false).Select(x => (x.path, x.head, tag: "SAVED")))
                .Concat(ReplayData.List(ReplayData.RecentFolder).Select(x => (x.path, x.head, tag: "RECENT")))
                .OrderByDescending(x => x.head.date, System.StringComparer.Ordinal).ToList();
            if (renderedKey != PageKey) selections[flow.State] = 0;   // opens on its first row (coming back, on the one left)
            ClearCore("REPLAYS", $"Every race you finish is kept: the newest {ReplayData.RecentKept} (RECENT), each Top 10 board's #1 lap and race (#1)\nand the ones you saved (SAVED). SAVE on a recent one keeps it for good.");
            int n = 0;
            foreach (var (path, d, tag) in all)
            {
                if (n >= 150) break; string p = path; bool best = tag.StartsWith("#1"); string who = d.split || string.IsNullOrEmpty(d.label) || tag == "SAVED" ? "" : d.label + "   ·   ";
                ReplayRow(n++, (best ? "best-" : tag == "SAVED" ? "saved-" : "recent-") + n, $"{tag}   ·   {d.Title}" + (d.bestTime > 0 ? "   ·   " + RaceHud.FormatTime(d.bestTime) : ""), $"{who}{d.setup}   ·   {d.When}{Stale(d)}",
                    best ? () => WatchReplayFile(p) : () => { replayPick = p; Navigate("replay-one"); });
            }
            if (all.Count == 0) details.text += "\nNo replays yet: finish a race.";
            details.GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight = 70;
            Row(n, "back", "Back", () => BackPage());
        }
        void RenderReplayOne()
        {
            if (string.IsNullOrEmpty(replayPick) || !File.Exists(replayPick)) { page = "replays"; RenderReplays(); return; }
            ReplayData d = null; try { d = ReplayData.Read(replayPick, true); } catch (System.Exception) { }
            if (d == null) { ClearCore("REPLAY", "This replay cannot be read."); Row(0, "delete", "DELETE", () => Confirm("DELETE THIS REPLAY?", Path.GetFileName(replayPick), DeletePick, "DELETE")); Row(1, "back", "Back", () => BackPage()); return; }
            long kb = new FileInfo(replayPick).Length / 1024; string stale = d.Staleness; if (renderedKey != PageKey) selections[flow.State] = 0;   // a new page opens on WATCH
            bool recent = d.kind == "recent"; string name = recent ? d.Title + (d.bestTime > 0 ? "   ·   " + RaceHud.FormatTime(d.bestTime) : "") + "   ·   " + d.When : d.label;
            ClearCore(recent ? "RECENT REPLAY" : "SAVED REPLAY", $"{name}\n{(recent && !d.split && d.label != "" ? d.label + "   ·   " : "")}{d.setup}   ·   {kb:N0} KB" + (stale != "" ? "\n" + stale : "") + (recent ? $"\nOne of the newest {ReplayData.RecentKept}: SAVE keeps it for good." : ""));
            int n = 0;
            Row(n++, "watch", "WATCH", () => WatchReplayFile(replayPick));
            if (recent) Row(n++, "save", "SAVE", SavePick);
            Row(n++, "delete", "DELETE", () => Confirm("DELETE THIS REPLAY?", recent ? d.Title + "   ·   " + d.When : d.label, DeletePick, "DELETE"));
            Row(n, "back", "Back", () => BackPage());
        }
        // a recent replay kept for good: written to saved/ (its recent copy goes)
        void SavePick()
        {
            string note;
            try { var d = ReplayData.Read(replayPick); d.recentAs = replayPick; note = ReplayData.Save(d); if (note == "REPLAY SAVED") replayPick = ReplayData.LastSaved; }
            catch (System.Exception e) { Debug.LogWarning("[Replay] save failed: " + e.Message); note = "SAVE FAILED"; }
            flow.Notify(note, 3); Show();
        }
        void DeletePick()
        {
            try { if (File.Exists(replayPick)) File.Delete(replayPick); } catch (System.Exception e) { Debug.LogWarning("[Replay] delete failed: " + e.Message); }
            replayPick = null; BackPage();
        }
        // a replay file from the page: here when it is this course (and not Free Roam), else its course loads first and it plays there
        public void WatchReplayFile(string path)
        {
            ReplayData d;
            try { d = ReplayData.Read(path); } catch (System.Exception e) { Debug.LogWarning("[Replay] " + e.Message); flow.Notify("This replay cannot be read", 4); return; }
            if (page != "replays" && page != "replay-one") { if (flow.State != RaceFlow.Stage.Ready) flow.QuitRace(); ResetPages(); Navigate("replays"); }
            int here = RacePlaylists.CourseOf(flow.gameObject.scene);
            if (flow.InRoamWorld || here != d.course)
            {
                if (flow.State == RaceFlow.Stage.Results) flow.QuitRace();
                flow.LoadCourseForReplay(d.course, path); return;
            }
            if (EventSystem.current) EventSystem.current.SetSelectedGameObject(null);
            ReplayViewer.Open(d);
        }
        void DisposeReplayInput() { watchAction?.Dispose(); saveAction?.Dispose(); }
    }
}
