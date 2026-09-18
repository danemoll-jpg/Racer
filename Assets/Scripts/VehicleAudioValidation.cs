#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Racer
{
    // Editor-only, ordinary-frame virtual input; never included in the release player.
    public sealed class VehicleAudioValidation : MonoBehaviour
    {
        readonly List<string> rows = new();
        Gamepad pad; RaceFlow flow; ArcadeVehicle car; VehicleAudio sound;
        int failures;
        void Check(bool ok, string detail) { rows.Add((ok ? "PASS " : "FAIL ") + detail); if (!ok) failures++; }
        IEnumerator Wait(float t) { yield return new WaitForSecondsRealtime(t); }
        void Input(float throttle = 0, float brake = 0, float steer = 0) => InputSystem.QueueStateEvent(pad, new GamepadState { rightTrigger = throttle, leftTrigger = brake, leftStick = new Vector2(steer, 0) });
        AudioSource Voice(int index) => car.GetComponents<AudioSource>()[index];
        IEnumerator Start()
        {
            yield return null;
            Directory.CreateDirectory("Docs/CR020-021");
            flow = FindAnyObjectByType<RaceFlow>(); car = flow.Race.vehicle;
            flow.UseValidationSave(Path.GetFullPath("Temp/CR020-audio-save"));
            sound = car.GetComponent<VehicleAudio>(); pad = InputSystem.AddDevice<Gamepad>();
            Application.runInBackground = true;
            yield return Wait(.8f);
            Check(Voice(0).isPlaying && Voice(0).volume > 0, "Ready idle plays");
            for (int i = 0; i < 5; i++)
            {
                var clip = Voice(i).clip; var data = new float[clip.samples]; clip.GetData(data, 0);
                float peak = data.Max(x => Mathf.Abs(x));
                float seam = Mathf.Abs(data[0] - data[data.Length - 1]);
                Check(peak <= .651f && seam < .1f, $"{clip.name}: peak={peak:F4}, boundary delta={seam:F6}, seconds={clip.length:F2}");
            }
            flow.StartRace(); yield return Wait(.3f);
            Check(Voice(0).volume > 0 && Voice(2).volume == 0, "Countdown idle, no rolling");
            yield return Wait(3);
            Input(1); yield return Wait(2.5f);
            Check(car.ForwardSpeed > 10 && Voice(1).volume > .05f && Voice(0).pitch > 1.1f, $"Acceleration speed={car.ForwardSpeed:F2}, pitch={Voice(0).pitch:F2}");
            Check(sound.RoadWeight > .8f && Voice(2).volume > 0, $"Road color sampling weight={sound.RoadWeight:F2}");
            Input(); yield return Wait(.6f);
            Check(Voice(1).volume < .01f && Voice(2).volume > 0, "Coast reduces engine load, retains rolling");
            Input(0, 1); yield return Wait(2.2f);
            Check(car.ForwardSpeed < -1 && Voice(1).volume > .04f, $"Brake through stop into reverse speed={car.ForwardSpeed:F2}");
            Input(); car.GetComponent<VehicleRespawn>().ResetVehicle(); yield return Wait(.4f);
            Check(Voice(4).volume == 0 && !Voice(5).isPlaying, "Reset clears transient/skid sound");
            Input(.6f, 0, .2f); yield return Wait(1.5f);
            Check(Voice(4).volume < .005f, $"Ordinary corner slip={sound.Slip:F2}, skid gain={Voice(4).volume:F4}");
            Input(); car.Body.linearVelocity = car.transform.forward * 18 + car.transform.right * 9;
            yield return Wait(.08f);
            Check(sound.Slip > 2.8f && Voice(4).volume > 0, $"Actual lateral-velocity skid slip={sound.Slip:F2}");
            car.GetComponent<VehicleRespawn>().ResetVehicle(); yield return Wait(.4f);
            car.Body.position += car.transform.right * 20; car.transform.position = car.Body.position; Physics.SyncTransforms();
            Input(.5f); yield return Wait(1.5f);
            Check(sound.RoadWeight < .2f && Voice(3).volume > 0, $"Off-road rolling weight={sound.RoadWeight:F2}");
            Input(); car.GetComponent<VehicleRespawn>().ResetVehicle(); yield return Wait(.4f);
            int before = sound.ImpactCount;
            car.Body.position += Vector3.up * 4; car.transform.position = car.Body.position; car.Body.linearVelocity = car.transform.forward * 8; Physics.SyncTransforms();
            yield return Wait(.2f);
            Check(car.GroundedWheels == 0 && Voice(2).volume == 0 && Voice(3).volume == 0 && Voice(4).volume == 0, "Airborne suppresses rolling and tires immediately");
            yield return Wait(1.5f);
            Check(sound.ImpactCount > before, "Physical drop/landing emits impact");
            yield return Wait(.3f); before = sound.ImpactCount;
            sound.Impact(12, true); sound.Impact(12, false); sound.Impact(12, true);
            Check(sound.ImpactCount == before + 1, "Shared impact cooldown suppresses overlapping collision/landing/prop chatter");
            flow.Pause(); int sample = Voice(0).timeSamples; yield return Wait(.3f);
            Check(AudioListener.pause && Voice(0).timeSamples == sample, "Pause freezes audio playback position");
            flow.OpenSettings(); flow.Save.Settings.vehicle = 0; flow.CloseSettings(); flow.Resume(); yield return Wait(.2f);
            Check(car.GetComponents<AudioSource>().All(a => a.volume == 0), "Vehicle mute affects all six voices");
            flow.Save.Settings.vehicle = .4f; flow.Save.Settings.master = 0; flow.Save.ApplySettings(); flow.Save.SaveSettings(); yield return Wait(.2f);
            Check(AudioListener.volume == 0, "Master mute applies globally");
            var saved = new RacerSave(flow.Save.DirectoryPath, "street-loop-gates-v1-laps3");
            Check(saved.Settings.vehicle == .4f && saved.Settings.master == 0, "Vehicle and master settings persist");
            string legacy = Path.GetFullPath("Temp/CR020-legacy"); Directory.CreateDirectory(legacy);
            File.WriteAllText(Path.Combine(legacy, "settings.json"), "{\"version\":1,\"master\":0.3,\"ambience\":0.2,\"feedback\":0.1,\"frameLimit\":120,\"vsync\":false}");
            saved = new RacerSave(legacy, "test");
            Check(saved.Settings.vehicle == .75f && saved.Settings.master == .3f && saved.Settings.frameLimit == 120, "Legacy settings retain values and default vehicle to 75%");
            flow.Save.Settings.master = .8f; flow.Save.Settings.vehicle = .75f; flow.Save.ApplySettings();
            for (int i = 0; i < 4; i++) { flow.StartRace(); yield return Wait(.1f); flow.Pause(); flow.Resume(); }
            Check(FindObjectsByType<AudioSource>().Length == 9 && car.GetComponents<VehicleAudio>().Length == 1, "Four restart/pause/resume cycles retain exactly nine sources and one VehicleAudio");
            rows.Add("COMPLETE failures=" + failures + "; virtual input and measured samples only; not a subjective audition or physical-controller test.");
            File.WriteAllLines("Docs/CR020-021/audio-validation.txt", rows);
            flow.Pause(); Destroy(gameObject);
        }
        void OnDestroy() { if (pad != null) InputSystem.RemoveDevice(pad); }
    }
}
#endif
