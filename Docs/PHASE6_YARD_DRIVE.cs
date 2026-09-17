UnityEditor.EditorApplication.isPaused=true;
var car=UnityEngine.Object.FindAnyObjectByType<Racer.ArcadeVehicle>();var body=car.Body;var mode=UnityEngine.Physics.simulationMode;var interp=body.interpolation;bool enabled=car.enabled;var respawn=car.GetComponent<Racer.VehicleRespawn>();bool resetEnabled=respawn.enabled;var log=new System.Collections.Generic.List<string>();
try{
 UnityEngine.Physics.simulationMode=UnityEngine.SimulationMode.Script;body.interpolation=UnityEngine.RigidbodyInterpolation.None;car.enabled=false;respawn.enabled=false;
 var dir=(Racer.Editor.Phase6Buildings.FormerOne-Racer.Editor.Phase6Buildings.Dan).normalized;
 foreach(bool reverse in new[]{false,true}){
  var a=Racer.Editor.Phase6Buildings.Dan+dir*20;var b=Racer.Editor.Phase6Buildings.FormerOne+dir*12;if(reverse){var temp=a;a=b;b=temp;}var f=(b-a).normalized;
  body.position=new UnityEngine.Vector3(a.x,Racer.Editor.Phase6Buildings.Ground(a)+.65f,a.z);body.rotation=UnityEngine.Quaternion.LookRotation(f);car.transform.SetPositionAndRotation(body.position,body.rotation);body.linearVelocity=body.angularVelocity=UnityEngine.Vector3.zero;car.ClearSteering();UnityEngine.Physics.SyncTransforms();
  for(int i=0;i<75;i++){car.Simulate(0,0,0,.02f);UnityEngine.Physics.Simulate(.02f);}float peak=0,up=1,error=0;bool reached=false;int steps=0;
  for(;steps<1400;steps++){
   float along=UnityEngine.Vector3.Dot(body.position-a,f);if(along>UnityEngine.Vector3.Distance(a,b)){reached=true;break;}
   var target=a+f*UnityEngine.Mathf.Clamp(along+8,0,UnityEngine.Vector3.Distance(a,b));target.y=body.position.y;var local=car.transform.InverseTransformPoint(target);float curve=2*local.x/UnityEngine.Mathf.Max(1,local.x*local.x+local.z*local.z);float angle=UnityEngine.Mathf.Lerp(car.slowSteerAngle,car.fastSteerAngle,UnityEngine.Mathf.Clamp01(UnityEngine.Mathf.Abs(car.ForwardSpeed)/car.topSpeed));float steer=UnityEngine.Mathf.Clamp(UnityEngine.Mathf.Atan(curve*car.wheelbase)*UnityEngine.Mathf.Rad2Deg/angle,-1,1);float speed=car.ForwardSpeed;
   car.Simulate(UnityEngine.Mathf.Clamp01((10-speed)*.7f+.35f),speed>10.3f?UnityEngine.Mathf.Clamp01((speed-10)*.5f):0,steer,.02f);UnityEngine.Physics.Simulate(.02f);peak=UnityEngine.Mathf.Max(peak,car.ForwardSpeed);up=UnityEngine.Mathf.Min(up,car.transform.up.y);var delta=body.position-(a+f*UnityEngine.Vector3.Dot(body.position-a,f));delta.y=0;error=UnityEngine.Mathf.Max(error,delta.magnitude);
  }
  log.Add($"{(reached&&up>.8f&&error<3?"PASS":"FAIL")} expanded-yard {(reverse?"return toward Dan":"toward former House 1")}: reached={reached}; time={steps*.02f:F2}s; peak={peak:F2}m/s; min upright={up:F3}; max lateral error={error:F3}m. From rest, virtual motor input / manually stepped PhysX.");
 }
}finally{UnityEngine.Physics.simulationMode=mode;body.interpolation=interp;car.enabled=enabled;respawn.enabled=resetEnabled;UnityEngine.Object.FindAnyObjectByType<Racer.RaceDirector>().RestartRace();System.IO.File.WriteAllLines("Docs/PHASE6_YARD_DRIVING.txt",log);}
return string.Join("\n",log);
