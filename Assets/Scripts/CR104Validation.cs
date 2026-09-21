using System;
using System.IO;
using System.Linq;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
namespace Racer
{
    public sealed class CR104Validation:MonoBehaviour
    {
        static string Arg(string k){var a=Environment.GetCommandLineArgs();int i=Array.IndexOf(a,k);return i>=0&&i+1<a.Length?a[i+1]:"";}
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]static void Boot(){if(Arg("-voiceCheck")==""||Arg("-racerTestSave")=="")return;StartupTitle.ValidationLoadDelay=1;InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;var g=new GameObject("CR104 isolated output check");DontDestroyOnLoad(g);g.AddComponent<CR104Validation>();}
        IEnumerator Start(){Application.runInBackground=true;string dir=Arg("-evidence");Directory.CreateDirectory(dir);yield return null;yield return null;var title=FindAnyObjectByType<StartupTitle>();var flow=FindAnyObjectByType<RaceFlow>();var capture=FindAnyObjectByType<AudioListener>().gameObject.AddComponent<CorrectionAudioCapture>();AudioListener.volume=.65f;capture.Begin(6);var keys=InputSystem.AddDevice<Keyboard>();bool skip=Arg("-voiceCheck")=="skip";float start=Time.realtimeSinceStartup;int maxSample=0;using(var log=new StreamWriter(dir+"/playback.csv")){log.WriteLine("elapsed,voiceSample,voicePlaying,themeVolume,radioCount,titleActive");while(Time.realtimeSinceStartup-start<(skip?1.25f:4.5f)){if(title&&title.Voice){maxSample=Math.Max(maxSample,title.Voice.timeSamples);log.WriteLine($"{Time.realtimeSinceStartup-start},{title.Voice.timeSamples},{title.Voice.isPlaying},{title.Theme.volume},{FindObjectsByType<LocalRadio>().Length},{StartupTitle.Active}");}yield return null;}}
            var samples=new float[title.Voice.clip.samples*title.Voice.clip.channels];title.Voice.clip.GetData(samples,0);var bytes=new byte[samples.Length*4];Buffer.BlockCopy(samples,0,bytes,0,bytes.Length);File.WriteAllBytes(dir+"/imported-voice.f32",bytes);
            File.WriteAllText(dir+"/mix.txt",capture.Finish(dir+"/startup-mix.wav")+"\nmaxObservedVoiceSample="+maxSample+" clipSamples="+title.Voice.clip.samples+"\nNo-input stayed on title="+(flow.State==RaceFlow.Stage.Title)+" radio absent="+(!FindAnyObjectByType<LocalRadio>()));AudioListener.volume=0;
            InputSystem.QueueStateEvent(keys,new KeyboardState(Key.Space));yield return null;yield return null;InputSystem.QueueStateEvent(keys,new KeyboardState());for(int i=0;i<8;i++)yield return null;File.WriteAllText(dir+"/transition.txt","Menu="+(flow.State==RaceFlow.Stage.Ready)+" titleGone="+(!StartupTitle.Active)+" radioAtMenu="+(flow.Radio!=null));InputSystem.RemoveDevice(keys);ThreeFeatureValidation.CaptureUi(dir+"/menu.png");File.WriteAllText(dir+"/done.txt","DSP capture and virtual keyboard check; no OS endpoint listening claimed.");Application.Quit();}
    }
}

