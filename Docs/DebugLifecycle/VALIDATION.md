# Debug session lifecycle correction

Safety checkpoint: clean main `23e383f95dc7ec1cf4a0570699fbade6531a37ed`.

Successful export now persists CLOSED/exported state in the session folder and ZIP. Closed sessions reject additional saves and numbering requests. F4 lazily creates a new timestamped session with BUG-001. Export writes a temporary archive and completes it before reporting success; a failed archive restores OPEN state and retains the reports. Existing history is never automatically deleted.

The Debug HUD/menu show session ID, state and report count. START NEW DEBUG SESSION closes without export and waits for the next capture. Unexported reports require explicit confirmation with Keep Current Session selected by default. F4 cannot bypass this dialog. Folder access remains available for closed history.

All **28 focused checks passed** in Unity 6000.6.1f1 using real Input System keyboard/controller/mouse events. See `checks.txt` / `done.txt` and `Assets/Scripts/DebugSessionLifecycleChecks.cs`.

- Capture BUG-001 and BUG-002, navigate to Export using controller, export successfully.
- Read CLOSED state from folder JSON and exported ZIP; verify both screenshots and entries.
- Reject a save to the closed session.
- Press F4 again: different timestamped folder, BUG-001 only, no old screenshots or report entries.
- Hash every previous session file and ZIP before/after: byte-for-byte preservation.
- Navigate START NEW DEBUG SESSION; require confirmation; ignore F4 while pending; controller B cancels; default keyboard Enter keeps the session; mouse explicit close preserves unexported history.
- No empty folder from manual close; following F4 starts another BUG-001.
- Lock only the test screenshot to force archive failure; confirm in-memory and on-disk session stay OPEN and old ZIP unchanged.

Visual inspection: `menu-open-session.png`, `menu-exported-session.png`, `confirmation.png`, `menu-new-session.png`, `hud-new-session.png`. Session labels, counts, controls and confirmation are readable without overlap. The batch-rendered menu screenshots are UI evidence only, not world-rendering acceptance.

Tests use isolated `Docs/DebugLifecycle/Sessions` history and `Temp/DebugLifecycleSave`. `session-paths.txt` identifies the three retained sessions and exported ZIP. No actual user DebugReports folder or previous export was modified or deleted. No track, physics, AI or unrelated gameplay tests were needed. Targeted verification is complete; proceed to fresh build and release.
