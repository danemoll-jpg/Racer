READ FIRST:

C:\Users\danmo\Racer\CODEX\_RULES.md
C:\Users\danmo\Racer\PROJECT_TODO.md

Follow _RULES.md throughout this task.

==================================================
TASK:
BACKYARD REVERSE SHORTCUT CORRECTION PASS
==================================================

Dan gameplay-tested the two new shortcuts for:

DAN'S BACKYARD LOOP - REVERSE

The shortcut concepts and general locations are accepted.

However, BOTH implementations are currently underwhelming and require
substantial correction.

DO NOT redesign the main Backyard Reverse route.

DO NOT invent additional shortcuts.

Correct:

1. ABANDONED LOGGING RIDGE
2. STORM DRAIN / GULLY JUMP
3. AI support for BOTH shortcuts

==================================================
SHORTCUT 1:
ABANDONED LOGGING RIDGE
==================================================

AUTHORITATIVE ANCHORS:

START:
X=394.1
Y=81.6
Z=79.4

END:
X=281.3
Y=76.7
Z=124.9

Course:
Dan's Backyard Loop - Reverse

==================================================
CURRENT PROBLEM
==================================================

The current Logging Ridge does NOT match the intended design.

It currently looks and drives primarily like:

A NORMAL BROWN DIRT TRAIL THROUGH TREES.

It remains relatively level with the surrounding terrain for much of the
shortcut.

There is very little sense that the player is actually driving ON A RIDGE.

The shortcut lacks:

- elevation;
- exposure;
- terrain variation;
- meaningful fall-off risk;
- natural obstacles;
- small jumps/hops;
- visual personality.

Dan described it as underwhelming.

DO NOT merely decorate the existing flat trail.

The terrain/profile needs meaningful improvement.

==================================================
INTENDED RIDGE EXPERIENCE
==================================================

The shortcut should feel like an:

ABANDONED LOGGING TRAIL RUNNING ALONG A NATURAL ELEVATED RIDGE.

The ridge should become VISIBLY and MEANINGFULLY elevated above the nearby
forest floor.

The player should clearly perceive:

"I am driving along the top of a ridge."

The sides should fall away enough that leaving the top matters.

==================================================
RIDGE HEIGHT
==================================================

Increase the ridge elevation substantially compared with the current build.

Do this NATURALLY.

Do NOT create:

- vertical artificial walls;
- a giant rectangular dirt platform;
- a bridge disguised as terrain;
- a uniformly raised road ribbon.

Shape the surrounding terrain so the ridge has:

- a recognizable crest;
- sloped sides;
- natural height variation;
- forest below/alongside it.

The ridge does NOT need identical height for its entire length.

In fact, avoid that.

Let it:

CLIMB
CREST
ROLL
DIP
RISE AGAIN
DESCEND TOWARD REJOIN

where the existing terrain permits.

==================================================
RIDGE WIDTH
==================================================

The top should be:

- comfortably usable by an ATV;
- reasonably usable by a motorcycle;
- narrow enough that poor driving can send the player down the side.

Do NOT make it frustratingly narrow.

Do NOT make it so broad that it feels like an ordinary dirt road.

==================================================
RIDGE SURFACE
==================================================

The top should NOT look like one perfectly uniform brown ribbon from beginning
to end.

Give it an abandoned logging-trail character.

Use appropriate variation such as:

- exposed dirt;
- worn wheel paths;
- grass/ground intrusion;
- roots where supported;
- rocks;
- stumps;
- scattered logs;
- fallen timber;
- uneven terrain.

Do not create collision clutter that constantly stops the vehicle.

==================================================
RIDGE DRIVING CHARACTER
==================================================

This shortcut's identity is:

FAST
ELEVATED
TECHNICAL
ROUGH
WOODED

Add some SMALL NATURAL JUMPS/HOPS.

Examples:

- short terrain crest;
- exposed root/earth rise;
- small logging mound;
- modest drop;
- uneven ridge transition.

Do NOT turn this into another giant-ramp shortcut.

But do not make it completely flat.

This is off-road racing.

Some airtime is expected and encouraged.

==================================================
FALLING OFF
==================================================

If the player leaves the ridge:

NO AUTOMATIC RESET.

The player should generally descend into:

- rough forest terrain;
- slower vegetation;
- awkward terrain;

and be able to recover.

Falling off should COST TIME.

Do NOT create an easier/faster route along the bottom.

Use natural:

- trees;
- rocks;
- stumps;
- dense slow bushes;
- terrain;

to make staying on the ridge advantageous.

NO invisible walls.

==================================================
KEEP IT WOODED
==================================================

Do NOT clear the forest.

Trees should remain close enough to reinforce:

- height;
- speed;
- danger;
- abandoned logging-trail character.

Move/remove individual trees only where necessary for:

- legitimate driving clearance;
- visibility;
- safe landing/hop envelopes.

==================================================
RIDGE ENTRY / REJOIN
==================================================

Preserve the approximate anchors:

ENTRY:
X=394.1 Y=81.6 Z=79.4

REJOIN:
X=281.3 Y=76.7 Z=124.9

Transitions must be smooth.

No:

- sudden terrain lip;
- buried trail;
- floating trail;
- collision seam;
- abrupt speed-killing transition.

==================================================
SHORTCUT 2:
STORM DRAIN / GULLY JUMP
==================================================

ENTRANCE:

X=166.8
Y=63.8
Z=73.0

Course:
Dan's Backyard Loop - Reverse

NEW EXIT LOCATION:

X=114.3
Y=42.5
Z=-66.1

IMPORTANT:

Dan measured this coordinate while standing on SOLID GROUND ABOVE the desired
drain exit.

Therefore:

Y=42.5 IS NOT THE DESIRED TUNNEL-FLOOR HEIGHT.

Use:

X approximately 114.3
Z approximately -66.1

as the authoritative horizontal exit location.

Inspect the EXISTING GULLY at this location.

Determine the correct Y from the actual:

BOTTOM / LOWER SIDEWALL OF THE GULLY.

DO NOT raise or fill the gully to meet Y=42.5.

The tunnel should emerge naturally through the gully wall near the bottom.

==================================================
REMOVE OLD EXIT / JUMP ARRANGEMENT
==================================================

The previous drain exit near:

X=107.9
Y=38.8
Z=21.1

and the previous separate jump arrangement near:

X=97.3
Y=34.4
Z=-21.1

are no longer the desired shortcut design.

Replace/rework that arrangement as necessary.

The NEW concept is:

DRAIN ENTRANCE
->
LONGER UNDERGROUND DRAIN
->
GULLY-WALL EXIT NEAR X=114.3 Z=-66.1
->
SHORT STRAIGHT GULLY RUN-UP
->
BIG JUMP ACROSS / OUT OF GULLY
->
LAND / REJOIN.

Remove obsolete shortcut-specific geometry that no longer serves the new
design, provided it is not shared/protected world geometry.

==================================================
DRAIN ENTRANCE:
FIX THE LIP
==================================================

The current entrance has a severe transition/lip problem.

Dan reports that the lip is nearly impossible to cross except from a very
specific angle.

This is unacceptable.

The entrance should be usable at normal racing speed.

Inspect:

- terrain;
- tunnel floor;
- entrance slab;
- collision surfaces;
- overlapping geometry.

Create a SMOOTH CONTINUOUS TRANSITION from the approach into the drain.

The player should be able to:

SEE OPENING
->
AIM AT OPENING
->
DRIVE STRAIGHT IN.

No magic approach angle.

No hard collision lip.

No abrupt speed loss.

Test this specifically with BOTH motorcycle and ATV.

==================================================
FORWARD GRATE
==================================================

Preserve the intended race-specific behavior.

During:

DAN'S BACKYARD LOOP - FORWARD

the storm-drain entrance is CLOSED by the grate.

During:

DAN'S BACKYARD LOOP - REVERSE

the entrance is OPEN.

The grate must not interfere with Reverse collision when open.

AI must NEVER attempt the drain when the Forward grate is closed.

==================================================
MAKE THE TUNNEL LONGER
==================================================

The current tunnel is much too short.

The new exit near:

X=114.3
Z=-66.1

should naturally create a substantially longer underground section.

The tunnel should descend naturally from the entrance toward the actual
gully-bottom exit elevation.

It may:

- curve;
- descend;
- subtly change direction;
- change width slightly;

where useful.

It does NOT need to be a straight featureless pipe.

==================================================
THE TUNNEL IS CURRENTLY TOO BLAND
==================================================

The current interior failed to deliver the environmental character requested
in the original design.

Improve it substantially.

This should feel like an actual large storm-water drainage system.

==================================================
WATER
==================================================

Add SHALLOW WATER through meaningful portions of the drain.

The player should visibly understand that this is a drainage tunnel.

Prefer:

- shallow center flow;
- puddled/wet sections;
- drainage channel;
- intermittent water where appropriate.

Do NOT make the water deep enough to stop normal racing.

If an existing safe water/splash effect is available, use it appropriately.

Do NOT invent a large new physics system solely for water.

==================================================
TUNNEL DETAIL
==================================================

Add restrained environmental variation such as:

- side drainage pipes;
- concrete channels;
- culvert seams;
- occasional debris near edges;
- damp/wet surfaces;
- overhead grates;
- shafts/openings of daylight where geographically sensible;
- modest tunnel-width variation;
- occasional structural supports/details.

Keep the main racing line clear.

Do NOT place:

- hard debris directly in the racing line;
- collision snags;
- narrow choke points;
- decorative objects that flip vehicles.

The tunnel should feel interesting while still being FUN TO DRIVE.

==================================================
LIGHTING / VISUAL PROGRESSION
==================================================

Create a stronger sense of progression:

ENTRANCE LIGHT
->
DARKER UNDERGROUND SECTION
->
OCCASIONAL LIGHT / GRATES
->
VISIBLE DAYLIGHT AHEAD
->
GULLY EXIT.

Do not make the tunnel pitch-black.

The player must be able to read the driving line.

==================================================
PROTECT SURFACE ROUTES
==================================================

The drain passes beneath existing Forest-route geometry.

Existing Forest routes remain PROTECTED.

Do NOT:

- raise them;
- lower them;
- reroute them;
- flatten them;
- cut holes through them.

Ensure enough vertical separation that there is no:

- surface breakthrough;
- tunnel roof visible through trail;
- collision interaction;
- floating Forest route.

==================================================
NEW GULLY EXIT
==================================================

At approximately:

X=114.3
Z=-66.1

create the drain opening THROUGH THE EXISTING GULLY WALL near the actual
bottom.

The visual transition should be dramatic:

UNDERGROUND
->
DAYLIGHT
->
GULLY
->
JUMP.

Do not create a normal road exit onto the upper terrain.

==================================================
JUMP IMMEDIATELY AFTER EXIT
==================================================

Dan now wants the shortcut to transition fairly directly from the tunnel exit
into the gully jump.

Do NOT recreate the previous long separate exit-to-jump arrangement.

Provide a SHORT but sufficient straight run-up after leaving the tunnel.

The player needs enough distance to:

- see the ramp;
- straighten;
- accelerate;
- hit it centered.

Then:

BIG GULLY JUMP.

The ramp should be visually obvious from the tunnel exit.

==================================================
JUMP DESIGN
==================================================

This IS intended to be a substantial jump.

It should:

- clear the significant gully section;
- provide satisfying airtime;
- have a straight approach;
- work for motorcycle;
- work for ATV;
- have a usable landing envelope;
- flow naturally back toward the Reverse course.

Use the actual opposite-bank terrain to determine the best exact:

- ramp location;
- ramp angle;
- landing;
- rejoin.

Do NOT require pixel-perfect alignment.

Do NOT create a speed-killing ramp lip.

==================================================
CLEAR THE DAMN JUMP CORRIDOR
==================================================

The current jump has TREES directly in the flight/landing path.

This makes successful jumps end in trees.

That must be corrected.

Create and verify a PROTECTED JUMP CORRIDOR covering:

TAKEOFF
->
EXPECTED FLIGHT ENVELOPE
->
LANDING
->
IMMEDIATE RECOVERY/REJOIN.

Within that corridor:

remove or reposition trees, bushes, rocks or props that create unavoidable
collisions.

Do NOT clear the entire surrounding forest.

Clear ONLY what is needed for a reasonable jump envelope.

The player should not successfully clear the gully only to immediately smash
into an unavoidable tree.

==================================================
FAILED JUMP
==================================================

Failure remains recoverable.

If the player undershoots/falls into the gully:

NO AUTOMATIC RESET solely because the shortcut failed.

The gully should remain:

- slower;
- rougher;
- escapable.

Failure costs time.

Do not create a faster ground-level bypass.

==================================================
AI SUPPORT:
REQUIRED FOR BOTH SHORTCUTS
==================================================

The previous implementation reported these shortcuts as PLAYER-ONLY.

Dan does NOT want that.

These are legitimate race shortcuts.

RIVAL AI MUST BE CAPABLE OF USING BOTH.

Do NOT simply mark them player-only because supporting them requires localized
AI work.

==================================================
AI:
LOGGING RIDGE
==================================================

Add the localized:

- racing line;
- route branch;
- shortcut decision;
- recovery/rejoin guidance;

needed for AI to use the Logging Ridge.

AI choosing the ridge should be able to:

ENTER
->
STAY GENERALLY ON RIDGE
->
HANDLE SMALL HOPS
->
REJOIN.

It does not need to drive perfectly.

But it must not consistently:

- fall immediately;
- hit trees;
- become stuck;
- ignore the shortcut after selecting it.

==================================================
AI:
STORM DRAIN
==================================================

Add the localized guidance required for AI to:

ENTER DRAIN CLEANLY
->
DRIVE TUNNEL
->
HANDLE BENDS/DESCENT
->
EXIT INTO GULLY
->
ALIGN WITH RAMP
->
JUMP
->
LAND
->
REJOIN.

AI selecting the shortcut must not:

- hit the entrance lip;
- drive into walls repeatedly;
- miss the exit;
- turn sharply immediately before the jump;
- intentionally drive into trees in the landing corridor.

==================================================
AI SHORTCUT CHOICE
==================================================

Use the existing AI shortcut-choice architecture.

Do NOT globally rewrite AI.

The AI may probabilistically choose these shortcuts consistent with existing
shortcut behavior.

Do not force every rival to use them every race.

Do not give AI impossible perfect knowledge or artificial teleporting.

==================================================
FORWARD AI
==================================================

During Backyard Forward:

the grate is closed.

Forward AI must NOT select/attempt the Storm Drain shortcut.

Do not accidentally expose either Reverse shortcut as a Forward AI route
unless explicitly intended by existing course design.

==================================================
MAP / MINIMAP
==================================================

Preserve the established optional-shortcut presentation.

Both shortcuts should display as optional shortcut routes when relevant.

Update the Logging Ridge route geometry if its physical path changes.

Update the Storm Drain representation to match the longer underground route
and new exit.

Represent the underground section clearly enough that surface crossings are
not misleading.

Do NOT redesign the map UI.

Do NOT redesign the racing minimap.

==================================================
WORLD DEPENDENCY RULES
==================================================

Follow _RULES.md strictly.

After terrain modifications:

- trees must be grounded;
- bushes must be grounded;
- fences must be grounded;
- props must be grounded;
- structures must be supported.

No:

- floating trees;
- buried trunks from terrain raising;
- suspended props;
- unsupported tunnel geometry;
- floating roads;
- terrain holes around unrelated structures.

Inspect the affected corridor after terrain work.

==================================================
PROTECT APPROVED CONTENT
==================================================

Do NOT modify unrelated approved content.

Protect:

- Backyard Reverse main route;
- Backyard Forward;
- existing Forward shortcuts;
- dump;
- existing approved gully geometry outside necessary local shortcut work;
- Forest routes above tunnel;
- Mountain tracks;
- Street tracks;
- world landmarks;
- UI/menu system;
- Records;
- Playlists;
- music;
- vehicle physics;
- global AI behavior.

==================================================
TARGETED TESTING ONLY
==================================================

Dan will perform final gameplay review.

Do NOT over-test.

Test BOTH shortcuts using:

MOTORCYCLE
ATV
RIVAL AI.

--------------------------------------------------
LOGGING RIDGE
--------------------------------------------------

Verify:

- ridge is visibly elevated;
- surrounding terrain falls away;
- shortcut no longer looks like an ordinary flat dirt trail;
- top remains drivable;
- meaningful terrain variation exists;
- small hops work;
- forest character remains;
- falling off is recoverable but slower;
- no easier flat bypass;
- entry/rejoin smooth;
- motorcycle works;
- ATV works;
- AI can successfully use it.

--------------------------------------------------
STORM DRAIN
--------------------------------------------------

Verify:

- Reverse entrance is open;
- Forward grate is closed;
- entrance lip problem is gone;
- motorcycle can enter from normal racing line;
- ATV can enter from normal racing line;
- tunnel is substantially longer;
- shallow water is visibly present;
- tunnel contains environmental variation;
- racing line remains clear;
- no collision snags;
- surface Forest routes remain intact;
- exit occurs through gully wall near X=114.3 Z=-66.1;
- actual Y matches the gully-bottom geometry rather than Y=42.5;
- exit flows naturally toward jump;
- ramp approach is straight;
- flight corridor is clear of trees;
- landing corridor is clear;
- motorcycle can clear jump;
- ATV can clear jump;
- failed jump is recoverable;
- AI can enter, traverse, jump, land and rejoin.

--------------------------------------------------
AI
--------------------------------------------------

Run enough targeted AI attempts to confirm each shortcut is actually usable.

Do not accept:

"player only"

as the final result.

If a specific localized AI defect appears:

fix it.

Do not respond by disabling AI shortcut use.

Then STOP TESTING.

==================================================
PROJECT_TODO / DOCUMENTATION
==================================================

Update PROJECT_TODO.md with:

LOGGING RIDGE CORRECTION:
- increased ridge elevation;
- terrain/profile improvements;
- natural obstacles/hops;
- AI support;
- testing.

STORM DRAIN CORRECTION:
- entrance lip fix;
- new longer tunnel;
- shallow water/environmental treatment;
- new exit near X=114.3 Z=-66.1;
- gully-bottom Y derived from actual terrain;
- direct gully-jump sequence;
- cleared flight/landing corridor;
- AI support;
- testing.

Preserve the authoritative shortcut anchors.

Update Backyard atlas/map documentation for the revised shortcut geometry.

Follow _RULES.md for the normal source-changing delivery workflow.

This is a normal gameplay/world-content update and requires the standard
completed delivery.

Complete all standing delivery requirements, including:

- update PROJECT_TODO.md and relevant documentation;
- completion commit;
- push source;
- fresh Unity Windows build;
- update the complete Builds\Latest folder including the latest Racer.exe;
- publish the new GitHub release;
- update/verify the release catalog;
- verify Play-Racer.cmd resolves to the new build;
- clean disposable build artifacts according to standing cleanup rules.

Do not invent a second delivery process.

Do not ask Dan to perform steps that can be completed automatically.

If external authentication or permission genuinely requires Dan:

1. preserve all completed work;
2. tell Dan exactly what action is required;
3. wait for that action;
4. resume this SAME task afterward.

==================================================
FINAL REPORT
==================================================

Keep the final report concise.

Report:

LOGGING RIDGE
- how the ridge elevation/profile was changed;
- terrain variation and natural obstacles/hops added;
- how fall-off/recovery works;
- confirmation the forest character was preserved;
- motorcycle result;
- ATV result;
- rival AI result.

STORM DRAIN
- entrance/lip correction;
- final tunnel length/path treatment;
- water and environmental details added;
- confirmation existing Forest routes above remain protected;
- actual gully-exit placement near X=114.3 Z=-66.1;
- confirmation exit Y was derived from the actual gully-bottom geometry and
  NOT the measured Y=42.5;
- exit-to-jump approach;
- jump takeoff/landing treatment;
- confirmation trees/obstacles were cleared from the required flight and
  landing corridor;
- failed-jump recovery behavior;
- motorcycle result;
- ATV result;
- rival AI result.

AI
- confirmation BOTH shortcuts are available to rival AI;
- shortcut-choice behavior;
- any localized AI routing/branch changes required;
- confirmation Forward AI does not attempt the closed Storm Drain.

PROTECTION / REGRESSION
- Backyard Reverse main route remains intact;
- Backyard Forward remains intact;
- relevant Forest routes above/near the shortcuts remain intact;
- no floating/buried terrain-dependent objects;
- no unintended easier bypasses created.

MAP / DOCUMENTATION
- shortcut map/atlas updated;
- PROJECT_TODO.md updated;
- authoritative anchors retained.

DELIVERY
- completion commit;
- push status;
- build/release/version;
- Builds\Latest status;
- catalog status;
- Play-Racer.cmd verification;
- cleanup/disk usage.

Then STOP.

Do NOT begin another shortcut, track, UI, Records, map, or world-content task.

Wait for Dan's gameplay review.