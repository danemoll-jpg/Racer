using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

namespace Racer
{
    // World coordinates and stable discovery IDs are shared by all four courses.
    public sealed partial class ExplorationMap : MonoBehaviour, IPointerClickHandler, IDragHandler, IScrollHandler
    {
        public const string Compatibility="woodstock-world-v2";
        public const int Columns=110, Rows=80;
        public static readonly Rect World=new(-900,-800,2200,1600);
        [Serializable] public sealed class Destination { public string id,title; public Vector3 position; public float yaw; }
        [Serializable] public sealed class Data { public int version=1; public string world=Compatibility; public List<int> visited=new(); public List<string> landmarks=new(); public List<string> routes=new(); public bool routesChosen,routesShown; }
        public Destination[] destinations=Array.Empty<Destination>();
        public Texture2D terrain;
        WorldMapVisual visual;WorldMapCourseOverlay courseOverlay;
        Vector2 MapNormalized(Vector3 p)=>visual?visual.Normalized(p):Normalized(p);
        Vector3 MapWorldPoint(Vector2 uv)=>visual?visual.WorldPoint(uv):WorldPoint(uv);
        Data data=new(); readonly HashSet<int> visited=new(); RaceDirector race; string path,error;
        Vector3 previous; bool sampled,dirty,resume; float nextReveal,nextSave,zoom=1; Vector2 center=new(.5f,.5f);
        GameObject panel; UnityEngine.UI.RawImage picture; UnityEngine.UI.Text status,heading,waypointLabel;
        readonly List<UnityEngine.UI.Text> markers=new(); Texture2D texture; int selected=-1;
        readonly List<UnityEngine.UI.Text> acorns=new();
        // 0.93 Part C: the acorn areas drawn on the map (regions, their names and counts), the places their directions name, the
        // list of areas in the side column (the chosen one highlighted and centred); 0.93 Part D: a fixed north arrow
        WorldMapAreaOverlay areaOverlay;readonly List<(UnityEngine.UI.Text text,AcornAreas.Area area,AcornAreas.Region region)> areaLabels=new();
        readonly List<(UnityEngine.UI.Text text,Vector3 at)> placeLabels=new();UnityEngine.UI.Text areaList,northArrow;int selectedArea=-1;
        ExplorationCollection Collection=>race?race.GetComponent<ExplorationCollection>():null;
        AcornAreas.Area[] Areas=>AcornAreas.Of(Collection);
        int closedFrame=-1;
        GameObject confirmation; UnityEngine.UI.Text confirmationText; int pending=-1,confirmationFrame=-1;
        public bool Confirming=>pending>=0;
        public string TravelMessage=>errorMessage;
        public bool Opened=>panel&&panel.activeSelf;
        public bool OwnsInput=>Opened||Time.frameCount==closedFrame;
        public Vector3? Waypoint {get;private set;}
        public int RevealedCount=>visited.Count;
        public bool Discovered(string id)=>data.landmarks.Contains(id);
        public void Initialize(RaceDirector owner,string root)
        {
            race=owner;error=null;path=Path.Combine(root,"exploration-map-"+Compatibility+".json");
            try {if(File.Exists(path))data=JsonUtility.FromJson<Data>(File.ReadAllText(path))??new();
                if(data.world!=Compatibility||data.version!=1)throw new IOException("Incompatible map data retained");
                data.visited??=new();data.landmarks??=new();foreach(int cell in data.visited)if(cell>=0&&cell<Columns*Rows)visited.Add(cell);
                foreach(var d in destinations)
                    if(d.id.StartsWith("property-",StringComparison.Ordinal)&&Visited(d.position)&&!data.landmarks.Contains(d.id))
                    { data.landmarks.Add(d.id); dirty=true; }
            } catch(Exception e){error=e.Message;}
        }
        public static Vector2 Normalized(Vector3 p)=>new((p.x-World.xMin)/World.width,(p.z-World.yMin)/World.height);
        public static Vector3 WorldPoint(Vector2 uv)=>new(World.xMin+uv.x*World.width,0,World.yMin+uv.y*World.height);
        public bool Visited(Vector3 p){var uv=Normalized(p);return uv.x>=0&&uv.x<1&&uv.y>=0&&uv.y<1&&visited.Contains(Mathf.FloorToInt(uv.y*Rows)*Columns+Mathf.FloorToInt(uv.x*Columns));}
        public void ResetMovement(){sampled=false;previous=race.vehicle.Body.position;nextReveal=Time.time+1;}
        void FixedUpdate()
        {
            if(DeveloperLocationHud.Inspecting){sampled=false;return;}
            if(!race||race.Flow.State!=RaceFlow.Stage.Racing)return;
            var p=race.vehicle.Body.position;
            if(!sampled){previous=p;sampled=true;return;}
            bool continuous=Vector3.Distance(previous,p)<=Mathf.Max(3,race.vehicle.Body.linearVelocity.magnitude*Time.fixedDeltaTime*2+.3f);previous=p;
            if(!continuous){nextReveal=Time.time+1;return;}
            if(Time.time<nextReveal)return;nextReveal=Time.time+.4f;
            Reveal(p);if(dirty&&Time.unscaledTime>nextSave){SaveInBackground();nextSave=Time.unscaledTime+3;}
        }
        public void Reveal(Vector3 p)
        {
            if(error!=null)return;var uv=Normalized(p);int cx=Mathf.FloorToInt(uv.x*Columns),cy=Mathf.FloorToInt(uv.y*Rows);
            for(int y=Mathf.Max(0,cy-3);y<=Mathf.Min(Rows-1,cy+3);y++)for(int x=Mathf.Max(0,cx-3);x<=Mathf.Min(Columns-1,cx+3);x++)
                if(Vector2.Distance(new(World.xMin+(x+.5f)*20,World.yMin+(y+.5f)*20),new(p.x,p.z))<=48)dirty|=visited.Add(y*Columns+x);
            foreach(var d in destinations)if(Vector3.Distance(d.position,p)<45&&!Discovered(d.id)){data.landmarks.Add(d.id);dirty=true;}
        }
        // 0.82 Part C: the periodic save while exploring writes the file on a worker thread (the main-thread write was a
        // hitch every 3 s in new land); one write at a time, and Save() (scene exit, pause, menu) waits for it first.
        System.Threading.Tasks.Task pendingWrite;string backgroundError;
        void SaveInBackground()
        {
            if(!dirty||error!=null||(pendingWrite!=null&&!pendingWrite.IsCompleted))return;
            data.visited=visited.OrderBy(i=>i).ToList();string json=JsonUtility.ToJson(data),target=path;dirty=false;
            pendingWrite=System.Threading.Tasks.Task.Run(()=>{try{AtomicSave.Write(target,json);}catch(Exception e){backgroundError=e.Message;}});
        }
        public void Save(){try{pendingWrite?.Wait();}catch(Exception){}if(backgroundError!=null&&error==null)error=backgroundError;if(!dirty||error!=null)return;try{data.visited=visited.OrderBy(i=>i).ToList();AtomicSave.Write(path,JsonUtility.ToJson(data));dirty=false;}catch(Exception e){error=e.Message;}}
        void OnApplicationPause(bool paused){if(paused)Save();}
        void OnDestroy(){Save();mapActions?.Dispose();mapActions=null;if(texture)Destroy(texture);if(panel)Destroy(panel);mapPrompts.Clear();}
        void Update()=>UpdateMapInput();
        public void Open()
        {
            MenuInput.ConsumeThroughRelease();
            if(!visual)visual=Resources.Load<WorldMapVisual>("WorldMaps/PermanentWorld");
            resume=race.Flow.State==RaceFlow.Stage.Racing;if(resume)race.Flow.Pause();if(!panel){BuildUI();BuildMapControls();}
            center=MapNormalized(race.vehicle.Body.position);selected=-1;selectedArea=-1;panel.SetActive(true);EventSystem.current?.SetSelectedGameObject(null);Repaint();Draw();Save();
        }
        // 0.93 Part C: open the map on one acorn area (from the Exploration page's list)
        public void OpenArea(int index){Open();SelectArea(index);}
        void SelectArea(int index)
        {
            var all=Areas;if(index<0||index>=all.Length){selectedArea=-1;Draw();return;}
            selectedArea=index;selected=-1;var b=all[index].bounds;center=MapNormalized(b.center);
            var extent=Vector2.Scale(MapNormalized(b.max)-MapNormalized(b.min),picture.rectTransform.rect.size);
            zoom=Mathf.Clamp(Mathf.Min(picture.rectTransform.rect.width/Mathf.Max(1,extent.x),picture.rectTransform.rect.height/Mathf.Max(1,extent.y))*.8f,1,6);Draw();
        }
        void StepArea(int d){int n=Areas.Length;if(n==0)return;SelectArea(selectedArea<0?(d>0?0:n-1):(selectedArea+d+n)%n);}
        public void Close(){MenuInput.ConsumeThroughRelease();CancelTravel();closedFrame=Time.frameCount;if(panel)panel.SetActive(false);Save();if(resume)race.Flow.Resume();}
        // 0.74: a custom destination anywhere in the world (map cursor, mouse click or a destination): the ground height under
        // it is found here. Drive-to only (no travel); not saved; shown in Free Roam only (WaypointGuide).
        public void SetWaypoint(Vector3 p)
        {
            float y=p.y;foreach(var h in Physics.RaycastAll(new Vector3(p.x,700,p.z),Vector3.down,1400,~0,QueryTriggerInteraction.Ignore).OrderBy(h=>h.distance)){if(h.collider.isTrigger||h.collider.attachedRigidbody)continue;var n=h.collider.name;if(n.IndexOf("trunk",StringComparison.OrdinalIgnoreCase)>=0||n.IndexOf("tree",StringComparison.OrdinalIgnoreCase)>=0)continue;y=h.point.y;break;}
            Waypoint=new Vector3(p.x,y,p.z);errorMessage=Visited(p)?"Waypoint set.":"Waypoint set in unexplored land.";
            if(!GetComponent<WaypointGuide>())gameObject.AddComponent<WaypointGuide>();
        }
        public void ClearWaypointFromGuide(){Waypoint=null;}
        void SelectNext(int direction){var known=Enumerable.Range(0,destinations.Length).Where(i=>Discovered(destinations[i].id)).ToArray();if(known.Length==0){selected=-1;errorMessage="Discover landmarks while exploring.";return;}int at=Array.IndexOf(known,selected);selected=known[(at+direction+known.Length)%known.Length];center=MapNormalized(destinations[selected].position);Draw();}
        public bool Travel(int index)
        {
            if(error!=null||!race.FreeRoam||index<0||index>=destinations.Length||!Discovered(destinations[index].id)){errorMessage="Travel needs free roam and a discovered destination.";return false;}
            var d=destinations[index];var recovery=race.vehicle.GetComponent<VehicleRespawn>();
            if(!recovery.TryFastTravel(d.position,Quaternion.Euler(0,d.yaw,0))){errorMessage="Arrival blocked or unsupported. Try again after traffic clears.";return false;}
            race.Flow.Activities.NewSession();race.Flow.Ghost.ResetSession();race.GetComponent<ExplorationCollection>()?.ResetMovement();
            race.Racers[0].Branch.Clear();race.Racers[0].FinishArmed=false;race.Racers[0].FinishApproach=0;
            race.ResetSampling(race.vehicle.Body.position,Time.timeAsDouble);ResetMovement();
            errorMessage="Arrived at "+d.title+". Active attempts cancelled.";center=MapNormalized(race.vehicle.Body.position);Save();return true;
        }
        string errorMessage="";
        void TravelSelected(){RequestTravel(selected);}
        public void RequestTravel(int index)
        {
            if(!Opened)return;
            if(index<0||index>=destinations.Length||!race.FreeRoam||!Discovered(destinations[index].id)){errorMessage="Travel needs free roam and a discovered destination.";Draw();return;}
            pending=index;confirmationFrame=Time.frameCount;confirmationText.text="Travel to "+destinations[index].title+"?\nActive attempts will end.";confirmation.SetActive(true);OpenMapSheet(true);
        }
        public void CancelTravel(){MenuInput.ConsumeThroughRelease();pending=-1;CloseMapSheet();if(confirmation)confirmation.SetActive(false);EventSystem.current?.SetSelectedGameObject(null);}
        public bool ConfirmTravel()
        {
            if(pending<0)return false;int index=pending;CancelTravel();
            if(!Travel(index)){Draw();return false;}
            string title=destinations[index].title;resume=true;Close();race.Flow.Notify("Arrived at "+title,4);return true;
        }
        RectTransform RectUI(string name,Transform parent,Vector2 position,Vector2 size)
        {var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(parent,false);r.anchorMin=r.anchorMax=r.pivot=new(.5f,.5f);r.anchoredPosition=position;r.sizeDelta=size;return r;}
        UnityEngine.UI.Text Text(string name,Transform parent,Vector2 p,Vector2 size,int fontSize)
        {var t=RectUI(name,parent,p,size).gameObject.AddComponent<UnityEngine.UI.Text>();t.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");t.fontSize=fontSize;t.color=Color.white;t.alignment=TextAnchor.MiddleCenter;t.raycastTarget=false;return t;}
        void Button(string text,Vector2 p,Action action)
        {var r=RectUI(text,panel.transform,p,new(240,38));r.gameObject.AddComponent<UnityEngine.UI.Image>().color=new(.1f,.29f,.31f);var b=r.gameObject.AddComponent<UnityEngine.UI.Button>();b.navigation=new UnityEngine.UI.Navigation{mode=UnityEngine.UI.Navigation.Mode.None};b.onClick.AddListener(()=>{action();EventSystem.current?.SetSelectedGameObject(null);});Text(text,r,Vector2.zero,new(232,35),18).text=text;}
        void BuildUI()
        {
            panel=new GameObject("Whole area map",typeof(RectTransform),typeof(Canvas),typeof(UnityEngine.UI.CanvasScaler),typeof(UnityEngine.UI.GraphicRaycaster));
            var canvas=panel.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=100;
            var scaler=panel.GetComponent<UnityEngine.UI.CanvasScaler>();scaler.uiScaleMode=UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new(1280,720);
            var bg=RectUI("Map background",panel.transform,Vector2.zero,new(1280,720));bg.gameObject.AddComponent<UnityEngine.UI.Image>().color=new(.025f,.045f,.055f,1);
            Text("Title",panel.transform,new(0,323),new(1200,42),26).text="WOODSTOCK / EXPLORATION MAP";
            var r=RectUI("Terrain",panel.transform,new(-150,10),new(740,visual?740*visual.bounds.height/visual.bounds.width:540));picture=r.gameObject.AddComponent<UnityEngine.UI.RawImage>();r.gameObject.AddComponent<UnityEngine.UI.RectMask2D>();
            var events=r.gameObject.AddComponent<MapPointer>();events.owner=this;
            var areaRect=RectUI("Acorn areas",r,Vector2.zero,r.sizeDelta);areaOverlay=areaRect.gameObject.AddComponent<WorldMapAreaOverlay>();areaOverlay.raycastTarget=false;
            var overlay=RectUI("Current course overlay",r,Vector2.zero,new(740,visual?740*visual.bounds.height/visual.bounds.width:540));courseOverlay=overlay.gameObject.AddComponent<WorldMapCourseOverlay>();courseOverlay.raycastTarget=false;overlay.gameObject.SetActive(data.routesShown);
            UnityEngine.UI.Text Label(string name,string text,int size,Color color,int side)
            {
                var t=Text(name,r,Vector2.zero,new(230,24),size);t.text=text;t.color=color;t.supportRichText=true;
                t.rectTransform.pivot=new(side<0?1:side>0?0:.5f,.5f);t.alignment=side<0?TextAnchor.MiddleRight:side>0?TextAnchor.MiddleLeft:TextAnchor.MiddleCenter;
                var edge=t.gameObject.AddComponent<UnityEngine.UI.Outline>();edge.effectColor=new(0,0,0,.85f);edge.effectDistance=new(1,-1);return t;
            }
            foreach(var area in Areas)foreach(var region in area.regions.Where(x=>x.named)){var t=Label("Area "+area.name,area.name,14,area.tint,area.labelSide);t.fontStyle=FontStyle.Bold;areaLabels.Add((t,area,region));}
            foreach(var place in AcornAreas.Places){var t=Label("Place "+place.label,(place.side>0?"· ":"")+place.label+(place.side<0?" ·":""),13,new(1,1,.86f,.95f),place.side);t.fontStyle=FontStyle.Italic;placeLabels.Add((t,place.at));}
            heading=Text("Player heading",r,Vector2.zero,new(35,35),27);heading.color=Color.cyan;heading.text="▲";
            waypointLabel=Text("Waypoint",r,Vector2.zero,new(25,25),24);waypointLabel.color=Color.yellow;waypointLabel.text="+";
            foreach(var d in destinations){var t=Text(d.id,r,Vector2.zero,new(170,35),16);markers.Add(t);}
            foreach(var item in race.GetComponent<ExplorationCollection>().sites){var t=Text(item.id,r,Vector2.zero,new(16,16),15);t.text="●";t.color=new(1,.72f,.2f);acorns.Add(t);}
            Text("Cursor",r,Vector2.zero,new(25,25),20).text="+";
            northArrow=Text("North",r,Vector2.zero,new(40,52),18);northArrow.text="▲\nN";northArrow.fontStyle=FontStyle.Bold;northArrow.lineSpacing=.8f;
            {var edge=northArrow.gameObject.AddComponent<UnityEngine.UI.Outline>();edge.effectColor=new(0,0,0,.9f);edge.effectDistance=new(1.2f,-1.2f);}
            areaList=Text("Acorn areas list",panel.transform,new(485,-150),new(240,210),15);areaList.alignment=TextAnchor.UpperLeft;areaList.supportRichText=true;
            status=Text("Map status",panel.transform,new(460,38),new(255,410),18);
            Button("Previous destination",new(460,-188),()=>SelectNext(-1));Button("Next destination",new(460,-232),()=>SelectNext(1));Button("Travel (free roam)",new(460,-276),TravelSelected);
            Button("Close map / M / B",new(-420,-316),Close);Button("Center on player",new(-150,-316),()=>center=MapNormalized(race.vehicle.Body.position));Button("Clear waypoint",new(120,-316),()=>Waypoint=null);
            Button("Show / hide course",new(460,267),()=>courseOverlay.gameObject.SetActive(!courseOverlay.gameObject.activeSelf));
            Text("Map controls",panel.transform,new(-100,291),new(1000,25),16).text="NORTH ↑   Mouse: wheel zoom / drag pan / click waypoint or landmark";
            Text("Controller controls",panel.transform,new(0,-343),new(1200,22),15).text="Stick / WASD: pan · triggers / wheel: zoom · A / Space: waypoint · D-pad: destinations · X: travel · B / Esc: close";
            confirmation=RectUI("Confirm destination",panel.transform,Vector2.zero,new(1280,720)).gameObject;
            confirmation.AddComponent<UnityEngine.UI.Image>().color=new(.02f,.04f,.06f,.97f);
            confirmationText=Text("Named travel question",confirmation.transform,new(0,55),new(850,160),28);
            void Choice(string label,float x,Action action){var r=RectUI(label,confirmation.transform,new(x,-85),new(220,55));r.gameObject.AddComponent<UnityEngine.UI.Image>().color=new(.1f,.29f,.31f);var b=r.gameObject.AddComponent<UnityEngine.UI.Button>();b.navigation=new UnityEngine.UI.Navigation{mode=UnityEngine.UI.Navigation.Mode.None};b.onClick.AddListener(()=>action());Text(label,r,Vector2.zero,new(210,50),24).text=label;}
            Choice("Yes",-135,()=>ConfirmTravel());Choice("No",135,CancelTravel);confirmation.SetActive(false);
        }
        void Repaint()
        {
            if(texture)Destroy(texture);var source=visual&&visual.image?visual.image:terrain;int w=source?source.width:Columns,h=source?source.height:Rows;texture=new Texture2D(w,h,TextureFormat.RGBA32,false);texture.filterMode=FilterMode.Bilinear;texture.wrapMode=TextureWrapMode.Clamp;
            var colors=source?source.GetPixels32():Enumerable.Repeat((Color32)new Color(.28f,.36f,.2f),w*h).ToArray();
            // Preserve existing discovery IDs/save grid, independently of the larger visual extent.
            for(int y=0;y<h;y++)for(int x=0;x<w;x++){var p=MapWorldPoint(new Vector2((x+.5f)/w,(y+.5f)/h));if(!Visited(p)){int i=y*w+x;var c=colors[i];colors[i]=visual?new Color32((byte)(c.r*.65f),(byte)(c.g*.65f),(byte)(c.b*.65f),255):new Color32(14,19,21,255);}}
            texture.SetPixels32(colors);texture.Apply(false,true);picture.texture=texture;
        }
        Vector2 ScreenPoint(Vector3 p){var uv=MapNormalized(p);var v=(uv-center)*zoom;return Vector2.Scale(v,picture.rectTransform.rect.size);}
        void Marker(UnityEngine.UI.Text t,Vector3 p){var q=ScreenPoint(p);t.rectTransform.anchoredPosition=q;t.gameObject.SetActive(Mathf.Abs(q.x)<picture.rectTransform.rect.width/2-20&&Mathf.Abs(q.y)<picture.rectTransform.rect.height/2-20);}
        void Draw()
        {
            picture.uvRect=new Rect(center-Vector2.one*.5f/zoom,Vector2.one/zoom);Marker(heading,race.vehicle.Body.position);heading.rectTransform.localRotation=Quaternion.Euler(0,0,-race.vehicle.transform.eulerAngles.y);
            courseOverlay.SetView(race,visual,center,zoom,RouteCourses);
            var collection=Collection;var all=Areas;areaOverlay.SetView(all,collection,visual,center,zoom,selectedArea);
            foreach(var (t,area,region) in areaLabels){bool done=area.Done(collection);t.text=(done?"✓ ":"")+area.name+"  "+area.Count(collection);t.color=done?new Color(.75f,.77f,.75f,.8f):area.tint;Marker(t,region.label);}
            foreach(var (t,at) in placeLabels)Marker(t,at);
            northArrow.rectTransform.anchoredPosition=picture.rectTransform.rect.size*.5f-new Vector2(26,34);
            var list=new System.Text.StringBuilder("<b>ACORN AREAS</b>   D-pad ↑ ↓\n");
            for(int i=0;i<all.Length;i++){var a=all[i];bool done=a.Done(collection);list.Append(i==selectedArea?"<color=#FFFFFF><b>▶ ":"<color=#"+ColorUtility.ToHtmlStringRGB(done?new Color(.75f,.77f,.75f):a.tint)+">   ").Append(a.name).Append("  ").Append(a.Count(collection)).Append(done?"  ✓":"").Append(i==selectedArea?"</b>":"").Append("</color>\n").Append("<size=13><color=#9FB7B4>").Append(a.direction).Append("</color></size>\n");}
            areaList.text=list.ToString();
            if(Waypoint.HasValue)Marker(waypointLabel,Waypoint.Value);else waypointLabel.gameObject.SetActive(false);
            for(int i=0;i<destinations.Length;i++){markers[i].text=(selected==i?"◆ ":"● ")+destinations[i].title;Marker(markers[i],destinations[i].position);if(!Discovered(destinations[i].id))markers[i].gameObject.SetActive(false);}
            for(int i=0;i<acorns.Count;i++){var s=collection.sites[i];Marker(acorns[i],s.position);if(!collection.Discovered(s.id)||!Visited(s.position))acorns[i].gameObject.SetActive(false);}
            string wpt=Waypoint.HasValue?"Waypoint "+DisplayUnits.Distance(Vector3.ProjectOnPlane(Waypoint.Value-race.vehicle.Body.position,Vector3.up).magnitude)+(race.FreeRoam?"":" (Free Roam only)")+"\n\n":"Mouse: click the map to set a waypoint, right-click to clear\n\n";
            status.text=wpt+RouteTitle+(selected>=0?destinations[selected].title:"Map Point")+"\n"+(race.FreeRoam?"Select for location actions":"Travel available in Free Roam")+"\n\n"+RouteLegend+"\n\n"+(collection?collection.Header:"")+"\n\n"+(error??errorMessage);
            // 0.93 Part C: no two names on top of each other: area names first, then place names (one that would overlap is left
            // out at this zoom), then the discovered landmarks (one that would overlap shows only its dot until zoomed in)
            var taken=new List<Rect>();
            bool Fits(UnityEngine.UI.Text t){var rt=t.rectTransform;float w=Mathf.Min(t.preferredWidth,rt.rect.width)+4,h=t.preferredHeight+2;var at=rt.anchoredPosition;var box=new Rect(at.x-w*rt.pivot.x,at.y-h*.5f,w,h);if(taken.Any(o=>o.Overlaps(box)))return false;taken.Add(box);return true;}
            foreach(var (t,_,_) in areaLabels)if(t.gameObject.activeSelf&&!Fits(t))t.gameObject.SetActive(false);
            foreach(var (t,_) in placeLabels)if(t.gameObject.activeSelf&&!Fits(t))t.gameObject.SetActive(false);
            for(int i=0;i<markers.Count;i++)if(markers[i].gameObject.activeSelf&&!Fits(markers[i])){markers[i].text=selected==i?"◆":"●";Fits(markers[i]);}
            RefreshMapPrompts();
        }        public void OnScroll(PointerEventData e){if(sheetOpen)return;zoom=Mathf.Clamp(zoom+e.scrollDelta.y*.25f,1,6);Draw();}
        public void OnDrag(PointerEventData e){if(sheetOpen)return;center-=new Vector2(e.delta.x/picture.rectTransform.rect.width,e.delta.y/picture.rectTransform.rect.height)/zoom;selected=-1;Draw();}
        // 0.74 mouse: a click on a discovered landmark opens its actions (as before); a click anywhere else puts the waypoint
        // there; a right-click clears it.
        public void OnPointerClick(PointerEventData e)
        {if(sheetOpen||e.dragging)return;if(e.button==PointerEventData.InputButton.Right){ClearWaypoint();return;}if(e.button!=PointerEventData.InputButton.Left)return;if(!RectTransformUtility.ScreenPointToLocalPointInRectangle(picture.rectTransform,e.position,e.pressEventCamera,out var q))return;
            var uv=center+new Vector2(q.x/picture.rectTransform.rect.width,q.y/picture.rectTransform.rect.height)/zoom;
            bool landmark=Enumerable.Range(0,destinations.Length).Any(i=>Discovered(destinations[i].id)&&(ScreenPoint(destinations[i].position)-q).magnitude<=20);
            if(landmark){center=uv;SelectReticle();return;}
            selected=-1;SetWaypoint(MapWorldPoint(uv));Draw();}
    }    public sealed class MapPointer:MonoBehaviour,IPointerClickHandler,IDragHandler,IScrollHandler
    {public ExplorationMap owner;public void OnPointerClick(PointerEventData e)=>owner.OnPointerClick(e);public void OnDrag(PointerEventData e)=>owner.OnDrag(e);public void OnScroll(PointerEventData e)=>owner.OnScroll(e);}
}
