using UnityEngine;

namespace Racer
{
    // Supplies pedals and steering to the same motor as the player. No speed multipliers or positional driving.
    public sealed class RoadDriver : MonoBehaviour
    {
        public RaceDirector Race;
        public ArcadeVehicle Car;
        [System.NonSerialized]
        public RacerState Racer;
        public int Direction { get; private set; } = 1;
        public float TargetSpeed { get; private set; }

        public int RecoveryCount { get; private set; }
        public float StalledSeconds => stalled;
        public float LastThrottle { get; private set; }
        public float LastBrake { get; private set; }
        public float BrakingSeconds { get; private set; }

        bool racing, finishParked;
        WoodlandRoute plannedBranch;
        static readonly float[] cornerUse={.46f,.65f,.82f}, brakeUse={.55f,.78f,.94f}, speedUse={.91f,.98f,1f}, hillTargets={26,29,31};
        float pace, stalled, safeS, lane, finishRunoff, nextRecovery;
        readonly RaycastHit[] hits = new RaycastHit[24];
        public void Initialize(RaceDirector race, ArcadeVehicle car, bool racer, int direction, float variation)
        {
            Race = race;
            Car = car;
            racing = racer;
            Direction = direction;
            pace = variation;
            lane = racer ? 1.7f : 2.6f * direction;
        }

        public void Place(float s, float side)
        {
            var p = Race.road.At(s, out var f);
            var right = Vector3.Cross(Vector3.up, f).normalized;
            p += right * side + Vector3.up * Mathf.Max(.4f,Car.suspensionLength-Physics.gravity.magnitude/Car.springStrength);
            Car.Body.position = p;
            Car.Body.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(f * Direction, Vector3.up));
            transform.SetPositionAndRotation(Car.Body.position, Car.Body.rotation);
            Car.Body.linearVelocity = Car.Body.angularVelocity = Vector3.zero;
            Car.ClearSteering();
            safeS = s;
            Racer?.SampleOrigin(Time.timeAsDouble);
        }

        void FixedUpdate()
        {
            if (!Race || !Car)
                return;
            Car.enabled = false;
            if (Race.Flow.State != RaceFlow.Stage.Racing)
            {
                Car.Body.isKinematic = true;
                return;
            }

            bool finished = Racer != null && (Racer.Progress.Finished || Racer.Dnf);
            if(finished && finishParked) { Car.Body.isKinematic=true; return; }
            Car.Body.isKinematic = false;
            if (finished && finishRunoff > 55)
            {
                if(Car.ForwardSpeed<.8f && Car.GroundedWheels>=2)
                {
                    Car.Body.linearVelocity=Car.Body.angularVelocity=Vector3.zero;
                    Car.Body.isKinematic=true; finishParked=true;
                }
                else Car.Simulate(0,1,0,Time.fixedDeltaTime);
                return;
            }

            float s = Race.road.Project(Car.Body.position, out float lateral);
            Car.GetComponent<VehicleRespawn>().RecordSafePosition();
            float speed = Mathf.Abs(Car.ForwardSpeed);
            Race.road.At(s, out var tangent);
            float look = Mathf.Clamp(7 + speed * .48f, 8, 25);
            float desiredLane = racing ? Mathf.Lerp(lane,2.05f,Race.road.HighwayBlend(s)) : Race.road.TrafficLane(s,Direction,pace>.975f);
            if(finished) { finishRunoff += speed*Time.fixedDeltaTime; desiredLane=4.7f; }
            // The connector ramp occupies the left six metres; both traffic directions use its ground bypass.
            bool bypass = Race.road.InBypass(s);
            if (bypass)
                desiredLane = Direction > 0 ? 3.2f : 6.2f;
            if (racing && !bypass && !finished)
            {
                bool slowerAhead = false, oncoming = false;
                foreach (var other in Race.Drivers)
                    if (other && other != this)
                    {
                        var delta = other.transform.position - transform.position;
                        float along = Vector3.Dot(delta, tangent);
                        float side = Mathf.Abs(Vector3.Dot(delta, Vector3.Cross(Vector3.up, tangent)));
                        if (other.Direction < 0 && along > -10 && along < 110)
                            oncoming = true;
                        if (other.Direction > 0 && along > 4 && along < 50 && side < 3.5f && other.Car.ForwardSpeed < speed + 2)
                            slowerAhead = true;
                    }

                if (Car != Race.vehicle)
                {
                    var delta = Race.vehicle.transform.position - transform.position;
                    float along = Vector3.Dot(delta, tangent);
                    if (along > 3 && along < 50 && Mathf.Abs(Vector3.Dot(delta, Vector3.Cross(Vector3.up, tangent))) < 3.5f && Race.vehicle.ForwardSpeed < speed + 2)
                        slowerAhead = true;
                }

                if (slowerAhead && (!oncoming || Race.road.HighwayBlend(s)>.95f))
                    desiredLane = Race.road.HighwayBlend(s)>.95f ? 6.15f : -2.3f;
            }

            if(racing && !finished && Racer!=null && Race.difficulty>0)
            {
                if(plannedBranch && s>plannedBranch.exitRoad+15) plannedBranch=null;
                if(!plannedBranch && Race.Branches!=null)
                    foreach(var branch in Race.Branches)
                        if(branch.aiValidated && s>=branch.entryRoad-55 && s<branch.entryRoad &&
                            (Race.difficulty==2 || Car.GetComponent<VehicleConfiguration>().Profile.Small)) { plannedBranch=branch; break; }
            }
            var activeBranch=plannedBranch && Racer?.Branch.Route==plannedBranch ? plannedBranch : null;
            if(!activeBranch && plannedBranch && s>=plannedBranch.entryRoad-2) activeBranch=plannedBranch;
            float branchS=activeBranch?activeBranch.Project(Car.Body.position,out _):0;
            if(plannedBranch || activeBranch) desiredLane=0;
            var target = activeBranch ? activeBranch.At(branchS+look,out _) : Race.road.At(s + Direction * look, out _);
            var ahead = tangent;
            if(activeBranch) activeBranch.At(branchS+look,out ahead); else Race.road.At(s+Direction*look,out ahead);
            target += Vector3.Cross(Vector3.up, ahead).normalized * desiredLane;
            var local = transform.InverseTransformPoint(target);
            float angle = Mathf.Atan2(local.x, local.z);
            float maxAngle = Mathf.Lerp(Car.slowSteerAngle, Car.fastSteerAngle, Mathf.Clamp01(speed / Car.topSpeed)) * Mathf.Deg2Rad;
            float steering = Mathf.Clamp(Mathf.Atan(2 * Car.wheelbase * Mathf.Sin(angle) / look) / maxAngle, -1, 1);
            int skill = racing ? Mathf.Clamp(Race.difficulty,0,2) : 0;
            float cornerGrip = racing ? Car.maxGripAcceleration*cornerUse[skill] : 7.5f;
            float judgment = racing ? Car.braking*brakeUse[skill] : 8f;
            TargetSpeed = (racing ? Car.topSpeed * speedUse[skill] : Mathf.Lerp(17,29,Race.road.HighwayBlend(s))) * pace;
            if(finished) TargetSpeed=12;
            // Consistency costs time through early lifting, never extra vehicle capability.
            if (racing && skill < 2) TargetSpeed *= 1 - (2-skill)*.035f*(.5f+.5f*Mathf.Sin(Time.time*.19f+pace*31));
            if(activeBranch) TargetSpeed=Mathf.Min(TargetSpeed,activeBranch.recommendedSpeed);
            for (float d = 0; d <= 100; d += 8)
            {
                Vector3 f,next;
                if(activeBranch) {activeBranch.At(branchS+d,out f);activeBranch.At(branchS+d+8,out next);}
                else {Race.road.At(s + Direction * d, out f);Race.road.At(s + Direction * (d + 8), out next);}
                float curvature = Vector3.Angle(Vector3.ProjectOnPlane(f,Vector3.up),Vector3.ProjectOnPlane(next,Vector3.up)) * Mathf.Deg2Rad / 8;
                float curveSpeed = Mathf.Sqrt(cornerGrip / Mathf.Max(.00015f, curvature));
                float hillSpeed = Mathf.Abs(f.y) > .14f ? (racing?hillTargets[skill]:24) : Car.topSpeed;
                // Without downforce, a convex crest cannot support v²/r greater than gravity.
                float crest=Mathf.Max(0,Mathf.Asin(f.y)-Mathf.Asin(next.y))/8;
                if(!activeBranch && crest>.001f) hillSpeed=Mathf.Min(hillSpeed,Mathf.Sqrt(6.5f/crest));
                TargetSpeed = Mathf.Min(TargetSpeed, Mathf.Sqrt(Mathf.Pow(Mathf.Min(curveSpeed, hillSpeed), 2) + 2 * judgment * Mathf.Max(0, d - 12)));
            }

            if (Mathf.Abs(angle) > .6f)
                TargetSpeed = Mathf.Min(TargetSpeed, 10);
            // Bounded non-alloc obstacle query; includes other cars and solid roadside objects, excludes own body.
            float range = 6 + speed * 1.5f;
            float radius=Mathf.Min(.7f,Car.GetComponent<BoxCollider>().size.x*.5f+.12f);
            int count = Physics.SphereCastNonAlloc(Car.Body.position + Vector3.up * .35f, radius, transform.forward, hits, range, ~0, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < count; i++)
            {
                var hit = hits[i];
                if (hit.rigidbody == Car.Body || hit.normal.y > .55f)
                    continue;
                float aheadSpeed=hit.rigidbody?Mathf.Max(0,Vector3.Dot(hit.rigidbody.linearVelocity,transform.forward)):0;
                float allowed = Mathf.Sqrt(Mathf.Max(0,aheadSpeed*aheadSpeed+2*judgment*(hit.distance-4)));
                TargetSpeed = Mathf.Min(TargetSpeed, allowed);
            }

            float throttle = speed < TargetSpeed ? Mathf.Clamp01((TargetSpeed - speed) * .5f) : 0;
            float brake = speed > TargetSpeed + .5f ? Mathf.Clamp01((speed - TargetSpeed) * .25f) : 0;
            if (TargetSpeed < 1 && speed < .7f)
            {
                throttle = brake = 0;
                Car.Body.linearVelocity *= .85f;
            }

            if (speed < 1.5f)
                stalled += Time.fixedDeltaTime;
            else
                stalled = 0;
            if (stalled > 5 && stalled < 8)
            {
                throttle = 0;
                brake = .45f;
                steering = -steering;
            }

            if (Time.time>=nextRecovery && (stalled > 12 || (lateral > 22 && stalled>5) || Vector3.Dot(transform.up, Vector3.up) < .1f))
            {
                TryRecover(s);
            }

            if (lateral < 5 && speed > 4 && Vector3.Dot(transform.forward, tangent * Direction) > .6f)
                safeS = s;
            LastThrottle=throttle; LastBrake=brake; if(brake>.1f) BrakingSeconds+=Time.fixedDeltaTime;
            Car.Simulate(throttle, brake, steering, Time.fixedDeltaTime);
        }

        void TryRecover(float current)
        {
            if(racing)
            {
                var recovery=Car.GetComponent<VehicleRespawn>();
                if(!recovery.TryRecoverLocal(true)) return;
                stalled=0; nextRecovery=Time.time+4; RecoveryCount++;
                if(Racer!=null) { Racer.Recoveries++; Racer.Branch.Recovered(Car.Body.position); Racer.SampleOrigin(Race.Clock); }
                return;
            }
            // Return behind the last stable sample, and never award progress. Wait if any vehicle occupies the pad.
            float destination = safeS - Direction * 8;
            float recoveryLane = Race.road.InBypass(destination) ? (Direction > 0 ? 3.2f : 6.2f) : Race.road.TrafficLane(destination,Direction,pace>.975f);
            var p = Race.road.At(destination, out var f) + Vector3.Cross(Vector3.up, f).normalized * recoveryLane + Vector3.up;
            if (Vector3.Distance(p, Race.vehicle.transform.position) < 40)
                return;
            foreach (var driver in Race.Drivers)
                if (driver && driver != this && Vector3.Distance(driver.transform.position, p) < 14)
                    return;
            if (!racing)
            {
                if (Vector3.Distance(p, Race.vehicle.transform.position) < 200 || Vector3.Distance(transform.position, Race.vehicle.transform.position) < 200)
                    return;
                var camera = Camera.main;
                if (camera)
                {
                    foreach (var point in new[]{p, transform.position})
                    {
                        var view = camera.WorldToViewportPoint(point);
                        if (view.z > 0 && view.x > -.1f && view.x < 1.1f && view.y > -.1f && view.y < 1.1f)
                            return;
                    }
                }
            }

            Place(destination, recoveryLane);
            stalled = 0;
            RecoveryCount++;
            if (Racer != null)
            {
                Racer.Recoveries++;
                Racer.Travel = 0;
                Racer.RecoveryStart = Race.road.Project(Car.Body.position, out _);
            }
        }
    }
}
