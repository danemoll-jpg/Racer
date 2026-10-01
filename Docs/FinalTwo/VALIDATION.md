# Final two corrections — 0.58.0-review1

Safety checkpoint: clean main `414d092ce92288927cb550e15def201085d6646f`. Dan accepts the prior rat visuals and Forest Forward cave environment. Only rat audio and one cave obstacle are changed.

## Storm Drain rat audio

The existing event triggered, but its generated squeak amplitude was only .16, scratch amplitude .09, and source gain .48 before spatial attenuation and master volume. The thin, regularly repeating high tone competed poorly with engine/radio. The source also stayed at the entrance instead of following the rats. Prior muted tests established triggering only.

The same source now plays three brief lower chirps over irregular scratching, at .48/.30 synthesis amplitudes and .85 source gain. Near distance is 16m with linear fade to silence by 48m; priority 80 prevents ambient voice competition. Full 3D localization remains. The 2.2-second event begins with the first visible movement, follows the group centroid, and retains the existing away/rearm conditions. No additional sound system, looping rat ambience, music ducking or visual/movement changes. Cave drips unchanged.

Two ordinary play-frame approaches use actual motorcycle/ATV motor physics, a chase-camera listener, engine throttle, radio and ambience. Isolated settings: master .8, vehicle .75, music .6, ambience 1. Each records one encounter and one sound, active engine and radio. Motorcycle mix peak .309352 / RMS .046784; ATV peak .331115 / RMS .047666; zero clipped samples. Rat source RMS .029946/.029470; nearest listener distance 5.61/6.17m. Listener DSP and source WAVs are local only and excluded from Git/package because they contain radio. These are signal/trigger checks, not a claim of human listening or OS speaker acceptance.

An initial fixture failed before entering the race because title startup had not initialized radio. A subsequent attempt remained muted by TestAudioMute; its failed evidence is retained. The final fixture temporarily disables that test-only component and restores mute immediately after both approaches. Production mute/settings behavior was not changed.

## Forest Forward Echo Cave rockfall

One overlapping four-rock partial cave-in occupies original cave stations 207–217, centered around `(82,38,137)`, well before the existing jump. Fractured blocks rise 2.25–3.9m and join the left wall. Steep lower faces stop careless driving without creating a launch ramp. The right passage is approximately 3.8m wide. The local AI line shifts right by 3.05m, aligning over stations 169–198 and rejoining over 225–254. Existing local guidance uses an 8m lookahead. No global AI changes or Forest Reverse AI use.

All rocks use their visible mesh for collision. ATV-width sweeps hit the left/center blockage and clear the intended right line. Existing recovery exclusion metadata spans original stations 188–239, including the opening and immediate approaches. It supplies no collider, reset trigger or vehicle forces; global recovery remains unchanged. All six exclusion/boundary checks pass.

Final targeted manual-physics drives: motorcycle and ATV both complete at a 27m/s target, minimum speed 26.76m/s, minimum up-vector .998, no airtime. Maximum tracking error .417/.416m. Straight-line runs physically stop at the rocks (minimum speed approximately zero) and remain upright (.998), requiring a different line. The production RoadDriver test covers the local approach, opening and exit with existing probabilistic branch selection, zero resets/recoveries. See `cave-driving/driving-done.txt` for the final result.

Saved player-height views were inspected. Initial inward mesh normals were corrected before final contact/traversal verification; first-check evidence is retained. The surrounding cave mesh, ceiling, floor, lights and formations remain unchanged. Automated steering is not human difficulty acceptance.

## Scope and delivery

Preservation audit: three existing scene records change (parent child list, route component, local guidance attachment), 24 new records, no existing objects removed. Backyard Reverse scene and the existing global driver, physics and recovery sources are unchanged. Forest Reverse is unchanged. Incidental Editor material/physics serialization is restored before the completion commit.

Targeted checks are the stop condition. No full laps, broad regressions or further world/UI/track work. Fresh build, signed game-58000 publication, complete Latest, production launcher verification and cleanup are recorded separately in PUBLICATION.md. Dan reviews final subjective sound and driving feel.
