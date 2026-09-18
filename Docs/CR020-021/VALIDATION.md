# CR-020 / CR-021 delivery — 0.2.0-review1

Safety checkpoint: `3dbe2b606fbd4a17a87dc118c295d4ab30dc1cae`. Git staging initially failed with index.lock permission denied; elevated retry succeeded before content edits.

Vehicle audio uses original CC0 synthesis, with separate idle/load harmonics, filtered rolling textures and slip noise. Engine revs are audio-only, continuously smoothed, without gear jumps. Reverse uses reverse pedal load and actual speed. Tire sound starts above 2.8 m/s lateral slip with a speed gate. Terrain vertex colors distinguish dark road from shoulder/grass; unknown surfaces use restrained loose-ground feedback. Rolling/tire voices mute immediately without two grounded wheels. Landing needs >0.12 seconds airborne and >2 m/s downward speed. Collision normal speed must exceed 2.5 m/s. Prop feedback fires on the existing successful break event. All impact types share a 0.22-second cooldown and a single interruptible voice.

Six vehicle voices are created once. Ready/countdown idle; racing responds to input/velocity; pause/settings use existing listener pause; reset clears transients; results fade engine and stop impacts. Master remains global; Vehicle Volume defaults to 75% and persists. Existing JSON is populated into initialized defaults, preserving prior fields when vehicle is absent. Ambience and race/UI controls remain separate.

## Tests before packaging

- `audio-validation.txt`: 24/24 checks passed, visible Editor Game view with ordinary frames and virtual Gamepad. Acceleration reached 19.59 m/s; reverse -9.35 m/s; injected actual lateral velocity produced 6.99 m/s slip; road/off-road weights 1.00/0.04. Physical drop produced a landing impact. Reset, pause playback position, mute, persistence, legacy settings and source accumulation checks passed.
- An initial loose-ground boundary sample check failed (0.103486). A short smooth endpoint join was added. All five final loops peak at 0.65 and have zero first/last sample delta. These measurements do not prove inaudible seams or good timbre.
- `Flow/validation.txt`: 37/37 existing race-flow regression checks passed. Virtual keyboard/Gamepad/mouse; synthetic gate crossing for results, isolated records/settings, props, countdown, pause, reset, repeated races, ambience mute and exactly nine total audio sources.
- Compilation succeeded. No scene, vehicle tuning, camera, terrain, rendering assets, packages or race rules changed. Build scene corrected from PrototypeTrack to StreetLoopGreybox; version set to 0.2.0-review1.

## Validation limits

No subjective speaker/headphone audition has been performed by this agent. The computer tools expose screenshots and input, not a listening feed. Procedural sound remains provisional arcade/placeholder quality; realism, repetition, masking and perceived loudness await Dan. Sample bounds and conservative gains provide headroom but are not a captured mixed-output clipping measurement. No physical controller or friend's PC tested. Airborne test is a physical drop, not a full authored-ramp traversal. Collision/prop cooldown is tested directly; the flow suite also triggers an actual prop contact callback. No new full three-lap human drive is claimed.

## Package

The committed release builder uses Windows x64, BuildOptions.None, script debugging/profiler connection off and refuses ENABLE_RUNTIME_PIPELINE. The accepted rendering configuration is preserved. VERSION.txt records the exact source commit read before building. Only runtime output, README, VERSION and license notices go into the archive; backup/debug folders and saves are excluded. Final build and extracted-copy results will be recorded after creation.

License sources: original synthesis documented in AUDIO-ASSET-NOTICES.txt; installed package license/notice files in PackageNotices; official [Unity 6000.6.1f1 Windows Mono notices](https://unity.com/releases/editor/whats-new/6000.6.1f1) retained as Unity-Windows-Mono-Notices.pdf.

Review: listen through idle/acceleration/coast/brake/reverse, ordinary turns/skids, road/off-road, jump/landing/impacts; try volume/mute, restart/results and relaunch persistence; test the physical controller and the friend's extracted Windows copy. CR-020/021 await review. No networking or deferred environment work was started.
