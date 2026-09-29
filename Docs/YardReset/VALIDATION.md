# Rollback and property correction validation

Safety checkpoint: clean `main` at `7ed9adab27fb22a765e24dad6d2eb8d66ab32c6c`, verified before project edits. Source history identifies `777b6242` as the pre-Backyard world and `6b7181fc` as the rejected construction plus accepted Kyle fix.

## Selective rollback

The two new Backyard scenes contained the failed course, cloned terrain grading and cleared tree meshes. Removing those scenes/assets and their selectable course entries restores use of the six established worlds with their original woods. Six accepted Kyle renderer/collider mesh copies remain byte-identical. Rejected-only ordered-gate/pursuit behavior is reverted to the pre-attempt source. Existing saved records remain readable. Saved playlists containing a removed course remain editable and receive a clear message instead of attempting to load a deleted scene.

[Serialized preservation checks](preservation.json) verify every existing object remains in the six established scenes, with changes confined to local property supporting meshes, affected fence endpoint/collider adjustments, the existing mailbox position and the new marker root. Other route/navigation data, tree/trunk objects, buildings, accepted Kyle meshes and original shared mesh assets are preserved. No new route connects the anchors.

## Property and dependencies

- Existing beige concrete horizontal footprint preserved. Start-to-hill elevation is smooth, the intended hill starts near X=422.7, and the lower parking/apron is flat from X≈409.5 through X≈391.9. Actual lower reference pavement is Y=79.45641 in Street scenes; other worlds use their own measured lower pavement, approximately Y=79.45346. Vehicle HUD Y includes clearance and is not substituted for ground height.
- Local terrain copies support the concrete, with a smooth shoulder blend and preserved building foundation interiors. Garage/kennel, pool house and main house are not relocated. Existing short dirt threshold is seated without extension into woods.
- 36 affected fence sections per world are refitted by endpoint; their visible meshes and trigger bounds are updated together. Existing entrance fence openings remain; no new race gates, start/finish infrastructure or signs are created.
- One existing mailbox per world moves from (463.448,85.989,-26.849) to approximately (466.3,80.391,2.0), on the south/left side when looking west into the main entrance. Moving the whole prefab leaves no original post/base behind and adds no duplicate.
- Eight exact X/Z markers per world, no colliders/race components. Single camera-facing development labels remain visible through foliage without clearing trees. Pink poles and labels are temporary validation aids.

See [exact authoring results](authoring.txt), [geometry checks](checks.txt), [entrance drive](entrance-drive.txt), and [atlas](ATLAS.md).

## Retained findings and limits

The first authoring attempt stopped before terrain changes because an exact boundary ray fell outside the old driveway collider; sampling just inside its edge corrected the cause. An initial saved-playlist guard needed the List.Exists API to compile. The first rendered inspection caught lost vertex colors and overlapping four-way text. Original terrain colors were interpolated from the unchanged historical mesh and vertices welded; text was replaced by a single camera-facing label with the existing font material so references remain visible through foliage. These are authoring/presentation corrections, not gameplay tuning.

Dan's referenced `front gate to my house.png` was not included in the supplied attachment directory and was not found under that name in Downloads. The actual existing scene geometry, previous project reference views and his explicit coordinates/description supplied the inspection basis.

No broad race, vehicle, AI, recovery, audio or all-track gameplay matrix is performed. Ordinary tests are muted. Dan's visual/driving acceptance and approval of the eight anchors remain pending. Forward construction is not authorized by this pass.

Publication/build/launcher results are recorded separately in PUBLICATION.md after delivery.

## Final bounded result

All 22 recorded geometry/marker/playlist checks pass. Parking vertex min/max both 79.45641; 439 terrain support samples span 0.02897–0.04288m below concrete. Garage corner maximum gap 0.01136m; pool house effectively zero. One muted 0.02-second-step motorcycle entrance crossing passed at an 8m/s approach, minimum speed 7.921m/s and maximum continuous air 0.020s (initial settling frame). Final entrance, parking and marker views were visually inspected. Testing stops here; no broader matrix is authorized.
