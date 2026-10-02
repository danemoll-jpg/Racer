# 0.68 Part F — redundant ground arrows

Dan, 2026-10-02: the game "goes a little crazy with the arrows"; redundant arrows may be removed, and any single removal can be asked back.

**Rules applied** (`Tools/Report068/Report068Arrows.cs`; dry run reviewed first, then applied):

- Removed: arrows on no route of the course (inherited from another course, e.g. the 72 Street Loop Route Atlas arrows in each Backyard scene, or floating over an inactive old runway); arrows pointing backwards for every route they lie on (none found); duplicates of another arrow for the same instruction within 12 m (the Route Atlas arrow is kept over older copies); repeated arrows on stretches with no turn (> 20° within 40 m) and no fork (40 m before to 80 m after), keeping one reassurance arrow per 150 m.
- Kept: every arrow before a turn, fork or junction; every gold shortcut arrow on the main road; every Route Atlas fork/rejoin arrow; Dan's 0.67 BUG-020 straight arrow (Backyard Forward, 70.5, 45.1, 61.1).
- Kept arrows more than 0.2 m off the surface were reseated flat on it (0.67 BUG-006 rule); see below.

Signs, gates, minimap, route lines and wrong-way guidance are unchanged.

## Counts

| Course / direction | Before | After | Removed |
|---|---:|---:|---:|
| Street Loop — Forward | 73 | 55 | 18 |
| Street Loop — Reverse | 54 | 40 | 14 |
| Forest Loop — Forward | 27 | 18 | 9 |
| Forest Loop — Reverse | 35 | 26 | 9 |
| Dan's Backyard Loop — Forward | 100 | 27 | 73 |
| Dan's Backyard Loop — Reverse | 106 | 29 | 77 |
| Mountain Loop — Forward | 71 | 47 | 24 |
| Mountain Loop — Reverse | 72 | 60 | 12 |
| **All courses** | **538** | **302** | **236** |

## Reseated kept arrows

- Forest Loop Reverse: 'Optional gold / House 3 Detour' (313.7, 46.1, -247.2).
- Backyard Forward: 'Main teal trail arrow' (205.9, 73.0, 98.3), corrected 0.51 m.
- Backyard Reverse: 'Reverse teal ground arrow' (304.4, 76.7, 102.6), 0.65 m, and (280.3, 76.3, 119.8), 0.52 m.
- Mountain Reverse: 'CR122 crossing gold arrow' (698.4, 80.1, -81.5), 0.99 m, and (707.3, 79.2, -70.7), 1.09 m (were floating over Downhill Ridge Cut).
- Mountain Reverse (BUG-005): 'Main teal / turn' (747, 85.8, -114.5), reseated after the crease was smoothed.

## Restoring one arrow

Each removed arrow is listed below with its object path and mesh asset. To bring one back, restore that GameObject from the 0.67 scene (commit `abb1652d`), or ask for it by coordinates. The full per-arrow classification (kept and removed, with reasons) is in `Docs/Report068/arrows/arrows-<scene>.csv`.

## Removed arrows by course

### Street Loop — Forward — removed 18

| Route | Station | x | y | z | Heading | Reason | Object | Mesh |
|---|---:|---:|---:|---:|---:|---|---|---|
| Fox Gully | 260.0 | 39.67 | 23.32 | -447.71 | -139 | repeated on straight (130 m after kept arrow) | Route atlas direction guidance/Optional gold / Fox Gully | StreetLoopGreybox-arrow-066 |
| Main | 125.0 | 318.64 | 36.94 | 429.21 | -180 | repeated on straight (100 m after kept arrow) | Route atlas direction guidance/Main teal / reassurance | StreetLoopGreybox-arrow-027 |
| Main | 525.0 | 459.93 | 85.32 | 91.89 | 159 | repeated on straight (113 m after kept arrow) | Route atlas direction guidance/Main teal / reassurance | StreetLoopGreybox-arrow-030 |
| Main | 725.0 | 498.74 | 87.83 | -99.58 | 152 | repeated on straight (100 m after kept arrow) | Route atlas direction guidance/Main teal / reassurance | StreetLoopGreybox-arrow-032 |
| Main | 1025.0 | 544.40 | 25.78 | -374.36 | -175 | repeated on straight (103 m after kept arrow) | Route atlas direction guidance/Main teal / reassurance | StreetLoopGreybox-arrow-034 |
| Main | 1625.0 | 182.16 | 44.34 | -311.87 | -66 | repeated on straight (23 m after kept arrow) | Route atlas direction guidance/Main teal / reassurance | StreetLoopGreybox-arrow-037 |
| Main | 1825.0 | 15.55 | 37.45 | -336.00 | -162 | repeated on straight (38 m after kept arrow) | Route atlas direction guidance/Main teal / reassurance | StreetLoopGreybox-arrow-038 |
| Main | 1925.0 | -1.98 | 32.43 | -433.52 | -176 | repeated on straight (138 m after kept arrow) | Route atlas direction guidance/Main teal / reassurance | StreetLoopGreybox-arrow-039 |
| Main | 2325.0 | -283.04 | 20.03 | -573.42 | -81 | repeated on straight (83 m after kept arrow) | Route atlas direction guidance/Main teal / reassurance | StreetLoopGreybox-arrow-041 |
| Main | 2825.0 | -621.56 | 6.84 | -380.16 | -1 | repeated on straight (46 m after kept arrow) | Route atlas direction guidance/Main teal / reassurance | StreetLoopGreybox-arrow-045 |
| Main | 3125.0 | -626.60 | 7.98 | -80.21 | -1 | repeated on straight (101 m after kept arrow) | Route atlas direction guidance/Main teal / reassurance | StreetLoopGreybox-arrow-047 |
| Main | 3325.0 | -630.14 | 8.79 | 119.76 | -1 | repeated on straight (100 m after kept arrow) | Route atlas direction guidance/Main teal / reassurance | StreetLoopGreybox-arrow-049 |
| Main | 3525.0 | -633.09 | 9.20 | 319.74 | -1 | repeated on straight (100 m after kept arrow) | Route atlas direction guidance/Main teal / reassurance | StreetLoopGreybox-arrow-051 |
| Main | 3825.0 | -521.84 | 8.02 | 539.37 | 94 | repeated on straight (88 m after kept arrow) | Route atlas direction guidance/Main teal / reassurance | StreetLoopGreybox-arrow-053 |
| Main | 4025.0 | -322.01 | 8.96 | 531.45 | 92 | repeated on straight (100 m after kept arrow) | Route atlas direction guidance/Main teal / reassurance | StreetLoopGreybox-arrow-055 |
| Main | 4225.0 | -122.04 | 8.25 | 529.09 | 90 | repeated on straight (100 m after kept arrow) | Route atlas direction guidance/Main teal / reassurance | StreetLoopGreybox-arrow-057 |
| Main | 4425.0 | 76.37 | 9.05 | 541.88 | 82 | repeated on straight (100 m after kept arrow) | Route atlas direction guidance/Main teal / reassurance | StreetLoopGreybox-arrow-059 |
| Pine Ridge | 260.0 | -385.95 | 20.97 | -441.61 | -43 | repeated on straight (130 m after kept arrow) | Route atlas direction guidance/Optional gold / Pine Ridge | StreetLoopGreybox-arrow-070 |

### Street Loop — Reverse — removed 14

| Route | Station | x | y | z | Heading | Reason | Object | Mesh |
|---|---:|---:|---:|---:|---:|---|---|---|
| Granite Creek Cut | 130.0 | -125.41 | 11.63 | 413.73 | -151 | repeated on straight (101 m after kept arrow) | Route atlas direction guidance/Optional gold / Granite Creek Cut | StreetLoopReverse-arrow-050 |
| Main | 125.0 | 200.89 | 9.14 | 553.69 | -98 | repeated on straight (113 m after kept arrow) | Route atlas direction guidance/Main teal / reassurance | StreetLoopReverse-arrow-018 |
| Main | 525.0 | -196.89 | 8.78 | 529.26 | -90 | repeated on straight (99 m after kept arrow) | Route atlas direction guidance/Main teal / reassurance | StreetLoopReverse-arrow-021 |
| Main | 725.0 | -396.82 | 8.51 | 534.13 | -88 | repeated on straight (100 m after kept arrow) | Route atlas direction guidance/Main teal / reassurance | StreetLoopReverse-arrow-023 |
| Main | 1025.0 | -625.60 | 8.78 | 444.41 | -168 | repeated on straight (88 m after kept arrow) | Route atlas direction guidance/Main teal / reassurance | StreetLoopReverse-arrow-025 |
| Main | 1225.0 | -631.88 | 9.14 | 244.89 | 179 | repeated on straight (100 m after kept arrow) | Route atlas direction guidance/Main teal / reassurance | StreetLoopReverse-arrow-027 |
| Main | 1525.0 | -627.00 | 8.07 | -55.07 | 179 | repeated on straight (57 m after kept arrow) | Route atlas direction guidance/Main teal / reassurance | StreetLoopReverse-arrow-030 |
| Main | 1725.0 | -624.05 | 7.37 | -255.04 | 179 | repeated on straight (100 m after kept arrow) | Route atlas direction guidance/Main teal / reassurance | StreetLoopReverse-arrow-032 |
| Main | 2125.0 | -505.75 | 6.98 | -549.66 | 91 | repeated on straight (113 m after kept arrow) | Route atlas direction guidance/Main teal / reassurance | StreetLoopReverse-arrow-035 |
| Main | 2325.0 | -307.44 | 15.39 | -569.47 | 99 | repeated on straight (100 m after kept arrow) | Route atlas direction guidance/Main teal / reassurance | StreetLoopReverse-arrow-037 |
| Main | 2725.0 | -3.73 | 31.06 | -458.55 | 2 | repeated on straight (113 m after kept arrow) | Route atlas direction guidance/Main teal / reassurance | StreetLoopReverse-arrow-040 |
| Main | 3325.0 | 411.25 | 37.90 | -436.68 | 143 | repeated on straight (63 m after kept arrow) | Route atlas direction guidance/Main teal / reassurance | StreetLoopReverse-arrow-042 |
| Main | 3525.0 | 526.46 | 27.48 | -496.74 | 21 | repeated on straight (63 m after kept arrow) | Route atlas direction guidance/Main teal / reassurance | StreetLoopReverse-arrow-043 |
| Main | 4425.0 | 320.61 | 69.44 | 308.36 | -3 | repeated on straight (113 m after kept arrow) | Route atlas direction guidance/Main teal / reassurance | StreetLoopReverse-arrow-046 |

### Forest Loop — Forward — removed 9

| Route | Station | x | y | z | Heading | Reason | Object | Mesh |
|---|---:|---:|---:|---:|---:|---|---|---|
| Echo Cave | 130.0 | 116.82 | 43.25 | 209.75 | -149 | repeated on straight (60 m after kept arrow) | Route atlas direction guidance/Optional gold / Echo Cave | LakeWoods-arrow-025 |
| Main | 225.0 | 501.71 | 79.24 | 157.71 | -58 | repeated on straight (88 m after kept arrow) | Route atlas direction guidance/Main teal / reassurance | LakeWoods-arrow-007 |
| Main | 425.0 | 330.99 | 80.59 | 252.33 | -60 | repeated on straight (100 m after kept arrow) | Route atlas direction guidance/Main teal / reassurance | LakeWoods-arrow-009 |
| Main | 625.0 | 146.94 | 45.80 | 284.58 | -104 | repeated on straight (38 m after kept arrow) | Route atlas direction guidance/Main teal / reassurance | LakeWoods-arrow-010 |
| Main | 725.0 | 59.65 | 34.31 | 252.41 | -116 | repeated on straight (138 m after kept arrow) | Route atlas direction guidance/Main teal / reassurance | LakeWoods-arrow-011 |
| Main | 925.0 | -94.95 | 23.24 | 130.02 | -150 | repeated on straight (100 m after kept arrow) | Route atlas direction guidance/Main teal / reassurance | LakeWoods-arrow-013 |
| Main | 1325.0 | 57.65 | 31.36 | -140.55 | 99 | repeated on straight (51 m after kept arrow) | Route atlas direction guidance/Main teal / reassurance | LakeWoods-arrow-017 |
| Main | 1525.0 | 239.48 | 46.57 | -197.43 | 124 | repeated on straight (100 m after kept arrow) | Route atlas direction guidance/Main teal / reassurance | LakeWoods-arrow-019 |
| Main | 1725.0 | 407.87 | 54.93 | -290.02 | 90 | repeated on straight (100 m after kept arrow) | Route atlas direction guidance/Main teal / reassurance | LakeWoods-arrow-021 |

### Forest Loop — Reverse — removed 9

| Route | Station | x | y | z | Heading | Reason | Object | Mesh |
|---|---:|---:|---:|---:|---:|---|---|---|
| Fern Gully | 130.0 | 129.94 | 36.87 | -73.32 | -32 | repeated on straight (87 m after kept arrow) | Route atlas direction guidance/Optional gold / Fern Gully | ForestLoopReverse-arrow-027 |
| Main | 325.0 | 447.23 | 68.02 | -199.21 | -85 | repeated on straight (113 m after kept arrow) | Route atlas direction guidance/Main teal / reassurance | ramp-arrow-0 |
| Main | 741.0 | 77.84 | 30.98 | -144.48 | -77 | repeated on straight (109 m after kept arrow) | Route atlas direction guidance/Main teal / reassurance | ForestLoopReverse-arrow-013 |
| Main | 941.0 | -100.25 | 21.00 | -72.56 | -33 | repeated on straight (100 m after kept arrow) | Route atlas direction guidance/Main teal / reassurance | ForestLoopReverse-arrow-015 |
| Main | 1141.0 | -102.47 | 20.71 | 115.99 | 26 | repeated on straight (100 m after kept arrow) | Route atlas direction guidance/Main teal / reassurance | ForestLoopReverse-arrow-017 |
| Main | 1441.0 | 134.52 | 43.03 | 281.21 | 74 | repeated on straight (86 m after kept arrow) | Route atlas direction guidance/Main teal / reassurance | ForestLoopReverse-arrow-019 |
| Main | 1641.0 | 320.23 | 78.72 | 258.80 | 121 | repeated on straight (100 m after kept arrow) | Route atlas direction guidance/Main teal / reassurance | ForestLoopReverse-arrow-021 |
| McFadden Cut | 110.0 | 534.70 | 32.43 | -247.25 | -123 | repeated on straight (47 m after kept arrow) | House 3 Detour optional visual guidance/Optional gold / House 3 Detour | detour-arrow-1 |
| McFadden Cut | 270.0 | 383.52 | 53.52 | -286.25 | -72 | repeated on straight (80 m after kept arrow) | House 3 Detour optional visual guidance/Optional gold / House 3 Detour | detour-arrow-3 |

### Dan's Backyard Loop — Forward — removed 73

| Route | Station | x | y | z | Heading | Reason | Object | Mesh |
|---|---:|---:|---:|---:|---:|---|---|---|
| — | 0.0 | -22.04 | 7.95 | 528.82 | 90 | not on this course | Route atlas direction guidance/Main teal / reassurance | StreetLoopGreybox-arrow-058 |
| — | 0.0 | 318.64 | 36.94 | 429.21 | -180 | not on this course | Route atlas direction guidance/Main teal / reassurance | StreetLoopGreybox-arrow-027 |
| — | 0.0 | -535.48 | 6.69 | -550.21 | -92 | not on this course | Route atlas direction guidance/Main teal / before Existing Southwest Cut fork | StreetLoopGreybox-arrow-003 |
| — | 0.0 | 490.40 | 22.67 | -544.66 | -100 | not on this course | Route atlas direction guidance/Main teal / turn | StreetLoopGreybox-arrow-015 |
| — | 0.0 | -222.04 | 8.92 | 529.45 | 91 | not on this course | Route atlas direction guidance/Main teal / reassurance | StreetLoopGreybox-arrow-056 |
| — | 0.0 | 175.99 | 9.02 | 550.19 | 83 | not on this course | Route atlas direction guidance/Main teal / reassurance | StreetLoopGreybox-arrow-060 |
| — | 0.0 | 425.73 | 35.74 | -457.12 | -34 | not on this course | Route atlas direction guidance/Main teal / reassurance | StreetLoopGreybox-arrow-036 |
| — | 0.0 | -480.64 | 7.07 | -550.82 | -86 | not on this course | Route atlas direction guidance/Main teal / reassurance | StreetLoopGreybox-arrow-043 |
| — | 0.0 | -581.44 | 6.21 | -551.64 | -88 | not on this course | Route atlas direction guidance/Main teal / main at Existing Southwest Cut fork | StreetLoopGreybox-arrow-004 |
| — | 0.0 | 509.20 | 23.05 | -528.82 | -143 | not on this course | Route atlas direction guidance/Main teal / turn | StreetLoopGreybox-arrow-014 |
| — | 0.0 | 39.67 | 23.32 | -447.71 | -139 | not on this course | Route atlas direction guidance/Optional gold / Fox Gully | StreetLoopGreybox-arrow-066 |
| — | 0.0 | 534.02 | 31.43 | -473.10 | -165 | not on this course | Route atlas direction guidance/Main teal / reassurance | StreetLoopGreybox-arrow-035 |
| — | 0.0 | 76.37 | 9.05 | 541.88 | 82 | not on this course | Route atlas direction guidance/Main teal / reassurance | StreetLoopGreybox-arrow-059 |
| — | 0.0 | 332.64 | 81.86 | 235.91 | 162 | not on this course | Route atlas direction guidance/Main teal / reassurance | StreetLoopGreybox-arrow-029 |
| — | 0.0 | -385.95 | 20.97 | -441.61 | -43 | not on this course | Route atlas direction guidance/Optional gold / Pine Ridge | StreetLoopGreybox-arrow-070 |
| — | 0.0 | 320.11 | 11.70 | 525.92 | -179 | not on this course | Route atlas direction guidance/Main teal / reassurance | StreetLoopGreybox-arrow-026 |
| — | 0.0 | -625.16 | 7.63 | -181.19 | -1 | not on this course | Route atlas direction guidance/Main teal / Pine Ridge rejoin | StreetLoopGreybox-arrow-011 |
| — | 0.0 | -477.47 | 15.97 | -349.38 | -49 | not on this course | Route atlas direction guidance/Optional gold / Pine Ridge | StreetLoopGreybox-arrow-071 |
| — | 0.0 | -618.95 | 6.29 | -480.12 | -2 | not on this course | Route atlas direction guidance/Main teal / reassurance | StreetLoopGreybox-arrow-044 |
| — | 0.0 | -50.33 | 28.12 | -541.39 | -142 | not on this course | Route atlas direction guidance/Optional gold / Fox Gully | StreetLoopGreybox-arrow-067 |
| — | 0.0 | -629.70 | 8.99 | 419.61 | 7 | not on this course | Route atlas direction guidance/Main teal / reassurance | StreetLoopGreybox-arrow-052 |
| — | 0.0 | 498.74 | 87.83 | -99.58 | 152 | not on this course | Route atlas direction guidance/Main teal / reassurance | StreetLoopGreybox-arrow-032 |
| — | 0.0 | -322.01 | 8.96 | 531.45 | 92 | not on this course | Route atlas direction guidance/Main teal / reassurance | StreetLoopGreybox-arrow-055 |
| — | 0.0 | -283.04 | 20.03 | -573.42 | -81 | not on this course | Route atlas direction guidance/Main teal / reassurance | StreetLoopGreybox-arrow-041 |
| — | 0.0 | 311.99 | 8.08 | 559.29 | 99 | not on this course | Route atlas direction guidance/Main teal / turn | StreetLoopGreybox-arrow-025 |
| — | 0.0 | -621.56 | 6.84 | -380.16 | -1 | not on this course | Route atlas direction guidance/Main teal / reassurance | StreetLoopGreybox-arrow-045 |
| — | 0.0 | -620.63 | 6.60 | -426.02 | -1 | not on this course | Route atlas direction guidance/Main teal / Existing Southwest Cut rejoin | StreetLoopGreybox-arrow-005 |
| — | 0.0 | -623.62 | 7.27 | -280.18 | -1 | not on this course | Route atlas direction guidance/Main teal / reassurance | StreetLoopGreybox-arrow-046 |
| — | 0.0 | 275.13 | 8.11 | 561.94 | 90 | not on this course | Route atlas direction guidance/Main teal / reassurance | StreetLoopGreybox-arrow-061 |
| — | 0.0 | -626.60 | 7.98 | -80.21 | -1 | not on this course | Route atlas direction guidance/Main teal / reassurance | StreetLoopGreybox-arrow-047 |
| — | 0.0 | -421.94 | 8.35 | 535.07 | 92 | not on this course | Route atlas direction guidance/Main teal / reassurance | StreetLoopGreybox-arrow-054 |
| — | 0.0 | 381.45 | 26.67 | -363.23 | -104 | not on this course | Route atlas direction guidance/Optional gold / Creek Leap | StreetLoopGreybox-arrow-064 |
| — | 0.0 | -84.41 | 29.95 | -579.90 | -92 | not on this course | Route atlas direction guidance/Main teal / reassurance | StreetLoopGreybox-arrow-040 |
| — | 0.0 | -581.38 | 10.89 | -271.35 | -54 | not on this course | Route atlas direction guidance/Optional gold / Pine Ridge | StreetLoopGreybox-arrow-072 |
| — | 0.0 | 302.29 | 39.65 | -393.34 | -68 | not on this course | Route atlas direction guidance/Optional gold / Creek Leap | StreetLoopGreybox-arrow-062 |
| — | 0.0 | -133.37 | 30.28 | -581.71 | -92 | not on this course | Route atlas direction guidance/Main teal / Fox Gully rejoin | StreetLoopGreybox-arrow-008 |
| — | 0.0 | -155.36 | 30.73 | -582.36 | -91 | not on this course | Route atlas direction guidance/Main teal / before Pine Ridge fork | StreetLoopGreybox-arrow-009 |
| — | 0.0 | -381.05 | 6.77 | -559.62 | -84 | not on this course | Route atlas direction guidance/Main teal / reassurance | StreetLoopGreybox-arrow-042 |
| — | 0.0 | -201.34 | 30.55 | -582.12 | -88 | not on this course | Route atlas direction guidance/Main teal / main at Pine Ridge fork | StreetLoopGreybox-arrow-010 |
| — | 0.0 | 111.70 | 37.54 | -340.56 | -156 | not on this course | Route atlas direction guidance/Optional gold / Fox Gully | StreetLoopGreybox-arrow-065 |
| — | 0.0 | -521.84 | 8.02 | 539.37 | 94 | not on this course | Route atlas direction guidance/Main teal / reassurance | StreetLoopGreybox-arrow-053 |
| — | 0.0 | -628.32 | 8.38 | 19.78 | -1 | not on this course | Route atlas direction guidance/Main teal / reassurance | StreetLoopGreybox-arrow-048 |
| — | 0.0 | -631.52 | 9.10 | 219.75 | -1 | not on this course | Route atlas direction guidance/Main teal / reassurance | StreetLoopGreybox-arrow-050 |
| — | 0.0 | 509.12 | 25.52 | -354.60 | -116 | not on this course | Route atlas direction guidance/Optional gold / Creek Leap | StreetLoopGreybox-arrow-063 |
| — | 0.0 | 560.91 | 28.11 | -273.22 | 177 | not on this course | Route atlas direction guidance/Main teal / main at Creek Leap fork | StreetLoopGreybox-arrow-001 |
| — | 0.0 | -633.09 | 9.20 | 319.74 | -1 | not on this course | Route atlas direction guidance/Main teal / reassurance | StreetLoopGreybox-arrow-051 |
| — | 0.0 | -630.14 | 8.79 | 119.76 | -1 | not on this course | Route atlas direction guidance/Main teal / reassurance | StreetLoopGreybox-arrow-049 |
| — | 0.0 | 552.57 | 35.22 | -228.74 | 164 | not on this course | Route atlas direction guidance/Main teal / before Creek Leap fork | StreetLoopGreybox-arrow-000 |
| — | 0.0 | -295.62 | 24.53 | -535.05 | -48 | not on this course | Route atlas direction guidance/Optional gold / Pine Ridge | StreetLoopGreybox-arrow-069 |
| — | 0.0 | -613.58 | 5.97 | -542.87 | -9 | not on this course | Route atlas direction guidance/Main teal / turn | StreetLoopGreybox-arrow-023 |
| — | 0.0 | 544.40 | 25.78 | -374.36 | -175 | not on this course | Route atlas direction guidance/Main teal / reassurance | StreetLoopGreybox-arrow-034 |
| — | 0.0 | -122.04 | 8.25 | 529.09 | 90 | not on this course | Route atlas direction guidance/Main teal / reassurance | StreetLoopGreybox-arrow-057 |
| — | 0.0 | 368.58 | 30.31 | -394.10 | -73 | not on this course | Route atlas direction guidance/Main teal / turn | StreetLoopGreybox-arrow-017 |
| — | 0.0 | -605.42 | 7.99 | 528.57 | 17 | not on this course | Route atlas direction guidance/Main teal / turn | StreetLoopGreybox-arrow-024 |
| — | 0.0 | -628.32 | 7.47 | -226.55 | -5 | not on this course | Route atlas direction guidance/Optional gold / Pine Ridge | StreetLoopGreybox-arrow-068 |
| — | 0.0 | 388.40 | 33.56 | -408.34 | -43 | not on this course | Route atlas direction guidance/Main teal / turn | StreetLoopGreybox-arrow-016 |
| — | 0.0 | 265.97 | 36.04 | -366.25 | -55 | not on this course | Route atlas direction guidance/Main teal / Creek Leap rejoin | StreetLoopGreybox-arrow-002 |
| Abandoned Cabin Jump | 127.0 | 392.66 | 76.27 | 180.39 | 121 | not on this course | Route atlas direction guidance/Main teal / turn | StreetLoopGreybox-arrow-013 |
| Main | 132.0 | 341.17 | 79.26 | 20.40 | -91 | repeated on straight (38 m after kept arrow) | Forward teal navigation - no colliders/Main teal trail arrow |  |
| Main | 418.0 | 240.55 | 40.86 | -347.44 | -53 | not on this course | Route atlas direction guidance/Main teal / before Fox Gully fork | StreetLoopGreybox-arrow-006 |
| Main | 424.0 | 202.75 | 46.06 | -321.89 | -62 | not on this course | Route atlas direction guidance/Main teal / main at Fox Gully fork | StreetLoopGreybox-arrow-007 |
| Main | 427.0 | 182.16 | 44.34 | -311.87 | -66 | not on this course | Route atlas direction guidance/Main teal / reassurance | StreetLoopGreybox-arrow-037 |
| Main | 527.0 | 32.09 | 35.59 | -302.18 | -141 | not on this course | Route atlas direction guidance/Main teal / turn | StreetLoopGreybox-arrow-020 |
| Main | 528.0 | -11.58 | 34.95 | -544.18 | -158 | not on this course | Route atlas direction guidance/Main teal / turn | StreetLoopGreybox-arrow-021 |
| Main | 529.0 | 15.55 | 37.45 | -336.00 | -162 | not on this course | Route atlas direction guidance/Main teal / reassurance | StreetLoopGreybox-arrow-038 |
| Main | 529.0 | -1.98 | 32.43 | -433.52 | -176 | not on this course | Route atlas direction guidance/Main teal / reassurance | StreetLoopGreybox-arrow-039 |
| Main | 529.0 | -24.71 | 31.30 | -564.85 | -133 | not on this course | Route atlas direction guidance/Main teal / turn | StreetLoopGreybox-arrow-022 |
| Main | 985.0 | 349.27 | 82.08 | 203.08 | 142 | not on this course | Route atlas direction guidance/Main teal / turn | StreetLoopGreybox-arrow-012 |
| Main | 1174.0 | 459.93 | 85.32 | 91.89 | 159 | not on this course | Route atlas direction guidance/Main teal / reassurance | StreetLoopGreybox-arrow-030 |
| Tree-Top Trail | 0.0 | 319.79 | 63.10 | 332.70 | 179 | not on this course | Route atlas direction guidance/Main teal / reassurance | StreetLoopGreybox-arrow-028 |
| Tree-Top Trail | 70.0 | 537.30 | 55.96 | -184.85 | 158 | not on this course | Route atlas direction guidance/Main teal / reassurance | StreetLoopGreybox-arrow-033 |
| Tree-Top Trail | 110.0 | 51.08 | 41.56 | -287.57 | -113 | not on this course | Route atlas direction guidance/Main teal / turn | StreetLoopGreybox-arrow-019 |
| Tree-Top Trail | 110.0 | 75.33 | 43.73 | -283.44 | -90 | not on this course | Route atlas direction guidance/Main teal / turn | StreetLoopGreybox-arrow-018 |

### Dan's Backyard Loop — Reverse — removed 77

| Route | Station | x | y | z | Heading | Reason | Object | Mesh |
|---|---:|---:|---:|---:|---:|---|---|---|
| — | 0.0 | 111.70 | 37.54 | -340.56 | -156 | not on this course | Route atlas direction guidance/Optional gold / Fox Gully | StreetLoopGreybox-arrow-065 |
| — | 0.0 | -50.33 | 28.12 | -541.39 | -142 | not on this course | Route atlas direction guidance/Optional gold / Fox Gully | StreetLoopGreybox-arrow-067 |
| — | 0.0 | -133.37 | 30.28 | -581.71 | -92 | not on this course | Route atlas direction guidance/Main teal / Fox Gully rejoin | StreetLoopGreybox-arrow-008 |
| — | 0.0 | -155.36 | 30.73 | -582.36 | -91 | not on this course | Route atlas direction guidance/Main teal / before Pine Ridge fork | StreetLoopGreybox-arrow-009 |
| — | 0.0 | -477.47 | 15.97 | -349.38 | -49 | not on this course | Route atlas direction guidance/Optional gold / Pine Ridge | StreetLoopGreybox-arrow-071 |
| — | 0.0 | -620.63 | 6.60 | -426.02 | -1 | not on this course | Route atlas direction guidance/Main teal / Existing Southwest Cut rejoin | StreetLoopGreybox-arrow-005 |
| — | 0.0 | -581.38 | 10.89 | -271.35 | -54 | not on this course | Route atlas direction guidance/Optional gold / Pine Ridge | StreetLoopGreybox-arrow-072 |
| — | 0.0 | 76.37 | 9.05 | 541.88 | 82 | not on this course | Route atlas direction guidance/Main teal / reassurance | StreetLoopGreybox-arrow-059 |
| — | 0.0 | 498.74 | 87.83 | -99.58 | 152 | not on this course | Route atlas direction guidance/Main teal / reassurance | StreetLoopGreybox-arrow-032 |
| — | 0.0 | -84.41 | 29.95 | -579.90 | -92 | not on this course | Route atlas direction guidance/Main teal / reassurance | StreetLoopGreybox-arrow-040 |
| — | 0.0 | 509.20 | 23.05 | -528.82 | -143 | not on this course | Route atlas direction guidance/Main teal / turn | StreetLoopGreybox-arrow-014 |
| — | 0.0 | -283.04 | 20.03 | -573.42 | -81 | not on this course | Route atlas direction guidance/Main teal / reassurance | StreetLoopGreybox-arrow-041 |
| — | 0.0 | -322.01 | 8.96 | 531.45 | 92 | not on this course | Route atlas direction guidance/Main teal / reassurance | StreetLoopGreybox-arrow-055 |
| — | 0.0 | 175.99 | 9.02 | 550.19 | 83 | not on this course | Route atlas direction guidance/Main teal / reassurance | StreetLoopGreybox-arrow-060 |
| — | 0.0 | 275.13 | 8.11 | 561.94 | 90 | not on this course | Route atlas direction guidance/Main teal / reassurance | StreetLoopGreybox-arrow-061 |
| — | 0.0 | -480.64 | 7.07 | -550.82 | -86 | not on this course | Route atlas direction guidance/Main teal / reassurance | StreetLoopGreybox-arrow-043 |
| — | 0.0 | -222.04 | 8.92 | 529.45 | 91 | not on this course | Route atlas direction guidance/Main teal / reassurance | StreetLoopGreybox-arrow-056 |
| — | 0.0 | -385.95 | 20.97 | -441.61 | -43 | not on this course | Route atlas direction guidance/Optional gold / Pine Ridge | StreetLoopGreybox-arrow-070 |
| — | 0.0 | -521.84 | 8.02 | 539.37 | 94 | not on this course | Route atlas direction guidance/Main teal / reassurance | StreetLoopGreybox-arrow-053 |
| — | 0.0 | -122.04 | 8.25 | 529.09 | 90 | not on this course | Route atlas direction guidance/Main teal / reassurance | StreetLoopGreybox-arrow-057 |
| — | 0.0 | -630.14 | 8.79 | 119.76 | -1 | not on this course | Route atlas direction guidance/Main teal / reassurance | StreetLoopGreybox-arrow-049 |
| — | 0.0 | -623.62 | 7.27 | -280.18 | -1 | not on this course | Route atlas direction guidance/Main teal / reassurance | StreetLoopGreybox-arrow-046 |
| — | 0.0 | 509.12 | 25.52 | -354.60 | -116 | not on this course | Route atlas direction guidance/Optional gold / Creek Leap | StreetLoopGreybox-arrow-063 |
| — | 0.0 | 537.30 | 55.96 | -184.85 | 158 | not on this course | Route atlas direction guidance/Main teal / reassurance | StreetLoopGreybox-arrow-033 |
| — | 0.0 | -421.94 | 8.35 | 535.07 | 92 | not on this course | Route atlas direction guidance/Main teal / reassurance | StreetLoopGreybox-arrow-054 |
| — | 0.0 | -618.95 | 6.29 | -480.12 | -2 | not on this course | Route atlas direction guidance/Main teal / reassurance | StreetLoopGreybox-arrow-044 |
| — | 0.0 | 39.67 | 23.32 | -447.71 | -139 | not on this course | Route atlas direction guidance/Optional gold / Fox Gully | StreetLoopGreybox-arrow-066 |
| — | 0.0 | 311.99 | 8.08 | 559.29 | 99 | not on this course | Route atlas direction guidance/Main teal / turn | StreetLoopGreybox-arrow-025 |
| — | 0.0 | 425.73 | 35.74 | -457.12 | -34 | not on this course | Route atlas direction guidance/Main teal / reassurance | StreetLoopGreybox-arrow-036 |
| — | 0.0 | -621.56 | 6.84 | -380.16 | -1 | not on this course | Route atlas direction guidance/Main teal / reassurance | StreetLoopGreybox-arrow-045 |
| — | 0.0 | 302.29 | 39.65 | -393.34 | -68 | not on this course | Route atlas direction guidance/Optional gold / Creek Leap | StreetLoopGreybox-arrow-062 |
| — | 0.0 | 265.97 | 36.04 | -366.25 | -55 | not on this course | Route atlas direction guidance/Main teal / Creek Leap rejoin | StreetLoopGreybox-arrow-002 |
| — | 0.0 | 318.64 | 36.94 | 429.21 | -180 | not on this course | Route atlas direction guidance/Main teal / reassurance | StreetLoopGreybox-arrow-027 |
| — | 0.0 | -626.60 | 7.98 | -80.21 | -1 | not on this course | Route atlas direction guidance/Main teal / reassurance | StreetLoopGreybox-arrow-047 |
| — | 0.0 | -613.58 | 5.97 | -542.87 | -9 | not on this course | Route atlas direction guidance/Main teal / turn | StreetLoopGreybox-arrow-023 |
| — | 0.0 | -22.04 | 7.95 | 528.82 | 90 | not on this course | Route atlas direction guidance/Main teal / reassurance | StreetLoopGreybox-arrow-058 |
| — | 0.0 | -605.42 | 7.99 | 528.57 | 17 | not on this course | Route atlas direction guidance/Main teal / turn | StreetLoopGreybox-arrow-024 |
| — | 0.0 | 240.55 | 40.86 | -347.44 | -53 | not on this course | Route atlas direction guidance/Main teal / before Fox Gully fork | StreetLoopGreybox-arrow-006 |
| — | 0.0 | -628.32 | 7.47 | -226.55 | -5 | not on this course | Route atlas direction guidance/Optional gold / Pine Ridge | StreetLoopGreybox-arrow-068 |
| — | 0.0 | -581.44 | 6.21 | -551.64 | -88 | not on this course | Route atlas direction guidance/Main teal / main at Existing Southwest Cut fork | StreetLoopGreybox-arrow-004 |
| — | 0.0 | -633.09 | 9.20 | 319.74 | -1 | not on this course | Route atlas direction guidance/Main teal / reassurance | StreetLoopGreybox-arrow-051 |
| — | 0.0 | -201.34 | 30.55 | -582.12 | -88 | not on this course | Route atlas direction guidance/Main teal / main at Pine Ridge fork | StreetLoopGreybox-arrow-010 |
| — | 0.0 | -535.48 | 6.69 | -550.21 | -92 | not on this course | Route atlas direction guidance/Main teal / before Existing Southwest Cut fork | StreetLoopGreybox-arrow-003 |
| — | 0.0 | -295.62 | 24.53 | -535.05 | -48 | not on this course | Route atlas direction guidance/Optional gold / Pine Ridge | StreetLoopGreybox-arrow-069 |
| — | 0.0 | -628.32 | 8.38 | 19.78 | -1 | not on this course | Route atlas direction guidance/Main teal / reassurance | StreetLoopGreybox-arrow-048 |
| — | 0.0 | -381.05 | 6.77 | -559.62 | -84 | not on this course | Route atlas direction guidance/Main teal / reassurance | StreetLoopGreybox-arrow-042 |
| — | 0.0 | -625.16 | 7.63 | -181.19 | -1 | not on this course | Route atlas direction guidance/Main teal / Pine Ridge rejoin | StreetLoopGreybox-arrow-011 |
| — | 0.0 | -24.71 | 31.30 | -564.85 | -133 | not on this course | Route atlas direction guidance/Main teal / turn | StreetLoopGreybox-arrow-022 |
| — | 0.0 | 392.66 | 76.27 | 180.39 | 121 | not on this course | Route atlas direction guidance/Main teal / turn | StreetLoopGreybox-arrow-013 |
| — | 0.0 | -629.70 | 8.99 | 419.61 | 7 | not on this course | Route atlas direction guidance/Main teal / reassurance | StreetLoopGreybox-arrow-052 |
| — | 0.0 | 490.40 | 22.67 | -544.66 | -100 | not on this course | Route atlas direction guidance/Main teal / turn | StreetLoopGreybox-arrow-015 |
| — | 0.0 | -631.52 | 9.10 | 219.75 | -1 | not on this course | Route atlas direction guidance/Main teal / reassurance | StreetLoopGreybox-arrow-050 |
| — | 0.0 | 320.11 | 11.70 | 525.92 | -179 | not on this course | Route atlas direction guidance/Main teal / reassurance | StreetLoopGreybox-arrow-026 |
| Abandoned Logging Ridge | 79.0 | 349.27 | 82.08 | 203.08 | 142 | not on this course | Route atlas direction guidance/Main teal / turn | StreetLoopGreybox-arrow-012 |
| Abandoned Logging Ridge | 89.0 | 332.64 | 81.86 | 235.91 | 162 | not on this course | Route atlas direction guidance/Main teal / reassurance | StreetLoopGreybox-arrow-029 |
| Main | 112.0 | 459.93 | 85.32 | 91.89 | 159 | not on this course | Route atlas direction guidance/Main teal / reassurance | StreetLoopGreybox-arrow-030 |
| Main | 782.0 | 15.55 | 37.45 | -336.00 | -162 | not on this course | Route atlas direction guidance/Main teal / reassurance | StreetLoopGreybox-arrow-038 |
| Main | 782.0 | -1.98 | 32.43 | -433.52 | -176 | not on this course | Route atlas direction guidance/Main teal / reassurance | StreetLoopGreybox-arrow-039 |
| Main | 782.0 | -11.58 | 34.95 | -544.18 | -158 | not on this course | Route atlas direction guidance/Main teal / turn | StreetLoopGreybox-arrow-021 |
| Main | 784.0 | 32.09 | 35.59 | -302.18 | -141 | not on this course | Route atlas direction guidance/Main teal / turn | StreetLoopGreybox-arrow-020 |
| Main | 793.0 | 41.50 | 36.61 | -94.13 | 88 | repeated on straight (30 m after kept arrow) | Backyard Reverse main route/Reverse teal ground arrow | arrow-18 |
| Main | 803.0 | 51.08 | 41.56 | -287.57 | -113 | not on this course | Route atlas direction guidance/Main teal / turn | StreetLoopGreybox-arrow-019 |
| Main | 828.0 | 75.33 | 43.73 | -283.44 | -90 | not on this course | Route atlas direction guidance/Main teal / turn | StreetLoopGreybox-arrow-018 |
| Main | 927.0 | 182.16 | 44.34 | -311.87 | -66 | not on this course | Route atlas direction guidance/Main teal / reassurance | StreetLoopGreybox-arrow-037 |
| Main | 929.0 | 202.75 | 46.06 | -321.89 | -62 | not on this course | Route atlas direction guidance/Main teal / main at Fox Gully fork | StreetLoopGreybox-arrow-007 |
| Main | 1064.0 | 257.63 | 64.69 | -23.96 | 87 | repeated on straight (30 m after kept arrow) | Backyard Reverse main route/Reverse teal ground arrow | arrow-14 |
| Main | 1094.0 | 287.06 | 70.14 | -21.99 | 82 | repeated on straight (60 m after kept arrow) | Backyard Reverse main route/Reverse teal ground arrow | arrow-23 |
| Main | 1124.0 | 315.98 | 75.68 | -16.44 | 80 | repeated on straight (90 m after kept arrow) | Backyard Reverse main route/Reverse teal ground arrow | arrow-10 |
| Main | 1183.0 | 373.51 | 84.79 | -6.20 | 79 | repeated on straight (29 m after kept arrow) | Backyard Reverse main route/Reverse teal ground arrow | Reduced jump guidance |
| Storm Drain / Gully Jump | 0.0 | 319.79 | 63.10 | 332.70 | 179 | not on this course | Route atlas direction guidance/Main teal / reassurance | StreetLoopGreybox-arrow-028 |
| Storm Drain / Gully Jump | 150.0 | 552.57 | 35.22 | -228.74 | 164 | not on this course | Route atlas direction guidance/Main teal / before Creek Leap fork | StreetLoopGreybox-arrow-000 |
| Storm Drain / Gully Jump | 155.0 | 560.91 | 28.11 | -273.22 | 177 | not on this course | Route atlas direction guidance/Main teal / main at Creek Leap fork | StreetLoopGreybox-arrow-001 |
| Storm Drain / Gully Jump | 162.0 | 544.40 | 25.78 | -374.36 | -175 | not on this course | Route atlas direction guidance/Main teal / reassurance | StreetLoopGreybox-arrow-034 |
| Storm Drain / Gully Jump | 164.0 | 534.02 | 31.43 | -473.10 | -165 | not on this course | Route atlas direction guidance/Main teal / reassurance | StreetLoopGreybox-arrow-035 |
| Storm Drain / Gully Jump | 165.0 | 388.40 | 33.56 | -408.34 | -43 | not on this course | Route atlas direction guidance/Main teal / turn | StreetLoopGreybox-arrow-016 |
| Storm Drain / Gully Jump | 165.0 | 381.45 | 26.67 | -363.23 | -104 | not on this course | Route atlas direction guidance/Optional gold / Creek Leap | StreetLoopGreybox-arrow-064 |
| Storm Drain / Gully Jump | 165.0 | 368.58 | 30.31 | -394.10 | -73 | not on this course | Route atlas direction guidance/Main teal / turn | StreetLoopGreybox-arrow-017 |

### Mountain Loop — Forward — removed 24

| Route | Station | x | y | z | Heading | Reason | Object | Mesh |
|---|---:|---:|---:|---:|---:|---|---|---|
| Main | 225.0 | 840.03 | 110.12 | -275.00 | 90 | repeated on straight (38 m after kept arrow) | Route atlas direction guidance/Main teal / reassurance | MountainLoop-arrow-024 |
| Main | 325.0 | 940.03 | 110.12 | -275.00 | 90 | repeated on straight (138 m after kept arrow) | Route atlas direction guidance/Main teal / reassurance | MountainLoop-arrow-025 |
| Main | 925.0 | 1349.15 | 122.97 | -114.71 | -43 | repeated on straight (100 m after kept arrow) | Route atlas direction guidance/Main teal / reassurance | MountainLoop-arrow-029 |
| Main | 1725.0 | 1125.65 | 152.04 | 140.12 | 75 | repeated on straight (116 m after kept arrow) | Route atlas direction guidance/Main teal / reassurance | MountainLoop-arrow-033 |
| Main | 1925.0 | 1243.71 | 152.11 | 222.02 | -106 | repeated on straight (38 m after kept arrow) | Route atlas direction guidance/Main teal / reassurance | MountainLoop-arrow-035 |
| Main | 2025.0 | 1147.50 | 152.12 | 194.71 | -106 | repeated on straight (138 m after kept arrow) | Route atlas direction guidance/Main teal / reassurance | MountainLoop-arrow-036 |
| Summit Traverse | 12.0 | 1000.93 | 160.90 | 102.86 | -89 | duplicate of Route atlas direction guidance/Optional gold / Summit Traverse (8 m) | CR133 rerouted shortcut guidance/CR133 shortcut gold arrow | MountainLoop-seated-marking-25 |
| Summit Traverse | 26.0 | 987.31 | 160.47 | 100.06 | -111 | duplicate of Route atlas direction guidance/Optional gold / Summit Traverse (6 m) | CR133 rerouted shortcut guidance/CR133 shortcut gold arrow | MountainLoop-seated-marking-19 |
| Summit Traverse | 40.0 | 974.50 | 157.56 | 94.41 | -116 | repeated on straight (20 m after kept arrow) | CR133 rerouted shortcut guidance/CR133 shortcut gold arrow | MountainLoop-seated-marking-8 |
| Summit Traverse | 54.0 | 961.75 | 155.41 | 88.64 | -110 | repeated on straight (34 m after kept arrow) | CR133 rerouted shortcut guidance/CR133 shortcut gold arrow | MountainLoop-seated-marking-18 |
| Summit Traverse | 68.0 | 948.72 | 152.93 | 83.53 | -114 | repeated on straight (48 m after kept arrow) | CR133 rerouted shortcut guidance/CR133 shortcut gold arrow | MountainLoop-seated-marking-23 |
| Summit Traverse | 82.0 | 936.08 | 150.25 | 77.51 | -117 | repeated on straight (62 m after kept arrow) | CR133 rerouted shortcut guidance/CR133 shortcut gold arrow | MountainLoop-seated-marking-7 |
| Summit Traverse | 96.0 | 923.63 | 147.38 | 71.10 | -117 | repeated on straight (76 m after kept arrow) | CR133 rerouted shortcut guidance/CR133 shortcut gold arrow | MountainLoop-seated-marking-15 |
| Summit Traverse | 110.0 | 911.12 | 144.30 | 64.82 | -115 | repeated on straight (90 m after kept arrow) | CR133 rerouted shortcut guidance/CR133 shortcut gold arrow | MountainLoop-seated-marking-1 |
| Summit Traverse | 124.0 | 898.30 | 140.97 | 59.22 | -111 | duplicate of Route atlas direction guidance/Optional gold / Summit Traverse (6 m) | CR133 rerouted shortcut guidance/CR133 shortcut gold arrow | MountainLoop-seated-marking-9 |
| Summit Traverse | 130.0 | 892.66 | 139.47 | 57.18 | -109 | repeated on straight (110 m after kept arrow) | Route atlas direction guidance/Optional gold / Summit Traverse | MountainLoop-seated-marking-27 |
| Summit Traverse | 138.0 | 885.06 | 137.36 | 54.68 | -107 | duplicate of Route atlas direction guidance/Optional gold / Summit Traverse (8 m) | CR133 rerouted shortcut guidance/CR133 shortcut gold arrow | MountainLoop-seated-marking-21 |
| Summit Traverse | 152.0 | 871.62 | 133.46 | 50.76 | -105 | repeated on straight (132 m after kept arrow) | CR133 rerouted shortcut guidance/CR133 shortcut gold arrow | MountainLoop-seated-marking-17 |
| Summit Traverse | 166.0 | 858.05 | 129.37 | 47.29 | -104 | repeated on straight (146 m after kept arrow) | CR133 rerouted shortcut guidance/CR133 shortcut gold arrow | MountainLoop-seated-marking-6 |
| Summit Traverse | 194.0 | 830.73 | 120.88 | 41.19 | -102 | repeated on straight (14 m after kept arrow) | CR133 rerouted shortcut guidance/CR133 shortcut gold arrow | MountainLoop-seated-marking-20 |
| Summit Traverse | 208.0 | 817.01 | 116.51 | 38.42 | -101 | repeated on straight (28 m after kept arrow) | CR133 rerouted shortcut guidance/CR133 shortcut gold arrow | MountainLoop-fork-75472-17664 |
| Summit Traverse | 222.0 | 803.22 | 111.83 | 35.95 | -100 | repeated on straight (42 m after kept arrow) | CR133 rerouted shortcut guidance/CR133 shortcut gold arrow | MountainLoop-fork-49087-17664 |
| Summit Traverse | 250.0 | 775.56 | 102.41 | 31.65 | -98 | duplicate of Route atlas direction guidance/Optional gold / Summit Traverse (10 m) | CR133 rerouted shortcut guidance/CR133 shortcut gold arrow | MountainLoop-fork-45859-17664 |
| Summit Traverse | 264.0 | 761.70 | 98.29 | 29.66 | -98 | duplicate of Route atlas direction guidance/Optional gold / Summit Traverse (4 m) | CR133 rerouted shortcut guidance/CR133 shortcut gold arrow | MountainLoop-fork-53488-17664 |

### Mountain Loop — Reverse — removed 12

| Route | Station | x | y | z | Heading | Reason | Object | Mesh |
|---|---:|---:|---:|---:|---:|---|---|---|
| Downhill Ridge Cut | 54.0 | 769.24 | 86.71 | -174.46 | -46 | repeated on straight (36 m after kept arrow) | CR122 continuous mountain support/CR122 crossing gold arrow | MountainLoopReverse-CR122-crossing-arrow-54 |
| Main | 825.0 | 990.00 | 152.12 | 216.22 | 180 | repeated on straight (113 m after kept arrow) | Route atlas direction guidance/Main teal / reassurance | MountainLoopReverse-arrow-034 |
| Main | 1504.0 | 793.50 | 91.77 | -187.29 | -80 | not on this course | CR122 continuous mountain support/CR122 crossing gold arrow | MountainLoopReverse-CR122-crossing-arrow-26 |
| Main | 1726.0 | 976.26 | 130.74 | -100.38 | 85 | repeated on straight (99 m after kept arrow) | Route atlas direction guidance/Main teal / reassurance | MountainLoopReverse-seated-marking-15 |
| Main | 1926.0 | 1171.84 | 137.90 | -75.47 | 110 | repeated on straight (100 m after kept arrow) | Route atlas direction guidance/Main teal / reassurance | MountainLoopReverse-arrow-041 |
| Main | 2226.0 | 1236.74 | 110.12 | -299.07 | -93 | repeated on straight (63 m after kept arrow) | Route atlas direction guidance/Main teal / reassurance | MountainLoopReverse-arrow-044 |
| Main | 2426.0 | 1037.30 | 114.75 | -300.00 | -90 | repeated on straight (100 m after kept arrow) | Route atlas direction guidance/Main teal / reassurance | MountainLoopReverse-arrow-046 |
| Main | 2950.0 | 780.23 | 87.89 | -183.02 | -63 | not on this course | CR122 continuous mountain support/CR122 crossing gold arrow | MountainLoopReverse-CR122-crossing-arrow-40 |
| Summit Traverse | 353.0 | 881.64 | 115.13 | -48.91 | -109 | duplicate of Summit merge gold arrow (0 m) | Summit merge gold arrow | MountainLoopReverse-seated-marking-12 |
| Summit Traverse | 389.0 | 863.40 | 111.47 | -76.76 | 167 | duplicate of Summit merge gold arrow (0 m) | Summit merge gold arrow | MountainLoopReverse-seated-marking-5 |
| Summit Traverse | 407.0 | 872.49 | 109.54 | -91.99 | 131 | duplicate of Summit merge gold arrow (0 m) | Summit merge gold arrow | MountainLoopReverse-seated-marking-8(Clone)(Clone)(Clone)(Clone)(Clone)(Clone)(Clone)(Clone)(Clone) |
| Summit Traverse | 425.0 | 888.59 | 115.10 | -99.00 | 89 | duplicate of Summit merge gold arrow (0 m) | Summit merge gold arrow | MountainLoopReverse-seated-marking-16 |

