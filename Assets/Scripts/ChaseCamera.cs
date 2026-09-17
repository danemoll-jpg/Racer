using UnityEngine;

namespace Racer
{
    public sealed class ChaseCamera : MonoBehaviour
    {
        public Transform target;
        public Vector3 offset = new(0, 3.6f, -7.5f);
        public float positionSmoothTime = 0.16f;
        public float headingResponse = 6;
        public float lookAhead = 3;
        public LayerMask obstructionMask = 1;
        public float collisionRadius = 0.3f;
        Vector3 velocity;
        Quaternion heading;
        VehicleRespawn respawn;
        void OnEnable()
        {
            if (!target) return;
            respawn = target.GetComponent<VehicleRespawn>();
            if (respawn) respawn.Respawned += Snap;
            Snap();
        }
        void OnDisable() { if (respawn) respawn.Respawned -= Snap; }
        public void Snap()
        {
            if (!target) return;
            heading = Quaternion.Euler(0, target.eulerAngles.y, 0); velocity = Vector3.zero;
            transform.position = target.position + heading * offset;
            Aim();
        }
        void LateUpdate()
        {
            if (!target) return;
            heading = Quaternion.Slerp(heading, Quaternion.Euler(0, target.eulerAngles.y, 0), 1 - Mathf.Exp(-headingResponse * Time.deltaTime));
            Vector3 desired = target.position + heading * offset;
            Vector3 pivot = target.position + Vector3.up;
            Vector3 direction = desired - pivot;
            if (Physics.SphereCast(pivot, collisionRadius, direction.normalized, out RaycastHit hit, direction.magnitude, obstructionMask, QueryTriggerInteraction.Ignore))
                desired = pivot + direction.normalized * Mathf.Max(0, hit.distance - 0.1f);
            Vector3 smoothed = Vector3.SmoothDamp(transform.position, desired, ref velocity, positionSmoothTime);
            // Clip the smoothed path too, so smoothing cannot carry the camera through a wall.
            direction = smoothed - pivot;
            if (Physics.SphereCast(pivot, collisionRadius, direction.normalized, out hit, direction.magnitude, obstructionMask, QueryTriggerInteraction.Ignore))
                smoothed = pivot + direction.normalized * Mathf.Max(0, hit.distance - 0.1f);
            transform.position = smoothed; Aim();
        }
        void Aim() => transform.LookAt(target.position + Vector3.up * 0.7f + heading * Vector3.forward * lookAhead, Vector3.up);
    }
}
