using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Racer
{
    public sealed class WoodlandSystemsValidation : MonoBehaviour
    {
        const string Dir="Docs/CR034-039/systems";
        readonly List<string> checks=new();
        void Check(bool pass,string name){checks.Add((pass?"PASS ":"FAIL ")+name);File.WriteAllLines(Dir+"/checks.txt",checks);}
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            var args=Environment.GetCommandLineArgs();
            if(Array.IndexOf(args,"-woodlandSystems")>=0&&Array.IndexOf(args,"-racerTestSave")>=0)
            { Application.runInBackground=true; new GameObject("Woodland highway and smash fixtures").AddComponent<WoodlandSystemsValidation>(); }
        }
        IEnumerator Start()
        {
            yield return null; Directory.CreateDirectory(Dir); Application.runInBackground=true;
            var race=FindAnyObjectByType<RaceDirector>(); race.opponents=false;race.traffic=true;race.trafficCount=4;race.Flow.StartRace();
            while(race.Flow.State!=RaceFlow.Stage.Racing)yield return null;
            race.vehicle.Body.isKinematic=true;race.vehicle.enabled=false;
            var synth=typeof(SmashAudio).GetMethod("Synthesize",BindingFlags.Static|BindingFlags.NonPublic);
            foreach(SmashAudio.Surface material in Enum.GetValues(typeof(SmashAudio.Surface)))
            {
                var hashes=new HashSet<float>();
                for(int v=0;v<3;v++)
                {
                    var clip=(AudioClip)synth.Invoke(null,new object[]{material,v});var pcm=new float[clip.samples];clip.GetData(pcm,0);
                    Check(pcm.All(x=>!float.IsNaN(x)&&Mathf.Abs(x)<=.851f),material+" PCM finite/headroom variant "+v);
                    hashes.Add(pcm[111]);Destroy(clip);
                }
                Check(hashes.Count==3,material+" distinct variants");
            }
            var at=race.vehicle.transform.position;
            SmashAudio.Play(at,30,SmashAudio.Surface.Wood);var audio=FindAnyObjectByType<SmashAudio>();int emitted=audio.Events;
            for(int i=0;i<100;i++)SmashAudio.Play(at,30,SmashAudio.Surface.Wood);
            Check(audio.Events==emitted,"Burst cooldown rejects 100 duplicate onsets");
            for(int i=0;i<8;i++){yield return new WaitForSeconds(.08f);SmashAudio.Play(at,20,(SmashAudio.Surface)(i%4));}
            var voices=audio.GetComponentsInChildren<AudioSource>();Check(voices.Length==4,"Exactly four shared voices after repeated impacts");
            float volume=race.Flow.Save.Settings.vehicle;race.Flow.Save.Settings.vehicle=0;yield return null;
            Check(voices.All(v=>v.volume==0),"Vehicle volume mutes active smash voices");race.Flow.Save.Settings.vehicle=volume;
            foreach(var material in new[]{SmashAudio.Surface.Wood,SmashAudio.Surface.ChainLink,SmashAudio.Surface.Mailbox,SmashAudio.Surface.Sign})
            {
                var prop=FindObjectsByType<BreakableProp>().First(p=>p.surface==material);int count=prop.BreakCount; race.vehicle.Body.isKinematic=false;
                race.vehicle.Body.linearVelocity=Vector3.forward*20;prop.SendMessage("Contact",race.vehicle.GetComponent<BoxCollider>());
                prop.SendMessage("Contact",race.vehicle.GetComponent<BoxCollider>());
                Check(prop.IsBroken&&prop.BreakCount==count+1&&!prop.GetComponent<BoxCollider>().enabled,material+" yields once and cannot block car");
            }
            yield return new WaitForSeconds(4.2f);Check(BreakableProp.MovingDebrisCount==0,"All material debris cleaned after lifetime");
            BreakableProp.RestoreRace();yield return null;Check(FindObjectsByType<BreakableProp>().All(p=>!p.IsBroken),"All material props restored");
            var drivers=race.Drivers.ToArray();
            for(int i=0;i<drivers.Length;i++)
            {
                int direction=i%2==0?1:-1;drivers[i].Initialize(race,drivers[i].Car,false,direction,i<2?.95f:1.0f);
                float s=direction>0?3600-i*20:4760+i*20;drivers[i].Place(s,race.road.TrafficLane(s,direction,i>=2));
            }
            var covered=new bool[4,3];var peak=new float[4];var maxError=new float[4];var lowSpeed=new float[4];
            using(var log=new StreamWriter(Dir+"/highway.csv"))
            {
                log.WriteLine("time,car,direction,station,lateral,lane_error,speed,target,grounded");float start=Time.time,next=0;
                while(Time.time-start<95)
                {
                    foreach(var d in drivers)
                    {
                        int i=Array.IndexOf(drivers,d);float s=race.road.Project(d.transform.position,out _);var p=race.road.At(s,out var f);
                        float side=Vector3.Dot(d.transform.position-p,Vector3.Cross(Vector3.up,f).normalized),blend=race.road.HighwayBlend(s);
                        if(blend>.98f){covered[i,1]=true;peak[i]=Mathf.Max(peak[i],d.Car.ForwardSpeed);maxError[i]=Mathf.Max(maxError[i],Mathf.Abs(side-race.road.TrafficLane(s,d.Direction,i>=2)));}
                        if(s<3720)covered[i,0]=true;if(s>4630)covered[i,2]=true;
                        if(d.Car.ForwardSpeed<2)lowSpeed[i]+=Time.deltaTime;
                        if(Time.time>=next)log.WriteLine($"{Time.time-start:F2},{i},{d.Direction},{s:F2},{side:F2},{Mathf.Abs(side-race.road.TrafficLane(s,d.Direction,i>=2)):F2},{d.Car.ForwardSpeed:F2},{d.TargetSpeed:F2},{d.Car.GroundedWheels}");
                    }
                    if(Time.time>=next){next=Time.time+.2f;log.Flush();}yield return null;
                }
            }
            for(int i=0;i<4;i++)
            {
                Check(covered[i,0]&&covered[i,1]&&covered[i,2],"Traffic "+i+" traverses highway and both transitions");
                Check(peak[i]>22&&maxError[i]<3&&lowSpeed[i]<8,$"Traffic {i}: highway peak={peak[i]:F2}m/s laneError={maxError[i]:F2}m lowSpeed={lowSpeed[i]:F2}s");
            }
            File.WriteAllText(Dir+"/done.txt",$"{checks.Count(x=>x.StartsWith("PASS"))}/{checks.Count} checks");
            if(!Application.isEditor)Application.Quit();
        }
    }
}
