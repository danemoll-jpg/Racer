# Matched-settings performance

Single visible Windows player at a time; Editor stopped in edit mode; other Racer instances closed. GTX1660Ti, Direct3D11, same Unity6000.6.1f1 runtime,1280×720, VSync off,120fps limit, original player and three original-profile opponents, Normal difficulty, four traffic cars. Both use the existing60-second ordinary-frame GarageValidation sample and isolated settings. Baseline runtime was copied from0.4.0-review1 without changing its saved data/build. Revision is the final release from `c57b6cce912b256de15da74a0e7ef19495bf8b48`.

| Build | Median frame | p95 frame | Peak process working set |
|---|---:|---:|---:|
| 0.4.0-review1 | 8.33ms | 8.35ms | 435.44MiB |
| 0.5.0-review1 | 8.33ms | 8.35ms | 467.57MiB |

Peak working set increased32.13MiB (7.4%). Final sampled resident sets were425.48/457.16MiB. These include runtime/application memory, not a managed-allocation or GPU-memory breakdown. Contact-face caching adds memory; this sample does not isolate its exact share. Process CPU totals were captured at different post-test times and must not be compared as equal-duration CPU measurements.

Both samples meet the120fps cap. This is a frame-budget comparison, not proof of higher uncapped throughput, absence of all hitches or performance on other hardware. AI behavior differs between versions, so route positions after60seconds are not identical. No recovery/missed gate occurred in either60-second sample. Final mixed full-race runs were capped around30fps and ran concurrently; their frame timing is retained but is not substituted for this isolated comparison.

Raw reports: `perf-baseline.txt`, `perf-revision.txt`, corresponding process JSON. `hud-release-driving.png` is captured from this actual committed-source release during ordinary driving.
