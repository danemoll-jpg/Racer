# Racer / Woodstock Rush - Codex Standing Rules

These instructions are mandatory for every Codex task in this project.

Read this file BEFORE making changes.

PROJECT_TODO.md is the authoritative source for project state, accepted decisions, bugs, backlog and current implementation status.

A direct instruction from Dan in the current task may override a standing rule for that task.

Do not modify or weaken these standing rules unless Dan explicitly requests a standing-rule change.

---

## 1. SAFETY CHECKPOINT BEFORE EVERY TASK

Before modifying the project:

- inspect Git status;
- preserve all current user/project changes;
- commit currently saved/uncommitted work as a safety checkpoint;
- if already clean, record the current HEAD.

Never discard Dan's current work to obtain a clean tree.

Never begin destructive changes without a rollback point.

---

## 2. READ PROJECT_TODO.MD FIRST

Before implementation:

Read PROJECT_TODO.md.

Use it to understand:

- current accepted baseline;
- completed work;
- current bugs;
- deferred/canceled work;
- route identities;
- known regressions;
- protected areas;
- release state.

Do not resurrect old plans that PROJECT_TODO.md says were canceled, superseded or completed.

The latest explicit decision wins over historical notes.

---

## 3. KISS

KEEP IT SIMPLE.

Implement the smallest change necessary.

Do not respond to a local problem by redesigning an entire system or area.

Do not add unrequested:

- roads;
- bridges;
- bypasses;
- ramps;
- platforms;
- systems;
- structures;
- features;
- scenery;
- alternate implementations.

Preserve working content.

---
==================================================
## 4. ENVIRONMENTAL DEPENDENCY / GROUNDING RULE
==================================================

Whenever changing the elevation, slope, position or shape of terrain, roads,
driveways, trails or other supporting surfaces:

YOU MUST INSPECT AND PRESERVE EVERYTHING PHYSICALLY DEPENDENT ON THAT SURFACE.

Do NOT treat terrain/road geometry in isolation.

Before modifying the supporting surface, identify nearby affected objects,
including where applicable:

- houses;
- garages;
- kennels;
- sheds;
- pool houses;
- fences;
- gates;
- signs;
- posts;
- trees;
- rocks;
- props;
- ramps;
- sidewalks;
- parking areas;
- road shoulders;
- colliders;
- checkpoints;
- navigation objects;
- other structures resting on or attached to the changed surface.

After changing the supporting surface:

ADJUST DEPENDENT OBJECTS TO MATCH THE NEW FINAL GROUND LEVEL.

==================================================
NO FLOATING STRUCTURES
==================================================

Buildings and grounded structures must NEVER be left suspended above the
terrain because the terrain beneath them was lowered.

If terrain around/beneath a structure must change:

- preserve a properly supported foundation/ground relationship;
- adjust terrain locally around the structure where appropriate;
- or reposition the structure vertically only when necessary to maintain its
  intended relationship to the ground.

Do not leave visible empty space beneath buildings.

Examples:

WRONG:
lower terrain
→ kennel remains at old elevation
→ kennel floats in air.

CORRECT:
lower surrounding terrain
→ preserve/reshape supported ground beneath kennel
→ kennel remains naturally grounded.

==================================================
NO BURIED STRUCTURES
==================================================

Objects must not become buried because terrain/road elevation was raised.

Examples include:

- fences;
- gates;
- signs;
- posts;
- building walls;
- props.

If the supporting terrain is intentionally raised:

adjust affected grounded objects so they continue to sit naturally on the new
surface.

Example:

WRONG:
raise driveway
→ fence stays at old Y
→ lower half of fence becomes buried.

CORRECT:
raise driveway
→ inspect affected fence/gate
→ raise/reseat affected fence sections to the new ground height
→ preserve their intended relationship to the driveway/property.

==================================================
PRESERVE RELATIVE RELATIONSHIPS
==================================================

When changing elevation, preserve intended relationships such as:

- fence sits on ground;
- gate meets driveway;
- sign posts meet terrain;
- building foundation meets ground;
- driveway meets parking area;
- parking area meets building/property;
- ramp meets road smoothly;
- road shoulder meets surrounding terrain.

Do not fix one surface while visibly breaking everything attached to it.

==================================================
LOCAL ADJUSTMENT ONLY
==================================================

Do not blindly move every nearby object by the same amount.

Inspect what is actually affected.

Some objects may need:

- vertical repositioning;
- local terrain support;
- fence/post adjustment;
- collider adjustment;
- no change at all.

Make the smallest coordinated changes necessary to keep the environment
physically coherent.

==================================================
POST-CHANGE GROUNDING CHECK
==================================================

After ANY terrain/elevation/road-height modification, perform a targeted
dependency inspection around the changed area.

Verify:

- no buildings float;
- no structures are buried;
- fences remain grounded;
- gates still meet their driveways;
- signs remain grounded;
- props remain supported;
- terrain does not expose building undersides;
- colliders still correspond to visible surfaces;
- road/driveway transitions remain clean.

This inspection is part of the terrain change itself and is NOT considered
optional extra testing.
## 5. REGRESSIONS

When Dan supplies Unity XYZ coordinates for a requested change, treat them as the authoritative location reference. Inspect the area around those coordinates and use screenshots/descriptions to identify the intended object/change. Do not substitute a similarly described location elsewhere. If coordinates conflict with a vague description, prioritize the coordinates. Ask Dan only if the intended object remains genuinely ambiguous after inspecting that location.

If something previously worked and is now broken:

CHECK GIT HISTORY / DIFFS FIRST.

Prefer identifying and correcting the regression over stacking another workaround on top.

Do not globally revert unrelated successful work.

---

## 6. DO NOT CHANGE UNRELATED SYSTEMS

Do not modify unrelated:

- track geometry;
- vehicle physics;
- AI;
- recovery;
- navigation;
- audio;
- UI;
- scenery;
- routes;

unless required by the current task.

If a requested fix can be local, keep it local.

---

## 7. PERMISSIONS - DO NOT BOTHER DAN UNLESS NECESSARY

Dan authorizes normal development operations required to complete assigned Racer tasks.

Do NOT ask Dan for redundant permission to:

- use Unity;
- approve normal Unity operations;
- edit project files;
- run existing project scripts/tools;
- use Git;
- commit;
- push;
- build;
- package;
- use the existing GitHub CLI;
- use the existing publisher;
- use the existing signing system;
- publish releases;
- perform explicitly authorized cleanup.

If a normal approval prompt can be approved from the Codex environment:

APPROVE / ALLOW IT AND CONTINUE.

Only involve Dan when an external security/authentication/permission boundary genuinely requires his direct interaction.

If that happens:

1. preserve all completed work;
2. explain exactly what is blocking progress;
3. give Dan the EXACT command or action required;
4. say exactly where to perform it;
5. WAIT for Dan;
6. after Dan reports success, RESUME THE SAME TASK FROM THE EXISTING STATE.

Do NOT:

- abandon the task;
- silently skip the blocked step;
- declare partial completion;
- redo completed implementation;
- unnecessarily rebuild;
- install replacement tooling merely to avoid asking Dan.

---

## 8. UNITY

Use the existing Unity project/tooling.

If Unity or Unity Hub presents a normal approval dialog that Codex can approve, approve it.

If Windows requires Dan to approve something manually, ask him for that specific action and then resume.

Do not unnecessarily restart Unity/build work after a permission issue is resolved.

---

## 9. GITHUB CLI

GitHub CLI already exists at:

`C:\Users\danmo\Racer\Builds\PublisherTools\gh-2.101.0\bin\gh.exe`

Use this executable directly.

Do NOT assume GitHub CLI is missing merely because:

`gh`

is not globally available on PATH.

Do NOT install another copy unless Dan explicitly requests it.

If GitHub authentication genuinely requires Dan:

give him the exact authentication command/action, WAIT, and resume afterward.

---

## 10. EXISTING SIGNING IDENTITY

Use the existing publisher/signing identity.

Never:

- regenerate signing keys;
- replace signing keys;
- delete signing keys;
- expose private keys;
- print private keys;
- create another publisher identity.

If access to the existing key requires Dan's Windows permission intervention, ask for the exact action and resume afterward.

---

## 11. TARGETED TESTING ONLY

Dan performs detailed gameplay testing.

Codex should perform enough testing to establish that the requested implementation technically works.

DO NOT spend excessive time/credits running broad regression matrices.

Test:

- the changed feature;
- directly affected dependencies;
- a small number of representative edge cases.

Do NOT automatically test every:

- track;
- direction;
- vehicle;
- AI configuration;
- difficulty;
- weather combination;

unless the task specifically requires it.

Passing the targeted acceptance checks is a STOP CONDITION.

Then build/release.

---

## 12. DO NOT TUNE SUBJECTIVE GAMEPLAY FOREVER

If a change is technically functional but requires subjective gameplay evaluation:

STOP TUNING.

Dan will playtest it.

Do not burn time trying dozens of tiny variations unless specifically asked.

---

## 13. UPDATE PROJECT_TODO.MD EVERY RUN

At completion of implementation/testing:

Update PROJECT_TODO.md with the ACTUAL state.

Record:

- what changed;
- decisions made;
- bugs fixed;
- bugs remaining;
- testing actually performed;
- new version/release;
- relevant commit IDs;
- new limitations/discoveries.

Remove or correct stale CURRENT backlog/status entries when Dan's later decision supersedes them.

Do not rewrite historical evidence merely because plans changed.

---

## 14. COMPLETION COMMIT AND PUSH

After implementation and TODO update:

Create a clear completion Git commit.

PUSH the completion source commit to the existing configured source remote/branch.

A local commit alone is NOT completion.

Verify the push succeeded.

---

## 15. FRESH UNITY BUILD

For every gameplay/source change requiring a new playable game:

BUILD A FRESH UNITY WINDOWS RUNTIME FROM THE COMPLETION SOURCE.

Do not publish an old Racer.exe with new metadata.

The new build must correspond to the completion source commit.

---

## 16. BUILDS\LATEST MUST CONTAIN THE NEW BUILD

The complete new runtime must be installed/staged at:

`C:\Users\danmo\Racer\Builds\Latest`

This means the complete Unity runtime, including as applicable:

- Racer.exe;
- Racer_Data;
- Unity/runtime dependencies;
- VERSION.txt;
- required DLLs;
- required data/assets.

Do NOT replace only Racer.exe inside an older runtime.

Do NOT update VERSION.txt around an old executable.

The local Latest runtime must represent the new build.

---

## 17. PUBLISH A NEW GAME RELEASE

For gameplay/source changes requiring a new playable build:

Publish a NEW game release using the existing Racer publisher workflow.

Release repository:

[https://github.com/danemoll-jpg/woodstock-rush-releases](https://github.com/danemoll-jpg/woodstock-rush-releases)

Do not overwrite the previous release.

Upload all required release assets.

Verify remotely that:

- the release exists;
- it is published;
- required assets exist;
- catalog/manifest identifies the new version;
- uploaded assets correspond to the new build.

A local build is NOT completion.

---

## 18. PLAY-RACER.CMD

The normal player entry point is:

`C:\Users\danmo\Racer\Play-Racer.cmd`

After publishing, verify the REAL production launcher path:

Play-Racer.cmd
→ existing signed launcher/updater
→ published catalog
→ newly published game
→ new Racer.exe/runtime

Do NOT hard-code Play-Racer.cmd to a temporary local build.

Do NOT replace the updater architecture merely to make verification pass.

If Play-Racer.cmd itself requires no textual change because the updater automatically resolves the new catalog, LEAVE THE SCRIPT UNCHANGED.

The requirement is that running Play-Racer.cmd resolves to and launches the new published game.

---

## 19. LOCAL AND PUBLISHED LATEST MUST MATCH

After publication:

`C:\Users\danmo\Racer\Builds\Latest`

must represent the same new game version as the published latest release.

Do not leave:

- GitHub on the new version while Builds\Latest contains the old game;
- Builds\Latest on the new version while the catalog still points to the old game.

---

## 20. DISK SPACE CHECK

Before a substantial build/package:

check available disk space.

The project previously accumulated more than 300 GB of obsolete build artifacts.

Do not repeat this.

---

## 21. MANDATORY POST-RELEASE CLEANUP

After:

- successful build;
- successful publication;
- remote verification;
- successful Play-Racer.cmd verification;

clean disposable build/release artifacts.

Examples include:

- PackageWork;
- temporary Preserved copies no longer required;
- temporary build directories;
- temporary verification installs;
- temporary public-download installs;
- obsolete CR### candidates;
- obsolete inspection builds;
- superseded review builds;
- duplicate ZIP + unpacked copies;
- obsolete staging directories;
- temporary packaging intermediates.

Do not accumulate complete copies of every build.

---

## 22. RETENTION

Generally retain:

- CURRENT complete local runtime;
- ONE immediately previous useful managed runtime where appropriate for rollback/updater behavior.

Do not retain a large historical collection of complete local builds when published releases already exist remotely.

---

## 23. NEVER DELETE DURING CLEANUP

Always preserve:

- Assets;
- Packages;
- ProjectSettings;
- project source;
- .git;
- PROJECT_TODO.md;
- CODEX_RULES.md;
- Docs;
- route atlas documentation;
- BundleMusic;
- deliberately staged music;
- PublisherPrivate;
- signing keys;
- PublisherTools;
- Launcher;
- LauncherSDK;
- Play-Racer.cmd;
- active signed catalog;
- active signed manifests;
- updater metadata;
- current active runtime;
- required previous managed runtime;
- user saves/settings;
- required release evidence.

If genuinely uncertain whether something is needed:

KEEP IT.

Do not break publishing/updating to save disk space.

---

## 24. CLEANUP REPORT

For release tasks, record approximately:

BEFORE CLEANUP:

- Builds directory size;
- C: free space.

AFTER CLEANUP:

- Builds directory size;
- C: free space;
- approximate space recovered.

---

## 25. DEFINITION OF DONE

For a normal gameplay/source update, completion means:

- safety checkpoint preserved;
- requested implementation complete;
- targeted verification complete;
- PROJECT_TODO.md updated;
- completion commit created;
- source commit pushed;
- fresh Unity runtime built;
- Builds\Latest updated with complete new runtime;
- new game release published;
- remote assets verified;
- signed catalog/manifest updated;
- Play-Racer.cmd verified against the new release;
- disposable artifacts cleaned;
- final disk usage measured.

If one of these steps is genuinely not applicable to a task, state why.

Do not silently omit applicable steps.

---

## 26. FINAL REPORT

Keep final reports concise.

Report:

IMPLEMENTATION

- requested changes and important implementation details.

TESTING

- targeted tests performed and results.

TODO

- important PROJECT_TODO.md updates.

SOURCE

- safety checkpoint;
- completion commit;
- branch;
- push confirmation.

BUILD

- game version;
- source identity;
- active local Racer.exe/runtime.

RELEASE

- published tag/version;
- remote verification.

LAUNCHER

- Play-Racer.cmd verification;
- actual version/runtime launched.

CLEANUP

- Builds before/after;
- recovered space;
- final C: free space.

Then STOP.

Do not begin unrelated improvements.
