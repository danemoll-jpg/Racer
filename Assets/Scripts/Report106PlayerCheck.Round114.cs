using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

namespace Racer
{
    // 0.114 evidence only (the 0.106 check component, never active without -p106check and -racerTestSave)
    public sealed partial class Report106PlayerCheck
    {
        // ---------------------------------------------------------------- Part A: the generated buildings from outside
        //   -p106kind shots114a  Dan's three viewpoints (his report be9da1: position, heading, a driver's eye on the Drifter Twin), driver's
        //                        views along Trickum Rd both ways (0.113's views) and along Hwy 92 both ways, a top-down and a low orbit of a
        //                        Hwy 92 strip center, a Trickum Rd house and the Trickum church; then each of Dan's spots driven into at 10 m/s
        static readonly (string name, Vector3 at, float heading)[] DanViews = {
            ("BUG-001", new Vector3(-645.75f, 9.29f, 350.72f), 255.0f), ("BUG-002", new Vector3(-676.40f, 0, 369.61f), 292.0f), ("BUG-003", new Vector3(-648.69f, 9.16f, 275.77f), 257.1f) };
        static readonly (string name, Vector2 at)[] Orbits = { ("strip-Cleaners-hwy92", new Vector2(-655.2f, 443.2f)), ("house-trickum", new Vector2(-667.1f, 282.5f)), ("church-trickum", new Vector2(-507.6f, -597.8f)) };
        IEnumerator Shots114a()
        {
            yield return ToFreeRoam("drifter"); var car = race.vehicle;
            var plan = FindObjectsByType<Hwy92Roadside>(FindObjectsSortMode.None).FirstOrDefault(r => r.resource == Hwy92Roadside.Trickum);
            Note($"Trickum plan: {(plan ? string.Join("; ", plan.Report) : "no component")}");
            foreach (var c in FindObjectsByType<Canvas>(FindObjectsSortMode.None)) c.enabled = false;
            car.Body.isKinematic = true; car.transform.position += Vector3.down * 200; yield return new WaitForSeconds(6); // the night-window glow fades out after the clock is set
            foreach (var (name, at, heading) in DanViews)
            {
                var f = Quaternion.Euler(0, heading, 0) * Vector3.forward; var eye = new Vector3(at.x, TopAt(new Vector3(at.x, 40, at.z)) + 1.5f, at.z);
                yield return Free($"A-{name}-after", eye, eye + f * 40 + Vector3.up * 2.5f, 62, 1920, 1080);
            }
            foreach (var (name, at, f) in TrickumViews)
            {
                var eye = new Vector3(at.x, TopAt(new Vector3(at.x, 40, at.y)) + 1.4f, at.y);
                yield return Free($"A-{name}-after", eye, eye + new Vector3(f.x, 0, f.y) * 40 + Vector3.down * 1.2f, 62, 1600, 900);
            }
            foreach (var (name, at, f) in Hwy92Views())
            {
                var eye = new Vector3(at.x, TopAt(new Vector3(at.x, 40, at.y)) + 1.4f, at.y);
                yield return Free($"A-{name}-after", eye, eye + new Vector3(f.x, 0, f.y) * 40 + Vector3.down * 1.2f, 62, 1600, 900);
            }
            foreach (var (name, at) in Orbits)
            {
                float g = TopAt(new Vector3(at.x, 40, at.y));
                yield return Free($"A-top-{name}", new Vector3(at.x, g + 120, at.y), new Vector3(at.x, g, at.y), 60, 1400, 1400, true, 28);
                for (int k = 0; k < 4; k++) // a low orbit: four sides, 30 m out, 4 m up
                {
                    var d = Quaternion.Euler(0, 45 + 90 * k, 0) * Vector3.forward; var centre = new Vector3(at.x, g + 3, at.y);
                    yield return Free($"A-orbit-{name}-{k * 90 + 45}", centre + d * 30 + Vector3.up * 1, centre, 60, 1600, 900);
                }
            }
            foreach (var c in FindObjectsByType<Canvas>(FindObjectsSortMode.None)) c.enabled = true;
            // drive in: from each of Dan's spots toward the building at 10 m/s; the car must stop at the wall (not end up inside)
            car.Body.isKinematic = false;
            foreach (var (name, at, heading) in DanViews)
            {
                Put(car, at, heading, 10); var start = car.Body.position; float t0 = Time.time, closest = 0; string hit = "nothing";
                var contacts = car.gameObject.GetComponent<Report106Contacts>() ?? car.gameObject.AddComponent<Report106Contacts>(); contacts.who = name; contacts.list.Clear();
                while (Time.time - t0 < 6) { Drive(car, car.Body.position + Quaternion.Euler(0, heading, 0) * Vector3.forward * 20, 10); closest = Mathf.Max(closest, Vector3.Distance(Flat(car.Body.position), Flat(start))); yield return new WaitForFixedUpdate(); }
                Release(); if (contacts.list.Count > 0) hit = contacts.list[0];
                bool inside = InsideFootprint(car.Body.position);
                Note($"{name} drive-in at 10 m/s, heading {heading:F0}: went {closest:F1} m, first contact: {hit}; ended at {V(car.Body.position)}, inside a building footprint: {(inside ? "YES" : "no")}");
            }
        }
        // (0.114 Part B: drive114 and shots114b were removed with the proposal markers in 0.115)
        static int Ahead(float[] s, int i, float d) { float want = s[i] + d; while (i < s.Length - 1 && s[i] < want) i++; return i; }
        static int NearestIndex(Vector3[] pts, float x, float z, out float d2) { int k = 0; d2 = float.MaxValue; for (int i = 0; i < pts.Length; i++) { float d = (pts[i].x - x) * (pts[i].x - x) + (pts[i].z - z) * (pts[i].z - z); if (d < d2) { d2 = d; k = i; } } return k; }
        static (string name, Vector2 at, Vector2 f)[] Hwy92Views() => new (string, Vector2, Vector2)[] { // the CR113 highway centreline, 3.5 m into each lane
            ("Hm550e", new Vector2(-550.1f, 536.9f), new Vector2(1.000f, -0.030f)), ("Hm550w", new Vector2(-549.9f, 543.9f), new Vector2(-1.000f, 0.030f)), ("Hm400e", new Vector2(-400.4f, 530.8f), new Vector2(0.999f, -0.035f)), ("Hm400w", new Vector2(-400.2f, 537.8f), new Vector2(-0.999f, 0.035f)), ("Hm250e", new Vector2(-250.5f, 526.3f), new Vector2(1.000f, -0.015f)), ("Hm250w", new Vector2(-250.3f, 533.3f), new Vector2(-1.000f, 0.015f)), ("Hm100e", new Vector2(-100.4f, 525.6f), new Vector2(1.000f, 0.000f)), ("Hm100w", new Vector2(-100.4f, 532.6f), new Vector2(-1.000f, -0.000f)), ("H50e", new Vector2(50.2f, 530.6f), new Vector2(0.955f, 0.295f)), ("H50w", new Vector2(48.2f, 537.3f), new Vector2(-0.955f, -0.295f)), ("H200e", new Vector2(199.8f, 548.5f), new Vector2(0.997f, 0.080f)), ("H200w", new Vector2(199.2f, 555.4f), new Vector2(-0.997f, -0.080f)) };
        static bool InsideFootprint(Vector3 p)
        {
            foreach (var h in Physics.RaycastAll(new Vector3(p.x, p.y + 40, p.z), Vector3.down, 80, ~0, QueryTriggerInteraction.Ignore))
                if (h.collider is MeshCollider && h.collider.transform.parent && h.collider.transform.parent.name.EndsWith("solids") && h.point.y > p.y) return true; // a building's roof right above the car
            return false;
        }
    }
}
