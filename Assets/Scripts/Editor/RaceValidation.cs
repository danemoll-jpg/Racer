using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using Object = UnityEngine.Object;

namespace Racer.Editor
{
    public static class RaceValidation
    {
        [MenuItem("Racer/Validate Phase 3 (Play mode)")]
        public static void Run()
        {
            if (!Application.isPlaying) throw new InvalidOperationException("Play StreetLoopGreybox first.");
            var race=Object.FindAnyObjectByType<RaceDirector>(); if(!race)throw new InvalidOperationException("Missing race.");
            var report=new List<string>();int failures=0;
            void Check(bool ok,string message){report.Add((ok?"PASS: ":"FAIL: ")+message);if(!ok)failures++;}
            var p=new RaceProgress(3,3);
            p.Cross(1,true,1);Check(!p.Started,"Checkpoints cannot start race");
            p.Cross(0,false,2);Check(!p.Started,"Wrong-way start cannot start race");
            p.Cross(0,true,10);p.Cross(2,true,11);p.Cross(3,true,12);p.Cross(0,true,20);
            Check(p.CompletedLaps==0,"Skipped CP1 cannot award a lap");
            p.Cross(1,true,21);p.Cross(2,true,22);p.Cross(3,false,23);p.Cross(3,true,24);p.Cross(0,true,30);
            Check(p.CompletedLaps==0,"Wrong-way gate invalidates lap even after recrossing forward");
            p.Cross(1,true,31);p.Cross(1,true,32);p.Cross(2,true,33);p.Cross(3,true,34);p.Cross(0,true,40);
            Check(p.CompletedLaps==0,"Repeated checkpoint invalidates current lap");
            for(int i=0;i<12;i++)p.Cross(0,true,41+i);
            Check(p.CompletedLaps==0,"Repeated finish crossings cannot award laps");
            p.Restart();p.Cross(0,true,100);
            for(int lap=0;lap<3;lap++){for(int g=1;g<=3;g++)p.Cross(g,true,100+lap*60+g*10);p.Cross(0,true,160+lap*60);}
            Check(p.Finished && p.CompletedLaps==3 && p.LastLap==60 && p.BestLap==60 && p.RaceTime(1000)==180,"Three ordered laps: 60s splits, 180s frozen total");
            p.Cross(0,true,1100);Check(p.CompletedLaps==3,"Finished race ignores further crossings");
            p.Restart();Check(!p.Started&&p.CompletedLaps==0&&p.NextGate==0&&p.LastLap==0&&p.BestLap==0&&p.RaceTime(2000)==0,"Restart clears all progress and timing");
            p.Cross(0,true,1);p.Cross(1,true,2);p.ResetToGrid();p.Cross(2,true,3);p.Cross(3,true,4);p.Cross(0,true,5);
            Check(p.CompletedLaps==0&&p.NextGate==1,"Vehicle reset cannot preserve checkpoint credit");
            var gate=race.gates[0];var tr=gate.transform;
            Vector3 At(float x,float y,float z)=>tr.TransformPoint(new Vector3(x,y,z));
            Check(gate.TryCross(At(0,0,-2),At(0,0,2),out bool f,out float fraction)&&f&&Mathf.Abs(fraction-.5f)<.0001f,"Swept forward crossing and sub-step timing");
            Check(gate.TryCross(At(0,0,2),At(0,0,-2),out f,out _)&&!f,"Swept reverse crossing");
            Check(!gate.TryCross(At(8,0,-2),At(8,0,2),out _,out _)&&!gate.TryCross(At(0,5,-2),At(0,5,2),out _,out _),"Outside-width and above-gate bypasses rejected");
            Check(!gate.TryCross(At(0,0,0),At(0,0,1),out _,out _),"Standing on plane does not repeat crossing");

            var car=race.vehicle;var body=car.Body;var reset=car.GetComponent<VehicleRespawn>();var input=car.GetComponent<VehicleInput>();
            var camera=Object.FindAnyObjectByType<ChaseCamera>();var hud=Object.FindAnyObjectByType<RaceHud>();
            var mode=Physics.simulationMode;var interpolation=body.interpolation;
            bool carEnabled=car.enabled,resetEnabled=reset.enabled,raceEnabled=race.enabled;
            Gamepad pad=null;Keyboard keyboard=null;
            try
            {
                Physics.simulationMode=SimulationMode.Script;body.interpolation=RigidbodyInterpolation.None;car.enabled=false;reset.enabled=false;
                // Leave director enabled so its reset event and restart action stay subscribed.
                race.RestartRace();race.ResetSampling(At(0,0,-2),0);race.Sample(At(0,0,2),tr.forward,1);
                Check(race.Progress.Started,"Director starts through physical gate geometry");
                race.Sample(At(0,0,-2),-tr.forward,2);Check(!race.Progress.LapValid,"Director rejects wrong-way finish");
                race.RestartRace();race.ResetSampling(At(0,0,-2),0);race.Sample(At(0,0,2),-tr.forward,1);
                Check(!race.Progress.Started,"Reversing rear-first through start does not count");
                race.RestartRace();race.ResetSampling(At(0,0,-2),0);race.Sample(At(0,0,20),tr.forward,1);
                Check(!race.Progress.Started,"Teleport over gate cannot start race");

                pad=InputSystem.AddDevice<Gamepad>();keyboard=InputSystem.AddDevice<Keyboard>();
                void Pad(GamepadState state){InputSystem.QueueStateEvent(pad,state);InputSystem.Update();input.SendMessage("Update");}
                void Keys(KeyboardState state){InputSystem.QueueStateEvent(keyboard,state);InputSystem.Update();input.SendMessage("Update");}
                Pad(new GamepadState{rightTrigger=.65f,leftStick=new Vector2(.35f,0)});float small=input.Steering;
                Check(input.Throttle>.6f&&small>0&&small<.6f,"Virtual gamepad progressive trigger and partial steering");
                Pad(new GamepadState{leftStick=new Vector2(.7f,0)});Check(input.Steering>small&&input.Steering<1,"Virtual gamepad progressive larger steering");
                Pad(new GamepadState{leftTrigger=1});Check(input.BrakeReverse>.99f,"Virtual LT brake/reverse");
                race.Progress.Cross(0,true,0);race.Progress.Cross(1,true,1);
                Pad(new GamepadState().WithButton(GamepadButton.Start));race.SendMessage("Update");
                Check(!race.Progress.Started&&race.Progress.CompletedLaps==0&&race.Progress.LastLap==0,"Virtual Start restarts race");
                Pad(new GamepadState());Keys(new KeyboardState(Key.W,Key.D));Check(input.Throttle>.99f&&input.Steering>.99f,"Virtual keyboard W/D");
                Keys(new KeyboardState(Key.S,Key.A));Check(input.BrakeReverse>.99f&&input.Steering<-.99f,"Virtual keyboard S/A");
                Keys(new KeyboardState(Key.UpArrow,Key.LeftArrow));Check(input.Throttle>.99f&&input.Steering<-.99f,"Virtual keyboard arrows");
                race.Progress.Cross(0,true,0);Keys(new KeyboardState(Key.Enter));race.SendMessage("Update");
                Check(!race.Progress.Started&&race.Progress.RaceTime(20)==0,"Virtual Enter clears race and timer");
                Keys(new KeyboardState());race.Progress.Cross(0,true,0);race.Progress.Cross(1,true,1);
                body.position+=Vector3.right*8;body.linearVelocity=Vector3.one*5;
                Pad(new GamepadState().WithButton(GamepadButton.Y));reset.SendMessage("FixedUpdate");
                Check(Vector3.Distance(body.position,reset.spawnPoint.position)<.01f&&body.linearVelocity.sqrMagnitude<.001f&&!race.Progress.LapActive,"Virtual Y retains fixed upright reset and abandons lap");
                Check(Vector3.Distance(camera.transform.position,car.transform.position+Quaternion.Euler(0,car.transform.eulerAngles.y,0)*camera.offset)<.05f,"Chase camera snaps on reset");
                Pad(new GamepadState());body.position+=Vector3.right*5;Keys(new KeyboardState(Key.R));reset.SendMessage("FixedUpdate");
                Check(Vector3.Distance(body.position,reset.spawnPoint.position)<.01f,"Virtual R reset");Keys(new KeyboardState());
                body.position=new Vector3(0,-20,0);car.transform.position=body.position;Physics.SyncTransforms();reset.SendMessage("FixedUpdate");Check(Vector3.Distance(body.position,reset.spawnPoint.position)<.01f,"Automatic fall reset preserved");
                race.RestartRace();hud.SendMessage("LateUpdate");Check(hud.display.text.Contains("LAP 1 / 3")&&hud.display.text.Contains("00:00.000")&&hud.display.text.Contains("START"),"HUD ready state");
                race.Progress.Cross(0,true,race.Clock);race.Progress.Cross(1,true,race.Clock+1);hud.SendMessage("LateUpdate");
                Check(hud.display.text.Contains("Next: CP 02"),"HUD next checkpoint updates");
                race.Progress.Invalidate("Wrong way");hud.SendMessage("LateUpdate");Check(hud.display.text.Contains("Wrong way"),"HUD invalid-lap warning");
                for(int lap=0;lap<3;lap++){race.Progress.Cross(0,true,lap*100);for(int g=1;g<race.gates.Length;g++)race.Progress.Cross(g,true,lap*100+g);race.Progress.Cross(0,true,(lap+1)*100);}
                hud.SendMessage("LateUpdate");Check(hud.display.text.Contains("Race complete")&&hud.display.text.Contains("Completed: 3"),"HUD completed race");

                race.RestartRace();
                var route=StreetLoopBuilder.Route();int index=10,steps=0;float maxError=0,minUp=1;int airborne=0;
                body.position=route[index]+Vector3.up*.7f;body.rotation=Quaternion.LookRotation(route[index+1]-route[index]);
                car.transform.SetPositionAndRotation(body.position,body.rotation);body.linearVelocity=body.angularVelocity=Vector3.zero;car.ClearSteering();Physics.SyncTransforms();
                race.ResetSampling(body.position,0);
                for(;steps<100000&&!race.Progress.Finished;steps++)
                {
                    float best=float.MaxValue;int advance=0;
                    for(int j=-4;j<=18;j++){int k=(index+j+route.Count)%route.Count;var diff=body.position-route[k];diff.y=0;if(diff.sqrMagnitude<best){best=diff.sqrMagnitude;advance=j;}}
                    index=(index+advance+route.Count)%route.Count;maxError=Mathf.Max(maxError,Mathf.Sqrt(best));minUp=Mathf.Min(minUp,Vector3.Dot(car.transform.up,Vector3.up));if(car.GroundedWheels<2)airborne++;
                    var local=car.transform.InverseTransformPoint(route[(index+3)%route.Count]);float curvature=2*local.x/Mathf.Max(1,local.x*local.x+local.z*local.z);
                    float desired=Mathf.Abs(curvature)>.025f?4.5f:8;float speed=car.ForwardSpeed;
                    float angle=Mathf.Lerp(car.slowSteerAngle,car.fastSteerAngle,Mathf.Clamp01(Mathf.Abs(speed)/car.topSpeed));
                    float steer=Mathf.Clamp(Mathf.Atan(curvature*car.wheelbase)*Mathf.Rad2Deg/angle,-1,1);
                    car.Simulate(Mathf.Clamp01((desired-speed)*.6f+.35f),speed>desired+1?Mathf.Clamp01((speed-desired)*.3f):0,steer,.02f);Physics.Simulate(.02f);
                    int before=race.Progress.CompletedLaps;race.Sample(body.position,car.transform.forward,(steps+1)*.02);
                    if(race.Progress.CompletedLaps>before)report.Add($"LAP {race.Progress.CompletedLaps}: {race.Progress.LastLap:F3}s, total {race.Progress.RaceTime(race.Clock):F3}s");
                    if(maxError>8||minUp<.5f)break;
                }
                Check(race.Progress.Finished&&race.Progress.CompletedLaps==3&&maxError<3.5f&&minUp>.8f,$"Three real PhysX laps: {steps*.02:F2}s, max centre error {maxError:F3}m, min upright {minUp:F3}, airborne steps {airborne}; 4.5/8 m/s target; no teleport during traversal");
                hud.SendMessage("LateUpdate");Check(hud.display.text.Contains("Completed: 3")&&hud.display.text.Contains("Race complete"),"HUD reflects actual simulated finish");
                race.RestartRace();hud.SendMessage("LateUpdate");Check(race.Progress.CompletedLaps==0&&race.Progress.LastLap==0&&race.Progress.BestLap==0&&race.Progress.RaceTime(race.Clock)==0&&hud.display.text.Contains("00:00.000"),"Restart after actual race clears laps, last/best, total and HUD");
            }
            finally
            {
                if(pad!=null)InputSystem.RemoveDevice(pad);if(keyboard!=null)InputSystem.RemoveDevice(keyboard);
                InputSystem.Update();input.SendMessage("Update");Physics.simulationMode=mode;body.interpolation=interpolation;
                car.enabled=carEnabled;reset.enabled=resetEnabled;race.enabled=raceEnabled;race.RestartRace();
            }
            report.Add("Failures: "+failures);report.Add("Virtual input and simulated driving only. Physical controller and subjective steering comfort require Dan.");
            File.WriteAllLines("Docs/PHASE3_TEST_RESULTS.txt",report);Debug.Log(string.Join("\n",report));
        }
    }
}
