using System.Collections.Generic;
using UnityEngine;

namespace Racer
{
    // One clipped UI mesh, built from the active race's existing centrelines.
    // No camera, render texture, world-map artwork or duplicate route metadata.
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class RacingMiniMap : UnityEngine.UI.MaskableGraphic
    {
        public RaceDirector race;
        readonly List<(Vector3 a, Vector3 b, bool shortcut)> segments = new();
        static readonly Color Main = new(.2f, .86f, .86f), Optional = new(1, .69f, .17f);
        Vector3 origin, forward, right;
        float nextUpdate;
        public const float Scale = .72f;
        public static readonly Vector2 PlayerPoint = new(0, -48);

        public static RacingMiniMap Create(Transform parent, RaceDirector owner, Font font)
        {
            var panel = new GameObject("Local racing map", typeof(RectTransform), typeof(UnityEngine.UI.Image));
            panel.transform.SetParent(parent, false);
            var rect = (RectTransform)panel.transform;
            rect.anchorMin = rect.anchorMax = rect.pivot = new(1, 1);
            rect.anchoredPosition = new(-18, -18); rect.sizeDelta = new(236, 220);
            panel.GetComponent<UnityEngine.UI.Image>().color = new(.025f, .055f, .07f, .9f);
            panel.GetComponent<UnityEngine.UI.Image>().raycastTarget = false;
            var clip = new GameObject("Clipped route window", typeof(RectTransform), typeof(UnityEngine.UI.RectMask2D));
            clip.transform.SetParent(rect, false);
            var cr = (RectTransform)clip.transform; cr.sizeDelta = new(224, 184); cr.anchoredPosition = new(0, 10);
            var map = new GameObject("Active course and racers", typeof(RectTransform)).AddComponent<RacingMiniMap>();
            map.transform.SetParent(cr, false); map.rectTransform.sizeDelta = cr.sizeDelta; map.raycastTarget = false; map.race = owner;
            var label = new GameObject("Route key", typeof(RectTransform)).AddComponent<UnityEngine.UI.Text>();
            label.transform.SetParent(rect, false); label.font = font; label.fontSize = 13; label.alignment = TextAnchor.MiddleCenter;
            label.raycastTarget = false; label.supportRichText = true;
            label.text = "<color=#33DBDB>MAIN</color>   <color=#FFB02B>SHORTCUT</color>   • RIVALS";
            label.rectTransform.sizeDelta = new(232, 24); label.rectTransform.anchoredPosition = new(0, -95);
            map.CacheRoutes(); return map;
        }

        void CacheRoutes()
        {
            Add(race.road.points, true, false);
            foreach (var route in race.Branches ?? System.Array.Empty<WoodlandRoute>())
                if (route && route.isActiveAndEnabled) Add(route.points, false, true);
        }
        void Add(Vector3[] points, bool closed, bool shortcut)
        {
            if (points == null || points.Length < 2) return;
            var previous = points[0];
            for (int i = 1; i < points.Length; i++)
                if ((points[i] - previous).sqrMagnitude >= 9 || i == points.Length - 1)
                { segments.Add((previous, points[i], shortcut)); previous = points[i]; }
            if (closed) segments.Add((previous, points[0], shortcut));
        }
        void LateUpdate()
        {
            bool visible = race && race.vehicle && race.Flow && !race.Flow.MenuVisible && !race.FreeRoam;
            // Keep this component active so it can restore its parent after menus.
            canvasRenderer.SetAlpha(visible ? 1 : 0);
            var panel = transform.parent.parent;
            // Narrow windows need vertical separation from the fixed left-hand lap panel.
            var canvasRect = panel.parent as RectTransform;
            bool narrow = canvasRect && canvasRect.rect.width < 680;
            panel.localScale = Vector3.one * (narrow ? .72f : 1);
            ((RectTransform)panel).anchoredPosition = new(-18, narrow ? -174 : -18);
            panel.GetComponent<UnityEngine.UI.Image>().enabled = visible;
            panel.GetComponentInChildren<UnityEngine.UI.Text>().enabled = visible;
            if (!visible || Time.unscaledTime < nextUpdate) return;
            nextUpdate = Time.unscaledTime + .05f;
            origin = race.vehicle.transform.position;
            forward = Vector3.ProjectOnPlane(race.vehicle.transform.forward, Vector3.up).normalized;
            if (forward.sqrMagnitude < .5f) forward = Vector3.forward;
            right = Vector3.Cross(Vector3.up, forward);
            SetVerticesDirty();
        }
        public Vector2 Project(Vector3 p)
        {
            var d = p - origin;
            return PlayerPoint + new Vector2(Vector3.Dot(d, right), Vector3.Dot(d, forward)) * Scale;
        }
        protected override void OnPopulateMesh(UnityEngine.UI.VertexHelper vh)
        {
            vh.Clear(); if (!race || !race.vehicle) return;
            var bounds = rectTransform.rect;
            foreach (var s in segments)
            {
                var a = Project(s.a); var b = Project(s.b);
                if (Mathf.Max(a.x,b.x)<bounds.xMin-5 || Mathf.Min(a.x,b.x)>bounds.xMax+5 || Mathf.Max(a.y,b.y)<bounds.yMin-5 || Mathf.Min(a.y,b.y)>bounds.yMax+5) continue;
                Line(vh, a, b, s.shortcut ? 3 : 5, s.shortcut ? Optional : Main);
            }
            foreach (var r in race.Racers)
            {
                if (!r.IsAi || !r.Car || r.Dnf) continue;
                var p = Project(r.Car.transform.position);
                if (!bounds.Contains(p)) continue;
                Diamond(vh, p, 5, new(.03f,.06f,.08f)); Diamond(vh, p, 3.5f, Color.white);
            }
            // Fixed heading-up player arrow, below centre to favour upcoming turns.
            Triangle(vh, PlayerPoint + new Vector2(0, 9), PlayerPoint + new Vector2(-7, -6), PlayerPoint + new Vector2(7, -6), new(.02f,.04f,.05f));
            Triangle(vh, PlayerPoint + new Vector2(0, 7), PlayerPoint + new Vector2(-4.5f, -4), PlayerPoint + new Vector2(4.5f, -4), new(1,.3f,.3f));
        }
        static void Line(UnityEngine.UI.VertexHelper vh, Vector2 a, Vector2 b, float width, Color c)
        {
            var d = b - a; if (d.sqrMagnitude < .001f) return;
            var n = new Vector2(-d.y,d.x).normalized * width * .5f;
            int i = vh.currentVertCount;
            vh.AddVert(a-n,c,Vector2.zero); vh.AddVert(a+n,c,Vector2.zero); vh.AddVert(b+n,c,Vector2.zero); vh.AddVert(b-n,c,Vector2.zero);
            vh.AddTriangle(i,i+1,i+2); vh.AddTriangle(i,i+2,i+3);
        }
        static void Triangle(UnityEngine.UI.VertexHelper vh, Vector2 a, Vector2 b, Vector2 c, Color color)
        { int i=vh.currentVertCount; vh.AddVert(a,color,Vector2.zero); vh.AddVert(b,color,Vector2.zero); vh.AddVert(c,color,Vector2.zero); vh.AddTriangle(i,i+1,i+2); }
        static void Diamond(UnityEngine.UI.VertexHelper vh, Vector2 p, float r, Color c)
        { Triangle(vh,p+Vector2.up*r,p+Vector2.left*r,p+Vector2.down*r,c); Triangle(vh,p+Vector2.up*r,p+Vector2.down*r,p+Vector2.right*r,c); }
    }
}
