#if UNITY_EDITOR || DEBUG
using System;
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
    // Development-only harness. Synthetic gate results always go to isolated validation storage.
    public sealed class Phase7Validation : MonoBehaviour
    {
        public string output = "Docs/Phase7";
        public string saveDirectory;
        public bool quitWhenDone;
        readonly List<string> rows = new();
        RaceFlow flow; RaceDirector race; Gamepad pad; Keyboard keyboard; Mouse mouse;
        int failures;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void AutoRun()
        {
            var args = Environment.GetCommandLineArgs();
            int index = Array.IndexOf(args, "-racerValidate");
            if (index < 0 || index + 1 >= args.Length) return;
            var runner = new GameObject("Phase 7 isolated validation").AddComponent<Phase7Validation>();
            runner.output = Path.GetFullPath(args[index+1]); runner.quitWhenDone = true;
        }
        void Check(bool ok, string text) { rows.Add((ok?"PASS ":"FAIL ") + text); if(!ok)failures++; }
        void Pad(GamepadState state) => InputSystem.QueueStateEvent(pad,state);
        IEnumerator KeyPress(Key key)
        { InputSystem.QueueStateEvent(keyboard,new KeyboardState(key)); yield return Wait(.1f); InputSystem.QueueStateEvent(keyboard,new KeyboardState()); yield return Wait(.1f); }
        IEnumerator Wait(float seconds) { yield return new WaitForSecondsRealtime(seconds); }
        IEnumerator Capture(string name)
        { yield return new WaitForEndOfFrame(); ScreenCapture.CaptureScreenshot(Path.Combine(output,name+".png")); yield return null; }
        void Cross(int gate, bool forward, ref double time)
        {
            var g=race.gates[gate]; var a=g.transform.TransformPoint(new Vector3(0,0,forward?-2:2));
            var b=g.transform.TransformPoint(new Vector3(0,0,forward?2:-2));
            race.ResetSampling(a,time); time+=2; race.Sample(b,forward?g.transform.forward:-g.transform.forward,time);
        }
        IEnumerator Start()
        {
            yield return null; flow=FindAnyObjectByType<RaceFlow>(); race=flow.Race;
            Application.runInBackground=true;
            Directory.CreateDirectory(output);
            saveDirectory ??= Path.Combine(Path.GetFullPath(output),"isolated-save");
            flow.UseValidationSave(saveDirectory);
            rows.Add("Synthetic validation only; isolated save: " + saveDirectory);
            rows.Add("Loaded previous launch best lap="+flow.Save.Best.lap+", race="+flow.Save.Best.race+", master="+flow.Save.Settings.master);
            var input=race.vehicle.GetComponent<VehicleInput>(); var body=race.vehicle.Body;
            pad=InputSystem.AddDevice<Gamepad>();
            keyboard=InputSystem.AddDevice<Keyboard>(); mouse=InputSystem.AddDevice<Mouse>();
            Check(flow.State==RaceFlow.Stage.Ready && Time.timeScale==0 && body.isKinematic,"Launch ready, physics locked");
            yield return Capture("ready");
            Pad(new GamepadState().WithButton(GamepadButton.A)); yield return Wait(.15f); Pad(new GamepadState());
            Check(flow.State==RaceFlow.Stage.Countdown,"Virtual A starts selected race action");
            var position=body.position; Pad(new GamepadState{rightTrigger=1,leftStick=Vector2.one}); yield return Wait(.35f);
            Check(!input.enabled && body.position==position && !race.Progress.Started,"Countdown prevents head start and timing");
            Pad(new GamepadState().WithButton(GamepadButton.Start)); yield return Wait(.1f); Pad(new GamepadState());
            float count=flow.CountdownRemaining; yield return Wait(.3f);
            Check(flow.State==RaceFlow.Stage.Paused && count==flow.CountdownRemaining,"Virtual Start pauses countdown without consuming it");
            yield return Capture("pause");
            yield return KeyPress(Key.Enter); Check(flow.State==RaceFlow.Stage.Countdown,"Keyboard Enter resumes without restarting");
            yield return KeyPress(Key.Escape); Check(flow.State==RaceFlow.Stage.Paused,"Keyboard Escape pauses");
            yield return KeyPress(Key.DownArrow); yield return KeyPress(Key.DownArrow); yield return KeyPress(Key.Space);
            Check(flow.State==RaceFlow.Stage.Settings,"Keyboard arrows/Space open selected Settings");
            yield return Capture("settings");
            var selected=EventSystem.current.currentSelectedGameObject;
            float master=flow.Save.Settings.master; yield return KeyPress(Key.Space);
            Check(flow.Save.Settings.master!=master && EventSystem.current.currentSelectedGameObject==selected,"Setting changes and retains focus");
            Pad(new GamepadState().WithButton(GamepadButton.B)); yield return Wait(.1f); Pad(new GamepadState());
            Check(flow.State==RaceFlow.Stage.Paused,"Virtual B returns settings to pause");
            Check(EventSystem.current.currentSelectedGameObject.GetComponentInChildren<UnityEngine.UI.Text>().text=="Settings","Back restores prior menu focus");
            var resume=FindObjectsByType<UnityEngine.UI.Button>().First(b=>b.GetComponentInChildren<UnityEngine.UI.Text>().text=="Resume");
            var point=RectTransformUtility.WorldToScreenPoint(null,resume.transform.position);
            InputSystem.QueueStateEvent(mouse,new MouseState{position=point}); yield return Wait(.1f);
            InputSystem.QueueStateEvent(mouse,new MouseState{position=point,buttons=1}); yield return Wait(.1f);
            InputSystem.QueueStateEvent(mouse,new MouseState{position=point}); yield return Wait(.1f);
            Check(flow.State==RaceFlow.Stage.Countdown,"Virtual mouse clicks Resume through Canvas raycast");
            yield return Wait(3.1f);
            Check(flow.State==RaceFlow.Stage.Racing && !body.isKinematic && input.enabled,"Resume completes countdown and restores driving");
            yield return Capture("hud");
            Pad(new GamepadState{rightTrigger=.5f}); yield return Wait(.6f); Pad(new GamepadState());
            Check(body.linearVelocity.magnitude>1,"Ordinary-frame virtual trigger drives car");
            flow.Pause(); position=body.position; double clock=race.Clock; yield return Wait(.3f);
            Check(body.position==position && race.Clock==clock && AudioListener.pause,"Pause freezes physics/timer and pauses environmental audio");
            Pad(new GamepadState().WithButton(GamepadButton.Y)); yield return Wait(.1f); Pad(new GamepadState());
            Check(body.position==position,"Menu Y does not reset vehicle");
            flow.Resume(); yield return Wait(.1f);
            double t=100; Cross(0,true,ref t); Cross(1,true,ref t);
            Pad(new GamepadState().WithButton(GamepadButton.Y)); yield return Wait(.12f); Pad(new GamepadState());
            Check(!race.Progress.LapActive && race.Progress.CompletedLaps==0 && flow.Notice!=null,"Virtual Y abandons current lap and shows reset feedback");
            double initialBest=flow.Save.Best.lap;
            t=200; Cross(0,true,ref t); Cross(2,true,ref t); Cross(0,true,ref t);
            Check(race.Progress.CompletedLaps==0 && flow.Save.Best.lap==initialBest,"Skipped checkpoint awards no lap/record");
            Cross(1,true,ref t); Cross(1,true,ref t); Check(!race.Progress.LapValid,"Repeated checkpoint invalidates");
            Cross(0,true,ref t); Cross(1,false,ref t); Check(!race.Progress.LapValid,"Wrong-way checkpoint invalidates");
            for(int i=0;i<5;i++)Cross(0,true,ref t);
            Check(race.Progress.CompletedLaps==0,"Repeated finish awards no laps");
            race.RestartRace(); yield return Wait(3.2f);
            Check(!race.Progress.Started && race.Progress.CompletedLaps==0,"Restart clears race then completes fresh countdown");
            t=1000; Cross(0,true,ref t);
            for(int lap=0;lap<race.laps;lap++)
            { for(int gate=1;gate<race.gates.Length;gate++)Cross(gate,true,ref t); Cross(0,true,ref t); }
            Check(race.Progress.Finished && flow.State==RaceFlow.Stage.Results && body.isKinematic,"Ordered synthetic gate finish produces results and stops car");
            Check(race.Progress.LapTimes.Count==3 && race.Progress.BestLap==race.Progress.LapTimes.Min(),"Results reuse all valid lap splits and current-race best");
            double savedRace=flow.Save.Best.race; Cross(0,true,ref t);
            Check(race.Progress.CompletedLaps==3 && flow.Save.Best.race==savedRace,"Finish repeated after results cannot award duplicate laps/records");
            yield return Capture("results");
            flow.OpenSettings(); flow.CloseSettings(); Check(flow.State==RaceFlow.Stage.Results,"Settings returns to results");
            flow.Save.Settings.master=.7f; flow.Save.Settings.vsync=false; flow.Save.Settings.frameLimit=60; flow.Save.SaveSettings(); flow.Save.ApplySettings();
            var reloaded=new RacerSave(saveDirectory,"street-loop-gates-v1-laps"+race.laps);
            Check(reloaded.Best.race==savedRace && reloaded.Best.lap>0 && reloaded.Settings.master==.7f,"Isolated bests/settings deserialize correctly");
            var malformed=Path.Combine(output,"malformed-save"); Directory.CreateDirectory(malformed);
            File.WriteAllText(Path.Combine(malformed,"records.json"),"{broken"); File.WriteAllText(Path.Combine(malformed,"settings.json"),"{\"version\":1,\"master\":-5}");
            var bad=new RacerSave(malformed,"isolated"); Check(bad.Best.lap==0 && bad.Settings.master==.8f,"Malformed/invalid isolated data loads defaults");
            var missing=new RacerSave(Path.Combine(output,Guid.NewGuid().ToString()),"isolated"); Check(missing.Best.race==0 && missing.Settings.vsync,"Missing data defaults");
            for(int i=0;i<3;i++)
            {
                race.RestartRace(); yield return Wait(.1f); flow.Pause(); race.RestartRace();
                Check(flow.State==RaceFlow.Stage.Countdown && race.Progress.CompletedLaps==0 && flow.Save.Best.race==savedRace,"Repeated paused/results restart "+i+" preserves PB");
            }
            yield return Wait(3.2f);
            var prop=FindObjectsByType<BreakableProp>()[0];
            body.linearVelocity=Vector3.forward*10; prop.SendMessage("OnTriggerEnter",race.vehicle.GetComponent<Collider>());
            Check(prop.IsBroken,"Prop contact still breaks once"); race.RestartRace(); yield return null;
            Check(!prop.IsBroken && !prop.PendingRestore,"Race restart restores prop safely");
            yield return Wait(3.2f);
            body.position=prop.GetComponent<BoxCollider>().bounds.center; race.vehicle.transform.position=body.position; Physics.SyncTransforms();
            BreakableProp.RestoreRace(); Check(prop.PendingRestore,"Prop restoration defers while vehicle overlaps");
            race.RestartRace(); yield return null; Check(!prop.PendingRestore,"Restart moves vehicle before restoring overlapped prop");
            yield return Wait(3.2f);
            flow.Save.Settings.ambience=0; yield return Wait(.1f);
            Check(FindObjectsByType<AudioSource>().Where(a=>!a.ignoreListenerPause).All(a=>a.volume==0),"Ambience slider mutes both environmental voices");
            flow.Save.Settings.ambience=1;
            var frames=new List<float>(); float until=Time.unscaledTime+3;
            while(Time.unscaledTime<until){frames.Add(Time.unscaledDeltaTime*1000);yield return null;}
            frames.Sort(); rows.Add($"Ordinary idle frames n={frames.Count}, median={frames[frames.Count/2]:F2}ms p95={frames[(int)(frames.Count*.95f)]:F2}ms max={frames.Last():F2}ms; no performance parity claim.");
            Check(FindObjectsByType<AudioSource>().Length==3,"Repeated transitions retain exactly three audio sources");
            rows.Add("COMPLETE failures="+failures); File.WriteAllLines(Path.Combine(output,"validation.txt"),rows);
            race.RestartRace();
            if(quitWhenDone)Application.Quit(); else Destroy(gameObject);
        }
        void OnDestroy(){if(pad!=null)InputSystem.RemoveDevice(pad);if(keyboard!=null)InputSystem.RemoveDevice(keyboard);if(mouse!=null)InputSystem.RemoveDevice(mouse);}
    }
}
#endif
