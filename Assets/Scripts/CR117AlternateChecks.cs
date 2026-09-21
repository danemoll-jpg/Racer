using System;
using System.IO;
using System.Linq;
using System.Collections;
using UnityEngine;
namespace Racer
{
    public sealed partial class CR112Validation
    {
        IEnumerator Alternates()
        {
            race.opponents=race.traffic=false;car.GetComponent<VehicleConfiguration>().Apply("moto");
            foreach(var branch in race.Branches.Where(b=>Arg("-branch")==""||b.title.Contains(Arg("-branch")))){
                flow.StartRace();while(flow.State==RaceFlow.Stage.Countdown)yield return null;
                ThreeFeatureValidation.CaptureUi(dir+"/start-finish-clearance.png");
                foreach(var b in race.Branches)b.aiValidated=b==branch;
                var pilot=car.gameObject.AddComponent<RoadDriver>();pilot.Initialize(race,car,true,1,1);pilot.Racer=race.Racers[0];car.GetComponent<VehicleInput>().enabled=false;pilot.Place(branch.entryRoad-40,0);race.ResetSampling(car.Body.position,race.Clock);
                // This is a local segment, not a claimed full lap. Seed only the prior
                // gate context; entrance, bypass and rejoin are then physically driven.
                race.Progress.Cross(0,true,race.Clock);float prior=race.road.Relative(branch.entryRoad-40,race.Origin);for(int g=1;g<race.gates.Length;g++)if(race.road.Relative(race.road.Project(race.gates[g].transform.position,out _),race.Origin)<prior)race.Progress.Cross(g,true,race.Clock);
                float start=Time.time,best=0;bool entered=false,rejoined=false;string tag=branch.title.Replace(' ','-');
                using(var w=new StreamWriter(dir+"/"+tag+".csv")){w.WriteLine("time,x,y,z,speed,branchStation,activeBranch,misses,recoveries");while(Time.time-start<130){yield return new WaitForFixedUpdate();float s=branch.Project(car.Body.position,out float lateral);if(race.Racers[0].Branch.Route==branch){entered=true;best=Math.Max(best,s);}var p=car.Body.position;w.WriteLine($"{Time.time-start},{p.x},{p.y},{p.z},{car.ForwardSpeed},{s},{race.Racers[0].Branch.Route==branch},{race.Progress.MissedGates},{pilot.RecoveryCount}");if(entered&&best>branch.Length-15&&race.Racers[0].Branch.Route==null){rejoined=true;break;}if(pilot.RecoveryCount>0)break;}}
                Check(entered&&rejoined&&pilot.RecoveryCount==0&&race.Progress.MissedGates==0,$"{branch.title}: normal-input entrance/traverse/rejoin; best={best:F1}/{branch.Length:F1}m, misses={race.Progress.MissedGates}, recoveries={pilot.RecoveryCount}");ThreeFeatureValidation.CaptureUi(dir+"/"+tag+"-rejoin.png");pilot.enabled=false;Destroy(pilot);branch.aiValidated=false;flow.Pause();flow.QuitRace();yield return null;
            }
            if(Arg("-skipRecovery")=="yes")yield break;
            flow.StartRace();while(flow.State==RaceFlow.Stage.Countdown)yield return null;var road=race.road;float station=road.Project(race.gates[1].transform.position,out _)+15;var p0=road.At(station,out var f);Place(p0,f);var recovery=car.GetComponent<VehicleRespawn>();recovery.SeedCoursePosition(p0);recovery.RecordSafePosition();car.Body.position=p0+Vector3.Cross(Vector3.up,f).normalized*12+Vector3.up*.8f;car.transform.position=car.Body.position;car.Body.rotation=Quaternion.LookRotation(f)*Quaternion.Euler(0,0,100);Physics.SyncTransforms();bool requested=recovery.TryRecoverLocal(true);yield return new WaitForSeconds(2);Check(requested&&car.transform.up.y>.8f&&Vector3.Distance(car.Body.position,p0)<100,$"{race.courseName}: local recovery near current route position; {recovery.LastRecovery}");ThreeFeatureValidation.CaptureUi(dir+"/local-recovery.png");
        }
    }
}
