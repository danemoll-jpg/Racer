using UnityEngine;
namespace Racer {
// Local metadata: the accepted race, physics and recovery systems own gameplay.
public sealed class BackyardForwardCourse:MonoBehaviour {
public Vector3[] anchors;
public float[] launchStations,landingStations;
public Vector3[] flightStarts,flightDirections;
public int Flight(float s) { for(int i=0;i<launchStations.Length;i++)if(s>=launchStations[i]-25&&s<=landingStations[i])return i;return -1; }
public float GridStation(RaceDirector race,int index)=>race.Origin-(index<2?7:3);
public float GridSide(int index)=>index%2==0?-1.1f:1.1f;
}}
