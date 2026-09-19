using UnityEngine;

namespace Racer
{
    public sealed class RaceHud : MonoBehaviour
    {
        public RaceDirector race;
        public UnityEngine.UI.Text display;
        UnityEngine.UI.Text speedometer;
        GameObject speedPanel;
        void Start()
        {
            var panel=(RectTransform)display.transform.parent;
            panel.anchorMin=panel.anchorMax=panel.pivot=new Vector2(0,1);
            panel.anchoredPosition=new(18,-18); panel.sizeDelta=new(300,race.Forest?132:108);
            display.fontSize=21; display.alignment=TextAnchor.UpperLeft;
            display.rectTransform.offsetMin=new(12,8); display.rectTransform.offsetMax=new(-10,-8);
            speedPanel=new GameObject("Speedometer",typeof(RectTransform),typeof(UnityEngine.UI.Image));
            var rect=speedPanel.GetComponent<RectTransform>(); rect.SetParent(transform,false);
            rect.anchorMin=rect.anchorMax=rect.pivot=new Vector2(1,0); rect.anchoredPosition=new(-18,18); rect.sizeDelta=new(164,62);
            speedPanel.GetComponent<UnityEngine.UI.Image>().color=new Color(.025f,.055f,.07f,.84f);
            var label=new GameObject("Speed",typeof(RectTransform),typeof(UnityEngine.UI.Text)); label.transform.SetParent(rect,false);
            speedometer=label.GetComponent<UnityEngine.UI.Text>(); speedometer.font=display.font; speedometer.fontSize=30; speedometer.color=Color.white; speedometer.alignment=TextAnchor.MiddleCenter; speedometer.raycastTarget=false;
            speedometer.rectTransform.anchorMin=Vector2.zero; speedometer.rectTransform.anchorMax=Vector2.one; speedometer.rectTransform.offsetMin=speedometer.rectTransform.offsetMax=Vector2.zero;
        }
        public static string FormatTime(double seconds)
        { int ms = (int)(seconds * 1000); return $"{ms / 60000:00}:{ms / 1000 % 60:00}.{ms % 1000:000}"; }
        public string BuildText()
        {
            var p = race.Progress;
            return (race.Forest?"FOREST LOOP\n":"")+$"LAP {Mathf.Min(p.CompletedLaps+1,p.TargetLaps)}/{p.TargetLaps}    {(race.opponents?$"POS {race.PlayerPosition}/{race.Racers.Count}":"SOLO 1/1")}\n"
                +$"Lap    {FormatTime(p.Finished?p.LastLap-p.CurrentLapPenalty:p.LapTime(race.Clock))}\nRace  {FormatTime(p.RaceTime(race.Clock))}";
        }
        void LateUpdate()
        {
            if(!race || race.Progress==null || !display) return;
            display.text=BuildText();
            if(speedPanel) { speedPanel.SetActive(!race.Flow.MenuVisible); speedometer.text=$"{Mathf.Abs(race.vehicle.ForwardSpeed)*3.6f:0} <size=17>km/h</size>"; }
        }
    }
}
