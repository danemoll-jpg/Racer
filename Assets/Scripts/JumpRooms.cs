using System;
using System.Collections.Generic;
using UnityEngine;

namespace Racer
{
    // 1.3.0-review8 Part B (Dan, 2026-10-30: "make sure that the bumpers allow plenty of room at the edge of a ramp so that it doesn't spin you out"):
    // every jump on a course line, found from the ground itself (so the scenes' ramps, the mountain flights, the Stunt Races' kickers beside the line
    // and any crest that throws you are all found the same way), and the room each one needs: from just before its lip, through its whole flight at
    // the speed the course allows there, to the end of its landing run-out, straight on from the lip, wide enough for a vehicle that takes off 3 m
    // either side of where it was found. HighBumpers leaves no bumper in a room.
    // - The ground is read across the road in lanes (every 1.5 m, within the paved width), each lane's ground profile on its own.
    // - The speed: what the course allows there: the corner speed (sqrt(Lateral * radius)) capped at the fastest vehicle's top speed, then limited by
    //   accelerating out of the corner before (Accel) and braking for the one after (Brake): an upper bound, so the room is never too short.
    // - A lip: a station where the ground within 4 m falls at least LipDrop below the line of the ground just before it (a ramp's or a kicker's edge,
    //   a drop; not a gentle crest). Flown from there (no drag): a jump when the vehicle
    //   would be at least MinClear above the ground and in the air for MinAir metres or more.
    public static class JumpRooms
    {
        public const float LipDrop = .25f, Top = 61, Lateral = 20, Accel = 14, Brake = 25, MinClear = 1f, MinAir = 10, Before = 6, RunOut = 25, Half = 6, Spread = .07f, LaneStep = 1.5f;
        public static bool Off = Array.IndexOf(Environment.GetCommandLineArgs(), "-r8RoomsOff") >= 0; // checks only: the bumpers as before this round, for the before
        public sealed class Room
        {
            public Vector3 lip, a, b, forward; public float half = Half, air, speed, clear, s; public int line; public float lane;
            public float yLip, yLand;
            public float length; public bool loop; // the line's: the room also clears the line itself from just before the lip to the end of the run-out
            public bool OnLine(int onLine, float at)
            {
                if (onLine != line) return false; float from = s - Before, to = s + air + RunOut;
                if (!loop) return at >= from && at <= to; float d = Mathf.Repeat(at - from, length); return d <= to - from;
            }
            // a point is in the room: within half of the straight path from a to b, and not on a road far above or below it
            public Vector3[] path; Bounds box; bool boxed; // the road on from the lip to the end of the run-out (every 2 m): a bumper of any line beside it is in the room too
            public bool Contains(Vector3 w)
            {
                if (!boxed)
                {
                    boxed = true; box = new Bounds(a, Vector3.zero); box.Encapsulate(b); if (path != null) foreach (var q in path) box.Encapsulate(q);
                    float m = half + Spread * (Vector3.Distance(a, b) + 10); box.Expand(new Vector3(2 * m, 40, 2 * m));
                }
                if (!box.Contains(w)) return false;
                if (path != null) foreach (var q in path) { float dx = q.x - w.x, dz = q.z - w.z; if (dx * dx + dz * dz <= half * half && Mathf.Abs(q.y - w.y) < 6) return true; }
                Vector2 A = new(a.x, a.z), B = new(b.x, b.z), P = new(w.x, w.z); var ab = B - A; float t = Mathf.Clamp01(Vector2.Dot(P - A, ab) / Mathf.Max(ab.sqrMagnitude, 1e-4f));
                float h = half + Spread * t * ab.magnitude; // wider the further from the lip: a take-off a few degrees off straight
                if ((A + ab * t - P).sqrMagnitude > h * h) return false;
                float y = Mathf.Lerp(yLip, yLand, t); return w.y > Mathf.Min(y, yLand, yLip) - 10 && w.y < Mathf.Max(y, yLip) + 10;
            }
        }
        public static List<Room> Find(PhysicsScene physics, int mask, Func<float, (Vector3 p, Vector3 f)> at, Func<float, float> half, float length, bool loop, int line, Func<Collider, bool> skip)
        {
            var rooms = new List<Room>(); if (length < 20) return rooms;
            int n = Mathf.FloorToInt(length) + 1; var p = new Vector3[n]; var f = new Vector3[n]; var hw = new float[n];
            for (int i = 0; i < n; i++) { var (q, fw) = at(Mathf.Min(i, length - .01f)); fw.y = 0; p[i] = q; f[i] = fw.sqrMagnitude > 1e-6f ? fw.normalized : (i > 0 ? f[i - 1] : Vector3.forward); hw[i] = half(Mathf.Min(i, length - .01f)); }
            int Wrap(int i) => loop ? (i % n + n) % n : i;
            bool In(int i) => loop || (i >= 0 && i < n);
            // the speed the course allows
            var v = new float[n];
            for (int i = 0; i < n; i++)
            {
                int i0 = Wrap(i - 5), i1 = Wrap(i + 5); if (!In(i - 5)) i0 = 0; if (!In(i + 5)) i1 = n - 1;
                float turn = Vector3.Angle(f[i0], f[i1]) * Mathf.Deg2Rad; float r = turn < 1e-3f ? 1e6f : 10f / turn;
                v[i] = Mathf.Min(Top, Mathf.Sqrt(Lateral * r));
            }
            for (int pass = 0; pass < (loop ? 2 : 1); pass++) for (int i = 1; i < n + (loop ? n : 0); i++) { int k = Wrap(i), j = Wrap(i - 1); if (!In(i)) break; v[k] = Mathf.Min(v[k], Mathf.Sqrt(v[j] * v[j] + 2 * Accel)); }
            for (int pass = 0; pass < (loop ? 2 : 1); pass++) for (int i = n - 2 + (loop ? n : 0); i >= 0; i--) { int k = Wrap(i), j = Wrap(i + 1); v[k] = Mathf.Min(v[k], Mathf.Sqrt(v[j] * v[j] + 2 * Brake)); }
            var hits = new RaycastHit[12];
            float Ground(Vector3 q)
            {
                int c = physics.Raycast(q + Vector3.up * 5, Vector3.down, hits, 125, mask, QueryTriggerInteraction.Ignore); float best = float.NegativeInfinity;
                for (int k = 0; k < c; k++) { var h = hits[k]; if (h.collider.attachedRigidbody || h.normal.y < .5f || h.collider.gameObject.layer == HighBumpers.Layer || (skip != null && skip(h.collider))) continue; if (h.point.y > best) best = h.point.y; }
                return best;
            }
            float widest = 0; for (int i = 0; i < n; i++) widest = Mathf.Max(widest, hw[i]);
            // the road under the line's middle (ground within 1.6 m of the line): where it is, a lane with no ground beside it is past the road's edge, not a jump
            var road = new bool[n]; for (int i = 0; i < n; i++) { float gc = Ground(p[i]); road[i] = !float.IsNegativeInfinity(gc) && Mathf.Abs(gc - p[i].y) < 1.6f; }
            var g = new float[n];
            float edge = Mathf.Max(0, widest - .5f), first = -Mathf.Floor(edge / LaneStep) * LaneStep;
            for (float x = first; x <= edge + .01f; x += LaneStep)
            {
                for (int i = 0; i < n; i++) g[i] = Mathf.Abs(x) <= Mathf.Max(0, hw[i] - .5f) + .01f ? Ground(p[i] + Vector3.Cross(Vector3.up, f[i]) * x) : float.NegativeInfinity;
                for (int i = 0; i < n; i++)
                {
                    if (!In(i - 2) || float.IsNegativeInfinity(g[i]) || float.IsNegativeInfinity(g[Wrap(i - 2)]) || Mathf.Abs(g[i] - p[i].y) > 4) continue; // the lip is on the road
                    float m = (g[i] - g[Wrap(i - 2)]) * .5f; if (m < -.6f || m > 1.2f) continue;
                    // a crest: the ground two metres on is under the line of the ground before it
                    float g4 = In(i + 4) ? g[Wrap(i + 4)] : float.NegativeInfinity; if (!(g4 < g[i] + 4 * m - LipDrop)) continue;
                    float vx = v[i] / Mathf.Sqrt(1 + m * m); if (vx < 3) continue;
                    float clear = 0; int land = -1; bool offEdge = false;
                    for (int d = 1; d < 320; d++)
                    {
                        if (!In(i + d)) break; float y = g[i] + m * d - 4.905f * d * d / (vx * vx); int k = Wrap(i + d); float gd = g[k];
                        if (road[k] && (float.IsNegativeInfinity(gd) || gd < p[k].y - 3)) { offEdge = true; break; } // the road goes on beside it: the lane is off its edge
                        if (!float.IsNegativeInfinity(gd) && y <= gd) { land = d; break; }
                        clear = Mathf.Max(clear, float.IsNegativeInfinity(gd) ? 99 : y - gd);
                    }
                    if (offEdge || land < MinAir || clear < MinClear) continue;
                    var lip = p[i] + Vector3.Cross(Vector3.up, f[i]) * x; lip.y = g[i];
                    var b = lip + f[i] * (land + RunOut); b.y = g[Wrap(i + land)];
                    rooms.Add(new Room { lip = lip, a = lip - f[i] * Before, b = b, forward = f[i], air = land, speed = v[i], clear = clear, s = i, line = line, lane = x, yLip = g[i], yLand = b.y, half = Mathf.Max(Half, Mathf.Abs(x) + hw[i] + 2.5f), length = length, loop = loop, path = Path(at, i - Before, i + land + RunOut, length, loop) });
                    i += 3;
                }
            }
            return rooms;
        }
        // the course's own authored flights (MountainFlights: the mountain courses' and Leviathan's big jumps): lip to landing, as authored
        public static List<Room> Flights(RaceDirector race)
        {
            var list = new List<Room>(); var mf = race ? race.GetComponent<MountainFlights>() : null; if (!mf || !race.road) return list;
            foreach (var fl in mf.flights)
            {
                var f = fl.landingEnd - fl.lip; f.y = 0; if (f.sqrMagnitude < 1) continue; float air = f.magnitude; f /= air;
                float s = race.road.Project(fl.lip, out float lat);
                list.Add(new Room { lip = fl.lip, a = fl.lip - f * Before, b = fl.landingEnd + f * RunOut, forward = f, air = air, speed = Mathf.Max(fl.aiTakeoffSpeed, 20), clear = 99, s = s, line = 0, lane = lat, yLip = fl.lip.y, yLand = fl.landingEnd.y, half = Mathf.Max(Half, Mathf.Abs(lat) + race.road.HalfWidth(s) + 2.5f), length = race.road.Length, loop = !race.road.openHighway,
                    path = Path(t => { var q = race.road.At(t, out var fw); return (q, fw); }, s - Before, s + air + RunOut, race.road.Length, !race.road.openHighway) });
            }
            return list;
        }
        static Vector3[] Path(Func<float, (Vector3 p, Vector3 f)> at, float from, float to, float length, bool loop)
        {
            var list = new List<Vector3>();
            for (float t = from; t <= to; t += 2)
            {
                if (loop) { list.Add(at(Mathf.Repeat(t, length)).p); continue; }
                // past a shortcut's end (where it rejoins the course): straight on from its end
                float u = Mathf.Clamp(t, 0, length - .01f); var (q, f) = at(u); f.y = 0; list.Add(t > u && f.sqrMagnitude > 1e-4f ? q + f.normalized * (t - u) : q);
            }
            return list.ToArray();
        }
        // the same jump found in several lanes or stations: one entry each (for the checks and the report)
        public static List<Room> Distinct(List<Room> rooms)
        {
            var list = new List<Room>();
            foreach (var r in rooms)
            {
                var same = list.Find(o => o.line == r.line && Mathf.Abs(o.s - r.s) < 30);
                if (same == null) list.Add(r); else if (r.air > same.air) { list.Remove(same); list.Add(r); }
            }
            return list;
        }
    }
}
