using UnityEngine;

namespace Racer
{
    public sealed partial class RaceMenus
    {
        GameObject finishPanel;UnityEngine.UI.Text lapCard,raceCard;UnityEngine.UI.Image finishAccent;
        MenuGlyph completeGlyph;UnityEngine.UI.Text completeKey;FinishPresentation.Result presentedLap;float flourishAt;
        GameObject roamHint;MenuGlyph roamGlyph;UnityEngine.UI.Text roamKey;
        void UpdateRoamHint()
        {
            bool visible=flow.State==RaceFlow.Stage.Racing&&flow.Race.FreeRoam&&Time.unscaledTime<flow.RoamMenuHintUntil;
            if(visible&&!roamHint){
                var r=Rect("Free Roam menu hint",shade.transform.parent);r.anchorMin=r.anchorMax=new(.5f,.28f);r.sizeDelta=new(470,76);r.gameObject.AddComponent<UnityEngine.UI.Image>().color=new(.025f,.065f,.085f,.94f);roamHint=r.gameObject;
                var label=Label("Menu hint",r,23,0);label.rectTransform.anchorMin=label.rectTransform.anchorMax=new(.5f,.5f);label.rectTransform.sizeDelta=new(105,32);label.rectTransform.anchoredPosition=new(-125,12);label.alignment=TextAnchor.MiddleCenter;label.text="PRESS";
                var menuLabel=Label("Menu action",r,23,0);menuLabel.rectTransform.anchorMin=menuLabel.rectTransform.anchorMax=new(.5f,.5f);menuLabel.rectTransform.sizeDelta=new(160,32);menuLabel.rectTransform.anchoredPosition=new(77,12);menuLabel.alignment=TextAnchor.MiddleCenter;menuLabel.text="FOR MENU";
                var context=Label("Menu context",r,16,0);context.rectTransform.anchorMin=context.rectTransform.anchorMax=new(.5f,.5f);context.rectTransform.sizeDelta=new(430,22);context.rectTransform.anchoredPosition=new(0,-20);context.alignment=TextAnchor.MiddleCenter;context.text="Race and game options";
                var icon=Rect("Menu button",r);icon.anchorMin=icon.anchorMax=new(.5f,.5f);icon.sizeDelta=new(42,30);icon.anchoredPosition=new(-50,12);roamGlyph=icon.gameObject.AddComponent<MenuGlyph>();roamGlyph.raycastTarget=false;
                roamKey=Label("Key",icon,15,0);Stretch(roamKey.rectTransform,0,0,0,0);roamKey.alignment=TextAnchor.MiddleCenter;
            }
            if(roamHint){roamHint.SetActive(visible);if(visible){string path=MenuInput.Controller?"<Gamepad>/start":"<Keyboard>/escape";roamGlyph.SetPath(path);roamKey.text=MenuGlyph.Label(path);}}
        }
        void UpdateFinishPresentation()
        {
            UpdateRoamHint();
            bool visible=flow.State==RaceFlow.Stage.Racing&&flow.Race.Progress.Finished&&flow.FinishCards.Lap!=null;
            if(!finishPanel)
            {
                var r=Rect("Finish achievements",shade.transform.parent);r.anchorMin=r.anchorMax=new(.5f,.5f);r.sizeDelta=new(960,280);r.gameObject.AddComponent<UnityEngine.UI.Image>().color=new(.025f,.065f,.085f,.97f);finishPanel=r.gameObject;
                lapCard=Label("Best lap",r,25,0);lapCard.rectTransform.anchorMin=new(0,0);lapCard.rectTransform.anchorMax=new(.5f,1);lapCard.rectTransform.offsetMin=new(24,34);lapCard.rectTransform.offsetMax=new(-12,-20);
                raceCard=Label("Total race",r,25,0);raceCard.rectTransform.anchorMin=new(.5f,0);raceCard.rectTransform.anchorMax=new(1,1);raceCard.rectTransform.offsetMin=new(12,34);raceCard.rectTransform.offsetMax=new(-24,-20);
                var context=Label("Category",r,17,0);context.rectTransform.anchorMin=new(0,0);context.rectTransform.anchorMax=new(1,0);context.rectTransform.sizeDelta=new(-30,30);context.rectTransform.anchoredPosition=new(0,17);context.text="Local compatible records · "+flow.Race.courseName;
                var accent=Rect("Teal flourish",r);accent.anchorMin=new(0,1);accent.anchorMax=new(1,1);accent.pivot=new(0,1);accent.sizeDelta=new(0,5);finishAccent=accent.gameObject.AddComponent<UnityEngine.UI.Image>();finishAccent.color=new(.2f,.95f,.8f);
                var icon=Rect("Complete binding",simulateRemaining.transform);icon.anchorMin=icon.anchorMax=new(0,.5f);icon.pivot=new(0,.5f);icon.anchoredPosition=new(12,0);icon.sizeDelta=new(54,36);completeGlyph=icon.gameObject.AddComponent<MenuGlyph>();completeGlyph.raycastTarget=false;completeKey=Label("Key",icon,16,0);Stretch(completeKey.rectTransform,0,0,0,0);completeKey.alignment=TextAnchor.MiddleCenter;
            }
            finishPanel.SetActive(visible);
            if(visible)
            {
                if(presentedLap!=flow.FinishCards.Lap){presentedLap=flow.FinishCards.Lap;flourishAt=Time.unscaledTime;}
                lapCard.text="BEST LAP\n"+flow.FinishCards.Lap.Text;raceCard.text="TOTAL RACE\n"+flow.FinishCards.Race.Text;
                finishAccent.rectTransform.localScale=new(Mathf.Lerp(.08f,1,Mathf.Clamp01((Time.unscaledTime-flourishAt)/.8f)),1,1);
            }
            if(CanSimulateRemaining)
            {
                string binding=MenuInput.Binding(submit);completeGlyph.SetPath(binding);completeKey.text=MenuGlyph.Label(binding);
                var text=simulateRemaining.transform.Find("Label").GetComponent<UnityEngine.UI.Text>();text.text="COMPLETE RACE";text.rectTransform.offsetMin=new(75,0);
                banner.text="Finishes the remaining AI with estimated times.";
            }
        }
    }
}
