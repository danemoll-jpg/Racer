using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Racer.Editor
{
    /// <summary>Explicit authoring tool; never regenerates content on load or in a player.</summary>
    public static class PrototypeBuilder
    {
        [MenuItem("Racer/Build Phase 1 Prototype (empty scene only)")]
        public static void Build()
        {
            var scene = SceneManager.GetActiveScene();
            if (scene.path != "Assets/Scenes/PrototypeTrack.unity" || scene.rootCount != 0)
                throw new System.InvalidOperationException("Builder requires the empty PrototypeTrack scene; existing work is never overwritten.");
            var ground = Material("PrototypeGround", new Color(0.32f, 0.34f, 0.36f));
            var blocks = Material("PrototypeConcrete", new Color(0.60f, 0.62f, 0.64f));
            var carMat = Material("PrototypeCar", new Color(0.18f, 0.36f, 0.52f));
            var dark = Material("PrototypeRubber", new Color(0.08f, 0.09f, 0.1f));
            var area = new GameObject("Greybox Test Area").transform;
            Cube("Open driving pad 220m x 260m", new(0, -0.5f, 30), new(220, 1, 260), ground, area);
            // Separate islands allow figure-eights and several turns without defining a race circuit.
            Cube("Turn island West", new(-42, 0.6f, 12), new(16, 1.2f, 32), blocks, area);
            Cube("Turn island East", new(42, 0.6f, 35), new(16, 1.2f, 32), blocks, area);
            for (int i = 0; i < 5; i++) Cube("Slalom block " + (i + 1), new(-65 + (i % 2) * 14, 0.5f, -55 + i * 20), new(3, 1, 3), blocks, area);
            Ramp("Gentle Ramp", -18, 40, 12, 14, 1.8f, blocks, area);
            Ramp("Large Ramp", 18, 65, 14, 18, 5, blocks, area);
            // Edge markers give scale without creating a walled course; falling off resets automatically.
            for (int i = 0; i < 9; i++) Cube("Edge marker", new(-108, 0.3f, -85 + i * 28), new(1, 0.6f, 2), blocks, area);

            var spawn = new GameObject("Respawn Pad").transform;
            spawn.position = new Vector3(0, 1.1f, -45);
            Cube("Spawn marker", new(0, 0.015f, -45), new(5, 0.02f, 7), blocks, area).GetComponent<Collider>().enabled = false;
            var car = new GameObject("Prototype Car"); car.layer = 2;
            car.transform.position = spawn.position;
            var body = car.AddComponent<Rigidbody>(); body.mass = 1200; body.linearDamping = 0.02f; body.angularDamping = 0.2f;
            body.interpolation = RigidbodyInterpolation.Interpolate; body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            var collider = car.AddComponent<BoxCollider>(); collider.center = new Vector3(0, 0.05f, 0); collider.size = new Vector3(1.85f, 0.65f, 3.7f);
            var physicsMaterial = new PhysicsMaterial("Prototype body low friction") { dynamicFriction = 0.1f, staticFriction = 0.1f, bounciness = 0, frictionCombine = PhysicsMaterialCombine.Minimum, bounceCombine = PhysicsMaterialCombine.Minimum };
            AssetDatabase.CreateAsset(physicsMaterial, "Assets/Vehicles/PrototypeBody.asset"); collider.sharedMaterial = physicsMaterial;
            Visual("Body", new(0, 0.05f, 0), new(1.85f, 0.65f, 3.7f), carMat, car.transform);
            Visual("Cabin", new(0, 0.6f, -0.3f), new(1.5f, 0.6f, 1.8f), blocks, car.transform);
            Visual("Front direction stripe", new(0, 0.39f, 1.25f), new(1.3f, 0.035f, 0.35f), blocks, car.transform);
            foreach (float x in new[] { -0.95f, 0.95f }) foreach (float z in new[] { -1.3f, 1.3f })
            {
                var wheel = GameObject.CreatePrimitive(PrimitiveType.Cylinder); wheel.name = "Placeholder wheel";
                Object.DestroyImmediate(wheel.GetComponent<Collider>()); wheel.layer = 2; wheel.transform.SetParent(car.transform, false);
                wheel.transform.localPosition = new(x, -0.2f, z); wheel.transform.localRotation = Quaternion.Euler(0, 0, 90); wheel.transform.localScale = new(0.65f, 0.16f, 0.65f);
                wheel.GetComponent<Renderer>().sharedMaterial = dark;
            }
            car.AddComponent<VehicleInput>(); car.AddComponent<ArcadeVehicle>();
            var reset = car.AddComponent<VehicleRespawn>();
            PrefabUtility.SaveAsPrefabAssetAndConnect(car, "Assets/Prefabs/PrototypeCar.prefab", InteractionMode.AutomatedAction);
            reset.spawnPoint = spawn;
            PrefabUtility.RecordPrefabInstancePropertyModifications(reset);
            var cameraObject = new GameObject("Chase Camera"); cameraObject.tag = "MainCamera";
            var camera = cameraObject.AddComponent<Camera>(); camera.fieldOfView = 65; camera.farClipPlane = 500; camera.nearClipPlane = 0.1f;
            cameraObject.AddComponent<AudioListener>();
            var chase = cameraObject.AddComponent<ChaseCamera>(); chase.target = car.transform; chase.Snap();
            var light = new GameObject("Sun").AddComponent<Light>(); light.type = LightType.Directional; light.intensity = 2;
            light.transform.rotation = Quaternion.Euler(50, -35, 0); light.shadows = LightShadows.Soft;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat; RenderSettings.ambientLight = new Color(0.55f, 0.57f, 0.6f);
            EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
        }

        static Material Material(string name, Color color)
        {
            var result = new Material(Shader.Find("Universal Render Pipeline/Lit")); result.name = name;
            result.color = color; result.SetFloat("_Smoothness", 0.15f);
            AssetDatabase.CreateAsset(result, "Assets/Materials/" + name + ".mat"); return result;
        }
        static GameObject Cube(string name, Vector3 position, Vector3 scale, Material material, Transform parent)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube); go.name = name; go.transform.SetParent(parent, false);
            go.transform.position = position; go.transform.localScale = scale; go.GetComponent<Renderer>().sharedMaterial = material; return go;
        }
        static void Visual(string name, Vector3 position, Vector3 scale, Material material, Transform parent)
        {
            var go = Cube(name, Vector3.zero, scale, material, parent); go.transform.localPosition = position; go.layer = 2; Object.DestroyImmediate(go.GetComponent<Collider>());
        }
        static void Ramp(string name, float x, float z, float width, float length, float height, Material material, Transform parent)
        {
            // Solid wedge has a flush takeoff entrance, not the vertical lip of a tilted box.
            var mesh = new Mesh { name = name };
            float w = width / 2;
            mesh.vertices = new[] { new Vector3(-w,0,0), new Vector3(w,0,0), new Vector3(-w,0,length), new Vector3(w,0,length), new Vector3(-w,height,length), new Vector3(w,height,length) };
            mesh.triangles = new[] { 0,4,1, 1,4,5, 0,2,4, 1,5,3, 2,3,4, 3,5,4, 0,1,2, 1,3,2 };
            mesh.RecalculateNormals(); mesh.RecalculateBounds();
            AssetDatabase.CreateAsset(mesh, "Assets/Track/" + name.Replace(" ", "") + ".asset");
            var go = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider));
            go.transform.SetParent(parent, false); go.transform.position = new Vector3(x, 0, z);
            go.GetComponent<MeshFilter>().sharedMesh = mesh; go.GetComponent<MeshCollider>().sharedMesh = mesh; go.GetComponent<Renderer>().sharedMaterial = material;
        }
    }
}
