# Woodstock Rush balance sheet

Version 0.112.0-review1, written 2026-10-12 03:33. Every current tuning value and where it lives (paths under Assets/Scripts). Read only: nothing here changes the game.
"inline" = a number written inside the code at that place (copied here); the rest are read from the code's own fields.

## Rival AI pace and skill per difficulty

| Value | Easy | Normal | Hard | Where |
|---|---|---|---|---|
| Corner speed use (share of grip) | 0.46 | 0.76 | 0.9 | RoadDriver.cs cornerUse |
| Brake use | 0.55 | 0.88 | 0.98 | RoadDriver.cs brakeUse |
| Top speed use | 0.91 | 1 | 1 | RoadDriver.cs speedUse |
| Hill target speed (m/s) | 26 | 32 | 35 | RoadDriver.cs hillTargets |
| Consistency lift (target speed dips up to) | 7 % | 1.2 % | none | RoadDriver.cs (inline, TargetSpeed *= 1 - ...) |
| Crest speed constant | 6.5 | 7.6 | 8.2 | RoadDriver.cs (inline, hillSpeed) |
| Throttle gain | 0.5 | 0.8 | 0.8 | RoadDriver.cs (inline, throttle) |
| Judgment error length (s) | 3.2 | 2.6 | 2 | DriverVariation.cs (inline) |
| Judgment error strength | 1 | 0.65 | 0.4 | DriverVariation.cs (inline) |
| Seconds between errors (+ 0-25 random) | 18 | 30 | 48 | DriverVariation.cs (inline) |
| Estimated finish skill factor | 0.80 | 1 | 1.08 | AiFinishEstimate.cs (inline) |

- Each rival's own pace: 0.94 + 0.025 × its slot (slot 0 = 0.94 … slot 4 = 1.04), RaceDirector.cs (inline, driver.Initialize). The results log records each rival's pace.
- Error effects: wide line 0.65, jump line 0.28, early braking −30 % (DriverVariation.cs, inline). Estimated finish pace: profile speed × 0.36 (forest) / 0.46, AiFinishEstimate.cs (inline).
- **AI catch-up / rubber band in races: none** (RoadDriver: no speed multipliers or positional driving). Only the shortcut choice depends on place and gap: probability = clamp(0.12 + 0.34 × rank share + 0.12 × clamp01(gap / 200 m) + risk ±0.04, 0.06, 0.65), ShortcutStrategy.cs (inline).
- Race timeouts: 1200 s × max(1, laps / 3) for the race, 90 s after the first finisher (RaceDirector.cs). Missed gate: +5 s.
- Weather grip (share of dry grip; car / tourer / moto / ATV ... as WeatherGrip.cs): rain 0.85 / 0.78 / 0.75 / 0.35, snow 0.7 / 0.65 / 0.62 / 0.35.

## Campaign: rivals and difficulty per chapter

### Chapter 1: Street Loop

| Event | Id | Kind | Course | Laps | Rivals | Difficulty | Rival pace | Conditions | Time limit (s) | Targets bronze / silver / gold | Pay (1st / gold) | Medal pay b / s / g | Bonus | Prize |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| Against the Clock | c1-against-clock | TimeTrial | Street Loop - Forward | 1 | — | Easy | — | Day / Clear | — | 150 / 140 / 134 | $1000 | $400 / $650 / $1000 | $500 |  |
| First Lap | c1-first-lap | Race | Street Loop - Forward | 2 | Rusty Vance (Street Classic), Mia Torres (Pebble Coupe), Big Ed Kowalski (Sundown Roadster) | Normal | 1.15 | Day / Clear | — | — (race: top 3 passes, 1st wins) | $1200 | — | $600 |  |
| Hwy 92 Speed Trap | c1-hwy92-trap | SpeedTrap | Street Loop - Forward | 1 | — | Easy | — | Day / Clear | 45 | 34 / 39 / 44 | $1000 | $400 / $650 / $1000 | $500 |  |
| Night Shift | c1-night-shift | Race | Street Loop - Forward | 3 | Mia Torres (Sundown Roadster), Big Ed Kowalski (Drifter Twin), Rusty Vance (Longroof GT) | Normal | 1.15 | Night / Clear / traffic | — | — (race: top 3 passes, 1st wins) | $1400 | — | $700 |  |
| Practice: Street Loop Reverse | c1-practice-reverse | TimeTrial | Street Loop - Reverse | 1 | — | Easy | — | Day / Clear | — | 180 / 155 / 138 | $500 | $200 / $320 / $500 | $250 |  |
| Wrong Way Round | c1-wrong-way | Race | Street Loop - Reverse | 2 | Rusty Vance (Pebble Coupe), Mia Torres (Skyfin Cruiser), June Park (Trail Four) | Normal | 1 | Dusk / Rain | — | — (race: top 3 passes, 1st wins) | $1400 | — | $700 |  |
| Street Loop Final | c1-street-final | Race | Street Loop - Forward | 3 | June Park (Trail Four), Rusty Vance (Longroof GT), Mia Torres (Sundown Roadster), Hollis Gray (Pebble Coupe), Big Ed Kowalski (Drifter Twin) | Normal | 1.12 | Day / Clear | — | — (race: top 3 passes, 1st wins) | $2500 | — | $1500 | moto |

### Chapter 2: Forest Loop

| Event | Id | Kind | Course | Laps | Rivals | Difficulty | Rival pace | Conditions | Time limit (s) | Targets bronze / silver / gold | Pay (1st / gold) | Medal pay b / s / g | Bonus | Prize |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| Cave Run | c2-cave-run | TimeTrial | Forest Loop - Forward | 1 | — | Easy | — | Day / Clear | — | 76 / 69 / 64 | $1300 | $520 / $840 / $1300 | $650 |  |
| Into the Woods | c2-into-woods | Race | Forest Loop - Forward | 2 | Rusty Vance (Longroof GT), Mia Torres (Pebble Coupe), June Park (Trail Four) | Normal | 1 | Day / Clear | — | — (race: top 3 passes, 1st wins) | $1500 | — | $750 |  |
| The Opening Jump | c2-opening-jump | Jump | Forest Loop - Forward | 1 | — | Easy | — | Day / Clear | 120 | 30 / 60 / 110 | $1300 | $520 / $840 / $1300 | $650 |  |
| Rain in the Pines | c2-rain-pines | Race | Forest Loop - Forward | 3 | Rusty Vance (Skyfin Cruiser), Big Ed Kowalski (Drifter Twin), June Park (Trail Four) | Normal | 1 | Dusk / Rain | — | — (race: top 3 passes, 1st wins) | $1700 | — | $850 |  |
| Practice: Forest Loop Reverse | c2-practice-reverse | TimeTrial | Forest Loop - Reverse | 1 | — | Easy | — | Day / Clear | — | 90 / 76 / 68 | $650 | $260 / $420 / $650 | $325 |  |
| The Long Way Back | c2-long-way-back | Race | Forest Loop - Reverse | 2 | Rusty Vance (Sundown Roadster), Big Ed Kowalski (Drifter Twin), June Park (Trail Four) | Normal | 1 | Day / Clear | — | — (race: top 3 passes, 1st wins) | $1700 | — | $850 |  |
| Forest Final | c2-forest-final | Race | Forest Loop - Forward | 3 | Big Ed Kowalski (Drifter Twin), Rusty Vance (Longroof GT), June Park (Trail Four), Mia Torres (Needle 600), Hollis Gray (Skyfin Cruiser) | Normal | 1 | Day / Clear | — | — (race: top 3 passes, 1st wins) | $2800 | — | $1400 | pebble |

### Chapter 3: Dan's Backyard

| Event | Id | Kind | Course | Laps | Rivals | Difficulty | Rival pace | Conditions | Time limit (s) | Targets bronze / silver / gold | Pay (1st / gold) | Medal pay b / s / g | Bonus | Prize |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| Round the Yard | c3-backyard-clock | TimeTrial | Dan's Backyard Loop - Forward | 1 | — | Easy | — | Day / Clear | — | 72 / 64 / 59 | $1600 | $640 / $1040 / $1600 | $800 |  |
| Backyard Dash | c3-backyard-dash | Race | Dan's Backyard Loop - Forward | 2 | Mia Torres (Needle 600), June Park (Trail Four), Big Ed Kowalski (Drifter Twin) | Normal | 1 | Day / Clear | — | — (race: top 3 passes, 1st wins) | $1800 | — | $900 |  |
| Practice: Backyard Reverse | c3-practice-reverse | TimeTrial | Dan's Backyard Loop - Reverse | 1 | — | Easy | — | Day / Clear | — | 95 / 82 / 72 | $800 | $320 / $520 / $800 | $400 |  |
| Two Wheels Only | c3-two-wheels | Race | Dan's Backyard Loop - Reverse | 2 | Mia Torres (Needle 600), Big Ed Kowalski (Drifter Twin), June Park (Trail Four) | Normal | 1 | Day / Clear | — | — (race: top 3 passes, 1st wins) | $2000 | — | $1000 |  |
| Snow Day | c3-snow-day | Race | Dan's Backyard Loop - Forward | 2 | June Park (Trail Four), Mia Torres (Needle 600), Rusty Vance (Longroof GT) | Hard | 1 | Day / Snow | — | — (race: top 3 passes, 1st wins) | $2000 | — | $1000 |  |
| Fence Line Smash | c3-fence-smash | Smash | Dan's Backyard Loop - Forward | 1 | — | Easy | — | Day / Clear | 45 | 3 / 6 / 10 | $1600 | $640 / $1040 / $1600 | $800 |  |
| Backyard Final | c3-backyard-final | Race | Dan's Backyard Loop - Forward | 3 | Mia Torres (Needle 600), Big Ed Kowalski (Drifter Twin), June Park (Trail Four), Rusty Vance (Longroof GT), Hollis Gray (Highball Fastback) | Hard | 1 | Day / Clear | — | — (race: top 3 passes, 1st wins) | $3400 | — | $1700 | scrambler |

### Chapter 4: Mountain Loop

| Event | Id | Kind | Course | Laps | Rivals | Difficulty | Rival pace | Conditions | Time limit (s) | Targets bronze / silver / gold | Pay (1st / gold) | Medal pay b / s / g | Bonus | Prize |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| Summit Clock | c4-summit-clock | TimeTrial | Mountain Loop - Forward | 1 | — | Easy | — | Day / Clear | — | 134 / 124 / 118 | $2000 | $800 / $1300 / $2000 | $1000 |  |
| First Ascent | c4-first-ascent | Race | Mountain Loop - Forward | 2 | Hollis Gray (Ridge Scrambler), June Park (Trail Four), Rusty Vance (Longroof GT) | Hard | 1.15 | Dawn / Clear | — | — (race: top 3 passes, 1st wins) | $2200 | — | $1100 |  |
| Practice: Mountain Loop Reverse | c4-practice-reverse | TimeTrial | Mountain Loop - Reverse | 1 | — | Easy | — | Day / Clear | — | 160 / 140 / 125 | $1000 | $400 / $650 / $1000 | $500 |  |
| Downhill | c4-downhill | Race | Mountain Loop - Reverse | 2 | Mia Torres (Needle 600), Hollis Gray (Ridge Scrambler), Big Ed Kowalski (Drifter Twin) | Hard | 1.15 | Day / Clear | — | — (race: top 3 passes, 1st wins) | $2400 | — | $1200 |  |
| Whiteout | c4-whiteout | Race | Mountain Loop - Forward | 2 | June Park (Trail Four), Hollis Gray (Ridge Scrambler), Rusty Vance (Highball Fastback) | Hard | 1.15 | Night / Snow | — | — (race: top 3 passes, 1st wins) | $2600 | — | $1300 |  |
| Summit Homeward Flight | c4-summit-flight | Jump | Mountain Loop - Forward | 1 | — | Easy | — | Day / Clear | 150 | 90 / 140 / 200 | $2000 | $800 / $1300 / $2000 | $1000 |  |
| Summit Final | c4-summit-final | Race | Mountain Loop - Forward | 3 | Mia Torres (Needle 600), Hollis Gray (Ridge Scrambler), Big Ed Kowalski (Drifter Twin), Rusty Vance (Highball Fastback), June Park (Trail Four) | Hard | 1.15 | Day / Clear | — | — (race: top 3 passes, 1st wins) | $4200 | — | $2100 | fastback |

### Championships

| Cup | Id | After chapter | Rounds | Rivals | Difficulty | Pay | Bonus |
|---|---|---|---|---|---|---|---|
| Street Cup | cup-street | 1 | Street Loop - Forward ×2 Day / Clear; Street Loop - Reverse ×2 Dusk / Clear; Street Loop - Forward ×3 Day / Clear | Mia Torres (Pebble Coupe), Big Ed Kowalski (Sundown Roadster), Hollis Gray (Skyfin Cruiser), Rusty Vance (Longroof GT), June Park (Trail Four) | Normal | $4000 | $2000 |
| Woodland Cup | cup-woodland | 2 | Street Loop - Forward ×2 Day / Clear; Forest Loop - Forward ×2 Day / Clear; Street Loop - Reverse ×2 Night / Clear; Forest Loop - Reverse ×2 Dusk / Rain | Rusty Vance (Longroof GT), Big Ed Kowalski (Drifter Twin), June Park (Trail Four), Mia Torres (Sundown Roadster), Hollis Gray (Skyfin Cruiser) | Normal | $6000 | $3000 |
| Backyard Cup | cup-backyard | 3 | Street Loop - Forward ×2 Day / Clear; Forest Loop - Forward ×2 Dawn / Clear; Dan's Backyard Loop - Forward ×2 Day / Clear; Street Loop - Reverse ×2 Night / Clear; Forest Loop - Reverse ×2 Day / Rain; Dan's Backyard Loop - Reverse ×2 Day / Snow | Mia Torres (Needle 600), Big Ed Kowalski (Drifter Twin), June Park (Trail Four), Rusty Vance (Longroof GT), Hollis Gray (Ridge Scrambler) | Hard | $8000 | $4000 |
| Woodstock Grand Championship | cup-grand | 4 | Street Loop - Forward ×2 Day / Clear; Forest Loop - Forward ×2 Day / Clear; Dan's Backyard Loop - Forward ×2 Dusk / Clear; Mountain Loop - Forward ×2 Dawn / Clear; Street Loop - Reverse ×2 Night / Rain; Forest Loop - Reverse ×2 Day / Snow; Dan's Backyard Loop - Reverse ×2 Day / Clear; Mountain Loop - Reverse ×2 Dusk / Clear | Mia Torres (Needle 600), Big Ed Kowalski (Drifter Twin), Rusty Vance (Highball Fastback), Hollis Gray (Ridge Scrambler), June Park (Trail Four) | Hard | $12000 | $6000 |

- Rival pace (0.112): a fixed factor per campaign race on the rivals' road speed and bend / hill limits (not on jump, run-up or shortcut speed limits, not on obstacle braking), at most 1.15; 1 = as before; championships 1 (CampaignData.cs Pace, RoadDriver.cs push). No rubber band.
- Chapter 4 rivals (campaign events, not championships) take a Mountain shortcut whenever it is faster for them (rough time on the shortcut at its speed limits against the bypassed road at 0.6 × top speed): the AI-validated ones only (Mountain Forward's Climbing Ridge Cut and Summit Traverse are not validated: rivals needed a reset every lap there) (CampaignRun.cs, RoadDriver.cs DecideShortcut, ShortcutStrategy.TryDecideFaster). Elsewhere the random choice above.
- Pay by place: 1 / 0.6 / 0.4 / 0.25 / 0.15 / 0.1 of first; medal shares 0 / 0.4 / 0.65 / 1; replays pay 0.5; cup points per place 10 / 7 / 5 / 3 / 2 / 1 (CampaignData.cs / Campaign.cs).
- Pass rules: a race passes in the top 3 and wins in 1st; other events pass at bronze and win at gold (Campaign.cs Commit). Time trials: lower is better.

## Practice laps

Practice laps are ordinary campaign time trials (one flying lap from the run-up) named "Practice: …" (ids contain -practice-), with lower pay and generous targets; required in the chapter order, optional under the legacy order (Campaign.cs LegacyAvailable). They are listed in the chapter tables above:

| Practice | Chapter | Course | Run-up (m) | Targets (s) | Pay |
|---|---|---|---|---|---|
| Practice: Street Loop Reverse | 1 | Street Loop - Reverse | 150 | 180 / 155 / 138 | $500 |
| Practice: Forest Loop Reverse | 2 | Forest Loop - Reverse | 150 | 90 / 76 / 68 | $650 |
| Practice: Backyard Reverse | 3 | Dan's Backyard Loop - Reverse | 150 | 95 / 82 / 72 | $800 |
| Practice: Mountain Loop Reverse | 4 | Mountain Loop - Reverse | 150 | 160 / 140 / 125 | $1000 |

## Medal targets outside the campaign

- Quick races, playlists and split-screen races: no medals (records and Top 10 only). Getaway, Cop vs Runner and Speed Patrol: no medals.
- **Stunt event** (10 minutes): bronze / silver / gold 20,000 / 50,000 / 90,000 points (StuntProgress.cs Targets). Gold earns the Skywhip 250.
  Trick points (ArcadeActivities.Stunts.cs): flip 300, roll 250, spin 150, loop 500, barrel roll 300; combo = points × links (at most 9); chain within 5 s; a trick needs 0.6 s air and 4 m; landing recovery window 1.5 s.

### Free Roam activities (scene data on each ActivitySite; per-vehicle targets for car / tourer / motorcycle / ATV where set)

| Scene | Site | Kind | Bronze / silver / gold | Per vehicle (car / tourer / moto / ATV) | Attempt seconds |
|---|---|---|---|---|---|
| StreetLoopGreybox | Trickum pavement jump (jump-01) | Jump | 20 / 45 / 75 m | — | 180 |
| StreetLoopGreybox | Fern Creek Leap (mountain-creek) | Jump | 5 / 14 / 25 m | — | 45 |
| StreetLoopGreybox | High Ridge Drop (mountain-ridge) | Jump | 5 / 14 / 25 m | — | 45 |
| StreetLoopGreybox | Fence line smash (smash-01) | Smash | 3 / 6 / 10 props | — | 8 |
| StreetLoopGreybox | Trickum / 92 speed trap 1 (speed-0) | Speed | 20 / 28 / 36 m/s | — | — (always armed) |
| StreetLoopGreybox | Trickum / 92 speed trap 2 (speed-1) | Speed | 20 / 28 / 36 m/s | — | — (always armed) |
| StreetLoopGreybox | Summit Homeward Flight (summit-homeward) | Jump | 30 / 65 / 100 m | — | 45 |

Authored in the editor scripts (Assets/Editor: ArcadeReviewRelease.cs, DiscoveryAuthoring.cs, CR110Mountain.cs, ExplorationAuthoring.cs) and the scene files.

## Vehicle unlock goals

| Vehicle | Id | Class | How to get it | Where |
|---|---|---|---|---|
| Street Classic | original | Car | owned from the start | CampaignData.Starters |
| Longroof GT | tourer | Car | Buy it in the Shop for $10,000 | CampaignData.Prices ($10,000, chapter 1) |
| Needle 600 | moto | Motorcycle | Prize: pass "Street Loop Final" (campaign chapter 1) | CampaignData.Events Prize (c1-street-final) |
| Trail Four | atv | ATV | owned from the start | CampaignData.Starters |
| Sundown Roadster | roadster | Car | Buy it in the Shop for $7,000 | CampaignData.Prices ($7,000, chapter 1) |
| Highball Fastback | fastback | Car | Prize: pass "Summit Final" (campaign chapter 4) | CampaignData.Events Prize (c4-summit-final) |
| Pebble Coupe | pebble | Car | Prize: pass "Forest Final" (campaign chapter 2) | CampaignData.Events Prize (c2-forest-final) |
| Skyfin Cruiser | skyfin | Car | Buy it in the Shop for $8,000 | CampaignData.Prices ($8,000, chapter 1) |
| Ridge Scrambler | scrambler | Motorcycle | Prize: pass "Backyard Final" (campaign chapter 3) | CampaignData.Events Prize (c3-backyard-final) |
| Drifter Twin | drifter | Motorcycle | In the Shop from chapter 2: $16,000 | CampaignData.Prices ($16,000, chapter 2) |
| Turf Rocket | mower | Mower | Find all 27 Woodland Acorns (0/27) | VehicleUnlocks.AcornsNeeded |
| Patrol Car | police | Car | Escape the police at heat 3+ (Normal or Hard) | PoliceProgress (HeatNeeded, SpeedersNeeded) |
| Patrol Cycle | policebike | Motorcycle | Catch 20 speeders in Speed Patrol (0 / 20) | PoliceProgress (HeatNeeded, SpeedersNeeded) |
| Clay Hopper | stuntbuggy | Buggy | free (Free Roam and the Stunt event only) | VehicleUnlocks (always earned) |
| Skywhip 250 | stuntbike | Motorcycle | Win a Gold medal (90,000 pts) in the Stunt event (Free Roam and the Stunt event only) | StuntProgress.Targets (gold) |
| Sundown Roadster Cup | roadstercup | Car | Prize: win the Street Cup (campaign championship after chapter 1) | VehicleProfile Cup edition of roadster (+5%), the cup-street prize (Campaign.CommitCupRace) |
| Drifter Twin Cup | driftercup | Motorcycle | Prize: win the Woodland Cup (campaign championship after chapter 2) | VehicleProfile Cup edition of drifter (+5%), the cup-woodland prize (Campaign.CommitCupRace) |
| Ridge Scrambler Cup | scramblercup | Motorcycle | Prize: win the Backyard Cup (campaign championship after chapter 3) | VehicleProfile Cup edition of scrambler (+5%), the cup-backyard prize (Campaign.CommitCupRace) |
| Needle 600 Champion | motochampion | Motorcycle | Prize: win the Woodstock Grand Championship (campaign championship after chapter 4) | VehicleProfile Cup edition of moto (+5%), the cup-grand prize (Campaign.CommitCupRace) |

- Acorns: 27 (VehicleUnlocks.cs). Patrol car: escape at heat 3+ on Normal or Hard (PoliceProgress.cs; the difficulty rule inline). Patrol cycle: 20 speeders caught. Stunt motorcycle: Stunt event gold (90,000).
- Upgrades: 3 levels; step per level Top speed 0.03, Acceleration 0.06, Grip 0.05, Handling 0.06; cost share of the vehicle's value 0.1 / 0.18 / 0.28 (CampaignData.cs).

## Police chase pacing (Getaway; GetawayChase.cs)

| Value | Easy | Normal | Hard | Where |
|---|---|---|---|---|
| Most cops at once | 4 | 6 | 6 | CopCap |
| Seconds per heat level while chased | 45 | 30 | 20 | HeatEvery |
| Seconds unseen to escape | 20 | 30 | 40 | EscapeBy |
| Bust meter rate | 0.8 | 1 | 1.25 | BustRate |
| Extra heat for a ram / roadblock run / 10 s off-road seen | no | yes | yes | inline |

| Heat level | What happens |
|---|---|
| 1 | start: min(most cops, 2) cops 62-78 m and 120 m+ behind (inline); backup checks every 4 s: wanted near cops = min(most cops, heat + 1), placed 250-450 m ahead; total cap most cops + 4 (inline) |
| 2 | as above with one more wanted cop |
| 3 | roadblocks every 38 s (at most 2, 240-420 m ahead, lifted after 80 s); every third new unit a police bike (inline). Heat 3+ escape earns the patrol car |
| 4-5 | helicopter (from heat 4, inline); heat 5 is the top |

- Constants: sight 120 m, near cops 150 m, catch within 10 m under 7 m/s, box speed 18, contact speed 22, pin 20 m, held 15 m/s within 20 m, runner release 3 s, end pause 4 s, reset hold 2 s.
- Bust meter fill per second by situation: 1/3, 1/6, 1/9, 1/6, 1/12, 1/6; drain 1/3 (inline). Round lengths 3 / 5 / 8 minutes (RaceMenus.Getaway.cs); a hidden-police chase 300 s.
- Cop speed: every cop copies the runner's top speed and acceleration (at least its grip and braking); catch-up boost beyond 150 m: 1 + 0.3 × clamp01((distance − 80) / 220) until within 80 m, at most 1.3 (inline: GetawayChase Boosts, CopDriver). Cop corner grip min(grip × 0.62, 14), braking max(6, braking × 0.75) (CopDriver.cs, inline).
- Helicopter: delay 2.4 s, altitude 58 m, cone 21 m, speed 62 m/s (PoliceHelicopter.cs).
- Hidden police in Free Roam: sees 70 m, over the limit by 10 mph, quiet 60 s at start and 180 s after a chase (HiddenPolice.cs).
- Cop vs Runner: catch within 10 m under 7 m/s for 3 s; the cop starts 3 s later, 80 m behind (PoliceChase.cs).
- Speed Patrol: radar 80 m / 10°, pursuit 30 m, speeders get away after 60 s, at most 3 at once; points: catch 100 + 10 per mph over, hit −50, no violation −25 (SpeedPatrol.cs); speeders 10-40 mph over, one every 20-30 s (inline).

## Vehicles

| Vehicle | Id | Class | Speed | Acceleration | Grip | Response | Mass |
|---|---|---|---|---|---|---|---|
| Street Classic | original | Car | 49 | 14.5 | 25 | 8 | 1200 |
| Longroof GT | tourer | Car | 52 | 13.5 | 24 | 6.5 | 1500 |
| Needle 600 | moto | Motorcycle | 61 | 18 | 32 | 12 | 220 |
| Trail Four | atv | ATV | 56 | 17 | 30 | 10.5 | 340 |
| Sundown Roadster | roadster | Car | 50 | 14.8 | 26 | 9 | 1000 |
| Highball Fastback | fastback | Car | 52 | 16 | 24 | 6.8 | 1400 |
| Pebble Coupe | pebble | Car | 49 | 14 | 27.5 | 8.8 | 950 |
| Skyfin Cruiser | skyfin | Car | 50.5 | 13 | 23 | 6 | 1750 |
| Ridge Scrambler | scrambler | Motorcycle | 56 | 17.5 | 31 | 11.5 | 200 |
| Drifter Twin | drifter | Motorcycle | 58 | 16 | 30 | 9.5 | 320 |
| Turf Rocket | mower | Mower | 61 | 18 | 32 | 12 | 760 |
| Patrol Car | police | Car | 55 | 16.5 | 26 | 8.2 | 1550 |
| Patrol Cycle | policebike | Motorcycle | 58 | 17.5 | 31 | 11.5 | 240 |
| Clay Hopper | stuntbuggy | Buggy | 50 | 17.5 | 29 | 10.5 | 520 |
| Skywhip 250 | stuntbike | Motorcycle | 55 | 18 | 31 | 11.5 | 190 |
| Sundown Roadster Cup | roadstercup | Car | 52.5 | 15.54 | 27.3 | 9.45 | 1050 |
| Drifter Twin Cup | driftercup | Motorcycle | 60.9 | 16.8 | 31.5 | 9.975 | 336 |
| Ridge Scrambler Cup | scramblercup | Motorcycle | 58.8 | 18.375 | 32.55 | 12.075 | 210 |
| Needle 600 Champion | motochampion | Motorcycle | 64.05 | 18.9 | 33.6 | 12.6 | 231 |

(VehicleProfile.cs)
