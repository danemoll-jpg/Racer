using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

namespace Racer
{
    // Runtime extension of the existing uGUI HUD; legacy font retained to match its text.
    public sealed class RaceMenus : MonoBehaviour
    {
        RaceFlow flow;
        GameObject shade;
        RectTransform card;
        UnityEngine.UI.Text title, details, banner;
        readonly List<UnityEngine.UI.Button> buttons = new();
        readonly Dictionary<RaceFlow.Stage, int> selections = new();
        RaceFlow.Stage shown;
        InputAction submit;
        InputActionAsset menuActions;
        InputActionReference submitReference;
        Font font;
        GameObject hudPanel;
        public void Initialize(RaceFlow owner)
        {
            flow = owner; font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var hud = FindAnyObjectByType<RaceHud>();
            hudPanel = hud.display.transform.parent.gameObject;
            var canvas = hud.GetComponent<Canvas>();
            if (!canvas.GetComponent<UnityEngine.UI.GraphicRaycaster>()) canvas.gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            var es = EventSystem.current;
            if (!es) es = new GameObject("Race menu EventSystem", typeof(EventSystem)).GetComponent<EventSystem>();
            var module = es.GetComponent<InputSystemUIInputModule>();
            if (!module) module = es.gameObject.AddComponent<InputSystemUIInputModule>();
            module.AssignDefaultActions();
            menuActions = ScriptableObject.CreateInstance<InputActionAsset>();
            var map = new InputActionMap("Race menu"); menuActions.AddActionMap(map);
            submit = map.AddAction("Menu confirm", InputActionType.Button);
            submit.AddBinding("<Keyboard>/space"); submit.AddBinding("<Gamepad>/buttonSouth");
            submitReference = InputActionReference.Create(submit); module.submit = submitReference;
            // Back is owned by RaceFlow; Enter/Start are exclusively pause/resume.
            module.cancel = null; submit.Enable();
            shade = Rect("Menu shade", canvas.transform).gameObject;
            Stretch(shade.GetComponent<RectTransform>(), 0, 0, 0, 0);
            shade.AddComponent<UnityEngine.UI.Image>().color = new Color(.015f, .025f, .04f, .78f);
            card = Rect("Race menu", shade.transform); card.anchorMin = card.anchorMax = card.pivot = new Vector2(.5f, .5f);
            card.sizeDelta = new Vector2(620, 620);
            card.gameObject.AddComponent<UnityEngine.UI.Image>().color = new Color(.035f, .065f, .085f, .98f);
            var layout = card.gameObject.AddComponent<UnityEngine.UI.VerticalLayoutGroup>();
            layout.padding = new RectOffset(28, 28, 20, 20); layout.spacing = 8;
            layout.childControlWidth = layout.childControlHeight = true; layout.childForceExpandHeight = false;
            title = Label("Title", card, 32, 48); title.color = new Color(.3f, .95f, .81f);
            details = Label("Details", card, 20, 160);
            for (int i = 0; i < 6; i++)
            {
                var rect = Rect("Action " + i, card);
                rect.gameObject.AddComponent<UnityEngine.UI.LayoutElement>().preferredHeight = 44;
                var img = rect.gameObject.AddComponent<UnityEngine.UI.Image>(); img.color = Color.white;
                var button = rect.gameObject.AddComponent<UnityEngine.UI.Button>(); button.targetGraphic = img;
                var colors = button.colors; colors.normalColor = new Color(.10f,.20f,.25f); colors.highlightedColor = new Color(.17f,.43f,.46f);
                colors.selectedColor = new Color(.17f,.43f,.46f); colors.pressedColor = new Color(.2f,.6f,.55f); colors.fadeDuration = .06f; button.colors = colors;
                var text = Label("Label", rect, 21, 0); Stretch(text.rectTransform, 10, 0, -10, 0); text.alignment = TextAnchor.MiddleCenter;
                buttons.Add(button);
            }
            var help = Label("Menu controls", card, 16, 38);
            help.text = "D-pad / stick / arrows: select     A / Space: confirm\nB / Esc: back     Enter / Start: pause or resume";
            banner = Label("Race feedback", canvas.transform, 26, 0);
            banner.alignment = TextAnchor.MiddleCenter; banner.color = new Color(.4f, 1, .85f);
            banner.rectTransform.anchorMin = new Vector2(.04f,.35f); banner.rectTransform.anchorMax = new Vector2(.96f,.6f);
            banner.rectTransform.offsetMin = banner.rectTransform.offsetMax = Vector2.zero;
            var outline = banner.gameObject.AddComponent<UnityEngine.UI.Outline>(); outline.effectColor = new Color(0,0,0,.85f); outline.effectDistance = new Vector2(2,-2);
            shown = RaceFlow.Stage.Ready;
        }
        static RectTransform Rect(string name, Transform parent)
        { var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>(); rect.SetParent(parent, false); return rect; }
        static void Stretch(RectTransform rect, float left, float bottom, float right, float top)
        { rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = new Vector2(left,bottom); rect.offsetMax = new Vector2(right,top); }
        UnityEngine.UI.Text Label(string name, Transform parent, int size, float height)
        {
            var rect = Rect(name, parent); var text = rect.gameObject.AddComponent<UnityEngine.UI.Text>();
            text.font = font; text.fontSize = size; text.color = Color.white; text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Wrap; text.verticalOverflow = VerticalWrapMode.Truncate;
            if (height > 0) rect.gameObject.AddComponent<UnityEngine.UI.LayoutElement>().preferredHeight = height;
            return text;
        }
        void Action(int index, string label, UnityEngine.Events.UnityAction action)
        {
            var b = buttons[index]; b.gameObject.SetActive(true); b.GetComponentInChildren<UnityEngine.UI.Text>().text = label;
            b.onClick.RemoveAllListeners(); b.onClick.AddListener(action);
        }
        string Record(double seconds) => seconds > 0 ? RaceHud.FormatTime(seconds) : "—";
        public void Show()
        {
            if (!shade) return;
            int selected = buttons.FindIndex(b => EventSystem.current && EventSystem.current.currentSelectedGameObject == b.gameObject);
            if (selected >= 0) selections[shown] = selected;
            shown = flow.State; shade.SetActive(flow.MenuVisible);
            hudPanel.SetActive(!flow.MenuVisible);
            EventSystem.current.SetSelectedGameObject(null);
            if (!flow.MenuVisible) return;
            foreach (var b in buttons) b.gameObject.SetActive(false);
            details.GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight = 160;
            if (shown == RaceFlow.Stage.Ready)
            {
                title.text = "RACER / STREET LOOP";
                details.text = "Three laps through the neighborhood.\nWait for GO, then cross START to begin timing.\n\nPersonal best lap   " + Record(flow.Save.Best.lap) + "\nPersonal best race  " + Record(flow.Save.Best.race);
                Action(0,"Start race",flow.StartRace); Action(1,"Settings",flow.OpenSettings); Action(2,"Quit",flow.Quit);
            }
            else if (shown == RaceFlow.Stage.Paused)
            {
                title.text = "PAUSED";
                details.text = "The race and countdown are stopped.\nResume to continue exactly where you left off.\n\nRestart Race clears this race and restores props.\nR / Y while driving resets only the car and current lap.";
                Action(0,"Resume",flow.Resume); Action(1,"Restart race",flow.StartRace); Action(2,"Settings",flow.OpenSettings); Action(3,"Quit",flow.Quit);
            }
            else if (shown == RaceFlow.Stage.Results)
            {
                title.text = "RACE COMPLETE";
                var p = flow.Race.Progress; var text = new StringBuilder();
                text.AppendLine("Total   " + RaceHud.FormatTime(p.RaceTime(flow.Race.Clock)) + (flow.NewRaceRecord ? "   NEW PB" : ""));
                for (int i = 0; i < p.LapTimes.Count; i++) text.AppendLine("Lap " + (i+1) + "   " + RaceHud.FormatTime(p.LapTimes[i]));
                text.AppendLine("Best lap   " + RaceHud.FormatTime(p.BestLap) + (flow.NewLapRecord ? "   NEW PB" : ""));
                details.text = text.ToString();
                Action(0,"Race again",flow.StartRace); Action(1,"Settings",flow.OpenSettings); Action(2,"Quit",flow.Quit);
            }
            else if (shown == RaceFlow.Stage.Settings)
            {
                title.text = "SETTINGS"; details.GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight = 96;
                details.text = "Select a setting to cycle its value. Changes apply now.\nVSync uses your display refresh; frame limit applies with VSync off.\nVolume steps: 0–100% in 10% increments.";
                var s = flow.Save.Settings;
                Action(0,$"Master volume   {s.master:P0}",()=>Adjust(()=>s.master=NextVolume(s.master)));
                Action(1,$"Ambience volume   {s.ambience:P0}",()=>Adjust(()=>s.ambience=NextVolume(s.ambience)));
                Action(2,$"Race / UI volume   {s.feedback:P0}",()=>Adjust(()=>s.feedback=NextVolume(s.feedback)));
                Action(3,"VSync   " + (s.vsync?"On":"Off"),()=>Adjust(()=>s.vsync=!s.vsync));
                Action(4,"Frame limit   " + s.frameLimit + " fps",()=>Adjust(()=>s.frameLimit=s.frameLimit==30?60:s.frameLimit==60?120:30));
                Action(5,"Back",flow.CloseSettings);
            }
            var active = buttons.FindAll(b=>b.gameObject.activeSelf);
            for (int i=0;i<active.Count;i++) active[i].navigation = new UnityEngine.UI.Navigation { mode=UnityEngine.UI.Navigation.Mode.Explicit, selectOnUp=active[(i+active.Count-1)%active.Count], selectOnDown=active[(i+1)%active.Count] };
            int focus = selections.TryGetValue(shown,out var prior)?prior:0;
            if (focus >= buttons.Count || !buttons[focus].gameObject.activeSelf) focus=0;
            EventSystem.current.SetSelectedGameObject(buttons[focus].gameObject);
        }
        static float NextVolume(float value) => value >= .99f ? 0 : Mathf.Min(1, (Mathf.Floor(value*10+.01f)+1)/10);
        void Adjust(System.Action action) { action(); flow.Save.ApplySettings(); flow.Save.SaveSettings(); flow.Click(); Show(); }
        void LateUpdate()
        {
            if (!flow || !banner) return;
            banner.gameObject.SetActive(!flow.MenuVisible);
            banner.text = flow.State == RaceFlow.Stage.Countdown ? "READY\n" + Mathf.CeilToInt(flow.CountdownRemaining) : flow.Notice ?? "";
            if (flow.MenuVisible && EventSystem.current && !EventSystem.current.currentSelectedGameObject) EventSystem.current.SetSelectedGameObject(buttons[0].gameObject);
        }
        void OnDestroy() { if(menuActions) { menuActions.Disable(); Destroy(menuActions); } if(submitReference) Destroy(submitReference); }
    }
}
