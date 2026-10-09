using System;
using System.IO;
using UnityEngine;

namespace Racer
{
    // 0.88 Part D: the acorn reward. Finding all of the Woodland Acorns unlocks the Turf Rocket riding mower. Read from the
    // existing acorn save (woodland-acorns-v1.json: no new progress to lose); once earned it stays earned (the save keeps
    // rewardEarned, and Restart Acorn Hunt carries it over). ExplorationCollection keeps this up to date while playing.
    public static class VehicleUnlocks
    {
        public const string AcornFile = "woodland-acorns-v1.json";
        public const int AcornsNeeded = 24;
        public static bool RewardEarned { get; private set; }
        public static int AcornsFound { get; private set; }
        // 0.99: the acorn reward and the two police vehicles (PoliceProgress); "Unlock everything (testing)" opens the police vehicles too
        public static bool Earned(VehicleProfile p) => p.Unlock == "police-car" ? PoliceProgress.CarEarned || Campaign.Testing : p.Unlock == "police-bike" ? PoliceProgress.BikeEarned || Campaign.Testing : RewardEarned;
        public static bool Locked(VehicleProfile p) => p != null && p.Reward && !Earned(p);
        // split-screen has everything unlocked except the acorn reward (as before)
        public static bool LockedInSplit(VehicleProfile p) => p != null && p.Reward && !p.IsPolice && !RewardEarned;
        public static string LockedTextFor(VehicleProfile p) => p.Unlock == "police-car" ? PoliceProgress.CarGoal : p.Unlock == "police-bike" ? PoliceProgress.BikeGoal : LockedText;
        public static string RewardNote(VehicleProfile p) => p.Unlock == "police-car" ? "A police goal: the patrol car." : p.Unlock == "police-bike" ? "A police goal: the patrol cycle." : "The acorn reward: a riding mower.";
        public static bool Locked(string id) => Locked(VehicleProfile.Find(id));
        public static string LockedText => $"Find all {AcornsNeeded} Woodland Acorns ({Mathf.Min(AcornsFound, AcornsNeeded)}/{AcornsNeeded})";
        public static void Set(int found, bool earned) { AcornsFound = found; RewardEarned = earned || found >= AcornsNeeded; }
        public static void Load(string root)
        {
            PoliceProgress.Load(root);
            int found = 0; bool earned = false;
            try
            {
                var path = Path.Combine(root, AcornFile);
                if (File.Exists(path)) { var data = JsonUtility.FromJson<ExplorationCollection.Save>(File.ReadAllText(path)); if (data != null) { found = data.found?.Count ?? 0; earned = data.rewardEarned; } }
            }
            catch (Exception) { }
            Set(found, earned);
        }
    }
}
