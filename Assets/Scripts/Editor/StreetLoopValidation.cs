using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object=UnityEngine.Object;
namespace Racer.Editor {
public static class StreetLoopValidation {
 [MenuItem("Racer/Validate Phase 2 (Play mode)")]
 public static void Run() {
  if(!Application.isPlaying || UnityEngine.SceneManagement.SceneManager.GetActiveScene().path!=StreetLoopBuilder.ScenePath)throw new InvalidOperationException("Play StreetLoopGreybox first.");
  var car=Object.FindAnyObjectByType<ArcadeVehicle>();var body=car.Body;var reset=car.GetComponent<VehicleRespawn>();
  var route=StreetLoopBuilder.Route();var report=new List<string>();int failures=0;
  void Check(bool pass,string s){report.Add((pass?"PASS: ":"FAIL: ")+s);if(!pass)failures++;}
  float length=0,grade=0;int blocked=0,missing=0;
  for(int i=0;i<route.Count;i++){
   var delta=route[(i+1)%route.Count]-route[i];length+=delta.magnitude;grade=Mathf.Max(grade,Mathf.Abs(delta.y)/new Vector2(delta.x,delta.z).magnitude);
   var side=Vector3.Cross(Vector3.up,delta).normalized;
   foreach(float offset in new[]{-3f,0f,3f}) {
    var p=route[i]+side*offset;
    if(!Physics.Raycast(p+Vector3.up*3,Vector3.down,out var hit,4,1) || Mathf.Abs(hit.point.y-p.y)>.2f)missing++;
   }
   if(Physics.CheckBox(route[i]+Vector3.up*2,new Vector3(3,1.5f,.8f),Quaternion.LookRotation(delta),1))blocked++;
  }
  Check(missing==0,"Continuous road support across 6m driving corridor: missing="+missing);
  Check(blocked==0,"Clear driving corridor: blocked="+blocked);
  report.Add($"Road length {length:F1}m; max local grade {grade*100:F1}%.");
  var oldMode=Physics.simulationMode;var oldInterpolation=body.interpolation;bool oldEnabled=car.enabled,oldReset=reset.enabled;
  try {
   Physics.simulationMode=SimulationMode.Script;body.interpolation=RigidbodyInterpolation.None;car.enabled=false;reset.enabled=false;
   foreach(int direction in new[]{1,-1}) {
    int index=10,progress=0;float maxError=0,minUp=1;int airborne=0,steps=0;
    var forward=route[(index+direction+route.Count)%route.Count]-route[index];
    body.position=route[index]+Vector3.up*.7f;body.rotation=Quaternion.LookRotation(forward);body.linearVelocity=Vector3.zero;body.angularVelocity=Vector3.zero;car.transform.SetPositionAndRotation(body.position,body.rotation);car.ClearSteering();Physics.SyncTransforms();
    for(;steps<60000 && progress<route.Count;steps++) {
     float best=float.MaxValue;int advance=0;
     for(int j=-4;j<=18;j++){int k=(index+direction*j+route.Count)%route.Count;var diff=body.position-route[k];diff.y=0;if(diff.sqrMagnitude<best){best=diff.sqrMagnitude;advance=j;}}
     index=(index+direction*advance+route.Count)%route.Count;progress+=advance;maxError=Mathf.Max(maxError,Mathf.Sqrt(best));minUp=Mathf.Min(minUp,Vector3.Dot(car.transform.up,Vector3.up));if(car.GroundedWheels<2)airborne++;
     int look=3;var target=route[(index+direction*look+route.Count)%route.Count];var local=car.transform.InverseTransformPoint(target);
     float curvature=2*local.x/Mathf.Max(1,local.x*local.x+local.z*local.z);
     float desired=Mathf.Abs(curvature)>.025f?4.5f:8;
     float speed=car.ForwardSpeed;float angle=Mathf.Lerp(car.slowSteerAngle,car.fastSteerAngle,Mathf.Clamp01(Mathf.Abs(speed)/car.topSpeed));
     float steer=Mathf.Clamp(Mathf.Atan(curvature*car.wheelbase)*Mathf.Rad2Deg/angle,-1,1);
     float throttle=Mathf.Clamp01((desired-speed)*.6f+.35f);float brake=speed>desired+1?Mathf.Clamp01((speed-desired)*.3f):0;
     car.Simulate(throttle,brake,steer,.02f);Physics.Simulate(.02f);
     if(maxError>8 || body.position.y < -10 || minUp<.5f)break;
    }
    Check(progress>=route.Count && maxError<3.5f && minUp>.8f,$"Full loop direction {direction}: progress {progress}/{route.Count}, time {steps*.02f:F1}s, max centre error {maxError:F2}m, min upright {minUp:F3}, airborne steps {airborne}. Real Phase 1 forces; no teleport during traversal.");
   }
  } finally {Physics.simulationMode=oldMode;body.interpolation=oldInterpolation;car.enabled=oldEnabled;reset.enabled=oldReset;reset.ResetVehicle();}
  report.Add("Failures: "+failures);File.WriteAllLines("Docs/PHASE2_TEST_RESULTS.txt",report);Debug.Log(string.Join("\n",report));
 }
}
}

