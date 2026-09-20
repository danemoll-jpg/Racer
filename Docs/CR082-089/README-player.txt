Woodstock Rush 0.14.0-review2 / Windows x64 correction review
Internal Racer file and save paths are preserved. No external upload.

Play-Racer.cmd launches the complete Builds/Latest runtime.
Choose Street, Forest, Street Reverse or Forest Reverse. Forest vehicles remain
motorcycle and ATV. Free Roam remains available with existing traffic/world systems.

This correction pass adds helmet-free people with faces, hair and separate clothing
colors; one shared household visit across both properties; continuous radio music
when changing courses; vehicle-body overlap at gate edges; and imperial displays.
Speed uses mph, travel distances use miles/feet, and jump distances use feet.
Canonical physics, saved measurements and medal comparisons are unchanged.

Reverse Street starts at its actual grid/finish. The Trickum launch gate is retired;
one required crossing sits beyond landing. Optional shortcut signs face the active
direction, and complete trees replace previously clipped canopy fragments.
Forest Reverse's grid and finish now use the supported lead-in nearer the first
junction, with physical boards marking the main bend and optional shortcut.

Trickum: reproduced reverse contact snags were corrected using authored supporting
face normals, including sweep CCD. The reverse ramp has gentler side shoulders.
No launch boost or global collision disable was added. Extreme/off-line crashes
and forward-layout slowdown uncertainty are retained in the validation report.

Esc / Start pauses. R / Y recovers locally and preserves earned race progress.
Genuine missed required gates still cost exactly five seconds once. A legitimate
vehicle-body overlap at an edge is accepted, with 7.9 inches of additional tolerance.
Free-roam activity attempts, clean landing requirements, scoring, water behavior,
speed traps, wildlife/audio and top-ten records remain available.

Radio: M / D-pad Down changes channel and Off; [ / ] or D-pad Left/Right changes song;
I / D-pad Up shows title while driving. Menu navigation does not act as radio input.
Stage selected music in BundleMusic and run Package-Racer.cmd to repackage.

New timing categories preserve historical saves. Unchanged stunt categories keep
their existing personal bests; the revised reverse ramp uses a new activity category.
See Docs/CR082-089/VALIDATION.md for actual results and retained failures.
Automated tests do not establish human visual acceptance, listening quality,
physical-controller behavior or driving enjoyment. Dan's review remains required.
CR-081 ghosts, collectibles, neighborhood expansion and home work remain approved
and queued after this correction pass. Split screen/networking are not included.

