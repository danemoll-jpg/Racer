# Route atlas — 0.30.0-review1

Derived from saved Unity scenes and collider samples. World X/Y/Z in metres; +Z north. Each course is a separate scene; the overall overlay is not one shared collision world.

## Views

- Overall.png: all six main routes and optional branches.
- One PNG per scene: active direction, start/finish, gates, optional entrances/rejoins, jumps and environment roads.
- House3-Forest-Laurel.png: promoted main route, retired detour, both driveway states, physical Laurel launch/flight and approximate practical landing/runout area.
- Granite-height-comparison.png: original navigation heights versus scene-specific ground; the promoted route now follows current support.
- House3-Forest-Laurel-before.png: retained pre-change reference.

## Stable local landmarks

- L01 House 3: X=418.00, Z=-160.00.
- L02 Driveway road mouth: X=515.18, Z=-132.87.
- L03 Driveway house end: X=436.47, Z=-152.33.
- L04 Granite east fork: X=600.57, Z=-161.74.
- L05 Granite west rejoin: X=200.89, Z=-175.39.
- L06 Laurel launch: X=346.60, Z=-176.13.
- L07: Laurel geometric landing at 32 m/s: (376.58, 80.03, -90.48).
- L08: Laurel geometric landing at 38 m/s: (389.86, 78.40, -52.54).

L07/L08 come from the existing verified physical ramp data in Docs/FocusedRecovery/verification.txt; invariance checks prove it is unchanged. The dashed landing/runout annotation adds 30m lateral allowance and 60m runout for planning; it does not define or restrict gameplay recovery. The entire Street Reverse pre-existing scene is preserved, including terrain and fencing beyond that annotation.

## Current identities and checkpoints

### StreetLoopGreybox — `street-v14-discovery`

- main: Main course; 2331 control points; from (319.00, 8.00, 550.00) to (318.16, 8.00, 553.30).
- shortcut: Fox Gully; 238 control points; from (220.82, 44.43, -333.13) to (-110.03, 30.01, -580.70).
- shortcut: Existing Southwest Cut; 91 control points; from (-560.08, 6.36, -551.35) to (-620.04, 6.39, -449.38).
- shortcut: Pine Ridge; 306 control points; from (-179.99, 30.88, -582.56) to (-624.83, 7.47, -204.56).
- shortcut: Creek Leap; 186 control points; from (558.37, 30.09, -252.13) to (285.74, 35.40, -378.58).
- driveway: House 3 valley driveway; 42 control points; from (515.18, 81.73, -132.87) to (436.47, 32.75, -152.33).
- S/F: (319.66, 16.81, 512.81).
- CP1: (321.97, 75.61, 288.66).
- CP2: (449.82, 87.12, 112.71).
- CP3: (502.45, 88.54, -106.62).
- CP4: (556.32, 26.57, -315.48).
- CP5: (502.74, 24.05, -536.44).
- CP6: (130.11, 38.61, -293.96).
- CP7: (1.30, 39.02, -398.93).
- CP8: (-288.51, 20.50, -572.54).
- CP9: (-501.25, 8.55, -549.75).
- CP10: (-621.67, 8.39, -374.52).
- CP11: (-625.96, 9.35, -124.56).
- CP12: (-629.27, 10.02, 70.83).
- CP13: (-632.88, 10.62, 302.81).
- CP14: (-604.65, 9.46, 530.50).
- CP15: (-376.24, 10.08, 533.35).
- CP16: (-145.94, 9.84, 529.10).
- CP17: (84.03, 10.53, 542.90).

### LakeWoods — `lake-v7-discovery`

- main: Main course; 2057 control points; from (566.00, 82.00, -40.00) to (566.08, 82.00, -41.34).
- shortcut: Echo Cave; 529 control points; from (205.31, 51.35, 294.55) to (-15.80, 26.19, -128.83).
- driveway: House 3 valley driveway; 42 control points; from (515.18, 81.73, -132.87) to (436.47, 32.75, -152.33).
- S/F: (566.81, 83.66, 24.98).
- CP1: (471.36, 79.39, 175.93).
- CP2: (-45.62, 28.17, 185.26).
- CP3: (209.89, 46.30, -179.69).
- CP4: (562.27, 37.32, -223.83).
- J1: J1 Creek crossing (forest-jump); start {'x': 402.08441162109375, 'y': 73.97017669677734, 'z': 215.72079467773438}; end {'x': 297.1917724609375, 'y': 72.1966781616211, 'z': 272.58990478515625}.
- J2: J2 Deep gully (forest-jump); start {'x': 161.08592224121094, 'y': 46.70865249633789, 'z': 287.9162292480469}; end {'x': 37.08386993408203, 'y': 34.08146667480469, 'z': 240.69911193847656}.
- J3: J3 Root roller (forest-jump); start {'x': -80.98110961914062, 'y': 23.360143661499023, 'z': 150.169677734375}; end {'x': -121.81674194335938, 'y': 19.831298828125, 'z': 55.30788040161133}.
- J4: J4 Linked kicker (forest-jump); start {'x': -123.63580322265625, 'y': 20.119646072387695, 'z': 43.45254898071289}; end {'x': -105.43516540527344, 'y': 20.09143829345703, 'z': -63.93218994140625}.
- J5: J5 Ridge drop (forest-jump); start {'x': 43.29134750366211, 'y': 30.24955177307129, 'z': -138.32667541503906}; end {'x': 168.12086486816406, 'y': 40.07364273071289, 'z': -163.9176025390625}.
- J6: J6 Homeward leap (forest-jump); start {'x': 252.1727752685547, 'y': 47.16930389404297, 'z': -206.1785125732422}; end {'x': 357.4905700683594, 'y': 50.49995040893555, 'z': -274.4570617675781}.

### StreetLoopReverse — `street-reverse-local-laurel-v6`

- main: Main course; 2331 control points; from (319.00, 8.00, 550.00) to (319.23, 8.03, 548.85).
- shortcut: Laurel Switchbacks; 959 control points; from (195.09, 45.76, -317.91) to (481.02, 88.51, -63.44).
- shortcut: Granite Creek Cut; 539 control points; from (-76.41, 7.96, 529.08) to (-628.42, 8.32, 25.41).
- driveway: House 3 valley driveway; 222 control points; from (515.10, 81.79, -132.69) to (436.47, 32.75, -152.33).
- S/F: (319.65, 16.98, 512.50).
- CP1: (84.04, 10.63, 542.90).
- CP2: (-145.96, 9.94, 529.10).
- CP3: (-376.23, 10.18, 533.35).
- CP4: (-604.65, 9.56, 530.50).
- CP5: (-632.88, 10.72, 302.81).
- CP6: (-624.59, 9.02, -220.44).
- CP7: (-621.67, 8.39, -374.52).
- CP8: (-501.24, 8.55, -549.75).
- CP9: (-288.22, 20.56, -572.59).
- CP10: (1.33, 39.15, -398.72).
- CP11: (130.09, 38.61, -293.96).
- CP12: (502.78, 24.16, -536.41).
- CP13: (556.33, 26.67, -315.42).
- CP14: (472.53, 84.93, -10.92).
- CP15: (449.86, 87.23, 112.62).
- CP16: (322.01, 75.78, 288.34).
- J1: Laurel straight runway - no recovery (recovery-exclusion); start {'x': 325.7912292480469, 'y': 66.02955627441406, 'z': -235.59646606445312}; end {'x': 346.6033020019531, 'y': 80.2695541381836, 'z': -176.13339233398438}.

### ForestLoopReverse — `forest-reverse-v6-granite-main`

- main: Main course; 1872 control points; from (566.00, 82.00, -40.00) to (565.95, 82.00, -39.21).
- shortcut: Fern Gully; 340 control points; from (200.89, 44.07, -175.39) to (37.07, 34.29, 240.69).
- driveway: House 3 valley driveway; 42 control points; from (515.18, 81.73, -132.87) to (436.47, 32.75, -152.33).
- S/F: (607.95, 79.75, -127.54).
- CP1: (555.56, 43.94, -206.75).
- CP2: (124.28, 40.64, -154.28).
- CP3: (-45.53, 28.17, 185.33).
- CP4: (471.42, 79.39, 175.89).
- J1: Reverse ridge crossing (forest-jump); start {'x': 182.30264282226562, 'y': 42.3657112121582, 'z': -168.21971130371094}; end {'x': 28.477928161621094, 'y': 29.20059585571289, 'z': -136.21377563476562}.
- J2: Reverse linked descent (forest-jump); start {'x': -97.5346450805664, 'y': 21.118017196655273, 'z': -76.63247680664062}; end {'x': -71.11955261230469, 'y': 24.247962951660156, 'z': 161.43264770507812}.
- J3: Reverse deep gully (forest-jump); start {'x': 24.05118751525879, 'y': 33.04872131347656, 'z': 233.3488006591797}; end {'x': 175.72732543945312, 'y': 48.230289459228516, 'z': 290.7947998046875}.
- J4: Reverse creek crossing (forest-jump); start {'x': 285.97698974609375, 'y': 64.3052978515625, 'z': 278.6631164550781}; end {'x': 415.2990417480469, 'y': 74.51679229736328, 'z': 208.6457061767578}.

### MountainLoop — `mountain-forward-v5-supported`

- main: Main course; 3714 control points; from (730.24, 83.73, -114.55) to (730.18, 83.73, -114.52).
- shortcut: Summit Traverse; 682 control points; from (1011.15, 161.20, 98.18) to (683.06, 85.47, 19.67).
- shortcut: Climbing Ridge Cut; 400 control points; from (747.00, 86.23, -120.00) to (1020.00, 139.32, -60.00).
- driveway: House 3 valley driveway; 42 control points; from (515.18, 81.73, -132.87) to (436.47, 32.75, -152.33).
- S/F: (726.42, 80.61, -43.39).
- CP1: (719.07, 83.05, -102.34).
- CP2: (820.23, 111.60, -275.00).
- CP3: (1043.93, 146.27, -29.19).
- CP4: (1014.72, 162.67, 89.63).
- CP5: (1191.66, 153.54, 169.66).
- CP6: (702.63, 83.39, -9.46).
- J1: Eastbound Gully Flight (mountain-flight); start {'x': 1070.0, 'y': 131.60000610351562, 'z': -275.0}; end {'x': 1350.0, 'y': 99.15999603271484, 'z': -275.0}.
- J2: Homeward Summit Flight (mountain-flight); start {'x': 980.3800659179688, 'y': 177.9199981689453, 'z': 147.2691192626953}; end {'x': 730.2625732421875, 'y': 96.75, 'z': 76.26741027832031}.
- J3: Jump run-up / no recovery checkpoint (recovery-exclusion); start {'x': 1120.8564453125, 'y': 151.926025390625, 'z': 138.9575958251953}; end {'x': 980.3800048828125, 'y': 177.9199981689453, 'z': 147.2691192626953}.
- J4: Jump run-up / no recovery checkpoint (recovery-exclusion); start {'x': 771.37548828125, 'y': 102.46199798583984, 'z': -222.26513671875}; end {'x': 1070.0, 'y': 131.60000610351562, 'z': -275.0}.

### MountainLoopReverse — `mountain-reverse-v5-supported`

- main: Main course; 3718 control points; from (717.47, 79.00, -59.43) to (717.34, 79.00, -59.68).
- shortcut: Downhill Ridge Cut; 918 control points; from (818.76, 95.71, -185.32) to (722.89, 79.00, -48.32).
- shortcut: Summit Traverse; 2219 control points; from (990.00, 163.62, 115.00) to (820.00, 98.40, -125.00).
- driveway: House 3 valley driveway; 42 control points; from (515.18, 81.73, -132.87) to (436.47, 32.75, -152.33).
- S/F: (724.52, 79.86, -6.19).
- CP1: (962.20, 164.72, 136.65).
- CP2: (992.80, 153.60, 314.78).
- CP3: (842.32, 96.09, -203.72).
- CP4: (854.22, 108.04, -105.98).
- CP5: (1254.87, 111.60, -297.29).
- CP6: (729.16, 83.22, -87.61).
- J1: Westbound Gully Flight (mountain-flight); start {'x': 1005.0, 'y': 131.60000610351562, 'z': -300.0}; end {'x': 725.0, 'y': 99.15999603271484, 'z': -300.0}.
- J2: South Face Summit Flight (mountain-flight); start {'x': 990.0, 'y': 177.9199981689453, 'z': 65.0}; end {'x': 990.0, 'y': 96.75, 'z': -195.0}.
- J3: Jump run-up / no recovery checkpoint (recovery-exclusion); start {'x': 793.469970703125, 'y': 86.49044799804688, 'z': -198.08462524414062}; end {'x': 716.8658447265625, 'y': 84.99044799804688, 'z': -123.37974548339844}.

## Relationships and limitations

- Granite Saddle exists as an authored branch only in Forest Reverse. Dan explicitly chose Reverse-only promotion; Forward retains its own terrain and Echo Cave.
- Promoted Granite retains all original X/Z points. Its old navigation heights were stale by up to about 21m; only navigation Y was aligned to current colliders.
- Laurel physical launch is approximately (345.94, 80.27, -178.02), while the Granite crossing near that X/Z is roughly Y=39–40 in its separate Forest scene. This overlay is not a physical collision between those scenes.
- The proposed straight House 3 line intersects the retained Laurel metadata corridor near X=500, Z=-137. No new driveway was built in Street Reverse; its terrain, driveway and Laurel data are unchanged.
- Old driveway overlays touching race roads were retained locally to avoid altering accepted collision surfaces. Broad terrain restoration along the old winding trace was intentionally avoided.
- Woodland visible outside the mapped corridors remains available for future planning, but no candidate route, entrance, jump or footprint is selected or built. Dan will choose after reviewing this atlas.

Raw references: routes-before.json, routes-after.json, detail-before.json, promotion.txt, house3-cleanup.txt, guidance.txt, targeted-checks.txt and preservation.txt.
