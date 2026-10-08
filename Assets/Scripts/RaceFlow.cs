using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Racer
{
    [DisallowMultipleComponent]
    public sealed class RaceFlow : MonoBehaviour
    {
        public enum Stage { Ready, Countdown, Racing, Paused, Results, Settings, Garage, Roster, Boards, Courses, Activities, Exploration, Title, Playlists, PlaylistVehicle }
        public RacePlaylists Playlists {get;private set;}
        public Stage State { get; private set; } = Stage.Ready;
        public RacerSave Save { get; private set; }
        public RaceDirector Race { get; private set; }
        public LocalRadio Radio { get; private set; }
        public RecordBoards Boards { get; private set; }
        public ArcadeActivities Activities {get;private set;}
        public CleanLapGhost Ghost {get;private set;}
        Stage extrasReturn;
        readonly System.Collections.Generic.Stack<Stage> callers=new();
        static bool returnToSetup;
        // 0.76: Free Roam has its own scene, built from Dan's Backyard Loop - Reverse. It is the same world whichever course
        // is selected; the course scenes are races only. RoamCourse is the course Free Roam was started from (the player
        // starts at its start location; Race Setup, Start Race and Return to Menu go back to its scene).
        public const string RoamScene="FreeRoamWorld";
        public static int RoamCourse;
        static bool pendingRace,roamHint;
        public bool InRoamWorld=>gameObject.scene.name==RoamScene;
        public bool RoamMenu { get; private set; }
        public float RoamMenuHintUntil { get; private set; }
        public void PushMenu(Stage stage){callers.Push(State);SetStage(stage);Click();}
        public void PopMenu(){SetStage(callers.Count>0?callers.Pop():Stage.Ready);Click();}
        public void RefreshMenu()=>menus.Show();
        public bool TrackBrowsingLocked=>!Race.FreeRoam&&(State==Stage.Racing||State==Stage.Countdown||State==Stage.Paused||callers.Contains(Stage.Paused)||callers.Contains(Stage.Racing)||callers.Contains(Stage.Countdown));
        public int LapRank { get; private set; }
        public int RaceRank { get; private set; }
        string attempt;
        public float CountdownRemaining { get; private set; }
        public string Notice { get; private set; }
        public bool NewLapRecord { get; private set; }
        public bool NewRaceRecord { get; private set; }
        public bool DebugMovementUsed { get; private set; }
        // 0.77: Trailer Mode also ends a race's record eligibility; the notice and results name what did it.
        public string DebugMovementReason { get; private set; } = "DEBUG RUN";
        public void MarkDebugMovement(string reason = "DEBUG RUN")
        {
            if(!Race.FreeRoam&&!TrackBrowsingLocked)return;
            if (DebugMovementUsed) return;
            DebugMovementUsed = true; DebugMovementReason = reason;
            if(Race.FreeRoam)return;
            Boards?.RevokeAttempt(); Save?.RevokeAttempt(); Ghost?.RejectDebugRun();
            LapRank = RaceRank = 0; NewLapRecord = NewRaceRecord = false;
            Notify(reason + " / competitive records disabled until restart", 8);
        }
        public bool MenuVisible => State != Stage.Countdown && State != Stage.Racing;
        Stage pausedStage, settingsReturn;
        VehicleInput input;
        VehicleRespawn respawn;
        RaceMenus menus;
        InputAction menu, back;
        public InputAction BackAction=>back;
        public InputAction PauseAction=>menu;
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
            if (InRoamWorld) ApplyRoamCourse();
            if (!Race.vehicle.GetComponent<VehicleAudio>()) Race.vehicle.gameObject.AddComponent<VehicleAudio>();
            originalKinematic = Race.vehicle.Body.isKinematic;
            string root = Path.Combine(Application.persistentDataPath, "Phase7", "street-loop-gates-v1-laps" + Race.laps);
            // Editor/standalone validation uses a separate directory, never the player's records.
            var args = System.Environment.GetCommandLineArgs();
            for (int i = 0; i + 1 < args.Length; i++) if (args[i] == "-racerTestSave") root = Path.GetFullPath(args[i + 1]);
#if UNITY_EDITOR
            if(!string.IsNullOrEmpty(ValidationSaveRoot))root=ValidationSaveRoot;
#endif
            Save = new RacerSave(root, "street-loop-gates-v1-laps" + Race.laps);
            VehicleUnlocks.Load(root); Campaign.Load(root); Campaign.Testing = Save.Settings.unlockEverything;
            VehicleVisual.NewModels = Save.Settings.newMotorcycle; RiderLook.Player = Save.Settings.rider;
            Scenery.Set(!Save.Settings.classicScenery); SceneryWorld.Attach(gameObject);
            Playlists=new RacePlaylists(root);
            Boards = new RecordBoards(root);
            // 0.94 Part A: a save from before names is Dan's (campaign, records or the welcome already seen): named "Dan", and
            // its Top 10 entries are his; a new player is asked with the welcome
            PlayerNames.Adopt(Save, Campaign.HasProgress || Boards.Any || (Save.Settings.hintsSeen ?? new string[0]).Length > 0); Boards.Owner(Save.Settings.playerName);
            Race.opponents = Save.Settings.opponents; Race.traffic = Save.Settings.traffic;
            Save.Settings.lastFiniteLaps=Mathf.Clamp(Save.Settings.lastFiniteLaps,1,5);
            Race.laps=Save.Settings.laps==0&&!Race.opponents?0:Mathf.Clamp(Save.Settings.laps==0?Save.Settings.lastFiniteLaps:Save.Settings.laps,1,5);
            Race.difficulty = Mathf.Clamp(Save.Settings.difficulty,0,2);
            var configuration = Race.vehicle.GetComponent<VehicleConfiguration>();
            if (!configuration) configuration = Race.vehicle.gameObject.AddComponent<VehicleConfiguration>();
            configuration.Apply(PlayerVehicleId);
            RestoreChoices();
            configuration.SetBodyColor(SelectedColor);
            Save.SelectRecords(Race.Category); Save.ApplySettings();
            var listener=FindAnyObjectByType<AudioListener>();if(listener&&!listener.GetComponent<AudioCeiling>())listener.gameObject.AddComponent<AudioCeiling>();
            feedback = gameObject.AddComponent<AudioSource>();
            feedback.playOnAwake = false; feedback.spatialBlend = 0; feedback.ignoreListenerPause = true;
            tick = Tone("Countdown", 520, .09f); go = Tone("Go", 880, .22f);
            finish = Tone("Finish", 660, .32f); record = Tone("Personal best", 1100, .22f); click = Tone("Menu", 380, .035f);
            ding = Tone("Checkpoint", 1040, .12f); buzz = Tone("Checkpoint missed", 145, .22f);
            menu = new InputAction("Pause", InputActionType.Button);
            menu.AddBinding("<Keyboard>/enter"); menu.AddBinding("<Keyboard>/escape"); menu.AddBinding("<Gamepad>/start"); menu.Enable();
            back = new InputAction("Back", InputActionType.Button);
            back.AddBinding("<Keyboard>/escape"); back.AddBinding("<Gamepad>/buttonEast"); back.Enable();
            menus = gameObject.AddComponent<RaceMenus>(); menus.Initialize(this);
            Activities=gameObject.AddComponent<ArcadeActivities>();Activities.Initialize(Race,root);
            CameraViews.Attach(this); TrailerMode.Attach(this); NameTags.Attach(this);
            Ghost=gameObject.AddComponent<CleanLapGhost>();Ghost.Initialize(Race,root);
            GetComponent<ExplorationCollection>()?.Initialize(Race,root);
            AcornBanner.Attach(this);UnlockNotice.Pending.Clear();UnlockNotice.Showing=null;UnlockNotice.Missed(Save); // 0.95 Part F
            GetComponent<ExplorationMap>()?.Initialize(Race,root);
            Hints.Flow=this;gameObject.AddComponent<HintWatch>().Initialize(this);
            LockVehicle(true);
            StartCoroutine(StartWhenBuilt());
        }
        // 0.82 Part B: the race, Free Roam, the title or the menu starts once the world is built (SceneryWorld builds it over
        // several frames behind the loading screen); the loading screen then shows the first frames and fades out.
        public bool Started { get; private set; }
        System.Collections.IEnumerator StartWhenBuilt()
        {
            var world=GetComponent<SceneryWorld>();float t0=Time.realtimeSinceStartup;
            while(world&&!world.Ready&&Time.realtimeSinceStartup-t0<60)yield return null;
            LoadingScreen.WorldReady();
            SmashAudio.Prepare();VehicleRespawn.LaunchSurfaces(gameObject.scene); // 0.82 Part C: first-use work behind the loading screen
            yield return null;
            if(SplitScreen.PendingStart&&SplitScreen.Active){SplitScreen.PendingStart=false;EnterMenuAfterTitle();SplitScreen.Configure(this);if(SplitScreen.Roaming&&InRoamWorld)StartFreeRoam();else StartRace();}
            else if(CampaignRun.PendingStart&&CampaignRun.Active!=null){CampaignRun.PendingStart=false;EnterMenuAfterTitle();CampaignRun.Configure(Race);Race.vehicle.GetComponent<VehicleConfiguration>().SetBodyColor(SelectedColor);StartRace();}
            else if(RacePlaylists.PendingStart){RacePlaylists.PendingStart=false;EnterMenuAfterTitle();Race.laps=RacePlaylists.Current.laps;StartRace();}
            else if(pendingRace){pendingRace=false;EnterMenuAfterTitle();StartRace();}
            else if(InRoamWorld){Radio=LocalRadio.Attach(this);StartFreeRoam();if(roamHint){roamHint=false;RoamMenuHintUntil=Time.unscaledTime+12;}}
            else if(StartupTitle.Begin(this))SetStage(Stage.Title);else EnterMenuAfterTitle();
            Started=true;LoadingScreen.Started();
        }
        // 0.82 Part B: every scene change goes through the loading screen: what is loading, the conditions and the vehicle
        void Go(string scene,bool roam=false)
        {
            Save.SaveSettings();Time.timeScale=1;AudioListener.pause=false;
            var (what,detail,course)=LoadingText(scene,roam);
            LoadingScreen.LoadScene(scene,what,detail,course);
        }
        (string what,string detail,CoursePreviewCatalog.Course course) LoadingText(string scene,bool roam)
        {
            int course=System.Array.IndexOf(RacePlaylists.Scenes,scene);var courses=CoursePreviewCatalog.Courses;
            string vehicle=VehicleProfile.Find(CampaignRun.Active!=null?CampaignRun.Vehicle:PlayerVehicleId).Name;
            string what=roam?"Free Roam":course>=0?RacePlaylists.Titles[course].Replace(" - "," — "):scene;
            string from=RacePlaylists.Titles[Mathf.Clamp(RoamCourse,0,RacePlaylists.Titles.Length-1)].Replace(" - "," — ");
            // 0.90 Part D: split-screen: both players' vehicles
            if(SplitScreen.Active&&!roam)return ("Split screen: "+(course>=0?RacePlaylists.Titles[course].Replace(" - "," — "):scene),$"Player 1: {VehicleProfile.Find(SplitScreen.P1Vehicle).Name}\nPlayer 2: {VehicleProfile.Find(SplitScreen.P2Vehicle).Name}{(SplitScreen.P2Ai?" (AI)":"")}\nDay · Clear · {SplitScreen.Laps} lap{(SplitScreen.Laps==1?"":"s")}",course>=0&&course<courses.Length?courses[course]:null);
            var campaign=CampaignRun.Active;if(campaign!=null&&!roam)return ("Campaign: "+campaign.Name,$"{RacePlaylists.Titles[campaign.Course].Replace(" - "," — ")}\n{campaign.Time} · {campaign.Weather}\n{vehicle}"+(Race.opponents?$"\nAgainst: {Race.RosterLabel}":""),course>=0&&course<courses.Length?courses[course]:null);
            string detail=roam?$"Starting at {from}\nWeather: {RoamWeatherLabel}\n{vehicle}":$"{TimeOfDayLabel} · {WeatherLabel}\n{vehicle}"+(Race.opponents?$"\nAgainst: {Race.RosterLabel}":"");
            return (what,detail,!roam&&course>=0&&course<courses.Length?courses[course]:null);
        }
        // FreeRoamWorld shows the selected course's name and vehicle rules and starts the player at that course's start.
        void ApplyRoamCourse()
        {
            RoamCourse=Mathf.Clamp(RoamCourse,0,RacePlaylists.Scenes.Length-1);
            Race.courseName=RacePlaylists.Titles[RoamCourse];
            var courses=CoursePreviewCatalog.Courses;if(RoamCourse>=courses.Length)return;var c=courses[RoamCourse];
            Race.forestOverride=c.forest?1:0;Race.carRuleScene=c.scene;
            var start=new GameObject("Free Roam start / "+c.scene).transform;start.SetPositionAndRotation(c.start,Quaternion.Euler(0,c.startYaw,0));respawn.spawnPoint=start;
        }
        void LoadScene(string scene)=>Go(scene,scene==RoamScene);
        // Leaving FreeRoamWorld for the selected course's scene (its menu page, or a race when pendingRace is set).
        void LeaveRoamWorld(string page)
        {
            WorldLook.Current?.SaveRoamClock();GetComponent<ExplorationMap>()?.Save();
            callers.Clear();menus.ResetPages();returnToSetup=page!=null;if(returnToSetup)menus.SetSceneReturn(page);
            // 0.89: Race Setup opens on a course the campaign has opened (Free Roam itself always has the whole world).
            LoadScene(RacePlaylists.Scenes[page=="race"&&!Campaign.CourseOpen(RoamCourse)?Campaign.FirstOpenCourse:RoamCourse]);
        }
        public void OpenRaceSetupFromRoam()=>LeaveRoamWorld("race");
        // 0.90 Part A: RACE on the main menu of a course the campaign has not opened (reached from Free Roam) goes to Race
        // Setup on an open course instead, so Race Setup never opens on something locked.
        public void OpenRaceSetupOnOpenCourse(){callers.Clear();menus.ResetPages();menus.SetSceneReturn("race");returnToSetup=true;LoadScene(RacePlaylists.Scenes[Campaign.FirstOpenCourse]);}
        public void EnterMenuAfterTitle(){Radio=LocalRadio.Attach(this);SetStage(Stage.Ready);if(returnToSetup){returnToSetup=false;menus.RestoreSceneReturn();}}
        // 0.90 Part B: a new player gets the welcome panel after the title (Start the campaign / Look around first) instead
        // of going straight into Free Roam.
        public void EnterFreeRoamAfterTitle(){if(Hints.WelcomeDue){EnterMenuAfterTitle();menus.OpenWelcome();return;}Radio=LocalRadio.Attach(this);roamHint=!InRoamWorld;StartFreeRoam();RoamMenuHintUntil=Time.unscaledTime+12;}
        void Update()
        {
            if (Save == null || !Started || LoadingScreen.Holding || State==Stage.Title || menus?.OwnsTextInput==true || DeveloperLocationHud.OwnsInput) return;
            if(GetComponent<ExplorationMap>()?.OwnsInput==true)return;
            if (finishAt > 0 && State != Stage.Paused && State != Stage.Settings && Time.unscaledTime >= finishAt) { finishAt = 0; Sound(finish); }
            if (MenuInput.Blocked) return;
            // 0.91 Part B: a paused split-screen race: Back / Start count from either player's device (no other); the setup
            // screen's Back only from player 1's
            if (SplitScreen.Race && (State == Stage.Paused || State == Stage.Settings) && ((back.WasPressedThisFrame() && !SplitScreen.PlayerDevice(back.activeControl?.device)) || (menu.WasPressedThisFrame() && !SplitScreen.PlayerDevice(menu.activeControl?.device)))) return;
            if (State == Stage.Ready && menus.SplitSetupOpen && SplitScreen.P1Device != null && back.WasPressedThisFrame() && back.activeControl?.device != SplitScreen.P1Device && !(SplitScreen.P1Device is Keyboard && back.activeControl?.device is Mouse)) return;
            if (MenuVisible && back.WasPressedThisFrame()) { MenuInput.ConsumeThroughRelease(back); Back(); return; }
            // 0.90 Part B: the controls card before a new player's first event holds the countdown until one press
            if (ControlsCard)
            {
                if (Time.unscaledTime - controlsShownAt > .4f && AnyPress()) { ControlsCard = false; Hints.MarkSeen("controls"); MenuInput.ConsumeThroughRelease(); Sound(tick); }
                return;
            }
            if (menu.WasPressedThisFrame() && !menus.ModalOpen)
            {
                if (State == Stage.Racing || State == Stage.Countdown) { MenuInput.ConsumeThroughRelease(menu); Pause(); }
                else if (menus.ResumeRoot) { MenuInput.ConsumeThroughRelease(menu); Resume(); }

            }
            else if (back.WasPressedThisFrame()) { MenuInput.ConsumeThroughRelease(back); Back(); }
            if (State == Stage.Countdown)
            {
                CountdownRemaining = Mathf.Max(0, CountdownRemaining - Time.deltaTime);
                int number = Mathf.CeilToInt(CountdownRemaining);
                if (number > 0 && number != lastTick) { lastTick = number; Sound(tick); }
                if (CountdownRemaining <= 0)
                {
                    LockVehicle(false); Race.ResetSampling(Race.vehicle.Body.position, Time.timeAsDouble);
                    foreach (var racer in Race.Racers) racer.Progress.BeginTiming(Time.timeAsDouble); SetStage(Stage.Racing); Notify(GoNotice, 3); Sound(go);
                }
            }
            if (Notice != null && Time.unscaledTime > noticeUntil) Notice = null;
        }
        void SetStage(Stage stage)
        {
            MenuInput.ConsumeThroughRelease();
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
        // 0.79: when the current race or Free Roam session began (the HUD shows its brief hints from there)
        public float SessionStartedAt { get; private set; } = -1;
        public void PrepareRestart()
        {
            SessionStartedAt = Time.unscaledTime;
            Activities?.NewSession();
            Ghost?.ResetSession();
            var runoff=Race.vehicle.GetComponent<RoadDriver>();
            if(runoff) { runoff.enabled=false; Destroy(runoff); }
            LockVehicle(false); Race.vehicle.enabled=true;
            Notice = null; nextBuzz = finishAt = penaltyUntil = 0; pendingMisses=0; CheckpointDings = CheckpointBuzzes = 0; feedback.Stop();
        }
        public void SelectRecords(string category)
        {
            Save.SelectRecords(category); menus?.Show();
        }
        public string LapLabel => Race.laps==0?"Unlimited":Race.laps.ToString();
        // 0.92 Part C: the cycles step either way (left / right on their rows)
        public void CycleLaps(int d=1){Race.laps=(Race.laps+d+6)%6;if(Race.opponents&&Race.laps==0)Race.laps=d>0?1:5;Save.Settings.laps=Race.laps;if(Race.laps>0)Save.Settings.lastFiniteLaps=Race.laps;Save.SaveSettings();SelectRecords(Race.Category);Click();}
        public void ToggleOpponents() { Race.opponents = !Race.opponents; if(Race.opponents&&Race.laps==0){Race.laps=Save.Settings.lastFiniteLaps;Save.Settings.laps=Race.laps;Notify("AI race: restored "+Race.laps+" finite laps",4);} Save.Settings.opponents = Race.opponents; Save.SaveSettings(); SelectRecords(Race.Category); Click(); }
        public void ToggleTraffic() { Race.traffic = !Race.traffic; Save.Settings.traffic = Race.traffic; Save.SaveSettings(); SelectRecords(Race.Category); Click(); }
        string GoNotice=>CampaignRun.Active==null?"GO!  Shared race clock started":CampaignRun.Active.Kind==CampaignEventKind.SpeedTrap?"GO!  Hit the speed trap ahead as fast as you can":CampaignRun.Active.Kind==CampaignEventKind.Jump?"GO!  Land the jump ahead as far as you can":CampaignRun.Active.Kind==CampaignEventKind.Smash?"GO!  Smash as many fence-line props as you can":CampaignRun.Active.Kind==CampaignEventKind.TimeTrial?"GO!  Flying lap: the clock starts at the START line":"GO!  "+CampaignRun.Active.Name;
        public string TimeOfDayLabel => ((TimeOfDay)Mathf.Clamp(Save.Settings.timeOfDay,0,3)).ToString();
        public string WeatherLabel => ((Weather)Mathf.Clamp(Save.Settings.weather,0,2)).ToString();
        public string RoamWeatherLabel => ((Weather)Mathf.Clamp(Save.Settings.roamWeather,0,2)).ToString();
        // 0.74: Dawn / Day / Dusk / Night (the saved value of Dawn is 3, so older saves keep their choice).
        public void CycleTimeOfDay(int d=1) { var order=LookPresets.MenuOrder; int i=System.Array.IndexOf(order,(TimeOfDay)Mathf.Clamp(Save.Settings.timeOfDay,0,3)); Save.Settings.timeOfDay=(int)order[(i+d+order.Length)%order.Length]; Save.SaveSettings(); menus?.Show(); Click(); }
        public void CycleWeather(int d=1) { Save.Settings.weather=(Mathf.Clamp(Save.Settings.weather,0,2)+d+3)%3; Save.SaveSettings(); menus?.Show(); Click(); }
        public void CycleRoamWeather(int d=1) { Save.Settings.roamWeather=(Mathf.Clamp(Save.Settings.roamWeather,0,2)+d+3)%3; Save.SaveSettings(); menus?.Show(); Click(); }
        public void CycleDifficulty(int d=1) { if(State!=Stage.Ready && State!=Stage.Results) return; Race.difficulty=(Race.difficulty+d+3)%3; Save.Settings.difficulty=Race.difficulty; Save.SaveSettings(); SelectRecords(Race.Category); Click(); }
        public void OpenGarage() { if(State!=Stage.Ready && State!=Stage.Results) return; PushMenu(Stage.Garage); }
        public void CloseGarage() { PopMenu(); }
        public void OpenRoster() { if(State!=Stage.Ready && State!=Stage.Results) return; PushMenu(Stage.Roster); }
        public void OpenBoards() { PushMenu(Stage.Boards); }
        public void OpenActivities(){PushMenu(Stage.Activities);}
        public void OpenExploration(){PushMenu(Stage.Exploration);}
        public void CloseExtras(){PopMenu();}
        public void ToggleGhost(){Ghost.Toggle();menus.Show();Click();}
        public void OpenCourses() { PushMenu(Stage.Courses); }
        public void SelectCourseEntry(int course)
        {
            if(State!=Stage.Courses || course<0 || course>=RacePlaylists.Scenes.Length)return;
            if(!menus.RoamTrackPick&&!Campaign.CourseOpen(course))return; // 0.89: Race picks only courses the campaign has opened
            menus.SaveSceneReturn();returnToSetup=true;Go(RacePlaylists.Scenes[course]);
        }
        public void SelectMountain(bool reverse){if(State!=Stage.Courses)return;Go(reverse?"MountainLoopReverse":"MountainLoop");}
        public void SelectBackyardForward(){if(State!=Stage.Courses)return;Go("DansBackyardForward");}
        public readonly FinishPresentation FinishCards=new();
        public string FinishSummary=>DebugMovementUsed?DebugMovementReason+" / competitive records disabled":FinishCards.Summary;
        public bool SetupFromResults=>callers.Count>0&&callers.Peek()==Stage.Results;
        public void OpenResultsSetup(){PushMenu(Stage.Ready);menus.OpenSetup();}
        public void OpenPlaylists(){PushMenu(Stage.Playlists);}
        public void StartPlaylist(RacePlaylists.Definition definition){if(definition.entries.Count==0){Notify("Add a race first",4);return;}var shut=definition.entries.FirstOrDefault(e=>e.course<RacePlaylists.Scenes.Length&&!Campaign.CourseOpen(e.course));if(shut!=null){Notify("LOCKED: "+RacePlaylists.Titles[shut.course]+" — "+Campaign.CourseHowTo(shut.course),6);return;}if(definition.entries.Exists(e=>e.course>=RacePlaylists.Scenes.Length)){Notify("Remove the rolled-back course from this playlist",4);return;}RacePlaylists.Begin(definition);LoadPlaylistEntry();}
        public void NextPlaylistRace(){if(State!=Stage.Results||RacePlaylists.PendingStart||!RacePlaylists.HasNext||RacePlaylists.Championship.Events[RacePlaylists.Position]==null)return;RacePlaylists.Advance();LoadPlaylistEntry();}
        void LoadPlaylistEntry(){var entry=RacePlaylists.Current;if(!RacePlaylists.Eligible(entry,Save.Settings.vehicleId)||(Race.opponents&&System.Array.Exists(Save.Settings.opponentRoster,id=>!RacePlaylists.Eligible(entry,id)))){SetStage(Stage.PlaylistVehicle);return;}RacePlaylists.PendingStart=true;Go(RacePlaylists.Scenes[entry.course]);}
        public void ChoosePlaylistVehicle(string id){if(State!=Stage.PlaylistVehicle||!RacePlaylists.Eligible(RacePlaylists.Current,id))return;Save.Settings.vehicleId=id;for(int i=0;i<Save.Settings.opponentRoster.Length;i++)if(!RacePlaylists.Eligible(RacePlaylists.Current,Save.Settings.opponentRoster[i]))Save.Settings.opponentRoster[i]=id;LoadPlaylistEntry();}
        public void CancelPlaylist(){callers.Clear();menus.ResetPages();RacePlaylists.Quit();PrepareRestart();Race.AbandonEvent();LockVehicle(true);Race.laps=Save.Settings.laps==0&&!Race.opponents?0:Mathf.Clamp(Save.Settings.laps,1,5);SetStage(Stage.Ready);}
        public void SelectCourse(bool lake)=>SelectCourse(lake,false);
        public void SelectCourse(bool lake,bool reverse)
        {
            if(State!=Stage.Courses)return;
            Go(reverse?(lake?"ForestLoopReverse":"StreetLoopReverse"):(lake?"LakeWoods":"StreetLoopGreybox"));
        }
        // 0.89: the colour of the vehicle being driven (the saved choice may be a vehicle the campaign has not given yet).
        public int SelectedColor => Save.Settings.bodyColors[Mathf.Max(0,VehicleProfile.IndexOf(Race.vehicle.GetComponent<VehicleConfiguration>().profileId))];
        // The player's vehicle: the saved choice when the campaign (or Testing) allows it, else the first one allowed.
        public string PlayerVehicleId => Race.PlayerVehicle(Save.Settings.vehicleId);
        void RestoreChoices()
        {
            Save.Settings.vehicleId=Race.EligibleVehicle(Save.Settings.vehicleId);
            // 0.81: one colour per vehicle; a save from before the new vehicles keeps its colours and gets defaults for the rest
            var colors=Save.Settings.bodyColors??new int[0];
            if(colors.Length!=VehicleProfile.All.Length){var grown=new int[VehicleProfile.All.Length];for(int c=0;c<grown.Length;c++)grown[c]=c<colors.Length?colors[c]:-1;Save.Settings.bodyColors=grown;}
            if(Save.Settings.opponentChoices==null || Save.Settings.opponentChoices.Length!=3) Save.Settings.opponentChoices=new[]{"mixed","mixed","mixed"};
            if(Save.Settings.opponentRoster==null || Save.Settings.opponentRoster.Length!=3) Save.Settings.opponentRoster=new[]{"tourer","moto","atv"};
            Race.opponentRoster=(string[])Save.Settings.opponentRoster.Clone();
            for(int i=0;i<3;i++) Race.opponentRoster[i]=Race.EligibleVehicle(Race.opponentRoster[i]);
        }
        public void SetColor(int color)
        {
            if(State!=Stage.Garage) return;
            int i=Mathf.Max(0,VehicleProfile.IndexOf(Race.vehicle.GetComponent<VehicleConfiguration>().profileId));
            Save.Settings.bodyColors[i]=Mathf.Clamp(color,0,VehiclePaint.Count-1);
            Race.vehicle.GetComponent<VehicleConfiguration>().SetBodyColor(SelectedColor);
            Save.SaveSettings(); menus.Show(); Click();
        }
        public void CycleOpponent(int slot,int d=1)
        {
            if(State!=Stage.Roster || slot<0 || slot>=3) return;
            var choices=System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Concat(System.Linq.Enumerable.Select(Race.EligibleVehicles,p=>p.Id),new[]{"random","mixed"}));
            int i=System.Array.IndexOf(choices,Save.Settings.opponentChoices[slot]);
            Save.Settings.opponentChoices[slot]=choices[(i+d+choices.Length)%choices.Length]; ResolveRoster();
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
                // 0.88: once earned, the riding mower turns up in Random / Mixed fields only now and then
                if(candidates.Count>1&&candidates.Contains("mower")&&Random.value<.75f) candidates.Remove("mower");
                Race.opponentRoster[i]=candidates[Random.Range(0,candidates.Count)]; used.Add(Race.opponentRoster[i]);
            }
            Save.Settings.opponentRoster=(string[])Race.opponentRoster.Clone(); Save.SaveSettings(); SelectRecords(Race.Category); Click();
        }
        public void SelectVehicle(string id)
        {
            if(State!=Stage.Garage) return;
            if(Campaign.VehicleLocked(VehicleProfile.Find(id))) return;
            id=Race.PlayerVehicle(id);
            Race.vehicle.GetComponent<VehicleConfiguration>().Apply(id);
            Save.Settings.vehicleId=VehicleProfile.Find(id).Id; Save.SaveSettings(); SelectRecords(Race.Category); Click();
            Race.vehicle.GetComponent<VehicleConfiguration>().SetBodyColor(SelectedColor); menus.Show();
        }
        // Garage Model Classic / New (0.73 motorcycle; 0.75 every vehicle): one setting, remembered; the AI vehicles follow
        // it at the next race start. Ambient traffic always keeps the classic cars.
        public string ModelLabel => Save.Settings.newMotorcycle ? "New" : "Classic";
        public void ToggleModel()
        {
            if(State!=Stage.Garage) return;
            Save.Settings.newMotorcycle=!Save.Settings.newMotorcycle; VehicleVisual.NewModels=Save.Settings.newMotorcycle;
            RebuildPlayerVisual(); Save.SaveSettings(); Click(); menus.Show();
        }
        // 0.75 garage Rider page (New models only): each row steps one option; Randomize picks a whole new look. Saved at
        // once; the AI riders are drawn at random for each race (RiderLook.Field).
        public RiderLook Rider => Save.Settings.rider;
        public void StepRider(int field, int direction)
        {
            if(State!=Stage.Garage || !VehicleVisual.NewModels) return;
            Save.Settings.rider.Step(field, direction); RiderChanged();
        }
        public void RandomizeRider()
        {
            if(State!=Stage.Garage || !VehicleVisual.NewModels) return;
            var current=Save.Settings.rider; RiderLook look; var random=new System.Random();
            do look=RiderLook.Random(random); while(look.SameAs(current));
            Save.Settings.rider=look; RiderChanged();
        }
        void RiderChanged()
        {
            RiderLook.Player=Save.Settings.rider; RebuildPlayerVisual(); Save.SaveSettings(); Click(); menus.Show();
        }
        void RebuildPlayerVisual()
        {
            var configuration=Race.vehicle.GetComponent<VehicleConfiguration>(); configuration.Apply(configuration.profileId); configuration.SetBodyColor(SelectedColor);
        }
        public void WipeoutFeedback() { if(State==Stage.Racing) Notify("R / Y: right vehicle locally",2); }
        public void ClearRecoveryFeedback() { if(Notice=="R / Y: right vehicle locally") Notice=null; }
        public void CheckpointFeedback(bool accepted, double seconds, int count)
        {
            if (State != Stage.Racing) return;
            if (accepted) { if (!Race.Progress.Finished) { CheckpointDings++; Sound(ding); } }
            else { if(Time.unscaledTime>=penaltyUntil) pendingMisses=0; pendingMisses+=count; penaltyUntil=Time.unscaledTime+5; if (Time.time >= nextBuzz) { CheckpointBuzzes++; Sound(buzz); nextBuzz = Time.time + .5f; } }
        }
        public bool ControlsCard { get; private set; }
        float controlsShownAt;
        static bool AnyPress()
        {
            if (Mouse.current?.leftButton.wasPressedThisFrame == true) return true;
            foreach (var k in InputSystem.devices) if (k is Keyboard kb && kb.anyKey.wasPressedThisFrame) return true;
            foreach (var pad in Gamepad.all) if (pad.buttonSouth.wasPressedThisFrame || pad.buttonEast.wasPressedThisFrame || pad.buttonNorth.wasPressedThisFrame || pad.buttonWest.wasPressedThisFrame || pad.startButton.wasPressedThisFrame) return true;
            return false;
        }
        public void BeginCountdown()
        {
            ControlsCard = Hints.ControlsDue; controlsShownAt = Time.unscaledTime;
            DebugMovementUsed=false; Save.BeginAttempt();
            attempt=System.Guid.NewGuid().ToString("N");LapRank=RaceRank=0;Boards.BeginAttempt();FinishCards.Begin(Boards,Race.Category,Save.Best);
            NewLapRecord = NewRaceRecord = false; CountdownRemaining = 3; lastTick = 3;
            if(CampaignRun.Active?.Kind==CampaignEventKind.SpeedTrap||CampaignRun.Active?.Kind==CampaignEventKind.Jump||CampaignRun.Active?.Kind==CampaignEventKind.Smash){var watch=GetComponent<CampaignTrapWatch>();if(!watch)watch=gameObject.AddComponent<CampaignTrapWatch>();watch.Initialize(this);}
            Notice = null; LockVehicle(true); SetStage(Stage.Countdown); Sound(tick);
        }
        public void StartRace() { if(!CampaignAllows())return; if(InRoamWorld){pendingRace=true;LeaveRoamWorld(null);return;} SessionStartedAt=Time.unscaledTime;RoamMenu=false;callers.Clear();menus.ResetPages(); if(RacePlaylists.Active!=null)RacePlaylists.Championship.Restart(RacePlaylists.Position);Race.FreeRoam=false;SetGateVisibility(true);Click(); if(LoadingScreen.Holding)Race.RestartRace();else StartCoroutine(RestartBehindLoadingScreen()); }
        // 0.84 Part G: building the rivals and traffic for a race takes about half a second, then the first frame with them
        // is slow too; START RACE used to freeze the menu for that time. The loading screen now comes up at once and the
        // race is built behind it (the countdown waits for it, as after a scene load).
        bool restarting;
        System.Collections.IEnumerator RestartBehindLoadingScreen()
        {
            if(restarting)yield break;restarting=true;
            var (what,detail,course)=LoadingText(gameObject.scene.name,false);
            LoadingScreen.Cover(what,detail,course);
            yield return null;yield return null; // the screen is drawn before the work starts
            restarting=false;Race.RestartRace();LoadingScreen.Started();
        }
        public void StartFreeRoam(){int course=System.Array.IndexOf(RacePlaylists.Scenes,gameObject.scene.name);if(!InRoamWorld&&course>=0){RoamCourse=course;LoadScene(RoamScene);return;}SessionStartedAt=Time.unscaledTime;RoamMenu=false;callers.Clear();menus.ResetPages();Race.FreeRoam=true;SetGateVisibility(false);Click();Race.RestartRace();}
        public void BeginRoaming(){Hints.FreeRoam();DebugMovementUsed=false;attempt=null;CountdownRemaining=0;LapRank=RaceRank=0;NewLapRecord=NewRaceRecord=false;LockVehicle(false);Race.GetComponent<WrongWayGuidance>()?.Clear();SetStage(Stage.Racing);}
        void SetGateVisibility(bool visible){foreach(var gate in Race.gates)foreach(var renderer in gate.GetComponentsInChildren<Renderer>(true))renderer.enabled=visible;}
        // 0.90 Part D: it cannot be resumed while a player's controller is missing. 0.91 Part B: the pause menu answers
        // either player's device (it names the one that paused).
        public void Pause() { if(SplitScreen.Race){var by=menu.activeControl?.device;SplitScreen.Race.PausedBy=SplitScreen.Race.MissingPlayer==1?SplitScreen.P2Device:SplitScreen.Race.MissingPlayer==2?SplitScreen.P1Device:by!=null&&menu.WasPressedThisFrame()?by:SplitScreen.P1Device;menus.RestrictMenuDevices(SplitScreen.P1Device,SplitScreen.P2Ai?null:SplitScreen.P2Device);}
            pausedStage = State; RoamMenuHintUntil=0;RoamMenu=Race.FreeRoam&&!SplitScreen.Active;if(RoamMenu)menus.ResetPages();SetStage(RoamMenu?Stage.Ready:Stage.Paused); Click(); }
        public void Resume() { if(SplitScreen.Race&&SplitScreen.Race.MissingPlayer!=0)return; if(SplitScreen.Race){SplitScreen.Race.PausedBy=null;menus.RestrictMenuDevices(null);} RoamMenu=false;SetStage(pausedStage); Click(); }
        public void OpenSettings() { PushMenu(Stage.Settings); }
        public void CloseSettings() { Save.SaveSettings(); PopMenu(); }
        public void Back() { if(State==Stage.Results){QuitRace();return;} if(menus.BackPage())return; if(State==Stage.PlaylistVehicle)CancelPlaylist();else if(State==Stage.Paused||(State==Stage.Ready&&RoamMenu))Resume();else if(State!=Stage.Ready&&MenuVisible)PopMenu(); }

        // 0.89: outside the campaign, Race uses only courses the campaign has opened and the player's own vehicles (stock).
        bool CampaignAllows()
        {
            if(CampaignRun.Active!=null||SplitScreen.Active)return true; // 0.90: split-screen has everything unlocked
            int course=InRoamWorld?RoamCourse:System.Array.IndexOf(RacePlaylists.Scenes,gameObject.scene.name);
            if(course>=0&&!Campaign.CourseOpen(course)){Notify("LOCKED: "+RacePlaylists.Titles[course]+" — "+Campaign.CourseHowTo(course),6);menus.Show();return false;}
            var configuration=Race.vehicle.GetComponent<VehicleConfiguration>();string allowed=Race.PlayerVehicle(configuration.profileId);
            if(allowed!=configuration.profileId){configuration.Apply(allowed);configuration.SetBodyColor(SelectedColor);}
            return true;
        }
        // 0.89: a campaign event, through the loading screen into its course (from any scene, Free Roam included).
        public void StartCampaignEvent(CampaignEvent e,string vehicle)
        {
            if(e==null||!Campaign.Available(e)&&!Campaign.Testing)return;
            if(InRoamWorld){WorldLook.Current?.SaveRoamClock();GetComponent<ExplorationMap>()?.Save();}
            CampaignRun.Begin(e,vehicle);callers.Clear();menus.ResetPages();Click();Go(RacePlaylists.Scenes[e.Course]);
        }
        public void RetryCampaignEvent(){if(CampaignRun.Active==null||CampaignRun.Cup!=null||State!=Stage.Results)return;CampaignRun.Retry();StartRace();}
        // 0.90: a championship: its next round (the first after Start / Restart), through the loading screen into its course.
        public void StartCupRound(CampaignCup cup)
        {
            if(cup==null||Campaign.Testing)return;
            if(InRoamWorld){WorldLook.Current?.SaveRoamClock();GetComponent<ExplorationMap>()?.Save();}
            if(!CampaignRun.BeginCupRound(cup))return;
            callers.Clear();menus.ResetPages();Click();Go(RacePlaylists.Scenes[CampaignRun.Active.Course]);
        }
        public void NextCupRace(){var cup=CampaignRun.Cup;if(cup==null||State!=Stage.Results||Campaign.ActiveCup(cup)==null)return;CampaignRun.End();StartCupRound(cup);}
        // Back to normal Race settings (the event's were never saved) and the campaign screen.
        void EndCampaignEvent()
        {
            int chapter=CampaignRun.Cup!=null?CampaignRun.Cup.AfterChapter:CampaignRun.Active?.Chapter??0;CampaignRun.End();
            Race.opponents=Save.Settings.opponents;Race.traffic=Save.Settings.traffic;Race.difficulty=Mathf.Clamp(Save.Settings.difficulty,0,2);
            Race.laps=Save.Settings.laps==0&&!Race.opponents?0:Mathf.Clamp(Save.Settings.laps==0?Save.Settings.lastFiniteLaps:Save.Settings.laps,1,5);
            // 0.90: always re-applied, so the campaign's upgrades never stay on the vehicle outside the campaign
            RestoreChoices();var configuration=Race.vehicle.GetComponent<VehicleConfiguration>();
            configuration.Apply(PlayerVehicleId);configuration.SetBodyColor(SelectedColor);
            SelectRecords(Race.Category);menus.OpenCampaign(chapter);
        }
        // 0.90 Part D: start a split-screen race (through the loading screen into its course) and leave one (back to the
        // menu, or to the setup screen to change it). Race's own settings were never changed and are put back.
        public void StartSplit()
        {
            if(!SplitScreen.Ready)return;
            if(InRoamWorld){WorldLook.Current?.SaveRoamClock();GetComponent<ExplorationMap>()?.Save();}
            RacePlaylists.Quit();CampaignRun.End();SplitScreen.Begin();callers.Clear();menus.ResetPages();Click();
            // 0.94 Parts B and C: Free Roam for two and Police Chase start at the chosen course's start in FreeRoamWorld
            if(SplitScreen.Mode!=SplitScreen.Kind.Race){RoamCourse=SplitScreen.Course;Go(RoamScene,true);return;}
            Go(RacePlaylists.Scenes[SplitScreen.Course]);
        }
        public void QuitSplit(bool setup)
        {
            if(!SplitScreen.Active)return;
            // 0.95 Part B: Change setup goes to the Police Chase setup for a chase; a chase started from inside Free Roam goes back to it
            if(InRoamWorld){callers.Clear();menus.ResetPages();bool police=SplitScreen.Mode==SplitScreen.Kind.Police,roam=police&&SplitScreen.FromRoam&&!setup;EndSplit();
                if(roam){SplitScreen.FromRoam=false;RoamCourse=SplitScreen.RoamReturn;LoadScene(RoamScene);return;}
                LeaveRoamWorld(setup?(police?"police":"split"):"");return;}
            QuitRace();if(setup)menus.OpenSplitSetup(true);
        }
        void EndSplit()
        {
            SplitScreen.End();menus.RestrictMenuDevices(null);
            Race.opponents=Save.Settings.opponents;Race.traffic=Save.Settings.traffic;Race.difficulty=Mathf.Clamp(Save.Settings.difficulty,0,2);
            Race.laps=Save.Settings.laps==0&&!Race.opponents?0:Mathf.Clamp(Save.Settings.laps==0?Save.Settings.lastFiniteLaps:Save.Settings.laps,1,5);
            RestoreChoices();var configuration=Race.vehicle.GetComponent<VehicleConfiguration>();configuration.Apply(PlayerVehicleId);configuration.SetBodyColor(SelectedColor);
            SelectRecords(Race.Category);
        }
        public void QuitRace()
        {
            callers.Clear();menus.ResetPages();
            if(State!=Stage.Paused && State!=Stage.Results && State!=Stage.Settings) return;
            if(InRoamWorld){if(SplitScreen.Active)EndSplit();LeaveRoamWorld("");return;}
            // 0.90: leaving a championship round before its result counts as did not finish (no retries in a championship)
            // (when that was the last round, its final standings and payout are still shown)
            if(CampaignRun.Cup!=null&&!CampaignRun.Done&&State!=Stage.Results){var forfeit=CampaignRun.Forfeit(this);if(forfeit!=null&&forfeit.CupFinished){LockVehicle(true);SetStage(Stage.Results);return;}}
            PrepareRestart(); Race.AbandonEvent(); respawn.CancelRecovery(); respawn.PlaceOnNearestGround(); LockVehicle(true);
            RacePlaylists.Quit();Race.laps=Save.Settings.laps==0&&!Race.opponents?0:Mathf.Clamp(Save.Settings.laps,1,5);
            Race.FreeRoam=false;SetGateVisibility(true);
            NewLapRecord=NewRaceRecord=false; Save.SaveSettings(); SetStage(Stage.Ready);
            Race.vehicle.GetComponent<VehicleAudio>()?.Silence();
            if(SplitScreen.Active)EndSplit();
            if(CampaignRun.Active!=null)EndCampaignEvent();
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
            // 0.89: campaign events never write the Top 10 boards or personal bests (they keep their own in the campaign save)
            bool records=!DebugMovementUsed&&CampaignRun.Active==null&&!SplitScreen.Active; // 0.90: nor split-screen
            LapRank=records?Boards.CompletedLap(attempt,Race.Category,profile,Race.Progress):0;
            bool best = records && Save.RecordLap(Race.Progress.LastLap); NewLapRecord |= best;
            if (Race.Progress.Finished)
            {
                NewRaceRecord = records && Save.RecordRace(Race.Progress.AdjustedTime(Race.Clock));
                RaceRank=records?Boards.CompletedRace(attempt,Race.Category,profile,Race.Progress,Race.Clock):0;
                if(records)FinishCards.Finish(Boards,Race.Category,attempt,Race.Progress,Race.Clock);
                input.enabled = respawn.enabled = false;
                // Clear the finish with normal pedals/steering so following racers are not blocked.
                var runoff=Race.vehicle.GetComponent<RoadDriver>();
                if(!runoff) { runoff=Race.vehicle.gameObject.AddComponent<RoadDriver>(); runoff.Initialize(Race,Race.vehicle,true,1,1); }
                runoff.Racer=Race.Racers[0];
                if (Time.time < nextBuzz) finishAt = Time.unscaledTime + .3f; else Sound(finish);
                Notify("FINISHED — provisional standings; waiting up to 90s for opponents", 95);
                // 0.82 Part E: the winner on camera for 2.5 s before the finish panel (player or the AI who won)
                if(Race.Racers.Count>1&&!SplitScreen.Active){RacerState won=null;foreach(var r in Race.Racers)if(r.Progress.Finished&&(won==null||r.Progress.RaceTime(Race.Clock)<won.Progress.RaceTime(Race.Clock)))won=r;WinnerShot.Begin(this,won,won==Race.Racers[0]);}
                if ((NewLapRecord || NewRaceRecord) && finishAt == 0) feedback.PlayOneShot(record, Save.Settings.feedback * .18f);
            }
            else if (best) { Notify("NEW PERSONAL BEST LAP  " + RaceHud.FormatTime(Race.Progress.LastLap), 4); Sound(record); }
            else if(LapRank>0)Notify("TOP 10 LAP / #"+LapRank+"  "+RaceHud.FormatTime(Race.Progress.LastLap),4);
        }
        public void CompleteResults() { if(WinnerShot.Active){StartCoroutine(ResultsAfterWinnerShot());return;} if(!DebugMovementUsed)RacePlaylists.Record(Race);if(CampaignRun.Active!=null)CampaignRun.Finish(this,GetComponent<CampaignTrapWatch>()?.Best??0);LockVehicle(true); SetStage(Stage.Results); }
        // 0.91 Part C: pause menu > End event in a campaign jump event (its best scored jump counts).
        public CampaignTrapWatch JumpEvent=>CampaignRun.Active?.Kind==CampaignEventKind.Jump&&!CampaignRun.Done?GetComponent<CampaignTrapWatch>():null;
        public void EndJumpEvent(){var watch=JumpEvent;if(watch)watch.EndNow();}
        System.Collections.IEnumerator ResultsAfterWinnerShot(){while(WinnerShot.Active)yield return null;CompleteResults();}
        public void Notify(string text, float duration) { Notice = text; noticeUntil = Time.unscaledTime + duration; }
        public void Click() => Sound(click);
#if UNITY_EDITOR
        public static string ValidationSaveRoot;
#endif
#if UNITY_EDITOR || DEBUG
        public void UseValidationSave(string directory)
        {
            if (Race.Progress.Started) throw new System.InvalidOperationException("Validation storage must be selected before racing.");
#if UNITY_EDITOR
            ValidationSaveRoot=directory;
#endif
            Save = new RacerSave(directory, "street-loop-gates-v1-laps" + Race.laps);
            VehicleUnlocks.Load(directory); Campaign.Load(directory); Campaign.Testing = Save.Settings.unlockEverything;
            VehicleVisual.NewModels = Save.Settings.newMotorcycle; RiderLook.Player = Save.Settings.rider;
            Scenery.Set(!Save.Settings.classicScenery);
            Boards = new RecordBoards(directory);
            PlayerNames.Adopt(Save, Campaign.HasProgress || Boards.Any || (Save.Settings.hintsSeen ?? new string[0]).Length > 0); Boards.Owner(Save.Settings.playerName);
            UnlockNotice.Pending.Clear(); UnlockNotice.Showing = null; UnlockNotice.Missed(Save); // 0.95 Part F: as at load, for this save
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
