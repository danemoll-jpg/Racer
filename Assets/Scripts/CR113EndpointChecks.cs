using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
namespace Racer
{
    public sealed partial class CR112Validation
    {
        IEnumerator HighwayEndpoints()
        {
            race.opponents=false;race.traffic=true;flow.StartFreeRoam();car.enabled=false;car.GetComponent<VehicleInput>().enabled=false;
            foreach(string side in new[]{"west","east"}){var root=GameObject.Find("Hwy 92 "+side).transform;Place(root.TransformPoint(new Vector3(2,0,410)),root.forward);float start=Time.time,minUp=1,maxAir=0,air=0;int traffic=0;
                using(var w=new StreamWriter(dir+"/"+side+"-outer.csv")){w.WriteLine("time,x,y,z,speed,wheels,up");while(root.InverseTransformPoint(car.Body.position).z<480&&Time.time-start<20){var local=root.InverseTransformPoint(car.Body.position);float yaw=Vector3.SignedAngle(Vector3.ProjectOnPlane(car.transform.forward,Vector3.up),root.forward,Vector3.up);car.Simulate(Mathf.Clamp01((12-car.ForwardSpeed)*.7f),car.ForwardSpeed>13?.3f:0,Mathf.Clamp(yaw*.045f+(2-local.x)*.1f,-.3f,.3f),Time.fixedDeltaTime);yield return new WaitForFixedUpdate();minUp=Math.Min(minUp,car.transform.up.y);air=car.GroundedWheels<2?air+Time.fixedDeltaTime:0;maxAir=Math.Max(maxAir,air);traffic=Math.Max(traffic,race.Drivers.Count(d=>d.HighwayTraffic&&Vector3.Distance(d.transform.position,root.TransformPoint(new Vector3(0,0,435)))<100));var p=car.Body.position;w.WriteLine($"{Time.time-start},{p.x},{p.y},{p.z},{car.ForwardSpeed},{car.GroundedWheels},{car.transform.up.y}");}}
                Check(root.InverseTransformPoint(car.Body.position).z>=480&&minUp>.9f&&maxAir<.3f,$"{side} outer endpoint normal drive: minUp={minUp:F3}, max unsupported={maxAir:F3}s; traffic near changed endpoint={traffic}");Shot(side+"-outer-after",root.TransformPoint(new Vector3(35,65,430)),root.TransformPoint(new Vector3(0,side=="east"?33.8f:0,445)));
            }
        }
    }
}
