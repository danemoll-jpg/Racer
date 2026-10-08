using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

namespace Racer
{
    public sealed partial class RaceMenus
    {
        // Subpages share the race lifecycle stage; their caller/focus belongs to presentation.
        string page="",renderedKey="";
        readonly Dictionary<RaceFlow.Stage,string> stagePages=new();
        readonly Stack<string> pages=new();
        readonly Dictionary<string,(string item,float scroll)> pageMemory=new();
        readonly Dictionary<int,Action<int>> adjustments=new();
        readonly MenuActionRegistry registry=new();
        UnityEngine.UI.ScrollRect scroll;
        RectTransform content;
        InputSystemUIInputModule uiModule;
        InputAction cancelAction,tabsAction,adjustAction,previousTab,deleteAction,spaceAction;
        string modalTitle,modalMessage,modalConfirmLabel="CONFIRM";
        Action modalConfirm;

        GameObject lastFocus;
        bool mapWasOpen;GameObject mapCallerFocus;
        readonly List<(MenuGlyph glyph,UnityEngine.UI.Text key,UnityEngine.UI.Text label,InputAction action,string fallback)> prompts=new();
        string PageKey=>flow.State+"/"+page+(modalConfirm!=null?"/modal":"");
        public bool ModalOpen=>modalConfirm!=null||page=="keyboard";
        public bool ResumeRoot=>(flow.State==RaceFlow.Stage.Paused||(flow.State==RaceFlow.Stage.Ready&&flow.RoamMenu))&&page==""&&modalConfirm==null;
        static string sceneReturnPage;
        static System.Collections.Generic.Dictionary<string,(string item,float scroll)> sceneMemory;
        public void SaveSceneReturn(){CapturePage();sceneReturnPage=stagePages.TryGetValue(RaceFlow.Stage.Ready,out var p)?p:"race";sceneMemory=new(pageMemory);}
        public void SetSceneReturn(string target){sceneReturnPage=target;sceneMemory=null;}
        public void RestoreSceneReturn(){page=sceneReturnPage??"race";stagePages[RaceFlow.Stage.Ready]=page;if(sceneMemory!=null)foreach(var entry in sceneMemory)pageMemory[entry.Key]=entry.Value;sceneMemory=null;Show();}
        public void OpenSetup(){page="race";stagePages[RaceFlow.Stage.Ready]=page;Show();}
        public void ResetPages(){page="";renderedKey="";pages.Clear();stagePages.Clear();stageStacks.Clear();modalConfirm=null;}
        void Navigate(string next){CapturePage();pages.Push(page);page=next;MenuInput.ConsumeThroughRelease();Show();}
        public bool BackPage()
        {
            if(modalConfirm!=null){modalConfirm=null;MenuInput.ConsumeThroughRelease();Show();return true;}
            if(page=="unlock"&&UnlockNotice.Showing!=null)return true; // 0.95 Part F: the unlock panel closes with A only
            if(SplitPickBack())return true;
            if(LaterBack())return true;
            if(flow.State==RaceFlow.Stage.Ready&&page=="race"&&flow.SetupFromResults){flow.PopMenu();return true;}
            if(page=="folder"){FolderBack();return true;}
            if(page=="keyboard"){CloseKeyboard(false);return true;}
            // 0.91 Part A: B in the Shop is its Back row (it left the Garage on the Shop page, so GARAGE later opened the Shop)
            if(flow.State==RaceFlow.Stage.Garage&&page=="shop"&&pages.Count==0){ShopBack();return true;}
            if(pages.Count>0){CapturePage();page=pages.Pop();MenuInput.ConsumeThroughRelease();Show();return true;}
            if(flow.State==RaceFlow.Stage.Ready&&page!=""){page="";Show();return true;}
            return false;
        }
        void Confirm(string heading,string message,Action commit,string confirmLabel="CONFIRM")
        {CapturePage();modalTitle=heading;modalMessage=message;modalConfirm=commit;modalConfirmLabel=confirmLabel;MenuInput.ConsumeThroughRelease();Show();}
        void CapturePage()
        {
            if(renderedKey==""||!scroll)return;
            var current=EventSystem.current?.currentSelectedGameObject;
            pageMemory[renderedKey]=(current?current.name:"",scroll.verticalNormalizedPosition);
        }
        void RestorePage()
        {
            renderedKey=PageKey;
            Canvas.ForceUpdateCanvases();
            if(pageMemory.TryGetValue(renderedKey,out var state))
            {
                var match=buttons.Concat(swatches).FirstOrDefault(b=>b.gameObject.activeInHierarchy&&b.name==state.item);
                if(match)EventSystem.current.SetSelectedGameObject(match.gameObject);
                scroll.verticalNormalizedPosition=state.scroll;
            }
            else scroll.verticalNormalizedPosition=1;
            if(modalConfirm!=null)EventSystem.current.SetSelectedGameObject(buttons[0].gameObject);
            lastFocus=null;
        }
        void BuildShell()
        {
            uiModule=EventSystem.current.GetComponent<InputSystemUIInputModule>();
            cancelAction=flow.BackAction;
            tabsAction=new InputAction("Next category",InputActionType.Button,"<Keyboard>/e");tabsAction.AddBinding("<Gamepad>/rightShoulder");tabsAction.Enable();
            previousTab=new InputAction("Previous category",InputActionType.Button,"<Keyboard>/q");previousTab.AddBinding("<Gamepad>/leftShoulder");previousTab.Enable();
            deleteAction=new InputAction("Delete",InputActionType.Button,"<Keyboard>/backspace");deleteAction.AddBinding("<Gamepad>/buttonWest");deleteAction.Enable();
            spaceAction=new InputAction("Space",InputActionType.Button,"<Keyboard>/space");spaceAction.AddBinding("<Gamepad>/buttonNorth");spaceAction.Enable();
            adjustAction=new InputAction("Adjust",InputActionType.Value);adjustAction.AddCompositeBinding("1DAxis").With("Negative","<Keyboard>/leftArrow").With("Positive","<Keyboard>/rightArrow");adjustAction.AddBinding("<Gamepad>/dpad/x");
            // 0.92 Part C: the left stick and A / D change a value too
            adjustAction.AddCompositeBinding("1DAxis").With("Negative","<Keyboard>/a").With("Positive","<Keyboard>/d");adjustAction.AddBinding("<Gamepad>/leftStick/x");adjustAction.Enable();
            card.sizeDelta=new Vector2(1020,656);
            card.GetComponent<UnityEngine.UI.VerticalLayoutGroup>().enabled=false;
            title.transform.SetParent(card,false);title.rectTransform.anchorMin=new(0,1);title.rectTransform.anchorMax=new(1,1);title.rectTransform.pivot=new(.5f,1);title.rectTransform.sizeDelta=new(-64,50);title.rectTransform.anchoredPosition=new(0,-20);
            var root=Rect("Scrollable menu",card);root.anchorMin=new(0,0);root.anchorMax=new(1,1);root.offsetMin=new(32,78);root.offsetMax=new(-32,-82);
            scroll=root.gameObject.AddComponent<UnityEngine.UI.ScrollRect>();scroll.horizontal=false;scroll.scrollSensitivity=32;scroll.movementType=UnityEngine.UI.ScrollRect.MovementType.Clamped;
            var viewport=Rect("Viewport",root);Stretch(viewport,0,0,0,0);viewport.gameObject.AddComponent<UnityEngine.UI.Image>().color=new(1,1,1,.01f);viewport.gameObject.AddComponent<UnityEngine.UI.Mask>().showMaskGraphic=false;
            content=Rect("Content",viewport);content.anchorMin=new(0,1);content.anchorMax=new(1,1);content.pivot=new(.5f,1);content.sizeDelta=Vector2.zero;
            var layout=content.gameObject.AddComponent<UnityEngine.UI.VerticalLayoutGroup>();layout.spacing=8;layout.childControlWidth=layout.childControlHeight=true;layout.childForceExpandHeight=false;
            content.gameObject.AddComponent<UnityEngine.UI.ContentSizeFitter>().verticalFit=UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;
            scroll.viewport=viewport;scroll.content=content;
            foreach(var child in new Transform[]{details.transform,playlistName.transform,preview.transform}.Concat(buttons.Select(b=>b.transform)).Append(swatchRow))child.SetParent(content,false);
            // Existing branches still address this layout until their later replacement.

            help.gameObject.SetActive(false);
            var footer=Rect("Context actions",card);footer.anchorMin=new(0,0);footer.anchorMax=new(1,0);footer.pivot=new(.5f,0);footer.sizeDelta=new(-64,48);footer.anchoredPosition=new(0,16);
            var row=footer.gameObject.AddComponent<UnityEngine.UI.HorizontalLayoutGroup>();row.spacing=16;row.childControlWidth=true;row.childControlHeight=false;row.childForceExpandWidth=false;
            AddPrompt(footer,submit,"Select","");AddPrompt(footer,cancelAction,"Back","");AddPrompt(footer,NavigateAction,"Navigate","");AddPrompt(footer,tabsAction,"Next tab","");
            // 0.92 Part C: on a value row the footer leads with how to change it
            AddPrompt(footer,null,"Change","");prompts[^1].glyph.transform.parent.SetAsFirstSibling();
        }
        void AddPrompt(Transform parent,InputAction action,string label,string fallback)
        {
            var group=Rect(label,parent);group.sizeDelta=new(210,38);group.gameObject.AddComponent<UnityEngine.UI.LayoutElement>().preferredWidth=210;
            var icon=Rect("Binding graphic",group);icon.anchorMin=icon.anchorMax=new(0,.5f);icon.pivot=new(0,.5f);icon.sizeDelta=new(54,34);
            var glyph=icon.gameObject.AddComponent<MenuGlyph>();glyph.raycastTarget=false;
            var text=Label("Binding",icon,18,0);Stretch(text.rectTransform,0,0,0,0);text.alignment=TextAnchor.MiddleCenter;
            var description=Label("Action",group,20,0);Stretch(description.rectTransform,66,0,0,0);description.alignment=TextAnchor.MiddleLeft;description.text=label;
            prompts.Add((glyph,text,description,action,fallback));
        }
        void UpdateShell()
        {
            if(!scroll)return;
            foreach(var row in bindingRows){string path=row.action!=null?MenuInput.Binding(row.action):MenuInput.Controller?row.pad:row.keyboard;row.glyph.SetPath(path);row.key.text=MenuGlyph.Label(path);}
            if(uiModule&&flow.State!=RaceFlow.Stage.Title)uiModule.enabled=!MenuInput.UiBlocked;
            if(EventSystem.current)EventSystem.current.sendNavigationEvents=!MenuInput.UiBlocked&&(DeveloperLocationHud.Interactive||flow.GetComponent<ExplorationMap>()?.OwnsInput!=true);
            // 0.92 Part A: while a menu page is up the menu moves the focus itself (RaceMenus.Navigation), one row per press;
            // the UI module keeps A / B (submit) and only its own move input is set aside
            ModuleMove(!OwnsNavigation);
            UpdateFocusFrame();
            int focusRow=FocusedRow;bool valueRow=flow.MenuVisible&&adjustments.ContainsKey(focusRow),listRow=valueRow&&listRows.Contains(focusRow);
            for(int pi=0;pi<prompts.Count;pi++)
            {
                var p=prompts[pi];
                if(pi==4){p.glyph.transform.parent.gameObject.SetActive(valueRow&&page!="keyboard");string change=MenuInput.Controller?"<Gamepad>/dpad":"<Keyboard>/leftRight";p.glyph.SetPath(change);p.key.text=MenuGlyph.Label(change);continue;}
                bool keyboard=page=="keyboard";bool tabs=(flow.State==RaceFlow.Stage.Settings&&page.StartsWith("settings"))||(page==""&&(flow.State==RaceFlow.Stage.Boards||flow.State==RaceFlow.Stage.Activities||flow.State==RaceFlow.Stage.Results));bool playlist=flow.State==RaceFlow.Stage.Playlists&&page==""&&playlistDraft!=null;
                bool garage=garageView&&!keyboard;
                p.glyph.transform.parent.gameObject.SetActive((pi<3||keyboard||tabs||playlist||garage)&&!(pi==0&&valueRow&&!listRow));
                if(pi==0)p.label.text=listRow?"Choose…":"Select";
                if(pi==1)p.label.text=flow.State==RaceFlow.Stage.Results?"Main Menu":"Back";
                if(pi==2){p.action=keyboard?deleteAction:playlist?playlistAdd:tabs?previousTab:NavigateAction;p.label.text=keyboard?"Delete":playlist?"Add Race":tabs?"Previous tab":"Navigate";}
                if(pi==3){p.action=keyboard?spaceAction:playlist?playlistContext:tabsAction;p.label.text=keyboard?"Space":playlist?"Actions":"Next tab";}
                string path=p.action!=null?MenuInput.Binding(p.action):MenuInput.Controller?p.fallback:"<Keyboard>/arrows";
                // 0.76 garage: the preview turns with the right stick, Q / E or a mouse drag.
                if(pi==3&&garage&&!tabs&&!playlist){path=MenuInput.Controller?"<Gamepad>/rightStick":"<Keyboard>/q";p.label.text=MenuInput.Controller?"Rotate":"/ E  Rotate";}
                p.glyph.SetPath(path);p.key.text=MenuGlyph.Label(path);
            }
            bool mapOpen=flow.GetComponent<ExplorationMap>()?.OwnsInput==true;
            if(mapOpen&&!mapWasOpen)mapCallerFocus=lastFocus;
            if(!mapOpen&&mapWasOpen&&mapCallerFocus&&mapCallerFocus.activeInHierarchy)EventSystem.current.SetSelectedGameObject(mapCallerFocus);
            mapWasOpen=mapOpen;
            if(!flow.MenuVisible||mapOpen||DeveloperLocationHud.OwnsInput)return;
            if(flow.State==RaceFlow.Stage.Settings&&(page=="library"||page=="music")&&Time.unscaledTime>=nextMusicRefresh){nextMusicRefresh=Time.unscaledTime+.25f;details.text=page=="music"?flow.Radio.ChannelName+"\n"+flow.Radio.Song:(flow.Radio.Bundled?"Bundled music":"Custom: "+System.IO.Path.GetFileName(flow.Radio.Folder.TrimEnd('\\','/')))+"\n"+flow.Radio.Status+"\n"+flow.Radio.ScanStatus;}
            var current=EventSystem.current?.currentSelectedGameObject;
            // Focus is presentation only. Only the track row's Submit/click commits a course. 0.83: the map follows the
            // highlight; Back (no route) shows the plain map.
            if(flow.State==RaceFlow.Stage.Courses&&page==""&&current&&current.name.StartsWith("course-")
                &&int.TryParse(current.name.Substring(7),out int focusedCourse))
            {
                previewTrack=focusedCourse;ShowCourseOnMap(focusedCourse,!RoamTrackPick);
            }
            else if(flow.State==RaceFlow.Stage.Courses&&page==""&&current&&current.name=="back")ShowCourseOnMap(-1);
            // 0.89: the campaign screen's map and caption follow the highlighted event
            else if(flow.State==RaceFlow.Stage.Ready&&page=="campaign"&&current&&current.name.StartsWith("cev-")&&current!=lastFocus)ShowCampaignEvent(CampaignData.Find(current.name.Substring(4)));
            else if(flow.State==RaceFlow.Stage.Ready&&page=="campaign"&&current&&current.name.StartsWith("ccup-")&&current!=lastFocus)ShowCampaignCup(CampaignData.FindCup(current.name.Substring(5)));
            if(current&&current!=lastFocus&&current.transform.IsChildOf(content))
            {
                Canvas.ForceUpdateCanvases();var r=current.GetComponent<RectTransform>();
                var owner=current.GetComponentInParent<UnityEngine.UI.ScrollRect>()??scroll;
                var bounds=RectTransformUtility.CalculateRelativeRectTransformBounds(owner.viewport,r);
                var view=owner.viewport.rect;float offset=bounds.min.y<view.yMin?view.yMin-bounds.min.y:bounds.max.y>view.yMax?view.yMax-bounds.max.y:0;
                owner.content.anchoredPosition+=new Vector2(0,offset);lastFocus=current;
            }
        }
        void UpdateCore()
        {
            if(MenuInput.Blocked||flow.GetComponent<ExplorationMap>()?.OwnsInput==true)return;
            UpdateUnlock(); // 0.95 Part F
            UpdateNavigation();
            UpdateFolder();UpdateKeyboard();UpdateLater();
            if(!flow.MenuVisible||modalConfirm!=null||page=="keyboard")return;
            // 0.92 Part D: what loading the campaign save granted (prize vehicles, a refund), said once
            if(Campaign.LoadNotice!=null&&flow.State==RaceFlow.Stage.Ready&&(page==""||page=="campaign")){var notice=Campaign.LoadNotice;Campaign.LoadNotice=null;Confirm("CAMPAIGN UPDATED",notice+"\nEvery chapter final now awards a vehicle.",Campaign.NoticeSeen,null);return;}
            if(flow.State==RaceFlow.Stage.Settings&&page.StartsWith("settings"))
            {
                var k=Keyboard.current;var g=Gamepad.current;
                int d=tabsAction.WasPressedThisFrame()?1:previousTab.WasPressedThisFrame()?-1:0;
                if(d!=0){string[] cats={"settings-gameplay","settings-audio","settings-display","settings-controls"};int i=Array.IndexOf(cats,page);page=cats[(i+d+4)%4];Show();MenuInput.ConsumeThroughRelease();return;}
            }
            UpdateAdjust();
        }
        void ClearCore(string heading,string summary)
        {
            foreach(var b in buttons)b.gameObject.SetActive(false);adjustments.Clear();listRows.Clear();
            title.text=heading;details.text=summary;details.fontSize=20;details.color=Color.white;details.alignment=TextAnchor.UpperLeft;details.GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight=string.IsNullOrEmpty(summary)?0:Mathf.Min(200,30*(summary.Count(c=>c=='\n')+1));
            details.gameObject.SetActive(!string.IsNullOrEmpty(summary));
            foreach(var b in buttons){var colors=b.colors;colors.normalColor=new(.10f,.20f,.25f);b.colors=colors;b.GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight=44;var label=b.GetComponentInChildren<UnityEngine.UI.Text>(true);label.alignment=TextAnchor.MiddleLeft;label.fontSize=21;label.color=Color.white;label.resizeTextForBestFit=false;label.horizontalOverflow=HorizontalWrapMode.Wrap;}
        }
        void Row(int index,string id,string label,Action callback){
            EnsureRows(index+1);var button=buttons[index];button.gameObject.SetActive(true);button.name=id;button.GetComponentInChildren<UnityEngine.UI.Text>(true).text=label;
            var entry=registry.Register(id,label,submit,callback,()=>button.interactable);button.onClick.RemoveAllListeners();button.onClick.AddListener(()=>entry.Execute());
        }
        // 0.92 Part C: one way to choose everywhere. A row that holds a value shows it as ‹ value › and is changed with left /
        // right (D-pad or stick, A / D or the arrow keys, or the arrows clicked with the mouse), held to repeat. A never steps
        // it: A only opens a list to pick from where the row has one (open), else does nothing.
        readonly HashSet<int> listRows=new();
        void Step(int index,string id,string label,Action<int> change,Action open=null)
        {
            Row(index,id,label,open??(()=>{}));adjustments[index]=change;if(open!=null)listRows.Add(index);
            var row=buttons[index];var text=row.GetComponentInChildren<UnityEngine.UI.Text>(true);text.rectTransform.offsetMin=new(58,0);text.rectTransform.offsetMax=new(-58,0);
            text.resizeTextForBestFit=true;text.resizeTextMinSize=14;text.resizeTextMaxSize=Mathf.Max(14,text.fontSize);
            for(int side=-1;side<=1;side+=2){int d=side;var r=Rect(d<0?"Previous value":"Next value",row.transform);r.anchorMin=r.anchorMax=r.pivot=new(d<0?0:1,.5f);r.anchoredPosition=Vector2.zero;r.sizeDelta=new(52,0);r.anchorMin=new(d<0?0:1,0);r.anchorMax=new(d<0?0:1,1);
                var image=r.gameObject.AddComponent<UnityEngine.UI.Image>();image.color=new(1,1,1,.001f);var arrow=r.gameObject.AddComponent<UnityEngine.UI.Button>();arrow.targetGraphic=image;arrow.navigation=new UnityEngine.UI.Navigation{mode=UnityEngine.UI.Navigation.Mode.None};
                arrow.onClick.AddListener(()=>{if(MenuInput.Blocked)return;change(d);});var glyph=Label("Arrow",r,30,0);Stretch(glyph.rectTransform,0,0,0,0);glyph.alignment=TextAnchor.MiddleCenter;glyph.text=d<0?"‹":"›";glyph.color=new(.55f,1,.9f);tableCells.Add(r.gameObject);}
        }
        // An On / Off row (a value: left / right flips it).
        void Toggle(int index,string id,string label,bool on,Action flip)=>Step(index,id,label+":   "+(on?"On":"Off"),d=>flip());
        float adjustNext;int adjustHeld;
        // the menus answer only these devices while split-screen restricts them (null = every device)
        InputDevice[] menuDevices;
        int FocusedRow=>EventSystem.current?buttons.FindIndex(b=>EventSystem.current.currentSelectedGameObject==b.gameObject):-1;
        void UpdateAdjust()
        {
            float v=adjustAction.ReadValue<float>();int dir=v>.5f?1:v<-.5f?-1:0;
            if(dir!=0&&menuDevices!=null&&adjustAction.activeControl!=null&&!menuDevices.Contains(adjustAction.activeControl.device))dir=0;
            if(dir==0){adjustHeld=0;return;}
            bool first=dir!=adjustHeld;if(!first&&Time.unscaledTime<adjustNext)return;
            adjustHeld=dir;adjustNext=Time.unscaledTime+(first?.42f:.11f);
            if(adjustments.TryGetValue(FocusedRow,out var change)){change(dir);if(first)MenuInput.ConsumeThroughRelease();}
        }
        // 0.76: the preview has its own fixed panel (RaceMenus.GaragePreview); the list reads: description, the vehicles,
        // colours, Model, Rider..., Back.
        void LayoutGarage()
        {
            LayoutGarageBody();
            details.transform.SetSiblingIndex(0);int at=1;
            for(int i=0;i<4;i++)if(buttons[i].gameObject.activeSelf){buttons[i].transform.SetSiblingIndex(at++);buttons[i].name="profile-"+i;}
            if(statBlock&&statBlock.gameObject.activeSelf)statBlock.SetSiblingIndex(at++);
            if(buttons.Count>8&&buttons[8].gameObject.activeSelf)buttons[8].transform.SetSiblingIndex(at++);
            swatchRow.SetSiblingIndex(at++);
            if(buttons.Count>5&&buttons[5].gameObject.activeSelf)buttons[5].transform.SetSiblingIndex(at++);
            if(buttons.Count>6&&buttons[6].gameObject.activeSelf)buttons[6].transform.SetSiblingIndex(at++);
            if(buttons.Count>7&&buttons[7].gameObject.activeSelf)buttons[7].transform.SetSiblingIndex(at++);
            buttons[4].transform.SetSiblingIndex(at);
        }
        void LayoutGarageBody()=>EnterGarageView();
        // 0.75 garage Rider page: the live preview (framed on the rider) with Randomize and Back beside it, the option rows
        // below in the existing row style (‹ › / left-right steps, select / click steps forward). Classic models show the
        // old rider, so the page then only says that customization needs the New models.
        void RenderRider()
        {
            var look=flow.Rider;bool on=VehicleVisual.NewModels;
            ClearCore("RIDER",on?"Choose your rider. Left / right changes a row; the AI riders are random each race.":"Rider customization needs the New models.\nSet Model: New in the garage (or below) to choose your rider.");
            details.GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight=on?30:60;
            swatchRow.gameObject.SetActive(false);preview.gameObject.SetActive(true);
            if(on)
            {
                Row(0,"randomize","Randomize",flow.RandomizeRider);Row(1,"rider-back","Back",()=>BackPage());
                for(int i=0;i<RiderLook.Fields;i++){int field=i;Step(2+i,"rider-"+i,look.Label(i),d=>flow.StepRider(field,d));}
            }
            else{Step(0,"model","Model:   "+flow.ModelLabel+"   (Classic / New)",d=>flow.ToggleModel());Row(1,"rider-back","Back",()=>BackPage());}
            LayoutGarageBody();
        }
        void ResetGarageLayout(){LeaveGarageView();LeaveCourseView();}
    }
}
