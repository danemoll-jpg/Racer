using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Racer.Editor
{
    public static class RaceSetup
    {
        [MenuItem("Racer/Add Phase 3 Race Systems")]
        public static void Build()
        {
            var scene = SceneManager.GetActiveScene();
            if (Application.isPlaying || scene.path != StreetLoopBuilder.ScenePath || scene.isDirty)
                throw new InvalidOperationException("Open the saved StreetLoopGreybox outside Play mode first.");
            if (Object.FindAnyObjectByType<RaceDirector>()) throw new InvalidOperationException("Race already exists; edit its course instead.");
            var car = Object.FindAnyObjectByType<ArcadeVehicle>();
            if (!car || !car.GetComponent<VehicleRespawn>().spawnPoint) throw new InvalidOperationException("Missing vehicle/spawn.");
            var root = new GameObject("Phase 3 - Race Systems");
            var director = root.AddComponent<RaceDirector>(); director.vehicle = car;
            var route = StreetLoopBuilder.Route();
            // First gate is ahead of the accepted fixed spawn; no environment regeneration.
            var indices = new List<int> { 22 }; float distance = 0;
            for (int i = 23; i < route.Count - 70; i++)
            { distance += Vector3.Distance(route[i-1], route[i]); if (distance >= 230) { indices.Add(i); distance = 0; } }
            var gates = new List<RaceGate>();
            var marker = new Material(Shader.Find("Universal Render Pipeline/Unlit")) { color = new Color(.1f,.9f,.85f) };
            var white = new Material(Shader.Find("Universal Render Pipeline/Unlit")) { color = Color.white };
            var black = new Material(Shader.Find("Universal Render Pipeline/Unlit")) { color = new Color(.04f,.04f,.04f) };
            AssetDatabase.CreateAsset(marker, "Assets/Materials/RaceGate.mat");
            AssetDatabase.CreateAsset(white, "Assets/Materials/RaceWhite.mat");
            AssetDatabase.CreateAsset(black, "Assets/Materials/RaceBlack.mat");
            foreach (int index in indices)
            {
                int number = gates.Count;
                var go = new GameObject(number == 0 ? "START FINISH" : $"CP {number:00}"); go.transform.SetParent(root.transform);
                go.transform.position = route[index] + Vector3.up * 1.5f;
                go.transform.rotation = Quaternion.LookRotation(route[index+1] - route[index-1]);
                var gate = go.AddComponent<RaceGate>(); gates.Add(gate);
                foreach (float x in new[] {-6f,6f}) Cube(go.transform, new Vector3(x, .5f, 0), new Vector3(.25f,4,.25f), number == 0 ? white : marker);
                Cube(go.transform, new Vector3(0,2.5f,0), new Vector3(12,.25f,.25f), number == 0 ? white : marker);
                var label = new GameObject("Gate label").AddComponent<TextMesh>(); label.transform.SetParent(go.transform,false);
                label.transform.localPosition = new Vector3(0,3.3f,0); label.transform.localRotation = Quaternion.identity;
                label.text = go.name + "  >"; label.anchor = TextAnchor.MiddleCenter; label.characterSize = .22f; label.fontSize = 48;
                if (number == 0)
                {
                    // Individual squares follow the existing collision surface; no physical obstacle.
                    for (int row=0; row<2; row++) for(int col=0; col<12; col++)
                    {
                        var p=go.transform.TransformPoint(new Vector3(-5.5f+col,0,(row-.5f)*.8f));
                        if (Physics.Raycast(p+Vector3.up*10,Vector3.down,out var hit,25,1))
                        {
                            var square=Cube(go.transform,Vector3.zero,new Vector3(1,.025f,.8f),(row+col)%2==0?white:black);
                            square.position=hit.point+hit.normal*.035f;
                            square.rotation=Quaternion.LookRotation(Vector3.ProjectOnPlane(go.transform.forward,hit.normal),hit.normal);
                        }
                    }
                }
                var arrow = new GameObject("Direction arrow").AddComponent<TextMesh>(); arrow.transform.SetParent(go.transform,false);
                arrow.transform.localPosition=new Vector3(0,-1.4f,5); arrow.transform.localRotation=Quaternion.Euler(90,0,0);
                arrow.text="^"; arrow.anchor=TextAnchor.MiddleCenter; arrow.characterSize=1; arrow.fontSize=64; arrow.color=Color.white;
            }
            director.gates = gates.ToArray();
            var canvas = new GameObject("Race HUD", typeof(Canvas), typeof(UnityEngine.UI.CanvasScaler)); canvas.transform.SetParent(root.transform);
            canvas.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;
            var scaler=canvas.GetComponent<UnityEngine.UI.CanvasScaler>(); scaler.uiScaleMode=UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution=new Vector2(1280,720); scaler.matchWidthOrHeight=.5f;
            var panel=new GameObject("Status panel",typeof(RectTransform),typeof(UnityEngine.UI.Image)); panel.transform.SetParent(canvas.transform,false);
            var rect=panel.GetComponent<RectTransform>();rect.anchorMin=rect.anchorMax=rect.pivot=new Vector2(0,1);rect.anchoredPosition=new Vector2(16,-16);rect.sizeDelta=new Vector2(610,170);
            panel.GetComponent<UnityEngine.UI.Image>().color=new Color(.025f,.04f,.055f,.88f); panel.GetComponent<UnityEngine.UI.Image>().raycastTarget=false;
            var text=new GameObject("Race status",typeof(RectTransform),typeof(UnityEngine.UI.Text));text.transform.SetParent(panel.transform,false);
            rect=text.GetComponent<RectTransform>();rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=new Vector2(12,8);rect.offsetMax=new Vector2(-12,-8);
            var display=text.GetComponent<UnityEngine.UI.Text>();display.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");display.fontSize=18;display.color=Color.white;display.raycastTarget=false;
            display.text="LAP 1 / 3\nRace 00:00.000\nCross START in the arrow direction\nEnter / Start: restart race    R / Y: reset car";
            var hud=canvas.AddComponent<RaceHud>();hud.race=director;hud.display=display;
            car.slowSteerAngle=33; car.steeringResponse=8;
            PrefabUtility.RecordPrefabInstancePropertyModifications(car);
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
            Debug.Log($"Phase 3 saved with {gates.Count-1} ordered checkpoints and three laps. Environment unchanged.");
        }
        static Transform Cube(Transform parent,Vector3 position,Vector3 scale,Material material)
        {
            var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name="Non-colliding gate marking";g.transform.SetParent(parent,false);
            g.transform.localPosition=position;g.transform.localScale=scale;g.GetComponent<Renderer>().sharedMaterial=material;
            Object.DestroyImmediate(g.GetComponent<Collider>());return g.transform;
        }
    }
}
