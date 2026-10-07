using UnityEngine;
using UnityEngine.UI;

namespace Racer
{
    // 0.90 Part A: the padlock drawn on locked vehicles (the preview) and locked tracks (their rows). Vector, like MenuGlyph,
    // so it stays crisp at every menu resolution: a body with a keyhole and a shackle above it.
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class PadlockMark : MaskableGraphic
    {
        public static PadlockMark Add(Transform parent, Vector2 anchor, Vector2 position, float size, Color tint)
        {
            var mark = new GameObject("Padlock", typeof(RectTransform)).AddComponent<PadlockMark>();
            mark.transform.SetParent(parent, false); mark.raycastTarget = false; mark.color = tint;
            var r = mark.rectTransform; r.anchorMin = r.anchorMax = r.pivot = anchor; r.anchoredPosition = position; r.sizeDelta = new Vector2(size * .8f, size);
            return mark;
        }
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear(); var r = GetPixelAdjustedRect(); var c = color;
            void Box(float x, float y, float w, float h, Color tint)
            { int i = vh.currentVertCount; vh.AddVert(new Vector2(x, y), tint, Vector2.zero); vh.AddVert(new Vector2(x + w, y), tint, Vector2.zero); vh.AddVert(new Vector2(x + w, y + h), tint, Vector2.zero); vh.AddVert(new Vector2(x, y + h), tint, Vector2.zero); vh.AddTriangle(i, i + 1, i + 2); vh.AddTriangle(i, i + 2, i + 3); }
            float w = r.width, h = r.height, bar = Mathf.Max(2, w * .16f);
            // shackle: an arch of short segments over two legs
            float cx = r.center.x, rx = w * .30f, legTop = r.yMin + h * .62f;
            Box(cx - rx - bar * .5f, r.yMin + h * .45f, bar, legTop - (r.yMin + h * .45f), c);
            Box(cx + rx - bar * .5f, r.yMin + h * .45f, bar, legTop - (r.yMin + h * .45f), c);
            const int steps = 10; float ry = h * .30f;
            for (int k = 0; k < steps; k++)
            {
                float a0 = Mathf.PI * k / steps, a1 = Mathf.PI * (k + 1) / steps;
                var p0 = new Vector2(cx + Mathf.Cos(a0) * rx, legTop + Mathf.Sin(a0) * ry); var p1 = new Vector2(cx + Mathf.Cos(a1) * rx, legTop + Mathf.Sin(a1) * ry);
                var d = (p1 - p0).normalized; var n = new Vector2(-d.y, d.x) * bar * .5f; int i = vh.currentVertCount;
                vh.AddVert(p0 - n, c, Vector2.zero); vh.AddVert(p0 + n, c, Vector2.zero); vh.AddVert(p1 + n, c, Vector2.zero); vh.AddVert(p1 - n, c, Vector2.zero);
                vh.AddTriangle(i, i + 1, i + 2); vh.AddTriangle(i, i + 2, i + 3);
            }
            // body and keyhole
            Box(r.xMin, r.yMin, w, h * .52f, c);
            var hole = new Color(.04f, .07f, .09f, c.a);
            Box(cx - w * .09f, r.yMin + h * .12f, w * .18f, h * .26f, hole);
        }
    }
}
