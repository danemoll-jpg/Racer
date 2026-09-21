using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
namespace Racer.Editor
{
    public static class CR112Release
    {
        public const string Version="0.20.0-review1";
        public static void Build(string tag,string version=Version,string evidence="Docs/CR112-118",string checkpoint="16ccff01e05c666d312c02df443dd4b22564f7a1")
        {
            if(Application.isPlaying||UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)throw new Exception("Saved edit mode required");
            string dir=tag=="release"?"Builds/Racer-"+version+"-Windows":"Builds/CR112-"+tag;Directory.CreateDirectory(dir);PlayerSettings.bundleVersion=version;
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=DiscoveryAuthoring.CR112Scenes,locationPathName=dir+"/Racer.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.None});
            File.WriteAllText(evidence+"/build-"+tag+".txt",report.summary.result+" errors="+report.summary.totalErrors+" warnings="+report.summary.totalWarnings);
            if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new Exception("Build failed");
            File.WriteAllText(dir+"/VERSION.txt","Racer "+version+" / Woodstock Rush\nReview evidence "+evidence+"\nSafety checkpoint "+checkpoint+"\n");
            Directory.CreateDirectory(dir+"/Licenses");foreach(var p in Directory.GetFiles("Assets/Plugins/LocalRadio","*.txt"))File.Copy(p,dir+"/Licenses/"+Path.GetFileName(p),true);
            foreach(var p in new[]{"LICENSE.txt","REVERSE-WILDLIFE-NOTICE.txt"})File.Copy("Assets/Audio/Wildlife/"+p,dir+"/Licenses/Wildlife-"+p,true);
        }
    }
}
