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
        IEnumerator ReverseGuidance()
        {
            race.opponents=race.traffic=false;car.GetComponent<VehicleConfiguration>().Apply("moto");flow.StartFreeRoam();car.enabled=false;car.GetComponent<VehicleInput>().enabled=false;
            var summit=race.Branches.Single(b=>b.title=="Summit Traverse");var ridge=race.Branches.Single(b=>b.title=="Downhill Ridge Cut");
            var legalA=ridge.At(11,out var legalF);var legalB=ridge.At(14,out _);
            Check(ridge.Enter(legalA,legalB,legalF),"Relocated Downhill Ridge Cut entrance grants entitlement");
            Check(!ridge.Enter(new(807,98,-145),new(803,98,-147),Vector3.left),"Old late cut does not grant shortcut entitlement");
            var exit=ridge.At(ridge.Length-2,out var exitF);Check(!ridge.Enter(exit-exitF*2,exit,exitF),"Exit-only entry does not grant shortcut entitlement");
            Check(ridge.bypassedGates.SequenceEqual(new[]{4,5})&&summit.bypassedGates.SequenceEqual(new[]{2,3}),"Only intended gate bypasses retained; entry gate moved before branch");
            var tail=new List<Vector3>();for(float s=215;s<summit.Length;s+=7)tail.Add(summit.At(s,out _));tail.Add(summit.points[^1]);tail.Add(new(817,98,-130));tail.Add(new(822,98,-134));tail.Add(new(829,100,-127));tail.Add(race.road.At(1600,out _));tail.Add(race.road.At(1620,out _));
            yield return CR121Drive("summit-tail-rejoin",tail.ToArray(),75,5);
            var entrance=new List<Vector3>();for(float s=1485;s<1510;s+=5)entrance.Add(race.road.At(s,out _));for(float s=0;s<ridge.Length;s+=5)entrance.Add(ridge.At(s,out _));entrance.Add(ridge.points[^1]);entrance.Add(race.road.At(ridge.exitRoad+12,out _));
            yield return CR121Drive("shortcut-2-entrance-and-rejoin",entrance.ToArray(),50,5);
            var start=race.road.At(1560,out _);var target=new Vector3(789,98,-152);var direction=(target-start).normalized;Place(start,direction);var contact=car.gameObject.AddComponent<CR121BarrierContact>();float began=Time.time,minX=start.x;
            ThreeFeatureValidation.CaptureUi(dir+"/illegal-cut-approach.png");
            while(Time.time-began<8&&!contact.Hit){car.Simulate(Mathf.Clamp01((5-car.ForwardSpeed)*.6f),car.ForwardSpeed>6?.4f:0,0,Time.fixedDeltaTime);yield return new WaitForFixedUpdate();minX=Math.Min(minX,car.Body.position.x);}
            car.Simulate(0,1,0,Time.fixedDeltaTime);Check(contact.Hit&&minX>800,$"Old left cut physically blocked: barrierContact={contact.Hit}, minimumX={minX:F2}");ThreeFeatureValidation.CaptureUi(dir+"/illegal-cut-blocked.png");
        }
        IEnumerator CR121Drive(string name,Vector3[] points,float limit,float speed)
        {
            Place(points[0],points[1]-points[0]);int next=1;float began=Time.time,minUp=1;ThreeFeatureValidation.CaptureUi(dir+"/"+name+"-approach.png");
            using(var w=new StreamWriter(dir+"/"+name+".csv")){
                w.WriteLine("seconds,x,y,z,speed,next,up,wheels");
                while(Time.time-began<limit&&next<points.Length){
                    var delta=Vector3.ProjectOnPlane(points[next]-car.Body.position,Vector3.up);if(delta.magnitude<3){next++;continue;}
                    var local=car.transform.InverseTransformPoint(points[next]);float steering=Mathf.Clamp(Mathf.Atan2(local.x,local.z)*2,-1,1);float target=Math.Abs(steering)>.8f?3:speed;
                    car.Simulate(Mathf.Clamp01((target-car.ForwardSpeed)*.6f),car.ForwardSpeed>target+1?.5f:0,steering,Time.fixedDeltaTime);yield return new WaitForFixedUpdate();minUp=Math.Min(minUp,car.transform.up.y);var p=car.Body.position;w.WriteLine($"{Time.time-began},{p.x},{p.y},{p.z},{car.ForwardSpeed},{next},{car.transform.up.y},{car.GroundedWheels}");if(car.transform.up.y<.4f)break;
                }
            }
            car.Simulate(0,1,0,Time.fixedDeltaTime);Check(next==points.Length&&minUp>.65f,$"{name}: one normal-input motorcycle traversal, waypoint {next}/{points.Length}, minUp={minUp:F2}");ThreeFeatureValidation.CaptureUi(dir+"/"+name+"-end.png");
        }
    }
    public sealed class CR121BarrierContact:MonoBehaviour
    {
        public bool Hit;void OnCollisionEnter(Collision c){if(c.collider.name.StartsWith("CR121 closed late cut barrier"))Hit=true;}
    }
}
