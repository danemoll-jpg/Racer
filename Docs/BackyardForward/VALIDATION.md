# Forward technical verification

Baseline: clean main `04d3faa1a29d0c05e33adf8f4a054c0b1212f818`. This implementation starts from the accepted rollback/property world, not the rejected Backyard scenes.

## Geometry and preservation

[Geometry checks](geometry-checks.txt) verify all nine hard X/Z anchors exactly, accepted property pavement by asset identity, perfectly flat parking at Y=79.45641, garage/kennel and pool-house foundation support, accepted blue checkpoint materials and absence of gate colliders. Thirty-one forest route samples have trees within 13m. The normal trail is 5.3m wide; its terrain tapers into the existing ground. No forest road/deck mesh is introduced.

The established six scene files and all accepted property/Kyle mesh assets remain unchanged in Git. New terrain and vegetation assets belong only to `DansBackyardForward`. Global vehicle physics, recovery and shortcut probabilities are unchanged. Forward's compact grid and flight steering are scoped to its `BackyardForwardCourse` component. A late local AI speed/line restriction was rejected after the first fresh-player check failed to finish one AI; those restrictions were removed before the final release build.

The dump floor is excavated 3.66m below the original sloping terrain. The launch-to-interior drop measures approximately 12.19m because the existing hillside descends and the dirt launch rises. The same continuous ravine is measured at Z=-108.6, 0 and 60.7; lower-rim-to-floor differences exceed 6m at all three samples. [Terrain profiles](terrain-profiles.csv) record the actual collider heights.

## Physical jumps and escape

[Final feature drives](feature-drive.txt) use the ordinary motorcycle motor in 0.02-second physics steps, normal pedals/steering, and the existing `VehicleSurfaceContacts` correction. No force boosts, teleportation during a drive, or altered physics parameters are used. Starts are placed for bounded approach tests; landing success rejects any ground contact inside the gap before the far bank.

- Dump: 2.16 seconds continuous flight; minimum horizontal launch speed 31.47m/s; grounded within the aligned far landing/runout beyond anchor 6.
- First ravine: 2.18 seconds continuous flight; minimum launch speed 31.05m/s; grounded on the western runout.
- Second ravine: 1.72 seconds continuous flight; minimum launch speed 29.63m/s; grounded on the eastern runout of the same ravine.
- Deliberately slow dump approach: falls inside and drives out, no reset.
- Ravine bottom: drives south to the shallow mouth, no reset.

Early manual fixture outputs are not valid clean-jump evidence: the fixture initially omitted the runtime surface-contact initialization and allowed eventual far-side arrival after a short landing. Both defects were corrected. The final strict results above supersede those exploratory assertions. No global contact behavior was changed.

## Race integration

The menu check selects the actual Forward button, checks its scene/course identity, starts all four racers facing straight west, and verifies minimap presence, blue gates and collider-free race objects. The full-lap fixture uses ordinary `RoadDriver` inputs for a motorcycle player proxy and three real AI (motorcycle/ATV/motorcycle), Normal difficulty, one lap, no traffic, isolated saves and muted audio. It never grants an estimated finish.

The initial Editor races finished all four with missed gates/local recoveries. In the first fresh candidate, one AI failed the completion window; the late local speed/line restrictions were removed. The corrected candidate's first run finished three racers, with one AI reaching the return before the ordinary 90-second finish grace expired. These unsuccessful runs remain in `first-runtime-checks` and `runtime-deadline-check`.

One repeat of the same final binary passed all ten assertions and physically finished all four, without estimated finishes: player proxy 1 missed gate/4 recoveries; EMBER 2/0; GOLD 2/1; BLUE 1/8. The previously timed-out EMBER completed this run without recovery. [Final fresh-player results](runtime-checks/done.txt) and [pace trace](runtime-checks/pace.csv) establish route traversal and working progression, not consistently clean AI or human acceptance. AI incidents and occasional deadline DNFs remain a review limitation; no global recovery, physics, shortcut probabilities or finish deadline was changed.

Ambient continuation traffic is disabled only inside the explicit no-traffic fixture. Final runtime logs contain no NullReferenceException or MissingReferenceException. Early Editor initialization errors from that unrelated system are not hidden as gameplay passes.

Visual checks cover start direction, flat parking, dump, both gully crossings, the shared ravine, wooded trail and road return. The atlas comes from the saved final route data. Dan's detailed gameplay review remains required; Reverse and optional shortcuts remain deferred.
