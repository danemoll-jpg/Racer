using System;
using UnityEngine;
namespace Racer
{
    // Navigation metadata for intentional airborne gaps; never changes vehicle forces.
    public sealed class MountainFlights:MonoBehaviour
    {
        // 0.72 AI pedal planning per flight (0 = unchanged): the most speed an AI takes off with (a faster take-off overshoots
        // onto the wrong deck), and the speed it may carry into the run-up (instead of the general 24 m/s mountain limit);
        // a flight with an entry speed is also driven at full throttle up its run-up (no pace lift).
        [Serializable] public sealed class Flight {public string name;public Vector3 start,lip,landingEnd,forward;public float approachStation,endStation;public float aiTakeoffSpeed,aiEntrySpeed;}
        public Flight[] flights=Array.Empty<Flight>();
        public bool Committed(RaceRoad road,float station){foreach(var f in flights)if(road.Relative(station,f.approachStation)<road.Relative(f.endStation,f.approachStation))return true;return false;}
        public Flight At(RaceRoad road,float station){foreach(var f in flights)if(road.Relative(station,f.approachStation)<road.Relative(f.endStation,f.approachStation))return f;return null;}
        public Flight Ahead(RaceRoad road,float station,float within){foreach(var f in flights)if(road.Relative(f.approachStation,station)<within)return f;return null;}
    }
}
