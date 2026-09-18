# CR-020 / CR-021 SESSION HANDOFF

**Current delivery:** CR-020 vehicle audio and CR-021 Windows solo package IMPLEMENTED, AWAITING REVIEW. Phase 8 remains accepted for now. CR-013/CR-018 remain deferred. No multiplayer/networking or environment expansion.

**Safety checkpoint:** 3dbe2b606fbd4a17a87dc118c295d4ab30dc1cae. Initial index.lock permission failure recovered by elevated retry before edits.

**Committed player source:** 72f037ea9d5f80a527e5172c6bb27f6a9e82bfed. VERSION.txt inside the package records this exact commit. The later completion commit records packaging and validation documentation; its ID is in Git history and the final task response.

**Audio/settings:** Original CC0 layered engine idle/load, smoothed audio-only revs, reverse, actual-slip tires, road/off-road rolling using existing terrain colors, restrained collision/landing/prop feedback with shared cooldown. Six fixed vehicle sources; nine including ambience/UI. Persistent Vehicle Volume defaults to 75% for existing saves. Master, ambience and race/UI controls preserved. No accepted vehicle physics/tuning changed. Procedural arcade/placeholder timbre still requires listening review.

**Windows package:** 0.2.0-review1, non-development x64. Builds/Racer-0.2.0-review1-Windows.zip; 76,284,958 bytes (72.75 MiB). SHA-256 A2124C11721AC816B60957C1AAF94D05FFB91857C3ED5997423736CF7E4D8303. Extract the complete archive, then launch Racer.exe inside its folder. Includes full runtime, README, VERSION, original-audio notices, package notices and official Unity Windows Mono notices. No upload or sending performed; other operating systems require separate builds.

**Actual validation:** 24/24 audio checks and 37/37 race-flow checks passed in visible Editor Game view with ordinary frames, virtual input and isolated saves. Build succeeded with zero errors/two existing warnings. 228 extracted files hash-match. Visible extracted player launches outside project; mouse settings/start/countdown work; all four changed volume values survive close/relaunch. Zero TCP/UDP endpoints observed. Detailed tests, source licenses, paths, logs and screenshot: Docs/CR020-021/VALIDATION.md.

**Limits:** No subjective listening feed, physical controller or friend's-PC testing. Desktop keyboard injection did not reliably reach the release; extracted-copy driving, pause/reset/restart/results still need manual validation despite passing Editor regression. No physically disconnected-network test or clean PC without Unity test. No mixed-output clipping measurement; loop/sample bounds are objective only. Existing stripped DOF/Panini player warnings and collision-prebake build warning remain; rendering was not altered to hide them.

**Preservation:** Scene/prefabs/environment/road/houses/store layout/forest/jump/shortcut/vehicle tuning/camera/packages unchanged from checkpoint. Build Settings now target StreetLoopGreybox instead of PrototypeTrack; product version updated. Personal saves untouched; standalone checks use the separate Temp/Racer-CR021-visible-save directory.

**Dan/friend review:** Extract complete Windows ZIP; finish a race; listen through idle/acceleration/coast/brake/reverse, ordinary turns/skids, off-road and jump/landing/props; test four volume controls, pause/reset/restart/results, relaunch persistence and a physical controller. Report hardware, audio device and any harsh/missing/repetitive sounds or stalls. Both CRs await review.
