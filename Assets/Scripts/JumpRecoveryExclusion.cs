using UnityEngine;
namespace Racer
{
    // Authoring metadata only: no collider and no vehicle forces.
    public sealed class JumpRecoveryExclusion : MonoBehaviour
    {
        public Vector3 start, end;
        public float halfWidth=15;
        public bool Contains(Vector3 p)
        {
            var axis=Vector3.ProjectOnPlane(end-start,Vector3.up);
            var offset=Vector3.ProjectOnPlane(p-start,Vector3.up);
            float t=Vector3.Dot(offset,axis)/Mathf.Max(.01f,axis.sqrMagnitude);
            return t>=0&&t<=1&&(offset-axis*t).magnitude<halfWidth
                &&p.y>=Mathf.Min(start.y,end.y)-6&&p.y<=Mathf.Max(start.y,end.y)+6;
        }
    }
}
