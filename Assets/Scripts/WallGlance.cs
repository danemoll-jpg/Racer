using System.Collections.Generic;
using UnityEngine;

namespace Racer
{
    // 1.3.0-review8 (Dan, 2026-10-30: "Review the recent bumper/barrier changes implemented in Chastain Rush and apply here (how it handles bumping
    // into one not impeding you)"). Ported from Chastain Rush Round 85 (Vehicle/WallGlance.cs, which came from this game's HIGH MODE). A player's
    // vehicle that hits a guide wall (GuideWalls: HIGH MODE's gutter bumpers, the races' closure barricades and rails, the mountain guardrails,
    // the ROAD CLOSED fences and the bridge rails) at a shallow angle is turned along the wall and keeps most of its speed, eased off it so it does
    // not ride or grind along it:
    // - the speed kept falls with the angle (1 - (1 - cos a) * Loss): 10 degrees keeps about 99 %, 30 degrees about 89 % (normal mode);
    // - normal mode: up to 50 degrees; past that, and for the rest of that touch, it is a crash as before (head on stops you);
    // - HIGH MODE: up to 82 degrees, less lost, and a vehicle pinned slow against a guide wall for 0.6 s is turned down the course (the race's
    //   line, else the wall) and set going at 8 m/s: never stopped against a bumper;
    // - the push is sideways only (no climbing, no launch): the vertical speed is never raised.
    // The players' vehicles only (both split-screen players; RaceFlow.Update adds it); AI racers, traffic and police are not changed.
    // Breakable things (fences meant to break, signs, mailboxes, smash spots, trees) and solid scenery (houses) are not guide walls.
    public sealed class WallGlance : MonoBehaviour
    {
        public const float NormalMax = 50f, HighMax = 82f, NormalLoss = .8f, HighLoss = .6f, Settle = .18f, PinTime = .6f, PinSpeed = 8f;
        ArcadeVehicle car;
        public int Player = 1;
        Vector3 lastVelocity, hitNormal, target; int hitCount; Collider hitCollider; float settleFor, pinnedFor, lastTouch = -10f; bool touching, blocked; // blocked: this touch began head on (a crash): no glance until it leaves the wall
        // checks: every glance (angle in, speed before, speed after)
        public static readonly List<string> Log = new List<string>(); public static int Glances, Pins, Crashes;
        public static float LastAngle, LastBefore, LastAfter;

        public static void Ensure(ArcadeVehicle c, int player)
        {
            if (!c) return; var g = c.GetComponent<WallGlance>(); if (!g) g = c.gameObject.AddComponent<WallGlance>(); g.Player = player;
        }
        public static bool Off = System.Array.IndexOf(System.Environment.GetCommandLineArgs(), "-r8GlanceOff") >= 0; // checks only: the old way, for the before
        bool On => !Off && car && car.Body && !car.Body.isKinematic && !car.Puppet;
        bool High => HighMode.ForPlayer(Player);
        void Awake() { car = GetComponent<ArcadeVehicle>(); }

        void FixedUpdate()
        {
            if (!On) { hitCount = 0; settleFor = 0; pinnedFor = 0; touching = false; if (car && car.Body) lastVelocity = car.Body.linearVelocity; return; }
            var body = car.Body; float dt = Time.fixedDeltaTime;
            if (hitCount > 0) Glance(body);
            // pinned (HIGH MODE): slow against a guide wall: turned down the course and on its way
            if (touching && High && Flat(body.linearVelocity).magnitude < 2.5f) pinnedFor += dt; else pinnedFor = 0;
            if (pinnedFor >= PinTime) Unpin(body);
            // the settle: the heading and the travel held on the wall's line for a moment (the tires would steer it back into the wall otherwise)
            if (settleFor > 0)
            {
                settleFor -= dt;
                var v = body.linearVelocity; var flat = Flat(v); float sp = flat.magnitude;
                if (sp > 1f) { var dir = Vector3.Slerp(flat / sp, target, .5f).normalized; body.linearVelocity = dir * sp + Vector3.up * v.y; }
                YawToward(body, Vector3.Dot(Flat(transform.forward), target) >= 0 ? target : -target, .1f);
            }
            hitCount = 0; touching = false; lastVelocity = body.linearVelocity;
        }

        void Glance(Rigidbody body)
        {
            if (blocked) return; // a head-on hit stays a crash even once the vehicle has swung round against the wall
            var n = Flat(hitNormal); if (n.sqrMagnitude < 1e-4f) return; n.Normalize();
            var before = Flat(lastVelocity); float speed = before.magnitude; if (speed < 3f) return;
            float into = -Vector3.Dot(before, n); if (into <= .2f) return; // already leaving the wall
            var t = Vector3.Cross(Vector3.up, n); float along = Vector3.Dot(before, t);
            if (Mathf.Abs(along) < .5f) along = CourseSign(t);
            if (along < 0) t = -t;
            float angle = Vector3.Angle(before, t); bool high = High;
            if (angle > (high ? HighMax : NormalMax)) { Crashes++; LastAngle = angle; blocked = true; return; } // head on: the wall stops it, as before
            float keep = 1f - (1f - Mathf.Cos(angle * Mathf.Deg2Rad)) * (high ? HighLoss : NormalLoss);
            float after = speed * keep, push = Mathf.Clamp(.8f + .12f * into, .8f, 3f);
            var v = body.linearVelocity; float vy = Mathf.Min(v.y, Mathf.Max(lastVelocity.y, 0f)); // never up off the wall
            body.linearVelocity = t * after + n * push + Vector3.up * vy;
            var w = body.angularVelocity; body.angularVelocity = new Vector3(w.x * .5f, 0f, w.z * .5f);
            target = (t * after + n * push).normalized; settleFor = Settle;
            YawToward(body, Vector3.Dot(Flat(transform.forward), t) >= 0 ? target : -target, .06f);
            Glances++; LastAngle = angle; LastBefore = speed; LastAfter = after;
            if (Log.Count < 400) Log.Add($"{Time.time:0.00} {(high ? "HIGH" : "normal")} {Name(hitCollider)} at ({body.position.x:0.0}, {body.position.z:0.0}): {angle:0} deg, {speed:0.0} -> {after:0.0} m/s");
        }
        // pinned against a guide wall in HIGH MODE: pointed down the course and set going
        void Unpin(Rigidbody body)
        {
            pinnedFor = 0; var n = Flat(hitNormal).normalized; if (n.sqrMagnitude < .5f) return; var t = Vector3.Cross(Vector3.up, n);
            if (CourseSign(t) < 0) t = -t;
            var rot = Quaternion.LookRotation(t, Vector3.up); var pos = body.position + n * .4f;
            body.position = pos; body.rotation = rot; transform.SetPositionAndRotation(pos, rot);
            body.linearVelocity = t * PinSpeed + n * 1f; body.angularVelocity = Vector3.zero; target = (t * PinSpeed + n).normalized; settleFor = Settle;
            Physics.SyncTransforms(); Pins++;
            if (Log.Count < 400) Log.Add($"{Time.time:0.00} HIGH pinned on {Name(hitCollider)} at ({pos.x:0.0}, {pos.z:0.0}): turned down the course");
        }
        // which way along the wall: the race's line where a race runs (its forward), else the vehicle's own heading
        float CourseSign(Vector3 t)
        {
            var race = Hints.Flow ? Hints.Flow.Race : null;
            if (race && !race.FreeRoam && race.road && race.road.points != null && race.road.points.Length > 1)
            {
                float s = race.road.Project(transform.position, out _); race.road.At(s, out var f);
                return Vector3.Dot(Flat(f), t) >= 0 ? 1f : -1f;
            }
            return Vector3.Dot(Flat(transform.forward), t) >= 0 ? 1f : -1f;
        }
        void YawToward(Rigidbody body, Vector3 heading, float seconds)
        {
            float err = Vector3.SignedAngle(Flat(transform.forward), heading, Vector3.up) * Mathf.Deg2Rad;
            var w = body.angularVelocity; w.y = Mathf.Clamp(err / seconds, -9f, 9f); body.angularVelocity = w;
        }
        void OnCollisionEnter(Collision c) => Contact(c);
        void OnCollisionStay(Collision c) => Contact(c);
        void Contact(Collision c)
        {
            if (!On || c.contactCount == 0 || !GuideWalls.Is(c.collider)) return;
            Vector3 sum = Vector3.zero; int k = 0; var com = car.Body.worldCenterOfMass;
            for (int i = 0; i < c.contactCount && i < 6; i++)
            {
                var cp = c.GetContact(i); var nn = cp.normal; if (Mathf.Abs(nn.y) > .5f) continue; // its top, a seam under the wheels: not the wall's face
                // the wall's own face there (a ray from the vehicle's middle at the contact's height): PhysX gives an edge's normal where two pieces meet,
                // which can point back against the travel and read as a head-on hit
                var from = new Vector3(com.x, cp.point.y, com.z); var dir = Flat(cp.point - from);
                if (dir.sqrMagnitude > 1e-4f && c.collider.Raycast(new Ray(from, dir.normalized), out var hit, dir.magnitude + 1.5f) && Mathf.Abs(hit.normal.y) < .5f) nn = hit.normal;
                if (Vector3.Dot(nn, com - cp.point) < 0) nn = -nn; // pointing from the wall to the vehicle
                sum += nn; k++;
            }
            if (k == 0) return;
            if (Time.time - lastTouch > .3f) blocked = false; // a new touch
            lastTouch = Time.time;
            touching = true; hitNormal = hitCount == 0 ? sum : hitNormal + sum; hitCount += k; hitCollider = c.collider;
            GuideWalls.Smooth(c.collider);
        }
        static Vector3 Flat(Vector3 v) { v.y = 0; return v; }
        static string Name(Collider c) => c ? c.name : "?";
    }

    // 1.3.0-review8: which colliders are guide walls (see WallGlance), and their surface made frictionless the first time one is met
    public static class GuideWalls
    {
        static readonly Dictionary<EntityId, bool> known = new Dictionary<EntityId, bool>();
        static PhysicsMaterial slick;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] static void Reset() { known.Clear(); }
        public static bool Is(Collider c)
        {
            if (!c || c.isTrigger || c.attachedRigidbody) return false;
            var id = c.GetEntityId(); if (known.TryGetValue(id, out bool yes)) return yes;
            yes = HighBumpers.IsBumper(c) || Named(c);
            if (known.Count > 20000) known.Clear();
            known[id] = yes; return yes;
        }
        // the barriers by name (the 1.3.0-review8 inventory of every scene): never anything breakable or a sign
        // - Leviathan's closed ways, the Stunt Races' closed shortcuts and Leviathan Reverse's J1 rail (LeviathanCourse.BuildBlocks: one collider a run);
        // - Mountain Loop's "Road closed barrier 4" (race only);
        // - Backyard Reverse's timber closures (race only);
        // - the mountain's natural barriers (the rock and earth banks named "Ground_Report06x/070 barrier ...": only their sides, never their tops).
        // No other guardrail, bridge rail or closure fence in the scenes has a collider (the rest are breakable, signs or buildings).
        static bool Named(Collider c)
        {
            if (c.GetComponentInParent<BreakableProp>() || c.GetComponentInParent<PhysicalSign>()) return false;
            string n = c.name;
            return n == "Barricade collision" || n == LeviathanCourse.BarrierRunName || n == "Barrier collision" || n == "Grounded timber barricade beam"
                || (n.StartsWith("Ground_Report0", System.StringComparison.Ordinal) && n.Contains(" barrier "));
        }
        // no friction against the vehicle's side, no bounce (bumpers have their own material)
        public static void Smooth(Collider c)
        {
            if (HighBumpers.IsBumper(c)) return;
            if (!slick) slick = new PhysicsMaterial("Guide wall") { dynamicFriction = 0f, staticFriction = 0f, frictionCombine = PhysicsMaterialCombine.Minimum, bounciness = 0f, bounceCombine = PhysicsMaterialCombine.Minimum };
            if (c.sharedMaterial != slick) c.sharedMaterial = slick;
        }
    }
}
