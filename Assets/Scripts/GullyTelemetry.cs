using System;
using System.IO;
using UnityEngine;

namespace Racer
{
    // Opt-in contact/speed evidence; never attached during ordinary play.
    public sealed class GullyTelemetry : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"-gullyTelemetry");
            if(i>=0 && i+1<args.Length && Array.IndexOf(args,"-racerTestSave")>=0)
            {Output=Path.GetFullPath(args[i+1]);FindAnyObjectByType<RaceDirector>().vehicle.gameObject.AddComponent<GullyTelemetry>();}
        }
        public static string Output = "Temp/gully-baseline.csv";
        StreamWriter writer;
        ArcadeVehicle car;
        Transform house;
        void Start()
        {
            car=GetComponent<ArcadeVehicle>();
            house=GameObject.Find("Fox Gully drive-through house (former 44 -451)").transform;
            writer=new StreamWriter(Output);
            writer.WriteLine("time,profile,x,y,z,houseX,houseY,houseZ,speed,forward,grounded,throttle,brake,up,wipeout,contact,impulse,nx,ny,nz,separation");
        }
        void FixedUpdate()=>Row("",0,Vector3.zero,0);
        void OnCollisionEnter(Collision c)=>Contact(c);
        void OnCollisionStay(Collision c)=>Contact(c);
        void Contact(Collision c)
        {
            for(int i=0;i<c.contactCount;i++)
            { var p=c.GetContact(i); Row(c.collider.name,c.impulse.magnitude,p.normal,p.separation); }
        }
        void Row(string contact,float impulse,Vector3 n,float separation)
        {
            if(writer==null||!house)return;
            var p=car.Body.position;var h=house.InverseTransformPoint(p);var input=car.GetComponent<VehicleInput>();var config=car.GetComponent<VehicleConfiguration>();
            writer.WriteLine(FormattableString.Invariant($"{Time.time:F3},{config.profileId},{p.x:F3},{p.y:F3},{p.z:F3},{h.x:F3},{h.y:F3},{h.z:F3},{car.Body.linearVelocity.magnitude:F3},{car.ForwardSpeed:F3},{car.GroundedWheels},{input.Throttle:F3},{input.BrakeReverse:F3},{car.transform.up.y:F3},{config.WipedOut},{contact.Replace(',','_')},{impulse:F3},{n.x:F3},{n.y:F3},{n.z:F3},{separation:F4}"));
            if(Time.frameCount%60==0)writer.Flush();
        }
        void OnDestroy(){writer?.Dispose();}
    }
}
