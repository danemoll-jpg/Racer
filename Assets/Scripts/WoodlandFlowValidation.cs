using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Racer
{
    public sealed class WoodlandFlowValidation : MonoBehaviour
    {
        RaceFlow flow;
        RaceDirector race;
        Gamepad pad; Keyboard keys; Mouse mouse;
        readonly List<string> rows=new();
        public static void Launch()=>new GameObject("Revision flow validation").AddComponent<WoodlandFlowValidation>();
        string Output=>"Docs/CR041-045/flow/"+(Application.isEditor?"editor":"standalone")+"-flow.txt";
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot() { var args=System.Environment.GetCommandLineArgs(); if(System.Array.IndexOf(args,"-woodlandFlow")>=0 && System.Array.IndexOf(args,"-racerTestSave")>=0) { Application.runInBackground=true; Launch(); } }
        void Check(bool pass,string label) { rows.Add((pass?"PASS ":"FAIL ")+label); File.WriteAllLines(Output,rows); }
        IEnumerator Key(Key key) { InputSystem.QueueStateEvent(keys,new KeyboardState(key)); yield return new WaitForSecondsRealtime(.12f); InputSystem.QueueStateEvent(keys,new KeyboardState()); yield return new WaitForSecondsRealtime(.12f); }
        IEnumerator Pad(GamepadButton button) { InputSystem.QueueStateEvent(pad,new GamepadState().WithButton(button)); yield return new WaitForSecondsRealtime(.12f); InputSystem.QueueStateEvent(pad,new GamepadState()); yield return new WaitForSecondsRealtime(.12f); }
        UnityEngine.UI.Button Button(string contains)=>FindObjectsByType<UnityEngine.UI.Button>().First(b=>b.gameObject.activeInHierarchy && b.GetComponentInChildren<UnityEngine.UI.Text>().text.Contains(contains));
        IEnumerator Navigate(string label,bool gamepad)
        {
            var target=Button(label).gameObject;
            for(int i=0;i<18 && EventSystem.current.currentSelectedGameObject!=target;i++)
                if(gamepad) yield return Pad(GamepadButton.DpadDown); else yield return Key(UnityEngine.InputSystem.Key.DownArrow);
            Check(EventSystem.current.currentSelectedGameObject==target,(gamepad?"Gamepad":"Keyboard")+" navigation reaches "+label);
        }
        IEnumerator MouseClick(UnityEngine.UI.Button button)
        {
            yield return null; Canvas.ForceUpdateCanvases();
            var p=RectTransformUtility.WorldToScreenPoint(null,button.transform.position);
            InputSystem.QueueStateEvent(mouse,new MouseState{position=p}); yield return new WaitForSecondsRealtime(.12f);
            InputSystem.QueueStateEvent(mouse,new MouseState{position=p,buttons=1}); yield return new WaitForSecondsRealtime(.12f);
            InputSystem.QueueStateEvent(mouse,new MouseState{position=p}); yield return new WaitForSecondsRealtime(.12f);
        }
        IEnumerator Start()
        {
            yield return null; flow=FindAnyObjectByType<RaceFlow>(); race=flow.Race;
            Application.runInBackground=true; Directory.CreateDirectory("Docs/CR041-045/flow");
#if UNITY_EDITOR
            flow.UseValidationSave(Path.GetFullPath("Temp/woodland-flow-"+System.Guid.NewGuid().ToString("N")));
            InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
#endif
            InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
            keys=InputSystem.AddDevice<Keyboard>(); pad=InputSystem.AddDevice<Gamepad>(); mouse=InputSystem.AddDevice<Mouse>();
            rows.Add("Virtual keyboard/mouse/gamepad through Input System and uGUI; no physical-controller claim.");
            foreach(var profile in VehicleProfile.All)
            {
                flow.OpenGarage(); flow.SelectVehicle(profile.Id); yield return null;
                var unpaintedBefore=VehiclePaint.UnpaintedSnapshot(race.vehicle.transform);
                yield return MouseClick(Button("Red"));
                Check(flow.SelectedColor==1,"Mouse swatch "+profile.Id);
                EventSystem.current.SetSelectedGameObject(Button("Black").gameObject); yield return Pad(GamepadButton.South);
                Check(flow.SelectedColor==6,"Gamepad swatch "+profile.Id);
                var load=new RacerSave(flow.Save.DirectoryPath,"legacy");
                int i=System.Array.IndexOf(VehicleProfile.All,profile);
                Check(load.Settings.bodyColors[i]==6,"Persistent per-profile color "+profile.Id);
                var renderers=race.vehicle.GetComponentsInChildren<Renderer>();
                var painted=renderers.Where(r=>VehiclePaint.IsBodyPaint(r.sharedMaterial)).ToArray();
                var block=new MaterialPropertyBlock();
                Check(painted.Length>0 && painted.All(r=>{r.GetPropertyBlock(block);return block.GetColor("_BaseColor")==VehiclePaint.Colors[6];}),"Body paint applied "+profile.Id);
                Check(VehiclePaint.UnpaintedUnchanged(race.vehicle.transform,unpaintedBefore),"Trim/rider unchanged "+profile.Id);
                ScreenCapture.CaptureScreenshot(Path.GetFullPath("Docs/CR041-045/flow/garage-"+profile.Id+"-"+(Application.isEditor?"editor":"standalone")+".png"));
                yield return new WaitForSecondsRealtime(.15f); flow.CloseGarage();
            }
            flow.OpenRoster();
            for(int slot=0;slot<3;slot++)
                for(int choice=0;choice<6;choice++) { flow.CycleOpponent(slot); yield return null; Check(VehicleProfile.All.Any(p=>p.Id==race.opponentRoster[slot]),$"Resolved slot {slot+1} / {flow.Save.Settings.opponentChoices[slot]}"); }
            flow.MixedRoster(); var roster=string.Join(",",race.opponentRoster); flow.CloseGarage();
            foreach(var chosen in VehicleProfile.All)
            {
                race.opponents=true; race.traffic=false; race.opponentRoster=new[]{chosen.Id,chosen.Id,chosen.Id}; flow.StartRace(); yield return null;
                Check(race.Racers.Skip(1).All(r=>r.Car.GetComponent<VehicleConfiguration>().profileId==chosen.Id && Mathf.Approximately(r.Car.topSpeed,chosen.Speed) && Mathf.Approximately(r.Car.Body.mass,chosen.Mass)),"Actual opponent capabilities "+chosen.Id);
                Check(race.Racers.All(a=>race.Racers.All(b=>a==b || !Physics.ComputePenetration(a.Car.GetComponent<BoxCollider>(),a.Car.Body.position,a.Car.Body.rotation,b.Car.GetComponent<BoxCollider>(),b.Car.Body.position,b.Car.Body.rotation,out _,out _))),"All-profile grid clear "+chosen.Id);
                flow.Pause(); flow.QuitRace(); yield return null;
            }
            race.opponentRoster=roster.Split(',');
            race.opponents=true; race.traffic=true; flow.StartRace();
            Check(string.Join(",",race.opponentRoster)==roster && race.Racers.Skip(1).Select(r=>r.Car.GetComponent<VehicleConfiguration>().profileId).SequenceEqual(race.opponentRoster),"True mixed profiles before GO");
            Check(race.Racers.All(a=>race.Racers.All(b=>a==b || !Physics.ComputePenetration(a.Car.GetComponent<BoxCollider>(),a.Car.Body.position,a.Car.Body.rotation,b.Car.GetComponent<BoxCollider>(),b.Car.Body.position,b.Car.Body.rotation,out _,out _))),"Profile-sized grid no overlaps");
            flow.Pause(); yield return Navigate("Quit Race",false); yield return Key(UnityEngine.InputSystem.Key.Space);
            Check(flow.State==RaceFlow.Stage.Ready && race.Drivers.Count==0 && !race.Progress.Started && flow.Save.Best.race==0,"Keyboard Quit Race during countdown; no incomplete record");
            flow.StartRace(); while(flow.State!=RaceFlow.Stage.Racing) yield return null;
            yield return Pad(GamepadButton.Start);
            yield return Navigate("Quit Race",true); yield return Pad(GamepadButton.South);
            Check(flow.State==RaceFlow.Stage.Ready && race.Drivers.Count==0,"Controller Pause / Quit Race during racing");
            flow.StartRace(); while(flow.State!=RaceFlow.Stage.Racing) yield return null;
            flow.Pause(); yield return MouseClick(Button("Quit Race"));
            Check(flow.State==RaceFlow.Stage.Ready && race.Drivers.Count==0,"Mouse Quit Race");
            flow.StartRace(); Check(string.Join(",",race.opponentRoster)==roster,"Restart/rematch roster stable");
            while(flow.State!=RaceFlow.Stage.Racing) yield return null;
            flow.Pause(); flow.QuitRace(); race.opponents=race.traffic=false;
            foreach(var profile in VehicleProfile.All)
            {
                flow.OpenGarage(); flow.SelectVehicle(profile.Id); flow.CloseGarage(); flow.StartRace();
                while(flow.State!=RaceFlow.Stage.Racing) yield return null;
                var car=race.vehicle; var recovery=car.GetComponent<VehicleRespawn>();
                var p=race.Progress; p.Cross(0,true,race.Clock-10); for(int g=1;g<=p.CheckpointCount;g++) p.Cross(g,true,race.Clock-5); p.Cross(0,true,race.Clock-2);
                for(int g=1;g<=4;g++) p.Cross(g,true,race.Clock); p.Miss(5,5);
                foreach(string scenario in new[]{"upright","flipped","slope","tree","moving-car"})
                {
                    float s=race.road.Project(race.gates[5].transform.position,out _)+25;
                    if(scenario=="slope") s=race.road.Project(new Vector3(319,0,400),out _);
                    var pos=race.road.At(s,out var f)+Vector3.up*.8f;
                    if(scenario=="tree")
                    {
                        var tree=FindObjectsByType<Collider>().Where(c=>!c.isTrigger && c.name.ToLowerInvariant().Contains("trunk")).OrderBy(c=>(c.transform.position-pos).sqrMagnitude).FirstOrDefault();
                        if(tree) pos=tree.bounds.center+Vector3.right*1.5f;
                    }
                    SetPose(car,pos,Quaternion.LookRotation(Vector3.ProjectOnPlane(f,Vector3.up))*Quaternion.Euler(0,0,scenario=="flipped"?180:0));
                    race.ResetSampling(pos,Time.timeAsDouble);
                    int next=p.NextGate,laps=p.CompletedLaps,misses=p.MissedGates; double penalty=p.PenaltySeconds,time=p.RaceTime(race.Clock);
                    ArcadeVehicle obstacle=null;
                    if(scenario=="moving-car")
                    {
                        var go=Instantiate(car.gameObject); obstacle=go.GetComponent<ArcadeVehicle>(); obstacle.enabled=false; go.GetComponent<VehicleInput>().enabled=false; go.GetComponent<VehicleRespawn>().enabled=false;
                        obstacle.GetComponent<VehicleConfiguration>().Apply("original"); SetPose(obstacle,pos+f*6,Quaternion.LookRotation(-f)); obstacle.Body.linearVelocity=-f*8;
                    }
                    float before=race.road.Project(pos,out _);
                    bool recovered=recovery.TryRecoverLocal();
                    float after=race.road.Project(car.Body.position,out _); float delta=Mathf.Repeat(after-before+race.road.Length*.5f,race.road.Length)-race.road.Length*.5f;
                    Check(recovered && car.transform.up.y>.65f && delta<=.1f && Vector3.Distance(pos,car.Body.position)<61 && car.Body.linearVelocity.magnitude<.01f,$"Local {profile.Id} {scenario}: recovered={recovered} move={Vector3.Distance(pos,car.Body.position):F2} forward={delta:F3}");
                    Check(p.NextGate==next && p.CompletedLaps==laps && p.MissedGates==misses && p.PenaltySeconds==penalty && p.LapActive && p.RaceTime(race.Clock)>=time,$"Preserve progress/time/penalties {profile.Id} {scenario}");
                    // No movement is sampled as recovery travel or gate credit.
                    race.Sample(car.Body.position,car.transform.forward,race.Clock);
                    Check(p.NextGate==next && p.MissedGates==misses,$"No recovery gate credit/miss chain {profile.Id} {scenario}");
                    if(obstacle) Destroy(obstacle.gameObject);
                }
                var localOrigin=car.Body.position; var localHeading=Quaternion.Euler(0,car.transform.eulerAngles.y,0);
                SetPose(car,localOrigin,localHeading*Quaternion.Euler(0,0,180)); race.ResetSampling(localOrigin,Time.timeAsDouble);
                yield return Key(UnityEngine.InputSystem.Key.R);
                Check(car.transform.up.y>.65f && p.CompletedLaps==1 && p.NextGate==6,"Keyboard R local recovery "+profile.Id);
                localOrigin=car.Body.position; SetPose(car,localOrigin,localHeading*Quaternion.Euler(0,0,180)); race.ResetSampling(localOrigin,Time.timeAsDouble);
                yield return Pad(GamepadButton.North);
                Check(car.transform.up.y>.65f && p.CompletedLaps==1 && p.NextGate==6,"Virtual gamepad Y local recovery "+profile.Id);
                flow.Pause(); flow.QuitRace();
            }
            Check(FindObjectsByType<AudioListener>().Length==1,"One audio listener");
            flow.OpenGarage(); flow.SelectVehicle("moto"); flow.CloseGarage();
            Check(flow.SelectedColor==6,"Color survives profile switches and restarts");
            flow.StartRace(); while(flow.State!=RaceFlow.Stage.Racing) yield return null;
            // A finish-plane recovery must not manufacture a lap, including repeated requests.
            var progress=race.Progress; progress.Cross(0,true,race.Clock-10);
            for(int gate=1;gate<=progress.CheckpointCount;gate++) progress.Cross(gate,true,race.Clock-5);
            race.Racers[0].FinishArmed=true;
            var finish=race.gates[0].transform;
            var finishPos=finish.position-finish.forward*2+Vector3.up*.8f;
            SetPose(race.vehicle,finishPos,Quaternion.LookRotation(finish.forward)); race.ResetSampling(finishPos,Time.timeAsDouble);
            int completed=progress.CompletedLaps;
            for(int repeat=0;repeat<3;repeat++) { race.vehicle.GetComponent<VehicleRespawn>().TryRecoverLocal(); race.Sample(race.vehicle.Body.position,race.vehicle.transform.forward,race.Clock); }
            Check(progress.CompletedLaps==completed && !progress.Finished,"Repeated local recovery beside finish grants no lap/finish");
            var recoveryState=race.vehicle.GetComponent<VehicleRespawn>(); recoveryState.CancelRecovery();
            SetPose(race.vehicle,new Vector3(0,10000,0),Quaternion.identity); var blocked=race.vehicle.Body.position;
            bool fallback=recoveryState.TryRecoverLocal();
            Check(!fallback && recoveryState.Pending && Vector3.Distance(blocked,race.vehicle.Body.position)<.01f,"No support/history: wait locally, never silently START");
            recoveryState.CancelRecovery(); SetPose(race.vehicle,finishPos,Quaternion.LookRotation(finish.forward)); race.ResetSampling(finishPos,Time.timeAsDouble);
            var settings=flow.Save.Settings; settings.master=.4f; settings.vehicle=.3f; settings.ambience=.2f; settings.feedback=.5f; flow.Save.ApplySettings(); flow.Save.SaveSettings();
            flow.Pause(); flow.OpenSettings(); flow.CloseSettings(); flow.Resume();
            Check(Mathf.Approximately(AudioListener.volume,.4f) && new RacerSave(flow.Save.DirectoryPath,"legacy").Settings.vehicle==.3f,"Settings and audio volumes retained across pause/settings");
            int dings=flow.CheckpointDings,buzzes=flow.CheckpointBuzzes;
            flow.CheckpointFeedback(true,0,0); flow.CheckpointFeedback(false,5,1); flow.CheckpointFeedback(false,5,1);
            Check(flow.CheckpointDings==dings+1 && flow.CheckpointBuzzes==buzzes+1 && flow.PenaltyNotice.Contains("2 gates  +10s"),"Checkpoint ding; throttled buzz retains both visible five-second charges");
            yield return new WaitForSecondsRealtime(5.2f);
            Check(flow.PenaltyNotice==null,"Temporary penalty notice expires");
            ScreenCapture.CaptureScreenshot(Path.GetFullPath("Docs/CR041-045/flow/hud-"+Screen.width+"x"+Screen.height+".png"));
            yield return new WaitForSecondsRealtime(.3f);
            flow.Pause(); Check(Time.timeScale==0,"Pause stops simulation");
            flow.QuitRace(); Check(!AudioListener.pause && race.vehicle.GetComponents<AudioSource>().All(s=>s.volume==0),"Quit Race clears audio and listener pause");
            flow.StartRace(); while(flow.State!=RaceFlow.Stage.Racing) yield return null;
            var prop=GameObject.Find("Mailbox - Dan - blue X").GetComponent<BreakableProp>();
            var direction=-prop.transform.forward; var place=prop.transform.position-direction*8;
            if(Physics.Raycast(place+Vector3.up*20,Vector3.down,out var support,60,1)) place=support.point+Vector3.up*.7f;
            SetPose(race.vehicle,place,Quaternion.LookRotation(direction)); race.vehicle.Body.linearVelocity=direction*12; race.ResetSampling(place,Time.timeAsDouble);
            InputSystem.QueueStateEvent(pad,new GamepadState{rightTrigger=.5f}); yield return new WaitForSeconds(1.5f);
            Check(prop.IsBroken,"Physical mailbox breakage"); InputSystem.QueueStateEvent(pad,new GamepadState());
            yield return new WaitForSeconds(4.5f); Check(BreakableProp.MovingDebrisCount==0,"Breakable debris cleanup");
            flow.StartRace(); yield return null;
            Check(!prop.IsBroken && !prop.PendingRestore && race.Progress.PenaltySeconds==0,"Explicit Restart Race restores props and clears event");
            flow.Pause(); flow.QuitRace();
            race.opponents=true; flow.StartRace(); while(flow.State!=RaceFlow.Stage.Racing) yield return null;
            var finishPoint=race.gates[0].transform;
            SetPose(race.vehicle,finishPoint.position+finishPoint.forward*2+Vector3.up*.8f,Quaternion.LookRotation(finishPoint.forward));
            race.vehicle.Body.linearVelocity=finishPoint.forward*20;
            race.ResetSampling(race.vehicle.Body.position,Time.timeAsDouble);
            var finishProgress=race.Progress; finishProgress.Cross(0,true,race.Clock-60);
            for(int lap=0;lap<race.laps;lap++)
            {
                for(int gate=1;gate<=finishProgress.CheckpointCount;gate++) finishProgress.Cross(gate,true,race.Clock-50+lap*15);
                finishProgress.Cross(0,true,race.Clock-40+lap*15);
            }
            var finishOrigin=race.vehicle.Body.position; flow.LapCompleted();
            yield return new WaitForSeconds(8);
            Check(finishProgress.Finished && Vector3.Distance(finishOrigin,race.vehicle.Body.position)>40,"Finished player clears line with ordinary motor control");
            var parked=race.vehicle.Body.position; yield return new WaitForSeconds(4);
            Check(race.vehicle.Body.isKinematic && Vector3.Distance(parked,race.vehicle.Body.position)<.1f,"Finished vehicle remains parked on slope without rollback");
            flow.Pause(); flow.QuitRace(); yield return null;
            Check(!race.vehicle.GetComponent<RoadDriver>() && race.Drivers.Count==0,"Quit removes finished-player runoff and opponents");
            rows.Add("DONE"); File.WriteAllLines(Output,rows); Destroy(gameObject); if(!Application.isEditor)Application.Quit();
        }
        static void SetPose(ArcadeVehicle car,Vector3 p,Quaternion q)
        { car.Body.position=p; car.Body.rotation=q; car.transform.SetPositionAndRotation(p,q); car.Body.linearVelocity=car.Body.angularVelocity=Vector3.zero; car.ClearSteering(); Physics.SyncTransforms(); }
        void OnDestroy() { if(pad!=null) InputSystem.RemoveDevice(pad); if(keys!=null) InputSystem.RemoveDevice(keys); if(mouse!=null) InputSystem.RemoveDevice(mouse); }
    }
}
