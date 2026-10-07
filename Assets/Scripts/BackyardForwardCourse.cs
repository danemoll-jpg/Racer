using UnityEngine;
namespace Racer {
// Local metadata: the accepted race, physics and recovery systems own gameplay.
public sealed class BackyardForwardCourse:MonoBehaviour {
public Vector3[] anchors;
public float[] launchStations,landingStations;
public Vector3[] flightStarts,flightDirections;
public int Flight(float s) { for(int i=0;i<launchStations.Length;i++)if(s>=launchStations[i]-25&&s<=landingStations[i])return i;return -1; }
// 0.93 (BUG-001): rows of two, 6 m apart (the longest vehicle is 5 m), the rivals from the front row back and the player's
// row last, for any field size. It had two rows 4 m apart, so a fifth and sixth vehicle were placed inside the third and
// fourth (every vehicle jammed at GO until reset) and long cars overlapped.
public float GridStation(RaceDirector race,int index){int rows=(race.RivalCount+2)/2;return race.Origin-3-6*(index<2?rows-1:(index-2)/2);}
public float GridSide(int index)=>index%2==0?-1.1f:1.1f;
}}
