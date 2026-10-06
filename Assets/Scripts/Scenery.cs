using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Racer
{
    // 0.78 Part A: the world scenery upgrade. A VISUAL replacement only: nothing here adds, moves or changes a collider,
    // road, route or scene file. When "Scenery: New" is on (Settings > Display, default New, saved), SceneryWorld hides
    // the old scenery's renderers in the loaded scene (forceRenderingOff, so their colliders and everything else stay as
    // they are) and draws the new kit fitted to them; "Classic" shows the old renderers again and draws nothing new.
    public static class Scenery
    {
        static bool on = true;
        public static bool New => on;
        public static event Action Changed;
        public static void Set(bool value)
        {
            Shader.SetGlobalFloat("_RacerScenery", value ? 1 : 0);
            if (on == value) return;
            on = value; Changed?.Invoke();
        }
    }

    // One per loaded world scene (RaceFlow attaches it). Builds the new scenery the first time New is shown.
    public sealed class SceneryWorld : MonoBehaviour
    {
        public static SceneryWorld Current { get; private set; }
        public SceneryTrees Trees { get; private set; }
        public SceneryBuildings Buildings { get; private set; }
        public SceneryProps Props { get; private set; }
        public SceneryWater Water { get; private set; }
        public SceneryGround Ground { get; private set; }
        public SceneryPaving Paving { get; private set; }
        public StreetSigns Signs { get; private set; }
        public float BuildMilliseconds { get; private set; }
        public string Timings { get; private set; } = "";
        public readonly List<Renderer> Hidden = new();
        bool built;

        public static SceneryWorld Attach(GameObject host)
        {
            var w = host.GetComponent<SceneryWorld>() ?? host.AddComponent<SceneryWorld>();
            return w;
        }
        void OnEnable() { Current = this; Scenery.Changed += Apply; }
        void OnDisable() { Scenery.Changed -= Apply; if (Current == this) Current = null; }
        public JunctionPaint Paint { get; private set; }
        void Start()
        {
            SceneryTrees.ClearFreeRoamTrunks(gameObject.scene); RoadPosts.Clear(gameObject.scene); RoadPosts.RetireNameBoards(gameObject.scene); MountainDirt.Apply(gameObject.scene); // 0.80: before the new kit is fitted
            Signs = StreetSigns.Attach(gameObject); Paint = JunctionPaint.Attach(gameObject); WorldEdge.Attach(gameObject); ScenePeople.DressCamp(gameObject.scene); Apply();
        }

        void Build()
        {
            built = true; var watch = System.Diagnostics.Stopwatch.StartNew();
            var lap = System.Diagnostics.Stopwatch.StartNew(); string Lap() { var t = lap.ElapsedMilliseconds; lap.Restart(); return t + " ms"; }
            Trees = gameObject.AddComponent<SceneryTrees>(); Trees.Build(gameObject.scene, Hidden); string trees = Lap();
            var town = new GameObject("New scenery (0.78)"); SceneManager.MoveGameObjectToScene(town, gameObject.scene);
            Buildings = town.AddComponent<SceneryBuildings>(); Buildings.Build(gameObject.scene, Hidden); string buildings = Lap();
            Props = town.AddComponent<SceneryProps>(); Props.Build(gameObject.scene, new HashSet<Renderer>(Hidden)); string props = Lap();
            Water = town.AddComponent<SceneryWater>(); Water.Build(gameObject.scene); string water = Lap();
            Paving = town.AddComponent<SceneryPaving>(); Paving.Build(gameObject.scene); string paving = Lap();
            Timings = $"trees {trees}, buildings {buildings}, rocks and props {props}, shores {water}, paving {paving}";
            Ground = town.AddComponent<SceneryGround>(); Ground.Trees = Trees;
            BuildMilliseconds = (float)watch.Elapsed.TotalMilliseconds;
            Debug.Log($"Scenery: built in {BuildMilliseconds:F0} ms ({Timings}); {Trees.Summary}; buildings redesigned {Buildings.Redesigned}, detailed {Buildings.Detailed}; rocks {Props.Rocks}, bevelled props {Props.Bevelled}; shores {Water.Shores} with {Water.Reeds} reed clumps; {Hidden.Count} old renderers replaced");
        }

        public void Apply()
        {
            if (Scenery.New && !built) Build();
            foreach (var r in Hidden) if (r) r.forceRenderingOff = Scenery.New;
            if (Trees) Trees.enabled = Scenery.New;
            if (Buildings) Buildings.SetShown(Scenery.New);
            if (Props) Props.SetShown(Scenery.New);
            if (Water) Water.SetShown(Scenery.New);
            if (Paving) Paving.SetShown(Scenery.New);
            if (Ground) Ground.enabled = Scenery.New;
        }
    }
}
