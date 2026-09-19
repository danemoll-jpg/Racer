using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
namespace Racer
{
    // Only used by the explicit standalone validation command line.
    public sealed class AmbientTrafficAudit:MonoBehaviour
    {
        StreamWriter log;
        float next;
        int captures;
        readonly Dictionary<AmbientVehicle,(int revision,Vector3 position)> seen=new();
        int visibleChanges,samples;float minGap=float.MaxValue;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot(){var a=Environment.GetCommandLineArgs();if(a.Contains("-trafficAudit")&&a.Contains("-racerTestSave"))new GameObject("Ambient traffic audit").AddComponent<AmbientTrafficAudit>();}
        void Start(){Directory.CreateDirectory("Docs/CR050-053/traffic");log=new StreamWriter("Docs/CR050-053/traffic/observations.csv");log.WriteLine("time,scope,body,paint,revision,x,z,recycles,recoveries");}
        void Update()
        {
            var race=FindAnyObjectByType<RaceDirector>();if(!race||race.Flow.State!=RaceFlow.Stage.Racing)return;
            var cars=FindObjectsByType<AmbientVehicle>();
            foreach(var car in cars)
            {
                var id=car;if(seen.TryGetValue(id,out var before)&&before.revision!=car.AppearanceRevision&&!AmbientVehicle.Offscreen(before.position)&&!AmbientVehicle.Offscreen(car.transform.position))visibleChanges++;
                seen[id]=(car.AppearanceRevision,car.transform.position);
            }
            if(Time.time<next)return;next=Time.time+.5f;samples++;
            foreach(var car in cars){var d=car.GetComponent<RoadDriver>();var p=car.transform.position;log.WriteLine($"{Time.time:F2},{(d?(d.HighwayTraffic?"highway":"local"):"extension")},{AmbientVehicle.BodyNames[car.BodyType]},{car.PaintIndex},{car.AppearanceRevision},{p.x:F2},{p.z:F2},{(d?d.HighwayRecycles:0)},{(d?d.RecoveryCount:0)}");}
            for(int i=0;i<race.Drivers.Count;i++)for(int j=i+1;j<race.Drivers.Count;j++)
            {var a=race.Drivers[i];var b=race.Drivers[j];if(!a.GetComponent<AmbientVehicle>()||!b.GetComponent<AmbientVehicle>()||a.Direction!=b.Direction)continue;float side=Mathf.Abs(Vector3.Dot(a.transform.right,b.transform.position-a.transform.position));if(side<2.1f)minGap=Mathf.Min(minGap,Vector3.Distance(a.transform.position,b.transform.position));}
            log.Flush();File.WriteAllText("Docs/CR050-053/traffic/summary.txt",$"samples={samples}; visible appearance changes={visibleChanges}; minimum same-direction near-lane centre distance={minGap:F2}m; bodies={cars.Select(c=>c.BodyType).Distinct().Count()}; paints={cars.Select(c=>c.PaintIndex).Distinct().Count()}");
            float s=race.road.Project(race.vehicle.transform.position,out _);
            if((captures==0&&s>300&&s<700)||(captures==1&&s>3850&&s<4300))
            {CombinedReviewValidation.Capture("Docs/CR050-053/traffic/"+(captures==0?"local":"highway")+".png");captures++;}
        }
        void OnDestroy(){log?.Dispose();}
    }
}
