using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Racer
{
    public sealed class DiscoveryValidation:MonoBehaviour
    {
        static string Arg(string key,string fallback=""){var a=Environment.GetCommandLineArgs();int i=Array.IndexOf(a,key);return i>=0&&i+1<a.Length?a[i+1]:fallback;}
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Boot(){if(Arg("-discoveryCheck")==""||Arg("-racerTestSave")=="")return;var g=new GameObject("CR091 explicit validation");DontDestroyOnLoad(g);g.AddComponent<DiscoveryValidation>();}
        RaceDirector race;RaceFlow flow;ArcadeVehicle car;string dir,contacts="";readonly List<string> checks=new();
        void Check(bool ok,string name){checks.Add((ok?"PASS ":"FAIL ")+name);File.WriteAllLines(dir+"/checks.txt",checks);}
        public void Contact(Collision c){foreach(var p in c.contacts)contacts+=c.collider.name+":"+p.normal.ToString("F3")+" separation="+p.separation+";";}
        IEnumerator Start()
        {
            Application.runInBackground=true;dir=Arg("-evidence","Docs/CR091-096/test");Directory.CreateDirectory(dir);
            string course=Arg("-course","StreetLoopGreybox");if(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name!=course)UnityEngine.SceneManagement.SceneManager.LoadScene(course);yield return null;yield return null;
            race=FindAnyObjectByType<RaceDirector>();flow=race.Flow;car=race.vehicle;
            if(Arg("-discoveryCheck")=="summit")yield return Summit();
            if(Arg("-discoveryCheck")=="finish")yield return Finish();
            File.WriteAllText(dir+"/done.txt","Completed. Read individual failures. No human/controller/listening acceptance.");Application.Quit();
        }
        IEnumerator Summit()
        {
            var root=GameObject.Find("CR094 summit launch").transform;var touch=car.gameObject.AddComponent<DiscoveryContacts>();touch.owner=this;
            File.WriteAllText(dir+"/ramps.csv","course,mode,vehicle,speed,line,direction,travel,air,minUp,recovered,awards,completed\n");
            File.WriteAllText(dir+"/flight.csv","vehicle,speed,line,launchX,launchY,launchZ,launchSpeed,crestX,crestY,crestZ,crestGround,crestClearance,landingX,landingY,landingZ,landingSpeed,longestFlight,peakY\n");
            foreach(var profile in race.EligibleVehicles)
            foreach(float speed in Arg("-quick")=="yes"?new[]{32f}:new[]{24f,32f,40f})
            foreach(float line in Arg("-quick")=="yes"?new[]{0f}:new[]{0f,-6f,6f,-(12-profile.Size.x*.5f),12-profile.Size.x*.5f})
            {
                if(flow.State!=RaceFlow.Stage.Ready){flow.Pause();flow.QuitRace();}flow.OpenGarage();flow.SelectVehicle(profile.Id);flow.CloseGarage();race.opponents=race.traffic=false;
                bool racing=Arg("-mode")=="race";if(racing)flow.StartRace();else flow.StartFreeRoam();while(flow.State!=RaceFlow.Stage.Racing)yield return null;
                Time.timeScale=float.TryParse(Arg("-testSpeed","1"),out var rate)?rate:1;
                car.enabled=false;car.GetComponent<VehicleInput>().enabled=false;bool reverse=Arg("-opposite")=="yes";var f=root.forward*(reverse?-1:1);float z=reverse?245:5;
                var p=root.TransformPoint(new Vector3(line,0,z));var hits=Physics.RaycastAll(p+Vector3.up*100,Vector3.down,200).Where(h=>h.collider.name.StartsWith("Ground_")).OrderBy(h=>h.distance).ToArray();p.y=hits[0].point.y+car.suspensionLength-.12f;
                car.Body.position=p;car.Body.rotation=Quaternion.LookRotation(f);car.transform.SetPositionAndRotation(p,car.Body.rotation);car.Body.linearVelocity=f*(Arg("-fromRest")=="yes"?0:speed);car.Body.angularVelocity=Vector3.zero;car.ClearSteering();Physics.SyncTransforms();FindAnyObjectByType<ChaseCamera>()?.Snap();flow.Activities.NewSession();race.ResetSampling(p,Time.timeAsDouble);
                float began=Time.time,minUp=1,air=0,travel=0,continuousAir=0,longestAir=0,peakY=p.y,launchSpeed=0,landingSpeed=0,crestGround=0,crestClearance=0;Vector3 launch=Vector3.zero,crest=Vector3.zero,landing=Vector3.zero;bool launched=false,crested=false,landed=false;int awards=flow.Activities.Awards;bool complete=false;string id=profile.Id+"-"+speed+"-"+line;int settled=0;
                using(var trace=new StreamWriter(dir+"/"+id+".csv")){
                    trace.WriteLine("time,x,y,z,speed,vx,vy,vz,throttle,brake,steer,wheels,suspension,alignment,up,contact");
                    while(Time.time-began<22){var local=root.InverseTransformPoint(car.Body.position);float yaw=Vector3.SignedAngle(Vector3.ProjectOnPlane(car.transform.forward,Vector3.up),f,Vector3.up);float steer=Mathf.Clamp(yaw*.045f+(line-local.x)*(reverse?-.10f:.10f),-.5f,.5f);float throttle=Mathf.Clamp01((speed-car.ForwardSpeed)*.5f),brake=car.ForwardSpeed>speed+1?.2f:0;
                        car.Simulate(throttle,brake,steer,Time.fixedDeltaTime);yield return new WaitForFixedUpdate();var q=car.Body.position;var v=car.Body.linearVelocity;minUp=Mathf.Min(minUp,car.transform.up.y);if(car.GroundedWheels<2)air+=Time.fixedDeltaTime;travel=Vector3.Dot(q-p,f);
                        trace.WriteLine($"{Time.time-began:F3},{q.x:F3},{q.y:F3},{q.z:F3},{car.ForwardSpeed:F3},{v.x:F3},{v.y:F3},{v.z:F3},{throttle:F3},{brake:F3},{steer:F3},{car.GroundedWheels},{car.SuspensionLift:F3},{car.AlignmentTorque:F3},{car.transform.up.y:F3},\"{contacts}\"");contacts="";
                        peakY=Mathf.Max(peakY,q.y);continuousAir=car.GroundedWheels<2?continuousAir+Time.fixedDeltaTime:0;longestAir=Mathf.Max(longestAir,continuousAir);
                        if(!reverse&&!launched&&local.z>98&&car.GroundedWheels<2){launched=true;launch=q;launchSpeed=car.ForwardSpeed;if(line==0)ThreeFeatureValidation.CaptureUi(dir+"/"+id+"-launch.png");}
                        if(launched&&!crested&&local.z>=125){crested=true;crest=q;var ground=Physics.RaycastAll(q+Vector3.up*150,Vector3.down,400).Where(h=>h.collider.name.StartsWith("Ground_")).OrderBy(h=>h.distance).First();crestGround=ground.point.y;crestClearance=car.GetComponent<Collider>().bounds.min.y-crestGround;if(line==0)ThreeFeatureValidation.CaptureUi(dir+"/"+id+"-crest.png");}
                        if(launched&&!landed&&local.z>132&&longestAir>1&&car.GroundedWheels>=2){landed=true;landing=q;landingSpeed=car.ForwardSpeed;if(line==0)ThreeFeatureValidation.CaptureUi(dir+"/"+id+"-landing.png");}
                        if(travel>200&&car.GroundedWheels>=2&&car.transform.up.y>.7f)settled++;else settled=0;if(settled>=20){complete=true;break;}if(Mathf.Abs(local.x)>45)break;
                    }
                }
                File.AppendAllText(dir+"/flight.csv",$"{profile.Id},{speed},{line},{launch.x:F3},{launch.y:F3},{launch.z:F3},{launchSpeed:F3},{crest.x:F3},{crest.y:F3},{crest.z:F3},{crestGround:F3},{crestClearance:F3},{landing.x:F3},{landing.y:F3},{landing.z:F3},{landingSpeed:F3},{longestAir:F3},{peakY:F3}\n");
                ThreeFeatureValidation.CaptureUi(dir+"/"+id+".png");bool recovered=car.GetComponent<VehicleRespawn>().TryRecoverLocal();File.AppendAllText(dir+"/ramps.csv",$"{race.courseName},{(racing?"race":"roam")},{profile.Id},{speed},{line},{(reverse?-1:1)},{travel:F2},{air:F2},{minUp:F3},{recovered},{flow.Activities.Awards-awards},{complete}\n");car.enabled=true;
            }
        }
        IEnumerator Finish()
        {
            race.opponents=race.traffic=false;flow.StartRace();while(flow.State!=RaceFlow.Stage.Racing)yield return null;car.enabled=false;car.Body.isKinematic=true;
            var gate=race.gates[0];double clock=100;var state=race.Racers[0];var progress=race.Progress;
            void Seed(int laps){progress.Restart();state.Branch.Clear();state.FinishArmed=false;state.FinishApproach=0;for(int lap=0;lap<=laps;lap++){progress.Cross(0,true,clock++);if(lap==laps)break;for(int i=1;i<race.gates.Length;i++)progress.Cross(i,true,clock++);}for(int i=1;i<race.gates.Length;i++)progress.Cross(i,true,clock++);state.FinishArmed=true;state.VerifiedRoad=race.road.Length-100;}
            void Passage(float x,float y,bool backwards=false){var start=gate.transform.TransformPoint(new Vector3(x,y,backwards?35:-35));race.ResetSampling(start,clock);for(int j=1;j<=140;j++){var p=gate.transform.TransformPoint(new Vector3(x,y,(backwards?35:-35)+(backwards?-1:1)*j*.5f));race.Sample(p,gate.transform.forward,clock+=.02);}}
            foreach(var profile in race.EligibleVehicles){car.GetComponent<VehicleConfiguration>().Apply(profile.Id);
                foreach(float y in new[]{0f,28f})foreach(float side in new[]{-1f,1f}){Seed(0);Passage(side*(gate.halfWidth+profile.Size.x+2),y);Check(progress.CompletedLaps==1&&progress.MissedGates==1&&progress.PenaltySeconds==5,profile.Id+" missed finish side="+side+" height="+y);int n=progress.CompletedLaps;Passage(side*(gate.halfWidth+profile.Size.x+2),y,true);Passage(side*(gate.halfWidth+profile.Size.x+2),y);Check(progress.CompletedLaps==n&&progress.MissedGates==1,"No reverse/linger duplicate");}
                Seed(progress.TargetLaps-1);Passage(gate.halfWidth+4,0);Check(progress.Finished&&progress.MissedGates==1,"Final lap completes with five seconds / "+profile.Id);
                Seed(0);state.FinishApproach=0;var from=gate.transform.position-gate.transform.forward*30+gate.transform.right*20;race.ResetSampling(from,clock);race.Sample(from+gate.transform.forward*60,gate.transform.forward,clock+=.02);Check(progress.CompletedLaps==0&&progress.MissedGates==0,"Teleport cannot finish / "+profile.Id);
                progress.Restart();state.FinishArmed=false;state.FinishApproach=0;Passage(gate.halfWidth+4,0);Check(progress.CompletedLaps==0&&progress.MissedGates==0,"Initial outside crossing does not award / "+profile.Id);
                foreach(var branch in race.Branches){
                    progress.Restart();state.Branch.Clear();state.FinishArmed=false;state.VerifiedRoad=0;state.FinishApproach=0;
                    progress.Cross(0,true,clock++);float origin=race.road.Project(gate.transform.position,out _);int first=Array.FindIndex(race.gates,g=>race.road.Relative(race.road.Project(g.transform.position,out _),origin)>race.road.Relative(branch.entryRoad,origin));
                    for(int i=1;i<first;i++)progress.Cross(i,true,clock++);
                    var entry=branch.At(0,out var heading);race.ResetSampling(entry-heading*3,clock);int exits=state.Branch.Exits;
                    for(float s=-2.5f;s<branch.Length+2;s+=.5f){var p=s<0?entry+heading*s:branch.At(Mathf.Min(s,branch.Length),out heading);car.transform.rotation=Quaternion.LookRotation(heading);race.Sample(p,heading,clock+=.02);}
                    bool exited=state.Branch.Exits>exits;
                    for(int i=1;i<race.gates.Length;i++)if(progress.NextGate==i)progress.Cross(i,true,clock++);
                    state.FinishArmed=true;state.VerifiedRoad=race.road.Length-100;Passage(gate.halfWidth+profile.Size.x+2,28);
                    Check(exited&&progress.CompletedLaps==1&&progress.MissedGates==1&&progress.PenaltySeconds==5,profile.Id+" sampled legal shortcut then airborne missed finish / "+branch.title);
                }
            }
            // Render one isolated final-lap penalty after the fixture burst expires.
            yield return new WaitForSecondsRealtime(5.1f);Seed(progress.TargetLaps-1);Passage(gate.halfWidth+4,0);Check(progress.Finished&&flow.PenaltyNotice=="Missed 1 gate  +5s (5s each)","Final-lap HUD presents exactly one five-second penalty");yield return null;ThreeFeatureValidation.CaptureUi(dir+"/finish-feedback.png");
        }
    }
    public sealed class DiscoveryContacts:MonoBehaviour{public DiscoveryValidation owner;void OnCollisionEnter(Collision c)=>owner.Contact(c);void OnCollisionStay(Collision c)=>owner.Contact(c);}
}
