using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using Racer;using Object=UnityEngine.Object;
public static class CheckExitDriving {
 public static string Main(string stage="before"){
 if(Application.isPlaying)throw new Exception("Edit mode required");string original=UnityEngine.SceneManagement.SceneManager.GetActiveScene().path;var mode=Physics.simulationMode;float volume=AudioListener.volume;var rows=new List<string>();
 try {AudioListener.volume=0;Physics.simulationMode=SimulationMode.Script;
 foreach(bool reverse in new[]{false,true}){
 EditorSceneManager.OpenScene(reverse?"Assets/Scenes/StreetLoopReverse.unity":"Assets/Scenes/StreetLoopGreybox.unity");var race=Object.FindAnyObjectByType<RaceDirector>();var car=race.vehicle;WoodlandRoute route;
 if(reverse){var atlas=JsonUtility.FromJson<InspectRouteAtlas.Atlas>(File.ReadAllText("Docs/ForestWaterJump/routes-current.json"));route=new GameObject("Temporary Forward exit test centreline").AddComponent<WoodlandRoute>();route.points=atlas.courses.Single(c=>c.scene=="StreetLoopGreybox").routes.Single(r=>r.name=="Pine Ridge").points;}
 else route=Object.FindObjectsByType<WoodlandRoute>().Single(b=>b.title=="Pine Ridge");
 if(stage=="before"&&GameObject.Find("Pine Ridge one-way exit"))GameObject.Find("Pine Ridge one-way exit").SetActive(false);
 typeof(ArcadeVehicle).GetMethod("Awake",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).Invoke(car,null);(car.GetComponent<VehicleConfiguration>()??car.gameObject.AddComponent<VehicleConfiguration>()).Apply("moto");
 foreach(var body in Object.FindObjectsByType<Rigidbody>())if(body!=car.Body)body.gameObject.SetActive(false);
 float station=route.Length-(reverse?37:74);var p=route.At(station,out var f);if(reverse)f=-f;car.Body.position=p+Vector3.up*.7f;car.Body.rotation=Quaternion.LookRotation(Vector3.ProjectOnPlane(f,Vector3.up));car.transform.SetPositionAndRotation(car.Body.position,car.Body.rotation);car.Body.linearVelocity=f*8;car.Body.angularVelocity=Vector3.zero;Physics.SyncTransforms();
 float max=station,min=station,air=0,maxAir=0,maxHeight=0;bool passed=false;float last=station;
 for(int frame=0;frame<750;frame++){
 float s=route.Project(car.Body.position,out var lateral);max=Math.Max(max,s);min=Math.Min(min,s);last=s;
 Vector3 target;if(!reverse&&s>route.Length-3)target=race.road.At(route.exitRoad+12,out _);else target=route.At(s+(reverse?-7:7),out _);
 var local=car.transform.InverseTransformPoint(target);float speed=car.Body.linearVelocity.magnitude;float angle=Mathf.Atan2(local.x,local.z);float maxAngle=Mathf.Lerp(car.slowSteerAngle,car.fastSteerAngle,Mathf.Clamp01(speed/car.topSpeed))*Mathf.Deg2Rad;float steer=Mathf.Clamp(Mathf.Atan(2*car.wheelbase*Mathf.Sin(angle)/7)/maxAngle,-1,1);
 car.Simulate(speed<10?.5f:0,speed>12?.25f:0,steer,.02f);Physics.Simulate(.02f);Physics.SyncTransforms();
 if(car.GroundedWheels==0)air+=.02f;else air=0;maxAir=Math.Max(maxAir,air);maxHeight=Math.Max(maxHeight,car.Body.position.y-route.At(s,out _).y);
 if(!reverse&&s>=route.Length-1){passed=true;break;}
 if(reverse&&s<route.Length-60){passed=true;break;}
 }
 bool ok=reverse?(stage=="before"?passed:!passed&&min>route.Length-52):passed;
 rows.Add($"{(ok?"PASS":"FAIL")} {(reverse?"Reverse entry":"Forward exit+turn")}: initial={station:F2}; last={last:F2}; min={min:F2}; max={max:F2}; reached={passed}; maxContinuousAir={maxAir:F2}s; maxHeightAboveOriginal={maxHeight:F2}m; unchanged moto physics / ordinary 0.02s input+simulation.");
 }
 File.WriteAllLines("Docs/FiveUpdates/exit-driving-"+stage+".txt",rows);return string.Join("\n",rows);
 }finally{Physics.simulationMode=mode;AudioListener.volume=volume;EditorSceneManager.OpenScene(original);}
 }
}
