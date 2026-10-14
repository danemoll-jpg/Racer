using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

namespace Racer
{
    // 0.114 evidence only (the 0.106 check component, never active without -p106check and -racerTestSave)
    public sealed partial class Report106PlayerCheck
    {
        // ---------------------------------------------------------------- Part A: the generated buildings from outside
        //   -p106kind shots114a  Dan's three viewpoints (his report be9da1: position, heading, a driver's eye on the Drifter Twin), driver's
        //                        views along Trickum Rd both ways (0.113's views) and along Hwy 92 both ways, a top-down and a low orbit of a
        //                        Hwy 92 strip center, a Trickum Rd house and the Trickum church; then each of Dan's spots driven into at 10 m/s
        static readonly (string name, Vector3 at, float heading)[] DanViews = {
            ("BUG-001", new Vector3(-645.75f, 9.29f, 350.72f), 255.0f), ("BUG-002", new Vector3(-676.40f, 0, 369.61f), 292.0f), ("BUG-003", new Vector3(-648.69f, 9.16f, 275.77f), 257.1f) };
        static readonly (string name, Vector2 at)[] Orbits = { ("strip-Cleaners-hwy92", new Vector2(-655.2f, 443.2f)), ("house-trickum", new Vector2(-667.1f, 282.5f)), ("church-trickum", new Vector2(-507.6f, -597.8f)) };
        IEnumerator Shots114a()
        {
            yield return ToFreeRoam("drifter"); var car = race.vehicle;
            var plan = FindObjectsByType<Hwy92Roadside>(FindObjectsSortMode.None).FirstOrDefault(r => r.resource == Hwy92Roadside.Trickum);
            Note($"Trickum plan: {(plan ? string.Join("; ", plan.Report) : "no component")}");
            foreach (var c in FindObjectsByType<Canvas>(FindObjectsSortMode.None)) c.enabled = false;
            car.Body.isKinematic = true; car.transform.position += Vector3.down * 200; yield return new WaitForSeconds(6); // the night-window glow fades out after the clock is set
            foreach (var (name, at, heading) in DanViews)
            {
                var f = Quaternion.Euler(0, heading, 0) * Vector3.forward; var eye = new Vector3(at.x, TopAt(new Vector3(at.x, 40, at.z)) + 1.5f, at.z);
                yield return Free($"A-{name}-after", eye, eye + f * 40 + Vector3.up * 2.5f, 62, 1920, 1080);
            }
            foreach (var (name, at, f) in TrickumViews)
            {
                var eye = new Vector3(at.x, TopAt(new Vector3(at.x, 40, at.y)) + 1.4f, at.y);
                yield return Free($"A-{name}-after", eye, eye + new Vector3(f.x, 0, f.y) * 40 + Vector3.down * 1.2f, 62, 1600, 900);
            }
            foreach (var (name, at, f) in Hwy92Views())
            {
                var eye = new Vector3(at.x, TopAt(new Vector3(at.x, 40, at.y)) + 1.4f, at.y);
                yield return Free($"A-{name}-after", eye, eye + new Vector3(f.x, 0, f.y) * 40 + Vector3.down * 1.2f, 62, 1600, 900);
            }
            foreach (var (name, at) in Orbits)
            {
                float g = TopAt(new Vector3(at.x, 40, at.y));
                yield return Free($"A-top-{name}", new Vector3(at.x, g + 120, at.y), new Vector3(at.x, g, at.y), 60, 1400, 1400, true, 28);
                for (int k = 0; k < 4; k++) // a low orbit: four sides, 30 m out, 4 m up
                {
                    var d = Quaternion.Euler(0, 45 + 90 * k, 0) * Vector3.forward; var centre = new Vector3(at.x, g + 3, at.y);
                    yield return Free($"A-orbit-{name}-{k * 90 + 45}", centre + d * 30 + Vector3.up * 1, centre, 60, 1600, 900);
                }
            }
            foreach (var c in FindObjectsByType<Canvas>(FindObjectsSortMode.None)) c.enabled = true;
            // drive in: from each of Dan's spots toward the building at 10 m/s; the car must stop at the wall (not end up inside)
            car.Body.isKinematic = false;
            foreach (var (name, at, heading) in DanViews)
            {
                Put(car, at, heading, 10); var start = car.Body.position; float t0 = Time.time, closest = 0; string hit = "nothing";
                var contacts = car.gameObject.GetComponent<Report106Contacts>() ?? car.gameObject.AddComponent<Report106Contacts>(); contacts.who = name; contacts.list.Clear();
                while (Time.time - t0 < 6) { Drive(car, car.Body.position + Quaternion.Euler(0, heading, 0) * Vector3.forward * 20, 10); closest = Mathf.Max(closest, Vector3.Distance(Flat(car.Body.position), Flat(start))); yield return new WaitForFixedUpdate(); }
                Release(); if (contacts.list.Count > 0) hit = contacts.list[0];
                bool inside = InsideFootprint(car.Body.position);
                Note($"{name} drive-in at 10 m/s, heading {heading:F0}: went {closest:F1} m, first contact: {hit}; ended at {V(car.Body.position)}, inside a building footprint: {(inside ? "YES" : "no")}");
            }
        }
        // ---------------------------------------------------------------- Part B: the combined-course proposals in Free Roam
        //   -p106kind drive114   each proposal driven end to end by the virtual controller (-p106vehicle, default the Street Classic): the bot
        //                        steers along the proposal's centreline at a speed from its curvature; the time, each numbered corner's
        //                        time, every assist (stuck 5 s, 15 m off the line or 6 m below it: put back 25 m on), contacts, the longest
        //                        time in the air; -p114only A|B for one
        //   -p106kind shots114b  the markers: driver's views (start arch, a corner, the Lake shore junction, the mountain, shortcut entrances),
        //                        top-down shots, the minimap and the big map; then Settings > Display > "Show track proposals" with the
        //                        controller only (focus, right to Off: markers and map lines gone, right to On, B back)
        IEnumerator Drive114()
        {
            yield return ToFreeRoam(Arg("-p106vehicle", "original")); var car = race.vehicle; BindPad();
            var tp = TrackProposals.Current; Note($"track proposals: {(tp ? string.Join("; ", tp.Report) + $", shown {TrackProposals.Showing}" : "missing")}");
            if (!tp) yield break;
            string only = Arg("-p114only", "");
            foreach (var p in TrackProposals.Plan.proposals)
            {
                if (only != "" && p.id != only) continue;
                var pts = TrackProposals.Points(p.pts); var s = new float[pts.Length]; for (int i = 1; i < pts.Length; i++) s[i] = s[i - 1] + Vector3.Distance(Flat(pts[i]), Flat(pts[i - 1]));
                var corners = p.corners.ToDictionary(c => c.i, c => c.n); var cornerTimes = new List<string>(); var assists = new List<string>();
                var contacts = car.gameObject.GetComponent<Report106Contacts>() ?? car.gameObject.AddComponent<Report106Contacts>(); contacts.who = p.id; contacts.list.Clear();
                Put(car, pts[2], Yaw(pts[6] - pts[2]), 0); yield return new WaitForSeconds(1.5f);
                int idx = 2; float t0 = Time.time, lastMove = Time.time, air = 0, maxAir = 0, topSpeed = 0; Vector3 airFrom = default, maxAirFrom = default, maxAirTo = default; float bestS = s[2];
                while (idx < pts.Length - 3 && Time.time - t0 < 900)
                {
                    var pos = car.Body.position;
                    int lo = Mathf.Max(0, idx - 10), hi = Mathf.Min(pts.Length - 1, idx + 40); int best = idx; float bd = float.MaxValue;
                    for (int i = lo; i <= hi; i++) { float d = (Flat(pts[i]) - Flat(pos)).sqrMagnitude; if (d < bd) { bd = d; best = i; } }
                    for (int i = idx + 1; i <= best; i++) if (corners.TryGetValue(i, out int n)) cornerTimes.Add($"{p.id}{n} {Time.time - t0:F1}");
                    idx = Mathf.Max(idx, best); float speed = car.ForwardSpeed; topSpeed = Mathf.Max(topSpeed, speed);
                    if (s[idx] > bestS + 3) { bestS = s[idx]; lastMove = Time.time; }
                    bool off = Mathf.Sqrt(bd) > 15 || (car.GroundedWheels > 0 && pos.y < pts[idx].y - 6);
                    if (car.GroundedWheels == 0) { if (air == 0) airFrom = pos; air += Time.fixedDeltaTime; } else { if (air > maxAir) { maxAir = air; maxAirFrom = airFrom; maxAirTo = pos; } air = 0; }
                    if (Time.time - lastMove > 5 || off)
                    {
                        int j = idx; while (j < pts.Length - 1 && s[j] < s[idx] + 25) j++;
                        assists.Add($"{(off ? "off the line" : "stuck")} at {V(pos)} (s {s[idx]:F0}, {Time.time - t0:F0} s) -> put back at s {s[j]:F0}");
                        Put(car, pts[j], Yaw(pts[Mathf.Min(pts.Length - 1, j + 4)] - pts[j]), 8); idx = j; bestS = s[j]; lastMove = Time.time; yield return new WaitForFixedUpdate(); continue;
                    }
                    // the speed: from the curvature ahead (6 m/s2 sideways, 5 m/s2 braking); full pedal up steep climbs
                    float cap = 32;
                    for (int k = 0; k <= 60; k += 15)
                    {
                        int a = Ahead(s, idx, k), b = Ahead(s, idx, k + 30); float turn = Vector3.Angle(Flat(pts[b] - pts[a]), Flat(pts[Mathf.Min(a + 3, pts.Length - 1)] - pts[a]));
                        float r = turn < 1 ? 1e4f : 30 / (turn * Mathf.Deg2Rad); cap = Mathf.Min(cap, Mathf.Sqrt(Mathf.Clamp(6 * r, 49, 1100) + 2 * 5 * k));
                        if (k == 0 && pts[b].y - pts[a].y > 6) cap = Mathf.Max(cap, 18);
                    }
                    var target = pts[Ahead(s, idx, Mathf.Clamp(speed * .9f, 8, 26))];
                    Drive(car, target, Mathf.Max(cap, 8), 0, 0, 0, 22);
                    yield return new WaitForFixedUpdate();
                }
                Release(); float total = Time.time - t0;
                Note($"{p.title}: driven end to end in {total:F1} s ({p.length:F0} m, average {p.length / total * 2.237f:F0} mph, top {topSpeed * 2.237f:F0} mph), {assists.Count} assists; longest in the air {maxAir:F2} s from {V(maxAirFrom)} to {V(maxAirTo)} ({Vector3.Distance(Flat(maxAirFrom), Flat(maxAirTo)):F0} m)");
                Note($"  corners: {string.Join(", ", cornerTimes)}");
                foreach (var a in assists) Note("  assist: " + a);
                foreach (var c in contacts.list.Take(20)) Note("  contact: " + c);
            }
        }
        static int Ahead(float[] s, int i, float d) { float want = s[i] + d; while (i < s.Length - 1 && s[i] < want) i++; return i; }
        static int NearestIndex(Vector3[] pts, float x, float z, out float d2) { int k = 0; d2 = float.MaxValue; for (int i = 0; i < pts.Length; i++) { float d = (pts[i].x - x) * (pts[i].x - x) + (pts[i].z - z) * (pts[i].z - z); if (d < d2) { d2 = d; k = i; } } return k; }
        IEnumerator Eye114(string name, Vector3[] pts, int i, float back)
        {
            int a = Mathf.Max(0, i - Mathf.RoundToInt(back / 2)); var f = Flat(pts[Mathf.Min(pts.Length - 1, a + 5)] - pts[a]).normalized;
            var eye = new Vector3(pts[a].x, TopAt(new Vector3(pts[a].x, pts[a].y + 20, pts[a].z)) + 1.5f, pts[a].z);
            yield return Free(name, eye, eye + f * 40 + Vector3.up * 1.5f, 62, 1600, 900);
        }
        IEnumerator Shots114b()
        {
            yield return ToFreeRoam("original"); var car = race.vehicle; var tp = TrackProposals.Current;
            Note($"track proposals: {(tp ? string.Join("; ", tp.Report) + $", shown {TrackProposals.Showing}" : "missing")}"); if (!tp) yield break;
            foreach (var c in FindObjectsByType<Canvas>(FindObjectsSortMode.None)) c.enabled = false;
            car.Body.isKinematic = true; car.transform.position += Vector3.down * 200;
            foreach (var p in TrackProposals.Plan.proposals)
            {
                var pts = TrackProposals.Points(p.pts);
                { int e = pts.Length - 16; var f = Flat(pts[^1] - pts[e]).normalized; var eye = new Vector3(pts[e].x, TopAt(new Vector3(pts[e].x, pts[e].y + 20, pts[e].z)) + 1.5f, pts[e].z); yield return Free($"B-{p.id}-start-arch", eye, eye + f * 40 + Vector3.up * 2.5f, 62, 1600, 900); } // 30 m before the line
                var c3 = p.corners[Mathf.Min(2, p.corners.Length - 1)]; yield return Eye114($"B-{p.id}-corner-{p.id}{c3.n}", pts, c3.i, 50);
                foreach (var (name, x, z) in new[] { ("lake-junction", 474f, -27f), ("mountain-entry", 690f, -95f), ("summit-runup", 1100f, 160f), ("north-arc", 900f, 140f) })
                {
                    int k = NearestIndex(pts, x, z, out float d2); if (d2 < 400) yield return Eye114($"B-{p.id}-{name}", pts, k, 40);
                }
                foreach (var sc in new[] { p.shortcuts[0], p.shortcuts[p.shortcuts.Length - 1] })
                {
                    var q = TrackProposals.Points(sc.pts); int k = NearestIndex(pts, q[0].x, q[0].z, out _);
                    yield return Eye114($"B-{p.id}-shortcut-{sc.title.Split(' ')[0]}", pts, k, 45);
                }
            }
            foreach (var (name, x, z, size) in new[] { ("start", 320f, 520f, 45f), ("lake-junction", 500f, -40f, 70f), ("lake-mountain", 760f, -20f, 160f), ("summit", 1060f, 150f, 120f), ("street-sw-corner", -600f, -530f, 70f) })
            {
                float g = TopAt(new Vector3(x, 300, z)); yield return Free($"B-top-{name}", new Vector3(x, g + 400, z), new Vector3(x, g, z), 60, 1400, 1400, true, size);
            }
            foreach (var c in FindObjectsByType<Canvas>(FindObjectsSortMode.None)) c.enabled = true;
            // the minimap and the big map (the car on the Street Loop by the Lake shore junction)
            car.Body.isKinematic = false; Put(car, new Vector3(468, 0, 40), 170, 0); FindAnyObjectByType<ChaseCamera>()?.Snap(); yield return new WaitForSeconds(2); yield return Snap("B-minimap");
            var map = flow.GetComponent<ExplorationMap>(); if (map) { map.Open(); yield return new WaitForSecondsRealtime(1.5f); yield return Snap("B-big-map"); map.Close(); yield return new WaitForSecondsRealtime(1); }
            // the setting, controller only
            yield return Press(GamepadButton.Start); yield return new WaitForSecondsRealtime(.8f); Note($"pause: state {flow.State}, focus '{Focus}'");
            yield return FocusRow("settings"); yield return Press(GamepadButton.South); yield return new WaitForSecondsRealtime(.6f);
            yield return FocusRow("tab-display"); yield return Press(GamepadButton.South); yield return new WaitForSecondsRealtime(.6f);
            yield return FocusRow("track-proposals"); Note($"settings display: focus '{Focus}', setting {flow.Save.Settings.showTrackProposals}"); yield return Snap("B-settings-on");
            yield return Press(GamepadButton.DpadRight); yield return new WaitForSecondsRealtime(.5f);
            string saved = System.IO.File.ReadAllText(System.IO.Path.Combine(flow.Save.DirectoryPath, "settings.json"));
            Note($"right: setting {flow.Save.Settings.showTrackProposals}, markers shown {TrackProposals.Showing}, settings.json says {(saved.Contains("\"showTrackProposals\": false") ? "false" : saved.Contains("\"showTrackProposals\": true") ? "true" : "missing")}"); yield return Snap("B-settings-off");
            yield return Press(GamepadButton.East); yield return new WaitForSecondsRealtime(.6f); Note($"B: state {flow.State}, page '{Page}', focus '{Focus}'");
            yield return Press(GamepadButton.East); yield return new WaitForSecondsRealtime(1f); Note($"B: state {flow.State}");
            yield return new WaitForSeconds(1); yield return Snap("B-minimap-off");
            if (map) { map.Open(); yield return new WaitForSecondsRealtime(1.5f); yield return Snap("B-big-map-off"); map.Close(); yield return new WaitForSecondsRealtime(1); }
            yield return Press(GamepadButton.Start); yield return new WaitForSecondsRealtime(.8f); yield return FocusRow("settings"); yield return Press(GamepadButton.South); yield return new WaitForSecondsRealtime(.6f);
            yield return FocusRow("tab-display"); yield return Press(GamepadButton.South); yield return new WaitForSecondsRealtime(.6f); yield return FocusRow("track-proposals");
            yield return Press(GamepadButton.DpadRight); yield return new WaitForSecondsRealtime(.5f); Note($"right again: setting {flow.Save.Settings.showTrackProposals}, markers shown {TrackProposals.Showing}");
            yield return Press(GamepadButton.East); yield return new WaitForSecondsRealtime(.6f); yield return Press(GamepadButton.East); yield return new WaitForSecondsRealtime(1f); Note($"B, B: state {flow.State}");
        }
        static (string name, Vector2 at, Vector2 f)[] Hwy92Views() => new (string, Vector2, Vector2)[] { // the CR113 highway centreline, 3.5 m into each lane
            ("Hm550e", new Vector2(-550.1f, 536.9f), new Vector2(1.000f, -0.030f)), ("Hm550w", new Vector2(-549.9f, 543.9f), new Vector2(-1.000f, 0.030f)), ("Hm400e", new Vector2(-400.4f, 530.8f), new Vector2(0.999f, -0.035f)), ("Hm400w", new Vector2(-400.2f, 537.8f), new Vector2(-0.999f, 0.035f)), ("Hm250e", new Vector2(-250.5f, 526.3f), new Vector2(1.000f, -0.015f)), ("Hm250w", new Vector2(-250.3f, 533.3f), new Vector2(-1.000f, 0.015f)), ("Hm100e", new Vector2(-100.4f, 525.6f), new Vector2(1.000f, 0.000f)), ("Hm100w", new Vector2(-100.4f, 532.6f), new Vector2(-1.000f, -0.000f)), ("H50e", new Vector2(50.2f, 530.6f), new Vector2(0.955f, 0.295f)), ("H50w", new Vector2(48.2f, 537.3f), new Vector2(-0.955f, -0.295f)), ("H200e", new Vector2(199.8f, 548.5f), new Vector2(0.997f, 0.080f)), ("H200w", new Vector2(199.2f, 555.4f), new Vector2(-0.997f, -0.080f)) };
        static bool InsideFootprint(Vector3 p)
        {
            foreach (var h in Physics.RaycastAll(new Vector3(p.x, p.y + 40, p.z), Vector3.down, 80, ~0, QueryTriggerInteraction.Ignore))
                if (h.collider is MeshCollider && h.collider.transform.parent && h.collider.transform.parent.name.EndsWith("solids") && h.point.y > p.y) return true; // a building's roof right above the car
            return false;
        }
    }
}
