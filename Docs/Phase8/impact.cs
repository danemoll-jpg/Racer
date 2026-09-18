var flow=UnityEngine.Object.FindAnyObjectByType<Racer.RaceFlow>();
if(flow.State!=Racer.RaceFlow.Stage.Racing)throw new Exception("Wait for countdown");
var car=flow.Race.vehicle;car.enabled=true;car.GetComponent<Racer.VehicleRespawn>().enabled=true;
var prop=UnityEngine.Object.FindObjectsByType<Racer.BreakableProp>().First(p=>p.name=="Mailbox - Dan - blue X");
var forward=Vector3.ProjectOnPlane(prop.transform.forward,Vector3.up).normalized;
var p=prop.GetComponent<BoxCollider>().bounds.center-forward*4;
var ground=UnityEngine.Physics.RaycastAll(p+Vector3.up*100,Vector3.down,300,1).First(h=>h.collider.transform.root.name=="Memory loop - north is +Z");
car.Body.position=ground.point+Vector3.up*.7f;car.Body.rotation=Quaternion.LookRotation(forward);car.transform.SetPositionAndRotation(car.Body.position,car.Body.rotation);car.Body.linearVelocity=forward*6;car.Body.angularVelocity=Vector3.zero;car.ClearSteering();Physics.SyncTransforms();Camera.main.GetComponent<Racer.ChaseCamera>().Snap();
var pad=UnityEngine.InputSystem.InputSystem.AddDevice<UnityEngine.InputSystem.Gamepad>();
float start=Time.time,minUp=1;int count=prop.BreakCount,last=-1;bool restarted=false,broken=false;
UnityEditor.EditorApplication.CallbackFunction tick=null;
tick=()=>{if(last==Time.frameCount)return;last=Time.frameCount;minUp=Mathf.Min(minUp,car.transform.up.y);
if(!restarted&&Time.time-start<2){UnityEngine.InputSystem.InputSystem.QueueStateEvent(pad,new UnityEngine.InputSystem.LowLevel.GamepadState{rightTrigger=.2f});return;}
if(!restarted){broken=prop.IsBroken&&prop.BreakCount==count+1;UnityEngine.InputSystem.InputSystem.RemoveDevice(pad);flow.StartRace();restarted=true;return;}
UnityEditor.EditorApplication.update-=tick;System.IO.File.WriteAllText("Docs/Phase8/impact.txt",$"{(broken&&!prop.IsBroken&&!prop.PendingRestore&&minUp>.8f?"PASS":"FAIL")} Ordinary-frame virtual Gamepad mailbox impact, initial 6m/s approach; brokenOnce={broken}, restored={!prop.IsBroken&&!prop.PendingRestore}, minimumUpright={minUp:F3}. Production restart restores prop; no physical-controller test.\n");};
UnityEditor.EditorApplication.update+=tick;return "Physical-contact impact test started";
