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
            At(0,out var f); f.y=0; f.Normalize();
            float a=Vector3.Dot(from-points[0],f),b=Vector3.Dot(to-points[0],f);
            if(a>=0 || b<0 || Vector3.Dot(heading,f)<.4f) return false;
            var p=Vector3.Lerp(from,to,-a/(b-a))-points[0];
            return Mathf.Abs(Vector3.Dot(p,Vector3.Cross(Vector3.up,f)))<halfWidth && Mathf.Abs(p.y)<4;
        }
    }

    // Each racer owns its evidence. Entry alone grants nothing; contiguous in-corridor travel
    // earns witnesses. Off-route motion freezes evidence; recovery cannot advance it.
    public sealed class BranchProgress
    {
        public WoodlandRoute Route { get; private set; }
        public float Position { get; private set; }
        public float Earned { get; private set; }
        public int Exits { get; private set; }
        public void Clear() { Route=null; Position=Earned=0; }
        public void Begin(WoodlandRoute route) { Route=route; Position=Earned=0; }
        public bool Advance(Vector3 from,Vector3 to,Vector3 heading)
        {
            var route=Route; if(!route) return false;
            float s=route.Project(to,out float lateral);
            var support=route.At(s,out var f);
            float step=Vector3.Distance(from,to);
            // Jump arcs can rise above the centreline, but the projected driving corridor stays bounded.
            // A ballistic landing may briefly overhang a bend before returning to supported trail.
            // Ground travel includes the supported three-metre shoulder; recovery stays in the core.
            float corridor=route.halfWidth+(to.y>support.y+2.5f?4:3);
            bool valid=lateral<=corridor && to.y>support.y-6 && to.y<support.y+22 && step<=10;
            if(valid && s<=Earned+step*1.4f+1 && Mathf.Abs(s-Position)<=step*1.5f+1)
            {
                Position=s;
                if(Vector3.Dot(to-from,f)>0) Earned=Mathf.Max(Earned,s);
            }
            if(valid && s>route.Length-3 && Earned>route.Length-4 && Vector3.Dot(heading,f)>.5f && Vector3.Dot(to-from,f)>0)
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
