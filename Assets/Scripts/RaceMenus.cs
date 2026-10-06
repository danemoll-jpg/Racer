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
    public sealed partial class RaceMenus : MonoBehaviour
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
        InputActionAsset menuActions,ownedUiActions;
        InputActionReference submitReference;
        Font font;
        UnityEngine.UI.Button simulateRemaining;
        bool waitingShown;
        // Show both available bindings: no guessed controller letter or new input scheme.
        public string CompleteRacePrompt {
            get {
                string keys=submit.GetBindingDisplayString(0);
                string pad=Gamepad.current==null?null:submit.GetBindingDisplayString(1);
                return "Press "+(string.IsNullOrEmpty(pad)?keys:pad+" / "+keys)+" to Complete Race";
            }
        }
        public bool CanSimulateRemaining => flow && flow.State==RaceFlow.Stage.Racing && flow.Race.Progress.Finished
            && !flow.Race.ClassificationFinal && flow.Race.Racers.Any(r=>r.IsAi&&!r.Classified&&!r.Dnf);
        GameObject hudPanel;
        UnityEngine.UI.RawImage preview;
        Camera previewCamera;
        RenderTexture previewTexture;
        GameObject previewRoot;
        RectTransform swatchRow;
        readonly List<UnityEngine.UI.Button> swatches=new();
        public void Initialize(RaceFlow owner)
        {
            MenuInput.Ensure(); flow = owner; font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var hud = FindAnyObjectByType<RaceHud>();
            hudPanel = hud.display.transform.parent.gameObject;
            var canvas = hud.GetComponent<Canvas>();
            if (!canvas.GetComponent<UnityEngine.UI.GraphicRaycaster>()) canvas.gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            var es = EventSystem.current;
            if (!es) es = new GameObject("Race menu EventSystem", typeof(EventSystem)).GetComponent<EventSystem>();
            var module = es.GetComponent<InputSystemUIInputModule>();
            if (!module) module = es.gameObject.AddComponent<InputSystemUIInputModule>();
            module.AssignDefaultActions();module.deselectOnBackgroundClick=false;
            // Own a clone: default modules otherwise unassign every custom reference on disable.
            ownedUiActions=Instantiate(module.actionsAsset);module.actionsAsset=ownedUiActions;
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
            for (int i = 0; i < 96; i++)
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
                button.onClick.AddListener(()=>{if(MenuInput.Blocked)return;MenuInput.ConsumeThroughRelease(submit);flow.SetColor(choice);});
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
            BuildShell();
            shown = RaceFlow.Stage.Ready;
            songBanner=Label("Current song",canvas.transform,18,0);
            songBanner.alignment=TextAnchor.MiddleCenter;
            songBanner.rectTransform.anchorMin=new Vector2(.12f,.035f);songBanner.rectTransform.anchorMax=new Vector2(.80f,.10f);
            songBanner.horizontalOverflow=HorizontalWrapMode.Overflow;
            songBanner.resizeTextForBestFit=true; songBanner.resizeTextMinSize=14; songBanner.resizeTextMaxSize=18;
            songBanner.rectTransform.offsetMin=songBanner.rectTransform.offsetMax=Vector2.zero;
            songBanner.gameObject.AddComponent<UnityEngine.UI.Outline>();
            var waitRect=Rect("Simulate remaining racers",canvas.transform);
            waitRect.anchorMin=waitRect.anchorMax=new Vector2(.5f,.79f);waitRect.sizeDelta=new Vector2(430,54);
            var waitImage=waitRect.gameObject.AddComponent<UnityEngine.UI.Image>();waitImage.color=new Color(.08f,.30f,.34f,.98f);
            simulateRemaining=waitRect.gameObject.AddComponent<UnityEngine.UI.Button>();simulateRemaining.targetGraphic=waitImage;
            var waitColors=simulateRemaining.colors;waitColors.highlightedColor=waitColors.selectedColor=new Color(.6f,1,1);simulateRemaining.colors=waitColors;
            var waitText=Label("Label",waitRect,21,0);Stretch(waitText.rectTransform,10,0,-10,0);waitText.alignment=TextAnchor.MiddleCenter;waitText.text="COMPLETE RACE";
            simulateRemaining.onClick.AddListener(()=>{if(CanSimulateRemaining)flow.Race.FinalizeUnfinishedAi();});
            waitRect.gameObject.SetActive(false);
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
            var b = buttons[index]; b.gameObject.SetActive(true); b.GetComponentInChildren<UnityEngine.UI.Text>(true).text = label;
            b.onClick.RemoveAllListeners(); b.onClick.AddListener(()=>{if(MenuInput.Blocked)return;MenuInput.ConsumeThroughRelease(submit);action();});
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
            ClearBindingRows();
            registry.Clear();
            CapturePage();
            PreparePage();
            ResetLaterLayout();ResetEntryLayout();ResetGarageLayout();
            details.transform.SetSiblingIndex(0);playlistName.transform.SetSiblingIndex(1);preview.transform.SetSiblingIndex(2);
            for(int i=0;i<buttons.Count;i++)buttons[i].transform.SetSiblingIndex(i+3);
            swatchRow.SetSiblingIndex(buttons.Count+3);
            foreach(var button in buttons)button.interactable=true;
            int selected = buttons.FindIndex(b => EventSystem.current && EventSystem.current.currentSelectedGameObject == b.gameObject);
            if(selected<0) { int swatch=swatches.FindIndex(b=>EventSystem.current && EventSystem.current.currentSelectedGameObject==b.gameObject); if(swatch>=0) selected=buttons.Count+swatch; }
            if (selected >= 0) selections[shown] = selected;
            shown = flow.State; shade.SetActive(flow.MenuVisible && shown!=RaceFlow.Stage.Title);
            simulateRemaining.gameObject.SetActive(false);waitingShown=false;
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
            foreach(var b in buttons) { b.GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight=(shown==RaceFlow.Stage.Garage || shown==RaceFlow.Stage.Results)?38:44;var label=b.GetComponentInChildren<UnityEngine.UI.Text>(true);label.supportRichText=false;label.alignment=TextAnchor.MiddleCenter; }
            details.GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight = 160;
            details.fontSize=20;
            details.supportRichText=false;
            if (shown == RaceFlow.Stage.Garage)
            {
                var profile=flow.Race.vehicle.GetComponent<VehicleConfiguration>().Profile;
                title.text=profile.Name + " / " + profile.Class;
                details.GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight=80;
                details.fontSize=18;
                details.text=$"{profile.Description}\n{(flow.Race.CarsRestricted?flow.Race.courseName+": motorcycles / ATVs only (player and AI).":flow.Race.courseName+": every vehicle available.")}\nBody color: choose a swatch below.";
                if(previewRoot) { previewRoot.SetActive(false); Destroy(previewRoot); }
                previewRoot=new GameObject("Garage display model"); previewRoot.layer=31; previewRoot.transform.position=new(10000,10000,10000); previewRoot.transform.rotation=Quaternion.Euler(0,-30,0);
                flow.Race.vehicle.GetComponent<VehicleConfiguration>().BuildPreview(previewRoot.transform);
                FramePreview(profile);
                if(page!="rider")
                {
                    // 0.81: ten vehicles - one row steps through them (left / right, or select for the next), with the class shown;
                    // the title and description above name the one in the preview.
                    var eligible=flow.Race.EligibleVehicles;int at=System.Array.FindIndex(eligible,v=>v.Id==profile.Id);
                    void StepVehicle(int d){if(eligible.Length==0)return;flow.SelectVehicle(eligible[((at<0?0:at)+d+eligible.Length)%eligible.Length].Id);}
                    adjustments.Clear();
                    Action(0,$"‹   Vehicle: {profile.Name}  ({profile.Class}, {at+1} of {eligible.Length})   ›",()=>StepVehicle(1));adjustments[0]=StepVehicle;
                    for(int k=1;k<4&&k<buttons.Count;k++)buttons[k].gameObject.SetActive(false);
                    Action(4,"Done / ready",flow.CloseGarage);
                    Action(5,"Model: "+flow.ModelLabel+"   (Classic / New)",flow.ToggleModel);
                    Action(6,"Rider…",()=>Navigate("rider"));
                }
            }
            RenderCore();
            var active = buttons.FindAll(b=>b.gameObject.activeSelf&&b.interactable);
            if(shown==RaceFlow.Stage.Garage&&page!="rider") active.AddRange(swatches);
            for (int i=0;i<active.Count;i++) active[i].navigation = new UnityEngine.UI.Navigation { mode=UnityEngine.UI.Navigation.Mode.Explicit, selectOnUp=active[(i+active.Count-1)%active.Count], selectOnDown=active[(i+1)%active.Count] };
            if(shown==RaceFlow.Stage.Garage&&page!="rider") for(int i=0;i<swatches.Count;i++) { var nav=swatches[i].navigation; nav.selectOnLeft=swatches[(i+swatches.Count-1)%swatches.Count]; nav.selectOnRight=swatches[(i+1)%swatches.Count]; swatches[i].navigation=nav; }
            ConfigureCoreFocus();ConfigureLaterFocus();
            int focus = selections.TryGetValue(shown,out var prior)?prior:0;
            if(editingPlaylistName&&!controllerName&&shown==RaceFlow.Stage.Playlists){playlistName.SetTextWithoutNotify(nameDraft);EventSystem.current.SetSelectedGameObject(playlistName.gameObject);playlistName.ActivateInputField();return;}
            if(shown==RaceFlow.Stage.Garage && page!="rider" && focus>=buttons.Count && focus<buttons.Count+swatches.Count) { EventSystem.current.SetSelectedGameObject(swatches[focus-buttons.Count].gameObject);RestorePage(); return; }
            if (focus >= buttons.Count || (!buttons[focus].gameObject.activeSelf||!buttons[focus].interactable)) focus=buttons.FindIndex(b=>b.gameObject.activeInHierarchy&&b.interactable);if(focus<0)return;
            EventSystem.current.SetSelectedGameObject(buttons[focus].gameObject);
            RestorePage();
        }
        static float NextVolume(float value) => Mathf.Min(1, (Mathf.Floor(value*10+.01f)+1)/10);
        void FinishName(bool save)
        {
            if(save){var clean=new string((nameDraft??"").Where(c=>!char.IsControl(c)).Take(64).ToArray()).Trim();if(clean.Length==0){flow.Notify("Enter a playlist name",3);Show();return;}var d=flow.Playlists.Definitions[playlistIndex];var old=d.name;d.name=clean;if(!flow.Playlists.Save()){d.name=old;Show();return;}}
            editingPlaylistName=false;nameClosedFrame=Time.frameCount;playlistName.DeactivateInputField();Show();
        }
        void Update()
        {
            if(DeveloperLocationHud.OwnsInput)return;
            UpdateCore();
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
            UpdateShell();UpdateGaragePreview();
            if(false&&musicPage&&flow.State==RaceFlow.Stage.Settings&&flow.Radio&&Time.unscaledTime>=nextMusicRefresh)
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
            bool waiting=CanSimulateRemaining&&!DeveloperLocationHud.OwnsInput;
            simulateRemaining.gameObject.SetActive(waiting);
            if(waiting)
            {
                banner.text="Finished — AI are still racing.\n"+CompleteRacePrompt;
                simulateRemaining.GetComponentInChildren<UnityEngine.UI.Text>(true).text=CompleteRacePrompt;
                Cursor.visible=true;
                if(!waitingShown&&EventSystem.current)EventSystem.current.SetSelectedGameObject(simulateRemaining.gameObject);
            }
            waitingShown=waiting;UpdateFinishPresentation();
            if(!countdown && flow.PenaltyNotice!=null)banner.text=flow.PenaltyNotice;
            if (!DeveloperLocationHud.OwnsInput && flow.MenuVisible && flow.State!=RaceFlow.Stage.Title && flow.GetComponent<ExplorationMap>()?.OwnsInput!=true && EventSystem.current && !EventSystem.current.currentSelectedGameObject) EventSystem.current.SetSelectedGameObject(buttons[0].gameObject);
        }
        void OnDestroy() { if(roamHint)Destroy(roamHint);if(finishPanel)Destroy(finishPanel);playlistAdd?.Dispose();playlistContext?.Dispose();if(ownedUiActions){ownedUiActions.Disable();Destroy(ownedUiActions);} if(textKeyboard!=null)textKeyboard.onTextInput-=TypedCharacter; tabsAction?.Dispose();adjustAction?.Dispose();previousTab?.Dispose();deleteAction?.Dispose();spaceAction?.Dispose(); if(menuActions) { menuActions.Disable(); Destroy(menuActions); } if(submitReference) Destroy(submitReference); if(previewRoot) Destroy(previewRoot); if(previewCamera) Destroy(previewCamera.gameObject); if(previewTexture) { previewTexture.Release(); Destroy(previewTexture); } }
    }
}
