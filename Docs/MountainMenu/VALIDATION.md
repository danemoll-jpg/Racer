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
