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
        public DriverVariation Variation { get; } = new();

        public int RecoveryCount { get; private set; }
        public float StalledSeconds => stalled;
        public float LastThrottle { get; private set; }
        public float LastBrake { get; private set; }
        public float BrakingSeconds { get; private set; }
        public bool HighwayTraffic;
        public int HighwayRecycles { get; private set; }
        public float MinimumRecyclePlayerDistance { get; private set; } = float.MaxValue;

        public RaceRoad DriveRoad => !racing && Race.ambientRoad ? Race.ambientRoad : Race.road;
        bool racing, finishParked;
        ForestLayout forestLayout;
        WoodlandRoute plannedBranch, progressBranch;
        float branchBest, branchStuck;
        static readonly float[] cornerUse={.46f,.76f,.90f}, brakeUse={.55f,.88f,.98f}, speedUse={.91f,1f,1f}, hillTargets={26,32,35};
        float pace, stalled, safeS, lane, finishRunoff, nextRecovery;
        readonly RaycastHit[] hits = new RaycastHit[24];
        public void Initialize(RaceDirector race, ArcadeVehicle car, bool racer, int direction, float variation)
        {
            Race = race;
            forestLayout=race.Forest?FindAnyObjectByType<ForestLayout>():null;
            Car = car;
            racing = racer;
            Direction = direction;
            pace = variation;
            lane = racer ? 1.7f : 2.6f * direction;
            Variation.Initialize(DriverVariation.Seed,race.Drivers.Count+Mathf.RoundToInt(variation*1000));
        }

        public void Place(float s, float side)
        {
            var p = DriveRoad.At(s, out var f);
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
            if(Racer!=null && Racer.Estimated) { Car.Body.isKinematic=true; return; }
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

            float s = DriveRoad.Project(Car.Body.position, out float lateral);
            if(HighwayTraffic && (Direction>0?s>4680 || s<3650:s<3650 || s>4680))
            { TryRecycleHighway(); s=DriveRoad.Project(Car.Body.position,out lateral); }
            Car.GetComponent<VehicleRespawn>().RecordSafePosition();
            float speed = Mathf.Abs(Car.ForwardSpeed);
            DriveRoad.At(s, out var tangent);
            float look = Mathf.Clamp(7 + speed * .48f, 8, 25);
            float desiredLane = racing ? Mathf.Lerp(lane,2.05f,DriveRoad.HighwayBlend(s)) : DriveRoad.TrafficLane(s,Direction,pace>.955f);
            if(racing&&forestLayout)desiredLane=.55f;
            if(finished) { finishRunoff += speed*Time.fixedDeltaTime; desiredLane=4.7f; }
            // The connector ramp occupies the left six metres; both traffic directions use its ground bypass.
            bool bypass = DriveRoad.InBypass(s);
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
                        float relativeDirection=Race.reverseCourse?Vector3.Dot(other.transform.forward,tangent):other.Direction;
                        if (relativeDirection < 0 && along > -10 && along < 110)
                            oncoming = true;
                        if (relativeDirection > 0 && along > 4 && along < 50 && side < 3.5f && other.Car.ForwardSpeed < speed + 2)
                            slowerAhead = true;
                    }

                if (Car != Race.vehicle)
                {
                    var delta = Race.vehicle.transform.position - transform.position;
                    float along = Vector3.Dot(delta, tangent);
                    if (along > 3 && along < 50 && Mathf.Abs(Vector3.Dot(delta, Vector3.Cross(Vector3.up, tangent))) < 3.5f && Race.vehicle.ForwardSpeed < speed + 2)
                        slowerAhead = true;
                }

                if (slowerAhead && (!forestLayout || DriveRoad.HalfWidth(s)>=4.9f) && (!oncoming || DriveRoad.HighwayBlend(s)>.95f))
                    desiredLane = DriveRoad.HighwayBlend(s)>.95f ? 6.15f : -2.3f;
                // Commit to a pass only when its destination lane has room alongside and ahead.
                float currentLane=Vector3.Dot(Car.Body.position-DriveRoad.At(s,out _),Vector3.Cross(Vector3.up,tangent).normalized);
                if(Mathf.Abs(desiredLane-currentLane)>1 && !LaneClear(s,desiredLane,speed))
                    desiredLane=Mathf.Clamp(currentLane,-DriveRoad.HalfWidth(s)+1.2f,DriveRoad.HalfWidth(s)-1.2f);
            }

            if(plannedBranch && (s>plannedBranch.exitRoad+15 || (plannedBranch.Project(Car.Body.position,out float exitLateral)>=plannedBranch.Length-7 && exitLateral<plannedBranch.halfWidth+7 && Racer?.Branch.Route==null))) plannedBranch=null;
            if(racing && !finished && Racer!=null && Race.difficulty>0)
            {
                if(!plannedBranch && Race.Branches!=null)
                    foreach(var branch in Race.Branches)
                        if(branch.aiValidated && s>=branch.entryRoad-55 && s<branch.entryRoad &&
                            (Race.difficulty==2 || Car.GetComponent<VehicleConfiguration>().Profile.Small)) { plannedBranch=branch; break; }
            }
            var activeBranch=plannedBranch && Racer?.Branch.Route==plannedBranch ? plannedBranch : null;
            if(!activeBranch && plannedBranch && s>=plannedBranch.entryRoad-(Race.reverseCourse?look:2)) activeBranch=plannedBranch;
            float branchS=activeBranch?activeBranch.Project(Car.Body.position,out _):0;
            if(activeBranch!=progressBranch){progressBranch=activeBranch;branchBest=branchS;branchStuck=0;}
            if(activeBranch&&Race.reverseCourse&&Direction>0){if(branchS>branchBest+2){branchBest=branchS;branchStuck=0;}else branchStuck+=Time.fixedDeltaTime;}else branchStuck=0;
            if(plannedBranch || activeBranch) desiredLane=0;
            // Commit to the readable central launch line; pass in the intervening pockets.
            if(racing&&forestLayout&&forestLayout.Approach(s))desiredLane=0;
            bool jumpApproach=racing&&forestLayout&&forestLayout.Approach(s)&&!forestLayout.IsLaunch(s);
            Variation.Step(this,s,speed,!racing||finished||plannedBranch||activeBranch||bypass||lateral>2.5f||Car.transform.up.y<.9f|| (forestLayout&&forestLayout.IsLaunch(s)),jumpApproach);
            if(Variation.Line!=0)desiredLane=Mathf.Clamp(desiredLane+Variation.Line,-DriveRoad.HalfWidth(s)+1.6f,DriveRoad.HalfWidth(s)-1.6f);
            var target = activeBranch ? activeBranch.At(branchS+look,out _) : DriveRoad.At(s + Direction * look, out _);
            var ahead = tangent;
            if(activeBranch) activeBranch.At(branchS+look,out ahead); else DriveRoad.At(s+Direction*look,out ahead);
            target += Vector3.Cross(Vector3.up, ahead).normalized * desiredLane;
            var local = transform.InverseTransformPoint(target);
            float angle = Mathf.Atan2(local.x, local.z);
            float maxAngle = Mathf.Lerp(Car.slowSteerAngle, Car.fastSteerAngle, Mathf.Clamp01(speed / Car.topSpeed)) * Mathf.Deg2Rad;
            float steering = Mathf.Clamp(Mathf.Atan(2 * Car.wheelbase * Mathf.Sin(angle) / look) / maxAngle, -1, 1);
            int skill = racing ? Mathf.Clamp(Race.difficulty,0,2) : 0;
            float cornerGrip = racing ? Car.maxGripAcceleration*cornerUse[skill] : 7.5f;
            float judgment = racing ? Car.braking*brakeUse[skill] : 8f;
            judgment*=Variation.Judgment;
            TargetSpeed = (racing ? Car.topSpeed * speedUse[skill] : Mathf.Lerp(17,29,DriveRoad.HighwayBlend(s))) * pace;
            if(finished) TargetSpeed=12;
            // Consistency costs time through early lifting, never extra vehicle capability.
            if (racing && skill < 2) TargetSpeed *= 1 - (skill==0?.07f:.012f)*(.5f+.5f*Mathf.Sin(Time.time*.19f+pace*31));
            if(activeBranch) TargetSpeed=Mathf.Min(TargetSpeed,activeBranch.SpeedAt(branchS));
            else if(plannedBranch&&plannedBranch.entrySpeed>0)TargetSpeed=Mathf.Min(TargetSpeed,Mathf.Sqrt(plannedBranch.entrySpeed*plannedBranch.entrySpeed+2*judgment*Mathf.Max(0,plannedBranch.entryRoad-s-12)));
            if(racing&&!activeBranch&&forestLayout&&forestLayout.Approach(s))TargetSpeed=Mathf.Min(TargetSpeed,32);
            if(!racing && pace>.955f && DriveRoad.HighwayBlend(s)>.01f && DriveRoad.HighwayBlend(s)<.85f)
                foreach(var other in Race.Drivers)
                    if(other&&other!=this&&other.Direction==Direction)
                    {
                        var delta=other.transform.position-transform.position;float along=Vector3.Dot(delta,tangent*Direction);
                        if(along>-5&&along<30&&Vector3.ProjectOnPlane(delta,Vector3.up).magnitude<35)
                            TargetSpeed=Mathf.Min(TargetSpeed,Mathf.Max(4,other.Car.ForwardSpeed-3));
                    }
            for (float d = 0; d <= 100; d += 8)
            {
                Vector3 f,next;
                if(activeBranch) {activeBranch.At(branchS+d,out f);activeBranch.At(branchS+d+8,out next);}
                else {DriveRoad.At(s + Direction * d, out f);DriveRoad.At(s + Direction * (d + 8), out next);}
                float curvature = Vector3.Angle(Vector3.ProjectOnPlane(f,Vector3.up),Vector3.ProjectOnPlane(next,Vector3.up)) * Mathf.Deg2Rad / 8;
                float curveSpeed = Mathf.Sqrt(cornerGrip / Mathf.Max(.00015f, curvature));
                float hillSpeed = Mathf.Abs(f.y) > .14f ? (activeBranch && Race.reverseCourse ? activeBranch.SpeedAt(branchS+d) : racing?hillTargets[skill]:24) : Car.topSpeed;
                // Without downforce, a convex crest cannot support v²/r greater than gravity.
                float crest=Mathf.Max(0,Mathf.Asin(f.y)-Mathf.Asin(next.y))/8;
                bool authoredFlight=racing&&forestLayout&&forestLayout.IsLaunch(s+Direction*d);
                if(!activeBranch && !authoredFlight && crest>.001f) hillSpeed=Mathf.Min(hillSpeed,Mathf.Sqrt((racing&&skill>0?(skill==2?8.2f:7.6f):6.5f)/crest));
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

            float throttle = speed < TargetSpeed ? Mathf.Clamp01((TargetSpeed - speed) * (racing&&skill>0?.8f:.5f)) : 0;
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

            if (Time.time>=nextRecovery && (stalled > 12 || branchStuck>10 || (lateral > 22 && stalled>5) || Vector3.Dot(transform.up, Vector3.up) < .1f))
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
                stalled=branchStuck=0;branchBest=Racer?.Branch.Position??0; nextRecovery=Time.time+4; RecoveryCount++;
                if(Racer!=null) { Racer.Recoveries++; Racer.Branch.Recovered(Car.Body.position); Racer.SampleOrigin(Race.Clock); }
                return;
            }
            // Return behind the last stable sample, and never award progress. Wait if any vehicle occupies the pad.
            float destination = safeS - Direction * 8;
            float recoveryLane = DriveRoad.InBypass(destination) ? (Direction > 0 ? 3.2f : 6.2f) : DriveRoad.TrafficLane(destination,Direction,pace>.955f);
            var p = DriveRoad.At(destination, out var f) + Vector3.Cross(Vector3.up, f).normalized * recoveryLane + Vector3.up;
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

            if(!racing)GetComponent<AmbientVehicle>()?.Recycle(p);
            Place(destination, recoveryLane);
            stalled = 0;
            RecoveryCount++;
            if (Racer != null)
            {
                Racer.Recoveries++;
                Racer.Travel = 0;
                Racer.RecoveryStart = DriveRoad.Project(Car.Body.position, out _);
            }
        }
        void TryRecycleHighway()
        {
            float destination=Direction>0?3760:4600;
            float side=DriveRoad.TrafficLane(destination,Direction,pace>.955f);
            var p=DriveRoad.At(destination,out var f)+Vector3.Cross(Vector3.up,f).normalized*side;
            var camera=Camera.main;
            foreach(var point in new[]{p,transform.position})
            {
                float distance=Vector3.Distance(point,Race.vehicle.transform.position);
                if(distance<220) return;
                // A far horizon point must not hold the entire highway population on
                // neighborhood roads. Visible recycling remains excluded within 500m.
                if(camera && distance<500) { var v=camera.WorldToViewportPoint(point); if(v.z>0&&v.x>-.15f&&v.x<1.15f&&v.y>-.15f&&v.y<1.15f)return; }
            }
            var right=Vector3.Cross(Vector3.up,f).normalized;
            foreach(var d in Race.Drivers) if(d&&d!=this)
            {
                var delta=d.transform.position-p;
                if(delta.magnitude<7 || Mathf.Abs(Vector3.Dot(delta,f))<55 && Mathf.Abs(Vector3.Dot(delta,right))<2.8f)return;
            }
            MinimumRecyclePlayerDistance=Mathf.Min(MinimumRecyclePlayerDistance,
                Vector3.Distance(p,Race.vehicle.transform.position),Vector3.Distance(transform.position,Race.vehicle.transform.position));
            HighwayRecycles++;
            GetComponent<AmbientVehicle>()?.Recycle(p);
            Place(destination,side);
            // Only pooled ambient traffic enters at cruising pace; racers never use this path.
            Car.Body.linearVelocity=f*Direction*Mathf.Min(25,Car.topSpeed*.6f);
        }
        bool LaneClear(float s,float targetLane,float speed)
        {
            var center=DriveRoad.At(s,out var forward);var right=Vector3.Cross(Vector3.up,forward).normalized;
            bool Blocks(ArcadeVehicle other)
            {
                var delta=other.Body.position-Car.Body.position;float along=Vector3.Dot(delta,forward);
                float lanePosition=Vector3.Dot(other.Body.position-center,right);
                return along>-12&&along<Mathf.Max(18,speed*.9f)&&Mathf.Abs(lanePosition-targetLane)<2.5f;
            }
            if(Car!=Race.vehicle&&Blocks(Race.vehicle))return false;
            foreach(var other in Race.Drivers)if(other&&other!=this&&Blocks(other.Car))return false;
            return true;
        }
    }
}
