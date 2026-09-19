using UnityEngine;

namespace Racer
{
    public sealed class RaceRoad : MonoBehaviour
    {
        public Vector3[] points;
        public bool forestTrail;
        public float bypassStart, bypassEnd;
        public bool InBypass(float s) => Relative(s, bypassStart) < Relative(bypassEnd, bypassStart);
        // The northern commercial corridor, with 70 m merge zones at both ends.
        public float HighwayBlend(float s) => Mathf.SmoothStep(0,1,Mathf.InverseLerp(3720,3790,s))*
            (1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(4560,4630,s)));
        public float HalfWidth(float s) => forestTrail?(s<110?6:Mathf.Repeat(s,260)<42?5:3.6f):Mathf.Lerp(4.5f,8.2f,HighwayBlend(s));
        public float TrafficLane(float s,int direction,bool inner=false) => direction*Mathf.Lerp(2.6f,inner?2.05f:6.15f,HighwayBlend(s));
        float[] distance;
        public float Length { get; private set; }

        public void Initialize()
        {
            if (distance != null && distance.Length == points.Length + 1 && Length > 0)
                return;
            distance = new float[points.Length + 1];
            for (int i = 0; i < points.Length; i++)
                distance[i + 1] = distance[i] + Vector3.Distance(points[i], points[(i + 1) % points.Length]);
            Length = distance[points.Length];
        }

        public Vector3 At(float s, out Vector3 forward)
        {
            Initialize();
            s = Mathf.Repeat(s, Length);
            int lo = 0, hi = points.Length;
            while (lo + 1 < hi)
            {
                int mid = (lo + hi) / 2;
                if (distance[mid] <= s)
                    lo = mid;
                else
                    hi = mid;
            }

            var a = points[lo];
            var b = points[(lo + 1) % points.Length];
            forward = (b - a).normalized;
            return Vector3.Lerp(a, b, (s - distance[lo]) / (distance[lo + 1] - distance[lo]));
        }

        public float Project(Vector3 p, out float lateral)
        {
            Initialize();
            float best = float.MaxValue, s = 0;
            for (int i = 0; i < points.Length; i++)
            {
                var a = points[i];
                var v = points[(i + 1) % points.Length] - a;
                float t = Mathf.Clamp01(Vector3.Dot(p - a, v) / v.sqrMagnitude);
                float d = (p - a - v * t).sqrMagnitude;
                if (d < best)
                {
                    best = d;
                    s = distance[i] + t * (distance[i + 1] - distance[i]);
                }
            }

            lateral = Mathf.Sqrt(best);
            return s;
        }

        public float Relative(float s, float origin) => Mathf.Repeat(s - origin, Length);
    }
}
