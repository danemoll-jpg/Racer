using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Racer
{
    // 0.109 Part C: the results log for balancing. One JSON line per finished or abandoned attempt, appended to BalanceLog/results-log.jsonl in the
    // game's data folder (a test save writes to <test save>/BalanceLog instead, never the real one). Past ~5 MB the file is renamed with its date and a
    // new one started; nothing is ever deleted. Written once at the end of each attempt (no per-frame cost); copied into every debug-report ZIP.
    // Nothing here changes play: it only reads what the game already decided.
    public static class ResultsLog
    {
        public const string FileName = "results-log.jsonl", SheetName = "balance-sheet.md";
        public const long RollBytes = 5L * 1024 * 1024;
        public static string Folder { get; private set; }
        public static string FilePath => Folder == null ? null : Path.Combine(Folder, FileName);
        public static string LastLine { get; private set; }
        public static int Written { get; private set; }
        // attempt numbers per event (this session's count plus the ones already in the log, read once)
        static Dictionary<string, int> attempts;
        static string lastEvent; static bool lastFinished;

        // RaceFlow.Start: the save root decides the folder (the real save: <data folder>/BalanceLog; a test or validation save: <it>/BalanceLog)
        public static void Init(string saveRoot, bool test)
        {
            string folder = test ? Path.Combine(saveRoot, "BalanceLog") : Path.Combine(Application.persistentDataPath, "BalanceLog");
            if (Folder != folder) { Folder = folder; attempts = null; }
        }

        // the attempt number for this event id (1 = the first try in the log), and whether it follows straight on from the same event
        public static (int number, bool retry) Attempt(string eventKey)
        {
            if (attempts == null) LoadAttempts();
            attempts.TryGetValue(eventKey, out int n); n++; attempts[eventKey] = n;
            bool retry = lastEvent == eventKey; lastEvent = eventKey; return (n, retry);
        }
        static void LoadAttempts()
        {
            attempts = new Dictionary<string, int>();
            try
            {
                if (Folder == null || !Directory.Exists(Folder)) return;
                foreach (var f in Directory.GetFiles(Folder, "results-log*.jsonl"))
                    foreach (var line in File.ReadLines(f))
                    {
                        int at = line.IndexOf("\"eventKey\":\"", StringComparison.Ordinal); if (at < 0) continue; at += 12;
                        int end = line.IndexOf('"', at); if (end <= at) continue;
                        string key = line.Substring(at, end - at); attempts.TryGetValue(key, out int n); attempts[key] = n + 1;
                    }
            }
            catch (Exception) { }
        }

        // the common head of every line (the mode's own fields follow, then Write)
        public static Json Line(string mode, string eventKey, int number, bool retry)
        {
            var j = new Json();
            j.Add("time", DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture)).Add("version", Application.version).Add("mode", mode).Add("eventKey", eventKey)
             .Add("attempt", number).Add("retry", retry);
            return j;
        }
        public static void Write(Json j)
        {
            if (Folder == null) return;
            var unlocks = UnlockNotice.TakeEarnedForLog(); if (unlocks.Length > 0) j.Add("unlocksEarned", unlocks);
            string line = j.ToString();
            try
            {
                Directory.CreateDirectory(Folder); string path = FilePath;
                var info = new FileInfo(path);
                if (info.Exists && info.Length > RollBytes) File.Move(path, Path.Combine(Folder, "results-log-" + DateTime.Now.ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture) + ".jsonl"));
                File.AppendAllText(path, line + "\n", new UTF8Encoding(false));
                LastLine = line; Written++;
            }
            catch (Exception e) { Debug.LogWarning("Results log not written: " + e.Message); }
        }

        // a small ordered JSON object writer (JsonUtility cannot write dictionaries or mixed values)
        public sealed class Json
        {
            readonly StringBuilder b = new StringBuilder("{"); bool any;
            Json Key(string k) { if (any) b.Append(','); any = true; Str(b, k); b.Append(':'); return this; }
            public Json Add(string k, string v) { Key(k); if (v == null) b.Append("null"); else Str(b, v); return this; }
            public Json Add(string k, int v) { Key(k); b.Append(v.ToString(CultureInfo.InvariantCulture)); return this; }
            public Json Add(string k, long v) { Key(k); b.Append(v.ToString(CultureInfo.InvariantCulture)); return this; }
            public Json Add(string k, float v) { Key(k); Num(b, v); return this; }
            public Json Add(string k, bool v) { Key(k); b.Append(v ? "true" : "false"); return this; }
            public Json Add(string k, Json v) { Key(k); b.Append(v == null ? "null" : v.ToString()); return this; }
            public Json Add(string k, IEnumerable<Json> v) { Key(k); b.Append('[').Append(string.Join(",", v.Select(x => x.ToString()))).Append(']'); return this; }
            public Json Add(string k, IEnumerable<string> v) { Key(k); b.Append('['); bool f = true; foreach (var s in v) { if (!f) b.Append(','); f = false; Str(b, s); } b.Append(']'); return this; }
            public Json Add(string k, IEnumerable<float> v) { Key(k); b.Append('['); bool f = true; foreach (var s in v) { if (!f) b.Append(','); f = false; Num(b, s); } b.Append(']'); return this; }
            public Json Add(string k, IEnumerable<int> v) { Key(k); b.Append('[').Append(string.Join(",", v.Select(x => x.ToString(CultureInfo.InvariantCulture)))).Append(']'); return this; }
            public override string ToString() => b.ToString() + "}";
            static void Num(StringBuilder b, float v) { if (float.IsNaN(v) || float.IsInfinity(v)) b.Append("null"); else b.Append(Math.Round(v, 3).ToString(CultureInfo.InvariantCulture)); }
            static void Str(StringBuilder b, string s)
            {
                b.Append('"');
                foreach (char c in s)
                    switch (c)
                    {
                        case '"': b.Append("\\\""); break;
                        case '\\': b.Append("\\\\"); break;
                        case '\n': b.Append("\\n"); break;
                        case '\r': b.Append("\\r"); break;
                        case '\t': b.Append("\\t"); break;
                        default: if (c < 0x20) b.Append("\\u").Append(((int)c).ToString("x4")); else b.Append(c); break;
                    }
                b.Append('"');
            }
        }
    }
}
