using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Racer.Editor
{
    // One local authoring operation; no runtime driving or race-rule changes.
    public static class Phase5Setup
    {
        public const string RootName = "Phase 5 - Southwest woodland shortcut";
        public static int EntryIndex, ExitIndex;
        public static List<Vector3> Path()
        {
            var route = StreetLoopBuilder.Route();
            EntryIndex = Closest(route, new Vector3(-560, 0, -552));
            ExitIndex = Closest(route, new Vector3(-620, 0, -450));
            var a = route[EntryIndex]; var d = route[ExitIndex];
            var b = a + (route[EntryIndex + 1] - route[EntryIndex]).normalized * 32;
            var c = d - (route[ExitIndex + 1] - route[ExitIndex]).normalized * 42;
            var path = new List<Vector3>();
            for (int i = 0; i <= 90; i++)
            {
                float t = i / 90f, u = 1 - t;
                path.Add(u*u*u*a + 3*u*u*t*b + 3*u*t*t*c + t*t*t*d);
            }
            return path;
        }
        public static int Closest(List<Vector3> points, Vector3 p)
        {
            int index=0; float best=float.MaxValue;
            for(int i=0;i<points.Count;i++){var q=points[i]-p;q.y=0;if(q.sqrMagnitude<best){best=q.sqrMagnitude;index=i;}}
            return index;
        }
        public static float Distance(Vector3 p,List<Vector3> path,out Vector3 nearest)
        {
            float best=float.MaxValue;nearest=default;
            for(int i=0;i<path.Count-1;i++)
            {
                var a=path[i];var v=path[i+1]-a;v.y=0;var q=p-a;q.y=0;
                float t=Mathf.Clamp01(Vector3.Dot(q,v)/v.sqrMagnitude);
                var n=Vector3.Lerp(a,path[i+1],t);q=p-n;q.y=0;
                if(q.sqrMagnitude<best){best=q.sqrMagnitude;nearest=n;}
            }
            return Mathf.Sqrt(best);
        }
        [MenuItem("Racer/Add Phase 5 Woodland Shortcut")]
        public static void Build()
        {
            var scene=SceneManager.GetActiveScene();
            if(Application.isPlaying||scene.isDirty||scene.path!=StreetLoopBuilder.ScenePath||GameObject.Find(RootName))
                throw new InvalidOperationException("Open saved StreetLoopGreybox outside Play mode; shortcut must not already exist.");
            var path=Path();var route=StreetLoopBuilder.Route();int tiles=0,vertices=0,trees=0,cubes=0;
            var root=new GameObject(RootName).transform;
            foreach(var filter in GameObject.Find("Memory loop - north is +Z").GetComponentsInChildren<MeshFilter>())
            {
                var mesh=filter.sharedMesh;var v=mesh.vertices;var colors=mesh.colors;bool changed=false;
                for(int i=0;i<v.Length;i++)
                {
                    // Bound first: preserve all terrain outside this one corner.
                    if(v[i].x < -640 || v[i].x > -545 || v[i].z < -570 || v[i].z > -435)continue;
                    float distance=Distance(v[i],path,out var nearest);if(distance>=12)continue;
                    float roadDistance=StreetLoopBuilder.Nearest(v[i],route,out _);
                    float weight=(1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(4,12,distance)))*Mathf.SmoothStep(0,1,Mathf.InverseLerp(6,12,roadDistance));
                    v[i].y=Mathf.Lerp(v[i].y,nearest.y,weight);
                    float dirt=(1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(2.6f,3.8f,distance)))*Mathf.SmoothStep(0,1,Mathf.InverseLerp(4,7,roadDistance));
                    colors[i]=Color.Lerp(colors[i],new Color(.49f,.32f,.16f),dirt);
                    if(weight>0||dirt>0){changed=true;vertices++;}
                }
                if(!changed)continue;
                mesh.vertices=v;mesh.colors=colors;mesh.RecalculateNormals();mesh.RecalculateBounds();EditorUtility.SetDirty(mesh);
                filter.GetComponent<MeshCollider>().sharedMesh=null;filter.GetComponent<MeshCollider>().sharedMesh=mesh;tiles++;
            }
            var woods=GameObject.Find("Woods replacing later subdivisions");
            foreach(var box in woods.GetComponentsInChildren<BoxCollider>())
                if(Distance(box.transform.position,path,out _)<9){Object.DestroyImmediate(box.gameObject);trees++;}
            // Existing forest is combined cubes (24 vertices each), so remove only cubes
            // intersecting the driving corridor, including overhanging crowns.
            foreach(var filter in woods.GetComponentsInChildren<MeshFilter>())
            {
                var mesh=filter.sharedMesh;var v=mesh.vertices;var normals=mesh.normals;var uv=mesh.uv;var tri=mesh.triangles;
                if(v.Length%24!=0)throw new InvalidOperationException("Unexpected forest mesh topology");
                var keep=new bool[v.Length/24];bool changed=false;
                for(int block=0;block<keep.Length;block++)
                {
                    var bounds=new Bounds(v[block*24],Vector3.zero);for(int k=1;k<24;k++)bounds.Encapsulate(v[block*24+k]);
                    var center=filter.transform.TransformPoint(bounds.center);
                    keep[block]=Distance(center,path,out _) > Mathf.Max(9,6+Mathf.Max(bounds.extents.x,bounds.extents.z));
                    if(!keep[block]){changed=true;cubes++;}
                }
                if(!changed)continue;
                var nv=new List<Vector3>();var nn=new List<Vector3>();var nu=new List<Vector2>();var nt=new List<int>();var map=new int[v.Length];
                for(int i=0;i<v.Length;i++)if(keep[i/24]){map[i]=nv.Count;nv.Add(v[i]);if(normals.Length==v.Length)nn.Add(normals[i]);if(uv.Length==v.Length)nu.Add(uv[i]);}
                for(int i=0;i<tri.Length;i+=3)if(keep[tri[i]/24]){nt.Add(map[tri[i]]);nt.Add(map[tri[i+1]]);nt.Add(map[tri[i+2]]);}
                mesh.Clear();mesh.SetVertices(nv);mesh.SetTriangles(nt,0);if(nn.Count>0)mesh.SetNormals(nn);if(nu.Count>0)mesh.SetUVs(0,nu);mesh.RecalculateBounds();EditorUtility.SetDirty(mesh);
            }
            Physics.SyncTransforms();
            RefreshGroundNormals();
            var mat=new Material(Shader.Find("Universal Render Pipeline/Lit")){color=new Color(.85f,.65f,.18f)};
            AssetDatabase.CreateAsset(mat,"Assets/Materials/Phase5Markers.mat");
            for(int i=15;i<80;i+=12)
            {
                var forward=(path[i+1]-path[i-1]).normalized;var right=Vector3.Cross(Vector3.up,forward).normalized;
                foreach(int side in new[]{-1,1})Marker(root,"Non-colliding path edge",path[i]+right*side*3.6f,new Vector3(.25f,.7f,.25f),mat);
            }
            var approach=route[Mathf.Max(0,EntryIndex-13)];
            Sign(root,approach+new Vector3(0,0,9),Quaternion.LookRotation(Vector3.left),"WOODLAND CUT  >\nNARROW DIRT PATH\n80 - 95 km/h",mat);
            Sign(root,path[77]+new Vector3(7,0,0),Quaternion.LookRotation(Vector3.forward),"REJOIN ROAD\nSTRAIGHTEN UP",mat);
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
            File.WriteAllText("Docs/PHASE5_BUILD.txt",$"Entry {path[0]} route index {EntryIndex}; rejoin {path[90]} index {ExitIndex}.\nLocal ground: {tiles} tiles, {vertices} vertices affected. Trees: {trees} trunk colliders and {cubes} visible cubes cleared.\nOne 5.2m dirt core, feathered to 7.6m; terrain blend to 12m. No gate, car, road-loop, jump, surface-physics or runtime source changes.\n");
        }
        public static void RefreshGroundNormals()
        {
            // Retain the original heightfield's central-difference shading, including tile
            // boundaries. Triangle-normal recalculation would alter shading outside the cut.
            var mesh=AssetDatabase.LoadAssetAtPath<Mesh>("Assets/Track/StreetLoop/Ground_80_80.asset");
            var vertices=mesh.vertices;var normals=new Vector3[vertices.Length];var heights=new Dictionary<Vector2,float>();
            foreach(var filter in GameObject.Find("Memory loop - north is +Z").GetComponentsInChildren<MeshFilter>())
                foreach(var p in filter.sharedMesh.vertices)
                    if(p.x>=-642&&p.x<=-478&&p.z>=-592&&p.z<=-428)heights[new Vector2(p.x,p.z)]=p.y;
            float Height(Vector3 p)=>heights[new Vector2(p.x,p.z)];
            for(int i=0;i<vertices.Length;i++){var p=vertices[i];normals[i]=new Vector3(Height(p-Vector3.right*2)-Height(p+Vector3.right*2),4,Height(p-Vector3.forward*2)-Height(p+Vector3.forward*2)).normalized;}
            mesh.normals=normals;EditorUtility.SetDirty(mesh);
        }
        static Transform Marker(Transform root,string name,Vector3 point,Vector3 size,Material mat)
        {
            if(!Physics.Raycast(point+Vector3.up*40,Vector3.down,out var hit,80,1))throw new Exception("Marker missing ground");
            var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=name;go.transform.SetParent(root);go.transform.position=hit.point+Vector3.up*size.y*.5f;go.transform.localScale=size;go.GetComponent<Renderer>().sharedMaterial=mat;Object.DestroyImmediate(go.GetComponent<Collider>());return go.transform;
        }
        static void Sign(Transform root,Vector3 point,Quaternion rotation,string words,Material mat)
        {
            var board=Marker(root,"Shortcut direction sign",point,new Vector3(7,3,.15f),mat);board.position+=Vector3.up*1.5f;board.rotation=rotation;
            var text=new GameObject("Shortcut advice").AddComponent<TextMesh>();text.transform.SetParent(root);text.transform.SetPositionAndRotation(board.position-board.forward*.12f,rotation);text.text=words;text.anchor=TextAnchor.MiddleCenter;text.alignment=TextAlignment.Center;text.characterSize=.105f;text.fontSize=64;text.color=Color.black;
        }
    }
}
