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
        struct SafeSample {public WoodlandRoute branch;public float station,side;}
        readonly List<SafeSample> history=new();
        ArcadeVehicle vehicle;
        VehicleInput input;
        BoxCollider box;
        Bounds clearance;
        RaceDirector race;
        Vector3 initialPosition;
        Quaternion initialRotation;
        float nextHistory, nextAttempt,lastRecoveryAt=-100;int recoveryEscalation;
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
            if(!Pending)RecordSafePosition();
            // AI owns its retry/cooldown bookkeeping. A second automatic respawn
            // here could otherwise move it without clearing its stuck counters.
            if(TryGetComponent<RoadDriver>(out var driver)&&driver.enabled)return;
            if(input.enabled && input.ConsumeReset()) ResetVehicle();
            else if(Pending && Time.time>=nextAttempt) TryRecoverLocal();
            else if(!Pending && transform.position.y<fallResetHeight) ResetVehicle();
        }
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
        ForestLayout forestLayout;
        float[] forestLipStations;
        bool UnsafeJump(Vector3 p)
        {
            jumpExclusions ??= FindObjectsByType<JumpRecoveryExclusion>();
            foreach(var zone in jumpExclusions)if(zone&&zone.Contains(p))return true;
            legacyLaunchSurfaces ??= FindObjectsByType<Collider>().Where(c=>c.name.StartsWith("Takeoff -")||c.name.StartsWith("Gully supported ramp")||c.name=="Reverse supported roadworks transition").ToArray();
            foreach(var ramp in legacyLaunchSurfaces){
                if(!ramp||!ramp.enabled||!ramp.gameObject.activeInHierarchy)continue;
                var bounds=ramp.bounds;bounds.Expand(new Vector3(1,4,1));
                if(bounds.Contains(p))return true;
                // Retain an occupied run-up, not a stopped pose at the ramp's foot.
                var routeStation=race.road.Project(p,out _);var ahead=race.road.At(routeStation+35,out _);
                if(bounds.Contains(ahead+Vector3.up*.5f)&&Mathf.Abs(ahead.y-p.y)<8)return true;
            }
            var flights=race.GetComponent<MountainFlights>();
            if(flights)foreach(var flight in flights.flights){
                var axis=Vector3.ProjectOnPlane(flight.forward,Vector3.up).normalized;
                var q=p-flight.start;float along=Vector3.Dot(q,axis);
                float lip=Vector3.Dot(flight.lip-flight.start,axis);
                if(along>=-60&&along<=lip+3&&Mathf.Abs(Vector3.Dot(q,Vector3.Cross(Vector3.up,axis)))<30&&p.y>=Mathf.Min(flight.start.y,flight.lip.y)-6&&p.y<=Mathf.Max(flight.start.y,flight.lip.y)+6)return true;
            }
            // Forest jump windows also include their long landing runouts. Find
            // each authored drop, rather than excluding that whole post-jump road.
            if(!flights&&race.Forest){
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
                        if(station>=forestLayout.jumpStarts[i]-35&&station<=forestLipStations[i]+2)return true;
                }
            }
            return false;
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
        public void RecordSafePosition()=>RecordSafePosition(Time.time);
        void RecordSafePosition(float now)
        {
            if(!race)race=FindAnyObjectByType<RaceDirector>();
            if(!race || !race.road || now<nextHistory)return;
            nextHistory=now+.1f;
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
            if(now-stableSince<.35f||stableTravel<1)return;
            safeStation=station;safeSide=side;safeBranch=branch;anchored=true;observed=p;
            // Once a landing is earned, an obstructed newer sample must not
            // fall back across that successfully completed jump.
            if(awaitingLanding){history.Clear();awaitingLanding=false;}
            if(history.Count==120)history.RemoveAt(0);
            history.Add(new SafeSample{branch=branch,station=station,side=side});
        }
        public void RestartAtStart()
        {
            Pending=false;history.Clear();nextHistory=nextAttempt=0;anchored=false;tracking=false;safeBranch=null;roamValid=false;stableSince=-1;untrackedTravel=stableTravel=0;
            roamHistory.Clear();awaitingLanding=false;
            Place(spawnPoint?spawnPoint.position:initialPosition,spawnPoint?Quaternion.Euler(0,spawnPoint.eulerAngles.y,0):initialRotation);
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
            if(race.FreeRoam&&roamValid)return RecoverRoaming();
            var state=race.Racers.FirstOrDefault(r=>r.Car==vehicle);
            var branch=anchored?safeBranch:null;
            var from=vehicle.Body.position;
            // Earned branch station or last supported course sample, never nearest arbitrary ground.
            float at=anchored?safeStation:race.road.Project(spawnPoint?spawnPoint.position:initialPosition,out _);
            var candidates=new List<SafeSample>();
            if(Time.time-lastRecoveryAt>35)recoveryEscalation=0;
            // Only occupied, established samples. Backward offsets can cross a
            // landing edge or select the ramp that the player just cleared.
            candidates.Add(new SafeSample{branch=branch,station=at,side=anchored?safeSide:0});
            // Last resort: previously occupied supported course samples, still checked
            // for current traffic/obstructions. Never switch to a different branch.
            for(int i=history.Count-1;i>=0;i--)
            {
                var sample=history[i];if(sample.branch!=branch)continue;
                float back=branch?at-sample.station:Mathf.Repeat(at-sample.station,race.road.Length);
                if(back>=0&&!candidates.Any(s=>Mathf.Abs(s.station-sample.station)<1))candidates.Add(sample);
            }
            foreach(var sample in candidates)
            {
                float s=sample.station;
                var point=branch?branch.At(s,out var f):race.road.At(s,out f);
                if(UnsafeJump(point))continue;
                var forward=Vector3.ProjectOnPlane(f,Vector3.up).normalized;
                float width=branch?branch.halfWidth:race.road.HalfWidth(s);
                float sideLimit=Mathf.Max(0,width-clearance.size.x*.5f-.5f);
                float alternate=Mathf.Min(3,sideLimit)*(recoveryEscalation%2==0?1:-1);
                var sides=preferRoad&&!branch?new[]{alternate,-alternate,sample.side,0f}:new[]{Mathf.Clamp(sample.side,-sideLimit,sideLimit),0f,Mathf.Min(1.7f,sideLimit),-Mathf.Min(1.7f,sideLimit),sideLimit,-sideLimit};
                foreach(float side in sides)
                {
                    var candidate=point+Vector3.Cross(Vector3.up,forward)*side;
                    if(!Supported(candidate,forward,out var position,out var rotation) || !Clear(position,rotation))continue;
                    if(Mathf.Abs(position.y-point.y)>5)continue;
                    Place(position,rotation);Pending=false;
                    lastRecoveryAt=Time.time;if(preferRoad)recoveryEscalation++;
                    safeStation=s;safeSide=side;safeBranch=branch;observed=position;anchored=true;trackingStation=s;trackingBranch=branch;tracking=true;
                    stableSince=-1;
                    if(state!=null&&state.Branch.Route!=branch){state.Branch.Clear();if(branch)state.Branch.Begin(branch);}
                    state?.Branch.Recovered(position);state?.SampleOrigin(race.Clock);
                    LastRecovery="Recovered to clear earned course support";Respawned?.Invoke();return true;
                }
            }
            Pending=true;nextAttempt=Time.time+.5f;LastRecovery="Waiting for clear course support";return false;
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
            foreach(var local in vehicle.suspensionPoints)
            {
                var origin=candidate+rotation*local+Vector3.up*14;
                // Select the upper supported driving surface, never the terrain under
                // a ramp. Cave ceilings are not driving surfaces; body clearance below
                // them is checked separately with the complete vehicle bounds.
                var supportHits=Physics.RaycastAll(origin,Vector3.down,30,vehicle.groundMask,QueryTriggerInteraction.Ignore)
                    .Where(h=>!h.rigidbody&&h.normal.y>=.65f&&IsCourseSupport(h.collider.name)).OrderBy(h=>h.distance).ToArray();
                if(supportHits.Length==0)return false;var hit=supportHits[0];
                foreach(var water in ShallowWater.Active)
                    if(water&&water.gameObject.scene==gameObject.scene&&water.Contains(hit.point)&&hit.point.y<water.Surface+.1f)return false;
                normal+=hit.normal; top=Mathf.Max(top,hit.point.y); low=Mathf.Min(low,hit.point.y);
            }
            normal.Normalize();
            if(top-low>vehicle.wheelbase*.8f) return false;
            rotation=Quaternion.LookRotation(Vector3.ProjectOnPlane(forward,normal),normal);
            position.y=top+Mathf.Max(vehicle.suspensionLength-.12f,box.size.y*.5f-box.center.y+.12f);
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

