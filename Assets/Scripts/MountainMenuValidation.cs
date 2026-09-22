using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Racer
{
    // Explicit isolated startup check only; inactive in normal play.
    public sealed class MountainMenuValidation : MonoBehaviour
    {
        static string Arg(string key) { var a=Environment.GetCommandLineArgs(); int i=Array.IndexOf(a,key); return i>=0&&i+1<a.Length?a[i+1]:""; }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            if(Arg("-mountainMenuCheck")==""||Arg("-racerTestSave")==""||FindAnyObjectByType<MountainMenuValidation>())return;
            var go=new GameObject("Mountain menu startup check"); DontDestroyOnLoad(go); go.AddComponent<MountainMenuValidation>();
        }
        string output; bool failed;
        void OnError(string message,string stack,LogType type)
        {
            if(type!=LogType.Exception&&type!=LogType.Error&&type!=LogType.Assert)return;
            failed=true; File.AppendAllText(output,"FAIL: "+message+"\n"+stack+"\n");
        }
        void Click(string label,bool prefix=false)
        {
            var button=FindObjectsByType<UnityEngine.UI.Button>().Single(b=>b.gameObject.activeInHierarchy&&b.GetComponentsInChildren<UnityEngine.UI.Text>().Any(t=>prefix?t.text.StartsWith(label):t.text==label));
            if(!button.IsInteractable())throw new Exception("Menu entry disabled: "+label);
            button.Select(); button.onClick.Invoke();
        }
        IEnumerator Start()
        {
            output=Path.Combine(Arg("-racerTestSave"),"mountain-menu.txt");
            Application.logMessageReceived+=OnError; Application.runInBackground=true;
            yield return null; yield return null;
            bool reverse=Arg("-mountainMenuCheck")=="reverse";
            string scene=reverse?"MountainLoopReverse":"MountainLoop";
            Click("Track: ",true); yield return null;
            Click(reverse?"Mountain Loop Reverse":"Mountain Loop");
            yield return null; yield return null;
            var race=FindAnyObjectByType<RaceDirector>();
            if(SceneManager.GetActiveScene().name!=scene||!race||!race.road||!race.vehicle||!race.vehicle.Body||race.Flow.State!=RaceFlow.Stage.Ready)
                throw new Exception("Wrong scene or missing race startup dependency: "+scene);
            Click("Start race");
            if(race.Flow.State!=RaceFlow.Stage.Countdown)throw new Exception("Race failed to start countdown");
            var spawn=race.vehicle.GetComponent<VehicleRespawn>().spawnPoint;
            if(!spawn)throw new Exception("Missing player spawn reference");
            var expected=spawn.position;
            if(race.opponents){float origin=race.road.Project(race.gates[0].transform.position,out _);expected=race.road.At(origin-32,out var direction)-Vector3.Cross(Vector3.up,direction).normalized*2.2f+Vector3.up*Mathf.Max(.4f,race.vehicle.suspensionLength-Physics.gravity.magnitude/race.vehicle.springStrength);}
            if(Vector3.Distance(race.vehicle.Body.position,expected)>1)throw new Exception("Player not at race starting grid");
            yield return new WaitForSecondsRealtime(4);
            if(race.Flow.State!=RaceFlow.Stage.Racing)throw new Exception("Race failed to reach GO");
            File.AppendAllText(output,$"{(failed?"FAIL":"PASS")}: menu -> {scene}; {race.courseName}; player at authored spawn; countdown -> Racing\n");
            Application.Quit(failed?1:0);
        }
        void OnDestroy(){Application.logMessageReceived-=OnError;}
    }
}
