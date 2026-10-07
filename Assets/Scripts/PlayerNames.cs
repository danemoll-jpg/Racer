using System.Linq;
using UnityEngine;

namespace Racer
{
    // 0.94 Part A: the player's name (Settings > Gameplay, the welcome, split-screen), the names used before on this PC, and
    // the colours name tags and boards use for people (humans) and AI drivers.
    public static class PlayerNames
    {
        public const int MaxLength = 12;
        public const string Ai = "AI";
        static RacerSave.Options Settings => Hints.Flow && Hints.Flow.Save != null ? Hints.Flow.Save.Settings : null;
        // The saved player name ("Player" until one is entered).
        public static string Player => string.IsNullOrEmpty(Settings?.playerName) ? "Player" : Settings.playerName;
        public static bool Named => !string.IsNullOrEmpty(Settings?.playerName);
        public static string Clean(string value) => new string((value ?? "").Where(c => !char.IsControl(c)).Take(MaxLength).ToArray()).Trim();
        // Names used before, most recent first (the saved player name first).
        public static string[] Known
        {
            get
            {
                var s = Settings; var list = (s?.knownNames ?? new string[0]).Where(n => !string.IsNullOrEmpty(n)).ToList();
                if (Named) { list.Remove(s.playerName); list.Insert(0, s.playerName); }
                return list.Distinct().ToArray();
            }
        }
        public static void Remember(string name)
        {
            var s = Settings; name = Clean(name); if (s == null || name.Length == 0 || name == Ai) return;
            var list = (s.knownNames ?? new string[0]).Where(n => n != name).ToList(); list.Insert(0, name);
            s.knownNames = list.Take(12).ToArray();
        }
        public static bool SetPlayer(string value)
        {
            var s = Settings; var name = Clean(value); if (s == null || name.Length == 0) return false;
            s.playerName = name; Remember(name); Hints.Flow.Save.SaveSettings(); return true;
        }
        // On load: a save from before names existed belongs to Dan (his campaign or records are in it); a new player is asked
        // with the welcome. Only in memory: the settings are written with the next change, as any setting.
        public static void Adopt(RacerSave save, bool existingPlayer)
        {
            var s = save.Settings; if (!string.IsNullOrEmpty(s.playerName)) return;
            if (existingPlayer) { s.playerName = "Dan"; if (!(s.knownNames ?? new string[0]).Contains("Dan")) s.knownNames = new[] { "Dan" }.Concat(s.knownNames ?? new string[0]).ToArray(); }
        }
        public static readonly Color Human = new(1f, .78f, .28f), Machine = new(.86f, .93f, 1f);
    }
}
