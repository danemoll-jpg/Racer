using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.EventSystems;
namespace Racer
{
    public sealed class TitleValidation:MonoBehaviour
    {
        static string Arg(string key,string fallback=""){var a=Environment.GetCommandLineArgs();int i=Array.IndexOf(a,key);return i>=0&&i+1<a.Length?a[i+1]:fallback;}
        static Keyboard keyboard;static Gamepad pad;static Mouse mouse;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]static void Boot()
        {
            if(Arg("-titleCheck")==""||Arg("-racerTestSave")=="")return;
            InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
            keyboard=InputSystem.AddDevice<Keyboard>();pad=InputSystem.AddDevice<Gamepad>();mouse=InputSystem.AddDevice<Mouse>();
            if(Arg("-held")=="yes"){Press(true);InputSystem.Update();}
            StartupTitle.ValidationLoadDelay=float.TryParse(Arg("-titleDelay"),out var delay)?delay:0;
            var g=new GameObject("Explicit title validation");DontDestroyOnLoad(g);g.AddComponent<TitleValidation>();
        }
        static void Press(bool on)
        {
            switch(Arg("-titleInput","keyboard")){
                case "gamepad":InputSystem.QueueStateEvent(pad,on?new GamepadState().WithButton(GamepadButton.South):new GamepadState());break;
                case "mouse":InputSystem.QueueStateEvent(mouse,on?new MouseState{position=new(640,360)}.WithButton(MouseButton.Left):new MouseState{position=new(640,360)});break;
                default:InputSystem.QueueStateEvent(keyboard,on?new KeyboardState(Key.Space):new KeyboardState());break;
            }
        }
        string dir;readonly List<string> checks=new();
        void Check(bool ok,string name){checks.Add((ok?"PASS ":"FAIL ")+name);File.WriteAllLines(dir+"/checks.txt",checks);}
        IEnumerator Start()
        {
            Application.runInBackground=true;dir=Arg("-evidence");Directory.CreateDirectory(dir);yield return null;yield return null;
            var flow=FindAnyObjectByType<RaceFlow>();var title=FindAnyObjectByType<StartupTitle>();
            Check(title&&flow.State==RaceFlow.Stage.Title,"Cold startup owns screen before menu");
            if(!title){Application.Quit(2);yield break;}
            float master=flow.Save.Settings.master,music=flow.Save.Settings.music;bool radio=flow.Save.Settings.radioOn;string station=flow.Save.Settings.radioChannel;
            Check(Mathf.Abs(AudioListener.volume-master)<.001f,"Saved Master volume applied before title audio");
            Check(!FindAnyObjectByType<LocalRadio>(),"No radio object or playback before menu");
            Check(FindObjectsByType<AudioListener>().Count(l=>l.enabled)==1,"Exactly one enabled listener");
            if(Arg("-held")=="yes"){yield return new WaitForSecondsRealtime(1);Check(!title.Armed&&!title.Advancing,"Input held since launch is ignored");Press(false);yield return null;yield return null;}
            while(!title.Armed)yield return null;
            yield return new WaitForSecondsRealtime(.1f);ThreeFeatureValidation.CaptureUi(dir+"/title.png",Screen.width,Screen.height);
            var image=FindObjectsByType<UnityEngine.UI.RawImage>().Single(i=>i.name=="Entire approved artwork");var corners=new Vector3[4];image.rectTransform.GetWorldCorners(corners);
            Check(corners.All(p=>p.x>=-.5f&&p.x<=Screen.width+.5f&&p.y>=-.5f&&p.y<=Screen.height+.5f),"Entire artwork contained at "+Screen.width+"x"+Screen.height);
            var r=image.rectTransform.rect;Check(Mathf.Abs(r.width/r.height-(float)image.texture.width/image.texture.height)<.001f,"Artwork exact aspect ratio");
            if(Arg("-titleCheck")=="loops"){
                float deadline=Time.realtimeSinceStartup+15;while(!title.Theme.isPlaying&&Time.realtimeSinceStartup<deadline)yield return null;
                Check(title.Theme.isPlaying&&title.VoiceStarts==1,"Supplied voice once and theme playing");
                float previous=title.Theme.time;int wraps=0;deadline=Time.realtimeSinceStartup+110;
                using(var trace=new StreamWriter(dir+"/audio-loop.csv")){trace.WriteLine("elapsed,themeTime,voicePlaying,themeVolume,master,radioObjects");while(wraps<3&&Time.realtimeSinceStartup<deadline){float time=title.Theme.time;if(time<previous-.5f)wraps++;previous=time;trace.WriteLine($"{Time.realtimeSinceStartup:F3},{time:F3},{title.Voice.isPlaying},{title.Theme.volume:F3},{AudioListener.volume},{FindObjectsByType<LocalRadio>().Length}");yield return new WaitForSecondsRealtime(.1f);}}
                Check(wraps>=3&&title.VoiceStarts==1&&!title.Voice.isPlaying,"Three complete theme loops; speech does not repeat");
                Check(flow.State==RaceFlow.Stage.Title&&!FindAnyObjectByType<LocalRadio>(),"No timed advance or radio through three loops");
            }
            if(Arg("-racerAudioCheck")=="yes"){
                var capture=FindAnyObjectByType<AudioListener>().gameObject.AddComponent<CorrectionAudioCapture>();
                AudioListener.volume=.65f;capture.Begin(2);yield return new WaitForSecondsRealtime(2.2f);File.WriteAllText(dir+"/audible-mix.txt",capture.Finish(dir+"/targeted-title-mix.wav"));AudioListener.volume=0;
            }
            var voice=title.Voice;var theme=title.Theme;Press(true);yield return null;yield return null;
            Check(title.Advancing&&!voice.isPlaying&&!theme.isPlaying,"Dismissal promptly stops both title sources");
            yield return new WaitForSecondsRealtime(.3f);Check(flow.State==RaceFlow.Stage.Title,"Held dismissal cannot submit menu");
            Press(false);for(int i=0;i<6;i++)yield return null;
            Check(!StartupTitle.Active&&flow.State==RaceFlow.Stage.Ready,"One transition to main menu; no accidental activation");
            Check(flow.Radio&&EventSystem.current.currentSelectedGameObject,"Radio attaches only at menu; controller focus restored");
            yield return new WaitForSecondsRealtime(Mathf.Max(2,StartupTitle.ValidationLoadDelay+1));
            Check(!FindAnyObjectByType<StartupTitle>()&&!voice&&!theme,"No stale delayed title source after skip");
            Check(flow.Save.Settings.master==master&&flow.Save.Settings.music==music&&flow.Save.Settings.radioOn==radio,"Saved volume and radio On/Off preserved");
            if(!string.IsNullOrEmpty(station))Check(flow.Save.Settings.radioChannel==station,"Saved station selection preserved");
            if(!radio)Check(!flow.Radio.Playing,"Saved Radio Off stays silent at menu");
            if(radio){float deadline=Time.realtimeSinceStartup+15;while(!flow.Radio.Playing&&Time.realtimeSinceStartup<deadline)yield return null;Check(flow.Radio.Playing,"Saved Radio On actually starts playback after menu entry");}
            ThreeFeatureValidation.CaptureUi(dir+"/menu.png",Screen.width,Screen.height);
            flow.StartFreeRoam();yield return null;flow.Pause();flow.QuitRace();yield return null;Check(!StartupTitle.Active&&flow.State==RaceFlow.Stage.Ready,"Pause and Quit Race do not replay title");
            flow.StartRace();yield return new WaitForSecondsRealtime(4);flow.Pause();flow.QuitRace();yield return null;Check(!StartupTitle.Active&&flow.State==RaceFlow.Stage.Ready,"Countdown/race return does not replay title");
            flow.OpenCourses();flow.SelectCourse(true);yield return null;yield return null;yield return null;flow=FindAnyObjectByType<RaceFlow>();
            Check(!StartupTitle.Active&&flow.State==RaceFlow.Stage.Ready,"Track change does not replay title");Check(FindObjectsByType<LocalRadio>().Length==1,"One radio after scene transition");
            InputSystem.RemoveDevice(keyboard);InputSystem.RemoveDevice(pad);InputSystem.RemoveDevice(mouse);
            AudioListener.volume=0;File.WriteAllText(dir+"/done.txt","Completed automated virtual-input and runtime checks. No physical Deck/controller or subjective listening claim.");Application.Quit();
        }
    }
}

