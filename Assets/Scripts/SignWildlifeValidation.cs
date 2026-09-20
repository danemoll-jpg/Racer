using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
namespace Racer
{
    public sealed class SignWildlifeValidation:MonoBehaviour
    {
        static string Arg(string key,string fallback){var a=Environment.GetCommandLineArgs();int i=Array.IndexOf(a,key);return i>=0&&i+1<a.Length?a[i+1]:fallback;}
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot(){var a=Environment.GetCommandLineArgs();if(!a.Contains("-signWildlifeTest")||!a.Contains("-racerTestSave")||FindAnyObjectByType<SignWildlifeValidation>())return;var g=new GameObject("Sign and wildlife diagnostics");DontDestroyOnLoad(g);g.AddComponent<SignWildlifeValidation>();}
        RaceDirector race;Wildlife wildlife;string root;readonly List<string> checks=new();Camera cam;RenderTexture target;readonly List<float> frames=new();
        void Check(bool pass,string label){checks.Add((pass?"PASS ":"FAIL ")+label);File.WriteAllLines(root+"/checks.txt",checks);}
        void LateUpdate(){if(cam&&target){cam.Render();frames.Add(Time.unscaledDeltaTime*1000);}}
        IEnumerator Start()
        {
            Application.runInBackground=true;root=Arg("-evidence","Docs/CR061-062/test");Directory.CreateDirectory(root);
            string scene=Arg("-course","LakeWoods");if(SceneManager.GetActiveScene().name!=scene)SceneManager.LoadScene(scene);yield return null;yield return null;
            race=FindAnyObjectByType<RaceDirector>();wildlife=race.GetComponent<Wildlife>();cam=Camera.main;target=new RenderTexture(1280,720,24);cam.targetTexture=target;QualitySettings.vSyncCount=0;Application.targetFrameRate=120;
            race.Flow.OpenGarage();race.Flow.SelectVehicle(race.Forest?"moto":"original");race.Flow.CloseGarage();race.traffic=true;race.opponents=true;if(Environment.GetCommandLineArgs().Contains("-roamPerformance"))race.Flow.StartFreeRoam();else race.Flow.StartRace();while(race.Flow.State!=RaceFlow.Stage.Racing)yield return null;
            race.Flow.Radio.SetFolder(Path.GetFullPath("BundleMusic"));float until=Time.realtimeSinceStartup+15;while(race.Flow.Radio.Scanning&&Time.realtimeSinceStartup<until)yield return null;if(!race.Flow.Save.Settings.radioOn)race.Flow.Radio.Toggle();while(!race.Flow.Radio.Playing&&Time.realtimeSinceStartup<until)yield return null;QualitySettings.vSyncCount=0;Application.targetFrameRate=120;
            Check(race.Flow.Radio.Playing,"Radio actually playing at normal saved-default volumes");
            string mode=Arg("-signWildlifeTest","signs");
            if(mode=="signs")yield return Signs();else if(mode=="wildlife")yield return Animals();else if(mode=="audio")yield return Audio();else yield return Drive();
            frames.Sort();File.WriteAllText(root+"/performance.txt",$"frames={frames.Count} median={frames[frames.Count/2]:F3} p95={frames[(int)(frames.Count*.95f)]:F3} ms; 1280x720 offscreen explicitly rendered, cap=120; people={race.GetComponent<AmbientLife>().Population}; wildlife={wildlife.SelectedCount}; traffic={race.Drivers.Count(d=>d.GetComponent<AmbientVehicle>())}; radio={race.Flow.Radio.Playing}; normalTimeScale={Time.timeScale}\n");
            File.WriteAllText(root+"/done.txt",$"{checks.Count(c=>c.StartsWith("PASS"))}/{checks.Count} passed; DSP capture is before OS endpoint; no human listening claimed");Application.Quit(checks.Any(c=>c.StartsWith("FAIL"))?2:0);
        }
        void Place(Vector3 p,Vector3 forward,float speed=0)
        {
            var car=race.vehicle;car.enabled=false;car.GetComponent<VehicleInput>().enabled=false;car.Body.isKinematic=false;car.Body.position=p;car.Body.rotation=Quaternion.LookRotation(Vector3.ProjectOnPlane(forward,Vector3.up));car.transform.SetPositionAndRotation(p,car.Body.rotation);car.Body.linearVelocity=forward.normalized*speed;car.Body.angularVelocity=Vector3.zero;car.ClearSteering();Physics.SyncTransforms();FindAnyObjectByType<ChaseCamera>().Snap();
        }
        void Shot(string name){var c=cam.transform;LivingWorldValidation.Capture(root+"/"+name+".png",c.position,c.position+c.forward*40);}
        IEnumerator Signs()
        {
            int expected=(race.Forest?38:37)+(race.reverseCourse?(race.Forest?6:5):0);var texts=FindObjectsByType<TextMesh>(FindObjectsInactive.Include);Check(texts.Length==expected,"Preserved physical sign faces plus authored reverse navigation");Check(!texts.Any(t=>SceneryText.RetiredHairpin(t.text,t.transform.parent.position)),"Exact retired hairpin sign absent");
            Check(texts.All(t=>t.GetComponentInParent<PhysicalSign>()&&!SceneryText.IsFloating(t)),"Every world text has a physical sign marker; no floating labels");
            File.WriteAllLines(root+"/signs.txt",texts.Select(t=>$"{t.name} | {t.text.Replace('\n','|')} | {t.transform.position:F4} | breakable={!!t.GetComponentInParent<BreakableProp>()}"));
            foreach(string name in new[]{"Road lettering","Road names","Fictional storefront identity","Recommended speed","Shortcut advice","Cave warning lettering"})
            {
                var t=texts.FirstOrDefault(t=>name=="Road names"?t.text.Contains("South Cherokee Lane"):t.name==name);if(!t)continue;
                var road=race.Forest&&name=="Cave warning lettering"?null:race.ambientRoad?race.ambientRoad:race.road;
                Vector3 p,f;if(road){float s=road.Project(t.transform.position,out _);p=road.At(s-16,out f);}else{p=FindAnyObjectByType<WoodlandRoute>().At(25,out f);}
                Place(p+Vector3.up*.8f,f,8);yield return new WaitForSeconds(.25f);FindAnyObjectByType<ChaseCamera>().Snap();Shot(name.Replace(' ','-')+"-driving");
                LivingWorldValidation.Capture(root+"/"+name.Replace(' ','-')+"-detail.png",t.transform.position-t.transform.forward*12+Vector3.up,t.transform.position);
            }
            if(race.Forest)Check(texts.First(t=>t.name=="Cave warning lettering").text.Replace('\n',' ')=="Warning: Cave Ahead Enter at your Own Risk","Exact cave warning wording and capitalization");
            foreach(var prop in texts.Select(t=>t.GetComponentInParent<BreakableProp>()).Where(p=>p).Distinct())
            {
                var children=prop.GetComponentsInChildren<TextMesh>();var local=children.Select(t=>t.transform.localPosition).ToArray();var origin=prop.transform.position;var rotation=prop.transform.rotation;var bounds=prop.GetComponent<BoxCollider>().bounds;
                Place(bounds.center-prop.transform.forward*4,prop.transform.forward,15);yield return new WaitForSeconds(.45f);
                Check(prop.IsBroken,"Physical vehicle trigger breaks "+prop.name);
                Check(children.Select((t,i)=>Vector3.Distance(t.transform.localPosition,local[i])<.0001f).All(x=>x),"Lettering stays on moving panel "+prop.name);
                Place(origin+Vector3.up*10+Vector3.right*25,Vector3.forward);race.vehicle.Body.isKinematic=true;
                yield return new WaitForSeconds(4.1f);Check(children.All(t=>!t.GetComponent<Renderer>().enabled),"Debris lettering hides "+prop.name);
                BreakableProp.RestoreRace();yield return null;Check(!prop.IsBroken&&Vector3.Distance(origin,prop.transform.position)<.001f&&Quaternion.Angle(rotation,prop.transform.rotation)<.01f&&children.All(t=>t.GetComponent<Renderer>().enabled),"Panel and lettering restore together "+prop.name);
            }
            race.RestartRace();yield return null;Check(FindObjectsByType<TextMesh>().Length==expected,"Race restart retains all other mounted lettering");Check(!FindObjectsByType<TextMesh>().Any(t=>SceneryText.RetiredHairpin(t.text,t.transform.parent.position)),"Restart cannot restore retired sign");
        }
        IEnumerator Animals()
        {
            Check(Enum.GetValues(typeof(Wildlife.Species)).Cast<Wildlife.Species>().All(s=>wildlife.habitats.Any(h=>h.species==s)),"All five wildlife habitats present");
            Check(wildlife.GetComponentsInChildren<Collider>(true).Length==0,"Wildlife pool has no vehicle colliders");
            using(var log=new StreamWriter(root+"/occupancy.csv"))
            {
                log.WriteLine("seed,selected,bird,squirrel,frog");
                for(int seed=1;seed<=30;seed++)
                {
                    Place(new Vector3(0,500,0),Vector3.up+Vector3.forward);cam.transform.position=new(0,500,0);cam.transform.rotation=Quaternion.LookRotation(Vector3.up);AmbientLife.ForcedSeed=seed;wildlife.SelectPopulation();
                    // Record the deterministic selection without making hidden animals visible for the camera.
                    log.WriteLine($"{seed},{wildlife.SelectedCount},{wildlife.habitats.Count(h=>h.species==Wildlife.Species.Bird)},{wildlife.habitats.Count(h=>h.species==Wildlife.Species.Squirrel)},{wildlife.habitats.Count(h=>h.species==Wildlife.Species.Frog)}");Check(wildlife.SelectedCount<=8,"Population capped seed="+seed);
                }
            }
            foreach(Wildlife.Species species in Enum.GetValues(typeof(Wildlife.Species)))
            {
                bool found=false;
                for(int seed=1;seed<=30&&!found;seed++)
                {
                    cam.GetComponent<ChaseCamera>().enabled=false;cam.transform.SetPositionAndRotation(new(0,500,0),Quaternion.LookRotation(Vector3.up));AmbientLife.ForcedSeed=seed;wildlife.SelectPopulation();
                    foreach(var site in wildlife.habitats.Where(h=>h.species==species))
                    {
                        Place(site.position-site.escape*12+Vector3.up*.8f,site.escape);race.vehicle.Body.isKinematic=true;cam.transform.SetPositionAndRotation(race.vehicle.transform.position,Quaternion.LookRotation(-site.escape));yield return new WaitForSeconds(.15f);
                        var animal=wildlife.GetComponentsInChildren<Transform>().FirstOrDefault(t=>t.name=="Wildlife "+species&&Vector3.Distance(t.position,site.position)<1);if(!animal)continue;found=true;
                        LivingWorldValidation.Capture(root+"/"+species+"-idle.png",animal.position+new Vector3(2,1.2f,3),animal.position+Vector3.up*.4f);
                        cam.GetComponent<ChaseCamera>().enabled=true;FindAnyObjectByType<ChaseCamera>().Snap();Shot(species+"-approach");race.vehicle.Body.isKinematic=false;race.vehicle.Body.linearVelocity=site.escape*6;yield return new WaitForSeconds(.7f);
                        LivingWorldValidation.Capture(root+"/"+species+"-flee.png",animal.position+new Vector3(3,2,4),animal.position+Vector3.up*.4f);Check(Vector3.Distance(animal.position,site.position)>.15f,species+" proximity movement/flee");
                        File.AppendAllText(root+"/species-scenarios.txt",$"{species}: lifeSeed={seed}, habitat={Array.IndexOf(wildlife.habitats,site)}, position={site.position:F3}\n");break;
                    }
                }
                Check(found,species+" reproducible visible fixture");
            }
            int old=wildlife.Seed;race.vehicle.GetComponent<VehicleRespawn>().TryRecoverLocal();Check(old==wildlife.Seed,"Local recovery retains wildlife selection");race.Flow.Pause();yield return new WaitForSecondsRealtime(.3f);Check(old==wildlife.Seed,"Pause retains selection");race.Flow.Resume();race.RestartRace();yield return null;Check(FindObjectsByType<Wildlife>().Length==1,"Restart keeps one pool");
        }
        IEnumerator Audio()
        {
            var listener=FindAnyObjectByType<AudioListener>();var capture=listener.gameObject.AddComponent<CorrectionAudioCapture>();
            var car=race.vehicle;var pad=InputSystem.AddDevice<Gamepad>();InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
            File.WriteAllText(root+"/audio.txt",$"Listener enabled={listener.enabled}; listeners={FindObjectsByType<AudioListener>().Length}; sampleRate={AudioSettings.outputSampleRate}; master={race.Flow.Save.Settings.master}; vehicle={race.Flow.Save.Settings.vehicle}; music={race.Flow.Save.Settings.music}; ambience={race.Flow.Save.Settings.ambience}; radio={race.Flow.Radio.Playing}\n");
            foreach(Wildlife.Species species in Enum.GetValues(typeof(Wildlife.Species)))
            {
                var site=wildlife.habitats.First(h=>h.species==species);float station=race.road.Project(site.position,out _);var p=race.road.At(station,out var f);foreach(var branch in FindObjectsByType<WoodlandRoute>()){float bs=branch.Project(site.position,out float bd);if(bd<Vector3.Distance(p,site.position))p=branch.At(bs,out f);}Place(p+Vector3.up*.7f,f);car.Body.isKinematic=true;car.enabled=true;car.GetComponent<VehicleInput>().enabled=true;InputSystem.QueueStateEvent(pad,new GamepadState{rightTrigger=.7f});yield return new WaitForSeconds(1);
                float until=Time.time+18;bool called=false;while(Time.time<until&&!(called=wildlife.Call(species,site.position)))yield return null;capture.Begin(4);string playingClip=wildlife.Voice.clip.name;float pitch=wildlife.Voice.pitch;yield return new WaitForSeconds(2.8f);
                Check(called,species+" spatial recording dispatched with normal engine/radio");File.AppendAllText(root+"/audio.txt",species+" clip="+playingClip+" pitch="+pitch+" distance="+Vector3.Distance(listener.transform.position,site.position)+" "+capture.Finish(root+"/"+species+"-mix.wav")+"\n");
                race.Flow.Save.Settings.ambience=0;yield return null;Check(wildlife.Voice.volume==0,"Ambience mute updates active "+species+" voice");race.Flow.Save.Settings.ambience=1;
            }
            if(race.Forest)
            {
                var life=race.GetComponent<AmbientLife>();life.SelectScenes();yield return new WaitForSeconds(3.2f);Place(life.batEntrance-life.batForward*20+Vector3.up,life.batForward,8);car.enabled=true;car.GetComponent<VehicleInput>().enabled=true;InputSystem.QueueStateEvent(pad,new GamepadState{rightTrigger=.7f});int before=life.Swarms;capture.Begin(4);yield return new WaitForSeconds(3.2f);Check(life.Swarms==before+1,"Natural approach triggers bat flight");File.AppendAllText(root+"/audio.txt","Bats "+capture.Finish(root+"/Bats-mix.wav")+"\n");
                Place(life.batEntrance-life.batForward*20+Vector3.up,life.batForward,8);yield return null;Check(life.Swarms==before+1,"Immediate repeat/recovery cannot spam bats");car.Body.isKinematic=true;yield return new WaitForSeconds(46);Check(life.Swarms==before+1,"Lingering beyond 45 second cooldown stays disarmed");
                Place(life.batEntrance-life.batForward*90,life.batForward);car.Body.isKinematic=true;yield return new WaitForSeconds(13);Place(life.batEntrance-life.batForward*20+Vector3.up,life.batForward,8);yield return null;Check(life.Swarms==before+2,"Leave 12 seconds rearms bats after cooldown");
            }
            race.Flow.Save.Settings.master=0;race.Flow.Save.ApplySettings();yield return new WaitForSeconds(.3f);capture.Begin(2);yield return new WaitForSeconds(1);var result=capture.Finish(root+"/master-muted.wav");File.AppendAllText(root+"/audio.txt","Muted "+result+"\n");Check(result.Contains("peak=0.000000"),"Master mute actual listener output is zero");InputSystem.RemoveDevice(pad);
            race.Flow.Save.Settings.master=.8f;race.Flow.Save.ApplySettings();race.RestartRace();yield return null;Check(wildlife.batFlight&&wildlife.birdCalls.All(c=>c),"Restart retains sound assets");
        }
        IEnumerator Drive()
        {
            // Ordinary route following, without teleporting to wildlife or forcing occupancy.
            var pilot=race.vehicle.gameObject.AddComponent<RoadDriver>();pilot.Initialize(race,race.vehicle,true,1,1);pilot.Racer=race.Racers[0];float until=Time.time+150;int occupied=0,empty=0;float next=0;var seen=new HashSet<string>();
            using var log=new StreamWriter(root+"/ordinary.csv");log.WriteLine("time,station,active,visible,sightings,calls");
            while(Time.time<until&&!race.ClassificationFinal)
            {
                yield return null;if(Time.time<next)continue;next=Time.time+1;int visible=wildlife.GetComponentsInChildren<Transform>().Count(t=>(t.name=="Wildlife Bird"||t.name=="Wildlife Squirrel"||t.name=="Wildlife Frog"||t.name=="Wildlife Deer"||t.name=="Wildlife Coyote")&&InFrame(t.position));if(visible>0)occupied++;else empty++;
                foreach(var animal in wildlife.GetComponentsInChildren<Transform>().Where(t=>(t.name=="Wildlife Deer"||t.name=="Wildlife Coyote")&&InFrame(t.position)&&cam.WorldToViewportPoint(t.position).x>.12f&&cam.WorldToViewportPoint(t.position).x<.88f))if(seen.Add(animal.name)){Shot("ordinary-"+animal.name);File.AppendAllText(root+"/ordinary-species.txt",animal.name+" time="+race.Clock+" seed="+wildlife.Seed+"\n");}
                log.WriteLine($"{race.Clock:F2},{race.road.Project(race.vehicle.transform.position,out _):F1},{wildlife.ActiveCount},{visible},{wildlife.Sightings},{wildlife.Calls}");if(visible>0&&occupied<4)Shot("ordinary-sighting-"+occupied);
                if((occupied+empty)%20==1){Shot("ordinary-route-"+(occupied+empty));File.AppendAllText(root+"/viewport.txt",$"time={race.Clock} car={race.vehicle.transform.position} cam={cam.transform.position} forward={cam.transform.forward} aspect={cam.aspect}\n"+string.Join("\n",wildlife.GetComponentsInChildren<Transform>().Where(t=>(t.name=="Wildlife Bird"||t.name=="Wildlife Squirrel"||t.name=="Wildlife Frog"||t.name=="Wildlife Deer"||t.name=="Wildlife Coyote")).Select(t=>$"{t.name} pos={t.position} vp={cam.WorldToViewportPoint(t.position)}"))+"\n");}
            }
            Check(occupied>0,"Ordinary run includes wildlife sightings");Check(empty>occupied,"Ordinary run has more empty periods than sightings");Check(race.Flow.Radio.Playing,"Radio continues during combined run");
            File.WriteAllText(root+"/ordinary-summary.txt",$"seed={wildlife.Seed} selected={wildlife.SelectedCount} occupiedSeconds={occupied} emptySeconds={empty} sightings={wildlife.Sightings} calls={wildlife.Calls}\n");
        }
        bool InFrame(Vector3 p){var v=cam.WorldToViewportPoint(p);return v.z>0&&v.z<65&&v.x>0&&v.x<1&&v.y>0&&v.y<1;}
    }
}
