using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Racer
{
    [DisallowMultipleComponent]
    public sealed class RaceFlow : MonoBehaviour
    {
        public enum Stage { Ready, Countdown, Racing, Paused, Results, Settings, Garage }
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
        AudioClip tick, go, finish, record, click, ding, buzz;
        float nextBuzz;
        float finishAt;
        public int CheckpointDings { get; private set; }
        public int CheckpointBuzzes { get; private set; }

        float noticeUntil;
        int lastTick;
        bool originalKinematic, locked;

        void Start()
        {
            Race = GetComponent<RaceDirector>();
            input = Race.vehicle.GetComponent<VehicleInput>();
            respawn = Race.vehicle.GetComponent<VehicleRespawn>();
            if (!Race.vehicle.GetComponent<VehicleAudio>()) Race.vehicle.gameObject.AddComponent<VehicleAudio>();
            originalKinematic = Race.vehicle.Body.isKinematic;
            string root = Path.Combine(Application.persistentDataPath, "Phase7", "street-loop-gates-v1-laps" + Race.laps);
            // Editor/standalone validation uses a separate directory, never the player's records.
            var args = System.Environment.GetCommandLineArgs();
            for (int i = 0; i + 1 < args.Length; i++) if (args[i] == "-racerTestSave") root = Path.GetFullPath(args[i + 1]);
            Save = new RacerSave(root, "street-loop-gates-v1-laps" + Race.laps);
            Race.opponents = Save.Settings.opponents; Race.traffic = Save.Settings.traffic;
            Race.difficulty = Mathf.Clamp(Save.Settings.difficulty,0,2);
            var configuration = Race.vehicle.GetComponent<VehicleConfiguration>();
            if (!configuration) configuration = Race.vehicle.gameObject.AddComponent<VehicleConfiguration>();
            configuration.Apply(Save.Settings.vehicleId);
            Save.SelectRecords(Race.Category); Save.ApplySettings();
            feedback = gameObject.AddComponent<AudioSource>();
            feedback.playOnAwake = false; feedback.spatialBlend = 0; feedback.ignoreListenerPause = true;
            tick = Tone("Countdown", 520, .09f); go = Tone("Go", 880, .22f);
            finish = Tone("Finish", 660, .32f); record = Tone("Personal best", 1100, .22f); click = Tone("Menu", 380, .035f);
            ding = Tone("Checkpoint", 1040, .12f); buzz = Tone("Checkpoint missed", 145, .22f);
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
            if (finishAt > 0 && State != Stage.Paused && State != Stage.Settings && Time.unscaledTime >= finishAt) { finishAt = 0; Sound(finish); }
            if (menu.WasPressedThisFrame())
            {
                if (State == Stage.Racing || State == Stage.Countdown) Pause();
                else if (State == Stage.Paused) Resume();
                else if (State == Stage.Settings) CloseSettings();
                else if (State == Stage.Garage) CloseGarage();
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
                    foreach (var racer in Race.Racers) racer.Progress.BeginTiming(Time.timeAsDouble); SetStage(Stage.Racing); Notify("GO!  Shared race clock started", 3); Sound(go);
                }
            }
            if (Notice != null && Time.unscaledTime > noticeUntil) Notice = null;
        }
        void SetStage(Stage stage)
        {
            State = stage;
            bool stopped = stage == Stage.Paused || stage == Stage.Settings || stage == Stage.Ready || stage == Stage.Results || stage == Stage.Garage;
            Time.timeScale = stopped ? 0 : 1; if (stopped && stage != Stage.Results && feedback) feedback.Stop();
            AudioListener.pause = stage == Stage.Paused || stage == Stage.Settings;
            input.enabled = stage == Stage.Racing && !Race.Progress.Finished;
            respawn.enabled = stage == Stage.Racing && !Race.Progress.Finished;
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
        public void PrepareRestart() { LockVehicle(false); Notice = null; nextBuzz = finishAt = 0; CheckpointDings = CheckpointBuzzes = 0; feedback.Stop(); }
        public void SelectRecords(string category)
        {
            Save.SelectRecords(category); menus?.Show();
        }
        public void ToggleOpponents() { Race.opponents = !Race.opponents; Save.Settings.opponents = Race.opponents; Save.SaveSettings(); SelectRecords(Race.Category); Click(); }
        public void ToggleTraffic() { Race.traffic = !Race.traffic; Save.Settings.traffic = Race.traffic; Save.SaveSettings(); SelectRecords(Race.Category); Click(); }
        public void CycleDifficulty() { if(State!=Stage.Ready && State!=Stage.Results) return; Race.difficulty=(Race.difficulty+1)%3; Save.Settings.difficulty=Race.difficulty; Save.SaveSettings(); SelectRecords(Race.Category); Click(); }
        public void OpenGarage() { if(State!=Stage.Ready && State!=Stage.Results) return; SetStage(Stage.Garage); Click(); }
        public void CloseGarage() { SetStage(Stage.Ready); Click(); }
        public void SelectVehicle(string id)
        {
            if(State!=Stage.Garage) return;
            Race.vehicle.GetComponent<VehicleConfiguration>().Apply(id);
            Save.Settings.vehicleId=VehicleProfile.Find(id).Id; Save.SaveSettings(); SelectRecords(Race.Category); Click();
        }
        public void WipeoutFeedback() { if(State==Stage.Racing) Notify("WIPEOUT — recover control or R / Y to reset. Reset abandons this lap; clock and penalties continue.",5); }
        public void CheckpointFeedback(bool accepted, double seconds, int count)
        {
            if (State != Stage.Racing) return;
            if (accepted) { if (!Race.Progress.Finished) { CheckpointDings++; Sound(ding); } }
            else { Notify($"Checkpoint missed{(count > 1 ? " x" + count : "")}  +{seconds:0.0}s", 4); if (Time.time >= nextBuzz) { CheckpointBuzzes++; Sound(buzz); nextBuzz = Time.time + .5f; } }
        }
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
        public void Back() { if (State == Stage.Settings) CloseSettings(); else if (State == Stage.Paused) Resume(); else if(State==Stage.Garage) CloseGarage(); }
        public void ResetFeedback()
        {
            if (State == Stage.Racing) { feedback.Stop(); nextBuzz = Time.time + .5f; Notify("CAR RESET — current lap abandoned. Cross START again. Race clock continues.", 5); Click(); }
        }
        public void LapCompleted()
        {
            if (State != Stage.Racing) return;
            bool best = Save.RecordLap(Race.Progress.LastLap); NewLapRecord |= best;
            if (Race.Progress.Finished)
            {
                NewRaceRecord = Save.RecordRace(Race.Progress.AdjustedTime(Race.Clock));
                LockVehicle(true); input.enabled = respawn.enabled = false;
                if (Time.time < nextBuzz) finishAt = Time.unscaledTime + .3f; else Sound(finish);
                Notify("FINISHED — provisional standings; waiting up to 90s for opponents", 95);
                if ((NewLapRecord || NewRaceRecord) && finishAt == 0) feedback.PlayOneShot(record, Save.Settings.feedback * .18f);
            }
            else if (best) { Notify("NEW PERSONAL BEST LAP  " + RaceHud.FormatTime(Race.Progress.LastLap), 4); Sound(record); }
        }
        public void CompleteResults() { LockVehicle(true); SetStage(Stage.Results); }
        void Notify(string text, float duration) { Notice = text; noticeUntil = Time.unscaledTime + duration; }
        public void Click() => Sound(click);
#if UNITY_EDITOR || DEBUG
        public void UseValidationSave(string directory)
        {
            if (Race.Progress.Started) throw new System.InvalidOperationException("Validation storage must be selected before racing.");
            Save = new RacerSave(directory, "street-loop-gates-v1-laps" + Race.laps);
            Save.SelectRecords(Race.Category); Save.ApplySettings(); menus.Show();
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
            foreach (var clip in new[] { tick, go, finish, record, click, ding, buzz }) if (clip) Destroy(clip);
        }
    }
}
