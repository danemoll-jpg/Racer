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
        public bool Armed { get; private set; }
        public bool Advancing { get; private set; }
        public int VoiceStarts { get; private set; }
        public AudioSource Voice { get; private set; }
        public AudioSource Theme { get; private set; }
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
            Voice=gameObject.AddComponent<AudioSource>();Theme=gameObject.AddComponent<AudioSource>();
            foreach(var source in new[]{Voice,Theme}){source.playOnAwake=false;source.spatialBlend=0;source.ignoreListenerPause=true;source.priority=32;}
            Voice.volume=.85f;Theme.volume=.09f;Theme.loop=true;
            StartCoroutine(LoadAudio());
        }
        static RectTransform Rect(string name,Transform parent){var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(parent,false);return r;}
        static void Stretch(RectTransform r){r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;}
        IEnumerator LoadAudio()
        {
            if(ValidationLoadDelay>0 && System.Environment.GetCommandLineArgs().Contains("-racerTestSave"))yield return new WaitForSecondsRealtime(ValidationLoadDelay);
            var voice=Resources.LoadAsync<AudioClip>("Title/Voice");var theme=Resources.LoadAsync<AudioClip>("Title/ThemeLoop");
            yield return voice;yield return theme;
            Voice.clip=voice.asset as AudioClip;Theme.clip=theme.asset as AudioClip;
            if(Voice.clip)Voice.clip.LoadAudioData();if(Theme.clip)Theme.clip.LoadAudioData();
            while((Voice.clip&&Voice.clip.loadState==AudioDataLoadState.Loading)||(Theme.clip&&Theme.clip.loadState==AudioDataLoadState.Loading))yield return null;
            // Give the supplied speech its complete audible tail before music enters.
            // The quiet final consonant was masked by the concurrent theme. Use the
            // decoded sample duration on the DSP clock, not a frame timeout or isPlaying.
            if(Voice.clip){
                double start=AudioSettings.dspTime+.05;
                double end=start+(double)Voice.clip.samples/Voice.clip.frequency+.2;
                Voice.PlayScheduled(start);VoiceStarts++;
                while(AudioSettings.dspTime<end)yield return null;
            }
            speechComplete=true;
            if(Advancing)yield break;
            if(Theme.clip)Theme.Play();
            while(!Advancing){Theme.volume=Mathf.MoveTowards(Theme.volume,.27f,Time.unscaledDeltaTime*.2f);yield return null;}
        }
        static System.Collections.Generic.IEnumerable<ButtonControl> Buttons()=>InputSystem.devices.Where(d=>d is Keyboard || d is Gamepad || d is Mouse).SelectMany(d=>d.allControls.OfType<ButtonControl>()).Where(b=>!b.synthetic&&!(b.parent is StickControl));
        public static bool ButtonHeld()=>Buttons().Any(b=>b.isPressed);
        static bool ButtonPressed()=>Buttons().Any(b=>b.wasPressedThisFrame);
        void Update()
        {
            if(Advancing)return;
            if(!Armed){if(Time.frameCount>openedFrame+2&&!ButtonHeld())Armed=true;return;}
            if(ButtonPressed()){Advancing=true;canvas.SetActive(false);Theme.Stop();StartCoroutine(EnterMenu());}
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
        void OnDestroy(){StopAllCoroutines();if(Voice)Voice.Stop();if(Theme)Theme.Stop();if(instance==this)instance=null;}
    }
}
