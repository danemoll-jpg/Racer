# Dan's Backyard — saved scene atlas

[Forward](DansBackyard.svg) · [Reverse](DansBackyardReverse.svg) · [Existing route atlas](../ForestWaterJump/ATLAS.md)

The maps are generated from the saved scene geometry JSON, not from the requested anchors. X/Z are Unity world metres; north is +Z. Teal is the active main course; gold is its legitimate directional shortcut. Grey shows the unchanged Street Loop road used by the return. The current layouts are approximately 1.09km Forward and 1.16km Reverse, with ten timing gates each.

| Feature | Location / relationship |
|---|---|
| Start / finish | (463.6,79.73,8), Dan's existing driveway; rider HUD height is above the road surface |
| Smooth property approach | West along the existing beige concrete to (422.7,82.8,9) |
| Property hill | Blended descent to the lowered parking area near X=409.5 |
| Lower parking / dirt start | Approximately Y=79.46, ending near (391.9,79.46,9.7) |
| Dump launch | (331.4,78.5,16.2); aligned westward toward the supplied landing X/Z |
| Dump bottom | Approximately Y=74.8, about 12 feet below the lip; drivable floor and gradual exit |
| Far-side landing | Passes (258.6,76.6,8.2); raised locally to approximately six feet above the dump floor |
| Reverse dump bypass | Wooded southern line via (344,76,-26), (307,73,-28), (276,74,-18); no backward dump jump |
| Landing runout / forest descent | Level runout through (240,76.5,6.2) and (205,75.5,2.3), then curves down through (185,72,-8), (164,63,-28), (151,54,-53), (142,44,-85), toward the supplied gully |
| Deep gully | Centred on X=96.1, Z=-108.6; launch bank around Y=40, escape floor around Y=35.1 |
| Right turn and second crossing | Right after the westward jump, looping north through (69,39,-96) and east over the shallower northern end near (96.1,40.5,-83) |
| Reverse deep-gully crossing | Grounded southern approach via (78,38.1,-120), (86,37.8,-134), (105,38,-137), (120,38.4,-125); avoids jumping backward into the high eastern lip |
| Winding return | Separate northwestern leg through (120,46,-35), (136,51,0), (165,59,32), (200,67,48), then northern turns through (275,73,100), (326,79,116), (402,85,111) |
| Return anchor | (456.4,81.34,67.4), then joins the existing South Cherokee Lane surface and returns to Dan's driveway |
| Optional ridge | Narrow technical line between (235,70,62) and (367,83,88); direction-specific branch and checkpoint entitlement |

The landing elevation intentionally follows the requested **dump-floor/landing relationship** rather than the tentative Y=68.2. No existing road or property was raised for it. The main forest trail and return use new, privately cloned terrain assets in the two Backyard scenes.

The combined historical atlas includes **Fern Gully only in ForestLoopReverse**. Actual collider inspection confirms different terrain near (140,40,-88): Y≈44.09 in StreetLoopGreybox versus Y≈37.62 in ForestLoopReverse. The new gully uses Dan's specified coordinates in the Street Loop world where he drove; Fern Gully and Forest Reverse terrain are unchanged. The initial inferred conflict was withdrawn.

This is an implementation atlas. Targeted driving verification and release delivery are recorded separately; human gameplay acceptance is not implied.
