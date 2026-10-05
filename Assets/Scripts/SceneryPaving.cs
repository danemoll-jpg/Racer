using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Racer
{
    // 0.79 Part F: one asphalt. The terrain's painted roads are drawn by the ground shader in the road grey; the paved
    // pieces that are separate meshes (Hwy 92's continuous four-lane highway, decorative road continuations, roadworks,
    // Reverse asphalt) used URP Lit materials of their own, which light differently: darker and bluer, with a hard seam
    // where they meet the terrain's road, and more so at night and in rain (no wet darkening, no road detail). The black
    // driveway was nearly black. With Scenery: New their renderers use Racer/Paved, the ground shader's road lighting in
    // the terrain road's own colour (driveways a shade lighter, same family). Classic swaps the old materials back.
    // Same renderers, same meshes and colliders; lane lines and edges are separate meshes and unchanged. The colour is
    // passed raw (a vector, not a colour property, which a linear-space project would convert) to match the vertex colours.
    public sealed class SceneryPaving : MonoBehaviour
    {
        public static readonly Color Road = new(.24f, .25f, .26f), Driveway = new(.27f, .275f, .29f);
        static Material road, driveway;
        readonly List<(Renderer r, Material[] classic, Material[] paved)> swaps = new();
        public readonly List<string> Names = new();
        public int Count => swaps.Count;
        public static bool IsAsphalt(Material m) => m && m.name.IndexOf("asphalt", System.StringComparison.OrdinalIgnoreCase) >= 0;
        static bool IsDriveway(Renderer r, Material m) => m.name.IndexOf("black asphalt", System.StringComparison.OrdinalIgnoreCase) >= 0
            || (r.name + "/" + (r.transform.parent ? r.transform.parent.name : "")).IndexOf("driveway", System.StringComparison.OrdinalIgnoreCase) >= 0;

        public void Build(Scene scene)
        {
            if (!road)
            {
                var source = Resources.Load<Material>("Scenery/Paved"); if (!source) { Debug.LogWarning("Scenery: paved material missing"); return; }
                road = new Material(source) { name = "Paved road (0.79)" }; road.SetVector("_Asphalt", Road);
                driveway = new Material(source) { name = "Paved driveway (0.79)" }; driveway.SetVector("_Asphalt", Driveway);
            }
            foreach (var r in scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<MeshRenderer>(true)))
            {
                var mats = r.sharedMaterials; if (!mats.Any(IsAsphalt)) continue;
                var paved = mats.Select(m => IsAsphalt(m) ? (IsDriveway(r, m) ? driveway : road) : m).ToArray();
                swaps.Add((r, mats, paved)); Names.Add(r.name + " (" + string.Join(", ", mats.Where(IsAsphalt).Select(m => m.name).Distinct()) + ")");
            }
        }
        public void SetShown(bool on) { foreach (var (r, classic, paved) in swaps) if (r) r.sharedMaterials = on ? paved : classic; }
    }
}
