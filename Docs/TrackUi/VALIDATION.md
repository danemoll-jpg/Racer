# Approved map and focused UI — 0.41.0-review1

Safety checkpoint: clean main `53dc95a622abf2103f1f45f7dc1abd395524f7e0`.

Dan approved Dan's Backyard Loop - Forward, including the corrected dense dump. No course construction was authorized or performed. Reverse is not implemented; optional shortcuts await Dan's visual map review.

- [Visual map](../BackyardForward/TRACK_MAP.png): 5040 × 3750, actual saved approved scene rendered orthographically, actual exported 1,684 route points and nine gates. Full course, surrounding woods/terrain, existing roads/trails/property landmarks, three major jumps, arrows and readable 50 m Unity X/Z grid. PNG opened and visually inspected. One zoomable overview is sufficient.
- [Preservation](preservation.json): all scenes/track geometry unchanged; all 1,684 route points and nine gates match earlier approved route data. AI, estimation, progression, timing, records, saves, road and minimap source unchanged.
- [Track selector](track-selector.png): seven consistent 54-unit rows; alphabetical base names, Forward before Reverse, secondary `Difficulty: TBD`. Metadata supports Unassigned/Easy/Medium/Hard; no final rating assigned. Playlist indices and scene identities retain their original order. Display sorting does not sort serialized identities.
- All seven actual menu buttons launched their intended scenes. Runtime record identities recorded in [course-identities.txt](course-identities.txt) match saved scenes. Existing save/board classes and record category construction are unchanged. Testing used isolated saves; no real records edited.
- [Waiting screen](waiting.png): button absent before finish; available with visible cursor after legitimate progression completion while AI remain; uses existing `FinalizeUnfinishedAi` directly; opens normal [results](results.png). Measured human and already-finished AI unchanged; only unfinished AI estimated/labeled; repeated activation harmless; no estimated board entries. Button absent after finalization and when all finish normally. Normal all-measured completion passes.
- Fixture seeds valid ordered gate progression to isolate UI/classification; it is not a human driving test or a full-race AI test. Mouse event handler invoked through the actual Button; physical controller interaction remains Dan's review.

Initial run: **25/27 assertions passed**, including every finish-button assertion. Two mountain loads failed because the Editor scene list omitted them; the existing release build already includes both. Temporarily aligned the Editor list with release scenes and reran only those unresolved menu cases: **8/8 pass**, including record identities. Restored the original Editor list afterward. The initial selector screenshot was obscured by the startup title; recaptured after dismissal. Original failures retained in initial-checks.txt; no gameplay retuning or broad regression matrix.

Targeted testing STOPPED. Dan reviews visual map/UI. General menu cleanup is BACKLOG ONLY.

Release/publication/launcher/cleanup evidence is recorded in PUBLICATION.md after delivery.
