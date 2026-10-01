# Backyard Reverse correction — technical verification

Safety checkpoint: `cad2c3f5c2885bb46c91adc9f7d8b6c6885701b1` on main. This pass supersedes the previous player-only implementation. [Authoritative request](REQUEST.md). Gameplay feel remains for Dan's review.

## Implementation

Logging Ridge retains the original horizontal line and approximate anchors `(394.1,81.6,79.4)` / `(281.3,76.7,124.9)`. Its rolling crest reaches about 9.02m above the original forest floor. A 4.8m usable top, broad irregular sloping shoulders, worn wheel paths, grass intrusion, two small earth rises, fallen timber and stumps replace the flat brown ribbon. The entry climb is spread over 29m. Local recommended speed is 18m/s for the technical crest. Traversable brush slows off-line vehicles; existing trees require steering around them. No reset trigger, invisible wall or new ground-level bypass was added.

The descending, bending drain has 162.21m underground and a 268.86m total branch. Its exit is `(114.3,30.67736,-66.1)`: floor height derives from the existing lower gully sample at `(103,-66.1)`, not the supplied upper-bank Y=42.5. The old exit/ramp is removed. A local opening passes through the lower wall; approximately 8m of straight run-up leads to a 12m curved takeoff with 6m rise and eased terminal grade. The opposite-bank landing and immediate rejoin have a locally cleared flight corridor.

The entrance uses one continuous concrete collision floor, registered with the existing ground-contact normal correction. Decorative water and ridge grass no longer duplicate solid floor collision. Broad shallow water skins, existing splash/immersion feedback, edge channels, side inlets, sediment, joints and modest lights provide readable underground progression. Surface roads above remain supported.

Both branches are available to the existing probabilistic, once-per-lap rival shortcut strategy. Local guidance limits look-ahead, retains the selected underground branch, points obstacle queries along the intended bend, and holds a straight jump target. Ordinary vehicle steering, braking and physics perform every traversal. Forward has only its original Tree-Top Trail and Cabin Jump metadata; its new drain grate and ridge gate close during races. Reverse/free roam opens them. Reverse layout ID `backyard-reverse-v3-ridge-drain` keeps historical Records separate without changing record storage.

## Targeted checks — complete

- [Four final traversals](final-traversals.txt): production driver on motorcycle and ATV completes both branches, with genuine branch entry and existing probabilistic selection. Zero resets and zero AI recoveries in all four. Ridge airtime 0.84s / 1.54s; drain airtime 2.80s / 2.58s. These are automated local driving checks, not human-controller acceptance. Some runs move off the centre line during flight/rejoin; maximum lateral error is recorded, not hidden.
- [Four recovery cases](final-recovery.txt): motorcycle/ATV ridge fall to rejoin in 17.08s / 18.40s; gully undershoot to the existing northern exit in 21.54s / 23.32s. Actual motor, terrain, trunks and brush resistance, without teleporting during each attempt. This edit-mode physics fixture does not exercise the live automatic-respawn event loop. Source/state checks verify unchanged recovery rules and no shortcut reset trigger.
- [23 state assertions](state-checks.txt): Reverse/Forward/free-roam gates, original Forward branches, both AI registrations, CP3-only drain entitlement, wrong-way and upper-surface rejection, brush resistance/exemption and grounded ridge trunks all pass. Maximum sampled trunk foot error is under 0.019m.
- [Protected support audit](road-supports.txt): 1,941 original road centre/shoulder samples, maximum change 0.01671m. Original road definitions, gates and flight data are unchanged. This covers surface roads near/over the edited footprints.
- [Preservation audit](preservation.json): six other course scenes and ten protected global runtime sources are byte-identical to the checkpoint. Nineteen original road/gate/flight components in each Backyard scene are unchanged. Four necessary original trunk objects per scene were removed from the jump corridor; affected remaining trees were reseated with their complete crowns.

## Recorded failures and scope

Initial attempts exposed a residual gully wall, protected-road overlap, duplicate visual floor contacts, excessive takeoff grade, a sharp ridge entry climb, and over-fast ridge guidance. These were corrected locally. Earlier traces remain under `initial-driving`, `second-driving`, `third-driving` and `fourth-driving`. The simple custom test pilot also left the drain entry or overturned on the ridge; final technical traversal uses the production game driver. This is not a claim that every approach speed or line succeeds.

Initial recovery waypoints hit existing trunks or aimed straight up the steep ridge side. Final recovery uses measured paths around those trunks; no world geometry was altered to satisfy the test. Failure costs time and navigation. There is no timed full-lap proof of shortcut advantage, exhaustive off-line recovery proof, broad vehicle/weather matrix, or human handling approval. Passing targeted checks is the stop condition.

## Presentation and delivery

Actual saved-geometry atlas, world map and course preview are refreshed for the new routes; optional gold and dashed underground conventions remain. See [atlas](ATLAS.html). Fresh Windows version is `0.55.0-review1`, game build `55000`. Publication/source/launcher and cleanup evidence is recorded separately in `PUBLICATION.md` after those delivery gates complete.

Final visual inspection also sealed the outer gully-mouth gap with buried lining outside the tested bore. This visual-only correction changes no floor, collision or route. Actual final views were inspected; driving testing was not repeated for this visual correction.
