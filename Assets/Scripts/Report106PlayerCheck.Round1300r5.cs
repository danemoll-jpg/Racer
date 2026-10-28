using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Racer
{
    // 1.3.0-review5 evidence only (Report106PlayerCheck: -p106check <dir> -racerTestSave <dir>, a copy of Dan's save):
    //   -p106kind acorns1300r5  Free Roam: every acorn's height over the ground under it; one acorn picked up in one-player normal, one in
    //                           one-player HIGH MODE, one by player 2 in split screen (the vehicle moved through it): the count, the save, the banner
    //   -p106kind order1300r5   the Stunt chapter's event list (order, open, pay) on the copy's chapter-5 run, a run that had passed only the
    //                           first three events before the swap, and a fresh chapter-5 run
    //   -p106kind land1300r5    landings in a Stunt Race (-p119course 4 / 9), car and bike, on flat, downhill and banked road: each launched off
    //                           the road, the stunt judge's verdict logged; then touch-downs on a side (rights itself / stays down) and the roof
    //   -p106kind lev1300r5     a normal Leviathan Reverse race: the player driven by the pad toward the course guide's own point ahead (the
    //                           HUD arrow's way) for the whole lap: the gates, wrong turns, TURN BACK; then the two out-and-back ends driven
    //                           the wrong way on purpose (straight on at each): when TURN BACK shows and where the guide takes the car back
    public sealed partial class Report106PlayerCheck
    {
        // ---------------------------------------------------------------- Part A
        IEnumerator Acorns1300r5()
        {
            foreach (var m in InputSystem.devices.OfType<Mouse>().ToList()) InputSystem.DisableDevice(m);
            yield return ToFreeRoam("original"); var col = race.GetComponent<ExplorationCollection>();
            Note($"Free Roam ({race.gameObject.scene.name}): acorns {col.Found}/{col.sites.Length}, HIGH {race.high}, split {SplitScreen.Active}");
            foreach (var s in col.sites) { float g = TopAt(s.position + Vector3.up * 3); if (s.position.y - g > 2.2f || g - s.position.y > .5f) Note($"  acorn {s.id} ({s.title}): {s.position.y - g:+0.0;-0.0} m under the top surface there: {string.Join(", ", Physics.RaycastAll(s.position + Vector3.up * 30, Vector3.down, 40, ~0, QueryTriggerInteraction.Ignore).OrderBy(h => h.distance).Select(h => $"{h.collider.name} at {h.point.y:F1}"))}"); }
            Note($"  every other acorn sits 0.5-2.2 m over the ground (a vehicle's centre passes within 2.8 m)");
            var open = col.sites.Where(s => !col.Discovered(s.id)).ToList(); Note($"  not found in this save copy: {string.Join(", ", open.Select(s => s.id))}");
            IEnumerator Through(ArcadeVehicle car, ExplorationCollection.Site s, string tag)
            {
                int before = col.Found; var dir = Flat(s.access - s.position).sqrMagnitude > 1 ? -Flat(s.access - s.position).normalized : Vector3.forward;
                var a = s.position - dir * 10 + Vector3.up * .3f; car.Body.isKinematic = true; car.Body.position = a; car.transform.position = a; Physics.SyncTransforms();
                for (int k = 0; k < 10; k++) yield return new WaitForFixedUpdate();
                for (float t = 0; t <= 20; t += 8 * Time.fixedDeltaTime) { car.Body.MovePosition(a + dir * t); yield return new WaitForFixedUpdate(); }
                yield return new WaitForSecondsRealtime(.4f); yield return Snap("A-" + tag);
                string saved = System.IO.File.ReadAllText(System.IO.Path.Combine(flow.SaveRoot, "woodland-acorns-v1.json"));
                Note($"  {tag}: through {s.id} ({s.title}): found {before} -> {col.Found}, in the save file {saved.Contains(s.id)}, banner '{AcornBanner.Current?.Shown}' (player {AcornBanner.Current?.Player})");
                car.Body.isKinematic = false;
            }
            if (open.Count < 3) { Note("  fewer than 3 acorns left in the copy: check stopped"); yield break; }
            yield return Through(race.vehicle, open[0], "one-player-normal");
            flow.SetHighMode(true); yield return new WaitForSecondsRealtime(1.5f); Bind(); Note($"  HIGH MODE on: {race.high}");
            yield return Through(race.vehicle, open[1], "one-player-high");
            flow.SetHighMode(false); yield return new WaitForSecondsRealtime(1);
            var pad2 = InputSystem.AddDevice<Gamepad>("Report1300r5Pad2");
            SplitScreen.Mode = SplitScreen.Kind.FreeRoam; SplitScreen.P1Device = pad; SplitScreen.P2Device = pad2; SplitScreen.P2Ai = false; SplitScreen.Traffic = false;
            flow.StartSplit(); yield return WaitRacing(); Bind(); yield return new WaitForSecondsRealtime(2); col = race.GetComponent<ExplorationCollection>();
            var p2 = SplitScreen.Race ? SplitScreen.Race.P2Car : null; Note($"  split screen Free Roam: active {SplitScreen.Active}, player 2's vehicle {(p2 ? p2.name : "none")}, acorns {col.Found}/{col.sites.Length}");
            if (p2) yield return Through(p2, open[2], "split-player-2");
            yield return new WaitForSecondsRealtime(1); yield return Snap("A-split-banner-after");
        }
        // ---------------------------------------------------------------- Part B
        IEnumerator Order1300r5()
        {
            Conditions(); yield return PanelLoop("order-start"); yield return ToCampaign(); yield return ToSlot(true);
            string List() => string.Join(", ", CampaignData.InChapter(5).Select((e, i) => $"{i + 1} {e.Name} ${e.Pay:N0}+{e.Bonus:N0}{(Campaign.Available(e) ? "" : " (locked)")}{(Campaign.Passed(e) ? " (passed)" : "")}"));
            Note($"== Dan's copy, {Campaign.SlotName}: chapter {Campaign.Current.chapter}, kept order {Campaign.Current.stuntOrderKept}; chapter 5: {List()}");
            yield return Choose("chapter-5"); yield return new WaitForSecondsRealtime(1); yield return Snap("B-chapter5-dan");
            var c = Campaign.Current;
            // a run that had passed Red Clay Session, Park Competition and Stunt Opener before the swap (Summit Tricks was open to it then)
            c.results.RemoveAll(r => r.id.StartsWith("c5-") && r.id != "c5-park-session" && r.id != "c5-park-competition" && r.id != "c5-stunt-opener"); c.stuntOrderKept = true;
            Note($"== a run that had passed the first three before the swap: {List()}");
            Menu.OpenCampaign(5); yield return new WaitForSecondsRealtime(1.2f); yield return Snap("B-chapter5-three-passed-before");
            c.results.RemoveAll(r => r.id.StartsWith("c5-")); c.stuntOrderKept = false; c.stuntOrder = 1305;
            Note($"== a fresh chapter 5 (a new run): {List()}");
            foreach (var id in new[] { "c5-park-session", "c5-park-competition", "c5-stunt-opener" }) { c.results.Add(new Campaign.Result { id = id, passed = true }); }
            Note($"   after the first three passed: {List()}");
            Menu.OpenCampaign(5); yield return new WaitForSecondsRealtime(1.2f); yield return Snap("B-chapter5-fresh-three-passed");
            Campaign.Load(flow.Save.DirectoryPath); // nothing above was saved
        }
        // ---------------------------------------------------------------- Part C
        IEnumerator Land1300r5()
        {
            Campaign.Testing = true; Conditions(); int course = int.Parse(Arg("-p119course", "4"));
            yield return QuickTo115(course); Conditions(); if (!race.stunts) flow.StepRaceType(1);
            var st = flow.Save.Settings; st.opponents = false; st.traffic = false; st.laps = 1; flow.Save.SaveSettings(); race.opponents = false; race.traffic = false; race.laps = 1;
            var results = new List<string>();
            foreach (var vehicle in Arg("-p1300vehicles", "stuntbuggy,stuntbike").Split(','))
            {
                flow.OpenGarage(); flow.SelectVehicle(vehicle); flow.CloseGarage(); flow.StartRace(); yield return WaitRacing(); Bind();
                var car = race.vehicle; var acts = flow.Activities; race.finishGraceSeconds = 100000;
                Note($"== {RacePlaylists.Titles[course]} Stunt Race ({race.courseName}), {vehicle}: judging on (StuntsOn {acts.StuntsOn})");
                // the road's flattest, steepest downhill and most banked stretches (every 10 m, ground normal straight below the line)
                var spots = new List<(string kind, float s, float slope, float bank)>();
                for (float s = race.Origin + 60; s < race.Origin + race.road.Length - 60; s += 10)
                {
                    var p = race.road.At(s, out var f); f = f.normalized; if (!Physics.Raycast(p + Vector3.up * 3, Vector3.down, out var hit, 8, car.groundMask, QueryTriggerInteraction.Ignore)) continue;
                    var p2 = race.road.At(s + 25, out _); var pm = race.road.At(s + 12, out _); float slope = Mathf.Atan2(p2.y - p.y, Flat(p2 - p).magnitude) * Mathf.Rad2Deg, s1 = Mathf.Atan2(pm.y - p.y, Flat(pm - p).magnitude) * Mathf.Rad2Deg, s2 = Mathf.Atan2(p2.y - pm.y, Flat(p2 - pm).magnitude) * Mathf.Rad2Deg; if (Mathf.Abs(s1 - s2) > 6 || slope < -28) continue; // a steady slope, not a drop
                    var right = Vector3.Cross(Vector3.up, Flat(f)).normalized; float bank = Mathf.Asin(Mathf.Clamp(Vector3.Dot(hit.normal, right), -1, 1)) * Mathf.Rad2Deg;
                    if (race.GetComponent<MountainFlights>() is MountainFlights mf && mf.flights != null && mf.flights.Any(x => s >= x.approachStation - 40 && s <= x.endStation + 20)) continue;
                    spots.Add(("", s, slope, bank));
                }
                var picks = new List<(string kind, float s, float slope, float bank)>();
                var flat = spots.OrderBy(x => Mathf.Abs(x.slope) + Mathf.Abs(x.bank)).First(); picks.Add(("flat", flat.s, flat.slope, flat.bank));
                var down = spots.OrderBy(x => x.slope).First(); picks.Add(("downhill", down.s, down.slope, down.bank));
                var banked = spots.OrderByDescending(x => Mathf.Abs(x.bank)).First(); picks.Add(("banked", banked.s, banked.slope, banked.bank));
                foreach (var pk in picks)
                {
                    yield return Launch(car, acts, pk.s - 18, 0, $"{vehicle} {pk.kind} (s {pk.s:F0}, slope {pk.slope:F0}°, bank {pk.bank:F0}°), wheels down", results);
                }
                // touch-downs on the side and the roof (on the flat stretch): turned just before the touch
                foreach (var roll in new[] { 65f, 90f, 115f, 130f, 180f }) yield return Launch(car, acts, flat.s - 18, roll, $"{vehicle} flat, turned {roll:F0}° onto its side / roof just before the touch", results);
            }
            Note("== summary"); foreach (var r in results) Note("  " + r);
        }
        IEnumerator Launch(ArcadeVehicle car, ArcadeActivities acts, float s, float roll, string what, List<string> results)
        {
            var p = race.road.At(s, out var f); f = Flat(f).normalized;
            Put(car, race.road.At(s - 36, out _), Yaw(f), 14); float t0 = Time.time; while (Time.time - t0 < 2.6f) { race.road.At(race.road.Project(car.Body.position, out _), out var d); f = Flat(d).normalized; car.Body.rotation = Quaternion.LookRotation(f, car.transform.up); car.Body.linearVelocity = f * 14 + Vector3.up * Mathf.Min(0, car.Body.linearVelocity.y); yield return new WaitForFixedUpdate(); }
            float at = acts.LastResultAt; int bails = acts.Bails; car.Body.linearVelocity = f * 14 + Vector3.up * 7.5f; bool turned = false, aired = false; string touch = "";
            t0 = Time.time;
            while (Time.time - t0 < 7 && acts.LastResultAt == at)
            {
                yield return new WaitForFixedUpdate();
                if (roll > 0 && !turned && car.Body.linearVelocity.y < 0 && Physics.Raycast(car.Body.worldCenterOfMass, Vector3.down, out var h, 30, car.groundMask, QueryTriggerInteraction.Ignore) && h.distance < 1.6f)
                { turned = true; car.Body.rotation = Quaternion.LookRotation(f, Vector3.up) * Quaternion.Euler(0, 0, roll); car.Body.angularVelocity = Vector3.zero; }
                if (car.GroundedWheels == 0) aired = true;
                if (aired && touch == "" && (car.GroundedWheels > 0)) touch = $"wheels first, up·ground {Vector3.Dot(car.transform.up, ArcadeActivities.GroundAt(car)):F2}, up·world {car.transform.up.y:F2}";
            }
            yield return new WaitForSeconds(.3f);
            string verdict = acts.LastResultAt == at ? "no verdict in 7 s" : acts.LastResult.Replace("\n", " ");
            string line = $"{what}: {verdict}{(acts.Bails > bails ? " — bail: " + acts.LastBail : "")}; first-touch tilt from the ground {acts.LastTouchTilt:F0}°; {(touch == "" ? "body touched first" : touch)}; wiped out {car.GetComponent<VehicleConfiguration>().WipedOut}";
            Note("  " + line); results.Add(line);
            if (roll >= 90 || acts.Bails > bails) yield return Snap($"C-{race.gameObject.scene.name}-{car.GetComponent<VehicleConfiguration>().profileId}-{roll:F0}");
            car.GetComponent<VehicleConfiguration>().Recover(); yield return new WaitForSeconds(.2f);
        }
        // ---------------------------------------------------------------- Part D
        IEnumerator Lev1300r5()
        {
            foreach (var m in InputSystem.devices.OfType<Mouse>().ToList()) InputSystem.DisableDevice(m);
            Campaign.Testing = true; DriverVariation.Seed = 1305; Conditions();
            yield return QuickTo115(9); Conditions(); if (race.stunts) flow.StepRaceType(1);
            var st = flow.Save.Settings; st.opponents = false; st.traffic = false; st.laps = 1; flow.Save.SaveSettings(); race.opponents = false; race.traffic = false; race.laps = 1;
            flow.OpenGarage(); flow.SelectVehicle("original"); flow.CloseGarage();
            flow.StartRace(); yield return WaitRacing(); Bind();
            var g = race.GetComponent<LeviathanGuide>(); var me = race.Racers[0]; var car = race.vehicle;
            Note($"== {race.courseName} ({race.courseId}), normal race, stunts {race.stunts}: {race.gates.Length} gates; CP 28 {V(race.GatePosition(28))}, CP 36 {V(race.GatePosition(36))} ({Vector3.Distance(race.GatePosition(28), race.GatePosition(36)):F0} m apart); CP 24 {V(race.GatePosition(24))}, CP 41 {V(race.GatePosition(41))}");
            Note($"   shortcuts in the race: {string.Join(", ", race.Branches.Select(b => $"{b.title} (s {b.entryRoad:F0}-{b.exitRoad:F0})"))}; the mountain: s {LeviathanCourse.Current.mountain[0]:F0}-{LeviathanCourse.Current.mountain[1]:F0}");
            Note($"   the scene's route arrows hidden in this race ({LeviathanCourse.ArrowReport.Count}): {string.Join("; ", LeviathanCourse.ArrowReport)}");
            // run 1: the guide's way all the lap
            yield return GuideLap(car, me, g, false);
            // run 2: straight on at each end of the out-and-back
            flow.StartRace(); yield return WaitRacing(); Bind(); car = race.vehicle; me = race.Racers[0]; g = race.GetComponent<LeviathanGuide>();
            yield return GuideLap(car, me, g, true);
        }
        static float Bearing(ArcadeVehicle car, Vector3 at) => Vector3.SignedAngle(Flat(car.transform.forward), Flat(at - car.Body.position), Vector3.up);
        float Arrow(ArcadeVehicle car, RacerState me) { var a = LeviathanGuide.Ahead(race, me, 60, 10); return Bearing(car, a[a.Count - 1]); } // as RaceHud draws it
        IEnumerator GuideLap(ArcadeVehicle car, RacerState me, LeviathanGuide g, bool wrong)
        {
            Pilot(car, me); var pilot = car.GetComponent<RoadDriver>(); pilot.enabled = false; BindPad(); var vi = car.GetComponent<VehicleInput>(); var flights = race.GetComponent<MountainFlights>()?.flights ?? new MountainFlights.Flight[0]; bool aiFlight = false;
            Note($"-- {(wrong ? "run 2: the wrong way on purpose at each end of the out-and-back" : "run 1: the guide's way, the whole lap")}");
            int lastNext = me.Progress.NextGate, turnBacks = 0, lastTb = -1; float stuck = 0, t0 = Time.time; bool shot1 = false, shot2 = false, wrong1 = false, wrong2 = false; Vector3 wrongAt = default; float wrongT = -1; string phase = "";
            var j1 = new Vector3(721, 0, 1); var j2 = new Vector3(472, 0, -22);
            Time.timeScale = 2;
            while (Time.time - t0 < 1100 && (flow.State == RaceFlow.Stage.Racing || flow.State == RaceFlow.Stage.Paused) && !me.Progress.Finished)
            {
                yield return null; AudioListener.volume = 0; if (Time.timeScale == 0) { Note($"   time scale 0 (state {flow.State}, page {Page}): set to 2"); Time.timeScale = 2; }
                if (flow.State == RaceFlow.Stage.Paused) { Note($"   paused (page {Page}): resumed"); flow.Resume(); yield return null; continue; }
                int next = me.Progress.NextGate; var p = car.Body.position;
                if (next != lastNext) { Note($"   {Time.time - t0:0.0} s: CP {lastNext} passed (next {next}){(me.Progress.MissedGates > 0 ? $", missed {me.Progress.MissedGates}" : "")}"); lastNext = next; }
                if (g.TurnBackGate != lastTb) { if (g.TurnBackGate >= 0) { turnBacks++; Note($"   {Time.time - t0:0.0} s: TURN BACK to CP {g.TurnBackGate} ({g.TurnBackWhy}) at {V(p)}{(wrongT >= 0 ? $", {Time.time - wrongT:0.0} s and {Vector3.Distance(Flat(p), Flat(wrongAt)):0} m after the wrong turn" : "")}"); } else Note($"   {Time.time - t0:0.0} s: TURN BACK gone at {V(p)}"); lastTb = g.TurnBackGate; if (g.TurnBackGate >= 0) { Time.timeScale = 1; yield return Snap($"D-turnback-{turnBacks}"); Time.timeScale = 2; } }
                // the junction shots, from the driver's view as the end comes up on the way back
                if (!wrong && !shot1 && next == 37 && Vector3.Distance(Flat(p), j1) < 30) { shot1 = true; Time.timeScale = 1; Note($"   J1 on the way back, {Vector3.Distance(Flat(p), j1):0} m before the turn: the HUD guide arrow points {Arrow(car, me):+0;-0}° (- left), CP 37 drawn {Bearing(car, race.GatePosition(37)):+0;-0}°, the way up (CP 29) {Bearing(car, race.GatePosition(29)):+0;-0}°"); yield return Snap("D-J1-way-back-at-the-turn"); Time.timeScale = 2; }
                if (!wrong && !shot2 && (next == 41 || next == 42) && Vector3.Distance(Flat(p), j2) < 28) { shot2 = true; Time.timeScale = 1; Note($"   J2 on the way back (next CP {next}), {Vector3.Distance(Flat(p), j2):0} m before the turn: the HUD guide arrow points {Arrow(car, me):+0;-0}° (+ right), CP 42 drawn {Bearing(car, race.GatePosition(42)):+0;-0}°, the way in (CP 22) {Bearing(car, race.GatePosition(22)):+0;-0}°"); yield return Snap("D-J2-way-back-at-the-turn"); Time.timeScale = 2; }
                Vector3 target; float cap = 21;
                var ahead = LeviathanGuide.Ahead(race, me, 24, 6); target = ahead[ahead.Count - 1];
                if (g.TurnBackGate >= 0 || Vector3.Distance(Flat(ahead[0]), Flat(p)) > 30) { target = race.GatePosition(next); cap = 9; }
                else if (next > 0 && LeviathanGuide.ToNextGate(race, me) < 28) { target = race.GatePosition(next); cap = 14; } // the drawn gate ahead: through it // the HUD arrow: at the gate itself
                float station = race.Origin + me.RoadPosition; bool inFlight = !(phase != "" || wrong && (next == 37 && Vector3.Distance(Flat(p), j1) < 9 && !wrong1 || next == 42 && Vector3.Distance(Flat(p), j2) < 7 && !wrong2)); // the road AI drives the lap; the pad (the guide's way) at the two ends of the out-and-back on the way back // the summit flight and the ridge drop: the road AI (the pad lands short)
                if (inFlight != aiFlight) { aiFlight = inFlight; pilot.enabled = inFlight; vi.enabled = !inFlight; if (!inFlight) vi.Bind(pad); Note($"   {Time.time - t0:0.0} s: {(inFlight ? "the road AI drives" : "the pad drives by the guide")} (s {station:F0})"); }
                // run 2: straight on at J1 (north, the way up the mountain) and on at J2 (south, the way the course came in)
                if (wrong && !wrong1 && next == 37 && Vector3.Distance(Flat(p), j1) < 8) { wrong1 = true; wrongAt = p; wrongT = Time.time; phase = "J1"; Note($"   {Time.time - t0:0.0} s: at J1, driven straight on north (CP 29's way)"); }
                if (wrong && !wrong2 && next == 42 && Vector3.Distance(Flat(p), j2) < 6) { wrong2 = true; wrongAt = p; wrongT = Time.time; phase = "J2"; var w2 = new Vector3(479, 0, -40); Put(car, w2, Yaw(new Vector3(503, 0, -110) - w2), 10); Note($"   {Time.time - t0:0.0} s: at J2, set 20 m down the wrong road (south, CP 23's way) and driven on"); }
                if (phase != "" && Time.time - wrongT < 14 && g.TurnBackGate < 0) { target = phase == "J1" ? new Vector3(726, 0, 60) : new Vector3(500, 0, -110); cap = 14; }
                else if (phase != "" && g.TurnBackGate < 0 && Time.time - wrongT >= 14) { var a0 = LeviathanGuide.Ahead(race, me, 12, 6); Note($"   {phase}: no TURN BACK within 14 s of the wrong turn: {Vector3.Distance(Flat(p), Flat(wrongAt)):0} m from it, {Vector3.Distance(Flat(p), Flat(a0[0])):0} m off this pass of the course, at {V(p)}"); phase = ""; }
                else if (phase != "" && g.TurnBackGate >= 0) phase = "";
                if (!aiFlight) Drive(car, target, cap);
                stuck = car.Body.linearVelocity.magnitude < 1.5f ? stuck + Time.deltaTime : 0;
                if (stuck > 6 && !aiFlight) { Note($"   stuck at {V(p)} (next CP {next}): reset"); car.GetComponent<VehicleRespawn>()?.ResetVehicle(); stuck = 0; yield return new WaitForSecondsRealtime(1); Note($"   after the reset: {V(car.Body.position)}, next CP {me.Progress.NextGate}"); }
            }
            Release(); Time.timeScale = 1;
            Note($"   end (state {flow.State}): finished {me.Progress.Finished} in {me.Progress.RaceTime(race.Clock):F1} s, missed gates {me.Progress.MissedGates}, TURN BACK shown {turnBacks} time(s), wrong-way warnings {(race.GetComponent<WrongWayGuidance>()?.Visible ?? false ? "showing at the end" : "none at the end")}");
        }
    }
}
