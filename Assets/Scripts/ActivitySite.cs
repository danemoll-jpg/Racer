using UnityEngine;
namespace Racer
{
    public sealed class ActivitySite:MonoBehaviour
    {
        public enum Kind { Jump, Smash, Speed }
        public string id,title;
        public Kind kind;
        public float radius=25,bronze=6,silver=14,gold=22;
        public Vector3 forward=Vector3.forward;
        public bool bothDirections=true;
        public BreakableProp[] props;
        public float Seconds=45;
        public float[] vehicleBronze,vehicleSilver,vehicleGold;
        public void Targets(string vehicle,out float b,out float s,out float g){int i=System.Array.FindIndex(VehicleProfile.All,p=>p.Id==vehicle);b=vehicleBronze?.Length==4?vehicleBronze[i]:bronze;s=vehicleSilver?.Length==4?vehicleSilver[i]:silver;g=vehicleGold?.Length==4?vehicleGold[i]:gold;}
        public int Medal(float value,string vehicle){Targets(vehicle,out float b,out float s,out float g);return value>=g?3:value>=s?2:value>=b?1:0;}
    }
}
