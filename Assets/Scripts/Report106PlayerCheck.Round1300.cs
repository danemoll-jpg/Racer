using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Racer
{
    // 1.3 Free Roam world evidence only (the 0.106 check component: -p106check <dir> -racerTestSave <dir>; never active without both):
    //   -p106kind world1300 -p1300part survey   Free Roam: a 4 m grid of the world (top surface, what it is, anything standing above the ground), the roads,
    //                                           houses, activity sites, the breakable trees, and a top-down picture of the whole world (1 m per pixel)
    //   -p1300part shots                        one in-game picture of every item (RoamWorld), the top-down picture with the items built
    //   -p1300part jumps -p1300ids 1,4          each ramp jumped by the pad (-p106vehicle, -p1300speed m/s at 70 m before the lip), normal mode then HIGH (-p1300high 0|1|2: off, on, both)
    //   -p1300part flips                        every flip ramp driven into by the pad, normal mode then HIGH: launched, how high, which way it landed, upright
    //   -p1300part trees                        a thin trunk at 12 m/s, a big trunk at 12 m/s (stands) and at 30 m/s (breaks), HIGH MODE's smaller speed cost;
    //                                           then far away: they stand again
    //   -p1300part smash                        a smash spot driven through; then far away: it is put back
    //   -p1300part perf [-p1300off]             two-player Free Roam in the forest: frame time at three views, and the same with a dozen trees broken
    //   Items can be read from -p1300data <file> (placing them) instead of the built-in Resources/RoamWorld.json; -p1300off: the world before this round.
    public sealed partial class Report106PlayerCheck
    {
        IEnumerator World1300()
        {
            string part = Arg("-p1300part", "survey");
            if (part == "campfence") { yield return CampFence1300(); yield break; } // 1.3.0-review2: the campaign's Fence Line Smash, driven
            if (part == "landings") { yield return Landings1300(int.Parse(Arg("-p1300course", "4"))); yield break; } // 1.3.0-review2: a HIGH MODE race, not Free Roam
            yield return ToFreeRoam(Arg("-p106vehicle", "original")); Campaign.Testing = true; Conditions(); HighMode.Declined = true; // the checks' resets never bring up the HIGH MODE question
            var rw = RoamWorld.Current; var trees = SceneryWorld.Current ? SceneryWorld.Current.Trees : null;
            Note($"Free Roam world check part {part}: items {(rw ? rw.Items.Length : 0)} ({(RoamWorld.Disabled ? "off: before" : RoamWorld.Override != "" ? "from " + RoamWorld.Override : "built-in")}), breakable trees {(trees ? trees.BreakableCount : 0)}");
            if (rw) foreach (var line in rw.Report) Note("  " + line);
            foreach (var line in RoamWorld.Cleared) Note("  cleared: " + line);
            if (part == "survey") yield return Survey1300();
            if (part == "shots") yield return Shots1300();
            if (part == "jumps") yield return Jumps1300();
            if (part == "flips") yield return Flips1300();
            if (part == "trees") yield return Trees1300();
            if (part == "smash") yield return Smash1300();
            if (part == "perf") yield return Perf1300();
            if (part == "loops") yield return Loops1300();
            if (part == "fence") yield return Fence1300();
            if (part == "entrances") yield return Entrances1300r3(); // 1.3.0-review3 (Report106PlayerCheck.Round1300r3.cs)
            if (part == "jumps3") yield return Jumps1300r3();
            if (part == "mapshots") yield return MapShots1300r3();
        }
        // the top static surface under a point, anywhere from 600 m down (TopAt only looks 30 m up and 50 m down from the point it is given)
        static float Top1300(Vector3 p) { float best = float.NaN; foreach (var h in Physics.RaycastAll(new Vector3(p.x, 600, p.z), Vector3.down, 1200, ~0, QueryTriggerInteraction.Ignore)) { if (h.collider.attachedRigidbody || h.collider.isTrigger) continue; if (float.IsNaN(best) || h.point.y > best) best = h.point.y; } return float.IsNaN(best) ? p.y : best; }
        void Put1300(ArcadeVehicle car, Vector3 pos, float yaw, float speed) { pos.y = Top1300(pos) + 1; Put(car, pos + Vector3.up * 40, yaw, speed); var rot = Quaternion.Euler(0, yaw, 0); car.Body.position = pos; car.transform.SetPositionAndRotation(pos, rot); Physics.SyncTransforms(); }
        static void HideUi1300(bool hide) { foreach (var c in FindObjectsByType<Canvas>(FindObjectsSortMode.None)) c.enabled = !hide; }
        void High1300(bool on) { HighMode.Set(on); race.high = on; HighAssist.Attach(race); if (flow.State != RaceFlow.Stage.Racing) flow.Resume(); } // the test save only

        // ---------------------------------------------------------------- the survey
        static readonly RaycastHit[] hits1300 = new RaycastHit[48]; static readonly Collider[] overlap1300 = new Collider[32];
        IEnumerator Survey1300()
        {
            var car = race.vehicle; car.Body.isKinematic = true; car.Body.position = new Vector3(0, 900, 0); Physics.SyncTransforms();
            var bb = Arg("-p1300bounds", "-1400,1700,-900,1000").Split(',').Select(F).ToArray(); float x0 = bb[0], x1 = bb[1], z0 = bb[2], z1 = bb[3]; const float step = 4;
            var sb = new StringBuilder(); sb.AppendLine($"grid {x0} {z0} {step} {(int)((x1 - x0) / step)} {(int)((z1 - z0) / step)}  (per cell: class top-y ground-y; G ground, R drivable, T trunk, B building/other, E world edge, N nothing; then O when something stands 0.6-4.6 m above the ground)");
            int rows = 0;
            for (float z = z0; z < z1; z += step)
            {
                var line = new StringBuilder();
                for (float x = x0; x < x1; x += step)
                {
                    int n = Physics.RaycastNonAlloc(new Vector3(x, 700, z), Vector3.down, hits1300, 1400, ~0, QueryTriggerInteraction.Ignore);
                    float top = float.MinValue, ground = float.MinValue; Collider topC = null; RaycastHit topH = default;
                    for (int i = 0; i < n; i++)
                    {
                        var h = hits1300[i]; var c = h.collider; if (c.attachedRigidbody && !c.attachedRigidbody.isKinematic) continue;
                        if (h.point.y > top) { top = h.point.y; topC = c; topH = h; }
                        if (c.name.StartsWith("Ground_") && h.point.y > ground) ground = h.point.y;
                    }
                    char k = 'N';
                    if (topC)
                    {
                        string nm = topC.name;
                        if (nm.StartsWith("World edge")) k = 'E';
                        else if (nm.IndexOf("trunk", System.StringComparison.OrdinalIgnoreCase) >= 0) k = 'T';
                        else if (SceneryTrees.OnDrivable(topH.point, null, out _)) k = 'R';
                        else if (nm.StartsWith("Ground_")) k = 'G';
                        else k = Mathf.Abs(top - ground) < .6f ? 'G' : 'B';
                    }
                    bool o = false; float gy = ground > float.MinValue ? ground : top;
                    if (topC && k != 'E')
                    {
                        int m = Physics.OverlapBoxNonAlloc(new Vector3(x, gy + 2.6f, z), new Vector3(step * .5f, 2f, step * .5f), overlap1300, Quaternion.identity, ~0, QueryTriggerInteraction.Ignore);
                        for (int i = 0; i < m; i++) { var c = overlap1300[i]; if (c.name.StartsWith("Ground_") || c.name.StartsWith("World edge") || (c.attachedRigidbody && !c.attachedRigidbody.isKinematic)) continue; o = true; break; }
                    }
                    line.Append(k).Append(o ? "O" : "").Append(' ').Append(top > float.MinValue ? top.ToString("F1", System.Globalization.CultureInfo.InvariantCulture) : "-").Append(' ').Append(ground > float.MinValue ? ground.ToString("F1", System.Globalization.CultureInfo.InvariantCulture) : "-").Append(',');
                }
                sb.AppendLine(line.ToString()); if (++rows % 40 == 0) yield return null;
            }
            File.WriteAllText(Path.Combine(outDir, "grid.txt"), sb.ToString()); Note($"grid written: {rows} rows");
            // roads, houses, sites, trees
            string F1(float v) => v.ToString("F1", System.Globalization.CultureInfo.InvariantCulture);
            string P3(Vector3 p) => $"[{F1(p.x)},{F1(p.y)},{F1(p.z)}]";
            var js = new StringBuilder("{\"roads\":[");
            bool first = true;
            foreach (var r in FindObjectsByType<RaceRoad>(FindObjectsSortMode.None))
            {
                if (r.points == null || r.points.Length < 2) continue; r.Initialize(); var pts = new List<string>(); for (float s = 0; s < r.Length; s += 5) pts.Add(P3(r.At(s, out _)));
                js.Append(first ? "" : ",").Append($"{{\"name\":\"{r.name}\",\"trail\":{(r.forestTrail ? "true" : "false")},\"open\":{(r.openHighway ? "true" : "false")},\"length\":{F1(r.Length)},\"pts\":[{string.Join(",", pts)}]}}"); first = false;
            }
            js.Append("],\"houses\":["); first = true;
            foreach (var holder in race.gameObject.scene.GetRootGameObjects().Where(g => g.name == "Remembered houses and approximate buildings" || g.name.StartsWith("Kyle")))
                foreach (Transform h in holder.transform)
                {
                    var cs = h.GetComponentsInChildren<Collider>().Where(c => !c.isTrigger).ToArray(); if (cs.Length == 0) continue; var b = cs[0].bounds; foreach (var c in cs) b.Encapsulate(c.bounds);
                    js.Append(first ? "" : ",").Append($"{{\"name\":\"{h.name.Replace("\"", "'")}\",\"min\":{P3(b.min)},\"max\":{P3(b.max)}}}"); first = false;
                }
            js.Append("],\"sites\":["); first = true;
            foreach (var s in FindObjectsByType<ActivitySite>(FindObjectsSortMode.None)) { js.Append(first ? "" : ",").Append($"{{\"id\":\"{s.id}\",\"kind\":\"{s.kind}\",\"at\":{P3(s.transform.position)},\"radius\":{F1(s.radius)}}}"); first = false; }
            js.Append("],\"trees\":["); first = true;
            if (SceneryWorld.Current && SceneryWorld.Current.Trees) foreach (var t in SceneryWorld.Current.Trees.BreakableTrees) { js.Append(first ? "" : ",").Append($"[{F1(t.bottom.x)},{F1(t.bottom.y)},{F1(t.bottom.z)},{t.width.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)}]"); first = false; }
            js.Append("],\"named\":["); first = true;
            foreach (var t in FindObjectsByType<Transform>(FindObjectsSortMode.None))
            {
                var nm = t.name; if (!(nm.IndexOf("clos", System.StringComparison.OrdinalIgnoreCase) >= 0 || nm.IndexOf("barri", System.StringComparison.OrdinalIgnoreCase) >= 0 || nm.IndexOf("NO ENTRY", System.StringComparison.OrdinalIgnoreCase) >= 0 || nm.IndexOf("gate", System.StringComparison.OrdinalIgnoreCase) >= 0)) continue;
                js.Append(first ? "" : ",").Append($"{{\"name\":\"{nm.Replace("\"", "'")}\",\"at\":{P3(t.position)}}}"); first = false;
            }
            js.Append("]}"); File.WriteAllText(Path.Combine(outDir, "world.json"), js.ToString()); Note("world.json written");
            yield return TopDown1300("world-top-down");
            car.Body.isKinematic = false;
        }
        IEnumerator TopDown1300(string name)
        {
            HideUi1300(true); WorldLook.Current?.SetClock(WorldLook.Current.Day, 12); var car = race.vehicle; bool kin = car.Body.isKinematic; car.Body.isKinematic = true;
            // tiles of 1100 x 800 m at 0.5 m per pixel over x -1400..1900, z -900..1500 (joined by the picture script), from below the clouds; the vehicle (and
            // so the main camera, which the trees' detail follows) above each tile's middle so every tree is drawn
            for (int qz = 0; qz < 3; qz++) for (int qx = 0; qx < 3; qx++)
            {
                float cx = -1400 + 550 + qx * 1100, cz = -900 + 400 + qz * 800;
                car.Body.position = new Vector3(cx, 380, cz); car.transform.position = car.Body.position; Physics.SyncTransforms(); FindAnyObjectByType<ChaseCamera>()?.Snap(); yield return null; yield return null;
                yield return Live1300($"{name}-{qx}{qz}", new Vector3(cx, 400, cz));
            }
            car.Body.isKinematic = kin; HideUi1300(false); Note($"{name}: 3 x 3 tiles, each 1100 x 800 m at 0.5 m per pixel (x -1400..1900, z -900..1500)");
        }

        // a top-down render taken as part of a normal frame (the trees are drawn per frame for the frame's cameras, not for a camera rendered by hand)
        IEnumerator Live1300(string name, Vector3 eye, Vector3? look = null, int w = 2200, int h = 1600, float size = 400)
        {
            var go = new GameObject("check camera"); var cam = go.AddComponent<Camera>(); cam.CopyFrom(Camera.main); cam.orthographic = look == null; cam.orthographicSize = size; cam.fieldOfView = 62; cam.nearClipPlane = .3f; cam.farClipPlane = 2000;
            go.transform.SetPositionAndRotation(eye, look is Vector3 l ? Quaternion.LookRotation(l - eye) : Quaternion.Euler(90, 0, 0)); var rt = new RenderTexture(w, h, 24); cam.targetTexture = rt; cam.aspect = (float)w / h; cam.depth = -10;
            yield return null; yield return new WaitForEndOfFrame(); yield return null; yield return new WaitForEndOfFrame();
            var prev = RenderTexture.active; RenderTexture.active = rt; var tex = new Texture2D(w, h, TextureFormat.RGB24, false); tex.ReadPixels(new Rect(0, 0, w, h), 0, 0); tex.Apply(); RenderTexture.active = prev;
            File.WriteAllBytes(Path.Combine(outDir, name + ".png"), tex.EncodeToPNG()); Destroy(tex); cam.targetTexture = null; rt.Release(); Destroy(go);
        }

        // ---------------------------------------------------------------- one picture of each item
        IEnumerator Shots1300()
        {
            var rw = RoamWorld.Current; if (!rw) { Note("NO ITEMS"); yield break; }
            var car = race.vehicle; car.Body.isKinematic = true; car.Body.position = new Vector3(0, 900, 0); Physics.SyncTransforms(); HideUi1300(true);
            foreach (var i in rw.Items)
            {
                var f = Quaternion.Euler(0, i.yaw, 0) * Vector3.forward; var right = Vector3.Cross(Vector3.up, f); var at = new Vector3(i.pos[0], 0, i.pos[1]); at.y = Top1300(at);
                Vector3 eye, look;
                // from the run-up / the road coming in (cleared), a little above a driver's eye
                if (i.kind == "smash") { eye = at - f * 24; look = at; }
                else if (i.kind == "loop") { eye = at - f * (i.radius + 26); look = at; }
                else if (i.kind == "flip") { eye = at - f * 45; look = at; }
                else { float ahead = i.kind == "kicker" ? 10 : i.kind == "through" ? i.gap + 8 : i.gap * .6f; eye = at - f * 34; look = at + f * ahead; }
                eye.y = Mathf.Max(Top1300(eye), at.y) + (i.kind == "loop" ? 12 : i.kind == "smash" ? 5 : 6); look.y = Top1300(look) + 1;
                car.Body.position = eye + Vector3.down * 3; car.transform.position = car.Body.position; Physics.SyncTransforms(); FindAnyObjectByType<ChaseCamera>()?.Snap();
                yield return Live1300($"item-{i.id.PadLeft(2, '0')}-{i.kind}", eye, look, 1600, 900);
                Note($"  item {i.id} {i.kind} '{i.title}' at {i.pos[0]:F0},{i.pos[1]:F0} facing {i.yaw:F0}: shot from {V(eye)}");
            }
            HideUi1300(false); yield return TopDown1300("items-top-down"); car.Body.isKinematic = false;
            // the map and the minimap with the icons
            flow.Resume(); Put1300(car, new Vector3(rw.Items[0].pos[0], 0, rw.Items[0].pos[1]) - Quaternion.Euler(0, rw.Items[0].yaw, 0) * Vector3.forward * 30, rw.Items[0].yaw, 0); yield return new WaitForSecondsRealtime(2); yield return Snap("minimap-icons");
            var map = flow.GetComponent<ExplorationMap>(); if (map) { map.Open(); yield return new WaitForSecondsRealtime(1); yield return Snap("map-icons"); map.Close(); }
        }

        // ---------------------------------------------------------------- jumps by the pad
        sealed class Flight1300 { public float takeoffSpeed, air, apex, distance, upAtRest; public bool wiped, landed; public Vector3 takeoff, land; public List<string> touches = new(); }
        IEnumerator Fly1300(ArcadeVehicle car, Vector3 start, Vector3 dir, float speed, Vector3 lip, float lipY, Flight1300 r, float limit = 14)
        {
            BindPad(); Put1300(car, start, Yaw(dir), speed); FindAnyObjectByType<ChaseCamera>()?.Snap();
            var contacts = car.GetComponent<Report106Contacts>() ?? car.gameObject.AddComponent<Report106Contacts>(); contacts.who = "vehicle"; contacts.list.Clear();
            var cfg = car.GetComponent<VehicleConfiguration>(); int wipes = cfg.Wipeouts; float t0 = Time.time, stable = 0; bool flying = false, past = false;
            var target = lip + dir * 400;
            while (Time.time - t0 < limit)
            {
                Drive(car, target + Vector3.up * car.Body.position.y, 60); yield return new WaitForFixedUpdate();
                var p = car.Body.position; float along = Vector3.Dot(Flat(p - lip), dir);
                if (!past && along > -1) past = true;
                if (past && !flying && car.GroundedWheels == 0) { flying = true; r.takeoff = p; r.takeoffSpeed = car.Body.linearVelocity.magnitude; }
                if (flying) { r.apex = Mathf.Max(r.apex, p.y - lipY); if (car.GroundedWheels == 0) r.air += Time.fixedDeltaTime; else if (!r.landed) { r.landed = true; r.land = p; } }
                if (r.landed) { stable = car.GroundedWheels >= 2 ? stable + Time.fixedDeltaTime : 0; if (stable > 1) break; }
            }
            Release(); r.distance = Vector3.Distance(Flat(r.land), Flat(r.takeoff)); r.upAtRest = car.transform.up.y; r.wiped = cfg.Wipeouts != wipes;
            r.touches = contacts.list.Where(s => !s.Contains("Free Roam") && !s.Contains("Ground_")).Distinct().Take(6).ToList();
        }
        IEnumerator Jumps1300()
        {
            var rw = RoamWorld.Current; var car = race.vehicle; float speed = F(Arg("-p1300speed", "30")); int high = int.Parse(Arg("-p1300high", "0"));
            var ids = Arg("-p1300ids", "").Split(',').Where(s => s.Length > 0).ToArray(); if (ids.Length == 0) ids = rw.Ramps.Keys.Where(k => rw.Items.First(i => i.id == k).kind != "flip").ToArray();
            foreach (var id in ids)
            {
                if (!rw.Ramps.TryGetValue(id, out var ramp)) { Note($"  {id}: no such ramp"); continue; }
                var item = rw.Items.First(i => i.id == id);
                foreach (bool on in high == 2 ? new[] { false, true } : new[] { high == 1 })
                {
                    High1300(on); var r = new Flight1300(); var start = ramp.lip - ramp.dir * 70;
                    bool shot = false; IEnumerator Watch() { while (r.air < .5f && !r.landed) yield return null; if (!shot) { shot = true; yield return Snap($"jump-{id.PadLeft(2, '0')}-{item.kind}-{(on ? "high" : "normal")}"); } }
                    StartCoroutine(Watch());
                    yield return Fly1300(car, start, ramp.dir, speed, ramp.lip, ramp.lipY, r);
                    Note($"  jump {id} ({item.kind}{(item.house != null && item.house != "" ? " over " + item.house : "")}) {(on ? "HIGH" : "normal")} {car.GetComponent<VehicleConfiguration>().profileId}: takeoff {r.takeoffSpeed:F1} m/s, air {r.air:F2} s, {r.distance:F0} m, apex {r.apex:F1} m over the lip, landed {(r.landed ? "yes" : "NO")} {(r.wiped ? "WIPED OUT" : "upright")} (up {r.upAtRest:F2}); touched: {(r.touches.Count == 0 ? "nothing but ground" : string.Join("; ", r.touches))}");
                }
            }
            High1300(false);
        }
        IEnumerator Flips1300()
        {
            var rw = RoamWorld.Current; var car = race.vehicle; float speed = F(Arg("-p1300speed", "24")); var cfg = car.GetComponent<VehicleConfiguration>();
            foreach (var flip in rw.Flips)
            {
                var ramp = rw.Ramps[flip.id];
                foreach (bool on in new[] { false, true })
                {
                    High1300(on); int launches = FlipRamp.Launches, w0 = cfg.Wipeouts; float heading0 = Yaw(ramp.dir);
                    var contacts = car.GetComponent<Report106Contacts>() ?? car.gameObject.AddComponent<Report106Contacts>(); contacts.who = "vehicle"; contacts.list.Clear();
                    BindPad(); Put1300(car, ramp.lip - ramp.dir * 75, heading0, speed); FindAnyObjectByType<ChaseCamera>()?.Snap(); float t0 = Time.time;
                    while (Time.time - t0 < 10 && FlipRamp.Launches == launches) { Drive(car, ramp.lip + ramp.dir * 50, 30); yield return new WaitForFixedUpdate(); }
                    Release(); bool launched = FlipRamp.Launches > launches; float apex = 0, air = 0, ground = 0; Vector3 from = car.Body.position; bool shot = false; t0 = Time.time;
                    while (launched && Time.time - t0 < 9)
                    {
                        yield return new WaitForFixedUpdate(); apex = Mathf.Max(apex, car.Body.position.y - ramp.lipY);
                        if (car.GroundedWheels < 2) { air += Time.fixedDeltaTime; ground = 0; } else ground += Time.fixedDeltaTime;
                        if (!shot && Time.time - t0 > 1.2f) { shot = true; yield return Snap($"flip-{flip.id.PadLeft(2, '0')}-{(on ? "high" : "normal")}-air"); }
                        if (air > .5f && ground > .6f) break;
                    }
                    float turned = Mathf.Abs(Mathf.DeltaAngle(heading0, car.transform.eulerAngles.y)); yield return Snap($"flip-{flip.id.PadLeft(2, '0')}-{(on ? "high" : "normal")}-landed");
                    Note($"  flip {flip.id} {(on ? "HIGH" : "normal")}: launched {(launched ? "yes" : "NO")} at {V(from)}, apex {apex:F1} m over the lip, air {air:F2} s, landed {Vector3.Dot(Flat(ramp.lip - car.Body.position), ramp.dir):F0} m back down the road from the lip, {(cfg.Wipeouts != w0 ? "WIPED OUT" : "no wipe-out")}, up {car.transform.up.y:F2}, facing {turned:F0} degrees from the way in{(turned > 150 ? " (back the way it came)" : " (NOT turned round)")}; touched: {string.Join("; ", contacts.list.Where(x => !x.Contains("Ground_")).Distinct().Take(4).DefaultIfEmpty("nothing but ground"))}");
                }
            }
            High1300(false);
        }

        // ---------------------------------------------------------------- 1.3.0-review2: HIGH MODE race jumps: the lane between the bumpers on each landing
        // (measured from the course line out to the bumper on each side, every 4 m from the lip to 60 m past it), a top-down picture of it, and the jump
        // driven by the pad on the line and 3 m either side of it: where it landed and whether it stayed between the bumpers
        // every jump of the course: a stretch of the main line or of a shortcut with no road under it (as the bumpers find it: they leave such a gap open)
        sealed class Jump1300 { public string line; public Func<float, (Vector3 p, Vector3 f)> at; public Func<Vector3, float> project; public float lip, land, length; public bool loop; }
        List<Jump1300> FindJumps1300()
        {
            var list = new List<Jump1300>(); var mask = race.vehicle.groundMask.value; var hits = new RaycastHit[16];
            bool Road(Vector3 p) { int n = Physics.RaycastNonAlloc(p + Vector3.up * 2.5f, Vector3.down, hits, 6, mask, QueryTriggerInteraction.Ignore); for (int i = 0; i < n; i++) { var h = hits[i]; if (h.collider.attachedRigidbody || h.normal.y < .5f || h.collider.gameObject.layer == HighBumpers.Layer) continue; if (Mathf.Abs(h.point.y - p.y) < 1.6f) return true; } return false; }
            void Scan(string name, Func<float, (Vector3, Vector3)> at, Func<Vector3, float> project, float length, bool loop)
            {
                float start = -1;
                for (float s = 0; s <= length; s += 2)
                {
                    var (p, _) = at(s); bool gap = !Road(p);
                    if (gap && start < 0) start = s;
                    if ((!gap || s + 2 > length) && start >= 0) { if (s - start >= 4) list.Add(new Jump1300 { line = name, at = at, project = project, lip = start - 2, land = s + 2, length = length, loop = loop }); start = -1; }
                }
            }
            var road = race.road; road.Initialize(); Scan("main", x => { var p = road.At(x, out var f); return (p, f); }, q => road.Project(q, out _), road.Length, !road.openHighway);
            foreach (var w in race.Branches ?? new WoodlandRoute[0]) { if (!w || !w.isActiveAndEnabled || w.points == null || w.points.Length < 2) continue; var r = w; Scan("shortcut '" + r.title + "'", x => { var p = r.At(x, out var f); return (p, f); }, q => r.Project(q, out _), r.Length, false); }
            return list;
        }
        IEnumerator Landings1300(int course)
        {
            Campaign.Testing = true; yield return QuickTo115(course); Conditions(); HighMode.Declined = true;
            flow.Save.Settings.vehicleId = Arg("-p106vehicle", "original"); flow.Save.SaveSettings(); flow.SetHighMode(true);
            flow.StartRace(); yield return WaitRacing(); Bind(); BindPad(); AudioListener.volume = 0;
            var car = race.vehicle; var flights = race.GetComponent<MountainFlights>()?.flights ?? new MountainFlights.Flight[0]; var jumps = FindJumps1300();
            Note($"== HIGH MODE {RacePlaylists.Titles[course]} ({race.courseId}): race HIGH {race.high}, {jumps.Count} jumps found (gaps in the line), bumpers {HighBumpers.Samples} stations, {HighBumpers.Openings} open, {HighBumpers.LandingStations} set wider on landings");
            var bumps = car.GetComponent<Bumps1200>() ?? car.gameObject.AddComponent<Bumps1200>();
            float Lane(Vector3 p, Vector3 dir) { return Physics.Raycast(p + Vector3.up * .8f, dir, out var h, 25, 1 << HighBumpers.Layer, QueryTriggerInteraction.Ignore) ? h.distance : 99; }
            for (int k = 0; k < jumps.Count; k++)
            {
                var j = jumps[k]; float S(float x) => j.loop ? Mathf.Repeat(x, j.length) : Mathf.Clamp(x, 0, j.length);
                var (lipP, lipF) = j.at(S(j.lip)); var (landP, _) = j.at(S(j.land));
                var fl = flights.FirstOrDefault(x => Vector3.Distance(Flat(x.lip), Flat(lipP)) < 40);
                var rows = new List<string>();
                for (float d = -4; d <= 52; d += 4) { var (p, fw) = j.at(S(j.land + d)); var right = Vector3.Cross(Vector3.up, Flat(fw).normalized); float l = Lane(p, -right), r = Lane(p, right); rows.Add($"{d:F0}:{(l < 99 ? l.ToString("F1") : "-")}|{(r < 99 ? r.ToString("F1") : "-")}"); }
                Note($"  jump {k} on the {j.line}{(fl != null ? $" ('{fl.name}')" : "")}: lip {V(lipP)}, lands {V(landP)}, gap {j.land - j.lip:F0} m; lane left|right of the line, metres past the landing's start: {string.Join(" ", rows)}");
                var mid = (lipP + landP) * .5f; car.Body.isKinematic = true; car.Body.position = mid + Vector3.up * 40; car.transform.position = car.Body.position; Physics.SyncTransforms();
                HideUi1300(true); yield return Live1300($"landing-{course}-{k}", mid + Vector3.up * 80, null, 1400, 1400, Mathf.Max(30, Vector3.Distance(lipP, landP) * .5f + 30)); HideUi1300(false); car.Body.isKinematic = false;
                float speed = fl != null && fl.aiTakeoffSpeed > 0 ? fl.aiTakeoffSpeed : 24;
                foreach (float off in new[] { 0f, -3f, 3f })
                {
                    float s0 = j.loop ? j.lip - 70 : Mathf.Max(0, j.lip - 70); var (fs, f0) = j.at(S(s0)); var right0 = Vector3.Cross(Vector3.up, Flat(f0).normalized);
                    Put1300(car, fs + right0 * off, Yaw(f0), speed * .9f); FindAnyObjectByType<ChaseCamera>()?.Snap(); int b0 = bumps.Hits; float t0 = Time.time; bool air = false, landed = false; Vector3 land = default; bool shot = false;
                    while (Time.time - t0 < 14)
                    {
                        float s = j.project(car.Body.position); var (tgt, tf) = j.at(S(s + 25)); tgt += Vector3.Cross(Vector3.up, Flat(tf).normalized) * off;
                        Drive(car, tgt, speed); yield return new WaitForFixedUpdate();
                        float rel = j.loop ? Mathf.Repeat(s - j.lip, j.length) : s - j.lip;
                        if (!air && car.GroundedWheels == 0 && rel > -6 && rel < j.land - j.lip + 10) air = true;
                        if (air && !shot && car.GroundedWheels == 0 && Time.time - t0 > 1) { shot = true; StartCoroutine(Snap($"landing-{course}-{k}-off{off:+0;-0;0}")); }
                        if (air && !landed && car.GroundedWheels >= 2) { landed = true; land = car.Body.position; t0 = Time.time - 11; }
                    }
                    Release(); float sl = j.project(land); var (lp, lf) = j.at(S(sl)); var lr = Vector3.Cross(Vector3.up, Flat(lf).normalized);
                    float lat = Vector3.Dot(Flat(land - lp), lr), lane = lat < 0 ? Lane(lp, -lr) : Lane(lp, lr);
                    var (ep, _) = j.at(S(j.project(car.Body.position))); float endLat = Vector3.Dot(Flat(car.Body.position - ep), lr);
                    Note($"    {off:+0;-0;0} m off the line at {speed:F0} m/s: {(landed ? $"landed {sl - j.land:F0} m past the landing's start, {lat:F1} m from the line (bumper on that side at {(lane < 99 ? lane.ToString("F1") : "none")} m) -> {(lane < 99 && Mathf.Abs(lat) > lane ? "OUTSIDE the bumpers" : "inside")}" : "no landing seen")}; bumper hits {bumps.Hits - b0}; 2 s later {endLat:F1} m from the line");
                }
            }
        }

        // ---------------------------------------------------------------- 1.3.0-review2: Fence Line Smash in Free Roam (Dan: "I break fences it should count. Cleared
        // almost the whole area didn't even get one point"): smashed without an attempt, with one started from the pause menu's Start, and in HIGH MODE
        IEnumerator Fence1300()
        {
            var act = flow.Activities; var props = SmashProps(out var site); var car = race.vehicle;
            Note($"  site {site?.id} '{site?.title}' at {V(site.transform.position)} radius {site.radius}: {props.Length} props, {site.Seconds} s; selected now {act.Selected?.id}");
            int broken(IEnumerable<BreakableProp> ps) => ps.Count(p => p.IsBroken);
            IEnumerator Run(string tag, int from, int n)
            {
                for (int i = from; i < from + n && i < props.Length; i++) yield return Through(props[i], site, 0);
                yield return new WaitForSeconds(.5f);
                Note($"  {tag}: {n} props driven through: {broken(props)} broken; counted {act.SmashCount}; attempt {(act.AttemptActive ? "running" : "not running")}, selected {act.Selected?.id}; HUD '{act.Hud.Replace('\n', '/')}'");
                yield return Snap("fence-" + tag);
            }
            // A: just smashing (no attempt)
            Put1300(car, site.transform.position, 0, 0); yield return new WaitForSeconds(1); Note($"  at the site: HUD '{act.Hud.Replace('\n', '/')}'");
            yield return Run("no-attempt", 0, 3);
            // B: the pause menu's Activities > Start / Retry (its own action: BeginAttempt, then resume)
            flow.Pause(); flow.OpenActivities(); yield return new WaitForSecondsRealtime(.5f); act.BeginAttempt(); flow.Resume(); yield return new WaitForSeconds(.5f);
            Note($"  START from the pause menu: attempt {(act.AttemptActive ? "running" : "NOT running")}, selected {act.Selected?.id}, counted {act.SmashCount}, standing {props.Count(p => !p.IsBroken)}");
            yield return Run("attempt-normal", 3, 5);
            // C: HIGH MODE
            act.Cancel(); High1300(true); act.BeginAttempt(); yield return new WaitForSeconds(.5f);
            yield return Run("attempt-high", 8, 5);
            High1300(false);
        }

        // the campaign's Fence Line Smash (Dan: "this in particular was during the campaign where you had to smash fences"), started from the campaign page and
        // driven by the pad from the event's own start into each marked prop in turn (no teleports), the count logged as it goes
        IEnumerator CampFence1300()
        {
            Conditions(); yield return PanelLoop("campfence-start"); yield return ToCampaign(); Note($"slot {Campaign.SlotName}");
            yield return StartSmash(Arg("-p106vehicle", "original")); BindPad();
            var act = flow.Activities; var props = SmashProps(out var site); var car = race.vehicle; int hits = 0; void Count(BreakableProp p, ArcadeVehicle v) { hits++; } BreakableProp.BrokenByVehicle += Count;
            Note($"  site {site?.id}: {props.Length} props; car {car.name} at {V(car.Body.position)}, activities' car is the race vehicle: {act.PlayerObject == car.gameObject}; selected {act.Selected?.id}, attempt {act.AttemptActive}, area {act.SmashArea}");
            yield return Snap("campfence-go");
            // 1.3.0-review2: any prop counts: the pad goes for the nearest standing prop (fence panels near the start first, not the old 14)
            var all = FindObjectsByType<BreakableProp>(FindObjectsSortMode.None).Where(x => Vector3.Distance(x.transform.position, car.Body.position) < 220).ToList();
            Note($"  {all.Count} breakable props within 220 m of the start; nearest {(all.Count > 0 ? all.Min(x => Vector3.Distance(x.transform.position, car.Body.position)) : 0):F0} m");
            float t0 = Time.time, next = 0; int target = 0; BreakableProp p = null; float chosen = 0;
            while (Time.time - t0 < 44 && flow.State == RaceFlow.Stage.Racing)
            {
                if (!p || p.IsBroken || Time.time - chosen > 6) { var cand = all.Where(x => x && !x.IsBroken && x != p).OrderBy(x => Vector3.Distance(Flat(x.transform.position), Flat(car.Body.position))).FirstOrDefault(); if (!cand) break; p = cand; chosen = Time.time; target++; }
                Drive(car, p.transform.position, 12); yield return new WaitForFixedUpdate();
                if (Time.time - t0 >= next) { next += 2; Note($"    t {Time.time - t0:F0}: at {V(car.Body.position)} {Flat(car.Body.linearVelocity).magnitude:F1} m/s, heading for '{p.name}' ({Vector3.Distance(Flat(car.Body.position), Flat(p.transform.position)):F0} m); broken {all.Count(x => x.IsBroken)} (of the old 14: {props.Count(x => x.IsBroken)}), any prop broken by a vehicle {hits}, counted {act.SmashCount}, live {CampaignRun.LiveCount}, attempt {act.AttemptActive}"); }
                if (Time.time - t0 > 12 && Mathf.Abs(Time.time - t0 - 12) < .03f) yield return Snap("campfence-mid");
            }
            Release(); BreakableProp.BrokenByVehicle -= Count;
            Note($"  end: broken {all.Count(x => x.IsBroken)} (of the old 14: {props.Count(x => x.IsBroken)}), counted {act.SmashCount}, live {CampaignRun.LiveCount}, state {flow.State}, panel '{CampaignEventUi.Current?.PanelText?.Replace('\n', '/')}'");
            yield return Snap("campfence-end");
        }

        // ---------------------------------------------------------------- the turnarounds: driven in from 70 m up the road, round the loop, back up the road
        IEnumerator Loops1300()
        {
            var rw = RoamWorld.Current; var car = race.vehicle;
            foreach (var i in rw.Items.Where(x => x.kind == "loop"))
            {
                var f = Quaternion.Euler(0, i.yaw, 0) * Vector3.forward; var c = new Vector3(i.pos[0], 0, i.pos[1]); c.y = Top1300(c);
                BindPad(); Put1300(car, c - f * (i.radius + 170), i.yaw, 14); FindAnyObjectByType<ChaseCamera>()?.Snap(); // from the channel through the edge bank, so the way in is driven too
                var contacts = car.GetComponent<Report106Contacts>() ?? car.gameObject.AddComponent<Report106Contacts>(); contacts.who = "vehicle"; contacts.list.Clear();
                float inAngle = Mathf.Atan2(-f.x, -f.z) * Mathf.Rad2Deg; var targets = new List<Vector3> { c };
                for (int k = 1; k <= 11; k++) { float a = (inAngle + 180 + k * 30 - 150) * Mathf.Deg2Rad; targets.Add(c + new Vector3(Mathf.Sin(a), 0, Mathf.Cos(a)) * i.radius * .8f); }
                targets.Add(c - f * (i.radius + 45));
                int next = 0, jumps = 0; float t0 = Time.time, farthest = -999; bool shot = false; var prev = car.Body.position;
                while (Time.time - t0 < 70 && next < targets.Count)
                {
                    Drive(car, targets[next], next == 0 ? 14 : 10); yield return new WaitForFixedUpdate();
                    farthest = Mathf.Max(farthest, Vector3.Dot(Flat(car.Body.position - c), f)); if (Vector3.Distance(prev, car.Body.position) > 8) jumps++; prev = car.Body.position;
                    if (Vector3.Distance(Flat(car.Body.position), Flat(targets[next])) < (next == 0 ? i.radius * .5f : 6)) { next++; if (next == 6 && !shot) { shot = true; yield return Snap($"loop-{i.id.PadLeft(2, '0')}"); } }
                }
                Release();
                float turned = Mathf.Abs(Mathf.DeltaAngle(i.yaw, car.transform.eulerAngles.y));
                Note($"  loop {i.id}: reached {next} of {targets.Count} points in {Time.time - t0:F0} s, deepest {farthest:F1} m past the loop's centre, now {Vector3.Dot(Flat(car.Body.position - c), -f):F0} m back up the road facing {turned:F0} degrees from the way in{(turned > 150 && next == targets.Count ? " (turned round)" : " (NOT through)")}, resets by the game (playable area) {jumps}; touched: {string.Join("; ", contacts.list.Where(s => !s.Contains("Ground_")).Distinct().Take(5).DefaultIfEmpty("nothing but ground"))}");
            }
        }

        // ---------------------------------------------------------------- trees
        IEnumerator Trees1300()
        {
            var car = race.vehicle; var trees = SceneryWorld.Current.Trees; var cfg = car.GetComponent<VehicleConfiguration>();
            // a trunk with 40 m of clear run-up (nothing else standing in a 2.4 m lane) on open ground, of the width wanted
            (BoxCollider box, Vector3 bottom, float width, Vector3 dir)? Pick(float lo, float hi, BoxCollider not = null)
            {
                foreach (var t in trees.BreakableTrees.Where(t => t.box.enabled && t.box != not && t.width >= lo && t.width <= hi && Mathf.Abs(t.bottom.x) < 600 && t.bottom.z > -600 && t.bottom.z < 450).OrderBy(t => t.bottom.x * 13 + t.bottom.z))
                    for (int a = 0; a < 8; a++)
                    {
                        var d = Quaternion.Euler(0, a * 45, 0) * Vector3.forward; var from = t.bottom - d * 40 + Vector3.up * 1;
                        if (Mathf.Abs(Top1300(from) - t.bottom.y) > 3) continue;
                        bool clear = true; foreach (var h in Physics.SphereCastAll(from, 1.2f, d, 38.5f, ~0, QueryTriggerInteraction.Ignore)) { if (h.collider == t.box || h.collider.name.StartsWith("Ground_") || h.collider.attachedRigidbody) continue; clear = false; break; }
                        if (clear) return (t.box, t.bottom, t.width, d);
                    }
                return null;
            }
            IEnumerator Hit(string name, (BoxCollider box, Vector3 bottom, float width, Vector3 dir) t, float speed, bool high)
            {
                High1300(high); int broken = TreeBreaker.Broken; int w0 = cfg.Wipeouts;
                Put1300(car, t.bottom - t.dir * 22, Yaw(t.dir), speed); FindAnyObjectByType<ChaseCamera>()?.Snap(); float before = speed, after = 0; bool shot = false; float t0 = Time.time;
                while (Time.time - t0 < 4)
                {
                    car.Body.linearVelocity = Vector3.ProjectOnPlane(car.Body.linearVelocity, Vector3.up).normalized * (TreeBreaker.Broken == broken ? speed : car.Body.linearVelocity.magnitude) + Vector3.up * car.Body.linearVelocity.y;
                    yield return new WaitForFixedUpdate();
                    if (TreeBreaker.Broken != broken && after == 0) { after = Flat(car.Body.linearVelocity).magnitude; }
                    if (TreeBreaker.Broken != broken && !shot && Time.time - t0 > 0) { shot = true; yield return new WaitForSeconds(.5f); yield return Snap("tree-" + name); }
                    if (Vector3.Dot(Flat(car.Body.position - t.bottom), t.dir) > 12) break;
                }
                if (!shot) yield return Snap("tree-" + name);
                Note($"    vehicle ended {Vector3.Dot(Flat(car.Body.position - t.bottom), t.dir):F1} m past the trunk, {Vector3.Distance(Flat(car.Body.position), Flat(t.bottom)):F1} m from it");
                bool down = TreeBreaker.Broken != broken;
                Note($"  {name}: trunk {t.width:F2} m wide at {V(t.bottom)} (needs {TreeBreaker.Needed(t.width, high):F1} m/s), hit at {before:F1} m/s {(high ? "in HIGH MODE" : "in normal mode")}: {(down ? $"BROKE, speed {before:F1} -> {after:F1} m/s ({(1 - after / before) * 100:F0}% lost)" : $"stood (vehicle now {Flat(car.Body.linearVelocity).magnitude:F1} m/s)")}, collider {(t.box.enabled ? "on" : "off")}, fallen trees {trees.FallenCount}, wipe-outs {cfg.Wipeouts - w0}; {TreeBreaker.Last}");
                car.Body.linearVelocity = Vector3.zero; yield return new WaitForSeconds(.5f);
            }
            var thin = Pick(.5f, .6f); var big = Pick(.95f, 2f); var thin2 = thin.HasValue ? Pick(.5f, .6f, thin.Value.box) : null;
            if (!thin.HasValue || !big.HasValue) { Note($"  no clear trunk found (thin {thin.HasValue}, big {big.HasValue})"); yield break; }
            yield return Hit("thin-12ms-normal", thin.Value, 12, false);
            yield return Hit("big-12ms-normal", big.Value, 12, false);
            yield return Hit("big-30ms-normal", big.Value, 30, false);
            if (thin2.HasValue) yield return Hit("thin-12ms-high", thin2.Value, 12, true);
            // 1.3.0-review2: HIGH MODE breaks trees much more easily: a thin one at 5 m/s, a big one at 12 m/s (both stand in normal mode)
            var thin3 = Pick(.5f, .6f, thin2?.box); var big2 = Pick(.95f, 2f, big.Value.box);
            if (thin3.HasValue) yield return Hit("thin-5ms-high", thin3.Value, 5, true);
            if (big2.HasValue) yield return Hit("big-12ms-high", big2.Value, 12, true);
            High1300(false);
            int fallen = trees.FallenCount; Put1300(car, thin.Value.bottom + Vector3.right * 400, 0, 0); yield return new WaitForSeconds(5);
            Note($"  then 400 m away for 5 s: fallen trees {fallen} -> {trees.FallenCount}, regrown {SceneryTrees.Regrown}, colliders back: thin {thin.Value.box.enabled}, big {big.Value.box.enabled}");
        }

        // ---------------------------------------------------------------- smash spots
        IEnumerator Smash1300()
        {
            var rw = RoamWorld.Current; var car = race.vehicle;
            foreach (var (item, props) in rw.Spots.Take(int.Parse(Arg("-p1300spots", "1"))))
            {
                var f = Quaternion.Euler(0, item.yaw, 0) * Vector3.forward; var at = new Vector3(item.pos[0], 0, item.pos[1]);
                var r = new Flight1300(); bool shot = false; IEnumerator Watch() { while (props.Count(p => p.IsBroken) < 3) yield return null; yield return new WaitForSeconds(.4f); if (!shot) { shot = true; yield return Snap($"smash-{item.id.PadLeft(2, '0')}"); } }
                StartCoroutine(Watch());
                var hit = new HashSet<BreakableProp>(); void Count(BreakableProp p, ArcadeVehicle v) { if (props.Contains(p)) hit.Add(p); } BreakableProp.BrokenByVehicle += Count;
                BindPad(); Put1300(car, at - f * 34, item.yaw, 20); float t0 = Time.time;
                while (Time.time - t0 < 4) { Drive(car, at + f * 60, 20); yield return new WaitForFixedUpdate(); }
                Release(); BreakableProp.BrokenByVehicle -= Count; int broken = hit.Count;
                Note($"  smash {item.id}: driven through at 20 m/s: {broken} of {props.Count} props smashed");
                Put1300(car, at + Vector3.right * 300, 0, 0); yield return new WaitForSeconds(4);
                Note($"  then 300 m away: {props.Count(p => p.IsBroken || p.PendingRestore)} still down, {props.Count(p => !p.IsBroken && !p.PendingRestore)} of {props.Count} back in place");
            }
        }

        // ---------------------------------------------------------------- two-player Free Roam in the forest
        IEnumerator Perf1300()
        {
            var pad2 = InputSystem.AddDevice<Gamepad>("Report1300Pad2"); QualitySettings.vSyncCount = 0; Application.targetFrameRate = -1;
            SplitScreen.Mode = SplitScreen.Kind.FreeRoam; SplitScreen.Solo = false; SplitScreen.P1Device = pad; SplitScreen.P2Device = pad2; SplitScreen.P2Ai = false; SplitScreen.P1Vehicle = "original"; SplitScreen.P2Vehicle = "atv";
            SplitScreen.Time = TimeOfDay.Day; SplitScreen.Weather = Weather.Clear; SplitScreen.Traffic = true; flow.StartSplit(); float w0 = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup - w0 < 150) { Bind(); if (flow && SplitRoam.Current && SplitRoam.Current.Car && flow.State == RaceFlow.Stage.Racing) break; yield return null; }
            QualitySettings.vSyncCount = 0; Application.targetFrameRate = -1; AudioListener.volume = 0; WorldLook.Current?.SetClock(WorldLook.Current.Day, 12);
            var views = Arg("-p1300views", "").Split(';').Where(s => s.Length > 0).Select(s => s.Split('|')).Select(a => (a[0], new Vector3(F(a[1]), 0, F(a[2])), F(a[3]))).ToList();
            var trees = SceneryWorld.Current.Trees; var results = new List<(string, float)>();
            IEnumerator Measure(string name, Vector3 at, float yaw)
            {
                var car = race.vehicle; Put1300(car, at, yaw, 0); car.Body.isKinematic = true; FindAnyObjectByType<ChaseCamera>()?.Snap();
                var c2 = SplitRoam.Current ? SplitRoam.Current.Car : null; if (c2) { Put1300(c2, at + Quaternion.Euler(0, yaw, 0) * new Vector3(4, 0, -6), yaw, 0); c2.Body.isKinematic = true; }
                yield return new WaitForSecondsRealtime(2);
                int frames = 0; float t0 = Time.realtimeSinceStartup; while (Time.realtimeSinceStartup - t0 < 4) { frames++; yield return null; }
                float ms = (Time.realtimeSinceStartup - t0) * 1000 / frames; results.Add((name, ms)); Note($"  {name}: {ms:F2} ms per frame ({frames} frames in 4 s)");
                car.Body.isKinematic = false; if (c2) c2.Body.isKinematic = false;
            }
            foreach (var (name, at, yaw) in views) yield return Measure(name, at, yaw);
            if (!RoamWorld.Disabled && trees.Breakable)
                foreach (var (name, at, yaw) in views)
                {
                    int n = 0; var f = Quaternion.Euler(0, yaw, 0) * Vector3.forward;
                    foreach (var t in trees.BreakableTrees.Where(t => t.box.enabled && Vector3.Distance(Flat(t.bottom), Flat(at + f * 25)) < 30).Take(12).ToList()) if (trees.Break(t.box, f, 20)) n++;
                    yield return Measure(name + $" with {n} trees broken in view", at, yaw);
                    if (name == views[0].Item1) yield return Snap("perf-broken-trees");
                }
            var worst = results.OrderByDescending(r => r.Item2).First();
            Note($"worst view: {worst.Item1} {worst.Item2:F2} ms; all views {results.Average(r => r.Item2):F2} ms (split-screen Free Roam {Screen.width}x{Screen.height}, {(RoamWorld.Disabled ? "before (off)" : "after")})");
        }
    }
}
