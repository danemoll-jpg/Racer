using System;
using UnityEngine;

namespace Racer
{
    [RequireComponent(typeof(ArcadeVehicle), typeof(VehicleInput))]
    public sealed class VehicleRespawn : MonoBehaviour
    {
        [Tooltip("Fixed clear pad. Reset never tries to spawn on a ramp or beside an obstacle.")]
        public Transform spawnPoint;
        public float fallResetHeight = -15;
        public event Action Respawned;
        Vector3 initialPosition;
        Quaternion initialRotation;
        ArcadeVehicle vehicle;
        VehicleInput input;
        void Awake()
        {
            vehicle = GetComponent<ArcadeVehicle>(); input = GetComponent<VehicleInput>();
            initialPosition = transform.position;
            initialRotation = Quaternion.Euler(0, transform.eulerAngles.y, 0);
        }
        void FixedUpdate()
        {
            if (input.ConsumeReset() || transform.position.y < fallResetHeight) ResetVehicle();
        }
        public void ResetVehicle()
        {
            var body = vehicle.Body;
            body.position = spawnPoint ? spawnPoint.position : initialPosition;
            body.rotation = spawnPoint ? Quaternion.Euler(0, spawnPoint.eulerAngles.y, 0) : initialRotation;
            body.linearVelocity = Vector3.zero; body.angularVelocity = Vector3.zero;
            transform.SetPositionAndRotation(body.position, body.rotation);
            vehicle.ClearSteering(); body.WakeUp();
            Respawned?.Invoke();
        }
    }
}
