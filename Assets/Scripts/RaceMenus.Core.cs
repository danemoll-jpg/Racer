using System;
using System.Linq;
using UnityEngine;

namespace Racer
{
    public sealed partial class RaceMenus
    {
        RaceFlow.Stage pageStage=RaceFlow.Stage.Ready;
        readonly System.Collections.Generic.Dictionary<RaceFlow.Stage,string[]> stageStacks=new();
        void PreparePage()
        {
            if(pageStage==flow.State)return;
            if(flow.State==RaceFlow.Stage.Activities)recordTab=2;
            stagePages[pageStage]=page;stageStacks[pageStage]=pages.Reverse().ToArray();
            pageStage=flow.State;page=stagePages.TryGetValue(pageStage,out var saved)?saved:"";
            pages.Clear();if(stageStacks.TryGetValue(pageStage,out var stack))foreach(var entry in stack)pages.Push(entry);
            modalConfirm=null;
        }
        void RenderCore()
        {
details.gameObject.SetActive(true);
            if(modalConfirm!=null)
            {
                ClearCore(modalTitle,modalMessage);preview.gameObject.SetActive(false);swatchRow.gameObject.SetActive(false);
                Row(0,"cancel","CANCEL",()=>{modalConfirm=null;Show();});
                Row(1,"confirm",modalConfirmLabel,()=>{var commit=modalConfirm;modalConfirm=null;commit();Show();});return;
            }
            if(page=="keyboard"){RenderKeyboard();return;}
            if(page=="folder"){RenderFolder();return;}
            if(page=="penalties")
            {
                var p=flow.Race.Progress;ClearCore("PENALTY DETAILS",$"{p.MissedGates} missed gates   ·   +{p.PenaltySeconds:0}s\nEach missed gate adds five seconds.");
                details.text+="\n\n"+string.Join("\n",p.Penalties.Select(e=>e.ToString()));
                details.GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight=Mathf.Max(90,details.preferredHeight+20);
                Row(0,"back","Back",()=>BackPage());return;
            }
            if(page=="help")
            {
                ClearCore("DETAILS",helpCopy);details.GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight=Mathf.Max(100,details.preferredHeight+20);
                Row(0,"back","Back",()=>BackPage());return;
            }
            if(page=="trailer"){RenderTrailer();return;}
            if(RenderLater())return;
            if(flow.State==RaceFlow.Stage.Ready)
            {
                if(page=="race")
                {
                    ClearCore("RACE SETUP",flow.Race.courseName+"\n"+flow.Race.vehicle.GetComponent<VehicleConfiguration>().Profile.Name+"  ·  "+flow.LapLabel+" laps\n"+(flow.Race.opponents?"3 AI · "+flow.Race.DifficultyName:"Solo / time trial")+"  ·  Traffic "+(flow.Race.traffic?"On":"Off"));
                    Row(0,"start","START RACE",flow.StartRace);var startColors=buttons[0].colors;startColors.normalColor=new(.1f,.38f,.35f);buttons[0].colors=startColors;buttons[0].GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight=54;Row(1,"tracks","Tracks",flow.OpenCourses);Row(2,"garage","Garage",flow.OpenGarage);Row(3,"opponents","Opponents",flow.OpenRoster);
                    Row(4,"laps","Laps: "+flow.LapLabel,flow.CycleLaps);Row(5,"mode",flow.Race.opponents?"Mode: Race vs 3 AI":"Mode: Solo / time trial",flow.ToggleOpponents);
                    Row(6,"difficulty","Difficulty: "+flow.Race.DifficultyName,flow.CycleDifficulty);Row(7,"traffic","Traffic: "+(flow.Race.traffic?"On":"Off"),flow.ToggleTraffic);
                    Row(8,"timeofday","Time of Day: "+flow.TimeOfDayLabel,flow.CycleTimeOfDay);Row(9,"weather","Weather: "+flow.WeatherLabel,flow.CycleWeather);
                    Row(10,"playlists","Playlists",flow.OpenPlaylists);Row(11,"back","Back",()=>BackPage());
                    if(flow.SetupFromResults&&RacePlaylists.Active!=null)
                    {
                        details.text+="\nActive playlist: this event keeps its saved track and lap count.\nReturn to Main to edit or start another playlist.";
                        details.GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight=180;
                        buttons[1].interactable=buttons[4].interactable=buttons[10].interactable=false;
                        buttons[0].GetComponentInChildren<UnityEngine.UI.Text>(true).text="RESTART CURRENT ENTRY";
                    }
                }
                else if(page=="roam")
                {
                    ClearCore("FREE ROAM",flow.Race.courseName+"\n"+flow.Race.vehicle.GetComponent<VehicleConfiguration>().Profile.Name+"\nExplore, collect acorns and score activities.");
                    Row(0,"explore","EXPLORE",flow.StartFreeRoam);Row(1,"roam-weather","Weather: "+flow.RoamWeatherLabel,flow.CycleRoamWeather);Row(2,"tracks","Tracks",flow.OpenCourses);Row(3,"garage","Garage",flow.OpenGarage);Row(4,"back","Back",()=>BackPage());
                }
                else
                {
                    ClearCore("WOODSTOCK RUSH","");
                    Row(0,"race","RACE",()=>{if(flow.InRoamWorld)flow.OpenRaceSetupFromRoam();else Navigate("race");});Row(1,"roam","FREE ROAM",()=>Navigate("roam"));Row(2,"garage","GARAGE",flow.OpenGarage);
                    Row(3,"records","RECORDS",flow.OpenBoards);Row(4,"exploration","EXPLORATION",flow.OpenExploration);Row(5,"settings","SETTINGS",flow.OpenSettings);Row(6,"quit","QUIT GAME",ConfirmQuit);
                    if(flow.RoamMenu){Row(7,"resume","RESUME DRIVING",flow.Resume);buttons[7].transform.SetSiblingIndex(buttons[0].transform.GetSiblingIndex());
                        Row(8,"trailer","TRAILER MODE",()=>Navigate("trailer"));buttons[8].transform.SetSiblingIndex(buttons[7].transform.GetSiblingIndex()+1);}
                }
            }
            else if(flow.State==RaceFlow.Stage.Paused)
            {
                if(page=="activities")
                {
                    var site=flow.Activities.Selected;var best=site?flow.Activities.PersonalBest(site):null;
                    ClearCore("ACTIVITIES","Traps and authored jumps score automatically while driving.\n"+(site?site.title+"\n"+flow.Activities.Location+"\n"+flow.Activities.Targets+" · PB "+ArcadeActivities.Measurement(site,best?.value??0):"No activity selected."));
                    Row(0,"activity","Activity: "+(site?site.title:"None"),()=>{flow.Activities.Cycle();Show();});
                    Row(1,"start","Start / Retry",()=>{flow.Activities.BeginAttempt();flow.Resume();});Row(2,"cancel","Cancel activity",()=>{flow.Activities.Cancel();Show();});Row(3,"back","Back",()=>BackPage());return;
                }
                ClearCore(flow.Race.FreeRoam?"FREE ROAM / PAUSED":"RACE PAUSED",flow.Race.FreeRoam?"":$"Elapsed {RaceHud.FormatTime(flow.Race.Progress.RaceTime(flow.Race.Clock))}  ·  +{flow.Race.Progress.PenaltySeconds:0}s penalties");
                Row(0,"resume","RESUME",flow.Resume);
                if(flow.Race.FreeRoam){Row(1,"map","MAP",()=>flow.GetComponent<ExplorationMap>()?.Open());Row(2,"activities","ACTIVITIES",()=>Navigate("activities"));}
                else Row(1,"restart","RESTART RACE",()=>Confirm("RESTART RACE?","This restarts the current event and clears its progress.",flow.StartRace));
                Row(flow.Race.FreeRoam?10:2,"trailer","TRAILER MODE",()=>Navigate("trailer"));
                Row(3,"settings","SETTINGS",flow.OpenSettings);Row(4,"return",flow.Race.FreeRoam?"RETURN TO MENU":"END RACE / RETURN TO MENU",()=>Confirm(flow.Race.FreeRoam?"RETURN TO MENU?":"END RACE AND RETURN TO MENU?",RacePlaylists.Active!=null?"The active playlist and championship progress will end. Saved playlists are kept.":"The current event will end.",flow.QuitRace));
                Row(5,"records","Records",flow.OpenBoards);Row(6,"exploration","Exploration",flow.OpenExploration);
                if(!flow.Race.FreeRoam)Row(7,"penalties","Penalty Details",()=>Navigate("penalties"));
                if(flow.Race.Progress.Finished&&!flow.Race.ClassificationFinal)Row(8,"complete","Complete Race",flow.Race.FinalizeUnfinishedAi);
                Row(9,"quit","Quit Game",ConfirmQuit);
            }
            else if(flow.State==RaceFlow.Stage.Settings)RenderSettings();
            else if(flow.State==RaceFlow.Stage.Exploration)
            {
                ClearCore("EXPLORATION",flow.GetComponent<ExplorationCollection>()?.Summary??"Collection loading");
                details.GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight=Mathf.Max(110,details.preferredHeight+20);
                Row(0,"map","World Map",()=>flow.GetComponent<ExplorationMap>()?.Open());
                Row(1,"restart-acorns","Restart Acorn Hunt…",()=>Confirm("RESTART ACORN HUNT?","Only acorn-hunt progress will reset. Map discoveries, records, ghosts and settings are preserved.",()=>flow.GetComponent<ExplorationCollection>()?.RestartCollection(true)));
                Row(2,"back","Back",flow.CloseExtras);
            }
            else if(flow.State==RaceFlow.Stage.Courses)
            {
                if(page=="course-preview"){RenderCoursePreview();return;}
                ClearCore("TRACKS","Select to use a track. Highlighting does not change Race Setup. Difficulty: TBD.");int i=0;
                foreach(int course in RacePlaylists.DisplayOrder){int choice=course;Row(i++,"course-"+course,RacePlaylists.Titles[course],()=>flow.SelectCourseEntry(choice));}
                Row(i++,"preview-track","Preview highlighted track",()=>Navigate("course-preview"));
                Row(i,"back","Back",flow.CloseGarage);
            }
            else if(flow.State==RaceFlow.Stage.Garage)
            {
                if(page=="rider"){RenderRider();return;}
                buttons[4].GetComponentInChildren<UnityEngine.UI.Text>(true).text="Back";
                LayoutGarage();
                for(int i=0;i<swatches.Count;i++){var text=swatches[i].GetComponentInChildren<UnityEngine.UI.Text>(true);text.text=(flow.SelectedColor==i?"✓ ":"")+VehiclePaint.Names[i];}
            }
            else if(flow.State==RaceFlow.Stage.Roster)
            {
                ClearCore("OPPONENT VEHICLES","Slot      Choice                         Resolved vehicle      Status");
                for(int i=0;i<3;i++){int slot=i;string choice=flow.Save.Settings.opponentChoices[i];string resolved=flow.Race.opponentRoster[i];
                    Row(i,"slot-"+i,$"{i+1}     {(choice=="random"?"Random":choice=="mixed"?"Mixed":VehicleProfile.Find(choice).Name)}     →     {VehicleProfile.Find(resolved).Name}     Ready",()=>flow.CycleOpponent(slot));}
                Row(3,"mixed","Mixed roster",flow.MixedRoster);Row(4,"reroll","Reroll Random / Mixed",flow.ResolveRoster);
                Row(5,"help","Details",()=>Help("Random may repeat. Mixed uses eligible profiles before repeating.\nResolved vehicles stay fixed for rematches."));Row(6,"back","Back",flow.CloseGarage);
            }
            else if(flow.State==RaceFlow.Stage.PlaylistVehicle)
            {
                string affected=(!RacePlaylists.Eligible(RacePlaylists.Current,flow.Save.Settings.vehicleId)?"Player\n":"")+string.Join("\n",Enumerable.Range(0,3).Where(i=>!RacePlaylists.Eligible(RacePlaylists.Current,flow.Save.Settings.opponentRoster[i])).Select(i=>"AI slot "+(i+1)+": "+VehicleProfile.Find(flow.Save.Settings.opponentRoster[i]).Name));
                ClearCore("VEHICLE COMPATIBILITY",RacePlaylists.PositionLabel+"\n"+RacePlaylists.Current.Title+"\nIncompatible: "+affected+"\nChoose your vehicle. Only incompatible AI slots will be replaced.\nCancel ends the playlist and returns to Main Menu.");
                Row(0,"cancel","Cancel / Return to Menu",flow.CancelPlaylist);Row(1,"moto","Motorcycle",()=>flow.ChoosePlaylistVehicle("moto"));Row(2,"atv","ATV",()=>flow.ChoosePlaylistVehicle("atv"));
            }
            else if(flow.State==RaceFlow.Stage.Results)
            {
                bool championship=RacePlaylists.Active!=null&&(showChampionship||RacePlaylists.Championship.Complete);
                if(!championship)Row(3,"penalties","Penalty Details",()=>Navigate("penalties"));
                if(RacePlaylists.Active!=null)Row(championship?3:2,"quit-playlist","Quit Playlist / menu",()=>Confirm("QUIT PLAYLIST?","The active championship will end. Saved playlists are kept.",flow.QuitRace));
            }
            else if(flow.State==RaceFlow.Stage.Boards)
            {
                Row(5,"back","Back",flow.CloseGarage);Row(6,"activity-records","Speed Trap / Jump Records",flow.OpenActivities);
                Row(7,"ghost","Ghost: "+(flow.Ghost.Enabled?"On":"Off"),flow.ToggleGhost);
                Row(8,"ghost-help","Ghost Details",()=>Help("Race your best clean lap.\n"+flow.Ghost.Status+"\nNo resets, teleports or missed gates; legal shortcuts qualify."));
            }
        }
        string helpCopy="";
        // Every quit-to-desktop control asks first; focus starts on CANCEL and B / Esc cancels (0.70 Part A).
        void ConfirmQuit()=>Confirm("QUIT WOODSTOCK RUSH?","",flow.Quit,"QUIT");
        void Help(string text){helpCopy=text;Navigate("help");}
        void RenderSettings()
        {
            var s=flow.Save.Settings;var radio=flow.Radio;
            if(page=="music")
            {
                ClearCore("LOCAL MUSIC",radio.ChannelName+"\n"+radio.Song);
                Row(0,"channel","Channel: "+radio.ChannelName+" / Off",()=>{radio.Toggle();Show();});Row(1,"previous","Previous",()=>{radio.Previous();Show();});Row(2,"next","Next",()=>{radio.Next();Show();});
                Step(3,"volume",$"Music {s.music:P0}",d=>Adjust(()=>s.music=Mathf.Clamp01(s.music+d*.1f)));Row(4,"library","Music Library",()=>Navigate("library"));Row(5,"back","Back",()=>BackPage());return;
            }
            if(page=="library")
            {
                string friendly=System.IO.Path.GetFileName(radio.Folder.TrimEnd('\\','/'));
                ClearCore("MUSIC LIBRARY",(radio.Bundled?"Bundled music":"Custom: "+friendly)+"\n"+radio.Status+"\n"+radio.ScanStatus);
                Row(0,"source","Source: "+(radio.Bundled?"Bundled":"Custom"),()=>{radio.SetSource(!radio.Bundled);Show();});Row(1,"folder","Choose Folder",OpenFolderPicker);
                Row(2,"rescan","Rescan",()=>{radio.Rescan();Show();});Row(3,"cancel-scan","Cancel Scan",()=>{radio.CancelScan();Show();});Row(4,"open-folder","Open Folder on Computer",radio.OpenFolder);
                Row(5,"help","Details",()=>Help(radio.Folder+"\nMP3 / WAV / Ogg. Nested folders are included as channels.\nAdd music files on your computer, then Rescan. See RADIO.md beside the game."));Row(6,"back","Back",()=>BackPage());return;
            }
            if(page=="")page="settings-gameplay";
            ClearCore("SETTINGS","");
            string[] cats={"gameplay","audio","display","controls"};
            for(int i=0;i<cats.Length;i++){string category=cats[i];Row(i,"tab-"+category,(page=="settings-"+category?"✓ ":"")+category.ToUpperInvariant(),()=>{page="settings-"+category;Show();});}
            if(page=="settings-gameplay")Row(4,"estimate","Estimate AI at Your Finish: "+(s.estimateAiFinishes?"On":"Off"),()=>Adjust(()=>s.estimateAiFinishes=!s.estimateAiFinishes));
            if(page=="settings-audio")
            {
                Step(4,"master",$"Master {s.master:P0}",d=>Adjust(()=>s.master=Mathf.Clamp01(s.master+d*.1f)));
                Step(5,"vehicle",$"Vehicle {s.vehicle:P0}",d=>Adjust(()=>s.vehicle=Mathf.Clamp01(s.vehicle+d*.1f)));
                Step(6,"ambience",$"Ambience {s.ambience:P0}",d=>Adjust(()=>s.ambience=Mathf.Clamp01(s.ambience+d*.1f)));
                Step(7,"feedback",$"Race / UI {s.feedback:P0}",d=>Adjust(()=>s.feedback=Mathf.Clamp01(s.feedback+d*.1f)));
                Step(8,"music-volume",$"Music {s.music:P0}",d=>Adjust(()=>s.music=Mathf.Clamp01(s.music+d*.1f)));
                Row(9,"music","Music",()=>Navigate("music"));
            }
            if(page=="settings-display")
            {
                details.gameObject.SetActive(true);details.text="VSync uses your display refresh. The frame cap applies with VSync off.";details.GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight=58;
                Row(4,"vsync","VSync: "+(s.vsync?"On":"Off"),()=>Adjust(()=>s.vsync=!s.vsync));
                Step(5,"fps","Frame cap: "+s.frameLimit+" FPS",d=>Adjust(()=>{int[] caps={30,60,120};s.frameLimit=caps[Mathf.Clamp(Array.IndexOf(caps,s.frameLimit)+d,0,2)];}));
                Row(6,"lightning","Lightning flashes: "+(s.lightningFlashes?"On":"Off"),()=>Adjust(()=>s.lightningFlashes=!s.lightningFlashes));
            }
            if(page=="settings-controls")RenderControls();
            if(page!="settings-controls")Row(14,"back","Back",flow.CloseSettings);
        }
        readonly System.Collections.Generic.List<(MenuGlyph glyph,UnityEngine.UI.Text key,UnityEngine.InputSystem.InputAction action,string pad,string keyboard)> bindingRows=new();
        void RenderControls()
        {
            int row=4;
            void Binding(string label,UnityEngine.InputSystem.InputAction action,string pad="",string keyboard=""){
                int i=row++;Row(i,"binding-"+label,label,()=>{});
                var button=buttons[i];var labelText=button.GetComponentInChildren<UnityEngine.UI.Text>(true);labelText.rectTransform.offsetMin=new(86,0);
                var icon=Rect("Current binding glyph",button.transform);icon.anchorMin=icon.anchorMax=new(0,.5f);icon.pivot=new(0,.5f);icon.anchoredPosition=new(12,0);icon.sizeDelta=new(62,34);
                var graphic=icon.gameObject.AddComponent<MenuGlyph>();graphic.raycastTarget=false;
                var key=Label("Control",icon,16,0);Stretch(key.rectTransform,0,0,0,0);key.alignment=TextAnchor.MiddleCenter;
                bindingRows.Add((graphic,key,action,pad,keyboard));
            }
            foreach(var action in flow.Race.vehicle.GetComponent<VehicleInput>().CurrentBindings)Binding("Driving / "+action.name,action);
            Binding("Menus / Select",submit);Binding("Menus / Back",cancelAction);Binding("Menus / Navigate",uiModule.move.action);
            Binding("Menus / Previous category",previousTab);Binding("Menus / Next category",tabsAction);
            Binding("Pause / Resume",flow.PauseAction);
            if(CameraViews.Current)Binding("Camera / Change view",CameraViews.Current.CycleAction);
            Binding("Trailer Mode / On or off (or pause menu)",null,"<Gamepad>/start","<Keyboard>/f8");
            foreach(var action in flow.GetComponent<ExplorationMap>().Bindings)Binding("Map / "+action.name,action);
            Binding("Radio / Previous",null,"<Gamepad>/dpad/left","<Keyboard>/leftBracket");Binding("Radio / Next",null,"<Gamepad>/dpad/right","<Keyboard>/rightBracket");Binding("Radio / Current song",null,"<Gamepad>/dpad/up","<Keyboard>/i");Binding("Radio / Channel or Off",null,"<Gamepad>/dpad/down","<Keyboard>/n");
            Row(row,"back","Back",flow.CloseSettings);
        }
        void ClearBindingRows()
        {
            foreach(var row in bindingRows)if(row.glyph)Destroy(row.glyph.gameObject);bindingRows.Clear();
            foreach(var b in buttons)b.GetComponentInChildren<UnityEngine.UI.Text>(true).rectTransform.offsetMin=new(10,0);
        }
    }
}
