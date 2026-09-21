using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UI;

namespace Racer
{
    // Process lifetime, deliberately independent of station selection and scene lifetime.
    public sealed class StartupTitle : MonoBehaviour
    {
        static bool completed;
        static StartupTitle instance;
        public static bool Active => instance && !instance.Advancing;
        public static bool SpeechPending => instance && !instance.speechComplete;
        bool speechComplete;
        double voiceScheduledAt=double.PositiveInfinity;
        bool cancelledBeforeSpeech;
        public const float ArtworkLeadIn=1.5f;
        public double ArtworkReadyDsp { get; private set; }
        public double VoiceScheduledDsp=>voiceScheduledAt;
        public static bool CancelUnstarted(double now,double scheduled)=>now<scheduled;
        public bool Armed { get; private set; }
        public bool Advancing { get; private set; }
        public int VoiceStarts { get; private set; }
        public AudioSource Voice { get; private set; }
        public AudioSource Theme { get; private set; }
        public AudioSource ThemeOpening { get; private set; }
        RaceFlow owner;
        GameObject canvas;
        BaseInputModule inputModule;
        bool moduleWasEnabled;
        int openedFrame;
        public static float ValidationLoadDelay;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetProcess() { completed=false; instance=null; ValidationLoadDelay=0; }
        public static bool Begin(RaceFlow flow)
        {
            var args=System.Environment.GetCommandLineArgs();
            if(completed || (args.Contains("-racerTestSave") && args.Contains("-racerSkipTitle")))return false;
            if(instance){instance.owner=flow;return !instance.Advancing;}
            instance=new GameObject("Woodstock Rush startup title").AddComponent<StartupTitle>();
            DontDestroyOnLoad(instance.gameObject);instance.owner=flow;instance.Open();return true;
        }
        void Open()
        {
            openedFrame=Time.frameCount;
            inputModule=EventSystem.current?EventSystem.current.GetComponent<BaseInputModule>():null;
            if(inputModule){moduleWasEnabled=inputModule.enabled;inputModule.enabled=false;}
            EventSystem.current?.SetSelectedGameObject(null);
            canvas=new GameObject("Approved title artwork",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler));
            canvas.transform.SetParent(transform,false);
            var c=canvas.GetComponent<Canvas>();c.renderMode=RenderMode.ScreenSpaceOverlay;c.sortingOrder=32000;
            var scaler=canvas.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new(1280,720);scaler.matchWidthOrHeight=.5f;
            var matte=Rect("Matte",canvas.transform);Stretch(matte);matte.gameObject.AddComponent<Image>().color=new(.012f,.019f,.025f);
            var area=Rect("Artwork area",matte);Stretch(area);area.offsetMin=new(0,42);
            var artwork=Resources.Load<Texture2D>("Title/Artwork");
            var frame=Rect("Entire approved artwork",area);Stretch(frame);
            var raw=frame.gameObject.AddComponent<RawImage>();raw.texture=artwork;raw.raycastTarget=false;
            var fit=frame.gameObject.AddComponent<AspectRatioFitter>();fit.aspectMode=AspectRatioFitter.AspectMode.FitInParent;fit.aspectRatio=artwork?(float)artwork.width/artwork.height:16f/9;
            var prompt=Rect("Fresh button prompt",matte);prompt.anchorMin=new(0,0);prompt.anchorMax=new(1,0);prompt.pivot=new(.5f,0);prompt.sizeDelta=new(0,42);
            var text=prompt.gameObject.AddComponent<Text>();text.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");text.fontSize=18;text.alignment=TextAnchor.MiddleCenter;text.color=new(.86f,.86f,.8f);text.text="Press any button";text.raycastTarget=false;
            Voice=gameObject.AddComponent<AudioSource>();Theme=gameObject.AddComponent<AudioSource>();ThemeOpening=gameObject.AddComponent<AudioSource>();
            foreach(var source in new[]{Voice,Theme,ThemeOpening}){source.playOnAwake=false;source.spatialBlend=0;source.ignoreListenerPause=true;source.priority=32;}
            Voice.volume=.85f;Theme.volume=ThemeOpening.volume=.27f;Theme.loop=true;
            StartCoroutine(LoadAudio());
        }
        static RectTransform Rect(string name,Transform parent){var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(parent,false);return r;}
        static void Stretch(RectTransform r){r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;}
        IEnumerator LoadAudio()
        {
            if(ValidationLoadDelay>0 && System.Environment.GetCommandLineArgs().Contains("-racerTestSave"))yield return new WaitForSecondsRealtime(ValidationLoadDelay);
            var voice=Resources.LoadAsync<AudioClip>("Title/Voice");var theme=Resources.LoadAsync<AudioClip>("Title/ThemeLoop");var opening=Resources.LoadAsync<AudioClip>("Title/ThemeOpening");
            yield return voice;yield return theme;yield return opening;
            Voice.clip=voice.asset as AudioClip;Theme.clip=theme.asset as AudioClip;ThemeOpening.clip=opening.asset as AudioClip;
            foreach(var source in new[]{Voice,Theme,ThemeOpening})if(source.clip)source.clip.LoadAudioData();
            while(new[]{Voice,Theme,ThemeOpening}.Any(s=>s.clip&&s.clip.loadState==AudioDataLoadState.Loading))yield return null;
            // Let artwork render and the audio device settle after asset/scene loading.
            // Human playback still clipped despite prior listener-tail captures.
            Canvas.ForceUpdateCanvases();yield return null;yield return null;
            double readyDsp=AudioSettings.dspTime;ArtworkReadyDsp=readyDsp;float ready=Time.realtimeSinceStartup;
            while(!Advancing&&(Time.realtimeSinceStartup-ready<ArtworkLeadIn||AudioSettings.dspTime-readyDsp<ArtworkLeadIn))yield return null;
            if(Advancing){speechComplete=true;yield break;}
            // Use the complete decoded duration; never fade or truncate the voice.
            if(Voice.clip){
                double start=AudioSettings.dspTime+.1;voiceScheduledAt=start;Voice.timeSamples=0;
                double end=start+(double)Voice.clip.samples/Voice.clip.frequency+.2;
                Voice.PlayScheduled(start);VoiceStarts++;
                while(!cancelledBeforeSpeech&&AudioSettings.dspTime<end)yield return null;
            }
            speechComplete=true;
            if(Advancing)yield break;
            // Full authored attack from sample zero at the established title level.
            if(Theme.clip){
                double start=AudioSettings.dspTime+.1;
                // The authored loop begins 240 ms into the original tune. Restore that
                // supplied opening once, then retain the existing circular loop intact.
                if(ThemeOpening.clip){ThemeOpening.timeSamples=0;ThemeOpening.PlayScheduled(start);start+=(double)ThemeOpening.clip.samples/ThemeOpening.clip.frequency;}
                Theme.timeSamples=0;Theme.PlayScheduled(start);
            }
        }
        static System.Collections.Generic.IEnumerable<ButtonControl> Buttons()=>InputSystem.devices.Where(d=>d is Keyboard || d is Gamepad || d is Mouse).SelectMany(d=>d.allControls.OfType<ButtonControl>()).Where(b=>!b.synthetic&&!(b.parent is StickControl));
        public static bool ButtonHeld()=>Buttons().Any(b=>b.isPressed);
        static bool ButtonPressed()=>Buttons().Any(b=>b.wasPressedThisFrame);
        void Update()
        {
            if(Advancing)return;
            if(!Armed){if(Time.frameCount>openedFrame+2&&!ButtonHeld())Armed=true;return;}
            if(ButtonPressed()){
                Advancing=true;canvas.SetActive(false);Theme.Stop();ThemeOpening.Stop();
                if(CancelUnstarted(AudioSettings.dspTime,voiceScheduledAt)){cancelledBeforeSpeech=true;Voice.Stop();speechComplete=true;}
                StartCoroutine(EnterMenu());
            }
        }
        IEnumerator EnterMenu()
        {
            // Wait through release and another input update: the dismissal cannot submit,
            // navigate, pause, or click the newly visible menu, even when held.
            do { yield return null; } while(ButtonHeld());
            yield return null;yield return null;
            completed=true;
            if(inputModule)inputModule.enabled=moduleWasEnabled;
            if(owner)owner.EnterMenuAfterTitle();
            // The process-lifetime source survives early input and scene changes. Radio
            // remains ducked until the scheduled full sample tail has completed.
            while(!speechComplete)yield return null;
            instance=null;Destroy(gameObject);
        }
        void OnDestroy(){StopAllCoroutines();if(Voice)Voice.Stop();if(Theme)Theme.Stop();if(ThemeOpening)ThemeOpening.Stop();if(instance==this)instance=null;}
    }
}
