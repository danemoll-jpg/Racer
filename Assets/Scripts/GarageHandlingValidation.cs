using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Racer
{
    public sealed class GarageHandlingValidation : MonoBehaviour
    {
        RaceDirector race;
        Gamepad pad;
        string output;
        readonly List<string> rows = new();
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            var a = System.Environment.GetCommandLineArgs();
            if (System.Array.IndexOf(a, "-racerGarageHandlingTest") >= 0 && System.Array.IndexOf(a, "-racerTestSave") >= 0)
                new GameObject("Isolated handling validation").AddComponent<GarageHandlingValidation>();
        }

        public static void Launch() => new GameObject("Handling validation").AddComponent<GarageHandlingValidation>();
        IEnumerator Start()
        {
            yield return null;
            race = FindAnyObjectByType<RaceDirector>();
            Application.runInBackground = true;
            output = Application.isEditor ? "Docs/CR026-027" : Path.Combine(Application.dataPath, "..", "Validation");
            Directory.CreateDirectory(output);
#if UNITY_EDITOR
  race.Flow.UseValidationSave(Path.GetFullPath("Temp/CR026-027-handling-save"));
  InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
#endif
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            race.opponents = false;
            race.traffic = false;
            pad = InputSystem.AddDevice<Gamepad>();
            rows.Add("Ordinary-frame virtual Gamepad; one initial placement per scenario, no per-frame pose or velocity corrections. No physical controller claim.");
            foreach (var profile in VehicleProfile.All)
            {
                race.Flow.CompleteResults(); race.Flow.OpenGarage(); race.Flow.SelectVehicle(profile.Id); race.Flow.CloseGarage();
                race.Flow.StartRace();
                while (race.Flow.State != RaceFlow.Stage.Racing) yield return null;
                rows.Add("VEHICLE " + profile.Id);
                yield return Drive("straight acceleration/braking/reverse", new Vector3(-565,0,536),0,profile.Small?17:22,profile.Speed,true);
                yield return Drive("fast corner entry",new Vector3(140,0,536),0,14,13,false,40);
                yield return Drive("hills",new Vector3(319,0,400),1.7f,18,29,false);
                yield return Drive("hairpin",new Vector3(546,0,-470),1.7f,22,13,false);
                yield return Drive("off-road and re-entry",new Vector3(-420,0,536),8,12,15,false);
                yield return Drive("jump 33m/s target",new Vector3(-627,0,-270),-1.5f,15,33,false);
                yield return Drive("jump 40m/s target",new Vector3(-627,0,-360),-1.5f,19,40,false);
                race.vehicle.GetComponent<VehicleRespawn>().ResetVehicle();
                yield return null;
                race.Flow.Pause(); var pos=race.vehicle.transform.position; yield return new WaitForSecondsRealtime(.3f);
                rows.Add($"Pause holds={Vector3.Distance(pos,race.vehicle.transform.position)<.01f}; audio sources={race.vehicle.GetComponents<AudioSource>().Length}; listeners={FindObjectsByType<AudioListener>().Length}; selected={race.Flow.Save.Settings.vehicleId}");
                race.Flow.Resume(); race.Flow.StartRace();
                while(race.Flow.State!=RaceFlow.Stage.Racing) yield return null;
                rows.Add($"Restart retained={race.vehicle.GetComponent<VehicleConfiguration>().profileId==profile.Id}; penalty={race.Progress.PenaltySeconds}; race voices={race.vehicle.GetComponents<AudioSource>().Length}");
                File.WriteAllLines(Path.Combine(output,"handling.txt"),rows);
            }
            var prop = GameObject.Find("Mailbox - Dan - blue X").GetComponent<BreakableProp>();
            var car = race.vehicle;
            var direction = -prop.transform.forward;
            var place = prop.transform.position - direction * 8;
            if (Physics.Raycast(place + Vector3.up * 20, Vector3.down, out var hit, 60, 1))
                place = hit.point + Vector3.up * .7f;
            car.Body.position = place;
            car.Body.rotation = Quaternion.LookRotation(direction);
            car.transform.SetPositionAndRotation(place, car.Body.rotation);
            car.Body.linearVelocity = direction * 12;
            car.Body.angularVelocity = Vector3.zero;
            race.ResetSampling(place, Time.timeAsDouble);
            Physics.SyncTransforms();
            InputSystem.QueueStateEvent(pad, new GamepadState{rightTrigger = .5f});
            yield return new WaitForSeconds(1.5f);
            rows.Add($"Mailbox physical impact: broken={prop.IsBroken}, debris={BreakableProp.MovingDebrisCount}");
            InputSystem.QueueStateEvent(pad, new GamepadState());
            yield return new WaitForSeconds(4.5f);
            rows.Add($"Debris cleanup: moving={BreakableProp.MovingDebrisCount}");
            race.RestartRace();
            yield return null;
            rows.Add($"Restart restoration: broken={prop.IsBroken}, pending={prop.PendingRestore}, penalties={race.Progress.PenaltySeconds}, next={race.Progress.NextGate}");
            InputSystem.QueueStateEvent(pad, new GamepadState());
            InputSystem.RemoveDevice(pad);
            pad = null;
            race.Flow.Pause();
            File.WriteAllLines(Path.Combine(output, "handling.txt"), rows);
        }

        IEnumerator Drive(string name, Vector3 near, float lane, float duration, float desired, bool braking, float initialSpeed = 0)
        {
            var car = race.vehicle;
            var road = race.road;
            float s = road.Project(near, out _);
            var p = road.At(s, out var f) + Vector3.Cross(Vector3.up, f).normalized * lane + Vector3.up * .8f;
            car.Body.position = p;
            car.Body.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(f, Vector3.up));
            car.transform.SetPositionAndRotation(p, car.Body.rotation);
            car.Body.linearVelocity = f.normalized * initialSpeed; car.Body.angularVelocity = Vector3.zero;
            car.ClearSteering();
            race.Progress.ResetToGrid();
            race.ResetSampling(p, Time.timeAsDouble);
            FindAnyObjectByType<ChaseCamera>().Snap();
            Physics.SyncTransforms();
            float begin = Time.time, max = 0, minUp = 1, air = 0, longestAir = 0, brakeStart = 0, brakeEnd = 0, maxLateral = 0;
            while (Time.time - begin < duration)
            {
                float elapsed = Time.time - begin;
                float at = road.Project(car.Body.position, out float lateral);
                float look = Mathf.Clamp(7 + Mathf.Abs(car.ForwardSpeed) * .45f, 8, 25);
                float targetLane=name=="off-road and re-entry" && elapsed>5 ? 0 : lane;
                var target = road.At(at + look, out var heading) + Vector3.Cross(Vector3.up, heading).normalized * targetLane;
                var local = car.transform.InverseTransformPoint(target);
                float curvature = 2 * local.x / Mathf.Max(1, local.x * local.x + local.z * local.z);
                float angle = Mathf.Lerp(car.slowSteerAngle, car.fastSteerAngle, Mathf.Clamp01(Mathf.Abs(car.ForwardSpeed) / car.topSpeed));
                float steer = Mathf.Clamp(Mathf.Atan(curvature * car.wheelbase) * Mathf.Rad2Deg / angle, -1, 1);
                bool stop = braking && elapsed > duration - 3;
                float throttle = stop ? 0 : Mathf.Clamp01((desired - car.ForwardSpeed) * .6f);
                float brake = stop ? 1 : car.ForwardSpeed > desired + 1 ? Mathf.Clamp01((car.ForwardSpeed - desired) * .3f) : 0;
                if (stop && brakeStart == 0)
                    brakeStart = car.ForwardSpeed;
                InputSystem.QueueStateEvent(pad, new GamepadState{rightTrigger = throttle, leftTrigger = brake, leftStick = new Vector2(Mathf.Abs(steer)<.001f?0:Mathf.Sign(steer)*(.12f+Mathf.Abs(steer)*.83f), 0)});
                max = Mathf.Max(max, car.ForwardSpeed);
                minUp = Mathf.Min(minUp, car.transform.up.y);
                maxLateral = Mathf.Max(maxLateral, lateral);
                if (car.GroundedWheels < 2)
                {
                    air += Time.deltaTime;
                    longestAir = Mathf.Max(air, longestAir);
                }
                else
                    air = 0;
                brakeEnd = car.ForwardSpeed;
                yield return null;
            }

            rows.Add($"{name}: peak={max:0.00}m/s ({max * 3.6f:0.0}km/h), minUp={minUp:0.000}, longestAir={longestAir:0.00}s, finalGrounded={car.GroundedWheels}, maxRoadDistance3D={maxLateral:0.00}m, brakeStart={brakeStart:0.00}, endSpeed={brakeEnd:0.00}, position={car.transform.position}");
            InputSystem.QueueStateEvent(pad, new GamepadState());
            yield return new WaitForSeconds(.2f);
        }

        void OnDestroy()
        {
            if (pad != null)
                InputSystem.RemoveDevice(pad);
        }
    }
}
