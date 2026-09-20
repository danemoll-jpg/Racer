# Marked activity inventory

Each of the four variants contains two speed-camera sites and three scored jump sites. Site IDs are stable; course/layout, vehicle and travel direction keep their records separate. Unmarked terrain flights can show generic clean-jump feedback but do not create a marked site's PB.

| ID | Street / Street Reverse | Forest / Forest Reverse | Direction |
|---|---|---|---|
| speed-0 | Trickum / 92 speed trap 1 | Forest speed trap 1 | Both; separately recorded |
| speed-1 | Trickum / 92 speed trap 2 | Forest speed trap 2 | Both; separately recorded |
| jump-01 | Trickum pavement jump | Forest opening jump | Authored course direction |
| mountain-creek | Fern Creek Leap | Fern Creek Leap | Marked mountain ascent |
| mountain-ridge | High Ridge Drop | High Ridge Drop | Marked ridge approach |

Both sides of each camera have physical signs. Mountain jump signs give a 35–55 mph intended approach. Speed-camera detection uses the site's plane and lateral radius; jump detection selects the nearest authored site at a forward takeoff and waits for a valid settled landing. Sign text and physics coordinates are not used to grant race gates.

The existing Fence line smash challenge remains separate and optional. It is not part of the Speed Traps/Jumps top-ten tabs.

Actual mountain ground profiles, normals and lateral samples are recorded in `authored-ramp-geometry-*.csv`. The final `release3` directories retain ordinary physics inputs, speeds, contacts, roll/stability and reset outcomes for the authored ramps. Opposite approaches and true shoulder edges are explicitly stress cases; they are not all clean or scored. See VALIDATION.md for the results and README-player.txt for the return-trail guide.
