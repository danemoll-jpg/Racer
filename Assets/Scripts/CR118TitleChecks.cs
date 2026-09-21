using System;
using System.IO;
using System.Linq;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
namespace Racer
{
    public sealed partial class CR112Validation
    {
        CorrectionAudioCapture titleCapture;
        IEnumerator TitleOutput()
        {
            yield return null;Bind();var title=FindAnyObjectByType<StartupTitle>();float deadline=Time.realtimeSinceStartup+12;
            while(title&&title.VoiceStarts==0&&Time.realtimeSinceStartup<deadline)yield return null;
            Check(title&&title.VoiceStarts==1,"Ordinary startup schedules supplied voice once");if(!title){AudioListener.volume=0;yield break;}
            Check(title.VoiceScheduledDsp-title.ArtworkReadyDsp>=1.5,"Artwork/clip-ready lead-in at least 1.5 seconds before speech");
            Check(StartupTitle.CancelUnstarted(1,double.PositiveInfinity)&&StartupTitle.CancelUnstarted(1,1.1)&&!StartupTitle.CancelUnstarted(1.1,1.1)&&!StartupTitle.CancelUnstarted(1.2,1.1),"Tiny timing fixture: dismissal cancels loading/scheduled speech, preserves started speech");
            File.WriteAllText(dir+"/startup-timing.txt",$"readyDSP={title.ArtworkReadyDsp} scheduledVoiceDSP={title.VoiceScheduledDsp} leadIn={title.VoiceScheduledDsp-title.ArtworkReadyDsp}");
            var clip=title.Voice.clip;var pcm=new float[clip.samples*clip.channels];clip.GetData(pcm,0);using(var w=new BinaryWriter(File.Create(dir+"/packaged-voice-float32.raw")))foreach(float sample in pcm)w.Write(sample);
            File.WriteAllText(dir+"/packaged-voice.txt",$"samples={clip.samples} channels={clip.channels} frequency={clip.frequency} load={clip.loadState}");
            AudioListener.volume=.5f;bool early=Arg("-advance")=="yes";
            var keyboard=InputSystem.AddDevice<Keyboard>();InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
            if(early){yield return new WaitForSecondsRealtime(.35f);InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.Space));yield return null;yield return null;InputSystem.QueueStateEvent(keyboard,new KeyboardState());}
            bool surviving=false;while(StartupTitle.SpeechPending&&Time.realtimeSinceStartup<deadline){if(early&&flow.State==RaceFlow.Stage.Ready&&title&&title.VoiceStarts==1)surviving=true;yield return null;}
            if(early){Check(surviving&&flow.State==RaceFlow.Stage.Ready,"Early advance enters menu while original speech survives without restart");yield return new WaitForSecondsRealtime(.4f);}
            else {yield return new WaitForSecondsRealtime(1.3f);Check(title&&title.Theme.isPlaying&&title.Theme.loop&&title.VoiceStarts==1&&flow.State==RaceFlow.Stage.Title,"Untouched startup reaches looping title theme after the original opening and full voice");}
            File.WriteAllText(dir+"/output-capture.txt",titleCapture.Finish(dir+"/final-player-output.wav"));AudioListener.volume=0;
            if(!early){InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.Space));yield return null;yield return null;InputSystem.QueueStateEvent(keyboard,new KeyboardState());yield return null;yield return null;}
            float menuDeadline=Time.realtimeSinceStartup+2;while(flow.State!=RaceFlow.Stage.Ready&&Time.realtimeSinceStartup<menuDeadline)yield return null;
            Check(flow.State==RaceFlow.Stage.Ready,"Portable runtime reaches menu after title");ThreeFeatureValidation.CaptureUi(dir+"/portable-menu.png");InputSystem.RemoveDevice(keyboard);AudioListener.volume=0;
            File.WriteAllText(dir+"/done.txt","Actual listener-DSP output captured through the phrase and transition. Audio-input tool cannot audition sound; no subjective or OS-endpoint listening claim. Isolated test muted again without saving volume changes.");
        }
    }
}
