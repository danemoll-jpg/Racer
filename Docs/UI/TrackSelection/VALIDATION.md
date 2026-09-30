# Race Setup track-confirmation regression — 0.52.0-review1

Safety checkpoint: clean main `81a7804e78bf51e5d7fd920586487da4a2f76235`.

## Root cause and bounded repair

Commit `d5fe1e3d` replaced the Race Setup track-row callback (`SelectCourseEntry`) with navigation to a preview page. The normal Select action therefore only previewed a track; committing required a second, separate Use This Track action. The scene-loading callback itself remained intact. Earlier browsing checks verified preview/cancellation but never submitted a track to completion.

Restore track-row Submit/click to `RaceFlow.SelectCourseEntry`. Keep the existing map preview accessible through a separately labeled Preview action for the last highlighted track. Focus updates only this preview identity. The existing explicit Use This Track action inside the preview continues to commit. B from either list or preview never commits.

Only `RaceMenus.Core.cs` and `RaceMenus.Shell.cs` change runtime behavior. The existing scene transition, Race Setup caller restoration, settings and course-specific vehicle eligibility remain the owners of selection. Race Setup continues using the loaded course across menus and race starts; no new across-application-restart save semantics or save schema are introduced. Stable scene/course IDs and serialized playlist indices are unchanged.

**Standing UI distinction:** Race Setup track selection commits the selected race course on Confirm. Maps / Records / Top 10 track browsing is read-only and NEVER mutates Race Setup. Merely focusing a track or opening its preview never commits or loads a course.

## Targeted verification

`Tools/Check-TrackSelection.cs` uses isolated muted saves, the production Input System/EventSystem and scene-loading callback. It checks all eight course/direction selections, setup summaries and vehicle eligibility; starts Backyard Forward from Street Forward and Forest Reverse from Backyard Forward; checks controller B, optional preview/Back, and different-track Records/Maps browsing without settings, category or scene mutation.

The first run passed Backyard Forward focus, controller confirmation, setup, eligibility and race start (5 assertions). Its fixture then tried QuitRace directly from countdown; production correctly requires Pause first. `initial-fixture-error.txt` and `initial-checks.txt` preserve that harness failure. The fixture now pauses first and resumes only the remaining cases. An initial compile-only scene-handle conversion warning was corrected before execution.

Final results and screenshots are recorded alongside this file. Simulated controller input verifies the production A/B paths; this is not physical-controller or Steam Deck acceptance. No laps, AI tuning, physics/geometry changes, records filtering changes, SCS work or broader gameplay matrix are included. Stop feature testing after these targeted checks pass.

Final result: **40/40 assertions PASS** (initial 5 + remaining 35). All eight course/directions confirmed; the two requested starts passed. B, optional preview/Back, Records and Maps isolation passed. Preview and final Setup screenshots inspected. Original Editor scene/build settings and temporary input settings restored. Feature testing STOPPED.
