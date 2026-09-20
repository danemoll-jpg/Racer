# Compiled-player results: release2

Automated evidence only. Concurrent hidden players are not performance benchmarks, human driving, physical-controller testing or listening. A successful process exit does not override failed assertions or ramp outcomes.

| Suite | Passed / total | Completed |
| --- | ---: | --- |
| ai-ForestLoopReverse | 6 / 6 | True |
| ai-LakeWoods | 6 / 6 | True |
| ai-StreetLoopGreybox | 11 / 12 | True |
| ai-StreetLoopReverse | 12 / 12 | True |
| art | 116 / 116 | True |
| departure | 12 / 12 | True |
| estimates | 96 / 96 | True |
| forest-scoring-ForestLoopReverse | 6 / 6 | True |
| forest-scoring-LakeWoods | 6 / 6 | True |
| guidance-ForestLoopReverse | 6 / 6 | True |
| guidance-LakeWoods | 6 / 6 | True |
| guidance-StreetLoopGreybox | 6 / 6 | True |
| guidance-StreetLoopReverse | 6 / 6 | True |
| race-ForestLoopReverse | 6 / 6 | True |
| race-LakeWoods | 6 / 6 | True |
| race-StreetLoopGreybox | 6 / 6 | True |
| race-StreetLoopReverse | 6 / 6 | True |
| radio | 41 / 41 | True |
| records | 29 / 29 | True |
| recovery-ForestLoopReverse | 46 / 46 | True |
| recovery-LakeWoods | 44 / 44 | True |
| recovery-StreetLoopGreybox | 100 / 100 | True |
| recovery-StreetLoopReverse | 92 / 92 | True |
| roam-recovery-ForestLoopReverse | 16 / 16 | True |
| roam-recovery-LakeWoods | 16 / 16 | True |
| roam-recovery-StreetLoopGreybox | 16 / 16 | True |
| roam-recovery-StreetLoopReverse | 32 / 32 | True |
| rules-ForestLoopReverse | 24 / 24 | True |
| rules-LakeWoods | 24 / 24 | True |
| rules-StreetLoopGreybox | 24 / 24 | True |
| rules-StreetLoopReverse | 24 / 24 | True |
| smash-ForestLoopReverse | 4 / 4 | True |
| smash-LakeWoods | 4 / 4 | True |
| smash-StreetLoopGreybox | 8 / 8 | True |
| smash-StreetLoopReverse | 8 / 8 | True |
| speed-ForestLoopReverse | 24 / 24 | True |
| speed-LakeWoods | 24 / 24 | True |
| speed-StreetLoopGreybox | 48 / 48 | True |
| speed-StreetLoopReverse | 48 / 48 | True |
| systems | 474 / 474 | True |

## Ramp matrix

The probe uses canonical m/s. 12/24/36/43 m/s = 26.8/53.7/80.5/96.2 mph. Completed does not guarantee a clean traversal: upright below 0.65 is separately counted. Raw traces retain inputs, contact normals/separation/impulse, suspension, velocity components and stability.

| Matrix | Runs | Non-completed/unstable final | Upright < 0.65 | Recovery failures | Completed |
| --- | ---: | ---: | ---: | ---: | --- |
| high-ForestLoopReverse-roam-no | 10 | 1 | 4 | 0 | True |
| high-ForestLoopReverse-roam-yes | 10 | 0 | 3 | 0 | True |
| high-LakeWoods-roam-no | 10 | 0 | 4 | 0 | True |
| high-LakeWoods-roam-yes | 10 | 0 | 3 | 0 | True |
| high-StreetLoopGreybox-race-no | 20 | 2 | 8 | 0 | True |
| high-StreetLoopGreybox-roam-no | 20 | 1 | 8 | 0 | True |
| high-StreetLoopGreybox-roam-yes | 20 | 1 | 6 | 0 | True |
| high-StreetLoopReverse-race-no | 20 | 0 | 0 | 0 | True |
| high-StreetLoopReverse-roam-no | 20 | 0 | 0 | 0 | True |
| high-StreetLoopReverse-roam-yes | 20 | 0 | 0 | 0 | True |
| outer-StreetLoopReverse-race-no--10.3 | 4 | 0 | 0 | 0 | True |
| outer-StreetLoopReverse-race-no-7.3 | 4 | 0 | 0 | 0 | True |
| outer-StreetLoopReverse-roam-no--10.3 | 4 | 0 | 0 | 0 | True |
| outer-StreetLoopReverse-roam-no-7.3 | 4 | 0 | 0 | 0 | True |
| outer-StreetLoopReverse-roam-yes--10.3 | 4 | 0 | 1 | 0 | True |
| outer-StreetLoopReverse-roam-yes-7.3 | 4 | 0 | 0 | 0 | True |
| ramp-ForestLoopReverse-roam-no | 30 | 1 | 11 | 0 | True |
| ramp-ForestLoopReverse-roam-yes | 30 | 0 | 9 | 0 | True |
| ramp-LakeWoods-roam-no | 30 | 1 | 11 | 0 | True |
| ramp-LakeWoods-roam-yes | 30 | 1 | 9 | 0 | True |
| ramp-StreetLoopGreybox-race-no | 60 | 2 | 23 | 0 | True |
| ramp-StreetLoopGreybox-roam-no | 60 | 2 | 23 | 0 | True |
| ramp-StreetLoopGreybox-roam-yes | 60 | 1 | 18 | 0 | True |
| ramp-StreetLoopReverse-race-no | 60 | 0 | 0 | 0 | True |
| ramp-StreetLoopReverse-roam-no | 60 | 0 | 0 | 0 | True |
| ramp-StreetLoopReverse-roam-yes | 60 | 0 | 0 | 0 | True |

Total recorded ramp traversals: 664. Explicit non-clean cases retained: 143.

## Failed assertions retained

- ai-StreetLoopGreybox: FAIL tourer persistent partial obstruction cleared without recovery loop; recoveries=2 seconds=35.00
