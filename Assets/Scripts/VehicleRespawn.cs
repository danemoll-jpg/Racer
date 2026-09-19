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
        readonly List<Vector3> history=new();
        ArcadeVehicle vehicle;
        VehicleInput input;
        BoxCollider box;
        RaceDirector race;
        Vector3 initialPosition;
        Quaternion initialRotation;
        float nextHistory, nextAttempt;
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
            if(input.enabled && input.ConsumeReset()) ResetVehicle();
            else if((Pending || transform.position.y<fallResetHeight) && Time.time>=nextAttempt) ResetVehicle();
        }
        public void RecordSafePosition()
        {
            if(vehicle.GroundedWheels>=3 && transform.up.y>.85f && Time.time>=nextHistory)
            {
                nextHistory=Time.time+.25f;
                if(history.Count==120) history.RemoveAt(0);
                history.Add(vehicle.Body.position);
            }
        }
        public void RestartAtStart()
        {
            Pending=false; history.Clear(); nextHistory=nextAttempt=0;
            Place(spawnPoint?spawnPoint.position:initialPosition,spawnPoint?Quaternion.Euler(0,spawnPoint.eulerAngles.y,0):initialRotation);
        }
        public void CancelRecovery() { Pending=false; history.Clear(); }
        public void ResetVehicle() => TryRecoverLocal();
        public bool TryRecoverLocal(bool preferRoad=false)
        {
            if(!race) race=FindAnyObjectByType<RaceDirector>();
            Vector3 from=vehicle.Body.position;
            var forward=Vector3.ProjectOnPlane(transform.forward,Vector3.up).normalized;
            if(forward.sqrMagnitude<.5f) forward=Vector3.forward;
            float currentS=race && race.road?race.road.Project(from,out _):0;
            if(race && race.road) { race.road.At(currentS,out var tangent); forward=Vector3.ProjectOnPlane(tangent,Vector3.up).normalized; }
            var state=race?race.Racers.FirstOrDefault(r=>r.Car==vehicle):null;
            var branch=state?.Branch.Route;
            var candidates=new List<Vector3>();
            if(branch)
            {
                float at=Mathf.Min(state.Branch.Position,state.Branch.Earned);
                branch.At(at,out forward); forward=Vector3.ProjectOnPlane(forward,Vector3.up).normalized;
                foreach(float back in new[]{2f,5f,9f,15f,24f,35f,48f}) candidates.Add(branch.At(Mathf.Max(0,at-back),out _));
            }
            if(!branch && preferRoad && race && race.road)
                foreach(float back in new[]{1f,3f,6f,10f,16f,24f})
                {
                    var roadPoint=race.road.At(currentS-back,out var direction);
                    roadPoint+=Vector3.Cross(Vector3.up,direction).normalized*(race.road.InBypass(currentS-back)?3.2f:1.7f);
                    if(Vector3.Distance(roadPoint,from)<60) candidates.Add(roadPoint);
                }
            candidates.Add(from);
            var right=Vector3.Cross(Vector3.up,forward);
            foreach(float radius in new[]{1.5f,3f,5f,8f,12f})
                foreach(var offset in new[]{right,-right,-forward,(-forward+right).normalized,(-forward-right).normalized})
                    candidates.Add(from+offset*radius);
            // Bounded local history; no silent fallback to START.
            foreach(var point in history) if(Vector3.Distance(point,from)<60) candidates.Add(point);
            if(!branch && !preferRoad) candidates.Sort((a,b)=>(a-from).sqrMagnitude.CompareTo((b-from).sqrMagnitude));
            foreach(var candidate in candidates)
            {
                if(!branch && race && race.road)
                {
                    float delta=Mathf.Repeat(race.road.Project(candidate,out _)-currentS+race.road.Length*.5f,race.road.Length)-race.road.Length*.5f;
                    if(delta>.05f || delta < -60) continue;
                }
                if(branch)
                {
                    float bs=branch.Project(candidate,out float bl);
                    if(bl>branch.halfWidth || bs>state.Branch.Position+.05f || bs<state.Branch.Position-60) continue;
                    branch.At(bs,out forward); forward=Vector3.ProjectOnPlane(forward,Vector3.up).normalized;
                }
                if(!Supported(candidate,forward,out var position,out var rotation) || !Clear(position,rotation)) continue;
                if(branch && branch.Project(position,out _)>state.Branch.Position+.05f) continue;
                if(!branch && race && race.road)
                {
                    float delta=Mathf.Repeat(race.road.Project(position,out _)-currentS+race.road.Length*.5f,race.road.Length)-race.road.Length*.5f;
                    if(delta>.05f) continue;
                    bool crossed=false;
                    foreach(var gate in race.gates) if(gate.TryCross(from,position,out bool ahead,out _) && ahead) { crossed=true; break; }
                    if(crossed) continue;
                }
                Place(position,rotation); Pending=false;
                state?.Branch.Recovered(position); state?.SampleOrigin(race.Clock);
                LastRecovery=Vector3.Distance(position,from)<2?"Righted locally":"Recovered to nearby support";
                Respawned?.Invoke(); return true;
            }
            Pending=true; nextAttempt=Time.time+.4f;
            LastRecovery="Waiting for clear local support";
            return false;
        }
        bool Supported(Vector3 candidate,Vector3 forward,out Vector3 position,out Quaternion rotation)
        {
            position=candidate; rotation=Quaternion.LookRotation(forward);
            Vector3 normal=Vector3.zero; float top=float.MinValue, low=float.MaxValue;
            foreach(var local in vehicle.suspensionPoints)
            {
                var origin=candidate+rotation*local+Vector3.up*5;
                if(!Physics.Raycast(origin,Vector3.down,out var hit,12,vehicle.groundMask,QueryTriggerInteraction.Ignore)
                    || hit.rigidbody || hit.normal.y<.65f) return false;
                string support=hit.collider.name;
                if(!support.StartsWith("Ground_") && !support.StartsWith("Takeoff -") && !support.StartsWith("Landing -") && !support.StartsWith("Gully supported ramp")) return false;
                normal+=hit.normal; top=Mathf.Max(top,hit.point.y); low=Mathf.Min(low,hit.point.y);
            }
            normal.Normalize();
            if(top-low>vehicle.wheelbase*.8f) return false;
            rotation=Quaternion.LookRotation(Vector3.ProjectOnPlane(forward,normal),normal);
            position.y=top+Mathf.Max(vehicle.suspensionLength-.12f,box.size.y*.5f-box.center.y+.12f);
            return true;
        }
        bool Clear(Vector3 position,Quaternion rotation)
        {
            var center=position+rotation*box.center;
            var half=Vector3.Scale(box.size*.5f,transform.lossyScale)+Vector3.one*.12f;
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
