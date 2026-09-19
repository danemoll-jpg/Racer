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
        public float entryInset, entryMargin=7;
        float[] lengths;
        public float Length { get { Initialize(); return lengths[^1]; } }
        public void Initialize()
        {
            if(lengths!=null && lengths.Length==points.Length) return;
            lengths=new float[points.Length];
            for(int i=1;i<points.Length;i++) lengths[i]=lengths[i-1]+Vector2.Distance(new(points[i-1].x,points[i-1].z),new(points[i].x,points[i].z));
        }
        public Vector3 At(float distance,out Vector3 forward)
        {
            Initialize(); distance=Mathf.Clamp(distance,0,lengths[^1]);
            int i=1; while(i<lengths.Length-1 && lengths[i]<distance) i++;
            forward=(points[i]-points[i-1]).normalized;
            return Vector3.Lerp(points[i-1],points[i],Mathf.InverseLerp(lengths[i-1],lengths[i],distance));
        }
        public float Project(Vector3 p,out float lateral)
        {
            Initialize(); float best=float.MaxValue,s=0;
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
            return to.y>support.y-3 && to.y<support.y+12;
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
        public void Clear() { Route=null; Position=Earned=RejoinSeconds=0; }
        public void Begin(WoodlandRoute route) { Route=route; Position=Earned=RejoinSeconds=0; }
        public bool Advance(Vector3 from,Vector3 to,Vector3 heading)
        {
            var route=Route; if(!route) return false;
            float s=route.Project(to,out float lateral);
            var support=route.At(s,out var f);
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
            if(valid && s>route.Length-5 && Vector3.Dot(to-from,f)>0)
            { Exits++; return true; }
            return false;
        }
        public void Recovered(Vector3 position)
        {
            if(!Route) return;
            float s=Route.Project(position,out float lateral);
            if(lateral<=Route.halfWidth+1 && s<=Earned+.05f) Position=s;
        }
        public float RoadPosition => Route ? Mathf.Lerp(Route.entryRoad,Route.exitRoad,Mathf.Clamp01(Position/Route.Length)) : 0;
    }
}
