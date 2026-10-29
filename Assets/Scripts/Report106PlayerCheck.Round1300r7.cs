using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Racer
{
    // 1.3.0-review7 evidence only (Report106PlayerCheck: -p106check <dir> -racerTestSave <dir>, a copy of Dan's save, 1280x800):
    //   -p106kind lev1300r7     Leviathan Reverse (course 9) and Forward (course 8), a normal race and a Stunt Race (-p1300courses, -p1300types):
    //                           the road AI to the stretch Dan named, then the pad as a player (steering down the course ahead, slowing for the
    //                           bends; when it stalls it backs up and tries again slower, never the AI) through it: Reverse from CP 38 (the end
    //                           of the mountain pass) to CP 44; Forward from CP 8 (Climbing Ridge Cut's junction, the Eastbound Gully Flight)
    //                           to CP 10, then from CP 13 (the end of its mountain) to CP 18. Chase-camera shots (Dan's camera) at Dan's photo 1
    //                           spot, every turn on those stretches, the cut junction and the landing, each with what the camera shows: the next
    //                           gate (in the frame, not hidden), chevrons, scene arrows, ROAD CLOSED boards on the route. Reverse normal race:
    //                           once into the J1 rail on purpose, at speed. Every gate, missed gate, TURN BACK, barricade touched.
    //   -p106kind ai1300r7      an AI Stunt Race (3 rivals, the player's car on the road AI too) on -p119course: each racer's missed gates, resets,
    //                           stuck, closed shortcut taken, and any racer within 1.5 m of the J1 rail or a barricade
    public sealed partial class Report106PlayerCheck
    {
        IEnumerator Lev1300r7()
        {
            foreach (var m in InputSystem.devices.OfType<Mouse>().ToList()) InputSystem.DisableDevice(m);
            Campaign.Testing = true; DriverVariation.Seed = 1307; Conditions();
            foreach (var cs in Arg("-p1300courses", "9,8").Split(','))
                foreach (var type in Arg("-p1300types", "normal,stunt").Split(','))
                {
                    int course = int.Parse(cs); yield return QuickTo115(course); Conditions(); bool stunt = type == "stunt"; if (race.stunts != stunt) flow.StepRaceType(1);
                    var st = flow.Save.Settings; st.opponents = false; st.traffic = false; st.laps = 1; flow.Save.SaveSettings(); race.opponents = false; race.traffic = false; race.laps = 1;
                    flow.OpenGarage(); flow.SelectVehicle("original"); flow.CloseGarage(); flow.StartRace(); yield return WaitRacing(); Bind();
                    Note($"== {race.courseName} ({race.courseId}), {(race.stunts ? "Stunt Race" : "normal race")}: line {race.road.Length:F0} m, {race.gates.Length} gates; record course {race.RecordCourse}; camera {(CameraViews.Current ? CameraViews.Current.PlayerView.ToString() : "?")} (Dan's save's view)");
                    Note($"   shortcuts: {string.Join(", ", race.AllBranches.Select(b => $"{b.title} (s {b.entryRoad:F0}-{b.exitRoad:F0}, bypasses {string.Join("/", b.bypassedGates)}){(race.Branches.Contains(b) ? " open" : " CLOSED")}"))}");
                    foreach (var line in StuntShortcuts.Report) Note("   closed for the Stunt Race: " + line);
                    Note($"   flights: {string.Join(", ", race.GetComponent<MountainFlights>().flights.Select(f => $"{f.name} s {f.approachStation:F0}-{f.endStation:F0}"))}; mountain s {LeviathanCourse.Current.mountain[0]:F0}-{LeviathanCourse.Current.mountain[1]:F0}");
                    Note($"   scene arrows hidden: {LeviathanCourse.ArrowReport.Count} ({LeviathanCourse.TealHidden} of them Mountain Loop's teal ones)");
                    if (type == "normal") foreach (var a in LeviathanCourse.ArrowReport.Where(a => a.Contains("teal"))) Note("     " + a);
                    if (type == "normal") DescribeBlocks1300r7();
                    if (course == 9) yield return Stretch1300r7(type, 38, 44, true);
                    else { yield return Stretch1300r7(type, 8, 10, false); yield return Stretch1300r7(type, 14, 18, false); }
                    flow.QuitRace(); yield return WaitMenu();
                }
        }
        // every barricade / rail in the race and how far it stands from the race line's edge (and an open shortcut's)
        void DescribeBlocks1300r7()
        {
            var holders = race.gameObject.scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Transform>(true)).Where(t => t.name.StartsWith("Leviathan road closed (")).ToList();
            int on = 0;
            foreach (var h in holders)
            {
                float clear = float.MaxValue;
                foreach (var c in h.GetComponentsInChildren<BoxCollider>())
                {
                    var p = c.bounds.center; float rs = race.road.Project(p, out float lat); var q = race.road.At(rs, out _);
                    if (Mathf.Abs(q.y - p.y) < 6) clear = Mathf.Min(clear, Mathf.Abs(lat) - race.road.HalfWidth(rs) - c.bounds.extents.magnitude * .5f);
                }
                if (clear < 0) on++;
                if (clear < 4 || h.name.Contains("J1") || h.name.Contains("1.3.0-review7")) Note($"     {h.name.Replace("Leviathan road closed ", "")} at {V(h.position)}: {(clear == float.MaxValue ? "not near the line" : $"{clear:0.0} m clear of the line's edge")}{(clear < 0 ? "   <-- ON THE ROUTE" : "")}");
            }
            Note($"   barricades and rails in this race: {holders.Count}, on the route: {on}");
        }
        // what the chase camera shows: a point in its frame and not hidden by the world
        bool Seen1300r7(Vector3 p)
        {
            var cam = Camera.main; if (!cam) return false; var v = cam.WorldToViewportPoint(p);
            if (v.z < 1 || v.x < .02f || v.x > .98f || v.y < .02f || v.y > .98f) return false;
            return !Physics.Linecast(cam.transform.position, p, out var hit, ~0, QueryTriggerInteraction.Ignore) || hit.collider.attachedRigidbody || Vector3.Distance(hit.point, p) < 2;
        }
        string View1300r7(RacerState me)
        {
            int next = me.Progress.NextGate; var g = race.gates[next]; var cam = Camera.main;
            var pts = new[] { g.transform.position + Vector3.up * 1, g.transform.TransformPoint(new Vector3(-g.halfWidth, 2, 0)), g.transform.TransformPoint(new Vector3(g.halfWidth, 2, 0)) };
            int seen = pts.Count(Seen1300r7); float dist = Vector3.Distance(Flat(cam.transform.position), Flat(g.transform.position));
            var chev = FindObjectsByType<Transform>(FindObjectsSortMode.None).Where(t => t.name.StartsWith("Leviathan chevron board") || t.name.StartsWith("Leviathan turn arrow board")).Where(t => Vector3.Distance(t.position, cam.transform.position) < 140 && Seen1300r7(t.position + Vector3.up * 1.6f)).Select(t => t.name.Replace("Leviathan ", "")).Distinct().ToList();
            var closed = FindObjectsByType<Transform>(FindObjectsSortMode.None).Where(t => t.name == "Road closed sign" && t.gameObject.activeInHierarchy && Vector3.Distance(t.position, cam.transform.position) < 120 && Seen1300r7(t.position + Vector3.up * 2)).Select(t => t.parent.name.Replace("Leviathan road closed ", "")).ToList();
            var onRoute = closed.Where(n => { var t = FindObjectsByType<Transform>(FindObjectsSortMode.None).FirstOrDefault(x => x.name == "Leviathan road closed " + n); if (!t) return false; float rs = race.road.Project(t.position, out float lat); return Mathf.Abs(lat) < race.road.HalfWidth(rs) - .5f; }).ToList();
            return $"next CP {next} {dist:0} m away: {(seen > 0 ? $"IN VIEW ({seen}/3 points)" : "not in view")}; chevrons in view: {(chev.Count == 0 ? "none" : string.Join(", ", chev))}; ROAD CLOSED in view: {(closed.Count == 0 ? "none" : string.Join(", ", closed))}{(onRoute.Count > 0 ? "  <-- ON THE ROUTE: " + string.Join(", ", onRoute) : "")}; HUD arrow {Arrow(race.vehicle, me):+0;-0}°; {ArrowsInView1300r6(race.vehicle)}";
        }
        // the pad as a player from gate `from` (next) until gate `until` is next: steer at the course ahead, slow for bends; a stall backs up and
        // tries again slower; never the road AI (it drives only before `from`)
        IEnumerator Stretch1300r7(string type, int from, int until, bool reverse)
        {
            var me = race.Racers[0]; var car = race.vehicle; var g = race.GetComponent<LeviathanGuide>(); Pilot(car, me); var pilot = car.GetComponent<RoadDriver>(); var vi = car.GetComponent<VehicleInput>();
            var flights = race.GetComponent<MountainFlights>()?.flights ?? new MountainFlights.Flight[0];
            race.finishGraceSeconds = 100000; bool padOn = false; int lastNext = me.Progress.NextGate, lastTb = -1, missed0 = me.Progress.MissedGates, stalls = 0, resets = 0; float slow = 1, stuck = 0, backUntil = -1, lastShotS = -999, lastReset = -999; int diag = 0; bool railDone = !(reverse && type == "normal");
            var shots = new HashSet<string>(); string tag = $"{(reverse ? "R" : "F")}-{type}";
            var panels = race.gameObject.scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<BoxCollider>(true)).Where(c => c.name == "Barricade collision" && c.gameObject.activeInHierarchy).ToList(); var touched = new HashSet<string>();
            var rail = race.gameObject.scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Transform>(true)).FirstOrDefault(t => t.name.StartsWith("Leviathan road closed (J1"));
            float t0 = Time.time; Time.timeScale = 3; Note($"-- {tag}: the road AI until CP {from} is next, then the pad as a player to CP {until}");
            // the turns on this stretch: where the course turns 35 degrees or more within 40 m (the shot is taken 25 m before)
            var turns = new List<float>(); float s0 = from < race.gates.Length ? race.GateStation(from - 1) : 0, s1 = race.GateStation(Mathf.Min(until, race.gates.Length - 1));
            for (float s = s0; s < s1; s += 5) { race.road.At(s, out var fa); race.road.At(s + 40, out var fb); if (Vector3.Angle(Flat(fa), Flat(fb)) > 35 && (turns.Count == 0 || s - turns[turns.Count - 1] > 60)) turns.Add(s); }
            Note($"   turns on the stretch (s): {string.Join(", ", turns.Select(x => x.ToString("F0")))}");
            while (Time.time - t0 < 1800 && (flow.State == RaceFlow.Stage.Racing || flow.State == RaceFlow.Stage.Paused) && !me.Progress.Finished && me.Progress.NextGate != until)
            {
                yield return null; AudioListener.volume = 0; if (Time.timeScale == 0) Time.timeScale = padOn ? 1 : 3;
                if (flow.State == RaceFlow.Stage.Paused) { Note($"   paused (page {Page}): resumed"); flow.Resume(); yield return null; continue; }
                int next = me.Progress.NextGate; var p = car.Body.position; float station = Mathf.Repeat(LeviathanGuide.Station(race, me), race.road.Length); float speed = car.Body.linearVelocity.magnitude;
                if (next != lastNext) { if (padOn) Note($"   {Time.time - t0:0} s: CP {lastNext} passed (next {next}){(me.Progress.MissedGates > missed0 ? $", MISSED {me.Progress.MissedGates - missed0}" : "")} at {V(p)}, {speed * 2.237f:0} mph"); missed0 = me.Progress.MissedGates; lastNext = next; slow = 1; }
                if (!padOn && next == from) { padOn = true; Time.timeScale = 1; pilot.enabled = false; BindPad(); Note($"   {Time.time - t0:0} s: CP {from} next: the pad from {V(p)} (s {station:F0})"); }
                if (!padOn) continue;
                bool flying = flights.Any(f => station > f.approachStation - 150 && station < f.endStation + 10); // the road AI takes its run-up too
                if (flying) { if (!pilot.enabled) { Note($"   {Time.time - t0:0} s: the road AI flies the jump (s {station:F0})"); pilot.enabled = true; vi.enabled = false; } stuck = 0; continue; }
                // the road AI drives the body itself and switches the car's own driving off each step: on again for the pad
                if (pilot.enabled || !vi.enabled || vi.Device != pad || !car.enabled) { if (diag++ < 5) Note($"   (pad control: road AI {pilot.enabled}, input {vi.enabled}, device {vi.Device?.name}, car driving {car.enabled}: the pad's)"); pilot.enabled = false; car.enabled = true; BindPad(); }
                if (g.TurnBackGate != lastTb) { if (g.TurnBackGate >= 0) Note($"   TURN BACK to CP {g.TurnBackGate} ({g.TurnBackWhy}) at {V(p)}"); lastTb = g.TurnBackGate; }
                foreach (var c in panels) if (c && c.bounds.SqrDistance(p) < 2.5f * 2.5f && touched.Add(c.transform.parent.parent.name)) Note($"   BARRICADE within 2.5 m: {c.transform.parent.parent.name.Replace("Leviathan road closed ", "")} at {V(p)}, {speed * 2.237f:0} mph");
                // the shots, from the chase camera
                IEnumerator Shot(string name, string note) { if (!shots.Add(name)) yield break; float ts = Time.timeScale; Time.timeScale = 0; yield return null; yield return Snap($"{tag}-{name}"); Note($"   shot {tag}-{name} at {V(p)} s {station:F0}: {note}; {View1300r7(me)}"); Time.timeScale = ts; }
                if (reverse && Vector3.Distance(Flat(p), new Vector3(724.6f, 0, -79.7f)) < 6) yield return Shot("1-dans-photo-1-spot", "Dan's photo 1 spot (the end of the descent past CR117 CP 6)");
                foreach (var ts in turns) if (station > ts - 28 && station < ts - 15 && Mathf.Abs(station - lastShotS) > 20) { lastShotS = station; yield return Shot($"turn-s{ts:F0}", $"25 m before the turn at s {ts:F0}"); }
                if (!reverse && race.AllBranches.FirstOrDefault(b => b.title == "Climbing Ridge Cut") is WoodlandRoute cut && station > cut.entryRoad - 45 && station < cut.entryRoad - 25) yield return Shot("2-climbing-ridge-cut-junction", $"the Climbing Ridge Cut junction ({(race.Branches.Contains(cut) ? "open" : "closed")}) 35 m on");
                var gully = flights.FirstOrDefault(f => f.name.Contains("Gully"));
                if (gully != null && station > gully.endStation + 2 && station < gully.endStation + 30 && car.GroundedWheels > 0) yield return Shot("3-gully-landing", $"the {gully.name} landing, looking ahead");
                // once into the J1 rail on purpose (Reverse normal race): from CP 40 (just before J1) straight on, flat out
                if (!railDone && rail && next == 41 && Vector3.Distance(Flat(p), Flat(race.GatePosition(40))) < 10)
                {
                    railDone = true; Note($"   into the J1 rail on purpose at {speed * 2.237f:0} mph from {V(p)}"); float tr = Time.time, maxV = 0; bool hit = false; var start = p;
                    while (Time.time - tr < 5) { Drive(car, p + Vector3.forward * 100 + Vector3.right * 4, 30); yield return null; var q = car.Body.position; maxV = Mathf.Max(maxV, car.Body.linearVelocity.magnitude); if (!hit && Vector3.Distance(Flat(q), Flat(rail.position)) < 7) { hit = true; Note($"   reached the rail at {car.Body.linearVelocity.magnitude * 2.237f:0} mph, {V(q)}"); } if (Time.time - tr > 1.5f && hit) break; }
                    yield return new WaitForSeconds(1.2f); Release(); yield return new WaitForSeconds(.8f);
                    var e = car.Body.position; float es = race.road.Project(e, out float lat);
                    Note($"   after the rail: at {V(e)}, {car.Body.linearVelocity.magnitude * 2.237f:0} mph, {Mathf.Abs(lat):0.0} m from the line at s {es:F0} (on the road: {Mathf.Abs(lat) < race.road.HalfWidth(es)}), upright {Vector3.Dot(car.transform.up, Vector3.up):0.00}, resets so far {me.Recoveries}; went {(e.z > rail.position.z + 3 ? "PAST it up the climb" : e.x < rail.position.x - 4 ? "left onto the route" : "stopped at it")}");
                    yield return Snap($"{tag}-4-after-the-rail"); continue;
                }
                // the pad: the course 10-30 m ahead (the road a player sees), slower for the bend coming; a stall backs up and goes on slower
                if (Time.time < backUntil) { InputSystem.QueueStateEvent(pad, new GamepadState { leftStick = new Vector2(-Mathf.Sign(Arrow(car, me)) * .8f, 0), leftTrigger = 1 }); continue; }
                // where the car is on the course (its own projection, near the race's station for it: the lake roads are driven twice)
                float cs = race.road.ProjectNear(p, station, 60, out float off); bool mountain = race.Leviathan.Mountain(cs);
                float look = Mathf.Clamp(speed * .9f, 8, 24); var target = race.road.At(cs + look, out _);
                race.road.At(cs + 10, out var f10); race.road.At(cs + 40, out var f40);
                float bend = Mathf.Max(Vector3.Angle(Flat(car.transform.forward), Flat(f10)), Vector3.Angle(Flat(f10), Flat(f40)));
                float cap = (mountain ? Mathf.Lerp(12, 5, Mathf.InverseLerp(10, 70, bend)) : Mathf.Lerp(22, 8, Mathf.InverseLerp(10, 90, bend))) * slow;
                if (me.Branch.Route) { var ah = LeviathanGuide.Ahead(race, me, 30, 5); target = ah[Mathf.Min(4, ah.Count - 1)]; }
                if (g.TurnBackGate >= 0 || Mathf.Abs(off) > 30) { target = race.GatePosition(next); cap = 8; }
                var fl = flights.FirstOrDefault(f => cs > f.approachStation - 80 && cs < f.endStation);
                if (fl != null) cap = Mathf.Max(cap, fl.aiTakeoffSpeed + 1); // a jump: a player goes for it
                Drive(car, target, cap, 0, 0, 0, 18);
                stuck = speed < 1.5f ? stuck + Time.deltaTime : 0;
                if (stuck > 2.5f)
                {
                    stuck = 0; stalls++; slow = Mathf.Max(.4f, slow * .75f);
                    // the game's own reset (as a player would press it), never two in a row (a second one opens the pause question)
                    if (stalls % 3 == 0 && Time.time - lastReset > 15) { resets++; lastReset = Time.time; Note($"   stalled again at {V(p)} (next CP {next}): the game's reset (a player's)"); car.GetComponent<VehicleRespawn>()?.ResetVehicle(); yield return new WaitForSecondsRealtime(3); if (flow.State == RaceFlow.Stage.Paused) flow.Resume(); }
                    else { Note($"   stalled at {V(p)} (next CP {next}): backs up, then on at {slow:0.00} of the speed; pad throttle {pad.rightTrigger.ReadValue():0.0} steer {pad.leftStick.ReadValue().x:0.0}, kinematic {car.Body.isKinematic}, grounded wheels {car.GroundedWheels}, state {flow.State}, time scale {Time.timeScale}, car enabled {car.enabled}"); backUntil = Time.time + 2f; }
                }
                if (speed > 4) slow = Mathf.Min(1, slow + Time.deltaTime * .02f);
            }
            Release(); Time.timeScale = 1; pilot.enabled = false;
            Note($"   end of the stretch: next CP {me.Progress.NextGate}, missed gates {me.Progress.MissedGates}, stalls {stalls}, resets {resets}, barricades touched {touched.Count}{(me.Progress.NextGate == until ? "" : "   <-- DID NOT GET THROUGH")}");
        }

        // ---------------------------------------------------------------- an AI Stunt Race (3 rivals)
        IEnumerator Ai1300r7()
        {
            Campaign.Testing = true; DriverVariation.Seed = 1307; Conditions(); int course = int.Parse(Arg("-p119course", "9"));
            yield return StuntQuick(course, false, 1, 1); yield return WaitRacing(); Bind();
            var me = race.Racers[0]; var car = race.vehicle; Pilot(car, me); race.finishGraceSeconds = 100000;
            Note($"== AI Stunt Race on {race.courseName} ({race.courseId}): {race.Racers.Count} racers, {race.gates.Length} gates; shortcuts open: {string.Join(", ", race.Branches.Select(b => b.title))}; closed: {string.Join(", ", race.AllBranches.Except(race.Branches).Select(b => b.title))}");
            var panels = race.gameObject.scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<BoxCollider>(true)).Where(c => c.name == "Barricade collision" && c.gameObject.activeInHierarchy).ToList();
            var reset0 = race.Racers.ToDictionary(r => r, r => r.Recoveries); var near = race.Racers.ToDictionary(r => r, r => new HashSet<string>()); var cut = race.Racers.ToDictionary(r => r, r => new HashSet<string>());
            var stuckFor = race.Racers.ToDictionary(r => r, r => 0f); var stuckAt = race.Racers.ToDictionary(r => r, r => new List<string>());
            Time.timeScale = 4; float t0 = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup - t0 < 1200 && flow.State == RaceFlow.Stage.Racing && !race.Racers.All(r => r.Progress.Finished || r.Dnf))
            {
                AudioListener.volume = 0; yield return null; if (Time.timeScale == 0) Time.timeScale = 4;
                foreach (var r in race.Racers)
                {
                    if (r.Progress.Finished || !r.Car) continue; var p = r.Car.Body.position;
                    foreach (var c in panels) if (c && c.bounds.SqrDistance(p) < 1.5f * 1.5f) near[r].Add(c.transform.parent.parent.name.Replace("Leviathan road closed ", ""));
                    foreach (var b in race.AllBranches.Except(race.Branches)) { float s = b.Project(p, out _); var q = b.At(s, out _); if (s > 20 && s < b.Length - 20 && Vector3.Distance(Flat(p), Flat(q)) < b.halfWidth && Mathf.Abs(p.y - q.y) < 3) { float rs = race.road.Project(p, out float lat); if (Mathf.Abs(lat) > race.road.HalfWidth(rs) + 2) cut[r].Add(b.title); } }
                    stuckFor[r] = r.Car.Body.linearVelocity.magnitude < 1 ? stuckFor[r] + Time.deltaTime : 0;
                    if (stuckFor[r] > 8) { stuckAt[r].Add(V(p)); stuckFor[r] = -1000; }
                }
            }
            Time.timeScale = 1;
            foreach (var r in race.Racers) Note($"  {(r.IsAi ? "rival" : "player (road AI)")} {Rp(r)}: {(r.Progress.Finished ? "finished" : r.Dnf ? "DNF" : "unfinished")}, missed gates {r.Progress.MissedGates}, resets {r.Recoveries - reset0[r]}, stuck 8 s+ {(stuckAt[r].Count == 0 ? "never" : string.Join("; ", stuckAt[r]))}, closed shortcut taken {(cut[r].Count == 0 ? "never" : string.Join(", ", cut[r]))}, within 1.5 m of a barricade/rail {(near[r].Count == 0 ? "never" : string.Join("; ", near[r]))}");
        }
    }
}
