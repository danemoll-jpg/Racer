using UnityEngine;
namespace Racer
{
    // Attached only to the two Backyard Reverse branches. Uses ordinary steering
    // and pedals; the strategy still makes the existing once-per-lap choice.
    public sealed class ReverseShortcutGuidance : MonoBehaviour
    {
        public float lookAhead=7;
        public float takeoff=-1, landing=-1;
        public bool Flight(float s)=>takeoff>=0&&s>=takeoff-15&&s<landing+4;
        public Vector3 Target(WoodlandRoute route,Vector3 position,float s,float look)
        {
            if(!Flight(s))return route.At(s+look,out _);
            var start=route.At(takeoff,out var axis);
            axis=Vector3.ProjectOnPlane(axis,Vector3.up).normalized;
            var target=start+axis*(Vector3.Dot(position-start,axis)+look);
            target.y=position.y;
            return target;
        }
    }
}
