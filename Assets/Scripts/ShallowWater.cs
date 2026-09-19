using System.Collections.Generic;
using UnityEngine;

namespace Racer
{
    // Surface geometry and immersion share the same local footprint. The bed is authored separately.
    public sealed class ShallowWater : MonoBehaviour
    {
        public bool round;
        public static readonly List<ShallowWater> Active = new();
        void OnEnable() { if (!Active.Contains(this)) Active.Add(this); }
        void OnDisable() => Active.Remove(this);
        public float Surface => transform.position.y + transform.lossyScale.y * .5f;
        public bool Contains(Vector3 point)
        {
            var p = transform.InverseTransformPoint(point);
            return round ? p.x*p.x+p.z*p.z < .25f : Mathf.Abs(p.x)<.5f && Mathf.Abs(p.z)<.5f;
        }
        public static float Sample(ArcadeVehicle car, out float surface)
        {
            surface=0; float result=0;
            foreach(var water in Active)
            {
                if(!water || water.gameObject.scene!=car.gameObject.scene || !water.Contains(car.Body.position)) continue;
                // Use wheel/body immersion, never an x/z-only trigger. Overflying jumps stay dry.
                float bottom=car.Body.position.y-car.suspensionLength;
                float depth=water.Surface-bottom;
                if(depth<=0 || bottom<water.Surface-1.6f) continue;
                float amount=Mathf.Clamp01(depth/.65f);
                if(amount>result){result=amount;surface=water.Surface;}
            }
            return result;
        }
    }
    public sealed class WaterFeedback : MonoBehaviour
    {
        ArcadeVehicle car; LineRenderer[] rings; AudioSource sound; AudioClip splash;
        RaceFlow flow;
        public int EntrySounds {get;private set;}
        public int ExitSounds {get;private set;}
        public bool FeedbackActive {get {if(sound&&sound.isPlaying)return true;if(rings!=null)foreach(var ring in rings)if(ring.enabled)return true;return false;}}
        float nextRipple, nextSound, previous; int index; readonly float[] born=new float[6];readonly Vector3[] centers=new Vector3[6];
        void Awake()
        {
            car=GetComponent<ArcadeVehicle>();flow=FindAnyObjectByType<RaceFlow>(); rings=new LineRenderer[6];
            // AI is cloned from the player. Replace inherited transient children and keep
            // our source below the vehicle's root-only engine-audio cleanup.
            foreach(Transform child in transform)if(child.name=="Water feedback"||child.name=="Water ripple"){child.gameObject.SetActive(false);Destroy(child.gameObject);}
            var pool=new GameObject("Water feedback");pool.transform.SetParent(transform,false);
            var material=new Material(Shader.Find("Universal Render Pipeline/Unlit")){color=new Color(.50f,.76f,.78f)};
            for(int i=0;i<rings.Length;i++)
            {
                var go=new GameObject("Water ripple");go.transform.SetParent(pool.transform,false);
                var line=go.AddComponent<LineRenderer>();line.sharedMaterial=material;line.loop=true;line.positionCount=16;line.widthMultiplier=.035f;line.useWorldSpace=true;line.enabled=false;rings[i]=line;born[i]=-100;
            }
            sound=pool.AddComponent<AudioSource>();sound.playOnAwake=false;sound.spatialBlend=1;sound.minDistance=5;sound.maxDistance=45;sound.dopplerLevel=0;
            splash=AmbientLife.Sound("Soft water",.32f,73,false);sound.clip=splash;
        }
        void Update()
        {
            float wet=car.WaterImmersion;
            if((wet>.08f)!=(previous>.08f) && Time.time>nextSound)
            { sound.volume=.12f*(flow?.Save?.Settings.ambience??1); sound.pitch=wet>previous?1:.8f;if(wet>previous)EntrySounds++;else ExitSounds++;sound.Play();nextSound=Time.time+.25f; }
            if(wet>.08f && car.Body.linearVelocity.magnitude>.4f && Time.time>nextRipple)
            {born[index]=Time.time;centers[index]=new Vector3(transform.position.x,car.WaterSurface+.035f,transform.position.z);index=(index+1)%rings.Length;nextRipple=Time.time+.22f;}
            for(int i=0;i<rings.Length;i++)
            {
                float age=Time.time-born[i];rings[i].enabled=wet>.01f&&age<.85f;
                if(!rings[i].enabled)continue;
                float r=.3f+age*1.7f;rings[i].widthMultiplier=.035f*(1-age/.85f);
                for(int j=0;j<16;j++){float a=j*Mathf.PI/8;rings[i].SetPosition(j,centers[i]+new Vector3(Mathf.Cos(a)*r,0,Mathf.Sin(a)*r));}
            }
            previous=wet;
        }
        public void Clear(){previous=0;nextRipple=nextSound=Time.time+.4f;if(sound)sound.Stop();if(rings!=null)for(int i=0;i<rings.Length;i++){rings[i].enabled=false;born[i]=-100;}}
        void OnDisable()=>Clear();
        void OnDestroy(){if(splash)Destroy(splash);if(rings!=null&&rings.Length>0&&rings[0])Destroy(rings[0].sharedMaterial);}
    }
}
