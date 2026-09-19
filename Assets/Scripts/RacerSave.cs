using System;
using System.IO;
using UnityEngine;

namespace Racer
{
    // Explicit rules version: crossing START after GO, ordered gates, three valid laps.
    public sealed class RacerSave
    {
        [Serializable] public sealed class Records
        {
            public int version = 1;
            public string course;
            public double lap, race;
        }
        [Serializable] public sealed class Options
        {
            public int version = 1;
            public float master = .8f, ambience = 1, feedback = .65f, vehicle = .75f;
            public float music = .6f;
            public bool radioOn=true;
            public string musicFolder="";
            public string musicSource="";
            public bool musicRecursive=true;
            public string radioChannel="";
            public int frameLimit = 60;
            public bool vsync = true;
            public bool opponents = true, traffic = true;
            public bool estimateAiFinishes = false;
            public string vehicleId = "original";
            public int difficulty = 1;
            public string[] opponentChoices = {"mixed","mixed","mixed"};
            public string[] opponentRoster = {"tourer","moto","atv"};
            public int[] bodyColors = {-1,-1,-1,-1};
            public HouseholdSchedule streetHouseholds = new(), forestHouseholds = new();
        }
        public Records Best { get; private set; }
        public Options Settings { get; private set; }
        public string Error { get; private set; }
        public string DirectoryPath { get; }
        string course;
        string recordFile = "records.json";
        public void SelectRecords(string category)
        {
            course = category; recordFile = "records-" + category + ".json";
            Best = Read<Records>(recordFile, r => r.version == 1 && r.course == course && Valid(r.lap) && Valid(r.race)) ?? new Records { course = course };
        }
        public RacerSave(string directory, string courseId)
        {
            DirectoryPath = directory; course = courseId;
            Best = Read<Records>("records.json", r => r.version == 1 && r.course == course && Valid(r.lap) && Valid(r.race)) ?? new Records { course = course };
            Settings = Read<Options>("settings.json", s => s.version == 1 && Volume(s.master) && Volume(s.ambience) && Volume(s.feedback) && Volume(s.vehicle) && (s.frameLimit == 30 || s.frameLimit == 60 || s.frameLimit == 120)) ?? new Options();
            if(!Volume(Settings.music))Settings.music=.6f;
        }
        static bool Valid(double n) => !double.IsNaN(n) && !double.IsInfinity(n) && n >= 0 && n < 31536000;
        static bool Volume(float n) => !float.IsNaN(n) && n >= 0 && n <= 1;
        T Read<T>(string name, Func<T, bool> validate) where T : class
        {
            string path = Path.Combine(DirectoryPath, name);
            if (!File.Exists(path)) return null;
            try
            {
                // Populate initialized defaults so pre-vehicle settings retain all old values.
                var data = Activator.CreateInstance<T>();
                JsonUtility.FromJsonOverwrite(File.ReadAllText(path), data);
                if (data != null && validate(data)) return data;
                Error = "Saved data was incompatible; defaults loaded.";
            }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException || e is ArgumentException)
            { Error = "Saved data could not be read; defaults loaded."; }
            return null;
        }
        bool Write(string name, object value)
        {
            try
            {
                Directory.CreateDirectory(DirectoryPath);
                string path = Path.Combine(DirectoryPath, name), temp = path + ".tmp";
                File.WriteAllText(temp, JsonUtility.ToJson(value, true));
                if (File.Exists(path)) File.Replace(temp, path, path + ".bak");
                else File.Move(temp, path);
                Error = null; return true;
            }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException)
            { Error = "Could not save. Records/settings remain in memory."; return false; }
        }
        public bool RecordLap(double seconds)
        {
            if (!Valid(seconds) || seconds <= 0 || (Best.lap > 0 && seconds >= Best.lap)) return false;
            Best.lap = seconds; Write(recordFile, Best); return true;
        }
        public bool RecordRace(double seconds)
        {
            if (!Valid(seconds) || seconds <= 0 || (Best.race > 0 && seconds >= Best.race)) return false;
            Best.race = seconds; Write(recordFile, Best); return true;
        }
        public void SaveSettings() => Write("settings.json", Settings);
        public void ApplySettings()
        {
            AudioListener.volume = Settings.master;
            QualitySettings.vSyncCount = Settings.vsync ? 1 : 0;
            Application.targetFrameRate = Settings.frameLimit;
        }
    }
}
