using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Racer
{
    public sealed class ExplorationValidation:MonoBehaviour
    {
        static string Arg(string key,string fallback){var a=Environment.GetCommandLineArgs();int i=Array.IndexOf(a,key);return i>=0&&i+1<a.Length?a[i+1]:fallback;}
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot(){if(Arg("-explorationCheck","")==""||Arg("-racerTestSave","")=="")return;var g=new GameObject("Explicit exploration validation");DontDestroyOnLoad(g);g.AddComponent<ExplorationValidation>();}
        string evidence,contacts="";
        public void Contact(Collision c){foreach(var p in c.contacts)contacts+=c.collider.name+":"+p.normal.ToString("F3")+";";}
        IEnumerator Start()
        {
            Application.runInBackground=true;QualitySettings.vSyncCount=0;Application.targetFrameRate=120;
            string course=Arg("-course","StreetLoopGreybox");if(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name!=course)UnityEngine.SceneManagement.SceneManager.LoadScene(course);yield return null;yield return null;
            evidence=Arg("-evidence","Docs/CR081-090/test");Directory.CreateDirectory(evidence);
            var race=FindAnyObjectByType<RaceDirector>();var flow=race.Flow;var car=race.vehicle;var collection=race.GetComponent<ExplorationCollection>();
            if(Arg("-explorationCheck","")=="controls")
            {
                var pad=UnityEngine.InputSystem.InputSystem.AddDevice<UnityEngine.InputSystem.Gamepad>();var rows=new List<string>();
                System.Collections.IEnumerator Press(UnityEngine.InputSystem.LowLevel.GamepadButton button)
                {UnityEngine.InputSystem.InputSystem.QueueStateEvent(pad,new UnityEngine.InputSystem.LowLevel.GamepadState().WithButton(button));yield return null;yield return null;UnityEngine.InputSystem.InputSystem.QueueStateEvent(pad,new UnityEngine.InputSystem.LowLevel.GamepadState());yield return null;yield return null;}
                void Check(bool ok,string label){rows.Add((ok?"PASS ":"FAIL ")+label);File.WriteAllLines(evidence+"/checks.txt",rows);}
                for(int i=0;i<11;i++)yield return Press(UnityEngine.InputSystem.LowLevel.GamepadButton.DpadDown);
                yield return Press(UnityEngine.InputSystem.LowLevel.GamepadButton.South);Check(flow.State==RaceFlow.Stage.Activities,"Virtual controller navigates main menu into Activity Records");
                if(flow.State!=RaceFlow.Stage.Activities)flow.OpenActivities();
                yield return Press(UnityEngine.InputSystem.LowLevel.GamepadButton.DpadDown);yield return Press(UnityEngine.InputSystem.LowLevel.GamepadButton.South);
                Check(FindObjectsByType<UnityEngine.UI.Text>().Any(t=>t.text=="ACTIVITY RECORDS / JUMPS"),"Controller selects Jumps tab");ThreeFeatureValidation.CaptureUi(evidence+"/controller-jumps.png");
                yield return Press(UnityEngine.InputSystem.LowLevel.GamepadButton.East);Check(flow.State==RaceFlow.Stage.Ready,"Controller returns from records");
                flow.OpenExploration();yield return null;bool enabled=flow.Ghost.Enabled;yield return Press(UnityEngine.InputSystem.LowLevel.GamepadButton.South);Check(flow.Ghost.Enabled!=enabled,"Controller toggles clean-lap ghost");
                yield return Press(UnityEngine.InputSystem.LowLevel.GamepadButton.East);flow.StartFreeRoam();flow.Pause();yield return null;
                Check(FindObjectsByType<UnityEngine.UI.Button>().Any(b=>b.GetComponentInChildren<UnityEngine.UI.Text>().text.StartsWith("Activity Records")),"Pause exposes Activity Records button");
                UnityEngine.InputSystem.InputSystem.RemoveDevice(pad);
            }
            if(Arg("-explorationCheck","")=="trails")
            {
                var rows=new List<string>();
                foreach(var road in collection.routes)
                foreach(float direction in new[]{1f,-1f})
                {
                    if(flow.State!=RaceFlow.Stage.Ready){flow.Pause();flow.QuitRace();}flow.StartFreeRoam();car.enabled=false;car.GetComponent<VehicleInput>().enabled=false;
                    float startStation=direction>0?5:road.Length-5;var p=road.At(startStation,out var forward);var hit=Physics.RaycastAll(p+Vector3.up*40,Vector3.down,100).Where(h=>h.collider.name.StartsWith("Ground_")).OrderBy(h=>h.distance).First();p.y=hit.point.y+car.suspensionLength-.12f;
                    car.Body.position=p;car.Body.rotation=Quaternion.LookRotation(Vector3.ProjectOnPlane(forward*direction,Vector3.up));car.transform.SetPositionAndRotation(p,car.Body.rotation);car.Body.linearVelocity=Vector3.zero;car.Body.angularVelocity=Vector3.zero;car.ClearSteering();Physics.SyncTransforms();
                    float previous=startStation,travel=0,began=Time.time,minUp=1,maxLateral=0;bool wet=false;
                    using(var log=new StreamWriter(evidence+"/trail-"+Array.IndexOf(collection.routes,road)+"-"+direction+".csv"))
                    {
                        log.WriteLine("time,station,travel,lateral,x,y,z,speed,up,water");
                        while(travel<road.Length-15&&Time.time-began<240)
                        {
                            float s=road.ProjectNear(car.Body.position,previous,30,out float lateral);float delta=Mathf.Repeat(s-previous+road.Length*.5f,road.Length)-road.Length*.5f;if(Mathf.Abs(delta)<5)travel+=delta*direction;previous=s;maxLateral=Mathf.Max(maxLateral,lateral);minUp=Mathf.Min(minUp,car.transform.up.y);wet|=car.WaterImmersion>.1f;
                            var aim=road.At(s+direction*9,out _);var q=car.transform.InverseTransformPoint(aim);float steering=Mathf.Clamp(Mathf.Atan2(q.x,q.z)*2,-1,1);car.Simulate(Mathf.Clamp01((12-car.ForwardSpeed)*.5f),car.ForwardSpeed>14?.3f:0,steering,Time.fixedDeltaTime);
                            p=car.Body.position;log.WriteLine($"{Time.time:F2},{s:F2},{travel:F2},{lateral:F2},{p.x:F2},{p.y:F2},{p.z:F2},{car.ForwardSpeed:F2},{car.transform.up.y:F3},{car.WaterImmersion:F3}");yield return new WaitForFixedUpdate();
                        }
                    }
                    bool recovered=car.GetComponent<VehicleRespawn>().TryRecoverLocal();rows.Add($"{road.name} direction={direction} complete={travel>=road.Length-15} travel={travel:F1}/{road.Length:F1} minUp={minUp:F3} maximum lateral={maxLateral:F2} wet={wet} recovery={recovered}");File.WriteAllLines(evidence+"/trails.txt",rows);car.enabled=true;
                }
            }
            if(Arg("-explorationCheck","")=="reload")
            {
                var rows=new List<string>();foreach(var name in new[]{"StreetLoopGreybox","LakeWoods","StreetLoopReverse","ForestLoopReverse"})
                {UnityEngine.SceneManagement.SceneManager.LoadScene(name);yield return null;yield return null;var found=FindAnyObjectByType<ExplorationCollection>();rows.Add((found.Found==24?"PASS ":"FAIL ")+name+" reload discovery count="+found.Found);}
                File.WriteAllLines(evidence+"/checks.txt",rows);
            }
            if(Arg("-explorationCheck","")=="views")
            {
                flow.StartFreeRoam();car.Body.isKinematic=true;car.enabled=false;car.GetComponent<VehicleInput>().enabled=false;FindAnyObjectByType<ChaseCamera>().enabled=false;
                var camera=Camera.main;var home=GameObject.Find("Dan - blue X").transform;
                var shots=new[]{("home-front",home.position+home.forward*28+Vector3.up*5,home.position+Vector3.up), ("home-rear",home.position-home.forward*65+Vector3.up*22,home.position-home.forward*20-Vector3.up*2),("property-overhead",home.position+Vector3.up*125,home.position-home.forward*16),("lake-mountain",new Vector3(675,108,-100),new Vector3(960,139,40)),("mountain-trails",new Vector3(980,260,-260),new Vector3(900,114,10))};
                foreach(var shot in shots){camera.transform.SetPositionAndRotation(shot.Item2,Quaternion.LookRotation(shot.Item3-shot.Item2));yield return null;yield return null;ThreeFeatureValidation.CaptureUi(evidence+"/"+shot.Item1+".png");}
            }
            if(Arg("-explorationCheck","")=="systems")
            {
                var rows=new List<string>();void Check(bool ok,string label){rows.Add((ok?"PASS ":"FAIL ")+label);File.WriteAllLines(evidence+"/checks.txt",rows);}
                string recordRoot=Path.Combine(flow.Save.DirectoryPath,"ledger-fixture");Directory.CreateDirectory(recordRoot);
                var legacy=new ArcadeActivities.Archive();legacy.results.Add(new(){key="trap/old-course/activities-v1/original",value=20,medal=2});
                var records=new ActivityRecords(recordRoot,legacy);Check(records.Archive.entries.Count==1&&records.Archive.entries[0].date==null,"One historical best migrated, date unknown");
                records=new ActivityRecords(recordRoot,legacy);Check(records.Archive.entries.Count==1,"Migration remains exactly once after reopen");
                for(int i=0;i<14;i++)records.Add(new(){id="attempt-"+i,key="test/current/original/forward",site="test",vehicle="original",value=30+i/2,medal=2,date="2026-09-20T00:00:00Z"});
                var board=records.Board("test/current/original/forward");Check(board.Count==10&&board[0].value==36&&board[1].value==36,"Top ten of fourteen, full precision and distinct ties");
                Check(!records.Add(board[0])&&records.Board(board[0].key).Count==10,"Reopening or duplicate attempt ID cannot add an entry");
                records=new ActivityRecords(recordRoot,legacy);Check(records.Board(board[0].key).Select(x=>x.id).SequenceEqual(board.Select(x=>x.id)),"Restart preserves tied ordering and top ten");
                var fixture=new GameObject("Ghost storage fixture").AddComponent<CleanLapGhost>();string ghostRoot=Path.Combine(flow.Save.DirectoryPath,"ghost-fixture");fixture.Initialize(race,ghostRoot);
                var buffer=(List<CleanLapGhost.Pose>)typeof(CleanLapGhost).GetField("recording",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).GetValue(fixture);
                void Seed(double time){fixture.Boundary(time,car.Body.position,car.Body.rotation,false,false);for(int j=1;j<=12;j++)buffer.Add(new(){t=j*.1f,p=car.Body.position+Vector3.forward*j,q=car.Body.rotation});}
                string GhostFile()=>Path.Combine(ghostRoot,"CleanLapGhosts",fixture.Key.Replace('/','_')+".json");
                Seed(0);fixture.Boundary(3,car.Body.position+Vector3.forward*20,car.Body.rotation,true,true);Check(File.Exists(GhostFile()),"Ghost storage fixture accepts completed clean sample sequence");
                var first=File.ReadAllText(GhostFile());Seed(10);fixture.Invalidate();fixture.Boundary(12,car.Body.position,car.Body.rotation,true,true);Check(File.ReadAllText(GhostFile())==first,"Reset/teleport invalidation cannot replace ghost");
                Seed(20);fixture.Boundary(24,car.Body.position,car.Body.rotation,true,true);Check(File.ReadAllText(GhostFile())==first,"Slower compatible lap cannot replace ghost");
                Seed(30);fixture.Boundary(32,car.Body.position,car.Body.rotation,true,true);Check(JsonUtility.FromJson<CleanLapGhost.Lap>(File.ReadAllText(GhostFile())).seconds==2,"Faster valid compatible lap replaces ghost");
                string priorCourse=race.courseId;race.courseId+="-fixture-incompatible";fixture.Refresh();Check(fixture.Status.Contains("No compatible"),"Changed layout cannot play historical ghost");race.courseId=priorCourse;Destroy(fixture.gameObject);
                Check(collection.sites.Length==24&&collection.sites.Select(s=>s.id).Distinct().Count()==24,"24 stable unique collectible IDs");
                flow.OpenActivities();yield return null;ThreeFeatureValidation.CaptureUi(evidence+"/activity-records.png");flow.CloseExtras();flow.OpenExploration();yield return null;ThreeFeatureValidation.CaptureUi(evidence+"/exploration-menu.png");flow.CloseExtras();
                flow.StartFreeRoam();car.enabled=false;car.GetComponent<VehicleInput>().enabled=false;
                foreach(var site in collection.sites)
                {
                    int i=Array.IndexOf(collection.sites,site);var road=i<12?(race.ambientRoad?race.ambientRoad:race.road):collection.routes[0];float targetStation=road.Project(site.position,out _);var p=road.At(targetStation-30,out var f);var ground=Physics.RaycastAll(p+Vector3.up*40,Vector3.down,100).Where(h=>h.collider.name.StartsWith("Ground_")).OrderBy(h=>h.distance).FirstOrDefault();p.y=ground.point.y+car.suspensionLength-.12f;
                    car.Body.position=p;car.Body.rotation=Quaternion.LookRotation(Vector3.ProjectOnPlane(f,Vector3.up));car.transform.SetPositionAndRotation(p,car.Body.rotation);car.Body.linearVelocity=car.transform.forward*10;car.Body.angularVelocity=Vector3.zero;car.ClearSteering();Physics.SyncTransforms();FindAnyObjectByType<ChaseCamera>()?.Snap();yield return new WaitForFixedUpdate();float began=Time.time;int found=collection.Found;
                    while(Time.time-began<12&&!collection.Discovered(site.id))
                    {float s=road.Project(car.Body.position,out _);var aim=road.At(s+10,out _);var local=car.transform.InverseTransformPoint(aim);float angle=Mathf.Atan2(local.x,local.z);float steering=Mathf.Clamp(angle*2,-1,1);car.Simulate(Mathf.Clamp01((13-car.ForwardSpeed)*.5f),car.ForwardSpeed>15?.3f:0,steering,Time.fixedDeltaTime);yield return new WaitForFixedUpdate();}
                    Check(collection.Discovered(site.id),site.id+" discovered through ordinary driving or supported initial spawn "+site.title);
                    Check(car.GetComponent<VehicleRespawn>().TryRecoverLocal(),site.id+" supported local return/reset");
                    yield return null; ThreeFeatureValidation.CaptureUi(evidence+"/"+site.id+".png");
                }
                var saved=JsonUtility.FromJson<ExplorationCollection.Save>(File.ReadAllText(Path.Combine(flow.Save.DirectoryPath,"woodland-acorns-v1.json")));Check(saved.found.Distinct().Count()==collection.Found,"Collectible discoveries written once to persistent isolated save");
                car.enabled=true;
            }
            if(Arg("-explorationCheck","")=="ramps")
            {
                var route=collection.routes[0];var contact=car.gameObject.AddComponent<ExplorationContacts>();contact.owner=this;
                File.WriteAllText(evidence+"/ramps.csv","course,vehicle,lip,direction,target,line,completed,minUp,recovered,awards\n");
                foreach(var profile in race.EligibleVehicles)
                foreach(float lip in new[]{213f,537f})
                foreach(float direction in Arg("-opposite","no")=="yes"?new[]{-1f}:new[]{1f})
                foreach(float speed in Arg("-quick","no")=="yes"?new[]{24f}:new[]{16f,24f,32f})
                foreach(float line in Arg("-quick","no")=="yes"?new[]{0f}:new[]{0f,-2f,2f,-((lip==213?16:7)-profile.Size.x*.5f),(lip==213?16:7)-profile.Size.x*.5f})
                {
                    if(flow.State!=RaceFlow.Stage.Ready){flow.Pause();flow.QuitRace();}flow.OpenGarage();flow.SelectVehicle(profile.Id);flow.CloseGarage();race.opponents=false;race.traffic=false;flow.StartFreeRoam();
                    car.enabled=false;car.GetComponent<VehicleInput>().enabled=false;Time.timeScale=float.Parse(Arg("-testSpeed","1"),System.Globalization.CultureInfo.InvariantCulture);
                    float start=lip-direction*65;var p=route.At(start,out var f);p+=Vector3.Cross(Vector3.up,f).normalized*line;
                    var ground=Physics.RaycastAll(p+Vector3.up*30,Vector3.down,80).Where(h=>h.collider.name.StartsWith("Ground_")).OrderBy(h=>h.distance).FirstOrDefault();p.y=ground.point.y+car.suspensionLength-.12f;
                    car.Body.position=p;car.Body.rotation=Quaternion.LookRotation(Vector3.ProjectOnPlane(f*direction,Vector3.up));car.transform.SetPositionAndRotation(p,car.Body.rotation);car.Body.linearVelocity=car.transform.forward*speed;car.Body.angularVelocity=Vector3.zero;car.ClearSteering();Physics.SyncTransforms();FindAnyObjectByType<ChaseCamera>()?.Snap();flow.Activities.NewSession();
                    float began=Time.time,minUp=1;bool completed=false;int awards=flow.Activities.Awards;
                    string id=profile.Id+"-"+lip+"-"+direction+"-"+speed+"-"+line;
                    using(var trace=new StreamWriter(evidence+"/"+id+".csv"))
                    {
                        trace.WriteLine("t,station,x,y,z,speed,vx,vy,vz,throttle,brake,steering,grounded,suspension,alignment,up,contacts");
                        while(Time.time-began<18)
                        {
                            float s=route.Project(car.Body.position,out _);float look=Mathf.Clamp(6+Mathf.Abs(car.ForwardSpeed)*.38f,8,19);var target=route.At(s+direction*look,out var ahead)+Vector3.Cross(Vector3.up,ahead).normalized*line;
                            var local=car.transform.InverseTransformPoint(target);float curvature=2*local.x/Mathf.Max(1,local.x*local.x+local.z*local.z);float angle=Mathf.Lerp(car.slowSteerAngle,car.fastSteerAngle,Mathf.Clamp01(Mathf.Abs(car.ForwardSpeed)/car.topSpeed));float steer=Mathf.Clamp(Mathf.Atan(curvature*car.wheelbase)*Mathf.Rad2Deg/angle,-1,1);float throttle=Mathf.Clamp01((speed-car.ForwardSpeed)*.5f),brake=car.ForwardSpeed>speed+1?.2f:0;
                            car.Simulate(throttle,brake,steer,Time.fixedDeltaTime);yield return new WaitForFixedUpdate();minUp=Mathf.Min(minUp,car.transform.up.y);var q=car.Body.position;var v=car.Body.linearVelocity;
                            trace.WriteLine($"{Time.time-began:F3},{s:F3},{q.x:F3},{q.y:F3},{q.z:F3},{car.ForwardSpeed:F3},{v.x:F3},{v.y:F3},{v.z:F3},{throttle:F3},{brake:F3},{steer:F3},{car.GroundedWheels},{car.SuspensionLift:F3},{car.AlignmentTorque:F3},{car.transform.up.y:F3},\"{contacts}\"");contacts="";
                            route.Project(car.Body.position,out float lateral);
                            if(lateral>50||Vector3.Distance(car.Body.position,p)>400)break;
                            if((s-lip)*direction>85&&Mathf.Abs(s-lip)<180&&lateral<Mathf.Abs(line)+8&&car.GroundedWheels>=2){completed=true;break;}
                        }
                    }
                    for(int i=0;i<20;i++){car.Simulate(0,.1f,0,Time.fixedDeltaTime);yield return new WaitForFixedUpdate();}
                    ThreeFeatureValidation.CaptureUi(evidence+"/"+id+".png");bool recovered=car.GetComponent<VehicleRespawn>().TryRecoverLocal();
                    File.AppendAllText(evidence+"/ramps.csv",$"{course},{profile.Id},{lip},{direction},{speed},{line},{completed},{minUp:F3},{recovered},{flow.Activities.Awards-awards}\n");car.enabled=true;
                }
            }
            File.WriteAllText(evidence+"/done.txt","Completed explicit test; inspect individual cases. Audio muted; no human/controller validation.");Application.Quit();
        }
    }
    public sealed class ExplorationContacts:MonoBehaviour{public ExplorationValidation owner;void OnCollisionEnter(Collision c)=>owner.Contact(c);void OnCollisionStay(Collision c)=>owner.Contact(c);}
}




