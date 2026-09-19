using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
namespace Racer
{
    // Opt-in standalone evidence. Fixtures are explicitly separate from physical driving.
    public sealed class CombinedReviewValidation:MonoBehaviour
    {
        const string Dir="Docs/CR046-049/standalone";
        RaceDirector race;Gamepad pad;readonly List<string> checks=new();
        void Check(bool pass,string label){checks.Add((pass?"PASS ":"FAIL ")+label);File.WriteAllLines(Dir+"/checks.txt",checks);}
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot(){var a=Environment.GetCommandLineArgs();if(a.Contains("-combinedReview")&&a.Contains("-racerTestSave")){Application.runInBackground=true;new GameObject("Combined review validation").AddComponent<CombinedReviewValidation>();}}
        IEnumerator Start()
        {
            yield return null;Directory.CreateDirectory(Dir);race=FindAnyObjectByType<RaceDirector>();var flow=race.Flow;
            InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;pad=InputSystem.AddDevice<Gamepad>();
            race.opponents=race.traffic=false;flow.StartRace();while(flow.State!=RaceFlow.Stage.Racing)yield return null;
            var car=race.vehicle;car.enabled=false;car.Body.isKinematic=true;
            float rejected=0;for(float s=0;s<race.road.Length;s+=2)if(!CircuitBoundary.Allowed(race.road.At(s,out _)))rejected++;
            Check(rejected==0,"Every original course centreline sample remains inside boundaries");
            foreach(var profile in VehicleProfile.All)
            foreach(var branch in race.Branches)
            {
                car.GetComponent<VehicleConfiguration>().Apply(profile.Id);
                var p=race.Progress;p.Restart();p.Cross(0,true,0);
                int first=Array.FindIndex(race.gates,g=>race.road.Project(g.transform.position,out _)>branch.entryRoad);
                for(int gate=1;gate<first;gate++)p.Cross(gate,true,gate);
                var state=race.Racers[0];state.Branch.Clear();double clock=100;
                var start=branch.At(0,out var f)-f*2+Vector3.up*.7f;race.ResetSampling(start,clock);
                void Move(Vector3 dest)
                {var from=state.Previous;int n=Mathf.Max(1,Mathf.CeilToInt(Vector3.Distance(from,dest)/.7f));for(int j=1;j<=n;j++)race.Sample(Vector3.Lerp(from,dest,j/(float)n),-f,clock+=.02);}
                Move(branch.At(12,out f)+Vector3.up*.7f);
                int next=p.NextGate;
                var middle=branch.At(branch.Length*.4f,out f);Move(middle+Vector3.up*12);
                Move(middle+Vector3.Cross(Vector3.up,f)*20+Vector3.down*8);
                for(int i=0;i<100;i++)race.Sample(state.Previous,-f,clock+=.02);
                Move(branch.At(10,out f)+Vector3.up*.7f);state.Branch.Recovered(branch.At(8,out _));
                Check(p.NextGate>=next&&p.MissedGates==0,profile.Id+" / "+branch.title+" wide/fall/stop/reverse retains bypass fixture");
                for(float s=10;s<=branch.Length;s+=1)Move(branch.At(s,out f)+Vector3.up*9);
                Move(branch.At(branch.Length,out f)+Vector3.up*9);
                Move(race.road.At(branch.exitRoad+45,out f)+Vector3.up*.7f);
                Check(p.MissedGates==0,profile.Id+" / "+branch.title+" airborne completion zero charges fixture");
                // Physically crossed later road gate resolves only unrelated missing gates.
                state.Branch.Clear();int expected=p.NextGate;
                if(expected>0&&expected+1<race.gates.Length)
                {var gate=race.gates[expected+1];race.ResetSampling(gate.transform.position-gate.transform.forward,clock);race.Sample(gate.transform.position+gate.transform.forward,-gate.transform.forward,clock+=.1);Check(p.MissedGates==1&&p.PenaltySeconds==5,profile.Id+" / "+branch.title+" unrelated later gate still charges exactly five");}
            }
            // Real rigidbody/FixedUpdate gate misses, forward movement with reversed body,
            // and flying boundary overshoots; only the initial pose/velocity is a fixture.
            foreach(var profile in VehicleProfile.All)
            {
                car.GetComponent<VehicleConfiguration>().Apply(profile.Id);
                foreach(bool miss in new[]{false,true})
                {
                    race.Progress.Restart();race.Progress.Cross(0,true,0);race.Racers[0].Branch.Clear();
                    var gate=race.gates[1];var forward=gate.transform.forward;var p=gate.transform.position-forward*7+Vector3.up*.8f+(miss?gate.transform.right*10:Vector3.zero);
                    Place(p,-forward,forward*24);car.enabled=false;
                    yield return new WaitForSeconds(1.4f);
                    Check(race.Progress.MissedGates==(miss?1:0)&&race.Progress.PenaltySeconds==(miss?5:0),profile.Id+" real FixedUpdate "+(miss?"road miss":"backward-body crossing"));
                    car.Body.isKinematic=true;
                }
                foreach(var boundary in FindObjectsByType<CircuitBoundary>())
                {
                    var p=boundary.transform.TransformPoint(new Vector3(0,35,20));Place(p,boundary.transform.forward,boundary.transform.forward*100);
                    yield return new WaitForSeconds(.2f);Check(!boundary.Outside(car.Body.position),profile.Id+" flying boundary "+boundary.name);car.Body.isKinematic=true;
                }
            }
            // Isolated local synthetic files, verified by actual decoded source samples.
            var radio=flow.Radio;radio.SetFolder(Path.GetFullPath("Temp/radio-fixtures"));radio.Rescan();
            yield return new WaitForSecondsRealtime(1);
            foreach(string ext in new[]{"mp3","wav","ogg","wav"})
            {
                typeof(LocalRadio).GetMethod("Play",BindingFlags.NonPublic|BindingFlags.Instance).Invoke(radio,new object[]{Path.GetFullPath("Temp/radio-fixtures/generated."+ext),false});
                float until=Time.realtimeSinceStartup+10;while(radio.Loading&&Time.realtimeSinceStartup<until)yield return null;
                yield return new WaitForSecondsRealtime(1.1f);
                var source=(AudioSource)typeof(LocalRadio).GetField("source",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(radio);
                // First GetOutputData activates the capture buffer; sample again after DSP runs.
                var buffer=new float[2048];source.GetOutputData(buffer,0);yield return new WaitForSecondsRealtime(.15f);source.GetOutputData(buffer,0);float peak=buffer.Max(v=>Mathf.Abs(v));
                Check(radio.Playing&&peak>.00001f,ext+" standalone actual decoded playback peak="+peak+" playing="+radio.Playing+" loading="+radio.Loading+" status="+radio.Status+" clip="+source.clip?.name+" state="+source.clip?.loadState+" length="+source.clip?.length+" volume="+source.volume+" master="+AudioListener.volume+" time="+source.time+" virtual="+source.isVirtual);
                Check(ext=="wav"?radio.Song=="generated":radio.Song.Contains("Racer Test Artist"),ext+" metadata / filename fallback: "+radio.Song);
            }
            radio.Toggle();yield return null;Check(!radio.Playing,"Radio off pauses source");radio.Toggle();yield return null;Check(radio.Playing,"Radio resumes");
            flow.Pause();yield return null;Check(radio.Playing,"Radio continues consistently through pause");flow.Resume();
            var currentField=typeof(LocalRadio).GetField("current",BindingFlags.NonPublic|BindingFlags.Instance);
            string before=(string)currentField.GetValue(radio);
            InputSystem.QueueStateEvent(pad,new GamepadState().WithButton(GamepadButton.DpadRight));yield return null;
            InputSystem.QueueStateEvent(pad,new GamepadState());yield return new WaitForSecondsRealtime(2);
            Check((string)currentField.GetValue(radio)!=before,"Virtual D-pad right selects another song: before="+Path.GetFileName(before)+" after="+Path.GetFileName((string)currentField.GetValue(radio))+" status="+radio.Status);
            InputSystem.QueueStateEvent(pad,new GamepadState().WithButton(GamepadButton.DpadLeft));yield return null;
            InputSystem.QueueStateEvent(pad,new GamepadState());yield return new WaitForSecondsRealtime(.7f);
            Check((string)currentField.GetValue(radio)==before,"Virtual D-pad left returns through history");
            InputSystem.QueueStateEvent(pad,new GamepadState().WithButton(GamepadButton.DpadDown));yield return null;
            InputSystem.QueueStateEvent(pad,new GamepadState());yield return null;Check(!radio.Playing,"Virtual D-pad down toggles off");
            flow.Pause();InputSystem.QueueStateEvent(pad,new GamepadState().WithButton(GamepadButton.DpadDown));yield return null;
            InputSystem.QueueStateEvent(pad,new GamepadState());yield return null;Check(!radio.Playing,"Menu D-pad does not toggle radio");flow.Resume();radio.Toggle();
            InputSystem.QueueStateEvent(pad,new GamepadState().WithButton(GamepadButton.DpadUp));yield return null;
            InputSystem.QueueStateEvent(pad,new GamepadState());yield return null;Check(radio.Toast!=null,"Virtual D-pad up shows current song");
            // Normal frame input with radio under engine/tire playback, capture actual listener DSP.
            car.GetComponent<VehicleConfiguration>().Apply("original");var roadPoint=race.road.At(4000,out var roadF)+Vector3.up*.7f;
            Place(roadPoint,roadF,roadF*27+Vector3.Cross(Vector3.up,roadF)*6);car.enabled=true;
            var capture=FindAnyObjectByType<AudioListener>().gameObject.AddComponent<CorrectionAudioCapture>();capture.Begin(5);
            InputSystem.QueueStateEvent(pad,new GamepadState{rightTrigger=.4f,leftTrigger=.8f,leftStick=new(.7f,0)});
            float tirePeak=0,listenUntil=Time.time+3;var tireSamples=new float[1024];
            var voices=(AudioSource[])typeof(VehicleAudio).GetField("voices",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(car.GetComponent<VehicleAudio>());
            while(Time.time<listenUntil){voices[4].GetOutputData(tireSamples,0);foreach(float sample in tireSamples)tirePeak=Mathf.Max(tirePeak,Mathf.Abs(sample));yield return null;}
            Check(tirePeak>.001f,"Actual tire voice output while engine and radio run: peak="+tirePeak);
            var mix=capture.Finish(Dir+"/engine-tires-radio.wav");File.WriteAllText(Dir+"/mix.txt",mix);
            Check(!mix.Contains("samples=0,"),"Listener DSP capture contains samples: "+mix);
            InputSystem.QueueStateEvent(pad,new GamepadState());Place(roadPoint,roadF,roadF*20);yield return new WaitForSeconds(1.1f);
            voices[4].GetOutputData(tireSamples,0);Check(tireSamples.Max(v=>Mathf.Abs(v))<.0001f,"Straight normal driving does not continuously squeal");
            Place(roadPoint+Vector3.up*20,roadF,roadF*20);yield return new WaitForSeconds(.4f);
            voices[4].GetOutputData(tireSamples,0);Check(tireSamples.Max(v=>Mathf.Abs(v))<.0001f,"Airborne tire voice is silent");Place(roadPoint,roadF,roadF*24);
            float maxFrame=0,sumFrame=0;int frames=0,slowFrames=0;
            for(int skip=0;skip<4;skip++)
            {
                radio.Next();radio.Rescan();float end=Time.realtimeSinceStartup+1.5f;
                while(Time.realtimeSinceStartup<end){float dt=Time.unscaledDeltaTime;maxFrame=Mathf.Max(maxFrame,dt);sumFrame+=dt;frames++;if(dt>.05f)slowFrames++;yield return null;}
            }
            File.WriteAllText(Dir+"/skip-rescan-frames.txt",$"Driving with four skips/rescans: frames={frames}; mean={sumFrame/Mathf.Max(1,frames)*1000:F2}ms; max={maxFrame*1000:F2}ms; frames>50ms={slowFrames}. Device/perceptual acceptance remains separate.");
            InputSystem.QueueStateEvent(pad,new GamepadState());car.enabled=false;car.Body.isKinematic=true;
            flow.Pause();flow.OpenSettings();
            var menus=flow.GetComponent<RaceMenus>();typeof(RaceMenus).GetField("musicPage",BindingFlags.NonPublic|BindingFlags.Instance).SetValue(menus,true);menus.Show();
            yield return new WaitForEndOfFrame();Capture(Dir+"/music-settings.png");yield return null;
            Check(flow.Save.Settings.musicFolder==radio.Folder,"Folder persists in settings");
            flow.CloseSettings();var ledger=race.Progress;ledger.Restart();ledger.Cross(0,true,race.Clock);
            for(int i=1;i<=6;i++)ledger.Miss(i,5,"road gate passed outside span","Fox Gully",race.Clock);
            typeof(RaceMenus).GetField("penaltyPage",BindingFlags.NonPublic|BindingFlags.Instance).SetValue(menus,0);menus.Show();
            yield return new WaitForEndOfFrame();Capture(Dir+"/pause-penalties.png");
            Check(ledger.Ledger.Count==ledger.MissedGates&&ledger.Ledger.Sum(e=>e.Seconds)==ledger.PenaltySeconds,"Pause ledger count and seconds reconcile");
            // Error handling uses generated disposable files, never a personal library.
            var play=typeof(LocalRadio).GetMethod("Play",BindingFlags.NonPublic|BindingFlags.Instance);
            play.Invoke(radio,new object[]{Path.GetFullPath("Temp/radio-fixtures/corrupt.mp3"),false});
            yield return new WaitForSecondsRealtime(.35f);
            var failed=(HashSet<string>)typeof(LocalRadio).GetField("failed",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(radio);
            Check(failed.Contains(Path.GetFullPath("Temp/radio-fixtures/corrupt.mp3")),"Corrupt file skipped without crashing");
            string empty=Path.GetFullPath("Temp/radio-empty-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(empty);
            radio.SetFolder(empty);yield return new WaitForSecondsRealtime(.4f);Check(radio.Count==0&&!radio.Playing,"Empty folder remains usable");
            radio.SetFolder(empty+"/missing");yield return new WaitForSecondsRealtime(.4f);Check(radio.Count==0&&!radio.Playing,"Missing folder remains usable");
            string disposable=empty+"/removed.mp3";File.Copy("Temp/radio-fixtures/generated.mp3",disposable);radio.SetFolder(empty);
            yield return new WaitForSecondsRealtime(1);File.Delete(disposable);radio.Rescan();yield return new WaitForSecondsRealtime(.5f);
            Check(radio.Count==0&&!radio.Playing,"File removed during playback reconciles on rescan");
            radio.SetFolder(Path.GetFullPath("Temp/radio-fixtures"));yield return new WaitForSecondsRealtime(1);
            File.WriteAllText(Dir+"/done.txt",$"{checks.Count(x=>x.StartsWith("PASS"))}/{checks.Count}; scripted fixtures, physical FixedUpdate and DSP evidence. No physical-controller or subjective listening acceptance.");
            InputSystem.RemoveDevice(pad);pad=null;Application.Quit();
        }
        void Place(Vector3 p,Vector3 f,Vector3 v)
        {var c=race.vehicle;c.Body.isKinematic=false;c.Body.position=p;c.Body.rotation=Quaternion.LookRotation(Vector3.ProjectOnPlane(f,Vector3.up));c.transform.SetPositionAndRotation(p,c.Body.rotation);c.Body.linearVelocity=v;c.Body.angularVelocity=Vector3.zero;c.ClearSteering();Physics.SyncTransforms();race.ResetSampling(p,Time.timeAsDouble);}
        public static void Capture(string path)
        {
            var camera=Camera.main;var canvases=FindObjectsByType<Canvas>();
            var modes=canvases.Select(c=>c.renderMode).ToArray();var cameras=canvases.Select(c=>c.worldCamera).ToArray();var distances=canvases.Select(c=>c.planeDistance).ToArray();
            var rt=new RenderTexture(1280,720,24);var prior=camera.targetTexture;var active=RenderTexture.active;var texture=new Texture2D(1280,720,TextureFormat.RGB24,false);
            try
            {foreach(var c in canvases){c.renderMode=RenderMode.ScreenSpaceCamera;c.worldCamera=camera;c.planeDistance=1;}Canvas.ForceUpdateCanvases();camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;texture.ReadPixels(new Rect(0,0,1280,720),0,0);texture.Apply();File.WriteAllBytes(path,texture.EncodeToPNG());}
            finally{for(int i=0;i<canvases.Length;i++){canvases[i].renderMode=modes[i];canvases[i].worldCamera=cameras[i];canvases[i].planeDistance=distances[i];}camera.targetTexture=prior;RenderTexture.active=active;Destroy(rt);Destroy(texture);}
        }
        void OnDestroy(){if(pad!=null&&pad.added)InputSystem.RemoveDevice(pad);}
    }
}
