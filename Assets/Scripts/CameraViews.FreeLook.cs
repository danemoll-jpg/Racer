using UnityEngine;

namespace Racer
{
    // 0.100 Part E: free look while driving. Each player's own right stick swings the view the way it is pushed (left, right,
    // down = behind; part-way = part of the way), R3 held looks straight behind, and on the keyboard the right mouse button held
    // with the mouse moved turns it (and tilts it a little). Chase views swing the camera around the vehicle; first person and
    // the front view turn the head. Let go and it eases back behind the vehicle in under a second. Driving is not affected
    // (nothing else reads the right stick, R3 or the right mouse button while driving). Not in Trailer Mode (its own cameras),
    // a menu, the world map, or outside the countdown and the race / Free Roam.
    public sealed partial class CameraViews
    {
        float lookYaw, lookPitch, lookYawSpeed, lookPitchSpeed, mouseYaw, mousePitch;
        bool Looking => Mathf.Abs(lookYaw) > .3f || Mathf.Abs(lookPitch) > .3f;
        void UpdateFreeLook(ArcadeVehicle car)
        {
            float yaw = 0, pitch = 0; bool held = false;
            bool driving = flow.State == RaceFlow.Stage.Racing || flow.State == RaceFlow.Stage.Countdown;
            var input = car ? car.GetComponent<VehicleInput>() : null;
            if (input && input.enabled && driving && !TrailerMode.Active && !MenuInput.Blocked && flow.GetComponent<ExplorationMap>()?.OwnsInput != true)
            {
                var stick = input.Look; float amount = Mathf.Clamp01((stick.magnitude - .15f) / .7f);
                if (input.LookBack) { yaw = 180; held = true; }
                else if (amount > 0) { yaw = Mathf.Atan2(stick.x, stick.y) * Mathf.Rad2Deg * amount; held = true; }
                if (input.MouseLook)
                {
                    mouseYaw = Mathf.Clamp(mouseYaw + input.MouseDelta.x * .2f, -180, 180); mousePitch = Mathf.Clamp(mousePitch - input.MouseDelta.y * .12f, -20, 30);
                    if (!held) { yaw = mouseYaw; pitch = mousePitch; held = true; }
                }
                else mouseYaw = mousePitch = 0;
            }
            else mouseYaw = mousePitch = 0;
            float smooth = held ? .1f : .22f;
            lookYaw = Mathf.SmoothDampAngle(lookYaw, yaw, ref lookYawSpeed, smooth, Mathf.Infinity, Time.unscaledDeltaTime);
            lookPitch = Mathf.SmoothDamp(lookPitch, pitch, ref lookPitchSpeed, smooth, Mathf.Infinity, Time.unscaledDeltaTime);
            if (!held && Mathf.Abs(Mathf.DeltaAngle(lookYaw, 0)) < .2f && Mathf.Abs(lookPitch) < .2f) { lookYaw = lookPitch = lookYawSpeed = lookPitchSpeed = 0; }
            lookYaw = Mathf.DeltaAngle(0, lookYaw);
        }
        Pose DriveLook(Pose pose, ArcadeVehicle car, bool head)
        {
            if (!Looking || !car) return pose;
            var turn = Quaternion.AngleAxis(lookYaw, Vector3.up);
            if (head) { pose.rotation = turn * pose.rotation * Quaternion.Euler(lookPitch, 0, 0); return pose; }
            var pivot = car.transform.position + Vector3.up;
            var offset = turn * (pose.position - pivot); float length = offset.magnitude;
            // kept clear of walls and banks like the chase camera itself
            if (length > .01f && Physics.SphereCast(pivot, chase.collisionRadius, offset / length, out var hit, length, chase.obstructionMask, QueryTriggerInteraction.Ignore))
                offset = offset / length * Mathf.Max(0, hit.distance - .1f);
            pose.position = pivot + offset; pose.rotation = turn * pose.rotation * Quaternion.Euler(lookPitch, 0, 0);
            return pose;
        }
    }
}
