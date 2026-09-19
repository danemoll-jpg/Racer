using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Racer
{
    // Explicit opt-in release diagnostics. Never activated in normal play or against a user save.
    public sealed class CorrectionValidation : MonoBehaviour
    {
        const string Dir="Docs/CR041-045/rules";
        readonly List<string> checks=new();
        RaceDirector race;
        double clock;
        void Check(bool pass,string name) { checks.Add((pass?"PASS ":"FAIL ")+name); File.WriteAllLines(Dir+"/checks.txt",checks); }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            var args=Environment.GetCommandLineArgs();
            if(args.Contains("-correctionRules") && args.Contains("-racerTestSave"))
            { Application.runInBackground=true; new GameObject("Correction rules and mixed audio capture").AddComponent<CorrectionValidation>(); }
        }
        public static void Launch() { Application.runInBackground=true; new GameObject("Correction rules and mixed audio capture").AddComponent<CorrectionValidation>(); }
        void Seed(int next)
        {
            race.Progress.Restart(); race.Progress.Cross(0,true,0);
            for(int i=1;i<next;i++) race.Progress.Cross(i,true,i);
            race.Racers[0].Branch.Clear(); race.Racers[0].FinishArmed=false;
            clock=100;
        }
        void Sample(Vector3 p,Vector3 f) { race.Sample(p,f,clock+=.02); }
        IEnumerator Start()
        {
            yield return null; Directory.CreateDirectory(Dir); Application.runInBackground=true;
            race=FindAnyObjectByType<RaceDirector>();
#if UNITY_EDITOR
            race.Flow.UseValidationSave(Path.GetFullPath("Temp/correction-rules-save"));
#endif
            race.opponents=race.traffic=false; race.Flow.StartRace();
            while(race.Flow.State!=RaceFlow.Stage.Racing)yield return null;
            race.vehicle.enabled=false; race.vehicle.Body.isKinematic=true;
            foreach(var profile in VehicleProfile.All)
            {
                race.vehicle.GetComponent<VehicleConfiguration>().Apply(profile.Id);
                foreach(var branch in race.Branches)
                foreach(float shoulder in new[]{0f,branch.halfWidth+2})
                foreach(bool backward in new[]{false,true})
                {
                    int first=Array.FindIndex(race.gates,g=>race.road.Project(g.transform.position,out _)>branch.entryRoad);
                    Seed(first); int buzz=race.Flow.CheckpointBuzzes;
                    Vector3 Point(float s,out Vector3 f) { var p=branch.At(s,out f); var side=Vector3.Cross(Vector3.up,f).normalized; return p+side*shoulder+Vector3.up*(backward?8:.7f); }
                    var previous=Point(0,out var forward)-forward*2; race.ResetSampling(previous,clock);
                    for(float s=0;s<branch.Length;s+=.6f) { var p=Point(s,out forward);Sample(p,backward?-forward:forward); }
                    Sample(Point(branch.Length,out forward),backward?-forward:forward);
                    Check(race.Racers[0].Branch.Route==null && race.Progress.MissedGates==0 && race.Progress.PenaltySeconds==0 && race.Flow.CheckpointBuzzes==buzz,
                        $"{profile.Id} {branch.title} swept shoulder={shoulder} backward/air={backward}: zero charge/buzz verified exit");
                }
            }
            foreach(int index in new[]{1,12})
            {
                var gate=race.gates[index]; Seed(index);
                var p=gate.transform.position; var f=gate.transform.forward;
                race.ResetSampling(p-f,clock); Sample(p+f,-f);
                Check(race.Progress.NextGate==index+1,"Backwards body forward passage CP"+index);
                Sample(p-f,f); Sample(p+f,f);
                Check(race.Progress.NextGate==index+1,"Duplicate passage CP"+index);
                Seed(index);race.ResetSampling(p+f,clock);Sample(p-f,f);
                Check(race.Progress.NextGate==index,"Wrong travel rejected CP"+index);
                Seed(index);race.ResetSampling(p-f*20,clock);Sample(p+f*20,f);
                Check(!race.Progress.LapActive,"Teleport rejected CP"+index);
            }
            foreach(var branch in race.Branches)
            {
                int first=Array.FindIndex(race.gates,g=>race.road.Project(g.transform.position,out _)>branch.entryRoad);Seed(first);
                var start=branch.At(0,out var f)+Vector3.up*.7f-f*2;race.ResetSampling(start,clock);
                for(float s=0;s<branch.Length*.55f;s+=.6f)Sample(branch.At(s,out f)+Vector3.up*.7f,f);
                int earnedNext=race.Progress.NextGate;
                for(float s=branch.Length*.55f-.6f;s>2;s-=.6f)Sample(branch.At(s,out f)+Vector3.up*.7f,-f);
                Check(race.Racers[0].Branch.Route==branch && race.Progress.NextGate==earnedNext && race.Progress.PenaltySeconds==0,branch.title+" reversing retains witnessed gates and branch");
                for(float s=2;s<=branch.Length;s+=.6f)Sample(branch.At(s,out f)+Vector3.up*.7f,f);
                Sample(branch.At(branch.Length,out f)+Vector3.up*.7f,f);
                Check(race.Progress.PenaltySeconds==0 && race.Racers[0].Branch.Route==null,branch.title+" reverse then full re-traversal exits without a penalty chain");
                Seed(first);race.ResetSampling(start,clock);
                for(float s=0;s<branch.Length*.55f;s+=.6f)Sample(branch.At(s,out f)+Vector3.up*.7f,f);
                int retained=race.Progress.NextGate;
                float rejoin=Mathf.Lerp(branch.entryRoad,branch.exitRoad,.55f);
                // Isolate the rejoin decision from approach physics; the physical matrix
                // separately covers ordinary-frame entrances, flight and recovery.
                race.ResetSampling(race.road.At(rejoin,out f)+Vector3.up*.7f,clock);
                for(int i=1;i<=320;i++)Sample(race.road.At(rejoin+i*.6f,out f)+Vector3.up*.7f,f);
                Check(race.Racers[0].Branch.Route==null && race.Progress.NextGate>=retained &&
                    race.Progress.PenaltySeconds==race.Progress.MissedGates*5,
                    branch.title+" sustained partial road rejoin keeps earned credit; remaining misses flat five");

                var ai=new RacerState("AI branch fixture",race.vehicle,race.gates.Length-1,1);
                ai.Progress.Cross(0,true,0);for(int i=1;i<first;i++)ai.Progress.Cross(i,true,i);
                ai.Previous=start;ai.PreviousTime=clock;
                var sampleAI=typeof(RaceDirector).GetMethod("SampleRacer",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance);
                for(float s=0;s<=branch.Length;s+=.6f){var point=branch.At(s,out f)+Vector3.up*.7f;sampleAI.Invoke(race,new object[]{ai,point,-f,clock+=.02,false});}
                var end=branch.At(branch.Length,out f)+Vector3.up*.7f;sampleAI.Invoke(race,new object[]{ai,end,-f,clock+=.02,false});
                Check(ai.Branch.Route==null && ai.Branch.Exits>0 && ai.Progress.PenaltySeconds==0 && ai.Progress.NextGate>=first,
                    branch.title+" AI shares backwards-body forward branch credit");
            }
            {
                int index=12;var gate=race.gates[index];var f=gate.transform.forward;
                Seed(index);var p=gate.transform.position+Vector3.up*16;race.ResetSampling(p-f,clock);Sample(p+f,-f);
                Check(race.Progress.NextGate==13,"Jamerson 16m airborne backwards-facing forward crossing");
                Seed(1);var ordinary=race.gates[1];p=ordinary.transform.position+Vector3.up*16;f=ordinary.transform.forward;
                race.ResetSampling(p-f,clock);Sample(p+f,f);Check(race.Progress.NextGate==1,"Other gate does not have unlimited jump volume");
                Seed(1);gate=race.gates[4];f=gate.transform.forward;p=gate.transform.position;
                race.ResetSampling(p-f,clock);Sample(p+f,f);
                Check(race.Progress.MissedGates==3 && race.Progress.PenaltySeconds==15,"Three genuine misses exactly fifteen seconds");
                Seed(1);gate=race.gates[0];p=gate.transform.position;f=gate.transform.forward;race.ResetSampling(p-f,clock);Sample(p+f,f);
                Check(race.Progress.CompletedLaps==0,"Unarmed finish rejected");
            }
            Check(race.Category.StartsWith("street-v8-landings-"),"New rules category preserves older records");
            {
                var branch=race.Branches.First(b=>b.title=="Existing Southwest Cut");
                int expected=Array.FindIndex(race.gates,g=>race.road.Project(g.transform.position,out _)>branch.exitRoad);
                Seed(expected);race.Racers[0].Branch.Begin(branch);var gate=race.gates[expected];var f=gate.transform.forward;
                race.ResetSampling(gate.transform.position-f,clock);Sample(gate.transform.position+f,-f);
                Check(race.Progress.NextGate==expected+1 && race.Progress.PenaltySeconds==0,
                    "Retained partial-rejoin context cannot hide a physically crossed unrelated expected gate");
            }
            string migration=Path.Combine(race.Flow.Save.DirectoryPath,"migration-fixture");Directory.CreateDirectory(migration);
            string oldCategory="street-v5-woodland-original-solo-1",oldFile=Path.Combine(migration,"records-"+oldCategory+".json");
            string oldJson=JsonUtility.ToJson(new RacerSave.Records{course=oldCategory,lap=123.45,race=380.25});File.WriteAllText(oldFile,oldJson);
            var save=new RacerSave(migration,"street-loop-gates-v1-laps3");save.SelectRecords(oldCategory);
            Check(save.Best.lap==123.45 && save.Best.race==380.25,"Historical record remains readable");
            save.Settings.master=.27f;save.Settings.vehicle=.43f;save.SaveSettings();save.SelectRecords(race.Category);
            Check(save.Best.lap==0 && save.Best.race==0,"Flat-five category does not compete against old rules");
            save.RecordLap(132.789);save.RecordRace(400.123);
            Check(File.ReadAllText(oldFile)==oldJson,"Writing new records leaves historical bytes intact");
            var reloaded=new RacerSave(migration,"street-loop-gates-v1-laps3");reloaded.SelectRecords(race.Category);
            Check(reloaded.Best.lap==132.789 && reloaded.Best.race==400.123,"Fractional adjusted totals persist in new category");
            Check(reloaded.Settings.master==.27f && reloaded.Settings.vehicle==.43f,"Record migration preserves saved volume values");
            // Actual dynamic-body / ordinary FixedUpdate crossings complement swept fixtures.
            // Initial poses/velocities are test setup; no manual Sample calls during travel.
            foreach(var profile in VehicleProfile.All)
            foreach(int scenario in new[]{0,1,2})
            {
                race.vehicle.GetComponent<VehicleConfiguration>().Apply(profile.Id);
                int gateIndex=scenario==1?12:1;Seed(gateIndex);
                var gate=race.gates[gateIndex];var forward=gate.transform.forward;
                var position=gate.transform.position+forward*(scenario==2?8:-8)+Vector3.up*(scenario==1?16:2);
                var body=race.vehicle.Body;body.isKinematic=false;body.position=position;body.rotation=Quaternion.LookRotation(-forward);
                race.vehicle.transform.SetPositionAndRotation(position,body.rotation);body.linearVelocity=forward*(scenario==2?-18:18);body.angularVelocity=scenario==1?Vector3.up*8:Vector3.zero;
                Physics.SyncTransforms();race.ResetSampling(position,Time.timeAsDouble);int buzz=race.Flow.CheckpointBuzzes;
                yield return new WaitForSeconds(.8f);
                Check(race.Progress.NextGate==(scenario==2?gateIndex:gateIndex+1) && race.Progress.MissedGates==0 && race.Flow.CheckpointBuzzes==buzz,
                    $"{profile.Id} physical FixedUpdate {(scenario==0?"backwards-body forward":scenario==1?"high spinning forward":"genuine reverse")} crossing");
                body.isKinematic=true;yield return null;
            }
            File.WriteAllText(Dir+"/done.txt",$"{checks.Count(x=>x.StartsWith("PASS"))}/{checks.Count} explicit swept fixtures; not ordinary-frame driving proof.");
            race.Flow.Pause(); if(!Application.isEditor)Application.Quit();
        }
    }
}
