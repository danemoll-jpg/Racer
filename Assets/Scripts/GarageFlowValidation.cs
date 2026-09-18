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
    public sealed class GarageFlowValidation : MonoBehaviour
    {
        readonly List<string> rows=new();
        Gamepad pad; Keyboard keys; Mouse mouse;
        RaceFlow flow;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            var args=System.Environment.GetCommandLineArgs();
            if(System.Array.IndexOf(args,"-racerGarageFlowTest")>=0 && System.Array.IndexOf(args,"-racerTestSave")>=0) Launch();
        }
        public static void Launch()=>new GameObject("Garage flow validation").AddComponent<GarageFlowValidation>();
        void Check(bool pass,string label)=>rows.Add((pass?"PASS ":"FAIL ")+label);
        IEnumerator Press(Key key) { InputSystem.QueueStateEvent(keys,new KeyboardState(key)); yield return new WaitForSecondsRealtime(.12f); InputSystem.QueueStateEvent(keys,new KeyboardState()); yield return new WaitForSecondsRealtime(.12f); }
        IEnumerator Confirm() { InputSystem.QueueStateEvent(pad,new GamepadState().WithButton(GamepadButton.South)); yield return new WaitForSecondsRealtime(.12f); InputSystem.QueueStateEvent(pad,new GamepadState()); yield return new WaitForSecondsRealtime(.12f); }
        IEnumerator Start()
        {
            yield return null; flow=FindAnyObjectByType<RaceFlow>(); Application.runInBackground=true;
#if UNITY_EDITOR
            flow.UseValidationSave(Path.GetFullPath("Temp/CR026-027-flow-"+System.Guid.NewGuid().ToString("N")));
            InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
#endif
            InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
            pad=InputSystem.AddDevice<Gamepad>(); keys=InputSystem.AddDevice<Keyboard>(); mouse=InputSystem.AddDevice<Mouse>();
            flow.OpenGarage();
            yield return Press(Key.DownArrow); yield return Confirm();
            Check(flow.Save.Settings.vehicleId=="tourer","Keyboard navigation and virtual gamepad confirm select Longroof");
            InputSystem.QueueStateEvent(pad,new GamepadState().WithButton(GamepadButton.DpadDown)); yield return new WaitForSecondsRealtime(.15f);
            InputSystem.QueueStateEvent(pad,new GamepadState()); yield return new WaitForSecondsRealtime(.15f); yield return Confirm();
            Check(flow.Save.Settings.vehicleId=="moto","Virtual D-pad navigation and A confirmation");
            foreach(var profile in VehicleProfile.All)
            {
                flow.SelectVehicle(profile.Id); yield return null;
                ScreenCapture.CaptureScreenshot(Path.Combine(GarageValidation.Output,"garage-"+profile.Id+".png"));
                yield return new WaitForSecondsRealtime(.15f);
                Check(flow.Race.vehicle.GetComponent<VehicleConfiguration>().profileId==profile.Id,"Applied "+profile.Id);
                var reload=new RacerSave(flow.Save.DirectoryPath,"legacy"); Check(reload.Settings.vehicleId==profile.Id,"Persisted "+profile.Id);
            }
            var button=FindObjectsByType<UnityEngine.UI.Button>().First(b=>b.gameObject.activeInHierarchy && b.GetComponentInChildren<UnityEngine.UI.Text>().text.Contains("Street Classic"));
            var point=RectTransformUtility.WorldToScreenPoint(null,button.transform.position);
            InputSystem.QueueStateEvent(mouse,new MouseState{position=point}); yield return new WaitForSecondsRealtime(.15f);
            InputSystem.QueueStateEvent(mouse,new MouseState{position=point,buttons=1}); yield return new WaitForSecondsRealtime(.15f);
            InputSystem.QueueStateEvent(mouse,new MouseState{position=point}); yield return new WaitForSecondsRealtime(.15f);
            Check(flow.Save.Settings.vehicleId=="original","Virtual mouse garage selection through uGUI raycast");
            yield return Press(Key.Escape); Check(flow.State==RaceFlow.Stage.Ready,"Escape returns garage to Ready");
            flow.Race.opponents=true; flow.Race.traffic=true; flow.StartRace();
            int dings=flow.CheckpointDings; flow.CheckpointFeedback(true,0,0); Check(flow.CheckpointDings==dings,"Countdown suppresses checkpoint cue");
            flow.Pause(); float remaining=flow.CountdownRemaining; yield return new WaitForSecondsRealtime(.3f); Check(flow.CountdownRemaining==remaining,"Pause holds countdown"); flow.Resume();
            while(flow.State!=RaceFlow.Stage.Racing) yield return null;
            Check(flow.Race.Racers.Count==4 && flow.Race.Drivers.Count==7,"Three opponents and four traffic");
            Check(flow.Race.Drivers.All(d=>d.GetComponents<AudioSource>().Length==0),"AI and traffic own no engine sources");
            Check(FindObjectsByType<AudioListener>().Length==1 && flow.Race.vehicle.GetComponents<AudioSource>().Length==6,"One listener and six player voices after repeated selection");
            flow.SelectVehicle("moto"); Check(flow.Race.vehicle.GetComponent<VehicleConfiguration>().profileId=="original","Driving rejects vehicle switch");
            flow.CheckpointFeedback(true,0,0); Check(flow.CheckpointDings==dings+1,"Player checkpoint cue");
            int buzz=flow.CheckpointBuzzes; flow.CheckpointFeedback(false,5,1); flow.CheckpointFeedback(false,5,1); Check(flow.CheckpointBuzzes==buzz+1,"Miss cue cooldown");
            var player=flow.Race.Progress; var ai=flow.Race.Racers[1].Progress; ai.BeginTiming(1); ai.Cross(0,true,2); ai.Miss(1,5);
            Check(player.MissedGates==0 && flow.CheckpointBuzzes==buzz+1,"Independent AI progress and player cue ownership");
            flow.Save.Settings.master=0; flow.Save.Settings.vehicle=.3f; flow.Save.ApplySettings(); flow.Save.SaveSettings();
            Check(AudioListener.volume==0,"Master mute preserved");
            flow.Race.RestartRace(); yield return null;
            Check(flow.CheckpointDings==0 && flow.CheckpointBuzzes==0 && flow.Race.Progress.PenaltySeconds==0,"Restart clears cues and race penalties");
            flow.CompleteResults(); flow.OpenGarage(); flow.SelectVehicle("atv"); flow.CloseGarage(); flow.StartRace();
            Check(flow.State==RaceFlow.Stage.Countdown && flow.Save.Settings.master==0 && flow.Save.Settings.vehicle==.3f,"Results / garage / race again retains volumes");
            flow.Pause(); Directory.CreateDirectory(GarageValidation.Output); File.WriteAllLines(Path.Combine(GarageValidation.Output,"flow.txt"),rows);
            Destroy(gameObject);
        }
        void OnDestroy() { if(pad!=null)InputSystem.RemoveDevice(pad); if(keys!=null)InputSystem.RemoveDevice(keys); if(mouse!=null)InputSystem.RemoveDevice(mouse); }
    }
}
