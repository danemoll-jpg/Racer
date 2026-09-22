# Mountain menu startup repair — 0.24.1-review1

Rollback checkpoint: `2fe1dc57d6ad0e3331a3762334832cc5fe5e5933`.

The latest structural release used `ReverseReviewRelease.Scenes`, an older
four-scene list. Neither Mountain scene was included in the player, although
the existing menu still called `SelectMountain` with the correct scene names.
Earlier releases explicitly appended both Mountain scenes.

The existing build script now appends both required scenes and fails if either
is missing. No menu labels, selection callbacks, race definitions, scene files,
road meshes, route geometry, terrain, ramps, or vehicle physics are changed.
The version and player notes identify this launch-only correction.

The opt-in `MountainMenuValidation` check uses isolated saves, selects the
actual Track and Mountain UI buttons, invokes Start race, checks the expected
scene and player spawn, and exits immediately after countdown reaches Racing.
It does not run during normal play. No driving or geometry acceptance is tested.

Build, startup checks, and publication results are recorded beside this file.

## Verified startup results

Windows build succeeded with zero errors and four existing warnings.
Both `forward.txt` and `reverse.txt` pass actual menu selection, correct scene,
normal starting grid, and countdown-to-Racing checks. The checks exited at GO;
no track driving or geometry repair was performed. Initial verification was
corrected to account for the existing opponent grid instead of assuming a solo
spawn; this did not change game startup behavior.

`git diff` confirms no Assets/Scenes or Assets/Track changes from the checkpoint.

## Publication and launcher verification

- Fix commit: d2d12df39f5a2d1f015355f82966b6f44a90d4e2,
  `Restore Mountain Forward and Reverse menu launches in release build`.
  Pushed to origin/main after explicit user approval.
- Existing packaging verified extracted ZIP, versioned runtime, and Latest by
  SHA-256. Existing signing key and release tooling used without key changes.
- Published version 0.24.1-review1 / game-24001 as the latest non-draft release:
  https://github.com/danemoll-jpg/woodstock-rush-releases/releases/tag/game-24001
- All three assets uploaded and remote digests verified. An initial immediate
  GitHub listing missed the newly created empty draft; the existing publisher's
  supported --resume-draft option completed the same draft successfully.
- Public catalog and game manifest signatures verified by the existing updater;
  all 261 public game files match Builds/Latest. Public download, installation,
  activation, and isolated startup handshake passed with exit code zero.
- Invoked unchanged Play-Racer.cmd and verified it launched
  C:\Users\danmo\Racer\Builds\Latest\Racer.exe, version 0.24.1-review1,
  with a responsive Racer window. Closed only that verification process.
- No Mountain scene, track asset, or route geometry was modified in this pass.
  No track repair or lengthy driving test was performed.
