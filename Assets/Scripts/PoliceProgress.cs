using System;
using System.IO;
using UnityEngine;

namespace Racer
{
    // 0.99 Part E: the two police vehicles are earned by police goals, kept in a small file of their own (police-progress-v1.json beside the other saves):
    //   Patrol car   escape the police at heat 3 or higher on Normal or Hard (a Getaway, or a hidden-police chase in Free Roam)
    //   Police bike  catch 20 speeders in Speed Patrol (counted across all rounds)
    // Only player 1's play counts (split-screen included, as it is player 1's save). Nothing before 0.99 was recorded, so earlier play does not count; Dan's save gets no grant.
    public static class PoliceProgress
    {
        public const string File = "police-progress-v1.json";
        public const int SpeedersNeeded = 20, HeatNeeded = 3;
        [Serializable] sealed class Data { public int version = 1; public bool carEarned, bikeEarned; public int speedersCaught; }
        static Data data = new(); static string path;
        public static bool CarEarned => data.carEarned; public static bool BikeEarned => data.bikeEarned; public static int SpeedersCaught => data.speedersCaught;
        public static string CarGoal => "Escape the police at heat 3+ (Normal or Hard)";
        public static string BikeGoal => $"Catch {SpeedersNeeded} speeders in Speed Patrol ({Math.Min(data.speedersCaught, SpeedersNeeded)} / {SpeedersNeeded})";
        static readonly System.Collections.Generic.List<UnlockNotice.Notice> waiting = new(); static bool holdNotices;
        public static void Load(string root)
        {
            data = new Data(); path = root != null ? Path.Combine(root, File) : null;
            try { if (path != null && System.IO.File.Exists(path)) data = JsonUtility.FromJson<Data>(System.IO.File.ReadAllText(path)) ?? new Data(); } catch (Exception) { data = new Data(); }
        }
        static void Save() { try { if (path != null) AtomicSave.Write(path, JsonUtility.ToJson(data, true)); } catch (Exception e) { Debug.LogWarning("Police progress not saved: " + e.Message); } }
        static RacerSave SaveOf => Hints.Flow ? Hints.Flow.Save : null;
        static void Earn(bool car)
        {
            if (car ? data.carEarned : data.bikeEarned) return; if (car) data.carEarned = true; else data.bikeEarned = true; Save();
            var n = new UnlockNotice.Notice { id = car ? "police-car" : "police-bike", heading = car ? "ESCAPED THE POLICE AT HEAT 3+" : $"{SpeedersNeeded} SPEEDERS CAUGHT", vehicle = car ? VehicleProfile.Police.Id : VehicleProfile.PoliceBike.Id };
            if (holdNotices) waiting.Add(n); else UnlockNotice.Raise(SaveOf, n);
        }
        // an embedded chase holds the panel until it is over (the panel pauses Free Roam)
        public static void HoldNotices(bool hold) { holdNotices = hold; if (!hold) { foreach (var n in waiting) UnlockNotice.Raise(SaveOf, n); waiting.Clear(); } }
        // a runner escaped (the round limit counts): player 1 at heat 3 or more on Normal or Hard
        public static void Escaped(GetawayChase chase, GetawayChase.Runner runner)
        {
            if (chase == null || runner == null || !runner.human || runner.player != 1 || chase.Mode != GetawayChase.Variant.Getaway) return;
            if (chase.Heat >= HeatNeeded && chase.Difficulty >= 1) Earn(true);
        }
        public static void SpeederCaught(int player)
        {
            if (player != 1) return; data.speedersCaught++; Save(); if (data.speedersCaught >= SpeedersNeeded) Earn(false);
        }
    }
}
