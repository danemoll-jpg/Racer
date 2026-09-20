using UnityEngine;

namespace Racer
{
    /// <summary>Directed timing plane with vehicle cross-section overlap at the crossing.</summary>
    public sealed class RaceGate : MonoBehaviour
    {
        public float halfWidth = 6;
        public float halfHeight = 3;
        public float upperHeight;
        public const float EdgeTolerance=.20f; // 7.9 inches beyond the rendered opening.
        // A finite swept region, used only after route progress arms the finish.
        public bool TryFinishRegion(Vector3 from, Vector3 to, out float fraction)
        {
            var a=transform.InverseTransformPoint(from);var b=transform.InverseTransformPoint(to);
            fraction=0;
            if(a.z>=0 || b.z<0) return false;
            fraction=-a.z/(b.z-a.z);var hit=Vector3.Lerp(a,b,fraction);
            return Mathf.Abs(hit.x)<=60 && hit.y>=-12 && hit.y<=120;
        }
        public bool TryCross(Vector3 from, Vector3 to, out bool forward, out float fraction)
            => TryCross(from,to,null,out forward,out fraction);
        public bool TryCross(Vector3 from,Vector3 to,BoxCollider vehicle,out bool forward,out float fraction)
        {
            var a = transform.InverseTransformPoint(from); var b = transform.InverseTransformPoint(to);
            forward = a.z < 0 && b.z >= 0;
            fraction = 0;
            if (!forward && !(a.z > 0 && b.z <= 0)) return false;
            fraction = -a.z / (b.z - a.z);
            var hit = Vector3.Lerp(a, b, fraction);
            if(!vehicle)return Mathf.Abs(hit.x)<=halfWidth&&hit.y>=-halfHeight&&hit.y<=Mathf.Max(halfHeight,upperHeight);
            var vt=vehicle.transform;
            var center=transform.InverseTransformPoint(Vector3.Lerp(from,to,fraction)+vt.TransformVector(vehicle.center));
            var x=transform.InverseTransformVector(vt.TransformVector(Vector3.right*vehicle.size.x*.5f));
            var y=transform.InverseTransformVector(vt.TransformVector(Vector3.up*vehicle.size.y*.5f));
            var z=transform.InverseTransformVector(vt.TransformVector(Vector3.forward*vehicle.size.z*.5f));
            System.Span<Vector3> corners=stackalloc Vector3[8];
            for(int i=0;i<8;i++)corners[i]=center+((i&1)==0?-x:x)+((i&2)==0?-y:y)+((i&4)==0?-z:z);
            float minX=float.PositiveInfinity,maxX=float.NegativeInfinity,minY=float.PositiveInfinity,maxY=float.NegativeInfinity;
            // Clip the actual oriented body against the plane. Projecting the full
            // vehicle width/length would over-credit yawed vehicles near a post.
            for(int i=0;i<8;i++)for(int bit=1;bit<=4;bit*=2)
            {
                if((i&bit)!=0)continue;var c=corners[i];var d=corners[i|bit];
                if(c.z*d.z>0||Mathf.Abs(d.z-c.z)<.000001f)continue;
                var p=Vector3.Lerp(c,d,Mathf.Clamp01(-c.z/(d.z-c.z)));
                minX=Mathf.Min(minX,p.x);maxX=Mathf.Max(maxX,p.x);minY=Mathf.Min(minY,p.y);maxY=Mathf.Max(maxY,p.y);
            }
            return minX<=halfWidth+EdgeTolerance&&maxX>=-halfWidth-EdgeTolerance
                &&minY<=Mathf.Max(halfHeight,upperHeight)+EdgeTolerance&&maxY>=-halfHeight-EdgeTolerance;
        }
        void OnDrawGizmosSelected()
        {
            Gizmos.matrix = transform.localToWorldMatrix; Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(halfWidth * 2, halfHeight * 2, .1f));
            Gizmos.DrawLine(Vector3.zero, Vector3.forward * 5);
        }
    }
}
