using System;
using System.IO;
using System.Linq;
using System.Collections;
using UnityEngine;
namespace Racer
{
    public sealed partial class CR112Validation
    {
        IEnumerator TurkeyVisits()
        {
            race.opponents=race.traffic=false;var street=race.ambientRoad?race.ambientRoad:race.road;float station=street.Project(GameObject.Find("Dan - blue X").transform.position,out _);var wildlife=race.GetComponent<Wildlife>();bool seen=false,absent=false;
            for(int visit=0;visit<6&&!(seen&&absent);visit++){
                flow.StartFreeRoam();car.GetComponent<VehicleInput>().enabled=false;var pilot=car.gameObject.AddComponent<RoadDriver>();pilot.Initialize(race,car,false,1,1);pilot.Place(station-70,0);float start=Time.time,travel=0;var previous=car.Body.position;
                while(travel<65&&Time.time-start<16){yield return new WaitForFixedUpdate();travel+=Vector3.ProjectOnPlane(car.Body.position-previous,Vector3.up).magnitude;previous=car.Body.position;}
                pilot.enabled=false;Destroy(pilot);car.enabled=false;car.Body.linearVelocity=car.Body.angularVelocity=Vector3.zero;
                var birds=wildlife.GetComponentsInChildren<Transform>().Where(t=>t.name=="Wildlife Turkey").ToArray();bool present=birds.Length>0;seen|=present;absent|=!present;
                Check(travel>=65&&AmbientLife.ForcedSeed==0&&birds.Length==(flow.Save.Settings.households.lastTurkeyVisit<2?3:0),$"Natural Kyle visit {visit+1}: schedule={flow.Save.Settings.households.lastTurkeyVisit}, active turkeys={birds.Length}");
                var target=wildlife.habitats.Where(h=>h.species==Wildlife.Species.Turkey).ElementAt(1).position+Vector3.up*.7f;Shot(present?"turkeys-present":"turkeys-absent",car.Body.position+Vector3.up*2.2f,target);
                if(present)Check(birds.All(b=>Math.Abs(b.position.y-Ground(b.position))<.15f),"Naturally selected turkey group has terrain support");
                File.AppendAllText(dir+"/visits.txt",$"visit={visit+1} car={car.Body.position} birds={string.Join(";",birds.Select(b=>b.position))} schedule={JsonUtility.ToJson(flow.Save.Settings.households)}\n");flow.Pause();flow.QuitRace();yield return null;
            }
            Check(seen&&absent,"Natural saved schedule reached both turkey-present and turkey-absent visits; inspect street-view captures for actual visibility");
        }
    }
}
