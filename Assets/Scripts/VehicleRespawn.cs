using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Racer
{
    [RequireComponent(typeof(ArcadeVehicle),typeof(VehicleInput))]
    public sealed class VehicleRespawn : MonoBehaviour
    {
        [Tooltip("Used only by explicit full race restart.")]
        public Transform spawnPoint;
        public float fallResetHeight=-15;
        public event Action Respawned;
        public bool Pending { get; private set; }
        public string LastRecovery { get; private set; }
        struct SafeSample {public WoodlandRoute branch;public float station,side,time;public int lap;}
        // Development diagnostics only; never displayed in the racing HUD.
        public string RecoveryDiagnostic { get; private set; }
        bool PlayerRecovery => race && race.vehicle==vehicle;
        readonly List<SafeSample> history=new();
        ArcadeVehicle vehicle;
        VehicleInput input;
        BoxCollider box;
        Bounds clearance;
        RaceDirector race;
        Vector3 initialPosition;
        Quaternion initialRotation;
        float nextHistory, nextAttempt,lastRecoveryAt=-100;int recoveryEscalation;bool phased;static int phaseCounter;
        readonly Collider[] overlaps=new Collider[64];
        void Awake()
        {
            vehicle=GetComponent<ArcadeVehicle>(); input=GetComponent<VehicleInput>(); box=GetComponent<BoxCollider>();
            initialPosition=transform.position; initialRotation=Quaternion.Euler(0,transform.eulerAngles.y,0);
        }
        void FixedUpdate()
        {
            if(!race) race=FindAnyObjectByType<RaceDirector>();
            if(race && race.Flow.State!=RaceFlow.Stage.Racing) return;
            // 0.68 failsafe (all scenes, player and AI): a vehicle that drops below the
            // world is placed back on the nearest course point at once, never left falling.
            if(transform.position.y<fallResetHeight){if(Time.time>=nextFailsafe){nextFailsafe=Time.time+.25f;RecoverNearest(true);}return;}
            lastAbove=vehicle.Body.position;haveAbove=true;
            if(!Pending)RecordSafePosition();
            // AI owns its retry/cooldown bookkeeping. A second automatic respawn
            // here could otherwise move it without clearing its stuck counters.
            if(TryGetComponent<RoadDriver>(out var driver)&&driver.enabled)return;
            if(input.enabled && input.ConsumeReset()){if(!RunUp())ResetVehicle();}
            else if(Pending && Time.time>=nextAttempt) TryRecoverLocal();
        }
        float nextFailsafe,aiFailingSince=-1;Vector3 lastAbove;bool haveAbove;
        float safeStation,safeSide,trackingStation;
        bool tracking;
        WoodlandRoute trackingBranch;
        bool anchored;
        Vector3 observed;
        WoodlandRoute safeBranch;
        float stableSince=-1;
        float untrackedTravel,stableTravel;
        Vector3 stableObserved;
        bool awaitingLanding;
        WoodlandRoute stableBranch;
        JumpRecoveryExclusion[] jumpExclusions;
        Collider[] legacyLaunchSurfaces;
        // 0.82 Part C: the legacy launch colliders, found once per scene for every vehicle (ordinal name tests) instead of
        // each vehicle scanning every collider with culture-aware tests on its first racing step (a 0.8 s hitch at GO).
        // RaceFlow warms it behind the loading screen.
        static Collider[] sharedLaunch; static UnityEngine.SceneManagement.Scene sharedLaunchScene;
        public static Collider[] LaunchSurfaces(UnityEngine.SceneManagement.Scene scene)
        {
            if (sharedLaunch == null || sharedLaunchScene != scene || !scene.isLoaded)
            {
                sharedLaunch = FindObjectsByType<Collider>().Where(c => { var n = c.name; return n.StartsWith("Takeoff -", StringComparison.Ordinal) || n.StartsWith("Gully supported ramp", StringComparison.Ordinal) || n == "Reverse supported roadworks transition"; }).ToArray();
                sharedLaunchScene = scene;
            }
            return sharedLaunch;
        }
        ForestLayout forestLayout;
        float[] forestLipStations;
        bool UnsafeJump(Vector3 p)
        {
            // Consume route flight metadata, including newly authored shortcuts;
            // a curved Ground_ takeoff must not depend on a legacy object name.
            foreach(var route in race.Branches??Array.Empty<WoodlandRoute>()){
                var guide=route.GetComponent<ReverseShortcutGuidance>();
                if(!guide||guide.takeoff<0)continue;
                float s=route.Project(p,out float lateral);var floor=route.At(s,out _);
                if(lateral<route.halfWidth+1&&Mathf.Abs(p.y-floor.y)<3
                    &&s>=guide.takeoff-32&&s<=guide.landing+6)return true;
            }
            jumpExclusions ??= FindObjectsByType<JumpRecoveryExclusion>();
            foreach(var zone in jumpExclusions)if(zone&&zone.Contains(p))return true;
            legacyLaunchSurfaces ??= LaunchSurfaces(gameObject.scene);
            foreach(var ramp in legacyLaunchSurfaces){
                if(!ramp||!ramp.enabled||!ramp.gameObject.activeInHierarchy)continue;
                var branch=race.Racers.FirstOrDefault(r=>r.Car==vehicle)?.Branch.Route;
                float station=branch?branch.Project(p,out _):race.road.Project(p,out _);
                var route=branch?branch.At(station,out var forward):race.road.At(station,out forward);
                forward=Vector3.ProjectOnPlane(forward,Vector3.up).normalized;
                // A shared collider can include a flat elevated deck or landing
                // runout. Classify the actual face, never its whole world AABB.
                if(LegacyLaunchAt(ramp,p,forward)||LegacyLaunchAt(ramp,p-forward*vehicle.wheelbase*.5f,forward))return true;
                float width=branch?branch.halfWidth:race.road.HalfWidth(station);
                if(Vector3.ProjectOnPlane(p-route,Vector3.up).magnitude>width+1||Mathf.Abs(p.y-route.y)>3)continue;
                float side=Vector3.Dot(p-route,Vector3.Cross(Vector3.up,forward));
                // Keep sufficient run-up for a real launch on this driving line.
                for(float d=3;d<=(PlayerRecovery?22:35);d+=4){
                    var ahead=branch?branch.At(station+d,out var f):race.road.At(station+d,out f);
                    f=Vector3.ProjectOnPlane(f,Vector3.up).normalized;
                    ahead+=Vector3.Cross(Vector3.up,f)*side;
                    if(LegacyLaunchAt(ramp,ahead+Vector3.up*.5f,f))return true;
                }
            }
            var flights=race.GetComponent<MountainFlights>();
            if(flights)foreach(var flight in flights.flights){
                var axis=Vector3.ProjectOnPlane(flight.forward,Vector3.up).normalized;
                var q=p-flight.start;float along=Vector3.Dot(q,axis);
                float lip=Vector3.Dot(flight.lip-flight.start,axis);
                if(along>=-(PlayerRecovery?22:60)&&along<=lip+3&&Mathf.Abs(Vector3.Dot(q,Vector3.Cross(Vector3.up,axis)))<30&&p.y>=Mathf.Min(flight.start.y,flight.lip.y)-6&&p.y<=Mathf.Max(flight.start.y,flight.lip.y)+6)return true;
            }
            // Forest jump windows also include their long landing runouts. Find
            // each authored drop, rather than excluding that whole post-jump road.
            if(!flights&&race.Forest&&(!PlayerRecovery||race.Racers.FirstOrDefault(r=>r.Car==vehicle)?.Branch.Route==null)){
                if(!forestLayout)forestLayout=FindAnyObjectByType<ForestLayout>();
                if(forestLayout){
                    if(forestLipStations==null){
                        forestLipStations=new float[forestLayout.jumpStarts.Length];
                        for(int i=0;i<forestLipStations.Length;i++){
                            float steepest=0,lip=forestLayout.jumpStarts[i];
                            for(float s=forestLayout.jumpStarts[i];s<forestLayout.jumpEnds[i];s+=1){race.road.At(s,out var f);if(f.y<steepest){steepest=f.y;lip=s;}}
                            forestLipStations[i]=lip;
                        }
                    }
                    float station=race.road.Project(p,out float distance);
                    if(distance<race.road.HalfWidth(station)+1)for(int i=0;i<forestLipStations.Length;i++)
                        if(station>=forestLayout.jumpStarts[i]-(PlayerRecovery?22:35)&&station<=forestLipStations[i]+2)return true;
                }
            }
            return false;
        }
        static bool LegacyLaunchAt(Collider surface,Vector3 p,Vector3 forward)
        {
            if(!surface.Raycast(new Ray(p+Vector3.up*3,Vector3.down),out var hit,6))return false;
            return hit.normal.y<.9f||Vector3.Dot(hit.normal,forward)<-.025f;
        }
        RaceRoad roamRoad;WoodlandRoute roamBranch;float roamStation,roamDirection=1;bool roamValid;
        struct RoamSample {public RaceRoad road;public WoodlandRoute branch;public float station,direction;}
        readonly List<RoamSample> roamHistory=new();
        public float SafeStation => safeStation;
        public void SeedCoursePosition(Vector3 position)
        {
            if(!race)race=FindAnyObjectByType<RaceDirector>();if(!race||!race.road)return;
            // A gate/branch exit is navigation evidence, not proof of a safe landing.
            // Keep the earned anchor until actual grounded driving validates its replacement.
            trackingStation=race.road.Project(position,out _);trackingBranch=null;tracking=true;observed=position;
            untrackedTravel=stableTravel=0;
            stableSince=-1;
        }
        // RoadDriver calls this every tick for rivals (whose FixedUpdate here is disabled), so it also keeps the last
        // position above the world for the fall-through failsafe (0.72).
        public void RecordSafePosition(){if(vehicle&&vehicle.Body.position.y>=fallResetHeight){lastAbove=vehicle.Body.position;haveAbove=true;}RecordSafePosition(Time.time);}
        void RecordSafePosition(float now)
        {
            if(!race)race=FindAnyObjectByType<RaceDirector>();
            if(!race || !race.road || now<nextHistory)return;
            // 0.82 Part C: AI and traffic sample on their own phase within the 0.1 s (in turn), so the cars no longer all do
            // this search in the same physics step (a 20-30 ms frame every 0.1 s in Free Roam traffic); the player as before.
            nextHistory=now+.1f;
            if(!phased){phased=true;if(race.vehicle!=vehicle)nextHistory+=(phaseCounter++%8)*.0125f;}
            if(race.FreeRoam){
                if(vehicle.GroundedWheels<2){awaitingLanding=true;stableSince=-1;return;}
                if(transform.up.y<.65f||vehicle.Body.angularVelocity.magnitude>2.5f||UnsafeJump(vehicle.Body.position)){stableSince=-1;return;}
                if(stableSince<0){stableSince=now;stableTravel=0;stableObserved=vehicle.Body.position;return;}
                stableTravel+=Vector3.ProjectOnPlane(vehicle.Body.position-stableObserved,Vector3.up).magnitude;stableObserved=vehicle.Body.position;
                if(now-stableSince>=.35f&&stableTravel>=1)RecordRoaming();return;
            }
            var state=race.Racers.FirstOrDefault(r=>r.Car==vehicle);
            var branch=state?.Branch.Route;
            var p=vehicle.Body.position;
            float travelled=Vector3.Distance(observed,p);
            bool continuous=!tracking||travelled<=90;observed=p;
            if(!continuous){stableSince=-1;return;}
            // A ballistic arc can be much farther than 75m from its last close
            // road projection. Grow the search by observed travel until the car
            // reaches the route again; never advance the SAFE anchor in the air.
            untrackedTravel+=travelled;
            float lateral;
            float station=branch?branch.Project(p,out lateral):tracking&&trackingBranch==branch?race.road.ProjectNear(p,trackingStation,Mathf.Max(75,untrackedTravel*1.5f+15),out lateral):race.road.Project(p,out lateral);
            if(lateral<12)untrackedTravel=0;
            trackingStation=station;trackingBranch=branch;tracking=true;
            if(vehicle.GroundedWheels<2){awaitingLanding=true;stableSince=-1;return;}
            if(transform.up.y<.65f||vehicle.Body.angularVelocity.magnitude>2.5f||UnsafeJump(p)
                ||(TryGetComponent<VehicleConfiguration>(out var configuration)&&configuration.WipedOut)){stableSince=-1;return;}
            Vector3 support=branch?branch.At(station,out var direction):race.road.At(station,out direction);
            // RaceRoad's 3D projection includes chassis height on climbs. Remove that
            // forward offset: recovery must never award a station ahead of the occupied footprint.
            float ahead=Vector3.Dot(Vector3.ProjectOnPlane(support-p,Vector3.up),Vector3.ProjectOnPlane(direction,Vector3.up).normalized);
            if(ahead>0){station-=ahead/Mathf.Max(.5f,Vector3.ProjectOnPlane(direction,Vector3.up).magnitude);support=branch?branch.At(station,out direction):race.road.At(station,out direction);}
            float width=branch?branch.halfWidth:race.road.HalfWidth(station);
            var normal=Vector3.Cross(direction,Vector3.Cross(Vector3.up,direction)).normalized;
            if(Vector3.ProjectOnPlane(p-support,Vector3.up).magnitude>width-box.size.x*.5f-.25f || Mathf.Abs(p.y-support.y)>2.5f||Mathf.Abs(Vector3.Dot(vehicle.Body.linearVelocity,normal))>2.5f
                ||Vector3.Dot(transform.forward,direction)<.35f||Vector3.Dot(vehicle.Body.linearVelocity,direction)<.5f){stableSince=-1;return;}
            // A supported patch of terrain below a ribbon is not a completed
            // landing. Validate the entire suspension footprint at route height.
            var forward=Vector3.ProjectOnPlane(direction,Vector3.up).normalized;
            float side=Vector3.Dot(p-support,Vector3.Cross(Vector3.up,forward));
            var occupied=support+Vector3.Cross(Vector3.up,forward)*side;
            if(!Supported(occupied,forward,out var supported,out var rotation)||Mathf.Abs(supported.y-p.y)>1.2f
                ||Vector3.Dot(transform.up,rotation*Vector3.up)<.92f){stableSince=-1;return;}
            MeasureClearance();
            if(!Clear(supported,rotation)){stableSince=-1;return;}
            if(stableSince<0||stableBranch!=branch){stableSince=now;stableBranch=branch;stableObserved=p;stableTravel=0;return;}
            stableTravel+=Mathf.Max(0,Vector3.Dot(p-stableObserved,direction));stableObserved=p;
            if(now-stableSince<(PlayerRecovery?.2f:.35f)||stableTravel<1)return;
            safeStation=station;safeSide=side;safeBranch=branch;anchored=true;observed=p;
            // Once a landing is earned, an obstructed newer sample must not
            // fall back across that successfully completed jump.
            if(awaitingLanding){history.Clear();awaitingLanding=false;}
            if(history.Count==120)history.RemoveAt(0);
            history.Add(new SafeSample{branch=branch,station=station,side=side,time=now,lap=state?.Progress.CompletedLaps??0});
        }
        public void RestartAtStart()
        {
            Pending=false;history.Clear();nextHistory=nextAttempt=0;anchored=false;tracking=false;safeBranch=null;roamValid=false;stableSince=-1;untrackedTravel=stableTravel=0;
            roamHistory.Clear();awaitingLanding=false;
            var p=spawnPoint?spawnPoint.position:initialPosition;var r=spawnPoint?Quaternion.Euler(0,spawnPoint.eulerAngles.y,0):initialRotation;
            // BUG-009: seat the start on its supporting surface (a spawn marker below the pavement fell through the world).
            if(Supported(p,r*Vector3.forward,out var seated,out _))p=seated;
            Place(p,r);
        }
        public void CancelRecovery(){Pending=false;history.Clear();roamHistory.Clear();awaitingLanding=false;anchored=false;tracking=false;safeBranch=null;roamValid=false;stableSince=-1;untrackedTravel=stableTravel=0;}
        public bool TryFastTravel(Vector3 candidate,Quaternion facing)
        {
            if(!race)race=FindAnyObjectByType<RaceDirector>();
            if(!race||!race.FreeRoam)return false;
            MeasureClearance();
            if(!Supported(candidate,facing*Vector3.forward,out var position,out var rotation)||!Clear(position,rotation))return false;
            CancelRecovery();Place(position,rotation);RecordRoaming();Respawned?.Invoke();return true;
        }
        // 0.91 Part C: in a campaign jump event the player's reset puts them back at the run-up to jump again.
        bool RunUp()
        {
            if(!race)race=FindAnyObjectByType<RaceDirector>();
            if(!race||race.vehicle!=vehicle||!CampaignRun.RunUpStart(race,out var position,out var rotation))return false;
            CancelRecovery();Place(position,rotation);SeedCoursePosition(position);LastRecovery="Back to the run-up";Respawned?.Invoke();
            FindAnyObjectByType<ChaseCamera>()?.Snap();return true;
        }
        public void ResetVehicle()
        {
            if(Pending)return;
            Pending=true;nextAttempt=Time.time+.8f;LastRecovery="Recovering";
        }
        public bool TryRecoverLocal(bool preferRoad=false)
        {
            if(!race)race=FindAnyObjectByType<RaceDirector>();
            if(!race || !race.road)return false;
            MeasureClearance();
            // Free Roam keeps its recent-road behaviour, but can no longer wait forever.
            if(race.FreeRoam)return roamValid&&RecoverRoaming()||RecoverNearest(false);
            // 0.68 player rule (Dan): the nearest usable track point, facing the race direction.
            // 0.72: racing AI uses the same rule, so a rival that fails is restored where it failed.
            var state=race.Racers.FirstOrDefault(r=>r.Car==vehicle);
            if(PlayerRecovery||state!=null)return RecoverNearest(false);
            var branch=anchored?safeBranch:null;
            var from=vehicle.Body.position;
            // Earned branch station or last supported course sample, never nearest arbitrary ground.
            float at=anchored?safeStation:race.road.Project(spawnPoint?spawnPoint.position:initialPosition,out _);
            var candidates=new List<SafeSample>();
            var rejected=new List<string>();
            float earnedBefore=state?.Branch.Route?state.Branch.Earned:state?.VerifiedRoad??at;
            if(Time.time-lastRecoveryAt>35)recoveryEscalation=0;
            // Only occupied, established samples. Backward offsets can cross a
            // landing edge or select the ramp that the player just cleared.
            int anchorIndex=history.FindLastIndex(h=>h.branch==branch&&Mathf.Abs(h.station-at)<.01f);
            candidates.Add(anchorIndex>=0?history[anchorIndex]:new SafeSample{branch=branch,station=at,side=anchored?safeSide:0,time=Time.time,lap=state?.Progress.CompletedLaps??0});
            // Last resort: previously occupied supported course samples, still checked
            // for current traffic/obstructions. Never switch to a different branch.
            for(int i=history.Count-1;i>=0;i--)
            {
                var sample=history[i];if(sample.branch!=branch)continue;
                float back=branch?at-sample.station:PlayerRecovery?Mathf.Repeat(at-sample.station+race.road.Length*.5f,race.road.Length)-race.road.Length*.5f:Mathf.Repeat(at-sample.station,race.road.Length);
                if(back>=0&&!candidates.Any(s=>Mathf.Abs(s.station-sample.station)<(PlayerRecovery?.01f:1)))candidates.Add(sample);
            }
            foreach(var sample in candidates)
            {
                float s=sample.station;
                if(PlayerRecovery&&sample.lap!=(state?.Progress.CompletedLaps??0)){rejected.Add($"{s:F2}: different lap");continue;}
                var point=branch?branch.At(s,out var f):race.road.At(s,out f);
                if(UnsafeJump(point)){rejected.Add($"{s:F2}: launch exclusion");continue;}
                var forward=Vector3.ProjectOnPlane(f,Vector3.up).normalized;
                float width=branch?branch.halfWidth:race.road.HalfWidth(s);
                float sideLimit=Mathf.Max(0,width-clearance.size.x*.5f-.5f);
                float alternate=Mathf.Min(3,sideLimit)*(recoveryEscalation%2==0?1:-1);
                var sides=preferRoad&&!branch?new[]{alternate,-alternate,sample.side,0f}:new[]{Mathf.Clamp(sample.side,-sideLimit,sideLimit),0f,Mathf.Min(1.7f,sideLimit),-Mathf.Min(1.7f,sideLimit),sideLimit,-sideLimit};
                string reason="no usable lateral placement";
                foreach(float side in sides)
                {
                    var candidate=point+Vector3.Cross(Vector3.up,forward)*side;
                    if(UnsafeJump(candidate)){reason="launch or landing exclusion";continue;}
                    if(!Supported(candidate,forward,out var position,out var rotation)){reason="unsupported footprint";continue;}
                    if(!Clear(position,rotation)){reason="obstacle or vehicle clearance";continue;}
                    if(Mathf.Abs(position.y-point.y)>5){reason="support outside route height";continue;}
                    Place(position,rotation);Pending=false;aiFailingSince=-1;
                    lastRecoveryAt=Time.time;if(preferRoad)recoveryEscalation++;
                    safeStation=s;safeSide=side;safeBranch=branch;observed=position;anchored=true;trackingStation=s;trackingBranch=branch;tracking=true;
                    stableSince=-1;
                    // A rejected newer sample must not become a forward destination on a second reset.
                    if(PlayerRecovery)history.RemoveAll(h=>h.time>sample.time);
                    if(state!=null&&state.Branch.Route!=branch){state.Branch.Clear();if(branch)state.Branch.Begin(branch);}
                    state?.Branch.Recovered(position);state?.SampleOrigin(race.Clock);
                    RecoveryDiagnostic=$"earnedBefore={earnedBefore:F2}; route={branch?.title??"main"}; selected={s:F2}; age={Mathf.Max(0,Time.time-sample.time):F2}s; backward={Mathf.Max(0,Vector3.Dot(from-position,forward)):F2}m; displacement={Vector3.Distance(from,position):F2}m; rejected={rejected.Count}; reasons={string.Join(" | ",rejected)}";
                    LastRecovery="Recovered to clear earned course support";Respawned?.Invoke();return true;
                }
                rejected.Add($"{s:F2}: {reason}");
            }
            // AI selection is unchanged; after 3 s of rejected earned samples it takes the
            // same guaranteed nearest-point placement as the player instead of waiting forever.
            if(aiFailingSince<0)aiFailingSince=Time.time;
            if(Time.time-aiFailingSince>=3)return RecoverNearest(false);
            Pending=true;nextAttempt=Time.time+.5f;LastRecovery="Waiting for clear course support";return false;
        }
        // ---------- 0.68 nearest-point recovery (PROJECT_TODO 0.68 Part A) ----------
        // Target = nearest route point to the vehicle on the route it is racing (main, or its branch),
        // searched around tracked progress so a multi-level mountain cannot snap to an unrelated part of
        // the lap. Unusable spots step outward 2 m at a time in both directions (forward first on a tie);
        // stepping backward never crosses a ramp/flight exclusion it reached from outside, so a missed
        // jump restores at/after its landing, not before the ramp. Always ends with a placement.
        sealed class Track
        {
            public RaceRoad road;public WoodlandRoute branch;public bool loop;public float length;
            public Vector3 At(float s,out Vector3 f)=>branch?branch.At(s,out f):road.At(s,out f);
            public float Width(float s)=>branch?branch.halfWidth:road.HalfWidth(s);
        }
        Track MainTrack(RaceRoad road){road.Initialize();return new Track{road=road,loop=!road.openHighway,length=road.Length};}
        static Track BranchTrack(WoodlandRoute b)=>new(){branch=b,loop=false,length=b.Length};
        static float Cost(Vector3 a,Vector3 b){var d=a-b;float dy=Mathf.Min(Mathf.Abs(d.y),40);return Mathf.Sqrt(d.x*d.x+d.z*d.z+dy*dy);}
        float Nearest(Track t,Vector3 from,float centre,float window)
        {
            float best=float.MaxValue,result=centre;
            for(float o=-window;o<=window;o+=1){float s=centre+o;if(!t.loop&&(s<0||s>t.length))continue;float c=Cost(t.At(s,out _),from);if(c<best){best=c;result=s;}}
            for(float o=-1;o<=1;o+=.1f){float s=result+o;if(!t.loop&&(s<0||s>t.length))continue;float c=Cost(t.At(s,out _),from);if(c<best){best=c;result=s;}}
            return t.loop?Mathf.Repeat(result,t.length):result;
        }
        bool RecoverNearest(bool fell)
        {
            if(!race)race=FindAnyObjectByType<RaceDirector>();
            MeasureClearance();
            var from=fell||vehicle.Body.position.y<fallResetHeight?(haveAbove?lastAbove:(spawnPoint?spawnPoint.position:initialPosition)):vehicle.Body.position;
            if(!race||!race.road)return PlaceStart(from,"no course");
            var state=race.FreeRoam?null:race.Racers.FirstOrDefault(r=>r.Car==vehicle);
            var tracks=new List<(Track t,float s0,float lo,float hi,float facing)>();
            if(race.FreeRoam)
            {
                // Nearest of every drivable road/trail in the scene; the current roaming direction is kept.
                var options=new List<(Track t,float s0,float cost)>();
                void Add(Track t){if(t==null||t.length<=0)return;float s=t.branch?t.branch.Project(from,out _):t.road.Project(from,out _);s=Nearest(t,from,s,25);options.Add((t,s,Cost(t.At(s,out _),from)));}
                Add(MainTrack(race.road));if(race.ambientRoad)Add(MainTrack(race.ambientRoad));
                var exploration=race.GetComponent<ExplorationCollection>();if(exploration)foreach(var r in exploration.routes)if(r)Add(MainTrack(r));
                foreach(var b in race.Branches??Array.Empty<WoodlandRoute>())if(b&&b.gameObject.activeInHierarchy)Add(BranchTrack(b));
                foreach(var o in options.OrderBy(o=>o.cost)){bool same=o.t.branch?o.t.branch==roamBranch:o.t.road==roamRoad;tracks.Add((o.t,o.s0,0,o.t.length,same?roamDirection:1));}
            }
            else
            {
                var branch=state?.Branch.Route;
                // 0.94 Part D: stopped in the Abandoned Cabin Jump's restored brush: onto the clear ground past its far edge
                if(branch&&!fell&&ShortcutUndergrowth.ResetPast(branch,from,out float past)){var bt=BranchTrack(branch);var edge=branch.At(past,out _);if(PlaceAt(bt,past,1,edge,state,past)){LastRecovery="Recovered past the brush";return true;}}
                if(branch)tracks.Add((BranchTrack(branch),Nearest(BranchTrack(branch),from,branch.Project(from,out _),branch.Length),0,branch.Length,1));
                var main=MainTrack(race.road);
                float window=tracking&&!trackingBranch?Mathf.Clamp(Mathf.Max(75,untrackedTravel*1.5f+15),75,main.length*.5f):main.length*.5f;
                float centre=tracking&&!trackingBranch?trackingStation:race.road.Project(from,out _);
                float s0=Nearest(main,from,centre,window);
                // A reset never crosses START/FINISH: it must not award or undo a lap.
                float rel=race.road.Relative(s0,race.Origin);
                tracks.Add((main,s0,s0-rel+.5f,s0+(main.length-rel)-.5f,1));
            }
            foreach(var (t,s0,lo,hi,facing) in tracks)if(SearchTrack(t,s0,lo,hi,facing,from,state))return true;
            return PlaceStart(from,"no usable station on the current route");
        }
        bool SearchTrack(Track t,float s0,float lo,float hi,float facing,Vector3 from,RacerState state)
        {
            const float step=2;
            bool Excluded(float s)=>!race.FreeRoam&&UnsafeJump(t.At(s,out _));
            bool backBlocked=false,backOutside=!Excluded(s0);
            float reach=Mathf.Max(hi-s0,s0-lo);
            for(int k=0;k*step<=reach;k++)
            {
                float sf=s0+k*step;
                if(sf<=hi&&!Excluded(sf)&&PlaceAt(t,sf,facing,from,state,s0))return true;
                if(k==0||backBlocked)continue;
                float sb=s0-k*step;if(sb<lo){backBlocked=true;continue;}
                bool excluded=Excluded(sb);
                if(excluded){if(backOutside)backBlocked=true;continue;}
                backOutside=true;
                if(PlaceAt(t,sb,facing,from,state,s0))return true;
            }
            return false;
        }
        bool PlaceAt(Track t,float s,float facing,Vector3 from,RacerState state,float s0)
        {
            if(t.loop)s=Mathf.Repeat(s,t.length);
            var point=t.At(s,out var f);var forward=Vector3.ProjectOnPlane(f*facing,Vector3.up).normalized;if(forward.sqrMagnitude<.01f)return false;
            var right=Vector3.Cross(Vector3.up,forward);
            float sideLimit=Mathf.Max(0,t.Width(s)-clearance.size.x*.5f-.5f),near=Mathf.Min(1.7f,sideLimit);
            float own=Vector3.Dot(from-point,right);
            var sides=new[]{Mathf.Clamp(own,-near,near),0f,Mathf.Clamp(own,-sideLimit,sideLimit),near,-near,sideLimit,-sideLimit};
            for(int i=0;i<sides.Length;i++)
            {
                float side=sides[i];if(Array.IndexOf(sides,side)<i)continue;
                var candidate=point+right*side;
                if(!race.FreeRoam&&UnsafeJump(candidate))continue;
                if(!Supported(candidate,forward,out var position,out var rotation))continue;
                if(!Clear(position,rotation))continue;
                if(Mathf.Abs(position.y-point.y)>5)continue;
                Place(position,rotation);Pending=false;aiFailingSince=-1;lastRecoveryAt=Time.time;
                history.Clear();awaitingLanding=false;stableSince=-1;untrackedTravel=0;
                if(race.FreeRoam){roamRoad=t.road;roamBranch=t.branch;roamStation=s;roamDirection=facing;roamValid=true;roamHistory.Clear();RecordRoaming();}
                else
                {
                    safeStation=s;safeSide=side;safeBranch=t.branch;observed=position;anchored=true;trackingStation=s;trackingBranch=t.branch;tracking=true;
                    if(state!=null&&state.Branch.Route!=t.branch){state.Branch.Clear();if(t.branch)state.Branch.Begin(t.branch);}
                    state?.Branch.Recovered(position);state?.SampleOrigin(race.Clock);
                }
                float moved=t.loop?Mathf.Repeat(s-s0+t.length*.5f,t.length)-t.length*.5f:s-s0;
                RecoveryDiagnostic=$"route={(t.branch?t.branch.title:t.road==race.road?"main":t.road.name)}; nearest={s0:F2}; selected={s:F2}; along={moved:F2}m; from={from:F2}; to={position:F2}; displacement={Vector3.Distance(from,position):F2}m";
                LastRecovery="Recovered to nearest course point";Respawned?.Invoke();return true;
            }
            return false;
        }
        // Last resort: the start position (always succeeds; checkpoints are not changed).
        bool PlaceStart(Vector3 from,string why)
        {
            var p=spawnPoint?spawnPoint.position:initialPosition;var r=spawnPoint?Quaternion.Euler(0,spawnPoint.eulerAngles.y,0):initialRotation;
            Place(p,r);Pending=false;aiFailingSince=-1;history.Clear();awaitingLanding=false;stableSince=-1;anchored=false;tracking=false;roamValid=false;
            if(race&&!race.FreeRoam){var state=race.Racers.FirstOrDefault(x=>x.Car==vehicle);state?.Branch.Clear();state?.SampleOrigin(race.Clock);}
            RecoveryDiagnostic=$"start position ({why}); from={from:F2}";LastRecovery="Recovered to start position";Respawned?.Invoke();return true;
        }
        // Leaving a race (BUG-009): the stopped vehicle is set on the nearest validated, clear course
        // support before it is locked, so it can never be left over a hole or inside geometry.
        public void PlaceOnNearestGround()
        {
            if(!race)race=FindAnyObjectByType<RaceDirector>();
            if(!race||!race.road||!vehicle||!box)return;
            MeasureClearance();
            var p=vehicle.Body.position;var f=Vector3.ProjectOnPlane(transform.forward,Vector3.up);
            if(p.y>=fallResetHeight&&vehicle.GroundedWheels>=2&&f.sqrMagnitude>.01f&&Supported(p,f.normalized,out var at,out var rot)&&Mathf.Abs(at.y-p.y)<1.2f&&Clear(at,rot))return;
            RecoverNearest(false);
        }
        void RecordRoaming()
        {
            var p=vehicle.Body.position;float best=float.MaxValue,station=0;RaceRoad road=null;WoodlandRoute branch=null;
            if(UnsafeJump(p))return;
            foreach(var candidate in new[]{race.road,race.ambientRoad})if(candidate){float s=candidate.Project(p,out float d);if(d<best){best=d;road=candidate;station=s;}}
            var exploration=race.GetComponent<ExplorationCollection>();
            if(exploration)foreach(var candidate in exploration.routes)if(candidate){float s=candidate.Project(p,out float d);if(d<best){best=d;road=candidate;station=s;}}
            foreach(var candidate in race.Branches){float s=candidate.Project(p,out float d);d=Vector3.Distance(p,candidate.At(s,out _));if(d<best){best=d;branch=candidate;road=null;station=s;}}
            float width=branch?branch.halfWidth:road.HalfWidth(station);var point=branch?branch.At(station,out var f):road.At(station,out f);
            if(Vector3.ProjectOnPlane(p-point,Vector3.up).magnitude>width-box.size.x*.5f || Mathf.Abs(p.y-point.y)>8)return;
            if(!Supported(point,Vector3.ProjectOnPlane(f,Vector3.up).normalized,out var supported,out var rotation)||Mathf.Abs(supported.y-p.y)>1.2f||Vector3.Dot(transform.up,rotation*Vector3.up)<.92f)return;
            MeasureClearance();if(!Clear(supported,rotation))return;
            roamRoad=road;roamBranch=branch;roamStation=station;roamValid=true;
            if(vehicle.Body.linearVelocity.magnitude>2)roamDirection=Vector3.Dot(vehicle.Body.linearVelocity,f)>=0?1:-1;
            if(awaitingLanding){roamHistory.Clear();awaitingLanding=false;}
            if(roamHistory.Count==120)roamHistory.RemoveAt(0);
            roamHistory.Add(new RoamSample{road=road,branch=branch,station=station,direction=roamDirection});
        }
        bool RecoverRoaming()
        {
            foreach(var sample in roamHistory.AsEnumerable().Reverse())
            {
                if(sample.road!=roamRoad||sample.branch!=roamBranch)continue;
                float s=sample.station;
                var p=roamBranch?roamBranch.At(s,out var f):roamRoad.At(s,out f);f=Vector3.ProjectOnPlane(f*sample.direction,Vector3.up).normalized;
                if(UnsafeJump(p))continue;
                float width=roamBranch?roamBranch.halfWidth:roamRoad.HalfWidth(s);float side=Mathf.Max(0,Mathf.Min(2,width-clearance.size.x*.5f-.5f));
                foreach(float offset in new[]{0f,side,-side})if(Supported(p+Vector3.Cross(Vector3.up,f)*offset,f,out var position,out var rotation)&&Clear(position,rotation))
                {Place(position,rotation);roamStation=s;Pending=false;LastRecovery="Recovered to recent safe road/trail";Respawned?.Invoke();return true;}
            }
            Pending=true;nextAttempt=Time.time+.5f;LastRecovery="Waiting for clear recent route";return false;
        }
        bool Supported(Vector3 candidate,Vector3 forward,out Vector3 position,out Quaternion rotation)
        {
            position=candidate; rotation=Quaternion.LookRotation(forward);
            Vector3 normal=Vector3.zero; float top=float.MinValue, low=float.MaxValue;
            var contacts=new List<Vector3>();
            foreach(var local in vehicle.suspensionPoints)
            {
                var origin=candidate+rotation*local+Vector3.up*2;
                // Select the upper supported driving surface, never the terrain under
                // a ramp. Cave ceilings are not driving surfaces; body clearance below
                // them is checked separately with the complete vehicle bounds.
                var supportHits=Physics.RaycastAll(origin,Vector3.down,4,vehicle.groundMask,QueryTriggerInteraction.Ignore)
                    .Where(h=>!h.rigidbody&&h.normal.y>=.82f&&IsCourseSupport(h.collider.name)).OrderBy(h=>Mathf.Abs(h.point.y-candidate.y)).ToArray();
                if(supportHits.Length==0)return false;var hit=supportHits[0];
                foreach(var water in ShallowWater.Active)
                    if(water&&water.gameObject.scene==gameObject.scene&&water.Contains(hit.point)&&water.Surface-hit.point.y>.18f)return false;
                contacts.Add(hit.point);
                normal+=hit.normal; top=Mathf.Max(top,hit.point.y); low=Mathf.Min(low,hit.point.y);
            }
            normal.Normalize();
            if(top-low>vehicle.wheelbase*.65f) return false;
            rotation=Quaternion.LookRotation(Vector3.ProjectOnPlane(forward,normal),normal);
            // Fit the support plane at chassis centre. The highest wheel on a
            // slope inflated the old spawn pose and prevented fresh anchors.
            float centreY=contacts.Average(p=>p.y+(normal.x*(p.x-candidate.x)+normal.z*(p.z-candidate.z))/normal.y);
            if(contacts.Any(p=>Mathf.Abs(Vector3.Dot(p-new Vector3(candidate.x,centreY,candidate.z),normal))>.22f))return false;
            position.y=centreY+Mathf.Max(vehicle.suspensionLength-.12f,box.size.y*.5f-box.center.y+.12f)/normal.y;
            return true;
        }
        static bool IsCourseSupport(string support)=>support.StartsWith("Ground_")||support.StartsWith("Takeoff -")||support.StartsWith("Landing -")||support.StartsWith("Gully supported ramp")||support.StartsWith("Decorative Road pavement")||support=="Reverse supported roadworks transition"||support=="Reverse flush west pavement apron";
        void MeasureClearance()
        {
            clearance=new Bounds(box.center,box.size);
            var waterFeedback=transform.Find("Water feedback");
            // The solver chassis does not include roofs, mirrors or the rider's head.
            // Use local mesh bounds so a rolled vehicle's world AABB cannot inflate
            // its width and incorrectly rule out a narrow trail.
            foreach(var renderer in GetComponentsInChildren<Renderer>())
            {
                if(!renderer.enabled||!(renderer is MeshRenderer||renderer is SkinnedMeshRenderer)||(waterFeedback&&renderer.transform.IsChildOf(waterFeedback)))continue;
                var bounds=renderer.localBounds;
                for(int x=-1;x<=1;x+=2)for(int y=-1;y<=1;y+=2)for(int z=-1;z<=1;z+=2)
                    clearance.Encapsulate(transform.InverseTransformPoint(renderer.transform.TransformPoint(bounds.center+Vector3.Scale(bounds.extents,new Vector3(x,y,z)))));
            }
            // Tire contact with the supporting road is intentional; suspension rays
            // validate that footprint. The solid clearance volume starts at chassis base.
            var min=clearance.min;min.y=box.center.y-box.size.y*.5f;clearance.SetMinMax(min,clearance.max);
        }
        bool Clear(Vector3 position,Quaternion rotation)
        {
            var center=position+rotation*clearance.center;
            var half=Vector3.Scale(clearance.size*.5f,transform.lossyScale)+Vector3.one*.12f;
            int count=Physics.OverlapBoxNonAlloc(center,half,overlaps,rotation,~0,QueryTriggerInteraction.Ignore);
            if(count==overlaps.Length) return false;
            for(int i=0;i<count;i++) if(overlaps[i].attachedRigidbody!=vehicle.Body) return false;
            foreach(var other in FindObjectsByType<ArcadeVehicle>())
            {
                if(other==vehicle || other.gameObject.scene!=gameObject.scene) continue;
                var otherBox=other.GetComponent<BoxCollider>(); if(!otherBox) continue;
                var delta=other.Body.position-position; var velocity=other.Body.linearVelocity;
                float t=velocity.sqrMagnitude>.01f?Mathf.Clamp(-Vector3.Dot(delta,velocity)/velocity.sqrMagnitude,0,.5f):0;
                if((delta+velocity*t).magnitude<half.magnitude+otherBox.size.magnitude*.5f+1) return false;
            }
            return true;
        }
        void Place(Vector3 position,Quaternion rotation)
        {
            var body=vehicle.Body;
            body.position=position; body.rotation=rotation;
            if(!body.isKinematic) body.linearVelocity=body.angularVelocity=Vector3.zero;
            transform.SetPositionAndRotation(position,rotation);
            vehicle.ClearSteering(); body.WakeUp(); Physics.SyncTransforms();
            if(race && race.vehicle==vehicle) FindAnyObjectByType<ChaseCamera>()?.Snap();
        }
    }
}

