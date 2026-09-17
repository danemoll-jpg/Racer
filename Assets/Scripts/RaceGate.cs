using UnityEngine;

namespace Racer
{
    /// <summary>A bounded directed plane, swept by the car centre (independent of colliders).</summary>
    public sealed class RaceGate : MonoBehaviour
    {
        public float halfWidth = 6;
        public float halfHeight = 3;
        public bool TryCross(Vector3 from, Vector3 to, out bool forward, out float fraction)
        {
            var a = transform.InverseTransformPoint(from); var b = transform.InverseTransformPoint(to);
            forward = a.z < 0 && b.z >= 0;
            fraction = 0;
            if (!forward && !(a.z > 0 && b.z <= 0)) return false;
            fraction = -a.z / (b.z - a.z);
            var hit = Vector3.Lerp(a, b, fraction);
            return Mathf.Abs(hit.x) <= halfWidth && Mathf.Abs(hit.y) <= halfHeight;
        }
        void OnDrawGizmosSelected()
        {
            Gizmos.matrix = transform.localToWorldMatrix; Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(halfWidth * 2, halfHeight * 2, .1f));
            Gizmos.DrawLine(Vector3.zero, Vector3.forward * 5);
        }
    }
}
