using System.Collections.Generic;
using System.Linq;

namespace Racer
{
    // 0.110 Part D (Dan: "it doesn't remember my vehicle from event to event"): the vehicles last driven, newest first, kept in settings.json for
    // player 1 and player 2. A vehicle counts as driven when an event, race, Free Roam or split-screen round starts with it; every setup then
    // preselects the newest one it allows (else the first it allows). Player 1's newest one that Race allows is also the Race / Free Roam vehicle.
    // The fresh run keeps its own list in its campaign save (Campaign.Drove), so it never changes this one.
    public static class RecentVehicles
    {
        const int Keep = 16;
        static RacerSave save;
        public static void Use(RacerSave s) => save = s;
        static string[] List(int player) => (save == null ? null : player == 2 ? save.Settings.recentVehiclesP2 : save.Settings.recentVehicles) ?? new string[0];
        public static string Last(int player) => List(player).FirstOrDefault();
        public static void Drove(int player, string id)
        {
            var p = VehicleProfile.All.FirstOrDefault(v => v.Id == id); if (save == null || p == null) return;
            var list = new[] { id }.Concat(List(player).Where(x => x != id)).Take(Keep).ToArray();
            bool race = player == 1 && !p.Stunt && !Campaign.VehicleLocked(p) && !VehicleUnlocks.Locked(p);
            if (list.SequenceEqual(List(player)) && (!race || save.Settings.vehicleId == id)) return;
            if (player == 2) save.Settings.recentVehiclesP2 = list; else save.Settings.recentVehicles = list;
            if (race) save.Settings.vehicleId = id;
            save.SaveSettings();
        }
        // the newest one driven that the list allows, else the first allowed (null: none allowed)
        public static string Pick(int player, IEnumerable<VehicleProfile> allowed)
        {
            var a = allowed.ToList(); if (a.Count == 0) return null;
            foreach (var id in List(player)) if (a.Any(v => v.Id == id)) return id;
            return a[0].Id;
        }
    }
}
