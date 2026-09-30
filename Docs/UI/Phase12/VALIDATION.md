# Phases 1 + 2 — combined UI review

Status: implementation, focused validation and delivery complete in **0.48.0-review1 / game-48000**. See [publication evidence](PUBLICATION.md). Physical-controller and visual acceptance remain with Dan.

Scope follows the approved UI_REDESIGN_PLAN.md with Dan's explicit delivery override: one review build combines foundation and core menus/music. Phases 3–5 remain deferred. Existing map, playlists, records, results and championship routes remain accessible.

## Evidence

- Initial checks: `checks.txt`, 24/26 pass. The two scene-transition failures used a fixture that destroyed the title without setting its completed flag and changed the radio save root between scenes. The corrected fixture uses a consistent isolated save root and skips the title deliberately; no title/audio redesign is involved.
- Screenshot review caught absent vector graphics (missing CanvasRenderer). Corrected and inspected colored A/B/X/Y circles, trigger/stick graphics, keyboard utilities and scrolling controls. Main, Garage, Race Setup, Tracks, folder, keyboard and Controls PNGs are retained. Camera-based UI captures can omit a legacy-font label during canvas/font rebuild; live label state was separately inspected. Physical display/Deck and final Phase 5 layout acceptance remain with Dan.
- Compilation succeeded before the first run. Existing Editor-only AmbientVehicle constructor/initialization exceptions appeared with the project's scene-reload configuration; no UI exception was observed. The unrelated traffic system remains unchanged.
- `followup.txt`: **15/15 pass** after correcting fixture focus routing and fixing a real ownership issue: Unity's default UI action asset discarded custom Submit references on disable. The module now owns a clone; simulated A and an effective X override dispatch through the EventSystem, with no parent-action leakage. Back, Escape, modal ownership, Settings→Pause, device switching/draft retention, folder unavailable/commit and track/music return pass.
- `keyboard-space.txt`: **PASS**. A keyboard Space text event inserts exactly one space and cannot also submit the focused on-screen key.
- `preservation.json`: protected scene data, gameplay and persistence-schema sources unchanged from safety checkpoint.
- Final compilation and fresh Windows build succeeded (zero errors, 12 warnings). Public signed download/startup and the unchanged production Play-Racer.cmd passed for managed 48000. **Targeted testing stopped.**
- Input events above use virtual Input System devices; they are not physical-controller or Deck acceptance. Initial failed checks and diagnostics are retained as evidence, superseded by the final follow-up results.

## Deliberate limits

No driving/AI/course matrix, scoring changes, HUD/minimap work, map interaction redesign, leaderboard redesign, playlist-editor redesign, post-finish celebrations or launcher UI redesign. Physical controller/Deck input and Dan's visual acceptance are not claimed by automated fixtures.

## Implementation mapping

| Approved surfaces | Adapter |
|---|---|
| Shared shell, action registry, semantic focus/scroll, caller Back | RaceMenus.Shell, MenuActionRegistry, RaceFlow navigation adapters |
| Active family, release ownership, effective binding, graphical glyphs | MenuInput, MenuGlyph; map/radio ownership adapters |
| Main, Race Setup, Tracks, Garage, Opponents, Pause/Activities | RaceMenus.Core and existing vehicle preview/service callbacks |
| Settings, Controls, Music, Library | RaceMenus.Core and existing settings/radio services |
| Controller folders and reusable keyboard | RaceMenus.Entry; playlist name callback only |
| Exploration entry/reset, compatibility, penalty details | Shared shell/modal; existing eligibility, collection and penalty services |
| Title | Prompt presentation only; existing art/audio/timing/release gate retained |
| Deferred map/records/playlists/results/championship | Existing implementations, with shared caller/input and necessary entry adapters |
