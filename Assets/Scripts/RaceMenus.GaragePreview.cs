using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Racer
{
    // 0.76 garage preview (vehicle page and Rider page). The preview stands in a fixed panel on the left of a wider card and
    // never scrolls; only the option list on the right scrolls. Its render texture always matches the panel's real pixel
    // size (the 0.75 texture was 640x280 shown in a 620x240 box, which squashed the vehicle). The camera frames the whole
    // vehicle (or, on the Rider page, the whole rider from head to feet; a car's body is hidden there) from a three-quarter
    // view. Right stick / Q and E / mouse drag turn it a full 360 degrees; left alone for a few seconds it turns slowly.
    public sealed partial class RaceMenus
    {
        RectTransform garagePanel; Vector2 menuCardSize; bool garageView;
        float previewYaw = 215, lastTurn = -100, previewRadius = 1, previewMargin = 1.04f; Vector3 previewCenter;
        InputAction rotateAction; readonly List<int> stickOverrides = new();
        static readonly Vector3 PreviewFocus = new(10000, 10000, 10000);
        const float PreviewPitch = 11, IdleTurnDelay = 3, IdleTurnSpeed = 12, TurnSpeed = 140;

        void EnterGarageView()
        {
            if (!garagePanel)
            {
                menuCardSize = card.sizeDelta;
                garagePanel = Rect("Garage preview panel", card);
                garagePanel.gameObject.AddComponent<UnityEngine.UI.Image>().color = new Color(.06f, .1f, .13f);
                rotateAction = new InputAction("Rotate preview", InputActionType.Value);
                rotateAction.AddBinding("<Gamepad>/rightStick/x");
                rotateAction.AddCompositeBinding("1DAxis").With("Negative", "<Keyboard>/q").With("Positive", "<Keyboard>/e");
            }
            // As wide as the screen allows (lower resolutions and narrower aspect ratios included).
            var parent = (RectTransform)card.parent;
            float width = Mathf.Min(1220, parent.rect.width - 40), height = Mathf.Min(menuCardSize.y, parent.rect.height - 24);
            card.sizeDelta = new Vector2(width, height);
            float panel = Mathf.Clamp(width * .5f, 340, 600);
            garagePanel.gameObject.SetActive(true);
            garagePanel.anchorMin = new Vector2(0, 0); garagePanel.anchorMax = new Vector2(0, 1); garagePanel.pivot = new Vector2(0, .5f);
            garagePanel.offsetMin = new Vector2(32, 78); garagePanel.offsetMax = new Vector2(32 + panel, -82);
            ((RectTransform)scroll.transform).offsetMin = new Vector2(32 + panel + 24, 78);
            preview.transform.SetParent(garagePanel, false); Stretch(preview.rectTransform, 0, 0, 0, 0);
            preview.GetComponent<UnityEngine.UI.LayoutElement>().ignoreLayout = true; preview.raycastTarget = true;
            if (!preview.GetComponent<GaragePreviewDrag>()) preview.gameObject.AddComponent<GaragePreviewDrag>().owner = this;
            if (!garageView) { garageView = true; rotateAction.Enable(); StickNavigation(false); }
            MatchPreviewTexture(); ShowPreviewLock();
        }
        // 0.90 Part A: a locked vehicle in the preview (Garage or Shop): its silhouette with a padlock and how to get it.
        string previewLock; GameObject lockBadge; UnityEngine.UI.Text lockText;
        void ShowPreviewLock()
        {
            if (!lockBadge)
            {
                var r = Rect("Locked vehicle badge", garagePanel); r.anchorMin = new Vector2(0, 0); r.anchorMax = new Vector2(1, 0); r.pivot = new Vector2(.5f, 0); r.sizeDelta = new Vector2(0, 74); r.anchoredPosition = Vector2.zero;
                r.gameObject.AddComponent<UnityEngine.UI.Image>().color = new Color(.02f, .04f, .05f, .82f); r.GetComponent<UnityEngine.UI.Image>().raycastTarget = false;
                PadlockMark.Add(r, new Vector2(0, .5f), new Vector2(18, 0), 46, new Color(1, .82f, .35f));
                lockText = Label("How to get it", r, 19, 0); lockText.rectTransform.anchorMin = Vector2.zero; lockText.rectTransform.anchorMax = Vector2.one; lockText.rectTransform.offsetMin = new Vector2(70, 4); lockText.rectTransform.offsetMax = new Vector2(-12, -4);
                lockText.alignment = TextAnchor.MiddleLeft; lockText.color = new Color(1, .9f, .62f); lockBadge = r.gameObject;
            }
            lockBadge.SetActive(previewLock != null); lockBadge.transform.SetAsLastSibling();
            if (previewLock != null) lockText.text = "LOCKED\n" + previewLock;
        }
        void LeaveGarageView()
        {
            if (!garageView) return;
            garageView = false; rotateAction.Disable(); StickNavigation(true); if (lockBadge) lockBadge.SetActive(false);
            card.sizeDelta = menuCardSize; ((RectTransform)scroll.transform).offsetMin = new Vector2(32, 78);
            garagePanel.gameObject.SetActive(false);
            preview.transform.SetParent(content, false); preview.GetComponent<UnityEngine.UI.LayoutElement>().ignoreLayout = false; preview.raycastTarget = false;
        }
        // The right stick turns the preview, so it does not also move the menu selection while the garage is open.
        void StickNavigation(bool on)
        {
            var action = uiModule ? NavigateAction : null; if (action == null) return;
            if (!on) { for (int i = 0; i < action.bindings.Count; i++) if ((action.bindings[i].path ?? "").Contains("rightStick") && !stickOverrides.Contains(i)) { action.ApplyBindingOverride(i, ""); stickOverrides.Add(i); } }
            else { foreach (int i in stickOverrides) action.RemoveBindingOverride(i); stickOverrides.Clear(); }
        }
        // Render texture = the panel's size in screen pixels (true proportions, sharp at 3840x2160).
        void MatchPreviewTexture()
        {
            Canvas.ForceUpdateCanvases();
            var canvas = preview.canvas ? preview.canvas.rootCanvas : null; float scale = canvas ? canvas.scaleFactor : 1;
            var size = preview.rectTransform.rect.size * scale;
            int w = Mathf.Clamp(Mathf.RoundToInt(size.x), 64, 2400), h = Mathf.Clamp(Mathf.RoundToInt(size.y), 64, 2400);
            if (previewTexture.width == w && previewTexture.height == h) return;
            previewTexture.Release(); previewTexture.width = w; previewTexture.height = h; previewTexture.antiAliasing = 4; previewTexture.Create();
            // re-assigned so the camera picks up the new size (it otherwise keeps the old aspect and squeezes the picture)
            previewCamera.targetTexture = null; previewCamera.targetTexture = previewTexture; preview.texture = previewTexture;
            if (previewRoot) PlacePreview();
        }
        // Called after each rebuild of the display model (vehicle, colour, Classic / New and rider changes all rebuild it).
        void FramePreview(VehicleProfile profile)
        {
            if (!previewRoot) return;
            var root = previewRoot.transform; root.SetPositionAndRotation(PreviewFocus, Quaternion.identity);
            bool riderPage = page == "rider" && VehicleVisual.NewModels;
            var rider = root.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == "Rider");
            if (riderPage && rider && !profile.Small) foreach (var r in previewRoot.GetComponentsInChildren<Renderer>(true)) if (!r.transform.IsChildOf(rider)) r.enabled = false;
            var shown = (riderPage && rider ? rider.GetComponentsInChildren<Renderer>() : previewRoot.GetComponentsInChildren<Renderer>()).Where(r => r.enabled && !(r is ParticleSystemRenderer)).ToArray();
            if (shown.Length == 0) return;
            var b = shown[0].bounds; foreach (var r in shown) b.Encapsulate(r.bounds);
            previewCenter = root.InverseTransformPoint(b.center); previewRadius = Mathf.Max(.3f, b.extents.magnitude); previewMargin = riderPage ? 1.16f : 1.04f;
            PlacePreview();
        }
        void PlacePreview()
        {
            if (!previewRoot) return;
            var turn = Quaternion.Euler(0, previewYaw, 0); var root = previewRoot.transform;
            root.SetPositionAndRotation(PreviewFocus - turn * previewCenter, turn);
            float aspect = Mathf.Max(.2f, (float)previewTexture.width / previewTexture.height), half = previewCamera.fieldOfView * .5f * Mathf.Deg2Rad;
            float fit = Mathf.Min(half, Mathf.Atan(Mathf.Tan(half) * aspect)), distance = previewRadius * previewMargin / Mathf.Sin(fit);
            var look = Quaternion.Euler(PreviewPitch, 0, 0); previewCamera.transform.SetPositionAndRotation(PreviewFocus - look * Vector3.forward * distance, look);
            previewCamera.aspect = aspect; previewCamera.nearClipPlane = .05f; previewCamera.farClipPlane = distance + previewRadius * 2 + 5;
        }
        void UpdateGaragePreview()
        {
            if (!garageView || !previewRoot) return;
            MatchPreviewTexture();
            float turn = rotateAction.ReadValue<float>();
            if (Mathf.Abs(turn) > .15f) { previewYaw += turn * TurnSpeed * Time.unscaledDeltaTime; lastTurn = Time.unscaledTime; }
            else if (Time.unscaledTime - lastTurn > IdleTurnDelay) previewYaw += IdleTurnSpeed * Time.unscaledDeltaTime;
            previewYaw = Mathf.Repeat(previewYaw, 360); PlacePreview();
        }
        public void DragPreview(float dx) { previewYaw -= dx * .45f; lastTurn = Time.unscaledTime; PlacePreview(); }
        public float PreviewYaw { get => previewYaw; set { previewYaw = value; lastTurn = Time.unscaledTime; PlacePreview(); } }
    }
    public sealed class GaragePreviewDrag : MonoBehaviour, IDragHandler
    {
        public RaceMenus owner;
        public void OnDrag(PointerEventData e) { if (owner) owner.DragPreview(e.delta.x); }
    }
}
