using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

namespace Racer
{
    public sealed partial class ExplorationMap
    {
        InputActionMap mapActions;
        InputAction mapToggle,panAction,zoomOutAction,zoomInAction,selectAction,routeAction,waypointAction,centerAction,prevAction,nextAction,helpAction,backAction,navigateAction;
        readonly MenuActionRegistry mapRegistry=new();
        readonly List<(MenuGlyph glyph,UnityEngine.UI.Text key,UnityEngine.UI.Text label,InputAction action)> mapPrompts=new();
        readonly List<UnityEngine.UI.Button> sheetButtons=new();
        GameObject actionSheet;bool sheetOpen;int sheetFocus;float nextNavigation;
        public IEnumerable<InputAction> Bindings {get{EnsureMapActions();return mapActions.actions;}}
        void EnsureMapActions()
        {
            if(mapActions!=null&&routeAction!=null&&routeAction.bindings.Count>0)return;mapActions?.Dispose();mapActions=new InputActionMap("World Map");
            InputAction ButtonAction(string name,string keyboard,string controller){var a=mapActions.AddAction(name,InputActionType.Button);a.AddBinding(keyboard);a.AddBinding(controller);return a;}
            mapToggle=ButtonAction("Open / close","<Keyboard>/m","<Gamepad>/select");
            selectAction=ButtonAction("Select location","<Keyboard>/space","<Gamepad>/buttonSouth");
            routeAction=ButtonAction("Show / hide race route","<Keyboard>/c","<Gamepad>/buttonWest");
            waypointAction=ButtonAction("Set / move / clear waypoint","<Keyboard>/f","<Gamepad>/buttonNorth");
            centerAction=ButtonAction("Center on player","<Keyboard>/home","<Gamepad>/rightStickPress");
            prevAction=ButtonAction("Previous discovered destination","<Keyboard>/q","<Gamepad>/leftShoulder");
            nextAction=ButtonAction("Next discovered destination","<Keyboard>/e","<Gamepad>/rightShoulder");
            helpAction=ButtonAction("Actions / Help","<Keyboard>/enter","<Gamepad>/start");
            backAction=ButtonAction("Back","<Keyboard>/escape","<Gamepad>/buttonEast");
            panAction=mapActions.AddAction("Pan",InputActionType.Value);panAction.AddBinding("<Gamepad>/leftStick");panAction.AddCompositeBinding("2DVector").With("Up","<Keyboard>/w").With("Down","<Keyboard>/s").With("Left","<Keyboard>/a").With("Right","<Keyboard>/d");
            zoomOutAction=ButtonAction("Zoom out","<Keyboard>/minus","<Gamepad>/leftTrigger");zoomInAction=ButtonAction("Zoom in","<Keyboard>/equals","<Gamepad>/rightTrigger");
            navigateAction=mapActions.AddAction("Actions navigation",InputActionType.Value);navigateAction.AddBinding("<Gamepad>/dpad/y");navigateAction.AddBinding("<Gamepad>/leftStick/y");navigateAction.AddCompositeBinding("1DAxis").With("Positive","<Keyboard>/upArrow").With("Negative","<Keyboard>/downArrow");mapActions.Enable();
        }
        void UpdateMapInput()
        {
            EnsureMapActions();if(!race||!race.Flow||MenuInput.Blocked||race.Flow.GetComponent<RaceMenus>()?.ModalOpen==true)return;
            if(mapToggle.WasPressedThisFrame()){if(Opened){if(sheetOpen){if(Confirming)CancelTravel();else CloseMapSheet();}else Close();}else if(race.Flow.State==RaceFlow.Stage.Racing||race.Flow.State==RaceFlow.Stage.Paused)Open();return;}
            if(!Opened)return;
            if(backAction.WasPressedThisFrame()){if(Confirming)CancelTravel();else if(sheetOpen)CloseMapSheet();else Close();return;}
            if(sheetOpen)
            {
                float y=navigateAction.ReadValue<float>();if(Mathf.Abs(y)>.5f&&Time.unscaledTime>=nextNavigation){sheetFocus=(sheetFocus+(y>0?-1:1)+sheetButtons.Count)%sheetButtons.Count;EventSystem.current.SetSelectedGameObject(sheetButtons[sheetFocus].gameObject);nextNavigation=Time.unscaledTime+.2f;}if(Mathf.Abs(y)<.2f)nextNavigation=0;
                if(selectAction.WasPressedThisFrame())sheetButtons[sheetFocus].onClick.Invoke();RefreshMapPrompts();return;
            }
            Vector2 pan=panAction.ReadValue<Vector2>();if(pan.sqrMagnitude>.04f){center+=pan*Time.unscaledDeltaTime*.4f/zoom;selected=-1;}
            zoom=Mathf.Clamp(zoom+(zoomInAction.ReadValue<float>()-zoomOutAction.ReadValue<float>())*Time.unscaledDeltaTime*3,1,6);
            // Keep the player and edge landmarks reachable at every zoom.
            center=new(Mathf.Clamp01(center.x),Mathf.Clamp01(center.y));
            if(prevAction.WasPressedThisFrame())SelectNext(-1);else if(nextAction.WasPressedThisFrame())SelectNext(1);
            else if(routeAction.WasPressedThisFrame())ToggleRoute();else if(waypointAction.WasPressedThisFrame())ToggleWaypoint();
            else if(centerAction.WasPressedThisFrame()){center=MapNormalized(race.vehicle.Body.position);selected=-1;}
            else if(helpAction.WasPressedThisFrame())OpenMapSheet(false);else if(selectAction.WasPressedThisFrame())SelectReticle();
            Draw();
        }
        void SelectReticle()
        {
            selected=Enumerable.Range(0,destinations.Length).Where(i=>Discovered(destinations[i].id)&&ScreenPoint(destinations[i].position).magnitude<=28).OrderBy(i=>ScreenPoint(destinations[i].position).sqrMagnitude).DefaultIfEmpty(-1).First();
            if(selected>=0)center=MapNormalized(destinations[selected].position);OpenMapSheet(false,true);Draw();
        }
        void ToggleRoute(){courseOverlay.gameObject.SetActive(!courseOverlay.gameObject.activeSelf);Draw();}
        void ToggleWaypoint(){var p=selected>=0?destinations[selected].position:MapWorldPoint(center);if(Waypoint.HasValue&&Vector2.Distance(new(Waypoint.Value.x,Waypoint.Value.z),new(p.x,p.z))<1)Waypoint=null;else SetWaypoint(p);Draw();}
        void CloseMapSheet(){sheetOpen=false;if(actionSheet)actionSheet.SetActive(false);MenuInput.ConsumeThroughRelease();EventSystem.current?.SetSelectedGameObject(null);}
        void OpenMapSheet(bool confirm,bool location=false)
        {
            if(actionSheet){actionSheet.SetActive(false);Destroy(actionSheet);}sheetButtons.Clear();mapRegistry.Clear();
            actionSheet=RectUI("Map actions",panel.transform,Vector2.zero,new(730,600)).gameObject;actionSheet.AddComponent<UnityEngine.UI.Image>().color=new(.025f,.065f,.085f,.99f);
            Text("Heading",actionSheet.transform,new(0,246),new(680,70),28).text=confirm?"Travel to "+destinations[pending].title+"?":location?(selected>=0?destinations[selected].title:"Map Point"):"MAP ACTIONS / HELP";
            Text("Help",actionSheet.transform,new(0,169),new(670,80),18).text=confirm?"Active activity and ghost attempts will end.\nArrival safety is checked again before travel.":"Unexplored areas are dimmed.\nExplored areas may still contain acorns.\nRoads and trails remain visible when the race route is hidden.";
            void Choice(string id,string label,Action action){int index=sheetButtons.Count;var r=RectUI(id,actionSheet.transform,new(0,91-index*49),new(650,43));var bg=r.gameObject.AddComponent<UnityEngine.UI.Image>();bg.color=new(.1f,.25f,.28f);var b=r.gameObject.AddComponent<UnityEngine.UI.Button>();b.targetGraphic=bg;var colors=b.colors;colors.selectedColor=new(.2f,.8f,.7f);b.colors=colors;b.navigation=new(){mode=UnityEngine.UI.Navigation.Mode.None};Text("Label",r,Vector2.zero,new(625,41),21).text=label;var entry=mapRegistry.Register(id,label,selectAction,action);b.onClick.AddListener(()=>entry.Execute());sheetButtons.Add(b);}
            Choice("cancel","CANCEL / BACK",()=>{if(Confirming)CancelTravel();else CloseMapSheet();});
            if(confirm)Choice("travel","TRAVEL",()=>ConfirmTravel());
            else
            {
                if(location&&selected>=0)Choice("travel","Travel"+(race.FreeRoam?"":" — Free Roam only"),()=>{int target=selected;CloseMapSheet();RequestTravel(target);});
                Choice("waypoint","Set / Move / Clear Waypoint",()=>{ToggleWaypoint();CloseMapSheet();});
                Choice("route",courseOverlay.gameObject.activeSelf?"Hide Race Route":"Show Race Route",()=>{ToggleRoute();CloseMapSheet();});
                Choice("center","Center on Player",()=>{center=MapNormalized(race.vehicle.Body.position);selected=-1;CloseMapSheet();});
                Choice("clear","Clear Waypoint",()=>{Waypoint=null;CloseMapSheet();});
                Choice("destinations","Next Discovered Destination",()=>{SelectNext(1);CloseMapSheet();});
            }
            sheetOpen=true;sheetFocus=0;EventSystem.current.SetSelectedGameObject(sheetButtons[0].gameObject);MenuInput.ConsumeThroughRelease();
        }
        void BuildMapControls()
        {
            EnsureMapActions();confirmation.SetActive(false);
            foreach(Transform child in panel.transform){if(child.GetComponent<UnityEngine.UI.Button>()||child.name=="Map controls"||child.name=="Controller controls"){child.gameObject.SetActive(false);Destroy(child.gameObject);}}
            picture.rectTransform.sizeDelta=new(930,visual?930*visual.bounds.height/visual.bounds.width:540);picture.rectTransform.anchoredPosition=new(-150,15);courseOverlay.rectTransform.sizeDelta=picture.rectTransform.sizeDelta;
            status.rectTransform.anchoredPosition=new(485,25);status.rectTransform.sizeDelta=new(235,500);status.fontSize=18;
            var footer=RectUI("Map bindings",panel.transform,new(0,-302),new(1220,94));
            var items=new[]{(selectAction,"Select"),(backAction,"Back"),(routeAction,"Race route"),(waypointAction,"Waypoint"),(prevAction,"Previous"),(nextAction,"Next"),(panAction,"Pan"),(zoomOutAction,"Zoom out"),(zoomInAction,"Zoom in"),(centerAction,"Center"),(helpAction,"Actions / Help")};
            for(int i=0;i<items.Length;i++){var item=items[i];var r=RectUI(item.Item2,footer,new(-510+(i%6)*204,23-(i/6)*46),new(195,40));var icon=RectUI("Glyph",r,new(-69,0),new(54,34));var glyph=icon.gameObject.AddComponent<MenuGlyph>();glyph.raycastTarget=false;var key=Text("Key",icon,Vector2.zero,new(54,34),15);var label=Text("Action",r,new(30,0),new(125,36),18);label.text=item.Item2;mapPrompts.Add((glyph,key,label,item.Item1));var b=r.gameObject.AddComponent<UnityEngine.UI.Button>();b.navigation=new(){mode=UnityEngine.UI.Navigation.Mode.None};int n=i;b.onClick.AddListener(()=>{if(sheetOpen)return;switch(n){case 0:SelectReticle();break;case 1:Close();break;case 2:ToggleRoute();break;case 3:ToggleWaypoint();break;case 4:SelectNext(-1);break;case 5:SelectNext(1);break;case 9:center=MapNormalized(race.vehicle.Body.position);selected=-1;break;case 10:OpenMapSheet(false);break;}Draw();});var hit=r.gameObject.AddComponent<UnityEngine.UI.Image>();hit.color=new(0,0,0,.01f);b.targetGraphic=hit;}
            RefreshMapPrompts();
        }
        void RefreshMapPrompts(){foreach(var p in mapPrompts){string binding=MenuInput.Binding(p.action);p.glyph.SetPath(binding);p.key.text=MenuGlyph.Label(binding);}}
    }
}
