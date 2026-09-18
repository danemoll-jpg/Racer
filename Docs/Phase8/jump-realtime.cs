var road=Racer.Editor.StreetLoopBuilder.Route();int index=Racer.Editor.Phase5Setup.Closest(road,new UnityEngine.Vector3(-627,0,-350));
var points=Enumerable.Range(0,330).Select(i=>road[(index+i)%road.Count]).ToArray();
var plan=new Racer.WoodlandBenchmark.Plan{seconds=22,routes=new[]{new Racer.WoodlandBenchmark.Route{name="ordinary-frame-jump",points=points,speed=32}}};
System.IO.File.WriteAllText("Docs/Phase8/jump-routes.json",UnityEngine.JsonUtility.ToJson(plan,true));
var benchmark=Racer.WoodlandBenchmark.Begin("Docs/Phase8/jump-routes.json","Docs/Phase8/jump-realtime.csv");benchmark.repeats=1;
var car=UnityEngine.Object.FindAnyObjectByType<Racer.ArcadeVehicle>();float air=0,takeoff=0,landing=0,minUp=1;bool flew=false,landed=false;int last=UnityEngine.Time.frameCount;
UnityEditor.EditorApplication.CallbackFunction tick=null;
tick=()=>{if(UnityEngine.Time.frameCount==last)return;last=UnityEngine.Time.frameCount;minUp=UnityEngine.Mathf.Min(minUp,car.transform.up.y);if(car.Body.position.z>-110&&car.Body.position.z<60){if(car.GroundedWheels<2){if(!flew)takeoff=car.ForwardSpeed;flew=true;air+=UnityEngine.Time.deltaTime;}else if(flew&&!landed){landed=true;landing=car.Body.position.z;}}if(benchmark)return;UnityEditor.EditorApplication.update-=tick;System.IO.File.WriteAllText("Docs/Phase8/jump-ordinary-result.txt",$"{(flew&&landed&&minUp>.8f?"PASS":"FAIL")} ordinary-frame virtual Gamepad jump from rest 240m before ramp: takeoff={takeoff:F2}m/s, air={air:F2}s, landed={landed}, landingZ={landing:F2}, upright={minUp:F3}. No manual Physics.Simulate or injected velocity.\n");};
UnityEditor.EditorApplication.update+=tick;
return "ordinary-frame jump started";



