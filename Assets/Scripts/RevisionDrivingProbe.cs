using System.Collections;
using System.IO;
using System.Globalization;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Racer
{
    // Explicit opt-in diagnostic: ordinary Update/FixedUpdate and actual input actions.
    public sealed class RevisionDrivingProbe : MonoBehaviour
    {
        public static string Label = "baseline";
        public static bool FullSuite;
        RaceDirector race;
        Gamepad pad;
        StreamWriter log;
        string scenario;
        float start, previousSpeed;
        public static void Launch() => new GameObject("Revision driving probe").AddComponent<RevisionDrivingProbe>();
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            var args=System.Environment.GetCommandLineArgs();
            if(System.Array.IndexOf(args,"-racerRevisionDriving")>=0 && System.Array.IndexOf(args,"-racerTestSave")>=0)
            { Application.runInBackground=true; Label="standalone"; FullSuite=true; Launch(); }
        }
        IEnumerator Start()
        {
            yield return null;
            race=FindAnyObjectByType<RaceDirector>();
#if UNITY_EDITOR
            race.Flow.UseValidationSave(Path.GetFullPath("Temp/revision-probe-save"));
            InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
#endif
            Application.runInBackground=true;
            InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
            pad=InputSystem.AddDevice<Gamepad>();
            Directory.CreateDirectory("Docs/CR028-033");
            log=new StreamWriter("Docs/CR028-033/"+Label+"-driving.csv");
            log.WriteLine("profile,scenario,time,speed,vx,vy,vz,throttle,brake,steering,grounded,wipeout,x,y,z,up,deltaSpeed,contact,impulse,nx,ny,nz,separation");
            race.opponents=race.traffic=false;
            File.WriteAllText("Docs/CR028-033/"+Label+"-measurements.txt","Ordinary-frame virtual Gamepad; no injected launch/speed; from rest each approach.\n");
            foreach(var id in FullSuite?new[]{"original","tourer","moto","atv"}:new[]{"moto","atv"})
            {
                race.Flow.CompleteResults(); race.Flow.OpenGarage(); race.Flow.SelectVehicle(id); race.Flow.CloseGarage(); race.Flow.StartRace();
                while(race.Flow.State!=RaceFlow.Stage.Racing) yield return null;
                var observer=race.vehicle.gameObject.AddComponent<RevisionContactProbe>(); observer.Owner=this;
                for(int repeat=0;repeat<2;repeat++)
                {
                    yield return Drive("road-"+repeat,new(-565,0,536),0,12);
                    yield return Drive("hill-"+repeat,new(319,0,400),1.7f,16);
                    yield return Drive("ramp-"+repeat,new(-627,0,-360),-1.5f,18);
                }
                Destroy(observer);
            }
            InputSystem.QueueStateEvent(pad,new GamepadState());
            InputSystem.RemoveDevice(pad); pad=null;
            log.Dispose(); log=null;
            race.Flow.Pause();
            File.WriteAllText("Docs/CR028-033/"+Label+"-done.txt","Ordinary-frame probe complete; virtual input, no physical-controller coverage.");
        }
        IEnumerator Drive(string name,Vector3 near,float lane,float duration)
        {
            var car=race.vehicle; var road=race.road;
            float s=road.Project(near,out _);
            var p=road.At(s,out var f)+Vector3.Cross(Vector3.up,f).normalized*lane+Vector3.up*.8f;
            car.Body.position=p; car.Body.rotation=Quaternion.LookRotation(Vector3.ProjectOnPlane(f,Vector3.up));
            car.transform.SetPositionAndRotation(p,car.Body.rotation); car.Body.linearVelocity=car.Body.angularVelocity=Vector3.zero;
            car.ClearSteering(); race.Progress.ResetToGrid(); race.ResetSampling(p,Time.timeAsDouble); Physics.SyncTransforms();
            FindAnyObjectByType<ChaseCamera>().Snap(); scenario=name; start=Time.time; previousSpeed=0;
            float entry=0,takeoff=0,air=0,landSpeed=0,apex=0,minUp=1,landZ=0; bool flew=false,landed=false;
            var jump=GameObject.Find("Phase 4 - Connector Jump").transform;
            while(Time.time-start<duration)
            {
                float at=road.Project(car.Body.position,out _), look=Mathf.Clamp(7+Mathf.Abs(car.ForwardSpeed)*.45f,8,25);
                var target=road.At(at+look,out var heading)+Vector3.Cross(Vector3.up,heading).normalized*lane;
                var local=car.transform.InverseTransformPoint(target);
                float curvature=2*local.x/Mathf.Max(1,local.x*local.x+local.z*local.z);
                float angle=Mathf.Lerp(car.slowSteerAngle,car.fastSteerAngle,Mathf.Clamp01(Mathf.Abs(car.ForwardSpeed)/car.topSpeed));
                float steer=Mathf.Clamp(Mathf.Atan(curvature*car.wheelbase)*Mathf.Rad2Deg/angle,-1,1);
                float desired=name.StartsWith("hill")?32:name.EndsWith("0")?36:42;
                float throttle=FullSuite && !name.StartsWith("road")?Mathf.Clamp01((desired-car.ForwardSpeed)*.6f):1;
                float brake=FullSuite && !name.StartsWith("road") && car.ForwardSpeed>desired+1?Mathf.Clamp01((car.ForwardSpeed-desired)*.3f):0;
                InputSystem.QueueStateEvent(pad,new GamepadState{rightTrigger=throttle,leftTrigger=brake,leftStick=new(Mathf.Abs(steer)<.001f?0:Mathf.Sign(steer)*(.12f+Mathf.Abs(steer)*.83f),0)});
                var jumpLocal=jump.InverseTransformPoint(car.Body.position);
                minUp=Mathf.Min(minUp,car.transform.up.y);
                if(jumpLocal.z>=0 && entry==0) entry=car.Body.linearVelocity.magnitude;
                if(name.StartsWith("ramp") && jumpLocal.z>20 && jumpLocal.z<220)
                {
                    apex=Mathf.Max(apex,jumpLocal.y);
                    if(car.GroundedWheels<2 && !landed) { if(!flew) takeoff=car.Body.linearVelocity.magnitude; flew=true; air+=Time.deltaTime; }
                    else if(flew && !landed && air>.1f) { landed=true; landSpeed=car.Body.linearVelocity.magnitude; landZ=jumpLocal.z; }
                }
                Row("",0,Vector3.zero,0); yield return null;
            }
            File.AppendAllText("Docs/CR028-033/"+Label+"-measurements.txt",$"{car.GetComponent<VehicleConfiguration>().profileId} {name}: entry={entry:F2}m/s takeoff={takeoff:F2}m/s air={air:F2}s apexAboveRampBase={apex:F2}m landingLocalZ={landZ:F2}m landingSpeed={landSpeed:F2}m/s landed={landed} minUp={minUp:F3} final={car.Body.position}\n");
            log.Flush(); InputSystem.QueueStateEvent(pad,new GamepadState());
            yield return new WaitForSeconds(.2f);
        }
        public void Contact(Collision c)
        {
            for(int i=0;i<c.contactCount;i++) { var p=c.GetContact(i); Row(c.collider.name.Replace(',','_'),c.impulse.magnitude,p.normal,p.separation); }
        }
        void Row(string contact,float impulse,Vector3 normal,float separation)
        {
            if(log==null || scenario==null) return;
            var c=race.vehicle; var v=c.Body.linearVelocity; var p=c.Body.position; var input=c.GetComponent<VehicleInput>(); float speed=v.magnitude;
            log.WriteLine(string.Format(CultureInfo.InvariantCulture,"{0},{1},{2:F3},{3:F3},{4:F3},{5:F3},{6:F3},{7:F3},{8:F3},{9:F3},{10},{11},{12:F3},{13:F3},{14:F3},{15:F3},{16:F3},{17},{18:F3},{19:F3},{20:F3},{21:F3},{22:F4}",c.GetComponent<VehicleConfiguration>().profileId,scenario,Time.time-start,speed,v.x,v.y,v.z,input.Throttle,input.BrakeReverse,input.Steering,c.GroundedWheels,c.GetComponent<VehicleConfiguration>().WipedOut,p.x,p.y,p.z,c.transform.up.y,speed-previousSpeed,contact,impulse,normal.x,normal.y,normal.z,separation));
            if(contact.Length==0) previousSpeed=speed;
        }
        void OnDestroy() { log?.Dispose(); if(pad!=null) InputSystem.RemoveDevice(pad); }
    }
    public sealed class RevisionContactProbe : MonoBehaviour
    {
        public RevisionDrivingProbe Owner;
        void OnCollisionEnter(Collision c) => Owner.Contact(c);
        void OnCollisionStay(Collision c) => Owner.Contact(c);
    }
}
