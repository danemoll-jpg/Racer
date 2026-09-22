# Simple Laurel ground ramp — 0.27.0-review1

Checkpoint: e88f23e622aa4f4c0e5e9912a11fe7543323be3c.

The reference image was located in Downloads and matched to the original dirt ridge near (340, 69.52, -190). Recent commit 867f68d8 introduced the long curved shortcut strip and raised terminal road-facing ramp. Its 21 meshes (driving strip, side support and 19 visual arrows) are removed. Four terrain references cut beneath that strip are restored to the recorded pre-regression assets from 489b220b. The older property driveway is preserved; it predates these shortcut attempts.

One existing near-side terrain mesh is locally reshaped. The straight axis is normalized (1, 0, 0.65); the usable top is 8m wide, with a 26m rising section and a short entry blend. Its rise is 10.92m and terminal grade is 0.72. Side feathering joins existing ground; the final 2m drops down the near cliff. No geometry crosses the chasm. Rendering and collision use the same mesh. There is no bridge, receiving platform, additional road or far-side redesign.

Targeted verification: 1,405 ramp samples with zero gaps or stacked top colliders; maximum adjacent normal change 0.692 degrees; maximum profile error 0.00073m. The outer side seam matches baseline terrain within 0.000076m. Both normal Street Loop routes pass 6,805 local samples each with zero missing support and at most 0.065m existing surface detail relative to the centerline.

Basic gravity-only trajectories at 32 and 38m/s reach existing terrain across the chasm. These are geometric checks, not vehicle playtests or a guarantee for every speed. No full races or subjective jump tuning were performed.

All retained scene MonoBehaviours, including route metadata and recovery exclusions, are unchanged. Recovery/reset and global vehicle-physics source are unchanged. Street Forward and every other race scene are unchanged. The far side receives no new or reshaped road, terrain or landing geometry; only the previous shortcut's removed terrain footprint is restored.

The existing Windows build, package verifier, publisher signing identity, component publisher, public updater and Play-Racer startup verification are used. Publication evidence is recorded separately after successful delivery.
