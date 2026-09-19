using System.Collections;
using System.IO;
using UnityEngine;
namespace Racer
{
    // Explicit physical diagnostic. Initial speed/pose are fixtures; thereafter pedals and suspension only.
    public sealed class ForestProbe : MonoBehaviour
    {
        public static string Label="before";
        public static float[] Stations={620,1020,1390};
        public static bool Nominal;
        static string EvidenceRoot="Docs/CR056";
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            var args=System.Environment.GetCommandLineArgs();if(System.Array.IndexOf(args,"-forestProbe")<0||System.Array.IndexOf(args,"-racerTestSave")<0)return;
            if(FindAnyObjectByType<ForestProbe>())return;
            var g=new GameObject("Forest standalone probe");DontDestroyOnLoad(g);g.AddComponent<ForestProbe>();Label="after";Stations=new float[]{340,610,900,1020,1310,1540};
            Nominal=System.Array.IndexOf(args,"-nominal")>=0;if(Nominal)Label="nominal";
            int evidence=System.Array.IndexOf(args,"-forestEvidence");if(evidence>=0&&evidence+1<args.Length)EvidenceRoot=args[evidence+1];
            if(System.Array.IndexOf(args,"-creekView")>=0){Stations=new float[]{340};Label="creek-view";}
        }
        RaceDirector race; StreamWriter log; string contact=""; float impulse;
        public static void Launch()=>new GameObject("Forest physical probe").AddComponent<ForestProbe>();
        IEnumerator Start()
        {
            if(System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"-forestProbe")>=0&&UnityEngine.SceneManagement.SceneManager.GetActiveScene().name!="LakeWoods")UnityEngine.SceneManagement.SceneManager.LoadScene("LakeWoods");
            yield return null;yield return null;race=FindAnyObjectByType<RaceDirector>();
#if UNITY_EDITOR
            race.Flow.UseValidationSave(Path.GetFullPath("Temp/forest-probe-save"));
#endif
            Application.runInBackground=true;Directory.CreateDirectory(EvidenceRoot+"/"+Label);
            log=new StreamWriter(EvidenceRoot+"/"+Label+"/telemetry.csv");
            log.WriteLine("vehicle,jump,time,station,speed,throttle,brake,steer,grounded,up,angularSpeed,contact,impulse,suspensionLift,alignmentTorque");
            race.opponents=race.traffic=false;
            foreach(string id in new[]{"moto","atv"}) foreach(float station in Stations)
            {
                if(race.Flow.State!=RaceFlow.Stage.Ready){race.Flow.Pause();race.Flow.QuitRace();}
                race.Flow.OpenGarage();race.Flow.SelectVehicle(id);race.Flow.CloseGarage();race.Flow.StartRace();
                while(race.Flow.State!=RaceFlow.Stage.Racing)yield return null;
                Time.timeScale=3;var car=race.vehicle;car.enabled=false;car.GetComponent<VehicleInput>().enabled=false;
                var observer=car.gameObject.AddComponent<ForestContactProbe>();observer.owner=this;
                var p=race.road.At(station-45,out var f);car.Body.position=p+Vector3.up*.7f;car.Body.rotation=Quaternion.LookRotation(Vector3.ProjectOnPlane(f,Vector3.up));
                car.transform.SetPositionAndRotation(car.Body.position,car.Body.rotation);car.Body.linearVelocity=f*32;car.Body.angularVelocity=Vector3.zero;car.ClearSteering();race.ResetSampling(car.Body.position,Time.timeAsDouble);
                float start=Time.time,air=0,min=100,takeoff=0,landing=0;bool flew=false,completed=false,captured=false;int creekFrame=0;
                while(Time.time-start<12)
                {
                    float s=race.road.Project(car.Body.position,out _);var local=car.transform.InverseTransformPoint(race.road.At(s+15,out _));
                    float curvature=2*local.x/Mathf.Max(1,local.x*local.x+local.z*local.z),angle=Mathf.Lerp(car.slowSteerAngle,car.fastSteerAngle,Mathf.Clamp01(car.ForwardSpeed/car.topSpeed));
                    float steer=Mathf.Clamp(Mathf.Atan(curvature*car.wheelbase)*Mathf.Rad2Deg/angle,-1,1);
                    float throttle=Nominal?Mathf.Clamp01((32-car.ForwardSpeed)*.5f):1,brake=Nominal&&car.ForwardSpeed>34?.3f:0;
                    car.Simulate(throttle,brake,steer,Time.fixedDeltaTime);float speed=car.Body.linearVelocity.magnitude;
                    if(s>=station && s<station+50)min=Mathf.Min(min,speed);
                    if(s>=station&&car.GroundedWheels<2){if(!flew)takeoff=speed;flew=true;air+=Time.fixedDeltaTime;}
                    else if(flew&&air>.15f&&landing==0)landing=speed;
                    log.WriteLine($"{id},{station},{Time.time-start:F3},{s:F3},{speed:F3},{throttle:F3},{brake:F3},{steer:F3},{car.GroundedWheels},{car.transform.up.y:F3},{car.Body.angularVelocity.magnitude:F3},{contact},{impulse:F3},{car.SuspensionLift:F3},{car.AlignmentTorque:F3}");contact="";impulse=0;
                    if(Nominal&&!captured&&id=="moto"&&s>station+55&&car.GroundedWheels<2){ThreeFeatureValidation.CaptureUi(EvidenceRoot+"/nominal/jump-"+station+".png");captured=true;}
                    if(Nominal&&id=="moto"&&station==340&&creekFrame<4&&s>station+36+creekFrame*4){FindAnyObjectByType<ChaseCamera>().Snap();ThreeFeatureValidation.CaptureUi(EvidenceRoot+"/"+Label+"/creek-"+creekFrame+++".png");}
                    if(s>station+150&&car.GroundedWheels>=2){completed=true;break;}
                    yield return new WaitForFixedUpdate();
                }
                File.AppendAllText(EvidenceRoot+"/"+Label+"/summary.txt",$"{id} {station}: initial=32 minLaunch={min:F2} takeoff={takeoff:F2} landing={landing:F2} airtime={air:F2} up={car.transform.up.y:F2} completed={completed}\n");
                log.Flush();Destroy(observer);
            }
            log.Dispose();race.Flow.Pause();File.WriteAllText(EvidenceRoot+"/"+Label+"/done.txt","Complete; physical scripted pedal test, not human acceptance.");
            if(!Application.isEditor)Application.Quit();
        }
        public void Contact(Collision c){contact=c.collider.name.Replace(',',' ');impulse=Mathf.Max(impulse,c.impulse.magnitude);}
    }
    public sealed class ForestContactProbe:MonoBehaviour
    {public ForestProbe owner;void OnCollisionEnter(Collision c)=>owner.Contact(c);void OnCollisionStay(Collision c)=>owner.Contact(c);}
}



