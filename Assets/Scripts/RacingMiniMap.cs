using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Racer
{
    // One clipped UI mesh, built from the active race's existing centrelines.
    // No camera, render texture, world-map artwork or duplicate route metadata.
    // 0.79 Part C: the same minimap in Free Roam, drawing the world's roads (paved) and trails from the scene's own road
    // centrelines, the activity sites and the map waypoint. J / B (controller) turns it off and on in Free Roam; the choice
    // is saved (default on). Races are unchanged.
    [RequireComponent(typeof(CanvasRenderer))]
    [DefaultExecutionOrder(-100)] // reads its toggle before the menus and the map act on the same press (B is also Back)
    public sealed class RacingMiniMap : UnityEngine.UI.MaskableGraphic
    {
        public RaceDirector race;
        readonly List<(Vector3 a, Vector3 b, bool shortcut)> segments = new();
        readonly List<(Vector3 a, Vector3 b, bool shortcut)> roamSegments = new(); bool roamCached;
        static readonly Color Main = new(.2f, .86f, .86f), Optional = new(1, .69f, .17f);
        static readonly Color Road = new(.84f, .86f, .88f), Trail = new(.80f, .60f, .34f), Waypoint = new(1, .84f, .2f);
        public const string RaceKey = "<color=#33DBDB>MAIN</color>   <color=#FFB02B>SHORTCUT</color>   • RIVALS";
        public const string RoamKey = "<color=#D6DBE0>ROADS</color>  <color=#CC9957>TRAILS</color>  <color=#FFB02B>◆</color>SITES  <color=#FFD633>◆</color>WAYPOINT";
        UnityEngine.InputSystem.InputAction toggle;
        public UnityEngine.InputSystem.InputAction ToggleAction => toggle;
        public static RacingMiniMap Instance { get; private set; }
        // 0.90 Part D: a split-screen half's own minimap: centred on Focus, the other player drawn as a coloured marker.
        [System.NonSerialized] public bool Split; [System.NonSerialized] public ArcadeVehicle Focus;
        ArcadeVehicle Vehicle => Focus ? Focus : race ? race.vehicle : null;
        public bool Shown { get; private set; }
        Vector3 origin, forward, right;
        float nextUpdate;
        public const float Scale = .72f;
        public static readonly Vector2 PlayerPoint = new(0, -48);
        // 0.93 Part D: a compass on the rim. The map turns with the vehicle (heading up), so the N moves round the rim to
        // where north (world +Z) is; small ticks mark E, S and W. Drawn in race, Free Roam and each split-screen half.
        UnityEngine.UI.Text north; Vector2 northDir = Vector2.up;
        // 0.93 Part C: in Free Roam, under the minimap, the acorn area the player is in while it still has acorns
        UnityEngine.UI.Text areaLine; float nextArea;
        const float RimInset = 11;
        Vector2 Rim(Vector2 d)
        {
            var r = rectTransform.rect; float hx = r.width * .5f - RimInset, hy = r.height * .5f - RimInset;
            float t = Mathf.Min(Mathf.Abs(d.x) > 1e-4f ? hx / Mathf.Abs(d.x) : float.MaxValue, Mathf.Abs(d.y) > 1e-4f ? hy / Mathf.Abs(d.y) : float.MaxValue);
            return r.center + d * t;
        }

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
            label.text = RaceKey;
            label.rectTransform.sizeDelta = new(232, 24); label.rectTransform.anchoredPosition = new(0, -95);
            var n = new GameObject("Compass N", typeof(RectTransform)).AddComponent<UnityEngine.UI.Text>();
            n.transform.SetParent(cr, false); n.font = font; n.fontSize = 16; n.fontStyle = FontStyle.Bold; n.alignment = TextAnchor.MiddleCenter;
            n.color = Color.white; n.raycastTarget = false; n.text = "N"; n.rectTransform.sizeDelta = new(22, 22);
            var edge = n.gameObject.AddComponent<UnityEngine.UI.Outline>(); edge.effectColor = new(0, 0, 0, .9f); edge.effectDistance = new(1.2f, -1.2f);
            map.north = n;
            var line = new GameObject("Acorn area", typeof(RectTransform)).AddComponent<UnityEngine.UI.Text>();
            line.transform.SetParent(rect, false); line.font = font; line.fontSize = 15; line.fontStyle = FontStyle.Bold; line.alignment = TextAnchor.MiddleCenter;
            line.color = new(1, .78f, .35f); line.raycastTarget = false; line.rectTransform.sizeDelta = new(236, 22); line.rectTransform.anchoredPosition = new(0, -123);
            var lineEdge = line.gameObject.AddComponent<UnityEngine.UI.Outline>(); lineEdge.effectColor = new(0, 0, 0, .9f); lineEdge.effectDistance = new(1.2f, -1.2f);
            line.enabled = false; map.areaLine = line;
            map.CacheRoutes(); return map;
        }

        protected override void Awake()
        {
            base.Awake(); if (!Application.isPlaying) return;
            toggle = new UnityEngine.InputSystem.InputAction("Minimap on / off (Free Roam)", UnityEngine.InputSystem.InputActionType.Button);
            toggle.AddBinding("<Keyboard>/j"); toggle.AddBinding("<Gamepad>/buttonEast"); toggle.Enable(); if (!Instance) Instance = this;
        }
        protected override void OnDestroy() { toggle?.Dispose(); if (Instance == this) Instance = null; base.OnDestroy(); }
        bool RoamHidden => race && race.Flow && race.Flow.Save != null && race.Flow.Save.Settings.roamMinimapHidden;
        public void SetRoamShown(bool on) { if (!race || !race.Flow || race.Flow.Save == null) return; race.Flow.Save.Settings.roamMinimapHidden = !on; race.Flow.Save.SaveSettings(); }
        void Update()
        {
            if (toggle == null || Split || !race || !race.Flow || race.Flow.Save == null || !race.FreeRoam || !toggle.WasPressedThisFrame()) return;
            var flow = race.Flow;
            if (flow.State != RaceFlow.Stage.Racing || flow.MenuVisible || MenuInput.Blocked || TrailerMode.Active || flow.GetComponent<ExplorationMap>()?.OwnsInput == true) return;
            SetRoamShown(RoamHidden); flow.Notify("Minimap: " + (RoamHidden ? "Off" : "On"), 1.5f);
        }
        // Free Roam: every road centreline in the world scene (paved roads and trails), collected once.
        void CacheRoam()
        {
            roamCached = true;
            foreach (var road in FindObjectsByType<RaceRoad>(FindObjectsSortMode.None))
                if (road.isActiveAndEnabled && road.points != null && road.points.Length > 1) Add(roamSegments, road.points, road.points.Length > 2 && (road.points[0] - road.points[road.points.Length - 1]).sqrMagnitude < 4, road.forestTrail);
        }
        void CacheRoutes()
        {
            Add(segments, race.road.points, true, false);
            foreach (var route in race.Branches ?? System.Array.Empty<WoodlandRoute>())
                if (route && route.isActiveAndEnabled) Add(segments, route.points, false, true);
        }
        static void Add(List<(Vector3 a, Vector3 b, bool shortcut)> list, Vector3[] points, bool closed, bool shortcut)
        {
            if (points == null || points.Length < 2) return;
            var previous = points[0];
            for (int i = 1; i < points.Length; i++)
                if ((points[i] - previous).sqrMagnitude >= 9 || i == points.Length - 1)
                { list.Add((previous, points[i], shortcut)); previous = points[i]; }
            if (closed) list.Add((previous, points[0], shortcut));
        }
        void LateUpdate()
        {
            bool roam = race && race.FreeRoam;
            bool visible = race && Vehicle && race.Flow && !race.Flow.MenuVisible && (!roam || !RoamHidden || Split) && SplitScreen.Active == Split;
            Shown = visible;
            if (roam && !roamCached && visible) CacheRoam();
            // Keep this component active so it can restore its parent after menus.
            canvasRenderer.SetAlpha(visible ? 1 : 0);
            var panel = transform.parent.parent;
            // Narrow windows need vertical separation from the fixed left-hand lap panel.
            var canvasRect = panel.parent as RectTransform;
            bool narrow = canvasRect && canvasRect.rect.width < 680;
            if (!Split) { panel.localScale = Vector3.one * (narrow ? .72f : 1); ((RectTransform)panel).anchoredPosition = new(-18, narrow ? -174 : -18); }
            panel.GetComponent<UnityEngine.UI.Image>().enabled = visible;
            var key = panel.Find("Route key").GetComponent<UnityEngine.UI.Text>(); key.enabled = visible; key.text = roam ? RoamKey : RaceKey;
            if (north) north.enabled = visible;
            if (areaLine && Time.unscaledTime >= nextArea)
            {
                nextArea = Time.unscaledTime + .25f; var collection = roam && visible && !Split ? race.GetComponent<ExplorationCollection>() : null;
                var area = collection ? AcornAreas.At(collection, Vehicle.transform.position) : null;
                areaLine.enabled = area != null && !area.Done(collection); if (areaLine.enabled) areaLine.text = area.name + "  " + area.Count(collection);
            }
            if (areaLine && !visible) areaLine.enabled = false;
            if (!visible || Time.unscaledTime < nextUpdate) return;
            nextUpdate = Time.unscaledTime + .05f;
            origin = Vehicle.transform.position;
            forward = Vector3.ProjectOnPlane(Vehicle.transform.forward, Vector3.up).normalized;
            if (forward.sqrMagnitude < .5f) forward = Vector3.forward;
            right = Vector3.Cross(Vector3.up, forward);
            northDir = new Vector2(right.z, forward.z).normalized; if (northDir.sqrMagnitude < .5f) northDir = Vector2.up;
            if (north) north.rectTransform.anchoredPosition = Rim(northDir);
            SetVerticesDirty();
        }
        public Vector2 Project(Vector3 p)
        {
            var d = p - origin;
            return PlayerPoint + new Vector2(Vector3.Dot(d, right), Vector3.Dot(d, forward)) * Scale;
        }
        protected override void OnPopulateMesh(UnityEngine.UI.VertexHelper vh)
        {
            vh.Clear(); if (!race || !Vehicle) return;
            var bounds = rectTransform.rect; bool roam = race.FreeRoam;
            foreach (var s in roam ? roamSegments : segments)
            {
                var a = Project(s.a); var b = Project(s.b);
                if (Mathf.Max(a.x,b.x)<bounds.xMin-5 || Mathf.Min(a.x,b.x)>bounds.xMax+5 || Mathf.Max(a.y,b.y)<bounds.yMin-5 || Mathf.Min(a.y,b.y)>bounds.yMax+5) continue;
                Line(vh, a, b, s.shortcut ? 3 : 5, roam ? (s.shortcut ? Trail : Road) : s.shortcut ? Optional : Main);
            }
            if (roam)
            {
                // activity sites, then the waypoint (kept at the edge of the window when it is further away)
                var sites = race.Flow.Activities?.Sites;
                if (sites != null) foreach (var site in sites) { if (!site) continue; var p = Project(site.transform.position); if (!bounds.Contains(p)) continue; Diamond(vh, p, 5.5f, new(.03f,.06f,.08f)); Diamond(vh, p, 4, Optional); }
                var map = race.Flow.GetComponent<ExplorationMap>();
                if (map && map.Waypoint.HasValue)
                {
                    var p = Project(map.Waypoint.Value); var inner = new Rect(bounds.xMin + 8, bounds.yMin + 8, bounds.width - 16, bounds.height - 16);
                    if (!inner.Contains(p))
                    {
                        var d = p - PlayerPoint; float t = 1;
                        if (d.x > 0) t = Mathf.Min(t, (inner.xMax - PlayerPoint.x) / d.x); if (d.x < 0) t = Mathf.Min(t, (inner.xMin - PlayerPoint.x) / d.x);
                        if (d.y > 0) t = Mathf.Min(t, (inner.yMax - PlayerPoint.y) / d.y); if (d.y < 0) t = Mathf.Min(t, (inner.yMin - PlayerPoint.y) / d.y);
                        p = PlayerPoint + d * t;
                    }
                    Diamond(vh, p, 8, new(.03f,.06f,.08f)); Diamond(vh, p, 6, Waypoint);
                }
            }
            // 0.95 Part G: Speed Patrol: the clocked speeders (red)
            if (roam && SpeedPatrol.Current) foreach (var m in SpeedPatrol.Current.ClockedMarks) { var p = Project(m); if (!bounds.Contains(p)) continue; Diamond(vh, p, 7, new(.03f,.06f,.08f)); Diamond(vh, p, 5, new Color(1, .2f, .15f)); }
            // 0.94 Part B: split-screen Free Roam: the other player in their colour, kept at the rim when further away
            if (roam && Split && SplitScreen.Race && SplitScreen.Race.P2Car)
            {
                var other = Vehicle == race.vehicle ? SplitScreen.Race.P2Car : race.vehicle; bool p1 = other == race.vehicle;
                var p = Project(other.transform.position); var inner = new Rect(bounds.xMin + 8, bounds.yMin + 8, bounds.width - 16, bounds.height - 16);
                if (!inner.Contains(p))
                {
                    var d = p - PlayerPoint; float t = 1;
                    if (d.x > 0) t = Mathf.Min(t, (inner.xMax - PlayerPoint.x) / d.x); if (d.x < 0) t = Mathf.Min(t, (inner.xMin - PlayerPoint.x) / d.x);
                    if (d.y > 0) t = Mathf.Min(t, (inner.yMax - PlayerPoint.y) / d.y); if (d.y < 0) t = Mathf.Min(t, (inner.yMin - PlayerPoint.y) / d.y);
                    p = PlayerPoint + d * t;
                }
                Diamond(vh, p, 8, new(.03f,.06f,.08f)); Diamond(vh, p, 6, p1 ? new Color(.3f, .95f, .81f) : new Color(1, .74f, .25f));
            }
            if (!roam) foreach (var r in race.Racers)
            {
                if ((Split ? r.Car == Vehicle : !r.IsAi) || !r.Car || r.Dnf) continue;
                var p = Project(r.Car.transform.position);
                if (!bounds.Contains(p)) continue;
                // 0.92 Part F: in split-screen the other player in their colour, the AI rivals small and white
                bool player = Split && (r == race.Racers[0] || (SplitScreen.Race && r == SplitScreen.Race.P2));
                Diamond(vh, p, player ? 7 : 5, new(.03f,.06f,.08f)); Diamond(vh, p, player ? 5 : 3.5f, player ? (r == race.Racers[0] ? new Color(.3f, .95f, .81f) : new Color(1, .74f, .25f)) : Color.white);
            }
            // 0.93 Part D: E, S and W ticks on the rim (the N is a label), at right angles to north
            var c = bounds.center;
            foreach (var d in new[] { new Vector2(northDir.y, -northDir.x), -northDir, new Vector2(-northDir.y, northDir.x) })
            { var rim = Rim(d); var from = c + (rim - c) * 1.0f + d * 4; var to = rim - d * 5; Line(vh, from, to, 5, new(.02f, .04f, .05f)); Line(vh, from, to, 2.5f, Color.white); }
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
