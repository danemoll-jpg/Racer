using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Racer.Editor
{
    // 0.99 Part F: Dan's recordings live in SourceArt/Audio/PoliceRadio (outside Assets, so Unity does not import them); this copies them into
    // Assets/Resources/PoliceRadio (and the offline test voice from SourceArt/Audio/PoliceRadio/_placeholder into Assets/Resources/PoliceRadioPlaceholder)
    // so they are in the build. Runs before every build; "Racer/Sync police radio clips" runs it by hand.
    public sealed class PoliceRadioSync : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;
        public void OnPreprocessBuild(BuildReport report) => Sync();
        const string Source = "SourceArt/Audio/PoliceRadio", Voices = "Assets/Resources/PoliceRadio", Placeholders = "Assets/Resources/PoliceRadioPlaceholder";
        [MenuItem("Racer/Sync police radio clips")]
        public static void Sync()
        {
            int a = Copy(Source, Voices, new[] { "*.mp3", "*.wav", "*.ogg" }), b = Copy(Source + "/_placeholder", Placeholders, new[] { "*.wav" });
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            foreach (var guid in AssetDatabase.FindAssets("t:AudioClip", new[] { Voices, Placeholders }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid); var imp = AssetImporter.GetAtPath(path) as AudioImporter; if (!imp) continue;
                var s = imp.defaultSampleSettings; if (s.loadType == AudioClipLoadType.DecompressOnLoad && imp.forceToMono) continue;
                s.loadType = AudioClipLoadType.DecompressOnLoad; imp.defaultSampleSettings = s; imp.forceToMono = true; imp.SaveAndReimport();
            }
            Debug.Log($"Police radio clips: {a} recordings, {b} test-voice clips synced into Resources");
        }
        public static void SyncAndExit() { Sync(); EditorApplication.Exit(0); }
        static int Copy(string from, string to, string[] patterns)
        {
            if (!Directory.Exists(from)) return 0; Directory.CreateDirectory(to);
            var files = patterns.SelectMany(p => Directory.GetFiles(from, p, SearchOption.TopDirectoryOnly)).ToArray(); int n = 0;
            foreach (var f in files) { var d = Path.Combine(to, Path.GetFileName(f)); if (!File.Exists(d) || new FileInfo(d).Length != new FileInfo(f).Length || File.GetLastWriteTimeUtc(d) < File.GetLastWriteTimeUtc(f)) { File.Copy(f, d, true); } n++; }
            // a file Dan removed from the source folder is removed from the build too
            foreach (var d in Directory.GetFiles(to).Where(x => !x.EndsWith(".meta"))) if (!files.Any(f => Path.GetFileName(f) == Path.GetFileName(d))) { File.Delete(d); if (File.Exists(d + ".meta")) File.Delete(d + ".meta"); }
            return n;
        }
    }
}
