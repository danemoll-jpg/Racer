using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
namespace Racer
{
    // Activity results never enter the race timer, penalty ledger or record boards.
    public sealed class ArcadeActivities:MonoBehaviour
    {
        [Serializable] public sealed class Best {public string key;public float value;public int medal;}
        [Serializable] public sealed class Archive {public int version=1;public List<Best> results=new();}
        public Archive Results {get;private set;}=new();
        public ActivitySite[] Sites {get;private set;}
        public ActivitySite Selected {get;private set;}
        public bool AttemptActive {get;private set;}
        public string Feedback {get;private set;}
        public int Awards {get;private set;}
        public float LastDistance {get;private set;}
        public float LastAirtime {get;private set;}
        public float LastSpeed {get;private set;}
        public float LastJumpAward {get;private set;}
        public int SmashCount=>smashed.Count;
        public string Location {get{if(!Selected||!car)return "";var delta=Selected.transform.position-car.Body.position;int compass=Mathf.RoundToInt(Mathf.Repeat(Mathf.Atan2(delta.x,delta.z)*Mathf.Rad2Deg,360)/45)%8;return $"{Vector3.ProjectOnPlane(delta,Vector3.up).magnitude:0} m {new[]{"N","NE","E","SE","S","SW","W","NW"}[compass]}";}}
        public string Hud=>Time.time<feedbackUntil?Feedback:AttemptActive?$"{Selected.title} / {Mathf.Max(0,deadline-Time.time):0}s / {Location}\n{(Selected.kind==ActivitySite.Kind.Smash?SmashCount+" distinct props":"Land a clean jump in the marked area")}":race.FreeRoam?$"FREE ROAM / {Selected?.title} / {Location}\nEsc or Start: activities, retry, menu":"";
        RaceDirector race;ArcadeVehicle car;VehicleConfiguration configuration;
        readonly HashSet<BreakableProp> smashed=new();readonly Dictionary<ActivitySite,bool> armed=new();
        string path;Vector3 previous,takeoff,landing;float warm,air,stable,feedbackUntil,deadline,blockedUntil,impactSpeed;bool sampled,flying,invalid,touchedDown;
        ActivitySite jumpSite;
        public void Initialize(RaceDirector director,string root)
        {
            race=director;car=race.vehicle;configuration=car.GetComponent<VehicleConfiguration>();path=Path.Combine(root,"activities-v1.json");
            Sites=FindObjectsByType<ActivitySite>().OrderBy(s=>s.id).ToArray();Selected=Sites.FirstOrDefault(s=>s.kind!=ActivitySite.Kind.Speed);
            if(File.Exists(path))try{Results=JsonUtility.FromJson<Archive>(File.ReadAllText(path))??new();if(Results.results==null)Results.results=new();}catch(Exception e){Debug.LogWarning("Activity save could not be read: "+e.Message);}
            car.GetComponent<VehicleRespawn>().Respawned+=Recovered;BreakableProp.BrokenByVehicle+=Smash;
            var contacts=car.gameObject.AddComponent<ActivityLandingContact>();contacts.activities=this;
        }
        public string Key(ActivitySite s)=>s.id+"/"+race.courseId+"/activities-v1/"+configuration.profileId;
        public Best PersonalBest(ActivitySite s)=>Results.results.FirstOrDefault(b=>b.key==Key(s));
        public string Targets{get{if(!Selected)return "";Selected.Targets(configuration.profileId,out float b,out float s,out float g);return $"Bronze {b:0} / silver {s:0} / gold {g:0} "+(Selected.kind==ActivitySite.Kind.Jump?"m":"props / "+Selected.Seconds.ToString("0")+"s");}}
        public void Cycle(){var choices=Sites.Where(s=>s.kind!=ActivitySite.Kind.Speed).ToArray();if(choices.Length==0)return;Cancel();Selected=choices[(Array.IndexOf(choices,Selected)+1)%choices.Length];}
        public void BeginAttempt()
        {
            if(!race.FreeRoam||!Selected)return;Cancel();smashed.Clear();BreakableProp.RestoreRace();AttemptActive=true;deadline=Time.time+Selected.Seconds;Message(Selected.title+" / attempt started",3);
        }
        public void Cancel(){AttemptActive=false;smashed.Clear();warm=0;ResetFlight();}
        public void NewSession(){Cancel();armed.Clear();sampled=false;warm=0;blockedUntil=Time.time+1;Feedback=null;feedbackUntil=0;LastDistance=LastAirtime=LastSpeed=LastJumpAward=0;}
        void Recovered(){if(AttemptActive)Message("Attempt cancelled by recovery / retry from pause menu",4);Cancel();armed.Clear();sampled=false;warm=0;blockedUntil=Time.time+2;}
        void ResetFlight(){flying=false;invalid=touchedDown=false;air=stable=0;jumpSite=null;}
        public void SolidContact(Vector3 normal,float relativeSpeed){if(flying&&(normal.y<.45f||relativeSpeed>21))invalid=true;}
        void Smash(BreakableProp prop,ArcadeVehicle source)
        {
            if(source!=car||!race.FreeRoam||!AttemptActive||Selected.kind!=ActivitySite.Kind.Smash||race.Flow.State!=RaceFlow.Stage.Racing||Time.time<blockedUntil||warm<.5f)return;
            if(Selected.props==null||!Selected.props.Contains(prop)||!smashed.Add(prop))return;
            Message(Selected.title+" / "+smashed.Count+" distinct props",2);
            if(smashed.Count>=Selected.gold)FinishSmash();
        }
        void FinishSmash(){var value=smashed.Count;AttemptActive=false;Award(Selected,value,"props");}
        void FixedUpdate()
        {
            if(!race||!race.Flow||race.Flow.State!=RaceFlow.Stage.Racing)return;
            if(!race.FreeRoam&&race.Progress.Finished)return;
            var p=car.Body.position;float dt=Time.fixedDeltaTime;bool grounded=car.GroundedWheels>=2;
            if(!sampled){previous=p;sampled=true;return;}
            float step=Vector3.Distance(p,previous);bool discontinuity=step>Mathf.Max(3,car.Body.linearVelocity.magnitude*dt*2+.3f);
            if(discontinuity){Recovered();previous=p;return;}
            if(Time.time<blockedUntil){previous=p;return;}
            if(AttemptActive&&Time.time>=deadline){if(Selected.kind==ActivitySite.Kind.Smash)FinishSmash();else{AttemptActive=false;Message("Jump attempt expired / retry from pause menu",4);}}
            if(grounded&&!flying){warm=car.transform.up.y>.8f?warm+dt:0;}
            if(!grounded&&!flying&&warm>=.5f&&Vector3.ProjectOnPlane(car.Body.linearVelocity,Vector3.up).magnitude>4)
            {
                flying=true;invalid=touchedDown=false;takeoff=previous;air=stable=0;impactSpeed=0;
                jumpSite=AttemptActive&&Selected.kind==ActivitySite.Kind.Jump&&Vector3.Distance(takeoff,Selected.transform.position)<Selected.radius&&Vector3.Dot(car.Body.linearVelocity,Selected.forward)>2?Selected:null;
            }
            if(flying)
            {
                invalid|=configuration.WipedOut;
                if(!grounded){if(!touchedDown)air+=dt;stable=0;impactSpeed=Mathf.Max(impactSpeed,-car.Body.linearVelocity.y);}
                else
                {
                    // Freeze measurement at first touchdown. Subsequent settling or
                    // suspension bounces must not extend the jump's distance/airtime.
                    if(!touchedDown){landing=p;touchedDown=true;}stable+=dt;
                    invalid|=configuration.WipedOut||car.transform.up.y<.65f||car.WaterImmersion>.05f||impactSpeed>18;
                    if(stable>=.3f)
                    {
                        float distance=Vector3.ProjectOnPlane(landing-takeoff,Vector3.up).magnitude;
                        if(!invalid&&air>=.25f&&air<12&&distance>=3&&distance<250)
                        {
                            LastDistance=distance;LastAirtime=air;Message($"CLEAN JUMP / {distance:0.0} m / {air:0.00} s / {Mathf.RoundToInt(distance*10+air*100)} pts",4);
                            if(jumpSite&&race.FreeRoam){AttemptActive=false;Award(jumpSite,distance,"m");}
                        }
                        else if(jumpSite){AttemptActive=false;Message("Jump not scored / unstable, wet or hard landing",4);}
                        ResetFlight();warm=0;
                    }
                }
            }
            if(race.FreeRoam&&warm>=.5f&&!flying)
            foreach(var site in Sites.Where(s=>s.kind==ActivitySite.Kind.Speed))
            {
                var f=site.forward.normalized;float a=Vector3.Dot(previous-site.transform.position,f),b=Vector3.Dot(p-site.transform.position,f);
                if(Mathf.Abs(b)>25)armed[site]=true;
                bool crossing=a<0&&b>=0 || site.bothDirections&&a>0&&b<=0;
                if(!crossing||!armed.TryGetValue(site,out bool ready)||!ready)continue;
                float t=a/(a-b);var cross=Vector3.Lerp(previous,p,t);var lateral=Vector3.ProjectOnPlane(cross-site.transform.position,f);
                float speed=Vector3.ProjectOnPlane(car.Body.linearVelocity,Vector3.up).magnitude;
                if(lateral.magnitude>site.radius||speed<3||Mathf.Abs(Vector3.Dot(car.Body.linearVelocity.normalized,f))<.65f||Mathf.Abs(step/dt-speed)>Mathf.Max(4,speed*.35f))continue;
                armed[site]=false;LastSpeed=speed;Award(site,speed,"m/s");
            }
            previous=p;
        }
        void Award(ActivitySite site,float value,string units)
        {
            if(!race.FreeRoam||!float.IsFinite(value)||value<0)return;int medal=site.Medal(value,configuration.profileId);var best=PersonalBest(site);
            if(site.kind==ActivitySite.Kind.Jump)LastJumpAward=value;
            if(best==null){best=new Best{key=Key(site)};Results.results.Add(best);}best.value=Mathf.Max(best.value,value);best.medal=Mathf.Max(best.medal,medal);Awards++;
            string measurement=site.kind==ActivitySite.Kind.Speed?$"{value*3.6f:0.0} km/h / PB {best.value*3.6f:0.0}":$"{value:0.0} {units} / PB {best.value:0.0}";
            Message(site.title+" / "+measurement+"\n"+new[]{"No medal yet","BRONZE","SILVER","GOLD"}[medal],5);
            try{AtomicSave.Write(path,JsonUtility.ToJson(Results,true));}catch(Exception e){Message("Activity result could not be saved: "+e.Message,6);}
        }
        void Message(string value,float seconds){Feedback=value;feedbackUntil=Time.time+seconds;}
        void OnDestroy(){BreakableProp.BrokenByVehicle-=Smash;if(car)car.GetComponent<VehicleRespawn>().Respawned-=Recovered;}
    }
    public sealed class ActivityLandingContact:MonoBehaviour {public ArcadeActivities activities;void OnCollisionEnter(Collision c){foreach(var contact in c.contacts)activities.SolidContact(contact.normal,Mathf.Abs(Vector3.Dot(c.relativeVelocity,contact.normal)));}}
}
