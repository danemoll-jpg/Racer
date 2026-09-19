using System.Collections.Generic;
using UnityEngine;

namespace Racer
{
    [DisallowMultipleComponent]
    public sealed class VehicleConfiguration : MonoBehaviour
    {
        static readonly HashSet<VehicleConfiguration> active = new();
        void OnEnable() => active.Add(this);
        void OnDisable() => active.Remove(this);
        public string profileId = "original";
        [SerializeField] string originalMotor;
        [SerializeField] Transform[] originalVisuals;
        [SerializeField] bool[] originalEnabled;
        [SerializeField] Vector3 originalSize, originalCenter;
        [SerializeField] float originalMass;
        public VehicleProfile Profile => VehicleProfile.Find(profileId);
        Transform generated;
        ArcadeVehicle motor;
        readonly List<Transform> wheels = new();
        float roll, crashUntil;
        float overturned, upright;
        Color? selectedPaint;
        public void SetBodyColor(int index) { if(index>=0 && index<VehiclePaint.Colors.Length) SetPaint(VehiclePaint.Colors[index]); }
        public void SetPaint(Color color) { selectedPaint=color; VehiclePaint.Apply(transform,color); }
        public bool WipedOut => Time.time < crashUntil;
        public int VehicleContactEvents { get; private set; }
        public void Recover() { crashUntil=0; overturned=upright=0; }
        public void PrepareContacts()
        {
            if(!motor || Profile.Small) return;
            bool smallNearby=false;
            foreach(var other in active)
                if(other && other!=this && other.motor && other.gameObject.scene==gameObject.scene && other.Profile.Small && (other.motor.Body.position-motor.Body.position).sqrMagnitude<400) { smallNearby=true; break; }
            // Sweep CCD resolves a first impact before mass scaling on this Unity version.
            // Use speculative detection only in the local car/small-vehicle interaction region.
            var mode=smallNearby?CollisionDetectionMode.ContinuousSpeculative:CollisionDetectionMode.ContinuousDynamic;
            if(motor.Body.collisionDetectionMode!=mode) motor.Body.collisionDetectionMode=mode;
        }
        public void Apply(string id)
        {
            motor = GetComponent<ArcadeVehicle>();
            var box = GetComponent<BoxCollider>();
            if (string.IsNullOrEmpty(originalMotor))
            {
                originalMotor = JsonUtility.ToJson(motor);
                originalSize = box.size; originalCenter = box.center; originalMass = motor.Body.mass;
                var children = new List<Transform>();
                foreach (Transform child in transform) if(child.name!="Water feedback"&&child.name!="Water ripple") children.Add(child);
                originalVisuals = children.ToArray();
                originalEnabled = children.ConvertAll(t=>t.gameObject.activeSelf).ToArray();
            }
            JsonUtility.FromJsonOverwrite(originalMotor, motor);
            profileId = VehicleProfile.Find(id).Id;
            selectedPaint=null;
            foreach(var renderer in GetComponentsInChildren<Renderer>(true))
                if(VehiclePaint.IsBodyPaint(renderer.sharedMaterial)) renderer.SetPropertyBlock(null);
            var p = Profile;
            for(int i=0;i<originalVisuals.Length;i++) if(originalVisuals[i]) originalVisuals[i].gameObject.SetActive(profileId=="original" && originalEnabled[i]);
            // Clones carry the generated hierarchy but not runtime field references.
            var old = transform.Find("Vehicle visual");
            if (old) { old.name="Retired visual"; old.gameObject.SetActive(false); Destroy(old.gameObject); }
            wheels.Clear(); generated = null;
            if (profileId != "original")
            {
                motor.topSpeed=p.Speed; motor.acceleration=p.Acceleration; motor.maxGripAcceleration=p.Grip;
                motor.steeringResponse=p.Response; motor.wheelbase=p.Wheelbase;
                motor.yawResponse=p.Small?11:8; motor.lateralGrip=p.Small?11:8;
                motor.slowSteerAngle=p.Small?29:31; motor.fastSteerAngle=p.Small?9:9.5f;
                motor.braking=p.Small?27:24;
                float track=p.Id=="moto"?.16f:p.Id=="atv"?.6f:.9f;
                float axle=p.Wheelbase*.5f;
                motor.suspensionPoints=new[]{new Vector3(-track,0,axle),new Vector3(track,0,axle),new Vector3(-track,0,-axle),new Vector3(track,0,-axle)};
                motor.suspensionLength=p.Small?.65f:.8f;
                motor.uprightStrength=p.Id=="moto"?19:22;
                motor.uprightDamping=p.Small?6:5;
                motor.airStability=p.Id=="moto"?.1f:.18f;
                motor.centreOfMass=new(0,p.Small?-.28f:-.35f,0);
                generated=VehicleVisual.Build(transform,p,wheels);
            }
            else foreach(var t in originalVisuals) if(t && t.name.Contains("wheel")) { wheels.Add(t); VehicleVisual.WheelDetail(t); }
            box.size=profileId=="original"?originalSize:p.Size;
            box.center=profileId=="original"?originalCenter:new Vector3(0,.05f,0);
            motor.Body.mass=profileId=="original"?originalMass:p.Mass;
            // Speculative CCD keeps small-vehicle contacts in the mass-modifiable solver.
            // Cars retain their existing sweep CCD; ground/obstacle material rules are unchanged.
            motor.Body.collisionDetectionMode=p.Small?CollisionDetectionMode.ContinuousSpeculative:CollisionDetectionMode.ContinuousDynamic;
            motor.Body.centerOfMass=motor.centreOfMass;
            motor.Body.ResetInertiaTensor(); motor.ClearSteering(); roll=crashUntil=0;
            VehicleContactEvents=0;
            VehicleContact.Register(motor.Body, p.Small);
            VehicleSurfaceContacts.Register(box);
            foreach(var c in GetComponentsInChildren<Collider>()) c.hasModifiableContacts=true;
            var camera=FindAnyObjectByType<ChaseCamera>();
            if(camera && camera.target==transform) { camera.offset=p.Camera; camera.Snap(); }
        }
        void LateUpdate()
        {
            if(!motor) return;
            roll += motor.ForwardSpeed * Time.deltaTime / .33f * Mathf.Rad2Deg;
            foreach(var wheel in wheels) if(wheel)
                wheel.localRotation=Quaternion.Euler(0,wheel.localPosition.z>0?motor.VisualSteering*27:0,0)*Quaternion.Euler(roll,0,90);
            if(generated && Profile.Id=="moto") generated.localRotation=Quaternion.Euler(0,0,-motor.VisualSteering*Mathf.Clamp(motor.ForwardSpeed,0,25)*.65f);
        }
        public void BuildPreview(Transform parent)
        {
            if(Profile.Id!="original") { VehicleVisual.Build(parent,Profile); if(selectedPaint.HasValue) VehiclePaint.Apply(parent,selectedPaint.Value); return; }
            for(int i=0;i<originalVisuals.Length;i++) if(originalVisuals[i] && originalEnabled[i])
            {
                var copy=Instantiate(originalVisuals[i].gameObject,parent,false);
                foreach(var t in copy.GetComponentsInChildren<Transform>(true)) t.gameObject.layer=parent.gameObject.layer;
                copy.SetActive(true);
            }
            if(selectedPaint.HasValue) VehiclePaint.Apply(parent,selectedPaint.Value);
        }
        void OnCollisionEnter(Collision collision)
        {
            if(collision.rigidbody && collision.rigidbody.GetComponent<ArcadeVehicle>()) VehicleContactEvents++;
            // Upright bumps and predicted CCD contacts are not a loss of control.
            // A sustained overturned state below handles meaningful recovery feedback.
        }
        void FixedUpdate()
        {
            if(!motor || !Profile.Small || motor.Body.isKinematic) return;
            bool needsHelp=transform.up.y<.35f && motor.Body.linearVelocity.magnitude<5;
            overturned=needsHelp?overturned+Time.fixedDeltaTime:0;
            upright=transform.up.y>.7f && motor.GroundedWheels>=2?upright+Time.fixedDeltaTime:0;
            if(overturned>1.2f && !WipedOut) Wipeout();
            if(upright>.35f && WipedOut) { Recover(); var flow=FindAnyObjectByType<RaceFlow>(); if(flow && flow.Race.vehicle==motor) flow.ClearRecoveryFeedback(); }
        }
        void Wipeout()
        {
            if(WipedOut) return;
            crashUntil=float.PositiveInfinity;
            var flow=FindAnyObjectByType<RaceFlow>();
            if(flow && flow.Race.vehicle==motor) flow.WipeoutFeedback();
        }
        void OnDestroy() { if(motor && motor.Body) VehicleContact.Unregister(motor.Body); var box=GetComponent<BoxCollider>(); if(box) VehicleSurfaceContacts.Unregister(box); }
    }
}
