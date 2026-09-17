using UnityEngine;
namespace Racer.Editor {
// CR-016 authored yard: footprint area is 25% of the previous radius-27 capsule.
public static class CompactYard {
 public static readonly Vector3 Center=new(415.8f,0,-1.1f);
 public static readonly Vector3 House=new(415.8f,0,-13);
 public const float Radius=26.76703f;
 public static float Distance(Vector3 p){p.y=0;return Vector3.Distance(p,Center);}
 public static float OldDistance(Vector3 p){var a=new Vector3(352,0,105.6f);var v=Center-a;p.y=0;return (p-a-v*Mathf.Clamp01(Vector3.Dot(p-a,v)/v.sqrMagnitude)).magnitude;}
 static readonly Vector3 AccessStart=House+new Vector3(15,0,8);
 static readonly Vector3 AccessEnd=RoadPoint();
 static Vector3 RoadPoint(){StreetLoopBuilder.Nearest(AccessStart,StreetLoopBuilder.Route(),out var p);p.y=0;return p;}
 public static float AccessDistance(Vector3 p){var a=AccessStart;var b=AccessEnd;p.y=a.y=b.y=0;var v=b-a;return (p-a-v*Mathf.Clamp01(Vector3.Dot(p-a,v)/v.sqrMagnitude)).magnitude;}
}
}

