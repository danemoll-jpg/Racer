using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Racer;
using Object=UnityEngine.Object;

public static class LocalCorrectiveChecks
{
    const string Folder="Docs/LocalRecovery";
    const BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
    static void Require(bool condition,string message){if(!condition)throw new Exception(message);}
    public static string Recovery()
    {
        var rows=new List<string>();float now=10;
        foreach(var scene in new[]{"StreetLoopGreybox","StreetLoopReverse","ForestLoopReverse","MountainLoop","MountainLoopReverse"}){
            EditorSceneManager.OpenScene("Assets/Scenes/"+scene+".unity");var race=Object.FindAnyObjectByType<RaceDirector>();var car=race.vehicle;var reset=car.GetComponent<VehicleRespawn>();var road=race.road;
            typeof(ArcadeVehicle).GetMethod("Awake",Private).Invoke(car,null);typeof(VehicleRespawn).GetMethod("Awake",Private).Invoke(reset,null);
            foreach(var body in Object.FindObjectsByType<Rigidbody>())if(body!=car.Body)body.gameObject.SetActive(false);
            race.Racers.Clear();var state=new RacerState("check",car,race.gates.Length-1,3);race.Racers.Add(state);
            var record=typeof(VehicleRespawn).GetMethod("RecordSafePosition",Private,null,new[]{typeof(float)},null);
            var support=typeof(VehicleRespawn).GetMethod("Supported",Private);var wheels=typeof(ArcadeVehicle).GetProperty("GroundedWheels");
            void Sample(float station,int contacts,WoodlandRoute branch=null,bool airborne=false){
                var p=branch?branch.At(station,out var f):road.At(station,out f);var args=new object[]{p,Vector3.ProjectOnPlane(f,Vector3.up).normalized,Vector3.zero,Quaternion.identity};
                bool supported=(bool)support.Invoke(reset,args);if(supported){p=(Vector3)args[2];}else p+=Vector3.up*.55f;
                if(airborne)p.y+=12;
                var rotation=supported?(Quaternion)args[3]:Quaternion.LookRotation(f);
                car.transform.SetPositionAndRotation(p,rotation);car.Body.position=p;car.Body.rotation=rotation;car.Body.linearVelocity=f*18;car.Body.angularVelocity=Vector3.zero;wheels.SetValue(car,contacts);Physics.SyncTransforms();record.Invoke(reset,new object[]{now});now+=.11f;
            }
            float origin=road.Project(car.transform.position,out _)+20;reset.CancelRecovery();reset.SeedCoursePosition(road.At(origin,out _));
            for(int i=0;i<36;i++)Sample(origin+i,2);
            float anchor=reset.SafeStation;Require(Math.Abs(anchor-(origin+35))<5,$"{scene}: normal two-contact driving left anchor stale ({anchor} vs {origin+35})");
            Require(anchor<=origin+35+.05f,$"{scene}: forward recovery credit");
            Sample(origin+38,0,null,true);Require(Math.Abs(reset.SafeStation-anchor)<.001f,scene+": airborne updated safe point");
            int next=state.Progress.NextGate;Require(reset.TryRecoverLocal(),scene+": recent occupied support was not usable");Require(state.Progress.NextGate==next,scene+": recovery awarded gate progress");
            Require(Math.Abs(road.Project(car.Body.position,out _)-anchor)<2,scene+": did not recover to recent station");
            reset.ResetVehicle();Require(reset.Pending,scene+": short delay not queued");float retry=(float)typeof(VehicleRespawn).GetField("nextAttempt",Private).GetValue(reset);Require(retry-Time.time>.7f&&retry-Time.time<.9f,scene+": unexpected reset delay");reset.CancelRecovery();
            rows.Add($"PASS {scene}: two supported contacts advance every 0.1s after 0.35s/1m stability; recent reset succeeds; no airborne/forward/gate credit; 0.8s requested-reset delay.");
            foreach(var flight in race.GetComponent<MountainFlights>()?.flights??Array.Empty<MountainFlights.Flight>()){
                float before=road.Project(flight.start,out _)-75;reset.SeedCoursePosition(road.At(before,out _));for(int i=0;i<9;i++)Sample(before+i,4);float old=reset.SafeStation;
                float lip=road.Project(flight.lip,out _),finish=road.Project(flight.landingEnd,out _)+20;
                for(float s=before+10;s<finish;s+=6){bool ramp=s>=road.Project(flight.start,out _)-55&&s<=lip;Sample(s,ramp?4:0,null,!ramp);Require(Math.Abs(reset.SafeStation-old)<.01f,$"{scene}/{flight.name}: ramp/airborne anchor");}
                for(int i=0;i<25;i++)Sample(finish+i,2);
                Require(Math.Abs(reset.SafeStation-(finish+24))<5,$"{scene}/{flight.name}: landing failed to advance: {reset.SafeStation}, target {finish+24}");
                var history=(IList)typeof(VehicleRespawn).GetField("history",Private).GetValue(reset);Require(history.Count>0,"No landing history");
                foreach(var h in history)Require((float)h.GetType().GetField("station").GetValue(h)>=finish-1,"Pre-jump fallback retained");
                float post=reset.SafeStation;Sample(finish+27,0,null,true);Require(reset.TryRecoverLocal(),"Post-jump recovery unavailable");Require(Math.Abs(reset.SafeStation-post)<.01f,"Later crash returned before completed jump");
                rows.Add($"PASS {scene}/{flight.name}: ramp and air rejected; post-landing anchor={post:F2}; pre-jump history retired; later crash resets after jump.");reset.CancelRecovery();
            }
            if(scene=="StreetLoopReverse"){
                var branch=Object.FindObjectsByType<WoodlandRoute>().Single(b=>b.title=="Laurel Switchbacks");state.Branch.Begin(branch);var zone=Object.FindObjectsByType<JumpRecoveryExclusion>().Single(z=>z.name.StartsWith("Laurel straight"));
                float start=branch.Project(zone.start,out _);reset.CancelRecovery();reset.SeedCoursePosition(branch.At(start-40,out _));
                for(int i=0;i<15;i++)Sample(start-35+i,4,branch);float old=reset.SafeStation;
                for(float s=start;s<start+55;s+=2)Sample(s,4,branch);Require(Math.Abs(old-reset.SafeStation)<.01f,"Laurel runway created recovery anchor");
                Sample(start+64,0,branch,true);for(int i=0;i<20;i++)Sample(start+76+i,2,branch);
                Require(reset.SafeStation>start+76,"Laurel direct main-road landing left pre-jump anchor active");rows.Add("PASS Laurel: occupied pre-runway sample retained across ramp/flight; stable main-road landing replaces it.");
            }
            now+=10;
        }
        EditorSceneManager.OpenScene("Assets/Scenes/StreetLoopReverse.unity");File.WriteAllLines(Folder+"/recovery-checks.txt",rows);return string.Join("\n",rows);
    }
    public static string LaurelGeometry()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/StreetLoopReverse.unity");Physics.SyncTransforms();var race=Object.FindAnyObjectByType<RaceDirector>();
        var zone=Object.FindObjectsByType<JumpRecoveryExclusion>().Single(z=>z.name.StartsWith("Laurel straight"));var axis=Vector3.ProjectOnPlane(zone.end-zone.start,Vector3.up).normalized;var lip=zone.end-axis*2;var rows=new List<string>();int count=0;float maxError=0,maxAngle=0;
        foreach(float lane in new[]{-3f,0,3}){Vector3 previous=Vector3.up;
            for(float s=0;s<=55;s+=.25f){float u=Mathf.Clamp01((s-30)/25);var p=zone.start+axis*s+Vector3.up*(.65f*u*u)+Vector3.Cross(Vector3.up,axis)*lane;
                var hits=Physics.RaycastAll(p+Vector3.up*4,Vector3.down,8,1,QueryTriggerInteraction.Ignore).Where(h=>!h.rigidbody).OrderBy(h=>h.distance).ToArray();Require(hits.Length>0,$"Missing runway collision {s}/{lane}");
                var hit=hits[0];Require(hit.collider.name=="Ground_Laurel continuous driving surface",$"Runway obstacle {s}/{lane}: {hit.collider.name}");maxError=Mathf.Max(maxError,Math.Abs(hit.point.y-p.y));if(s>0)maxAngle=Mathf.Max(maxAngle,Vector3.Angle(previous,hit.normal));previous=hit.normal;count++;
                Require(hits.Count(h=>Math.Abs(h.point.y-p.y)<.03f)==1,$"Duplicate runway surface {s}/{lane}");
            }
        }
        Require(maxError<.005f&&maxAngle<1,$"Runway continuity error={maxError}, angle={maxAngle}");
        foreach(var mf in GameObject.Find("Local Laurel replacement").GetComponentsInChildren<MeshFilter>())if(mf.TryGetComponent<MeshCollider>(out var c))Require(c.sharedMesh==mf.sharedMesh,"Visible/collision mismatch");
        Require(!GameObject.Find("CR133 straight Laurel")&&!GameObject.Find("CR122 Laurel local jump"),"Old malformed overlays retained");
        rows.Add($"PASS straight approach/ramp: {count} cross-lane samples; max height error {maxError:F6}m; adjacent angle {maxAngle:F3} degrees; no duplicate colliders; removed old overlays.");
        foreach(float speed in new[]{18f,24,32,38}){
            bool landed=false;Vector3 contact=default;
            for(float t=.12f;t<2.5f;t+=.01f){var p=lip+axis*(speed*t)+Vector3.up*(speed*.052f*t+.5f*Physics.gravity.y*t*t);
                var hits=Physics.RaycastAll(p+Vector3.up*20,Vector3.down,60,1,QueryTriggerInteraction.Ignore).Where(h=>!h.rigidbody&&h.collider.name=="Ground_Laurel continuous driving surface").OrderBy(h=>h.distance).ToArray();
                if(hits.Length>0&&p.y<=hits[0].point.y+.02f){landed=true;contact=hits[0].point;break;}
            }
            Require(landed,$"Existing-performance flight has no main-road landing at {speed}m/s");float station=race.road.Project(contact,out _);Require(station>3929&&station<3999,"Flight landed on shortcut, not main road");rows.Add($"PASS unboosted geometric flight envelope {speed}m/s: main-road contact at {contact}, station {station:F2}.");
        }
        File.WriteAllLines(Folder+"/Laurel-checks.txt",rows);
        Shot("Laurel-straight",zone.start-axis*6+Vector3.up*3,lip+axis*20);
        Shot("Laurel-overview",new Vector3(380,175,-345),new Vector3(398,65,-230));
        Shot("Laurel-landing",lip-axis*15+Vector3.up*8,race.road.At(3970,out _));
        return string.Join("\n",rows);
    }
    static void Shot(string name,Vector3 position,Vector3 target)
    {
        var go=new GameObject("Temporary localized review camera",typeof(Camera));var c=go.GetComponent<Camera>();c.transform.SetPositionAndRotation(position,Quaternion.LookRotation(target-position));c.fieldOfView=65;c.farClipPlane=1500;
        var rt=new RenderTexture(1280,800,24);var texture=new Texture2D(1280,800,TextureFormat.RGB24,false);var old=RenderTexture.active;
        try{c.targetTexture=rt;c.Render();RenderTexture.active=rt;texture.ReadPixels(new Rect(0,0,1280,800),0,0);texture.Apply();File.WriteAllBytes(Folder+"/"+name+".png",texture.EncodeToPNG());}
        finally{RenderTexture.active=old;Object.DestroyImmediate(texture);Object.DestroyImmediate(rt);Object.DestroyImmediate(go);}
    }
}
