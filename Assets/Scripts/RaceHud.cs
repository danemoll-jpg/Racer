using UnityEngine;

namespace Racer
{
    public sealed class RaceHud : MonoBehaviour
    {
        public RaceDirector race;
        public UnityEngine.UI.Text display;
        UnityEngine.UI.Text speedometer;
        GameObject speedPanel;
        GameObject wrongPanel;
        UnityEngine.UI.Text wrongText,wrongArrow;
        UnityEngine.UI.Text activities;
        UnityEngine.UI.Text waypointText,waypointArrow;GameObject waypointPanel;
        // 0.79 Part A: the Free Roam day and clock on their own (top left); Part H: the camera hint (above the speedometer)
        GameObject clockPanel;UnityEngine.UI.Text clockDay,clockTime;MoonIcon moon;
        UnityEngine.UI.Text cameraHint;float cameraHintLeft,roamHintLeft,seenSession=-1;string seenView;
        public const float CameraHintSeconds=4,RoamHintSeconds=8;
        public bool ClockVisible=>clockPanel&&clockPanel.activeInHierarchy;
        public bool CameraHintVisible=>cameraHint&&cameraHint.gameObject.activeInHierarchy&&cameraHint.color.a>.01f;
        public string CameraHintText=>cameraHint?cameraHint.text:"";
        public RectTransform ClockRect=>clockPanel?(RectTransform)clockPanel.transform:null;
        void Start()
        {
            RacingMiniMap.Create(transform, race, display.font);
            DeveloperLocationHud.Create(transform, race, display.font);
            var panel=(RectTransform)display.transform.parent;
            panel.anchorMin=panel.anchorMax=panel.pivot=new Vector2(0,1);
            panel.anchoredPosition=new(18,-18); panel.sizeDelta=new(300,race.Forest||race.reverseCourse?132:108);
            display.fontSize=21; display.alignment=TextAnchor.UpperLeft;
            display.rectTransform.offsetMin=new(12,8); display.rectTransform.offsetMax=new(-10,-8);
            speedPanel=new GameObject("Speedometer",typeof(RectTransform),typeof(UnityEngine.UI.Image));
            var rect=speedPanel.GetComponent<RectTransform>(); rect.SetParent(transform,false);
            rect.anchorMin=rect.anchorMax=rect.pivot=new Vector2(1,0); rect.anchoredPosition=new(-18,18); rect.sizeDelta=new(164,62);
            speedPanel.GetComponent<UnityEngine.UI.Image>().color=new Color(.025f,.055f,.07f,.84f);
            var label=new GameObject("Speed",typeof(RectTransform),typeof(UnityEngine.UI.Text)); label.transform.SetParent(rect,false);
            speedometer=label.GetComponent<UnityEngine.UI.Text>(); speedometer.font=display.font; speedometer.fontSize=30; speedometer.color=Color.white; speedometer.alignment=TextAnchor.MiddleCenter; speedometer.raycastTarget=false;
            speedometer.rectTransform.anchorMin=Vector2.zero; speedometer.rectTransform.anchorMax=Vector2.one; speedometer.rectTransform.offsetMin=speedometer.rectTransform.offsetMax=Vector2.zero;
            wrongPanel=new GameObject("Wrong way guidance",typeof(RectTransform),typeof(UnityEngine.UI.Image));
            var wr=wrongPanel.GetComponent<RectTransform>();wr.SetParent(transform,false);wr.anchorMin=wr.anchorMax=wr.pivot=new(.5f,1);wr.anchoredPosition=new(0,-24);wr.sizeDelta=new(480,116);
            wrongPanel.GetComponent<UnityEngine.UI.Image>().color=new(.37f,.015f,.018f,.98f);
            UnityEngine.UI.Text Label(string name,Vector2 position,Vector2 dimensions,int fontSize)
            {var t=new GameObject(name,typeof(RectTransform),typeof(UnityEngine.UI.Text)).GetComponent<UnityEngine.UI.Text>();t.transform.SetParent(wr,false);t.font=display.font;t.fontSize=fontSize;t.color=new(1,.84f,.35f);t.alignment=TextAnchor.MiddleCenter;t.raycastTarget=false;t.rectTransform.anchoredPosition=position;t.rectTransform.sizeDelta=dimensions;return t;}
            wrongArrow=Label("Local course direction",new(-184,0),new(88,88),68);wrongArrow.text="↑";
            wrongText=Label("Wrong way and local reset",new(48,0),new(366,108),38);wrongText.color=Color.white;wrongText.supportRichText=true;wrongPanel.SetActive(false);
            // 0.74 Free Roam waypoint line: distance and an arrow pointing to it (relative to the vehicle), bottom centre.
            waypointPanel=new GameObject("Waypoint guidance",typeof(RectTransform),typeof(UnityEngine.UI.Image));var wp=waypointPanel.GetComponent<RectTransform>();wp.SetParent(transform,false);wp.anchorMin=wp.anchorMax=wp.pivot=new(.5f,0);wp.anchoredPosition=new(0,18);wp.sizeDelta=new(330,54);
            waypointPanel.GetComponent<UnityEngine.UI.Image>().color=new Color(.025f,.055f,.07f,.84f);
            UnityEngine.UI.Text WLabel(string name,Vector2 position,Vector2 dimensions,int fontSize){var t=new GameObject(name,typeof(RectTransform),typeof(UnityEngine.UI.Text)).GetComponent<UnityEngine.UI.Text>();t.transform.SetParent(wp,false);t.font=display.font;t.fontSize=fontSize;t.color=new(1,.86f,.3f);t.alignment=TextAnchor.MiddleCenter;t.raycastTarget=false;t.rectTransform.anchoredPosition=position;t.rectTransform.sizeDelta=dimensions;return t;}
            waypointArrow=WLabel("Waypoint direction",new(-132,0),new(50,50),40);waypointArrow.text="↑";waypointText=WLabel("Waypoint distance",new(26,0),new(270,50),24);waypointPanel.SetActive(false);
            BuildClock();BuildCameraHint();
            var feedback=new GameObject("Arcade activity feedback",typeof(RectTransform),typeof(UnityEngine.UI.Text));feedback.transform.SetParent(transform,false);activities=feedback.GetComponent<UnityEngine.UI.Text>();activities.font=display.font;activities.fontSize=21;activities.color=new Color(1,.9f,.5f);activities.raycastTarget=false;activities.alignment=TextAnchor.LowerLeft;activities.rectTransform.anchorMin=activities.rectTransform.anchorMax=activities.rectTransform.pivot=Vector2.zero;activities.rectTransform.anchoredPosition=new(22,78);activities.rectTransform.sizeDelta=new(700,80);feedback.AddComponent<UnityEngine.UI.Outline>();
        }
        UnityEngine.UI.Text HudText(string name,Transform parent,int size,TextAnchor anchor,Color colour)
        {
            var t=new GameObject(name,typeof(RectTransform),typeof(UnityEngine.UI.Text)).GetComponent<UnityEngine.UI.Text>();t.transform.SetParent(parent,false);
            t.font=display.font;t.fontSize=size;t.alignment=anchor;t.color=colour;t.raycastTarget=false;var o=t.gameObject.AddComponent<UnityEngine.UI.Outline>();o.effectColor=new(0,0,0,.85f);o.effectDistance=new(1.4f,-1.4f);return t;
        }
        // Day and time, large, white with an outline on a soft dark backing: readable on every sky; nothing else in it.
        void BuildClock()
        {
            clockPanel=new GameObject("Free Roam day and clock",typeof(RectTransform),typeof(UnityEngine.UI.Image));var r=(RectTransform)clockPanel.transform;r.SetParent(transform,false);
            r.anchorMin=r.anchorMax=r.pivot=new(0,1);r.anchoredPosition=new(18,-18);r.sizeDelta=new(150,78);
            var image=clockPanel.GetComponent<UnityEngine.UI.Image>();image.color=new(.02f,.04f,.05f,.58f);image.raycastTarget=false;
            clockDay=HudText("Day",r,19,TextAnchor.MiddleLeft,new(.88f,.92f,.96f));clockDay.rectTransform.anchorMin=clockDay.rectTransform.anchorMax=clockDay.rectTransform.pivot=new(0,1);clockDay.rectTransform.anchoredPosition=new(12,-5);clockDay.rectTransform.sizeDelta=new(100,24);
            clockTime=HudText("Time",r,38,TextAnchor.MiddleLeft,Color.white);clockTime.fontStyle=FontStyle.Bold;clockTime.rectTransform.anchorMin=clockTime.rectTransform.anchorMax=clockTime.rectTransform.pivot=new(0,1);clockTime.rectTransform.anchoredPosition=new(10,-27);clockTime.rectTransform.sizeDelta=new(136,46);
            moon=new GameObject("Moon phase",typeof(RectTransform)).AddComponent<MoonIcon>();moon.transform.SetParent(r,false);moon.raycastTarget=false;
            moon.rectTransform.anchorMin=moon.rectTransform.anchorMax=moon.rectTransform.pivot=new(1,1);moon.rectTransform.anchoredPosition=new(-10,-7);moon.rectTransform.sizeDelta=new(20,20);
            clockPanel.SetActive(false);
        }
        void BuildCameraHint()
        {
            cameraHint=HudText("Camera hint",transform,19,TextAnchor.MiddleRight,new(1,.92f,.62f));var r=cameraHint.rectTransform;
            r.anchorMin=r.anchorMax=r.pivot=new(1,0);r.anchoredPosition=new(-20,86);r.sizeDelta=new(340,28);cameraHint.gameObject.SetActive(false);
        }
        // the camera control and the current view: a few seconds when driving starts and whenever the view changes
        void UpdateCameraHint()
        {
            var flow=race.Flow;var views=CameraViews.Current;if(!cameraHint)return;
            bool driving=flow.State==RaceFlow.Stage.Racing||flow.State==RaceFlow.Stage.Countdown;
            if(flow.State==RaceFlow.Stage.Racing&&seenSession!=flow.SessionStartedAt){seenSession=flow.SessionStartedAt;cameraHintLeft=CameraHintSeconds;roamHintLeft=race.FreeRoam?RoamHintSeconds:0;}
            string view=views?views.PlayerViewName:null;if(view!=null&&view!=seenView){if(seenView!=null)cameraHintLeft=CameraHintSeconds;seenView=view;}
            // the hints count down in shown time only (at most 0.1 s a frame), so a loading hitch does not use them up
            float step=Mathf.Min(Time.unscaledDeltaTime,.1f);
            bool show=driving&&!flow.MenuVisible&&!TrailerMode.Active&&views&&cameraHintLeft>0;
            if(driving&&!flow.MenuVisible){cameraHintLeft=Mathf.Max(0,cameraHintLeft-step);roamHintLeft=Mathf.Max(0,roamHintLeft-step);}
            cameraHint.gameObject.SetActive(show);if(!show)return;
            cameraHint.text="V / X: camera — "+view;var c=cameraHint.color;c.a=Mathf.Clamp01(cameraHintLeft);cameraHint.color=c;
        }
        public static string FormatTime(double seconds)
        { int ms = (int)(seconds * 1000); return $"{ms / 60000:00}:{ms / 1000 % 60:00}.{ms % 1000:000}"; }
        public string BuildText()
        {
            if(race.FreeRoam)return "";
            var p = race.Progress;
            return (race.reverseCourse?race.courseName.ToUpperInvariant()+"\n":race.Forest?race.courseName.ToUpperInvariant()+"\n":"")+(p.Unlimited?$"LAP {p.CompletedLaps+1} · {p.CompletedLaps} completed":$"LAP {Mathf.Min(p.CompletedLaps+1,p.TargetLaps)}/{p.TargetLaps}")+$"    {(race.opponents?$"POS {race.PlayerPosition}/{race.Racers.Count}":"SOLO")}\n"
                +$"Lap    {FormatTime(p.Finished?p.LastLap-p.CurrentLapPenalty:p.LapTime(race.Clock))}\nRace  {FormatTime(p.RaceTime(race.Clock))}";
        }
        void LateUpdate()
        {
            if(!race || race.Progress==null || !display) return;
            display.text=BuildText();
            display.transform.parent.gameObject.SetActive(!race.FreeRoam&&!race.Flow.MenuVisible);
            UpdateCameraHint();
            // 0.79 Part B: in Free Roam only what is relevant now: the activity you are at, the attempt, its result, an acorn
            // just found, and the menu hint for a few seconds after Free Roam begins; the day and clock have their own box.
            if(activities)
            {
                var lines=new System.Collections.Generic.List<string>();
                if(!race.Flow.MenuVisible)
                {
                    string activity=race.Flow.Activities?.Hud;if(!string.IsNullOrEmpty(activity))lines.Add(activity);
                    if(race.FreeRoam)
                    {
                        if(race.GetComponent<ExplorationCollection>() is ExplorationCollection collection&&!string.IsNullOrEmpty(collection.Hud))lines.Add(collection.Hud);
                        if(roamHintLeft>0)lines.Add("Esc or Start: activities, retry, menu");
                    }
                }
                if(race.FreeRoam)activities.rectTransform.sizeDelta=new(700,110);activities.text=string.Join("\n",lines);
            }
            if(clockPanel)
            {
                bool clock=race.FreeRoam&&!race.Flow.MenuVisible&&WorldLook.Current&&WorldLook.Current.Mode=="Free Roam";clockPanel.SetActive(clock);
                if(clock){var look=WorldLook.Current;clockDay.text="Day "+look.Day;clockTime.text=look.Clock;moon.Phase=look.MoonPhase;}
            }
            var guide=race.GetComponent<WaypointGuide>();if(!guide)guide=FindAnyObjectByType<WaypointGuide>();
            if(waypointPanel){bool show=guide&&guide.Active&&!race.Flow.MenuVisible;waypointPanel.SetActive(show);if(show){waypointText.text=guide.Hud;waypointArrow.rectTransform.localRotation=Quaternion.Euler(0,0,-guide.Bearing);}}
            var guidance=race.GetComponent<WrongWayGuidance>();
            if(wrongPanel)
            {
                wrongPanel.SetActive(guidance&&guidance.Visible&&!race.Flow.MenuVisible&&!race.Progress.Finished);
                if(wrongPanel.activeSelf)
                {
                    wrongText.text="<b>WRONG WAY</b>\n<size=23>"+race.vehicle.GetComponent<VehicleInput>().ResetControlLabel+": reset locally</size>";
                    var cam=Camera.main;var forward=Vector3.ProjectOnPlane(cam.transform.forward,Vector3.up).normalized;
                    wrongArrow.rectTransform.localRotation=Quaternion.Euler(0,0,-Vector3.SignedAngle(forward,guidance.Direction,Vector3.up));
                }
            }
            if(speedPanel) { speedPanel.SetActive(!race.Flow.MenuVisible); speedometer.text=$"{DisplayUnits.Mph(Mathf.Abs(race.vehicle.ForwardSpeed)):0} <size=17>mph</size>"; }
        }
    }
}
