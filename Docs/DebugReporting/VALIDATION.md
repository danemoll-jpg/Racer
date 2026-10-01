# Debug reporting and Mountain cleanup / 0.63.0-review1

Phase A was completed before Phase B implementation. The ten individually documented local repairs and their limitations are in `../MountainCleanup/VALIDATION.md`; ten coordinate views and physical-probe evidence accompany it. Original route/navigation coordinates, lower tunnel and corrected Summit rejoin are preserved.

Phase B targeted Editor checks pass (`checks.txt`, 28 checks): defaults off, F3/F4, live metadata, screenshot before comment, natural keyboard typing, Enter save, controller B cancel, input isolation, three accumulated reports, Markdown/JSON, relative PNG links, ZIP contents, folder action, keyboard/controller detached fly, camera-coordinate capture, return without progress changes, PB/Top 10 rollback, persistent ghost rejection, and restart eligibility. Debug-on timeout suppression and normal debug-off timeout both pass. Initial failures in text entry and the test's release-barrier timing are retained in `initial-checks.txt`; corrected checks pass.

Three temporary reports were generated in isolated validation storage; a final sample session is retained as evidence. Screenshots and overlay/menu renders were visually inspected. Batch Editor capture explicitly renders the same camera and live HUD canvases because batch mode has no end-of-frame callback; ordinary runtime capture uses ScreenCapture after end of frame. No broad course/vehicle matrix was run for debug features.

Player saves/settings were not used for these checks. The temporary test components are removed from the saved scene; their source is Editor-only. Production physics, global AI, recovery and unrelated tracks are unchanged. The source build and signed-publication results are recorded separately after delivery.

See `DEBUG_MODE.md` for controls, report paths and eligibility rules. These are technical acceptance checks; Dan's gameplay/visual review remains authoritative.
