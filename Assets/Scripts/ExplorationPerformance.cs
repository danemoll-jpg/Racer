using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Racer
{
    public sealed class ExplorationPerformance:MonoBehaviour
    {
        static string Arg(string key,string fallback){var a=Environment.GetCommandLineArgs();int i=Array.IndexOf(a,key);return i>=0&&i+1<a.Length?a[i+1]:fallback;}
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot(){if(Arg("-explorationPerformance","")==""||Arg("-racerTestSave","")=="")return;var go=new GameObject("Explicit expanded world performance probe");DontDestroyOnLoad(go);go.AddComponent<ExplorationPerformance>();}
        RaceDirector race;RaceRoad route;ArcadeVehicle car;bool driving;
        IEnumerator Start()
        {
            var course=Arg("-course","StreetLoopGreybox");if(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name!=course)UnityEngine.SceneManagement.SceneManager.LoadScene(course);yield return null;yield return null;
            race=FindAnyObjectByType<RaceDirector>();car=race.vehicle;route=race.GetComponent<ExplorationCollection>().routes[0];race.traffic=true;race.Flow.StartFreeRoam();car.enabled=false;car.GetComponent<VehicleInput>().enabled=false;
            if(Arg("-area","mountain")=="home")route=race.ambientRoad?race.ambientRoad:race.road; float station=Arg("-area","mountain")=="home"?route.Project(GameObject.Find("Dan - blue X").transform.position,out _):70; var p=route.At(station,out var f);var hit=Physics.RaycastAll(p+Vector3.up*40,Vector3.down,80).Where(h=>h.collider.name.StartsWith("Ground_")).OrderBy(h=>h.distance).First();p.y=hit.point.y+car.suspensionLength-.12f;car.Body.position=p;car.Body.rotation=Quaternion.LookRotation(f);car.transform.SetPositionAndRotation(p,car.Body.rotation);car.Body.linearVelocity=Vector3.zero;Physics.SyncTransforms();FindAnyObjectByType<ChaseCamera>().Snap();driving=true;
            Application.runInBackground=true;QualitySettings.vSyncCount=0;Application.targetFrameRate=120;Time.timeScale=1;var camera=Camera.main;var target=new RenderTexture(1280,720,24);camera.targetTexture=target;
            string dir=Arg("-evidence","Docs/CR081-090/performance");Directory.CreateDirectory(dir);var frames=new List<float>();float began=Time.realtimeSinceStartup;int rendered=0;
            while(Time.realtimeSinceStartup-began<75){yield return null;camera.Render();rendered++;if(Time.realtimeSinceStartup-began>10)frames.Add(Time.unscaledDeltaTime*1000);}
            frames.Sort();File.WriteAllText(dir+"/performance.txt",$"Expanded mountain physical drive, 75 seconds, 10-second warmup. Explicit 1280x720 camera rendering; timeScale=1; no concurrent test should run. Frames={rendered}, sampled={frames.Count}; median={frames[frames.Count/2]:F3} ms; p95={frames[(int)(frames.Count*.95)]:F3} ms; p99={frames[(int)(frames.Count*.99)]:F3} ms; managed={GC.GetTotalMemory(false)/1048576.0:F2} MiB. Traffic enabled={race.traffic}; active traffic={race.Drivers.Count}; ambient people components={FindObjectsByType<AmbientLife>().Length}; radio={race.Flow.Radio?.ChannelName}. Audio output is temporarily muted; normal preferences unchanged. These are observed offscreen frame times, not human acceptance.\n");
            camera.targetTexture=null;target.Release();Destroy(target);ThreeFeatureValidation.CaptureUi(dir+"/mountain-performance.png");File.WriteAllText(dir+"/done.txt","Complete");Application.Quit();
        }
        void FixedUpdate()
        {
            if(!driving)return;float s=route.Project(car.Body.position,out _);var aim=route.At(s+12,out _);var local=car.transform.InverseTransformPoint(aim);float steer=Mathf.Clamp(Mathf.Atan2(local.x,local.z)*2,-1,1);car.Simulate(Mathf.Clamp01((20-car.ForwardSpeed)*.4f),car.ForwardSpeed>22?.25f:0,steer,Time.fixedDeltaTime);
        }
    }
}

