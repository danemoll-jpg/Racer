using UnityEngine;
namespace Racer
{
    // Original synthesized short gobble: pulsed, descending, rough voiced syllables.
    public static class TurkeyVoice
    {
        public static AudioClip Create(){const int rate=44100;var data=new float[(int)(rate*1.05f)];float phase=0;for(int i=0;i<data.Length;i++){float t=i/(float)rate;float syllable=Mathf.Repeat(t*17,1);float env=Mathf.Sin(Mathf.PI*syllable);env*=env*Mathf.Sin(Mathf.PI*t/1.05f);phase+=2*Mathf.PI*(430-170*syllable+35*Mathf.Sin(t*91))/rate;data[i]=env*(Mathf.Sin(phase)+.38f*Mathf.Sin(phase*2.02f)+.17f*Mathf.Sin(phase*3.1f))*.35f;}var clip=AudioClip.Create("Wild turkey short gobble",data.Length,1,rate,false);clip.SetData(data,0);return clip;}
    }
}
