using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Racer
{
    // 0.111 Part B (Dan: Fence Line Smash "said I smashed nothing ... nothing was working"): a smash site's area is its own props list (smash-01:
    // the 14 fence panels and rails either side of the site; the same fence carries on past them, unlisted). While a smash attempt runs (a campaign
    // Smash event or Free Roam's timed attempt) every prop of the area still standing carries a small orange marker over it, so what counts can
    // be seen and the fence beyond is clearly not part of it. The HUD line says the count, what the area holds and the next medal.
    public sealed partial class ArcadeActivities
    {
        // the props in the selected smash area (distinct, present)
        public int SmashArea => Selected && Selected.kind == ActivitySite.Kind.Smash && Selected.props != null ? Selected.props.Where(p => p).Distinct().Count() : 0;
        // the moment of the last counted smash (the event panel's "+1" pop)
        public float SmashPop { get; private set; } = -10;
        // Free Roam's attempt line: "7 of 14 smashed  ·  3 more for gold"
        string SmashLine { get { if (!Selected || !car) return SmashCount + " smashed"; Selected.Targets(configuration.profileId, out float b, out float s, out float g); return $"{SmashCount} of {SmashArea} smashed  ·  {NextMedal(SmashCount, b, s, g)}"; } }
        public static string NextMedal(int count, float bronze, float silver, float gold)
        {
            if (count >= gold) return "GOLD: keep going";
            var (target, name) = count < bronze ? (bronze, "bronze") : count < silver ? (silver, "silver") : (gold, "gold");
            int more = Mathf.Max(1, Mathf.CeilToInt(target) - count); return $"{more} more for {name}";
        }
        readonly Dictionary<BreakableProp, Transform> markers = new(); static Material markerMaterial; static Mesh markerMesh;
        void LateUpdate()
        {
            bool on = AttemptActive && Selected && Selected.kind == ActivitySite.Kind.Smash && Selected.props != null && race && race.Flow && race.Flow.State == RaceFlow.Stage.Racing;
            if (!on) { if (markers.Count > 0) ClearMarkers(); return; }
            foreach (var prop in Selected.props)
            {
                if (!prop) continue; bool standing = !smashed.Contains(prop) && !prop.IsBroken;
                markers.TryGetValue(prop, out var m);
                if (!standing) { if (m) m.gameObject.SetActive(false); continue; }
                if (!m) markers[prop] = m = Marker(prop);
                m.gameObject.SetActive(true);
                m.localRotation = Quaternion.Euler(0, Time.time * 90 % 360, 45); m.localPosition = Vector3.up * Mathf.Sin(Time.time * 3 + prop.transform.position.x) * .08f;
            }
        }
        Transform Marker(BreakableProp prop)
        {
            if (!markerMaterial) { markerMaterial = new Material(Shader.Find("Universal Render Pipeline/Unlit")) { name = "Smash target marker" }; markerMaterial.SetColor("_BaseColor", new Color(1f, .45f, .05f)); }
            if (!markerMesh) { var cube = GameObject.CreatePrimitive(PrimitiveType.Cube); markerMesh = cube.GetComponent<MeshFilter>().sharedMesh; Destroy(cube); }
            float top = prop.transform.position.y + 1; foreach (var r in prop.GetComponentsInChildren<Renderer>()) top = Mathf.Max(top, r.bounds.max.y);
            var anchor = new GameObject("Smash marker").transform; anchor.position = new Vector3(prop.transform.position.x, top + .55f, prop.transform.position.z);
            var shape = new GameObject("Diamond"); shape.transform.SetParent(anchor, false); shape.transform.localScale = Vector3.one * .26f;
            shape.AddComponent<MeshFilter>().sharedMesh = markerMesh; var mr = shape.AddComponent<MeshRenderer>(); mr.sharedMaterial = markerMaterial; mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            return shape.transform;
        }
        void ClearMarkers() { foreach (var m in markers.Values) if (m) Destroy(m.parent ? m.parent.gameObject : m.gameObject); markers.Clear(); }
        void OnDisable() => ClearMarkers();
    }
}
