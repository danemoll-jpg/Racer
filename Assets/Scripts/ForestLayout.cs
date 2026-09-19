using UnityEngine;
namespace Racer
{
    public sealed class ForestLayout:MonoBehaviour
    {
        public float[] jumpStarts,jumpEnds;
        public string[] jumpNames;
        public bool IsLaunch(float station)
        {for(int i=0;i<jumpStarts.Length;i++)if(station>=jumpStarts[i]-10&&station<=jumpEnds[i])return true;return false;}
        public bool Approach(float station)
        {for(int i=0;i<jumpStarts.Length;i++)if(station>=jumpStarts[i]-65&&station<=jumpEnds[i])return true;return false;}
    }
}
