using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Racer
{
    // Explicit isolated CR112–118 checks only; no old-course or ghost assertions.
    public sealed partial class CR112Validation:MonoBehaviour
    {
        static string Arg(string key,string fallback=""){var a=Environment.GetCommandLineArgs();int i=Array.IndexOf(a,key);return i>=0&&i+1<a.Length?a[i+1]:fallback;}
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Boot(){if(Arg("-cr112Check")==""||Arg("-racerTestSave")==""||FindAnyObjectByType<CR112Validation>())return;var g=new GameObject("CR112 bounded checks");DontDestroyOnLoad(g);var test=g.AddComponent<CR112Validation>();if(Arg("-cr112Check")=="title"){test.titleCapture=FindAnyObjectByType<AudioListener>().gameObject.AddComponent<CorrectionAudioCapture>();test.titleCapture.Begin(8);}}
        string dir;RaceDirector race;RaceFlow flow;ArcadeVehicle car;readonly List<string> checks=new();
        void Check(bool pass,string text){checks.Add((pass?"PASS ":"FAIL ")+text);File.WriteAllLines(dir+"/checks.txt",checks);}
        void Bind(){race=FindAnyObjectByType<RaceDirector>();flow=race.Flow;car=race.vehicle;}
        IEnumerator Load(string scene){if(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name!=scene)UnityEngine.SceneManagement.SceneManager.LoadScene(scene);yield return null;yield return null;Bind();}
        static float Ground(Vector3 p)=>Physics.RaycastAll(new(p.x,400,p.z),Vector3.down,800,1,QueryTriggerInteraction.Ignore).Where(h=>h.collider.name.StartsWith("Ground_")||h.collider.name=="Decorative Road pavement").Select(h=>h.point.y).DefaultIfEmpty(-999).Max();
        void Place(Vector3 p,Vector3 f){p.y=Ground(p)+car.suspensionLength-.12f;car.Body.position=p;car.Body.rotation=Quaternion.LookRotation(Vector3.ProjectOnPlane(f,Vector3.up));car.transform.SetPositionAndRotation(p,car.Body.rotation);car.Body.linearVelocity=car.Body.angularVelocity=Vector3.zero;car.ClearSteering();Physics.SyncTransforms();race.ResetSampling(p,race.Clock);FindAnyObjectByType<ChaseCamera>()?.Snap();}
        void Shot(string name,Vector3 position,Vector3 target){var c=Camera.main;var p=c.transform.position;var q=c.transform.rotation;c.transform.position=position;c.transform.LookAt(target);ThreeFeatureValidation.CaptureUi(dir+"/"+name+".png");c.transform.SetPositionAndRotation(p,q);}
        IEnumerator Start()
        {
            Application.runInBackground=true;dir=Arg("-evidence");Directory.CreateDirectory(dir);
            if(Arg("-cr112Check")=="title"){yield return TitleOutput();Application.Quit();yield break;}
            yield return Load(Arg("-course","MountainLoop"));
            if(Arg("-cr112Check")=="flight")yield return Flight();
            else if(Arg("-cr112Check")=="naming"){yield return Naming();ModelChecks();}
            else if(Arg("-cr112Check")=="households")yield return Households();
            else if(Arg("-cr112Check")=="playlist")yield return ChampionshipLaps();
            else if(Arg("-cr112Check")=="roads")yield return RoadSurfaces();
            else if(Arg("-cr112Check")=="alternates")yield return Alternates();
            else if(Arg("-cr112Check")=="turkeys")yield return TurkeyVisits();
            else if(Arg("-cr112Check")=="endpoints")yield return HighwayEndpoints();
            else if(Arg("-cr112Check")=="climb")yield return ClimbSegment();
            else if(Arg("-cr112Check")=="scoring")yield return ScoringFixture();
            else if(Arg("-cr112Check")=="reverse-local")yield return ReverseLocal();
            File.WriteAllText(dir+"/done.txt","Bounded normal-motor-input automation. Read checks for actual outcomes; no human fun or physical controller approval.");Application.Quit();
        }
        IEnumerator Flight()
        {
            race.opponents=race.traffic=false;car.GetComponent<VehicleConfiguration>().Apply(Arg("-vehicle","moto"));flow.StartFreeRoam();car.enabled=false;car.GetComponent<VehicleInput>().enabled=false;
            var flight=race.GetComponent<MountainFlights>().flights[int.Parse(Arg("-flight","0"))];float offset=float.Parse(Arg("-offset","0"));var right=Vector3.Cross(Vector3.up,flight.forward).normalized;
            Place(flight.start+flight.forward*5+right*offset,flight.forward);float begin=Time.time,air=0,speed=0,minClear=999,landedAt=0;Vector3 takeoff=default,landing=default;bool launched=false,landed=false,obstruction=false;int stable=0;
            RoadDriver pilot=null;if(Arg("-pilot")=="yes"){pilot=car.gameObject.AddComponent<RoadDriver>();pilot.Initialize(race,car,true,1,1);pilot.Racer=race.Racers[0];var contact=car.gameObject.AddComponent<CR117ContactLog>();contact.Path=dir+"/flight-contacts.txt";}
            float lip=Vector3.Dot(flight.lip-flight.start,flight.forward);
            using(var w=new StreamWriter(dir+"/flight.csv")){
                w.WriteLine("time,x,y,z,speed,wheels,up,station,clearance,throttle,brake,steer");
                while(Time.time-begin<45){var delta=car.Body.position-flight.start;float side=Vector3.Dot(delta,right),s=Vector3.Dot(delta,flight.forward);float yaw=Vector3.SignedAngle(Vector3.ProjectOnPlane(car.transform.forward,Vector3.up),flight.forward,Vector3.up);float steer=Mathf.Clamp(yaw*.045f+(offset-side)*.1f,-.35f,.35f);float throttle=landed?Mathf.Clamp01((12-car.ForwardSpeed)*.6f):1;float brake=landed&&Time.time-landedAt>.8f&&car.ForwardSpeed>14?.4f:0;
                    if(!pilot)car.Simulate(throttle,brake,steer,Time.fixedDeltaTime);yield return new WaitForFixedUpdate();if(pilot){throttle=pilot.LastThrottle;brake=pilot.LastBrake;steer=float.NaN;}var p=car.Body.position;s=Vector3.Dot(p-flight.start,flight.forward);var bounds=car.GetComponent<Collider>().bounds;float clear=999;foreach(float x in new[]{bounds.min.x,bounds.center.x,bounds.max.x})foreach(float z in new[]{bounds.min.z,bounds.center.z,bounds.max.z})clear=Math.Min(clear,bounds.min.y-Ground(new(x,0,z)));
                    if(!launched&&s>lip-2&&car.GroundedWheels<2){launched=true;takeoff=p;speed=car.Body.linearVelocity.magnitude;ThreeFeatureValidation.CaptureUi(dir+"/takeoff-chase.png");}
                    if(launched&&!landed){air+=Time.fixedDeltaTime;if(air>2&&air<2.04f)ThreeFeatureValidation.CaptureUi(dir+"/flight-chase.png");if(s>lip+5&&s<lip+90){minClear=Math.Min(minClear,clear);if(clear<=0||car.GroundedWheels>=2)obstruction=true;}if(air>1&&(car.GroundedWheels>=2||clear<=.15f)){landed=true;landedAt=Time.time;landing=p;ThreeFeatureValidation.CaptureUi(dir+"/landing-chase.png");}}
                    w.WriteLine($"{Time.time-begin},{p.x},{p.y},{p.z},{car.ForwardSpeed},{car.GroundedWheels},{car.transform.up.y},{s},{clear},{throttle},{brake},{steer}");
                    if(landed&&car.GroundedWheels>=2&&car.transform.up.y>.65f)stable++;else stable=0;if(stable>65||Math.Abs(side)>35)break;
                }
            }
            float distance=Vector3.ProjectOnPlane(landing-takeoff,Vector3.up).magnitude;
            race.road.Project(landing,out float landingRoadDistance);
            Check(launched&&landed&&air>=3&&distance>=90&&!obstruction&&stable>65&&(!pilot||(pilot.RecoveryCount==0&&landingRoadDistance<8)),$"{flight.name} {Arg("-vehicle")} offset={offset}: takeoff={speed:F2} m/s air={air:F2}s distance={distance:F2}m clear={minClear:F2}m obstruction={obstruction} stable={stable} landingRoadDistance={landingRoadDistance:F2}");
            File.WriteAllText(dir+"/flight-result.json",JsonUtility.ToJson(new FlightResult{flight=flight.name,profile=Arg("-vehicle"),offset=offset,speed=speed,air=air,distance=distance,clearance=minClear,launched=launched,landed=landed,obstruction=obstruction,stable=stable},true));
        }
        [Serializable]sealed class FlightResult{public string flight,profile;public float offset,speed,air,distance,clearance;public bool launched,landed,obstruction;public int stable;}
    }
}
