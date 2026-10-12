using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Racer
{
    // 0.111 evidence only, on the 0.106 check (same switches: -p106check <dir> -racerTestSave <dir>; never active without both):
    //   -p106kind stats111  Part A by controller, in MY CAMPAIGN and the FRESH RUN: an event page (the stat card beside the vehicle, stepped left / right),
    //                       a championship page, the vehicle pick, the Shop; one upgrade bought (test money) and its gold marks on the event page
    //   -p106kind smash111  Part B: Fence Line Smash by controller (moto): grounded and airborne hits, a reset in the middle, time runs out at 8; then a car
    //                       smashing all 14 before the time; Free Roam's smash attempt with a reset. The vehicle is carried through each prop (a test shortcut)
    //   -p106kind prize111  Part C: the fresh run's Chapter 3 final (its prize taken back in the test copy, 1 lap, slower rivals: test shortcuts): results,
    //                       the congratulations, the fresh garage; MY CAMPAIGN unchanged
    //   -p106kind cups111   Part D: championships won (earlier rounds filled in and 1-lap rounds: test shortcuts): the Woodland Cup in the fresh run (no leak),
    //                       then MY CAMPAIGN (a test copy with the Woodland Cup already won: granted on load), the Street, Backyard and Grand
    public sealed partial class Report106PlayerCheck
    {
        string Owns => string.Join(",", Campaign.Current.owned);
        IEnumerator ToCampaign() { yield return FocusRow("campaign"); yield return Press(GamepadButton.South); yield return new WaitForSecondsRealtime(1.2f); }
        IEnumerator ToSlot(bool fresh)
        {
            if (Campaign.Fresh != fresh) yield return Choose("campaign-slot"); yield return new WaitForSecondsRealtime(.6f);
            if (Page.EndsWith("modal")) { Note($"  on switching: '{Texts("CAMPAIGN UPDATED")}' '{Texts("Prize for")}'"); yield return Snap("note-" + (fresh ? "fresh" : "main") + "-" + Screen.width); yield return Press(GamepadButton.South); yield return new WaitForSecondsRealtime(.8f); }
            Note($"slot: {Campaign.SlotName}; title '{Title}'");
        }
        IEnumerator FocusOn(string id) { var b = ActiveButton(id); if (b) { UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(b.gameObject); yield return null; } }
        string StatCardText => Texts("Your vehicle:") + " || card: " + Texts(VehicleProfile.All.Select(p => p.Name + "   ").ToArray());
        string Bars(string id) { var cell = Resources.FindObjectsOfTypeAll<RectTransform>().FirstOrDefault(r => r && r.gameObject.activeInHierarchy && r.name == "Stats " + id); if (!cell) return "(no stat card for " + id + ")";
            return string.Join(", ", cell.GetComponentsInChildren<RectTransform>().Where(r => r.name == "Fill" || r.name == "Upgraded").Select(r => (r.name == "Upgraded" ? "+gold " : "") + r.anchorMax.x.ToString("0.00"))) + $"; card {cell.rect.width:0}x{cell.rect.height:0}, screen {Screen.width}x{Screen.height}"; }
        string CardsLayout => string.Join("; ", Resources.FindObjectsOfTypeAll<RectTransform>().Where(r => r && r.gameObject.activeInHierarchy && (r.name.StartsWith("Preview ") || r.name.StartsWith("Stats "))).Select(r => { var w = new Vector3[4]; r.GetWorldCorners(w); return $"{r.name} x {w[0].x:0}-{w[2].x:0} y {w[0].y:0}-{w[2].y:0}"; }));

        // ---------------------------------------------------------------- Part A
        IEnumerator Stats111()
        {
            Conditions(); string tag = Screen.width + "x" + Screen.height; yield return PanelLoop("stats111-start");
            yield return ToCampaign();
            foreach (bool fresh in new[] { false, true })
            {
                yield return ToSlot(fresh); string slot = fresh ? "fresh" : "main";
                yield return Choose("chapter-1"); yield return Choose("cev-c1-first-lap");
                Note($"[{slot}] event page '{Page}': {RowText("event-vehicle")}; bars of {campaignVehicleShown}: {Bars(campaignVehicleShown)}"); Note($"   layout: {CardsLayout}"); yield return Snap($"A-{slot}-event-{tag}");
                yield return FocusOn("event-vehicle"); yield return Press(GamepadButton.DpadRight); yield return new WaitForSecondsRealtime(.6f);
                Note($"[{slot}] stepped right: {RowText("event-vehicle")}; bars of {campaignVehicleShown}: {Bars(campaignVehicleShown)}"); yield return Snap($"A-{slot}-event-stepped-{tag}");
                yield return Choose("event-vehicle"); Note($"[{slot}] A: page '{Page}' (the vehicle pick); stat block shown {Menu.ShownStats.Length} bars [{string.Join(", ", Menu.ShownStats.Select(f => f.ToString("0.00")))}]"); yield return Snap($"A-{slot}-pick-{tag}"); yield return Back();
                yield return Back();
                var cup = CampaignData.Cups.First(c => Campaign.CupOpen(c) && Campaign.ActiveCup(c) == null); yield return Choose("chapter-" + cup.AfterChapter); yield return Choose("ccup-" + cup.Id);
                Note($"[{slot}] championship '{cup.Id}' page '{Page}': {RowText("event-vehicle")}; prize line '{Texts("Prize: ")}'; bars {Bars(campaignVehicleShown)}"); Note($"   layout: {CardsLayout}"); yield return Snap($"A-{slot}-cup-{tag}"); yield return Back();
                yield return Choose("shop"); Note($"[{slot}] Shop '{Title}': {RowText("shop-vehicle")}; bars [{string.Join(", ", Menu.ShownStats.Select(f => f.ToString("0.00")))}]"); yield return Snap($"A-{slot}-shop-{tag}"); yield return Back();
                if (fresh || Screen.width != 1920) continue;
                // one upgrade bought by controller (money put in the test copy: a test shortcut) on the Needle 600, then its gold marks on the event page
                Campaign.Current.money += 5000; yield return Choose("shop"); yield return FocusOn("shop-vehicle");
                for (int i = 0; i < 16 && !RowText("shop-vehicle").StartsWith("Needle 600 "); i++) yield return Press(GamepadButton.DpadRight);
                yield return Choose("upgrade-0"); yield return FocusRow("confirm", 3); yield return Press(GamepadButton.South); yield return new WaitForSecondsRealtime(1);
                Note($"[{slot}] bought: {Campaign.UpgradeSummary("moto")}; Shop bars [{string.Join(", ", Menu.ShownStats.Select(f => f.ToString("0.00")))}]"); yield return Snap($"A-{slot}-shop-upgraded"); yield return Back();
                yield return Choose("chapter-1"); yield return Choose("cev-c1-first-lap"); yield return FocusOn("event-vehicle");
                for (int i = 0; i < 12 && !RowText("event-vehicle").Contains("Needle 600"); i++) yield return Press(GamepadButton.DpadRight);
                Note($"[{slot}] event page on the upgraded Needle 600: {RowText("event-vehicle")}; bars {Bars("moto")}; card heading '{Texts("Needle 600   ")}'"); yield return Snap($"A-{slot}-event-upgraded"); yield return Back();
            }
            yield return Back();
        }
        string campaignVehicleShown { get { var cell = Resources.FindObjectsOfTypeAll<RectTransform>().FirstOrDefault(r => r && r.gameObject.activeInHierarchy && r.name.StartsWith("Stats ")); return cell ? cell.name.Substring(6) : "(none)"; } }

        // ---------------------------------------------------------------- Part B
        BreakableProp[] SmashProps(out ActivitySite site) { site = flow.Activities.Sites.FirstOrDefault(s => s.id == "smash-01"); return site ? site.props.Where(p => p).ToArray() : new BreakableProp[0]; }
        // carried through the prop at 11 m/s from 5 m away: on the ground, or `lift` m up (in the air)
        IEnumerator Through(BreakableProp prop, ActivitySite site, float lift)
        {
            // across the panel (its sensor box's thin side), from the road side
            var car = race.vehicle; var box = prop.GetComponent<BoxCollider>(); var size = Vector3.Scale(box.size, prop.transform.lossyScale);
            var dir = Flat(size.x < size.z ? prop.transform.right : prop.transform.forward).normalized; if (Vector3.Dot(dir, Flat(prop.transform.position - site.transform.position)) < 0) dir = -dir;
            var start = prop.transform.position - dir * 5;
            var rot = Quaternion.LookRotation(dir); var pos = start; pos.y = TopAt(start) + .7f + lift;
            car.Body.position = pos; car.Body.rotation = rot; car.transform.SetPositionAndRotation(pos, rot); car.Body.linearVelocity = dir * 11 + (lift > 0 ? Vector3.up * 1.5f : Vector3.zero); car.Body.angularVelocity = Vector3.zero; car.GetComponent<VehicleRespawn>()?.CancelRecovery(); Physics.SyncTransforms();
            float t0 = Time.time; while (Time.time - t0 < 1.1f && !prop.IsBroken) yield return new WaitForFixedUpdate();
            Note($"    through {prop.name} at {V(prop.transform.position)} ({(lift > 0 ? "airborne, wheels on the ground: " + car.GroundedWheels : "on the ground")}): broken {prop.IsBroken}; counted {flow.Activities.SmashCount}, live {CampaignRun.LiveCount}");
        }
        IEnumerator StartSmash(string vehicle)
        {
            yield return Choose("chapter-3"); yield return Choose("cev-c3-fence-smash"); Note($"  event page: '{Texts("Smash as many")}' / '{Texts("14 to smash")}'");
            yield return FocusOn("event-vehicle"); for (int i = 0; i < 12 && !RowText("event-vehicle").Contains("Vehicle:   " + VehicleProfile.Find(vehicle).Name + " "); i++) yield return Press(GamepadButton.DpadRight);
            Note($"  {RowText("event-vehicle")}"); yield return Snap("B-event-page-" + vehicle);
            yield return Choose("start-event"); yield return new WaitForSecondsRealtime(2); yield return WaitRacing(); yield return new WaitForSecondsRealtime(.5f);
            Note($"  running: {flow.State}; attempt active {flow.Activities.AttemptActive}; area {flow.Activities.SmashArea}; panel '{CampaignEventUi.Current?.PanelText}'");
        }
        IEnumerator Smash111()
        {
            Conditions(); yield return PanelLoop("smash111-start"); yield return ToCampaign(); Note($"slot {Campaign.SlotName}");
            // run 1: the moto, 8 props (2 of them in the air), a reset after the 5th, then the time runs out
            yield return StartSmash("moto"); var props = SmashProps(out var site); Note($"site {site?.id}: {props.Length} props listed; the event's targets {string.Join("/", CampaignRun.Active.Targets)}");
            yield return Snap("B-markers-start");
            { var line = Flat(props[props.Length - 1].transform.position - props[0].transform.position).normalized; var perp = Vector3.Cross(Vector3.up, line); if (Vector3.Dot(perp, race.vehicle.Body.position - site.transform.position) < 0) perp = -perp;
              yield return Free("B-markers-view", site.transform.position + perp * 13 + Vector3.up * 4.5f, site.transform.position + Vector3.up * .8f, 55); Note("  the marked props seen from the road side (a fixed camera)"); }
            for (int i = 0; i < 5; i++) yield return Through(props[i], site, i == 3 || i == 4 ? 1.6f : 0);
            yield return Snap("B-panel-after-5");
            race.vehicle.GetComponent<VehicleRespawn>().ResetVehicle(); yield return new WaitForSecondsRealtime(1.5f);
            Note($"  RESET (the reset button's action): attempt active {flow.Activities.AttemptActive}; counted {flow.Activities.SmashCount}; live {CampaignRun.LiveCount}; HUD '{CampaignEventUi.Current?.PanelText}'");
            for (int i = 5; i < 8; i++) yield return Through(props[i], site, 0);
            var car = race.vehicle; car.Body.position = site.transform.position + Vector3.right * 30 + Vector3.up * 2; car.Body.linearVelocity = Vector3.zero; Physics.SyncTransforms();
            Note($"  8 smashed; waiting for the time limit ({CampaignRun.Active.TimeLimit} s)"); yield return Snap("B-panel-8");
            float t0 = Time.realtimeSinceStartup; while (Time.realtimeSinceStartup - t0 < 90 && flow.State != RaceFlow.Stage.Results) yield return null; yield return new WaitForSecondsRealtime(1.5f);
            var o = CampaignRun.Last; Note($"  result: state {flow.State}; recorded {o?.Score} (medal {o?.Medal}: {MedalUi.Name(o?.Medal ?? 0)}), passed {o?.Passed}; attempts [{string.Join("; ", CampaignRun.Attempts.Select(a => $"{a.Value} scored {a.Scored} medal {a.Medal} {a.Reason}"))}]");
            yield return Snap("B-results-8");
            // run 2: a car (Street Classic), every prop down before the time
            yield return Choose("campaign-continue"); yield return WaitMenu(); yield return PanelLoop("smash111-after-1");
            yield return StartSmash("original"); props = SmashProps(out site); float started = (float)race.Progress.RaceTime(race.Clock);
            for (int i = 0; i < props.Length && flow.State == RaceFlow.Stage.Racing; i++) yield return Through(props[i], site, i % 3 == 2 ? 1.6f : 0);
            float allDown = (float)race.Progress.RaceTime(race.Clock); t0 = Time.realtimeSinceStartup; while (Time.realtimeSinceStartup - t0 < 20 && flow.State != RaceFlow.Stage.Results) yield return null; yield return new WaitForSecondsRealtime(1.5f);
            o = CampaignRun.Last; Note($"  all {props.Length} down at {allDown:0.0} s of {CampaignRun.Active?.TimeLimit} s; state {flow.State} at {(float)race.Progress.RaceTime(race.Clock):0.0} s; recorded {o?.Score} ({MedalUi.Name(o?.Medal ?? 0)})");
            yield return Snap("B-results-all");
            yield return Choose("campaign-continue"); yield return WaitMenu(); yield return PanelLoop("smash111-after-2"); yield return Back();
            // Free Roam's smash site: a timed attempt, two hits, a reset, one more
            yield return ToFreeRoam("atv"); var acts = flow.Activities; props = SmashProps(out site);
            while (acts.Selected != site) acts.Cycle(); acts.BeginAttempt(); Note($"Free Roam: attempt at '{site.title}' ({site.Seconds} s): '{acts.Hud}'");
            yield return Through(props[0], site, 0); yield return Through(props[1], site, 1.6f);
            race.vehicle.GetComponent<VehicleRespawn>().ResetVehicle(); yield return new WaitForSecondsRealtime(1f); Note($"  RESET: attempt active {acts.AttemptActive}; counted {acts.SmashCount}; HUD '{acts.Hud.Replace("\n", " / ")}'");
            yield return Through(props[2], site, 0); Note($"  HUD '{acts.Hud.Replace("\n", " / ")}'"); yield return Snap("B-free-roam");
        }

        // ---------------------------------------------------------------- Part C
        string Hash(Campaign.State s) => JsonUtility.ToJson(s).GetHashCode().ToString("x");
        IEnumerator Prize111()
        {
            Conditions(); string dir = flow.Save.DirectoryPath, main = Path.Combine(dir, Campaign.File); string mainHash = Hash(main);
            yield return PanelLoop("prize111-start"); yield return ToCampaign(); yield return ToSlot(true);
            // the test copy back at the Chapter 3 final: its prize and the final's pass taken back (a test shortcut)
            Campaign.Current.owned.Remove("scrambler"); var r = Campaign.ResultOf("c3-backyard-final"); r.passed = false; r.won = false; Campaign.Save();
            var e = CampaignData.Find("c3-backyard-final"); e.Laps = 1; Note($"fresh run before: {Slot}; final laps set to 1 (a test shortcut)");
            yield return Choose("chapter-3"); yield return Choose("cev-c3-backyard-final"); Note($"  event page: {RowText("event-vehicle")}; '{Texts("Prize: ")}'"); yield return Snap("C-final-page");
            yield return Choose("start-event"); yield return new WaitForSecondsRealtime(2); yield return WaitRacing();
            foreach (var d in race.Drivers) if (d && d.Car && d.Car != race.vehicle) { d.enabled = false; d.Car.Body.linearVelocity = Vector3.zero; d.Car.Body.isKinematic = true; } // the rivals held at the start (a test shortcut)
            Pilot(race.vehicle, race.Racers[0]); yield return Drive(400, () => flow.State == RaceFlow.Stage.Results); yield return new WaitForSecondsRealtime(2);
            var o = CampaignRun.Last; Note($"result: {o?.Headline}; passed {o?.Passed}; prize {o?.PrizeWon}; unlocked [{string.Join("; ", o?.Unlocked ?? new System.Collections.Generic.List<string>())}]; strip '{Texts("PRIZE WON")}'");
            yield return Snap("C-results");
            yield return Choose("campaign-continue"); yield return new WaitForSecondsRealtime(1.5f); yield return PanelLoop("prize111-after"); yield return WaitMenu();
            Note($"fresh run after: {Slot}; seen [{string.Join(",", flow.Save.Settings.unlocksSeen ?? new string[0])}]");
            yield return Choose("shop"); yield return FocusOn("shop-vehicle"); for (int i = 0; i < 16 && !RowText("shop-vehicle").StartsWith("Ridge Scrambler "); i++) yield return Press(GamepadButton.DpadRight);
            Note($"  fresh Shop/garage: {RowText("shop-vehicle")}; '{Texts("Ridge Scrambler /")}'"); yield return Snap("C-fresh-garage"); yield return Back();
            Note($"MY CAMPAIGN file unchanged: {Hash(main) == mainHash}");
            yield return ToSlot(false); Note($"  {Slot}"); yield return PanelLoop("prize111-main", 2); yield return Back();
        }

        // ---------------------------------------------------------------- Part D
        IEnumerator WinCup(string id, string tag)
        {
            var c = CampaignData.FindCup(id); foreach (var round in c.Rounds) round.Laps = 1;
            yield return Choose("chapter-" + c.AfterChapter); yield return Choose("ccup-" + id);
            Note($"  [{tag}] '{c.Name}' page: {RowText("event-vehicle")}; '{Texts("Prize: ")}'; prize card '{Texts("PRIZE: ")}'"); yield return Snap($"D-{tag}-{id}-page");
            yield return Choose("cup-start"); yield return new WaitForSecondsRealtime(1);
            var progress = Campaign.ActiveCup(c); for (int i = 1; i < c.Rounds.Length; i++) progress.races.Add(new Campaign.CupRace { places = new[] { 1, 0, 0, 0, 0, 0 } }); // the earlier rounds filled in (a test shortcut)
            yield return WaitRacing(); Note($"  [{tag}] racing the last round ({progress.races.Count + 1} of {c.Rounds.Length}, 1 lap) on {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name} with {race.vehicle.GetComponent<VehicleConfiguration>().profileId}");
            Pilot(race.vehicle, race.Racers[0]); yield return Drive(400, () => flow.State == RaceFlow.Stage.Results); yield return new WaitForSecondsRealtime(2);
            var o = CampaignRun.Last; Note($"  [{tag}] final: {o?.CupPosition} overall, won {o?.Won}; prize {o?.PrizeWon}; unlocked [{string.Join("; ", o?.Unlocked ?? new System.Collections.Generic.List<string>())}]; strip '{Texts("PRIZE WON")}'");
            yield return Snap($"D-{tag}-{id}-results");
            yield return Choose("campaign-continue"); yield return new WaitForSecondsRealtime(1.5f); yield return PanelLoop($"cups111-{tag}-{id}"); yield return WaitMenu();
        }
        IEnumerator ShowInShop(string id, string tag)
        {
            yield return Choose("shop"); yield return FocusOn("shop-vehicle"); var p = VehicleProfile.Find(id);
            for (int i = 0; i < 18 && !RowText("shop-vehicle").StartsWith(p.Name + " "); i++) yield return Press(GamepadButton.DpadRight);
            Note($"  [{tag}] garage/Shop: {RowText("shop-vehicle")}; '{Texts(p.Name + " /")}'; bars [{string.Join(", ", Menu.ShownStats.Select(f => f.ToString("0.00")))}] vs its base {p.CupBase} [{string.Join(", ", Enumerable.Range(0, 5).Select(k => RaceMenus.StatFraction(k, VehicleProfile.Find(p.CupBase)).ToString("0.00")))}]");
            yield return Snap($"D-{tag}-{id}-garage"); yield return Back();
        }
        string RaceLocks => string.Join(",", VehicleProfile.All.Where(p => p.CupEdition).Select(p => p.Id + (Campaign.VehicleLocked(p) ? ":locked" : ":open")));
        IEnumerator Cups111()
        {
            Conditions(); string dir = flow.Save.DirectoryPath, main = Path.Combine(dir, Campaign.File); string mainHash = Hash(main);
            Note($"start: slot {Campaign.SlotName}; Race: {RaceLocks}; load notice '{Campaign.LoadNotice}'"); yield return PanelLoop("cups111-start");
            yield return ToCampaign(); yield return ToSlot(true); Note($"  {Slot}");
            yield return WinCup("cup-woodland", "fresh"); yield return ShowInShop("driftercup", "fresh");
            Note($"fresh: {Slot}; MY CAMPAIGN file unchanged {Hash(main) == mainHash}; Race: {RaceLocks}");
            yield return ToSlot(false); Note($"MY CAMPAIGN: {Slot}; Race: {RaceLocks}");
            yield return Back(); yield return PanelLoop("cups111-main-retro"); yield return ToCampaign();
            foreach (var id in new[] { "cup-street", "cup-backyard", "cup-grand" }) yield return WinCup(id, "main");
            yield return ShowInShop("motochampion", "main"); yield return ShowInShop("driftercup", "main");
            Note($"end: {Slot}; Race: {RaceLocks}"); yield return Back();
            // the Race garage: the Cup editions won in MY CAMPAIGN
            yield return FocusRow("garage"); yield return Press(GamepadButton.South); yield return new WaitForSecondsRealtime(1);
            flow.SelectVehicle("motochampion"); Menu.Show(); yield return new WaitForSecondsRealtime(1); Note($"Race garage: vehicle {flow.PlayerVehicleId}; '{Texts("Needle 600 Champion")}'"); yield return Snap("D-race-garage-champion");
            yield return Back();
        }
    }
}
