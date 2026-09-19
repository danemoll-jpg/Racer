using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Racer
{
    // Explicit opt-in fixtures and ordinary FixedUpdate driving. Uses isolated saves only.
    public sealed class WoodlandValidation : MonoBehaviour
    {
        public static string Label="editor";
        public static bool Drive=true;
        public static bool ExcursionOnly;
        public static string ProfileFilter="", RouteFilter="";
        RaceDirector race;
        Gamepad pad;
        readonly List<string> checks=new();
        string Dir=>"Docs/CR034-039/"+Label;
        void Check(bool pass,string name){checks.Add((pass?"PASS ":"FAIL ")+name);File.WriteAllLines(Dir+"/rules.txt",checks);}
        public static void Launch()=>new GameObject("Woodland integrated validation").AddComponent<WoodlandValidation>();
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            var args=Environment.GetCommandLineArgs(); if(Array.IndexOf(args,"-woodlandTest")<0 || Array.IndexOf(args,"-racerTestSave")<0)return;
            Label="standalone"; int i=Array.IndexOf(args,"-woodlandLabel"); if(i>=0&&i+1<args.Length)Label=args[i+1];
            Drive=Array.IndexOf(args,"-woodlandRulesOnly")<0;
            ExcursionOnly=Array.IndexOf(args,"-woodlandExcursionOnly")>=0;
            i=Array.IndexOf(args,"-woodlandProfile"); if(i>=0&&i+1<args.Length)ProfileFilter=args[i+1];
            i=Array.IndexOf(args,"-woodlandRoute"); if(i>=0&&i+1<args.Length)RouteFilter=args[i+1];
            Application.runInBackground=true; Launch();
        }
        IEnumerator Start()
        {
            yield return null; Directory.CreateDirectory(Dir); race=FindAnyObjectByType<RaceDirector>(); Application.runInBackground=true;
#if UNITY_EDITOR
            race.Flow.UseValidationSave(Path.GetFullPath("Temp/woodland-save-"+Guid.NewGuid().ToString("N")));
            InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
#endif
            InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
            pad=InputSystem.AddDevice<Gamepad>();
            race.opponents=race.traffic=false; race.Flow.StartRace(); while(race.Flow.State!=RaceFlow.Stage.Racing)yield return null;
            race.vehicle.enabled=false; race.vehicle.Body.isKinematic=true;
            foreach(var branch in race.Branches) Rules(branch);
            foreach(var profile in VehicleProfile.All)
            {
                race.vehicle.GetComponent<VehicleConfiguration>().Apply(profile.Id); race.vehicle.GetComponent<VehicleConfiguration>().SetBodyColor(6); yield return null;
                var renderers=race.vehicle.GetComponentsInChildren<Renderer>();var block=new MaterialPropertyBlock();
                Check(renderers.Where(r=>VehiclePaint.IsBodyPaint(r.sharedMaterial)).All(r=>{r.GetPropertyBlock(block);return block.GetColor("_BaseColor")==VehiclePaint.Colors[6];}),"Black body "+profile.Id);
                Check(renderers.Where(r=>!VehiclePaint.IsBodyPaint(r.sharedMaterial)).All(r=>{r.GetPropertyBlock(block);return block.isEmpty;}),"Unpainted trim/glass/rider "+profile.Id);
            }
            Check(race.Category.StartsWith("street-v8-landings"),"Versioned course records");
            Check(VehiclePaint.Names[6]=="Black","Stable appended black swatch index");
            foreach(var branch in race.Branches.Where(b=>b.title!="Existing Southwest Cut"))
                Check(branch.bypassedGates.Length>=2,branch.title+" explicitly bypasses multiple gates");
            for(float s=3800;s<4540;s+=100)
                foreach(int dir in new[]{-1,1})
                {
                    float lane=race.road.TrafficLane(s,dir); Check(Mathf.Abs(lane)>5&&Mathf.Abs(lane)<race.road.HalfWidth(s),"Highway outer lane supported envelope "+s+" "+dir);
                }
            race.vehicle.Body.isKinematic=false; race.vehicle.enabled=true;
            if(Drive)
            {
                File.WriteAllText(Dir+"/driving.csv","profile,route,attempt,road,finished,seconds,entry_mps,exit_mps,max_lateral,min_up,air_seconds,misses,penalty,branch_exits,recoveries\n");
                foreach(var profile in VehicleProfile.All.Where(p=>ProfileFilter==""||p.Id==ProfileFilter))
                {
                    race.Flow.Pause();race.Flow.QuitRace();race.Flow.OpenGarage();race.Flow.SelectVehicle(profile.Id);race.Flow.CloseGarage();race.opponents=race.traffic=false;race.Flow.StartRace();
                    while(race.Flow.State!=RaceFlow.Stage.Racing)yield return null;
                    foreach(var branch in race.Branches.Where(b=>RouteFilter==""||b.title==RouteFilter))
                    {
                        if(!ExcursionOnly){for(int repeat=0;repeat<2;repeat++) yield return Attempt(branch,repeat,false);yield return Attempt(branch,2,false);}
                        yield return Attempt(branch,3,false);
                        if(!ExcursionOnly)for(int repeat=0;repeat<2;repeat++) yield return Attempt(branch,repeat,true);
                    }
                }
            }
            InputSystem.QueueStateEvent(pad,new GamepadState());InputSystem.RemoveDevice(pad);pad=null;
            race.Flow.Pause();File.WriteAllText(Dir+"/done.txt",$"Complete; {checks.Count(x=>x.StartsWith("PASS"))}/{checks.Count} invariant checks. Driving outcomes in CSV; these are not human fun/difficulty acceptance.");
            if(!Application.isEditor) Application.Quit();
        }
        void Seed(WoodlandRoute branch)
        {
            var p=race.Progress;p.Restart();p.Cross(0,true,0);
            int first=Array.FindIndex(race.gates,g=>race.road.Project(g.transform.position,out _)>branch.entryRoad);
            for(int i=1;i<first;i++)p.Cross(i,true,i);
            var r=race.Racers[0];r.Branch.Clear();r.Travel=0;r.VerifiedRoad=r.RoadPosition=branch.entryRoad-race.Origin;r.FinishArmed=false;
        }
        void Rules(WoodlandRoute branch)
        {
            var tracker=new BranchProgress();tracker.Begin(branch);var previous=branch.points[0];bool exited=false;
            for(float s=1;s<branch.Length;s+=1) { var p=branch.At(s,out var f);exited|=tracker.Advance(previous,p,f);previous=p; }
            var end=branch.At(branch.Length,out var direction);exited|=tracker.Advance(previous,end,direction);
            Check(exited,branch.title+" contiguous clean exit earns evidence");
            tracker.Begin(branch);Check(!tracker.Advance(branch.points[0],end,direction)&&tracker.Earned==0,branch.title+" entrance-to-exit teleport rejected");
            tracker.Begin(branch);var at=branch.At(20,out var forward);Check(!tracker.Advance(branch.points[0],at,forward),branch.title+" unobserved progress rejected");
            tracker.Begin(branch);previous=branch.points[0];
            for(float s=1;s<=30;s++) { var p=branch.At(s,out var f);tracker.Advance(previous,p,f);previous=p; }
            float earned=tracker.Earned;
            for(float s=29;s>=10;s--) { var p=branch.At(s,out var f);tracker.Advance(previous,p,-f);previous=p; }
            Check(tracker.Position<12&&Mathf.Abs(tracker.Earned-earned)<.1f,branch.title+" reversing lowers standing without new credit");
            tracker.Recovered(branch.At(8,out _));Check(tracker.Position<9,branch.title+" backward local recovery");
            tracker.Recovered(end);Check(tracker.Position<9,branch.title+" forward recovery rejected");
            float beforeExcursion=tracker.Earned;var outside=branch.At(branch.Length*.65f,out var outsideForward)+Vector3.Cross(Vector3.up,outsideForward).normalized*30;
            Check(!tracker.Advance(previous,outside,outsideForward)&&tracker.Earned==beforeExcursion,branch.title+" off-corridor excursion earns no progress");
            tracker.Recovered(branch.At(8,out _));previous=branch.At(8,out _);exited=false;
            for(float s=9;s<branch.Length;s++){var point=branch.At(s,out var f);exited|=tracker.Advance(previous,point,f);previous=point;}
            exited|=tracker.Advance(previous,end,direction);Check(exited,branch.title+" failed excursion followed by backward recovery can complete");
            Seed(branch);var r=race.Racers[0];var start=branch.At(0,out var sf)-sf*2+Vector3.up*.7f;double now=10;
            race.ResetSampling(start,now);r.Previous=start;
            for(float s=0;s<branch.Length;s+=1) { var p=branch.At(s,out var f)+Vector3.up*.7f; race.Sample(p,f,now+=.04); }
            race.Sample(end+Vector3.up*.7f,direction,now+=.04);
            int expected=branch.bypassedGates.Length>0?branch.bypassedGates[^1]+1:race.Progress.NextGate;
            Check(race.Progress.PenaltySeconds==0&&race.Progress.MissedGates==0&&r.Branch.Route==null&&race.Progress.NextGate==expected,branch.title+" integrated legal bypass no penalties / correct next gate");
            int laps=race.Progress.CompletedLaps;race.Progress.Cross(0,true,now+1);Check(race.Progress.CompletedLaps==laps,branch.title+" early finish still refused");
            Seed(branch);race.ResetSampling(start,10);race.Sample(branch.points[0]+Vector3.up*.7f,sf,10.1);
            var exit=end+Vector3.up*.7f;race.Sample(exit,direction,10.2);Check(!race.Progress.LapActive,branch.title+" teleport cannot finish lap");
            r.Branch.Clear();
            // A complete forward lap with this one legal branch must still finish normally.
            race.Progress.Restart();race.Progress.Cross(0,true,0);r.FinishArmed=false;r.Travel=0;r.VerifiedRoad=0;
            var lapStart=race.road.At(race.Origin+2,out _)+Vector3.up*.7f;race.ResetSampling(lapStart,1);double lapClock=1;
            for(float s=race.Origin+3;s<branch.entryRoad;s+=1){var point=race.road.At(s,out var f)+Vector3.up*.7f;race.Sample(point,f,lapClock+=.04);}
            for(float s=0;s<branch.Length;s+=1){var point=branch.At(s,out var f)+Vector3.up*.7f;race.Sample(point,f,lapClock+=.04);}
            race.Sample(end+Vector3.up*.7f,direction,lapClock+=.04);
            for(float s=branch.exitRoad+1;s<race.road.Length+race.Origin+3;s+=1){var point=race.road.At(s,out var f)+Vector3.up*.7f;race.Sample(point,f,lapClock+=.04);}
            Check(race.Progress.CompletedLaps==1&&race.Progress.MissedGates==0&&race.Progress.PenaltySeconds==0,branch.title+" complete forward lap after legal bypass");
            r.Branch.Clear();
        }
        IEnumerator Attempt(WoodlandRoute branch,int attempt,bool mainRoad)
        {
            var car=race.vehicle;var body=car.Body;Seed(branch);var r=race.Racers[0];
            float startS=branch.entryRoad-12;var p=race.road.At(startS,out var f)+Vector3.up*.7f;
            body.position=p;body.rotation=Quaternion.LookRotation(Vector3.ProjectOnPlane(f,Vector3.up));car.transform.SetPositionAndRotation(p,body.rotation);
            body.linearVelocity=f*branch.recommendedSpeed;body.angularVelocity=Vector3.zero;car.ClearSteering();Physics.SyncTransforms();race.ResetSampling(p,Time.timeAsDouble);FindAnyObjectByType<ChaseCamera>().Snap();
            BreakableProp.RestoreRace();
            int audioBefore=FindAnyObjectByType<SmashAudio>()?.Events??0, buzzBefore=race.Flow.CheckpointBuzzes;
            float begin=Time.time,entry=0,air=0,minUp=1,maxLat=0;bool finished=false,failed=false;int recoveries=0,exits=r.Branch.Exits;float nextLog=0,stalled=0;
            using var trace=new StreamWriter(Dir+"/"+car.GetComponent<VehicleConfiguration>().profileId+"-"+branch.title.Replace(' ','-')+"-"+(mainRoad?"road":"branch")+"-"+attempt+".csv");
            trace.WriteLine("time,x,y,z,speed,target,grounded,branchS,earned,misses");
            while(Time.time-begin<55)
            {
                float roadS=race.road.Project(body.position,out _);float s=branch.Project(body.position,out float lateral);var current=branch.At(s,out var tangent);
                bool onBranch=!mainRoad && r.Branch.Exits==exits && (r.Branch.Route==branch || roadS>=branch.entryRoad-6&&roadS<branch.exitRoad);
                float look=Mathf.Clamp(6+Mathf.Abs(car.ForwardSpeed)*.4f,8,23);
                var target=onBranch?branch.At(s+look,out _):race.road.At(roadS+look,out _);
                // A repeat uses imperfect entrance/shoulder lines, then returns to the house aperture.
                if(onBranch && attempt==1 && s<100) target+=Vector3.Cross(Vector3.up,tangent).normalized*(2.5f*Mathf.Sin(s*.045f));
                var local=car.transform.InverseTransformPoint(target);float curvature=2*local.x/Mathf.Max(1,local.x*local.x+local.z*local.z);
                float angle=Mathf.Lerp(car.slowSteerAngle,car.fastSteerAngle,Mathf.Clamp01(Mathf.Abs(car.ForwardSpeed)/car.topSpeed));
                float steer=Mathf.Clamp(Mathf.Atan(curvature*car.wheelbase)*Mathf.Rad2Deg/angle,-1,1);
                float desired=mainRoad?car.topSpeed*.95f:branch.recommendedSpeed+(attempt==1?2:0);
                for(float d=0;d<90;d+=6)
                {
                    Vector3 a,b;
                    if(onBranch) { branch.At(s+d,out a);branch.At(s+d+8,out b); }else {race.road.At(roadS+d,out a);race.road.At(roadS+d+8,out b);}
                    float curve=Vector3.Angle(Vector3.ProjectOnPlane(a,Vector3.up),Vector3.ProjectOnPlane(b,Vector3.up))*Mathf.Deg2Rad/8;
                    float cap=Mathf.Sqrt(car.maxGripAcceleration*.7f/Mathf.Max(.0001f,curve));
                    if(mainRoad && Mathf.Abs(a.y)>.14f)cap=Mathf.Min(cap,29);
                    float crest=Mathf.Max(0,Mathf.Asin(a.y)-Mathf.Asin(b.y))/8;
                    // Keep the authored stunt airborne, but brake for natural entry/rejoin crests.
                    bool intentionalJump=!mainRoad && (branch.title=="Fox Gully" ? s+d>=210 && s+d<=335 : s+d>=110 && s+d<=255);
                    if(!intentionalJump && crest>.001f) cap=Mathf.Min(cap,Mathf.Sqrt(6.5f/crest));
                    desired=Mathf.Min(desired,Mathf.Sqrt(cap*cap+2*car.braking*.75f*Mathf.Max(0,d-10)));
                }
                if(!mainRoad && attempt==2 && !failed && s>branch.Length*(branch.title=="Fox Gully"?.55f:.32f))
                { failed=true; InputSystem.QueueStateEvent(pad,new GamepadState{leftTrigger=1});yield return new WaitForSeconds(1.2f);bool ok=car.GetComponent<VehicleRespawn>().TryRecoverLocal();if(ok)recoveries++; }
                if(!mainRoad && attempt==3 && !failed && s>branch.Length*.28f)
                {
                    failed=true;
                    // Real pedal/stick excursion, then braking through zero into reverse.
                    // No pose/velocity edits: an imperfect line must retain its earned gates.
                    InputSystem.QueueStateEvent(pad,new GamepadState{rightTrigger=.6f,leftStick=new(.9f,0)});
                    yield return new WaitForSeconds(.65f);
                    InputSystem.QueueStateEvent(pad,new GamepadState{leftTrigger=1});
                    yield return new WaitForSeconds(2.1f);
                    float reverseDeadline=Time.time+3;
                    while(car.ForwardSpeed>-.8f&&Time.time<reverseDeadline)yield return new WaitForFixedUpdate();
                    branch.Project(body.position,out float departureLateral);
                    File.AppendAllText(Dir+"/excursions.txt",$"{car.GetComponent<VehicleConfiguration>().profileId} {branch.title}: speed before recovery={car.ForwardSpeed:F3}m/s; lateral={departureLateral:F3}m; grounded={car.GroundedWheels}; up={car.transform.up.y:F3}; misses={race.Progress.MissedGates}; seconds={race.Progress.PenaltySeconds}\n");
                    bool ok=car.GetComponent<VehicleRespawn>().TryRecoverLocal();if(ok)recoveries++;
                }
                InputSystem.QueueStateEvent(pad,new GamepadState{rightTrigger=Mathf.Clamp01((desired-car.ForwardSpeed)*.6f),leftTrigger=car.ForwardSpeed>desired+1?Mathf.Clamp01((car.ForwardSpeed-desired)*.3f):0,leftStick=new(Mathf.Abs(steer)<.001f?0:Mathf.Sign(steer)*(.12f+Mathf.Abs(steer)*.83f),0)});
                if(entry==0 && roadS>=branch.entryRoad) entry=car.ForwardSpeed;
                stalled=car.ForwardSpeed<2?stalled+Time.deltaTime:0; if(stalled>6)break;
                minUp=Mathf.Min(minUp,car.transform.up.y);if(onBranch)maxLat=Mathf.Max(maxLat,lateral);if(car.GroundedWheels<2)air+=Time.deltaTime;
                if(Time.time>=nextLog){nextLog=Time.time+.1f;trace.WriteLine($"{Time.time-begin:F3},{body.position.x:F3},{body.position.y:F3},{body.position.z:F3},{car.ForwardSpeed:F3},{desired:F3},{car.GroundedWheels},{r.Branch.Position:F3},{r.Branch.Earned:F3},{race.Progress.MissedGates}");}
                float nextStation=race.gates.Select(g=>race.road.Project(g.transform.position,out _)).Where(st=>st>branch.exitRoad+1).DefaultIfEmpty(branch.exitRoad+180).Min();
                if(roadS>nextStation+30 && roadS<nextStation+90 && (mainRoad || r.Branch.Exits>exits)) {finished=true;break;}
                if(attempt==0 && Time.time-begin>3 && Time.time-begin<3.1f) ScreenCapture.CaptureScreenshot(Path.GetFullPath(Dir+"/drive-"+branch.title.Replace(' ','-')+"-"+car.GetComponent<VehicleConfiguration>().profileId+".png"));
                yield return null;
            }
            File.AppendAllText(Dir+"/driving.csv",$"{car.GetComponent<VehicleConfiguration>().profileId},{branch.title},{attempt},{mainRoad},{finished},{Time.time-begin:F3},{entry:F3},{car.ForwardSpeed:F3},{maxLat:F3},{minUp:F3},{air:F3},{race.Progress.MissedGates},{race.Progress.PenaltySeconds:F3},{r.Branch.Exits-exits},{recoveries}\n");
            var glass=FindObjectsByType<BreakableProp>().Where(prop=>prop.surface==SmashAudio.Surface.Glass).ToArray();
            File.AppendAllText(Dir+"/physical-objects.txt",$"{car.GetComponent<VehicleConfiguration>().profileId} {branch.title} attempt={attempt} road={mainRoad} glassBroken={glass.Count(prop=>prop.IsBroken)}/{glass.Length} smashEvents={(FindAnyObjectByType<SmashAudio>()?.Events??0)-audioBefore} buzzes={race.Flow.CheckpointBuzzes-buzzBefore}\n");
            InputSystem.QueueStateEvent(pad,new GamepadState()); yield return new WaitForSeconds(.2f);
        }
        void OnDestroy(){if(pad!=null&&pad.added)InputSystem.RemoveDevice(pad);}
    }
}
