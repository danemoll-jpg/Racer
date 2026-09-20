using System;
using System.IO;
using System.Linq;
using UnityEngine;
namespace Racer
{
    // Observer for ordinary-frame multi-lap validation; does not drive or move a car.
    [DefaultExecutionOrder(2000)] public sealed class GhostValidation:MonoBehaviour
    {
        static string Arg(string key,string fallback){var a=Environment.GetCommandLineArgs();int i=Array.IndexOf(a,key);return i>=0&&i+1<a.Length?a[i+1]:fallback;}
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot(){if(Arg("-ghostValidation","")==""||Arg("-racerTestSave","")=="")return;var g=new GameObject("Clean lap ghost observer");DontDestroyOnLoad(g);g.AddComponent<GhostValidation>();}
        RaceFlow flow;int lap=-1;bool enabledGhost,captured;string path;float nextStorage;
        void LateUpdate()
        {
            if(!flow)flow=FindAnyObjectByType<RaceFlow>();if(!flow||flow.Ghost==null)return;
            if(path==null){path=Arg("-evidence","Docs/CR081-090/ghost");Directory.CreateDirectory(path);}
            if(!enabledGhost){if(!flow.Ghost.Enabled)flow.Ghost.Toggle();enabledGhost=true;}
            var p=flow.Race.Progress;
            if(p.CompletedLaps!=lap){lap=p.CompletedLaps;File.AppendAllText(path+"/ghost-events.txt",$"Lap={lap} misses={p.MissedGates} {flow.Ghost.Status}\n");}
            var visual=GameObject.Find("Personal best clean-lap ghost / visual only");
            if(!captured&&visual){captured=true;ThreeFeatureValidation.CaptureUi(path+"/ghost-replay.png");StartCoroutine(PauseCheck(visual));}
            var dir=Path.Combine(flow.Save.DirectoryPath,"CleanLapGhosts");
            if(Time.unscaledTime>=nextStorage&&Directory.Exists(dir))
            {
                nextStorage=Time.unscaledTime+1;
                var files=Directory.GetFiles(dir,"*.json");if(files.Length>0)File.WriteAllLines(path+"/ghost-storage.txt",files.Select(f=>{var data=JsonUtility.FromJson<CleanLapGhost.Lap>(File.ReadAllText(f));return data.key+" seconds="+data.seconds+" samples="+data.poses.Count+" first="+data.poses[0].t+" last="+data.poses[^1].t+" bytes="+new FileInfo(f).Length;}));
            }
        }
        System.Collections.IEnumerator PauseCheck(GameObject visual)
        {
            var before=visual.transform.position;double time=Time.timeAsDouble;flow.Pause();yield return new WaitForSecondsRealtime(.15f);
            bool frozen=System.Math.Abs(Time.timeAsDouble-time)<.0001&&(visual.transform.position-before).sqrMagnitude<.000001f;
            File.WriteAllText(path+"/ghost-replay-checks.txt",(frozen?"PASS ":"FAIL ")+"Pause freezes lap time and ghost pose\n"+(visual.GetComponentsInChildren<Collider>().Length==0&&visual.GetComponentsInChildren<Rigidbody>().Length==0?"PASS ":"FAIL ")+"Ghost has no colliders or rigidbodies\n");
            flow.Resume();
        }
    }
}
