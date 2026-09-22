# Forest Loop Reverse hill — 0.29.0-review1

Rollback checkpoint: 2aa186ee90b8e48e7b33a94fdb5768c68141b843.

The supplied screenshot matches Granite Saddle at the opening Forest Loop Reverse fork, near world (483,42,-202), facing west. Commit 14240a16 (CR097 House 3 driveway support) deformed terrain across this existing trail without protecting its elevation profile. Both affected scene-specific assets have been unchanged since that commit; the latest Laurel changes were not the cause.

Only height coordinates in two existing ForestLoopReverse terrain meshes change. The lower approach, climb and crest use a smooth interpolated profile matching the existing crossing. The centreline rises approximately 21.9m from the low approach to the crest over about 92m. Maximum sampled absolute grade across the 8m driving width falls from 4.8606 (78.37 degrees) to 0.3984 (21.72 degrees). Six-metre side blends preserve terrain outside the local corridor and explicitly protect the adjacent main trail. No road, ramp, bypass or structure is added. The existing House 3 driveway is retained.

1,305 targeted collision samples found no gaps or new overlapping surfaces. The driveway retains its original approximately 0.065m surface overlay. Rendering and collision use the same meshes. Terrain tile-edge heights match exactly. 2,393 comparisons against checkpoint terrain confirm unchanged support for the main trail, ambient road and driveway within 1mm. All 2,512 edited vertices retain their x/z coordinates; vertices outside the local corridor are unchanged. Triangulation is retained. Vehicle physics, reset/recovery, AI and route components are untouched. Other scenes do not reference either edited asset.

Gameplay acceptance is left to Dan. No races or vehicle tuning were performed. Before the existing Windows release build, C: had approximately 356 GiB free. Build/publication/launcher/cleanup results are recorded separately after success.

The one-off authoring tool reads original mesh text exported with `git show 2aa186ee:Assets/Track/Discovery/ForestLoopReverse-house3-supported-Ground_560_240.asset` and the corresponding Ground_640_240 path into `Temp/ForestHill-original-Ground_560_240.txt` and `Temp/ForestHill-original-Ground_640_240.txt`. It runs through Unity `run_script`; it is not a runtime system.
