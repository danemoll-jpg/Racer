# Trailer / Photo Mode, camera views and screenshots (0.77, on screen since 0.79)

Woodstock Rush can film itself. **Trailer Mode** gives a clean screen, extra cameras, slow motion, an automatic
director and (in Free Roam) the time of day and weather on demand. It is also a photo mode. Separately, ordinary play
has four **camera views**, including first person.

## Camera views in ordinary play

Press **V** (keyboard) or **X** (controller) while driving to cycle the views. The choice is saved.

Since 0.79 you do not need to remember this:

- A small line above the speedometer, such as `V / X: camera — Chase`, appears for a few seconds when driving starts
  and whenever the view changes, then fades.
- The pause menu (races and Free Roam) has a **Camera view** row that cycles the views.
- **Settings > Controls** lists the binding.

| View | What it shows |
| --- | --- |
| Chase | The normal camera (the default). |
| Far chase | Further back and higher: more of the road and of big jumps. |
| First person | From the rider's eyes (motorcycle and ATV: over the bars, the rider's head hidden; cars: from the driver's seat through the windscreen). |
| Front | From the front of the vehicle with no bodywork in view. |

Races count for records in every view. During a wipeout or a reset, first person and front ease out to the chase view
and come back when the vehicle is upright again.

## Trailer / Photo Mode

**F8** turns it on and off. It is also on the pause menu (races and Free Roam) and the main menu as
**TRAILER / PHOTO MODE (F8)**. That page has every Trailer Mode setting, so a controller player can reach everything.

### The controls panel (0.79)

When Trailer Mode starts, a panel at the top left lists everything:

- the nine cameras by number and name, with the current one highlighted;
- the camera action and Auto hold;
- slow motion;
- hide HUD and panel, the arrows / gates / beacon, and the screenshot;
- in Free Roam, time of day, clock −1 h / +1 h, clock pause, weather, moon and lightning.

Each entry shows its key, or its controller button when a controller is in use.

- **Mouse:** click any entry. The pointer shows while the panel is on screen.
- **Controller:** **B** gives the panel the focus: the D-pad chooses, **A** uses, **B** returns to driving. Meanwhile
  the D-pad and A do not work the cameras.
- **H** (L3) hides the panel together with the HUD, for a clean recording; press it again to bring both back.
  **F1** shows or hides the panel on its own.
- While the panel is hidden, a small `H: show controls` line appears for a couple of seconds when you press a key
  other than the driving controls, then fades.
- Screenshots (P / F12 / R3) render the camera alone. The panel, the line and the HUD never appear in them.

- Nothing is drawn on screen while driving: HUD, speedometer, minimap, lap box, Free Roam text, waypoint line,
  notifications, radio song and the debug panel are all hidden. Menus and the map still appear when opened.
- The course arrows, checkpoint gates and the waypoint beacon are hidden too. Turn them back on with **G**.
- Turning it off puts everything back as it was.
- It stays on across scene changes (for example, starting a race from Free Roam) until it is turned off.
- **Records:** a race run in Trailer Mode does not count for records, the same rule as Debug movement. This includes a
  race that was started in Trailer Mode. Free Roam is unaffected.

### Keyboard

| Key | Action |
| --- | --- |
| F8 | Trailer Mode on / off |
| 1 – 9 | Camera: 1 Chase, 2 Orbit, 3 Side tracking, 4 Front, 5 Fixed, 6 Flyover, 7 Free camera, 8 Auto, 9 First person |
| Tab, or the same number again | Camera action: Fixed re-plants beside the road ahead, Side switches sides, Auto skips to the next shot |
| Left Shift (hold) | Auto: keep the current shot |
| Z (hold) | Slow motion 0.25× |
| X | Slow motion 0.5× on / off |
| − / = | Field of view wider / narrower |
| Page Up / Page Down | Camera distance (closer / further) |
| Home / End | Camera height (higher / lower) |
| H | HUD and the controls panel off / on together |
| G | Arrows, gates and waypoint beacon on / off |
| P or F12 | Screenshot |
| F | Shake a fist (0.78; the rider's gesture, also outside Trailer Mode) |
| F1 | Controls panel on / off |

**Free camera (7):** W A S D move, Q / E down / up, right mouse button to look, Shift fast, Ctrl precise. The
vehicle's own controls are off while the free camera is in use.

**Free Roam conditions** (Trailer Mode only):

| Key | Action |
| --- | --- |
| T | Time of day: Dawn → Day → Dusk → Night |
| , / . | Clock −1 hour / +1 hour |
| K | Pause / run the clock |
| B | Weather: Clear → Rain → Snow |
| O | Moon phase (eight phases) |
| L | Lightning strike now (switches to Rain if needed) |

### Controller

| Button | Action |
| --- | --- |
| Start > Trailer / Photo Mode | On / off, plus every setting below and the conditions |
| B | Give the controls panel the focus (D-pad choose, A use, B leave) |
| D-pad ← / → | Previous / next camera (Auto is in the cycle) |
| A | Camera action (Fixed re-plant, Side switch, Auto next shot) |
| X (hold) | Auto: keep the current shot |
| LB (hold) | Slow motion 0.25× |
| RB | Slow motion 0.5× on / off (outside Trailer Mode RB shakes a fist; in Trailer Mode use F) |
| D-pad ↑ / ↓ | Field of view narrower / wider |
| Right stick | Camera distance (left / right) and height (up / down) |
| R3 | Screenshot |
| L3 | HUD and the controls panel off / on together |

**Free camera:** left stick to move, right stick to look, RT / LT for up / down.

In Trailer Mode the D-pad drives the cameras, not the radio. The radio keys [ ] I N still work.

### Cameras

| Camera | Behaviour |
| --- | --- |
| Chase | The normal camera, but smoother, a little lower and wider. |
| Orbit | Circles the vehicle slowly. Distance and height are adjustable. |
| Side tracking | Low, alongside the vehicle. The action key switches sides. |
| Front | Ahead of the vehicle, looking back at the rider. |
| Fixed | Stays where it is and turns to follow the vehicle as it rides past. The action key re-plants it beside the road just ahead. |
| Flyover | High, slow and drifting over the vehicle. |
| Free camera | The detached inspection camera, smoothed, without the debug overlay. It cannot pass through terrain or buildings. |
| Auto | The game directs itself (see below). |
| First person | As in ordinary play. |

- All camera moves are smoothed. Choosing a camera by hand blends to it.
- Cameras are kept in front of terrain and walls.
- Field of view, distance and height changes last until Trailer Mode is turned off.

### Auto

Auto cuts between Chase, Orbit, Side tracking, Front, Fixed, Flyover and, sparingly, First person, while you just ride.

- Each shot is held for 4–8 seconds, and the same camera never comes twice in a row.
- It prefers Fixed when there is a clear spot beside the road ahead, and Side tracking or Orbit on straights.
- At a jump it cuts as the vehicle takes off (to Flyover or a low Fixed view beside the flight) and holds that shot
  until the vehicle has landed.
- It never cuts mid-air or during a wipeout.
- It checks the line of sight (terrain, trunks, buildings) before choosing a shot, and leaves a shot that becomes
  blocked.
- Cuts are instant. The action key skips to the next shot; holding Left Shift or X keeps the current one.
- Slow motion and the conditions controls work while Auto runs.

### Slow motion

- 0.25× while Z / LB is held, or 0.5× toggled with X / RB.
- It eases in and out over about 0.3 s.
- The physics step shrinks with it, so motion stays smooth.
- The game's sound is turned down while slowed rather than pitched down.

### Conditions (Free Roam)

- Time of day, clock, weather, moon phase and lightning are set instantly, by key or on the pause menu page.
- Nothing is written to the save.
- Turning Trailer Mode off restores the saved Free Roam clock, calendar and weather exactly as they were.
- In races, time and weather are chosen at Race Setup as before.

### Screenshots

- **P**, **F12**, **R3** or **pause menu > Trailer Mode > Take screenshot** saves a PNG of the camera's view at the
  window's full resolution, with no HUD or menus.
- Files go to `Screenshots`, beside `DebugReports`: `%USERPROFILE%\AppData\LocalLow\DefaultCompany\Racer\Screenshots`.
- **Pause menu > Trailer Mode > Open Screenshots Folder** opens that folder.

## Recording with OBS

1. Capture the game window, either with Game Capture or Window Capture on `Racer.exe`.
2. Press F8.
3. Pick a camera, or 8 for Auto, and ride.
4. Use Z / LB for slow-motion moments.


## Rider gestures (0.78)

- **Shake a fist:** F on the keyboard, RB on a controller (in Trailer Mode RB keeps its 0.5× toggle, so use F). The rider raises the left fist and shakes it three times, then takes the bars again; in the cars the driver's fist goes out of the side window. A short cooldown stops spamming. Seen in first person the fist comes up in front of you. Needs Model: New.
- **AI riders** shake a fist at whoever just ran into them (or after recovering from a wipe-out someone caused), now and then, not on every bump.
- **Victory:** the winner of a race (player or AI) raises both fists and pumps them as they cross the line; in a car, a fist pumped out of the window.


## Free Roam screen (0.79)

- **Day and clock:** top left, on their own (with the moon phase). They hide with the rest of the HUD in Trailer Mode.
- **Activities:** the name of an activity and how to start it appear only while you are at its start. During an attempt,
  the timer line shows; afterwards, the result shows for a few seconds. The nearest activity is on the map and in the
  pause menu.
- **Acorns:** `WOODLAND ACORNS n/24` shows for a few seconds when you find one. The total is in the pause menu.
- `Esc or Start: activities, retry, menu` shows for a few seconds when Free Roam begins.
- **Minimap:** the same minimap as in races, showing roads, trails, activity sites and the waypoint.
  **J** (keyboard) or **B** (controller) turns it on / off while driving. The choice is saved; the default is on. In
  Trailer Mode it hides with the HUD, and B gives the controls panel the focus instead.
