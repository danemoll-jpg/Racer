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
    public sealed class ExplorationMap : MonoBehaviour, IPointerClickHandler, IDragHandler, IScrollHandler
    {
        public const string Compatibility="woodstock-world-v2";
        public const int Columns=110, Rows=80;
        public static readonly Rect World=new(-900,-800,2200,1600);
        [Serializable] public sealed class Destination { public string id,title; public Vector3 position; public float yaw; }
        [Serializable] public sealed class Data { public int version=1; public string world=Compatibility; public List<int> visited=new(); public List<string> landmarks=new(); }
        public Destination[] destinations=Array.Empty<Destination>();
        public Texture2D terrain;
        Data data=new(); readonly HashSet<int> visited=new(); RaceDirector race; string path,error;
        Vector3 previous; bool sampled,dirty,resume; float nextReveal,nextSave,zoom=1; Vector2 center=new(.5f,.5f);
        GameObject panel; UnityEngine.UI.RawImage picture; UnityEngine.UI.Text status,heading,waypointLabel;
        readonly List<UnityEngine.UI.Text> markers=new(); Texture2D texture; int selected=-1;
        readonly List<UnityEngine.UI.Text> acorns=new();
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
            race=owner;path=Path.Combine(root,"exploration-map-"+Compatibility+".json");
            try {if(File.Exists(path))data=JsonUtility.FromJson<Data>(File.ReadAllText(path))??new();
                if(data.world!=Compatibility||data.version!=1)throw new IOException("Incompatible map data retained");
                data.visited??=new();data.landmarks??=new();foreach(int cell in data.visited)if(cell>=0&&cell<Columns*Rows)visited.Add(cell);
            } catch(Exception e){error=e.Message;}
        }
        public static Vector2 Normalized(Vector3 p)=>new((p.x-World.xMin)/World.width,(p.z-World.yMin)/World.height);
        public static Vector3 WorldPoint(Vector2 uv)=>new(World.xMin+uv.x*World.width,0,World.yMin+uv.y*World.height);
        public bool Visited(Vector3 p){var uv=Normalized(p);return uv.x>=0&&uv.x<1&&uv.y>=0&&uv.y<1&&visited.Contains(Mathf.FloorToInt(uv.y*Rows)*Columns+Mathf.FloorToInt(uv.x*Columns));}
        public void ResetMovement(){sampled=false;previous=race.vehicle.Body.position;nextReveal=Time.time+1;}
        void FixedUpdate()
        {
            if(!race||race.Flow.State!=RaceFlow.Stage.Racing)return;
            var p=race.vehicle.Body.position;
            if(!sampled){previous=p;sampled=true;return;}
            bool continuous=Vector3.Distance(previous,p)<=Mathf.Max(3,race.vehicle.Body.linearVelocity.magnitude*Time.fixedDeltaTime*2+.3f);previous=p;
            if(!continuous){nextReveal=Time.time+1;return;}
            if(Time.time<nextReveal)return;nextReveal=Time.time+.4f;
            Reveal(p);if(dirty&&Time.unscaledTime>nextSave){Save();nextSave=Time.unscaledTime+3;}
        }
        public void Reveal(Vector3 p)
        {
            if(error!=null)return;var uv=Normalized(p);int cx=Mathf.FloorToInt(uv.x*Columns),cy=Mathf.FloorToInt(uv.y*Rows);
            for(int y=Mathf.Max(0,cy-3);y<=Mathf.Min(Rows-1,cy+3);y++)for(int x=Mathf.Max(0,cx-3);x<=Mathf.Min(Columns-1,cx+3);x++)
                if(Vector2.Distance(new(World.xMin+(x+.5f)*20,World.yMin+(y+.5f)*20),new(p.x,p.z))<=48)dirty|=visited.Add(y*Columns+x);
            foreach(var d in destinations)if(Vector3.Distance(d.position,p)<45&&!Discovered(d.id)){data.landmarks.Add(d.id);dirty=true;}
        }
        public void Save(){if(!dirty||error!=null)return;try{data.visited=visited.OrderBy(i=>i).ToList();AtomicSave.Write(path,JsonUtility.ToJson(data));dirty=false;}catch(Exception e){error=e.Message;}}
        void OnApplicationPause(bool paused){if(paused)Save();}
        void OnDestroy(){Save();if(texture)Destroy(texture);if(panel)Destroy(panel);}
        void Update()
        {
            if(!race||!race.Flow)return;
            var k=Keyboard.current;var g=Gamepad.current;
            if((k?.mKey.wasPressedThisFrame??false)||(g?.selectButton.wasPressedThisFrame??false)){if(Opened)Close();else if(race.Flow.State==RaceFlow.Stage.Racing||race.Flow.State==RaceFlow.Stage.Paused)Open();return;}
            if(!Opened)return;
            if(Confirming){if(Time.frameCount==confirmationFrame)return;if((k?.escapeKey.wasPressedThisFrame??false)||(g?.buttonEast.wasPressedThisFrame??false)){CancelTravel();return;}if((k?.spaceKey.wasPressedThisFrame??false)||(g?.buttonSouth.wasPressedThisFrame??false)){ConfirmTravel();return;}return;}
            if((k?.escapeKey.wasPressedThisFrame??false)||(g?.buttonEast.wasPressedThisFrame??false)){Close();return;}
            Vector2 pan=g?.leftStick.ReadValue()??Vector2.zero;
            if(k!=null)pan+=new Vector2((k.dKey.isPressed?1:0)-(k.aKey.isPressed?1:0),(k.wKey.isPressed?1:0)-(k.sKey.isPressed?1:0));
            center+=pan*Time.unscaledDeltaTime*.4f/zoom;
            if(g!=null){zoom=Mathf.Clamp(zoom+(g.rightTrigger.ReadValue()-g.leftTrigger.ReadValue())*Time.unscaledDeltaTime*3,1,6);
                if(g.dpad.right.wasPressedThisFrame)SelectNext(1);if(g.dpad.left.wasPressedThisFrame)SelectNext(-1);
                if(g.buttonSouth.wasPressedThisFrame)SetWaypoint(WorldPoint(center));if(g.buttonWest.wasPressedThisFrame)TravelSelected();}
            if(k?.spaceKey.wasPressedThisFrame??false)SetWaypoint(WorldPoint(center));
            float margin=.5f/zoom;center=new(Mathf.Clamp(center.x,margin,1-margin),Mathf.Clamp(center.y,margin,1-margin));Draw();
        }
        public void Open()
        {
            resume=race.Flow.State==RaceFlow.Stage.Racing;if(resume)race.Flow.Pause();if(!panel)BuildUI();
            center=Normalized(race.vehicle.Body.position);panel.SetActive(true);EventSystem.current?.SetSelectedGameObject(null);Repaint();Draw();Save();
        }
        public void Close(){CancelTravel();closedFrame=Time.frameCount;if(panel)panel.SetActive(false);Save();if(resume)race.Flow.Resume();}
        public void SetWaypoint(Vector3 p){Waypoint=p;}
        void SelectNext(int direction){if(destinations.Length==0)return;selected=(selected+direction+destinations.Length)%destinations.Length;if(Discovered(destinations[selected].id))center=Normalized(destinations[selected].position);}
        public bool Travel(int index)
        {
            if(error!=null||!race.FreeRoam||index<0||index>=destinations.Length||!Discovered(destinations[index].id)){errorMessage="Travel needs free roam and a discovered destination.";return false;}
            var d=destinations[index];var recovery=race.vehicle.GetComponent<VehicleRespawn>();
            if(!recovery.TryFastTravel(d.position,Quaternion.Euler(0,d.yaw,0))){errorMessage="Arrival blocked or unsupported. Try again after traffic clears.";return false;}
            race.Flow.Activities.NewSession();race.Flow.Ghost.ResetSession();race.GetComponent<ExplorationCollection>()?.ResetMovement();
            race.Racers[0].Branch.Clear();race.Racers[0].FinishArmed=false;race.Racers[0].FinishApproach=0;
            race.ResetSampling(race.vehicle.Body.position,Time.timeAsDouble);ResetMovement();
            errorMessage="Arrived at "+d.title+". Active attempts cancelled.";center=Normalized(race.vehicle.Body.position);Save();return true;
        }
        string errorMessage="";
        void TravelSelected(){RequestTravel(selected);}
        public void RequestTravel(int index)
        {
            if(!Opened)return;
            if(index<0||index>=destinations.Length||!race.FreeRoam||!Discovered(destinations[index].id)){errorMessage="Travel needs free roam and a discovered destination.";Draw();return;}
            pending=index;confirmationFrame=Time.frameCount;confirmationText.text="Travel to "+destinations[index].title+"?\nA / Space: Yes    B / Esc: No";confirmation.SetActive(true);
        }
        public void CancelTravel(){pending=-1;if(confirmation)confirmation.SetActive(false);EventSystem.current?.SetSelectedGameObject(null);}
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
            var r=RectUI("Terrain",panel.transform,new(-150,10),new(740,540));picture=r.gameObject.AddComponent<UnityEngine.UI.RawImage>();r.gameObject.AddComponent<UnityEngine.UI.RectMask2D>();
            var events=r.gameObject.AddComponent<MapPointer>();events.owner=this;
            heading=Text("Player heading",r,Vector2.zero,new(35,35),27);heading.color=Color.cyan;heading.text="▲";
            waypointLabel=Text("Waypoint",r,Vector2.zero,new(25,25),24);waypointLabel.color=Color.yellow;waypointLabel.text="+";
            foreach(var d in destinations){var t=Text(d.id,r,Vector2.zero,new(170,35),16);markers.Add(t);}
            foreach(var item in race.GetComponent<ExplorationCollection>().sites){var t=Text(item.id,r,Vector2.zero,new(16,16),15);t.text="●";t.color=new(1,.72f,.2f);acorns.Add(t);}
            Text("Cursor",r,Vector2.zero,new(25,25),20).text="+";
            status=Text("Map status",panel.transform,new(460,38),new(255,410),18);
            Button("Previous destination",new(460,-188),()=>SelectNext(-1));Button("Next destination",new(460,-232),()=>SelectNext(1));Button("Travel (free roam)",new(460,-276),TravelSelected);
            Button("Close map / M / B",new(-420,-316),Close);Button("Center on player",new(-150,-316),()=>center=Normalized(race.vehicle.Body.position));Button("Clear waypoint",new(120,-316),()=>Waypoint=null);
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
            if(texture)Destroy(texture);int w=terrain?terrain.width:Columns,h=terrain?terrain.height:Rows;texture=new Texture2D(w,h,TextureFormat.RGBA32,false);texture.filterMode=FilterMode.Point;texture.wrapMode=TextureWrapMode.Clamp;
            var colors=terrain?terrain.GetPixels():Enumerable.Repeat(new Color(.28f,.36f,.2f),w*h).ToArray();
            for(int y=0;y<h;y++)for(int x=0;x<w;x++)if(!visited.Contains((y*Rows/h)*Columns+x*Columns/w))colors[y*w+x]=new(.055f,.075f,.083f);
            texture.SetPixels(colors);texture.Apply();picture.texture=texture;
        }
        Vector2 ScreenPoint(Vector3 p){var uv=Normalized(p);var v=(uv-center)*zoom;return new(v.x*740,v.y*540);}
        void Marker(UnityEngine.UI.Text t,Vector3 p){var q=ScreenPoint(p);t.rectTransform.anchoredPosition=q;t.gameObject.SetActive(Mathf.Abs(q.x)<350&&Mathf.Abs(q.y)<250);}
        void Draw()
        {
            picture.uvRect=new Rect(center-Vector2.one*.5f/zoom,Vector2.one/zoom);Marker(heading,race.vehicle.Body.position);heading.rectTransform.localRotation=Quaternion.Euler(0,0,-race.vehicle.transform.eulerAngles.y);
            if(Waypoint.HasValue)Marker(waypointLabel,Waypoint.Value);else waypointLabel.gameObject.SetActive(false);
            for(int i=0;i<destinations.Length;i++){markers[i].text=(selected==i?"◆ ":"● ")+destinations[i].title;Marker(markers[i],destinations[i].position);if(!Discovered(destinations[i].id))markers[i].gameObject.SetActive(false);}
            var collection=race.GetComponent<ExplorationCollection>();for(int i=0;i<acorns.Count;i++){var s=collection.sites[i];Marker(acorns[i],s.position);if(!collection.Discovered(s.id)||!Visited(s.position))acorns[i].gameObject.SetActive(false);}
            string choice=selected<0?"Select a discovered landmark.":Discovered(destinations[selected].id)?destinations[selected].title:"Undiscovered destination";
            status.text=choice+"\n"+(race.FreeRoam?"Free roam travel":"Race: travel disabled")+"\n\nVisited terrain is NOT fully searched.\n\n"+(race.GetComponent<ExplorationCollection>()?.Summary??"")+"\n\n"+(error??errorMessage);
        }
        public void OnScroll(PointerEventData e){zoom=Mathf.Clamp(zoom+e.scrollDelta.y*.25f,1,6);}
        public void OnDrag(PointerEventData e){center-=new Vector2(e.delta.x/740,e.delta.y/540)/zoom;}
        public void OnPointerClick(PointerEventData e)
        {if(e.dragging||e.button!=PointerEventData.InputButton.Left)return;if(!RectTransformUtility.ScreenPointToLocalPointInRectangle(picture.rectTransform,e.position,e.pressEventCamera,out var q))return;
            if(Confirming)return;var p=WorldPoint(center+new Vector2(q.x/740,q.y/540)/zoom);selected=Array.FindIndex(destinations,d=>Discovered(d.id)&&Vector2.Distance(ScreenPoint(d.position),q)<24);if(selected<0)SetWaypoint(p);else RequestTravel(selected);}
    }
    public sealed class MapPointer:MonoBehaviour,IPointerClickHandler,IDragHandler,IScrollHandler
    {public ExplorationMap owner;public void OnPointerClick(PointerEventData e)=>owner.OnPointerClick(e);public void OnDrag(PointerEventData e)=>owner.OnDrag(e);public void OnScroll(PointerEventData e)=>owner.OnScroll(e);}
}
