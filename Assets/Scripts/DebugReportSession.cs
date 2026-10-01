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
            public bool debugMovementUsed;
        }
        [Serializable] public sealed class Report
        {
            public int schemaVersion = 1;
            public string started;
            public List<Bug> bugs = new();
        }
        public readonly string DirectoryPath;
        public readonly Report Data = new();
        public int Count => Data.bugs.Count;
        public DebugReportSession(string root)
        {
            Data.started = DateTimeOffset.Now.ToString("o");
            string name = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-fff");
            DirectoryPath = Path.Combine(root, name + "_" + Guid.NewGuid().ToString("N").Substring(0, 6));
            Directory.CreateDirectory(Path.Combine(DirectoryPath, "Screenshots"));
        }
        public string NextId => "BUG-" + (Count + 1).ToString("000");
        public void Save(Bug bug)
        {
            if (!File.Exists(Path.Combine(DirectoryPath, bug.screenshot))) throw new IOException("The screenshot is missing; capture again.");
            if (!Data.bugs.Contains(bug)) Data.bugs.Add(bug);
            Write();
        }
        public void Write()
        {
            AtomicSave.Write(Path.Combine(DirectoryPath, "bugs.json"), JsonUtility.ToJson(Data, true));
            var md = new StringBuilder("# Racer bug report\n\nSession: " + Data.started + "\n\n");
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
                  .AppendLine("Debug movement used: " + b.debugMovementUsed).AppendLine("```").AppendLine()
                  .AppendLine("![" + b.id + "](" + b.screenshot.Replace('\\', '/') + ")").AppendLine();
            }
            AtomicSave.Write(Path.Combine(DirectoryPath, "BUG_REPORT.md"), md.ToString());
        }
        static string F(double n) => n.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
        static string Position(Vector3 p) => "X=" + F(p.x) + ", Y=" + F(p.y) + ", Z=" + F(p.z);
        public string Export()
        {
            Write();
            string zip = DirectoryPath + "_" + DateTime.Now.ToString("HHmmssfff") + ".zip";
            ZipFile.CreateFromDirectory(DirectoryPath, zip, System.IO.Compression.CompressionLevel.Optimal, true);
            return zip;
        }
    }
}
