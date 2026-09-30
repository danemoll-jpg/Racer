# UI architecture audit

Date: 2026-09-30. **Design/audit only; no redesign implemented.** Source checkpoint: `014f1eb72bbff6efd11fd0a40dced94370cc989b`, clean `main`. Playable baseline remains 0.47.0-review1 / game-47000. Proposal: [UI_REDESIGN_PLAN.md](UI_REDESIGN_PLAN.md).

## Scope and evidence

This is an inspection of the actual saved implementation, not a physical-controller playtest or a claim of visual acceptance. Runtime-generated hierarchies, action callbacks, navigation, save models, and a representative serialized HUD canvas were inspected. No Unity build, gameplay run, audio test, save mutation, or release was needed. Layout risks below are code-derived, not measured screenshots.

The census contains **31 screens, subviews, dialogs, and presentation systems**: 28 in-game surfaces (including non-menu feedback and the developer location tool), one external music chooser, and two launcher surfaces. Lap/race and speed/jump views are counted separately because they have different data contracts; their historical categories are states, not extra screens. Driving HUD, small racing minimap, navigation arrows and timing display are protected dependencies, not redesign targets. Editor tools, validation fixtures, world signs and documentation atlases are not normal player menus.

Evidence index (paths relative to repository root; symbols are the stable lookup anchors):

| Key | Implementation inspected | What it establishes |
|---|---|---|
| M | `Assets/Scripts/RaceMenus.cs`: `Initialize`, `Show`, `Update`, `LateUpdate`, `FinishName`, `MusicDetails` | Runtime uGUI card, all 15 reusable buttons, subviews, fixed text heights, navigation, naming and waiting prompt |
| F | `Assets/Scripts/RaceFlow.cs`: `Stage`, `Update`, `SetStage`, `Back`, `CloseGarage`, `CloseExtras`, `LapCompleted` | State transitions, action ownership, return paths, pause/audio/vehicle coupling, PB/rank capture |
| W | `Assets/Scripts/ExplorationMap.cs`: `Update`, `BuildUI`, `SelectNext`, `RequestTravel`, `ConfirmTravel`, `Travel` | Map input, pointer-only controls, discovery and safe travel |
| WV | `Assets/Scripts/WorldMapVisual.cs`, `WorldMapCourseOverlay.cs`; W.`Repaint`/`Draw` | Permanent-world image vs optional race overlay; larger visual bounds vs legacy discovery grid |
| P | `Assets/Scripts/RacePlaylists.cs`, `PlaylistChampionship.cs` | Stable course indices, alphabetical presentation, finite entries, compatibility and championship rules |
| R | `Assets/Scripts/RecordBoards.cs`, `RacerSave.cs`; `RaceDirector.cs`: `Category`, `Standings`, `FinalizeUnfinishedAi` | Local records, category partitions, legacy PBs, measured/estimated classification |
| A | `Assets/Scripts/ArcadeActivities.cs`, `ActivityRecords.cs`, `ExplorationCollection.cs`, `CleanLapGhost.cs` | Automatic activities, smash attempts, historical records, collection reset and ghost eligibility |
| U | `Assets/Scripts/LocalRadio.cs`, `MusicCollection.cs`, `StartupTitle.cs`, `DeveloperLocationHud.cs` | Persistent music, native chooser, title dismissal and diagnostic controls |
| L | `Launcher/launcher.cpp`: `Draw`, `Key`, `Confirm`, `Proc`, `wWinMain` | Native updater window, XInput controls and fatal OS error dialog |
| C | `Assets/Scenes/StreetLoopGreybox.unity`: Race HUD canvas (around line 868358); `Assets/Scripts/Editor/RaceSetup.cs` | Existing uGUI/1280×720 scaling, reused by M; no framework replacement required |

## Current navigation

```text
Native launcher -> Title -> Ready (main menu AND race setup)
Ready -> Courses / Garage / Roster / Playlists / Boards
      -> Activities / Exploration -> full Map -> Travel confirmation
      -> Settings -> Local Music -> Music Collection -> Windows folder chooser
      -> Start Race -> Countdown -> Racing -> waiting for AI -> Results
      -> Free Roam -> Racing state with FreeRoam=true
Racing / Countdown -> Pause -> Settings / Activities / Exploration
Racing / Pause -- View or M --> Map
Results -> replay / Garage / Roster / Boards / Activities / Exploration / Settings
Playlist -> compatibility choice when needed -> event -> results -> next event
                                                    -> championship subview
```

Settings remembers `settingsReturn`; Activities/Exploration remember `extrasReturn`. Garage, Roster, Boards, Courses and Playlists use `CloseGarage`, which returns to Ready except when an active playlist has finished. Consequently ordinary Results → Records/Garage → Back loses the Results parent. Map is a separate overlay, not a `RaceFlow.Stage`; it resumes immediately only when opened from driving, except successful travel deliberately resumes free roam. Music subviews are booleans inside Settings: their visible Back buttons traverse subpages, while B exits Settings via `RaceFlow.Back`. Escape is bound to the Pause action, not Back: its dispatch closes some screens but lacks Courses, Boards and Playlists cases despite the common “B / Esc: back” footer. These are source-confirmed mismatches.

## Full screen census and preservation decisions

Risk: L = mainly presentation, M = navigation/state dependency, H = persistent data, cross-scene or input ownership dependency. Each row includes current purpose, entry/actions, problems, proposed parent/actions and a preservation classification. No functional feature is proposed for silent removal.

| ID / current surface (evidence) | Entry, purpose and existing actions | Current problems / redundant information | Proposed name, parent and navigation/actions; classification | Risk |
|---|---|---|---|---|
| 01 Title (U) | Launcher/game startup; fresh button advances; approved art, speech and theme | Generic text prompt; separate button polling/input-release gate | Title → Main Menu; KEEP art/audio and release gate; SIMPLIFY prompt with active-device glyph | M |
| 02 Ready / main race menu (M,F) | Title, quit race and most Back paths; start, mode, traffic, difficulty, garage, roster, track, laps, playlists, records, activities, exploration, settings, quit | Fifteen unrelated buttons squeezed to 27px; setup mixed with destinations; duplicated track heading/selector | Main Menu + Race Setup; MOVE setup fields to Race, shared destinations to root; KEEP every action, finite 1–5 laps and unlimited solo | H |
| 03 Select Track (M,P,F) | Ready; eight track buttons with difficulty; selection loads scene immediately | Tall stack; technical eligibility prose; no Escape dispatch; selection has loading side effects | Tracks under Race Setup; SIMPLIFY scroll list/detail panel, select/back, same eligibility and scene mapping | H |
| 04 Garage (M,F) | Ready/Results; profile buttons, rendered preview, per-profile color swatches, Done | “Select/Selected” repeated; swatches also in vertical focus chain; generic Forest/Street copy misdescribes other restricted courses; result return lost | Garage under Main, invoked contextually by Race/Results/Free Roam setup; KEEP preview/profiles/colors/restrictions; return to caller | M |
| 05 Opponent Vehicles (M,F) | Ready/Results; three slots, fixed/random/mixed choices, Mixed roster, Reroll, Done | Explains algorithms and restart behavior in a paragraph; slot intent and resolved vehicle blended | Opponents under Race Setup; SIMPLIFY three rows showing choice and resolved profile; KEEP mixed/random/reroll/fixed restart roster | M |
| 06 Race Pause (M,F) | Enter/Escape/Start during racing/countdown; resume, restart, settings, quit event/game, penalties, estimate unfinished AI, activities/exploration | Recovery instructions are shown while recovery component is disabled; different labels for same finalize action; long stack | Pause; KEEP Resume/Restart/Settings/Return to Menu, MOVE penalties/records/ghosts into clear child pages; RENAME finalize to Complete Race | H |
| 07 Free Roam Pause (M,F,A) | Pause in free roam; resume, cycle non-speed activity, start/retry, cancel, settings/music, menu, quit, records/exploration | Activity management mixed with navigation; selected jump can be manually begun despite automatic scoring explanation | Free Roam Pause → Activities; KEEP selection, targets/PB/location, begin/retry/cancel and reset effects; prominent Map | H |
| 08 Settings (M,R) | Ready/Pause/Results; master, ambience, race/UI, vehicle volumes; VSync; 30/60/120fps; music; AI estimate option | One-way cycling, volume wraps to zero; audio/display/gameplay mixed | Settings → Gameplay / Audio / Display / Controls; MOVE existing fields, SIMPLIFY steppers/toggles; Controls initially binding reference | M |
| 09 Local Music (M,U) | Settings; music volume, channel including Off, next/previous, open folder, rescan, collection setup | Folder path and driving instructions dominate; B skips visible parent; shared Settings focus | Music under Audio; KEEP playback and volume; MOVE folder/rescan setup to Library while keeping obvious link | M |
| 10 Music Collection (M,U) | Local Music; bundled/custom source, show song, choose/open folder, rescan/cancel scan | “Folder channels / nested albums included” is a ShowSong action, not recursion toggle; filesystem status crowded into same text block | Music Library under Music; KEEP source/folder/scan/cancel; RENAME informational channel row; setup/help disclosure | H |
| 11 Windows folder chooser (U) | Choose custom folder invokes `SHBrowseForFolderW` on STA thread; select/cancel local directory | Leaves game; no in-game gamepad navigation bridge; not controller-complete; Explorer launch likewise external | Choose Music Folder under Library; MOVE normal selection into controller-operable in-game folder browser; KEEP optional native chooser/Open Folder | H |
| 12 Exploration / Personal Best (M,A) | Ready/Pause/Results; ghost toggle/status, collection summary, map, restart collection | Ghost racing feature mixed with exploration; “actual recorded poses” and migration details prominent | Exploration under Main/Pause; KEEP collection progress/map; MOVE ghost preference/status to Records → Ghost; return to caller | M |
| 13 Restart acorn hunt confirmation (M,A) | Second press on renamed exploration button resets only acorns; Back button clears pending flag | Inline arming instead of modal; B path does not explicitly clear `confirmCollectionRestart`, risking stale armed state on reopen | Restart Acorn Hunt dialog under Exploration; KEEP exact reset scope; explicit Cancel/Restart, default Cancel | H |
| 14 Whole-area map (W,WV) | Exploration button or M/View from driving/pause; pan, zoom, center waypoint, destinations, travel, course toggle, recenter, clear waypoint, close | Three mouse-only buttons; hidden destination steps; mouse click on landmark requests travel while A always sets center waypoint; text-heavy side area | World Map under Exploration and Pause shortcut; KEEP all data/actions, SIMPLIFY one reticle and context panel | H |
| 15 Fast-travel confirmation (W) | X on selected destination or landmark click; Yes/No; validates free roam/discovery/arrival | Plain A/B text; eligibility failures in status block; special resume behavior | Travel to [landmark]? dialog from World Map; KEEP validation, cancellation, successful close/resume/arrival toast | H |
| 16 Lap Top 10 (M,R) | Ready/Results Boards; lap/race switch, previous/next saved category, current configuration, Back | Space-aligned text, internal-looking layout names, metadata/precision explanation; no driver field; Escape gap | Records → Lap; KEEP compatible and historical categories, ties/new markers; real table + filters/details | H |
| 17 Total Race Top 10 (M,R) | Same board, race tab and category actions | Same problems; lap count affects race partition; no driver identities stored | Records → Race; KEEP adjusted times/category partition; label local driver You, legacy identity unknown | H |
| 18 Speed Trap records (M,A) | Activity Records from Ready/Pause/Results; sites, current/historical category and category cycling | Dense text, raw layout fragment, historical/vehicle/direction context hard to browse | Records → Speed Traps; KEEP measurements/medals/date/site/vehicle/direction/history; readable columns | H |
| 19 Jump records (M,A) | Same activity view, Jump selector | Same problems; distance/airtime data not all prominent; “activities” implies broader coverage than screen provides | Records → Jumps; KEEP distance/medal/history; show stored airtime in detail where known | H |
| 20 Saved Race Playlists (M,P) | Ready; new, name, start, prior/next entry, add, course/direction, laps, move earlier/later, remove, save, next saved playlist, Back | One entry visible; fourteen buttons; source-order course cycling; no dirty-exit contract; no playlist deletion button exists | Playlists under Race; SIMPLIFY saved-list + visible ordered entries; KEEP all existing operations and duplicates | H |
| 21 Keyboard playlist naming (M) | Name playlist opens single-line InputField; type/paste/select text, Enter/save, Escape/cancel; 64 characters | Technical naming/Deck instructions; initially keyboard mode even when gamepad opened it | Rename dialog from playlist header; KEEP keyboard editing/64-character sanitation/save error handling | H |
| 22 Controller name picker (M) | Start or mode button switches from text field; previous/next position, previous/next letter, save/cancel, switch input mode | Four separate actions per character; poor speed and discoverability | Same Rename dialog with graphical on-screen keyboard; MERGE two naming views; KEEP controller-only naming and keyboard alternative | M |
| 23 Choose compatible vehicles (M,F,P) | Playlist start/next when player or roster ineligible; Motorcycle or ATV + replace incompatible opponents, quit playlist | Long consequence paragraph, no preview of affected slots | Vehicle Compatibility dialog in playlist launch; KEEP explicit choice/cancel and only-incompatible replacement | H |
| 24 Post-finish waiting (M,F,R) | Player finishes before AI; explicit Complete Race button, provisional notice; automatic finalization option | Binding display strings and keyboard/controller combined; button duplicates banner; no best-lap achievement summary | Finish Summary with visible Complete Race primary action; KEEP guard/CR-064/waiting; shared glyph prompt | H |
| 25 Final race results (M,F,R) | Final classification; replay, settings, menu, penalties, garage, roster, boards, activities/exploration | Text standings; many destinations; initial per-lap/best-lap text is overwritten by standings; rank/PB reduced to footer | Results → Standings / Lap Times / Penalties; KEEP replay/setup/records/settings access through named child actions | H |
| 26 Penalty breakdown (M,R) | Pause/Results button cycles four-event pages and overview | Not an independently navigable table; repeated press cycles away; technical event strings | Penalties child under Pause/Results; KEEP all events/missed count/flat charges, scroll and Back | M |
| 27 Playlist championship (M,P) | Event results button or automatic final championship; event pages, cumulative points, winner, restart, next race, quit | Same text card, dense tie rules; distinction from current-race results unclear | Championship tab under playlist Results; KEEP 10/6/4/2/DNF0, ties/joint winners, solo completion and per-event snapshots | H |
| 28 Feedback/transition overlays (M,F,U,A) | Countdown, GO, finish/PB/Top10, recovery, activity medals/results, collection, song/station, save/travel notices | Several systems share/overwrite banner priority; implementation phrases shown; prompts mixed-device | KEEP driving/countdown/radio/activity HUD behavior protected; redesign only menu notices and finish summary; document shared-canvas dependency | H |
| 29 Developer location tool (U) | F3 toggle / F4 copy XYZ; only visible outside menus | Keyboard-only diagnostic, not normal player function | KEEP unchanged and outside menu redesign; no controller shortcut stolen for developer function | L |
| 30 Native launcher/updater (L) | Play-Racer.cmd; autoplay, Play, check, game update, soundtrack update, rollback, cancel, exit | Plain mixed-device footer; XInput slot 0 D-pad/A/B only, no stick navigation | KEEP separate pre-game launcher and all operations; presentation parity follow-up only, no updater rewrite | H |
| 31 Launcher fatal error dialog (L) | `wWinMain` catch opens Windows MessageBox; acknowledge | OS dialog is not wired to launcher XInput; controller-only recovery not assured | KEEP error reporting; propose in-launcher dismissible error surface as separate launcher follow-up | H |

## Confirmed controller gaps and control conflicts

1. **Map buttons:** W creates every utility button with `Navigation.Mode.None`. Show/Hide Course, Center on Player and Clear Waypoint have no corresponding input branch. Mouse is required for those actions, including keyboard-only operation. This corroborates Dan's course-toggle report.
2. **Destination cycling:** D-pad left/right does exist, but `SelectNext` steps through all destination indices. An undiscovered item changes selection without centering, giving a hidden/nonactionable stop. The centered reticle exists, but cannot select a landmark with A: A sets a waypoint; X travels to a separate selected index. Panning can leave that selection stale.
3. **Prompt truth:** most screens show A/Space, B/Esc and Enter/Start together. No shared last-active-device service or graphical controller glyph renderer was found. Presence of `Gamepad.current` in CompleteRacePrompt means connected, not actively used. Actual resolved bindings are only consulted for that one prompt.
4. **Back semantics:** Escape is not a universal Back; B inside music skips its subpages; ordinary Results child screens can return to Ready. Focus is saved by Stage/button index, so Settings subviews and Results/championship share state. Selection restoration can target a different semantic action.
5. **Input conflicts:** map M and driving radio M both poll independently. Map opening pauses first or radio toggles first depending on update ordering; no explicit common ownership exists. Gamepad View and radio D-pad avoid that opening collision. Modal ownership/release guards already exist in map/title/name paths and must be preserved centrally, not removed.
6. **External setup:** the native music picker and Explorer do not provide an in-game controller-only path. Normal custom-folder selection requires a controller-capable replacement; copying files in Explorer remains an optional desktop utility, not a normal menu action.
7. **Naming:** controller fallback exists, so naming is not mouse-only, but the 64-position letter cycle is cumbersome and initially opens the keyboard field. Keyboard typing has explicit ownership/release protection.
8. **Launcher boundary:** basic gamepad navigation exists; do not call the whole launcher mouse-only. Its fatal OS message box and stick navigation need separate treatment if the controller-first promise is extended through installation/recovery.

## Structural and terminology findings

The 700×680 card is a single mutable layout with 15 reused buttons, fixed text heights and truncated vertical text. Dense Ready/playlist rows are 27px, activity rows 25px; results details have a 180px area even as standings/metadata grow. Source structure predicts readability/clipping risk, particularly with long names and many laps; visual severity needs later layout review. No list virtualization or explicit scrolling is present in these menus. Use responsive lists and actual table columns instead of adding more fixed-height buttons.

| Current wording / behavior | Proposed treatment |
|---|---|
| Back / Done-ready / Back to menu / Return / Quit Race | Back for one navigation level; Resume for play; Return to Menu for abandoning/leaving an event; Quit Game only for process exit |
| Dim terrain: unexplored. Visited is NOT fully searched. | “Unexplored areas are dimmed.” “Explored areas may still contain acorns.” Put explanation in Map Help; progress stays visible |
| Skip waiting / estimate remaining AI vs Complete Race | Complete Race, with a short explanation that unfinished AI receive estimated results |
| Internal course version/category as primary title | Standard display title; rules/layout/roster metadata in Record Details; retain stable category keys |
| Clean-lap ghosts: actual recorded poses, local only | “Race your best clean lap.” Keep exclusions in Ghost Help |
| Current configuration / full-precision / tie order paragraphs | Filter summary + Details/Help; preserve behavior, not permanent instructions |
| Forest-only eligibility wording for all restricted tracks | Show eligibility from selected course; all non-Street tracks currently restrict player/AI to motorcycle/ATV |
| “Folder channels / nested albums included” clickable row | Clearly labeled information/now-playing action; scanner currently forces recursive scanning, not a working user recursion switch |

Duplicated navigation is useful when it preserves context: Records/Settings from Pause and Results should remain shortcuts to one shared screen. Duplicate *implementations* and multiple phrases for the same action should merge. Map progress may summarize Exploration; it must not become a second collection-reset owner. Music volume belongs to Audio, playback to Music, filesystem setup to Library.

## Data and technical constraints

- Preserve `RacePlaylists.Scenes` numeric indices; sort only `DisplayOrder`. Eight names already follow “base - Forward/Reverse,” alphabetical base then Forward first. Difficulty entries are currently unassigned/TBD; do not invent ratings or mistake them for AI Easy/Normal/Hard.
- Playlist definitions persist; the active playlist is a clone and championship progress is in memory. Preserve duplicates, 1–5 laps, restart replacing an event snapshot, and no unlimited playlist entries. No delete-playlist or resume-championship feature exists to migrate.
- Boards store local player attempts, not online/AI driver identities. `RecordBoards.Entry` has id/category/vehicle/date/race/legacy/seconds/order, no driver name. Never invent named rivals for these tables. AI standings and championship identities are a different data source.
- Lap boards remove only the trailing lap-count part of category; race boards retain it. Course rules/layout, direction through course identity, vehicle, opponents/roster/difficulty and traffic remain partitions. Preserve full-precision sorting, stable tie attempt order, received IDs, legacy migration and read-failure protection.
- `LapRank` is reassigned on each lap and `NewLapRecord` accumulates across the race. They are insufficient for reliable **best-lap** rank/improvement celebration. `LapCompleted` writes new PBs/boards immediately; capture old PB and old #1 before writes, then resolve the best lap's surviving entry rank at finish. Do not change timing/scoring.
- Results currently construct lap detail text then overwrite it. The data still exists in `RaceProgress.LapTimes`; expose it in a dedicated view instead of assuming the current screen displays it.
- Activity board UI filters only Speed/Jump, even though smash challenge personal bests exist in `ArcadeActivities.Results` and the pause selector. Preserve smash attempts/PBs in Activities; do not fabricate a new historical smash Top 10 from the wrong store.
- Map visual bounds and `woodstock-world-v2` 110×80 discovery grid differ intentionally. Preserve normalized conversions, landmark IDs/property migration, hidden undiscovered markers, acorn saves and visited-cell semantics. Course overlay visibility must not hide permanent physical roads/trails/shortcuts.
- Safe travel requires discovery + free roam + supported unblocked arrival. It resets active activity/ghost sampling and route branch state. Cancellation leaves position/attempt unchanged; success closes map and resumes with arrival notice. Preserve these consequences explicitly.
- Menus share the RaceHud canvas; `SetStage` controls time scale, audio pause, input/recovery enablement, cursor and HUD visibility. Extract menu presentation without rewriting those lifecycle semantics. Persistent LocalRadio survives scene changes; opening Tracks/Garage must not skip songs or rescan collections.
- Existing controls are created in code across several owners; there is no demonstrated user rebind UI. Centralize bindings and derive prompts now; proposing a complete remapping feature would be additional scope.
- The launcher is native C++/GDI+/XInput, not uGUI. It can share glyph artwork/control vocabulary, not the Unity runtime service. Signing, updater, music ownership, startup handshake and release behavior stay protected.

## Audit disposition

All runtime features above are KEEP, MOVE, MERGE, RENAME or SIMPLIFY. **No runtime feature removal is proposed.** Remove only redundant instruction paragraphs, duplicate labels and nonfunctional navigation clutter after equivalent contextual prompts/help are present. No gameplay bug is claimed fixed. The plan prioritizes shared input/navigation, core screens and music setup, map, lists/records, then finish feedback; Dan reviews before implementation.
