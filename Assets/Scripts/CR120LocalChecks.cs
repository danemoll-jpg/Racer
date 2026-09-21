using System;
using System.IO;
using System.Linq;
using System.Collections;
using UnityEngine;
namespace Racer
{
    public sealed partial class CR112Validation
    {
        IEnumerator ReverseLocal()
        {
            race.opponents=race.traffic=false;car.GetComponent<VehicleConfiguration>().Apply("moto");flow.StartFreeRoam();car.enabled=false;car.GetComponent<VehicleInput>().enabled=false;
            var branch=race.Branches.Single(b=>b.title=="Downhill Ridge Cut");
            foreach(var segment in new[]{"summit-approach","ridge-main"}){
                bool alternate=segment=="ridge-chicane";float from=alternate?0:segment=="summit-approach"?395:branch.entryRoad-30;
                float to=alternate?branch.Length:segment=="summit-approach"?495:branch.entryRoad+55;
                Vector3 At(float s,out Vector3 heading){var p=alternate?branch.At(s,out heading):race.road.At(s,out heading);if(alternate){float offset=-1.6f*Mathf.Exp(-Mathf.Pow((s-38)/10,2))+1.6f*Mathf.Exp(-Mathf.Pow((s-63)/10,2));p+=Vector3.Cross(Vector3.up,heading).normalized*offset;}return p;}
                var p0=At(from,out var f);Place(p0,f);float start=Time.time,best=from,minUp=1;ThreeFeatureValidation.CaptureUi(dir+"/"+segment+"-approach.png");
                using(var w=new StreamWriter(dir+"/"+segment+".csv")){
                    w.WriteLine("seconds,x,y,z,speed,station,lateral,up,wheels");
                    while(Time.time-start<24){
                        float s=alternate?branch.Project(car.Body.position,out _):race.road.Project(car.Body.position,out _);best=Math.Max(best,s);if(s>=to-3)break;
                        var target=At(Math.Min(s+5,to),out _);var local=car.transform.InverseTransformPoint(target);float steering=Mathf.Clamp(Mathf.Atan2(local.x,local.z)*2,-1,1);
                        car.Simulate(Mathf.Clamp01((7-car.ForwardSpeed)*.6f),car.ForwardSpeed>8?.4f:0,steering,Time.fixedDeltaTime);yield return new WaitForFixedUpdate();minUp=Math.Min(minUp,car.transform.up.y);
                        var p=car.Body.position;w.WriteLine($"{Time.time-start},{p.x},{p.y},{p.z},{car.ForwardSpeed},{s},0,{car.transform.up.y},{car.GroundedWheels}");
                        if(car.transform.up.y<.4f)break;
                    }
                }
                Check(best>=to-3&&minUp>.65f,$"{segment}: one normal-input motorcycle segment; reached={best:F1}/{to:F1}, minUp={minUp:F2}. No AI/full lap.");ThreeFeatureValidation.CaptureUi(dir+"/"+segment+"-end.png");
            }
        }
    }
}
