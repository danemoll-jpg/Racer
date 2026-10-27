using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Racer
{
    // 1.3.0-review3 evidence only (Dan: "both jumps 4 and 8 have entrances that stop momentum. Please fix them and review the rest of the new ramps"):
    //   -p106kind world1300 -p1300part entrances [-p1300ids 4,8] [-p1300quick 1]
    //       every Free Roam ramp (and the flip ramps and the turnaround): the ground across the toe and along the last 15 m of the run-up (steps, anything
    //       standing on it), a driver's-view shot of the entrance, then the pad drives at it at 10, 18 and 26 m/s on the centre line and 3 m to each side,
    //       and at 18 m/s 15 degrees off-line: speed 5 m before the toe and on the ramp's face, the loss beyond the climb, any hit on a vertical face, wrecks
    //       and resets. -p1300quick 1: only the centre line at 18 m/s (the Clay Hopper's one run).
    //   -p1300part jumps3 [-p1300ids ...]   each house, gap and table jump (and the kickers and the barn) at its sensible speed and 20% slower: where it lands,
    //       upright or not, what it touched in the air; and whether a vehicle that comes down short is trapped (it drives on for 5 s and must get away)
    public sealed partial class Report106PlayerCheck
    {
        // a hit on a vertical face (contact normal near level) of anything but the vehicle's own wheels' ground
        sealed class Faces1300r3 : MonoBehaviour
        {
            public readonly List<string> list = new(); public Vector3 toe, dir;
            void OnCollisionEnter(Collision c) => Look(c);
            void OnCollisionStay(Collision c) => Look(c);
            void Look(Collision c)
            {
                for (int i = 0; i < c.contactCount; i++)
                {
                    var p = c.GetContact(i); if (Mathf.Abs(p.normal.y) > .5f || c.impulse.magnitude < 50) continue;
                    var d = p.point - toe; d.y = 0; var right = Vector3.Cross(Vector3.up, dir); Tops1300r3(p.point, out _, out float g, out _);
                    var s = $"'{c.collider.name}' (normal {p.normal.x:F1},{p.normal.y:F1},{p.normal.z:F1}, impulse {c.impulse.magnitude:F0}, at {Vector3.Dot(d, dir):+0.0;-0.0} m along from the toe, {Vector3.Dot(d, right):+0.0;-0.0} m right, {p.point.y - g:+0.00;-0.00} m over the ground)"; if (!list.Contains(s)) list.Add(s); break;
                }
            }
        }
        static float Lip1300r3(RoamWorld.Item i) => i.height / (1 - Mathf.Cos(i.angle * Mathf.Deg2Rad)) * Mathf.Sin(i.angle * Mathf.Deg2Rad);
        // the ramp's own top and the ground's top (everything else static) under a point
        static void Tops1300r3(Vector3 p, out float ramp, out float ground, out string what)
        {
            ramp = float.NaN; ground = float.NaN; what = "";
            foreach (var h in Physics.RaycastAll(new Vector3(p.x, 600, p.z), Vector3.down, 1200, ~0, QueryTriggerInteraction.Ignore))
            {
                var c = h.collider; if (c.attachedRigidbody || c.isTrigger) continue;
                if (c.name.Contains("Free Roam")) { if (float.IsNaN(ramp) || h.point.y > ramp) ramp = h.point.y; }
                else if (float.IsNaN(ground) || h.point.y > ground) { ground = h.point.y; what = c.name; }
            }
        }
        IEnumerable<RoamWorld.Item> Ramps1300r3()
        {
            var rw = RoamWorld.Current; var ids = Arg("-p1300ids", "").Split(',').Where(s => s.Length > 0).ToArray();
            return rw.Items.Where(i => i.kind is "kicker" or "table" or "gap" or "house" or "through" or "flip" or "loop").Where(i => ids.Length == 0 || ids.Contains(i.id));
        }
        // where the entrance is: the toe (the ramp's first edge on the ground), the way in, the half width, the face point (on the ramp, 35% of the way up)
        (Vector3 toe, Vector3 dir, float half, float face) Entrance1300r3(RoamWorld.Item i)
        {
            var dir = Quaternion.Euler(0, i.yaw, 0) * Vector3.forward; var at = new Vector3(i.pos[0], 0, i.pos[1]);
            if (i.kind == "loop") return (at - dir * i.radius, dir, i.radius * .6f, 4);
            float lip = Lip1300r3(i); return (at - dir * lip, dir, i.width * .5f, lip * .35f);
        }

        IEnumerator Entrances1300r3()
        {
            var car = race.vehicle; var cfg = car.GetComponent<VehicleConfiguration>(); var resp = car.GetComponent<VehicleRespawn>(); bool quick = Arg("-p1300quick", "0") == "1";
            var faces = car.GetComponent<Faces1300r3>() ?? car.gameObject.AddComponent<Faces1300r3>(); float g = -Physics.gravity.y;
            Note($"== entrances, vehicle {cfg.profileId}{(quick ? " (centre line at 18 m/s only)" : "")}");
            foreach (var i in Ramps1300r3())
            {
                var (toe, dir, half, face) = Entrance1300r3(i); var right = Vector3.Cross(Vector3.up, dir);
                // 1. the ground: across the toe (ramp edge minus ground, every metre) and along the last 15 m of the run-up on five lines
                if (!quick)
                {
                    var across = new List<string>(); float worst = 0;
                    for (float x = -half + .3f; x <= half - .3f + .01f; x += Mathf.Max(1, (2 * half - .6f) / 10))
                    {
                        Tops1300r3(toe + dir * .1f + right * x, out float rampY, out float groundY, out _);
                        float step = float.IsNaN(rampY) ? float.NaN : rampY - groundY; if (!float.IsNaN(step)) worst = Mathf.Max(worst, step);
                        across.Add(float.IsNaN(step) ? "-" : step.ToString("+0.00;-0.00"));
                    }
                    Note($"  {i.id} {i.kind} toe {V(toe)}: ramp edge minus ground across the toe (left to right): {string.Join(" ", across)}; highest step {worst:F2} m");
                    var lines = new List<string>();
                    foreach (float x in new[] { -half + .5f, -half * .5f, 0, half * .5f, half - .5f })
                    {
                        float prev = float.NaN, maxStep = 0, sMax = 0; var ys = new List<float>(); string at = "";
                        for (float s = -15; s <= face; s += .25f)
                        {
                            var p = toe + dir * s + right * x; Tops1300r3(p, out float rampY, out float groundY, out string what);
                            float y = float.IsNaN(rampY) ? groundY : Mathf.Max(rampY, groundY); if (s <= 0) ys.Add(y);
                            if (!float.IsNaN(prev) && y - prev > maxStep) { maxStep = y - prev; sMax = s; at = !float.IsNaN(rampY) && rampY >= groundY ? "ramp" : what; }
                            prev = y;
                        }
                        // evenness: the most the run-up's last 15 m rise above / dip below the straight line from its start to the toe
                        float hump = 0, dip = 0; for (int k = 0; k < ys.Count; k++) { float line = Mathf.Lerp(ys[0], ys[^1], k / (float)(ys.Count - 1)); hump = Mathf.Max(hump, ys[k] - line); dip = Mathf.Max(dip, line - ys[k]); }
                        lines.Add($"{x:+0.0;-0.0;0}: rise {ys[^1] - ys[0]:+0.00;-0.00} m, biggest 0.25 m step {maxStep:F2} at {sMax:+0.0;-0.0} ({at}), hump {hump:F2} dip {dip:F2}");
                    }
                    Note($"    run-up, last 15 m to the face point (metres right of the centre line): {string.Join(" | ", lines)}");
                    var things = new HashSet<string>();
                    for (float s = -20; s <= face; s += 2)
                    {
                        var p = toe + dir * s; Tops1300r3(p, out float rampY, out float groundY, out _); float y = float.IsNaN(rampY) ? groundY : Mathf.Max(rampY, groundY);
                        foreach (var c in Physics.OverlapBox(new Vector3(p.x, y + 1.4f, p.z), new Vector3(half + 1, 1.1f, 1), Quaternion.LookRotation(dir), ~0, QueryTriggerInteraction.Ignore))
                            if (!c.attachedRigidbody && !c.name.StartsWith("Ground_") && !(c is TerrainCollider) && !c.name.StartsWith("World edge")) things.Add($"'{c.name}' at {s:+0;-0} m");
                    }
                    Note($"    standing on the run-up and entrance: {(things.Count == 0 ? "nothing" : string.Join("; ", things.Take(12)))}");
                    // the ramp's own collider: any triangle facing back down the run-up (near vertical) that stands above the ground
                    var exposed = new List<string>(); int facing = 0;
                    foreach (var mc in FindObjectsByType<MeshCollider>(FindObjectsSortMode.None).Where(m => m.name.Contains("Free Roam") && (m.name.Contains($"ramp {i.id} (") || m.name.Contains($"flip ramp {i.id} ") || m.name.Contains($"turnaround {i.id} "))))
                    {
                        var mesh = mc.sharedMesh; var v = mesh.vertices; var t = mesh.triangles; var tf = mc.transform;
                        for (int k = 0; k + 2 < t.Length; k += 3)
                        {
                            Vector3 a = tf.TransformPoint(v[t[k]]), b = tf.TransformPoint(v[t[k + 1]]), c = tf.TransformPoint(v[t[k + 2]]); var n = Vector3.Cross(b - a, c - a).normalized;
                            if (Mathf.Abs(n.y) > .5f || Vector3.Dot(n, -dir) < .7f) continue; facing++;
                            foreach (var q in new[] { a, b, c, (a + b + c) / 3 }) { Tops1300r3(q, out _, out float gq, out _); if (q.y - gq > .03f) { var d = q - toe; d.y = 0; exposed.Add($"{Vector3.Dot(d, dir):+0.0;-0.0} along, {Vector3.Dot(d, right):+0.0;-0.0} right: {q.y - gq:F2} m up"); } }
                        }
                    }
                    Note($"    ramp faces turned toward the way in: {facing} triangles; above the ground: {(exposed.Count == 0 ? "none" : string.Join("; ", exposed.Distinct().Take(8)))}");
                    // the driver's view of the entrance
                    var eye = toe - dir * 22; eye.y = Top1300(eye) + 1.6f; var look = toe + dir * face; look.y = Top1300(look) + .5f;
                    car.Body.isKinematic = true; car.Body.position = eye + Vector3.down * 3; car.transform.position = car.Body.position; Physics.SyncTransforms(); HideUi1300(true);
                    yield return Live1300($"entrance-{i.id.PadLeft(2, '0')}-{i.kind}", eye, look, 1280, 720); HideUi1300(false); car.Body.isKinematic = false;
                }
                // 2. driven at by the pad
                var runs = new List<(float speed, float off, float angle)>();
                if (quick) runs.Add((18, 0, 0)); else { foreach (float sp in new[] { 10f, 18f, 26f }) foreach (float off in new[] { 0f, -3f, 3f }) runs.Add((sp, off, 0)); runs.Add((18, 0, 15)); }
                var summary = new List<string>(); faces.toe = toe; faces.dir = dir;
                foreach (var (speed, off, angle) in runs)
                {
                    High1300(false); BindPad(); var d = Quaternion.Euler(0, angle, 0) * dir; var aim = toe + right * off; var start = aim - d * (angle != 0 ? 25 : 45);
                    Put1300(car, start, Yaw(d), speed); FindAnyObjectByType<ChaseCamera>()?.Snap(); faces.list.Clear(); int w0 = cfg.Wipeouts, r0 = resp ? resp.Resets : 0, launches = FlipRamp.Launches;
                    float t0 = Time.time, v5 = float.NaN, y5 = 0, vFace = float.NaN, yFace = 0, vMin = float.MaxValue, vLast = 0, yLast = 0; var prev = car.Body.position; bool jumped = false;
                    while (Time.time - t0 < 9)
                    {
                        Drive(car, aim + d * 80 + Vector3.up * car.Body.position.y, speed); yield return new WaitForFixedUpdate();
                        var p = car.Body.position; if (Vector3.Distance(p, prev) > 6) jumped = true; prev = p;
                        float along = Vector3.Dot(Flat(p - aim), d), v = car.Body.linearVelocity.magnitude;
                        if (float.IsNaN(v5) && along >= -5) { v5 = v; y5 = p.y; }
                        if (!float.IsNaN(v5) && float.IsNaN(vFace) && FlipRamp.Launches == launches) vMin = Mathf.Min(vMin, v);
                        // a flip ramp's launch replaces the velocity: its face speed is the one just before the launch
                        if (float.IsNaN(vFace) && FlipRamp.Launches > launches) { vFace = vLast; yFace = yLast; break; }
                        if (float.IsNaN(vFace) && along >= face) { vFace = v; yFace = p.y; break; }
                        vLast = v; yLast = p.y;
                    }
                    Release();
                    float expect = float.IsNaN(v5) ? float.NaN : Mathf.Sqrt(Mathf.Max(0, v5 * v5 - 2 * g * (yFace - y5)));
                    float loss = float.IsNaN(vFace) || expect <= 0 ? 1 : Mathf.Max(0, 1 - vFace / expect);
                    bool wreck = cfg.Wipeouts != w0, reset = (resp && resp.Resets != r0) || jumped, pass = !float.IsNaN(vFace) && loss <= .10f && faces.list.Count == 0 && !wreck && !reset;
                    string tag = $"{speed:F0} m/s {(angle != 0 ? $"{angle:F0} deg off-line" : off == 0 ? "centre" : $"{off:+0;-0} m")}";
                    Note($"    {tag}: 5 m before the toe {v5:F1} m/s, on the face {(float.IsNaN(vFace) ? "NEVER REACHED" : $"{vFace:F1} m/s")} (climb {yFace - y5:F2} m, {expect:F1} m/s expected from the climb alone, lowest {vMin:F1}): loss beyond the climb {loss * 100:F0}%; vertical faces hit: {(faces.list.Count == 0 ? "none" : string.Join("; ", faces.list.Take(3)))}{(wreck ? "; WRECKED" : "")}{(reset ? "; RESET" : "")} -> {(pass ? "pass" : "FAIL")}");
                    summary.Add($"{tag} {loss * 100:F0}%{(pass ? "" : " FAIL")}");
                    car.Body.linearVelocity = Vector3.zero; yield return new WaitForSeconds(.3f);
                }
                Note($"  {i.id} {i.kind} {cfg.profileId} summary: {string.Join(", ", summary)}");
            }
        }

        // -p1300part mapshots: the Free Roam map and the minimap with the item icons and numbers, the player between ramps 1, 11 and 5 (several items in view)
        IEnumerator MapShots1300r3()
        {
            var car = race.vehicle; HideUi1300(false); Put1300(car, new Vector3(-120, 0, -230), 300, 0); FindAnyObjectByType<ChaseCamera>()?.Snap(); yield return new WaitForSecondsRealtime(2);
            string tag = $"{Screen.width}x{Screen.height}"; yield return Snap("minimap-" + tag);
            var map = flow.GetComponent<ExplorationMap>(); if (map) { map.Open(); yield return new WaitForSecondsRealtime(1); yield return Snap("map-" + tag); map.Close(); }
            Note($"  map and minimap shot at {tag}");
        }
        // the speed (approach, 70 m out) that carries a jump from its lip onto the landing's middle, by the flight sums (takeoff at the ramp's angle, the
        // climb's loss added back); kickers: 18 m/s; the barn: 34 m/s, as 1.3 drove it
        float Sensible1300r3(RoamWorld.Item i, (Vector3 lip, Vector3 dir, float lipY, Vector3 land) ramp)
        {
            float g = -Physics.gravity.y; if (i.kind == "kicker") return 18; if (i.kind == "through") return 34;
            float range = i.kind == "table" ? i.gap + 8 : i.gap + 4, landY = Top1300(ramp.lip + ramp.dir * range), dy = landY - ramp.lipY, th = i.angle * Mathf.Deg2Rad;
            float v2 = g * range * range / (2 * Mathf.Cos(th) * Mathf.Cos(th) * Mathf.Max(.5f, range * Mathf.Tan(th) - dy));
            return Mathf.Sqrt(v2 + 2 * g * i.height);
        }
        // as Fly1300, the pad holding the approach speed (Fly1300 drives at its 60 m/s cap: every jump at full throttle)
        IEnumerator Fly1300r3(ArcadeVehicle car, Vector3 start, Vector3 dir, float speed, Vector3 lip, float lipY, Flight1300 r)
        {
            BindPad(); Put1300(car, start, Yaw(dir), speed); FindAnyObjectByType<ChaseCamera>()?.Snap();
            var contacts = car.GetComponent<Report106Contacts>() ?? car.gameObject.AddComponent<Report106Contacts>(); contacts.who = "vehicle"; contacts.list.Clear();
            var cfg = car.GetComponent<VehicleConfiguration>(); int wipes = cfg.Wipeouts; float t0 = Time.time, stable = 0; bool flying = false, past = false;
            while (Time.time - t0 < 14)
            {
                Drive(car, lip + dir * 400 + Vector3.up * car.Body.position.y, flying || past ? 0 : speed); yield return new WaitForFixedUpdate();
                var p = car.Body.position; float along = Vector3.Dot(Flat(p - lip), dir);
                if (!past && along > -1) past = true;
                if (past && !flying && car.GroundedWheels == 0) { flying = true; r.takeoff = p; r.takeoffSpeed = car.Body.linearVelocity.magnitude; }
                if (flying) { r.apex = Mathf.Max(r.apex, p.y - lipY); if (car.GroundedWheels == 0) r.air += Time.fixedDeltaTime; else if (!r.landed) { r.landed = true; r.land = p; } }
                if (r.landed) { stable = car.GroundedWheels >= 2 ? stable + Time.fixedDeltaTime : 0; if (stable > 1) break; }
            }
            Release(); r.distance = Vector3.Distance(Flat(r.land), Flat(r.takeoff)); r.upAtRest = car.transform.up.y; r.wiped = cfg.Wipeouts != wipes;
            r.touches = contacts.list.Where(x => !x.Contains("Free Roam") && !x.Contains("Ground_")).Distinct().Take(6).ToList();
        }
        IEnumerator Jumps1300r3()
        {
            var rw = RoamWorld.Current; var car = race.vehicle; var cfg = car.GetComponent<VehicleConfiguration>();
            var faces = car.GetComponent<Faces1300r3>() ?? car.gameObject.AddComponent<Faces1300r3>();
            Note($"== jumps at the sensible speed and 20% slower, vehicle {cfg.profileId}");
            foreach (var i in Ramps1300r3().Where(x => x.kind is not ("flip" or "loop")))
            {
                var ramp = rw.Ramps[i.id]; float sensible = Sensible1300r3(i, ramp);
                // the speed that clears it, found by trying: from the flight sums' speed, 1.5 m/s more while it comes down short of the landing (the
                // table's slope, past the barn), 1.5 m/s less while it flies into the barn's wall above the opening
                if (i.kind != "kicker")
                {
                    var tries = new List<string>();
                    for (int k = 0; k < 10; k++)
                    {
                        var t = new Flight1300(); faces.list.Clear(); High1300(false);
                        yield return Fly1300r3(car, ramp.lip - ramp.dir * 70, ramp.dir, sensible, ramp.lip, ramp.lipY, t);
                        float at = Vector3.Dot(Flat(t.land - ramp.lip), ramp.dir);
                        bool barnHit = faces.list.Any(x => x.Contains("barn")), roof = t.touches.Any(x => x.Contains("roof") || x.Contains("Pitched"));
                        bool high = i.kind == "through" && barnHit && t.apex > 3, ok = t.landed && !roof && (i.kind == "through" ? !barnHit && at > i.gap + RoamWorld.BarnLength : at >= i.gap + 1);
                        tries.Add($"{sensible:F1} m/s: {(t.landed ? at.ToString("F0") + " m" : "no landing")}{(barnHit ? ", hit the barn" : "")}{(roof ? ", hit the roof" : "")}");
                        if (ok) break; sensible += high ? -1.5f : 1.5f;
                    }
                    Note($"  {i.id} {i.kind}: the clearing speed by trying: {string.Join("; ", tries)}");
                }
                foreach (float k in i.kind is "gap" or "house" or "table" ? new[] { 1f, .8f } : new[] { 1f })
                {
                    High1300(false); var r = new Flight1300(); float speed = sensible * k; faces.list.Clear();
                    yield return Fly1300r3(car, ramp.lip - ramp.dir * 70, ramp.dir, speed, ramp.lip, ramp.lipY, r);
                    float landAt = Vector3.Dot(Flat(r.land - ramp.lip), ramp.dir), landFrom = i.kind is "gap" or "house" ? i.gap : i.kind == "table" ? i.gap : 0;
                    Tops1300r3(car.Body.position, out float rampY, out float groundY, out string under); bool onItem = !float.IsNaN(rampY) && rampY > groundY;
                    // trapped? drive on for 5 s (and back for 3): it must get 6 m away from where it came to rest
                    var rest = car.Body.position; BindPad(); float t0 = Time.time;
                    while (Time.time - t0 < 5) { Drive(car, rest + ramp.dir * 60 + Vector3.up * rest.y, 10); yield return new WaitForFixedUpdate(); }
                    if (Vector3.Distance(car.Body.position, rest) < 6) { t0 = Time.time; while (Time.time - t0 < 3) { InputSystem.QueueStateEvent(pad, new UnityEngine.InputSystem.LowLevel.GamepadState { leftTrigger = 1 }); yield return new WaitForFixedUpdate(); } }
                    Release(); float moved = Vector3.Distance(car.Body.position, rest);
                    string where = !r.landed ? "no landing seen" : i.kind is "gap" or "house" ? (landAt < landFrom ? $"SHORT, came down {landAt:F0} m from the lip (the landing starts at {landFrom:F0} m)" : landAt <= landFrom + 4 + 30 ? $"on the landing ({landAt:F0} m from the lip)" : $"past the landing ({landAt:F0} m from the lip)")
                        : i.kind == "table" ? (landAt < landFrom ? $"on the table ({landAt:F0} m from the lip, the slope starts at {landFrom:F0} m)" : $"on the landing slope or past it ({landAt:F0} m from the lip)") : $"{landAt:F0} m from the lip";
                    Note($"  {i.id} {i.kind} at {speed:F1} m/s ({(k == 1 ? "clears it" : "20% slower")}): takeoff {r.takeoffSpeed:F1} m/s, apex {r.apex:F1} m over the lip, {where}, {(r.wiped ? "WIPED OUT" : "upright")} (up {r.upAtRest:F2}); touched in the air: {(r.touches.Count == 0 ? "nothing but ground" : string.Join("; ", r.touches))}{(faces.list.Count > 0 ? "; faces: " + string.Join("; ", faces.list.Take(2)) : "")}; came to rest on {(onItem ? "the ramp" : under)}, then drove {moved:F0} m away -> {(moved >= 6 ? "not trapped" : "TRAPPED")}");
                    if (k == 1 && i.kind is "house" or "gap") yield return Snap($"jump3-{i.id.PadLeft(2, '0')}-rest");
                }
            }
        }
    }
}
