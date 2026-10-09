# Woodstock Rush — police radio voice script

For Dan to record. Written 2026-10-09 from the radio lines the game uses in 0.98 (Getaway, Cop vs Runner, Speed Patrol) plus the lines the next round adds (hidden police, police bike). The game builds most calls from pieces: a phrase, then a place or road. Record each piece on its own and the game joins them, so "Suspect off-road heading toward" + "Moll's" plays as one call.

## How to record

- One file per line, named by its ID: `R01.wav`, `P07.wav`, and so on. WAV or MP3, any sample rate, mono is fine.
- Plain voice, close to the microphone, no effects. The game adds the radio sound itself (narrow "walkie-talkie" tone, a click and a little static at the start and end).
- Leave about half a second of silence before and after each line. The game trims it.
- Phrase pieces that end with a place: say them as if a place follows, without dropping your voice at the end ("Suspect off-road, heading toward…").
- Place and road names: say them as a complete ending ("…Moll's.").
- Two or three takes of the common lines are welcome (name them `R05a`, `R05b`): the game picks one at random so it sounds less repetitive.
- Put the files in `SourceArt/Audio/PoliceRadio/` in the project. Missing files fall back to text only, so you can record a few at a time.

## Whole lines (Getaway)

| ID | Line | When |
|---|---|---|
| R01 | Dispatch, unit in pursuit. Suspect on the road. | Chase starts |
| R02 | Visual on the suspect. | A cop sees you again |
| R03 | Suspect in custody. | Caught |
| R04 | We lost the suspect. | You escaped |
| R05 | Time's up. The suspect is gone. | The round clock ran out |
| R06 | Air support inbound. Eyes on the suspect. | Helicopter joins (heat 4) |
| R07 | Air one, I have the suspect in the light. | Helicopter spots you |
| R08 | Air one, lost them under the trees. | Helicopter loses you |
| R09 | The suspect got through the roadblock. | You passed a roadblock |
| R10 | Units covering the exits. | Cops sent to trail exits (no place known) |
| R11 | Suspect rammed a unit! | You hit a cop |
| R12 | All units, all units, suspect still at large. | Heat 5 |
| R13 | Backup requested. | Heat 2 |
| R14 | Requesting roadblocks and more units. | Heat 3 |
| R15 | Every unit responding. | Heat 5 |
| R16 | Suspect is running cross-country. | Off-road for a while while seen |
| R17 | Pull over! Pull over now! | Loudspeaker, cop close behind (occasional) |
| R18 | Stop the vehicle! | Loudspeaker, alternative |

## Phrase pieces (a place or road follows)

| ID | Piece | Example |
|---|---|---|
| P01 | Lost visual. Last seen near… | …the lake road. |
| P02 | Suspect off-road, heading toward… | …Moll's. |
| P03 | Suspect back on the road near… | …Hwy 92. |
| P04 | Units posted at the exits near… | …the Campsite. |
| P05 | …and… (short joiner, for two places) | …the Campsite and McFadden's. |
| P06 | Roadblock set up on… | …the Summit road. |
| P07 | …ahead of the suspect. (ending, after the road) | |
| P08 | …joining from… (after the unit number) | Unit three, joining from Trickum Road. |
| P09 | …rejoining from… | |
| P10 | Speeder heading toward… (hidden police and Speed Patrol) | …the Highway market. |

## Unit numbers

| ID | Say |
|---|---|
| U1–U6 | "Unit one" … "Unit six" (six files: U1.wav to U6.wav) |
| U7 | "Air one" |

## Places (the map's named places)

| ID | Say |
|---|---|
| L01 | Moll's |
| L02 | the lake gateway |
| L03 | the ridge overlook |
| L04 | the Highway market |
| L05 | Trickum woods |
| L06 | the summit run-up |
| L07 | Roger's |
| L08 | McFadden's |
| L09 | Anderson's |
| L10 | the campsite |
| L11 | the storm drain |
| L12 | South Cherokee Lane |
| L13 | the Hwy 92 speed trap |
| L14 | the pool house |
| L15 | Kyle's house |
| L16 | the lake |

## Roads

| ID | Say |
|---|---|
| D01 | Hwy 92 ("Highway ninety-two") |
| D02 | the Street Loop |
| D03 | the Summit road |
| D04 | the mountain trails |
| D05 | the lake road |
| D06 | a driveway |
| D07 | the forest trails |
| D08 | the back roads |
| D09 | the hills |
| D10 | Trickum Road |

## Speed Patrol (you are the cop; dispatch talks to you)

| ID | Line | When |
|---|---|---|
| S01 | Dispatch to all units, speeders reported in the area. | Round starts |
| S02 | Clocked him. | Radar catches a speeder |
| S03 | Speeder's running! | Speeder flees |
| S04 | Suspect pulled over. Nice work. | Catch |
| S05 | Speeder got away. | Escape |
| S06 | Watch it, that was a civilian. | You hit traffic |
| S07 | No violation, let them go. | Wrong pull-over |
| S08 | Time's up, finish your chase. | Clock ends |

## Hidden police in Free Roam (next round)

| ID | Line | When |
|---|---|---|
| H01 | Unit in position, watching the road. | Optional, when you pass close to a hidden cop |
| H02 | Got a speeder! In pursuit. | A hidden cop clocks you and starts a chase |
| H03 | Suspect's gone. Back to patrol. | You escaped; Free Roam carries on |
| H04 | Suspect in custody. | Caught; Free Roam carries on |

## Notes

- About 85 short files in all; the most important dozen are R01–R06, P01–P03, U1–U3, and the places you go most.
- If you would rather each place be a full sentence ("Suspect heading toward Moll's"), say so; it sounds smoother but needs about 50 more files.
