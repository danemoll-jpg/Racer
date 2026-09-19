using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace Racer
{
    public sealed class ForestChecks:MonoBehaviour
    {
        readonly List<string> checks=new();
        void Check(bool pass,string label){checks.Add((pass?"PASS ":"FAIL ")+label);File.WriteAllLines("Docs/CR056/rules.txt",checks);}
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot(){var args=Environment.GetCommandLineArgs();if(!args.Contains("-forestChecks")||!args.Contains("-racerTestSave")||FindAnyObjectByType<ForestChecks>())return;var g=new GameObject("Forest rules");DontDestroyOnLoad(g);g.AddComponent<ForestChecks>();}
        IEnumerator Start()
        {
            Application.runInBackground=true;if(SceneManager.GetActiveScene().name!="LakeWoods")SceneManager.LoadScene("LakeWoods");yield return null;yield return null;
            Directory.CreateDirectory("Docs/CR056");var r=FindAnyObjectByType<RaceDirector>();var flow=r.Flow;
            Check(r.Forest&&r.courseName=="Forest Loop","Forest name and version");
            Check(r.EligibleVehicles.Select(p=>p.Id).SequenceEqual(new[]{"moto","atv"}),"Forest eligible player / AI pool");
            flow.OpenGarage();flow.SelectVehicle("tourer");Check(r.vehicle.GetComponent<VehicleConfiguration>().profileId=="moto","Ineligible saved/manual vehicle coerced to motorcycle");flow.CloseGarage();
            flow.OpenRoster();flow.Save.Settings.opponentChoices=new[]{"original","random","mixed"};flow.ResolveRoster();Check(r.opponentRoster.All(id=>VehicleProfile.Find(id).Small),"Fixed/random/mixed AI choices remain eligible");flow.CloseGarage();
            var board=new RecordBoards(Path.GetFullPath("Temp/forest-board-rules"));
            Check(board.Add("bad"+Guid.NewGuid(),"lake-v2-forest-original-solo-clear-laps3",true,100,"original")==0,"New Forest category rejects car entries");
            string token=Guid.NewGuid().ToString();board.Add(token,"lake-v1-original-solo-clear-laps3",true,100,"original");Check(board.Board("lake-v1-original-solo-clear-laps3",true).Any(e=>e.id==token),"Historical v1 car boards retained");
            Check(RecordBoards.Describe("lake-v1-original-solo-clear-laps3").Contains("historical"),"Historical record labeling is explicit");
            var layout=FindAnyObjectByType<ForestLayout>();Check(layout.jumpStarts.Length>=6,"Six distinct main jumps authored");
            var branch=r.Branches.Single();Check(branch.bypassedGates.Length>0,"Cave grants explicit bypass entitlement");
            Check(branch.Length<branch.exitRoad-branch.entryRoad,"Cave distance is shorter than bypassed main section");
            Check(r.gates.Skip(1).All(g=>{float s=r.road.Project(g.transform.position,out _);return !layout.IsLaunch(s)&&Mathf.Abs(s-branch.entryRoad)>65&&Mathf.Abs(s-branch.exitRoad)>65;}),"Mandatory gates outside launch/landing and cave mouths");
            Check(r.ambientRoad&&r.ambientRoad!=r.road&&!r.ambientRoad.forestTrail,"Independent preserved street route for ambient traffic");
            r.opponents=r.traffic=true;flow.StartRace();while(flow.State!=RaceFlow.Stage.Racing)yield return null;
            Check(r.Racers.All(s=>s.Car.GetComponent<VehicleConfiguration>().Profile.Small),"Spawned Forest roster contains motorcycles / ATVs only");
            Check(r.Drivers.Where(d=>d.GetComponent<AmbientVehicle>()).All(d=>d.DriveRoad==r.ambientRoad),"Every spawned civilian selects street route");
            foreach(var driver in r.Drivers.Where(d=>d.GetComponent<AmbientVehicle>())){driver.Place(700,2.6f);r.ambientRoad.Project(driver.transform.position,out var lateral);Check(lateral<4,"Civilian placement uses actual street: "+driver.name);}
            ThreeFeatureValidation.CaptureUi("Docs/CR056/gameplay-grid.png");
            // Exercise production recovery/recycling with isolated, off-screen street fixtures.
            // The methods retain their ordinary distance, visibility and occupancy guards.
            var civilians=r.Drivers.Where(d=>d.GetComponent<AmbientVehicle>()).ToArray();
            var recover=typeof(RoadDriver).GetMethod("TryRecover",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic);
            var recycle=typeof(RoadDriver).GetMethod("TryRecycleHighway",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic);
            foreach(var driver in civilians)
            {
                int before=driver.RecoveryCount;
                for(float station=400;station<r.ambientRoad.Length-50&&driver.RecoveryCount==before;station+=180)
                {driver.Place(station,2.6f);recover.Invoke(driver,new object[]{station});}
                r.ambientRoad.Project(driver.transform.position,out float lateral);
                Check(driver.RecoveryCount>before&&lateral<8,"Civilian guarded local recovery stays on street: "+driver.name);
                int recycled=driver.HighwayRecycles;
                driver.Place(driver.Direction>0?4750:3600,2.6f);recycle.Invoke(driver,null);
                r.ambientRoad.Project(driver.transform.position,out lateral);
                Check(driver.HighwayRecycles>recycled&&lateral<8,"Civilian guarded highway recycling stays on street: "+driver.name);
                driver.Place(700+Array.IndexOf(civilians,driver)*120,2.6f);
            }
            var falls=new List<string>{"profile,initial_up,recovered,final_up,supported,gate_preserved,penalty_preserved,earned_preserved"};
            foreach(string id in new[]{"moto","atv"})
            {
                var car=r.vehicle;car.GetComponent<VehicleConfiguration>().Apply(id);car.enabled=false;car.GetComponent<VehicleInput>().enabled=false;
                var state=r.Racers[0];state.Branch.Begin(branch);
                for(float s=2;s<=200;s+=2)state.Branch.Advance(branch.At(s-2,out _),branch.At(s,out var heading),heading);
                var point=branch.At(200,out var forward);var rotation=Quaternion.LookRotation(Vector3.ProjectOnPlane(forward,Vector3.up))*Quaternion.Euler(0,0,110);
                car.Body.isKinematic=false;car.Body.position=point+Vector3.up*1.8f;car.Body.rotation=rotation;car.transform.SetPositionAndRotation(car.Body.position,rotation);car.Body.linearVelocity=car.Body.angularVelocity=Vector3.zero;Physics.SyncTransforms();
                r.ResetSampling(car.Body.position,r.Clock);
                float initialUp=car.transform.up.y,end=Time.time+2;
                while(Time.time<end){car.Simulate(0,0,0,Time.fixedDeltaTime);yield return new WaitForFixedUpdate();}
                int gate=state.Progress.NextGate;double penalty=state.Progress.PenaltySeconds;float earned=state.Branch.Earned;
                bool recovered=car.GetComponent<VehicleRespawn>().TryRecoverLocal(true);
                car.Simulate(0,0,0,Time.fixedDeltaTime);yield return new WaitForFixedUpdate();
                bool gateOk=state.Progress.NextGate==gate,penaltyOk=state.Progress.PenaltySeconds==penalty,earnedOk=state.Branch.Position<=earned+.1f;
                Check(initialUp<0&&recovered&&car.transform.up.y>.85f&&car.GroundedWheels>=2&&gateOk&&penaltyOk&&earnedOk,id+" physical cave topple fixture recovers locally without new credit or penalty");
                falls.Add($"{id},{initialUp:F3},{recovered},{car.transform.up.y:F3},{car.GroundedWheels},{gateOk},{penaltyOk},{earnedOk}");
            }
            File.WriteAllLines("Docs/CR056/falls.csv",falls);
            File.WriteAllText("Docs/CR056/rules-done.txt",$"{checks.Count(c=>c.StartsWith("PASS"))}/{checks.Count}");Application.Quit();
        }
    }
}

