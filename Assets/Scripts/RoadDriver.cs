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

        bool racing;
        float pace, stalled, safeS, lane, finishRunoff;
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

            if (Car == Race.vehicle && Racer != null && (Racer.Progress.Finished || Racer.Dnf))
            {
                Car.Body.isKinematic = true;
                return;
            }

            Car.Body.isKinematic = false;
            bool finished = Racer != null && (Racer.Progress.Finished || Racer.Dnf);
            if (finished && finishRunoff > 55)
            {
                Car.Simulate(0, Car.ForwardSpeed > 1 ? 1 : 0, 0, Time.fixedDeltaTime);
                return;
            }

            float s = Race.road.Project(Car.Body.position, out float lateral);
            float speed = Mathf.Abs(Car.ForwardSpeed);
            Race.road.At(s, out var tangent);
            float look = Mathf.Clamp(7 + speed * .48f, 8, 25);
            float desiredLane = lane;
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

                if (slowerAhead && !oncoming)
                    desiredLane = -2.3f;
            }

            var target = Race.road.At(s + Direction * look, out var ahead);
            target += Vector3.Cross(Vector3.up, ahead).normalized * desiredLane;
            var local = transform.InverseTransformPoint(target);
            float angle = Mathf.Atan2(local.x, local.z);
            float maxAngle = Mathf.Lerp(Car.slowSteerAngle, Car.fastSteerAngle, Mathf.Clamp01(speed / Car.topSpeed)) * Mathf.Deg2Rad;
            float steering = Mathf.Clamp(Mathf.Atan(2 * Car.wheelbase * Mathf.Sin(angle) / look) / maxAngle, -1, 1);
            int skill = racing ? Mathf.Clamp(Race.difficulty,0,2) : 0;
            float cornerGrip = racing ? new[]{7.5f,11f,14f}[skill] : 7.5f;
            float judgment = racing ? new[]{8f,12f,15f}[skill] : 8f;
            TargetSpeed = (racing ? Car.topSpeed * .94f : 17) * pace;
            if(finished) TargetSpeed=12;
            // Consistency costs time through early lifting, never extra vehicle capability.
            if (racing && skill < 2) TargetSpeed *= 1 - (2-skill)*.035f*(.5f+.5f*Mathf.Sin(Time.time*.19f+pace*31));
            for (float d = 8; d <= 100; d += 8)
            {
                Race.road.At(s + Direction * d, out var f);
                Race.road.At(s + Direction * (d + 8), out var next);
                float curvature = Vector3.Angle(f, next) * Mathf.Deg2Rad / 8;
                float curveSpeed = Mathf.Sqrt(cornerGrip / Mathf.Max(.005f, curvature));
                float hillSpeed = Mathf.Abs(f.y) > .18f ? 24 : 45;
                TargetSpeed = Mathf.Min(TargetSpeed, Mathf.Sqrt(Mathf.Pow(Mathf.Min(curveSpeed, hillSpeed), 2) + 2 * judgment * Mathf.Max(0, d - 12)));
            }

            if (Mathf.Abs(angle) > .6f)
                TargetSpeed = Mathf.Min(TargetSpeed, 10);
            // Bounded non-alloc obstacle query; includes other cars and solid roadside objects, excludes own body.
            float range = 6 + speed * 1.5f;
            int count = Physics.SphereCastNonAlloc(Car.Body.position + Vector3.up * .35f, .75f, transform.forward, hits, range, ~0, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < count; i++)
            {
                var hit = hits[i];
                if (hit.rigidbody == Car.Body || hit.normal.y > .55f)
                    continue;
                float allowed = Mathf.Sqrt(Mathf.Max(0, 2 * 8 * (hit.distance - 4)));
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

            if (stalled > 12 || lateral > 22 || Vector3.Dot(transform.up, Vector3.up) < .1f)
            {
                TryRecover(s);
            }

            if (lateral < 5 && speed > 4 && Vector3.Dot(transform.forward, tangent * Direction) > .6f)
                safeS = s;
            Car.Simulate(throttle, brake, steering, Time.fixedDeltaTime);
        }

        void TryRecover(float current)
        {
            // Return behind the last stable sample, and never award progress. Wait if any vehicle occupies the pad.
            float destination = safeS - Direction * 8;
            float recoveryLane = Race.road.InBypass(destination) ? (Direction > 0 ? 3.2f : 6.2f) : lane;
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
