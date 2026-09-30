# Track browsing and Race Setup route preview

Safety checkpoint: clean main `43c8ef0d9a6e0ad85da1929304fdc66474ee1e04`.

## Requested behavior

- Records: explicit Track selector offers all eight current course/direction choices, even without times. Browsing keeps race settings/scene unchanged. Existing saved configurations remain distinct, with counts and visible vehicle/mode/traffic/lap labels; historical times are not combined with newer layouts.
- Full World Map: Track footer action (T / D-pad right) and Actions/Help choice select and frame a route; Cancel retains the prior selection. Main routes, shortcuts and gates come from read-only snapshots of the actual saved scenes. Travel/discovery/waypoints and the racing minimap remain unchanged.
- Paused-race Records and Map stay on the current track. Free Roam and between-race browsing are available.
- Race Setup: selecting a track opens its map preview; **Use This Track** performs the existing scene selection. Back returns to the track list.

## Record preservation

Read-only inspection found 219 stored board entries: 161 lap, 58 race, across 85 configurations, plus 61 legacy best-time files. The count and SHA-256 are in `player-records-readonly.json`. No player saves were edited, deleted, restored or reset. The prior screen's exact-configuration filtering could display an empty board while other configurations contained times.

## Focused checks

Final `checks.txt` / `done.txt`: **20/20 PASS**. Covers all track options, direct board selection and saved entry display, Lap/Race selection retention, Reverse separation, Current Race restoration, route geometry and visibility, cancellation, unchanged scene/race/settings, Race Setup preview, race locks and Free Roam availability.

Tests used muted isolated `Temp/TrackBrowsingSave*` files. The first fixture captured settings before the asynchronous radio initialization settled, causing two preservation assertions to fail; baseline capture was corrected. The second fixture invoked a course button in the same step as opening the menu, so the normal release barrier rejected it; waiting for that barrier produced the passing final result. Those initial outputs are preserved. No game implementation was changed to bypass input barriers. No physical-controller or broad driving test is claimed.

720p screenshots inspected: Records configuration/track controls, separate map Forward/Reverse routes and Race Setup preview. Final preview shows the complete main route and legal branches, and both selection/Back controls. Protected scene/assets and gameplay changes are excluded from the Git diff.

`Tools/Capture-CoursePreviews.cs` generated all eight routes from saved scene components, opening temporary scenes additively and closing without saving. `capture.txt` records source IDs and path/gate counts. Future route edits must refresh this generated presentation data to keep previews current.

Feature verification is complete. Delivery evidence will be recorded in `PUBLICATION.md`; the built `VERSION.txt` identifies the completion source.
