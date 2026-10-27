using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

namespace Racer
{
    // 1.3.0-review3 Parts D and E evidence only (Report106PlayerCheck: -p106check <dir> -racerTestSave <dir>, a copy of Dan's save):
    //   -p106kind comp1300r3   the Stunt chapter with the controller only: the main menu > CAMPAIGN > the run with chapter 5 (Dan's copy) > chapter 5's list,
    //                          the Park Competition's page (B back to the list, A again), started; the player ridden by the park bot; the rivals seen riding
    //                          and tricking, the standings as they change, the HUD; the results with the places; then the list from a fresh chapter-5 save
    //                          (-p1300comp seconds: a shorter round, checks only)
    //   -p106kind closed1300r3 a Stunt Race (3 rivals) on Mountain Loop Forward (-p119course 4) or Reverse (5): each closed shortcut's barricades from the
    //                          driver's view, then the race (the player on the road AI): any rival on a shortcut, stuck or reset by a barricade;
    //                          -p1300normal 1: a normal race there instead: the shortcuts open, no barricade
    public sealed partial class Report106PlayerCheck
    {
        IEnumerator Comp1300r3()
        {
            Conditions(); yield return PanelLoop("comp-start"); yield return ToCampaign(); yield return ToSlot(true);
            string List() => string.Join(", ", CampaignData.InChapter(5).Select((e, i) => $"{i + 1} {e.Name}{(Campaign.Available(e) ? "" : " (locked)")}{(Campaign.Passed(e) ? " (passed)" : "")}"));
            Note($"== Dan's copy, {Campaign.SlotName}: chapter {Campaign.Current.chapter}; chapter 5: {List()}");
            yield return Choose("chapter-5"); yield return new WaitForSecondsRealtime(1); yield return Snap("D-chapter5-dan"); Note($"  page '{Page}'");
            yield return Choose("cev-c5-park-competition"); yield return new WaitForSecondsRealtime(1); yield return Snap("D-competition-page");
            Note($"  event page '{Page}': {Texts("Park competition", "Rivals:", "Pass:", "Pays:").Replace("\n", " ")}");
            yield return Press(GamepadButton.East); yield return new WaitForSecondsRealtime(.8f); Note($"  B: page '{Page}'");
            yield return Choose("cev-c5-park-competition"); yield return new WaitForSecondsRealtime(.8f);
            float limit = F(Arg("-p1300comp", "-1")); if (limit > 0) StuntEvent.TestLimit = limit;
            yield return Choose("start-event"); yield return new WaitForSecondsRealtime(2); yield return WaitRacing(); Bind();
            var ev = StuntEvent.Current; float t0 = Time.realtimeSinceStartup; while (ev && ev.State != StuntEvent.Phase.Running && Time.realtimeSinceStartup - t0 < 20) yield return null;
            Note($"  started: event {(ev ? ev.State.ToString() : "none")}, competition {StuntEvent.Competition?.Id}, riders {(ev ? ev.Riders.Count : 0)}, round {(StuntEvent.TestLimit > 0 ? StuntEvent.TestLimit + " s (shortened for the check)" : ev?.Limit + " s")}");
            if (!ev) yield break;
            AudioListener.volume = 0; yield return Snap("D-competition-start");
            var bot = new Bot { pad = pad, car = race.vehicle }; BindPad(); float nextLog = 0; bool riderShot = false, hudShot = false; var leaders = new List<string>(); string lastLeader = "";
            Time.timeScale = 2;
            while (ev && ev.State != StuntEvent.Phase.Done && flow.State != RaceFlow.Stage.Results)
            {
                yield return null; AudioListener.volume = 0; if (flow.State == RaceFlow.Stage.Racing) BotStep(bot);
                var top = ev.Standings[0]; string leader = ev.NameOf(top); if (top.points > 0 && leader != lastLeader) { lastLeader = leader; leaders.Add($"{ev.Clock:0}s {leader} {top.points}"); }
                if (ev.Clock >= nextLog) { nextLog += 30; Note($"  {ev.Clock:0} s: " + string.Join("; ", ev.Standings.Select(r => $"{ev.NameOf(r)} {r.points} ({r.combos} combos{(r.car && r.car.GetComponent<ParkRival>() ? $", resets {r.car.GetComponent<ParkRival>().Resets}" : "")})"))); }
                if (!hudShot && ev.Clock > 25 && ev.Standings.Any(r => r.points > 0)) { hudShot = true; Time.timeScale = 1; yield return Snap("D-competition-hud"); Time.timeScale = 2; }
                // a rival in the air turning a trick, seen from beside it
                if (!riderShot && ev.Clock > 10) foreach (var r in ev.Riders.Where(x => x.player >= 3))
                {
                    var car = r.car; if (!car || car.GroundedWheels > 0 || !car.AiAir) continue;
                    riderShot = true; Time.timeScale = 1; var side = Vector3.Cross(Vector3.up, Flat(car.Body.linearVelocity).normalized);
                    yield return Free("D-rival-trick", car.Body.position + side * 14 + Vector3.up * 3, car.Body.position, 60); Time.timeScale = 2; Note($"  shot: {ev.NameOf(r)} in the air turning a trick at {V(car.Body.position)}"); break;
                }
            }
            Time.timeScale = 1; Note("  the lead: " + string.Join(" -> ", leaders));
            foreach (var r in ev.Riders.Where(x => x.player >= 3)) { var t = r.car ? r.car.GetComponent<RivalTricks>() : null; Note($"  {ev.NameOf(r)} ({r.vehicle}): {r.points} pts, {r.combos} combos, best {r.best}; flights {t?.Flights}, tricks turned {t?.Tricks}; resets {r.car?.GetComponent<ParkRival>()?.Resets}"); }
            float w = Time.realtimeSinceStartup; while (flow.State != RaceFlow.Stage.Results && Time.realtimeSinceStartup - w < 60) yield return null;
            yield return new WaitForSecondsRealtime(2); yield return Snap("D-competition-results"); StuntEvent.TestLimit = -1;
            var o = CampaignRun.Last; Note($"  result: {(o == null ? "none" : $"{o.Headline}; passed {o.Passed}; pay {o.Pay}{(o.Bonus > 0 ? $" + bonus {o.Bonus}" : "")}; unlocked [{string.Join(", ", o.Unlocked)}]")}");
            Note($"  results page '{Page}': {Texts("1", "2", "3", "4", "5", "6", "Place").Replace("\n", " ")}");
            yield return Choose("campaign-continue"); yield return WaitMenu(); Bind(); yield return new WaitForSecondsRealtime(1.5f);
            Note($"  CONTINUE: page '{Page}', chapter 5: {List()}");
            // a fresh chapter-5 save (as a player who has just reached it): the park first and open, the competition second and locked behind it
            var c = Campaign.Current; c.chapter = 5; c.results.RemoveAll(r => r.id.StartsWith("c5-")); Campaign.Save(); Campaign.Load(flow.Save.DirectoryPath);
            Menu.OpenCampaign(5); yield return new WaitForSecondsRealtime(1.5f); yield return Snap("D-chapter5-fresh"); Note($"== a fresh chapter 5: {List()}; page '{Page}'");
        }

        IEnumerator Closed1300r3()
        {
            Campaign.Testing = true; DriverVariation.Seed = System.Environment.TickCount; Conditions();
            int course = int.Parse(Arg("-p119course", "4")); bool normal = Arg("-p1300normal", "0") == "1";
            if (normal) { yield return QuickTo115(course); Conditions(); if (race.stunts) flow.StepRaceType(1); flow.StartRace(); }
            else yield return StuntQuick(course, false, 1, 1);
            yield return WaitRacing(); Bind();
            var root = race.gameObject.scene.GetRootGameObjects().FirstOrDefault(g => g.name.StartsWith("Stunt Race closed shortcuts"));
            Note($"== {(normal ? "normal race" : "Stunt Race")} on {RacePlaylists.Titles[course]} ({race.gameObject.scene.name}): stunts {race.stunts}, record course {race.RecordCourse}; shortcuts in the race {race.Branches?.Length}, of {race.AllBranches?.Length}: {string.Join(", ", race.AllBranches?.Select(b => b.title + (b.isActiveAndEnabled ? " (open)" : " (closed)")) ?? new string[0])}; barricades {(root ? root.GetComponentsInChildren<BoxCollider>().Length + " panels" : "none")}");
            foreach (var line in StuntShortcuts.Report) Note("  closed: " + line);
            if (root) foreach (Transform block in root.transform.GetChild(0)) { var p = block.position; float rs = race.road.Project(p, out float lat); Note($"  {block.name}: {Mathf.Abs(lat):0.0} m from the main road's line (its half width {race.road.HalfWidth(rs):0.0} m)"); }
            if (normal) yield break;

            int k = 0; var car0 = race.vehicle; car0.Body.isKinematic = true;
            // each barricade where it stands, from the way a driver would turn in (its sign faces them), and from above
            if (root) foreach (Transform block in root.transform.GetChild(0))
            {
                var at = block.position; var way = block.forward; var eye = at - way * 22; eye.y = TopAt(eye + Vector3.up * 5) + 1.6f;
                car0.Body.position = eye + Vector3.down * 2; car0.transform.position = car0.Body.position; Physics.SyncTransforms(); FindAnyObjectByType<ChaseCamera>()?.Snap();
                string tag = block.name.Replace("Leviathan road closed (", "").Replace(")", "").Replace(' ', '-');
                yield return Free($"E-barricade-{course}-{++k}-{tag}", eye, at + Vector3.up * 1.2f, 62, 1280, 720);
                yield return Free($"E-barricade-{course}-{k}-{tag}-top", at + Vector3.up * 70 - Vector3.forward * .5f, at, 60, 900, 900, true, 45);
            }
            car0.Body.isKinematic = false; flow.StartRace(); yield return WaitRacing(); Bind();
            root = race.gameObject.scene.GetRootGameObjects().FirstOrDefault(g => g.name.StartsWith("Stunt Race closed shortcuts")); // built again at the restart
            var me = race.Racers[0]; var car = race.vehicle; Pilot(car, me); race.finishGraceSeconds = 100000;
            var ai = race.Racers.Where(r => r.IsAi).ToList(); var reset0 = ai.ToDictionary(r => r, r => r.Recoveries); var on = ai.ToDictionary(r => r, r => new HashSet<string>()); var resets = new List<string>();
            var barriers = root ? root.GetComponentsInChildren<BoxCollider>().Select(c => c.transform.position).ToList() : new List<Vector3>(); Note($"  barricade panels after the restart: {barriers.Count}");
            Time.timeScale = 4; float t0 = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup - t0 < 900 && flow.State == RaceFlow.Stage.Racing && !(me.Progress.Finished && ai.All(r => r.Progress.Finished || r.Dnf)))
            {
                AudioListener.volume = 0; yield return null; if (Time.timeScale == 0) Time.timeScale = 4;
                foreach (var r in ai)
                {
                    var p = r.Car.Body.position;
                    foreach (var b in race.AllBranches) { float s = b.Project(p, out _); var q = b.At(s, out _); if (s > StuntShortcuts.Inset + 6 && s < b.Length - StuntShortcuts.Inset - 6 && Vector3.Distance(Flat(p), Flat(q)) < b.halfWidth && Mathf.Abs(p.y - q.y) < 3) { float rs = race.road.Project(p, out float lat); if (Mathf.Abs(lat) > race.road.HalfWidth(rs) + 2) on[r].Add(b.title); } } // off the main road (a shortcut's lower part may run along it)
                    if (r.Recoveries - reset0[r] > resets.Count(x => x.StartsWith(Rp(r) + "#" + r.GetHashCode())))
                    { float near = barriers.Count == 0 ? 999 : barriers.Min(x => Vector3.Distance(x, p)); resets.Add($"{Rp(r)}#{r.GetHashCode()} reset at {V(p)}, {near:0} m from the nearest barricade"); }
                }
                if (me.Progress.Finished && ai.Any(r => !r.Progress.Finished && !r.Dnf) && race.Clock > 900) break;
            }
            Time.timeScale = 1;
            foreach (var r in ai) Note($"  rival {Rp(r)}: {(r.Progress.Finished ? "finished" : r.Dnf ? "DNF" : "unfinished")}, resets {r.Recoveries - reset0[r]}, on a closed shortcut: {(on[r].Count == 0 ? "never" : string.Join(", ", on[r]))}");
            Note($"  resets: {(resets.Count == 0 ? "none" : string.Join("; ", resets.Select(x => x.Substring(x.IndexOf(' ') + 1))))}; player {(me.Progress.Finished ? "finished" : "unfinished")}, missed gates {me.Progress.MissedGates}");
        }
    }
}
