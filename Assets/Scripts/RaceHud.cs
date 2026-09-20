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
        void Start()
        {
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
            var feedback=new GameObject("Arcade activity feedback",typeof(RectTransform),typeof(UnityEngine.UI.Text));feedback.transform.SetParent(transform,false);activities=feedback.GetComponent<UnityEngine.UI.Text>();activities.font=display.font;activities.fontSize=21;activities.color=new Color(1,.9f,.5f);activities.raycastTarget=false;activities.alignment=TextAnchor.LowerLeft;activities.rectTransform.anchorMin=activities.rectTransform.anchorMax=activities.rectTransform.pivot=Vector2.zero;activities.rectTransform.anchoredPosition=new(22,78);activities.rectTransform.sizeDelta=new(700,80);feedback.AddComponent<UnityEngine.UI.Outline>();
        }
        public static string FormatTime(double seconds)
        { int ms = (int)(seconds * 1000); return $"{ms / 60000:00}:{ms / 1000 % 60:00}.{ms % 1000:000}"; }
        public string BuildText()
        {
            if(race.FreeRoam)return "";
            var p = race.Progress;
            return (race.reverseCourse?race.courseName.ToUpperInvariant()+"\n":race.Forest?"FOREST LOOP\n":"")+$"LAP {Mathf.Min(p.CompletedLaps+1,p.TargetLaps)}/{p.TargetLaps}    {(race.opponents?$"POS {race.PlayerPosition}/{race.Racers.Count}":"SOLO 1/1")}\n"
                +$"Lap    {FormatTime(p.Finished?p.LastLap-p.CurrentLapPenalty:p.LapTime(race.Clock))}\nRace  {FormatTime(p.RaceTime(race.Clock))}";
        }
        void LateUpdate()
        {
            if(!race || race.Progress==null || !display) return;
            display.text=BuildText();
            display.transform.parent.gameObject.SetActive(!race.FreeRoam&&!race.Flow.MenuVisible);
            if(activities)activities.text=race.Flow.MenuVisible?"":race.Flow.Activities?.Hud;
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
