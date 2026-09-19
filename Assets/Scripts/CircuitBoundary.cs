using UnityEngine;
namespace Racer
{
    // Enforced before race sampling, including airborne overshoots. Traffic scenery is separate.
    [DefaultExecutionOrder(-200)]
    public sealed class CircuitBoundary : MonoBehaviour
    {
        RaceDirector race;
        public bool Outside(Vector3 p)
        {
            var q=transform.InverseTransformPoint(p);
            return q.z>26 && Mathf.Abs(q.x)<Mathf.Max(90,q.z*2);
        }
        public static bool Allowed(Vector3 p)
        {
            foreach(var boundary in FindObjectsByType<CircuitBoundary>())if(boundary.Outside(p))return false;
            return true;
        }
        void FixedUpdate()
        {
            if(!race)race=FindAnyObjectByType<RaceDirector>();
            if(!race||race.Flow.State!=RaceFlow.Stage.Racing||!Outside(race.vehicle.Body.position))return;
            var body=race.vehicle.Body;var local=transform.InverseTransformPoint(body.position);
            local.z=24;var p=transform.TransformPoint(local);
            body.position=p;race.vehicle.transform.position=p;
            float outward=Vector3.Dot(body.linearVelocity,transform.forward);
            if(outward>0)body.linearVelocity-=transform.forward*outward;
            race.ResetSampling(p,Time.timeAsDouble);
        }
    }
    // Decorative traffic stays behind the closure on a separate access road, hidden at tunnel ends.
    public sealed class ContinuationTraffic : MonoBehaviour
    {
        public Transform[] cars;
        public float[] heights;
        public float start=-150,step=5;
        public float Height(float z)
        {if(heights==null||heights.Length<2)return 0;float index=Mathf.Clamp((z-start)/step,0,heights.Length-1);int i=Mathf.Min((int)index,heights.Length-2);return Mathf.Lerp(heights[i],heights[i+1],index-i);}
        void Update()
        {
            for(int i=0;i<cars.Length;i++)
            {
                float s=Mathf.Repeat(Time.time*15+i*85,340);
                bool reverse=i%2==1;
                float z=65+(reverse?340-s:s);
                cars[i].localPosition=new(reverse?-3:3,Height(z)+1,z);
                cars[i].localRotation=Quaternion.LookRotation(new Vector3(0,Height(z+1)-Height(z-1),2)*(reverse?-1:1));
            }
        }
    }
}
