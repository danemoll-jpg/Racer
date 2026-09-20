using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
namespace Racer
{
    public sealed class DiscoverySystemsValidation:MonoBehaviour
    {
        static string Arg(string key,string fallback=""){var a=Environment.GetCommandLineArgs();int i=Array.IndexOf(a,key);return i>=0&&i+1<a.Length?a[i+1]:fallback;}
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Boot(){if(Arg("-discoverySystems")==""||Arg("-racerTestSave")=="")return;var g=new GameObject("CR091 systems validation");DontDestroyOnLoad(g);g.AddComponent<DiscoverySystemsValidation>();}
        RaceDirector race;RaceFlow flow;ArcadeVehicle car;ExplorationCollection collection;ExplorationMap map;string dir;readonly List<string> checks=new();
        void Check(bool ok,string name){checks.Add((ok?"PASS ":"FAIL ")+name);File.WriteAllLines(dir+"/checks.txt",checks);}
        void Bind(){race=FindAnyObjectByType<RaceDirector>();flow=race.Flow;car=race.vehicle;collection=race.GetComponent<ExplorationCollection>();map=race.GetComponent<ExplorationMap>();}
        IEnumerator Start()
        {
            Application.runInBackground=true;dir=Arg("-evidence","Docs/CR091-096/systems");Directory.CreateDirectory(dir);string course=Arg("-course","StreetLoopGreybox");if(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name!=course)UnityEngine.SceneManagement.SceneManager.LoadScene(course);yield return null;yield return null;Bind();
            switch(Arg("-discoverySystems")){case "collections":yield return Collections();break;case "return":yield return SummitReturn();break;case "map":yield return MapChecks();break;case "views":yield return Views();break;case "reload":yield return Reload();break;}
            if(Arg("-discoverySystems")=="records"){
                var stored=JsonUtility.FromJson<ActivityRecords.Data>(File.ReadAllText(Path.Combine(flow.Save.DirectoryPath,"activity-records-v2.json")));
                var jumps=stored.entries.Where(e=>e.site=="summit-homeward").ToArray();Check(jumps.Length>0&&jumps.All(e=>flow.Activities.Records.Archive.entries.Any(r=>r.id==e.id&&r.value==e.value&&r.key==e.key)),"Cold relaunch retains real scored summit attempts and exact categories");
                flow.OpenActivities();yield return null;ThreeFeatureValidation.CaptureUi(dir+"/records-after-relaunch.png");
            }
            File.WriteAllText(dir+"/done.txt","Completed explicit checks; inspect failures. No human/controller/listening acceptance.");Application.Quit();
        }
        void Place(Vector3 p,Vector3 forward)
        {
            var hit=Physics.RaycastAll(p+Vector3.up*30,Vector3.down,80).Where(h=>h.collider.name.StartsWith("Ground_")).OrderBy(h=>h.distance).First();p.y=hit.point.y+car.suspensionLength-.12f;
            car.Body.position=p;car.Body.rotation=Quaternion.LookRotation(Vector3.ProjectOnPlane(forward,Vector3.up));car.transform.SetPositionAndRotation(p,car.Body.rotation);car.Body.linearVelocity=car.Body.angularVelocity=Vector3.zero;car.ClearSteering();Physics.SyncTransforms();car.GetComponent<VehicleRespawn>().CancelRecovery();collection.ResetMovement();map.ResetMovement();flow.Activities.NewSession();race.ResetSampling(p,Time.timeAsDouble);FindAnyObjectByType<ChaseCamera>()?.Snap();
        }
        void Drive(Vector3 target,float speed=7){var local=car.transform.InverseTransformPoint(target);float steer=Mathf.Clamp(Mathf.Atan2(local.x,local.z)*2,-1,1);car.Simulate(Mathf.Clamp01((speed-car.ForwardSpeed)*.6f),car.ForwardSpeed>speed+1?.3f:0,steer,Time.fixedDeltaTime);}
        IEnumerator Collections()
        {
            race.traffic=race.opponents=false;flow.StartFreeRoam();car.enabled=false;car.GetComponent<VehicleInput>().enabled=false;
            Time.timeScale=float.TryParse(Arg("-testSpeed","1"),out var rate)?rate:1;
            Check(collection.sites.Length==24&&collection.sites.Select(s=>s.id).Distinct().Count()==24,"24 unique preserved IDs");
            int off=0;foreach(var s in collection.sites){(race.ambientRoad?race.ambientRoad:race.road).Project(s.position,out float d);if(d>15)off++;}Check(off>=18,"Most items off ordinary race roads: "+off);
            for(int i=0;i<collection.sites.Length;i++){
                var s=collection.sites[i];car.GetComponent<VehicleConfiguration>().Apply(race.EligibleVehicles[i%race.EligibleVehicles.Length].Id);var forward=s.position-s.access;if(Vector3.ProjectOnPlane(forward,Vector3.up).magnitude<1)forward=Vector3.forward;
                Place(s.access-forward.normalized*3,forward);for(int n=0;n<35;n++){car.Simulate(0,0,0,.02f);yield return new WaitForFixedUpdate();}
                car.GetComponent<VehicleRespawn>().RecordSafePosition();float start=Time.time,minUp=1;
                while(Time.time-start<16&&!collection.Discovered(s.id)){Drive(s.position);yield return new WaitForFixedUpdate();minUp=Mathf.Min(minUp,car.transform.up.y);}
                Check(collection.Discovered(s.id),s.id+" ordinary-frame approach / "+car.GetComponent<VehicleConfiguration>().profileId+" minUp="+minUp);
                ThreeFeatureValidation.CaptureUi(dir+"/"+s.id+".png");bool returned=car.GetComponent<VehicleRespawn>().TryRecoverLocal();Check(returned,s.id+" supported local reset/return");yield return new WaitForFixedUpdate();
            }
            map.Save();Check(collection.Found==24,"All 24 found in isolated save");Check(map.RevealedCount>0,"Physical exploration saved fog");
            File.WriteAllText(dir+"/save-root.txt",flow.Save.DirectoryPath);File.WriteAllText(dir+"/fog-count.txt",map.RevealedCount.ToString());
        }
        IEnumerator SummitReturn()
        {
            race.traffic=race.opponents=false;flow.StartFreeRoam();car.enabled=false;car.GetComponent<VehicleInput>().enabled=false;Time.timeScale=4;
            var path=collection.routes.Single(r=>r.name=="Summit approach and safe return").points.TakeLast(7).ToArray();
            foreach(var profile in race.EligibleVehicles){car.GetComponent<VehicleConfiguration>().Apply(profile.Id);Place(path[0],path[1]-path[0]);float start=Time.time,minUp=1;int target=1;
                using(var trace=new StreamWriter(dir+"/return-"+profile.Id+".csv")){trace.WriteLine("time,x,y,z,speed,wheels,up,target");
                    while(Time.time-start<100&&target<path.Length){Drive(path[target],8);yield return new WaitForFixedUpdate();var p=car.Body.position;minUp=Mathf.Min(minUp,car.transform.up.y);trace.WriteLine($"{Time.time-start:F3},{p.x:F3},{p.y:F3},{p.z:F3},{car.ForwardSpeed:F3},{car.GroundedWheels},{car.transform.up.y:F3},{target}");if(Vector3.ProjectOnPlane(p-path[target],Vector3.up).magnitude<5)target++;}
                }
                Check(target==path.Length&&minUp>.65f,profile.Id+" physically drove summit return / reached="+target+" minUp="+minUp);Check(car.GetComponent<VehicleRespawn>().TryRecoverLocal(),profile.Id+" return local reset");ThreeFeatureValidation.CaptureUi(dir+"/return-"+profile.Id+".png");
                var driveway=GameObject.Find("Kyle descending driveway").GetComponent<RaceRoad>().points;
                foreach(bool uphill in new[]{false,true}){var points=uphill?driveway.Reverse().ToArray():driveway;Place(points[0],points[1]-points[0]);start=Time.time;minUp=1;int next=1;while(Time.time-start<60&&next<points.Length){Drive(points[next],6);yield return new WaitForFixedUpdate();minUp=Mathf.Min(minUp,car.transform.up.y);if(Vector3.ProjectOnPlane(car.Body.position-points[next],Vector3.up).magnitude<4)next++;}Check(next==points.Length&&minUp>.65f,profile.Id+" Kyle driveway "+(uphill?"up":"down")+" minUp="+minUp);}
            }
        }
        IEnumerator MapChecks()
        {
            race.traffic=race.opponents=false;flow.StartFreeRoam();car.enabled=false;car.GetComponent<VehicleInput>().enabled=false;
            Check(!map.Travel(-1),"Invalid destination rejected");int hidden=Array.FindIndex(map.destinations,d=>!map.Discovered(d.id));if(hidden>=0)Check(!map.Travel(hidden),"Undiscovered destination rejected");
            // Explicit discovery fixtures isolate travel eligibility; physical fog is tested by collection driving.
            foreach(var d in map.destinations)map.Reveal(d.position);
            foreach(var profile in race.EligibleVehicles){car.GetComponent<VehicleConfiguration>().Apply(profile.Id);foreach(var d in map.destinations){int cells=map.RevealedCount,found=collection.Found,awards=flow.Activities.Awards;flow.Activities.BeginAttempt();bool ok=map.Travel(Array.IndexOf(map.destinations,d));Check(ok,profile.Id+" supported/clear arrival "+d.title);yield return new WaitForFixedUpdate();Check(map.RevealedCount==cells&&collection.Found==found&&flow.Activities.Awards==awards&&!flow.Activities.AttemptActive,"Teleport has no path reveal/collection/award, cancels attempt");}}
            race.FreeRoam=false;Check(!map.Travel(0),"Race travel rejected");race.FreeRoam=true;
            InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;var pad=InputSystem.AddDevice<Gamepad>();
            IEnumerator Press(GamepadButton b){InputSystem.QueueStateEvent(pad,new GamepadState().WithButton(b));yield return null;yield return null;InputSystem.QueueStateEvent(pad,new GamepadState());yield return null;yield return null;}
            yield return Press(GamepadButton.Select);Check(map.Opened,"Virtual controller opens map");yield return Press(GamepadButton.South);Check(map.Waypoint.HasValue,"Virtual controller places waypoint");yield return Press(GamepadButton.DpadRight);yield return Press(GamepadButton.West);Check(map.Opened&&flow.State==RaceFlow.Stage.Paused,"Map controller actions do not activate underlying menus");
            InputSystem.QueueStateEvent(pad,new GamepadState{rightTrigger=1,leftStick=new(.5f,.25f)});yield return new WaitForSecondsRealtime(.5f);InputSystem.QueueStateEvent(pad,new GamepadState());yield return null;ThreeFeatureValidation.CaptureUi(dir+"/map-controller.png");
            var mouse=InputSystem.AddDevice<Mouse>();var picture=FindObjectsByType<UnityEngine.UI.RawImage>().First(p=>p.name=="Terrain");var point=RectTransformUtility.WorldToScreenPoint(null,picture.transform.TransformPoint(new Vector3(-190,100,0)));var waypointBefore=map.Waypoint;
            InputSystem.QueueStateEvent(mouse,new MouseState{position=point});yield return null;yield return null;InputSystem.QueueStateEvent(mouse,new MouseState{position=point}.WithButton(MouseButton.Left));yield return null;yield return null;InputSystem.QueueStateEvent(mouse,new MouseState{position=point});yield return null;yield return null;Check(map.Waypoint.HasValue&&map.Waypoint!=waypointBefore,"Virtual mouse click places world-coordinate waypoint");
            var field=typeof(ExplorationMap).GetField("zoom",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic);float beforeZoom=(float)field.GetValue(map);InputSystem.QueueStateEvent(mouse,new MouseState{position=point,scroll=new Vector2(0,1)});yield return null;yield return null;Check((float)field.GetValue(map)>beforeZoom,"Virtual mouse wheel zooms map");InputSystem.RemoveDevice(mouse);
            yield return Press(GamepadButton.East);Check(!map.Opened&&flow.State==RaceFlow.Stage.Racing,"Virtual controller closes/resumes map");InputSystem.RemoveDevice(pad);
            int count=collection.Found,fog=map.RevealedCount;string settings=File.ReadAllText(Path.Combine(flow.Save.DirectoryPath,"settings.json"));Check(!collection.RestartCollection(false)&&collection.Found==count,"Unconfirmed collectibles restart rejected");Check(collection.RestartCollection(true)&&collection.Found==0,"Explicit confirmed collectibles-only restart");Check(map.RevealedCount==fog&&File.ReadAllText(Path.Combine(flow.Save.DirectoryPath,"settings.json"))==settings,"Restart preserves fog and settings");map.Save();
        }
        IEnumerator Reload()
        {
            int fog=map.RevealedCount;foreach(var name in new[]{"StreetLoopGreybox","LakeWoods","StreetLoopReverse","ForestLoopReverse"}){UnityEngine.SceneManagement.SceneManager.LoadScene(name);yield return null;yield return null;Bind();Check(collection.Found==24,name+" cold/track reload keeps all found IDs");Check(map.RevealedCount==fog&&fog>0,name+" cold/track reload retains same fog");}
        }
        IEnumerator Views()
        {
            var lettering=FindObjectsByType<TextMesh>().ToDictionary(t=>t,t=>t.GetComponent<Renderer>().enabled);BreakableProp.RestoreRace();yield return null;
            Check(lettering.All(p=>p.Key.GetComponent<Renderer>().enabled==p.Value),"Destruction restoration preserves authored lettering visibility");
            flow.StartFreeRoam();car.Body.isKinematic=true;car.enabled=false;car.GetComponent<VehicleInput>().enabled=false;FindAnyObjectByType<ChaseCamera>().enabled=false;var camera=Camera.main;
            var home=GameObject.Find("Dan - blue X").transform;var h2=GameObject.Find("Original house 2").transform;var h3=GameObject.Find("House 3 / Rocky Way Acres entrance").transform;var kyle=GameObject.Find("Friend across street - blue circle").transform;var camp=GameObject.Find("Permanent mountainside camp / two seated guys").transform;
            var shots=new[]{("home-front",home.position+home.forward*38+Vector3.up*7,home.position+Vector3.up),("home-rear",home.position-home.forward*65+Vector3.up*20,home.position-home.forward*20),("frontage",home.position+Vector3.up*155,Vector3.Lerp(home.position,h2.position,.5f)),("house2",h2.position+h2.forward*40+Vector3.up*10,h2.position+Vector3.up*3),("rocky-way",h3.position-h3.forward*27+Vector3.up*5,h3.position+Vector3.up*4),("kyle",kyle.position+kyle.forward*42+Vector3.up*12,kyle.position+Vector3.up*3),("camp",camp.position+new Vector3(-10,6,-10),camp.position+new Vector3(1,1,0)),("summit",new Vector3(1080,193,232),new Vector3(915,150,190))};
            foreach(var s in shots){camera.transform.SetPositionAndRotation(s.Item2,Quaternion.LookRotation(s.Item3-s.Item2));yield return null;yield return null;ThreeFeatureValidation.CaptureUi(dir+"/"+s.Item1+".png");}
            var signs=FindObjectsByType<TextMesh>().Where(t=>t.GetComponent<Renderer>().enabled&&!SceneryText.IsFloating(t)).ToArray();File.WriteAllLines(dir+"/physical-sign-inventory.txt",signs.Select(t=>t.name+" | "+t.text.Replace('\n','/')+" | "+t.transform.position));
            int index=0;foreach(var text in signs){var p=text.transform.position;var size=text.GetComponent<Renderer>().bounds.size;float distance=Mathf.Clamp(Mathf.Max(size.x,size.y)*1.2f,5,35);camera.transform.SetPositionAndRotation(p-text.transform.forward*distance,Quaternion.LookRotation(text.transform.forward));yield return null;ThreeFeatureValidation.CaptureUi(dir+"/sign-"+(index++).ToString("000")+".png");}
        }
    }
}
