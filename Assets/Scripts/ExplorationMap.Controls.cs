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
        InputAction areaPrevAction,areaNextAction,mapToggle,panAction,zoomOutAction,zoomInAction,selectAction,routeAction,waypointAction,centerAction,prevAction,nextAction,helpAction,backAction,navigateAction,trackAction,clearWaypointAction;
        // 0.76: the routes of any courses chosen under Track are drawn over the one world, several at once, remembered with
        // the map data. Choosing never loads a scene, changes the world or moves the player. During a race: that race only.
        string CurrentScene=>race.Flow.InRoamWorld?RacePlaylists.Scenes[Mathf.Clamp(RaceFlow.RoamCourse,0,RacePlaylists.Scenes.Length-1)]:gameObject.scene.name;
        List<string> ShownRoutes{get{if(!data.routesChosen)data.routes=new List<string>{CurrentScene};return data.routes??=new List<string>();}}
        List<CoursePreviewCatalog.Course> RouteCourses=>race.Flow.TrackBrowsingLocked?null:CoursePreviewCatalog.Courses.Where(c=>ShownRoutes.Contains(c.scene)).ToList();
        string RouteTitle{get{if(race.Flow.TrackBrowsingLocked)return race.courseName+"\nCurrent race\n\n";var shown=RouteCourses;return (shown.Count==0?"No race route chosen":shown.Count==1?RacePlaylists.Titles[Array.IndexOf(RacePlaylists.Scenes,shown[0].scene)]:shown.Count+" race routes")+"\nChoose routes with Track\n\n";}}
        string RouteLegend=>!courseOverlay.gameObject.activeSelf?"Race routes hidden":"Cyan main route / Gold shortcuts\nWhite bar: start / finish"+(RouteCourses?.Any(c=>c.raceOnly)==true?"\nDashed: race-only route\n(no road in Free Roam)":"");
        void ToggleCourseRoute(int course)
        {
            var routes=ShownRoutes;data.routesChosen=true;var scene=RacePlaylists.Scenes[course];bool on=!routes.Remove(scene);if(on)routes.Add(scene);
            data.routesShown=true;courseOverlay.gameObject.SetActive(true);dirty=true;Save();
            if(on){var c=CoursePreviewCatalog.Courses[course];var bounds=new Bounds(c.main[0],Vector3.zero);foreach(var p in c.main)bounds.Encapsulate(p);center=MapNormalized(bounds.center);var extent=Vector2.Scale(MapNormalized(bounds.max)-MapNormalized(bounds.min),picture.rectTransform.rect.size);zoom=Mathf.Clamp(Mathf.Min(picture.rectTransform.rect.width/Mathf.Max(1,extent.x),picture.rectTransform.rect.height/Mathf.Max(1,extent.y))*.8f,1,6);selected=-1;}
            Draw();
        }
        readonly MenuActionRegistry mapRegistry=new();
        readonly List<(MenuGlyph glyph,UnityEngine.UI.Text key,UnityEngine.UI.Text label,InputAction action)> mapPrompts=new();
        readonly List<UnityEngine.UI.Button> sheetButtons=new();
        GameObject actionSheet;bool sheetOpen;int sheetFocus;float nextNavigation;
        public IEnumerable<InputAction> Bindings {get{EnsureMapActions();return mapActions.actions;}}
        void EnsureMapActions()
        {
            if(mapActions!=null&&routeAction!=null&&routeAction.bindings.Count>0&&clearWaypointAction!=null&&areaNextAction!=null)return;mapActions?.Dispose();mapActions=new InputActionMap("World Map");
            InputAction ButtonAction(string name,string keyboard,string controller){var a=mapActions.AddAction(name,InputActionType.Button);a.AddBinding(keyboard);a.AddBinding(controller);return a;}
            mapToggle=ButtonAction("Open / close","<Keyboard>/m","<Gamepad>/select");
            selectAction=ButtonAction("Select location","<Keyboard>/space","<Gamepad>/buttonSouth");
            routeAction=ButtonAction("Show / hide race route","<Keyboard>/c","<Gamepad>/buttonWest");
            waypointAction=ButtonAction("Set / move / clear waypoint","<Keyboard>/f","<Gamepad>/buttonNorth");
            clearWaypointAction=ButtonAction("Clear waypoint","<Keyboard>/backspace","<Gamepad>/leftStickPress");
            centerAction=ButtonAction("Center on player","<Keyboard>/home","<Gamepad>/rightStickPress");
            prevAction=ButtonAction("Previous discovered destination","<Keyboard>/q","<Gamepad>/leftShoulder");
            nextAction=ButtonAction("Next discovered destination","<Keyboard>/e","<Gamepad>/rightShoulder");
            helpAction=ButtonAction("Actions / Help","<Keyboard>/enter","<Gamepad>/start");
            trackAction=ButtonAction("Select map track","<Keyboard>/t","<Gamepad>/dpad/right");
            backAction=ButtonAction("Back","<Keyboard>/escape","<Gamepad>/buttonEast");
            // 0.93 Part C: step through the acorn areas (each highlighted and centred)
            areaPrevAction=ButtonAction("Previous acorn area","<Keyboard>/pageUp","<Gamepad>/dpad/up");areaNextAction=ButtonAction("Next acorn area","<Keyboard>/pageDown","<Gamepad>/dpad/down");
            panAction=mapActions.AddAction("Pan",InputActionType.Value);panAction.AddBinding("<Gamepad>/leftStick");panAction.AddCompositeBinding("2DVector").With("Up","<Keyboard>/w").With("Down","<Keyboard>/s").With("Left","<Keyboard>/a").With("Right","<Keyboard>/d");
            zoomOutAction=ButtonAction("Zoom out","<Keyboard>/minus","<Gamepad>/leftTrigger");zoomInAction=ButtonAction("Zoom in","<Keyboard>/equals","<Gamepad>/rightTrigger");
            navigateAction=mapActions.AddAction("Actions navigation",InputActionType.Value);navigateAction.AddBinding("<Gamepad>/dpad/y");navigateAction.AddBinding("<Gamepad>/leftStick/y");navigateAction.AddCompositeBinding("1DAxis").With("Positive","<Keyboard>/upArrow").With("Negative","<Keyboard>/downArrow");mapActions.Enable();
        }
        void UpdateMapInput()
        {
            EnsureMapActions();if(!race||!race.Flow||MenuInput.Blocked||race.Flow.GetComponent<RaceMenus>()?.ModalOpen==true)return;
            if(mapToggle.WasPressedThisFrame()){MenuInput.ConsumeThroughRelease(mapToggle);if(Opened){if(sheetOpen){if(Confirming)CancelTravel();else CloseMapSheet();}else Close();}else if(race.Flow.State==RaceFlow.Stage.Racing||race.Flow.State==RaceFlow.Stage.Paused)Open();return;}
            if(!Opened)return;
            if(backAction.WasPressedThisFrame()){MenuInput.ConsumeThroughRelease(backAction);if(Confirming)CancelTravel();else if(sheetOpen)CloseMapSheet();else Close();return;}
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
            else if(routeAction.WasPressedThisFrame())ToggleRoute();else if(waypointAction.WasPressedThisFrame())ToggleWaypoint();else if(clearWaypointAction.WasPressedThisFrame())ClearWaypoint();
            else if(centerAction.WasPressedThisFrame()){center=MapNormalized(race.vehicle.Body.position);selected=-1;}
            else if(areaNextAction.WasPressedThisFrame())StepArea(1);else if(areaPrevAction.WasPressedThisFrame())StepArea(-1);
            else if(trackAction.WasPressedThisFrame())OpenMapSheet(false,false,true);else if(helpAction.WasPressedThisFrame())OpenMapSheet(false);else if(selectAction.WasPressedThisFrame())SelectReticle();
            Draw();
        }
        void SelectReticle()
        {
            selected=Enumerable.Range(0,destinations.Length).Where(i=>Discovered(destinations[i].id)&&ScreenPoint(destinations[i].position).magnitude<=28).OrderBy(i=>ScreenPoint(destinations[i].position).sqrMagnitude).DefaultIfEmpty(-1).First();
            if(selected>=0)center=MapNormalized(destinations[selected].position);OpenMapSheet(false,true);Draw();
        }
        void ToggleRoute(){courseOverlay.gameObject.SetActive(!courseOverlay.gameObject.activeSelf);data.routesShown=courseOverlay.gameObject.activeSelf;dirty=true;Save();Draw();}
        // 0.74: the waypoint goes at the cursor (the "+" in the middle of the map, moved by panning) or at the selected
        // destination - anywhere on the map, explored or not; pressing it again on the same spot clears it.
        void ToggleWaypoint(){var p=selected>=0?destinations[selected].position:MapWorldPoint(center);if(Waypoint.HasValue&&ScreenPoint(Waypoint.Value).magnitude<12&&selected<0)ClearWaypoint();else SetWaypoint(p);Draw();}
        void ClearWaypoint(){if(Waypoint.HasValue)errorMessage="Waypoint cleared.";Waypoint=null;Draw();}
        void CloseMapSheet(){sheetOpen=false;if(actionSheet)actionSheet.SetActive(false);MenuInput.ConsumeThroughRelease();EventSystem.current?.SetSelectedGameObject(null);}
        void OpenMapSheet(bool confirm,bool location=false,bool tracks=false,int focus=0)
        {
            if(tracks&&race.Flow.TrackBrowsingLocked)return;
            if(actionSheet){actionSheet.SetActive(false);Destroy(actionSheet);}sheetButtons.Clear();mapRegistry.Clear();
            actionSheet=RectUI("Map actions",panel.transform,Vector2.zero,new(730,600)).gameObject;actionSheet.AddComponent<UnityEngine.UI.Image>().color=new(.025f,.065f,.085f,.99f);
            Text("Heading",actionSheet.transform,new(0,246),new(680,70),28).text=tracks?"MAP · SHOW TRACK ROUTES":confirm?"Travel to "+destinations[pending].title+"?":location?(selected>=0?destinations[selected].title:"Map Point"):"MAP ACTIONS / HELP";
            if(!tracks)Text("Help",actionSheet.transform,new(0,169),new(670,80),18).text=confirm?"Active activity and ghost attempts will end.\nArrival safety is checked again before travel.":"Unexplored areas are dimmed.\nExplored areas may still contain acorns.\nRoads and trails remain visible when the race route is hidden.";
            void Choice(string id,string label,Action action){int index=sheetButtons.Count;var r=RectUI(id,actionSheet.transform,new(0,(tracks?190:91)-index*49),new(650,43));var bg=r.gameObject.AddComponent<UnityEngine.UI.Image>();bg.color=new(.1f,.25f,.28f);var b=r.gameObject.AddComponent<UnityEngine.UI.Button>();b.targetGraphic=bg;var colors=b.colors;colors.selectedColor=new(.2f,.8f,.7f);b.colors=colors;b.navigation=new(){mode=UnityEngine.UI.Navigation.Mode.None};Text("Label",r,Vector2.zero,new(625,41),21).text=label;var entry=mapRegistry.Register(id,label,selectAction,action);b.onClick.AddListener(()=>entry.Execute());sheetButtons.Add(b);}
            Choice("cancel","CANCEL / BACK",()=>{if(Confirming)CancelTravel();else CloseMapSheet();});
            if(tracks)
            {
                int row=1;foreach(int i in RacePlaylists.DisplayOrder){int choice=i,at=row++;if(i>=CoursePreviewCatalog.Courses.Length)continue;Choice("map-track-"+i,(ShownRoutes.Contains(RacePlaylists.Scenes[i])?"[x]  ":"[  ]  ")+RacePlaylists.Titles[i]+(CoursePreviewCatalog.Courses[i].raceOnly?"  (race-only)":""),()=>{ToggleCourseRoute(choice);OpenMapSheet(false,false,true,at);});}
            }
            else if(confirm)Choice("travel",SplitRoam.Current?"BRING BOTH PLAYERS HERE":"TRAVEL",()=>ConfirmTravel());
            else
            {
                if(location&&selected>=0)Choice("travel",(SplitRoam.Current?"Bring both players here":"Travel")+(race.FreeRoam?"":" — Free Roam only"),()=>{int target=selected;CloseMapSheet();RequestTravel(target);});
                Choice("waypoint","Set / Move / Clear Waypoint",()=>{ToggleWaypoint();CloseMapSheet();});
                Choice("route",courseOverlay.gameObject.activeSelf?"Hide Race Route":"Show Race Route",()=>{ToggleRoute();CloseMapSheet();});
                Choice("center","Center on Player",()=>{center=MapNormalized(race.vehicle.Body.position);selected=-1;CloseMapSheet();});
                Choice("clear","Clear Waypoint",()=>{ClearWaypoint();CloseMapSheet();});
                Choice("destinations","Next Discovered Destination",()=>{SelectNext(1);CloseMapSheet();});
                if(!race.Flow.TrackBrowsingLocked)Choice("tracks","Select Track",()=>OpenMapSheet(false,false,true));
            }
            sheetOpen=true;sheetFocus=Mathf.Clamp(focus,0,sheetButtons.Count-1);EventSystem.current.SetSelectedGameObject(sheetButtons[sheetFocus].gameObject);MenuInput.ConsumeThroughRelease();
        }
        void BuildMapControls()
        {
            EnsureMapActions();confirmation.SetActive(false);
            foreach(Transform child in panel.transform){if(child.GetComponent<UnityEngine.UI.Button>()||child.name=="Map controls"||child.name=="Controller controls"){child.gameObject.SetActive(false);Destroy(child.gameObject);}}
            picture.rectTransform.sizeDelta=new(930,visual?930*visual.bounds.height/visual.bounds.width:540);picture.rectTransform.anchoredPosition=new(-150,15);courseOverlay.rectTransform.sizeDelta=picture.rectTransform.sizeDelta;
            status.rectTransform.anchoredPosition=new(485,120);status.rectTransform.sizeDelta=new(235,300);status.fontSize=17;
            areaOverlay.rectTransform.sizeDelta=picture.rectTransform.sizeDelta;areaList.rectTransform.anchoredPosition=new(487,-150);
            var footer=RectUI("Map bindings",panel.transform,new(0,-302),new(1220,94));
            var items=new[]{(selectAction,"Select"),(backAction,"Back"),(routeAction,"Race route"),(waypointAction,"Waypoint"),(prevAction,"Previous"),(nextAction,"Next"),(clearWaypointAction,"Clear wpt"),(panAction,"Move cursor"),(zoomOutAction,"Zoom out"),(zoomInAction,"Zoom in"),(centerAction,"Center"),(helpAction,"Actions / Help"),(trackAction,"Track"),(areaNextAction,"Acorn area")};
            for(int i=0;i<items.Length;i++){var item=items[i];var r=RectUI(item.Item2,footer,new(-522+(i%7)*174,23-(i/7)*46),new(170,40));var icon=RectUI("Glyph",r,new(-69,0),new(54,34));var glyph=icon.gameObject.AddComponent<MenuGlyph>();glyph.raycastTarget=false;var key=Text("Key",icon,Vector2.zero,new(54,34),15);var label=Text("Action",r,new(30,0),new(112,36),16);label.text=item.Item2;mapPrompts.Add((glyph,key,label,item.Item1));var b=r.gameObject.AddComponent<UnityEngine.UI.Button>();b.navigation=new(){mode=UnityEngine.UI.Navigation.Mode.None};int n=i;b.onClick.AddListener(()=>{if(sheetOpen)return;switch(n){case 0:SelectReticle();break;case 1:Close();break;case 2:ToggleRoute();break;case 3:ToggleWaypoint();break;case 4:SelectNext(-1);break;case 5:SelectNext(1);break;case 6:ClearWaypoint();break;case 10:center=MapNormalized(race.vehicle.Body.position);selected=-1;break;case 11:OpenMapSheet(false);break;case 12:OpenMapSheet(false,false,true);break;case 13:StepArea(1);break;}Draw();});var hit=r.gameObject.AddComponent<UnityEngine.UI.Image>();hit.color=new(0,0,0,.01f);b.targetGraphic=hit;}
            RefreshMapPrompts();
        }
        void RefreshMapPrompts(){foreach(var p in mapPrompts){string binding=MenuInput.Binding(p.action);p.glyph.SetPath(binding);p.key.text=binding=="<Gamepad>/dpad/right"?"D→":binding=="<Gamepad>/dpad/down"?"D↑↓":MenuGlyph.Label(binding);if(p.action==trackAction)p.glyph.transform.parent.gameObject.SetActive(!race.Flow.TrackBrowsingLocked);}}
    }
}
