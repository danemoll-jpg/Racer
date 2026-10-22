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
    // by a new #1) and the saved replays (watch or delete), each marked when recorded on another build or before its course changed. A replay of
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
            var best = ReplayData.List(true); var saved = ReplayData.List(false);
            if (renderedKey != PageKey) selections[flow.State] = 0;   // opens on its first row (coming back, on the one left)
            ClearCore("REPLAYS", "Every race is recorded. The #1 lap and the #1 race of each Top 10 board are kept here, replaced by a new #1.\nSAVE REPLAY on the finish screen keeps any race until you delete it.");
            int n = 0;
            foreach (var (path, d) in best.OrderBy(x => x.head.Title).ThenBy(x => x.head.setup).ThenBy(x => x.head.kind))
            {
                if (n >= 40) break; string p = path;
                ReplayRow(n++, "best-" + n, $"{(d.kind == "lap" ? "#1 LAP" : "#1 RACE")}   ·   {d.Title}   ·   {RaceHud.FormatTime(d.bestTime)}", $"{d.setup}   ·   {d.When}{Stale(d)}", () => WatchReplayFile(p));
            }
            if (best.Count == 0) details.text += "\nNo #1 replays yet: set a #1 lap or race on a Top 10 board.";
            foreach (var (path, d) in saved)
            {
                if (n >= 80) break; string p = path;
                ReplayRow(n++, "saved-" + n, "SAVED   ·   " + d.label, $"{d.setup}{Stale(d)}", () => { replayPick = p; Navigate("replay-one"); });
            }
            details.GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight = 70;
            Row(n, "back", "Back", () => BackPage());
        }
        void RenderReplayOne()
        {
            if (string.IsNullOrEmpty(replayPick) || !File.Exists(replayPick)) { page = "replays"; RenderReplays(); return; }
            ReplayData d = null; try { d = ReplayData.Read(replayPick, true); } catch (System.Exception) { }
            if (d == null) { ClearCore("REPLAY", "This replay cannot be read."); Row(0, "delete", "DELETE", () => Confirm("DELETE THIS REPLAY?", Path.GetFileName(replayPick), DeletePick, "DELETE")); Row(1, "back", "Back", () => BackPage()); return; }
            long kb = new FileInfo(replayPick).Length / 1024; string stale = d.Staleness; if (renderedKey != PageKey) selections[flow.State] = 0;   // a new page opens on WATCH
            ClearCore("SAVED REPLAY", $"{d.label}\n{d.setup}   ·   {kb:N0} KB" + (stale != "" ? "\n" + stale : ""));
            Row(0, "watch", "WATCH", () => WatchReplayFile(replayPick));
            Row(1, "delete", "DELETE", () => Confirm("DELETE THIS REPLAY?", d.label, DeletePick, "DELETE"));
            Row(2, "back", "Back", () => BackPage());
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
