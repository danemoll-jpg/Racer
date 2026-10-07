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
        public static bool Locked(VehicleProfile p) => p != null && p.Reward && !RewardEarned;
        public static bool Locked(string id) => Locked(VehicleProfile.Find(id));
        public static string LockedText => $"Find all {AcornsNeeded} Woodland Acorns ({Mathf.Min(AcornsFound, AcornsNeeded)}/{AcornsNeeded})";
        public static void Set(int found, bool earned) { AcornsFound = found; RewardEarned = earned || found >= AcornsNeeded; }
        public static void Load(string root)
        {
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
