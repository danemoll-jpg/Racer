using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Racer
{
    // 1.3.0-review8 evidence only (Report106PlayerCheck: -p106check <dir> -racerTestSave <dir>, a copy of Dan's save, 1280x800). Before: -r8GlanceOff
    // (barriers as before: no WallGlance) and -r8RoomsOff (HIGH bumpers built as before: no room at jumps, square ends).
    //   -p106kind jumps1300r8   B: every course (-p1300courses, default all ten) in a HIGH MODE race (and, -p1300normal 1, a normal race): every jump on the
    //                           course line and its shortcuts (JumpRooms, found from the ground), flown on the line and 3 m either side (and on the lane it
    //                           was found in) at the speed the course allows there (the fastest vehicle, the Needle 600): every bumper or barrier touched
    //                           in the air or on the landing run-out; a picture of the gap at the biggest jump of each mountain course
    //   -p106kind lap1300r8     A: one HIGH MODE lap of -p1300courses (default Mountain Loop Forward 4 and Leviathan Reverse 9) by the pad as a player,
    //                           steering 7 m off the line every 150 m (alternate sides, never near a jump): stalls against a bumper (2.5 s barely moving
    //                           within 3 m of one), glances, pins; three pictures of the first glance
    //   -p106kind wall1300r8    A: normal mode, the Street Classic, throttle held, hands off, into Leviathan Reverse's longest barricade and Mountain Loop's
    //                           "Road closed barrier 4" at 10, 20, 30 and 70 degrees: speed before, 0.6 s after, heading against the barrier's line;
    //                           three pictures of the 20 degree hit on each
    public sealed class Report1300r8Touch : MonoBehaviour
    {
        public readonly List<string> touched = new(); public ArcadeVehicle car; public bool counting = true; public string where = "";
        void OnCollisionEnter(Collision c) => Hit(c);
        void OnCollisionStay(Collision c) => Hit(c);
        void Hit(Collision c)
        {
            if (!c.collider || c.contactCount == 0 || !counting) return; bool bumper = HighBumpers.IsBumper(c.collider);
            if (!bumper && !GuideWalls.Is(c.collider)) return;
            var cp = c.GetContact(0); if (Mathf.Abs(cp.normal.y) > .5f && !bumper) return; // a natural barrier's top is ground
            string what = (bumper ? "bumper " : "barrier ") + c.collider.name + (car && car.GroundedWheels == 0 ? " IN THE AIR" : " on the ground") + $" at {cp.point.x:F0},{cp.point.y:F0},{cp.point.z:F0}{where}";
            if (!touched.Contains(what)) touched.Add(what);
        }
    }
    public sealed partial class Report106PlayerCheck
    {
        void Quiet1300r8()
        {
            // the rivals and traffic of a HIGH MODE race parked out of the way (the check flies one vehicle at a time)
            foreach (var c in FindObjectsByType<ArcadeVehicle>(FindObjectsSortMode.None)) if (c && c != race.vehicle && c.gameObject.activeSelf) c.gameObject.SetActive(false);
        }
        IEnumerator HighRace1300r8(int course, bool high)
        {
            yield return QuickTo115(course); Conditions();
            if (HighMode.On != high) flow.SetHighMode(high);
            if (!high && race.stunts) flow.StepRaceType(1);
            var st = flow.Save.Settings; if (!high) { st.opponents = false; st.traffic = false; st.laps = 1; flow.Save.SaveSettings(); race.opponents = false; race.traffic = false; race.laps = 1; }
            flow.StartRace(); yield return WaitRacing(); Bind(); Quiet1300r8(); race.finishGraceSeconds = 100000;
            yield return Vehicle1300r8("moto");
        }
        IEnumerator Vehicle1300r8(string id)
        {
            var cfg = race.vehicle.GetComponent<VehicleConfiguration>(); if (cfg.profileId == id) yield break;
            cfg.Apply(id); yield return new WaitForFixedUpdate(); Bind();
        }
        // the race's lines with walls (the course line and its open shortcuts), as HighBumpers builds them
        List<(string name, System.Func<float, (Vector3, Vector3)> at, System.Func<float, float> half, float length, bool loop)> Lines1300r8()
        {
            var list = new List<(string, System.Func<float, (Vector3, Vector3)>, System.Func<float, float>, float, bool)>();
            var main = race.road; main.Initialize(); list.Add(("course line", s => { var p = main.At(s, out var f); return (p, f); }, main.HalfWidth, main.Length, !main.openHighway));
            foreach (var b in race.Branches ?? new WoodlandRoute[0]) if (b && b.isActiveAndEnabled && b.points != null && b.points.Length > 1) { var r = b; list.Add((r.title, s => { var p = r.At(s, out var f); return (p, f); }, _ => r.halfWidth, r.Length, false)); }
            return list;
        }
        IEnumerator Jumps1300r8()
        {
            foreach (var m in InputSystem.devices.OfType<Mouse>().ToList()) InputSystem.DisableDevice(m);
            Campaign.Testing = true; Conditions(); string tag = (JumpRooms.Off ? "before" : "after") + (WallGlance.Off ? "-noglance" : "");
            var modes = Arg("-p1300normal", "0") == "1" ? new[] { true, false } : new[] { true };
            foreach (var cs in Arg("-p1300courses", "0,1,2,3,4,5,6,7,8,9").Split(','))
                foreach (bool high in modes)
                {
                    int course = int.Parse(cs); yield return HighRace1300r8(course, high);
                    var car = race.vehicle; var touch = car.GetComponent<Report1300r8Touch>() ?? car.gameObject.AddComponent<Report1300r8Touch>(); touch.car = car;
                    var lines = Lines1300r8(); var rooms = new List<JumpRooms.Room>();
                    for (int li = 0; li < lines.Count; li++) { var l = lines[li]; rooms.AddRange(JumpRooms.Find(race.gameObject.scene.GetPhysicsScene(), car.groundMask.value, l.at, l.half, l.length, l.loop, li, c => GuideWalls.Is(c))); }
                    rooms.AddRange(JumpRooms.Flights(race));
                    var jumps = JumpRooms.Distinct(rooms).OrderBy(r => r.line).ThenBy(r => r.s).ToList();
                    Note($"== {race.courseName} ({race.courseId}) {(high ? "HIGH MODE" : "normal")} [{tag}]: {jumps.Count} jumps ({rooms.Count} lane finds) on {lines.Count} lines; " +
                         (high ? $"bumper stations {HighBumpers.Samples}, left out for a jump {HighBumpers.RoomStations}, openings {HighBumpers.Openings}, run ends shaped {HighBumpers.Ends}" : $"guide walls: {FindObjectsByType<Collider>(FindObjectsSortMode.None).Count(c => c.gameObject.activeInHierarchy && GuideWalls.Is(c) && !HighBumpers.IsBumper(c))}"));
                    int touchedJumps = 0; var top = car.topSpeed;
                    foreach (var j in jumps)
                    {
                        var l = lines[j.line]; var offsets = new List<float> { -3, 0, 3 }; if (Mathf.Abs(j.lane) > 3.2f) offsets.Add(j.lane);
                        var hitsHere = new List<string>(); float bestAir = 0;
                        foreach (float off in offsets)
                        {
                            float speed = Mathf.Min(j.speed, top); var (p0, f0) = l.at(Mathf.Max(0, j.s - 45)); f0.y = 0; f0.Normalize();
                            var start = p0 + Vector3.Cross(Vector3.up, f0) * off; Put(car, start, Yaw(f0), speed * .97f); BindPad(); touch.touched.Clear(); touch.counting = false;
                            float t0 = Time.time, airFrom = -1, air = 0; var endAlong = Vector3.Dot(Flat(j.b - start), j.forward);
                            while (Time.time - t0 < 9)
                            {
                                yield return new WaitForFixedUpdate(); if (flow.State == RaceFlow.Stage.Paused) flow.Resume();
                                var p = car.Body.position; float along = Vector3.Dot(Flat(p - start), j.forward);
                                float ls = Mathf.Clamp(j.s + (along - 45) + 25, 0, l.length - .01f); var (q, _) = l.at(ls); var (qq, fq) = l.at(Mathf.Clamp(ls, 0, l.length - .01f)); fq.y = 0;
                                Drive(car, q + Vector3.Cross(Vector3.up, fq.normalized) * off, speed);
                                if (car.GroundedWheels == 0) { if (airFrom < 0) airFrom = Time.time; } else if (airFrom >= 0) { air = Mathf.Max(air, Time.time - airFrom); airFrom = -1; }
                                touch.counting = along >= 45 - JumpRooms.Before && along <= 45 + j.air + JumpRooms.RunOut; touch.where = $" ({along - 45:F0} m past the lip)";
                                if (along > endAlong) break;
                            }
                            Release(); bestAir = Mathf.Max(bestAir, air);
                            foreach (var h in touch.touched) hitsHere.Add($"{off:+0;-0;0} m: {h}");
                        }
                        if (hitsHere.Count > 0) touchedJumps++;
                        Note($"  jump on {l.name} s {j.s:F0} at {V(j.lip)} (lane {j.lane:+0.0;-0.0}): {j.air:F0} m in the air at {j.speed:F0} m/s allowed (flown at {Mathf.Min(j.speed, top):F0}), {j.clear:F1} m over the ground, longest flight {bestAir:F1} s; " +
                             (hitsHere.Count == 0 ? "nothing touched" : "TOUCHED: " + string.Join("; ", hitsHere.Distinct())));
                    }
                    Note($"  jumps with a bumper or barrier touched: {touchedJumps} of {jumps.Count}");
                    // the gap at the biggest jump of each mountain course (from above and behind its lip)
                    if (high && course is 4 or 5 or 8 or 9 && jumps.Count > 0)
                    {
                        var big = jumps.OrderByDescending(j => j.air).First();
                        yield return Free($"B-{race.courseId}-{tag}-biggest-jump", big.lip - big.forward * 18 + Vector3.up * 14, big.lip + big.forward * Mathf.Min(big.air, 60), 70, 1280, 800);
                        yield return Free($"B-{race.courseId}-{tag}-biggest-jump-landing", big.lip + big.forward * (big.air + 30) + Vector3.up * 16, big.lip + big.forward * (big.air * .6f), 70, 1280, 800);
                        Note($"  pictures: the biggest jump, s {big.s:F0} ({big.air:F0} m)");
                    }
                    flow.QuitRace(); yield return WaitMenu();
                }
            if (HighMode.On) flow.SetHighMode(false);
        }

        // ---------------------------------------------------------------- A: a HIGH MODE lap into the edges
        IEnumerator Lap1300r8()
        {
            foreach (var m in InputSystem.devices.OfType<Mouse>().ToList()) InputSystem.DisableDevice(m);
            Campaign.Testing = true; Conditions(); string tag = WallGlance.Off ? "before" : "after";
            foreach (var cs in Arg("-p1300courses", "4,9").Split(','))
            {
                int course = int.Parse(cs); yield return HighRace1300r8(course, true); yield return Vehicle1300r8("original");
                var car = race.vehicle; var me = race.Racers[0]; BindPad(); var road = race.road; float L = road.Length;
                var flights = race.GetComponent<MountainFlights>()?.flights ?? new MountainFlights.Flight[0];
                var bumpers = HighBumpers.Root(race.gameObject.scene).GetComponentsInChildren<MeshCollider>();
                bool NearJump(float s) { foreach (var j in HighBumpers.Rooms) if (j.line == 0 && Mathf.Abs(Mathf.DeltaAngle(s / L * 360, j.s / L * 360)) * L / 360 < j.air + 80) return true; foreach (var f in flights) if (road.Relative(s, f.approachStation - 120) < road.Relative(f.endStation + 40, f.approachStation - 120)) return true; return false; }
                int g0 = WallGlance.Glances, p0 = WallGlance.Pins, c0 = WallGlance.Crashes, log0 = WallGlance.Log.Count, stalls = 0, pushes = 0, resets0 = me.Recoveries; bool shot = false;
                float stuck = 0, t0 = Time.time, station = road.Project(car.Body.position, out _), lastPush = -1; var stallAt = new List<string>(); int lap0 = me.Progress.CompletedLaps, resets = 0, carried = 0; float offFor = 0, stuckAny = 0, nextNote = 0, lastReset = -99;
                Time.timeScale = 3; Note($"== A. {race.courseName} ({race.courseId}) HIGH MODE, one lap by the pad steering into the edges [{tag}]: line {L:F0} m, {bumpers.Length} bumper pieces");
                while (Time.time - t0 < 900 && me.Progress.CompletedLaps == lap0 && flow.State is RaceFlow.Stage.Racing or RaceFlow.Stage.Paused)
                {
                    yield return null; AudioListener.volume = 0; if (Time.timeScale == 0) Time.timeScale = 3;
                    if (flow.State == RaceFlow.Stage.Paused) { flow.Resume(); continue; }
                    var p = car.Body.position; station = road.ProjectNear(p, station, 60, out float lat); float speed = Flat(car.Body.linearVelocity).magnitude;
                    // the course's big flights (the jump check flies them): carried to just past the landing, on at 15 m/s
                    var flight = flights.FirstOrDefault(f => road.Relative(station, f.approachStation - 30) < road.Relative(f.endStation + 10, f.approachStation - 30));
                    if (flight != null) { var q = road.At(flight.endStation + 15, out var fq); Put(car, q, Yaw(Flat(fq)), 15); BindPad(); station = flight.endStation + 15; carried++; stuck = 0; stuckAny = 0; offFor = 0; yield return new WaitForSeconds(.5f); if (flow.State == RaceFlow.Stage.Paused) flow.Resume(); continue; }
                    offFor = Mathf.Abs(lat) > 25 ? offFor + Time.deltaTime : 0;
                    if (offFor > 4 && Time.time - lastReset > 15) { offFor = 0; lastReset = Time.time; resets++; Note($"    off the course at {V(p)} (s {station:F0}, {lat:F0} m off the line): the game's reset"); car.GetComponent<VehicleRespawn>()?.ResetVehicle(); yield return new WaitForSecondsRealtime(2); if (flow.State == RaceFlow.Stage.Paused) flow.Resume(); station = road.Project(car.Body.position, out _); continue; }
                    // the line ahead; every 150 m, 20 m aimed 7 m off the middle (alternate sides), never near a jump
                    float look = Mathf.Clamp(speed * .9f, 8, 22); var target = road.At(station + look, out var fa); int block = Mathf.FloorToInt(station / 150); float into = station - block * 150;
                    if (block > 0 && into > 60 && into < 80 && !NearJump(block * 150 + 60)) { target += Vector3.Cross(Vector3.up, Flat(fa).normalized) * (block % 2 == 0 ? 1 : -1) * 7; if (block * 150 != lastPush) { lastPush = block * 150; pushes++; } }
                    road.At(station + 10, out var f10); road.At(station + 40, out var f40); float bend = Vector3.Angle(Flat(f10), Flat(f40));
                    float cap = Mathf.Lerp(26, 9, Mathf.InverseLerp(10, 80, bend)); if (race.Leviathan && race.Leviathan.Mountain(station)) cap = Mathf.Min(cap, 16);
                    foreach (var f in flights) if (road.Relative(station, f.approachStation - 80) < road.Relative(f.endStation, f.approachStation - 80)) cap = Mathf.Max(cap, f.aiTakeoffSpeed + 1);
                    if (me.Branch.Route) target = LeviathanGuide.Ahead(race, me, 30, 5).Last();
                    Drive(car, target, cap, 0, 0, 0, 18);
                    if (!shot && WallGlance.Glances > g0 && !WallGlance.Off) { shot = true; float ts = Time.timeScale; Time.timeScale = 1; yield return Snap($"A-{race.courseId}-glance-0"); yield return new WaitForSeconds(.3f); yield return Snap($"A-{race.courseId}-glance-1"); yield return new WaitForSeconds(.6f); yield return Snap($"A-{race.courseId}-glance-2"); Time.timeScale = ts; }
                    // a stall against a bumper: barely moving 2.5 s within 3 m of one
                    bool near = Physics.OverlapSphere(p, 3f, ~0, QueryTriggerInteraction.Ignore).Any(HighBumpers.IsBumper);
                    stuck = speed < 1.5f && car.GroundedWheels > 0 ? stuck + Time.deltaTime : 0;
                    stuckAny = speed < 1.5f ? stuckAny + Time.deltaTime : 0;
                    if (Time.time > nextNote) { nextNote = Time.time + 30; Note($"    {Time.time - t0:F0} s: at {V(p)} s {station:F0} ({station / L * 100:F0} % of the lap), {speed:F0} m/s, next CP {me.Progress.NextGate}, off the line {lat:F1} m"); }
                    if (stuckAny > 6 && Time.time - lastReset > 15) { stuckAny = 0; lastReset = Time.time; resets++; Note($"    stuck 6 s at {V(p)} (s {station:F0}, near a bumper {near}): the game's reset"); car.GetComponent<VehicleRespawn>()?.ResetVehicle(); yield return new WaitForSecondsRealtime(2); if (flow.State == RaceFlow.Stage.Paused) flow.Resume(); station = road.Project(car.Body.position, out _); continue; }
                    if (stuck > 2.5f) { stuck = 0; if (near) { stalls++; stallAt.Add(V(p)); } InputSystem.QueueStateEvent(pad, new GamepadState { leftTrigger = 1, leftStick = new Vector2(lat > 0 ? 1 : -1, 0) }); yield return new WaitForSeconds(1.2f); }
                }
                Time.timeScale = 1; Release();
                Note($"  {Time.time - t0:F0} s, lap done {me.Progress.CompletedLaps > lap0}, {pushes} pushes; stalls against a bumper {stalls}{(stallAt.Count > 0 ? " at " + string.Join("; ", stallAt) : "")}; glances {WallGlance.Glances - g0}, pinned and turned {WallGlance.Pins - p0}, head on {WallGlance.Crashes - c0}; resets {me.Recoveries - resets0} (the check's own {resets}); carried past a big flight {carried}");
                foreach (var line in WallGlance.Log.Skip(log0).Take(12)) Note("    " + line);
                flow.QuitRace(); yield return WaitMenu();
            }
            if (HighMode.On) flow.SetHighMode(false);
        }

        // ---------------------------------------------------------------- A: normal mode into a barricade and a mountain barrier
        IEnumerator Wall1300r8()
        {
            foreach (var m in InputSystem.devices.OfType<Mouse>().ToList()) InputSystem.DisableDevice(m);
            Campaign.Testing = true; Conditions(); string tag = WallGlance.Off ? "before" : "after";
            Note($"== A. normal mode, the Street Classic into a barrier at 10, 20, 30 and 70 degrees, throttle held, hands off [{tag}]");
            // Leviathan Reverse's longest closure barricade
            yield return HighRace1300r8(9, false); yield return Vehicle1300r8("original");
            var all = FindObjectsByType<BoxCollider>(FindObjectsSortMode.None).Where(b => b.name == LeviathanCourse.BarrierRunName && b.size.x > 7).ToList();
            var runs = all.Where(b => !b.transform.parent.name.Contains("J1")).ToList(); var j1 = all.Where(b => b.transform.parent.name.Contains("J1")).ToList();
            if (Site1300r8(runs, out var bs)) { Note($"  the barricade: {bs.box.transform.parent.name}, {bs.box.size.x:F1} m long at {V(bs.box.transform.position)}; run-ins {bs.why}"); yield return Hits1300r8("barricade", bs, 24, tag); }
            else Note("  no barricade with a clear run-in");
            // the rail on Leviathan Reverse's mountain (J1, 1.3.0-review7; its chevron boards are breakable signs, as every sign)
            if (Site1300r8(j1, out var js)) { Note($"  the mountain rail: {js.box.transform.parent.name}, {js.box.size.x:F1} m long; run-ins {js.why}"); yield return Hits1300r8("mountain-rail", js, 24, tag); }
            flow.QuitRace(); yield return WaitMenu();
            Note("  (Mountain Loop's 'Road closed barrier 4' has no flat run-in on either side to measure from; no guardrail on the mountain courses has a collider)");
        }
        sealed class R8Site { public BoxCollider box; public Vector3 face, n, t; public string why; }
        static readonly float[] Angles1300r8 = { 10f, 20f, 30f, 70f };
        static Vector3 Start1300r8(Vector3 face, Vector3 n, Vector3 t, float angle, out Vector3 dir, out Vector3 hitAt)
        {
            dir = (t * Mathf.Cos(angle * Mathf.Deg2Rad) - n * Mathf.Sin(angle * Mathf.Deg2Rad)).normalized;
            hitAt = face + n * 1.2f; return hitAt - dir * Mathf.Min(14f, 2.5f / Mathf.Max(.17f, Mathf.Sin(angle * Mathf.Deg2Rad)) + 6);
        }
        // the barrier, face and way along it whose run-ins for every angle are on flat ground (within 0.8 m) with nothing in the way
        bool Site1300r8(List<BoxCollider> boxes, out R8Site site)
        {
            site = null; float best = float.MaxValue;
            foreach (var box in boxes)
            {
                var tr = box.transform; var ax = box.size.x * tr.lossyScale.x >= box.size.z * tr.lossyScale.z ? tr.right : tr.forward; ax = Flat(ax).normalized;
                var mid = tr.TransformPoint(box.center);
                foreach (int side in new[] { 1, -1 }) foreach (int way in new[] { 1, -1 })
                    {
                        var n = Vector3.Cross(Vector3.up, ax) * side; var t = ax * way;
                        if (!Physics.Raycast(mid + n * 6, -n, out var fh, 10, ~0, QueryTriggerInteraction.Ignore) || fh.collider != box) continue;
                        var face = fh.point - t * 1.5f; float worst = 0; bool clear = true;
                        foreach (float a in Angles1300r8)
                        {
                            var st = Start1300r8(face, n, t, a, out _, out var hitAt);
                            float y0 = TopAt(hitAt); for (int k = 0; k <= 6; k++) { var q = Vector3.Lerp(st, hitAt, k / 6f); worst = Mathf.Max(worst, Mathf.Abs(TopAt(q) - y0)); }
                            var from = new Vector3(st.x, TopAt(st) + .9f, st.z); var to = new Vector3(hitAt.x, y0 + .9f, hitAt.z);
                            if (Physics.Linecast(from, to, out var lh, ~0, QueryTriggerInteraction.Ignore) && !lh.collider.attachedRigidbody && lh.collider != box && lh.normal.y < .5f) clear = false;
                        }
                        if (clear && worst < 2f && worst < best) { best = worst; site = new R8Site { box = box, face = face, n = n, t = t, why = $"flat within {worst:0.00} m, clear" }; }
                    }
            }
            return site != null;
        }
        // one hit per angle near the barrier's middle: placed back from the face, heading into it at the angle, throttle held, hands off
        IEnumerator Hits1300r8(string what, R8Site site, float speed, string tag)
        {
            var car = race.vehicle; BindPad(); var n = site.n; var t = site.t; RaycastHit face = default; face.point = site.face;
            foreach (float angle in Angles1300r8)
            {
                var start = Start1300r8(face.point, n, t, angle, out var dir, out _);
                Put(car, start, Yaw(dir), speed); BindPad(); FindAnyObjectByType<ChaseCamera>()?.Snap();
                float tt = 0, touched = -1, before = 0, after = 0, head = 0, gap = 0; int pic = 0; bool pics = angle == 20f; int g0 = WallGlance.Glances, c0 = WallGlance.Crashes;
                while (tt < 4f)
                {
                    InputSystem.QueueStateEvent(pad, new GamepadState { rightTrigger = 1 }); yield return new WaitForFixedUpdate(); tt += Time.fixedDeltaTime;
                    if (flow.State == RaceFlow.Stage.Paused) flow.Resume();
                    var p = car.Body.position; float dist = Vector3.Dot(Flat(p - face.point), n);
                    if (touched < 0) { before = Flat(car.Body.linearVelocity).magnitude; if (dist < 1.4f) touched = tt; }
                    if (pics && touched >= 0 && pic == 0) { pic++; yield return Snap($"A-{what}-20deg-{tag}-0"); }
                    if (pics && touched >= 0 && pic == 1 && tt > touched + .3f) { pic++; yield return Snap($"A-{what}-20deg-{tag}-1"); }
                    if (touched >= 0 && tt >= touched + .6f && after == 0) { after = Flat(car.Body.linearVelocity).magnitude; head = Vector3.Angle(Flat(car.transform.forward), t); gap = dist; }
                    if (pics && after > 0 && pic == 2 && tt > touched + .9f) { pic++; yield return Snap($"A-{what}-20deg-{tag}-2"); }
                    if (after > 0 && (!pics || pic >= 3)) break;
                }
                Release();
                // the same run 10 m clear of the barrier (the ground's own drag): its speed at the same moment
                float free = 0; if (touched >= 0)
                {
                    yield return new WaitForSeconds(.3f); Put(car, start + n * 10, Yaw(dir), speed); BindPad(); float tf = 0;
                    while (tf < touched + .6f) { InputSystem.QueueStateEvent(pad, new GamepadState { rightTrigger = 1 }); yield return new WaitForFixedUpdate(); tf += Time.fixedDeltaTime; if (flow.State == RaceFlow.Stage.Paused) flow.Resume(); }
                    free = Flat(car.Body.linearVelocity).magnitude; Release();
                }
                Note($"  {what} at {angle:0} degrees: {before:0.0} m/s before, {after:0.0} m/s 0.6 s after ({(free > 0 ? after / free * 100 : 0):0} % of the same run clear of it, {free:0.0} m/s), heading {head:0} degrees off the barrier's line, {gap:0.0} m from it; glance {WallGlance.Glances - g0}{(WallGlance.Glances > g0 ? $" (the glance: {WallGlance.LastAngle:0} degrees, {WallGlance.LastBefore:0.0} -> {WallGlance.LastAfter:0.0} m/s)" : "")}, head on {WallGlance.Crashes - c0}{(touched < 0 ? " (NEVER TOUCHED)" : "")}");
                yield return new WaitForSeconds(.4f);
            }
        }
    }
}
