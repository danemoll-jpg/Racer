using System;
using System.IO;
using System.Collections;
using UnityEngine;
namespace Racer
{
    public sealed partial class CR112Validation
    {
        IEnumerator ClimbSegment()
        {
            flow.Save.Settings.opponents=false;race.opponents=race.traffic=false;car.GetComponent<VehicleConfiguration>().Apply("moto");flow.StartRace();while(flow.State==RaceFlow.Stage.Countdown)yield return null;
            var pilot=car.gameObject.AddComponent<RoadDriver>();pilot.Initialize(race,car,true,1,1);pilot.Racer=race.Racers[0];car.GetComponent<VehicleInput>().enabled=false;float startStation=race.road.Project(new Vector3(1020,139,-60),out _)-30;pilot.Place(startStation, -1.5f);race.ResetSampling(car.Body.position,race.Clock);
            var contact=car.gameObject.AddComponent<CR117ContactLog>();contact.Path=dir+"/climb-contacts.txt";float began=Time.time,minUp=1;bool reached=false;
            using(var w=new StreamWriter(dir+"/climb.csv")){w.AutoFlush=true;w.WriteLine("time,x,y,z,speed,wheels,up,obstacle");while(Time.time-began<45){yield return new WaitForFixedUpdate();var p=car.Body.position;minUp=Math.Min(minUp,car.transform.up.y);w.WriteLine($"{Time.time-began},{p.x},{p.y},{p.z},{car.ForwardSpeed},{car.GroundedWheels},{car.transform.up.y},{pilot.LastObstacle}");if(Vector3.Distance(p,new Vector3(1080,152,145))<12){reached=true;break;}if(pilot.RecoveryCount>0||minUp<.5f)break;}}
            Check(reached&&minUp>.75f&&pilot.RecoveryCount==0,$"Observed climb dependency: ordinary motor-input segment, minUp={minUp:F3}, recoveries={pilot.RecoveryCount}");ThreeFeatureValidation.CaptureUi(dir+"/climb-end.png");
        }
    }
}
