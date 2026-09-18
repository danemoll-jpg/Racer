using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Racer.Editor
{
    public static class Phase8Review
    {
        public const string Dir = "Docs/Phase8";
        static void Guard()
        {
            if (Application.isPlaying || SceneManager.GetActiveScene().isDirty || SceneManager.GetActiveScene().path != StreetLoopBuilder.ScenePath)
                throw new InvalidOperationException("Open saved StreetLoopGreybox outside Play mode.");
            Directory.CreateDirectory(Dir);
        }
        public static void Build(string label)
        {
            Guard(); string folder = "Builds/Phase8" + (label == "before" ? "Baseline" : "");
            Directory.CreateDirectory(folder);
            var r = BuildPipeline.BuildPlayer(new BuildPlayerOptions { scenes = new[] { StreetLoopBuilder.ScenePath }, locationPathName = folder + "/Racer.exe", target = BuildTarget.StandaloneWindows64, options = BuildOptions.Development });
            File.WriteAllText(Dir + "/build-" + label + ".txt", $"{r.summary.result}; errors={r.summary.totalErrors}; warnings={r.summary.totalWarnings}; bytes={r.summary.totalSize}; duration={r.summary.totalTime}\n" + string.Join("\n", r.steps.SelectMany(s => s.messages).Where(m => m.type == LogType.Error || m.type == LogType.Warning).Select(m => m.content)));
        }
        public static void Capture(string label)
        {
            Guard(); var cam = Camera.main; var car = Object.FindAnyObjectByType<ArcadeVehicle>();
            var cp = cam.transform.position; var cq = cam.transform.rotation; var vp = car.transform.position; var vq = car.transform.rotation;
            var chase = cam.GetComponent<ChaseCamera>(); bool enabled = chase.enabled;
            var road = StreetLoopBuilder.Route(); var cut = Phase5Setup.Path();
            var probes = new[] { ("road", road, Phase5Setup.Closest(road,new Vector3(470,0,30))), ("commercial",road,Phase5Setup.Closest(road,new Vector3(-300,0,530))), ("forest",road,Phase5Setup.Closest(road,new Vector3(350,0,-450))), ("shortcut",cut,40), ("jump",road,Phase5Setup.Closest(road,new Vector3(-625,0,-170))) };
            try
            {
                chase.enabled = false;
                foreach (var (name,path,i) in probes)
                {
                    var f = Vector3.ProjectOnPlane(path[i+1]-path[i],Vector3.up).normalized;
                    car.transform.SetPositionAndRotation(path[i]+Vector3.up*.7f,Quaternion.LookRotation(f)); chase.Snap();
                    foreach (var size in new[] { new Vector2Int(1920,1080), new Vector2Int(1280,720) })
                    {
                        var rt = new RenderTexture(size.x,size.y,24); var old = cam.targetTexture; var active=RenderTexture.active;
                        var tex = new Texture2D(size.x,size.y,TextureFormat.RGB24,false);
                        try {cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,size.x,size.y),0,0);tex.Apply();File.WriteAllBytes($"{Dir}/{label}-{name}-{size.x}.png",tex.EncodeToPNG());}
                        finally {cam.targetTexture=old;RenderTexture.active=active;Object.DestroyImmediate(tex);Object.DestroyImmediate(rt);}
                    }
                }
            }
            finally {car.transform.SetPositionAndRotation(vp,vq);cam.transform.SetPositionAndRotation(cp,cq);chase.enabled=enabled;}
        }
        public static void Routes()
        {
            Guard();var road=StreetLoopBuilder.Route();var cut=Phase5Setup.Path();
            WoodlandBenchmark.Route Segment(string name,Vector3 p,float speed) {int k=Phase5Setup.Closest(road,p);return new WoodlandBenchmark.Route{name=name,speed=speed,points=Enumerable.Range(0,500).Select(i=>road[(k+i)%road.Count]).ToArray()};}
            var routes=new[]{Segment("road",new Vector3(470,0,30),18),Segment("commercial",new Vector3(-400,0,530),18),Segment("forest",new Vector3(350,0,-450),18),new WoodlandBenchmark.Route{name="shortcut",speed=14,points=cut.ToArray()}};
            File.WriteAllText(Dir+"/routes.json",JsonUtility.ToJson(new WoodlandBenchmark.Plan{seconds=24,routes=routes},true));
        }
    }
}
