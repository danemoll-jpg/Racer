using UnityEngine;

namespace Racer {
// Two voices for the entire scene. Race restarts/reset do not restart or duplicate ambience.
[DisallowMultipleComponent]
public sealed class WoodlandAmbience : MonoBehaviour {
    public AudioClip wind;
    public AudioClip[] birds;
    AudioSource windSource, birdSource;
    Transform listener;
    float nextBird;
    readonly System.Random random = new(6018);
    public float WoodlandWeight { get; private set; }
    float Range(float min, float max) => Mathf.Lerp(min,max,(float)random.NextDouble());
    void Awake() {
        listener=FindAnyObjectByType<AudioListener>()?.transform;
        windSource=gameObject.AddComponent<AudioSource>();
        windSource.clip=wind;windSource.loop=true;windSource.spatialBlend=0;
        windSource.volume=0;windSource.playOnAwake=false;windSource.priority=180;
        if(wind)windSource.Play();
        var bird=new GameObject("Occasional woodland bird");bird.transform.SetParent(transform,false);
        birdSource=bird.AddComponent<AudioSource>();birdSource.spatialBlend=1;
        birdSource.rolloffMode=AudioRolloffMode.Linear;birdSource.minDistance=8;birdSource.maxDistance=65;
        birdSource.dopplerLevel=0;birdSource.playOnAwake=false;birdSource.priority=190;
        nextBird=Time.time+Range(8,15);
    }
    void Update() {
        if(!listener)return;
        // Broad northern commercial zone; no trigger boundaries or abrupt transitions.
        float target=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(460,560,listener.position.z));
        WoodlandWeight=Mathf.MoveTowards(WoodlandWeight,target,Time.deltaTime*.12f);
        windSource.volume=Mathf.MoveTowards(windSource.volume,Mathf.Lerp(.085f,.15f,WoodlandWeight),Time.deltaTime*.025f);
        if(Time.time<nextBird||birdSource.isPlaying)return;
        nextBird=Time.time+Range(12,26);
        if(birds==null||birds.Length==0)return;
        float angle=Range(0,Mathf.PI*2),radius=Range(18,32);
        birdSource.transform.position=listener.position+new Vector3(Mathf.Cos(angle)*radius,Range(5,10),Mathf.Sin(angle)*radius);
        birdSource.clip=birds[random.Next(birds.Length)];birdSource.pitch=Range(.88f,1.12f);
        birdSource.volume=Mathf.Lerp(.025f,.13f,WoodlandWeight);birdSource.Play();
    }
}
}
