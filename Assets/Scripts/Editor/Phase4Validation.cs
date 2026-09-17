using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object=UnityEngine.Object;

namespace Racer.Editor
{
    public static class Phase4Validation
    {
        const float Dt=.02f;
        static List<string> lines;
        static ArcadeVehicle car;
        static Rigidbody body;
        static List<Vector3> route;
        static int Nearest(Vector3 p)
        {int index=0;float best=float.MaxValue;for(int i=0;i<route.Count;i++){var d=p-route[i];d.y=0;if(d.sqrMagnitude<best){best=d.sqrMagnitude;index=i;}}return index;}
        static void Place(int index,float speed=0,float lateral=0,float yaw=0)
        {
            var f=Vector3.ProjectOnPlane(route[(index+1)%route.Count]-route[index],Vector3.up).normalized;
            var p=route[index]+Vector3.Cross(Vector3.up,f)*lateral;
            if(!Physics.Raycast(p+Vector3.up*25,Vector3.down,out var hit,60,1))throw new Exception("Missing surface");
            body.position=hit.point+Vector3.up*.65f;body.rotation=Quaternion.LookRotation(f)*Quaternion.Euler(0,yaw,0);
            car.transform.SetPositionAndRotation(body.position,body.rotation);body.linearVelocity=body.angularVelocity=Vector3.zero;car.ClearSteering();Physics.SyncTransforms();
            for(int i=0;i<75;i++)Step(0,0,0);
            body.linearVelocity=car.transform.forward*speed;
        }
        static void Step(float throttle,float brake,float steer){car.Simulate(throttle,brake,steer,Dt);Physics.Simulate(Dt);}
        static int Advance(int index,int direction=1)
        {float best=float.MaxValue;int next=index;for(int j=-5;j<=30;j++){int k=(index+direction*j+route.Count)%route.Count;var d=body.position-route[k];d.y=0;if(d.sqrMagnitude<best){best=d.sqrMagnitude;next=k;}}return next;}
        static float Steering(int index,int direction=1,float offset=0)
        {
            int ahead=Mathf.Max(3,Mathf.RoundToInt(Mathf.Abs(car.ForwardSpeed)*.28f));
            int k=(index+direction*ahead+route.Count)%route.Count;
            var f=(route[(k+1)%route.Count]-route[k]).normalized;
            var target=route[k]+Vector3.Cross(Vector3.up,f).normalized*offset;
            var local=car.transform.InverseTransformPoint(target);
            float curvature=2*local.x/Mathf.Max(1,local.x*local.x+local.z*local.z);
            float angle=Mathf.Lerp(car.slowSteerAngle,car.fastSteerAngle,Mathf.Clamp01(Mathf.Abs(car.ForwardSpeed)/car.topSpeed));
            return Mathf.Clamp(Mathf.Atan(curvature*car.wheelbase)*Mathf.Rad2Deg/angle,-1,1);
        }
        static void Drive(float desired,float steer)
        {
            int direction=desired<0?-1:1;float speed=car.ForwardSpeed*direction;
            float pedal=Mathf.Clamp01((Mathf.Abs(desired)-speed)*.7f+.35f);
            float brake=speed>Mathf.Abs(desired)+.3f?Mathf.Clamp01((speed-Mathf.Abs(desired))*.5f):0;
            Step(direction>0?pedal:brake,direction>0?brake:pedal,steer);
        }
        [MenuItem("Racer/Validate Phase 4 Roads (Play mode)")]
        public static void Roads()=>Run("roads");
        [MenuItem("Racer/Validate Phase 4 Jump (Play mode)")]
        public static void Jump()=>Run("jump");
        [MenuItem("Racer/Validate Phase 4 Racing Laps (Play mode)")]
        public static void Laps()=>Run("laps");
        public static void Run(string mode)
        {
            if(!Application.isPlaying)throw new Exception("Enter Play mode first");
            car=Object.FindAnyObjectByType<ArcadeVehicle>();body=car.Body;route=StreetLoopBuilder.Route();lines=new List<string>();
            var reset=car.GetComponent<VehicleRespawn>();var race=Object.FindAnyObjectByType<RaceDirector>();
            string original=JsonUtility.ToJson(car);var oldMode=Physics.simulationMode;var oldInterp=body.interpolation;bool enabled=car.enabled,resetEnabled=reset.enabled;
            try
            {
                Physics.simulationMode=SimulationMode.Script;body.interpolation=RigidbodyInterpolation.None;car.enabled=false;reset.enabled=false;
                if(mode=="roads")RoadTests();else if(mode=="jump")JumpTests();else if(mode=="limits")LimitTests();else LapTest(race);
            }
            finally{JsonUtility.FromJsonOverwrite(original,car);Physics.simulationMode=oldMode;body.interpolation=oldInterp;car.enabled=enabled;reset.enabled=resetEnabled;race.RestartRace();}
            lines.Add("Real PhysX 0.02s steps with virtual motor commands; injected initial velocities where stated. No physical controller or subjective approval.");
            File.WriteAllLines("Docs/PHASE4_"+mode.ToUpperInvariant()+".txt",lines);Debug.Log(string.Join("\n",lines));
        }
        static void RoadTests()
        {
            foreach(bool revised in new[]{false,true})
            {
                Phase4Setup.Tune(car,revised);string label=revised?"REVISED":"BASELINE";
                foreach(var test in new[]{("connector sustained",new Vector3(-627,0,-360),35f,6f),("main straight",new Vector3(-460,0,534),38f,8f),("ordinary bends",new Vector3(400,0,175),20f,10f),("hairpin",new Vector3(546,0,-470),9f,15f),("hills",new Vector3(319,0,400),20f,12f),("reverse",new Vector3(-629,0,100),-8f,8f)})
                {
                    int index=Nearest(test.Item2);Place(index,test.Item3);float maxError=0,minUp=1,maxSlip=0,peak=0,sum=0;int air=0,steps=0;
                    for(;steps<test.Item4/Dt;steps++)
                    {
                        index=Advance(index,test.Item3<0?-1:1);Drive(test.Item3,Steering(index,test.Item3<0?-1:1));
                        var d=body.position-route[index];d.y=0;maxError=Mathf.Max(maxError,d.magnitude);minUp=Mathf.Min(minUp,car.transform.up.y);maxSlip=Mathf.Max(maxSlip,Mathf.Abs(Vector3.Dot(body.linearVelocity,car.transform.right)));peak=Mathf.Max(peak,Mathf.Abs(car.ForwardSpeed));sum+=Mathf.Abs(car.ForwardSpeed);if(car.GroundedWheels<2)air++;
                        if(maxError>10||minUp<.5f)break;
                    }
                    lines.Add($"{(maxError<4.5f&&minUp>.8f?"PASS":"FAIL")} {label} {test.Item1}: injected {test.Item3}m/s; duration {steps*Dt:F2}s; mean {sum/Mathf.Max(1,steps):F2}, peak {peak:F2}m/s; error {maxError:F2}m; min up {minUp:F3}; lateral peak {maxSlip:F2}m/s; air steps {air}");
                }
                int straight=Nearest(new Vector3(-623,0,-370));Place(straight);
                float maxSpeed=0;int idx=straight;
                // Starts from rest; no injected velocity. Stop before the ramp.
                for(int i=0;i<1500&&body.position.z<-150;i++){idx=Advance(idx);Step(1,0,Steering(idx));maxSpeed=Mathf.Max(maxSpeed,car.ForwardSpeed);}
                lines.Add($"{label} acceleration from rest on connector: peak {maxSpeed:F2}m/s ({maxSpeed*3.6f:F1}km/h), end {body.position}");
                Place(Nearest(new Vector3(-628,0,100)),35);float distance=0;var start=body.position;int brakingSteps=0;
                while(car.ForwardSpeed>1&&brakingSteps++<200)Step(0,1,0);
                distance=Vector3.Distance(start,body.position);lines.Add($"{label} full braking 35 to {car.ForwardSpeed:F2}m/s: {brakingSteps*Dt:F2}s / {distance:F2}m, upright {car.transform.up.y:F3}");
                foreach(string diagnostic in new[]{"yaw","slip","body"})
                {
                    Place(Nearest(new Vector3(-628,0,100)),25);
                    if(diagnostic=="slip")body.linearVelocity+=car.transform.right*4;
                    if(diagnostic=="body")body.rotation*=Quaternion.Euler(0,0,8);
                    for(int i=0;i<50;i++)
                    {Step(.5f,0,diagnostic=="yaw"?.3f:0);if(i==4||i==14||i==49)lines.Add($"DIAG {label} {diagnostic} t={(i+1)*Dt:F2}: yaw {body.angularVelocity.y:F3}rad/s, slip {Vector3.Dot(body.linearVelocity,car.transform.right):F3}m/s, up {car.transform.up.y:F4}, vertical {body.linearVelocity.y:F3}m/s");}
                }
            }
        }
        static void JumpTests()
        {
            Phase4Setup.Tune(car,true);
            foreach(float speed in new[]{12f,32f,42f,46f})foreach(float yaw in new[]{0f,3f,-3f})
            {
                int index=Nearest(new Vector3(-627,0,-138));Place(index,speed,-1.5f,yaw);
                float takeoff=0,landing=0,apex=0,minUp=1,air=0,maxOffset=0;bool flew=false,landed=false;int airSteps=0;
                for(int i=0;i<650;i++)
                {
                    index=Advance(index);
                    // Fixed initial heading during takeoff/flight; correction only after landing.
                    Drive(speed,landed?Steering(index):0);
                    float z=body.position.z;minUp=Mathf.Min(minUp,car.transform.up.y);apex=Mathf.Max(apex,body.position.y-route[index].y);
                    maxOffset=Mathf.Max(maxOffset,Mathf.Abs(body.position.x-route[index].x));
                    if(car.GroundedWheels<2&&z>-112){airSteps++;if(airSteps==4){flew=true;takeoff=car.ForwardSpeed;}if(flew)air+=Dt;}
                    else if(flew&&!landed){landed=true;landing=z;}
                    if(landed&&z>landing+35||minUp<.4f||z>65)break;
                }
                lines.Add($"{(flew&&landed&&minUp>.8f&&maxOffset<8?"PASS":"LIMIT")} approach target/injected {speed}m/s yaw {yaw}deg: takeoff {takeoff:F2}m/s; landed={landed}, landing Z={landing:F2}; air {air:F2}s; apex above road {apex:F2}m; min upright {minUp:F3}; max lateral offset {maxOffset:F2}m");
            }
            // Continuous ordinary-road lane to the right of the ramp, at low and racing speeds.
            foreach(float speed in new[]{6f,30f})
            {int index=Nearest(new Vector3(-627,0,-140));Place(index,speed,3);float minUp=1;int air=0;for(int i=0;i<1000&&body.position.z<-60;i++){index=Advance(index);Drive(speed,Steering(index,1,3));minUp=Mathf.Min(minUp,car.transform.up.y);if(car.GroundedWheels<2)air++;}lines.Add($"BYPASS {speed}m/s end Z {body.position.z:F2}; min upright {minUp:F3}; air steps {air}");}
            foreach(float startZ in new[]{-145f,-35f})foreach(int side in new[]{-1,1})
            {int index=Nearest(new Vector3(-628,0,startZ));Place(index,12,side*10);float minUp=1,minClear=10;for(int i=0;i<350;i++){index=Advance(index);Drive(12,Steering(index));minUp=Mathf.Min(minUp,car.transform.up.y);if(Physics.Raycast(body.position+Vector3.up*10,Vector3.down,out var hit,50,1))minClear=Mathf.Min(minClear,body.position.y-hit.point.y);}lines.Add($"SHOULDER side {side} start Z={startZ}, 12m/s: upright {minUp:F3}; clearance {minClear:F3}m; end {body.position}");}
            var race=Object.FindAnyObjectByType<RaceDirector>();race.RestartRace();race.Progress.Cross(0,true,0);race.Progress.Cross(1,true,1);
            body.position=new Vector3(-650,4,-50);body.rotation=Quaternion.Euler(0,0,120);body.linearVelocity=Vector3.one*20;
            car.GetComponent<VehicleRespawn>().ResetVehicle();
            lines.Add($"RECOVERY failed landing reset: lapActive={race.Progress.LapActive}; completed={race.Progress.CompletedLaps}; speed={body.linearVelocity.magnitude}; upright={car.transform.up.y}; nextGate={race.Progress.NextGate}");
        }
        static void LapTest(RaceDirector race)
        {
            Phase4Setup.Tune(car,true);var limits=new float[route.Count];
            for(int i=0;i<route.Count;i++)
            {
                var a=route[(i+route.Count-4)%route.Count];var b=route[i];var c=route[(i+4)%route.Count];
                float curvature=Vector3.Angle(Vector3.ProjectOnPlane(b-a,Vector3.up),Vector3.ProjectOnPlane(c-b,Vector3.up))*Mathf.Deg2Rad/Mathf.Max(1,Vector3.Distance(a,c)*.5f);
                limits[i]=Mathf.Clamp(Mathf.Sqrt(7/Mathf.Max(.001f,curvature)),7,40);
                if(b.y>20)limits[i]=Mathf.Min(limits[i],22);
                if(b.x<-610&&b.z>-145&&b.z<55)limits[i]=32;
            }
            for(int pass=0;pass<3;pass++)for(int i=route.Count-1;i>=0;i--){int n=(i+1)%route.Count;limits[i]=Mathf.Min(limits[i],Mathf.Sqrt(limits[n]*limits[n]+14*Vector3.Distance(route[i],route[n])));}
            race.RestartRace();int index=10;Place(index);race.ResetSampling(body.position,0);
            float maxError=0,minUp=1,peak=0,sum=0;int steps=0,racing=0,air=0;
            for(;steps<85000&&!race.Progress.Finished;steps++)
            {
                index=Advance(index);Drive(limits[index],Steering(index));int before=race.Progress.CompletedLaps;race.Sample(body.position,car.transform.forward,(steps+1)*Dt);
                if(race.Progress.CompletedLaps>before)lines.Add($"LAP {race.Progress.CompletedLaps}: {race.Progress.LastLap:F3}s");
                var d=body.position-route[index];d.y=0;maxError=Mathf.Max(maxError,d.magnitude);minUp=Mathf.Min(minUp,car.transform.up.y);peak=Mathf.Max(peak,car.ForwardSpeed);sum+=car.ForwardSpeed;if(car.ForwardSpeed>=25)racing++;if(car.GroundedWheels<2)air++;
                if(steps%5000==0)lines.Add($"PROGRESS t={steps*Dt:F1}s index={index} next={race.Progress.NextGate} laps={race.Progress.CompletedLaps} valid={race.Progress.LapValid} speed={car.ForwardSpeed:F2}");
                if(maxError>9||minUp<.5f){lines.Add($"STOP index={index}, position={body.position}");break;}
            }
            lines.Add($"{(race.Progress.Finished?"PASS":"FAIL")} actual three-lap traversal: laps={race.Progress.CompletedLaps}; next={race.Progress.NextGate}; valid={race.Progress.LapValid}; time={steps*Dt:F2}s; peak={peak:F2}m/s; mean={sum/Mathf.Max(1,steps):F2}; seconds>=25m/s={racing*Dt:F2}; max error={maxError:F2}m; min upright={minUp:F3}; airborne seconds={air*Dt:F2}; last lap={race.Progress.LastLap:F3}");
        }
        static void LimitTests()
        {
            Phase4Setup.Tune(car,true);
            int index=Nearest(new Vector3(-550,0,534));Place(index);float peak=0,minUp=1;int steps=0,near=0;
            for(;steps<2000&&body.position.x<50;steps++){index=Advance(index);Step(1,0,Steering(index));peak=Mathf.Max(peak,car.ForwardSpeed);minUp=Mathf.Min(minUp,car.transform.up.y);if(car.ForwardSpeed>=40)near++;}
            lines.Add($"TOP SPEED from rest, main road ~600m: {steps*Dt:F2}s; peak {peak:F3}m/s ({peak*3.6f:F1}km/h); >=40m/s for {near*Dt:F2}s; upright {minUp:F3}; end {body.position}. No velocity injection.");
            foreach(var test in new[]{(60f,0f),(42f,-8f),(6f,0f)})
            {
                index=Nearest(new Vector3(-627,0,-138));Place(index,test.Item1,-1.5f,test.Item2);
                bool flew=false,landed=false;float takeoff=0,landing=0,min=1,offset=0;int airSteps=0;
                for(int i=0;i<1600;i++)
                {
                    index=Advance(index);Drive(test.Item1,0);min=Mathf.Min(min,car.transform.up.y);offset=Mathf.Max(offset,Mathf.Abs(body.position.x-route[index].x));
                    if(car.GroundedWheels<2&&body.position.z>-112){airSteps++;if(airSteps==4){flew=true;takeoff=car.ForwardSpeed;}}
                    else if(flew&&!landed){landed=true;landing=body.position.z;}
                    if(landed||min<.4f||body.position.z>200)break;
                }
                var failedPosition=body.position;var race=Object.FindAnyObjectByType<RaceDirector>();race.RestartRace();race.Progress.Cross(0,true,0);
                for(int g=1;g<=13;g++)race.Progress.Cross(g,true,g);
                body.position=failedPosition;car.GetComponent<VehicleRespawn>().ResetVehicle();
                // Crossing only the remaining gates after recovery must never credit a lap.
                for(int g=14;g<=19;g++)race.Progress.Cross(g,true,g+1);race.Progress.Cross(0,true,30);
                lines.Add($"LIMIT injected {test.Item1}m/s yaw {test.Item2}: takeoff {takeoff:F2}; flew={flew}; landed={landed} Z={landing:F2}; min up {min:F3}; offset {offset:F2}; failed/end position {failedPosition}; reset then remaining-gates/finish completed laps={race.Progress.CompletedLaps} (must be 0)");
            }
            int missing=0;float minClear=100;
            for(float z=-76;z<=40;z+=4)for(float x=-8;x<=8;x+=1)
            {index=Nearest(new Vector3(-628,0,z));var p=route[index]+Vector3.right*x;if(!Physics.Raycast(p+Vector3.up*15,Vector3.down,out var hit,35,1))missing++;else minClear=Mathf.Min(minClear,hit.point.y);}
            lines.Add($"LANDING SUPPORT ray samples: missing={missing}, lowest surface Y={minClear:F2}; existing road/shoulders retained.");
        }
        public static void RaceRegression()
        {
            var settings=UnityEngine.InputSystem.InputSystem.settings;
            var behavior=settings.editorInputBehaviorInPlayMode;var background=settings.backgroundBehavior;
            bool run=Application.runInBackground;var previous=File.ReadAllBytes("Docs/PHASE3_TEST_RESULTS.txt");
            try
            {
                Application.runInBackground=true;
                settings.editorInputBehaviorInPlayMode=UnityEngine.InputSystem.InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
                settings.backgroundBehavior=UnityEngine.InputSystem.InputSettings.BackgroundBehavior.IgnoreFocus;
                RaceValidation.Run();File.Copy("Docs/PHASE3_TEST_RESULTS.txt","Docs/PHASE4_RACE_REGRESSION.txt",true);
            }
            finally{File.WriteAllBytes("Docs/PHASE3_TEST_RESULTS.txt",previous);settings.editorInputBehaviorInPlayMode=behavior;settings.backgroundBehavior=background;Application.runInBackground=run;}
        }
    }
}
