using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Racer
{
    // 0.112 evidence only, on the 0.106 check (same switches: -p106check <dir> -racerTestSave <dir>; never active without both):
    //   -p106kind pace112   Part A: the AI field alone in campaign races: -p112events "c1-first-lap,c4-downhill" -p112runs 5; the player's vehicle is
    //                       parked out of the race (no colliders, kinematic) and the finish grace lifted so every rival is timed to the line (checks only).
    //                       -p112set "c1-first-lap=pace:1.1/diff:1/rivals:tourer+moto+atv;..." tries other values without a build (checks only).
    //                       One line per rival per run: vehicle, time, resets, missed gates, shortcuts taken.
    public sealed partial class Report106PlayerCheck
    {
        static void Override(CampaignEvent e, string spec)
        {
            foreach (var part in spec.Split('/'))
            {
                var kv = part.Split(':'); if (kv.Length != 2) continue;
                if (kv[0] == "pace") e.Pace = F(kv[1]);
                else if (kv[0] == "diff") e.Difficulty = int.Parse(kv[1]);
                else if (kv[0] == "rivals") e.Rivals = kv[1].Split('+');
                else if (kv[0] == "laps") e.Laps = int.Parse(kv[1]);
            }
        }
        IEnumerator Pace112()
        {
            Campaign.Testing = true; DriverVariation.Seed = System.Environment.TickCount; CampaignRun.NoRivalShortcuts = Arg("-p112nocuts", "0") == "1"; if (Arg("-p112cuts", null) != null) CampaignRun.RivalShortcutsUnvalidated = Arg("-p112cuts", "").Split('|'); Note("chapter 4 rival shortcuts " + (CampaignRun.NoRivalShortcuts ? "off (check)" : "on: validated + " + string.Join(", ", CampaignRun.RivalShortcutsUnvalidated)));
            var events = Arg("-p112events", "c1-first-lap").Split(','); int runs = int.Parse(Arg("-p112runs", "5"));
            foreach (var spec in Arg("-p112set", "").Split(';').Where(x => x.Contains("=")))
            { var e = CampaignData.Find(spec.Split('=')[0]); if (e != null) { Override(e, spec.Split('=')[1]); Note($"override {e.Id}: {spec.Split('=')[1]}"); } }
            foreach (var id in events)
            {
                var e = CampaignData.Find(id); if (e == null) { Note("no event " + id); continue; }
                Note($"== {e.Id} '{e.Name}' course {RacePlaylists.Scenes[e.Course]} laps {e.Laps} {e.Conditions} difficulty {e.Difficulty} pace {CampaignRunPace(e):0.000} rivals {string.Join(",", e.Rivals)}");
                // every run a fresh start through the loading screen, as a player's first attempt (a Retry from the results gave the rivals extra resets and other times)
                for (int run = 1; run <= runs; run++)
                {
                    Conditions(); flow.StartCampaignEvent(e, "atv");
                    yield return new WaitForSecondsRealtime(1); yield return WaitRacing(); Bind();
                    var me = race.vehicle; me.Body.isKinematic = true; foreach (var c in me.GetComponentsInChildren<Collider>()) c.enabled = false;
                    var vi = me.GetComponent<VehicleInput>(); if (vi) vi.enabled = false; var respawn = me.GetComponent<VehicleRespawn>(); if (respawn) respawn.enabled = false;
                    race.finishGraceSeconds = 100000;
                    var ai = race.Racers.Where(r => r.IsAi).ToList(); var reset0 = ai.ToDictionary(r => r, r => r.Recoveries); var resetAt = ai.ToDictionary(r => r, r => new List<string>()); var shortcuts = ai.ToDictionary(r => r, r => new List<string>());
                    var on = ai.ToDictionary(r => r, r => (WoodlandRoute)null);
                    Time.timeScale = 6; float t0 = Time.realtimeSinceStartup;
                    while (Time.realtimeSinceStartup - t0 < 1500 && flow.State == RaceFlow.Stage.Racing && !ai.All(r => r.Progress.Finished || r.Dnf))
                    {
                        AudioListener.volume = 0; yield return null; if (Time.timeScale == 0) Time.timeScale = 6;
                        foreach (var r in ai) if (r.Recoveries - reset0[r] > resetAt[r].Count) resetAt[r].Add($"{V(r.Car.Body.position)} road {r.RoadPosition:F0}{(r.Branch.Route ? " on " + r.Branch.Route.title : "")}");
                        foreach (var r in ai) if (r.Branch.Route != on[r]) { on[r] = r.Branch.Route; if (on[r]) shortcuts[r].Add(on[r].title + " lap " + (r.Progress.CompletedLaps + 1)); }
                    }
                    Time.timeScale = 1;
                    foreach (var r in ai.OrderBy(r => r.Progress.Finished ? r.ClassifiedTime(race.Clock) : 1e9))
                        Note(string.Format(CultureInfo.InvariantCulture, "  run {0} {1,-10} {2} resets {3} penalty {4} shortcuts [{5}]", run, r.Car.GetComponent<VehicleConfiguration>().profileId,
                            r.Progress.Finished ? r.ClassifiedTime(race.Clock).ToString("0.0", CultureInfo.InvariantCulture) : (r.Dnf ? "DNF" : "unfinished"), r.Recoveries - reset0[r], r.Progress.PenaltySeconds, string.Join("; ", shortcuts[r])) + " | last decision: " + r.Car.GetComponent<RoadDriver>()?.Shortcuts.Diagnostic + (resetAt[r].Count > 0 ? " | resets at " + string.Join("; ", resetAt[r]) : ""));
                    race.finishGraceSeconds = 0; float w = Time.realtimeSinceStartup; while (flow.State != RaceFlow.Stage.Results && Time.realtimeSinceStartup - w < 30) yield return null;
                    flow.QuitRace(); yield return WaitMenu();
                }
            }
        }
        // ---------------------------------------------------------------- Part D: the trailer jump (Street Loop Forward)
        //   -p106kind jump112   three runs over the jump at 78 / 87 / 96 mph in the ATV and the Street Classic (started at speed 30 m before the ramp on the
        //                       jump's centre line, held at that speed by the pad): take-off, landing, distance, air time, contacts; then a 1-lap race with
        //                       rivals: who went over the jump (height above the road there); then shots from Dan's two report positions and around it
        IEnumerator ToStreetLoop()
        {
            Campaign.Testing = true;
            for (int attempt = 0; attempt < 4 && UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "StreetLoopGreybox"; attempt++)
            {
                if (flow.State == RaceFlow.Stage.Title) flow.EnterMenuAfterTitle();
                flow.OpenCourses(); yield return null; flow.SelectCourse(false, false);
                float t1 = Time.realtimeSinceStartup; while (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "StreetLoopGreybox" && Time.realtimeSinceStartup - t1 < 40) yield return null;
            }
            yield return WaitFlow(); Conditions();
        }
        IEnumerator Jump112()
        {
            yield return ToStreetLoop(); var frame = GameObject.Find("Phase 4 - Connector Jump").transform;
            var takeoff = frame.GetComponentsInChildren<MeshCollider>().First(c => c.name.StartsWith("Takeoff -"));
            Note($"jump: {takeoff.name}, collider mesh {takeoff.sharedMesh.name} ({takeoff.sharedMesh.vertexCount} vertices, bounds {takeoff.bounds.center:F2} size {takeoff.bounds.size:F2}); dressing {(takeoff.transform.Find("Trailer dressing") ? "yes" : "no")}; solid colliders under the jump {frame.GetComponentsInChildren<Collider>(true).Count(c => !c.isTrigger)}");
            foreach (var vehicle in new[] { "atv", "original" })
            {
                flow.OpenGarage(); flow.SelectVehicle(vehicle); flow.CloseGarage(); race.opponents = false; race.traffic = false; race.laps = 1;
                flow.StartRace(); yield return WaitRacing(); var car = race.vehicle; BindPad();
                var contacts = car.gameObject.AddComponent<Report106Contacts>(); contacts.who = vehicle;
                foreach (float mph in new[] { 78f, 87f, 96f })
                {
                    float speed = mph * .44704f; contacts.list.Clear();
                    var start = frame.TransformPoint(new Vector3(-1.5f, 0, -30)); Put(car, start, Yaw(frame.forward), speed);
                    float t0 = Time.time; bool air = false; Vector3 off = default, on = default; float offT = 0, top = -999, lipSpeed = 0;
                    while (Time.time - t0 < 12)
                    {
                        var aim = frame.TransformPoint(new Vector3(-1.5f, 0, frame.InverseTransformPoint(car.Body.position).z + 25)); Drive(car, aim, speed, steerGain: 40);
                        yield return new WaitForFixedUpdate();
                        if (!air && car.GroundedWheels == 0 && frame.InverseTransformPoint(car.Body.position).z > 20) { air = true; off = car.Body.position; offT = Time.time; lipSpeed = car.Body.linearVelocity.magnitude; }
                        if (air) top = Mathf.Max(top, car.Body.position.y);
                        if (air && car.GroundedWheels >= 2 && Time.time - offT > .2f) { on = car.Body.position; break; }
                    }
                    Release();
                    var touches = contacts.list.Where(t => !t.Contains("Takeoff") && !t.Contains("Ground")).Distinct().ToList();
                    Note($"  {vehicle} {mph:0} mph: take-off at z {frame.InverseTransformPoint(off).z:F1} ({lipSpeed * 2.237f:0} mph), landing z {frame.InverseTransformPoint(on).z:F1}, distance {Vector3.Distance(Flat(off), Flat(on)):F1} m, air {(on == default ? -1 : Time.time - offT):F2} s, top {top - off.y:F2} m above the take-off, lateral {frame.InverseTransformPoint(on).x:F1}; other contacts {touches.Count} {string.Join(" | ", touches.Take(4))}");
                    yield return new WaitForSeconds(.5f);
                }
                Destroy(contacts); flow.QuitRace(); yield return WaitMenu();
            }
            // the rivals in a 1-lap race: who went over the jump
            flow.OpenGarage(); flow.SelectVehicle("atv"); flow.CloseGarage(); race.opponents = true; race.traffic = false; race.laps = 1; race.difficulty = 1;
            flow.StartRace(); yield return WaitRacing(); Pilot(race.vehicle, race.Racers[0]);
            var over = race.Racers.ToDictionary(r => r, r => -99f);
            Time.timeScale = 5; float r0 = Time.realtimeSinceStartup;
            while (flow.State == RaceFlow.Stage.Racing && Time.realtimeSinceStartup - r0 < 400)
            {
                AudioListener.volume = 0; yield return null; if (Time.timeScale == 0) Time.timeScale = 5;
                foreach (var r in race.Racers) { var l = frame.InverseTransformPoint(r.Car.Body.position); if (l.z > 0 && l.z < 60 && l.x > -6 && l.x < 3) over[r] = Mathf.Max(over[r], l.y); }
            }
            Time.timeScale = 1;
            Note($"race: finished {flow.State == RaceFlow.Stage.Results}; " + string.Join("; ", race.Racers.Select(r => $"{r.Name} ({r.Car.GetComponent<VehicleConfiguration>().profileId}{(r.IsAi ? ", AI" : ", player by road AI")}): highest over the jump {(over[r] < -90 ? "not on it" : over[r].ToString("F1") + " m")}, resets {r.Recoveries}, finished {r.Progress.Finished}")));
            flow.QuitRace(); yield return WaitMenu();
            yield return Shots112();
        }
        IEnumerator Shots112()
        {
            foreach (var c in FindObjectsByType<Canvas>(FindObjectsSortMode.None)) c.enabled = false;
            string tag = Arg("-p112tag", "after");
            // Dan's report positions (about the chase camera's view: 6 m behind, 2.6 m up, looking ahead)
            foreach (var (name, pos, yaw, back) in new[] { ("D-sign-dan-view", new Vector3(-637.79f, 7.87f, -149.2f), 353.5f, 6f), ("D-trailer-dan-view", new Vector3(-623.32f, 8.94f, -87.67f), .7f, 6f), ("D-trailer-approach", new Vector3(-626.6f, 7.9f, -175f), .7f, 0f) })
            { var f = Quaternion.Euler(0, yaw, 0) * Vector3.forward; var eye = pos - f * back + Vector3.up * 2.6f; yield return Free($"{name}-{tag}", eye, eye + f * 30 + Vector3.down * 2.5f, 60, 1600, 900); }
            var frame = GameObject.Find("Phase 4 - Connector Jump").transform;
            yield return Free($"D-trailer-side-{tag}", frame.TransformPoint(new Vector3(22, 5, 25)), frame.TransformPoint(new Vector3(-1.5f, 3, 25)), 60, 1600, 900);
            yield return Free($"D-trailer-rear-{tag}", frame.TransformPoint(new Vector3(6, 3, -22)), frame.TransformPoint(new Vector3(-1.5f, 2, 30)), 60, 1600, 900);
            yield return Free($"D-trailer-front-{tag}", frame.TransformPoint(new Vector3(8, 3, 92)), frame.TransformPoint(new Vector3(-1.5f, 2, 30)), 60, 1600, 900);
            var sign = frame.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name.StartsWith("Jump approach sign (") || t.name == "Breakable - Jump approach sign");
            if (sign)
            {
                yield return Free($"D-sign-front-{tag}", sign.position - frame.forward * 9 + Vector3.up * 2.2f, sign.position + Vector3.up * 2.6f, 50, 1600, 900);
                yield return Free($"D-sign-back-{tag}", sign.position + frame.forward * 9 + Vector3.up * 2.2f, sign.position + Vector3.up * 2.6f, 50, 1600, 900);
                Note($"sign '{sign.name}' at {V(sign.position)} heading {sign.eulerAngles.y:F1}, up {sign.up.y:F3}, ground {TopAt(sign.position + Vector3.up * 10):F2}, active {sign.gameObject.activeInHierarchy}, breakable {(sign.GetComponent<BreakableProp>() ? "yes" : "no")}; text '{sign.GetComponentInChildren<TextMesh>(true)?.text.Replace("\n", " / ")}'");
            }
            foreach (var c in FindObjectsByType<Canvas>(FindObjectsSortMode.None)) c.enabled = true;
        }

        // ---------------------------------------------------------------- Part E: buy and select (test save prepared: both slots without the four Shop vehicles)
        //   -p106kind shop112   from the main menu (FRESH RUN active): RACE > Garage: each locked Shop vehicle, its BUY row (or the reason), bought and selected;
        //                       then the campaign Shop, an event page and the vehicle pick in the FRESH RUN; then MY CAMPAIGN: buy one in its Shop, select it on
        //                       an event page and in the pick; the Race garage after
        string Slots => $"{Campaign.SlotName}: money {Campaign.Money(Campaign.Current.money)}, owned [{string.Join(",", Campaign.Current.owned)}]";
        string GarageTitle => Texts(VehicleProfile.All.Select(p => p.Name + " / ").ToArray());
        IEnumerator GarageTo(string id)
        {
            yield return FocusOn("profile-0");
            for (int i = 0; i < 20 && !(RowText("profile-0").StartsWith(VehicleProfile.Find(id).Name + "  ") || RowText("profile-0").StartsWith(VehicleProfile.Find(id).Name + "   ")); i++) yield return Press(GamepadButton.DpadRight);
        }
        IEnumerator StepExact(string vehicle)
        {
            var row = ActiveButton("event-vehicle"); if (!row) yield break; UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(row.gameObject); yield return null;
            string want = "Vehicle:   " + VehicleProfile.Find(vehicle).Name + "   ";
            for (int i = 0; i < 16 && !RowText("event-vehicle").StartsWith(want); i++) yield return Press(GamepadButton.DpadRight);
        }
        IEnumerator Shop112()
        {
            Conditions(); yield return PanelLoop("E-start"); Note("start: " + Slots + $"; Race vehicle {race.vehicle.GetComponent<VehicleConfiguration>().profileId}; page '{Page}'");
            yield return FocusRow("race"); yield return Press(GamepadButton.South); yield return new WaitForSecondsRealtime(1); yield return FocusRow("garage"); yield return Press(GamepadButton.South); yield return new WaitForSecondsRealtime(1);
            Note($"Race garage: page '{Page}', state {flow.State}");
            foreach (var id in new[] { "drifter", "roadster", "skyfin", "tourer" })
            {
                yield return GarageTo(id);
                var buy = ActiveButton("garage-buy");
                Note($"  {id}: title '{GarageTitle}', row '{RowText("profile-0")}', buy row '{RowText("garage-buy")}' (can press: {(buy ? buy.interactable : false)})"); yield return Snap($"E-race-garage-{id}-locked");
                if (buy && buy.interactable)
                {
                    yield return FocusRow("garage-buy", 6); yield return Press(GamepadButton.South); yield return new WaitForSecondsRealtime(.8f);
                    Note($"    confirm: '{Texts("BUY THE")}'"); yield return FocusRow("confirm", 3); yield return Press(GamepadButton.South); yield return new WaitForSecondsRealtime(1.2f);
                    Note($"    bought: title '{GarageTitle}', row '{RowText("profile-0")}', Race vehicle {race.vehicle.GetComponent<VehicleConfiguration>().profileId}, locked {Campaign.VehicleLocked(VehicleProfile.Find(id))}; {Slots}"); yield return Snap($"E-race-garage-{id}-bought");
                }
            }
            yield return Back(); yield return Back(); yield return new WaitForSecondsRealtime(1); Note($"back: page '{Page}'");
            // the campaign (FRESH RUN): Shop, an event page, an event that excludes cars and its vehicle pick
            yield return ToCampaign(); Note($"campaign: {Slots}; page '{Page}'");
            yield return Choose("shop"); yield return FocusOn("shop-vehicle");
            for (int i = 0; i < 16 && !RowText("shop-vehicle").StartsWith("Drifter Twin  ("); i++) yield return Press(GamepadButton.DpadRight);
            Note($"  Shop: '{RowText("shop-vehicle")}', buy row '{RowText("buy")}', status '{Texts("OWNED", "Price")}'"); yield return Snap("E-fresh-shop-drifter"); yield return Back();
            yield return Choose("chapter-2"); yield return Choose("cev-c2-into-woods"); yield return StepExact("drifter"); Note($"  event Into the Woods: {RowText("event-vehicle")}"); yield return Snap("E-fresh-event-drifter");
            yield return StepExact("roadster"); Note($"  event Into the Woods: {RowText("event-vehicle")}"); yield return Back();
            yield return Choose("chapter-3"); yield return Choose("cev-c3-two-wheels"); Note($"  event Two Wheels Only: {RowText("event-vehicle")}");
            yield return Choose("event-vehicle"); yield return FocusOn("pick-vehicle");
            for (int i = 0; i < 16 && !RowText("pick-vehicle").StartsWith("Drifter Twin   "); i++) yield return Press(GamepadButton.DpadRight);
            Note($"  vehicle pick on the Drifter Twin: '{RowText("pick-vehicle")}', use row '{RowText("pick-use")}'"); yield return Snap("E-fresh-pick-drifter");
            for (int i = 0; i < 16 && !RowText("pick-vehicle").StartsWith("Sundown Roadster   "); i++) yield return Press(GamepadButton.DpadRight);
            Note($"  vehicle pick on the Roadster: '{RowText("pick-vehicle")}', use row '{RowText("pick-use")}', '{Texts("Not for this event")}'"); yield return Snap("E-fresh-pick-roadster"); yield return Back(); yield return Back();
            // MY CAMPAIGN: buy the Longroof GT in its Shop, select it on an event page
            yield return ToSlot(false); yield return PanelLoop("E-main-switch"); Note($"main: {Slots}; page '{Page}'");
            yield return Choose("shop"); yield return FocusOn("shop-vehicle");
            for (int i = 0; i < 16 && !RowText("shop-vehicle").StartsWith(VehicleProfile.Find("tourer").Name + " "); i++) yield return Press(GamepadButton.DpadRight);
            Note($"  Shop: '{RowText("shop-vehicle")}', buy row '{RowText("buy")}'");
            yield return Choose("buy"); yield return FocusRow("confirm", 3); yield return Press(GamepadButton.South); yield return new WaitForSecondsRealtime(1.2f);
            Note($"  bought: buy row '{RowText("buy")}'; {Slots}"); yield return Snap("E-main-shop-tourer"); yield return Back();
            yield return Choose("chapter-1"); yield return Choose("cev-c1-first-lap"); yield return StepExact("tourer"); Note($"  event First Lap: {RowText("event-vehicle")}"); yield return Snap("E-main-event-tourer");
            yield return Back(); yield return Back(); yield return new WaitForSecondsRealtime(1); yield return PanelLoop("E-main-back"); Note($"back: page '{Page}'");
            Note("Race garage locked now: [" + string.Join(", ", VehicleProfile.All.Where(p => !p.Reward && Campaign.VehicleLocked(p)).Select(p => p.Id)) + $"] (slot {Campaign.SlotName})");
            yield return FocusRow("garage"); yield return Press(GamepadButton.South); yield return new WaitForSecondsRealtime(1); yield return GarageTo("skyfin");
            Note($"  Race garage on the Skyfin: '{GarageTitle}', Race vehicle {race.vehicle.GetComponent<VehicleConfiguration>().profileId}"); yield return Snap("E-race-garage-after"); yield return Back();
        }
        // ---------------------------------------------------------------- Part F: a campaign race from the menus (test copy of the fresh-run save)
        //   -p106kind race112 -p112event c1-first-lap   CAMPAIGN > chapter > event > START EVENT by controller, the player's vehicle driven by the road AI
        //                       (a stand-in for Dan; checks only); every 15 s the nearest rival's gap; the result, each rival's time and resets
        IEnumerator Race112()
        {
            Conditions(); yield return PanelLoop("F-start"); var e = CampaignData.Find(Arg("-p112event", "c1-first-lap"));
            yield return ToCampaign(); Note($"campaign {Campaign.SlotName}; event {e.Id}: difficulty {e.Difficulty}, rival pace {CampaignRunPace(e):0.00}, rivals {string.Join(",", e.Rivals)}");
            yield return Choose("chapter-" + e.Chapter); yield return Choose("cev-" + e.Id); Note($"  page '{Page}': {RowText("event-vehicle")}");
            yield return Choose("start-event"); yield return new WaitForSecondsRealtime(2); yield return WaitRacing();
            Note($"  racing in {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}: {race.Racers.Count - 1} rivals, player {race.vehicle.GetComponent<VehicleConfiguration>().profileId}");
            Pilot(race.vehicle, race.Racers[0]); var reset0 = race.Racers.ToDictionary(r => r, r => r.Recoveries); var gaps = new List<string>(); float next = 15;
            Time.timeScale = 5; float r0 = Time.realtimeSinceStartup;
            while (flow.State == RaceFlow.Stage.Racing && Time.realtimeSinceStartup - r0 < 600)
            {
                AudioListener.volume = 0; yield return null; if (Time.timeScale == 0) Time.timeScale = 5;
                float t = (float)race.Progress.RaceTime(race.Clock);
                if (t > next && !race.Racers[0].Progress.Finished) { next += 15; var me = race.Racers[0]; float mine = race.RemainingDistance(me); gaps.Add($"{t:0}s {race.Racers.Where(r => r.IsAi).Min(r => race.RemainingDistance(r) - mine):0}m"); }
            }
            Time.timeScale = 1; yield return new WaitForSecondsRealtime(2);
            var p = race.Racers[0]; Note($"  result: state {flow.State}; player {(p.Progress.Finished ? p.ClassifiedTime(race.Clock).ToString("0.0") : "unfinished")} s, place {race.Ordered(true).IndexOf(p) + 1}");
            foreach (var r in race.Racers.Where(r => r.IsAi)) Note($"    {r.Name} ({r.Car.GetComponent<VehicleConfiguration>().profileId}): {(r.Classified ? r.ClassifiedTime(race.Clock).ToString("0.0") + (r.Estimated ? " (estimated)" : "") : "unfinished")} s, resets {r.Recoveries - reset0[r]}, behind the player {(r.Classified && p.Classified ? (r.ClassifiedTime(race.Clock) - p.ClassifiedTime(race.Clock)).ToString("0.0") + " s" : "-")}");
            Note("  nearest rival behind the player (metres, negative = ahead): " + string.Join(", ", gaps));
            yield return Snap("F-results-" + e.Id); yield return PanelLoop("F-after");
        }
        static float CampaignRunPace(CampaignEvent e) => e.Kind != CampaignEventKind.Race ? 1 : Mathf.Clamp(e.Pace, .5f, CampaignRun.RivalPaceCap);
    }
}
