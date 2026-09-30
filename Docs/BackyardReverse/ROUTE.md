# Constructed Backyard Reverse coordinates

Unity metres; Y values are final authored geometry, not the original driven vehicle-centre heights. Gate positions below are gate transforms (1.5m above supporting terrain).

Course ID: `backyard-reverse-v1-main`. Main route length: 1257.47m. No optional Reverse branches.

| Feature | Launch lip | Supported receiving corridor |
|---|---|---|
| North gully westbound | (128.00, 61.20, 80.70) | (55.00, 42.94, 80.70) |
| South gully eastbound | (72.40, 44.07, -94.10) | (128.00, 42.18, -94.10) |
| Pool-house flight east-northeast | (387.94, 88.69, -3.31) | (439.00, 81.73, 6.90) |

Measured touchdown differs with speed and vehicle. Technical motorcycle/ATV runs landed near (50, 43, 80), (151, 45, -95) and (457, 80, 12). The receiving corridor continues through these touchdown areas. The pool-house ramp passes near the supplied (378.6, -9.8) reference; its final lip is farther along the gradual run-up to avoid a premature crest.

| Gate | Position |
|---|---|
| Reverse START FINISH | (472.88, 78.50, 33.85) |
| Reverse blue CP 1 | (398.00, 83.24, 80.00) |
| Reverse blue CP 2 | (186.00, 69.51, 80.70) |
| Reverse blue CP 3 | (36.00, 39.83, 19.00) |
| Reverse blue CP 4 | (29.96, 38.25, -94.54) |
| Reverse blue CP 5 | (170.04, 47.41, -96.82) |
| Reverse blue CP 6 | (265.98, 67.64, -23.54) |
| Reverse blue CP 7 | (330.24, 79.73, -14.62) |

AI uses the actual Reverse main RaceRoad and existing Backyard flight alignment metadata / ForestLayout approach metadata. No global difficulty, physics, shortcut probability, recovery or minimap code was changed. Gates avoid the lip, flight and immediate touchdown regions.

Current local correction evidence: [0.47 validation](../BackyardReverseCorrections/VALIDATION.md). Pool-house flight metadata is on the measured supported lip; physical flight tests precede only this metadata correction.
