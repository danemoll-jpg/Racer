# In-race map/menu input regression — 0.53.0-review1

Safety checkpoint: clean main `6a06d9ef9a31be08793ce1379f1ba1e71ac23042`.

## Reproduced cause

The shared Phase 1 input barrier introduced in `ed4eafb3` reused `StartupTitle.ButtonHeld()`, which scans every keyboard/gamepad/mouse button, including throttle/brake triggers and keyboard driving keys. Every lifecycle transition sets this barrier. Both RaceFlow input and World Map input return early while it is active. Holding throttle after opening/closing a menu could therefore keep both buttons unresponsive indefinitely. The early return could also stall countdown.

`Tools/Reproduce-RaceInput.cs` reproduced the pre-fix failure with isolated muted saves: held throttle stalls countdown, Pause and Map presses are ignored, releasing throttle clears the barrier (`reproduction.txt`).

## Focused repair

`MenuInput.ConsumeThroughRelease` now captures held button controls from the consumed UI InputAction, rather than waiting for all device input to become neutral. Lifecycle-only transitions keep the short frame barrier. Nested transitions retain their consumed controls, and releasing or disconnecting those controls clears the barrier. Unrelated/new driving input cannot extend it. Effective bindings are used, so an actual UI action rebound to a trigger still waits for that trigger.

The shared action registry, legacy menu callbacks and direct pause/back/map-toggle handlers pass their actual UI action. Title-screen dismissal remains unchanged. No timeout workaround, removal of held-button protection, vehicle-input/physics/AI change, geometry/map redesign, records change or SCS work.

## Bounded acceptance

`Tools/Check-RaceInput.cs` runs one isolated muted scene with production Input System/EventSystem actions. It checks real countdown under held throttle/brake/steering; two pause/Confirm-resume/map/Back cycles with those inputs continuously held; two Escape/M cycles with W/A held; held UI action protection; controller disconnection; and a trigger bound to a UI action. Vehicle motion is frozen after countdown so this remains an input/lifecycle check rather than a driving test.

Simulated controller/keyboard checks do not establish physical-controller or Steam Deck acceptance. Detailed gameplay remains Dan's review. Stop feature testing once these targeted checks pass, then perform the standing build/release/launcher workflow.

Final targeted result: **34 unique assertions PASS** (32 repeated controller/keyboard/countdown/disconnection checks in `initial-checks.txt`, then two effective-binding checks in `rebinding.txt`). Earlier rebound assertions ran from EditorApplication and saw the Editor input buffer, then asserted release before the queued event reached the player. Preserved diagnostics show runtime throttle was still 1 at that premature assertion. The corrected fixture consumes in the Dynamic input context and waits for observed release; final throttle=0, brake=0.6, barrier=false. Runtime implementation did not change during these fixture corrections. No passed gameplay cycle was repeated. Original input settings restored. Feature testing STOPPED.
