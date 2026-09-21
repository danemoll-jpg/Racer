# CR-101–104 implementation

`Assets/Scripts/Editor/CR101Authoring.cs` contains the scene-authoring changes; all four scenes were saved through the live Unity Editor, including mesh assets and colliders. No scene YAML was hand-edited.

To apply to the preceding 0.17 scene state, invoke these methods through the Editor in this order: `CR103Geometry`, `CR103Landing`, `CR103Connection`, `CR101Property`, `CR101CloseFence`, `CR103Return`, `CR101CloseRemainingFence`, `CR102Signs`. Property and boundary changes use markers to avoid translating the already moved structures twice. The authoring adjusts batched visual vertices and retained child colliders together. Terrain edits are limited to the property, runway, connecting approach, landing and return corridors.

`CR101Release` exposes scoped generation/build jobs through `Queue` and `Temp/cr101-request.txt`; the release job builds the four saved scenes as Windows x64 Mono version 0.18.0-review1. `CR101Audit` compares local ground samples and moved destination/activity coordinates. Older authoring passes remain historical and should not be reapplied over the corrected scenes without reapplying this correction sequence.

The five summit boards use a dedicated front-only font shader/material. Fitting uses generated text bounds at its actual world scale, with 0.35 m horizontal and 0.25 m vertical margins on 7 x 2.3 m backings.

`StartupTitle` schedules the unchanged imported speech and waits for its complete sample duration plus 200 ms before starting the existing looping theme. Intentional input cancels the sequence. Radio activation remains in the existing menu transition.

`CR101Validation` and `CR104Validation` run only with their explicit unique command flag AND an isolated test-save root. They do not activate for normal play. `Tools/Run-CR101-Check.ps1` uses temporary muted preferences and hidden rendered Windows players. The speech fixture temporarily captures/unmutes only its own process and remutes afterward.

`Tools/Package-Racer.ps1` retains the existing complete-runtime, staged-music, preservation, extraction and per-file SHA256 workflow. Version 0.18 includes this pass's README/validation and the current vehicle asset notices. `Package-Racer.cmd` selects the new version.
