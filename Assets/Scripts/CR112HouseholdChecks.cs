using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Racer
{
    public sealed partial class CR112Validation
    {
        IEnumerator Households()
        {
            race.opponents=race.traffic=false;car.GetComponent<VehicleConfiguration>().Apply("moto");var street=race.ambientRoad?race.ambientRoad:race.road;street.Initialize();var dan=GameObject.Find("Dan - blue X").transform;
            float station=street.Project(dan.position,out _);var life=race.GetComponent<AmbientLife>();var wildlife=race.GetComponent<Wildlife>();var seen=new HashSet<string>();bool turkeyPresent=false,turkeyAbsent=false;
            var camp=GameObject.Find("Permanent mountainside camp / two seated guys");
            for(int visit=0;visit<6;visit++){
                flow.StartFreeRoam();car.GetComponent<VehicleInput>().enabled=false;
                var pilot=car.gameObject.AddComponent<RoadDriver>();pilot.Initialize(race,car,false,1,1);pilot.Place(station-70,0);
                // Ordinary saved selection only: no forced seed/state or activation calls.
                float start=Time.time,travel=0;var previous=car.Body.position;while(travel<65&&Time.time-start<16){yield return new WaitForFixedUpdate();travel+=Vector3.ProjectOnPlane(car.Body.position-previous,Vector3.up).magnitude;previous=car.Body.position;}
                Check(travel>=65,"Ordinary street approach covers 65 metres");
                pilot.enabled=false;Destroy(pilot);car.enabled=false;car.Body.linearVelocity=Vector3.zero;car.Body.angularVelocity=Vector3.zero;
                string group=life.DanScene==0?"football":life.DanScene==1?"coffee":life.FriendScene?"smokers":"empty";seen.Add(group);
                var selected=life.DanScene==0?life.football:life.DanScene==1?life.coffee:life.FriendScene?life.smoking:Array.Empty<Vector3>();
                var residents=life.GetComponentsInChildren<Transform>().Where(t=>t.name=="Ambient resident").ToArray();var all=life.football.Concat(life.coffee).Concat(life.smoking).ToArray();int count=residents.Count(t=>all.Any(p=>Vector3.Distance(t.position,p)<.15f));
                Check(AmbientLife.ForcedSeed==0&&count==selected.Length,$"Natural visit {visit+1}: {group}, household count={count}");
                Check(camp&&camp.activeInHierarchy&&camp.GetComponentsInChildren<Transform>().Count(t=>t.name=="Seated guy")==2,$"Visit {visit+1}: independent two campers remain present");
                if(selected.Length>0){var center=selected.Aggregate(Vector3.zero,(a,p)=>a+p)/selected.Length;Shot("visit-"+(visit+1)+"-"+group,car.Body.position+Vector3.up*2.2f,center+Vector3.up);Check(selected.All(p=>Math.Abs(p.y-Ground(p))<.1f),group+" current ground support");}
                else Shot("visit-"+(visit+1)+"-empty",car.Body.position+Vector3.up*2.2f,dan.position+dan.forward*25);
                var turkeys=wildlife.GetComponentsInChildren<Transform>().Where(t=>t.name=="Wildlife Turkey").ToArray();bool present=turkeys.Length>0;turkeyPresent|=present;turkeyAbsent|=!present;
                var sites=wildlife.habitats.Where(h=>h.species==Wildlife.Species.Turkey).ToArray();if(sites.Length>0){var target=sites[1].position+Vector3.up*.7f;Shot("visit-"+(visit+1)+"-turkeys-"+(present?"present":"absent"),car.Body.position+Vector3.up*2.2f,target);}
                File.AppendAllText(dir+"/natural-visits.txt",$"visit={visit+1} group={group} turkeys={turkeys.Length} car={car.Body.position} schedule={JsonUtility.ToJson(flow.Save.Settings.households)}\n");
                flow.Pause();flow.QuitRace();yield return null;
            }
            Check(seen.SetEquals(new[]{"football","coffee","smokers","empty"}),"Unforced saved visit sequence reaches all three household groups and empty");Check(turkeyPresent&&turkeyAbsent,"Natural turkey schedule includes present and absent visits");
            if(camp)Shot("independent-camp",camp.transform.position+new Vector3(9,5,9),camp.transform.position+Vector3.up);
        }
    }
}
