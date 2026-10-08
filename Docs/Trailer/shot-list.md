# Woodstock Rush — trailer shot list

A 50 to 55 second trailer built from 18 short clips. Updated for 0.97 (2026-10-08) from the 0.77 list: the game now has eleven vehicles and the Turf Rocket mower, rider characters with looks, Kyle's house and mailbox, the campaign, split-screen, and the police modes (Police Chase, Speed Patrol, Getaway with its helicopter). Claude Code films it itself: the game plays each shot and renders it offline (see "How it is filmed"). Claude (chat) will copy this file back to the Project doc `claude/trailer-shot-list.md`.

## How it is filmed (0.97)

- A trailer capture mode (development builds and the editor only, not in the release menus): `Tools/Report097/Run-Trailer.ps1`. It sets the place, time, weather, moon, vehicle, colour and rider look, drives the vehicle (the game's AI on the road, or a recorded path for jumps), places the Trailer Mode camera, and records offline at a fixed 60 frames a second (`Time.captureFramerate = 60`), every frame at full quality: 3840×2160 if it renders, else 2560×1440. Slow motion is rendered as real slow motion (more frames), never stretched.
- Game sound is recorded with the frames (engine, rain, thunder); the radio is off.
- Each shot is its own clip, a few seconds longer than the edit uses. Three takes of anything with a jump.
- Output: `Trailer/Clips/NN-name.mp4`, the first cut `Trailer/WoodstockRush-trailer-v1.mp4` (game sound) and `-silent.mp4` (no sound, for scoring), stills in `Trailer/Stills/`. Nothing is committed to git.

## Shots

| # | What | Where | Camera | Conditions | In the edit |
|---|---|---|---|---|---|
| 1 | Empty misty road; the Needle 600 enters frame and rides away | A straight on the road loop with trees | Fixed (5) | Dawn, Clear | 4 s. Quiet opening |
| 2 | The rider being customized: hat, shirt, hair changing | Garage, Rider page | Rotate the preview | n/a | 3 s, three or four quick cuts |
| 3 | The eleven vehicles and the Turf Rocket turning, one after another | Garage, vehicle screen | Rotate the preview | n/a | 3 s, fast cuts |
| 4 | Race start, the pack pulls away | Any course grid, full AI field | Side tracking (3), low | Day, Clear | 3 s. Music kicks in |
| 5 | Fast run past houses and fences | Road loop | Chase (1) | Day, Clear | 3 s |
| 6 | Over the handlebars at speed | Road loop or a trail | First person (9) | Day, Clear | 2 s |
| 7 | Smashing through a fence or mailboxes | Fence-line smash area | Side tracking (3) | Day, Clear | 2 s |
| 8 | Into the storm-drain tunnel, headlight on the walls | Storm drain | Chase (1), then Front (4) | Night, Clear | 4 s |
| 9 | Out through the dump and gullies with a small jump | Backyard area | Orbit (2) | Dusk, Clear | 3 s |
| 10 | The Turf Rocket mower cutting across Kyle's lawn past the mailbox | Kyle's house and driveway | Side tracking (3) | Day, Clear | 2 s. A laugh |
| 11 | Lightning bolt over the mountain, rider small in frame | A road facing the mountain | Fixed (5), wide | Night, Rain; lightning on cue | 3 s. Hold on the flash |
| 12 | Wet road, rain streaks, tail light; the grip loosening | Any road | Chase (1), camera low | Dusk, Rain | 2 s |
| 13 | The sledders go by as the bike passes on a white road | The sledding hill | Fixed (5) | Day, Snow | 3 s |
| 14 | Broom hockey on the frozen pool, slow orbit | The pool at Dan's house | Free camera (7) | Day, Snow | 2 s |
| 15 | Police: cruisers with lights and the helicopter's spotlight on a runner | Hwy 92 / the loop, Getaway | Chase (1), then Flyover (6) | Night, Clear | 3 s. Lights, siren, searchlight |
| 16 | Split-screen: two riders side by side on the same road | Road loop | Both views | Day, Clear | 2 s |
| 17 | THE GIANT JUMP: run-up, take-off, the whole flight | Summit giant jump | Side tracking (3) for the run-up; Flyover (6) or Fixed (5) from below for the flight | Dusk, Clear, full moon | 6 to 7 s. Slow motion from take-off. The music's peak |
| 18 | COVER SHOT: a car and the ATV airborne over the crest toward camera, golden light, the house on the left | The road crest that looks back at Dan's house, as in the cover art | Fixed (5) planted low just past the crest, looking back up it | Dusk, Clear, clock paused with the sun low behind | 4 s in slow motion. The title WOODSTOCK RUSH appears over this |

End card: the cover artwork itself, 3 seconds.

Total in the edit: about 52 s plus the end card.

## Notes on shot 18 (matching the cover art)

- The art shows two vehicles in the air at once. Free Roam has only the player, so this is a race on a course that runs over that crest with an AI field of ATVs, and the player in the blue car. The AI is paced so both leave the ground together.
- The Fixed camera is planted first, low and close to the road, then the vehicles drive back over the crest toward it. Slow motion from take-off; a full-resolution still at the peak.
- If the crest does not launch the car enough at racing speed, that is reported, not fixed: a small bump there could be added so the cover moment exists in the game.

## Structure

- 0 to 10 s: calm opening (1), then the garage (2, 3).
- 10 to 24 s: racing and the neighbourhood (4 to 10).
- 24 to 38 s: the world in different light and weather, and the law (11 to 16).
- 38 to 52 s: the giant jump (17), the cover shot (18), the title.

## Notes

- If a rival or traffic car blocks a shot, the take is redone.
- Not available in Free Roam: the Forest cave and the Mountain race roads. Shot 17 (and the crest of shot 18) are filmed in their race scenes with Trailer Mode on.
- Split-screen (16) and the garage (2, 3) are not Trailer Mode scenes; see the run report for how each was filmed.

## Music

Use a track you have the right to use (royalty-free, or your own). Something that starts calm, lifts at about 10 seconds and peaks around 38 seconds fits this structure. Drop it into `SourceArt/Trailer/Music/` and the cut is re-made with it; with no file there the first cut is game sound only, with a silent version for scoring.
