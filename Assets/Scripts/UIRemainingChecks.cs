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
    public sealed class UIRemainingChecks:MonoBehaviour
    {
        const string Output="Docs/UI/Phase345";
        static readonly BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
        readonly List<string> checks=new();RaceFlow flow;RaceMenus menus;Gamepad pad;
        InputSettings.BackgroundBehavior background;InputSettings.EditorInputBehaviorInPlayMode editorInput;
        public static void Launch(){new GameObject("Remaining UI targeted checks").AddComponent<UIRemainingChecks>();}
        object Get(object owner,string name)=>owner.GetType().GetField(name,Private).GetValue(owner);
        void Set(object owner,string name,object value)=>owner.GetType().GetField(name,Private).SetValue(owner,value);
        object Call(object owner,string name,params object[] args)=>owner.GetType().GetMethod(name,Private).Invoke(owner,args);
        void Check(bool good,string label){checks.Add((good?"PASS ":"FAIL ")+label);File.WriteAllLines(Output+"/checks.txt",checks);}
        UnityEngine.UI.Button Button(string id)=>((List<UnityEngine.UI.Button>)Get(menus,"buttons")).First(b=>b.name==id&&b.gameObject.activeInHierarchy);
        IEnumerator Click(string id){while(MenuInput.Blocked)yield return null;var b=Button(id);EventSystem.current.SetSelectedGameObject(b.gameObject);b.onClick.Invoke();yield return null;yield return null;while(MenuInput.Blocked)yield return null;}
        IEnumerator Back(){while(MenuInput.Blocked)yield return null;flow.Back();yield return null;yield return null;while(MenuInput.Blocked)yield return null;}
        IEnumerator Pad(GamepadButton button){while(MenuInput.Blocked)yield return null;InputSystem.QueueStateEvent(pad,new GamepadState().WithButton(button));yield return null;yield return null;InputSystem.QueueStateEvent(pad,new GamepadState());yield return null;yield return null;while(MenuInput.Blocked)yield return null;}
        void Update(){AudioListener.volume=0;}
        IEnumerator Start()
        {
            Directory.CreateDirectory(Output);background=InputSystem.settings.backgroundBehavior;editorInput=InputSystem.settings.editorInputBehaviorInPlayMode;InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            yield return null;flow=FindAnyObjectByType<RaceFlow>();menus=flow.GetComponent<RaceMenus>();
            Check(flow.Save.DirectoryPath.Contains("UIRemaining"),"All services initialized with isolated save root");if(!flow.Save.DirectoryPath.Contains("UIRemaining"))yield break;
            var title=FindAnyObjectByType<StartupTitle>();if(title)Destroy(title.gameObject);typeof(StartupTitle).GetField("completed",BindingFlags.Static|BindingFlags.NonPublic).SetValue(null,true);flow.EnterMenuAfterTitle();pad=InputSystem.AddDevice<Gamepad>();yield return null;yield return null;
            var initial=new RacePlaylists.Definition{name="Weekend Races — long playlist name",entries=Enumerable.Range(0,8).Select(i=>new RacePlaylists.Entry{course=i,laps=i%5+1}).ToList()};if(flow.Playlists.Definitions.Count==0)flow.Playlists.SaveDraft(-1,initial);else flow.Playlists.SaveDraft(0,initial);
            yield return Click("race");yield return Click("playlists");Check(((List<UnityEngine.UI.Button>)Get(menus,"buttons")).Count(b=>b.name.StartsWith("entry-")&&b.gameObject.activeInHierarchy)>=8,"Playlist exposes every entry as a list");
            yield return Pad(GamepadButton.RightShoulder);ThreeFeatureValidation.CaptureUi(Output+"/playlists-720.png");ThreeFeatureValidation.CaptureUi(Output+"/playlists-800.png",1280,800);ThreeFeatureValidation.CaptureUi(Output+"/playlists-1080.png",1920,1080);
            yield return Click("entry-0");yield return Click("edit-course-4");yield return Click("apply");Check(flow.Playlists.Definitions[0].entries[0].course==0,"Editing does not mutate saved definition");yield return Click("back");Check((string)Get(menus,"page")=="playlist-dirty","Dirty Back opens Save / Discard / Cancel");yield return Click("cancel");Check(flow.State==RaceFlow.Stage.Playlists,"Dirty Cancel keeps editor");yield return Click("back");yield return Click("discard");Check(flow.Playlists.Definitions[0].entries[0].course==0,"Discard preserves saved snapshot");
            yield return Click("playlists");yield return Click("entry-0");yield return Click("edit-course-2");yield return Click("apply");yield return Click("save");Check(new RacePlaylists(flow.Save.DirectoryPath).Definitions[0].entries[0].course==2,"Save persists edited entry");
            yield return Click("entry-1");yield return Back();yield return Click("actions");yield return Click("move");EventSystem.current.SetSelectedGameObject(Button("move-3").gameObject);yield return null;yield return null;yield return Back();Check(((RacePlaylists.Definition)Get(menus,"playlistDraft")).entries[1].course==1,"Move cancellation restores order");yield return Back();
            yield return Pad(GamepadButton.West);Check((string)Get(menus,"page")=="playlist-editor","Labeled X opens Add Race");yield return Click("apply");yield return Click("save");Check(flow.Playlists.Definitions[0].entries.Count==9,"Duplicate entries retained after add/save");yield return Back();yield return Back();
            string category=flow.Race.Category;flow.Boards.Add("fixture/one",category,false,121.2,"moto");flow.Boards.Add("fixture/tie",category,false,121.2,"moto");flow.Boards.Add("fixture/legacy",category,false,125,"moto",null,true);
            yield return Click("records");ThreeFeatureValidation.CaptureUi(Output+"/records.png");Check(Button("record-fixture/legacy")!=null,"Legacy and current entries appear in aligned records table");yield return Pad(GamepadButton.RightShoulder);Check((int)Get(menus,"recordTab")==1,"RB switches visible Records tabs");yield return Click("categories");Check(flow.Race.Category==category,"Record browsing does not change race category or scene");yield return Back();yield return Back();
            var map=flow.GetComponent<ExplorationMap>();foreach(var d in map.destinations.Take(2))map.Reveal(d.position);map.Open();yield return null;yield return null;while(MenuInput.Blocked)yield return null;
            yield return Pad(GamepadButton.RightShoulder);Check((int)Get(map,"selected")>=0&&map.Discovered(map.destinations[(int)Get(map,"selected")].id),"Map shoulder cycling reaches only discovered landmarks");
            var overlay=(WorldMapCourseOverlay)Get(map,"courseOverlay");bool before=overlay.gameObject.activeSelf;yield return Pad(GamepadButton.West);Check(overlay.gameObject.activeSelf!=before,"X toggles race overlay using controller");Check(((UnityEngine.UI.RawImage)Get(map,"picture")).texture!=null,"Permanent geography remains when overlay toggles");
            yield return Pad(GamepadButton.North);Check(map.Waypoint.HasValue,"Y sets selected waypoint");yield return Pad(GamepadButton.North);Check(!map.Waypoint.HasValue,"Y clears the same waypoint");
            ThreeFeatureValidation.CaptureUi(Output+"/map-720.png");ThreeFeatureValidation.CaptureUi(Output+"/map-800.png",1280,800);ThreeFeatureValidation.CaptureUi(Output+"/map-1080.png",1920,1080);
            yield return Pad(GamepadButton.South);Check((bool)Get(map,"sheetOpen")&&!map.Confirming,"A selects landmark without travelling");yield return Pad(GamepadButton.East);Check(map.Opened&&!(bool)Get(map,"sheetOpen"),"B dismisses map context before closing map");
            flow.Race.FreeRoam=true;map.RequestTravel((int)Get(map,"selected"));yield return null;Check(map.Confirming&&EventSystem.current.currentSelectedGameObject.name=="cancel","Travel confirmation defaults to Cancel");yield return Pad(GamepadButton.South);Check(!map.Confirming&&map.Opened,"Default A cancels travel without closing map");
            int target=(int)Get(map,"selected");var original=map.destinations[target].position;map.destinations[target].position=new(50000,50000,50000);map.RequestTravel(target);Check(!map.ConfirmTravel()&&map.Opened,"Blocked or unsupported arrival retains map and error");map.destinations[target].position=original;
            flow.Race.FreeRoam=false;yield return Pad(GamepadButton.East);Check(!map.Opened&&flow.State==RaceFlow.Stage.Ready,"Map Back returns to caller");
            Check(FinishPresentation.Evaluate(90,1,100,100).headline=="NEW COURSE RECORD!","Strict #1 improvement");Check(FinishPresentation.Evaluate(90,1,0,0).headline=="FIRST COURSE RECORD!","Empty board first record");Check(FinishPresentation.Evaluate(110,2,100,120).headline=="PODIUM TIME!","Rank 2–3 podium");Check(FinishPresentation.Evaluate(130,5,100,140).headline=="TOP 10 TIME!","Rank 4–10 presentation");Check(FinishPresentation.Evaluate(130,0,100,140).headline=="NEW PERSONAL BEST!","PB fallback");Check(FinishPresentation.Evaluate(100,2,100,100).headline=="Matched Best","Tie never claims faster");Check(FinishPresentation.Evaluate(150,0,100,140).headline=="Finished","No achievement");Check(FinishPresentation.Evaluate(99.9999,1,100,100).comparison.Contains("Less than"),"Submillisecond improvement is not zero");
            var progress=new RaceProgress(1,3);var boards=new RecordBoards(Path.Combine(flow.Save.DirectoryPath,"finish"));var presentation=new FinishPresentation();presentation.Begin(boards,category,new RacerSave.Records());progress.Cross(0,true,0);double clock=0;foreach(double time in new[]{80d,60d,70d}){progress.Cross(1,true,clock+1);clock+=time;progress.Cross(0,true,clock);boards.CompletedLap("attempt",category,"moto",progress);}boards.CompletedRace("attempt",category,"moto",progress,clock);presentation.Finish(boards,category,"attempt",progress,clock);Check(presentation.Lap.id=="attempt/lap/2"&&presentation.Lap.rank==1&&presentation.Lap.time==60,"Fastest lap is resolved after all writes, not final lap rank");
            string denied=Path.Combine(flow.Save.DirectoryPath,"file-not-directory");File.WriteAllText(denied,"retained");var failed=new RacePlaylists(denied);Check(!failed.SaveDraft(-1,initial)&&failed.Definitions.Count==0&&File.ReadAllText(denied)=="retained","Failed playlist save preserves snapshot and existing file");
            var championship=new PlaylistChampionship(2);var finish=new PlaylistChampionship.Event{course="Fixture",order=new[]{new PlaylistChampionship.Finish{id="player",name="You",place=1},new PlaylistChampionship.Finish{id="ai",name="Rival",place=2,estimated=true}}};championship.Record(0,finish);championship.Record(1,finish);Check(championship.Complete&&championship.Standings()[0].points==20,"Championship retains existing points and snapshots");championship.Restart(0);Check(!championship.Complete&&championship.Standings()[0].points==10,"Restart replaces event score instead of accumulating");
            flow.FinishCards.Begin(flow.Boards,category,flow.Save.Best);flow.FinishCards.Finish(boards,category,"attempt",progress,clock);flow.PushMenu(RaceFlow.Stage.Results);yield return null;ThreeFeatureValidation.CaptureUi(Output+"/results.png");yield return Click("tab-LAP TIMES");ThreeFeatureValidation.CaptureUi(Output+"/lap-times.png");yield return Click("settings");yield return Back();Check(flow.State==RaceFlow.Stage.Results,"Settings retains Results caller");yield return Click("setup");yield return Back();Check(flow.State==RaceFlow.Stage.Results,"Race Setup returns to Results");
            File.WriteAllText(Output+"/done.txt",checks.Count(c=>c.StartsWith("PASS"))+"/"+checks.Count+" pass");UnityEditor.EditorApplication.isPlaying=false;
        }
        void OnDestroy(){if(pad!=null&&pad.added)InputSystem.RemoveDevice(pad);InputSystem.settings.backgroundBehavior=background;InputSystem.settings.editorInputBehaviorInPlayMode=editorInput;RaceFlow.ValidationSaveRoot=null;}
    }
}
#endif
