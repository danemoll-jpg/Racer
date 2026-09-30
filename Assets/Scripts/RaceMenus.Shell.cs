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
        string modalTitle,modalMessage;
        Action modalConfirm;

        GameObject lastFocus;
        bool mapWasOpen;GameObject mapCallerFocus;
        readonly List<(MenuGlyph glyph,UnityEngine.UI.Text key,UnityEngine.UI.Text label,InputAction action,string fallback)> prompts=new();
        string PageKey=>flow.State+"/"+page+(modalConfirm!=null?"/modal":"");
        public bool ModalOpen=>modalConfirm!=null||page=="keyboard";
        public bool ResumeRoot=>flow.State==RaceFlow.Stage.Paused&&page==""&&modalConfirm==null;
        static string sceneReturnPage;
        static System.Collections.Generic.Dictionary<string,(string item,float scroll)> sceneMemory;
        public void SaveSceneReturn(){CapturePage();sceneReturnPage=stagePages.TryGetValue(RaceFlow.Stage.Ready,out var p)?p:"race";sceneMemory=new(pageMemory);}
        public void RestoreSceneReturn(){page=sceneReturnPage??"race";stagePages[RaceFlow.Stage.Ready]=page;if(sceneMemory!=null)foreach(var entry in sceneMemory)pageMemory[entry.Key]=entry.Value;sceneMemory=null;Show();}
        public void OpenSetup(){page="race";stagePages[RaceFlow.Stage.Ready]=page;Show();}
        public void ResetPages(){page="";pages.Clear();stagePages.Clear();stageStacks.Clear();modalConfirm=null;}
        void Navigate(string next){CapturePage();pages.Push(page);page=next;MenuInput.ConsumeThroughRelease();Show();}
        public bool BackPage()
        {
            if(modalConfirm!=null){modalConfirm=null;MenuInput.ConsumeThroughRelease();Show();return true;}
            if(page=="folder"){FolderBack();return true;}
            if(page=="keyboard"){CloseKeyboard(false);return true;}
            if(pages.Count>0){CapturePage();page=pages.Pop();MenuInput.ConsumeThroughRelease();Show();return true;}
            if(flow.State==RaceFlow.Stage.Ready&&page!=""){page="";Show();return true;}
            return false;
        }
        void Confirm(string heading,string message,Action commit)
        {CapturePage();modalTitle=heading;modalMessage=message;modalConfirm=commit;MenuInput.ConsumeThroughRelease();Show();}
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
            adjustAction=new InputAction("Adjust",InputActionType.Value);adjustAction.AddCompositeBinding("1DAxis").With("Negative","<Keyboard>/leftArrow").With("Positive","<Keyboard>/rightArrow");adjustAction.AddBinding("<Gamepad>/dpad/x");adjustAction.Enable();
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
            AddPrompt(footer,submit,"Select","");AddPrompt(footer,cancelAction,"Back","");AddPrompt(footer,uiModule.move.action,"Navigate","");AddPrompt(footer,tabsAction,"Next tab","");
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
            if(uiModule&&flow.State!=RaceFlow.Stage.Title)uiModule.enabled=!MenuInput.Blocked;
            if(EventSystem.current)EventSystem.current.sendNavigationEvents=!MenuInput.Blocked&&flow.GetComponent<ExplorationMap>()?.OwnsInput!=true;
            for(int pi=0;pi<prompts.Count;pi++)
            {
                var p=prompts[pi];
                bool keyboard=page=="keyboard";bool tabs=flow.State==RaceFlow.Stage.Settings&&page.StartsWith("settings");
                p.glyph.transform.parent.gameObject.SetActive(pi<3||keyboard||tabs);
                if(pi==2){p.action=keyboard?deleteAction:tabs?previousTab:uiModule.move.action;p.label.text=keyboard?"Delete":tabs?"Previous tab":"Navigate";}
                if(pi==3){p.action=keyboard?spaceAction:tabsAction;p.label.text=keyboard?"Space":"Next tab";}
                string path=p.action!=null?MenuInput.Binding(p.action):MenuInput.Controller?p.fallback:"<Keyboard>/arrows";
                p.glyph.SetPath(path);p.key.text=MenuGlyph.Label(path);
            }
            bool mapOpen=flow.GetComponent<ExplorationMap>()?.OwnsInput==true;
            if(mapOpen&&!mapWasOpen)mapCallerFocus=lastFocus;
            if(!mapOpen&&mapWasOpen&&mapCallerFocus&&mapCallerFocus.activeInHierarchy)EventSystem.current.SetSelectedGameObject(mapCallerFocus);
            mapWasOpen=mapOpen;
            if(!flow.MenuVisible||mapOpen)return;
            if(flow.State==RaceFlow.Stage.Settings&&(page=="library"||page=="music")&&Time.unscaledTime>=nextMusicRefresh){nextMusicRefresh=Time.unscaledTime+.25f;details.text=page=="music"?flow.Radio.ChannelName+"\n"+flow.Radio.Song:(flow.Radio.Bundled?"Bundled music":"Custom: "+System.IO.Path.GetFileName(flow.Radio.Folder.TrimEnd('\\','/')))+"\n"+flow.Radio.Status+"\n"+flow.Radio.ScanStatus;}
            var current=EventSystem.current?.currentSelectedGameObject;
            if(current&&current!=lastFocus&&current.transform.IsChildOf(content))
            {
                Canvas.ForceUpdateCanvases();var r=current.GetComponent<RectTransform>();
                var bounds=RectTransformUtility.CalculateRelativeRectTransformBounds(scroll.viewport,r);
                var view=scroll.viewport.rect;float offset=bounds.min.y<view.yMin?view.yMin-bounds.min.y:bounds.max.y>view.yMax?view.yMax-bounds.max.y:0;
                content.anchoredPosition+=new Vector2(0,offset);lastFocus=current;
            }
        }
        void UpdateCore()
        {
            if(MenuInput.Blocked||flow.GetComponent<ExplorationMap>()?.OwnsInput==true)return;
            UpdateFolder();UpdateKeyboard();
            if(!flow.MenuVisible||modalConfirm!=null||page=="keyboard")return;
            if(flow.State==RaceFlow.Stage.Settings&&page.StartsWith("settings"))
            {
                var k=Keyboard.current;var g=Gamepad.current;
                int d=tabsAction.WasPressedThisFrame()?1:previousTab.WasPressedThisFrame()?-1:0;
                if(d!=0){string[] cats={"settings-gameplay","settings-audio","settings-display","settings-controls"};int i=Array.IndexOf(cats,page);page=cats[(i+d+4)%4];Show();MenuInput.ConsumeThroughRelease();return;}
            }
            if(adjustAction.WasPressedThisFrame())
            {
                int i=buttons.FindIndex(b=>EventSystem.current.currentSelectedGameObject==b.gameObject);
                if(adjustments.TryGetValue(i,out var change)){change(adjustAction.ReadValue<float>()<0?-1:1);MenuInput.ConsumeThroughRelease();}
            }
        }
        void ClearCore(string heading,string summary)
        {
            foreach(var b in buttons)b.gameObject.SetActive(false);adjustments.Clear();
            title.text=heading;details.text=summary;details.fontSize=20;details.GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight=string.IsNullOrEmpty(summary)?0:Mathf.Min(200,30*(summary.Count(c=>c=='\n')+1));
            details.gameObject.SetActive(!string.IsNullOrEmpty(summary));
            foreach(var b in buttons){var colors=b.colors;colors.normalColor=new(.10f,.20f,.25f);b.colors=colors;b.GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight=44;b.GetComponentInChildren<UnityEngine.UI.Text>().alignment=TextAnchor.MiddleLeft;}
        }
        void Row(int index,string id,string label,Action callback){
            var button=buttons[index];button.gameObject.SetActive(true);button.name=id;button.GetComponentInChildren<UnityEngine.UI.Text>().text=label;
            var entry=registry.Register(id,label,submit,callback,()=>button.interactable);button.onClick.RemoveAllListeners();button.onClick.AddListener(()=>entry.Execute());
        }
        void Step(int index,string id,string label,Action<int> change){Row(index,id,"‹   "+label+"   ›",()=>change(1));adjustments[index]=change;}
        RectTransform garageBody,garageProfiles;
        void LayoutGarage()
        {
            if(!garageBody)
            {
                garageBody=Rect("Garage preview and profiles",content);
                garageBody.gameObject.AddComponent<UnityEngine.UI.LayoutElement>().preferredHeight=240;
                var horizontal=garageBody.gameObject.AddComponent<UnityEngine.UI.HorizontalLayoutGroup>();horizontal.spacing=24;horizontal.childControlWidth=horizontal.childControlHeight=true;horizontal.childForceExpandWidth=false;
                garageProfiles=Rect("Vehicle profiles",garageBody);garageProfiles.gameObject.AddComponent<UnityEngine.UI.LayoutElement>().preferredWidth=280;
                var vertical=garageProfiles.gameObject.AddComponent<UnityEngine.UI.VerticalLayoutGroup>();vertical.spacing=8;vertical.childControlWidth=vertical.childControlHeight=true;vertical.childForceExpandHeight=false;
                previewTexture.Release();previewTexture.width=640;previewTexture.height=280;previewTexture.Create();
            }
            garageBody.gameObject.SetActive(true);garageBody.SetSiblingIndex(1);
            preview.transform.SetParent(garageBody,false);preview.transform.SetSiblingIndex(0);
            preview.GetComponent<UnityEngine.UI.LayoutElement>().preferredWidth=620;
            for(int i=0;i<4;i++)if(buttons[i].gameObject.activeSelf){buttons[i].transform.SetParent(garageProfiles,false);buttons[i].name="profile-"+i;buttons[i].GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight=44;}
            swatchRow.SetSiblingIndex(2);buttons[4].transform.SetSiblingIndex(3);
        }
        void ResetGarageLayout()
        {
            if(!garageBody)return;
            preview.transform.SetParent(content,false);
            foreach(var b in buttons)if(b.transform.parent==garageProfiles)b.transform.SetParent(content,false);
            garageBody.gameObject.SetActive(false);
        }
    }
}
