# Mountain Reverse main-route restoration — 0.62.0-review1

Safety checkpoint: clean main `5e11668cb53f3921812c8941b883ecf132574839`.

## Regression and correction

The 0.61 support pass treated upper road edges as unsupported over empty terrain and filled intentional lower-road clearance. Its junction flattening also changed the lower run-up vertices that happened to share the same height range, creating an abrupt exit step. The original validation followed Summit Traverse, not the obstructed lower main road.

- Restored the existing lower Mountain Reverse main route as a continuous mountain-cut/tunnel corridor, including approach and continuation. Its X/Z line, left loop, direction and checkpoint transforms/order are unchanged. Original lower run-up elevations were recovered from the pre-polish `b29330bc` scene and driving mesh. Distance metadata follows the same world-space branch anchors after the grade restoration.
- Upper shoulders use earth embankments, a rock vault and portal outcrops around the lower opening. The reserved opening is approximately 18m wide with over 7m headroom; the bounded floor/forward-clearance audit spans a 12m usable width. No invisible barriers. Eight visible colliding boulders protect selected upper outer shoulders.
- Replaced local unmatched-edge support fins, trimmed terrain at the upper shoulder and removed buried secondary support faces. The latter required clearance below the road too, rather than only above its surface.
- Restored/grounded the Main Route sign, locally affected signs/cairns/complete trees, and raised only CP1's visual crossbar by 3.5m. Its logical checkpoint transform and progression are unchanged.
- Summit Traverse's corrected rejoin coordinates remain exact. Its final heading differs from the existing main direction by 0.34435 degrees; the old U-turn was not restored.
- No changes to existing global AI, vehicle physics, recovery, UI, startup, Race Complete, music, records, other scenes or the launcher. Preview changes reflect the restored lower road elevation only.

## Targeted verification — complete / stopped

- Final motorcycle main-route traversal: station 430 through 962.34, completed; zero resets and zero production-driver recoveries. Maximum airborne interval over the whole approach 0.38s, minimum up-vector Y 0.85.
- Final ATV main-route traversal: station 430 through 962.29, completed; zero resets/recoveries. Maximum airborne interval 0.32s, minimum up-vector Y 0.86.
- These are normal Unity vehicle physics controlled by the existing production RoadDriver. They cover the main left loop, approach, tunnel and restored run-up continuation, and also provide the requested short local AI evidence. No full race or global AI matrix.
- One motorcycle Summit Traverse traversal completed, entered through the existing probabilistic AI branch choice and continued beyond the unchanged rejoin. Zero resets/recoveries; maximum airborne interval 1.28s. Raw maximum branch distance includes pre-entry/post-merge positions, not an edge-excursion measurement.
- Final static corridor audit: 660 floor probes, zero missing floor and zero forward obstruction; upper probes have zero missing/high support. All 9 representative existing recovery `Supported`/full rider-body `Clear` checks pass. No general recovery-system change.
- Actual chase-camera images show both vehicles below the vault and continuing onto the restored grade. Saved scene views document the supported junction, sign and raised gate. No human/controller/subjective visual acceptance is claimed.
- Final grounding evidence and preservation results are separate files in this folder. 176 local trees were inspected; final foot error is recorded in grounding-final.txt. A final visual review retained the original surrounding native hillside meshes, keeping only the required upper-shoulder intersection cut.

## Retained failures and limits

The first motorcycle/ATV lower attempts failed. Above-floor rays missed secondary sloped support faces buried just below pavement; vehicle contacts on those faces caused a launch/roll near the entrance. Removing those faces through 4m below the driving surface fixed both normal-physics retries. The original driving surface remains. Initial geometry probes also exposed the lower-road step and a small upper shoulder overlap, which were corrected before final acceptance.

Initial Editor commands exceeded their response deadline while authoring continued; completion was checked before subsequent operations. A fixture import/recompile guard prevented an early test launch. First screenshot capture used an end-of-frame yield and could delay logging while the Game view was hidden; final lower tests do not depend on that yield. Initial results remain in `initial-runtime` and `runtime`; final accepted main results are in `final-runtime`. The successful Summit result is retained in `initial-runtime/driving-done.txt`.

Do not resume broad Mountain polish. Await Dan's gameplay review after release delivery.
