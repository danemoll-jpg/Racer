# Local track and global recovery correction — 0.25.0-review1

Initial rollback checkpoint: `283c918a`, "Checkpoint project before localized
Laurel and recovery corrections". Intermediate work was also preserved in
`398f1ace` before re-authoring the final Laurel takeoff/landing connection.

## Scope and implementation

The two older screenshots refer to Street Loop Reverse / Laurel Pass. The
first-arrow screenshot refers to Mountain Loop Reverse. Only those two scenes
are authored. Mountain Forward and all other track geometry are unchanged.

Laurel's earlier CR122/CR133 repair targeted a driveway crossing and produced
an elevated route with a long detour and unsupported spans. Those repair roots
are removed, rather than hidden beneath another road. The replacement follows
the local valley and has earth embankments joined into the terrain. Its final
30m approach and 25m ramp share one horizontal axis aimed at the main road.
The ramp rises 0.65m with a quadratic profile and uses the same mesh for display
and collision. A small supported landing shoulder replaces the existing surface
within its footprint. The property entrance sign is moved outside the runway.
No house, vehicle tuning, global physics or speed boost is changed.

The first Mountain Reverse arrow already had no collider. Its supporting road
had a shallow dip and coarse changes in face normals. Only the original road
station interval 35–100 is replaced with a smooth visible/collidable surface,
with adjacent shoulder heights matched locally and a 14m exit feather. The arrow is seated on the
surface and remains non-colliding. This is a technical correction to a plausible
bounce source, not a claim of reproducing the user's motorcycle event.

Recovery previously required three suspension contacts, world-up alignment above
0.9 and one uninterrupted second of stability. Ordinary two-contact driving,
slopes and cornering could repeatedly restart that timer. Recovery now samples
every 0.1s, requires at least two contacts plus a supported full footprint,
alignment with the actual road normal, clear vehicle bounds, forward travel,
0.35s stability and 1m travelled. It retains the occupied lateral road position.
Mountain/Laurel metadata, legacy launch surfaces and Forest run-ups are excluded; airborne, overturned,
off-route and obstructed samples cannot replace the anchor. Projection height
offsets are removed so uphill chassis height cannot manufacture forward distance.

Successful landing clears pre-jump history when its first valid sample is
accepted. Recovery tries the latest occupied sample before older clear samples.
Navigation/branch completion seeds tracking only; it no longer invents an
unchecked safe landing. R/Y and automatic fall recovery queue 0.8s, with lost
momentum after reset. AI's existing direct recovery API remains available.

## Targeted verification

See `recovery-checks.txt` for deterministic fixtures across five scenes, all four
Mountain flights and Laurel. They check frequent grounded advancement, two-contact
support, ramp/airborne rejection, post-jump advancement, retired pre-jump history,
actual recent-anchor placement, no gate credit, and the short delay.

See `Laurel-checks.txt` for cross-lane ramp surface/collider continuity and the
unboosted geometric landing envelope. Static images review the route and landing.
These are targeted technical checks, not a full-race or physical gameplay claim.
Earlier failed diagnostic reports are retained alongside the final results so the
sign collision and takeoff-edge correction are traceable.

The existing Windows build script, package verifier, publisher signing key,
component manifests, GitHub publisher and native launcher verification are used.
Publication evidence is added after the release is uploaded and checked.

## Final technical results

- Laurel: 663 runway lane samples, 0.16mm maximum profile error, 0.335-degree maximum adjacent normal change, no stacked colliders.
- Full local Laurel route: 1,347 samples, maximum profile error 10.3mm; no blocking road-surface colliders.
- Existing-speed geometric flights at 18, 24, 32 and 38m/s intersect the main-road landing surface.
- Five-scene recovery fixtures pass, including the Forest landing runout, four Mountain flights and Laurel. No full races run.
- First-arrow five-lane scan: maximum adjacent normal change reduced from 2.114 to 1.686 degrees; the arrow sits on the new flat approach instead of the old dip. Exact rider-reported event remains a human gameplay check.
- Global vehicle physics and Mountain Forward scene are byte-unchanged from the initial checkpoint.
