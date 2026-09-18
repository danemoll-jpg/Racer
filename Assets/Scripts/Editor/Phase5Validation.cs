using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object=UnityEngine.Object;

namespace Racer.Editor
{
    public static class Phase5Validation
    {
        const float Dt=.02f;
        static ArcadeVehicle car;static Rigidbody body;static RaceDirector race;
        static List<string> log;static List<Vector3> road,cut;
        static float[] Limits(List<Vector3> path)
        {
            var limits=new float[path.Count];
            for(int i=0;i<path.Count;i++)
            {
                var a=path[(i+path.Count-4)%path.Count];var b=path[i];var c=path[(i+4)%path.Count];
                float curvature=Vector3.Angle(Vector3.ProjectOnPlane(b-a,Vector3.up),Vector3.ProjectOnPlane(c-b,Vector3.up))*Mathf.Deg2Rad/Mathf.Max(1,Vector3.Distance(a,c)*.5f);
                limits[i]=Mathf.Clamp(Mathf.Sqrt(7/Mathf.Max(.001f,curvature)),7,40);
                if(b.y>20)limits[i]=Mathf.Min(limits[i],22);
                if(b.x<-610&&b.z>-145&&b.z<55)limits[i]=32;
                if(b.x>-622&&b.x<-555&&b.z>-555&&b.z<-445)limits[i]=Mathf.Min(limits[i],26);
            }
            for(int pass=0;pass<3;pass++)for(int i=path.Count-1;i>=0;i--){int n=(i+1)%path.Count;limits[i]=Mathf.Min(limits[i],Mathf.Sqrt(limits[n]*limits[n]+14*Vector3.Distance(path[i],path[n])));}
            return limits;
        }
        static List<Vector3> MakeCut()
        {
            var path=Phase5Setup.Path();var result=new List<Vector3>();
            result.AddRange(road.GetRange(0,Phase5Setup.EntryIndex));
            // Match road sample spacing, keeping the same steering look-ahead in metres.
            var previous=path[0];result.Add(previous);float carry=0;
            for(int i=1;i<path.Count;i++)
            {
                float distance=Vector3.Distance(previous,path[i]);
                while(carry+distance>=2){float f=(2-carry)/distance;previous=Vector3.Lerp(previous,path[i],f);result.Add(previous);distance=Vector3.Distance(previous,path[i]);carry=0;}
                carry+=distance;previous=path[i];
            }
            result.AddRange(road.GetRange(Phase5Setup.ExitIndex,road.Count-Phase5Setup.ExitIndex));return result;
        }
        static void Step(float throttle,float brake,float steer){car.Simulate(throttle,brake,steer,Dt);Physics.Simulate(Dt);}
        static void Place(List<Vector3> path,int index,float speed,float lateral=0,float yaw=0)
        {
            var forward=Vector3.ProjectOnPlane(path[(index+1)%path.Count]-path[index],Vector3.up).normalized;
            var p=path[index]+Vector3.Cross(Vector3.up,forward)*lateral;
            if(!Physics.Raycast(p+Vector3.up*30,Vector3.down,out var hit,80,1))throw new Exception("Missing start support");
            body.position=hit.point+Vector3.up*.65f;body.rotation=Quaternion.LookRotation(forward)*Quaternion.Euler(0,yaw,0);
            car.transform.SetPositionAndRotation(body.position,body.rotation);body.linearVelocity=body.angularVelocity=Vector3.zero;car.ClearSteering();Physics.SyncTransforms();
            for(int i=0;i<75;i++)Step(0,0,0);body.linearVelocity=car.transform.forward*speed;
        }
        static int Advance(List<Vector3> path,int index)
        {
            float best=float.MaxValue;int found=index;
            for(int j=-5;j<=30;j++){int k=(index+j+path.Count)%path.Count;var d=body.position-path[k];d.y=0;if(d.sqrMagnitude<best){best=d.sqrMagnitude;found=k;}}return found;
        }
        static void Drive(List<Vector3> path,int index,float desired,float bias=0)
        {
            int ahead=Mathf.Max(3,Mathf.RoundToInt(Mathf.Abs(car.ForwardSpeed)*.28f));int k=(index+ahead)%path.Count;
            var local=car.transform.InverseTransformPoint(path[k]);float curvature=2*local.x/Mathf.Max(1,local.x*local.x+local.z*local.z);
            float angle=Mathf.Lerp(car.slowSteerAngle,car.fastSteerAngle,Mathf.Clamp01(Mathf.Abs(car.ForwardSpeed)/car.topSpeed));
            float steer=Mathf.Clamp(Mathf.Atan(curvature*car.wheelbase)*Mathf.Rad2Deg/angle+bias,-1,1);
            float speed=car.ForwardSpeed;Step(Mathf.Clamp01((desired-speed)*.7f+.35f),speed>desired+.3f?Mathf.Clamp01((speed-desired)*.5f):0,steer);
        }
        [MenuItem("Racer/Validate Phase 5 Timing (Play mode)")]
        public static void Timing()=>Run("timing");
        public static void Run(string mode)
        {
            if(!Application.isPlaying)throw new Exception("Enter Play mode first");
            car=Object.FindAnyObjectByType<ArcadeVehicle>();body=car.Body;race=Object.FindAnyObjectByType<RaceDirector>();road=StreetLoopBuilder.Route();cut=MakeCut();log=new();
            var reset=car.GetComponent<VehicleRespawn>();var oldMode=Physics.simulationMode;var interp=body.interpolation;bool enabled=car.enabled,re=reset.enabled;
            try
            {
                Physics.simulationMode=SimulationMode.Script;body.interpolation=RigidbodyInterpolation.None;car.enabled=false;reset.enabled=false;
                if(mode=="timing")TimingTests();else if(mode=="recovery")Recovery();else Laps(mode);
            }
            finally{Physics.simulationMode=oldMode;body.interpolation=interp;car.enabled=enabled;reset.enabled=re;race.RestartRace();File.WriteAllLines("Docs/PHASE5_"+mode.ToUpperInvariant()+".txt",log);}
            Debug.Log("Phase5 "+mode+" complete; see Docs report. Virtual motor commands, manually stepped PhysX; no physical controller.");
        }
        static void TimingTests()
        {
            var start=road[Phase5Setup.EntryIndex-20];var end=road[Phase5Setup.ExitIndex+25];
            log.Add($"Common timing planes: start {start}, end {end}; approaches start at same location with injected speed; settle 1.5s excluded. End plane crossing interpolated. Entire approach and rejoin included.");
            foreach(bool shortcut in new[]{false,true})foreach(float speed in new[]{22f,24f,26f})Trial(shortcut,speed,0,0,false,end);
            foreach(float speed in new[]{6f,24f,34f,42f})foreach(float yaw in new[]{-4f,4f})Trial(true,speed,0,yaw,true,end);
            foreach(float offset in new[]{-2f,2f,-5f,5f})Trial(true,26,offset,0,true,end);
            foreach(float yaw in new[]{-10f,10f})Trial(true,34,0,yaw,true,end);
        }
        static void Trial(bool shortcut,float speed,float lateral,float yaw,bool stress,Vector3 end)
        {
            var path=shortcut?cut:road;var limits=Limits(path);int index=Phase5Setup.EntryIndex-20;
            Place(path,index,speed,lateral,yaw);float peak=0,sum=0,minUp=1,maxError=0,minClear=99;int steps=0,air=0;bool reached=false;float time=0,entrySpeed=0,rejoinSpeed=0;
            var endForward=(road[Phase5Setup.ExitIndex+26]-end).normalized;
            for(;steps<2500;steps++)
            {
                var before=body.position;index=Advance(path,index);
                // Stress cases sustain their target through the corner, exposing overspeed limits.
                Drive(path,index,stress?speed:limits[index]);
                peak=Mathf.Max(peak,car.ForwardSpeed);sum+=car.ForwardSpeed;minUp=Mathf.Min(minUp,car.transform.up.y);
                var delta=body.position-path[index];delta.y=0;maxError=Mathf.Max(maxError,delta.magnitude);if(car.GroundedWheels<2)air++;
                if(Physics.Raycast(body.position+Vector3.up*15,Vector3.down,out var hit,50,1))minClear=Mathf.Min(minClear,body.position.y-hit.point.y);else minClear=-999;
                if(entrySpeed==0&&index>=Phase5Setup.EntryIndex)entrySpeed=car.ForwardSpeed;
                if(rejoinSpeed==0&&body.position.z>road[Phase5Setup.ExitIndex].z)rejoinSpeed=car.ForwardSpeed;
                float a=Vector3.Dot(before-end,endForward),b=Vector3.Dot(body.position-end,endForward);
                if(a<0&&b>=0){time=(steps+Mathf.Clamp01(-a/(b-a)))*Dt;reached=true;break;}
                if(minUp<.5f||maxError>18||minClear<0)break;
            }
            bool clean=reached&&minUp>.8f&&maxError<(shortcut?2.6f:4.5f)&&minClear>0;
            log.Add($"{(clean?"PASS":"FAIL")} {(stress?"STRESS":"PAIRED")} {(shortcut?"SHORTCUT":"NORMAL")} initial={speed:F1}m/s offset={lateral:F1}m yaw={yaw:F1}: reached={reached}; time={time:F3}s; mean={sum/Mathf.Max(1,steps+1):F2}; peak={peak:F2}; entry={entrySpeed:F2}; rejoin={rejoinSpeed:F2}m/s; max center error={maxError:F2}m; up={minUp:F3}; min clearance={minClear:F3}m; airborne={air*Dt:F2}s; end={body.position}");
        }
        public static void ValidatePhase7Mixed()
        {
            car=Object.FindAnyObjectByType<ArcadeVehicle>(); body=car.Body; race=Object.FindAnyObjectByType<RaceDirector>();
            string isolatedRoot=System.IO.Path.GetFullPath("Docs/Phase7")+System.IO.Path.DirectorySeparatorChar;
            if(!Application.isPlaying || race.Flow.State!=RaceFlow.Stage.Racing || race.Progress.Started || !System.IO.Path.GetFullPath(race.Flow.Save.DirectoryPath).StartsWith(isolatedRoot,StringComparison.OrdinalIgnoreCase))
                throw new Exception("Start a fresh race with isolated Phase7 validation storage and wait for countdown.");
            road=StreetLoopBuilder.Route();cut=MakeCut();log=new();
            var reset=car.GetComponent<VehicleRespawn>();var mode=Physics.simulationMode;var interp=body.interpolation;
            try { Physics.simulationMode=SimulationMode.Script;body.interpolation=RigidbodyInterpolation.None;car.enabled=false;reset.enabled=false;Laps("mixed",false); }
            finally { Physics.simulationMode=mode;body.interpolation=interp;race.RestartRace();File.WriteAllLines("Docs/Phase7/mixed-laps.txt",log); }
        }
        static void Laps(string mode, bool restart=true)
        {
            if(restart)race.RestartRace();var path=mode=="normal"?road:cut;var limits=Limits(path);int index=10;Place(path,index,0);race.ResetSampling(body.position,0);
            float peak=0,sum=0,error=0,up=1;int steps=0;
            for(;steps<90000&&!race.Progress.Finished;steps++)
            {
                index=Advance(path,index);Drive(path,index,limits[index]);int before=race.Progress.CompletedLaps;race.Sample(body.position,car.transform.forward,(steps+1)*Dt);
                var d=body.position-path[index];d.y=0;error=Mathf.Max(error,d.magnitude);up=Mathf.Min(up,car.transform.up.y);peak=Mathf.Max(peak,car.ForwardSpeed);sum+=car.ForwardSpeed;
                if(race.Progress.CompletedLaps>before)
                {
                    log.Add($"LAP {race.Progress.CompletedLaps}: {race.Progress.LastLap:F3}s via {(path==road?"normal":"shortcut")}");
                    if(mode=="mixed"){path=path==road?cut:road;limits=Limits(path);index=Phase5Setup.Closest(path,body.position);}
                }
                if(!race.Progress.LapValid&&race.Progress.Started||error>9||up<.5f){log.Add("STOP "+race.Progress.Status+" at "+body.position);break;}
            }
            log.Add($"{(race.Progress.Finished?"PASS":"FAIL")} {mode}: completed={race.Progress.CompletedLaps}; valid={race.Progress.LapValid}; time={steps*Dt:F2}s; mean={sum/Mathf.Max(1,steps):F2}m/s; peak={peak:F2}m/s; max error={error:F2}m; min up={up:F3}");
        }
        static void Recovery()
        {
            var path=Phase5Setup.Path();int missing=0,obstacles=0;float maxStep=0;
            for(int i=0;i<path.Count;i++)foreach(float offset in new[]{-3f,0,3f})
            {
                var f=path[Mathf.Min(i+1,path.Count-1)]-path[Mathf.Max(0,i-1)];var p=path[i]+Vector3.Cross(Vector3.up,f).normalized*offset;
                if(!Physics.Raycast(p+Vector3.up*30,Vector3.down,out var hit,60,1)){missing++;continue;}
                if(!hit.collider.transform.IsChildOf(GameObject.Find("Memory loop - north is +Z").transform))obstacles++;
                if(i>0){var prev=path[i-1]+Vector3.Cross(Vector3.up,f).normalized*offset;if(Physics.Raycast(prev+Vector3.up*30,Vector3.down,out var h,60,1))maxStep=Mathf.Max(maxStep,Mathf.Abs(hit.point.y-h.point.y));}
            }
            log.Add($"SUPPORT 273 samples: missing={missing}; obstructed={obstacles}; maximum neighboring height difference={maxStep:F3}m (sample spacing varies).");
            foreach(int location in new[]{20,45,75,90})
            {
                race.RestartRace();race.Progress.Cross(0,true,0);for(int g=1;g<=11;g++)race.Progress.Cross(g,true,g);
                body.position=path[location]+Vector3.right*9+Vector3.up;body.linearVelocity=Vector3.one*20;body.rotation=Quaternion.Euler(0,0,110);
                car.GetComponent<VehicleRespawn>().ResetVehicle();
                bool resetOk=!race.Progress.LapActive&&race.Progress.NextGate==0&&body.linearVelocity.sqrMagnitude==0&&car.transform.up.y>.99f;
                for(int g=12;g<race.gates.Length;g++)race.Progress.Cross(g,true,20+g);race.Progress.Cross(0,true,50);
                log.Add($"{(resetOk&&race.Progress.CompletedLaps==0?"PASS":"FAIL")} failed shortcut reset point {location}: no remaining-gate/finish credit; completed={race.Progress.CompletedLaps}");
            }
            foreach(string mode in new[]{"skip","repeat","wrongway"})
            {
                race.RestartRace();race.Progress.Cross(0,true,0);
                for(int g=1;g<race.gates.Length;g++)
                {if(mode=="skip"&&g==7)continue;race.Progress.Cross(g,true,g);if(g==11&&mode=="repeat")race.Progress.Cross(g,true,g+.1);if(g==11&&mode=="wrongway")race.Progress.Cross(g,false,g+.1);}
                race.Progress.Cross(0,true,40);log.Add($"{(race.Progress.CompletedLaps==0?"PASS":"FAIL")} {mode}: no lap awarded");
            }
            // Physical recovery from the shoulder on each transition, plus a missed entrance.
            foreach(int point in new[]{Phase5Setup.EntryIndex-4,Phase5Setup.ExitIndex+2})foreach(float side in new[]{-8f,8f})
            {
                int index=point;Place(road,index,18,side,0);float minUp=1,minClear=99;
                for(int s=0;s<350;s++){index=Advance(road,index);Drive(road,index,18);minUp=Mathf.Min(minUp,car.transform.up.y);if(Physics.Raycast(body.position+Vector3.up*20,Vector3.down,out var hit,60,1))minClear=Mathf.Min(minClear,body.position.y-hit.point.y);}
                var d=body.position-road[index];d.y=0;log.Add($"RECOVERY transition index={point}, offset={side}m target=18m/s: end error={d.magnitude:F2}m; up={minUp:F3}; clearance={minClear:F3}; speed={car.ForwardSpeed:F2}");
            }
            foreach(bool entry in new[]{true,false})foreach(float yaw in new[]{-5f,5f})
            {
                var drivingPath=entry?cut:road;int index=entry?Phase5Setup.EntryIndex-2:Phase5Setup.ExitIndex-2;
                Place(drivingPath,index,24,0,yaw);float error=0,minUp=1,minClear=99;
                for(int s=0;s<220;s++){index=Advance(drivingPath,index);Drive(drivingPath,index,24);var d=body.position-drivingPath[index];d.y=0;error=Mathf.Max(error,d.magnitude);minUp=Mathf.Min(minUp,car.transform.up.y);if(Physics.Raycast(body.position+Vector3.up*20,Vector3.down,out var hit,60,1))minClear=Mathf.Min(minClear,body.position.y-hit.point.y);}
                log.Add($"ANGLED {(entry?"entrance":"re-entry")} injected 24m/s yaw={yaw}: max error={error:F2}m; upright={minUp:F3}; clearance={minClear:F3}m; final speed={car.ForwardSpeed:F2}m/s");
            }
        }
        public static void Realtime()
        {
            if(!Application.isPlaying||EditorApplication.isPaused)throw new Exception("Play unpaused first");
            car=Object.FindAnyObjectByType<ArcadeVehicle>();body=car.Body;race=Object.FindAnyObjectByType<RaceDirector>();road=StreetLoopBuilder.Route();cut=MakeCut();
            var settings=UnityEngine.InputSystem.InputSystem.settings;var focus=settings.editorInputBehaviorInPlayMode;var background=settings.backgroundBehavior;bool run=Application.runInBackground;
            settings.editorInputBehaviorInPlayMode=UnityEngine.InputSystem.InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            settings.backgroundBehavior=UnityEngine.InputSystem.InputSettings.BackgroundBehavior.IgnoreFocus;Application.runInBackground=true;
            var pad=UnityEngine.InputSystem.InputSystem.AddDevice<UnityEngine.InputSystem.Gamepad>();
            int index=Phase5Setup.EntryIndex-20;var mode=Physics.simulationMode;Physics.simulationMode=SimulationMode.Script;
            Place(cut,index,24);Physics.simulationMode=mode;Object.FindAnyObjectByType<ChaseCamera>().Snap();
            float start=Time.time,peak=0,error=0,minUp=1,cameraDistance=0;int frames=0;double wall=EditorApplication.timeSinceStartup;
            EditorApplication.CallbackFunction tick=null;
            tick=()=>
            {
                bool reached=false;string failure=null;
                try
                {
                    index=Advance(cut,index);peak=Mathf.Max(peak,car.ForwardSpeed);var d=body.position-cut[index];d.y=0;error=Mathf.Max(error,d.magnitude);minUp=Mathf.Min(minUp,car.transform.up.y);frames++;
                    cameraDistance=Mathf.Max(cameraDistance,Vector3.Distance(Camera.main.transform.position,body.position));
                    reached=body.position.z>=road[Phase5Setup.ExitIndex+25].z;
                    if(!reached&&Time.time-start<25&&EditorApplication.timeSinceStartup-wall<90)
                    {
                        int ahead=Mathf.Max(3,Mathf.RoundToInt(Mathf.Abs(car.ForwardSpeed)*.28f));var local=car.transform.InverseTransformPoint(cut[(index+ahead)%cut.Count]);
                        float curvature=2*local.x/Mathf.Max(1,local.x*local.x+local.z*local.z);float angle=Mathf.Lerp(car.slowSteerAngle,car.fastSteerAngle,Mathf.Clamp01(Mathf.Abs(car.ForwardSpeed)/car.topSpeed));
                        float steer=Mathf.Clamp(Mathf.Atan(curvature*car.wheelbase)*Mathf.Rad2Deg/angle,-1,1);float speed=car.ForwardSpeed;
                        // Invert the accepted axisDeadzone processor to request the same
                        // effective steering as the motor-command follower, not weaker input.
                        float stick=Mathf.Abs(steer)<.00001f?0:Mathf.Sign(steer)*(.12f+.83f*Mathf.Abs(steer));
                        UnityEngine.InputSystem.InputSystem.QueueStateEvent(pad,new UnityEngine.InputSystem.LowLevel.GamepadState{leftStick=new Vector2(stick,0),rightTrigger=Mathf.Clamp01((24-speed)*.7f+.35f),leftTrigger=speed>24.3f?Mathf.Clamp01((speed-24)*.5f):0});
                        return;
                    }
                }
                catch(Exception e){failure=e.ToString();}
                finally
                {
                    if(reached||failure!=null||Time.time-start>=25||EditorApplication.timeSinceStartup-wall>=90)
                    {
                        EditorApplication.update-=tick;
                        File.WriteAllText("Docs/PHASE5_REALTIME.txt",$"{(reached&&error<3.8f&&minUp>.8f?"PASS":"FAIL")} ordinary-frame virtual Gamepad: reached={reached}; initial=24m/s; elapsed game={Time.time-start:F3}s; wall={EditorApplication.timeSinceStartup-wall:F3}s; editor updates={frames}; peak={peak:F2}m/s; max center error={error:F2}m; upright={minUp:F3}; max camera distance={cameraDistance:F2}m; HUD={Object.FindAnyObjectByType<RaceHud>().display.text}; exception={failure}\nUses Input System virtual Gamepad with normal Update/FixedUpdate, not manual PhysX stepping. Initial velocity injected. No physical controller.\n");
                        UnityEngine.InputSystem.InputSystem.RemoveDevice(pad);settings.editorInputBehaviorInPlayMode=focus;settings.backgroundBehavior=background;Application.runInBackground=run;race.RestartRace();
                    }
                }
            };
            EditorApplication.update+=tick;
        }
    }
}
