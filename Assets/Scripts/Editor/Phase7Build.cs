using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Racer.Editor
{
    public static class Phase7Build
    {
        [MenuItem("Racer/Build Phase 7 Review Player")]
        public static void Build()
        {
            if(Application.isPlaying || SceneManager.GetActiveScene().isDirty) throw new System.InvalidOperationException("Save scene and exit Play mode first.");
            Directory.CreateDirectory("Builds/Phase7"); Directory.CreateDirectory("Docs/Phase7");
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions { scenes=new[]{StreetLoopBuilder.ScenePath},locationPathName="Builds/Phase7/Racer.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development });
            File.WriteAllText("Docs/Phase7/build.txt",$"{report.summary.result}; errors={report.summary.totalErrors}; warnings={report.summary.totalWarnings}; bytes={report.summary.totalSize}; duration={report.summary.totalTime}\n"+string.Join("\n",report.steps.SelectMany(s=>s.messages).Where(m=>m.type==LogType.Error||m.type==LogType.Warning).Select(m=>m.content)));
        }
    }
}
