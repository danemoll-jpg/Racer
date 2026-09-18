using UnityEngine;

namespace Racer
{
    /// <summary>Original synthesized Foley, four bounded spatial voices shared by every prop.</summary>
    public sealed class SmashAudio : MonoBehaviour
    {
        public enum Surface { Wood, ChainLink, Mailbox, Sign }
        static SmashAudio instance;
        static readonly float[] WoodDelays={.055f,.12f,.20f,.29f}, MetalDelays={.08f,.17f,.29f,.43f,.61f};
        readonly AudioSource[] voices=new AudioSource[4];
        readonly AudioClip[,] clips=new AudioClip[4,3];
        readonly float[] gains=new float[4];
        float next;
        RaceFlow flow;
        public int Events { get; private set; }
        public static void Prepare()
        {
            if(!instance) instance=new GameObject("Bounded smash audio").AddComponent<SmashAudio>();
        }
        public static void Play(Vector3 at,float speed,Surface material)
        {
            Prepare();
            instance.Emit(at,speed,material);
        }
        void Awake()
        {
            instance=this; flow=FindAnyObjectByType<RaceFlow>();
            for(int i=0;i<4;i++)
            {
                var child=new GameObject("Smash voice "+i); child.transform.SetParent(transform);
                voices[i]=child.AddComponent<AudioSource>(); var v=voices[i];
                v.playOnAwake=false; v.spatialBlend=1; v.dopplerLevel=0; v.minDistance=8; v.maxDistance=65; v.rolloffMode=AudioRolloffMode.Linear; v.priority=160;
                for(int j=0;j<3;j++) clips[i,j]=Synthesize((Surface)i,j);
            }
        }
        static AudioClip Synthesize(Surface kind,int variant)
        {
            const int rate=22050; float duration=kind==Surface.Wood?.55f:1.0f;
            var data=new float[(int)(rate*duration)]; var random=new System.Random(3500+(int)kind*37+variant*103);
            float low=0;
            for(int i=0;i<data.Length;i++)
            {
                float t=i/(float)rate,n=(float)random.NextDouble()*2-1; low=Mathf.Lerp(low,n,.17f);
                float sample;
                if(kind==Surface.Wood)
                {
                    sample=(n*.65f+low*.35f)*Mathf.Exp(-t*24)+Mathf.Sin(t*(480+variant*70))*Mathf.Exp(-t*45)*.35f;
                    foreach(float delay in WoodDelays) if(t>delay) sample+=n*.16f*Mathf.Exp(-(t-delay)*55);
                }
                else
                {
                    float baseHz=kind==Surface.Mailbox?330:kind==Surface.Sign?520:780;
                    sample=n*.36f*Mathf.Exp(-t*28);
                    for(int partial=1;partial<=5;partial++) sample+=Mathf.Sin(t*2*Mathf.PI*(baseHz+variant*43)*partial*1.13f)*.14f/partial*Mathf.Exp(-t*(5+partial*2));
                    foreach(float delay in MetalDelays) if(t>delay) sample+=(n*.18f+low*.1f)*Mathf.Exp(-(t-delay)*32);
                }
                data[i]=Mathf.Clamp(sample,-.85f,.85f)*Mathf.Min(1,i/24f);
            }
            var clip=AudioClip.Create("Original "+kind+" "+variant,data.Length,1,rate,false); clip.SetData(data,0); return clip;
        }
        void Emit(Vector3 at,float speed,Surface material)
        {
            if(flow && flow.State!=RaceFlow.Stage.Racing || Time.time<next) return;
            int slot=System.Array.FindIndex(voices,v=>!v.isPlaying); if(slot<0) return;
            next=Time.time+.075f; Events++;
            var source=voices[slot]; source.transform.position=at;
            source.clip=clips[(int)material,Events%3]; source.pitch=.94f+(Events%5)*.035f;
            gains[slot]=Mathf.Lerp(.10f,.28f,Mathf.InverseLerp(1,32,speed));
            source.volume=gains[slot]*(flow?.Save?.Settings.vehicle??.75f); source.Play();
        }
        void Update()
        {
            for(int i=0;i<voices.Length;i++)
            {
                voices[i].volume=gains[i]*(flow?.Save?.Settings.vehicle??.75f);
                if(flow && flow.State!=RaceFlow.Stage.Racing && flow.State!=RaceFlow.Stage.Paused && flow.State!=RaceFlow.Stage.Settings) voices[i].Stop();
            }
        }
        void OnDestroy() { foreach(var clip in clips) if(clip) Destroy(clip); if(instance==this) instance=null; }
    }
}
