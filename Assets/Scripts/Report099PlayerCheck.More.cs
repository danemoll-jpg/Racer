using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Racer
{
    // 0.99 checks for Parts D-F in the built player (see Report099PlayerCheck.cs)
    //   -p99kind hidden   Free Roam: the hidden police (a pass under the limit, off the road, over the limit, a bust, an escape, the setting Off) and a shot of one in its spot
    //   -p99kind getaway  a solo Getaway (Normal): the radio line by line (text, IDs, whose voice), heat forced up to see police bikes among the units, shots
    //   -p99kind patrol   Speed Patrol with the police bike as the cop
    //   -p99kind unlocks  the two police goals on an isolated save: silhouettes, the panel, the vehicles in the garage, then a race with the bike
    public sealed partial class Report099PlayerCheck
    {
        IEnumerator RunMore99(string kind)
        {
            switch (kind)
            {
                case "hidden": yield return Hidden(); break;
                case "getaway": yield return GetawayRadio(); break;
                case "patrol": yield return Patrol(); break;
                case "unlocks": yield return Unlocks(); break;
                default: Note("unknown kind " + kind); break;
            }
        }
        const BindingFlags NonPublic = BindingFlags.NonPublic | BindingFlags.Instance;
        RaceMenus Menus => (RaceMenus)typeof(RaceFlow).GetField("menus", NonPublic).GetValue(flow);
        void CloseUnlockPanel() { var m = Menus; if (m.UnlockOpen) typeof(RaceMenus).GetMethod("CloseUnlock", NonPublic).Invoke(m, null); }
        void ShowGarageLocked(string id) { var m = Menus; typeof(RaceMenus).GetField("garageLocked", NonPublic).SetValue(m, id); typeof(RaceMenus).GetMethod("Show", NonPublic | BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null).Invoke(m, null); }
        IEnumerator ToFreeRoam(string vehicle)
        {
            flow.Save.Settings.vehicleId = vehicle; flow.Save.Settings.hints = false; flow.Save.SaveSettings(); Conditions(0, 0);
            flow.StartFreeRoam(); float t0 = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup - t0 < 120 && UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != RaceFlow.RoamScene) yield return null;
            Bind(); t0 = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup - t0 < 120) { Bind(); if (flow && race && flow.Started && !LoadingScreen.Holding && flow.State == RaceFlow.Stage.Racing) break; yield return null; }
            AudioListener.volume = 0; yield return new WaitForSecondsRealtime(1f);
        }
        void Hands(bool on) { race.vehicle.GetComponent<VehicleInput>().enabled = on; }
        // a scripted pass: the player's vehicle carried along the road nodes at a steady speed (the physics body is set each step)
        IEnumerator Carry(RoadNet net, List<int> path, float mps, Func<bool> stop)
        {
            var car = race.vehicle; Hands(false); var pts = path.Select(n => net.P[n]).ToList(); int i = 0; var cur = pts[0];
            while (i + 1 < pts.Count && !stop())
            {
                yield return new WaitForFixedUpdate(); float walked = mps * Time.fixedDeltaTime;
                while (walked > 0 && i + 1 < pts.Count) { float seg = Vector3.Distance(cur, pts[i + 1]); if (seg <= walked) { walked -= seg; cur = pts[i + 1]; i++; } else { cur = Vector3.MoveTowards(cur, pts[i + 1], walked); walked = 0; } }
                var dir = i + 1 < pts.Count ? (pts[i + 1] - cur) : car.transform.forward; dir.y = 0; if (dir.sqrMagnitude < .01f) dir = car.transform.forward; dir.Normalize();
                car.Body.position = cur + Vector3.up * .55f; car.Body.rotation = Quaternion.LookRotation(dir); car.Body.linearVelocity = dir * mps; car.Body.angularVelocity = Vector3.zero;
            }
        }
        List<int> RoadPast(RoadNet net, HiddenPolice.Spot s, float before, float after, out bool ok)
        {
            ok = false; int n = net.Nearest(s.pos, out _, 80); if (n < 0) return null; var t = net.Tangent(n);
            int a = net.Ahead(n, -t, before), b = net.Ahead(n, t, after); var path = net.Path(a, b); ok = path != null && path.Count > 3; return path;
        }
        IEnumerator Hidden()
        {
            yield return ToFreeRoam(Arg("-p99vehicle", "original"));
            float w0 = Time.realtimeSinceStartup; while (!HiddenPolice.Current && Time.realtimeSinceStartup - w0 < 20) yield return null;
            var hp = HiddenPolice.Current; if (!hp) { Note("FAIL no HiddenPolice in Free Roam"); yield break; }
            Note($"hiding places: {hp.All.Count} ({string.Join(", ", hp.All.GroupBy(s => s.road).Select(g => g.Count() + " " + g.Key))}); active this session: {hp.Active.Count} at {string.Join(" | ", hp.Active.Select(s => s.road + " " + s.pos.ToString("F0")))}");
            hp.AttachedAt = Time.time - 200; Note("setting On, difficulty " + flow.Save.Settings.hiddenPoliceDifficulty);
            var spot = hp.Active[0]; var net = hp.Net; var path = RoadPast(net, spot, 150, 110, out bool ok); if (!ok) { Note("FAIL no road path past the spot"); yield break; }
            int limit = SpeedPatrol.LimitAt(race, spot.pos, out _); Note($"spot 1 on {spot.road} at {spot.pos:F0}, limit {limit} mph");
            // a shot of the hidden cop in its spot (the player 45 m up the road, stopped)
            var car = race.vehicle; Hands(false);
            yield return Carry(net, path, 12, () => spot.prop != null && Vector3.Distance(car.Body.position, spot.pos) < 45);
            car.Body.linearVelocity = Vector3.zero; yield return new WaitForSecondsRealtime(1.5f);
            Note($"cop prop exists {spot.prop != null}, {(spot.prop ? Vector3.Distance(car.Body.position, spot.prop.Body.position).ToString("F0") : "-")} m from the player");
            if (spot.prop)
            {
                var cam = Camera.main; var chaseCam = FindAnyObjectByType<ChaseCamera>(); if (chaseCam) chaseCam.enabled = false; var cv = CameraViews.Current; if (cv) cv.enabled = false;
                var side = Vector3.Cross(Vector3.up, spot.forward); var eye = spot.prop.Body.position + spot.forward * 13 + Vector3.up * 2.2f + side * 6;
                cam.transform.SetPositionAndRotation(eye, Quaternion.LookRotation(spot.prop.Body.position + Vector3.up - eye));
                var canv = FindObjectsByType<Canvas>(FindObjectsSortMode.None); foreach (var c in canv) c.enabled = false; yield return null; yield return null; yield return Snap("hidden-cop-in-its-spot"); foreach (var c in canv) c.enabled = true;
                if (chaseCam) chaseCam.enabled = true; if (cv) cv.enabled = true;
            }
            // 1: under the limit: nothing
            var pass1 = RoadPast(net, spot, 140, 100, out ok);
            yield return Carry(net, pass1, SpeedPatrol.Mps(limit - 6), () => GetawayChase.Current != null);
            Note($"pass UNDER the limit ({limit - 6} in a {limit}): chase started = {GetawayChase.Current != null} (expected False)");
            // 2: off the road at speed: nothing
            {
                int n = net.Nearest(spot.pos, out _, 80); var t = net.Tangent(n); var right = Vector3.Cross(Vector3.up, t); float sideSign = Vector3.Dot(spot.pos - net.P[n], right) >= 0 ? 1 : -1;
                Vector3 a = net.P[n]; bool found = false; foreach (float lateral in new[] { 40f, 60f, 80f, 110f }) foreach (float sg in new[] { -sideSign, sideSign }) { var cand = net.P[n] - t * 60 + right * sg * lateral; SpeedPatrol.LimitAt(race, cand, out bool onR); if (!found && !onR && net.OffNet(cand) > 20) { a = cand; found = true; } }
                Note($"off-road start point {a:F0} found {found}, {net.OffNet(a):F0} m beyond every road"); Hands(false); float tt = 0; var carr = race.vehicle; carr.Body.position = a + Vector3.up * 3; carr.Body.rotation = Quaternion.LookRotation(t);
                while (tt < 5 && GetawayChase.Current == null) { yield return new WaitForFixedUpdate(); tt += Time.fixedDeltaTime; carr.Body.linearVelocity = t * 30 + Vector3.up * Mathf.Min(0, carr.Body.linearVelocity.y); carr.Body.rotation = Quaternion.LookRotation(t); }
            }
            Note($"pass OFF the road at 67 mph: chase started = {GetawayChase.Current != null} (expected False)");
            // 3: over the limit: the cop clocks it and pulls out
            yield return Carry(net, RoadPast(net, spot, 140, 100, out ok), SpeedPatrol.Mps(limit + 22), () => GetawayChase.Current != null);
            float t1 = Time.realtimeSinceStartup; while (GetawayChase.Current == null && Time.realtimeSinceStartup - t1 < 6) yield return null;
            var g = GetawayChase.Current; Note($"pass OVER the limit ({limit + 22} in a {limit}): chase started = {g != null} (expected True); flash '{HiddenPolice.Flash}'; embedded {(g != null && g.Embedded)}, heat {(g ? g.Heat : 0)}, cops {(g ? g.Cops.Count : 0)}");
            if (g == null) yield break;
            yield return Snap("hidden-chase-start"); yield return new WaitForSecondsRealtime(1.2f); yield return Snap("hidden-chase-hud");
            // 4: busted: the driver stops; the cop arrives
            var runner = g.Runners[0]; Hands(false); float t2 = Time.realtimeSinceStartup;
            while (GetawayChase.Current == g && !runner.caught && Time.realtimeSinceStartup - t2 < 40) { race.vehicle.Body.linearVelocity = Vector3.zero; race.vehicle.Body.angularVelocity = Vector3.zero; yield return new WaitForFixedUpdate(); }
            Note($"bust: caught {runner.caught} after {Time.realtimeSinceStartup - t2:F0} s real, banner '{g.Banner}', radio '{(g.RadioLog.Count > 0 ? g.RadioLog.Last() : "")}'");
            yield return Snap("hidden-busted"); float t3 = Time.realtimeSinceStartup; while (GetawayChase.Current != null && Time.realtimeSinceStartup - t3 < 10) yield return null;
            var cc = race.vehicle; float lat = net.OffNet(cc.Body.position); Note($"after the bust: chase gone = {GetawayChase.Current == null}, vehicle at {cc.Body.position:F0}, speed {cc.Body.linearVelocity.magnitude:F1} m/s, {lat:F1} m beyond the road net, kinematic {cc.Body.isKinematic}, input enabled {cc.GetComponent<VehicleInput>().enabled}, state {flow.State}");
            Hands(true); yield return new WaitForSecondsRealtime(.5f); yield return Snap("hidden-after-bust");
            // 5: an escape: a second chase (the quiet time is skipped), the escape meter filled
            hp.LastEnd = -999; hp.AttachedAt = Time.time - 200; var spot2 = hp.Active.FirstOrDefault(s => s != spot) ?? hp.All.First(s => s != spot); if (!hp.Active.Contains(spot2)) hp.Active.Add(spot2);
            var path2 = RoadPast(net, spot2, 140, 100, out ok); int lim2 = SpeedPatrol.LimitAt(race, spot2.pos, out _);
            if (ok) { yield return Carry(net, path2, SpeedPatrol.Mps(lim2 + 24), () => GetawayChase.Current != null); t1 = Time.realtimeSinceStartup; while (GetawayChase.Current == null && Time.realtimeSinceStartup - t1 < 6) yield return null; }
            g = GetawayChase.Current; Note($"second pass over the limit ({lim2 + 24} in a {lim2}): chase = {g != null}");
            if (g != null)
            {
                runner = g.Runners[0]; Hands(true); float t4 = Time.realtimeSinceStartup; while (Time.realtimeSinceStartup - t4 < 2) yield return null; typeof(GetawayChase).GetMethod("Escaped", NonPublic).Invoke(g, new object[] { runner, false });
                yield return new WaitForSecondsRealtime(1f); Note($"escape: escaped {runner.escaped}, banner '{g.Banner}', radio '{(g.RadioLog.Count > 0 ? g.RadioLog.Last() : "")}'"); yield return Snap("hidden-escaped");
                t3 = Time.realtimeSinceStartup; while (GetawayChase.Current != null && Time.realtimeSinceStartup - t3 < 10) yield return null;
                Note($"after the escape: chase gone = {GetawayChase.Current == null}, cops left in the world {FindObjectsByType<CopDriver>(FindObjectsSortMode.None).Length}, vehicle speed {race.vehicle.Body.linearVelocity.magnitude:F1}, kinematic {race.vehicle.Body.isKinematic}");
            }
            // 6: the setting Off: no hidden cops (cars gone, no trip)
            flow.Save.Settings.hiddenPolice = false; hp.LastEnd = -999; yield return new WaitForSecondsRealtime(1f);
            int props = hp.All.Count(s => s.prop != null); var path3 = RoadPast(net, spot, 140, 100, out ok); if (ok) yield return Carry(net, path3, SpeedPatrol.Mps(limit + 22), () => GetawayChase.Current != null);
            Note($"setting Off: hidden cop cars standing = {props} (expected 0); a pass at {limit + 22} in a {limit}: chase started = {GetawayChase.Current != null} (expected False)");
            Note("PoliceProgress: car earned " + PoliceProgress.CarEarned + ", bike earned " + PoliceProgress.BikeEarned);
        }

        IEnumerator GetawayRadio()
        {
            // solo Getaway on Normal (as GetawayPlayerCheck), the runner on the road AI at pace 2
            SplitScreen.P1Device = UnityEngine.InputSystem.Keyboard.current; SplitScreen.Solo = true; SplitScreen.P2AiRunner = false; SplitScreen.P2Ai = true; SplitScreen.Mode = SplitScreen.Kind.Police; SplitScreen.PoliceGame = SplitScreen.Game.Getaway;
            SplitScreen.PoliceDifficulty = 1; SplitScreen.PoliceMinutes = 5; SplitScreen.Course = 0; SplitScreen.Time = TimeOfDay.Day; SplitScreen.Weather = Weather.Clear; SplitScreen.Traffic = true; SplitScreen.P1Vehicle = Arg("-p99vehicle", "moto"); SplitScreen.P2Vehicle = "atv";
            flow.Save.Settings.hints = false; flow.Save.ApplySettings(); flow.StartSplit(); float t0 = Time.realtimeSinceStartup; yield return null;
            while (Time.realtimeSinceStartup - t0 < 150) { Bind(); if (flow && race && flow.Started && !LoadingScreen.Holding && flow.State == RaceFlow.Stage.Racing && GetawayChase.Current != null && GetawayChase.Current.Runners.Count > 0) break; yield return null; }
            var g = GetawayChase.Current; var runner = g.Runners[0]; var car = race.vehicle; AudioListener.volume = 0;
            var pilot = car.gameObject.AddComponent<RoadDriver>(); pilot.Initialize(race, car, false, 1, 2f); car.GetComponent<VehicleInput>().enabled = false;
            float real0 = Time.realtimeSinceStartup; int seenRadio = 0, seenVoice = 0; var bikeSeen = new HashSet<string>(); int lastHeat = 1; float lastHeatShot = 0; bool holdFree = Arg("-p99hold", "1") == "1";
            Note($"start: heat {g.Heat}, cops {g.Cops.Count}, {string.Join(", ", g.Cops.Select(c => c.car.GetComponent<VehicleConfiguration>().profileId))}");
            float limitSeconds = float.Parse(Arg("-p99seconds", "180"));
            while (g.State != GetawayChase.Phase.Over && g.State != GetawayChase.Phase.Done && Time.realtimeSinceStartup - real0 < limitSeconds)
            {
                yield return null; if (holdFree) { runner.bust = 0; runner.escape = 0; }
                if (g.Heat != lastHeat) { lastHeat = g.Heat; Note($"[{g.Clock:F0}s] HEAT {g.Heat}: alert '{g.Alert}'"); if (Time.realtimeSinceStartup - lastHeatShot > 3) { lastHeatShot = Time.realtimeSinceStartup; yield return Snap($"getaway-heat{g.Heat}"); } }
                for (; seenRadio < g.RadioLog.Count; seenRadio++) Note($"[{g.Clock:F0}s] radio text: {g.RadioLog[seenRadio]}");
                for (; seenVoice < PoliceRadioVoice.Played.Count; seenVoice++) Note($"   voice: {PoliceRadioVoice.Played[seenVoice]}");
                foreach (var c in g.Cops) if (c.car && c.car.GetComponent<VehicleConfiguration>().profileId == "policebike" && bikeSeen.Add(c.car.name)) { Note($"[{g.Clock:F0}s] police bike unit {c.car.name} at heat {g.Heat}, {Vector3.Distance(c.car.Body.position, car.Body.position):F0} m from the runner"); yield return Snap("getaway-police-bike"); }
            }
            // new units at heat 3 or more: one in three is a police bike (the chase's own SpawnCop, six of them placed ahead of the runner)
            { var spawn = typeof(GetawayChase).GetMethod("SpawnCop", NonPublic); int node = g.Net.Nearest(car.Body.position, out _, 400); int ahead = g.Net.Ahead(node, car.transform.forward, 300); var before = g.Cops.Count;
              for (int i = 0; i < 6; i++) spawn.Invoke(g, new object[] { g.Net.P[ahead] + Vector3.right * (i * 4), car.transform.forward, false, true });
              Note($"six new units at heat {g.Heat}: {string.Join(", ", g.Cops.Skip(before).Select(c => c.car.GetComponent<VehicleConfiguration>().profileId))}"); foreach (var c in g.Cops.Skip(before)) if (c.car.GetComponent<VehicleConfiguration>().profileId == "policebike") bikeSeen.Add(c.car.name);
              var bikeUnit = g.Cops.Skip(before).FirstOrDefault(c => c.car.GetComponent<VehicleConfiguration>().profileId == "policebike"); if (bikeUnit != null) { var chaseCam = FindAnyObjectByType<ChaseCamera>(); var cam = Camera.main; if (chaseCam) chaseCam.enabled = false; var cv = CameraViews.Current; if (cv) cv.enabled = false; var bp = bikeUnit.car.Body.position; var eye = bp + Vector3.up * 1.8f + Vector3.right * 4.5f - bikeUnit.car.transform.forward * 4; cam.transform.SetPositionAndRotation(eye, Quaternion.LookRotation(bp + Vector3.up * .8f - eye)); bikeUnit.lights.Siren = true; yield return new WaitForSecondsRealtime(.7f); yield return Snap("getaway-police-bike-close"); if (chaseCam) chaseCam.enabled = true; if (cv) cv.enabled = true; } }
            Note($"cops by profile at the end: {string.Join(", ", g.Cops.Where(c => c.car).GroupBy(c => c.car.GetComponent<VehicleConfiguration>().profileId).Select(x => x.Key + " " + x.Count()))}; police bikes seen {bikeSeen.Count}");
            // the end lines: escaped, spoken at once (priority)
            holdFree = false; typeof(GetawayChase).GetMethod("Escaped", NonPublic).Invoke(g, new object[] { runner, false }); yield return new WaitForSecondsRealtime(1.5f); for (; seenVoice < PoliceRadioVoice.Played.Count; seenVoice++) Note($"   voice: {PoliceRadioVoice.Played[seenVoice]}"); for (; seenRadio < g.RadioLog.Count; seenRadio++) Note($"[{g.Clock:F0}s] radio text: {g.RadioLog[seenRadio]}");
            if (PoliceRadioVoice.Current) { WriteWav(Path.Combine(outDir, "radio-sample.wav"), PoliceRadioVoice.Current); Note("one processed line written to radio-sample.wav"); }
            Note($"{PoliceRadioVoice.Played.Count} lines spoken; one clip at a time by construction (queue of 3, a priority line replaces the waiting ones)");
            flow.QuitSplit(false); yield return new WaitForSecondsRealtime(2);
        }
        static void WriteWav(string path, AudioClip clip)
        {
            var data = new float[clip.samples * clip.channels]; clip.GetData(data, 0); using var f = new FileStream(path, FileMode.Create); using var w = new BinaryWriter(f);
            int bytes = data.Length * 2; w.Write(new[] { 'R', 'I', 'F', 'F' }); w.Write(36 + bytes); w.Write(new[] { 'W', 'A', 'V', 'E', 'f', 'm', 't', ' ' }); w.Write(16); w.Write((short)1); w.Write((short)clip.channels); w.Write(clip.frequency); w.Write(clip.frequency * clip.channels * 2); w.Write((short)(clip.channels * 2)); w.Write((short)16); w.Write(new[] { 'd', 'a', 't', 'a' }); w.Write(bytes);
            foreach (var v in data) w.Write((short)(Mathf.Clamp(v, -1, 1) * 32767));
        }

        IEnumerator Patrol()
        {
            SplitScreen.P1Device = UnityEngine.InputSystem.Keyboard.current; SplitScreen.Solo = true; SplitScreen.P2Ai = true; SplitScreen.Mode = SplitScreen.Kind.Police; SplitScreen.PoliceGame = SplitScreen.Game.SpeedPatrol; SplitScreen.CopVehicle = VehicleProfile.PoliceBike.Id;
            SplitScreen.PoliceMinutes = 3; SplitScreen.Course = 0; SplitScreen.Time = TimeOfDay.Day; SplitScreen.Weather = Weather.Clear; SplitScreen.Traffic = true; flow.Save.Settings.hints = false; flow.Save.ApplySettings(); flow.StartSplit();
            float t0 = Time.realtimeSinceStartup; yield return null; while (Time.realtimeSinceStartup - t0 < 150) { Bind(); if (flow && race && flow.Started && !LoadingScreen.Holding && flow.State == RaceFlow.Stage.Racing && SpeedPatrol.Current != null && SpeedPatrol.Current.Cops.Count > 0) break; yield return null; }
            AudioListener.volume = 0; var sp = SpeedPatrol.Current; var car = race.vehicle; Note($"player vehicle profile {car.GetComponent<VehicleConfiguration>().profileId}");
            yield return new WaitForSecondsRealtime(4); yield return Snap("patrol-bike-start"); int seenVoice = 0;
            for (; seenVoice < PoliceRadioVoice.Played.Count; seenVoice++) Note($"   voice: {PoliceRadioVoice.Played[seenVoice]}");
            Note($"speeders caught before {PoliceProgress.SpeedersCaught}"); var catchM = typeof(SpeedPatrol).GetMethod("Catch", NonPublic);
            var d = FindObjectsByType<RoadDriver>(FindObjectsSortMode.None).FirstOrDefault(x => x.GetComponent<AmbientVehicle>());
            if (d) { var s = new SpeedPatrol.Speeder { driver = d, over = 20, clockedOver = 22, clocked = true }; catchM.Invoke(sp, new object[] { s, sp.Cops[0], false }); }
            yield return new WaitForSecondsRealtime(1.5f); for (; seenVoice < PoliceRadioVoice.Played.Count; seenVoice++) Note($"   voice: {PoliceRadioVoice.Played[seenVoice]}");
            Note($"speeders caught after {PoliceProgress.SpeedersCaught}; cop line '{sp.Cops[0].line}'"); yield return Snap("patrol-bike-catch");
            flow.QuitSplit(false); yield return new WaitForSecondsRealtime(2);
        }

        IEnumerator Unlocks()
        {
            var bike = VehicleProfile.PoliceBike; var car = VehicleProfile.Police; Campaign.Testing = false; flow.Save.Settings.unlockEverything = false;
            Note($"fresh isolated save: car locked {VehicleUnlocks.Locked(car)}, bike locked {VehicleUnlocks.Locked(bike)}; texts '{VehicleUnlocks.LockedTextFor(car)}' / '{VehicleUnlocks.LockedTextFor(bike)}'");
            Note($"in the garage list: {string.Join(", ", race.PlayerVehicles.Select(v => v.Id))}; locked silhouettes: {string.Join(", ", VehicleProfile.All.Where(v => VehicleUnlocks.Locked(v)).Select(v => v.Id))}");
            flow.OpenGarage(); flow.SelectVehicle("moto"); yield return null; yield return new WaitForSecondsRealtime(.6f);
            ShowGarageLocked(bike.Id); yield return new WaitForSecondsRealtime(.8f); yield return Snap("unlocks-garage-bike-locked"); ShowGarageLocked(car.Id); yield return new WaitForSecondsRealtime(.8f); yield return Snap("unlocks-garage-car-locked"); flow.CloseGarage(); yield return null;
            for (int i = 0; i < 19; i++) PoliceProgress.SpeederCaught(1); Note($"19 catches: bike earned {PoliceProgress.BikeEarned}, text '{VehicleUnlocks.LockedTextFor(bike)}'"); PoliceProgress.SpeederCaught(1);
            Note($"20 catches: bike earned {PoliceProgress.BikeEarned}; pending panels {string.Join(",", UnlockNotice.Pending.Select(n => n.id))}");
            yield return new WaitForSecondsRealtime(1.5f); yield return Snap("unlocks-panel-bike"); Note($"panel open: {Menus.UnlockOpen} showing {UnlockNotice.Showing?.vehicle}");
            CloseUnlockPanel(); yield return new WaitForSecondsRealtime(.5f);
            Note($"patrol car earned before the escape: {PoliceProgress.CarEarned}");
            yield return RunEscapeGoal();
            Note($"patrol car earned after an escape at heat 3 on Normal: {PoliceProgress.CarEarned}; pending panels {string.Join(",", UnlockNotice.Pending.Select(n => n.id))}");
            yield return new WaitForSecondsRealtime(1.5f); yield return Snap("unlocks-panel-car"); CloseUnlockPanel(); yield return new WaitForSecondsRealtime(.5f);
            Note($"after both: in the garage list {string.Join(", ", race.PlayerVehicles.Select(v => v.Id))}; locked {string.Join(",", VehicleProfile.All.Where(v => VehicleUnlocks.Locked(v)).Select(v => v.Id))}");
            yield return GoMountainOrStay();
            yield return StartRace("policebike", new[] { "original", "moto" }, 1); Note($"Race Setup race: player vehicle {race.vehicle.GetComponent<VehicleConfiguration>().profileId}, rivals {string.Join(",", race.Racers.Skip(1).Select(r => r.Car.GetComponent<VehicleConfiguration>().profileId))}");
            yield return new WaitForSecondsRealtime(2); yield return Snap("unlocks-bike-in-a-race"); var b = race.vehicle; Note($"bike on its wheels: grounded wheels {b.GroundedWheels}, up {b.transform.up.y:F2}");
            flow.Pause(); yield return null; flow.QuitRace(); yield return new WaitForSecondsRealtime(1);
        }
        IEnumerator GoMountainOrStay() { Bind(); yield break; }
        IEnumerator RunEscapeGoal()
        {
            // the escape goal needs a Getaway runner at heat 3+ on Normal: a solo Getaway, heat raised with the chase's own BumpHeat, then the escape meter filled
            SplitScreen.P1Device = UnityEngine.InputSystem.Keyboard.current; SplitScreen.Solo = true; SplitScreen.P2AiRunner = false; SplitScreen.P2Ai = true; SplitScreen.Mode = SplitScreen.Kind.Police; SplitScreen.PoliceGame = SplitScreen.Game.Getaway; SplitScreen.PoliceDifficulty = 1; SplitScreen.PoliceMinutes = 5; SplitScreen.Course = 0; SplitScreen.P1Vehicle = "original";
            flow.StartSplit(); float t0 = Time.realtimeSinceStartup; yield return null;
            while (Time.realtimeSinceStartup - t0 < 150) { Bind(); if (flow && race && flow.Started && !LoadingScreen.Holding && flow.State == RaceFlow.Stage.Racing && GetawayChase.Current != null && GetawayChase.Current.Runners.Count > 0) break; yield return null; }
            var g = GetawayChase.Current; yield return new WaitForSecondsRealtime(4); var bump = typeof(GetawayChase).GetMethod("BumpHeat"); bump.Invoke(g, new object[] { "test" }); bump.Invoke(g, new object[] { "test" });
            var r = g.Runners[0]; r.bust = 0; Note($"escape at heat {g.Heat} on difficulty {g.Difficulty}"); typeof(GetawayChase).GetMethod("Escaped", NonPublic).Invoke(g, new object[] { r, false }); yield return new WaitForSecondsRealtime(1.5f);
            Note($"runner escaped {r.escaped}, outcome {r.outcome}"); flow.QuitSplit(false); yield return new WaitForSecondsRealtime(3); Bind();
        }
    }
}
