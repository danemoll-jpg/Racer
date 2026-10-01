# Debug review controls

Debug Mode defaults off. It replaces the earlier F3 coordinate HUD / F4 clipboard shortcut.

- **F3:** toggle Debug Mode.
- **F4:** capture the current view and metadata, then open a comment dialog.
- **F6:** debug menu. Controller Start opens this menu while Debug Mode is on.
- **Enter:** save the comment. **Shift+Enter:** new line. **Esc / controller B:** cancel.

The HUD shows course, direction, race/free-roam mode, XYZ, heading, vehicle, speed, lap/checkpoint and session report count. The debug menu provides Resume, Capture Bug, Debug Fly, Return to Vehicle, Export Bug Report ZIP, Open Debug Report Folder, Toggle Debug HUD and Exit Debug Mode.

F4 freezes the captured state, renders the view without the comment/debug-menu overlay, writes a PNG, then opens the comment field. Ordinary HUD/minimap remain useful context. Comments support normal typing, selection, clipboard paste, backspace and delete. Gameplay/menu input stays blocked until the dialog closes and its closing input is released.

Reports live under `%USERPROFILE%\AppData\LocalLow\DefaultCompany\Racer\DebugReports\<date_time_id>`. Each session contains `BUG_REPORT.md`, `bugs.json`, and `Screenshots/BUG-001.png`, etc. The exported ZIP sits beside the session folder, includes relative image references, and can be shared by itself. The debug menu displays the folder/export path. Sessions accumulate across course changes within a game launch.

Detached fly controls: **WASD** move, **Q/E** down/up, **hold right mouse** to look, **Shift** fast, **Ctrl** precise. Controller: left stick moves, right stick looks, triggers move down/up, right shoulder fast and left shoulder precise. Open F6/Start and choose Return to Vehicle. The world is paused while inspecting; the vehicle, progress and checkpoints do not move. F4 records camera XYZ/heading in fly mode and also preserves the vehicle position separately.

Entering fly during an active race marks that race as a **DEBUG RUN**. It disables PB/Top 10/race/ghost/playlist results until restart and revokes records already written by that attempt while preserving its previous records. Fly cannot earn activities, collectibles or map discoveries. HUD and captures alone do not invalidate records.

**Race timeout is suspended for the entire time Debug Mode is enabled**, including the overall time limit and post-finisher grace limit. Turning it off resumes the remaining timeout budget. Race/lap timing remains normal while driving; only report dialogs, the debug menu and fly inspection pause the world.
