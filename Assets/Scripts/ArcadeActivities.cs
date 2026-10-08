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
        public ActivityRecords Records {get;private set;}
        public ActivitySite[] Sites {get;private set;}
        public ActivitySite Selected {get;private set;}
        public bool AttemptActive {get;private set;}
        public string Feedback {get;private set;}
        public int Awards {get;private set;}
        public GameObject PlayerObject=>car?car.gameObject:null;
        public float LastDistance {get;private set;}
        public float LastAirtime {get;private set;}
        public float LastSpeed {get;private set;}
        public float LastJumpAward {get;private set;}
        public ActivitySite LastAwardSite {get;private set;}
        public string LastJumpDiagnostic {get;private set;}
        public int SmashCount=>smashed.Count;
        public string Location {get{if(!Selected||!car)return "";var delta=Selected.transform.position-car.Body.position;int compass=Mathf.RoundToInt(Mathf.Repeat(Mathf.Atan2(delta.x,delta.z)*Mathf.Rad2Deg,360)/45)%8;return $"{DisplayUnits.Distance(Vector3.ProjectOnPlane(delta,Vector3.up).magnitude)} {new[]{"N","NE","E","SE","S","SW","W","NW"}[compass]}";}}
        public string Hud=>CampaignEventHidesHud?"":CampaignHud(Time.time<feedbackUntil?Feedback:AttemptActive?$"{Selected.title} / {Mathf.Max(0,deadline-Time.time):0}s / {Location}\n{(Selected.kind==ActivitySite.Kind.Smash?SmashCount+" distinct props":"Land a jump from the marked ramp")}":race.FreeRoam?AtStart:"");
        // 0.96 Part A: a campaign jump / speed trap / smash event shows its own panel and attempt banner (CampaignEventUi), not
        // these lines
        static bool CampaignEventHidesHud=>CampaignRun.Active!=null&&CampaignRun.Active.Kind!=CampaignEventKind.Race&&CampaignRun.Active.Kind!=CampaignEventKind.TimeTrial;
        // 0.96 Part A: attempts that were not scored (a jump flown but not counted), for the event's attempt banner
        public int Misses{get;private set;}public string LastMiss{get;private set;}public ActivitySite LastMissSite{get;private set;}
        void Miss(ActivitySite site,string why){Misses++;LastMiss=why;LastMissSite=site;}
        // 0.96 Part B: the medal of the last scored result in Free Roam (-1 = none), for the HUD's medal badge, and the three
        // targets of a site as the medal displays write them (gold, silver, bronze)
        public int LastMedal{get;private set;}=-1;public string ResultValue{get;private set;}="";public bool ResultShowing=>LastMedal>=0&&Time.time<feedbackUntil;
        public string[] TargetStrings(ActivitySite s){if(!s||!car)return null;s.Targets(configuration.profileId,out float b,out float si,out float g);
            return s.kind==ActivitySite.Kind.Jump?new[]{MedalUi.Feet(g),MedalUi.Feet(si),MedalUi.Feet(b)}:s.kind==ActivitySite.Kind.Speed?new[]{MedalUi.Mph(g),MedalUi.Mph(si),MedalUi.Mph(b)}:new[]{Mathf.RoundToInt(g)+" props",Mathf.RoundToInt(si)+" props",Mathf.RoundToInt(b)+" props"};}
        // 0.91 Part C: during a campaign jump event its targets, the best so far and the time left stay on the HUD.
        string CampaignHud(string line){var watch=race&&race.Flow?race.Flow.GetComponent<CampaignTrapWatch>():null;var extra=watch?watch.Hud:null;return string.IsNullOrEmpty(extra)?line:string.IsNullOrEmpty(line)?extra:line+"\n"+extra;}
        // 0.79 Part B: in Free Roam, the activity the player is at (within its start radius) and how to start it; nothing
        // otherwise (the nearest one and its distance stay on the map and in the pause menu).
        public ActivitySite AtSite{get{if(!car||Sites==null)return null;var p=car.Body.position;return Sites.Where(s=>s&&Vector3.Distance(p,s.transform.position)<s.radius).OrderBy(s=>Vector3.SqrMagnitude(p-s.transform.position)).FirstOrDefault();}}
        // being at a jump or smash site makes it the selected activity, so pause menu > Activities starts this one
        string AtStart{get{var site=AtSite;if(!site)return "";if(!AttemptActive&&site.kind!=ActivitySite.Kind.Speed)Selected=site;string menu=MenuInput.Controller?"Start":"Esc";
            return site.title+"\n"+(site.kind==ActivitySite.Kind.Speed?"Speed trap: drive through it, either way":site.kind==ActivitySite.Kind.Jump?$"Jump: land it to score  ·  {menu} > Activities for a timed attempt":$"{menu} > Activities > Start to begin");}}
        RaceDirector race;ArcadeVehicle car;VehicleConfiguration configuration;
        readonly HashSet<BreakableProp> smashed=new();readonly Dictionary<ActivitySite,bool> armed=new();
        string path;Vector3 previous,takeoff,landing;float warm,air,stable,feedbackUntil,deadline,blockedUntil,impactSpeed;bool sampled,flying,invalid,touchedDown;
        ActivitySite jumpSite;string rejected;
        // 0.91 Part C: the first reason this flight cannot score (null = none so far)
        void Reject(string why){invalid=true;rejected??=why;}
        float contactImpact,minimumContactUp=1;
        bool oppositeAttempt;
        static bool Summit(ActivitySite site)=>site&&(site.id=="summit-homeward"||site.id=="summit-southface");
        bool AlignedWithSupport()
        {
            Vector3 normal=Vector3.zero;int count=0;
            foreach(var point in car.suspensionPoints)if(Physics.Raycast(car.transform.TransformPoint(point),-car.transform.up,out var hit,car.suspensionLength,car.groundMask,QueryTriggerInteraction.Ignore)&&hit.normal.y>.45f){normal+=hit.normal;count++;}
            return count>=2&&Vector3.Dot(car.transform.up,normal.normalized)>.85f;
        }
        // 0.94 Part B: car = another player's vehicle (split-screen Free Roam player 2: results shown to them, never recorded)
        public void Initialize(RaceDirector director,string root,ArcadeVehicle vehicle=null)
        {
            race=director;car=vehicle?vehicle:race.vehicle;configuration=car.GetComponent<VehicleConfiguration>();path=Path.Combine(root,"activities-v1.json");
            Sites=FindObjectsByType<ActivitySite>().OrderBy(s=>s.id).ToArray();Selected=Sites.FirstOrDefault(s=>s.kind!=ActivitySite.Kind.Speed);
            if(File.Exists(path))try{Results=JsonUtility.FromJson<Archive>(File.ReadAllText(path))??new();if(Results.results==null)Results.results=new();}catch(Exception e){Debug.LogWarning("Activity save could not be read: "+e.Message);}
            Records=new ActivityRecords(root,Results);
            car.GetComponent<VehicleRespawn>().Respawned+=Recovered;BreakableProp.BrokenByVehicle+=Smash;
            var contacts=car.gameObject.AddComponent<ActivityLandingContact>();contacts.activities=this;
        }
        // Timing-gate revisions do not invalidate untouched stunt/speed records.
        // Reverse Street's changed ramp shoulder gets a new activity category.
        public static string ActivityCourse(string course)=>course switch{
            "street-v12-corrections"=>"street-v11-arcade", "lake-v5-corrections"=>"lake-v4-arcade",
            "forest-reverse-v3-corrections"=>"forest-reverse-v2-arcade",
            // 0.87: the Summit Climb shortcut does not move any activity site, so their results stay with the v7 course.
            "lake-v8-summit-climb"=>"lake-v7-discovery", _=>course};
        string Suffix(ActivitySite s)=>"/activities-v2/"+configuration.profileId+(s.kind==ActivitySite.Kind.Speed&&oppositeAttempt?"/opposite":"/forward");
        public string Key(ActivitySite s)=>s.id+"/"+ActivityCourse(race.courseId)+Suffix(s);
        // The site's own key plus, in FreeRoamWorld, the keys it had in the course scenes.
        public string[] Keys(ActivitySite s)=>s.legacyRecords==null||s.legacyRecords.Length==0?new[]{Key(s)}:new[]{Key(s)}.Concat(s.legacyRecords.Select(k=>k+Suffix(s))).ToArray();
        public Best PersonalBest(ActivitySite s)
        {
            var own=Results.results.FirstOrDefault(b=>b.key==Key(s));if(s.legacyRecords==null||s.legacyRecords.Length==0)return own;
            var keys=Keys(s);var all=Results.results.Where(b=>keys.Contains(b.key)).ToList();if(all.Count==0)return null;
            return new Best{key=Key(s),value=all.Max(b=>b.value),medal=all.Max(b=>b.medal)};
        }
        public static string Measurement(ActivitySite site,float value)=>site.kind==ActivitySite.Kind.Speed?MedalUi.Mph(value):site.kind==ActivitySite.Kind.Jump?MedalUi.Feet(value):value.ToString("0")+" props"; // 0.96 Part B: whole feet and mph
        public string Targets{get{if(!Selected)return "";Selected.Targets(configuration.profileId,out float b,out float s,out float g);return Selected.kind==ActivitySite.Kind.Jump?$"Bronze {DisplayUnits.Target(b)} / silver {DisplayUnits.Target(s)} / gold {DisplayUnits.Target(g)}":$"Bronze {Measurement(Selected,b)} / silver {Measurement(Selected,s)} / gold {Measurement(Selected,g)} / {Selected.Seconds:0}s";}}
        public void Cycle(int d=1){var choices=Sites.Where(s=>s.kind!=ActivitySite.Kind.Speed).ToArray();if(choices.Length==0)return;Cancel();Selected=choices[(Math.Max(0,Array.IndexOf(choices,Selected))+d+choices.Length)%choices.Length];}
        public void BeginAttempt()
        {
            if(!race.FreeRoam||!Selected)return;Cancel();smashed.Clear();BreakableProp.RestoreRace();AttemptActive=true;deadline=Time.time+Selected.Seconds;Message(Selected.title+" / attempt started",3);
        }
        // 0.90: a campaign smash event: the site's attempt runs from GO for the event's time limit (Free Roam starts its own
        // attempts from the pause menu). Its count goes to the campaign, never to the activity records.
        public void BeginCampaignSmash(ActivitySite site,float seconds){if(!site)return;Cancel();smashed.Clear();Selected=site;AttemptActive=true;deadline=Time.time+seconds;warm=.5f;blockedUntil=0;Message(site.title+" / smash as many as you can",3);}
        public float LastSmashScore {get;private set;}
        public void Cancel(){AttemptActive=false;smashed.Clear();warm=0;ResetFlight();}
        public void NewSession(){Cancel();armed.Clear();sampled=false;warm=0;blockedUntil=Time.time+1;Feedback=null;feedbackUntil=0;LastDistance=LastAirtime=LastSpeed=LastJumpAward=0;}
        void Recovered(){if(AttemptActive)Message("Attempt cancelled by recovery / retry from pause menu",4);Cancel();armed.Clear();sampled=false;warm=0;blockedUntil=Time.time+2;}
        void ResetFlight(){flying=false;invalid=touchedDown=false;air=stable=0;jumpSite=null;rejected=null;}
        // The jump the player is going for: the campaign event's, or Free Roam's timed attempt.
        ActivitySite TargetJump=>CampaignRun.Active?.Kind==CampaignEventKind.Jump?CampaignRun.Site(race):AttemptActive&&Selected&&Selected.kind==ActivitySite.Kind.Jump?Selected:null;
        // 0.91 Part C: an ordinary jump counts if the vehicle survives it, so a hard or glancing contact no longer rejects it;
        // the two summit flights keep their own rules (a wall or steep bank in the flight rejects them).
        public void SolidContact(Vector3 normal,float relativeSpeed){if(!flying)return;contactImpact=Mathf.Max(contactImpact,relativeSpeed);minimumContactUp=Mathf.Min(minimumContactUp,normal.y);if(normal.y<.45f&&Summit(jumpSite))Reject("Hit a wall or a steep bank");}
        ActivitySite SiteAt(Vector3 at)=>Sites.Where(s=>s.kind==ActivitySite.Kind.Jump&&Vector3.Distance(at,s.transform.position)<s.radius&&Vector3.Dot(car.Body.linearVelocity,s.forward.normalized)>2).OrderBy(s=>Vector3.SqrMagnitude(at-s.transform.position)).FirstOrDefault();
        void Smash(BreakableProp prop,ArcadeVehicle source)
        {
            if(source!=car||!(race.FreeRoam||CampaignRun.Active?.Kind==CampaignEventKind.Smash)||!AttemptActive||Selected.kind!=ActivitySite.Kind.Smash||race.Flow.State!=RaceFlow.Stage.Racing||Time.time<blockedUntil||warm<.5f)return;
            if(Selected.props==null||!Selected.props.Contains(prop)||!smashed.Add(prop))return;
            Message(Selected.title+" / "+smashed.Count+" distinct props",2);
            if(smashed.Count>=Selected.gold)FinishSmash();
        }
        void FinishSmash(){var value=smashed.Count;AttemptActive=false;Award(Selected,value,"props");}
        void FixedUpdate()
        {
            if(DeveloperLocationHud.Inspecting){NewSession();return;}
            if(!race||!race.Flow||race.Flow.State!=RaceFlow.Stage.Racing)return;
            if(!race.FreeRoam&&race.Progress.Finished)return;
            var p=car.Body.position;float dt=Time.fixedDeltaTime;bool grounded=car.GroundedWheels>=2;
            if(!sampled){previous=p;sampled=true;return;}
            float step=Vector3.Distance(p,previous);bool discontinuity=step>Mathf.Max(3,car.Body.linearVelocity.magnitude*dt*2+.3f);
            if(discontinuity){Recovered();previous=p;return;}
            if(Time.time<blockedUntil){previous=p;return;}
            if(AttemptActive&&Time.time>=deadline){if(Selected.kind==ActivitySite.Kind.Smash)FinishSmash();else{AttemptActive=false;Message("Jump attempt expired / retry from pause menu",4);}}
            if(grounded&&!flying){bool summitRunup=Sites.Any(s=>Summit(s)&&Vector3.Distance(p,s.transform.position)<s.radius&&Vector3.Dot(car.Body.linearVelocity,s.forward)>2);warm=car.transform.up.y>(summitRunup?.65f:.8f)?warm+dt:0;}
            if(!grounded&&!flying&&warm>=.5f&&Vector3.ProjectOnPlane(car.Body.linearVelocity,Vector3.up).magnitude>4)
            {
                flying=true;invalid=touchedDown=false;rejected=null;takeoff=previous;air=stable=0;impactSpeed=contactImpact=0;minimumContactUp=1;
                jumpSite=SiteAt(takeoff);
            }
            if(flying)
            {
                if(configuration.WipedOut)Reject("Wiped out on landing");
                // 0.91 Part C: a hop off the ramp's lip (under 0.25 s in the air) that takes off again before settling is not
                // the jump: the real flight starts here (0.90 measured the hop, and the jump after it was never scored).
                if(!grounded&&touchedDown&&air<.25f){takeoff=previous;air=0;touchedDown=false;impactSpeed=0;var site=SiteAt(takeoff);if(site)jumpSite=site;}
                if(!grounded){if(!touchedDown)air+=dt;stable=0;impactSpeed=Mathf.Max(impactSpeed,-car.Body.linearVelocity.y);}
                else
                {
                    // Freeze measurement at first touchdown. Subsequent settling or
                    // suspension bounces must not extend the jump's distance/airtime.
                    if(!touchedDown){landing=p;touchedDown=true;}stable+=dt;
                    // 0.91 Part C: a jump counts if the vehicle survives it: down on its wheels, upright, not wiped out, not in
                    // the water. No hard-landing limit (0.90 rejected a vertical impact over 18 m/s, so a jump taken flat out
                    // never scored). The two summit flights also need sustained level support (below).
                    if(car.WaterImmersion>.05f)Reject("Landed in the water");
                    if(configuration.WipedOut)Reject("Wiped out on landing");
                    if(car.transform.up.y<.65f)Reject(car.transform.up.y<0?"Landed upside down":"Landed on your side");
                    if(stable>=(Summit(jumpSite)?.75f:.3f))
                    {
                        float distance=Vector3.ProjectOnPlane(landing-takeoff,Vector3.up).magnitude;
                        if(Summit(jumpSite)&&!AlignedWithSupport())Reject("Not level on the landing");
                        LastJumpDiagnostic=$"site={jumpSite?.id} invalid={invalid} why={rejected} verticalImpact={impactSpeed:F2} contactImpact={contactImpact:F2} contactUp={minimumContactUp:F2} up={car.transform.up.y:F2} wiped={configuration.WipedOut} air={air:F2} distance={distance:F2}";
                        bool tooShort=air<.25f||distance<3||(jumpSite&&Vector3.Dot(landing-takeoff,jumpSite.forward.normalized)<3);
                        bool hop=air<.25f&&!invalid;
                        bool tooLong=air>=(Summit(jumpSite)?20:12)||distance>=(Summit(jumpSite)?2000:250);
                        // The two summit giant flights are meant to go as far as possible (0.71: no 250 m cap for them).
                        if(!invalid&&!tooShort&&!tooLong)
                        {
                            LastDistance=distance;LastAirtime=air;Message($"CLEAN JUMP / {DisplayUnits.Jump(distance)} / {air:0.00} s / {Mathf.RoundToInt(distance*10+air*100)} pts",4);
                            if(jumpSite){AttemptActive=false;Award(jumpSite,distance,"m");}
                        }
                        else if(jumpSite&&!hop){AttemptActive=false;Miss(jumpSite,rejected??(tooShort?"Too short to count":"Left the course"));Message($"{jumpSite.title} / NOT SCORED: {rejected??(tooShort?"Too short to count":"Left the course")}",5);}
                        // a real flight beside the jump being attempted, but not from its ramp
                        else if(!tooShort&&TargetJump is ActivitySite target&&Vector3.Distance(takeoff,target.transform.position)<target.radius*3&&Vector3.Dot(landing-takeoff,target.forward.normalized)>3){Miss(target,"took off outside the marked area");Message($"{target.title} / NOT SCORED: Took off outside the marked area",5);}
                        ResetFlight();warm=hop?.5f:0; // a hop is no jump: the next takeoff still counts
                    }
                }
            }
            // A bump or valid airborne crossing must not disable a speed camera.
            // Reset warmup, swept position/velocity agreement, direction and rearming
            // already reject discontinuities and repeated parked crossings.
            if(!configuration.WipedOut)
            foreach(var site in Sites.Where(s=>s.kind==ActivitySite.Kind.Speed))
            {
                var f=site.forward.normalized;float a=Vector3.Dot(previous-site.transform.position,f),b=Vector3.Dot(p-site.transform.position,f);
                if(Mathf.Abs(b)>25)armed[site]=true;
                bool crossing=a<0&&b>=0 || site.bothDirections&&a>0&&b<=0;
                if(!crossing||!armed.TryGetValue(site,out bool ready)||!ready)continue;
                float t=a/(a-b);var cross=Vector3.Lerp(previous,p,t);var lateral=Vector3.ProjectOnPlane(cross-site.transform.position,f);
                float speed=Vector3.ProjectOnPlane(car.Body.linearVelocity,Vector3.up).magnitude;
                if(lateral.magnitude>site.radius||speed<3||Mathf.Abs(Vector3.Dot(car.Body.linearVelocity.normalized,f))<.65f||Mathf.Abs(step/dt-speed)>Mathf.Max(4,speed*.35f))continue;
                armed[site]=false;oppositeAttempt=b<a;LastSpeed=speed;Award(site,speed,"m/s");
            }
            previous=p;
        }
        void Award(ActivitySite site,float value,string units)
        {
            if(DeveloperLocationHud.Inspecting)return;
            if(!float.IsFinite(value)||value<=0)return;int medal=site.Medal(value,configuration.profileId);var best=PersonalBest(site);
            // 0.89: a campaign event never writes activity records or personal bests; its result goes to the campaign save.
            if(CampaignRun.Active!=null){LastAwardSite=site;if(site.kind==ActivitySite.Kind.Jump)LastJumpAward=value;if(site.kind==ActivitySite.Kind.Smash)LastSmashScore=value;Awards++;
                var e=CampaignRun.Active;string award=e.Site==site.id&&e.Kind!=CampaignEventKind.Race&&e.Kind!=CampaignEventKind.TimeTrial?" / "+new[]{"NO MEDAL","BRONZE","SILVER","GOLD"}[Campaign.MedalFor(e,value)]:"";
                Message(site.title+" / "+(site.kind==ActivitySite.Kind.Jump?"SCORED: ":"")+Measurement(site,value)+award,6);return;}
            // 0.90 Part D: split-screen results are not written to the activity records either.
            if(SplitScreen.Active){Awards++;Message(site.title+" / "+Measurement(site,value),6);return;}
            bool improved=best==null||value>best.value;
            if(site.kind!=ActivitySite.Kind.Smash)Records.Add(new ActivityRecords.Entry{id=Guid.NewGuid().ToString("N"),key=Key(site),site=site.id,vehicle=configuration.profileId,date=DateTime.UtcNow.ToString("o"),value=value,airtime=site.kind==ActivitySite.Kind.Jump?LastAirtime:0,medal=medal});
            if(site.kind==ActivitySite.Kind.Jump)LastJumpAward=value;
            var own=Results.results.FirstOrDefault(b=>b.key==Key(site));if(own==null){own=new Best{key=Key(site)};Results.results.Add(own);}own.value=Mathf.Max(own.value,value);own.medal=Mathf.Max(own.medal,medal);Awards++;
            string measurement=Measurement(site,value)+" / PB "+Measurement(site,Mathf.Max(own.value,best?.value??0));
            Message(site.title+" / "+measurement+"\n"+(improved?"NEW BEST":"personal best retained")+(site.kind==ActivitySite.Kind.Jump?$" / {LastAirtime:0.00}s / {Mathf.RoundToInt(value*10+LastAirtime*100)} pts":""),6);LastMedal=medal;ResultValue=Measurement(site,value);
            try{AtomicSave.Write(path,JsonUtility.ToJson(Results,true));}catch(Exception e){Message("Activity result could not be saved: "+e.Message,6);}
        }
        void Message(string value,float seconds){Feedback=value;feedbackUntil=Time.time+seconds;LastMedal=-1;}
        void OnDestroy(){BreakableProp.BrokenByVehicle-=Smash;if(car&&car.TryGetComponent<VehicleRespawn>(out var respawn))respawn.Respawned-=Recovered;}
    }
    public sealed class ActivityLandingContact:MonoBehaviour {public ArcadeActivities activities;void OnCollisionEnter(Collision c){if(!activities||activities.PlayerObject!=gameObject)return;foreach(var contact in c.contacts)activities.SolidContact(contact.normal,Mathf.Abs(Vector3.Dot(c.relativeVelocity,contact.normal)));}}
}
