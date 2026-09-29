using System.Collections.Generic;
using System.Text;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

namespace Racer
{
    // Runtime extension of the existing uGUI HUD; legacy font retained to match its text.
    public sealed class RaceMenus : MonoBehaviour
    {
        RaceFlow flow;
        GameObject shade;
        RectTransform card;
        UnityEngine.UI.Text title, details, banner, songBanner, help;
        bool musicPage;
        bool musicCollectionPage;
        bool raceBoard;
        int playlistIndex,entryIndex,nameCursor; bool editingPlaylistName;
        int championshipPage; bool showChampionship;
        UnityEngine.UI.InputField playlistName;
        string nameDraft;
        bool controllerName;
        int nameClosedFrame=-1;
        public bool OwnsTextInput => editingPlaylistName || Time.frameCount<=nameClosedFrame+1;
        bool confirmCollectionRestart;
        bool jumpRecords,historyActivities;int activitySite,activityCategory;
        string boardCategory;
        readonly List<UnityEngine.UI.Button> buttons = new();
        readonly Dictionary<RaceFlow.Stage, int> selections = new();
        RaceFlow.Stage shown;
        int penaltyPage = -1;
        float nextMusicRefresh;
        InputAction submit;
        InputActionAsset menuActions;
        InputActionReference submitReference;
        Font font;
        GameObject hudPanel;
        UnityEngine.UI.RawImage preview;
        Camera previewCamera;
        RenderTexture previewTexture;
        GameObject previewRoot;
        RectTransform swatchRow;
        readonly List<UnityEngine.UI.Button> swatches=new();
        public void Initialize(RaceFlow owner)
        {
            flow = owner; font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var hud = FindAnyObjectByType<RaceHud>();
            hudPanel = hud.display.transform.parent.gameObject;
            var canvas = hud.GetComponent<Canvas>();
            if (!canvas.GetComponent<UnityEngine.UI.GraphicRaycaster>()) canvas.gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            var es = EventSystem.current;
            if (!es) es = new GameObject("Race menu EventSystem", typeof(EventSystem)).GetComponent<EventSystem>();
            var module = es.GetComponent<InputSystemUIInputModule>();
            if (!module) module = es.gameObject.AddComponent<InputSystemUIInputModule>();
            module.AssignDefaultActions();
            menuActions = ScriptableObject.CreateInstance<InputActionAsset>();
            var map = new InputActionMap("Race menu"); menuActions.AddActionMap(map);
            submit = map.AddAction("Menu confirm", InputActionType.Button);
            submit.AddBinding("<Keyboard>/space"); submit.AddBinding("<Gamepad>/buttonSouth");
            submitReference = InputActionReference.Create(submit); module.submit = submitReference;
            // Back is owned by RaceFlow; Enter/Start are exclusively pause/resume.
            module.cancel = null; submit.Enable();
            shade = Rect("Menu shade", canvas.transform).gameObject;
            Stretch(shade.GetComponent<RectTransform>(), 0, 0, 0, 0);
            shade.AddComponent<UnityEngine.UI.Image>().color = new Color(.015f, .025f, .04f, .78f);
            card = Rect("Race menu", shade.transform); card.anchorMin = card.anchorMax = card.pivot = new Vector2(.5f, .5f);
            card.sizeDelta = new Vector2(700, 680);
            card.gameObject.AddComponent<UnityEngine.UI.Image>().color = new Color(.035f, .065f, .085f, .98f);
            var layout = card.gameObject.AddComponent<UnityEngine.UI.VerticalLayoutGroup>();
            layout.padding = new RectOffset(28, 28, 20, 20); layout.spacing = 8;
            layout.childControlWidth = layout.childControlHeight = true; layout.childForceExpandHeight = false;
            title = Label("Title", card, 32, 48); title.color = new Color(.3f, .95f, .81f);
            details = Label("Details", card, 20, 160);
            var nameRect=Rect("Playlist name",card);
            nameRect.gameObject.AddComponent<UnityEngine.UI.LayoutElement>().preferredHeight=44;
            var nameBackground=nameRect.gameObject.AddComponent<UnityEngine.UI.Image>();nameBackground.color=new(.12f,.24f,.29f);
            playlistName=nameRect.gameObject.AddComponent<UnityEngine.UI.InputField>();
            playlistName.targetGraphic=nameBackground;
            var nameText=Label("Editable name",nameRect,23,0);Stretch(nameText.rectTransform,12,4,-12,-4);
            playlistName.textComponent=nameText;playlistName.characterLimit=64;
            playlistName.lineType=UnityEngine.UI.InputField.LineType.SingleLine;
            playlistName.onValueChanged.AddListener(value=>nameDraft=value);
            nameRect.gameObject.SetActive(false);
            var previewRect = Rect("Vehicle preview",card);
            previewRect.gameObject.AddComponent<UnityEngine.UI.LayoutElement>().preferredHeight=130;
            preview=previewRect.gameObject.AddComponent<UnityEngine.UI.RawImage>(); preview.raycastTarget=false;
            previewTexture=new RenderTexture(640,130,16); preview.texture=previewTexture;
            previewCamera=new GameObject("Garage preview camera").AddComponent<Camera>();
            previewCamera.cullingMask=1<<31; previewCamera.clearFlags=CameraClearFlags.SolidColor;
            previewCamera.backgroundColor=new Color(.06f,.1f,.13f); previewCamera.targetTexture=previewTexture;
            previewCamera.transform.position=new Vector3(10000,10003,9994); previewCamera.transform.LookAt(new Vector3(10000,10000.5f,10000));
            previewCamera.fieldOfView=36; previewCamera.farClipPlane=30;
            for (int i = 0; i < 15; i++)
            {
                var rect = Rect("Action " + i, card);
                rect.gameObject.AddComponent<UnityEngine.UI.LayoutElement>().preferredHeight = 44;
                var img = rect.gameObject.AddComponent<UnityEngine.UI.Image>(); img.color = Color.white;
                var button = rect.gameObject.AddComponent<UnityEngine.UI.Button>(); button.targetGraphic = img;
                var colors = button.colors; colors.normalColor = new Color(.10f,.20f,.25f); colors.highlightedColor = new Color(.17f,.43f,.46f);
                colors.selectedColor = new Color(.17f,.43f,.46f); colors.pressedColor = new Color(.2f,.6f,.55f); colors.fadeDuration = .06f; button.colors = colors;
                var text = Label("Label", rect, 21, 0); Stretch(text.rectTransform, 10, 0, -10, 0); text.alignment = TextAnchor.MiddleCenter;
                buttons.Add(button);
            }
            swatchRow=Rect("Body color swatches",card);
            swatchRow.gameObject.AddComponent<UnityEngine.UI.LayoutElement>().preferredHeight=36;
            var swatchLayout=swatchRow.gameObject.AddComponent<UnityEngine.UI.HorizontalLayoutGroup>(); swatchLayout.spacing=6; swatchLayout.childControlWidth=swatchLayout.childControlHeight=true; swatchLayout.childForceExpandWidth=true;
            for(int i=0;i<VehiclePaint.Colors.Length;i++)
            {
                int choice=i; var rect=Rect(VehiclePaint.Names[i],swatchRow);
                var img=rect.gameObject.AddComponent<UnityEngine.UI.Image>(); img.color=VehiclePaint.Colors[i];
                var button=rect.gameObject.AddComponent<UnityEngine.UI.Button>(); button.targetGraphic=img;
                var colors=button.colors; colors.selectedColor=colors.highlightedColor=Color.white; colors.normalColor=new Color(.72f,.72f,.72f); button.colors=colors;
                button.onClick.AddListener(()=>flow.SetColor(choice));
                var text=Label("Color",rect,17,0); Stretch(text.rectTransform,0,0,0,0); text.alignment=TextAnchor.MiddleCenter; text.text=VehiclePaint.Names[i]; text.color=(i==1 || i==3 || i==5 || i==6)?Color.white:Color.black;
                swatches.Add(button);
            }
            help = Label("Menu controls", card, 16, 38);
            help.text = "D-pad / stick / arrows: select     A / Space: confirm\nB / Esc: back     Enter / Start: pause or resume";
            banner = Label("Race feedback", canvas.transform, 26, 0);
            banner.alignment = TextAnchor.MiddleCenter; banner.color = new Color(.4f, 1, .85f);
            banner.rectTransform.anchorMin = new Vector2(.04f,.35f); banner.rectTransform.anchorMax = new Vector2(.96f,.6f);
            banner.rectTransform.offsetMin = banner.rectTransform.offsetMax = Vector2.zero;
            var outline = banner.gameObject.AddComponent<UnityEngine.UI.Outline>(); outline.effectColor = new Color(0,0,0,.85f); outline.effectDistance = new Vector2(2,-2);
            shown = RaceFlow.Stage.Ready;
            songBanner=Label("Current song",canvas.transform,18,0);
            songBanner.alignment=TextAnchor.MiddleCenter;
            songBanner.rectTransform.anchorMin=new Vector2(.12f,.035f);songBanner.rectTransform.anchorMax=new Vector2(.80f,.10f);
            songBanner.horizontalOverflow=HorizontalWrapMode.Overflow;
            songBanner.resizeTextForBestFit=true; songBanner.resizeTextMinSize=14; songBanner.resizeTextMaxSize=18;
            songBanner.rectTransform.offsetMin=songBanner.rectTransform.offsetMax=Vector2.zero;
            songBanner.gameObject.AddComponent<UnityEngine.UI.Outline>();
        }
        static RectTransform Rect(string name, Transform parent)
        { var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>(); rect.SetParent(parent, false); return rect; }
        static void Stretch(RectTransform rect, float left, float bottom, float right, float top)
        { rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = new Vector2(left,bottom); rect.offsetMax = new Vector2(right,top); }
        UnityEngine.UI.Text Label(string name, Transform parent, int size, float height)
        {
            var rect = Rect(name, parent); var text = rect.gameObject.AddComponent<UnityEngine.UI.Text>();
            text.font = font; text.fontSize = size; text.color = Color.white; text.raycastTarget = false;
            text.supportRichText=false;
            text.horizontalOverflow = HorizontalWrapMode.Wrap; text.verticalOverflow = VerticalWrapMode.Truncate;
            if (height > 0) rect.gameObject.AddComponent<UnityEngine.UI.LayoutElement>().preferredHeight = height;
            return text;
        }
        void Action(int index, string label, UnityEngine.Events.UnityAction action)
        {
            var b = buttons[index]; b.gameObject.SetActive(true); b.GetComponentInChildren<UnityEngine.UI.Text>().text = label;
            b.onClick.RemoveAllListeners(); b.onClick.AddListener(action);
        }
        string Record(double seconds) => seconds > 0 ? RaceHud.FormatTime(seconds) : "—";
        void PenaltyDetails(RaceProgress p)
        {
            details.fontSize=16;
            details.GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight=200;
            details.text=$"{p.MissedGates} MISSED GATES / +{p.PenaltySeconds:0}s / page {penaltyPage+1} of {(p.Penalties.Count+3)/4}\n";
            for(int i=penaltyPage*4;i<Mathf.Min(p.Penalties.Count,(penaltyPage+1)*4);i++) details.text+=p.Penalties[i]+"\n";
        }
        public void Show()
        {
            if (!shade) return;
            int selected = buttons.FindIndex(b => EventSystem.current && EventSystem.current.currentSelectedGameObject == b.gameObject);
            if(selected<0) { int swatch=swatches.FindIndex(b=>EventSystem.current && EventSystem.current.currentSelectedGameObject==b.gameObject); if(swatch>=0) selected=buttons.Count+swatch; }
            if (selected >= 0) selections[shown] = selected;
            shown = flow.State; shade.SetActive(flow.MenuVisible && shown!=RaceFlow.Stage.Title);
            if(shown!=RaceFlow.Stage.Results)showChampionship=false;
            playlistName.gameObject.SetActive(shown==RaceFlow.Stage.Playlists&&editingPlaylistName&&!controllerName);
            help.text=editingPlaylistName&&!controllerName?"Type / paste / select text    Enter: save    Escape: cancel":"D-pad / stick / arrows: select     A / Space: confirm\nB / Esc: back     Enter / Start: pause or resume";
            if(shown!=RaceFlow.Stage.Settings)musicPage=false;
            preview.gameObject.SetActive(shown==RaceFlow.Stage.Garage);
            previewCamera.enabled=shown==RaceFlow.Stage.Garage;
            swatchRow.gameObject.SetActive(shown==RaceFlow.Stage.Garage);
            if (shown != RaceFlow.Stage.Results && shown!=RaceFlow.Stage.Paused) penaltyPage = -1;
            hudPanel.SetActive(!flow.MenuVisible);
            EventSystem.current.SetSelectedGameObject(null);
            if (!flow.MenuVisible || shown==RaceFlow.Stage.Title) return;
            foreach (var b in buttons) b.gameObject.SetActive(false);
            card.GetComponent<UnityEngine.UI.VerticalLayoutGroup>().spacing=shown==RaceFlow.Stage.Garage?5:8;
            title.GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight=shown==RaceFlow.Stage.Garage?42:48;
            title.fontSize=32;
            foreach(var b in buttons) b.GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight=(shown==RaceFlow.Stage.Garage || shown==RaceFlow.Stage.Results)?38:44;
            details.GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight = 160;
            details.fontSize=20;
            details.supportRichText=shown==RaceFlow.Stage.Boards;
            if (shown == RaceFlow.Stage.Ready)
            {
                title.text = "WOODSTOCK RUSH / "+flow.Race.courseName.ToUpperInvariant();
                details.text = "Cross START to begin lap 1.";
                Action(0,"Start race",flow.StartRace); Action(1,"Settings",flow.OpenSettings); Action(2,"Quit Game",flow.Quit);
                Action(3, flow.Race.opponents ? "Mode: Race vs 3 AI" : "Mode: Solo / time trial", flow.ToggleOpponents);
                Action(4, "Traffic: " + (flow.Race.traffic ? "On" : "Off"), flow.ToggleTraffic);
                Action(5,"Difficulty: " + flow.Race.DifficultyName,flow.CycleDifficulty);
                Action(6,"Garage: " + flow.Race.vehicle.GetComponent<VehicleConfiguration>().Profile.Name,flow.OpenGarage);
                Action(7,"Opponent vehicles",flow.OpenRoster);
                Action(8,"Track: "+flow.Race.courseName,flow.OpenCourses);
                Action(9,"Records / Top 10",()=>{boardCategory=null;flow.OpenBoards();});
                Action(10,"Free Roam / explore + arcade activities",flow.StartFreeRoam);
                Action(11,"Activity Records / Speed Traps and Jumps",flow.OpenActivities);
                Action(12,"Exploration / clean-lap ghosts",flow.OpenExploration);
                Action(13,"Laps: "+flow.LapLabel,flow.CycleLaps);
                Action(14,"Saved race playlists",flow.OpenPlaylists);
                foreach(var b in buttons)b.GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight=27;
                details.GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight=86;
                card.GetComponent<UnityEngine.UI.VerticalLayoutGroup>().spacing=5;
                details.fontSize=18;
                details.text=$"{flow.LapLabel} laps / clock starts at GO\n{(flow.Race.opponents?flow.Race.RosterLabel:"Solo time trial")}\nBest lap {Record(flow.Save.Best.lap)}"+(flow.Race.laps==0?" / End session from pause":$"  /  race {Record(flow.Save.Best.race)}");
            }
            else if (shown == RaceFlow.Stage.Paused)
            {
                title.text = "PAUSED";
                var p=flow.Race.Progress;
                details.text=$"Elapsed {RaceHud.FormatTime(p.RaceTime(flow.Race.Clock))}  +{p.PenaltySeconds:0.0}s penalties\nAdjusted {RaceHud.FormatTime(p.AdjustedTime(flow.Race.Clock))}\nR / Y: recover locally; time and lap progress continue.\nRestart Race clears this event and restores props.";
                Action(0,"Resume",flow.Resume); Action(1,"Restart Race",flow.StartRace); Action(2,"Settings",flow.OpenSettings); Action(3,RacePlaylists.Active!=null?"Quit Playlist / menu":flow.Race.laps==0?"End session / menu":"Quit Race / Return to Menu",flow.QuitRace); Action(4,"Quit Game",flow.Quit);
                Action(5,"Penalty breakdown / next page",()=>{ penaltyPage++; if(penaltyPage*4>=p.Penalties.Count) penaltyPage=-1; Show(); });
                if(p.Finished && !flow.Race.ClassificationFinal) Action(6,"Skip waiting / estimate remaining AI",flow.Race.FinalizeUnfinishedAi);
                if(penaltyPage>=0) PenaltyDetails(p);
                if(flow.Race.FreeRoam)
                {
                    title.text="FREE ROAM / PAUSED";
                    var site=flow.Activities.Selected;var best=site?flow.Activities.PersonalBest(site):null;
                    details.text="Traps and authored jumps score automatically while driving.\nR / Y: local reset. Only smash challenges need activation.\n"+(site?$"{site.title} / {flow.Activities.Location}\n{flow.Activities.Targets} / PB {ArcadeActivities.Measurement(site,best?.value??0)}":"");
                    foreach(var button in buttons)button.gameObject.SetActive(false);
                    Action(0,"Resume exploring",flow.Resume);
                    Action(1,"Activity: "+(site?site.title:"none"),()=>{flow.Activities.Cycle();Show();});
                    Action(2,"Start / retry selected activity",()=>{flow.Activities.BeginAttempt();flow.Resume();});
                    Action(3,"Cancel activity",()=>{flow.Activities.Cancel();flow.Resume();});
                    Action(4,"Settings / music",flow.OpenSettings);
                    Action(5,"Return to menu / choose race",flow.QuitRace);
                    Action(6,"Quit Game",flow.Quit);
                }
                Action(7,"Activity Records / Speed Traps and Jumps",flow.OpenActivities);
                Action(8,"Exploration / clean-lap ghosts",flow.OpenExploration);
                foreach(var b in buttons)b.GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight=32;
            }
            else if (shown == RaceFlow.Stage.Results)
            {
                title.text = flow.Race.courseName.ToUpperInvariant()+" / RACE COMPLETE";
                var p = flow.Race.Progress; var text = new StringBuilder();
                text.AppendLine("Total   " + RaceHud.FormatTime(p.RaceTime(flow.Race.Clock)) + (flow.NewRaceRecord ? "   NEW PB" : ""));
                for (int i = 0; i < p.LapTimes.Count; i++) text.AppendLine("Lap " + (i+1) + "   " + RaceHud.FormatTime(p.LapTimes[i]));
                text.AppendLine("Best lap   " + RaceHud.FormatTime(p.BestLap) + (flow.NewLapRecord ? "   NEW PB" : ""));
                details.text = text.ToString();
                details.GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight = 180;
                details.text = $"Driving {RaceHud.FormatTime(p.RaceTime(flow.Race.Clock))} + {p.PenaltySeconds:0.0}s penalties\nAdjusted {RaceHud.FormatTime(p.AdjustedTime(flow.Race.Clock))}\n" + flow.Race.Standings() + $"\nYour missed gates: {p.MissedGates}";
                Action(0,"Race again",flow.StartRace); Action(1,"Settings",flow.OpenSettings); Action(2,"Return to Menu",flow.QuitRace);
                Action(3, "Penalty breakdown / next page", () => { penaltyPage++; if (penaltyPage * 4 >= p.Penalties.Count) penaltyPage = -1; Show(); });
                Action(4,"Garage / next vehicle",flow.OpenGarage);
                Action(5,"Opponent vehicles",flow.OpenRoster);
                Action(6,"Records / Top 10",()=>{boardCategory=null;flow.OpenBoards();});
                Action(7,"Activity Records / Speed Traps and Jumps",flow.OpenActivities);
                Action(8,"Exploration / clean-lap ghosts",flow.OpenExploration);
                foreach(var b in buttons)b.GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight=30;
                details.text+=$"\nLap {(flow.NewLapRecord?"NEW PB / ":"")}{(flow.LapRank>0?"TOP 10 #"+flow.LapRank:"")}  Race {(flow.NewRaceRecord?"NEW PB / ":"")}{(flow.RaceRank>0?"TOP 10 #"+flow.RaceRank:"")}";
                if (penaltyPage >= 0) {
                    PenaltyDetails(p);
                }
                if(RacePlaylists.Active!=null){
                    var championship=RacePlaylists.Championship;
                    title.text="PLAYLIST RESULTS "+(RacePlaylists.Position+1)+"/"+RacePlaylists.Active.entries.Count;details.text+="\n"+RacePlaylists.PositionLabel;
                    Action(0,"Restart current entry",flow.StartRace);Action(2,"Quit Playlist / menu",flow.QuitRace);
                    if(RacePlaylists.HasNext)Action(9,"Next Race: "+RacePlaylists.Active.entries[RacePlaylists.Position+1].Title,flow.NextPlaylistRace);
                    Action(10,"Championship / all event summaries",()=>{showChampionship=true;championshipPage=RacePlaylists.Position;Show();});
                    if(showChampionship||championship.Complete){
                        foreach(var b in buttons)b.gameObject.SetActive(false);
                        title.text=championship.Announcement;title.fontSize=25;
                        title.GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight=65;
                        details.text=championship.Summary(championshipPage);details.fontSize=17;
                        details.GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight=330;
                        Action(0,"Previous event",()=>{championshipPage=(championshipPage+championship.Events.Length-1)%championship.Events.Length;Show();});
                        Action(1,"Next event",()=>{championshipPage=(championshipPage+1)%championship.Events.Length;Show();});
                        Action(2,"Restart current entry",flow.StartRace);Action(3,"Quit Playlist / menu",flow.QuitRace);
                        if(RacePlaylists.HasNext)Action(4,"Next Race",flow.NextPlaylistRace);
                    }
                }
            }
            else if(shown==RaceFlow.Stage.PlaylistVehicle)
            {
                title.text="CHOOSE COMPATIBLE VEHICLES";details.text=RacePlaylists.PositionLabel+"\n"+RacePlaylists.Current.Title+"\nThis event requires motorcycles / ATVs. Choose your vehicle and replace incompatible opponents with that same profile. Nothing has changed yet.";
                Action(0,"Use Motorcycle + replace incompatible opponents",()=>flow.ChoosePlaylistVehicle("moto"));Action(1,"Use ATV + replace incompatible opponents",()=>flow.ChoosePlaylistVehicle("atv"));Action(2,"Quit Playlist / menu",flow.CancelPlaylist);
            }
            else if(shown==RaceFlow.Stage.Playlists)
            {
                title.text="SAVED RACE PLAYLISTS";var library=flow.Playlists.Definitions;
                if(library.Count==0){details.text=flow.Playlists.Error??"Create a local playlist. Definitions stay saved when you quit.";Action(0,"New playlist",()=>{library.Add(new RacePlaylists.Definition());playlistIndex=library.Count-1;Show();});Action(1,"Back",flow.CloseGarage);}
                else
                {
                    playlistIndex=Mathf.Clamp(playlistIndex,0,library.Count-1);var definition=library[playlistIndex];entryIndex=Mathf.Clamp(entryIndex,0,Mathf.Max(0,definition.entries.Count-1));
                    details.text=definition.name+" · playlist "+(playlistIndex+1)+" / "+library.Count+"\n";
                    if(editingPlaylistName)
                    {
                        details.text+="Type or paste a name. Enter saves; Escape cancels.\nSteam Deck: Steam + X opens the keyboard.\nController: Start switches to the character picker; B cancels.";
                        if(controllerName){
                            nameCursor=Mathf.Clamp(nameCursor,0,63);string name=nameDraft.PadRight(64);details.text+="\n"+name.Substring(0,nameCursor)+"["+name[nameCursor]+"]"+name.Substring(nameCursor+1);
                            void Change(int delta){const string alphabet=" ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-";var chars=nameDraft.PadRight(64).ToCharArray();int at=alphabet.IndexOf(chars[nameCursor]);chars[nameCursor]=alphabet[(Mathf.Max(0,at)+delta+alphabet.Length)%alphabet.Length];nameDraft=new string(chars);Show();}
                            Action(0,"Previous position",()=>{nameCursor=(nameCursor+63)%64;Show();});Action(1,"Next position",()=>{nameCursor=(nameCursor+1)%64;Show();});Action(2,"Previous letter",()=>Change(-1));Action(3,"Next letter",()=>Change(1));
                        }
                        Action(4,"Save name",()=>FinishName(true));Action(5,"Cancel",()=>FinishName(false));
                        Action(6,controllerName?"Use keyboard text field":"Controller character picker",()=>{controllerName=!controllerName;Show();});
                    }
                    else
                    {
                        details.text+=definition.entries.Count==0?"No races yet":$"Entry {entryIndex+1} / {definition.entries.Count}: "+definition.entries[entryIndex].Title;
                        details.text+="\n"+(flow.Playlists.Error??"Roster and difficulty use race setup. Unlimited is standalone.");
                        Action(0,"Start playlist",()=>flow.StartPlaylist(definition));Action(1,"Name playlist",()=>{nameDraft=definition.name;controllerName=false;editingPlaylistName=true;Show();});
                        Action(2,"Previous entry",()=>{entryIndex=Mathf.Max(0,entryIndex-1);Show();});Action(3,"Next entry",()=>{entryIndex=Mathf.Min(definition.entries.Count-1,entryIndex+1);Show();});
                        Action(4,"Add race (duplicates allowed)",()=>{definition.entries.Add(new RacePlaylists.Entry());entryIndex=definition.entries.Count-1;Show();});
                        if(definition.entries.Count>0){var entry=definition.entries[entryIndex];Action(5,"Course / direction: "+RacePlaylists.Titles[entry.course],()=>{entry.course=(entry.course+1)%RacePlaylists.Scenes.Length;Show();});Action(6,"Laps: "+entry.laps,()=>{entry.laps=entry.laps%5+1;Show();});Action(7,"Move entry earlier",()=>{if(entryIndex>0){definition.entries.RemoveAt(entryIndex);definition.entries.Insert(--entryIndex,entry);}Show();});Action(8,"Move entry later",()=>{if(entryIndex+1<definition.entries.Count){definition.entries.RemoveAt(entryIndex);definition.entries.Insert(++entryIndex,entry);}Show();});Action(9,"Remove entry",()=>{definition.entries.RemoveAt(entryIndex);Show();});}
                        Action(10,"Save playlist",()=>{if(flow.Playlists.Save())flow.Notify("Playlist saved",3);Show();});Action(11,"Next saved playlist",()=>{playlistIndex=(playlistIndex+1)%library.Count;entryIndex=0;Show();});Action(12,"New playlist",()=>{library.Add(new RacePlaylists.Definition());playlistIndex=library.Count-1;entryIndex=0;Show();});Action(13,"Back",flow.CloseGarage);
                    }
                    details.GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight=110;foreach(var b in buttons)b.GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight=27;card.GetComponent<UnityEngine.UI.VerticalLayoutGroup>().spacing=5;
                }
            }
            else if(shown==RaceFlow.Stage.Activities)
            {
                var sites=flow.Activities.Sites.Where(s=>s.kind==(jumpRecords?ActivitySite.Kind.Jump:ActivitySite.Kind.Speed)).ToArray();
                activitySite=Mathf.Clamp(activitySite,0,Mathf.Max(0,sites.Length-1));
                title.text="ACTIVITY RECORDS / "+(jumpRecords?"JUMPS":"SPEED TRAPS");
                details.fontSize=16;details.GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight=335;
                details.text="No authored sites on this track.";
                if(sites.Length>0)
                {
                    var site=sites[activitySite];string current=flow.Activities.Key(site);
                    var categories=flow.Activities.Records.Archive.entries.Where(e=>e.site==site.id).Select(e=>e.key).Append(current).Distinct().OrderBy(x=>x).ToArray();
                    activityCategory=(activityCategory+categories.Length)%categories.Length;string key=historyActivities?categories[activityCategory]:current;
                    var entries=flow.Activities.Records.Board(key);
                    var pieces=key.Split('/');string layout=key==current?"Current layout":"Historical layout "+pieces[1];string vehicleId=pieces.Length>3?pieces[3]:flow.Race.vehicle.GetComponent<VehicleConfiguration>().profileId;
                    details.text=site.title+"\n"+layout+" / "+VehicleProfile.Find(vehicleId).Name+" / "+(key.EndsWith("/opposite")?"opposite travel":"forward travel")+"\nRank   Result   Vehicle   Date (UTC)   Medal\n";
                    for(int i=0;i<entries.Count;i++){var e=entries[i];details.text+=$"{i+1}. {ArcadeActivities.Measurement(site,e.value)}  {VehicleProfile.Find(e.vehicle).Name}  {(string.IsNullOrEmpty(e.date)?"Unknown (legacy)":e.date.Substring(0,10))}  {new[]{"—","Bronze","Silver","Gold"}[Mathf.Clamp(e.medal,0,3)]}\n";}
                    details.text+=entries.Count==0?"No valid attempts yet. Drive through a trap or land an authored jump.\n":"PB: "+ArcadeActivities.Measurement(site,entries[0].value)+"\n";
                    details.text+=flow.Activities.Records.Error??"Distinct ties retain attempt order. Historical categories remain separate.";
                }
                foreach(var b in buttons)b.GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight=25;
                card.GetComponent<UnityEngine.UI.VerticalLayoutGroup>().spacing=4;title.GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight=42;
                Action(0,(jumpRecords?"":"Selected: ")+"Speed Traps",()=>{jumpRecords=false;activitySite=activityCategory=0;Show();});
                Action(1,(jumpRecords?"Selected: ":"")+"Jumps",()=>{jumpRecords=true;activitySite=activityCategory=0;Show();});
                Action(2,"Previous site",()=>{activitySite=(activitySite+sites.Length-1)%Mathf.Max(1,sites.Length);activityCategory=0;Show();});
                Action(3,"Next site",()=>{activitySite=(activitySite+1)%Mathf.Max(1,sites.Length);activityCategory=0;Show();});
                Action(4,historyActivities?"Use current compatible category":"Browse saved / historical categories",()=>{historyActivities=!historyActivities;Show();});
                Action(5,"Next saved category",()=>{historyActivities=true;activityCategory++;Show();});
                Action(6,"Back",flow.CloseExtras);
            }
            else if(shown==RaceFlow.Stage.Exploration)
            {
                title.text="EXPLORATION / PERSONAL BEST";
                flow.Ghost.Refresh();
                details.text="Clean-lap ghosts: actual recorded poses, local only.\nNo resets, teleports or missed gates; legal shortcuts qualify.\n"+flow.Ghost.Status+"\n\n"+(flow.GetComponent<ExplorationCollection>()?.Summary??"Collection loading");
                details.GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight=280;details.fontSize=18;
                Action(0,"Clean-lap ghost: "+(flow.Ghost.Enabled?"ON":"OFF"),flow.ToggleGhost);
                
                Action(1,"Whole-area map / discovered fast travel",()=>flow.GetComponent<ExplorationMap>()?.Open());
                Action(2,confirmCollectionRestart?"CONFIRM: restart ONLY the 24 acorns":"Restart relocated acorn hunt...",()=>{if(confirmCollectionRestart){flow.GetComponent<ExplorationCollection>()?.RestartCollection(true);confirmCollectionRestart=false;}else confirmCollectionRestart=true;Show();});
                Action(3,"Back / cancel restart",()=>{confirmCollectionRestart=false;flow.CloseExtras();});
                details.text+="\nRelocated found IDs stay collected unless you restart.\nRestart preserves map, records, ghosts and settings.";
            }
            else if(shown==RaceFlow.Stage.Courses)
            {
                title.text="SELECT TRACK";
                details.text="Street: all four vehicles. Forest: motorcycles / ATVs.\nReverse courses have their own jumps and optional shortcuts.\nSeparate direction, rules and record categories.";
                Action(0,"Street Loop",()=>flow.SelectCourse(false));
                Action(1,"Forest Loop",()=>flow.SelectCourse(true));
                Action(2,"Street Loop Reverse",()=>flow.SelectCourse(false,true));
                Action(3,"Forest Loop Reverse",()=>flow.SelectCourse(true,true));
                Action(4,"Mountain Loop",()=>flow.SelectMountain(false));Action(5,"Mountain Loop Reverse",()=>flow.SelectMountain(true));Action(6,"Back",flow.CloseGarage);
            }
            else if(shown==RaceFlow.Stage.Boards)
            {
                title.text=raceBoard?"TOP 10 / TOTAL RACE":"TOP 10 / LAP";
                if(boardCategory==null)boardCategory=raceBoard?flow.Race.Category:RecordBoards.LapCategory(flow.Race.Category);
                details.fontSize=16;details.GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight=340;
                var entries=flow.Boards.Board(boardCategory,raceBoard);
                details.text=RecordBoards.Describe(boardCategory)+"\nRank    Adjusted time       Vehicle       Date (UTC)\n";
                for(int i=0;i<entries.Count;i++)
                {
                    var e=entries[i];bool recent=flow.Boards.IsNew(e.id);
                    string row=$"{i+1,2}.  {RaceHud.FormatTime(e.seconds)}  {VehicleProfile.Find(e.vehicle).Name}  {(string.IsNullOrEmpty(e.date)||e.date.Length<10?"Unknown (legacy)":e.date.Substring(0,10))}";
                    details.text+=(recent?"<color=#57F5C3>"+row+(i==0?"  PB":"  NEW")+"</color>":row)+"\n";
                }
                if(entries.Count==0)details.text+="No eligible completed attempts in this category.\n";
                details.text+="\nFull-precision ordering; ties keep attempt order.\n"+(flow.Boards.Error??"");
                foreach(var b in buttons)b.GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight=30;
                Action(0,(raceBoard?"":"Selected: ")+"Lap",()=>{raceBoard=false;boardCategory=null;Show();});
                Action(1,(raceBoard?"Selected: ":"")+"Race",()=>{raceBoard=true;boardCategory=null;Show();});
                Action(2,"Previous saved category",()=>CycleBoard(-1));
                Action(3,"Next saved category",()=>CycleBoard(1));
                Action(4,"Current track / vehicle / race configuration",()=>{boardCategory=null;Show();});
                Action(5,"Back to menu",flow.CloseGarage);
            }
            else if (shown == RaceFlow.Stage.Garage)
            {
                var profile=flow.Race.vehicle.GetComponent<VehicleConfiguration>().Profile;
                title.text=profile.Name + " / " + profile.Class;
                details.GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight=80;
                details.fontSize=18;
                details.text=$"{profile.Description}\n{(flow.Race.Forest?"Forest Loop: motorcycles / ATVs only (player and AI).":"Street Loop: all four profiles available.")}\nBody color: choose a swatch below. R / Y recovers locally.";
                if(previewRoot) { previewRoot.SetActive(false); Destroy(previewRoot); }
                previewRoot=new GameObject("Garage display model"); previewRoot.layer=31; previewRoot.transform.position=new(10000,10000,10000); previewRoot.transform.rotation=Quaternion.Euler(0,-30,0);
                flow.Race.vehicle.GetComponent<VehicleConfiguration>().BuildPreview(previewRoot.transform);
                for(int i=0;i<flow.Race.EligibleVehicles.Length;i++) { var choice=flow.Race.EligibleVehicles[i]; Action(i,(profile.Id==choice.Id?"Selected: ":"Select: ")+choice.Name,()=>flow.SelectVehicle(choice.Id)); }
                Action(4,"Done / ready",flow.CloseGarage);
            }
            else if(shown==RaceFlow.Stage.Roster)
            {
                title.text="OPPONENT VEHICLES";
                details.text="Choose each slot. Random may repeat; Mixed uses eligible profiles before repeating.\nResolved roster stays fixed for restart/rematch.\n\n"+flow.Race.RosterLabel;
                details.fontSize=18;
                for(int i=0;i<3;i++) { int slot=i; string choice=flow.Save.Settings.opponentChoices[i]; Action(i,$"Slot {i+1}: {(choice=="random"?"Random":choice=="mixed"?"Mixed":VehicleProfile.Find(choice).Name)}  →  {VehicleProfile.Find(flow.Race.opponentRoster[i]).Name}",()=>flow.CycleOpponent(slot)); }
                Action(3,"Use Mixed roster",flow.MixedRoster); Action(4,"Reroll Random / Mixed",flow.ResolveRoster); Action(5,"Done / ready",flow.CloseGarage);
            }
            else if (shown == RaceFlow.Stage.Settings)
            {
                card.GetComponent<UnityEngine.UI.VerticalLayoutGroup>().spacing=6;
                foreach(var b in buttons)b.GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight=musicPage?32:38;
                title.text = "SETTINGS"; details.GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight = 96;
                details.text = "Select a setting to cycle its value. Changes apply now.\nVSync uses your display refresh; frame limit applies with VSync off.\nVolume steps: 0–100% in 10% increments.";
                var s = flow.Save.Settings;
                Action(0,$"Master volume   {s.master:P0}",()=>Adjust(()=>s.master=NextVolume(s.master)));
                Action(1,$"Ambience volume   {s.ambience:P0}",()=>Adjust(()=>s.ambience=NextVolume(s.ambience)));
                Action(2,$"Race / UI volume   {s.feedback:P0}",()=>Adjust(()=>s.feedback=NextVolume(s.feedback)));
                Action(3,$"Vehicle volume   {s.vehicle:P0}",()=>Adjust(()=>s.vehicle=NextVolume(s.vehicle)));
                Action(4,"VSync   " + (s.vsync?"On":"Off"),()=>Adjust(()=>s.vsync=!s.vsync));
                Action(5,"Frame limit   " + s.frameLimit + " fps",()=>Adjust(()=>s.frameLimit=s.frameLimit==30?60:s.frameLimit==60?120:30));
                Action(6,"Back",flow.CloseSettings);
                Action(7,"Music / local radio",()=>{musicPage=true;Show();});
                if(!musicPage) Action(8,"Estimate AI at your finish: "+(s.estimateAiFinishes?"On":"Off"),()=>Adjust(()=>s.estimateAiFinishes=!s.estimateAiFinishes));
                if(musicPage)
                {
                    var radio=flow.Radio; title.text="LOCAL MUSIC";
                    details.fontSize=16;details.GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight=150;
                    details.text=MusicDetails();
                    Action(0,$"Music volume {s.music:P0}",()=>Adjust(()=>s.music=NextVolume(s.music)));
                    Action(1,"Channel: "+radio.ChannelName+" / next channel or Off",()=>{radio.Toggle();Show();});
                    Action(2,"Next track",()=>{radio.Next();Show();});
                    Action(3,"Previous track",()=>{radio.Previous();Show();});
                    Action(4,"Open Music folder",radio.OpenFolder);Action(5,"Rescan Music folder",()=>{radio.Rescan();Show();});
                    Action(6,"Music source / collection setup",()=>{musicCollectionPage=true;Show();});Action(7,"Back to settings",()=>{musicPage=false;musicCollectionPage=false;Show();});
                    if(musicCollectionPage)
                    {
                        title.text="MUSIC COLLECTION";
                        Action(0,"Source: "+(radio.Bundled?"Bundled music":"Custom folder")+" (switch)",()=>{radio.SetSource(!radio.Bundled);Show();});
                        Action(1,"Folder channels / nested albums included",()=>{radio.ShowSong();Show();});
                        Action(2,"Choose custom folder",radio.ChooseFolder);
                        Action(3,"Open selected folder",radio.OpenFolder);
                        Action(4,"Rescan collection",()=>{radio.Rescan();Show();});
                        Action(5,"Cancel scan",()=>{radio.CancelScan();Show();});
                        Action(6,"Back to music controls",()=>{musicCollectionPage=false;Show();});
                        buttons[7].gameObject.SetActive(false);
                    }
                }
            }
            var active = buttons.FindAll(b=>b.gameObject.activeSelf);
            if(shown==RaceFlow.Stage.Garage) active.AddRange(swatches);
            for (int i=0;i<active.Count;i++) active[i].navigation = new UnityEngine.UI.Navigation { mode=UnityEngine.UI.Navigation.Mode.Explicit, selectOnUp=active[(i+active.Count-1)%active.Count], selectOnDown=active[(i+1)%active.Count] };
            if(shown==RaceFlow.Stage.Garage) for(int i=0;i<swatches.Count;i++) { var nav=swatches[i].navigation; nav.selectOnLeft=swatches[(i+swatches.Count-1)%swatches.Count]; nav.selectOnRight=swatches[(i+1)%swatches.Count]; swatches[i].navigation=nav; }
            int focus = selections.TryGetValue(shown,out var prior)?prior:0;
            if(editingPlaylistName&&!controllerName&&shown==RaceFlow.Stage.Playlists){playlistName.SetTextWithoutNotify(nameDraft);EventSystem.current.SetSelectedGameObject(playlistName.gameObject);playlistName.ActivateInputField();return;}
            if(shown==RaceFlow.Stage.Garage && focus>=buttons.Count && focus<buttons.Count+swatches.Count) { EventSystem.current.SetSelectedGameObject(swatches[focus-buttons.Count].gameObject); return; }
            if (focus >= buttons.Count || !buttons[focus].gameObject.activeSelf) focus=0;
            EventSystem.current.SetSelectedGameObject(buttons[focus].gameObject);
        }
        static float NextVolume(float value) => value >= .99f ? 0 : Mathf.Min(1, (Mathf.Floor(value*10+.01f)+1)/10);
        void FinishName(bool save)
        {
            if(save){var clean=new string((nameDraft??"").Where(c=>!char.IsControl(c)).Take(64).ToArray()).Trim();if(clean.Length==0){flow.Notify("Enter a playlist name",3);Show();return;}var d=flow.Playlists.Definitions[playlistIndex];var old=d.name;d.name=clean;if(!flow.Playlists.Save()){d.name=old;Show();return;}}
            editingPlaylistName=false;nameClosedFrame=Time.frameCount;playlistName.DeactivateInputField();Show();
        }
        void Update()
        {
            if(!editingPlaylistName)return;
            if(!controllerName&&Gamepad.current?.startButton.wasPressedThisFrame==true){controllerName=true;playlistName.DeactivateInputField();Show();return;}
            if(Keyboard.current?.escapeKey.wasPressedThisFrame==true||Gamepad.current?.buttonEast.wasPressedThisFrame==true)FinishName(false);
            else if(Keyboard.current?.enterKey.wasPressedThisFrame==true||Keyboard.current?.numpadEnterKey.wasPressedThisFrame==true)FinishName(true);
        }
        void CycleBoard(int direction)
        {
            var categories=flow.Boards.Categories(raceBoard).Append(raceBoard?flow.Race.Category:RecordBoards.LapCategory(flow.Race.Category)).Distinct().OrderBy(c=>c,System.StringComparer.Ordinal).ToArray();
            int i=System.Array.IndexOf(categories,boardCategory);boardCategory=categories[(i+direction+categories.Length)%categories.Length];Show();
        }
        string MusicDetails()
        {
            var radio=flow.Radio;
            return radio.Song+"\n"+radio.Status+"\n"+radio.ScanStatus+"\n"+(radio.Bundled?"Bundled: ":"Custom: ")+radio.Folder+"\n"+(musicCollectionPage?"Add songs here, then Rescan. ZIP setup: RADIO.md beside game.":"Driving: D-pad →/←/↑/↓ or ] / [ / I / M");
        }
        void Adjust(System.Action action) { action(); flow.Save.ApplySettings(); flow.Save.SaveSettings(); flow.Click(); Show(); }
        void LateUpdate()
        {
            if (!flow || !banner) return;
            if(musicPage&&flow.State==RaceFlow.Stage.Settings&&flow.Radio&&Time.unscaledTime>=nextMusicRefresh)
            {
                nextMusicRefresh=Time.unscaledTime+.25f;
                var radio=flow.Radio;
                details.text=MusicDetails();
            }
            banner.gameObject.SetActive(!flow.MenuVisible);
            bool countdown=flow.State==RaceFlow.Stage.Countdown;
            banner.fontSize=countdown?26:20;
            banner.rectTransform.anchorMin=countdown?new Vector2(.04f,.35f):new Vector2(.2f,.88f);
            banner.rectTransform.anchorMax=countdown?new Vector2(.96f,.6f):new Vector2(.8f,.96f);
            banner.text = countdown ? flow.Race.ModeLabel + "\n"+(flow.Race.opponents?flow.Race.RosterLabel+"\n":"")+Mathf.CeilToInt(flow.CountdownRemaining) : flow.Notice ?? "";
            if(!countdown && flow.PenaltyNotice!=null)banner.text=flow.PenaltyNotice;
            songBanner.gameObject.SetActive(!flow.MenuVisible);songBanner.text=flow.Radio?.Toast??"";
            var recovery=flow.Race.vehicle.GetComponent<VehicleRespawn>();
            if(!countdown && recovery.Pending) banner.text=recovery.LastRecovery+" — race clock continues";
            if (flow.State == RaceFlow.Stage.Racing && flow.Race.Progress.Finished)
                banner.text = "Finished — AI are racing. Pause to skip waiting / estimate AI.";
            if(!countdown && flow.PenaltyNotice!=null)banner.text=flow.PenaltyNotice;
            if (flow.MenuVisible && flow.State!=RaceFlow.Stage.Title && flow.GetComponent<ExplorationMap>()?.OwnsInput!=true && EventSystem.current && !EventSystem.current.currentSelectedGameObject) EventSystem.current.SetSelectedGameObject(buttons[0].gameObject);
        }
        void OnDestroy() { if(menuActions) { menuActions.Disable(); Destroy(menuActions); } if(submitReference) Destroy(submitReference); if(previewRoot) Destroy(previewRoot); if(previewCamera) Destroy(previewCamera.gameObject); if(previewTexture) { previewTexture.Release(); Destroy(previewTexture); } }
    }
}
