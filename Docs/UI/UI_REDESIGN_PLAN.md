# Controller-first UI redesign plan

Date: 2026-09-30. **Direction approved by Dan on 2026-09-30. Current implementation request combines Phases 1 + 2 into one review build; Phases 3–5 remain deferred.** Based on the [source audit and 31-surface census](UI_AUDIT.md) at `014f1eb72bbff6efd11fd0a40dced94370cc989b`. Preserve dark translucent surfaces, teal/cyan accents and the simple existing visual identity.

## Architecture and navigation

Use the existing uGUI system. Separate reusable menu presentation from `RaceFlow` game lifecycle and feature services. A small navigation stack records screen, caller, selected item identity, tab/filter and scroll position. Reuse one screen implementation wherever invoked. Do not introduce a new game state machine for driving or replace the HUD.

```text
Title
└─ Main Menu
   ├─ Race
   │  ├─ Race Setup → Tracks / Garage / Opponents → Start Race
   │  └─ Playlists → Saved lists + Entries → Entry editor / Rename
   │                                      → Start → Compatibility if needed
   ├─ Free Roam → launch summary (current world/course + vehicle) → Explore
   │             └─ Tracks / Garage shortcuts, returning to this summary
   ├─ Garage
   ├─ Records → Lap / Race / Speed Traps / Jumps / Ghost
   ├─ Exploration → World Map / Acorn progress / Restart Acorn Hunt
   ├─ Settings → Gameplay / Audio / Display / Controls
   │                    Audio → Music → Music Library → Choose Folder
   └─ Quit Game

Race Pause: Resume / Restart Race / Settings / Return to Menu
            secondary: Race Details (penalties), Records, Exploration, Quit Game
Free Roam Pause: Resume / Map / Activities / Settings / Return to Menu
                secondary: Records, Exploration, Quit Game
Finish Summary → Results: Standings / Lap Times / Penalties
                          + Championship for playlists
                 actions: Race Again or Next Race, Race Setup, Records,
                          Settings, Return to Menu
```

Race opens Race Setup directly with a clearly visible Playlists destination; avoid an extra empty choice screen. Free Roam uses the current course scene's world, not a newly invented unified world scene. Track changes retain the existing scene-loading semantics and return context. Garage reached from Main returns to Main; Garage from Race/Results returns there. World Map opened during driving resumes on Back; opened from Pause or Exploration returns there. Successful travel keeps the existing close-and-resume behavior.

B always removes the innermost overlay/page first. B at Main does nothing destructive. Menu pauses/resumes driving; within child menus it never silently abandons the session. Return to Menu ends a running event/playlist through the existing teardown, with explicit confirmation when progress will be lost. Completed standalone Results may return without a destructive warning. Existing Next Race, restart-current-entry and championship summary remain distinct actions. Save each parent's focus/scroll by semantic identity, not the old shared button index.

## Shared presentation system

Proposed small components (names describe responsibilities, not new files created in this pass):

| Component | Responsibility |
|---|---|
| Menu shell | Header, optional tabs, content, persistent contextual action footer, modal layer |
| Navigation stack | Caller, semantic focus, scroll, tab state; routes one Back action |
| UI action registry | Semantic actions, actual bindings, enabled state, label and callback; buttons and prompts invoke the same action |
| Input context owner | Driving / Menu / Map / Modal / Text Entry; prevents one press reaching two owners |
| Active input service | One process-level device-family decision; notifications to every visible menu prompt |
| Glyph provider | Resolved binding/control path + device layout → graphic; keyboard/mouse keycap alternative |
| Focus list/table | Explicit neighboring controls, scroll selected row into view, item identity restoration |
| Modal / notification | Named consequences, default safe choice, concise error and retry feedback |

Keep one EventSystem; prevent old `RaceFlow.Update`, EventSystem submit and raw map/name polling from handling the same action during migration. Preserve title/map/name release gates and consume the closing input through release so it cannot start a race, place a waypoint or reactivate a dialog. Menus operate using unscaled time while gameplay is paused. Device disconnect restores visible focus and keyboard/mouse prompts without confirming anything. Scene changes reattach presentation to the new flow while the active-device service and radio remain process-level owners.

### Glyphs and active-device switching

Render real graphical assets: A in a green circle, B in a red circle, X in a blue circle, Y in a yellow circle, with readable letters/outlines so color is not the only cue. Use recognizable bumper shapes for LB/RB, trigger silhouettes for LT/RT, four-way D-pad, separate left/right stick silhouettes, L3/R3 click badges if used, Menu's three-line button and View's overlapping-window button. Use Xbox's View name for this device family; device-specific Options labels may differ. Do not render `[A]`, “Press B” or “LB / RB” as the controller UI.

Prompts contain graphic + short action label, e.g. the green south-button glyph beside **Select**. Resolve the effective binding on the active action/control scheme; never choose an A picture merely because an action is named Confirm. Retain actual Space confirm and existing driving bindings unless a documented collision requires changing routing. Keyboard uses keycaps; mouse uses button/wheel shapes. Composite bindings can show the D-pad or stick as a unit. Unsupported controller layouts use a neutral shaped control icon and resolved short label; never falsely label a non-Xbox control A. Missing required glyphs fail the implementation acceptance check.

Switch immediately on a meaningful button/key/click/wheel action. Analog activation must cross a dead zone; ignore idle stick drift and synthetic cursor events. Require intentional mouse movement (initial tuning target: 8 screen pixels accumulated in 100ms) before switching from controller; retain the last deliberate family while idle. A brief analog-only debounce prevents flicker without delaying actual button presses. Merely connecting a controller does not change prompts. Input-field typing changes prompts but does not dismiss the field. Test controller → keyboard/mouse → controller with selection and pending edits preserved.

### Shared control vocabulary

These names describe proposed bindings; the shipped UI must show graphics, not this documentation table as text instructions.

| Intent | Xbox default | Keyboard/mouse default | Contract |
|---|---|---|---|
| Navigate | D-pad / Left Stick | Arrows; point/click | Visible focus, predictable neighboring row; no hidden action on focus |
| Select / Confirm | A | Space; left click | Fresh press once; never destructive by merely landing on an item |
| Back / Cancel | B | Escape | One page/modal; cancel edits where explicitly transactional |
| Tabs / major category | LB / RB | Q / E outside text entry; clickable tabs | Only on screens with visible tabs; no second conflicting meaning |
| Adjust selected field | D-pad left/right | Left/right; stepper/slider | Clamped values, no accidental volume 100% → 0 wrap |
| Secondary action | X | Visible labeled keycap/button | Screen-specific label; no hidden overloaded action |
| Context action | Y | Visible labeled keycap/button | Context-specific label, never inherited driving Recovery in a menu |
| Pause / Resume | Menu | Enter or Escape during driving | Escape becomes Back within menus; Enter saves only inside text editor |
| Open World Map | View | M | Owns the opening press; resolve radio-M collision explicitly |

Radio driving keys remain otherwise unchanged. Proposed collision resolution: M belongs to map; radio channel/off remains D-pad Down and the visible Music screen, with keyboard radio toggle moved to a nonconflicting key (proposed N, verify all source bindings before implementation). This narrow input dependency is explicit and does **not** authorize racing HUD redesign. Controls page must show the final actual bindings. Existing R/Y driving recovery and camera/vehicle controls stay untouched.

### Visual hierarchy and layout

Retain the palette: near-black blue/teal shade, dark translucent panels, teal/cyan selection and white primary text. Use a border/marker plus fill for focus, distinct from a persistent selected tick. Disabled actions remain readable with a reason. Keep headers concise and move implementation details into Help/Details. Use one action footer and at most four context actions; less frequent actions go in a visible Actions sheet, never an undisclosed chord.

Design at the existing 1280×720 reference with a 32px safe margin; initial target sizes: 30–34px title, 22px body, 18px secondary, 44px rows, 32px glyphs. These are starting layout tokens for review, not measured accessibility claims. Verify 1280×720, 1280×800 and 1920×1080 layouts later, including long names and empty/error states. Avoid shrinking type to fit fourteen actions. Tables use fixed aligned columns and scrolling; selected rows stay in view. Use a correct ScrollRect → masked Viewport → Content hierarchy, with layout/fitter ownership that does not fight its parent. No HUD font conversion or global CanvasScaler change.

## Core screens (audit 01–10, 12–13, 23, 26)

- **Race Setup:** track/direction, player vehicle, laps, mode, difficulty and traffic form a compact configuration summary. Start Race is prominent; Garage, Opponents, Tracks and Playlists are visible destinations. Unlimited remains solo-only; toggling AI restores the last finite lap count. No silent incompatible roster changes.
- **Tracks:** eight scrollable rows grouped alphabetically by base: Dan's Backyard Loop, Forest Loop, Mountain Loop, Street Loop; Forward precedes Reverse. Preserve exact `RacePlaylists.Titles` strings, numeric scene indices and TBD difficulty. Focus previews metadata only; A commits selection/scene load. Use a simple list plus detail pane, not eight oversized cards. No new map imagery is needed.
- **Garage:** large existing vehicle preview; profile list beside it; named color swatches below with visible selection ring. Explicit horizontal swatch navigation and predictable return to profiles. Show the selected course's actual restrictions and reason for unavailable profiles. Keep saved color per profile; do not add speculative vehicle stats or unlocks.
- **Opponents:** three rows with Slot, Choice (specific/Random/Mixed), Resolved Vehicle, and ready/eligible state. Show Mixed roster and Reroll as clear actions; retain the resolved roster across rematches. Small help explains that Random may repeat and Mixed uses eligible profiles before repeating.
- **Race Pause:** Resume first, Restart Race second, Settings and Return to Menu prominent. Penalties and exploration/records are reachable in secondary pages. When finished but awaiting AI, Complete Race is available here too with the same action label/guard. Never suggest Y recovery works while paused unless a future explicit action actually performs it.
- **Free Roam Pause/Activities:** Resume and Map first. Activities lists the current selectable jump/smash sites, targets, PB and direction/distance, with Start/Retry and Cancel. Preserve current automatic trap/jump scoring; do not accidentally require activation for those events when exposing the existing manual attempt action.
- **Settings:** Gameplay holds Estimate AI at Your Finish. Audio holds Master, Vehicle, Ambience, Race/UI, Music and a Music link. Display holds VSync and 30/60/120 frame cap with a short explanation when VSync makes the cap irrelevant. Controls shows actual driving/menu/map bindings. No invented resolution, units, accessibility or full remap settings in this pass. Existing settings still apply/save immediately; Back is not an undo promise.
- **Exploration:** acorn total and region progress, World Map, and a secondary Restart Acorn Hunt action. Ghost racing moves to Records → Ghost, with clean-lap eligibility/help and the existing on/off/status only. Do not invent ghost deletion/export controls. Restart uses an explicit default-Cancel modal naming the acorn-only scope; map/records/ghosts/settings survive.
- **Compatibility:** show upcoming playlist event and affected player/AI slots. Offer Motorcycle or ATV; confirm replaces only incompatible opponents with the chosen profile. Cancel retains the existing quit-playlist/menu outcome and makes that consequence explicit.
- **Penalty Details:** readable event rows and totals, retaining all source ledger entries and flat five-second charges. Return to the invoking Pause/Results tab; no scoring edits.
- **Title:** retain artwork, voice/theme assets and timing, fresh-input/release gate. Only adapt the advance prompt to active-device presentation. Audio investigation is outside this redesign.

## Music and controller-only setup (audit 09–11)

Music shows channel, artist/song, Previous/Next, Off/channel choice and volume. Library shows source (Bundled/Custom), friendly folder name, Rescan, Cancel Scan, Choose Folder and optional Open Folder on Computer. Status/errors and scan progress are visible near those controls; full path, format/scan limits, recursive-folder explanation and RADIO.md guidance belong in Details/Help. Keep the current MP3/WAV/Ogg support, nested folder channels, history, skip/error behavior and managed/personal music ownership.

Choose Folder needs a small in-game directory picker, not an instruction to use a mouse or Steam overlay. Show available local roots, breadcrumb/current path, scrollable directories, Parent Folder, Use This Folder and Cancel. D-pad/stick navigate, A opens a folder or activates the focused Use action, B goes back/cancels at root, LB/RB change visible root tabs if present. A shared on-screen keyboard handles optional path entry. Keep enumeration bounded/asynchronous, expose access denied/missing-folder messages and do not follow arbitrary directory links indefinitely. Only commit a valid absolute local path after Use This Folder; cancelling keeps current source and song. Preserve current rejection of network/URL sources. Native chooser and Explorer remain optional desktop shortcuts; adding/managing actual files is an OS operation, not a prerequisite for controller selection of an existing music folder.

Source switches/rescans may intentionally restart playback under existing semantics. Ordinary navigation, course switching and reopening screens must not. Implement library setup in phase 2 so the normal-menu controller promise is not deferred indefinitely. Physical Windows/Deck path browsing will require later platform acceptance; do not claim the native chooser already solves this.

## World Map interaction (audit 14–15)

Use a fixed center reticle over a large pannable map (roughly three quarters of content width), with a narrow selected-location panel, compact exploration progress and contextual footer. Preserve the permanent-world render and optional course overlay. All permanent roads, trails and shortcuts remain in the base image even when the course overlay is hidden. Main route stays teal and optional shortcuts gold; color represents route role, not geometry ownership. Small racing minimap remains unchanged.

| Action | Xbox | Keyboard/mouse | Exact behavior |
|---|---|---|---|
| Pan reticle across world | Left Stick | WASD / drag | Map moves under fixed reticle, zoom-adjusted speed and bounded center |
| Zoom | LT out / RT in | Wheel; minus/plus keys | Zoom around reticle; retain supported 1–6 range initially |
| Previous/next discovered destination | LB / RB | Q / E; visible list arrows | Skip undiscovered IDs; center and select visible destination; no hidden stops |
| Select point/landmark | A | Space / left click | Selects reticle target; landmark snap within visible screen-space radius; empty point gets coordinate-free “Map point” card |
| Show/Hide Course | X | C / clickable toggle | Toggle current overlay only; label shows current state |
| Set/Clear Waypoint | Y | F / clickable action | Selected point differs from waypoint: Set/Move; same target: Clear; Actions sheet also has explicit Clear Waypoint |
| Center on Player | Right Stick click | Home / button | Recenter and select player; clear stale landmark focus, retain waypoint |
| Back | B | Escape / Back button | Close location sheet first, then map to caller; View/M also close map |
| Travel | A on explicit Travel in location sheet | Space / click Travel | Opens named confirmation; no travel from mere focus or first map selection |
| Actions / Help | Menu | Tab / visible Actions button | Opens focusable utility sheet, including explicit Clear Waypoint, Center on Player and Controls; B returns to map |

Footer shows primary A/X/Y/B actions plus a compact Menu-glyph Actions affordance; zoom and destination glyphs sit beside their map controls. Every action, including stick-click recenter and Clear Waypoint, also has a focusable visible control in the Actions sheet; no hidden mandatory stick clicks. Right Stick movement is unused initially. D-pad navigates a location/actions sheet when open; analog panning is suspended there. LB/RB are destination categories here, not another nested tab assignment. A opens a location sheet after selection. While any modal is open, View/M/Menu cannot bypass it or resume driving; Cancel closes the modal first.

Selection and panning must not diverge: pan invalidates stale landmark selection until the reticle snaps/selects another target. Pointer click and A share the same select-then-context model. Undiscovered landmarks stay hidden; discovered acorn markers retain existing disclosure restrictions. An empty discovered-destinations list shows “Discover landmarks while exploring” and disables cycling without focus loss.

Travel is available only for discovered landmarks in free roam. Disabled Travel explains “Available in Free Roam” or discovery requirement. Confirmation names destination and warns that the active activity ends; Cancel is initially focused. A confirms only after a fresh press; B cancels. Recheck arrival safety at commit time. Failure keeps map open with the existing blocked/unsupported explanation; success closes, resumes and shows Arrived at [name]. Keep all existing reset/sampling/branch safety calls and discovery persistence.

Help copy: “Unexplored areas are dimmed.” “Explored areas may still contain acorns.” Keep progress separate from geography, and do not imply visited means searched. Raw world XYZ belongs only to the protected developer tool/documentation.

## Playlists (audit 20–23, 27)

Use a saved-playlist list on the left (about 25% width), the selected playlist's ordered race rows in the center, and a compact header/action area. At 720p target at least six readable rows; scroll to reveal more. Each row shows position, exact track/direction title and lap count. Focus/selection and an “Unsaved changes” indicator are distinct.

```text
PLAYLISTS               My weekend races              Unsaved changes
Saved playlists        #  Track / direction                         Laps
> My weekend races     1  Street Loop - Forward                       3
  Forest practice     >2  Forest Loop - Forward                       2
  New playlist         3  Mountain Loop - Reverse                     3
                       4  Dan's Backyard Loop - Forward               1
                       + Add race

                       Edit   Actions   Save   Start playlist
```

This is a layout sketch, not final controller text. All footer bindings render glyphs at implementation.

| Function | Proposed interaction and preservation |
|---|---|
| Select/switch saved playlist | Up/down in left list, A opens its entries; B from entries returns to saved list. Preserve selection/scroll per playlist |
| Create/name/rename | New Playlist row creates a draft and Rename dialog; header Rename action remains available later. On-screen key grid for controller; normal type/paste/select for keyboard; Save/Cancel; same 64-character limit |
| Browse races | Up/down directly selects visible rows and scrolls; duplicates remain separate entries |
| Add/edit | Add Race row or X Add opens common entry editor; A Edit opens selected row; use existing alphabetically sorted track choices and 1–5 lap stepper; Apply/Cancel |
| Reorder | Y Actions → Move; up/down previews destination, A commits, B restores original order; explicit Move Up/Down buttons also available; row numbering updates immediately |
| Remove | Actions → Remove Race with named row confirmation; keep focus on nearest remaining row, empty state exposes Add Race |
| Save | Always-visible Save action; successful persistence clears dirty flag; failure retains draft and shows actionable error, never “Saved” |
| Start | Visible Start action; disabled when empty/invalid with reason; dirty draft gets Save and Start / Start Without Saving / Cancel; existing clone semantics retained |
| Leave/switch with edits | Save Changes / Discard Changes / Cancel dialog. Discard restores last saved snapshot, not an already mutated library reference |

LB/RB may switch visible Saved/Entries panes only if represented as pane tabs; do not also switch playlists while a reorder/edit mode is active. Prefer A-enter/B-return pane navigation to avoid overloaded shoulders. X/Y are labels for current Add/Actions only; no secret hold/chord to reorder. Existing controller picker may remain as a temporary fallback during phase 1, but phase 4 must provide efficient controller naming. Folder-picker path entry can reuse the keyboard component developed in phase 2.

The shared on-screen keyboard uses D-pad/stick to focus keys, A to enter, X to delete the prior character and Y for Space, with visible Shift, caret movement, Save and Cancel controls. B cancels the draft through the same dialog contract. Use short repeats only for navigation/deletion, never repeated Save. Keyboard typing/paste and controller entry edit the same draft without resetting it on device switch; textbox characters are escaped as text, not interpreted as rich-text markup.

Do not sort/rewrite serialized course indices, flatten duplicates, change eligibility, add unlimited races, delete saved playlists, persist championship sessions or change scoring as part of this presentation work. New playlist deletion is not an existing function and is outside the proposed baseline. Invalid/read-only libraries keep their original files; no automatic overwrite recovery.

## Records and leaderboard tables (audit 16–19)

Records has clear Lap, Race, Speed Traps, Jumps and Ghost tabs. LB/RB changes tab, filters choose track/site, direction, vehicle and compatible current/historical configuration. Filter controls replace blind saved-category cycling, but an “All saved configurations” browser must retain access to every existing category. Selecting a track here does not load a gameplay scene.

Race/lap table: **Rank | Driver | Vehicle | Time**. Primary heading is the standard track name; subtitle contains lap/race and relevant lap count. Driver is **You** for known local-player attempts; migrated entries without identity use **Local record (legacy)**. No online names or AI records are invented. Preserve actual categories even when two human-readable headings look alike.

Speed table: Rank | Driver | Vehicle | Speed | Medal. Jump table: Rank | Driver | Vehicle | Distance | Medal, with recorded airtime in row details. Dates, penalties where actually stored, traffic, roster, difficulty, layout/rules version, legacy status and tie-order explanation belong in Details. Do not invent per-attempt penalty breakdown for saved board entries: that field is absent. Use imperial display conversions already in `DisplayUnits`; canonical values remain unchanged.

Use aligned numeric columns, right-aligned times, subtle alternating rows, teal current/new-record highlight plus a New badge, and prominent top-three ranks without excessive decoration. Empty state says which configuration has no records and how to set one. Save/read errors get a clear status strip; never show fabricated empty success. Unrecognized historic identifiers remain available in Details under “Historical layout” with the raw key, not silently merged with the current course.

Ghost tab preserves enable/disable, compatible best status and clean-lap help. Smash challenge PBs remain visible in Free Roam Activities; adding a smash Top 10 would require a separate data/UI decision. Records screens must not turn current AI finish estimates into stored player times.

## Finish summary, achievements and results (audit 24–28)

Capture a read-only finish presentation snapshot from existing completion events. For lap and race independently store eligible measured time, compatible category, prior personal best, prior board leader, final rank, attempt/entry ID and whether it is a strict improvement. Best lap is the fastest eligible lap from this attempt, not simply the final lap's `LapRank`. Resolve its rank after all this player's lap insertions; retain full precision and existing tie ordering. Do not write duplicate board entries to derive the display.

| Condition, separately per Lap/Race | Headline |
|---|---|
| Strictly beats an existing compatible #1 | NEW COURSE RECORD! |
| First eligible time in an empty board | FIRST COURSE RECORD! |
| Newly retained rank 2–3 | PODIUM TIME! |
| Newly retained rank 4–10 | TOP 10 TIME! |
| Strict personal best outside Top 10, if supported by the relevant PB category | NEW PERSONAL BEST! |
| Equal to existing #1 without strict improvement | Matched Best / Top 10 as appropriate; never “faster” |
| No achievement | Finished; show measured times without false celebration |

“Course record” means **this local compatible category**, not a global/world record across vehicles/configurations. Show the category subtitle. PB outside Top 10 is a conditional fallback, not a fabricated case: local-only boards and different legacy PB partitions can make it unusual. Preserve the current category scopes and explain them rather than changing record rules to force a headline.

Show two compact cards if both qualify: Lap and Race, each with rank/time and its own prior comparison. “Double Top 10!” can be a shared heading, but never hide the stronger course-record label. Improvement is old minus new at full precision, formatted consistently; label **Previous Course Best** vs **Your Previous Best** accurately. With no previous value show “First recorded time,” no invented delta. If rounding would display 0.000s for a true improvement, say “Less than 0.001s faster.”

Animate briefly (about 0.6–1.0s) with a teal accent sweep and optional existing-volume-controlled short sting. No long input lock, repeated stings or unskippable sequence. Let the cards settle immediately on input without consuming a second unintended action; keep Complete Race visible throughout waiting. Reduced presentation can be a later preference, not a new mandatory settings project.

When unfinished AI remain, the primary footer is the actual Submit glyph + **Complete Race**. Its action calls existing guarded `FinalizeUnfinishedAi` exactly once; CR-064 estimates, automatic preference and natural completion timeout are unchanged. Include a small explanation: “Finishes the remaining AI with estimated times.” If classification finishes naturally/automatically during celebration, transition once and retain the cards on Results. No stale actionable waiting button.

Final Results shows Standings (measured / Estimated / DNF clearly distinguished), Lap Times (restores the currently overwritten detail), and Penalties. Next Race is primary for a playlist with another event; Race Again/Restart Current Entry remains explicit and preserves championship replacement semantics. Championship has cumulative points and scrollable event summaries, winner/joint winners, solo completion, and existing tie rules in Help. Every secondary function currently exposed from Results remains reachable through Race Setup, Records, Exploration and Settings without losing Results as the caller.

## Migration and protected behavior

Add presentation adapters around existing services first. Maintain the current screens as a fallback until each replacement's routes/actions are complete, then retire that screen's old branch; never run two listeners for the same context. No permanent player-facing dual UI. Reuse existing save formats and paths by default; no record/category/playlist index migration is needed for visual sorting or new navigation. Draft editing requires a copy, not mutation of the loaded library followed by an imaginary Cancel.

Preserve settings, colors/roster, course IDs, ghost/discovery/acorn IDs, record received/migration IDs, atomic writes and unreadable-file protection. Keep title audio and persistent music lifetime intact. Restrict RaceFlow changes to adapters and necessary navigation/input ownership. Do not edit vehicle physics, geometry, AI, CR-064 math, recovery, race timing, routing arrows, HUD or racing minimap. Menu/HUD shared canvas and the map-M/radio-M collision are documented dependencies; neither permits a driving UI redesign.

Launcher 30–31 is inventoried because it is player-facing, but sits outside the five in-game phases. A separately approved presentation-only launcher follow-up can use the same graphical assets, meaningful-device detection, stick navigation and controller-dismissable error surface. Preserve Play/update/offline/rollback/signing/music/startup behavior. Until that is delivered, acceptance must say “in-game menus controller-complete,” not claim complete controller recovery through every native launcher failure.

## Five bounded implementation phases

No phase is implemented by this audit. Each requires a later request and leaves a playable game. Apply standing build/push/release rules to each future source-changing delivery. Verification below is targeted; no all-track/AI/vehicle gameplay matrix.

| Phase | Deliverable / dependency | Targeted acceptance and stop condition |
|---|---|---|
| 1. Shared navigation and input | Shell, action registry, glyphs, active-device service, focus stack, modal/text ownership; adapt a representative existing menu without replacing gameplay | Controller → mouse/keyboard → controller prompts; no drift flicker; rebinding override reflected where supported; disconnect fallback; A/B/Escape correct; close/release cannot activate parent; no HUD or audio changes |
| 2. Core screens and music | Main/Race/Tracks/Garage/Opponents/Pause/Settings/Music, in-game folder picker and reusable keyboard; needs phase 1 | Reach every existing action without mouse; one track scene switch keeps caller/music; profile/color/roster and unlimited/finite settings retained; nested Back/focus; one valid folder, cancel and unavailable path; no native picker dependency for controller |
| 3. Exploration and World Map | Large map, unified reticle, destination list, all utility bindings, travel/reset dialogs; needs phase 1 and core pause routing | Controller pan/zoom/select/waypoint/clear/center/course toggle; undiscovered destinations skipped; one safe travel, cancel and blocked arrival; permanent network unaffected by overlay; opening M does not toggle music; no minimap change |
| 4. Playlists and Records | Visible playlist lists/editor/reorder/name/dirty flow; real record tables/filter details/Ghost; needs common lists and keyboard | Edit/reorder/remove duplicate rows; cancel restores draft/order; save/reopen/switch and invalid/empty state; one compatibility choice; current + historical board isolation and tied ordering; no fabricated identity or save-schema mutation |
| 5. Finish feedback and consistency | Best-lap/race snapshot, waiting action, results/lap/penalty/championship views; needs record adapters and phase 1 shell | Small synthetic fixtures for #1, #2–3, #4–10, PB fallback, first/tie/no record, both achievements and best-lap-not-last; one waiting → Complete Race/results path; estimated/DNF labels; one championship snapshot restart; verify 720p/800p/1080p menu layouts and final navigation census |

For every phase, check the changed screens against audit IDs and actual action callbacks. Keyboard/mouse must retain equivalent operations. Dan's physical controller/Deck and visual acceptance remain distinct from automated UI fixtures. Stop after the phase's checks pass; no unrelated gameplay retesting.

## Review decisions and completion definition

Recommended decisions for Dan's review: seven Main Menu destinations, current uGUI retained, five in-game phases, fixed-reticle map with X course overlay/Y waypoint, list-based playlists, category-preserving local tables, brief dual lap/race achievement cards, and the small in-game folder picker needed for genuine controller setup. Native launcher presentation is a separately bounded follow-up.

Implementation is complete only when every normal in-game action in audit 01–28 is reachable with a controller (11 replaced by an in-game option), prompts follow the active device and binding, Back/focus are consistent, saves and protected gameplay remain intact, and Dan has reviewed the resulting UI. This document marks none of those implementation checks complete.
