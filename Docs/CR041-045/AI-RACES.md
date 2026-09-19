# Review2 pace and traffic measurements

These are real-time standalone motor/input runs, not manually stepped physics. The original car is an automated reference pilot, not Dan. Tourer, motorcycle and ATV are actual mixed-profile opponents. All use the same vehicle capabilities as the garage profiles. Normal/Hard tuning changes driver decisions, not motor power or speed caps. No racer teleport catch-up exists. AI currently takes the safe road fallback; autonomous stunt flags remain unvalidated/false.

## Completed three-lap races

Both current races used20 traffic vehicles, the same roster/grid, route and settings. They ran alongside the route-validation player: their ~33.3ms frame timings are **not** the single-player performance benchmark. Historical0.6.0 races below had four traffic cars and ~16.7ms frames. Before/after totals are useful observed outcomes, not a controlled isolation of driver tuning from traffic/render timing. The fresh performance comparison uses one foreground player per run.

Times in seconds. A negative reference gap means the opponent finished ahead of the automated original car.

| Difficulty/profile | Laps1 /2 /3 | Race | Gap to reference | Misses/recoveries |
|---|---|---:|---:|---:|
| Normal original reference |145.186 /143.589 /146.827|438.139|0|0/0|
| Normal tourer |144.058 /143.097 /138.470|427.843|-10.296|0/0|
| Normal motorcycle |135.889 /124.912 /126.728|389.082|-49.057|0/0|
| Normal ATV |138.191 /146.838 /159.064|445.294|+7.155|0/0|
| Hard original reference |136.981 /141.921 /132.886|414.323|0|0/0|
| Hard tourer |139.698 /145.215 /134.240|421.369|+7.046|0/0|
| Hard motorcycle |129.447 /125.252 /138.325|394.576|-19.747|0/0|
| Hard ATV |134.912 /141.501 /132.325|409.937|-4.386|0/0|

The motorcycle won both current races. Normal's remaining winner gaps were38.761s tourer,49.057s original,56.212s ATV. Hard's were15.361s ATV,19.747s original,26.793s tourer. These gaps do not establish a human winning chance or prove Easy is comfortably winnable.

| Difficulty/profile | Old race,4 traffic | New race,20 traffic | Change | Old best lap | New best lap |
|---|---:|---:|---:|---:|---:|
| Normal original |445.884|438.139|-7.745|144.622|143.589|
| Normal tourer |436.412|427.843|-8.569|144.277|138.470|
| Normal motorcycle |396.873|389.082|-7.791|131.296|124.912|
| Normal ATV |446.272|445.294|-0.978|144.104|138.191|
| Hard original |429.212|414.323|-14.889|139.723|132.886|
| Hard tourer |417.092|421.369|+4.277|138.101|134.240|
| Hard motorcycle |380.484|394.576|+14.092|125.879|125.252|
| Hard ATV |429.666|409.937|-19.729|139.129|132.325|

The old Normal ATV additionally had one +5 miss (adjusted451.272); current all eight racers had no misses, DNFs or recoveries. Faster clean portions do not guarantee faster total races through denser traffic. Hard tourer/motorcycle total regressions are retained and disclosed. Normal ATV braking rose68.66→78.48s; Hard tourer braking fell53.42→49.48s while motorcycle rose55.18→57.20s. Full speeds/braking/laps are in `race-measurements.csv` and each run's `results.txt`/`pace.csv`.

## Sector evidence

Approximate first-lap Hard road-sector durations from0.25s telemetry. The headings describe the gate-to-gate interval; AI used the road, not the optional branch/jump. Start/finish timing remains the exact lap totals above.

| Profile | CP3→4 | CP6→7 | CP9→10 | CP13→14 |
|---|---:|---:|---:|---:|
| Original reference |8.28|8.00|6.12|5.34|
| Tourer |8.26|7.74|6.12|4.80|
| Motorcycle |6.14|7.74|5.34|4.28|
| ATV |7.20|8.80|5.88|4.52|

All Normal/Hard before/after sector transitions are retained in `sectors-CR034-039-{1,2}.csv` and `sectors-CR041-045-{1,2}.csv`. They expose individual slow sectors rather than treating eventual finish as proof of pace.

## Four-lane traffic

Population is16 dedicated highway vehicles plus4 local vehicles. Highway count is configurable0–24; a bounded pool reuses existing cars. Counts below refer to observed cars within road stations3760–4600, so they need not equal the configured dedicated pool at every instant.

| Measurement | Normal | Hard |
|---|---:|---:|
| Observed seconds after warmup |438.16|414.34|
| Highway population mean / min / max |11.49 /2 /17|11.68 /2 /17|
| Highway mean speed |26.72m/s|26.66m/s|
| Highway peak speed |28.686m/s|28.686m/s|
| Passes at station4100 |160|154|
| Passes/minute |21.91|22.30|
| Safe pool recycles |150|144|
| Closest player distance at recycling |220m|220m|
| Highway stopped samples |0.034%|0.065%|
| Minimum same-direction lane centre gap in core |26.45m|24.52m|
| Same-lane pair samples below5m |0 /27,642|0 /27,000|
| Traffic recoveries |0|0|

All four lanes had comparable sample coverage. The core gap measure uses stations3850–4500, same direction and lateral separation<=2.4m; it is not a claim that junction contacts are impossible. Brief occupancy dips remain because safe recycling yields to near-player/visible-space restrictions. No near-player spawn is used to hold an artificial constant count. Initial failed recycling traces are retained separately.

## Difficulty limits

Easy's driver constants are unchanged. Normal/Hard now use76/90% corner grip,88/98% braking envelope, full permitted cruising speed and32/35m/s steep-hill targets. Vehicle-specific grip, brakes, wheelbase and top speed continue to shape each profile. Hard should lose less time to cautious driving, but the human challenge and traffic feel still need Dan's playtest.

The final Easy one-lap check finished4/4 with no misses or automatic recoveries: original169.806s, tourer159.942s, motorcycle143.870s, ATV153.192s. Race totals were172.348/162.165/145.429/154.399s respectively. This confirms preserved slower driving, not human win probability.

The Hard one-lap deliberate mistake used three seconds of full braking at CP8 followed by a successful local recovery (`mistake.txt`: true, clock63.400, position4). Original lap144.212s versus136.981s on the clean race's first lap: +7.231s observed cost, with no miss charge or lap loss. Opponent laps were138.287/129.457/141.357s (tourer/motorcycle/ATV), race totals140.493/130.998/142.546s versus original146.737s. All opponents finished ahead. Traffic also changes these gaps, so the delta is not a deterministic recovery surcharge. The results file's recovery counter tracks driver-triggered automatic recovery and remains0; the explicitly requested successful local recovery is separately logged and must not be mistaken for zero recovery actions.

## Isolated foreground before/after lap

The final performance runs used one visible foreground player at a time,1280×720, VSync off and120fps cap, same Normal roster/grid. Old traffic population4, new20. Raw/adjusted race totals are separated below because the old ATV missed one gate; its recorded lap includes the5s penalty.

| Profile | Old recorded lap | New recorded lap | Old raw /adjusted race | New raw /adjusted race |
|---|---:|---:|---:|---:|
| Original |152.184|145.189|154.823 /154.823|147.724 /147.724|
| Tourer |145.447|144.060|147.669 /147.669|146.276 /146.276|
| Motorcycle |132.298|135.889|133.892 /133.892|137.441 /137.441|
| ATV |159.761|138.191|155.966 /160.966|139.391 /139.391|

Again, the motorcycle's first lap is slower in heavier traffic; later clear laps show the pace improvement. New run: no misses or recoveries for any racer. Median/p95 old8.33/8.35ms, new8.33/8.34ms; Windows peak working set482.2→480.6MiB. Both were capped at120fps, so this does not measure unlimited rendering capacity or guarantee other hardware performance. See the complete `performance-before` and `performance-after` evidence.
