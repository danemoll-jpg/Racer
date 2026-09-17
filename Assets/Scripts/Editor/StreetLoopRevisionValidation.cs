using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using Object=UnityEngine.Object;
namespace Racer.Editor {
public static class StreetLoopRevisionValidation {
 public static void Controls(){
  var car=Object.FindAnyObjectByType<ArcadeVehicle>();var input=car.GetComponent<VehicleInput>();var reset=car.GetComponent<VehicleRespawn>();var camera=Object.FindAnyObjectByType<ChaseCamera>();var lines=new List<string>();
  var keyboard=UnityEngine.InputSystem.InputSystem.AddDevice<UnityEngine.InputSystem.Keyboard>();
  try {
   void Check(bool ok,string name)=>lines.Add((ok?"PASS ":"FAIL ")+name);
   void Keys(params UnityEngine.InputSystem.Key[] keys){UnityEngine.InputSystem.InputSystem.QueueStateEvent(keyboard,new UnityEngine.InputSystem.LowLevel.KeyboardState(keys));UnityEngine.InputSystem.InputSystem.Update();input.SendMessage("Update");}
   Keys(UnityEngine.InputSystem.Key.W,UnityEngine.InputSystem.Key.D);Check(input.Throttle>.99f&&input.Steering>.99f,"Virtual keyboard W/D");
   Keys(UnityEngine.InputSystem.Key.S,UnityEngine.InputSystem.Key.A);Check(input.BrakeReverse>.99f&&input.Steering<-.99f,"Virtual keyboard S/A");
   Keys(UnityEngine.InputSystem.Key.UpArrow,UnityEngine.InputSystem.Key.LeftArrow);Check(input.Throttle>.99f&&input.Steering<-.99f,"Virtual keyboard arrows");
   car.Body.position+=Vector3.right*4;car.Body.linearVelocity=Vector3.one*5;Keys(UnityEngine.InputSystem.Key.R);reset.SendMessage("FixedUpdate");Check(Vector3.Distance(car.Body.position,reset.spawnPoint.position)<.01f&&car.Body.linearVelocity.sqrMagnitude<.001f,"R resets to original fixed-spawn system with cleared momentum");
   var expected=car.transform.position+Quaternion.Euler(0,car.transform.eulerAngles.y,0)*camera.offset;Check(Vector3.Distance(camera.transform.position,expected)<.05f,"Camera snaps on reset");Keys();
   camera.SendMessage("LateUpdate");Check(Vector3.Distance(camera.transform.position,car.transform.position)<12,"Chase camera follow remains near car");
  }finally{UnityEngine.InputSystem.InputSystem.RemoveDevice(keyboard);UnityEngine.InputSystem.InputSystem.Update();input.SendMessage("Update");reset.ResetVehicle();}
  lines.Add("Virtual keyboard only; physical keyboard interaction and physical controller not exercised by this test.");File.WriteAllLines("Docs/PHASE2_REVISION_CONTROLS.txt",lines);
 }
 [MenuItem("Racer/Validate Phase 2 Shoulders (Play mode)")]
 public static void RunShoulders()=>Run();
 public static void Run(int only=-1){
  if(!Application.isPlaying)throw new InvalidOperationException("Play revised scene first");
  var report=new List<string>();var r=StreetLoopBuilder.Route();var car=Object.FindAnyObjectByType<ArcadeVehicle>();var body=car.Body;var reset=car.GetComponent<VehicleRespawn>();int failures=0;
  void Check(bool good,string s){report.Add((good?"PASS ":"FAIL ")+s);if(!good)failures++;}
  float worst=0;int missing=0,overlap=0;string where="";
  for(int i=0;i<r.Count;i+=5){var side=Vector3.Cross(Vector3.up,r[(i+1)%r.Count]-r[i]).normalized;float last=0;
   for(int j=-64;j<=64;j++){var p=r[i]+side*(j*.25f);var hits=Physics.RaycastAll(p+Vector3.up*20,Vector3.down,60,1);if(hits.Length==0){missing++;continue;}Array.Sort(hits,(a,b)=>a.distance.CompareTo(b.distance));if(hits.Length>1&&Mathf.Abs(hits[0].point.y-hits[1].point.y)>.01f)overlap++;float y=hits[0].point.y;if(j>-64&&Mathf.Abs(y-last)>worst){worst=Mathf.Abs(y-last);where=$"sample {i} offset {j*.25f}";}last=y;}
  }
  Check(missing==0&&overlap==0,$"Lateral +/-16m scan every 5 route samples, 0.25m across: missing={missing}, stacked hits={overlap}, max adjacent rise={worst:F3}m ({where}).");
  var oldMode=Physics.simulationMode;var oldInterp=body.interpolation;bool enabled=car.enabled,resetEnabled=reset.enabled;
  try{Physics.simulationMode=SimulationMode.Script;body.interpolation=RigidbodyInterpolation.None;car.enabled=false;reset.enabled=false;
   // Real force integration. Teleport only to initialise independent cases; never during traversal.
   foreach(int sample in new[]{30,110,180,260,350,440,520,620,700,900,1150,1400,1700,1950,2200})foreach(int sign in new[]{-1,1})foreach(float speed in new[]{3f,8f,15f})foreach(float angle in new[]{25f,55f}){
    if(only>=0 && sample!=only)continue; int at=sample%r.Count;var f=(r[(at+1)%r.Count]-r[at]);f.y=0;f.Normalize();var side=Vector3.Cross(Vector3.up,f).normalized;var direction=(f*Mathf.Cos(angle*Mathf.Deg2Rad)-side*sign*Mathf.Sin(angle*Mathf.Deg2Rad)).normalized;
    // Follow the horizontal bend while sweeping across +/-15m. Straight chords can
    // leave the intended shoulder and strike trees or steep natural slopes on bends.
    var path=new List<Vector3>();int pathSteps=Mathf.CeilToInt(30/Mathf.Tan(angle*Mathf.Deg2Rad)/2);
    for(int k=0;k<=pathSteps;k++){int ri=(at+k)%r.Count;var tangent=r[(ri+1)%r.Count]-r[(ri+r.Count-1)%r.Count];var lateral=Vector3.Cross(Vector3.up,tangent).normalized;path.Add(r[ri]+lateral*sign*Mathf.Lerp(15,-15,k/(float)pathSteps));}
    var start=path[0];direction=Vector3.ProjectOnPlane(path[1]-path[0],Vector3.up).normalized;int pathIndex=0;
    Physics.Raycast(start+Vector3.up*30,Vector3.down,out var support,100,1);body.position=support.point+Vector3.up*.65f;body.rotation=Quaternion.LookRotation(direction);body.linearVelocity=Vector3.zero;body.angularVelocity=Vector3.zero;car.transform.SetPositionAndRotation(body.position,body.rotation);car.ClearSteering();Physics.SyncTransforms();
    int under=0,air=0,consecutive=0,maxAir=0;float minUp=1,minClear=10;bool completed=false;
    for(int step=0;step<5000;step++){float best=float.MaxValue;int next=pathIndex;for(int k=pathIndex;k<Mathf.Min(path.Count,pathIndex+8);k++){var delta=body.position-path[k];delta.y=0;if(delta.sqrMagnitude<best){best=delta.sqrMagnitude;next=k;}}pathIndex=next;
     float s=car.ForwardSpeed;float desired=step<50?0:speed;var local=car.transform.InverseTransformPoint(path[Mathf.Min(path.Count-1,pathIndex+2)]);float curvature=2*local.x/Mathf.Max(1,local.x*local.x+local.z*local.z);float steeringAngle=Mathf.Lerp(car.slowSteerAngle,car.fastSteerAngle,Mathf.Clamp01(Mathf.Abs(s)/car.topSpeed));float steering=Mathf.Clamp(Mathf.Atan(curvature*car.wheelbase)*Mathf.Rad2Deg/steeringAngle,-1,1);car.Simulate(Mathf.Clamp01((desired-s)*.6f+.35f),s>desired+1?Mathf.Clamp01((s-desired)*.4f):0,steering,.02f);Physics.Simulate(.02f);
     if(step>50){minUp=Mathf.Min(minUp,Vector3.Dot(car.transform.up,Vector3.up));if(car.GroundedWheels<2){air++;consecutive++;maxAir=Mathf.Max(maxAir,consecutive);}else consecutive=0;if(Physics.Raycast(body.position+Vector3.up*10,Vector3.down,out var h,100,1)){float clearance=body.position.y-h.point.y;minClear=Mathf.Min(minClear,clearance);if(clearance<0)under++;}else under++;}
     if(pathIndex>=path.Count-2){completed=true;break;}if(body.position.y<-20||minUp<.5f)break;
    }
    Check(completed&&under==0&&minUp>.8f&&maxAir<15,$"Re-entry sample={at} side={sign} speed={speed} angle={angle}: crossed={completed} belowSurfaceSteps={under} minClear={minClear:F3} minUp={minUp:F3} airborneSteps={air} longest={maxAir} end={body.position} speedEnd={car.ForwardSpeed:F2}.");
   }
  }finally{Physics.simulationMode=oldMode;body.interpolation=oldInterp;car.enabled=enabled;reset.enabled=resetEnabled;reset.ResetVehicle();}
  report.Add("Failures: "+failures);File.WriteAllLines("Docs/PHASE2_REVISION_REENTRY.txt",report);
 }
 public static void Capture(){
  var car=Object.FindAnyObjectByType<ArcadeVehicle>();var cam=Camera.main;var cp=cam.transform.position;var cr=cam.transform.rotation;
  void Shot(string name,Vector3 p,Vector3 target,bool ortho=false){cam.transform.position=p;cam.transform.LookAt(target);cam.orthographic=ortho;cam.orthographicSize=820;var rt=new RenderTexture(1440,1000,24);var previous=cam.targetTexture;cam.targetTexture=rt;cam.Render();var active=RenderTexture.active;RenderTexture.active=rt;var image=new Texture2D(1440,1000,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1440,1000),0,0);image.Apply();File.WriteAllBytes("Docs/Revision_"+name+".png",image.EncodeToPNG());cam.targetTexture=previous;RenderTexture.active=active;Object.DestroyImmediate(image);Object.DestroyImmediate(rt);}
  Shot("Overview",new Vector3(0,1500,0),Vector3.zero,true);cam.orthographic=false;
  var r=StreetLoopBuilder.Route();foreach(int i in new[]{100,300,470,620,1150,1450,2150}){var forward=(r[i+3]-r[i]).normalized;car.Body.position=r[i]+Vector3.up*.65f;car.Body.rotation=Quaternion.LookRotation(Vector3.ProjectOnPlane(forward,Vector3.up));car.Body.linearVelocity=Vector3.zero;car.Body.angularVelocity=Vector3.zero;car.transform.SetPositionAndRotation(car.Body.position,car.Body.rotation);Physics.SyncTransforms();Object.FindAnyObjectByType<ChaseCamera>().Snap();Shot("Road_"+i,cam.transform.position,car.transform.position+Vector3.up*.7f+Vector3.ProjectOnPlane(forward,Vector3.up).normalized*3);}
  foreach(string name in new[]{"Original house 3","Friend across street - blue circle"}){var h=GameObject.Find(name);StreetLoopBuilder.Nearest(h.transform.position,r,out var near);Shot(name.Contains("3")?"House3":"Friend",near+Vector3.up*3.6f,h.transform.position+Vector3.up*3);}
  cam.transform.SetPositionAndRotation(cp,cr);cam.orthographic=false;car.GetComponent<VehicleRespawn>().ResetVehicle();
 }
}
}




