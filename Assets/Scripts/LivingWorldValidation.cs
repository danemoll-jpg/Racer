using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace Racer
{
    public sealed class LivingWorldValidation:MonoBehaviour
    {
        static string Arg(string key,string fallback){var a=Environment.GetCommandLineArgs();int i=Array.IndexOf(a,key);return i>=0&&i+1<a.Length?a[i+1]:fallback;}
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot(){var a=Environment.GetCommandLineArgs();if(!a.Contains("-livingTest")||!a.Contains("-racerTestSave")||FindAnyObjectByType<LivingWorldValidation>())return;var g=new GameObject("Living world diagnostics");DontDestroyOnLoad(g);g.AddComponent<LivingWorldValidation>();}
        RaceDirector race;string root; readonly List<string> checks=new();
        Camera renderCamera;RenderTexture renderTarget;
        void LateUpdate(){if(renderCamera&&renderTarget)renderCamera.Render();}
        void Check(bool pass,string detail){checks.Add((pass?"PASS ":"FAIL ")+detail);File.WriteAllLines(root+"/checks.txt",checks);}
        IEnumerator Start()
        {
            Application.runInBackground=true;string scene=Arg("-course","LakeWoods");if(SceneManager.GetActiveScene().name!=scene)SceneManager.LoadScene(scene);yield return null;yield return null;
            root=Arg("-evidence","Docs/CR057-060/probe");Directory.CreateDirectory(root);race=FindAnyObjectByType<RaceDirector>();
            if(Arg("-livingTest","probe")=="race"){yield return Races();}else if(Arg("-livingTest","probe")=="lifecycle"){yield return Lifecycle();}else if(Arg("-livingTest","probe")=="scenes"){yield return Scenes();}else{yield return Probe();}
            File.WriteAllText(root+"/done.txt","Complete");Application.Quit();
        }
        IEnumerator Races()
        {
            int skill=int.Parse(Arg("-racerDifficulty","1"));bool baseline=DriverVariation.Disabled;
            bool performance=Environment.GetCommandLineArgs().Contains("-performance");
            if(performance){renderCamera=Camera.main;renderTarget=new RenderTexture(1280,720,24);renderCamera.targetTexture=renderTarget;QualitySettings.vSyncCount=0;Application.targetFrameRate=120;}
            using var result=new StreamWriter(root+"/races.csv");result.WriteLine("seed,difficulty,baseline,driver,finished,dnf,laps,time,misses,recoveries,errors,brakeSeconds");
            for(int seed=101;seed<=(performance?101:103);seed++)
            {
                AmbientLife.ForcedSeed=seed;race.opponents=true;race.traffic=true;race.difficulty=skill;race.laps=2;race.maximumRaceSeconds=1000;
                race.Flow.OpenGarage();race.Flow.SelectVehicle(race.Forest?"moto":"original");race.Flow.CloseGarage();race.opponentRoster=race.Forest?new[]{"moto","atv","moto"}:new[]{"tourer","moto","atv"};race.Racers[0]=new RacerState("Reference",race.vehicle,race.gates.Length-1,2);
                race.Flow.StartRace();var pilot=race.vehicle.gameObject.AddComponent<RoadDriver>();pilot.Initialize(race,race.vehicle,true,1,1);pilot.Racer=race.Racers[0];
                Time.timeScale=float.Parse(Arg("-timeScale","12"));float start=Time.realtimeSinceStartup;var frames=new List<float>();bool batFixture=performance&&Environment.GetCommandLineArgs().Contains("-batPerformance"),placedBat=false;int maxBats=0;
                using(var trace=new StreamWriter(root+"/trace-"+seed+".csv"))
                {
                    trace.WriteLine("time,driver,station,speed,target,throttle,brake,errors,errorWeight,line,brakeJudgment,grounded,water,recoveries");float next=0;
                    while(!race.ClassificationFinal&&Time.realtimeSinceStartup-start<180)
                    {
                        yield return null;if(race.Flow.State==RaceFlow.Stage.Racing)Time.timeScale=float.Parse(Arg("-timeScale","12"));frames.Add(Time.unscaledDeltaTime*1000);
                        if(batFixture&&race.Clock>2&&!placedBat){var life=race.GetComponent<AmbientLife>();pilot.enabled=false;Place(life.batEntrance-life.batForward*20+Vector3.up,life.batForward);race.vehicle.Body.linearVelocity=life.batForward*7;placedBat=true;}
                        if(batFixture){maxBats=Mathf.Max(maxBats,race.GetComponent<AmbientLife>().GetComponentsInChildren<Transform>().Count(t=>t.name=="Cave bat"));if(race.Clock>20)break;}
                        if(race.Clock<next)continue;next=(float)race.Clock+.2f;
                        foreach(var r in race.Racers){var d=r.Car.GetComponent<RoadDriver>();trace.WriteLine($"{race.Clock:F3},{r.Name},{race.road.Project(r.Car.Body.position,out _):F2},{r.Car.ForwardSpeed:F3},{d.TargetSpeed:F3},{d.LastThrottle:F3},{d.LastBrake:F3},{d.Variation.Events},{d.Variation.Weight:F3},{d.Variation.Line:F3},{d.Variation.Judgment:F3},{r.Car.GroundedWheels},{r.Car.WaterImmersion:F3},{r.Recoveries}");}
                    }
                }
                foreach(var r in race.Racers){var d=r.Car.GetComponent<RoadDriver>();result.WriteLine($"{seed},{skill},{baseline},{r.Name},{r.Progress.Finished},{r.Dnf},{r.Progress.CompletedLaps},{r.Progress.RaceTime(race.Clock):F3},{r.Progress.MissedGates},{r.Recoveries},{d.Variation.Events},{d.BrakingSeconds:F3}");}result.Flush();
                frames.Sort();File.AppendAllText(root+"/performance.txt",$"seed={seed} frames={frames.Count} median={frames[frames.Count/2]:F3} p95={frames[(int)(frames.Count*.95f)]:F3} timescale={Time.timeScale} peakMiB={System.Diagnostics.Process.GetCurrentProcess().PeakWorkingSet64/1048576.0:F1}\n");
                File.AppendAllText(root+"/performance.txt",$"explicitRender={performance} resolution=1280x720 traffic={race.Drivers.Count(d=>d.GetComponent<AmbientVehicle>())} population={race.GetComponent<AmbientLife>().Population} swarms={race.GetComponent<AmbientLife>().Swarms} radioCount={race.Flow.Radio.Count} playing={race.Flow.Radio.Playing} channel={race.Flow.Radio.ChannelName}\n");
                if(batFixture)File.AppendAllText(root+"/performance.txt",$"batFixture=true maxSimultaneousBats={maxBats}; deliberately positioned cave fixture, not race-completion evidence\n");
                pilot.enabled=false;Destroy(pilot);Time.timeScale=1;race.Flow.Pause();race.Flow.QuitRace();yield return null;
            }
        }
        void Place(Vector3 p,Vector3 forward)
        {var c=race.vehicle;c.Body.position=p;c.Body.rotation=Quaternion.LookRotation(forward);c.transform.SetPositionAndRotation(p,c.Body.rotation);c.Body.linearVelocity=c.Body.angularVelocity=Vector3.zero;c.ClearSteering();Physics.SyncTransforms();}
        IEnumerator Drive(float seconds,float throttle,float brake,float steering,StreamWriter log,string label)
        {float begin=Time.time;while(Time.time-begin<seconds){var c=race.vehicle;c.Simulate(throttle,brake,steering,Time.fixedDeltaTime);log.WriteLine($"{label},{Time.time-begin:F3},{c.Body.position.x:F3},{c.Body.position.y:F3},{c.Body.position.z:F3},{c.ForwardSpeed:F3},{c.GroundedWheels},{c.WaterImmersion:F3}");yield return new WaitForFixedUpdate();}}
        IEnumerator Probe()
        {
            Check(FindObjectsByType<TextMesh>().All(t=>t.name=="Fictional storefront identity"),"No floating TextMesh; mounted storefront lettering only");
            Check(race.gates.Length>2&&FindAnyObjectByType<RaceHud>(),"HUD and checkpoint components retained");
            var life=race.GetComponent<AmbientLife>();var states=new HashSet<string>();
            using(var occupancy=new StreamWriter(root+"/occupancy.csv"))
            {occupancy.WriteLine("seed,dan,friend,population,highwaySlots,residentialSlots");for(int i=1;i<=30;i++){AmbientLife.ForcedSeed=i;life.SelectScenes();states.Add(life.DanScene+"-"+life.FriendScene);occupancy.WriteLine($"{i},{life.DanScene},{life.FriendScene},{life.Population},{life.highway.Length},{life.residential.Length}");}}
            Check(states.Count==6,"All six independent household combinations across 30 seeds");Check(life.highway.Length>life.residential.Length*3,"Hwy 92 has substantially more supported slots");
            Check(life.GetComponentsInChildren<Collider>().All(c=>!c.enabled||c.GetComponentInParent<ArcadeVehicle>()),"Ambient figures have no blocking colliders");
            Time.timeScale=1;
            foreach(int seed in new[]{1,2,4,9,13,17}){AmbientLife.ForcedSeed=seed;life.SelectScenes();yield return new WaitForSeconds(.4f);Capture(root+"/dan-"+seed+".png",life.football[0]+new Vector3(7,8,13),life.football[0]+new Vector3(1,1,3));yield return null;Capture(root+"/friend-"+seed+".png",life.smoking[0]+new Vector3(-8,4,8),life.smoking[0]+Vector3.up);}
            using(var natural=new StreamWriter(root+"/natural-occupancy.csv")){natural.WriteLine("seed,dan,friend,population");AmbientLife.ForcedSeed=0;for(int i=0;i<12;i++){life.SelectScenes();natural.WriteLine($"{life.Seed},{life.DanScene},{life.FriendScene},{life.Population}");yield return new WaitForSeconds(.03f);}}
            race.opponents=race.traffic=false;race.Flow.OpenGarage();race.Flow.SelectVehicle("moto");race.Flow.CloseGarage();race.Flow.StartRace();while(race.Flow.State!=RaceFlow.Stage.Racing)yield return null;
            var car=race.vehicle;car.enabled=false;car.GetComponent<VehicleInput>().enabled=false;Time.timeScale=5;
            using(var log=new StreamWriter(root+"/water.csv"))
            {
                log.WriteLine("action,time,x,y,z,speed,grounded,immersion");
                foreach(string id in new[]{"moto","atv"})
                {
                    car.GetComponent<VehicleConfiguration>().Apply(id);car.enabled=false;
                    foreach(var water in ShallowWater.Active.ToArray())
                    {
                        for(int side=0;side<4;side++)
                        {
                            var direction=Quaternion.Euler(0,side*90,0)*Vector3.forward;var center=water.transform.position;var probe=center+direction*(water.round?48:2);
                            if(!Physics.Raycast(probe+Vector3.up*20,Vector3.down,out var bed,50,1,QueryTriggerInteraction.Ignore))continue;
                            Place(bed.point+Vector3.up*(car.suspensionLength-.14f),direction);
                            yield return Drive(1,0,0,0,log,id+" settle "+water.name+side);
                            Check(car.GroundedWheels>=2&&car.WaterImmersion>.1f,id+" grounded immersion "+water.name+side);
                            yield return Drive(water.round?3:.7f,1,0,.3f,log,"water turn");yield return Drive(2,0,1,0,log,"stop reverse");
                            Check(car.ForwardSpeed<0,"Reverse available "+id+water.name+side);
                            Place(bed.point+Vector3.up*(car.suspensionLength-.14f),direction);
                            bool exited=false;float until=Time.time+18;while(Time.time<until){yield return Drive(.1f,1,0,0,log,"drive out "+id+water.name+side);if(car.WaterImmersion==0&&car.GroundedWheels>=2&&car.ForwardSpeed>1&&!water.Contains(car.Body.position)){exited=true;break;}}
                            Check(exited,"Drive out "+id+water.name+side);
                            if(water.round)Capture(root+"/shore-"+id+side+".png",car.transform.position-car.transform.forward*7+Vector3.up*3,car.transform.position+Vector3.up);
                        }
                        Place(water.transform.position+Vector3.up*7,Vector3.forward);car.Body.linearVelocity=Vector3.forward*25;
                        car.Simulate(0,0,0,.02f);Check(car.WaterImmersion==0,"Airborne water crossing unaffected "+water.name);
                    }
                }
                car.GetComponent<VehicleRespawn>().TryRecoverLocal();Check(car.WaterImmersion==0,"Recovery clears immersion immediately");
                if(race.Forest)
                {
                    Place(life.batEntrance-life.batForward*20+Vector3.up,life.batForward);car.Body.linearVelocity=life.batForward*8;int before=life.Swarms;yield return null;
                    Check(life.Swarms==before+1,"Approaching rider triggers one bat swarm");
                    Time.timeScale=1;yield return new WaitForSeconds(.7f);Capture(root+"/bats.png",life.batEntrance-life.batForward*23+Vector3.up*3,life.batRoost-Vector3.up*3);
                    yield return new WaitForSeconds(3);Place(life.batEntrance-life.batForward*20+Vector3.up,life.batForward);car.Body.linearVelocity=life.batForward*8;yield return null;Check(life.Swarms==before+1,"Near-entry repeat remains disarmed");
                }
            }
            Time.timeScale=1;race.Flow.Pause();
        }
        IEnumerator Lifecycle()
        {
            var life=race.GetComponent<AmbientLife>();AmbientLife.ForcedSeed=9;race.opponents=false;race.traffic=true;
            race.Flow.OpenGarage();race.Flow.SelectVehicle("moto");race.Flow.CloseGarage();race.Flow.StartRace();while(race.Flow.State!=RaceFlow.Stage.Racing)yield return null;
            var car=race.vehicle;car.enabled=false;car.GetComponent<VehicleInput>().enabled=false;var feedback=car.GetComponent<WaterFeedback>();Time.timeScale=3;
            using var log=new StreamWriter(root+"/entry.csv");log.WriteLine("action,time,x,y,z,speed,grounded,immersion");
            var lake=ShallowWater.Active.First(w=>w.round);
            var wetDriver=race.Drivers.First();var wetOrigin=wetDriver.Car.Body.position;wetDriver.enabled=false;wetDriver.Car.Body.isKinematic=false;wetDriver.Car.Body.position=lake.transform.position-Vector3.up*.15f;wetDriver.Car.transform.position=wetDriver.Car.Body.position;wetDriver.Car.Body.linearVelocity=Vector3.forward*4;float wetUntil=Time.time+1;while(Time.time<wetUntil){wetDriver.Car.Simulate(1,0,0,Time.fixedDeltaTime);yield return new WaitForFixedUpdate();}yield return null;Check(wetDriver.Car.WaterImmersion>.5f&&wetDriver.Car.GetComponent<WaterFeedback>().FeedbackActive,"AI shares actual immersion, resistance and feedback");wetDriver.Car.Body.position=wetOrigin;wetDriver.Car.ClearSteering();wetDriver.enabled=true;
            foreach(string id in new[]{"moto","atv"})foreach(int side in new[]{0,1,2,3})
            {
                car.GetComponent<VehicleConfiguration>().Apply(id);car.enabled=false;var direction=Quaternion.Euler(0,side*90,0)*Vector3.forward;
                var p=lake.transform.position+direction*69;Physics.Raycast(p+Vector3.up*25,Vector3.down,out var ground,50,1,QueryTriggerInteraction.Ignore);Place(ground.point+Vector3.up*(car.suspensionLength-.14f),-direction);
                int sounds=feedback.EntrySounds;float until=Time.time+12;while(Time.time<until&&car.WaterImmersion<.8f)yield return Drive(.1f,1,0,0,log,"entry "+id+side);
                yield return null;Check(car.WaterImmersion>=.8f&&car.GroundedWheels>=2,"Supported entry from dry shore "+id+side);Check(feedback.EntrySounds>sounds,"Entry sound dispatched "+id+side);
                float yaw=car.transform.eulerAngles.y;yield return Drive(1,1,0,.6f,log,"immersed turn");Check(Mathf.Abs(Mathf.DeltaAngle(yaw,car.transform.eulerAngles.y))>3,"Immersed steering changes heading "+id+side);
                Capture(root+"/immersed-"+id+side+".png",car.transform.position+new Vector3(5,2,4),car.transform.position+Vector3.up*.4f);
                int exits=feedback.ExitSounds;
                yield return Drive(3,0,1,0,log,"brake through zero to reverse");Check(car.ForwardSpeed<-.2f,"Stop and reverse from immersed motion "+id+side);
                until=Time.time+30;while(Time.time<until&&car.WaterImmersion>0){var heading=car.transform.InverseTransformDirection(-direction);float reverseSteer=-Mathf.Clamp(Mathf.Atan2(heading.x,heading.z)*2,-.65f,.65f);yield return Drive(.1f,0,1,reverseSteer,log,"reverse out");}yield return null;
                Check(car.WaterImmersion==0,"Reverse out removes resistance "+id+side);Check(feedback.ExitSounds>exits,"Exit sound dispatched "+id+side);
                car.ClearSteering();Check(!feedback.FeedbackActive,"Recovery cleanup stops all water feedback "+id+side);
            }
            int seed=life.Seed,dan=life.DanScene,pop=life.Population;bool friend=life.FriendScene;race.Flow.Pause();yield return new WaitForSecondsRealtime(.15f);Check(seed==life.Seed&&dan==life.DanScene&&friend==life.FriendScene&&pop==life.Population,"Pause keeps occupancy stable");race.Flow.Resume();Time.timeScale=5;
            car.GetComponent<VehicleRespawn>().TryRecoverLocal();Check(seed==life.Seed&&dan==life.DanScene&&pop==life.Population,"Local recovery keeps occupancy stable");car.enabled=false;car.GetComponent<VehicleInput>().enabled=false;
            Capture(root+"/coffee.png",life.coffee[0]+new Vector3(5,3,5),life.coffee[0]+new Vector3(0,1,1));
            var traffic=race.Drivers.First();var oldPosition=traffic.Car.Body.position;int beforeAI=life.Swarms;traffic.Car.Body.position=life.batEntrance;yield return null;Check(life.Swarms==beforeAI,"Nearby AI does not trigger bats");traffic.Car.Body.position=oldPosition;
            Place(life.batEntrance-life.batForward*20+Vector3.up,life.batForward);car.Body.linearVelocity=life.batForward*8;int swarms=life.Swarms;yield return null;
            Check(life.Swarms==swarms+1,"ATV approach triggers pooled bats");var audio=life.GetComponentsInChildren<AudioSource>().First(a=>a.name=="Cave flutter source");Check(audio.isPlaying,"Bat flutter/chirp source starts");
            var samples=new float[1024];audio.clip.GetData(samples,0);Check(samples.Any(x=>Mathf.Abs(x)>.001f),"Bat clip has nonzero synthesized samples (not listening)");
            car.Body.isKinematic=true;yield return new WaitForSeconds(46);Check(life.Swarms==swarms+1,"Lingering beyond cooldown does not retrigger");
            car.Body.isKinematic=false;Place(life.batEntrance-life.batForward*85,life.batForward);car.Body.isKinematic=true;yield return new WaitForSeconds(13);car.Body.isKinematic=false;Place(life.batEntrance-life.batForward*20+Vector3.up,life.batForward);car.Body.linearVelocity=life.batForward*8;yield return null;Check(life.Swarms==swarms+2,"Leaving for 12 seconds rearms after cooldown");
            race.RestartRace();Check(car.WaterImmersion==0&&!feedback.FeedbackActive,"Restart clears water feedback");
            Time.timeScale=1;SceneManager.LoadScene("StreetLoopGreybox");yield return null;yield return null;Check(ShallowWater.Active.All(w=>w.gameObject.scene==SceneManager.GetActiveScene())&&ShallowWater.Active.Count==1,"Track change removes old water volumes");Check(FindObjectsByType<AmbientLife>().Length==1,"Track change does not duplicate ambient pools");
        }
        IEnumerator Scenes()
        {
            var life=race.GetComponent<AmbientLife>();Time.timeScale=1;var street=race.ambientRoad?race.ambientRoad:race.road;var house=GameObject.Find("Dan - blue X").transform;var road=street.At(street.Project(house.position,out _),out _);var away=Vector3.ProjectOnPlane(house.position-road,Vector3.up).normalized;var along=Vector3.Cross(Vector3.up,away);
            Check(life.football.All(p=>Vector3.Dot(p-road,away)>12&&Mathf.Abs(Vector3.Dot(p-road,along))<16),"All football endpoints and connecting flights inside accepted rectangular fence");
            Check(life.coffee.All(p=>Vector3.Dot(p-road,away)<10&&Vector3.Dot(p-road,away)>7),"Coffee group outside front fence and beyond traffic lane");
            foreach(var p in life.highway.Concat(life.residential)){street.Project(p,out float distance);Check(distance>10,"Pedestrian slot outside traffic lane");if(race.Forest){race.road.Project(p,out float trailDistance);Check(trailDistance>8,"Pedestrian slot outside forest racing trail");}}
            foreach(int seed in new[]{1,3,9,13,2,4})
            {
                AmbientLife.ForcedSeed=seed;life.SelectScenes();yield return new WaitForSeconds(.6f);
                Capture(root+"/football-"+seed+".png",life.football[0]+new Vector3(7,6,12),life.football[0]+new Vector3(1,1,2));yield return null;
                Capture(root+"/coffee-"+seed+".png",life.coffee[0]+new Vector3(4,3,4),life.coffee[0]+new Vector3(0,1,1));yield return null;
                Capture(root+"/friend-"+seed+".png",life.smoking[0]+new Vector3(-5,3,6),life.smoking[0]+Vector3.up);
                foreach(var p in life.GetComponentsInChildren<Transform>().Where(t=>t.name=="Ambient resident")){bool supported=Physics.Raycast(p.position+Vector3.up*.4f,Vector3.down,out var hit,1,1,QueryTriggerInteraction.Ignore)&&Mathf.Abs(hit.point.y-p.position.y)<.08f;Check(supported,"Supported feet seed="+seed+" "+p.position);}
            }
            Check(FindObjectsByType<TextMesh>().All(t=>t.name=="Fictional storefront identity"),"No floating world text in final scene");
            var camera=FindAnyObjectByType<ChaseCamera>();camera.Snap();ThreeFeatureValidation.CaptureUi(root+"/hud-grid.png");
        }
        public static void Capture(string file,Vector3 from,Vector3 to)
        {
            if(SystemInfo.graphicsDeviceType==UnityEngine.Rendering.GraphicsDeviceType.Null)return;
            var camera=Camera.main;var position=camera.transform.position;var rotation=camera.transform.rotation;var previous=camera.targetTexture;camera.transform.position=from;camera.transform.LookAt(to);
            var rt=new RenderTexture(1280,720,24);camera.targetTexture=rt;camera.Render();var old=RenderTexture.active;RenderTexture.active=rt;var image=new Texture2D(1280,720,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1280,720),0,0);image.Apply();File.WriteAllBytes(file,image.EncodeToPNG());camera.targetTexture=previous;camera.transform.SetPositionAndRotation(position,rotation);RenderTexture.active=old;Destroy(image);Destroy(rt);
        }
    }
}
