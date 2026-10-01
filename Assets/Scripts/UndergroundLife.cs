using UnityEngine;
namespace Racer
{
    // Fixed, non-colliding scenery. No spawns, navigation or vehicle forces.
    public sealed class UndergroundLife : MonoBehaviour
    {
        public Transform[] rats;
        public Vector3[] refuges;
        public Vector3 entrance, forward;
        public bool cave;
        Vector3[] homes;
        RaceDirector race; AudioSource sound; AudioClip clip;
        float started=-100, nextSound, awaySince=-1; bool armed=true;
        public int Encounters {get;private set;}
        public int Sounds {get;private set;}
        void Start()
        {
            race=FindAnyObjectByType<RaceDirector>();
            homes=new Vector3[rats.Length];for(int i=0;i<rats.Length;i++)homes[i]=rats[i].position;
            var audioObject=new GameObject("Localized underground audio");audioObject.transform.SetParent(transform,false);
            sound=audioObject.AddComponent<AudioSource>();sound.playOnAwake=false;sound.spatialBlend=1;
            sound.minDistance=cave?4:16;sound.maxDistance=cave?25:48;sound.rolloffMode=AudioRolloffMode.Linear;sound.dopplerLevel=0;
            if(!cave)sound.priority=80;
            const int rate=22050;float duration=cave?1.3f:2.2f;var samples=new float[(int)(rate*duration)];var rng=new System.Random(cave?37:83);
            for(int i=0;i<samples.Length;i++){
                float t=i/(float)rate;
                if(cave){float pulse=t% .37f;float env=Mathf.Exp(-pulse*38);samples[i]=.28f*env*Mathf.Sin(2*Mathf.PI*(1500*pulse-800*pulse*pulse));}
                else{
                    // Three short chirps over irregular scratching; leave space between
                    // calls instead of a thin, regularly repeating high-frequency tone.
                    float chirp=0;
                    foreach(float onset in new[]{.03f,.47f,1.12f}){
                        float u=(t-onset)/.16f;
                        if(u>0&&u<1)chirp+=.48f*Mathf.Sin(Mathf.PI*u)*Mathf.Sin(2*Mathf.PI*(1900*(t-onset)+650*(t-onset)*(t-onset)));
                    }
                    float scratch=Mathf.Pow(Mathf.Max(0,Mathf.Sin(t*83+1.4f*Mathf.Sin(t*21))),4);
                    samples[i]=chirp+.30f*(float)(rng.NextDouble()*2-1)*scratch*(1-t/duration*.65f);
                }
                samples[i]*=Mathf.Clamp01(t*60)*Mathf.Clamp01((duration-t)*8);
            }
            clip=AudioClip.Create(cave?"Quiet cave drips":"Rat squeaks and scurry",samples.Length,1,rate,false);clip.SetData(samples,0);sound.clip=clip;nextSound=Time.time+7;
        }
        void PlayAt(Vector3 p){sound.transform.position=p;sound.volume=(cave?.11f:.85f)*(race.Flow.Save?.Settings.ambience??1);sound.Play();Sounds++;}
        Vector3 RatCenter(){var p=Vector3.zero;int count=0;foreach(var rat in rats)if(rat&&rat.gameObject.activeSelf){p+=rat.position;count++;}return count>0?p/count:entrance;}
        void Update()
        {
            if(!race||!race.vehicle||race.Flow.State!=RaceFlow.Stage.Racing)return;
            var p=race.vehicle.Body.position;
            if(cave){if(Vector3.Distance(p,entrance)<24&&Time.time>nextSound){PlayAt(entrance);nextSound=Time.time+8+Mathf.PingPong(Time.time,5);}return;}
            float distance=Vector3.Distance(p,entrance),along=Vector3.Dot(p-entrance,forward);
            if(armed&&distance<15&&along>-12&&along<6&&Vector3.Dot(race.vehicle.Body.linearVelocity,forward)>1){armed=false;started=Time.time;Encounters++;}
            if(!armed){
                float age=Time.time-started;
                if(age>=.15f&&Sounds<Encounters)PlayAt(RatCenter());
                for(int i=0;i<rats.Length;i++){
                    float t=Mathf.Clamp01((age-.15f-i*.10f)/2.6f);var target=Vector3.Lerp(homes[i],refuges[i],t);
                    rats[i].position=target+Vector3.up*(Mathf.Sin(age*55+i)*.018f*(1-t));rats[i].rotation=Quaternion.LookRotation((refuges[i]-homes[i]).normalized);
                    rats[i].gameObject.SetActive(t<1);
                }
                if(sound.isPlaying){sound.transform.position=RatCenter();sound.volume=.85f*(race.Flow.Save?.Settings.ambience??1);}
                if(distance>65){if(awaySince<0)awaySince=Time.time;if(Time.time-awaySince>20){armed=true;for(int i=0;i<rats.Length;i++){rats[i].position=homes[i];rats[i].gameObject.SetActive(true);}}}else awaySince=-1;
            }
        }
        void OnDestroy(){if(clip)Destroy(clip);}
    }
}
