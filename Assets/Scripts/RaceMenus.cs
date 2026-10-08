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
        // 0.88 Part D: the locked reward vehicle shown in the garage (null = the player's own vehicle) and its dark silhouette
        string garageLocked;static Material silhouette;
        public string GarageLockedVehicle=>garageLocked;
        static void Silhouette(Transform root)
        {
            if(!silhouette){silhouette=new Material(Shader.Find("Universal Render Pipeline/Unlit")){name="Locked vehicle silhouette"};silhouette.SetColor("_BaseColor",new Color(.16f,.17f,.19f));}
            foreach(var r in root.GetComponentsInChildren<Renderer>(true)){var m=new Material[r.sharedMaterials.Length];for(int i=0;i<m.Length;i++)m[i]=silhouette;r.sharedMaterials=m;}
        }
        // Show both available bindings: no guessed controller letter or new input scheme.
        public string CompleteRacePrompt {
            get {
                string keys=submit.GetBindingDisplayString(0);
                string pad=Gamepad.current==null?null:submit.GetBindingDisplayString(1);
                return "Press "+(string.IsNullOrEmpty(pad)?keys:pad+" / "+keys)+" to Complete Race";
            }
        }
        public bool CanSimulateRemaining => flow && !SplitScreen.Active && flow.State==RaceFlow.Stage.Racing && flow.Race.Progress.Finished
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
                var colors = button.colors; colors.normalColor = new Color(.10f,.20f,.25f); colors.highlightedColor = new Color(.13f,.27f,.32f); // 0.92 Part A: hover is not the focus look
                rect.gameObject.AddComponent<MenuHoverSelect>();
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
            registry.Clear();adjustments.Clear();listRows.Clear();minisUsed=0;ShowPrize(null);
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
            if(statBlock)statBlock.gameObject.SetActive(false);
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
            if(shown!=RaceFlow.Stage.Garage)garageLocked=null;
            previewLock=null;
            if (shown == RaceFlow.Stage.Garage && page=="shop") BuildShopPreview(); // 0.89 campaign Shop
            else if (shown == RaceFlow.Stage.Garage)
            {
                var profile=flow.Race.vehicle.GetComponent<VehicleConfiguration>().Profile;
                // 0.88 Part D: a reward vehicle not yet earned is in the list as a locked silhouette (not selectable)
                // 0.89: vehicles the campaign has not given the player are in the list too, locked, with how to get them
                var locked=garageLocked!=null&&(VehicleUnlocks.Locked(garageLocked)||Campaign.VehicleLocked(VehicleProfile.Find(garageLocked)))?VehicleProfile.Find(garageLocked):null;if(locked==null)garageLocked=null;
                var shownProfile=locked??profile;
                title.text=shownProfile.Name + " / " + shownProfile.Class+(locked!=null?"  (locked)":"");
                details.GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight=80;
                details.fontSize=18;
                details.text=locked!=null?(locked.Reward?$"LOCKED: {VehicleUnlocks.LockedText}.\nThe acorn reward: a riding mower.":$"LOCKED: {Campaign.HowToGet(locked)}.\nCampaign vehicle: buy it in the Shop or win it in an event.")
                    :$"{profile.Description}\n{(flow.Race.CarsRestricted?flow.Race.courseName+": motorcycles / ATVs only (player and AI).":flow.Race.courseName+": every vehicle available.")}\nColour: left / right on the Colour row.";
                if(previewRoot) { previewRoot.SetActive(false); Destroy(previewRoot); }
                previewRoot=new GameObject("Garage display model"); previewRoot.layer=31; previewRoot.transform.position=new(10000,10000,10000); previewRoot.transform.rotation=Quaternion.Euler(0,-30,0);
                // 0.90 Part A: every locked vehicle is a dark silhouette with a padlock and how to get it (as the mower was)
                if(locked!=null){VehicleVisual.Build(previewRoot.transform,locked);Silhouette(previewRoot.transform);swatchRow.gameObject.SetActive(false);previewLock=locked.Reward?VehicleUnlocks.LockedText:Campaign.HowToGet(locked);}
                else flow.Race.vehicle.GetComponent<VehicleConfiguration>().BuildPreview(previewRoot.transform);
                FramePreview(shownProfile);
                if(page!="rider")
                {
                    // 0.81: ten vehicles - one row steps through them (left / right, or select for the next), with the class shown;
                    // the title and description above name the one in the preview. 0.88: locked reward vehicles are in the
                    // list after the others; stepping onto one shows it locked and leaves the chosen vehicle as it was.
                    var eligible=flow.Race.PlayerVehicles;
                    var list=eligible.Concat(VehicleProfile.All.Where(v=>(VehicleUnlocks.Locked(v)||Campaign.VehicleLocked(v))&&(!flow.Race.CarsRestricted||v.Small))).ToArray();
                    int at=System.Array.FindIndex(list,v=>v.Id==shownProfile.Id);
                    void StepVehicle(int d){if(list.Length==0)return;var next=list[((at<0?0:at)+d+list.Length)%list.Length];
                        if(VehicleUnlocks.Locked(next)||Campaign.VehicleLocked(next)){garageLocked=next.Id;flow.Click();Show();Hints.LockedItem();}else{garageLocked=null;flow.SelectVehicle(next.Id);}}
                    adjustments.Clear();
                    // 0.92 Part C: the vehicle, colour and model rows change with left / right; A never steps them
                    Step(0,"profile-0",locked!=null?$"{shownProfile.Name}  (locked)   ·   {at+1} / {list.Length}":$"{shownProfile.Name}   ·   {at+1} / {list.Length}",StepVehicle);
                    for(int k=1;k<4&&k<buttons.Count;k++)buttons[k].gameObject.SetActive(false);
                    Row(4,"done","Done / ready",flow.CloseGarage);
                    Step(5,"model","Model:   "+flow.ModelLabel+"   (Classic / New)",d=>flow.ToggleModel());
                    if(locked==null){int colour=flow.SelectedColor;Step(8,"colour","Colour:   "+VehiclePaint.Name(colour),d=>flow.SetColor(VehiclePaint.Next(colour,d)));}
                    Row(6,"rider","Rider…",()=>Navigate("rider"));
                    Row(7,"garage-shop","Shop…   ("+Campaign.Money(Campaign.Current.money)+")",()=>{shopVehicle=locked!=null&&!locked.Reward?locked.Id:shopVehicle;shopFromCampaign=false;Navigate("shop");});
                    if(locked==null)ShowGarageStats(profile); // 0.82 Part D
                }
            }
            RenderCore();SplitSetupDevices();
            var active = buttons.FindAll(b=>b.gameObject.activeSelf&&b.interactable);
            // 0.91 Part A: up / down follow the rows as drawn (rows moved on screen, like CAMPAIGN and SPLIT SCREEN, were
            // skipped when this followed the row numbers)
            active.Sort((x,y)=>ScreenOrder(x.transform,y.transform));
            for (int i=0;i<active.Count;i++) active[i].navigation = new UnityEngine.UI.Navigation { mode=UnityEngine.UI.Navigation.Mode.Explicit, selectOnUp=active[(i+active.Count-1)%active.Count], selectOnDown=active[(i+1)%active.Count] };
            // 0.92 Part C: the colour swatches only show the colours (the Colour row changes it; the mouse can still click one)
            foreach(var swatch in swatches){swatch.navigation=new UnityEngine.UI.Navigation{mode=UnityEngine.UI.Navigation.Mode.None};swatch.gameObject.SetActive(swatches.IndexOf(swatch)<VehiclePaint.Count);}
            ConfigureCoreFocus();ConfigureLaterFocus();
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            {var missed=UnreachableRows();if(missed.Count>0)Debug.LogError($"MENU NAVIGATION: {PageKey}: not reachable with a controller: {string.Join(", ",missed)}");
             var order=NavigationOrderFault();if(order!=null)Debug.LogError($"MENU NAVIGATION: {PageKey}: down does not follow the drawn order: {order}");}
#endif
            int focus = selections.TryGetValue(shown,out var prior)?prior:0;
            // 0.96 Part A: once a jump event is complete, START opens the pause menu on FINISH EVENT
            if(shown==RaceFlow.Stage.Paused&&page==""&&renderedKey!=PageKey&&CampaignRun.RunBestMedal>=1&&flow.JumpEvent!=null&&buttons.Count>12&&buttons[12].gameObject.activeSelf)focus=12;
            // 0.90 Part B: until the first campaign event is finished, CAMPAIGN is the main menu's default selection
            bool campaignDefault=shown==RaceFlow.Stage.Ready&&page==""&&!flow.RoamMenu&&NewToCampaign&&buttons[10].gameObject.activeSelf&&renderedKey!=PageKey;
            if(editingPlaylistName&&!controllerName&&shown==RaceFlow.Stage.Playlists){playlistName.SetTextWithoutNotify(nameDraft);EventSystem.current.SetSelectedGameObject(playlistName.gameObject);playlistName.ActivateInputField();return;}
            // 0.92 Part B: in the split-screen garages each device works its own half (no row has the focus)
            if(SplitPickOpen){EventSystem.current.SetSelectedGameObject(null);RestorePage();EventSystem.current.SetSelectedGameObject(null);return;}
            if (focus >= buttons.Count || (!buttons[focus].gameObject.activeSelf||!buttons[focus].interactable)) focus=buttons.FindIndex(b=>b.gameObject.activeInHierarchy&&b.interactable);if(focus<0)return;
            EventSystem.current.SetSelectedGameObject(buttons[focus].gameObject);
            RestorePage();
            if(campaignDefault)EventSystem.current.SetSelectedGameObject(buttons[10].gameObject);
        }
        // Hierarchy order (the order rows are drawn top to bottom, left to right within a row group).
        static int ScreenOrder(Transform a,Transform b)
        {
            var pa=new List<int>();for(var t=a;t;t=t.parent)pa.Insert(0,t.GetSiblingIndex());
            var pb=new List<int>();for(var t=b;t;t=t.parent)pb.Insert(0,t.GetSiblingIndex());
            for(int i=0;i<Mathf.Min(pa.Count,pb.Count);i++)if(pa[i]!=pb[i])return pa[i].CompareTo(pb[i]);
            return pa.Count.CompareTo(pb.Count);
        }
        // 0.91 Part A: the visible, usable rows of the current page that D-pad / stick navigation cannot reach from the
        // first one (followed through each row's up / down / left / right links). Empty when every row can be reached.
        public List<string> UnreachableRows()
        {
            var visible=buttons.Where(b=>b&&b.gameObject.activeInHierarchy&&b.interactable&&b.transform.IsChildOf(card)).ToList();
            if(visible.Count==0)return new List<string>();
            var seen=new HashSet<UnityEngine.UI.Selectable>{visible[0]};var queue=new Queue<UnityEngine.UI.Selectable>(seen);
            while(queue.Count>0){var s=queue.Dequeue();foreach(var next in new[]{s.FindSelectableOnUp(),s.FindSelectableOnDown(),s.FindSelectableOnLeft(),s.FindSelectableOnRight()})
                if(next&&next.gameObject.activeInHierarchy&&next.interactable&&seen.Add(next))queue.Enqueue(next);}
            return visible.Where(b=>!seen.Contains(b)).Select(b=>b.name).ToList();
        }
        // 0.92 Part A: the check tests order, not only reachability: from the first row drawn, down (rows) times must visit
        // the rows in the order they are drawn (top to bottom) and come back to the first. Null when it does; else the walk.
        public string NavigationOrderFault()
        {
            var drawn=buttons.Where(b=>b&&b.gameObject.activeInHierarchy&&b.interactable&&b.transform.IsChildOf(card)&&b.navigation.mode!=UnityEngine.UI.Navigation.Mode.None).OrderBy(b=>b.transform,Drawn).ToList();
            if(drawn.Count<2||page=="keyboard"||leftPaneButtons.Count>0)return null; // the keyboard grid and the two-pane pages are walked by their own links
            var seen=new List<UnityEngine.UI.Selectable>{drawn[0]};UnityEngine.UI.Selectable at=drawn[0];
            for(int i=0;i<drawn.Count;i++){at=at?at.FindSelectableOnDown():null;seen.Add(at);}
            bool ok=true;for(int i=0;i<=drawn.Count;i++)if(seen[i]!=drawn[i%drawn.Count])ok=false;
            return ok?null:string.Join(" > ",seen.Select(s=>s?s.name:"(none)"))+"   drawn: "+string.Join(" > ",drawn.Select(b=>b.name));
        }
        public string PageName=>PageKey;
        static float NextVolume(float value) => Mathf.Min(1, (Mathf.Floor(value*10+.01f)+1)/10);
        void FinishName(bool save)
        {
            if(save){var clean=new string((nameDraft??"").Where(c=>!char.IsControl(c)).Take(64).ToArray()).Trim();if(clean.Length==0){flow.Notify("Enter a playlist name",3);Show();return;}var d=flow.Playlists.Definitions[playlistIndex];var old=d.name;d.name=clean;if(!flow.Playlists.Save()){d.name=old;Show();return;}}
            editingPlaylistName=false;nameClosedFrame=Time.frameCount;playlistName.DeactivateInputField();Show();
        }
        void Update()
        {
            if(DeveloperLocationHud.OwnsInput)return;
            UpdateCore();UpdateSplitJoin();UpdateSplitPick();
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
            UpdateShell();UpdateGaragePreview();UpdateMinis();
            if(false&&musicPage&&flow.State==RaceFlow.Stage.Settings&&flow.Radio&&Time.unscaledTime>=nextMusicRefresh)
            {
                nextMusicRefresh=Time.unscaledTime+.25f;
                var radio=flow.Radio;
                details.text=MusicDetails();
            }
            banner.gameObject.SetActive(!flow.MenuVisible&&!SplitScreen.Active); // 0.90 Part D: each half has its own
            bool countdown=flow.State==RaceFlow.Stage.Countdown;
            banner.fontSize=countdown?26:20;
            banner.rectTransform.anchorMin=countdown?new Vector2(.04f,.35f):new Vector2(.2f,.88f);
            banner.rectTransform.anchorMax=countdown?new Vector2(.96f,.6f):new Vector2(.8f,.96f);
            banner.text = countdown ? (CampaignRun.Active!=null?"CAMPAIGN: "+CampaignRun.Active.Name.ToUpperInvariant()+"\n":"")+flow.Race.ModeLabel + "\n"+(flow.Race.opponents?flow.Race.RosterLabel+"\n":"")+Mathf.CeilToInt(flow.CountdownRemaining) : flow.Notice ?? "";
            if(!countdown && flow.PenaltyNotice!=null)banner.text=flow.PenaltyNotice;
            songBanner.gameObject.SetActive(!flow.MenuVisible);songBanner.text=flow.Radio?.Toast??"";
            var recovery=flow.Race.vehicle.GetComponent<VehicleRespawn>();
            if(!countdown && recovery.Pending) banner.text=recovery.LastRecovery+" — race clock continues";
            bool waiting=CanSimulateRemaining&&!DeveloperLocationHud.OwnsInput&&!WinnerShot.Active;
            simulateRemaining.gameObject.SetActive(waiting);
            if(waiting)
            {
                banner.text="Finished — AI are still racing.\n"+CompleteRacePrompt;
                simulateRemaining.GetComponentInChildren<UnityEngine.UI.Text>(true).text=CompleteRacePrompt;
                Cursor.visible=true;
                if(!waitingShown&&EventSystem.current)EventSystem.current.SetSelectedGameObject(simulateRemaining.gameObject);
            }
            waitingShown=waiting;UpdateFinishPresentation();UpdateControlsCard();UpdateHint();
            if(flow.ControlsCard)banner.text="";
            if(WinnerShot.Active&&!SplitScreen.Active&&CampaignRun.Active!=null)banner.text="WINNER\n"+WinnerShot.LastWinner; // 0.94 Part A; 0.95 Part C: the campaign only
            if(!countdown && flow.PenaltyNotice!=null)banner.text=flow.PenaltyNotice;
            if (!DeveloperLocationHud.OwnsInput && flow.MenuVisible && flow.State!=RaceFlow.Stage.Title && !SplitPickOpen && flow.GetComponent<ExplorationMap>()?.OwnsInput!=true && EventSystem.current && !EventSystem.current.currentSelectedGameObject) EventSystem.current.SetSelectedGameObject(buttons[0].gameObject);
        }
        void OnDestroy() { if(roamHint)Destroy(roamHint);if(finishPanel)Destroy(finishPanel);playlistAdd?.Dispose();playlistContext?.Dispose();if(ownedUiActions){ownedUiActions.Disable();Destroy(ownedUiActions);} if(textKeyboard!=null)textKeyboard.onTextInput-=TypedCharacter; tabsAction?.Dispose();adjustAction?.Dispose();previousTab?.Dispose();deleteAction?.Dispose();spaceAction?.Dispose(); if(menuActions) { menuActions.Disable(); Destroy(menuActions); } if(submitReference) Destroy(submitReference); if(previewRoot) Destroy(previewRoot); if(previewCamera) Destroy(previewCamera.gameObject); if(previewTexture) { previewTexture.Release(); Destroy(previewTexture); } }
    }
}
