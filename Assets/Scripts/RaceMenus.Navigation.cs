using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Racer
{
    // 0.92 Part A: Dan still found CAMPAIGN and SPLIT SCREEN skipped in the 0.91 game. Its up / down links were right (the
    // built game, driven from a cold launch with an emulated controller and through the connected XInput controller's own
    // device, went down every page row by row), so this removes the two things left that could make a press land on the
    // wrong row or look as if it had:
    // 1. Navigation: Unity's UI module stepped once per navigation event, and every device bound to Navigate (each
    //    controller, the keyboard) reports its own; a press seen twice (a controller and an input mapper, two devices for one
    //    pad, the stick and the D-pad together) stepped twice. While a menu page is up the menu now moves the focus itself:
    //    one held direction from every allowed device together, one row per press, repeating after 0.45 s held (every 0.11
    //    s), along the page's up / down / left / right links (the rows as drawn).
    // 2. Focus: the CAMPAIGN row's own colour was almost the highlight's colour, and a row under a resting mouse pointer
    //    lit up like the focused one. The focused row now has a bright frame and marker; the pointer only highlights a row
    //    while the mouse is in use, and then selects it, so only one row is ever lit.
    public sealed partial class RaceMenus
    {
        bool OwnsNavigation => flow && flow.MenuVisible && flow.State != RaceFlow.Stage.Title && flow.GetComponent<ExplorationMap>()?.OwnsInput != true && !DeveloperLocationHud.OwnsInput && !SplitPickOpen;
        Vector2Int navHeld; float navNext;
        public int NavigationSteps { get; private set; }
        static bool Allowed(InputDevice d, InputDevice[] only) => only == null || only.Contains(d);
        // The direction held on every allowed device together (D-pad, left stick, arrow keys); up / down win a diagonal.
        Vector2Int HeldDirection()
        {
            float x = 0, y = 0;
            foreach (var pad in Gamepad.all)
            {
                if (!pad.added || !Allowed(pad, menuDevices)) continue;
                var v = pad.dpad.ReadValue(); var s = pad.leftStick.ReadValue(); if (s.magnitude > .6f) v += s;
                x += v.x; y += v.y;
            }
            var k = Keyboard.current;
            if (k != null && Allowed(k, menuDevices) && !editingPlaylistName) { if (k.upArrowKey.isPressed) y += 1; if (k.downArrowKey.isPressed) y -= 1; if (k.leftArrowKey.isPressed) x -= 1; if (k.rightArrowKey.isPressed) x += 1; }
            if (Mathf.Abs(y) >= .5f && Mathf.Abs(y) >= Mathf.Abs(x)) return new Vector2Int(0, y > 0 ? 1 : -1);
            if (Mathf.Abs(x) >= .5f) return new Vector2Int(x > 0 ? 1 : -1, 0);
            return Vector2Int.zero;
        }
        void UpdateNavigation()
        {
            if (!OwnsNavigation || !EventSystem.current) { navHeld = Vector2Int.zero; return; }
            var dir = HeldDirection();
            if (dir == Vector2Int.zero) { navHeld = Vector2Int.zero; return; }
            bool first = dir != navHeld; if (!first && Time.unscaledTime < navNext) return;
            navHeld = dir; navNext = Time.unscaledTime + (first ? .45f : .11f);
            var current = EventSystem.current.currentSelectedGameObject; var from = current ? current.GetComponent<UnityEngine.UI.Selectable>() : null;
            if (!from || !from.gameObject.activeInHierarchy)
            {
                var top = buttons.Where(b => b.gameObject.activeInHierarchy && b.interactable).OrderBy(b => b.transform, Drawn).FirstOrDefault();
                if (top) EventSystem.current.SetSelectedGameObject(top.gameObject); return;
            }
            var next = dir.y > 0 ? from.FindSelectableOnUp() : dir.y < 0 ? from.FindSelectableOnDown() : dir.x < 0 ? from.FindSelectableOnLeft() : from.FindSelectableOnRight();
            if (next && next.gameObject.activeInHierarchy && next.interactable) { EventSystem.current.SetSelectedGameObject(next.gameObject); NavigationSteps++; }
        }
        UnityEngine.InputSystem.InputActionReference moduleMove; bool moduleMoveOn = true;
        // The UI module's own move (stick / D-pad / arrows navigation) on or off; submit and the rest are untouched.
        void ModuleMove(bool on)
        {
            if (!uiModule || on == moduleMoveOn) return;
            if (!on) { moduleMove = uiModule.move; uiModule.move = null; } else if (moduleMove) uiModule.move = moduleMove;
            moduleMoveOn = on;
        }
        UnityEngine.InputSystem.InputAction NavigateAction => (moduleMove ? moduleMove : uiModule.move)?.action;
        static readonly System.Collections.Generic.Comparer<Transform> Drawn = System.Collections.Generic.Comparer<Transform>.Create(ScreenOrder);

        // ---------- the focus frame ----------
        RectTransform focusFrame;
        void UpdateFocusFrame()
        {
            if (!focusFrame)
            {
                focusFrame = Rect("Focus frame", card);
                void Edge(string name, Vector2 min, Vector2 max, Vector2 offMin, Vector2 offMax, Color c) { var r = Rect(name, focusFrame); r.anchorMin = min; r.anchorMax = max; r.offsetMin = offMin; r.offsetMax = offMax; var i = r.gameObject.AddComponent<UnityEngine.UI.Image>(); i.color = c; i.raycastTarget = false; }
                var white = new Color(1, 1, 1, .95f);
                Edge("Top", new(0, 1), new(1, 1), new(0, -3), Vector2.zero, white); Edge("Bottom", Vector2.zero, new(1, 0), Vector2.zero, new(0, 3), white);
                Edge("Left", Vector2.zero, new(0, 1), Vector2.zero, new(3, 0), white); Edge("Right", new(1, 0), Vector2.one, new(-3, 0), Vector2.zero, white);
                Edge("Marker", Vector2.zero, new(0, 1), new(3, 3), new(11, -3), new Color(.3f, .95f, .81f));
            }
            var current = EventSystem.current ? EventSystem.current.currentSelectedGameObject : null;
            bool show = flow.MenuVisible && current && current.activeInHierarchy && current.transform.IsChildOf(card) && current.GetComponent<UnityEngine.UI.Selectable>() && !current.GetComponent<UnityEngine.UI.InputField>();
            if (!show) { if (focusFrame.parent != card) focusFrame.SetParent(card, false); focusFrame.gameObject.SetActive(false); return; }
            focusFrame.gameObject.SetActive(true);
            if (focusFrame.parent != current.transform) { focusFrame.SetParent(current.transform, false); Stretch(focusFrame, 0, 0, 0, 0); }
            focusFrame.SetAsLastSibling();
        }
    }
    // 0.92 Part A: the mouse pointer highlights a row only while the mouse is in use, and then selects it, so the pointer
    // resting over the menu never lights a second row beside the controller's focus.
    public sealed class MenuHoverSelect : MonoBehaviour, IPointerEnterHandler, IPointerMoveHandler
    {
        public void OnPointerEnter(PointerEventData e) => Pick();
        public void OnPointerMove(PointerEventData e) => Pick();
        void Pick()
        {
            if (MenuInput.Controller || !EventSystem.current) return;
            var s = GetComponent<UnityEngine.UI.Selectable>(); if (s && s.interactable && EventSystem.current.currentSelectedGameObject != gameObject) EventSystem.current.SetSelectedGameObject(gameObject);
        }
    }
}
