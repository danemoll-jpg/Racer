# 0.20.1-review1 — CR-118 reopened; CR-120/121 local cleanup

Safety checkpoint: `489b220b9ed1352731b8b655471bcf373620bb57`, verified clean HEAD before edits.

Dan verified household people and turkeys and accepted both Mountain Loop layouts as passable/fun. Older NOT ACCEPTED and pending-sighting wording is superseded. General course polish is future work. Hwy 92 and keyboard naming have not been reviewed by Dan; championship acceptance was not reported. Ghosts remain accepted and untouched.

## Exact findings

- Reverse summit: the southbound runway crosses the summit arrival at roughly (990,164,115). Runway paint and the large jump sign are visible from the incoming west approach, before the actual 220-metre run-up. The angled screen separates the west side-cut while leaving the proper runway centerline open. Crossing paint is hidden locally; the advance sign points left toward the full approach. Only the reverse scene changes; no jump/run-up/main centerline is redesigned.
- Late reverse junction: the Downhill Ridge Cut is 108.1 m against 1,397.9 m of main route. Its optional sign was only 2.2 m from the main centerline; the approach capture visibly shows conflicting signs in the driving area. Those signs are moved onto shoulders. Dan then supplied the exact screenshot: this is a wrong-end/merge invitation, not a request to harden the branch. The unshipped chicane interpretation was removed. Nearby inviting gold arrows are hidden, a merge board distinguishes the branch, and extra teal arrows show the unobstructed main continuation. Accepted shortcut difficulty, original centerline, rejoin and bypass gate credit remain.
- Theme onset: the existing 31.36-second loop begins at sample 11,520 (240 ms at 48 kHz) of the supplied 31.6-second recording. Its remaining body matches the offset source within one 16-bit level. Playing this loop from sample zero omitted the original attack. A new 240-ms opening resource copies the exact supplied PCM prefix, schedules once, and hands directly to the unchanged existing loop. Original MP3, voice WAV, loop WAV and artwork remain unchanged.
- Speech: previous tail captures did not resolve Dan's human clipping report. Artwork is rendered and all clips ready, followed by a 1.5-second realtime/DSP lead-in and 100-ms scheduled buffer. Already-started voice retains its complete tail; dismissal before start cancels pending speech. Radio remains held until speech completes. Theme starts at its established level, without a fade masking its attack.

## Bounded checks

Before/after Editor renders and route/sign audits are retained here. One representative motorcycle local approach per changed area is authorized; no full laps, AI runs, jump/vehicle matrices or repeated pilot tuning. Two brief final-player startup captures are authorized, otherwise players use mute and isolated saves. Actual results are appended below. Audio capture is listener DSP, before the OS endpoint; the available tools cannot audition audio. Human speech/theme acceptance remains open.


The two supplied screenshots are retained as Dan-needs-barrier.png and Dan-confused.png. The initial unshipped interpretation added a chicane; it was removed immediately when the screenshot clarification arrived, before any driving tests. The first close render also found the screen partly below the elevated trail and the relocated boards too low on sloping shoulders. Only those placements/support heights were corrected. Intermediate candidate builds are preserved and were not driven. No failed local vehicle approach has been rerun.


Final Editor views verified-summit-close.png and verified-Downhill-Ridge-Cut-approach.png were inspected: the right-hand jump arrow is occluded by the grounded screen, the main left arrow remains visible, and the merge continuation is free of signs with visible teal paint/gate. The moved boards are readable on grounded posts rather than buried in the sloping shoulders. Geometry/source changes are limited to this reverse scene and its regeneration recipe.


Final build succeeded with zero errors and two warnings. The only local player run passed both segments: summit approach reached 492.1/495.0 m, minimum up 0.99; ridge main continuation reached 1627.9/1630.8 m, minimum up 0.98. Motorcycle normal throttle/brake/steering from rest, Master mute, isolated temporary saves. No AI, full lap, shortcut traversal, jump matrix or rerun was performed. Evidence: local-player.


## Final startup and package outcomes
Exactly two final-player launches were used and remuted immediately after capture. Both pass all five timing/state checks. Untouched startup ran from the extracted ZIP, played the complete voice and restored theme opening, and reached the menu. Early transition preserved the already-started voice. The tiny four-boundary timing fixture covers cancellation before scheduling/start versus preservation after start; a separate third delay-dismissal launch was not performed.

Listener DSP waveform evidence: both full voices correlate 0.99999977 with the supplied recording, including first 200 ms 0.99999996 and final 350 ms 0.99997505. The original theme first second correlates 0.99999947, first 200 ms 0.99999926, at constant fitted gain 0.13485 (no attack fade). The new prefix is an exact copy of the original first 240 ms. This closes the demonstrated missing-theme-prefix defect at the captured signal level, not Dan's human clipping report: no OS-endpoint recording or tool audition is available. Human speech/theme acceptance remains open. Source recording, artwork, prior loop and user volume preferences are unchanged.

All 451 complete-package files were SHA256 checked across runtime, Latest and extracted ZIP. 187 playable bundled tracks plus two originals remain; prior Latest/music and 0.20.0-review1 are preserved. Final metadata/hash report is package-verification.json. No external upload. No other gameplay or audio tests were run.

