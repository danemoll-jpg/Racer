using System;
using System.IO;
using System.Linq;
using System.Collections;
using UnityEngine;
namespace Racer
{
    public sealed partial class CR112Validation
    {
        IEnumerator ChampionshipLaps()
        {
            flow.Save.Settings.vehicleId="moto";flow.Save.Settings.opponents=true;flow.Save.Settings.traffic=false;flow.Save.Settings.opponentRoster=new[]{"moto","atv","moto"};flow.Save.Settings.estimateAiFinishes=false;flow.Save.SaveSettings();race.opponents=true;
            var definition=new RacePlaylists.Definition{name="Main Flight Championship",entries=new(){new(){course=4,laps=1},new(){course=5,laps=1},new(){course=5,laps=1}}};flow.StartPlaylist(definition);yield return null;yield return null;Bind();
            if(Arg("-resumeEvent")==""){
                yield return MainLap("forward-moto",true);if(!race.Progress.Finished)yield break;
                var totals=RacePlaylists.Championship.Standings().Select(s=>s.points).ToArray();flow.OpenSettings();flow.CloseSettings();flow.CompleteResults();
                Check(RacePlaylists.Championship.Standings().Select(s=>s.points).SequenceEqual(totals),"Reopening/finalizing results does not double-award points");
            }else{
                // Reconstruct provisional points from the retained completed standings
                // across a build. This fixture is cleared by Restart; no fixture result
                // contributes to final totals. Remaining physical laps start normally.
                RacePlaylists.Championship.Record(0,UnityEngine.JsonUtility.FromJson<PlaylistChampionship.Event>(File.ReadAllText(Arg("-resumeEvent"))));
                File.WriteAllText(dir+"/resumed-evidence.txt","Forward motorcycle lap and result-reopen checks reused from final10/playlist. Provisional replacement fixture reconstructed from retained standings; see provisional-resume-notes.txt beside "+Arg("-resumeEvent"));
            }
            var previous=RacePlaylists.Championship.Events[0];
            car.GetComponent<VehicleConfiguration>().Apply("atv");flow.Save.Settings.vehicleId="atv";flow.Save.Settings.estimateAiFinishes=true;flow.StartRace();
            Check(RacePlaylists.Championship.Events[0]==null&&!RacePlaylists.Championship.Complete,"Restart removes current provisional result");
            if(Arg("-resumeEvent")!="")flow.Save.Settings.estimateAiFinishes=false;
            yield return MainLap("forward-atv",Arg("-resumeEvent")!="");if(!race.Progress.Finished)yield break;
            Check(RacePlaylists.Championship.Events.Count(e=>e!=null)==1&&!ReferenceEquals(previous,RacePlaylists.Championship.Events[0]),"Restarted forward entry replaces its previous event snapshot");
            flow.Save.Settings.estimateAiFinishes=false;flow.NextPlaylistRace();flow.NextPlaylistRace();yield return null;yield return null;Bind();Check(RacePlaylists.Position==1,"Repeated Next advances exactly one entry");
            yield return MainLap("reverse-atv",true);if(!race.Progress.Finished)yield break;
            flow.Save.Settings.vehicleId="moto";flow.Save.Settings.estimateAiFinishes=true;flow.NextPlaylistRace();yield return null;yield return null;Bind();yield return MainLap("reverse-moto",false);if(!race.Progress.Finished)yield break;
            var championship=RacePlaylists.Championship;Check(championship.Complete&&championship.Events.Length==3,"Three-event playlist reaches complete all-event summary");
            Check(championship.Standings().All(s=>s.points==championship.Events.Sum(e=>e.order.Single(r=>r.id==s.id).points)),"Cumulative totals equal each event's assigned points");
            Check(championship.Events.All(e=>e.order.Select(r=>r.id).OrderBy(id=>id).SequenceEqual(new[]{"racer-0","racer-1","racer-2","racer-3"})),"Racer identities stay stable across courses and player vehicle changes");
            File.WriteAllText(dir+"/championship.txt",championship.Announcement+"\n"+string.Join("\n\n",Enumerable.Range(0,3).Select(championship.Summary)));ThreeFeatureValidation.CaptureUi(dir+"/championship-final.png");
            flow.NextPlaylistRace();Check(RacePlaylists.Position==2&&championship.Complete,"Next after final event does not award or advance again");
            flow.QuitRace();flow.StartPlaylist(definition);yield return null;yield return null;Bind();flow.Pause();flow.QuitRace();Check(RacePlaylists.Active==null&&RacePlaylists.Championship==null&&flow.State==RaceFlow.Stage.Ready,"Quitting unfinished playlist returns to menu without a completed winner");
        }
        IEnumerator MainLap(string tag,bool verifyAi)
        {
            while(flow.State==RaceFlow.Stage.Countdown)yield return null;
            ThreeFeatureValidation.CaptureUi(dir+"/"+tag+"-start-clearance.png");
            var pilot=car.gameObject.AddComponent<RoadDriver>();pilot.Initialize(race,car,true,1,1);pilot.Racer=race.Racers[0];car.GetComponent<VehicleInput>().enabled=false;
            var flights=race.GetComponent<MountainFlights>().flights;var began=new float[flights.Length];var launch=new Vector3[flights.Length];var speeds=new float[flights.Length];var landed=new bool[flights.Length];var shots=new bool[flights.Length];float start=Time.time,next=0;var contact=car.gameObject.AddComponent<CR117ContactLog>();contact.Path=dir+"/"+tag+"-contacts.txt";
            using(var w=new StreamWriter(dir+"/"+tag+".csv")){
                w.AutoFlush=true;w.WriteLine("time,x,y,z,speed,wheels,up,throttle,brake,nextGate,lap,recoveries,groundBelow,targetSpeed,obstacle");
                while(!race.ClassificationFinal&&Time.time-start<420){yield return new WaitForFixedUpdate();var p=car.Body.position;
                    for(int i=0;i<flights.Length;i++){var f=flights[i];float s=Vector3.Dot(p-f.lip,f.forward);float side=Math.Abs(Vector3.Dot(p-f.lip,Vector3.Cross(Vector3.up,f.forward)));if(began[i]==0&&s>-2&&s<20&&side<20&&car.GroundedWheels<2){began[i]=Time.time;launch[i]=p;speeds[i]=car.Body.linearVelocity.magnitude;ThreeFeatureValidation.CaptureUi(dir+"/"+tag+"-"+i+"-takeoff.png");}
                        if(began[i]>0&&!landed[i]){float air=Time.time-began[i];if(air>=2&&!shots[i]){shots[i]=true;ThreeFeatureValidation.CaptureUi(dir+"/"+tag+"-"+i+"-flight.png");}if(air>1&&car.GroundedWheels>=2){landed[i]=true;float distance=Vector3.ProjectOnPlane(p-launch[i],Vector3.up).magnitude;Check(air>=3&&distance>=90,$"{tag} {f.name}: attainable takeoff={speeds[i]:F2} m/s, air={air:F2}s, distance={distance:F2}m, wheel-supported landing {p}");ThreeFeatureValidation.CaptureUi(dir+"/"+tag+"-"+i+"-landing.png");}}
                    }
                    if(Time.time>next){next=Time.time+.1f;w.WriteLine($"{Time.time-start},{p.x},{p.y},{p.z},{car.ForwardSpeed},{car.GroundedWheels},{car.transform.up.y},{pilot.LastThrottle},{pilot.LastBrake},{race.Progress.NextGate},{race.Progress.CompletedLaps},{pilot.RecoveryCount},{Ground(p)},{pilot.TargetSpeed},{pilot.LastObstacle}");}
                    if(pilot.RecoveryCount>0){Check(false,tag+": stopped at first unintended automatic recovery; inspect local connection");break;}
                }
            }
            Check(race.Progress.Finished&&race.Progress.CompletedLaps==1&&race.Progress.MissedGates==0,$"{tag}: complete ordinary-input main-line lap; misses={race.Progress.MissedGates}, recoveries={pilot.RecoveryCount}");Check(landed.All(x=>x),tag+": both mandatory big jumps encountered on main line");
            if(verifyAi)Check(race.Racers.Skip(1).All(r=>r.Progress.Finished),tag+": three AI navigate and physically finish the new direction");
            File.WriteAllText(dir+"/"+tag+"-results.txt",race.Standings());if(pilot){pilot.enabled=false;Destroy(pilot);}Destroy(contact);ThreeFeatureValidation.CaptureUi(dir+"/"+tag+"-results.png");
        }
    }
    public sealed class CR117ContactLog:MonoBehaviour
    {
        public string Path;
        void OnCollisionEnter(Collision c){if(Path==null)return;File.AppendAllText(Path,$"time={Time.time} position={transform.position} hit={c.collider.name} impulse={c.impulse.magnitude} contacts={string.Join(";",c.contacts.Select(p=>p.point+" normal="+p.normal))}\n");}
    }
}
