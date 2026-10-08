using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Racer
{
    // 0.96 Part E: the road network the AI cops drive. Free Roam has a handful of drivable centre lines (RaceRoad: the street
    // loop with Hwy 92 beside it, the forest and mountain trails, the lake connection, the driveways). The network samples each
    // every 10 m into nodes, joins consecutive nodes, and joins nodes of different roads (or far apart on one road) that meet
    // within a few metres: that is where a cop may turn from one road onto another. A* finds the way; "on the network" means
    // within a road's width of one of its nodes. Cops never leave it (CopDriver follows the paths); the runner can.
    public sealed class RoadNet
    {
        public const float Step = 10, JoinDistance = 9, EndJoinDistance = 24, OnRoadDistance = 15;
        public struct Edge { public int to; public float cost; }
        public readonly List<Vector3> P = new();
        public readonly List<int> Road = new();            // index into Roads
        public readonly List<float> S = new();             // station on that road
        public readonly List<float> Half = new();          // half width there
        public readonly List<List<Edge>> Adj = new();
        public readonly List<RaceRoad> Roads = new();
        public readonly List<string> RoadNames = new();
        public int[] Component;
        readonly Dictionary<long, List<int>> grid = new(); const float Cell = 24;
        public int Count => P.Count;
        static long Key(int x, int z) => ((long)x << 32) ^ (uint)z;

        public static RoadNet Build(RaceDirector race)
        {
            var net = new RoadNet();
            var roads = Object.FindObjectsByType<RaceRoad>(FindObjectsSortMode.None).Where(r => r && r.isActiveAndEnabled && r.points != null && r.points.Length > 3).OrderBy(r => r.name).ToList();
            var first = new List<int>(); var last = new List<int>();
            foreach (var road in roads)
            {
                road.Initialize(); if (road.Length < 40) continue;
                int ri = net.Roads.Count; net.Roads.Add(road); net.RoadNames.Add(road.name);
                int start = net.P.Count; int n = Mathf.Max(2, Mathf.RoundToInt(road.Length / Step)); float step = road.Length / n;
                bool closed = !road.openHighway;
                int count = closed ? n : n + 1;
                for (int i = 0; i < count; i++)
                {
                    float s = Mathf.Min(i * step, road.Length - .01f); var p = road.At(s, out _);
                    net.P.Add(p); net.Road.Add(ri); net.S.Add(s); net.Half.Add(road.HalfWidth(s)); net.Adj.Add(new List<Edge>());
                }
                for (int i = 0; i < count - 1; i++) net.Link(start + i, start + i + 1, 1f);
                if (closed) net.Link(start + count - 1, start, 1f);
                first.Add(start); last.Add(start + count - 1);
            }
            net.BuildGrid();
            // joins: nodes of different roads (or well apart on one road) within JoinDistance; road ends within EndJoinDistance
            for (int i = 0; i < net.Count; i++)
            {
                foreach (int j in net.Near(net.P[i], JoinDistance))
                {
                    if (j <= i) continue; if (Mathf.Abs(net.P[i].y - net.P[j].y) > 5) continue;
                    if (net.Road[i] == net.Road[j] && Mathf.Abs(net.S[i] - net.S[j]) < 100) continue;
                    net.Link(i, j, 5f); // 0.97: a hop between roads costs about 45 m, so the cops do not zig-zag between parallel roads (Hwy 92 beside the loop)
                }
            }
            for (int r = 0; r < roads.Count && r < net.Roads.Count; r++)
            {
                if (!net.Roads[r].openHighway) continue;
                foreach (int end in new[] { first[r], last[r] })
                {
                    int best = -1; float bd = EndJoinDistance;
                    foreach (int j in net.Near(net.P[end], EndJoinDistance)) { if (net.Road[j] == r) continue; float d = Vector3.Distance(net.P[end], net.P[j]); if (d < bd && Mathf.Abs(net.P[end].y - net.P[j].y) < 7) { bd = d; best = j; } }
                    if (best >= 0) net.Link(end, best, 2f);
                }
            }
            net.Components();
            return net;
        }
        void Link(int a, int b, float scale)
        {
            float d = Vector3.Distance(P[a], P[b]) * scale * (Roads[Road[a]].forestTrail || Roads[Road[b]].forestTrail ? 1.35f : 1f);
            if (!Adj[a].Any(e => e.to == b)) Adj[a].Add(new Edge { to = b, cost = d });
            if (!Adj[b].Any(e => e.to == a)) Adj[b].Add(new Edge { to = a, cost = d });
        }
        void BuildGrid()
        {
            grid.Clear();
            for (int i = 0; i < P.Count; i++) { long k = Key(Mathf.FloorToInt(P[i].x / Cell), Mathf.FloorToInt(P[i].z / Cell)); if (!grid.TryGetValue(k, out var l)) grid[k] = l = new List<int>(); l.Add(i); }
        }
        public IEnumerable<int> Near(Vector3 p, float radius)
        {
            int cx = Mathf.FloorToInt(p.x / Cell), cz = Mathf.FloorToInt(p.z / Cell), r = Mathf.CeilToInt(radius / Cell);
            float r2 = radius * radius;
            for (int x = cx - r; x <= cx + r; x++)
                for (int z = cz - r; z <= cz + r; z++)
                    if (grid.TryGetValue(Key(x, z), out var l))
                        foreach (int i in l) { var d = P[i] - p; if (d.x * d.x + d.z * d.z <= r2) yield return i; }
        }
        // the nearest node (horizontal distance, within maxVertical of p.y); -1 when none within search
        public int Nearest(Vector3 p, out float distance, float search = 400, float maxVertical = 9)
        {
            int best = -1; distance = float.MaxValue;
            for (float r = 30; r <= search + 1; r *= 2)
            {
                foreach (int i in Near(p, r)) { var d = P[i] - p; float h = Mathf.Sqrt(d.x * d.x + d.z * d.z); if (Mathf.Abs(d.y) > maxVertical) continue; if (h < distance) { distance = h; best = i; } }
                if (best >= 0 && distance <= r) break;
            }
            return best;
        }
        // is the point on a road: within the road's width (plus a verge) of the nearest node's centre line
        public bool OnNet(Vector3 p, out int node, float extra = 3f)
        {
            node = Nearest(p, out float d, 60); if (node < 0) return false;
            // nodes are 10 m apart, so up to 5 m of the distance can be along the road
            return d <= Half[node] + extra + 5f;
        }
        // distance from the point to the nearest road (the horizontal distance to the nearest node, less the node spacing allowance)
        public float OffNet(Vector3 p) { int n = Nearest(p, out float d, 300); return n < 0 ? 999 : Mathf.Max(0, d - 4f); }
        public Vector3 Tangent(int node) { Roads[Road[node]].At(S[node], out var f); f.y = 0; return f.sqrMagnitude < .0001f ? Vector3.forward : f.normalized; }

        // ---------- A* ----------
        readonly List<float> g = new(); readonly List<int> came = new(); readonly List<byte> state = new();
        public List<int> Path(int from, int to, float maxCost = 20000)
        {
            if (from < 0 || to < 0 || from >= Count || to >= Count) return null;
            if (from == to) return new List<int> { from };
            if (Component[from] != Component[to]) return null;
            int n = Count; g.Clear(); came.Clear(); state.Clear();
            for (int i = 0; i < n; i++) { g.Add(float.MaxValue); came.Add(-1); state.Add(0); }
            g[from] = 0; state[from] = 1; var goal = P[to];
            var heap = new List<(float f, int node)>(); heap.Add((Vector3.Distance(P[from], goal), from));
            while (heap.Count > 0)
            {
                int bi = 0; float bf = heap[0].f; for (int i = 1; i < heap.Count; i++) if (heap[i].f < bf) { bf = heap[i].f; bi = i; }
                var cur = heap[bi].node; heap[bi] = heap[heap.Count - 1]; heap.RemoveAt(heap.Count - 1);
                if (state[cur] == 2) continue; state[cur] = 2;
                if (cur == to) break;
                foreach (var e in Adj[cur])
                {
                    if (state[e.to] == 2) continue; float ng = g[cur] + e.cost; if (ng >= g[e.to] || ng > maxCost) continue;
                    g[e.to] = ng; came[e.to] = cur; heap.Add((ng + Vector3.Distance(P[e.to], goal), e.to));
                }
            }
            if (came[to] < 0) return null;
            var path = new List<int>(); for (int c = to; c >= 0; c = came[c]) { path.Add(c); if (c == from) break; }
            path.Reverse(); return path;
        }
        public float PathLength(List<int> path) { float l = 0; for (int i = 1; i < path.Count; i++) l += Vector3.Distance(P[path[i - 1]], P[path[i]]); return l; }
        void Components()
        {
            Component = new int[Count]; for (int i = 0; i < Count; i++) Component[i] = -1; int c = 0; var stack = new Stack<int>();
            for (int i = 0; i < Count; i++)
            {
                if (Component[i] >= 0) continue; Component[i] = c; stack.Push(i);
                while (stack.Count > 0) { int a = stack.Pop(); foreach (var e in Adj[a]) if (Component[e.to] < 0) { Component[e.to] = c; stack.Push(e.to); } }
                c++;
            }
            ComponentCount = c;
        }
        public int ComponentCount { get; private set; }
        public int LargestComponent => Component == null || Count == 0 ? -1 : Component.GroupBy(x => x).OrderByDescending(x => x.Count()).First().Key;
        // nodes reachable ahead of a node along a direction (the straightest continuation at each junction) for `metres`
        public int Ahead(int node, Vector3 heading, float metres)
        {
            int cur = node; float walked = 0; Vector3 dir = heading; dir.y = 0; if (dir.sqrMagnitude < .01f) dir = Tangent(node); dir.Normalize(); int prev = -1;
            while (walked < metres)
            {
                int best = -1; float bestDot = -2;
                foreach (var e in Adj[cur]) { if (e.to == prev) continue; var d = P[e.to] - P[cur]; d.y = 0; if (d.sqrMagnitude < .01f) continue; float dot = Vector3.Dot(d.normalized, dir); if (dot > bestDot) { bestDot = dot; best = e.to; } }
                if (best < 0 || bestDot < -.2f) break;
                var step = P[best] - P[cur]; step.y = 0; walked += step.magnitude; dir = Vector3.Lerp(dir, step.normalized, .6f).normalized; prev = cur; cur = best;
            }
            return cur;
        }
        // nodes at a station-ish distance away within a graph radius (for searching around a point): every node within `radius` metres of the
        // node by straight line that is reachable, thinned to one per `spacing`
        public List<int> Around(int node, float radius, float spacing)
        {
            var list = new List<int>(); foreach (int i in Near(P[node], radius)) { if (Component[i] != Component[node]) continue; if (list.All(j => Vector3.Distance(P[i], P[j]) >= spacing)) list.Add(i); }
            return list;
        }
        // 0.96: what the radio calls a road
        public string RoadName(int node)
        {
            if (node < 0) return ""; string n = RoadNames[Road[node]].ToLowerInvariant();
            if (n.Contains("highway")) return "Hwy 92"; if (n.Contains("phase 3")) return "the Street Loop"; if (n.Contains("summit")) return "the Summit road"; if (n.Contains("mountain")) return "the mountain trails";
            if (n.Contains("lake")) return "the lake road"; if (n.Contains("driveway")) return "a driveway"; if (n.Contains("navigation")) return "the forest trails"; return "the back roads";
        }
        // the nearest named place (a map destination) within 450 m, else the road
        public string PlaceName(RaceDirector race, Vector3 p)
        {
            var map = race && race.Flow ? race.Flow.GetComponent<ExplorationMap>() : null; string best = null; float bd = 450;
            if (map != null && map.destinations != null) foreach (var d in map.destinations) { float dist = Vector3.Distance(d.position, p); if (dist < bd) { bd = dist; best = d.title; } }
            if (best != null) return best; int n = Nearest(p, out _, 300); return n >= 0 ? RoadName(n) : "the hills";
        }
    }
}
