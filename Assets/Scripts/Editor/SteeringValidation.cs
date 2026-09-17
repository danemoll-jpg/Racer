using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object=UnityEngine.Object;

namespace Racer.Editor
{
    public static class SteeringValidation
    {
        [MenuItem("Racer/Validate CR-010 (Play mode)")]
        public static void Run()
        {
            if(!Application.isPlaying)throw new InvalidOperationException("Enter Play mode first.");
            var car=Object.FindAnyObjectByType<ArcadeVehicle>();var body=car.Body;var reset=car.GetComponent<VehicleRespawn>();
            var race=Object.FindAnyObjectByType<RaceDirector>();var route=StreetLoopBuilder.Route();var report=new List<string>();int failures=0;
            var oldMode=Physics.simulationMode;var oldInterpolation=body.interpolation;float oldAngle=car.slowSteerAngle,oldResponse=car.steeringResponse;
            bool motor=car.enabled,respawn=reset.enabled;
            try
            {
                Physics.simulationMode=SimulationMode.Script;body.interpolation=RigidbodyInterpolation.None;car.enabled=false;reset.enabled=false;
                foreach(bool revised in new[]{false,true})
                {
                    car.slowSteerAngle=revised?33:32;car.steeringResponse=revised?8:7;
                    // Reference-map coordinates: ordinary bend, hairpin, entrance hills, fast connecting road, fast main road, reverse.
                    var cases=new[]{("ordinary bend",1015f,792f,10f), ("hairpin",1120f,1420f,5f), ("hills",940f,610f,10f),
                        ("fast connector",80f,1000f,25f), ("fast main",400f,468f,25f), ("reverse",80f,1000f,-8f)};
                    foreach(var test in cases)
                    {
                        var point=new Vector3((test.Item2-650)*1.1f,0,(950-test.Item3)*1.1f);int index=0;float closest=float.MaxValue;
                        for(int i=0;i<route.Count;i++){var d=route[i]-point;d.y=0;if(d.sqrMagnitude<closest){closest=d.sqrMagnitude;index=i;}}
                        int direction=test.Item4<0?-1:1;
                        body.position=route[index]+Vector3.up*.7f;body.rotation=Quaternion.LookRotation(route[(index+1)%route.Count]-route[index]);
                        car.transform.SetPositionAndRotation(body.position,body.rotation);body.linearVelocity=body.angularVelocity=Vector3.zero;car.ClearSteering();Physics.SyncTransforms();
                        for(int i=0;i<50;i++){car.Simulate(0,0,0,.02f);Physics.Simulate(.02f);}
                        body.linearVelocity=car.transform.forward*test.Item4;
                        float maxError=0,minUp=1,maxAngular=0;int airborne=0;
                        for(int step=0;step<400;step++)
                        {
                            float best=float.MaxValue;int advance=0;
                            for(int j=-4;j<=24;j++){int k=(index+direction*j+route.Count)%route.Count;var d=body.position-route[k];d.y=0;if(d.sqrMagnitude<best){best=d.sqrMagnitude;advance=j;}}
                            index=(index+direction*advance+route.Count)%route.Count;maxError=Mathf.Max(maxError,Mathf.Sqrt(best));minUp=Mathf.Min(minUp,Vector3.Dot(car.transform.up,Vector3.up));maxAngular=Mathf.Max(maxAngular,body.angularVelocity.magnitude);if(car.GroundedWheels<2)airborne++;
                            int look=Mathf.Max(3,Mathf.RoundToInt(Mathf.Abs(car.ForwardSpeed)*.35f));
                            var local=car.transform.InverseTransformPoint(route[(index+direction*look+route.Count)%route.Count]);
                            float curvature=2*local.x/Mathf.Max(1,local.x*local.x+local.z*local.z);
                            float angle=Mathf.Lerp(car.slowSteerAngle,car.fastSteerAngle,Mathf.Clamp01(Mathf.Abs(car.ForwardSpeed)/car.topSpeed));
                            float steering=Mathf.Clamp(Mathf.Atan(curvature*car.wheelbase)*Mathf.Rad2Deg/angle,-1,1);
                            float desired=Mathf.Abs(test.Item4),speed=car.ForwardSpeed*direction;
                            float pedal=Mathf.Clamp01((desired-speed)*.6f+(direction==1?.35f:.6f));float brake=speed>desired+1?Mathf.Clamp01((speed-desired)*.3f):0;
                            car.Simulate(direction==1?pedal:brake,direction==1?brake:pedal,steering,.02f);Physics.Simulate(.02f);
                            if(maxError>10||minUp<.5f)break;
                        }
                        bool pass=maxError<4&&minUp>.8f&&maxAngular<5;if(!pass)failures++;
                        report.Add($"{(pass?"PASS":"FAIL")}: {(revised?"revised 33/8":"original 32/7")} {test.Item1}, target {test.Item4}m/s, 8s; max error {maxError:F3}m, min upright {minUp:F3}, max angular {maxAngular:F3}, airborne steps {airborne}");
                    }
                    foreach(float speed in new[]{10f,25f,-8f})
                    {
                        int index=0;float best=float.MaxValue;var point=new Vector3(-627,0,-55);
                        for(int i=0;i<route.Count;i++){var delta=route[i]-point;delta.y=0;if(delta.sqrMagnitude<best){best=delta.sqrMagnitude;index=i;}}
                        body.position=route[index]+Vector3.up*.7f;body.rotation=Quaternion.LookRotation(route[index+1]-route[index]);
                        car.transform.SetPositionAndRotation(body.position,body.rotation);body.linearVelocity=body.angularVelocity=Vector3.zero;car.ClearSteering();Physics.SyncTransforms();
                        for(int i=0;i<50;i++){car.Simulate(0,0,0,.02f);Physics.Simulate(.02f);}
                        body.linearVelocity=car.transform.forward*speed;float minUp=1,yawAt100ms=0;
                        for(int i=0;i<30;i++){car.Simulate(0,0,1,.02f);Physics.Simulate(.02f);minUp=Mathf.Min(minUp,Vector3.Dot(car.transform.up,Vector3.up));if(i==4)yawAt100ms=Vector3.Dot(body.angularVelocity,car.transform.up);}
                        bool pass=minUp>.95f&&Mathf.Sign(yawAt100ms)==Mathf.Sign(speed);if(!pass)failures++;
                        report.Add($"{(pass?"PASS":"FAIL")}: {(revised?"revised":"original")} identical full-lock step at {speed}m/s, yaw at 100ms {yawAt100ms:F4}rad/s, minimum upright over 600ms {minUp:F4}. Reverse changes yaw sign.");
                    }
                }
            }
            finally{car.slowSteerAngle=oldAngle;car.steeringResponse=oldResponse;Physics.simulationMode=oldMode;body.interpolation=oldInterpolation;car.enabled=motor;reset.enabled=respawn;race.RestartRace();}
            report.Add("Failures: "+failures);report.Add("Automated steering commands through real PhysX. These are not physical-controller tests or subjective feel approval.");
            File.WriteAllLines("Docs/CR010_TEST_RESULTS.txt",report);Debug.Log(string.Join("\n",report));
        }
    }
}
