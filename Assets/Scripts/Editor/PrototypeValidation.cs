using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Racer.Editor
{
    /// <summary>Run explicitly in Play mode. Uses real PhysX scene geometry and virtual input devices.</summary>
    public static class PrototypeValidation
    {
        [UnityEditor.MenuItem("Racer/Validate Phase 1 (Play mode)")]
        public static void Run()
        {
            if (!Application.isPlaying) throw new System.InvalidOperationException("Enter Play mode first.");
            var car = Object.FindAnyObjectByType<ArcadeVehicle>();
            var input = car.GetComponent<VehicleInput>();
            var reset = car.GetComponent<VehicleRespawn>();
            var camera = Object.FindAnyObjectByType<ChaseCamera>();
            var body = car.Body;
            var report = new List<string>();
            int failures = 0;
            void Check(bool pass, string name) { report.Add((pass ? "PASS: " : "FAIL: ") + name); if (!pass) failures++; }
            var previousMode = Physics.simulationMode;
            var previousInterpolation = body.interpolation;
            Gamepad pad = null; Keyboard keyboard = null;
            void Place(Vector3 position, Quaternion rotation, Vector3 velocity)
            {
                body.position = position; body.rotation = rotation; body.linearVelocity = velocity; body.angularVelocity = Vector3.zero;
                car.transform.SetPositionAndRotation(position, rotation); car.ClearSteering(); Physics.SyncTransforms();
            }
            void Step(float throttle, float brake, float steer, int count)
            {
                for (int i = 0; i < count; i++) { car.Simulate(throttle, brake, steer, 0.02f); Physics.Simulate(0.02f); }
            }
            try
            {
                Physics.simulationMode = SimulationMode.Script; car.enabled = false; reset.enabled = false;
                body.interpolation = RigidbodyInterpolation.None;
                pad = InputSystem.AddDevice<Gamepad>(); keyboard = InputSystem.AddDevice<Keyboard>();
                InputSystem.QueueStateEvent(pad, new GamepadState { rightTrigger = 0.8f, leftStick = new Vector2(0.5f, 0) });
                InputSystem.Update(); input.SendMessage("Update");
                Check(input.Throttle > 0.75f && input.Steering > 0.3f, "Xbox layout: analog right trigger and left stick");
                Place(new Vector3(0, 0.7f, -45), Quaternion.identity, Vector3.zero);
                Step(input.Throttle, input.BrakeReverse, input.Steering, 100);
                Check(body.linearVelocity.magnitude > 8 && Mathf.Abs(car.transform.eulerAngles.y) > 10, "Gamepad input drives and steers vehicle");
                InputSystem.QueueStateEvent(pad, new GamepadState { leftTrigger = 1 }.WithButton(GamepadButton.Y));
                InputSystem.Update(); input.SendMessage("Update");
                Check(input.BrakeReverse > 0.95f, "Xbox left trigger brake/reverse binding");
                Place(new Vector3(10, 2, -40), Quaternion.Euler(0, 30, 170), new Vector3(10, -2, 3));
                reset.SendMessage("FixedUpdate");
                Check(Vector3.Distance(body.position, reset.spawnPoint.position) < 0.01f && body.linearVelocity.sqrMagnitude < 0.001f && Vector3.Dot(car.transform.up, Vector3.up) > 0.99f, "Xbox Y resets flipped car upright and clears momentum");
                Check(Vector3.Distance(camera.transform.position, car.transform.position + camera.offset) < 0.05f, "Camera snaps immediately on reset");
                InputSystem.QueueStateEvent(pad, new GamepadState());
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.W, Key.D));
                InputSystem.Update(); input.SendMessage("Update");
                Check(input.Throttle > 0.99f && input.Steering > 0.99f, "Keyboard W/D throttle and steering");
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.S, Key.A)); InputSystem.Update(); input.SendMessage("Update");
                Check(input.BrakeReverse > 0.99f && input.Steering < -0.99f, "Keyboard S/A reverse and steering");
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.UpArrow, Key.LeftArrow)); InputSystem.Update(); input.SendMessage("Update");
                Check(input.Throttle > 0.99f && input.Steering < -0.99f, "Keyboard arrow fallback");
                Place(new Vector3(20, 3, -40), Quaternion.Euler(0, 0, 180), Vector3.one);
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.R)); InputSystem.Update(); input.SendMessage("Update"); reset.SendMessage("FixedUpdate");
                Check(Vector3.Distance(body.position, reset.spawnPoint.position) < 0.01f, "Keyboard R reset");
                InputSystem.QueueStateEvent(keyboard, new KeyboardState()); InputSystem.Update(); input.SendMessage("Update");

                Place(new Vector3(0, 0.7f, -70), Quaternion.identity, Vector3.zero);
                Step(1, 0, 0, 200); float forwardSpeed = car.ForwardSpeed;
                Check(forwardSpeed > 20 && forwardSpeed < 39, "Acceleration for 4s: " + forwardSpeed.ToString("F2") + " m/s");
                Step(0, 1, 0, 50); float brakeSpeed = car.ForwardSpeed;
                Check(brakeSpeed < forwardSpeed * 0.25f, "Braking for 1s: " + brakeSpeed.ToString("F2") + " m/s");
                Step(0, 1, 0, 150);
                Check(car.ForwardSpeed < -5 && car.ForwardSpeed > -12, "Held brake engages reverse: " + car.ForwardSpeed.ToString("F2") + " m/s");

                Place(new Vector3(0, 0.7f, -30), Quaternion.identity, new Vector3(7, 0, 15));
                Step(0, 0, 0, 50);
                Check(Mathf.Abs(body.linearVelocity.x) < 1, "Lateral grip catches 7 m/s slide within 1s");
                Place(new Vector3(0, 0.7f, -35), Quaternion.identity, new Vector3(0, 0, 25));
                float minimumUp = 1;
                for (int i = 0; i < 500; i++) { Step(1, 0, 1, 1); minimumUp = Mathf.Min(minimumUp, Vector3.Dot(car.transform.up, Vector3.up)); }
                Check(minimumUp > 0.8f, "10s full-lock cornering stays upright; minimum up=" + minimumUp.ToString("F3"));

                foreach (var ramp in new[] { new Vector3(-18, 40, 1.8f), new Vector3(18, 65, 5) })
                foreach (float approach in new[] { 15f, 25f })
                {
                    Place(new Vector3(ramp.x, 0.7f, ramp.y - 18), Quaternion.identity, new Vector3(0, 0, approach));
                    float peakY = 0, peakAngular = 0, minimumRampUp = 1; bool airborne = false;
                    for (int i = 0; i < 240; i++)
                    {
                        Step(0.4f, 0, 0, 1); peakY = Mathf.Max(peakY, body.position.y); peakAngular = Mathf.Max(peakAngular, body.angularVelocity.magnitude);
                        airborne |= car.GroundedWheels == 0 && body.position.y > ramp.z;
                        minimumRampUp = Mathf.Min(minimumRampUp, Vector3.Dot(car.transform.up, Vector3.up));
                    }
                    Check(airborne && peakY > ramp.z && peakY < 15 && peakAngular < 5 && minimumRampUp > 0 && body.position.y > 0 && body.position.y < 1.2f && Vector3.Dot(car.transform.up, Vector3.up) > 0.85f,
                        "Ramp height " + ramp.z + "m at " + approach + " m/s: peak=" + peakY.ToString("F2") + "m, angular=" + peakAngular.ToString("F2") + ", minUp=" + minimumRampUp.ToString("F2") + ", landedY=" + body.position.y.ToString("F2") + ", z=" + body.position.z.ToString("F1"));
                }
                Place(new Vector3(0, -20, 0), Quaternion.identity, Vector3.down * 20); reset.SendMessage("FixedUpdate");
                Check(Vector3.Distance(body.position, reset.spawnPoint.position) < 0.01f, "Automatic fall recovery below -15m");
                camera.Snap();
                Step(1, 0, 0, 40); camera.SendMessage("LateUpdate");
                Check(Vector3.Distance(camera.transform.position, car.transform.position) < 12 && Vector3.Dot(camera.transform.forward, (car.transform.position + Vector3.up - camera.transform.position).normalized) > 0.9f, "Chase camera stays near and looks toward moving vehicle");
            }
            finally
            {
                if (pad != null) InputSystem.RemoveDevice(pad);
                if (keyboard != null) InputSystem.RemoveDevice(keyboard);
                InputSystem.Update(); input.SendMessage("Update");
                Physics.simulationMode = previousMode; body.interpolation = previousInterpolation;
                car.enabled = true; reset.enabled = true; reset.ResetVehicle();
            }
            report.Add("Failures: " + failures);
            report.Add("Virtual devices verify bindings; physical Xbox hardware and subjective handling require Dan's acceptance.");
            Directory.CreateDirectory("Logs"); File.WriteAllLines("Logs/Phase1Validation.txt", report);
            Debug.Log("Phase 1 validation: " + (report.Count - 2) + " checks; " + failures + " failures. See Logs/Phase1Validation.txt.");
        }
    }
}
