# UI Phases 3–5 — combined review

Safety checkpoint: clean main `69d8d2e44c77ef45e2e69d0b43f5d28b6acd7da8`.

Dan explicitly authorized the remaining approved phases together. The accepted Phase 1–2 shell, glyphs, effective-binding resolution, input barriers, semantic focus, caller stack, keyboard and modals remain the baseline.

## Implementation

- Full World Map: fixed reticle, discovered-only destination cycling, explicit point/landmark context, route toggle, waypoint set/move/clear, player centering, graphical controls and Actions/Help. Travel retains discovery/free-roam checks, the existing safe arrival implementation, reset consequences and explicit default-Cancel confirmation. Permanent geography and discovery identifiers are unchanged.
- Playlists: independently scrollable saved and entry panes, copied drafts, shared naming keyboard, finite entry editor, duplicate-preserving add/edit/remove/move, cancellable move preview, dirty Save/Discard/Cancel and Save-and-Start/Start-Without-Saving. Failed writes retain the draft and loaded snapshot. The serialized library format and active championship clone remain unchanged.
- Records: Lap/Race/Speed Traps/Jumps/Ghost tabs, aligned rows, current and saved configurations, track/direction/vehicle filters for race boards, actual local/legacy identity, date/configuration/airtime details and existing ghost controls. No category merging or new driver identities.
- Finish/results: independent lap/race snapshots capture prior leaders and PBs before writes, select the fastest completed lap, and resolve its final retained rank. First/strict record, podium, Top 10, tie, PB fallback and submillisecond comparisons use existing precision. Short teal flourish, guarded Complete Race, Standings/Lap Times/Penalties/Championship and caller-preserving child destinations use existing race and championship data.

## Targeted evidence

Final core fixture: **38/38 PASS** (`checks.txt`). It covers draft isolation/save/discard/cancel, move cancellation, duplicate entries, graphical controller map actions, discovered cycling, travel cancellation/blocked arrival, record navigation/category isolation, achievement hierarchy/ties/submillisecond comparisons, best-lap-not-last ranking, failed writes, championship snapshot replacement and Results callers.

One supported discovered-home travel passed and resumed with the arrival notice (`final-travel-and-initial-finish.txt`). A synthetic finish-only follow-up passed **4/4** (`final-checks.txt`): waiting Complete Race, one guarded transition to Results, harmless repeated activation, explicit estimated/DNF AI and the recorded lap list. This does not claim physically driven laps. The first final fixture hit the pre-existing Editor traffic initialization exception; the next lacked AI because its own Return-to-Menu teardown removed them. The corrected fixture creates normal AI slots with traffic off; no gameplay/estimation source was changed to satisfy it.

`checks-initial.txt`, `checks-second.txt` and `checks-third.txt` retain earlier failures. Initial inspection exposed hidden table-label lookup and zero-height list panes; both were corrected. Map verification exposed disposed action references across Editor sessions and empty error state blocking discovery; lifetime/initialization were corrected. Final checks supersede those failures. Graphical map controls and readable playlist/record/results views were inspected; representative 720p, 800p and 1080p captures are retained. A final Results-only layout correction removes duplicate summary text, and its captures are `results-final-*`. Final source also guards active-playlist track/lap editing in Results Setup and avoids guessing units for unknown historical activity sites.

**Targeted feature testing STOPPED.** No broad gameplay matrix.
Tests use isolated `Temp/UIRemainingSave` data and virtual Input System devices. They do not establish physical-controller, Steam Deck or subjective visual acceptance. Existing Editor-only AmbientVehicle initialization exceptions remain unrelated and unchanged. No broad driving, AI, vehicle or course matrix is authorized or performed.

Publication, exact source identity, complete Latest installation, signed catalog, production launcher and cleanup evidence are tracked separately in PUBLICATION.md. VERSION.txt identifies the built source. Physical-controller and visual acceptance remains pending.
