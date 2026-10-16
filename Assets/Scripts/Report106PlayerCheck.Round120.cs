using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Racer
{
    // 0.120 evidence only (the 0.106 check component: -p106check <dir> -racerTestSave <dir>; never active without both):
    //   -p106kind probe120   a quick Stunt Race (-p119course 4 / 5 / 8 / 9) alone: the course line every 3 m (line.csv: position, heading, the ground
    //                        height across it from 7 m left to 7 m right, whether something stands there), its gates and big jumps, then one lap
    //                        driven by the road AI (speed.csv: the speed at each station), for placing the stunt bumps (Tools/Report120/py/bumps.py)
    public sealed partial class Report106PlayerCheck
    {
        static string N(float v) => v.ToString("0.##", CultureInfo.InvariantCulture);
        IEnumerator StuntAlone120(int course, string vehicle = "stuntbuggy")
        {
            yield return QuickTo115(course); Conditions();
            var st = flow.Save.Settings; st.stuntSplitScore = false; flow.Save.SaveSettings();
            if (!race.stunts) flow.StepRaceType(1); if (race.splitScore) flow.StepStuntScoring(1);
            flow.OpenGarage(); flow.SelectVehicle(vehicle); flow.CloseGarage();
            st.opponents = false; st.traffic = false; st.laps = 1; st.estimateAiFinishes = false; flow.Save.SaveSettings();
            race.opponents = false; race.traffic = false; race.laps = 1;
            flow.StartRace(); yield return WaitRacing(); Bind();
        }
        IEnumerator Probe120()
        {
            Campaign.Testing = true; Conditions(); int course = int.Parse(Arg("-p119course", "4"));
            yield return StuntAlone120(course);
            var road = race.road; var car = race.vehicle; int mask = car.groundMask;
            Note($"== probe {RacePlaylists.Titles[course]}: {race.courseName} ({race.courseId}), line {road.Length:F0} m, stunts {race.stunts}");
            float Ground(Vector3 q, float near, out bool thing)
            {
                thing = false; float y = float.NaN, best = 1e9f;
                foreach (var h in Physics.RaycastAll(new Vector3(q.x, near + 40, q.z), Vector3.down, 90, mask, QueryTriggerInteraction.Ignore))
                    if (!h.collider.attachedRigidbody && Mathf.Abs(h.point.y - near) < best) { best = Mathf.Abs(h.point.y - near); y = h.point.y; }
                if (float.IsNaN(y)) return y;
                foreach (var c in Physics.OverlapCapsule(new Vector3(q.x, y + 1, q.z), new Vector3(q.x, y + 2.6f, q.z), 1.1f, ~0, QueryTriggerInteraction.Ignore))
                    if (!c.attachedRigidbody && (mask & (1 << c.gameObject.layer)) == 0) { thing = true; break; }
                return y;
            }
            var sb = new StringBuilder("s,x,y,z,fx,fz,hw,g-7,g-5,g-3,g0,g3,g5,g7,o-6,o-3,o0,o3,o6\n");
            for (float s = 0; s < road.Length; s += 3)
            {
                var p = road.At(s, out var f); f = Flat(f).normalized; var r = new Vector3(f.z, 0, -f.x);
                sb.Append($"{N(s)},{N(p.x)},{N(p.y)},{N(p.z)},{N(f.x)},{N(f.z)},{N(road.HalfWidth(s))}");
                foreach (var l in new[] { -7, -5, -3, 0, 3, 5, 7 }) { float g = Ground(p + r * l, p.y, out _); sb.Append("," + (float.IsNaN(g) ? "" : N(g))); }
                foreach (var l in new[] { -6, -3, 0, 3, 6 }) { Ground(p + r * l, p.y, out bool thing); sb.Append(thing ? ",1" : ",0"); }
                sb.Append('\n');
            }
            File.WriteAllText(Path.Combine(outDir, "line.csv"), sb.ToString());
            var gates = new StringBuilder("gate,s,x,z\n");
            for (int i = 0; i < race.gates.Length; i++) { var g = race.gates[i]; if (!g) continue; float s = road.Project(g.transform.position, out _); gates.Append($"{i},{N(s)},{N(g.transform.position.x)},{N(g.transform.position.z)}\n"); }
            File.WriteAllText(Path.Combine(outDir, "gates.csv"), gates.ToString());
            var flights = race.GetComponent<MountainFlights>()?.flights ?? new MountainFlights.Flight[0];
            File.WriteAllText(Path.Combine(outDir, "flights.csv"), "name,approach,end,lipx,lipz\n" + string.Concat(flights.Select(fl => $"{fl.name.Replace(',', ' ')},{N(fl.approachStation)},{N(fl.endStation)},{N(fl.lip.x)},{N(fl.lip.z)}\n")));
            // the marked jump sites ("High Ridge Drop" and the like) and Leviathan's own kickers: no bump on or just before them either
            var jumps = new StringBuilder("name,x,z,s\n");
            foreach (var site in flow.Activities.Sites.Where(x => x && x.kind == ActivitySite.Kind.Jump)) jumps.Append($"{site.title.Replace(',', ' ')},{N(site.transform.position.x)},{N(site.transform.position.z)},{N(road.Project(site.transform.position, out _))}\n");
            foreach (var k in LeviathanCourse.Current != null && race.Leviathan ? LeviathanCourse.Current.kickers ?? new LeviathanCourse.Kicker[0] : new LeviathanCourse.Kicker[0]) jumps.Append($"{k.at.Replace(',', ' ')},{N(k.pos[0])},{N(k.pos[1])},{N(road.Project(new Vector3(k.pos[0], k.pos.Length > 2 ? k.pos[2] : 0, k.pos[1]), out _))}\n");
            File.WriteAllText(Path.Combine(outDir, "jumps.csv"), jumps.ToString());
            var branches = race.Branches ?? new WoodlandRoute[0];
            Note($"  {race.gates.Length} gates, {flights.Length} flights ({string.Join(", ", flights.Select(fl => $"{fl.name} {fl.approachStation:F0}-{fl.endStation:F0}"))}), {branches.Length} shortcuts");
            // one lap by the road AI: its speed along the line
            var me = race.Racers[0]; Pilot(car, me); race.finishGraceSeconds = 100000;
            var speed = new StringBuilder("s,speed,x,z,air\n"); Time.timeScale = 4; float t0 = Time.realtimeSinceStartup, next = 0;
            while (Time.realtimeSinceStartup - t0 < 900 && flow.State == RaceFlow.Stage.Racing && !me.Progress.Finished)
            {
                yield return new WaitForFixedUpdate(); AudioListener.volume = 0; if (Time.timeScale == 0) Time.timeScale = 4;
                if (Time.time < next) continue; next = Time.time + .1f;
                speed.Append($"{N(me.RoadPosition)},{N(car.Body.linearVelocity.magnitude)},{N(car.Body.position.x)},{N(car.Body.position.z)},{(car.GroundedWheels == 0 ? 1 : 0)}\n");
            }
            Time.timeScale = 1; File.WriteAllText(Path.Combine(outDir, "speed.csv"), speed.ToString());
            Note($"  AI lap: finished {me.Progress.Finished}, {me.Progress.RaceTime(race.Clock):F1} s, recoveries {me.Recoveries}");
        }
        // ---------------------------------------------------------------- checks: the trick panel, the multiplier, the park, the bumps, the garage
        //   -p106kind park120    the Stunt event alone (shortened): a hop, then at once a flight turned backflip + roll + 360 by the pad (Part B: x3; the
        //                        trick panel mid-combo and after the landing: Part A); a double backflip (x1); a backflip out over the south fence onto the
        //                        Hwy 92 side followed by a hop outside (Part D: the backflip counts, the hop does not), then the reset back into the park
        //   -p106kind split120   the Stunt event, two players (split screen): both turning a trick at once: a shot of both views' trick panels
        //   -p106kind race120    a quick Stunt Race (-p119course, -p115diff, 3 rivals) with the bumps: the player driven by the road AI (its tricks turned by
        //                        RivalTricks): shots mid-trick and after a landing; every flight off a bump (player and rivals); rival resets and how far
        //                        from a bump; then a normal race in the same scene: no bumps
        //   -p106kind clip120    a quick Stunt Race alone: the player run straight at bump -p120bump with a trick turned by RivalTricks: frames for a clip
        //   -p106kind garage120  the controller only: Race Setup (Stunt Race) > Garage: the stunt vehicles; the Skywhip 250 chosen, the race started in it;
        //                        a Stunt chapter event's garage (A on its vehicle row); a Chapter 1 event's garage (no stunt vehicles); B back each time
        static Vector3 Turned(ArcadeVehicle car) => car.transform.InverseTransformDirection(car.Body.angularVelocity) * (Time.fixedDeltaTime * Mathf.Rad2Deg);
        // a trick turned with a pad as a player would: each (axis, degrees) at full stick in turn (0 flip: left stick up / down; 2 roll: left stick
        // left / right; 1 spin: right stick), then hands off; one shot named `shot` when `when` is true
        IEnumerator PadTrick(Gamepad g, ArcadeVehicle car, (int axis, float deg)[] steps, string shot = null, System.Func<bool> when = null)
        {
            float t0 = Time.time; while (car.GroundedWheels > 0 && Time.time - t0 < 1.5f) yield return new WaitForFixedUpdate();
            Vector3 turned = Vector3.zero; int i = 0; float stepBase = 0; bool shotDone = shot == null;
            while (Time.time - t0 < 9 && car.GroundedWheels == 0)
            {
                yield return new WaitForFixedUpdate(); turned += Turned(car);
                var st = new GamepadState();
                if (i < steps.Length)
                {
                    // the stick eased off as the turn nears its end (so it stops square), as a player feathering it would
                    var (axis, deg) = steps[i]; float rem = deg - (turned[axis] - stepBase);
                    float rate = axis == 1 ? ArcadeVehicle.AirSpinRate : axis == 0 ? ArcadeVehicle.AirTipRate : ArcadeVehicle.AirRollRate, stick = Mathf.Clamp(rem * Mathf.Deg2Rad * 4f / rate, -1, 1);
                    float w = car.transform.InverseTransformDirection(car.Body.angularVelocity)[axis];
                    if (Mathf.Abs(rem) < 8 && Mathf.Abs(w) < .5f) { i++; if (i < steps.Length) stepBase = turned[steps[i].axis]; }
                    else if (axis == 0) st.leftStick = new Vector2(0, stick); else if (axis == 2) st.leftStick = new Vector2(-stick, 0); else st.rightStick = new Vector2(stick, 0);
                }
                InputSystem.QueueStateEvent(g, st);
                if (!shotDone && when != null && when()) { shotDone = true; float ts = Time.timeScale; Time.timeScale = 0; yield return Snap(shot); Time.timeScale = ts; }
            }
            Neutral(g);
        }
        string RaceText => FindAnyObjectByType<RaceHud>()?.display?.text?.Replace("\n", " | ") ?? "";
        IEnumerator StartEvent120(int players, Gamepad pad2)
        {
            StuntEvent.TestLimit = 400; Conditions(); flow.Save.Settings.hints = false; flow.Save.SaveSettings();
            SplitScreen.Mode = SplitScreen.Kind.Stunt; SplitScreen.Solo = players == 1; SplitScreen.P1Device = pad; SplitScreen.P2Device = pad2; SplitScreen.P2Ai = players == 1;
            SplitScreen.P1Vehicle = "stuntbuggy"; SplitScreen.P2Vehicle = "stuntbike"; SplitScreen.FromRoam = false; SplitScreen.Time = TimeOfDay.Day; SplitScreen.Weather = Weather.Clear;
            if (players > 1) { SplitScreen.P1Name = "Player One"; SplitScreen.P2Pick = "Player Two"; }
            flow.StartSplit(); float r0 = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup - r0 < 150) { Bind(); if (flow && StuntEvent.Current && StuntEvent.Current.State == StuntEvent.Phase.Running) break; yield return null; }
            AudioListener.volume = 0; WorldLook.Current?.SetClock(WorldLook.Current.Day, 12); yield return new WaitForSeconds(1);
        }
        IEnumerator Settle(ArcadeVehicle car, float seconds = .8f) { float t = Time.time; while (Time.time - t < 6 && car.GroundedWheels < 2) yield return new WaitForFixedUpdate(); t = Time.time; while (Time.time - t < seconds) yield return new WaitForFixedUpdate(); }
        // a flight just launched: off the ground, then down on the wheels and settled (the game's landing takes 0.3 s on the wheels)
        IEnumerator Land(ArcadeVehicle car, float seconds = .5f) { float t = Time.time; while (Time.time - t < 2 && car.GroundedWheels > 0) yield return new WaitForFixedUpdate(); yield return Settle(car, seconds); }
        IEnumerator Park120()
        {
            yield return StartEvent120(1, null); var ev = StuntEvent.Current; if (!ev) { Note("the event did not start"); yield break; }
            var car = race.vehicle; var acts = flow.Activities; var r = ev.Of(1);
            Note($"Stunt event alone; trick panel on: {acts.TrickBox}; vehicle {car.GetComponent<VehicleConfiguration>().profileId}");
            // Part B: a hop (a plain jump link), then at once a flight with three trick types
            yield return Stand(pad, car, P(112, 30), 0, 7, .3f);
            car.Body.linearVelocity += Vector3.up * 7; yield return Land(car, .4f);
            Note($"  hop: '{acts.LastStunt.Replace("\n", " ")}' {acts.LastStuntPoints} pts; combo x{acts.ComboLinks}; panel '{TrickHud.Text(acts)}'");
            car.Body.linearVelocity = Flat(car.transform.forward) * 5 + Vector3.up * 23;
            yield return PadTrick(pad, car, new[] { (0, -360f), (2, 360f), (1, 360f) }, "A-park-mid-combo", () => acts.Live != null && acts.Live.types >= 2);
            yield return Settle(car, .5f);
            Note($"  three types: '{acts.LastStunt.Replace("\n", " ")}', {acts.LastStuntPoints} pts, types {acts.LastFlightTypes}, turned {acts.LastStuntRotation}; combo x{acts.ComboLinks} {acts.ComboTotal}; panel '{TrickHud.Text(acts)}'");
            yield return Snap("A-park-after-landing");
            float w = Time.time; while (acts.ComboLinks > 0 && Time.time - w < 8) yield return null;
            Note($"  banked: event points {r.points}, last log {r.log.LastOrDefault()}");
            // the same type twice: x1
            yield return Stand(pad, car, P(112, 30), 0, 5, .3f);
            car.Body.linearVelocity = Flat(car.transform.forward) * 5 + Vector3.up * 19;
            yield return PadTrick(pad, car, new[] { (0, -720f) }); yield return Settle(car, .5f);
            Note($"  same type twice: '{acts.LastStunt.Replace("\n", " ")}', {acts.LastStuntPoints} pts, types {acts.LastFlightTypes}, turned {acts.LastStuntRotation}; panel '{TrickHud.Text(acts)}'");
            w = Time.time; while (acts.ComboLinks > 0 && Time.time - w < 8) yield return null;
            // Part D: out over the south fence (Hwy 92 side), then a hop outside within the chain time
            int before = r.points, outside = r.outside; var outcome = new List<string>();
            yield return Flight(pad, car, "a backflip off the floor by the south fence, out toward Hwy 92", P(60, 13), 180, 12, 8, new Vector3(-360, 0, 0), outcome, .3f); foreach (var o in outcome) Note("  " + o);
            Note($"  landed {(StuntPark.OnPark(car.Body.position) ? "IN" : "outside")} the park at {UV(car.Body.position)}; combo x{acts.ComboLinks} {acts.ComboTotal}");
            yield return Settle(car, .3f); car.Body.linearVelocity = Flat(car.transform.forward) * 8 + Vector3.up * 7; float hopAt = Time.time;
            yield return Land(car, .5f);
            Note($"  the hop outside: landed {(StuntPark.OnPark(car.Body.position) ? "in" : "outside")} the park, {Time.time - hopAt:0.0} s after take-off; result '{acts.LastResult}'; panel '{TrickHud.Text(acts)}'; combo x{acts.ComboLinks} {acts.ComboTotal}"); yield return Snap("D-hop-outside");
            w = Time.time; while (acts.ComboLinks > 0 && Time.time - w < 8) yield return null; yield return new WaitForSeconds(.5f);
            Note($"  event points {before:N0} -> {r.points:N0} (+{r.points - before:N0}); not counted {outside} -> {r.outside}; log: {string.Join(" | ", r.log.Skip(Mathf.Max(0, r.log.Count - 2)))}");
            InputSystem.QueueStateEvent(pad, new GamepadState().WithButton(GamepadButton.North)); yield return new WaitForSeconds(.15f); Neutral(pad); yield return new WaitForSeconds(2.5f);
            Note($"  reset (Y) -> '{car.GetComponent<VehicleRespawn>().LastRecovery}' at {UV(car.Body.position)}, {(StuntPark.OnPark(car.Body.position) ? "IN the park" : "NOT in the park")}");
            yield return Snap("D-after-reset");
            StuntEvent.TestLimit = -1;
        }
        IEnumerator Split120()
        {
            var pad2 = InputSystem.AddDevice<Gamepad>("Report120Pad2");
            yield return StartEvent120(2, pad2); var ev = StuntEvent.Current; if (!ev) { Note("the event did not start"); yield break; }
            var c1 = race.vehicle; var c2 = SplitRoam.Current ? SplitRoam.Current.Car : null; var a1 = flow.Activities; var a2 = SplitRoam.Current ? SplitRoam.Current.Activities2 : null;
            Note($"split Stunt event: P2 car {(c2 ? c2.GetComponent<VehicleConfiguration>().profileId : "none")}, panels on {a1.TrickBox} / {(a2 && a2.TrickBox)}");
            var s1 = StartCoroutine(Stand(pad, c1, P(112, 30), 0, 5, .3f)); var s2 = StartCoroutine(Stand(pad2, c2, P(40, 30), 0, 5, .3f)); yield return s1; yield return s2;
            c1.Body.linearVelocity = Flat(c1.transform.forward) * 5 + Vector3.up * 20; c2.Body.linearVelocity = Flat(c2.transform.forward) * 5 + Vector3.up * 20;
            var t1 = StartCoroutine(PadTrick(pad, c1, new[] { (0, -360f), (1, 360f) }, "A-split-both-views", () => a1.Live != null && a1.Live.types >= 1 && a2 && a2.Live != null && a2.Live.types >= 1));
            var t2 = StartCoroutine(PadTrick(pad2, c2, new[] { (2, 360f), (1, 360f) })); yield return t1; yield return t2;
            yield return Settle(c1, .3f);
            Note($"  P1 '{a1.LastStunt.Replace("\n", " ")}' {a1.LastStuntPoints} pts; P2 '{a2?.LastStunt.Replace("\n", " ")}' {a2?.LastStuntPoints} pts; panels '{TrickHud.Text(a1)}' / '{TrickHud.Text(a2)}'");
            StuntEvent.TestLimit = -1;
        }
        IEnumerator Race120()
        {
            Campaign.Testing = true; DriverVariation.Seed = System.Environment.TickCount; Conditions();
            int course = int.Parse(Arg("-p119course", "4")), diff = int.Parse(Arg("-p115diff", "1"));
            yield return StuntQuick(course, false, 1, diff); yield return WaitRacing(); Bind();
            var bumps = StuntBumps.For(race.courseId)?.bumps ?? new LeviathanCourse.Kicker[0];
            Note($"== Stunt Race on {RacePlaylists.Titles[course]} ({race.courseId}), {race.DifficultyName}: bumps built {StuntBumps.Built} of {bumps.Length}; rivals {string.Join(", ", race.Racers.Where(x => x.IsAi).Select(Rp))}");
            (int i, float d) Nearest(Vector3 p) { int bi = -1; float bd = 1e9f; for (int i = 0; i < bumps.Length; i++) { float d = Vector2.Distance(new Vector2(p.x, p.z), new Vector2(bumps[i].pos[0], bumps[i].pos[1])); if (d < bd) { bd = d; bi = i; } } return (bi, bd); }
            string Near(Vector3 p) { var n = Nearest(p); return n.i < 0 ? "no bump" : $"bump {n.i + 1} ({bumps[n.i].at}) {n.d:F0} m"; }
            var me = race.Racers[0]; var car = race.vehicle; Pilot(car, me); var steer = car.gameObject.AddComponent<RivalTricks>(); steer.Initialize(race, 9); steer.Bind(me); steer.SteerOnly = true;
            race.finishGraceSeconds = 100000; var acts = flow.Activities;
            var ai = race.Racers.Where(x => x.IsAi).ToList(); var reset0 = ai.ToDictionary(x => x, x => x.Recoveries); var resets = new List<string>(); var counted = ai.ToDictionary(x => x, x => 0);
            var all = race.Racers.ToList(); var air = all.ToDictionary(x => x, x => false); var from = all.ToDictionary(x => x, x => Vector3.zero); var bumpFlights = new List<string>();
            bool midShot = false, landShot = false; Time.timeScale = 3; float t0 = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup - t0 < 1500 && flow.State == RaceFlow.Stage.Racing && !(me.Progress.Finished && ai.All(x => x.Progress.Finished || x.Dnf)))
            {
                yield return new WaitForFixedUpdate(); AudioListener.volume = 0; if (Time.timeScale == 0) Time.timeScale = 3;
                foreach (var x in ai) while (x.Recoveries - reset0[x] > counted[x]) { counted[x]++; resets.Add($"{Rp(x)} at {V(x.Car.Body.position)}: nearest {Near(x.Car.Body.position)}"); }
                foreach (var x in all)
                {
                    var c = x.Car; if (!c) continue; bool off = c.GroundedWheels == 0;
                    if (off && !air[x]) { air[x] = true; from[x] = c.Body.position; }
                    else if (!off && c.GroundedWheels >= 2 && air[x]) { air[x] = false; var n = Nearest(from[x]); if (n.i >= 0 && n.d < 12 && Vector3.Distance(Flat(from[x]), Flat(c.Body.position)) > 4) bumpFlights.Add($"{(x == me ? "player" : Rp(x))} off bump {n.i + 1}: {Vector3.Distance(Flat(from[x]), Flat(c.Body.position)):F0} m"); }
                }
                if (!midShot && acts.Live != null && acts.Live.names.Count > 0) { midShot = true; Time.timeScale = 1; yield return Snap("A-race-mid-trick-" + course); Note($"  mid-trick: panel '{TrickHud.Text(acts)}'; race panel '{RaceText}'"); Time.timeScale = 3; }
                if (!landShot && acts.LastResultKind == 1 && Time.time - acts.LastResultAt < .2f && acts.LastFlightTypes > 0) { landShot = true; Time.timeScale = 1; yield return Snap("A-race-after-landing-" + course); Note($"  after a landing: panel '{TrickHud.Text(acts)}'; race panel '{RaceText}'"); Time.timeScale = 3; }
                if (me.Progress.Finished && ai.Any(x => !x.Progress.Finished && !x.Dnf) && race.Progress.RaceTime(race.Clock) > 1200) break;
            }
            Time.timeScale = 1;
            Note($"  player: {(me.Progress.Finished ? me.ClassifiedTime(race.Clock).ToString("0.0", CultureInfo.InvariantCulture) : "unfinished")} s (bonus {me.Progress.BonusSeconds:0.0} s, {me.Progress.StuntPoints} pts, best combo {me.Progress.BestCombo}); most types in a flight {steer.Types}; recoveries {me.Recoveries}");
            Note("  player flights: " + steer.Log.Replace("\n", " | "));
            foreach (var x in ai) { var t = x.Car.GetComponent<RivalTricks>(); Note($"  rival {Rp(x)}: {(x.Progress.Finished ? x.ClassifiedTime(race.Clock).ToString("0.0", CultureInfo.InvariantCulture) : x.Dnf ? "DNF" : "unfinished")} (bonus {x.Progress.BonusSeconds:0.0} s, {x.Progress.StuntPoints} pts), resets {x.Recoveries - reset0[x]}, flights {t?.Flights}, tricks {t?.Tricks}, most types {t?.Types}, bails {t?.Bails}"); if (t) Note("    " + t.Log.Replace("\n", " | ")); }
            Note($"  flights off a bump: {bumpFlights.Count}: {string.Join("; ", bumpFlights)}");
            Note($"  rival resets: {resets.Count}{(resets.Count > 0 ? ": " + string.Join("; ", resets) : "")}");
            race.finishGraceSeconds = 0; float wr = Time.realtimeSinceStartup; while (flow.State != RaceFlow.Stage.Results && Time.realtimeSinceStartup - wr < 40) yield return null;
            // a normal race in the same scene: no bumps
            flow.QuitRace(); yield return WaitMenu(); Bind(); if (race.stunts) flow.StepRaceType(1); yield return WaitMenu(); Bind();
            race.opponents = false; flow.StartRace(); yield return WaitRacing(); Bind();
            Note($"  then a normal race here: stunts {race.stunts}, bumps built {StuntBumps.Built}, bump objects {UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects().Count(g => g.name == "Stunt Race bumps")}");
        }
        IEnumerator Clip120()
        {
            Campaign.Testing = true; Conditions(); int course = int.Parse(Arg("-p119course", "4")), k = int.Parse(Arg("-p120bump", "1")) - 1; float speed = F(Arg("-p120speed", "24"));
            yield return StuntAlone120(course); var car = race.vehicle; var acts = flow.Activities; var me = race.Racers[0];
            var b = StuntBumps.For(race.courseId).bumps[k]; var f = Quaternion.Euler(0, b.yaw, 0) * Vector3.forward; var lip = new Vector3(b.pos[0], b.pos.Length > 2 ? b.pos[2] : 0, b.pos[1]);
            var start = lip - f * F(Arg("-p120runup", "60")); start.y = lip.y + 3; if (Physics.Raycast(start + Vector3.up * 30, Vector3.down, out var hit, 80, car.groundMask)) start.y = hit.point.y + .6f;
            Note($"== clip: bump {k + 1} '{b.at}' on {race.courseName}: lip {V(lip)} yaw {b.yaw:F0}, {b.height:F1} m over {b.length:F1} m; run-up from {V(start)} at {speed} m/s");
            BindPad(); Put(car, start, b.yaw, speed); FindAnyObjectByType<ChaseCamera>()?.Snap();
            Time.captureFramerate = 30; int frame = 0; var dir = Path.Combine(outDir, "clip"); Directory.CreateDirectory(dir); bool flown = false, landed = false; float landedAt = 0, t0 = Time.time, turnedX = 0;
            while (Time.time - t0 < 20)
            {
                float to = Vector3.Dot(car.Body.position - lip, f);
                // over the lip: a backflip turned with the pad (eased off so it stops square), as a player would; on the ground: straight at the bump
                if (car.GroundedWheels == 0 && to > -3 && !landed) { turnedX += Turned(car).x * (Time.deltaTime / Time.fixedDeltaTime); float rem = -360 - turnedX, w = car.transform.InverseTransformDirection(car.Body.angularVelocity).x; InputSystem.QueueStateEvent(pad, new GamepadState { leftStick = new Vector2(0, Mathf.Abs(rem) < 8 && Mathf.Abs(w) < .5f ? 0 : Mathf.Clamp(rem * Mathf.Deg2Rad * 4f / ArcadeVehicle.AirTipRate, -1, 1)) }); }
                else Drive(car, lip + f * 200, speed);
                yield return new WaitForEndOfFrame();
                if (to > -50) ScreenCapture.CaptureScreenshot(Path.Combine(dir, $"f{frame++:0000}.png"));
                if (car.GroundedWheels == 0 && to > -2) flown = true;
                if (frame == 0 && Time.frameCount % 30 == 0) Note($"    run-up: at {V(car.Body.position)} {to:F0} m from the lip, {car.Body.linearVelocity.magnitude:F1} m/s, input {car.GetComponent<VehicleInput>()?.enabled}, state {flow.State}");
                if (flown && !landed && acts.LastResultAt > t0 && Time.time - acts.LastResultAt < .1f) { landed = true; landedAt = Time.time; }
                if (landed && Time.time - landedAt > 2) break;
                yield return null;
            }
            Time.captureFramerate = 0; Release();
            Note($"  flown {flown}: '{acts.LastStunt.Replace("\n", " ")}' {acts.LastStuntPoints} pts, types {acts.LastFlightTypes}, result '{acts.LastResult}'; {frame} frames");
        }
        string PickRow => FindObjectsByType<UnityEngine.UI.Button>(FindObjectsSortMode.None).FirstOrDefault(b => b.isActiveAndEnabled && b.name == "pick-vehicle")?.GetComponentInChildren<UnityEngine.UI.Text>().text ?? "?";
        IEnumerator Right() { InputSystem.QueueStateEvent(pad, new GamepadState { leftStick = new Vector2(1, 0) }); yield return new WaitForSecondsRealtime(.15f); Release(); yield return new WaitForSecondsRealtime(.6f); }
        IEnumerator Garage120()
        {
            Conditions(); var st = flow.Save.Settings; st.stuntRace = false; flow.Save.SaveSettings();
            yield return QuickTo115(4); Campaign.Testing = false; st = flow.Save.Settings; Menu.ResetPages(); flow.RefreshMenu(); yield return new WaitForSecondsRealtime(1);
            Note($"start: page '{Page}', focus '{Focus}', saved stunt vehicle {st.stuntVehicleId}, Skywhip locked {VehicleUnlocks.Locked(VehicleProfile.StuntBike)}");
            yield return FocusRow("race"); yield return Press(GamepadButton.South); yield return new WaitForSecondsRealtime(1);
            yield return FocusRow("race-type"); if (!race.stunts) { yield return Right(); yield return new WaitForSecondsRealtime(1.5f); yield return WaitMenu(); Bind(); }
            if (Page.Contains("modal") || Page.Contains("unlock")) { yield return Press(GamepadButton.South); yield return new WaitForSecondsRealtime(1); }
            Note($"Race Setup: stunts {race.stunts}, focus '{Focus}'");
            yield return FocusRow("garage"); yield return Press(GamepadButton.South); yield return new WaitForSecondsRealtime(1.2f);
            Note($"Garage: {Texts("Clay", "Skywhip")}"); yield return Snap("E-race-garage");
            var seen = new List<string>();
            for (int i = 0; i < 4 && race.vehicle.GetComponent<VehicleConfiguration>().profileId != "stuntbike"; i++) { yield return Right(); seen.Add(Texts("Clay", "Skywhip")); }
            Note($"  right: {string.Join(" / ", seen)}");
            yield return Press(GamepadButton.East); yield return new WaitForSecondsRealtime(.8f);
            Note($"B: page '{Page}', vehicle {race.vehicle.GetComponent<VehicleConfiguration>().profileId}, saved stunt vehicle {st.stuntVehicleId}, normal vehicle {st.vehicleId}");
            yield return FocusRow("start"); yield return Press(GamepadButton.South); yield return WaitRacing(); Bind();
            Note($"started: stunts {race.stunts}, driving {race.vehicle.GetComponent<VehicleConfiguration>().profileId}, bumps built {StuntBumps.Built}"); yield return Snap("E-race-started");
            flow.Pause(); yield return new WaitForSecondsRealtime(.5f); flow.QuitRace(); yield return WaitMenu(); Bind(); st = flow.Save.Settings;
            // the Stunt chapter (chapter 5 open on the test save, as campaign119) and Chapter 1: each first event's garage
            var c = Campaign.Current; var c4 = CampaignData.Find("c4-summit-final");
            if (Campaign.ResultOf(c4.Id)?.passed != true) c.results.Add(new Campaign.Result { id = c4.Id, runs = 1, passed = true, bestPlace = 1, field = 6 });
            Campaign.Save(); Campaign.Load(flow.Save.DirectoryPath);
            foreach (var chapter in new[] { 5, 1 })
            {
                var e = CampaignData.InChapter(chapter).First();
                Menu.OpenCampaign(chapter); yield return new WaitForSecondsRealtime(1.2f); if (Page.Contains("modal")) { yield return Press(GamepadButton.South); yield return new WaitForSecondsRealtime(1); }
                yield return FocusRow("cev-" + e.Id, 40); yield return Press(GamepadButton.South); yield return new WaitForSecondsRealtime(1.2f);
                Note($"chapter {chapter}: '{e.Name}': page '{Page}', {Texts("Vehicle:")}");
                yield return FocusRow("event-vehicle", 40); yield return Press(GamepadButton.South); yield return new WaitForSecondsRealtime(1.2f);
                yield return Snap($"E-campaign-garage-ch{chapter}");
                var names = new List<string>(); for (int i = 0; i < 12; i++) { names.Add(PickRow); yield return Right(); }
                Note($"  garage (page '{Page}'), right through: {string.Join(" / ", names.Distinct())}");
                if (chapter == 5)
                {
                    for (int i = 0; i < 6 && !PickRow.StartsWith("Skywhip"); i++) yield return Right();
                    yield return Snap("E-campaign-garage-skywhip");
                    yield return FocusRow("pick-use"); yield return Press(GamepadButton.South); yield return new WaitForSecondsRealtime(1);
                    Note($"  USE: page '{Page}', {Texts("Vehicle:")}; saved stunt vehicle {st.stuntVehicleId}");
                }
                else { yield return Press(GamepadButton.East); yield return new WaitForSecondsRealtime(.8f); Note($"  B: page '{Page}'"); }
                yield return Press(GamepadButton.East); yield return new WaitForSecondsRealtime(.8f); Note($"  B: page '{Page}'");
            }
        }
    }
}
