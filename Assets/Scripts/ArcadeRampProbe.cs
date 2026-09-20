using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace Racer
{
    // Explicit diagnostic: setup precedes the run; all measured traversal uses ordinary motor frames.
    public sealed class ArcadeRampProbe : MonoBehaviour
    {
        public string evidence="Docs/CR075-080/baseline";
        public bool full;
        public float[] lines;
        public bool Done {get;private set;}
        static string Arg(string key,string fallback){var args=Environment.GetCommandLineArgs();int n=Array.IndexOf(args,key);return n>=0&&n+1<args.Length?args[n+1]:fallback;}
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot(){var args=Environment.GetCommandLineArgs();if(!Array.Exists(args,a=>a=="-arcadeRamp")||!Array.Exists(args,a=>a=="-racerTestSave")||FindAnyObjectByType<ArcadeRampProbe>())return;var go=new GameObject("Arcade ramp matrix");DontDestroyOnLoad(go);var probe=go.AddComponent<ArcadeRampProbe>();probe.full=true;probe.evidence=Arg("-evidence","Docs/CR075-080/ramp-matrix");}
        string contact="";
        public void Contact(Collision c){if(c.contactCount>0)contact=c.collider.name+":"+c.GetContact(0).normal.ToString("F2");}
        IEnumerator Start()
        {
            if(Array.Exists(Environment.GetCommandLineArgs(),a=>a=="-arcadeRamp")){string scene=Arg("-course","StreetLoopReverse");if(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name!=scene)UnityEngine.SceneManagement.SceneManager.LoadScene(scene);yield return null;yield return null;Application.runInBackground=true;QualitySettings.vSyncCount=0;Application.targetFrameRate=120;}
            Directory.CreateDirectory(evidence);var race=FindAnyObjectByType<RaceDirector>();var flow=race.Flow;var car=race.vehicle;
            var probe=car.gameObject.AddComponent<RampContacts>();probe.owner=this;
            var root=GameObject.Find("Phase 4 - Connector Jump").transform;
            bool reversedRamp=race.reverseCourse&&!race.Forest;
            bool guardMode=Arg("-arcadeRamp","")=="recovery";
            bool activityMode=Arg("-arcadeRamp","")=="activities"||guardMode;
            var rows=new List<string>{"course,vehicle,speed,line,seconds,travel,air,minUp,recovered,outcome,scoredDistance,scoredAirtime,awards"};
            foreach(var profile in Array.FindAll(race.EligibleVehicles,p=>Arg("-rampVehicle","")==""||p.Id==Arg("-rampVehicle","")))
            foreach(float speed in float.TryParse(Arg("-rampSpeed",""),System.Globalization.NumberStyles.Float,System.Globalization.CultureInfo.InvariantCulture,out float chosenSpeed)?new[]{chosenSpeed}:Arg("-arcadeRamp","")=="high"?new[]{43f}:guardMode?new[]{28f}:activityMode?new[]{18f,28f,38f}:full?new[]{12f,24f,36f}:new[]{12f})
            foreach(float line in float.TryParse(Arg("-rampLine",""),System.Globalization.NumberStyles.Float,System.Globalization.CultureInfo.InvariantCulture,out float chosenLine)?new[]{chosenLine}:activityMode?new[]{-1.5f}:lines??(full?new[]{-1.5f,-3f,0f,-4.7f,1.7f}:new[]{-1.5f,1.7f}))
            {
                if(flow.State!=RaceFlow.Stage.Ready){flow.Pause();flow.QuitRace();}flow.OpenGarage();flow.SelectVehicle(profile.Id);flow.CloseGarage();race.opponents=race.traffic=false;if(activityMode||race.Forest)flow.StartFreeRoam();else flow.StartRace();
                while(flow.State!=RaceFlow.Stage.Racing)yield return null;
                car.enabled=false;car.GetComponent<VehicleInput>().enabled=false;
                var f=root.forward*(reversedRamp?-1:1);var start=root.TransformPoint(new Vector3(line,0,reversedRamp?125:-40));
                if(Physics.Raycast(start+Vector3.up*20,Vector3.down,out var ground,50,1))start.y=ground.point.y+car.suspensionLength-.12f;
                car.Body.position=start;car.Body.rotation=Quaternion.LookRotation(f);car.transform.SetPositionAndRotation(start,car.Body.rotation);car.Body.linearVelocity=f*speed;car.Body.angularVelocity=Vector3.zero;car.ClearSteering();Physics.SyncTransforms();race.ResetSampling(start,race.Clock);
                FindAnyObjectByType<ChaseCamera>()?.Snap();float begin=Time.time,air=0,minUp=1,travel=0;bool passed=false;
                int awards=flow.Activities.Awards;if(activityMode){flow.Activities.NewSession();flow.Activities.BeginAttempt();}
                using(var trace=new StreamWriter(evidence+"/"+profile.Id+"-"+speed+"-"+line+".csv"))
                {
                    trace.WriteLine("time,speed,travel,x,y,z,grounded,lift,torque,contact");
                    while(Time.time-begin<22)
                    {
                        float lateral=Vector3.Dot(car.Body.position-start,root.right);float yaw=Vector3.SignedAngle(car.transform.forward,f,Vector3.up);
                        car.Simulate(Mathf.Clamp01((speed-car.ForwardSpeed)*.5f),car.ForwardSpeed>speed+1?.2f:0,Mathf.Clamp(yaw*.045f-lateral*(reversedRamp?-.10f:.10f),-.5f,.5f),Time.fixedDeltaTime);
                        yield return new WaitForFixedUpdate();travel=Vector3.Dot(car.Body.position-start,f);if(car.GroundedWheels<2)air+=Time.fixedDeltaTime;minUp=Mathf.Min(minUp,car.transform.up.y);
                        var p=car.Body.position;trace.WriteLine($"{Time.time-begin:F3},{car.ForwardSpeed:F3},{travel:F3},{p.x:F3},{p.y:F3},{p.z:F3},{car.GroundedWheels},{car.SuspensionLift:F3},{car.AlignmentTorque:F3},\"{contact}\"");contact="";
                        if(guardMode&&air>.35f&&car.GroundedWheels==0){passed=car.GetComponent<VehicleRespawn>().TryRecoverLocal();break;}
                        if(travel>150&&car.GroundedWheels>=2){passed=true;break;}
                    }
                }
                int supported=0;
                int settleFrames=Mathf.CeilToInt(float.Parse(Arg("-rampSettle","0.6"),System.Globalization.CultureInfo.InvariantCulture)/Time.fixedDeltaTime);
                for(int settle=0;settle<settleFrames;settle++)
                {
                    car.Simulate(0,0,0,Time.fixedDeltaTime);yield return new WaitForFixedUpdate();minUp=Mathf.Min(minUp,car.transform.up.y);
                    supported=car.GroundedWheels>=2&&car.transform.up.y>=.65f?supported+1:0;
                }
                ThreeFeatureValidation.CaptureUi(evidence+"/"+profile.Id+"-"+speed+"-"+line+".png");
                if(guardMode)passed&=!flow.Activities.AttemptActive&&flow.Activities.Awards==awards;
                string outcome=!passed?"stalled/crashed":guardMode?"midair reset excluded":supported<15?"cleared / unstable landing":"completed";
                bool recovered=car.GetComponent<VehicleRespawn>().TryRecoverLocal();rows.Add($"{race.courseName},{profile.Id},{speed},{line},{Time.time-begin:F3},{travel:F3},{air:F3},{minUp:F3},{recovered},{outcome},{flow.Activities.LastDistance:F3},{flow.Activities.LastAirtime:F3},{flow.Activities.Awards-awards}");File.WriteAllLines(evidence+"/ramps.csv",rows);
                car.enabled=true;
            }
            Done=true;File.WriteAllText(evidence+"/done.txt","Completed; inspect failures in ramps.csv");flow.Pause();
            if(Array.Exists(Environment.GetCommandLineArgs(),a=>a=="-arcadeRamp"))Application.Quit();
        }
    }
    public sealed class RampContacts:MonoBehaviour {public ArcadeRampProbe owner;void OnCollisionStay(Collision c)=>owner.Contact(c);}
}
