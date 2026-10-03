using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using UnityEngine;

namespace Racer
{
    // Portable, self-contained files; no player-save or publisher dependencies.
    public sealed class DebugReportSession
    {
        [Serializable] public sealed class Bug
        {
            public string id, timestamp, course, direction, mode, vehicle, version, buildGuid, scene;
            public Vector3 position, rotation, vehiclePosition;
            public float heading, speedMps, roadProgress, branchProgress;
            public int lap, nextCheckpoint;
            public string branch, viewpoint, screenshot, comment;
            public string conditions;
            public bool debugMovementUsed;
        }
        [Serializable] public sealed class Report
        {
            public int schemaVersion = 2;
            public string started, sessionId, closedAt;
            public bool closed, exported;
            public List<Bug> bugs = new();
        }
        public readonly string DirectoryPath;
        public readonly Report Data = new();
        public int Count => Data.bugs.Count;
        public string Id => Data.sessionId;
        public bool Closed => Data.closed;
        public DebugReportSession(string root)
        {
            Data.started = DateTimeOffset.Now.ToString("o");
            string name = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-fff");
            DirectoryPath = Path.Combine(root, name + "_" + Guid.NewGuid().ToString("N").Substring(0, 6));
            Data.sessionId = Path.GetFileName(DirectoryPath);
            Directory.CreateDirectory(Path.Combine(DirectoryPath, "Screenshots"));
        }
        DebugReportSession(string directory, Report data) { DirectoryPath = directory; Data = data; }
        // Sessions survive quitting: only Export or an explicit new session ends one. The most recently
        // started session folder with reports is resumed if it is still OPEN. Closed, empty or unreadable
        // newest sessions mean a fresh session; no folder is ever modified here.
        public static DebugReportSession ResumeLatest(string root)
        {
            if (!Directory.Exists(root)) return null;
            var folders = Directory.GetDirectories(root);
            Array.Sort(folders, StringComparer.Ordinal);
            for (int i = folders.Length - 1; i >= 0; i--)
            {
                string json = Path.Combine(folders[i], "bugs.json");
                if (!File.Exists(json)) continue;
                try
                {
                    var data = JsonUtility.FromJson<Report>(File.ReadAllText(json));
                    if (data == null || data.bugs == null || data.sessionId != Path.GetFileName(folders[i])) return null;
                    return !data.closed && data.bugs.Count > 0 ? new DebugReportSession(folders[i], data) : null;
                }
                catch { return null; }
            }
            return null;
        }
        public string NextId => Closed ? throw new InvalidOperationException("This debug session is closed.") : "BUG-" + (Count + 1).ToString("000");
        public void Save(Bug bug)
        {
            if (Closed) throw new InvalidOperationException("Cannot append to a closed debug session.");
            if (!File.Exists(Path.Combine(DirectoryPath, bug.screenshot))) throw new IOException("The screenshot is missing; capture again.");
            if (!Data.bugs.Contains(bug)) Data.bugs.Add(bug);
            Write();
        }
        public void Write()
        {
            AtomicSave.Write(Path.Combine(DirectoryPath, "bugs.json"), JsonUtility.ToJson(Data, true));
            var md = new StringBuilder("# Racer bug report\n\nSession: " + Id + "\nStarted: " + Data.started
                + "\nStatus: " + (Closed ? "CLOSED" : "OPEN") + "\nReports: " + Count
                + (Closed ? "\nClosed: " + Data.closedAt + "\nExported: " + Data.exported : "") + "\n\n");
            foreach (var b in Data.bugs)
            {
                md.AppendLine("## " + b.id).AppendLine().AppendLine("Comment:");
                foreach (string line in (b.comment ?? "").Replace("\r", "").Split('\n')) md.AppendLine("> " + line);
                md.AppendLine().AppendLine("```text")
                  .AppendLine("Timestamp: " + b.timestamp).AppendLine("Course: " + b.course)
                  .AppendLine("Direction: " + b.direction).AppendLine("Mode: " + b.mode)
                  .AppendLine("Position: " + Position(b.position)).AppendLine("Rotation: " + Position(b.rotation))
                  .AppendLine("Heading: " + F(b.heading) + " degrees").AppendLine("Viewpoint: " + b.viewpoint)
                  .AppendLine("Vehicle: " + b.vehicle).AppendLine("Vehicle position: " + Position(b.vehiclePosition))
                  .AppendLine("Speed: " + F(b.speedMps) + " m/s / " + F(DisplayUnits.Mph(b.speedMps)) + " mph")
                  .AppendLine("Lap: " + b.lap + " / Next checkpoint: " + b.nextCheckpoint)
                  .AppendLine("Road progress: " + F(b.roadProgress) + " m / Branch: " + b.branch + " / " + F(b.branchProgress) + " m")
                  .AppendLine("Version: " + b.version + " / Build: " + b.buildGuid).AppendLine("Scene: " + b.scene)
                  .AppendLine("Conditions: " + (string.IsNullOrEmpty(b.conditions) ? "not recorded" : b.conditions))
                  .AppendLine("Debug movement used: " + b.debugMovementUsed).AppendLine("```").AppendLine()
                  .AppendLine("![" + b.id + "](" + b.screenshot.Replace('\\', '/') + ")").AppendLine();
            }
            AtomicSave.Write(Path.Combine(DirectoryPath, "BUG_REPORT.md"), md.ToString());
        }
        static string F(double n) => n.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
        static string Position(Vector3 p) => "X=" + F(p.x) + ", Y=" + F(p.y) + ", Z=" + F(p.z);
        public string Export()
        {
            if (Closed) throw new InvalidOperationException("This session is already closed; its history is preserved.");
            string zip = DirectoryPath + "_" + Guid.NewGuid().ToString("N").Substring(0, 8) + ".zip";
            string temporary = zip + ".tmp";
            try
            {
                // Include the final lifecycle state in both the folder and portable archive.
                Data.closed = Data.exported = true; Data.closedAt = DateTimeOffset.Now.ToString("o");
                Write();
                ZipFile.CreateFromDirectory(DirectoryPath, temporary, System.IO.Compression.CompressionLevel.Optimal, true);
                File.Move(temporary, zip);
                return zip;
            }
            catch
            {
                Data.closed = Data.exported = false; Data.closedAt = null;
                try { Write(); } catch { /* Keep the original export error; in-memory reports remain open. */ }
                try { if (File.Exists(temporary)) File.Delete(temporary); } catch { /* Never delete a prior export. */ }
                throw;
            }
        }
        public void Close()
        {
            if (Closed) return;
            Data.closed = true; Data.closedAt = DateTimeOffset.Now.ToString("o");
            try { Write(); }
            catch { Data.closed = false; Data.closedAt = null; throw; }
        }
    }
}
