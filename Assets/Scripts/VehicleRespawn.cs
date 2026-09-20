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
        struct SafeSample {public WoodlandRoute branch;public float station;}
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
            RecordSafePosition();
            // AI owns its retry/cooldown bookkeeping. A second automatic respawn
            // here could otherwise move it without clearing its stuck counters.
            if(TryGetComponent<RoadDriver>(out var driver)&&driver.enabled)return;
            if(input.enabled && input.ConsumeReset()) ResetVehicle();
            else if((Pending || transform.position.y<fallResetHeight) && Time.time>=nextAttempt) ResetVehicle();
        }
        float safeStation,trackingStation;
        bool tracking;
        WoodlandRoute trackingBranch;
        bool anchored;
        Vector3 observed;
        WoodlandRoute safeBranch;
        RaceRoad roamRoad;WoodlandRoute roamBranch;float roamStation,roamDirection=1;bool roamValid;
        public float SafeStation => safeStation;
        public void SeedCoursePosition(Vector3 position)
        {
            if(!race)race=FindAnyObjectByType<RaceDirector>();if(!race||!race.road)return;
            safeStation=race.road.Project(position,out _);safeBranch=null;observed=position;anchored=true;
            trackingStation=safeStation;trackingBranch=null;tracking=true;
        }
        public void RecordSafePosition()
        {
            if(!race)race=FindAnyObjectByType<RaceDirector>();
            if(!race || !race.road || Time.time<nextHistory)return;
            nextHistory=Time.time+.25f;
            if(race.FreeRoam){if(vehicle.GroundedWheels>=3&&transform.up.y>=.85f)RecordRoaming();return;}
            var state=race.Racers.FirstOrDefault(r=>r.Car==vehicle);
            var branch=state?.Branch.Route;
            var p=vehicle.Body.position;
            bool continuous=!tracking||Vector3.Distance(observed,p)<=90;observed=p;
            if(!continuous)return;
            float station=branch?branch.Project(p,out _):tracking&&trackingBranch==branch?race.road.ProjectNear(p,trackingStation,75,out _):race.road.Project(p,out _);
            trackingStation=station;trackingBranch=branch;tracking=true;
            if(vehicle.GroundedWheels<3||transform.up.y<.85f)return;
            Vector3 support=branch?branch.At(station,out var direction):race.road.At(station,out direction);
            float width=branch?branch.halfWidth:race.road.HalfWidth(station);
            if(Vector3.ProjectOnPlane(p-support,Vector3.up).magnitude>width-.4f || Mathf.Abs(p.y-support.y)>4)return;
            safeStation=station;safeBranch=branch;anchored=true;observed=p;
            if(history.Count==120)history.RemoveAt(0);
            history.Add(new SafeSample{branch=branch,station=station});
        }
        public void RestartAtStart()
        {
            Pending=false;history.Clear();nextHistory=nextAttempt=0;anchored=false;tracking=false;safeBranch=null;roamValid=false;
            Place(spawnPoint?spawnPoint.position:initialPosition,spawnPoint?Quaternion.Euler(0,spawnPoint.eulerAngles.y,0):initialRotation);
        }
        public void CancelRecovery(){Pending=false;history.Clear();anchored=false;tracking=false;safeBranch=null;roamValid=false;}
        public void ResetVehicle()=>TryRecoverLocal();
        public bool TryRecoverLocal(bool preferRoad=false)
        {
            if(!race)race=FindAnyObjectByType<RaceDirector>();
            if(!race || !race.road)return false;
            MeasureClearance();
            if(race.FreeRoam&&roamValid)return RecoverRoaming();
            var state=race.Racers.FirstOrDefault(r=>r.Car==vehicle);
            var branch=state?.Branch.Route;
            var from=vehicle.Body.position;
            // Earned branch station or last supported course sample, never nearest arbitrary ground.
            float at=branch?Mathf.Min(state.Branch.Position,state.Branch.Earned):
                anchored?(safeBranch?safeBranch.entryRoad:safeStation):race.road.Project(spawnPoint?spawnPoint.position:initialPosition,out _);
            var candidates=new List<float>();
            if(Time.time-lastRecoveryAt>35)recoveryEscalation=0;
            float retreat=preferRoad?Mathf.Min(36,recoveryEscalation*9):0;
            foreach(float back in new[]{2f,5f,9f,15f,24f,35f,48f,65f,90f,120f,160f})
                candidates.Add(branch?Mathf.Max(0,at-back-retreat):at-back-retreat);
            // Last resort: previously occupied supported course samples, still checked
            // for current traffic/obstructions. Never switch to a different branch.
            for(int i=history.Count-1;i>=0;i--)
            {
                var sample=history[i];if(sample.branch!=branch)continue;
                float back=branch?at-sample.station:Mathf.Repeat(at-sample.station,race.road.Length);
                if(back>=0&&back<=160&&!candidates.Any(s=>Mathf.Abs(s-sample.station)<1))candidates.Add(sample.station);
            }
            foreach(float s in candidates)
            {
                var point=branch?branch.At(s,out var f):race.road.At(s,out f);
                var forward=Vector3.ProjectOnPlane(f,Vector3.up).normalized;
                float width=branch?branch.halfWidth:race.road.HalfWidth(s);
                float sideLimit=Mathf.Max(0,width-clearance.size.x*.5f-.5f);
                float alternate=Mathf.Min(3,sideLimit)*(recoveryEscalation%2==0?1:-1);
                var sides=preferRoad&&!branch?new[]{alternate,-alternate,0f,Mathf.Min(1.7f,sideLimit),-Mathf.Min(1.7f,sideLimit)}:new[]{0f,Mathf.Min(1.7f,sideLimit),-Mathf.Min(1.7f,sideLimit),sideLimit,-sideLimit};
                foreach(float side in sides)
                {
                    var candidate=point+Vector3.Cross(Vector3.up,forward)*side;
                    if(!Supported(candidate,forward,out var position,out var rotation) || !Clear(position,rotation))continue;
                    if(Mathf.Abs(position.y-point.y)>5)continue;
                    Place(position,rotation);Pending=false;
                    lastRecoveryAt=Time.time;if(preferRoad)recoveryEscalation++;
                    safeStation=s;safeBranch=branch;observed=position;anchored=true;trackingStation=s;trackingBranch=branch;tracking=true;
                    state?.Branch.Recovered(position);state?.SampleOrigin(race.Clock);
                    LastRecovery="Recovered to clear earned course support";Respawned?.Invoke();return true;
                }
            }
            Pending=true;nextAttempt=Time.time+.5f;LastRecovery="Waiting for clear course support";return false;
        }
        void RecordRoaming()
        {
            var p=vehicle.Body.position;float best=float.MaxValue,station=0;RaceRoad road=null;WoodlandRoute branch=null;
            foreach(var candidate in new[]{race.road,race.ambientRoad})if(candidate){float s=candidate.Project(p,out float d);if(d<best){best=d;road=candidate;station=s;}}
            var exploration=race.GetComponent<ExplorationCollection>();
            if(exploration)foreach(var candidate in exploration.routes)if(candidate){float s=candidate.Project(p,out float d);if(d<best){best=d;road=candidate;station=s;}}
            foreach(var candidate in race.Branches){float s=candidate.Project(p,out float d);d=Vector3.Distance(p,candidate.At(s,out _));if(d<best){best=d;branch=candidate;road=null;station=s;}}
            float width=branch?branch.halfWidth:road.HalfWidth(station);var point=branch?branch.At(station,out var f):road.At(station,out f);
            if(Vector3.ProjectOnPlane(p-point,Vector3.up).magnitude>width-box.size.x*.5f || Mathf.Abs(p.y-point.y)>8)return;
            roamRoad=road;roamBranch=branch;roamStation=station;roamValid=true;
            if(vehicle.Body.linearVelocity.magnitude>2)roamDirection=Vector3.Dot(vehicle.Body.linearVelocity,f)>=0?1:-1;
        }
        bool RecoverRoaming()
        {
            foreach(float back in new[]{2f,5f,10f,18f,30f,50f,80f,120f})
            {
                float s=roamStation-roamDirection*back;if(roamBranch)s=Mathf.Clamp(s,0,roamBranch.Length);
                var p=roamBranch?roamBranch.At(s,out var f):roamRoad.At(s,out f);f=Vector3.ProjectOnPlane(f*roamDirection,Vector3.up).normalized;
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
