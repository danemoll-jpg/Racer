using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Racer
{
    // 0.93 Part C: the four acorn areas (ExplorationCollection.Site.approach) as places on the maps. An area's acorns can be far
    // apart (Western gullies spans the whole west), so an area is drawn as one or more rounded regions: acorns within Join
    // metres of each other share one, the convex hull of its acorns grown by Margin all round; a lone acorn gets a wider
    // circle whose centre is moved off the acorn, so no region's middle gives a spot away. Directions are plain words from
    // places a player knows (the three roads, Dan's and Kyle's houses, the lake, the mountain); north is world +Z, as on
    // every map in the game.
    public static class AcornAreas
    {
        public const float Margin = 60, Join = 150, Lone = 85, LoneShift = 40;
        public sealed class Region { public Vector3[] outline; public Vector3 label; public Bounds bounds; public ExplorationCollection.Site[] sites; public bool named; }
        public sealed class Area
        {
            public string name, direction; public Color tint; public int labelSide; public ExplorationCollection.Site[] sites; public Region[] regions; public Bounds bounds;
            public int Found(ExplorationCollection c) => sites.Count(s => c.Discovered(s.id));
            public int Total => sites.Length;
            public string Count(ExplorationCollection c) => $"{Found(c)} / {Total}";
            public bool Done(ExplorationCollection c) => Found(c) >= Total;
            public bool Contains(Vector3 p) => regions.Any(r => Inside(r.outline, p));
        }
        // the outline is convex and counter-clockwise (seen from above, +X right, +Z up)
        static bool Inside(Vector3[] o, Vector3 p)
        {
            for (int i = 0; i < o.Length; i++)
            {
                var a = o[i]; var b = o[(i + 1) % o.Length];
                if ((b.x - a.x) * (p.z - a.z) - (b.z - a.z) * (p.x - a.x) < 0) return false;
            }
            return true;
        }

        // where each area is, in words, from where its acorns really are (Docs/Report093/Lists/C-areas.txt)
        static readonly Dictionary<string, string> Directions = new()
        {
            { "Neighborhood woodland", "Around Dan's and Kyle's houses, and west of S Cherokee Ln up toward Hwy 92" },
            { "Western gullies", "West of S Cherokee Ln, spread along Hwy 92 and Trickum Rd" },
            { "Creek pockets", "East of the lake, and two higher up the mountain" },
            { "Ridge woodland", "North-east of the lake, up the mountain toward the summit" },
        };
        static readonly Dictionary<string, Color> Tints = new()
        {
            { "Neighborhood woodland", new Color(.55f, .9f, .35f) },
            { "Western gullies", new Color(1f, .6f, .3f) },
            { "Creek pockets", new Color(.35f, .75f, 1f) },
            { "Ridge woodland", new Color(.9f, .55f, 1f) },
        };
        // the places the directions refer to, labelled on the world map; side: -1 written to the left of the point, 1 to
        // its right, 0 centred on it
        public static readonly (string label, Vector3 at, int side)[] Places =
        {
            ("S Cherokee Ln", new Vector3(357, 0, 300), 1), ("Hwy 92", new Vector3(-150, 0, 560), 0), ("Trickum Rd", new Vector3(-623, 0, -250), 1),
            ("Dan's house", new Vector3(434, 0, -55), -1), ("Kyle's house", new Vector3(505, 0, -70), 1),
            ("The lake", new Vector3(615, 0, -20), 0), ("Mountain summit", new Vector3(1230, 0, 200), 0),
        };

        static ExplorationCollection built; static Area[] areas = new Area[0];
        public static Area[] Of(ExplorationCollection c)
        {
            if (!c) return new Area[0];
            if (built == c && areas.Length > 0) return areas;
            built = c;
            areas = c.sites.GroupBy(s => s.approach).Select(g => Build(g.Key, g.ToArray())).ToArray();
            return areas;
        }
        public static Area At(ExplorationCollection c, Vector3 p) => Of(c).FirstOrDefault(a => a.Contains(p));

        static Area Build(string name, ExplorationCollection.Site[] sites)
        {
            // groups: acorns within Join of another acorn of the group (single linkage)
            var groups = new List<List<ExplorationCollection.Site>>();
            foreach (var s in sites)
            {
                var near = groups.Where(g => g.Any(o => Flat(o.position - s.position) < Join)).ToList();
                var merged = new List<ExplorationCollection.Site> { s }; foreach (var g in near) { merged.AddRange(g); groups.Remove(g); }
                groups.Add(merged);
            }
            var regions = groups.Select(MakeRegion).ToArray();
            // one name per area (several labels crowd the map at its default zoom): on the region with the most acorns, of
            // those the one furthest from the place names; the others are known by their colour
            regions.OrderByDescending(r => r.sites.Length).ThenByDescending(r => Places.Min(p => Flat(p.at - r.label))).First().named = true;
            var bounds = regions[0].bounds; foreach (var r in regions) bounds.Encapsulate(r.bounds);
            return new Area { name = name, sites = sites, regions = regions, bounds = bounds, direction = Directions.TryGetValue(name, out var d) ? d : "", tint = Tints.TryGetValue(name, out var t) ? t : new Color(.9f, .9f, .6f), labelSide = name == "Neighborhood woodland" ? -1 : name == "Ridge woodland" ? 1 : 0 };
        }
        static float Flat(Vector3 v) => new Vector2(v.x, v.z).magnitude;
        static Region MakeRegion(List<ExplorationCollection.Site> group)
        {
            var pts = group.Select(s => new Vector2(s.position.x, s.position.z)).ToList(); float grow = Margin;
            if (pts.Count == 1)
            {
                // a lone acorn: a wider circle, its centre moved LoneShift off the acorn in a direction fixed by the acorn's id
                int h = 17; foreach (char ch in group[0].id) h = h * 31 + ch; float a = ((h & 0x7fffffff) % 360) * Mathf.Deg2Rad; pts[0] += new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * LoneShift; grow = Lone;
            }
            // the boundary of (hull + a disc of radius grow): for each outward direction u, the point furthest along u, plus grow * u
            var outline = new List<Vector3>(); const int steps = 72;
            for (int k = 0; k < steps; k++)
            {
                float a = k * Mathf.PI * 2 / steps; var u = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
                var far = pts.OrderByDescending(p => Vector2.Dot(p, u)).First() + u * grow; outline.Add(new Vector3(far.x, 0, far.y));
            }
            var bounds = new Bounds(outline[0], Vector3.zero); foreach (var p in outline) bounds.Encapsulate(p);
            // the name goes in the upper part of the region (its middle is often where other labels are)
            var middle = pts.Aggregate(Vector2.zero, (x, y) => x + y) / pts.Count;
            var label = new Vector3(middle.x, 0, Mathf.Lerp(middle.y, bounds.max.z, .55f));
            return new Region { outline = outline.ToArray(), label = label, bounds = bounds, sites = group.ToArray() };
        }
    }
}
