using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Racer
{
    [DisallowMultipleComponent]
    public sealed class RaceFlow : MonoBehaviour
    {
        public enum Stage { Ready, Countdown, Racing, Paused, Results, Settings, Garage, Roster, Boards, Courses }
        public Stage State { get; private set; } = Stage.Ready;
        public RacerSave Save { get; private set; }
        public RaceDirector Race { get; private set; }
        public LocalRadio Radio { get; private set; }
        public RecordBoards Boards { get; private set; }
        public ArcadeActivities Activities {get;private set;}
        public int LapRank { get; private set; }
        public int RaceRank { get; private set; }
        string attempt;
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
        public string PenaltyNotice => Time.unscaledTime<penaltyUntil && pendingMisses>0 ? $"Missed {pendingMisses} gate{(pendingMisses==1?"":"s")}  +{pendingMisses*5}s (5s each)" : null;
        int pendingMisses;
        float penaltyUntil;
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
            Boards = new RecordBoards(root);
            Race.opponents = Save.Settings.opponents; Race.traffic = Save.Settings.traffic;
            Race.difficulty = Mathf.Clamp(Save.Settings.difficulty,0,2);
            var configuration = Race.vehicle.GetComponent<VehicleConfiguration>();
            if (!configuration) configuration = Race.vehicle.gameObject.AddComponent<VehicleConfiguration>();
            configuration.Apply(Race.EligibleVehicle(Save.Settings.vehicleId));
            RestoreChoices();
            configuration.SetBodyColor(SelectedColor);
            Save.SelectRecords(Race.Category); Save.ApplySettings();
            Radio=LocalRadio.Attach(this);
            var listener=FindAnyObjectByType<AudioListener>();if(listener&&!listener.GetComponent<AudioCeiling>())listener.gameObject.AddComponent<AudioCeiling>();
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
            Activities=gameObject.AddComponent<ArcadeActivities>();Activities.Initialize(Race,root);
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
                else if (State == Stage.Roster) CloseGarage();
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
            if(stage!=Stage.Racing&&stage!=Stage.Paused&&stage!=Stage.Settings)
                Race?.GetComponent<WrongWayGuidance>()?.Clear();
            bool stopped = stage != Stage.Racing && stage != Stage.Countdown;
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
        public void PrepareRestart()
        {
            Activities?.NewSession();
            var runoff=Race.vehicle.GetComponent<RoadDriver>();
            if(runoff) { runoff.enabled=false; Destroy(runoff); }
            LockVehicle(false); Race.vehicle.enabled=true;
            Notice = null; nextBuzz = finishAt = penaltyUntil = 0; pendingMisses=0; CheckpointDings = CheckpointBuzzes = 0; feedback.Stop();
        }
        public void SelectRecords(string category)
        {
            Save.SelectRecords(category); menus?.Show();
        }
        public void ToggleOpponents() { Race.opponents = !Race.opponents; Save.Settings.opponents = Race.opponents; Save.SaveSettings(); SelectRecords(Race.Category); Click(); }
        public void ToggleTraffic() { Race.traffic = !Race.traffic; Save.Settings.traffic = Race.traffic; Save.SaveSettings(); SelectRecords(Race.Category); Click(); }
        public void CycleDifficulty() { if(State!=Stage.Ready && State!=Stage.Results) return; Race.difficulty=(Race.difficulty+1)%3; Save.Settings.difficulty=Race.difficulty; Save.SaveSettings(); SelectRecords(Race.Category); Click(); }
        public void OpenGarage() { if(State!=Stage.Ready && State!=Stage.Results) return; SetStage(Stage.Garage); Click(); }
        public void CloseGarage() { SetStage(Stage.Ready); Click(); }
        public void OpenRoster() { if(State!=Stage.Ready && State!=Stage.Results) return; SetStage(Stage.Roster); Click(); }
        public void OpenBoards() { SetStage(Stage.Boards); Click(); }
        public void OpenCourses() { SetStage(Stage.Courses); Click(); }
        public void SelectCourse(bool lake)=>SelectCourse(lake,false);
        public void SelectCourse(bool lake,bool reverse)
        {
            if(State!=Stage.Courses)return;
            Save.SaveSettings(); Time.timeScale=1; AudioListener.pause=false;
            UnityEngine.SceneManagement.SceneManager.LoadScene(reverse?(lake?"ForestLoopReverse":"StreetLoopReverse"):(lake?"LakeWoods":"StreetLoopGreybox"));
        }
        public int SelectedColor => Save.Settings.bodyColors[System.Array.FindIndex(VehicleProfile.All,p=>p.Id==Save.Settings.vehicleId)];
        void RestoreChoices()
        {
            Save.Settings.vehicleId=Race.EligibleVehicle(Save.Settings.vehicleId);
            if(Save.Settings.bodyColors==null || Save.Settings.bodyColors.Length!=4) Save.Settings.bodyColors=new[]{-1,-1,-1,-1};
            if(Save.Settings.opponentChoices==null || Save.Settings.opponentChoices.Length!=3) Save.Settings.opponentChoices=new[]{"mixed","mixed","mixed"};
            if(Save.Settings.opponentRoster==null || Save.Settings.opponentRoster.Length!=3) Save.Settings.opponentRoster=new[]{"tourer","moto","atv"};
            Race.opponentRoster=(string[])Save.Settings.opponentRoster.Clone();
            for(int i=0;i<3;i++) Race.opponentRoster[i]=Race.EligibleVehicle(Race.opponentRoster[i]);
        }
        public void SetColor(int color)
        {
            if(State!=Stage.Garage) return;
            int i=System.Array.FindIndex(VehicleProfile.All,p=>p.Id==Save.Settings.vehicleId);
            Save.Settings.bodyColors[i]=Mathf.Clamp(color,0,VehiclePaint.Colors.Length-1);
            Race.vehicle.GetComponent<VehicleConfiguration>().SetBodyColor(SelectedColor);
            Save.SaveSettings(); menus.Show(); Click();
        }
        public void CycleOpponent(int slot)
        {
            if(State!=Stage.Roster || slot<0 || slot>=3) return;
            var choices=Race.Forest?new[]{"moto","atv","random","mixed"}:new[]{"original","tourer","moto","atv","random","mixed"};
            int i=System.Array.IndexOf(choices,Save.Settings.opponentChoices[slot]);
            Save.Settings.opponentChoices[slot]=choices[(i+1)%choices.Length]; ResolveRoster();
        }
        public void MixedRoster() { if(State!=Stage.Roster) return; Save.Settings.opponentChoices=new[]{"mixed","mixed","mixed"}; ResolveRoster(); }
        public void ResolveRoster()
        {
            if(State!=Stage.Roster) return;
            var used=new System.Collections.Generic.HashSet<string>();
            for(int i=0;i<3;i++)
            {
                string choice=Save.Settings.opponentChoices[i];
                if(choice!="random" && choice!="mixed") { Race.opponentRoster[i]=Race.EligibleVehicle(choice); used.Add(Race.opponentRoster[i]); }
            }
            for(int i=0;i<3;i++)
            {
                string choice=Save.Settings.opponentChoices[i]; if(choice!="random" && choice!="mixed") continue;
                var candidates=new System.Collections.Generic.List<string>();
                foreach(var p in Race.EligibleVehicles) if(choice=="random" || !used.Contains(p.Id)) candidates.Add(p.Id);
                if(candidates.Count==0) foreach(var p in Race.EligibleVehicles) candidates.Add(p.Id);
                Race.opponentRoster[i]=candidates[Random.Range(0,candidates.Count)]; used.Add(Race.opponentRoster[i]);
            }
            Save.Settings.opponentRoster=(string[])Race.opponentRoster.Clone(); Save.SaveSettings(); SelectRecords(Race.Category); Click();
        }
        public void SelectVehicle(string id)
        {
            if(State!=Stage.Garage) return;
            id=Race.EligibleVehicle(id);
            Race.vehicle.GetComponent<VehicleConfiguration>().Apply(id);
            Save.Settings.vehicleId=VehicleProfile.Find(id).Id; Save.SaveSettings(); SelectRecords(Race.Category); Click();
            Race.vehicle.GetComponent<VehicleConfiguration>().SetBodyColor(SelectedColor); menus.Show();
        }
        public void WipeoutFeedback() { if(State==Stage.Racing) Notify("R / Y: right vehicle locally",2); }
        public void ClearRecoveryFeedback() { if(Notice=="R / Y: right vehicle locally") Notice=null; }
        public void CheckpointFeedback(bool accepted, double seconds, int count)
        {
            if (State != Stage.Racing) return;
            if (accepted) { if (!Race.Progress.Finished) { CheckpointDings++; Sound(ding); } }
            else { if(Time.unscaledTime>=penaltyUntil) pendingMisses=0; pendingMisses+=count; penaltyUntil=Time.unscaledTime+5; if (Time.time >= nextBuzz) { CheckpointBuzzes++; Sound(buzz); nextBuzz = Time.time + .5f; } }
        }
        public void BeginCountdown()
        {
            attempt=System.Guid.NewGuid().ToString("N");LapRank=RaceRank=0;Boards.BeginAttempt();
            NewLapRecord = NewRaceRecord = false; CountdownRemaining = 3; lastTick = 3;
            Notice = null; LockVehicle(true); SetStage(Stage.Countdown); Sound(tick);
        }
        public void StartRace() { Race.FreeRoam=false;SetGateVisibility(true);Click(); Race.RestartRace(); }
        public void StartFreeRoam(){Race.FreeRoam=true;SetGateVisibility(false);Click();Race.RestartRace();}
        public void BeginRoaming(){attempt=null;CountdownRemaining=0;LapRank=RaceRank=0;NewLapRecord=NewRaceRecord=false;LockVehicle(false);Race.GetComponent<WrongWayGuidance>()?.Clear();SetStage(Stage.Racing);}
        void SetGateVisibility(bool visible){foreach(var gate in Race.gates)foreach(var renderer in gate.GetComponentsInChildren<Renderer>(true))renderer.enabled=visible;}
        public void Pause() { pausedStage = State; SetStage(Stage.Paused); Click(); }
        public void Resume() { SetStage(pausedStage); Click(); }
        public void OpenSettings() { settingsReturn = State; SetStage(Stage.Settings); Click(); }
        public void CloseSettings() { Save.SaveSettings(); SetStage(settingsReturn); Click(); }
        public void Back() { if (State == Stage.Settings) CloseSettings(); else if (State == Stage.Paused) Resume(); else if(State==Stage.Garage || State==Stage.Roster || State==Stage.Boards || State==Stage.Courses) CloseGarage(); }
        public void QuitRace()
        {
            if(State!=Stage.Paused && State!=Stage.Results && State!=Stage.Settings) return;
            PrepareRestart(); Race.AbandonEvent(); respawn.CancelRecovery(); LockVehicle(true);
            Race.FreeRoam=false;SetGateVisibility(true);
            NewLapRecord=NewRaceRecord=false; Save.SaveSettings(); SetStage(Stage.Ready);
            Race.vehicle.GetComponent<VehicleAudio>()?.Silence();
        }
        public void ResetFeedback()
        {
            if (State == Stage.Racing) { ClearRecoveryFeedback(); }
        }
        public void LapCompleted()
        {
            if (State != Stage.Racing || Race.FreeRoam) return;
            if(string.IsNullOrEmpty(attempt)||Race.Progress.CompletedLaps<=0)return;
            string profile=Race.vehicle.GetComponent<VehicleConfiguration>().profileId;
            LapRank=Boards.CompletedLap(attempt,Race.Category,profile,Race.Progress);
            bool best = Save.RecordLap(Race.Progress.LastLap); NewLapRecord |= best;
            if (Race.Progress.Finished)
            {
                NewRaceRecord = Save.RecordRace(Race.Progress.AdjustedTime(Race.Clock));
                RaceRank=Boards.CompletedRace(attempt,Race.Category,profile,Race.Progress,Race.Clock);
                input.enabled = respawn.enabled = false;
                // Clear the finish with normal pedals/steering so following racers are not blocked.
                var runoff=Race.vehicle.GetComponent<RoadDriver>();
                if(!runoff) { runoff=Race.vehicle.gameObject.AddComponent<RoadDriver>(); runoff.Initialize(Race,Race.vehicle,true,1,1); }
                runoff.Racer=Race.Racers[0];
                if (Time.time < nextBuzz) finishAt = Time.unscaledTime + .3f; else Sound(finish);
                Notify("FINISHED — provisional standings; waiting up to 90s for opponents", 95);
                if ((NewLapRecord || NewRaceRecord) && finishAt == 0) feedback.PlayOneShot(record, Save.Settings.feedback * .18f);
            }
            else if (best) { Notify("NEW PERSONAL BEST LAP  " + RaceHud.FormatTime(Race.Progress.LastLap), 4); Sound(record); }
            else if(LapRank>0)Notify("TOP 10 LAP / #"+LapRank+"  "+RaceHud.FormatTime(Race.Progress.LastLap),4);
        }
        public void CompleteResults() { LockVehicle(true); SetStage(Stage.Results); }
        void Notify(string text, float duration) { Notice = text; noticeUntil = Time.unscaledTime + duration; }
        public void Click() => Sound(click);
#if UNITY_EDITOR || DEBUG
        public void UseValidationSave(string directory)
        {
            if (Race.Progress.Started) throw new System.InvalidOperationException("Validation storage must be selected before racing.");
            Save = new RacerSave(directory, "street-loop-gates-v1-laps" + Race.laps);
            Boards = new RecordBoards(directory);
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
