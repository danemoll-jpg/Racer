using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Racer
{
    [DisallowMultipleComponent]
    public sealed class RaceFlow : MonoBehaviour
    {
        public enum Stage { Ready, Countdown, Racing, Paused, Results, Settings }
        public Stage State { get; private set; } = Stage.Ready;
        public RacerSave Save { get; private set; }
        public RaceDirector Race { get; private set; }
        public float CountdownRemaining { get; private set; }
        public string Notice { get; private set; }
        public bool NewLapRecord { get; private set; }
        public bool NewRaceRecord { get; private set; }
        public bool MenuVisible => State != Stage.Countdown && State != Stage.Racing;
        Stage pausedStage, settingsReturn;
        VehicleInput input;
        VehicleRespawn respawn;
        RaceMenus menus;
        InputAction menu, back;
        AudioSource feedback;
        AudioClip tick, go, finish, record, click;
        float noticeUntil;
        int lastTick;
        bool originalKinematic, locked;

        void Start()
        {
            Race = GetComponent<RaceDirector>();
            input = Race.vehicle.GetComponent<VehicleInput>();
            respawn = Race.vehicle.GetComponent<VehicleRespawn>();
            originalKinematic = Race.vehicle.Body.isKinematic;
            string root = Path.Combine(Application.persistentDataPath, "Phase7", "street-loop-gates-v1-laps" + Race.laps);
            // Editor/standalone validation uses a separate directory, never the player's records.
            var args = System.Environment.GetCommandLineArgs();
            for (int i = 0; i + 1 < args.Length; i++) if (args[i] == "-racerTestSave") root = Path.GetFullPath(args[i + 1]);
            Save = new RacerSave(root, "street-loop-gates-v1-laps" + Race.laps);
            Save.ApplySettings();
            feedback = gameObject.AddComponent<AudioSource>();
            feedback.playOnAwake = false; feedback.spatialBlend = 0; feedback.ignoreListenerPause = true;
            tick = Tone("Countdown", 520, .09f); go = Tone("Go", 880, .22f);
            finish = Tone("Finish", 660, .32f); record = Tone("Personal best", 1100, .22f); click = Tone("Menu", 380, .035f);
            menu = new InputAction("Pause", InputActionType.Button);
            menu.AddBinding("<Keyboard>/enter"); menu.AddBinding("<Keyboard>/escape"); menu.AddBinding("<Gamepad>/start"); menu.Enable();
            back = new InputAction("Back", InputActionType.Button);
            back.AddBinding("<Gamepad>/buttonEast"); back.Enable();
            menus = gameObject.AddComponent<RaceMenus>(); menus.Initialize(this);
            LockVehicle(true); SetStage(Stage.Ready);
        }
        void Update()
        {
            if (Save == null) return;
            if (menu.WasPressedThisFrame())
            {
                if (State == Stage.Racing || State == Stage.Countdown) Pause();
                else if (State == Stage.Paused) Resume();
                else if (State == Stage.Settings) CloseSettings();
            }
            else if (back.WasPressedThisFrame()) Back();
            if (State == Stage.Countdown)
            {
                CountdownRemaining = Mathf.Max(0, CountdownRemaining - Time.deltaTime);
                int number = Mathf.CeilToInt(CountdownRemaining);
                if (number > 0 && number != lastTick) { lastTick = number; Sound(tick); }
                if (CountdownRemaining <= 0)
                {
                    LockVehicle(false); Race.ResetSampling(Race.vehicle.Body.position, Time.timeAsDouble);
                    SetStage(Stage.Racing); Notify("GO!  Cross START to begin timing", 3); Sound(go);
                }
            }
            if (Notice != null && Time.unscaledTime > noticeUntil) Notice = null;
        }
        void SetStage(Stage stage)
        {
            State = stage;
            bool stopped = stage == Stage.Paused || stage == Stage.Settings || stage == Stage.Ready || stage == Stage.Results;
            Time.timeScale = stopped ? 0 : 1;
            AudioListener.pause = stage == Stage.Paused || stage == Stage.Settings;
            input.enabled = stage == Stage.Racing;
            respawn.enabled = stage == Stage.Racing;
            Cursor.visible = MenuVisible; Cursor.lockState = CursorLockMode.None;
            menus?.Show();
        }
        void LockVehicle(bool value)
        {
            if (locked == value) return;
            locked = value;
            Race.vehicle.enabled = !value;
            if (value) { Race.vehicle.Body.linearVelocity = Vector3.zero; Race.vehicle.Body.angularVelocity = Vector3.zero; }
            Race.vehicle.Body.isKinematic = value || originalKinematic;
        }
        public void PrepareRestart() { LockVehicle(false); Notice = null; }
        public void BeginCountdown()
        {
            NewLapRecord = NewRaceRecord = false; CountdownRemaining = 3; lastTick = 3;
            Notice = null; LockVehicle(true); SetStage(Stage.Countdown); Sound(tick);
        }
        public void StartRace() { Click(); Race.RestartRace(); }
        public void Pause() { pausedStage = State; SetStage(Stage.Paused); Click(); }
        public void Resume() { SetStage(pausedStage); Click(); }
        public void OpenSettings() { settingsReturn = State; SetStage(Stage.Settings); Click(); }
        public void CloseSettings() { Save.SaveSettings(); SetStage(settingsReturn); Click(); }
        public void Back() { if (State == Stage.Settings) CloseSettings(); else if (State == Stage.Paused) Resume(); }
        public void ResetFeedback()
        {
            if (State == Stage.Racing) { Notify("CAR RESET — current lap abandoned. Cross START again. Race clock continues.", 5); Click(); }
        }
        public void LapCompleted()
        {
            if (State != Stage.Racing) return;
            bool best = Save.RecordLap(Race.Progress.LastLap); NewLapRecord |= best;
            if (Race.Progress.Finished)
            {
                NewRaceRecord = Save.RecordRace(Race.Progress.RaceTime(Race.Clock));
                LockVehicle(true); SetStage(Stage.Results); Sound(finish);
                if (NewLapRecord || NewRaceRecord) feedback.PlayOneShot(record, Save.Settings.feedback * .18f);
            }
            else if (best) { Notify("NEW PERSONAL BEST LAP  " + RaceHud.FormatTime(Race.Progress.LastLap), 4); Sound(record); }
        }
        void Notify(string text, float duration) { Notice = text; noticeUntil = Time.unscaledTime + duration; }
        public void Click() => Sound(click);
#if UNITY_EDITOR || DEBUG
        public void UseValidationSave(string directory)
        {
            if (Race.Progress.Started) throw new System.InvalidOperationException("Validation storage must be selected before racing.");
            Save = new RacerSave(directory, "street-loop-gates-v1-laps" + Race.laps);
            Save.ApplySettings(); menus.Show();
        }
#endif
        void Sound(AudioClip clip) { if (feedback && clip) feedback.PlayOneShot(clip, Save.Settings.feedback * .22f); }
        public void Quit()
        {
            Save.SaveSettings();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
        // Original sine synthesis with smooth envelopes; CC0, no external audio assets.
        static AudioClip Tone(string name, float hz, float seconds)
        {
            int count = Mathf.CeilToInt(44100 * seconds); var data = new float[count];
            for (int i = 0; i < count; i++) data[i] = Mathf.Sin(i * 2 * Mathf.PI * hz / 44100) * Mathf.Sin(Mathf.PI * i / count) * .6f;
            var clip = AudioClip.Create(name, count, 1, 44100, false); clip.SetData(data, 0); return clip;
        }
        void OnDestroy()
        {
            menu?.Dispose(); back?.Dispose(); Time.timeScale = 1; AudioListener.pause = false;
            foreach (var clip in new[] { tick, go, finish, record, click }) if (clip) Destroy(clip);
        }
    }
}
