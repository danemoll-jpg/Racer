#if UNITY_EDITOR
using System.IO;using UnityEngine;
namespace Racer{public sealed class RegressionContactLog:MonoBehaviour{public StreamWriter log;public RaceRoad road;
 void OnCollisionStay(Collision c){if(log==null)return;float s=road.Project(transform.position,out float d);if(s<880||s>1000)return;foreach(var p in c.contacts){var lp=transform.InverseTransformPoint(p.point);log.WriteLine($"{Time.time:F3},{s:F2},{c.collider.name},{lp.x:F2},{lp.y:F2},{lp.z:F2},{p.normal.y:F2},{c.impulse.magnitude:F1}");}}
 void FixedUpdate(){if(log==null)return;float s=road.Project(transform.position,out _);if(s<880||s>1000)return;var car=GetComponent<ArcadeVehicle>();foreach(var local in car.suspensionPoints){var o=transform.TransformPoint(local);if(Physics.Raycast(o,-transform.up,out var h,car.suspensionLength,car.groundMask,QueryTriggerInteraction.Ignore))log.WriteLine($"{Time.time:F3},{s:F2},WHEEL {h.collider.name},{local.x:F2},{local.y:F2},{local.z:F2},{h.normal.y:F2},{h.distance:F2}");}}
}}
#endif
