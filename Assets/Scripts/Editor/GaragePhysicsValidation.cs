using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;

namespace Racer.Editor
{
    public static class GaragePhysicsValidation
    {
        static readonly List<string> rows=new();
        static PhysicsScene physics;
        static Scene scene;
        public static void Run()
        {
            if(!Application.isPlaying) throw new InvalidOperationException("Run in Play mode after RaceFlow initializes.");
            rows.Clear();
            scene=SceneManager.CreateScene("Isolated garage physics",new CreateSceneParameters(LocalPhysicsMode.Physics3D));
            physics=scene.GetPhysicsScene();
            var ground=GameObject.CreatePrimitive(PrimitiveType.Cube); SceneManager.MoveGameObjectToScene(ground,scene);
            ground.transform.position=new(0,-.5f,0); ground.transform.localScale=new(4000,1,4000);
            try
            {
                foreach(var profile in VehicleProfile.All) Measure(profile.Id);
                foreach(string small in new[]{"moto","atv"}) foreach(float speed in new[]{8f,25f,45f}) foreach(string contact in new[]{"rear","side","glance","stationary","sustained"}) foreach(bool smallInitiates in new[]{false,true}) Contact(small,speed,contact,smallInitiates);
                SaveChecks();
                Directory.CreateDirectory("Docs/CR026-027"); File.WriteAllLines("Docs/CR026-027/physics.txt",rows);
            }
            finally { SceneManager.UnloadSceneAsync(scene); }
        }
        static ArcadeVehicle Create(string id,Vector3 position,Quaternion rotation)
        {
            var source=UnityEngine.Object.FindAnyObjectByType<RaceDirector>().vehicle;
            var go=UnityEngine.Object.Instantiate(source.gameObject); SceneManager.MoveGameObjectToScene(go,scene);
            foreach(var driver in go.GetComponents<RoadDriver>()) { driver.enabled=false; UnityEngine.Object.Destroy(driver); }
            go.GetComponent<VehicleInput>().enabled=false; go.GetComponent<VehicleRespawn>().enabled=false;
            var audio=go.GetComponent<VehicleAudio>(); if(audio) { audio.enabled=false; UnityEngine.Object.Destroy(audio); }
            foreach(var voice in go.GetComponents<AudioSource>()) UnityEngine.Object.Destroy(voice);
            var car=go.GetComponent<ArcadeVehicle>(); car.enabled=false;
            go.GetComponent<VehicleConfiguration>().Apply(id);
            car.Body.isKinematic=false; car.Body.position=position; car.Body.rotation=rotation; go.transform.SetPositionAndRotation(position,rotation);
            car.Body.linearVelocity=car.Body.angularVelocity=Vector3.zero;
            return car;
        }
        static void Step(ArcadeVehicle car,float throttle,float brake,float steer,int count)
        {
            for(int i=0;i<count;i++) { car.Simulate(throttle,brake,steer,.02f); physics.Simulate(.02f); }
        }
        static void Remove(ArcadeVehicle car) { VehicleContact.Unregister(car.Body); UnityEngine.Object.DestroyImmediate(car.gameObject); }
        static void Measure(string id)
        {
            var car=Create(id,new(0,.8f,0),Quaternion.identity);
            Step(car,0,0,0,100); Step(car,1,0,0,1000);
            float speed=car.ForwardSpeed; Step(car,0,1,0,150); float reverse=car.ForwardSpeed;
            car.Body.position=new(0,.8f,0); car.Body.rotation=Quaternion.identity; car.transform.SetPositionAndRotation(car.Body.position,car.Body.rotation);
            car.Body.linearVelocity=Vector3.forward*20; car.Body.angularVelocity=Vector3.zero; car.ClearSteering();
            Step(car,0,0,.3f,25); float yaw=car.Body.angularVelocity.y;
            float minUp=1; for(int i=0;i<250;i++) { Step(car,.55f,0,.3f,1); minUp=Mathf.Min(minUp,car.transform.up.y); }
            rows.Add($"{id}: straight20s={speed:F3}m/s; brake/reverse3s={reverse:F3}; yawAt0.5s20mps30percent={yaw:F3}rad/s; 5sTurnMinUp={minUp:F3}");
            Remove(car);
        }
        static void Contact(string small,float speed,string kind,bool smallInitiates)
        {
            var a=Create("original",new(0,.8f,0),Quaternion.identity);
            var b=Create(small,new(0,.8f,8),Quaternion.identity);
            Step(a,0,0,0,70); Step(b,0,0,0,70);
            var hitter=smallInitiates?b:a; var target=smallInitiates?a:b;
            Vector3 forward=kind=="side"?Vector3.right:Vector3.forward;
            hitter.Body.position=target.Body.position-forward*8+(kind=="glance"?Vector3.right*.8f:Vector3.zero);
            hitter.Body.rotation=Quaternion.LookRotation(forward); hitter.transform.SetPositionAndRotation(hitter.Body.position,hitter.Body.rotation);
            hitter.Body.linearVelocity=forward*speed;
            if(kind!="stationary" && kind!="sustained" && kind!="side") target.Body.linearVelocity=Vector3.forward*speed*.25f;
            Vector3 carInitial=a.Body.linearVelocity;
            var control=Create("original",a.Body.position+Vector3.right*100,a.Body.rotation);
            control.Body.linearVelocity=a.Body.linearVelocity;
            Vector3 initialOffset=control.Body.position-a.Body.position;
            float maxCarDelta=0,maxSmallAngular=0,maxVelocity=0,maxPenetration=0;
            for(int i=0;i<(kind=="sustained"?500:150);i++)
            {
                // Coast contact fixtures isolate collision effects; sustained pedals expose pushing exploits.
                a.Simulate(kind=="sustained"&&!smallInitiates?1:0,0,0,.02f);
                b.Simulate(kind=="sustained"&&smallInitiates?1:0,0,0,.02f);
                control.Simulate(kind=="sustained"&&!smallInitiates?1:0,0,0,.02f);
                physics.Simulate(.02f);
                maxCarDelta=Mathf.Max(maxCarDelta,Mathf.Abs(a.Body.linearVelocity.x-carInitial.x));
                maxSmallAngular=Mathf.Max(maxSmallAngular,b.Body.angularVelocity.magnitude);
                maxVelocity=Mathf.Max(maxVelocity,a.Body.linearVelocity.magnitude,b.Body.linearVelocity.magnitude);
                if(Physics.ComputePenetration(a.GetComponent<BoxCollider>(),a.Body.position,a.Body.rotation,b.GetComponent<BoxCollider>(),b.Body.position,b.Body.rotation,out _,out float distance)) maxPenetration=Mathf.Max(maxPenetration,distance);
            }
            float carDeviation=Vector3.Distance(control.Body.position-initialOffset,a.Body.position);
            int contacts=b.GetComponent<VehicleConfiguration>().VehicleContactEvents;
            rows.Add($"{(carDeviation<.1f && maxPenetration<.12f && maxVelocity<65 && contacts>0?"PASS":"FAIL")} contact {small} {speed} {kind} {(smallInitiates?"small->car":"car->small")}: events={contacts}, carVsNoContactDeviation={carDeviation:F4}m, carLateralDelta={maxCarDelta:F3}, smallAngular={maxSmallAngular:F3}, maxSpeed={maxVelocity:F3}, maxOverlap={maxPenetration:F3}; final car={a.Body.position} small={b.Body.position}");
            Remove(a);Remove(b);Remove(control);
        }
        static void SaveChecks()
        {
            string directory=Path.GetFullPath("Temp/CR026-027-records-"+Guid.NewGuid().ToString("N")); Directory.CreateDirectory(directory);
            File.WriteAllText(Path.Combine(directory,"settings.json"),"{\"version\":1,\"master\":0.3,\"vehicle\":0.2,\"opponents\":false}");
            var save=new RacerSave(directory,"legacy");
            rows.Add($"migration: original={save.Settings.vehicleId=="original"} normal={save.Settings.difficulty==1} volume={save.Settings.master}/{save.Settings.vehicle} solo={!save.Settings.opponents}");
            save.SelectRecords("original-solo"); save.RecordRace(123); save.SelectRecords("moto-solo"); bool separate=save.Best.race==0; save.RecordRace(98); save.SelectRecords("original-solo");
            rows.Add($"record separation={separate&&save.Best.race==123}; legacy/settings preserved={File.Exists(Path.Combine(directory,"settings.json"))}");
        }
    }
}
