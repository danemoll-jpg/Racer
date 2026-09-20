# CR-097–100 implementation

Reviewed baseline: 0.16.0-review3. Safety checkpoint: `acafa41ce76c410e4b970dbbd7c9cf14e487fc8e`, created before modifications after the supported elevated Git retry. Dan's review is not blanket acceptance of exploration, map, or other systems.

- Dedicated property correction generator operates on all four saved variants. House placement stays fixed; House 2 faces parallel to Dan's. Grounded segmented roadside fencing, two Dan openings, and connecting side/rear boundaries are authored together. Removal of Kyle's three named fence sections is also fixed in the old fence generator.
- House 3 is the second neighbor, under Rocky Way Acres. A continuous curved gravel drive descends south through the valley to its front yard, with matching collidable terrain and a turning apron. The sign and house remain in place.
- Summit uses a continuous increasing takeoff grade, a higher lip, and a wider supported landing/rollout. No handling changes or boost. Physical signs and teal flags identify the east approach and westbound launch; the map landmark is discovery gated. Existing northern return connects to the original ridge trail.
- A flat-footed dome mesh, crossed arched poles, entrance, and seams replace the spherical tent. Both seated campers and fire remain. The brown props were collectible clue cairns: three irregular gray stones now suggest hidden woodland pockets. Stable acorn IDs and the hidden pickups remain.
- The exact approved PNG is embedded in Resources/Title with full-image aspect fitting and a separate prompt matte. Source MP3s are preserved in Assets/TitleSource; dedicated PCM title clips are embedded separately from the radio library. Voice is supplied audio, not TTS. Voice starts once, music is ducked then loops. Master volume applies through the existing listener.
- Startup defers radio attachment until a deliberate released-then-fresh keyboard/mouse/gamepad button and consumes the press through release. Track changes and menu/race returns do not replay the title. Async title loading is canceled on skip. No extra listener is created.

Validation and limitations are recorded separately. Temporary test mute and isolated saves are not release defaults. No mountain race, split screen, multiplayer, or external upload.

The final road apron samples the actual road/shoulder colliders across nine transverse vertices. Terrain mesh colliders are explicitly recooked after generator edits. Fence grounding samples both terrain and road support. The retained fence-smash activity is rebound to a clear, nearly level 14-panel run after regeneration; its ID, target, timing and records remain unchanged. Rebinding was necessary because the old component referenced replaced fence objects.

Final compiled assembly SHA256: `3118C2702EC5EF37BD83C4DBA8279B9F04192237F593DB16941BD6B7EA36F8B3`. The final addition to this assembly is a test-only launch-from-rest option; normal vehicle handling and jump acceptance rules are unchanged.
