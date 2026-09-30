#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Racer
{
    // Explicitly launched, isolated-save acceptance fixture. Never runs in a player build.
    public sealed class UIFoundationChecks : MonoBehaviour
    {
        const string Output="Docs/UI/Phase12";
        readonly List<string> checks=new();
        RaceFlow flow;RaceMenus menus;Gamepad pad;Keyboard keyboard;bool followup;
        InputSettings.BackgroundBehavior oldBackground;InputSettings.EditorInputBehaviorInPlayMode oldEditorInput;
        static readonly BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
        public static void Launch(bool followup=false){var go=new GameObject("Phase 1+2 focused checks");DontDestroyOnLoad(go);go.AddComponent<UIFoundationChecks>().followup=followup;}
        void Check(bool good,string label){checks.Add((good?"PASS ":"FAIL ")+label);File.WriteAllLines(Output+(followup?"/followup.txt":"/checks.txt"),checks);}
        object Get(string name)=>typeof(RaceMenus).GetField(name,Private).GetValue(menus);
        void Call(string name,params object[] args)=>typeof(RaceMenus).GetMethod(name,Private).Invoke(menus,args);
        UnityEngine.UI.Button Button(string id)=>((List<UnityEngine.UI.Button>)Get("buttons")).Single(b=>b.name==id&&b.gameObject.activeInHierarchy);
        IEnumerator Click(string id){while(MenuInput.Blocked)yield return null;var button=Button(id);EventSystem.current.SetSelectedGameObject(button.gameObject);button.onClick.Invoke();yield return null;yield return null;while(MenuInput.Blocked)yield return null;}
        IEnumerator Back(){while(MenuInput.Blocked)yield return null;flow.Back();yield return null;yield return null;while(MenuInput.Blocked)yield return null;}
        void Update(){AudioListener.volume=0;}
        IEnumerator Start()
        {
            Directory.CreateDirectory(Output);Application.runInBackground=true;
            oldBackground=InputSystem.settings.backgroundBehavior;oldEditorInput=InputSystem.settings.editorInputBehaviorInPlayMode;
            InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
            InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            yield return null;flow=FindAnyObjectByType<RaceFlow>();menus=flow.GetComponent<RaceMenus>();
            var title=FindAnyObjectByType<StartupTitle>();if(title)Destroy(title.gameObject);
            typeof(StartupTitle).GetField("completed",BindingFlags.Static|BindingFlags.NonPublic).SetValue(null,true);
            flow.UseValidationSave(Path.GetFullPath("Temp/UIFoundationSave"));flow.EnterMenuAfterTitle();yield return null;yield return null;
            if(followup){yield return FollowUp();yield break;}
            Check(((List<UnityEngine.UI.Button>)Get("buttons")).Count(b=>b.gameObject.activeInHierarchy)==7,"Main has seven approved destinations");
            ThreeFeatureValidation.CaptureUi(Output+"/main.png");
            yield return Click("race");Check((string)Get("page")=="race","Race enters setup directly");
            yield return Click("garage");Check(flow.State==RaceFlow.Stage.Garage,"Garage reachable from setup");
            ThreeFeatureValidation.CaptureUi(Output+"/garage.png");yield return Back();
            Check(flow.State==RaceFlow.Stage.Ready&&(string)Get("page")=="race"&&EventSystem.current.currentSelectedGameObject.name=="garage","Garage returns to setup and restores semantic focus");
            ThreeFeatureValidation.CaptureUi(Output+"/race-setup.png");yield return Back();yield return Click("garage");yield return Back();
            Check((string)Get("page")=="","Garage from Main returns to Main");
            yield return Click("settings");yield return Click("tab-audio");
            flow.Save.Settings.master=1;flow.RefreshMenu();yield return Click("master");Check(flow.Save.Settings.master==1,"Volume at 100 percent does not wrap");
            var changes=(Dictionary<int,Action<int>>)Get("adjustments");changes[4](-1);Check(Mathf.Approximately(flow.Save.Settings.master,.9f),"Audio adjustment decreases by one step");
            yield return Click("music");yield return Click("library");string source=flow.Save.Settings.musicSource,folder=flow.Save.Settings.musicFolder,song=flow.Radio.Song;
            yield return Click("folder");while(Get("directoryTask")!=null)yield return null;
            ThreeFeatureValidation.CaptureUi(Output+"/folder.png");yield return Click("cancel");
            Check(flow.Save.Settings.musicSource==source&&flow.Save.Settings.musicFolder==folder&&flow.Radio.Song==song,"Folder Cancel preserves source, folder and song");
            Check(!RaceMenus.ValidDirectory(@"\\server\share")&&!RaceMenus.ValidDirectory("https://example.com")&&!RaceMenus.ValidDirectory("Temp/missing-ui-folder"),"Network, URL and relative paths rejected");
            Directory.CreateDirectory("Temp/UIFoundationMusic/Album");Check(RaceMenus.ValidDirectory(Path.GetFullPath("Temp/UIFoundationMusic")),"Valid local absolute directory accepted");
            yield return Click("folder");while(Get("directoryTask")!=null)yield return null;yield return Click("path");
            Call("InsertText","draft");string draft=(string)Get("keyboardDraft");
            pad=InputSystem.AddDevice<Gamepad>();InputSystem.QueueStateEvent(pad,new GamepadState().WithButton(GamepadButton.West));yield return null;yield return null;
            InputSystem.QueueStateEvent(pad,new GamepadState());yield return null;yield return null;
            Check(MenuInput.Controller,"Meaningful controller input activates graphical prompts");
            Check(((string)Get("keyboardDraft")).Length==draft.Length-1,"Controller X deletes in shared draft");
            var submit=(InputAction)Get("submit");submit.ApplyBindingOverride(1,"<Gamepad>/buttonWest");Check(MenuInput.Binding(submit)=="<Gamepad>/buttonWest","Effective binding override resolves to X rather than conceptual A");submit.RemoveAllBindingOverrides();
            ThreeFeatureValidation.CaptureUi(Output+"/keyboard-controller.png");
            InputSystem.QueueStateEvent(pad,new GamepadState{leftStick=new Vector2(.05f,0)});yield return null;yield return null;
            Check(MenuInput.Controller,"Idle stick drift retains deliberate input family");
            yield return Back();Check((string)Get("page")=="folder","Keyboard Cancel returns one layer");yield return Click("cancel");yield return Back();Check((string)Get("page")=="music","Back Library returns Music");yield return Back();Check((string)Get("page")=="settings-audio","Back Music returns Audio category");yield return Back();Check(flow.State==RaceFlow.Stage.Ready,"Settings returns Main");
            yield return Click("exploration");yield return Click("restart-acorns");Check(EventSystem.current.currentSelectedGameObject.name=="cancel","Acorn reset modal defaults to Cancel");yield return Back();Check(flow.State==RaceFlow.Stage.Exploration,"Modal Back preserves parent");yield return Back();
            typeof(RaceFlow).GetMethod("SetStage",Private).Invoke(flow,new object[]{RaceFlow.Stage.Results});yield return null;flow.OpenGarage();yield return null;flow.Back();yield return null;
            Check(flow.State==RaceFlow.Stage.Results,"Garage returns to Results caller");
            flow.OpenSettings();yield return null;flow.Back();yield return null;Check(flow.State==RaceFlow.Stage.Results,"Settings returns to Results caller");
            flow.QuitRace();yield return null;yield return Click("race");yield return Click("tracks");
            Check(((List<UnityEngine.UI.Button>)Get("buttons")).Where(b=>b.gameObject.activeInHierarchy&&b.name.StartsWith("course-")).Select(b=>b.GetComponentInChildren<UnityEngine.UI.Text>().text).SequenceEqual(RacePlaylists.DisplayOrder.Select(i=>RacePlaylists.Titles[i])),"Eight track names use stable alphabetical display order");
            ThreeFeatureValidation.CaptureUi(Output+"/tracks.png");
            var radio=flow.Radio;yield return Click("course-6");yield return null;yield return null;
            flow=FindAnyObjectByType<RaceFlow>();menus=flow.GetComponent<RaceMenus>();flow.UseValidationSave(Path.GetFullPath("Temp/UIFoundationSave"));
            Check(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name==RacePlaylists.Scenes[6]&&(string)Get("page")=="race","Track scene switch returns to Race Setup");
            Check(flow.Radio==radio,"Track change retains process music service");
            InputSystem.RemoveDevice(pad);yield return null;Check(!MenuInput.Controller,"Disconnect falls back without activating a menu action");
            File.WriteAllText(Output+"/done.txt",$"{checks.Count(c=>c.StartsWith("PASS"))}/{checks.Count} pass");
            UnityEditor.EditorApplication.isPlaying=false;
        }
        IEnumerator Pad(GamepadButton button){while(MenuInput.Blocked)yield return null;InputSystem.QueueStateEvent(pad,new GamepadState().WithButton(button));yield return null;yield return null;InputSystem.QueueStateEvent(pad,new GamepadState());yield return null;yield return null;while(MenuInput.Blocked)yield return null;}
        IEnumerator FollowUp()
        {
            pad=InputSystem.AddDevice<Gamepad>();keyboard=InputSystem.AddDevice<Keyboard>();
            yield return null;yield return null;while(MenuInput.Blocked)yield return null;yield return null;
            var effectiveSubmit=(InputAction)Get("submit");var module=EventSystem.current.GetComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            Check(module.submit.action==effectiveSubmit&&effectiveSubmit.enabled,"UI module retains owned Submit through release barriers");
            EventSystem.current.SetSelectedGameObject(Button("race").gameObject);yield return Pad(GamepadButton.South);
            Check((string)Get("page")=="race"&&flow.State==RaceFlow.Stage.Ready,"One physical A opens Race Setup without leaking into Start Race");
            yield return Pad(GamepadButton.East);Check((string)Get("page")=="","Physical B removes one layer");
            effectiveSubmit.ApplyBindingOverride(1,"<Gamepad>/buttonWest");EventSystem.current.SetSelectedGameObject(Button("race").gameObject);yield return Pad(GamepadButton.West);
            Check((string)Get("page")=="race"&&module.submit.action==effectiveSubmit,"Actual X Submit override opens setup through EventSystem");effectiveSubmit.RemoveAllBindingOverrides();yield return Back();
            yield return Click("race");InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.Escape));yield return null;yield return null;InputSystem.QueueStateEvent(keyboard,new KeyboardState());yield return null;yield return null;
            Check((string)Get("page")==""&&flow.State==RaceFlow.Stage.Ready,"Physical Escape is Back inside menus");
            flow.Pause();yield return null;yield return Click("settings");yield return Back();Check(flow.State==RaceFlow.Stage.Paused,"Settings returns to Pause");
            yield return Click("return");yield return Pad(GamepadButton.East);Check(flow.State==RaceFlow.Stage.Paused&&!menus.ModalOpen,"B cancels Return-to-Menu modal without resuming/abandoning");
            flow.Resume();yield return null;yield return Click("settings");yield return Click("tab-audio");yield return Click("music");yield return Click("library");yield return Click("folder");while(Get("directoryTask")!=null)yield return null;yield return Click("path");
            string draft=(string)Get("keyboardDraft");yield return Pad(GamepadButton.LeftShoulder);Check(MenuInput.Controller,"Controller family active");
            InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.LeftShift));yield return null;yield return null;InputSystem.QueueStateEvent(keyboard,new KeyboardState());yield return null;yield return null;
            Check(!MenuInput.Controller&&(string)Get("keyboardDraft")==draft,"Keyboard switches prompts without losing text draft");
            yield return Pad(GamepadButton.LeftShoulder);Check(MenuInput.Controller&&(string)Get("keyboardDraft")==draft,"Return to controller preserves text and selection");
            var glyphs=FindObjectsByType<MenuGlyph>(FindObjectsInactive.Exclude);Check(glyphs.All(g=>g.GetComponent<CanvasRenderer>()!=null),"Graphical glyphs have CanvasRenderers");
            ThreeFeatureValidation.CaptureUi(Output+"/keyboard-controller.png");
            yield return Back();yield return Click("cancel");yield return Back();yield return Back();yield return Click("tab-controls");ThreeFeatureValidation.CaptureUi(Output+"/controls.png");
            yield return Click("tab-audio");yield return Click("music");yield return Click("library");yield return Click("folder");while(Get("directoryTask")!=null)yield return null;
            typeof(RaceMenus).GetField("folderDraft",Private).SetValue(menus,Path.GetFullPath("Temp/UIFoundationMissingFolder"));Call("ReadDirectories");while(Get("directoryTask")!=null)yield return null;
            Check(!Button("use-folder").interactable&&!string.IsNullOrEmpty((string)Get("folderError")),"Unavailable folder reports an error and cannot commit");
            Directory.CreateDirectory("Temp/UIFoundationMusic/Album");var chosen=Path.GetFullPath("Temp/UIFoundationMusic");typeof(RaceMenus).GetField("folderDraft",Private).SetValue(menus,chosen);Call("ReadDirectories");while(Get("directoryTask")!=null)yield return null;
            yield return Click("use-folder");Check(flow.Save.Settings.musicSource=="custom"&&flow.Save.Settings.musicFolder==chosen,"Use This Folder alone commits validated local path");
            yield return Back();yield return Back();yield return Back();yield return Click("race");yield return Click("tracks");var radio=flow.Radio;
            yield return Click("course-6");yield return null;yield return null;flow=FindAnyObjectByType<RaceFlow>();menus=flow.GetComponent<RaceMenus>();
            Check(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name==RacePlaylists.Scenes[6]&&(string)Get("page")=="race","Track scene switch returns to Race Setup");Check(flow.Radio==radio,"Track switch retains the same music service and save root");
            ThreeFeatureValidation.CaptureUi(Output+"/race-controller.png");
            File.WriteAllText(Output+"/followup-done.txt",$"{checks.Count(c=>c.StartsWith("PASS"))}/{checks.Count} pass");UnityEditor.EditorApplication.isPlaying=false;
        }
        void OnDestroy(){InputSystem.settings.backgroundBehavior=oldBackground;InputSystem.settings.editorInputBehaviorInPlayMode=oldEditorInput;if(keyboard!=null&&keyboard.added)InputSystem.RemoveDevice(keyboard);RaceFlow.ValidationSaveRoot=null;if(pad!=null&&pad.added)InputSystem.RemoveDevice(pad);}
    }
}
#endif
