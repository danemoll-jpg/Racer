using System;
using UnityEngine;
namespace Racer
{
    // Navigation metadata for intentional airborne gaps; never changes vehicle forces.
    public sealed class MountainFlights:MonoBehaviour
    {
        [Serializable] public sealed class Flight {public string name;public Vector3 start,lip,landingEnd,forward;public float approachStation,endStation;}
        public Flight[] flights=Array.Empty<Flight>();
        public bool Committed(RaceRoad road,float station){foreach(var f in flights)if(road.Relative(station,f.approachStation)<road.Relative(f.endStation,f.approachStation))return true;return false;}
        public Flight At(RaceRoad road,float station){foreach(var f in flights)if(road.Relative(station,f.approachStation)<road.Relative(f.endStation,f.approachStation))return f;return null;}
    }
}
