using UnityEngine;

namespace Racer
{
    /// <summary>Authored forward-only legal branch. Distances use the road's immutable centreline.</summary>
    public sealed class WoodlandRoute : MonoBehaviour
    {
        public string title;
        public Vector3[] points;
        public float entryRoad, exitRoad, halfWidth=5, recommendedSpeed=32;
        public int[] bypassedGates;
        public bool aiValidated;
        // Optional full-map underground span; zero length preserves old routes.
        public float undergroundStart, undergroundEnd;
        public float entryHeightBelow=3, entryHeightAbove=12;
        public float entrySpeed,entrySpeedDistance;
        public float SpeedAt(float station)=>entrySpeed>0&&station<entrySpeedDistance?entrySpeed:recommendedSpeed;
        public float entryInset, entryMargin=7;
        // 0.113 Part A: an optional line for the rivals only (their steering, speed, progress on the shortcut and resets), where the
        // ground no longer follows the route's own points (Laurel Switchbacks since the 0.27 ramp). Empty = the route's points. The
        // player's progress, entry, gates and resets always use the route's own points.
        public Vector3[] aiPoints;
        public bool HasAiLine=>aiPoints!=null&&aiPoints.Length>1;
        float[] lengths, aiLengths;
        static float[] Lengths(Vector3[] p){var l=new float[p.Length];for(int i=1;i<p.Length;i++)l[i]=l[i-1]+Vector2.Distance(new(p[i-1].x,p[i-1].z),new(p[i].x,p[i].z));return l;}
        float[] AiLengths(){if(aiLengths==null||aiLengths.Length!=aiPoints.Length)aiLengths=Lengths(aiPoints);return aiLengths;}
        public float LengthFor(bool ai)=>ai&&HasAiLine?AiLengths()[^1]:Length;
        public Vector3 AtFor(bool ai,float distance,out Vector3 forward)=>ai&&HasAiLine?At(aiPoints,AiLengths(),distance,out forward):At(distance,out forward);
        public float ProjectFor(bool ai,Vector3 p,out float lateral)=>ai&&HasAiLine?Project(aiPoints,AiLengths(),p,out lateral):Project(p,out lateral);
        public float Length { get { Initialize(); return lengths[^1]; } }
        public void Initialize()
        {
            if(lengths!=null && lengths.Length==points.Length) return;
            lengths=Lengths(points);
        }
        public Vector3 At(float distance,out Vector3 forward){Initialize();return At(points,lengths,distance,out forward);}
        static Vector3 At(Vector3[] points,float[] lengths,float distance,out Vector3 forward)
        {
            distance=Mathf.Clamp(distance,0,lengths[^1]);
            int i=1; while(i<lengths.Length-1 && lengths[i]<distance) i++;
            forward=(points[i]-points[i-1]).normalized;
            return Vector3.Lerp(points[i-1],points[i],Mathf.InverseLerp(lengths[i-1],lengths[i],distance));
        }
        public float Project(Vector3 p,out float lateral){Initialize();return Project(points,lengths,p,out lateral);}
        static float Project(Vector3[] points,float[] lengths,Vector3 p,out float lateral)
        {
            float best=float.MaxValue,s=0;
            for(int i=1;i<points.Length;i++)
            {
                var v=points[i]-points[i-1]; v.y=0; var q=p-points[i-1]; q.y=0;
                float t=Mathf.Clamp01(Vector3.Dot(q,v)/Mathf.Max(.001f,v.sqrMagnitude));
                float d=(q-v*t).sqrMagnitude;
                if(d<best) { best=d; s=Mathf.Lerp(lengths[i-1],lengths[i],t); }
            }
            lateral=Mathf.Sqrt(best); return s;
        }
        public bool Enter(Vector3 from,Vector3 to,Vector3 heading)
        {
            var entrance=At(entryInset,out var f); f.y=0; f.Normalize();
            if(Vector3.Distance(from,to)>10 || Vector3.Dot(to-from,f)<=.001f) return false;
            float a=Vector3.Dot(from-entrance,f),b=Vector3.Dot(to-entrance,f);
            // Bounded entrance apron catches late/shoulder entries as well as plane crossings.
            float s=Project(to,out float lateral);
            if(b<0 || s<entryInset || s>entryInset+30 || lateral>halfWidth+entryMargin) return false;
            var support=At(s,out _);
            return to.y>support.y-entryHeightBelow && to.y<support.y+entryHeightAbove;
        }
    }

    // Entry grants the explicit gate entitlement in RaceDirector. Position is only for
    // standings/recovery; a deviation can never revoke gates already credited.
    public sealed class BranchProgress
    {
        public WoodlandRoute Route { get; private set; }
        public float Position { get; private set; }
        public float Earned { get; private set; }
        public int Exits { get; private set; }
        public float RejoinSeconds;
        public bool Ai; // 0.113 Part A: a rival: its progress is on the route's rivals' line (WoodlandRoute.aiPoints) when it has one
        public void Clear() { Route=null; Position=Earned=RejoinSeconds=0; }
        public void Begin(WoodlandRoute route) { Route=route; Position=Earned=RejoinSeconds=0; }
        public bool Advance(Vector3 from,Vector3 to,Vector3 heading)
        {
            var route=Route; if(!route) return false;
            float s=route.ProjectFor(Ai,to,out float lateral);
            var support=route.AtFor(Ai,s,out var f);
            float step=Vector3.Distance(from,to);
            // Jump arcs can rise above the centreline, but the projected driving corridor stays bounded.
            // A ballistic landing may briefly overhang a bend before returning to supported trail.
            // Ground travel includes the supported three-metre shoulder; recovery stays in the core.
            float corridor=route.halfWidth+24;
            bool valid=lateral<=corridor && to.y>support.y-30 && to.y<support.y+45 && step<=10;
            if(valid)
            {
                Position=s;
                if(Vector3.Dot(to-from,f)>0) Earned=Mathf.Max(Earned,s);
            }
            if(valid && s>route.LengthFor(Ai)-5 && Vector3.Dot(to-from,f)>0)
            { Exits++; return true; }
            return false;
        }
        public void Recovered(Vector3 position)
        {
            if(!Route) return;
            float s=Route.ProjectFor(Ai,position,out float lateral);
            if(lateral<=Route.halfWidth+1 && s<=Earned+.05f) Position=s;
        }
        public float RoadPosition => Route ? Mathf.Lerp(Route.entryRoad,Route.exitRoad,Mathf.Clamp01(Position/Route.LengthFor(Ai))) : 0;
    }
}
