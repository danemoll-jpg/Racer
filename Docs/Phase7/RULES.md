# Phase 7 game flow

Phase 6 remains accepted. Phase 7 is one delivery; Dan retains review/acceptance. CR-013 photo accuracy and CR-018 exact house placement stay deferred. No stunt scoring, speed traps or Phase 8 work.

## Controls

- Drive: RT / W / Up; brake and reverse: LT / S / Down; steer: left stick / A-D / Left-Right.
- Start a race: select Start race with A / Space or click it. Every race starts with a three-second countdown.
- Pause/resume: Start/Menu, Enter or Escape. These replace the old immediate-restart bindings. B also resumes from Pause.
- Restart the whole race: Pause > Restart race, or Results > Race again. Restores the original fixed spawn, clears all current timing/splits/checkpoints, restores breakable props with overlap deferral, and runs another countdown. Personal bests survive.
- Vehicle reset: Y / R, while racing only. Returns to the existing spawn, clears current-lap checkpoint credit and shows an abandonment message. Completed valid laps survive; the total race clock continues. It does not restore props or restart the race.
- Menu navigation: D-pad, left stick or arrow keys; A / Space confirms; mouse can click. B / Escape backs out of Settings to the screen that opened it. Enter/Start also returns from Settings. Focus is restored to the previous menu item. Escape has no destructive action on Ready/Results.

## Timing and eligibility

The retained timing rule is crossing START in the arrow direction after GO. Neither Ready nor the countdown consumes race time. The car is kinematic and its motor/input/reset components are disabled during countdown; the accepted vehicle settings are never rewritten. GO restores the original rigidbody mode and motor. The approach to START after GO remains untimed, as in the accepted race logic.

Pause uses simulation time scale zero, preserving the current countdown or race state and physics velocities. Input/UI continues with unscaled updates. Environmental audio pauses; short UI feedback remains available. Resume restores the prior state, without replaying countdown ticks. Results freeze scoring on the final valid gate sample, clear car velocity and lock the car; the existing camera stays in place safely.

The race still requires three valid laps. Ordered gates, heading checks, swept crossings, discontinuity rejection and unrestricted paths between gates remain unchanged. Invalid attempts and reset-abandoned laps cannot enter lap results. A valid completed lap **does qualify immediately**, even if the race is later abandoned. Only a completed three-lap race qualifies for the race record. Total race time includes time spent on invalid or reset-abandoned attempts; displayed lap splits contain only completed valid laps. Repeated finish samples are ignored.

## Saves and settings

Normal storage is `Application.persistentDataPath/Phase7/street-loop-gates-v1-laps3/`, currently Windows `%USERPROFILE%/AppData/LocalLow/DefaultCompany/Racer/Phase7/street-loop-gates-v1-laps3/`.

`records.json` contains schema version, course/rules identifier, best valid lap and best completed race. `settings.json` contains version, master/ambience/feedback volumes, VSync and frame cap. Current race progress is memory-only. Course/rules version must change if the track, gate rules, timing rule or accepted vehicle class materially changes; lap count is already part of the key.

Missing files use defaults. Malformed, nonfinite, negative or incompatible values are rejected. Writes use a temporary file and atomic replacement with `.bak` of the previous file. Read/write failures do not crash gameplay; the HUD reports that data could not be saved and the in-memory record remains. Backups are retained for recovery, not automatically imported. Tests use explicitly isolated directories under `Docs/Phase7`, never the normal player directory.

Defaults: master 80%, ambience 100%, race/UI 65%, VSync on, frame cap 60. Volume buttons cycle upward by roughly 10 percentage points and wrap from 100% to mute. Changes apply and save immediately. Frame cap choices are 30/60/120; VSync takes precedence. No resolution/fullscreen switch is exposed, so no disruptive display confirmation is needed. No setting changes vehicle tuning.

## Audio sources

Countdown, GO, finish, record and menu clicks are original runtime sine synthesis in `RaceFlow.Tone`, with smooth envelopes and restrained levels. These generated sounds are dedicated to CC0-1.0; no downloaded, paid, recorded or third-party samples. One shared UI source is added. The two original wind/bird voices and their generated Phase 6 assets are retained; see `Assets/Environment/Phase6/AUDIO_SOURCES.md` and `Docs/Phase6Environment/VALIDATION.md` for their original provenance. Volume multipliers apply without restarting or duplicating ambience.

## Review checklist

Launch, select Start race and hold throttle through the countdown; pause and resume once before GO. Drive through START, try the shortcut and jump, reset the car once, then restart from Pause. Complete three valid laps, inspect splits/new-record labels and choose Race again. Try Settings with controller/keyboard/mouse; quit and relaunch to confirm your records and settings. Judge audio balance and physical-controller comfort.
