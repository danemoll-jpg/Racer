using System;
using System.IO;
using System.Linq;
using System.Collections;
using UnityEngine;
namespace Racer
{
    public sealed partial class CR112Validation
    {
        IEnumerator RoadSurfaces()
        {
            race.opponents=false;race.traffic=true;car.GetComponent<VehicleConfiguration>().Apply("moto");flow.StartFreeRoam();car.GetComponent<VehicleInput>().enabled=false;
            foreach(bool east in new[]{false,true}){
                string tag=east?"east":"west";var before=east?new Vector3(175,9,550):new Vector3(-810,8,539);var after=east?new Vector3(505,25,561):new Vector3(-490,8,538);float startStation=race.throughRoad.Project(before,out _),endStation=race.throughRoad.Project(after,out _);
                var pilot=car.gameObject.AddComponent<RoadDriver>();pilot.Initialize(race,car,false,1,1);pilot.HighwayTraffic=true;pilot.Place(startStation,2.05f);float begin=Time.time,minUp=1,maxAir=0,air=0;int trafficSeen=0;
                using(var w=new StreamWriter(dir+"/"+tag+"-drive.csv")){w.WriteLine("time,x,y,z,speed,wheels,up,trafficNearJoin");while(race.throughRoad.Project(car.Body.position,out _)<endStation&&Time.time-begin<35){yield return new WaitForFixedUpdate();minUp=Math.Min(minUp,car.transform.up.y);air=car.GroundedWheels<2?air+Time.fixedDeltaTime:0;maxAir=Math.Max(maxAir,air);int visible=race.Drivers.Count(d=>d.HighwayTraffic&&Math.Abs(d.transform.position.x-(east?310:-640))<100);trafficSeen=Math.Max(trafficSeen,visible);var p=car.Body.position;w.WriteLine($"{Time.time-begin},{p.x},{p.y},{p.z},{car.ForwardSpeed},{car.GroundedWheels},{car.transform.up.y},{visible}");}}
                Check(race.throughRoad.Project(car.Body.position,out _)>endStation-2&&minUp>.7f&&maxAir<.5f,$"{tag} seam normal drive: minimum up={minUp:F3}, maximum unsupported interval={maxAir:F3}s");Check(trafficSeen>0,tag+" through-traffic observed at changed join");pilot.enabled=false;Destroy(pilot);car.enabled=false;
                Shot(tag+"-after",east?new(400,60,495):new(-600,42,490),east?new(361,8,561):new(-651,8,539));
            }
            race.traffic=false;flow.StartFreeRoam();car.enabled=false;car.GetComponent<VehicleInput>().enabled=false;var drive=GameObject.Find("Kyle descending driveway").GetComponent<RaceRoad>();var f=(drive.points[1]-drive.points[0]).normalized;var points=new[]{drive.points[0]-f*8,drive.points[0]+f*2,drive.points[1],drive.points[2]};Place(points[0],f);int target=1;float started=Time.time,lowest=1;
            while(target<points.Length&&Time.time-started<18){var local=car.transform.InverseTransformPoint(points[target]);float steer=Mathf.Clamp(Mathf.Atan2(local.x,local.z)*2,-1,1);car.Simulate(Mathf.Clamp01((10-car.ForwardSpeed)*.7f),car.ForwardSpeed>11?.3f:0,steer,Time.fixedDeltaTime);yield return new WaitForFixedUpdate();lowest=Math.Min(lowest,car.transform.up.y);if(Vector3.ProjectOnPlane(car.Body.position-points[target],Vector3.up).magnitude<3)target++;}
            Check(target==points.Length&&lowest>.7f,"Normal drive over repaired frontage driveway edge join");Shot("frontage-overhead-after",new(474,160,-12),new(474,0,-12));Shot("frontage-street-after",new(474,87,-13),drive.points[1]+Vector3.up);
        }
    }
}
