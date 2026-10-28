using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Racer
{
    // 1.3.0-review3 (Dan: "want to implement blocking off the shortcuts on the mountain tracks on the Mountain forward and mountain reverse stunt
    // races"): in a Stunt Race on Mountain Loop Forward or Reverse (Quick Race, split screen, the Stunt chapter) every shortcut is closed: a timber
    // barricade with a one-sided ROAD CLOSED sign (Leviathan's closed ways, 0.117) across each end of each shortcut, a little way in from the main
    // road; the race no longer has them (RaceDirector.Branches empty: no gate credit for one, no rival takes one, none on the minimap, no reset onto
    // one). Normal races there, Leviathan, Free Roam: every shortcut open, no barricade. Built at each race start (RaceDirector.RestartRace), removed
    // otherwise. These two Stunt Races' records start again under their own course key (RecordSuffix).
    public static class StuntShortcuts
    {
        const string RootName = "Stunt Race closed shortcuts (1.3.0-review3)";
        public const string RecordSuffix = "-closed";
        public const float Inset = 14, ClearMargin = 6; // metres in from each end of the shortcut, at least; metres clear of the main road's edge
        public static readonly List<string> Report = new(); // checks: each shortcut closed and where
        static readonly List<WoodlandRoute> disabled = new();
        public static bool Closed(RaceDirector race) => race && race.stunts && !race.FreeRoam && !race.Leviathan && race.gameObject.scene.name is "MountainLoop" or "MountainLoopReverse";
        public static void Apply(RaceDirector race)
        {
            var scene = race.gameObject.scene; Report.Clear();
            foreach (var old in scene.GetRootGameObjects().Where(g => g.name == RootName)) { old.SetActive(false); Object.Destroy(old); }
            // 1.3.0-review6 (Dan, 2026-10-27: "Just the ones on the mountain"): in a Leviathan Stunt Race the shortcuts that leave the course on its
            // mountain (Leviathan Reverse's Downhill Ridge Cut) are closed the same way; its others (below the mountain) stay open. Its records
            // keep their course id (the lap's new id, leviathan-reverse-v5, already starts new boards).
            var shut = Closed(race) ? (race.AllBranches ?? new WoodlandRoute[0]).Where(b => b).ToList() : MountainOnes(race);
            race.CloseBranches(shut);
            foreach (var b in disabled) if (b) b.enabled = true; disabled.Clear(); // the ones closed by an earlier Stunt Race, open again
            foreach (var b in shut) if (b && b.enabled) { b.enabled = false; disabled.Add(b); }
            foreach (var map in Object.FindObjectsByType<RacingMiniMap>(FindObjectsSortMode.None)) map.RecacheRoutes();
            // a closed shortcut's own boards (Leviathan's "<  DOWNHILL RIDGE CUT" / REJOIN ROAD) are not shown
            if (race.Leviathan) foreach (var t in race.Leviathan.GetComponentsInChildren<Transform>(true).Concat(scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<Transform>(true))))
                    if (t && t.name.StartsWith("Leviathan shortcut board")) t.gameObject.SetActive(!(race.AllBranches ?? new WoodlandRoute[0]).Any(b => b && shut.Contains(b) && t.name.Contains("(" + b.title + " ")));
            if (shut.Count == 0) return;
            var root = new GameObject(RootName).transform; SceneManager.MoveGameObjectToScene(root.gameObject, scene);
            var blocks = new List<LeviathanCourse.Block>();
            foreach (var b in shut)
            {
                if (!b) continue; float len = b.Length;
                // the way in at its start (traffic heading along it) and at its end (traffic that would turn in from the far end, heading back along it),
                // each moved further in until the whole barricade is clear of the main road (where a shortcut leaves or joins it, it runs beside it)
                // (a shortcut whose lower part runs along the main road is closed where it leaves the road)
                float sIn = Clear(race, b, Inset, 1, len - Inset), sOut = Clear(race, b, len - Inset, -1, sIn + 10);
                var pIn = b.At(sIn, out var fIn); var pOut = b.At(sOut, out var fOut);
                blocks.Add(new LeviathanCourse.Block { kind = "barricade", at = b.title + " start", pos = new[] { pIn.x, pIn.z, pIn.y }, yaw = Yaw(fIn), half = b.halfWidth + 1.5f });
                blocks.Add(new LeviathanCourse.Block { kind = "barricade", at = b.title + " end", pos = new[] { pOut.x, pOut.z, pOut.y }, yaw = Yaw(-fOut), half = b.halfWidth + 1.5f });
                Report.Add($"{b.title}: closed at its start ({pIn.x:F0}, {pIn.y:F0}, {pIn.z:F0}) and its end ({pOut.x:F0}, {pOut.y:F0}, {pOut.z:F0})");
            }
            LeviathanCourse.BuildBlocks(new LeviathanCourse.Data { blocks = blocks.ToArray() }, root);
            Physics.SyncTransforms();
            Debug.Log("Stunt Race shortcuts closed: " + string.Join("; ", Report));
        }
        // a Leviathan Stunt Race's shortcuts that leave the course on its mountain (none in any other race)
        public static List<WoodlandRoute> MountainOnes(RaceDirector race)
        {
            var lev = race ? race.Leviathan : null;
            if (!lev || !race.stunts || race.FreeRoam || race.AllBranches == null) return new List<WoodlandRoute>();
            return race.AllBranches.Where(b => b && lev.Mountain(b.entryRoad)).ToList();
        }
        static float Yaw(Vector3 f) => Mathf.Atan2(f.x, f.z) * Mathf.Rad2Deg;
        // the first station from s (stepping by way) where the barricade across the shortcut, ends included, is 6 m clear of the main road's edge (the
        // rivals' line runs wider than the road's edge where a shortcut leaves or joins it)
        static float Clear(RaceDirector race, WoodlandRoute b, float s, int way, float limit)
        {
            float len = b.Length, half = b.halfWidth + 1.5f, from = s; var road = race.road;
            for (; way > 0 ? s <= limit : s >= limit; s += way * 2)
            {
                var p = b.At(s, out var f); var right = Vector3.Cross(Vector3.up, f).normalized; bool clear = true;
                foreach (float x in new[] { -half, 0, half })
                {
                    var q = p + right * x; float rs = road.Project(q, out float lateral); var c = road.At(rs, out _);
                    if (Mathf.Abs(lateral) < road.HalfWidth(rs) + ClearMargin && Mathf.Abs(q.y - c.y) < 6) { clear = false; break; }
                }
                if (clear) return s;
            }
            return from; // nowhere clear: where it was asked for
        }
    }
}
