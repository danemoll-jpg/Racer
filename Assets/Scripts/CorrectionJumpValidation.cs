using System;
using System.IO;
using System.Linq;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
namespace Racer
{
    public sealed class CorrectionJumpValidation:MonoBehaviour
    {
        static string Dir { get {var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"-jumpEvidence");return i>=0&&i+1<args.Length?args[i+1]:"Docs/CR041-045/jamerson";} }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot(){var a=Environment.GetCommandLineArgs();if(a.Contains("-correctionJump")&&a.Contains("-racerTestSave")){Application.runInBackground=true;new GameObject("Jamerson real frame flights").AddComponent<CorrectionJumpValidation>();}}
        IEnumerator Start()
        {
            yield return null;Directory.CreateDirectory(Dir);var race=FindAnyObjectByType<RaceDirector>();
            var pad=InputSystem.AddDevice<Gamepad>();InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
            File.WriteAllText(Dir+"/results.csv","profile,attempt,entry,apexAboveBase,airSeconds,landed,gate12Credited,misses,penalty,finalUp\n");
            foreach(var profile in VehicleProfile.All)
            {
                race.Flow.OpenGarage();race.Flow.SelectVehicle(profile.Id);race.Flow.CloseGarage();race.opponents=race.traffic=false;race.Flow.StartRace();while(race.Flow.State!=RaceFlow.Stage.Racing)yield return null;
                for(int attempt=0;attempt<2;attempt++)
                {
                    var car=race.vehicle;float station=race.road.Project(new Vector3(-627,0,-360),out _);var p=race.road.At(station,out var f)+Vector3.Cross(Vector3.up,f).normalized*-1.5f+Vector3.up*.7f;
                    car.Body.position=p;car.Body.rotation=Quaternion.LookRotation(Vector3.ProjectOnPlane(f,Vector3.up));car.transform.SetPositionAndRotation(p,car.Body.rotation);car.Body.linearVelocity=car.Body.angularVelocity=Vector3.zero;car.ClearSteering();Physics.SyncTransforms();race.ResetSampling(p,Time.timeAsDouble);FindAnyObjectByType<ChaseCamera>().Snap();
                    race.Progress.Restart();race.Progress.Cross(0,true,0);for(int i=1;i<=10;i++)race.Progress.Cross(i,true,i);
                    race.Racers[0].Branch.Clear();var ramp=GameObject.Find("Phase 4 - Connector Jump").transform;
                    float start=Time.time,air=0,apex=0,entry=0;bool airborne=false,landed=false;
                    using(var trace=new StreamWriter(Dir+"/"+profile.Id+"-"+attempt+".csv"))
                    {
                        trace.WriteLine("time,x,y,z,speed,grounded,nextGate,misses,penalty");float next=0;
                        while(Time.time-start<19)
                        {
                            float s=race.road.Project(car.Body.position,out _);float look=Mathf.Clamp(7+Mathf.Abs(car.ForwardSpeed)*.45f,8,25);var target=race.road.At(s+look,out var ahead)+Vector3.Cross(Vector3.up,ahead).normalized*-1.5f;
                            var local=car.transform.InverseTransformPoint(target);float curvature=2*local.x/Mathf.Max(1,local.x*local.x+local.z*local.z);float angle=Mathf.Lerp(car.slowSteerAngle,car.fastSteerAngle,Mathf.Clamp01(Mathf.Abs(car.ForwardSpeed)/car.topSpeed));float steer=Mathf.Clamp(Mathf.Atan(curvature*car.wheelbase)*Mathf.Rad2Deg/angle,-1,1);float desired=attempt==0?36:car.topSpeed;
                            InputSystem.QueueStateEvent(pad,new GamepadState{rightTrigger=Mathf.Clamp01((desired-car.ForwardSpeed)*.6f),leftTrigger=car.ForwardSpeed>desired+1?Mathf.Clamp01((car.ForwardSpeed-desired)*.3f):0,leftStick=new(Mathf.Abs(steer)<.001f?0:Mathf.Sign(steer)*(.12f+.83f*Mathf.Abs(steer)),0)});
                            var q=ramp.InverseTransformPoint(car.Body.position);if(q.z>=0&&entry==0)entry=car.ForwardSpeed;
                            if(q.z>35&&q.z<260){apex=Mathf.Max(apex,q.y);if(car.GroundedWheels<2&&!landed){airborne=true;air+=Time.deltaTime;}else if(airborne&&air>.1f)landed=true;}
                            if(Time.time>=next){next=Time.time+.1f;var v=car.Body.position;trace.WriteLine($"{Time.time-start:F3},{v.x:F3},{v.y:F3},{v.z:F3},{car.ForwardSpeed:F3},{car.GroundedWheels},{race.Progress.NextGate},{race.Progress.MissedGates},{race.Progress.PenaltySeconds}");}
                            yield return null;
                        }
                    }
                    File.AppendAllText(Dir+"/results.csv",$"{profile.Id},{attempt},{entry:F3},{apex:F3},{air:F3},{landed},{race.Progress.NextGate>=13},{race.Progress.MissedGates},{race.Progress.PenaltySeconds},{car.transform.up.y:F3}\n");
                    InputSystem.QueueStateEvent(pad,new GamepadState());yield return new WaitForSeconds(.2f);
                }
                race.Flow.Pause();race.Flow.QuitRace();
            }
            InputSystem.RemoveDevice(pad);File.WriteAllText(Dir+"/done.txt","Eight ordinary-frame flights complete; review credit, landings and penalties independently.");Application.Quit();
        }
    }
}
