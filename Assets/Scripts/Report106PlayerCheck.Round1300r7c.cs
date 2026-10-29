using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Racer
{
    // 1.3.0-review7 Part C evidence only (Report106PlayerCheck -p106kind holes1300r7, Free Roam, a copy of Dan's save): the Hwy 92 band (x -1260..1000,
    // z -p1300z0..-p1300z1), every metre: the top ground under it (no triggers, vehicles, trunks or the world edge). A hole is a metre with no ground at
    // all, or ground 3 m or more below the ground 3 m round it on three sides or more (a pit). Holes are grouped; each group: where, how big, what
    // stands round it (the colliders and renderers whose bounds reach it), a shot from 20 m off and one from above. -p1300drive 1: then the car along
    // both shoulders of Hwy 92 at speed (the highway's line found as the RaceRoad named for it), each fall below the ground noted.
    public sealed partial class Report106PlayerCheck
    {
        static bool Hole1300r7(float x, float z, out float y)
        {
            y = float.NaN;
            foreach (var h in Physics.RaycastAll(new Vector3(x, 400, z), Vector3.down, 800, ~0, QueryTriggerInteraction.Ignore))
            {
                var c = h.collider; if (c.isTrigger || c.attachedRigidbody || c.name.StartsWith("World edge") || c.name.IndexOf("trunk", System.StringComparison.OrdinalIgnoreCase) >= 0) continue;
                if (float.IsNaN(y) || h.point.y > y) y = h.point.y;
            }
            return float.IsNaN(y);
        }
        IEnumerator Holes1300r7()
        {
            yield return ToFreeRoam("original"); var car = race.vehicle; car.Body.isKinematic = true;
            foreach (var c in FindObjectsByType<Canvas>(FindObjectsSortMode.None)) c.enabled = false;
            float x0 = float.Parse(Arg("-p1300x0", "-1260")), x1 = float.Parse(Arg("-p1300x1", "1000")), z0 = float.Parse(Arg("-p1300z0", "460")), z1 = float.Parse(Arg("-p1300z1", "660"));
            int W = Mathf.CeilToInt(x1 - x0), H = Mathf.CeilToInt(z1 - z0); var y = new float[W, H];
            var roads = FindObjectsByType<RaceRoad>(FindObjectsSortMode.None); Note($"roads in Free Roam: {string.Join(", ", roads.Select(r => $"{r.name} ({r.Length:F0} m)"))}");
            int none = 0;
            for (int i = 0; i < W; i++) { for (int j = 0; j < H; j++) if (Hole1300r7(x0 + i + .5f, z0 + j + .5f, out y[i, j])) none++; if (i % 200 == 0) yield return null; }
            var bad = new List<Vector2Int>();
            for (int i = 3; i < W - 3; i++) for (int j = 3; j < H - 3; j++)
            {
                if (float.IsNaN(y[i, j])) { bad.Add(new Vector2Int(i, j)); continue; }
                int lower = 0; foreach (var d in new[] { new Vector2Int(3, 0), new Vector2Int(-3, 0), new Vector2Int(0, 3), new Vector2Int(0, -3) }) { float n = y[i + d.x, j + d.y]; if (!float.IsNaN(n) && n - y[i, j] > 3) lower++; }
                if (lower >= 3) bad.Add(new Vector2Int(i, j));
            }
            // groups (8-connected, 2 m reach)
            var left = new HashSet<Vector2Int>(bad); var groups = new List<List<Vector2Int>>();
            while (left.Count > 0) { var s = left.First(); left.Remove(s); var g = new List<Vector2Int> { s }; for (int k = 0; k < g.Count; k++) for (int dx = -2; dx <= 2; dx++) for (int dz = -2; dz <= 2; dz++) { var q = g[k] + new Vector2Int(dx, dz); if (left.Remove(q)) g.Add(q); } groups.Add(g); }
            Note($"== Hwy 92 band x {x0:F0}..{x1:F0}, z {z0:F0}..{z1:F0}: {W * H} metres, {none} with no ground at all; {groups.Count} hole(s)/pit(s)");
            var all = FindObjectsByType<Renderer>(FindObjectsSortMode.None); var cols = FindObjectsByType<Collider>(FindObjectsSortMode.None);
            int n0 = 0;
            foreach (var g in groups.OrderByDescending(g => g.Count).Take(int.Parse(Arg("-p1300holes", "16"))))
            {
                var c = new Vector3(x0 + (float)g.Average(q => q.x) + .5f, 0, z0 + (float)g.Average(q => q.y) + .5f); var ring = new List<float>();
                foreach (var d in new[] { 4, -4 }) { if (!float.IsNaN(Ground1300r7(c + new Vector3(d, 0, 0)))) ring.Add(Ground1300r7(c + new Vector3(d, 0, 0))); if (!float.IsNaN(Ground1300r7(c + new Vector3(0, 0, d)))) ring.Add(Ground1300r7(c + new Vector3(0, 0, d))); }
                c.y = ring.Count > 0 ? ring.Max() : 0; int noGround = g.Count(q => float.IsNaN(y[q.x, q.y]));
                var b = new Bounds(c, new Vector3(6, 30, 6));
                var rs = all.Where(r => r && r.enabled && r.bounds.Intersects(b)).Select(r => r.name).Distinct().Take(8); var cs = cols.Where(k => k && k.enabled && k.bounds.Intersects(b)).Select(k => k.name).Distinct().Take(8);
                string tag = $"C-hole-{++n0}-{c.x:F0}-{c.z:F0}";
                Note($"   {tag}: {g.Count} m² ({noGround} with no ground), x {x0 + g.Min(q => q.x):F0}..{x0 + g.Max(q => q.x) + 1:F0}, z {z0 + g.Min(q => q.y):F0}..{z0 + g.Max(q => q.y) + 1:F0}, ground round it {c.y:F1}; renderers: {string.Join(", ", rs)}; colliders: {string.Join(", ", cs)}");
                if (n0 <= 10) { yield return Free(tag, c + new Vector3(0, 9, -20), c, 55, 1280, 800); yield return Free(tag + "-top", c + Vector3.up * 60, c, 60, 800, 800, true, 14); }
            }
            Note("seams filled: " + string.Join("; ", RoamWorld.SeamReport));
            if (Arg("-p1300drive", "0") == "1") { car.Body.isKinematic = false; yield return Shoulders1300r7(); car.Body.isKinematic = true; }
            foreach (var q in new[] { new Vector3(799, 0, 548), new Vector3(799, 0, 575), new Vector3(738, 0, 517) }) { var c = q; c.y = Ground1300r7(c + Vector3.left * 6); yield return Free($"C-spot-{q.x:F0}-{q.z:F0}", c + new Vector3(-14, 6, q.z < 561 ? 12 : -12), c, 55, 1280, 800); yield return Free($"C-spot-{q.x:F0}-{q.z:F0}-top", c + Vector3.up * 60, c, 60, 800, 800, true, 14); }
            // the flip ramp at Hwy 92's east end (item 18), whatever was found: the view Dan had (down the shoulder toward it)
            var f = new Vector3(936, 0, 563); f.y = Ground1300r7(f + Vector3.left * 30);
            yield return Free("C-flip-18-from-the-road", f + new Vector3(-45, 4, -10), f, 55, 1280, 800);
            yield return Free("C-flip-18-top", f + Vector3.up * 60, f, 60, 800, 800, true, 30);
            car.Body.isKinematic = false;
        }
        // both shoulders of Hwy 92 by the pad, flat out east past x 800 and back: every 0.1 s the car's height over the ground under it, a fall noted;
        // a chase-camera shot as it passes each filled spot
        IEnumerator Shoulders1300r7()
        {
            var car = race.vehicle; Pilot(car, race.Racers[0]); car.GetComponent<RoadDriver>().enabled = false; car.enabled = true; BindPad();
            foreach (var c in FindObjectsByType<Canvas>(FindObjectsSortMode.None)) c.enabled = true;
            var road = FindObjectsByType<RaceRoad>(FindObjectsSortMode.None).FirstOrDefault(r => r.name.Contains("CR113 highway")); if (!road) { Note("   no Hwy 92 line found"); yield break; }
            road.Initialize(); Note($"   driving from {V(car.Body.position)} along {road.name} ({road.Length:F0} m)");
            // the pad along Hwy 92's own line, on a shoulder (2.5 m past the paved edge): east on the right-hand (south) one to x 860, back west on the north one to x 700
            foreach (var (east, name) in new[] { (true, "south"), (false, "north") })
            {
                float t0 = Time.time, maxSpeed = 0, minOver = float.MaxValue; bool fell = false, shot = false; int resets0 = race.Racers[0].Recoveries; float x0 = car.Body.position.x;
                while (Time.time - t0 < 90 && (east ? car.Body.position.x < 860 : car.Body.position.x > 700))
                {
                    var p = car.Body.position; float s = road.Project(p, out _); road.At(s, out var f0); int dir = (f0.x > 0) == east ? 1 : -1;
                    float s2 = s + dir * 30; var q = road.At(s2, out var f); var right = Vector3.Cross(Vector3.up, f * dir).normalized; float side = east ? 1 : 1; // the right-hand shoulder of the way driven
                    q += right * side * (road.HalfWidth(s2) + 2.5f); bool near = Mathf.Abs(p.x - 800) < 120;
                    Drive(car, q, near ? 30 : 34); yield return new WaitForSeconds(.1f); if (!car.enabled) car.enabled = true;
                    var line = road.At(s, out _); minOver = near ? Mathf.Min(minOver, p.y - line.y) : minOver; maxSpeed = Mathf.Max(maxSpeed, near ? car.Body.linearVelocity.magnitude : 0);
                    if (p.y < line.y - 4) { fell = true; Note($"   FELL beside Hwy 92 at {V(p)} (road {line.y:F1} m)"); break; }
                    if (!shot && Mathf.Abs(p.x - (east ? 785 : 815)) < 5) { shot = true; float ts = Time.timeScale; Time.timeScale = 0; yield return Snap($"C-after-{name}-shoulder-{(east ? "east" : "west")}bound"); Note($"   at the filled spot ({name} shoulder) {V(p)}, {car.Body.linearVelocity.magnitude * 2.237f:0} mph, {Mathf.Abs(road.ProjectNear(p, s, 30, out float lat) - s) * 0 + Mathf.Abs(lat):0.0} m from the line"); Time.timeScale = ts; }
                }
                Release();
                Note($"   {name} shoulder, {(east ? "eastbound" : "westbound")} from x {x0:F0}: reached x {car.Body.position.x:F0}, up to {maxSpeed * 2.237f:0} mph within 120 m of the spots, lowest {minOver:F1} m over the road line there, fell {fell}, resets {race.Racers[0].Recoveries - resets0}");
                yield return new WaitForSeconds(1);
            }
        }
        static float Ground1300r7(Vector3 p) { Hole1300r7(p.x, p.z, out float y); return y; }
    }
}
