using System;
using System.IO;
using UnityEngine;

namespace Racer
{
    // Opt-in observational trace for real FixedUpdate driving, shared player/AI state.
    public sealed class CorrectionTrace : MonoBehaviour
    {
        StreamWriter log,traffic;RaceDirector race;float next;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"-raceTrace");
            if(i<0||i+1>=args.Length)return;
            var t=new GameObject("Opt-in race state trace").AddComponent<CorrectionTrace>();
            string path=Path.GetFullPath(args[i+1]);Directory.CreateDirectory(Path.GetDirectoryName(path));t.log=new StreamWriter(path){AutoFlush=true};
            t.log.WriteLine("time,racer,x,y,z,speed,grounded,branch,position,earned,rejoinSeconds,road,nextGate,misses,penalty,buzzes");
            t.traffic=new StreamWriter(path+".traffic.csv"){AutoFlush=true};t.traffic.WriteLine("time,name,highwayPool,direction,station,lateral,speed,target,brake,recoveries,grounded,recycles,minRecycleDistance");
        }
        void LateUpdate()
        {
            if(!race)race=FindAnyObjectByType<RaceDirector>();if(!race||log==null||Time.unscaledTime<next)return;next=Time.unscaledTime+.1f;
            foreach(var r in race.Racers){var p=r.Car.Body.position;log.WriteLine($"{race.Clock:F3},{r.Name},{p.x:F3},{p.y:F3},{p.z:F3},{r.Car.Body.linearVelocity.magnitude:F3},{r.Car.GroundedWheels},{r.Branch.Route?.title??"none"},{r.Branch.Position:F3},{r.Branch.Earned:F3},{r.Branch.RejoinSeconds:F3},{r.RoadPosition:F3},{r.Progress.NextGate},{r.Progress.MissedGates},{r.Progress.PenaltySeconds:F3},{race.Flow.CheckpointBuzzes}");}
            foreach(var d in race.Drivers)if(d&&d.Racer==null){float s=race.road.Project(d.Car.Body.position,out _);var p=race.road.At(s,out var f);float side=Vector3.Dot(d.Car.Body.position-p,Vector3.Cross(Vector3.up,f).normalized);traffic.WriteLine($"{race.Clock:F3},{d.name},{d.HighwayTraffic},{d.Direction},{s:F3},{side:F3},{d.Car.ForwardSpeed:F3},{d.TargetSpeed:F3},{d.LastBrake:F3},{d.RecoveryCount},{d.Car.GroundedWheels},{d.HighwayRecycles},{d.MinimumRecyclePlayerDistance:F1}");}
        }
        void OnDestroy(){log?.Dispose();traffic?.Dispose();}
    }
}
